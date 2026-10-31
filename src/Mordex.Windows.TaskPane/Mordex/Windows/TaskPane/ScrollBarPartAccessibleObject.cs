using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane
{
	internal class ScrollBarPartAccessibleObject : AccessibleObject
	{
		private int _areaIndex;

		private ControlStyleControl _owner;

		public override AccessibleRole Role
		{
			get
			{
				if (_areaIndex == 2)
				{
					return AccessibleRole.Indicator;
				}
				return AccessibleRole.PushButton;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (_areaIndex != 2)
				{
					return "Press";
				}
				return base.DefaultAction;
			}
		}

		public override string Name
		{
			get
			{
				bool isVertical = _owner.GetScrollBarType() == ScrollBarType.Vertical;
				switch (_areaIndex)
				{
				case 0:
					if (isVertical)
					{
						return "Line Up";
					}
					return "Line Left";
				case 1:
					if (isVertical)
					{
						return "Page Up";
					}
					return "Page Left";
				case 2:
					return "Position";
				case 3:
					if (isVertical)
					{
						return "Page Down";
					}
					return "Page Right";
				case 4:
					if (isVertical)
					{
						return "Line Down";
					}
					return "Line Right";
				default:
					return "";
				}
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (Parent.State == AccessibleStates.Invisible)
				{
					return AccessibleStates.Invisible;
				}
				return _owner.ComputeAccessibleStates(_areaIndex);
			}
		}

		public override Rectangle Bounds => _owner.GetAreaScreenBounds(_areaIndex);

		public override AccessibleObject Parent => _owner.AccessibilityObject;

		internal ScrollBarPartAccessibleObject(ControlStyleControl owner, int areaID)
		{
			_areaIndex = areaID;
			_owner = owner;
		}

		public override void DoDefaultAction()
		{
			if (_areaIndex != 2)
			{
				_owner.InvokeAreaAction(_areaIndex);
			}
			base.DoDefaultAction();
		}
	}
}
