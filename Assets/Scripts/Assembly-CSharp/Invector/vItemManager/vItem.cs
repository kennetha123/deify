using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
	[Serializable]
	public class vItem : ScriptableObject
	{
		[HideInInspector]
		public int id;

		[HideInInspector]
		public string description = "Item Description";

		[HideInInspector]
		public vItemType type;

		[HideInInspector]
		public Sprite icon;

		[HideInInspector]
		public bool stackable = true;

		[HideInInspector]
		public int maxStack;

		public int amount;

		[HideInInspector]
		public GameObject originalObject;

		[HideInInspector]
		public GameObject dropObject;

		[HideInInspector]
		public List<vItemAttribute> attributes = new List<vItemAttribute>();

		[HideInInspector]
		public bool isInEquipArea;

		public bool destroyAfterUse = true;

		public bool canBeUsed = true;

		public bool canBeDroped = true;

		public bool canBeDestroyed = true;

		public bool displayAttributes = true;

		[Header("Animation Settings")]
		[vHelpBox("Triggers a animation when Equipping a Weapon or enabling item.\nYou can also trigger an animation if the ItemType is a Consumable", vHelpBoxAttribute.MessageType.None)]
		public string EnableAnim = "LowBack";

		[vHelpBox("Triggers a animation when Unequipping a Weapon or disable item", vHelpBoxAttribute.MessageType.None)]
		public string DisableAnim = "LowBack";

		[vHelpBox("Delay to enable the Weapon/Item object when Equipping\n If ItemType is a Consumable use this to delay the item usage.", vHelpBoxAttribute.MessageType.None)]
		public float enableDelayTime = 0.5f;

		[vHelpBox("Delay to hide the Weapon/Item object when Unequipping", vHelpBoxAttribute.MessageType.None)]
		public float disableDelayTime = 0.5f;

		[vHelpBox("If the item is equippable use this to set a custom handler to instantiate the SpawnObject", vHelpBoxAttribute.MessageType.None)]
		public string customHandler;

		[vHelpBox("If the item is equippable and need to use two hand\n<color=yellow><b>This option makes it impossible to equip two items</b></color>", vHelpBoxAttribute.MessageType.None)]
		public bool twoHandWeapon;

		public Texture2D iconTexture
		{
			get
			{
				if (!icon)
				{
					return null;
				}
				try
				{
					if (icon.rect.width != (float)icon.texture.width || icon.rect.height != (float)icon.texture.height)
					{
						Texture2D obj = new Texture2D((int)icon.textureRect.width, (int)icon.textureRect.height)
						{
							name = icon.name
						};
						Color[] pixels = icon.texture.GetPixels((int)icon.textureRect.x, (int)icon.textureRect.y, (int)icon.textureRect.width, (int)icon.textureRect.height);
						obj.SetPixels(pixels);
						obj.Apply();
						return obj;
					}
					return icon.texture;
				}
				catch
				{
					Debug.LogWarning("Icon texture of the " + base.name + " is not Readable", icon.texture);
					return icon.texture;
				}
			}
		}

		public vItemAttribute GetItemAttribute(vItemAttributes attribute)
		{
			if (attributes != null)
			{
				return attributes.Find((vItemAttribute _attribute) => _attribute.name == attribute);
			}
			return null;
		}

		public vItemAttribute GetItemAttribute(string name)
		{
			if (attributes != null)
			{
				return attributes.Find((vItemAttribute attribute) => attribute.name.ToString().Equals(name));
			}
			return null;
		}
	}
}
