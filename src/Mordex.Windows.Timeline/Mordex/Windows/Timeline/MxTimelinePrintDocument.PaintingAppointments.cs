using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimelinePrintDocument
	{
		internal void DrawItemText(Graphics graphics, TimelineItem timelineItem, int imageIndex, int integerArgument, TimelineFormatStyle timelineFormatStyle, StringFormat format)
		{
			Brush brush = timelineFormatStyle.GetForeColorBrush(_timeline.GetForegroundColor());
			Rectangle rectangle = TimelineUtilities.RoundToRectangle(timelineItem._rectangleF);
			Rectangle imageBounds = rectangle;
			Image image = _timeline.GetItemImage(timelineItem, imageIndex);
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
			if (!_timeline.ShowLabel || !timelineItem.IsLabelVisible() || rectangle.Width <= 0)
			{
				return;
			}
			string text = _timeline.GetItemLabelText(timelineItem);
			if (brush != null)
			{
				if (rectangle.Left < integerArgument)
				{
					rectangle.Width -= integerArgument - rectangle.Left;
					rectangle.X = integerArgument;
					text = "..." + text;
				}
				if (rectangle.Width > 0 && brush != null)
				{
					graphics.DrawString(text, timelineFormatStyle.ResolveFont(_timeline.Font), brush, rectangle, format);
				}
			}
		}

		private void DrawRowLabels(Graphics graphics, Rectangle rectangle, int lastRowIndex, Rectangle rectangleArgument, StringFormat stringFormat)
		{
			if (_rowLayouts == null || _rowLayouts.Count == 0)
			{
				return;
			}
			int rowTop = _timeline.GetHeaderHeight() + rectangle.Top + 1;
			int contentTop = rowTop;
			MxRowLayoutState mordexRowLayoutState = null;
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(PredefinedStyle.GroupRow);
			timelineFormatStyle.ApplyPrintStyle();
			TimelineFormatStyle rowHeaderStyle = new TimelineFormatStyle(PredefinedStyle.RowHeader);
			rowHeaderStyle.ApplyPrintStyle();
			TimelineGroup timelineGroup = null;
			if (!_grayShading)
			{
				timelineFormatStyle.ForeColor = Color.Black;
				rowHeaderStyle.ForeColor = Color.Black;
				timelineFormatStyle.BackColor = Color.White;
				timelineFormatStyle.BackColorGradient = Color.White;
				rowHeaderStyle.BackColor = Color.White;
				rowHeaderStyle.BackColorGradient = Color.White;
			}
			try
			{
				for (int i = 0; i < _rowLayouts.Count; i++)
				{
					MxRowLayoutState mordexRowLayoutStateArgument = (MxRowLayoutState)_rowLayouts[i];
					rowTop = mordexRowLayoutStateArgument.GetRowBounds().Y;
					if (i > lastRowIndex)
					{
						_firstRowIndex = i - 1;
						break;
					}
					if (i >= _firstRowIndex)
					{
						if (mordexRowLayoutStateArgument.GetRowType() == RowType.Group)
						{
							timelineGroup = (TimelineGroup)mordexRowLayoutStateArgument.GetNode().Group;
							DrawTimelineAreaEventArgs e = new DrawTimelineAreaEventArgs(graphics, Timeline, OwnerDrawnArea.GroupRow, mordexRowLayoutStateArgument.GetRowBounds(), mordexRowLayoutStateArgument, null, DateTime.MinValue);
							e.SetMordexRowLayoutState(mordexRowLayoutState);
							Timeline.RaiseDrawTimelineArea(e);
							if (!e.Handled)
							{
								DrawGroupRow(graphics, mordexRowLayoutStateArgument.GetRowBounds(), mordexRowLayoutStateArgument, stringFormat, mordexRowLayoutState, timelineGroup, timelineFormatStyle, rectangleArgument);
							}
						}
						else
						{
							if (mordexRowLayoutStateArgument.GetTimelineGroupRow() != null && !mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().IsEmpty)
							{
								DrawTimelineAreaEventArgs rowHeaderArgs = new DrawTimelineAreaEventArgs(graphics, Timeline, OwnerDrawnArea.RowHeader, mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle(), mordexRowLayoutStateArgument, null, DateTime.MinValue);
								rowHeaderArgs.SetMordexRowLayoutState(mordexRowLayoutState);
								Timeline.RaiseDrawTimelineArea(rowHeaderArgs);
								if (!rowHeaderArgs.Handled)
								{
									DrawRowHeader(graphics, contentTop, mordexRowLayoutStateArgument, mordexRowLayoutStateArgument.GetTimelineGroupRow(), mordexRowLayoutState, stringFormat, rowHeaderStyle, rectangleArgument);
								}
							}
							_timeline.DrawGroupIndentColumns(graphics, _pageContentBounds, mordexRowLayoutStateArgument, rowTop, mordexRowLayoutStateArgument.GetGroupIndex(), false, timelineFormatStyle, !_grayShading);
						}
						mordexRowLayoutState = mordexRowLayoutStateArgument;
					}
					else if (mordexRowLayoutStateArgument.GetTimelineGroupRow() != null && i + mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().Height / (_rowHeight - 1) > _firstRowIndex && !mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().IsEmpty)
					{
						DrawTimelineAreaEventArgs continuedHeaderArgs = new DrawTimelineAreaEventArgs(graphics, Timeline, OwnerDrawnArea.RowHeader, mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle(), mordexRowLayoutStateArgument, null, DateTime.MinValue);
						continuedHeaderArgs.SetMordexRowLayoutState(mordexRowLayoutState);
						Timeline.RaiseDrawTimelineArea(continuedHeaderArgs);
						if (!continuedHeaderArgs.Handled)
						{
							DrawRowHeader(graphics, contentTop, mordexRowLayoutStateArgument, mordexRowLayoutStateArgument.GetTimelineGroupRow(), mordexRowLayoutState, stringFormat, timelineFormatStyle, rectangleArgument);
						}
					}
					if (i == _firstRowIndex)
					{
						_firstRowIndex = i + 1;
					}
				}
				if (mordexRowLayoutState != null && Timeline.Groups.Count > 0)
				{
					int bottomLineY = mordexRowLayoutState.GetRowBounds().Bottom - 1;
					if (bottomLineY > rectangle.Bottom - 1)
					{
						bottomLineY = rectangle.Bottom - 1;
					}
					if (_grayShading)
					{
						Pen pen = new Pen(_timeline.FlatBorderColor);
						if (bottomLineY < rectangleArgument.Bottom)
						{
							graphics.DrawLine(pen, rectangle.Left, bottomLineY, rectangle.Width, bottomLineY);
						}
						pen.Dispose();
					}
					else if (bottomLineY < rectangleArgument.Bottom)
					{
						graphics.DrawLine(Pens.Black, rectangle.Left, bottomLineY, rectangle.Right, bottomLineY);
					}
				}
			}
			catch
			{
			}
			timelineFormatStyle.DisposeResources();
		}

		private void DrawGroupRow(Graphics graphics, Rectangle rect, MxRowLayoutState mordexRowLayoutState, StringFormat format, MxRowLayoutState mordexRowLayoutStateArgument, TimelineGroup timelineGroup, TimelineFormatStyle timelineFormatStyle, Rectangle rectangleArgument)
		{
			int index = timelineGroup.Index;
			TimelineGroupRow timelineGroupRow = _timeline.GetGroupRow(mordexRowLayoutState.GetNode());
			_ = _pageContentBounds.Left;
			_ = _timeline.GroupIndent;
			Rectangle rectangle = new Rectangle(rect.Left + (_timeline.GroupIndent - 14) / 2 + index, rect.Top + (_groupRowHeight - 14) / 2, 12, 12);
			mordexRowLayoutState.SetExpandBoxBounds(rectangle);
			Rectangle captionBounds = new Rectangle(rect.Left + _timeline.GroupIndent + 5, rect.Top, rect.Width - rect.Left + 1, _groupRowHeight);
			Rectangle imageBounds = captionBounds;
			Pen pen = null;
			pen = (_grayShading ? new Pen(ControlPaint.Dark(timelineFormatStyle.BackColor, 0f)) : new Pen(Color.Black));
			if (index > 0)
			{
				Rectangle indentBounds = new Rectangle(_pageContentBounds.Left, rect.Top, rect.Left, _groupRowHeight);
				Rectangle indentGradientBounds = indentBounds;
				indentGradientBounds.Width = _pageContentBounds.Width;
				_ = (TimelineGroup)mordexRowLayoutState.GetParentRow().GetNode().Group;
				Brush brush = timelineFormatStyle.GetBackBrush(indentGradientBounds);
				if (brush != null)
				{
					graphics.FillRectangle(brush, indentBounds);
				}
				for (int level = index - 1; level > 0; level--)
				{
					int lineX = _pageContentBounds.Left + level * _timeline.GroupIndent;
					graphics.DrawLine(pen, lineX, rect.Top, lineX, rect.Bottom - 1);
				}
			}
			Brush groupBrush = timelineFormatStyle.GetBackBrush(rect);
			if (groupBrush != null)
			{
				graphics.FillRectangle(groupBrush, rect);
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
			if (rect.Width >= 0)
			{
				Image image = _timeline.GetGroupRowImage(timelineGroupRow);
				if (image != null)
				{
					Size size = image.Size;
					graphics.DrawImage(image, imageBounds.X, imageBounds.Y + (imageBounds.Height - size.Height) / 2, size.Width, size.Height);
					captionBounds.X += size.Width + 2;
				}
				timelineFormatStyle.ResolveFont(_timeline.Font);
				Brush captionBrush = timelineFormatStyle.GetForeColorBrush(Timeline.GetForegroundColor());
				string text = timelineGroupRow.GroupCaption + timelineGroupRow.GetCaptionSuffix();
				text = text.Replace("\r\n", " ");
				if (text.Length > 0 && captionBrush != null)
				{
					graphics.DrawString(text, timelineFormatStyle.ResolveFont(_timeline.Font), captionBrush, captionBounds, format);
				}
				if (timelineFormatStyle.ForeColor.IsEmpty)
				{
					timelineFormatStyle.ForeColor = Timeline.GetForegroundColor();
				}
				if (PrintExpandBox)
				{
					DrawImage(graphics, rectangle, timelineFormatStyle, mordexRowLayoutState.IsExpanded(), false);
				}
			}
		}

		private void DrawRowHeader(Graphics graphics, int minTop, MxRowLayoutState mordexRowLayoutState, TimelineGroupRow timelineGroupRow, MxRowLayoutState mordexRowLayoutStateArgument, StringFormat format, TimelineFormatStyle timelineFormatStyle, Rectangle rectangleArgument)
		{
			TimelineGroup timelineGroup = timelineGroupRow.Group;
			Rectangle rectangle = mordexRowLayoutState.GetMordexRowLayoutStateRectangle();
			int top = rectangle.Top;
			if (top < minTop)
			{
				top = minTop;
			}
			Rectangle expandBoxBounds = Rectangle.Empty;
			int expandBoxWidth = 0;
			if (_timeline.AllowExpandRowHeader && PrintExpandBox)
			{
				expandBoxBounds = new Rectangle(rectangle.Left + (_timeline.GroupIndent - 14) / 2 + timelineGroup.Index, top + (_rowHeight - 14) / 2, 12, 12);
				expandBoxWidth = 16;
			}
			mordexRowLayoutState.SetExpandBoxBounds(expandBoxBounds);
			Rectangle captionBounds = new Rectangle(rectangle.Left + expandBoxWidth, top, rectangle.Width - expandBoxWidth, _rowHeight);
			Rectangle imageBounds = captionBounds;
			Pen pen = null;
			pen = (_grayShading ? new Pen(ControlPaint.Dark(timelineFormatStyle.BackColor, 0f)) : new Pen(Color.Black));
			if (_grayShading)
			{
				Brush brush = timelineFormatStyle.GetBackBrush(rectangle);
				Color backColor = timelineFormatStyle.BackColor;
				_timeline._scrollBarStyle.DrawBackground(graphics, rectangle, false, false, _timeline.HeaderAppearance, backColor, brush);
			}
			else
			{
				graphics.FillRectangle(Brushes.White, rectangle);
				TimelineUtilities.DrawFlatBorder(graphics, pen, rectangle);
			}
			if (mordexRowLayoutStateArgument != null)
			{
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			}
			if (rectangle.Width >= 0)
			{
				Image image = _timeline.GetGroupRowImage(timelineGroupRow);
				if (image != null)
				{
					Size size = image.Size;
					imageBounds.Inflate(-4, 0);
					graphics.DrawImage(image, imageBounds.X, imageBounds.Y + (imageBounds.Height - size.Height) / 2, size.Width, size.Height);
					captionBounds.X += size.Width + 8;
					captionBounds.Width -= size.Width + 8;
				}
				timelineFormatStyle.ResolveFont(_timeline.Font);
				Brush captionBrush = timelineFormatStyle.GetForeColorBrush(_timeline.GetForegroundColor());
				string groupCaption = timelineGroupRow.GroupCaption;
				groupCaption = groupCaption.Replace("\r\n", " ");
				if (groupCaption.Length > 0 && captionBrush != null)
				{
					graphics.DrawString(groupCaption, timelineFormatStyle.ResolveFont(_timeline.Font), captionBrush, captionBounds, format);
				}
				if (_timeline.AllowExpandRowHeader && PrintExpandBox)
				{
					DrawImage(graphics, expandBoxBounds, timelineFormatStyle, mordexRowLayoutState.GetParentRow().IsExpanded(), true);
				}
			}
		}
	}
}
