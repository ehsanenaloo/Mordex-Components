using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridFormatConditionCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal GridTable _ownerTable;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Condition";

		public GridFormatCondition this[int index] => (GridFormatCondition)base.InnerList[index];

		public GridFormatCondition this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridFormatCondition gridFormatCondition = (GridFormatCondition)enumerator.Current;
						if (string.Compare(gridFormatCondition.Key, key, ignoreCase: true) == 0)
						{
							return gridFormatCondition;
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
			int value = 0;
			for (PropertyValue propertyValue = info["Condition" + value]; propertyValue != null; propertyValue = info["Condition" + value])
			{
				GridFormatCondition gridFormatCondition = new GridFormatCondition();
				base.InnerList.Add(gridFormatCondition);
				gridFormatCondition.SetOwnerCollection(this);
				gridFormatCondition._collectionIndex = value;
				value++;
				propertyValue.DeserializeObject(gridFormatCondition);
			}
			NotifyConditionsChanged();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int otherValue = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridFormatCondition value = (GridFormatCondition)enumerator.Current;
					info.AddValue("Condition" + otherValue, value);
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

		internal GridFormatConditionCollection(GridTable table)
		{
			_ownerTable = table;
		}

		internal MxDataGrid ComputeDataGrid()
		{
			if (_ownerTable != null)
			{
				return _ownerTable.DataGrid;
			}
			return null;
		}

		public int IndexOf(GridFormatCondition formatCondition)
		{
			return base.InnerList.IndexOf(formatCondition);
		}

		public void AddRange(GridFormatCondition[] conditions)
		{
			for (int i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] != null)
				{
					base.InnerList.Add(conditions[i]);
					conditions[i]._collectionIndex = base.Count - 1;
					conditions[i].SetOwnerCollection(this);
				}
			}
			NotifyConditionsChanged();
		}

		public void Add(GridFormatCondition condition)
		{
			if (condition == null)
			{
				throw new ArgumentNullException();
			}
			if (condition.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (!condition.IsValidForTable(_ownerTable))
			{
				throw new ArgumentException();
			}
			base.InnerList.Add(condition);
			condition._collectionIndex = base.Count - 1;
			condition.SetOwnerCollection(this);
			NotifyConditionsChanged();
		}

		public new void RemoveAt(int index)
		{
			GridFormatCondition condition = this[index];
			Remove(condition);
		}

		public void Remove(GridFormatCondition condition)
		{
			if (condition == null)
			{
				throw new ArgumentNullException();
			}
			base.InnerList.Remove(condition);
			condition.SetOwnerCollection(null);
			RenumberItems();
			NotifyConditionsChanged();
		}

		private void RenumberItems()
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i]._collectionIndex = i;
			}
		}

		protected override void OnClear()
		{
			base.OnClear();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridFormatCondition gridFormatCondition = (GridFormatCondition)enumerator.Current;
					gridFormatCondition.SetOwnerCollection(null);
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

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			NotifyConditionsChanged();
		}

		private void NotifyConditionsChanged()
		{
			ApplyConditionsToGrid();
		}

		internal void ApplyConditionsToGrid()
		{
			if (ComputeDataGrid() != null)
			{
				ComputeDataGrid().InvalidateTableRows(_ownerTable);
			}
		}
	}
}
