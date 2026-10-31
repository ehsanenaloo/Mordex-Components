using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Printing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	/// <summary>Prints the grid.</summary>
	/// <remarks>The grid always prints with the system colors on white paper (the Standard look), whatever theme the control shows. A dark or light theme does not change the printout. Colors that you set on the grid styles are kept.</remarks>
	public class MxDataGridPrintDocument : PrintDocument, IGridStyleSource
	{
		internal ControlStyle _printControlStyle;

		private MxDataGrid _dataGrid;

		private bool _printCellBackground = true;

		private bool _repeatHeaders = true;

		private bool _printBackgroundImage;

		private bool _printHierarchical;

		private bool _printCollapsedRows = true;

		private FitColumnsMode _fitColumns;

		private int _cardColumnsPerPage = 2;

		private bool _expandFarColumn = true;

		private int _pageNumber;

		private ArrayList _arrayList;

		internal Rectangle _pageBounds;

		private int _totalPageCount;

		private PrintPageRowLayout _currentPageLayout;

		private float _floatValue = 1f;

		private int _layoutPageIndex;

		private int _nextRowIndex;

		private string _pageHeaderLeft = "";

		private string _pageHeaderCenter = "";

		private string _pageHeaderRight = "";

		private string _pageFooterLeft = "";

		private string _pageFooterCenter = "";

		private string _pageFooterRight = "";

		private int _headerDistance = 85;

		private int _footerDistance = 85;

		private GridFormatStyle _gridFormatStyle = new GridFormatStyle();

		private GridFormatStyle _gridPrintDocumentGridFormatStyle = new GridFormatStyle();

		private bool _translateSystemColors = true;

		[Description("The DataGrid control that contains the data this instance will print.")]
		[DefaultValue(null)]
		public MxDataGrid DataGrid
		{
			get
			{
				return _dataGrid;
			}
			set
			{
				_dataGrid = value;
			}
		}

		[DefaultValue(true)]
		[Description("Determines whether the background of cells should be printed.")]
		public bool PrintCellBackground
		{
			get
			{
				return _printCellBackground;
			}
			set
			{
				_printCellBackground = value;
			}
		}

		[DefaultValue(true)]
		[Description("Determines whether column headers should be printed on any page or only in the first page.")]
		public bool RepeatHeaders
		{
			get
			{
				return _repeatHeaders;
			}
			set
			{
				_repeatHeaders = value;
			}
		}

		[Description("Determines whether the background image of the control should be printed.")]
		[DefaultValue(false)]
		public bool PrintBackgroundImage
		{
			get
			{
				return _printBackgroundImage;
			}
			set
			{
				_printBackgroundImage = value;
			}
		}

		[Description("Determines whether the DataGrid control should print its hierarchical structure or as a single table.")]
		[DefaultValue(false)]
		public bool PrintHierarchical
		{
			get
			{
				return _printHierarchical;
			}
			set
			{
				_printHierarchical = value;
			}
		}

		[DefaultValue(true)]
		[Description("Determines whether collapsed rows should be printed.")]
		public bool PrintCollapsedRows
		{
			get
			{
				return _printCollapsedRows;
			}
			set
			{
				_printCollapsedRows = value;
			}
		}

		[DefaultValue(FitColumnsMode.NoFit)]
		[Description("Determines how columns sholud be sized to fit in a page.")]
		public FitColumnsMode FitColumns
		{
			get
			{
				return _fitColumns;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FitColumnsMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_fitColumns = value;
			}
		}

		[DefaultValue(2)]
		[Description("Specifies the number of card columns to print in a page.")]
		public int CardColumnsPerPage
		{
			get
			{
				return _cardColumnsPerPage;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException();
				}
				_cardColumnsPerPage = value;
			}
		}

		[Description("Determines whether the last column in a page should be expanded to fit the page client area.")]
		[DefaultValue(true)]
		public bool ExpandFarColumn
		{
			get
			{
				return _expandFarColumn;
			}
			set
			{
				_expandFarColumn = value;
			}
		}

		[Browsable(false)]
		public int TotalPages => _totalPageCount;

		[Browsable(false)]
		public int Page => _pageNumber;

		[Description("The text displayed in the header section on the left of the page.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string PageHeaderLeft
		{
			get
			{
				return _pageHeaderLeft;
			}
			set
			{
				_pageHeaderLeft = value;
			}
		}

		[Localizable(true)]
		[Description("The text displayed in the header section on the center of the page.")]
		[DefaultValue("")]
		public string PageHeaderCenter
		{
			get
			{
				return _pageHeaderCenter;
			}
			set
			{
				_pageHeaderCenter = value;
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		[Description("The text displayed in the header section on the right of the page.")]
		public string PageHeaderRight
		{
			get
			{
				return _pageHeaderRight;
			}
			set
			{
				_pageHeaderRight = value;
			}
		}

		[Localizable(true)]
		[DefaultValue("")]
		[Description("The text displayed in the footer section on the left of the page.")]
		public string PageFooterLeft
		{
			get
			{
				return _pageFooterLeft;
			}
			set
			{
				_pageFooterLeft = value;
			}
		}

		[Description("The text displayed in the footer section on the center of the page.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string PageFooterCenter
		{
			get
			{
				return _pageFooterCenter;
			}
			set
			{
				_pageFooterCenter = value;
			}
		}

		[Localizable(true)]
		[DefaultValue("")]
		[Description("The text displayed in the footer section on the right of the page.")]
		public string PageFooterRight
		{
			get
			{
				return _pageFooterRight;
			}
			set
			{
				_pageFooterRight = value;
			}
		}

		[Localizable(true)]
		[Description("Represents the distance, in hundredths of an inch, from the top edge of the paper to the top edge of the header.")]
		[DefaultValue(85)]
		public int HeaderDistance
		{
			get
			{
				return _headerDistance;
			}
			set
			{
				_headerDistance = value;
			}
		}

		[DefaultValue(85)]
		[Description("Represents the distance, in hundredths of an inch, from the bottom edge of the paper to the top edge of the header.")]
		[Localizable(true)]
		public int FooterDistance
		{
			get
			{
				return _footerDistance;
			}
			set
			{
				_footerDistance = value;
			}
		}

		[Description("The format settings used to draw page headers.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public GridFormatStyle PageHeaderFormatStyle => _gridFormatStyle;

		[Description("Format settings used to draw page footers.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public GridFormatStyle PageFooterFormatStyle => _gridPrintDocumentGridFormatStyle;

		[Description("Determines whether system colors should be translated to gray scale colors for printing")]
		[DefaultValue(true)]
		public bool TranslateSystemColors
		{
			get
			{
				return _translateSystemColors;
			}
			set
			{
				_translateSystemColors = value;
			}
		}

		Font IGridStyleSource.ControlStyleFont => _dataGrid.Font;

		bool IGridStyleSource.IsRightToLeft => _dataGrid.RightToLeft == RightToLeft.Yes;

		private void CreatePrintControlStyle()
		{
			_printControlStyle = new ControlStyle(this);
			_printControlStyle.UseClassicLook();
			_printControlStyle.SetFlatBorderColor(_dataGrid._gridControlStyle.GetFlatBorderColor());
		}

		protected override void OnBeginPrint(PrintEventArgs e)
		{
			base.OnBeginPrint(e);
			_pageNumber = 0;
			if (_dataGrid == null || e.Cancel)
			{
				e.Cancel = true;
				return;
			}
			_printLook?.Dispose();
			_printLook = PrintLook.Begin();
			CreatePrintControlStyle();
			_arrayList = _dataGrid.ComputeArrayList(this);
			if (_arrayList.Count == 0 && _dataGrid.View != View.TableView && _dataGrid.RootTable != null)
			{
				e.Cancel = true;
				return;
			}
			e.Cancel = false;
			_layoutPageIndex = 0;
			_nextRowIndex = 0;
		}

		public void PrepareDocument()
		{
			if (_dataGrid == null)
			{
				return;
			}
			using (Graphics graphics = base.PrinterSettings.CreateMeasurementGraphics())
			{
				PrintEventArgs e = new PrintEventArgs();
				OnBeginPrint(e);
				if (e.Cancel)
				{
					return;
				}
				_arrayList = _dataGrid.ComputeArrayList(this);
				CountPages(graphics);
				base.PrinterSettings.FromPage = 1;
				base.PrinterSettings.ToPage = _totalPageCount;
				GridTableCollection tables = DataGrid.Tables;
				foreach (GridTable item in tables)
				{
					item._printLayout = null;
				}
				_floatValue = 1f;
				OnEndPrint(e);
			}
		}

		private Rectangle RaiseQueryPageSettings()
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

		private void CountPages(Graphics graphics)
		{
			try
			{
				_layoutPageIndex = 0;
				_nextRowIndex = 0;
				_totalPageCount = 0;
				_pageBounds = RaiseQueryPageSettings();
				if (_dataGrid.View == View.TableView)
				{
					RenderContent(graphics, _pageBounds);
				}
				else
				{
					CountCardPages(graphics, _pageBounds);
				}
				base.PrinterSettings.MinimumPage = 1;
				base.PrinterSettings.MaximumPage = _totalPageCount;
			}
			catch (Exception ex)
			{
				Mordex.Windows.Common.MxMessageBox.Show(ex.ToString());
			}
		}

		private void RenderContent(Graphics graphics, Rectangle rectangle)
		{
			if (_fitColumns == FitColumnsMode.Zooming)
			{
				int totalWidth = 0;
				foreach (MordexRow item in _arrayList)
				{
					GridTable gridTable = (GridTable)item.Table;
					gridTable.InitializePrintLayout(this, rectangle);
					if (gridTable._printLayout._gridState18Array[0].GetTotalWidth() > totalWidth)
					{
						totalWidth = gridTable._printLayout._gridState18Array[0].GetTotalWidth();
					}
				}
				if (totalWidth > rectangle.Width)
				{
					_floatValue = (float)rectangle.Width / (float)totalWidth;
				}
				else
				{
					_floatValue = 1f;
				}
				if ((double)_floatValue < 0.1)
				{
					_floatValue = 1f;
				}
			}
			if (_floatValue != 1f)
			{
				graphics.PageScale = _floatValue;
			}
			_totalPageCount = 1;
			while (AdvanceTablePage(graphics, rectangle))
			{
				_totalPageCount++;
			}
		}

		private void CountCardPages(Graphics graphics, Rectangle rectangle)
		{
			_totalPageCount = 1;
			while (MeasureCardPage(graphics, rectangle))
			{
				_totalPageCount++;
			}
		}

		private bool MeasureCardPage(Graphics graphics, Rectangle rectangle)
		{
			PrintPageCardLayout layoutCardPage = LayoutCardPage(graphics, rectangle);
			_dataGrid.GetCardCaptionStyle();
			layoutCardPage._pageTables.ClearAndDispose();
			layoutCardPage = null;
			if (_nextRowIndex < _arrayList.Count)
			{
				return true;
			}
			return false;
		}

		private bool AdvanceTablePage(Graphics graphics, Rectangle rectangle)
		{
			bool advanceTablePage = false;
			rectangle = RaiseQueryPageSettings();
			if (_currentPageLayout == null)
			{
				LayoutTablePage(graphics, rectangle);
			}
			_layoutPageIndex++;
			if (_layoutPageIndex >= _currentPageLayout.GetPageSectionCount())
			{
				advanceTablePage = ((_nextRowIndex < _arrayList.Count) ? true : false);
				_layoutPageIndex = 0;
				_currentPageLayout._pageRows.ClearAll();
				_currentPageLayout = null;
			}
			else
			{
				advanceTablePage = true;
			}
			return advanceTablePage;
		}

		// A dark theme does not print: the default theme is the Standard theme until the job ends (see PrintLook).
		private IDisposable _printLook;

		protected override void OnEndPrint(PrintEventArgs e)
		{
			try
			{
				base.OnEndPrint(e);
			}
			finally
			{
				_printLook?.Dispose();
				_printLook = null;
				if (_dataGrid != null)
				{
					GridTableCollection tables = DataGrid.Tables;
					foreach (GridTable item in tables)
					{
						item._printLayout = null;
					}
				}
				_floatValue = 1f;
				_totalPageCount = 0;
				if (_currentPageLayout != null)
				{
					_currentPageLayout._pageRows.ClearAll();
					_currentPageLayout = null;
				}
				if (_printControlStyle != null)
				{
					_printControlStyle.GetLayoutState().DisposeRenderers();
					_printControlStyle._printDocument = null;
					_printControlStyle = null;
				}
			}
		}

		protected override void OnPrintPage(PrintPageEventArgs e)
		{
			try
			{
				if (_pageNumber == 0)
				{
					CountPages(e.Graphics);
					_layoutPageIndex = 0;
					_nextRowIndex = 0;
				}
				_dataGrid._owner = this;
				_pageBounds = RaiseQueryPageSettings();
				if (_dataGrid.View == View.TableView)
				{
					ProcessPrintPageEventArgs(e);
				}
				else
				{
					PrintCardPage(e);
				}
				e.Graphics.PageScale = 1f;
				PrintPageFooter(e);
			}
			finally
			{
				if (_dataGrid != null)
				{
					_dataGrid._owner = null;
				}
			}
		}

		private void ProcessPrintPageEventArgs(PrintPageEventArgs eventArgs)
		{
			_pageNumber++;
			if (IsPageInPrintRange(_pageNumber))
			{
				base.OnPrintPage(eventArgs);
			}
			if (_printBackgroundImage && _dataGrid.BackgroundImage != null)
			{
				_dataGrid.DrawControlBackground(eventArgs.Graphics, eventArgs.MarginBounds, eventArgs.MarginBounds, false, false);
			}
			if (_floatValue != 1f)
			{
				eventArgs.Graphics.PageScale = _floatValue;
			}
			if (_currentPageLayout == null)
			{
				LayoutTablePage(eventArgs.Graphics, eventArgs.MarginBounds);
			}
			if (IsPageInPrintRange(_pageNumber))
			{
				DataGrid.LayoutRowStateItems(_currentPageLayout._pageRows, eventArgs.Graphics);
				foreach (MxRowLayoutState item in _currentPageLayout._pageRows)
				{
					DrawLayoutRow(eventArgs.Graphics, item, _layoutPageIndex);
				}
			}
			_layoutPageIndex++;
			if (_layoutPageIndex >= _currentPageLayout.GetPageSectionCount())
			{
				if (_nextRowIndex < _arrayList.Count)
				{
					eventArgs.HasMorePages = true;
				}
				else
				{
					eventArgs.HasMorePages = false;
				}
				_layoutPageIndex = 0;
				_currentPageLayout._pageRows.ClearAll();
				_currentPageLayout = null;
			}
			else
			{
				eventArgs.HasMorePages = true;
			}
			if (!IsPageInPrintRange(_pageNumber) && eventArgs.HasMorePages)
			{
				ProcessPrintPageEventArgs(eventArgs);
			}
			if (base.PrinterSettings.PrintRange == PrintRange.SomePages && _pageNumber == base.PrinterSettings.ToPage)
			{
				eventArgs.HasMorePages = false;
			}
		}

		private bool IsPageInPrintRange(int value)
		{
			if (base.PrinterSettings.PrintRange == PrintRange.SomePages)
			{
				if (value >= base.PrinterSettings.FromPage && value <= base.PrinterSettings.ToPage)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		private GridFormatStyle BuildRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = new GridFormatStyle(null, PredefinedStyle.RowFormatStyle);
			gridFormatStyle = gridFormatStyle.CloneWithDefaults(_dataGrid.DefaultAlphaLevel, _dataGrid.GetBackColorAlphaMode(), _dataGrid.GetForeColorAlphaMode(), _dataGrid.BlendColor);
			gridFormatStyle = gridFormatStyle.MergeWith(_dataGrid.RowFormatStyle, false, false);
			if (gridTable != null && gridTable.RowFormatStyle != null)
			{
				gridFormatStyle = gridFormatStyle.ApplyOverrides(gridTable.RowFormatStyle);
			}
			gridFormatStyle.UpdateFloatValue(_floatValue);
			return gridFormatStyle;
		}

		internal GridFormatStyle BuildCardColumnHeaderStyle(int value)
		{
			GridFormatStyle gridFormatStyle = BuildAlternatingRowStyle(_dataGrid.RootTable, value);
			return gridFormatStyle.ApplyOverrides(_dataGrid.CardColumnHeaderFormatStyle);
		}

		private GridFormatStyle BuildAlternatingRowStyle(GridTable gridTable, int value)
		{
			GridFormatStyle gridFormatStyle = BuildRowStyle(gridTable);
			if (!_dataGrid.AlternatingColors || value % 2 == 0)
			{
				return gridFormatStyle;
			}
			gridFormatStyle = gridFormatStyle.ApplyOverrides(new GridFormatStyle(null, PredefinedStyle.AlternatingRowFormatStyle));
			gridFormatStyle = gridFormatStyle.MergeWith(_dataGrid.AlternatingRowFormatStyle, false, false);
			if (gridTable != null && gridTable.AlternatingRowFormatStyle != null)
			{
				gridFormatStyle = gridFormatStyle.ApplyOverrides(gridTable.AlternatingRowFormatStyle);
			}
			gridFormatStyle.UpdateFloatValue(_floatValue);
			return gridFormatStyle;
		}

		private GridFormatStyle BuildTotalRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = DataGrid.GetTotalRowStyle(gridTable);
			gridFormatStyle.UpdateFloatValue(_floatValue);
			return gridFormatStyle;
		}

		private GridFormatStyle BuildGroupRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = new GridFormatStyle(null, PredefinedStyle.GroupRowFormatStyle);
			gridFormatStyle = gridFormatStyle.CloneWithDefaults(_dataGrid.DefaultAlphaLevel, _dataGrid.GetBackColorAlphaMode(), _dataGrid.GetForeColorAlphaMode(), _dataGrid.BlendColor);
			gridFormatStyle = gridFormatStyle.MergeWith(_dataGrid.GroupRowFormatStyle, false, false);
			if (gridTable != null && gridTable.GroupRowFormatStyle != null)
			{
				gridFormatStyle = gridFormatStyle.ApplyOverrides(gridTable.GroupRowFormatStyle);
			}
			gridFormatStyle.UpdateFloatValue(_floatValue);
			return gridFormatStyle;
		}

		private GridFormatStyle BuildGroupTotalRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = BuildGroupRowStyle(gridTable);
			gridFormatStyle = gridFormatStyle.MergeWith(_dataGrid.GroupTotalRowFormatStyle, false, false);
			if (gridTable != null && gridTable.GroupTotalRowFormatStyle != null)
			{
				gridFormatStyle = gridFormatStyle.MergeWith(gridTable.GroupTotalRowFormatStyle, false, false);
			}
			gridFormatStyle.UpdateFloatValue(_floatValue);
			return gridFormatStyle;
		}

		internal GridFormatStyle BuildHeaderStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = new GridFormatStyle(null, PredefinedStyle.HeaderFormatStyle);
			gridFormatStyle = gridFormatStyle.CloneWithDefaults(_dataGrid.DefaultAlphaLevel, _dataGrid.GetBackColorAlphaMode(), DataGrid.GetForeColorAlphaMode(), _dataGrid.BlendColor);
			gridFormatStyle = gridFormatStyle.MergeWith(_dataGrid.HeaderFormatStyle, false, false);
			if (gridTable != null && !gridTable.HeaderFormatStyle.IsDefaultStyle())
			{
				gridFormatStyle = gridFormatStyle.MergeWith(gridTable.HeaderFormatStyle, false, false);
			}
			gridFormatStyle.UpdateFloatValue(_floatValue);
			return gridFormatStyle;
		}

		internal GridFormatStyle BuildCardCaptionStyle()
		{
			GridFormatStyle gridFormatStyle = new GridFormatStyle(null, PredefinedStyle.CardCaptionFormatStyle);
			gridFormatStyle = gridFormatStyle.CloneWithDefaults(_dataGrid.DefaultAlphaLevel, _dataGrid.GetBackColorAlphaMode(), DataGrid.GetForeColorAlphaMode(), _dataGrid.BlendColor);
			gridFormatStyle = gridFormatStyle.MergeWith(_dataGrid.CardCaptionFormatStyle, false, false);
			gridFormatStyle.UpdateFloatValue(_floatValue);
			return gridFormatStyle;
		}

		private void DrawLayoutRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, int value)
		{
			switch (mordexRowLayoutState._rowType)
			{
			case LayoutRowType.GroupTotalRow:
				DrawGroupTotalRow(graphics, mordexRowLayoutState, value);
				break;
			case LayoutRowType.GroupHeaderRow:
				DrawGroupHeaderRow(graphics, mordexRowLayoutState, value);
				break;
			case LayoutRowType.ColumnHeader:
				DrawColumnHeaderRow(graphics, mordexRowLayoutState, value);
				break;
			case LayoutRowType.DataRow:
				DrawDataRow(graphics, mordexRowLayoutState, value);
				break;
			case LayoutRowType.TotalRow:
				DrawTotalRow(graphics, mordexRowLayoutState, value);
				break;
			case LayoutRowType.Space:
			{
				GridFormatStyle gridFormatStyle = BuildRowStyle(mordexRowLayoutState.LayoutTable);
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					if (item._rowHeight == value)
					{
						DataGrid.PaintLayoutItemForPrint(graphics, this, item, gridFormatStyle, item.GetAbsoluteItemRect());
					}
				}
				gridFormatStyle.DisposeResources();
				break;
			}
			case LayoutRowType.NewRow:
			case LayoutRowType.FilterRow:
				break;
			}
		}

		private void DrawTotalRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, int value)
		{
			GridFormatStyle gridFormatStyle = BuildTotalRowStyle(mordexRowLayoutState.LayoutTable);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState._gridRow.RowStyle);
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._rowHeight == value)
				{
					GridFormatStyle otherGridFormatStyle = ((item._column == null) ? applyOverrides : applyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle));
					DataGrid.PaintLayoutItemForPrint(graphics, this, item, otherGridFormatStyle, item.GetAbsoluteItemRect());
					if (item._column != null && otherGridFormatStyle != applyOverrides)
					{
						otherGridFormatStyle.DisposeResources();
					}
				}
				applyOverrides.DisposeResources();
				gridFormatStyle.DisposeResources();
			}
		}

		private void DrawGroupTotalRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, int value)
		{
			GridFormatStyle gridFormatStyle = BuildGroupTotalRowStyle(mordexRowLayoutState.LayoutTable);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState._gridRow.Group.GetGroupRowStyle());
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.RowStyle);
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._rowHeight == value)
				{
					GridFormatStyle otherGridFormatStyle = ((item._column == null) ? otherApplyOverrides : otherApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle));
					_dataGrid.PaintLayoutItemForPrint(graphics, this, item, otherGridFormatStyle, item.GetAbsoluteItemRect());
					if (item._column != null && otherGridFormatStyle != otherApplyOverrides)
					{
						otherGridFormatStyle.DisposeResources();
					}
				}
				otherApplyOverrides.DisposeResources();
				applyOverrides.DisposeResources();
				gridFormatStyle.DisposeResources();
			}
		}

		internal GridFormatStyle BuildGroupHeaderCellStyle(GridGroupLayoutState gridGroupLayoutState, bool condition)
		{
			GridFormatStyle gridFormatStyle = BuildGroupRowStyle(gridGroupLayoutState._gridGroup.Table);
			GridRow gridRow = _dataGrid.GetGridRowForLogicalRow(gridGroupLayoutState.GetMordexRow());
			gridFormatStyle = gridFormatStyle.ApplyOverrides(gridGroupLayoutState._gridGroup.GetGroupRowStyle());
			if (gridRow != null)
			{
				gridFormatStyle = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
				gridFormatStyle = gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
			}
			return gridFormatStyle;
		}

		private void DrawColumnHeaderRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, int value)
		{
			GridFormatStyle gridFormatStyle = BuildHeaderStyle(mordexRowLayoutState.LayoutTable);
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._rowHeight == value)
				{
					Rectangle rectangle = item.GetAbsoluteItemRect();
					GridFormatStyle applyOverrides = null;
					applyOverrides = ((item._column != null) ? gridFormatStyle.ApplyOverrides(item._column.HeaderStyle) : ((item._columnSet == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(item._columnSet.HeaderStyle)));
					_dataGrid.PaintLayoutItemForPrint(graphics, this, item, applyOverrides, rectangle);
					if (applyOverrides != gridFormatStyle)
					{
						applyOverrides.DisposeResources();
					}
				}
			}
		}

		private void LayoutTablePage(Graphics graphics, Rectangle marginBounds)
		{
			_currentPageLayout = new PrintPageRowLayout(marginBounds, _floatValue);
			MordexRow mordexRow = null;
			if (_arrayList.Count > 0)
			{
				mordexRow = (MordexRow)_arrayList[_nextRowIndex];
			}
			MordexRow arrayListItem = null;
			GridTable gridTable = null;
			if (_nextRowIndex == 0)
			{
				gridTable = ((mordexRow != null) ? ((GridTable)mordexRow.Table) : _dataGrid.RootTable);
				gridTable.InitializePrintLayout(this, marginBounds);
				if (gridTable.AreColumnHeadersVisible() || (gridTable.CellLayoutMode == CellLayoutMode.UseColumnSets && gridTable.AreColumnSetHeadersVisible()))
				{
					GridFormatStyle gridFormatStyle = BuildHeaderStyle(gridTable);
					MxRowLayoutState mordexRowLayoutState = gridTable.RenderContent(graphics, this, null, null, gridFormatStyle);
					gridFormatStyle.DisposeResources();
					_currentPageLayout.TryAddRow(mordexRowLayoutState, true);
				}
			}
			else
			{
				arrayListItem = (MordexRow)_arrayList[_nextRowIndex - 1];
				if (arrayListItem.Table != mordexRow.Table || _repeatHeaders)
				{
					gridTable = (GridTable)mordexRow.Table;
					gridTable.InitializePrintLayout(this, marginBounds);
					if (gridTable.AreColumnHeadersVisible() || (gridTable.CellLayoutMode == CellLayoutMode.UseColumnSets && gridTable.AreColumnSetHeadersVisible()))
					{
						GridFormatStyle gridFormatStyle = BuildHeaderStyle(gridTable);
						MxRowLayoutState renderContent = gridTable.RenderContent(graphics, this, null, null, gridFormatStyle);
						gridFormatStyle.DisposeResources();
						_currentPageLayout.TryAddRow(renderContent, true);
					}
				}
			}
			bool condition = true;
			bool otherCondition = true;
			for (int i = _nextRowIndex; i < _arrayList.Count; i++)
			{
				mordexRow = (MordexRow)_arrayList[i];
				GridTable table = (GridTable)mordexRow.Table;
				table.InitializePrintLayout(this, marginBounds);
				MxRowLayoutState spaceRowLayout;
				if (gridTable != table)
				{
					if (!condition && DataGrid.TableSpacing > 0)
					{
						spaceRowLayout = table.BuildSpaceRowLayout(graphics, mordexRow, arrayListItem, DataGrid.TableSpacing, _printCollapsedRows);
						foreach (GridGroupLayoutState item in spaceRowLayout._arrayList)
						{
							item._rowHeight = 0;
						}
						if (!_currentPageLayout.TryAddRow(spaceRowLayout, false))
						{
							break;
						}
					}
					if ((arrayListItem == null || !(mordexRow is MordexTotalRow)) && (table.AreHeadersRepeated() || RowStateHelper.IsFirstSibling(mordexRow) || arrayListItem == null) && (table.AreColumnHeadersVisible() || (table.CellLayoutMode == CellLayoutMode.UseColumnSets && table.AreColumnSetHeadersVisible())))
					{
						GridFormatStyle gridFormatStyle = BuildHeaderStyle(table);
						spaceRowLayout = table.RenderContent(graphics, this, mordexRow, arrayListItem, gridFormatStyle);
						gridFormatStyle.DisposeResources();
						if (!_currentPageLayout.TryAddRow(spaceRowLayout, false))
						{
							break;
						}
					}
					gridTable = table;
				}
				condition = false;
				GridRow gridRow = DataGrid.GetGridRowForLogicalRow(mordexRow);
				DataGrid.PrepareRowForDisplay(gridRow, false);
				switch (gridRow.RowType)
				{
				case RowType.GroupHeader:
				{
					GridFormatStyle gridFormatStyle = BuildGroupRowStyle(table);
					spaceRowLayout = table.BuildPrintGroupHeaderRowLayout(graphics, this, gridRow, arrayListItem, gridFormatStyle);
					gridFormatStyle.DisposeResources();
					break;
				}
				case RowType.GroupFooter:
				{
					GridFormatStyle gridFormatStyle = BuildGroupTotalRowStyle(table);
					spaceRowLayout = table.BuildPrintGroupTotalRowLayout(graphics, this, gridRow, arrayListItem, gridFormatStyle);
					gridFormatStyle.DisposeResources();
					break;
				}
				case RowType.TotalRow:
				{
					GridFormatStyle gridFormatStyle = BuildTotalRowStyle(table);
					spaceRowLayout = ((!table.IsRootTable()) ? table.BuildPrintTotalRowLayout(graphics, this, gridRow, arrayListItem, gridFormatStyle) : table.BuildPrintTotalRowLayout(graphics, this, gridRow, null, gridFormatStyle));
					gridFormatStyle.DisposeResources();
					break;
				}
				default:
				{
					int positionOfChild = mordexRow.ParentRow.PositionOfChild(mordexRow);
					GridFormatStyle gridFormatStyle = BuildAlternatingRowStyle(table, positionOfChild);
					spaceRowLayout = table.BuildPrintDataRowLayout(graphics, this, gridRow, arrayListItem, gridFormatStyle);
					gridFormatStyle.DisposeResources();
					break;
				}
				}
				spaceRowLayout._gridRow = gridRow;
				if (!_currentPageLayout.TryAddRow(spaceRowLayout, otherCondition))
				{
					break;
				}
				_nextRowIndex = i + 1;
				otherCondition = false;
				arrayListItem = mordexRow;
			}
		}

		private void DrawGroupHeaderRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, int value)
		{
			GridFormatStyle gridFormatStyle = BuildGroupRowStyle(mordexRowLayoutState.LayoutTable);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState.GridRow.Group.GetGroupRowStyle());
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.RowStyle);
			GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.GetCachedFormatStyle());
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._rowHeight != value)
				{
					continue;
				}
				if (item._type == LayoutItemType.GroupHeaderCell)
				{
					if (mordexRowLayoutState._gridRow.Cells[item._column.Index]._groupHeaderTotal is GridGroupHeaderTotal gridGroupHeaderTotal)
					{
						GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridGroupHeaderTotal.FormatStyle);
						GridFormatStyle firstApplyOverrides = previousApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle);
						_dataGrid.PaintLayoutItemForPrint(graphics, this, item, firstApplyOverrides, item.GetAbsoluteItemRect());
						firstApplyOverrides.DisposeResources();
						previousApplyOverrides.DisposeResources();
					}
					else
					{
						GridFormatStyle secondApplyOverrides = nextApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle);
						_dataGrid.PaintLayoutItemForPrint(graphics, this, item, secondApplyOverrides, item.GetAbsoluteItemRect());
						secondApplyOverrides.DisposeResources();
					}
				}
				else
				{
					_dataGrid.PaintLayoutItemForPrint(graphics, this, item, nextApplyOverrides, item.GetAbsoluteItemRect());
				}
			}
			nextApplyOverrides.DisposeResources();
			otherApplyOverrides.DisposeResources();
			applyOverrides.DisposeResources();
			gridFormatStyle.DisposeResources();
		}

		private void DrawDataRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, int value)
		{
			GridFormatStyle gridFormatStyle = BuildAlternatingRowStyle(mordexRowLayoutState.LayoutTable, mordexRowLayoutState.GetRowPosition());
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState._gridRow.RowStyle);
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.GetCachedFormatStyle());
			GridFormatStyle nextApplyOverrides = null;
			GridFormatStyle previousApplyOverrides = null;
			GridFormatStyle firstApplyOverrides = null;
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._rowHeight != value)
				{
					continue;
				}
				Rectangle rectangle = item.GetAbsoluteItemRect();
				if (item._column != null && item._type != LayoutItemType.PreviewRow)
				{
					nextApplyOverrides = otherApplyOverrides.ApplyOverrides(item._column.CellStyle);
					previousApplyOverrides = nextApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].GetEffectiveFormatStyle());
					firstApplyOverrides = previousApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle);
				}
				else if (item._type == LayoutItemType.PreviewRow)
				{
					nextApplyOverrides = gridFormatStyle.ApplyOverrides(_dataGrid.PreviewRowFormatStyle);
					if (item.GetOwnerTable().PreviewRowFormatStyle != null)
					{
						firstApplyOverrides = nextApplyOverrides.ApplyOverrides(item.GetOwnerTable().PreviewRowFormatStyle);
					}
					firstApplyOverrides = firstApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.GetCachedRowStyle());
				}
				else
				{
					firstApplyOverrides = otherApplyOverrides;
				}
				_dataGrid.PaintLayoutItemForPrint(graphics, this, item, firstApplyOverrides, rectangle);
				if (item._column != null && item._type != LayoutItemType.PreviewRow)
				{
					if (firstApplyOverrides != previousApplyOverrides)
					{
						firstApplyOverrides.DisposeResources();
					}
					if (previousApplyOverrides != nextApplyOverrides)
					{
						previousApplyOverrides.DisposeResources();
					}
					if (nextApplyOverrides != otherApplyOverrides)
					{
						nextApplyOverrides.DisposeResources();
					}
				}
				if (otherApplyOverrides != applyOverrides)
				{
					otherApplyOverrides.DisposeResources();
				}
				if (applyOverrides != gridFormatStyle)
				{
					applyOverrides.DisposeResources();
				}
			}
		}

		private void PrintCardPage(PrintPageEventArgs eventArgs)
		{
			_pageNumber++;
			if (IsPageInPrintRange(_pageNumber))
			{
				base.OnPrintPage(eventArgs);
			}
			if (_printBackgroundImage && _dataGrid.BackgroundImage != null)
			{
				_dataGrid.DrawControlBackground(eventArgs.Graphics, eventArgs.MarginBounds, eventArgs.MarginBounds, false, false);
			}
			PrintPageCardLayout layoutCardPage = LayoutCardPage(eventArgs.Graphics, eventArgs.MarginBounds);
			if (IsPageInPrintRange(_pageNumber))
			{
				GridFormatStyle gridFormatStyle = BuildCardCaptionStyle();
				foreach (GridTableLayoutState item in layoutCardPage._pageTables)
				{
					DrawCard(eventArgs.Graphics, item, gridFormatStyle);
				}
			}
			layoutCardPage._pageTables.ClearAndDispose();
			layoutCardPage = null;
			if (_nextRowIndex < _arrayList.Count)
			{
				eventArgs.HasMorePages = true;
			}
			else
			{
				eventArgs.HasMorePages = false;
			}
			if (!IsPageInPrintRange(_pageNumber) && eventArgs.HasMorePages)
			{
				PrintCardPage(eventArgs);
			}
			if (base.PrinterSettings.PrintRange == PrintRange.SomePages && _pageNumber == base.PrinterSettings.ToPage)
			{
				eventArgs.HasMorePages = false;
			}
		}

		private void DrawCard(Graphics graphics, GridTableLayoutState gridTableLayoutState, GridFormatStyle otherGridFormatStyle)
		{
			GridFormatStyle gridFormatStyle = BuildAlternatingRowStyle(gridTableLayoutState.LayoutTable, gridTableLayoutState.GetLogicalRowIndex());
			GridFormatStyle cardColumnHeaderStyle = BuildCardColumnHeaderStyle(gridTableLayoutState.GetLogicalRowIndex());
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridTableLayoutState.GridRow.RowStyle);
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridTableLayoutState.GridRow.GetCachedFormatStyle());
			foreach (GridGroupLayoutState item in gridTableLayoutState._arrayList)
			{
				switch (item._type)
				{
				case LayoutItemType.CardHeader:
				{
					GridFormatStyle nextApplyOverrides = cardColumnHeaderStyle.ApplyOverrides(item._column.HeaderStyle);
					_dataGrid.PaintLayoutItemForPrint(graphics, this, item, nextApplyOverrides, item.GetAbsoluteItemRect());
					if (nextApplyOverrides != cardColumnHeaderStyle)
					{
						nextApplyOverrides.DisposeResources();
					}
					break;
				}
				case LayoutItemType.CardCell:
					if (item._column != null)
					{
						GridFormatStyle previousApplyOverrides = otherApplyOverrides.ApplyOverrides(item._column.CellStyle);
						GridFormatStyle firstApplyOverrides = previousApplyOverrides.ApplyOverrides(gridTableLayoutState.GridRow.Cells[item._column.Index].GetEffectiveFormatStyle());
						GridFormatStyle secondApplyOverrides = firstApplyOverrides.ApplyOverrides(gridTableLayoutState.GridRow.Cells[item._column.Index].FormatStyle);
						_dataGrid.PaintLayoutItemForPrint(graphics, this, item, secondApplyOverrides, item.GetAbsoluteItemRect());
						if (secondApplyOverrides != firstApplyOverrides)
						{
							secondApplyOverrides.DisposeResources();
						}
						if (firstApplyOverrides != previousApplyOverrides)
						{
							firstApplyOverrides.DisposeResources();
						}
						if (previousApplyOverrides != otherApplyOverrides)
						{
							previousApplyOverrides.DisposeResources();
						}
					}
					else
					{
						_dataGrid.PaintLayoutItemForPrint(graphics, this, item, otherApplyOverrides, item.GetAbsoluteItemRect());
					}
					break;
				case LayoutItemType.CardCaption:
					_dataGrid.PaintLayoutItemForPrint(graphics, this, item, otherGridFormatStyle, item.GetAbsoluteItemRect());
					break;
				}
			}
			if (otherApplyOverrides != applyOverrides)
			{
				otherApplyOverrides.DisposeResources();
				otherApplyOverrides = null;
			}
			if (applyOverrides != gridFormatStyle)
			{
				applyOverrides.DisposeResources();
				applyOverrides = null;
			}
			if (DataGrid.CardBorders)
			{
				Pen pen = GdiCache.Pen(otherGridFormatStyle.BackColor);
				Rectangle rectangle = gridTableLayoutState.GetBounds();
				rectangle = _dataGrid.MirrorRectForPrintRtl(this, graphics, rectangle);
				graphics.DrawRectangle(pen, rectangle);
				pen = null;
			}
		}

		private PrintPageCardLayout LayoutCardPage(Graphics graphics, Rectangle marginBounds)
		{
			PrintPageCardLayout printPageCardLayout = new PrintPageCardLayout(_cardColumnsPerPage, _dataGrid.CardSpacing, marginBounds, 1f);
			GridFormatStyle gridFormatStyle = _dataGrid.GetCardCaptionStyle();
			for (int nextRowIndex = _nextRowIndex; nextRowIndex < _arrayList.Count; nextRowIndex = (_nextRowIndex = nextRowIndex + 1))
			{
				MordexRow mordexRow = (MordexRow)_arrayList[nextRowIndex];
				GridRow gridRow = _dataGrid.GetGridRowForLogicalRow(mordexRow);
				_dataGrid.PrepareRowForDisplay(gridRow, false);
				int positionOfChild = mordexRow.ParentRow.PositionOfChild(mordexRow);
				GridFormatStyle alternatingRowStyle = BuildAlternatingRowStyle((GridTable)mordexRow.Table, positionOfChild);
				GridFormatStyle cardColumnHeaderStyle = _dataGrid.GetCardColumnHeaderStyle(positionOfChild);
				GridTableLayoutState gridTableLayoutState = _dataGrid.BuildPrintCardLayout(graphics, this, gridRow, marginBounds, gridFormatStyle, cardColumnHeaderStyle, alternatingRowStyle);
				alternatingRowStyle.DisposeResources();
				cardColumnHeaderStyle.DisposeResources();
				if (!printPageCardLayout.TryAddTable(gridTableLayoutState))
				{
					break;
				}
			}
			gridFormatStyle.DisposeResources();
			return printPageCardLayout;
		}

		private void PrintPageFooter(PrintPageEventArgs eventArgs)
		{
			Rectangle rectangle = new Rectangle(eventArgs.MarginBounds.Left, eventArgs.PageBounds.Bottom - _footerDistance, eventArgs.MarginBounds.Width, _footerDistance);
			using (StringFormat stringFormat = new StringFormat())
			{
				DrawHeaderAndFooter(eventArgs, rectangle, stringFormat);
			}
		}

		private void DrawHeaderAndFooter(PrintPageEventArgs eventArgs, Rectangle rectangle, StringFormat stringFormat)
		{
			GridFormatStyle gridFormatStyle = _dataGrid.GetRowStyle(null);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(_gridPrintDocumentGridFormatStyle);
			applyOverrides = applyOverrides.CreatePrintStyle(this);
			Brush brush = applyOverrides.GetForeBrush();
			if (brush != null)
			{
				if (_pageFooterLeft.Length != 0)
				{
					stringFormat.Alignment = StringAlignment.Near;
					eventArgs.Graphics.DrawString(ReplaceHeaderFooterTokens(_pageFooterLeft, Page, _totalPageCount), applyOverrides.ResolveFont(_dataGrid.Font), brush, rectangle, stringFormat);
				}
				if (_pageFooterRight.Length != 0)
				{
					stringFormat.Alignment = StringAlignment.Far;
					eventArgs.Graphics.DrawString(ReplaceHeaderFooterTokens(_pageFooterRight, Page, _totalPageCount), applyOverrides.ResolveFont(_dataGrid.Font), brush, rectangle, stringFormat);
				}
				if (_pageFooterCenter.Length != 0)
				{
					stringFormat.Alignment = StringAlignment.Center;
					eventArgs.Graphics.DrawString(ReplaceHeaderFooterTokens(_pageFooterCenter, Page, _totalPageCount), applyOverrides.ResolveFont(_dataGrid.Font), brush, rectangle, stringFormat);
				}
			}
			if (applyOverrides != gridFormatStyle)
			{
				applyOverrides.DisposeResources();
			}
			rectangle = new Rectangle(eventArgs.MarginBounds.Left, 0, eventArgs.MarginBounds.Width, _headerDistance);
			applyOverrides = gridFormatStyle.ApplyOverrides(_gridFormatStyle);
			applyOverrides = applyOverrides.CreatePrintStyle(this);
			brush = applyOverrides.GetForeBrush();
			if (brush != null)
			{
				stringFormat.LineAlignment = StringAlignment.Far;
				if (_pageHeaderLeft.Length != 0)
				{
					stringFormat.Alignment = StringAlignment.Near;
					eventArgs.Graphics.DrawString(ReplaceHeaderFooterTokens(_pageHeaderLeft, Page, _totalPageCount), applyOverrides.ResolveFont(_dataGrid.Font), brush, rectangle, stringFormat);
				}
				if (_pageHeaderRight.Length != 0)
				{
					stringFormat.Alignment = StringAlignment.Far;
					eventArgs.Graphics.DrawString(ReplaceHeaderFooterTokens(_pageHeaderRight, Page, _totalPageCount), applyOverrides.ResolveFont(_dataGrid.Font), brush, rectangle, stringFormat);
				}
				if (_pageHeaderCenter.Length != 0)
				{
					stringFormat.Alignment = StringAlignment.Center;
					eventArgs.Graphics.DrawString(ReplaceHeaderFooterTokens(_pageHeaderCenter, Page, _totalPageCount), applyOverrides.ResolveFont(_dataGrid.Font), brush, rectangle, stringFormat);
				}
			}
			if (applyOverrides != gridFormatStyle)
			{
				applyOverrides.DisposeResources();
			}
			gridFormatStyle.DisposeResources();
		}

		void IGridStyleSource.OnControlStyleChanged()
		{
		}

		MxColorSchemeAppearanceState IGridStyleSource.GetDrawingStyle(GridStyleArea styleArea)
		{
			return new MxColorSchemeAppearanceState(useTextRenderer: false);
		}

		ImageList IGridStyleSource.GetImageList()
		{
			return DataGrid.ComputeImageList();
		}

		private string ReplaceHeaderFooterTokens(string otherText, int value, int otherValue)
		{
			string text = otherText;
			text = text.Replace("[Time Printed]", DateTime.Now.ToShortTimeString());
			text = text.Replace("[Page #]", value.ToString());
			text = text.Replace("[Total Pages]", otherValue.ToString());
			text = text.Replace("[Date Printed]", DateTime.Today.ToShortDateString());
			return text.Replace("[User Name]", SystemInformation.UserName);
		}
	}
}
