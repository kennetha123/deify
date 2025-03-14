using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vEventSystems
{
	public class vAnimatorTagAdvanced : vAnimatorTagBase
	{
		public enum vAnimatorEventTriggerType
		{
			AllByNormalizedTime = 0,
			EnterStateExitByNormalized = 1,
			EnterByNormalizedExitState = 2,
			EnterStateExitState = 3
		}

		[Serializable]
		public class vAdvancedTags
		{
			public string tagName;

			public vAnimatorEventTriggerType tagType;

			public Vector2 normalizedTime = new Vector2(0.1f, 0.8f);

			private int loopCount;

			private bool isEnter;

			private bool isExit;

			public vAdvancedTags(string tag)
			{
				tagName = tag;
				tagType = vAnimatorEventTriggerType.AllByNormalizedTime;
			}

			public void UpdateEventTrigger(float normalizedTime, List<vAnimatorStateInfos> stateInfos, int layer, float speed = 1f, bool debug = false)
			{
				float num = Mathf.Clamp(normalizedTime, 0f, (float)loopCount + 1f);
				if (!isEnter && tagType != vAnimatorEventTriggerType.EnterStateExitByNormalized && tagType != vAnimatorEventTriggerType.EnterStateExitState && num >= (float)loopCount + this.normalizedTime.x / speed)
				{
					if (debug)
					{
						Debug.Log("ADD TAG " + tagName + " in  " + normalizedTime);
					}
					AddTag(stateInfos, layer);
				}
				if (!isExit && isEnter && tagType != vAnimatorEventTriggerType.EnterByNormalizedExitState && tagType != vAnimatorEventTriggerType.EnterStateExitState && num >= (float)loopCount + this.normalizedTime.y / speed)
				{
					RemoveTag(stateInfos, layer);
					if (debug)
					{
						Debug.Log("REMOVE TAG " + tagName + " in  " + normalizedTime);
					}
				}
				if (normalizedTime > (float)(loopCount + 1))
				{
					isEnter = false;
					isExit = false;
					loopCount++;
				}
			}

			public void AddTag(List<vAnimatorStateInfos> stateInfos, int layer)
			{
				for (int i = 0; i < stateInfos.Count; i++)
				{
					stateInfos[i].AddStateInfo(tagName, layer);
				}
				isEnter = true;
			}

			public void RemoveTag(List<vAnimatorStateInfos> stateInfos, int layer)
			{
				for (int i = 0; i < stateInfos.Count; i++)
				{
					stateInfos[i].RemoveStateInfo(tagName, layer);
					isExit = true;
				}
			}

			public void Init()
			{
				isEnter = false;
				isExit = false;
				loopCount = 0;
			}
		}

		public bool debug;

		public List<vAdvancedTags> tags = new List<vAdvancedTags>
		{
			new vAdvancedTags("CustomAction")
		};

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateEnter(animator, stateInfo, layerIndex);
			if (stateInfos == null)
			{
				return;
			}
			for (int i = 0; i < tags.Count; i++)
			{
				tags[i].Init();
				if (tags[i].tagType == vAnimatorEventTriggerType.EnterStateExitState || tags[i].tagType == vAnimatorEventTriggerType.EnterStateExitByNormalized)
				{
					if (debug)
					{
						Debug.Log("ADD TAG " + tags[i].tagName + " OnStateEnter  ");
					}
					tags[i].AddTag(stateInfos, layerIndex);
				}
				else
				{
					tags[i].UpdateEventTrigger(stateInfo.normalizedTime, stateInfos, layerIndex, animator.speed, debug);
				}
			}
		}

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (stateInfos != null)
			{
				for (int i = 0; i < tags.Count; i++)
				{
					if (tags[i].tagType != vAnimatorEventTriggerType.EnterStateExitState)
					{
						tags[i].UpdateEventTrigger(stateInfo.normalizedTime, stateInfos, layerIndex, animator.speed, debug);
					}
				}
			}
			base.OnStateUpdate(animator, stateInfo, layerIndex);
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (stateInfos != null)
			{
				for (int i = 0; i < tags.Count; i++)
				{
					if (tags[i].tagType == vAnimatorEventTriggerType.EnterStateExitState || tags[i].tagType == vAnimatorEventTriggerType.EnterByNormalizedExitState)
					{
						if (debug)
						{
							Debug.Log("REMOVE TAG " + tags[i].tagName + " OnStateExit  ");
						}
						tags[i].RemoveTag(stateInfos, layerIndex);
					}
					else
					{
						tags[i].UpdateEventTrigger(stateInfo.normalizedTime, stateInfos, layerIndex, animator.speed, debug);
					}
				}
			}
			base.OnStateExit(animator, stateInfo, layerIndex);
		}
	}
}
