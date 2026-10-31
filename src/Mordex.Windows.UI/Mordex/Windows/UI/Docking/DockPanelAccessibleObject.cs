using System.Collections.Generic;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	internal class DockPanelAccessibleObject : Control.ControlAccessibleObject
	{
		private MxDockPanel _panel;

		public override string Name
		{
			get
			{
				string accessibleName = _panel.AccessibleName;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				accessibleName = _panel.Text;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				return _panel.Name;
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
				if (_panel.AccessibleRole != AccessibleRole.Default)
				{
					return _panel.AccessibleRole;
				}
				return AccessibleRole.Pane;
			}
		}

		public override AccessibleStates State => AccessibleStates.None;

		public DockPanelAccessibleObject(MxDockPanel panel)
			: base(panel)
		{
			_panel = panel;
		}

		public override int GetChildCount()
		{
			int count = 0;
			if (_panel._containerCaptionControl != null)
			{
				count++;
			}
			else if (_panel._captionRenderer != null)
			{
				count++;
			}
			if (_panel.InnerContainer != null)
			{
				count++;
			}
			return count;
		}

		public override AccessibleObject GetChild(int index)
		{
			IList<AccessibleObject> list = new List<AccessibleObject>();
			if (_panel._containerCaptionControl != null)
			{
				list.Add(_panel._containerCaptionControl.AccessibilityObject);
			}
			else if (_panel._captionRenderer != null)
			{
				list.Add(_panel._captionRenderer.GetAccessibleObject());
			}
			if (_panel.InnerContainer != null)
			{
				list.Add(_panel.InnerContainer.AccessibilityObject);
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}
	}
}
