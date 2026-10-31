using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Accessibility;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class MxDataGrid
	{
		protected override void OnGotFocus(EventArgs e)
		{
			ApplyGotFocus();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			ApplyLostFocus();
		}

		protected override void OnEnter(EventArgs e)
		{
			if (AutoEdit && _currentColumn != null && EditMode == EditMode.EditOff && IsCellVisible(Row, CurrentColumn))
			{
				_isEnteringForEdit = true;
				BeginCellEdit(false, '\0', false);
				_isEnteringForEdit = false;
			}
			if (_gridColumnContainerControl == null || !_gridColumnContainerControl.Visible)
			{
				base.OnGotFocus(e);
			}
			else if (_gridColumnContainerControl.GetActiveEditControl() != null)
			{
				_gridColumnContainerControl.GetActiveEditControl().Select();
			}
			else
			{
				_gridColumnContainerControl.Select();
			}
			base.OnEnter(e);
			InvalidateSelectedRows();
		}

		protected override void OnLeave(EventArgs e)
		{
			if (!_editorHadFocus)
			{
				if (_gridColumnContainerControl == null || _gridColumnContainerControl.Visible)
				{
					base.OnLostFocus(e);
				}
				base.OnLeave(e);
				InvalidateSelectedRows();
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			NativeMethods.RemoveProp(base.Handle, "PROP_ISGRIDEXWINDOW");
			base.OnHandleDestroyed(e);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			NativeMethods.SetProp(base.Handle, "PROP_ISGRIDEXWINDOW", new IntPtr(1));
			EnsureVisible();
		}

		private void OnStateGotFocus(object sender, EventArgs eventArgs)
		{
			ApplyGotFocus();
		}

		private void OnStateLostFocus(object sender, EventArgs eventArgs)
		{
			ApplyLostFocus();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			RefreshRowCountAndInvalidate();
		}

		internal void AddInternalChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _resetDataSourceHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _resetDataSourceHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		internal void RemoveInternalChangedHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _resetDataSourceHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _resetDataSourceHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected virtual void OnFirstChange(RowActionEventArgs e)
		{
			if (base.Events[_eventKeyFirstChange] is RowActionEventHandler rowActionEventHandler)
			{
				rowActionEventHandler(this, e);
			}
		}

		protected virtual void OnError(ErrorEventArgs e)
		{
			if (base.Events[_eventKeyError] is ErrorEventHandler errorEventHandler)
			{
				errorEventHandler(this, e);
			}
		}

		protected virtual void OnGroupsChanging(GroupsChangingEventArgs e)
		{
			if (base.Events[_eventKeyGroupsChanging] is GroupsChangingEventHandler groupsChangingEventHandler)
			{
				groupsChangingEventHandler(this, e);
			}
		}

		protected virtual void OnGroupsChanged(GroupsChangedEventArgs e)
		{
			if (base.Events[_eventKeyGroupsChanged] is GroupsChangedEventHandler groupsChangedEventHandler)
			{
				groupsChangedEventHandler(this, e);
			}
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			NotifyRefreshListeners();
		}

		protected override void OnVisualStyleAreaChanged()
		{
			base.OnVisualStyleAreaChanged();
			_tableHeaderFormatStyle.ApplyPredefinedColors();
			_rowWithErrorsFormatStyle.ApplyPredefinedColors();
			_linkFormatStyle.ApplyPredefinedColors();
			_alternatingRowFormatStyle.ApplyPredefinedColors();
			_cardCaptionFormatStyle.ApplyPredefinedColors();
			_cardColumnHeaderFormatStyle.ApplyPredefinedColors();
			_filterRowFormatStyle.ApplyPredefinedColors();
			_groupByBoxFormatStyle.ApplyPredefinedColors();
			_groupByBoxInfoFormatStyle.ApplyPredefinedColors();
			_groupRowFormatStyle.ApplyPredefinedColors();
			_groupTotalRowFormatStyle.ApplyPredefinedColors();
			_headerFormatStyle.ApplyPredefinedColors();
			_rowHeaderFormatStyle.ApplyPredefinedColors();
			_previewRowFormatStyle.ApplyPredefinedColors();
			_rowFormatStyle.ApplyPredefinedColors();
			_selectedFormatStyle.ApplyPredefinedColors();
			_selectedInactiveFormatStyle.ApplyPredefinedColors();
			_totalRowFormatStyle.ApplyPredefinedColors();
			_focusCellFormatStyle.ApplyPredefinedColors();
			RefreshRowCountAndInvalidate();
		}

		internal void OnGroupComputeStarted()
		{
		}

		internal void OnGroupComputeEnded()
		{
			MarkTablesForAutoResize();
		}

		internal void RaiseGroupsChanging(GroupsChangingEventArgs eventArgs)
		{
			OnGroupsChanging(eventArgs);
		}

		internal void RaiseGroupsChanged(GroupsChangedEventArgs eventArgs)
		{
			OnGroupsChanged(eventArgs);
		}
	}
}
