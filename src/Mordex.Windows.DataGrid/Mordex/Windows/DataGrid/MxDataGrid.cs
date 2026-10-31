using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Accessibility;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	[Designer("Mordex.Windows.DataGrid.Design.DataGridDesigner, Mordex.Windows.DataGrid.Design")]
	[ToolboxItem(true)]
	[ComplexBindingProperties("DataSource", "DataMember")]
	[DefaultProperty("DataSource")]
	[DefaultEvent("FormattingRow")]
	public partial class MxDataGrid : ScrollableBase, ISupportInitialize, IFormatStyleOwner, IMordexXmlLayoutsSupport, ITextBoxOwner, IDragHost, IMordexContainer, ISupportFilterBuilder, IPersistComponentSettings, IGridDropListOwner
	{
		internal class MxDataGridAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleRole Role => AccessibleRole.Table;

			public override string Name
			{
				get
				{
					string accessibleName = base.Owner.AccessibleName;
					if (accessibleName != null)
					{
						return accessibleName;
					}
					return base.Owner.Name;
				}
				set
				{
					base.Owner.AccessibleName = value;
				}
			}

			internal MxDataGridAccessibleObject(MxDataGrid owner)
				: base(owner)
			{
			}

			internal MxDataGrid ComputeDataGrid()
			{
				return (MxDataGrid)base.Owner;
			}

			internal MordexRootRow GetRootRow()
			{
				return ComputeDataGrid().GetRowSourceManager().RootRow;
			}

			public override int GetChildCount()
			{
				if (GetRootRow() != null && ComputeDataGrid() != null)
				{
					int rowCount = 0;
					switch (ComputeDataGrid().View)
					{
					case View.TableView:
						rowCount = 2;
						rowCount += GetRootRow().Rows.Count;
						if (ComputeDataGrid().RootTable != null && ComputeDataGrid().RootTable.IsAddNewAllowed() && (ComputeDataGrid().RootTable.ComputeNewRowPosition() == NewRowPosition.TopRow || ComputeDataGrid().RootTable.Groups.Count == 0))
						{
							rowCount++;
						}
						if (ComputeDataGrid().RootTable.IsTotalRowVisible())
						{
							rowCount++;
						}
						break;
					case View.CardView:
						rowCount = ComputeDataGrid().RowCount;
						break;
					case View.SingleCard:
						if (ComputeDataGrid().RowCount > 0)
						{
							rowCount = 1;
						}
						break;
					}
					return rowCount + 2;
				}
				return 0;
			}

			public override AccessibleObject GetChild(int index)
			{
				MxDataGrid dataGrid = ComputeDataGrid();
				MordexTableRow mordexTableRow = GetRootRow();
				if (mordexTableRow == null)
				{
					return null;
				}
				if (dataGrid._view == View.TableView)
				{
					if (index == 0)
					{
						return dataGrid.GetOrCreateGridAccessibleObject();
					}
					index--;
					if (index == 0)
					{
						return dataGrid.GetOrCreateRowAccessibleObject(mordexTableRow);
					}
					index--;
					if (dataGrid.RootTable.IsAddNewAllowed() && dataGrid.RootTable.ComputeNewRowPosition() == NewRowPosition.TopRow)
					{
						if (index == 0)
						{
							return dataGrid.GetOrCreateRowAccessibleObject(mordexTableRow.NewRow);
						}
						index--;
					}
				}
				if (dataGrid._view == View.SingleCard)
				{
					if (dataGrid.RowCount > 0)
					{
						if (index == 0 && dataGrid.CurrentRow != null)
						{
							return dataGrid.GetOrCreateRowAccessibleObject(dataGrid.CurrentRow.GetLogicalRow());
						}
						index--;
					}
				}
				else
				{
					if (index >= 0 && index < mordexTableRow.Rows.Count)
					{
						return dataGrid.GetOrCreateRowAccessibleObject(mordexTableRow.Rows[index]);
					}
					index -= mordexTableRow.Rows.Count;
				}
				if (dataGrid.RootTable.ComputeNewRowPosition() == NewRowPosition.BottomRow && dataGrid.RootTable.IsAddNewAllowed())
				{
					if (index == 0)
					{
						return dataGrid.GetOrCreateRowAccessibleObject(mordexTableRow.NewRow);
					}
					index--;
				}
				if (dataGrid._view == View.TableView && dataGrid.RootTable.IsTotalRowVisible())
				{
					if (index == 0)
					{
						return dataGrid.GetOrCreateRowAccessibleObject(mordexTableRow.TotalRow);
					}
					index--;
				}
				switch (index)
				{
				case 0:
					return ComputeDataGrid().GetVScroll().AccessibilityObject;
				case 1:
					return ComputeDataGrid().GetHorizontalScrollBar().AccessibilityObject;
				default:
					return null;
				}
			}

			public override AccessibleObject HitTest(int integer, int integerArgument)
			{
				return ComputeDataGrid().GetAccessibleObjectAtPoint(integer, integerArgument);
			}

			public override void Select(AccessibleSelection flags)
			{
				base.Select(flags);
			}

			public override AccessibleObject GetFocused()
			{
				MxDataGrid dataGrid = ComputeDataGrid();
				if (dataGrid.CurrentRow == null)
				{
					return null;
				}
				if (dataGrid.GetOrCreateRowAccessibleObject(dataGrid.CurrentRow.GetLogicalRow()) is RowAccessibleObject mordexRowAccessibleObject)
				{
					if (dataGrid.CurrentRow.RowType == RowType.GroupHeader)
					{
						return mordexRowAccessibleObject;
					}
					if (dataGrid.CurrentColumn == null)
					{
						return mordexRowAccessibleObject.GetChild(0);
					}
					return mordexRowAccessibleObject.ComputeAccessibleObject(dataGrid.CurrentColumn) as GridColumnAccessibleObject;
				}
				return base.GetFocused();
			}

			public override AccessibleObject GetSelected()
			{
				MxDataGrid dataGrid = ComputeDataGrid();
				if (dataGrid.CurrentRow == null)
				{
					return null;
				}
				if (dataGrid.GetOrCreateRowAccessibleObject(dataGrid.CurrentRow.GetLogicalRow()) is RowAccessibleObject result)
				{
					return result;
				}
				return base.GetSelected();
			}
		}

		public const int alphaEmpty = -1;

		public const int filterRowPosition = -2;

		public const int newTopRowPosition = -1;

		public const int emptyCurrentRow = -3;

		private GridDataSourceManager _gridDataSourceManager;

		private object _currentRowData;

		private ArrayList _arrayList;

		private bool _isInitializing;

		private CurrencyManager _currencyManager;

		private bool _hasCurrencyManagerHandlers;

		private bool _isBuildingBoundData;

		private string _dataMember = "";

		private bool _isCreatingBindingContext;

		private BindingContext _bindingContext;

		private int _rowCount;

		private object _dataSource;

		private bool _isPaintingEnabled = true;

		private GridFormatStyleCollection _formatStyleCollection;

		private GridLayout _currentLayout;

		private GridLayout _gridLayout;

		private GridLayoutCollection _layouts;

		private Rectangle _partialPaintBounds;

		private bool _isRenderingPartial;

		internal MxDataGridPrintDocument _owner;

		private bool _bufferNeedsRecreate;

		private bool _isPaintInProgress;

		private GridWatermarkImage _watermarkImage;

		private View _view;

		private int _horizontalScrollOffset;

		private int _firstRow;

		private int _contentWidth;

		private bool _isLayoutValid;

		internal TableLayoutCollection _cardRowLayouts;

		internal RowLayoutList _bodyRowLayouts;

		internal RowLayoutList _headerRowLayouts;

		internal RowLayoutList _footerRowLayouts;

		private GridHitTester _hitTester;

		private GridHitTester HitTester => _hitTester ??= new GridHitTester(this);

		private bool _isRenderingContent;

		private int _tableViewHorizontalScrollIncrement = 8;

		private int _verticalScrollOffset;

		private int _maxCardHeight;

		private int _bodyTopY;

		private int _bodyBottomY;

		private RowLayoutList _previousBodyRowLayouts;

		private bool _hasStaleRowCells;

		private int _groupByBoxHeight;

		private bool _isDragPending;

		private Rectangle _dragStartBounds;

		private Point _mouseDownPoint;

		private UpdateScope _selectionChangeBatch;

		private CornerShadowRenderer _cachedImageRenderer;

		private bool _isFocusHandled;

		private bool _isEnteringForEdit;

		private GridGroupLayoutState _gridGridGroupLayoutState;

		private FocusCellDisplayMode _focusCellDisplayMode;

		private int _selectionAnchorRow;

		private GridColumn _gridColumn;

		private bool _isCellMousePressed;

		private GridValueState _gridGridValueState;

		private GridGroupLayoutState _mouseDownLayoutItem;

		private System.Windows.Forms.Timer _gridTimer;

		private MouseButtons _mouseButtons;

		private int _dragSelectStartRow = -1;

		private Point _selectionRectStart = new Point(-3, -3);

		private Point _selectionRectEnd = new Point(-3, -3);

		private int _dragSelectCurrentRow = -1;

		private int _mouseClickCount;

		private GridGroupLayoutState _selectorPressedItem;

		private Rectangle _dragThresholdBounds;

		private GridGroupLayoutState _activeButtonItem;

		private bool _isButtonPressActive;

		private GridGroupLayoutState _lastClickedLayoutItem;

		private bool _isMouseDownSelectionPending;

		private bool _isCellDragSelecting;

		private bool _isDragSelectionSuppressed;

		private HeaderDragFeedback _pressFeedbackRenderer;

		private GridGroupLayoutState _pressedHeaderItem;

		private int _headerDragState;

		private bool _isHeaderPressed;

		private bool _isUpdatingHeaderPress;

		private GridGroupLayoutState _toolTipLayoutItem;

		private GridWindowMessageHandler _gridWindowMessageHandler;

		private GridGroupLayoutState _hoveredLayoutItem;

		internal bool _isEditorDialogOpen;

		private bool _groupByBoxVisible = true;

		private int _indent = 18;

		private int _groupIndent = 16;

		private SelectionMode _selectionMode;

		private CellSelectionMode _cellSelectionMode;

		private bool _treeLines = true;

		private Color _treeLineColor = SystemColors.ControlDark;

		private Pen _rowBorderPen;

		private Pen _gridlinePen;

		private bool _hierarchical;

		private ImageList _externalImageList;

		private GridRowCache _rowCache;

		private Dictionary<MordexRow, GridRow> _dictionary;
#pragma warning disable CS0414 // Layout state kept for the persisted format.

		private bool _isClearing;
#pragma warning restore CS0414

		private GridRow _gridRow;

		private GridRow _row;

		private bool _isRowEditSuppressed;

		private GridColumn _currentColumn;

		private bool _isCurrentCellChanging;

		private bool _isStartingEdit;

		private GridTableCollection _gridTableCollection;

		private bool _isSelectingAll;

		private GridSelectedItemCollection _gridSelectedItemCollection;

		private bool _isSelectionUpdating;

		private bool _isSelectionNotifyEnabled = true;

		private GridColumnContainerControl _gridColumnContainerControl;

		// The style made for a custom cell editor. The grid owns it and disposes it when the edit ends.
		private GridFormatStyle _customEditStyle;

		private System.Windows.Forms.Timer _editDelayTimer;

		private GridColumnContainerControl _gridGridColumnContainerControl = new GridColumnContainerControl();

		private bool _editorHadFocus;

		private GridValueListItemCollectionScrollableBase _gridValueListItemCollectionScrollableBase;

		private bool _isDropDownToggling;

		private System.Windows.Forms.Timer _filterRowDelayTimer;

		private GridColumn _gridGridColumn;

		private bool _isKeyEventHandled;

		private bool _eventErrorHandling = true;

		private bool _isDataChangeTrackingEnabled = true;

		private bool _isDataChanging;

		private UpdatingCellEventArgs _updatingCellEventArgs;

		private bool _autoEdit;

		private bool _cellDataChanged;

		private int _cardWidth = 250;

		private string _cardCaptionPrefix = "";

		private int _cardSpacing = 12;

		private int _cardInnerSpacing;

		private bool _cardBorders = true;

		private CardViewGridlines _cardViewGridlines;

		private bool _showEmptyFields = true;

		private bool _automaticSort = true;

		private bool _allowCardSizing = true;

		internal Rectangle[] _cardBounds;

		private bool _expandableCards = true;

		private bool _cardHeaders = true;

		private HideSelection _hideSelection;

		private BackgroundImageDrawMode _backgroundImageDrawMode = BackgroundImageDrawMode.Center;

		private bool _alternatingColors;

		private int _tableSpacing = 4;

		private EventHandler _resetDataSourceHandler;

		private bool _isPreviewTextCached;

		private bool _showRowPreview = true;

		private bool _isRowPreviewDirty;

		private FilterMode _filterMode;

		private FieldChooserForm _gridFieldChooser;

		private bool _columnAutoResize;

		private InheritableBoolean _hideColumnsWhenGrouped = InheritableBoolean.False;

		private InheritableBoolean _allowRemoveColumns = InheritableBoolean.False;

		private InheritableBoolean _rowHeaders = InheritableBoolean.False;

		private InheritableBoolean _allowDelete = InheritableBoolean.False;

		private InheritableBoolean _allowEdit = InheritableBoolean.True;

		private InheritableBoolean _columnHeaders = InheritableBoolean.True;

		private InheritableBoolean _columnSetHeaders = InheritableBoolean.True;

		private ColumnSetNavigation _columnSetNavigation;

		private Color _gridLineColor = SystemColors.ControlDark;

		private GridLines _gridLines = GridLines.Both;

		private GridLineStyle _gridLineStyle = GridLineStyle.SmallDots;

		private InheritableBoolean _repeatHeaders = InheritableBoolean.True;

		private bool _continuousScroll = true;

		private bool _emptyRows;

		private bool _allowColumnDrag = true;

		private InheritableBoolean _expandableGroups = InheritableBoolean.True;

		private float _defaultBlend;

		private Color _blendColor = SystemColors.Window;

		private FocusStyle _focusStyle;

		private bool _flatBorderInHeaders = true;

		private GridDropDownCollection _dropDowns;

		private bool _hasEditorsControlStyle;

		private ControlStyle _editorsControlStyle;

		private TabKeyBehavior _tabKeyBehavior;

		private EnterKeyBehavior _enterKeyBehavior;

		private bool _acceptsEscape = true;

		private ImageSource _imageSource = ImageSource.ExternalImageList;

		private ImageList _imageList;

		private bool _centerSingleCard = true;

		private InheritableBoolean _tableHeaders = InheritableBoolean.False;

		private GridOwnerDrawnArea _ownerDrawnAreas;

		private int _defaultAlphaLevel = -1;

		private AlphaMode _defaultAlphaMode = AlphaMode.Opaque;

		private AlphaMode _defaultBackColorAlphaMode;

		private AlphaMode _defaultForeColorAlphaMode;

		private GridFormatStyle _rowWithErrorsFormatStyle;

		private GridFormatStyle _linkFormatStyle;

		private GridFormatStyle _focusCellFormatStyle;

		private GridFormatStyle _rowFormatStyle;

		private GridFormatStyle _rowHeaderFormatStyle;

		private GridFormatStyle _headerFormatStyle;

		private GridFormatStyle _tableHeaderFormatStyle;

		private GridFormatStyle _alternatingRowFormatStyle;

		private GridFormatStyle _groupByBoxFormatStyle;

		private GridFormatStyle _groupByBoxInfoFormatStyle;

		private GridFormatStyle _groupRowFormatStyle;

		private GridFormatStyle _groupTotalRowFormatStyle;

		private GridFormatStyle _totalRowFormatStyle;

		private GridFormatStyle _gridGridFormatStyle;

		private GridFormatStyle _previewRowFormatStyle;

		private GridFormatStyle _selectedFormatStyle;

		private GridFormatStyle _selectedInactiveFormatStyle;

		private GridFormatStyle _cardCaptionFormatStyle;

		private GridFormatStyle _cardColumnHeaderFormatStyle;

		private GridFormatStyle _filterRowFormatStyle;

		private static readonly object _eventKeyFormattingRow = new object();

		private static readonly object _eventKeyLoadingRow = new object();

		private static readonly object _eventKeyEditingCell = new object();

		private static readonly object _eventKeyCellEdited = new object();

		private static readonly object _eventKeyMovingColumn = new object();

		private static readonly object _eventKeyColumnMoved = new object();

		private static readonly object _eventKeyCellUpdated = new object();

		private static readonly object _eventKeyUpdatingCell = new object();

		private static readonly object _eventKeyError = new object();

		private static readonly object _eventKeyDeletingRecords = new object();

		private static readonly object _eventKeyRecordsDeleted = new object();

		private static readonly object _eventKeyGroupsChanging = new object();

		private static readonly object _eventKeyGroupsChanged = new object();

		private static readonly object _eventKeyRecordUpdated = new object();

		private static readonly object _eventKeyRecordAdded = new object();

		private static readonly object _eventKeyUpdatingRecord = new object();

		private static readonly object _eventKeyAddingRecord = new object();

		private static readonly object _eventKeyDraggingColumn = new object();

		private static readonly object _eventKeyDraggingGroup = new object();

		private static readonly object _eventKeyDraggingColumnSet = new object();

		private static readonly object _eventKeySizingCards = new object();

		private static readonly object _eventKeyColumnButtonClick = new object();

		private static readonly object _eventKeyClearFilterButtonClick = new object();

		private static readonly object _eventKeySizingColumn = new object();

		private static readonly object _eventKeySizingColumnSet = new object();

		private static readonly object _eventKeyColumnHeaderClick = new object();

		private static readonly object _eventKeyGroupByBoxHeaderClick = new object();

		private static readonly object _eventKeyDropDown = new object();

		private static readonly object _eventKeyScroll = new object();

		private static readonly object _eventKeyDropDownHide = new object();

		private static readonly object _eventKeyNotInList = new object();

		private static readonly object _eventKeyCurrentCellChanging = new object();

		private static readonly object _eventKeyCurrentCellChanged = new object();

		private static readonly object _eventKeySelectionChanged = new object();

		private static readonly object _eventKeyApplyingFilter = new object();

		private static readonly object _eventKeyFilterApplied = new object();

		private static readonly object _eventKeyLayoutLoad = new object();

		private static readonly object _eventKeyCurrentLayoutChanging = new object();

		private static readonly object _eventKeyCurrentLayoutChanged = new object();

		private static readonly object _eventKeyLinkClicked = new object();

		private static readonly object _eventKeyDrawGridArea = new object();

		private static readonly object _eventKeyRowCountChanged = new object();

		private static readonly object _eventKeyInitCustomEdit = new object();

		private static readonly object _eventKeyEndCustomEdit = new object();

		private static readonly object _eventKeyExpandingRow = new object();

		private static readonly object _eventKeyRowExpanded = new object();

		private static readonly object _eventKeyCollapsingRow = new object();

		private static readonly object _eventKeyRowCollapsed = new object();

		private static readonly object _eventKeyGetNewRow = new object();

		private static readonly object _eventKeyFirstChange = new object();

		private static readonly object _eventKeyCancelingRowEdit = new object();

		private static readonly object _eventKeyRowEditCanceled = new object();

		private static readonly object _eventKeyCancelingCellEdit = new object();

		private static readonly object _eventKeyCellEditCanceled = new object();

		private static readonly object _cellValueChangedEventKey = new object();

		private static readonly object _sortKeysChangedEventKey = new object();

		private static readonly object _inputMaskErrorEventKey = new object();

		private static readonly object _rowDragEventKey = new object();

		private static readonly object _getChildListEventKey = new object();

		private static readonly object _editModeChangedEventKey = new object();

		private static readonly object _rowDoubleClickEventKey = new object();

		private static readonly object _fieldChooserClosedEventKey = new object();

		private static readonly object _columnVisibleChangedEventKey = new object();

		private static readonly object _columnVisibleChangingEventKey = new object();

		private static readonly object _parseEditValueEventKey = new object();

		private static readonly object _createMultipleValueItemEventKey = new object();

		private static readonly object _deletingRecordEventKey = new object();

		private static readonly object _rowCheckStateChangedEventKey = new object();

		private static readonly object _rowCheckStateChangingEventKey = new object();

		private static readonly object _rootTableChangedEventKey = new object();

		private static readonly object _columnAutoSizeEventKey = new object();

		private static readonly object _fetchCellToolTipTextEventKey = new object();

		private static readonly object _fitColumnsEventKey = new object();

		private bool _updateOnLeave = true;

		private bool _delayEditionOnClick;

		private string _cellToolTipText = "";

		private CellToolTip _cellToolTip = CellToolTip.UseCellToolTipText;

		private IncrementalSearchMode _incrementalSearchMode;

		private GridColumn _searchColumn;

		private string _incrementalSearchText = "";

		private System.Windows.Forms.Timer _incrementalSearchTimer;

		private UpdateMode _updateMode;

		private bool _showErrors = true;

		private int _errorImageIndex = -2;

		private string _errorImageKey = "";

		private GridColumnContainerControl _previousColumnContainerControl;

		private InvalidValueAction _invalidValueAction = InvalidValueAction.DiscardChanges;

		private int _frozenColumns;

		private bool _isSelectionVisible;

		private bool _wasTableView;

		private bool _useDefaultMenu = true;

		private GridBuiltInTextList _builtInTexts;

		private int _lastRecordPosition;

		private MordexRow _mordexRow;

		private bool _isNoRowSelected;

		private bool _isCurrentCellVisible = true;

		private bool _isEditCancelling;

		private List<IRefreshListener> _refreshListeners;

		private TotalRowPosition _totalRowPosition;

		private RowHeaderContent _rowHeaderContent = RowHeaderContent.RowHeaderText;

		private GridRow _cachedRow;

		private bool _keepRowSettings;

		private bool _useGroupRowSelector;

		private bool _selectOnExpand = true;

		private ColumnAutoSizeMode _columnAutoSizeMode;

		private ApplicationSettings _mordexApplicationSettings;

		private bool _saveSettings;

		private string _settingsKey = "";

		private BufferedGraphics _bufferedGraphics;

		private bool _isBufferStale;

		private Rectangle _bufferedClipRectangle;

		private NewRowEnterKeyBehavior _newRowEnterKeyBehavior;

		private FilterRowUpdateMode _filterRowUpdateMode;

		private int _filterRowUpdateDelay;

		private FilterConditionOperator _defaultFilterRowComparison;

		private FilterRowButtonStyle _filterRowButtonStyle = FilterRowButtonStyle.ClearButton;

		private ContentAlignment _emptyGridInfoAlignment = ContentAlignment.TopCenter;

		private GridAccessibleObject _groupAccessibleObject;

		internal MxDataGridAccessibleObject _tableAccessibleObject;

		private Image _treeExpandGlyph;

		private Image _treeCollapseGlyph;

		private ExpandBoxStyle _groupExpandBoxStyle;

		private ExpandBoxStyle _hierarchicalExpandBoxStyle;

		public MxDataGrid()
		{
			SetDefaultControlSize();
			InitializeCollections();
			SetStyle(ControlStyles.ContainerControl, value: true);
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.StandardDoubleClick, value: true);
			SetStyle(ControlStyles.StandardClick, value: false);
			SetStyle(ControlStyles.UserMouse, value: true);
			ResourceImages.GetCurrentRowImage();
		}
	}
}
