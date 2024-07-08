#if UNITY_EDITOR
using Pantomime.Authoring.So;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Pantomime
{
    [CustomEditor(typeof(PantomimeAnimator))]
    public class PantomimeAnimatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Edit animations"))
            {
                var w = EditorWindow.GetWindow<PantomimeGraphWindow>();
                w.Init((PantomimeAnimator)target);
                // w.Show();
            }
        }
    }
}
#endif