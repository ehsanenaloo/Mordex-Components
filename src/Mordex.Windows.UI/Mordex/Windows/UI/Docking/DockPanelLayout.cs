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

namespace Mordex.Windows.UI.Docking
{
	[TypeConverter(typeof(DockPanelLayoutConverter))]
	[DesignerSerializer("Mordex.Windows.UI.Design.DockPanelLayoutSerializer, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class DockPanelLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxDockManager _panelManager;

		private string _key = "";

		private object _tag;

		private CustomPropertyCollection _customProperties;

		private string _layoutString = "";

		internal bool _isCurrentLayout;

		private DockPanelLayoutCollection _owningCollection;

		private string _description = "";

		private LayoutReferenceCollection _layoutReferences;

		private DockPanelLayoutCollection _childLayouts;

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

		[Browsable(false)]
		[Localizable(true)]
		[DefaultValue("")]
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
				if (GetPanelManager() != null)
				{
					if (GetPanelManager().CurrentLayout == this)
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
						_owningCollection.PanelManager.CurrentLayout = this;
					}
					else
					{
						_owningCollection.PanelManager.CurrentLayout = null;
					}
				}
				else
				{
					_isCurrentLayout = value;
				}
			}
		}

		[Browsable(false)]
		public DockPanelLayout ParentLayout
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
					return GetOwningCollection().PanelManager;
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public LayoutReferenceCollection LayoutReferences => _layoutReferences;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public DockPanelLayoutCollection ChildLayouts => _childLayouts;

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

		public static DockPanelLayout FromStream(Stream stream)
		{
			DockPanelLayout uiPanelLayout = new DockPanelLayout();
			uiPanelLayout.LoadFromStream(stream);
			return uiPanelLayout;
		}

		public static DockPanelLayout FromXMLString(string xmlString)
		{
			DockPanelLayout uiPanelLayout = new DockPanelLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["UIPanelLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(uiPanelLayout);
			return uiPanelLayout;
		}

		public static DockPanelLayout FromXMLReader(XmlReader xmlReader)
		{
			DockPanelLayout uiPanelLayout = new DockPanelLayout();
			XmlDocument xmlDocument = LayoutImportLimits.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["UIPanelLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(uiPanelLayout);
			return uiPanelLayout;
		}

		public DockPanelLayout()
		{
			_childLayouts = new DockPanelLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
			_customProperties = new CustomPropertyCollection();
		}

		internal MxDockManager GetPanelManager()
		{
			if (_owningCollection != null)
			{
				return _owningCollection.PanelManager;
			}
			return _panelManager;
		}

		internal void SetPanelManager(MxDockManager panelManager)
		{
			if (_panelManager != panelManager)
			{
				if (panelManager == null)
				{
					_isCurrentLayout = false;
				}
				_panelManager = panelManager;
				if (_panelManager != null)
				{
					_isCurrentLayout = true;
				}
			}
		}

		internal DockPanelLayout GetMergedLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			DockPanelLayout parentLayout = ParentLayout.GetMergedLayout();
			DockPanelLayout uiPanelLayout = new DockPanelLayout();
			mordexLayoutMerger.MergeLayouts(uiPanelLayout, parentLayout, this);
			return uiPanelLayout;
		}

		internal DockPanelLayoutCollection GetOwningCollection()
		{
			return _owningCollection;
		}

		internal void SetOwningCollection(DockPanelLayoutCollection uiPanelLayoutCollection)
		{
			_owningCollection = uiPanelLayoutCollection;
		}

		public void Update()
		{
			if (IsCurrentLayout)
			{
				if (ParentLayout == null)
				{
					DockPanelLayout layout = GetPanelManager().GetLayout();
					_layoutString = layout._layoutString;
					_layoutReferences = layout._layoutReferences;
				}
				else
				{
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					DockPanelLayout currentLayout = GetPanelManager().GetLayout();
					mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), currentLayout);
				}
			}
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("UIPanelLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.Docking.MxDockManager";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("UIPanelLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.Docking.MxDockManager";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("UIPanelLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.Docking.MxDockManager";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void LoadFromStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["UIPanelLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("UIPanelLayout", this);
		}

		public object Clone()
		{
			DockPanelLayout uiPanelLayout = new DockPanelLayout();
			uiPanelLayout._key = _key;
			uiPanelLayout._layoutString = _layoutString;
			foreach (LayoutReference item in _layoutReferences)
			{
				uiPanelLayout.LayoutReferences.Add(item);
			}
			uiPanelLayout._tag = _tag;
			uiPanelLayout._description = _description;
			return uiPanelLayout;
		}

		public DockPanelLayout CloneLayout(bool includeChildren)
		{
			DockPanelLayout uiPanelLayout = (DockPanelLayout)Clone();
			if (includeChildren)
			{
				foreach (DockPanelLayout item in _childLayouts)
				{
					uiPanelLayout._childLayouts.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return uiPanelLayout;
		}

		public bool IsDirty()
		{
			if (IsCurrentLayout)
			{
				DockPanelLayout layout = GetPanelManager().GetLayout();
				return !layout.HasSameContent(GetMergedLayout());
			}
			return false;
		}

		private bool HasSameContent(DockPanelLayout other)
		{
			if (other._layoutString != _layoutString)
			{
				return false;
			}
			if (other.LayoutReferences.Count > 0 || LayoutReferences.Count > 0)
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
			if (_childLayouts.Count > 0)
			{
				writer.AddValue("ChildLayout", _childLayouts);
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

		internal void EnsureIncrementalLayoutString()
		{
			if (ParentLayout != null && (_layoutString == "" || !LayoutMerger.IsIncrementalLayout(_layoutString)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), this);
			}
		}

		internal DockPanelLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (DockPanelLayout item in _childLayouts)
			{
				DockPanelLayout currentLayout = item.FindCurrentLayout();
				if (currentLayout != null)
				{
					return currentLayout;
				}
			}
			return null;
		}

		internal bool HasStandAloneRoot()
		{
			if (LayoutMerger.IsIncrementalLayout(_layoutString))
			{
				if (ParentLayout == null)
				{
					return false;
				}
				return ParentLayout.HasStandAloneRoot();
			}
			return true;
		}

		public DockPanelLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			DockPanelLayout uiPanelLayout = GetMergedLayout();
			uiPanelLayout._isCurrentLayout = false;
			uiPanelLayout._key = _key;
			uiPanelLayout._tag = _tag;
			uiPanelLayout._description = _description;
			return uiPanelLayout;
		}
	}
}
