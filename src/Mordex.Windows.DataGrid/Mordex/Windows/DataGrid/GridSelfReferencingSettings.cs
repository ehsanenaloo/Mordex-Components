using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class GridSelfReferencingSettings : MordexSelfReferencingSettingsBase, IMordexXmlLayoutsSupport
	{
		private GridColumn _expandColumn;

		private bool _autoSizeColumnOnExpand;

		internal GridTable _owner;

		public GridTable Table => (GridTable)base.BaseTable;

		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		[Description("The column that contains the expand glyph. If this property is not set, the first visible column in the table is used.")]
		[Editor("Mordex.Windows.DataGrid.Design.ColumnEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(null)]
		public GridColumn ExpandColumn
		{
			get
			{
				return _expandColumn;
			}
			set
			{
				_expandColumn = value;
			}
		}

		[Description("Determines how self referencing rows are grouped.")]
		[DefaultValue(HierarchicalGroupMode.ParentRows)]
		public HierarchicalGroupMode HierarchicalGroupMode
		{
			get
			{
				return DataState.ComputeHierarchicalGroupMode(base.BaseHierarchicalGroupMode);
			}
			set
			{
				base.BaseHierarchicalGroupMode = DataState.ToDataHierarchicalGroupMode(value);
			}
		}

		[DefaultValue("")]
		[Editor("Mordex.Windows.DataGrid.Design.DataMemberEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("The property in the datasource that returns the list of child rows when HierarchicalMode is SelfReferencingChildList.")]
		public string ChildListMember
		{
			get
			{
				return base.BaseChildListMember;
			}
			set
			{
				base.BaseChildListMember = value;
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.DataMemberEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("The child column of the self referencing relation, as in a data relation: the property that holds the key of the parent row, for example ParentId.")]
		[DefaultValue("")]
		public string ChildDataMember
		{
			get
			{
				return base.BaseChildMember;
			}
			set
			{
				base.BaseChildMember = value;
			}
		}

		[DefaultValue("")]
		[Editor("Mordex.Windows.DataGrid.Design.DataMemberEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("The parent column of the self referencing relation, as in a data relation: the property that holds the key of the row itself, for example Id.")]
		public string ParentDataMember
		{
			get
			{
				return base.BaseParentMember;
			}
			set
			{
				base.BaseParentMember = value;
			}
		}

		[DefaultValue(null)]
		[Description("The value that determines which rows are treated as first level rows.")]
		[Editor("Mordex.Windows.DataGrid.Design.ObjectTypeEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ObjectTypeConverter, Mordex.Windows.DataGrid.Design")]
		public object ParentRootValue
		{
			get
			{
				return base.BaseParentRootValue;
			}
			set
			{
				base.BaseParentRootValue = value;
			}
		}

		[Description("Determines how first level rows are calculated.")]
		[DefaultValue(ParentRootMode.NullValue)]
		public ParentRootMode ParentRootMode
		{
			get
			{
				return DataState.ComputeParentRootMode(base.BaseParentRootMode);
			}
			set
			{
				base.BaseParentRootMode = DataState.ToDataParentRootMode(value);
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether rows without parent should be treated as root rows.")]
		public bool TreatOrphanRowsAsRoot
		{
			get
			{
				return base.BaseTreatOrphanRowsAsRoot;
			}
			set
			{
				base.BaseTreatOrphanRowsAsRoot = value;
			}
		}

		[DefaultValue(false)]
		public bool AutoSizeColumnOnExpand
		{
			get
			{
				return _autoSizeColumnOnExpand;
			}
			set
			{
				_autoSizeColumnOnExpand = value;
			}
		}

		[DefaultValue(SelfReferencingAggregationMode.AllRows)]
		public SelfReferencingAggregationMode AggregationMode
		{
			get
			{
				return DataState.ComputeSelfReferencingAggregationMode(base.BaseAggregationMode);
			}
			set
			{
				if (!Enum.IsDefined(typeof(SelfReferencingAggregationMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				base.BaseAggregationMode = DataState.ToDataSelfReferencingAggregationMode(value);
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => "SelfReferencingSettings";

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal GridSelfReferencingSettings(GridTable table)
			: base(table)
		{
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			ChildDataMember = (string)LayoutReader.GetPropertyValue(reader, "ChildDataMember", typeof(string), "");
			ChildListMember = (string)LayoutReader.GetPropertyValue(reader, "ChildListMember", typeof(string), "");
			int propertyValue = (int)LayoutReader.GetPropertyValue(reader, "ExpandColumn", typeof(int), -1);
			if (propertyValue >= 0 && propertyValue < _owner.Columns.Count)
			{
				ExpandColumn = _owner.Columns[propertyValue];
			}
			HierarchicalGroupMode = (HierarchicalGroupMode)LayoutReader.GetPropertyValue(reader, "HierarchicalGroupMode", typeof(HierarchicalGroupMode), HierarchicalGroupMode.ParentRows);
			AggregationMode = (SelfReferencingAggregationMode)LayoutReader.GetPropertyValue(reader, "AggregationMode", typeof(SelfReferencingAggregationMode), SelfReferencingAggregationMode.AllRows);
			ParentDataMember = (string)LayoutReader.GetPropertyValue(reader, "ParentDataMember", typeof(string), "");
			ParentRootMode = (ParentRootMode)LayoutReader.GetPropertyValue(reader, "ParentRootMode", typeof(ParentRootMode), ParentRootMode.NullValue);
			string text = (string)LayoutReader.GetPropertyValue(reader, "ParentRootValueType", typeof(string), "");
			if (text == "")
			{
				ParentRootValue = null;
			}
			else
			{
				string value = (string)LayoutReader.GetPropertyValue(reader, "ParentRootValue", typeof(string), null);
				ParentRootValue = LayoutTypeResolver.ConvertOrKeepText(value, text);
			}
			TreatOrphanRowsAsRoot = (bool)LayoutReader.GetPropertyValue(reader, "TreatOrphanRowsAsRoot", typeof(bool), false);
			_autoSizeColumnOnExpand = (bool)LayoutReader.GetPropertyValue(reader, "AutoSizeColumnOnExpand", typeof(bool), false);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_autoSizeColumnOnExpand)
			{
				info.AddValue("AutoSizeColumnOnExpand", _autoSizeColumnOnExpand);
			}
			if (ChildDataMember.Length > 0)
			{
				info.AddValue("ChildDataMember", ChildDataMember);
			}
			if (ChildListMember.Length > 0)
			{
				info.AddValue("ChildListMember", ChildListMember);
			}
			if (ExpandColumn != null)
			{
				info.AddValue("ExpandColumn", ExpandColumn.Index);
			}
			if (HierarchicalGroupMode != HierarchicalGroupMode.ParentRows)
			{
				info.AddValue("HierarchicalGroupMode", HierarchicalGroupMode);
			}
			if (ParentDataMember.Length > 0)
			{
				info.AddValue("ParentDataMember", ParentDataMember);
			}
			if (ParentRootMode != ParentRootMode.NullValue)
			{
				info.AddValue("ParentRootMode", ParentRootMode);
			}
			if (AggregationMode != SelfReferencingAggregationMode.AllRows)
			{
				info.AddValue("AggregationMode", AggregationMode);
			}
			if (ParentRootValue != null)
			{
				if (!(ParentRootValue is string))
				{
					info.AddValue("ParentRootValueType", ParentRootValue.GetType().FullName);
				}
				info.AddValue("ParentRootValue", ParentRootValue);
			}
			if (TreatOrphanRowsAsRoot)
			{
				info.AddValue("TreatOrphanRowsAsRoot", TreatOrphanRowsAsRoot);
			}
		}

		internal bool HasNonDefaultSettings()
		{
			if (ChildDataMember.Length > 0)
			{
				return true;
			}
			if (ChildListMember.Length > 0)
			{
				return true;
			}
			if (ExpandColumn != null)
			{
				return true;
			}
			if (HierarchicalGroupMode != HierarchicalGroupMode.ParentRows)
			{
				return true;
			}
			if (ParentDataMember.Length > 0)
			{
				return true;
			}
			if (ParentRootMode != ParentRootMode.NullValue)
			{
				return true;
			}
			if (ParentRootValue != null)
			{
				return true;
			}
			if (TreatOrphanRowsAsRoot)
			{
				return true;
			}
			return false;
		}
	}
}
