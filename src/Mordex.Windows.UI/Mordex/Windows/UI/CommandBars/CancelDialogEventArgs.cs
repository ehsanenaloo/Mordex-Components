using System;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	public class CancelDialogEventArgs : EventArgs
	{
		internal MxCommandManager _commandManager;

		private bool _cancel;

		private DialogType _dialogType;

		private Form _dialog;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		public DialogType DialogType => _dialogType;

		public Form Dialog
		{
			get
			{
				if (_dialog == null)
				{
					switch (_dialogType)
					{
					case DialogType.Customize:
						_dialog = new CustomizeForm();
						break;
					case DialogType.Keyboard:
						_dialog = new CustomizeKeyboardForm();
						break;
					case DialogType.NewCommandBar:
						_dialog = new NameCommandBarForm();
						break;
					case DialogType.RenameCommandBar:
						_dialog = new NameCommandBarForm();
						_dialog.Text = "Rename Toolbar";
						break;
					}
				}
				if (_dialog is CustomizeForm)
				{
					((CustomizeForm)_dialog).AttachCommandManager(_commandManager);
				}
				else if (_dialog is CustomizeKeyboardForm)
				{
					((CustomizeKeyboardForm)_dialog)._commandManager = _commandManager;
				}
				else if (_dialog is NameCommandBarForm)
				{
					((NameCommandBarForm)_dialog)._commandManager = _commandManager;
				}
				return _dialog;
			}
			set
			{
				_dialog = value;
			}
		}

		internal CancelDialogEventArgs(MxCommandManager commandManager, DialogType dialogType)
		{
			_commandManager = commandManager;
			_dialogType = dialogType;
		}
	}
}
