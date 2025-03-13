using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.ClickToMove
{
	[vClassHeader("CLICK TO MOVE INPUT", true, "icon_v2", false, "")]
	public class vClickToMoveInput : vThirdPersonInput
	{
		[Serializable]
		public class vCursorByTag
		{
			public string tag;

			public Texture2D cursorTexture;

			public CursorMode cursorMode;
		}

		[Serializable]
		public class vOnEnableCursor : UnityEvent<Vector3>
		{
		}

		[vEditorToolbar("Cursor", false, "", false, false)]
		public List<vCursorByTag> cursorByTag;

		[vEditorToolbar("Layer", false, "", false, false)]
		[Header("Click To Move Properties")]
		public LayerMask clickMoveLayer = 1;

		[vEditorToolbar("Events", false, "", false, false)]
		public vOnEnableCursor onEnableCursor = new vOnEnableCursor();

		public UnityEvent onDisableCursor;

		[HideInInspector]
		public Vector3 cursorPoint;

		public Dictionary<string, vCursorByTag> customCursor;

		public Collider target { get; set; }

		protected override void Start()
		{
			base.Start();
			customCursor = new Dictionary<string, vCursorByTag>();
			for (int i = 0; i < cursorByTag.Count; i++)
			{
				if (!customCursor.ContainsKey(cursorByTag[i].tag))
				{
					customCursor.Add(cursorByTag[i].tag, cursorByTag[i]);
				}
			}
		}

		protected override IEnumerator CharacterInit()
		{
			yield return StartCoroutine(_003C_003En__0());
			cursorPoint = base.transform.position;
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			MoveToPoint();
		}

		protected override void MoveCharacter()
		{
			cc.rotateByWorld = true;
			ClickAndMove();
		}

		protected virtual void ClickAndMove()
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.PositiveInfinity, clickMoveLayer))
			{
				string text = hitInfo.collider.gameObject.tag;
				ChangeCursorByTag(text);
				CheckClickPoint(hitInfo);
			}
		}

		protected virtual void CheckClickPoint(RaycastHit hit)
		{
			if (Input.GetMouseButton(0))
			{
				if (Input.GetMouseButtonDown(0))
				{
					target = hit.collider;
				}
				if (onEnableCursor != null)
				{
					onEnableCursor.Invoke(hit.point);
				}
				cursorPoint = hit.point;
			}
		}

		protected virtual void ChangeCursorByTag(string tag)
		{
			if (customCursor.Count > 0)
			{
				if (customCursor.ContainsKey(tag))
				{
					Cursor.SetCursor(customCursor[tag].cursorTexture, Vector2.zero, customCursor[tag].cursorMode);
				}
				else
				{
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				}
			}
		}

		protected void MoveToPoint()
		{
			if (!NearPoint(cursorPoint, base.transform.position) && (bool)target)
			{
				MoveCharacter(cursorPoint);
				return;
			}
			if (onDisableCursor != null)
			{
				onDisableCursor.Invoke();
			}
			cc.input = Vector2.Lerp(cc.input, Vector3.zero, 20f * Time.deltaTime);
		}

		public void SetTargetPosition(Vector3 value)
		{
			cursorPoint = value;
			Vector3 normalized = (value - base.transform.position).normalized;
			cc.input = new Vector2(normalized.x, normalized.z);
		}

		public void ClearTarget()
		{
			cc.input = Vector2.zero;
			target = null;
		}

		protected virtual bool NearPoint(Vector3 a, Vector3 b)
		{
			Vector3 a2 = new Vector3(a.x, base.transform.position.y, a.z);
			Vector3 b2 = new Vector3(b.x, base.transform.position.y, b.z);
			return Vector3.Distance(a2, b2) <= 0.5f;
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.CharacterInit();
		}
	}
}
