using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane
{
	internal class TaskPaneGroupAccessibleObject : AccessibleObject
	{
		private TaskPaneGroup _group;

		public override string DefaultAction
		{
			get
			{
				if (_group != null)
				{
					if (_group.Expanded)
					{
						return "Collapse";
					}
					return "Expand";
				}
				return base.DefaultAction;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (_group != null && _group.TaskPane != null)
				{
					return _group.TaskPane.AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				Control control = ((_group != null) ? _group.TaskPane : null);
				if (_group != null)
				{
					if (control != null)
					{
						Point location = control.PointToScreen(_group.GetHeaderBounds().Location);
						return new Rectangle(location, _group.GetHeaderBounds().Size);
					}
					return _group.GetHeaderBounds();
				}
				return base.Bounds;
			}
		}

		public override string Description
		{
			get
			{
				if (_group != null)
				{
					return _group.AccessibleDescription;
				}
				return base.Description;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_group != null)
				{
					return _group.GetAccessibleRole();
				}
				return base.Role;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (_group != null)
				{
					return _group.ComputeAccessibleStates();
				}
				return base.State;
			}
		}

		public override string Name
		{
			get
			{
				if (_group != null)
				{
					if (_group.AccessibleName != null && _group.AccessibleName.Length > 0)
					{
						return _group.AccessibleName;
					}
					if (_group.Text != null)
					{
						string caption = _group.Text.Replace("&", "");
						if (caption.Length > 0)
						{
							return caption;
						}
					}
				}
				return base.Name;
			}
		}

		internal TaskPaneGroupAccessibleObject(TaskPaneGroup group)
		{
			_group = group;
		}

		public override void DoDefaultAction()
		{
			if (_group != null)
			{
				_group.Expanded = !_group.Expanded;
			}
			else
			{
				base.DoDefaultAction();
			}
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_group != null)
			{
				if (_group.Container && _group.ContainerControl != null)
				{
					childCount++;
				}
				else if (_group.Items != null)
				{
					childCount = _group.Items.Count;
				}
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_group != null)
			{
				if (_group.Container && _group.ContainerControl != null)
				{
					if (index == 0)
					{
						return _group.ContainerControl.AccessibilityObject;
					}
				}
				else if (_group.Items != null)
				{
					if (index >= 0 && index < _group.Items.Count)
					{
						return _group.Items[index].GetAccessibleObject();
					}
					index -= _group.Items.Count;
				}
			}
			return null;
		}

		public override AccessibleObject HitTest(int integer, int integerArgument)
		{
			if (_group != null && _group.TaskPane != null)
			{
				Point clientPoint = _group.TaskPane.PointToClient(new Point(integer, integerArgument));
				if (_group.Container && _group.ContainerControl != null)
				{
					if (_group.ContainerControl.ClientRectangle.Contains(clientPoint))
					{
						return _group.ContainerControl.AccessibilityObject;
					}
				}
				else
				{
					TaskPaneItem itemAt = _group.TaskPane.GetItemAt(clientPoint);
					if (itemAt != null)
					{
						return itemAt.GetAccessibleObject();
					}
				}
			}
			return this;
		}
	}
}
