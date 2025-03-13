using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter
{
	[vClassHeader("Decal Manager", true, "icon_v2", false, "", openClose = false)]
	public class vDecalManager : vMonoBehaviour
	{
		[Serializable]
		public class DecalObject
		{
			public string tag;

			public GameObject hitEffect;

			public List<GameObject> decals;

			public GameObject GetDecal()
			{
				if (decals.Count > 1)
				{
					int index = UnityEngine.Random.Range(0, decals.Count - 1);
					return decals[index];
				}
				if (decals.Count == 1)
				{
					return decals[0];
				}
				return null;
			}
		}

		public LayerMask layermask;

		public List<DecalObject> decalObjects;

		public virtual void CreateDecal(RaycastHit hitInfo)
		{
			CreateDecal(hitInfo.collider.gameObject, hitInfo.point, hitInfo.normal);
		}

		public virtual void CreateDecal(GameObject target, Vector3 position, Vector3 normal)
		{
			if ((int)layermask != ((int)layermask | (1 << target.layer)))
			{
				return;
			}
			DecalObject decalObject = decalObjects.Find((DecalObject d) => d.tag.Equals(target.tag));
			RaycastHit hitInfo;
			if (decalObject != null && Physics.SphereCast(new Ray(position + normal * 0.1f, -normal), 0.0001f, out hitInfo, 1f, layermask) && hitInfo.collider.gameObject == target)
			{
				Quaternion rotation = Quaternion.LookRotation(hitInfo.normal, Vector3.up);
				if (decalObject.hitEffect != null)
				{
					UnityEngine.Object.Instantiate(decalObject.hitEffect, hitInfo.point, rotation).transform.SetParent(vObjectContainer.root, true);
				}
				GameObject decal = decalObject.GetDecal();
				if (decal != null)
				{
					UnityEngine.Object.Instantiate(decal, hitInfo.point, rotation).transform.SetParent(target.gameObject.transform, true);
				}
			}
		}
	}
}
