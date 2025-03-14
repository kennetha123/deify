using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
	[Serializable]
	public class vThirdPersonCameraListData : ScriptableObject
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public List<vThirdPersonCameraState> tpCameraStates;

		public vThirdPersonCameraListData()
		{
			tpCameraStates = new List<vThirdPersonCameraState>();
			tpCameraStates.Add(new vThirdPersonCameraState("Default"));
		}
	}
}
