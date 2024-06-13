using System;
using Unity.Mathematics;
using UnityEngine;

namespace Pantomime.Authoring.So
{
    [CreateAssetMenu(menuName = "Pantomime/Create avatar mask")]
    public class PantomimeAvatarMask : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private PantomimeMaskCreator mask_s;
        [SerializeField] private Parts parts_s;

        [SerializeField] public ulong[] mask = Array.Empty<ulong>();


        public void OnBeforeSerialize()
        {
            if (mask_s == null) return;
            bool[] booleanArray = new bool[mask_s.bones.Length];
            foreach (var bone in mask_s.bones)
            {
                if ((parts_s & bone.part) != 0)
                {
                    booleanArray[bone.boneIndex] = true;
                }
            }

            mask = new ulong[(int)math.ceil(booleanArray.Length / 64f)];

            for (int i = 0; i < booleanArray.Length; i++)
            {
                int ulongIndex = i / 64;
                int bitIndex = i % 64;
                if (!booleanArray[i])
                {
                    mask[ulongIndex] |= (ulong)1 << bitIndex;
                }
                else
                {
                    mask[ulongIndex] &= ~((ulong)1 << bitIndex);
                }
            }
        }
        public void OnAfterDeserialize()
        {
        }
    }
}