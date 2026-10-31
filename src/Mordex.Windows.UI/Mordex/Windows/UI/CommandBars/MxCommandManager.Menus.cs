using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandManager
	{
		[DefaultValue(null)]
		[TypeConverter(typeof(ContextMenuConverter))]
		[Editor(typeof(ContextMenuEditor), typeof(UITypeEditor))]
		public MxContextMenu GetContextMenu(Component component)
		{
			if (_contextMenusByComponent != null && _contextMenusByComponent.Contains(component))
			{
				return (MxContextMenu)_contextMenusByComponent[component];
			}
			return null;
		}

		internal ContextMenuStrip GetOrCreateContextMenu()
		{
			if (_contextMenu == null)
			{
				_contextMenu = new ContextMenuStrip();
			}
			return _contextMenu;
		}

		[TypeConverter(typeof(ContextMenuConverter))]
		[Editor(typeof(ContextMenuEditor), typeof(UITypeEditor))]
		public void SetContextMenu(Component component, MxContextMenu ctMenu)
		{
			if (component == null)
			{
				return;
			}
			if (_contextMenusByComponent == null)
			{
				_contextMenusByComponent = new Hashtable();
			}
			if (_contextMenusByComponent.Contains(component))
			{
				if (component is Control && ((Control)component).ContextMenuStrip == GetOrCreateContextMenu())
				{
					((Control)component).ContextMenuStrip = null;
				}
				_contextMenusByComponent.Remove(component);
				SetContextMenu(component, null);
			}
			try
			{
				if (ctMenu != null)
				{
					_contextMenusByComponent.Add(component, ctMenu);
					if (component is Control)
					{
						if (component is TextBox)
						{
							((Control)component).ContextMenuStrip = GetOrCreateContextMenu();
						}
						((Control)component).MouseUp += OnControlMouseUp;
						((Control)component).MouseDown += OnControlMouseDown;
						((Control)component).KeyUp += OnControlKeyUp;
					}
					else if (component is NotifyIcon)
					{
						((NotifyIcon)component).MouseUp += OnControlMouseUp;
					}
				}
				else if (component is Control)
				{
					((Control)component).MouseUp -= OnControlMouseUp;
					((Control)component).MouseDown -= OnControlMouseDown;
					((Control)component).KeyUp -= OnControlKeyUp;
				}
				else if (component is NotifyIcon)
				{
					((NotifyIcon)component).MouseUp -= OnControlMouseUp;
				}
			}
			catch (ArgumentException)
			{
				// The component already has a menu entry; it keeps the one it had.
			}
		}

		private void OnControlMouseDown(object sender, MouseEventArgs eventArgs)
		{
			if (_contextMenusByComponent != null && _contextMenusByComponent.Count > 0 && sender is Control && eventArgs.Button == MouseButtons.Right && _contextMenusByComponent != null && _contextMenusByComponent.ContainsKey(sender as Component))
			{
				((MxContextMenu)_contextMenusByComponent[sender as Component])?.SetRightButtonDown(true);
			}
		}

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (_contextMenusByComponent == null || _contextMenusByComponent.Count <= 0)
			{
				return;
			}
			if (sender is Control)
			{
				if (eventArgs.Button != MouseButtons.Right || _contextMenusByComponent == null || !_contextMenusByComponent.ContainsKey(sender as Component))
				{
					return;
				}
				MxContextMenu uiContextMenu = (MxContextMenu)_contextMenusByComponent[sender as Component];
				if (uiContextMenu == null || !uiContextMenu.IsRightButtonDown())
				{
					return;
				}
				uiContextMenu.SetRightButtonDown(false);
				try
				{
					Control control = sender as Control;
					ContextMenuStrip contextMenu = null;
					if (control != null)
					{
						contextMenu = control.ContextMenuStrip;
						control.ContextMenuStrip = null;
					}
					uiContextMenu.Show(sender as Control);
					if (control != null)
					{
						control.ContextMenuStrip = contextMenu;
					}
					return;
				}
				catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException || ex is Win32Exception)
				{
					// The control was closed before its menu could open.
					return;
				}
			}
			if (sender is Component)
			{
				TryShowContextMenu(sender as Component);
			}
		}

		private void OnControlKeyUp(object sender, KeyEventArgs eventArgs)
		{
			if (eventArgs.KeyData != Keys.Apps || _contextMenusByComponent == null || !_contextMenusByComponent.ContainsKey(sender))
			{
				return;
			}
			MxContextMenu uiContextMenu = (MxContextMenu)_contextMenusByComponent[sender];
			if (uiContextMenu == null)
			{
				return;
			}
			Point empty = Point.Empty;
			if (sender is TextBoxBase)
			{
				NativeMethods.NativePoint k = default(NativeMethods.NativePoint);
				if (NativeMethods.GetCaretPos(ref k))
				{
					empty.X = k._x;
					empty.Y = k._y;
				}
			}
			Control control = sender as Control;
			if (empty.IsEmpty)
			{
				empty.X = control.Width / 2;
				empty.Y = control.Height / 2;
			}
			uiContextMenu.Show(control, empty);
		}

		private bool TryShowEditContextMenu(Control control)
		{
			if (GetEditTargetControls() != null)
			{
				foreach (Control item in GetEditTargetControls())
				{
					IntPtr handle = item.Handle;
					if (item is ITextBoxOwner)
					{
						ITextBoxOwner textBoxOwner = (ITextBoxOwner)item;
						if (textBoxOwner.TextBoxControl == null)
						{
							continue;
						}
						handle = textBoxOwner.TextBoxControl.Handle;
					}
					if (handle == control.Handle)
					{
						EditContextMenu.AttachToControl(control);
						RenderingHelper.TryFocusControl(control);
						EditContextMenu.Show(control);
						return true;
					}
				}
			}
			return false;
		}

		private bool TryShowContextMenu(Component key)
		{
			if (_contextMenusByComponent != null && _contextMenusByComponent.ContainsKey(key))
			{
				MxContextMenu uiContextMenu = (MxContextMenu)_contextMenusByComponent[key];
				if (uiContextMenu != null)
				{
					if (key is Control)
					{
						uiContextMenu.Show(key as Control);
					}
					else
					{
						uiContextMenu.Show(null);
					}
					return true;
				}
			}
			return false;
		}

		internal void ShowKeyboardDialog()
		{
			CancelDialogEventArgs e = new CancelDialogEventArgs(this, DialogType.Keyboard);
			OnShowingCustomizeDialog(e);
			if (!e.Cancel)
			{
				e.Dialog.ShowDialog();
				DialogEventArgs shownArgs = new DialogEventArgs(e.Dialog);
				OnCustomizeDialogShown(shownArgs);
			}
		}

		public void ShowCustomizeDialog()
		{
			CancelDialogEventArgs e = new CancelDialogEventArgs(this, DialogType.Customize);
			OnShowingCustomizeDialog(e);
			if (e.Cancel)
			{
				return;
			}
			_isCustomizing = true;
			RefreshRebars();
			if (e.Dialog is CustomizeForm)
			{
				CustomizeForm customizeForm = (CustomizeForm)e.Dialog;
				RefreshRebarRows();
				customizeForm.Show();
				customizeForm.RunMessageLoop();
				CollapseRebarRows();
				foreach (MxCommandBar commandBar in CommandBars)
				{
					commandBar.SetWrappableSetting(commandBar._wrappable);
					commandBar.SetCustomizeSelectedCommand(null);
					commandBar.SetDropIndex(-1);
					commandBar.SetChildMenu(null);
				}
			}
			else
			{
				e.Dialog.ShowDialog();
			}
			_isCustomizing = false;
			RefreshRebars();
			DialogEventArgs shownArgs = new DialogEventArgs(e.Dialog);
			OnCustomizeDialogShown(shownArgs);
		}

		internal MxContextMenu GetToolbarsMenu()
		{
			if (_toolbarsMenu == null || _isToolbarsMenuDirty)
			{
				BuildToolbarsMenu();
				_isToolbarsMenuDirty = false;
			}
			return _toolbarsMenu;
		}

		private void OnUIEditContextMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			TextBoxBase textBoxBase = EditContextMenu.GetTargetTextBox();
			string key;
			if (textBoxBase == null || (key = eventArgs.Command.Key) == null)
			{
				return;
			}
			if (!(key == "cmdUndo"))
			{
				if (!(key == "cmdCut"))
				{
					if (!(key == "cmdCopy"))
					{
						if (!(key == "cmdPaste"))
						{
							if (key == "cmdSelectAll")
							{
								textBoxBase.SelectAll();
							}
						}
						else
						{
							textBoxBase.Paste();
						}
					}
					else
					{
						textBoxBase.Copy();
					}
				}
				else
				{
					textBoxBase.Cut();
				}
			}
			else
			{
				textBoxBase.Undo();
			}
		}

		private void OnCommandMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			MxCommand uiCommand = GetSelectedCommand();
			string key;
			if (uiCommand == null || (key = eventArgs.Command.Key) == null)
			{
				return;
			}
			if (!(key == "Reset"))
			{
				if (!(key == "Delete"))
				{
					if (!(key == "DefaultStyle"))
					{
						if (!(key == "TextAlways"))
						{
							if (!(key == "ImageText"))
							{
								if (!(key == "BeginGroup"))
								{
									return;
								}
								MxCommand previousCommand = null;
								int index = 0;
								if (GetActiveElement().LayoutItems != null)
								{
									CommandLayoutState uiCommandLayoutState = GetActiveElement().LayoutItems.FindCommandItem(uiCommand);
									if (uiCommandLayoutState != null && uiCommandLayoutState._index > 0)
									{
										index = uiCommandLayoutState._index;
										previousCommand = ((CommandLayoutState)GetActiveElement().LayoutItems.FindItemByIndex(uiCommandLayoutState._index - 1)).GetCommand();
									}
								}
								if (previousCommand != null)
								{
									if (previousCommand.IsSeparator())
									{
										GetActiveElement().Commands.Remove(previousCommand);
										return;
									}
									MxCommand command = new MxCommand(GetSeparatorCommand());
									GetActiveElement().Commands.Insert(index, command);
								}
							}
							else
							{
								uiCommand.CommandStyle = CommandStyle.TextImage;
							}
						}
						else
						{
							uiCommand.CommandStyle = CommandStyle.Text;
						}
					}
					else
					{
						uiCommand.CommandStyle = CommandStyle.Default;
					}
				}
				else
				{
					GetActiveElement().SelectedCommand = null;
					GetActiveElement().Commands.Remove(uiCommand);
				}
			}
			else
			{
				uiCommand.ResetCustomizedProperties();
			}
		}

		private void OnToolbarsMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			if (eventArgs.Command.GetBuiltInId() == "CommandBar")
			{
				if (eventArgs.Command.Tag != null)
				{
					MxCommandBar uiCommandBar = (MxCommandBar)eventArgs.Command.Tag;
					if (uiCommandBar.CanClose())
					{
						uiCommandBar.Visible = !uiCommandBar.Visible;
					}
				}
			}
			else if (eventArgs.Command.GetBuiltInId() == "Customize")
			{
				GetToolbarsMenu().SetPopupControl(null);
				ShowCustomizeDialog();
			}
		}

		internal void ShowCommandMenu(Point point, int anchorHeight)
		{
			if (GetActiveElement() != null)
			{
				MxCommand uiCommand = GetSelectedCommand();
				MxContextMenu uiContextMenu = BuildCommandMenu(GetSelectedCommand());
				uiContextMenu._commandManager = this;
				if (!point.IsEmpty)
				{
					uiContextMenu.ShowPopup(GetActiveElement().Control, point, anchorHeight);
				}
				else
				{
					uiContextMenu.Show(GetSelectedCommand().Control);
				}
				TextBox textBox = uiContextMenu.Commands["Name"].GetTextBox();
				if (uiCommand != null)
				{
					uiCommand.Text = textBox.Text;
				}
				textBox.Dispose();
				textBox = null;
			}
		}

		internal void ShowToolbarsMenu(Control control)
		{
			ShowToolbarsMenu(control, Point.Empty);
		}

		internal void ShowToolbarsMenu(Control control, Point pos)
		{
			MxContextMenu uiContextMenu = GetToolbarsMenu();
			foreach (MxCommand command in uiContextMenu.Commands)
			{
				if (command.GetBuiltInId() == "CommandBar" && command.Tag != null)
				{
					MxCommandBar uiCommandBar = (MxCommandBar)command.Tag;
					command.IsChecked = uiCommandBar.Visible;
				}
			}
			uiContextMenu._commandManager = this;
			if (!pos.IsEmpty)
			{
				uiContextMenu.Show(control, pos);
			}
			else
			{
				uiContextMenu.Show(control);
			}
		}

		public void SaveDefaults()
		{
			foreach (MxCommand command in Commands)
			{
				command.Commands.SetOriginalCommands();
			}
			foreach (MxCommandBar commandBar in CommandBars)
			{
				commandBar.Commands.SetOriginalCommands();
			}
			foreach (MxContextMenu contextMenu in ContextMenus)
			{
				contextMenu.Commands.SetOriginalCommands();
			}
		}

		internal MxCommand GetSeparatorCommand()
		{
			if (_separatorCommand == null)
			{
				_separatorCommand = new MxCommand("Separator", "", CommandType.Separator);
				_separatorCommand._commandManager = this;
			}
			return _separatorCommand;
		}

		private MxContextMenu BuildCommandMenu(MxCommand uiCommand)
		{
			MxContextMenu uiContextMenu = new MxContextMenu();
			uiContextMenu.CommandClick += OnCommandMenuCommandClick;
			string text = BuiltInTexts[CommandsBuiltInText.Reset];
			MxCommand baseCommand = new MxCommand("Reset", text);
			MxCommand command = new MxCommand(baseCommand);
			uiContextMenu.Commands.Add(command);
			text = BuiltInTexts[CommandsBuiltInText.Delete];
			baseCommand = new MxCommand("Delete", text);
			command = new MxCommand(baseCommand);
			uiContextMenu.Commands.Add(command);
			command = new MxCommand("Separator1", "", CommandType.Separator);
			uiContextMenu.Commands.Add(command);
			text = BuiltInTexts[CommandsBuiltInText.Name];
			baseCommand = new MxCommand("Name", text, CommandType.TextBoxCommand);
			baseCommand._commandManager = this;
			command = new MxCommand(baseCommand);
			uiContextMenu.Commands.Add(command);
			baseCommand.ControlValue = uiCommand.GetEffectiveText();
			command = new MxCommand("Separator2", "", CommandType.Separator);
			uiContextMenu.Commands.Add(command);
			text = BuiltInTexts[CommandsBuiltInText.DefaultStyle];
			baseCommand = new MxCommand("DefaultStyle", text);
			if (uiCommand.GetEffectiveCommandStyle() == CommandStyle.Default)
			{
				baseCommand.Checked = InheritableBoolean.True;
			}
			command = new MxCommand(baseCommand);
			uiContextMenu.Commands.Add(command);
			text = BuiltInTexts[CommandsBuiltInText.TextAlways];
			baseCommand = new MxCommand("TextAlways", text);
			if (uiCommand.GetEffectiveCommandStyle() == CommandStyle.Text)
			{
				baseCommand.Checked = InheritableBoolean.True;
			}
			command = new MxCommand(baseCommand);
			uiContextMenu.Commands.Add(command);
			text = BuiltInTexts[CommandsBuiltInText.ImageAndText];
			baseCommand = new MxCommand("ImageText", text);
			if (uiCommand.GetEffectiveCommandStyle() == CommandStyle.TextImage)
			{
				baseCommand.Checked = InheritableBoolean.True;
			}
			command = new MxCommand(baseCommand);
			uiContextMenu.Commands.Add(command);
			command = new MxCommand("Separator3", "", CommandType.Separator);
			uiContextMenu.Commands.Add(command);
			text = BuiltInTexts[CommandsBuiltInText.BeginGroup];
			baseCommand = new MxCommand("BeginGroup", text);
			if (GetActiveElement().LayoutItems != null)
			{
				CommandLayoutState uiCommandLayoutState = GetActiveElement().LayoutItems.FindCommandItem(uiCommand);
				if (uiCommandLayoutState != null)
				{
					if (uiCommandLayoutState._index > 0)
					{
						CommandLayoutState previousItem = (CommandLayoutState)GetActiveElement().LayoutItems.FindItemByIndex(uiCommandLayoutState._index - 1);
						if (previousItem != null && previousItem.GetCommand() != null && previousItem.GetCommand().IsSeparator())
						{
							baseCommand.Checked = InheritableBoolean.True;
						}
					}
					else
					{
						baseCommand.Enabled = InheritableBoolean.False;
					}
				}
			}
			command = new MxCommand(baseCommand);
			uiContextMenu.Commands.Add(command);
			return uiContextMenu;
		}

		private void BuildToolbarsMenu()
		{
			_toolbarsMenu = new MxContextMenu();
			_toolbarsMenu.CommandClick += OnToolbarsMenuCommandClick;
			foreach (MxCommandBar commandBar in CommandBars)
			{
				if (commandBar.CommandBarType == CommandBarType.ToolBar)
				{
					string text = "";
					if (commandBar._detachedIndex >= 0)
					{
						text = commandBar.GetBarIndex().ToString();
					}
					MxCommand uiCommand = new MxCommand("CommandBar" + text, commandBar.Text);
					uiCommand.SetBuiltInId("CommandBar");
					uiCommand.Tag = commandBar;
					MxCommand command = new MxCommand(uiCommand);
					_toolbarsMenu.Commands.Add(command);
				}
			}
			string customizeText = BuiltInTexts[CommandsBuiltInText.Customize];
			MxCommand customizeCommand = new MxCommand("Customize", customizeText);
			customizeCommand.SetBuiltInId("Customize");
			if (_allowCustomize == InheritableBoolean.False)
			{
				customizeCommand.Enabled = InheritableBoolean.False;
			}
			MxCommand customizeItem = new MxCommand(customizeCommand);
			_toolbarsMenu.Commands.Add(customizeItem);
			_toolbarsMenu._commandManager = this;
		}

		internal void ProcessUICommandBar(MxCommandBar uiCommandBar)
		{
			CancelDialogEventArgs e;
			if (uiCommandBar == null)
			{
				e = new CancelDialogEventArgs(this, DialogType.NewCommandBar);
				if (base.DesignMode)
				{
					string text = "uiCommandBar1";
					int suffix = 1;
					while (!IsCommandBarKeyUnique(null, text))
					{
						suffix++;
						text = "uiCommandBar" + suffix;
					}
					((NameCommandBarForm)e.Dialog).ToolbarName = text;
				}
				OnShowingCustomizeDialog(e);
			}
			else
			{
				e = new CancelDialogEventArgs(this, DialogType.RenameCommandBar);
				((NameCommandBarForm)e.Dialog).SetCommandBar(uiCommandBar);
				OnShowingCustomizeDialog(e);
			}
			if (!e.Cancel)
			{
				if (e.Dialog is NameCommandBarForm)
				{
					((NameCommandBarForm)e.Dialog).SetCommandBar(uiCommandBar);
				}
				e.Dialog.ShowDialog();
			}
		}
	}
}
