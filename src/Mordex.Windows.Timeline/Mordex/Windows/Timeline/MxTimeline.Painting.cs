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
		protected override void OnPaint(PaintEventArgs e)
		{
			if (_themeRenderer == null)
			{
				_themeRenderer = new TimelineThemeRenderer();
			}
			if (_isTimescaleDirty)
			{
				if (!_isTimescaleValidationEnabled)
				{
					ResumeTimescaleValidation();
				}
				UpdateTimescaleMetrics();
			}
			if (_areRowsDirty)
			{
				RebuildRows();
			}
			if (_isLayoutDirty)
			{
				RecalculateLayout();
			}
			DrawTimelineAreaEventArgs backgroundArgs = new DrawTimelineAreaEventArgs(e.Graphics, this, OwnerDrawnArea.ControlBackground, base.ClientRectangle, null, null, DateTime.MinValue);
			RaiseDrawTimelineArea(backgroundArgs);
			if (!backgroundArgs.Handled)
			{
				DrawControlBackground(e.Graphics, base.ClientRectangle, BackgroundImage, e.ClipRectangle);
			}
			if (BottomTier.Interval != TimelineInterval.Year && BottomTier.Interval != TimelineInterval.Month && BottomTier.Interval != TimelineInterval.Week)
			{
				DrawWorkingTimeBackground(e.Graphics, base.ClientRectangle, FirstDate, _intervalWidth, (int)_floatValue);
			}
			if (HighlightCurrentTime)
			{
				DrawCurrentTimeMarker(e.Graphics);
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Height = GetHeaderHeight() + 1;
			if (GridLines != GridLines.None)
			{
				DrawGridLines(e.Graphics, base.ClientRectangle);
			}
			DrawGroupRowsAndHeaders(e.Graphics, e.ClipRectangle, textLayoutRenderer);
			Region clip = e.Graphics.Clip;
			Region region = new Region(new Rectangle(clientRectangle.Left, clientRectangle.Top, clientRectangle.Width, clientRectangle.Height + 1));
			e.Graphics.Clip = region;
			if (TimescaleTiers == TimescaleTiers.ThreeTiers)
			{
				bool showTopWeekNumbers = _showWeekNumbers;
				if (showTopWeekNumbers && (MiddleTier.Interval == TimelineInterval.Month || MiddleTier.Interval == TimelineInterval.Week || MiddleTier.Interval == TimelineInterval.Day))
				{
					showTopWeekNumbers = false;
				}
				DrawUpperTier(e.Graphics, TopTier, _firstDate, TopTier.GetBounds(), e.ClipRectangle, showTopWeekNumbers, true, true);
			}
			if (TimescaleTiers != TimescaleTiers.OneTier)
			{
				bool showMiddleWeekNumbers = _showWeekNumbers;
				if (showMiddleWeekNumbers && BottomTier.Interval == TimelineInterval.Week)
				{
					showMiddleWeekNumbers = false;
				}
				DrawUpperTier(e.Graphics, MiddleTier, _firstDate, MiddleTier.GetBounds(), e.ClipRectangle, showMiddleWeekNumbers, _timelineTimescaleTiers == TimescaleTiers.TwoTiers, false);
				if (TimescaleTiers == TimescaleTiers.ThreeTiers)
				{
					e.Graphics.DrawLine(GdiCache.Pen(_themeRenderer._buttonBorderColor), MiddleTier.GetBounds().Left, MiddleTier.GetBounds().Top, MiddleTier.GetBounds().Right, MiddleTier.GetBounds().Top);
				}
			}
			DrawLowerTier(e.Graphics, _firstDate, BottomTier.GetBounds(), _floatValue, _intervalWidth, e.ClipRectangle, BottomTier.HasFocus());
			region.Dispose();
			region = null;
			int contentLeft = clientRectangle.Left;
			if (Groups.Count > 0)
			{
				contentLeft = (Groups.Count - 1) * _groupIndent + clientRectangle.Left + 1;
				if (ShowLastGroupAsRowHeader && Groups.Count > 0)
				{
					contentLeft += RowHeaderWidth;
				}
			}
			region = new Region(new Rectangle(contentLeft, GetHeaderHeight() + clientRectangle.Top + 1, base.ClientRectangle.Width - contentLeft + clientRectangle.Left, base.ClientRectangle.Height - GetHeaderHeight()));
			e.Graphics.Clip = region;
			DrawItems(e.Graphics, textLayoutRenderer, e.ClipRectangle);
			region.Dispose();
			region = null;
			e.Graphics.Clip = clip;
			textLayoutRenderer.ResetFormat();
			RenderContent(e.Graphics, GetClientRectangle(), _themeRenderer, GetEffectiveBorderStyle());
			DisposeCachedResources();
			base.OnPaint(e);
			if (_hasPendingLastDateChange)
			{
				OnLastDateChanged(EventArgs.Empty);
				_hasPendingLastDateChange = false;
			}
		}

		internal void DrawBackground(Graphics graphics, TimelineItem timelineItem, Rectangle rectangleArgument, TimelineFormatStyle timelineFormatStyle, TimelineFormatStyle timelineFormatStyleArgument, TimelineFormatStyle estimatedBarStyle, Rectangle paintBounds, bool clipToBounds)
		{
			Rectangle rectangle = TimelineUtilities.RoundToRectangle(timelineItem._rectangleF);
			Rectangle clientBounds = rectangleArgument;
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
			Brush brush = timelineFormatStyle.GetBackBrush(rectangle);
			if (brush != null)
			{
				Rectangle rect = rectangle;
				if (clipToBounds)
				{
					if (rect.X < clientBounds.X)
					{
						int leftClip = clientBounds.X - rectangle.X;
						rect.X = clientBounds.X;
						rect.Width -= leftClip;
					}
					if (rect.Right > clientBounds.Right)
					{
						rect.Width = clientBounds.Right - rect.X;
					}
				}
				graphics.FillRectangle(brush, rect);
			}
			if (timelineFormatStyle.BackgroundImage != null)
			{
				DrawBackgroundImage(graphics, rectangle, timelineFormatStyle.BackgroundImage, timelineFormatStyle.BackgroundImageDrawMode, paintBounds);
			}
			if (timelineItem._durationWidth > 0.0 && _showItemsDurationBar)
			{
				DrawItemBar(graphics, timelineItem._durationBarBounds, timelineFormatStyleArgument, paintBounds, clientBounds, clipToBounds);
			}
			if (timelineItem._estimatedDurationWidth > 0.0 && _showItemsEstimatedDurationBar)
			{
				DrawItemBar(graphics, timelineItem._estimatedBarBounds, estimatedBarStyle, paintBounds, clientBounds, clipToBounds);
			}
			Pen pen = null;
			bool ownsPen = false;
			if (ItemsLineStyle == ItemsLineStyle.Solid)
			{
				pen = GdiCache.Pen(timelineFormatStyleArgument.BackColor);
			}
			else if (ItemsLineStyle == ItemsLineStyle.SmallDots)
			{
				pen = TimelineUtilities.CreateDottedPen(GetBackgroundColor(), timelineFormatStyleArgument.BackColor);
				ownsPen = true;
			}
			if (pen != null)
			{
				rectangle = Rectangle.Intersect(rectangle, rectangleArgument);
				TimelineUtilities.DrawFlatBorder(graphics, pen, rectangle);
				if (ownsPen)
				{
					pen.Dispose();
				}
				pen = null;
			}
		}

		private void DrawGridLines(Graphics graphics, Rectangle rectangle)
		{
			if (GridLines == GridLines.None)
			{
				return;
			}
			if (GridLineStyle != GridLineStyle.SmallDots)
			{
				DrawGridLineSegments(graphics, rectangle, GdiCache.Pen(GridLineColor));
				return;
			}
			Pen pen = new Pen(GridLineColor);
			if (GridLineStyle == GridLineStyle.SmallDots)
			{
				pen.DashStyle = DashStyle.Dot;
			}
			DrawGridLineSegments(graphics, rectangle, pen);
			pen.Dispose();
			pen = null;
		}

		private void DrawGridLineSegments(Graphics graphics, Rectangle rectangle, Pen pen)
		{
			if (GridLines == GridLines.Vertical || GridLines == GridLines.Both)
			{
				int i = (int)_floatValue;
				int y = rectangle.Top + GetHeaderHeight() + 1;
				int y2 = _rowsBottom;
				for (; i <= rectangle.Right; i += _intervalWidth)
				{
					graphics.DrawLine(pen, i, y, i, y2);
				}
			}
			if (GridLines == GridLines.Horizontal || GridLines == GridLines.Both)
			{
				int left = rectangle.Left;
				int right = rectangle.Right;
				foreach (MxRowLayoutState item in _rowLayouts)
				{
					if (!item.GetRowBounds().IsEmpty)
					{
						int top = item.GetRowBounds().Top;
						if (top > rectangle.Bottom)
						{
							break;
						}
						graphics.DrawLine(pen, left, top, right, top);
					}
				}
			}
		}

		internal void DrawExpandArrow(Graphics graphics, Rectangle rectangleArgument, bool isExpanded, bool booleanArgument)
		{
			Rectangle rectangle = new Rectangle(0, 0, 8, 8);
			rectangle.X = rectangleArgument.X + (rectangleArgument.Width - rectangle.Width) / 2;
			rectangle.Y = rectangleArgument.Y + (rectangleArgument.Height - rectangle.Height) / 2 - 1;
			rectangleArgument = rectangle;
			if (booleanArgument)
			{
				if (isExpanded)
				{
					TimelineRenderer.DrawHotExpandedImage(graphics, rectangle);
				}
				else
				{
					TimelineRenderer.DrawImage(graphics, rectangle);
				}
				return;
			}
			GraphicsPath graphicsPath = new GraphicsPath();
			if (!isExpanded)
			{
				graphicsPath.StartFigure();
				graphicsPath.AddLine(new Point(rectangleArgument.X, rectangleArgument.Y), new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Y + rectangleArgument.Height / 2));
				graphicsPath.AddLine(new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Y + rectangleArgument.Height / 2), new Point(rectangleArgument.X, rectangleArgument.Bottom));
				graphicsPath.AddLine(new Point(rectangleArgument.X, rectangleArgument.Bottom), new Point(rectangleArgument.X, rectangleArgument.Y));
				graphicsPath.CloseFigure();
				SmoothingMode smoothingMode = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.DrawPath(Pens.Gray, graphicsPath);
				graphics.SmoothingMode = smoothingMode;
				return;
			}
			rectangleArgument.Y--;
			graphicsPath.StartFigure();
			graphicsPath.AddLine(new Point(rectangleArgument.X, rectangleArgument.Bottom), new Point(rectangleArgument.X + rectangleArgument.Width / 2 + 1, rectangleArgument.Bottom));
			graphicsPath.AddLine(new Point(rectangleArgument.X + rectangleArgument.Width / 2 + 1, rectangleArgument.Bottom), new Point(rectangleArgument.X + rectangleArgument.Width / 2 + 1, rectangleArgument.Y + rectangleArgument.Height / 2 - 1));
			graphicsPath.AddLine(new Point(rectangleArgument.X + rectangleArgument.Width / 2 + 1, rectangleArgument.Y + rectangleArgument.Height / 2 - 1), new Point(rectangleArgument.X, rectangleArgument.Bottom));
			graphicsPath.CloseFigure();
			SmoothingMode expandedSmoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Point[] points = new Point[3]
			{
				new Point(rectangleArgument.X - 1, rectangleArgument.Bottom),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2 + 1, rectangleArgument.Bottom),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2 + 1, rectangleArgument.Y + rectangleArgument.Height / 2 - 2)
			};
			graphics.FillPolygon(GdiCache.Brush(Color.FromArgb(80, 80, 80)), points);
			points = new Point[6]
			{
				new Point(rectangleArgument.X + 1, rectangleArgument.Bottom),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Bottom),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2 + 1, rectangleArgument.Bottom - 1),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2 + 1, rectangleArgument.Y + rectangleArgument.Height / 2),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Y + rectangleArgument.Height / 2),
				new Point(rectangleArgument.X + 1, rectangleArgument.Bottom - 1)
			};
			graphics.DrawPolygon(Pens.Black, points);
			graphics.SmoothingMode = expandedSmoothingMode;
		}

		internal void DrawImage(Graphics graphics, Rectangle rectArgument, Appearance appearance, TimelineFormatStyle timelineFormatStyle, bool isExpanded, bool booleanArgument, bool isThemed, bool isHot)
		{
			Pen pen = null;
			Rectangle rect = (isThemed ? new Rectangle(0, 0, 16, 16) : ((appearance == Appearance.Flat) ? new Rectangle(0, 0, 10, 10) : new Rectangle(0, 0, 11, 11)));
			rect.X = rectArgument.X + (rectArgument.Width - rect.Width) / 2;
			rect.Y = rectArgument.Y + (rectArgument.Height - rect.Height) / 2;
			if (!isThemed)
			{
				Brush brush = null;
				if (timelineFormatStyle != null)
				{
					brush = timelineFormatStyle.GetBackBrush(rectArgument);
					pen = GdiCache.Pen(timelineFormatStyle.ResolveForeColor(Color.Black));
				}
				rect.Width++;
				rect.Height++;
				Color color = timelineFormatStyle.BackColor;
				if (!timelineFormatStyle.BackColorGradient.IsEmpty)
				{
					color = timelineFormatStyle.BackColorGradient;
				}
				_scrollBarStyle.DrawBackground(graphics, rectArgument, false, false, appearance, color, brush);
				if (appearance != Appearance.Flat)
				{
					rect.Inflate(-3, -3);
					rect.Width--;
					rect.Height--;
				}
				else
				{
					rect.Inflate(-2, -2);
				}
			}
			switch (ResolveExpandBoxStyle(isExpanded, isThemed))
			{
			case ExpandBoxStyle.Arrows:
				DrawExpandArrow(graphics, rectArgument, isExpanded, isHot);
				break;
			case ExpandBoxStyle.UseCustomGlyph:
				if (isExpanded)
				{
					if (ExpandGlyph != null)
					{
						graphics.DrawImage(ExpandGlyph, rectArgument);
					}
				}
				else if (CollapseGlyph != null)
				{
					graphics.DrawImage(CollapseGlyph, rectArgument);
				}
				break;
			default:
				if (isThemed)
				{
					Image image = ((!isExpanded) ? GetCollapsedBoxImage(_themeRenderer._headerGradientStartColor, _themeRenderer._lineColor, _themeRenderer._controlBorderColor, booleanArgument) : GetExpandedBoxImage(_themeRenderer._headerGradientStartColor, _themeRenderer._lineColor, _themeRenderer._controlBorderColor, booleanArgument));
					if (image != null)
					{
						graphics.DrawImageUnscaled(image, rectArgument);
					}
				}
				else if (booleanArgument)
				{
					using (Image glyphImage = TimelineUtilities.MakeMagentaTransparent(isExpanded ? TimelineImageResources.GetExpandedBitmap() : TimelineImageResources.GetCollapsedBitmap()))
					{
						if (glyphImage != null)
						{
							rect.Offset(-1, -1);
							graphics.DrawImageUnscaled(glyphImage, rect);
						}
					}
				}
				else
				{
					if (pen == null)
					{
						pen = GdiCache.Pen(Color.Black);
					}
					graphics.DrawLine(pen, rect.Left, rect.Top + rect.Height / 2, rect.Right - 1, rect.Top + rect.Height / 2);
					if (!isExpanded)
					{
						graphics.DrawLine(pen, rect.Left + rect.Width / 2, rect.Top, rect.Left + rect.Width / 2, rect.Bottom - 1);
					}
				}
				break;
			}
		}

		internal void DrawDefaultArea(DrawTimelineAreaEventArgs eventArgs, object sender)
		{
			switch (eventArgs.OwnerDrawnArea)
			{
			case OwnerDrawnArea.Items:
			{
				TimelineItem item = eventArgs.Item;
				if (item == null)
				{
					break;
				}
				TimelineFormatStyle itemsStyle = new TimelineFormatStyle(_itemsFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
				TimelineFormatStyle barStyle = new TimelineFormatStyle(_itemsBarFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
				TimelineFormatStyle estimatedBarStyle = new TimelineFormatStyle(_itemsEstimatedBarFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
				TimelineFormatStyle mergedItemStyle = itemsStyle;
				TimelineFormatStyle mergedBarStyle = barStyle;
				TimelineFormatStyle mergedEstimatedBarStyle = estimatedBarStyle;
				foreach (TimelineFormatCondition condition in _formatConditions)
				{
					if (condition.Enabled && condition.EvaluateItem(item))
					{
						if (condition.AllowMerge)
						{
							mergedItemStyle = mergedItemStyle.MergeWith(condition.FormatStyle);
							mergedBarStyle = mergedBarStyle.MergeWith(condition.ItemsBarFormatStyle);
							mergedEstimatedBarStyle = mergedEstimatedBarStyle.MergeWith(condition.ItemsEstimatedBarFormatStyle);
						}
						else
						{
							mergedItemStyle = itemsStyle.MergeWith(condition.FormatStyle);
							mergedBarStyle = barStyle.MergeWith(condition.ItemsBarFormatStyle);
							mergedEstimatedBarStyle = estimatedBarStyle.MergeWith(condition.ItemsEstimatedBarFormatStyle);
						}
					}
				}
				if (!item.FormatStyle.HasOnlyDefaultValues())
				{
					mergedItemStyle = mergedItemStyle.MergeWith(item.FormatStyle);
				}
				if (!item.ItemBarFomatStyle.HasOnlyDefaultValues())
				{
					mergedBarStyle = mergedBarStyle.MergeWith(item.ItemBarFomatStyle);
				}
				if (!item.ItemEstimatedBarFormatStyle.HasOnlyDefaultValues())
				{
					mergedEstimatedBarStyle = mergedEstimatedBarStyle.MergeWith(item.ItemEstimatedBarFormatStyle);
				}
				DrawBackground(eventArgs.Graphics, item, base.ClientRectangle, mergedItemStyle, mergedBarStyle, mergedEstimatedBarStyle, eventArgs.Bounds, !_isPrinting);
				mergedItemStyle.DisposeResources();
				mergedEstimatedBarStyle.DisposeResources();
				mergedBarStyle.DisposeResources();
				break;
			}
			case OwnerDrawnArea.ControlBackground:
				DrawControlBackground(eventArgs.Graphics, eventArgs.Bounds, BackgroundImage, eventArgs.Bounds);
				break;
			case OwnerDrawnArea.GroupRow:
				if (sender is MxRowLayoutState mordexRowLayoutState)
				{
					TimelineFormatStyle groupRowStyle = new TimelineFormatStyle(_groupRowsFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
					TimelineGroupRow timelineGroupRow = GetGroupRow(mordexRowLayoutState.GetNode());
					DrawGroupRow(eventArgs.Graphics, eventArgs.Bounds, mordexRowLayoutState, timelineGroupRow, null, eventArgs.GetMordexRowLayoutState(), groupRowStyle, eventArgs.Bounds, false);
				}
				break;
			case OwnerDrawnArea.RowHeader:
				if (sender is MxRowLayoutState mordexRowLayoutStateArgument)
				{
					TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_rowHeadersFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
					DrawRowHeader(eventArgs.Graphics, GetHeaderHeight() + base.ClientRectangle.Top + 1, mordexRowLayoutStateArgument, mordexRowLayoutStateArgument.GetTimelineGroupRow(), eventArgs.GetMordexRowLayoutState(), null, timelineFormatStyle, eventArgs.Bounds, false);
					DrawGroupIndentColumns(eventArgs.Graphics, base.ClientRectangle, mordexRowLayoutStateArgument, mordexRowLayoutStateArgument.GetRowBounds().Top, mordexRowLayoutStateArgument.GetGroupIndex(), true, timelineFormatStyle, false);
				}
				break;
			}
		}

		internal void DrawControlBackground(Graphics graphics, Rectangle rectangleArgument, Image image, Rectangle paintBounds)
		{
			Rectangle rectangle = Rectangle.Intersect(paintBounds, rectangleArgument);
			if (rectangle.Width != 0 && rectangle.Height != 0)
			{
				TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_backgroundFormatStyle);
				if (timelineFormatStyle.BackColor.IsEmpty)
				{
					timelineFormatStyle.BackColor = GetBackgroundColor();
				}
				Brush brush = timelineFormatStyle.GetBackBrush(rectangle);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
				}
				rectangle = Rectangle.Intersect(paintBounds, rectangleArgument);
				timelineFormatStyle.DisposeResources();
				if (BackgroundImage != null)
				{
					DrawBackgroundImage(graphics, rectangleArgument, BackgroundImage, _backgroundImageDrawMode, paintBounds);
				}
				if (WatermarkImage != null)
				{
					_watermarkImage.DrawImage(graphics, rectangleArgument);
				}
			}
		}

		internal void DrawBackgroundImage(Graphics graphics, Rectangle destRect, Image image, BackgroundImageDrawMode backgroundImageDrawMode, Rectangle rectangleArgument)
		{
			Rectangle rectangle = Rectangle.Intersect(rectangleArgument, destRect);
			if (rectangle.Width == 0 || rectangle.Height == 0)
			{
				return;
			}
			if (backgroundImageDrawMode == BackgroundImageDrawMode.Empty)
			{
				backgroundImageDrawMode = _backgroundImageDrawMode;
			}
			if (image != null)
			{
				switch (backgroundImageDrawMode)
				{
				case BackgroundImageDrawMode.Empty:
				case BackgroundImageDrawMode.Stretch:
					graphics.DrawImage(image, destRect, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
					break;
				case BackgroundImageDrawMode.Tile:
				{
					Brush brush = new TextureBrush(image, WrapMode.Tile);
					graphics.FillRectangle(brush, destRect);
					brush.Dispose();
					break;
				}
				case BackgroundImageDrawMode.Center:
				{
					Region region = new Region(destRect);
					Region clip = graphics.Clip;
					graphics.Clip = region;
					int x = destRect.X + (destRect.Width - image.Width) / 2;
					int y = destRect.Y + (destRect.Height - image.Height) / 2;
					graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
					graphics.Clip = clip;
					region.Dispose();
					break;
				}
				}
			}
		}
	}
}
