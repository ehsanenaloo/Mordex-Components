using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	internal class AddColumnDialog : Form
	{
		private GridColumn _gridColumn;

		private GridTable _targetTable;

		private bool _isRefreshingMembers;

		private int _currentStep = -1;

		private IContainer _iContainer = default;

		private Panel _pnlStepBinding;

		private RadioButton _optSelector;

		private RadioButton _optMultipleValues;

		private RadioButton _optSingleValue;

		private Label _label;

		private Button _btnPrevious;

		private Button _btnNext;

		private Button _btnFinish;

		private Button _btnCancel;

		private CheckBox _chkUseHeaderSelector;

		private Panel _pnlStepDisplay;

		private Label _lblBoundMode;

		private ComboBox _cboBoundMode;

		private TextBox _txtKey;

		private Label _lblKey;

		private Label _lblDataMember;

		private ComboBox _cboDataMember;

		private ComboBox _cboMultipleValuesDataMember;

		private Label _lblMultipleValuesDataMember;

		private CheckBox _checkBox;

		private Label _addColumnDialogLabel;

		private TextBox _txtCaption;

		private Label _lblCaption;

		private ComboBox _cboColumnType;

		private Label _lblColumnType;

		private ComboBox _cboEditType;

		private Label _lblEditType;

		internal AddColumnDialog()
		{
			InitializeControls();
		}

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
		}

		internal GridColumn ShowAndCreateColumn(GridTable gridTable, string text, Form owner)
		{
			base.StartPosition = FormStartPosition.CenterParent;
			_gridColumn = new GridColumn();
			_gridColumn.Key = text;
			_gridColumn.SetBaseParentCollection(gridTable.Columns);
			_txtKey.Text = text;
			RefreshDataMembers(gridTable);
			_targetTable = gridTable;
			ProcessComboBox(_cboBoundMode, typeof(ColumnBoundMode), ColumnBoundMode.Bound);
			UpdateAddColumnDialog();
			GoToStep(0);
			ProcessComboBox(_cboBoundMode, typeof(ColumnBoundMode), _gridColumn.BoundMode);
			ProcessComboBox(_cboEditType, typeof(EditType), _gridColumn.EditType);
			ProcessComboBox(_cboColumnType, typeof(ColumnType), _gridColumn.ColumnType);
			if (gridTable.DataGrid is GridDropDown)
			{
				_lblEditType.Visible = false;
				_cboEditType.Visible = false;
				_optSelector.Visible = false;
			}
			GoToStep(0);
			if (ShowDialog(owner) == DialogResult.OK)
			{
				if (_gridColumn.ActAsSelector)
				{
					_gridColumn.Width = 20;
				}
				_gridColumn.Visible = _checkBox.Checked;
				return _gridColumn;
			}
			return null;
		}

		private static void ProcessComboBox(ComboBox comboBox, Type enumType, object value)
		{
			comboBox.Items.Clear();
			Array values = Enum.GetValues(enumType);
			foreach (object item in values)
			{
				comboBox.Items.Add(item);
			}
			comboBox.SelectedItem = value;
		}

		private void OnOptSingleValueCheckedChanged(object sender, EventArgs eventArgs)
		{
			UpdateAddColumnDialog();
		}

		private void UpdateAddColumnDialog()
		{
			RefreshDataMembers(_targetTable);
			_gridColumn.ActAsSelector = _optSelector.Checked;
			_gridColumn.MultipleValues = _optMultipleValues.Checked;
			_gridColumn.BoundMode = (ColumnBoundMode)_cboBoundMode.SelectedItem;
			_chkUseHeaderSelector.Visible = _gridColumn.ActAsSelector;
			_lblBoundMode.Visible = !_gridColumn.ActAsSelector;
			_cboBoundMode.Visible = !_gridColumn.ActAsSelector;
			_lblKey.Visible = _gridColumn.BoundMode != ColumnBoundMode.Bound && !_gridColumn.ActAsSelector;
			_txtKey.Visible = _gridColumn.BoundMode != ColumnBoundMode.Bound && !_gridColumn.ActAsSelector;
			_lblDataMember.Visible = _gridColumn.BoundMode == ColumnBoundMode.Bound && !_gridColumn.ActAsSelector;
			_cboDataMember.Visible = _gridColumn.BoundMode == ColumnBoundMode.Bound && !_gridColumn.ActAsSelector;
			_lblMultipleValuesDataMember.Visible = _gridColumn.MultipleValues;
			_cboMultipleValuesDataMember.Visible = _gridColumn.MultipleValues;
			UpdateNavigationButtons();
		}

		private int GetLastStepIndex()
		{
			if (_gridColumn.ActAsSelector)
			{
				return 0;
			}
			return 1;
		}

		private Panel ComputePanel(int value)
		{
			switch (value)
			{
			case 0:
				return _pnlStepBinding;
			case 1:
				return _pnlStepDisplay;
			default:
				return null;
			}
		}

		private int GetCurrentStep()
		{
			return _currentStep;
		}

		private void GoToStep(int lastStepIndex)
		{
			if (lastStepIndex < 0)
			{
				lastStepIndex = 0;
			}
			if (lastStepIndex > GetLastStepIndex())
			{
				lastStepIndex = GetLastStepIndex();
			}
			if (_currentStep != lastStepIndex)
			{
				if (ComputePanel(_currentStep) != null)
				{
					ComputePanel(_currentStep).Visible = false;
				}
				_currentStep = lastStepIndex;
				if (_currentStep == 1)
				{
					_txtCaption.Text = _gridColumn.Caption;
					_cboEditType.SelectedItem = _gridColumn.EditType;
					_cboColumnType.SelectedItem = _gridColumn.ColumnType;
				}
				if (ComputePanel(_currentStep) != null)
				{
					ComputePanel(_currentStep).Visible = true;
				}
				UpdateNavigationButtons();
			}
		}

		private void UpdateNavigationButtons()
		{
			_btnPrevious.Enabled = _currentStep > 0;
			_btnNext.Enabled = _currentStep < GetLastStepIndex();
		}

		private void RefreshDataMembers(GridTable gridTable)
		{
			if (_targetTable != null && _gridColumn.MultipleValues == _isRefreshingMembers)
			{
				return;
			}
			_isRefreshingMembers = _gridColumn.MultipleValues;
			_cboDataMember.Items.Clear();
			_cboDataMember.DisplayMember = "Name";
			PropertyDescriptor[] array = gridTable.GetBaseProperties(!_gridColumn.MultipleValues, _gridColumn.MultipleValues);
			if (array != null)
			{
				PropertyDescriptor[] otherArray = array;
				foreach (PropertyDescriptor item in otherArray)
				{
					_cboDataMember.Items.Add(item);
				}
			}
		}

		private void OnChkUseHeaderSelectorCheckedChanged(object sender, EventArgs eventArgs)
		{
			_gridColumn.UseHeaderSelector = _chkUseHeaderSelector.Checked;
		}

		private void OnCboBoundModeSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_gridColumn.BoundMode = (ColumnBoundMode)_cboBoundMode.SelectedItem;
			UpdateAddColumnDialog();
		}

		private void OnCboDataMemberSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			OnCboDataMemberTextChanged(sender, eventArgs);
		}

		private void OnCboDataMemberTextChanged(object sender, EventArgs eventArgs)
		{
			if (_gridColumn.BoundMode != ColumnBoundMode.Bound)
			{
				return;
			}
			_gridColumn.DataMember = _cboDataMember.Text;
			_gridColumn.Key = _gridColumn.DataMember;
			int stringExact = _cboDataMember.FindStringExact(_cboDataMember.Text);
			if (stringExact != -1)
			{
				PropertyDescriptor propertyDescriptor = (PropertyDescriptor)_cboDataMember.Items[stringExact];
				if (propertyDescriptor.PropertyType == typeof(byte[]))
				{
					_gridColumn.ColumnType = ColumnType.BoundImage;
					_gridColumn.EditType = EditType.NoEdit;
					_gridColumn.FormatString = "";
				}
				else if (propertyDescriptor.PropertyType == typeof(DateTime))
				{
					_gridColumn.ColumnType = ColumnType.Text;
					_gridColumn.FormatString = "d";
					_gridColumn.EditType = EditType.DatePicker;
				}
				else if (propertyDescriptor.PropertyType == typeof(bool))
				{
					_gridColumn.ColumnType = ColumnType.CheckBox;
					_gridColumn.EditType = EditType.CheckBox;
					_gridColumn.FormatString = "";
				}
				else
				{
					_gridColumn.ColumnType = ColumnType.Text;
					_gridColumn.EditType = EditType.TextBox;
					_gridColumn.FormatString = "";
				}
				if (_gridColumn.MultipleValues)
				{
					ProcessPropertyDescriptor(propertyDescriptor);
				}
				_gridColumn.Caption = Regex.Replace(propertyDescriptor.DisplayName, "(\\p{Ll})(\\p{Lu})|_+", "$1 $2");
			}
			else
			{
				_gridColumn.Caption = _gridColumn.DataMember;
			}
		}

		private void ProcessPropertyDescriptor(PropertyDescriptor propertyDescriptor)
		{
			_cboMultipleValuesDataMember.Items.Clear();
			GridTable gridTable = new GridTable();
			gridTable.DataMember = propertyDescriptor.Name;
			_targetTable.ChildTables.Add(gridTable);
			gridTable.RetrieveBaseFields();
			_targetTable.ChildTables.Remove(gridTable);
			_cboMultipleValuesDataMember.DisplayMember = "Name";
			foreach (GridColumn column in gridTable.Columns)
			{
				_cboMultipleValuesDataMember.Items.Add(column.DataMember);
			}
		}

		private void OnBtnPreviousClick(object sender, EventArgs eventArgs)
		{
			if (GetCurrentStep() > 0)
			{
				GoToStep(GetCurrentStep() - 1);
			}
		}

		private void OnBtnNextClick(object sender, EventArgs eventArgs)
		{
			if (GetCurrentStep() < GetLastStepIndex())
			{
				GoToStep(GetCurrentStep() + 1);
			}
		}

		private void OnBtnFinishClick(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.OK;
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.Cancel;
		}

		private void OnTxtCaptionTextChanged(object sender, EventArgs eventArgs)
		{
			_gridColumn.Caption = _txtCaption.Text;
		}

		private void OnCboColumnTypeSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_gridColumn.ColumnType = (ColumnType)_cboColumnType.SelectedItem;
		}

		private void OnCboEditTypeSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_gridColumn.EditType = (EditType)_cboEditType.SelectedItem;
		}

		private void OnCheckBoxCheckedChanged(object sender, EventArgs eventArgs)
		{
			_gridColumn.Visible = _checkBox.Visible;
		}

		private void OnCboMultipleValuesDataMemberTextChanged(object sender, EventArgs eventArgs)
		{
			_gridColumn.MultipleValueDataMember = _cboMultipleValuesDataMember.Text;
		}

		private void OnTxtKeyTextChanged(object sender, EventArgs eventArgs)
		{
			_gridColumn.Key = _txtKey.Text;
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
			_pnlStepBinding = new Panel();
			_cboMultipleValuesDataMember = new ComboBox();
			_lblMultipleValuesDataMember = new Label();
			_label = new Label();
			_lblDataMember = new Label();
			_optSelector = new RadioButton();
			_cboDataMember = new ComboBox();
			_optMultipleValues = new RadioButton();
			_txtKey = new TextBox();
			_optSingleValue = new RadioButton();
			_lblKey = new Label();
			_chkUseHeaderSelector = new CheckBox();
			_lblBoundMode = new Label();
			_cboBoundMode = new ComboBox();
			_btnPrevious = new Button();
			_btnNext = new Button();
			_btnFinish = new Button();
			_btnCancel = new Button();
			_pnlStepDisplay = new Panel();
			_checkBox = new CheckBox();
			_addColumnDialogLabel = new Label();
			_txtCaption = new TextBox();
			_lblCaption = new Label();
			_cboColumnType = new ComboBox();
			_lblColumnType = new Label();
			_cboEditType = new ComboBox();
			_lblEditType = new Label();
			_pnlStepBinding.SuspendLayout();
			_pnlStepDisplay.SuspendLayout();
			SuspendLayout();
			_pnlStepBinding.Controls.Add(_cboMultipleValuesDataMember);
			_pnlStepBinding.Controls.Add(_lblMultipleValuesDataMember);
			_pnlStepBinding.Controls.Add(_label);
			_pnlStepBinding.Controls.Add(_lblDataMember);
			_pnlStepBinding.Controls.Add(_optSelector);
			_pnlStepBinding.Controls.Add(_cboDataMember);
			_pnlStepBinding.Controls.Add(_optMultipleValues);
			_pnlStepBinding.Controls.Add(_txtKey);
			_pnlStepBinding.Controls.Add(_optSingleValue);
			_pnlStepBinding.Controls.Add(_lblKey);
			_pnlStepBinding.Controls.Add(_chkUseHeaderSelector);
			_pnlStepBinding.Controls.Add(_lblBoundMode);
			_pnlStepBinding.Controls.Add(_cboBoundMode);
			_pnlStepBinding.Location = new Point(12, 12);
			_pnlStepBinding.Name = "pnlStep0";
			_pnlStepBinding.Size = new Size(420, 145);
			_pnlStepBinding.TabIndex = 0;
			_pnlStepBinding.Visible = false;
			_cboMultipleValuesDataMember.FormattingEnabled = true;
			_cboMultipleValuesDataMember.ItemHeight = 13;
			_cboMultipleValuesDataMember.Location = new Point(226, 112);
			_cboMultipleValuesDataMember.Name = "cboMultipleValuesDataMember";
			_cboMultipleValuesDataMember.Size = new Size(185, 21);
			_cboMultipleValuesDataMember.TabIndex = 17;
			_cboMultipleValuesDataMember.SelectedIndexChanged += OnCboMultipleValuesDataMemberTextChanged;
			_cboMultipleValuesDataMember.TextChanged += OnCboMultipleValuesDataMemberTextChanged;
			_lblMultipleValuesDataMember.Location = new Point(160, 92);
			_lblMultipleValuesDataMember.Name = "lblMultipleValuesDataMember";
			_lblMultipleValuesDataMember.Size = new Size(171, 17);
			_lblMultipleValuesDataMember.TabIndex = 16;
			_lblMultipleValuesDataMember.Text = "MultipleValuesDataMember:";
			_label.Location = new Point(14, 10);
			_label.Name = "label1";
			_label.Size = new Size(368, 24);
			_label.TabIndex = 8;
			_label.Text = "Choose what kind of column you want to add.";
			_lblDataMember.Location = new Point(145, 68);
			_lblDataMember.Name = "lblDataMember";
			_lblDataMember.Size = new Size(80, 16);
			_lblDataMember.TabIndex = 14;
			_lblDataMember.Text = "DataMember:";
			_optSelector.AutoSize = true;
			_optSelector.Location = new Point(29, 83);
			_optSelector.Name = "optSelector";
			_optSelector.Size = new Size(64, 17);
			_optSelector.TabIndex = 2;
			_optSelector.Text = "Selector";
			_optSelector.UseVisualStyleBackColor = true;
			_optSelector.CheckedChanged += OnOptSingleValueCheckedChanged;
			_cboDataMember.FormattingEnabled = true;
			_cboDataMember.ItemHeight = 13;
			_cboDataMember.Location = new Point(226, 65);
			_cboDataMember.Name = "cboDataMember";
			_cboDataMember.Size = new Size(185, 21);
			_cboDataMember.TabIndex = 15;
			_cboDataMember.SelectedIndexChanged += OnCboDataMemberSelectedIndexChanged;
			_cboDataMember.TextChanged += OnCboDataMemberTextChanged;
			_optMultipleValues.AutoSize = true;
			_optMultipleValues.Location = new Point(29, 60);
			_optMultipleValues.Name = "optMultipleValues";
			_optMultipleValues.Size = new Size(96, 17);
			_optMultipleValues.TabIndex = 1;
			_optMultipleValues.Text = "Multiple Values";
			_optMultipleValues.UseVisualStyleBackColor = true;
			_optMultipleValues.CheckedChanged += OnOptSingleValueCheckedChanged;
			_txtKey.Location = new Point(226, 66);
			_txtKey.Name = "txtKey";
			_txtKey.Size = new Size(185, 20);
			_txtKey.TabIndex = 13;
			_txtKey.Visible = false;
			_txtKey.TextChanged += OnTxtKeyTextChanged;
			_optSingleValue.AutoSize = true;
			_optSingleValue.Checked = true;
			_optSingleValue.Location = new Point(29, 37);
			_optSingleValue.Name = "optSingleValue";
			_optSingleValue.Size = new Size(84, 17);
			_optSingleValue.TabIndex = 0;
			_optSingleValue.TabStop = true;
			_optSingleValue.Text = "Single Value";
			_optSingleValue.UseVisualStyleBackColor = true;
			_optSingleValue.CheckedChanged += OnOptSingleValueCheckedChanged;
			_lblKey.Location = new Point(145, 70);
			_lblKey.Name = "lblKey";
			_lblKey.Size = new Size(40, 16);
			_lblKey.TabIndex = 12;
			_lblKey.Text = "Key:";
			_lblKey.Visible = false;
			_chkUseHeaderSelector.AutoSize = true;
			_chkUseHeaderSelector.Checked = true;
			_chkUseHeaderSelector.CheckState = CheckState.Checked;
			_chkUseHeaderSelector.Location = new Point(148, 92);
			_chkUseHeaderSelector.Name = "chkUseHeaderSelector";
			_chkUseHeaderSelector.Size = new Size(119, 17);
			_chkUseHeaderSelector.TabIndex = 9;
			_chkUseHeaderSelector.Text = "UseHeaderSelector";
			_chkUseHeaderSelector.UseVisualStyleBackColor = true;
			_chkUseHeaderSelector.CheckedChanged += OnChkUseHeaderSelectorCheckedChanged;
			_lblBoundMode.Location = new Point(145, 40);
			_lblBoundMode.Name = "lblBoundMode";
			_lblBoundMode.Size = new Size(80, 16);
			_lblBoundMode.TabIndex = 10;
			_lblBoundMode.Text = "BoundMode:";
			_lblBoundMode.Visible = false;
			_cboBoundMode.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboBoundMode.FormattingEnabled = true;
			_cboBoundMode.ItemHeight = 13;
			_cboBoundMode.Location = new Point(226, 39);
			_cboBoundMode.Name = "cboBoundMode";
			_cboBoundMode.Size = new Size(185, 21);
			_cboBoundMode.TabIndex = 11;
			_cboBoundMode.Visible = false;
			_cboBoundMode.SelectedIndexChanged += OnCboBoundModeSelectedIndexChanged;
			_btnPrevious.Enabled = false;
			_btnPrevious.Location = new Point(94, 163);
			_btnPrevious.Name = "btnPrevious";
			_btnPrevious.Size = new Size(80, 26);
			_btnPrevious.TabIndex = 1;
			_btnPrevious.Text = "< Previous";
			_btnPrevious.UseVisualStyleBackColor = true;
			_btnPrevious.Click += OnBtnPreviousClick;
			_btnNext.Location = new Point(180, 163);
			_btnNext.Name = "btnNext";
			_btnNext.Size = new Size(80, 26);
			_btnNext.TabIndex = 2;
			_btnNext.Text = "Next >";
			_btnNext.UseVisualStyleBackColor = true;
			_btnNext.Click += OnBtnNextClick;
			_btnFinish.Location = new Point(266, 163);
			_btnFinish.Name = "btnFinish";
			_btnFinish.Size = new Size(80, 26);
			_btnFinish.TabIndex = 3;
			_btnFinish.Text = "Finish";
			_btnFinish.UseVisualStyleBackColor = true;
			_btnFinish.Click += OnBtnFinishClick;
			_btnCancel.Location = new Point(352, 163);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 26);
			_btnCancel.TabIndex = 4;
			_btnCancel.Text = "Cancel";
			_btnCancel.UseVisualStyleBackColor = true;
			_btnCancel.Click += OnBtnCancelClick;
			_pnlStepDisplay.Controls.Add(_checkBox);
			_pnlStepDisplay.Controls.Add(_addColumnDialogLabel);
			_pnlStepDisplay.Controls.Add(_txtCaption);
			_pnlStepDisplay.Controls.Add(_lblCaption);
			_pnlStepDisplay.Controls.Add(_cboColumnType);
			_pnlStepDisplay.Controls.Add(_lblColumnType);
			_pnlStepDisplay.Controls.Add(_cboEditType);
			_pnlStepDisplay.Controls.Add(_lblEditType);
			_pnlStepDisplay.Location = new Point(12, 12);
			_pnlStepDisplay.Name = "pnlStep1";
			_pnlStepDisplay.Size = new Size(420, 145);
			_pnlStepDisplay.TabIndex = 5;
			_pnlStepDisplay.Visible = false;
			_checkBox.CheckAlign = ContentAlignment.MiddleRight;
			_checkBox.Checked = true;
			_checkBox.CheckState = CheckState.Checked;
			_checkBox.Location = new Point(243, 115);
			_checkBox.Name = "checkBox1";
			_checkBox.Size = new Size(72, 16);
			_checkBox.TabIndex = 15;
			_checkBox.Text = "Visible";
			_checkBox.CheckedChanged += OnCheckBoxCheckedChanged;
			_addColumnDialogLabel.Location = new Point(14, 12);
			_addColumnDialogLabel.Name = "label2";
			_addColumnDialogLabel.Size = new Size(328, 16);
			_addColumnDialogLabel.TabIndex = 14;
			_addColumnDialogLabel.Text = "Column Properties:";
			_txtCaption.Location = new Point(131, 34);
			_txtCaption.Name = "txtCaption";
			_txtCaption.Size = new Size(272, 20);
			_txtCaption.TabIndex = 9;
			_txtCaption.TextChanged += OnTxtCaptionTextChanged;
			_lblCaption.Location = new Point(43, 36);
			_lblCaption.Name = "lblCaption";
			_lblCaption.Size = new Size(48, 16);
			_lblCaption.TabIndex = 8;
			_lblCaption.Text = "&Caption:";
			_cboColumnType.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboColumnType.FormattingEnabled = true;
			_cboColumnType.ItemHeight = 13;
			_cboColumnType.Location = new Point(131, 58);
			_cboColumnType.Name = "cboColumnType";
			_cboColumnType.Size = new Size(184, 21);
			_cboColumnType.TabIndex = 11;
			_cboColumnType.SelectedIndexChanged += OnCboColumnTypeSelectedIndexChanged;
			_lblColumnType.Location = new Point(43, 60);
			_lblColumnType.Name = "lblColumnType";
			_lblColumnType.Size = new Size(80, 16);
			_lblColumnType.TabIndex = 10;
			_lblColumnType.Text = "Column&Type:";
			_cboEditType.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboEditType.FormattingEnabled = true;
			_cboEditType.ItemHeight = 13;
			_cboEditType.Location = new Point(131, 82);
			_cboEditType.Name = "cboEditType";
			_cboEditType.Size = new Size(184, 21);
			_cboEditType.TabIndex = 13;
			_cboEditType.SelectedIndexChanged += OnCboEditTypeSelectedIndexChanged;
			_lblEditType.Location = new Point(43, 84);
			_lblEditType.Name = "lblEditType";
			_lblEditType.Size = new Size(80, 16);
			_lblEditType.TabIndex = 12;
			_lblEditType.Text = "&EditType:";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(438, 198);
			base.ControlBox = false;
			base.Controls.Add(_btnCancel);
			base.Controls.Add(_btnFinish);
			base.Controls.Add(_btnNext);
			base.Controls.Add(_btnPrevious);
			base.Controls.Add(_pnlStepBinding);
			base.Controls.Add(_pnlStepDisplay);
			base.Name = "AddColumnWizardEx";
			Text = "Add Column";
			base.Load += OnControlLoad;
			_pnlStepBinding.ResumeLayout(performLayout: false);
			_pnlStepBinding.PerformLayout();
			_pnlStepDisplay.ResumeLayout(performLayout: false);
			_pnlStepDisplay.PerformLayout();
			ResumeLayout(performLayout: false);
		}
	}
}
