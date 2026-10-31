using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridDropDown
	{
		string IMordexXmlLayoutsSupport.Identifier => Key;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		[Browsable(false)]
		public GridDropDownCollection DropDownCollection => _ownerCollection;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new ControlStyle EditorsControlStyle => base.EditorsControlStyle;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				base.Visible = value;
			}
		}

		[DefaultValue(8)]
		[Description("Indicates the number of rows that are visible is the drop down list.")]
		public int VisibleRows
		{
			get
			{
				return _visibleRows;
			}
			set
			{
				if (value != _visibleRows)
				{
					_visibleRows = value;
				}
			}
		}

		[DefaultValue("")]
		[Description("Specifies the property of the data source from which to draw the value.")]
		public string ValueMember
		{
			get
			{
				return _valueMember;
			}
			set
			{
				if (value != _valueMember)
				{
					_valueMember = value;
					SetHashtable();
				}
			}
		}

		[DefaultValue("")]
		[Description("Specifies the property of the data source whose contents you want to display.")]
		public string DisplayMember
		{
			get
			{
				return _displayMember;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (value != _displayMember)
				{
					_displayMember = value;
					SetHashtable();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool ReplaceValues
		{
			get
			{
				return _replaceValues;
			}
			set
			{
				if (value != _replaceValues)
				{
					_replaceValues = value;
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new InheritableBoolean AllowAddNew
		{
			get
			{
				return base.AllowAddNew;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool AllowCardSizing
		{
			get
			{
				return base.AllowCardSizing;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new InheritableBoolean AllowDelete
		{
			get
			{
				return base.AllowDelete;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new InheritableBoolean AllowEdit
		{
			get
			{
				return base.AllowEdit;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new InheritableBoolean AllowRemoveColumns
		{
			get
			{
				return base.AllowRemoveColumns;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool AutoEdit
		{
			get
			{
				return base.AutoEdit;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool AutomaticSort
		{
			get
			{
				return base.AutomaticSort;
			}
			set
			{
				base.AutomaticSort = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string CalendarNoneText
		{
			get
			{
				return "";
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string CalendarTodayText
		{
			get
			{
				return "";
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool CardBorders
		{
			get
			{
				return base.CardBorders;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new GridFormatStyle CardCaptionFormatStyle => base.CardCaptionFormatStyle;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string CardCaptionPrefix
		{
			get
			{
				return base.CardCaptionPrefix;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridFormatStyle CardColumnHeaderFormatStyle => base.CardColumnHeaderFormatStyle;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool CardHeaders
		{
			get
			{
				return base.CardHeaders;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new int CardSpacing
		{
			get
			{
				return base.CardSpacing;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new int CardWidth
		{
			get
			{
				return base.CardWidth;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new ColumnSetNavigation ColumnSetNavigation
		{
			get
			{
				return base.ColumnSetNavigation;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool DataChanged
		{
			get
			{
				return base.DataChanged;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridDropDownCollection DropDowns => base.DropDowns;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool EmptyRows
		{
			get
			{
				return base.EmptyRows;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool EventErrorHandling
		{
			get
			{
				return base.EventErrorHandling;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool ExpandableCards
		{
			get
			{
				return base.ExpandableCards;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new FilterMode FilterMode
		{
			get
			{
				return base.FilterMode;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridFormatStyle FilterRowFormatStyle => base.FilterRowFormatStyle;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new GridFormatStyle GroupByBoxFormatStyle => base.GroupByBoxFormatStyle;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new GridFormatStyle GroupByBoxInfoFormatStyle => base.GroupByBoxInfoFormatStyle;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool GroupByBoxVisible
		{
			get
			{
				return base.GroupByBoxVisible;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new int GroupIndent
		{
			get
			{
				return base.GroupIndent;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new GroupMode GroupMode
		{
			get
			{
				return base.GroupMode;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new GridFormatStyle GroupTotalRowFormatStyle => base.GroupTotalRowFormatStyle;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new GroupTotals GroupTotals
		{
			get
			{
				return base.GroupTotals;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new InheritableBoolean HideColumnsWhenGrouped
		{
			get
			{
				return base.HideColumnsWhenGrouped;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new HideSelection HideSelection
		{
			get
			{
				return base.HideSelection;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool Hierarchical
		{
			get
			{
				return base.Hierarchical;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new int Indent
		{
			get
			{
				return base.Indent;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridFormatStyle NewRowFormatStyle
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new NewRowPosition NewRowPosition
		{
			get
			{
				return base.NewRowPosition;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool RecordNavigator
		{
			get
			{
				return base.RecordNavigator;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string RecordNavigatorText
		{
			get
			{
				return "";
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new InheritableBoolean RepeatHeaders
		{
			get
			{
				return base.RepeatHeaders;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridFormatStyle SelectedInactiveFormatStyle => base.SelectedInactiveFormatStyle;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new SelectionMode SelectionMode
		{
			get
			{
				return base.SelectionMode;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool ShowEmptyFields
		{
			get
			{
				return base.ShowEmptyFields;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new GridTable RootTable
		{
			get
			{
				return base.RootTable;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridTableCollection Tables => base.Tables;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new int TableSpacing
		{
			get
			{
				return base.TableSpacing;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new InheritableBoolean TotalRow
		{
			get
			{
				return base.TotalRow;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridFormatStyle TotalRowFormatStyle => base.TotalRowFormatStyle;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color TreeLineColor
		{
			get
			{
				return base.TreeLineColor;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DefaultValue(IncrementalSearchMode.None)]
		public new IncrementalSearchMode IncrementalSearchMode
		{
			get
			{
				return base.IncrementalSearchMode;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[DefaultValue(null)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridColumn SearchColumn
		{
			get
			{
				return base.SearchColumn;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool TreeLines
		{
			get
			{
				return base.TreeLines;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new View View
		{
			get
			{
				return base.View;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new InheritableBoolean ExpandableGroups
		{
			get
			{
				return base.ExpandableGroups;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new ControlStyle ControlStyle => base.ControlStyle;

		[Browsable(false)]
		public GridColumnCollection Columns => RootTable.Columns;

		[Browsable(false)]
		public GridSortKeyCollection SortKeys => base.RootTable.SortKeys;

		[Browsable(false)]
		public GridFormatConditionCollection FormatConditions => base.RootTable.FormatConditions;

		[Description("Indicates how coluwns will be rendered.")]
		[DefaultValue(CellLayoutMode.UseColumns)]
		public CellLayoutMode CellLayoutMode
		{
			get
			{
				return base.RootTable.CellLayoutMode;
			}
			set
			{
				base.RootTable.CellLayoutMode = value;
			}
		}

		[Browsable(false)]
		public GridColumnSetCollection ColumnSets => base.RootTable.ColumnSets;

		[DefaultValue(1)]
		[Description("Indicates how many lines tall will be the column set headers.")]
		public int ColumnSetHeaderLines
		{
			get
			{
				return base.RootTable.ColumnSetHeaderLines;
			}
			set
			{
				base.RootTable.ColumnSetHeaderLines = value;
			}
		}

		[Description("Indicates how many row levels will the dropdown have in a column set layout.")]
		[DefaultValue(2)]
		public int ColumnSetRowCount
		{
			get
			{
				return base.RootTable.ColumnSetRowCount;
			}
			set
			{
				base.RootTable.ColumnSetRowCount = value;
			}
		}

		[DefaultValue(1)]
		[Description("Indicates how many lines tall the column headers are.")]
		public int HeaderLines
		{
			get
			{
				return base.RootTable.HeaderLines;
			}
			set
			{
				base.RootTable.HeaderLines = value;
			}
		}

		[Description("The width of the row headers.")]
		[DefaultValue(18)]
		public int RowHeaderWidth
		{
			get
			{
				return base.RootTable.RowHeaderWidth;
			}
			set
			{
				base.RootTable.RowHeaderWidth = value;
			}
		}

		[DefaultValue(-1)]
		[Description("Indicates the height of the rows.")]
		public int RowHeight
		{
			get
			{
				return base.RootTable.RowHeight;
			}
			set
			{
				base.RootTable.RowHeight = value;
			}
		}

		[Browsable(false)]
		public GridGroupCollection Groups => base.RootTable.Groups;

		[Description("The key used to identify the object in a colletion.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new object DataSource
		{
			get
			{
				return base.DataSource;
			}
			set
			{
				base.DataSource = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string DataMember
		{
			get
			{
				return base.DataMember;
			}
			set
			{
				base.DataMember = value;
			}
		}

		[Description("Represents the GridColumn object that contains the image to associote with a value.")]
		[DefaultValue(null)]
		[Editor("Mordex.Windows.DataGrid.Design.ColumnEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		public GridColumn ImageColumn
		{
			get
			{
				return _imageColumn;
			}
			set
			{
				if (value != null && value.GetOwnerColumnCollection() != Columns)
				{
					throw new InvalidOperationException("Column must belong to the Columns collection of the control.");
				}
				_imageColumn = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool AcceptsEscape
		{
			get
			{
				return base.AcceptsEscape;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool CenterSingleCard
		{
			get
			{
				return base.CenterSingleCard;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool DelayEditionOnClick
		{
			get
			{
				return base.DelayEditionOnClick;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new CellSelectionMode CellSelectionMode
		{
			get
			{
				return base.CellSelectionMode;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new EnterKeyBehavior EnterKeyBehavior
		{
			get
			{
				return base.EnterKeyBehavior;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string GroupByBoxInfoText
		{
			get
			{
				return "";
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool HasEditorsControlStyle
		{
			get
			{
				return base.HasEditorsControlStyle;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new GridFormatStyle RowWithErrorsFormatStyle
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new TabKeyBehavior TabKeyBehavior
		{
			get
			{
				return base.TabKeyBehavior;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool UpdateOnLeave
		{
			get
			{
				return base.UpdateOnLeave;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether to display preview rows.")]
		public bool PreviewRow
		{
			get
			{
				return base.RootTable.PreviewRow;
			}
			set
			{
				base.RootTable.PreviewRow = value;
			}
		}

		[Description("Specifies the distance to indent the text in the preview row.")]
		[DefaultValue(16)]
		public int PreviewRowLeftMargin
		{
			get
			{
				return base.RootTable.PreviewRowLeftMargin;
			}
			set
			{
				base.RootTable.PreviewRowLeftMargin = value;
			}
		}

		[Description("Specifies the number of lines to be displayed in preview rows.")]
		[DefaultValue(3)]
		public int PreviewRowLines
		{
			get
			{
				return base.RootTable.PreviewRowLines;
			}
			set
			{
				base.RootTable.PreviewRowLines = value;
			}
		}

		[Description("Specifies the property of the data source whose contents you want to display in the preview row.")]
		[DefaultValue("")]
		public string PreviewRowMember
		{
			get
			{
				return base.RootTable.PreviewRowMember;
			}
			set
			{
				base.RootTable.PreviewRowMember = value;
			}
		}

		bool ILookUpTable.ReplaceValues => _replaceValues;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Parent = IntPtr.Zero;
				createParams.ExStyle |= 128;
				return createParams;
			}
		}
	}
}
