using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector
{
	[RequireComponent(typeof(BoxCollider))]
	[vClassHeader("SimpleTrigger", true, "icon_v2", false, "", openClose = false)]
	public class vSimpleTrigger : vMonoBehaviour
	{
		[Serializable]
		public class vTriggerEvent : UnityEvent<Collider>
		{
		}

		public bool useFilter = true;

		public List<string> tagsToDetect = new List<string> { "Player" };

		public LayerMask layerToDetect = 0;

		public vTriggerEvent onTriggerEnter;

		public vTriggerEvent onTriggerExit;

		[HideInInspector]
		public bool inCollision;

		private bool triggerStay;

		private Collider other;

		private void OnDrawGizmos()
		{
			Color color = new Color(1f, 0f, 0f, 0.2f);
			Color color2 = new Color(0f, 1f, 0f, 0.2f);
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
			Gizmos.color = ((inCollision && Application.isPlaying) ? color : color2);
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
		}

		private void Start()
		{
			inCollision = false;
			base.gameObject.GetComponent<BoxCollider>().isTrigger = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!useFilter || (tagsToDetect.Contains(other.gameObject.tag) && IsInLayerMask(other.gameObject, layerToDetect) && this.other == null))
			{
				inCollision = true;
				this.other = other;
				onTriggerEnter.Invoke(other);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (!useFilter || (tagsToDetect.Contains(other.gameObject.tag) && IsInLayerMask(other.gameObject, layerToDetect) && (this.other == null || this.other.gameObject == other.gameObject)))
			{
				inCollision = false;
				onTriggerExit.Invoke(other);
				this.other = null;
			}
		}

		private bool IsInLayerMask(GameObject obj, LayerMask mask)
		{
			return (mask.value & (1 << obj.layer)) > 0;
		}
	}
}
