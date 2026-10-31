using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBox
	{
		protected virtual void OnDataSourceChanged(EventArgs e)
		{
			if (base.Events[_dataSourceChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		private void BindDataSource(object dataSource, BindingMemberInfo dataMemberInfo, bool forceRebind)
		{
			if (IsDataSourceEmpty())
			{
				SetDataSourceEmpty(dataSource == null);
			}
			bool isDataSourceChanged = _dataSource != dataSource;
			bool isDataMemberChanged = !_dataMemberInfo.Equals(dataMemberInfo);
			if (forceRebind || isDataSourceChanged || isDataMemberChanged)
			{
				if (_dataSource is IComponent)
				{
					((IComponent)_dataSource).Disposed -= OnDataSourceDisposed;
				}
				_dataSource = dataSource;
				_dataMemberInfo = dataMemberInfo;
				if (_dataSource is IComponent)
				{
					((IComponent)_dataSource).Disposed += OnDataSourceDisposed;
				}
				CurrencyManager currencyManager = null;
				if (dataSource != null && _hasBindingContext && GetBindingContext() != null && dataSource != Convert.DBNull)
				{
					currencyManager = (CurrencyManager)GetBindingContext()[dataSource, dataMemberInfo.BindingMember];
				}
				if (_currencyManager != currencyManager)
				{
					_currencyManager = currencyManager;
				}
				if (isDataSourceChanged || isDataMemberChanged || forceRebind)
				{
					SetActiveCurrencyManager(_currencyManager);
				}
			}
		}

		internal bool SupportsListChangeNotification()
		{
			if (GetActiveCurrencyManager().List == null)
			{
				return false;
			}
			if (!typeof(IBindingList).IsAssignableFrom(GetActiveCurrencyManager().List.GetType()))
			{
				return false;
			}
			return ((IBindingList)GetActiveCurrencyManager().List).SupportsChangeNotification;
		}

		internal CurrencyManager GetActiveCurrencyManager()
		{
			return _activeCurrencyManager;
		}

		internal void SetActiveCurrencyManager(CurrencyManager currencyManager)
		{
			if (_activeCurrencyManager == currencyManager)
			{
				return;
			}
			if (SelectedItem == null && !base.DroppedDown)
			{
				base.Text = "";
			}
			UnsubscribeFromCurrencyManager();
			_activeCurrencyManager = currencyManager;
			if (_activeCurrencyManager != null)
			{
				if (_baseItems != null)
				{
					_baseItems._owner = this;
				}
				_boundList = GetActiveCurrencyManager().List;
			}
			else
			{
				_boundList = null;
			}
			ReloadItems();
			SubscribeToCurrencyManager();
		}

		internal void ReloadItems()
		{
			Items.DetachAllItems();
			if (GetActiveCurrencyManager() == null)
			{
				if (SelectedItem != null && !Items.Contains(SelectedItem.Value))
				{
					_selectedItem = null;
				}
				return;
			}
			try
			{
				_baseItems.RestoreOriginalOrder();
				for (int i = 0; i < GetActiveCurrencyManager().Count; i++)
				{
					object row = _activeCurrencyManager.List[i];
					if (row is DataRowView)
					{
						DataRowView dataRowView = (DataRowView)row;
						if (dataRowView.IsNew)
						{
							continue;
						}
					}
					ComboBoxItem uiComboBoxItem = new ComboBoxItem(GetActiveCurrencyManager().List[i]);
					_baseItems.AddItemUnchecked(uiComboBoxItem);
				}
				if (_sorted)
				{
					_baseItems.Sort();
				}
				if (_baseItems.Count > 0)
				{
					if (SelectedValue != null)
					{
						ComboBoxItem valueItem = Items[_selectedValue];
						if (_selectedItem == null)
						{
							if (valueItem == null && SelectInDataSource)
							{
								valueItem = _baseItems[0];
							}
							else
							{
								_raiseSelectionEvents = false;
							}
							SelectedItem = valueItem;
							_raiseSelectionEvents = true;
						}
						else
						{
							_selectedItem = valueItem;
							if (valueItem != null)
							{
								base.Text = valueItem.Text;
							}
							else
							{
								base.Text = "";
							}
							OnSelectedItemChanged(EventArgs.Empty);
							OnSelectedIndexChanged(EventArgs.Empty);
							OnSelectedValueChanged(EventArgs.Empty);
						}
					}
					else if (SelectInDataSource)
					{
						SelectedItem = _baseItems[0];
					}
					else
					{
						base.Text = "";
					}
				}
				if (_formatStyleRenderer != null && _formatStyleRenderer.GetIsDropDownOpen())
				{
					_formatStyleRenderer.FitToItems(base.ClientRectangle.Width);
				}
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// A data source that fails while binding is dropped, so the combo stays usable without it.
				SetActiveCurrencyManager(null);
			}
		}

		private void OnControlItemChanged(object sender, ItemChangedEventArgs eventArgs)
		{
			if (base.InvokeRequired)
			{
				PostToOwnerThread(delegate
				{
					HandleItemChanged(eventArgs);
				});
			}
			else
			{
				HandleItemChanged(eventArgs);
			}
		}

		private void SyncPositionWithSelection()
		{
			if (!_selectInDataSource || _currencyManager == null)
			{
				return;
			}
			if (_selectedItem != null)
			{
				if (_sorted)
				{
					CurrencyManager.Position = _selectedItem._originalIndex;
				}
				else
				{
					CurrencyManager.Position = _selectedItem.Index;
				}
			}
			else if (CurrencyManager.Count > 0)
			{
				CurrencyManager.Position = 0;
			}
			else
			{
				CurrencyManager.Position = -1;
			}
		}

		private void OnControlPositionChanged(object sender, EventArgs eventArgs)
		{
			if (_selectInDataSource)
			{
				ComboBoxItem uiComboBoxItem = Items.GetItemAt(GetActiveCurrencyManager().Position, Sorted);
				if (uiComboBoxItem != null)
				{
					SelectedItem = uiComboBoxItem;
				}
			}
		}

		internal object GetSourceObject(ComboBoxItem uiComboBoxItem)
		{
			if (_activeCurrencyManager != null)
			{
				if (Items._isSorted)
				{
					return _activeCurrencyManager.List[uiComboBoxItem._originalIndex];
				}
				return _activeCurrencyManager.List[uiComboBoxItem._listIndex];
			}
			return null;
		}

		private void HandleItemChanged(ItemChangedEventArgs eventArgs)
		{
			if (_boundList == null || _activeCurrencyManager == null || !SupportsListChangeNotification() || _boundList != _activeCurrencyManager.List)
			{
				ReloadItems();
			}
		}

		private void HandleListChanged(ListChangedEventArgs eventArgs)
		{
			switch (eventArgs.ListChangedType)
			{
			case ListChangedType.ItemAdded:
			{
				if (Items._isSorted)
				{
					ReloadItems();
					break;
				}
				object addedRow = GetActiveCurrencyManager().List[eventArgs.NewIndex];
				if (addedRow is DataRowView)
				{
					DataRowView dataRowView = (DataRowView)addedRow;
					if (dataRowView.IsNew)
					{
						break;
					}
				}
				ComboBoxItem newItem = new ComboBoxItem(addedRow);
				Items.InsertItemUnchecked(eventArgs.NewIndex, newItem);
				break;
			}
			case ListChangedType.ItemChanged:
				if (SelectedItem != null)
				{
					SelectedValue = SelectedItem.Value;
					base.Text = SelectedItem.Text;
				}
				break;
			case ListChangedType.ItemDeleted:
			{
				if (Items._isSorted)
				{
					ReloadItems();
					break;
				}
				ComboBoxItem deletedItem = FindItemBySelectedIndex(eventArgs.NewIndex);
				if (deletedItem != null)
				{
					Items.RemoveItemUnchecked(deletedItem);
					deletedItem.DataRow = null;
					if (SelectedItem == deletedItem)
					{
						SelectedItem = null;
					}
				}
				break;
			}
			case ListChangedType.ItemMoved:
				if (eventArgs.OldIndex >= 0)
				{
					ComboBoxItem uiComboBoxItem = FindItemBySelectedIndex(eventArgs.OldIndex);
					if (uiComboBoxItem != null)
					{
						Items.MoveItem(uiComboBoxItem, eventArgs.NewIndex);
					}
					break;
				}
				goto case ListChangedType.ItemAdded;
			case ListChangedType.Reset:
				ReloadItems();
				break;
			}
		}

		private void OnControlListChanged(object sender, ListChangedEventArgs eventArgs)
		{
			if (base.InvokeRequired)
			{
				// The posted call runs later, when the indexes in eventArgs may be stale,
				// so rebuild the items from the list as it is then.
				PostToOwnerThread(delegate
				{
					HandleListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
				});
			}
			else
			{
				HandleListChanged(eventArgs);
			}
		}

		// Data source events can come from a worker thread. Waiting for the UI thread
		// there can deadlock, so the work is posted and runs later on the UI thread.
		private void PostToOwnerThread(Action work)
		{
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			Action guardedWork = delegate
			{
				if (!base.IsDisposed)
				{
					work();
				}
			};
			try
			{
				BeginInvoke(guardedWork);
			}
			catch (InvalidOperationException)
			{
				// The window was destroyed meanwhile; there is nothing left to update.
			}
		}

		private ComboBoxItem FindItemBySelectedIndex(int selectedIndex)
		{
			foreach (ComboBoxItem item in Items)
			{
				if (item._listIndex == selectedIndex)
				{
					return item;
				}
			}
			return null;
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			UpdateBindingContext(BindingContext);
		}

		private BindingContext GetBindingContext()
		{
			return _bindingContext;
		}

		private void UpdateBindingContext(BindingContext bindingContext)
		{
			if (_bindingContext != bindingContext)
			{
				_bindingContext = bindingContext;
				_hasBindingContext = true;
				BindDataSource(DataSource, _dataMemberInfo, true);
			}
		}

		private string ConvertValueToText(object value)
		{
			if (value != null)
			{
				try
				{
					Type type = value.GetType();
					object[] customAttributes = type.GetCustomAttributes(typeof(TypeConverterAttribute), inherit: true);
					if (customAttributes.Length > 0)
					{
						_ = (TypeConverterAttribute)customAttributes[0];
						Type converterType = Type.GetType(((TypeConverterAttribute)customAttributes[0]).ConverterTypeName);
						TypeConverter typeConverter = (TypeConverter)Activator.CreateInstance(converterType, nonPublic: false);
						if (typeConverter.CanConvertTo(typeof(string)))
						{
							return (string)typeConverter.ConvertTo(null, CultureInfo.CurrentCulture, value, typeof(string));
						}
					}
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// A broken custom converter on the value type falls back to ToString.
				}
				return value.ToString();
			}
			return "";
		}

		internal string GetDisplayText(object component)
		{
			if (DisplayMember != "" && GetActiveCurrencyManager() != null)
			{
				PropertyDescriptorCollection itemProperties = GetActiveCurrencyManager().GetItemProperties();
				PropertyDescriptor propertyDescriptor = itemProperties.Find(DisplayMember, ignoreCase: true);
				if (propertyDescriptor == null)
				{
					return ConvertValueToText(component);
				}
				object displayValue = null;
				if (component != null)
				{
					displayValue = propertyDescriptor.GetValue(component);
				}
				if (displayValue == DBNull.Value)
				{
					return "";
				}
				if (displayValue != null)
				{
					return displayValue.ToString();
				}
			}
			else if (component != null)
			{
				return ConvertValueToText(component);
			}
			return "";
		}

		internal object GetMemberValue(object component)
		{
			if (ValueMember != "" && GetActiveCurrencyManager() != null)
			{
				PropertyDescriptorCollection itemProperties = GetActiveCurrencyManager().GetItemProperties();
				PropertyDescriptor propertyDescriptor = itemProperties.Find(ValueMember, ignoreCase: true);
				if (propertyDescriptor != null)
				{
					object memberValue = null;
					if (component != null)
					{
						memberValue = propertyDescriptor.GetValue(component);
					}
					if (memberValue == DBNull.Value)
					{
						return null;
					}
					return memberValue;
				}
				return null;
			}
			return component;
		}

		private void UnsubscribeFromCurrencyManager()
		{
			if (_activeCurrencyManager != null)
			{
				// The manager's list can change after subscribing (a related list does), so use the subscribed one.
				if (_itemList is IBindingList subscribedList)
				{
					subscribedList.ListChanged -= OnControlListChanged;
				}
				_itemList = null;
				GetActiveCurrencyManager().ItemChanged -= OnControlItemChanged;
				GetActiveCurrencyManager().PositionChanged -= OnControlPositionChanged;
			}
		}

		private void SubscribeToCurrencyManager()
		{
			if (_activeCurrencyManager != null)
			{
				_itemList = GetActiveCurrencyManager().List;
				if (SupportsListChangeNotification())
				{
					((IBindingList)GetActiveCurrencyManager().List).ListChanged += OnControlListChanged;
				}
				GetActiveCurrencyManager().ItemChanged += OnControlItemChanged;
				GetActiveCurrencyManager().PositionChanged += OnControlPositionChanged;
			}
		}

		private void OnDataSourceDisposed(object sender, EventArgs eventArgs)
		{
			BindDataSource(null, new BindingMemberInfo(""), true);
		}
	}
}
