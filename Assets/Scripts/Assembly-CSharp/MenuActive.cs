using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MenuActive : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	public bool choosenActive;

	public int id;

	private MainMenu mainMenu;

	public UnityEvent function;

	private void Start()
	{
		mainMenu = Object.FindObjectOfType<MainMenu>();
	}

	private void Update()
	{
		if (mainMenu.arrowIsReady)
		{
			if (choosenActive)
			{
				mainMenu.arrowActive[id].SetActive(true);
			}
			else
			{
				mainMenu.arrowActive[id].SetActive(false);
			}
			if (choosenActive && mainMenu.menuSelect.GetButtonDown())
			{
				function.Invoke();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		mainMenu.DisableArrow();
		mainMenu.menuActive[id].choosenActive = true;
	}
}
