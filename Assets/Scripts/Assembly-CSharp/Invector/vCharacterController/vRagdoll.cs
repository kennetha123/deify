using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController
{
	[vClassHeader("RAGDOLL SYSTEM", true, "ragdollIcon", true, "Every gameobject children of the character must have their tag added in the IgnoreTag List.")]
	public class vRagdoll : vMonoBehaviour
	{
		private enum RagdollState
		{
			animated = 0,
			ragdolled = 1,
			blendToAnim = 2
		}

		private class BodyPart
		{
			public Transform transform;

			public Rigidbody rigidbody;

			public Collider collider;

			public Vector3 storedPosition;

			public Quaternion storedRotation;

			public BodyPart(Transform t)
			{
				transform = t;
				rigidbody = t.GetComponent<Rigidbody>();
				collider = t.GetComponent<Collider>();
			}
		}

		[vButton("Active Ragdoll", "ActivateRagdoll", typeof(vRagdoll), true)]
		public bool removePhysicsAfterDie;

		[Tooltip("SHOOTER: Keep false to use detection hit on each children collider, don't forget to change the layer to BodyPart from hips to all childrens. MELEE: Keep true to only hit the main Capsule Collider.")]
		public bool disableColliders;

		public AudioSource collisionSource;

		public AudioClip collisionClip;

		[Header("Add Tags for Weapons or Itens here:")]
		public List<string> ignoreTags = new List<string> { "Weapon", "Ignore Ragdoll" };

		public AnimatorStateInfo stateInfo;

		internal vICharacter iChar;

		private Animator animator;

		internal Transform characterChest;

		internal Transform characterHips;

		[NonSerialized]
		public bool isActive;

		private bool inStabilize;

		private bool updateBehaviour;

		private RagdollState state;

		private float ragdollToMecanimBlendTime = 0.5f;

		private float mecanimToGetUpTransitionTime = 0.05f;

		private float ragdollingEndTime = -100f;

		private Vector3 ragdolledHipPosition;

		private Vector3 ragdolledHeadPosition;

		private Vector3 ragdolledFeetPosition;

		private List<BodyPart> bodyParts = new List<BodyPart>();

		private Transform hipsParent;

		private bool inApplyDamage;

		private GameObject _ragdollContainer;

		private bool ragdolled
		{
			get
			{
				return state != RagdollState.animated;
			}
			set
			{
				if (value)
				{
					if (state == RagdollState.animated)
					{
						setKinematic(false);
						setCollider(false);
						animator.enabled = false;
						state = RagdollState.ragdolled;
					}
					return;
				}
				characterHips.parent = hipsParent;
				isActive = false;
				if (state != RagdollState.ragdolled)
				{
					return;
				}
				setKinematic(true);
				setCollider(true);
				ragdollingEndTime = Time.time;
				animator.enabled = true;
				state = RagdollState.blendToAnim;
				foreach (BodyPart bodyPart in bodyParts)
				{
					bodyPart.storedRotation = bodyPart.transform.rotation;
					bodyPart.storedPosition = bodyPart.transform.position;
				}
				ragdolledFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftToes).position + animator.GetBoneTransform(HumanBodyBones.RightToes).position);
				ragdolledHeadPosition = animator.GetBoneTransform(HumanBodyBones.Head).position;
				ragdolledHipPosition = animator.GetBoneTransform(HumanBodyBones.Hips).position;
				if (animator.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0f)
				{
					animator.Play("StandUp@FromBack");
				}
				else
				{
					animator.Play("StandUp@FromBelly");
				}
			}
		}

		private void Start()
		{
			animator = GetComponent<Animator>();
			iChar = GetComponent<vICharacter>();
			if (iChar != null)
			{
				iChar.onActiveRagdoll.AddListener(ActivateRagdoll);
			}
			characterChest = animator.GetBoneTransform(HumanBodyBones.Chest);
			characterHips = animator.GetBoneTransform(HumanBodyBones.Hips);
			hipsParent = characterHips.parent;
			CreateRagdollContainer();
			if (!characterHips)
			{
				return;
			}
			Component[] componentsInChildren = characterHips.GetComponentsInChildren(typeof(Transform));
			bodyParts.Add(new BodyPart(characterHips));
			Component[] array = componentsInChildren;
			foreach (Component component in array)
			{
				if (ignoreTags.Contains(component.tag) || !component)
				{
					continue;
				}
				Transform transform = component as Transform;
				if (transform != base.transform && (bool)transform.GetComponent<Rigidbody>())
				{
					BodyPart bodyPart = new BodyPart(transform);
					if (bodyPart.rigidbody != null)
					{
						bodyPart.rigidbody.isKinematic = true;
						component.tag = base.gameObject.tag;
					}
					bodyParts.Add(bodyPart);
				}
			}
			setKinematic(true);
			setCollider(true);
		}

		private void CreateRagdollContainer()
		{
			if (!_ragdollContainer)
			{
				_ragdollContainer = new GameObject("RagdollContainer " + base.gameObject.name);
			}
			_ragdollContainer.hideFlags = HideFlags.HideInHierarchy;
		}

		private void LateUpdate()
		{
			if (!(animator == null) && (updateBehaviour || animator.updateMode != AnimatorUpdateMode.Fixed))
			{
				updateBehaviour = false;
				RagdollBehaviour();
			}
		}

		private void FixedUpdate()
		{
			updateBehaviour = true;
			if (isActive && iChar.currentHealth > 0f)
			{
				if (!_ragdollContainer)
				{
					CreateRagdollContainer();
				}
				if (characterHips.parent != _ragdollContainer.transform)
				{
					characterHips.SetParent(_ragdollContainer.transform);
				}
				if (ragdolled && !inStabilize)
				{
					ragdolled = false;
					StartCoroutine(ResetPlayer(1f));
				}
				else if ((animator != null && !animator.isActiveAndEnabled && ragdolled) || (animator == null && ragdolled))
				{
					base.transform.position = characterHips.position;
				}
			}
		}

		private void OnDestroy()
		{
			try
			{
				if ((bool)_ragdollContainer && (bool)characterHips && characterHips.parent == _ragdollContainer.transform)
				{
					characterHips.SetParent(hipsParent);
					UnityEngine.Object.Destroy(_ragdollContainer.gameObject);
				}
			}
			catch (UnityException ex)
			{
				Debug.LogWarning(ex.Message, base.gameObject);
			}
		}

		private void ResetDamage()
		{
			inApplyDamage = false;
		}

		public void ApplyDamage(vDamage damage)
		{
			if (isActive && ragdolled && !inApplyDamage && iChar != null)
			{
				inApplyDamage = true;
				iChar.TakeDamage(damage);
				Invoke("ResetDamage", 0.2f);
			}
		}

		public void ActivateRagdoll()
		{
			ActivateRagdoll(null);
		}

		public void ActivateRagdoll(vDamage damage)
		{
			if (!isActive && (damage == null || damage.activeRagdoll))
			{
				if (!_ragdollContainer)
				{
					CreateRagdollContainer();
				}
				inApplyDamage = true;
				isActive = true;
				if (base.transform.parent != null && !base.transform.parent.gameObject.isStatic)
				{
					base.transform.parent = null;
				}
				bool flag = true;
				if (iChar != null)
				{
					iChar.EnableRagdoll();
					flag = !(iChar.currentHealth > 0f);
				}
				inStabilize = true;
				ragdolled = true;
				StartCoroutine(RagdollStabilizer(2f));
				if (!flag)
				{
					characterHips.SetParent(_ragdollContainer.transform);
				}
				Invoke("ResetDamage", 0.2f);
			}
		}

		public void OnRagdollCollisionEnter(vRagdollCollision ragdolCollision)
		{
			if (ragdolCollision.ImpactForce > 1f && (bool)collisionSource)
			{
				collisionSource.clip = collisionClip;
				collisionSource.volume = ragdolCollision.ImpactForce * 0.05f;
				if (!collisionSource.isPlaying)
				{
					collisionSource.Play();
				}
			}
		}

		private IEnumerator RagdollStabilizer(float delay)
		{
			float rdStabilize = float.PositiveInfinity;
			yield return new WaitForSeconds(delay);
			while (rdStabilize > ((iChar != null && iChar.isDead) ? 0.0001f : 0.1f) && animator != null && !animator.isActiveAndEnabled)
			{
				rdStabilize = characterChest.GetComponent<Rigidbody>().linearVelocity.magnitude;
				yield return new WaitForEndOfFrame();
			}
			if (iChar != null && iChar.isDead)
			{
				yield return new WaitForEndOfFrame();
				DestroyComponents();
			}
			inStabilize = false;
		}

		private IEnumerator ResetPlayer(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			if (iChar != null)
			{
				iChar.ResetRagdoll();
			}
		}

		private void RagdollBehaviour()
		{
			if (iChar == null || !(iChar.currentHealth > 0f) || iChar == null || !iChar.ragdolled || state != RagdollState.blendToAnim)
			{
				return;
			}
			if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
			{
				Vector3 vector = ragdolledHipPosition - animator.GetBoneTransform(HumanBodyBones.Hips).position;
				Vector3 vector2 = base.transform.position + vector;
				RaycastHit[] array = Physics.RaycastAll(new Ray(vector2 + Vector3.up, Vector3.down));
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit raycastHit = array[i];
					if (!raycastHit.transform.IsChildOf(base.transform))
					{
						vector2.y = Mathf.Max(vector2.y, raycastHit.point.y);
					}
				}
				base.transform.position = vector2;
				Vector3 vector3 = ragdolledHeadPosition - ragdolledFeetPosition;
				vector3.y = 0f;
				Vector3 vector4 = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
				Vector3 vector5 = animator.GetBoneTransform(HumanBodyBones.Head).position - vector4;
				vector5.y = 0f;
				base.transform.rotation *= Quaternion.FromToRotation(vector5.normalized, vector3.normalized);
			}
			float value = 1f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
			value = Mathf.Clamp01(value);
			foreach (BodyPart bodyPart in bodyParts)
			{
				if (bodyPart.transform != base.transform)
				{
					if (bodyPart.transform == animator.GetBoneTransform(HumanBodyBones.Hips))
					{
						bodyPart.transform.position = Vector3.Lerp(bodyPart.transform.position, bodyPart.storedPosition, value);
					}
					bodyPart.transform.rotation = Quaternion.Slerp(bodyPart.transform.rotation, bodyPart.storedRotation, value);
				}
			}
			if (value == 0f)
			{
				state = RagdollState.animated;
			}
		}

		private void setKinematic(bool newValue)
		{
			foreach (BodyPart bodyPart in bodyParts)
			{
				if (!ignoreTags.Contains(bodyPart.transform.tag) && (bool)bodyPart.rigidbody)
				{
					bodyPart.rigidbody.isKinematic = newValue;
				}
			}
		}

		private void setCollider(bool newValue)
		{
			foreach (BodyPart bodyPart in bodyParts)
			{
				if (!ignoreTags.Contains(bodyPart.transform.tag) && !bodyPart.transform.Equals(base.transform) && (bool)bodyPart.collider)
				{
					if (disableColliders)
					{
						bodyPart.collider.enabled = !newValue;
					}
					else
					{
						bodyPart.collider.isTrigger = newValue;
					}
				}
			}
		}

		private void DestroyComponents()
		{
			if (!removePhysicsAfterDie)
			{
				return;
			}
			MonoBehaviour[] componentsInChildren = GetComponentsInChildren<MonoBehaviour>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].transform != base.transform)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i]);
				}
			}
			CharacterJoint[] componentsInChildren2 = GetComponentsInChildren<CharacterJoint>();
			if (componentsInChildren2 != null)
			{
				CharacterJoint[] array = componentsInChildren2;
				foreach (CharacterJoint characterJoint in array)
				{
					if (!ignoreTags.Contains(characterJoint.gameObject.tag) && characterJoint.transform != base.transform)
					{
						UnityEngine.Object.Destroy(characterJoint);
					}
				}
			}
			Rigidbody[] componentsInChildren3 = GetComponentsInChildren<Rigidbody>();
			if (componentsInChildren3 != null)
			{
				Rigidbody[] array2 = componentsInChildren3;
				foreach (Rigidbody rigidbody in array2)
				{
					if (!ignoreTags.Contains(rigidbody.gameObject.tag) && rigidbody.transform != base.transform)
					{
						UnityEngine.Object.Destroy(rigidbody);
					}
				}
			}
			Collider[] componentsInChildren4 = GetComponentsInChildren<Collider>();
			if (componentsInChildren4 == null)
			{
				return;
			}
			Collider[] array3 = componentsInChildren4;
			foreach (Collider collider in array3)
			{
				if (!ignoreTags.Contains(collider.gameObject.tag) && collider.transform != base.transform)
				{
					UnityEngine.Object.Destroy(collider);
				}
			}
		}
	}
}
