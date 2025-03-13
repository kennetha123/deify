using System.Collections.Generic;
using UnityEngine;

namespace Invector.vEventSystems
{
	public class vAnimatorStateInfos
	{
		public Animator animator;

		private Dictionary<string, List<int>> statesRunning = new Dictionary<string, List<int>>();

		public int currentlayer;

		public vAnimatorStateInfos(Animator animator)
		{
			this.animator = animator;
		}

		public void RegisterListener()
		{
			vAnimatorTag[] behaviours = animator.GetBehaviours<vAnimatorTag>();
			for (int i = 0; i < behaviours.Length; i++)
			{
				behaviours[i].RemoveStateInfoListener(this);
				behaviours[i].AddStateInfoListener(this);
			}
		}

		public void RemoveListener()
		{
			if ((bool)animator)
			{
				vAnimatorTag[] behaviours = animator.GetBehaviours<vAnimatorTag>();
				for (int i = 0; i < behaviours.Length; i++)
				{
					behaviours[i].RemoveStateInfoListener(this);
				}
			}
		}

		internal void AddStateInfo(string tag, int info)
		{
			if (!statesRunning.ContainsKey(tag))
			{
				statesRunning.Add(tag, new List<int> { info });
			}
			else
			{
				statesRunning[tag].Add(info);
			}
			currentlayer = info;
		}

		internal void RemoveStateInfo(string tag, int info)
		{
			if (statesRunning.ContainsKey(tag) && statesRunning[tag].Exists((int _info) => _info.Equals(info)))
			{
				int item = statesRunning[tag].Find((int _info) => _info.Equals(info));
				statesRunning[tag].Remove(item);
				if (statesRunning[tag].Count == 0)
				{
					statesRunning.Remove(tag);
				}
			}
			if (currentlayer == info)
			{
				currentlayer = -1;
			}
		}

		public bool HasTag(string tag)
		{
			return statesRunning.ContainsKey(tag);
		}

		public bool HasAllTags(params string[] tags)
		{
			bool result = ((tags.Length != 0) ? true : false);
			for (int i = 0; i < tags.Length; i++)
			{
				if (!HasTag(tags[i]))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public bool HasAnyTag(params string[] tags)
		{
			bool result = false;
			for (int i = 0; i < tags.Length; i++)
			{
				if (HasTag(tags[i]))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public AnimatorStateInfo? GetCurrentAnimatorStateUsingTag(string tag)
		{
			if (currentlayer != -1 && HasTag(tag) && statesRunning[tag].Exists((int _inf) => _inf.Equals(currentlayer)))
			{
				return animator.GetCurrentAnimatorStateInfo(currentlayer);
			}
			return null;
		}
	}
}
