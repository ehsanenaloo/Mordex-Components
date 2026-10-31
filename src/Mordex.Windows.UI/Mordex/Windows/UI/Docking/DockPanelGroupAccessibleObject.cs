using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	internal class DockPanelGroupAccessibleObject : Control.ControlAccessibleObject
	{
		private MxDockPanelGroup _panelGroup;

		public override Rectangle Bounds
		{
			get
			{
				if (_panelGroup != null)
				{
					return new Rectangle(_panelGroup.PointToScreen(_panelGroup.DisplayRectangle.Location), _panelGroup.DisplayRectangle.Size);
				}
				return base.Bounds;
			}
		}

		public override string Name
		{
			get
			{
				if (_panelGroup != null)
				{
					string accessibleName = _panelGroup.AccessibleName;
					if (accessibleName != null && accessibleName.Length > 0)
					{
						return accessibleName;
					}
					accessibleName = _panelGroup.Text;
					if (accessibleName != null && accessibleName.Length > 0)
					{
						return accessibleName;
					}
					return _panelGroup.Name;
				}
				return "Panel Group";
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
				if (_panelGroup.AccessibleRole != AccessibleRole.Default)
				{
					return _panelGroup.AccessibleRole;
				}
				if (_panelGroup.GroupStyle == PanelGroupStyle.HorizontalTiles || _panelGroup.GroupStyle == PanelGroupStyle.VerticalTiles)
				{
					return AccessibleRole.Pane;
				}
				return AccessibleRole.PageTabList;
			}
		}

		public override AccessibleStates State => AccessibleStates.None;

		public DockPanelGroupAccessibleObject(MxDockPanelGroup panelGroup)
			: base(panelGroup)
		{
			_panelGroup = panelGroup;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_panelGroup._containerCaptionControl != null)
			{
				childCount++;
			}
			else if (_panelGroup._captionRenderer != null)
			{
				childCount++;
			}
			if (_panelGroup != null && _panelGroup._groupState != null)
			{
				childCount += _panelGroup._groupState.GetAccessibleChildCount();
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_panelGroup._containerCaptionControl != null)
			{
				if (index == 0)
				{
					return _panelGroup._containerCaptionControl.AccessibilityObject;
				}
				index--;
			}
			else if (_panelGroup._captionRenderer != null)
			{
				if (index == 0)
				{
					return _panelGroup._captionRenderer.GetAccessibleObject();
				}
				index--;
			}
			if (_panelGroup != null && _panelGroup._groupState != null)
			{
				return _panelGroup._groupState.GetAccessibleChild(index);
			}
			return null;
		}
	}
}
