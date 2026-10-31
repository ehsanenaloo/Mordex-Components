using System;
using System.Collections;

namespace Mordex.Windows.UI.CommandBars
{
	internal class CommandBarOrderComparer : IComparer
	{
		private MxCommandManager _commandManager;

		public bool _compareMergedValues;

		public bool _compareByRow;

		public bool _isHorizontal = true;

		internal CommandBarOrderComparer(bool compareByRow, MxCommandManager cmdManager)
		{
			_compareByRow = compareByRow;
			_commandManager = cmdManager;
		}

		internal CommandBarOrderComparer(bool compareByRow, bool compareMergedValues, MxCommandManager cmdManager)
		{
			_compareByRow = compareByRow;
			_compareMergedValues = compareMergedValues;
			_commandManager = cmdManager;
		}

		public int Compare(object first, object second)
		{
			if (first == second)
			{
				return 0;
			}
			if (!(first is MxCommandBar) || !(second is MxCommandBar))
			{
				throw new InvalidOperationException();
			}
			if (_compareMergedValues)
			{
				int mergeComparison = CompareMerged(first, second);
				if (mergeComparison != 0)
				{
					return mergeComparison;
				}
			}
			int comparison = 0;
			if (_compareByRow)
			{
				int firstRowIndex = ((MxCommandBar)first).RowIndex;
				int secondRowIndex = ((MxCommandBar)second).RowIndex;
				comparison = firstRowIndex.CompareTo(secondRowIndex);
				if (comparison != 0)
				{
					return comparison;
				}
			}
			if (_isHorizontal)
			{
				int firstX = ((MxCommandBar)first).GetPosition().X;
				int secondX = ((MxCommandBar)second).GetPosition().X;
				comparison = firstX.CompareTo(secondX);
				if (comparison == 0)
				{
					firstX = ((MxCommandBar)first).Location.X;
					secondX = ((MxCommandBar)second).Location.X;
					comparison = firstX.CompareTo(secondX);
				}
			}
			else
			{
				int firstY = ((MxCommandBar)first).GetPosition().Y;
				int secondY = ((MxCommandBar)second).GetPosition().Y;
				comparison = firstY.CompareTo(secondY);
				if (comparison == 0)
				{
					firstY = ((MxCommandBar)first).Location.Y;
					secondY = ((MxCommandBar)second).Location.Y;
					comparison = firstY.CompareTo(secondY);
				}
			}
			if (comparison == 0)
			{
				if (_commandManager != null)
				{
					int firstBarIndex = _commandManager.CommandBars.IndexOf((MxCommandBar)first);
					int secondBarIndex = _commandManager.CommandBars.IndexOf((MxCommandBar)second);
					comparison = firstBarIndex.CompareTo(secondBarIndex);
					if (comparison != 0)
					{
						return comparison;
					}
				}
				int firstItemIndex = ((MxCommandBar)first).GetBarIndex();
				int secondItemIndex = ((MxCommandBar)second).GetBarIndex();
				comparison = firstItemIndex.CompareTo(secondItemIndex);
			}
			return comparison;
		}

		public int CompareMerged(object first, object second)
		{
			MxCommandBar firstBar = (MxCommandBar)first;
			MxCommandBar secondBar = (MxCommandBar)second;
			int comparison = 0;
			if (_compareByRow)
			{
				int firstMergeRowOrder = firstBar.MergeRowOrder;
				int secondMergeRowOrder = secondBar.MergeRowOrder;
				if (firstMergeRowOrder >= 0 && secondMergeRowOrder >= 0)
				{
					comparison = firstMergeRowOrder.CompareTo(secondMergeRowOrder);
					if (comparison != 0)
					{
						return comparison;
					}
				}
			}
			if (_isHorizontal)
			{
				int firstX = firstBar.GetMergedLocation().X;
				int secondX = secondBar.GetMergedLocation().X;
				comparison = firstX.CompareTo(secondX);
			}
			else
			{
				int firstY = firstBar.GetMergedLocation().Y;
				int secondY = secondBar.GetMergedLocation().Y;
				comparison = firstY.CompareTo(secondY);
			}
			if (comparison == 0)
			{
				bool firstIsMerged = firstBar.GetIsMergedCommandBar();
				bool secondIsMerged = secondBar.GetIsMergedCommandBar();
				comparison = firstIsMerged.CompareTo(secondIsMerged);
			}
			if (comparison == 0)
			{
				int firstIndex = firstBar.GetBarIndex();
				int secondIndex = secondBar.GetBarIndex();
				comparison = firstIndex.CompareTo(secondIndex);
			}
			return comparison;
		}
	}
}
