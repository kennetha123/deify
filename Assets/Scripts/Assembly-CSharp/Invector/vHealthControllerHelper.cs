using UnityEngine;

namespace Invector
{
	public static class vHealthControllerHelper
	{
		private static vIHealthController GetHealthController(this GameObject gameObject)
		{
			return gameObject.GetComponent<vIHealthController>();
		}

		public static bool HasHealth(this GameObject gameObject)
		{
			return gameObject.GetHealthController() != null;
		}

		public static bool IsDead(this GameObject gameObject)
		{
			vIHealthController healthController = gameObject.GetHealthController();
			if (healthController != null)
			{
				return healthController.isDead;
			}
			return true;
		}
	}
}
