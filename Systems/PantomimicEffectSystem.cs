// using System;
// using System.Runtime.CompilerServices;
// using Latios.Kinemation;
// using Latios.Kinemation.Systems;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;
// using xdd.Pantomimess;
// using xdd.Pantomimess.Authoring;
//
// namespace Pantomime.Systems
// {
//     [UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
//     [UpdateBefore(typeof(CopyTransformFromBoneSystem))]
//     [BurstCompile]
//     public partial struct PantomimicEffectSystem : ISystem
//     {
//         [BurstCompile]
//         public void OnUpdate(ref SystemState state)
//         {
//             new PantomimeEffectJob
//             {
//                 deltaTime = SystemAPI.Time.DeltaTime,
//             }.ScheduleParallel();
//         }
//     }
//
//     [BurstCompile]
//     [WithAll(typeof(Simulate))]
//     internal partial struct PantomimeEffectJob : IJobEntity
//     {
//         public float deltaTime;
//
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static float Angle(in float3 from, in float3 to)
//         {
//             float num = (float)math.sqrt((double)math.lengthsq(from) * (double)math.lengthsq(to));
//             return (double)num < 1.0000000036274937E-15 ? 0.0f : math.acos(math.clamp(math.dot(math.normalize(from), math.normalize(to)), -1, 1));
//         }
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static quaternion FromToRotation(float3 from, float3 to)
//         {
//             return quaternion.AxisAngle(
//                 angle: math.acos(math.clamp(math.dot(math.normalize(from), math.normalize((to))), -1f, 1f)),
//                 axis: math.normalize(math.cross(from, to))
//             );
//         }
//         [BurstCompile]
//         private void Execute([EntityIndexInQuery] int index, in PantomimeEffects data, in PantomimeEffectsTarget target, OptimizedSkeletonAspect skeleton, in LocalToWorld pos)
//         {
//             ref var effects = ref data.effectsSets.Value.effects;
//             if (effects.Length == 0) return;
//             var targetPosition = target.target;
//             var distance = math.distancesq(targetPosition, pos.Position);
//             var bones = skeleton.bones;
//
//             for (int i = 0; i < effects.Length; i++)
//             {
//                 ref var set = ref effects[i];
//                 if (distance < set.minMaxDistanceSq.x || distance > set.minMaxDistanceSq.y) continue;
//                 switch (set.type)
//                 {
//                     case PantomimeEffects.Type.WeightedChain:
//                         for (int j = 0; j < set.rules.Length; j++)
//                         {
//                             ref var rule = ref set.rules[j];
//                             var rawBone = bones[rule.boneIndex];
//                             var boneFwdAxis = math.rotate(rawBone.worldRotation, math.forward());
//                             var targetAxis = targetPosition - rawBone.worldPosition;
//                             var angle = Angle(boneFwdAxis, math.normalize(targetPosition - boneFwdAxis));
//                             if (angle > rule.angleLimit)
//                             {
//                                 targetAxis = rawBone.worldPosition + math.lerp(targetAxis, boneFwdAxis, (angle - rule.angleLimit) / rule.angleTolerance) - rawBone.worldPosition;
//                             }
//
//                             var towards = math.nlerp(quaternion.identity, FromToRotation(boneFwdAxis, targetAxis), rule.weight);
//                             rawBone.worldRotation = math.mul(towards, rawBone.worldRotation);
//                             Debug.DrawRay(rawBone.worldPosition, math.rotate(rawBone.worldRotation, math.forward()) * 2f, Color.cyan);
//                         }
//                         break;
//                 }
//             }
//         }
//     }
//
// }