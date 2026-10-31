using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	internal class TabPagesDesignerDialog : Form
	{
		private Container _container = new Container();

		private Button _btnOK;

		private CommandManagerEditorControl _collectionPage;

		public TabPagesDesignerDialog(MxTabControl tabControl)
		{
			InitializeControls();
			_collectionPage.SetTabControl(tabControl);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_collectionPage = new CommandManagerEditorControl();
			_btnOK = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_collectionPage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_collectionPage.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_collectionPage.Location = new Point(0, 0);
			_collectionPage.Name = "collectionPage";
			_collectionPage.Size = new Size(523, 344);
			_collectionPage.SetTabControl(null);
			_collectionPage.TabIndex = 0;
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(443, 350);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 24);
			_btnOK.TabIndex = 0;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			base.ClientSize = new Size(531, 381);
			base.Controls.Add(_btnOK);
			base.Controls.Add(_collectionPage);
			base.Name = "TabPagesDesigner";
			Text = "TabPagesDesigner";
			ResumeLayout(performLayout: false);
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			_collectionPage.CommitTabPages();
			Close();
		}
	}
}
