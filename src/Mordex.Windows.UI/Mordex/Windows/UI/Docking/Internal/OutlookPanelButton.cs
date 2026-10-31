using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class OutlookPanelButton
	{
		private DockPanelBase _panel;

		private IPanelButtonOwner _owner;

		private Rectangle _bounds;

		private bool _isHot;

		private bool _isSelected;

		private bool _isPressed;

		private string _accessibleName;

		private string _text;

		private CommandImageSource _imageSource;

		internal bool _canHover;

		private MxDockPanelGroup _panelGroup;

		private AccessibleRole _accessibleRole;

		private AccessibleObject _accessibleObject;

		private string _kind;

		internal OutlookPanelButton(IPanelButtonOwner owner, DockPanelBase panel, AccessibleRole accessibleRole)
			: this(owner, panel, accessibleRole, "")
		{
		}

		internal OutlookPanelButton(IPanelButtonOwner owner, DockPanelBase panel, AccessibleRole accessibleRole, string accessibleName)
		{
			_panel = panel;
			_owner = owner;
			_text = "";
			_kind = "";
			_canHover = true;
			_accessibleName = accessibleName;
			_accessibleRole = accessibleRole;
			if (owner is OutlookNavigatorGroupState groupState)
			{
				_panelGroup = groupState._panelGroup;
			}
		}

		public AccessibleRole GetAccessibleRole()
		{
			return _accessibleRole;
		}

		public MxDockPanelGroup GetPanelGroup()
		{
			return _panelGroup;
		}

		public string GetAccessibleName()
		{
			if (_accessibleName == null || (_accessibleName.Length == 0 && _panel != null))
			{
				string text = _panel.AccessibleName;
				if (text == null || text.Length == 0)
				{
					text = _panel.Text;
				}
				if (text == null || text.Length == 0)
				{
					text = _panel.Name;
				}
				return text;
			}
			return _accessibleName;
		}

		public AccessibleStates GetAccessibleStates()
		{
			AccessibleStates accessibleStates = AccessibleStates.Focusable;
			if (IsPressed())
			{
				accessibleStates |= AccessibleStates.Pressed;
			}
			if (IsSelected())
			{
				accessibleStates |= AccessibleStates.Checked;
			}
			if (IsHot())
			{
				accessibleStates |= AccessibleStates.HotTracked;
			}
			return accessibleStates;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new DockPanelButtonAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal DockPanelBase GetPanel()
		{
			return _panel;
		}

		internal Rectangle GetBounds()
		{
			return _bounds;
		}

		internal void SetBounds(Rectangle rectangle)
		{
			_bounds = rectangle;
		}

		internal ButtonState GetButtonState()
		{
			ButtonState result = ButtonState.Normal;
			if (_isHot && _isSelected)
			{
				result = ButtonState.Hot | ButtonState.Selected;
			}
			else if (_isHot)
			{
				result = ButtonState.Hot;
			}
			else if (_isPressed)
			{
				result = ButtonState.Pressed;
			}
			else if (_isSelected)
			{
				result = ButtonState.Selected;
			}
			return result;
		}

		internal void NotifyOwner()
		{
			if (_owner != null)
			{
				_owner.Invalidate(this);
			}
		}

		internal bool IsHot()
		{
			return _isHot;
		}

		internal void SetHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				NotifyOwner();
			}
		}

		internal bool IsSelected()
		{
			return _isSelected;
		}

		internal void SetSelected(bool uiPanelBaseLayoutStateBoolean)
		{
			if (_isSelected != uiPanelBaseLayoutStateBoolean)
			{
				_isSelected = uiPanelBaseLayoutStateBoolean;
				NotifyOwner();
			}
		}

		internal bool IsPressed()
		{
			return _isPressed;
		}

		internal void SetPressed(bool isPressed)
		{
			if (_isPressed != isPressed)
			{
				_isPressed = isPressed;
				NotifyOwner();
			}
		}

		internal CommandImageSource GetImageSource()
		{
			return _imageSource;
		}

		internal void SetImageSource(CommandImageSource imageSource)
		{
			_imageSource = imageSource;
			NotifyOwner();
		}

		internal string GetText()
		{
			return _text;
		}

		internal void SetText(string text)
		{
			if (text == null)
			{
				text = "";
			}
			if (_text != text)
			{
				_text = text;
				NotifyOwner();
			}
		}

		internal string GetKind()
		{
			return _kind;
		}

		internal void SetKind(string uiPanelBaseLayoutStateText)
		{
			if (uiPanelBaseLayoutStateText == null)
			{
				uiPanelBaseLayoutStateText = "";
			}
			_kind = uiPanelBaseLayoutStateText;
		}
	}
}
