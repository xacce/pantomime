﻿using Latios.Kinemation;
using Unity.Entities;
using Unity.Mathematics;

namespace xdd.Pantomimess
{
    public partial struct PantomimeEffects : IComponentData
    {
        public partial struct Rule
        {
            public int boneIndex;

            public float weight;

            //after this value (radians) we apply tolerance to difference
            public float angleLimit;

            //0.1-360 - lesser = less sharp curves, 
            public float angleTolerance;
            public quaternion fixedRotationOffset; // for aim animation contains rotation for spines
        }

        public enum Type
        {
            LocalDirectionWeightedChain,
            GlobalDirectionWeightedChain,
        }

        public partial struct Effect
        {
            public Type type;
            public BlobArray<Rule> rules;
            public float2 minMaxDistanceSq { get; set; }
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
                public ulong tags;
                public bool loop;
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
        public void Reset()
        {
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
    }

    public partial struct PantomimeTags : IComponentData
    {
        public ulong tags;
    }
}