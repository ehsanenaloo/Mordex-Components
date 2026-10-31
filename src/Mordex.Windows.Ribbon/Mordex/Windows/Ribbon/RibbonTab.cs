using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	[DesignTimeVisible(false)]
	[Designer("Mordex.Windows.Ribbon.Design.RibbonTabDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(false)]
	public class RibbonTab : Component, ICommandContainer, IInvalidatable, IMordexXmlLayoutsSupport
	{
		private IContainer _components;

		private bool _isDisposed;

		private string _accessibleName;

		private string _accessibleDescription;

		private AccessibleRole _accessibleRole;

		private string _text;

		private string _keyTip;

		private string _key;

		private Image _image;

		private Icon _icon;

		private int _imageIndex;

		private RibbonGroupCollection _groups;

		private bool _visible;

		private bool _enabled;

		private object _tag;

		private ITabOwner _tabOwner;

		private int _index;

		private int _indexInOwner;

		private bool _allowMerge;

		private int _mergeOrder = -1;

		private CommandMergeType _mergeType;

		private bool _isMerged;

		private bool _isHiddenByMerge;

		internal RibbonTab _unusedTabReference;

		private string _imageKey;

		private RibbonForegroundStyle _foregroundStyle;

		private string _name;

		internal int _scrollOffset;

		private bool _isMarkedSelected;

		internal MxRibbon _ribbon;

		internal MxRibbon _standaloneRibbon;

		internal MxRibbon _mergeSourceRibbon;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		MxRibbon ICommandContainer.Ribbon => Ribbon;

		MxRibbonContextMenu ICommandContainer.RibbonContextMenu => null;

		CommandType ICommandContainer.CommandType => CommandType.RibbonTab;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Small;

		bool ICommandContainer.IsEnabled => IsEffectivelyEnabled();

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Horizontal;

		CommandCollection ICommandContainer.Commands => null;

		AccessibleObject ICommandContainer.AccessibilityObject => null;

		[Browsable(false)]
		public Rectangle Bounds
		{
			get
			{
				if (Ribbon != null)
				{
					return Ribbon.GetTabBounds(this);
				}
				return Rectangle.Empty;
			}
		}

		[DefaultValue("")]
		[Description("Represents the letter to press after the Alt key to activate the command.")]
		[Localizable(true)]
		public string KeyTip
		{
			get
			{
				return _keyTip;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				_keyTip = value;
			}
		}

		[Localizable(true)]
		[DefaultValue("")]
		[Description("Represents the text displayed on the tab.")]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_text != value)
				{
					_text = value;
					RequestRelayout(false);
				}
			}
		}

		[Localizable(true)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Reppresents the image displayed on the tab.")]
		public Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				if (_image != value)
				{
					_image = value;
					RequestRelayout(false);
				}
			}
		}

		[DefaultValue(null)]
		[Localizable(true)]
		[Description("Reppresents the icon displayed on the tab.")]
		public Icon Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				if (_icon != value)
				{
					_icon = value;
					RequestRelayout(false);
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed on the tab.")]
		[Localizable(true)]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageIndexConverter))]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					RequestRelayout(false);
				}
			}
		}

		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageKeyConverter))]
		[Localizable(true)]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("Represents the key accessor for the image in the ImageList displayed on the tab.")]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (_imageKey != value)
				{
					_imageKey = value;
					RequestRelayout(false);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RibbonGroupCollection Groups => _groups;

		[Description("Represents the object that holds the the color and FontStyle used to draw the tab\u00b4s text.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RibbonForegroundStyle ForegroundStyle => _foregroundStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public bool Selected
		{
			get
			{
				if (!IsMarkedSelected() && IsMerged() && Ribbon != null && Ribbon._mergeHost != null && Equals(Ribbon._mergeHost.SelectedTab))
				{
					return true;
				}
				return IsMarkedSelected();
			}
			set
			{
				if (Ribbon != null)
				{
					Ribbon.SelectedTab = this;
				}
				if (GetRootRibbon() != null)
				{
					GetRootRibbon().SelectedTab = this;
				}
			}
		}

		[Browsable(false)]
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

		[Description("Indicates whether the tab is visible or hidded.")]
		[DefaultValue(true)]
		public bool Visible
		{
			get
			{
				if (_isHiddenByMerge)
				{
					return false;
				}
				return _visible;
			}
			set
			{
				if (_visible != value)
				{
					_visible = value;
					RequestRelayout(true);
				}
			}
		}

		[Description("Indicates whether the tab is enabled.")]
		[DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					InvalidateRibbon();
				}
			}
		}

		[Description("Represents the key used to identify an object in a collection")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (value != null && value.Length <= 0)
				{
					throw new ArgumentException("The Key cannot be blank.");
				}
				_key = value;
			}
		}

		[DefaultValue(-1)]
		[Localizable(true)]
		[Description("Determines the relative position of the Tab when the Ribbon is merged.")]
		public int MergeOrder
		{
			get
			{
				return _mergeOrder;
			}
			set
			{
				_mergeOrder = value;
			}
		}

		[DefaultValue(CommandMergeType.MergeItems)]
		[Description("Determines how the tab is handled when merging the Ribbon control")]
		public CommandMergeType MergeType
		{
			get
			{
				return _mergeType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CommandMergeType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_mergeType != value)
				{
					_mergeType = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int Index
		{
			get
			{
				return _index;
			}
			internal set
			{
				_index = value;
			}
		}

		[Browsable(false)]
		public MxRibbon Ribbon
		{
			get
			{
				if (GetTabOwner() != null)
				{
					return GetTabOwner().Ribbon;
				}
				return _standaloneRibbon;
			}
		}

		[Description("The role that will be reported to accessibility clients.")]
		[DefaultValue(AccessibleRole.Default)]
		public AccessibleRole AccessibleRole
		{
			get
			{
				return _accessibleRole;
			}
			set
			{
				if (_accessibleRole != value)
				{
					_accessibleRole = value;
				}
			}
		}

		[Description("The name that will be reported to accessibility clients.")]
		[DefaultValue(null)]
		public string AccessibleName
		{
			get
			{
				return _accessibleName;
			}
			set
			{
				if (_accessibleName != value)
				{
					_accessibleName = value;
				}
			}
		}

		[Description("The description that will be reported to accessibility clients.")]
		[DefaultValue(null)]
		public string AccessibleDescription
		{
			get
			{
				return _accessibleDescription;
			}
			set
			{
				if (_accessibleDescription != value)
				{
					_accessibleDescription = value;
				}
			}
		}

		[Description("Specifies whether the RibbonTab should be merged when the its parent Ribbon gets merged.")]
		[DefaultValue(true)]
		public bool AllowMerge
		{
			get
			{
				return _allowMerge;
			}
			set
			{
				_allowMerge = value;
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
				}
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				ApplySiteKeyAndName();
			}
		}

		protected override void Dispose(bool disposing)
		{
			_isDisposed = true;
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			if (disposing)
			{
				_standaloneRibbon = null;
				if (GetTabOwner() != null)
				{
					GetTabOwner().TabDisposed(this);
				}
				foreach (RibbonGroup group in Groups)
				{
					group.SetParentContainer(null);
					group.Dispose();
				}
				SetRibbon(null);
			}
			base.Dispose(disposing);
		}

		internal bool HasBeenDisposed()
		{
			return _isDisposed;
		}

		private void InitializeComponents()
		{
			_components = new Container();
		}

		public RibbonTab()
		{
			InitializeComponents();
			InitializeFields();
		}

		public RibbonTab(string text)
			: this()
		{
			_text = text;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			Deserialize(reader);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter writer)
		{
			Serialize(writer);
		}

		void ICommandContainer.InsertCommand(int index, CommandBase command)
		{
			if (command is RibbonGroup)
			{
				Groups.Insert(index, command as RibbonGroup);
			}
		}

		void ICommandContainer.DetachCommand(CommandBase command)
		{
			Groups.DetachGroup(command);
		}

		void ICommandContainer.CommandAdded(CommandBase command)
		{
			if (Ribbon != null)
			{
				((ICommandContainer)Ribbon).CommandAdded(command);
			}
		}

		void ICommandContainer.CommandRemoved(CommandBase command)
		{
			if (Ribbon != null)
			{
				((ICommandContainer)Ribbon).CommandRemoved(command);
			}
		}

		void ICommandContainer.CommandClicked(CommandEventArgs eventArgs)
		{
			if (Ribbon != null)
			{
				((ICommandContainer)Ribbon).CommandClicked(eventArgs);
			}
		}

		bool ICommandContainer.CommandDropDownClosing(DropDownClosingEventArgs eventArgs)
		{
			if (Ribbon != null)
			{
				return ((ICommandContainer)Ribbon).CommandDropDownClosing(eventArgs);
			}
			return false;
		}

		void ICommandContainer.CommandDropDownClosed(CommandEventArgs eventArgs)
		{
			if (Ribbon != null)
			{
				((ICommandContainer)Ribbon).CommandDropDownClosed(eventArgs);
			}
		}

		void ICommandContainer.CommandDropDownOpened(CommandEventArgs eventArgs)
		{
			if (Ribbon != null)
			{
				((ICommandContainer)Ribbon).CommandDropDownOpened(eventArgs);
			}
		}

		bool ICommandContainer.CommandDropDownOpening(CommandCancelEventArgs eventArgs)
		{
			if (Ribbon != null)
			{
				return ((ICommandContainer)Ribbon).CommandDropDownOpening(eventArgs);
			}
			return false;
		}

		void ICommandContainer.CommandDisposed(CommandEventArgs eventArgs)
		{
			if (Groups.Contains(eventArgs.Command as RibbonGroup))
			{
				Groups.Remove(eventArgs.Command as RibbonGroup);
			}
		}

		bool ICommandContainer.CanBeParentTo(CommandType commandType)
		{
			if (commandType == CommandType.RibbonGroup)
			{
				return true;
			}
			return false;
		}

		void ICommandContainer.CommandCheckedChanged(CommandEventArgs eventArgs)
		{
		}

		internal MxRibbon GetRootRibbon()
		{
			if (_ribbon != null)
			{
				MxRibbon field_ = _ribbon;
				while (field_ != null && field_._mergeHost != null)
				{
					field_ = field_._mergeHost;
				}
				return field_;
			}
			if (_tabOwner is RibbonContextualTab)
			{
				return ((RibbonContextualTab)_tabOwner).GetRootRibbon();
			}
			return null;
		}

		internal void SetRibbon(MxRibbon ribbon)
		{
			_ribbon = ribbon;
		}

		internal bool IsMerged()
		{
			if (!_isMerged && _tabOwner is RibbonContextualTab)
			{
				return ((RibbonContextualTab)_tabOwner).IsMerged();
			}
			return _isMerged;
		}

		internal void SetMerged(bool isMerged)
		{
			_isMerged = isMerged;
		}

		internal void UpdateMergedState(MxRibbon mergeRibbon, bool isMerged)
		{
			if (_isMerged == isMerged)
			{
				return;
			}
			if (!isMerged)
			{
				if (_mergeSourceRibbon != mergeRibbon)
				{
					return;
				}
			}
			else
			{
				_mergeSourceRibbon = mergeRibbon;
			}
			_isMerged = isMerged;
		}

		internal int GetIndexInOwner()
		{
			return _indexInOwner;
		}

		internal void SetIndexInOwner(int index)
		{
			_indexInOwner = index;
		}

		internal ITabOwner GetTabOwner()
		{
			return _tabOwner;
		}

		internal void SetTabOwner(ITabOwner tabOwner)
		{
			_tabOwner = tabOwner;
		}

		internal bool IsMarkedSelected()
		{
			return _isMarkedSelected;
		}

		internal void SetMarkedSelected(bool isSelected)
		{
			_isMarkedSelected = isSelected;
		}

		internal bool IsEffectivelyEnabled()
		{
			if (!Enabled)
			{
				return false;
			}
			if (GetTabOwner() != null)
			{
				if (_tabOwner is MxRibbon && Ribbon != null && Ribbon._mergeHost != null && !Ribbon._mergeHost.Enabled)
				{
					return Ribbon._mergeHost.Enabled;
				}
				return GetTabOwner().IsEnabled;
			}
			return true;
		}

		void IInvalidatable.Invalidate()
		{
			Invalidate();
		}

		internal virtual void Invalidate()
		{
			if (Ribbon != null)
			{
				Ribbon.Invalidate();
			}
		}

		internal virtual void Serialize(LayoutWriter writer)
		{
			if (!_allowMerge)
			{
				writer.AddValue("AllowMerge", _allowMerge);
			}
			if (!_enabled)
			{
				writer.AddValue("Enabled", _enabled);
			}
			if (_foregroundStyle.HasCustomStyle())
			{
				writer.AddValue("ForegroundStyle", _foregroundStyle);
			}
			if (_groups.Count > 0)
			{
				writer.AddValue("Groups", _groups);
			}
			if (_icon != null)
			{
				writer.AddValue("Icon", _icon);
			}
			if (_image != null)
			{
				writer.AddValue("Image", _image);
			}
			if (_imageIndex != -1)
			{
				writer.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey != null)
			{
				writer.AddValue("ImageKey", _imageKey);
			}
			if (_key != null)
			{
				writer.AddValue("Key", _key);
			}
			if (_keyTip.Length > 0)
			{
				writer.AddValue("KeyTip", _keyTip);
			}
			if (_mergeType != CommandMergeType.MergeItems)
			{
				writer.AddValue("MergeType", _mergeType);
			}
			if (_mergeOrder != -1)
			{
				writer.AddValue("MergeOrder", _mergeOrder);
			}
			if (_name != null && _name.Length > 0)
			{
				writer.AddValue("Name", Name);
			}
			if (_tag != null)
			{
				if (_tag.GetType() != typeof(string))
				{
					writer.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				writer.AddValue("Tag", _tag);
			}
			if (_text.Length > 0)
			{
				writer.AddValue("Text", _text);
			}
			if (!_visible)
			{
				writer.AddValue("Visible", _visible);
			}
		}

		internal virtual void Deserialize(LayoutReader reader)
		{
			_name = (string)LayoutReader.GetPropertyValue(reader, "Name", typeof(string), "");
			_allowMerge = (bool)LayoutReader.GetPropertyValue(reader, "AllowMerge", typeof(bool), true);
			_enabled = (bool)LayoutReader.GetPropertyValue(reader, "Enabled", typeof(bool), true);
			PropertyValue propertyValue = reader["ForegroundStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_foregroundStyle);
			}
			else
			{
				_foregroundStyle.ResetAll();
			}
			_icon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_image = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), null);
			_key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), null);
			_keyTip = (string)LayoutReader.GetPropertyValue(reader, "KeyTip", typeof(string), "");
			_mergeOrder = (int)LayoutReader.GetPropertyValue(reader, "MergeOrder", typeof(int), -1);
			_mergeType = (CommandMergeType)LayoutReader.GetPropertyValue(reader, "MergeType", typeof(CommandMergeType), CommandMergeType.MergeItems);
			_visible = (bool)LayoutReader.GetPropertyValue(reader, "Visible", typeof(bool), true);
			_text = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
			_tag = LayoutTypeRegistry.ReadTag(reader);
			DeserializeGroups(reader);
		}

		internal virtual void InitializeFields()
		{
			_groups = new RibbonGroupCollection(this);
			_key = "";
			_keyTip = "";
			_text = "";
			_visible = true;
			_enabled = true;
			_imageIndex = -1;
			_allowMerge = true;
			_mergeOrder = -1;
			_index = -1;
			_indexInOwner = -1;
			_imageKey = "";
			_mergeType = CommandMergeType.MergeItems;
			_accessibleRole = AccessibleRole.Default;
			_foregroundStyle = new RibbonForegroundStyle(this);
			_scrollOffset = 0;
		}

		internal void SyncNameFromSite()
		{
			if (Site != null && Site.Name != null)
			{
				Name = Site.Name;
			}
		}

		private void DeserializeGroups(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["Groups"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_groups);
				return;
			}
			_groups.DisposeAll();
		}

		internal bool TryProcessShortcut(Shortcut shortcut, Control inputControl, Message msg)
		{
			if (!Visible || !Enabled)
			{
				return false;
			}
			foreach (RibbonGroup item in _groups)
			{
				if (item.SearchShortcut(shortcut, inputControl, msg))
				{
					return true;
				}
			}
			return false;
		}

		internal void RequestRelayout(bool updateTabStrip)
		{
			if (Ribbon != null)
			{
				Ribbon.RequestTabStripLayout(updateTabStrip);
			}
			if (GetRootRibbon() != null)
			{
				GetRootRibbon().RequestTabStripLayout(updateTabStrip);
			}
		}

		internal void InvalidateRibbon()
		{
			if (Ribbon != null)
			{
				Ribbon.InvalidateRibbonAndCaption();
			}
			if (GetRootRibbon() != null)
			{
				GetRootRibbon().InvalidateRibbonAndCaption();
			}
		}

		public RibbonTab Clone()
		{
			RibbonTab ribbonTab = new RibbonTab();
			ribbonTab._enabled = _enabled;
			ribbonTab._groups = _groups.CloneForTab(ribbonTab);
			ribbonTab._icon = _icon;
			ribbonTab._image = _image;
			ribbonTab._imageIndex = _imageIndex;
			ribbonTab._key = _key;
			ribbonTab._keyTip = _keyTip;
			ribbonTab._tag = _tag;
			ribbonTab._text = _text;
			ribbonTab._visible = _visible;
			ribbonTab._foregroundStyle = _foregroundStyle.CloneForOwner(ribbonTab);
			return ribbonTab;
		}

		internal bool IsNotInherited()
		{
			if (base.Container == null)
			{
				return true;
			}
			IInheritanceService inheritanceService = (IInheritanceService)GetService(typeof(IInheritanceService));
			if (inheritanceService != null)
			{
				if (inheritanceService.GetInheritanceAttribute(this) == InheritanceAttribute.NotInherited)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		internal bool IsEditable()
		{
			if (base.Container == null)
			{
				return true;
			}
			IInheritanceService inheritanceService = (IInheritanceService)GetService(typeof(IInheritanceService));
			if (inheritanceService != null)
			{
				if (inheritanceService.GetInheritanceAttribute(this) == InheritanceAttribute.InheritedReadOnly)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		private void ApplySiteKeyAndName()
		{
			if (Site != null)
			{
				if (_key == null)
				{
					_key = Site.Name;
				}
				if (base.Site.Name != null)
				{
					Name = base.Site.Name;
				}
			}
		}

		internal void SetHiddenByMerge(bool isHidden)
		{
			_isHiddenByMerge = isHidden;
		}
	}
}
