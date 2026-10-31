using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Tabs
{
	public partial class MxTabControl
	{
		private ThemeHook _themeHook;

		private bool IsThemed => true;

		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				if (!IsThemed)
				{
					return;
				}
				_themeHook = new ThemeHook(this, OnThemeChangedForCaption);
			}
			_themeHook.Update(IsThemed);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
			if (!base.DesignMode)
			{
				FormWatch.Start();
			}
			UpdateCaptionTabs(true);
		}

		// Paints the strip, the tabs, the strip buttons and the page border from the default theme. The layout has been made by the caller.
		private void PaintThemedControl(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			Rectangle client = base.ClientRectangle;
			graphics.FillRectangle(theme.Brush(ThemeRole.SurfaceAlt), client);
			Rectangle page = HasVisibleTabs() ? _pageDisplayBounds : client;
			if (!page.IsEmpty)
			{
				if (!theme.Style.Detail.TabPageBack.IsEmpty)
				{
					// The Windows XP page: white at the top, a warm gray at the bottom.
					GdiCache.FillGradient(graphics, page, theme.Style.Detail.TabPageBack, System.Drawing.Color.FromArgb(244, 243, 238), LinearGradientMode.Vertical);
				}
				else
				{
					graphics.FillRectangle(theme.Brush(ThemeRole.TabActive), page);
				}
				if (_pageBorder == PageBorder.All)
				{
					graphics.DrawRectangle(theme.Pen(ThemeRole.Border), page.X, page.Y, page.Width - 1, page.Height - 1);
				}
				else if (_pageBorder == PageBorder.StripLine && HasVisibleTabs())
				{
					PaintThemedStripLine(graphics, theme, page);
				}
			}
			if (!HasVisibleTabs() || _tabStripBounds.IsEmpty)
			{
				return;
			}
			Rectangle strip = _tabStripBounds;
			strip.Inflate(2, 2);
			if (!graphics.ClipBounds.IntersectsWith(strip))
			{
				return;
			}
			GraphicsState state = graphics.Save();
			graphics.SetClip(strip, CombineMode.Intersect);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			TextLayoutRenderer textRenderer = CreateTabTextRenderer();
			TabPageRenderer selected = null;
			for (int i = 0; i < _pageRenderers.Count; i++)
			{
				TabPageRenderer tab = (TabPageRenderer)_pageRenderers.FindItemByIndex(i);
				if (!((FormatStyleLayoutState)tab).IsVisible() || Rectangle.Intersect(_tabStripBounds, tab._bounds).IsEmpty)
				{
					continue;
				}
				if (tab.IsPressed())
				{
					selected = tab;
				}
				PaintThemedTab(graphics, theme, tab, textRenderer);
			}
			if (selected != null)
			{
				PaintThemedTab(graphics, theme, selected, textRenderer);
			}
			textRenderer.ResetCache();
			graphics.SmoothingMode = SmoothingMode.Default;
			PaintThemedStripButtons(graphics, theme);
			graphics.Restore(state);
		}

		private void PaintThemedStripLine(Graphics graphics, ResolvedTheme theme, Rectangle page)
		{
			Pen pen = theme.Pen(ThemeRole.Border);
			switch (_tabStripAlignment)
			{
			case TabStripAlignment.Top:
				graphics.DrawLine(pen, page.Left, page.Top, page.Right - 1, page.Top);
				break;
			case TabStripAlignment.Bottom:
				graphics.DrawLine(pen, page.Left, page.Bottom - 1, page.Right - 1, page.Bottom - 1);
				break;
			case TabStripAlignment.Left:
				graphics.DrawLine(pen, page.Left, page.Top, page.Left, page.Bottom - 1);
				break;
			default:
				graphics.DrawLine(pen, page.Right - 1, page.Top, page.Right - 1, page.Bottom - 1);
				break;
			}
		}

		private void PaintThemedTab(Graphics graphics, ResolvedTheme theme, TabPageRenderer tab, TextLayoutRenderer textRenderer)
		{
			bool enabled = base.Enabled && ((FormatStyleLayoutState)tab).IsEnabled();
			bool selected = tab.IsPressed();
			bool hot = enabled && tab.IsHot();
			ThemeRole fill = selected ? ThemeRole.TabActive : hot ? ThemeRole.TabHot : ThemeRole.TabInactive;
			Rectangle bounds = tab._bounds;
			int radius = Math.Min(theme.Radius(ThemeElement.Tab, 2), Math.Min(bounds.Width, bounds.Height) / 2);
			// The shape is a rounded rectangle that reaches into the page, so only the outer corners show.
			Rectangle shape = bounds;
			switch (_tabStripAlignment)
			{
			case TabStripAlignment.Top:
				shape.Height += radius;
				break;
			case TabStripAlignment.Bottom:
				shape.Y -= radius;
				shape.Height += radius;
				break;
			case TabStripAlignment.Left:
				shape.Width += radius;
				break;
			default:
				shape.X -= radius;
				shape.Width += radius;
				break;
			}
			GraphicsState state = graphics.Save();
			graphics.SetClip(bounds, CombineMode.Intersect);
			bool trapezoid = _tabStripAlignment == TabStripAlignment.Top && (selected || hot) && theme.DrawTrapezoidTab(graphics, new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height), fill, ThemeRole.Border, false);
			if (!trapezoid)
			{
				if (selected && theme.Style.GlossBand && _tabStripAlignment == TabStripAlignment.Top && shape.Width > 0 && shape.Height > 0)
				{
					// Office 2007: the selected tab fades from a tinted top into the page color and has a light inner edge.
					Color face = theme.Color(fill);
					using (LinearGradientBrush brush = new LinearGradientBrush(shape, MxTheme.Darken(face, 0.07f), face, LinearGradientMode.Vertical))
					{
						theme.FillRoundedWith(graphics, shape, brush, radius);
					}
					graphics.DrawLine(GdiCache.Pen(Color.FromArgb(160, Color.White)), bounds.Left + radius, bounds.Top + 1, bounds.Right - radius - 1, bounds.Top + 1);
				}
				else
				{
					theme.FillRounded(graphics, shape, fill, radius);
				}
			}
			if (trapezoid)
			{
			}
			else if (theme.Style.TabRaised && _tabStripAlignment == TabStripAlignment.Top)
			{
				theme.DrawRaisedTab(graphics, bounds);
			}
			else if (selected || hot)
			{
				theme.DrawRounded(graphics, shape, ThemeRole.Border, radius);
			}
			if (selected || hot)
			{
				theme.DrawTabAccent(graphics, bounds, _tabStripAlignment == TabStripAlignment.Top ? 0 : _tabStripAlignment == TabStripAlignment.Bottom ? 1 : _tabStripAlignment == TabStripAlignment.Left ? 2 : 3);
			}
			graphics.Restore(state);
			if (selected)
			{
				Pen join = theme.Pen(ThemeRole.TabActive);
				switch (_tabStripAlignment)
				{
				case TabStripAlignment.Top:
					graphics.DrawLine(join, bounds.Left + 1, bounds.Bottom - 1, bounds.Right - 2, bounds.Bottom - 1);
					break;
				case TabStripAlignment.Bottom:
					graphics.DrawLine(join, bounds.Left + 1, bounds.Top, bounds.Right - 2, bounds.Top);
					break;
				case TabStripAlignment.Left:
					graphics.DrawLine(join, bounds.Right - 1, bounds.Top + 1, bounds.Right - 1, bounds.Bottom - 2);
					break;
				default:
					graphics.DrawLine(join, bounds.Left, bounds.Top + 1, bounds.Left, bounds.Bottom - 2);
					break;
				}
			}
			ThemeRole text = !enabled ? ThemeRole.TextDisabled : selected ? ThemeRole.Text : ThemeRole.TextMuted;
			graphics.SmoothingMode = SmoothingMode.Default;
			PaintTabContent(graphics, tab, Font, textRenderer, theme.Brush(text), false, selected, theme.Color(fill));
			PaintTabCloseButton(graphics, theme, tab, enabled, selected);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
		}

		private void PaintThemedStripButtons(Graphics graphics, ResolvedTheme theme)
		{
			if (_closeButton != null)
			{
				bool enabled = base.Enabled && _closeButton.IsEnabled();
				Rectangle bounds = _closeButton._bounds;
				PaintThemedButtonFace(graphics, theme, bounds, enabled, _closeButton.IsHot(), _closeButton.IsPressed());
				Pen pen = theme.Pen(enabled ? ThemeRole.Text : ThemeRole.TextDisabled);
				int s = Math.Max(2, Math.Min(bounds.Width, bounds.Height) / 5);
				int cx = bounds.X + bounds.Width / 2;
				int cy = bounds.Y + bounds.Height / 2;
				graphics.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
				graphics.DrawLine(pen, cx - s, cy + s, cx + s, cy - s);
			}
			if (_scrollBackButton == null)
			{
				return;
			}
			PaintThemedScrollButton(graphics, theme, _scrollBackButton, -1);
			PaintThemedScrollButton(graphics, theme, _scrollForwardButton, 1);
		}

		private void PaintThemedButtonFace(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool enabled, bool hot, bool pressed)
		{
			if (enabled && (hot || pressed))
			{
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				theme.FillRounded(graphics, bounds, pressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, Math.Min(theme.Shape.CornerRadius, 4));
				graphics.SmoothingMode = smoothing;
			}
		}

		private void PaintThemedScrollButton(Graphics graphics, ResolvedTheme theme, FormatStyleLayoutState button, int direction)
		{
			bool enabled = base.Enabled && button.IsEnabled();
			Rectangle bounds = button._bounds;
			PaintThemedButtonFace(graphics, theme, bounds, enabled, button.IsHot(), button.IsPressed());
			Pen pen = theme.Pen(enabled ? ThemeRole.Text : ThemeRole.TextDisabled);
			float cx = bounds.X + bounds.Width / 2f;
			float cy = bounds.Y + bounds.Height / 2f;
			float s = Math.Max(2f, theme.Scaled(3));
			if (_isHorizontalStrip)
			{
				float tip = cx + direction * s / 2f;
				float back = cx - direction * s / 2f;
				graphics.DrawLine(pen, back, cy - s, tip, cy);
				graphics.DrawLine(pen, tip, cy, back, cy + s);
			}
			else
			{
				float tip = cy + direction * s / 2f;
				float back = cy - direction * s / 2f;
				graphics.DrawLine(pen, cx - s, back, cx, tip);
				graphics.DrawLine(pen, cx, tip, cx + s, back);
			}
		}
	}
}
