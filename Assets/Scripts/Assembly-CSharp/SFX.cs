using UnityEngine;

public class SFX : MonoBehaviour
{
	public AudioClip[] sfxList;

	public void PlaySFX(int id)
	{
		GetComponent<AudioSource>().PlayOneShot(sfxList[id]);
	}
}
