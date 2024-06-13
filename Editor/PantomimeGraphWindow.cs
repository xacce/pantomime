using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Pantomime.Editor.Nodes;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using xdd.Pantomimess.Authoring;

namespace Pantomime.Editor
{
    public class PantomimeGraphWindow : EditorWindow
    {
        PantomimeGraphEditor _graph;
        private PantomimeCollectionAuthoring _sourceData;

        [MenuItem("Pantomime/Create pantomime")]
        public static void FromMenu()
        {
            var w = GetWindow<PantomimeGraphWindow>();
            w.Show();
        }

        private void OnEnable()
        {
            Init();
            EditorApplication.playModeStateChanged += Init;
        }

        private void Init(PlayModeStateChange obj)
        {
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
            if (Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent(out PantomimeCollectionAuthoring sourceData))
            {
                _sourceData = sourceData;
                foreach (var graphPosition in _sourceData._positions)
                {
                    if (GUID.TryParse(graphPosition.guid, out var guid)) restoredPositions[guid] = graphPosition.rect;
                }
                _graph.LoadFromAuthoring(sourceData);
                _graph.RestorePositions(restoredPositions);
            }
            rootVisualElement.Add(_graph);
        }

      

        private void OnDisable()
        {
            if (_graph != null && _sourceData != null)
            {
                _graph.SavePositions();
            }
            EditorApplication.playModeStateChanged -= Init;
            if (rootVisualElement != null)
                rootVisualElement.Clear();
        }
    }
}