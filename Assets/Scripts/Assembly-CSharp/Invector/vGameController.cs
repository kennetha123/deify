using System;
using System.Collections;
using Invector.vCharacterController;
using LastBoss.Character;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Invector
{
	public class vGameController : MonoBehaviour
	{
		[Serializable]
		public class OnRealoadGame : UnityEvent
		{
		}

		[Tooltip("Assign here the locomotion (empty transform) to spawn the Player")]
		public Transform spawnPoint;

		[Tooltip("Assign the Character Prefab to instantiate at the SpawnPoint, leave unassign to Restart the Scene")]
		public GameObject playerPrefab;

		[Tooltip("Time to wait until the scene restart or the player will be spawned again")]
		public float respawnTimer = 4f;

		[Tooltip("Check if you want to leave your dead body at the place you died")]
		public bool destroyBodyAfterDead;

		[HideInInspector]
		public OnRealoadGame OnReloadGame = new OnRealoadGame();

		[HideInInspector]
		public GameObject currentPlayer;

		private vThirdPersonController currentController;

		public static vGameController instance;

		private GameObject oldPlayer;

		public bool displayInfoInFadeText = true;

		public GameObject spawnTransform;

		private RevengeMode revenge;

		public bool thereIsEnemyInRevenge;

		private void Start()
		{
			GameObject.FindGameObjectWithTag("Player").transform.position = spawnTransform.transform.position;
			if (instance == null)
			{
				instance = this;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				base.gameObject.name = base.gameObject.name + " Instance";
				SceneManager.sceneLoaded += OnLevelFinishedLoading;
				if (displayInfoInFadeText && (bool)vHUDController.instance)
				{
					vHUDController.instance.ShowText("Init Scene");
				}
				FindPlayer();
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void OnCharacterDead(GameObject _gameObject)
		{
			revenge = GameObject.FindGameObjectWithTag("Player").GetComponent<RevengeMode>();
			revenge.isDead = true;
			if (!revenge.isRevengeModeAvailable)
			{
				RealDead();
			}
			StartCoroutine(Waiting(0.5f, base.gameObject));
		}

		private IEnumerator Waiting(float timer, GameObject _gameObject)
		{
			yield return new WaitForSeconds(timer);
			if (revenge.isRevengeModeAvailable && thereIsEnemyInRevenge)
			{
				GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>().Play("Dead");
				revenge.EnterSpiritMode();
				yield break;
			}
			oldPlayer = _gameObject;
			if (playerPrefab != null)
			{
				StartCoroutine(Spawn());
				yield break;
			}
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Restarting Scene...");
			}
			Invoke("ResetScene", respawnTimer);
		}

		public void RealDead()
		{
			GameObject.Find("SFX").GetComponent<SFX>().PlaySFX(0);
			GameObject.Find("BLACK BACKGROUND").GetComponent<Animator>().Play("FadeOut");
			GameObject.Find("Dead Screen").GetComponent<Animator>().Play("YouDead");
			Debug.Log("Tes");
		}

		public void Spawn(Transform _spawnPoint)
		{
			if (!(playerPrefab != null))
			{
				return;
			}
			if (oldPlayer != null && destroyBodyAfterDead)
			{
				if (displayInfoInFadeText && (bool)vHUDController.instance)
				{
					vHUDController.instance.ShowText("Player destroyed: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
				}
				UnityEngine.Object.Destroy(oldPlayer);
			}
			else if (oldPlayer != null)
			{
				if (displayInfoInFadeText && (bool)vHUDController.instance)
				{
					vHUDController.instance.ShowText("Remove Player Components: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
				}
				DestroyPlayerComponents(oldPlayer);
			}
			currentPlayer = UnityEngine.Object.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
			currentController = currentPlayer.GetComponent<vThirdPersonController>();
			currentController.onDead.AddListener(OnCharacterDead);
			OnReloadGame.Invoke();
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Spawn player: " + currentPlayer.name.Replace("(Clone)", ""));
			}
		}

		public IEnumerator Spawn()
		{
			yield return new WaitForSeconds(respawnTimer);
			if (!(playerPrefab != null) || !(spawnPoint != null))
			{
				yield break;
			}
			if (oldPlayer != null && destroyBodyAfterDead)
			{
				if (displayInfoInFadeText && (bool)vHUDController.instance)
				{
					vHUDController.instance.ShowText("Player destroyed: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
				}
				UnityEngine.Object.Destroy(oldPlayer);
			}
			else
			{
				if (displayInfoInFadeText && (bool)vHUDController.instance)
				{
					vHUDController.instance.ShowText("Remove Player Components: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
				}
				DestroyPlayerComponents(oldPlayer);
			}
			yield return new WaitForEndOfFrame();
			currentPlayer = UnityEngine.Object.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
			currentController = currentPlayer.GetComponent<vThirdPersonController>();
			currentController.onDead.AddListener(OnCharacterDead);
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Respawn player: " + currentPlayer.name.Replace("(Clone)", ""));
			}
			OnReloadGame.Invoke();
		}

		private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
		{
			if (currentController.currentHealth > 0f)
			{
				if (displayInfoInFadeText && (bool)vHUDController.instance)
				{
					vHUDController.instance.ShowText("Load Scene: " + scene.name);
				}
				return;
			}
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Reload Scene");
			}
			OnReloadGame.Invoke();
			FindPlayer();
		}

		private void FindPlayer()
		{
			vThirdPersonController vThirdPersonController = UnityEngine.Object.FindObjectOfType<vThirdPersonController>();
			if ((bool)vThirdPersonController)
			{
				currentPlayer = vThirdPersonController.gameObject;
				currentController = vThirdPersonController;
				vThirdPersonController.onDead.AddListener(OnCharacterDead);
				if (displayInfoInFadeText && (bool)vHUDController.instance)
				{
					vHUDController.instance.ShowText("Found player: " + currentPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
				}
			}
			else if (currentPlayer == null && playerPrefab != null && spawnPoint != null)
			{
				Spawn(spawnPoint);
			}
		}

		public void ResetScene()
		{
			UnityEngine.Object.Destroy(GameObject.FindGameObjectWithTag("Player"));
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		private void DestroyPlayerComponents(GameObject target)
		{
			if ((bool)target)
			{
				MonoBehaviour[] componentsInChildren = target.GetComponentsInChildren<MonoBehaviour>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i]);
				}
				Collider component = target.GetComponent<Collider>();
				if (component != null)
				{
					UnityEngine.Object.Destroy(component);
				}
				Rigidbody component2 = target.GetComponent<Rigidbody>();
				if (component2 != null)
				{
					UnityEngine.Object.Destroy(component2);
				}
				Animator component3 = target.GetComponent<Animator>();
				if (component3 != null)
				{
					UnityEngine.Object.Destroy(component3);
				}
			}
		}
	}
}
