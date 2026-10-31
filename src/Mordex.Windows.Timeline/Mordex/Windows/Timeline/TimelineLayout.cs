using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Timeline
{
	[DesignerSerializer("Mordex.Windows.Timeline.Design.TimelineLayoutSerializer, Mordex.Windows.Timeline.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[TypeConverter(typeof(TimelineLayoutConverter))]
	public class TimelineLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxTimeline _timeline;

		private string _key = "";

		private object _tag;

		private PersistenceMode _persistenceMode = PersistenceMode.All;

		private string _layoutString = "";

		internal bool _isCurrentLayout;

		private TimelineLayoutCollection _owningCollection;

		private bool _isDirty;

		private string _description = "";

		internal MxTimeline _owner;

		private object _dataSource;

		private string _dataMember = "";

		private TimelineLayoutCollection _childLayouts;

		private LayoutReferenceCollection _layoutReferences;

		[Browsable(false)]
		public TimelineLayout ParentLayout
		{
			get
			{
				if (_owningCollection != null)
				{
					return _owningCollection.GetParentLayout();
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
					if (_owningCollection != null && string.Compare(_key, value, ignoreCase: true) != 0 && _owningCollection.Contains(value))
					{
						throw new InvalidOperationException("There is already another object in the collection with this key.");
					}
					_key = value;
				}
			}
		}

		[DefaultValue(null)]
		[TypeConverter("System.ComponentModel.StringConverter")]
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
		[Localizable(true)]
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
				if (GetTimeline() != null)
				{
					if (GetTimeline().CurrentLayout == this)
					{
						return true;
					}
					return false;
				}
				return _isCurrentLayout;
			}
			set
			{
				if (_owningCollection != null)
				{
					if (value)
					{
						_owningCollection.GetTimeline().CurrentLayout = this;
					}
					else
					{
						_owningCollection.GetTimeline().CurrentLayout = null;
					}
				}
				else
				{
					_isCurrentLayout = value;
				}
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

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		string IMordexLayout.DesignTimeName
		{
			get
			{
				if (GetTimelineLayoutCollection() == null)
				{
					return "DesignTimeLayout";
				}
				if (ParentLayout == null)
				{
					return "Layout_" + GetTimelineLayoutCollection().IndexOf(this);
				}
				return GetTimelineLayoutCollection().IndexOf(this).ToString();
			}
		}

		object IMordexLayout.Owner
		{
			get
			{
				if (ParentLayout == null)
				{
					if (GetTimelineLayoutCollection() == null)
					{
						return _owner;
					}
					return GetTimelineLayoutCollection().GetOwnerTimeline();
				}
				return ParentLayout;
			}
		}

		[Description("Represents the data source from which the Timeline control displays its data when this instance is the current layout.")]
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
					if (_dataSource is IComponent)
					{
						((IComponent)_dataSource).Disposed -= OnDataSourceDisposed;
					}
					_dataSource = value;
					if (_dataSource is IComponent)
					{
						((IComponent)_dataSource).Disposed += OnDataSourceDisposed;
					}
					if (_timeline != null)
					{
						_timeline.SetDataBinding(_dataSource, _dataMember);
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
					if (_timeline != null)
					{
						_timeline.SetDataBinding(_dataSource, _dataMember);
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public TimelineLayoutCollection ChildLayouts => _childLayouts;

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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public LayoutReferenceCollection LayoutReferences => _layoutReferences;

		public static TimelineLayout FromStream(Stream stream)
		{
			TimelineLayout timelineLayout = new TimelineLayout();
			timelineLayout.LoadFromStream(stream);
			return timelineLayout;
		}

		public static TimelineLayout FromXMLString(string xmlString)
		{
			TimelineLayout timelineLayout = new TimelineLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["TimelineLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(timelineLayout);
			return timelineLayout;
		}

		public static TimelineLayout FromXMLReader(XmlReader xmlReader)
		{
			TimelineLayout timelineLayout = new TimelineLayout();
			XmlDocument xmlDocument = LayoutImportLimits.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["TimelineLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(timelineLayout);
			return timelineLayout;
		}

		public TimelineLayout()
		{
			_childLayouts = new TimelineLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
		}

		internal MxTimeline GetTimeline()
		{
			if (_owningCollection != null)
			{
				return _owningCollection.GetTimeline();
			}
			return null;
		}

		internal void SetTimeline(MxTimeline timeline)
		{
			if (_timeline != timeline)
			{
				if (timeline == null)
				{
					_isCurrentLayout = false;
				}
				_timeline = timeline;
				if (_timeline != null)
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

		internal TimelineLayout GetEffectiveLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			TimelineLayout parentLayout = ParentLayout.GetEffectiveLayout();
			TimelineLayout timelineLayout = new TimelineLayout();
			mordexLayoutMerger.MergeLayouts(timelineLayout, parentLayout, this);
			timelineLayout._dataMember = DataMember;
			timelineLayout._dataSource = _dataSource;
			return timelineLayout;
		}

		internal TimelineLayoutCollection GetTimelineLayoutCollection()
		{
			return _owningCollection;
		}

		internal void SetTimelineLayoutCollection(TimelineLayoutCollection timelineLayoutCollection)
		{
			_owningCollection = timelineLayoutCollection;
		}

		public void Update()
		{
			if (_timeline != null)
			{
				if (ParentLayout == null)
				{
					TimelineLayout layout = _timeline.GetLayout();
					_layoutString = layout._layoutString;
					_layoutReferences = layout.LayoutReferences;
					_dataSource = layout._dataSource;
					_dataMember = layout._dataMember;
					_isDirty = false;
				}
				else
				{
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					TimelineLayout currentLayout = _timeline.GetLayout();
					_dataSource = currentLayout._dataSource;
					_dataMember = currentLayout._dataMember;
					mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetEffectiveLayout(), currentLayout);
					_isDirty = false;
				}
			}
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("TimelineLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Timeline.MxTimeline";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("TimelineLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Timeline.MxTimeline";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("TimelineLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Timeline.MxTimeline";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void LoadFromStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["TimelineLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("TimelineLayout", this);
		}

		public object Clone()
		{
			TimelineLayout timelineLayout = new TimelineLayout();
			timelineLayout._key = _key;
			foreach (LayoutReference item in _layoutReferences)
			{
				timelineLayout.LayoutReferences.Add(item);
			}
			timelineLayout._layoutString = _layoutString;
			timelineLayout._dataMember = _dataMember;
			timelineLayout._dataSource = _dataSource;
			timelineLayout._description = _description;
			timelineLayout._tag = _tag;
			return timelineLayout;
		}

		public TimelineLayout CloneLayout(bool includeChildren)
		{
			TimelineLayout timelineLayout = (TimelineLayout)Clone();
			if (includeChildren)
			{
				foreach (TimelineLayout item in _childLayouts)
				{
					timelineLayout._childLayouts.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return timelineLayout;
		}

		public bool IsDirty()
		{
			if (_timeline != null)
			{
				if (_isDirty)
				{
					return true;
				}
				_timeline.GetLayout();
				return !IsSameLayout(GetEffectiveLayout());
			}
			return false;
		}

		private bool IsSameLayout(TimelineLayout timelineLayout)
		{
			if (_dataSource != timelineLayout._dataSource)
			{
				return false;
			}
			if (_dataMember != timelineLayout._dataMember)
			{
				return false;
			}
			if (timelineLayout._layoutString != _layoutString)
			{
				return false;
			}
			if (timelineLayout.LayoutReferences.Count > 0)
			{
				return false;
			}
			if (_layoutReferences.Count > 0)
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
			reader.DeserializeObject("ChildLayouts", _childLayouts);
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
			if (_childLayouts.Count > 0)
			{
				writer.AddValue("ChildLayouts", _childLayouts);
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
			if (_timeline != null)
			{
				_timeline.SetDataBinding(_dataSource, _dataMember);
			}
		}

		internal void UpdateTimelineLayout()
		{
			if (ParentLayout != null && (_layoutString == "" || !LayoutMerger.IsIncrementalLayout(_layoutString)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetEffectiveLayout(), this);
			}
		}

		internal TimelineLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (TimelineLayout item in _childLayouts)
			{
				TimelineLayout foundLayout = item.FindCurrentLayout();
				if (foundLayout != null)
				{
					return foundLayout;
				}
			}
			return null;
		}

		internal bool CanBeLoaded()
		{
			if (LayoutMerger.IsIncrementalLayout(_layoutString))
			{
				if (ParentLayout == null)
				{
					return false;
				}
				return ParentLayout.CanBeLoaded();
			}
			return true;
		}

		public TimelineLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			TimelineLayout timelineLayout = GetEffectiveLayout();
			timelineLayout._dataMember = _dataMember;
			timelineLayout._dataSource = _dataSource;
			timelineLayout._isCurrentLayout = false;
			timelineLayout._key = _key;
			timelineLayout._tag = _tag;
			timelineLayout._description = _description;
			return timelineLayout;
		}
	}
}
