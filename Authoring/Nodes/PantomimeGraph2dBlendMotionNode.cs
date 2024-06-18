using System;
using Pantomime.Authoring.So;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine.UIElements;

namespace Pantomime.Editor.Nodes
{
    public sealed class PantomimeGraph2dBlendMotionNode : AbstractGraphSingleMotionNode<PantomimeGraphMotionLayerPort, PantomimeGraphMotionBlendableClipsPort>
    {
        private readonly EnumField _x;

        private readonly EnumField _y;
        private readonly EnumField _type;

        public PantomimeGraph2dBlendMotionNode(GUID guid, IPantomimeParams prms) : base(guid, prms)
        {
            title = "Blendable motion";
            _type = new EnumField(PantomimeCollection.BlendMode.FreeformCartesian2d) { };
            _x = new EnumField((Enum)Enum.ToObject(prms.GeDynamicValuesType(), 0));
            _y = new EnumField((Enum)Enum.ToObject(prms.GeDynamicValuesType(), 0));
        }
        public PantomimeGraph2dBlendMotionNode(PantomimeCollectionAuthoring._Motion motion, IPantomimeParams prms) : base(
            motion,
            prms)
        {
            RefreshExpandedState();
        }
        protected override void Post()
        {
            extensionContainer.Add(_x);
            extensionContainer.Add(_y);
            extensionContainer.Add(_type);
            base.Post();
        }
        public override int2 variables => new int2((int)(object)_x.value, (int)(object)(_y.value));
        public override PantomimeCollection.BlendMode blendMode => (PantomimeCollection.BlendMode)_type.value;
    }
}