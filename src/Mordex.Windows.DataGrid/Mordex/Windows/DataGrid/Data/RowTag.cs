using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Data
{
	internal class RowTag
	{
		private bool _isExpanded;

		private bool _dataState3Boolean;

		private GridFormatStyleState _gridFormatStyleState;

		private RowCellSelection _mordexRowState;

		internal RowTag()
		{
			_isExpanded = true;
			_dataState3Boolean = false;
		}

		internal bool GetDataState3Boolean()
		{
			return _dataState3Boolean;
		}

		internal void SetDataState3Boolean(bool dataState3Boolean)
		{
			_dataState3Boolean = dataState3Boolean;
		}

		internal bool GetIsExpanded()
		{
			return _isExpanded;
		}

		internal void SetIsExpanded(bool condition)
		{
			_isExpanded = condition;
		}

		internal GridFormatStyleState GetFormatStyleState()
		{
			return _gridFormatStyleState;
		}

		internal void SetFormatStyleState(GridFormatStyleState gridFormatStyleState)
		{
			_gridFormatStyleState = gridFormatStyleState;
		}

		internal RowCellSelection GetCellSelectionState()
		{
			return _mordexRowState;
		}

		internal void SetCellSelectionState(RowCellSelection mordexRowState)
		{
			_mordexRowState = mordexRowState;
		}
	}
}
