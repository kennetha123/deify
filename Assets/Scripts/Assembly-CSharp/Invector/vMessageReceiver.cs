using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Invector
{
	[vClassHeader("MESSAGE RECEIVER", "Use this component with the vMessageSender to call Events.")]
	public class vMessageReceiver : vMonoBehaviour
	{
		[Serializable]
		public class OnReceiveMessageEvent : UnityEvent<string>
		{
		}

		[Serializable]
		public class vMessageListener
		{
			public string Name;

			public OnReceiveMessageEvent onReceiveMessage;

			public vMessageListener(string name)
			{
				Name = name;
			}

			public vMessageListener(string name, UnityAction<string> listener)
			{
				Name = name;
				onReceiveMessage.AddListener(listener);
			}
		}

		public OnReceiveMessageEvent defaultListener;

		public List<vMessageListener> messagesListeners;

		public void AddListener(string name, UnityAction<string> listener)
		{
			if (messagesListeners.Exists((vMessageListener l) => l.Name.Equals(name)))
			{
				messagesListeners.Find((vMessageListener l) => l.Name.Equals(name)).onReceiveMessage.AddListener(listener);
			}
			else
			{
				messagesListeners.Add(new vMessageListener(name, listener));
			}
		}

		public void RemoveListener(string name, UnityAction<string> listener)
		{
			if (messagesListeners.Exists((vMessageListener l) => l.Name.Equals(name)))
			{
				messagesListeners.Find((vMessageListener l) => l.Name.Equals(name)).onReceiveMessage.RemoveListener(listener);
			}
		}

		public void Send(string name, string message)
		{
			if (messagesListeners.Exists((vMessageListener l) => l.Name.Equals(name)))
			{
				messagesListeners.Find((vMessageListener l) => l.Name.Equals(name)).onReceiveMessage.Invoke(message);
			}
			else
			{
				defaultListener.Invoke(message);
			}
		}

		public void Send(string name)
		{
			if (messagesListeners.Exists((vMessageListener l) => l.Name.Equals(name)))
			{
				messagesListeners.Find((vMessageListener l) => l.Name.Equals(name)).onReceiveMessage.Invoke(string.Empty);
			}
			else
			{
				defaultListener.Invoke(string.Empty);
			}
		}
	}
}
