using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane
{
	internal class TaskPaneItemAccessibleObject : AccessibleObject
	{
		private TaskPaneItem _item;

		public override string DefaultAction
		{
			get
			{
				switch (_item.ItemType)
				{
				case ItemType.Description:
				case ItemType.Separator:
					return null;
				default:
					return "Press";
				}
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (_item != null && _item.TaskPane != null)
				{
					return _item.TaskPane.AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				Control control = ((_item != null) ? _item.TaskPane : null);
				if (_item != null)
				{
					if (control != null)
					{
						Point location = control.PointToScreen(_item.Bounds.Location);
						return new Rectangle(location, _item.Bounds.Size);
					}
					return _item.Bounds;
				}
				return base.Bounds;
			}
		}

		public override string Description
		{
			get
			{
				if (_item != null)
				{
					return _item.AccessibleDescription;
				}
				return base.Description;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_item != null)
				{
					return _item.GetAccessibleRole();
				}
				return base.Role;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (_item != null)
				{
					return _item.ComputeAccessibleStates();
				}
				return base.State;
			}
		}

		public override string Name
		{
			get
			{
				if (_item != null)
				{
					if (_item.AccessibleName != null && _item.AccessibleName.Length > 0)
					{
						return _item.AccessibleName;
					}
					if (_item.Text != null)
					{
						string caption = _item.Text.Replace("&", "");
						if (caption.Length > 0)
						{
							return caption;
						}
					}
				}
				return base.Name;
			}
		}

		internal TaskPaneItemAccessibleObject(TaskPaneItem item)
		{
			_item = item;
		}

		public override void DoDefaultAction()
		{
			if (_item != null && _item.ItemType == ItemType.LinkButton)
			{
				_item.InvokeOnClick();
			}
		}
	}
}
