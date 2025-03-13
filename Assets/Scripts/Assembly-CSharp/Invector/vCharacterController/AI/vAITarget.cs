using System;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
	[Serializable]
	public class vAITarget : vAISimpleTarget
	{
		public vIHealthController healthController;

		public vIControlAICombat combateController;

		public vIMeleeFighter meleeFighter;

		public vICharacter character;

		public bool isFixedTarget = true;

		[HideInInspector]
		public bool isLost;

		[HideInInspector]
		public bool _hadHealthController;

		public bool hasCollider
		{
			get
			{
				return base.collider != null;
			}
		}

		public bool hasHealthController
		{
			get
			{
				if (_hadHealthController && healthController == null)
				{
					base.transform = null;
				}
				return healthController != null;
			}
		}

		public bool isDead
		{
			get
			{
				bool result = true;
				if (hasHealthController)
				{
					result = healthController.isDead;
				}
				else if (_hadHealthController)
				{
					result = true;
				}
				else if (!base.transform.gameObject.activeInHierarchy)
				{
					result = true;
				}
				else if ((bool)_collider)
				{
					result = !_collider.enabled;
				}
				return result;
			}
		}

		public bool isArmed
		{
			get
			{
				if (!isFighter)
				{
					return false;
				}
				if (meleeFighter == null)
				{
					if (combateController == null)
					{
						return false;
					}
					return combateController.isArmed;
				}
				return meleeFighter.isArmed;
			}
		}

		public bool isBlocking
		{
			get
			{
				if (!isFighter)
				{
					return false;
				}
				if (meleeFighter == null)
				{
					if (combateController == null)
					{
						return false;
					}
					return combateController.isBlocking;
				}
				return meleeFighter.isBlocking;
			}
		}

		public bool isAttacking
		{
			get
			{
				if (!isFighter)
				{
					return false;
				}
				if (meleeFighter == null)
				{
					if (combateController == null)
					{
						return false;
					}
					return combateController.isAttacking;
				}
				return meleeFighter.isAttacking;
			}
		}

		public bool isFighter
		{
			get
			{
				if (meleeFighter == null)
				{
					return combateController != null;
				}
				return true;
			}
		}

		public bool isCharacter
		{
			get
			{
				return character != null;
			}
		}

		public float currentHealth
		{
			get
			{
				if (hasHealthController)
				{
					return healthController.currentHealth;
				}
				return 0f;
			}
		}

		public override void InitTarget(Transform target)
		{
			base.InitTarget(target);
			if ((bool)target)
			{
				healthController = target.GetComponent<vIHealthController>();
				_hadHealthController = healthController != null;
				meleeFighter = target.GetComponent<vIMeleeFighter>();
				character = target.GetComponent<vICharacter>();
				combateController = target.GetComponent<vIControlAICombat>();
			}
		}

		public override void ClearTarget()
		{
			base.ClearTarget();
			healthController = null;
			meleeFighter = null;
			character = null;
			combateController = null;
		}
	}
}
