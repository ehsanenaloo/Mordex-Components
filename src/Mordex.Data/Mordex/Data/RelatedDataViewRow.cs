using System;
using System.Data;

namespace Mordex.Data
{
	internal class RelatedDataViewRow : IComparable
	{
		private DataRow _dataRow;

		private int _index;

		private RelationChildListProvider _childListProvider;

		private RelatedDataView _view;

		internal RelatedDataViewRow(int index, RelationChildListProvider dataViewManager)
		{
			_childListProvider = dataViewManager;
			_index = index;
			_dataRow = GetRowView().Row;
		}

		internal DataRow GetDataRow()
		{
			return _dataRow;
		}

		internal RelatedDataView GetView()
		{
			return _view;
		}

		internal void SetView(RelatedDataView view)
		{
			_view = view;
		}

		internal int GetIndex()
		{
			return _index;
		}

		internal void SetIndex(int index)
		{
			_index = index;
		}

		internal DataRowView GetRowView()
		{
			return _childListProvider.GetRowView(this);
		}

		internal void DeleteRow()
		{
			DataRowView dataRowView = GetRowView();
			dataRowView.Delete();
		}

		internal void NotifyChanged()
		{
			if (_view != null)
			{
				_view.NotifyRowChanged(this);
			}
		}

		internal void Detach()
		{
			_view = null;
			_childListProvider = null;
			_index = -1;
		}

		int IComparable.CompareTo(object other)
		{
			RelatedDataViewRow otherRow = (RelatedDataViewRow)other;
			return _index.CompareTo(otherRow._index);
		}
	}
}
