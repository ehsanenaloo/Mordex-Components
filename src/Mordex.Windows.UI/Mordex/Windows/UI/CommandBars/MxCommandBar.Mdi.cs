using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		private MxCommand BuildMdiSystemMenu()
		{
			_mdiSystemMenu = new MxCommand();
			if (CommandManager.GetForm().ActiveMdiChild != null)
			{
				_mdiSystemMenu.Icon = new Icon(CommandManager.GetForm().ActiveMdiChild.Icon, MdiIconSize, MdiIconSize);
			}
			else
			{
				_mdiSystemMenu.Icon = new Icon(CommandManager.GetForm().Icon, MdiIconSize, MdiIconSize);
			}
			_mdiSystemMenu.CommandStyle = CommandStyle.Image;
			string text = _commandManager.BuiltInTexts[CommandsBuiltInText.Restore];
			MxCommand uiCommand = new MxCommand("Restore", text);
			uiCommand.Image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Restore());
			MxCommand restoreCommand = new MxCommand(uiCommand);
			restoreCommand.Click += OnMdiSystemMenuClick;
			_mdiSystemMenu.Commands.Add(restoreCommand);
			string moveText = _commandManager.BuiltInTexts[CommandsBuiltInText.Move];
			uiCommand = new MxCommand("Move", moveText);
			uiCommand.Enabled = InheritableBoolean.False;
			MxCommand command = new MxCommand(uiCommand);
			_mdiSystemMenu.Commands.Add(command);
			string sizeText = _commandManager.BuiltInTexts[CommandsBuiltInText.Size];
			uiCommand = new MxCommand("Size", sizeText);
			uiCommand.Enabled = InheritableBoolean.False;
			MxCommand sizeCommand = new MxCommand(uiCommand);
			_mdiSystemMenu.Commands.Add(sizeCommand);
			string minimizeText = _commandManager.BuiltInTexts[CommandsBuiltInText.Minimize];
			uiCommand = new MxCommand("Minimize", minimizeText);
			uiCommand.Image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Minimize());
			MxCommand minimizeCommand = new MxCommand(uiCommand);
			minimizeCommand.Click += OnMdiSystemMenuClick;
			_mdiSystemMenu.Commands.Add(minimizeCommand);
			string maximizeText = _commandManager.BuiltInTexts[CommandsBuiltInText.Maximize];
			uiCommand = new MxCommand("Maximize", maximizeText);
			uiCommand.Enabled = InheritableBoolean.False;
			MxCommand maximizeCommand = new MxCommand(uiCommand);
			_mdiSystemMenu.Commands.Add(maximizeCommand);
			_mdiSystemMenu.Commands.Add(new MxCommand("Separator", "", CommandType.Separator));
			string closeText = _commandManager.BuiltInTexts[CommandsBuiltInText.Close];
			uiCommand = new MxCommand("Close", closeText, -1, Shortcut.CtrlF4);
			uiCommand.Image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Close());
			MxCommand closeCommand = new MxCommand(uiCommand);
			closeCommand.Click += OnMdiSystemMenuClick;
			_mdiSystemMenu.Commands.Add(closeCommand);
			return _mdiSystemMenu;
		}

		private void OnMdiSystemMenuClick(object sender, CommandEventArgs eventArgs)
		{
			_isMenuActive = false;
			SetChildMenu(null);
			string key;
			if ((key = eventArgs.Command.Key) == null)
			{
				return;
			}
			if (!(key == "Restore"))
			{
				if (!(key == "Minimize"))
				{
					if (key == "Close")
					{
						CloseMdiChild();
					}
				}
				else
				{
					MinimizeMdiChild();
				}
			}
			else
			{
				RestoreMdiChild();
			}
		}

		internal Form GetParentForm()
		{
			return _parentForm;
		}

		internal void SetParentForm(Form form)
		{
			if (_parentForm != form)
			{
				if (_parentForm != null)
				{
					_parentForm.MdiChildActivate -= OnFormMdiChildActivate;
				}
				_parentForm = form;
				if (_parentForm != null)
				{
					_parentForm.MdiChildActivate += OnFormMdiChildActivate;
				}
			}
		}

		internal Form GetActiveMdiChild()
		{
			return _activeMdiChild;
		}

		private void AttachActiveMdiChild()
		{
			if (_activeMdiChild != null)
			{
				_activeMdiChild.SizeChanged -= OnFormSizeChanged;
			}
			if (_parentForm != null && _commandBarType == CommandBarType.Menu)
			{
				_activeMdiChild = _parentForm.ActiveMdiChild;
				if (_activeMdiChild != null)
				{
					_activeMdiChild.SizeChanged += OnFormSizeChanged;
					SetMdiChildMaximized(_activeMdiChild.WindowState == FormWindowState.Maximized);
				}
				else
				{
					SetMdiChildMaximized(false);
				}
				InvalidateLayout();
			}
			else
			{
				SetMdiChildMaximized(false);
			}
		}

		private void OnFormSizeChanged(object sender, EventArgs eventArgs)
		{
			if (_activeMdiChild != null && _commandBarType == CommandBarType.Menu)
			{
				SetMdiChildMaximized(_activeMdiChild.WindowState == FormWindowState.Maximized);
			}
			else
			{
				SetMdiChildMaximized(false);
			}
		}

		internal void CloseMdiChild()
		{
			if (_activeMdiChild != null)
			{
				_activeMdiChild.Close();
			}
		}

		internal void RestoreMdiChild()
		{
			if (_activeMdiChild != null)
			{
				_activeMdiChild.SuspendLayout();
				_activeMdiChild.WindowState = FormWindowState.Normal;
				_activeMdiChild.Invalidate(invalidateChildren: true);
				_activeMdiChild.ResumeLayout();
			}
		}

		internal void MinimizeMdiChild()
		{
			if (_activeMdiChild != null)
			{
				_activeMdiChild.WindowState = FormWindowState.Minimized;
			}
		}

		internal bool IsMdiChildMaximized()
		{
			if (_activeMdiChild != null)
			{
				return _activeMdiChild.WindowState == FormWindowState.Maximized;
			}
			return _isMdiChildMaximized;
		}

		internal void SetMdiChildMaximized(bool isMaximized)
		{
			if (_isMdiChildMaximized != isMaximized)
			{
				_isMdiChildMaximized = isMaximized;
				if (!isMaximized)
				{
					_mdiButtons = null;
					_mdiSystemMenu = null;
				}
				else
				{
					Wrappable = InheritableBoolean.True;
				}
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCommands()
		{
			Commands.UpdateOwner();
		}

		internal void RestoreUnmergedCommands()
		{
			Controls.Clear();
			if (_commands._mergedCommands != null)
			{
				MxCommandCollection uiCommandCollection = _commands.GetUnmergedCommands();
				if (uiCommandCollection != null)
				{
					_commands.DetachAllAndReset();
					_commands = uiCommandCollection;
				}
			}
		}

		private void OnFormMdiChildActivate(object sender, EventArgs eventArgs)
		{
			if (GetParentForm() != null && GetParentForm().IsMdiContainer)
			{
				AttachActiveMdiChild();
			}
		}
	}
}
