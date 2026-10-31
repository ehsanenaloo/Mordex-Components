using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI
{
	internal class LayoutItemAccessibleObject : AccessibleObject
	{
		private FormatStyleLayoutState _item;

		public override AccessibleObject Parent
		{
			get
			{
				if (_item.GetParentControl() != null)
				{
					return _item.GetParentControl().AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override string Name
		{
			get
			{
				if (_item != null)
				{
					return _item.AccessibleName;
				}
				return base.Name;
			}
		}

		public override AccessibleRole Role => _item.AccessibleRole;

		public override AccessibleStates State => _item.AccessibleStates;

		public override Rectangle Bounds
		{
			get
			{
				if (_item != null)
				{
					if (_item.GetParentControl() != null)
					{
						return new Rectangle(_item.GetParentControl().PointToScreen(_item._bounds.Location), _item._bounds.Size);
					}
					return _item._bounds;
				}
				return base.Bounds;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (_item.AccessibleDefaultActionDescription != null)
				{
					return _item.AccessibleDefaultActionDescription;
				}
				return "Press";
			}
		}

		public LayoutItemAccessibleObject(FormatStyleLayoutState item)
		{
			_item = item;
		}

		public override void DoDefaultAction()
		{
			if (_item != null)
			{
				_item.InvokeDefaultAction();
			}
			else
			{
				base.DoDefaultAction();
			}
		}
	}
}
