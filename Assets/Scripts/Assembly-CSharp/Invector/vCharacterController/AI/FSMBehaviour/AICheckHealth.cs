namespace Invector.vCharacterController.AI.FSMBehaviour
{
	public class AICheckHealth : vStateDecision
	{
		public enum vCheckValue
		{
			Equals = 0,
			Less = 1,
			Greater = 2,
			NoEqual = 3
		}

		public vCheckValue checkValue = vCheckValue.NoEqual;

		public float value;

		public override string categoryName
		{
			get
			{
				return "";
			}
		}

		public override string defaultName
		{
			get
			{
				return "Check Health";
			}
		}

		public override bool Decide(vIFSMBehaviourController fsmBehaviour)
		{
			return CheckValue(fsmBehaviour);
		}

		protected virtual bool CheckValue(vIFSMBehaviourController fsmBehaviour)
		{
			if (fsmBehaviour == null)
			{
				return false;
			}
			float num = fsmBehaviour.aiController.currentHealth / (float)fsmBehaviour.aiController.MaxHealth * 100f;
			switch (checkValue)
			{
			case vCheckValue.Equals:
				return num == value;
			case vCheckValue.Less:
				return num < value;
			case vCheckValue.Greater:
				return num > value;
			case vCheckValue.NoEqual:
				return num != value;
			default:
				return false;
			}
		}
	}
}
