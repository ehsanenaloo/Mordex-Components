using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonGroupBoxRenderer
	{
		private void AddGalleryColumn(RibbonCommandColumn item)
		{
			if (_galleryColumns == null)
			{
				_galleryColumns = new List<RibbonCommandColumn>();
			}
			_galleryColumns.Add(item);
		}

		public void CalculateCollapsedLayout(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (_isCollapsed)
			{
				_captionGapWidth = (int)textRenderer.MeasureStyledText(graphics, "  ", GetRibbonGroup().ForegroundStyle, font, 0, 0).Width;
				if (_captionGapWidth < 12)
				{
					_captionGapWidth = 12;
				}
				int captionWidth = 0;
				if (!_isSingleWordCaption)
				{
					captionWidth = RibbonDrawingUtils.MeasureTwoLineTextWidth(graphics, textRenderer, GetCaptionText(), GetRibbonGroup().ForegroundStyle, font, 0, _captionGapWidth);
					if (RibbonDrawingUtils.MeasureLastWord(graphics, GetRibbonGroup().Text, new Rectangle(0, 0, captionWidth, font.Height * 2), GetRenderHost().Font).Right > (float)(captionWidth - 4))
					{
						captionWidth += _captionGapWidth;
					}
				}
				else
				{
					captionWidth = (int)textRenderer.MeasureStyledText(graphics, GetRibbonGroup().Text, GetRibbonGroup().ForegroundStyle, font, 0, 0).Width;
				}
				if (captionWidth > GetRenderHost().LargeImageSize.Width + 10)
				{
					_fullItemWidth = captionWidth;
				}
				else
				{
					_fullItemWidth = GetRenderHost().LargeImageSize.Width + 10;
				}
				_fullItemWidth += 2;
				textRenderer.SetWrapEnabled(false);
				return;
			}
			int itemsSpace = GetRibbonGroup().ItemsSpace;
			int maxColumnWidth = 0;
			int columnHeightLimit = GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows;
			int usedColumnHeight = 0;
			int totalColumnsWidth = 0;
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				item.CalculateFullItemWidth(graphics, textRenderer, font);
				int itemHeight = GetItemHeight(item, columnHeightLimit);
				if (usedColumnHeight + itemHeight > columnHeightLimit)
				{
					totalColumnsWidth += maxColumnWidth + itemsSpace;
					usedColumnHeight = itemHeight;
					maxColumnWidth = item._fullItemWidth;
				}
				else if (item._fullItemWidth > maxColumnWidth)
				{
					maxColumnWidth = item._fullItemWidth;
					usedColumnHeight += itemHeight + itemsSpace;
				}
				else
				{
					usedColumnHeight += itemHeight + itemsSpace;
				}
				item._ownerBottom = _bounds.Bottom;
			}
			if (_dialogLauncher != null)
			{
				_dialogLauncher._ownerBottom = _bounds.Bottom;
			}
			if (maxColumnWidth > 0)
			{
				totalColumnsWidth += maxColumnWidth;
			}
			if (_fullCaptionWidth > totalColumnsWidth)
			{
				_isCollapsed = true;
				CalculateCollapsedLayout(graphics, textRenderer, font);
				return;
			}
			Padding padding = ((_commandBaseRendererRibbon != null) ? _commandBaseRendererRibbon.GetGroupBoxPadding() : new Padding(4));
			if (!_isCollapsed)
			{
				_fullItemWidth = totalColumnsWidth + padding.Left + padding.Right;
			}
			else
			{
				_fullItemWidth = totalColumnsWidth + 4;
			}
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_childRenderers = new List<CommandBaseRenderer>();
			_isCollapsed = false;
			int itemsSpace = GetRibbonGroup().ItemsSpace;
			int columnWidth = 0;
			int columnHeightLimit = GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows;
			int usedColumnHeight = 0;
			int totalWidth = 0;
			int rowIndex = 0;
			_commandColumns = new List<RibbonCommandColumn>();
			_galleryColumns = null;
			RibbonCommandColumn currentColumn = null;
			bool startNewColumn = false;
			bool hasWideItem = false;
			bool canSetColumnWidth = true;
			foreach (CommandBase command in GetRibbonGroup().Commands)
			{
				if (!command.Visible)
				{
					continue;
				}
				CommandBaseRenderer commandBaseRenderer = CreateLayoutItem(command);
				commandBaseRenderer.CalculateFullItemWidth(graphics, textRenderer, font);
				int itemHeight = GetItemHeight(commandBaseRenderer, columnHeightLimit);
				if (usedColumnHeight + itemHeight > columnHeightLimit)
				{
					rowIndex = 0;
					totalWidth += columnWidth + itemsSpace;
					usedColumnHeight = itemHeight + itemsSpace;
					if (currentColumn != null)
					{
						currentColumn.SetColumnWidth(columnWidth);
						if (hasWideItem || commandBaseRenderer.CommandType == CommandType.ContainerControl || (commandBaseRenderer.CommandType == CommandType.ButtonGroup && itemHeight != _commandBaseRendererRibbon.GetSmallItemHeight()))
						{
							startNewColumn = true;
							canSetColumnWidth = false;
						}
						else
						{
							canSetColumnWidth = true;
						}
					}
					columnWidth = commandBaseRenderer._fullItemWidth;
				}
				else if (commandBaseRenderer._fullItemWidth > columnWidth)
				{
					columnWidth = commandBaseRenderer._fullItemWidth;
					usedColumnHeight += itemHeight + itemsSpace;
				}
				else
				{
					usedColumnHeight += itemHeight + itemsSpace;
				}
				CommandType commandType = commandBaseRenderer.CommandType;
				if (commandType == CommandType.ButtonGroup || commandType == CommandType.ContainerControl)
				{
					hasWideItem = true;
					commandType = CommandType.SmallButton;
				}
				if (currentColumn == null || commandType != currentColumn._commandsType || startNewColumn)
				{
					startNewColumn = false;
					if (canSetColumnWidth && currentColumn != null && currentColumn.GetColumnWidth() <= 0)
					{
						currentColumn.SetColumnWidth(columnWidth);
					}
					rowIndex = 0;
					currentColumn = new RibbonCommandColumn(_commandBaseRendererRibbon, commandType);
					currentColumn.SetColumnWidth(columnWidth);
					_commandColumns.Add(currentColumn);
					if (commandType == CommandType.Gallery)
					{
						AddGalleryColumn(currentColumn);
					}
				}
				commandBaseRenderer._rowInColumn = rowIndex;
				commandBaseRenderer._columnIndex = _commandColumns.Count - 1;
				currentColumn._items.Add(commandBaseRenderer);
				_childRenderers.Add(commandBaseRenderer);
				rowIndex++;
				if (rowIndex >= GetRenderHost().MaxItemRows)
				{
					rowIndex = 0;
				}
			}
			currentColumn?.SetColumnWidth(columnWidth);
			for (int i = 0; i < _commandColumns.Count; i++)
			{
				RibbonCommandColumn thisValue = _commandColumns[i];
				foreach (CommandBaseRenderer item in thisValue._items)
				{
					if (item.CommandType == CommandType.ContainerControl)
					{
						item.SetLayoutWidth(thisValue.GetColumnWidth());
					}
					if (item.CommandType == CommandType.ButtonGroup)
					{
						totalWidth = TrimButtonGroupWidth(item as ButtonGroupRenderer, i, totalWidth, columnHeightLimit);
					}
				}
			}
			if (IsDesignerMouseIn() && GetCommand().IsEditable())
			{
				CommandBaseRenderer addCommandRenderer = new RibbonGroupAddCommandRenderer(_commandBaseRendererRibbon, this);
				addCommandRenderer.CalculateFullItemWidth(graphics, textRenderer, font);
				int addCommandHeight = GetItemHeight(addCommandRenderer, columnHeightLimit);
				if (usedColumnHeight + addCommandHeight > columnHeightLimit)
				{
					rowIndex = 0;
					totalWidth += columnWidth + itemsSpace;
					usedColumnHeight = addCommandHeight + itemsSpace;
					columnWidth = addCommandRenderer._fullItemWidth;
				}
				else if (addCommandRenderer._fullItemWidth > columnWidth)
				{
					columnWidth = addCommandRenderer._fullItemWidth;
					usedColumnHeight += addCommandHeight + itemsSpace;
				}
				else
				{
					usedColumnHeight += addCommandHeight + itemsSpace;
				}
				CommandType addCommandType = addCommandRenderer.CommandType;
				if (currentColumn == null || addCommandType != currentColumn._commandsType)
				{
					rowIndex = 0;
					currentColumn = new RibbonCommandColumn(_commandBaseRendererRibbon, addCommandType);
					_commandColumns.Add(currentColumn);
				}
				addCommandRenderer._rowInColumn = rowIndex;
				addCommandRenderer._columnIndex = _commandColumns.Count - 1;
				currentColumn._items.Add(addCommandRenderer);
				_childRenderers.Add(addCommandRenderer);
				rowIndex++;
				if (rowIndex >= GetRenderHost().MaxItemRows)
				{
					rowIndex = 0;
				}
			}
			if (columnWidth > 0)
			{
				totalWidth += columnWidth;
			}
			Padding padding = ((_commandBaseRendererRibbon != null) ? _commandBaseRendererRibbon.GetGroupBoxPadding() : new Padding(4));
			if (!_isCollapsed)
			{
				_fullItemWidth = totalWidth + padding.Left + padding.Right;
			}
			else
			{
				_fullItemWidth = totalWidth + 4;
			}
			_contentWidth = _fullItemWidth;
			int captionTextWidth = (int)textRenderer.MeasureStyledText(graphics, GetCaptionText(), GetRibbonGroup().ForegroundStyle, font, 0, 0).Width + 1;
			if (_isCollapsed)
			{
				captionTextWidth += 2;
			}
			int captionExtraWidth = 0;
			_captionWidth = captionTextWidth + captionExtraWidth + 2;
			_fullCaptionWidth = _captionWidth;
			_minCaptionWidth = _commandBaseRendererRibbon.GetSmallItemHeight() + captionExtraWidth + 2;
			if (_captionWidth > _fullItemWidth)
			{
				_fullItemWidth = _captionWidth;
			}
		}

		private int TrimButtonGroupWidth(ButtonGroupRenderer buttonGroup, int columnIndex, int totalWidth, int columnHeightLimit)
		{
			RibbonCommandColumn thisValue = _commandColumns[columnIndex];
			if (buttonGroup != null)
			{
				if (columnIndex == _commandColumns.Count - 1)
				{
					buttonGroup.SetHasRightSeparator(false);
					buttonGroup._fullItemWidth -= 6;
					totalWidth -= 6;
				}
				else
				{
					RibbonCommandColumn nextColumn = _commandColumns[columnIndex + 1];
					int nextColumnHeight = 0;
					foreach (CommandBaseRenderer item in nextColumn.GetItems())
					{
						nextColumnHeight += GetItemHeight(item, columnHeightLimit);
					}
					int heightBeforeGroup = 0;
					foreach (CommandBaseRenderer columnItem in thisValue._items)
					{
						if (columnItem != buttonGroup)
						{
							heightBeforeGroup += GetItemHeight(columnItem, columnHeightLimit);
							continue;
						}
						break;
					}
					if (heightBeforeGroup >= nextColumnHeight)
					{
						buttonGroup.SetHasRightSeparator(false);
						buttonGroup._fullItemWidth -= 6;
					}
				}
			}
			return totalWidth;
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (_hasImageContent || _isCollapsed)
			{
				int iconTopOffset = 1;
				_collapsedIconBounds = new Rectangle(left, top + iconTopOffset, GetRenderHost().LargeImageSize.Width, GetRenderHost().LargeImageSize.Height);
				Size smallImageSize = GetRenderHost().SmallImageSize;
				int iconBottomGap = (int)((double)_collapsedIconBounds.Height * 0.2) + 1;
				_imageBounds = new Rectangle(_collapsedIconBounds.X + (_collapsedIconBounds.Width - smallImageSize.Width) / 2, _collapsedIconBounds.Y + (_collapsedIconBounds.Height - smallImageSize.Height - iconBottomGap) / 2, smallImageSize.Width, smallImageSize.Height);
			}
			else
			{
				_collapsedIconBounds = Rectangle.Empty;
				_imageBounds = Rectangle.Empty;
			}
			int captionWidth = 0;
			if (!_isCollapsed)
			{
				captionWidth = width;
				_captionBarBounds = new Rectangle(left + 1, top + itemHeight - _commandBaseRendererRibbon._groupLabelHeight - 1, width - 2, _commandBaseRendererRibbon._groupLabelHeight);
				if (GetRibbonGroup().DialogButtonVisible)
				{
					_dialogLauncher = new RibbonGroupDialogLauncherRenderer(GetRenderHost(), this);
					_dialogLauncher._bounds = new Rectangle(left + width - 17, _captionBarBounds.Top + (_captionBarBounds.Height - 16) / 2, 15, 14);
					captionWidth -= 18;
					_dialogLauncher._bounds.Offset(-1, 3);
				}
				else
				{
					_dialogLauncher = null;
				}
				_textBounds = new Rectangle(left, top + itemHeight - _commandBaseRendererRibbon._groupLabelHeight - 1, captionWidth, _commandBaseRendererRibbon._groupLabelHeight + 1);
			}
			else
			{
				captionWidth = (int)textRenderer.MeasureStyledText(graphics, GetCaptionText(), GetRibbonGroup().ForegroundStyle, GetRenderHost().Font, 0, 0).Width;
				_dialogLauncher = null;
				captionWidth = width;
				int captionTop = top + GetRenderHost().LargeImageSize.Height;
				captionTop += 4;
				_textBounds = new Rectangle(left, captionTop, captionWidth, GetRenderHost().Font.Height * 2);
				using (RibbonFontLease font = GetRibbonGroup().ForegroundStyle.LeaseFont(GetRenderHost().Font))
				{
					_lastWordBounds = RibbonDrawingUtils.GetLastWordBounds(graphics, textRenderer, GetRibbonGroup().Text, _textBounds, font.Font, _captionGapWidth, _isSingleWordCaption);
				}
				_lastWordBounds.Y -= 3;
				base.RemoveHostedControls();
				_captionBarBounds = Rectangle.Empty;
			}
			_bounds = new Rectangle(left, top, width, itemHeight);
			if (!_isCollapsed)
			{
				LayoutColumnItems(graphics, textRenderer, width);
				if (GetRibbonGroup().CenterItems)
				{
					ApplyColumnHeightLimit();
				}
			}
			CenterCollapsedIcons();
		}

		private void LayoutColumnItems(Graphics graphics, TextLayoutRenderer textRenderer, int width)
		{
			int columnLeft = _bounds.Left + _commandBaseRendererRibbon.GetGroupBoxPadding().Left;
			int itemTop = _bounds.Top + _commandBaseRendererRibbon.GetGroupBoxPadding().Top;
			int itemsSpace = GetRibbonGroup().ItemsSpace;
			int columnWidth = 0;
			int columnHeightLimit = GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows;
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				int itemHeight = GetItemHeight(item, GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows);
				if (itemTop + itemHeight > _bounds.Top + columnHeightLimit + 2)
				{
					itemTop = _bounds.Top + _commandBaseRendererRibbon.GetGroupBoxPadding().Top;
					columnLeft += columnWidth + itemsSpace;
					columnWidth = 0;
				}
				item._ownerBottom = _bounds.Bottom;
				item.ArrangeItem(graphics, textRenderer, columnLeft, itemTop, item.GetLayoutWidth(), itemHeight);
				if (itemHeight < GetRenderHost().SmallItemHeight)
				{
					itemHeight = GetRenderHost().SmallItemHeight;
				}
				itemTop += itemHeight + itemsSpace;
				if (item._bounds.Width > columnWidth)
				{
					columnWidth = item._bounds.Width;
				}
			}
			if (_dialogLauncher != null)
			{
				_dialogLauncher._ownerBottom = _bounds.Bottom;
			}
		}

		public bool CanReduce(bool allowReduction)
		{
			if (_commandBaseRendererRibbon != null && _commandBaseRendererRibbon.IsInDesignMode())
			{
				return false;
			}
			if (GetRibbonGroup() == null)
			{
				return false;
			}
			if (_captionWidth > _contentWidth && _captionWidth > _minCaptionWidth)
			{
				return true;
			}
			if (GetRibbonGroup().AllowAutoSizeItems && !_isCollapsed)
			{
				foreach (RibbonCommandColumn item in _commandColumns)
				{
					if (item.CanShrink(allowReduction))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void ShrinkOrCollapse(Graphics graphics, TextLayoutRenderer textLayoutRenderer, bool forceCollapse, Font font)
		{
			if (GetRibbonGroup() == null)
			{
				return;
			}
			if (GetRibbonGroup().AllowAutoSizeItems)
			{
				for (int columnIndex = _commandColumns.Count - 1; columnIndex >= 0; columnIndex--)
				{
					RibbonCommandColumn thisValue = _commandColumns[columnIndex];
					if (thisValue.CanShrink(true))
					{
						thisValue.Shrink();
						CalculateCollapsedLayout(graphics, textLayoutRenderer, font);
						break;
					}
				}
			}
			if (forceCollapse)
			{
				_isCollapsed = true;
				CalculateCollapsedLayout(graphics, textLayoutRenderer, font);
			}
		}

		private void CenterCollapsedIcons()
		{
			if (_isCollapsed)
			{
				int width = _bounds.Width;
				int centeredX = 0;
				if (!_collapsedIconBounds.IsEmpty)
				{
					centeredX = (width - _imageBounds.Width) / 2;
					centeredX += _bounds.X;
					_imageBounds.X = centeredX;
					centeredX = (width - _collapsedIconBounds.Width) / 2;
					centeredX += _bounds.X;
					_collapsedIconBounds.X = centeredX;
				}
			}
		}

		public void ScrollIntoView()
		{
			if (_commandBaseRendererRibbon != null)
			{
				RibbonTabContentRenderer field_ = _commandBaseRendererRibbon._tabContentRenderer;
				field_.ScrollGroupIntoView(this);
			}
		}

		public void ShowCollapsedPopup()
		{
			ScrollIntoView();
			SetPopupOpen(true);
			CollapsedGroupPopup popup = new CollapsedGroupPopup(_commandBaseRendererRibbon, GetRibbonGroup().Commands, this, GetRibbonGroup());
			popup.Disposed += OnControlDisposed;
			Rectangle rectangle = _bounds;
			rectangle.Y += 2;
			popup.ShowAt(GetRenderHost().PointToScreen(new Point(rectangle.X, rectangle.Top)));
		}
	}
}
