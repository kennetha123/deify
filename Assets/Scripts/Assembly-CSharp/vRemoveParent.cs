using UnityEngine;

public class vRemoveParent : MonoBehaviour
{
	public void RemoveParentOfOtherTransform(Transform target)
	{
		target.parent = null;
	}

	public void RemoveParent()
	{
		base.transform.parent = null;
	}
}
