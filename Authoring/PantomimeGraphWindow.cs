#if UNITY_EDITOR
using System.Collections.Generic;
using Pantomime.Authoring.So;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pantomime
{
    public class PantomimeGraphWindow : EditorWindow
    {
        PantomimeGraphEditor _graph;
        private PantomimeAnimator _sourceData;

        // [MenuItem("Pantomime/Create pantomime")]
        // public static void FromMenu()
        // {
        //     
        // }

  
        public void Init(PantomimeAnimator animator)
        {
            _sourceData = animator;
            Init();
        }
        

        private void Init()
        {
            rootVisualElement.Clear();
            _graph = new PantomimeGraphEditor()
            {
                name = "Pantomime",
            };
            _graph.StretchToParentSize();
            Dictionary<GUID, Rect> restoredPositions = new Dictionary<GUID, Rect>();
           
            foreach (var graphPosition in _sourceData.positions_s)
            {
                if (GUID.TryParse(graphPosition.guid, out var guid)) restoredPositions[guid] = graphPosition.rect;
            }
            _graph.LoadFromAuthoring(_sourceData);
            _graph.RestorePositions(restoredPositions);
            rootVisualElement.Add(_graph);
        }

      

        private void OnDisable()
        {
            if (_graph != null && _sourceData != null)
            {
                _graph.SavePositions();
            }
            if (rootVisualElement != null)
                rootVisualElement.Clear();
        }
    }
}
#endif