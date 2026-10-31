using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class GridFormatCondition : MordexConditionBase, IMordexXmlLayoutsSupport, IFormatStyleOwner, IConditionOwner
	{
		private bool _areValuesConverted = true;

		private string _key = "";

		internal GridTable _ownerTable;

		private GridFormatConditionCollection _gridFormatConditionCollection;

		internal int _collectionIndex;

		private GridFilterCondition _filterCondition;

		internal bool _requiresTableRefresh;

		private GridFormatStyle _formatStyle;

		private GridFormatStyle _previewRowFormatStyle;

		private GridColumn _targetColumn;

		private bool _enabled = true;

		private bool _allowMerge = true;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("The key used to identify an object in a colletion.")]
		[DefaultValue("")]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		[Browsable(false)]
		public override int Index => _collectionIndex;

		[DefaultValue(null)]
		[TypeConverter("Mordex.Windows.DataGrid.Design.FilterConditionConverter, Mordex.Windows.DataGrid.Design")]
		[Editor("Mordex.Windows.DataGrid.Design.FilterConditionEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("The filter condition that rows need to meet in order to apply the FormatStyle associated to this instance in them.")]
		public GridFilterCondition FilterCondition
		{
			get
			{
				return _filterCondition;
			}
			set
			{
				if (_filterCondition != value)
				{
					if (_filterCondition != null)
					{
						_filterCondition.ProcessIConditionOwner(null);
					}
					_filterCondition = value;
					if (_filterCondition != null)
					{
						_filterCondition.ProcessIConditionOwner(this);
					}
				}
			}
		}

		[DefaultValue(null)]
		[Description("The GridColumn to compare the value(s).")]
		[Editor("Mordex.Windows.DataGrid.Design.ColumnEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		public GridColumn Column
		{
			get
			{
				return (GridColumn)base.BaseField;
			}
			set
			{
				base.BaseField = value;
				RefreshOwnerGrid();
			}
		}

		[Browsable(false)]
		public GridTable Table
		{
			get
			{
				if (_gridFormatConditionCollection == null)
				{
					return _ownerTable;
				}
				return _gridFormatConditionCollection._ownerTable;
			}
		}

		[DefaultValue(null)]
		[TypeConverter(typeof(StringConverter))]
		[Description("The object value to compare.")]
		public object Value1
		{
			get
			{
				return base.BaseValue1;
			}
			set
			{
				base.BaseValue1 = value;
				_areValuesConverted = true;
				RefreshOwnerGrid();
			}
		}

		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		[Description("The object value to compare when the condition operator requires two values.")]
		public object Value2
		{
			get
			{
				return base.BaseValue2;
			}
			set
			{
				base.BaseValue2 = value;
				_areValuesConverted = true;
				RefreshOwnerGrid();
			}
		}

		[Description("The format settings that will be applied to the rows that match the condition.")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public GridFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _formatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _formatStyle)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (ComputeDataGrid() == null || ComputeDataGrid() != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_formatStyle != null && _formatStyle._owner == this)
				{
					_formatStyle._owner = null;
				}
				_formatStyle = value;
				RefreshOwnerGrid();
			}
		}

		[Description("The format settings that will be applied to the preview row in rows that match the condition.")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public GridFormatStyle PreviewRowFormatStyle
		{
			get
			{
				if (_previewRowFormatStyle == null)
				{
					_previewRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _previewRowFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _previewRowFormatStyle)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (ComputeDataGrid() == null || ComputeDataGrid() != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_previewRowFormatStyle != null && _previewRowFormatStyle._owner == this)
				{
					_previewRowFormatStyle._owner = null;
				}
				_previewRowFormatStyle = value;
				RefreshOwnerGrid();
			}
		}

		[Description("The condition operator used for comparison.")]
		[DefaultValue(ConditionOperator.Equal)]
		public ConditionOperator ConditionOperator
		{
			get
			{
				return DataState.ComputeConditionOperator(base.BaseConditionOperator);
			}
			set
			{
				base.BaseConditionOperator = DataState.ToDataConditionOperator(value);
				RefreshOwnerGrid();
			}
		}

		[DefaultValue(null)]
		[Description("The GridColumn object where the FormatStyle will be applied.")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		[Editor("Mordex.Windows.DataGrid.Design.ColumnEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public GridColumn TargetColumn
		{
			get
			{
				return _targetColumn;
			}
			set
			{
				if (Table != null && value != null && value.Table != Table)
				{
					throw new InvalidOperationException();
				}
				if (value != _targetColumn)
				{
					_targetColumn = value;
					RefreshOwnerGrid();
				}
			}
		}

		[Description("Determines whether to apply the format condition in the records of the parent table.")]
		[DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					RefreshOwnerGrid();
				}
			}
		}

		[Description("Determines whether the format style associated to this format condition should be merged with the format style of other format condition instances.")]
		[DefaultValue(true)]
		public bool AllowMerge
		{
			get
			{
				return _allowMerge;
			}
			set
			{
				if (value != _allowMerge)
				{
					_allowMerge = value;
					NotifyCollectionChanged();
				}
			}
		}

		IMordexTable IConditionOwner.Table => Table;

		MxDataGrid IFormatStyleOwner.DataGrid => ComputeDataGrid();

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			GridTable table = Table;
			MxDataGrid dataGrid = ComputeDataGrid();
			if (table != null)
			{
				int otherPropertyValue = (int)LayoutReader.GetPropertyValue(info, "ColIndex", typeof(int), -1);
				if (otherPropertyValue >= 0 && otherPropertyValue < table.Columns.Count)
				{
					Column = table.Columns[otherPropertyValue];
				}
				otherPropertyValue = (int)LayoutReader.GetPropertyValue(info, "TargetColIndex", typeof(int), -1);
				if (otherPropertyValue >= 0 && otherPropertyValue < table.Columns.Count)
				{
					_targetColumn = table.Columns[otherPropertyValue];
				}
			}
			ConditionOperator = (ConditionOperator)LayoutReader.GetPropertyValue(info, "ConditionOperator", typeof(ConditionOperator), ConditionOperator.Equal);
			if (_formatStyle != null && _formatStyle._owner == this)
			{
				_formatStyle._owner = null;
				_formatStyle = null;
			}
			_formatStyle = (GridFormatStyle)LayoutReader.GetPropertyValue(info, "FormatStyle", typeof(GridFormatStyle), null);
			if (_formatStyle == null)
			{
				int nextPropertyValue = (int)LayoutReader.GetPropertyValue(info, "FormatStyleIndex", typeof(int), -1);
				if (dataGrid != null && nextPropertyValue >= 0 && nextPropertyValue < dataGrid.FormatStyles.Count)
				{
					_formatStyle = dataGrid.FormatStyles[nextPropertyValue];
				}
			}
			else
			{
				_formatStyle._owner = this;
			}
			if (_previewRowFormatStyle != null && _previewRowFormatStyle._owner == this)
			{
				_previewRowFormatStyle._owner = null;
				_previewRowFormatStyle = null;
			}
			_previewRowFormatStyle = (GridFormatStyle)LayoutReader.GetPropertyValue(info, "PreviewRowFormatStyle", typeof(GridFormatStyle), null);
			if (_previewRowFormatStyle == null)
			{
				int previousPropertyValue = (int)LayoutReader.GetPropertyValue(info, "PreviewRowFormatStyleIndex", typeof(int), -1);
				if (dataGrid != null && previousPropertyValue >= 0 && previousPropertyValue < dataGrid.FormatStyles.Count)
				{
					_previewRowFormatStyle = dataGrid.FormatStyles[previousPropertyValue];
				}
			}
			else
			{
				_previewRowFormatStyle._owner = this;
			}
			_enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			_allowMerge = (bool)LayoutReader.GetPropertyValue(info, "AllowMerge", typeof(bool), true);
			Value1 = LayoutReader.GetPropertyValue(info, "Value1", typeof(string), null);
			Value2 = LayoutReader.GetPropertyValue(info, "Value2", typeof(string), null);
			PropertyValue propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				GridFilterCondition gridFilterCondition = new GridFilterCondition();
				gridFilterCondition._owner = table;
				propertyValue.DeserializeObject(gridFilterCondition);
				gridFilterCondition._owner = null;
				FilterCondition = gridFilterCondition;
			}
			else
			{
				FilterCondition = null;
			}
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_areValuesConverted = false;
			ConvertValuesToColumnType();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Column != null)
			{
				info.AddValue("ColIndex", Column.Index);
			}
			if (ConditionOperator != ConditionOperator.Equal)
			{
				info.AddValue("ConditionOperator", ConditionOperator);
			}
			if (_formatStyle != null)
			{
				if (_formatStyle._owner == this)
				{
					if (!_formatStyle.IsDefaultStyle())
					{
						info.AddValue("FormatStyle", _formatStyle);
					}
				}
				else if (ComputeDataGrid() != null)
				{
					int value = ComputeDataGrid().FormatStyles.IndexOf(_formatStyle);
					info.AddValue("FormatStyleIndex", value);
				}
			}
			if (_previewRowFormatStyle != null)
			{
				if (_previewRowFormatStyle._owner == this)
				{
					if (!_previewRowFormatStyle.IsDefaultStyle())
					{
						info.AddValue("PreviewRowFormatStyle", _previewRowFormatStyle);
					}
				}
				else if (ComputeDataGrid() != null)
				{
					int otherIndexOf = ComputeDataGrid().FormatStyles.IndexOf(_previewRowFormatStyle);
					info.AddValue("PreviewRowFormatStyleIndex", otherIndexOf);
				}
			}
			if (_targetColumn != null)
			{
				info.AddValue("TargetColIndex", _targetColumn.Index);
			}
			if (!_enabled)
			{
				info.AddValue("Enabled", _enabled);
			}
			if (Value1 != null)
			{
				info.AddValue("Value1", Value1);
			}
			if (Value2 != null)
			{
				info.AddValue("Value2", Value2);
			}
			if (_key != string.Empty)
			{
				info.AddValue("Key", _key);
			}
			if (!_allowMerge)
			{
				info.AddValue("AllowMerge", _allowMerge);
			}
			if (_filterCondition != null)
			{
				info.AddValue("FilterCondition", _filterCondition);
			}
		}

		internal void ConvertValuesToColumnType()
		{
			if (!_areValuesConverted && Column != null)
			{
				if (Column.CompareTarget == ColumnCompareTarget.Value)
				{
					object item = Value1;
					object otherItem = Value2;
					if (Column.DataTypeCode == TypeCode.String)
					{
						if (item is string && ((string)item).Length == 0)
						{
							item = null;
						}
						if (otherItem is string && ((string)otherItem).Length == 0)
						{
							otherItem = null;
						}
					}
					try
					{
						if (item != null && item.GetType() != Column.Type)
						{
							Value1 = PropertyValue.ConvertValue(item.ToString(), Column.Type);
						}
						if (otherItem != null && otherItem.GetType() != Column.Type)
						{
							Value2 = PropertyValue.ConvertValue(otherItem.ToString(), Column.Type);
						}
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
				_areValuesConverted = true;
			}
			if (_filterCondition != null)
			{
				_filterCondition.ConvertValuesToColumnType();
			}
		}

		public GridFormatCondition()
			: this(null, ConditionOperator.Equal, null, null)
		{
		}

		public GridFormatCondition(GridColumn column, ConditionOperator conditionOperator, object value)
			: this(column, conditionOperator, value, null)
		{
		}

		public GridFormatCondition(GridColumn column, ConditionOperator conditionOperator, object value1, object value2)
		{
			Column = column;
			ConditionOperator = conditionOperator;
			Value1 = value1;
			Value2 = value2;
			_requiresTableRefresh = false;
			_collectionIndex = -1;
		}

		public GridFormatCondition Clone()
		{
			GridFormatCondition gridFormatCondition = new GridFormatCondition();
			gridFormatCondition._allowMerge = _allowMerge;
			gridFormatCondition._areValuesConverted = _areValuesConverted;
			gridFormatCondition.Column = Column;
			gridFormatCondition.ConditionOperator = ConditionOperator;
			gridFormatCondition._enabled = _enabled;
			if (_filterCondition != null)
			{
				gridFormatCondition._filterCondition = _filterCondition.Clone();
			}
			if (FormatStyle._owner == this)
			{
				gridFormatCondition._formatStyle = new GridFormatStyle(_formatStyle);
				gridFormatCondition._formatStyle._owner = gridFormatCondition;
			}
			gridFormatCondition._key = _key;
			gridFormatCondition._targetColumn = _targetColumn;
			gridFormatCondition.Value1 = Value1;
			gridFormatCondition.Value2 = Value2;
			return gridFormatCondition;
		}

		internal GridFormatConditionCollection GetOwnerCollection()
		{
			return _gridFormatConditionCollection;
		}

		internal void SetOwnerCollection(GridFormatConditionCollection gridFormatConditionCollection)
		{
			if (gridFormatConditionCollection != _gridFormatConditionCollection)
			{
				_gridFormatConditionCollection = gridFormatConditionCollection;
				if (_gridFormatConditionCollection == null)
				{
					_collectionIndex = -1;
				}
			}
		}

		internal MxDataGrid ComputeDataGrid()
		{
			if (_ownerTable != null)
			{
				return _ownerTable.DataGrid;
			}
			if (_gridFormatConditionCollection != null)
			{
				return _gridFormatConditionCollection.ComputeDataGrid();
			}
			return null;
		}

		internal bool IsValidForTable(GridTable gridTable)
		{
			if (gridTable == null)
			{
				return true;
			}
			if (_targetColumn != null && _targetColumn.Table != gridTable)
			{
				return false;
			}
			if (_filterCondition != null)
			{
				try
				{
					_filterCondition.ValidateBaseCondition(gridTable);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					return false;
				}
				return true;
			}
			if (Column == null || Column.Table != gridTable)
			{
				return false;
			}
			return true;
		}

		private void RefreshOwnerGrid()
		{
			MxDataGrid dataGrid = ComputeDataGrid();
			if (dataGrid != null)
			{
				if (_requiresTableRefresh)
				{
					dataGrid.ResetRowCacheOnTableChange(Table);
				}
				else
				{
					NotifyCollectionChanged();
				}
			}
		}

		internal void NotifyCollectionChanged()
		{
			if (_gridFormatConditionCollection != null)
			{
				_gridFormatConditionCollection.ApplyConditionsToGrid();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFormatStyle()
		{
			FormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFormatStyle()
		{
			if (_formatStyle == null)
			{
				return false;
			}
			if (_formatStyle._owner == this && _formatStyle.IsDefaultStyle())
			{
				return false;
			}
			return true;
		}

		public bool EvaluateRow(GridRow row)
		{
			if (row.RowType == RowType.Record)
			{
				return MatchesRow(row.GetLogicalRow());
			}
			return false;
		}

		internal bool MatchesRow(MordexRow row)
		{
			if (_filterCondition != null)
			{
				return _filterCondition.MatchBaseRow(row);
			}
			return BaseEvaluateRow(row);
		}

		internal void ClearUnusedFormatStyle()
		{
			if (!GridHelper.IsStyleAttached(_formatStyle))
			{
				_formatStyle = null;
			}
		}

		void IFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			if (ComputeDataGrid() != null)
			{
				ComputeDataGrid().RefreshRowCountAndInvalidate();
			}
		}
	}
}
