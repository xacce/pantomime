# if UNITY_EDITOR
using Pantomime.Authoring.So;
using Unity.Mathematics;
using UnityEditor;

namespace Pantomime.Editor.Nodes
{
    public sealed class PantomimeGraphSingleMotionNode : AbstractGraphSingleMotionNode<PantomimeGraphMotionLayerPort, PantomimeGraphMotionClipsPort>
    {
        // private readonly Port _la;
        public PantomimeGraphSingleMotionNode(GUID guid, IPantomimeParams prms) : base(guid, prms)
        {
            Post();
        }
        public PantomimeGraphSingleMotionNode(PantomimeCollectionAuthoring._Motion motion, IPantomimeParams prms) : base(motion, prms)
        {
            title = "Motion";
            Post();
        }
        public override int2 variables => new int2(-1, -1);
    }
}
#endif