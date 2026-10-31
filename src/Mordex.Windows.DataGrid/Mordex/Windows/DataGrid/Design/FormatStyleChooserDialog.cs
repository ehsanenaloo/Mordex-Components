using System;
using System.Drawing;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	internal class FormatStyleChooserDialog : Form
	{
		private RadioButton _useOwnStyle;

		private RadioButton _useExistingStyle;

		private ListBox _formatStyleList;

		private PropertyGrid _formatStyleProperties;

		private Button _cmdOk;

		private Label _label;

		private Button _cmdCancel;

		private GridFormatStyle _gridFormatStyle;

		public FormatStyleChooserDialog()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			ResourceManager resourceManager = new ResourceManager(typeof(FormatStyleChooserDialog));
			_cmdCancel = new Button();
			_formatStyleList = new ListBox();
			_cmdOk = new Button();
			_formatStyleProperties = new PropertyGrid();
			_useExistingStyle = new RadioButton();
			_useOwnStyle = new RadioButton();
			_label = new Label();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_cmdCancel.DialogResult = DialogResult.Cancel;
			_cmdCancel.Location = new Point(339, 245);
			_cmdCancel.Name = "cmdCancel";
			_cmdCancel.Size = new Size(80, 24);
			_cmdCancel.TabIndex = 5;
			_cmdCancel.Text = "Cancel";
			_formatStyleList.IntegralHeight = false;
			_formatStyleList.Location = new Point(9, 37);
			_formatStyleList.Name = "formatStyleList";
			_formatStyleList.Size = new Size(409, 195);
			_formatStyleList.TabIndex = 2;
			_formatStyleList.Visible = false;
			_cmdOk.DialogResult = DialogResult.OK;
			_cmdOk.Location = new Point(255, 245);
			_cmdOk.Name = "cmdOk";
			_cmdOk.Size = new Size(80, 24);
			_cmdOk.TabIndex = 4;
			_cmdOk.Text = "OK";
			_formatStyleProperties.CommandsVisibleIfAvailable = true;
			_formatStyleProperties.HelpVisible = false;
			_formatStyleProperties.LargeButtons = false;
			_formatStyleProperties.LineColor = SystemColors.Control;
			_formatStyleProperties.Location = new Point(9, 37);
			_formatStyleProperties.Name = "formatStyleProperties";
			_formatStyleProperties.PropertySort = PropertySort.Alphabetical;
			_formatStyleProperties.Size = new Size(409, 195);
			_formatStyleProperties.TabIndex = 3;
			_formatStyleProperties.Text = "propertyGrid1";
			_formatStyleProperties.ToolbarVisible = false;
			_formatStyleProperties.ViewBackColor = SystemColors.Window;
			_formatStyleProperties.ViewForeColor = SystemColors.WindowText;
			_useExistingStyle.Location = new Point(175, 11);
			_useExistingStyle.Name = "radioButton2";
			_useExistingStyle.Size = new Size(152, 20);
			_useExistingStyle.TabIndex = 1;
			_useExistingStyle.Text = "Use Existing FormatStyle";
			_useExistingStyle.CheckedChanged += OnRadioButton2CheckedChanged;
			_useOwnStyle.Checked = true;
			_useOwnStyle.Location = new Point(16, 8);
			_useOwnStyle.Name = "useOwnStyle";
			_useOwnStyle.Size = new Size(152, 24);
			_useOwnStyle.TabIndex = 0;
			_useOwnStyle.TabStop = true;
			_useOwnStyle.Text = "Create New FormatStyle";
			_useOwnStyle.CheckedChanged += OnUseOwnStyleCheckedChanged;
			_label.BackColor = SystemColors.Window;
			_label.Location = new Point(63, 53);
			_label.Name = "label1";
			_label.Size = new Size(290, 31);
			_label.TabIndex = 6;
			_label.Text = "There are no FormatStyles defined in the control.";
			_label.Visible = false;
			base.AcceptButton = _cmdOk;
			base.CancelButton = _cmdCancel;
			base.ClientSize = new Size(424, 274);
			base.Controls.AddRange(new Control[7] { _label, _cmdCancel, _cmdOk, _useExistingStyle, _useOwnStyle, _formatStyleProperties, _formatStyleList });
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.Icon = (Icon)resourceManager.GetObject("$this.Icon");
			base.Name = "FormatStyleDialog";
			Text = "FormatStyle Chooser";
			ResumeLayout(performLayout: false);
		}

		private void OnRadioButton2CheckedChanged(object sender, EventArgs eventArgs)
		{
			if (_useExistingStyle.Checked)
			{
				_formatStyleProperties.Visible = false;
				_formatStyleList.Visible = true;
				if (_formatStyleList.Items.Count == 0)
				{
					_label.Visible = true;
					_label.BringToFront();
				}
				else
				{
					_label.Visible = false;
				}
			}
		}

		private void OnUseOwnStyleCheckedChanged(object sender, EventArgs eventArgs)
		{
			if (_useOwnStyle.Checked)
			{
				_formatStyleList.Visible = false;
				if (_gridFormatStyle.IsInCollection())
				{
					_gridFormatStyle = new GridFormatStyle();
				}
				_formatStyleProperties.SelectedObject = _gridFormatStyle;
				_formatStyleProperties.Visible = true;
				_label.Visible = false;
			}
		}
	}
}
