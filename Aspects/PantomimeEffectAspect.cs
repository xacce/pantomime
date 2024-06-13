using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using xdd.Pantomimess;

namespace Pantomime.Aspects
{
    [BurstCompile]
    public readonly partial struct PantomimeEffectAspect : IAspect
    {
        private readonly RefRO<PantomimeEffects> _data;
        private readonly OptimizedSkeletonAspect _skeletonAspect;

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
        public void ApplyEffects(in float3 toPosition, in float3 myPosition, float3 myFwdDirection)
        {
            ref var effects = ref _data.ValueRO.effectsSets.Value.effects;
            if (effects.Length == 0) return;
            var targetPosition = toPosition;
            var distance = math.distancesq(targetPosition, myPosition);
            var bones = _skeletonAspect.bones;

            for (int i = 0; i < effects.Length; i++)
            {
                ref var set = ref effects[i];
                if (distance < set.minMaxDistanceSq.x || distance > set.minMaxDistanceSq.y) continue;
                switch (set.type)
                {
                    case PantomimeEffects.Type.LocalDirectionWeightedChain:
                        for (int j = 0; j < set.rules.Length; j++)
                        {
                            ref var rule = ref set.rules[j];
                            var rawBone = bones[rule.boneIndex];
                            var boneFwdAxis = math.rotate(rawBone.worldRotation, math.forward());
                            var targetAxis = targetPosition - rawBone.worldPosition;
                            var angle = Angle(boneFwdAxis, math.normalize(targetPosition - rawBone.worldPosition));
                            if (angle > rule.angleLimit)
                            {
                                targetAxis = rawBone.worldPosition + math.lerp(targetAxis, boneFwdAxis, (angle - rule.angleLimit) / rule.angleTolerance) - rawBone.worldPosition;
                            }

                            var towards = math.nlerp(quaternion.identity, FromToRotation(boneFwdAxis, targetAxis), rule.weight);
                            rawBone.worldRotation = math.mul(math.mul(towards, rawBone.worldRotation), rule.fixedRotationOffset);
                            Debug.DrawRay(rawBone.worldPosition, math.rotate(rawBone.worldRotation, math.forward()) * 2f, Color.cyan);
                        }
                        break;
                    case PantomimeEffects.Type.GlobalDirectionWeightedChain:
                        for (int j = 0; j < set.rules.Length; j++)
                        {
                            ref var rule = ref set.rules[j];
                            var rawBone = bones[rule.boneIndex];
                            var targetAxis = targetPosition - myPosition;
                            var angle = Angle(myFwdDirection, math.normalize(targetPosition - myPosition));
                            if (angle > rule.angleLimit)
                            {
                                targetAxis = rawBone.worldPosition + math.lerp(targetAxis, myFwdDirection, (angle - rule.angleLimit) / rule.angleTolerance) - rawBone.worldPosition;
                            }

                            var towards = math.nlerp(quaternion.identity, FromToRotation(myFwdDirection, targetAxis), rule.weight);
                            rawBone.worldRotation = math.mul(towards, rawBone.worldRotation);
                            Debug.DrawRay(rawBone.worldPosition, math.rotate(rawBone.worldRotation, math.forward()) * 2f, Color.cyan);
                        }
                        break;
                }
            }
        }
    }
}