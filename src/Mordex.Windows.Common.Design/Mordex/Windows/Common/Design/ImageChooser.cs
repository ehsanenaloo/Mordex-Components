using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Common.Design
{
	internal class ImageChooser : Form
	{
		private ImageEditor editor;

		private IContainer components = default;

		private RadioButton optImage;

		private RadioButton optImageIndex;

		private RadioButton optImageKey;

		private RadioButton optNoImage;

		private Button okButton;

		private Button cancelButton;

		private Label promptLabel;

		public ImageChooser()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			InitializeComponent();
			base.StartPosition = FormStartPosition.CenterScreen;
		}

		private void ImageChooser_Load(object sender, EventArgs eventArgs)
		{
		}

		internal DialogResult ShowDialog(ImageEditor editor)
		{
			this.editor = editor;
			switch (editor.imageMode)
			{
			case 0:
				optImage.Checked = true;
				break;
			case 1:
				optImageKey.Checked = true;
				break;
			case 2:
				optImageIndex.Checked = true;
				break;
			}
			ShowDialog(editor.FindForm());
			return base.DialogResult;
		}

		private void OnOkClicked(object sender, EventArgs eventArgs)
		{
			if (optImage.Checked)
			{
				editor.EditImage();
			}
			else if (optNoImage.Checked)
			{
				editor.ClearImage();
			}
			else if (optImageKey.Checked)
			{
				editor.EditImageKey();
			}
			else if (optImageIndex.Checked)
			{
				editor.EditImageIndex();
			}
			base.DialogResult = DialogResult.OK;
		}

		private void OnCancelClicked(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.Cancel;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.optImage = new System.Windows.Forms.RadioButton();
			this.optImageIndex = new System.Windows.Forms.RadioButton();
			this.optImageKey = new System.Windows.Forms.RadioButton();
			this.optNoImage = new System.Windows.Forms.RadioButton();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.promptLabel = new System.Windows.Forms.Label();
			base.SuspendLayout();
			this.optImage.AutoSize = true;
			this.optImage.Location = new System.Drawing.Point(41, 47);
			this.optImage.Name = "optImage";
			this.optImage.Size = new System.Drawing.Size(96, 17);
			this.optImage.TabIndex = 0;
			this.optImage.TabStop = true;
			this.optImage.Text = "Image Property";
			this.optImage.UseVisualStyleBackColor = true;
			this.optImageIndex.AutoSize = true;
			this.optImageIndex.Location = new System.Drawing.Point(41, 93);
			this.optImageIndex.Name = "optImageIndex";
			this.optImageIndex.Size = new System.Drawing.Size(122, 17);
			this.optImageIndex.TabIndex = 1;
			this.optImageIndex.TabStop = true;
			this.optImageIndex.Text = "ImageIndex Property";
			this.optImageIndex.UseVisualStyleBackColor = true;
			this.optImageKey.AutoSize = true;
			this.optImageKey.Location = new System.Drawing.Point(41, 70);
			this.optImageKey.Name = "optImageKey";
			this.optImageKey.Size = new System.Drawing.Size(114, 17);
			this.optImageKey.TabIndex = 2;
			this.optImageKey.TabStop = true;
			this.optImageKey.Text = "ImageKey Property";
			this.optImageKey.UseVisualStyleBackColor = true;
			this.optNoImage.AutoSize = true;
			this.optNoImage.Location = new System.Drawing.Point(41, 116);
			this.optNoImage.Name = "optNoImage";
			this.optNoImage.Size = new System.Drawing.Size(71, 17);
			this.optNoImage.TabIndex = 3;
			this.optNoImage.TabStop = true;
			this.optNoImage.Text = "No Image";
			this.optNoImage.UseVisualStyleBackColor = true;
			this.okButton.Location = new System.Drawing.Point(68, 148);
			this.okButton.Name = "button1";
			this.okButton.Size = new System.Drawing.Size(76, 28);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(OnOkClicked);
			this.cancelButton.Location = new System.Drawing.Point(150, 148);
			this.cancelButton.Name = "button2";
			this.cancelButton.Size = new System.Drawing.Size(80, 28);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(OnCancelClicked);
			this.promptLabel.Location = new System.Drawing.Point(12, 13);
			this.promptLabel.Name = "label1";
			this.promptLabel.Size = new System.Drawing.Size(151, 28);
			this.promptLabel.TabIndex = 6;
			this.promptLabel.Text = "Select the property you want to use to specify the Image:";
			base.AcceptButton = this.okButton;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.cancelButton;
			base.ClientSize = new System.Drawing.Size(242, 186);
			base.Controls.Add(this.promptLabel);
			base.Controls.Add(this.cancelButton);
			base.Controls.Add(this.okButton);
			base.Controls.Add(this.optNoImage);
			base.Controls.Add(this.optImageKey);
			base.Controls.Add(this.optImageIndex);
			base.Controls.Add(this.optImage);
			base.Name = "ImageChooser";
			this.Text = "ImageChooser";
			base.Load += new System.EventHandler(ImageChooser_Load);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
