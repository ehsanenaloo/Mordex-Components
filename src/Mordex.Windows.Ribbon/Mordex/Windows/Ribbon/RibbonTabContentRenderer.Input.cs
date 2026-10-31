using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabContentRenderer
	{
		public HitTest GetHitTest(int pointX, int pointY)
		{
			if (_bounds.Contains(pointX, pointY))
			{
				if (_leftScrollButton != null && _leftScrollButton._bounds.Contains(pointX, pointY))
				{
					return HitTest.RibbonScrollButton;
				}
				if (_rightScrollButton != null && _rightScrollButton._bounds.Contains(pointX, pointY))
				{
					return HitTest.RibbonScrollButton;
				}
				RibbonGroupBoxRenderer ribbonRenderer = FindGroupRendererAt(pointX, pointY);
				if (ribbonRenderer != null)
				{
					return HitTest.Command;
				}
				return HitTest.RibbonBackground;
			}
			return HitTest.None;
		}

		public bool HandleMouseUp(MouseEventArgs eventArgs)
		{
			if (_isMousePressed)
			{
				_isMousePressed = false;
				FindGroupRendererAt(eventArgs.X, eventArgs.Y)?.OnMouseUp(eventArgs);
				return true;
			}
			return false;
		}

		public bool HandleMouseDown(MouseEventArgs eventArgs)
		{
			if (_leftScrollButton != null && _leftScrollButton._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				StartScrollLeft();
				_isMousePressed = true;
				SetHotGroup(null);
				return true;
			}
			if (_rightScrollButton != null && _rightScrollButton._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				StartScrollRight();
				_isMousePressed = true;
				SetHotGroup(null);
				return true;
			}
			RibbonGroupBoxRenderer ribbonRenderer = FindGroupRendererAt(eventArgs.X, eventArgs.Y);
			if (ribbonRenderer != null)
			{
				_isMousePressed = true;
				ribbonRenderer.OnMouseDown(eventArgs);
				return true;
			}
			return false;
		}

		public bool HandleMouseMove(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.None && !_isMousePressed)
			{
				return false;
			}
			if (_isScrollTimerRunning)
			{
				return true;
			}
			if (_leftScrollButton != null && _leftScrollButton._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				SetHotScrollButton(_leftScrollButton);
				return true;
			}
			if (_rightScrollButton != null && _rightScrollButton._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				SetHotScrollButton(_rightScrollButton);
				return true;
			}
			SetHotScrollButton(null);
			RibbonGroupBoxRenderer ribbonRenderer = FindGroupRendererAt(eventArgs.X, eventArgs.Y);
			SetHotGroup(ribbonRenderer);
			if (ribbonRenderer != null)
			{
				ribbonRenderer.OnMouseMove(eventArgs);
				return true;
			}
			return false;
		}

		public RibbonGroupBoxRenderer FindGroupRendererForCommand(CommandBase command)
		{
			if (_groupRenderers != null)
			{
				foreach (RibbonGroupBoxRenderer item in _groupRenderers)
				{
					if (item.GetRibbonGroup().Commands.Contains(command))
					{
						return item;
					}
				}
			}
			return null;
		}

		public RibbonGroupBoxRenderer FindGroupRenderer(RibbonGroup ribbonGroup)
		{
			if (_groupRenderers != null)
			{
				foreach (RibbonGroupBoxRenderer item in _groupRenderers)
				{
					if (item.GetRibbonGroup() == ribbonGroup)
					{
						return item;
					}
				}
			}
			return null;
		}
	}
}
