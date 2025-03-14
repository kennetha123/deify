using System;
using UnityEngine;

namespace Invector
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	public class vHideInInspectorAttribute : PropertyAttribute
	{
		public string refbooleanProperty;

		public bool invertValue;

		public bool hideProperty { get; set; }

		public vHideInInspectorAttribute(string refbooleanProperty, bool invertValue = false)
		{
			this.refbooleanProperty = refbooleanProperty;
			this.invertValue = invertValue;
		}
	}
}
