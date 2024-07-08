using Latios;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pantomime.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(Unity.Transforms.TransformSystemGroup))]
    public partial struct PantomimicSystem : ISystem
    {
        private float _previousDeltaTime;

        public void OnCreate(ref SystemState state)
        {
            _previousDeltaTime = 8f * math.EPSILON;
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new PantomimicFullStackJob
            {
                et = (float)SystemAPI.Time.ElapsedTime,
                deltaTime = SystemAPI.Time.DeltaTime,
                previousDeltaTime = _previousDeltaTime,
            }.ScheduleParallel();
            _previousDeltaTime = SystemAPI.Time.DeltaTime;
        }

        [BurstCompile]
        internal partial struct PantomimicFullStackJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public float et;
            public float deltaTime;
            public float previousDeltaTime;

            [NativeDisableContainerSafetyRestriction]
            NativeHashMap<int2, float> _clipWeight;

            [BurstCompile]
            public void Execute(OptimizedSkeletonAspect skeleton, DynamicBuffer<PantomimeRuntimeLayerElement> runtimeLayers, ref PantomimeRuntime runtime,
                DynamicBuffer<PantomimeTriggerElement> triggers,
                DynamicBuffer<PantomimeDynamicValue> dynamicValues,
                in PantomimeFlags flags,
                in PantomimeCollection collection)
            {
                _clipWeight.Clear();
                bool hasTriggers = !triggers.IsEmpty;
                bool hasLazyTrigger = false; //if in previous tick we have some animations completed, we need start blend AFTER new animations was sampled.
                // var triggersRaw = triggers.Reinterpret<int>().AsNativeArray();
                ref var clips = ref collection.clipsBlob.Value.clips;
                ref var layers = ref collection.blobData.Value.layersBlob;

                # region Sample layers

                bool hasBlendings = false;
                for (int l = 0; l < layers.Length; l++)
                {
                    ref var layer = ref layers[l];
                    ref var runtimeLayer = ref runtimeLayers.ElementAt(l);

                    #region Handle triggers

                    bool done = false;
                    for (int m = 0; m < layer.motions.Length; m++)
                    {
                        if (done) break;
                        ref var candidate = ref layers[l].motions[m];

                        if (
                            hasTriggers &&
                            (candidate.allowReentering || runtimeLayer.currenMotion != m) && (flags.flags & candidate.flags) == candidate.flags) //todo maybe trigger is index?
                        {
                            // Debug.Log($"Layer: {l}, flags candidate: {candidate.flags}, flags: {flags}, motion index: {m}");

                            for (int i = 0; i < triggers.Length; i++)
                            {
                                if (triggers[i].type == candidate.trigger)
                                {
                                    if (triggers[i].fixedDuration > 0)
                                    {
                                        runtimeLayer.timeMultiplier = candidate.duration / triggers[i].fixedDuration;
                                    }
                                    else
                                    {
                                        runtimeLayer.timeMultiplier = 0f;
                                    }
                                    runtimeLayer.currentDuration = 0f;
                                    runtimeLayer.currenMotion = m;
                                    runtimeLayer.Transit(m);
                                    // runtime.blendState = PantomimeRuntime.BlendState.Start;
                                    hasBlendings = true;
                                    done = true;
                                    break;
                                }
                            }
                        }
                    }

                    #endregion

                    if (runtimeLayer.currenMotion == -1) continue;
                    ref var motion = ref layer.motions[runtimeLayer.currenMotion];

                    #region Sample motions

                    runtimeLayer.currentDuration += runtimeLayer.timeMultiplier == 0 ? deltaTime : deltaTime * runtimeLayer.timeMultiplier;
                    var layerTotalWeight = motion.blendMode != PantomimeCollection.BlendMode.Nothing
                        ? Helpers.BuildWeights(ref _clipWeight, ref dynamicValues, collection.blobData, l, runtimeLayer.currenMotion)
                        : 1f;

                    for (int i = 0; i < motion.clipIndexes.Length; i++)
                    {
                        var clipIndex = motion.clipIndexes[i];
                        var time = motion.loop ? clips[clipIndex].LoopToClipTime(runtimeLayer.currentDuration) : math.min(runtimeLayer.currentDuration, motion.duration);
                        if (layers[l].overrideMode) skeleton.nextSampleWillOverwrite = true;

                        switch (motion.blendMode)
                        {
                            case PantomimeCollection.BlendMode.Nothing:
                                if (layers[l].hasMask)
                                    clips[clipIndex].SamplePose(ref skeleton, layer.mask.AsSpan(), time, layer.baseWeight);
                                else
                                    clips[clipIndex].SamplePose(ref skeleton, time, layer.baseWeight);
                                break;
                            case PantomimeCollection.BlendMode.FreeformCartesian2d or PantomimeCollection.BlendMode.FreeformDirectional2d:
                                var weight = _clipWeight[new int2(l, i)] / layerTotalWeight;
                                if (weight > 0.01)
                                {
                                    if (layer.hasMask)
                                        clips[clipIndex].SamplePose(ref skeleton, layer.mask.AsSpan(), time, weight);
                                    else
                                        clips[clipIndex].SamplePose(ref skeleton, time, weight);
                                }
                                break;
                            case PantomimeCollection.BlendMode.Directional1d:
                                var lw = _clipWeight[new int2(l, i)];
                                if (lw > 0.01)
                                {
                                    if (layer.hasMask)
                                        clips[clipIndex].SamplePose(ref skeleton, layer.mask.AsSpan(), time, lw);
                                    else
                                        clips[clipIndex].SamplePose(ref skeleton, time, lw);
                                }
                                break;
                        }
                    }

                    #endregion

                    #region Auto exit

                    if (!motion.loop && runtimeLayer.currentDuration >= motion.duration && !motion.disableAutoExit)
                    {
                        runtimeLayer.currenMotion = -1;
                        hasLazyTrigger = true;
                        // runtime.needLazyBlend = true;
                    }

                    #endregion

                }

                #endregion

                triggers.Clear();
                if (hasLazyTrigger) triggers.Add(new PantomimeTriggerElement() { type = 0 });

                #region Inertia blend

                if (hasBlendings)
                {
                    runtime.blendState = PantomimeRuntime.BlendState.Blend;
                    runtime.blendDuration = 0.15f + deltaTime;
                    runtime.currentBlendDuration = 0f;
                    // skeleton.SyncHistory();
                    skeleton.StartNewInertialBlend(previousDeltaTime, runtime.blendDuration);
                    //TODO RESOLVE THIS
                    skeleton.InertialBlend(0); //looks like is not okay... but idk why latios setup pose without blending in frame with blend was started
                }
                else if (runtime.blendState == PantomimeRuntime.BlendState.Blend)
                {
                    // case PantomimeRuntime.BlendState.Start:
                    //     runtime.blendState = PantomimeRuntime.BlendState.Blend;
                    //     runtime.blendDuration = 0.15f + deltaTime;
                    //     runtime.currentBlendDuration = 0f;
                    //     // skeleton.SyncHistory();
                    //     skeleton.StartNewInertialBlend(previousDeltaTime, runtime.blendDuration);
                    //     //TODO RESOLVE THIS
                    //     skeleton.InertialBlend(0); //looks like is not okay... but idk why latios setup pose without blending in frame with blend was started
                    //     break;
                    runtime.currentBlendDuration += deltaTime;
                    if (runtime.currentBlendDuration > runtime.blendDuration)
                    {
                        runtime.blendState = PantomimeRuntime.BlendState.Nothing;
                    }
                    else
                    {
                        skeleton.InertialBlend(runtime.currentBlendDuration);
                    }
                }

                #endregion

                skeleton.EndSamplingAndSync();

            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                if (!_clipWeight.IsCreated)
                {
                    _clipWeight = new NativeHashMap<int2, float>(100, Allocator.Temp);//todo we need 100?
                }
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
            {
            }
        }
    }


}