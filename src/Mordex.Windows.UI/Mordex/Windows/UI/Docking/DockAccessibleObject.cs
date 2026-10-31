using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class DockAccessibleObject : AccessibleObject
	{
		private DockCaptionRenderer _panelRenderer;

		public override string Name
		{
			get
			{
				string text = "";
				if (_panelRenderer._panel != null)
				{
					text = _panelRenderer._panel.Text;
					if (text.Length == 0)
					{
						text = _panelRenderer._panel.Name;
					}
				}
				if (text.Length > 0)
				{
					return text;
				}
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override AccessibleRole Role => AccessibleRole.TitleBar;

		public override AccessibleStates State => AccessibleStates.Focusable;

		public override string Value
		{
			get
			{
				if (_panelRenderer._panel != null)
				{
					return _panelRenderer._panel.Text;
				}
				return base.Value;
			}
			set
			{
				base.Value = value;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (GetOwnerControl() != null)
				{
					return GetOwnerControl().AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				if (_panelRenderer != null)
				{
					if (GetOwnerControl() != null)
					{
						Point location = GetOwnerControl().PointToScreen(_panelRenderer.GetCaptionBounds().Location);
						return new Rectangle(location, _panelRenderer.GetCaptionBounds().Size);
					}
					return _panelRenderer.GetCaptionBounds();
				}
				return base.Bounds;
			}
		}

		public DockAccessibleObject(DockCaptionRenderer captionItem)
		{
			_panelRenderer = captionItem;
		}

		public override int GetChildCount()
		{
			int count = 0;
			if (_panelRenderer._captionButton != null)
			{
				count++;
			}
			if (_panelRenderer._autoHideButton != null)
			{
				count++;
			}
			if (_panelRenderer._closeButton != null)
			{
				count++;
			}
			if (_panelRenderer._dragHandle != null)
			{
				count++;
			}
			return count;
		}

		private Control GetOwnerControl()
		{
			if (_panelRenderer != null && _panelRenderer._panel != null)
			{
				if (_panelRenderer._panel.ContainerCaptionControl != null)
				{
					return _panelRenderer._panel.ContainerCaptionControl;
				}
				return _panelRenderer._panel;
			}
			return null;
		}

		public override AccessibleObject GetChild(int index)
		{
			IList<AccessibleObject> list = new List<AccessibleObject>();
			if (_panelRenderer._captionButton != null)
			{
				list.Add(_panelRenderer._captionButton.GetAccessibleObject());
			}
			if (_panelRenderer._autoHideButton != null)
			{
				list.Add(_panelRenderer._autoHideButton.GetAccessibleObject());
			}
			if (_panelRenderer._closeButton != null)
			{
				list.Add(_panelRenderer._closeButton.GetAccessibleObject());
			}
			if (_panelRenderer._dragHandle != null)
			{
				list.Add(_panelRenderer._dragHandle.GetAccessibleObject());
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}
	}
}
