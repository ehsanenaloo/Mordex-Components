using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI.CommandBars
{
	public class NameCommandBarForm : Form
	{
		private const int WmActivateApp = 0x001C;

		private Container _components = new Container();

		public MxButton btnOK;

		public MxButton btnCancel;

		public Label label1;

		public TextBox txtName;

		private MxCommandBar _commandBar;

		internal MxCommandManager _commandManager;

		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string ToolbarName
		{
			get
			{
				return txtName.Text;
			}
			set
			{
				txtName.Text = value;
			}
		}

		public MxCommandManager CommandManager => _commandManager;

		public NameCommandBarForm()
		{
			InitializeControls();
			Mordex.Windows.Common.ControlThemer.AttachDialog(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			btnOK = new MxButton();
			btnCancel = new MxButton();
			label1 = new Label();
			txtName = new TextBox();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			btnOK.DialogResult = DialogResult.OK;
			btnOK.Location = new Point(100, 48);
			btnOK.Name = "btnOK";
			btnOK.Size = new Size(75, 23);
			btnOK.TabIndex = 2;
			btnOK.Text = "OK";
			btnOK.Click += OnOkButtonClick;
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.Location = new Point(184, 48);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(75, 23);
			btnCancel.TabIndex = 3;
			btnCancel.Text = "Cancel";
			label1.AutoSize = true;
			label1.FlatStyle = FlatStyle.System;
			label1.Location = new Point(4, 4);
			label1.Name = "label1";
			label1.Size = new Size(75, 13);
			label1.TabIndex = 0;
			label1.Text = "&Toolbar name:";
			txtName.Location = new Point(4, 20);
			txtName.Name = "txtName";
			txtName.Size = new Size(256, 20);
			txtName.TabIndex = 1;
			base.AcceptButton = btnOK;
			base.CancelButton = btnCancel;
			base.ClientSize = new Size(264, 76);
			base.Controls.Add(txtName);
			base.Controls.Add(label1);
			base.Controls.Add(btnOK);
			base.Controls.Add(btnCancel);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "NameCommandBarForm";
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			Text = "New Toolbar";
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		internal void SetCommandBar(MxCommandBar commandBar)
		{
			_commandBar = commandBar;
		}

		internal string GetDuplicatedNameMessage()
		{
			if (CommandManager != null)
			{
				return CommandManager.BuiltInTexts[CommandsBuiltInText.DuplicatedNameErrorMessage];
			}
			return "A toolbar named '{0}' already exist. Please type another name.";
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WmActivateApp && m.WParam != IntPtr.Zero)
			{
				BringToFront();
			}
			base.WndProc(ref m);
		}

		protected override void OnLoad(EventArgs e)
		{
			if (base.DesignMode)
			{
				return;
			}
			base.OnLoad(e);
			if (_commandBar != null)
			{
				txtName.Text = _commandBar.Key;
			}
			else if (ToolbarName.Length <= 0)
			{
				int suffix = 1;
				string text = "Custom " + suffix;
				while (!CommandManager.IsCommandBarKeyUnique(null, text))
				{
					suffix++;
					text = "Custom " + suffix;
				}
				txtName.Text = text;
			}
		}

		internal string GetEmptyNameMessage()
		{
			if (CommandManager != null)
			{
				return CommandManager.BuiltInTexts[CommandsBuiltInText.EmptyNameErrorMessage];
			}
			return "The toolbar name cannot be blank. Please type a name";
		}

		private void OnOkButtonClick(object sender, EventArgs eventArgs)
		{
			string caption = "";
			if (CommandManager.GetForm() != null)
			{
				caption = CommandManager.GetForm().Text;
			}
			if (txtName.Text.Length == 0)
			{
				Mordex.Windows.Common.MxMessageBox.Show(this, GetEmptyNameMessage(), caption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				RenderingHelper.TryFocusControl(txtName);
				return;
			}
			if (!CommandManager.IsCommandBarKeyUnique(_commandBar, txtName.Text))
			{
				if (GetDuplicatedNameMessage().IndexOf("{0}") >= 0)
				{
					Mordex.Windows.Common.MxMessageBox.Show(this, string.Format(GetDuplicatedNameMessage(), txtName.Text), caption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					Mordex.Windows.Common.MxMessageBox.Show(this, GetDuplicatedNameMessage(), caption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				RenderingHelper.TryFocusControl(txtName);
				txtName.SelectAll();
				return;
			}
			if (_commandBar == null)
			{
				_commandBar = new MxCommandBar();
				if (!CommandManager.IsInDesignMode())
				{
					_commandBar.AllowDelete = true;
					_commandBar.AllowRename = true;
				}
			}
			_commandBar.Key = txtName.Text;
			_commandBar.Text = txtName.Text;
			_commandBar.Name = txtName.Text;
			if (_commandBar.CommandManager == null)
			{
				CommandManager.CommandBars.Add(_commandBar);
			}
			base.DialogResult = DialogResult.OK;
		}
	}
}
