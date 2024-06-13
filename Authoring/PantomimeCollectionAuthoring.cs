using System;
using System.Collections.Generic;
using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Pantomime.Authoring.So;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace xdd.Pantomimess.Authoring
{
#if UNITY_EDITOR
    public class PantomimeCollectionAuthoring : MonoBehaviour
    {
        [Serializable]
        public class _Layer
        {
            public float baseWeight;
            public string guid;
            public bool overrideMode;
            public _Motion[] motions;
            public PantomimeAvatarMask mask;
        }

        [Serializable]
        public class _Motion
        {
            [Serializable]
            public class _Clip
            {
                public string guid;
                public AnimationClip clip;
                public float2 blendPosition;
            }

            public string guid;
            public int2 dynamicVariables = new int2(-1, -1);
            public PantomimeCollection.BlendMode blendMode;
            public _Clip[] clips;
            public int trigger;
            public ulong boolean;
            public bool isDefault;
            public bool loop;
        }
        [SerializeField] public PantomimeParamsUnified params_s;
        
        [SerializeField] public _Layer[] layers_s = Array.Empty<_Layer>();
        [Serializable]
        public struct GraphPosition
        {
            public Rect rect;
            public string guid;
        }
        [SerializeField] public GraphPosition[] _positions = Array.Empty<GraphPosition>();

        [TemporaryBakingType]
        struct PantomimeCollectionAuthoringSmartBaker : ISmartBakeItem<PantomimeCollectionAuthoring>
        {
            private SmartBlobberHandle<SkeletonClipSetBlob> _clipsBlob;
            private BlobAssetReference<PantomimeCollection.PantomimeBlobData> _motionsBlob;

            public bool Bake(PantomimeCollectionAuthoring authoring, IBaker baker)
            {
                var e = baker.GetEntity(TransformUsageFlags.Dynamic);
                HashSet<AnimationClip> hashSetClips = new HashSet<AnimationClip>();
                baker.AddBuffer<PantomimeRuntimeLayerElement>(e);
                baker.AddBuffer<PantomimeDynamicValue>(e);
                int zq = 0;
                int dynoMax = 0;
                foreach (var layer in authoring.layers_s)
                {
                    baker.AppendToBuffer(
                        e,
                        new PantomimeRuntimeLayerElement()
                        {
                            currentDuration = 0f,
                            currenMotion = layer.motions.ToList().FindIndex(motion => motion.isDefault),
                        });
                    foreach (var motion in layer.motions)
                    {
                        foreach (var clip in motion.clips)
                        {
                            hashSetClips.Add(clip.clip);
                        }
                        dynoMax = math.max(dynoMax, motion.dynamicVariables.x);
                        dynoMax = math.max(dynoMax, motion.dynamicVariables.y);
                    }
                    zq++;
                }
                for (int i = 0; i < dynoMax + 1; i++)
                {
                    baker.AppendToBuffer(e, new PantomimeDynamicValue());
                }
                var clipsArr = hashSetClips.ToList();
                var clips = new NativeArray<SkeletonClipConfig>(clipsArr.Count, Allocator.Temp);
                for (int i = 0; i < hashSetClips.Count; i++)
                {
                    clips[i] = new SkeletonClipConfig { clip = clipsArr[i], settings = SkeletonClipCompressionSettings.kDefaultSettings };
                }
                _clipsBlob = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var pantomimeBlob = ref builder.ConstructRoot<PantomimeCollection.PantomimeBlobData>();
                var layers = builder.Allocate(ref pantomimeBlob.layersBlob, authoring.layers_s.Length);
                for (int i = 0; i < authoring.layers_s.Length; i++)
                {
                    var layer = authoring.layers_s[i];
                    layers[i].overrideMode = layer.overrideMode;

                    layers[i].hasMask = false;
                    layers[i].baseWeight = layer.baseWeight;
                    if (layer.mask)
                    {
                        var masks = builder.Allocate(ref layers[i].mask, layer.mask.mask.Length);
                        for (int j = 0; j < layer.mask.mask.Length; j++)
                        {
                            masks[j] = layer.mask.mask[j];
                        }
                        layers[i].hasMask = true;
                    }


                    var motions = builder.Allocate(ref layers[i].motions, layer.motions.Length);
                    for (int z = 0; z < layer.motions.Length; z++)
                    {
                        var motion = layer.motions[z];
                        var motionClips = builder.Allocate(ref motions[z].clipIndexes, motion.clips.Length);
                        var clipPositions = builder.Allocate(ref motions[z].clipPositions, motion.clips.Length);
                        motions[z].trigger = (int)motion.trigger;
                        motions[z].tags = (ulong)motion.boolean;
                        motions[z].loop = motion.loop;
                        motions[z].isDefault = motion.isDefault;
                        motions[z].blendMode = motion.blendMode;
                        motions[z].dynamicVariables = motion.dynamicVariables;
                        for (int j = 0; j < motion.clips.Length; j++)
                        {
                            clipPositions[j] = motion.clips[j].blendPosition;
                            motionClips[j] = clipsArr.IndexOf(motion.clips[j].clip);
                            motions[z].duration = math.max(motions[z].duration, motion.clips[j].clip.length);
                        }
                    }


                }
                _motionsBlob = builder.CreateBlobAssetReference<PantomimeCollection.PantomimeBlobData>(Allocator.Persistent);
                builder.Dispose();
                baker.AddBlobAsset(ref _motionsBlob, out _);

                baker.AddComponent<PantomimeCollection>(e);
                baker.AddBuffer<PantomimeTriggerElement>(e);
                baker.AddComponent<PantomimeTags>(e);
                baker.AddComponent<PantomimeRuntime>(e);
                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
            {
                entityManager.SetComponentData(entity, new PantomimeCollection() { clipsBlob = _clipsBlob.Resolve(entityManager), blobData = _motionsBlob });
            }
        }

        class PantomimeCollectionAuthoringBaker : SmartBaker<PantomimeCollectionAuthoring, PantomimeCollectionAuthoring.PantomimeCollectionAuthoringSmartBaker>
        {
        }

    }

#endif

}