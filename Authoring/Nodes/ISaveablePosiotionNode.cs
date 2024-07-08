# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Pantomime.Editor.Nodes
{
    public interface ISaveablePosiotionNode
    {
        public Rect GetPosition();
        public void SetPosition(Rect newPos);
        public GUID guid { get; }
    }
}
#endif