using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridCustomGroupRow : MordexCustomGroupRowBase, IMordexXmlLayoutsSupport
	{
		private int _groupImageIndex;

		private Image _groupImage;

		private string _groupImageKey;

		internal GridTable _ownerTable;

		[DefaultValue("")]
		public string GroupCaption
		{
			get
			{
				return base.BaseGroupCaption;
			}
			set
			{
				base.BaseGroupCaption = value;
			}
		}

		[DefaultValue(-1)]
		public int GroupImageIndex
		{
			get
			{
				return _groupImageIndex;
			}
			set
			{
				if (_groupImageIndex != value)
				{
					_groupImageIndex = value;
					if (_groupImageIndex != -1)
					{
						_groupImageKey = "";
					}
				}
			}
		}

		[DefaultValue("")]
		public string GroupImageKey
		{
			get
			{
				return _groupImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_groupImageKey != value)
				{
					_groupImageKey = value;
					if (!string.IsNullOrEmpty(_groupImageKey))
					{
						_groupImageIndex = -1;
					}
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(null)]
		public Image GroupImage
		{
			get
			{
				return _groupImage;
			}
			set
			{
				_groupImage = value;
			}
		}

		[DefaultValue(false)]
		public bool ShowRemainingRows
		{
			get
			{
				return base.BaseShowRemainingRows;
			}
			set
			{
				base.BaseShowRemainingRows = value;
			}
		}

		[DefaultValue(null)]
		[Description("The condition that rows need to meet to belong to the group row.")]
		[Editor("Mordex.Windows.DataGrid.Design.FilterConditionEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.FilterConditionConverter, Mordex.Windows.DataGrid.Design")]
		public GridFilterCondition Condition
		{
			get
			{
				return (GridFilterCondition)base.BaseCondition;
			}
			set
			{
				base.BaseCondition = value;
			}
		}

		[DefaultValue(false)]
		public bool ShowWhenEmpty
		{
			get
			{
				return base.BaseShowWhenEmpty;
			}
			set
			{
				base.BaseShowWhenEmpty = value;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => "Row" + Index;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public GridCustomGroupRow()
			: this("", null)
		{
		}

		public GridCustomGroupRow(string groupCaption)
			: this(groupCaption, null)
		{
		}

		public GridCustomGroupRow(string groupCaption, GridFilterCondition condition)
		{
			GroupCaption = groupCaption;
			Condition = condition;
			ResetGroupImage();
		}

		public GridCustomGroupRow(GridColumn column, object value)
		{
			if (value != null)
			{
				GroupCaption = value.ToString();
			}
			Condition = new GridFilterCondition(column, ConditionOperator.Equal, value);
			ResetGroupImage();
		}

		public GridCustomGroupRow(string groupCaption, GridColumn column, object value)
		{
			GroupCaption = groupCaption;
			Condition = new GridFilterCondition(column, ConditionOperator.Equal, value);
			ResetGroupImage();
		}

		private void ResetGroupImage()
		{
			_groupImageIndex = -1;
			_groupImageKey = "";
			_groupImage = null;
		}

		internal GridTable GetOwnerTable()
		{
			if (_ownerTable != null)
			{
				return _ownerTable;
			}
			if (base.CollectionOwner is GridCustomGroup gridCustomGroup)
			{
				return gridCustomGroup.GetOwnerTable();
			}
			return null;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader mordexReader)
		{
			ShowWhenEmpty = (bool)LayoutReader.GetPropertyValue(mordexReader, "ShowWhenEmpty", typeof(bool), false);
			ShowRemainingRows = (bool)LayoutReader.GetPropertyValue(mordexReader, "ShowRemainingRows", typeof(bool), false);
			GroupCaption = (string)LayoutReader.GetPropertyValue(mordexReader, "GroupCaption", typeof(string), "");
			_groupImage = (Image)LayoutReader.GetPropertyValue(mordexReader, "GroupImage", typeof(Image), null);
			_groupImageKey = (string)LayoutReader.GetPropertyValue(mordexReader, "GroupImageKey", typeof(string), "");
			_groupImageIndex = (int)LayoutReader.GetPropertyValue(mordexReader, "GroupImageIndex", typeof(int), -1);
			PropertyValue propertyValue = mordexReader["Condition"];
			if (propertyValue != null)
			{
				GridFilterCondition gridFilterCondition = new GridFilterCondition();
				gridFilterCondition._owner = GetOwnerTable();
				propertyValue.DeserializeObject(gridFilterCondition);
				gridFilterCondition._owner = null;
				Condition = gridFilterCondition;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter mordexWriter)
		{
			if (ShowWhenEmpty)
			{
				mordexWriter.AddValue("ShowWhenEmpty", ShowWhenEmpty);
			}
			if (!string.IsNullOrEmpty(GroupCaption))
			{
				mordexWriter.AddValue("GroupCaption", GroupCaption);
			}
			if (_groupImage != null)
			{
				mordexWriter.AddValue("GroupImage", _groupImage);
			}
			if (!string.IsNullOrEmpty(_groupImageKey))
			{
				mordexWriter.AddValue("GroupImageKey", _groupImageKey);
			}
			if (_groupImageIndex != -1)
			{
				mordexWriter.AddValue("GroupImageIndex", _groupImageIndex);
			}
			if (Condition != null)
			{
				mordexWriter.AddValue("Condition", Condition);
			}
			if (ShowRemainingRows)
			{
				mordexWriter.AddValue("ShowRemainingRows", ShowRemainingRows);
			}
		}
	}
}
