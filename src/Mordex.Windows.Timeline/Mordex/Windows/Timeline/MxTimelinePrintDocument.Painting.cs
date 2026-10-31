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
		protected override void OnPrintPage(PrintPageEventArgs e)
		{
			if (_currentPageIndex == 0 || _isLayoutPending)
			{
				CalculateLayout(e.Graphics);
				_firstRowIndex = 0;
				_isLayoutPending = false;
				if (base.PrinterSettings.PrintRange == PrintRange.SomePages)
				{
					for (int i = 1; i < base.PrinterSettings.FromPage; i++)
					{
						int skippedRowCount = 0;
						MeasurePageRows(ref skippedRowCount);
						if (_firstRowIndex + skippedRowCount > _rowLayouts.Count)
						{
							skippedRowCount = _rowLayouts.Count - _firstRowIndex;
						}
						_firstRowIndex += skippedRowCount;
						if (_firstRowIndex >= _rowLayouts.Count)
						{
							_firstRowIndex = 0;
						}
					}
				}
			}
			int rowCount = 0;
			int height = MeasurePageRows(ref rowCount);
			if (_firstRowIndex + rowCount > _rowLayouts.Count)
			{
				rowCount = _rowLayouts.Count - _firstRowIndex;
			}
			LayoutPageRows(rowCount);
			if (IsPageInPrintRange(_currentPageIndex))
			{
				base.OnPrintPage(e);
			}
			Region clip = e.Graphics.Clip;
			Rectangle rectangle = _pageContentBounds;
			rectangle.Height = height;
			if (IsPageInPrintRange(_currentPageIndex))
			{
				Region region = new Region(_pageContentBounds);
				e.Graphics.Clip = region;
				StringFormat stringFormat = new StringFormat();
				stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
				stringFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
				stringFormat.LineAlignment = StringAlignment.Center;
				stringFormat.Trimming = StringTrimming.EllipsisCharacter;
				if (_timeline._themeRenderer == null)
				{
					_timeline._themeRenderer = new TimelineThemeRenderer();
				}
				if (_grayShading)
				{
					DrawTimelineAreaEventArgs backgroundArgs = new DrawTimelineAreaEventArgs(e.Graphics, Timeline, OwnerDrawnArea.ControlBackground, _pageContentBounds, null, null, DateTime.MinValue);
					_timeline.RaiseDrawTimelineArea(backgroundArgs);
					if (!backgroundArgs.Handled)
					{
						_timeline.DrawControlBackground(e.Graphics, rectangle, _timeline.BackgroundImage, rectangle);
					}
					if (_timeline.BottomTier.Interval != TimelineInterval.Year && _timeline.BottomTier.Interval != TimelineInterval.Month && _timeline.BottomTier.Interval != TimelineInterval.Week)
					{
						_timeline.DrawWorkingTimeBackground(e.Graphics, _pageContentBounds, _pageStartDate, _intervalWidth, (int)_timescaleOriginX);
					}
				}
				DrawGridLines(e.Graphics, rectangle);
				DrawRowLabels(e.Graphics, _pageContentBounds, _firstRowIndex + rowCount, rectangle, stringFormat);
				DrawTimescale(e.Graphics, _pageContentBounds);
				region.Dispose();
				region = null;
				int timelineAreaLeft = _timeline.GetItemsLeft(_pageContentBounds.Left);
				region = new Region(new Rectangle(timelineAreaLeft, _timeline.GetHeaderHeight() + rectangle.Top + 1, _pageContentBounds.Right - timelineAreaLeft, rectangle.Height - _timeline.GetHeaderHeight()));
				e.Graphics.Clip = region;
				RenderContent(e.Graphics, stringFormat, _pageContentBounds);
				region.Dispose();
				region = null;
				e.Graphics.Clip = clip;
				if (!_grayShading)
				{
					TimelineUtilities.DrawFlatBorder(e.Graphics, Pens.Black, rectangle);
				}
				else
				{
					_timeline.RenderContent(e.Graphics, rectangle, null, BorderStyle.Flat);
				}
				DrawText(e.Graphics, e.MarginBounds, stringFormat);
				stringFormat.Dispose();
				stringFormat = null;
			}
			if (_firstRowIndex < _rowLayouts.Count)
			{
				e.HasMorePages = true;
			}
			else
			{
				_firstRowIndex = 0;
				if (_pageEndDate < EndDate)
				{
					SetPageStartDate(_pageEndDate);
					UpdatePageEndDate(_pageEndDate, _intervalsPerPage);
					if (_pageEndDate > EndDate)
					{
						_pageEndDate = EndDate;
					}
					PreparePageTimeRange(e.Graphics);
					e.HasMorePages = true;
				}
				else
				{
					_timeline.InvalidateRows();
					_timeline.InvalidateLayout();
				}
			}
			if (base.PrinterSettings.PrintRange == PrintRange.SomePages && _currentPageIndex == base.PrinterSettings.ToPage)
			{
				e.HasMorePages = false;
			}
			_currentPageIndex++;
		}

		private void RenderContent(Graphics graphics, StringFormat stringFormat, Rectangle rectangle)
		{
			stringFormat.LineAlignment = StringAlignment.Far;
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_timeline.ItemsFormatStyle, _timeline.GetEffectiveAlpha(), _timeline.GetBackColorAlphaMode(), _timeline.GetForeColorAlphaMode());
			TimelineFormatStyle barBaseStyle = new TimelineFormatStyle(_timeline.ItemsBarFormatStyle, _timeline.GetEffectiveAlpha(), _timeline.GetBackColorAlphaMode(), _timeline.GetForeColorAlphaMode());
			TimelineFormatStyle estimatedBarBaseStyle = new TimelineFormatStyle(_timeline.ItemsEstimatedBarFormatStyle, _timeline.GetEffectiveAlpha(), _timeline.GetBackColorAlphaMode(), _timeline.GetForeColorAlphaMode());
			int timelineAreaLeft = _timeline.GetItemsLeft(_pageContentBounds.Left);
			TimelineFormatStyle plainStyle = new TimelineFormatStyle();
			plainStyle.ForeColor = Color.Black;
			plainStyle.BackColor = Color.White;
			_timeline._isPrinting = true;
			foreach (TimelineItem item in _pageItems)
			{
				if (!item.IsVisibleInRow() || Rectangle.Intersect(rectangle, TimelineUtilities.RoundToRectangle(item._rectangleF)).IsEmpty)
				{
					continue;
				}
				TimelineFormatStyle itemStyle = timelineFormatStyle;
				TimelineFormatStyle itemBarStyle = barBaseStyle;
				TimelineFormatStyle itemEstimatedBarStyle = estimatedBarBaseStyle;
				TimelineFormatStyle conditionItemStyle = itemStyle;
				TimelineFormatStyle conditionBarStyle = itemBarStyle;
				TimelineFormatStyle conditionEstimatedBarStyle = itemEstimatedBarStyle;
				int imageIndex = -1;
				ImageList imageList = _timeline.GetImageList();
				foreach (TimelineFormatCondition formatCondition in Timeline.FormatConditions)
				{
					if (!formatCondition.Enabled || !formatCondition.EvaluateItem(item))
					{
						continue;
					}
					int conditionImageIndex = TimelineUtilities.GetImageIndex(imageList, formatCondition.ImageIndex, formatCondition.ImageKey);
					if (formatCondition.AllowMerge)
					{
						if (conditionImageIndex != -1)
						{
							imageIndex = conditionImageIndex;
						}
						conditionItemStyle = conditionItemStyle.MergeWith(formatCondition.FormatStyle);
						conditionBarStyle = conditionBarStyle.MergeWith(formatCondition.ItemsBarFormatStyle);
						conditionEstimatedBarStyle = conditionEstimatedBarStyle.MergeWith(formatCondition.ItemsEstimatedBarFormatStyle);
					}
					else
					{
						imageIndex = conditionImageIndex;
						conditionItemStyle = itemStyle.MergeWith(formatCondition.FormatStyle);
						conditionBarStyle = itemBarStyle.MergeWith(formatCondition.ItemsBarFormatStyle);
						conditionEstimatedBarStyle = itemEstimatedBarStyle.MergeWith(formatCondition.ItemsEstimatedBarFormatStyle);
					}
				}
				itemStyle = conditionItemStyle;
				itemBarStyle = conditionBarStyle;
				itemEstimatedBarStyle = conditionEstimatedBarStyle;
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
				if (!_grayShading)
				{
					itemStyle = plainStyle;
				}
				if (imageIndex == -1)
				{
					imageIndex = TimelineUtilities.GetImageIndex(imageList, item.ImageIndex, item.ImageKey);
				}
				DrawTimelineAreaEventArgs e = new DrawTimelineAreaEventArgs(graphics, Timeline, OwnerDrawnArea.Items, TimelineUtilities.RoundToRectangle(item._rectangleF), item, null, item.StartTime);
				_timeline.RaiseDrawTimelineArea(e);
				if (!e.Handled)
				{
					_timeline.DrawBackground(graphics, item, rectangle, itemStyle, itemBarStyle, itemEstimatedBarStyle, rectangle, false);
					DrawItemText(graphics, item, imageIndex, timelineAreaLeft, itemStyle, stringFormat);
				}
				if (!_grayShading)
				{
					TimelineUtilities.DrawFlatBorder(graphics, Pens.Black, TimelineUtilities.RoundToRectangle(item._rectangleF));
				}
				itemStyle.DisposeResources();
				itemBarStyle.DisposeResources();
				itemEstimatedBarStyle.DisposeResources();
				plainStyle.DisposeResources();
			}
			_timeline._isPrinting = false;
			stringFormat.LineAlignment = StringAlignment.Center;
		}

		private void DrawText(Graphics graphics, Rectangle rectangle, StringFormat format)
		{
			Size empty = Size.Empty;
			Rectangle textBounds = Rectangle.Empty;
			Font font = PageHeaderFormatStyle.ResolveFont(PageHeaderFont);
			Font footerFont = PageFooterFormatStyle.ResolveFont(PageFooterFont);
			Brush brush = PageHeaderFormatStyle.GetForeColorBrush(Color.Black);
			Brush footerBrush = PageFooterFormatStyle.GetForeColorBrush(Color.Black);
			if (PageHeaderLeft.Length > 0)
			{
				string text = ExpandPlaceholders(_pageHeaderLeft);
				empty = graphics.MeasureString(text, font, 0, format).ToSize();
				textBounds = new Rectangle(rectangle.Left, rectangle.Top - empty.Height, empty.Width + 1, empty.Height + 1);
				graphics.DrawString(text, font, brush, textBounds, format);
			}
			if (PageHeaderCenter.Length > 0)
			{
				string headerCenterText = ExpandPlaceholders(_pageHeaderCenter);
				empty = graphics.MeasureString(headerCenterText, font, 0, format).ToSize();
				textBounds = new Rectangle(rectangle.Left + (rectangle.Width - empty.Width) / 2, rectangle.Top - empty.Height, empty.Width + 1, empty.Height + 1);
				graphics.DrawString(headerCenterText, font, brush, textBounds, format);
			}
			if (PageFooterLeft.Length > 0)
			{
				string footerLeftText = ExpandPlaceholders(_pageFooterLeft);
				empty = graphics.MeasureString(footerLeftText, footerFont, 0, format).ToSize();
				textBounds = new Rectangle(rectangle.Left, rectangle.Top + rectangle.Height, empty.Width + 1, empty.Height + 1);
				graphics.DrawString(footerLeftText, footerFont, footerBrush, textBounds, format);
			}
			if (PageFooterCenter.Length > 0)
			{
				string footerCenterText = ExpandPlaceholders(_pageFooterCenter);
				empty = graphics.MeasureString(footerCenterText, footerFont, 0, format).ToSize();
				textBounds = new Rectangle(rectangle.Left + (rectangle.Width - empty.Width) / 2, rectangle.Top + rectangle.Height, empty.Width + 1, empty.Height + 1);
				graphics.DrawString(footerCenterText, footerFont, footerBrush, textBounds, format);
			}
			format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			if (PageHeaderRight.Length > 0)
			{
				string headerRightText = ExpandPlaceholders(_pageHeaderRight);
				empty = graphics.MeasureString(headerRightText, font, 0, format).ToSize();
				textBounds = new Rectangle(rectangle.Left + rectangle.Width - empty.Width, rectangle.Top - empty.Height, empty.Width + 1, empty.Height + 1);
				graphics.DrawString(headerRightText, font, brush, textBounds, format);
			}
			if (PageFooterRight.Length > 0)
			{
				string footerRightText = ExpandPlaceholders(_pageFooterRight);
				empty = graphics.MeasureString(footerRightText, footerFont, 0, format).ToSize();
				textBounds = new Rectangle(rectangle.Left + rectangle.Width - empty.Width, rectangle.Top + rectangle.Height, empty.Width + 1, empty.Height + 1);
				graphics.DrawString(footerRightText, footerFont, footerBrush, textBounds, format);
			}
			PageHeaderFormatStyle.DisposeResources();
			PageFooterFormatStyle.DisposeResources();
		}

		private void DrawGridLines(Graphics graphics, Rectangle rectangle)
		{
			if (Timeline.GridLines == GridLines.None)
			{
				return;
			}
			Pen pen = null;
			pen = ((!_grayShading) ? new Pen(Color.Black) : new Pen(_timeline.GridLineColor));
			if (Timeline.GridLineStyle == GridLineStyle.SmallDots)
			{
				pen.DashStyle = DashStyle.Dot;
			}
			if (Timeline.GridLines == GridLines.Vertical || Timeline.GridLines == GridLines.Both)
			{
				int i = (int)_timescaleOriginX;
				int y = rectangle.Top + Timeline.GetHeaderHeight() + 1;
				int bottom = rectangle.Bottom;
				for (; i <= rectangle.Right; i += _intervalWidth)
				{
					graphics.DrawLine(pen, i, y, i, bottom);
				}
			}
			if (Timeline.GridLines == GridLines.Horizontal || _timeline.GridLines == GridLines.Both)
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
			pen.Dispose();
			pen = null;
		}

		private void DrawImage(Graphics graphics, Rectangle rectArgument, TimelineFormatStyle timelineFormatStyle, bool isExpanded, bool booleanArgument)
		{
			if (_grayShading)
			{
				_timeline.DrawImage(graphics, rectArgument, _timeline.HeaderAppearance, timelineFormatStyle, isExpanded, booleanArgument, false, false);
				return;
			}
			Rectangle rect = new Rectangle(0, 0, 10, 10);
			rect.X = rectArgument.X + (rectArgument.Width - rect.Width) / 2;
			rect.Y = rectArgument.Y + (rectArgument.Height - rect.Height) / 2;
			rect.Width++;
			rect.Height++;
			TimelineUtilities.DrawFlatBorder(graphics, Pens.Black, rectArgument);
			rect.Inflate(-2, -2);
			switch (_timeline.ResolveExpandBoxStyle(isExpanded, false))
			{
			case ExpandBoxStyle.Arrows:
				_timeline.DrawExpandArrow(graphics, rectArgument, isExpanded, false);
				return;
			case ExpandBoxStyle.UseCustomGlyph:
				if (isExpanded)
				{
					if (_timeline.ExpandGlyph != null)
					{
						graphics.DrawImage(_timeline.ExpandGlyph, rectArgument);
					}
				}
				else if (_timeline.CollapseGlyph != null)
				{
					graphics.DrawImage(_timeline.CollapseGlyph, rectArgument);
				}
				return;
			}
			if (booleanArgument)
			{
				using (Image image = TimelineUtilities.MakeMagentaTransparent(isExpanded ? TimelineImageResources.GetExpandedBitmap() : TimelineImageResources.GetCollapsedBitmap()))
				{
					if (image != null)
					{
						rect.Offset(-1, -1);
						graphics.DrawImageUnscaled(image, rect);
					}
				}
			}
			else
			{
				graphics.DrawLine(Pens.Black, rect.Left, rect.Top + rect.Height / 2, rect.Right - 1, rect.Top + rect.Height / 2);
				if (!isExpanded)
				{
					graphics.DrawLine(Pens.Black, rect.Left + rect.Width / 2, rect.Top, rect.Left + rect.Width / 2, rect.Bottom - 1);
				}
			}
		}

		internal void DrawBackground(Graphics graphics, DateTime dateTime, Rectangle rect, float floatValue, int intervalWidth, Rectangle rectangle, bool boolean)
		{
			if (rect.Height == 0 || rect.Width == 0)
			{
				return;
			}
			Brush brush = null;
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(PredefinedStyle.LowerScale);
			timelineFormatStyle.ApplyPrintStyle();
			if (_grayShading)
			{
				brush = timelineFormatStyle.GetBackBrush(rect);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rect);
				}
			}
			else
			{
				graphics.FillRectangle(Brushes.White, rect);
				timelineFormatStyle.ForeColor = Color.Black;
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Far);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			Timeline.DrawLowerTierCells(graphics, textLayoutRenderer, dateTime, rect, floatValue, intervalWidth, false, timelineFormatStyle, Rectangle.Empty);
			timelineFormatStyle.DisposeResources();
			textLayoutRenderer.ResetFormat();
		}
	}
}
