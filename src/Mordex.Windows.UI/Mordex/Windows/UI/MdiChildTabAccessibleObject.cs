using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI
{
	internal class MdiChildTabAccessibleObject : AccessibleObject
	{
		private MdiTabItem _item;

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
					return _item.GetAccessibleName();
				}
				return base.Name;
			}
		}

		public override AccessibleRole Role => _item.GetAccessibleRole();

		public override AccessibleStates State => _item.AccessibleStates;

		public override Rectangle Bounds
		{
			get
			{
				if (_item != null)
				{
					if (_item.GetParentControl() != null && !_item.GetParentControl().IsDisposed)
					{
						return new Rectangle(_item.GetParentControl().PointToScreen(_item.GetBounds().Location), _item.GetBounds().Size);
					}
					return _item.GetBounds();
				}
				return base.Bounds;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (_item.GetAccessibleDefaultAction() != null)
				{
					return _item.GetAccessibleDefaultAction();
				}
				return "Press";
			}
		}

		public MdiChildTabAccessibleObject(MdiTabItem item)
		{
			_item = item;
		}

		public override void DoDefaultAction()
		{
			if (_item != null)
			{
				_item.Click();
			}
			else
			{
				base.DoDefaultAction();
			}
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_item._closeButton != null && _item._closeButton.GetIsVisible() && index == 0)
			{
				return _item._closeButton.GetAccessibleObject();
			}
			return null;
		}

		public override int GetChildCount()
		{
			if (_item._closeButton != null && _item._closeButton.GetIsVisible())
			{
				return 1;
			}
			return 0;
		}
	}
}
