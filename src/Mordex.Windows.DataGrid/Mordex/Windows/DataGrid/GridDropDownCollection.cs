using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid
{
	public class GridDropDownCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private MxDataGrid _owner;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "DropDown";

		public MxDataGrid DataGrid => _owner;

		public GridDropDown this[int index] => (GridDropDown)base.List[index];

		public GridDropDown this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridDropDown gridDropDown = (GridDropDown)enumerator.Current;
						if (string.Compare(key, gridDropDown.Key, ignoreCase: true) == 0)
						{
							return gridDropDown;
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
			for (PropertyValue propertyValue = info["DropDown" + value]; propertyValue != null; propertyValue = info["DropDown" + value])
			{
				GridDropDown gridDropDown = new GridDropDown();
				base.List.Add(gridDropDown);
				value++;
				propertyValue.DeserializeObject(gridDropDown);
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
					GridDropDown value = (GridDropDown)enumerator.Current;
					info.AddValue("DropDown" + otherValue, value);
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

		internal GridDropDownCollection(MxDataGrid owner)
		{
			_owner = owner;
		}

		public GridDropDown Add()
		{
			return Add("");
		}

		public GridDropDown Add(string key)
		{
			GridDropDown gridDropDown = new GridDropDown();
			gridDropDown.Key = key;
			AddToList(gridDropDown);
			return gridDropDown;
		}

		internal void AddToList(GridDropDown gridDropDown)
		{
			base.List.Add(gridDropDown);
		}

		public void AddRange(GridDropDown[] dropDowns)
		{
			for (int i = 0; i < dropDowns.Length; i++)
			{
				base.List.Add(dropDowns[i]);
			}
		}

		protected override void OnInsert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			GridDropDown gridDropDown = (GridDropDown)value;
			if (gridDropDown._ownerCollection != null)
			{
				Mordex.Windows.Common.MxMessageBox.Show("ownercollection is not null");
				throw new InvalidOperationException();
			}
			if (gridDropDown._dropListOwner is MxMultiColumnComboBox)
			{
				Mordex.Windows.Common.MxMessageBox.Show("owner is combo");
				throw new InvalidOperationException();
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			GridDropDown gridDropDown = (GridDropDown)value;
			gridDropDown.Location = new Point(-10000, -10000);
			_owner.Controls.Add(gridDropDown);
			gridDropDown._ownerCollection = this;
			gridDropDown.UpdateBindingContext(_owner.GetBindingContext());
		}

		public void Remove(GridDropDown dropDown)
		{
			base.List.Remove(dropDown);
		}

		public int IndexOf(GridDropDown dropDown)
		{
			return base.List.IndexOf(dropDown);
		}

		protected override void OnClear()
		{
			if (_owner != null)
			{
				for (int value = base.Count - 1; value == 0; value--)
				{
					GridDropDown gridDropDown = this[value];
					gridDropDown._ownerCollection = null;
					gridDropDown.Dispose();
				}
			}
		}

		protected override void OnClearComplete()
		{
			if (_owner != null)
			{
				_owner.ClearOrphanRootTableDropDowns();
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			if (_owner != null)
			{
				GridDropDown gridDropDown = (GridDropDown)value;
				gridDropDown._ownerCollection = null;
				gridDropDown.Dispose();
				_owner.ClearOrphanRootTableDropDowns();
			}
		}

		public bool Contains(string key)
		{
			foreach (GridDropDown item in base.List)
			{
				if (string.Compare(item.Key, key, ignoreCase: true) == 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
