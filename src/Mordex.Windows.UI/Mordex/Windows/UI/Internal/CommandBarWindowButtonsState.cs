using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandBarWindowButtonsState : CommandLayoutState
	{
		private const int ButtonSize = 16;

		private const int ButtonGap = 1;

		internal bool _drawHorizontal = true;

		private MxCommandBar _commandBar;

		private FormatStyleLayoutState _minimizeButton;

		private FormatStyleLayoutState _closeButton;

		private FormatStyleLayoutState _restoreButton;

		private FormatStyleLayoutState _hoveredButton;

		internal CommandBarWindowButtonsState(MxCommandBar commandBar)
		{
			_commandBar = commandBar;
			_restoreButton = new FormatStyleLayoutState(_commandBar);
			_minimizeButton = new FormatStyleLayoutState(_commandBar);
			_closeButton = new FormatStyleLayoutState(_commandBar);
		}

		internal bool AreMinMaxButtonsAvailable()
		{
			if (_commandBar.GetActiveMdiChild() != null)
			{
				if (!_commandBar.GetActiveMdiChild().ControlBox)
				{
					return false;
				}
				if (!_commandBar.GetActiveMdiChild().MinimizeBox && !_commandBar.GetActiveMdiChild().MaximizeBox)
				{
					return false;
				}
				switch (_commandBar.GetActiveMdiChild().FormBorderStyle)
				{
				case FormBorderStyle.None:
				case FormBorderStyle.FixedToolWindow:
				case FormBorderStyle.SizableToolWindow:
					return false;
				default:
					return true;
				}
			}
			return true;
		}

		public MxCommandBar GetCommandBar()
		{
			return _commandBar;
		}

		public FormatStyleLayoutState GetMinimizeButton()
		{
			return _minimizeButton;
		}

		public FormatStyleLayoutState GetCloseButton()
		{
			return _closeButton;
		}

		public FormatStyleLayoutState GetRestoreButton()
		{
			return _restoreButton;
		}

		public FormatStyleLayoutState GetHoveredButton()
		{
			return _hoveredButton;
		}

		public void SetHoveredButton(FormatStyleLayoutState hoveredButton)
		{
			if (_hoveredButton == hoveredButton)
			{
				return;
			}
			if (_hoveredButton != null)
			{
				_hoveredButton.SetHot(false);
				_hoveredButton.SetPressed(false);
			}
			_hoveredButton = hoveredButton;
			if (_hoveredButton != null)
			{
				_hoveredButton.SetHot(true);
				if (_hoveredButton.MouseDown)
				{
					_hoveredButton.SetPressed(true);
				}
			}
		}

		public void HandleMouseUp(MouseEventArgs eventArgs)
		{
			if (_restoreButton.MouseDown)
			{
				_restoreButton.MouseDown = false;
				if (_restoreButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_commandBar.RestoreMdiChild();
				}
			}
			if (_minimizeButton.MouseDown)
			{
				_minimizeButton.MouseDown = false;
				if (_minimizeButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_commandBar.MinimizeMdiChild();
				}
			}
			if (_closeButton.MouseDown)
			{
				_closeButton.MouseDown = false;
				if (_closeButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_commandBar.CloseMdiChild();
				}
			}
		}

		public void HandleMouseDown(MouseEventArgs eventArgs)
		{
			FormatStyleLayoutState uiFormatStyleLayoutState = HitTestButton(eventArgs.X, eventArgs.Y);
			if (uiFormatStyleLayoutState != null && uiFormatStyleLayoutState.IsEnabled())
			{
				uiFormatStyleLayoutState.SetPressed(true);
				uiFormatStyleLayoutState.MouseDown = true;
			}
		}

		public string GetHoveredButtonToolTipText()
		{
			if (GetHoveredButton() == null || GetCommandBar() == null)
			{
				return "";
			}
			if (GetHoveredButton() == GetMinimizeButton())
			{
				return GetCommandBar().CommandManager.BuiltInTexts[CommandsBuiltInText.Minimize];
			}
			if (GetHoveredButton() == GetCloseButton())
			{
				return GetCommandBar().CommandManager.BuiltInTexts[CommandsBuiltInText.Close];
			}
			if (GetHoveredButton() == GetRestoreButton())
			{
				return GetCommandBar().CommandManager.BuiltInTexts[CommandsBuiltInText.Restore];
			}
			return "";
		}

		public bool HandleMouseMove(MouseEventArgs eventArgs)
		{
			bool result = false;
			FormatStyleLayoutState uiFormatStyleLayoutState = HitTestButton(eventArgs.X, eventArgs.Y);
			if (uiFormatStyleLayoutState != null && !uiFormatStyleLayoutState.IsEnabled())
			{
				uiFormatStyleLayoutState = null;
			}
			if (uiFormatStyleLayoutState != GetHoveredButton())
			{
				result = true;
			}
			SetHoveredButton(uiFormatStyleLayoutState);
			return result;
		}

		public FormatStyleLayoutState HitTestButton(int x, int y)
		{
			Rectangle rectangle = _restoreButton._bounds;
			rectangle.Inflate(ButtonGap, ButtonGap);
			if (rectangle.Contains(x, y) && _restoreButton.IsVisible())
			{
				return _restoreButton;
			}
			rectangle = _closeButton._bounds;
			rectangle.Inflate(ButtonGap, ButtonGap);
			if (rectangle.Contains(x, y) && _closeButton.IsVisible())
			{
				return _closeButton;
			}
			rectangle = _minimizeButton._bounds;
			rectangle.Inflate(ButtonGap, ButtonGap);
			if (rectangle.Contains(x, y) && _minimizeButton.IsVisible())
			{
				return _minimizeButton;
			}
			return null;
		}

		public void LayoutButtons(Rectangle rectangle, bool isHorizontal, int minOffset)
		{
			int buttonSize = ButtonSize;
			if (isHorizontal)
			{
				_bounds.X = rectangle.Right - _bounds.Width;
				int y = _bounds.Y + (_bounds.Height - buttonSize) / 2;
				int buttonX = _bounds.X;
				if (buttonX < minOffset)
				{
					buttonX = minOffset;
				}
				_minimizeButton._bounds = new Rectangle(buttonX, y, buttonSize, buttonSize);
				buttonX += buttonSize + ButtonGap;
				_restoreButton._bounds = new Rectangle(buttonX, y, buttonSize, buttonSize);
				buttonX += buttonSize + ButtonGap;
				_closeButton._bounds = new Rectangle(buttonX, y, buttonSize, buttonSize);
			}
			else
			{
				_bounds.Y = rectangle.Bottom - _bounds.Height;
				int buttonY = _bounds.Y;
				int x = _bounds.X + (_bounds.Width - buttonSize) / 2;
				if (buttonY < minOffset)
				{
					buttonY = minOffset;
				}
				_minimizeButton._bounds = new Rectangle(x, buttonY, buttonSize, buttonSize);
				buttonY += buttonSize + ButtonGap;
				_restoreButton._bounds = new Rectangle(x, buttonY, buttonSize, buttonSize);
				buttonY += buttonSize + ButtonGap;
				_closeButton._bounds = new Rectangle(x, buttonY, buttonSize, buttonSize);
			}
			if (_commandBar.GetActiveMdiChild() == null)
			{
				return;
			}
			Form form = GetCommandBar().GetActiveMdiChild();
			if (!form.ControlBox)
			{
				_closeButton.SetEnabled(false);
				_restoreButton.SetEnabled(false);
				_minimizeButton.SetEnabled(false);
				return;
			}
			_closeButton.SetEnabled(true);
			if (!form.MinimizeBox)
			{
				_minimizeButton.SetEnabled(false);
			}
			else
			{
				_minimizeButton.SetEnabled(true);
			}
			_restoreButton.SetEnabled(true);
			if (!AreMinMaxButtonsAvailable())
			{
				_minimizeButton.SetVisible(false);
				_restoreButton.SetVisible(false);
			}
		}

		internal void RenderContent(Graphics graphics, VisualStylePainter renderer, StateStyles uiStateStyles, Color color)
		{
			Image image = null;
			if (_minimizeButton.IsVisible())
			{
				DrawButton(graphics, _minimizeButton, renderer, uiStateStyles);
				image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Minimize());
				if (image != null)
				{
					RenderingHelper.DrawRecoloredImage(graphics, image, _minimizeButton._bounds, color);
				}
			}
			if (_restoreButton.IsVisible())
			{
				DrawButton(graphics, _restoreButton, renderer, uiStateStyles);
				image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Restore());
				if (image != null)
				{
					RenderingHelper.DrawRecoloredImage(graphics, image, _restoreButton._bounds, color);
				}
			}
			if (!_closeButton.IsVisible())
			{
				return;
			}
			DrawButton(graphics, _closeButton, renderer, uiStateStyles);
			image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Close());
			if (image != null)
			{
				RenderingHelper.DrawRecoloredImage(graphics, image, _closeButton._bounds, color);
			}
		}

		internal FormatStyle ResolveButtonFormatStyle(StateStyles uiStateStyles, bool isHot, bool isPressed, bool isEnabled)
		{
			if (!_commandBar.Enabled)
			{
				isEnabled = false;
			}
			FormatStyle uiFormatStyle = new FormatStyle(uiStateStyles.FormatStyle);
			if (!isEnabled)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.DisabledFormatStyle);
			}
			else if (isPressed && isHot)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.PressedFormatStyle);
			}
			else if (isPressed)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.SelectedFormatStyle);
			}
			else if (isHot)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.HotFormatStyle);
			}
			return uiFormatStyle;
		}

		private void DrawButton(Graphics graphics, FormatStyleLayoutState uiFormatStyleLayoutState, VisualStylePainter renderer, StateStyles uiStateStyles)
		{
			if (renderer != null)
			{
				renderer.DrawToolBarItem(graphics, uiFormatStyleLayoutState._bounds, uiFormatStyleLayoutState.IsEnabled(), uiFormatStyleLayoutState.IsHot(), uiFormatStyleLayoutState.IsPressed(), false);
				return;
			}
			FormatStyle uiFormatStyle = ResolveButtonFormatStyle(uiStateStyles, uiFormatStyleLayoutState.IsHot(), uiFormatStyleLayoutState.IsPressed(), uiFormatStyleLayoutState.IsEnabled());
			RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, _bounds, uiFormatStyleLayoutState._bounds);
			if (uiFormatStyleLayoutState.IsHot() || uiFormatStyleLayoutState.IsPressed())
			{
				Pen pen = GdiCache.Pen(uiFormatStyle.GetStateBorderColor(uiFormatStyleLayoutState.IsHot(), uiFormatStyleLayoutState.IsPressed()));
				RenderingHelper.DrawRectangleOutline(graphics, pen, uiFormatStyleLayoutState._bounds);
				pen = null;
			}
			uiFormatStyle.DisposeResources();
		}
	}
}
