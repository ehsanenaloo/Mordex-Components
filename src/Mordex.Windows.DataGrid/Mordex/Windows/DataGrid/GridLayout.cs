using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	[TypeConverter("Mordex.Windows.DataGrid.Design.GridLayoutConverter, Mordex.Windows.DataGrid.Design")]
	[DesignerSerializer("Mordex.Windows.DataGrid.Design.GridLayoutSerializer, Mordex.Windows.DataGrid.Design", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class GridLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxDataGrid _dataGrid;

		private string _key = "";

		private object _tag;

		private bool _isModified;

		private PersistenceMode _persistenceMode = PersistenceMode.All;

		private string _layoutData = "";

		internal bool _isCurrentLayout;

		private GridLayoutCollection _gridLayoutCollection;

		internal object _dataSource;

		internal string _dataMember = "";

		private GridLayoutCollection _childLayouts;

		private string _description = "";

		private LayoutReferenceCollection _layoutReferences;

		internal Control _owner;

		[Browsable(false)]
		public GridLayout ParentLayout
		{
			get
			{
				if (_gridLayoutCollection != null)
				{
					return _gridLayoutCollection.GetParentLayout();
				}
				return null;
			}
		}

		[DefaultValue("")]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (_key != value)
				{
					if (_gridLayoutCollection != null && string.Compare(_key, value, ignoreCase: true) != 0 && _gridLayoutCollection.Contains(value))
					{
						throw new InvalidOperationException("There is already another object in the collection with this key.");
					}
					_key = value;
				}
			}
		}

		[TypeConverter("System.ComponentModel.StringConverter")]
		[DefaultValue(null)]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		[DefaultValue("")]
		[Browsable(false)]
		[Localizable(true)]
		public string LayoutString
		{
			get
			{
				return _layoutData;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_layoutData != value)
				{
					_layoutData = value;
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(false)]
		public bool IsCurrentLayout
		{
			get
			{
				if (ComputeDataGrid() != null)
				{
					if (ComputeDataGrid().CurrentLayout == this)
					{
						return true;
					}
					return false;
				}
				return _isCurrentLayout;
			}
			set
			{
				if (_gridLayoutCollection != null)
				{
					if (value)
					{
						_gridLayoutCollection.ComputeDataGrid().CurrentLayout = this;
					}
					else
					{
						_gridLayoutCollection.ComputeDataGrid().CurrentLayout = null;
					}
				}
				else
				{
					_isCurrentLayout = value;
				}
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		[DefaultValue(null)]
		public object DataSource
		{
			get
			{
				return _dataSource;
			}
			set
			{
				if (_dataSource != value)
				{
					if (_dataSource is IComponent component)
					{
						component.Disposed -= OnControlDisposed;
					}
					_dataSource = value;
					if (_dataSource is IComponent otherComponent)
					{
						otherComponent.Disposed += OnControlDisposed;
					}
					if (_dataGrid != null)
					{
						_dataGrid.SetDataBinding(_dataSource, _dataMember);
					}
				}
			}
		}

		[DefaultValue("")]
		[Browsable(false)]
		public string DataMember
		{
			get
			{
				return _dataMember;
			}
			set
			{
				if (_dataMember != value)
				{
					_dataMember = value;
					if (_dataGrid != null)
					{
						_dataGrid.SetDataBinding(_dataSource, _dataMember);
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridLayoutCollection ChildLayouts => _childLayouts;

		[Browsable(false)]
		public bool StandAlone
		{
			get
			{
				if (_layoutData != "" && LayoutMerger.IsIncrementalLayout(_layoutData))
				{
					return false;
				}
				return true;
			}
		}

		[DefaultValue("")]
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public LayoutReferenceCollection LayoutReferences => _layoutReferences;

		string IMordexLayout.DesignTimeName
		{
			get
			{
				if (GetOwnerCollection() == null)
				{
					return "DesignTimeLayout";
				}
				if (ParentLayout == null)
				{
					return "Layout_" + GetOwnerCollection().IndexOf(this);
				}
				return GetOwnerCollection().IndexOf(this).ToString();
			}
		}

		object IMordexLayout.Owner
		{
			get
			{
				if (ParentLayout == null)
				{
					if (GetOwnerCollection() == null)
					{
						return _owner;
					}
					return GetOwnerCollection().GetDataGrid();
				}
				return ParentLayout;
			}
		}

		public static GridLayout FromStream(Stream stream)
		{
			GridLayout gridLayout = new GridLayout();
			gridLayout.ProcessStream(stream);
			return gridLayout;
		}

		public static GridLayout FromXMLString(string xmlString)
		{
			GridLayout gridLayout = new GridLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["GridLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(gridLayout);
			return gridLayout;
		}

		public static GridLayout FromXMLReader(XmlReader xmlReader)
		{
			GridLayout gridLayout = new GridLayout();
			XmlDocument xmlDocument = LayoutImportLimits.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["GridLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(gridLayout);
			return gridLayout;
		}

		public GridLayout()
		{
			_childLayouts = new GridLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
		}

		internal MxDataGrid ComputeDataGrid()
		{
			if (_gridLayoutCollection != null)
			{
				return _gridLayoutCollection.ComputeDataGrid();
			}
			return null;
		}

		internal void ProcessDataGrid(MxDataGrid dataGrid)
		{
			if (_dataGrid != dataGrid)
			{
				if (dataGrid == null)
				{
					_isCurrentLayout = false;
				}
				_dataGrid = dataGrid;
				if (_dataGrid != null)
				{
					_isCurrentLayout = true;
				}
			}
		}

		internal PersistenceMode GetPersistenceMode()
		{
			return _persistenceMode;
		}

		internal void UpdatePersistenceMode(PersistenceMode persistenceMode)
		{
			if (persistenceMode != _persistenceMode)
			{
				_persistenceMode = persistenceMode;
			}
		}

		internal GridLayoutCollection GetOwnerCollection()
		{
			return _gridLayoutCollection;
		}

		internal void SetOwnerCollection(GridLayoutCollection gridLayoutCollection)
		{
			_gridLayoutCollection = gridLayoutCollection;
		}

		internal GridLayout GetMergedLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			GridLayout parentLayout = ParentLayout.GetMergedLayout();
			GridLayout gridLayout = new GridLayout();
			mordexLayoutMerger.MergeLayouts(gridLayout, parentLayout, this);
			gridLayout._dataMember = DataMember;
			gridLayout._dataSource = _dataSource;
			return gridLayout;
		}

		public void Update()
		{
			if (_dataGrid == null)
			{
				return;
			}
			if (ParentLayout == null)
			{
				GridLayout layout = _dataGrid.GetLayout();
				_layoutData = layout._layoutData;
				_layoutReferences = layout.LayoutReferences;
				_dataSource = layout._dataSource;
				_dataMember = layout._dataMember;
				_isModified = false;
				if (_childLayouts == null || _childLayouts.Count <= 0)
				{
					return;
				}
				{
					foreach (GridLayout item in _childLayouts)
					{
						_ = item;
					}
					return;
				}
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			GridLayout otherLayout = _dataGrid.GetLayout();
			_dataSource = otherLayout._dataSource;
			_dataMember = otherLayout._dataMember;
			mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), otherLayout);
			_isModified = false;
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("GridLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.DataGrid.MxDataGrid";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("GridLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.DataGrid.MxDataGrid";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("GridLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.DataGrid.MxDataGrid";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void ProcessStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["GridLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("GridLayout", this);
		}

		public object Clone()
		{
			GridLayout gridLayout = new GridLayout();
			gridLayout._key = _key;
			gridLayout._description = _description;
			gridLayout._layoutData = _layoutData;
			foreach (LayoutReference item in _layoutReferences)
			{
				gridLayout.LayoutReferences.Add(item);
			}
			gridLayout._dataMember = _dataMember;
			gridLayout._dataSource = _dataSource;
			gridLayout._tag = _tag;
			return gridLayout;
		}

		public GridLayout CloneLayout(bool includeChildren)
		{
			GridLayout gridLayout = (GridLayout)Clone();
			if (includeChildren)
			{
				foreach (GridLayout item in _childLayouts)
				{
					gridLayout._childLayouts.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return gridLayout;
		}

		public bool IsDirty()
		{
			if (_dataGrid != null)
			{
				if (_isModified)
				{
					return true;
				}
				GridLayout layout = _dataGrid.GetLayout();
				if (ParentLayout != null)
				{
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					GridLayout gridLayout = new GridLayout();
					gridLayout._dataSource = layout._dataSource;
					gridLayout._dataMember = layout.DataMember;
					mordexLayoutMerger.CreateIncrementalLayout(gridLayout, ParentLayout.GetMergedLayout(), layout);
					return !gridLayout.IsSameAs(this);
				}
				return !layout.IsSameAs(GetMergedLayout());
			}
			return false;
		}

		private bool IsSameAs(GridLayout gridLayout)
		{
			if (_dataSource != gridLayout._dataSource)
			{
				return false;
			}
			if (_dataMember != gridLayout._dataMember)
			{
				return false;
			}
			if (gridLayout._layoutData != _layoutData)
			{
				return false;
			}
			if (gridLayout.LayoutReferences.Count > 0)
			{
				return false;
			}
			if (_layoutReferences.Count > 0)
			{
				return false;
			}
			return true;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			_layoutReferences.Clear();
			info.DeserializeObject("LayoutReferences", _layoutReferences);
			_isCurrentLayout = (bool)LayoutReader.GetPropertyValue(info, "IsCurrentLayout", typeof(bool), false);
			_description = (string)LayoutReader.GetPropertyValue(info, "Description", typeof(string), "");
			info.DeserializeObject("ChildLayouts", _childLayouts);
			string text = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			if (text != null)
			{
				string propertyValue = (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null);
				if (propertyValue == null)
				{
					_tag = text;
				}
				else
				{
					_tag = LayoutTypeResolver.ConvertOrKeepText(text, propertyValue);
				}
			}
			_layoutData = ((RawXML)LayoutReader.GetPropertyValue(info, "LayoutString", typeof(RawXML), new RawXML(""))).XML;
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
			if (_layoutReferences.Count > 0)
			{
				info.AddValue("LayoutReferences", _layoutReferences);
			}
			if (_description != "")
			{
				info.AddValue("Description", _description);
			}
			if (_layoutData != "")
			{
				PropertyValue propertyValue = new PropertyValue("LayoutString", new RawXML(_layoutData));
				info.AddValue(propertyValue);
			}
			if (_isCurrentLayout)
			{
				info.AddValue("IsCurrentLayout", _isCurrentLayout);
			}
			if (_tag != null)
			{
				if (!(_tag is string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				info.AddValue("Tag", _tag);
			}
			if (_childLayouts.Count > 0)
			{
				info.AddValue("ChildLayouts", _childLayouts);
			}
		}

		private void OnControlDisposed(object sender, EventArgs eventArgs)
		{
			DataSource = null;
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			_dataSource = dataSource;
			_dataMember = dataMember;
			if (_dataGrid != null)
			{
				_dataGrid.SetDataBinding(_dataSource, _dataMember);
			}
		}

		internal void EnsureIncrementalLayout()
		{
			if (ParentLayout != null && (_layoutData == "" || !LayoutMerger.IsIncrementalLayout(_layoutData)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), this);
			}
		}

		internal GridLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (GridLayout item in _childLayouts)
			{
				GridLayout currentLayout = item.FindCurrentLayout();
				if (currentLayout != null)
				{
					return currentLayout;
				}
			}
			return null;
		}

		internal bool HasFullLayout()
		{
			if (LayoutMerger.IsIncrementalLayout(_layoutData))
			{
				if (ParentLayout == null)
				{
					return false;
				}
				return ParentLayout.HasFullLayout();
			}
			return true;
		}

		public GridLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			GridLayout gridLayout = GetMergedLayout();
			gridLayout._dataMember = _dataMember;
			gridLayout._dataSource = _dataSource;
			gridLayout._isCurrentLayout = false;
			gridLayout._key = _key;
			gridLayout._tag = _tag;
			return gridLayout;
		}

		internal void UpdateIsCurrentLayout(GridLayout gridLayout)
		{
			if (this != gridLayout)
			{
				_isCurrentLayout = false;
			}
			ChildLayouts.MarkCurrentLayout(gridLayout);
		}
	}
}
