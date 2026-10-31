using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	// List boxes, list views and tree views in the older looks. They lose the Explorer look of Windows 11 (hover boxes,
	// chevrons, the pale blue selection): the suite draws the selected row in the selection of the look (navy in Classic,
	// blue in Windows XP, the orange gradient in Office 2003 and 2007) and the frame in the border of the look.
	public static partial class ControlThemer
	{
		private sealed class ListState
		{
			public bool DrawModeSaved;
			public DrawMode ListDrawMode;
			public int ItemHeight;
			public TreeViewDrawMode TreeDrawMode;
			public NativeListFrame Frame;
		}

		private static readonly ConditionalWeakTable<Control, ListState> ListStates = new ConditionalWeakTable<Control, ListState>();

		private static bool IsList(Control control)
		{
			return control is ListBox || control is ListView || control is TreeView;
		}

		// The theme whose native look a control gets: none (the plain look of Windows, with the boxed plus and minus of a
		// tree) for a list in an older look, so the Explorer look of Windows 11 does not show.
		private static MxTheme NativeLookFor(Control control, MxTheme theme)
		{
			return theme != null && IsList(control) && ThemeSelection.IsOlderLook(theme) ? null : theme;
		}

		private static void ApplyListLook(Control control, MxTheme theme, bool themed)
		{
			if (!IsList(control))
			{
				return;
			}
			bool older = themed && ThemeSelection.IsOlderLook(theme);
			ListState state = ListStates.GetValue(control, key => new ListState());
			if (control is ListBox box)
			{
				if (older && !state.DrawModeSaved && box.DrawMode == DrawMode.Normal)
				{
					state.ListDrawMode = box.DrawMode;
					state.ItemHeight = box.ItemHeight;
					state.DrawModeSaved = true;
					box.DrawItem += DrawListBoxItem;
					box.DrawMode = DrawMode.OwnerDrawFixed;
					box.ItemHeight = Math.Max(state.ItemHeight, box.Font.Height);
				}
				else if (!older && state.DrawModeSaved)
				{
					box.DrawItem -= DrawListBoxItem;
					box.DrawMode = state.ListDrawMode;
					box.ItemHeight = state.ItemHeight;
					state.DrawModeSaved = false;
				}
			}
			else if (control is TreeView tree)
			{
				if (older && !state.DrawModeSaved && tree.DrawMode == TreeViewDrawMode.Normal)
				{
					state.TreeDrawMode = tree.DrawMode;
					state.DrawModeSaved = true;
					tree.DrawNode += DrawTreeNode;
					tree.DrawMode = TreeViewDrawMode.OwnerDrawText;
				}
				else if (!older && state.DrawModeSaved)
				{
					tree.DrawNode -= DrawTreeNode;
					tree.DrawMode = state.TreeDrawMode;
					state.DrawModeSaved = false;
				}
			}
			if (older && state.Frame == null)
			{
				state.Frame = new NativeListFrame(control);
			}
			else if (!older && state.Frame != null)
			{
				state.Frame.Detach();
				state.Frame = null;
			}
			control.Invalidate();
		}

		private static MxTheme OlderListTheme()
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			return IsThemed(theme) && ThemeSelection.IsOlderLook(theme) ? theme : null;
		}

		private static void DrawListBoxItem(object sender, DrawItemEventArgs e)
		{
			ListBox box = (ListBox)sender;
			MxTheme theme = OlderListTheme();
			if (theme == null)
			{
				e.DrawBackground();
				return;
			}
			Graphics graphics = e.Graphics;
			Rectangle bounds = e.Bounds;
			bool selected = (e.State & DrawItemState.Selected) != 0;
			graphics.FillRectangle(GdiCache.Brush(box.BackColor), bounds);
			if (e.Index < 0 || e.Index >= box.Items.Count)
			{
				return;
			}
			// A list box shows its selection also without the focus, as the native one does.
			if (selected)
			{
				ThemeSelection.Fill(graphics, bounds, theme, true);
			}
			Color text = !box.Enabled ? theme.GetColor(ThemeRole.TextDisabled) : selected ? ThemeSelection.TextColor(theme, true) : box.ForeColor;
			Rectangle textBounds = new Rectangle(bounds.X + 2, bounds.Y, Math.Max(0, bounds.Width - 4), bounds.Height);
			TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
			if (box.RightToLeft == RightToLeft.Yes)
			{
				flags |= TextFormatFlags.Right | TextFormatFlags.RightToLeft;
			}
			TextRenderer.DrawText(graphics, box.GetItemText(box.Items[e.Index]), e.Font ?? box.Font, textBounds, text, flags);
			if ((e.State & DrawItemState.Focus) != 0 && (e.State & DrawItemState.NoFocusRect) == 0)
			{
				ControlPaint.DrawFocusRectangle(graphics, bounds, text, selected ? theme.GetColor(ThemeRole.Selection) : box.BackColor);
			}
		}

		private static void DrawTreeNode(object sender, DrawTreeNodeEventArgs e)
		{
			TreeView tree = (TreeView)sender;
			MxTheme theme = OlderListTheme();
			if (theme == null || e.Node == null || e.Bounds.Width <= 0 || e.Bounds.Height <= 0 || (e.State & TreeNodeStates.Selected) == 0)
			{
				e.DrawDefault = true;
				return;
			}
			Graphics graphics = e.Graphics;
			bool active = tree.Focused;
			Rectangle bounds = e.Bounds;
			ThemeSelection.Fill(graphics, bounds, theme, active);
			Color text = !tree.Enabled ? theme.GetColor(ThemeRole.TextDisabled) : ThemeSelection.TextColor(theme, active);
			TextRenderer.DrawText(graphics, e.Node.Text, e.Node.NodeFont ?? tree.Font, bounds, text, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
			if (active && (e.State & TreeNodeStates.Focused) != 0)
			{
				ControlPaint.DrawFocusRectangle(graphics, bounds, text, theme.GetColor(ThemeRole.Selection));
			}
		}

		// A selected item of a list view in the details view. Returns false when the native drawing is to be used.
		private static bool DrawListViewSubItem(DrawListViewSubItemEventArgs e)
		{
			ListView list = (ListView)e.Item.ListView;
			MxTheme theme = OlderListTheme();
			if (theme == null || list == null || list.CheckBoxes || !e.Item.Selected || (!list.FullRowSelect && e.ColumnIndex > 0))
			{
				return false;
			}
			bool active = list.Focused;
			if (!active && list.HideSelection)
			{
				return false;
			}
			Graphics graphics = e.Graphics;
			Rectangle bounds = e.Bounds;
			graphics.FillRectangle(GdiCache.Brush(list.BackColor), bounds);
			Rectangle label = bounds;
			if (e.ColumnIndex == 0)
			{
				// The first column holds the image; the selection starts after it, as the native one does.
				Rectangle icon = list.GetItemRect(e.ItemIndex, ItemBoundsPortion.Icon);
				ImageList images = list.SmallImageList;
				if (images != null && icon.Width > 0)
				{
					int index = e.Item.ImageIndex >= 0 ? e.Item.ImageIndex : string.IsNullOrEmpty(e.Item.ImageKey) ? -1 : images.Images.IndexOfKey(e.Item.ImageKey);
					if (index >= 0 && index < images.Images.Count)
					{
						images.Draw(graphics, icon.X, icon.Y + (icon.Height - images.ImageSize.Height) / 2, index);
					}
					label = Rectangle.FromLTRB(Math.Max(bounds.X, icon.Right), bounds.Y, bounds.Right, bounds.Bottom);
				}
				if (list.Columns.Count > 0)
				{
					label.Width = Math.Min(label.Width, list.Columns[0].Width - (label.X - bounds.X));
				}
			}
			ThemeSelection.Fill(graphics, label, theme, active);
			Color text = ThemeSelection.TextColor(theme, active);
			HorizontalAlignment align = e.Header != null ? e.Header.TextAlign : HorizontalAlignment.Left;
			TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine
				| (align == HorizontalAlignment.Right ? TextFormatFlags.Right : align == HorizontalAlignment.Center ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left);
			Rectangle textBounds = Rectangle.Inflate(label, -2, 0);
			string value = e.ColumnIndex == 0 ? e.Item.Text : e.SubItem.Text;
			TextRenderer.DrawText(graphics, value, e.ColumnIndex == 0 ? e.Item.Font : e.SubItem.Font, textBounds, text, flags);
			if (active && e.Item.Focused && e.ColumnIndex == 0)
			{
				Rectangle ring = list.FullRowSelect ? Rectangle.FromLTRB(label.X, label.Y, list.GetItemRect(e.ItemIndex).Right, label.Bottom) : label;
				ControlPaint.DrawFocusRectangle(graphics, ring, text, theme.GetColor(ThemeRole.Selection));
			}
			return true;
		}

		// A selected item in the icon, list and tile views: the image, then the label on the selection.
		private static bool DrawListViewItem(DrawListViewItemEventArgs e)
		{
			ListView list = e.Item.ListView;
			MxTheme theme = OlderListTheme();
			if (theme == null || list == null || list.CheckBoxes || !e.Item.Selected || list.View == View.Details)
			{
				return false;
			}
			bool active = list.Focused;
			if (!active && list.HideSelection)
			{
				return false;
			}
			Graphics graphics = e.Graphics;
			graphics.FillRectangle(GdiCache.Brush(list.BackColor), e.Bounds);
			Rectangle icon = list.GetItemRect(e.ItemIndex, ItemBoundsPortion.Icon);
			Rectangle label = list.GetItemRect(e.ItemIndex, ItemBoundsPortion.Label);
			ImageList images = list.View == View.LargeIcon || list.View == View.Tile ? list.LargeImageList : list.SmallImageList;
			if (images != null && icon.Width > 0)
			{
				int index = e.Item.ImageIndex >= 0 ? e.Item.ImageIndex : string.IsNullOrEmpty(e.Item.ImageKey) ? -1 : images.Images.IndexOfKey(e.Item.ImageKey);
				if (index >= 0 && index < images.Images.Count)
				{
					images.Draw(graphics, icon.X + (icon.Width - images.ImageSize.Width) / 2, icon.Y + (icon.Height - images.ImageSize.Height) / 2, index);
				}
			}
			ThemeSelection.Fill(graphics, label, theme, active);
			Color text = ThemeSelection.TextColor(theme, active);
			TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis
				| (list.View == View.LargeIcon ? TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak | TextFormatFlags.Top : TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.Left);
			TextRenderer.DrawText(graphics, e.Item.Text, e.Item.Font, label, text, flags);
			if (active && e.Item.Focused)
			{
				ControlPaint.DrawFocusRectangle(graphics, label, text, theme.GetColor(ThemeRole.Selection));
			}
			return true;
		}

		/// <summary>
		/// Paints the frame of a list in an older look after Windows has painted it: the sunken 3D edge of Windows Classic
		/// or a line in the border color of the look with the back color inside it.
		/// </summary>
		private sealed class NativeListFrame : NativeWindow
		{
			private const int WmNcPaint = 0x0085;

			private const int WmPrint = 0x0317;

			private const long PrintNonClient = 0x0002;

			private const int GwlStyle = -16;

			private const int GwlExStyle = -20;

			private const int WsBorder = 0x00800000;

			private const int WsExClientEdge = 0x00000200;

			private const uint RedrawFrame = 0x0400;

			private const uint RedrawInvalidate = 0x0001;

			[DllImport("user32.dll")]
			private static extern IntPtr GetWindowDC(IntPtr window);

			[DllImport("user32.dll")]
			private static extern int ReleaseDC(IntPtr window, IntPtr dc);

			[DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
			private static extern int GetWindowLong(IntPtr window, int index);

			[DllImport("user32.dll")]
			private static extern bool RedrawWindow(IntPtr window, IntPtr update, IntPtr region, uint flags);

			private readonly Control _control;

			internal NativeListFrame(Control control)
			{
				_control = control;
				control.HandleCreated += OnHandleCreated;
				control.HandleDestroyed += OnHandleDestroyed;
				if (control.IsHandleCreated)
				{
					AssignHandle(control.Handle);
					Redraw();
				}
			}

			internal void Detach()
			{
				_control.HandleCreated -= OnHandleCreated;
				_control.HandleDestroyed -= OnHandleDestroyed;
				ReleaseHandle();
				Redraw();
			}

			private void OnHandleCreated(object sender, EventArgs e)
			{
				AssignHandle(_control.Handle);
			}

			private void OnHandleDestroyed(object sender, EventArgs e)
			{
				ReleaseHandle();
			}

			private void Redraw()
			{
				if (_control.IsHandleCreated)
				{
					RedrawWindow(_control.Handle, IntPtr.Zero, IntPtr.Zero, RedrawFrame | RedrawInvalidate);
				}
			}

			protected override void WndProc(ref Message m)
			{
				base.WndProc(ref m);
				// A print of the window (DrawToBitmap, PrintWindow) draws the frame itself without a paint message.
				bool print = m.Msg == WmPrint && ((long)m.LParam & PrintNonClient) != 0 && m.WParam != IntPtr.Zero;
				if (m.Msg != WmNcPaint && !print)
				{
					return;
				}
				MxTheme theme = OlderListTheme();
				if (theme == null)
				{
					return;
				}
				bool clientEdge = (GetWindowLong(Handle, GwlExStyle) & WsExClientEdge) != 0;
				bool border = (GetWindowLong(Handle, GwlStyle) & WsBorder) != 0;
				if (!clientEdge && !border)
				{
					return;
				}
				Rectangle bounds = new Rectangle(0, 0, _control.Width, _control.Height);
				if (print)
				{
					using (Graphics graphics = Graphics.FromHdc(m.WParam))
					{
						PaintFrame(graphics, bounds, theme, clientEdge, _control.BackColor);
					}
					return;
				}
				IntPtr dc = GetWindowDC(Handle);
				if (dc == IntPtr.Zero)
				{
					return;
				}
				try
				{
					using (Graphics graphics = Graphics.FromHdc(dc))
					{
						PaintFrame(graphics, bounds, theme, clientEdge, _control.BackColor);
					}
				}
				finally
				{
					ReleaseDC(Handle, dc);
				}
			}
		}

		/// <summary>Draws the frame of a list: the sunken edge of Windows Classic, or one line in the border color with the back color inside.</summary>
		internal static void PaintFrame(Graphics graphics, Rectangle bounds, MxTheme theme, bool clientEdge, Color back)
		{
			if (bounds.Width < 4 || bounds.Height < 4)
			{
				return;
			}
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			if (theme.Style.ButtonLook == ThemeButtonLook.Classic)
			{
				// The sunken edge of Windows 2000: shadow and white outside, dark shadow and face inside.
				Color shadow = theme.GetColor(ThemeRole.Border);
				Color dark = theme.GetColor(ThemeRole.BorderStrong);
				Color face = theme.GetColor(ThemeRole.Surface);
				Pen outerDark = GdiCache.Pen(clientEdge ? shadow : dark);
				graphics.DrawLine(outerDark, bounds.X, bounds.Y, right - 1, bounds.Y);
				graphics.DrawLine(outerDark, bounds.X, bounds.Y, bounds.X, bottom - 1);
				Pen outerLight = GdiCache.Pen(clientEdge ? Color.White : dark);
				graphics.DrawLine(outerLight, bounds.X, bottom, right, bottom);
				graphics.DrawLine(outerLight, right, bounds.Y, right, bottom);
				if (clientEdge)
				{
					graphics.DrawLine(GdiCache.Pen(dark), bounds.X + 1, bounds.Y + 1, right - 2, bounds.Y + 1);
					graphics.DrawLine(GdiCache.Pen(dark), bounds.X + 1, bounds.Y + 1, bounds.X + 1, bottom - 2);
					graphics.DrawLine(GdiCache.Pen(face), bounds.X + 1, bottom - 1, right - 1, bottom - 1);
					graphics.DrawLine(GdiCache.Pen(face), right - 1, bounds.Y + 1, right - 1, bottom - 1);
				}
				return;
			}
			graphics.DrawRectangle(GdiCache.Pen(theme.GetColor(ThemeRole.Border)), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
			if (clientEdge)
			{
				graphics.DrawRectangle(GdiCache.Pen(back), bounds.X + 1, bounds.Y + 1, bounds.Width - 3, bounds.Height - 3);
			}
		}
	}
}
