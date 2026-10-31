using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	internal class SelfReferencingSettingsDialog : Form
	{
		private GridTable _targetTable;

		private IContainer _iContainer = default;

		private GroupBox _gbSelfReferencing;

		private Label _label;

		private Label _selfReferencingSettingsDialogLabel;

		private ComboBox _cboChildMember;

		private ComboBox _cboParentMember;

		private ComboBox _cboParentRootMode;

		private Label _lblParentRootMode;

		private CheckBox _chkTreatOrphanRowsAsRoot;

		private TypedValueEditorControl _parentRootValueEditor;

		private GroupBox _gbSelfReferencingChildListMember;

		private ComboBox _cboChildListMember;

		private Label _lblChildListMember;

		private Label _lblRootCriteriaHint;

		private Label _lblExpandColumn;

		private ComboBox _cboExpandColumn;

		private Label _lblHierarchicalGroupMode;

		private ComboBox _cboHierarchicalGroupMode;

		private GroupBox _gbParentRootValue;

		private GroupBox _groupBox;

		private RadioButton _optUseChildTables;

		private RadioButton _optSelfReferencing;

		private RadioButton _optSelfReferencingChildList;

		private Button _btnOk;

		private Button _btnCancel;

		internal SelfReferencingSettingsDialog()
		{
			InitializeControls();
		}

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
		}

		internal DialogResult ComputeDialogResult(GridSelfReferencingSettings gridSelfReferencingSettings)
		{
			_targetTable = gridSelfReferencingSettings.Table;
			switch (_targetTable.HierarchicalMode)
			{
			case HierarchicalMode.SelfReferencing:
				_optSelfReferencing.Checked = true;
				break;
			case HierarchicalMode.SelfReferencingChildList:
				_optSelfReferencingChildList.Checked = true;
				break;
			case HierarchicalMode.UseChildTables:
				_optUseChildTables.Checked = true;
				break;
			}
			AddPropertyNames(_cboParentMember, _targetTable.GetBaseProperties(true, false));
			_cboParentMember.Text = gridSelfReferencingSettings.ParentDataMember;
			AddPropertyNames(_cboChildMember, _targetTable.GetBaseProperties(true, false));
			_cboChildMember.Text = gridSelfReferencingSettings.ChildDataMember;
			_cboHierarchicalGroupMode.DataSource = Enum.GetValues(typeof(HierarchicalGroupMode));
			_cboHierarchicalGroupMode.SelectedItem = gridSelfReferencingSettings.HierarchicalGroupMode;
			_cboParentRootMode.DataSource = Enum.GetValues(typeof(ParentRootMode));
			_cboParentRootMode.SelectedItem = gridSelfReferencingSettings.ParentRootMode;
			FillExpandColumnList(_targetTable);
			_parentRootValueEditor.SetTypedValue(gridSelfReferencingSettings.ParentRootValue);
			_chkTreatOrphanRowsAsRoot.Checked = gridSelfReferencingSettings.TreatOrphanRowsAsRoot;
			AddPropertyNames(_cboChildListMember, _targetTable.GetBaseProperties(false, true));
			_cboChildListMember.Text = gridSelfReferencingSettings.ChildListMember;
			DialogResult dialogResult = ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				if (_optUseChildTables.Checked)
				{
					_targetTable.HierarchicalMode = HierarchicalMode.UseChildTables;
				}
				else if (_optSelfReferencing.Checked)
				{
					_targetTable.HierarchicalMode = HierarchicalMode.SelfReferencing;
				}
				else if (_optSelfReferencingChildList.Checked)
				{
					_targetTable.HierarchicalMode = HierarchicalMode.SelfReferencingChildList;
				}
				_targetTable.SelfReferencingSettings.ChildListMember = _cboChildListMember.Text;
				_targetTable.SelfReferencingSettings.ChildDataMember = _cboChildMember.Text;
				object selectedValue = _cboExpandColumn.SelectedValue;
				if (selectedValue == DBNull.Value)
				{
					_targetTable.SelfReferencingSettings.ExpandColumn = null;
				}
				else
				{
					_targetTable.SelfReferencingSettings.ExpandColumn = (GridColumn)selectedValue;
				}
				_targetTable.SelfReferencingSettings.HierarchicalGroupMode = (HierarchicalGroupMode)_cboHierarchicalGroupMode.SelectedItem;
				_targetTable.SelfReferencingSettings.ParentDataMember = _cboParentMember.Text;
				_targetTable.SelfReferencingSettings.ParentRootMode = (ParentRootMode)_cboParentRootMode.SelectedItem;
				if (_targetTable.SelfReferencingSettings.ParentRootMode == ParentRootMode.UseParentRootValue)
				{
					_targetTable.SelfReferencingSettings.ParentRootValue = _parentRootValueEditor.GetTypedValue();
				}
				else
				{
					_targetTable.SelfReferencingSettings.ParentRootValue = null;
				}
				_targetTable.SelfReferencingSettings.TreatOrphanRowsAsRoot = _chkTreatOrphanRowsAsRoot.Checked;
			}
			return dialogResult;
		}

		private void AddPropertyNames(ComboBox comboBox, PropertyDescriptor[] propertyDescriptorArray)
		{
			if (propertyDescriptorArray != null)
			{
				foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorArray)
				{
					comboBox.Items.Add(propertyDescriptor.Name);
				}
			}
		}

		private void FillExpandColumnList(GridTable gridTable)
		{
			_cboExpandColumn.ValueMember = "Value";
			_cboExpandColumn.DisplayMember = "Text";
			GridValueListItemCollection gridValueListItemCollection = new GridValueListItemCollection();
			gridValueListItemCollection.Add(DBNull.Value, "(None)");
			foreach (GridColumn column in gridTable.Columns)
			{
				gridValueListItemCollection.Add(column, column.Key);
			}
			_cboExpandColumn.DataSource = gridValueListItemCollection;
			GridColumn expandColumn = gridTable.SelfReferencingSettings.ExpandColumn;
			if (expandColumn == null)
			{
				_cboExpandColumn.SelectedValue = DBNull.Value;
			}
			else
			{
				_cboExpandColumn.SelectedValue = expandColumn;
			}
		}

		private void OnCboParentRootModeSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			ParentRootMode parentRootMode = (ParentRootMode)_cboParentRootMode.SelectedItem;
			_gbParentRootValue.Enabled = parentRootMode == ParentRootMode.UseParentRootValue;
		}

		private void OnOptUseChildTablesCheckedChanged(object sender, EventArgs eventArgs)
		{
			_cboHierarchicalGroupMode.Enabled = !_optUseChildTables.Checked;
			_cboExpandColumn.Enabled = !_optUseChildTables.Checked;
		}

		private void OnOptSelfReferencingCheckedChanged(object sender, EventArgs eventArgs)
		{
			_gbSelfReferencing.Enabled = _optSelfReferencing.Checked;
			_gbSelfReferencing.BringToFront();
		}

		private void OnOptSelfReferencingChildListCheckedChanged(object sender, EventArgs eventArgs)
		{
			_gbSelfReferencingChildListMember.Enabled = _optSelfReferencingChildList.Checked;
			_gbSelfReferencingChildListMember.BringToFront();
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
			_gbSelfReferencing = new GroupBox();
			_gbParentRootValue = new GroupBox();
			_parentRootValueEditor = new TypedValueEditorControl();
			_lblRootCriteriaHint = new Label();
			_chkTreatOrphanRowsAsRoot = new CheckBox();
			_cboParentRootMode = new ComboBox();
			_lblParentRootMode = new Label();
			_cboChildMember = new ComboBox();
			_cboParentMember = new ComboBox();
			_label = new Label();
			_selfReferencingSettingsDialogLabel = new Label();
			_gbSelfReferencingChildListMember = new GroupBox();
			_cboChildListMember = new ComboBox();
			_lblChildListMember = new Label();
			_lblExpandColumn = new Label();
			_cboExpandColumn = new ComboBox();
			_lblHierarchicalGroupMode = new Label();
			_cboHierarchicalGroupMode = new ComboBox();
			_groupBox = new GroupBox();
			_optSelfReferencingChildList = new RadioButton();
			_optSelfReferencing = new RadioButton();
			_optUseChildTables = new RadioButton();
			_btnOk = new Button();
			_btnCancel = new Button();
			_gbSelfReferencing.SuspendLayout();
			_gbParentRootValue.SuspendLayout();
			_gbSelfReferencingChildListMember.SuspendLayout();
			_groupBox.SuspendLayout();
			SuspendLayout();
			_gbSelfReferencing.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_gbSelfReferencing.Controls.Add(_gbParentRootValue);
			_gbSelfReferencing.Controls.Add(_lblRootCriteriaHint);
			_gbSelfReferencing.Controls.Add(_chkTreatOrphanRowsAsRoot);
			_gbSelfReferencing.Controls.Add(_cboParentRootMode);
			_gbSelfReferencing.Controls.Add(_lblParentRootMode);
			_gbSelfReferencing.Controls.Add(_cboChildMember);
			_gbSelfReferencing.Controls.Add(_cboParentMember);
			_gbSelfReferencing.Controls.Add(_label);
			_gbSelfReferencing.Controls.Add(_selfReferencingSettingsDialogLabel);
			_gbSelfReferencing.Enabled = false;
			_gbSelfReferencing.Location = new Point(20, 64);
			_gbSelfReferencing.Name = "gbSelfReferencing";
			_gbSelfReferencing.Size = new Size(518, 207);
			_gbSelfReferencing.TabIndex = 2;
			_gbSelfReferencing.TabStop = false;
			_gbSelfReferencing.Text = "Specify the keys that relate rows in the datasource:";
			_gbParentRootValue.Controls.Add(_parentRootValueEditor);
			_gbParentRootValue.Enabled = false;
			_gbParentRootValue.Location = new Point(19, 118);
			_gbParentRootValue.Name = "gbParentRootValue";
			_gbParentRootValue.Size = new Size(311, 80);
			_gbParentRootValue.TabIndex = 11;
			_gbParentRootValue.TabStop = false;
			_gbParentRootValue.Text = "ParentRootValue:";
			_parentRootValueEditor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_parentRootValueEditor.Location = new Point(18, 19);
			_parentRootValueEditor.Name = "obectTypeControl1";
			_parentRootValueEditor.Size = new Size(287, 60);
			_parentRootValueEditor.TabIndex = 6;
			_lblRootCriteriaHint.AutoSize = true;
			_lblRootCriteriaHint.Location = new Point(16, 69);
			_lblRootCriteriaHint.Name = "label9";
			_lblRootCriteriaHint.Size = new Size(395, 13);
			_lblRootCriteriaHint.TabIndex = 10;
			_lblRootCriteriaHint.Text = "Select the criteria to use when defining which rows in the datasource are root rows";
			_chkTreatOrphanRowsAsRoot.AutoSize = true;
			_chkTreatOrphanRowsAsRoot.CheckAlign = ContentAlignment.MiddleRight;
			_chkTreatOrphanRowsAsRoot.Location = new Point(358, 118);
			_chkTreatOrphanRowsAsRoot.Name = "chkTreatOrphanRowsAsRoot";
			_chkTreatOrphanRowsAsRoot.Size = new Size(148, 17);
			_chkTreatOrphanRowsAsRoot.TabIndex = 7;
			_chkTreatOrphanRowsAsRoot.Text = "TreatOrphanRowsAsRoot";
			_cboParentRootMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboParentRootMode.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboParentRootMode.FormattingEnabled = true;
			_cboParentRootMode.Location = new Point(130, 91);
			_cboParentRootMode.Name = "cboParentRootMode";
			_cboParentRootMode.Size = new Size(265, 21);
			_cboParentRootMode.TabIndex = 5;
			_cboParentRootMode.SelectedIndexChanged += OnCboParentRootModeSelectedIndexChanged;
			_lblParentRootMode.AutoSize = true;
			_lblParentRootMode.Location = new Point(16, 91);
			_lblParentRootMode.Name = "label4";
			_lblParentRootMode.Size = new Size(97, 13);
			_lblParentRootMode.TabIndex = 4;
			_lblParentRootMode.Text = "Parent Root Mode:";
			_cboChildMember.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboChildMember.FormattingEnabled = true;
			_cboChildMember.Location = new Point(266, 40);
			_cboChildMember.Name = "cboChildMember";
			_cboChildMember.Size = new Size(240, 21);
			_cboChildMember.TabIndex = 3;
			_cboParentMember.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboParentMember.FormattingEnabled = true;
			_cboParentMember.Location = new Point(19, 40);
			_cboParentMember.Name = "cboParentMember";
			_cboParentMember.Size = new Size(240, 21);
			_cboParentMember.TabIndex = 2;
			_label.AutoSize = true;
			_label.Location = new Point(16, 20);
			_label.Name = "label2";
			_label.Size = new Size(102, 13);
			_label.TabIndex = 1;
			_label.Text = "ParentDataMember:";
			_selfReferencingSettingsDialogLabel.AutoSize = true;
			_selfReferencingSettingsDialogLabel.Location = new Point(263, 20);
			_selfReferencingSettingsDialogLabel.Name = "label1";
			_selfReferencingSettingsDialogLabel.Size = new Size(94, 13);
			_selfReferencingSettingsDialogLabel.TabIndex = 0;
			_selfReferencingSettingsDialogLabel.Text = "ChildDataMember:";
			_gbSelfReferencingChildListMember.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_gbSelfReferencingChildListMember.Controls.Add(_cboChildListMember);
			_gbSelfReferencingChildListMember.Controls.Add(_lblChildListMember);
			_gbSelfReferencingChildListMember.Location = new Point(20, 64);
			_gbSelfReferencingChildListMember.Name = "gbSelfReferencingChildListMember";
			_gbSelfReferencingChildListMember.Size = new Size(518, 207);
			_gbSelfReferencingChildListMember.TabIndex = 6;
			_gbSelfReferencingChildListMember.TabStop = false;
			_gbSelfReferencingChildListMember.Text = "Specify the property or relation that returns the list of child rows of a row:";
			_cboChildListMember.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboChildListMember.FormattingEnabled = true;
			_cboChildListMember.Location = new Point(130, 36);
			_cboChildListMember.Name = "cboChildListMember";
			_cboChildListMember.Size = new Size(265, 21);
			_cboChildListMember.TabIndex = 2;
			_lblChildListMember.AutoSize = true;
			_lblChildListMember.Location = new Point(34, 39);
			_lblChildListMember.Name = "label6";
			_lblChildListMember.Size = new Size(87, 13);
			_lblChildListMember.TabIndex = 1;
			_lblChildListMember.Text = "ChildListMember:";
			_lblExpandColumn.AutoSize = true;
			_lblExpandColumn.Location = new Point(23, 311);
			_lblExpandColumn.Name = "label5";
			_lblExpandColumn.Size = new Size(81, 13);
			_lblExpandColumn.TabIndex = 10;
			_lblExpandColumn.Text = "ExpandColumn:";
			_cboExpandColumn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboExpandColumn.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboExpandColumn.FormattingEnabled = true;
			_cboExpandColumn.Location = new Point(150, 308);
			_cboExpandColumn.Name = "cboExpandColumn";
			_cboExpandColumn.Size = new Size(265, 21);
			_cboExpandColumn.TabIndex = 9;
			_lblHierarchicalGroupMode.AutoSize = true;
			_lblHierarchicalGroupMode.Location = new Point(23, 284);
			_lblHierarchicalGroupMode.Name = "label3";
			_lblHierarchicalGroupMode.Size = new Size(122, 13);
			_lblHierarchicalGroupMode.TabIndex = 8;
			_lblHierarchicalGroupMode.Text = "HierarchicalGroupMode:";
			_cboHierarchicalGroupMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboHierarchicalGroupMode.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboHierarchicalGroupMode.FormattingEnabled = true;
			_cboHierarchicalGroupMode.Location = new Point(150, 281);
			_cboHierarchicalGroupMode.Name = "cboHierarchicalGroupMode";
			_cboHierarchicalGroupMode.Size = new Size(265, 21);
			_cboHierarchicalGroupMode.TabIndex = 7;
			_groupBox.Controls.Add(_optSelfReferencingChildList);
			_groupBox.Controls.Add(_optSelfReferencing);
			_groupBox.Controls.Add(_optUseChildTables);
			_groupBox.Location = new Point(20, 6);
			_groupBox.Name = "groupBox2";
			_groupBox.Size = new Size(518, 52);
			_groupBox.TabIndex = 11;
			_groupBox.TabStop = false;
			_groupBox.Text = "Hierarchical Mode:";
			_optSelfReferencingChildList.AutoSize = true;
			_optSelfReferencingChildList.Location = new Point(366, 29);
			_optSelfReferencingChildList.Name = "optSelfReferencingChildList";
			_optSelfReferencingChildList.Size = new Size(140, 17);
			_optSelfReferencingChildList.TabIndex = 2;
			_optSelfReferencingChildList.TabStop = true;
			_optSelfReferencingChildList.Text = "SelfReferencingChildList";
			_optSelfReferencingChildList.UseVisualStyleBackColor = true;
			_optSelfReferencingChildList.CheckedChanged += OnOptSelfReferencingChildListCheckedChanged;
			_optSelfReferencing.AutoSize = true;
			_optSelfReferencing.Location = new Point(192, 29);
			_optSelfReferencing.Name = "optSelfReferencing";
			_optSelfReferencing.Size = new Size(101, 17);
			_optSelfReferencing.TabIndex = 1;
			_optSelfReferencing.TabStop = true;
			_optSelfReferencing.Text = "SelfReferencing";
			_optSelfReferencing.UseVisualStyleBackColor = true;
			_optSelfReferencing.CheckedChanged += OnOptSelfReferencingCheckedChanged;
			_optUseChildTables.AutoSize = true;
			_optUseChildTables.Location = new Point(20, 29);
			_optUseChildTables.Name = "optUseChildTables";
			_optUseChildTables.Size = new Size(99, 17);
			_optUseChildTables.TabIndex = 0;
			_optUseChildTables.TabStop = true;
			_optUseChildTables.Text = "UseChildTables";
			_optUseChildTables.UseVisualStyleBackColor = true;
			_optUseChildTables.CheckedChanged += OnOptUseChildTablesCheckedChanged;
			_btnOk.DialogResult = DialogResult.OK;
			_btnOk.Location = new Point(372, 348);
			_btnOk.Name = "btnOk";
			_btnOk.Size = new Size(80, 24);
			_btnOk.TabIndex = 12;
			_btnOk.Text = "OK";
			_btnOk.UseVisualStyleBackColor = true;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(458, 348);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 13;
			_btnCancel.Text = "Cancel";
			_btnCancel.UseVisualStyleBackColor = true;
			base.AcceptButton = _btnOk;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(544, 378);
			base.Controls.Add(_btnCancel);
			base.Controls.Add(_btnOk);
			base.Controls.Add(_groupBox);
			base.Controls.Add(_lblExpandColumn);
			base.Controls.Add(_cboExpandColumn);
			base.Controls.Add(_lblHierarchicalGroupMode);
			base.Controls.Add(_cboHierarchicalGroupMode);
			base.Controls.Add(_gbSelfReferencing);
			base.Controls.Add(_gbSelfReferencingChildListMember);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			MinimumSize = new Size(450, 400);
			base.Name = "SelfReferencingWizard";
			Text = "Self Referencing Settings";
			base.Load += OnControlLoad;
			_gbSelfReferencing.ResumeLayout(performLayout: false);
			_gbSelfReferencing.PerformLayout();
			_gbParentRootValue.ResumeLayout(performLayout: false);
			_gbSelfReferencingChildListMember.ResumeLayout(performLayout: false);
			_gbSelfReferencingChildListMember.PerformLayout();
			_groupBox.ResumeLayout(performLayout: false);
			_groupBox.PerformLayout();
			ResumeLayout(performLayout: false);
			PerformLayout();
		}
	}
}
