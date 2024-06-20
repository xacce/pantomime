using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pantomime.Aspects
{
    [BurstCompile]
    public readonly partial struct PantomimeEffectAspect : IAspect
    {
        readonly RefRO<PantomimeEffects> _data;
        readonly OptimizedSkeletonAspect _skeletonAspect;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(in float3 from, in float3 to)
        {
            float num = (float)math.sqrt((double)math.lengthsq(from) * (double)math.lengthsq(to));
            return (double)num < 1.0000000036274937E-15 ? 0.0f : math.acos(math.clamp(math.dot(math.normalize(from), math.normalize(to)), -1, 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion FromToRotation(float3 from, float3 to)
        {
            return quaternion.AxisAngle(
                angle: math.acos(math.clamp(math.dot(math.normalize(from), math.normalize((to))), -1f, 1f)),
                axis: math.normalize(math.cross(from, to))
            );
        }

        [BurstCompile]
        public void ApplyEffects(in float3 target, in float3 position, in float3 forward, float deltaTime)
        {
            ref var effects = ref _data.ValueRO.effectsSets.Value.effects;
            if (effects.Length == 0) return;
            var targetPosition = target;
            var distance = math.distancesq(targetPosition, position);
            var bones = _skeletonAspect.bones;

            for (int i = 0; i < effects.Length; i++)
            {
                ref var set = ref effects[i];
                if (distance < set.minMaxDistanceSq.x || distance > set.minMaxDistanceSq.y) continue;
                var childBone = bones[set.targetBone];
                var _childFwd = math.normalizesafe(math.rotate(childBone.worldRotation, set.fwdAxis));
                var _dirToTargetFromChild = math.normalizesafe(targetPosition - childBone.worldPosition);
                var angle = Angle(_childFwd, _dirToTargetFromChild);
                if (angle > set.angleLimit)
                {
                    targetPosition = childBone.worldPosition + math.lerp(_dirToTargetFromChild, _childFwd, math.min((angle - set.angleLimit) / math.radians(set.angleTolerance), 1f));

                }
                for (int j = 0; j < set.rules.Length; j++)
                {
                    ref var rule = ref set.rules[j];

                    // var childBone = bones[rule.targetBone];
                    for (int ii = 0; ii < set.iterations; ii++)
                    {
                        //xdd
                        switch (set.type)
                        {
                            case PantomimeEffects.Type.BruteForceIk:
                                // var childBone = bones[set.targetBone];
                                var parentBone = bones[rule.rotatingBone];
                                var childFwd = math.normalizesafe(math.rotate(childBone.worldRotation, set.fwdAxis));
                                var dirToTargetFromChild = math.normalizesafe(targetPosition - childBone.worldPosition);
                                var rot = math.mul(math.nlerp(quaternion.identity, FromToRotation(childFwd, dirToTargetFromChild), rule.weight), parentBone.worldRotation);
                                parentBone.worldRotation = rot;
                                break;
                        }
                    }
                }
            }
        }
    }
}