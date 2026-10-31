using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class DockCaptionRenderer
	{
		internal class CaptionItem
		{
			private DockCaptionRenderer _owner;

			private string _accessibleName;

			private AccessibleRole _accessibleRole;

			private string _accessibleDescription;

			internal PanelButtonType _buttonType;

			internal Rectangle _bounds;

			internal Control _control;

			private bool _isHot;

			private bool _isPressed;

			private AccessibleObject _accessibleObject;

			internal CaptionItem(DockCaptionRenderer panelCaptionItem, string accessibleName, AccessibleRole accessibleRole, string accessibleDescription)
			{
				_owner = panelCaptionItem;
				_accessibleName = accessibleName;
				_accessibleRole = accessibleRole;
				_accessibleDescription = accessibleDescription;
			}

			public string GetAccessibleName()
			{
				return _accessibleName;
			}

			public AccessibleRole GetAccessibleRole()
			{
				return _accessibleRole;
			}

			public string GetAccessibleDescription()
			{
				return _accessibleDescription;
			}

			public AccessibleStates GetAccessibleStates()
			{
				AccessibleStates accessibleStates = AccessibleStates.Focusable;
				if (GetIsPressed())
				{
					accessibleStates |= AccessibleStates.Pressed;
				}
				if (GetIsHot())
				{
					accessibleStates |= AccessibleStates.HotTracked;
				}
				return accessibleStates;
			}

			public void Click()
			{
				if (_owner != null)
				{
					_owner.RaiseItemClick(this);
				}
			}

			internal AccessibleObject GetAccessibleObject()
			{
				if (_accessibleObject == null)
				{
					_accessibleObject = new DockCaptionButtonAccessibleObject(this);
				}
				return _accessibleObject;
			}

			internal bool GetIsHot()
			{
				return _isHot;
			}

			internal void SetIsHot(bool isHot)
			{
				if (_isHot != isHot)
				{
					_isHot = isHot;
					InvalidateItem();
				}
			}

			internal bool GetIsPressed()
			{
				return _isPressed;
			}

			internal void SetIsPressed(bool isPressed)
			{
				if (_isPressed != isPressed)
				{
					_isPressed = isPressed;
					InvalidateItem();
				}
			}

			internal bool ContainsPoint(int x, int y)
			{
				return _bounds.Contains(x, y);
			}

			private void InvalidateItem()
			{
				if (_control == null)
				{
					return;
				}
				if (_control is DockPanelBase)
				{
					DockPanelBase uiPanelBase = _control as DockPanelBase;
					if (uiPanelBase.ContainerCaptionControl != null)
					{
						uiPanelBase.ContainerCaptionControl.Invalidate();
					}
				}
				_control.Invalidate(_bounds);
			}
		}

		private const int MaxButtonSize = 18;

		private const int DragHandleWidth = 8;

		private const int CaptionTextLeftPadding = 4;

		private const int CloseImageIndex = 0;

		private const int CaptionButtonImageIndex = 5;

		internal DockPanelBase _panel;

		private Rectangle _captionBounds;

		internal CaptionItem _dragHandle;

		internal CaptionItem _closeButton;

		internal CaptionItem _autoHideButton;

		internal CaptionItem _captionButton;

		private int _captionHeight;

		private AccessibleObject _accessibleObject;

		private CaptionItem _pressedItem;

		private CaptionItem _hotItem;

		internal PanelCaptionStyle _captionStyle;

		internal DockCaptionRenderer(DockPanelBase panel)
		{
			_panel = panel;
			LayoutItems();
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new DockAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal CaptionItem GetPressedItem()
		{
			return _pressedItem;
		}

		internal void SetPressedItem(CaptionItem pressedItem)
		{
			if (pressedItem == _captionButton)
			{
				pressedItem = null;
			}
			if (_pressedItem != pressedItem)
			{
				if (_pressedItem != null)
				{
					_pressedItem.SetIsPressed(false);
				}
				_pressedItem = pressedItem;
				if (_pressedItem != null)
				{
					_pressedItem.SetIsPressed(true);
				}
			}
		}

		internal CaptionItem GetHotItem()
		{
			return _hotItem;
		}

		internal void SetHotItem(CaptionItem hotItem)
		{
			if (_hotItem == hotItem)
			{
				return;
			}
			if (_hotItem != null)
			{
				_hotItem.SetIsHot(false);
			}
			_hotItem = hotItem;
			if (_hotItem != null)
			{
				switch (_hotItem._buttonType)
				{
				case PanelButtonType.CloseButton:
					_panel.ApplyToolTipText(_panel.PanelManager.BuiltInTexts[PanelsBuiltInText.ClosePanelToolTip]);
					break;
				case PanelButtonType.AutoHideButton:
					_panel.ApplyToolTipText(_panel.PanelManager.BuiltInTexts[PanelsBuiltInText.AutoHidePanelToolTip]);
					break;
				case PanelButtonType.CaptionButton:
					_panel.ApplyToolTipText(_panel.PanelManager.BuiltInTexts[PanelsBuiltInText.TaskPaneSelectorToolTip]);
					break;
				}
				_hotItem.SetIsHot(true);
			}
			else
			{
				_panel.ApplyToolTipText("");
			}
		}

		private CaptionItem HitTestItem(int x, int y)
		{
			if (_closeButton != null && _closeButton.ContainsPoint(x, y))
			{
				return _closeButton;
			}
			if (_autoHideButton != null && _autoHideButton.ContainsPoint(x, y))
			{
				return _autoHideButton;
			}
			if (_captionButton != null && _captionButton.ContainsPoint(x, y))
			{
				return _captionButton;
			}
			if (_dragHandle != null && _dragHandle.ContainsPoint(x, y))
			{
				return _dragHandle;
			}
			return null;
		}

		private void DrawButton(Graphics graphics, FormatStyle normalStyle, Font font, CaptionItem item, Image buttonImage)
		{
			if (_panel.PanelManager != null)
			{
				ButtonState buttonState = ButtonState.Normal;
				if (!_panel.Enabled)
				{
					buttonState = ButtonState.Disabled;
				}
				else if (item.GetIsPressed())
				{
					buttonState = ButtonState.Pressed;
				}
				else if (item.GetIsHot())
				{
					buttonState = ButtonState.Hot;
				}
				FormatStyle uiFormatStyle;
				if (buttonState != ButtonState.Normal)
				{
					uiFormatStyle = _panel.PanelManager.GetCaptionStyle(PanelCaptionStyle.Standard, false);
					uiFormatStyle = _panel.GetCaptionButtonFormatStyle(uiFormatStyle, buttonState);
				}
				else
				{
					uiFormatStyle = normalStyle;
				}
				_panel.PanelManager.GetPanelRenderer().DrawPanelButton(graphics, _captionStyle, _panel, item._buttonType, item._bounds, buttonState, font, uiFormatStyle, buttonImage);
				uiFormatStyle.DisposeResources();
			}
		}

		internal bool HandleMouseMove(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.None)
			{
				SetHotItem(HitTestItem(eventArgs.X, eventArgs.Y));
			}
			else if (GetPressedItem() != null)
			{
				GetPressedItem().SetIsPressed(GetPressedItem().ContainsPoint(eventArgs.X, eventArgs.Y));
				return true;
			}
			return false;
		}

		internal bool HandleMouseUp(MouseEventArgs eventArgs)
		{
			if (GetPressedItem() != null)
			{
				if (GetPressedItem().ContainsPoint(eventArgs.X, eventArgs.Y))
				{
					RaiseItemClick(GetPressedItem());
				}
				SetPressedItem(null);
				SetHotItem(null);
				return true;
			}
			return false;
		}

		internal void ClearHotItem()
		{
			SetHotItem(null);
		}

		internal bool HandleMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				CaptionItem item = HitTestItem(eventArgs.X, eventArgs.Y);
				if (item != null && item != _dragHandle)
				{
					RaiseItemMouseDown(item);
					SetPressedItem(item);
					return true;
				}
			}
			return false;
		}

		internal void LayoutItems()
		{
			_captionBounds = _panel.DisplayRectangle;
			if (_panel.HasPanelBorder() && _panel.HasCaptionBorder())
			{
				_captionBounds.Inflate(1, 1);
			}
			_captionBounds.Height = _panel.GetPanelCaptionHeight();
			if (_panel.ParentGroup != null)
			{
				BorderEdges borderEdges = _panel.ParentGroup.GetChildBorderEdges(_panel);
				if ((borderEdges & BorderEdges.Top) == 0)
				{
					_captionBounds.Y--;
				}
				if ((borderEdges & BorderEdges.Left) == 0)
				{
					_captionBounds.X--;
					_captionBounds.Width++;
				}
				if ((borderEdges & BorderEdges.Right) == 0)
				{
					_captionBounds.Width++;
				}
				if ((borderEdges & BorderEdges.Bottom) == 0)
				{
					_captionBounds.Height++;
				}
			}
			int freeRight = _captionBounds.Right - 2;
			int buttonSize = (_captionBounds.Height > MaxButtonSize) ? MaxButtonSize : (_captionBounds.Height - 2);
			_captionHeight = _panel.PreferredCaptionHeight;
			_closeButton = _panel.IsCloseButtonShown() ? CreateRightAlignedButton("Close", PanelButtonType.CloseButton, ref freeRight, buttonSize) : null;
			_autoHideButton = _panel.IsAutoHideButtonShown() ? CreateRightAlignedButton("Auto Hide", PanelButtonType.AutoHideButton, ref freeRight, buttonSize) : null;
			if (_panel.HasDragHandle())
			{
				_dragHandle = new CaptionItem(this, "Drag", AccessibleRole.Indicator, "");
				_dragHandle._buttonType = PanelButtonType.DragHandle;
				_dragHandle._bounds = new Rectangle(_captionBounds.X, _captionBounds.Y - 1, DragHandleWidth, _captionBounds.Height + 2);
			}
			else
			{
				_dragHandle = null;
			}
			if (_panel.IsTextShownAsButton())
			{
				buttonSize++;
				_captionButton = new CaptionItem(this, _panel.Text, AccessibleRole.ButtonMenu, "");
				_captionButton._buttonType = PanelButtonType.CaptionButton;
				_captionButton._bounds = Rectangle.FromLTRB(_captionBounds.Left + DragHandleWidth, _captionBounds.Y + 1, freeRight - 2, _captionBounds.Top + 1 + buttonSize);
				_captionButton._bounds.Y += (_captionBounds.Height - _captionButton._bounds.Height) / 2 - 1;
				_captionButton._control = _panel;
			}
			else
			{
				_captionButton = null;
			}
		}

		// Places a square button at the right end of the free caption area and moves the free edge left of it.
		private CaptionItem CreateRightAlignedButton(string name, PanelButtonType buttonType, ref int rightEdge, int buttonSize)
		{
			CaptionItem button = new CaptionItem(this, name, AccessibleRole.PushButton, "");
			button._buttonType = buttonType;
			button._bounds = new Rectangle(rightEdge - buttonSize - 1, _captionBounds.Y + 1, buttonSize, buttonSize);
			rightEdge -= buttonSize + 2;
			button._bounds.Y += (_captionHeight - button._bounds.Height) / 2 - 1;
			button._control = _panel;
			return button;
		}

		internal void RaiseItemClick(CaptionItem item)
		{
			if (_panel != null)
			{
				_panel.HandleCaptionButtonClick(item._buttonType);
			}
		}

		private void RaiseItemMouseDown(CaptionItem item)
		{
			if (_panel != null)
			{
				_panel.HandleCaptionButtonDown(item._bounds, item._buttonType);
			}
		}

		internal Rectangle GetCaptionBounds()
		{
			return _captionBounds;
		}

		internal void DrawCaption(Graphics graphics, bool drawCaptionButtonBorder, int offsetX, int offsetY)
		{
			_captionStyle = _panel.GetEffectiveCaptionStyle();
			FormatStyle uiFormatStyle = _panel.GetCaptionFormatStyle(_panel.IsFloatingRoot(), _panel.Enabled);
			Font font = _panel.GetCaptionFont(uiFormatStyle);
			DockManagerRendererBase managerRenderer = _panel.PanelManager.GetPanelRenderer();
			_captionBounds.Offset(offsetX, offsetY);
			if (_closeButton != null)
			{
				_closeButton._bounds.Offset(offsetX, offsetY);
			}
			if (_autoHideButton != null)
			{
				_autoHideButton._bounds.Offset(offsetX, offsetY);
			}
			if (_captionButton == null)
			{
				int right = _captionBounds.Right;
				if (_closeButton != null)
				{
					right = _closeButton._bounds.X - 2;
				}
				if (_autoHideButton != null)
				{
					right = _autoHideButton._bounds.X - 2;
				}
				Rectangle textBounds = Rectangle.FromLTRB(_captionBounds.X + CaptionTextLeftPadding, _captionBounds.Y, right, _captionBounds.Y + _captionHeight);
				managerRenderer.DrawPanelCaptionBackground(graphics, _captionStyle, _panel, _captionBounds, uiFormatStyle);
				FormatStyle infoTextStyle = null;
				if (_panel.InfoText.Length > 0)
				{
					infoTextStyle = _panel.GetInfoTextFormatStyle(_panel.IsFloatingRoot());
				}
				managerRenderer.DrawPanelCaptionForeground(graphics, _captionStyle, _panel, textBounds, font, uiFormatStyle, infoTextStyle);
			}
			else
			{
				managerRenderer.DrawPanelCaptionBackground(graphics, _captionStyle, _panel, _captionBounds, uiFormatStyle);
			}
			if (_panel.HasCaptionBorder())
			{
				managerRenderer.DrawCaptionBorder(graphics, _panel, _captionBounds);
			}
			if (_closeButton != null)
			{
				int closeImageIndex = (CloseImageIndex);
				DrawButton(graphics, uiFormatStyle, font, _closeButton, MxDockManager.GetButtonImage(closeImageIndex, _panel.PanelManager.DefaultPanelSettings));
			}
			if (_autoHideButton != null)
			{
				int autoHideImageIndex = ((!_panel.IsAutoHideEffective()) ? _panel.GetDockedButtonImageIndex() : _panel.GetAutoHideButtonImageIndex());
				DrawButton(graphics, uiFormatStyle, font, _autoHideButton, MxDockManager.GetButtonImage(autoHideImageIndex, _panel.PanelManager.DefaultPanelSettings));
			}
			if (_captionButton != null)
			{
				DrawButton(graphics, uiFormatStyle, font, _captionButton, MxDockManager.GetButtonImage(CaptionButtonImageIndex,_panel.PanelManager.DefaultPanelSettings));
				if (drawCaptionButtonBorder)
				{
					Rectangle bounds = _captionButton._bounds;
					managerRenderer.DrawCaptionBorder(graphics, _panel, bounds);
				}
			}
			if (_dragHandle != null)
			{
				managerRenderer.DrawGrabItem(graphics, _dragHandle._bounds);
			}
			_captionBounds.Offset(-offsetX, -offsetY);
			if (_closeButton != null)
			{
				_closeButton._bounds.Offset(-offsetX, -offsetY);
			}
			if (_autoHideButton != null)
			{
				_autoHideButton._bounds.Offset(-offsetX, -offsetY);
			}
		}

		internal CaptionItem GetDragHandle()
		{
			return _dragHandle;
		}
	}
}
