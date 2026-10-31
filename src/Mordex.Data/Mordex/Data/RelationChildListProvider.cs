using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace Mordex.Data
{
	internal class RelationChildListProvider
	{
		internal DataSourceManager _owner;

		private DataRelation _relation;

		private DataView _dataView;

		private ArrayList _itemStates;

		private Dictionary<DataRow, RelatedDataViewRow> _itemStateByRow;

		private bool _disposed;

		private Hashtable _collectionsByParentRow;

		private bool _isEndingEdit;

		internal RelationChildListProvider(DataSourceManager dsManager, DataRelation relation)
		{
			_owner = dsManager;
			_relation = relation;
			relation.ParentTable.RowChanged += OnParentRowChanged;
			relation.ParentTable.RowDeleted += OnParentRowDeleted;
			relation.ParentTable.TableCleared += OnParentTableCleared;
			_dataView = new DataView(relation.ChildTable);
			string sortExpression = BuildSortExpression();
			if (sortExpression != null)
			{
				_dataView.Sort = sortExpression;
			}
			_dataView.ListChanged += OnDataViewListChanged;
			RebuildItemRows();
			_collectionsByParentRow = new Hashtable();
		}

		private void OnParentTableCleared(object sender, DataTableClearEventArgs eventArgs)
		{
			List<RelatedDataView> list = new List<RelatedDataView>();
			foreach (RelatedDataView value in _collectionsByParentRow.Values)
			{
				list.Add(value);
			}
			foreach (RelatedDataView item in list)
			{
				item.DisposeView();
			}
		}

		private void OnParentRowDeleted(object sender, DataRowChangeEventArgs eventArgs)
		{
			if (_collectionsByParentRow.ContainsKey(eventArgs.Row))
			{
				((RelatedDataView)_collectionsByParentRow[eventArgs.Row]).DisposeView();
			}
		}

		private void OnParentRowChanged(object sender, DataRowChangeEventArgs eventArgs)
		{
			if (eventArgs.Action == DataRowAction.Change && _collectionsByParentRow[eventArgs.Row] is RelatedDataView collection)
			{
				ReloadCollectionIfKeyChanged(collection);
			}
		}

		internal void DisposeResources()
		{
			_disposed = true;
			if (_relation != null)
			{
				_relation.ParentTable.RowChanged -= OnParentRowChanged;
				_relation.ParentTable.RowDeleted -= OnParentRowDeleted;
				_relation.ParentTable.TableCleared -= OnParentTableCleared;
			}
			_relation = null;
			if (_dataView != null)
			{
				_dataView.ListChanged -= OnDataViewListChanged;
				_dataView.Dispose();
				_dataView = null;
			}
			foreach (RelatedDataView value in _collectionsByParentRow.Values)
			{
				value.DisposeView();
			}
			_collectionsByParentRow.Clear();
			_collectionsByParentRow = null;
			foreach (RelatedDataViewRow item in _itemStates)
			{
				item.Detach();
			}
			_itemStates.Clear();
			_itemStates = null;
			_itemStateByRow = null;
		}

		internal DataView GetDataView()
		{
			return _dataView;
		}

		internal DataRelation GetRelation()
		{
			return _relation;
		}

		internal DataRowView GetRowView(RelatedDataViewRow itemState)
		{
			if (itemState.GetIndex() >= 0 && itemState.GetIndex() < _dataView.Count)
			{
				return _dataView[itemState.GetIndex()];
			}
			return null;
		}

		private void RebuildItemRows()
		{
			_itemStates = new ArrayList();
			_itemStateByRow = new Dictionary<DataRow, RelatedDataViewRow>(_dataView.Count);
			for (int i = 0; i < _dataView.Count; i++)
			{
				RelatedDataViewRow itemState = new RelatedDataViewRow(i, this);
				_itemStates.Add(itemState);
				_itemStateByRow[itemState.GetDataRow()] = itemState;
			}
		}

		private string BuildSortExpression()
		{
			string text = "";
			DataColumn[] childColumns = _relation.ChildColumns;
			foreach (DataColumn dataColumn in childColumns)
			{
				if (dataColumn.ColumnName.Contains(','))
				{
					// DataView.Sort splits on every comma, even inside brackets, so the view keeps the table order.
					return null;
				}
				if (text.Length > 0)
				{
					text += ", ";
				}
				// DataView.Sort strips the outer brackets and does not unescape, so ']' inside the name stays as is.
				text += "[" + dataColumn.ColumnName + "]";
			}
			return text;
		}

		// The relation's own index matches the keys, so the table's Locale, CaseSensitive and string trimming rules apply.
		private void LoadChildRows(RelatedDataView collection, DataRow parentRow, object[] keyValues, bool raiseReset)
		{
			collection._parentKeyValues = keyValues;
			collection.BeginBulkLoad(raiseReset);
			try
			{
				List<RelatedDataViewRow> itemStates = new List<RelatedDataViewRow>();
				foreach (DataRow childRow in parentRow.GetChildRows(_relation))
				{
					if (_itemStateByRow.TryGetValue(childRow, out RelatedDataViewRow itemState))
					{
						itemStates.Add(itemState);
					}
				}
				itemStates.Sort((first, second) => first.GetIndex().CompareTo(second.GetIndex()));
				foreach (RelatedDataViewRow itemState in itemStates)
				{
					collection.AddRow(itemState);
				}
			}
			finally
			{
				collection.EndBulkLoad();
			}
		}

		private object[] GetParentKeyValues(DataRow parentRow)
		{
			object[] keyValues = new object[_relation.ParentColumns.Length];
			for (int i = 0; i < keyValues.Length; i++)
			{
				keyValues[i] = parentRow[_relation.ParentColumns[i]];
			}
			return keyValues;
		}

		private void RemoveItem(int integer)
		{
			if (integer < _itemStates.Count)
			{
				for (int i = integer + 1; i < _itemStates.Count; i++)
				{
					RelatedDataViewRow shiftedState = (RelatedDataViewRow)_itemStates[i];
					shiftedState.SetIndex(shiftedState.GetIndex() - 1);
				}
				RelatedDataViewRow removedState = (RelatedDataViewRow)_itemStates[integer];
				if (_itemStateByRow.TryGetValue(removedState.GetDataRow(), out RelatedDataViewRow mappedState) && mappedState == removedState)
				{
					_itemStateByRow.Remove(removedState.GetDataRow());
				}
				if (removedState.GetView() != null)
				{
					removedState.GetView().RemoveRow(removedState);
				}
				_itemStates.RemoveAt(integer);
				removedState.Detach();
			}
		}

		internal void ReloadCollectionIfKeyChanged(RelatedDataView collection)
		{
			DataRow row = collection.GetParentRow().Row;
			object[] keyValues = GetParentKeyValues(row);
			if (!collection.HasSameKeys(keyValues))
			{
				LoadChildRows(collection, row, keyValues, true);
			}
		}

		private void RebuildAfterReset()
		{
			_owner.RowManager.SuspendIndex();
			try
			{
				RebuildItemRows();
				ArrayList arrayList = new ArrayList();
				foreach (RelatedDataView value2 in _collectionsByParentRow.Values)
				{
					DataRow row = value2.GetParentRow().Row;
					if (row.RowState == DataRowState.Detached)
					{
						arrayList.Add(value2);
						continue;
					}
					LoadChildRows(value2, row, GetParentKeyValues(row), true);
				}
				foreach (RelatedDataView item in arrayList)
				{
					item.DisposeView();
				}
			}
			finally
			{
				_owner.RowManager.ResumeIndex();
			}
		}

		private void OnDataViewListChanged(object sender, ListChangedEventArgs eventArgs)
		{
			switch (eventArgs.ListChangedType)
			{
			case ListChangedType.Reset:
				RebuildAfterReset();
				break;
			case ListChangedType.ItemMoved:
				if (!_isEndingEdit)
				{
					MoveItem(eventArgs.OldIndex, eventArgs.NewIndex);
				}
				break;
			case ListChangedType.ItemAdded:
				InsertItemRow(eventArgs.NewIndex);
				break;
			case ListChangedType.ItemDeleted:
				RemoveItem(eventArgs.NewIndex);
				break;
			case ListChangedType.ItemChanged:
				RefreshItemCollection(eventArgs.NewIndex);
				break;
			}
		}

		private void InsertItemRow(int index)
		{
			if (!_dataView[index].IsNew)
			{
				RelatedDataViewRow itemState = new RelatedDataViewRow(index, this);
				_itemStates.Insert(index, itemState);
				_itemStateByRow[itemState.GetDataRow()] = itemState;
				for (int i = index + 1; i < _itemStates.Count; i++)
				{
					((RelatedDataViewRow)_itemStates[i]).SetIndex(i);
				}
				FindCollectionForItem(itemState)?.AddRow(itemState);
			}
		}

		internal void EndEditIgnoringMoves(DataRowView dataRowView)
		{
			bool wasEndingEdit = _isEndingEdit;
			_isEndingEdit = true;
			try
			{
				dataRowView.EndEdit();
			}
			finally
			{
				_isEndingEdit = wasEndingEdit;
			}
		}

		private RelatedDataView FindCollectionForItem(RelatedDataViewRow itemState)
		{
			DataRow parentRow = itemState.GetRowView().Row.GetParentRow(_relation);
			if (parentRow == null)
			{
				return null;
			}
			return (RelatedDataView)_collectionsByParentRow[parentRow];
		}

		private void MoveItem(int index, int newIndex)
		{
			RelatedDataViewRow itemState = (RelatedDataViewRow)_itemStates[index];
			_itemStates.RemoveAt(index);
			_itemStates.Insert(newIndex, itemState);
			itemState.SetIndex(newIndex);
			if (index > newIndex)
			{
				for (int i = newIndex; i <= index; i++)
				{
					((RelatedDataViewRow)_itemStates[i]).SetIndex(i);
				}
			}
			else if (newIndex > index)
			{
				for (int j = index; j <= newIndex; j++)
				{
					((RelatedDataViewRow)_itemStates[j]).SetIndex(j);
				}
			}
			RefreshItemCollection(newIndex);
		}

		private void RefreshItemCollection(int integer)
		{
			RelatedDataViewRow itemState = (RelatedDataViewRow)_itemStates[integer];
			RelatedDataView currentView = itemState.GetView();
			if (currentView != null)
			{
				if (Array.IndexOf(itemState.GetDataRow().GetParentRows(_relation), currentView.GetParentDataRow()) >= 0)
				{
					itemState.NotifyChanged();
					return;
				}
				currentView.RemoveRow(itemState);
			}
			FindCollectionForItem(itemState)?.AddRow(itemState);
		}

		internal void RemoveCollection(RelatedDataView collection)
		{
			if (!_disposed && _collectionsByParentRow.Contains(collection.GetParentDataRow()))
			{
				_collectionsByParentRow.Remove(collection.GetParentDataRow());
			}
		}

		internal RelatedDataView GetOrCreateCollection(DataRowView parentRow)
		{
			if (_collectionsByParentRow.ContainsKey(parentRow.Row))
			{
				return (RelatedDataView)_collectionsByParentRow[parentRow.Row];
			}
			RelatedDataView collection = new RelatedDataView(this, parentRow);
			LoadChildRows(collection, parentRow.Row, GetParentKeyValues(parentRow.Row), false);
			_collectionsByParentRow.Add(collection.GetParentDataRow(), collection);
			return collection;
		}
	}
}
