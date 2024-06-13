using System;
using System.Linq;
using Pantomime.Authoring.So;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using xdd.Pantomimess;

namespace Pantomime.Editor.Nodes
{
    public sealed class PantomimeGraphSingleMotionNode : AbstractGraphSingleMotionNode<PantomimeGraphMotionLayerPort, PantomimeGraphMotionClipsPort>
    {
        // private readonly Port _la;

        public PantomimeGraphSingleMotionNode(GUID guid, int trigger, ulong booleans, bool loop, bool isDefault,IPantomimeParams prms) : base(guid, trigger, booleans, loop, isDefault, prms)
        {
            title = "Motion";
        }
        public override int2 variables => new int2(-1, -1);
    }
}