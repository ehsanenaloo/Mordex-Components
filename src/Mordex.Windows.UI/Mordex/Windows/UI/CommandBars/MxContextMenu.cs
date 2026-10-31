using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	[TypeConverter(typeof(ContextMenuConverter))]
	[Editor(typeof(ContextMenuEditor), typeof(UITypeEditor))]
	[DefaultProperty("VisualStyle")]
	[ToolboxItem(false)]
	[DefaultEvent("CommandClick")]
	public partial class MxContextMenu : Component, IAccessibleCommandBarsElement, IFormatStyleOwner, ICommandMenuOwner, ISupportInitialize, IMordexXmlLayoutsSupport
	{
		private Container _components = new Container();

		private RightToLeft _rightToLeft = RightToLeft.Inherit;

		private bool _isProcessingShortcut;

		private bool _isRightButtonDown;

		private bool _shouldShowInfrequentItems;

		private bool _isInitializing;

		private Size _smallImageSize = Size.Empty;

		private Size _largeImageSize = Size.Empty;

		private InheritableBoolean _showToolTips;

		internal Control _sourceControl;

		internal int _index = -1;

		private string _key = "";

		private MxCommandCollection _commands;

		private ImageList _imageList;

		private ImageList _largeImageList;

		internal MxCommandManager _commandManager;

		private PopupSettings _popupSettings;

		private InheritableBoolean _showShadowUnderMenus;

		private View _view;

		private bool _useMnemonic = true;

		private DropDownAnimation _dropDownAnimation;

		private CommandControl _popupControl;

		internal bool _isRightAligned;

		internal int _ownerHeight = -1;

		internal int _ownerWidth = -1;

		private static readonly object _commandMouseHoverEvent = new object();

		private static readonly object _commandMouseLeaveEvent = new object();

		private static readonly object _commandClickEvent = new object();

		private static readonly object _commandControlValueChangedEvent = new object();

		private static readonly object _popupEvent = new object();

		private static readonly object _closedEvent = new object();

		private static readonly object _closingEvent = new object();

		private static readonly object _commandCloseUpEvent = new object();

		private static readonly object _commandClosingEvent = new object();

		private static readonly object _commandPopupEvent = new object();

		public MxContextMenu()
			: this(null, "")
		{
		}

		public MxContextMenu(string key)
			: this(null, key)
		{
		}

		public MxContextMenu(MxCommandManager commandManager, string key)
		{
			InitializeMembers();
			CommandManager = commandManager;
			Key = key;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_dropDownAnimation = (DropDownAnimation)LayoutReader.GetPropertyValue(reader, "Animation", typeof(DropDownAnimation), DropDownAnimation.Default);
			PropertyValue propertyValue = reader["Commands"];
			MxCommandCollection uiCommandCollection = new MxCommandCollection(this);
			LayoutReader mordexLayoutReader = null;
			IList<MxCommand> list = new List<MxCommand>();
			foreach (MxCommand item in _commands)
			{
				list.Add(item);
			}
			if (propertyValue != null)
			{
				mordexLayoutReader = propertyValue.GetLayoutReader();
				if (mordexLayoutReader != null)
				{
					int commandIndex = 0;
					PropertyValue namedCommandValue = mordexLayoutReader["Command" + commandIndex];
					while (namedCommandValue != null)
					{
						PropertyValue commandValue = mordexLayoutReader[commandIndex];
						LayoutReader layoutReader = commandValue.GetLayoutReader();
						string text = (string)LayoutReader.GetPropertyValue(layoutReader, "Name", typeof(string), "");
						MxCommand uiCommand = _commands.FindByName(text);
						if (uiCommand == null)
						{
							uiCommand = new MxCommand(commandValue.Name);
						}
						else
						{
							list.Remove(uiCommand);
						}
						commandValue.DeserializeObject(uiCommand);
						if (CommandManager != null)
						{
							uiCommand.ApplyLayoutState(namedCommandValue, CommandManager);
						}
						commandIndex++;
						namedCommandValue = mordexLayoutReader["Command" + commandIndex];
						uiCommandCollection.AddKeepingPositions(uiCommand);
					}
					_commands.DetachAllAndReset();
					_commands = uiCommandCollection;
					_commands.DeserializeOriginalCommands(propertyValue);
					_commands.DeserializeMergedCommands(propertyValue);
				}
				_commands.DeserializeMergedCommands(propertyValue);
			}
			else
			{
				_commands.DetachAllAndReset();
				_commands = uiCommandCollection;
			}
			foreach (MxCommand staleCommand in list)
			{
				staleCommand.Dispose();
			}
			Key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), "");
			_popupSettings = (PopupSettings)LayoutReader.GetPropertyValue(reader, "PopupSettings", typeof(PopupSettings), null);
			if (_popupSettings != null)
			{
				_popupSettings._owner = this;
			}
			_view = (View)LayoutReader.GetPropertyValue(reader, "View", typeof(View), View.Default);
			_showShadowUnderMenus = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ShowShadowUnderMenus", typeof(InheritableBoolean), InheritableBoolean.Default);
			_showToolTips = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ShowToolTips", typeof(InheritableBoolean), InheritableBoolean.Default);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_dropDownAnimation != DropDownAnimation.Default)
			{
				info.AddValue("Animation", _dropDownAnimation);
			}
			if (_commands != null && _commands.Count > 0)
			{
				info.AddValue("Commands", _commands);
			}
			if (_key.Length > 0)
			{
				info.AddValue("Key", _key);
			}
			if (_popupSettings != null && _popupSettings.ShouldSerialize())
			{
				info.AddValue("PopupSettings", _popupSettings);
			}
			if (_showShadowUnderMenus != InheritableBoolean.Default)
			{
				info.AddValue("ShowShadowUnderMenus", _showShadowUnderMenus);
			}
			if (_showToolTips != InheritableBoolean.Default)
			{
				info.AddValue("ShowToolTips", _showToolTips);
			}
			if (_view != View.Default)
			{
				info.AddValue("View", _view);
			}
		}

		ImageList IAccessibleCommandBarsElement.GetImageList(bool smallIcons, MxCommand cmd)
		{
			if (smallIcons)
			{
				return GetImageListForView(View.SmallIcons, cmd);
			}
			return GetImageListForView(View.LargeIcons, cmd);
		}

		ImageList ICommandMenuOwner.GetImageList(MxCommand cmd)
		{
			return GetImageListForView(GetEffectiveView(), cmd);
		}

		private void InitializeMembers()
		{
			_components = new Container();
			_commands = new MxCommandCollection(this);
			_showShadowUnderMenus = InheritableBoolean.Default;
		}

		internal bool IsNotInheritedReadOnly()
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

		internal bool IsRightButtonDown()
		{
			return _isRightButtonDown;
		}

		internal void SetRightButtonDown(bool isDown)
		{
			_isRightButtonDown = isDown;
		}

		internal bool ShouldShowInfrequentItems()
		{
			if (_commandManager != null && CommandManager.AlwaysShowFullMenus)
			{
				return true;
			}
			if (CommandManager != null && CommandManager._isCustomizing)
			{
				return true;
			}
			return _shouldShowInfrequentItems;
		}

		internal void SetShowInfrequentItems(bool shouldShow)
		{
			if (_shouldShowInfrequentItems != shouldShow)
			{
				_shouldShowInfrequentItems = shouldShow;
			}
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

		protected override void Dispose(bool disposing)
		{
			if (CommandManager != null && CommandManager.ContextMenus.Contains(this))
			{
				CommandManager.ContextMenus.Remove(this);
			}
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			_commands.DetachAll();
			for (int i = 0; i < _commands.Count; i++)
			{
				MxCommand uiCommand = _commands[i];
				uiCommand.ForceDispose();
			}
			_commands._originalCommands = null;
			base.Dispose(disposing);
			base.Events.Dispose();
		}

		internal bool IsInitializing()
		{
			return _isInitializing;
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing && (_commandManager == null || !_commandManager.IsInitializing()))
			{
				_isInitializing = false;
			}
			InvalidateMenu();
		}

		internal void CompleteInitialization()
		{
			_isInitializing = false;
			BindBaseCommands();
			InvalidateMenu();
		}

		internal bool ShouldShowToolTips()
		{
			if (_showToolTips == InheritableBoolean.Default)
			{
				if (CommandManager != null)
				{
					return CommandManager.ShowToolTipOnMenus;
				}
			}
			else if (_showToolTips == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal void BindBaseCommands()
		{
			if (_isInitializing)
			{
				return;
			}
			for (int i = 0; i < _commands.Count; i++)
			{
				MxCommand uiCommand = _commands[i];
				if (uiCommand.BaseCommand != null)
				{
					if (uiCommand.BaseCommand.CommandManager == null && CommandManager != null)
					{
						CommandManager.Commands.Add(uiCommand.BaseCommand);
					}
				}
				else if (uiCommand.CommandType == CommandType.Separator)
				{
					if (CommandManager != null)
					{
						uiCommand.SetBaseCommand(CommandManager.GetSeparatorCommand());
					}
				}
				else
				{
					if (uiCommand.Key.Length <= 0 || CommandManager == null)
					{
						continue;
					}
					try
					{
						MxCommand baseCommand = CommandManager.Commands[uiCommand.Key];
						if (baseCommand != null)
						{
							uiCommand.SetBaseCommand(baseCommand);
						}
					}
					catch
					{
					}
				}
			}
			if (_commandManager == null)
			{
				return;
			}
			foreach (MxCommand item in _commands)
			{
				if (item.BaseCommand == null)
				{
					throw new InvalidOperationException("The commands in the CommandCollection must have a BaseCommand located in the UICommandManager");
				}
			}
		}

		internal ImageList GetImageListForView(View view, MxCommand uiCommand)
		{
			if (view == View.SmallIcons)
			{
				if (_imageList != null)
				{
					return _imageList;
				}
				if (_commandManager != null)
				{
					if (uiCommand != null && uiCommand.IsMergedCommand && _commandManager._mergedSmallImageList != null)
					{
						return _commandManager._mergedSmallImageList;
					}
					return _commandManager.ImageList;
				}
			}
			else
			{
				if (_largeImageList != null)
				{
					return _largeImageList;
				}
				if (_commandManager != null)
				{
					if (uiCommand != null && uiCommand.IsMergedCommand && _commandManager._mergedLargeImageList != null)
					{
						return _commandManager._mergedLargeImageList;
					}
					return _commandManager.LargeImageList;
				}
			}
			return null;
		}

		internal View GetEffectiveView()
		{
			if (_view == View.Default)
			{
				if (_commandManager != null)
				{
					return _commandManager.View;
				}
				return View.SmallIcons;
			}
			return View;
		}

		internal bool ShouldShowShadow()
		{
			if (_showShadowUnderMenus == InheritableBoolean.True)
			{
				return true;
			}
			if (_showShadowUnderMenus == InheritableBoolean.False)
			{
				return false;
			}
			if (CommandManager != null)
			{
				return CommandManager.ShowShadowUnderMenus;
			}
			return true;
		}

		internal DropDownAnimation GetEffectiveDropDownAnimation()
		{
			if (_dropDownAnimation == DropDownAnimation.Default)
			{
				if (_commandManager != null)
				{
					if (_commandManager.MenuAnimation == DropDownAnimation.Default)
					{
						return DropDownAnimation.System;
					}
					return _commandManager.MenuAnimation;
				}
				return DropDownAnimation.System;
			}
			return _dropDownAnimation;
		}

		internal CommandControl GetPopupControl()
		{
			return _popupControl;
		}

		internal void SetPopupControl(CommandControl uiCommandControl)
		{
			if (_popupControl != uiCommandControl)
			{
				if (_popupControl != null)
				{
					_popupControl.Dispose();
					OnClosed(EventArgs.Empty);
				}
				_popupControl = uiCommandControl;
			}
		}

		internal bool TryClose(CommandCloseReason closeReason)
		{
			if (_popupControl != null)
			{
				if (_popupControl._parentItem != null && _popupControl._parentItem.GetCommand() != null)
				{
					MxCommand uiCommand = _popupControl._parentItem.GetCommand();
					if (uiCommand.ConfirmClose(closeReason))
					{
						SetPopupControl(null);
						return true;
					}
					return false;
				}
				CommandClosingEventArgs e = new CommandClosingEventArgs(null, closeReason);
				OnClosing(e);
				if (!e.Cancel)
				{
					SetPopupControl(null);
				}
				return !e.Cancel;
			}
			return true;
		}

		public MxCommand GetDefaultCommand()
		{
			foreach (MxCommand command in Commands)
			{
				if (command.IsDefaultItem())
				{
					return command;
				}
			}
			return null;
		}

		public MxCommand GetCommandAt(int x, int y)
		{
			return HitTestCommandItem(x, y)?.GetCommand();
		}

		public MxCommand GetCommandAt(Point pt)
		{
			return GetCommandAt(pt.X, pt.Y);
		}

		public MxCommand GetCommandAt()
		{
			Point position = Cursor.Position;
			return GetCommandAt(position.X, position.Y);
		}

		internal CommandLayoutState HitTestCommandItem(int screenX, int screenY)
		{
			if (_popupControl != null)
			{
				CommandControl childMenu = _popupControl;
				CommandLayoutState result = null;
				while (childMenu != null && !childMenu.IsDisposed)
				{
					Point p = new Point(screenX, screenY);
					Point point = childMenu.PointToClient(p);
					CommandLayoutState uiCommandLayoutState = childMenu.HitTestItem(point.X, point.Y);
					if (uiCommandLayoutState != null)
					{
						result = uiCommandLayoutState;
					}
					childMenu = childMenu.ChildMenu;
				}
				return result;
			}
			return null;
		}

		public Rectangle GetCommandBounds(MxCommand command)
		{
			if (_popupControl != null)
			{
				return _popupControl.GetCommandScreenBounds(command);
			}
			return Rectangle.Empty;
		}

		internal PopupSettings GetEffectivePopupSettings()
		{
			PopupSettings uiPopupSettings = null;
			if (_commandManager != null)
			{
				uiPopupSettings = CommandManager.CreatePopupSettings();
				uiPopupSettings = uiPopupSettings.MergeWith(PopupSettings, null);
			}
			else
			{
				uiPopupSettings = new PopupSettings();
				CommandBarsColorSet commandBarsAppearanceState = new CommandBarsColorSet();
				commandBarsAppearanceState.ApplyVisualStyle();
				uiPopupSettings.StateStyles.FormatStyle.BackColor = commandBarsAppearanceState._menuBackColor;
				uiPopupSettings.ImageAreaFormatStyle.BackColor = commandBarsAppearanceState._imageAreaBackColor;
				uiPopupSettings.ImageAreaFormatStyle.BackColorGradient = commandBarsAppearanceState._imageAreaGradientColor;
				uiPopupSettings.InfrequentItemsFormatStyle.BackColor = commandBarsAppearanceState._infrequentAreaBackColor;
				uiPopupSettings.InfrequentItemsFormatStyle.BackColorGradient = commandBarsAppearanceState._infrequentAreaGradientColor;
				uiPopupSettings = uiPopupSettings.MergeWith(PopupSettings, BuildStateStyles(commandBarsAppearanceState));
			}
			return uiPopupSettings;
		}

		internal StateStyles BuildStateStyles(CommandBarsColorSet commandBarsAppearanceState)
		{
			StateStyles uiStateStyles = new StateStyles();
			uiStateStyles.DisabledFormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.GrayText);
			uiStateStyles.HotFormatStyle.BackColor = commandBarsAppearanceState._hotBackColor;
			uiStateStyles.PressedFormatStyle.BackColor = commandBarsAppearanceState._pressedBackColor;
			uiStateStyles.SelectedFormatStyle.BackColor = commandBarsAppearanceState._selectedBackColor;
			return uiStateStyles;
		}

		public void Show()
		{
			ShowPopup(null, Cursor.Position, 0);
		}

		public void Show(Control control)
		{
			ShowPopup(control, Cursor.Position, 0);
		}

		internal void ShowPopup(Control control, Point point, int anchorHeight)
		{
			_sourceControl = control;
			if (_commands.Count == 0)
			{
				return;
			}
			PopupSettings baseSettings = GetEffectivePopupSettings();
			CommandControl uiCommandControl = null;
			if (_commands.Count == 1)
			{
				MxCommand uiCommand = _commands[0];
				if (uiCommand.CommandType == CommandType.ColorPickerCommand || uiCommand.CommandType == CommandType.ControlPopup)
				{
					CommandLayoutState parentItem = new CommandLayoutState(uiCommand, "");
					uiCommandControl = new CommandControl(uiCommand.Commands, parentItem, this, baseSettings, holdMessages: true);
				}
			}
			if (uiCommandControl == null)
			{
				uiCommandControl = new CommandControl(_commands, null, this, baseSettings, holdMessages: true);
			}
			if (_ownerWidth != -1)
			{
				uiCommandControl._anchorSize = new Size(_ownerWidth, 0);
			}
			uiCommandControl._upwardOffset = _ownerHeight;
			uiCommandControl._anchorHeight = anchorHeight;
			if (_isRightAligned)
			{
				uiCommandControl._alignRight = true;
			}
			SetPopupControl(uiCommandControl);
			if (Form.ActiveForm == null)
			{
				uiCommandControl._activateOnShow = true;
			}
			uiCommandControl.ShowAt(point);
		}

		public void Show(Control control, Point pos)
		{
			pos = control.PointToScreen(pos);
			ShowPopup(control, pos, 0);
		}

		public void Show(Point pos)
		{
			ShowPopup(null, pos, 0);
		}

		public void Show(Control control, int x, int y)
		{
			Point p = new Point(x, y);
			p = control.PointToScreen(p);
			Show(control, p);
		}

		public void Close()
		{
			TryClose(CommandCloseReason.CloseCalled);
		}

		internal bool ExecuteShortcutCommand(Shortcut shortcut, MxCommandCollection uiCommandCollection, Control control, Message msg)
		{
			foreach (MxCommand item in uiCommandCollection)
			{
				if (!item.IsEnabled || !item.IsEffectivelyVisible())
				{
					continue;
				}
				if (item.Shortcut == shortcut)
				{
					if (control == null || control.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageNotNeeded)
					{
						_isProcessingShortcut = true;
						item.PerformClick();
						_isProcessingShortcut = false;
					}
					else if (control != null)
					{
						NativeMethods.NativeMsg c = new NativeMethods.NativeMsg
						{
							_hwnd = msg.HWnd,
							_lParam = msg.LParam,
							_wParam = msg.WParam,
							_message = msg.Msg
						};
						NativeMethods.TranslateMessage(ref c);
						NativeMethods.DispatchMessage(ref c);
					}
					return true;
				}
				if (item.Commands.Count > 0 && ExecuteShortcutCommand(shortcut, item.Commands, control, msg))
				{
					return true;
				}
			}
			return false;
		}

		internal void InvalidateMenu()
		{
			if (GetPopupControl() != null)
			{
				GetPopupControl().InvalidateLayout();
			}
		}

		internal void RaisePopup()
		{
			OnPopup(EventArgs.Empty);
		}
	}
}
