using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Design.Designer;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	internal class MultiColumnComboConfigurationWizardDialog : Form
	{
		private int _lastStep = 4;

		private GridEditorControl _columnsCollection;

		private Panel _panelDataSource;

		private Panel _panelSelectColumns;

		internal MxMultiColumnComboBox _temp;

		private ImageList _imageList;

		private Button _btnNext;

		private Button _btnPrevious;

		private Button _btnFinish;

		private Label _label;

		private MxDataGrid _gridAvailableColumns;

		private Panel _panelColumns;

		private Panel _panelMembers;

		private Label _multiColumnComboConfigurationWizardDialogLabel;

		private ComboBox _cboValueMember;

		private ComboBox _cboDisplayMember;

		private Label _lblDisplayMember;

		private ComboBox _cboImageColumn;

		private Label _lblImageColumn;

		private Label _lblValueMemberHint;

		private Label _lblDisplayMemberHint;

		private Label _lblImageColumnHint;

		private Label _lblWarning;

		private Label _lblDefineColumns;

		private Panel _panel;

		private Label _lblKey;

		private Panel _pnlDropDownProps;

		private TextBox _textBox;

		private ComboBox _comboBox;

		private Label _lblSelectColumn;

		private IContainer _iContainer;

		private Label _lblSelectDataSource;

		private GroupBox _groupBox;

		private DataSourceEditorControl _dropDownSourceControl;

		private Panel _panelCheckedItems;

		private Label _lblCheckedDataSource;

		private DataSourceEditorControl _checkedItemsSourceControl;

		private Label _lblValueItemHint;

		private ComboBox _cboValueItemDataMember;

		private Label _lblValueItemDataMember;

		private MxCheckedComboBox _checkedComboBox;

		private object _dataSource;

		private string _dataMember = "";

		internal MxDataGrid _dataGrid;

		internal GridColumn _gridColumn;

		private int _currentStep;

		internal MultiColumnComboConfigurationWizardDialog()
		{
			InitializeControls();
			_dropDownSourceControl.AddChangedHandler(OnDataSourceChanged);
			_checkedItemsSourceControl.AddChangedHandler(OnCheckedSourceChanged);
		}

		private void OnDataSourceChanged(object sender, EventArgs eventArgs)
		{
			SetDataSource(_dropDownSourceControl.GetDataSource());
			SetDataMember(_dropDownSourceControl.GetDataMember());
			UpdateButtonStates();
		}

		private void OnCheckedSourceChanged(object sender, EventArgs eventArgs)
		{
			if (_checkedComboBox != null)
			{
				LoadValueItemChoices();
				UpdateButtonStates();
			}
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MultiColumnComboConfigurationWizardDialog));
			_imageList = new ImageList(_iContainer);
			_panelDataSource = new Panel();
			_lblSelectDataSource = new Label();
			_lblWarning = new Label();
			_dropDownSourceControl = new DataSourceEditorControl();
			_panelSelectColumns = new Panel();
			_gridAvailableColumns = new MxDataGrid();
			_label = new Label();
			_btnNext = new Button();
			_btnPrevious = new Button();
			_btnFinish = new Button();
			_panelColumns = new Panel();
			_lblDefineColumns = new Label();
			_columnsCollection = new GridEditorControl();
			_panelMembers = new Panel();
			_panel = new Panel();
			_groupBox = new GroupBox();
			_lblValueMemberHint = new Label();
			_cboDisplayMember = new ComboBox();
			_cboImageColumn = new ComboBox();
			_lblDisplayMemberHint = new Label();
			_lblImageColumnHint = new Label();
			_lblDisplayMember = new Label();
			_lblImageColumn = new Label();
			_cboValueMember = new ComboBox();
			_multiColumnComboConfigurationWizardDialogLabel = new Label();
			_pnlDropDownProps = new Panel();
			_lblSelectColumn = new Label();
			_comboBox = new ComboBox();
			_textBox = new TextBox();
			_lblKey = new Label();
			_temp = new MxMultiColumnComboBox();
			_panelCheckedItems = new Panel();
			_lblValueItemHint = new Label();
			_cboValueItemDataMember = new ComboBox();
			_lblValueItemDataMember = new Label();
			_lblCheckedDataSource = new Label();
			_checkedItemsSourceControl = new DataSourceEditorControl();
			_panelDataSource.SuspendLayout();
			_panelSelectColumns.SuspendLayout();
			((ISupportInitialize)_gridAvailableColumns).BeginInit();
			_panelColumns.SuspendLayout();
			_panelMembers.SuspendLayout();
			_panel.SuspendLayout();
			_groupBox.SuspendLayout();
			_pnlDropDownProps.SuspendLayout();
			((ISupportInitialize)_temp).BeginInit();
			_panelCheckedItems.SuspendLayout();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Empty;
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			_panelDataSource.Controls.Add(_lblSelectDataSource);
			_panelDataSource.Controls.Add(_lblWarning);
			_panelDataSource.Controls.Add(_dropDownSourceControl);
			_panelDataSource.Location = new Point(8, 0);
			_panelDataSource.Name = "panelStep1";
			_panelDataSource.Size = new Size(448, 266);
			_panelDataSource.TabIndex = 8;
			_panelDataSource.Visible = false;
			_lblSelectDataSource.Location = new Point(13, 12);
			_lblSelectDataSource.Name = "lblSelectDataSource";
			_lblSelectDataSource.Size = new Size(408, 17);
			_lblSelectDataSource.TabIndex = 11;
			_lblSelectDataSource.Text = "Select the DataSource for the Drop Down List:";
			_lblWarning.Location = new Point(16, 223);
			_lblWarning.Name = "lblWarning";
			_lblWarning.Size = new Size(408, 40);
			_lblWarning.TabIndex = 10;
			_lblWarning.Text = "**Note: DataSource and DataMember properties are not preserved at design time. You need to set those properties in the dropdown when the form is loaded.";
			_dropDownSourceControl.Location = new Point(0, 32);
			_dropDownSourceControl.Name = "dataSourceControl1";
			_dropDownSourceControl.Size = new Size(448, 188);
			_dropDownSourceControl.TabIndex = 12;
			_panelSelectColumns.Controls.Add(_gridAvailableColumns);
			_panelSelectColumns.Controls.Add(_label);
			_panelSelectColumns.Location = new Point(8, 0);
			_panelSelectColumns.Name = "panelStep2";
			_panelSelectColumns.Size = new Size(448, 266);
			_panelSelectColumns.TabIndex = 9;
			_panelSelectColumns.Visible = false;
			_gridAvailableColumns.ColumnAutoResize = true;
			_gridAvailableColumns.GridLineStyle = GridLineStyle.Solid;
			_gridAvailableColumns.GroupByBoxVisible = false;
			_gridAvailableColumns.Location = new Point(8, 32);
			_gridAvailableColumns.Name = "grid1";
			_gridAvailableColumns.SaveSettings = false;
			_gridAvailableColumns.ScrollBarWidth = 17;
			_gridAvailableColumns.Size = new Size(432, 231);
			_gridAvailableColumns.TabIndex = 1;
			_label.Location = new Point(8, 8);
			_label.Name = "label3";
			_label.Size = new Size(352, 24);
			_label.TabIndex = 0;
			_label.Text = "Select the columns you want to see in the dropdown list";
			_btnNext.Enabled = false;
			_btnNext.Location = new Point(285, 272);
			_btnNext.Name = "btnNext";
			_btnNext.Size = new Size(80, 24);
			_btnNext.TabIndex = 12;
			_btnNext.Text = "Next ->";
			_btnNext.Click += OnBtnNextClick;
			_btnPrevious.Enabled = false;
			_btnPrevious.Location = new Point(197, 272);
			_btnPrevious.Name = "btnPrevious";
			_btnPrevious.Size = new Size(80, 24);
			_btnPrevious.TabIndex = 13;
			_btnPrevious.Text = "<- Previous";
			_btnPrevious.Click += OnBtnPreviousClick;
			_btnFinish.Location = new Point(373, 272);
			_btnFinish.Name = "btnFinish";
			_btnFinish.Size = new Size(80, 24);
			_btnFinish.TabIndex = 14;
			_btnFinish.Text = "Finish";
			_btnFinish.Click += OnBtnFinishClick;
			_panelColumns.Controls.Add(_lblDefineColumns);
			_panelColumns.Controls.Add(_columnsCollection);
			_panelColumns.Location = new Point(8, 0);
			_panelColumns.Name = "panelStep3";
			_panelColumns.Size = new Size(448, 266);
			_panelColumns.TabIndex = 15;
			_panelColumns.Visible = false;
			_lblDefineColumns.Location = new Point(8, 8);
			_lblDefineColumns.Name = "label10";
			_lblDefineColumns.Size = new Size(344, 16);
			_lblDefineColumns.TabIndex = 17;
			_lblDefineColumns.Text = "Define DropDown Columns:";
			_columnsCollection.Location = new Point(8, 24);
			_columnsCollection.Name = "columnsCollection";
			_columnsCollection.Size = new Size(432, 239);
			_columnsCollection.TabIndex = 16;
			_panelMembers.Controls.Add(_panel);
			_panelMembers.Controls.Add(_pnlDropDownProps);
			_panelMembers.Location = new Point(8, 0);
			_panelMembers.Name = "panelStep4";
			_panelMembers.Size = new Size(448, 263);
			_panelMembers.TabIndex = 16;
			_panelMembers.Visible = false;
			_panel.Controls.Add(_groupBox);
			_panel.Dock = DockStyle.Fill;
			_panel.Location = new Point(0, 92);
			_panel.Name = "panel1";
			_panel.Size = new Size(448, 171);
			_panel.TabIndex = 9;
			_groupBox.Controls.Add(_lblValueMemberHint);
			_groupBox.Controls.Add(_cboDisplayMember);
			_groupBox.Controls.Add(_cboImageColumn);
			_groupBox.Controls.Add(_lblDisplayMemberHint);
			_groupBox.Controls.Add(_lblImageColumnHint);
			_groupBox.Controls.Add(_lblDisplayMember);
			_groupBox.Controls.Add(_lblImageColumn);
			_groupBox.Controls.Add(_cboValueMember);
			_groupBox.Controls.Add(_multiColumnComboConfigurationWizardDialogLabel);
			_groupBox.Location = new Point(3, 6);
			_groupBox.Name = "groupBox1";
			_groupBox.Size = new Size(442, 161);
			_groupBox.TabIndex = 9;
			_groupBox.TabStop = false;
			_groupBox.Text = "DropDown Settings:";
			_lblValueMemberHint.AutoSize = true;
			_lblValueMemberHint.Location = new Point(6, 17);
			_lblValueMemberHint.Name = "label7";
			_lblValueMemberHint.Size = new Size(303, 13);
			_lblValueMemberHint.TabIndex = 6;
			_lblValueMemberHint.Text = "Select the property to use as the actual value for the control.";
			_cboDisplayMember.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboDisplayMember.Location = new Point(110, 85);
			_cboDisplayMember.Name = "cboDisplayMember";
			_cboDisplayMember.Size = new Size(308, 21);
			_cboDisplayMember.TabIndex = 3;
			_cboImageColumn.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboImageColumn.Location = new Point(110, 133);
			_cboImageColumn.Name = "cboImageColumn";
			_cboImageColumn.Size = new Size(308, 21);
			_cboImageColumn.TabIndex = 5;
			_lblDisplayMemberHint.AutoSize = true;
			_lblDisplayMemberHint.Location = new Point(6, 65);
			_lblDisplayMemberHint.Name = "label8";
			_lblDisplayMemberHint.Size = new Size(267, 13);
			_lblDisplayMemberHint.TabIndex = 7;
			_lblDisplayMemberHint.Text = "Select the property to use as the Text for the control.";
			_lblImageColumnHint.AutoSize = true;
			_lblImageColumnHint.Location = new Point(6, 113);
			_lblImageColumnHint.Name = "label9";
			_lblImageColumnHint.Size = new Size(310, 13);
			_lblImageColumnHint.TabIndex = 8;
			_lblImageColumnHint.Text = "Select the column that contains the image to use in the control.";
			_lblDisplayMember.Location = new Point(6, 85);
			_lblDisplayMember.Name = "label5";
			_lblDisplayMember.Size = new Size(96, 16);
			_lblDisplayMember.TabIndex = 2;
			_lblDisplayMember.Text = "DisplayMember:";
			_lblImageColumn.Location = new Point(6, 133);
			_lblImageColumn.Name = "label6";
			_lblImageColumn.Size = new Size(96, 16);
			_lblImageColumn.TabIndex = 4;
			_lblImageColumn.Text = "ImageColumn:";
			_cboValueMember.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboValueMember.Location = new Point(110, 37);
			_cboValueMember.Name = "cboValueMember";
			_cboValueMember.Size = new Size(308, 21);
			_cboValueMember.TabIndex = 1;
			_multiColumnComboConfigurationWizardDialogLabel.Location = new Point(6, 37);
			_multiColumnComboConfigurationWizardDialogLabel.Name = "label4";
			_multiColumnComboConfigurationWizardDialogLabel.Size = new Size(96, 16);
			_multiColumnComboConfigurationWizardDialogLabel.TabIndex = 0;
			_multiColumnComboConfigurationWizardDialogLabel.Text = "ValueMember:";
			_pnlDropDownProps.Controls.Add(_lblSelectColumn);
			_pnlDropDownProps.Controls.Add(_comboBox);
			_pnlDropDownProps.Controls.Add(_textBox);
			_pnlDropDownProps.Controls.Add(_lblKey);
			_pnlDropDownProps.Dock = DockStyle.Top;
			_pnlDropDownProps.Location = new Point(0, 0);
			_pnlDropDownProps.Name = "pnlDropDownProps";
			_pnlDropDownProps.Size = new Size(448, 92);
			_pnlDropDownProps.TabIndex = 10;
			_lblSelectColumn.AutoSize = true;
			_lblSelectColumn.Location = new Point(8, 40);
			_lblSelectColumn.Name = "label13";
			_lblSelectColumn.Size = new Size(228, 13);
			_lblSelectColumn.TabIndex = 11;
			_lblSelectColumn.Text = "Select the Column that will use this dropdown:";
			_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_comboBox.Location = new Point(113, 56);
			_comboBox.Name = "comboBox1";
			_comboBox.Size = new Size(308, 21);
			_comboBox.TabIndex = 9;
			_textBox.Location = new Point(112, 8);
			_textBox.Name = "textBox1";
			_textBox.Size = new Size(309, 21);
			_textBox.TabIndex = 8;
			_lblKey.Location = new Point(8, 10);
			_lblKey.Name = "label11";
			_lblKey.Size = new Size(32, 16);
			_lblKey.TabIndex = 7;
			_lblKey.Text = "Key:";
			_temp.Location = new Point(5, 280);
			_temp.Name = "temp";
			_temp.SelectedIndex = -1;
			_temp.SelectedItem = null;
			_temp.Size = new Size(176, 21);
			_temp.TabIndex = 10;
			_temp.TextAlignment = TextAlignment.Near;
			_temp.UseCompatibleTextRendering = false;
			_temp.Visible = false;
			_panelCheckedItems.Controls.Add(_lblValueItemHint);
			_panelCheckedItems.Controls.Add(_cboValueItemDataMember);
			_panelCheckedItems.Controls.Add(_lblValueItemDataMember);
			_panelCheckedItems.Controls.Add(_lblCheckedDataSource);
			_panelCheckedItems.Controls.Add(_checkedItemsSourceControl);
			_panelCheckedItems.Location = new Point(8, 0);
			_panelCheckedItems.Name = "panelStep5";
			_panelCheckedItems.Size = new Size(448, 266);
			_panelCheckedItems.TabIndex = 17;
			_panelCheckedItems.Visible = false;
			_lblValueItemHint.AutoSize = true;
			_lblValueItemHint.Location = new Point(16, 211);
			_lblValueItemHint.Name = "label2";
			_lblValueItemHint.Size = new Size(303, 13);
			_lblValueItemHint.TabIndex = 15;
			_lblValueItemHint.Text = "Select the property to use as the actual value for the control.";
			_cboValueItemDataMember.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboValueItemDataMember.Location = new Point(157, 232);
			_cboValueItemDataMember.Name = "cboValueItemDataMember";
			_cboValueItemDataMember.Size = new Size(288, 21);
			_cboValueItemDataMember.TabIndex = 14;
			_lblValueItemDataMember.Location = new Point(33, 234);
			_lblValueItemDataMember.Name = "label12";
			_lblValueItemDataMember.Size = new Size(127, 15);
			_lblValueItemDataMember.TabIndex = 13;
			_lblValueItemDataMember.Text = "ValueItemDataMember:";
			_lblCheckedDataSource.Location = new Point(8, 12);
			_lblCheckedDataSource.Name = "label1";
			_lblCheckedDataSource.Size = new Size(408, 17);
			_lblCheckedDataSource.TabIndex = 11;
			_lblCheckedDataSource.Text = "Select the DataSource that contains the Values that are checked:";
			_checkedItemsSourceControl.Location = new Point(0, 32);
			_checkedItemsSourceControl.Name = "dataSourceControl2";
			_checkedItemsSourceControl.Size = new Size(448, 176);
			_checkedItemsSourceControl.TabIndex = 12;
			base.ClientSize = new Size(460, 302);
			base.Controls.Add(_panelCheckedItems);
			base.Controls.Add(_panelMembers);
			base.Controls.Add(_panelDataSource);
			base.Controls.Add(_panelColumns);
			base.Controls.Add(_panelSelectColumns);
			base.Controls.Add(_btnFinish);
			base.Controls.Add(_btnPrevious);
			base.Controls.Add(_btnNext);
			base.Controls.Add(_temp);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "DropDownWizard";
			base.StartPosition = FormStartPosition.CenterScreen;
			_panelDataSource.ResumeLayout(performLayout: false);
			_panelSelectColumns.ResumeLayout(performLayout: false);
			((ISupportInitialize)_gridAvailableColumns).EndInit();
			_panelColumns.ResumeLayout(performLayout: false);
			_panelMembers.ResumeLayout(performLayout: false);
			_panel.ResumeLayout(performLayout: false);
			_groupBox.ResumeLayout(performLayout: false);
			_groupBox.PerformLayout();
			_pnlDropDownProps.ResumeLayout(performLayout: false);
			_pnlDropDownProps.PerformLayout();
			((ISupportInitialize)_temp).EndInit();
			_panelCheckedItems.ResumeLayout(performLayout: false);
			_panelCheckedItems.PerformLayout();
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		internal object GetDataSource()
		{
			return _dataSource;
		}

		internal void SetDataSource(object value)
		{
			if (_dataSource != value)
			{
				_dataSource = value;
				UpdateMultiColumnComboConfigurationWizardDialog();
			}
		}

		private string GetDataMember()
		{
			return _dataMember;
		}

		private void SetDataMember(string text)
		{
			if (_dataMember != text)
			{
				_dataMember = text;
				UpdateMultiColumnComboConfigurationWizardDialog();
			}
		}

		private void UpdateMultiColumnComboConfigurationWizardDialog()
		{
			UpdateButtonStates();
		}

		private void CopyDropDownSettings(GridDropDown gridDropDown, GridDropDown dropDown)
		{
			dropDown.SetDataBinding(gridDropDown.DataSource, gridDropDown.DataMember);
			MemoryStream memoryStream = new MemoryStream();
			gridDropDown.SaveLayoutFile(memoryStream);
			memoryStream.Position = 0L;
			dropDown.LoadLayoutFile(memoryStream);
			memoryStream.Close();
			dropDown.ValueMember = gridDropDown.ValueMember;
			dropDown.DisplayMember = gridDropDown.DisplayMember;
			if (gridDropDown.ImageColumn != null)
			{
				dropDown.ImageColumn = dropDown.Columns[gridDropDown.ImageColumn.Key];
			}
			else
			{
				dropDown.ImageColumn = null;
			}
		}

		private void FillColumnKeyList()
		{
			_comboBox.Items.Clear();
			_comboBox.ValueMember = "Value";
			_comboBox.DisplayMember = "Text";
			foreach (GridTable table in _dataGrid.Tables)
			{
				foreach (GridColumn column in table.Columns)
				{
					GridValueListItem item = new GridValueListItem(column, column.Key);
					_comboBox.Items.Add(item);
				}
			}
		}

		internal DialogResult ComputeDialogResult(GridDropDown gridDropDown, IContainer iContainer)
		{
			_lastStep = 4;
			Text = "MultiColumnCombo Configuration Wizard";
			CopyDropDownSettings(gridDropDown, _temp.DropDownList);
			_dropDownSourceControl.UpdateIContainer(iContainer);
			_dropDownSourceControl.SetDataSource(gridDropDown.DataSource);
			_dropDownSourceControl.SetDataMember(gridDropDown.DataMember);
			if (gridDropDown._dropListOwner == null)
			{
				_pnlDropDownProps.Visible = true;
				_textBox.Text = gridDropDown.Key;
				if (_dataGrid != null)
				{
					FillColumnKeyList();
				}
			}
			else
			{
				_lblWarning.Visible = false;
				if (gridDropDown._dropListOwner is MxCheckedComboBox)
				{
					_lastStep = 5;
					_checkedItemsSourceControl.UpdateIContainer(iContainer);
					_dropDownSourceControl.SetDataSourceLabel("DropDownDataSource:");
					_dropDownSourceControl.SetDataMemberLabel("DropDownDataMember:");
					_checkedItemsSourceControl.SetDataSourceLabel("ValuesDataSource:");
					_checkedItemsSourceControl.SetDataMemberLabel("ValuesDataMember:");
					_checkedComboBox = (MxCheckedComboBox)gridDropDown._dropListOwner;
					Text = "CheckedComboBox Configuration Wizard";
				}
				_pnlDropDownProps.Visible = false;
			}
			_dataSource = gridDropDown.DataSource;
			_dataMember = gridDropDown.DataMember;
			CopyDataBinding(gridDropDown);
			UpdateButtonStates();
			UpdateMultiColumnComboConfigurationWizardDialogInteger(1);
			DialogResult dialogResult = ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				CopyDropDownSettings(_temp.DropDownList, gridDropDown);
				if (_cboValueMember.SelectedIndex == 0)
				{
					gridDropDown.ValueMember = "";
				}
				else
				{
					gridDropDown.ValueMember = _cboValueMember.Text;
				}
				if (_cboDisplayMember.SelectedIndex == 0)
				{
					gridDropDown.DisplayMember = "";
				}
				else
				{
					gridDropDown.DisplayMember = _cboDisplayMember.Text;
				}
				GridColumn gridColumn = (GridColumn)((GridValueListItem)_cboImageColumn.SelectedItem).Value;
				if (gridColumn != null)
				{
					gridDropDown.ImageColumn = gridDropDown.Columns[gridColumn.Key];
				}
				else
				{
					gridDropDown.ImageColumn = null;
				}
				if (gridDropDown._dropListOwner == null)
				{
					gridDropDown.Key = _textBox.Text;
					if (_comboBox.SelectedIndex != -1)
					{
						GridValueListItem gridValueListItem = (GridValueListItem)_comboBox.SelectedItem;
						_gridColumn = (GridColumn)gridValueListItem.Value;
						_gridColumn.DropDown = gridDropDown;
					}
				}
				else if (_checkedComboBox != null)
				{
					_checkedComboBox.ValuesDataMember = _checkedItemsSourceControl.GetDataMember();
					_checkedComboBox.ValuesDataSource = _checkedItemsSourceControl.GetDataSource();
					_checkedComboBox.ValueItemDataMember = _cboValueItemDataMember.Text;
				}
			}
			return dialogResult;
		}

		private void CopyDataBinding(GridDropDown gridDropDown)
		{
			_temp.DataSource = gridDropDown.DataSource;
			_temp.DataMember = gridDropDown.DataMember;
		}

		private void UpdateButtonStates()
		{
			if (CanAdvance())
			{
				_btnNext.Enabled = GetMultiColumnComboConfigurationWizardDialogInteger() != _lastStep;
				_btnFinish.Enabled = GetMultiColumnComboConfigurationWizardDialogInteger() == _lastStep;
			}
			else
			{
				_btnNext.Enabled = false;
				_btnFinish.Enabled = false;
			}
			_btnPrevious.Enabled = GetMultiColumnComboConfigurationWizardDialogInteger() != 1;
		}

		private int GetMultiColumnComboConfigurationWizardDialogInteger()
		{
			return _currentStep;
		}

		private void UpdateMultiColumnComboConfigurationWizardDialogInteger(int multiColumnComboConfigurationWizardDialogInteger)
		{
			if (_currentStep != multiColumnComboConfigurationWizardDialogInteger)
			{
				try
				{
					CommitCurrentStep(_currentStep < multiColumnComboConfigurationWizardDialogInteger);
					_currentStep = multiColumnComboConfigurationWizardDialogInteger;
					ShowCurrentStep();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			}
		}

		private void CommitCurrentStep(bool condition)
		{
			switch (_currentStep)
			{
			case 1:
				_temp.DataSource = GetDataSource();
				_temp.DataMember = GetDataMember();
				_panelDataSource.Visible = false;
				break;
			case 2:
				if (condition)
				{
					ApplyCheckedColumns();
				}
				_panelSelectColumns.Visible = false;
				break;
			case 3:
				((IGridElement)_columnsCollection).CommitChanges();
				_panelColumns.Visible = false;
				break;
			case 4:
				ApplyMemberSettings();
				_panelMembers.Visible = false;
				break;
			case 5:
				ApplyValueItemDataMember();
				_panelCheckedItems.Visible = false;
				break;
			}
		}

		private void ApplyValueItemDataMember()
		{
			if (_cboValueItemDataMember.SelectedIndex <= -1)
			{
				_checkedComboBox.ValueItemDataMember = "";
			}
			else
			{
				_checkedComboBox.ValueItemDataMember = ((GridValueListItem)_cboValueItemDataMember.SelectedItem).Text;
			}
		}

		private void ApplyMemberSettings()
		{
			if (_cboValueMember.SelectedIndex <= -1)
			{
				_temp.ValueMember = "";
			}
			else
			{
				_temp.ValueMember = ((GridValueListItem)_cboValueMember.SelectedItem).Text;
			}
			if (_cboDisplayMember.SelectedIndex == -1)
			{
				_temp.DisplayMember = "";
			}
			else
			{
				_temp.DisplayMember = _cboDisplayMember.Text;
			}
			GridValueListItem gridValueListItem = (GridValueListItem)_cboImageColumn.SelectedItem;
			if (gridValueListItem != null)
			{
				_temp.DropDownList.ImageColumn = (GridColumn)gridValueListItem.Value;
			}
			else
			{
				_temp.DropDownList.ImageColumn = null;
			}
		}

		private void ApplyCheckedColumns()
		{
			try
			{
				GridRow[] checkedRows = _gridAvailableColumns.GetCheckedRows();
				ArrayList arrayList = new ArrayList();
				GridRow[] array = checkedRows;
				foreach (GridRow gridRow in array)
				{
					string text = (string)gridRow.Cells["DataMember"].Value;
					if (!HasBoundColumn(text))
					{
						arrayList.Add(text);
					}
				}
				for (int index = _temp.DropDownList.Columns.Count - 1; index >= 0; index--)
				{
					GridColumn gridColumn = _temp.DropDownList.Columns[index];
					if (gridColumn.BoundMode == ColumnBoundMode.Bound)
					{
						bool condition = false;
						GridRow[] otherCheckedRows = checkedRows;
						foreach (GridRow otherGridRow in otherCheckedRows)
						{
							string strA = (string)otherGridRow.Cells["DataMember"].Value;
							if (string.Compare(strA, gridColumn.DataMember, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
							{
								condition = true;
							}
						}
						if (!condition)
						{
							_temp.DropDownList.Columns.RemoveAt(index);
						}
					}
				}
				if (arrayList.Count > 0)
				{
					MxDataGrid dataGrid = new MxDataGrid();
					dataGrid.SetDataBinding(GetDataSource(), GetDataMember());
					dataGrid.RetrieveStructure();
					foreach (string item in arrayList)
					{
						if (dataGrid.RootTable.Columns.Contains(item))
						{
							GridColumn column = dataGrid.RootTable.Columns[item];
							dataGrid.RootTable.Columns.Remove(column);
							_temp.DropDownList.Columns.Add(column);
						}
					}
				}
				if (_checkedComboBox == null)
				{
					return;
				}
				bool otherCondition = false;
				foreach (GridColumn otherColumn in _temp.DropDownList.Columns)
				{
					if (otherColumn.ActAsSelector || otherColumn.ShowRowSelector)
					{
						otherCondition = true;
						break;
					}
				}
				if (!otherCondition)
				{
					GridColumn nextColumn = new GridColumn();
					nextColumn.ActAsSelector = true;
					nextColumn.Width = 24;
					nextColumn.Key = _checkedComboBox.Name + "_Selector";
					_temp.DropDownList.Columns.Insert(0, nextColumn);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void ShowCurrentStep()
		{
			switch (_currentStep)
			{
			case 1:
				_panelDataSource.Visible = true;
				break;
			case 2:
				LoadAvailableColumns();
				_panelSelectColumns.Visible = true;
				break;
			case 3:
				LoadColumnsEditor();
				_panelColumns.Visible = true;
				break;
			case 4:
				LoadMemberChoices();
				_panelMembers.Visible = true;
				break;
			case 5:
				_panelCheckedItems.Visible = true;
				break;
			}
			UpdateButtonStates();
		}

		private void LoadMemberChoices()
		{
			ArrayList arrayList = new ArrayList();
			ArrayList otherArrayList = new ArrayList();
			ArrayList nextArrayList = new ArrayList();
			nextArrayList.Add(new GridValueListItem(null, "(none)"));
			arrayList.Add(new GridValueListItem("", "(None)"));
			otherArrayList.Add(new GridValueListItem("", "(None)"));
			if (GetDataSource() == null)
			{
				_cboDisplayMember.DropDownStyle = ComboBoxStyle.DropDown;
				_cboValueMember.DropDownStyle = ComboBoxStyle.DropDown;
				foreach (GridColumn column in _temp.DropDownList.Columns)
				{
					if (column.BoundMode == ColumnBoundMode.Bound && column.DataMember.Length > 0)
					{
						arrayList.Add(new GridValueListItem(column.DataMember, column.DataMember));
						otherArrayList.Add(new GridValueListItem(column.DataMember, column.DataMember));
					}
				}
			}
			else
			{
				_cboDisplayMember.DropDownStyle = ComboBoxStyle.DropDownList;
				_cboValueMember.DropDownStyle = ComboBoxStyle.DropDownList;
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[GetDataSource(), GetDataMember()];
				PropertyDescriptorCollection itemProperties = currencyManager.GetItemProperties();
				foreach (PropertyDescriptor item in itemProperties)
				{
					arrayList.Add(new GridValueListItem(item.Name, item.Name));
					otherArrayList.Add(new GridValueListItem(item.Name, item.Name));
				}
			}
			int selectedIndex = 0;
			foreach (GridColumn otherColumn in _temp.DropDownList.Columns)
			{
				if (otherColumn.ColumnType == ColumnType.BoundImage || otherColumn.ColumnType == ColumnType.Image || otherColumn.ColumnType == ColumnType.ImageAndText)
				{
					nextArrayList.Add(new GridValueListItem(otherColumn, otherColumn.DataMember));
					if (otherColumn == _temp.DropDownList.ImageColumn)
					{
						selectedIndex = nextArrayList.Count - 1;
					}
				}
			}
			_cboDisplayMember.DisplayMember = "Text";
			_cboDisplayMember.DataSource = arrayList;
			if (_temp.DisplayMember == "")
			{
				_cboDisplayMember.SelectedIndex = 0;
			}
			else
			{
				_cboDisplayMember.SelectedIndex = _cboDisplayMember.FindString(_temp.DisplayMember);
			}
			_cboValueMember.DisplayMember = "Text";
			_cboValueMember.DataSource = otherArrayList;
			if (_temp.ValueMember == "")
			{
				_cboValueMember.SelectedIndex = 0;
			}
			else
			{
				_cboValueMember.SelectedIndex = _cboValueMember.FindString(_temp.ValueMember);
			}
			_cboImageColumn.DisplayMember = "Text";
			_cboImageColumn.DataSource = nextArrayList;
			_cboImageColumn.SelectedIndex = selectedIndex;
		}

		private void LoadValueItemChoices()
		{
			if (_checkedComboBox == null)
			{
				return;
			}
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new GridValueListItem("", "(None)"));
			object dataSource = _checkedItemsSourceControl.GetDataSource();
			string dataMember = _checkedItemsSourceControl.GetDataMember();
			if (dataSource == null)
			{
				_cboValueItemDataMember.DropDownStyle = ComboBoxStyle.DropDown;
			}
			else
			{
				_cboValueItemDataMember.DropDownStyle = ComboBoxStyle.DropDownList;
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dataSource, dataMember];
				PropertyDescriptorCollection itemProperties = currencyManager.GetItemProperties();
				foreach (PropertyDescriptor item in itemProperties)
				{
					arrayList.Add(new GridValueListItem(item.Name, item.Name));
				}
			}
			_cboValueItemDataMember.DisplayMember = "Text";
			_cboValueItemDataMember.DataSource = arrayList;
			if (_checkedComboBox.ValueItemDataMember == "")
			{
				_cboValueItemDataMember.SelectedIndex = 0;
			}
			else
			{
				_cboValueItemDataMember.SelectedIndex = _cboValueItemDataMember.FindString(_checkedComboBox.ValueItemDataMember);
			}
		}

		private void LoadColumnsEditor()
		{
			IGridElement columnsCollection = _columnsCollection;
			columnsCollection.SelectedObject = null;
			columnsCollection.SelectedObject = _temp.DropDownList.Columns;
		}

		private void LoadAvailableColumns()
		{
			CurrencyManager currencyManager = (CurrencyManager)BindingContext[GetDataSource(), GetDataMember()];
			PropertyDescriptorCollection itemProperties = currencyManager.GetItemProperties();
			ArrayList arrayList = new ArrayList();
			foreach (PropertyDescriptor item in itemProperties)
			{
				GridValueListItem value = new GridValueListItem(item.Name, item.Name);
				arrayList.Add(value);
			}
			_gridAvailableColumns.DataSource = arrayList;
			_gridAvailableColumns.GroupByBoxVisible = false;
			GridTable gridTable = new GridTable();
			GridColumn gridColumn = gridTable.Columns.Add("Checked");
			gridColumn.ActAsSelector = true;
			gridColumn.AllowSize = false;
			gridColumn.Caption = "";
			gridColumn._width = 24;
			gridColumn = gridTable.Columns.Add("DataMember");
			gridColumn.DataMember = "Text";
			gridColumn.Selectable = false;
			gridColumn.AllowSize = true;
			_gridAvailableColumns.RootTable = gridTable;
			_gridAvailableColumns.ColumnAutoResize = true;
			GridRow[] rows = _gridAvailableColumns.GetRows();
			GridRow[] array = rows;
			foreach (GridRow gridRow in array)
			{
				if (HasBoundColumn((string)gridRow.Cells["DataMember"].Value))
				{
					gridRow.IsChecked = true;
				}
			}
		}

		private bool HasBoundColumn(string strB)
		{
			foreach (GridColumn column in _temp.DropDownList.Columns)
			{
				if (column.BoundMode == ColumnBoundMode.Bound && string.Compare(column.DataMember, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}

		private bool CanAdvance()
		{
			int currentStep = _currentStep;
			if (currentStep == 1)
			{
				return _dropDownSourceControl.IsSelectionComplete();
			}
			return true;
		}

		private void OnBtnPreviousClick(object sender, EventArgs eventArgs)
		{
			if (_currentStep == 3 && GetDataSource() == null)
			{
				UpdateMultiColumnComboConfigurationWizardDialogInteger(1);
			}
			else
			{
				UpdateMultiColumnComboConfigurationWizardDialogInteger(GetMultiColumnComboConfigurationWizardDialogInteger() - 1);
			}
		}

		private void OnBtnNextClick(object sender, EventArgs eventArgs)
		{
			if (_currentStep == 1 && GetDataSource() == null)
			{
				UpdateMultiColumnComboConfigurationWizardDialogInteger(3);
			}
			else
			{
				UpdateMultiColumnComboConfigurationWizardDialogInteger(GetMultiColumnComboConfigurationWizardDialogInteger() + 1);
			}
		}

		private void OnBtnFinishClick(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.OK;
		}
	}
}
