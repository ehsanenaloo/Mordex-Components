using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonLayoutItem
	{
		public Control _parentControl;

		public Rectangle _bounds;

		public Rectangle _textBounds;

		public Rectangle _imageBounds;

		public bool _isVisible;

		public bool _isEnabled;

		private bool _hot;

		private bool _pushed;

		private bool _selected;

		private int _itemIndex;

		protected AccessibleObject _cachedAccessibilityObject;

		protected string _accessibleName;

		protected string _accessibleDescription;

		public virtual AccessibleObject AccessibilityObject
		{
			get
			{
				if (_cachedAccessibilityObject == null)
				{
					_cachedAccessibilityObject = new RibbonLayoutItemAccessibleObject(this);
				}
				return _cachedAccessibilityObject;
			}
		}

		public virtual string AccessibleName
		{
			get
			{
				return _accessibleName;
			}
			set
			{
				_accessibleName = value;
			}
		}

		public virtual string AccessibleDescription
		{
			get
			{
				return _accessibleDescription;
			}
			set
			{
				_accessibleDescription = value;
			}
		}

		public virtual AccessibleStates AccessibleState
		{
			get
			{
				AccessibleStates accessibleStates = (Focusable ? AccessibleStates.Focusable : AccessibleStates.None);
				if (!_isVisible)
				{
					accessibleStates |= AccessibleStates.Invisible;
				}
				if (Pushed)
				{
					accessibleStates |= AccessibleStates.Pressed;
				}
				if (!_isEnabled)
				{
					accessibleStates |= AccessibleStates.Unavailable;
				}
				if (Hot)
				{
					accessibleStates |= AccessibleStates.HotTracked;
				}
				return accessibleStates;
			}
		}

		public virtual AccessibleRole AccessibleRole => AccessibleRole.PushButton;

		public virtual string DefaultAction => null;

		public virtual bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected != value)
				{
					_selected = value;
				}
			}
		}

		public virtual bool Hot
		{
			get
			{
				return _hot;
			}
			set
			{
				_hot = value;
			}
		}

		public virtual bool Pushed
		{
			get
			{
				return _pushed;
			}
			set
			{
				if (_pushed != value)
				{
					_pushed = value;
					if (AccessibilityObject != null && AccessibilityObject is Control.ControlAccessibleObject controlAccessibleObject)
					{
						controlAccessibleObject.NotifyClients(AccessibleEvents.StateChange);
					}
				}
			}
		}

		public virtual bool Focusable => true;

		public RibbonLayoutItem()
			: this(null)
		{
		}

		public RibbonLayoutItem(Control parentControl)
		{
			_isEnabled = true;
			_isVisible = true;
			_parentControl = parentControl;
		}

		public virtual void DoDefaultAction()
		{
		}

		public int GetItemIndex()
		{
			return _itemIndex;
		}

		public void SetItemIndex(int ribbonLayoutState5Integer)
		{
			_itemIndex = ribbonLayoutState5Integer;
		}
	}
}
