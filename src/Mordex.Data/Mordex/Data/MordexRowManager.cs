using System;
using System.Collections;
using System.Collections.Generic;

namespace Mordex.Data
{
	public class MordexRowManager : IEnumerable
	{
		private MordexRootRow _root;

		private List<MordexRow> _rows;

		private List<MordexRow> _spare;

		private DataSourceManager _owner;

		internal int _indexSuspendCount;

		public int Count
		{
			get
			{
				if (_rows == null)
				{
					return 0;
				}
				return _rows.Count;
			}
		}

		public MordexRow this[int index]
		{
			get
			{
				if (_indexSuspendCount == 0)
				{
					if (index == -1)
					{
						if (_root == null)
						{
							return null;
						}
						return _root.NewRow;
					}
					return _rows[index];
				}
				return null;
			}
		}

		public bool IsIndexSuspend => _indexSuspendCount > 0;

		internal MordexRowManager(MordexRootRow root, DataSourceManager owner, int indexSuspendCount)
		{
			_owner = owner;
			_root = root;
			if (indexSuspendCount == 0)
			{
				Refresh();
			}
			else
			{
				_indexSuspendCount = indexSuspendCount;
			}
		}

		private DataSourceManager GetOwner()
		{
			return _owner;
		}

		public void Refresh()
		{
			int previousCount = 0;
			List<MordexRow> previous = _rows;
			if (previous != null)
			{
				previousCount = previous.Count;
				ClearOrAssignRowPositions(true);
			}
			List<MordexRow> next = _spare ?? new List<MordexRow>();
			_spare = null;
			_rows = next;
			if (previous != null)
			{
				previous.Clear();
				_spare = previous;
			}
			if (_root != null)
			{
				FillRows(_root, _rows);
				ClearOrAssignRowPositions(false);
				if (previousCount != _rows.Count)
				{
					NotifyRowCountChanged();
				}
			}
		}

		private static void FillRows(MordexRow mordexRow, List<MordexRow> list)
		{
			mordexRow?.FillList(list, childRowsOnly: false);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _rows.GetEnumerator();
		}

		private void NotifyRowCountChanged()
		{
			GetOwner();
		}

		internal void HandleRowChange(MordexRow mordexRow, RowChangeType rowChangeType)
		{
			switch (rowChangeType)
			{
			case RowChangeType.VisibleChanged:
				if (mordexRow.Visible)
				{
					RebuildAfterRowAdded(mordexRow);
				}
				else
				{
					RemoveRowAndDescendants(mordexRow);
				}
				break;
			case RowChangeType.ChildRowReset:
				RebuildAndNotify(mordexRow);
				break;
			case RowChangeType.ExpandedChanged:
				ApplyExpandedChange(mordexRow);
				break;
			case RowChangeType.Add:
				RebuildAfterRowAdded(mordexRow);
				break;
			case RowChangeType.Delete:
				RemoveRowAndDescendants(mordexRow);
				break;
			case RowChangeType.Move:
				RebuildAndNotify(mordexRow.ParentRow);
				break;
			}
		}

		private void RebuildAndNotify(MordexRow mordexRow)
		{
			if (_indexSuspendCount == 0)
			{
				Refresh();
				GetOwner().HandleRowManagerChanged(this);
			}
		}

		internal void ClearOrAssignRowPositions(bool boolean)
		{
			if (boolean)
			{
				foreach (MordexRow item in _rows)
				{
					item.SetRowManagerPosition(-1);
				}
				return;
			}
			RenumberFrom(0);
		}

		private void RenumberFrom(int start)
		{
			for (int i = start; i < _rows.Count; i++)
			{
				_rows[i].SetRowManagerPosition(i);
			}
		}

		private int RebuildAfterRowAdded(MordexRow mordexRow)
		{
			if (_indexSuspendCount == 0 && !TryInsertAddedRow(mordexRow))
			{
				Refresh();
			}
			return -1;
		}

		// Inserts the rows of an added row at the place a full rebuild would put them.
		// Returns false when the place cannot be told from the neighbours.
		private bool TryInsertAddedRow(MordexRow row)
		{
			if (_rows == null || _root == null)
			{
				return false;
			}
			MordexRow parent = row.ParentRow;
			if (parent == null)
			{
				return false;
			}
			MordexRow top = null;
			for (MordexRow ancestor = parent; ancestor != null; ancestor = ancestor.ParentRow)
			{
				if (!ancestor.Visible || !ancestor.Expanded)
				{
					return true;
				}
				top = ancestor;
			}
			if (top != _root)
			{
				return true;
			}
			List<MordexRow> added = new List<MordexRow>();
			row.FillList(added, childRowsOnly: false);
			if (added.Count == 0)
			{
				return true;
			}
			foreach (MordexRow item in added)
			{
				if (item.RowManagerPosition != -1)
				{
					return false;
				}
			}
			int index = FindPositionAfter(row);
			if (index < 0 || index > _rows.Count)
			{
				return false;
			}
			_rows.InsertRange(index, added);
			RenumberFrom(index);
			return true;
		}

		// Position of the first listed row that follows the output of the given row; -1 when unknown.
		private int FindPositionAfter(MordexRow row)
		{
			MordexRow current = row;
			while (true)
			{
				MordexRow parent = current.ParentRow;
				if (parent == null)
				{
					return -1;
				}
				if (!parent.HasRowCollection())
				{
					return -1;
				}
				MordexRowCollection siblings = parent.Rows;
				int count = siblings.Count;
				int at = count > 0 && siblings[count - 1] == current ? count - 1 : siblings.IndexOf(current);
				if (at < 0)
				{
					return -1;
				}
				for (int i = at + 1; i < count; i++)
				{
					MordexRow next = siblings[i];
					if (next.Visible)
					{
						return next.IsVirtualRow() ? -1 : next.RowManagerPosition;
					}
				}
				MordexRow[] post = parent.GetPostListRowsInternal();
				if (post != null)
				{
					foreach (MordexRow item in post)
					{
						if (item.Visible)
						{
							return item.IsVirtualRow() ? -1 : item.RowManagerPosition;
						}
					}
				}
				if (parent == _root)
				{
					return _rows.Count;
				}
				current = parent;
			}
		}

		private void RemoveRowAndDescendants(MordexRow row)
		{
			if (_indexSuspendCount != 0)
			{
				return;
			}
			int rowManagerPosition = row.RowManagerPosition;
			if (rowManagerPosition >= 0)
			{
				int end = rowManagerPosition;
				while (end < Count && (_rows[end] == row || _rows[end].IsDescendentOf(row)))
				{
					end++;
				}
				for (int i = rowManagerPosition; i < end; i++)
				{
					_rows[i].SetRowManagerPosition(-1);
				}
				_rows.RemoveRange(rowManagerPosition, end - rowManagerPosition);
				RenumberFrom(rowManagerPosition);
				GetOwner().HandleRowManagerChanged(this);
			}
		}

		private void ApplyExpandedChange(MordexRow row)
		{
			if (_indexSuspendCount != 0 || !row.IsRowVisible())
			{
				return;
			}
			int insertIndex = row.RowManagerPosition + 1;
			if (row.Expanded)
			{
				List<MordexRow> list = new List<MordexRow>();
				row.FillList(list, childRowsOnly: true);
				List<MordexRow> added = list.FindAll(candidate => candidate.RowManagerPosition == -1);
				_rows.InsertRange(insertIndex, added);
				RenumberFrom(insertIndex);
			}
			else if (!TryReplaceSubtree(row))
			{
				Refresh();
			}
			GetOwner().HandleRowManagerChanged(this);
		}

		// Replaces the listed rows of a collapsed row and its descendants with the rows it lists now.
		private bool TryReplaceSubtree(MordexRow row)
		{
			int position = row.RowManagerPosition;
			if (position < 0 || position >= _rows.Count || _rows[position] != row)
			{
				return false;
			}
			List<MordexRow> now = new List<MordexRow>();
			row.FillList(now, childRowsOnly: false);
			if (now.Count == 0 || now[0] != row)
			{
				return false;
			}
			int end = position + 1;
			while (end < _rows.Count && _rows[end].IsDescendentOf(row))
			{
				end++;
			}
			for (int i = position + 1; i < end; i++)
			{
				_rows[i].SetRowManagerPosition(-1);
			}
			_rows.RemoveRange(position + 1, end - position - 1);
			if (now.Count > 1)
			{
				_rows.InsertRange(position + 1, now.GetRange(1, now.Count - 1));
			}
			RenumberFrom(position + 1);
			return true;
		}

		public void SuspendIndex()
		{
			if (GetOwner() != null)
			{
				if (_indexSuspendCount == 0)
				{
					GetOwner().RaiseSuspendRowManagerIndex();
				}
				_indexSuspendCount++;
			}
		}

		public void ResumeIndex()
		{
			if (GetOwner() != null)
			{
				_indexSuspendCount--;
				if (_indexSuspendCount < 0)
				{
					throw new Exception("ResumeIndex was called without a matching SuspendIndex call.");
				}
				if (_indexSuspendCount == 0)
				{
					Refresh();
					GetOwner().RaiseResumeRowManagerIndex();
					GetOwner().HandleRowManagerChanged(this);
				}
			}
		}
	}
}
