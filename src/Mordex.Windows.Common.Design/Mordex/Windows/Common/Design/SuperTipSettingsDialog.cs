using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Common.Design
{
	internal class SuperTipSettingsDialog : Form
	{
		private SuperTipSettings original;

		private MxSuperTip superTip;

		private SuperTipSettings settings;

		private IContainer components = default;

		private TextBox txtHeaderText;

		private Label textLabel;

		private Label descriptionLabel;

		private Button btnOK;

		private Button btnCancel;

		private Label footerLabel;

		private TextBox txtFooter;

		private Button previewButton;

		private ImageEditor headerImageEditor;

		private ImageEditor footerImageEditor;

		private TextBox txtDescription;

		private ImageEditor descriptionImageEditor;

		private Label imageLabel;

		private Label headerLabel;

		public SuperTipSettingsDialog()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			InitializeComponent();
			base.StartPosition = FormStartPosition.CenterScreen;
		}

		internal DialogResult ShowDialog(ITypeDescriptorContext context, IServiceProvider provider, MxSuperTip superTip, SuperTipSettings settings)
		{
			this.superTip = superTip;
			original = settings;
			this.settings = new SuperTipSettings();
			CopySettings(this.settings, original);
			txtHeaderText.Text = this.settings.HeaderText;
			txtDescription.Text = this.settings.Text;
			txtFooter.Text = this.settings.FooterText;
			headerImageEditor.Edit(this.settings, superTip, "HeaderImage", "HeaderImageIndex", "HeaderImageKey", context, provider);
			descriptionImageEditor.Edit(this.settings, superTip, "Image", "ImageIndex", "ImageKey", context, provider);
			footerImageEditor.Edit(this.settings, superTip, "FooterImage", "FooterImageIndex", "FooterImageKey", context, provider);
			DialogResult dialogResult = ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				ReadTextBoxesIntoSettings();
				CopySettings(original, this.settings);
			}
			return dialogResult;
		}

		private void CopySettings(SuperTipSettings target, SuperTipSettings source)
		{
			target.Text = source.Text;
			target.Image = source.Image;
			target.HeaderText = source.HeaderText;
			target.HeaderImage = source.HeaderImage;
			target.FooterText = source.FooterText;
			target.FooterImage = source.FooterImage;
			target.HeaderImageIndex = source.HeaderImageIndex;
			target.HeaderImageKey = source.HeaderImageKey;
			target.ImageIndex = source.ImageIndex;
			target.ImageKey = source.ImageKey;
			target.FooterImageIndex = source.FooterImageIndex;
			target.FooterImageKey = source.FooterImageKey;
		}

		private void ReadTextBoxesIntoSettings()
		{
			settings.HeaderText = txtHeaderText.Text;
			settings.Text = txtDescription.Text;
			settings.FooterText = txtFooter.Text;
		}

		private void SuperTipSettingsDialog_Load(object sender, EventArgs eventArgs)
		{
		}

		private void OnPreviewClicked(object sender, EventArgs eventArgs)
		{
			ReadTextBoxesIntoSettings();
			Point p = new Point(previewButton.Left, previewButton.Top + previewButton.Height);
			p = previewButton.Parent.PointToScreen(p);
			superTip.Show(settings, p);
		}

		private void OnCancelClicked(object sender, EventArgs eventArgs)
		{
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
			this.txtHeaderText = new System.Windows.Forms.TextBox();
			this.textLabel = new System.Windows.Forms.Label();
			this.descriptionLabel = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.footerLabel = new System.Windows.Forms.Label();
			this.txtFooter = new System.Windows.Forms.TextBox();
			this.previewButton = new System.Windows.Forms.Button();
			this.headerImageEditor = new Mordex.Windows.Common.Design.ImageEditor();
			this.footerImageEditor = new Mordex.Windows.Common.Design.ImageEditor();
			this.txtDescription = new System.Windows.Forms.TextBox();
			this.descriptionImageEditor = new Mordex.Windows.Common.Design.ImageEditor();
			this.imageLabel = new System.Windows.Forms.Label();
			this.headerLabel = new System.Windows.Forms.Label();
			base.SuspendLayout();
			this.txtHeaderText.Location = new System.Drawing.Point(102, 32);
			this.txtHeaderText.Name = "txtHeaderText";
			this.txtHeaderText.Size = new System.Drawing.Size(226, 20);
			this.txtHeaderText.TabIndex = 0;
			this.textLabel.AutoSize = true;
			this.textLabel.Location = new System.Drawing.Point(99, 9);
			this.textLabel.Name = "label1";
			this.textLabel.Size = new System.Drawing.Size(31, 13);
			this.textLabel.TabIndex = 1;
			this.textLabel.Text = "Text:";
			this.descriptionLabel.AutoSize = true;
			this.descriptionLabel.Location = new System.Drawing.Point(18, 61);
			this.descriptionLabel.Name = "label2";
			this.descriptionLabel.Size = new System.Drawing.Size(63, 13);
			this.descriptionLabel.TabIndex = 2;
			this.descriptionLabel.Text = "Description:";
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(289, 166);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(80, 24);
			this.btnOK.TabIndex = 4;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(375, 166);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(80, 24);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.footerLabel.AutoSize = true;
			this.footerLabel.Location = new System.Drawing.Point(18, 133);
			this.footerLabel.Name = "label3";
			this.footerLabel.Size = new System.Drawing.Size(37, 13);
			this.footerLabel.TabIndex = 7;
			this.footerLabel.Text = "Footer";
			this.txtFooter.Location = new System.Drawing.Point(102, 130);
			this.txtFooter.Name = "txtFooter";
			this.txtFooter.Size = new System.Drawing.Size(226, 20);
			this.txtFooter.TabIndex = 6;
			this.previewButton.Location = new System.Drawing.Point(461, 166);
			this.previewButton.Name = "button1";
			this.previewButton.Size = new System.Drawing.Size(80, 24);
			this.previewButton.TabIndex = 8;
			this.previewButton.Text = "Preview...";
			this.previewButton.UseVisualStyleBackColor = true;
			this.previewButton.Click += new System.EventHandler(OnPreviewClicked);
			this.headerImageEditor.BackColor = System.Drawing.SystemColors.Window;
			this.headerImageEditor.Location = new System.Drawing.Point(344, 32);
			this.headerImageEditor.Name = "ieImage";
			this.headerImageEditor.Size = new System.Drawing.Size(197, 20);
			this.headerImageEditor.TabIndex = 9;
			this.footerImageEditor.BackColor = System.Drawing.SystemColors.Window;
			this.footerImageEditor.Location = new System.Drawing.Point(344, 130);
			this.footerImageEditor.Name = "ieFooter";
			this.footerImageEditor.Size = new System.Drawing.Size(197, 20);
			this.footerImageEditor.TabIndex = 11;
			this.txtDescription.Location = new System.Drawing.Point(102, 58);
			this.txtDescription.Multiline = true;
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.Size = new System.Drawing.Size(226, 65);
			this.txtDescription.TabIndex = 3;
			this.descriptionImageEditor.BackColor = System.Drawing.SystemColors.Window;
			this.descriptionImageEditor.Location = new System.Drawing.Point(344, 58);
			this.descriptionImageEditor.Name = "ieDescription";
			this.descriptionImageEditor.Size = new System.Drawing.Size(197, 20);
			this.descriptionImageEditor.TabIndex = 10;
			this.imageLabel.AutoSize = true;
			this.imageLabel.Location = new System.Drawing.Point(341, 9);
			this.imageLabel.Name = "label4";
			this.imageLabel.Size = new System.Drawing.Size(36, 13);
			this.imageLabel.TabIndex = 12;
			this.imageLabel.Text = "Image";
			this.headerLabel.AutoSize = true;
			this.headerLabel.Location = new System.Drawing.Point(18, 35);
			this.headerLabel.Name = "label5";
			this.headerLabel.Size = new System.Drawing.Size(45, 13);
			this.headerLabel.TabIndex = 13;
			this.headerLabel.Text = "Header:";
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(550, 196);
			base.ControlBox = false;
			base.Controls.Add(this.headerLabel);
			base.Controls.Add(this.imageLabel);
			base.Controls.Add(this.footerImageEditor);
			base.Controls.Add(this.descriptionImageEditor);
			base.Controls.Add(this.headerImageEditor);
			base.Controls.Add(this.previewButton);
			base.Controls.Add(this.footerLabel);
			base.Controls.Add(this.txtFooter);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnOK);
			base.Controls.Add(this.txtDescription);
			base.Controls.Add(this.descriptionLabel);
			base.Controls.Add(this.textLabel);
			base.Controls.Add(this.txtHeaderText);
			base.Name = "SuperTipSettingsDialog";
			this.Text = "ToolTip Settings";
			base.Load += new System.EventHandler(SuperTipSettingsDialog_Load);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
