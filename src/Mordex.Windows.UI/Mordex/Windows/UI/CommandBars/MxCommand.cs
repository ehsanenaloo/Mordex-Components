using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	[DefaultEvent("Click")]
	[TypeConverter(typeof(CommandConverter))]
	[DesignerSerializer("Mordex.Windows.UI.Design.CommandSerializer, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[ToolboxItem(false)]
	[DefaultProperty("CommandType")]
	[DesignTimeVisible(false)]
	public partial class MxCommand : Component, IDisposable, IAccessibleCommandBarsElement, IFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		private const int DefaultControlWidth = 100;

		// Marks that the text has no mnemonic character.
		private const char NoMnemonic = '0';

		private string _accessibleName;

		private string _accessibleDescription;

		private AccessibleRole _accessibleRole = AccessibleRole.Default;

		internal CommandLayoutState _owner;

		internal bool _mouseDown;

		private string _name = "";

		private bool _showInCustomizeDialog = true;

		private int _position = -1;

		private int _listIndex = -1;

		private MxCommand _mergeTarget;

		private MxCommand _baseCommand;

		private bool _isAddRemoveButtonsItem;

		internal bool _isMergedCommand;

		internal Guid _guid;

		private bool _isHiddenByMerge;

		private string _description = "";

		private CommandAlignment _alignment;

		private AccessibleObject _accessibleObject;

		internal ContentAlignment _textAlignment;

		private ContentAlignment _imageAlignment;

		private TextImageRelation _textImageRelation;

		private Size _commandSize;

		private InheritableBoolean _allowCustomizeCommands;

		private InheritableBoolean _allowFloatingChildren;

		internal CommandMergeType _mergeType;

		private int _mergeOrder = -1;

		private int _layoutIndex;

		private string _categoryName = "";

		private CommandCategory _category;

		private InheritableBoolean _checkedValue;

		private InheritableBoolean _isEditableControl;

		private Control _control;

		private CommandStyle _commandStyle;

		private DropDownButtonStyle _dropDownButtonStyle;

		private CommandType _commandType = CommandType.Command;

		private Hashtable _hostedControls;

		private string _text = "";

		private Icon _icon;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private int _largeImageIndex = -1;

		private string _largeImageKey = "";

		private int _hotImageIndex = -1;

		private string _hotImageKey = "";

		private int _largeHotImageIndex = -1;

		private string _largeHotImageKey = "";

		private int _disabledImageIndex = -1;

		private string _disabledImageKey = "";

		private int _largeDisabledImageIndex = -1;

		private string _largeDisabledImageKey = "";

		private int _selectedImageIndex = -1;

		private string _selectedImageKey = "";

		private int _largeSelectedImageIndex = -1;

		private string _largeSelectedImageKey = "";

		private int _daysForInfrequency = -1;

		private Color _imageReplaceableColor = Color.Empty;

		private Image _commandImage;

		private string _key = "";

		private string _builtInId = "";

		private StateStyles _stateStyles;

		private PopupSettings _popupSettings;

		private MxCommandCollection _commands;

		private object _originalShortcut;

		private bool _isShortcutSet;

		private Shortcut _shortcut;

		private bool _hasMnemonic;

		private char _mnemonic = NoMnemonic;

		internal InheritableBoolean _visible;

		private object _tag;

		private string _tagString;

		private Cursor _cursor;

		private InheritableBoolean _mdiList;

		private int _controlWidth = -1;

		private InheritableBoolean _defaultItem;

		private InheritableBoolean _showTextInContainers;

		private InheritableBoolean _enabled;

		private string _toolTipText = "";

		private FrequencyMode _frequencyMode = FrequencyMode.Frequent;

		internal MxCommandManager _commandManager;

		private DateTime _referenceDate = DateTime.MinValue;

		private DateTime _lastClickedDate = DateTime.MinValue;

		internal bool _isClickInvokedInCode;

		private IAccessibleCommandBarsElement _hostElement;

		private MxCommandBar _floatingBar;

		private static readonly object _mouseHoverEvent = new object();

		private static readonly object _mouseLeaveEvent = new object();

		private static readonly object _controlValueChangedEvent = new object();

		private static readonly object _clickEvent = new object();

		private static readonly object _closeUpEvent = new object();

		private static readonly object _closingEvent = new object();

		private static readonly object _popupEvent = new object();

		private static readonly object _commandHoverEvent = new object();

		private static readonly object _commandMouseLeaveEvent = new object();

		private static readonly object _commandControlValueChangedEvent = new object();

		private static readonly object _commandClickedEvent = new object();

		private static readonly object _commandExpandedEvent = new object();

		private static readonly object _commandCollapsedEvent = new object();

		private static readonly object _commandCloseUpEvent = new object();

		private static readonly object _commandClosingEvent = new object();

		private static readonly object _commandPopupEvent = new object();

		private static readonly object _commandRemovedEvent = new object();

		private static readonly object _commandAddedEvent = new object();

		private static readonly object _commandDisposedEvent = new object();

		// Set when ForceDispose or Dispose() already released the command, so the base Dispose call only raises Disposed.
		private bool _isCleanedUp;

		private static readonly object _invalidatedEvent = new object();

		private static readonly object _expandRequestedEvent = new object();

		private static readonly object _collapseRequestedEvent = new object();

		public MxCommand(string key)
		{
			Key = key;
		}

		public MxCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public MxCommand(string key, string text, int imageIndex)
		{
			Key = key;
			Text = text;
			_imageIndex = imageIndex;
			_largeImageIndex = imageIndex;
		}

		public MxCommand(string key, string text, CommandType commandType)
		{
			Key = key;
			Text = text;
			CommandType = commandType;
		}

		public MxCommand()
		{
		}

		public MxCommand(string key, string text, int imageIndex, Shortcut shortcut)
		{
			Key = key;
			Text = text;
			_imageIndex = imageIndex;
			_largeImageIndex = imageIndex;
			_shortcut = shortcut;
		}

		public MxCommand(MxCommand baseCommand)
		{
			while (baseCommand.BaseCommand != null)
			{
				baseCommand = baseCommand.BaseCommand;
			}
			SetBaseCommand(baseCommand);
		}

		internal void SetBaseCommand(MxCommand baseCommand)
		{
			if (_baseCommand == baseCommand)
			{
				return;
			}
			UnhookBaseCommand();
			if (baseCommand == null)
			{
				_key = _baseCommand.Key;
			}
			_baseCommand = baseCommand;
			if (_baseCommand != null)
			{
				_baseCommand.AddInvalidatedHandler(RelayInvalidated);
				_baseCommand.AddCommandClickedHandler(RelayCommandClicked);
				_baseCommand.AddCommandHoverHandler(RelayCommandHover);
				_baseCommand.AddCommandMouseLeaveHandler(RelayCommandMouseLeave);
				_baseCommand.AddCommandPopupHandler(RelayCommandPopup);
				_baseCommand.AddCommandCloseUpHandler(RelayCommandCloseUp);
				_baseCommand.AddCommandClosingHandler(RelayCommandClosing);
				_baseCommand.AddCommandAddedHandler(RelayCommandAdded);
				_baseCommand.AddCommandRemovedHandler(RelayCommandRemoved);
				if (!_baseCommand.IsMergedCommand)
				{
					_baseCommand.AddCommandDisposedHandler(OnBaseCommandDisposed);
				}
				_baseCommand.AddCommandControlValueChangedHandler(RelayCommandControlValueChanged);
				_baseCommand.ControlValueChanged += OnBaseCommandControlValueChanged;
				_baseCommand.AddCommandExpandedHandler(RelayCommandExpanded);
				_baseCommand.AddCommandCollapsedHandler(RelayCommandCollapsed);
				if (HasEmbeddedControl() && Control != null)
				{
					Control.Width = ControlWidth;
				}
			}
		}

		internal bool CanModify()
		{
			if (_baseCommand != null)
			{
				return _baseCommand.CanModify();
			}
			return CanModifySelf();
		}

		internal bool CanModifySelf()
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

		internal bool CanBeClicked()
		{
			if (IsEnabled)
			{
				if (IsSeparator())
				{
					return false;
				}
				if (HasEmbeddedControl())
				{
					return false;
				}
				if (CommandType == CommandType.Label)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		internal bool CanBeHotTracked()
		{
			if (IsEnabled)
			{
				if (IsSeparator())
				{
					return false;
				}
				switch (CommandType)
				{
				case CommandType.Label:
				case CommandType.ComboBoxCommand:
				case CommandType.TextBoxCommand:
				case CommandType.ControlContainer:
					return false;
				default:
					return true;
				}
			}
			return false;
		}

		internal bool IsDesignerEditing()
		{
			if (base.DesignMode)
			{
				if (CommandManager != null && CommandManager.IsEditorDialogOpen())
				{
					return false;
				}
				return true;
			}
			return false;
		}

		internal void SyncNameFromSite()
		{
			if (Site != null && Site.Name.Length > 0)
			{
				Name = Site.Name;
			}
		}

		internal int GetPosition()
		{
			return _position;
		}

		internal void UpdatePosition(int position)
		{
			if (_position != position)
			{
				_position = position;
			}
		}

		internal int GetListIndex()
		{
			return _listIndex;
		}

		internal void SetListIndex(int listIndex)
		{
			_listIndex = listIndex;
		}

		internal MxCommand GetMergeTarget()
		{
			return _mergeTarget;
		}

		internal void SetMergeTarget(MxCommand uiCommand)
		{
			_mergeTarget = uiCommand;
		}

		public void Expand()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			RaiseExpandRequested(e);
		}

		public void Collapse()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			RaiseCollapseRequested(e);
		}

		internal bool IsAddRemoveButtonsItem()
		{
			if (_baseCommand != null)
			{
				return _baseCommand._isAddRemoveButtonsItem;
			}
			return _isAddRemoveButtonsItem;
		}

		internal void SetAddRemoveButtonsItem(bool isAddRemoveButtonsItem)
		{
			if (_baseCommand != null)
			{
				BaseCommand._isAddRemoveButtonsItem = isAddRemoveButtonsItem;
			}
			else
			{
				_isAddRemoveButtonsItem = isAddRemoveButtonsItem;
			}
		}

		public virtual MxCommand Clone()
		{
			return CreateCopy(true);
		}

		internal MxCommand CreateCopy(bool copyCommands)
		{
			MxCommand uiCommand = new MxCommand();
			uiCommand.SetBaseCommand(_baseCommand);
			uiCommand._allowFloatingChildren = _allowFloatingChildren;
			uiCommand._allowCustomizeCommands = _allowCustomizeCommands;
			uiCommand._checkedValue = _checkedValue;
			uiCommand._isMergedCommand = IsMergedCommand;
			if (copyCommands)
			{
				uiCommand._commands = Commands.CloneCommands();
			}
			else
			{
				uiCommand._commands = new MxCommandCollection(uiCommand);
			}
			uiCommand._category = _category;
			uiCommand._categoryName = _categoryName;
			uiCommand._commandStyle = _commandStyle;
			uiCommand._commandType = _commandType;
			uiCommand._cursor = _cursor;
			uiCommand._description = _description;
			uiCommand._enabled = _enabled;
			uiCommand._icon = _icon;
			uiCommand._commandImage = _commandImage;
			uiCommand._frequencyMode = _frequencyMode;
			uiCommand._daysForInfrequency = _daysForInfrequency;
			uiCommand._isEditableControl = _isEditableControl;
			uiCommand._mdiList = _mdiList;
			uiCommand._accessibleDescription = _accessibleDescription;
			uiCommand._accessibleName = _accessibleName;
			uiCommand._accessibleRole = _accessibleRole;
			uiCommand._disabledImageIndex = _disabledImageIndex;
			uiCommand._disabledImageKey = _disabledImageKey;
			uiCommand._largeDisabledImageIndex = _largeDisabledImageIndex;
			uiCommand._largeDisabledImageKey = _largeDisabledImageKey;
			uiCommand._hotImageIndex = _hotImageIndex;
			uiCommand._hotImageKey = _hotImageKey;
			uiCommand._largeHotImageIndex = _largeHotImageIndex;
			uiCommand._largeHotImageKey = _largeHotImageKey;
			uiCommand._selectedImageIndex = _selectedImageIndex;
			uiCommand._selectedImageKey = _selectedImageKey;
			uiCommand._largeSelectedImageIndex = _largeSelectedImageIndex;
			uiCommand._largeSelectedImageKey = _largeSelectedImageKey;
			uiCommand._imageIndex = _imageIndex;
			uiCommand._imageKey = _imageKey;
			uiCommand._largeImageIndex = _largeImageIndex;
			uiCommand._largeImageKey = _largeImageKey;
			uiCommand._mergeOrder = _mergeOrder;
			uiCommand._mergeType = _mergeType;
			if (_popupSettings != null)
			{
				uiCommand._popupSettings = PopupSettings.CreateCopy();
			}
			uiCommand._shortcut = _shortcut;
			if (_stateStyles != null)
			{
				uiCommand._stateStyles = new StateStyles(StateStyles);
			}
			uiCommand.Tag = _tag;
			uiCommand.Text = _text;
			uiCommand._toolTipText = _toolTipText;
			uiCommand._visible = _visible;
			return uiCommand;
		}

		internal void SetIsMergedCommand(bool isMergedCommand)
		{
			_isMergedCommand = isMergedCommand;
		}

		internal void UpdateMergeState(Guid guid, bool isMergedCommand)
		{
			if (_isMergedCommand == isMergedCommand)
			{
				return;
			}
			if (!isMergedCommand)
			{
				if (_guid != guid)
				{
					return;
				}
			}
			else
			{
				_guid = guid;
				_key = Key;
			}
			_isMergedCommand = isMergedCommand;
		}

		internal bool IsHiddenByMerge()
		{
			return _isHiddenByMerge;
		}

		internal void SetHiddenByMerge(bool isHiddenByMerge)
		{
			_isHiddenByMerge = isHiddenByMerge;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new CommandAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal int GetLayoutIndex()
		{
			return _layoutIndex;
		}

		internal void SetLayoutIndex(int layoutIndex)
		{
			_layoutIndex = layoutIndex;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isCleanedUp)
			{
				base.Dispose(disposing);
				base.Events.Dispose();
				return;
			}
			if (base.Container != null)
			{
				base.Container.Remove(this);
			}
			if (_commands != null)
			{
				_commands.DetachAll();
				for (int i = 0; i < _commands.Count; i++)
				{
					MxCommand uiCommand = _commands[i];
					uiCommand.ForceDispose();
				}
				_commands._originalCommands = null;
			}
			CommandEventArgs e = new CommandEventArgs(this);
			RaiseCommandDisposed(e);
			if (_hostedControls != null)
			{
				foreach (Control value in _hostedControls.Values)
				{
					value.Dispose();
				}
			}
			if (_control != null && (CommandManager == null || !CommandManager._isBatchUpdating))
			{
				_control.Dispose();
				_control = null;
			}
			UnhookBaseCommand();
			_baseCommand = null;
			_hostElement = null;
			_commandManager = null;
			base.Dispose(disposing);
			base.Events.Dispose();
		}

		internal void ForceDispose()
		{
			if (base.Container != null)
			{
				base.Container.Remove(this);
			}
			UnhookBaseCommand();
			if (_commands != null)
			{
				_commands.DetachAll();
				for (int i = 0; i < _commands.Count; i++)
				{
					MxCommand uiCommand = _commands[i];
					uiCommand.ForceDispose();
				}
			}
			_baseCommand = null;
			if (_hostedControls != null)
			{
				foreach (Control value in _hostedControls.Values)
				{
					value.Dispose();
				}
			}
			if (_control != null && (CommandManager == null || !CommandManager._isBatchUpdating))
			{
				_control.Dispose();
				_control = null;
			}
			_hostElement = null;
			_commandManager = null;
			_isCleanedUp = true;
			base.Dispose();
		}

		public new void Dispose()
		{
			if (IsMergedCommand)
			{
				return;
			}
			if (base.Container != null)
			{
				base.Container.Remove(this);
			}
			UnhookBaseCommand();
			if (_commands != null)
			{
				_commands.DetachAll();
				for (int i = 0; i < _commands.Count; i++)
				{
					MxCommand uiCommand = _commands[i];
					uiCommand.ForceDispose();
				}
				_commands._originalCommands = null;
			}
			if (_hostedControls != null)
			{
				foreach (Control value in _hostedControls.Values)
				{
					value.Dispose();
				}
			}
			CommandEventArgs e = new CommandEventArgs(this);
			RaiseCommandDisposed(e);
			if (_control != null && (CommandManager == null || !CommandManager._isBatchUpdating))
			{
				_control.Dispose();
				_control = null;
			}
			_hostElement = null;
			_baseCommand = null;
			_commandManager = null;
			_isCleanedUp = true;
			base.Dispose();
		}

		private void UnhookBaseCommand()
		{
			if (_baseCommand != null)
			{
				_baseCommand.RemoveInvalidatedHandler(RelayInvalidated);
				_baseCommand.ControlValueChanged -= OnBaseCommandControlValueChanged;
				_baseCommand.RemoveCommandClickedHandler(RelayCommandClicked);
				_baseCommand.RemoveCommandHoverHandler(RelayCommandHover);
				_baseCommand.RemoveCommandMouseLeaveHandler(RelayCommandMouseLeave);
				_baseCommand.RemoveCommandPopupHandler(RelayCommandPopup);
				_baseCommand.RemoveCommandCloseUpHandler(RelayCommandCloseUp);
				_baseCommand.RemoveCommandClosingHandler(RelayCommandClosing);
				_baseCommand.RemoveCommandAddedHandler(RelayCommandAdded);
				_baseCommand.RemoveCommandRemovedHandler(RelayCommandRemoved);
				_baseCommand.RemoveCommandDisposedHandler(OnBaseCommandDisposed);
				_baseCommand.RemoveCommandControlValueChangedHandler(RelayCommandControlValueChanged);
				_baseCommand.RemoveCommandExpandedHandler(RelayCommandExpanded);
				_baseCommand.RemoveCommandCollapsedHandler(RelayCommandCollapsed);
			}
		}

		internal void RestoreOriginalShortcut()
		{
			if (_originalShortcut != null)
			{
				Shortcut = (Shortcut)_originalShortcut;
				_originalShortcut = null;
			}
		}

		internal void ResetCustomizedProperties()
		{
			_checkedValue = InheritableBoolean.Default;
			Commands.UpdateOwner();
			_commandStyle = CommandStyle.Default;
			_description = "";
			_disabledImageIndex = -1;
			_disabledImageKey = "";
			_largeDisabledImageIndex = -1;
			_largeDisabledImageKey = "";
			_hotImageIndex = -1;
			_hotImageKey = "";
			_largeHotImageIndex = -1;
			_largeHotImageKey = "";
			_imageIndex = -1;
			_imageKey = "";
			_largeImageIndex = -1;
			_largeImageKey = "";
			_selectedImageIndex = -1;
			_selectedImageKey = "";
			_largeSelectedImageIndex = -1;
			_largeSelectedImageKey = "";
			_icon = null;
			_commandImage = null;
			Shortcut = Shortcut.None;
			Text = "";
			_toolTipText = "";
			_visible = InheritableBoolean.Default;
			_mdiList = InheritableBoolean.Default;
			_enabled = InheritableBoolean.Default;
		}

		internal string GetBuiltInId()
		{
			if (_builtInId.Length > 0)
			{
				return _builtInId;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetBuiltInId();
			}
			return _builtInId;
		}

		internal void SetBuiltInId(string builtInId)
		{
			_builtInId = builtInId;
		}

		internal void UpdateCommands(MxCommandCollection commands)
		{
			if (_commands != null)
			{
				_commands.DetachAllAndReset();
			}
			_commands = commands;
		}

		internal object GetOriginalShortcut()
		{
			return _originalShortcut;
		}

		internal void UpdateOriginalShortcut(object originalShortcut)
		{
			if (_originalShortcut == null)
			{
				_originalShortcut = originalShortcut;
			}
		}

		internal bool HasMnemonic()
		{
			if (!_hasMnemonic && _baseCommand != null)
			{
				return _baseCommand.HasMnemonic();
			}
			return _hasMnemonic;
		}

		internal void SetHasMnemonic(bool hasMnemonic)
		{
			_hasMnemonic = hasMnemonic;
		}

		internal bool IsSeparator()
		{
			return CommandType == CommandType.Separator;
		}

		internal void SetVisible(bool isVisible)
		{
			if (isVisible)
			{
				Visible = InheritableBoolean.True;
			}
			else
			{
				Visible = InheritableBoolean.False;
			}
		}

		internal bool IsInfrequent()
		{
			if (FrequencyMode == FrequencyMode.AlwaysFrequent)
			{
				return false;
			}
			if (FrequencyMode == FrequencyMode.AlwaysInfrequent)
			{
				return true;
			}
			if (GetReferenceDate() == DateTime.MinValue)
			{
				if (GetLastClickedDate() != DateTime.MinValue)
				{
					return false;
				}
				if (FrequencyMode == FrequencyMode.Infrequent)
				{
					return true;
				}
				return false;
			}
			int infrequencyDays = GetEffectiveDaysForInfrequency();
			if (infrequencyDays == 0)
			{
				if (FrequencyMode == FrequencyMode.Infrequent)
				{
					return true;
				}
				return false;
			}
			double elapsedDays = 0.0;
			if (GetLastClickedDate() == DateTime.MinValue)
			{
				elapsedDays = DateTime.Now.Subtract(GetReferenceDate()).TotalDays;
				if (elapsedDays > (double)infrequencyDays)
				{
					return true;
				}
				if (FrequencyMode == FrequencyMode.Infrequent)
				{
					return true;
				}
				return false;
			}
			if (GetLastClickedDate() > DateTime.Now)
			{
				return false;
			}
			elapsedDays = GetReferenceDate().Subtract(GetLastClickedDate()).TotalDays;
			if (elapsedDays > (double)infrequencyDays)
			{
				return true;
			}
			return false;
		}

		internal MxCommand[] CreateMdiListCommands()
		{
			if (CommandType == CommandType.MdiList)
			{
				Form form = CommandManager.GetForm();
				if (form != null && form.MdiChildren.Length > 0)
				{
					int index = 0;
					MxCommand[] mdiCommands;
					if (Commands.Count > 0)
					{
						mdiCommands = new MxCommand[form.MdiChildren.Length + 1];
						MxCommand uiCommand = new MxCommand();
						uiCommand.CommandType = CommandType.Separator;
						mdiCommands.SetValue(uiCommand, index);
						index++;
					}
					else
					{
						mdiCommands = new MxCommand[form.MdiChildren.Length];
					}
					int windowNumber = 1;
					Form[] mdiChildren = form.MdiChildren;
					foreach (Form mdiChild in mdiChildren)
					{
						MxCommand windowCommand = new MxCommand();
						windowCommand.Text = "&" + windowNumber + " " + mdiChild.Text;
						if (form.ActiveMdiChild == mdiChild)
						{
							windowCommand.Checked = InheritableBoolean.True;
						}
						windowCommand.Tag = mdiChild;
						mdiCommands.SetValue(windowCommand, index);
						index++;
						windowNumber++;
					}
					return mdiCommands;
				}
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUsageData()
		{
			SetReferenceDate(DateTime.MinValue);
			SetLastClickedDate(DateTime.MinValue);
		}

		private DateTime GetReferenceDate()
		{
			if (_baseCommand != null)
			{
				return _baseCommand._referenceDate;
			}
			return _referenceDate;
		}

		private void SetReferenceDate(DateTime dateTime)
		{
			if (_baseCommand != null)
			{
				_baseCommand._referenceDate = dateTime;
			}
			else
			{
				_referenceDate = dateTime;
			}
		}

		private DateTime GetLastClickedDate()
		{
			if (_baseCommand != null)
			{
				return _baseCommand._lastClickedDate;
			}
			return _lastClickedDate;
		}

		private void SetLastClickedDate(DateTime lastClickedDate)
		{
			if (_baseCommand != null)
			{
				_baseCommand._lastClickedDate = lastClickedDate;
			}
			else
			{
				_lastClickedDate = lastClickedDate;
			}
		}

		public void InvokeOnClick()
		{
			_isClickInvokedInCode = true;
			PerformClick();
			_isClickInvokedInCode = false;
		}

		internal void PerformClick()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			if (CommandType == CommandType.ToggleButton)
			{
				if (IsChecked)
				{
					if (_baseCommand != null)
					{
						_baseCommand.Checked = InheritableBoolean.False;
						if (_checkedValue != InheritableBoolean.Default)
						{
							Checked = InheritableBoolean.False;
						}
					}
					else
					{
						Checked = InheritableBoolean.False;
					}
				}
				else if (_baseCommand != null)
				{
					_baseCommand.Checked = InheritableBoolean.True;
					if (_checkedValue != InheritableBoolean.Default)
					{
						Checked = InheritableBoolean.True;
					}
				}
				else
				{
					Checked = InheritableBoolean.True;
				}
			}
			SetLastClickedDate(DateTime.Now);
			OnClick(e);
			if (_baseCommand != null)
			{
				CommandEventArgs baseEventArgs = new CommandEventArgs(BaseCommand);
				_baseCommand.OnClick(baseEventArgs);
			}
		}

		internal IAccessibleCommandBarsElement GetHostElement()
		{
			return _hostElement;
		}

		internal void SetHostElement(IAccessibleCommandBarsElement hostElement)
		{
			_hostElement = hostElement;
		}

		ImageList IAccessibleCommandBarsElement.GetImageList(bool smallIcons, MxCommand cmd)
		{
			if (_hostElement != null)
			{
				return _hostElement.GetImageList(smallIcons, this);
			}
			return null;
		}

		MxCommandManager IAccessibleCommandBarsElement.GetCommandManager()
		{
			return CommandManager;
		}
	}
}
