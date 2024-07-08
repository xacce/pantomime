# if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pantomime.Authoring.So
{
    [CreateAssetMenu(menuName = "Pantomime/Animator")]
    public class PantomimeAnimator : ScriptableObject
    {
        [SerializeField] public PantomimeCollectionAuthoring._Layer[] layers_s = Array.Empty<PantomimeCollectionAuthoring._Layer>();
        [SerializeField] public PantomimeParamsUnified params_s;
        [SerializeField] public PantomimeCollectionAuthoring.GraphPosition[] positions_s = Array.Empty<PantomimeCollectionAuthoring.GraphPosition>();
    }
}
#endif