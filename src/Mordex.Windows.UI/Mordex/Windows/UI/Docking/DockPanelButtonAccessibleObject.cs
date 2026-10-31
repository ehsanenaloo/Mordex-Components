using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class DockPanelButtonAccessibleObject : AccessibleObject
	{
		private OutlookPanelButton _panelButton;

		public override string Name
		{
			get
			{
				if (_panelButton != null)
				{
					return _panelButton.GetAccessibleName();
				}
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override AccessibleRole Role => AccessibleRole.PushButton;

		public override AccessibleStates State
		{
			get
			{
				if (_panelButton != null)
				{
					return _panelButton.GetAccessibleStates();
				}
				return base.State;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (_panelButton.GetPanelGroup() != null)
				{
					return _panelButton.GetPanelGroup().AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				Control control = ((_panelButton != null) ? _panelButton.GetPanelGroup() : null);
				if (_panelButton != null)
				{
					if (control != null)
					{
						Point location = _panelButton.GetPanelGroup().PointToScreen(_panelButton.GetBounds().Location);
						return new Rectangle(location, _panelButton.GetBounds().Size);
					}
					return _panelButton.GetBounds();
				}
				return base.Bounds;
			}
		}

		public DockPanelButtonAccessibleObject(OutlookPanelButton pnlButton)
		{
			_panelButton = pnlButton;
		}
	}
}
