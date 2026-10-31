using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class DockCaptionButtonAccessibleObject : AccessibleObject
	{
		private DockCaptionRenderer.CaptionItem _captionButton;

		public override string Name
		{
			get
			{
				if (_captionButton != null)
				{
					return _captionButton.GetAccessibleName();
				}
				return base.Name;
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
				if (_captionButton != null)
				{
					return _captionButton.GetAccessibleRole();
				}
				return base.Role;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (_captionButton != null)
				{
					return _captionButton.GetAccessibleStates();
				}
				return base.State;
			}
		}

		public override string Description
		{
			get
			{
				if (_captionButton != null)
				{
					return _captionButton.GetAccessibleDescription();
				}
				return base.Description;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (_captionButton._control != null)
				{
					return _captionButton._control.AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				Control control = ((_captionButton != null) ? _captionButton._control : null);
				if (_captionButton != null)
				{
					if (control != null)
					{
						Point location = control.PointToScreen(_captionButton._bounds.Location);
						return new Rectangle(location, _captionButton._bounds.Size);
					}
					return _captionButton._bounds;
				}
				return base.Bounds;
			}
		}

		public override string DefaultAction => "Press";

		public DockCaptionButtonAccessibleObject(DockCaptionRenderer.CaptionItem buttonInfo)
		{
			_captionButton = buttonInfo;
		}

		public override void DoDefaultAction()
		{
			if (_captionButton != null)
			{
				_captionButton.Click();
			}
			else
			{
				base.DoDefaultAction();
			}
		}
	}
}
