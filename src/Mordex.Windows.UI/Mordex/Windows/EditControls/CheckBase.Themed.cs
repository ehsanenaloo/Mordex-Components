using System.ComponentModel;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Controls
{
	public partial class CheckBase : IThemeListener
	{
		private bool _listensToThemes;

		void IThemeListener.OnThemeChanged()
		{
			Invalidate();
		}

		// A themed control repaints when the default theme changes. Other looks do not listen.
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

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (_listensToThemes)
			{
				MxThemeManager.RemoveListener(this);
				_listensToThemes = false;
			}
			base.OnHandleDestroyed(e);
		}

		// Paints a check box or a radio button, its image and its text from the default theme. The layout has been made by the
		// caller with the same style. Colors and a font set in FormatStyle win over the theme. A disabled control draws with
		// DisabledFormatStyle over FormatStyle; its text takes the disabled text color of the theme unless DisabledFormatStyle sets one.
		private protected void PaintThemed(Graphics graphics, FormatStyle style, bool radio, bool isChecked, bool isIndeterminate)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool enabled = base.Enabled;
			bool hot = enabled && GetIsHot();
			bool pressed = enabled && hot && GetIsPressed();
			bool on = isChecked || isIndeterminate;
			ThemeCheckGlyph markShape = theme.Style.Check;
			ThemeRole fill;
			ThemeRole border;
			if (!enabled)
			{
				fill = on && markShape == ThemeCheckGlyph.Default ? ThemeRole.BorderDisabled : ThemeRole.InputBackDisabled;
				border = ThemeRole.BorderDisabled;
			}
			else if (on && markShape == ThemeCheckGlyph.Default)
			{
				fill = pressed ? ThemeRole.AccentPressed : hot ? ThemeRole.AccentHot : ThemeRole.Accent;
				border = fill;
			}
			else
			{
				fill = pressed ? ThemeRole.SurfacePressed : ThemeRole.InputBack;
				border = pressed ? ThemeRole.BorderPressed : hot ? ThemeRole.BorderHot : ThemeRole.BorderStrong;
			}
			ButtonRenderer.DrawParentBackground(graphics, base.ClientRectangle, this);
			bool customBack = !_formatStyle.BackColor.IsEmpty || _formatStyle.BackgroundImage != null || (!enabled && (!_disabledFormatStyle.BackColor.IsEmpty || _disabledFormatStyle.BackgroundImage != null));
			if (customBack)
			{
				RenderingHelper.DrawFormatStyleBackground(graphics, style, base.ClientRectangle, base.ClientRectangle);
			}
			Rectangle glyph = _glyphBounds;
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			ThemeRole markRole = markShape != ThemeCheckGlyph.Default ? (enabled ? ThemeRole.Text : ThemeRole.TextDisabled) : enabled ? ThemeRole.TextOnAccent : ThemeRole.SurfaceDisabled;
			if (theme.DrawChoiceGlyph(graphics, glyph, radio, isChecked, isIndeterminate, enabled, hot, pressed))
			{
			}
			else if (theme.DrawGlassBox(graphics, glyph, radio, enabled, hot, pressed))
			{
				Color mark = theme.BoxMarkColor(enabled);
				if (radio && isChecked)
				{
					graphics.FillEllipse(GdiCache.Brush(mark), Rectangle.Inflate(glyph, -glyph.Width / 3, -glyph.Height / 3));
				}
				else if (!radio && isChecked)
				{
					Pen markPen = GdiCache.Pen(mark, Math.Max(2f, theme.Scale * 2f));
					float u = glyph.Width / 16f;
					graphics.DrawLine(markPen, glyph.X + 3.5f * u, glyph.Y + 8f * u, glyph.X + 6.5f * u, glyph.Y + 11f * u);
					graphics.DrawLine(markPen, glyph.X + 6.5f * u, glyph.Y + 11f * u, glyph.X + 12.5f * u, glyph.Y + 4.5f * u);
				}
				else if (!radio && isIndeterminate)
				{
					Rectangle bar = Rectangle.Inflate(glyph, -glyph.Width / 4, 0);
					bar.Y = glyph.Y + glyph.Height / 2 - 1;
					bar.Height = 2;
					graphics.FillRectangle(GdiCache.Brush(mark), bar);
				}
			}
			else if (radio)
			{
				graphics.FillEllipse(theme.Brush(fill), glyph);
				if (theme.Style.InputBorder == ThemeBorder.Sunken)
				{
					theme.DrawSunkenEllipse(graphics, glyph);
				}
				else
				{
					graphics.DrawEllipse(theme.Pen(border), glyph.X, glyph.Y, glyph.Width - 1, glyph.Height - 1);
				}
				if (isChecked)
				{
					Rectangle dot = Rectangle.Inflate(glyph, -glyph.Width / 3, -glyph.Height / 3);
					graphics.FillEllipse(theme.Brush(markRole), dot);
				}
			}
			else
			{
				int radius = Math.Min(theme.Radius(ThemeElement.Input), glyph.Width / 3);
				theme.FillRounded(graphics, glyph, fill, radius);
				if (markShape != ThemeCheckGlyph.Default)
				{
					theme.DrawBorder(graphics, glyph, theme.Style.InputBorder, border, radius);
				}
				else
				{
					theme.DrawRounded(graphics, glyph, border, radius);
				}
				if (isChecked && markShape != ThemeCheckGlyph.Default)
				{
					theme.DrawCheck(graphics, Rectangle.Inflate(glyph, -1, -1), markRole, markShape);
				}
				else if (isChecked)
				{
					Pen mark = theme.Pen(markRole, 2);
					float u = glyph.Width / 16f;
					graphics.DrawLine(mark, glyph.X + 3.5f * u, glyph.Y + 8f * u, glyph.X + 6.5f * u, glyph.Y + 11f * u);
					graphics.DrawLine(mark, glyph.X + 6.5f * u, glyph.Y + 11f * u, glyph.X + 12.5f * u, glyph.Y + 4.5f * u);
				}
				else if (isIndeterminate)
				{
					Rectangle bar = Rectangle.Inflate(glyph, -glyph.Width / 4, 0);
					bar.Y = glyph.Y + glyph.Height / 2 - 1;
					bar.Height = 2;
					graphics.FillRectangle(theme.Brush(markRole), bar);
				}
			}
			graphics.SmoothingMode = smoothing;
			DrawContentImage(graphics, style.BackColor);
			FormatStyle textStyle = enabled ? _formatStyle : _disabledFormatStyle;
			Color textColor = textStyle.ForeColor.IsEmpty ? theme.Color(enabled ? ThemeRole.Text : ThemeRole.TextDisabled) : style.GetEffectiveForeColor();
			if (Text.Length > 0 && !_textBounds.IsEmpty && !textColor.IsEmpty)
			{
				// The layout measures the text with GDI+; the text is drawn with GDI, so it gets the rest of the row to avoid a false ellipsis.
				Rectangle textArea = _textBounds;
				textArea.Width = Math.Max(textArea.Width, base.ClientRectangle.Right - textArea.X);
				TextRenderer.DrawText(graphics, Text, style.ResolveFont(Font), textArea, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			}
			if (Focused && ShowFocusRectangle)
			{
				Rectangle ring = Rectangle.Inflate(_textBounds, 2, 1);
				ring.Intersect(base.ClientRectangle);
				theme.DrawRounded(graphics, ring, ThemeRole.BorderFocus, Math.Min(theme.Shape.CornerRadius, 3));
			}
		}
	}
}
