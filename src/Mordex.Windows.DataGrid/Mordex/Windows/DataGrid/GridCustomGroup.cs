using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid
{
	public class GridCustomGroup : MordexCustomGroupBase, IMordexXmlLayoutsSupport
	{
		private string _headerCaption;

		private bool _showInFieldChooser;

		internal GridTable _ownerTable;

		public CustomGroupType CustomGroupType
		{
			get
			{
				return DataState.ComputeCustomGroupType(base.BaseCustomGroupType);
			}
			set
			{
				base.BaseCustomGroupType = DataState.ToDataCustomGroupType(value);
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.CompositeColumnsEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(null)]
		[TypeConverter("Mordex.Windows.DataGrid.Design.CompositeColumnConverter, Mordex.Windows.DataGrid.Design")]
		public GridColumn[] CompositeColumns
		{
			get
			{
				MordexFieldBase[] baseCompositeFields = base.BaseCompositeFields;
				if (baseCompositeFields == null)
				{
					return null;
				}
				GridColumn[] array = new GridColumn[baseCompositeFields.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (GridColumn)baseCompositeFields[i];
				}
				return array;
			}
			set
			{
				base.BaseCompositeFields = value;
			}
		}

		public string HeaderCaption
		{
			get
			{
				if (_headerCaption == null)
				{
					if (CustomGroupType == CustomGroupType.CompositeColumns)
					{
						GridColumn[] compositeColumns = CompositeColumns;
						string text = "";
						if (compositeColumns != null)
						{
							for (int i = 0; i < compositeColumns.Length; i++)
							{
								if (i > 0)
								{
									text += " - ";
								}
								text += compositeColumns[i].Caption;
							}
						}
						return text;
					}
					return "";
				}
				return _headerCaption;
			}
			set
			{
				_headerCaption = value;
			}
		}

		public bool ShowInFieldChooser
		{
			get
			{
				return _showInFieldChooser;
			}
			set
			{
				if (_showInFieldChooser != value)
				{
					_showInFieldChooser = value;
					ComputeDataGrid()?.NotifyRefreshListeners();
				}
			}
		}

		public GridCustomGroupRowCollection GroupRows => (GridCustomGroupRowCollection)base.BaseGroupRows;

		public string Key
		{
			get
			{
				return base.BaseKey;
			}
			set
			{
				base.BaseKey = value;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => "CustomGroup" + Index;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public GridCustomGroup()
		{
			_headerCaption = null;
			_showInFieldChooser = true;
		}

		internal GridTable GetOwnerTable()
		{
			if (_ownerTable != null)
			{
				return _ownerTable;
			}
			return base.CollectionOwner as GridTable;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void ResetHeaderCaption()
		{
			HeaderCaption = null;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool ShouldSerializeHeaderCaption()
		{
			return _headerCaption != null;
		}

		internal MxDataGrid ComputeDataGrid()
		{
			return GetOwnerTable()?.DataGrid;
		}

		protected override MordexCustomGroupRowCollectionBase CreateGroupRows()
		{
			return new GridCustomGroupRowCollection(this);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader mordexReader)
		{
			_headerCaption = (string)LayoutReader.GetPropertyValue(mordexReader, "HeaderCaption", typeof(string), null);
			Key = (string)LayoutReader.GetPropertyValue(mordexReader, "Key", typeof(string), "");
			CustomGroupType = (CustomGroupType)LayoutReader.GetPropertyValue(mordexReader, "CustomGroupType", typeof(CustomGroupType), CustomGroupType.ConditionalGroupRows);
			int propertyValue = (int)LayoutReader.GetPropertyValue(mordexReader, "CompositeColumnsLength", typeof(int), -1);
			GridTable gridTable = GetOwnerTable();
			if (propertyValue >= 0 && gridTable != null)
			{
				GridColumn[] array = new GridColumn[propertyValue];
				for (int i = 0; i < propertyValue; i++)
				{
					int otherPropertyValue = (int)LayoutReader.GetPropertyValue(mordexReader, "CompositeColumn" + i, typeof(int), -1);
					if (otherPropertyValue >= 0 && otherPropertyValue < gridTable.Columns.Count)
					{
						array[i] = gridTable.Columns[otherPropertyValue];
					}
				}
				CompositeColumns = array;
			}
			mordexReader.DeserializeObject("GroupRows", GroupRows);
			_showInFieldChooser = (bool)LayoutReader.GetPropertyValue(mordexReader, "ShowInFieldChooser", typeof(bool), true);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter mordexWriter)
		{
			GridColumn[] compositeColumns = CompositeColumns;
			if (compositeColumns != null)
			{
				mordexWriter.AddValue("CompositeColumnsLength", compositeColumns.Length);
				for (int i = 0; i < compositeColumns.Length; i++)
				{
					mordexWriter.AddValue("CompositeColumn" + i, compositeColumns[i].Index);
				}
			}
			if (CustomGroupType != CustomGroupType.ConditionalGroupRows)
			{
				mordexWriter.AddValue("CustomGroupType", CustomGroupType);
			}
			if (!string.IsNullOrEmpty(Key))
			{
				mordexWriter.AddValue("Key", Key);
			}
			if (GroupRows.Count > 0)
			{
				mordexWriter.AddValue("GroupRows", GroupRows);
			}
			if (_headerCaption != null)
			{
				mordexWriter.AddValue("HeaderCaption", _headerCaption);
			}
			if (!ShowInFieldChooser)
			{
				mordexWriter.AddValue("ShowInFieldChooser", ShowInFieldChooser);
			}
		}

		internal void RemoveOrphanedFields()
		{
			if (base.BaseCompositeFields != null)
			{
				bool condition = false;
				MordexFieldBase[] baseCompositeFields = base.BaseCompositeFields;
				foreach (MordexFieldBase mordexFieldBase in baseCompositeFields)
				{
					if (mordexFieldBase is GridColumn gridColumn && gridColumn.GetOwnerColumnCollection() == null)
					{
						condition = true;
						break;
					}
				}
				if (condition)
				{
					List<MordexFieldBase> list = new List<MordexFieldBase>();
					MordexFieldBase[] otherBaseCompositeFields = base.BaseCompositeFields;
					foreach (MordexFieldBase item in otherBaseCompositeFields)
					{
						if (((IMordexField)item).Table != null)
						{
							list.Add(item);
						}
					}
					if (list.Count == 0)
					{
						base.BaseCompositeFields = null;
					}
					else
					{
						base.BaseCompositeFields = list.ToArray();
					}
				}
			}
			foreach (GridCustomGroupRow groupRow in GroupRows)
			{
				if (groupRow.Condition != null)
				{
					groupRow.Condition.ClearInvalidColumnReferences();
				}
			}
		}
	}
}
