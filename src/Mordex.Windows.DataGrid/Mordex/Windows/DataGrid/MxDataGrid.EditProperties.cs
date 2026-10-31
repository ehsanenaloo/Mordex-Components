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
using System.Runtime.CompilerServices;
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
	public partial class MxDataGrid
	{
		[Description("Indicates whether the control should update the current row when users moves to another control.")]
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool UpdateOnLeave
		{
			get
			{
				return _updateOnLeave;
			}
			set
			{
				_updateOnLeave = value;
			}
		}

		[DefaultValue(false)]
		[Category("Behavior")]
		[Description("Indicates whether the should should wait for presenting the edit control in a cell when the user clicks in it.")]
		public bool DelayEditionOnClick
		{
			get
			{
				return _delayEditionOnClick;
			}
			set
			{
				_delayEditionOnClick = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(false)]
		public bool HasCheckedRows
		{
			get
			{
				if (_gridDataSourceManager.RootRow != null)
				{
					return _gridDataSourceManager.RootRow.CheckedState != CheckedState.Unchecked;
				}
				return false;
			}
		}

		[DefaultValue("")]
		[Description("Text used as tool tip for cells")]
		public string CellToolTipText
		{
			get
			{
				return _cellToolTipText;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_cellToolTipText = value;
			}
		}

		[DefaultValue(CellToolTip.UseCellToolTipText)]
		[Category("Behavior")]
		[Description("Indicates which tool tip text is used in cells.")]
		public CellToolTip CellToolTip
		{
			get
			{
				return _cellToolTip;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CellToolTip), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == CellToolTip.Default)
				{
					value = CellToolTip.UseCellToolTipText;
				}
				_cellToolTip = value;
			}
		}

		[DefaultValue(IncrementalSearchMode.None)]
		[Description("Specifies how the control will search records when the user types the control")]
		public IncrementalSearchMode IncrementalSearchMode
		{
			get
			{
				return _incrementalSearchMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(IncrementalSearchMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_incrementalSearchMode = value;
			}
		}

		[Description("Represents the column in the RootTable the control should search for values when it is in incremental search mode.")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(null)]
		[Editor("Mordex.Windows.DataGrid.Design.ColumnEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridColumn SearchColumn
		{
			get
			{
				return _searchColumn;
			}
			set
			{
				if (value != null && value.Table != RootTable)
				{
					throw new ArgumentException("SearchColumn must belong to the root table of the control.");
				}
				_searchColumn = value;
			}
		}

		[Description("Determines when the control updates the data source.")]
		[DefaultValue(UpdateMode.RowUpdate)]
		public UpdateMode UpdateMode
		{
			get
			{
				return _updateMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(UpdateMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_updateMode == value)
				{
					return;
				}
				if (_updateMode == UpdateMode.CellUpdate)
				{
					UpdateData();
				}
				_updateMode = value;
				if (_updateMode == UpdateMode.CellUpdate)
				{
					if (_invalidValueAction == InvalidValueAction.Continue)
					{
						_invalidValueAction = InvalidValueAction.DiscardChanges;
					}
					else if (_invalidValueAction == InvalidValueAction.ContinueAndShowErrorMessage)
					{
						_invalidValueAction = InvalidValueAction.DiscardChangesAndShowErrorMessage;
					}
				}
			}
		}

		[Description("Indicates whether the error image is shown in cells with errors.")]
		[DefaultValue(true)]
		public bool ShowErrors
		{
			get
			{
				return _showErrors;
			}
			set
			{
				if (value != _showErrors)
				{
					_showErrors = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(-2)]
		[Description("The image list index value of the image displayed in rows and cells with errors.")]
		public int ErrorImageIndex
		{
			get
			{
				return _errorImageIndex;
			}
			set
			{
				if (value < -2)
				{
					throw new ArgumentException();
				}
				if (_errorImageIndex != value)
				{
					_errorImageIndex = value;
					if (_errorImageIndex != -1)
					{
						_errorImageKey = "";
					}
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("The key accessor for the error image in the ImageList.")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue("")]
		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public string ErrorImageKey
		{
			get
			{
				return _errorImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_errorImageKey != value)
				{
					_errorImageKey = value;
					if (_errorImageKey.Length > 0)
					{
						_errorImageIndex = -1;
					}
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Determines the behavior of the control when an invalid value has been entered by the user in a cell.")]
		[DefaultValue(InvalidValueAction.DiscardChanges)]
		public InvalidValueAction InvalidValueAction
		{
			get
			{
				return _invalidValueAction;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InvalidValueAction), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_updateMode == UpdateMode.CellUpdate && !_isInitializing && (value == InvalidValueAction.Continue || value == InvalidValueAction.ContinueAndShowErrorMessage))
				{
					throw new InvalidOperationException("Continue or ContinueAndShowErrorMessage can not be used when UpdateMode is equal to CellUpdate.");
				}
				if (value == InvalidValueAction.Default)
				{
					value = InvalidValueAction.DiscardChanges;
				}
				_invalidValueAction = value;
			}
		}

		[DefaultValue(0)]
		[Description("Specifies the number of columns at the left that are fixed in the control.\n\r Setting this property to -1 will make just the row header fixed.")]
		public int FrozenColumns
		{
			get
			{
				return _frozenColumns;
			}
			set
			{
				if (value < -1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _frozenColumns)
				{
					_frozenColumns = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[DefaultValue(false)]
		[Browsable(false)]
		public bool CurrentCellDroppedDown
		{
			get
			{
				if (_gridColumnContainerControl != null)
				{
					return _gridColumnContainerControl.IsDroppedDown();
				}
				return false;
			}
			set
			{
				if (CurrentCellDroppedDown == value)
				{
					return;
				}
				EnsureVisible();
				BeginCellEdit(false, '\0', false);
				if (_gridColumnContainerControl == null)
				{
					return;
				}
				_gridColumnContainerControl.SetDroppedDown(value);
				if (Control.MouseButtons == MouseButtons.None && value)
				{
					Control control = _gridColumnContainerControl.GetDropControl();
					if (control != null && !control.Capture)
					{
						GridHelper.SetCapture(control, true);
					}
				}
			}
		}

		public override Cursor Cursor
		{
			get
			{
				if (!_isLayoutValid)
				{
					return base.Cursor;
				}
				Cursor cursor = GetCursorAtPointer();
				if (cursor == null)
				{
					return base.Cursor;
				}
				return cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("LayoutStream property will be removed in future versions. Use DesignTimeLayout property instead.")]
		[DefaultValue(null)]
		[Browsable(false)]
		public Stream LayoutStream
		{
			get
			{
				return null;
			}
			set
			{
				if (value != null)
				{
					LoadLayoutFile(value);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public TextBox EditTextBox
		{
			get
			{
				if (EditMode == EditMode.EditOn && _gridColumnContainerControl != null)
				{
					return _gridColumnContainerControl.ComputeTextBox();
				}
				return null;
			}
		}

		TextBox ITextBoxOwner.TextBoxControl => EditTextBox;

		bool ITextBoxOwner.UseDefaultMenu
		{
			get
			{
				return _useDefaultMenu;
			}
			set
			{
				if (_useDefaultMenu == value)
				{
					return;
				}
				_useDefaultMenu = value;
				if (EditTextBox != null)
				{
					if (!_useDefaultMenu)
					{
						EditTextBox.ContextMenuStrip = new ContextMenuStrip();
					}
					else
					{
						EditTextBox.ContextMenuStrip = null;
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Localizable(true)]
		[Browsable(false)]
		public GridBuiltInTextList BuiltInTexts => _builtInTexts;

		[Localizable(true)]
		[Browsable(false)]
		public string BuiltInTextsData
		{
			get
			{
				return BuiltInTexts.GetCustomTextsXml();
			}
			set
			{
				BuiltInTexts.SetCustomTextsXml(value);
			}
		}

		[DefaultValue(false)]
		[Description("Determines whether the FilterCondition of a table is evaludated only when it is set or dinamically (every time a row is updated)")]
		public bool DynamicFiltering
		{
			get
			{
				return _gridDataSourceManager.DynamicFiltering;
			}
			set
			{
				_gridDataSourceManager.DynamicFiltering = value;
			}
		}

		[Description("Determines the position of the total row in DataGrid control")]
		[DefaultValue(TotalRowPosition.BottomScrollable)]
		public TotalRowPosition TotalRowPosition
		{
			get
			{
				return _totalRowPosition;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TotalRowPosition), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_totalRowPosition != value)
				{
					_totalRowPosition = value;
					if (_gridDataSourceManager != null)
					{
						GetRowManager().Refresh();
					}
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Determines what is rendered in row headers.")]
		[DefaultValue(RowHeaderContent.RowHeaderText)]
		public RowHeaderContent RowHeaderContent
		{
			get
			{
				return _rowHeaderContent;
			}
			set
			{
				if (!Enum.IsDefined(typeof(RowHeaderContent), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == RowHeaderContent.Default)
				{
					value = RowHeaderContent.RowHeaderText;
				}
				if (_rowHeaderContent != value)
				{
					_rowHeaderContent = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override bool AutoSize
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		[Browsable(false)]
		public GridRow CurrentRow => GetRow();

		[Description("Determines whether row and cell settings should be preserved when a row is scrolled out of view.")]
		[DefaultValue(false)]
		public bool KeepRowSettings
		{
			get
			{
				return _keepRowSettings;
			}
			set
			{
				if (_keepRowSettings != value)
				{
					if (_keepRowSettings)
					{
						ResetDataSourceRowState();
					}
					_keepRowSettings = value;
					if (!_keepRowSettings)
					{
						Refresh();
					}
				}
			}
		}

		[Description("Determines whether the control acts as a bound or unbound control.")]
		[DefaultValue(BoundMode.Bound)]
		public BoundMode BoundMode
		{
			get
			{
				return DataState.ToGridBoundMode(_gridDataSourceManager.BoundMode);
			}
			set
			{
				if (!Enum.IsDefined(typeof(BoundMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (BoundMode != value)
				{
					_gridDataSourceManager.BoundMode = DataState.ComputeBoundMode(value);
					if (BoundMode == BoundMode.Unbound)
					{
						_dataSource = null;
						_dataMember = "";
					}
					SetDataSourceAndMember(_dataSource, _dataMember, true);
				}
			}
		}

		IMordexTable ISupportFilterBuilder.RootTable => RootTable;

		[DefaultValue(RowCheckStateBehavior.CheckIndividualRows)]
		[Description("Determines how rows are checked in a hierarchical grid.")]
		public RowCheckStateBehavior RowCheckStateBehavior
		{
			get
			{
				return DataState.ComputeRowCheckStateBehavior(GetRowSourceManager().CheckedBehavior);
			}
			set
			{
				if (value != RowCheckStateBehavior)
				{
					GetRowSourceManager().CheckedBehavior = DataState.ComputeCheckedBehavior(value);
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Determines whether chec box selectors are shown in group rows.")]
		[DefaultValue(false)]
		public bool UseGroupRowSelector
		{
			get
			{
				return _useGroupRowSelector;
			}
			set
			{
				if (_useGroupRowSelector != value)
				{
					_useGroupRowSelector = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Determines whether a row should be selected when the user clicks in its expand box.")]
		[DefaultValue(true)]
		public bool SelectOnExpand
		{
			get
			{
				return _selectOnExpand;
			}
			set
			{
				_selectOnExpand = value;
			}
		}

		[Description("Determines how the size of column is calculated when its AutoSize method is used.")]
		[DefaultValue(ColumnAutoSizeMode.AllCellsAndHeader)]
		public ColumnAutoSizeMode ColumnAutoSizeMode
		{
			get
			{
				return _columnAutoSizeMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ColumnAutoSizeMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == ColumnAutoSizeMode.Default)
				{
					value = ColumnAutoSizeMode.AllCellsAndHeader;
				}
				if (_columnAutoSizeMode != value)
				{
					_columnAutoSizeMode = value;
				}
			}
		}

		[Description("Indicates whether the control should automatically persist its application settings properties.")]
		[DefaultValue(false)]
		public bool SaveSettings
		{
			get
			{
				return _saveSettings;
			}
			set
			{
				_saveSettings = value;
			}
		}

		[Description("Gets or sets the value of the application settings key for the current instance of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string SettingsKey
		{
			get
			{
				if (string.IsNullOrEmpty(_settingsKey) && base.Name.Length > 0)
				{
					return base.Name;
				}
				return _settingsKey;
			}
			set
			{
				_settingsKey = value;
			}
		}

		[DefaultValue(NewRowEnterKeyBehavior.Default)]
		[Description("Indicates the behavior of the control when the enter key is pressed in the add new row.")]
		public NewRowEnterKeyBehavior NewRowEnterKeyBehavior
		{
			get
			{
				return _newRowEnterKeyBehavior;
			}
			set
			{
				if (!Enum.IsDefined(typeof(NewRowEnterKeyBehavior), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_newRowEnterKeyBehavior = value;
			}
		}

		[DefaultValue(FilterRowUpdateMode.Default)]
		public FilterRowUpdateMode FilterRowUpdateMode
		{
			get
			{
				return _filterRowUpdateMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FilterRowUpdateMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_filterRowUpdateMode = value;
			}
		}

		[DefaultValue(0)]
		public int FilterRowUpdateDelay
		{
			get
			{
				return _filterRowUpdateDelay;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				_filterRowUpdateDelay = value;
			}
		}

		[DefaultValue(FilterConditionOperator.Default)]
		public FilterConditionOperator DefaultFilterRowComparison
		{
			get
			{
				return _defaultFilterRowComparison;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FilterConditionOperator), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_defaultFilterRowComparison = value;
			}
		}

		[DefaultValue(FilterRowButtonStyle.ClearButton)]
		public FilterRowButtonStyle FilterRowButtonStyle
		{
			get
			{
				return _filterRowButtonStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FilterRowButtonStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == FilterRowButtonStyle.Default)
				{
					value = FilterRowButtonStyle.ClearButton;
				}
				if (_filterRowButtonStyle != value)
				{
					_filterRowButtonStyle = value;
					if (FilterMode != FilterMode.None)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[DefaultValue(ContentAlignment.TopCenter)]
		public ContentAlignment EmptyGridInfoAlignment
		{
			get
			{
				return _emptyGridInfoAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ContentAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_emptyGridInfoAlignment != value)
				{
					_emptyGridInfoAlignment = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(null)]
		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public Image TreeExpandGlyph
		{
			get
			{
				return _treeExpandGlyph;
			}
			set
			{
				if (_treeExpandGlyph != value)
				{
					_treeExpandGlyph = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[DefaultValue(ExpandBoxStyle.UseDefault)]
		public ExpandBoxStyle GroupExpandBoxStyle
		{
			get
			{
				return _groupExpandBoxStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ExpandBoxStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_groupExpandBoxStyle != value)
				{
					_groupExpandBoxStyle = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[DefaultValue(ExpandBoxStyle.UseDefault)]
		public ExpandBoxStyle HierarchicalExpandBoxStyle
		{
			get
			{
				return _hierarchicalExpandBoxStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ExpandBoxStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_hierarchicalExpandBoxStyle != value)
				{
					_hierarchicalExpandBoxStyle = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(null)]
		public Image TreeCollapseGlyph
		{
			get
			{
				return _treeCollapseGlyph;
			}
			set
			{
				if (_treeCollapseGlyph != value)
				{
					_treeCollapseGlyph = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public GridSelectedCellsCollection SelectedCells
		{
			get
			{
				if (SelectionMode != SelectionMode.MultipleCellSelection)
				{
					return null;
				}
				GridSelectedCellsCollection gridSelectedCellsCollection = new GridSelectedCellsCollection();
				_gridSelectedItemCollection.Sort();
				foreach (GridSelectedItem item in (IEnumerable)_gridSelectedItemCollection)
				{
					gridSelectedCellsCollection.AddSelectedCells(item);
				}
				return gridSelectedCellsCollection;
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
			}
		}

		public event EventHandler FitColumns
		{
			add
			{
				base.Events.AddHandler(_fitColumnsEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_fitColumnsEventKey, value);
			}
		}

		public event ColumnActionEventHandler ColumnAutoSize
		{
			add
			{
				base.Events.AddHandler(_columnAutoSizeEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_columnAutoSizeEventKey, value);
			}
		}

		public event FetchCellToolTipTextEventHandler FetchCellToolTipText
		{
			add
			{
				base.Events.AddHandler(_fetchCellToolTipTextEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_fetchCellToolTipTextEventKey, value);
			}
		}

		public event EventHandler RootTableChanged
		{
			add
			{
				base.Events.AddHandler(_rootTableChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_rootTableChangedEventKey, value);
			}
		}

		public event RowCheckStateChangingEventHandler RowCheckStateChanging
		{
			add
			{
				base.Events.AddHandler(_rowCheckStateChangingEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_rowCheckStateChangingEventKey, value);
			}
		}

		public event RowCheckStateChangeEventHandler RowCheckStateChanged
		{
			add
			{
				base.Events.AddHandler(_rowCheckStateChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_rowCheckStateChangedEventKey, value);
			}
		}

		public event RowActionCancelEventHandler DeletingRecord
		{
			add
			{
				base.Events.AddHandler(_deletingRecordEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_deletingRecordEventKey, value);
			}
		}

		public event CreateMultipleValueItemEventHandler CreateMultipleValueItem
		{
			add
			{
				base.Events.AddHandler(_createMultipleValueItemEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_createMultipleValueItemEventKey, value);
			}
		}

		public event ParseEditValueEventHandler ParseEditValue
		{
			add
			{
				base.Events.AddHandler(_parseEditValueEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_parseEditValueEventKey, value);
			}
		}

		public event ColumnActionCancelEventHandler ColumnVisibleChanging
		{
			add
			{
				base.Events.AddHandler(_columnVisibleChangingEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_columnVisibleChangingEventKey, value);
			}
		}

		public event ColumnActionEventHandler ColumnVisibleChanged
		{
			add
			{
				base.Events.AddHandler(_columnVisibleChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_columnVisibleChangedEventKey, value);
			}
		}

		public event EventHandler FieldChooserClosed
		{
			add
			{
				base.Events.AddHandler(_fieldChooserClosedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_fieldChooserClosedEventKey, value);
			}
		}

		public event RowActionEventHandler RowDoubleClick
		{
			add
			{
				base.Events.AddHandler(_rowDoubleClickEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_rowDoubleClickEventKey, value);
			}
		}

		public event EventHandler EditModeChanged
		{
			add
			{
				base.Events.AddHandler(_editModeChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_editModeChangedEventKey, value);
			}
		}

		[Description("Event fired when a row is loading the list of one of its child tables")]
		public event GetChildListEventHandler GetChildList
		{
			add
			{
				base.Events.AddHandler(_getChildListEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_getChildListEventKey, value);
			}
		}

		[Description("Event fired when a row is dragged in the control")]
		public event RowDragEventHandler RowDrag
		{
			add
			{
				base.Events.AddHandler(_rowDragEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_rowDragEventKey, value);
			}
		}

		[Description("Event fired when the control can not commit changes in a cell because the text entered by the user does not match the input mask defined for the cell.")]
		public event InputMaskErrorEventHandler InputMaskError
		{
			add
			{
				base.Events.AddHandler(_inputMaskErrorEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_inputMaskErrorEventKey, value);
			}
		}

		[Description("Event fired when the value in the current cell has changed.")]
		public event ColumnActionEventHandler CellValueChanged
		{
			add
			{
				base.Events.AddHandler(_cellValueChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_cellValueChangedEventKey, value);
			}
		}

		[Description("Event fired when the changes in a cell are about to be canceled by the user.")]
		public event ColumnActionCancelEventHandler CancelingCellEdit
		{
			add
			{
				base.Events.AddHandler(_eventKeyCancelingCellEdit, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyCancelingCellEdit, value);
			}
		}

		[Description("Event fired when the changes in a cell are canceled by the user.")]
		public event ColumnActionEventHandler CellEditCanceled
		{
			add
			{
				base.Events.AddHandler(_eventKeyCellEditCanceled, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyCellEditCanceled, value);
			}
		}

		[Description("Event fired when the changes in a row are about to be canceled by the user.")]
		public event RowActionCancelEventHandler CancelingRowEdit
		{
			add
			{
				base.Events.AddHandler(_eventKeyCancelingRowEdit, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyCancelingRowEdit, value);
			}
		}

		[Description("Event fired when the changes in a row are canceled by the user.")]
		public event RowActionEventHandler RowEditCanceled
		{
			add
			{
				base.Events.AddHandler(_eventKeyRowEditCanceled, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyRowEditCanceled, value);
			}
		}
	}
}
