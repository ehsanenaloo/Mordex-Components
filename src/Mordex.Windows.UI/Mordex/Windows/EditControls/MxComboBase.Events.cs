using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBase
	{
		[Description("Event fired when the ReadOnly property has changed.")]
		public event EventHandler ReadOnlyChanged
		{
			add
			{
				base.Events.AddHandler(_readOnlyChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_readOnlyChangedKey, value);
			}
		}

		[Description("Event fired when the drop down list is about to be displayed.")]
		public event CancelEventHandler DroppingDown
		{
			add
			{
				base.Events.AddHandler(_droppingDownKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_droppingDownKey, value);
			}
		}

		[Description("Event fired when the drop down list is closed.")]
		public event EventHandler Closed
		{
			add
			{
				base.Events.AddHandler(_closedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_closedKey, value);
			}
		}

		[Description("Event fired when the drop-down list is shown.")]
		public event EventHandler DropDown
		{
			add
			{
				EventHandler eventHandler = _dropDownHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dropDownHandler, updatedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _dropDownHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dropDownHandler, updatedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			if (base.Events[_readOnlyChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnDroppingDown(CancelEventArgs e)
		{
			if (base.Events[_droppingDownKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnClosed(EventArgs e)
		{
			if (base.Events[_closedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnDropDown(EventArgs e)
		{
			if (_dropDownHandler != null)
			{
				_dropDownHandler(this, e);
			}
		}
	}
}
