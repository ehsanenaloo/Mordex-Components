using System;
using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>The parts of a custom window frame, in window coordinates (the origin is the top left corner of the window).</summary>
	public sealed class FormFrameMetrics
	{
		/// <summary>The client area, as the frame reports it to Windows.</summary>
		public Rectangle Client { get; internal set; }

		/// <summary>The caption, from the top of the visible window to the client area.</summary>
		public Rectangle Caption { get; internal set; }

		public Rectangle Icon { get; internal set; }

		public Rectangle Title { get; internal set; }

		public Rectangle MinimizeButton { get; internal set; }

		public Rectangle MaximizeButton { get; internal set; }

		public Rectangle CloseButton { get; internal set; }

		/// <summary>The width of the frame on the left, right and bottom. 0 when the window is maximized.</summary>
		public int Border { get; internal set; }

		/// <summary>The part of a maximized window that lies outside the screen on each side.</summary>
		public int Overhang { get; internal set; }

		public bool Maximized { get; internal set; }

		/// <summary>The window rectangle that is visible: the window minus the overhang of a maximized window.</summary>
		public Rectangle Visible { get; internal set; }

		internal bool Mirrored { get; set; }

		internal static FormFrameMetrics Compute(Size window, int dpi, int overhang, FormFrameStyle style, bool maximized, bool controlBox, bool minimizeBox, bool maximizeBox, bool showIcon, bool mirrored, int leftInset = 0, bool keepIcon = false)
		{
			float scale = dpi / 96f;
			int S(int value) => (int)Math.Round(value * scale);
			FormFrameMetrics m = new FormFrameMetrics { Maximized = maximized, Mirrored = mirrored, Overhang = maximized ? overhang : 0 };
			Rectangle visible = maximized
				? new Rectangle(overhang, overhang, Math.Max(0, window.Width - overhang * 2), Math.Max(0, window.Height - overhang * 2))
				: new Rectangle(Point.Empty, window);
			int border = maximized ? 0 : S(style.BorderWidth);
			int caption = S(style.CaptionHeight);
			if (maximized)
			{
				caption = Math.Max(S(20), caption - S(style.BorderWidth));
			}
			m.Visible = visible;
			m.Border = border;
			m.Caption = new Rectangle(visible.X, visible.Y, visible.Width, caption);
			m.Client = new Rectangle(visible.X + border, visible.Y + caption, Math.Max(0, visible.Width - border * 2), Math.Max(0, visible.Height - caption - border));

			int right = visible.Right - border;
			int top = visible.Y + (maximized ? 0 : border);
			int barHeight = caption - (maximized ? 0 : border);
			Rectangle close = Rectangle.Empty, max = Rectangle.Empty, min = Rectangle.Empty;
			if (controlBox)
			{
				switch (style.Kind)
				{
				case FormFrameKind.Luna:
				{
					int size = S(21);
					int y = top + (barHeight - size) / 2;
					int x = right - S(5) - size;
					close = new Rectangle(x, y, size, size);
					x -= size + S(2);
					max = new Rectangle(x, y, size, size);
					x -= size + S(2);
					min = new Rectangle(x, y, size, size);
					break;
				}
				case FormFrameKind.Classic:
				{
					int w = S(16);
					int h = S(14);
					int y = top + (barHeight - h) / 2 - S(1);
					int x = right - S(2) - w;
					close = new Rectangle(x, y, w, h);
					x -= w + S(2);
					max = new Rectangle(x, y, w, h);
					x -= w;
					min = new Rectangle(x, y, w, h);
					break;
				}
				case FormFrameKind.Glass:
				{
					int h = S(19);
					int y = visible.Y + (maximized ? 0 : S(1));
					int x = right - S(4) - S(44);
					close = new Rectangle(x, y, S(44), h);
					x -= S(28);
					max = new Rectangle(x, y, S(28), h);
					x -= S(28);
					min = new Rectangle(x, y, S(28), h);
					break;
				}
				default:
				{
					int w = S(36);
					int y = visible.Y + (maximized ? 0 : 1);
					int h = barHeight - (maximized ? 0 : 1);
					int x = right - S(44);
					close = new Rectangle(x, y, S(44), h);
					x = close.X - w;
					max = new Rectangle(x, y, w, h);
					x -= w;
					min = new Rectangle(x, y, w, h);
					break;
				}
				}
				if (!maximizeBox && !minimizeBox)
				{
					max = Rectangle.Empty;
					min = Rectangle.Empty;
				}
				else
				{
					if (!minimizeBox)
					{
						min = Rectangle.Empty;
					}
					if (!maximizeBox)
					{
						max = Rectangle.Empty;
					}
				}
			}
			int left = visible.X + border + S(6);
			bool inset = leftInset > 0 && !mirrored;
			if ((!inset || keepIcon) && controlBox && showIcon)
			{
				int iconSize = S(16);
				m.Icon = new Rectangle(visible.X + border + S(style.Kind == FormFrameKind.Classic ? 2 : 6), top + (barHeight - iconSize) / 2, iconSize, iconSize);
				left = m.Icon.Right + S(5);
			}
			if (inset)
			{
				left = Math.Max(left, visible.X + leftInset);
			}
			int firstButton = right;
			foreach (Rectangle button in new[] { close, max, min })
			{
				if (!button.IsEmpty)
				{
					firstButton = Math.Min(firstButton, button.X);
				}
			}
			m.CloseButton = close;
			m.MaximizeButton = max;
			m.MinimizeButton = min;
			m.Title = new Rectangle(left, top, Math.Max(0, firstButton - S(6) - left), barHeight);
			if (mirrored)
			{
				m.Mirror(visible);
			}
			return m;
		}

		private void Mirror(Rectangle visible)
		{
			Icon = MirrorRect(Icon, visible);
			Title = MirrorRect(Title, visible);
			MinimizeButton = MirrorRect(MinimizeButton, visible);
			MaximizeButton = MirrorRect(MaximizeButton, visible);
			CloseButton = MirrorRect(CloseButton, visible);
		}

		private static Rectangle MirrorRect(Rectangle rect, Rectangle visible)
		{
			if (rect.IsEmpty)
			{
				return rect;
			}
			return new Rectangle(visible.X + visible.Right - rect.Right, rect.Y, rect.Width, rect.Height);
		}
	}
}
