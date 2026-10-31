using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private GridLayout _parentLayout;

		private MxDataGrid _dataGrid;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public GridLayout this[int index] => (GridLayout)base.List[index];

		public GridLayout this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridLayout gridLayout = (GridLayout)enumerator.Current;
						if (string.Compare(gridLayout.Key, key, ignoreCase: true) == 0)
						{
							return gridLayout;
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

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int value = 0;
			for (PropertyValue propertyValue = info["Layout" + value]; propertyValue != null; propertyValue = info["Layout" + value])
			{
				GridLayout gridLayout = new GridLayout();
				propertyValue.DeserializeObject(gridLayout);
				Add(gridLayout);
				value++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int otherValue = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridLayout value = (GridLayout)enumerator.Current;
					info.AddValue("Layout" + otherValue, value);
					otherValue++;
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
		}

		internal GridLayoutCollection(MxDataGrid dataGrid)
		{
			_dataGrid = dataGrid;
		}

		internal GridLayoutCollection(GridLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal GridLayout GetParentLayout()
		{
			return _parentLayout;
		}

		internal MxDataGrid ComputeDataGrid()
		{
			if (_parentLayout == null)
			{
				return _dataGrid;
			}
			return _parentLayout.ComputeDataGrid();
		}

		internal MxDataGrid GetDataGrid()
		{
			return _dataGrid;
		}

		public void Add(GridLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(GridLayout[] layouts)
		{
			foreach (GridLayout layout in layouts)
			{
				Add(layout);
			}
		}

		protected override void OnInsert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (value.GetType() != typeof(GridLayout))
			{
				throw new InvalidCastException();
			}
			GridLayout gridLayout = (GridLayout)value;
			if (gridLayout.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !gridLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			GridLayout gridLayout = (GridLayout)value;
			gridLayout.SetOwnerCollection(this);
			gridLayout.EnsureIncrementalLayout();
		}

		public void Remove(GridLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxDataGrid dataGrid = ComputeDataGrid();
			if (dataGrid != null && dataGrid.CurrentLayout == (GridLayout)value)
			{
				dataGrid.CurrentLayout = null;
			}
			((GridLayout)value).SetOwnerCollection(null);
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridLayout gridLayout = (GridLayout)enumerator.Current;
					gridLayout.SetOwnerCollection(null);
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
		}

		public int IndexOf(GridLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(GridLayout layout)
		{
			return base.List.Contains(layout);
		}

		public bool Contains(string layoutKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridLayout gridLayout = (GridLayout)enumerator.Current;
					if (string.Compare(gridLayout.Key, layoutKey, ignoreCase: true) == 0)
					{
						return true;
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
			return false;
		}

		internal GridLayout FindCurrentLayout()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridLayout gridLayout = (GridLayout)enumerator.Current;
					GridLayout currentLayout = gridLayout.FindCurrentLayout();
					if (currentLayout != null)
					{
						return currentLayout;
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
			return null;
		}

		internal void MarkCurrentLayout(GridLayout otherGridLayout)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridLayout gridLayout = (GridLayout)enumerator.Current;
					gridLayout.UpdateIsCurrentLayout(otherGridLayout);
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
		}
	}
}
