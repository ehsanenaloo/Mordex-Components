using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimeline
	{
		// For tests: false draws every label at once.
		internal static bool BatchLabels = true;

		// For the benchmark: the tier labels are drawn at once.
		internal static bool BatchTiers = true;

		private void DrawItems(Graphics graphics, TextLayoutRenderer textLayoutRenderer, Rectangle rectangle)
		{
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Far);
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_itemsFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
			TimelineFormatStyle barStyle = new TimelineFormatStyle(ItemsBarFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
			TimelineFormatStyle estimatedBarStyle = new TimelineFormatStyle(_itemsEstimatedBarFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
			int itemsLeft = GetItemsLeft(base.ClientRectangle.Left);
			ImageList imageList = GetImageList();
			// The labels are drawn together; an item that is painted over a label drawn before it flushes the batch first.
			TextBatch labelBatch = BatchLabels && textLayoutRenderer._useTextRenderer ? new TextBatch() : null;
			textLayoutRenderer.Batch = labelBatch;
			foreach (TimelineItem item in _visibleItems)
			{
				if (!item.IsVisibleInRow() || Rectangle.Intersect(rectangle, TimelineUtilities.RoundToRectangle(item._rectangleF)).IsEmpty)
				{
					continue;
				}
				TimelineFormatStyle itemStyle = timelineFormatStyle;
				TimelineFormatStyle itemBarStyle = barStyle;
				TimelineFormatStyle itemEstimatedBarStyle = estimatedBarStyle;
				int imageIndex = -1;
				TimelineFormatStyle mergedItemStyle = itemStyle;
				TimelineFormatStyle mergedBarStyle = itemBarStyle;
				TimelineFormatStyle mergedEstimatedBarStyle = itemEstimatedBarStyle;
				foreach (TimelineFormatCondition condition in _formatConditions)
				{
					if (!condition.Enabled || !condition.EvaluateItem(item))
					{
						continue;
					}
					int conditionImageIndex = TimelineUtilities.GetImageIndex(imageList, condition.ImageIndex, condition.ImageKey);
					if (condition.AllowMerge)
					{
						if (conditionImageIndex != -1)
						{
							imageIndex = conditionImageIndex;
						}
						mergedItemStyle = mergedItemStyle.MergeWith(condition.FormatStyle);
						mergedBarStyle = mergedBarStyle.MergeWith(condition.ItemsBarFormatStyle);
						mergedEstimatedBarStyle = mergedEstimatedBarStyle.MergeWith(condition.ItemsEstimatedBarFormatStyle);
					}
					else
					{
						imageIndex = conditionImageIndex;
						mergedItemStyle = itemStyle.MergeWith(condition.FormatStyle);
						mergedBarStyle = itemBarStyle.MergeWith(condition.ItemsBarFormatStyle);
						mergedEstimatedBarStyle = itemEstimatedBarStyle.MergeWith(condition.ItemsEstimatedBarFormatStyle);
					}
				}
				itemStyle = mergedItemStyle;
				itemBarStyle = mergedBarStyle;
				itemEstimatedBarStyle = mergedEstimatedBarStyle;
				if (!item.FormatStyle.HasOnlyDefaultValues())
				{
					itemStyle = itemStyle.MergeWith(item.FormatStyle);
				}
				if (!item.ItemBarFomatStyle.HasOnlyDefaultValues())
				{
					itemBarStyle = itemBarStyle.MergeWith(item.ItemBarFomatStyle);
				}
				if (!item.ItemEstimatedBarFormatStyle.HasOnlyDefaultValues())
				{
					itemEstimatedBarStyle = itemEstimatedBarStyle.MergeWith(item.ItemEstimatedBarFormatStyle);
				}
				if (imageIndex == -1)
				{
					imageIndex = TimelineUtilities.GetImageIndex(imageList, item.ImageIndex, item.ImageKey);
				}
				DrawTimelineAreaEventArgs e = new DrawTimelineAreaEventArgs(graphics, this, OwnerDrawnArea.Items, TimelineUtilities.RoundToRectangle(item._rectangleF), item, null, item.StartTime);
				if (labelBatch != null && labelBatch.Count > 0 && ((_ownerDrawnArea & OwnerDrawnArea.Items) != OwnerDrawnArea.None || labelBatch.IntersectsPending(Rectangle.Inflate(e.Bounds, 2, 2))))
				{
					labelBatch.Flush(graphics);
				}
				RaiseDrawTimelineArea(e);
				if (!e.Handled)
				{
					DrawBackground(graphics, item, base.ClientRectangle, itemStyle, itemBarStyle, itemEstimatedBarStyle, rectangle, true);
					DrawItemLabel(graphics, item, imageIndex, itemsLeft, itemStyle, textLayoutRenderer);
				}
				itemStyle.DisposeResources();
				itemBarStyle.DisposeResources();
				itemEstimatedBarStyle.DisposeResources();
			}
			labelBatch?.Flush(graphics);
			textLayoutRenderer.Batch = null;
			if (_draggedItems != null && _draggedItems.Count > 0)
			{
				foreach (TimelineItem draggedItem in _draggedItems)
				{
					if (Rectangle.Intersect(rectangle, TimelineUtilities.RoundToRectangle(draggedItem._rectangleF)).IsEmpty)
					{
						continue;
					}
					TimelineFormatStyle draggedItemStyle = timelineFormatStyle;
					TimelineFormatStyle draggedBarStyle = barStyle;
					TimelineFormatStyle draggedEstimatedBarStyle = estimatedBarStyle;
					TimelineFormatStyle mergedDraggedItemStyle = draggedItemStyle;
					TimelineFormatStyle mergedDraggedBarStyle = draggedBarStyle;
					TimelineFormatStyle mergedDraggedEstimatedBarStyle = draggedEstimatedBarStyle;
					int draggedImageIndex = -1;
					foreach (TimelineFormatCondition draggedCondition in _formatConditions)
					{
						if (!draggedCondition.Enabled || !draggedCondition.EvaluateItem(draggedItem))
						{
							continue;
						}
						int draggedConditionImageIndex = TimelineUtilities.GetImageIndex(imageList, draggedCondition.ImageIndex, draggedCondition.ImageKey);
						if (draggedCondition.AllowMerge)
						{
							if (draggedConditionImageIndex != -1)
							{
								draggedImageIndex = draggedConditionImageIndex;
							}
							mergedDraggedItemStyle = mergedDraggedItemStyle.MergeWith(draggedCondition.FormatStyle);
							mergedDraggedBarStyle = mergedDraggedBarStyle.MergeWith(draggedCondition.ItemsBarFormatStyle);
							mergedDraggedEstimatedBarStyle = mergedDraggedEstimatedBarStyle.MergeWith(draggedCondition.ItemsEstimatedBarFormatStyle);
						}
						else
						{
							draggedImageIndex = draggedConditionImageIndex;
							mergedDraggedItemStyle = draggedItemStyle.MergeWith(draggedCondition.FormatStyle);
							mergedDraggedBarStyle = draggedBarStyle.MergeWith(draggedCondition.ItemsBarFormatStyle);
							draggedEstimatedBarStyle = draggedEstimatedBarStyle.MergeWith(draggedCondition.ItemsEstimatedBarFormatStyle);
						}
					}
					draggedItemStyle = mergedDraggedItemStyle;
					draggedBarStyle = mergedDraggedBarStyle;
					draggedEstimatedBarStyle = mergedDraggedEstimatedBarStyle;
					if (!draggedItem.FormatStyle.HasOnlyDefaultValues())
					{
						draggedItemStyle = draggedItemStyle.MergeWith(draggedItem.FormatStyle);
					}
					if (!draggedItem.ItemBarFomatStyle.HasOnlyDefaultValues())
					{
						draggedBarStyle = draggedBarStyle.MergeWith(draggedItem.ItemBarFomatStyle);
					}
					if (!draggedItem.ItemEstimatedBarFormatStyle.HasOnlyDefaultValues())
					{
						draggedEstimatedBarStyle = draggedEstimatedBarStyle.MergeWith(draggedItem.ItemEstimatedBarFormatStyle);
					}
					if (draggedImageIndex == -1)
					{
						draggedImageIndex = TimelineUtilities.GetImageIndex(imageList, draggedItem.ImageIndex, draggedItem.ImageKey);
					}
					DrawTimelineAreaEventArgs draggedDrawArgs = new DrawTimelineAreaEventArgs(graphics, this, OwnerDrawnArea.Items, TimelineUtilities.RoundToRectangle(draggedItem._rectangleF), draggedItem, null, draggedItem.StartTime);
					RaiseDrawTimelineArea(draggedDrawArgs);
					if (!draggedDrawArgs.Handled)
					{
						DrawBackground(graphics, draggedItem, base.ClientRectangle, draggedItemStyle, draggedBarStyle, draggedEstimatedBarStyle, rectangle, true);
						DrawItemLabel(graphics, draggedItem, draggedImageIndex, itemsLeft, draggedItemStyle, textLayoutRenderer);
					}
					draggedItemStyle.DisposeResources();
					draggedBarStyle.DisposeResources();
					draggedEstimatedBarStyle.DisposeResources();
				}
			}
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
		}

		private void DrawItemBar(Graphics graphics, Rectangle rect, TimelineFormatStyle timelineFormatStyle, Rectangle rectangle, Rectangle rectangleArgument, bool clipToBounds)
		{
			Brush brush = timelineFormatStyle.GetBackBrush(rect);
			if (brush != null)
			{
				if (clipToBounds)
				{
					if (rect.X < rectangleArgument.X)
					{
						int leftClip = rectangleArgument.X - rect.X;
						rect.X = rectangleArgument.X;
						rect.Width -= leftClip;
					}
					if (rect.Right > rectangleArgument.Right)
					{
						rect.Width = rectangleArgument.Right - rect.X;
					}
				}
				graphics.FillRectangle(brush, rect);
			}
			if (timelineFormatStyle.BackgroundImage != null)
			{
				DrawBackgroundImage(graphics, rect, timelineFormatStyle.BackgroundImage, timelineFormatStyle.BackgroundImageDrawMode, rectangle);
			}
		}

		internal void DrawItemLabel(Graphics graphics, TimelineItem timelineItem, int imageIndex, int integerArgument, TimelineFormatStyle timelineFormatStyle, TextLayoutRenderer textLayoutRenderer)
		{
			if (timelineItem.Selected)
			{
				if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
				{
					if (_hideSelection == HideSelection.HighlightInactive)
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(_selectedInactiveFormatStyle);
					}
				}
				else
				{
					timelineFormatStyle = timelineFormatStyle.MergeWith(_selectedFormatStyle);
				}
			}
			Brush brush = timelineFormatStyle.GetForeColorBrush(GetForegroundColor());
			Rectangle rectangle = TimelineUtilities.RoundToRectangle(timelineItem._rectangleF);
			Rectangle imageBounds = rectangle;
			Image image = GetItemImage(timelineItem, imageIndex);
			if (image != null)
			{
				Size size = image.Size;
				if (imageBounds.Left < integerArgument)
				{
					imageBounds.Width -= integerArgument - imageBounds.Left;
					imageBounds.X = integerArgument;
				}
				if (imageBounds.Width > size.Width)
				{
					graphics.DrawImage(image, imageBounds.X, imageBounds.Bottom - size.Height - 1, size.Width, size.Height);
					integerArgument += size.Width + 4;
					rectangle.X += size.Width + 4;
					rectangle.Width -= size.Width + 4;
				}
			}
			if (!_showLabel || !timelineItem.IsLabelVisible() || rectangle.Width <= 0)
			{
				return;
			}
			string text = GetItemLabelText(timelineItem);
			if (brush != null)
			{
				if (rectangle.Left < integerArgument)
				{
					rectangle.Width -= integerArgument - rectangle.Left;
					rectangle.X = integerArgument;
					text = "..." + text;
				}
				if (rectangle.Width > 0)
				{
					Font labelFont = timelineFormatStyle.ResolveFont(Font);
					// A font the item style owns is disposed right after this item, so such a label cannot wait in the batch.
					TextBatch pending = ReferenceEquals(labelFont, Font) ? null : textLayoutRenderer.Batch;
					if (pending != null)
					{
						pending.Flush(graphics);
						textLayoutRenderer.Batch = null;
					}
					textLayoutRenderer.DrawTextWithBrush(graphics, text, rectangle, labelFont, brush);
					if (pending != null)
					{
						textLayoutRenderer.Batch = pending;
					}
				}
			}
		}

		private void DrawGroupRowsAndHeaders(Graphics graphics, Rectangle rectangleArgument, TextLayoutRenderer textLayoutRenderer)
		{
			if (_rowLayouts == null || _rowLayouts.Count == 0)
			{
				return;
			}
			int rowTop = GetHeaderHeight() + base.ClientRectangle.Top + 1;
			int contentTop = rowTop;
			MxRowLayoutState mordexRowLayoutState = null;
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_groupRowsFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
			TimelineFormatStyle rowHeaderStyle = new TimelineFormatStyle(_rowHeadersFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
			bool isHeaderHandled = false;
			foreach (MxRowLayoutState item in _rowLayouts)
			{
				rowTop = item.GetRowBounds().Y;
				if (rowTop > base.ClientRectangle.Bottom)
				{
					break;
				}
				if (item.GetRowIndex() >= _firstVisibleRowLayout.GetRowIndex())
				{
					if (item.GetRowType() == RowType.Group)
					{
						Rectangle rectangle = item.GetRowBounds();
						DrawTimelineAreaEventArgs e = new DrawTimelineAreaEventArgs(graphics, this, OwnerDrawnArea.GroupRow, rectangle, item, null, DateTime.MinValue);
						e.SetMordexRowLayoutState(mordexRowLayoutState);
						RaiseDrawTimelineArea(e);
						if (!e.Handled)
						{
							TimelineGroupRow timelineGroupRow = GetGroupRow(item.GetNode());
							DrawGroupRow(graphics, rectangle, item, timelineGroupRow, textLayoutRenderer, mordexRowLayoutState, timelineFormatStyle, rectangleArgument, true);
						}
					}
					else
					{
						if (item.GetTimelineGroupRow() != null && !item.GetMordexRowLayoutStateRectangle().IsEmpty)
						{
							Rectangle rect = item.GetMordexRowLayoutStateRectangle();
							if (item.GetGroupIndex() > 0)
							{
								int indentWidth = item.GetGroupIndex() * _groupIndent;
								rect.Width += indentWidth;
								rect.X -= indentWidth;
							}
							DrawTimelineAreaEventArgs rowHeaderArgs = new DrawTimelineAreaEventArgs(graphics, this, OwnerDrawnArea.RowHeader, rect, item, null, DateTime.MinValue);
							rowHeaderArgs.SetMordexRowLayoutState(mordexRowLayoutState);
							RaiseDrawTimelineArea(rowHeaderArgs);
							isHeaderHandled = rowHeaderArgs.Handled;
							if (!isHeaderHandled)
							{
								DrawRowHeader(graphics, contentTop, item, item.GetTimelineGroupRow(), mordexRowLayoutState, textLayoutRenderer, rowHeaderStyle, rectangleArgument, true);
							}
						}
						if (!isHeaderHandled)
						{
							DrawGroupIndentColumns(graphics, base.ClientRectangle, item, rowTop, item.GetGroupIndex(), true, timelineFormatStyle, false);
						}
					}
					mordexRowLayoutState = item;
				}
				else if (item.GetTimelineGroupRow() != null && item.GetRowIndex() + item.GetMordexRowLayoutStateRectangle().Height / (RowHeight - 1) > _firstVisibleRowLayout.GetRowIndex() && item.GetTimelineGroupRow() != null && !item.GetMordexRowLayoutStateRectangle().IsEmpty)
				{
					DrawTimelineAreaEventArgs partialRowHeaderArgs = new DrawTimelineAreaEventArgs(graphics, this, OwnerDrawnArea.RowHeader, item.GetMordexRowLayoutStateRectangle(), item, null, DateTime.MinValue);
					partialRowHeaderArgs.SetMordexRowLayoutState(mordexRowLayoutState);
					RaiseDrawTimelineArea(partialRowHeaderArgs);
					if (!partialRowHeaderArgs.Handled)
					{
						DrawRowHeader(graphics, contentTop, item, item.GetTimelineGroupRow(), mordexRowLayoutState, textLayoutRenderer, timelineFormatStyle, rectangleArgument, true);
					}
				}
			}
			if (mordexRowLayoutState != null && Groups.Count > 0)
			{
				Pen pen = _themeRenderer.GetBorderPen();
				graphics.DrawLine(pen, base.ClientRectangle.Left, mordexRowLayoutState.GetRowBounds().Bottom - 1, base.ClientRectangle.Width, mordexRowLayoutState.GetRowBounds().Bottom - 1);
			}
			timelineFormatStyle.DisposeResources();
		}

		internal void DrawGroupIndentColumns(Graphics graphics, Rectangle rectangleArgument, MxRowLayoutState mordexRowLayoutState, int y1, int groupIndex, bool isThemed, TimelineFormatStyle timelineFormatStyleArgument, bool booleanArgument)
		{
			Pen pen = null;
			pen = (isThemed ? _themeRenderer.GetBorderPen() : (booleanArgument ? GdiCache.Pen(Color.Black) : (timelineFormatStyleArgument.BackColorGradient.IsEmpty ? GdiCache.Pen(ControlPaint.Dark(timelineFormatStyleArgument.BackColor, 0f)) : GdiCache.Pen(ControlPaint.Dark(timelineFormatStyleArgument.BackColorGradient, 0f)))));
			int rowBottom = y1 + RowHeight;
			int indentWidth = groupIndex * _groupIndent;
			if (_showLastGroupAsRowHeader)
			{
				indentWidth--;
			}
			if (groupIndex > 0)
			{
				Rectangle rectangle = new Rectangle(rectangleArgument.Left, y1, indentWidth, RowHeight);
				if (!isThemed)
				{
					Rectangle fullWidthBounds = rectangle;
					fullWidthBounds.Width = rectangleArgument.Width;
					TimelineGroup timelineGroup = Groups[groupIndex - 1];
					TimelineFormatStyle timelineFormatStyle = timelineFormatStyleArgument;
					if (!timelineGroup.FormatStyle.HasOnlyDefaultValues())
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(timelineGroup.FormatStyle);
					}
					Brush brush = timelineFormatStyle.GetBackBrush(fullWidthBounds);
					if (brush != null)
					{
						graphics.FillRectangle(brush, rectangle);
					}
					if (timelineFormatStyle.BackgroundImage != null)
					{
						Region region = new Region(rectangle);
						Region clip = graphics.Clip;
						graphics.Clip = region;
						DrawBackgroundImage(graphics, fullWidthBounds, timelineFormatStyle.BackgroundImage, timelineFormatStyle.BackgroundImageDrawMode, rectangle);
						graphics.Clip = clip;
						region.Dispose();
						region = null;
					}
					timelineFormatStyle.DisposeResources();
				}
				else
				{
					graphics.FillRectangle(GdiCache.Brush(_themeRenderer._tierGradientEndColor), rectangle);
				}
				for (int levelIndex = groupIndex - 1; levelIndex > 0; levelIndex--)
				{
					int lineX = rectangleArgument.Left + levelIndex * _groupIndent;
					graphics.DrawLine(pen, lineX, y1, lineX, rowBottom - 1);
				}
			}
			if (groupIndex > 0)
			{
				graphics.DrawLine(pen, rectangleArgument.Left + indentWidth, y1, rectangleArgument.Left + indentWidth, rowBottom - 1);
			}
		}

		internal void DrawRowHeader(Graphics graphics, int visibleTop, MxRowLayoutState mordexRowLayoutState, TimelineGroupRow timelineGroupRow, MxRowLayoutState mordexRowLayoutStateArgument, TextLayoutRenderer textLayoutRenderer, TimelineFormatStyle timelineFormatStyleArgument, Rectangle rectangleArgument, bool booleanArgument)
		{
			if (timelineGroupRow == null || mordexRowLayoutState == null)
			{
				return;
			}
			TimelineGroup timelineGroup = timelineGroupRow.Group;
			if (timelineGroup == null)
			{
				return;
			}
			Rectangle rectangle = mordexRowLayoutState.GetMordexRowLayoutStateRectangle();
			int top = rectangle.Top;
			if (top < visibleTop)
			{
				top = visibleTop;
			}
			Rectangle expandBoxBounds = Rectangle.Empty;
			int expandBoxWidth = 0;
			if (AllowExpandRowHeader)
			{
				expandBoxBounds = new Rectangle(rectangle.Left + (_groupIndent - 14) / 2 + timelineGroup.Index, top + (RowHeight - 14) / 2, 12, 12);
				expandBoxWidth = 16;
			}
			mordexRowLayoutState.SetExpandBoxBounds(expandBoxBounds);
			mordexRowLayoutState.GetTimelineGroupRow()._bounds = expandBoxBounds;
			TimelineFormatStyle timelineFormatStyle = timelineFormatStyleArgument;
			if (!timelineGroup.FormatStyle.HasOnlyDefaultValues())
			{
				timelineFormatStyle = timelineFormatStyle.MergeWith(timelineGroup.FormatStyle);
			}
			if (!timelineGroupRow.FormatStyle.HasOnlyDefaultValues())
			{
				timelineFormatStyle = timelineFormatStyle.MergeWith(timelineGroupRow.FormatStyle);
			}
			if (mordexRowLayoutState.IsGroupSelected())
			{
				if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
				{
					if (_hideSelection == HideSelection.HighlightInactive)
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(_selectedInactiveFormatStyle);
					}
				}
				else
				{
					timelineFormatStyle = timelineFormatStyle.MergeWith(_selectedFormatStyle);
				}
			}
			Rectangle captionBounds = new Rectangle(rectangle.Left + expandBoxWidth, top, rectangle.Width - expandBoxWidth, RowHeight);
			Rectangle imageBounds = captionBounds;
			Pen pen = null;
			pen = _themeRenderer.GetBorderPen();
			Brush themedHeaderBrush = timelineFormatStyle.GetBackBrush(rectangle);
			if (themedHeaderBrush != null)
			{
				graphics.FillRectangle(themedHeaderBrush, rectangle);
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			if (mordexRowLayoutStateArgument != null)
			{
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			}
			if (rectangle.Width < 0)
			{
				timelineFormatStyle.DisposeResources();
				return;
			}
			if (booleanArgument)
			{
				Image image = GetGroupRowImage(timelineGroupRow);
				if (image != null)
				{
					Size size = image.Size;
					imageBounds.X += 6;
					graphics.DrawImage(image, imageBounds.X, imageBounds.Y + (imageBounds.Height - size.Height) / 2, size.Width, size.Height);
					captionBounds.X += size.Width + 6;
					captionBounds.Width -= size.Width + 6;
				}
				timelineFormatStyle.ResolveFont(Font);
				string groupCaption = timelineGroupRow.GroupCaption;
				groupCaption = groupCaption.Replace("\r\n", " ");
				if (groupCaption.Length > 0)
				{
					textLayoutRenderer.DrawText(graphics, groupCaption, captionBounds, timelineFormatStyle.ResolveFont(Font), timelineFormatStyle.ResolveForeColor(GetForegroundColor()));
				}
			}
			if (AllowExpandRowHeader)
			{
				DrawImage(graphics, expandBoxBounds, _headerAppearance, timelineFormatStyle, mordexRowLayoutState.GetParentRow().IsExpanded(), true, true, mordexRowLayoutState.IsHot());
			}
		}

		private void DrawGroupRow(Graphics graphics, Rectangle rect, MxRowLayoutState mordexRowLayoutState, TimelineGroupRow timelineGroupRow, TextLayoutRenderer textLayoutRenderer, MxRowLayoutState mordexRowLayoutStateArgument, TimelineFormatStyle timelineFormatStyleArgument, Rectangle rectangleArgument, bool booleanArgument)
		{
			TimelineGroup timelineGroup = timelineGroupRow.Group;
			if (timelineGroup == null)
			{
				return;
			}
			int index = timelineGroup.Index;
			Rectangle rectangle = new Rectangle(rect.Left + (_groupIndent - 14) / 2 + index, rect.Top + (GetGroupRowHeight() - 14) / 2, 12, 12);
			mordexRowLayoutState.SetExpandBoxBounds(rectangle);
			TimelineFormatStyle timelineFormatStyle = timelineFormatStyleArgument;
			if (!timelineGroup.FormatStyle.HasOnlyDefaultValues())
			{
				timelineFormatStyle = timelineFormatStyle.MergeWith(timelineGroup.FormatStyle);
			}
			if (!timelineGroupRow.FormatStyle.HasOnlyDefaultValues())
			{
				timelineFormatStyle = timelineFormatStyle.MergeWith(timelineGroupRow.FormatStyle);
			}
			TimelineFormatStyle unselectedStyle = timelineFormatStyle;
			if (mordexRowLayoutState.IsGroupSelected())
			{
				if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
				{
					if (_hideSelection == HideSelection.HighlightInactive)
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(_selectedInactiveFormatStyle);
					}
				}
				else
				{
					timelineFormatStyle = timelineFormatStyle.MergeWith(_selectedFormatStyle);
				}
			}
			Rectangle captionBounds = new Rectangle(rect.Left + _groupIndent + 5, rect.Top, rect.Width - rect.Left + 1, GetGroupRowHeight());
			Rectangle imageBounds = captionBounds;
			if (_themeRenderer == null)
			{
				_themeRenderer = new TimelineThemeRenderer();
			}
			Pen pen = _themeRenderer.GetBorderPen();
			if (index > 0)
			{
				Rectangle indentBounds = new Rectangle(base.ClientRectangle.Left, rect.Top, rect.Left, GetGroupRowHeight());
				Rectangle themedFullWidthBounds = indentBounds;
				themedFullWidthBounds.Width = base.ClientRectangle.Width;
				if (themedFullWidthBounds.Width > 0 && themedFullWidthBounds.Height > 0)
				{
					graphics.FillRectangle(_themeRenderer.GetTierGradientBrush(), indentBounds);
				}
				for (int levelIndex = index - 1; levelIndex > 0; levelIndex--)
				{
					int lineX = base.ClientRectangle.Left + levelIndex * _groupIndent;
					graphics.DrawLine(pen, lineX, rect.Top, lineX, rect.Bottom - 1);
				}
			}
			Brush rowBrush = timelineFormatStyle.GetBackBrush(rect);
			if (rowBrush != null)
			{
				graphics.FillRectangle(rowBrush, rect);
			}
			if (timelineFormatStyle.BackgroundImage != null)
			{
				DrawBackgroundImage(graphics, rect, timelineFormatStyle.BackgroundImage, timelineFormatStyle.BackgroundImageDrawMode, rectangleArgument);
			}
			if (mordexRowLayoutStateArgument != null)
			{
				graphics.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
			}
			graphics.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
			if (index > 0)
			{
				graphics.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Bottom - 1);
			}
			if (rect.Width < 0)
			{
				timelineFormatStyle.DisposeResources();
				return;
			}
			if (booleanArgument)
			{
				Image image = GetGroupRowImage(timelineGroupRow);
				if (image != null)
				{
					Size size = image.Size;
					graphics.DrawImage(image, imageBounds.X, imageBounds.Y + (imageBounds.Height - size.Height) / 2, size.Width, size.Height);
					captionBounds.X += size.Width + 2;
				}
				timelineFormatStyle.ResolveFont(Font);
				string text = timelineGroupRow.GroupCaption + timelineGroupRow.GetCaptionSuffix();
				text = text.Replace("\r\n", " ");
				if (text.Length > 0)
				{
					textLayoutRenderer.DrawText(graphics, text, captionBounds, timelineFormatStyle.ResolveFont(Font), timelineFormatStyle.ResolveForeColor(GetForegroundColor()));
				}
				if (unselectedStyle.ForeColor.IsEmpty)
				{
					unselectedStyle.ForeColor = GetForegroundColor();
				}
			}
			DrawImage(graphics, rectangle, _headerAppearance, unselectedStyle, mordexRowLayoutState.IsExpanded(), false, true, mordexRowLayoutState.IsHot());
			timelineFormatStyle.DisposeResources();
			unselectedStyle.DisposeResources();
		}
	}
}
