using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane
{
	internal class TaskPaneAccessibleObject : Control.ControlAccessibleObject
	{
		private MxTaskPane _taskPane;

		public override string Name
		{
			get
			{
				string accessibleName = _taskPane.AccessibleName;
				if (accessibleName != null)
				{
					return accessibleName;
				}
				accessibleName = _taskPane.Text;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				return _taskPane.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_taskPane.AccessibleRole != AccessibleRole.Default)
				{
					return _taskPane.AccessibleRole;
				}
				return AccessibleRole.Pane;
			}
		}

		public override AccessibleStates State => AccessibleStates.Focusable;

		public TaskPaneAccessibleObject(MxTaskPane taskPane)
			: base(taskPane)
		{
			_taskPane = taskPane;
		}

		public override int GetChildCount()
		{
			int childCount = _taskPane.Groups.Count;
			if (_taskPane._verticalScrollbar != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index >= 0 && index < _taskPane.Groups.Count)
			{
				return _taskPane.Groups[index].GetAccessibleObject();
			}
			index -= _taskPane.Groups.Count;
			if (_taskPane._verticalScrollbar != null)
			{
				if (index == 0)
				{
					return _taskPane._verticalScrollbar.AccessibilityObject;
				}
				index--;
			}
			return null;
		}

		public override AccessibleObject HitTest(int integer, int integerArgument)
		{
			if (_taskPane != null)
			{
				Point clientPoint = _taskPane.PointToClient(new Point(integer, integerArgument));
				foreach (TaskPaneGroup group in _taskPane.Groups)
				{
					if (group.GetHeaderBounds().Contains(clientPoint))
					{
						return group.GetAccessibleObject();
					}
					if (group.Container && group.ContainerControl != null && group.ContainerControl.Bounds.Contains(clientPoint))
					{
						return group.ContainerControl.AccessibilityObject;
					}
					if (group.GetItemsBounds().Contains(clientPoint))
					{
						return group.GetAccessibleObject();
					}
				}
				if (_taskPane._verticalScrollbar != null && _taskPane._verticalScrollbar.Bounds.Contains(clientPoint))
				{
					return _taskPane._verticalScrollbar.AccessibilityObject;
				}
			}
			return base.HitTest(integer, integerArgument);
		}
	}
}
