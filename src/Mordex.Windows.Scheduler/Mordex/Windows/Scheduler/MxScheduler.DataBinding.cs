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
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		internal bool IsFieldBound(AppointmentChange appointmentChange)
		{
			if (_schedulerCurrencyManager != null)
			{
				switch (appointmentChange)
				{
				case AppointmentChange.EndTime:
					if (_endTimeDescriptor != null)
					{
						return true;
					}
					return false;
				case AppointmentChange.StartTime:
					if (_schedulerPropertyDescriptor != null)
					{
						return true;
					}
					return false;
				case AppointmentChange.Owner:
					if (_propertyDescriptor != null)
					{
						return true;
					}
					return false;
				case AppointmentChange.Text:
					if (_textDescriptor != null)
					{
						return true;
					}
					return false;
				case AppointmentChange.Description:
					if (_descriptionDescriptor != null)
					{
						return true;
					}
					return false;
				case AppointmentChange.RecurrencePattern:
					if (_recurrenceInfoDescriptor != null)
					{
						return true;
					}
					return false;
				default:
					return false;
				}
			}
			return false;
		}

		private void RemoveFromDataSource(SchedulerAppointment schedulerAppointment)
		{
			if (_isAppointmentDragActive || _schedulerCurrencyManager == null)
			{
				return;
			}
			_shouldHandleItemDeleted = false;
			try
			{
				object dataItem = ((schedulerAppointment._dataRow == null) ? _schedulerCurrencyManager.List[schedulerAppointment._index] : schedulerAppointment._dataRow);
				if (dataItem is IEditableObject)
				{
					((IEditableObject)dataItem).BeginEdit();
				}
				_schedulerCurrencyManager.RemoveAt(schedulerAppointment._index);
				if (dataItem is IEditableObject)
				{
					((IEditableObject)dataItem).EndEdit();
				}
			}
			catch
			{
				_shouldHandleItemDeleted = true;
			}
			schedulerAppointment._index = -1;
			_shouldHandleItemDeleted = true;
		}

		internal void AddToDataSource(SchedulerAppointment item)
		{
			if ((_isAppointmentDragActive && item._recurrenceState != RecurrenceState.Exception && _draggedAppointmentCopies != null && _draggedAppointmentCopies.Contains(item)) || GetCurrencyManager() == null || item.DataRow != null)
			{
				return;
			}
			_shouldHandleItemAdded = false;
			object newItem = null;
			bool isItemFromEvent = false;
			if (!_usesInternalTable)
			{
				GetNewObjectEventArgs e = new GetNewObjectEventArgs();
				OnGetNewObject(e);
				newItem = e.NewObject;
			}
			if (newItem == null)
			{
				try
				{
					_schedulerCurrencyManager.AddNew();
					newItem = _schedulerCurrencyManager.Current;
				}
				catch
				{
					_publicAppointments.RemoveWithoutNotify(item);
					_appointments.RemoveWithoutNotify(item);
					throw;
				}
			}
			else
			{
				isItemFromEvent = true;
			}
			if (_schedulerPropertyDescriptor != null)
			{
				item._dataRow = newItem;
				if (item._index < 0 && item._recurrenceState == RecurrenceState.Exception)
				{
					item._index = _schedulerCurrencyManager.List.Count - 1;
				}
				_schedulerPropertyDescriptor.SetValue(newItem, item.StartTime);
				if (_endTimeDescriptor != null)
				{
					_endTimeDescriptor.SetValue(newItem, item.EndTime);
				}
				if (_textDescriptor != null && !_textDescriptor.IsReadOnly)
				{
					_textDescriptor.SetValue(newItem, item.Text);
				}
				if (_descriptionDescriptor != null && !_descriptionDescriptor.IsReadOnly)
				{
					_descriptionDescriptor.SetValue(newItem, item.Description);
				}
				if (_propertyDescriptor != null && !_propertyDescriptor.IsReadOnly)
				{
					object owner = item.Owner;
					if (owner != null)
					{
						_propertyDescriptor.SetValue(newItem, owner);
					}
				}
				if (_recurrenceInfoDescriptor != null && !_recurrenceInfoDescriptor.IsReadOnly)
				{
					_recurrenceInfoDescriptor.SetValue(newItem, item.RecurrencePatternInfo);
				}
				if (_appointmentSettingsDescriptor != null && !_appointmentSettingsDescriptor.IsReadOnly)
				{
					_appointmentSettingsDescriptor.SetValue(newItem, item.SettingsInfo);
				}
				if (item.HasFieldValues())
				{
					foreach (SchedulerField field in Fields)
					{
						if (!IsBuiltInField(field) && field.IncludeInCopy)
						{
							SetItemValue(newItem, field.DataMember, item.GetValue(field));
						}
					}
					item.SetSchedulerAppointmentValue();
				}
				item.SetDataRowEditStarted(true);
				item._scheduler = this;
			}
			if (isItemFromEvent)
			{
				IList boundList = _schedulerCurrencyManager.List;
				DetachCurrencyManagerEvents();
				_schedulerCurrencyManager.SuspendBinding();
				boundList.Add(newItem);
				_schedulerCurrencyManager.ResumeBinding();
				AttachCurrencyManagerEvents();
				_shouldHandleItemAdded = true;
			}
		}

		private void OnDataSourceListChanged(ListChangedEventArgs eventArgs)
		{
			InvalidateAppointmentStyles();
			switch (eventArgs.ListChangedType)
			{
			case ListChangedType.ItemAdded:
			{
				if (!_shouldHandleItemAdded)
				{
					break;
				}
				object changedItem = _schedulerCurrencyManager.List[eventArgs.NewIndex];
				if (changedItem is DataRowView)
				{
					DataRowView dataRowView = (DataRowView)changedItem;
					if (dataRowView.IsNew)
					{
						break;
					}
				}
				AddAppointmentFromDataSource(eventArgs.NewIndex);
				break;
			}
			case ListChangedType.ItemChanged:
				if (_shouldHandleItemChanged)
				{
					RefreshAppointmentFromDataSource(eventArgs.NewIndex);
				}
				break;
			case ListChangedType.ItemDeleted:
				if (_shouldHandleItemDeleted)
				{
					RemoveAppointmentByDataIndex(eventArgs.NewIndex);
				}
				break;
			case ListChangedType.ItemMoved:
				if (eventArgs.OldIndex >= 0)
				{
					_appointments.MoveAppointment(eventArgs.OldIndex, eventArgs.NewIndex);
					break;
				}
				goto case ListChangedType.ItemAdded;
			case ListChangedType.Reset:
				_isReadingAppointment = true;
				ReloadAppointmentsFromDataSource();
				_isReadingAppointment = false;
				break;
			}
		}

		private void OnDataSourceDisposed(object sender, EventArgs eventArgs)
		{
			DataSource = null;
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			BindDataSource(dataSource, new BindingMemberInfo(dataMember), true);
		}

		private void BindDataSource(object dataSource, BindingMemberInfo bindingMemberInfo, bool forceRebind)
		{
			if (IsUnboundMode())
			{
				SetUnboundMode(dataSource == null);
			}
			if (_schedulerCurrencyManager == null)
			{
				forceRebind = true;
			}
			if (!IsUnboundMode())
			{
				bool isDataSourceChanged = _dataSource != dataSource;
				bool isBindingMemberChanged = !_bindingMemberInfo.Equals(bindingMemberInfo);
				if (forceRebind || isDataSourceChanged || isBindingMemberChanged)
				{
					if (_dataSource is IComponent)
					{
						((IComponent)_dataSource).Disposed -= ResetBindingOnDataSourceDisposed;
					}
					_dataSource = dataSource;
					_bindingMemberInfo = bindingMemberInfo;
					if (_dataSource is IComponent)
					{
						((IComponent)_dataSource).Disposed += ResetBindingOnDataSourceDisposed;
					}
					CurrencyManager currencyManager = null;
					if (dataSource != null && BindingContext != null && dataSource != Convert.DBNull)
					{
						currencyManager = (CurrencyManager)BindingContext[dataSource, bindingMemberInfo.BindingMember];
					}
					if (_currencyManager != currencyManager)
					{
						_currencyManager = currencyManager;
					}
					if (isDataSourceChanged || isBindingMemberChanged || forceRebind)
					{
						SetCurrencyManager(_currencyManager);
					}
				}
				return;
			}
			if (_dataTable == null)
			{
				forceRebind = true;
				UpdateDataTable();
			}
			if (_dataTable == null)
			{
				return;
			}
			if (BindingContext != null)
			{
				if (forceRebind && _dataTable != null)
				{
					SetCurrencyManager((CurrencyManager)BindingContext[_dataTable]);
				}
			}
			else
			{
				BindingContext bindingContext = new BindingContext();
				SetCurrencyManager((CurrencyManager)bindingContext[_dataTable]);
			}
		}

		internal void SetFieldValueByKey(SchedulerAppointment schedulerAppointment, string text, object value)
		{
			if (schedulerAppointment._dataRow != null)
			{
				SetItemValue(schedulerAppointment._dataRow, text, value);
			}
			else if (schedulerAppointment._index >= 0)
			{
				SetItemValue(_schedulerCurrencyManager.List[schedulerAppointment._index], text, value);
			}
		}

		internal void SetFieldValueByIndex(SchedulerAppointment schedulerAppointment, int fieldIndex, object value)
		{
			if (schedulerAppointment._dataRow != null)
			{
				SetItemValue(schedulerAppointment._dataRow, _fields[fieldIndex].DataMember, value);
			}
			else if (schedulerAppointment._index >= 0)
			{
				SetItemValue(_schedulerCurrencyManager.List[schedulerAppointment._index], _fields[fieldIndex].DataMember, value);
			}
		}

		internal void SetFieldValue(SchedulerAppointment schedulerAppointment, SchedulerField schedulerField, object value)
		{
			if (schedulerAppointment._dataRow != null)
			{
				SetItemValue(schedulerAppointment._dataRow, schedulerField.DataMember, value);
			}
			else if (schedulerAppointment._index >= 0)
			{
				SetItemValue(_schedulerCurrencyManager.List[schedulerAppointment._index], schedulerField.DataMember, value);
			}
		}

		internal object GetFieldValue(SchedulerAppointment schedulerAppointment, SchedulerField schedulerField)
		{
			return GetMemberValue(schedulerAppointment, schedulerField.DataMember);
		}

		internal object GetFieldValueByIndex(SchedulerAppointment schedulerAppointment, int fieldIndex)
		{
			try
			{
				return GetMemberValue(schedulerAppointment, _fields[fieldIndex].DataMember);
			}
			catch
			{
				return null;
			}
		}

		internal void SyncDataTableColumn(SchedulerField schedulerField, FieldEditType fieldEditType)
		{
			if (IsUnboundMode() && _dataTable != null)
			{
				switch (fieldEditType)
				{
				case FieldEditType.Add:
					_dataTable.Columns.Add(schedulerField.DataMember, schedulerField.Type);
					break;
				case FieldEditType.Remove:
					_dataTable.Columns.Remove(schedulerField.DataMember);
					break;
				case FieldEditType.Clear:
					UpdateDataTable();
					break;
				case FieldEditType.Change:
					break;
				}
			}
		}

		internal void UpdateDataTable()
		{
			if (_dataTable != null)
			{
				DisposeDataTable();
			}
			_dataTable = new DataTable("UnboundTable");
			_dataTable.Columns.Add("___StartTimeMember", typeof(DateTime));
			_dataTable.Columns.Add("___EndTimeMember", typeof(DateTime));
			_dataTable.Columns.Add("___TextMember", typeof(string));
			_dataTable.Columns.Add("___DescriptionMember", typeof(string));
			_dataTable.Columns.Add("___OwnerMember", typeof(object));
			_dataTable.Columns.Add("___RecurrenceInfoMember", typeof(string));
			_dataTable.Columns.Add("___AppointmentLayoutInfoMember", typeof(string));
			foreach (SchedulerField item in _fields)
			{
				_dataTable.Columns.Add(new DataColumn(item.DataMember, item.Type));
			}
			BindDataSource(null, new BindingMemberInfo(""), true);
		}

		private void DisposeDataTable()
		{
			if (_dataTable != null)
			{
				_dataTable.Dispose();
				_dataTable = null;
				SetCurrencyManager(null);
			}
		}

		private bool IsUserDefinedFieldName(string text)
		{
			if (IsUnboundMode())
			{
				if (text.Equals(GetDescriptionMemberName()))
				{
					return false;
				}
				if (text.Equals(GetTextMemberName()))
				{
					return false;
				}
				if (text.Equals(GetStartTimeMemberName()))
				{
					return false;
				}
				if (text.Equals(GetEndTimeMemberName()))
				{
					return false;
				}
				if (text.Equals(GetOwnerMemberName()))
				{
					return false;
				}
			}
			return true;
		}

		internal string[] GetFieldNames()
		{
			if (GetCurrencyManager() != null)
			{
				ArrayList arrayList = new ArrayList();
				PropertyDescriptorCollection itemProperties = _schedulerCurrencyManager.GetItemProperties();
				foreach (PropertyDescriptor item in itemProperties)
				{
					if (IsUserDefinedFieldName(item.Name))
					{
						arrayList.Add(item.Name);
					}
				}
				string[] fieldNames = new string[arrayList.Count];
				arrayList.CopyTo(fieldNames);
				return fieldNames;
			}
			return new string[0];
		}

		private void UpdateColumnResizeGuide(int x, int integerArgument)
		{
			Rectangle rectangle = new Rectangle(_dayAreaLeft - 4, base.ClientRectangle.Top, GetClientRight() - (_dayAreaLeft - 4), base.ClientRectangle.Height);
			if (rectangle.Contains(x, integerArgument))
			{
				x -= _dayAreaLeft - 4;
				if (x > 0)
				{
					int columnCount = DayColumns;
					if (columnCount <= 0)
					{
						columnCount = _dayLayouts.Count;
					}
					int columnsAreaWidth = GetClientRight() - (_dayAreaLeft - 4);
					int columnWidth = columnsAreaWidth / columnCount;
					int resizedColumnCount = columnsAreaWidth / x;
					if (x > columnWidth)
					{
						resizedColumnCount = columnCount - resizedColumnCount;
					}
					if (resizedColumnCount > _dayLayouts.Count)
					{
						resizedColumnCount = _dayLayouts.Count;
					}
					if (resizedColumnCount <= 0)
					{
						resizedColumnCount++;
					}
					ShowColumnResizeGuides(columnsAreaWidth / resizedColumnCount);
				}
				else
				{
					ShowColumnResizeGuides(-1);
				}
			}
			else
			{
				ShowColumnResizeGuides(-1);
			}
		}

		private void ShowColumnResizeGuides(int columnWidth)
		{
			if (columnWidth == _dayResizeColumnWidth)
			{
				return;
			}
			int i = _dayAreaLeft - 4;
			int guideTop = _dayAreaTop + 2;
			int guideHeight = GetClientBottom() - guideTop;
			Point empty = Point.Empty;
			Point guideBottom = Point.Empty;
			if (_dayResizeColumnWidth != -1)
			{
				for (; i < GetClientRight(); i += _dayResizeColumnWidth)
				{
					Rectangle rc = new Rectangle(i, guideTop, 1, guideTop + guideHeight);
					Invalidate(rc, invalidateChildren: true);
				}
			}
			_dayResizeColumnWidth = columnWidth;
			i = _dayAreaLeft - 4;
			if (_dayResizeColumnWidth == -1)
			{
				return;
			}
			for (; i < GetClientRight(); i += _dayResizeColumnWidth)
			{
				empty = PointToScreen(new Point(i, guideTop));
				guideBottom = PointToScreen(new Point(i, guideTop + guideHeight));
				if (i > _dayAreaLeft - 4 && i <= base.ClientRectangle.Right - _dayResizeColumnWidth)
				{
					ControlPaint.DrawReversibleLine(empty, guideBottom, SystemColors.ControlDark);
				}
			}
		}

		protected virtual void OnDataSourceChanged(EventArgs e)
		{
			if (base.Events[_dataSourceChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			BindDataSource(_dataSource, _bindingMemberInfo, false);
			base.OnBindingContextChanged(e);
		}

		public void RetrieveFields()
		{
			if (_schedulerCurrencyManager == null || IsUnboundMode())
			{
				return;
			}
			_fields.Clear();
			PropertyDescriptorCollection itemProperties = _schedulerCurrencyManager.GetItemProperties();
			foreach (PropertyDescriptor item in itemProperties)
			{
				SchedulerField field = new SchedulerField(item.Name);
				_fields.Add(field);
			}
		}

		private bool IsBuiltInField(SchedulerField schedulerField)
		{
			if (schedulerField.DataMemberMapping == DataMemberMapping.Custom)
			{
				if (string.Compare(schedulerField.DataMember, GetDescriptionMemberName(), ignoreCase: true) == 0)
				{
					return true;
				}
				if (string.Compare(schedulerField.DataMember, GetTextMemberName(), ignoreCase: true) == 0)
				{
					return true;
				}
				if (string.Compare(schedulerField.DataMember, GetStartTimeMemberName(), ignoreCase: true) == 0)
				{
					return true;
				}
				if (string.Compare(schedulerField.DataMember, GetEndTimeMemberName(), ignoreCase: true) == 0)
				{
					return true;
				}
				if (string.Compare(schedulerField.DataMember, GetOwnerMemberName(), ignoreCase: true) == 0)
				{
					return true;
				}
				if (string.Compare(schedulerField.DataMember, GetRecurrenceInfoFieldName(), ignoreCase: true) == 0)
				{
					return true;
				}
				if (string.Compare(schedulerField.DataMember, GetAppointmentSettingsMemberName(), ignoreCase: true) == 0)
				{
					return true;
				}
				return false;
			}
			return true;
		}
	}
}
