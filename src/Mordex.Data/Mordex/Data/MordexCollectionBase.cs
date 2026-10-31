using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Mordex.Data
{
	public abstract class MordexCollectionBase : IList, ICollection, IEnumerable
	{
		protected const int ErrorCodeActionCanceled = 1;

		protected const int ErrorCodeNotCollectionElement = 2;

		protected const int ErrorCodeAddingNullElement = 3;

		protected const int ErrorCodeItemBelongsToOtherCollection = 4;

		protected const int ErrorCodeCustom = 10;

		private ArrayList _items;

		protected ICollectionOwner owner;

		protected IList List => this;

		object ICollection.SyncRoot => _items.SyncRoot;

		bool ICollection.IsSynchronized => _items.IsSynchronized;

		public int Count => _items.Count;

		bool IList.IsFixedSize => false;

		bool IList.IsReadOnly => false;

		object IList.this[int index]
		{
			get
			{
				return _items[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected MordexCollectionBase(ICollectionOwner owner)
		{
			this.owner = owner;
			_items = new ArrayList();
		}

		private void RenumberElements()
		{
			for (int i = 0; i < _items.Count; i++)
			{
				((MordexCollectionElement)_items[i]).SetIndex(i);
			}
		}

		internal ICollectionOwner GetOwner()
		{
			return owner;
		}

		internal void DetachOwner()
		{
			owner = null;
			Clear();
		}

		public IEnumerator GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		void ICollection.CopyTo(Array destination, int index)
		{
			_items.CopyTo(destination, index);
		}

		int IList.IndexOf(object value)
		{
			return _items.IndexOf(value);
		}

		bool IList.Contains(object value)
		{
			return _items.Contains(value);
		}

		void IList.Insert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (OnInsert(index, value))
			{
				_items.Insert(index, value);
				OnInsertComplete(index, value);
			}
		}

		public void Clear()
		{
			if (Count != 0 && OnClear())
			{
				_items.Clear();
				OnClearComplete();
			}
		}

		int IList.Add(object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (!OnInsert(-1, value))
			{
				return -1;
			}
			int newIndex = _items.Add(value);
			OnInsertComplete(newIndex, value);
			return newIndex;
		}

		void IList.Remove(object value)
		{
			int index = _items.IndexOf(value);
			if (index != -1)
			{
				((IList)this).RemoveAt(index);
			}
		}

		void IList.RemoveAt(int index)
		{
			if (index < 0 || index >= _items.Count)
			{
				throw new ArgumentOutOfRangeException();
			}
			object value = _items[index];
			if (OnRemove(index, value))
			{
				_items.RemoveAt(index);
				OnRemoveComplete(index, value);
			}
		}

		protected virtual string GetErrorMessage(int errorCode)
		{
			switch (errorCode)
			{
			case 1:
				return "Action was canceled by the user.";
			case 3:
				return "Can not add null values to the collection.";
			case 2:
				return "Only types derived from MordexCollectionElement can be contained in the collection.";
			default:
				return "";
			}
		}

		protected void BaseReplaceElements(IList objects)
		{
			for (int index = Count - 1; index >= 0; index--)
			{
				if (!objects.Contains(List[index]))
				{
					List.RemoveAt(index);
				}
			}
			_items = new ArrayList();
			for (int i = 0; i < objects.Count; i++)
			{
				MordexCollectionElement mordexCollectionElement = objects[i] as MordexCollectionElement;
				if (mordexCollectionElement.GetOwningCollection() == this)
				{
					_items.Add(mordexCollectionElement);
				}
			}
			for (int j = 0; j < objects.Count; j++)
			{
				MordexCollectionElement elementToInsert = objects[j] as MordexCollectionElement;
				if (elementToInsert.GetOwningCollection() == null)
				{
					List.Insert(j, elementToInsert);
				}
			}
			RenumberElements();
		}

		protected virtual bool OnClear()
		{
			if (owner != null)
			{
				switch (owner.OnCollectionChanging(this, CollectionChangeType.Clear, null))
				{
				case CollectionChangingAction.CancelNoError:
					return false;
				case CollectionChangingAction.Cancel:
					throw new Exception(GetErrorMessage(1));
				}
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MordexCollectionElement mordexCollectionElement = (MordexCollectionElement)enumerator.Current;
					mordexCollectionElement.SetIndex(-1);
					mordexCollectionElement.SetOwningCollection(null);
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
			return true;
		}

		protected virtual void OnClearComplete()
		{
			if (owner != null)
			{
				owner.OnCollectionChanged(this, CollectionChangeType.Clear, null);
			}
		}

		protected virtual bool OnInsert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(GetErrorMessage(3));
			}
			if (!(value is MordexCollectionElement mordexCollectionElement))
			{
				throw new ArgumentException(GetErrorMessage(2));
			}
			if (mordexCollectionElement.GetOwningCollection() != null)
			{
				throw new InvalidOperationException(GetErrorMessage(4));
			}
			if (owner != null)
			{
				switch (owner.OnCollectionChanging(this, CollectionChangeType.Insert, value))
				{
				case CollectionChangingAction.CancelNoError:
					return false;
				case CollectionChangingAction.Cancel:
					throw new Exception(GetErrorMessage(1));
				}
			}
			return true;
		}

		protected virtual void OnInsertComplete(int index, object value)
		{
			MordexCollectionElement mordexCollectionElement = value as MordexCollectionElement;
			mordexCollectionElement.SetOwningCollection(this);
			RenumberElements();
			if (owner != null)
			{
				owner.OnCollectionChanged(this, CollectionChangeType.Insert, value);
			}
		}

		protected virtual bool OnRemove(int index, object value)
		{
			if (owner != null)
			{
				switch (owner.OnCollectionChanging(this, CollectionChangeType.Remove, value))
				{
				case CollectionChangingAction.CancelNoError:
					return false;
				case CollectionChangingAction.Cancel:
					throw new Exception(GetErrorMessage(1));
				}
			}
			return true;
		}

		protected virtual void OnRemoveComplete(int index, object value)
		{
			MordexCollectionElement mordexCollectionElement = value as MordexCollectionElement;
			mordexCollectionElement.SetIndex(-1);
			mordexCollectionElement.SetOwningCollection(null);
			RenumberElements();
			if (owner != null)
			{
				owner.OnCollectionChanged(this, CollectionChangeType.Remove, value);
			}
		}
	}
}
