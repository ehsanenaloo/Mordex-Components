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
		[Description("Event fired when the DataGrid control is about to add a new row.")]
		public event GetNewRowEventHandler GetNewRow
		{
			add
			{
				base.Events.AddHandler(_eventKeyGetNewRow, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyGetNewRow, value);
			}
		}

		[Description("Occurs when the DataGrid control is about to add a new row.")]
		public event RowActionEventHandler FirstChange
		{
			add
			{
				base.Events.AddHandler(_eventKeyFirstChange, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyFirstChange, value);
			}
		}

		[Description("Occurs when the RowCount property changes.")]
		public event EventHandler RowCountChanged
		{
			add
			{
				base.Events.AddHandler(_eventKeyRowCountChanged, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyRowCountChanged, value);
			}
		}

		[Description("Occurs each time a GridRow is loaded for display")]
		public event RowLoadEventHandler FormattingRow
		{
			add
			{
				base.Events.AddHandler(_eventKeyFormattingRow, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyFormattingRow, value);
			}
		}

		[Description("Occurs each time a GridRow is loaded.")]
		public event RowLoadEventHandler LoadingRow
		{
			add
			{
				base.Events.AddHandler(_eventKeyLoadingRow, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyLoadingRow, value);
			}
		}

		[Description("Occurs after cell edition ends.")]
		public event ColumnActionEventHandler CellEdited
		{
			add
			{
				base.Events.AddHandler(_eventKeyCellEdited, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyCellEdited, value);
			}
		}

		[Description("Occurs before the user enters edit mode by typing a character or clicking in a cell.")]
		public event EditingCellEventHandler EditingCell
		{
			add
			{
				base.Events.AddHandler(_eventKeyEditingCell, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyEditingCell, value);
			}
		}

		[Description("Occurs before a column is moved by the user.")]
		public event MovingColumnEventHandler MovingColumn
		{
			add
			{
				base.Events.AddHandler(_eventKeyMovingColumn, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyMovingColumn, value);
			}
		}

		[Description("Occurs after a column has been moved by the user.")]
		public event ColumnActionEventHandler ColumnMoved
		{
			add
			{
				base.Events.AddHandler(_eventKeyColumnMoved, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyColumnMoved, value);
			}
		}

		[Description("Occurs after changes in a cell are copied into the row's buffer.")]
		public event ColumnActionEventHandler CellUpdated
		{
			add
			{
				base.Events.AddHandler(_eventKeyCellUpdated, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyCellUpdated, value);
			}
		}

		[Description("Occurs before updating the changes in a cell to the row's buffer")]
		public event UpdatingCellEventHandler UpdatingCell
		{
			add
			{
				base.Events.AddHandler(_eventKeyUpdatingCell, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyUpdatingCell, value);
			}
		}

		[Description("Occurs as the result of an error when updating a row.")]
		public event ErrorEventHandler Error
		{
			add
			{
				base.Events.AddHandler(_eventKeyError, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyError, value);
			}
		}

		[Description("Occurs before one or more rows are about to be deleted.")]
		public event CancelEventHandler DeletingRecords
		{
			add
			{
				base.Events.AddHandler(_eventKeyDeletingRecords, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyDeletingRecords, value);
			}
		}

		[Description("Occurs after one or more records have been deleted")]
		public event EventHandler RecordsDeleted
		{
			add
			{
				base.Events.AddHandler(_eventKeyRecordsDeleted, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyRecordsDeleted, value);
			}
		}

		[Description("Occurs before changes in the groups collection are applied.")]
		public event GroupsChangingEventHandler GroupsChanging
		{
			add
			{
				base.Events.AddHandler(_eventKeyGroupsChanging, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyGroupsChanging, value);
			}
		}

		[Description("Occurs after a change in the groups collection has been commited.")]
		public event GroupsChangedEventHandler GroupsChanged
		{
			add
			{
				base.Events.AddHandler(_eventKeyGroupsChanged, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyGroupsChanged, value);
			}
		}

		[Description("Occurs after a change in the SortKeys collection has been commited.")]
		public event EventHandler SortKeysChanged
		{
			add
			{
				base.Events.AddHandler(_sortKeysChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_sortKeysChangedEventKey, value);
			}
		}

		[Description("Occurs after a record changes in a record have been passed to the datasource.")]
		public event EventHandler RecordUpdated
		{
			add
			{
				base.Events.AddHandler(_eventKeyRecordUpdated, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyRecordUpdated, value);
			}
		}

		[Description("Occurs after a new record has been added to the datasource.")]
		public event EventHandler RecordAdded
		{
			add
			{
				base.Events.AddHandler(_eventKeyRecordAdded, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyRecordAdded, value);
			}
		}

		[Description("Occurs before the changes made by the user are committed to the datasource.")]
		public event CancelEventHandler UpdatingRecord
		{
			add
			{
				base.Events.AddHandler(_eventKeyUpdatingRecord, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyUpdatingRecord, value);
			}
		}

		[Description("Occurs before adding a new record to the data source.")]
		public event CancelEventHandler AddingRecord
		{
			add
			{
				base.Events.AddHandler(_eventKeyAddingRecord, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyAddingRecord, value);
			}
		}

		[Description("Occurs before a column header drag operation begins.")]
		public event ColumnActionCancelEventHandler DraggingColumn
		{
			add
			{
				base.Events.AddHandler(_eventKeyDraggingColumn, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyDraggingColumn, value);
			}
		}

		[Description("Occurs before a group header drag operation begins.")]
		public event GroupActionCancelEventHandler DraggingGroup
		{
			add
			{
				base.Events.AddHandler(_eventKeyDraggingGroup, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyDraggingGroup, value);
			}
		}

		[Description("Occurs before a column set header drag operation begins.")]
		public event ColumnSetActionCancelEventHandler DraggingColumnSet
		{
			add
			{
				base.Events.AddHandler(_eventKeyDraggingColumnSet, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyDraggingColumnSet, value);
			}
		}

		[Description("Occurs when the user resizes cards.")]
		public event SizingCardsEventHandler SizingCards
		{
			add
			{
				base.Events.AddHandler(_eventKeySizingCards, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeySizingCards, value);
			}
		}

		[Description("Occurs when user clicks in a button on a cell.")]
		public event ColumnActionEventHandler ColumnButtonClick
		{
			add
			{
				base.Events.AddHandler(_eventKeyColumnButtonClick, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyColumnButtonClick, value);
			}
		}

		[Description("Occurs when user clicks in the clear button on a filter row cell.")]
		public event ColumnActionEventHandler ClearFilterButtonClick
		{
			add
			{
				base.Events.AddHandler(_eventKeyClearFilterButtonClick, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyClearFilterButtonClick, value);
			}
		}

		[Description("Occurs when the user sizes a column.")]
		public event SizingColumnEventHandler SizingColumn
		{
			add
			{
				base.Events.AddHandler(_eventKeySizingColumn, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeySizingColumn, value);
			}
		}

		[Description("Occurs when the user sizes a ColumnSet.")]
		public event SizingColumnSetEventHandler SizingColumnSet
		{
			add
			{
				base.Events.AddHandler(_eventKeySizingColumnSet, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeySizingColumnSet, value);
			}
		}

		[Description("Occurs when the user clicks in a column header.")]
		public event ColumnActionEventHandler ColumnHeaderClick
		{
			add
			{
				base.Events.AddHandler(_eventKeyColumnHeaderClick, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyColumnHeaderClick, value);
			}
		}

		[Description("Occurs when the user clicks in a group by box header.")]
		public event GroupActionEventHandler GroupByBoxHeaderClick
		{
			add
			{
				base.Events.AddHandler(_eventKeyGroupByBoxHeaderClick, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyGroupByBoxHeaderClick, value);
			}
		}

		[Description("Occurs before a dropdown list or calendar dropdown is shown.")]
		public event ColumnActionEventHandler DropDown
		{
			add
			{
				base.Events.AddHandler(_eventKeyDropDown, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyDropDown, value);
			}
		}

		[Description("Occurs when the user scroll the DataGrid control.")]
		public new event EventHandler Scroll
		{
			add
			{
				base.Events.AddHandler(_eventKeyScroll, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyScroll, value);
			}
		}

		[Description("Occurs when a dropdown list or calendar dropdown is hidden.")]
		public event DropDownHideEventHandler DropDownHide
		{
			add
			{
				base.Events.AddHandler(_eventKeyDropDownHide, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyDropDownHide, value);
			}
		}

		[Description("Occurs when a value typed by the user is not present in the ValueList or DropDown associated with a column.")]
		public event NotInListEventHandler NotInList
		{
			add
			{
				base.Events.AddHandler(_eventKeyNotInList, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyNotInList, value);
			}
		}

		[Description("Occurs after a cell is selected.")]
		public event EventHandler CurrentCellChanged
		{
			add
			{
				base.Events.AddHandler(_eventKeyCurrentCellChanged, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyCurrentCellChanged, value);
			}
		}

		[Description("Occurs before a cell is selected.")]
		public event CurrentCellChangingEventHandler CurrentCellChanging
		{
			add
			{
				base.Events.AddHandler(_eventKeyCurrentCellChanging, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyCurrentCellChanging, value);
			}
		}

		[Description("Occurs when the selected row(s) change.")]
		public event EventHandler SelectionChanged
		{
			add
			{
				base.Events.AddHandler(_eventKeySelectionChanged, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeySelectionChanged, value);
			}
		}

		[Description("Occurs before the rows in the control are filtered.")]
		public event CancelEventHandler ApplyingFilter
		{
			add
			{
				base.Events.AddHandler(_eventKeyApplyingFilter, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyApplyingFilter, value);
			}
		}

		[Description("Occurs after filter has been applied.")]
		public event EventHandler FilterApplied
		{
			add
			{
				base.Events.AddHandler(_eventKeyFilterApplied, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyFilterApplied, value);
			}
		}

		[Description("Occurs after a layout is loaded.")]
		public event EventHandler LayoutLoad
		{
			add
			{
				base.Events.AddHandler(_eventKeyLayoutLoad, value);
			}
			remove
			{
				base.Events.AddHandler(_eventKeyLayoutLoad, value);
			}
		}

		[Description("Occurs after the CurrentLayout property has changed.")]
		public event EventHandler CurrentLayoutChanged
		{
			add
			{
				base.Events.AddHandler(_eventKeyCurrentLayoutChanged, value);
			}
			remove
			{
				base.Events.AddHandler(_eventKeyCurrentLayoutChanged, value);
			}
		}

		[Description("Occurs before the CurrentLayout property changes")]
		public event CancelEventHandler CurrentLayoutChanging
		{
			add
			{
				base.Events.AddHandler(_eventKeyCurrentLayoutChanging, value);
			}
			remove
			{
				base.Events.AddHandler(_eventKeyCurrentLayoutChanging, value);
			}
		}

		[Description("Occurs when a link column is clicked.")]
		public event ColumnActionEventHandler LinkClicked
		{
			add
			{
				base.Events.AddHandler(_eventKeyLinkClicked, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyLinkClicked, value);
			}
		}

		[Description("Occurs when an owner drawn area needs to be painted.")]
		public event DrawGridAreaEventHandler DrawGridArea
		{
			add
			{
				base.Events.AddHandler(_eventKeyDrawGridArea, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyDrawGridArea, value);
			}
		}

		[Description("Occurs when a custom edit column is about to be edited.")]
		public event InitCustomEditEventHandler InitCustomEdit
		{
			add
			{
				base.Events.AddHandler(_eventKeyInitCustomEdit, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyInitCustomEdit, value);
			}
		}

		[Description("Occurs when edition in a custom edit column ends.")]
		public event EndCustomEditEventHandler EndCustomEdit
		{
			add
			{
				base.Events.AddHandler(_eventKeyEndCustomEdit, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyEndCustomEdit, value);
			}
		}

		[Description("Occurs before a row is expanded.")]
		public event RowActionCancelEventHandler ExpandingRow
		{
			add
			{
				base.Events.AddHandler(_eventKeyExpandingRow, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyExpandingRow, value);
			}
		}

		[Description("Occurs before a row is collapsed.")]
		public event RowActionCancelEventHandler CollapsingRow
		{
			add
			{
				base.Events.AddHandler(_eventKeyCollapsingRow, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyCollapsingRow, value);
			}
		}

		[Description("Occurs after a row is expanded.")]
		public event RowActionEventHandler RowExpanded
		{
			add
			{
				base.Events.AddHandler(_eventKeyRowExpanded, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyRowExpanded, value);
			}
		}

		[Description("Occurs after a row is collapsed.")]
		public event RowActionEventHandler RowCollapsed
		{
			add
			{
				base.Events.AddHandler(_eventKeyRowCollapsed, value);
			}
			remove
			{
				base.Events.RemoveHandler(_eventKeyRowCollapsed, value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new event EventHandler AutoSizeChanged
		{
			add
			{
			}
			remove
			{
			}
		}
	}
}
