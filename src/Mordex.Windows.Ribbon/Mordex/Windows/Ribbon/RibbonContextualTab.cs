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
	[ToolboxItem(false)]
	[DesignTimeVisible(false)]
	public class RibbonContextualTab : Component, ICommandContainer, ITabOwner, IMordexXmlLayoutsSupport, IInvalidatable
	{
		private string _text;

		private Image _image;

		private Icon _icon;

		private int _imageIndex;

		private bool _visible;

		private int _indexInOwner;

		private string _key;

		private object _tag;

		internal MxRibbon _ribbon;

		private bool _active;

		private int _originalIndex;

		private bool _allowMerge;

		private int _mergeOrder = -1;

		private CommandMergeType _mergeType;

		private bool _isMerged;

		private bool _isHiddenByMerge;

		internal RibbonContextualTab _unusedTabReference;

		private string _imageKey;

		private Color _backColor;

		private RibbonForegroundStyle _foregroundStyle;

		private string _name;

		private ControlCollection _controls;

		private RibbonTabCollection _tabs;

		internal MxRibbon _standaloneRibbon;

		internal MxRibbon _mergeSourceRibbon;

		private IContainer _components;

		MxRibbon ICommandContainer.Ribbon => _ribbon;

		MxRibbonContextMenu ICommandContainer.RibbonContextMenu => null;

		CommandType ICommandContainer.CommandType => CommandType.RibbonTab;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Small;

		bool ICommandContainer.IsEnabled => IsEffectivelyEnabled();

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Horizontal;

		CommandCollection ICommandContainer.Commands => null;

		AccessibleObject ICommandContainer.AccessibilityObject => null;

		MxRibbon ITabOwner.Ribbon => Ribbon;

		bool ITabOwner.IsEnabled => IsEffectivelyEnabled();

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(true)]
		[Description("Specifies whether the ContextualTab should be merged when the its parent Ribbon gets merged.")]
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

		[Description("Determines the relative position of the Tab when the Ribbon is merged.")]
		[DefaultValue(-1)]
		[Localizable(true)]
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

		[Description("The background color for the ContextualTab")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Color BackColor
		{
			get
			{
				return _backColor;
			}
			set
			{
				if (_backColor != value)
				{
					_backColor = value;
					InvalidateRibbon();
				}
			}
		}

		[Description("Determines how the ContextualTab is handled when merging the Ribbon")]
		[DefaultValue(CommandMergeType.MergeItems)]
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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RibbonTabCollection Tabs => _tabs;

		[Editor(typeof(ControlsEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlCollection Controls
		{
			get
			{
				if (_controls == null)
				{
					_controls = new ControlCollection(this);
				}
				return _controls;
			}
		}

		[Description("Represents the text displayed on the contextual tab.")]
		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Description("Reppresents the image displayed on the tab header.")]
		[DefaultValue(null)]
		[Localizable(true)]
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
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
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
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the image list index value of the image displayed on the tab.")]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
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
				}
			}
		}

		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("Represents the key accessor for the image in the ImageList displayed on the tab.")]
		[DefaultValue(null)]
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
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the the color and FontStyle used to draw the tab\u00b4s text.")]
		public RibbonForegroundStyle ForegroundStyle => _foregroundStyle;

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

		[Description("Indicates whether the contextual tab is visible or hidded.")]
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
				if (_key != value)
				{
					_key = value;
				}
			}
		}

		[Browsable(false)]
		public MxRibbon Ribbon => _ribbon;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public bool Active
		{
			get
			{
				return _active;
			}
			set
			{
				if (_active != value)
				{
					_active = value;
					if (GetRootRibbon() != null)
					{
						MxRibbon ribbon = GetRootRibbon();
						ribbon.SyncActiveContextualTab(this);
					}
					else if (_ribbon != null)
					{
						_ribbon.SyncActiveContextualTab(this);
					}
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

		public RibbonContextualTab()
		{
			InitializeComponents();
			InitializeFields();
		}

		public RibbonContextualTab(string text)
			: this()
		{
			_text = text;
		}

		void ICommandContainer.InsertCommand(int index, CommandBase command)
		{
		}

		void ICommandContainer.DetachCommand(CommandBase command)
		{
		}

		void ICommandContainer.CommandAdded(CommandBase command)
		{
			if (_ribbon != null)
			{
				((ICommandContainer)_ribbon).CommandAdded(command);
			}
		}

		void ICommandContainer.CommandRemoved(CommandBase command)
		{
			if (_ribbon != null)
			{
				((ICommandContainer)_ribbon).CommandAdded(command);
			}
		}

		void ICommandContainer.CommandClicked(CommandEventArgs eventArgs)
		{
			if (_ribbon != null)
			{
				((ICommandContainer)_ribbon).CommandClicked(eventArgs);
			}
		}

		bool ICommandContainer.CommandDropDownClosing(DropDownClosingEventArgs eventArgs)
		{
			if (_ribbon != null)
			{
				((ICommandContainer)_ribbon).CommandDropDownClosing(eventArgs);
			}
			return false;
		}

		void ICommandContainer.CommandDropDownClosed(CommandEventArgs eventArgs)
		{
			if (_ribbon != null)
			{
				((ICommandContainer)_ribbon).CommandDropDownClosed(eventArgs);
			}
		}

		void ICommandContainer.CommandDropDownOpened(CommandEventArgs eventArgs)
		{
			if (_ribbon != null)
			{
				((ICommandContainer)_ribbon).CommandDropDownOpened(eventArgs);
			}
		}

		bool ICommandContainer.CommandDropDownOpening(CommandCancelEventArgs eventArgs)
		{
			if (_ribbon != null)
			{
				return ((ICommandContainer)_ribbon).CommandDropDownOpening(eventArgs);
			}
			return false;
		}

		void ICommandContainer.CommandDisposed(CommandEventArgs eventArgs)
		{
		}

		bool ICommandContainer.CanBeParentTo(CommandType commandType)
		{
			return false;
		}

		void ICommandContainer.CommandCheckedChanged(CommandEventArgs eventArgs)
		{
		}

		void ITabOwner.TabAdded(RibbonTab tab)
		{
			if (Ribbon != null)
			{
				((ITabOwner)Ribbon).TabAdded(tab);
			}
		}

		void ITabOwner.TabRemoved(RibbonTab tab)
		{
			if (Ribbon != null)
			{
				((ITabOwner)Ribbon).TabRemoved(tab);
			}
		}

		void ITabOwner.TabDisposed(RibbonTab tab)
		{
			if (Tabs.Contains(tab))
			{
				Tabs.Remove(tab);
			}
		}

		void ITabOwner.RemoveTab(RibbonTab tab)
		{
			Tabs.DetachTab(tab);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_name = (string)LayoutReader.GetPropertyValue(reader, "Name", typeof(string), "");
			_allowMerge = (bool)LayoutReader.GetPropertyValue(reader, "AllowMerge", typeof(bool), true);
			_backColor = (Color)LayoutReader.GetPropertyValue(reader, "BackColor", typeof(Color), Color.Empty);
			_icon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_image = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), null);
			_key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), null);
			_mergeOrder = (int)LayoutReader.GetPropertyValue(reader, "MergeOrder", typeof(int), -1);
			_mergeType = (CommandMergeType)LayoutReader.GetPropertyValue(reader, "MergeType", typeof(CommandMergeType), CommandMergeType.MergeItems);
			_visible = (bool)LayoutReader.GetPropertyValue(reader, "Visible", typeof(bool), true);
			_text = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
			PropertyValue propertyValue = reader["ForegroundStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_foregroundStyle);
			}
			else
			{
				_foregroundStyle.ResetAll();
			}
			_tag = LayoutTypeRegistry.ReadTag(reader);
			DeserializeTabs(reader);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter writer)
		{
			if (!_allowMerge)
			{
				writer.AddValue("AllowMerge", _allowMerge);
			}
			if (!_backColor.IsEmpty)
			{
				writer.AddValue("BackColor", _backColor);
			}
			if (_foregroundStyle.HasCustomStyle())
			{
				writer.AddValue("ForegroundStyle", _foregroundStyle);
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
			if (_tabs.Count > 0)
			{
				writer.AddValue("TabSet", _tabs);
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

		void IInvalidatable.Invalidate()
		{
			if (_ribbon != null)
			{
				_ribbon.Invalidate();
			}
		}

		internal void InitializeFields()
		{
			_key = "";
			_text = "";
			_visible = true;
			_backColor = Color.FromArgb(201, 0, 119);
			_tabs = new RibbonTabCollection(this);
			_controls = new ControlCollection(this);
			_allowMerge = true;
			_mergeOrder = -1;
			_indexInOwner = -1;
			_originalIndex = -1;
			_imageIndex = -1;
			_mergeType = CommandMergeType.MergeItems;
			_foregroundStyle = new RibbonForegroundStyle(this);
		}

		internal MxRibbon GetRootRibbon()
		{
			if (_standaloneRibbon != null)
			{
				MxRibbon field_ = _standaloneRibbon;
				while (field_ != null && field_._mergeHost != null)
				{
					field_ = field_._mergeHost;
				}
				return field_;
			}
			return null;
		}

		internal void SetStandaloneRibbon(MxRibbon ribbonContextualTabRibbon)
		{
			_standaloneRibbon = ribbonContextualTabRibbon;
		}

		internal int GetIndexInOwner()
		{
			return _indexInOwner;
		}

		internal void SetIndexInOwner(int index)
		{
			_indexInOwner = index;
		}

		internal bool IsEffectivelyEnabled()
		{
			if (Ribbon != null)
			{
				if (Ribbon._mergeHost != null && !Ribbon._mergeHost.Enabled)
				{
					return Ribbon._mergeHost.Enabled;
				}
				return Ribbon.Enabled;
			}
			return true;
		}

		internal bool IsMerged()
		{
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

		internal void SetHiddenByMerge(bool ribbonContextualTabBoolean)
		{
			_isHiddenByMerge = ribbonContextualTabBoolean;
		}

		internal int GetOriginalIndex()
		{
			return _originalIndex;
		}

		internal void SetOriginalIndex(int ribbonContextualTabInteger)
		{
			_originalIndex = ribbonContextualTabInteger;
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

		internal bool TryProcessShortcut(Shortcut shortcut, Control control, Message message)
		{
			if (!Active && !_isHiddenByMerge)
			{
				return false;
			}
			foreach (RibbonTab tab in Tabs)
			{
				if (tab.TryProcessShortcut(shortcut, control, message))
				{
					return true;
				}
			}
			return false;
		}

		internal void SyncNameFromSite()
		{
			if (Site != null && Site.Name != null)
			{
				Name = Site.Name;
			}
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
				Ribbon.Invalidate(invalidateChildren: true);
			}
			if (GetRootRibbon() != null)
			{
				GetRootRibbon().Invalidate(invalidateChildren: true);
			}
		}

		internal void HookAllControlEvents()
		{
			foreach (Control control in Controls)
			{
				control.GotFocus += OnControlEnter;
				control.LostFocus += OnControlLeave;
				control.DoubleClick += OnControlDoubleClick;
				control.Enter += OnControlEnter;
				control.Leave += OnControlLeave;
			}
		}

		internal void UnhookAllControlEvents()
		{
			foreach (Control control in Controls)
			{
				control.GotFocus -= OnControlEnter;
				control.LostFocus -= OnControlLeave;
				control.DoubleClick -= OnControlDoubleClick;
				control.Enter -= OnControlEnter;
				control.Leave -= OnControlLeave;
			}
		}

		internal void HookControlEvents(Control control)
		{
			if (_ribbon != null)
			{
				control.GotFocus += OnControlEnter;
				control.LostFocus += OnControlLeave;
				control.DoubleClick += OnControlDoubleClick;
				control.Enter += OnControlEnter;
				control.Leave += OnControlLeave;
			}
		}

		internal void UnhookControlEvents(Control control)
		{
			control.GotFocus -= OnControlEnter;
			control.LostFocus -= OnControlLeave;
			control.Enter -= OnControlEnter;
			control.Leave -= OnControlLeave;
			control.DoubleClick -= OnControlDoubleClick;
		}

		private void OnControlDoubleClick(object sender, EventArgs eventArgs)
		{
			Active = true;
			if (!Active || (Ribbon == null && GetRootRibbon() == null))
			{
				return;
			}
			foreach (RibbonTab item in _tabs)
			{
				if (item.Enabled && item.Visible)
				{
					if (IsMerged() && GetRootRibbon() != null)
					{
						GetRootRibbon().SelectedTab = item;
					}
					else if (Ribbon != null)
					{
						Ribbon.SelectedTab = item;
					}
					break;
				}
			}
		}

		private void OnControlEnter(object sender, EventArgs eventArgs)
		{
			Active = true;
		}

		private void OnControlLeave(object sender, EventArgs eventArgs)
		{
			if (Active && sender is Control control && !control.ContainsFocus)
			{
				if (Ribbon != null)
				{
					Active = Ribbon.ShouldKeepContextualTabActive(this);
				}
				else
				{
					Active = false;
				}
			}
		}

		private void DeserializeTabs(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["TabSet"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_tabs);
				return;
			}
			_tabs.DisposeAll();
		}

		internal int CountVisibleTabs()
		{
			int visibleCount = 0;
			foreach (RibbonTab tab in Tabs)
			{
				if (tab.Visible)
				{
					visibleCount++;
				}
			}
			return visibleCount;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			if (disposing)
			{
				if (_ribbon != null && _ribbon.ContextualTabs.Contains(this))
				{
					_ribbon.ContextualTabs.Remove(this);
				}
				if (_tabs != null)
				{
					for (int i = 0; i < _tabs.Count; i++)
					{
						RibbonTab ribbonTab = _tabs[i];
						ribbonTab.SetTabOwner(null);
						ribbonTab.Dispose();
					}
				}
				_standaloneRibbon = null;
			}
			UnhookAllControlEvents();
			base.Dispose(disposing);
		}

		private void InitializeComponents()
		{
			_components = new Container();
		}
	}
}
