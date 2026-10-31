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

namespace Mordex.Windows.UI.Tabs
{
	[DesignerSerializer("Mordex.Windows.UI.Design.TabLayoutSerializer, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[TypeConverter(typeof(TabLayoutConverter))]
	public class TabLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxTabControl _tabControl;

		private string _key = "";

		private object _tag;

		private CustomPropertyCollection _customProperties;

		private string _layoutString = "";

		internal bool _isCurrentLayout;

		private TabLayoutCollection _ownerCollection;

		private bool _isDirty;

		private string _description = "";

		private LayoutReferenceCollection _layoutReferences;

		private TabLayoutCollection _childLayouts;

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
					if (_ownerCollection != null && string.Compare(_key, value, ignoreCase: true, CultureInfo.InvariantCulture) != 0 && _ownerCollection.Contains(value))
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

		[Browsable(false)]
		[DefaultValue("")]
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
				if (GetTabControl() != null)
				{
					if (GetTabControl().CurrentLayout == this)
					{
						return true;
					}
					return false;
				}
				return _isCurrentLayout;
			}
			set
			{
				if (_ownerCollection != null)
				{
					if (value)
					{
						_ownerCollection.GetTabControl().CurrentLayout = this;
					}
					else
					{
						_ownerCollection.GetTabControl().CurrentLayout = null;
					}
				}
				else
				{
					_isCurrentLayout = value;
				}
			}
		}

		[Browsable(false)]
		public TabLayout ParentLayout
		{
			get
			{
				if (_ownerCollection != null)
				{
					return _ownerCollection.GetParentLayout();
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
					return GetOwnerCollection().TabControl;
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public LayoutReferenceCollection LayoutReferences => _layoutReferences;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TabLayoutCollection ChildLayouts => _childLayouts;

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

		public static TabLayout FromStream(Stream stream)
		{
			TabLayout uiTabLayout = new TabLayout();
			uiTabLayout.LoadFromStream(stream);
			return uiTabLayout;
		}

		public static TabLayout FromXMLString(string xmlString)
		{
			TabLayout uiTabLayout = new TabLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["UITabLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(uiTabLayout);
			return uiTabLayout;
		}

		public static TabLayout FromXMLReader(XmlReader xmlReader)
		{
			TabLayout uiTabLayout = new TabLayout();
			XmlDocument xmlDocument = LayoutImportLimits.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["UITabLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(uiTabLayout);
			return uiTabLayout;
		}

		public TabLayout()
		{
			_childLayouts = new TabLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
			_customProperties = new CustomPropertyCollection();
		}

		internal void SetTabControl(MxTabControl tabControl)
		{
			if (_tabControl != tabControl)
			{
				if (tabControl == null)
				{
					_isCurrentLayout = false;
				}
				_tabControl = tabControl;
				if (_tabControl != null)
				{
					_isCurrentLayout = true;
				}
			}
		}

		internal TabLayout GetMergedLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			TabLayout parentLayout = ParentLayout.GetMergedLayout();
			TabLayout uiTabLayout = new TabLayout();
			mordexLayoutMerger.MergeLayouts(uiTabLayout, parentLayout, this);
			return uiTabLayout;
		}

		internal MxTabControl GetTabControl()
		{
			if (_ownerCollection != null)
			{
				return _ownerCollection.GetTabControl();
			}
			return null;
		}

		internal TabLayoutCollection GetOwnerCollection()
		{
			return _ownerCollection;
		}

		internal void SetOwnerCollection(TabLayoutCollection collection)
		{
			_ownerCollection = collection;
		}

		public void Update()
		{
			if (_tabControl != null)
			{
				if (ParentLayout == null)
				{
					TabLayout layout = _tabControl.GetLayout();
					_layoutString = layout._layoutString;
					_layoutReferences = layout._layoutReferences;
					_isDirty = false;
				}
				else
				{
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					TabLayout currentLayout = _tabControl.GetLayout();
					mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), currentLayout);
					_isDirty = false;
				}
			}
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("UITabLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.Tabs.MxTabControl";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("UITabLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.Tabs.MxTabControl";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("UITabLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.Tabs.MxTabControl";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void LoadFromStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["UITabLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("UITabLayout", this);
		}

		public object Clone()
		{
			TabLayout uiTabLayout = new TabLayout();
			uiTabLayout._key = _key;
			uiTabLayout._layoutString = _layoutString;
			foreach (LayoutReference item in _layoutReferences)
			{
				uiTabLayout.LayoutReferences.Add(item);
			}
			uiTabLayout._tag = _tag;
			uiTabLayout._description = _description;
			return uiTabLayout;
		}

		public TabLayout CloneLayout(bool includeChildren)
		{
			TabLayout uiTabLayout = (TabLayout)Clone();
			if (includeChildren)
			{
				foreach (TabLayout item in _childLayouts)
				{
					uiTabLayout._childLayouts.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return uiTabLayout;
		}

		public bool IsDirty()
		{
			if (_tabControl != null)
			{
				if (_isDirty)
				{
					return true;
				}
				TabLayout layout = _tabControl.GetLayout();
				return !layout.HasSameContent(GetMergedLayout());
			}
			return false;
		}

		private bool HasSameContent(TabLayout other)
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
			// Serialize writes "ChildLayout". Some saved layouts use "ChildLayouts", so read both.
			if (!reader.DeserializeObject("ChildLayout", _childLayouts))
			{
				reader.DeserializeObject("ChildLayouts", _childLayouts);
			}
			PropertyValue propertyValue = reader["CustomProperties"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_customProperties);
			}
			else
			{
				_customProperties.Clear();
			}
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
					_tag = LayoutTypePolicy.ConvertOrKeepText(text, tagTypeName);
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

		internal void MakeIncremental()
		{
			if (ParentLayout != null && (_layoutString == "" || !LayoutMerger.IsIncrementalLayout(_layoutString)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.GetMergedLayout(), this);
			}
		}

		internal TabLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (TabLayout item in _childLayouts)
			{
				TabLayout currentChild = item.FindCurrentLayout();
				if (currentChild != null)
				{
					return currentChild;
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

		public TabLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			TabLayout uiTabLayout = GetMergedLayout();
			uiTabLayout._isCurrentLayout = false;
			uiTabLayout._key = _key;
			uiTabLayout._tag = _tag;
			uiTabLayout._description = _description;
			return uiTabLayout;
		}
	}
}
