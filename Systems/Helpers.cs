using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Pantomime.Systems
{
    [BurstCompile]
    public static class Helpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float angle(float2 from, float2 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = math.sqrt(math.lengthsq(from) * math.lengthsq(to));

            if (denominator < 1E-15F)
                return 0F;

            float dot = math.clamp(math.dot(from, to) / denominator, -1F, 1F);
            return math.radians(math.acos(dot));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float angleSigned(in float2 from, in float2 to)
        {
            float unsignedAngle = angle(from, to);
            float sign = math.sign(from.x * to.y - from.y * to.x);
            return unsignedAngle * sign;
        }
       
        [BurstCompile]
        public static float BuildWeights(ref NativeHashMap<int2, float> map, ref DynamicBuffer<PantomimeDynamicValue> values,
            in BlobAssetReference<PantomimeCollection.PantomimeBlobData> blob, int li, int mi)
        {
            float ttl = 0;
            ref var motion = ref blob.Value.layersBlob[li].motions[mi];
            switch (motion.blendMode)
            {
                case PantomimeCollection.BlendMode.Directional1d:
                    float num = values[motion.dynamicVariables.x].value;
                    float sm = 0;
                    float best = float.MaxValue;
                    int goodIndex = 0;

                    for (int i = 0; i < motion.clipIndexes.Length; i++)
                    {
                        var motionValue = motion.clipPositions[i].x;
                        if (motionValue <= num)
                        {
                            goodIndex = i;
                        }
                        i++;
                    }

                    float origin = goodIndex > 0
                        ? math.abs(motion.clipPositions[goodIndex].x - motion.clipPositions[goodIndex - 1].x)
                        : math.abs(motion.clipPositions[goodIndex].x - motion.clipPositions[goodIndex + 1].x);
                    for (int i = 0; i < motion.clipIndexes.Length; i++)
                    {
                        float weight = math.max(1 - math.abs(motion.clipPositions[i].x - num) / origin, 0f);
                        map[new int2(li, i)] = weight;
                    }
                    break;
                case PantomimeCollection.BlendMode.FreeformCartesian2d:
                    for (int i = 0; i < motion.clipIndexes.Length; i++)
                    {
                        var motionPosition = motion.clipPositions[i];
                        var minWeight = float.PositiveInfinity;

                        float2 parametricVector = new float2(values[motion.dynamicVariables.x].value, values[motion.dynamicVariables.y].value) - motionPosition;
                        for (int j = 0; j < motion.clipIndexes.Length; j++)
                        {
                            if (i == j) continue;
                            var referenceMotionPosition = motion.clipPositions[j];
                            float2 referenceVector = referenceMotionPosition - motionPosition;
                            float weight = math.max(1f - math.dot(parametricVector, referenceVector) / math.lengthsq(referenceVector), 0);
                            if (weight < minWeight)
                            {
                                minWeight = weight;
                            }

                        }
                        map[new int2(li, i)] = minWeight;
                        ttl += minWeight;
                    }
                    break;
                case PantomimeCollection.BlendMode.FreeformDirectional2d:
                    var blendParameter = new float2(values[motion.dynamicVariables.x].value, values[motion.dynamicVariables.y].value);
                    for (int i = 0; i < motion.clipIndexes.Length; i++)
                    {
                        var motionPosition = motion.clipPositions[i];
                        var minWeight = float.PositiveInfinity;

                        for (int j = 0; j < motion.clipIndexes.Length; j++)
                        {
                            if (i == j) continue;
                            var referenceMotionPosition = motion.clipPositions[j];

                            float2 parametricVector =
                                new float2(
                                    (math.length(blendParameter) - math.length(motionPosition)) /
                                    ((math.length(referenceMotionPosition) + math.length(motionPosition)) / 2f),
                                    angleSigned(blendParameter, motionPosition));
                            float2 referenceVector =
                                new float2(
                                    (math.length(referenceMotionPosition) - math.length(motionPosition)) /
                                    ((math.length(referenceMotionPosition) + math.length(motionPosition)) / 2f),
                                    angleSigned(blendParameter, motionPosition));
                            float dot = math.dot(parametricVector, referenceVector);
                            float weight = math.abs(1f - dot);
                            if (weight < minWeight)
                            {
                                minWeight = weight;
                            }
                        }
                        map[new int2(li, i)] = minWeight;
                        ttl += minWeight;
                    }
                    break;
            }
            return ttl;
        }
    }
}