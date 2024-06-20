using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pantomime
{
    public class PantomimeEffectsAuthoring : MonoBehaviour
    {
        [Serializable]
        class _Rule
        {
            public int priority;
            public float weight;
            public int parentBoneIndex;
        }

        [Serializable]
        class _Effect
        {
            [FormerlySerializedAs("smooth")] public float iterations = 3f;
            public float smooth_s = 3f;
            public float2 minMaxDistance;
            public PantomimeEffects.Type type;
            public _Rule[] rules = Array.Empty<_Rule>();
            public float angleLimit;
            public int targetBone;
            public float3 childFwdAxis;
            public float angleTolerance;
        }

        [SerializeField] private _Effect[] _effects = Array.Empty<_Effect>();

        class B : Baker<PantomimeEffectsAuthoring>
        {
            public override void Bake(PantomimeEffectsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PantomimeEffects>(entity);
                // AddComponent<PantomimeEffectsTarget>(entity);
                var builder = new BlobBuilder(Allocator.Temp);
                ref var pantomimeBlob = ref builder.ConstructRoot<PantomimeEffects.EffectsSet>();
                var effects = builder.Allocate(ref pantomimeBlob.effects, authoring._effects.Length);
                for (int i = 0; i < authoring._effects.Length; i++)
                {
                    var effect = authoring._effects[i];
                    effects[i].type = effect.type;
                    effects[i].angleLimit = math.radians(effect.angleLimit);
                    effects[i].iterations = effect.iterations;
                    effects[i].smooth_s = effect.smooth_s;
                    effects[i].minMaxDistanceSq = new float2(effect.minMaxDistance.x * effect.minMaxDistance.x, effect.minMaxDistance.y * effect.minMaxDistance.y);
                    effects[i].targetBone = effect.targetBone;
                    effects[i].fwdAxis = effect.childFwdAxis;
                    effects[i].angleTolerance = effect.angleTolerance;
                    var rules = builder.Allocate(ref effects[i].rules, effect.rules.Length);
                    var rulesSource = effect.rules.ToList();
                    rulesSource.Sort((rule, rule1) => rule.priority > rule1.priority ? -1 : 1);
                    for (int z = 0; z < rulesSource.Count; z++)
                    {
                        var rule = rulesSource[z];
                        rules[z].weight = rule.weight;
                    
                        rules[z].rotatingBone = rule.parentBoneIndex;
                    }


                }
                var blob = builder.CreateBlobAssetReference<PantomimeEffects.EffectsSet>(Allocator.Persistent);
                builder.Dispose();
                AddBlobAsset(ref blob, out _);
                SetComponent(
                    entity,
                    new PantomimeEffects()
                    {
                        effectsSets = blob,
                    });
            }
        }
    }


}