using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Text;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridTable : MordexTableBase, IFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		private MxDataGrid _dataGrid;

		private GridColumnSetCollection _columnSets;

		private CellLayoutMode _cellLayoutMode;

		private InheritableBoolean _columnSetHeaders;

		private int _columnSetRowCount = 2;

		private int _previewRowLeftMargin = 16;

		private bool _previewRow;

		private string _previewRowMember = "";

		private int _previewRowLines = 3;

		private int _headerLines = 1;

		private InheritableBoolean _columnHeaders;

		private int _columnSetHeaderLines = 1;

		private InheritableBoolean _rowHeaders;

		private int _rowHeaderWidth = -1;

		private Color _lineColor = Color.Empty;

		private GridLines _gridLines;

		private GridLineStyle _gridLineStyle;

		private Pen _gridLinePen;

		private bool _isGridLinePenShared;

		private InheritableBoolean _allowEdit;

		private InheritableBoolean _allowDelete;

		private InheritableBoolean _repeatHeaders;

		private GridFormatConditionCollection _formatConditions;

		private int _rowHeight = -1;

		private GridFormatCondition _groupCondition;

		private bool _useGroupCondition;

		private bool _showGroupConditionCount;

		private string _groupConditionCountTitle = "";

		internal bool _isAutoResizePending;

		internal ColumnLayoutMap _printLayout;

		private InheritableBoolean _hideColumnsWhenGrouped;

		private InheritableBoolean _allowRemoveColumns;

		private InheritableBoolean _expandableGroups;

		private string _newRowCaption = "";

		private InheritableBoolean _tableHeader;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private bool _allowGroup = true;

		private object _tag;

		private string _cellToolTipContent;

		private CellToolTip _cellToolTip;

		private RowHeaderContent _rowHeaderContent;

		private InheritableBoolean _useGroupRowSelector;

		private GridFormatStyle _groupByBoxInfoFormatStyle;

		private GridFormatStyle _rowFormatStyle;

		private GridFormatStyle _rowHeaderFormatStyle;

		private GridFormatStyle _headerFormatStyle;

		private GridFormatStyle _tableHeaderFormatStyle;

		private GridFormatStyle _alternatingRowFormatStyle;

		private GridFormatStyle _groupRowFormatStyle;

		private GridFormatStyle _groupTotalRowFormatStyle;

		private GridFormatStyle _totalRowFormatStyle;

		private GridFormatStyle _newRowFormatStyle;

		private GridFormatStyle _previewRowFormatStyle;
	}
}
