using System;
using System.Linq;
using Pantomime.Authoring.So;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using xdd.Pantomimess;

namespace Pantomime.Editor.Nodes
{
    public interface IMotionNode : ISaveablePosiotionNode
    {
        public bool isDefault { get; }
        public bool loop { get; }
        public ulong booleans { get; }
        public int trigger { get; }
        public Port layerPort { get; }
        public Port clipsPort { get; }
        public PantomimeCollection.BlendMode blendMode { get; }
        public int2 variables { get; }
    }

    public abstract class AbstractGraphSingleMotionNode<TLayerPortType, TMotionPortType> : Node, IMotionNode
    {
        private readonly Port _layer;
        private readonly Port _clips;

        private readonly EnumFlagsField _booleans;
        private readonly EnumField _trigger;
        private readonly Toggle _isDefault;
        private readonly Toggle _loop;
        private readonly GUID _guid;

        // private readonly Port _la;

        public AbstractGraphSingleMotionNode(GUID guid, int trigger, ulong booleans, bool loop, bool isDefault, IPantomimeParams prms)
        {
            _guid = guid;
            _layer = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(TLayerPortType));
            _clips = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(TMotionPortType));
            _layer.portName = "Layer";
            _clips.portName = "Clips";
            _trigger = new EnumField((Enum)Enum.ToObject(prms.GetTriggersType(), 0))
            {
                value = (Enum)Enum.ToObject(prms.GetTriggersType(), trigger)
            };
            _booleans = new EnumFlagsField((Enum)Enum.ToObject(prms.GetFlagsType(), 0))
            {
                value =  (Enum)Enum.ToObject(prms.GetFlagsType(), booleans)
            };
            _loop = new Toggle("Loop")
            {
                value = loop,
            };
            _isDefault = new Toggle("Is default")
            {
                value = isDefault
            };
            extensionContainer.Add(_trigger);
            extensionContainer.Add(_booleans);
            extensionContainer.Add(_loop);
            extensionContainer.Add(_isDefault);
            outputContainer.Add(_clips);
            inputContainer.Add(_layer);
            RefreshExpandedState();
        }

        public bool isDefault => _isDefault.value;
        public bool loop => _loop.value;
        public ulong booleans => (ulong)(int)(object)_booleans.value;
        public int trigger => (int)(object)_trigger.value;
        public Port layerPort => _layer;
        public Port clipsPort => _clips;

        public virtual PantomimeCollection.BlendMode blendMode => PantomimeCollection.BlendMode.Nothing;
        public virtual int2 variables => new int2(-1, -1);

        public GUID guid => _guid;
    }
}