using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Data
{
	internal class DataBindableList : BindableList
	{
		private CurrencyManager _currencyManager;

		public override event ListChangedEventHandler ListChanged
		{
			add
			{
				if (_currencyManager != null && _currencyManager.List != null && _currencyManager.List is IBindingList)
				{
					((IBindingList)_currencyManager.List).ListChanged += value;
				}
				else
				{
					_currencyManager.ListChanged += value;
				}
			}
			remove
			{
				if (_currencyManager != null && _currencyManager.List != null && _currencyManager.List is IBindingList)
				{
					((IBindingList)_currencyManager.List).ListChanged -= value;
				}
				else
				{
					_currencyManager.ListChanged -= value;
				}
			}
		}

		internal DataBindableList(CurrencyManager currencyManager)
			: base(currencyManager.List)
		{
			_currencyManager = currencyManager;
		}
	}
}
