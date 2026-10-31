using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.UI;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Controls
{
	internal partial class ComboDropDownList : ScrollableHostControl
	{
		private ImageList _imageList;

		private const int SeparatorHeight = 3;

		private const int ItemPadding = 2;

		private const int ImageTextGap = 6;

		private const int PageStep = 7;

		private const int WsChild = 0x40000000;

		private const int WsClipChildren = 0x02000000;

		private const int WsExToolWindow = 0x80;

		private const int WsExTopMost = 0x8;

		private const int WmCancelMode = 0x1F;

		private const int WmActivateApp = 0x1C;

		private const int WmMouseActivate = 0x21;

		private const int MaNoActivateAndEat = 3;

		private const int SwHide = 0;

		private int _maxVisibleItems = 8;

		private int _maxDropDownWidth = -1;

		private ComboBoxItemCollection _items;

		private TextLayoutRenderer _textLayoutRenderer;

		private int _indentWidth;

		private int _lastVisibleIndex = -1;

		private bool _useItemFonts;

		internal DropDownAnimation _dropDownAnimation = DropDownAnimation.System;

		protected IDropListOwner _owner;

		private IntPtr _originalParent = IntPtr.Zero;

		private int _firstVisibleIndex;

		private int _lastMouseY = -1;

		private ComboBoxItem _hotItem;

		private FormatStyle _normalFormatStyle;

		private FormatStyle _highlightFormatStyle;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Parent = IntPtr.Zero;
				createParams.Style = WsChild | WsClipChildren;
				createParams.ExStyle = WsExToolWindow | WsExTopMost;
				return createParams;
			}
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == WmCancelMode || msg.Msg == WmActivateApp)
			{
				HideDropDown(true);
				base.WndProc(ref msg);
			}
			else if (msg.Msg == WmMouseActivate)
			{
				msg.Result = (IntPtr)MaNoActivateAndEat;
				base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new MxAccessibleObject(this);
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (GetContentBounds().Contains(eventArgs.X, eventArgs.Y))
			{
				int itemIndex = GetItemIndexAt(eventArgs.Button, eventArgs.X, eventArgs.Y);
				if (itemIndex != -1 && _items.Count > 0)
				{
					SetHighlightedItem(_items[itemIndex]);
					_lastMouseY = eventArgs.Y;
				}
				base.OnMouseDown(eventArgs);
				return;
			}
			Point point = new Point(eventArgs.X, eventArgs.Y);
			Control control = GetChildAtPoint(point);
			if (control == GetVerticalScrollBar())
			{
				point = PointToScreen(point);
				point = control.PointToClient(point);
				((ThemedScrollBar)control).ForwardMouseDown(eventArgs, point.X, point.Y);
			}
			else
			{
				base.OnMouseDown(eventArgs);
				HideDropDown(true);
			}
		}

		internal ComboDropDownList()
		{
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			UpdateBorderStyle(Mordex.Windows.UI.BorderStyle.Flat);
			base.TabStop = false;
			GetVerticalScrollBar().MouseUp += OnScrollBarMouseUp;
			RightToLeft = RightToLeft.Inherit;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && base.IsHandleCreated && base.InvokeRequired)
			{
				// The window must be destroyed on the thread that owns it, so run the whole
				// disposal there. BeginInvoke does not wait, so a busy UI thread cannot deadlock it.
				try
				{
					BeginInvoke(new MethodInvoker(Dispose));
					return;
				}
				catch (InvalidOperationException)
				{
					// The handle went away in the meantime; dispose here instead.
				}
			}
			_items = null;
			_owner = null;
			base.Dispose(disposing);
		}

		private void OnScrollBarMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (GetIsDropDownOpen())
			{
				RenderingHelper.SetMouseCapture(this, true);
			}
		}

		public ImageList GetImageList()
		{
			return _imageList;
		}

		public void SetImageList(ImageList imageList)
		{
			if (imageList != _imageList)
			{
				_imageList = imageList;
			}
		}

		public void SetMaxVisibleItems(int maxVisibleItems)
		{
			_maxVisibleItems = maxVisibleItems;
		}

		public void SetMaxDropDownWidth(int uiFormatStyleRendererInteger)
		{
			_maxDropDownWidth = uiFormatStyleRendererInteger;
		}

		public ComboBoxItemCollection GetDropDownItems()
		{
			return _items;
		}

		public void SetDropDownItems(ComboBoxItemCollection uiComboBoxItemCollection)
		{
			if (uiComboBoxItemCollection != _items)
			{
				_items = uiComboBoxItemCollection;
			}
		}

		public TextLayoutRenderer GetTextLayoutRenderer()
		{
			if (_textLayoutRenderer == null)
			{
				if (_owner != null)
				{
					_textLayoutRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
				}
				else
				{
					_textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: true);
				}
				_textLayoutRenderer.SetShowHotkeyPrefix(false);
				_textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				_textLayoutRenderer.SetWordWrap(false);
				_textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				_textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
				_textLayoutRenderer.SetRightToLeft(RightToLeft);
			}
			return _textLayoutRenderer;
		}

		public int GetIndentWidth()
		{
			return _indentWidth;
		}

		public void SetIndentWidth(int indentWidth)
		{
			_indentWidth = indentWidth;
		}

		internal void FitToItems(int minWidth)
		{
			if (_items == null || _items.Count == 0)
			{
				int rowHeight = base.FontHeight;
				if (GetImageList() != null && GetImageList().ImageSize.Height > rowHeight)
				{
					rowHeight = GetImageList().ImageSize.Height + ItemPadding;
				}
				base.Height = rowHeight + ItemPadding;
				base.SetIsVerticalScrollNeeded(false);
				return;
			}
			Graphics graphics = CreateGraphics();
			graphics.PageUnit = GraphicsUnit.Pixel;
			int totalHeight = 0;
			int maxWidth = 0;
			int itemCount = 0;
			int minItemHeight = Font.Height + ItemPadding;
			foreach (ComboBoxItem item in _items)
			{
				if (item._isSeparator)
				{
					totalHeight += SeparatorHeight;
					continue;
				}
				itemCount++;
				int itemWidth = ItemPadding;
				int itemHeight = ItemPadding;
				Image image = item.GetDisplayImage(_imageList);
				if (image != null)
				{
					itemWidth += image.Width + ImageTextGap;
					itemHeight += image.Height + ItemPadding;
				}
				if (item.Text.Length != 0)
				{
					itemWidth += (int)GetTextLayoutRenderer().MeasureText(graphics, item.Text, Font, 0).Width + 1;
				}
				if (item.IndentLevel > 0)
				{
					itemWidth += item.IndentLevel * GetIndentWidth();
				}
				if (itemWidth > maxWidth)
				{
					maxWidth = itemWidth;
				}
				if (minItemHeight > itemHeight)
				{
					itemHeight = minItemHeight;
				}
				if (itemCount <= _maxVisibleItems)
				{
					totalHeight += itemHeight;
				}
			}
			maxWidth += 4;
			totalHeight += 4;
			if (GetDropDownItems().Count > _maxVisibleItems)
			{
				maxWidth = ((maxWidth + SystemInformation.VerticalScrollBarWidth));
			}
			if (maxWidth < minWidth)
			{
				maxWidth = minWidth;
			}
			if (_maxDropDownWidth > 0 && maxWidth > _maxDropDownWidth)
			{
				maxWidth = _maxDropDownWidth;
			}
			graphics.Dispose();
			graphics = null;
			base.Size = new Size(maxWidth, totalHeight);
			if (_firstVisibleIndex > _items.Count - _maxVisibleItems)
			{
				_firstVisibleIndex = 0;
			}
			if (GetDropDownItems().Count > _maxVisibleItems)
			{
				SetVerticalScrollRange(0, GetDropDownItems().Count - 1, _maxVisibleItems, _firstVisibleIndex);
				base.SetIsVerticalScrollNeeded(true);
			}
			else
			{
				_firstVisibleIndex = 0;
				base.SetIsVerticalScrollNeeded(false);
			}
		}

		internal Rectangle GetItemBounds(ComboBoxItem targetItem)
		{
			try
			{
				if (!base.IsHandleCreated || _items == null)
				{
					return Rectangle.Empty;
				}
				Rectangle rectangle = GetContentBounds();
				if (_lastVisibleIndex == -1 && _owner != null)
				{
					Update();
				}
				int index = targetItem.Index;
				if (index < _firstVisibleIndex)
				{
					return Rectangle.Empty;
				}
				int top = rectangle.Top;
				int minItemHeight = Font.Height + ItemPadding;
				for (int i = _firstVisibleIndex; i < _items.Count; i++)
				{
					ComboBoxItem currentItem = _items[i];
					int itemHeight = ItemPadding;
					Image image = currentItem.GetDisplayImage(_imageList);
					if (image != null)
					{
						itemHeight += image.Height + ItemPadding;
					}
					if (minItemHeight > itemHeight)
					{
						itemHeight = minItemHeight;
					}
					if (targetItem == currentItem)
					{
						if (currentItem._isSeparator)
						{
							return new Rectangle(rectangle.X, top, rectangle.Width, SeparatorHeight);
						}
						return new Rectangle(rectangle.X, top, rectangle.Width, itemHeight);
					}
					top = ((!currentItem._isSeparator) ? (top + itemHeight) : (top + SeparatorHeight));
				}
				return Rectangle.Empty;
			}
			catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
			{
				// The item list can change while the list is open; report no bounds for the item.
				return Rectangle.Empty;
			}
		}

		private int GetItemIndexAt(MouseButtons buttons, int x, int y)
		{
			Rectangle rectangle = GetContentBounds();
			int edgeIndex = 0;
			if (_items == null || (!rectangle.Contains(x, y) && buttons == MouseButtons.None))
			{
				return -1;
			}
			if (y < rectangle.Top)
			{
				edgeIndex = _firstVisibleIndex - 1;
				if (edgeIndex < 0)
				{
					edgeIndex = 0;
				}
				return edgeIndex;
			}
			if (y > rectangle.Bottom)
			{
				edgeIndex = _lastVisibleIndex + 1;
				if (edgeIndex > GetDropDownItems().Count - 1)
				{
					edgeIndex = GetDropDownItems().Count - 1;
				}
				return edgeIndex;
			}
			if (!base.IsHandleCreated)
			{
				return -1;
			}
			int minItemHeight = Font.Height + ItemPadding;
			int top = rectangle.Top;
			for (int i = _firstVisibleIndex; i < _items.Count; i++)
			{
				ComboBoxItem currentItem = _items[i];
				if (currentItem._isSeparator)
				{
					if (y >= top && y < top + SeparatorHeight)
					{
						return -1;
					}
					top += SeparatorHeight;
					continue;
				}
				int itemHeight = ItemPadding;
				Image image = currentItem.GetDisplayImage(_imageList);
				if (image != null)
				{
					itemHeight += image.Height + ItemPadding;
				}
				if (minItemHeight > itemHeight)
				{
					itemHeight = minItemHeight;
				}
				if (y >= top && y < top + itemHeight)
				{
					return i;
				}
				top += itemHeight;
				if (top > rectangle.Bottom)
				{
					break;
				}
			}
			return _lastVisibleIndex;
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (eventArgs.Button == MouseButtons.Left)
			{
				HideDropDown(!GetContentBounds().Contains(eventArgs.X, eventArgs.Y));
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			DrawItems(eventArgs.Graphics, eventArgs.ClipRectangle);
			PaintBorder(eventArgs.Graphics, null);
			if (_textLayoutRenderer != null)
			{
				_textLayoutRenderer.ResetCache();
				_textLayoutRenderer = null;
			}
		}

		private void DrawItems(Graphics graphics, Rectangle clipRectangle)
		{
			DrawThemedItems(graphics);
		}

		public void SetUseItemFonts(bool uiFormatStyleRendererBoolean)
		{
			_useItemFonts = uiFormatStyleRendererBoolean;
		}

		internal void ShowDropDown(Rectangle ownerBounds, IDropListOwner owner)
		{
			Cursor = Cursors.Arrow;
			_owner = owner;
			base.Width = ownerBounds.Width;
			base.Parent = (Control)_owner;
			FitToItems(ownerBounds.Width);
			_originalParent = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			Point location = new Point(ownerBounds.X, ownerBounds.Bottom + 1);
			Rectangle virtualScreen = SystemInformation.VirtualScreen;
			if (location.X + base.Width > virtualScreen.Right)
			{
				location.X = virtualScreen.Right - base.Width;
			}
			if (location.X < virtualScreen.Left)
			{
				location.X = SystemInformation.VirtualScreen.Left;
			}
			if (location.Y + base.Height > virtualScreen.Bottom)
			{
				if (ownerBounds.Y > virtualScreen.Bottom - ownerBounds.Bottom)
				{
					location.Y = ownerBounds.Y - base.Height;
					if (location.Y < SystemInformation.VirtualScreen.Top)
					{
						location.Y = SystemInformation.VirtualScreen.Top;
						base.Height = ownerBounds.Y;
					}
				}
				else
				{
					base.Height = virtualScreen.Bottom - ownerBounds.Bottom;
				}
			}
			base.Location = location;
			if (GetHighlightedItem() != null)
			{
				SetFirstVisibleIndex(GetHighlightedItem()._listIndex);
			}
			SetIsDropDownOpen(true);
			UpdateThemeListening();
			Show();
			GetVerticalScrollBar().RefreshThumb();
		}

		internal void HideDropDown(bool cancel)
		{
			if (!GetIsDropDownOpen())
			{
				return;
			}
			RenderingHelper.SetMouseCapture(this, false);
			SetIsDropDownOpen(false);
			UpdateThemeListening();
			NativeMethods.ShowWindow(base.Handle, SwHide);
			NativeMethods.SetParent(base.Handle, _originalParent);
			_originalParent = IntPtr.Zero;
			if (!cancel && GetHighlightedItem() != null)
			{
				try
				{
					AccessibilityNotifyClients(AccessibleEvents.Focus, GetHighlightedItem()._listIndex);
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// Accessibility notification is best effort and must not stop the list from closing.
				}
			}
			if (_owner != null)
			{
				_owner.OnDropListHide(cancel);
				_owner = null;
			}
		}

		protected void SetFirstVisibleIndex(int firstIndex)
		{
			if (_items == null)
			{
				firstIndex = 0;
			}
			else if (firstIndex > _items.Count - _maxVisibleItems)
			{
				firstIndex = _items.Count - _maxVisibleItems;
			}
			if (firstIndex < 0)
			{
				firstIndex = 0;
			}
			if (firstIndex != _firstVisibleIndex)
			{
				_firstVisibleIndex = firstIndex;
				GetVerticalScrollBar().SetValue(_firstVisibleIndex);
				Invalidate();
			}
		}

		protected internal override void OnScrollBarScroll(int scrollCode)
		{
			switch (scrollCode)
			{
			case 0:
			case 1:
				SetFirstVisibleIndex(GetVerticalScrollBar().GetValue());
				break;
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (eventArgs.Y != _lastMouseY)
			{
				int itemIndex = GetItemIndexAt(eventArgs.Button, eventArgs.X, eventArgs.Y);
				if (itemIndex != -1 && _items.Count > 0)
				{
					SetHighlightedItem(_items[itemIndex]);
					_lastMouseY = eventArgs.Y;
				}
			}
			if (GetVerticalScrollBar() != null)
			{
				GetVerticalScrollBar().SetIsMouseOver(GetVerticalScrollBar().Bounds.Contains(eventArgs.X, eventArgs.Y));
			}
		}

		public ComboBoxItem GetHighlightedItem()
		{
			return _hotItem;
		}

		public void SetHighlightedItem(ComboBoxItem uiComboBoxItem)
		{
			if (uiComboBoxItem == _hotItem)
			{
				return;
			}
			_hotItem = uiComboBoxItem;
			if (_hotItem != null)
			{
				int index = GetDropDownItems().IndexOf(_hotItem);
				if (index < _firstVisibleIndex)
				{
					SetFirstVisibleIndex(index);
				}
				else if (index > _lastVisibleIndex)
				{
					SetFirstVisibleIndex(index - PageStep);
				}
				if (GetIsDropDownOpen())
				{
					try
					{
						AccessibilityNotifyClients(AccessibleEvents.Focus, index);
					}
					catch (Exception ex) when (!(ex is OutOfMemoryException))
					{
						// Accessibility notification is best effort and must not stop the selection change.
					}
				}
			}
			Invalidate();
		}

		internal void ScrollSelectionIntoView()
		{
			int selectedIndex = GetDropDownItems().IndexOf(_hotItem);
			if (selectedIndex < _firstVisibleIndex)
			{
				SetFirstVisibleIndex(selectedIndex);
				return;
			}
			int firstIndex = selectedIndex;
			if (firstIndex + _maxVisibleItems >= GetDropDownItems().Count)
			{
				firstIndex = GetDropDownItems().Count - _maxVisibleItems;
			}
			SetFirstVisibleIndex(firstIndex);
		}

		private void SelectItemAt(int index)
		{
			if (_items != null)
			{
				if (index < 0)
				{
					index = 0;
				}
				if (index > _items.Count - 1)
				{
					index = _items.Count - 1;
				}
				if (index != -1 && _items.Count > 0)
				{
					SetHighlightedItem(_items[index]);
				}
			}
		}

		internal virtual bool ProcessDropDownKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Prior:
			{
				int pageUpIndex = ((_hotItem != null) ? (_items.IndexOf(_hotItem) - PageStep) : 0);
				SelectItemAt(pageUpIndex);
				break;
			}
			case Keys.Next:
			{
				int pageDownIndex = ((_hotItem != null) ? (_items.IndexOf(_hotItem) + PageStep) : 0);
				SelectItemAt(pageDownIndex);
				break;
			}
			case Keys.Down:
			{
				int nextIndex = _items.IndexOf(_hotItem) + 1;
				SelectItemAt(nextIndex);
				break;
			}
			case Keys.Up:
			{
				int previousIndex = _items.IndexOf(_hotItem) - 1;
				SelectItemAt(previousIndex);
				break;
			}
			case Keys.Home:
				SelectItemAt(0);
				break;
			case Keys.End:
				if (_items != null)
				{
					SelectItemAt(_items.Count);
				}
				break;
			case Keys.Return:
			case Keys.F4:
			case Keys.Up | Keys.Alt:
				if (GetIsDropDownOpen())
				{
					HideDropDown(false);
				}
				break;
			case Keys.Escape:
				if (GetIsDropDownOpen())
				{
					HideDropDown(true);
				}
				break;
			default:
				return false;
			case Keys.Left:
			case Keys.Right:
				break;
			}
			return true;
		}

		internal void SetNormalFormatStyle(FormatStyle uiFormatStyle)
		{
			_normalFormatStyle = uiFormatStyle;
		}

		internal void SetHighlightFormatStyle(FormatStyle uiFormatStyleRendererUIFormatStyle)
		{
			_highlightFormatStyle = uiFormatStyleRendererUIFormatStyle;
		}
	}
}
