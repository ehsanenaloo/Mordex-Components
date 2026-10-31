using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class PrintPageCardLayout
	{
		private int _cardSpacing;

		private int _cardColumns;

		private int _currentX;

		private int _gridState20Integer;

		private int _pageTop;

		private int _currentY;

		private int _currentColumn;

		internal TableLayoutCollection _pageTables;

		internal PrintPageCardLayout(int cardColumns, int cardSpacing, Rectangle marginBounds, float scale)
		{
			_gridState20Integer = (int)((float)marginBounds.Height / scale);
			_currentX = marginBounds.Left;
			_pageTop = marginBounds.Top;
			_currentY = marginBounds.Top;
			_pageTables = new TableLayoutCollection();
			_cardColumns = cardColumns;
			_cardSpacing = cardSpacing;
		}

		internal bool TryAddTable(GridTableLayoutState gridTableLayoutState)
		{
			if (_currentY > _pageTop && _currentY + gridTableLayoutState.GetHeight() > _gridState20Integer)
			{
				_currentColumn++;
				_currentX += gridTableLayoutState.GetWidth() + _cardSpacing;
				_currentY = _pageTop;
			}
			if (_currentColumn >= _cardColumns)
			{
				return false;
			}
			_pageTables.AddTableLayout(gridTableLayoutState);
			gridTableLayoutState._location = new Point(_currentX, _currentY);
			_currentY += gridTableLayoutState.GetHeight() + _cardSpacing;
			return true;
		}
	}
}
