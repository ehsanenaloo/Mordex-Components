using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.TaskPane
{
	[TypeConverter(typeof(TaskPaneLayoutConverter))]
	[DesignerSerializer("Mordex.Windows.TaskPane.Design.TaskPaneLayoutSerializer, Mordex.Windows.TaskPane.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class TaskPaneLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxTaskPane _taskPane;

		private string _key = "";

		private object _tag;

		private string _layoutString = "";

		internal bool _isCurrentLayout;

		private TaskPaneLayoutCollection _parentCollection;

		private bool _isModified;

		private string _description = "";

		private TaskPaneLayoutCollection _childLayouts;

		private LayoutReferenceCollection _layoutReferences;

		[Browsable(false)]
		public TaskPaneLayout ParentLayout
		{
			get
			{
				if (_parentCollection != null)
				{
					return _parentCollection.GetParentLayout();
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
					if (_parentCollection != null && !string.Equals(_key, value, StringComparison.OrdinalIgnoreCase) && _parentCollection.Contains(value))
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
				if (ResolveTaskPane() != null)
				{
					if (ResolveTaskPane().CurrentLayout == this)
					{
						return true;
					}
					return false;
				}
				return _isCurrentLayout;
			}
			set
			{
				if (_parentCollection != null)
				{
					if (value)
					{
						_parentCollection.ResolveTaskPane().CurrentLayout = this;
					}
					else
					{
						_parentCollection.ResolveTaskPane().CurrentLayout = null;
					}
				}
				else
				{
					_isCurrentLayout = value;
				}
			}
		}

		[DefaultValue("")]
		[Description("Represents a string that can be used to store an informational description in the layout.")]
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
						return null;
					}
					return GetOwnerCollection().GetOwnerTaskPane();
				}
				return ParentLayout;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TaskPaneLayoutCollection ChildLayouts => _childLayouts;

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

		public static TaskPaneLayout FromStream(Stream stream)
		{
			TaskPaneLayout taskPaneLayout = new TaskPaneLayout();
			taskPaneLayout.LoadFromStream(stream);
			return taskPaneLayout;
		}

		public static TaskPaneLayout FromXMLString(string xmlString)
		{
			TaskPaneLayout taskPaneLayout = new TaskPaneLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["TaskPaneLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(taskPaneLayout);
			return taskPaneLayout;
		}

		public static TaskPaneLayout FromXMLReader(XmlReader xmlReader)
		{
			TaskPaneLayout taskPaneLayout = new TaskPaneLayout();
			XmlDocument xmlDocument = LayoutImportLimits.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["TaskPaneLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(taskPaneLayout);
			return taskPaneLayout;
		}

		public TaskPaneLayout()
		{
			_childLayouts = new TaskPaneLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
		}

		internal MxTaskPane ResolveTaskPane()
		{
			if (_parentCollection != null)
			{
				return _parentCollection.ResolveTaskPane();
			}
			return null;
		}

		internal void AttachTaskPane(MxTaskPane taskPane)
		{
			if (_taskPane != taskPane)
			{
				if (taskPane == null)
				{
					_isCurrentLayout = false;
				}
				_taskPane = taskPane;
				if (_taskPane != null)
				{
					_isCurrentLayout = true;
				}
			}
		}

		internal TaskPaneLayout ResolveEffectiveLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			TaskPaneLayout parentLayout = ParentLayout.ResolveEffectiveLayout();
			TaskPaneLayout taskPaneLayout = new TaskPaneLayout();
			mordexLayoutMerger.MergeLayouts(taskPaneLayout, parentLayout, this);
			return taskPaneLayout;
		}

		internal TaskPaneLayoutCollection GetOwnerCollection()
		{
			return _parentCollection;
		}

		internal void SetOwnerCollection(TaskPaneLayoutCollection taskPaneLayoutCollection)
		{
			_parentCollection = taskPaneLayoutCollection;
		}

		public void Update()
		{
			if (_taskPane != null)
			{
				if (ParentLayout == null)
				{
					TaskPaneLayout layout = _taskPane.GetLayout();
					_layoutReferences = layout.LayoutReferences;
					_layoutString = layout._layoutString;
					_isModified = false;
				}
				else
				{
					ParentLayout.ResolveEffectiveLayout();
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					TaskPaneLayout paneLayout = _taskPane.GetLayout();
					mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.ResolveEffectiveLayout(), paneLayout);
					_isModified = false;
				}
			}
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("TaskPaneLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.TaskPane.MxTaskPane";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("TaskPaneLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.TaskPane.MxTaskPane";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("TaskPaneLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.TaskPane.MxTaskPane";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void LoadFromStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["TaskPaneLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("TaskPaneLayout", this);
		}

		public object Clone()
		{
			TaskPaneLayout taskPaneLayout = new TaskPaneLayout();
			taskPaneLayout._key = _key;
			taskPaneLayout._layoutString = _layoutString;
			taskPaneLayout._description = _description;
			taskPaneLayout._tag = _tag;
			foreach (LayoutReference item in _layoutReferences)
			{
				taskPaneLayout._layoutReferences.Add(new LayoutReference(item.ReferenceId, item.Instance));
			}
			return taskPaneLayout;
		}

		public TaskPaneLayout CloneLayout(bool includeChildren)
		{
			TaskPaneLayout taskPaneLayout = (TaskPaneLayout)Clone();
			if (includeChildren)
			{
				foreach (TaskPaneLayout item in _childLayouts)
				{
					taskPaneLayout._childLayouts.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return taskPaneLayout;
		}

		public bool IsDirty()
		{
			if (_taskPane != null)
			{
				if (_isModified)
				{
					return true;
				}
				_taskPane.GetLayout();
				return !IsEquivalentTo(ResolveEffectiveLayout());
			}
			return false;
		}

		private bool IsEquivalentTo(TaskPaneLayout taskPaneLayout)
		{
			if (taskPaneLayout._layoutString != _layoutString)
			{
				return false;
			}
			if (taskPaneLayout.LayoutReferences.Count > 0)
			{
				return false;
			}
			if (LayoutReferences.Count > 0)
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
			string tagString = (string)LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null);
			_tag = LayoutTypePolicy.ConvertOrKeepText(tagString, (string)LayoutReader.GetPropertyValue(reader, "TagTypeName", typeof(string), null));
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

		internal void EnsureIncrementalLayout()
		{
			if (ParentLayout != null && (_layoutString == "" || !LayoutMerger.IsIncrementalLayout(_layoutString)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.ResolveEffectiveLayout(), this);
			}
		}

		internal TaskPaneLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (TaskPaneLayout item in _childLayouts)
			{
				TaskPaneLayout foundLayout = item.FindCurrentLayout();
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

		public TaskPaneLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			TaskPaneLayout taskPaneLayout = ResolveEffectiveLayout();
			taskPaneLayout._isCurrentLayout = false;
			taskPaneLayout._key = _key;
			taskPaneLayout._tag = _tag;
			taskPaneLayout._description = _description;
			return taskPaneLayout;
		}
	}
}
