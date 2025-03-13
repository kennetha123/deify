using UnityEngine;
using UnityEngine.AI;

public class SimpleHumanoidIKSamplePointAndClickController : MonoBehaviour
{
	public GameObject particle;

	protected NavMeshAgent agent;

	protected Animator animator;

	protected SimpleHumanoidIKSampleLocomotion locomotion;

	protected Object particleClone;

	private int destroyCountDown = 25;

	private bool setDestinationInNextFrame;

	[SerializeField]
	private float walkingSpeed = 1f;

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;
		animator = GetComponent<Animator>();
		locomotion = new SimpleHumanoidIKSampleLocomotion(animator);
		particleClone = null;
	}

	protected void SetDestination()
	{
		destroyCountDown = 25;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(ray, out hitInfo))
		{
			if (particleClone != null)
			{
				Object.Destroy(particleClone);
				particleClone = null;
			}
			Quaternion rotation = default(Quaternion);
			rotation.SetLookRotation(hitInfo.normal * -1f, Vector3.forward);
			Vector3 position = hitInfo.point + 1f * Vector3.up;
			particleClone = Object.Instantiate(particle, position, rotation);
			agent.destination = hitInfo.point;
		}
	}

	protected void SetupAgentLocomotion()
	{
		if (AgentDone() && locomotion != null)
		{
			locomotion.Do(0f, 0f);
			if (particleClone != null && destroyCountDown == 0)
			{
				Object.Destroy(particleClone);
				particleClone = null;
			}
		}
		else if (locomotion != null)
		{
			float magnitude = agent.desiredVelocity.magnitude;
			Vector3 vector = Quaternion.Inverse(base.transform.rotation) * agent.desiredVelocity;
			float direction = Mathf.Atan2(vector.x, vector.z) * 180f / 3.14159f;
			locomotion.Do(magnitude, direction);
		}
	}

	private void OnAnimatorMove()
	{
		if (Time.deltaTime != 0f)
		{
			agent.velocity = animator.deltaPosition / Time.deltaTime;
		}
		else
		{
			agent.velocity = Vector3.zero;
		}
		base.transform.rotation = animator.rootRotation;
	}

	protected bool AgentDone()
	{
		if (!agent.pathPending)
		{
			return AgentStopping();
		}
		return false;
	}

	protected bool AgentStopping()
	{
		return agent.remainingDistance <= agent.stoppingDistance;
	}

	private void Update()
	{
		if (destroyCountDown != 0)
		{
			destroyCountDown--;
		}
		if (setDestinationInNextFrame)
		{
			SetDestination();
			setDestinationInNextFrame = false;
		}
		if (Input.GetButtonDown("Fire1"))
		{
			agent.speed = walkingSpeed;
			setDestinationInNextFrame = true;
		}
		SetupAgentLocomotion();
	}
}
