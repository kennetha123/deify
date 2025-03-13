using System;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
	[vClassHeader("AI COMPANION", true, "icon_v2", false, "")]
	public class vAICompanion : vMonoBehaviour, vIAIComponent
	{
		public vHealthController friend;

		public string friendTag = "Player";

		public float maxFriendDistance;

		public float minFriendDistance;

		public bool forceFollow;

		internal vControlAI controlAI;

		protected vAICompanionControl controller;

		public Type ComponentType
		{
			get
			{
				return GetType();
			}
		}

		public bool friendIsFar
		{
			get
			{
				return friendDistance > maxFriendDistance;
			}
		}

		public bool friendIsDead
		{
			get
			{
				if ((bool)friend)
				{
					return friend.isDead;
				}
				return false;
			}
		}

		public float friendDistance
		{
			get
			{
				if (!friend)
				{
					return 0f;
				}
				return (friend.transform.position - base.transform.position).magnitude;
			}
		}

		protected void Start()
		{
			controlAI = GetComponent<vControlAI>();
			controlAI.onDead.AddListener(RemoveCompanion);
			if (!friend)
			{
				FindFriend();
			}
		}

		private void RemoveCompanion(GameObject arg0)
		{
			if (controller != null && controller.aICompanions.Contains(this))
			{
				controller.aICompanions.Remove(this);
			}
		}

		public void FindFriend()
		{
			vHealthController vHealthController = UnityEngine.Object.FindObjectsOfType<vHealthController>().vToList().Find((vHealthController p) => p.gameObject.CompareTag(friendTag));
			if ((bool)vHealthController)
			{
				friend = vHealthController;
				controller = friend.GetComponent<vAICompanionControl>();
				if ((bool)controller && !controller.aICompanions.Contains(this))
				{
					controller.aICompanions.Add(this);
				}
			}
		}

		public void GoToFriend()
		{
			if ((bool)friend && (bool)controlAI)
			{
				if (friendDistance > minFriendDistance)
				{
					controlAI.SetSpeed((!(friendDistance > minFriendDistance * 2f)) ? vAIMovementSpeed.Walking : vAIMovementSpeed.Running);
					controlAI.MoveTo(friend.transform.position);
				}
				else
				{
					controlAI.LookTo(friend.transform.position);
					controlAI.Stop();
				}
			}
		}
	}
}
