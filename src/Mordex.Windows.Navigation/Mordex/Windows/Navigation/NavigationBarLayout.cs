using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	[DesignerSerializer("Mordex.Windows.Navigation.Design.NavigationBarLayoutSerializer, Mordex.Windows.Navigation.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[TypeConverter(typeof(NavigationBarLayoutConverter))]
	public class NavigationBarLayout : ICloneable, IMordexLayout, IMordexXmlLayoutsSupport
	{
		private MxNavigationBar _navigationBar;

		private string _key = "";

		private object _tag;

		private string _layoutString = "";

		internal bool _isCurrentLayout;

		private NavigationBarLayoutCollection _parentCollection;

		private bool _isModified;

		private string _description = "";

		private NavigationBarLayoutCollection _childLayouts;

		private LayoutReferenceCollection _layoutReferences;

		[Browsable(false)]
		public NavigationBarLayout ParentLayout
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
				if (ResolveNavigationBar() != null)
				{
					if (ResolveNavigationBar().CurrentLayout == this)
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
						_parentCollection.ResolveNavigationBar().CurrentLayout = this;
					}
					else
					{
						_parentCollection.ResolveNavigationBar().CurrentLayout = null;
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
				if (GetParentCollection() == null)
				{
					return "DesignTimeLayout";
				}
				if (ParentLayout == null)
				{
					return "Layout_" + GetParentCollection().IndexOf(this);
				}
				return GetParentCollection().IndexOf(this).ToString();
			}
		}

		object IMordexLayout.Owner
		{
			get
			{
				if (ParentLayout == null)
				{
					if (GetParentCollection() == null)
					{
						return null;
					}
					return GetParentCollection().GetOwnerNavigationBar();
				}
				return ParentLayout;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarLayoutCollection ChildLayouts => _childLayouts;

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

		public static NavigationBarLayout FromStream(Stream stream)
		{
			NavigationBarLayout navigationBarLayout = new NavigationBarLayout();
			navigationBarLayout.LoadFromStream(stream);
			return navigationBarLayout;
		}

		public static NavigationBarLayout FromXMLString(string xmlString)
		{
			NavigationBarLayout navigationBarLayout = new NavigationBarLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			PropertyValue propertyValue = mordexLayoutReader["NavigationBarLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml string");
			}
			propertyValue.DeserializeObject(navigationBarLayout);
			return navigationBarLayout;
		}

		public static NavigationBarLayout FromXMLReader(XmlReader xmlReader)
		{
			NavigationBarLayout navigationBarLayout = new NavigationBarLayout();
			XmlDocument xmlDocument = LayoutImportLimits.LoadDocument(xmlReader);
			LayoutReader mordexLayoutReader = LayoutReader.FromParentNode(xmlDocument);
			PropertyValue propertyValue = mordexLayoutReader["NavigationBarLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Invalid xml");
			}
			propertyValue.DeserializeObject(navigationBarLayout);
			return navigationBarLayout;
		}

		public NavigationBarLayout()
		{
			_childLayouts = new NavigationBarLayoutCollection(this);
			_layoutReferences = new LayoutReferenceCollection();
		}

		internal MxNavigationBar ResolveNavigationBar()
		{
			if (_parentCollection != null)
			{
				return _parentCollection.ResolveNavigationBar();
			}
			return null;
		}

		internal void SetAttachedNavigationBar(MxNavigationBar navigationBar)
		{
			if (_navigationBar != navigationBar)
			{
				if (navigationBar == null)
				{
					_isCurrentLayout = false;
				}
				_navigationBar = navigationBar;
				if (_navigationBar != null)
				{
					_isCurrentLayout = true;
				}
			}
		}

		internal NavigationBarLayout ResolveMergedLayout()
		{
			if (ParentLayout == null)
			{
				return this;
			}
			LayoutMerger mordexLayoutMerger = new LayoutMerger();
			NavigationBarLayout parentLayout = ParentLayout.ResolveMergedLayout();
			NavigationBarLayout navigationBarLayout = new NavigationBarLayout();
			mordexLayoutMerger.MergeLayouts(navigationBarLayout, parentLayout, this);
			return navigationBarLayout;
		}

		internal NavigationBarLayoutCollection GetParentCollection()
		{
			return _parentCollection;
		}

		internal void SetParentCollection(NavigationBarLayoutCollection navigationBarLayoutCollection)
		{
			_parentCollection = navigationBarLayoutCollection;
		}

		public void Update()
		{
			if (_navigationBar != null)
			{
				if (ParentLayout == null)
				{
					NavigationBarLayout layout = _navigationBar.GetLayout();
					_layoutString = layout._layoutString;
					_layoutReferences = layout.LayoutReferences;
					_isModified = false;
				}
				else
				{
					NavigationBarLayout parentLayout = ParentLayout.ResolveMergedLayout();
					LayoutMerger mordexLayoutMerger = new LayoutMerger();
					NavigationBarLayout currentLayout = _navigationBar.GetLayout();
					mordexLayoutMerger.CreateIncrementalLayout(this, parentLayout, currentLayout);
					_isModified = false;
				}
			}
		}

		public void Save(Stream stream)
		{
			PropertyValue propertyValue = new PropertyValue("NavigationBarLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Navigation.MxNavigationBar";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void Save(XmlWriter xmlWriter)
		{
			PropertyValue propertyValue = new PropertyValue("NavigationBarLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Navigation.MxNavigationBar";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			xmlWriter.WriteRaw(mordexLayoutWriter.ToString());
		}

		public string GetXmlString()
		{
			PropertyValue propertyValue = new PropertyValue("NavigationBarLayout", this);
			propertyValue.LayoutType = "Mordex.Windows.Navigation.MxNavigationBar";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		private void LoadFromStream(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["NavigationBarLayout"];
			if (propertyValue == null)
			{
				throw new ArgumentException("Stream format is not recognizable");
			}
			mordexLayoutReader.DeserializeObject("NavigationBarLayout", this);
		}

		public object Clone()
		{
			NavigationBarLayout navigationBarLayout = new NavigationBarLayout();
			navigationBarLayout._key = _key;
			navigationBarLayout._layoutString = _layoutString;
			navigationBarLayout._description = _description;
			navigationBarLayout._tag = _tag;
			foreach (LayoutReference item in _layoutReferences)
			{
				navigationBarLayout.LayoutReferences.Add(new LayoutReference(item.ReferenceId, item.Instance));
			}
			return navigationBarLayout;
		}

		public NavigationBarLayout CloneLayout(bool includeChildren)
		{
			NavigationBarLayout navigationBarLayout = (NavigationBarLayout)Clone();
			if (includeChildren)
			{
				foreach (NavigationBarLayout item in _childLayouts)
				{
					navigationBarLayout._childLayouts.Add(item.CloneLayout(includeChildren: true));
				}
			}
			return navigationBarLayout;
		}

		public bool IsDirty()
		{
			if (_navigationBar != null)
			{
				if (_isModified)
				{
					return true;
				}
				_navigationBar.GetLayout();
				return !HasSameContent(ResolveMergedLayout());
			}
			return false;
		}

		private bool HasSameContent(NavigationBarLayout navigationBarLayout)
		{
			if (navigationBarLayout._layoutString != _layoutString)
			{
				return false;
			}
			if (navigationBarLayout._layoutReferences.Count > 0 || _layoutReferences.Count > 0)
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
			_description = (string)LayoutReader.GetPropertyValue(reader, "Description", typeof(string), "");
			_isCurrentLayout = (bool)LayoutReader.GetPropertyValue(reader, "IsCurrentLayout", typeof(bool), false);
			reader.DeserializeObject("ChildLayouts", _childLayouts);
			string tagText = (string)LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null);
			_tag = LayoutTypePolicy.ConvertOrKeepText(tagText, (string)LayoutReader.GetPropertyValue(reader, "TagTypeName", typeof(string), null));
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

		internal void UpdateIncrementalLayout()
		{
			if (ParentLayout != null && (_layoutString == "" || !LayoutMerger.IsIncrementalLayout(_layoutString)))
			{
				LayoutMerger mordexLayoutMerger = new LayoutMerger();
				mordexLayoutMerger.CreateIncrementalLayout(this, ParentLayout.ResolveMergedLayout(), this);
			}
		}

		internal NavigationBarLayout FindCurrentLayout()
		{
			if (_isCurrentLayout)
			{
				return this;
			}
			foreach (NavigationBarLayout item in _childLayouts)
			{
				NavigationBarLayout foundLayout = item.FindCurrentLayout();
				if (foundLayout != null)
				{
					return foundLayout;
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

		public NavigationBarLayout ConvertToStandAlone()
		{
			if (StandAlone)
			{
				return CloneLayout(includeChildren: false);
			}
			NavigationBarLayout navigationBarLayout = ResolveMergedLayout();
			navigationBarLayout._isCurrentLayout = false;
			navigationBarLayout._key = _key;
			navigationBarLayout._tag = _tag;
			navigationBarLayout._description = _description;
			return navigationBarLayout;
		}
	}
}
