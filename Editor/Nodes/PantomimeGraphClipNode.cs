using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pantomime.Editor.Nodes
{
    public interface IClipNode : ISaveablePosiotionNode
    {
        public Port motionPort { get; }
        public AnimationClip clip { get; }
        public GUID guid { get; }
        public float2 blendPosition { get; }
    }

    public sealed class PantomimeGraphClipNode : Node, IClipNode
    {
        private readonly Port _motionPort;
        private readonly GUID _guid;
        private readonly ObjectField _clip;


        public PantomimeGraphClipNode(GUID guid, AnimationClip clip = null)
        {
            title = "Clip";
            _guid = guid;
            _motionPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(PantomimeGraphClipMotionPort));
            _motionPort.portName = "Motion";
            _clip = new ObjectField("Clip")
            {
                value = clip,
            };
            extensionContainer.Add(_clip);
            inputContainer.Add(_motionPort);
            RefreshExpandedState();
        }

        public Port motionPort => _motionPort;
        public AnimationClip clip => _clip.value is AnimationClip c ? c : null;
        public float2 blendPosition => new float2(-1, -1);
        public GUID guid => _guid;
    }
}