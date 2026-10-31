using System;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class UpdateScope : IDisposable
	{
		private MxDataGrid _grid;

		private int _depth;

		private bool _isRefreshPending;

		internal UpdateScope(MxDataGrid grid)
		{
			_grid = grid;
		}

		internal void Enter()
		{
			_depth++;
		}

		internal bool IsActive()
		{
			return _depth > 0;
		}

		public void Dispose()
		{
			if (_depth > 0)
			{
				_depth--;
				if (_grid != null && _depth == 0 && _isRefreshPending)
				{
					_grid.RaiseSelectionChangedOrDefer();
					_isRefreshPending = false;
				}
			}
		}

		internal void SetRefreshPending(bool condition)
		{
			_isRefreshPending = condition;
		}
	}
}
