using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	[DesignerSerializer("Mordex.Windows.Ribbon.Design.RibbonLayoutSerializer, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[TypeConverter(typeof(RibbonLayoutConverter))]
	public class RibbonLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxRibbon _ribbon;

		private string _key = "";

		private object _tag;

		private CustomPropertyCollection _customProperties;

		private string _layoutXml = "";

		internal bool _isCurrentLayout;

		private string _description = "";

		private LayoutReferenceCollection _layoutReferences;

		private RibbonLayoutCollection _childLayouts;

		private RibbonLayoutCollection _owningCollection;

		private bool _isModified;

		[DefaultValue("")]
		[Description("Represents the key used to identify the layout in a collection")]
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

		[DefaultValue(null)]
		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
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
		[Browsable(false)]
		[Localizable(true)]
		public string LayoutString
		{
			get
			{
				return _layoutXml;
			}
			set
			{
				_layoutXml = value;
			}
		}

		[DefaultValue(false)]
		public bool IsCurrentLayout
		{
			get
			{
				if (FindRibbon() != null)
				{
					if (FindRibbon().CurrentLayout == this)
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
						_owningCollection.ResolveRibbon().CurrentLayout = this;
					}
					else
					{
						_owningCollection.ResolveRibbon().CurrentLayout = null;
					}
				}
				else
				{
					_isCurrentLayout = value;
				}
			}
		}

		[Browsable(false)]
		public RibbonLayout ParentLayout
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
					return "Layout_" + GetOwningCollection().IndexOf(this).ToString(CultureInfo.InvariantCulture);
				}
				return GetOwningCollection().IndexOf(this).ToString(CultureInfo.InvariantCulture);
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
					return GetOwningCollection().Ribbon;
				}
				return ParentLayout;
			}
		}

		LayoutReferenceCollection IMordexLayout.LayoutReferences => LayoutReferences;

		string IMordexLayout.LayoutString
		{
			get
			{
				return LayoutString;
			}
			set
			{
				LayoutString = value;
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
		public RibbonLayoutCollection ChildLayouts => _childLayouts;

		[Browsable(false)]
		public bool StandAlone
		{
			get
			{
				if (_layoutXml != "" && LayoutMerger.IsIncrementalLayout(_layoutXml))
				{
					return false;
				}
				return true;
			}
		}

		public RibbonLayout()
		{
			_childLayouts = new RibbonLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
			_customProperties = new CustomPropertyCollection();
		}

		public static RibbonLayout FromStream(Stream stream)
		{
			RibbonLayout ribbonLayout = new RibbonLayout();
			ribbonLayout.LoadFromStream(stream);
			return ribbonLayout;
		}

		public static RibbonLayout FromXMLString(string xmlString)
		{
			RibbonLayout ribbonLayout = new RibbonLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["RibbonLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(ribbonLayout);
			return ribbonLayout;
		}

		public static RibbonLayout FromXMLReader(XmlReader xmlReader)
		{
			RibbonLayout ribbonLayout = new RibbonLayout();
			XmlDocument xmlDocument = LayoutTypeRegistry.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["RibbonLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(ribbonLayout);
			return ribbonLayout;
		}

		internal void AttachRibbon(MxRibbon ribbon)
		{
			if (_ribbon != ribbon)
			{
				if (ribbon == null)
				{
					_isCurrentLayout = false;
				}
				_ribbon = ribbon;
				if (_ribbon != null)
				{
					_isCurrentLayout = true;
				}
			}
		}

		internal RibbonLayout GetMergedLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			RibbonLayout parentLayout = ParentLayout.GetMergedLayout();
			RibbonLayout ribbonLayout = new RibbonLayout();
			mordexLayoutMerger.MergeLayouts(ribbonLayout, parentLayout, this);
			return ribbonLayout;
		}

		internal MxRibbon FindRibbon()
		{
			if (_owningCollection != null)
			{
				return _owningCollection.ResolveRibbon();
			}
			return null;
		}

		internal RibbonLayoutCollection GetOwningCollection()
		{
			return _owningCollection;
		}

		internal void SetOwningCollection(RibbonLayoutCollection ribbonLayoutCollection)
		{
			_owningCollection = ribbonLayoutCollection;
		}

		public void Update()
		{
			if (_ribbon != null)
			{
				if (ParentLayout == null)
				{
					RibbonLayout layout = _ribbon.GetLayout();
					_layoutXml = layout._layoutXml;
					_layoutReferences = layout._layoutReferences;
					_isModified = false;
				}
				else
				{
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					RibbonLayout currentRibbonLayout = _ribbon.GetLayout();
					mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), currentRibbonLayout);
					_isModified = false;
				}
			}
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("RibbonLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Ribbon.MxRibbon";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("RibbonLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Ribbon.MxRibbon";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("RibbonLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Ribbon.MxRibbon";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void LoadFromStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["RibbonLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("RibbonLayout", this);
		}

		public object Clone()
		{
			RibbonLayout ribbonLayout = new RibbonLayout();
			ribbonLayout._key = _key;
			ribbonLayout._layoutXml = _layoutXml;
			foreach (LayoutReference item in _layoutReferences)
			{
				ribbonLayout.LayoutReferences.Add(item);
			}
			ribbonLayout._tag = _tag;
			ribbonLayout._description = _description;
			return ribbonLayout;
		}

		public RibbonLayout CloneLayout(bool includeChildren)
		{
			RibbonLayout ribbonLayout = (RibbonLayout)Clone();
			if (includeChildren)
			{
				foreach (RibbonLayout item in _childLayouts)
				{
					ribbonLayout._childLayouts.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return ribbonLayout;
		}

		public bool IsDirty()
		{
			if (_ribbon != null)
			{
				if (_isModified)
				{
					return true;
				}
				RibbonLayout layout = _ribbon.GetLayout();
				return !layout.IsEquivalentTo(GetMergedLayout());
			}
			return false;
		}

		private bool IsEquivalentTo(RibbonLayout ribbonLayout)
		{
			if (ribbonLayout._layoutXml != _layoutXml)
			{
				return false;
			}
			if (ribbonLayout.LayoutReferences.Count > 0 || LayoutReferences.Count > 0)
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
			reader.DeserializeObject("ChildLayout", _childLayouts);
			PropertyValue propertyValue = reader["CustomProperties"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_customProperties);
			}
			else
			{
				_customProperties.Clear();
			}
			_tag = LayoutTypeRegistry.ReadTag(reader);
			_layoutXml = ((RawXML)LayoutReader.GetPropertyValue(reader, "LayoutString", typeof(RawXML), new RawXML(""))).XML;
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
			if (_childLayouts.Count > 0)
			{
				writer.AddValue("ChildLayout", _childLayouts);
			}
			if (_layoutXml != "")
			{
				writer.AddValue("LayoutString", new RawXML(_layoutXml));
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

		internal void EnsureIncrementalLayout()
		{
			if (ParentLayout != null && (_layoutXml == "" || !LayoutMerger.IsIncrementalLayout(_layoutXml)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), this);
			}
		}

		internal RibbonLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (RibbonLayout item in _childLayouts)
			{
				RibbonLayout currentLayout = item.FindCurrentLayout();
				if (currentLayout != null)
				{
					return currentLayout;
				}
			}
			return null;
		}

		internal bool HasStandAloneBase()
		{
			if (LayoutMerger.IsIncrementalLayout(_layoutXml))
			{
				if (ParentLayout == null)
				{
					return false;
				}
				return ParentLayout.HasStandAloneBase();
			}
			return true;
		}

		public RibbonLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			RibbonLayout ribbonLayout = GetMergedLayout();
			ribbonLayout._isCurrentLayout = false;
			ribbonLayout._key = _key;
			ribbonLayout._tag = _tag;
			ribbonLayout._description = _description;
			return ribbonLayout;
		}
	}
}
