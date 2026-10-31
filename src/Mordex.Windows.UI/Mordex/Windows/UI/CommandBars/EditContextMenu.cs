using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.CommandBars
{
	[DesignerSerializer("Mordex.Windows.UI.Design.EditContextMenuSerializer, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	[ProvideProperty("UseMordexEditMenu", typeof(Control))]
	public class EditContextMenu : MxContextMenu, IExtenderProvider
	{
		private string _cutText = "Cu&t";

		private MxCommandManager _owningManager;

		private string _copyText = "&Copy";

		private string _undoText = "&Undo";

		private string _pasteText = "&Paste";

		private string _selectAllText = "Select A&ll";

		private ContextMenuStrip _contextMenu;

		internal ArrayList _editMenuControls;

		[DefaultValue("Cu&t")]
		public string CutText
		{
			get
			{
				return _cutText;
			}
			set
			{
				if (_cutText != value)
				{
					_cutText = value;
					MxCommand uiCommand = Commands.FindByKey("cmdCut");
					if (uiCommand != null)
					{
						uiCommand.Text = value;
					}
				}
			}
		}

		[DefaultValue("&Copy")]
		public string CopyText
		{
			get
			{
				return _copyText;
			}
			set
			{
				if (_copyText != value)
				{
					_copyText = value;
					MxCommand uiCommand = Commands.FindByKey("cmdCopy");
					if (uiCommand != null)
					{
						uiCommand.Text = value;
					}
				}
			}
		}

		[DefaultValue("&Undo")]
		public string UndoText
		{
			get
			{
				return _undoText;
			}
			set
			{
				if (_undoText != value)
				{
					_undoText = value;
					MxCommand uiCommand = Commands.FindByKey("cmdUndo");
					if (uiCommand != null)
					{
						uiCommand.Text = value;
					}
				}
			}
		}

		[DefaultValue("&Paste")]
		public string PasteText
		{
			get
			{
				return _pasteText;
			}
			set
			{
				if (_pasteText != value)
				{
					_pasteText = value;
					MxCommand uiCommand = Commands.FindByKey("cmdPaste");
					if (uiCommand != null)
					{
						uiCommand.Text = value;
					}
				}
			}
		}

		[DefaultValue("Select A&ll")]
		public string SelectAllText
		{
			get
			{
				return _selectAllText;
			}
			set
			{
				if (_selectAllText != value)
				{
					_selectAllText = value;
					MxCommand uiCommand = Commands.FindByKey("cmdSelectAll");
					if (uiCommand != null)
					{
						uiCommand.Text = value;
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new MxCommandCollection Commands => base.Commands;

		internal EditContextMenu(MxCommandManager manager)
		{
			AddEditCommands();
			_owningManager = manager;
		}

		internal MxCommandManager GetOwningManager()
		{
			return _owningManager;
		}

		private void AddEditCommands()
		{
			MxCommand uiCommand = new MxCommand("cmdUndo", UndoText);
			Image image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Undo());
			uiCommand.Image = image;
			Commands.Add(uiCommand);
			Commands.AddSeparator();
			uiCommand = new MxCommand("cmdCut", CutText);
			image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Cut());
			uiCommand.Image = image;
			Commands.Add(uiCommand);
			uiCommand = new MxCommand("cmdCopy", CopyText);
			image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Copy());
			uiCommand.Image = image;
			Commands.Add(uiCommand);
			uiCommand = new MxCommand("cmdPaste", PasteText);
			image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Paste());
			uiCommand.Image = image;
			Commands.Add(uiCommand);
			Commands.AddSeparator();
			uiCommand = new MxCommand("cmdSelectAll", SelectAllText);
			Commands.Add(uiCommand);
		}

		internal void AttachToControl(Control control)
		{
			_sourceControl = control;
			if (System.Drawing.Color.Empty.IsEmpty)
			{
			}
			if (GetTargetTextBox() == null)
			{
				return;
			}
			MxCommand uiCommand = Commands.FindByKey("cmdUndo");
			if (uiCommand != null)
			{
				uiCommand.IsEnabled = GetTargetTextBox().CanUndo && !GetTargetTextBox().ReadOnly;
				if (_undoText == "&Undo")
				{
					uiCommand.Text = _owningManager.BuiltInTexts[CommandsBuiltInText.UndoText];
				}
			}
			uiCommand = Commands.FindByKey("cmdPaste");
			if (uiCommand != null)
			{
				if (GetTargetTextBox().ReadOnly)
				{
					uiCommand.IsEnabled = false;
				}
				else if (ClipboardHasText())
				{
					uiCommand.IsEnabled = true;
				}
				else
				{
					uiCommand.IsEnabled = false;
				}
				if (_pasteText == "&Paste")
				{
					uiCommand.Text = _owningManager.BuiltInTexts[CommandsBuiltInText.PasteText];
				}
			}
			uiCommand = Commands.FindByKey("cmdCopy");
			if (uiCommand != null)
			{
				if (GetTargetTextBox().SelectionLength > 0)
				{
					uiCommand.IsEnabled = true;
				}
				else
				{
					uiCommand.IsEnabled = false;
				}
				if (_copyText == "&Copy")
				{
					uiCommand.Text = _owningManager.BuiltInTexts[CommandsBuiltInText.CopyText];
				}
			}
			uiCommand = Commands.FindByKey("cmdCut");
			if (uiCommand != null)
			{
				if (GetTargetTextBox().SelectionLength > 0 && !GetTargetTextBox().ReadOnly)
				{
					uiCommand.IsEnabled = true;
				}
				else
				{
					uiCommand.IsEnabled = false;
				}
				if (_cutText == "Cu&t")
				{
					uiCommand.Text = _owningManager.BuiltInTexts[CommandsBuiltInText.CutText];
				}
			}
			uiCommand = Commands.FindByKey("cmdSelectAll");
			if (uiCommand != null)
			{
				if (GetTargetTextBox().SelectionLength < GetTargetTextBox().Text.Length)
				{
					uiCommand.IsEnabled = true;
				}
				else
				{
					uiCommand.IsEnabled = false;
				}
				if (_selectAllText == "Select A&ll")
				{
					uiCommand.Text = _owningManager.BuiltInTexts[CommandsBuiltInText.SelectAllText];
				}
			}
		}

		internal TextBoxBase GetTargetTextBox()
		{
			if (_sourceControl != null)
			{
				if (_sourceControl is TextBoxBase)
				{
					return (TextBoxBase)_sourceControl;
				}
				if (_sourceControl is ITextBoxOwner)
				{
					return ((ITextBoxOwner)_sourceControl).TextBoxControl;
				}
			}
			return null;
		}

		bool IExtenderProvider.CanExtend(object target)
		{
			if (target is TextBoxBase)
			{
				return true;
			}
			if (target is ITextBoxOwner)
			{
				return true;
			}
			return false;
		}

		[DefaultValue(false)]
		public bool GetUseMordexEditMenu(Control control)
		{
			if (_editMenuControls != null && _editMenuControls.Contains(control))
			{
				return true;
			}
			return false;
		}

		internal ContextMenuStrip GetOrCreateContextMenu()
		{
			if (_contextMenu == null)
			{
				_contextMenu = new ContextMenuStrip();
			}
			return _contextMenu;
		}

		public void SetUseMordexEditMenu(Control control, bool value)
		{
			if (control == null)
			{
				return;
			}
			if (_editMenuControls == null)
			{
				_editMenuControls = new ArrayList();
			}
			if (_editMenuControls.Contains(control))
			{
				if (control.ContextMenuStrip == GetOrCreateContextMenu())
				{
					control.ContextMenuStrip = null;
				}
				control.Disposed -= OnControlDisposed;
				_editMenuControls.Remove(control);
				SetUseMordexEditMenu(control, value: false);
			}
			try
			{
				if (value)
				{
					if (control is TextBoxBase)
					{
						control.ContextMenuStrip = GetOrCreateContextMenu();
					}
					control.Disposed += OnControlDisposed;
					_editMenuControls.Add(control);
				}
				if (control is ITextBoxOwner)
				{
					((ITextBoxOwner)control).UseDefaultMenu = !value;
				}
			}
			catch
			{
			}
		}

		private void OnControlDisposed(object sender, EventArgs eventArgs)
		{
			SetUseMordexEditMenu(sender as Control, value: false);
		}

		private static bool ClipboardHasText()
		{
			try
			{
				IDataObject data = Clipboard.GetDataObject();
				return data != null && data.GetDataPresent(DataFormats.Text);
			}
			catch (ExternalException)
			{
				return false;
			}
		}
	}
}
