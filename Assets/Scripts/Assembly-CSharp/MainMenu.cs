using System.Collections;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public GameObject title;

	public GameObject pressAnyButton;

	public GameObject[] menus;

	public GenericInput menuSelect = new GenericInput("Enter", "A", "A");

	public GenericInput menuHorizontal = new GenericInput("W", "Vertical", "Vertical");

	public MenuActive[] menuActive;

	public GameObject[] arrowActive;

	[SerializeField]
	public int menuActiveId;

	[SerializeField]
	public bool arrowIsReady;

	private float timer;

	private void Start()
	{
		DisableArrow();
	}

	public void DisableArrow()
	{
		for (int i = 0; i < arrowActive.Length; i++)
		{
			arrowActive[i].SetActive(false);
		}
		for (int j = 0; j < menuActive.Length; j++)
		{
			menuActive[j].choosenActive = false;
		}
	}

	public void PressAnyButton()
	{
		pressAnyButton.SetActive(false);
		title.GetComponent<Animator>().Play("TitleMoving");
		for (int i = 0; i < menus.Length; i++)
		{
			menus[i].GetComponent<Animator>().Play("NewGame");
		}
		StartCoroutine(ArrowShown());
	}

	public void NewGame()
	{
		SceneManager.LoadScene(1);
	}

	public void Feedback()
	{
		Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSc7JF4c6B9kfzQzsEPkRd8xWw3dPofU96YqJrBes_p9f4D8LA/viewform");
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	private IEnumerator ArrowShown()
	{
		yield return new WaitForSeconds(3f);
		arrowIsReady = true;
		menuActive[0].choosenActive = true;
	}

	private void Update()
	{
		if (Input.anyKeyDown && pressAnyButton.active)
		{
			PressAnyButton();
		}
		if (menuHorizontal.GetAxis() != 0f)
		{
			timer -= Time.deltaTime;
		}
		if (!(timer <= 0f))
		{
			return;
		}
		DisableArrow();
		if (menuHorizontal.GetAxis() < 0f)
		{
			menuActiveId++;
			if (menuActiveId < 0)
			{
				menuActiveId = 2;
			}
			else if (menuActiveId > 2)
			{
				menuActiveId = 0;
			}
			menuActive[menuActiveId].choosenActive = true;
		}
		else if (menuHorizontal.GetAxis() > 0f)
		{
			DisableArrow();
			menuActiveId--;
			if (menuActiveId < 0)
			{
				menuActiveId = 2;
			}
			else if (menuActiveId > 2)
			{
				menuActiveId = 0;
			}
			menuActive[menuActiveId].choosenActive = true;
		}
		timer = 0.2f;
	}
}
