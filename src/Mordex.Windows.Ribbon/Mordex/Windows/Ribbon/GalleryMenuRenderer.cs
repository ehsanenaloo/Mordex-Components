using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class GalleryMenuRenderer : CommandBaseRenderer, ICommandDesignTarget
	{
		internal IList<CommandBaseRenderer> _itemRenderers;

		private Gallery _gallery;

		private Size _requestedSize;

		private Size _preferredSize;

		private int _proposedColumns;

		private int _itemHeight;

		private int _itemWidth;

		private CommandBaseEditorDialog _menuOwner;

		internal bool _hasScrollBar;

		private int _separatorHeight;

		private int _contentHeight;

		private int _scrollOffset;

		private CommandBaseRenderer _hotItem;

		private RibbonContextMenuRenderer _contextMenuRenderer;

		private RibbonControl _scrollBar;

		private CommandBaseRenderer _designHighlightItem;

		private CommandBase _designSelectedCommand;

		private bool _displayDescription;

		private int _minColumns;

		private int _maxColumns;

		private IList<List<CommandBaseRenderer>> _itemRows;

		Control ICommandDesignTarget.OwnerControl => _menuOwner;

		ImageList ICommandDesignTarget.DesignerImageList
		{
			get
			{
				if (GetRibbon() != null)
				{
					return GetRibbon()._designerImageList;
				}
				return null;
			}
		}

		public override AccessibleRole AccessibleRole => AccessibleRole.ListItem;

		public override CommandType CommandType => CommandType.Gallery;

		public override bool Hot
		{
			get
			{
				return base.Hot;
			}
			set
			{
				if (base.Hot != value)
				{
					base.Hot = value;
					if (!Hot)
					{
						SetHotItem(null);
					}
				}
			}
		}

		public GalleryMenuRenderer(Gallery gallery, ICommandRenderHost owner, int proposedColumns, CommandBaseEditorDialog menuOwner)
			: this(gallery, owner, proposedColumns, menuOwner, displayDescription: true)
		{
		}

		public GalleryMenuRenderer(Gallery gallery, ICommandRenderHost owner, int proposedColumns, CommandBaseEditorDialog menuOwner, bool displayDescription)
			: base(gallery, owner, null)
		{
			_gallery = gallery;
			_menuOwner = menuOwner;
			if (proposedColumns > 0)
			{
				_proposedColumns = proposedColumns;
				_minColumns = proposedColumns;
			}
			else
			{
				_proposedColumns = gallery.MaxGalleryColumns;
				_minColumns = gallery.GetEffectiveMinColumns();
			}
			_maxColumns = gallery.MaxGalleryColumns;
			_isEnabled = true;
			_isVisible = true;
			_displayDescription = displayDescription;
		}

		bool ICommandDesignTarget.CanAddCommand(CommandType commandType)
		{
			return true;
		}

		void ICommandDesignTarget.AddNewCommand(CommandBase command, bool editItem)
		{
			if (_menuOwner != null)
			{
				_menuOwner.AddCommandToContainer(command, GetGallery(), editItem);
			}
		}

		Point ICommandDesignTarget.PointToScreen(Point clientPoint)
		{
			if (_menuOwner != null)
			{
				return _menuOwner.PointToScreen(clientPoint);
			}
			return clientPoint;
		}

		public override void Dispose()
		{
			if (_scrollBar != null)
			{
				_scrollBar.MouseUp -= OnStateMouseUp;
				_scrollBar.MouseDown -= OnStateMouseDown;
				_scrollBar.RemoveScrollHandler(OnScrollBarScroll);
			}
			base.Dispose();
		}

		public MxRibbon GetRibbon()
		{
			if (_gallery != null)
			{
				return _gallery.Ribbon;
			}
			return null;
		}

		public int GetScrollOffset()
		{
			return _scrollOffset;
		}

		public void SetScrollOffset(int scrollOffset)
		{
			if (_scrollOffset != scrollOffset)
			{
				_scrollOffset = scrollOffset;
				if (_menuOwner != null)
				{
					_menuOwner.MarkLayoutDirty();
				}
			}
		}

		public Gallery GetGallery()
		{
			return _gallery;
		}

		public Size GetRequestedSize()
		{
			return _requestedSize;
		}

		public void SetRequestedSize(Size galleryRenderer2Size)
		{
			if (_requestedSize != galleryRenderer2Size)
			{
				_requestedSize = galleryRenderer2Size;
				CalculatePreferredSize();
			}
		}

		public Size GetPreferredSize()
		{
			return _preferredSize;
		}

		public CommandBaseRenderer GetHotItem()
		{
			return _hotItem;
		}

		public void SetHotItem(CommandBaseRenderer galleryRenderer2CommandBaseRenderer)
		{
			if (_hotItem == galleryRenderer2CommandBaseRenderer)
			{
				return;
			}
			if (_hotItem != null)
			{
				_hotItem.Hot = false;
				if (_hotItem.GetCommand() is IMouseEventInvoker)
				{
					((IMouseEventInvoker)_hotItem.GetCommand()).InvokeMouseLeave();
				}
			}
			if (galleryRenderer2CommandBaseRenderer != null)
			{
				if (galleryRenderer2CommandBaseRenderer.GetCommand() is IMouseEventInvoker)
				{
					((IMouseEventInvoker)galleryRenderer2CommandBaseRenderer.GetCommand()).InvokeMouseEnter();
				}
				if (galleryRenderer2CommandBaseRenderer.GetCommand().Ribbon != null)
				{
					galleryRenderer2CommandBaseRenderer.GetCommand().Ribbon.ShowSuperTip(galleryRenderer2CommandBaseRenderer);
				}
			}
			_hotItem = galleryRenderer2CommandBaseRenderer;
			if (_hotItem != null)
			{
				_hotItem.Hot = true;
			}
			Invalidate();
		}

		public int GetProposedColumns()
		{
			return _proposedColumns;
		}

		public void SetProposedColumns(int proposedColumns)
		{
			if (_proposedColumns != proposedColumns)
			{
				_proposedColumns = proposedColumns;
				RebuildItemRows();
			}
		}

		public RibbonContextMenuRenderer GetContextMenuRenderer()
		{
			if (_contextMenuRenderer == null)
			{
				_contextMenuRenderer = new RibbonContextMenuRenderer(GetRenderHost(), this, drawImageArea: true);
			}
			return _contextMenuRenderer;
		}

		internal CommandBase GetDesignSelectedCommand()
		{
			return _designSelectedCommand;
		}

		internal void SetDesignSelectedCommand(CommandBase commandBase)
		{
			if (_designSelectedCommand != commandBase)
			{
				_designSelectedCommand = commandBase;
				Invalidate();
			}
		}

		internal CommandBaseRenderer GetDesignHighlightItem()
		{
			return _designHighlightItem;
		}

		internal void SetDesignHighlightItem(CommandBaseRenderer highlightItem)
		{
			if (_designHighlightItem != highlightItem)
			{
				_designHighlightItem = highlightItem;
				Invalidate();
			}
		}

		public override void Invalidate()
		{
			if (_menuOwner != null)
			{
				_menuOwner.Invalidate(_bounds);
			}
		}

		internal CommandBaseRenderer GetItemAt(int x, int y)
		{
			if (_itemRenderers != null)
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item._bounds.Contains(x, y))
					{
						return item;
					}
				}
			}
			return null;
		}

		private Size GetItemImageSize(GalleryItem galleryItem)
		{
			if (RibbonDrawingUtils.HasImageSource(galleryItem.Icon, galleryItem.Image, _gallery.ImageList, galleryItem.ImageIndex, galleryItem.ImageKey))
			{
				if (!_gallery.ImageSize.IsEmpty)
				{
					return _gallery.ImageSize;
				}
				if (galleryItem.Icon != null)
				{
					return galleryItem.Icon.Size;
				}
				if (galleryItem.Image != null)
				{
					return galleryItem.Image.Size;
				}
				if (_gallery.ImageList != null)
				{
					return _gallery.ImageList.ImageSize;
				}
			}
			return Size.Empty;
		}

		protected int MeasureItemWidth(Graphics graphics, TextLayoutRenderer textLayoutRenderer, Font font)
		{
			if (_gallery.ItemsWidth > 0)
			{
				return _gallery.ItemsWidth;
			}
			if (_gallery.GalleryItems.Count == 0)
			{
				return 100;
			}
			int maxWidth = 0;
			bool isTextBesideImage = _gallery.ItemsTextImageRelation != TextImageRelation.TextAboveImage && _gallery.ItemsTextImageRelation != TextImageRelation.ImageAboveText;
			foreach (GalleryItem galleryItem in _gallery.GalleryItems)
			{
				int itemWidth = 0;
				Size size = GetItemImageSize(galleryItem);
				if (!size.IsEmpty)
				{
					itemWidth = size.Width + 4;
				}
				if (galleryItem.Text.Length > 0 && galleryItem.GetEffectiveStyle() != CommandStyle.Image)
				{
					int textWidth = (int)textLayoutRenderer.MeasureStyledText(graphics, galleryItem.Text, galleryItem.ForegroundStyle, font, 0, 0).Width + 3;
					if (isTextBesideImage || size.IsEmpty)
					{
						if (galleryItem.Description.Length > 0)
						{
							int descriptionWidth = RibbonDrawingUtils.MeasureTwoLineTextWidth(graphics, textLayoutRenderer, galleryItem.Description, galleryItem.DescriptionForegroundStyle, font, textWidth, 0);
							if (descriptionWidth > textWidth)
							{
								textWidth = descriptionWidth;
							}
						}
						itemWidth += textWidth;
					}
					else if (textWidth > itemWidth)
					{
						itemWidth = textWidth;
					}
				}
				itemWidth += 4;
				if (itemWidth > maxWidth)
				{
					maxWidth = itemWidth;
				}
			}
			if (maxWidth == 0)
			{
				return GetRenderHost().LargeImageSize.Width;
			}
			return maxWidth;
		}

		protected int MeasureItemHeight(Graphics graphics, TextLayoutRenderer textLayoutRenderer, Font font)
		{
			if (_gallery.ItemsHeight > 0)
			{
				return _gallery.ItemsHeight;
			}
			if (_gallery.GalleryItems.Count == 0)
			{
				return GetRenderHost().LargeImageSize.Height;
			}
			int maxHeight = GetRenderHost().SmallItemHeight;
			bool isTextBesideImage = _gallery.ItemsTextImageRelation != TextImageRelation.TextAboveImage && _gallery.ItemsTextImageRelation != TextImageRelation.ImageAboveText;
			foreach (GalleryItem galleryItem in _gallery.GalleryItems)
			{
				int itemHeight = 0;
				Size size = GetItemImageSize(galleryItem);
				if (!size.IsEmpty)
				{
					itemHeight = size.Height + 4;
				}
				if (galleryItem.Text.Length > 0 && galleryItem.GetEffectiveStyle() != CommandStyle.Image)
				{
					if (!isTextBesideImage || size.IsEmpty)
					{
						itemHeight += font.Height + 4;
					}
					else
					{
						int textHeight = font.Height + 3;
						if (galleryItem.Description.Length > 0)
						{
							textHeight += (font.Height + 3) * 2;
						}
						if (textHeight > itemHeight)
						{
							itemHeight = textHeight;
						}
					}
				}
				itemHeight += 4;
				if (itemHeight > maxHeight)
				{
					maxHeight = itemHeight;
				}
			}
			return maxHeight;
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int height)
		{
			if (_scrollOffset < 0)
			{
				_scrollOffset = 0;
			}
			Font font = GetRenderHost().Font;
			_bounds = new Rectangle(left, top, width, height);
			_requestedSize = new Size(width, height);
			LayoutItems(graphics, textRenderer, font, left, top);
			CalculatePreferredSize();
			if (_hasScrollBar)
			{
				ConfigureScrollBar(_bounds, 0, _contentHeight - 1, _bounds.Height, _scrollOffset, _itemHeight);
			}
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_itemRenderers = new List<CommandBaseRenderer>();
			foreach (GalleryItem galleryItem in _gallery.GalleryItems)
			{
				if (galleryItem.Visible)
				{
					if (galleryItem.ItemType == GalleryItemType.GalleryTitle)
					{
						_itemRenderers.Add(new RibbonForegroundStyleRenderer(galleryItem, GetRenderHost()));
					}
					else
					{
						_itemRenderers.Add(new GalleryItemRenderer(galleryItem, GetRenderHost(), this, _displayDescription));
					}
				}
			}
			if (GetRibbon() != null && GetRibbon().IsInDesignMode())
			{
				_itemRenderers.Add(new GalleryState(GetRibbon(), _gallery));
			}
			RebuildItemRows();
			_separatorHeight = font.Height + 6;
			_itemWidth = MeasureItemWidth(graphics, textRenderer, font);
			_itemHeight = MeasureItemHeight(graphics, textRenderer, font);
			CalculatePreferredSize();
			_fullItemWidth = _preferredSize.Width;
		}

		private void CalculatePreferredSize()
		{
			if (_itemRenderers == null)
			{
				return;
			}
			int columns = _proposedColumns;
			int rows = _gallery.MaxGalleryRows;
			int itemCount = 0;
			int separatorCount = 0;
			int contentHeight = 0;
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item.CommandType == CommandType.Separator)
				{
					separatorCount++;
				}
				else
				{
					itemCount++;
				}
			}
			if (!GetRequestedSize().IsEmpty)
			{
				columns = ((!_hasScrollBar) ? (GetRequestedSize().Width / _itemWidth) : ((GetRequestedSize().Width - SystemInformation.VerticalScrollBarWidth) / _itemWidth));
				if (columns < _minColumns)
				{
					columns = _minColumns;
				}
				if (columns > _maxColumns && _maxColumns > 0)
				{
					columns = _maxColumns;
				}
				SetProposedColumns(columns);
				rows = GetRequestedSize().Height / _itemHeight;
				if (_gallery.MaxGalleryRows > 0 && rows > _gallery.MaxGalleryRows)
				{
					rows = _gallery.MaxGalleryRows;
				}
				if (rows < _gallery.GetEffectiveMinRows())
				{
					rows = _gallery.GetEffectiveMinRows();
				}
				int rowsHeight = rows * _itemHeight;
				if (GetRequestedSize().Height > rowsHeight)
				{
					if (rows <= _gallery.MaxGalleryRows)
					{
						_requestedSize = new Size(_requestedSize.Width, rowsHeight);
					}
					contentHeight = GetRequestedSize().Height;
				}
				else
				{
					contentHeight = rowsHeight;
				}
			}
			else
			{
				if (columns < _minColumns)
				{
					columns = _minColumns;
				}
				int requiredRows = itemCount / columns;
				if (requiredRows == 0 || itemCount % columns != 0)
				{
					requiredRows++;
				}
				if (rows <= 0)
				{
					rows = requiredRows + separatorCount;
				}
				if (rows < _gallery.GetEffectiveMinRows())
				{
					rows = _gallery.GetEffectiveMinRows();
				}
				SetProposedColumns(columns);
				if (requiredRows > _gallery.MaxGalleryRows && _gallery.MaxGalleryRows > 0)
				{
					_hasScrollBar = true;
				}
				if (_gallery.MaxGalleryRows > 0 && requiredRows > _gallery.MaxGalleryRows)
				{
					requiredRows = _gallery.MaxGalleryRows;
				}
				contentHeight = requiredRows * _itemHeight + separatorCount * _separatorHeight;
				if (_gallery.Commands.Count > 0)
				{
					contentHeight++;
				}
			}
			Size size = new Size(columns * _itemWidth, contentHeight);
			if (_gallery.AllowDropDownResize)
			{
				_hasScrollBar = true;
			}
			if (_hasScrollBar)
			{
				size.Width += SystemInformation.VerticalScrollBarWidth;
				if (size.Height < SystemInformation.VerticalScrollBarArrowHeight * 2)
				{
					size.Height = SystemInformation.VerticalScrollBarArrowHeight * 2;
				}
			}
			if (size.Width < _requestedSize.Width)
			{
				size.Width = _requestedSize.Width;
			}
			if (size.Height < _requestedSize.Height)
			{
				size.Height = _requestedSize.Height;
			}
			_preferredSize = size;
		}

		public void RebuildItemRows()
		{
			_itemRows = new List<List<CommandBaseRenderer>>();
			List<CommandBaseRenderer> list = new List<CommandBaseRenderer>();
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item.CommandType == CommandType.Separator)
				{
					if (list.Count > 0)
					{
						_itemRows.Add(list);
						list = new List<CommandBaseRenderer>();
					}
					list.Add(item);
					_itemRows.Add(list);
					list = new List<CommandBaseRenderer>();
				}
				else
				{
					if (list.Count >= GetProposedColumns())
					{
						_itemRows.Add(list);
						list = new List<CommandBaseRenderer>();
						list.Add(item);
					}
					else
					{
						list.Add(item);
					}
					((GalleryItemRenderer)item)._rowIndex = _itemRows.Count;
					((GalleryItemRenderer)item)._columnInGalleryRow = list.Count - 1;
				}
			}
			if (list.Count > 0)
			{
				_itemRows.Add(list);
			}
		}

		private void LayoutItems(Graphics graphics, TextLayoutRenderer textRenderer, Font font, int left, int top)
		{
			int trailingRows = 0;
			int currentTop = top;
			int currentLeft = left;
			int rowHeight = 0;
			if (GetProposedColumns() == 1)
			{
				int width = _preferredSize.Width;
				if (_hasScrollBar)
				{
					width -= SystemInformation.VerticalScrollBarWidth;
				}
			}
			_contentHeight = 0;
			currentTop -= GetScrollOffset();
			foreach (List<CommandBaseRenderer> item in _itemRows)
			{
				foreach (CommandBaseRenderer itemRenderer in item)
				{
					int width;
					if (itemRenderer.CommandType == CommandType.Separator)
					{
						width = _bounds.Width;
						if (_hasScrollBar)
						{
							width -= SystemInformation.VerticalScrollBarWidth;
						}
						rowHeight = ((RibbonForegroundStyleRenderer)itemRenderer).GetPreferredHeight(font);
					}
					else
					{
						width = _itemWidth;
						rowHeight = _itemHeight;
					}
					itemRenderer.ArrangeItem(graphics, textRenderer, currentLeft, currentTop, width, rowHeight);
					currentLeft += _itemWidth;
				}
				currentTop += rowHeight;
				currentLeft = left;
				_contentHeight += rowHeight;
			}
			if (trailingRows > 0)
			{
				_contentHeight += rowHeight;
				currentTop += rowHeight;
			}
			if (currentTop < _bounds.Bottom)
			{
				_scrollOffset -= _bounds.Bottom - currentTop;
			}
		}

		internal void ConfigureScrollBar(Rectangle rectangle, int minimum, int maximum, int largeChange, int scrollValue, int smallChange)
		{
			try
			{
				if (_scrollBar == null)
				{
					_scrollBar = new RibbonControl(ScrollBarType.Vertical);
					_scrollBar.AddScrollHandler(OnScrollBarScroll);
					_scrollBar.MouseDown += OnStateMouseDown;
					_scrollBar.MouseUp += OnStateMouseUp;
				}
				if (GetRibbon() != null)
				{
				}
				_scrollBar.SetMinimum(minimum);
				_scrollBar.SetMaximum(maximum);
				if (largeChange > 0)
				{
					_scrollBar.SetLargeChange(largeChange);
				}
				else
				{
					_scrollBar.SetLargeChange(smallChange);
				}
				_scrollBar.SetSmallChange(smallChange);
				_scrollBar.SetScrollValue(scrollValue);
				if (maximum <= largeChange)
				{
					_scrollBar.Enabled = false;
				}
				else
				{
					_scrollBar.Enabled = true;
				}
				int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
				if (_menuOwner.RightToLeft == RightToLeft.Yes)
				{
					_scrollBar.Location = new Point(rectangle.X, rectangle.Top);
				}
				else
				{
					_scrollBar.Location = new Point(rectangle.Right - verticalScrollBarWidth, rectangle.Top);
				}
				_scrollBar.Size = new Size(verticalScrollBarWidth, rectangle.Height);
				if (!_menuOwner.Controls.Contains(_scrollBar))
				{
					_menuOwner.Controls.Add(_scrollBar);
				}
			}
			catch (System.Exception ex) when (ex is System.ObjectDisposedException || ex is System.InvalidOperationException)
			{
				// The menu or its scroll bar was disposed while the layout was running.
			}
		}

		private void OnStateMouseUp(object sender, MouseEventArgs eventArgs)
		{
			_menuOwner.SetPressedScrollBar(null);
		}

		private void OnStateMouseDown(object sender, MouseEventArgs eventArgs)
		{
			_menuOwner.SetPressedScrollBar(_scrollBar);
		}

		private void OnScrollBarScroll(object sender, ScrollEventArgs eventArgs)
		{
			SetScrollOffset(eventArgs.NewValue);
		}

		public void MoveSelectionUp()
		{
			if (GetHotItem() == null)
			{
				return;
			}
			GalleryItemRenderer galleryItemRenderer = (GalleryItemRenderer)GetHotItem();
			int startRow = galleryItemRenderer._rowIndex - 1;
			int columnIndex = galleryItemRenderer._columnInGalleryRow;
			for (int rowIndex = startRow; rowIndex >= 0; rowIndex--)
			{
				List<CommandBaseRenderer> list = _itemRows[rowIndex];
				if (list.Count > columnIndex)
				{
					CommandBaseRenderer thisValue = list[columnIndex];
					if (thisValue.CommandType != CommandType.Separator)
					{
						SetHotItem(thisValue);
						return;
					}
				}
			}
			if (_menuOwner != null)
			{
				_menuOwner.SelectPrevious();
			}
		}

		public void MoveSelectionDown()
		{
			if (GetHotItem() == null)
			{
				return;
			}
			GalleryItemRenderer galleryItemRenderer = (GalleryItemRenderer)GetHotItem();
			int startRow = galleryItemRenderer._rowIndex + 1;
			int columnIndex = galleryItemRenderer._columnInGalleryRow;
			for (int i = startRow; i < _itemRows.Count; i++)
			{
				List<CommandBaseRenderer> list = _itemRows[i];
				if (list.Count > columnIndex)
				{
					CommandBaseRenderer thisValue = list[columnIndex];
					if (thisValue.CommandType != CommandType.Separator)
					{
						SetHotItem(thisValue);
						return;
					}
				}
			}
			if (_menuOwner != null)
			{
				_menuOwner.SelectNext();
			}
		}

		public void MoveSelectionLeft()
		{
			if (GetHotItem() == null || GetProposedColumns() <= 0)
			{
				return;
			}
			GalleryItemRenderer galleryItemRenderer = (GalleryItemRenderer)GetHotItem();
			List<CommandBaseRenderer> list = _itemRows[galleryItemRenderer._rowIndex];
			if (list.Count == 1)
			{
				return;
			}
			int startColumn = galleryItemRenderer._columnInGalleryRow - 1;
			for (int columnIndex = startColumn; columnIndex >= 0; columnIndex--)
			{
				CommandBaseRenderer thisValue = list[columnIndex];
				if (thisValue.CommandType != CommandType.Separator)
				{
					SetHotItem(thisValue);
					return;
				}
			}
			for (int wrapIndex = list.Count - 1; wrapIndex >= 0; wrapIndex--)
			{
				CommandBaseRenderer wrapCandidate = list[wrapIndex];
				if (wrapCandidate.CommandType != CommandType.Separator)
				{
					SetHotItem(wrapCandidate);
					break;
				}
			}
		}

		public void MoveSelectionRight()
		{
			if (GetHotItem() == null || GetProposedColumns() <= 0)
			{
				return;
			}
			GalleryItemRenderer galleryItemRenderer = (GalleryItemRenderer)GetHotItem();
			List<CommandBaseRenderer> list = _itemRows[galleryItemRenderer._rowIndex];
			if (list.Count == 1)
			{
				return;
			}
			int startColumn = galleryItemRenderer._columnInGalleryRow + 1;
			for (int i = startColumn; i < list.Count; i++)
			{
				CommandBaseRenderer thisValue = list[i];
				if (thisValue.CommandType != CommandType.Separator)
				{
					SetHotItem(thisValue);
					return;
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				CommandBaseRenderer wrapCandidate = list[j];
				if (wrapCandidate.CommandType != CommandType.Separator)
				{
					SetHotItem(wrapCandidate);
					break;
				}
			}
		}

		public void InvokeHotItem()
		{
			if (GetHotItem() != null && GetHotItem().GetCommand().Enabled && GetHotItem().GetCommand().CanBeClicked)
			{
				GetHotItem().GetCommand().InvokeOnClick();
			}
		}

		public void ScrollByWheel(int wheelDelta)
		{
			if (_scrollBar != null)
			{
				if (wheelDelta < 0)
				{
					RibbonControl ribbonControl = _scrollBar;
					ribbonControl.SetScrollValue(ribbonControl.GetScrollValue() + _scrollBar.GetSmallChange());
				}
				else if (wheelDelta > 0)
				{
					RibbonControl scrollBarDown = _scrollBar;
					scrollBarDown.SetScrollValue(scrollBarDown.GetScrollValue() - _scrollBar.GetSmallChange());
				}
				SetScrollOffset(_scrollBar.GetScrollValue());
			}
		}

		internal bool TrySelectLastItem()
		{
			if (_itemRenderers != null)
			{
				for (int index = _itemRenderers.Count - 1; index >= 0; index--)
				{
					CommandBaseRenderer thisValue = _itemRenderers[index];
					if (thisValue.CommandType != CommandType.Separator && thisValue._isEnabled)
					{
						SetHotItem(thisValue);
						return true;
					}
				}
			}
			return false;
		}

		internal bool TrySelectFirstItem()
		{
			if (_itemRenderers != null)
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item.CommandType != CommandType.Separator && item._isEnabled)
					{
						SetHotItem(item);
						return true;
					}
				}
			}
			return false;
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = GetItemAt(eventArgs.X, eventArgs.Y);
			GetRenderHost().MouseDownItem = commandBaseRenderer;
			if (commandBaseRenderer != null && eventArgs.Button == MouseButtons.Left)
			{
				commandBaseRenderer.Pushed = true;
			}
			commandBaseRenderer?.OnMouseDown(eventArgs);
		}

		public override void OnMouseMove(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = GetItemAt(eventArgs.X, eventArgs.Y);
			if (commandBaseRenderer != null && commandBaseRenderer.CommandType == CommandType.Separator)
			{
				commandBaseRenderer = null;
			}
			if (commandBaseRenderer != null && eventArgs.Button != MouseButtons.None)
			{
				GetRenderHost().MouseDownItem = commandBaseRenderer;
				if (eventArgs.Button == MouseButtons.Left)
				{
					commandBaseRenderer.Pushed = true;
				}
			}
			SetHotItem(commandBaseRenderer);
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = GetItemAt(eventArgs.X, eventArgs.Y);
			if (commandBaseRenderer != null && commandBaseRenderer.GetCommand().Enabled)
			{
				if (!commandBaseRenderer.Pushed && eventArgs.Button == MouseButtons.Left)
				{
					commandBaseRenderer.GetCommand().InvokeOnClick();
				}
				commandBaseRenderer.OnMouseUp(eventArgs);
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			switch (_gallery.ItemsTextAlignment)
			{
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomRight:
				textRenderer.SetLineAlignment(StringAlignment.Far);
				break;
			case ContentAlignment.TopLeft:
			case ContentAlignment.TopCenter:
			case ContentAlignment.TopRight:
				textRenderer.SetLineAlignment(StringAlignment.Near);
				break;
			default:
				textRenderer.SetLineAlignment(StringAlignment.Center);
				break;
			}
			switch (_gallery.ItemsTextAlignment)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.BottomLeft:
				textRenderer.SetStringAlignment(StringAlignment.Near);
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				textRenderer.SetStringAlignment(StringAlignment.Far);
				break;
			default:
				textRenderer.SetStringAlignment(StringAlignment.Center);
				break;
			}
			GraphicsState savedState = null;
			savedState = graphics.Save();
			graphics.SetClip(_bounds);
			textRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				item.Paint(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
			}
			if (_menuOwner._isDesignMode)
			{
				PaintDesignSelection(graphics);
			}
			if (_gallery.Commands.Count > 0)
			{
				Pen pen = GdiCache.Pen(colorScheme._menuSeparator);
				graphics.DrawLine(pen, _bounds.X, _bounds.Bottom - 1, _bounds.Right - 1, _bounds.Bottom - 1);
			}
			graphics.Restore(savedState);
			return;
		}

		private void PaintDesignSelection(Graphics graphics)
		{
			if (GetDesignHighlightItem() != null)
			{
				DrawSelectionRectangle(graphics, GetDesignHighlightItem()._bounds, DashStyle.Dot);
			}
			if (GetDesignSelectedCommand() != null)
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item.GetCommand() == GetDesignSelectedCommand())
					{
						DrawSelectionRectangle(graphics, item._bounds, DashStyle.Solid);
						break;
					}
				}
			}
			ISelectionService selectionService = (ISelectionService)GetRenderHost().Site.GetService(typeof(ISelectionService));
			if (selectionService == null || selectionService.SelectionCount <= 1)
			{
				return;
			}
			foreach (Component selectedComponent in selectionService.GetSelectedComponents())
			{
				if (!(selectedComponent is GalleryItem))
				{
					continue;
				}
				foreach (CommandBaseRenderer selectedRenderer in _itemRenderers)
				{
					if (selectedRenderer.GetCommand() == selectedComponent)
					{
						DrawSelectionRectangle(graphics, selectedRenderer._bounds, DashStyle.Dot);
						break;
					}
				}
			}
		}

		private void DrawSelectionRectangle(Graphics graphics, Rectangle rectangle, DashStyle dashStyle)
		{
			Pen pen = new Pen(Color.Black);
			pen.DashStyle = dashStyle;
			if (rectangle.Height <= 4)
			{
				rectangle.Y -= 2;
				rectangle.Height = 4;
			}
			RibbonDrawingUtils.DrawRectangleOutline(graphics, pen, rectangle);
			pen.Dispose();
		}
	}
}
