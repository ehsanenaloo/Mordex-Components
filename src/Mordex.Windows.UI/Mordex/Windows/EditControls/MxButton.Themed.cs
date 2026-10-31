using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxButton : IThemeListener
	{
		private bool _listensToThemes;

		private readonly Point[] _themedArrow = new Point[3];

		void IThemeListener.OnThemeChanged()
		{
			Invalidate();
		}

		// A themed button repaints when the default theme changes. Other looks do not listen.
		private void UpdateThemeListening()
		{
			bool wanted = IsHandleCreated;
			if (wanted && !_listensToThemes)
			{
				MxThemeManager.AddListener(this);
				_listensToThemes = true;
			}
			else if (!wanted && _listensToThemes)
			{
				MxThemeManager.RemoveListener(this);
				_listensToThemes = false;
			}
		}

		protected override void OnHandleCreated(System.EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(System.EventArgs e)
		{
			if (_listensToThemes)
			{
				MxThemeManager.RemoveListener(this);
				_listensToThemes = false;
			}
			base.OnHandleDestroyed(e);
		}

		// The layout of the classic button places the image. DrawImage stays virtual, so a derived button draws its own image; a
		// disabled image is grayed against the disabled face.
		// Three square dots in the middle of the button, the same in every font.
		private void DrawThemedEllipsis(Graphics graphics, ResolvedTheme theme, Rectangle bounds, Color color, bool pressed)
		{
			int dot = System.Math.Max(2, theme.Scaled(2));
			int gap = dot;
			int total = dot * 3 + gap * 2;
			int x = bounds.X + (bounds.Width - total) / 2;
			int y = bounds.Y + (bounds.Height - dot) / 2;
			if (pressed && theme.Style.ButtonLook == ThemeButtonLook.Classic)
			{
				x++;
				y++;
			}
			Brush brush = GdiCache.Brush(color);
			for (int i = 0; i < 3; i++)
			{
				graphics.FillRectangle(brush, x + i * (dot + gap), y, dot, dot);
			}
		}

		private void DrawThemedImage(Graphics graphics, Color face)
		{
			FormatStyle style = new FormatStyle();
			style.BackColor = face;
			GraphicsState state = graphics.Save();
			graphics.IntersectClip(_contentBounds);
			DrawImage(graphics, style);
			graphics.Restore(state);
		}

		private void DrawThemedDropDown(Graphics graphics, ResolvedTheme theme, ThemeRole separatorRole, Color arrowColor)
		{
			Rectangle area = _dropDownBounds;
			if (ResolveButtonStyle() == ButtonStyle.DropDownButton)
			{
				int inset = theme.Scaled(5);
				int x = RightToLeft == RightToLeft.Yes ? area.Right - 1 : area.X;
				graphics.DrawLine(theme.Pen(separatorRole), x, area.Y + inset, x, area.Bottom - inset - 1);
			}
			int half = theme.Scaled(3);
			int cx = area.X + area.Width / 2;
			int cy = area.Y + area.Height / 2;
			_themedArrow[0] = new Point(cx - half, cy - half / 2);
			_themedArrow[1] = new Point(cx + half, cy - half / 2);
			_themedArrow[2] = new Point(cx, cy + half / 2 + 1);
			graphics.FillPolygon(GdiCache.Brush(arrowColor), _themedArrow);
		}

		// A disabled button takes the disabled text color of the theme unless a disabled state style sets a text color.
		private bool HasDisabledForeColor()
		{
			if (!StateStyles.DisabledFormatStyle.ForeColor.IsEmpty)
			{
				return true;
			}
			StateStyles commandStyles = GetAssociatedCommand()?.StateStyles;
			return commandStyles != null && !commandStyles.DisabledFormatStyle.ForeColor.IsEmpty;
		}

		// Paints the face, the border, the focus ring and the text from the default theme. The theme decides the colors and the
		// corner radius. A face (solid or gradient) or text color in the resolved state style wins over the theme, and the text
		// uses the font that the layout measured.
		private void PaintThemed(Graphics graphics, FormatStyle style, Font font)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool enabled = base.Enabled;
			bool pressed = enabled && (IsPressed() || IsDropDownPressed());
			bool hot = enabled && IsHot();
			bool accent = enabled && _isDefaultButton && _highlightActiveButton;
			ThemeRole fill;
			ThemeRole border;
			ThemeRole text;
			if (!enabled)
			{
				fill = ThemeRole.SurfaceDisabled;
				border = ThemeRole.BorderDisabled;
				text = ThemeRole.TextDisabled;
			}
			else if (accent)
			{
				fill = pressed ? ThemeRole.AccentPressed : hot ? ThemeRole.AccentHot : ThemeRole.Accent;
				border = fill;
				text = ThemeRole.TextOnAccent;
			}
			else
			{
				fill = pressed ? ThemeRole.SurfacePressed : hot ? ThemeRole.SurfaceHot : ThemeRole.SurfaceRaised;
				border = pressed ? ThemeRole.BorderPressed : hot ? ThemeRole.BorderHot : ThemeRole.Border;
				text = ThemeRole.Text;
			}
			Rectangle bounds = base.ClientRectangle;
			ButtonRenderer.DrawParentBackground(graphics, bounds, this);
			int radius = theme.Radius(ThemeElement.Button);
			Color customFace = style.GetBlendedBackColor();
			Color face = customFace.IsEmpty ? theme.Color(fill) : customFace;
			if (!customFace.IsEmpty)
			{
				// A gradient brush is laid out from the top-left corner of the bounds, where the rounded fill draws it.
				Brush faceBrush = style.HasGradient() ? style.GetBackgroundBrush(new Rectangle(Point.Empty, bounds.Size)) : GdiCache.Brush(customFace);
				if (faceBrush != null)
				{
					theme.FillRoundedWith(graphics, bounds, faceBrush, radius);
				}
				theme.DrawRounded(graphics, bounds, border, radius);
			}
			else if (!((!accent || theme.Style.Modern.Glass != ThemeGlassKind.None) && theme.DrawButtonLook(graphics, bounds, fill, border, radius, enabled, hot, pressed, enabled && _isDefaultButton)))
			{
				theme.FillRounded(graphics, bounds, fill, radius);
				theme.DrawRounded(graphics, bounds, border, radius);
			}
			bool styledText = enabled ? !style.ForeColor.IsEmpty : HasDisabledForeColor();
			Color textColor = styledText ? style.GetEffectiveForeColor() : theme.Color(text);
			if (!styledText && enabled && !_backColor.IsEmpty && customFace.A == 255)
			{
				// The theme text was chosen for the theme face; on a BackColor face it gives way to black or white when that reads better.
				Color other = MxTheme.Luminance(textColor) < 0.5 ? Color.White : Color.Black;
				textColor = ColorContrast.BetterText(textColor, other, customFace);
			}
			if (!_imageBounds.IsEmpty)
			{
				DrawThemedImage(graphics, face);
			}
			ButtonStyle buttonStyle = ResolveButtonStyle();
			if ((buttonStyle == ButtonStyle.DropDown || buttonStyle == ButtonStyle.DropDownButton) && !_dropDownBounds.IsEmpty)
			{
				DrawThemedDropDown(graphics, theme, buttonStyle == ButtonStyle.DropDownButton ? border : ThemeRole.Border, textColor);
			}
			if (buttonStyle == ButtonStyle.Ellipsis)
			{
				DrawThemedEllipsis(graphics, theme, bounds, textColor, pressed);
			}
			else if (ButtonText.Length > 0 && !_textBounds.IsEmpty && !textColor.IsEmpty)
			{
				// The layout measures the text with GDI+; the text is drawn with GDI, so it gets a little more width to avoid a false ellipsis.
				Rectangle textArea = Rectangle.Inflate(_textBounds, theme.Scaled(6), 0);
				textArea.Intersect(bounds);
				if (pressed && theme.Style.ButtonLook == ThemeButtonLook.Classic)
				{
					textArea.Offset(1, 1);
				}
				TextRenderer.DrawText(graphics, ButtonText, font, textArea, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			}
			if (Focused && ShowFocusRectangle)
			{
				Rectangle ring = bounds;
				ring.Inflate(-theme.Scaled(3), -theme.Scaled(3));
				theme.DrawRounded(graphics, ring, ThemeRole.BorderFocus, System.Math.Max(0, radius - 2));
			}
		}
	}
}
