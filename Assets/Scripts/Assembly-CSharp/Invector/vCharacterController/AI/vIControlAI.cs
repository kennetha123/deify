using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
	public interface vIControlAI : vIHealthController, vIDamageReceiver
	{
		Vector3 selfStartPosition { get; set; }

		Vector3 targetDestination { get; }

		Collider selfCollider { get; }

		Animator animator { get; }

		vAnimatorStateInfos animatorStateInfos { get; }

		vWaypointArea waypointArea { get; set; }

		vAIReceivedDamegeInfo receivedDamage { get; }

		vWaypoint targetWaypoint { get; }

		List<vWaypoint> visitedWaypoints { get; set; }

		vAITarget currentTarget { get; }

		Vector3 lastTargetPosition { get; }

		bool ragdolled { get; }

		bool isInDestination { get; }

		bool isMoving { get; }

		bool isStrafing { get; }

		bool isRolling { get; }

		bool isCrouching { get; set; }

		bool targetInLineOfSight { get; }

		vAIMovementSpeed movementSpeed { get; }

		float targetDistance { get; }

		float changeWaypointDistance { get; }

		float remainingDistance { get; }

		float stopingDistance { get; set; }

		bool selfStartingPoint { get; }

		bool customStartPoint { get; }

		Vector3 customStartPosition { get; }

		void CreatePrimaryComponents();

		void CreateSecondaryComponents();

		bool HasComponent<T>() where T : vIAIComponent;

		T GetAIComponent<T>() where T : vIAIComponent;

		void SetDetectionLayer(LayerMask mask);

		void SetDetectionTags(List<string> tags);

		void SetObstaclesLayer(LayerMask mask);

		void SetLineOfSight(float fov = -1f, float minDistToDetect = -1f, float maxDistToDetect = -1f, float lostTargetDistance = -1f);

		void NextWayPoint();

		void SetSpeed(vAIMovementSpeed speed);

		void MoveTo(Vector3 destination);

		void StrafeMoveTo(Vector3 destination, Vector3 forwardDiretion);

		void RotateTo(Vector3 direction);

		void RollTo(Vector3 direction);

		void SetCurrentTarget(Transform target, bool overrideCanseeTarget = true);

		void RemoveCurrentTarget();

		void FindTarget(bool checkForObstacles = true);

		void FindSpecificTarget(List<string> m_detectTags, LayerMask m_detectLayer, bool checkForObstables = true);

		void LookAround();

		void LookTo(Vector3 point, float stayLookTime = 1f, float offsetLookHeight = -1f);

		void LookToTarget(Transform target, float stayLookTime = 1f, float offsetLookHeight = -1f);

		void Stop();

		void ForceUpdatePath(float timeInUpdate = 1f);
	}
}
