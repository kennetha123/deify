using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController
{
	[vClassHeader("MoveSet Speed", "Use this to add extra speed into a specific MoveSet")]
	public class vMoveSetSpeed : vMonoBehaviour
	{
		[Serializable]
		public class vMoveSetControlSpeed
		{
			public int moveset;

			public float walkSpeed = 1.5f;

			public float runningSpeed = 1.5f;

			public float sprintSpeed = 1.5f;

			public float crouchSpeed = 1.5f;
		}

		private vThirdPersonMotor cc;

		private vMoveSetControlSpeed defaultFree = new vMoveSetControlSpeed();

		private vMoveSetControlSpeed defaultStrafe = new vMoveSetControlSpeed();

		public List<vMoveSetControlSpeed> listFree;

		public List<vMoveSetControlSpeed> listStrafe;

		private int currentMoveSet;

		private void Start()
		{
			cc = GetComponent<vThirdPersonMotor>();
			defaultFree.walkSpeed = cc.freeSpeed.walkSpeed;
			defaultFree.runningSpeed = cc.freeSpeed.runningSpeed;
			defaultFree.sprintSpeed = cc.freeSpeed.sprintSpeed;
			defaultStrafe.walkSpeed = cc.strafeSpeed.walkSpeed;
			defaultStrafe.runningSpeed = cc.strafeSpeed.runningSpeed;
			defaultStrafe.sprintSpeed = cc.strafeSpeed.sprintSpeed;
			StartCoroutine(UpdateMoveSetSpeed());
		}

		private IEnumerator UpdateMoveSetSpeed()
		{
			while (true)
			{
				yield return new WaitForSeconds(0.1f);
				ChangeSpeed();
			}
		}

		private void ChangeSpeed()
		{
			currentMoveSet = (int)Mathf.Round(cc.animator.GetFloat("MoveSet_ID"));
			if (cc.isStrafing)
			{
				vMoveSetControlSpeed vMoveSetControlSpeed = listStrafe.Find((vMoveSetControlSpeed l) => l.moveset == currentMoveSet);
				if (vMoveSetControlSpeed != null)
				{
					cc.freeSpeed.walkSpeed = vMoveSetControlSpeed.walkSpeed;
					cc.freeSpeed.runningSpeed = vMoveSetControlSpeed.runningSpeed;
					cc.freeSpeed.sprintSpeed = vMoveSetControlSpeed.sprintSpeed;
					cc.freeSpeed.crouchSpeed = vMoveSetControlSpeed.crouchSpeed;
				}
				else
				{
					cc.strafeSpeed.walkSpeed = defaultStrafe.walkSpeed;
					cc.strafeSpeed.runningSpeed = defaultStrafe.runningSpeed;
					cc.strafeSpeed.sprintSpeed = defaultStrafe.sprintSpeed;
					cc.strafeSpeed.crouchSpeed = defaultStrafe.crouchSpeed;
				}
			}
			else
			{
				vMoveSetControlSpeed vMoveSetControlSpeed2 = listFree.Find((vMoveSetControlSpeed l) => l.moveset == currentMoveSet);
				if (vMoveSetControlSpeed2 != null)
				{
					cc.freeSpeed.walkSpeed = vMoveSetControlSpeed2.walkSpeed;
					cc.freeSpeed.runningSpeed = vMoveSetControlSpeed2.runningSpeed;
					cc.freeSpeed.sprintSpeed = vMoveSetControlSpeed2.sprintSpeed;
					cc.freeSpeed.crouchSpeed = vMoveSetControlSpeed2.crouchSpeed;
				}
				else
				{
					cc.strafeSpeed.walkSpeed = defaultFree.walkSpeed;
					cc.strafeSpeed.runningSpeed = defaultFree.runningSpeed;
					cc.strafeSpeed.sprintSpeed = defaultFree.sprintSpeed;
					cc.strafeSpeed.crouchSpeed = defaultFree.crouchSpeed;
				}
			}
		}
	}
}
