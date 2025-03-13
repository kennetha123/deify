using UnityEngine;
using UnityEngine.SceneManagement;

namespace Invector.vCharacterController
{
	public class vLoadLevel : MonoBehaviour
	{
		[Tooltip("Write the name of the level you want to load")]
		public string levelToLoad;

		[Tooltip("True if you need to spawn the character into a transform location on the scene to load")]
		public bool findSpawnPoint = true;

		[Tooltip("Assign here the spawnPoint name of the scene that you will load")]
		public string spawnPointName;

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.tag.Equals("Player"))
			{
				new GameObject("spawnPointFinder").AddComponent<vFindSpawnPoint>().AlighObjetToSpawnPoint(other.gameObject, spawnPointName);
				SceneManager.LoadScene(levelToLoad);
			}
		}
	}
}
