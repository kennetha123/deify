using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Invector.vItemManager
{
	public class vSetFirstSelectable : MonoBehaviour
	{
		public GameObject firstSelectable;

		public void ApplyFirstSelectable(GameObject firstSelectable)
		{
			this.firstSelectable = firstSelectable;
		}

		private void OnEnable()
		{
			StartCoroutine(SetSelectableHandle(firstSelectable));
		}

		private IEnumerator SetSelectableHandle(GameObject target)
		{
			if (base.enabled)
			{
				yield return new WaitForEndOfFrame();
				SetSelectable(target);
			}
		}

		private void SetSelectable(GameObject target)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerExitHandler);
			EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
			ExecuteEvents.Execute(target, eventData, ExecuteEvents.selectHandler);
		}
	}
}
