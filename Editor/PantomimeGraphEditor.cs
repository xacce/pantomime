using System;
using System.Collections.Generic;
using System.Linq;
using Pantomime.Editor.Nodes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using xdd.Pantomimess;
using xdd.Pantomimess.Authoring;

namespace Pantomime.Editor
{
    interface Isd
    {
        public static string kek { get; }
    }

    public class PantomimeGraphEditor : GraphView
    {
        private PantomimeCollectionAuthoring _authoring;

        public PantomimeGraphEditor()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            var toolbar = new Toolbar() { };
            var compileBtn = new Button(() => { Compile(); }) { text = "Compile" };
            toolbar.Add(compileBtn);
            Add(toolbar);
        }
       

        public void SavePositions()
        {
            var so = new SerializedObject(_authoring);
            var field = so.FindProperty("_positions");
            field.arraySize = 0;
            field.ClearArray();
            UpdatePositions(field);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void Compile()
        {
            var so = new SerializedObject(_authoring);
            var field = so.FindProperty("layers_s");
            field.ClearArray();
            List<PantomimeGraphLayerNode> layers = new List<PantomimeGraphLayerNode>();
            foreach (var node in nodes)
            {
                if (node is PantomimeGraphLayerNode layer)
                {
                    layers.Add(layer);
                }
            }
            foreach (var layer in layers.OrderBy(node => node.GetPosition().y))
            {
                field.arraySize++;
                var layerSerialized = new PantomimeCollectionAuthoring._Layer
                {
                    guid = layer.guid.ToString(),
                    mask = layer.mask,
                    overrideMode = layer.overrideMode,
                    baseWeight = layer.baseWeight,
                };
                List<PantomimeCollectionAuthoring._Motion> motions = new List<PantomimeCollectionAuthoring._Motion>();
                foreach (var motionsPortConnection in layer.motionsPort.connections)
                {
                    if (motionsPortConnection.input.node is not IMotionNode motion) continue;
                    var motionSerialized = new PantomimeCollectionAuthoring._Motion
                    {
                        boolean = motion.booleans,
                        guid = motion.guid.ToString(),
                        loop = motion.loop,
                        trigger = motion.trigger,
                        blendMode = motion.blendMode,
                        isDefault = motion.isDefault,
                        dynamicVariables = motion.variables,
                    };
                    List<PantomimeCollectionAuthoring._Motion._Clip> clips = new List<PantomimeCollectionAuthoring._Motion._Clip>();
                    foreach (var clipsPortConnection in motion.clipsPort.connections)
                    {
                        if (clipsPortConnection.input.node is not IClipNode clipNode) continue;
                        var clipSerialized = new PantomimeCollectionAuthoring._Motion._Clip
                        {
                            clip = clipNode.clip,
                            guid = clipNode.guid.ToString(),
                            blendPosition = clipNode.blendPosition,
                        };
                        clips.Add(clipSerialized);
                    }
                    motionSerialized.clips = clips.ToArray();
                    motions.Add(motionSerialized);
                }
                layerSerialized.motions = motions.ToArray();
                field.GetArrayElementAtIndex(field.arraySize - 1).boxedValue = layerSerialized;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            SavePositions();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (evt.target is GraphView)
            {
                evt.menu.AppendAction("Add layer", e => AddLayer(e));
                evt.menu.AppendAction("Add motion", e => AddMotion(e));
                evt.menu.AppendAction("Add clip", e => AddClip(e));
                evt.menu.AppendAction("Add blendable motion", e => AddBlendableMotion(e));
                evt.menu.AppendAction("Add blendable clip", e => AddBlendableClip(e));
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compat = new List<Port>();
            foreach (var port in ports)
            {
                if (startPort == port || startPort.node == port.node) continue;
                AddSymmetryCompat(typeof(PantomimeGraphLayerMotionsPort), typeof(PantomimeGraphMotionLayerPort), startPort, port, compat);
                AddSymmetryCompat(typeof(PantomimeGraphMotionClipsPort), typeof(PantomimeGraphClipMotionPort), startPort, port, compat);
                AddSymmetryCompat(typeof(PantomimeGraphMotionBlendableClipsPort), typeof(PantomimeGraphBlendableClipMotionPort), startPort, port, compat);
            }
            return compat;
        }

        private void AddSymmetryCompat(Type t1, Type t2, Port p1, Port p2, List<Port> ports)
        {
            if (p1.portType == t1 && p2.portType == t2) ports.Add(p2);
            if (p1.portType == t2 && p2.portType == t1) ports.Add(p2);
        }

        void AddClip(DropdownMenuAction dropdownMenuAction)
        {
            var c = new PantomimeGraphClipNode(GUID.Generate());
            AddElement(c);
            c.SetPosition(new Rect(viewTransform.matrix.inverse.MultiplyPoint(dropdownMenuAction.eventInfo.localMousePosition), new Vector2(100, 100)));

        }

        void AddBlendableClip(DropdownMenuAction dropdownMenuAction)
        {
            var c = new PantomimeGraphBlendableClipNode(GUID.Generate(), float2.zero);
            AddElement(c);
            c.SetPosition(new Rect(viewTransform.matrix.inverse.MultiplyPoint(dropdownMenuAction.eventInfo.localMousePosition), new Vector2(100, 100)));
        }

        void AddBlendableMotion(DropdownMenuAction dropdownMenuAction)
        {
            var m = new PantomimeGraph2dBlendMotionNode(GUID.Generate(), PantomimeCollection.BlendMode.Nothing, 0, 0, false, false, int2.zero, _authoring.params_s);
            AddElement(m);
            m.SetPosition(new Rect(viewTransform.matrix.inverse.MultiplyPoint(dropdownMenuAction.eventInfo.localMousePosition), new Vector2(100, 100)));
        }

        void AddLayer(DropdownMenuAction dropdownMenuAction)
        {
            var l = new PantomimeGraphLayerNode(GUID.Generate());
            AddElement(l);
            l.SetPosition(new Rect(viewTransform.matrix.inverse.MultiplyPoint(dropdownMenuAction.eventInfo.localMousePosition), new Vector2(100, 100)));
        }

        void AddMotion(DropdownMenuAction dropdownMenuAction)
        {
            var m = new PantomimeGraphSingleMotionNode(GUID.Generate(), 0, 0, false, false, _authoring.params_s);
            AddElement(m);
            m.SetPosition(new Rect(viewTransform.matrix.inverse.MultiplyPoint(dropdownMenuAction.eventInfo.localMousePosition), new Vector2(100, 100)));
        }

        private void UpdatePositions(SerializedProperty prop)
        {
            int i = 0;
            foreach (var node in nodes)
            {
                if (node is ISaveablePosiotionNode saveable)
                {
                    prop.arraySize++;
                    prop.GetArrayElementAtIndex(prop.arraySize - 1).boxedValue = new PantomimeCollectionAuthoring.GraphPosition
                    {
                        guid = saveable.guid.ToString(),
                        rect = saveable.GetPosition()
                    };
                    i++;
                }
            }
        }

        public void LoadFromAuthoring(PantomimeCollectionAuthoring authoring)
        {
            _authoring = authoring;
            foreach (var layer in authoring.layers_s)
            {
                if (!GUID.TryParse(layer.guid, out var guid)) continue;
                var layerNode = new PantomimeGraphLayerNode(guid, layer.mask, layer.overrideMode, layer.baseWeight);
                AddElement(layerNode);
                foreach (var motion in layer.motions)
                {
                    if (!GUID.TryParse(motion.guid, out var mguid)) continue;
                    IMotionNode node;
                    if (motion.blendMode == PantomimeCollection.BlendMode.Nothing)
                    {
                        node = new PantomimeGraphSingleMotionNode(mguid, (int)motion.trigger, (ulong)motion.boolean, motion.loop, motion.isDefault, _authoring.params_s);
                    }
                    else
                    {
                        node = new PantomimeGraph2dBlendMotionNode(
                            mguid,
                            motion.blendMode,
                            (int)motion.trigger,
                            (ulong)motion.boolean,
                            motion.loop,
                            motion.isDefault,
                            motion.dynamicVariables,
                            _authoring.params_s);
                    }
                    AddElement((GraphElement)node);
                    var edge = layerNode.motionsPort.ConnectTo(node.layerPort);
                    AddElement(edge);
                    
                    foreach (var clip in motion.clips)
                    {
                        if (!GUID.TryParse(clip.guid, out var clipGuid)) continue;
                        IClipNode clipNode;
                        if (motion.blendMode == PantomimeCollection.BlendMode.Nothing)
                        {
                            clipNode = new PantomimeGraphClipNode(clipGuid, clip.clip);
                        }
                        else
                        {
                            clipNode = new PantomimeGraphBlendableClipNode(clipGuid, clip.blendPosition, clip.clip);
                        }

                        AddElement((GraphElement)clipNode);
                        AddElement(node.clipsPort.ConnectTo(clipNode.motionPort));
                    }
                }
            }
        }

        public void RestorePositions(Dictionary<GUID, Rect> restoredPositions)
        {
            foreach (var node in nodes)
            {
                if (node is ISaveablePosiotionNode saveablePosiotionNode && restoredPositions.TryGetValue(saveablePosiotionNode.guid, out var pos))
                {
                    saveablePosiotionNode.SetPosition(pos);
                }
            }
        }
    }
}