using System;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	internal class GridControlDialog : Form
	{
		private TextBox _txtLayoutName;

		private Label _label;

		private Button _btnOK;

		private Button _btnCancel;

		public GridControlDialog()
		{
			_txtLayoutName = new TextBox();
			_label = new Label();
			_btnOK = new Button();
			_btnCancel = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_txtLayoutName.Location = new Point(12, 32);
			_txtLayoutName.Name = "txtLayoutName";
			_txtLayoutName.Size = new Size(256, 20);
			_txtLayoutName.TabIndex = 0;
			_txtLayoutName.Text = "";
			_label.AutoSize = true;
			_label.Location = new Point(8, 8);
			_label.Name = "label1";
			_label.Size = new Size(262, 13);
			_label.TabIndex = 1;
			_label.Text = "Enter the key of the layout in the Layouts collection.";
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(108, 60);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 24);
			_btnOK.TabIndex = 2;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(192, 60);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 3;
			_btnCancel.Text = "Cancel";
			_btnCancel.Click += OnBtnCancelClick;
			base.AcceptButton = _btnOK;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(280, 89);
			base.Controls.AddRange(new Control[4] { _btnCancel, _btnOK, _label, _txtLayoutName });
			base.Name = "LayoutNameDialog";
			Text = "DataGrid Control";
			ResumeLayout(performLayout: false);
		}

		public string GetLayoutName()
		{
			return _txtLayoutName.Text;
		}

		public void SetLayoutName(string text)
		{
			_txtLayoutName.Text = text;
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			Close();
		}
	}
}
