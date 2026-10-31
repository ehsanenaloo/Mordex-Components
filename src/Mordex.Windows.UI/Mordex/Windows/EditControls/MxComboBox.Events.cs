using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBox
	{
		[Description("Event fired when the DatesSource property of the UIComboBox has changed.")]
		public event EventHandler DataSourceChanged
		{
			add
			{
				base.Events.AddHandler(_dataSourceChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dataSourceChangedKey, value);
			}
		}

		[Description("Event fired when the SelectedItem property has changed.")]
		public event EventHandler SelectedItemChanged
		{
			add
			{
				EventHandler eventHandler = _selectedItemChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedItemChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _selectedItemChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedItemChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		[Description("Event fired when the SelectedIndex property has changed.")]
		public event EventHandler SelectedIndexChanged
		{
			add
			{
				base.Events.AddHandler(_selectedIndexChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedIndexChangedKey, value);
			}
		}

		[Description("Event fired when the SelectedValue property has changed.")]
		public event EventHandler SelectedValueChanged
		{
			add
			{
				EventHandler eventHandler = _selectedValueChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedValueChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _selectedValueChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedValueChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		protected virtual void OnSelectedItemChanged(EventArgs e)
		{
			if (_selectedItemChangedHandler != null)
			{
				_selectedItemChangedHandler(this, e);
			}
		}

		protected virtual void OnSelectedIndexChanged(EventArgs e)
		{
			if (base.Events[_selectedIndexChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnSelectedValueChanged(EventArgs e)
		{
			if (_selectedValueChangedHandler != null)
			{
				_selectedValueChangedHandler(this, e);
			}
		}
	}
}
