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
		// The sizes of the close button on each tab of a browser theme, in pixels at 96 DPI, measured on Chrome: an 8 pixel
		// cross in a 16 pixel round hover face, its middle 16 pixels from the right edge of the tab.
		private const int TabCloseSize = 16;
		private const int TabCloseGap = 4;
		private const int TabCloseRightPad = 8;

		// True when the last layout gave each closable tab its own close button.
		internal bool _tabCloseLayout;
		private MxTabPage _tabCloseHot;
		private MxTabPage _tabClosePressed;

		/// <summary>
		/// True when each tab of the strip shows its own close button, as the tabs of Chrome and Firefox do: the close button is
		/// on, the theme is a browser theme, and the strip is one row of horizontal tabs in the control.
		/// </summary>
		internal bool TabCloseButtonsShown
		{
			get
			{
				if (!_showCloseButton || _multiLine || CaptionTabsActive)
				{
					return false;
				}
				if (_tabStripAlignment != TabStripAlignment.Top && _tabStripAlignment != TabStripAlignment.Bottom)
				{
					return false;
				}
				if (_textOrientation == TextOrientation.Vertical)
				{
					return false;
				}
				return FormFrameStyle.For(MxThemeManager.DefaultTheme).Kind == FormFrameKind.Browser;
			}
		}

		private int ScaleForTabClose(int value) => (int)Math.Round(value * DeviceDpi / 96f);

		/// <summary>The width that a closable tab grows by for its close button.</summary>
		internal int TabCloseExtent => ScaleForTabClose(TabCloseSize + TabCloseGap);

		internal MxTabPage TabCloseHotPage => _tabCloseHot;

		internal MxTabPage TabClosePressedPage => _tabClosePressed;

		// Called at the start of a layout: a change of the theme or of a setting can give the tabs their close buttons or take
		// them away, and that changes the width of each tab.
		private void SyncTabCloseLayout()
		{
			bool shown = TabCloseButtonsShown;
			if (shown == _tabCloseLayout)
			{
				return;
			}
			_tabCloseLayout = shown;
			_tabThickness = 0;
			foreach (MxTabPage page in _tabPages)
			{
				page._isSizeDirty = true;
			}
			if (!shown)
			{
				_tabCloseHot = null;
				_tabClosePressed = null;
			}
		}

		private void OnThemeChangedForTabClose()
		{
			if (TabCloseButtonsShown != _tabCloseLayout)
			{
				InvalidateLayout();
				if (base.IsHandleCreated && !base.IsDisposed)
				{
					LayoutTabsNow();
				}
			}
		}

		private bool HasTabClose(TabPageRenderer tab)
		{
			return _tabCloseLayout && tab != null && tab.GetTabPage() != null && tab.GetTabPage().AllowClose;
		}

		/// <summary>The close button of the tab in client coordinates, or empty when the tab has none.</summary>
		internal Rectangle GetTabCloseBounds(TabPageRenderer tab)
		{
			if (!HasTabClose(tab))
			{
				return Rectangle.Empty;
			}
			Rectangle bounds = tab._bounds;
			int size = ScaleForTabClose(TabCloseSize);
			int x = bounds.Right - ScaleForTabClose(TabCloseRightPad) - size;
			if (x < bounds.X)
			{
				return Rectangle.Empty;
			}
			return new Rectangle(x, bounds.Y + (bounds.Height - size) / 2, size, size);
		}

		/// <summary>The tab of the page in client coordinates, or empty when the strip does not show it.</summary>
		internal Rectangle GetTabBounds(MxTabPage page)
		{
			TabPageRenderer tab = page == null ? null : _pageRenderers.FindTabPageItem(page);
			return tab == null ? Rectangle.Empty : tab._bounds;
		}

		internal Rectangle GetTabCloseBounds(MxTabPage page)
		{
			return page == null ? Rectangle.Empty : GetTabCloseBounds(_pageRenderers.FindTabPageItem(page));
		}

		// The page whose close button lies under the point, or null.
		private MxTabPage HitTestTabClose(Point point)
		{
			if (!_tabCloseLayout)
			{
				return null;
			}
			TabPageRenderer tab = HitTestTab(point.X, point.Y);
			if (tab == null || !((FormatStyleLayoutState)tab).IsVisible())
			{
				return null;
			}
			return GetTabCloseBounds(tab).Contains(point) ? tab.GetTabPage() : null;
		}

		// The close button leaves room on the right of the text and the image.
		private void ReserveTabCloseSpace(TabPageRenderer tab)
		{
			if (!HasTabClose(tab))
			{
				return;
			}
			int extent = TabCloseExtent;
			if (!tab._textBounds.IsEmpty)
			{
				tab._textBounds.Width = Math.Max(0, tab._textBounds.Width - extent);
			}
			else if (!tab._imageBounds.IsEmpty)
			{
				tab._imageBounds.Width = Math.Max(0, tab._imageBounds.Width - extent);
			}
		}

		private void SetTabCloseHot(MxTabPage page)
		{
			if (!ReferenceEquals(_tabCloseHot, page))
			{
				InvalidateTabClose(_tabCloseHot);
				_tabCloseHot = page;
				InvalidateTabClose(page);
			}
		}

		private void InvalidateTabClose(MxTabPage page)
		{
			Rectangle bounds = GetTabCloseBounds(page);
			if (!bounds.IsEmpty)
			{
				bounds.Inflate(1, 1);
				Invalidate(bounds);
			}
		}

		// A press on a close button of a tab. True when the press is taken, so the tab is not selected.
		private bool TabCloseMouseDown(MouseEventArgs e)
		{
			_tabClosePressed = null;
			if (e.Button != MouseButtons.Left || !base.Enabled)
			{
				return false;
			}
			MxTabPage page = HitTestTabClose(e.Location);
			if (page == null)
			{
				return false;
			}
			if (page.Enabled)
			{
				_tabClosePressed = page;
				SetTabCloseHot(page);
				InvalidateTabClose(page);
			}
			return true;
		}

		private void TabCloseMouseMove(MouseEventArgs e)
		{
			MxTabPage page = base.Enabled ? HitTestTabClose(e.Location) : null;
			if (page != null && !page.Enabled)
			{
				page = null;
			}
			// While a close button is pressed, only that button lights up.
			if (_tabClosePressed != null && !ReferenceEquals(page, _tabClosePressed))
			{
				page = null;
			}
			SetTabCloseHot(page);
		}

		// The release closes the page only over the close button that the press started on.
		private void TabCloseMouseUp(MouseEventArgs e)
		{
			MxTabPage pressed = _tabClosePressed;
			if (pressed == null)
			{
				return;
			}
			_tabClosePressed = null;
			InvalidateTabClose(pressed);
			if (base.Enabled && pressed.Enabled && pressed.AllowClose && ReferenceEquals(HitTestTabClose(e.Location), pressed))
			{
				_tabCloseHot = null;
				pressed.TabVisible = false;
			}
		}

		private void TabCloseMouseLeave()
		{
			if (_tabClosePressed == null)
			{
				SetTabCloseHot(null);
			}
		}

		private void PaintTabCloseButton(Graphics graphics, ResolvedTheme theme, TabPageRenderer tab, bool enabled, bool selected)
		{
			Rectangle bounds = GetTabCloseBounds(tab);
			if (bounds.IsEmpty || !graphics.ClipBounds.IntersectsWith(bounds))
			{
				return;
			}
			MxTabPage page = tab.GetTabPage();
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (enabled && ReferenceEquals(page, _tabCloseHot))
			{
				ThemeRole face = ReferenceEquals(page, _tabClosePressed) ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot;
				graphics.FillEllipse(theme.Brush(face), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
			}
			float scale = DeviceDpi / 96f;
			Pen pen = GdiCache.Pen(theme.Color(!enabled ? ThemeRole.TextDisabled : selected ? ThemeRole.Text : ThemeRole.TextMuted), Math.Max(1f, 1.25f * scale));
			float arm = 4 * scale;
			float cx = bounds.X + (bounds.Width - 1) / 2f;
			float cy = bounds.Y + (bounds.Height - 1) / 2f;
			graphics.DrawLine(pen, cx - arm, cy - arm, cx + arm, cy + arm);
			graphics.DrawLine(pen, cx - arm, cy + arm, cx + arm, cy - arm);
			graphics.SmoothingMode = smoothing;
		}
	}
}
