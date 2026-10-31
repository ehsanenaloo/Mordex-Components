using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Design
{
	internal class InputMaskDialog : Form
	{
		private MxDataGrid _maskGrid;

		private Label _label;

		private MaskEdit _maskPreviewEdit;

		private Button _button;

		private GroupBox _grpTryIt;

		private NumericEdit _numericPreviewEdit;

		private Button _inputMaskDialogButton;

		private GroupBox _grpMaskEdit;

		private TextBox _txtMask;

		private CheckBox _chkIncludeLiterals;

		private ComboBox _cboMaskPrompt;

		private Label _lblMaskPrompt;

		private Label _lblMask;

		private GroupBox _grpNumeric;

		private Label _lblDecimals;

		private Label _inputMaskDialogLabel;

		private MxIntegerUpDown _neDecimalDigits;

		private Label _lblCommonMasks;

		private ImageList _imageList;

		private ToolTip _toolTip;

		private Button _btnInsertZero;

		private Button _btnInsertHash;

		private Button _btnInsertLetterL;

		private Button _btnInsertQuestion;

		private Button _btnInsertUpperA;

		private Button _btnInsertLowerA;

		private Button _btnInsertAmpersand;

		private Button _btnInsertUpperC;

		private Button _btnInsertDot;

		private Button _btnInsertComma;

		private Button _btnInsertColon;

		private Button _btnInsertSlash;

		private Button _btnInsertBang;

		private Button _btnInsertBackslash;

		private Button _btnClearMask;

		private IContainer _iContainer;

		private string _previewText = "";

		private bool _isNumericMode;

		internal InputMaskDialog()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			GridLayout gridLayout = new GridLayout();
			_maskGrid = new MxDataGrid();
			_grpTryIt = new GroupBox();
			_label = new Label();
			_maskPreviewEdit = new MaskEdit();
			_numericPreviewEdit = new NumericEdit();
			_button = new Button();
			_grpMaskEdit = new GroupBox();
			_btnClearMask = new Button();
			_btnInsertBackslash = new Button();
			_btnInsertBang = new Button();
			_btnInsertSlash = new Button();
			_btnInsertColon = new Button();
			_btnInsertComma = new Button();
			_btnInsertDot = new Button();
			_btnInsertUpperC = new Button();
			_btnInsertAmpersand = new Button();
			_btnInsertLowerA = new Button();
			_btnInsertUpperA = new Button();
			_btnInsertQuestion = new Button();
			_btnInsertLetterL = new Button();
			_btnInsertHash = new Button();
			_chkIncludeLiterals = new CheckBox();
			_lblMaskPrompt = new Label();
			_cboMaskPrompt = new ComboBox();
			_txtMask = new TextBox();
			_lblMask = new Label();
			_btnInsertZero = new Button();
			_imageList = new ImageList(_iContainer);
			_inputMaskDialogButton = new Button();
			_grpNumeric = new GroupBox();
			_inputMaskDialogLabel = new Label();
			_lblDecimals = new Label();
			_neDecimalDigits = new MxIntegerUpDown();
			_lblCommonMasks = new Label();
			_toolTip = new ToolTip(_iContainer);
			((ISupportInitialize)_maskGrid).BeginInit();
			_grpTryIt.SuspendLayout();
			_grpMaskEdit.SuspendLayout();
			_grpNumeric.SuspendLayout();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_maskGrid.BuiltInTextsData = "<LocalizableData ID=\"LocalizableStrings\" Collection=\"true\"><GroupByBoxInfo>Which input mask matches how  you want data to look</GroupByBoxInfo></LocalizableData>";
			_maskGrid.ColumnAutoResize = true;
			gridLayout.LayoutString = "<GridLayoutData><RootTable><Columns Collection=\"true\"><Column0 ID=\"InputMask\"><Bound>False</Bound><Caption>Input Mask:</Caption><EditType>CalendarDropDown</EditType><Key>InputMask</Key><Position>0</Position><Selectable>False</Selectable><Width>155</Width></Column0><Column1 ID=\"SampleData\"><Bound>False</Bound><Caption>Sample Data:</Caption><Key>SampleData</Key><Position>1</Position><Selectable>False</Selectable><Width>108</Width></Column1><Column2 ID=\"Category\"><Bound>False</Bound><Caption>Category</Caption><DefaultGroupPrefix /><Key>Category</Key><Position>2</Position><Visible>False</Visible></Column2></Columns><GroupCondition ID=\"\" /><Groups Collection=\"true\"><Group0 ID=\"Group0\"><ColIndex>2</ColIndex><GroupInterval>Value</GroupInterval></Group0></Groups></RootTable></GridLayoutData>";
			_maskGrid.DesignTimeLayout = gridLayout;
			_maskGrid.EditorsControlStyle.ButtonAppearance = ButtonAppearance.Regular;
			_maskGrid.EmptyRows = true;
			_maskGrid.ExpandableGroups = InheritableBoolean.False;
			_maskGrid.FlatBorderColor = SystemColors.ControlDark;
			_maskGrid.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_maskGrid.GridLineColor = SystemColors.Control;
			_maskGrid.GridLines = GridLines.None;
			_maskGrid.GridLineStyle = GridLineStyle.Solid;
			_maskGrid.GroupByBoxFormatStyle.BackColor = SystemColors.Control;
			_maskGrid.GroupByBoxInfoFormatStyle.BackColor = SystemColors.Info;
			_maskGrid.GroupByBoxInfoFormatStyle.ForeColor = SystemColors.InfoText;
			_maskGrid.GroupByBoxVisible = false;
			_maskGrid.GroupRowFormatStyle.FontBold = TriState.True;
			_maskGrid.HeaderFormatStyle.Appearance = Appearance.Flat;
			_maskGrid.HideSelection = HideSelection.Highlight;
			_maskGrid.Location = new Point(7, 26);
			_maskGrid.Name = "grid1";
			_maskGrid.ScrollBars = ScrollBars.Vertical;
			_maskGrid.Size = new Size(300, 150);
			_maskGrid.TabIndex = 0;
			_maskGrid.TabKeyBehavior = TabKeyBehavior.ControlNavigation;
			_maskGrid.FormattingRow += OnGrid1FormattingRow;
			_maskGrid.DoubleClick += OnGrid1SelectionChanged;
			_maskGrid.SelectionChanged += OnGrid1SelectionChanged;
			_maskGrid.LoadingRow += OnGrid1LoadingRow;
			_grpTryIt.Controls.AddRange(new Control[3] { _label, _maskPreviewEdit, _numericPreviewEdit });
			_grpTryIt.Location = new Point(7, 267);
			_grpTryIt.Name = "grpTryIt";
			_grpTryIt.Size = new Size(301, 36);
			_grpTryIt.TabIndex = 2;
			_grpTryIt.TabStop = false;
			_label.Location = new Point(6, 10);
			_label.Name = "label1";
			_label.Size = new Size(42, 20);
			_label.TabIndex = 3;
			_label.Text = "Try It:";
			_maskPreviewEdit.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_maskPreviewEdit.Location = new Point(50, 13);
			_maskPreviewEdit.Name = "maskEdit1";
			_maskPreviewEdit.Size = new Size(245, 21);
			_maskPreviewEdit.TabIndex = 4;
			_maskPreviewEdit.Text = "";
			_maskPreviewEdit.Validating += OnMaskEdit1Validating;
			_numericPreviewEdit.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_numericPreviewEdit.Location = new Point(50, 11);
			_numericPreviewEdit.Name = "numericEdit1";
			_numericPreviewEdit.Size = new Size(245, 21);
			_numericPreviewEdit.TabIndex = 4;
			_numericPreviewEdit.Text = "0.00";
			NumericEdit numericEdit = _numericPreviewEdit;
			int[] bits = new int[4];
			numericEdit.Value = new decimal(bits);
			_numericPreviewEdit.Visible = false;
			_numericPreviewEdit.WordWrap = false;
			_numericPreviewEdit.Validating += OnNumericEdit1Validating;
			_button.Location = new Point(316, 28);
			_button.Name = "button1";
			_button.Size = new Size(80, 24);
			_button.TabIndex = 5;
			_button.Text = "OK";
			_button.Click += OnButtonClick;
			_grpMaskEdit.Controls.AddRange(new Control[20]
			{
				_btnClearMask, _btnInsertBackslash, _btnInsertBang, _btnInsertSlash, _btnInsertColon, _btnInsertComma, _btnInsertDot, _btnInsertUpperC, _btnInsertAmpersand, _btnInsertLowerA,
				_btnInsertUpperA, _btnInsertQuestion, _btnInsertLetterL, _btnInsertHash, _chkIncludeLiterals, _lblMaskPrompt, _cboMaskPrompt, _txtMask, _lblMask, _btnInsertZero
			});
			_grpMaskEdit.Location = new Point(7, 175);
			_grpMaskEdit.Name = "grpMaskEdit";
			_grpMaskEdit.Size = new Size(301, 92);
			_grpMaskEdit.TabIndex = 1;
			_grpMaskEdit.TabStop = false;
			_btnClearMask.Location = new Point(210, 37);
			_btnClearMask.Name = "button17";
			_btnClearMask.Size = new Size(72, 22);
			_btnClearMask.TabIndex = 22;
			_btnClearMask.TabStop = false;
			_btnClearMask.Text = "Clear";
			_btnClearMask.Click += OnClearMaskClick;
			_btnInsertBackslash.Location = new Point(264, 10);
			_btnInsertBackslash.Name = "button16";
			_btnInsertBackslash.Size = new Size(20, 20);
			_btnInsertBackslash.TabIndex = 21;
			_btnInsertBackslash.TabStop = false;
			_btnInsertBackslash.Tag = "\\";
			_btnInsertBackslash.Text = "\\";
			_toolTip.SetToolTip(_btnInsertBackslash, "Causes the character that follows to be displayed as a literal character. Used to display any of the characters listed in this table as literal characters (for example, \\C is displayed as just C).");
			_btnInsertBackslash.Click += OnInsertMaskCharClick;
			_btnInsertBackslash.Enter += OnButtonEnter;
			_btnInsertBang.Location = new Point(244, 10);
			_btnInsertBang.Name = "button15";
			_btnInsertBang.Size = new Size(20, 20);
			_btnInsertBang.TabIndex = 20;
			_btnInsertBang.TabStop = false;
			_btnInsertBang.Tag = "!";
			_btnInsertBang.Text = "!";
			_toolTip.SetToolTip(_btnInsertBang, "You can include the exclamation point anywhere in the input mask. When this character is used in a mask, characters are displayed from right to left, rather than from left to right when reading a text with non required characters.  *Characters typed into the mask always fill from left to right. ");
			_btnInsertBang.Click += OnInsertBangClick;
			_btnInsertBang.Enter += OnButtonEnter;
			_btnInsertSlash.Location = new Point(224, 10);
			_btnInsertSlash.Name = "button14";
			_btnInsertSlash.Size = new Size(20, 20);
			_btnInsertSlash.TabIndex = 19;
			_btnInsertSlash.TabStop = false;
			_btnInsertSlash.Tag = "/";
			_btnInsertSlash.Text = "/";
			_toolTip.SetToolTip(_btnInsertSlash, "Date separator placeholder. (The actual character used depends on the regional settings.)");
			_btnInsertSlash.Click += OnInsertMaskCharClick;
			_btnInsertSlash.Enter += OnButtonEnter;
			_btnInsertColon.Location = new Point(204, 10);
			_btnInsertColon.Name = "button13";
			_btnInsertColon.Size = new Size(20, 20);
			_btnInsertColon.TabIndex = 18;
			_btnInsertColon.TabStop = false;
			_btnInsertColon.Tag = ":";
			_btnInsertColon.Text = ":";
			_toolTip.SetToolTip(_btnInsertColon, "Time separator placeholder. (The actual character used depends on the regional settings.)");
			_btnInsertColon.Click += OnInsertMaskCharClick;
			_btnInsertColon.Enter += OnButtonEnter;
			_btnInsertComma.Location = new Point(184, 10);
			_btnInsertComma.Name = "button12";
			_btnInsertComma.Size = new Size(20, 20);
			_btnInsertComma.TabIndex = 17;
			_btnInsertComma.TabStop = false;
			_btnInsertComma.Tag = ",";
			_btnInsertComma.Text = ",";
			_toolTip.SetToolTip(_btnInsertComma, "Thousands separator placeholder. (The actual character used depends on the regional settings.)");
			_btnInsertComma.Click += OnInsertMaskCharClick;
			_btnInsertComma.Enter += OnButtonEnter;
			_btnInsertDot.Location = new Point(164, 10);
			_btnInsertDot.Name = "button11";
			_btnInsertDot.Size = new Size(20, 20);
			_btnInsertDot.TabIndex = 16;
			_btnInsertDot.TabStop = false;
			_btnInsertDot.Tag = ".";
			_btnInsertDot.Text = ".";
			_toolTip.SetToolTip(_btnInsertDot, "Decimal separator placeholder. (The actual character used depends on the regional settings.)");
			_btnInsertDot.Click += OnInsertMaskCharClick;
			_btnInsertDot.Enter += OnButtonEnter;
			_btnInsertUpperC.Location = new Point(144, 10);
			_btnInsertUpperC.Name = "button10";
			_btnInsertUpperC.Size = new Size(20, 20);
			_btnInsertUpperC.TabIndex = 15;
			_btnInsertUpperC.TabStop = false;
			_btnInsertUpperC.Tag = "C";
			_btnInsertUpperC.Text = "C";
			_toolTip.SetToolTip(_btnInsertUpperC, "Any character or a space. (Entry optional)");
			_btnInsertUpperC.Click += OnInsertMaskCharClick;
			_btnInsertUpperC.Enter += OnButtonEnter;
			_btnInsertAmpersand.Location = new Point(124, 10);
			_btnInsertAmpersand.Name = "button9";
			_btnInsertAmpersand.Size = new Size(20, 20);
			_btnInsertAmpersand.TabIndex = 14;
			_btnInsertAmpersand.TabStop = false;
			_btnInsertAmpersand.Tag = "&";
			_btnInsertAmpersand.Text = "&&";
			_toolTip.SetToolTip(_btnInsertAmpersand, "Any character or a space. (Entry required)");
			_btnInsertAmpersand.Click += OnInsertMaskCharClick;
			_btnInsertAmpersand.Enter += OnButtonEnter;
			_btnInsertLowerA.Location = new Point(104, 10);
			_btnInsertLowerA.Name = "button8";
			_btnInsertLowerA.Size = new Size(20, 20);
			_btnInsertLowerA.TabIndex = 13;
			_btnInsertLowerA.TabStop = false;
			_btnInsertLowerA.Tag = "a";
			_btnInsertLowerA.Text = "a";
			_toolTip.SetToolTip(_btnInsertLowerA, "Letter or digit. (Entry optional)");
			_btnInsertLowerA.Click += OnInsertMaskCharClick;
			_btnInsertLowerA.Enter += OnButtonEnter;
			_btnInsertUpperA.Location = new Point(84, 10);
			_btnInsertUpperA.Name = "button7";
			_btnInsertUpperA.Size = new Size(20, 20);
			_btnInsertUpperA.TabIndex = 12;
			_btnInsertUpperA.TabStop = false;
			_btnInsertUpperA.Tag = "A";
			_btnInsertUpperA.Text = "A";
			_toolTip.SetToolTip(_btnInsertUpperA, "Letter or digit. (Entry required)");
			_btnInsertUpperA.Click += OnInsertMaskCharClick;
			_btnInsertUpperA.Enter += OnButtonEnter;
			_btnInsertQuestion.Location = new Point(64, 10);
			_btnInsertQuestion.Name = "button6";
			_btnInsertQuestion.Size = new Size(20, 20);
			_btnInsertQuestion.TabIndex = 11;
			_btnInsertQuestion.TabStop = false;
			_btnInsertQuestion.Tag = "?";
			_btnInsertQuestion.Text = "?";
			_toolTip.SetToolTip(_btnInsertQuestion, "Letter (A – Z). (Entry optional)");
			_btnInsertQuestion.Click += OnInsertMaskCharClick;
			_btnInsertQuestion.Enter += OnButtonEnter;
			_btnInsertLetterL.Location = new Point(44, 10);
			_btnInsertLetterL.Name = "button5";
			_btnInsertLetterL.Size = new Size(20, 20);
			_btnInsertLetterL.TabIndex = 10;
			_btnInsertLetterL.TabStop = false;
			_btnInsertLetterL.Tag = "L";
			_btnInsertLetterL.Text = "L";
			_toolTip.SetToolTip(_btnInsertLetterL, "Letter (A – Z). (Entry required)");
			_btnInsertLetterL.Click += OnInsertMaskCharClick;
			_btnInsertLetterL.Enter += OnButtonEnter;
			_btnInsertHash.Location = new Point(24, 10);
			_btnInsertHash.Name = "button4";
			_btnInsertHash.Size = new Size(20, 20);
			_btnInsertHash.TabIndex = 9;
			_btnInsertHash.TabStop = false;
			_btnInsertHash.Tag = "#";
			_btnInsertHash.Text = "#";
			_toolTip.SetToolTip(_btnInsertHash, "Digit or space. (Entry optional)");
			_btnInsertHash.Click += OnInsertMaskCharClick;
			_btnInsertHash.Enter += OnButtonEnter;
			_chkIncludeLiterals.CheckAlign = ContentAlignment.MiddleRight;
			_chkIncludeLiterals.Checked = true;
			_chkIncludeLiterals.CheckState = CheckState.Checked;
			_chkIncludeLiterals.Location = new Point(187, 66);
			_chkIncludeLiterals.Name = "chkIncludeLiterals";
			_chkIncludeLiterals.Size = new Size(109, 21);
			_chkIncludeLiterals.TabIndex = 3;
			_chkIncludeLiterals.Text = "Include Literals:";
			_lblMaskPrompt.AutoSize = true;
			_lblMaskPrompt.Location = new Point(5, 66);
			_lblMaskPrompt.Name = "lblMaskPrompt";
			_lblMaskPrompt.Size = new Size(73, 14);
			_lblMaskPrompt.TabIndex = 3;
			_lblMaskPrompt.Text = "Mask Prompt:";
			_cboMaskPrompt.Items.AddRange(new object[4]
			{
				"_",
				"<Space>",
				"*",
				"#"
			});
			_cboMaskPrompt.Location = new Point(80, 64);
			_cboMaskPrompt.MaxLength = 1;
			_cboMaskPrompt.Name = "cboMaskPrompt";
			_cboMaskPrompt.Size = new Size(103, 21);
			_cboMaskPrompt.TabIndex = 2;
			_cboMaskPrompt.TextChanged += OnCboMaskPromptTextChanged;
			_txtMask.Location = new Point(50, 37);
			_txtMask.Name = "txtMask";
			_txtMask.Size = new Size(157, 21);
			_txtMask.TabIndex = 1;
			_txtMask.Text = "";
			_txtMask.TextChanged += OnTxtMaskTextChanged;
			_lblMask.AutoSize = true;
			_lblMask.Location = new Point(7, 39);
			_lblMask.Name = "lblMask";
			_lblMask.Size = new Size(34, 14);
			_lblMask.TabIndex = 0;
			_lblMask.Text = "Mask:";
			_btnInsertZero.Location = new Point(4, 10);
			_btnInsertZero.Name = "button3";
			_btnInsertZero.Size = new Size(20, 20);
			_btnInsertZero.TabIndex = 8;
			_btnInsertZero.TabStop = false;
			_btnInsertZero.Tag = "0";
			_btnInsertZero.Text = "0";
			_toolTip.SetToolTip(_btnInsertZero, "Digit (0-9). (Entry required)");
			_btnInsertZero.Click += OnInsertMaskCharClick;
			_btnInsertZero.Enter += OnButtonEnter;
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(1, 16);
			_imageList.TransparentColor = Color.Transparent;
			_inputMaskDialogButton.Location = new Point(316, 55);
			_inputMaskDialogButton.Name = "button2";
			_inputMaskDialogButton.Size = new Size(80, 24);
			_inputMaskDialogButton.TabIndex = 6;
			_inputMaskDialogButton.Text = "Cancel";
			_inputMaskDialogButton.Click += OnCancelClick;
			_grpNumeric.Controls.AddRange(new Control[3] { _inputMaskDialogLabel, _lblDecimals, _neDecimalDigits });
			_grpNumeric.Location = new Point(7, 175);
			_grpNumeric.Name = "grpNumeric";
			_grpNumeric.Size = new Size(301, 92);
			_grpNumeric.TabIndex = 6;
			_grpNumeric.TabStop = false;
			_grpNumeric.Visible = false;
			_inputMaskDialogLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Italic, GraphicsUnit.Point, 0);
			_inputMaskDialogLabel.Location = new Point(7, 39);
			_inputMaskDialogLabel.Name = "label2";
			_inputMaskDialogLabel.Size = new Size(287, 43);
			_inputMaskDialogLabel.TabIndex = 2;
			_inputMaskDialogLabel.Text = "If -1 is used, the number of decimals specified in the regional settings is used or 0 if the Type of the value is an integral type.";
			_lblDecimals.AutoSize = true;
			_lblDecimals.Location = new Point(10, 17);
			_lblDecimals.Name = "lblDecimals";
			_lblDecimals.Size = new Size(79, 14);
			_lblDecimals.TabIndex = 1;
			_lblDecimals.Text = "Decimal Digits:";
			_neDecimalDigits.BorderStyle = BorderStyle.SunkenLight3D;
			_neDecimalDigits.Location = new Point(96, 14);
			_neDecimalDigits.Maximum = 20;
			_neDecimalDigits.Minimum = -1;
			_neDecimalDigits.Name = "neDecimalDigits";
			_neDecimalDigits.Size = new Size(48, 21);
			_neDecimalDigits.TabIndex = 0;
			_neDecimalDigits.TextAlignment = TextAlignment.Center;
			_neDecimalDigits.UpDownStyle = UpDownStyle.UpDownList;
			_lblCommonMasks.Location = new Point(9, 5);
			_lblCommonMasks.Name = "label3";
			_lblCommonMasks.Size = new Size(242, 18);
			_lblCommonMasks.TabIndex = 7;
			_lblCommonMasks.Text = "Common Masks:";
			_lblCommonMasks.TextAlign = ContentAlignment.MiddleLeft;
			_toolTip.ShowAlways = true;
			base.ClientSize = new Size(402, 308);
			base.ControlBox = false;
			base.Controls.AddRange(new Control[7] { _lblCommonMasks, _inputMaskDialogButton, _button, _maskGrid, _grpTryIt, _grpMaskEdit, _grpNumeric });
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.Name = "InputMaskDesigner";
			Text = "Input Mask";
			((ISupportInitialize)_maskGrid).EndInit();
			_grpTryIt.ResumeLayout(performLayout: false);
			_grpMaskEdit.ResumeLayout(performLayout: false);
			_grpNumeric.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		internal bool GetChecked()
		{
			return _chkIncludeLiterals.Checked;
		}

		internal string GetPromptCharText()
		{
			if (_cboMaskPrompt.Text.Length == 0)
			{
				return "_";
			}
			if (_cboMaskPrompt.Text == (string)_cboMaskPrompt.Items[1])
			{
				return " ";
			}
			return _cboMaskPrompt.Text;
		}

		private void OnGrid1LoadingRow(object sender, RowLoadEventArgs eventArgs)
		{
			string[] array = (string[])eventArgs.Row.DataRow;
			eventArgs.Row.Cells[0].Value = array[0];
			eventArgs.Row.Cells[1].Value = array[2];
			eventArgs.Row.Cells[2].Value = array[3];
		}

		private void OnButtonClick(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.OK;
			Hide();
		}

		private void OnGrid1SelectionChanged(object sender, EventArgs eventArgs)
		{
			GridRow row = _maskGrid.GetRow();
			if (row != null && row.RowType == RowType.Record)
			{
				string[] array = (string[])row.DataRow;
				if (array[3] == "B")
				{
					UpdateInputMaskDialogBoolean(true);
				}
				else
				{
					UpdateInputMaskDialogBoolean(false);
					_txtMask.Text = array[1];
				}
				_previewText = array[1];
			}
			else
			{
				UpdateInputMaskDialogBoolean(false);
				_txtMask.Text = "";
			}
			RefreshPreview();
		}

		internal void UpdateInputMaskDialogBoolean(bool inputMaskDialogBoolean)
		{
			if (_isNumericMode != inputMaskDialogBoolean)
			{
				_isNumericMode = inputMaskDialogBoolean;
				if (_isNumericMode)
				{
					_grpMaskEdit.Visible = false;
					_grpNumeric.Visible = true;
					_maskPreviewEdit.Visible = false;
					_numericPreviewEdit.Visible = true;
					_neDecimalDigits.Value = -1;
				}
				else
				{
					_grpMaskEdit.Visible = true;
					_grpNumeric.Visible = false;
					_numericPreviewEdit.Visible = false;
					_maskPreviewEdit.Visible = true;
				}
				RefreshPreview();
			}
		}

		private void RefreshPreview()
		{
			if (_isNumericMode)
			{
				string text;
				if ((text = _previewText.ToUpper(CultureInfo.InvariantCulture)) != null)
				{
					if (!(text == "GENERAL"))
					{
						if (!(text == "CURRENCY"))
						{
							if (!(text == "NUMBER"))
							{
								if (text == "PERCENT")
								{
									_numericPreviewEdit.FormatMask = NumericEditFormatMask.Percent;
								}
							}
							else
							{
								_numericPreviewEdit.FormatMask = NumericEditFormatMask.Number;
							}
						}
						else
						{
							_numericPreviewEdit.FormatMask = NumericEditFormatMask.Currency;
						}
					}
					else
					{
						_numericPreviewEdit.FormatMask = NumericEditFormatMask.General;
					}
				}
				_numericPreviewEdit.DecimalDigits = _neDecimalDigits.Value;
			}
			else
			{
				_maskPreviewEdit.Text = "";
				try
				{
					_maskPreviewEdit.PromptChar = char.Parse(GetPromptCharText());
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					_maskPreviewEdit.PromptChar = '_';
				}
				_maskPreviewEdit.IncludeLiterals = GetChecked();
				_maskPreviewEdit.Mask = _txtMask.Text;
			}
		}

		private void OnTxtMaskTextChanged(object sender, EventArgs eventArgs)
		{
			_previewText = _txtMask.Text;
			RefreshPreview();
		}

		private void OnCancelClick(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.Cancel;
			Hide();
		}

		private void OnGrid1FormattingRow(object sender, RowLoadEventArgs eventArgs)
		{
		}

		private void OnCboMaskPromptTextChanged(object sender, EventArgs eventArgs)
		{
			_maskPreviewEdit.PromptChar = char.Parse(GetPromptCharText());
		}

		private void OnInsertMaskCharClick(object sender, EventArgs eventArgs)
		{
			Button button = (Button)sender;
			_txtMask.SelectedText = (string)button.Tag;
		}

		private void OnInsertBangClick(object sender, EventArgs eventArgs)
		{
			if (!_txtMask.Text.StartsWith("!"))
			{
				_txtMask.Text = "!" + _txtMask.Text;
			}
		}

		private void OnButtonEnter(object sender, EventArgs eventArgs)
		{
			_txtMask.Focus();
		}

		private void OnClearMaskClick(object sender, EventArgs eventArgs)
		{
			_txtMask.Text = "";
			_txtMask.Focus();
		}

		private void OnNumericEdit1Validating(object sender, CancelEventArgs eventArgs)
		{
		}

		private void OnMaskEdit1Validating(object sender, CancelEventArgs eventArgs)
		{
			if (!_maskPreviewEdit.IsTextValid())
			{
				MessageBox.Show("'" + _maskPreviewEdit.Text + "' is not valid for Mask: '" + _maskPreviewEdit.Mask + "'");
			}
		}
	}
}
