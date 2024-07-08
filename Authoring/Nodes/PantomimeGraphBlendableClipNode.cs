# if UNITY_EDITOR
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pantomime.Editor.Nodes
{
    public sealed class PantomimeGraphBlendableClipNode : Node, ISaveablePosiotionNode, IClipNode
    {
        private readonly Port _motionPort;
        private readonly GUID _guid;
        private readonly ObjectField _clip;
        private readonly Vector2Field _blendPosition;


        public PantomimeGraphBlendableClipNode(GUID guid, float2 blendPosition, AnimationClip clip = null)
        {
            title = "Blendable clip";
            _guid = guid;
            _motionPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(PantomimeGraphBlendableClipMotionPort));
            _motionPort.portName = "Motion";
            _clip = new ObjectField("Clip")
            {
                value = clip,
                style = { minWidth = 250}
            };
            _blendPosition = new Vector2Field("Blend")
            {
                value = blendPosition
            };
            extensionContainer.Add(_clip);
            extensionContainer.Add(_blendPosition);
            inputContainer.Add(_motionPort);
            RefreshExpandedState();
        }

        public Port motionPort => _motionPort;
        public AnimationClip clip => _clip.value is AnimationClip c ? c : null;
        public GUID guid => _guid;
        public float2 blendPosition => _blendPosition.value;
    }
}
#endif