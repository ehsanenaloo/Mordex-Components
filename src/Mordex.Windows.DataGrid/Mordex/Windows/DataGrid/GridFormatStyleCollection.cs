using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class GridFormatStyleCollection : CollectionBase, IFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		internal MxDataGrid _owner;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Style";

		public GridFormatStyle this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridFormatStyle gridFormatStyle = (GridFormatStyle)enumerator.Current;
						if (gridFormatStyle.Key == key)
						{
							return gridFormatStyle;
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

		public GridFormatStyle this[int index] => (GridFormatStyle)base.InnerList[index];

		MxDataGrid IFormatStyleOwner.DataGrid => _owner;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int value = 0;
			for (PropertyValue propertyValue = info["Style" + value]; propertyValue != null; propertyValue = info["Style" + value])
			{
				GridFormatStyle gridFormatStyle = new GridFormatStyle();
				base.InnerList.Add(gridFormatStyle);
				gridFormatStyle._owner = this;
				value++;
				propertyValue.DeserializeObject(gridFormatStyle);
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
					GridFormatStyle value = (GridFormatStyle)enumerator.Current;
					info.AddValue("Style" + otherValue, value);
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

		internal GridFormatStyleCollection(MxDataGrid owner)
		{
			_owner = owner;
		}

		public GridFormatStyle Add()
		{
			GridFormatStyle gridFormatStyle = new GridFormatStyle();
			Add(gridFormatStyle);
			return gridFormatStyle;
		}

		internal MxDataGrid GetOwner()
		{
			return _owner;
		}

		public void Add(GridFormatStyle formatStyle)
		{
			base.List.Add(formatStyle);
		}

		public void AddRange(GridFormatStyle[] formatStyles)
		{
			foreach (GridFormatStyle value in formatStyles)
			{
				base.List.Add(value);
			}
		}

		protected override void OnInsert(int index, object value)
		{
			if (!(value is GridFormatStyle))
			{
				throw new InvalidOperationException();
			}
			GridFormatStyle gridFormatStyle = (GridFormatStyle)value;
			if (gridFormatStyle._owner != null)
			{
				throw new InvalidOperationException();
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			((GridFormatStyle)value)._owner = this;
		}

		public void Remove(GridFormatStyle formatStyle)
		{
			base.List.Remove(formatStyle);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			GridFormatStyle gridFormatStyle = (GridFormatStyle)value;
			gridFormatStyle._owner = null;
			GetOwner().ClearUnusedRootTableStyles();
			NotifyOwnerChanged();
		}

		protected override void OnClear()
		{
			base.OnClear();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridFormatStyle gridFormatStyle = (GridFormatStyle)enumerator.Current;
					gridFormatStyle._owner = null;
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
			GetOwner().ClearUnusedRootTableStyles();
			NotifyOwnerChanged();
		}

		public int IndexOf(GridFormatStyle formatStyle)
		{
			return base.InnerList.IndexOf(formatStyle);
		}

		internal void NotifyOwnerChanged()
		{
			if (_owner != null)
			{
				_owner.RefreshRowCountAndInvalidate();
			}
		}

		void IFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			NotifyOwnerChanged();
		}
	}
}
