using System;
using System.Collections;

namespace Mordex.Windows.DataGrid
{
	public class GridTableCollection : CollectionBase
	{
		public GridTable this[int index] => (GridTable)base.List[index];

		public GridTable this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridTable gridTable = (GridTable)enumerator.Current;
						if (string.Compare(gridTable.Key, key, ignoreCase: true) == 0)
						{
							return gridTable;
						}
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				throw new ArgumentOutOfRangeException();
			}
		}

		internal GridTableCollection()
		{
		}

		internal void AddTableWithChildren(GridTable gridTable)
		{
			base.List.Add(gridTable);
			AddChildTables(gridTable.ChildTables);
		}

		internal void AddChildTables(GridChildTableCollection gridChildTableCollection)
		{
			if (gridChildTableCollection == null)
			{
				return;
			}
			foreach (GridTable item in gridChildTableCollection)
			{
				AddTableWithChildren(item);
			}
		}

		internal int IndexOfTable(GridTable gridTable)
		{
			for (int i = 0; i < base.Count; i++)
			{
				if (this[i] == gridTable)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
