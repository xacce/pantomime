using System;
using Codice.Client.BaseCommands;
using Latios.Kinemation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Pantomime
{
    public partial struct PantomimeEffects : IComponentData
    {
        [Serializable]
        public partial struct Rule
        {
            [FormerlySerializedAs("parentBoneIndex")]
            public int rotatingBone;
            public float weight;

            //after this value (radians) we apply tolerance to difference
            //0.1-360 - lesser = less sharp curves, 
            // public float angleTolerance;
            // public quaternion fixedRotationOffset; // for aim animation contains rotation for spines

        }

        public enum Type
        {
            BruteForceIk,
        }

        public partial struct Effect
        {
            public Type type;
            public BlobArray<Rule> rules;
            public float2 minMaxDistanceSq { get; set; }
            public float angleTolerance { get; set; }

            public float3 fwdAxis;

            public int targetBone;

            public float angleLimit;

            public float iterations;
            public float smooth_s;
        }

        public partial struct EffectsSet
        {
            public BlobArray<Effect> effects;
        }

        public BlobAssetReference<EffectsSet> effectsSets;
    }

    [InternalBufferCapacity(0)]
    public partial struct PantomimeDynamicValue : IBufferElementData
    {
        public float value;
    }


    public partial struct PantomimeCollection : IComponentData
    {
        public enum BlendMode
        {
            Nothing,
            FreeformCartesian2d,
            FreeformDirectional2d,
            Directional1d,
        }

        public struct PantomimeBlobData
        {
            public struct PantomimeLayerBlobData
            {
                public bool hasMask;
                public BlobArray<PantomimeMotionBlobData> motions;
                public BlobArray<ulong> mask;
                public bool overrideMode;
                public bool isDefault;
                public float baseWeight;
            }

            public struct PantomimeMotionBlobData
            {
                public BlobArray<int> clipIndexes;
                public BlobArray<float2> clipPositions;
                public int2 dynamicVariables;
                public BlendMode blendMode;
                public int trigger;
                public ulong flags;
                public bool loop;
                public bool disableAutoExit;
                public bool allowReentering;
                public bool isDefault;
                public float duration;
            }

            public BlobArray<PantomimeLayerBlobData> layersBlob;
        }

        public BlobAssetReference<PantomimeBlobData> blobData;
        public BlobAssetReference<SkeletonClipSetBlob> clipsBlob;
    }

    public partial struct PantomimeRuntime : IComponentData
    {
        public enum BlendState
        {
            Nothing,
            Start,
            Blend
        }

        public BlendState blendState;
        public float blendDuration;
        public float currentBlendDuration;
    }

    [InternalBufferCapacity(0)]
    public partial struct PantomimeRuntimeLayerElement : IBufferElementData
    {
        public int currenMotion;
        public float currentDuration;
        public float timeMultiplier;


        public void Reset()
        {
            timeMultiplier = 0;
            currenMotion = -1;
            currentDuration = 0;
        }
        public void Transit(int index)
        {
            currenMotion = index;
            currentDuration = 0;

        }
    }

    [InternalBufferCapacity(0)]
    public partial struct PantomimeTriggerElement : IBufferElementData
    {
        public int type;
        public float fixedDuration;

        public static void Trigger(ref EntityCommandBuffer ecb, in Entity entity, int type, float duration = 0)
        {
            ecb.AppendToBuffer(
                entity,
                new PantomimeTriggerElement()
                {
                    type = type,
                    fixedDuration = duration,
                });
        }
    }

    public partial struct PantomimeFlags : IComponentData
    {
        public ulong flags;

        public void Set(int flag)
        {
            flags |= (uint)flag;
        }

        public void UnSet(int flag)
        {
            flags &= ~(uint)flag;
        }
    }
}