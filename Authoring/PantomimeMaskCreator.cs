# if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using Latios.Kinemation;
using NUnit.Framework;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pantomime.Authoring
{
    [TemporaryBakingType]
    internal partial struct PantomimeMaskBakingBodyPartElement : IBufferElementData
    {
        public Parts part;
        public Entity entity;

    }

    [TemporaryBakingType]
    internal partial struct PantomimeMaskBakingBodyData : IComponentData
    {
        // public Parts part;
        public UnityObjectRef<PantomimeMaskCreator> mask;
    }

    [Flags]
    internal enum Parts
    {
        Nothing = 0,
        LeftHand = 1,
        RightHand = 2,
        Head = 4,
        UpperBody = 8,
        LeftLeg = 16,
        RightLeg = 32,
        Hips = 64,
    }

    [Serializable]
    struct PrebakedPantomimeBodyPart
    {
        public Parts part;
        public int boneIndex;
    }

    class PantomimeMaskCreator : MonoBehaviour
    {
        [SerializeField] private GameObject[] leftHand_s = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] rightHand_s = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] head_s = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] upperBody_s = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] leftLeg_s = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] rightLeg_s = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] hips_s = Array.Empty<GameObject>();

        [SerializeField] public PrebakedPantomimeBodyPart[] bones = Array.Empty<PrebakedPantomimeBodyPart>();
        // [SerializeField] public int boneCount = 0;

        class B : Baker<PantomimeMaskCreator>
        {
            private void GetEntities(Entity entity, GameObject[] gos, Parts to)
            {
                foreach (var go in gos)
                {
                    AppendToBuffer(entity, new PantomimeMaskBakingBodyPartElement() { entity = GetEntity(go, TransformUsageFlags.None), part = to });
                }
            }

            public override void Bake(PantomimeMaskCreator authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(
                    entity,
                    new PantomimeMaskBakingBodyData
                    {
                        // part = authoring.parts_s,
                        mask = authoring,
                    });
                AddBuffer<PantomimeMaskBakingBodyPartElement>(entity);
                GetEntities(entity, authoring.leftHand_s, Parts.LeftHand);
                GetEntities(entity, authoring.rightHand_s, Parts.RightHand);
                GetEntities(entity, authoring.head_s, Parts.Head);
                GetEntities(entity, authoring.upperBody_s, Parts.UpperBody);
                GetEntities(entity, authoring.leftLeg_s, Parts.LeftLeg);
                GetEntities(entity, authoring.rightLeg_s, Parts.RightLeg);
                GetEntities(entity, authoring.hips_s, Parts.Hips);
            }
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial class BSs : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var (cfg, elements) in SystemAPI.Query<PantomimeMaskBakingBodyData, DynamicBuffer<PantomimeMaskBakingBodyPartElement>>())
            {
                short max = short.MinValue;

                foreach (var element in elements)
                {
                    var index = EntityManager.GetComponentData<CopyLocalToParentFromBone>(element.entity).boneIndex;
                    if (index > max)
                    {
                        max = index;
                    }

                }
                var so = new SerializedObject(cfg.mask.Value);
                PrebakedPantomimeBodyPart[] parts = new PrebakedPantomimeBodyPart[max+1];
                foreach (var element in elements)
                {
                    var index = EntityManager.GetComponentData<CopyLocalToParentFromBone>(element.entity).boneIndex;
                    parts[index] = new PrebakedPantomimeBodyPart()
                    {
                        part = element.part,
                        boneIndex = index,
                    };
                }
                var f = so.FindProperty(nameof(PantomimeMaskCreator.bones));
                f.arraySize = parts.Length;
                foreach (var part in parts)
                {
                    f.GetArrayElementAtIndex(part.boneIndex).boxedValue = part;
                }
                so.ApplyModifiedPropertiesWithoutUndo();

            }
        }
    }

}
#endif