using UnityEngine;

namespace Invector
{
	public class vAnimatorSetInt : vAnimatorSetValue<int>
	{
		[vHelpBox("Random Value between Default Value and Max Value", vHelpBoxAttribute.MessageType.None)]
		public bool randomEnter;

		[vHideInInspector("randomEnter", false)]
		public int maxEnterValue;

		public bool randomExit;

		[vHideInInspector("randomExit", false)]
		public int maxExitValue;

		protected override int GetEnterValue()
		{
			if (!randomEnter)
			{
				return base.GetEnterValue();
			}
			return Random.Range(base.GetEnterValue(), maxEnterValue);
		}

		protected override int GetExitValue()
		{
			if (!randomExit)
			{
				return base.GetExitValue();
			}
			return Random.Range(base.GetExitValue(), maxExitValue);
		}
	}
}
