using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class GridGroupHeaderTotal : MordexGroupTotalBase, IMordexXmlLayoutsSupport, IConditionOwner, IFormatStyleOwner
	{
		internal GridTable _ownerTable;

		private GridFormatStyle _formatStyle;

		private GridColumn _columnAlign;

		private string _totalSuffix = ")";

		private string _totalPrefix = "(";

		private FormatMode _totalFormatMode = FormatMode.UseIFormattable;

		private string _formatString = "";

		private GridFilterCondition _filterCondition;

		string IMordexXmlLayoutsSupport.Identifier => Key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		IMordexTable IConditionOwner.Table => Table;

		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Format settings used to draw the group header total.")]
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
				if (_formatStyle == value)
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
				RefreshGrid();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ColumnEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Represents the GridColumn to which the total will be aligned.")]
		[DefaultValue(null)]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		public GridColumn ColumnAlign
		{
			get
			{
				return _columnAlign;
			}
			set
			{
				if (_columnAlign != value)
				{
					_columnAlign = value;
					RefreshGrid();
				}
			}
		}

		[Browsable(false)]
		public GridTable Table => (GridTable)base.BaseTable;

		[DefaultValue(null)]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		[Description("Represents the GridColumn object to summarize.")]
		[Editor("Mordex.Windows.DataGrid.Design.ColumnEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public GridColumn Column
		{
			get
			{
				return (GridColumn)base.BaseField;
			}
			set
			{
				base.BaseField = value;
			}
		}

		[Description("Represents the aggregate function whose result will be shown in the row.")]
		[DefaultValue(AggregateFunction.None)]
		public AggregateFunction AggregateFunction
		{
			get
			{
				return DataState.ComputeAggregateFunction(base.BaseAggregateFunction);
			}
			set
			{
				base.BaseAggregateFunction = DataState.ToDataAggregateFunction(value);
			}
		}

		[Description("The text displayed after the total value.")]
		[DefaultValue(")")]
		public string TotalSuffix
		{
			get
			{
				return _totalSuffix;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (_totalSuffix != value)
				{
					_totalSuffix = value;
					RefreshGrid();
				}
			}
		}

		[DefaultValue("(")]
		[Description("The text displayed before the total value.")]
		public string TotalPrefix
		{
			get
			{
				return _totalPrefix;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentException("TotalPrefix can not be null.");
				}
				if (_totalPrefix != value)
				{
					_totalPrefix = value;
					RefreshGrid();
				}
			}
		}

		[DefaultValue(FormatMode.UseIFormattable)]
		[Description("Determines how values will be formatted.")]
		public FormatMode TotalFormatMode
		{
			get
			{
				return _totalFormatMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FormatMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == FormatMode.UseDefault)
				{
					value = FormatMode.UseIFormattable;
				}
				if (value != _totalFormatMode)
				{
					_totalFormatMode = value;
					RefreshGrid();
				}
			}
		}

		[DefaultValue("")]
		[Description("Represents the format to use when rendering total values.")]
		public string TotalFormatString
		{
			get
			{
				return _formatString;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentException("TotalFormatString can not be null.");
				}
				if (value != _formatString)
				{
					_formatString = value;
					RefreshGrid();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.FilterConditionEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Represents the filter condition that rows need to meet in order to include them in the summary operation.")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.FilterConditionConverter, Mordex.Windows.DataGrid.Design")]
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
					RefreshGrid();
				}
			}
		}

		[DefaultValue("")]
		[Description("The key used to identify an object in a colletion.")]
		public string Key
		{
			get
			{
				return base.BaseKey;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentException("Key can not be null.");
				}
				base.BaseKey = value;
			}
		}

		MxDataGrid IFormatStyleOwner.DataGrid => ComputeDataGrid();

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			GridTable gridTable = GetEffectiveTable();
			if (gridTable != null)
			{
				int otherPropertyValue = (int)LayoutReader.GetPropertyValue(info, "ColIndex", typeof(int), -1);
				if (otherPropertyValue >= 0 && otherPropertyValue < gridTable.Columns.Count)
				{
					Column = gridTable.Columns[otherPropertyValue];
				}
				otherPropertyValue = (int)LayoutReader.GetPropertyValue(info, "ColumnAlign", typeof(int), -1);
				if (otherPropertyValue >= 0 && otherPropertyValue < gridTable.Columns.Count)
				{
					ColumnAlign = gridTable.Columns[otherPropertyValue];
				}
			}
			AggregateFunction = (AggregateFunction)LayoutReader.GetPropertyValue(info, "AggregateFunction", typeof(AggregateFunction), AggregateFunction.None);
			PropertyValue propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				_filterCondition = new GridFilterCondition();
				_filterCondition._owner = gridTable;
				propertyValue.DeserializeObject(_filterCondition);
				_filterCondition._owner = null;
			}
			_totalFormatMode = (FormatMode)LayoutReader.GetPropertyValue(info, "TotalFormatMode", typeof(FormatMode), FormatMode.UseIFormattable);
			_formatString = (string)LayoutReader.GetPropertyValue(info, "TotalFormatString", typeof(string), "");
			_totalPrefix = (string)LayoutReader.GetPropertyValue(info, "TotalPrefix", typeof(string), "(");
			_totalSuffix = (string)LayoutReader.GetPropertyValue(info, "TotalSuffix", typeof(string), ")");
			Key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			info["FormatStyle"]?.DeserializeObject(FormatStyle);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Column != null && Column.Index != -1)
			{
				info.AddValue("ColIndex", Column.Index);
			}
			if (AggregateFunction != AggregateFunction.None)
			{
				info.AddValue("AggregateFunction", AggregateFunction);
			}
			if (_filterCondition != null)
			{
				info.AddValue("FilterCondition", _filterCondition);
			}
			if (_totalFormatMode != FormatMode.UseIFormattable)
			{
				info.AddValue("TotalFormatMode", _totalFormatMode);
			}
			if (_formatString != "")
			{
				info.AddValue("TotalFormatString", _formatString);
			}
			if (_totalPrefix != "(")
			{
				info.AddValue("TotalPrefix", _totalPrefix);
			}
			if (_totalSuffix != ")")
			{
				info.AddValue("TotalSuffix", _totalSuffix);
			}
			if (Key.Length > 0)
			{
				info.AddValue("Key", Key);
			}
			if (_columnAlign != null)
			{
				info.AddValue("ColumnAlign", _columnAlign.Index);
			}
			if (_formatStyle != null && !_formatStyle.IsDefaultStyle())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
		}

		internal GridTable GetEffectiveTable()
		{
			if (_ownerTable != null)
			{
				return _ownerTable;
			}
			return Table;
		}

		public GridGroupHeaderTotal()
		{
		}

		public GridGroupHeaderTotal(GridColumn column, AggregateFunction aggregateFunction)
		{
			Column = column;
			AggregateFunction = aggregateFunction;
		}

		internal MxDataGrid ComputeDataGrid()
		{
			if (Table != null)
			{
				return Table.DataGrid;
			}
			return null;
		}

		internal string GetTotalText(GridRow gridRow)
		{
			GridColumn gridColumn = Column;
			if (gridColumn != null || AggregateFunction == AggregateFunction.Count || AggregateFunction == AggregateFunction.None)
			{
				if (gridColumn == null && gridRow.Table.Columns.Count > 0)
				{
					gridColumn = gridRow.Table.Columns[0];
				}
				object subTotal = gridRow.GetSubTotal(gridColumn, AggregateFunction, FilterCondition);
				return FormatTotal(subTotal);
			}
			return "";
		}

		internal string FormatTotal(object value)
		{
			string text = "";
			text = (GridHelper.IsNullOrDBNull(value) ? "" : ((TotalFormatMode == FormatMode.UseIFormattable) ? ((!(value is IFormattable) || TotalFormatString.Length <= 0) ? GridHelper.ToSafeString(value) : ((IFormattable)value).ToString(TotalFormatString, GridHelper.GetCurrentCulture())) : ((TotalFormatString.Length <= 0) ? GridHelper.ToSafeString(value) : string.Format(TotalFormatString, value))));
			if (text.Length > 0)
			{
				text = TotalPrefix + text;
				text += TotalSuffix;
			}
			return text;
		}

		private void RefreshGrid()
		{
			if (Table != null && Table.DataGrid != null)
			{
				Table.DataGrid.RebuildRows(true);
			}
		}

		internal GridGroupHeaderTotalCollection GetParentCollection()
		{
			return (GridGroupHeaderTotalCollection)base.BaseParentCollection;
		}

		internal void ValidateForTable(GridTable gridTable)
		{
			if (Column == null && AggregateFunction != AggregateFunction.Count && AggregateFunction != AggregateFunction.None)
			{
				throw new InvalidOperationException("You must specify the Column.");
			}
			if (_filterCondition != null)
			{
				_filterCondition.ValidateBaseCondition(gridTable);
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
