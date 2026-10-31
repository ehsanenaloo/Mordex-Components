using System;
using System.ComponentModel;

namespace Mordex.Data
{
	internal class HierarchyNode : IComparable, IEditableObject, IHierarchyParent
	{
		internal int _rowCacheIndex;

		private IHierarchyParent _parent;

		private int _dataRowIndex;

		private object _parentMemberValue;

		private object _childMemberValue;

		private MordexFieldBaseState _selfReferencingManager;

		private HierarchyNodeCollection _children;

		internal bool _isEditing;

		MordexFieldBaseState IHierarchyParent.SelfReferencingManager => _selfReferencingManager;

		HierarchyNodeCollection IHierarchyParent.ChildRows => _children;

		internal HierarchyNode(MordexFieldBaseState selfReferencingManager, int dataRowIndex)
		{
			_children = new HierarchyNodeCollection(selfReferencingManager, this);
			_dataRowIndex = dataRowIndex;
			_selfReferencingManager = selfReferencingManager;
		}

		internal IHierarchyParent GetParent()
		{
			return _parent;
		}

		internal HierarchyNodeCollection GetChildren()
		{
			return _children;
		}

		internal int GetDataRowIndex()
		{
			return _dataRowIndex;
		}

		internal void SetDataRowIndex(int dataRowIndex)
		{
			_dataRowIndex = dataRowIndex;
		}

		internal object GetParentMemberValue()
		{
			return _parentMemberValue;
		}

		internal void SetParentMemberValue(object value)
		{
			_parentMemberValue = value;
		}

		internal object GetChildMemberValue()
		{
			return _childMemberValue;
		}

		internal void SetChildMemberValue(object dataState3Value)
		{
			_childMemberValue = dataState3Value;
		}

		internal object GetSourceItem()
		{
			if (_dataRowIndex >= 0 && _dataRowIndex < _selfReferencingManager.GetBindableList().Count)
			{
				return _selfReferencingManager.GetBindableList()[_dataRowIndex];
			}
			return null;
		}

		int IComparable.CompareTo(object value)
		{
			object otherValue = ((!(value is HierarchyNode otherNode)) ? value : otherNode.GetParentMemberValue());
			return _selfReferencingManager.CompareKeys(_parentMemberValue, otherValue);
		}

		void IEditableObject.BeginEdit()
		{
			if (GetSourceItem() is IEditableObject editableObject)
			{
				editableObject.BeginEdit();
			}
			else
			{
				_isEditing = true;
			}
		}

		void IEditableObject.EndEdit()
		{
			if (GetSourceItem() is IEditableObject editableObject)
			{
				editableObject.EndEdit();
			}
			else if (_isEditing)
			{
				_isEditing = false;
				_selfReferencingManager.OnNodeChanged(this, true);
				return;
			}
			_isEditing = false;
			_selfReferencingManager.OnNodeEditEnded(this);
		}

		void IEditableObject.CancelEdit()
		{
			if (GetSourceItem() is IEditableObject editableObject)
			{
				editableObject.CancelEdit();
			}
			_isEditing = false;
		}

		internal void SetParent(IHierarchyParent newParent, bool notify)
		{
			if (newParent != _parent)
			{
				if (_parent != null)
				{
					_parent.RemoveNode(this);
				}
				_parent = newParent;
				newParent?.AddNode(this, notify);
			}
		}

		void IHierarchyParent.AddNode(HierarchyNode node, bool notify)
		{
			_children.InsertNode(node, notify);
		}

		void IHierarchyParent.RemoveNode(HierarchyNode node)
		{
			_children.RemoveNode(node);
		}
	}
}
