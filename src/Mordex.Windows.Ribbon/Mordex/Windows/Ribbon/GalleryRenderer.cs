using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class GalleryRenderer : RibbonDropDownButtonRenderer, IKeyboardNavigable, IDropDownItem
	{
		private int _columns;

		internal IList<CommandBaseRenderer> _itemRenderers;

		private Gallery _gallery;

		private int _itemWidth;

		private int _itemHeight;

		private Rectangle _itemsViewport;

		private GalleryScrollButtonRenderer _pressedScrollButton;

		private GalleryScrollButtonRenderer _hotScrollButton;

		internal GalleryScrollButtonRenderer _scrollUpButton;

		internal GalleryScrollButtonRenderer _scrollDownButton;

		internal GalleryScrollButtonRenderer _expandButton;

		private int _scrollRow;

		private const int ScrollRepeatDelayMilliseconds = 500;

		private const int ScrollRepeatIntervalMilliseconds = 100;

		private Timer _scrollTimer;

		private int _maxScrollRow;

		private Rectangle _galleryBounds;

		private CommandBaseRenderer _hoveredItem;

		internal bool _hasReducedColumns;

		public override AccessibleObject AccessibilityObject
		{
			get
			{
				if (_cachedAccessibilityObject == null)
				{
					_cachedAccessibilityObject = new GalleryAccessibleObject(this);
				}
				return _cachedAccessibilityObject;
			}
		}

		public override AccessibleRole AccessibleRole => AccessibleRole.List;

		public override CommandType CommandType => CommandType.Gallery;

		public virtual int Columns
		{
			get
			{
				return _columns;
			}
			set
			{
				if (_columns != value)
				{
					_columns = value;
				}
			}
		}

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
					if (!value)
					{
						SetHotScrollButton(null);
						SetHoveredItem(null);
					}
					base.Hot = value;
				}
			}
		}

		public override bool Active
		{
			get
			{
				return base.Active;
			}
			set
			{
				base.Active = value;
				if (_expandButton != null)
				{
					if (value)
					{
						_expandButton.Hot = true;
					}
					else
					{
						_expandButton.Hot = false;
					}
				}
			}
		}

		public GalleryRenderer(Gallery gallery, ICommandRenderHost owner, CommandBaseRenderer itemOwner, int maxColumns)
			: base(gallery, owner, itemOwner)
		{
			_gallery = gallery;
			_scrollRow = 0;
			if (_gallery != null)
			{
				_scrollRow = _gallery._scrollRowOffset;
			}
			if (maxColumns > 0)
			{
				_columns = maxColumns;
			}
			else
			{
				int visibleItemCount = 0;
				foreach (GalleryItem galleryItem in gallery.GalleryItems)
				{
					if (galleryItem.Visible)
					{
						visibleItemCount++;
					}
				}
				_columns = visibleItemCount;
			}
			_commandBaseRendererRibbon = gallery.Ribbon;
			_isSingleWordText = gallery.Text.IndexOf(" ") <= 0;
		}

		internal bool IsCollapsedToButton()
		{
			if (_gallery.MinGalleryColumns <= 0 && (_itemRenderers == null || _itemRenderers.Count == 0))
			{
				if (_hasReducedColumns && _columns < 0)
				{
					return true;
				}
				return false;
			}
			if (_columns >= _gallery.GetEffectiveMinColumns())
			{
				return false;
			}
			return true;
		}

		internal void SetHoveredItem(CommandBaseRenderer galleryRendererCommandBaseRenderer)
		{
			if (_hoveredItem == galleryRendererCommandBaseRenderer)
			{
				return;
			}
			if (_hoveredItem != null)
			{
				_hoveredItem.StartPressAnimation();
				_hoveredItem.Hot = false;
				if (_hoveredItem.GetCommand() is IMouseEventInvoker)
				{
					((IMouseEventInvoker)_hoveredItem.GetCommand()).InvokeMouseLeave();
				}
			}
			if (galleryRendererCommandBaseRenderer != null && galleryRendererCommandBaseRenderer.GetCommand() is IMouseEventInvoker)
			{
				((IMouseEventInvoker)galleryRendererCommandBaseRenderer.GetCommand()).InvokeMouseEnter();
			}
			_hoveredItem = galleryRendererCommandBaseRenderer;
			if (_hoveredItem != null)
			{
				_hoveredItem.Hot = true;
			}
			if (GetCommand().Ribbon != null)
			{
				GetCommand().Ribbon.ShowSuperTip(_hoveredItem);
			}
		}

		public void SetHotScrollButton(GalleryScrollButtonRenderer hotButton)
		{
			if (_hotScrollButton == hotButton)
			{
				return;
			}
			if (_hotScrollButton != null)
			{
				_hotScrollButton.Hot = false;
			}
			_hotScrollButton = hotButton;
			if (_hotScrollButton != null)
			{
				_hotScrollButton.Hot = true;
				if (GetRenderHost().FocusItem != this)
				{
					GetRenderHost().FocusItem = null;
				}
			}
		}

		public void SetPressedScrollButton(GalleryScrollButtonRenderer pressedButton)
		{
			if (_pressedScrollButton != pressedButton)
			{
				if (_pressedScrollButton != null)
				{
					StopScrollTimer();
					_pressedScrollButton.Pushed = false;
				}
				_pressedScrollButton = pressedButton;
				if (_pressedScrollButton != null)
				{
					OnScrollButtonPressed(_pressedScrollButton);
				}
			}
		}

		public int GetScrollRow()
		{
			return _scrollRow;
		}

		public void SetScrollRow(int scrollRow)
		{
			if (scrollRow > _maxScrollRow)
			{
				StopScrollTimer();
				scrollRow = _maxScrollRow;
			}
			if (scrollRow < 0)
			{
				StopScrollTimer();
				scrollRow = 0;
			}
			if (_scrollRow != scrollRow)
			{
				_scrollRow = scrollRow;
				if (_gallery != null)
				{
					_gallery._scrollRowOffset = _scrollRow;
				}
				RecalculateLayout();
				Invalidate();
			}
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

		protected virtual int GetItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (_gallery.ItemsWidth > 0)
			{
				return _gallery.ItemsWidth;
			}
			int maxWidth = GetRenderHost().LargeImageSize.Width;
			bool isTextStacked = _gallery.ItemsTextImageRelation == TextImageRelation.ImageAboveText || _gallery.ItemsTextImageRelation == TextImageRelation.TextAboveImage;
			foreach (GalleryItem galleryItem in _gallery.GalleryItems)
			{
				int currentItemWidth = 0;
				Size size = GetItemImageSize(galleryItem);
				if (!size.IsEmpty)
				{
					currentItemWidth = size.Width + 4;
				}
				if (galleryItem.Text.Length > 0 && galleryItem.GetEffectiveStyle() != CommandStyle.Image)
				{
					int textWidth = (int)textRenderer.MeasureStyledText(graphics, galleryItem.Text, galleryItem.ForegroundStyle, font, 0, 0).Width + 3;
					if (isTextStacked)
					{
						if (textWidth > currentItemWidth)
						{
							currentItemWidth = textWidth;
						}
					}
					else
					{
						currentItemWidth += textWidth;
					}
				}
				currentItemWidth += 4;
				if (currentItemWidth > maxWidth)
				{
					maxWidth = currentItemWidth;
				}
			}
			if (maxWidth == 0)
			{
				return GetRenderHost().LargeImageSize.Width;
			}
			return maxWidth;
		}

		protected virtual int GetItemHeight(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (_gallery.ItemsHeight > 0)
			{
				return _gallery.ItemsHeight;
			}
			if (_gallery.GalleryItems.Count == 0)
			{
				return GetRenderHost().LargeImageSize.Height;
			}
			bool isTextStacked = _gallery.ItemsTextImageRelation == TextImageRelation.ImageAboveText || _gallery.ItemsTextImageRelation == TextImageRelation.TextAboveImage;
			int maxHeight = GetRenderHost().SmallItemHeight;
			foreach (GalleryItem galleryItem in _gallery.GalleryItems)
			{
				int currentItemHeight = 0;
				Size size = GetItemImageSize(galleryItem);
				if (!size.IsEmpty)
				{
					currentItemHeight = size.Height + 4;
				}
				if (galleryItem.Text.Length > 0 && galleryItem.GetEffectiveStyle() != CommandStyle.Image)
				{
					int textHeight = font.Height + 4;
					if (isTextStacked)
					{
						currentItemHeight += textHeight;
					}
					else if (textHeight > currentItemHeight)
					{
						currentItemHeight = textHeight;
					}
				}
				currentItemHeight += 4;
				if (currentItemHeight > maxHeight)
				{
					maxHeight = currentItemHeight;
				}
			}
			return maxHeight;
		}

		public bool CanShrink(bool allowShrink)
		{
			if (IsCollapsedToButton())
			{
				return false;
			}
			if (_columns > _gallery.GetEffectiveMinColumns())
			{
				return true;
			}
			if (allowShrink)
			{
				return true;
			}
			return false;
		}

		public void LayoutScrollButtons()
		{
			if (_scrollUpButton == null)
			{
				_scrollUpButton = new GalleryScrollButtonRenderer(_commandBaseRendererRibbon, _gallery, GalleryScrollButtonRenderer.ScrollButtonType.ScrollUp);
			}
			if (_scrollDownButton == null)
			{
				_scrollDownButton = new GalleryScrollButtonRenderer(_commandBaseRendererRibbon, _gallery, GalleryScrollButtonRenderer.ScrollButtonType.ScrollDown);
			}
			if (_expandButton == null)
			{
				_expandButton = new GalleryScrollButtonRenderer(_commandBaseRendererRibbon, _gallery, GalleryScrollButtonRenderer.ScrollButtonType.Expand);
			}
			int buttonHeight = _galleryBounds.Height / 3;
			int extraPixels = _galleryBounds.Height % 3;
			int buttonTop = _galleryBounds.Top;
			int x = _galleryBounds.Right - 15;
			Rectangle rectangle = new Rectangle(x, buttonTop, 15, buttonHeight);
			if (extraPixels > 0)
			{
				rectangle.Height++;
				buttonTop++;
				extraPixels--;
			}
			_scrollUpButton._bounds = rectangle;
			buttonTop += buttonHeight;
			rectangle = new Rectangle(x, buttonTop, 15, buttonHeight);
			if (extraPixels > 0)
			{
				rectangle.Height++;
				buttonTop++;
				extraPixels--;
			}
			_scrollDownButton._bounds = rectangle;
			buttonTop += buttonHeight;
			rectangle = new Rectangle(x, buttonTop, 15, buttonHeight);
			_expandButton._bounds = rectangle;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_itemRenderers = new List<CommandBaseRenderer>();
			foreach (GalleryItem galleryItem in _gallery.GalleryItems)
			{
				if (galleryItem.ItemType == GalleryItemType.Item)
				{
					_itemRenderers.Add(new GalleryItemRenderer(galleryItem, GetRenderHost(), this, displayDescription: false));
				}
			}
			_itemWidth = GetItemWidth(graphics, textRenderer, font);
			_itemHeight = GetItemHeight(graphics, textRenderer, font);
			if (_itemRenderers.Count == 0)
			{
				if (_commandBaseRendererRibbon != null)
				{
					_fullItemWidth = _commandBaseRendererRibbon.GetContentAreaBounds().Height;
				}
				else
				{
					_fullItemWidth = 48;
				}
				if (IsCollapsedToButton())
				{
					base.CalculateFullItemWidth(graphics, textRenderer, font);
				}
				base.SetLayoutWidth(_fullItemWidth);
			}
			else
			{
				if (IsCollapsedToButton())
				{
					base.CalculateFullItemWidth(graphics, textRenderer, font);
				}
				else
				{
					_fullItemWidth = _itemWidth * _columns + 15 + 4;
				}
				base.SetLayoutWidth(_fullItemWidth);
			}
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (width == 0)
			{
				_bounds = Rectangle.Empty;
				LayoutScrollButtons();
				return;
			}
			bool scrollToSelection = false;
			if (_scrollRow == -1)
			{
				scrollToSelection = true;
				_scrollRow = 0;
			}
			if (!IsCollapsedToButton())
			{
				_columns = _gallery.MaxGalleryColumns;
				if (_columns <= 0 && _itemWidth > 0)
				{
					_columns = width / _itemWidth;
				}
				_bounds = new Rectangle(left, top, width, itemHeight);
				_galleryBounds = new Rectangle(left, top + 3, width, itemHeight - 6);
				_itemsViewport = new Rectangle(left + 2, top + 5, width - 4 - 15, _bounds.Height - 10);
				int currentX = _itemsViewport.X;
				int y = _itemsViewport.Y;
				y -= _scrollRow * _itemHeight;
				int rightLimit = left + width - 15 - 2;
				LayoutScrollButtons();
				int rowCount = 1;
				foreach (GalleryItemRenderer item in _itemRenderers)
				{
					if (currentX + _itemWidth > rightLimit)
					{
						y += _itemHeight;
						currentX = _itemsViewport.X;
						rowCount++;
					}
					item.ArrangeItem(graphics, textRenderer, currentX, y, _itemWidth, _itemHeight);
					item._ownerBottom = _ownerBottom;
					item._rowIndex = rowCount - 1;
					currentX += _itemWidth;
				}
				int visibleRows = _itemsViewport.Height / _itemHeight;
				if (visibleRows < 1)
				{
					visibleRows = 1;
				}
				_maxScrollRow = rowCount - visibleRows;
				if (scrollToSelection && _gallery.SelectedItem != null)
				{
					ScrollItemIntoView(_gallery.SelectedItem);
				}
				_scrollUpButton._isEnabled = _scrollRow != 0;
				_scrollDownButton._isEnabled = _scrollRow < _maxScrollRow;
			}
			else
			{
				base.ArrangeItem(graphics, textRenderer, left, top, width, itemHeight);
			}
		}

		private void RecalculateLayout()
		{
			using Graphics graphics = _commandBaseRendererRibbon.CreateGraphics();
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!GetRenderHost().UseCompatibleTextRendering);
			ArrangeItem(graphics, textRenderer, _bounds.Left, _bounds.Top, _bounds.Width, _bounds.Height);
		}

		internal void ScrollItemIntoView(GalleryItem galleryItem)
		{
			if (_itemRenderers == null)
			{
				return;
			}
			GalleryItemRenderer galleryItemRenderer = null;
			foreach (GalleryItemRenderer item in _itemRenderers)
			{
				if (item.GetCommand() == galleryItem)
				{
					galleryItemRenderer = item;
					break;
				}
			}
			if (galleryItemRenderer != null)
			{
				int visibleRows = _itemsViewport.Height / _itemHeight;
				SetScrollRow(galleryItemRenderer._rowIndex - (visibleRows - 1));
			}
		}

		internal CommandBaseRenderer GetItemRendererAt(int x, int y)
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

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (IsCollapsedToButton())
			{
				base.Paint(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
				return;
			}
			Color color = colorScheme._galleryBackground;
			if (Hot)
			{
				color = colorScheme._galleryHotBackground;
			}
			Rectangle rect = _galleryBounds;
			rect.Width--;
			ThemedRibbonPainter.GalleryFrame(graphics, colorScheme.Theme, rect, Hot);
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
			Region clip = null;
			GraphicsState savedState = null;
			savedState = graphics.Save();
			graphics.SetClip(_itemsViewport);
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				Rectangle rectangle = Rectangle.Intersect(_itemsViewport, item._bounds);
				if (rectangle.Width > 0 && rectangle.Height > 0)
				{
					item.Paint(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
				}
			}
			if (savedState != null)
			{
				graphics.Restore(savedState);
			}
			else
			{
				graphics.Clip = clip;
				clip.Dispose();
			}
			_scrollUpButton.DrawButton(graphics, colorScheme);
			_scrollDownButton.DrawButton(graphics, colorScheme);
			_expandButton.DrawButton(graphics, colorScheme);
		}

		public override void OnMouseMove(MouseEventArgs eventArgs)
		{
			if (IsCollapsedToButton())
			{
				base.OnMouseMove(eventArgs);
				return;
			}
			GalleryScrollButtonRenderer scrollButton = GetScrollButtonAt(eventArgs.X, eventArgs.Y);
			SetHotScrollButton(scrollButton);
			if (scrollButton != null)
			{
				SetHoveredItem(null);
				return;
			}
			CommandBaseRenderer commandBaseRenderer = GetItemRendererAt(eventArgs.X, eventArgs.Y);
			SetHoveredItem(commandBaseRenderer);
			commandBaseRenderer?.OnMouseMove(eventArgs);
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			if (IsCollapsedToButton())
			{
				base.OnMouseUp(eventArgs);
				return;
			}
			SetPressedScrollButton(null);
			GetItemRendererAt(eventArgs.X, eventArgs.Y)?.OnMouseUp(eventArgs);
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (IsCollapsedToButton())
			{
				base.OnMouseDown(eventArgs);
				return;
			}
			GalleryScrollButtonRenderer scrollButton = GetScrollButtonAt(eventArgs.X, eventArgs.Y);
			if (eventArgs.Button == MouseButtons.Left)
			{
				SetPressedScrollButton(scrollButton);
			}
			if (scrollButton == null)
			{
				CommandBaseRenderer commandBaseRenderer = GetItemRendererAt(eventArgs.X, eventArgs.Y);
				GetRenderHost().MouseDownItem = commandBaseRenderer;
				if (commandBaseRenderer != null && eventArgs.Button == MouseButtons.Left)
				{
					commandBaseRenderer.Pushed = true;
				}
				commandBaseRenderer?.OnMouseDown(eventArgs);
			}
			else if (eventArgs.Button == MouseButtons.Right && _commandBaseRendererRibbon != null)
			{
				((ICommandRenderHost)_commandBaseRendererRibbon).ShowQuickCustomizeMenu(GetCommand());
			}
		}

		internal GalleryScrollButtonRenderer GetScrollButtonAt(int x, int y)
		{
			if (_scrollUpButton != null && _scrollUpButton._bounds.Contains(x, y))
			{
				return _scrollUpButton;
			}
			if (_scrollDownButton != null && _scrollDownButton._bounds.Contains(x, y))
			{
				return _scrollDownButton;
			}
			if (_expandButton != null && _expandButton._bounds.Contains(x, y))
			{
				return _expandButton;
			}
			return null;
		}

		public void OnScrollButtonPressed(GalleryScrollButtonRenderer button)
		{
			if (button._isEnabled)
			{
				button.Pushed = true;
				switch (button.GetButtonType())
				{
				case GalleryScrollButtonRenderer.ScrollButtonType.Expand:
					DropDown(selectFirstItem: false);
					break;
				case GalleryScrollButtonRenderer.ScrollButtonType.ScrollDown:
					SetScrollRow(GetScrollRow() + 1);
					StartScrollTimer();
					break;
				case GalleryScrollButtonRenderer.ScrollButtonType.ScrollUp:
					SetScrollRow(GetScrollRow() - 1);
					StartScrollTimer();
					break;
				}
			}
		}

		private void StartScrollTimer()
		{
			if (_scrollTimer == null)
			{
				_scrollTimer = new Timer();
				_scrollTimer.Interval = ScrollRepeatDelayMilliseconds;
				_scrollTimer.Tick += OnScrollTimerTick;
				_scrollTimer.Start();
			}
		}

		public override void StopAnimation()
		{
			StopScrollTimer();
			base.StopAnimation();
		}

		private void StopScrollTimer()
		{
			if (_scrollTimer != null)
			{
				_scrollTimer.Stop();
				_scrollTimer.Tick -= OnScrollTimerTick;
				_scrollTimer.Dispose();
				_scrollTimer = null;
			}
		}

		private void OnScrollTimerTick(object sender, EventArgs eventArgs)
		{
			if (_scrollTimer == null)
			{
				return;
			}
			_scrollTimer.Interval = ScrollRepeatIntervalMilliseconds;
			if (_pressedScrollButton != null)
			{
				if (_pressedScrollButton.GetButtonType() == GalleryScrollButtonRenderer.ScrollButtonType.ScrollUp)
				{
					SetScrollRow(GetScrollRow() - 1);
				}
				else
				{
					SetScrollRow(GetScrollRow() + 1);
				}
			}
		}

		public override void DropDown(bool selectFirstItem)
		{
			if (!_gallery.CanOpenDropDown())
			{
				return;
			}
			if (IsCollapsedToButton())
			{
				base.DropDown(selectFirstItem);
				return;
			}
			ICommandRenderHost renderHost = GetRenderHost().ParentControl as ICommandRenderHost;
			if (renderHost == null && GetRenderHost() is MxRibbon)
			{
				renderHost = (MxRibbon)GetRenderHost();
			}
			CommandBaseEditorDialog commandBaseEditorDialog = new CommandBaseEditorDialog(renderHost, _gallery.Commands, null, _gallery);
			Point point = renderHost?.PointToScreen(_bounds.Location) ?? GetRenderHost().PointToScreen(_bounds.Location);
			commandBaseEditorDialog._minPopupSize = _bounds.Size;
			commandBaseEditorDialog.SetGalleryColumnCount(_itemsViewport.Width / _itemWidth);
			commandBaseEditorDialog._selectFirstItem = selectFirstItem;
			commandBaseEditorDialog.ShowAt(point);
			SetPressedScrollButton(null);
		}

		bool IKeyboardNavigable.SelectLastItem()
		{
			GetRenderHost().ActiveInputItem = this;
			return true;
		}

		bool IKeyboardNavigable.SelectFirstItem()
		{
			GetRenderHost().ActiveInputItem = this;
			return true;
		}

		bool IKeyboardNavigable.MoveNext(CommandBaseRenderer selectedItem)
		{
			return ((IKeyboardNavigable)this).MoveRight(selectedItem);
		}

		bool IKeyboardNavigable.MovePrevious(CommandBaseRenderer selectedItem)
		{
			return ((IKeyboardNavigable)this).MoveLeft(selectedItem);
		}

		bool IKeyboardNavigable.MoveRight(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectFirstItem();
			}
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveRight(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveLeft(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectLastItem();
			}
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveLeft(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveDown(CommandBaseRenderer selectedItem)
		{
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveDown(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveUp(CommandBaseRenderer selectedItem)
		{
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveUp(this);
			}
			return false;
		}

		CommandBase IKeyboardNavigable.GetCommandAt(int x, int y)
		{
			if (_bounds.Contains(x, y))
			{
				CommandBaseRenderer commandBaseRenderer = GetItemRendererAt(x, y);
				if (commandBaseRenderer != null)
				{
					return commandBaseRenderer.GetCommand();
				}
				return GetCommand();
			}
			return null;
		}

		CommandBaseRenderer IKeyboardNavigable.GetCommandLayoutItem(CommandBase command)
		{
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item.GetCommand() == command)
				{
					return item;
				}
			}
			return null;
		}

		void IKeyboardNavigable.AddKeyTips(Graphics graphics, TextLayoutRenderer textRenderer, Font font, IList<RibbonKeyTipEntry> keyTips, bool showToolTip)
		{
			IKeyTipProvider galleryRendererGallery = _gallery;
			if (galleryRendererGallery.HasKeyTip)
			{
				SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(font, GetRenderHost().UseCompatibleTextRendering, galleryRendererGallery.KeyTip, _commandBaseRendererRibbon._colorScheme);
				superTipSettingsWindowMessageHandler.SetCompactMode(true);
				superTipSettingsWindowMessageHandler.SetSticky(true);
				superTipSettingsWindowMessageHandler._isKeyTipEnabled = _gallery.IsEnabled;
				superTipSettingsWindowMessageHandler.ShowAt(GetRenderHost().ParentControl, GetRenderHost().ParentControl.PointToScreen(GetKeyTipLocation(graphics, textRenderer, font, galleryRendererGallery.KeyTip)));
				RibbonKeyTipEntry item = new RibbonKeyTipEntry(galleryRendererGallery.KeyTip, _gallery, this, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
				keyTips.Add(item);
			}
		}

		void IKeyboardNavigable.RemoveControls()
		{
			if (GetRenderHost() == null || GetRenderHost().ParentControl == null)
			{
				return;
			}
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item is ContainerControlCommandRenderer)
				{
					Control control = ((ContainerControlCommandRenderer)item).GetHostedControl();
					if (GetRenderHost().ParentControl.Controls.Contains(control))
					{
						GetRenderHost().ParentControl.Controls.Remove(control);
					}
				}
				else if (item is IKeyboardNavigable)
				{
					((IKeyboardNavigable)item).RemoveControls();
				}
			}
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			DropDown(selectFirstItem);
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (_expandButton != null)
			{
				int x = _expandButton._bounds.X + 8;
				int y = _expandButton._bounds.Bottom - 3;
				return new Point(x, y);
			}
			return base.GetKeyTipLocation(graphics, textRenderer, font, keyTip);
		}
	}
}
