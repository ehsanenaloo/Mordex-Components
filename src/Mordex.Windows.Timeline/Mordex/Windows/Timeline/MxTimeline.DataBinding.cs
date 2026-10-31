using System;
using System.Collections;
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
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimeline
	{
		private void OnDataSourceDisposed(object sender, EventArgs eventArgs)
		{
			DataSource = null;
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			ApplyDataBinding(dataSource, new BindingMemberInfo(dataMember));
			InvalidateLayout();
		}

		private void ApplyDataBinding(object dataSource, BindingMemberInfo bindingMemberInfo)
		{
			if (dataSource is ISupportInitializeNotification supportInitializeNotification && !supportInitializeNotification.IsInitialized)
			{
				if (_pendingDataSource != null && _pendingDataSource is ISupportInitializeNotification pendingInitializeNotification)
				{
					pendingInitializeNotification.Initialized -= OnControlInitialized;
				}
				_pendingDataSource = dataSource;
				_pendingDataMember = bindingMemberInfo.BindingMember;
				supportInitializeNotification.Initialized += OnControlInitialized;
				return;
			}
			SetUnbound(dataSource == null);
			CurrencyManager currencyManager = null;
			if (!IsUnbound())
			{
				if (dataSource != null)
				{
					currencyManager = (CurrencyManager)GetBindingContext()[dataSource, bindingMemberInfo.BindingMember];
				}
				_bindingMemberInfo = bindingMemberInfo;
				_dataSource = dataSource;
			}
			else
			{
				if (_dataTable == null)
				{
					UpdateDataTable();
				}
				currencyManager = (CurrencyManager)GetBindingContext()[_dataTable];
			}
			UpdateCurrencyManager(currencyManager);
		}

		private BindingContext GetBindingContext()
		{
			if (_bindingContext == null)
			{
				_bindingContext = new BindingContext();
			}
			return _bindingContext;
		}

		private void UpdateDataTable()
		{
			if (_dataTable != null)
			{
				DisposeUnboundTable();
			}
			_dataTable = new DataTable("UnboundTable");
			_dataTable.Columns.Add("___StartTimeMember", typeof(DateTime));
			_dataTable.Columns.Add("___EndTimeMember", typeof(DateTime));
			_dataTable.Columns.Add("___DescriptionMember", typeof(string));
			_dataTable.Columns.Add("___EstimatedStartTimeMember", typeof(DateTime));
			_dataTable.Columns.Add("___EstimatedEndTimeMember", typeof(DateTime));
			foreach (TimelineField field in Fields)
			{
				if (field.DataMemberMapping == DataMemberMapping.Custom)
				{
					try
					{
						_dataTable.Columns.Add(new DataColumn(field.DataMember, field.Type));
					}
					catch
					{
						_dataTable.Columns.Add(new DataColumn(field.DataMember, typeof(object)));
					}
				}
			}
			ApplyDataBinding(null, new BindingMemberInfo(""));
		}

		private void DisposeUnboundTable()
		{
			if (_dataTable != null)
			{
				_dataTable.Dispose();
				_dataTable = null;
			}
		}

		internal string GetStartTimeField()
		{
			if (_isUnbound)
			{
				return "___StartTimeMember";
			}
			return _startTimeField;
		}

		internal string GetEstimatedStartTimeField()
		{
			if (_isUnbound)
			{
				return "___EstimatedStartTimeMember";
			}
			return _estimatedStartTimeField;
		}

		internal string GetEndTimeField()
		{
			if (_isUnbound)
			{
				return "___EndTimeMember";
			}
			return _endTimeField;
		}

		internal string GetEstimatedEndTimeField()
		{
			if (_isUnbound)
			{
				return "___EstimatedEndTimeMember";
			}
			return _estimatedEndTimeField;
		}

		internal string GetTextField()
		{
			if (_isUnbound)
			{
				return "___DescriptionMember";
			}
			return _textField;
		}

		public void RetrieveFields()
		{
			if (GetCurrencyManager() != null && !IsUnbound())
			{
				_timelineDataSourceManager.CreateTableStructure(hierarchical: false);
				OnRootTableChanged(EventArgs.Empty);
			}
		}

		internal TimelineField GetImageField()
		{
			return _timelineField;
		}

		internal void UpdateTimelineField(TimelineField timelineField)
		{
			if (_timelineField != timelineField)
			{
				if (_timelineField != null)
				{
					_timelineField.ItemsImage = false;
				}
				_timelineField = timelineField;
				Invalidate();
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			_bindingContext = BindingContext;
			Refetch();
		}

		internal void SyncDataTableColumn(TimelineField timelineField, EditType editType)
		{
			if (!IsUnbound() || _dataTable == null)
			{
				return;
			}
			switch (editType)
			{
			case EditType.Add:
				if (timelineField.DataMemberMapping == DataMemberMapping.Custom)
				{
					_dataTable.Columns.Add(timelineField.DataMember, timelineField.Type);
				}
				break;
			case EditType.Remove:
				if (timelineField.DataMemberMapping == DataMemberMapping.Custom)
				{
					_dataTable.Columns.Remove(timelineField.DataMember);
				}
				break;
			case EditType.Clear:
				UpdateDataTable();
				break;
			case EditType.Change:
				break;
			}
		}

		internal TimelineDataSourceManager GetTimelineDataSourceManager()
		{
			return _timelineDataSourceManager;
		}

		internal TimelineMordexTableBase GetRootTable()
		{
			return (TimelineMordexTableBase)_timelineDataSourceManager.RootTable;
		}
	}
}
