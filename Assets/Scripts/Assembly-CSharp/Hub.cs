using Invector.vCharacterController;
using LastBoss.Character;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hub : MonoBehaviour
{
	private vThirdPersonController player;

	public bool isEndOfTheGame;

	private Canvas canvasEnding;

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<vThirdPersonController>();
		if (isEndOfTheGame)
		{
			canvasEnding = GameObject.Find("CanvasEnding").GetComponent<Canvas>();
			canvasEnding.enabled = false;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (isEndOfTheGame)
		{
			SceneManager.LoadScene(0);
		}
		else if (!collision.gameObject.GetComponent<RevengeMode>().isRevengeMode)
		{
			Reset();
		}
	}

	public void Reset()
	{
		player.ResetHealth();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
