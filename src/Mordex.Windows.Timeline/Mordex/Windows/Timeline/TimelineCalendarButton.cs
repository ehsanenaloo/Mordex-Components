using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline
{
	internal class TimelineCalendarButton
	{
		private int _arrowDirection;

		private Control _owner;

		public Rectangle _bounds;

		private bool _isPressed;

		private bool _isHovered;

		public TimelineCalendarButton(Control owner, int arrowDirection)
		{
			if (owner == null)
			{
				throw new ArgumentNullException();
			}
			_owner = owner;
			_arrowDirection = arrowDirection;
		}

		private void InvalidateButton()
		{
			if (_owner != null && _bounds.Width > 0)
			{
				_owner.Invalidate(_bounds);
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
				InvalidateButton();
			}
		}

		public bool IsHovered()
		{
			return _isHovered;
		}

		public void SetHovered(bool timelineRenderer2Boolean)
		{
			if (_isHovered != timelineRenderer2Boolean)
			{
				_isHovered = timelineRenderer2Boolean;
				InvalidateButton();
			}
		}

		public void DrawText(Graphics graphics, string text, Font font, ScrollBarStyle scrollBarStyle, bool isRightToLeft)
		{
			Rectangle rectangle = scrollBarStyle.DrawButtonBackground(graphics, _bounds, _isHovered, _isPressed);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.FormatFlags = StringFormatFlags.NoClip;
			if (isRightToLeft)
			{
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			graphics.DrawString(text, font, scrollBarStyle.GetControlTextBrush(), rectangle, stringFormat);
		}

		public bool ContainsPoint(int x, int integerArgument)
		{
			if (_bounds.Width == 0)
			{
				return false;
			}
			return _bounds.Contains(x, integerArgument);
		}
	}
}
