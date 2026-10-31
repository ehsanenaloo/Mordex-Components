using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation
{
	internal class NavigationGroupAccessibleObject : AccessibleObject
	{
		private NavigationGroup _group;

		public override string DefaultAction
		{
			get
			{
				if (_group != null)
				{
					if (_group.Selected)
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
				if (_group != null && _group.NavigationBar != null)
				{
					return _group.NavigationBar.AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				Control control = ((_group != null) ? _group.NavigationBar : null);
				if (_group != null)
				{
					if (control != null)
					{
						Point location = control.PointToScreen(_group.Bounds.Location);
						return new Rectangle(location, _group.Bounds.Size);
					}
					return _group.Bounds;
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
						string displayText = _group.Text.Replace("&", "");
						if (displayText.Length > 0)
						{
							return displayText;
						}
					}
				}
				return base.Name;
			}
		}

		internal NavigationGroupAccessibleObject(NavigationGroup group)
		{
			_group = group;
		}

		public override void DoDefaultAction()
		{
			if (_group != null)
			{
				_group.Selected = !_group.Selected;
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
	}
}
