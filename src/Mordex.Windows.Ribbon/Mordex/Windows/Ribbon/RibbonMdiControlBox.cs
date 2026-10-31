using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonMdiControlBox
	{
		private RibbonLayoutItem _minimizeButton;

		private RibbonLayoutItem _restoreButton;

		private RibbonLayoutItem _closeButton;

		private RibbonLayoutItem _hotButton;

		private MxRibbon _ribbon;

		private Rectangle _bounds;

		public RibbonMdiControlBox(MxRibbon ribbon)
		{
			_ribbon = ribbon;
			_restoreButton = new RibbonLayoutItem(ribbon);
			_minimizeButton = new RibbonLayoutItem(ribbon);
			_closeButton = new RibbonLayoutItem(ribbon);
		}

		internal AccessibleObject GetRestoreAccessibleObject()
		{
			if (_restoreButton != null)
			{
				return _restoreButton.AccessibilityObject;
			}
			return null;
		}

		internal AccessibleObject GetMinimizeAccessibleObject()
		{
			if (_minimizeButton != null)
			{
				return _minimizeButton.AccessibilityObject;
			}
			return null;
		}

		internal AccessibleObject GetCloseAccessibleObject()
		{
			if (_closeButton != null)
			{
				return _closeButton.AccessibilityObject;
			}
			return null;
		}

		public void SetHotButton(RibbonLayoutItem newHotButton)
		{
			if (_hotButton != newHotButton)
			{
				if (_hotButton != null)
				{
					_hotButton.Hot = false;
				}
				_hotButton = newHotButton;
				if (_hotButton != null)
				{
					_hotButton.Hot = true;
				}
				_ribbon.Invalidate(_bounds);
				if (_ribbon.IsGlassCaptionActive())
				{
					_ribbon.RefreshWindowFrame(true);
				}
			}
		}

		public bool HandleMouseUp(MouseEventArgs eventArgs)
		{
			if (_restoreButton.Pushed)
			{
				_restoreButton.Pushed = false;
				if (_restoreButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_ribbon.RestoreAllMdiChildren();
					return true;
				}
			}
			if (_minimizeButton.Pushed)
			{
				_minimizeButton.Pushed = false;
				if (_minimizeButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_ribbon.MinimizeMdiChild();
					return true;
				}
			}
			if (_closeButton.Pushed)
			{
				_closeButton.Pushed = false;
				if (_closeButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_ribbon.CloseMdiChild();
					return true;
				}
			}
			return false;
		}

		public bool HandleMouseDown(MouseEventArgs eventArgs)
		{
			if (_bounds.Contains(eventArgs.X, eventArgs.Y) && eventArgs.Button == MouseButtons.Left)
			{
				RibbonLayoutItem pressedButton = GetButtonAt(eventArgs.X, eventArgs.Y);
				if (pressedButton != null && pressedButton._isEnabled)
				{
					pressedButton.Pushed = true;
					_ribbon.Invalidate(_bounds);
					return true;
				}
			}
			return false;
		}

		public bool SetHotButtonAt(Point point)
		{
			if (_bounds.Contains(point.X, point.Y))
			{
				RibbonLayoutItem button = GetButtonAt(point.X, point.Y);
				if (button != null && !button._isEnabled)
				{
					button = null;
				}
				SetHotButton(button);
				return button != null;
			}
			SetHotButton(null);
			return false;
		}

		public RibbonLayoutItem GetButtonAt(int x, int y)
		{
			Rectangle bounds = _restoreButton._bounds;
			bounds.Inflate(1, 1);
			if (bounds.Contains(x, y))
			{
				return _restoreButton;
			}
			bounds = _closeButton._bounds;
			bounds.Inflate(1, 1);
			if (bounds.Contains(x, y))
			{
				return _closeButton;
			}
			bounds = _minimizeButton._bounds;
			bounds.Inflate(1, 1);
			if (bounds.Contains(x, y))
			{
				return _minimizeButton;
			}
			return null;
		}

		public void LayoutButtons(Rectangle rectangle, int minLeft)
		{
			int y = rectangle.Y;
			int left = rectangle.X;
			int buttonSize = rectangle.Width / 3;
			if (left < minLeft)
			{
				left = minLeft;
			}
			_bounds = new Rectangle(left, y, rectangle.Width, rectangle.Height);
			_minimizeButton._bounds = new Rectangle(left, y, buttonSize, buttonSize);
			left += buttonSize;
			_restoreButton._bounds = new Rectangle(left, y, buttonSize, buttonSize);
			left += buttonSize;
			_closeButton._bounds = new Rectangle(left, y, buttonSize, buttonSize);
			if (_ribbon.GetMdiChildForm() == null)
			{
				return;
			}
			if (!_ribbon.GetMdiChildForm().ControlBox)
			{
				_closeButton._isEnabled = false;
				_restoreButton._isEnabled = false;
				_minimizeButton._isEnabled = false;
				return;
			}
			_closeButton._isEnabled = true;
			if (!_ribbon.GetMdiChildForm().MinimizeBox)
			{
				_minimizeButton._isEnabled = false;
			}
			else
			{
				_minimizeButton._isEnabled = true;
			}
			if (!_ribbon.GetMdiChildForm().MaximizeBox)
			{
				_restoreButton._isEnabled = false;
			}
			else
			{
				_restoreButton._isEnabled = true;
			}
		}

		internal void PaintOffice2010Buttons(Graphics graphics)
		{
			RenderContent(graphics, _minimizeButton);
			RibbonChromePainter.DrawMinimizeGlyphFlat(graphics, Rectangle.Inflate(_minimizeButton._bounds, -2, -2), _ribbon._colorScheme, true, false, false, _minimizeButton._isEnabled);
			RenderContent(graphics, _restoreButton);
			RibbonChromePainter.DrawRestoreGlyphFlat(graphics, Rectangle.Inflate(_restoreButton._bounds, -2, -2), _ribbon._colorScheme, true, false, false);
			RenderContent(graphics, _closeButton);
			RibbonChromePainter.DrawCloseButtonFlat(graphics, Rectangle.Inflate(_closeButton._bounds, -2, -2), _ribbon._colorScheme, true, false, false);
		}

		private void RenderContent(Graphics graphics, RibbonLayoutItem button)
		{
			if (button.Hot)
			{
				if (button.Pushed)
				{
					RibbonChromePainter.DrawBackground(graphics, button._bounds, _ribbon._colorScheme, true);
				}
				else
				{
					RibbonChromePainter.DrawHotButtonBackground(graphics, button._bounds, _ribbon._colorScheme, true);
				}
			}
		}
	}
}
