# if UNITY_EDITOR
using System;
using Pantomime.Authoring.So;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pantomime.Editor.Nodes
{
    public sealed class PantomimeGraph2dBlendMotionNode : AbstractGraphSingleMotionNode<PantomimeGraphMotionLayerPort, PantomimeGraphMotionBlendableClipsPort>
    {
        private  EnumField _x;

        private  EnumField _y;
        private  EnumField _type;

        public PantomimeGraph2dBlendMotionNode(GUID guid, IPantomimeParams prms) : base(guid, prms)
        {
            title = "Blendable motion";
            _type = new EnumField(PantomimeCollection.BlendMode.FreeformCartesian2d) { };
            _x = new EnumField((Enum)Enum.ToObject(prms.GeDynamicValuesType(), 0));
            _y = new EnumField((Enum)Enum.ToObject(prms.GeDynamicValuesType(), 0));
            Post();
        }

        public PantomimeGraph2dBlendMotionNode(PantomimeCollectionAuthoring._Motion motion, IPantomimeParams prms) : base(
            motion,
            prms)
        {
            _type = new EnumField(PantomimeCollection.BlendMode.FreeformCartesian2d) { value = motion.blendMode };
            _x = new EnumField((Enum)Enum.ToObject(prms.GeDynamicValuesType(), 0))
            {
                value = (Enum)Enum.ToObject(prms.GeDynamicValuesType(), motion.dynamicVariables.x)
            };
            _y = new EnumField((Enum)Enum.ToObject(prms.GeDynamicValuesType(), 0))
            {
                value = (Enum)Enum.ToObject(prms.GeDynamicValuesType(), motion.dynamicVariables.y)
            };
            Post();
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
#endif