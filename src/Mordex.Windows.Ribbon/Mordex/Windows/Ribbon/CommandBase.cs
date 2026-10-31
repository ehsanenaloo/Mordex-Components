using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[ToolboxItem(false)]
	[DefaultEvent("Click")]
	[DesignTimeVisible(false)]
	[Designer("Mordex.Windows.Ribbon.Design.CommandDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class CommandBase : Component, IMordexXmlLayoutsSupport, IImageListProvider
	{
		private bool _visible;

		private string _key;

		private bool _enabled;

		private int _indexInOwner;

		private int _originalIndex;

		private string _accessibleName;

		private string _accessibleDescription;

		private AccessibleRole _accessibleRole;

		private object _tag;

		private ICommandContainer _parentContainer;

		private int _mergeOrder = -1;

		private CommandMergeType _mergeType;

		private bool _isHiddenByMerge;

		private bool? _nullable;

		internal CommandBase _replacedCommand;

		internal CommandMergeType _defaultMergeType;

		internal bool _defaultVisible = true;

		private string _name = "";

		private string _commandType;

		private bool _hasMnemonic;

		private QuickAccessVisibility _quickAccessVisibility;

		internal CommandCollection _commandsOwner;

		private AccessibleObject _cachedAccessibilityObject;

		internal CommandBaseRenderer _renderer;

		internal MxRibbon _ribbon;

		private bool _allowAddToQuickAccessBar;

		internal MxRibbon _directRibbon;

		private MxRibbon _mergeSourceRibbon;

		private static readonly object s_clickEvent = new object();

		private static readonly object s_enabledChangedEvent = new object();

		private static readonly object s_visibleChangedEvent = new object();

		private IContainer _components;

		private bool _isDisposed;

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CommandCollection CommandsOwner
		{
			get
			{
				return _commandsOwner;
			}
			set
			{
				if (_commandsOwner != value)
				{
					_commandsOwner = value;
				}
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		public MxRibbon Ribbon
		{
			get
			{
				if (GetParentContainer() != null)
				{
					return GetParentContainer().Ribbon;
				}
				if (_directRibbon != null)
				{
					return _directRibbon;
				}
				return null;
			}
		}

		[Description("Indicates whether the command is visible or hidded.")]
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
					NotifyTextChanged("Visible");
					OnVisibleChanged(new CommandEventArgs(this));
				}
			}
		}

		[Description("Indicates whether the command can be added to the QuickAccess bar.")]
		[DefaultValue(true)]
		public bool AllowAddToQuickAccessBar
		{
			get
			{
				return _allowAddToQuickAccessBar;
			}
			set
			{
				if (_allowAddToQuickAccessBar != value)
				{
					_allowAddToQuickAccessBar = value;
				}
			}
		}

		[Browsable(false)]
		[Description("Specifies whether the command will be visible when it is added to the QuickAccessCommands collection of the Ribbon.")]
		[DefaultValue(QuickAccessVisibility.Visible)]
		[Obsolete("This property is obsolete, use the QuickAccessCustomizeList collection of the Ribbon to add the command to the customize menu.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public QuickAccessVisibility QuickAccessVisibility
		{
			get
			{
				return _quickAccessVisibility;
			}
			set
			{
				if (_quickAccessVisibility != value)
				{
					_quickAccessVisibility = value;
					NotifyTextChanged("QuickAccessVisibility");
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the Command is enabled.")]
		public virtual bool Enabled
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
					InvalidateLayout();
					OnEnabledChanged(new CommandEventArgs(this));
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
				if (_key != value)
				{
					if (value != null && value.Length <= 0)
					{
						throw new ArgumentException("The Key cannot be blank.");
					}
					_key = value;
				}
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

		internal virtual AccessibleObject AccessibilityObject
		{
			get
			{
				if (_cachedAccessibilityObject == null)
				{
					_cachedAccessibilityObject = new CommandBaseAccessibleObject(this);
				}
				return _cachedAccessibilityObject;
			}
		}

		[DefaultValue(AccessibleRole.Default)]
		[Description("The role that will be reported to accessibility clients.")]
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

		internal virtual AccessibleRole Role
		{
			get
			{
				if (AccessibleRole != AccessibleRole.Default)
				{
					return AccessibleRole;
				}
				switch (CommandType)
				{
				case CommandType.Separator:
					return AccessibleRole.Separator;
				case CommandType.ButtonGroup:
				case CommandType.Box:
				case CommandType.RibbonGroup:
					return AccessibleRole.Grouping;
				case CommandType.Gallery:
					return AccessibleRole.List;
				case CommandType.GalleryItem:
					return AccessibleRole.ListItem;
				case CommandType.Label:
					return AccessibleRole.StaticText;
				case CommandType.DropDownButton:
					if (GetParentContainer() is DropDownCommand)
					{
						return AccessibleRole.MenuItem;
					}
					return AccessibleRole.ButtonDropDown;
				case CommandType.ContainerControl:
					return AccessibleRole.Client;
				default:
					if (GetParentContainer() is DropDownCommand)
					{
						return AccessibleRole.MenuItem;
					}
					return AccessibleRole.PushButton;
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(-1)]
		[Description("Determines the relative position of the Command when the Ribbon is merged.")]
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

		[Description("Determines how the command is handled when merging the Ribbon control")]
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

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
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
		}

		[DefaultValue(null)]
		[Browsable(false)]
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

		[Browsable(false)]
		public Rectangle Bounds
		{
			get
			{
				if (Ribbon != null)
				{
					return Ribbon.GetCommandRendererBounds(this);
				}
				if (GetOwningStatusBar() != null)
				{
					return GetOwningStatusBar().GetCommandBounds(this);
				}
				return Rectangle.Empty;
			}
		}

		internal virtual bool IsEnabled
		{
			get
			{
				if (!Enabled)
				{
					return false;
				}
				if (GetParentContainer() != null)
				{
					if (_parentContainer is MxRibbon && Ribbon != null && Ribbon._mergeHost != null && !Ribbon._mergeHost.Enabled)
					{
						return Ribbon._mergeHost.Enabled;
					}
					return GetParentContainer().IsEnabled;
				}
				return true;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal virtual bool HasMnemonic
		{
			get
			{
				return _hasMnemonic;
			}
			set
			{
				_hasMnemonic = value;
			}
		}

		internal virtual CommandType CommandType => CommandType.CommandBase;

		internal virtual bool CanBeClicked => true;

		ImageList IImageListProvider.ImageList
		{
			get
			{
				if (Ribbon != null)
				{
					if (Ribbon.SuperTipComponent != null && Ribbon.SuperTipComponent.ImageList != null)
					{
						return Ribbon.SuperTipComponent.ImageList;
					}
					return Ribbon.ImageList;
				}
				if (GetOwningStatusBar() != null)
				{
					if (GetOwningStatusBar().SuperTipComponent != null && GetOwningStatusBar().SuperTipComponent.ImageList != null)
					{
						return GetOwningStatusBar().SuperTipComponent.ImageList;
					}
					return GetOwningStatusBar().ImageList;
				}
				if (GetOwningContextMenu() != null)
				{
					return GetOwningContextMenu().ImageList;
				}
				return null;
			}
		}

		[Description("Occurs when the Command is clicked.")]
		public event CommandEventHandler Click
		{
			add
			{
				base.Events.AddHandler(s_clickEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_clickEvent, value);
			}
		}

		[Description("Occurs when the Enabled property is changed.")]
		public event CommandEventHandler EnabledChanged
		{
			add
			{
				base.Events.AddHandler(s_enabledChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_enabledChangedEvent, value);
			}
		}

		[Description("Occurs when the Visible property is changed.")]
		public event CommandEventHandler VisibleChanged
		{
			add
			{
				base.Events.AddHandler(s_visibleChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_visibleChangedEvent, value);
			}
		}

		internal MxRibbon ResolveRootRibbon()
		{
			MxRibbon field_ = _ribbon;
			if (field_ == null && IsMerged() && Ribbon != null)
			{
				field_ = Ribbon._mergeHost;
			}
			if (field_ != null)
			{
				while (field_._mergeHost != null)
				{
					field_ = field_._mergeHost;
				}
			}
			return field_;
		}

		internal CommandBase()
		{
			InitializeComponents();
			InitializeFields();
		}

		internal CommandBase(string key)
			: this()
		{
			_key = key;
		}

		internal virtual void InitializeFields()
		{
			_visible = true;
			_enabled = true;
			_mergeOrder = -1;
			_mergeType = CommandMergeType.Replace;
			_defaultMergeType = CommandMergeType.Replace;
			_quickAccessVisibility = QuickAccessVisibility.Visible;
			_indexInOwner = -1;
			_originalIndex = -1;
			_key = "";
			_allowAddToQuickAccessBar = true;
			_accessibleRole = AccessibleRole.Default;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			Deserialize(reader);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter writer)
		{
			Serialize(writer);
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

		internal MxRibbonContextMenu GetOwningContextMenu()
		{
			if (GetParentContainer() != null)
			{
				return GetParentContainer().RibbonContextMenu;
			}
			return null;
		}

		internal MxRibbonStatusBar GetOwningStatusBar()
		{
			if (GetParentContainer() != null)
			{
				if (GetParentContainer() is MxRibbonStatusBar)
				{
					return GetParentContainer() as MxRibbonStatusBar;
				}
				if (GetParentContainer() is CommandBase)
				{
					return ((CommandBase)GetParentContainer()).GetOwningStatusBar();
				}
			}
			return null;
		}

		internal MxRibbonContextMenu FindContextMenuOwner()
		{
			if (GetParentContainer() != null)
			{
				if (GetParentContainer() is MxRibbonContextMenu)
				{
					return GetParentContainer() as MxRibbonContextMenu;
				}
				if (GetParentContainer() is CommandBase)
				{
					return ((CommandBase)GetParentContainer()).FindContextMenuOwner();
				}
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public bool ShouldSerializeVisible()
		{
			return _visible != _defaultVisible;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public void ResetVisible()
		{
			Visible = _defaultVisible;
		}

		internal void SetHiddenByMerge(bool commandBaseBoolean)
		{
			_isHiddenByMerge = commandBaseBoolean;
		}

		internal void SyncNameFromSite()
		{
			if (Site != null && Site.Name != null)
			{
				Name = Site.Name;
			}
		}

		internal void SetNullable(bool nullable)
		{
			_nullable = nullable;
		}

		internal void ApplyMergeState(MxRibbon mergeRibbon, bool nullable)
		{
			if (_nullable == nullable || !_nullable.HasValue)
			{
				return;
			}
			if (!nullable)
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
			_nullable = nullable;
		}

		internal bool IsMerged()
		{
			if (!_nullable.HasValue)
			{
				if (_parentContainer != null)
				{
					if (_parentContainer is CommandBase)
					{
						return ((CommandBase)_parentContainer).IsMerged();
					}
					if (_parentContainer is RibbonTab)
					{
						return ((RibbonTab)_parentContainer).IsMerged();
					}
				}
				return false;
			}
			return _nullable.Value;
		}

		internal int GetIndexInOwner()
		{
			return _indexInOwner;
		}

		internal void SetIndexInOwner(int index)
		{
			_indexInOwner = index;
		}

		internal int GetOriginalIndex()
		{
			return _originalIndex;
		}

		internal void SetOriginalIndex(int commandBaseInteger)
		{
			_originalIndex = commandBaseInteger;
		}

		internal ICommandContainer GetParentContainer()
		{
			return _parentContainer;
		}

		internal void SetParentContainer(ICommandContainer iAccessibleRibbonElement)
		{
			_parentContainer = iAccessibleRibbonElement;
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMergeType()
		{
			return _mergeType != _defaultMergeType;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public void ResetMergeType()
		{
			MergeType = _defaultMergeType;
		}

		public virtual object Clone()
		{
			CommandBase commandBase = new CommandBase();
			commandBase._enabled = _enabled;
			commandBase._key = _key;
			commandBase._tag = _tag;
			commandBase._visible = _visible;
			commandBase._allowAddToQuickAccessBar = _allowAddToQuickAccessBar;
			return commandBase;
		}

		internal virtual bool SearchShortcut(Shortcut shortcut, Control inputControl, Message msg)
		{
			return false;
		}

		internal virtual void Serialize(LayoutWriter writer)
		{
			writer.AddValue("CommandType", GetType().FullName);
			if (!_allowAddToQuickAccessBar)
			{
				writer.AddValue("AllowAddToQuickAccessBar", _allowAddToQuickAccessBar);
			}
			if (!_enabled)
			{
				writer.AddValue("Enabled", _enabled);
			}
			if (_key != null)
			{
				writer.AddValue("Key", _key);
			}
			if (_mergeOrder != -1)
			{
				writer.AddValue("MergeOrder", _mergeOrder);
			}
			if (_mergeType != _defaultMergeType)
			{
				writer.AddValue("MergeType", _mergeType);
			}
			if (Name != null && Name.Length > 0)
			{
				writer.AddValue("Name", _name);
			}
			if (_quickAccessVisibility != QuickAccessVisibility.Visible)
			{
				writer.AddValue("QuickAccessVisibility", _quickAccessVisibility);
			}
			if (_tag != null)
			{
				if (_tag.GetType() != typeof(string))
				{
					writer.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				writer.AddValue("Tag", _tag);
			}
			if (!_visible)
			{
				writer.AddValue("Visible", _visible);
			}
		}

		internal virtual void Deserialize(LayoutReader reader)
		{
			_commandType = (string)LayoutReader.GetPropertyValue(reader, "CommandType", typeof(string), GetType().FullName);
			_name = (string)LayoutReader.GetPropertyValue(reader, "Name", typeof(string), "");
			_enabled = (bool)LayoutReader.GetPropertyValue(reader, "Enabled", typeof(bool), true);
			_allowAddToQuickAccessBar = (bool)LayoutReader.GetPropertyValue(reader, "AllowAddToQuickAccessBar", typeof(bool), true);
			_key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), null);
			_mergeOrder = (int)LayoutReader.GetPropertyValue(reader, "MergeOrder", typeof(int), -1);
			_mergeType = (CommandMergeType)LayoutReader.GetPropertyValue(reader, "MergeType", typeof(CommandMergeType), _defaultMergeType);
			_visible = (bool)LayoutReader.GetPropertyValue(reader, "Visible", typeof(bool), true);
			_quickAccessVisibility = (QuickAccessVisibility)LayoutReader.GetPropertyValue(reader, "QuickAccessVisibility", typeof(QuickAccessVisibility), QuickAccessVisibility.Visible);
			_tag = LayoutTypeRegistry.ReadTag(reader);
		}

		internal void NotifyTextChanged(string text)
		{
			if (Ribbon != null)
			{
				Ribbon.NotifyCommandPropertyChanged(this, text);
			}
			if (IsMerged() && Ribbon != null && Ribbon._mergeHost != null)
			{
				NotifyParentRibbonsTextChanged(Ribbon, text);
			}
			if (ResolveRootRibbon() != null)
			{
				ResolveRootRibbon().NotifyCommandPropertyChanged(this, text);
			}
			if (GetOwningStatusBar() != null)
			{
				GetOwningStatusBar().OnCommandPropertyChanged(this, text);
			}
		}

		internal void NotifyParentRibbonsTextChanged(MxRibbon ribbon, string text)
		{
			if (ribbon != null && ribbon._mergeHost != null)
			{
				ribbon._mergeHost.NotifyCommandPropertyChanged(this, text);
				NotifyParentRibbonsTextChanged(ribbon._mergeHost, text);
			}
		}

		internal void InvalidateOwnerRibbons(MxRibbon ribbon)
		{
			if (ribbon != null && ribbon._mergeHost != null)
			{
				ribbon._mergeHost.InvalidateRibbonAndQuickAccess();
				InvalidateOwnerRibbons(ribbon._mergeHost);
			}
		}

		internal void InvalidateLayout()
		{
			if (Ribbon != null)
			{
				Ribbon.InvalidateCommandLayout(this);
			}
			if (ResolveRootRibbon() != null)
			{
				ResolveRootRibbon().InvalidateCommandLayout(this);
			}
			else if (Ribbon != null && Ribbon._mergeHost != null)
			{
				InvalidateOwnerRibbons(Ribbon);
			}
			else if (CommandsOwner != null && CommandsOwner._parent is CommandBase)
			{
				((CommandBase)CommandsOwner._parent).InvalidateLayout();
			}
			if (GetOwningStatusBar() != null)
			{
				GetOwningStatusBar().Invalidate();
			}
		}

		public virtual void InvokeOnClick()
		{
			// One check of the enabled state covers the state change of the click and the click event, so a disabled
			// command or container changes nothing.
			if (!IsEnabled)
			{
				return;
			}
			ApplyClickState();
			if (Ribbon != null)
			{
				if (Ribbon._mergeHost != null)
				{
					Ribbon._mergeHost.SetKeyTipMode(false);
				}
				Ribbon.SetKeyTipMode(false);
			}
			CommandEventArgs e = new CommandEventArgs(this);
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandClicked(e);
			}
			OnClick(e);
		}

		/// <summary>The state that a click changes before the click event, for example the checked state. It runs only for an enabled command.</summary>
		private protected virtual void ApplyClickState()
		{
		}

		protected virtual void OnClick(CommandEventArgs e)
		{
			if (base.Events[s_clickEvent] is CommandEventHandler commandEventHandler && !base.DesignMode)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnEnabledChanged(CommandEventArgs e)
		{
			if (base.Events[s_enabledChangedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnVisibleChanged(CommandEventArgs e)
		{
			if (base.Events[s_visibleChangedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
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
				if (_parentContainer != null)
				{
					_parentContainer.CommandDisposed(new CommandEventArgs(this));
				}
			}
			_directRibbon = null;
			_ribbon = null;
			base.Dispose(disposing);
			if (disposing)
			{
				// The Disposed event is raised by the base class, so the handlers are released after it.
				base.Events.Dispose();
			}
		}

		internal bool HasBeenDisposed()
		{
			return _isDisposed;
		}

		private void InitializeComponents()
		{
			_components = new Container();
		}
	}
}
