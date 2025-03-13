using Invector.vCharacterController.TopDownShooter;
using UnityEngine;

namespace Invector.vShooter
{
	public class vShooterTopDownCursor : MonoBehaviour
	{
		private vTopDownShooterInput shooter;

		public GameObject sprite;

		public LineRenderer lineRender;

		private void Start()
		{
			shooter = Object.FindObjectOfType<vTopDownShooterInput>();
		}

		private void FixedUpdate()
		{
			if (!shooter)
			{
				return;
			}
			if ((bool)sprite)
			{
				if (shooter.isAiming && !sprite.gameObject.activeSelf)
				{
					sprite.gameObject.SetActive(true);
				}
				else if (!shooter.isAiming && sprite.gameObject.activeSelf)
				{
					sprite.gameObject.SetActive(false);
				}
			}
			base.transform.position = shooter.aimPosition;
			Vector3 vector = shooter.transform.position - shooter.aimPosition;
			vector.y = 0f;
			if (!(vector != Vector3.zero))
			{
				return;
			}
			base.transform.rotation = Quaternion.LookRotation(vector);
			if ((bool)lineRender)
			{
				lineRender.SetPosition(0, shooter.topDownController.lookPos);
				lineRender.SetPosition(1, shooter.aimPosition);
				if (shooter.isAiming && !lineRender.enabled)
				{
					lineRender.enabled = true;
				}
				else if (!shooter.isAiming && lineRender.enabled)
				{
					lineRender.enabled = false;
				}
			}
		}
	}
}
