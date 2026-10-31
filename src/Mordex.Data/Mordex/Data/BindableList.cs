using System.Collections;
using System.ComponentModel;

namespace Mordex.Data
{
	public class BindableList
	{
		private IBindingList _bindingList;

		private IList _list;

		public IList List => _list;

		public IBindingList BindingList => _bindingList;

		public virtual bool SupportsChangeNotification
		{
			get
			{
				if (_bindingList != null)
				{
					return _bindingList.SupportsChangeNotification;
				}
				return false;
			}
		}

		public int Count
		{
			get
			{
				if (_list != null)
				{
					return _list.Count;
				}
				return 0;
			}
		}

		public object this[int index]
		{
			get
			{
				if (_list != null)
				{
					return _list[index];
				}
				return null;
			}
		}

		public virtual event ListChangedEventHandler ListChanged
		{
			add
			{
				if (_bindingList != null)
				{
					_bindingList.ListChanged += value;
				}
			}
			remove
			{
				if (_bindingList != null)
				{
					_bindingList.ListChanged -= value;
				}
			}
		}

		public BindableList(IList list)
		{
			_list = list;
			_bindingList = list as IBindingList;
		}

		internal bool AllowsRemove()
		{
			if (_bindingList != null)
			{
				return _bindingList.AllowRemove;
			}
			return true;
		}

		public virtual object AddNew()
		{
			if (_bindingList != null)
			{
				return _bindingList.AddNew();
			}
			return null;
		}

		public void CancelAddNew(int index)
		{
			if (_list is ICancelAddNew cancelAddNew)
			{
				cancelAddNew.CancelNew(index);
			}
		}

		public void EndAddNew(object dataRow)
		{
			if (_list is ICancelAddNew cancelAddNew)
			{
				int itemIndex = _list.IndexOf(dataRow);
				cancelAddNew.EndNew(itemIndex);
			}
		}
	}
}
