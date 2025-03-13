using System;
using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter
{
	[vClassHeader("Projectile Control", "The damage value is changed from minDamage, maxDamage, DropOffStart, DropOffEnd of the ShooterWeapon", openClose = false)]
	public class vProjectileControl : vMonoBehaviour
	{
		[Serializable]
		public class ProjectileCastColliderEvent : UnityEvent<RaycastHit>
		{
		}

		[Serializable]
		public class ProjectilePassDamage : UnityEvent<vDamage>
		{
		}

		public vBulletLifeSettings bulletLifeSettings;

		public int bulletLife = 100;

		public bool debugTrajetory;

		public bool debugHittedObject;

		public vDamage damage;

		public float forceMultiplier = 1f;

		public bool destroyOnCast = true;

		public ProjectilePassDamage onPassDamage;

		public ProjectileCastColliderEvent onCastCollider;

		public ProjectileCastColliderEvent onDestroyProjectile;

		[HideInInspector]
		public bool damageByDistance;

		[HideInInspector]
		public int minDamage;

		[HideInInspector]
		public int maxDamage;

		[HideInInspector]
		public float DropOffStart = 8f;

		[HideInInspector]
		public float velocity = 580f;

		[HideInInspector]
		public float DropOffEnd = 50f;

		[HideInInspector]
		public Vector3 startPosition;

		[HideInInspector]
		public LayerMask hitLayer = -1;

		[HideInInspector]
		public List<string> ignoreTags;

		[HideInInspector]
		public Transform shooterTransform;

		protected Vector3 previousPosition;

		protected Rigidbody _rigidBody;

		protected Color debugColor = Color.green;

		private int debugLife;

		private float castDist;

		protected virtual void Start()
		{
			base.transform.SetParent(vObjectContainer.root, true);
			debugLife = bulletLife;
			_rigidBody = GetComponent<Rigidbody>();
			startPosition = base.transform.position;
			previousPosition = base.transform.position - base.transform.forward * 0.1f;
		}

		protected virtual void Update()
		{
			if (_rigidBody.linearVelocity.magnitude > 1f)
			{
				base.transform.rotation = Quaternion.LookRotation(_rigidBody.linearVelocity.normalized, base.transform.up);
			}
			RaycastHit hitInfo;
			if (Physics.Linecast(previousPosition, base.transform.position + base.transform.forward * 0.5f, out hitInfo, hitLayer))
			{
				if (!hitInfo.collider)
				{
					return;
				}
				float num = Vector3.Distance(startPosition, base.transform.position) + castDist;
				if (!ignoreTags.Contains(hitInfo.collider.gameObject.tag) && (!(shooterTransform != null) || !hitInfo.collider.transform.IsChildOf(shooterTransform)))
				{
					if (debugHittedObject)
					{
						Debug.Log(hitInfo.collider.gameObject.name, hitInfo.collider);
					}
					onCastCollider.Invoke(hitInfo);
					damage.damageValue = maxDamage;
					if (damageByDistance)
					{
						float num2 = 0f;
						int num3 = maxDamage - minDamage;
						if (num - DropOffStart >= 0f)
						{
							num2 = Mathf.Clamp((float)(int)Math.Round((double)(100f * (num - DropOffStart)) / (double)(DropOffEnd - DropOffStart)) * 0.01f, 0f, 1f);
							damage.damageValue = maxDamage - (int)((float)num3 * num2);
						}
						else
						{
							damage.damageValue = maxDamage;
						}
					}
					damage.hitPosition = hitInfo.point;
					damage.receiver = hitInfo.collider.transform;
					if (damage.damageValue > 0)
					{
						onPassDamage.Invoke(damage);
						hitInfo.collider.gameObject.ApplyDamage(damage, damage.sender.GetComponent<vIMeleeFighter>());
					}
					Rigidbody component = hitInfo.collider.gameObject.GetComponent<Rigidbody>();
					if ((bool)component && !hitInfo.collider.gameObject.isStatic)
					{
						component.AddForce(base.transform.forward * damage.damageValue * forceMultiplier, ForceMode.Impulse);
					}
					base.transform.position = hitInfo.point + base.transform.forward * 0.02f;
					startPosition = base.transform.position;
					castDist = num;
					if (debugTrajetory)
					{
						Debug.DrawLine(base.transform.position, previousPosition, debugColor, 10f);
					}
					if (destroyOnCast)
					{
						if ((bool)bulletLifeSettings)
						{
							vBulletLifeSettings.vBulletLifeInfo reduceLife = bulletLifeSettings.GetReduceLife(hitInfo.collider.gameObject.tag, hitInfo.collider.gameObject.layer);
							bulletLife -= reduceLife.lostLife;
							if (debugTrajetory)
							{
								DrawHitPoint(hitInfo.point);
							}
							bool flag = false;
							if (bulletLife > 0 && !reduceLife.ricochet)
							{
								for (float num4 = 0f; num4 <= reduceLife.maxThicknessToCross; num4 += 0.01f)
								{
									Vector3 point = base.transform.position + base.transform.forward * num4;
									if (!hitInfo.collider.bounds.Contains(point))
									{
										hitInfo.point = point;
										hitInfo.normal = base.transform.forward;
										onCastCollider.Invoke(hitInfo);
										flag = true;
										break;
									}
								}
							}
							if (!flag && !reduceLife.ricochet)
							{
								bulletLife = 0;
								base.transform.position = hitInfo.point;
								onDestroyProjectile.Invoke(hitInfo);
								UnityEngine.Object.Destroy(base.gameObject);
							}
							maxDamage -= maxDamage - maxDamage * reduceLife.lostDamage / 100;
							minDamage -= minDamage - minDamage * reduceLife.lostDamage / 100;
							if (maxDamage < 0)
							{
								maxDamage = 0;
							}
							if (minDamage < 0)
							{
								minDamage = 0;
							}
							float num5 = UnityEngine.Random.Range(reduceLife.minChangeTrajectory, reduceLife.maxChangeTrajectory) * (float)((UnityEngine.Random.Range(-1, 1) >= 0) ? 1 : (-1));
							float num6 = UnityEngine.Random.Range(reduceLife.minChangeTrajectory, reduceLife.maxChangeTrajectory) * (float)((UnityEngine.Random.Range(-1, 1) >= 0) ? 1 : (-1));
							if (num6 > 60f || num6 < -60f)
							{
								num5 = Mathf.Clamp(num5, -15f, 15f);
							}
							if (num5 != 0f || num6 != 0f)
							{
								Vector3 vector = Quaternion.Euler(num5, num6, 0f) * _rigidBody.linearVelocity;
								if (vector != Vector3.zero)
								{
									_rigidBody.linearVelocity = vector * ((!reduceLife.ricochet) ? 1 : (-1));
									base.transform.forward = vector * ((!reduceLife.ricochet) ? 1 : (-1));
								}
							}
							if (debugTrajetory)
							{
								float num7 = (float)bulletLife / (float)debugLife * 100f;
								debugColor = ((num7 > 76f) ? Color.green : ((num7 > 51f) ? Color.yellow : ((num7 > 26f) ? new Color(1f, 0.5f, 0f) : Color.red)));
								debugColor.a = 0.5f;
							}
						}
						if (bulletLife <= 0 || bulletLifeSettings == null)
						{
							base.transform.position = hitInfo.point;
							onDestroyProjectile.Invoke(hitInfo);
							UnityEngine.Object.Destroy(base.gameObject);
						}
					}
				}
				else
				{
					base.transform.position = hitInfo.point + base.transform.forward * 0.02f;
					if (debugTrajetory)
					{
						Debug.DrawLine(base.transform.position, previousPosition, debugColor, 10f);
					}
				}
			}
			else if (debugTrajetory)
			{
				Debug.DrawLine(base.transform.position, previousPosition, debugColor, 10f);
			}
			previousPosition = base.transform.position;
		}

		private void DrawHitPoint(Vector3 point)
		{
			Debug.DrawRay(point, -base.transform.forward * 0.1f, Color.red, 10f);
			Debug.DrawRay(point, base.transform.right * 0.1f, Color.red, 10f);
			Debug.DrawRay(point, -base.transform.right * 0.1f, Color.red, 10f);
			Debug.DrawRay(point, base.transform.up * 0.1f, Color.red, 10f);
			Debug.DrawRay(point, -base.transform.up * 0.1f, Color.red, 10f);
		}

		public void RemoveParentOfOther(Transform other)
		{
			other.SetParent(vObjectContainer.root, true);
		}
	}
}
