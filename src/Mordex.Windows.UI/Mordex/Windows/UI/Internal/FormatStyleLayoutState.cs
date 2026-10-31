using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Internal
{
	internal class FormatStyleLayoutState
	{
		private const int InvalidateMargin = 2;

		protected AccessibleObject _accessibleObject;

		private string _accessibleDefaultActionDescription;

		private bool _isVisible = true;

		private bool _isEnabled = true;

		private bool _isHot;

		internal ILayoutItemOwner _owner;

		protected bool _mouseDown;

		public int _index = -1;

		private bool _isPressed;

		public Image _itemImage;

		public Rectangle _imageBounds = Rectangle.Empty;

		public Rectangle _textBounds = Rectangle.Empty;

		public string _caption = "";

		public Rectangle _bounds = Rectangle.Empty;

		protected Control _parentControl;

		public virtual AccessibleObject AccessibleObject
		{
			get
			{
				if (_accessibleObject == null)
				{
					_accessibleObject = new LayoutItemAccessibleObject(this);
				}
				return _accessibleObject;
			}
		}

		public virtual string AccessibleName => null;

		public virtual AccessibleStates AccessibleStates
		{
			get
			{
				AccessibleStates accessibleStates = AccessibleStates.Focusable;
				if (!IsVisible())
				{
					accessibleStates |= AccessibleStates.Invisible;
				}
				else if (!IsEnabled())
				{
					accessibleStates |= AccessibleStates.Unavailable;
				}
				else if (MouseDown)
				{
					accessibleStates |= AccessibleStates.Pressed;
				}
				return accessibleStates;
			}
		}

		public virtual AccessibleRole AccessibleRole => AccessibleRole.PushButton;

		public virtual string AccessibleDefaultActionDescription
		{
			get
			{
				if (_accessibleDefaultActionDescription != null)
				{
					return _accessibleDefaultActionDescription;
				}
				return "Press";
			}
			set
			{
				_accessibleDefaultActionDescription = value;
			}
		}

		public virtual bool MouseDown
		{
			get
			{
				return _mouseDown;
			}
			set
			{
				if (_mouseDown != value)
				{
					_mouseDown = value;
					if (_owner != null)
					{
						_owner.AccessibleStateChanged(this);
					}
				}
			}
		}

		public FormatStyleLayoutState()
		{
		}

		public FormatStyleLayoutState(string text, Control parentControl)
		{
			_parentControl = parentControl;
			_caption = text;
		}

		public FormatStyleLayoutState(ILayoutItemOwner owner)
		{
			_owner = owner;
		}

		public FormatStyleLayoutState(string text)
		{
			_caption = text;
		}

		public FormatStyleLayoutState(string text, Rectangle bounds)
		{
			_caption = text;
			_bounds = bounds;
		}

		public bool IsVisible()
		{
			return _isVisible;
		}

		public void SetVisible(bool isVisible)
		{
			if (_isVisible != isVisible)
			{
				_isVisible = isVisible;
				if (_owner != null)
				{
					_owner.AccessibleStateChanged(this);
				}
			}
		}

		public bool IsEnabled()
		{
			return _isEnabled;
		}

		public void SetEnabled(bool isEnabled)
		{
			if (_isEnabled != isEnabled)
			{
				_isEnabled = isEnabled;
				if (GetParentControl() != null)
				{
					GetParentControl().Invalidate(Rectangle.Inflate(_bounds, InvalidateMargin, InvalidateMargin));
				}
				if (_owner != null)
				{
					_owner.AccessibleStateChanged(this);
				}
			}
		}

		public bool IsHot()
		{
			return _isHot;
		}

		public void SetHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				if (GetParentControl() != null)
				{
					Rectangle rc = _bounds;
					rc.Inflate(InvalidateMargin, InvalidateMargin);
					GetParentControl().Invalidate(rc);
				}
			}
		}

		public bool IsPressed()
		{
			return _isPressed;
		}

		public void SetPressed(bool isPressed)
		{
			if (_isPressed != isPressed)
			{
				_isPressed = isPressed;
				if (_owner != null)
				{
					_owner.Invalidate(_bounds);
				}
				else if (_parentControl != null)
				{
					_parentControl.Invalidate(_bounds);
				}
			}
		}

		public Control GetParentControl()
		{
			if (_owner != null)
			{
				return _owner.ParentControl;
			}
			return _parentControl;
		}

		public void SetParentControl(Control parentControl)
		{
			_parentControl = parentControl;
		}

		public void InvokeDefaultAction()
		{
			if (_owner != null)
			{
				_owner.DoDefaultAction(this);
			}
		}
	}
}
