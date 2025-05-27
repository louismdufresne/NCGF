using UnityEngine;

//[] Operations
//[] Holds static functions to perform routine multi-step processes across NCGF
public static class NCGF_Operations
{
    public static MonoBehaviour SetAndReturnCleanedChild(MonoBehaviour parent, MonoBehaviour child)
    {
        if (parent == null || child == null) return child;

        var parentT = parent.transform;
        var childT = child.transform;

        childT.parent       = parentT;
        childT.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        childT.localScale   = Vector3.one;

        child.gameObject.layer = parent.gameObject.layer;
        return child;
    }
}
