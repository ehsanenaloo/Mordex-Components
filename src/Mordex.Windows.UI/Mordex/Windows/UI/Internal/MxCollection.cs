using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Internal
{
	[DefaultMember("Item")]
	internal class MxCollection : CollectionBase
	{
		private ILayoutItemOwner _itemOwner;

		private Control _parentControl;

		internal MxCollection(Control parentControl)
		{
			if (parentControl is ILayoutItemOwner)
			{
				_itemOwner = parentControl as ILayoutItemOwner;
			}
			else
			{
				_parentControl = parentControl;
			}
		}

		private void AttachOwner(FormatStyleLayoutState item)
		{
			if (_itemOwner != null)
			{
				item._owner = _itemOwner;
			}
			else if (item.GetParentControl() == null)
			{
				item.SetParentControl(_parentControl);
			}
		}

		public void AddItem(FormatStyleLayoutState uiFormatStyleLayoutState)
		{
			AttachOwner(uiFormatStyleLayoutState);
			uiFormatStyleLayoutState._index = base.List.Count;
			base.List.Add(uiFormatStyleLayoutState);
		}

		public void InsertItem(int index, FormatStyleLayoutState uiFormatStyleLayoutState)
		{
			if (index > base.List.Count)
			{
				index = base.List.Count;
			}
			AttachOwner(uiFormatStyleLayoutState);
			base.List.Insert(index, uiFormatStyleLayoutState);
			RenumberItems();
		}

		public int IndexOfItem(FormatStyleLayoutState uiFormatStyleLayoutState)
		{
			return base.List.IndexOf(uiFormatStyleLayoutState);
		}

		public void RemoveItem(FormatStyleLayoutState uiFormatStyleLayoutState)
		{
			if (base.List.Contains(uiFormatStyleLayoutState))
			{
				uiFormatStyleLayoutState.SetParentControl(null);
				uiFormatStyleLayoutState._owner = null;
				uiFormatStyleLayoutState._index = -1;
				base.List.Remove(uiFormatStyleLayoutState);
			}
		}

		public void ClearItems()
		{
			foreach (FormatStyleLayoutState item in base.List)
			{
				if (item is CommandLayoutState uiCommandLayoutState && uiCommandLayoutState.GetCommand() != null)
				{
					uiCommandLayoutState.SetParentControl(null);
					uiCommandLayoutState.GetCommand()._owner = null;
				}
			}
			base.List.Clear();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			if (value is CommandLayoutState uiCommandLayoutState && uiCommandLayoutState.GetCommand() != null)
			{
				uiCommandLayoutState.GetCommand()._owner = null;
			}
			RenumberItems();
		}

		private void RenumberItems()
		{
			for (int i = 0; i < base.Count; i++)
			{
				((FormatStyleLayoutState)base.InnerList[i])._index = i;
			}
		}

		public CommandLayoutState GetCommandItemAt(int index)
		{
			return (CommandLayoutState)base.List[index];
		}

		public CommandLayoutState FindCommandItem(MxCommand command)
		{
			foreach (CommandLayoutState item in base.List)
			{
				if (item.GetCommand().Equals(command))
				{
					return item;
				}
			}
			return null;
		}

		public TabPageRenderer FindTabPageItem(MxTabPage tabPage)
		{
			foreach (TabPageRenderer item in base.List)
			{
				if (item.GetTabPage().Equals(tabPage))
				{
					return item;
				}
			}
			return null;
		}

		public FormatStyleLayoutState FindItemByIndex(int index)
		{
			foreach (FormatStyleLayoutState item in base.List)
			{
				if (item._index == index)
				{
					return item;
				}
			}
			return (FormatStyleLayoutState)base.List[index];
		}

		public bool ContainsItem(FormatStyleLayoutState uiFormatStyleLayoutState)
		{
			return base.List.Contains(uiFormatStyleLayoutState);
		}
	}
}
