using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class MdiTabItem
	{
		private const int DefaultButtonSize = 16;

		private const int ContentPadding = 4;

		private const int MinTextWidth = 15;

		private const int CloseButtonSlotWidth = 18;

		private const int CloseButtonRightOffset = 20;

		private const int TabVerticalPadding = 6;

		private const int SelectedTabExtraHeight = 2;

		private const int TabTopInset = 3;

		private const int TabOverlapOffset = 5;

		private const string MeasureSampleText = "W";

		private MdiChildTab _childTab;

		private int _slantWidth;

		internal MdiTabItem _closeButton;

		private Point _location;

		internal bool _isHot;

		private bool _isSelected;

		private TabButtonType _buttonType;

		private bool _useCompatibleTextRendering;

		private bool _closeButtonOnTab;


		private Control _parentControl;

		private AccessibleObject _accessibleObject;

		private bool _isVisible = true;

		internal bool _isDisabled;

		internal bool _isPressed;

		private Size _itemSize;

		private int _tabOverlap;

		public virtual AccessibleStates AccessibleStates
		{
			get
			{
				AccessibleStates accessibleStates = AccessibleStates.Focusable;
				if (!GetIsVisible())
				{
					accessibleStates |= AccessibleStates.Invisible;
				}
				else if (_isPressed)
				{
					accessibleStates |= AccessibleStates.Pressed;
				}
				else if (_isHot)
				{
					accessibleStates |= AccessibleStates.HotTracked;
				}
				if (_buttonType == TabButtonType.ChildForm)
				{
					accessibleStates = (_isDisabled ? (accessibleStates | AccessibleStates.Unavailable) : (accessibleStates | AccessibleStates.Selectable));
					if (IsSelected())
					{
						accessibleStates |= AccessibleStates.Selected;
					}
				}
				return accessibleStates;
			}
		}

		internal MdiTabItem(TabButtonType buttonType, MdiChildTab mdiChild, Control parentControl, bool useCompatibleTextRendering, bool closeButtonOnTab)
		{
			_parentControl = parentControl;
			_buttonType = buttonType;
			_childTab = mdiChild;
			_location = Point.Empty;
			_useCompatibleTextRendering = useCompatibleTextRendering;
			_closeButtonOnTab = closeButtonOnTab;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new MdiChildTabAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal Control GetParentControl()
		{
			return _parentControl;
		}

		public string GetAccessibleName()
		{
			if (_childTab != null)
			{
				string text = GetChildTab().Text;
				text = text.Replace("&", "");
				if (text.Length == 0 && GetChildTab().Form != null)
				{
					text = GetChildTab().Form.Name;
				}
				return text;
			}
			return null;
		}

		public AccessibleRole GetAccessibleRole()
		{
			if (_buttonType == TabButtonType.ChildForm)
			{
				return AccessibleRole.PageTab;
			}
			return AccessibleRole.PushButton;
		}

		public string GetAccessibleDefaultAction()
		{
			if (_buttonType == TabButtonType.ChildForm)
			{
				return "Switch";
			}
			return "Press";
		}

		public void Click()
		{
			if (_parentControl != null && _parentControl is MxMdiTabGroup uiMdiTabGroup)
			{
				uiMdiTabGroup.HandleTabButtonClick(this);
			}
		}

		internal bool IsSelected()
		{
			if (!_isSelected || _buttonType != TabButtonType.ChildForm)
			{
				return _isSelected;
			}
			return _childTab.Form == _childTab.Form.MdiParent.ActiveMdiChild;
		}

		internal void SetIsSelected(bool isSelected)
		{
			_isSelected = isSelected;
		}

		internal MdiChildTab GetChildTab()
		{
			return _childTab;
		}

		internal Point GetLocation()
		{
			return _location;
		}

		internal void SetLocation(Point point)
		{
			if (_location != point)
			{
				_location = point;
				PositionCloseButton();
			}
		}

		internal string GetText()
		{
			if (_buttonType == TabButtonType.ChildForm)
			{
				return _childTab.Text;
			}
			return "";
		}

		internal bool GetIsVisible()
		{
			return _isVisible;
		}

		internal void SetIsVisible(bool isVisible)
		{
			_isVisible = isVisible;
		}

		internal ButtonState GetButtonState()
		{
			if (_isDisabled)
			{
				return ButtonState.Disabled;
			}
			if (_isPressed)
			{
				return ButtonState.Pressed;
			}
			if (IsSelected())
			{
				if (_isHot)
				{
					return ButtonState.Pressed;
				}
				return ButtonState.Selected;
			}
			if (_isHot)
			{
				return ButtonState.Hot;
			}
			return ButtonState.Normal;
		}

		internal ButtonState GetStoredButtonState()
		{
			if (_isDisabled)
			{
				return ButtonState.Disabled;
			}
			if (_isPressed)
			{
				return ButtonState.Pressed;
			}
			if (_isSelected)
			{
				if (_isHot)
				{
					return ButtonState.Pressed;
				}
				return ButtonState.Selected;
			}
			if (_isHot)
			{
				return ButtonState.Hot;
			}
			return ButtonState.Normal;
		}

		private Size MeasureTextSize(Graphics graphics, Font font)
		{
			string text = GetText();
			if (text == null || text.Length == 0)
			{
				text = MeasureSampleText;
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetWordWrap(false);
			Size size = new Size((int)textLayoutRenderer.MeasureText(graphics, text, font, 0).Width + 1, font.Height + 1);
			textLayoutRenderer.ResetCache();
			return size;
		}

		private static int CalcTabOverlap(int slantWidth)
		{
			return -(slantWidth * 3 / 4) + TabOverlapOffset;
		}

		internal int MeasureTabHeight(Graphics graphics, bool useSlantedTabs, FormatStyle uiFormatStyle, Font baseFont)
		{
			if (_buttonType != TabButtonType.ChildForm)
			{
				return DefaultButtonSize;
			}
			Font font = uiFormatStyle.ResolveFont(baseFont);
			Size textSize = MeasureTextSize(graphics, font);
			Size imageSize = Size.Empty;
			if (HasImage())
			{
				imageSize = GetImageSize();
			}
			int height = textSize.Height;
			if (height < imageSize.Height)
			{
				height = imageSize.Height;
			}
			height += TabVerticalPadding;
			if (useSlantedTabs)
			{
				_slantWidth = height;
				height += SelectedTabExtraHeight;
				_tabOverlap = CalcTabOverlap(_slantWidth);
			}
			else {
				_slantWidth = 0;
			}
			return height;
		}

		internal Size MeasureTabSize(Graphics graphics, bool useSlantedTabs, FormatStyle uiFormatStyle, Font baseFont, int fixedHeight, bool isSelectedTab)
		{
			if (_buttonType != TabButtonType.ChildForm)
			{
				return new Size(DefaultButtonSize, DefaultButtonSize);
			}
			Font font = uiFormatStyle.ResolveFont(baseFont);
			Size textSize = MeasureTextSize(graphics, font);
			if (textSize.Width < MinTextWidth)
			{
				textSize.Width = MinTextWidth;
			}
			Size imageSize = Size.Empty;
			if (HasImage())
			{
				imageSize = GetImageSize();
			}
			int width = imageSize.Width + ContentPadding;
			if (imageSize.Width > 0)
			{
				width += ContentPadding;
			}
			if ((_closeButtonOnTab && isSelectedTab))
			{
				width += CloseButtonSlotWidth;
				if (_closeButton == null)
				{
					_closeButton = new MdiTabItem(TabButtonType.Close, _childTab, _parentControl, _useCompatibleTextRendering, closeButtonOnTab: false);
				}
			}
			width += textSize.Width;
			int height = textSize.Height;
			if (height < imageSize.Height)
			{
				height = imageSize.Height;
			}
			height += TabVerticalPadding;
			if (fixedHeight == -1)
			{
				fixedHeight = ((!useSlantedTabs) ? height : (height + SelectedTabExtraHeight));
			}
			if (useSlantedTabs)
			{
				_slantWidth = fixedHeight - SelectedTabExtraHeight;
				_tabOverlap = CalcTabOverlap(_slantWidth);
				width += _slantWidth;
			}
			else
			{
				_slantWidth = 0;
			}
			return new Size(width, fixedHeight);
		}

		private bool HasImage()
		{
			if (_buttonType == TabButtonType.ChildForm)
			{
				return !_childTab.CreateImageSource().IsEmpty();
			}
			return false;
		}

		internal Size GetItemSize()
		{
			return _itemSize;
		}

		internal void SetItemSize(Size size)
		{
			if (_itemSize != size)
			{
				_itemSize = size;
				PositionCloseButton();
			}
		}

		internal Rectangle GetBounds()
		{
			return new Rectangle(_location, _itemSize);
		}

		internal Rectangle GetBodyBounds()
		{
			return new Rectangle(_location.X + _slantWidth, _location.Y, _itemSize.Width - _slantWidth, _itemSize.Height);
		}

		internal int GetTabOverlap()
		{
			return _tabOverlap;
		}

		internal Size GetImageSize()
		{
			return _childTab.GetImageSize();
		}

		internal TabButtonType GetButtonType()
		{
			return _buttonType;
		}

		internal bool HitTest(bool isSlanted, int x, int y)
		{
			return HitTestBounds(GetBounds(), isSlanted, x, y);
		}

		internal bool HitTestBounds(Rectangle bounds, bool isSlanted, int x, int y)
		{
			if (bounds.IsEmpty)
			{
				return false;
			}
			if (isSlanted)
			{
				if (!bounds.Contains(x, y))
				{
					return false;
				}
				if (y < TabTopInset)
				{
					return false;
				}
				Rectangle rectangle = bounds;
				rectangle.Y += TabTopInset;
				rectangle.Height -= TabTopInset;
				rectangle.Width = rectangle.Height;
				if (rectangle.Contains(x, y))
				{
					x -= rectangle.X;
					y -= rectangle.Y;
					if (x + y > rectangle.Width)
					{
						return true;
					}
					return false;
				}
				return true;
			}
			return bounds.Contains(x, y);
		}

		private void PositionCloseButton()
		{
			if (_closeButton != null)
			{
				Rectangle rectangle = new Rectangle(_location, _itemSize);
				_closeButton.SetLocation(new Point(rectangle.Right - CloseButtonRightOffset, rectangle.Y));
			}
		}
	}
}
