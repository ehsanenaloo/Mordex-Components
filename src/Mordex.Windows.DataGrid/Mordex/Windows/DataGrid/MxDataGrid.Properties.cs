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
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		protected CurrencyManager CurrencyManager => _currencyManager;

		[Description("The name of a list in a data source for which the control displays its data.")]
		[Category("Data")]
		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(UITypeEditor))]
		public string DataMember
		{
			get
			{
				return _dataMember;
			}
			set
			{
				if (value != null && value.Length > 0 && BoundMode == BoundMode.Unbound)
				{
					throw new InvalidOperationException("DataMember can not be set in an unbound grid.");
				}
				string text = _dataMember;
				try
				{
					SetDataSourceAndMember(_dataSource, value, false);
					if (_currentLayout != null)
					{
						_currentLayout._dataSource = _dataSource;
						_currentLayout._dataMember = _dataMember;
					}
				}
				catch (Exception)
				{
					_dataMember = text;
				}
			}
		}

		[Description("The source of data for the DataGrid control.")]
		[Category("Data")]
		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
		[AttributeProvider(typeof(IListSource))]
		public object DataSource
		{
			get
			{
				return _dataSource;
			}
			set
			{
				if (value != null && BoundMode == BoundMode.Unbound)
				{
					throw new InvalidOperationException("DataSource can not be set in an unbound grid.");
				}
				if (_dataSource != value)
				{
					if (_dataSource is IComponent)
					{
						((IComponent)_dataSource).Disposed -= OnDataSourceDisposed;
					}
					if (value == null)
					{
						_dataMember = "";
					}
					SetDataSourceAndMember(value, _dataMember, false);
					if (_dataSource is IComponent)
					{
						((IComponent)_dataSource).Disposed += OnDataSourceDisposed;
					}
					if (_currentLayout != null)
					{
						_currentLayout._dataSource = _dataSource;
						_currentLayout._dataMember = _dataMember;
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridTable RootTable
		{
			get
			{
				return (GridTable)_gridDataSourceManager.RootTable;
			}
			set
			{
				ReplaceRootTable(value, true);
			}
		}

		[Description("The collection of GridFormatStyle objects used in the DataGrid control.")]
		[Browsable(false)]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridFormatStyleCollection FormatStyles
		{
			get
			{
				return _formatStyleCollection;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_formatStyleCollection._owner = null;
				_formatStyleCollection = value;
				_formatStyleCollection._owner = this;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		[Browsable(false)]
		public GridLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.ComputeDataGrid() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the control");
				}
				if (value != null && !value.HasFullLayout())
				{
					throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
				}
				if (!UpdateData())
				{
					DataChanged = false;
				}
				if (_currentLayout == value)
				{
					return;
				}
				CancelEventArgs e = new CancelEventArgs(cancel: false);
				OnCurrentLayoutChanging(e);
				if (!e.Cancel)
				{
					if (_currentLayout != null)
					{
						_currentLayout.ProcessDataGrid(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.ProcessDataGrid(this);
					}
					if (!_isInitializing)
					{
						ApplyCurrentLayout();
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridLayoutCollection Layouts => _layouts;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public GridLayout DesignTimeLayout
		{
			get
			{
				if (ShouldSerializeDesignTimeLayout())
				{
					GridLayout gridLayout = new GridLayout();
					if (this is GridDropDown)
					{
						gridLayout.UpdatePersistenceMode(PersistenceMode.All);
					}
					else
					{
						gridLayout.UpdatePersistenceMode(PersistenceMode.TableStructure | PersistenceMode.DropDowns | PersistenceMode.FormatStyles | PersistenceMode.InternalImageList);
					}
					LayoutWriter mordexLayoutWriter = new LayoutWriter();
					WriteLayoutByMode(mordexLayoutWriter, gridLayout.GetPersistenceMode());
					LayoutWriter.StoreInLayout("GridLayoutData", mordexLayoutWriter, gridLayout);
					gridLayout.DataMember = "";
					gridLayout.DataSource = null;
					gridLayout._owner = this;
					return gridLayout;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					if (this is GridDropDown)
					{
						value.UpdatePersistenceMode(PersistenceMode.All);
					}
					else
					{
						value.UpdatePersistenceMode(PersistenceMode.TableStructure | PersistenceMode.DropDowns | PersistenceMode.FormatStyles | PersistenceMode.InternalImageList);
					}
				}
				if (value != null && !string.IsNullOrEmpty(value.LayoutString))
				{
					LoadLayout(value);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the GridWaterImage that oontains the information used to draw a watermark image in the background.")]
		public GridWatermarkImage WatermarkImage => _watermarkImage;

		[Category("Appearance")]
		[DefaultValue(View.TableView)]
		[Description("Determines how DataGrid control presents the records.")]
		public View View
		{
			get
			{
				return _view;
			}
			set
			{
				if (!Enum.IsDefined(typeof(View), value))
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value == _view)
				{
					return;
				}
				UpdateData();
				if (value != View.TableView)
				{
					HideFieldChooser();
					if (RootTable != null)
					{
						RootTable.Groups.Clear();
					}
					if (_cardRowLayouts == null)
					{
						_cardRowLayouts = new TableLayoutCollection();
					}
				}
				_view = value;
				_horizontalScrollOffset = 0;
				_verticalScrollOffset = 0;
				if (RootTable != null)
				{
					RootTable.ColumnSets.RecalculateColumnPositions();
				}
				RefreshDataSourceView();
				MarkTablesForAutoResize();
				RefreshRowCountAndInvalidate();
				if (_view != View.TableView && _row != null && _row.Table != RootTable)
				{
					Row = 0;
				}
				_wasTableView = _view == View.TableView;
				EnsureVisible();
				if (_tableAccessibleObject != null)
				{
					_tableAccessibleObject.NotifyClients(AccessibleEvents.Reorder);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public int HorizontalScrollPosition
		{
			get
			{
				if (_view == View.TableView)
				{
					return GetHorizontalScrollOffset();
				}
				return FirstRow;
			}
			set
			{
				if (_view == View.TableView)
				{
					SetHorizontalScrollOffset(value);
				}
				else
				{
					FirstRow = value;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public int VerticalScrollPosition
		{
			get
			{
				if (_view == View.TableView)
				{
					return FirstRow;
				}
				return GetVerticalScrollOffset();
			}
			set
			{
				if (_view == View.TableView)
				{
					FirstRow = value;
				}
				else
				{
					SetVerticalScrollOffset(value);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int FirstRow
		{
			get
			{
				return _firstRow;
			}
			set
			{
				if (value > RowCount - 1)
				{
					value = RowCount - 1;
				}
				if (value < 0)
				{
					value = 0;
				}
				if (value == _firstRow)
				{
					return;
				}
				if (_view == View.TableView)
				{
					if (!TryScrollRowsToFirstRow(_firstRow, value))
					{
						SetFirstVisibleRow(value);
						RefreshRowCountAndInvalidate();
						CalculateLayoutIfReady();
					}
				}
				else
				{
					SetFirstVisibleRow(value);
					RefreshRowCountAndInvalidate();
					CalculateLayoutIfReady();
				}
				OnScroll(EventArgs.Empty);
			}
		}

		[DefaultValue(8)]
		public int TableViewHorizontalScrollIncrement
		{
			get
			{
				return _tableViewHorizontalScrollIncrement;
			}
			set
			{
				if (_tableViewHorizontalScrollIncrement < -1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (_tableViewHorizontalScrollIncrement != value)
				{
					_tableViewHorizontalScrollIncrement = value;
					if (View == View.TableView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		public override bool Focused => base.ContainsFocus;

		[DefaultValue(FocusCellDisplayMode.UseFocusCellFormatStyle)]
		public FocusCellDisplayMode FocusCellDisplayMode
		{
			get
			{
				return _focusCellDisplayMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FocusCellDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_focusCellDisplayMode != value)
				{
					_focusCellDisplayMode = value;
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("'GroupByBoxInfoText' will be removed in future versions. Use DataGrid.BuiltInTexts(GridBuiltInText.GroupByBoxInfo) instead.")]
		[Browsable(false)]
		public string GroupByBoxInfoText
		{
			get
			{
				return BuiltInTexts[GridBuiltInText.GroupByBoxInfo];
			}
			set
			{
				BuiltInTexts[GridBuiltInText.GroupByBoxInfo] = value;
			}
		}

		[Category("Table View")]
		[Description("Determines whether the 'Group By Box' should be drawn.")]
		[DefaultValue(true)]
		public bool GroupByBoxVisible
		{
			get
			{
				return _groupByBoxVisible;
			}
			set
			{
				if (value != _groupByBoxVisible)
				{
					_groupByBoxVisible = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("The distance, in pixels, to indent each of the child tables.")]
		[Category("Table View")]
		[DefaultValue(18)]
		public int Indent
		{
			get
			{
				return _indent;
			}
			set
			{
				if (value < 9)
				{
					throw new ArgumentOutOfRangeException("Indent property must be greater than or equal to 9.");
				}
				if (value != _indent)
				{
					_indent = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("The distance, in pixels, to indent group levels.")]
		[DefaultValue(16)]
		[Category("Table View")]
		public int GroupIndent
		{
			get
			{
				return _groupIndent;
			}
			set
			{
				if (value < 9)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _groupIndent)
				{
					_groupIndent = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Determines how the user can select rows in the control.")]
		[Category("Behavior")]
		[DefaultValue(SelectionMode.SingleSelection)]
		public SelectionMode SelectionMode
		{
			get
			{
				if (_selectionMode == SelectionMode.MultipleCellSelection)
				{
					if (View != View.TableView)
					{
						return SelectionMode.MultipleSelectionSameTable;
					}
					if (UsesColumnSets(RootTable))
					{
						return SelectionMode.MultipleSelectionSameTable;
					}
				}
				return _selectionMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(SelectionMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _selectionMode)
				{
					_selectionMode = value;
				}
			}
		}

		[DefaultValue(CellSelectionMode.EntireRow)]
		[Description("Determines how the selected cell is highlighted.")]
		[Category("Behavior")]
		public CellSelectionMode CellSelectionMode
		{
			get
			{
				if (SelectionMode == SelectionMode.MultipleCellSelection)
				{
					return CellSelectionMode.SingleCell;
				}
				return _cellSelectionMode;
			}
			set
			{
				if (_cellSelectionMode != value)
				{
					_cellSelectionMode = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Indicates whether tree lines should be drawn in hierarchical mode.")]
		[Category("Appearance")]
		[DefaultValue(true)]
		public bool TreeLines
		{
			get
			{
				return _treeLines;
			}
			set
			{
				if (value != _treeLines)
				{
					_treeLines = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("The color of tree lines in hierarchical mode.")]
		[Category("Appearance")]
		public Color TreeLineColor
		{
			get
			{
				return _treeLineColor;
			}
			set
			{
				if (value != _treeLineColor)
				{
					_treeLineColor = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(false)]
		[Description("Indicates whether the control should display hierarchical tables.")]
		public bool Hierarchical
		{
			get
			{
				return _hierarchical;
			}
			set
			{
				if (value != _hierarchical)
				{
					_hierarchical = value;
					CollapseRecords();
					if (_gridFieldChooser != null)
					{
						ShowFieldChooser();
					}
					MarkTablesForAutoResize();
				}
			}
		}

		[Description("The ImageList that contains the images used in the control.")]
		[Category("Appearance")]
		[DefaultValue(null)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("ExternalImageList is no longer supported. Use ImageList property instead.")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ImageList ExternalImageList
		{
			get
			{
				return _externalImageList;
			}
			set
			{
				if (value != _externalImageList)
				{
					_externalImageList = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(null)]
		[Description("The ImageList that contains the images used in the control.")]
		public ImageList ImageList
		{
			get
			{
				return _externalImageList;
			}
			set
			{
				if (value != _externalImageList)
				{
					_externalImageList = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Browsable(false)]
		public GridRow FilterRow => GetFilterGridRow();

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Row
		{
			get
			{
				if (_row != null)
				{
					return _row.Position;
				}
				return -3;
			}
			set
			{
				if (!IsValidRowPosition(value) && value != -3)
				{
					throw new ArgumentOutOfRangeException();
				}
				_isCurrentCellChanging = true;
				IsRowFocusableInCurrentColumn(value);
				SyncCellSelectionWithCurrent();
				_isCurrentCellChanging = false;
				EnsureVisible();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public int Col
		{
			get
			{
				if (_currentColumn == null)
				{
					return -1;
				}
				return _currentColumn.Position;
			}
			set
			{
				if (value < -1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value == Col)
				{
					return;
				}
				if (value == -1)
				{
					_currentColumn = null;
				}
				else
				{
					if (CurrentTable == null)
					{
						return;
					}
					GridColumn gridColumn = GetLastEditableColumn(_row);
					if (gridColumn == null)
					{
						return;
					}
					if (value < gridColumn.Position)
					{
						GridColumn columnInPosition = CurrentTable.Columns.GetColumnInPosition(value);
						if (IsCellSelectable(_row, columnInPosition))
						{
							CurrentColumn = columnInPosition;
						}
						else
						{
							Col = value + 1;
						}
					}
					else
					{
						CurrentColumn = gridColumn;
					}
				}
			}
		}

		[Browsable(false)]
		public GridTable CurrentTable
		{
			get
			{
				if (_row == null)
				{
					return RootTable;
				}
				return _row.Table;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridColumn CurrentColumn
		{
			get
			{
				return _currentColumn;
			}
			set
			{
				if (value == _currentColumn)
				{
					return;
				}
				if (_row != null && value != null && _row.Table != value.Table)
				{
					Col = value.Position;
					return;
				}
				CurrentCellChangingEventArgs e = new CurrentCellChangingEventArgs(_row, value, cancel: false);
				if (!_isEditCancelling)
				{
					OnCurrentCellChanging(e);
				}
				if (!e.Cancel && AcceptCellEdit())
				{
					_currentColumn = value;
					if (_row != null && _row.RowType != RowType.GroupHeader)
					{
						EnsureVisible();
					}
					SyncCellSelectionWithCurrent();
					InvalidateGridRowLayout(_row);
					if (_autoEdit && base.ContainsFocus)
					{
						BeginCellEdit(false, '\0', false);
					}
					if (!_isEditCancelling)
					{
						OnCurrentCellChanged(EventArgs.Empty);
					}
				}
			}
		}

		[Browsable(false)]
		public GridTableCollection Tables => _gridTableCollection;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridSelectedItemCollection SelectedItems => _gridSelectedItemCollection;

		AccessibleObject IGridDropListOwner.AccessibleObject => base.AccessibilityObject;

		[Description("Indicates whether the Error event should be raised when an exception occurs.")]
		[DefaultValue(true)]
		public bool EventErrorHandling
		{
			get
			{
				return _eventErrorHandling;
			}
			set
			{
				_eventErrorHandling = value;
			}
		}

		[Description("Indicates whether the control should enter into edit mode when a cell is selected.")]
		[Category("Behavior")]
		[DefaultValue(false)]
		public bool AutoEdit
		{
			get
			{
				return _autoEdit;
			}
			set
			{
				if (value != _autoEdit)
				{
					_autoEdit = value;
					if (_autoEdit && _gridColumnContainerControl == null)
					{
						BeginCellEdit(false, '\0', false);
					}
				}
			}
		}

		[Description("The width, in pixels, of cards.")]
		[Category("Card View")]
		[DefaultValue(250)]
		public int CardWidth
		{
			get
			{
				if (_view == View.SingleCard)
				{
					return base.ClientRectangle.Width - CardSpacing * 2;
				}
				return _cardWidth;
			}
			set
			{
				if (value != _cardWidth)
				{
					_cardWidth = value;
					if (View == View.CardView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[DefaultValue("")]
		[Description("Text used as prefix in card captions.")]
		[Localizable(true)]
		[Category("Card View")]
		public string CardCaptionPrefix
		{
			get
			{
				return _cardCaptionPrefix;
			}
			set
			{
				if (value != _cardCaptionPrefix)
				{
					_cardCaptionPrefix = value;
					if (_view == View.CardView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[DefaultValue(12)]
		[Description("The space, in pixels, between cards.")]
		[Category("Card View")]
		public int CardSpacing
		{
			get
			{
				return _cardSpacing;
			}
			set
			{
				if (value != _cardSpacing)
				{
					_cardSpacing = value;
					if (_view != View.TableView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[DefaultValue(0)]
		[Description("Specifies the space between fields inside a card.")]
		public int CardInnerSpacing
		{
			get
			{
				return _cardInnerSpacing;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (_cardInnerSpacing != value)
				{
					_cardInnerSpacing = value;
					if (_view != View.TableView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Category("Card View")]
		[DefaultValue(true)]
		[Description("Indicates whether cards have borders.")]
		public bool CardBorders
		{
			get
			{
				return _cardBorders;
			}
			set
			{
				if (value != _cardBorders)
				{
					_cardBorders = value;
					if (_view != View.TableView)
					{
						Invalidate();
					}
				}
			}
		}

		[DefaultValue(CardViewGridlines.None)]
		[Description("Specfies how grid lines are drawn inside a card.")]
		[Category("Card View")]
		public CardViewGridlines CardViewGridlines
		{
			get
			{
				return _cardViewGridlines;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CardViewGridlines), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_cardViewGridlines != value)
				{
					_cardViewGridlines = value;
					if (_view != View.TableView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Category("Card View")]
		[Description("Indicates whether empty fields should be shown in a card.")]
		[DefaultValue(true)]
		public bool ShowEmptyFields
		{
			get
			{
				return _showEmptyFields;
			}
			set
			{
				if (value != _showEmptyFields)
				{
					_showEmptyFields = value;
					if (_view == View.CardView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Description("Indicates whether the control should sort rows when user clicks in a column header.")]
		[Category("Behavior")]
		[DefaultValue(true)]
		public bool AutomaticSort
		{
			get
			{
				return _automaticSort;
			}
			set
			{
				if (value != _automaticSort)
				{
					_automaticSort = value;
				}
			}
		}

		[Category("Card View")]
		[DefaultValue(true)]
		[Description("Indicates whether the user can resize cards.")]
		public bool AllowCardSizing
		{
			get
			{
				return _allowCardSizing;
			}
			set
			{
				if (value != _allowCardSizing)
				{
					_allowCardSizing = value;
					if (_view == View.CardView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Description("Indicates whether cards can be expanded and collapsed")]
		[Category("Card View")]
		[DefaultValue(true)]
		public bool ExpandableCards
		{
			get
			{
				return _expandableCards;
			}
			set
			{
				if (value != _expandableCards)
				{
					_expandableCards = value;
					if (!_expandableCards)
					{
						ExpandCards();
					}
					if (_view != View.TableView)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Category("Card View")]
		[Description("Indicates whether card caption bars are shown.")]
		[DefaultValue(true)]
		public bool CardHeaders
		{
			get
			{
				return _cardHeaders;
			}
			set
			{
				if (value != _cardHeaders)
				{
					_cardHeaders = value;
					if (_view != View.SingleCard)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Description("Indicates how selected items will be displayed when the DataGrid control is not focused.")]
		[Category("Appearance")]
		[DefaultValue(HideSelection.HideSelection)]
		public HideSelection HideSelection
		{
			get
			{
				return _hideSelection;
			}
			set
			{
				if (!Enum.IsDefined(typeof(HideSelection), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _hideSelection)
				{
					_hideSelection = value;
					if (!base.ContainsFocus)
					{
						InvalidateSelectedRows();
					}
				}
			}
		}

		[Category("Appearance")]
		[Description("Indicates how the background image should be drawn.")]
		[DefaultValue(BackgroundImageDrawMode.Center)]
		public BackgroundImageDrawMode BackgroundImageDrawMode
		{
			get
			{
				return _backgroundImageDrawMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BackgroundImageDrawMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backgroundImageDrawMode)
				{
					_backgroundImageDrawMode = value;
					Invalidate();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new ImageLayout BackgroundImageLayout
		{
			get
			{
				return base.BackgroundImageLayout;
			}
			set
			{
			}
		}

		[Description("Indicates whether the control should paint every other row using the AlternatingRowFormatStyle settings")]
		[Category("Appearance")]
		[DefaultValue(false)]
		public bool AlternatingColors
		{
			get
			{
				return _alternatingColors;
			}
			set
			{
				if (value != _alternatingColors)
				{
					_alternatingColors = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}
	}
}
