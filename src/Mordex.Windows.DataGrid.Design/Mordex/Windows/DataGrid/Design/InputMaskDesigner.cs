using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	internal class InputMaskDesigner : Form
	{
		private MxDataGrid _masksGrid;

		private Label _tryItLabel;

		private MaskEdit _tryMaskEdit;

		private Button _okButton;

		private GroupBox grpTryIt;

		private NumericEdit _tryNumericEdit;

		private Button _cancelButton;

		private GroupBox grpMaskEdit;

		private TextBox txtMask;

		private CheckBox chkIncludeLiterals;

		private ComboBox cboMaskPrompt;

		private Label lblMaskPrompt;

		private Label lblMask;

		private GroupBox grpNumeric;

		private Label lblDecimals;

		private Label _decimalsHintLabel;

		private MxIntegerUpDown neDecimalDigits;

		private Label _commonMasksLabel;

		private ImageList _buttonImages;

		private ToolTip _maskToolTip;

		private Button _digitRequiredButton;

		private Button _digitOptionalButton;

		private Button _letterRequiredButton;

		private Button _letterOptionalButton;

		private Button _alphanumericRequiredButton;

		private Button _alphanumericOptionalButton;

		private Button _anyCharRequiredButton;

		private Button _anyCharOptionalButton;

		private Button _decimalSeparatorButton;

		private Button _thousandsSeparatorButton;

		private Button _timeSeparatorButton;

		private Button _dateSeparatorButton;

		private Button _rightToLeftButton;

		private Button _literalEscapeButton;

		private Button _clearButton;

		private IContainer components;

		private bool includeNumericFormats;

		private string[][] masks;

		private string inputMask = "";

		private bool isNumericMask;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal bool IncludeNumericFormats
		{
			get
			{
				return includeNumericFormats;
			}
			set
			{
				includeNumericFormats = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal bool IncludeLiterals
		{
			get
			{
				return chkIncludeLiterals.Checked;
			}
			set
			{
				chkIncludeLiterals.Checked = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal string MaskPrompt
		{
			get
			{
				if (cboMaskPrompt.Text.Length == 0)
				{
					return "_";
				}
				if (cboMaskPrompt.Text == (string)cboMaskPrompt.Items[1])
				{
					return " ";
				}
				return cboMaskPrompt.Text;
			}
			set
			{
				if (value == " ")
				{
					cboMaskPrompt.SelectedIndex = 1;
				}
				else
				{
					cboMaskPrompt.Text = value;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal string InputMask
		{
			get
			{
				if (IsNumericMask)
				{
					if (neDecimalDigits.Value != -1)
					{
						return inputMask + neDecimalDigits.Value;
					}
					return inputMask;
				}
				return inputMask;
			}
			set
			{
				if (includeNumericFormats && IsMaskNumeric(value))
				{
					IsNumericMask = true;
					return;
				}
				IsNumericMask = false;
				inputMask = value;
				if (string.IsNullOrEmpty(inputMask))
				{
					return;
				}
				bool condition = false;
				for (int i = 0; i < masks.Length; i++)
				{
					string[] array = masks[i];
					if (array[1] == value)
					{
						condition = true;
						GridRow row = _masksGrid.GetRow(array);
						if (row != null)
						{
							_masksGrid.MoveTo(row);
						}
						break;
					}
				}
				if (!condition)
				{
					_masksGrid.Row = -3;
					inputMask = value;
					txtMask.Text = inputMask;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal bool IsNumericMask
		{
			get
			{
				return isNumericMask;
			}
			set
			{
				if (isNumericMask != value)
				{
					isNumericMask = value;
					if (isNumericMask)
					{
						grpMaskEdit.Visible = false;
						grpNumeric.Visible = true;
						_tryMaskEdit.Visible = false;
						_tryNumericEdit.Visible = true;
						neDecimalDigits.Value = -1;
					}
					else
					{
						grpMaskEdit.Visible = true;
						grpNumeric.Visible = false;
						_tryNumericEdit.Visible = false;
						_tryMaskEdit.Visible = true;
					}
					SetTryProperties();
				}
			}
		}

		internal InputMaskDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			InitializeComponent();
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
			this.components = new System.ComponentModel.Container();
			Mordex.Windows.DataGrid.GridLayout gridLayout = new Mordex.Windows.DataGrid.GridLayout();
			this._masksGrid = new Mordex.Windows.DataGrid.MxDataGrid();
			this.grpTryIt = new System.Windows.Forms.GroupBox();
			this._tryItLabel = new System.Windows.Forms.Label();
			this._tryMaskEdit = new Mordex.Windows.DataGrid.Editors.MaskEdit();
			this._tryNumericEdit = new Mordex.Windows.DataGrid.Editors.NumericEdit();
			this._okButton = new System.Windows.Forms.Button();
			this.grpMaskEdit = new System.Windows.Forms.GroupBox();
			this._clearButton = new System.Windows.Forms.Button();
			this._literalEscapeButton = new System.Windows.Forms.Button();
			this._rightToLeftButton = new System.Windows.Forms.Button();
			this._dateSeparatorButton = new System.Windows.Forms.Button();
			this._timeSeparatorButton = new System.Windows.Forms.Button();
			this._thousandsSeparatorButton = new System.Windows.Forms.Button();
			this._decimalSeparatorButton = new System.Windows.Forms.Button();
			this._anyCharOptionalButton = new System.Windows.Forms.Button();
			this._anyCharRequiredButton = new System.Windows.Forms.Button();
			this._alphanumericOptionalButton = new System.Windows.Forms.Button();
			this._alphanumericRequiredButton = new System.Windows.Forms.Button();
			this._letterOptionalButton = new System.Windows.Forms.Button();
			this._letterRequiredButton = new System.Windows.Forms.Button();
			this._digitOptionalButton = new System.Windows.Forms.Button();
			this.chkIncludeLiterals = new System.Windows.Forms.CheckBox();
			this.lblMaskPrompt = new System.Windows.Forms.Label();
			this.cboMaskPrompt = new System.Windows.Forms.ComboBox();
			this.txtMask = new System.Windows.Forms.TextBox();
			this.lblMask = new System.Windows.Forms.Label();
			this._digitRequiredButton = new System.Windows.Forms.Button();
			this._buttonImages = new System.Windows.Forms.ImageList(this.components);
			this._cancelButton = new System.Windows.Forms.Button();
			this.grpNumeric = new System.Windows.Forms.GroupBox();
			this._decimalsHintLabel = new System.Windows.Forms.Label();
			this.lblDecimals = new System.Windows.Forms.Label();
			this.neDecimalDigits = new Mordex.Windows.DataGrid.Editors.MxIntegerUpDown();
			this._commonMasksLabel = new System.Windows.Forms.Label();
			this._maskToolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)this._masksGrid).BeginInit();
			this.grpTryIt.SuspendLayout();
			this.grpMaskEdit.SuspendLayout();
			this.grpNumeric.SuspendLayout();
			base.SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this._masksGrid.BuiltInTextsData = "<LocalizableData ID=\"LocalizableStrings\" Collection=\"true\"><GroupByBoxInfo>Which input mask matches how  you want data to look</GroupByBoxInfo></LocalizableData>";
			this._masksGrid.ColumnAutoResize = true;
			gridLayout.LayoutString = "<GridLayoutData><RootTable><Columns Collection=\"true\"><Column0 ID=\"InputMask\"><Bound>False</Bound><Caption>Input Mask:</Caption><EditType>CalendarDropDown</EditType><Key>InputMask</Key><Position>0</Position><Selectable>False</Selectable><Width>155</Width></Column0><Column1 ID=\"SampleData\"><Bound>False</Bound><Caption>Sample Data:</Caption><Key>SampleData</Key><Position>1</Position><Selectable>False</Selectable><Width>108</Width></Column1><Column2 ID=\"Category\"><Bound>False</Bound><Caption>Category</Caption><DefaultGroupPrefix /><Key>Category</Key><Position>2</Position><Visible>False</Visible></Column2></Columns><GroupCondition ID=\"\" /><Groups Collection=\"true\"><Group0 ID=\"Group0\"><ColIndex>2</ColIndex><GroupInterval>Value</GroupInterval></Group0></Groups></RootTable></GridLayoutData>";
			this._masksGrid.DesignTimeLayout = gridLayout;
			this._masksGrid.EditorsControlStyle.ButtonAppearance = Mordex.Windows.DataGrid.ButtonAppearance.Regular;
			this._masksGrid.EmptyRows = true;
			this._masksGrid.ExpandableGroups = Mordex.Windows.DataGrid.InheritableBoolean.False;
			this._masksGrid.FlatBorderColor = System.Drawing.SystemColors.ControlDark;
			this._masksGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this._masksGrid.GridLineColor = System.Drawing.SystemColors.Control;
			this._masksGrid.GridLines = Mordex.Windows.DataGrid.GridLines.None;
			this._masksGrid.GridLineStyle = Mordex.Windows.DataGrid.GridLineStyle.Solid;
			this._masksGrid.GroupByBoxFormatStyle.BackColor = System.Drawing.SystemColors.Control;
			this._masksGrid.GroupByBoxInfoFormatStyle.BackColor = System.Drawing.SystemColors.Info;
			this._masksGrid.GroupByBoxInfoFormatStyle.ForeColor = System.Drawing.SystemColors.InfoText;
			this._masksGrid.GroupByBoxVisible = false;
			this._masksGrid.GroupRowFormatStyle.FontBold = Mordex.Windows.DataGrid.TriState.True;
			this._masksGrid.HeaderFormatStyle.Appearance = Mordex.Windows.DataGrid.Appearance.Flat;
			this._masksGrid.HideSelection = Mordex.Windows.DataGrid.HideSelection.Highlight;
			this._masksGrid.Location = new System.Drawing.Point(7, 26);
			this._masksGrid.Name = "grid1";
			this._masksGrid.ScrollBars = Mordex.Windows.DataGrid.ScrollBars.Vertical;
			this._masksGrid.Size = new System.Drawing.Size(300, 150);
			this._masksGrid.TabIndex = 0;
			this._masksGrid.TabKeyBehavior = Mordex.Windows.DataGrid.TabKeyBehavior.ControlNavigation;
			this._masksGrid.FormattingRow += new Mordex.Windows.DataGrid.RowLoadEventHandler(grid1_FormattingRow);
			this._masksGrid.DoubleClick += new System.EventHandler(grid1_SelectionChanged);
			this._masksGrid.SelectionChanged += new System.EventHandler(grid1_SelectionChanged);
			this._masksGrid.LoadingRow += new Mordex.Windows.DataGrid.RowLoadEventHandler(grid1_LoadingRow);
			this.grpTryIt.Controls.AddRange(new System.Windows.Forms.Control[3] { this._tryItLabel, this._tryMaskEdit, this._tryNumericEdit });
			this.grpTryIt.Location = new System.Drawing.Point(7, 267);
			this.grpTryIt.Name = "grpTryIt";
			this.grpTryIt.Size = new System.Drawing.Size(301, 36);
			this.grpTryIt.TabIndex = 2;
			this.grpTryIt.TabStop = false;
			this._tryItLabel.Location = new System.Drawing.Point(6, 10);
			this._tryItLabel.Name = "label1";
			this._tryItLabel.Size = new System.Drawing.Size(42, 20);
			this._tryItLabel.TabIndex = 3;
			this._tryItLabel.Text = "Try It:";
			this._tryMaskEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this._tryMaskEdit.Location = new System.Drawing.Point(50, 13);
			this._tryMaskEdit.Name = "maskEdit1";
			this._tryMaskEdit.Size = new System.Drawing.Size(245, 21);
			this._tryMaskEdit.TabIndex = 4;
			this._tryMaskEdit.Text = "";
			this._tryMaskEdit.Validating += new System.ComponentModel.CancelEventHandler(maskEdit1_Validating);
			this._tryNumericEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this._tryNumericEdit.Location = new System.Drawing.Point(50, 11);
			this._tryNumericEdit.Name = "numericEdit1";
			this._tryNumericEdit.Size = new System.Drawing.Size(245, 21);
			this._tryNumericEdit.TabIndex = 4;
			this._tryNumericEdit.Text = "0.00";
			Mordex.Windows.DataGrid.Editors.NumericEdit numericEdit = this._tryNumericEdit;
			int[] bits = new int[4];
			numericEdit.Value = new decimal(bits);
			this._tryNumericEdit.Visible = false;
			this._tryNumericEdit.WordWrap = false;
			this._tryNumericEdit.Validating += new System.ComponentModel.CancelEventHandler(numericEdit1_Validating);
			this._okButton.Location = new System.Drawing.Point(316, 28);
			this._okButton.Name = "button1";
			this._okButton.Size = new System.Drawing.Size(80, 24);
			this._okButton.TabIndex = 5;
			this._okButton.Text = "OK";
			this._okButton.Click += new System.EventHandler(button1_Click);
			this.grpMaskEdit.Controls.AddRange(new System.Windows.Forms.Control[20]
			{
				this._clearButton, this._literalEscapeButton, this._rightToLeftButton, this._dateSeparatorButton, this._timeSeparatorButton, this._thousandsSeparatorButton, this._decimalSeparatorButton, this._anyCharOptionalButton, this._anyCharRequiredButton, this._alphanumericOptionalButton,
				this._alphanumericRequiredButton, this._letterOptionalButton, this._letterRequiredButton, this._digitOptionalButton, this.chkIncludeLiterals, this.lblMaskPrompt, this.cboMaskPrompt, this.txtMask, this.lblMask, this._digitRequiredButton
			});
			this.grpMaskEdit.Location = new System.Drawing.Point(7, 175);
			this.grpMaskEdit.Name = "grpMaskEdit";
			this.grpMaskEdit.Size = new System.Drawing.Size(301, 92);
			this.grpMaskEdit.TabIndex = 1;
			this.grpMaskEdit.TabStop = false;
			this._clearButton.Location = new System.Drawing.Point(210, 37);
			this._clearButton.Name = "button17";
			this._clearButton.Size = new System.Drawing.Size(72, 22);
			this._clearButton.TabIndex = 22;
			this._clearButton.TabStop = false;
			this._clearButton.Text = "Clear";
			this._clearButton.Click += new System.EventHandler(button17_Click);
			this._literalEscapeButton.Location = new System.Drawing.Point(264, 10);
			this._literalEscapeButton.Name = "button16";
			this._literalEscapeButton.Size = new System.Drawing.Size(20, 20);
			this._literalEscapeButton.TabIndex = 21;
			this._literalEscapeButton.TabStop = false;
			this._literalEscapeButton.Tag = "\\";
			this._literalEscapeButton.Text = "\\";
			this._maskToolTip.SetToolTip(this._literalEscapeButton, "Causes the character that follows to be displayed as a literal character. Used to display any of the characters listed in this table as literal characters (for example, \\C is displayed as just C).");
			this._literalEscapeButton.Click += new System.EventHandler(btnMask_Click);
			this._literalEscapeButton.Enter += new System.EventHandler(button10_Enter);
			this._rightToLeftButton.Location = new System.Drawing.Point(244, 10);
			this._rightToLeftButton.Name = "button15";
			this._rightToLeftButton.Size = new System.Drawing.Size(20, 20);
			this._rightToLeftButton.TabIndex = 20;
			this._rightToLeftButton.TabStop = false;
			this._rightToLeftButton.Tag = "!";
			this._rightToLeftButton.Text = "!";
			this._maskToolTip.SetToolTip(this._rightToLeftButton, "You can include the exclamation point anywhere in the input mask. When this character is used in a mask, characters are displayed from right to left, rather than from left to right when reading a text with non required characters.  *Characters typed into the mask always fill from left to right. ");
			this._rightToLeftButton.Click += new System.EventHandler(button15_Click);
			this._rightToLeftButton.Enter += new System.EventHandler(button10_Enter);
			this._dateSeparatorButton.Location = new System.Drawing.Point(224, 10);
			this._dateSeparatorButton.Name = "button14";
			this._dateSeparatorButton.Size = new System.Drawing.Size(20, 20);
			this._dateSeparatorButton.TabIndex = 19;
			this._dateSeparatorButton.TabStop = false;
			this._dateSeparatorButton.Tag = "/";
			this._dateSeparatorButton.Text = "/";
			this._maskToolTip.SetToolTip(this._dateSeparatorButton, "Date separator placeholder. (The actual character used depends on the regional settings.)");
			this._dateSeparatorButton.Click += new System.EventHandler(btnMask_Click);
			this._dateSeparatorButton.Enter += new System.EventHandler(button10_Enter);
			this._timeSeparatorButton.Location = new System.Drawing.Point(204, 10);
			this._timeSeparatorButton.Name = "button13";
			this._timeSeparatorButton.Size = new System.Drawing.Size(20, 20);
			this._timeSeparatorButton.TabIndex = 18;
			this._timeSeparatorButton.TabStop = false;
			this._timeSeparatorButton.Tag = ":";
			this._timeSeparatorButton.Text = ":";
			this._maskToolTip.SetToolTip(this._timeSeparatorButton, "Time separator placeholder. (The actual character used depends on the regional settings.)");
			this._timeSeparatorButton.Click += new System.EventHandler(btnMask_Click);
			this._timeSeparatorButton.Enter += new System.EventHandler(button10_Enter);
			this._thousandsSeparatorButton.Location = new System.Drawing.Point(184, 10);
			this._thousandsSeparatorButton.Name = "button12";
			this._thousandsSeparatorButton.Size = new System.Drawing.Size(20, 20);
			this._thousandsSeparatorButton.TabIndex = 17;
			this._thousandsSeparatorButton.TabStop = false;
			this._thousandsSeparatorButton.Tag = ",";
			this._thousandsSeparatorButton.Text = ",";
			this._maskToolTip.SetToolTip(this._thousandsSeparatorButton, "Thousands separator placeholder. (The actual character used depends on the regional settings.)");
			this._thousandsSeparatorButton.Click += new System.EventHandler(btnMask_Click);
			this._thousandsSeparatorButton.Enter += new System.EventHandler(button10_Enter);
			this._decimalSeparatorButton.Location = new System.Drawing.Point(164, 10);
			this._decimalSeparatorButton.Name = "button11";
			this._decimalSeparatorButton.Size = new System.Drawing.Size(20, 20);
			this._decimalSeparatorButton.TabIndex = 16;
			this._decimalSeparatorButton.TabStop = false;
			this._decimalSeparatorButton.Tag = ".";
			this._decimalSeparatorButton.Text = ".";
			this._maskToolTip.SetToolTip(this._decimalSeparatorButton, "Decimal separator placeholder. (The actual character used depends on the regional settings.)");
			this._decimalSeparatorButton.Click += new System.EventHandler(btnMask_Click);
			this._decimalSeparatorButton.Enter += new System.EventHandler(button10_Enter);
			this._anyCharOptionalButton.Location = new System.Drawing.Point(144, 10);
			this._anyCharOptionalButton.Name = "button10";
			this._anyCharOptionalButton.Size = new System.Drawing.Size(20, 20);
			this._anyCharOptionalButton.TabIndex = 15;
			this._anyCharOptionalButton.TabStop = false;
			this._anyCharOptionalButton.Tag = "C";
			this._anyCharOptionalButton.Text = "C";
			this._maskToolTip.SetToolTip(this._anyCharOptionalButton, "Any character or a space. (Entry optional)");
			this._anyCharOptionalButton.Click += new System.EventHandler(btnMask_Click);
			this._anyCharOptionalButton.Enter += new System.EventHandler(button10_Enter);
			this._anyCharRequiredButton.Location = new System.Drawing.Point(124, 10);
			this._anyCharRequiredButton.Name = "button9";
			this._anyCharRequiredButton.Size = new System.Drawing.Size(20, 20);
			this._anyCharRequiredButton.TabIndex = 14;
			this._anyCharRequiredButton.TabStop = false;
			this._anyCharRequiredButton.Tag = "&";
			this._anyCharRequiredButton.Text = "&&";
			this._maskToolTip.SetToolTip(this._anyCharRequiredButton, "Any character or a space. (Entry required)");
			this._anyCharRequiredButton.Click += new System.EventHandler(btnMask_Click);
			this._anyCharRequiredButton.Enter += new System.EventHandler(button10_Enter);
			this._alphanumericOptionalButton.Location = new System.Drawing.Point(104, 10);
			this._alphanumericOptionalButton.Name = "button8";
			this._alphanumericOptionalButton.Size = new System.Drawing.Size(20, 20);
			this._alphanumericOptionalButton.TabIndex = 13;
			this._alphanumericOptionalButton.TabStop = false;
			this._alphanumericOptionalButton.Tag = "a";
			this._alphanumericOptionalButton.Text = "a";
			this._maskToolTip.SetToolTip(this._alphanumericOptionalButton, "Letter or digit. (Entry optional)");
			this._alphanumericOptionalButton.Click += new System.EventHandler(btnMask_Click);
			this._alphanumericOptionalButton.Enter += new System.EventHandler(button10_Enter);
			this._alphanumericRequiredButton.Location = new System.Drawing.Point(84, 10);
			this._alphanumericRequiredButton.Name = "button7";
			this._alphanumericRequiredButton.Size = new System.Drawing.Size(20, 20);
			this._alphanumericRequiredButton.TabIndex = 12;
			this._alphanumericRequiredButton.TabStop = false;
			this._alphanumericRequiredButton.Tag = "A";
			this._alphanumericRequiredButton.Text = "A";
			this._maskToolTip.SetToolTip(this._alphanumericRequiredButton, "Letter or digit. (Entry required)");
			this._alphanumericRequiredButton.Click += new System.EventHandler(btnMask_Click);
			this._alphanumericRequiredButton.Enter += new System.EventHandler(button10_Enter);
			this._letterOptionalButton.Location = new System.Drawing.Point(64, 10);
			this._letterOptionalButton.Name = "button6";
			this._letterOptionalButton.Size = new System.Drawing.Size(20, 20);
			this._letterOptionalButton.TabIndex = 11;
			this._letterOptionalButton.TabStop = false;
			this._letterOptionalButton.Tag = "?";
			this._letterOptionalButton.Text = "?";
			this._maskToolTip.SetToolTip(this._letterOptionalButton, "Letter (A – Z). (Entry optional)");
			this._letterOptionalButton.Click += new System.EventHandler(btnMask_Click);
			this._letterOptionalButton.Enter += new System.EventHandler(button10_Enter);
			this._letterRequiredButton.Location = new System.Drawing.Point(44, 10);
			this._letterRequiredButton.Name = "button5";
			this._letterRequiredButton.Size = new System.Drawing.Size(20, 20);
			this._letterRequiredButton.TabIndex = 10;
			this._letterRequiredButton.TabStop = false;
			this._letterRequiredButton.Tag = "L";
			this._letterRequiredButton.Text = "L";
			this._maskToolTip.SetToolTip(this._letterRequiredButton, "Letter (A – Z). (Entry required)");
			this._letterRequiredButton.Click += new System.EventHandler(btnMask_Click);
			this._letterRequiredButton.Enter += new System.EventHandler(button10_Enter);
			this._digitOptionalButton.Location = new System.Drawing.Point(24, 10);
			this._digitOptionalButton.Name = "button4";
			this._digitOptionalButton.Size = new System.Drawing.Size(20, 20);
			this._digitOptionalButton.TabIndex = 9;
			this._digitOptionalButton.TabStop = false;
			this._digitOptionalButton.Tag = "#";
			this._digitOptionalButton.Text = "#";
			this._maskToolTip.SetToolTip(this._digitOptionalButton, "Digit or space. (Entry optional)");
			this._digitOptionalButton.Click += new System.EventHandler(btnMask_Click);
			this._digitOptionalButton.Enter += new System.EventHandler(button10_Enter);
			this.chkIncludeLiterals.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.chkIncludeLiterals.Checked = true;
			this.chkIncludeLiterals.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkIncludeLiterals.Location = new System.Drawing.Point(187, 66);
			this.chkIncludeLiterals.Name = "chkIncludeLiterals";
			this.chkIncludeLiterals.Size = new System.Drawing.Size(109, 21);
			this.chkIncludeLiterals.TabIndex = 3;
			this.chkIncludeLiterals.Text = "Include Literals:";
			this.lblMaskPrompt.AutoSize = true;
			this.lblMaskPrompt.Location = new System.Drawing.Point(5, 66);
			this.lblMaskPrompt.Name = "lblMaskPrompt";
			this.lblMaskPrompt.Size = new System.Drawing.Size(73, 14);
			this.lblMaskPrompt.TabIndex = 3;
			this.lblMaskPrompt.Text = "Mask Prompt:";
			this.cboMaskPrompt.Items.AddRange(new object[4] { "_", "<Space>", "*", "#" });
			this.cboMaskPrompt.Location = new System.Drawing.Point(80, 64);
			this.cboMaskPrompt.MaxLength = 1;
			this.cboMaskPrompt.Name = "cboMaskPrompt";
			this.cboMaskPrompt.Size = new System.Drawing.Size(103, 21);
			this.cboMaskPrompt.TabIndex = 2;
			this.cboMaskPrompt.TextChanged += new System.EventHandler(cboMaskPrompt_TextChanged);
			this.txtMask.Location = new System.Drawing.Point(50, 37);
			this.txtMask.Name = "txtMask";
			this.txtMask.Size = new System.Drawing.Size(157, 21);
			this.txtMask.TabIndex = 1;
			this.txtMask.Text = "";
			this.txtMask.TextChanged += new System.EventHandler(txtMask_TextChanged);
			this.lblMask.AutoSize = true;
			this.lblMask.Location = new System.Drawing.Point(7, 39);
			this.lblMask.Name = "lblMask";
			this.lblMask.Size = new System.Drawing.Size(34, 14);
			this.lblMask.TabIndex = 0;
			this.lblMask.Text = "Mask:";
			this._digitRequiredButton.Location = new System.Drawing.Point(4, 10);
			this._digitRequiredButton.Name = "button3";
			this._digitRequiredButton.Size = new System.Drawing.Size(20, 20);
			this._digitRequiredButton.TabIndex = 8;
			this._digitRequiredButton.TabStop = false;
			this._digitRequiredButton.Tag = "0";
			this._digitRequiredButton.Text = "0";
			this._maskToolTip.SetToolTip(this._digitRequiredButton, "Digit (0-9). (Entry required)");
			this._digitRequiredButton.Click += new System.EventHandler(btnMask_Click);
			this._digitRequiredButton.Enter += new System.EventHandler(button10_Enter);
			this._buttonImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this._buttonImages.ImageSize = new System.Drawing.Size(1, 16);
			this._buttonImages.TransparentColor = System.Drawing.Color.Transparent;
			this._cancelButton.Location = new System.Drawing.Point(316, 55);
			this._cancelButton.Name = "button2";
			this._cancelButton.Size = new System.Drawing.Size(80, 24);
			this._cancelButton.TabIndex = 6;
			this._cancelButton.Text = "Cancel";
			this._cancelButton.Click += new System.EventHandler(button2_Click);
			this.grpNumeric.Controls.AddRange(new System.Windows.Forms.Control[3] { this._decimalsHintLabel, this.lblDecimals, this.neDecimalDigits });
			this.grpNumeric.Location = new System.Drawing.Point(7, 175);
			this.grpNumeric.Name = "grpNumeric";
			this.grpNumeric.Size = new System.Drawing.Size(301, 92);
			this.grpNumeric.TabIndex = 6;
			this.grpNumeric.TabStop = false;
			this.grpNumeric.Visible = false;
			this._decimalsHintLabel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
			this._decimalsHintLabel.Location = new System.Drawing.Point(7, 39);
			this._decimalsHintLabel.Name = "label2";
			this._decimalsHintLabel.Size = new System.Drawing.Size(287, 43);
			this._decimalsHintLabel.TabIndex = 2;
			this._decimalsHintLabel.Text = "If -1 is used, the number of decimals specified in the regional settings is used or 0 if the Type of the value is an integral type.";
			this.lblDecimals.AutoSize = true;
			this.lblDecimals.Location = new System.Drawing.Point(10, 17);
			this.lblDecimals.Name = "lblDecimals";
			this.lblDecimals.Size = new System.Drawing.Size(79, 14);
			this.lblDecimals.TabIndex = 1;
			this.lblDecimals.Text = "Decimal Digits:";
			this.neDecimalDigits.BorderStyle = Mordex.Windows.DataGrid.BorderStyle.SunkenLight3D;
			this.neDecimalDigits.Location = new System.Drawing.Point(96, 14);
			this.neDecimalDigits.Maximum = 20;
			this.neDecimalDigits.Minimum = -1;
			this.neDecimalDigits.Name = "neDecimalDigits";
			this.neDecimalDigits.Size = new System.Drawing.Size(48, 21);
			this.neDecimalDigits.TabIndex = 0;
			this.neDecimalDigits.TextAlignment = Mordex.Windows.DataGrid.TextAlignment.Center;
			this.neDecimalDigits.UpDownStyle = Mordex.Windows.DataGrid.UpDownStyle.UpDownList;
			this._commonMasksLabel.Location = new System.Drawing.Point(9, 5);
			this._commonMasksLabel.Name = "label3";
			this._commonMasksLabel.Size = new System.Drawing.Size(242, 18);
			this._commonMasksLabel.TabIndex = 7;
			this._commonMasksLabel.Text = "Common Masks:";
			this._commonMasksLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._maskToolTip.ShowAlways = true;
			base.ClientSize = new System.Drawing.Size(402, 308);
			base.ControlBox = false;
			base.Controls.AddRange(new System.Windows.Forms.Control[7] { this._commonMasksLabel, this._cancelButton, this._okButton, this._masksGrid, this.grpTryIt, this.grpMaskEdit, this.grpNumeric });
			this.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Name = "InputMaskDesigner";
			this.Text = "Input Mask";
			((System.ComponentModel.ISupportInitialize)this._masksGrid).EndInit();
			this.grpTryIt.ResumeLayout(false);
			this.grpMaskEdit.ResumeLayout(false);
			this.grpNumeric.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		internal void Initialize()
		{
			BindingContext = new BindingContext();
			FillGrid();
		}

		private void FillGrid()
		{
			string text = "A";
			string otherText = "C";
			string nextText = "B";
			GridColumn gridColumn = _masksGrid.RootTable.Columns["Category"];
			gridColumn.HasValueList = true;
			gridColumn.ValueList.Add(new GridValueListItem("A", "Text Masks"));
			gridColumn.ValueList.Add(new GridValueListItem("C", "DateTime Masks"));
			gridColumn.ValueList.Add(new GridValueListItem("B", "Numeric Masks"));
			if (includeNumericFormats)
			{
				masks = new string[11][];
			}
			else
			{
				masks = new string[7][];
			}
			masks[0] = new string[4] { "Phone Number", "!(###) 000-0000", "(345) 562-5781", text };
			masks[1] = new string[4] { "Social Security Number", "000-00-0000", "568-24-5875", text };
			masks[2] = new string[4] { "ZIP Code", "00000-####", "95280-7966", text };
			masks[3] = new string[4] { "Short Date", "00/00/0000", "09/12/2002", otherText };
			masks[4] = new string[4] { "Medium Date", "00-LLL-00", "9/Dec/2002", otherText };
			masks[5] = new string[4] { "Short Time", "00:00", "13:00", otherText };
			masks[6] = new string[4] { "Medium Time", "00:00 LL", "01:00 PM", otherText };
			if (includeNumericFormats)
			{
				masks[7] = new string[4]
				{
					"General",
					"General",
					1450f.ToString("G"),
					nextText
				};
				masks[8] = new string[4]
				{
					"Number",
					"Number",
					1450f.ToString("N"),
					nextText
				};
				masks[9] = new string[4]
				{
					"Currency",
					"Currency",
					1450f.ToString("C"),
					nextText
				};
				masks[10] = new string[4]
				{
					"Percent",
					"Percent",
					0.25f.ToString("P"),
					nextText
				};
			}
			_masksGrid.DataSource = masks;
		}

		private void grid1_LoadingRow(object sender, RowLoadEventArgs eventArgs)
		{
			string[] array = (string[])eventArgs.Row.DataRow;
			eventArgs.Row.Cells[0].Value = array[0];
			eventArgs.Row.Cells[1].Value = array[2];
			eventArgs.Row.Cells[2].Value = array[3];
		}

		private void button1_Click(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.OK;
			Hide();
		}

		private void grid1_SelectionChanged(object sender, EventArgs eventArgs)
		{
			GridRow row = _masksGrid.GetRow();
			if (row != null && row.RowType == RowType.Record)
			{
				string[] array = (string[])row.DataRow;
				if (array[3] == "B")
				{
					IsNumericMask = true;
				}
				else
				{
					IsNumericMask = false;
					txtMask.Text = array[1];
				}
				inputMask = array[1];
			}
			else
			{
				IsNumericMask = false;
				txtMask.Text = "";
			}
			SetTryProperties();
		}

		private static bool IsMaskNumeric(string mask)
		{
			string text = "CURRENCY";
			string value = "PERCENT";
			string otherValue = "GENERAL";
			string nextValue = "NUMBER";
			mask = mask.ToUpper();
			if (mask.StartsWith(text))
			{
				if (mask == text)
				{
					return true;
				}
				mask = mask.Substring(text.Length);
			}
			else if (mask.StartsWith(value))
			{
				if (mask == value)
				{
					return true;
				}
				mask = mask.Substring(value.Length);
			}
			else if (mask.StartsWith(otherValue))
			{
				if (mask == otherValue)
				{
					return true;
				}
				mask = mask.Substring(otherValue.Length);
			}
			else
			{
				if (!mask.StartsWith(nextValue))
				{
					return false;
				}
				if (mask == nextValue)
				{
					return true;
				}
				mask = mask.Substring(nextValue.Length);
			}
			try
			{
				int.Parse(mask);
				return true;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return false;
			}
		}

		private void SetTryProperties()
		{
			if (isNumericMask)
			{
				switch (inputMask.ToUpper(CultureInfo.InvariantCulture))
				{
				case "GENERAL":
					_tryNumericEdit.FormatMask = NumericEditFormatMask.General;
					break;
				case "CURRENCY":
					_tryNumericEdit.FormatMask = NumericEditFormatMask.Currency;
					break;
				case "NUMBER":
					_tryNumericEdit.FormatMask = NumericEditFormatMask.Number;
					break;
				case "PERCENT":
					_tryNumericEdit.FormatMask = NumericEditFormatMask.Percent;
					break;
				}
				_tryNumericEdit.DecimalDigits = neDecimalDigits.Value;
			}
			else
			{
				_tryMaskEdit.Text = "";
				try
				{
					_tryMaskEdit.PromptChar = char.Parse(MaskPrompt);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					_tryMaskEdit.PromptChar = '_';
				}
				_tryMaskEdit.IncludeLiterals = IncludeLiterals;
				_tryMaskEdit.Mask = txtMask.Text;
			}
		}

		private void txtMask_TextChanged(object sender, EventArgs eventArgs)
		{
			inputMask = txtMask.Text;
			SetTryProperties();
		}

		private void button2_Click(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.Cancel;
			Hide();
		}

		private void grid1_FormattingRow(object sender, RowLoadEventArgs eventArgs)
		{
		}

		private void cboMaskPrompt_TextChanged(object sender, EventArgs eventArgs)
		{
			_tryMaskEdit.PromptChar = char.Parse(MaskPrompt);
		}

		private void btnMask_Click(object sender, EventArgs eventArgs)
		{
			Button button = (Button)sender;
			txtMask.SelectedText = (string)button.Tag;
		}

		private void button15_Click(object sender, EventArgs eventArgs)
		{
			if (!txtMask.Text.StartsWith("!"))
			{
				txtMask.Text = "!" + txtMask.Text;
			}
		}

		private void button10_Enter(object sender, EventArgs eventArgs)
		{
			txtMask.Focus();
		}

		private void button17_Click(object sender, EventArgs eventArgs)
		{
			txtMask.Text = "";
			txtMask.Focus();
		}

		private void numericEdit1_Validating(object sender, CancelEventArgs eventArgs)
		{
		}

		private void maskEdit1_Validating(object sender, CancelEventArgs eventArgs)
		{
			if (!_tryMaskEdit.IsTextValid())
			{
				MessageBox.Show("'" + _tryMaskEdit.Text + "' is not valid for Mask: '" + _tryMaskEdit.Mask + "'");
			}
		}
	}
}
