using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Invector.vCharacterController.AI
{
	[SelectionBase]
	[vClassHeader("AI BASIC CONTROLLER", true, "icon_v2", false, "", iconName = "AI-icon")]
	public class vControlAI : vAIMotor, vIControlAI, vIHealthController, vIDamageReceiver
	{
		[vEditorToolbar("Start", false, "", false, false)]
		public bool disableAgentOnStart = true;

		[vEditorToolbar("Agent", false, "", false, false, order = 5)]
		[SerializeField]
		protected bool useNavMeshAgent = true;

		[SerializeField]
		protected vAIUpdateQuality updatePathQuality = vAIUpdateQuality.Medium;

		[SerializeField]
		[Range(1f, 10f)]
		protected float aceleration = 8f;

		[SerializeField]
		[Range(0.05f, 10f)]
		protected float _stopingDistance = 0.2f;

		[Header("Increase StoppingDistance by speed")]
		[SerializeField]
		[Range(0.05f, 10f)]
		protected float _walkingStopingDistance;

		[SerializeField]
		[Range(0.05f, 10f)]
		protected float _runningStopingDistance = 0.1f;

		[SerializeField]
		[Range(0.05f, 10f)]
		protected float _sprintingStopingDistance = 0.15f;

		[vEditorToolbar("Waypoint", false, "", false, false, order = 6)]
		[vHelpBox("You can create a new WaypointArea at the Invector/AIController/Components/Create new WaypointArea", vHelpBoxAttribute.MessageType.Info)]
		[SerializeField]
		protected vWaypointArea _waypointArea;

		[SerializeField]
		protected float _changeWaypointDistance;

		[SerializeField]
		protected bool _invertWaypointsOrder;

		[SerializeField]
		protected bool _randomStartingPoint = true;

		[SerializeField]
		protected bool _randomWaypoint = true;

		[SerializeField]
		protected bool startUsingSpecificWaypoint;

		[SerializeField]
		protected int startWaypointIndex;

		[SerializeField]
		protected bool startUsingNearWayPoint;

		[SerializeField]
		protected bool _selfStartingPoint;

		[SerializeField]
		protected Transform _customStartingPoint;

		[vEditorToolbar("Detection", false, "", false, false, order = 7)]
		[vHelpBox("Use a empty trasform inside the headBone transform as reference to the character Eyes", vHelpBoxAttribute.MessageType.None)]
		public Transform detectionPointReference;

		[SerializeField]
		[vEnumFlag]
		public vAISightMethod sightMethod = vAISightMethod.Center | vAISightMethod.Top;

		[SerializeField]
		protected vAIUpdateQuality findTargetUpdateQuality = vAIUpdateQuality.High;

		[SerializeField]
		protected vAIUpdateQuality canseeTargetUpdateQuality = vAIUpdateQuality.Medium;

		[SerializeField]
		[Tooltip("find target with current target found")]
		protected bool findOtherTarget;

		[SerializeField]
		protected float _changeTargetDelay = 2f;

		[SerializeField]
		protected bool findTargetByDistance = true;

		[SerializeField]
		protected float _fieldOfView = 90f;

		[SerializeField]
		protected float _minDistanceToDetect = 3f;

		[SerializeField]
		protected float _maxDistanceToDetect = 6f;

		[SerializeField]
		[vReadOnly(true)]
		protected bool _hasPositionOfTheTarget;

		[SerializeField]
		[vReadOnly(true)]
		protected bool _targetInLineOfSight;

		[vHelpBox("Considerer maxDistanceToDetect value + lostTargetDistance", vHelpBoxAttribute.MessageType.None)]
		[SerializeField]
		protected float _lostTargetDistance = 4f;

		[SerializeField]
		protected float _timeToLostWithoutSight = 5f;

		[Header("--- Layers to Detect ----")]
		[SerializeField]
		protected LayerMask _detectLayer;

		[SerializeField]
		protected vTagMask _detectTags;

		[SerializeField]
		protected LayerMask _obstacles = 1;

		[vEditorToolbar("Debug", false, "", false, false)]
		[vHelpBox("Debug Options", vHelpBoxAttribute.MessageType.None)]
		[SerializeField]
		protected bool _debugVisualDetection;

		[SerializeField]
		protected bool _debugRaySight;

		[SerializeField]
		protected bool _debugLastTargetPosition;

		[SerializeField]
		protected vAITarget _currentTarget;

		[SerializeField]
		protected vAIReceivedDamegeInfo _receivedDamage = new vAIReceivedDamegeInfo();

		internal vAIHeadtrack _headtrack;

		protected Vector3 _lastTargetPosition;

		protected int _currentWaypoint;

		private float lostTargetTime;

		private Vector3 lastValidDestination;

		private NavMeshHit navHit;

		private float changeTargetTime;

		protected bool isWaypointStarted;

		protected Vector3 _destination;

		protected Vector3 lasDestination;

		protected Vector3 temporaryDirection;

		[HideInInspector]
		public NavMeshAgent navMeshAgent;

		protected NavMeshHit navMeshHit;

		protected float updatePathTime;

		protected float updateFindTargetTime;

		protected float canseeTargetUpdateTime;

		protected float temporaryDirectionTime;

		protected float timeToResetOutDistance;

		protected float forceUpdatePathTime;

		protected bool isOutOfDistance;

		private int findAgentDestinationRadius;

		protected virtual Dictionary<Type, vIAIComponent> aiComponents { get; set; }

		protected virtual Vector3 destination
		{
			get
			{
				return _destination;
			}
			set
			{
				_destination = value;
			}
		}

		public virtual vAIReceivedDamegeInfo receivedDamage
		{
			get
			{
				return _receivedDamage;
			}
			protected set
			{
				_receivedDamage = value;
			}
		}

		public virtual bool targetInLineOfSight
		{
			get
			{
				return _targetInLineOfSight;
			}
		}

		public virtual vAITarget currentTarget
		{
			get
			{
				return _currentTarget;
			}
			protected set
			{
				_currentTarget = value;
			}
		}

		public virtual Vector3 lastTargetPosition
		{
			get
			{
				return _lastTargetPosition;
			}
			protected set
			{
				_lastTargetPosition = value;
			}
		}

		public virtual float targetDistance
		{
			get
			{
				if (currentTarget == null || currentTarget.isDead)
				{
					return float.PositiveInfinity;
				}
				return Vector3.Distance(currentTarget.transform.position, base.transform.position);
			}
		}

		public virtual bool isInDestination
		{
			get
			{
				if (useNavMeshAgent && (remainingDistance <= stopingDistance || (navMeshAgent.hasPath && remainingDistance > stopingDistance && navMeshAgent.desiredVelocity.magnitude < 0.1f)))
				{
					return true;
				}
				return remainingDistance <= stopingDistance;
			}
		}

		public virtual bool isMoving
		{
			get
			{
				return input.sqrMagnitude > 0.1f;
			}
		}

		public virtual float remainingDistance
		{
			get
			{
				if (!navMeshAgent || !navMeshAgent.enabled || !useNavMeshAgent || !isOnNavMesh)
				{
					return remainingDistanceWithoutAgent;
				}
				return navMeshAgent.remainingDistance;
			}
		}

		protected virtual float remainingDistanceWithoutAgent
		{
			get
			{
				return Vector3.Distance(base.transform.position, new Vector3(destination.x, base.transform.position.y, destination.z));
			}
		}

		public virtual Collider selfCollider
		{
			get
			{
				return _capsuleCollider;
			}
		}

		public virtual bool isOnJumpLink
		{
			get
			{
				if (!useNavMeshAgent)
				{
					return false;
				}
				if (navMeshAgent.isOnOffMeshLink && navMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross)
				{
					return true;
				}
				OffMeshLink offMeshLink = navMeshAgent.currentOffMeshLinkData.offMeshLink;
				if (offMeshLink != null && offMeshLink.area == NavMesh.GetAreaFromName("Jump"))
				{
					return true;
				}
				return false;
			}
		}

		public virtual bool isOnNavMesh
		{
			get
			{
				if (!useNavMeshAgent)
				{
					return false;
				}
				if (navMeshAgent.enabled)
				{
					return navMeshAgent.isOnNavMesh;
				}
				if (NavMesh.SamplePosition(base.transform.position, out navMeshHit, _capsuleCollider.radius, navMeshAgent.areaMask))
				{
					return true;
				}
				return false;
			}
		}

		public virtual Vector3 targetDestination
		{
			get
			{
				return _destination;
			}
		}

		public virtual float stopingDistance
		{
			get
			{
				return stopingDistanceRelativeToSpeed + _stopingDistance;
			}
			set
			{
				_stopingDistance = value;
			}
		}

		protected virtual float stopingDistanceRelativeToSpeed
		{
			get
			{
				if (base.movementSpeed != 0)
				{
					if (base.movementSpeed != vAIMovementSpeed.Running)
					{
						if (base.movementSpeed != vAIMovementSpeed.Sprinting)
						{
							return _walkingStopingDistance;
						}
						return _sprintingStopingDistance;
					}
					return _runningStopingDistance;
				}
				return 1f;
			}
		}

		public virtual Vector3 selfStartPosition { get; set; }

		public virtual vWaypointArea waypointArea
		{
			get
			{
				return _waypointArea;
			}
			set
			{
				if (value != null && value != _waypointArea)
				{
					List<vWaypoint> validPoints = value.GetValidPoints();
					if (_randomStartingPoint)
					{
						_currentWaypoint = UnityEngine.Random.Range(0, validPoints.Count);
					}
				}
				_waypointArea = value;
			}
		}

		public virtual vWaypoint targetWaypoint { get; protected set; }

		public virtual List<vWaypoint> visitedWaypoints { get; set; }

		public virtual bool selfStartingPoint
		{
			get
			{
				return _selfStartingPoint;
			}
			protected set
			{
				_selfStartingPoint = value;
			}
		}

		public virtual float changeWaypointDistance { get; protected set; }

		public bool customStartPoint
		{
			get
			{
				if (!selfStartingPoint)
				{
					return _customStartingPoint != null;
				}
				return false;
			}
		}

		public Vector3 customStartPosition
		{
			get
			{
				if (!customStartPoint)
				{
					return base.transform.position;
				}
				return _customStartingPoint.position;
			}
		}

		Transform vIDamageReceiver.transform
		{
			get
			{
				return base.transform;
			}
		}

		GameObject vIDamageReceiver.gameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		public virtual void CreatePrimaryComponents()
		{
			if (GetComponent<Rigidbody>() == null)
			{
				base.gameObject.AddComponent<Rigidbody>();
				Rigidbody component = GetComponent<Rigidbody>();
				component.mass = 50f;
				component.constraints = RigidbodyConstraints.FreezeRotation;
			}
			if (GetComponent<CapsuleCollider>() == null)
			{
				CapsuleCollider capsuleCollider = base.gameObject.AddComponent<CapsuleCollider>();
				base.animator = GetComponent<Animator>();
				if ((bool)base.animator)
				{
					Transform boneTransform = base.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
					float height = (float)Math.Round(Vector3.Distance(b: base.animator.GetBoneTransform(HumanBodyBones.Hips).position, a: boneTransform.position) * 2f, 2);
					capsuleCollider.height = height;
					capsuleCollider.center = new Vector3(0f, (float)Math.Round(capsuleCollider.height * 0.5f, 2), 0f);
					capsuleCollider.radius = (float)Math.Round(capsuleCollider.height * 0.15f, 2);
				}
			}
			if (GetComponent<NavMeshAgent>() == null)
			{
				base.gameObject.AddComponent<NavMeshAgent>();
			}
		}

		public virtual void CreateSecondaryComponents()
		{
		}

		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();
			if (_debugLastTargetPosition && (bool)currentTarget.transform && _hasPositionOfTheTarget)
			{
				Color color = (_targetInLineOfSight ? Color.green : Color.red);
				color.a = 0.2f;
				Gizmos.color = color;
				Gizmos.DrawLine(base.transform.position + Vector3.up * 1.5f, lastTargetPosition + Vector3.up * 1.5f);
				color.a = 1f;
				Gizmos.color = color;
				Gizmos.DrawLine(lastTargetPosition, lastTargetPosition + Vector3.up * 1.5f);
				Vector3 normalized = (lastTargetPosition - base.transform.position).normalized;
				normalized.y = 0f;
				Vector3 vector = Quaternion.AngleAxis(90f, Vector3.up) * normalized;
				Vector3 from = lastTargetPosition + Vector3.up * 1.5f - normalized;
				Vector3 to = lastTargetPosition + Vector3.up * 1.5f + normalized * 0.5f + vector * 0.25f;
				Vector3 vector2 = lastTargetPosition + Vector3.up * 1.5f + normalized * 0.5f - vector * 0.25f;
				Gizmos.DrawLine(from, to);
				Gizmos.DrawLine(from, vector2);
				Gizmos.DrawLine(vector2, to);
				Gizmos.DrawSphere(lastTargetPosition + Vector3.up * 1.5f, 0.1f);
			}
		}

		protected override void Start()
		{
			_receivedDamage = new vAIReceivedDamegeInfo();
			changeWaypointDistance = _changeWaypointDistance;
			selfStartPosition = ((!_selfStartingPoint && (bool)_customStartingPoint) ? _customStartingPoint.position : base.transform.position);
			_destination = base.transform.position;
			lasDestination = _destination;
			navMeshAgent = GetComponent<NavMeshAgent>();
			if (!navMeshAgent)
			{
				return;
			}
			navMeshAgent.updatePosition = false;
			navMeshAgent.updateRotation = false;
			if (isOnNavMesh)
			{
				navMeshAgent.enabled = true;
			}
			RotateTo(base.transform.forward);
			if (currentTarget != null)
			{
				currentTarget.InitTarget(currentTarget.transform);
			}
			_headtrack = GetComponent<vAIHeadtrack>();
			base.Start();
			aiComponents = new Dictionary<Type, vIAIComponent>();
			vIAIComponent[] components = GetComponents<vIAIComponent>();
			for (int i = 0; i < components.Length; i++)
			{
				if (!aiComponents.ContainsKey(components[i].ComponentType))
				{
					aiComponents.Add(components[i].ComponentType, components[i]);
				}
			}
			StartCoroutine(AlignDetectionPoint());
		}

		protected virtual IEnumerator AlignDetectionPoint()
		{
			yield return new WaitForSeconds(0.1f);
			if ((bool)detectionPointReference)
			{
				detectionPointReference.rotation = base.transform.rotation;
			}
		}

		protected override void UpdateAI()
		{
			base.UpdateAI();
			CalcMovementDirection();
			HandleTarget();
			if (receivedDamage != null)
			{
				receivedDamage.Update();
			}
		}

		public override void ResetRagdoll()
		{
			base.ResetRagdoll();
			if ((bool)_headtrack)
			{
				_headtrack.canLook = true;
			}
		}

		public override void EnableRagdoll()
		{
			base.EnableRagdoll();
			if ((bool)_headtrack)
			{
				_headtrack.canLook = false;
			}
		}

		public override void RemoveComponents()
		{
			base.RemoveComponents();
			if (removeComponentsAfterDie)
			{
				UnityEngine.Object.Destroy(navMeshAgent);
			}
		}

		protected override void OnAnimatorMove()
		{
			if (Time.deltaTime != 0f)
			{
				if (!customAction && useNavMeshAgent && (bool)navMeshAgent && navMeshAgent.enabled)
				{
					navMeshAgent.velocity = base.animator.deltaPosition / Time.deltaTime * Mathf.Clamp(remainingDistanceWithoutAgent - stopingDistance, 0f, 1f);
					navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, maxSpeed, aceleration * Time.deltaTime);
					navMeshAgent.nextPosition = base.animator.rootPosition;
				}
				base.OnAnimatorMove();
			}
		}

		public override void Stop()
		{
			base.Stop();
			if (useNavMeshAgent && (bool)navMeshAgent && navMeshAgent.isOnNavMesh && !navMeshAgent.isStopped)
			{
				navMeshAgent.isStopped = true;
				destination = base.transform.position;
				ForceUpdatePath();
				navMeshAgent.ResetPath();
			}
		}

		public override void DisableAIController()
		{
			if (disableAgentOnStart && (bool)navMeshAgent)
			{
				navMeshAgent.enabled = false;
			}
			base.DisableAIController();
		}

		protected virtual vWaypoint GetWaypoint()
		{
			if (waypointArea == null)
			{
				return null;
			}
			List<vWaypoint> validPoints = waypointArea.GetValidPoints(_invertWaypointsOrder);
			if (!isWaypointStarted)
			{
				if (startUsingSpecificWaypoint)
				{
					_currentWaypoint = startWaypointIndex % validPoints.Count;
				}
				else if (startUsingNearWayPoint)
				{
					_currentWaypoint = GetNearPointIndex();
				}
				else if (_randomWaypoint)
				{
					_currentWaypoint = UnityEngine.Random.Range(0, validPoints.Count);
				}
				else
				{
					_currentWaypoint = 0;
				}
			}
			if (isWaypointStarted)
			{
				if (_randomWaypoint)
				{
					_currentWaypoint = UnityEngine.Random.Range(0, validPoints.Count);
				}
				else
				{
					_currentWaypoint++;
				}
			}
			if (!isWaypointStarted)
			{
				isWaypointStarted = true;
				visitedWaypoints = new List<vWaypoint>();
			}
			if (_currentWaypoint >= validPoints.Count)
			{
				_currentWaypoint = 0;
			}
			if (validPoints.Count == 0)
			{
				return null;
			}
			if (visitedWaypoints.Count == validPoints.Count)
			{
				visitedWaypoints.Clear();
			}
			if (visitedWaypoints.Contains(validPoints[_currentWaypoint]))
			{
				return null;
			}
			return validPoints[_currentWaypoint];
		}

		public int GetNearPointIndex()
		{
			List<vWaypoint> validPoints = waypointArea.GetValidPoints(_invertWaypointsOrder);
			int result = 0;
			float num = float.PositiveInfinity;
			for (int i = 0; i < validPoints.Count; i++)
			{
				float num2 = Vector3.Distance(base.transform.position, validPoints[i].position);
				if (num2 < num)
				{
					result = i;
					num = num2;
				}
			}
			return result;
		}

		protected float GetUpdateTimeFromQuality(vAIUpdateQuality quality)
		{
			switch (quality)
			{
			default:
				return 0.1f;
			case vAIUpdateQuality.High:
				return 0.25f;
			case vAIUpdateQuality.Medium:
				return 0.75f;
			case vAIUpdateQuality.Low:
				return 1f;
			case vAIUpdateQuality.VeryLow:
				return 2f;
			}
		}

		protected virtual void UpdateAgentPath()
		{
			updatePathTime -= Time.deltaTime;
			if (updatePathTime > 0f && forceUpdatePathTime <= 0f && navMeshAgent.hasPath)
			{
				return;
			}
			forceUpdatePathTime -= Time.deltaTime;
			updatePathTime = GetUpdateTimeFromQuality(updatePathQuality);
			if (base.isDead || isJumping || !base.isGrounded)
			{
				return;
			}
			Vector3 vector = _destination;
			if (((base.movementSpeed == vAIMovementSpeed.Idle || !(vector != lasDestination)) && navMeshAgent.hasPath) || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
			{
				return;
			}
			if (NavMesh.SamplePosition(vector, out navHit, _capsuleCollider.radius + (float)findAgentDestinationRadius, navMeshAgent.areaMask) && (navHit.position - navMeshAgent.destination).magnitude > stopingDistance)
			{
				navMeshAgent.destination = navHit.position;
				lasDestination = vector;
			}
			else if ((navHit.position - navMeshAgent.destination).magnitude > stopingDistance)
			{
				findAgentDestinationRadius++;
				if (findAgentDestinationRadius >= 10)
				{
					findAgentDestinationRadius = 0;
				}
			}
		}

		protected virtual void CalcMovementDirection()
		{
			if (base.isDead || isJumping)
			{
				return;
			}
			if (useNavMeshAgent && (bool)navMeshAgent)
			{
				ControlNavMeshAgent();
				UpdateAgentPath();
			}
			Vector3 vector = (((navMeshAgent.hasPath || !(remainingDistanceWithoutAgent > navMeshAgent.stoppingDistance + _capsuleCollider.radius)) && navMeshAgent != null && navMeshAgent.enabled && useNavMeshAgent) ? (navMeshAgent.desiredVelocity * ((!isInDestination) ? 1 : 0)) : ((new Vector3(destination.x, base.transform.position.y, destination.z) - base.transform.position).normalized * Mathf.Clamp(remainingDistanceWithoutAgent - stopingDistance, 0f, 1f)));
			Vector3 vector2 = base.transform.InverseTransformDirection(vector);
			if (useNavMeshAgent && navMeshAgent.enabled)
			{
				OffMeshLinkData currentOffMeshLinkData = navMeshAgent.currentOffMeshLinkData;
				if (navMeshAgent.isOnOffMeshLink)
				{
					vector = currentOffMeshLinkData.endPos - base.transform.position;
					vector2 = base.transform.InverseTransformDirection(vector);
				}
			}
			if (vector2.magnitude > 0.1f)
			{
				if (temporaryDirectionTime <= 0f && !base.isStrafing)
				{
					SetMovementInput(vector2, aceleration);
				}
				else
				{
					SetMovementInput(vector2, (temporaryDirectionTime <= 0f) ? base.transform.forward : temporaryDirection, aceleration);
				}
			}
			else if (temporaryDirectionTime > 0f && temporaryDirection.magnitude >= 0.1f && vector2.magnitude < 0.1f)
			{
				TurnOnSpot(temporaryDirection);
			}
			else
			{
				input = Vector3.zero;
			}
			if (!base.isGrounded || isJumping || base.isRolling)
			{
				navMeshAgent.enabled = false;
			}
			temporaryDirectionTime -= Time.deltaTime;
		}

		protected virtual void CheckAgentDistanceFromAI()
		{
			if (!useNavMeshAgent || !navMeshAgent || !navMeshAgent.enabled)
			{
				return;
			}
			if (Vector3.Distance(base.transform.position, navMeshAgent.nextPosition) > stopingDistance * 1.5f && !isOutOfDistance)
			{
				timeToResetOutDistance = 3f;
				isOutOfDistance = true;
			}
			if (!isOutOfDistance)
			{
				return;
			}
			timeToResetOutDistance -= Time.deltaTime;
			if (timeToResetOutDistance <= 0f)
			{
				isOutOfDistance = false;
				if (Vector3.Distance(base.transform.position, navMeshAgent.nextPosition) > stopingDistance)
				{
					navMeshAgent.enabled = false;
				}
			}
		}

		protected virtual void ControlNavMeshAgent()
		{
			if (!base.isDead)
			{
				if (useNavMeshAgent && (bool)navMeshAgent)
				{
					navMeshAgent.stoppingDistance = stopingDistance;
				}
				if ((Time.deltaTime == 0f || !navMeshAgent.enabled) && !base.ragdolled && !isJumping && base.isGrounded && !navMeshAgent.enabled && isOnNavMesh)
				{
					navMeshAgent.enabled = true;
				}
				if (navMeshAgent.enabled && isOnJumpLink && !isJumping && base.isGrounded)
				{
					Vector3 vector = navMeshAgent.currentOffMeshLinkData.endPos - base.transform.position;
					Vector3 jumpTarget = base.transform.position + vector.normalized * (vector.magnitude + stopingDistance);
					JumpTo(jumpTarget);
				}
				if (isJumping || !base.isGrounded || base.ragdolled)
				{
					navMeshAgent.enabled = false;
				}
				CheckAgentDistanceFromAI();
			}
		}

		protected virtual bool CheckCanSeeTarget()
		{
			if (currentTarget != null && currentTarget.transform != null && currentTarget.collider == null && InFOVAngle(currentTarget.transform.position, _fieldOfView))
			{
				if (sightMethod == (vAISightMethod)0)
				{
					return true;
				}
				Vector3 start = (detectionPointReference ? detectionPointReference.position : (base.transform.position + Vector3.up * (selfCollider.bounds.size.y * 0.8f)));
				if (!Physics.Linecast(start, currentTarget.transform.position, _obstacles))
				{
					if (_debugRaySight)
					{
						Debug.DrawLine(start, currentTarget.transform.position, Color.green, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
					}
					return true;
				}
				if (_debugRaySight)
				{
					Debug.DrawLine(start, currentTarget.transform.position, Color.red, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
				}
			}
			else if ((bool)currentTarget.collider)
			{
				return CheckCanSeeTarget(currentTarget.collider);
			}
			return false;
		}

		protected virtual bool CheckCanSeeTarget(Collider target)
		{
			if (target != null && InFOVAngle(target.bounds.center, _fieldOfView))
			{
				if (sightMethod == (vAISightMethod)0)
				{
					return true;
				}
				Vector3 start = (detectionPointReference ? detectionPointReference.position : (base.transform.position + Vector3.up * (selfCollider.bounds.size.y * 0.8f)));
				if (sightMethod.Contains<vAISightMethod>(vAISightMethod.Center))
				{
					if (!Physics.Linecast(start, target.bounds.center, _obstacles))
					{
						if (_debugRaySight)
						{
							Debug.DrawLine(start, target.bounds.center, Color.green, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
						}
						return true;
					}
					if (_debugRaySight)
					{
						Debug.DrawLine(start, target.bounds.center, Color.red, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
					}
				}
				if (sightMethod.Contains<vAISightMethod>(vAISightMethod.Top))
				{
					if (!Physics.Linecast(start, target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, _obstacles))
					{
						if (_debugRaySight)
						{
							Debug.DrawLine(start, target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, Color.green, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
						}
						return true;
					}
					if (_debugRaySight)
					{
						Debug.DrawLine(start, target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, Color.red, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
					}
				}
				if (sightMethod.Contains<vAISightMethod>(vAISightMethod.Bottom))
				{
					if (!Physics.Linecast(start, target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, _obstacles))
					{
						if (_debugRaySight)
						{
							Debug.DrawLine(start, target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, Color.green, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
						}
						return true;
					}
					if (_debugRaySight)
					{
						Debug.DrawLine(start, target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, Color.red, GetUpdateTimeFromQuality(canseeTargetUpdateQuality));
					}
				}
			}
			return false;
		}

		protected virtual bool InFOVAngle(Vector3 viewPoint, float fieldOfView)
		{
			Vector3 vector = (detectionPointReference ? detectionPointReference.position : _capsuleCollider.bounds.center);
			if (Vector3.Distance(vector, viewPoint) < _minDistanceToDetect)
			{
				return true;
			}
			if (Vector3.Distance(vector, viewPoint) > _maxDistanceToDetect)
			{
				return false;
			}
			Quaternion quaternion = Quaternion.LookRotation(viewPoint - vector, Vector3.up);
			Vector3 vector2 = (detectionPointReference ? detectionPointReference.eulerAngles : base.transform.eulerAngles);
			Vector3 eulerAngle = quaternion.eulerAngles - vector2;
			float y = eulerAngle.NormalizeAngle().y;
			float x = eulerAngle.NormalizeAngle().x;
			if (y <= fieldOfView * 0.5f && y >= 0f - fieldOfView * 0.5f && x <= fieldOfView * 0.5f && x >= 0f - fieldOfView * 0.5f)
			{
				return true;
			}
			return false;
		}

		protected virtual void HandleTarget()
		{
			if (_hasPositionOfTheTarget && (bool)currentTarget.transform)
			{
				lastTargetPosition = currentTarget.transform.position;
			}
			canseeTargetUpdateTime -= Time.deltaTime;
			if (canseeTargetUpdateTime > 0f)
			{
				return;
			}
			if (currentTarget != null && (bool)currentTarget.transform)
			{
				_targetInLineOfSight = CheckCanSeeTarget();
				if (!_targetInLineOfSight || targetDistance >= _maxDistanceToDetect + _lostTargetDistance)
				{
					if (lostTargetTime < Time.time)
					{
						_hasPositionOfTheTarget = false;
						lostTargetTime = Time.time + _timeToLostWithoutSight;
					}
				}
				else
				{
					lostTargetTime = Time.time + _timeToLostWithoutSight;
					_hasPositionOfTheTarget = true;
					currentTarget.isLost = false;
				}
			}
			else
			{
				_targetInLineOfSight = false;
				_hasPositionOfTheTarget = false;
			}
			HandleLostTarget();
			canseeTargetUpdateTime = GetUpdateTimeFromQuality(canseeTargetUpdateQuality);
		}

		protected virtual void HandleLostTarget()
		{
			if (currentTarget == null || !(currentTarget.transform != null))
			{
				return;
			}
			if (currentTarget.hasHealthController && (currentTarget.isDead || targetDistance > _maxDistanceToDetect + _lostTargetDistance || (!targetInLineOfSight && !_hasPositionOfTheTarget)))
			{
				if (currentTarget.isFixedTarget)
				{
					currentTarget.isLost = true;
				}
				else
				{
					currentTarget.ClearTarget();
				}
			}
			else if (!currentTarget.hasHealthController && (currentTarget.transform == null || !currentTarget.transform.gameObject.activeSelf || targetDistance > _maxDistanceToDetect + _lostTargetDistance || (!targetInLineOfSight && !_hasPositionOfTheTarget)))
			{
				if (currentTarget.isFixedTarget)
				{
					currentTarget.isLost = true;
				}
				else
				{
					currentTarget.ClearTarget();
				}
			}
		}

		protected static bool IsInLayerMask(int layer, LayerMask layermask)
		{
			return (int)layermask == ((int)layermask | (1 << layer));
		}

		public virtual void SetDetectionLayer(LayerMask mask)
		{
			_detectLayer = mask;
		}

		public virtual void SetDetectionTags(List<string> tags)
		{
			_detectTags = tags;
		}

		public virtual void SetObstaclesLayer(LayerMask mask)
		{
			_obstacles = mask;
		}

		public virtual void SetLineOfSight(float fov = -1f, float minDistToDetect = -1f, float maxDistToDetect = -1f, float lostTargetDistance = -1f)
		{
			if (fov != -1f)
			{
				_fieldOfView = fov;
			}
			if (minDistToDetect != -1f)
			{
				_minDistanceToDetect = minDistToDetect;
			}
			if (maxDistToDetect != -1f)
			{
				_maxDistanceToDetect = maxDistToDetect;
			}
			if (lostTargetDistance != -1f)
			{
				_lostTargetDistance = lostTargetDistance;
			}
		}

		public virtual void FindTarget(bool checkForObstacles = true)
		{
			FindSpecificTarget(_detectTags, _detectLayer, checkForObstacles);
		}

		public virtual void FindSpecificTarget(List<string> m_detectTags, LayerMask m_detectLayer, bool checkForObstables = true)
		{
			if (updateFindTargetTime > Time.time)
			{
				return;
			}
			updateFindTargetTime = Time.time + GetUpdateTimeFromQuality(findTargetUpdateQuality);
			if ((!findOtherTarget && (bool)currentTarget.transform) || ((bool)currentTarget.transform && currentTarget.isFixedTarget && !findOtherTarget))
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.up, _maxDistanceToDetect, m_detectLayer);
			Transform transform = ((currentTarget != null && _hasPositionOfTheTarget) ? currentTarget.transform : null);
			float num = (((bool)transform && targetInLineOfSight) ? targetDistance : float.PositiveInfinity);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].transform != base.transform && m_detectTags.Contains(array[i].gameObject.tag) && (!checkForObstables || CheckCanSeeTarget(array[i])))
				{
					if (!findTargetByDistance)
					{
						transform = array[i].transform;
						break;
					}
					float num2 = Vector3.Distance(array[i].transform.position, base.transform.position);
					if (num2 < num)
					{
						transform = array[i].transform;
						num = num2;
					}
				}
			}
			if ((currentTarget == null || (transform != null && transform != currentTarget.transform)) && transform != null)
			{
				SetCurrentTarget(transform);
			}
		}

		public virtual void SetCurrentTarget(Transform target, bool overrideCanseTarget = true)
		{
			if (changeTargetTime < Time.time)
			{
				changeTargetTime = _changeTargetDelay + Time.time;
				currentTarget.InitTarget(target);
				if (overrideCanseTarget)
				{
					currentTarget.isLost = false;
					_targetInLineOfSight = true;
					_hasPositionOfTheTarget = false;
				}
				updateFindTargetTime = 0f;
				updatePathTime = 0f;
				lastTargetPosition = target.position;
				LookToTarget(target, 2f);
			}
		}

		public virtual void RemoveCurrentTarget()
		{
			currentTarget.ClearTarget();
		}

		public virtual void LookAround()
		{
			if ((bool)_headtrack && !lockMovement)
			{
				_headtrack.LookAround();
			}
		}

		public virtual void LookTo(Vector3 point, float stayLookTime = 1f, float offsetLookHeight = -1f)
		{
			if ((bool)_headtrack && !lockMovement)
			{
				_headtrack.LookAtPoint(point, stayLookTime, offsetLookHeight);
			}
		}

		public virtual void LookToTarget(Transform target, float stayLookTime = 1f, float offsetLookHeight = -1f)
		{
			if ((bool)_headtrack && !lockMovement)
			{
				_headtrack.LookAtTarget(target, stayLookTime, offsetLookHeight);
			}
		}

		public virtual void SetSpeed(vAIMovementSpeed movementSpeed)
		{
			if (base.movementSpeed != movementSpeed)
			{
				if (movementSpeed == vAIMovementSpeed.Idle)
				{
					Stop();
				}
				base.movementSpeed = movementSpeed;
			}
		}

		public virtual void MoveTo(Vector3 newDestination)
		{
			if (!lockMovement)
			{
				if (base.isStrafing)
				{
					updatePathTime = 0f;
				}
				SetFreeLocomotion();
				Vector3 vector = newDestination - base.transform.position;
				vector.y = 0f;
				destination = newDestination;
				temporaryDirection = base.transform.forward;
				temporaryDirectionTime = 0f;
			}
		}

		public virtual void StrafeMoveTo(Vector3 newDestination, Vector3 targetDirection)
		{
			if (useNavMeshAgent && (bool)navMeshAgent && navMeshAgent.isOnNavMesh && navMeshAgent.isStopped)
			{
				navMeshAgent.isStopped = false;
			}
			SetStrafeLocomotion();
			destination = newDestination;
			temporaryDirection = targetDirection;
			temporaryDirectionTime = 2f;
		}

		public virtual void RotateTo(Vector3 targetDirection)
		{
			targetDirection.y = 0f;
			if (Vector3.Angle(base.transform.forward, targetDirection) > 20f)
			{
				temporaryDirection = targetDirection;
				temporaryDirectionTime = 2f;
			}
		}

		public virtual void NextWayPoint()
		{
			targetWaypoint = GetWaypoint();
		}

		public override void TakeDamage(vDamage damage)
		{
			base.TakeDamage(damage);
			if (damage.damageValue > 0)
			{
				if ((!currentTarget.transform || ((bool)currentTarget.transform && !currentTarget.isFixedTarget) || (currentTarget.isFixedTarget && findOtherTarget)) && (bool)damage.sender && IsInLayerMask(damage.sender.gameObject.layer, _detectLayer) && _detectTags.Contains(damage.sender.gameObject.tag))
				{
					SetCurrentTarget(damage.sender, false);
				}
				receivedDamage.UpdateDamage(damage);
				updatePathTime = 0f;
			}
		}

		public void ForceUpdatePath(float timeInUpdate = 1f)
		{
			forceUpdatePathTime = timeInUpdate;
		}

		public bool HasComponent<T>() where T : vIAIComponent
		{
			return aiComponents.ContainsKey(typeof(T));
		}

		public T GetAIComponent<T>() where T : vIAIComponent
		{
			if (!aiComponents.ContainsKey(typeof(T)))
			{
				return default(T);
			}
			return (T)aiComponents[typeof(T)];
		}
	}
}
