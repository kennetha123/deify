using UnityEngine;

public class ME_FixParticlesAligment : MonoBehaviour
{
	private void Start()
	{
		GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>().alignment = ParticleSystemRenderSpace.World;
	}
}
