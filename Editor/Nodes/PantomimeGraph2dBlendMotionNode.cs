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
    public sealed class PantomimeGraph2dBlendMotionNode : AbstractGraphSingleMotionNode<PantomimeGraphMotionLayerPort, PantomimeGraphMotionBlendableClipsPort>
    {
        private readonly EnumField _x;

        private readonly EnumField _y;
        private readonly EnumField _type;


        public PantomimeGraph2dBlendMotionNode(
            GUID guid, PantomimeCollection.BlendMode blendMode, int trigger, ulong booleans, bool loop, bool isDefault, int2 variables, IPantomimeParams prms) : base(
            guid,
            trigger,
            booleans,
            loop,
            isDefault,
            prms)
        {
            title = "Blendable motion";
            _type = new EnumField(PantomimeCollection.BlendMode.FreeformCartesian2d) { value = blendMode };

            _x = new EnumField((Enum)Enum.ToObject(prms.GeDynamicValuesType(), 0))
            {
                value = (Enum)Enum.ToObject(prms.GeDynamicValuesType(), variables.x)
            };
            _y = new EnumField((Enum)Enum.ToObject(prms.GeDynamicValuesType(), 0))
            {
                value = (Enum)Enum.ToObject(prms.GeDynamicValuesType(), variables.y)
            };

            extensionContainer.Add(_x);
            extensionContainer.Add(_y);
            extensionContainer.Add(_type);
            RefreshExpandedState();
        }
        public override int2 variables => new int2((int)(object)_x.value, (int)(object)(_y.value));
        public override PantomimeCollection.BlendMode blendMode => (PantomimeCollection.BlendMode)_type.value;
    }
}