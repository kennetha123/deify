using UnityEngine;

[DisallowMultipleComponent]
public class SimpleHumanoidFootIK : MonoBehaviour
{
	public struct IKTarget
	{
		public Vector3 position;

		public Vector3 hintPosition;

		public bool ikEnabled;

		public Quaternion rotation;

		public bool rotationEnabled;

		public AvatarIKGoal ikGoal;

		public AvatarIKHint ikHint;

		public bool hintEnabled;

		public HumanBodyBones ikBone;

		public HumanBodyBones hintBone;

		public RaycastHit hit;

		public float boneTargetDistance;

		public float hintOffset;

		public float advancedForwardDetectionHeight;

		public IKTarget(Vector3 position, bool ikEnabled, Quaternion rotation, bool rotationEnabled, AvatarIKGoal ikGoal, AvatarIKHint ikHint, Vector3 hintPosition, bool hintEnabled, HumanBodyBones ikBone, HumanBodyBones hintBone, RaycastHit hit, float boneTargetDistance, float hintOffset, float advancedForwardDetectionHeight)
		{
			this.position = position;
			this.hintPosition = hintPosition;
			this.ikEnabled = ikEnabled;
			this.rotation = rotation;
			this.rotationEnabled = rotationEnabled;
			this.ikGoal = ikGoal;
			this.ikHint = ikHint;
			this.hintEnabled = hintEnabled;
			this.ikBone = ikBone;
			this.hintBone = hintBone;
			this.hit = hit;
			this.boneTargetDistance = boneTargetDistance;
			this.hintOffset = hintOffset;
			this.advancedForwardDetectionHeight = advancedForwardDetectionHeight;
		}
	}

	public bool ikEnabled = true;

	public LayerMask layerMask = 1;

	public float groundedControlTolerance = 0.02f;

	public bool advancedForwardDetect = true;

	public float advancedForwardDetectionRange = 0.25f;

	public float raycastDistance = 0.5f;

	public float fixedVerticalBodyPositionOffset;

	public float lerpValue = 0.5f;

	private Animator animator;

	private IKTarget[] targets;

	private float bodyOffset;

	private void Start()
	{
		targets = new IKTarget[2];
		targets[0] = new IKTarget(Vector3.zero, false, Quaternion.identity, false, AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee, Vector3.zero, false, HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg, default(RaycastHit), 0f, 0f, 0f);
		targets[1] = new IKTarget(Vector3.zero, false, Quaternion.identity, false, AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee, Vector3.zero, false, HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg, default(RaycastHit), 0f, 0f, 0f);
	}

	private void Update()
	{
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
	}

	private void OnAnimatorIK()
	{
		if (ikEnabled && !(animator == null))
		{
			lerpValue = Mathf.Clamp01(lerpValue);
			bodyOffset = Mathf.Lerp(bodyOffset, CalculateTargets(), lerpValue);
			ApplyBodyPosition(bodyOffset);
			for (int i = 0; i < targets.Length; i++)
			{
				ApplyIK(targets[i]);
			}
		}
	}

	private float CalculateTargets()
	{
		float num = 0f;
		for (int i = 0; i < targets.Length; i++)
		{
			targets[i].boneTargetDistance = ((i == 0) ? animator.leftFeetBottomHeight : animator.rightFeetBottomHeight);
			targets[i].ikEnabled = false;
			targets[i].rotationEnabled = false;
			targets[i].hintEnabled = false;
			Vector3 position = targets[i].position;
			if (!Physics.Raycast(animator.GetBoneTransform(targets[i].ikBone).position + new Vector3(0f, raycastDistance, 0f), Vector3.down, out targets[i].hit, 2f * raycastDistance, layerMask))
			{
				continue;
			}
			float y = targets[i].hit.point.y;
			if (GetRootPosition().y + groundedControlTolerance > animator.GetBoneTransform(targets[i].ikBone).position.y - targets[i].boneTargetDistance)
			{
				targets[i].position = targets[i].hit.point + new Vector3(0f, targets[i].boneTargetDistance, 0f);
				targets[i].rotation = Quaternion.FromToRotation(Vector3.up, targets[i].hit.normal) * animator.GetIKRotation(targets[i].ikGoal);
			}
			else
			{
				targets[i].position = targets[i].hit.point + new Vector3(0f, animator.GetBoneTransform(targets[i].ikBone).position.y - GetRootPosition().y, 0f);
				targets[i].rotation = animator.GetIKRotation(targets[i].ikGoal);
			}
			if (advancedForwardDetect)
			{
				Vector3 vector = Vector3.zero;
				if (position != Vector3.zero)
				{
					vector = targets[i].position - position;
				}
				float num2 = 180f;
				if (vector != Vector3.zero)
				{
					num2 = Vector3.Angle(vector, animator.velocity);
				}
				if (num2 < 90f)
				{
					if (Physics.Raycast(targets[i].position + animator.velocity.normalized * advancedForwardDetectionRange + new Vector3(0f, raycastDistance, 0f), Vector3.down, out targets[i].hit, 2f * raycastDistance, layerMask))
					{
						if (y < targets[i].hit.point.y && Vector3.Angle(targets[i].hit.normal, Vector3.up) < 10f)
						{
							targets[i].advancedForwardDetectionHeight = Mathf.Lerp(targets[i].advancedForwardDetectionHeight, targets[i].hit.point.y - y, lerpValue);
							targets[i].advancedForwardDetectionHeight = Mathf.Clamp01(targets[i].advancedForwardDetectionHeight);
							targets[i].position.y += targets[i].advancedForwardDetectionHeight;
						}
						else
						{
							targets[i].advancedForwardDetectionHeight = Mathf.Lerp(targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
						}
					}
					else
					{
						targets[i].advancedForwardDetectionHeight = Mathf.Lerp(targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
					}
				}
				else
				{
					targets[i].advancedForwardDetectionHeight = Mathf.Lerp(targets[i].advancedForwardDetectionHeight, 0f, lerpValue);
				}
			}
			targets[i].ikEnabled = true;
			targets[i].rotationEnabled = true;
			if (animator.GetBoneTransform(targets[i].ikBone).position.y - targets[i].position.y > num)
			{
				num = animator.GetBoneTransform(targets[i].ikBone).position.y - targets[i].position.y;
			}
			targets[i].hintOffset = Vector3.Distance(animator.GetBoneTransform(targets[i].ikBone).position, targets[i].position);
			targets[i].hintEnabled = true;
		}
		return num;
	}

	private Vector3 GetRootPosition()
	{
		if (animator != null)
		{
			return animator.rootPosition;
		}
		return base.transform.position;
	}

	private void ApplyBodyPosition(float maxVerticalIkDistance)
	{
		fixedVerticalBodyPositionOffset = Mathf.Clamp(fixedVerticalBodyPositionOffset, -0.05f, 0.05f);
		Vector3 vector = new Vector3(0f, 0f - maxVerticalIkDistance + fixedVerticalBodyPositionOffset, 0f);
		animator.bodyPosition += vector;
	}

	private void ApplyIK(IKTarget target)
	{
		if (animator == null)
		{
			return;
		}
		if (!target.ikEnabled)
		{
			animator.SetIKPositionWeight(target.ikGoal, 0f);
			animator.SetIKRotationWeight(target.ikGoal, 0f);
			animator.SetIKHintPositionWeight(target.ikHint, 0f);
			return;
		}
		animator.SetIKPosition(target.ikGoal, target.position);
		animator.SetIKPositionWeight(target.ikGoal, 1f);
		if (!target.rotationEnabled)
		{
			animator.SetIKRotationWeight(target.ikGoal, 0f);
		}
		else
		{
			animator.SetIKRotation(target.ikGoal, target.rotation);
			animator.SetIKRotationWeight(target.ikGoal, 1f);
		}
		if (!target.hintEnabled)
		{
			animator.SetIKHintPositionWeight(target.ikHint, 0f);
			return;
		}
		Vector3 position = base.gameObject.transform.InverseTransformPoint(animator.GetIKHintPosition(target.ikHint));
		position += new Vector3(0f, target.hintOffset, target.hintOffset);
		Vector3 hintPosition = base.gameObject.transform.TransformPoint(position);
		animator.SetIKHintPosition(target.ikHint, hintPosition);
		animator.SetIKHintPositionWeight(target.ikHint, 1f);
	}
}
