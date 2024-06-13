using Pantomime.Authoring.So;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Pantomime.Editor.Nodes
{
    public sealed class PantomimeGraphLayerNode : Node, ISaveablePosiotionNode
    {
        private readonly Port _motions;
        private readonly Toggle _override;
        private readonly GUID _guid;
        private ObjectField _avatarMask;
        private readonly FloatField _baseWeight;

        public PantomimeGraphLayerNode(GUID guid, PantomimeAvatarMask mask = null, bool overrideMode = false,float baseWeight=1f)
        {
            title = "Layer";
            _guid = guid;
            _motions = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(PantomimeGraphLayerMotionsPort));
            _motions.portName = "Motions";
            _override = new Toggle("Override mode") { value = overrideMode };
            _baseWeight = new FloatField("Weight") { value = baseWeight };
            _avatarMask = new ObjectField("Mask")
            {
                value = mask,
            };
            extensionContainer.Add(_avatarMask);
            extensionContainer.Add(_override);
            extensionContainer.Add(_baseWeight);
            outputContainer.Add(_motions);
            RefreshExpandedState();
        }

        public bool overrideMode => _override.value;
        public Port motionsPort => _motions;

        public int GetTypeId()
        {
            return 0;
        }

        public GUID guid => _guid;
        public bool hasAvatarMask => _avatarMask.value != null && (_avatarMask.value is PantomimeAvatarMask);
        public PantomimeAvatarMask mask => (PantomimeAvatarMask)_avatarMask.value;
        public float baseWeight => _baseWeight.value;
    }
}