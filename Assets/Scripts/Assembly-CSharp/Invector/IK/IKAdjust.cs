using System;

namespace Invector.IK
{
	[Serializable]
	public class IKAdjust
	{
		public IKOffsetTransform weaponHandOffset;

		public IKOffsetTransform weaponHintOffset;

		public IKOffsetTransform supportHandOffset;

		public IKOffsetTransform supportHintOffset;

		public IKOffsetSpine spineOffset;
	}
}
