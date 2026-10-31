using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	internal class RibbonDialog : Form
	{
		private TextBox _txtLayoutName;

		private Label _nameLabel;

		private Button _btnOK;

		private Button _btnCancel;

		public RibbonDialog()
		{
			InitializeControls();
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

		private void InitializeControls()
		{
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_txtLayoutName = new TextBox();
			_nameLabel = new Label();
			_btnOK = new Button();
			_btnCancel = new Button();
			SuspendLayout();
			_txtLayoutName.Location = new Point(12, 32);
			_txtLayoutName.Name = "txtLayoutName";
			_txtLayoutName.Size = new Size(256, 20);
			_txtLayoutName.TabIndex = 0;
			_txtLayoutName.Text = "";
			_nameLabel.AutoSize = true;
			_nameLabel.Location = new Point(8, 8);
			_nameLabel.Name = "label1";
			_nameLabel.Size = new Size(262, 13);
			_nameLabel.TabIndex = 1;
			_nameLabel.Text = "Enter the key of the layout in the Layouts collection.";
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
			base.Controls.AddRange(new Control[4] { _btnCancel, _btnOK, _nameLabel, _txtLayoutName });
			base.Name = "LayoutNameDialog";
			Text = "Ribbon";
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			ResumeLayout(performLayout: false);
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			Close();
		}
	}
}
