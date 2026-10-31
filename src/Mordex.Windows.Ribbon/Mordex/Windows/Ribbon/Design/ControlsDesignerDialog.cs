using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	internal class ControlsDesignerDialog : Form
	{
		private Panel _panel;

		private Button _btnOK;

		public ControlsDesignerDialog(RibbonContextualTab contextualTab)
		{
			InitializeControls();
			Control control = contextualTab.Ribbon?.GetHostForm() ?? contextualTab.Ribbon?.Parent;
			RibbonContextualTabEditorControl value = new RibbonContextualTabEditorControl(contextualTab, control)
			{
				Dock = DockStyle.Fill
			};
			_panel.Controls.Add(value);
		}

		private void InitializeControls()
		{
			_panel = new Panel();
			_btnOK = new Button();
			SuspendLayout();
			_panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_panel.Location = new Point(0, 0);
			_panel.Name = "panel1";
			_panel.Size = new Size(484, 396);
			_panel.TabIndex = 0;
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(397, 402);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(75, 23);
			_btnOK.TabIndex = 1;
			_btnOK.Text = "OK";
			_btnOK.UseVisualStyleBackColor = true;
			base.AcceptButton = _btnOK;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(484, 437);
			base.Controls.Add(_btnOK);
			base.Controls.Add(_panel);
			base.Name = "ControlsDesignerDialog";
			Text = "Controls Designer";
			ResumeLayout(performLayout: false);
		}
	}
}
