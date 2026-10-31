using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	/// <summary>Prints the timeline.</summary>
	/// <remarks>When the default theme is light, the printout uses the theme colors. When it is dark, the job prints with the Standard theme (system colors on white paper); the theme of the screen does not change. See <see cref="PrintLook"/>.</remarks>
	public partial class MxTimelinePrintDocument : PrintDocument
	{
		private Rectangle _initialPageBounds = Rectangle.Empty;

		private Rectangle _pageContentBounds;

		private int _totalPages;

		private TimelineCollection _rowLayoutCollection;

		private int _rowHeight;

		private int _groupRowHeight;

		private ArrayList _rowLayouts;

		private ArrayList _printItems;

		private ArrayList _pageItems;

		private double _pageStartOffset;

		private double _pageEndOffset;

		private int _intervalWidth = 84;

		private float _timescaleOriginX;

		private int _firstRowIndex;

		private int _currentPageIndex;

		private int _totalRowsHeight;

		private DateTime _pageStartDate;

		private DateTime _pageEndDate;

		private float _intervalsPerPage;

		private DateTime _startDate = DateTime.MinValue;

		private string _pageFooterCenter = "";

		private string _pageFooterLeft = "";

		private string _pageFooterRight = "";

		private string _pageHeaderCenter = "";

		private string _pageHeaderLeft = "";

		private string _pageHeaderRight = "";

		private TimelineFormatStyle _pageHeaderFormatStyle = new TimelineFormatStyle();

		private TimelineFormatStyle _pageFooterFormatStyle = new TimelineFormatStyle();

		private Font _pageHeaderFont;

		private Font _pageFooterFont;

		private DateTime _endDate = DateTime.MinValue;

		private bool _printCollapsedRows = true;

		private bool _grayShading = true;

		private bool _printExpandBox = true;

		private MxTimeline _timeline;

		private bool _isLayoutPending;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The format settings used to draw page headers.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public TimelineFormatStyle PageHeaderFormatStyle => _pageHeaderFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Format settings used to draw page footers.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public TimelineFormatStyle PageFooterFormatStyle => _pageFooterFormatStyle;

		[Localizable(true)]
		[Description("The Font object to apply to the text displayed on the header section.")]
		public Font PageHeaderFont
		{
			get
			{
				if (_pageHeaderFont == null)
				{
					return Control.DefaultFont;
				}
				return _pageHeaderFont;
			}
			set
			{
				_pageHeaderFont = value;
			}
		}

		[Localizable(true)]
		[Description("The Font object to apply to the text displayed on the footer section.")]
		public Font PageFooterFont
		{
			get
			{
				if (_pageFooterFont == null)
				{
					return Control.DefaultFont;
				}
				return _pageFooterFont;
			}
			set
			{
				_pageFooterFont = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeStartDate()
		{
			return _startDate != DateTime.MinValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetStartDate()
		{
			_startDate = DateTime.MinValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializePageHeaderFont()
		{
			return _pageHeaderFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetPageHeaderFont()
		{
			_pageHeaderFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializePageFooterFont()
		{
			return _pageFooterFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetPageFooterFont()
		{
			_pageFooterFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeEndDate()
		{
			return _endDate != DateTime.MinValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetEndTime()
		{
			_endDate = DateTime.MinValue;
		}

		private Rectangle GetPageContentBounds()
		{
			QueryPageSettingsEventArgs e = new QueryPageSettingsEventArgs(base.DefaultPageSettings);
			OnQueryPageSettings(e);
			Rectangle bounds = e.PageSettings.Bounds;
			Margins margins = e.PageSettings.Margins;
			Rectangle result = new Rectangle(bounds.Left + margins.Left, bounds.Top + margins.Top, bounds.Width, bounds.Height);
			result.Width -= margins.Left + margins.Right;
			result.Height -= margins.Top + margins.Bottom;
			return result;
		}

		private string ExpandPlaceholders(string textArgument)
		{
			string text = textArgument;
			text = ((_timeline.TimeFormat != TimeFormat.TwelveHours) ? text.Replace("[Time Printed]", DateTime.Now.ToString("%H:mm")) : text.Replace("[Time Printed]", DateTime.Now.ToShortTimeString()));
			text = text.Replace("[Page #]", (_currentPageIndex + 1).ToString());
			text = text.Replace("[Total Pages]", _totalPages.ToString());
			text = text.Replace("[Date Printed]", DateTime.Today.ToShortDateString());
			return text.Replace("[User Name]", SystemInformation.UserName);
		}

		private int ComputeHorizontalPageCount()
		{
			_intervalWidth = _timeline.IntervalWidth;
			long ticks = EndDate.Subtract(StartDate).Ticks;
			int timelineWidth = _pageContentBounds.Width;
			if (!_startDate.Equals(DateTime.MinValue))
			{
				timelineWidth = _pageContentBounds.Width;
				if (_timeline.Groups.Count > 0)
				{
					timelineWidth -= _timeline.GroupIndent * (_timeline.Groups.Count - 1);
					if (_timeline.ShowLastGroupAsRowHeader)
					{
						timelineWidth -= _timeline.RowHeaderWidth;
					}
				}
			}
			_intervalsPerPage = timelineWidth / _intervalWidth;
			UpdatePageEndDate(StartDate, _intervalsPerPage);
			int totalIntervals = 0;
			int pageCount = 0;
			if (_timeline.BottomTier.Interval == TimelineInterval.Year)
			{
				totalIntervals = EndDate.Year - StartDate.Year;
				pageCount = (int)((float)totalIntervals / _intervalsPerPage);
				if ((float)totalIntervals % _intervalsPerPage != 0f || pageCount == 0)
				{
					pageCount++;
				}
			}
			else if (_timeline.BottomTier.Interval == TimelineInterval.Month)
			{
				int yearDifference = EndDate.Year - StartDate.Year;
				totalIntervals = yearDifference * 12 + (EndDate.Month - StartDate.Month);
				pageCount = (int)((float)totalIntervals / _intervalsPerPage);
				if ((float)totalIntervals % _intervalsPerPage != 0f || pageCount == 0)
				{
					pageCount++;
				}
			}
			else
			{
				long pageTicks = _pageEndDate.Subtract(StartDate).Ticks;
				totalIntervals = (int)(ticks / ((long)_timeline._intervalSeconds * 10000000L));
				if (totalIntervals == 0 || ticks % ((long)_timeline._intervalSeconds * 10000000L) != 0)
				{
					totalIntervals++;
				}
				pageCount = (int)(ticks / pageTicks);
				if (ticks % pageTicks != 0 || pageCount == 0)
				{
					pageCount++;
				}
			}
			_intervalWidth = pageCount * timelineWidth / totalIntervals;
			if (_timeline.BottomTier.Interval == TimelineInterval.Year || _timeline.BottomTier.Interval == TimelineInterval.Month)
			{
				_intervalsPerPage = timelineWidth / _intervalWidth;
				while (_intervalsPerPage * (float)pageCount < (float)totalIntervals)
				{
					pageCount++;
					_intervalWidth = pageCount * timelineWidth / totalIntervals;
				}
			}
			else
			{
				_intervalsPerPage = (float)timelineWidth / (float)_intervalWidth;
			}
			UpdatePageEndDate(StartDate, _intervalsPerPage);
			if (_pageEndDate > EndDate)
			{
				_pageEndDate = EndDate;
			}
			return pageCount;
		}

		private void CalculateLayout(Graphics graphics)
		{
			try
			{
				_totalPages = 0;
				_pageContentBounds = GetPageContentBounds();
				_totalPages = 0;
				_initialPageBounds = _pageContentBounds;
				_rowHeight = _timeline.RowHeight;
				_groupRowHeight = _timeline.GetGroupRowHeight();
				int horizontalPageCount = ComputeHorizontalPageCount();
				SetPageStartDate(StartDate);
				BuildRowLayouts(graphics);
				PreparePageTimeRange(graphics);
				_totalPages = 1;
				_ = _pageContentBounds.Height;
				_timeline.GetHeaderHeight();
				int verticalPageCount = _totalRowsHeight / (_pageContentBounds.Height - _timeline.GetHeaderHeight() + 1);
				if (_totalRowsHeight % (_pageContentBounds.Height - _timeline.GetHeaderHeight() + 1) != 0)
				{
					verticalPageCount++;
				}
				if (verticalPageCount == 0)
				{
					verticalPageCount++;
				}
				_totalPages = verticalPageCount * horizontalPageCount;
				base.PrinterSettings.MinimumPage = 1;
				base.PrinterSettings.MaximumPage = _totalPages;
			}
			catch (Exception ex)
			{
				Mordex.Windows.Common.MxMessageBox.Show(ex.ToString());
			}
		}

		private bool IsItemOnPage(double doubleValue, double doubleValueArgument)
		{
			if (doubleValue >= _pageStartOffset && doubleValue <= _pageEndOffset)
			{
				return true;
			}
			if (doubleValueArgument > _pageStartOffset && doubleValue < _pageEndOffset)
			{
				return true;
			}
			return false;
		}

		private void UpdatePageItems()
		{
			if (_printItems == null)
			{
				return;
			}
			_pageItems = new ArrayList();
			_timeline.GetHeaderHeight();
			_ = _pageContentBounds.Top;
			foreach (TimelineItem item in _printItems)
			{
				if (IsItemOnPage(item._leftOffset, item._doubleValue))
				{
					_pageItems.Add(item);
				}
			}
		}

		private void LayoutPageRows(int rowCount)
		{
			int endRowIndex = _firstRowIndex + rowCount;
			int rowTop = _timeline.GetHeaderHeight() + _pageContentBounds.Top + 1;
			for (int i = _firstRowIndex; i < _rowLayouts.Count; i++)
			{
				MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)_rowLayouts[i];
				if (mordexRowLayoutState.GetRowType() == RowType.Group)
				{
					int groupLeft = _pageContentBounds.Left + _timeline.GroupIndent * mordexRowLayoutState.GetGroupIndex();
					mordexRowLayoutState.SetRowBounds(new Rectangle(groupLeft, rowTop, _pageContentBounds.Right - groupLeft, _groupRowHeight));
					rowTop += _groupRowHeight - 1;
					continue;
				}
				int rowLeft = _pageContentBounds.Left + _timeline.GroupIndent * mordexRowLayoutState.GetGroupIndex();
				mordexRowLayoutState.SetRowBounds(new Rectangle(rowLeft, rowTop, _pageContentBounds.Right - rowLeft, _rowHeight));
				if (mordexRowLayoutState.GetTimelineGroupRow() != null && !mordexRowLayoutState.GetMordexRowLayoutStateRectangle().IsEmpty)
				{
					mordexRowLayoutState.SetMordexRowLayoutStateRectangle(new Rectangle(mordexRowLayoutState.GetMordexRowLayoutStateRectangle().Left, rowTop, mordexRowLayoutState.GetMordexRowLayoutStateRectangle().Width, mordexRowLayoutState.GetMordexRowLayoutStateRectangle().Height));
				}
				rowTop += _rowHeight - 1;
			}
			rowTop = _timeline.GetHeaderHeight() + _pageContentBounds.Top + 1;
			for (int previousRowIndex = _firstRowIndex - 1; previousRowIndex >= 0; previousRowIndex--)
			{
				MxRowLayoutState mordexRowLayoutStateArgument = (MxRowLayoutState)_rowLayouts[previousRowIndex];
				if (mordexRowLayoutStateArgument.GetRowType() == RowType.Group)
				{
					int previousGroupLeft = _pageContentBounds.Left + _timeline.GroupIndent * mordexRowLayoutStateArgument.GetGroupIndex();
					rowTop -= _groupRowHeight - 1;
					mordexRowLayoutStateArgument.SetRowBounds(new Rectangle(previousGroupLeft, rowTop, _pageContentBounds.Right - previousGroupLeft, _groupRowHeight));
				}
				else
				{
					int previousRowLeft = _pageContentBounds.Left + _timeline.GroupIndent * mordexRowLayoutStateArgument.GetGroupIndex();
					rowTop -= _rowHeight - 1;
					mordexRowLayoutStateArgument.SetRowBounds(new Rectangle(previousRowLeft, rowTop, _pageContentBounds.Right - previousRowLeft, _rowHeight));
					if (mordexRowLayoutStateArgument.GetTimelineGroupRow() != null && !mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().IsEmpty)
					{
						mordexRowLayoutStateArgument.SetMordexRowLayoutStateRectangle(new Rectangle(mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().Left, rowTop, mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().Width, mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().Height));
					}
				}
			}
			foreach (TimelineItem item in _printItems)
			{
				if (item.GetSubRowIndex() + item.GetRowIndex() >= _firstRowIndex && item.GetSubRowIndex() + item.GetRowIndex() <= endRowIndex)
				{
					float x = (float)_pageContentBounds.Left + (float)((item._leftOffset - _pageStartOffset) * (double)_intervalWidth);
					MxRowLayoutState itemRowLayout = (MxRowLayoutState)_rowLayouts[item.GetRowIndex()];
					int itemTop = itemRowLayout.GetRowBounds().Y + 1;
					itemTop += item.GetSubRowIndex() * (_rowHeight - 1);
					item._rectangleF = new RectangleF(x, itemTop, (float)((item._doubleValue - item._leftOffset) * (double)_intervalWidth), _rowHeight - 2);
					item._durationWidth = item._durationLength * (double)_intervalWidth;
					item._estimatedDurationWidth = item._estimatedDurationLength * (double)_intervalWidth;
					item.UpdateTimelineItemRectangle((float)_pageContentBounds.Left + (float)((item._estimatedStartOffset - _pageStartOffset) * (double)_intervalWidth), _timeline.ItemsDurationBarHeight);
					item.UpdateDurationBarBounds((float)_pageContentBounds.Left + (float)((item._startOffset - _pageStartOffset) * (double)_intervalWidth), _timeline.ItemsDurationBarHeight);
					_pageItems.Add(item);
				}
				else
				{
					item._rectangleF = Rectangle.Empty;
					item._estimatedBarBounds = Rectangle.Empty;
					item._durationBarBounds = Rectangle.Empty;
				}
			}
		}

		private bool IsPageInPrintRange(int pageIndex)
		{
			if (base.PrinterSettings.PrintRange == PrintRange.SomePages)
			{
				if (pageIndex >= base.PrinterSettings.FromPage && pageIndex <= base.PrinterSettings.ToPage)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		private int MeasurePageRows(ref int rowCount)
		{
			int usedHeight = _timeline.GetHeaderHeight() + 1;
			for (int i = _firstRowIndex; i < _rowLayouts.Count; i++)
			{
				MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)_rowLayouts[i];
				if (mordexRowLayoutState.GetRowType() == RowType.Group)
				{
					if (usedHeight + _groupRowHeight > _pageContentBounds.Height)
					{
						return usedHeight;
					}
					usedHeight += _groupRowHeight - 1;
				}
				else
				{
					if (usedHeight + _rowHeight > _pageContentBounds.Height)
					{
						return usedHeight;
					}
					usedHeight += _rowHeight - 1;
				}
				rowCount++;
			}
			return usedHeight;
		}

		public void PrepareDocument()
		{
			if (_timeline != null)
			{
				using (Graphics graphics = base.PrinterSettings.CreateMeasurementGraphics())
				{
					PrintEventArgs e = new PrintEventArgs();
					base.OnBeginPrint(e);
					if (!e.Cancel)
					{
						CalculateLayout(graphics);
						base.PrinterSettings.FromPage = 1;
						base.PrinterSettings.ToPage = _totalPages;
						base.OnEndPrint(e);
					}
				}
			}
		}

		internal void DrawTierBackground(Graphics graphics, Timescale timescale, DateTime dateTime, Rectangle rect, Rectangle rectangle, bool shouldShowWeekNumbers, bool booleanArgument, bool isTopTier)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(PredefinedStyle.UpperScale);
			timelineFormatStyle.ApplyPrintStyle();
			Brush brush = null;
			if (_grayShading)
			{
				brush = timelineFormatStyle.GetBackBrush(Rectangle.Ceiling(rect));
				if (brush != null)
				{
					graphics.FillRectangle(brush, rect);
				}
			}
			else
			{
				graphics.FillRectangle(Brushes.White, rect);
			}
			Pen pen = null;
			if (_grayShading)
			{
				pen = new Pen(SystemColors.ControlDark);
			}
			else
			{
				pen = new Pen(Color.Black);
				timelineFormatStyle.BackColor = Color.Black;
			}
			Timeline.DrawTierLabels(graphics, timescale, dateTime, rect, textLayoutRenderer, timelineFormatStyle, shouldShowWeekNumbers, booleanArgument, _intervalWidth, isTopTier);
			graphics.DrawLine(pen, rect.Left, rect.Bottom, rect.Right - 1, rect.Bottom);
			pen.Dispose();
			textLayoutRenderer.ResetFormat();
			timelineFormatStyle.DisposeResources();
		}

		private void DrawTimescale(Graphics graphics, Rectangle rectangleArgument)
		{
			Rectangle rectangle = rectangleArgument;
			rectangle.Height = _timeline.GetHeaderHeight() + 1;
			Region clip = graphics.Clip;
			Region region = (graphics.Clip = new Region(new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height + 1)));
			rectangleArgument.Y++;
			Rectangle tierBounds = rectangleArgument;
			if (_timeline.TimescaleTiers == TimescaleTiers.ThreeTiers)
			{
				tierBounds.Height = _timeline.TopTier.GetBounds().Height;
				bool shouldShowTopWeekNumbers = _timeline.ShowWeekNumbers;
				if (shouldShowTopWeekNumbers && (_timeline.MiddleTier.Interval == TimelineInterval.Month || _timeline.MiddleTier.Interval == TimelineInterval.Week || _timeline.MiddleTier.Interval == TimelineInterval.Day))
				{
					shouldShowTopWeekNumbers = false;
				}
				DrawTierBackground(graphics, Timeline.TopTier, _pageStartDate, tierBounds, rectangleArgument, shouldShowTopWeekNumbers, true, true);
				tierBounds.Y += tierBounds.Height;
			}
			if (_timeline.TimescaleTiers != TimescaleTiers.OneTier)
			{
				tierBounds.Height = _timeline.MiddleTier.GetHeight();
				bool shouldShowMiddleWeekNumbers = _timeline.ShowWeekNumbers;
				if (shouldShowMiddleWeekNumbers && Timeline.BottomTier.Interval == TimelineInterval.Week)
				{
					shouldShowMiddleWeekNumbers = false;
				}
				DrawTierBackground(graphics, _timeline.MiddleTier, _pageStartDate, tierBounds, rectangleArgument, shouldShowMiddleWeekNumbers, _timeline.TimescaleTiers == TimescaleTiers.TwoTiers, false);
				if (_timeline.TimescaleTiers == TimescaleTiers.ThreeTiers && GrayShading)
				{
					graphics.DrawLine(SystemPens.ControlLightLight, tierBounds.Left, tierBounds.Top, tierBounds.Right, tierBounds.Top);
				}
				tierBounds.Y += tierBounds.Height;
			}
			tierBounds.Height = _timeline.BottomTier.GetBounds().Height;
			DrawBackground(graphics, _pageStartDate, tierBounds, _timescaleOriginX, _intervalWidth, rectangleArgument, false);
			region.Dispose();
			region = null;
			graphics.Clip = clip;
		}

		// A dark theme does not print: the default theme is the Standard theme until the job ends (see PrintLook).
		private IDisposable _printLook;

		protected override void OnEndPrint(PrintEventArgs e)
		{
			base.OnEndPrint(e);
			_printLook?.Dispose();
			_printLook = null;
		}

		protected override void OnBeginPrint(PrintEventArgs e)
		{
			base.OnBeginPrint(e);
			if (_timeline == null)
			{
				e.Cancel = true;
				return;
			}
			_printLook?.Dispose();
			_printLook = PrintLook.Begin();
			_timeline.Update();
			if (StartDate == EndDate)
			{
				EndDate = StartDate.AddSeconds(_timeline._intervalSeconds);
			}
			_currentPageIndex = 0;
			_isLayoutPending = true;
			if (base.PrinterSettings.PrintRange == PrintRange.SomePages)
			{
				_currentPageIndex = base.PrinterSettings.FromPage;
			}
		}
	}
}
