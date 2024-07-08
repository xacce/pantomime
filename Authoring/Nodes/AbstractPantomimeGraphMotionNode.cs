# if UNITY_EDITOR
using System;
using JetBrains.Annotations;
using Pantomime.Authoring.So;
using Pantomime.Editor.Nodes;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pantomime
{
    public interface IMotionNode : ISaveablePosiotionNode
    {
        public bool isDefault { get; }
        public bool allowReentering { get; }
        public bool loop { get; }
        public ulong booleans { get; }
        public int trigger { get; }
        public Port layerPort { get; }
        public Port clipsPort { get; }
        public PantomimeCollection.BlendMode blendMode { get; }
        public int2 variables { get; }
        bool disableAutoExit { get; }
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
        private readonly Toggle _disableAutoExit;
        private readonly Toggle _allowReentering;

        protected AbstractGraphSingleMotionNode(GUID guid, IPantomimeParams prms)
        {
            _guid = guid;
            _layer = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(TLayerPortType));
            _clips = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(TMotionPortType));
            _layer.portName = "Layer";
            _clips.portName = "Clips";

            _trigger = new EnumField((Enum)Enum.ToObject(prms.GetTriggersType(), 0));
            _booleans = new EnumFlagsField((Enum)Enum.ToObject(prms.GetFlagsType(), 0));
            _loop = new Toggle("Loop");
            _isDefault = new Toggle("Is default");
            _disableAutoExit = new Toggle("Disable auto exit");
            _allowReentering = new Toggle("Allow re-entering");
        }

        protected AbstractGraphSingleMotionNode(PantomimeCollectionAuthoring._Motion motion, IPantomimeParams prms)
        {
            _guid = new GUID(motion.guid);
            _layer = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(TLayerPortType));
            _clips = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(TMotionPortType));
            _layer.portName = "Layer";
            _clips.portName = "Clips";

            _trigger = new EnumField((Enum)Enum.ToObject(prms.GetTriggersType(), 0))
            {
                value = (Enum)Enum.ToObject(prms.GetTriggersType(), motion.trigger)
            };
            _booleans = new EnumFlagsField((Enum)Enum.ToObject(prms.GetFlagsType(), 0))
            {
                value = (Enum)Enum.ToObject(prms.GetFlagsType(), motion.boolean)
            };
            _loop = new Toggle("Loop")
            {
                value = motion.loop,
            };
            _isDefault = new Toggle("Is default")
            {
                value = motion.isDefault
            };
            _disableAutoExit = new Toggle("Disable auto exit")
            {
                value = motion.disableAutoExit
            };
            _allowReentering = new Toggle("Allow re-entering")
            {
                value = motion.allowReentering
            };
        }

        protected virtual void Post()
        {
            Debug.Log("Override post");
            extensionContainer.Add(_trigger);
            extensionContainer.Add(_booleans);
            extensionContainer.Add(_loop);
            extensionContainer.Add(_isDefault);
            extensionContainer.Add(_disableAutoExit);
            extensionContainer.Add(_allowReentering);
            outputContainer.Add(_clips);
            inputContainer.Add(_layer);
            RefreshExpandedState();
        }

        public bool isDefault => _isDefault.value;
        public bool allowReentering => _allowReentering.value;
        public bool loop => _loop.value;
        public ulong booleans => (ulong)(int)(object)_booleans.value;
        public int trigger => (int)(object)_trigger.value;
        public Port layerPort => _layer;
        public Port clipsPort => _clips;

        public virtual PantomimeCollection.BlendMode blendMode => PantomimeCollection.BlendMode.Nothing;
        public virtual int2 variables => new int2(-1, -1);
        public bool disableAutoExit => _disableAutoExit.value;

        public GUID guid => _guid;
    }
}
#endif