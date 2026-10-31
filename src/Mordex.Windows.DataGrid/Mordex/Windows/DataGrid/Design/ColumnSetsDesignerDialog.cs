using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class ColumnSetsDesignerDialog : Form
	{
		private Container _container = new Container();

		private Button _btnClear;

		private Button _btnAdd;

		private ColumnsetDesigner _designer;

		private Label _lblColumns;

		private Label _lblDesigner;

		private Panel _panel;

		private Label _selectedObjectLabel;

		private Panel _columnSetsDesignerDialogPanel;

		private NumericUpDown _columnSetRowCount;

		private Label _label;

		private Label _columnSetsDesignerDialogLabel;

		private NumericUpDown _columnSetHederLines;

		private Label _columnSetHeaderLinesLabel;

		private NumericUpDown _headerLines;

		private Label _headerLinesLabel;

		private ComboBox _columnSetHeaders;

		private ComboBox _columnHeaders;

		private Label _columnSetHeadersLabel;

		private Label _columnHeadersLabel;

		private Button _cmdRemove;

		private Button _cmdClearColumns;

		private ColumnList _columnListBox;

		private Button _btnOK;

		private Button _button;

		private PropertyGrid _tablePropertyGrid;

		private GridTable _targetTable;

		private MxDataGrid _dataGrid;

		private InheritableBoolean _columnHeadersSetting;

		private InheritableBoolean _columnSetHeadersSetting;

		internal ColumnSetsDesignerDialog()
		{
			InitializeControls();
			_designer.AddHeaderDraggedHandler(OnColumnListChanged);
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ColumnSetsDesignerDialog));
			_designer = new ColumnsetDesigner();
			_columnListBox = new ColumnList();
			_btnClear = new Button();
			_btnAdd = new Button();
			_lblColumns = new Label();
			_lblDesigner = new Label();
			_panel = new Panel();
			_tablePropertyGrid = new PropertyGrid();
			_columnSetsDesignerDialogLabel = new Label();
			_selectedObjectLabel = new Label();
			_columnSetsDesignerDialogPanel = new Panel();
			_columnHeadersLabel = new Label();
			_columnSetHeadersLabel = new Label();
			_columnHeaders = new ComboBox();
			_columnSetHeaders = new ComboBox();
			_headerLines = new NumericUpDown();
			_headerLinesLabel = new Label();
			_columnSetHederLines = new NumericUpDown();
			_columnSetHeaderLinesLabel = new Label();
			_columnSetRowCount = new NumericUpDown();
			_label = new Label();
			_cmdRemove = new Button();
			_cmdClearColumns = new Button();
			_btnOK = new Button();
			_button = new Button();
			_panel.SuspendLayout();
			_columnSetsDesignerDialogPanel.SuspendLayout();
			((ISupportInitialize)_headerLines).BeginInit();
			((ISupportInitialize)_columnSetHederLines).BeginInit();
			((ISupportInitialize)_columnSetRowCount).BeginInit();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_designer.AllowDrop = true;
			_designer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_designer.Location = new Point(160, 64);
			_designer.Name = "designer";
			_designer.ScrollBarWidth = 17;
			_designer.Size = new Size(400, 120);
			_designer.TabIndex = 0;
			_designer.SelectedObjectChange += OnDesignerSelectedObjectChange;
			_columnListBox.AllowDrop = true;
			_columnListBox.CurrentTable = null;
			_columnListBox.FirstColumn = 0;
			_columnListBox.Location = new Point(8, 64);
			_columnListBox.Name = "columnList";
			_columnListBox.Rows = 8;
			_columnListBox.ScrollBarWidth = 17;
			_columnListBox.Size = new Size(144, 120);
			_columnListBox.TabIndex = 7;
			_btnClear.Location = new Point(216, 8);
			_btnClear.Name = "btnClear";
			_btnClear.Size = new Size(104, 24);
			_btnClear.TabIndex = 1;
			_btnClear.Text = "Clear ColumnSets";
			_btnClear.Click += OnBtnClearClick;
			_btnAdd.Location = new Point(8, 8);
			_btnAdd.Name = "btnAdd";
			_btnAdd.Size = new Size(96, 24);
			_btnAdd.TabIndex = 2;
			_btnAdd.Text = "Add ColumnSet";
			_btnAdd.Click += OnBtnAddClick;
			_lblColumns.BackColor = SystemColors.ActiveCaption;
			_lblColumns.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_lblColumns.ForeColor = SystemColors.ActiveCaptionText;
			_lblColumns.Location = new Point(8, 40);
			_lblColumns.Name = "lblColumns";
			_lblColumns.Size = new Size(144, 24);
			_lblColumns.TabIndex = 8;
			_lblColumns.Text = "Available columns";
			_lblColumns.TextAlign = ContentAlignment.MiddleLeft;
			_lblDesigner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_lblDesigner.BackColor = SystemColors.ActiveCaption;
			_lblDesigner.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_lblDesigner.ForeColor = SystemColors.ActiveCaptionText;
			_lblDesigner.Location = new Point(160, 40);
			_lblDesigner.Name = "lblDesigner";
			_lblDesigner.Size = new Size(400, 24);
			_lblDesigner.TabIndex = 9;
			_lblDesigner.Text = "ColumnSet Layout";
			_lblDesigner.TextAlign = ContentAlignment.MiddleLeft;
			_panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_panel.Controls.Add(_tablePropertyGrid);
			_panel.Controls.Add(_columnSetsDesignerDialogLabel);
			_panel.Controls.Add(_selectedObjectLabel);
			_panel.Controls.Add(_columnSetsDesignerDialogPanel);
			_panel.Location = new Point(8, 192);
			_panel.Name = "panel1";
			_panel.Size = new Size(552, 160);
			_panel.TabIndex = 11;
			_tablePropertyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_tablePropertyGrid.HelpVisible = false;
			_tablePropertyGrid.LineColor = SystemColors.Control;
			_tablePropertyGrid.Location = new Point(224, 24);
			_tablePropertyGrid.Name = "propertyGrid1";
			_tablePropertyGrid.PropertySort = PropertySort.Alphabetical;
			_tablePropertyGrid.Size = new Size(328, 136);
			_tablePropertyGrid.TabIndex = 18;
			_tablePropertyGrid.ToolbarVisible = false;
			_tablePropertyGrid.PropertyValueChanged += OnPropertyGrid1PropertyValueChanged;
			_columnSetsDesignerDialogLabel.BackColor = SystemColors.ActiveCaption;
			_columnSetsDesignerDialogLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_columnSetsDesignerDialogLabel.ForeColor = SystemColors.ActiveCaptionText;
			_columnSetsDesignerDialogLabel.Location = new Point(0, 0);
			_columnSetsDesignerDialogLabel.Name = "label2";
			_columnSetsDesignerDialogLabel.Size = new Size(216, 24);
			_columnSetsDesignerDialogLabel.TabIndex = 17;
			_columnSetsDesignerDialogLabel.Text = "Table Properties";
			_columnSetsDesignerDialogLabel.TextAlign = ContentAlignment.MiddleLeft;
			_selectedObjectLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_selectedObjectLabel.BackColor = SystemColors.ActiveCaption;
			_selectedObjectLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_selectedObjectLabel.ForeColor = SystemColors.ActiveCaptionText;
			_selectedObjectLabel.Location = new Point(224, 0);
			_selectedObjectLabel.Name = "lblSelectedObject";
			_selectedObjectLabel.Size = new Size(328, 24);
			_selectedObjectLabel.TabIndex = 15;
			_selectedObjectLabel.Text = "(none)";
			_selectedObjectLabel.TextAlign = ContentAlignment.MiddleLeft;
			_columnSetsDesignerDialogPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			_columnSetsDesignerDialogPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			_columnSetsDesignerDialogPanel.Controls.Add(_columnHeadersLabel);
			_columnSetsDesignerDialogPanel.Controls.Add(_columnSetHeadersLabel);
			_columnSetsDesignerDialogPanel.Controls.Add(_columnHeaders);
			_columnSetsDesignerDialogPanel.Controls.Add(_columnSetHeaders);
			_columnSetsDesignerDialogPanel.Controls.Add(_headerLines);
			_columnSetsDesignerDialogPanel.Controls.Add(_headerLinesLabel);
			_columnSetsDesignerDialogPanel.Controls.Add(_columnSetHederLines);
			_columnSetsDesignerDialogPanel.Controls.Add(_columnSetHeaderLinesLabel);
			_columnSetsDesignerDialogPanel.Controls.Add(_columnSetRowCount);
			_columnSetsDesignerDialogPanel.Controls.Add(_label);
			_columnSetsDesignerDialogPanel.Location = new Point(0, 24);
			_columnSetsDesignerDialogPanel.Name = "panel2";
			_columnSetsDesignerDialogPanel.Size = new Size(216, 136);
			_columnSetsDesignerDialogPanel.TabIndex = 16;
			_columnHeadersLabel.AutoSize = true;
			_columnHeadersLabel.Location = new Point(8, 115);
			_columnHeadersLabel.Name = "label6";
			_columnHeadersLabel.Size = new Size(86, 13);
			_columnHeadersLabel.TabIndex = 24;
			_columnHeadersLabel.Text = "ColumnHeaders:";
			_columnSetHeadersLabel.AutoSize = true;
			_columnSetHeadersLabel.Location = new Point(8, 88);
			_columnSetHeadersLabel.Name = "label5";
			_columnSetHeadersLabel.Size = new Size(102, 13);
			_columnSetHeadersLabel.TabIndex = 23;
			_columnSetHeadersLabel.Text = "ColumnSetHeaders:";
			_columnHeaders.DropDownStyle = ComboBoxStyle.DropDownList;
			_columnHeaders.Location = new Point(128, 112);
			_columnHeaders.Name = "columnHeaders";
			_columnHeaders.Size = new Size(80, 21);
			_columnHeaders.TabIndex = 22;
			_columnHeaders.SelectedIndexChanged += OnColumnHeadersSelectedIndexChanged;
			_columnSetHeaders.DropDownStyle = ComboBoxStyle.DropDownList;
			_columnSetHeaders.Location = new Point(128, 85);
			_columnSetHeaders.Name = "columnSetHeaders";
			_columnSetHeaders.Size = new Size(80, 21);
			_columnSetHeaders.TabIndex = 21;
			_columnSetHeaders.SelectedIndexChanged += OnColumnSetHeadersSelectedIndexChanged;
			_headerLines.Location = new Point(128, 58);
			_headerLines.Maximum = new decimal(new int[4] { 256, 0, 0, 0 });
			_headerLines.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
			_headerLines.Name = "headerLines";
			_headerLines.Size = new Size(80, 21);
			_headerLines.TabIndex = 20;
			_headerLines.TextAlign = HorizontalAlignment.Right;
			_headerLines.Value = new decimal(new int[4] { 1, 0, 0, 0 });
			_headerLines.ValueChanged += OnHeaderLinesValueChanged;
			_headerLinesLabel.AutoSize = true;
			_headerLinesLabel.Location = new Point(8, 61);
			_headerLinesLabel.Name = "label4";
			_headerLinesLabel.Size = new Size(70, 13);
			_headerLinesLabel.TabIndex = 19;
			_headerLinesLabel.Text = "HeaderLines:";
			_columnSetHederLines.Location = new Point(128, 31);
			_columnSetHederLines.Maximum = new decimal(new int[4] { 256, 0, 0, 0 });
			_columnSetHederLines.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
			_columnSetHederLines.Name = "columnSetHederLines";
			_columnSetHederLines.Size = new Size(80, 21);
			_columnSetHederLines.TabIndex = 18;
			_columnSetHederLines.TextAlign = HorizontalAlignment.Right;
			_columnSetHederLines.Value = new decimal(new int[4] { 1, 0, 0, 0 });
			_columnSetHederLines.ValueChanged += OnColumnSetHederLinesValueChanged;
			_columnSetHeaderLinesLabel.AutoSize = true;
			_columnSetHeaderLinesLabel.Location = new Point(8, 34);
			_columnSetHeaderLinesLabel.Name = "label3";
			_columnSetHeaderLinesLabel.Size = new Size(121, 13);
			_columnSetHeaderLinesLabel.TabIndex = 17;
			_columnSetHeaderLinesLabel.Text = "ColumnSetHeaderLines:";
			_columnSetRowCount.Location = new Point(128, 4);
			_columnSetRowCount.Maximum = new decimal(new int[4] { 256, 0, 0, 0 });
			_columnSetRowCount.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
			_columnSetRowCount.Name = "columnSetRowCount";
			_columnSetRowCount.Size = new Size(80, 21);
			_columnSetRowCount.TabIndex = 16;
			_columnSetRowCount.TextAlign = HorizontalAlignment.Right;
			_columnSetRowCount.Value = new decimal(new int[4] { 1, 0, 0, 0 });
			_columnSetRowCount.ValueChanged += OnColumnSetRowCountValueChanged;
			_label.AutoSize = true;
			_label.Location = new Point(8, 7);
			_label.Name = "label1";
			_label.Size = new Size(112, 13);
			_label.TabIndex = 15;
			_label.Text = "ColumnSetRowCount:";
			_cmdRemove.Enabled = false;
			_cmdRemove.Location = new Point(112, 8);
			_cmdRemove.Name = "cmdRemove";
			_cmdRemove.Size = new Size(96, 24);
			_cmdRemove.TabIndex = 12;
			_cmdRemove.Text = "Remove";
			_cmdRemove.Click += OnCmdRemoveClick;
			_cmdClearColumns.Location = new Point(328, 8);
			_cmdClearColumns.Name = "cmdClearColumns";
			_cmdClearColumns.Size = new Size(104, 24);
			_cmdClearColumns.TabIndex = 13;
			_cmdClearColumns.Text = "Remove Columns";
			_cmdClearColumns.Click += OnCmdClearColumnsClick;
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(395, 357);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 24);
			_btnOK.TabIndex = 14;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			_button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_button.DialogResult = DialogResult.Cancel;
			_button.Location = new Point(479, 357);
			_button.Name = "button1";
			_button.Size = new Size(80, 24);
			_button.TabIndex = 15;
			_button.Text = "Cancel";
			_button.Click += OnButtonClick;
			base.AcceptButton = _btnOK;
			base.CancelButton = _button;
			base.ClientSize = new Size(564, 386);
			base.Controls.Add(_button);
			base.Controls.Add(_btnOK);
			base.Controls.Add(_cmdClearColumns);
			base.Controls.Add(_cmdRemove);
			base.Controls.Add(_panel);
			base.Controls.Add(_lblDesigner);
			base.Controls.Add(_lblColumns);
			base.Controls.Add(_btnAdd);
			base.Controls.Add(_btnClear);
			base.Controls.Add(_designer);
			base.Controls.Add(_columnListBox);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "ColumnSetDesignerForm";
			base.StartPosition = FormStartPosition.CenterScreen;
			Text = "ColumnSets Designer";
			base.Load += OnControlLoad;
			_panel.ResumeLayout(performLayout: false);
			_columnSetsDesignerDialogPanel.ResumeLayout(performLayout: false);
			_columnSetsDesignerDialogPanel.PerformLayout();
			((ISupportInitialize)_headerLines).EndInit();
			((ISupportInitialize)_columnSetHederLines).EndInit();
			((ISupportInitialize)_columnSetRowCount).EndInit();
			ResumeLayout(performLayout: false);
		}

		private void OnBtnAddClick(object sender, EventArgs eventArgs)
		{
			GridColumnSet gridColumnSet = _targetTable.ColumnSets.Add();
			_designer.SetSelectedObject(gridColumnSet);
			_designer.RebuildLayout();
			UpdateColumnSetsDesignerDialog();
		}

		internal DialogResult ComputeDialogResult(MxDataGrid dataGrid, GridTable gridTable)
		{
			MemoryStream memoryStream = new MemoryStream();
			dataGrid.SaveLayoutFile(memoryStream);
			memoryStream.Position = 0L;
			_targetTable = gridTable;
			CellLayoutMode cellLayoutMode = gridTable.CellLayoutMode;
			gridTable.CellLayoutMode = CellLayoutMode.UseColumnSets;
			bool columnAutoResize = dataGrid.ColumnAutoResize;
			dataGrid.ColumnAutoResize = false;
			_columnHeadersSetting = gridTable.ColumnHeaders;
			_columnSetHeadersSetting = gridTable.ColumnSetHeaders;
			gridTable.ColumnHeaders = InheritableBoolean.True;
			gridTable.ColumnSetHeaders = InheritableBoolean.True;
			_dataGrid = dataGrid;
			_columnSetRowCount.Value = gridTable.ColumnSetRowCount;
			_columnSetHederLines.Value = gridTable.ColumnSetHeaderLines;
			_headerLines.Value = gridTable.HeaderLines;
			_columnHeaders.Items.Clear();
			_columnHeaders.Items.Add(InheritableBoolean.Default);
			_columnHeaders.Items.Add(InheritableBoolean.True);
			_columnHeaders.Items.Add(InheritableBoolean.False);
			_columnSetHeaders.Items.Add(InheritableBoolean.Default);
			_columnSetHeaders.Items.Add(InheritableBoolean.True);
			_columnSetHeaders.Items.Add(InheritableBoolean.False);
			_columnHeaders.SelectedItem = _columnHeadersSetting;
			_columnSetHeaders.SelectedItem = _columnSetHeadersSetting;
			_designer.InitializeControl(dataGrid, gridTable);
			_columnListBox.UpdateColumnListBoolean(true);
			_columnListBox.UpdateColumnListDataGrid(dataGrid);
			_columnListBox.CurrentTable = gridTable;
			_columnListBox.SetColumnsetDesigner(_designer);
			_columnListBox.Rows = 6;
			UpdateColumnSetsDesignerDialog();
			ShowDialog();
			gridTable.CellLayoutMode = cellLayoutMode;
			gridTable.ColumnHeaders = _columnHeadersSetting;
			gridTable.ColumnSetHeaders = _columnSetHeadersSetting;
			_dataGrid.ColumnAutoResize = columnAutoResize;
			if (base.DialogResult == DialogResult.OK)
			{
				gridTable.CellLayoutMode = CellLayoutMode.UseColumnSets;
			}
			else
			{
				_dataGrid.LoadLayoutFile(memoryStream);
			}
			memoryStream.Close();
			memoryStream = null;
			return base.DialogResult;
		}

		private void UpdateColumnSetsDesignerDialog()
		{
			Size size = _designer.GetSizeWithScrollBars(new Size(_designer.ClientSize.Width, _designer.GetLayoutHeight()));
			if (size.Height < _columnListBox.Height)
			{
				_designer.Height = _columnListBox.Height;
			}
			else
			{
				_designer.Height = size.Height;
			}
			_panel.Top = _designer.Bottom + 4;
			base.ClientSize = new Size(base.ClientSize.Width, _panel.Bottom + 30);
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			UpdateColumnSetsDesignerDialog();
		}

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
		}

		private void OnColumnListChanged(object sender, EventArgs eventArgs)
		{
			_columnListBox.UpdateColumnList();
			_columnListBox.RebuildLayout();
		}

		private void OnDesignerSelectedObjectChange(object sender, EventArgs eventArgs)
		{
			bool enabled = true;
			object selectedObject;
			if (_designer.GetSelectedObject() is GridColumnSet)
			{
				GridColumnSet gridColumnSet = (GridColumnSet)_designer.GetSelectedObject();
				_selectedObjectLabel.Text = "ColumnSet[" + gridColumnSet.Index + "] - '" + gridColumnSet.Key + "'";
				selectedObject = gridColumnSet;
			}
			else if (_designer.GetSelectedObject() is GridColumn)
			{
				GridColumn gridColumn = (GridColumn)_designer.GetSelectedObject();
				_selectedObjectLabel.Text = "Column[" + gridColumn.Index + "] - '" + gridColumn.Key + "'";
				selectedObject = new GridColumnState(gridColumn);
			}
			else
			{
				enabled = false;
				_selectedObjectLabel.Text = "(none)";
				selectedObject = null;
			}
			_tablePropertyGrid.SelectedObject = selectedObject;
			_cmdRemove.Enabled = enabled;
		}

		private void OnPropertyGrid1PropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			_designer.RebuildLayout();
		}

		private void OnColumnSetRowCountValueChanged(object sender, EventArgs eventArgs)
		{
			int columnSetRowCount = (int)_columnSetRowCount.Value;
			_targetTable.ColumnSetRowCount = columnSetRowCount;
			_designer.RebuildLayout();
			UpdateColumnSetsDesignerDialog();
		}

		private void OnBtnClearClick(object sender, EventArgs eventArgs)
		{
			_targetTable.ColumnSets.Clear();
			_designer.SetSelectedObject(null);
			_designer.RebuildLayout();
			_designer.SetSelectedObject(null);
			UpdateColumnSetsDesignerDialog();
		}

		private void OnColumnSetHederLinesValueChanged(object sender, EventArgs eventArgs)
		{
			_targetTable.ColumnSetHeaderLines = (int)_columnSetHederLines.Value;
			_designer.RebuildLayout();
			UpdateColumnSetsDesignerDialog();
		}

		private void OnHeaderLinesValueChanged(object sender, EventArgs eventArgs)
		{
			_targetTable.HeaderLines = (int)_headerLines.Value;
			_designer.RebuildLayout();
			UpdateColumnSetsDesignerDialog();
		}

		private void OnCmdRemoveClick(object sender, EventArgs eventArgs)
		{
			try
			{
				if (_designer.GetSelectedObject() is GridColumnSet)
				{
					_targetTable.ColumnSets.Remove((GridColumnSet)_designer.GetSelectedObject());
				}
				else if (_designer.GetSelectedObject() is GridColumn)
				{
					GridColumn gridColumn = (GridColumn)_designer.GetSelectedObject();
					gridColumn._owner.Remove(gridColumn);
					_columnListBox.RebuildLayout();
				}
				_designer.RebuildLayout();
				_designer.SetSelectedObject(null);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void OnCmdClearColumnsClick(object sender, EventArgs eventArgs)
		{
			foreach (GridColumn column in _targetTable.Columns)
			{
				if (column._owner != null)
				{
					column._owner.Remove(column);
				}
			}
			_columnListBox.RebuildLayout();
			_designer.RebuildLayout();
			UpdateColumnSetsDesignerDialog();
			_designer.SetSelectedObject(null);
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			_targetTable.ColumnSetHeaders = (InheritableBoolean)_columnSetHeaders.SelectedItem;
			Close();
		}

		private void OnButtonClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnColumnSetHeadersSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_columnSetHeadersSetting = (InheritableBoolean)_columnSetHeaders.SelectedItem;
		}

		private void OnColumnHeadersSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_columnSetHeadersSetting = (InheritableBoolean)_columnHeaders.SelectedItem;
		}
	}
}
