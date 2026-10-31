using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class PrintPageRowLayout
	{
		private int _usedHeight;

		private int _gridState19Integer;

		private int _pageHeight;

		private int _currentY;

		private int _sectionCount = 1;

		internal RowLayoutList _pageRows;

		internal PrintPageRowLayout(Rectangle marginBounds, float scale)
		{
			_pageHeight = (int)((float)marginBounds.Height / scale);
			_gridState19Integer = (int)((float)marginBounds.Left / scale);
			_currentY = (int)((float)marginBounds.Top / scale);
			_pageRows = new RowLayoutList();
		}

		public int GetPageSectionCount()
		{
			return _sectionCount;
		}

		internal bool TryAddRow(MxRowLayoutState mordexRowLayoutState, bool condition)
		{
			if (!condition && _pageRows.GetCount() != 0 && _usedHeight + mordexRowLayoutState._rowHeight >= _pageHeight)
			{
				return false;
			}
			_pageRows.AddRow(mordexRowLayoutState);
			mordexRowLayoutState.SetLocation(new Point(_gridState19Integer, _currentY));
			_currentY += mordexRowLayoutState._rowHeight;
			_usedHeight += mordexRowLayoutState._rowHeight;
			if (mordexRowLayoutState.LayoutTable.GetPrintSectionCount() > _sectionCount)
			{
				_sectionCount = mordexRowLayoutState._owner.GetPrintSectionCount();
			}
			return true;
		}
	}
}
