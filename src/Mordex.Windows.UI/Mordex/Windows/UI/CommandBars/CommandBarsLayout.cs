using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.CommandBars
{
	[TypeConverter(typeof(CommandBarsLayoutConverter))]
	[DesignerSerializer("Mordex.Windows.UI.Design.CommandBarsLayoutSerializer, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class CommandBarsLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxCommandManager _commandManager;

		private string _key = "";

		private object _tag;

		private CustomPropertyCollection _customProperties;

		private string _layoutString = "";

		internal bool _isCurrentLayout;

		private CommandBarsLayoutCollection _owningCollection;

		private bool _isModified;

		private string _description = "";

		private LayoutReferenceCollection _layoutReferences;

		private CommandBarsLayoutCollection _childLayout;

		[Description("Represents the key used to identify the layout in a collection")]
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
					if (_owningCollection != null && string.Compare(_key, value, ignoreCase: true, CultureInfo.InvariantCulture) != 0 && _owningCollection.Contains(value))
					{
						throw new InvalidOperationException("There is already another object in the collection with this key.");
					}
					_key = value;
				}
			}
		}

		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
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

		public CustomPropertyCollection CustomProperties => _customProperties;

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
				if (FindCommandManager() != null)
				{
					if (FindCommandManager().CurrentLayout == this)
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
						_owningCollection.FindCommandManager().CurrentLayout = this;
					}
					else
					{
						_owningCollection.FindCommandManager().CurrentLayout = null;
					}
				}
				else
				{
					_isCurrentLayout = value;
				}
			}
		}

		[Browsable(false)]
		public CommandBarsLayout ParentLayout
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

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		string IMordexLayout.DesignTimeName
		{
			get
			{
				if (GetOwningCollection() == null)
				{
					return "DesignTimeLayout";
				}
				if (ParentLayout == null)
				{
					return "Layout_" + GetOwningCollection().IndexOf(this);
				}
				return GetOwningCollection().IndexOf(this).ToString();
			}
		}

		object IMordexLayout.Owner
		{
			get
			{
				if (ParentLayout == null)
				{
					if (GetOwningCollection() == null)
					{
						return null;
					}
					return GetOwningCollection().CommandManager;
				}
				return ParentLayout;
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public LayoutReferenceCollection LayoutReferences => _layoutReferences;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandBarsLayoutCollection ChildLayouts => _childLayout;

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

		public static CommandBarsLayout FromStream(Stream stream)
		{
			CommandBarsLayout uiCommandBarsLayout = new CommandBarsLayout();
			uiCommandBarsLayout.LoadFromStream(stream);
			return uiCommandBarsLayout;
		}

		public static CommandBarsLayout FromXMLString(string xmlString)
		{
			CommandBarsLayout uiCommandBarsLayout = new CommandBarsLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["UICommandBarsLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(uiCommandBarsLayout);
			return uiCommandBarsLayout;
		}

		public static CommandBarsLayout FromXMLReader(XmlReader xmlReader)
		{
			CommandBarsLayout uiCommandBarsLayout = new CommandBarsLayout();
			XmlDocument xmlDocument = LayoutImportLimits.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["UICommandBarsLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(uiCommandBarsLayout);
			return uiCommandBarsLayout;
		}

		public CommandBarsLayout()
		{
			_childLayout = new CommandBarsLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
			_customProperties = new CustomPropertyCollection();
		}

		internal void AttachCommandManager(MxCommandManager uiCommandManager)
		{
			if (_commandManager != uiCommandManager)
			{
				if (uiCommandManager == null)
				{
					_isCurrentLayout = false;
				}
				_commandManager = uiCommandManager;
				if (_commandManager != null)
				{
					_isCurrentLayout = true;
				}
			}
		}

		internal CommandBarsLayout ResolveMergedLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			CommandBarsLayout parentLayout = ParentLayout.ResolveMergedLayout();
			CommandBarsLayout uiCommandBarsLayout = new CommandBarsLayout();
			mordexLayoutMerger.MergeLayouts(uiCommandBarsLayout, parentLayout, this);
			return uiCommandBarsLayout;
		}

		internal MxCommandManager FindCommandManager()
		{
			if (_owningCollection != null)
			{
				return _owningCollection.FindCommandManager();
			}
			return null;
		}

		internal CommandBarsLayoutCollection GetOwningCollection()
		{
			return _owningCollection;
		}

		internal void SetOwningCollection(CommandBarsLayoutCollection uiCommandBarsLayoutCollection)
		{
			_owningCollection = uiCommandBarsLayoutCollection;
		}

		public void Update()
		{
			if (_commandManager != null)
			{
				if (ParentLayout == null)
				{
					CommandBarsLayout layout = _commandManager.GetLayout();
					_layoutString = layout._layoutString;
					_layoutReferences = layout._layoutReferences;
					_isModified = false;
				}
				else
				{
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					CommandBarsLayout currentLayout = _commandManager.GetLayout();
					mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.ResolveMergedLayout(), currentLayout);
					_isModified = false;
				}
			}
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("UICommandBarsLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.CommandBars.MxCommandManager";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("UICommandBarsLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.CommandBars.MxCommandManager";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("UICommandBarsLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.CommandBars.MxCommandManager";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void LoadFromStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["UICommandBarsLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("UICommandBarsLayout", this);
		}

		public object Clone()
		{
			CommandBarsLayout uiCommandBarsLayout = new CommandBarsLayout();
			uiCommandBarsLayout._key = _key;
			uiCommandBarsLayout._layoutString = _layoutString;
			foreach (LayoutReference item in _layoutReferences)
			{
				uiCommandBarsLayout.LayoutReferences.Add(item);
			}
			uiCommandBarsLayout._tag = _tag;
			uiCommandBarsLayout._description = _description;
			return uiCommandBarsLayout;
		}

		public CommandBarsLayout CloneLayout(bool includeChildren)
		{
			CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)Clone();
			if (includeChildren)
			{
				foreach (CommandBarsLayout item in _childLayout)
				{
					uiCommandBarsLayout._childLayout.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return uiCommandBarsLayout;
		}

		public bool IsDirty()
		{
			if (_commandManager != null)
			{
				if (_isModified)
				{
					return true;
				}
				CommandBarsLayout layout = _commandManager.GetLayout();
				return !layout.HasSameContent(ResolveMergedLayout());
			}
			return false;
		}

		private bool HasSameContent(CommandBarsLayout uiCommandBarsLayout)
		{
			if (uiCommandBarsLayout._layoutString != _layoutString)
			{
				return false;
			}
			if (uiCommandBarsLayout.LayoutReferences.Count > 0 || LayoutReferences.Count > 0)
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
			reader.DeserializeObject("ChildLayout", _childLayout);
			PropertyValue propertyValue = reader["CustomProperties"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_customProperties);
			}
			else
			{
				_customProperties.Clear();
			}
			string tagText = (string)LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null);
			if (tagText != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(reader, "TagTypeName", typeof(string), null);
				if (tagTypeName == null)
				{
					_tag = tagText;
				}
				else
				{
					_tag = LayoutTypePolicy.ConvertOrKeepText(tagText, tagTypeName);
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
			if (_customProperties.Count > 0)
			{
				writer.AddValue("CustomProperties", _customProperties);
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

		internal void CreateIncrementalLayoutIfMissing()
		{
			if (ParentLayout != null && (_layoutString == "" || !LayoutMerger.IsIncrementalLayout(_layoutString)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.ResolveMergedLayout(), this);
			}
		}

		internal CommandBarsLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (CommandBarsLayout item in _childLayout)
			{
				CommandBarsLayout currentChild = item.FindCurrentLayout();
				if (currentChild != null)
				{
					return currentChild;
				}
			}
			return null;
		}

		internal bool IsResolvable()
		{
			if (LayoutMerger.IsIncrementalLayout(_layoutString))
			{
				if (ParentLayout == null)
				{
					return false;
				}
				return ParentLayout.IsResolvable();
			}
			return true;
		}

		public CommandBarsLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			CommandBarsLayout uiCommandBarsLayout = ResolveMergedLayout();
			uiCommandBarsLayout._isCurrentLayout = false;
			uiCommandBarsLayout._key = _key;
			uiCommandBarsLayout._tag = _tag;
			uiCommandBarsLayout._description = _description;
			return uiCommandBarsLayout;
		}
	}
}
