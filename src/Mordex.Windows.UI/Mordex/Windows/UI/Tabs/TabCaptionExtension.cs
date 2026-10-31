using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Tabs
{
	/// <summary>Draws the tabs of a tab control into the caption of a themed browser window frame, and handles the mouse on them.</summary>
	internal sealed class TabCaptionExtension : IFrameCaptionExtension
	{
		internal enum Part
		{
			None,
			Tab,
			Close,
			Add,
			Back,
			Forward
		}

		internal struct Item
		{
			internal int Index;
			internal MxTabPage Page;
			internal Rectangle Bounds;
			internal Rectangle Close;
		}

		// The places of the parts of the strip, in window coordinates.
		internal sealed class StripLayout
		{
			internal readonly List<Item> Items = new List<Item>();
			// The free part of the caption between the edge and the window buttons that the strip may use.
			internal Rectangle Interval;
			internal Rectangle Area;
			internal Rectangle Add;
			internal Rectangle Back;
			internal Rectangle Forward;
			internal int Step;
			internal int Scroll;
			internal int MaxScroll;
			internal bool Mirrored;
		}

		private static readonly StringFormat LeftToRight = new StringFormat(StringFormatFlags.NoWrap) { Trimming = StringTrimming.EllipsisCharacter, LineAlignment = StringAlignment.Center, HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None };

		private static readonly StringFormat RightToLeft = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.DirectionRightToLeft) { Trimming = StringTrimming.EllipsisCharacter, LineAlignment = StringAlignment.Center, HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None };

		private readonly MxTabControl _tabs;
		private readonly MxThemedFormFrame _frame;
		private readonly Form _form;
		private MxTabPage _hotPage;
		private Part _hotPart;
		private MxTabPage _pressedPage;
		private Part _pressedPart;
		private int _scroll;
		private bool _scrollToSelected = true;
		private int _dragStartX;
		private int _dragOrigin = -1;
		private bool _dragging;
		private ThemedToolTip _tip;
		private Timer _tipTimer;
		private StripLayout _layout;
		private (Rectangle Caption, Rectangle Minimize, Rectangle Maximize, Rectangle Close, bool Mirrored, bool Maximized, int Dpi, bool Firefox, int Version) _layoutKey;
		private int _version;
		private List<MxTabPage> _accessiblePages;
		private List<AccessibleObject> _accessible;
		private StripLayout _accessibleLayout;

		internal TabCaptionExtension(MxTabControl tabs, MxThemedFormFrame frame)
		{
			_tabs = tabs;
			_frame = frame;
			_form = frame.Form;
			if (_form != null)
			{
				_form.VisibleChanged += OnFormVisibleChanged;
			}
		}

		internal MxThemedFormFrame Frame => _frame;

		/// <summary>True while the control may show its tabs in this caption: the frame is a browser frame of its form and the control is shown.</summary>
		internal bool IsEligible => IsEligibleFor(_tabs, _frame);

		/// <summary>True while the caption shows the tabs: the control is eligible and this extension holds the slot of the frame.</summary>
		internal bool IsActive => ReferenceEquals(_frame.CaptionExtension, this) && IsEligible;

		internal static bool IsEligibleFor(MxTabControl tabs, MxThemedFormFrame frame)
		{
			if (tabs.IsDisposed || !frame.IsPainting || frame.CurrentStyle.Kind != FormFrameKind.Browser)
			{
				return false;
			}
			Form form = frame.Form;
			// Control.Visible includes the parents, but it is false for every child while the form itself is hidden.
			return form != null && ReferenceEquals(tabs.FindForm(), form) && (tabs.Visible || !form.Visible);
		}

		public bool HidesTitle => IsActive;

		private float Scale => _tabs.DeviceDpi / 96f;

		private int S(int value) => (int)Math.Round(value * Scale);

		public int LeftInset => IsActive ? S(8) : 0;

		internal void Refresh()
		{
			_version++;
			_scrollToSelected = true;
			// A page that left the control or its strip ends its accessible objects now, not when a client next asks for the children.
			if (ExpireAccessible(part => part.Page == null || IsShownPage(part.Page)))
			{
				_tabs.NotifyCaptionAccessibility(AccessibleEvents.Reorder, -1);
			}
			if (IsActive)
			{
				_frame.InvalidateCaption();
			}
		}

		internal void Release()
		{
			ExpireAccessible(part => false);
			if (_form != null)
			{
				_form.VisibleChanged -= OnFormVisibleChanged;
			}
			ClearPress();
			_hotPage = null;
			_hotPart = Part.None;
			HideTip();
			_tipTimer?.Dispose();
			_tipTimer = null;
			_tip?.Dispose();
			_tip = null;
		}

		private void OnFormVisibleChanged(object sender, EventArgs e)
		{
			_tabs.OnCaptionEligibilityChanged();
		}

		private bool Firefox => MxThemeManager.ResolveDefault(_tabs.DeviceDpi).Theme.Family == "Firefox";

		/// <summary>
		/// Places the parts of the strip in the free interval of the caption: between the left edge and the window buttons, or,
		/// in a mirrored window, between the window buttons and the right edge. A mirrored strip runs from right to left.
		/// </summary>
		internal static StripLayout Compute(Rectangle caption, FormFrameMetrics metrics, float scale, bool firefox, IReadOnlyList<bool> closable, int selected, bool showAdd, int scroll, bool scrollToSelected)
		{
			int S(int value) => (int)Math.Round(value * scale);
			StripLayout layout = new StripLayout();
			bool mirrored = metrics != null && metrics.Mirrored;
			bool maximized = metrics != null && metrics.Maximized;
			int lo = caption.X + S(mirrored ? 40 : 8);
			int hi = caption.Right - S(mirrored ? 8 : 40);
			if (metrics != null)
			{
				foreach (Rectangle button in new[] { metrics.MinimizeButton, metrics.MaximizeButton, metrics.CloseButton })
				{
					if (button.IsEmpty)
					{
						continue;
					}
					if (mirrored)
					{
						lo = Math.Max(lo, button.Right + S(40));
					}
					else
					{
						hi = Math.Min(hi, button.X - S(40));
					}
				}
			}
			hi = Math.Max(lo, hi);
			int top = caption.Y + S(firefox ? (maximized ? 4 : 6) : (maximized ? 4 : 8));
			int bottom = caption.Bottom - S(firefox ? (maximized ? 4 : 6) : 0);
			int height = Math.Max(0, bottom - top);
			int count = closable.Count;
			int gap = S(2);
			int addSize = showAdd ? S(28) + S(4) : 0;
			int available = Math.Max(0, hi - lo - addSize);
			int width = count == 0 ? 0 : Math.Max(S(48), Math.Min(S(225), (available - gap * (count - 1)) / count));
			int total = count == 0 ? 0 : count * width + gap * (count - 1);
			layout.Step = width + gap;
			layout.Mirrored = mirrored;
			int areaLeft = lo;
			int areaRight = lo + Math.Min(total, available);
			bool overflow = total > available;
			if (overflow)
			{
				int arrow = S(22);
				layout.Back = new Rectangle(lo, top + (height - S(24)) / 2, arrow, S(24));
				layout.Forward = new Rectangle(lo + available - arrow, layout.Back.Y, arrow, S(24));
				areaLeft = layout.Back.Right + S(2);
				areaRight = layout.Forward.X - S(2);
				int visible = Math.Max(0, areaRight - areaLeft);
				layout.MaxScroll = Math.Max(0, total - visible);
				if (scrollToSelected && selected >= 0)
				{
					int start = selected * layout.Step;
					if (start < scroll)
					{
						scroll = start;
					}
					else if (start + width > scroll + visible)
					{
						scroll = start + width - visible;
					}
				}
				scroll = Math.Max(0, Math.Min(layout.MaxScroll, scroll));
			}
			else
			{
				scroll = 0;
			}
			layout.Scroll = scroll;
			layout.Area = new Rectangle(areaLeft, top, Math.Max(0, areaRight - areaLeft), height);
			int x = areaLeft - scroll;
			int closeSize = S(20);
			for (int i = 0; i < count; i++)
			{
				Rectangle bounds = new Rectangle(x, top, width, height);
				Rectangle close = closable[i] && width >= S(70) ? new Rectangle(bounds.Right - closeSize - S(8), bounds.Y + (height - closeSize) / 2, closeSize, closeSize) : Rectangle.Empty;
				layout.Items.Add(new Item { Index = i, Bounds = bounds, Close = close });
				x += layout.Step;
			}
			if (showAdd)
			{
				int size = S(28);
				int addX = (overflow ? layout.Forward.Right : areaRight) + S(2);
				layout.Add = new Rectangle(addX, top + (height - size) / 2, size, size);
			}
			layout.Interval = new Rectangle(lo, top, hi - lo, height);
			if (mirrored)
			{
				Rectangle Mirror(Rectangle r) => r.IsEmpty ? r : new Rectangle(lo + hi - r.Right, r.Y, r.Width, r.Height);
				layout.Area = Mirror(layout.Area);
				layout.Back = Mirror(layout.Back);
				layout.Forward = Mirror(layout.Forward);
				layout.Add = Mirror(layout.Add);
				for (int i = 0; i < layout.Items.Count; i++)
				{
					Item item = layout.Items[i];
					item.Bounds = Mirror(item.Bounds);
					item.Close = Mirror(item.Close);
					layout.Items[i] = item;
				}
			}
			return layout;
		}

		// The layout is kept until the pages, the selection, the scroll, the frame or the DPI change, so moving the pointer
		// and hit testing do not build it again.
		internal StripLayout CurrentLayout()
		{
			FormFrameMetrics metrics = _frame.GetMetricsWithoutExtension();
			Rectangle caption = metrics?.Caption ?? Rectangle.Empty;
			bool firefox = Firefox;
			var key = (caption, metrics?.MinimizeButton ?? Rectangle.Empty, metrics?.MaximizeButton ?? Rectangle.Empty, metrics?.CloseButton ?? Rectangle.Empty,
				metrics != null && metrics.Mirrored, metrics != null && metrics.Maximized, _tabs.DeviceDpi, firefox, _version);
			if (_layout != null && _layoutKey.Equals(key))
			{
				return _layout;
			}
			List<MxTabPage> pages = new List<MxTabPage>();
			List<bool> closable = new List<bool>();
			foreach (MxTabPage page in _tabs.TabPages)
			{
				if (page.TabVisible)
				{
					pages.Add(page);
					closable.Add(CanClose(page));
				}
			}
			StripLayout layout = Compute(caption, metrics, Scale, firefox, closable, pages.IndexOf(_tabs.SelectedTab), _tabs.ShowAddTabButton, _scroll, _scrollToSelected);
			for (int i = 0; i < layout.Items.Count; i++)
			{
				Item item = layout.Items[i];
				item.Page = pages[i];
				layout.Items[i] = item;
			}
			_scroll = layout.Scroll;
			_scrollToSelected = false;
			_layout = layout;
			_layoutKey = key;
			ExpireAccessible(part => HasPart(layout, part.Part, part.Page));
			if (_accessiblePages != null && !SamePages(_accessiblePages, pages))
			{
				_tabs.NotifyCaptionAccessibility(AccessibleEvents.Reorder, -1);
			}
			_accessiblePages = pages;
			return layout;
		}

		private static bool SamePages(List<MxTabPage> a, List<MxTabPage> b)
		{
			if (a.Count != b.Count)
			{
				return false;
			}
			for (int i = 0; i < a.Count; i++)
			{
				if (!ReferenceEquals(a[i], b[i]))
				{
					return false;
				}
			}
			return true;
		}

		// True while the page belongs to the control and shows a tab.
		private bool IsShownPage(MxTabPage page)
		{
			int index = page.Index;
			return index >= 0 && index < _tabs.TabPages.Count && ReferenceEquals(_tabs.TabPages[index], page) && page.TabVisible;
		}

		private static bool HasPart(StripLayout layout, Part part, MxTabPage page)
		{
			switch (part)
			{
			case Part.Add:
				return !layout.Add.IsEmpty;
			case Part.Back:
			case Part.Forward:
				return !layout.Back.IsEmpty;
			case Part.Tab:
			case Part.Close:
				foreach (Item item in layout.Items)
				{
					if (ReferenceEquals(item.Page, page))
					{
						return part == Part.Tab || !item.Close.IsEmpty;
					}
				}
				return false;
			}
			return false;
		}

		// Ends the accessible objects that fail the test. They are forgotten, so a part that comes back gets a new object and a
		// client that kept the old one keeps an inert object.
		private bool ExpireAccessible(Func<PartAccessibleObject, bool> keep)
		{
			if (_accessible == null)
			{
				return false;
			}
			List<AccessibleObject> kept = new List<AccessibleObject>(_accessible.Count);
			foreach (AccessibleObject o in _accessible)
			{
				PartAccessibleObject part = (PartAccessibleObject)o;
				if (part.Alive && keep(part))
				{
					kept.Add(part);
				}
				else
				{
					part.Alive = false;
				}
			}
			if (kept.Count == _accessible.Count)
			{
				return false;
			}
			// A new list, because a caller may still hold the old one.
			_accessible = kept;
			_accessibleLayout = null;
			return true;
		}

		// True while the object stands for a part that the caption shows now.
		private bool IsCurrent(PartAccessibleObject part)
		{
			if (!part.Alive || !IsActive)
			{
				return false;
			}
			StripLayout layout = CurrentLayout();
			return part.Alive && HasPart(layout, part.Part, part.Page) && (part.Page == null || IsShownPage(part.Page));
		}

		// A tab has a close button only when the control shows close buttons and the page may be closed.
		private bool CanClose(MxTabPage page) => _tabs.ShowCloseButton && page.AllowClose;

		public void Paint(Graphics graphics, Rectangle caption, bool active)
		{
			if (!IsActive)
			{
				return;
			}
			ResolvedTheme theme = MxThemeManager.ResolveDefault(_tabs.DeviceDpi);
			bool firefox = theme.Theme.Family == "Firefox";
			bool dark = theme.Theme.Kind == ThemeKind.Dark;
			bool enabled = _tabs.Enabled;
			StripLayout layout = CurrentLayout();
			SmoothingMode smoothing = graphics.SmoothingMode;
			GraphicsState state = graphics.Save();
			graphics.SetClip(Rectangle.Intersect(layout.Area, caption), CombineMode.Intersect);
			foreach (Item item in layout.Items)
			{
				if (!ReferenceEquals(item.Page, _tabs.SelectedTab))
				{
					PaintTab(graphics, theme, layout, item, false, firefox, dark, enabled);
				}
			}
			foreach (Item item in layout.Items)
			{
				if (ReferenceEquals(item.Page, _tabs.SelectedTab))
				{
					PaintTab(graphics, theme, layout, item, true, firefox, dark, enabled);
				}
			}
			graphics.Restore(state);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (!layout.Back.IsEmpty)
			{
				int back = layout.Mirrored ? 1 : -1;
				PaintArrow(graphics, theme, layout.Back, Part.Back, back, enabled && layout.Scroll > 0, firefox);
				PaintArrow(graphics, theme, layout.Forward, Part.Forward, -back, enabled && layout.Scroll < layout.MaxScroll, firefox);
			}
			if (!layout.Add.IsEmpty)
			{
				if (enabled && _hotPart == Part.Add)
				{
					theme.FillRounded(graphics, layout.Add, _pressedPart == Part.Add ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, S(firefox ? 6 : 14));
				}
				Pen pen = GdiCache.Pen(theme.Color(enabled ? ThemeRole.Text : ThemeRole.TextDisabled), Math.Max(1f, Scale * 1.5f));
				int arm = S(6);
				int cx = layout.Add.X + layout.Add.Width / 2;
				int cy = layout.Add.Y + layout.Add.Height / 2;
				graphics.DrawLine(pen, cx - arm, cy, cx + arm, cy);
				graphics.DrawLine(pen, cx, cy - arm, cx, cy + arm);
			}
			graphics.SmoothingMode = smoothing;
		}

		private void PaintArrow(Graphics graphics, ResolvedTheme theme, Rectangle bounds, Part part, int direction, bool enabled, bool firefox)
		{
			if (enabled && _hotPart == part)
			{
				theme.FillRounded(graphics, bounds, _pressedPart == part ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, S(firefox ? 4 : 10));
			}
			Pen pen = GdiCache.Pen(theme.Color(enabled ? ThemeRole.Text : ThemeRole.TextDisabled), Math.Max(1f, Scale * 1.5f));
			int s = S(4);
			float cx = bounds.X + bounds.Width / 2f;
			float cy = bounds.Y + bounds.Height / 2f;
			float tip = cx + direction * s / 2f;
			float back = cx - direction * s / 2f;
			graphics.DrawLine(pen, back, cy - s, tip, cy);
			graphics.DrawLine(pen, tip, cy, back, cy + s);
		}

		private void PaintTab(Graphics graphics, ResolvedTheme theme, StripLayout layout, Item item, bool selected, bool firefox, bool dark, bool enabled)
		{
			bool hot = enabled && ReferenceEquals(item.Page, _hotPage);
			Rectangle bounds = item.Bounds;
			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return;
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (firefox)
			{
				int radius = S(6);
				if (selected || hot)
				{
					theme.FillRounded(graphics, bounds, selected ? ThemeRole.TabActive : ThemeRole.SurfaceHot, radius);
				}
				if (selected)
				{
					Color ring = dark ? Color.FromArgb(0xC0, 0xA0, 0xFF) : Color.FromArgb(0x75, 0x42, 0xE5);
					Rectangle edge = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
					using (GraphicsPath path = Rounded(edge, radius))
					{
						graphics.DrawPath(GdiCache.Pen(enabled ? ring : theme.Color(ThemeRole.TextDisabled), Math.Max(1f, Scale)), path);
					}
				}
			}
			else if (selected || hot)
			{
				int radius = S(8);
				Rectangle shape = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height + radius);
				GraphicsState state = graphics.Save();
				graphics.SetClip(bounds, CombineMode.Intersect);
				theme.FillRounded(graphics, shape, selected ? ThemeRole.TabActive : ThemeRole.SurfaceHot, radius);
				graphics.Restore(state);
			}
			graphics.SmoothingMode = SmoothingMode.None;
			bool mirrored = layout.Mirrored;
			int pad = S(10);
			int left = bounds.X + pad;
			int right = bounds.Right - pad;
			if (!item.Close.IsEmpty)
			{
				if (mirrored)
				{
					left = item.Close.Right + S(2);
				}
				else
				{
					right = item.Close.X - S(2);
				}
			}
			Image image = _tabs.GetTabImage(item.Page);
			if (image != null)
			{
				int size = S(16);
				int x = mirrored ? right - size : left;
				graphics.DrawImage(image, new Rectangle(x, bounds.Y + (bounds.Height - size) / 2, size, size));
				if (mirrored)
				{
					right = x - S(8);
				}
				else
				{
					left = x + size + S(8);
				}
			}
			if (right > left)
			{
				bool pageEnabled = enabled && item.Page.Enabled;
				Color color = theme.Color(!pageEnabled ? ThemeRole.TextDisabled : selected ? ThemeRole.Text : ThemeRole.TextMuted);
				System.Drawing.Text.TextRenderingHint hint = graphics.TextRenderingHint;
				graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				graphics.DrawString(item.Page.Text, _tabs.Font, GdiCache.Brush(color), new RectangleF(left, bounds.Y, right - left, bounds.Height), mirrored ? RightToLeft : LeftToRight);
				graphics.TextRenderingHint = hint;
			}
			if (!item.Close.IsEmpty)
			{
				bool closeHot = hot && _hotPart == Part.Close;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				if (closeHot)
				{
					theme.FillRounded(graphics, item.Close, _pressedPart == Part.Close && ReferenceEquals(_pressedPage, item.Page) ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, S(firefox ? 4 : 10));
				}
				Pen pen = GdiCache.Pen(theme.Color(enabled ? ThemeRole.Text : ThemeRole.TextDisabled), Math.Max(1f, Scale * 1.2f));
				int arm = S(4);
				int cx = item.Close.X + item.Close.Width / 2;
				int cy = item.Close.Y + item.Close.Height / 2;
				graphics.DrawLine(pen, cx - arm, cy - arm, cx + arm, cy + arm);
				graphics.DrawLine(pen, cx - arm, cy + arm, cx + arm, cy - arm);
			}
		}

		private static GraphicsPath Rounded(Rectangle r, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			int d = Math.Max(2, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
			path.AddArc(r.X, r.Y, d, d, 180, 90);
			path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
			path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
			path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}

		// What lies under the point.
		internal static Part Find(StripLayout layout, Point point, out MxTabPage page)
		{
			page = null;
			if (!layout.Add.IsEmpty && layout.Add.Contains(point))
			{
				return Part.Add;
			}
			if (!layout.Back.IsEmpty)
			{
				if (layout.Back.Contains(point))
				{
					return Part.Back;
				}
				if (layout.Forward.Contains(point))
				{
					return Part.Forward;
				}
			}
			if (!layout.Area.Contains(point))
			{
				return Part.None;
			}
			foreach (Item item in layout.Items)
			{
				if (item.Bounds.Contains(point))
				{
					page = item.Page;
					return !item.Close.IsEmpty && item.Close.Contains(point) ? Part.Close : Part.Tab;
				}
			}
			return Part.None;
		}

		public bool HitTest(Point point)
		{
			return IsActive && Find(CurrentLayout(), point, out _) != Part.None;
		}

		public void FrameChanged()
		{
			_tabs.OnCaptionFrameChanged();
		}

		public void MouseMove(Point point)
		{
			if (!IsActive)
			{
				return;
			}
			if (!_tabs.Enabled)
			{
				CancelPress();
				SetHot(null, Part.None);
				return;
			}
			StripLayout layout = CurrentLayout();
			// The frame sends moves only while a gesture holds the capture or the pointer is over the caption, so a pressed tab
			// means that the button is still down.
			if (_pressedPage != null && _pressedPart == Part.Tab)
			{
				if (!_dragging && Math.Abs(point.X - _dragStartX) > S(5))
				{
					_dragging = true;
					HideTip();
				}
				if (_dragging)
				{
					Drag(layout, point);
					return;
				}
			}
			Part part = Find(layout, point, out MxTabPage page);
			if (SetHot(page, part) && page != null && part == Part.Tab && _tabs.ShowToolTips && _pressedPart == Part.None)
			{
				StartTip();
			}
		}

		private bool SetHot(MxTabPage page, Part part)
		{
			if (ReferenceEquals(page, _hotPage) && part == _hotPart)
			{
				return false;
			}
			_hotPage = page;
			_hotPart = part;
			_frame.InvalidateCaption();
			HideTip();
			return true;
		}

		// The tab being dragged takes the place of the tab that the pointer passes the middle of.
		private void Drag(StripLayout layout, Point point)
		{
			foreach (Item item in layout.Items)
			{
				if (ReferenceEquals(item.Page, _pressedPage) || !item.Bounds.Contains(point))
				{
					continue;
				}
				int middle = item.Bounds.X + item.Bounds.Width / 2;
				bool after = item.Page.Index > _pressedPage.Index;
				// A later tab lies to the right, or to the left in a mirrored strip.
				bool towardsRight = after != layout.Mirrored;
				if (towardsRight ? point.X > middle : point.X < middle)
				{
					_tabs.TabPages.MovePage(_pressedPage, item.Page);
					_frame.InvalidateCaption();
				}
				break;
			}
		}

		public void MouseLeave()
		{
			HideTip();
			if (_hotPage != null || _hotPart != Part.None)
			{
				_hotPage = null;
				_hotPart = Part.None;
				_frame.InvalidateCaption();
			}
		}

		public void MouseDown(Point point)
		{
			ClearPress();
			if (!IsActive || !_tabs.Enabled)
			{
				return;
			}
			HideTip();
			Part part = Find(CurrentLayout(), point, out MxTabPage page);
			if (page != null && !page.Enabled)
			{
				part = Part.None;
				page = null;
			}
			_pressedPage = page;
			_pressedPart = part;
			_dragStartX = point.X;
			_dragOrigin = page == null ? -1 : page.Index;
			if (part == Part.Tab)
			{
				_tabs.SelectedTab = page;
			}
			_frame.InvalidateCaption();
		}

		public void MouseUp(Point point)
		{
			MxTabPage page = _pressedPage;
			Part pressed = _pressedPart;
			bool dragged = _dragging;
			ClearPress();
			if (pressed != Part.None && !dragged && IsActive && _tabs.Enabled)
			{
				StripLayout layout = CurrentLayout();
				Part over = Find(layout, point, out MxTabPage overPage);
				if (pressed == over && (page == null || ReferenceEquals(page, overPage)))
				{
					Run(pressed, page, layout);
				}
			}
			_frame.InvalidateCaption();
		}

		// The action of a part, for the mouse and for accessibility. The caller has checked that the control is enabled.
		private void Run(Part part, MxTabPage page, StripLayout layout)
		{
			switch (part)
			{
			case Part.Tab:
				if (page != null && page.Enabled && IsShownPage(page))
				{
					_tabs.SelectedTab = page;
				}
				break;
			case Part.Add:
				_tabs.RaiseAddTabClick();
				break;
			case Part.Close:
				if (page != null && page.Enabled && IsShownPage(page) && CanClose(page))
				{
					page.TabVisible = false;
				}
				break;
			case Part.Back:
				_scroll = Math.Max(0, layout.Scroll - layout.Step);
				_version++;
				break;
			case Part.Forward:
				_scroll = Math.Min(layout.MaxScroll, layout.Scroll + layout.Step);
				_version++;
				break;
			}
		}

		public void GestureCanceled()
		{
			// A drag that does not end with a release puts the tab back where it was.
			if (_dragging && _pressedPage != null && _dragOrigin >= 0 && _dragOrigin < _tabs.TabPages.Count && _pressedPage.Index != _dragOrigin)
			{
				_tabs.TabPages.MovePage(_pressedPage, _tabs.TabPages[_dragOrigin]);
			}
			ClearPress();
			_frame.InvalidateCaption();
		}

		public void CaptionReleased()
		{
			_tabs.OnCaptionTabsReleased(this);
		}

		private void ClearPress()
		{
			_pressedPage = null;
			_pressedPart = Part.None;
			_dragging = false;
			_dragOrigin = -1;
		}

		// Ends a press of this extension that the frame still holds.
		internal void CancelPress()
		{
			if (_pressedPart == Part.None)
			{
				return;
			}
			if (ReferenceEquals(_frame.CaptionExtension, this) && _frame.IsCaptionGestureActive)
			{
				_frame.CancelCaptionGesture();
			}
			else
			{
				GestureCanceled();
			}
		}

		internal bool IsPressed => _pressedPart != Part.None;

		private void StartTip()
		{
			if (_tipTimer == null)
			{
				_tipTimer = new Timer { Interval = 600 };
				_tipTimer.Tick += (s, e) => ShowTip();
			}
			_tipTimer.Stop();
			_tipTimer.Start();
		}

		private void ShowTip()
		{
			_tipTimer?.Stop();
			Form form = _tabs.FindForm();
			MxTabPage page = _hotPage;
			if (form == null || page == null || _hotPart != Part.Tab || !IsActive)
			{
				return;
			}
			string text = string.IsNullOrEmpty(page.ToolTipText) ? page.Text : page.ToolTipText;
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (_tip == null)
			{
				_tip = new ThemedToolTip();
			}
			Point at = form.PointToClient(Cursor.Position);
			_tip.Show(text, form, at.X, at.Y + S(24), 5000);
		}

		private void HideTip()
		{
			_tipTimer?.Stop();
			Form form = _tip == null ? null : _form ?? _tabs.FindForm();
			if (form != null && !form.IsDisposed)
			{
				_tip.Hide(form);
			}
		}

		// The accessible objects of the parts that the caption shows, in the order of the strip.
		internal List<AccessibleObject> GetAccessibleObjects()
		{
			StripLayout layout = CurrentLayout();
			if (_accessible != null && ReferenceEquals(_accessibleLayout, layout))
			{
				return _accessible;
			}
			// The objects of the parts that are still there are kept, so a client that holds one keeps a live object.
			List<AccessibleObject> list = new List<AccessibleObject>();
			foreach (Item item in layout.Items)
			{
				list.Add(Reuse(Part.Tab, item.Page));
				if (!item.Close.IsEmpty)
				{
					list.Add(Reuse(Part.Close, item.Page));
				}
			}
			if (!layout.Back.IsEmpty)
			{
				list.Add(Reuse(Part.Back, null));
				list.Add(Reuse(Part.Forward, null));
			}
			if (!layout.Add.IsEmpty)
			{
				list.Add(Reuse(Part.Add, null));
			}
			if (_accessible != null)
			{
				foreach (AccessibleObject old in _accessible)
				{
					if (!list.Contains(old))
					{
						((PartAccessibleObject)old).Alive = false;
					}
				}
			}
			_accessible = list;
			_accessibleLayout = layout;
			return list;
		}

		private AccessibleObject Reuse(Part part, MxTabPage page)
		{
			if (_accessible != null)
			{
				foreach (AccessibleObject old in _accessible)
				{
					PartAccessibleObject candidate = (PartAccessibleObject)old;
					if (candidate.Alive && candidate.Part == part && ReferenceEquals(candidate.Page, page))
					{
						return candidate;
					}
				}
			}
			return new PartAccessibleObject(this, part, page);
		}

		internal int IndexOfAccessibleTab(MxTabPage page)
		{
			List<AccessibleObject> list = GetAccessibleObjects();
			for (int i = 0; i < list.Count; i++)
			{
				PartAccessibleObject part = (PartAccessibleObject)list[i];
				if (part.Part == Part.Tab && ReferenceEquals(part.Page, page))
				{
					return i;
				}
			}
			return -1;
		}

		internal AccessibleObject AccessibleHitTest(Point screen)
		{
			if (!IsActive)
			{
				return null;
			}
			Point point = _frame.ScreenToWindow(screen);
			StripLayout layout = CurrentLayout();
			Part part = Find(layout, point, out MxTabPage page);
			if (part == Part.None)
			{
				return null;
			}
			foreach (AccessibleObject o in GetAccessibleObjects())
			{
				PartAccessibleObject candidate = (PartAccessibleObject)o;
				if (candidate.Part == part && ReferenceEquals(candidate.Page, page))
				{
					return candidate;
				}
			}
			return null;
		}

		private Rectangle BoundsOf(Part part, MxTabPage page, out bool offscreen)
		{
			offscreen = false;
			StripLayout layout = CurrentLayout();
			Rectangle bounds = Rectangle.Empty;
			switch (part)
			{
			case Part.Add:
				bounds = layout.Add;
				break;
			case Part.Back:
				bounds = layout.Back;
				break;
			case Part.Forward:
				bounds = layout.Forward;
				break;
			default:
				foreach (Item item in layout.Items)
				{
					if (ReferenceEquals(item.Page, page))
					{
						bounds = part == Part.Close ? item.Close : item.Bounds;
						break;
					}
				}
				if (!bounds.IsEmpty)
				{
					bounds = Rectangle.Intersect(bounds, layout.Area);
				}
				break;
			}
			offscreen = bounds.IsEmpty;
			return bounds.IsEmpty ? Rectangle.Empty : _frame.WindowToScreen(bounds);
		}

		private bool CanRun(Part part, MxTabPage page)
		{
			if (!IsActive || !_tabs.Enabled)
			{
				return false;
			}
			StripLayout layout = CurrentLayout();
			switch (part)
			{
			case Part.Tab:
			case Part.Close:
				return page != null && page.Enabled && IsShownPage(page) && (part == Part.Tab || CanClose(page));
			case Part.Back:
				return !layout.Back.IsEmpty && layout.Scroll > 0;
			case Part.Forward:
				return !layout.Forward.IsEmpty && layout.Scroll < layout.MaxScroll;
			case Part.Add:
				return !layout.Add.IsEmpty;
			}
			return false;
		}

		private sealed class PartAccessibleObject : AccessibleObject
		{
			private readonly TabCaptionExtension _owner;

			internal PartAccessibleObject(TabCaptionExtension owner, Part part, MxTabPage page)
			{
				_owner = owner;
				Part = part;
				Page = page;
			}

			internal Part Part { get; }

			internal MxTabPage Page { get; }

			internal bool Alive { get; set; } = true;

			public override AccessibleObject Parent => _owner._tabs.AccessibilityObject;

			public override string Name
			{
				get
				{
					switch (Part)
					{
					case Part.Tab:
						return Page.Text;
					case Part.Close:
						return "Close " + Page.Text;
					case Part.Add:
						return "New tab";
					case Part.Back:
						return "Scroll back";
					default:
						return "Scroll forward";
					}
				}
				set
				{
				}
			}

			public override AccessibleRole Role => Part == Part.Tab ? AccessibleRole.PageTab : AccessibleRole.PushButton;

			public override string DefaultAction => Part == Part.Tab ? "Switch" : "Press";

			public override Rectangle Bounds => _owner.IsCurrent(this) ? _owner.BoundsOf(Part, Page, out _) : Rectangle.Empty;

			public override AccessibleStates State
			{
				get
				{
					if (!_owner.IsCurrent(this))
					{
						return AccessibleStates.Unavailable | AccessibleStates.Invisible | AccessibleStates.Offscreen;
					}
					AccessibleStates state = AccessibleStates.None;
					_owner.BoundsOf(Part, Page, out bool offscreen);
					if (offscreen)
					{
						state |= AccessibleStates.Offscreen;
					}
					if (!_owner.CanRun(Part, Page))
					{
						state |= AccessibleStates.Unavailable;
					}
					if (Part == Part.Tab)
					{
						state |= AccessibleStates.Selectable;
						if (ReferenceEquals(_owner._tabs.SelectedTab, Page))
						{
							state |= AccessibleStates.Selected;
						}
					}
					else if (_owner._pressedPart == Part && ReferenceEquals(_owner._pressedPage, Page))
					{
						state |= AccessibleStates.Pressed;
					}
					if (_owner._hotPart == Part && ReferenceEquals(_owner._hotPage, Page))
					{
						state |= AccessibleStates.HotTracked;
					}
					return state;
				}
			}

			public override void DoDefaultAction()
			{
				if (_owner.IsCurrent(this) && _owner.CanRun(Part, Page))
				{
					_owner.Run(Part, Page, _owner.CurrentLayout());
					_owner._frame.InvalidateCaption();
				}
			}

			public override void Select(AccessibleSelection flags)
			{
				if (Part == Part.Tab && (flags & (AccessibleSelection.TakeSelection | AccessibleSelection.AddSelection)) != 0)
				{
					DoDefaultAction();
				}
			}
		}
	}
}
