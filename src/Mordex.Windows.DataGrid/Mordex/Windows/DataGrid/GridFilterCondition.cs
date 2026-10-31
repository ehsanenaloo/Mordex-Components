using System;
using System.Collections;
using System.ComponentModel;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class GridFilterCondition : MordexConditionBase, IMordexXmlLayoutsSupport, IFilterCondition
	{
		internal GridTable _owner;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

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
			}
		}

		[DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return base.BaseEnabled;
			}
			set
			{
				base.BaseEnabled = value;
			}
		}

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

		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		public object Value1
		{
			get
			{
				return base.BaseValue1;
			}
			set
			{
				base.BaseValue1 = value;
			}
		}

		[DefaultValue(null)]
		public GridColumn ValueColumn1
		{
			get
			{
				return (GridColumn)base.BaseValueField1;
			}
			set
			{
				base.BaseValueField1 = value;
			}
		}

		[DefaultValue(null)]
		public GridColumn ValueColumn2
		{
			get
			{
				return (GridColumn)base.BaseValueField2;
			}
			set
			{
				base.BaseValueField2 = value;
			}
		}

		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		public object Value2
		{
			get
			{
				return base.BaseValue2;
			}
			set
			{
				base.BaseValue2 = value;
			}
		}

		[DefaultValue(false)]
		public bool Negation
		{
			get
			{
				return base.BaseNegation;
			}
			set
			{
				base.BaseNegation = value;
			}
		}

		[DefaultValue(LogicalOperator.None)]
		public LogicalOperator LogicalOperator
		{
			get
			{
				return DataState.ComputeLogicalOperator(base.BaseLogicalOperator);
			}
			set
			{
				base.BaseLogicalOperator = DataState.ToDataLogicalOperator(value);
			}
		}

		public GridFilterConditionCollection Conditions => (GridFilterConditionCollection)base.BaseConditions;

		GridFilterCondition IFilterCondition.FilterCondition => this;

		public bool IsComposite => base.BaseIsComposite;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			base.DisableValidation = true;
			Negation = (bool)LayoutReader.GetPropertyValue(info, "Negation", typeof(bool), false);
			ConditionOperator = (ConditionOperator)LayoutReader.GetPropertyValue(info, "ConditionOperator", typeof(ConditionOperator), ConditionOperator.Equal);
			LogicalOperator = (LogicalOperator)LayoutReader.GetPropertyValue(info, "LogicalOperator", typeof(LogicalOperator), LogicalOperator.None);
			Column = ReadColumnReference(info, "Column");
			int propertyValue = -1;
			GridTable gridTable = GetOwnerTable();
			if (gridTable != null)
			{
				propertyValue = (int)LayoutReader.GetPropertyValue(info, "ValueColumnIndex1", typeof(int), -1);
				if (propertyValue >= 0 && propertyValue < gridTable.Columns.Count)
				{
					ValueColumn1 = gridTable.Columns[propertyValue];
				}
				propertyValue = (int)LayoutReader.GetPropertyValue(info, "ValueColumnIndex2", typeof(int), -1);
				if (propertyValue >= 0 && propertyValue < gridTable.Columns.Count)
				{
					ValueColumn2 = gridTable.Columns[propertyValue];
				}
			}
			Value1 = DeserializeGridFilterConditionState(info, "Value1");
			Value2 = DeserializeGridFilterConditionState(info, "Value2");
			base.BaseEnabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			info.DeserializeObject("Conditions", Conditions);
			ConvertValuesToColumnType();
			base.DisableValidation = false;
		}

		private object DeserializeGridFilterConditionState(LayoutReader reader, string propertyName)
		{
			string text = "";
			int otherPropertyValue = (int)LayoutReader.GetPropertyValue(reader, propertyName + "_Count", typeof(int), -1);
			if (otherPropertyValue > -1)
			{
				object[] array = new object[otherPropertyValue];
				for (int i = 0; i < array.Length; i++)
				{
					object propertyValue = LayoutReader.GetPropertyValue(reader, propertyName + "_" + i, typeof(object), null);
					string nextPropertyValue = null;
					if (propertyValue == null || propertyValue is string)
					{
						nextPropertyValue = (string)LayoutReader.GetPropertyValue(reader, propertyName + "_" + i, typeof(string), null);
						if (nextPropertyValue == null)
						{
							continue;
						}
						text = (string)LayoutReader.GetPropertyValue(reader, "TypeName" + propertyName + "_" + i, typeof(string), null);
						if (text == null)
						{
							array[i] = nextPropertyValue;
							continue;
						}
						array[i] = LayoutTypeResolver.ConvertOrKeepText(nextPropertyValue, text);
					}
					else
					{
						array[i] = propertyValue;
					}
				}
				return array;
			}
			object previousPropertyValue = LayoutReader.GetPropertyValue(reader, propertyName, typeof(object), null);
			string firstPropertyValue = null;
			if (previousPropertyValue == null || previousPropertyValue is string)
			{
				firstPropertyValue = (string)LayoutReader.GetPropertyValue(reader, propertyName, typeof(string), null);
				if (firstPropertyValue != null)
				{
					text = (string)LayoutReader.GetPropertyValue(reader, "TypeName" + propertyName, typeof(string), null);
					if (text == null)
					{
						return firstPropertyValue;
					}
					return LayoutTypeResolver.ConvertOrKeepText(firstPropertyValue, text);
				}
			}
			return previousPropertyValue;
		}

		private GridColumn ReadColumnReference(LayoutReader reader, string text)
		{
			GridTable gridTable = GetOwnerTable();
			if (gridTable != null)
			{
				int propertyValue = (int)LayoutReader.GetPropertyValue(reader, text + "_TableIndex", typeof(int), -1);
				if (propertyValue != -1)
				{
					GridTableCollection gridTableCollection = MxDataGrid.CreateTableCollection(gridTable);
					if (propertyValue < 0 || propertyValue >= gridTableCollection.Count)
					{
						return null;
					}
					gridTable = gridTableCollection[propertyValue];
				}
				int otherPropertyValue = (int)LayoutReader.GetPropertyValue(reader, text + "Index", typeof(int), -1);
				if (otherPropertyValue >= 0 && otherPropertyValue < gridTable.Columns.Count)
				{
					return gridTable.Columns[otherPropertyValue];
				}
			}
			return null;
		}

		private void SerializeGridFilterConditionState(LayoutWriter mordexLayoutWriter, string text, GridColumn gridColumn)
		{
			if (gridColumn != null)
			{
				GridFilterCondition gridFilterCondition = GetRootCondition();
				if (gridFilterCondition.GetBaseOwner() != null && gridFilterCondition.GetBaseOwner().Table != null && gridFilterCondition.GetBaseOwner().Table != gridColumn.Table)
				{
					GridTable gridTable = GetOwnerTable();
					GridTableCollection gridTableCollection = MxDataGrid.CreateTableCollection(gridTable);
					int indexOfTable = gridTableCollection.IndexOfTable(gridColumn.Table);
					mordexLayoutWriter.AddValue(text + "_TableIndex", indexOfTable);
				}
				mordexLayoutWriter.AddValue(text + "Index", gridColumn.Index);
			}
		}

		private GridFilterCondition GetRootCondition()
		{
			if (GetParentCondition() == null)
			{
				return this;
			}
			return GetParentCondition().GetRootCondition();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Negation)
			{
				info.AddValue("Negation", Negation);
			}
			SerializeGridFilterConditionState(info, "Column", Column);
			if (ConditionOperator != ConditionOperator.Equal)
			{
				info.AddValue("ConditionOperator", ConditionOperator);
			}
			if (LogicalOperator != LogicalOperator.None)
			{
				info.AddValue("LogicalOperator", LogicalOperator);
			}
			if (Value1 != null)
			{
				WriteValueOrList(info, "Value1", Value1);
			}
			if (Value2 != null)
			{
				WriteValueOrList(info, "Value2", Value2);
			}
			if (ValueColumn1 != null)
			{
				info.AddValue("ValueColumnIndex1", ValueColumn1.Index);
			}
			if (ValueColumn2 != null)
			{
				info.AddValue("ValueColumnIndex2", ValueColumn2.Index);
			}
			if (Conditions.Count > 0)
			{
				info.AddValue("Conditions", Conditions);
			}
			if (!Enabled)
			{
				info.AddValue("Enabled", Enabled);
			}
		}

		private void WriteValueOrList(LayoutWriter mordexLayoutWriter, string propertyName, object value)
		{
			if (value is IList)
			{
				IList list = (IList)value;
				mordexLayoutWriter.AddValue(propertyName + "_Count", list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					object listItem = list[i];
					if (listItem != null)
					{
						if (!(listItem is string))
						{
							mordexLayoutWriter.AddValue("TypeName" + propertyName + "_" + i, listItem.GetType().FullName);
						}
						mordexLayoutWriter.AddValue(propertyName + "_" + i, listItem);
					}
				}
			}
			else
			{
				if (!(value is string))
				{
					mordexLayoutWriter.AddValue("TypeName" + propertyName, value.GetType().FullName);
				}
				mordexLayoutWriter.AddValue(propertyName, value);
			}
		}

		public GridFilterCondition()
			: this(null, ConditionOperator.Equal, null, null)
		{
		}

		public GridFilterCondition(GridColumn column, ConditionOperator conditionOperator, object value1)
			: this(column, conditionOperator, value1, null)
		{
		}

		public GridFilterCondition(GridColumn column, ConditionOperator conditionOperator, object value1, object value2)
			: base(column, DataState.ToDataConditionOperator(conditionOperator), value1, value2)
		{
		}

		public GridFilterCondition Clone()
		{
			return (GridFilterCondition)BaseClone();
		}

		internal GridFilterCondition GetParentCondition()
		{
			if (GetParentCollection() != null)
			{
				return GetParentCollection().GetOwnerCondition();
			}
			return null;
		}

		internal GridTable GetOwnerTable()
		{
			if (_owner != null)
			{
				return _owner;
			}
			return base.BaseTable as GridTable;
		}

		internal IConditionOwner GetBaseOwner()
		{
			return base.BaseOwner;
		}

		internal void ProcessIConditionOwner(IConditionOwner iConditionOwner)
		{
			base.BaseOwner = iConditionOwner;
		}

		internal bool MatchBaseRow(MordexRow row)
		{
			return BaseEvaluateRow(row);
		}

		public bool EvaluateRow(GridRow row)
		{
			if (row.RowType == RowType.Record)
			{
				return MatchBaseRow(row.GetLogicalRow());
			}
			return false;
		}

		internal void ValidateBaseCondition(GridTable table)
		{
			BaseValidate(table);
		}

		internal void ConvertValuesToColumnType()
		{
			if (Column != null && Column.CompareTarget != ColumnCompareTarget.Text && Column.Type != typeof(object))
			{
				object item = Value1;
				object otherItem = Value2;
				if (!(item is Array))
				{
					if (Column.Type != typeof(string))
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
					if (item != null && item.GetType() != Column.Type)
					{
						try
						{
							Value1 = PropertyValue.ConvertValue(item.ToString(), Column.Type);
						}
						catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
						{
							GridExceptions.Report(caught);
						}
					}
					if (otherItem != null && otherItem.GetType() != Column.Type)
					{
						try
						{
							Value2 = PropertyValue.ConvertValue(otherItem.ToString(), Column.Type);
						}
						catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
						{
							GridExceptions.Report(caught);
						}
					}
				}
			}
			if (Conditions == null)
			{
				return;
			}
			foreach (GridFilterCondition condition in Conditions)
			{
				condition.ConvertValuesToColumnType();
			}
		}

		private string BuildConditionText()
		{
			if (IsComposite)
			{
				string text = "(";
				if (!IsEmptyCondition())
				{
					text += BuildSimpleConditionText();
				}
				foreach (GridFilterCondition condition in Conditions)
				{
					if (Conditions.IndexOf(condition) != 0 || !IsEmptyCondition())
					{
						if (condition.LogicalOperator == LogicalOperator.And)
						{
							text += " And ";
						}
						else if (condition.LogicalOperator == LogicalOperator.Xor)
						{
							text += " Xor ";
						}
						else if (condition.LogicalOperator == LogicalOperator.Or)
						{
							text += " Or ";
						}
					}
					text += condition.BuildConditionText();
				}
				return text + ")";
			}
			return BuildSimpleConditionText();
		}

		private string BuildSimpleConditionText()
		{
			if (IsEmptyCondition())
			{
				if (Negation)
				{
					return "(False)";
				}
				return "(Empty)";
			}
			string text = ((Column == null) ? "(null)" : ((!(Column.Key == "")) ? Column.Key : Column.Index.ToString(GridHelper.GetCurrentCulture())));
			string simpleConditionText = "([" + text + "] " + GetOperatorText(ConditionOperator) + " ";
			simpleConditionText = ((ValueColumn1 != null) ? string.Concat(str2: (!(ValueColumn1.Key == "")) ? ValueColumn1.Key : ValueColumn1.Index.ToString(GridHelper.GetCurrentCulture()), str0: simpleConditionText, str1: "[", str3: "]") : (simpleConditionText + Value1));
			if (ConditionOperator == ConditionOperator.Between || ConditionOperator == ConditionOperator.NotBetween)
			{
				simpleConditionText = ((ValueColumn2 == null) ? (simpleConditionText + " And " + Value2) : string.Concat(str2: (!(ValueColumn2.Key == "")) ? ValueColumn2.Key : ValueColumn2.Index.ToString(GridHelper.GetCurrentCulture()), str0: simpleConditionText, str1: " And [", str3: "]"));
			}
			else if (ConditionOperator == ConditionOperator.In || ConditionOperator == ConditionOperator.NotIn)
			{
				simpleConditionText += " }";
			}
			simpleConditionText += ")";
			if (Negation)
			{
				simpleConditionText = "(Not " + simpleConditionText + ")";
			}
			return simpleConditionText;
		}

		private string GetOperatorText(ConditionOperator conditionOperator)
		{
			switch (ConditionOperator)
			{
			case ConditionOperator.BeginsWith:
				return "BeginsWith";
			case ConditionOperator.Between:
				return "Between";
			case ConditionOperator.Contains:
				return "Contains";
			case ConditionOperator.EndsWith:
				return "EndsWith";
			case ConditionOperator.Equal:
				return "=";
			case ConditionOperator.GreaterThan:
				return ">";
			case ConditionOperator.GreaterThanOrEqualTo:
				return ">=";
			case ConditionOperator.LessThan:
				return "<";
			case ConditionOperator.LessThanOrEqualTo:
				return "<=";
			case ConditionOperator.NotBetween:
				return "Not between";
			case ConditionOperator.NotContains:
				return "Doesn't contain";
			case ConditionOperator.NotEqual:
				return "<>";
			case ConditionOperator.IsNull:
				return "Is Null";
			case ConditionOperator.NotIsNull:
				return "Isn't Null";
			case ConditionOperator.IsEmpty:
				return "Is empty";
			case ConditionOperator.NotIsEmpty:
				return "Isn't empty";
			case ConditionOperator.In:
				return "Any of: {";
			case ConditionOperator.NotIn:
				return "None of: {";
			default:
				return "";
			}
		}

		public override string ToString()
		{
			return BuildConditionText();
		}

		public GridFilterCondition AddCondition()
		{
			GridFilterCondition gridFilterCondition = new GridFilterCondition();
			AddCondition(gridFilterCondition);
			return gridFilterCondition;
		}

		public void AddCondition(GridFilterCondition condition)
		{
			AddCondition(LogicalOperator.None, condition);
		}

		public void AddCondition(LogicalOperator logicalOperator, GridFilterCondition condition)
		{
			BaseAddCondition(DataState.ToDataLogicalOperator(logicalOperator), condition);
		}

		public void RemoveCondition(GridFilterCondition condition)
		{
			BaseRemoveCondition(condition);
		}

		public void RemoveConditionAt(int index)
		{
			BaseRemoveConditionAt(index);
		}

		public void Clear()
		{
			BaseClear();
		}

		internal bool IsEmptyCondition()
		{
			if (Column != null)
			{
				return false;
			}
			if (Value1 != null)
			{
				return false;
			}
			if (ValueColumn1 != null)
			{
				return false;
			}
			if (ConditionOperator != ConditionOperator.Equal)
			{
				return false;
			}
			if (ConditionOperator == ConditionOperator.Between || ConditionOperator == ConditionOperator.NotBetween)
			{
				if (Value2 != null)
				{
					return false;
				}
				if (ValueColumn2 != null)
				{
					return false;
				}
			}
			return true;
		}

		protected override MordexConditionCollectionBase CreateConditions()
		{
			return new GridFilterConditionCollection(this);
		}

		protected override MordexConditionBase CreateCondition()
		{
			return new GridFilterCondition();
		}

		internal GridFilterConditionCollection GetParentCollection()
		{
			return (GridFilterConditionCollection)base.BaseParentCollection;
		}

		internal void ClearInvalidColumnReferences()
		{
			base.DisableValidation = true;
			if (Column != null && Column.GetOwnerColumnCollection() == null)
			{
				Column = null;
			}
			if (ValueColumn1 != null && ValueColumn1.GetOwnerColumnCollection() == null)
			{
				ValueColumn1 = null;
			}
			if (ValueColumn2 != null && ValueColumn2.GetOwnerColumnCollection() == null)
			{
				ValueColumn2 = null;
			}
			foreach (GridFilterCondition condition in Conditions)
			{
				condition.ClearInvalidColumnReferences();
			}
			base.DisableValidation = false;
		}
	}
}
