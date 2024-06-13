using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace xdd.Pantomimess.Authoring
{
    public class PantomimeEffectsAuthoring : MonoBehaviour
    {
        [Serializable]
        class _Rule
        {
            public int priority;
            public int boneIndex;
            public float weight;
            public float angleLimit;
            public float angleTolerance;
            public float3 fixedRotationOffset;
        }

        [Serializable]
        class _Effect
        {
            public float2 minMaxDistance;
            public PantomimeEffects.Type type;
            public _Rule[] rules = Array.Empty<_Rule>();
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
                    effects[i].minMaxDistanceSq = new float2(effect.minMaxDistance.x * effect.minMaxDistance.x, effect.minMaxDistance.y * effect.minMaxDistance.y);
                    var rules = builder.Allocate(ref effects[i].rules, effect.rules.Length);
                    var rulesSource = effect.rules.ToList();
                    rulesSource.Sort((rule, rule1) => rule.priority > rule1.priority ? -1 : 1);
                    for (int z = 0; z < rulesSource.Count; z++)
                    {
                        var rule = rulesSource[z];
                        rules[z].weight = rule.weight;
                        rules[z].boneIndex = rule.boneIndex;
                        rules[z].angleLimit = math.radians(rule.angleLimit);
                        rules[z].angleTolerance = rule.angleTolerance;
                        rules[z].fixedRotationOffset = quaternion.Euler(math.radians(rule.fixedRotationOffset));
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