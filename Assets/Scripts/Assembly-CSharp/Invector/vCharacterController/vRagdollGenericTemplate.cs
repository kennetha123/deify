using UnityEngine;

namespace Invector.vCharacterController
{
	[CreateAssetMenu(menuName = "Invector/Basic Locomotion/Components/vRagdoll Generic Template")]
	public class vRagdollGenericTemplate : ScriptableObject
	{
		[Header("--- Bones Names ---")]
		public string root = "Hips";

		public string leftHips = "LeftUpperLeg";

		public string leftKnee = "LeftLowerLeg";

		public string leftFoot = "LeftFoot";

		public string rightHips = "RightUpperLeg";

		public string rightKnee = "RightLowerLeg";

		public string rightFoot = "RightFoot";

		public string leftArm = "LeftUpperArm";

		public string leftElbow = "LeftLowerArm";

		public string rightArm = "RightUpperArm";

		public string rightElbow = "RightLowerArm";

		public string middleSpine = "Chest";

		public string head = "Head";

		public Transform GetRoot(Transform rootTransform)
		{
			return GetBone(root, rootTransform);
		}

		public Transform GetLeftHips(Transform rootTransform)
		{
			return GetBone(leftHips, rootTransform);
		}

		public Transform GetLeftKnee(Transform rootTransform)
		{
			return GetBone(leftKnee, rootTransform);
		}

		public Transform GetLeftFoot(Transform rootTransform)
		{
			return GetBone(leftFoot, rootTransform);
		}

		public Transform GetRightHips(Transform rootTransform)
		{
			return GetBone(rightHips, rootTransform);
		}

		public Transform GetRightKnee(Transform rootTransform)
		{
			return GetBone(rightKnee, rootTransform);
		}

		public Transform GetRightFoot(Transform rootTransform)
		{
			return GetBone(rightFoot, rootTransform);
		}

		public Transform GetLeftArm(Transform rootTransform)
		{
			return GetBone(leftArm, rootTransform);
		}

		public Transform GetLeftElbow(Transform rootTransform)
		{
			return GetBone(leftElbow, rootTransform);
		}

		public Transform GetRightArm(Transform rootTransform)
		{
			return GetBone(rightArm, rootTransform);
		}

		public Transform GetRightElbow(Transform rootTransform)
		{
			return GetBone(rightElbow, rootTransform);
		}

		public Transform GetMiddleSpine(Transform rootTransform)
		{
			return GetBone(middleSpine, rootTransform);
		}

		public Transform GetHead(Transform rootTransform)
		{
			return GetBone(head, rootTransform);
		}

		private Transform GetBone(string boneName, Transform rootTransform)
		{
			Transform[] componentsInChildren = rootTransform.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.name.Contains(boneName))
				{
					return componentsInChildren[i];
				}
				if (componentsInChildren[i].gameObject.name.ToUpper().Contains(boneName))
				{
					return componentsInChildren[i];
				}
				if (componentsInChildren[i].gameObject.name.ToUpper().Contains(boneName.ToUpper()))
				{
					return componentsInChildren[i];
				}
				if (componentsInChildren[i].gameObject.name.ToLower().Contains(boneName.ToUpper()))
				{
					return componentsInChildren[i];
				}
				if (componentsInChildren[i].gameObject.name.ToLower().Contains(boneName.ToLower()))
				{
					return componentsInChildren[i];
				}
				if (componentsInChildren[i].gameObject.name.ToLower().Contains(boneName))
				{
					return componentsInChildren[i];
				}
			}
			return null;
		}
	}
}
