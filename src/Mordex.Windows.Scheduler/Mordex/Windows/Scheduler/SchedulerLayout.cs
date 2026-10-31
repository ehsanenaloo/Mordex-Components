using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter(typeof(SchedulerLayoutConverter))]
	[DesignerSerializer("Mordex.Windows.Scheduler.Design.SchedulerLayoutSerializer, Mordex.Windows.Scheduler.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class SchedulerLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxScheduler _scheduler;

		private string _key = "";

		private object _tag;

		private PersistenceMode _persistenceMode = PersistenceMode.All;

		private string _layoutString = "";

		internal bool _isCurrentLayout;

		private SchedulerLayoutCollection _layoutCollection;

		private bool _isDirty;

		internal MxScheduler _owner;

		private string _description = "";

		private object _dataSource;

		private string _dataMember = "";

		private LayoutReferenceCollection _layoutReferences;

		private SchedulerLayoutCollection _childLayout;

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
					if (_layoutCollection != null && string.Compare(_key, value, ignoreCase: true) != 0 && _layoutCollection.Contains(value))
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

		[Localizable(true)]
		[DefaultValue("")]
		[Browsable(false)]
		public string LayoutString
		{
			get
			{
				return _layoutString;
			}
			set
			{
				_layoutString = value;
			}
		}

		[DefaultValue(false)]
		public bool IsCurrentLayout
		{
			get
			{
				if (GetScheduler() != null)
				{
					if (GetScheduler().CurrentLayout == this)
					{
						return true;
					}
					return false;
				}
				return _isCurrentLayout;
			}
			set
			{
				if (_layoutCollection != null)
				{
					if (value)
					{
						_layoutCollection.GetOwnerScheduler().CurrentLayout = this;
					}
					else
					{
						_layoutCollection.GetOwnerScheduler().CurrentLayout = null;
					}
				}
				else
				{
					_isCurrentLayout = value;
				}
			}
		}

		[Browsable(false)]
		public SchedulerLayout ParentLayout
		{
			get
			{
				if (_layoutCollection != null)
				{
					return _layoutCollection.GetParentLayout();
				}
				return null;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		string IMordexLayout.DesignTimeName
		{
			get
			{
				if (GetSchedulerLayoutCollection() == null)
				{
					return "DesignTimeLayout";
				}
				if (ParentLayout == null)
				{
					return "Layout_" + GetSchedulerLayoutCollection().IndexOf(this);
				}
				return GetSchedulerLayoutCollection().IndexOf(this).ToString();
			}
		}

		object IMordexLayout.Owner
		{
			get
			{
				if (ParentLayout == null)
				{
					if (GetSchedulerLayoutCollection() == null)
					{
						return _owner;
					}
					return GetSchedulerLayoutCollection().Scheduler;
				}
				return ParentLayout;
			}
		}

		[Description("Represents a string that can be used to store an informational description in the layout.")]
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
		[DefaultValue(null)]
		[Description("Represents the data source from which the Schedule control displays its data when this instance is the current layout.")]
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
					if (_dataSource is IComponent)
					{
						((IComponent)_dataSource).Disposed -= OnDataSourceDisposed;
					}
					_dataSource = value;
					if (_dataSource is IComponent)
					{
						((IComponent)_dataSource).Disposed += OnDataSourceDisposed;
					}
					if (GetSchedule() != null)
					{
						GetSchedule().SetDataBinding(_dataSource, _dataMember);
					}
				}
			}
		}

		[Description("The name of a list in a data source from which the control displays its data.")]
		[DefaultValue("")]
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
					if (GetSchedule() != null)
					{
						GetSchedule().SetDataBinding(_dataSource, _dataMember);
					}
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public LayoutReferenceCollection LayoutReferences => _layoutReferences;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public SchedulerLayoutCollection ChildLayouts => _childLayout;

		[Browsable(false)]
		public bool StandAlone
		{
			get
			{
				if (_layoutString != "" && LayoutMerger.IsIncrementalLayout(_layoutString))
				{
					return false;
				}
				return true;
			}
		}

		public static SchedulerLayout FromStream(Stream stream)
		{
			SchedulerLayout schedulerLayout = new SchedulerLayout();
			schedulerLayout.LoadFromStream(stream);
			return schedulerLayout;
		}

		public static SchedulerLayout FromXMLString(string xmlString)
		{
			SchedulerLayout schedulerLayout = new SchedulerLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["SchedulerLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(schedulerLayout);
			return schedulerLayout;
		}

		public static SchedulerLayout FromXMLReader(XmlReader xmlReader)
		{
			SchedulerLayout schedulerLayout = new SchedulerLayout();
			XmlDocument xmlDocument = LayoutImportLimits.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["SchedulerLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(schedulerLayout);
			return schedulerLayout;
		}

		public SchedulerLayout()
		{
			_childLayout = new SchedulerLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
			_owner = null;
		}

		internal MxScheduler GetSchedule()
		{
			return _scheduler;
		}

		internal void SetSchedule(MxScheduler scheduler)
		{
			if (_scheduler != scheduler)
			{
				if (scheduler == null)
				{
					_isCurrentLayout = false;
				}
				_scheduler = scheduler;
				if (_scheduler != null)
				{
					_isCurrentLayout = true;
				}
			}
		}

		internal PersistenceMode GetPersistenceMode()
		{
			return _persistenceMode;
		}

		internal void SetPersistenceMode(PersistenceMode persistenceMode)
		{
			_persistenceMode = persistenceMode;
		}

		internal SchedulerLayout GetMergedLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			SchedulerLayout parentLayout = ParentLayout.GetMergedLayout();
			SchedulerLayout schedulerLayout = new SchedulerLayout();
			mordexLayoutMerger.MergeLayouts(schedulerLayout, parentLayout, this);
			schedulerLayout._dataMember = DataMember;
			schedulerLayout._dataSource = _dataSource;
			return schedulerLayout;
		}

		internal MxScheduler GetScheduler()
		{
			if (_layoutCollection != null)
			{
				return _layoutCollection.GetOwnerScheduler();
			}
			return null;
		}

		internal SchedulerLayoutCollection GetSchedulerLayoutCollection()
		{
			return _layoutCollection;
		}

		internal void SetSchedulerLayoutCollection(SchedulerLayoutCollection schedulerLayoutCollection)
		{
			_layoutCollection = schedulerLayoutCollection;
		}

		public void Update()
		{
			if (_scheduler != null)
			{
				if (ParentLayout == null)
				{
					SchedulerLayout layout = _scheduler.GetLayout();
					_layoutString = layout._layoutString;
					_layoutReferences = layout._layoutReferences;
					_dataSource = layout._dataSource;
					_dataMember = layout._dataMember;
					_isDirty = false;
				}
				else
				{
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					SchedulerLayout currentLayout = _scheduler.GetLayout();
					_dataSource = currentLayout._dataSource;
					_dataMember = currentLayout._dataMember;
					mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), currentLayout);
					_isDirty = false;
				}
			}
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("SchedulerLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Scheduler.MxScheduler";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("SchedulerLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Scheduler.MxScheduler";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("SchedulerLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Scheduler.MxScheduler";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void LoadFromStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["SchedulerLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("SchedulerLayout", this);
		}

		public object Clone()
		{
			SchedulerLayout schedulerLayout = new SchedulerLayout();
			schedulerLayout._key = _key;
			schedulerLayout._layoutString = _layoutString;
			foreach (LayoutReference item in _layoutReferences)
			{
				schedulerLayout.LayoutReferences.Add(item);
			}
			schedulerLayout._dataMember = _dataMember;
			schedulerLayout._dataSource = _dataSource;
			schedulerLayout._tag = _tag;
			schedulerLayout._description = _description;
			return schedulerLayout;
		}

		public SchedulerLayout CloneLayout(bool includeChildren)
		{
			SchedulerLayout schedulerLayout = (SchedulerLayout)Clone();
			if (includeChildren)
			{
				foreach (SchedulerLayout item in _childLayout)
				{
					schedulerLayout._childLayout.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return schedulerLayout;
		}

		public bool IsDirty()
		{
			if (_scheduler != null)
			{
				if (_isDirty)
				{
					return true;
				}
				_scheduler.GetLayout();
				return !HasSameContent(GetMergedLayout());
			}
			return false;
		}

		private bool HasSameContent(SchedulerLayout schedulerLayout)
		{
			if (_dataSource != schedulerLayout._dataSource)
			{
				return false;
			}
			if (_dataMember != schedulerLayout._dataMember)
			{
				return false;
			}
			if (schedulerLayout._layoutString != _layoutString)
			{
				return false;
			}
			if (schedulerLayout.LayoutReferences.Count > 0 || LayoutReferences.Count > 0)
			{
				return false;
			}
			return true;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), "");
			_layoutReferences.Clear();
			reader.DeserializeObject("LayoutReferences", _layoutReferences);
			_isCurrentLayout = (bool)LayoutReader.GetPropertyValue(reader, "IsCurrentLayout", typeof(bool), false);
			_description = (string)LayoutReader.GetPropertyValue(reader, "Description", typeof(string), "");
			reader.DeserializeObject("ChildLayouts", _childLayout);
			string text = (string)LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null);
			if (text != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(reader, "TagTypeName", typeof(string), null);
				if (tagTypeName == null)
				{
					_tag = text;
				}
				else
				{
					try
					{
						_tag = PropertyValue.ConvertValue(text, LayoutTypeResolver.Resolve(tagTypeName));
					}
					catch
					{
						_tag = text;
					}
				}
			}
			_layoutString = ((RawXML)LayoutReader.GetPropertyValue(reader, "LayoutString", typeof(RawXML), new RawXML(""))).XML;
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter writer)
		{
			if (_key != "")
			{
				writer.AddValue("Key", _key);
			}
			if (_layoutReferences.Count > 0)
			{
				writer.AddValue("LayoutReferences", _layoutReferences);
			}
			if (_description != "")
			{
				writer.AddValue("Description", _description);
			}
			if (_childLayout.Count > 0)
			{
				writer.AddValue("ChildLayout", _childLayout);
			}
			if (_layoutString != "")
			{
				writer.AddValue("LayoutString", new RawXML(_layoutString));
			}
			if (_isCurrentLayout)
			{
				writer.AddValue("IsCurrentLayout", _isCurrentLayout);
			}
			if (_tag != null)
			{
				if (!(_tag is string))
				{
					writer.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				writer.AddValue("Tag", _tag);
			}
		}

		private void OnDataSourceDisposed(object sender, EventArgs eventArgs)
		{
			DataSource = null;
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			_dataSource = dataSource;
			_dataMember = dataMember;
			if (_scheduler != null)
			{
				_scheduler.SetDataBinding(_dataSource, _dataMember);
			}
		}

		internal void UpdateSchedulerLayout()
		{
			if (ParentLayout != null && (_layoutString == "" || !LayoutMerger.IsIncrementalLayout(_layoutString)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), this);
			}
		}

		internal SchedulerLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (SchedulerLayout item in _childLayout)
			{
				SchedulerLayout foundLayout = item.FindCurrentLayout();
				if (foundLayout != null)
				{
					return foundLayout;
				}
			}
			return null;
		}

		internal bool CanLoad()
		{
			if (LayoutMerger.IsIncrementalLayout(_layoutString))
			{
				if (ParentLayout == null)
				{
					return false;
				}
				return ParentLayout.CanLoad();
			}
			return true;
		}

		public SchedulerLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			SchedulerLayout schedulerLayout = GetMergedLayout();
			schedulerLayout._dataMember = _dataMember;
			schedulerLayout._dataSource = _dataSource;
			schedulerLayout._isCurrentLayout = false;
			schedulerLayout._key = _key;
			schedulerLayout._tag = _tag;
			schedulerLayout._description = _description;
			return schedulerLayout;
		}
	}
}
