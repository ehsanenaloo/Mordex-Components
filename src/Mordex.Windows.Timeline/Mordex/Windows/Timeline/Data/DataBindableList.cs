using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.Timeline.Data
{
	internal class DataBindableList : BindableList
	{
		private CurrencyManager _currencyManager;

		public override event ListChangedEventHandler ListChanged
		{
			add
			{
				if (_currencyManager != null)
				{
					_currencyManager.ListChanged += value;
				}
			}
			remove
			{
				if (_currencyManager != null)
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
