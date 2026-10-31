using System;
using System.ComponentModel;

namespace Mordex.Data
{
	public abstract class MordexCollectionElement
	{
		protected const int ErrorCodeCancelChanging = 1;

		protected const int ErrorCodeCustom = 10;

		private MordexCollectionBase _parentCollection;

		private ICollectionOwner _collectionOwner;

		private int _index;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected MordexCollectionBase BaseParentCollection => _parentCollection;

		protected ICollectionOwner CollectionOwner => _collectionOwner;

		[Browsable(false)]
		public virtual int Index => _index;

		protected MordexCollectionElement()
		{
			_index = -1;
		}

		internal MordexCollectionBase GetOwningCollection()
		{
			return _parentCollection;
		}

		internal void SetOwningCollection(MordexCollectionBase parent)
		{
			_parentCollection = parent;
			if (parent == null)
			{
				_collectionOwner = null;
			}
			else
			{
				_collectionOwner = parent.GetOwner();
			}
		}

		protected void BaseSetParentCollection(MordexCollectionBase parent)
		{
			_parentCollection = parent;
			if (parent == null)
			{
				_collectionOwner = null;
			}
			else
			{
				_collectionOwner = parent.GetOwner();
			}
		}

		protected virtual bool OnSettingsChanging()
		{
			if (CollectionOwner != null)
			{
				switch (CollectionOwner.OnCollectionChanging(_parentCollection, CollectionChangeType.ItemChange, this))
				{
				case CollectionChangingAction.CancelNoError:
					return false;
				case CollectionChangingAction.Cancel:
					throw new Exception(GetErrorMessage(1));
				}
			}
			return true;
		}

		protected virtual void OnSettingsChanged()
		{
			if (CollectionOwner != null)
			{
				CollectionOwner.OnCollectionChanged(_parentCollection, CollectionChangeType.ItemChange, this);
			}
		}

		protected virtual string GetErrorMessage(int errorCode)
		{
			if (errorCode == 1)
			{
				return "Operation cancelled.";
			}
			return "Errors ocurred.";
		}

		internal void SetIndex(int integer)
		{
			_index = integer;
		}
	}
}
