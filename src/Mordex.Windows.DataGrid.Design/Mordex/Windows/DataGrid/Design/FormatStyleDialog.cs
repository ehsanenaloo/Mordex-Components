using System.ComponentModel;
using System;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	internal class FormatStyleDialog : Form
	{
		private RadioButton useOwnStyle;

		private RadioButton _useExistingStyle;

		private ListBox formatStyleList;

		private PropertyGrid formatStyleProperties;

		private Button cmdOk;

		private Label _noStylesLabel;

		private Button cmdCancel;

		private GridFormatStyleCollection _availableStyles;

		private GridFormatStyle _selectedStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal GridFormatStyleCollection AvailableStyles
		{
			get
			{
				return _availableStyles;
			}
			set
			{
				_availableStyles = value;
				if (_availableStyles != null)
				{
					formatStyleList.Items.Clear();
					for (int i = 0; i < _availableStyles.Count; i++)
					{
						GridFormatStyle gridFormatStyle = _availableStyles[i];
						formatStyleList.Items.Add("FormatStyle[" + i + "] - " + gridFormatStyle.Key);
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal GridFormatStyle SelectedFormatStyle
		{
			get
			{
				if (useOwnStyle.Checked)
				{
					return _selectedStyle;
				}
				if (_availableStyles != null && formatStyleList.SelectedIndex != -1)
				{
					return _availableStyles[formatStyleList.SelectedIndex];
				}
				return null;
			}
			set
			{
				if (value == null)
				{
					_selectedStyle = new GridFormatStyle();
					formatStyleProperties.SelectedObject = _selectedStyle;
					useOwnStyle.Checked = true;
					return;
				}
				_selectedStyle = value;
				if (DesignHelper.IsInFormatStylesCollection(_selectedStyle))
				{
					_useExistingStyle.Checked = true;
					if (_availableStyles == null)
					{
						return;
					}
					for (int i = 0; i < _availableStyles.Count; i++)
					{
						if (_availableStyles[i] == _selectedStyle)
						{
							formatStyleList.SelectedIndex = i;
							break;
						}
					}
				}
				else
				{
					// The dialog edits a copy, so Cancel leaves the caller's own style untouched.
					_selectedStyle = new GridFormatStyle(value);
					useOwnStyle.Checked = true;
					formatStyleProperties.SelectedObject = _selectedStyle;
				}
			}
		}

		public FormatStyleDialog()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Mordex.Windows.DataGrid.Design.FormatStyleDialog));
			this.cmdCancel = new System.Windows.Forms.Button();
			this.formatStyleList = new System.Windows.Forms.ListBox();
			this.cmdOk = new System.Windows.Forms.Button();
			this.formatStyleProperties = new System.Windows.Forms.PropertyGrid();
			this._useExistingStyle = new System.Windows.Forms.RadioButton();
			this.useOwnStyle = new System.Windows.Forms.RadioButton();
			this._noStylesLabel = new System.Windows.Forms.Label();
			base.SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(339, 245);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(80, 24);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "Cancel";
			this.formatStyleList.IntegralHeight = false;
			this.formatStyleList.Location = new System.Drawing.Point(9, 37);
			this.formatStyleList.Name = "formatStyleList";
			this.formatStyleList.Size = new System.Drawing.Size(409, 195);
			this.formatStyleList.TabIndex = 2;
			this.formatStyleList.Visible = false;
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.Location = new System.Drawing.Point(255, 245);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(80, 24);
			this.cmdOk.TabIndex = 4;
			this.cmdOk.Text = "OK";
			this.formatStyleProperties.CommandsVisibleIfAvailable = true;
			this.formatStyleProperties.HelpVisible = false;
			this.formatStyleProperties.LargeButtons = false;
			this.formatStyleProperties.LineColor = System.Drawing.SystemColors.Control;
			this.formatStyleProperties.Location = new System.Drawing.Point(9, 37);
			this.formatStyleProperties.Name = "formatStyleProperties";
			this.formatStyleProperties.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
			this.formatStyleProperties.Size = new System.Drawing.Size(409, 195);
			this.formatStyleProperties.TabIndex = 3;
			this.formatStyleProperties.Text = "propertyGrid1";
			this.formatStyleProperties.ToolbarVisible = false;
			this.formatStyleProperties.ViewBackColor = System.Drawing.SystemColors.Window;
			this.formatStyleProperties.ViewForeColor = System.Drawing.SystemColors.WindowText;
			this._useExistingStyle.Location = new System.Drawing.Point(175, 11);
			this._useExistingStyle.Name = "radioButton2";
			this._useExistingStyle.Size = new System.Drawing.Size(152, 20);
			this._useExistingStyle.TabIndex = 1;
			this._useExistingStyle.Text = "Use Existing FormatStyle";
			this._useExistingStyle.CheckedChanged += new System.EventHandler(radioButton2_CheckedChanged);
			this.useOwnStyle.Checked = true;
			this.useOwnStyle.Location = new System.Drawing.Point(16, 8);
			this.useOwnStyle.Name = "useOwnStyle";
			this.useOwnStyle.Size = new System.Drawing.Size(152, 24);
			this.useOwnStyle.TabIndex = 0;
			this.useOwnStyle.TabStop = true;
			this.useOwnStyle.Text = "Create New FormatStyle";
			this.useOwnStyle.CheckedChanged += new System.EventHandler(useOwnStyle_CheckedChanged);
			this._noStylesLabel.BackColor = System.Drawing.SystemColors.Window;
			this._noStylesLabel.Location = new System.Drawing.Point(63, 53);
			this._noStylesLabel.Name = "label1";
			this._noStylesLabel.Size = new System.Drawing.Size(290, 31);
			this._noStylesLabel.TabIndex = 6;
			this._noStylesLabel.Text = "There are no FormatStyles defined in the control.";
			this._noStylesLabel.Visible = false;
			base.AcceptButton = this.cmdOk;
			base.CancelButton = this.cmdCancel;
			base.ClientSize = new System.Drawing.Size(424, 274);
			base.Controls.AddRange(new System.Windows.Forms.Control[7] { this._noStylesLabel, this.cmdCancel, this.cmdOk, this._useExistingStyle, this.useOwnStyle, this.formatStyleProperties, this.formatStyleList });
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)resourceManager.GetObject("$this.Icon");
			base.Name = "FormatStyleDialog";
			this.Text = "FormatStyle Chooser";
			base.ResumeLayout(false);
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs eventArgs)
		{
			if (_useExistingStyle.Checked)
			{
				formatStyleProperties.Visible = false;
				formatStyleList.Visible = true;
				if (formatStyleList.Items.Count == 0)
				{
					_noStylesLabel.Visible = true;
					_noStylesLabel.BringToFront();
				}
				else
				{
					_noStylesLabel.Visible = false;
				}
			}
		}

		private void useOwnStyle_CheckedChanged(object sender, EventArgs eventArgs)
		{
			if (useOwnStyle.Checked)
			{
				formatStyleList.Visible = false;
				if (DesignHelper.IsInFormatStylesCollection(_selectedStyle))
				{
					_selectedStyle = new GridFormatStyle();
				}
				formatStyleProperties.SelectedObject = _selectedStyle;
				formatStyleProperties.Visible = true;
				_noStylesLabel.Visible = false;
			}
		}
	}
}
