using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	[ToolboxItem(false)]
	internal class CompositeColumnsChooser : UserControl
	{
		private GridTable _targetTable;

		private List<GridColumn> columns;

		private IContainer components = default;

		private TableLayoutPanel _layoutPanel;

		private ListBox lbAvailable;

		private Label _availableLabel;

		private Label _groupColumnsLabel;

		private ListBox lbSelected;

		private Panel _buttonPanel;

		private Button btnMoveUp;

		private Button btnRemove;

		private Button btnAdd;

		private Button btnMoveDown;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal GridTable TargetTable
		{
			get
			{
				return _targetTable;
			}
			set
			{
				if (_targetTable != value)
				{
					_targetTable = value;
					OnTableChanged();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public GridColumn[] Columns
		{
			get
			{
				return columns.ToArray();
			}
			set
			{
				if (value != null)
				{
					columns = new List<GridColumn>(value);
				}
				else
				{
					columns = new List<GridColumn>();
				}
				FillControls(fillAvailable: true, fillSelected: true);
			}
		}

		public CompositeColumnsChooser()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			InitializeComponent();
			columns = new List<GridColumn>();
		}

		private void OnTableChanged()
		{
			FillControls(fillAvailable: true, fillSelected: false);
		}

		private void FillControls(bool fillAvailable, bool fillSelected)
		{
			if (fillAvailable)
			{
				lbAvailable.DisplayMember = "Text";
				lbAvailable.Items.Clear();
				if (_targetTable != null)
				{
					foreach (GridColumn column in _targetTable.Columns)
					{
						if (!columns.Contains(column))
						{
							lbAvailable.Items.Add(new GridValueListItem(column, GetName(column.Index, column)));
						}
					}
				}
				if (lbAvailable.Items.Count > 0)
				{
					lbAvailable.SelectedIndex = 0;
				}
			}
			if (fillSelected)
			{
				lbSelected.DisplayMember = "Text";
				lbSelected.Items.Clear();
				foreach (GridColumn otherColumn in columns)
				{
					lbSelected.Items.Add(new GridValueListItem(otherColumn, GetName(otherColumn.Index, otherColumn)));
				}
				if (lbSelected.Items.Count > 0)
				{
					lbSelected.SelectedIndex = 0;
				}
			}
			EnableButtons();
		}

		internal static string GetName(int index, object value)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(value).Find("Key", ignoreCase: true);
			string text = "";
			if (propertyDescriptor != null)
			{
				text = (string)propertyDescriptor.GetValue(value);
			}
			if (text.Length > 0)
			{
				return "[" + index + "] - '" + text + "'";
			}
			return "[" + index + "] - {" + value.GetType().Name + "}";
		}

		private void EnableButtons()
		{
			btnAdd.Enabled = lbAvailable.SelectedIndex >= 0;
			btnMoveDown.Enabled = lbSelected.SelectedIndex < lbSelected.Items.Count - 1;
			btnMoveUp.Enabled = lbSelected.SelectedIndex > 0;
			btnRemove.Enabled = lbSelected.SelectedIndex >= 0;
		}

		private void btnAdd_Click(object sender, EventArgs eventArgs)
		{
			DoAdd();
		}

		private void DoAdd()
		{
			if (lbAvailable.SelectedItem is GridValueListItem gridValueListItem)
			{
				int selectedIndex = lbAvailable.SelectedIndex;
				columns.Add((GridColumn)gridValueListItem.Value);
				FillControls(fillAvailable: true, fillSelected: true);
				if (selectedIndex < lbAvailable.Items.Count - 1)
				{
					lbAvailable.SelectedIndex = selectedIndex;
				}
				else
				{
					lbAvailable.SelectedIndex = lbAvailable.Items.Count - 1;
				}
			}
		}

		private void DoRemove()
		{
			if (lbSelected.SelectedItem is GridValueListItem gridValueListItem)
			{
				int selectedIndex = lbSelected.SelectedIndex;
				columns.Remove((GridColumn)gridValueListItem.Value);
				FillControls(fillAvailable: true, fillSelected: true);
				if (selectedIndex < lbSelected.Items.Count - 1)
				{
					lbSelected.SelectedIndex = selectedIndex;
				}
				else
				{
					lbSelected.SelectedIndex = lbSelected.Items.Count - 1;
				}
			}
		}

		private void lbAvailable_DoubleClick(object sender, EventArgs eventArgs)
		{
			DoAdd();
		}

		private void lbSelected_SelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			EnableButtons();
		}

		private void btnRemove_Click(object sender, EventArgs eventArgs)
		{
			DoRemove();
		}

		private void lbSelected_DoubleClick(object sender, EventArgs eventArgs)
		{
			DoRemove();
		}

		private void btnMoveUp_Click(object sender, EventArgs eventArgs)
		{
			int selectedIndex = lbSelected.SelectedIndex;
			GridColumn item = columns[selectedIndex];
			columns.RemoveAt(selectedIndex);
			selectedIndex--;
			columns.Insert(selectedIndex, item);
			FillControls(fillAvailable: false, fillSelected: true);
			lbSelected.SelectedIndex = selectedIndex;
		}

		private void btnMoveDown_Click(object sender, EventArgs eventArgs)
		{
			int selectedIndex = lbSelected.SelectedIndex;
			GridColumn item = columns[selectedIndex];
			columns.RemoveAt(selectedIndex);
			selectedIndex++;
			columns.Insert(selectedIndex, item);
			FillControls(fillAvailable: false, fillSelected: true);
			lbSelected.SelectedIndex = selectedIndex;
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
			this._layoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.btnMoveDown = new System.Windows.Forms.Button();
			this.lbSelected = new System.Windows.Forms.ListBox();
			this.lbAvailable = new System.Windows.Forms.ListBox();
			this._availableLabel = new System.Windows.Forms.Label();
			this._groupColumnsLabel = new System.Windows.Forms.Label();
			this._buttonPanel = new System.Windows.Forms.Panel();
			this.btnRemove = new System.Windows.Forms.Button();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnMoveUp = new System.Windows.Forms.Button();
			this._layoutPanel.SuspendLayout();
			this._buttonPanel.SuspendLayout();
			base.SuspendLayout();
			this._layoutPanel.ColumnCount = 4;
			this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
			this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90f));
			this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25f));
			this._layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25f));
			this._layoutPanel.Controls.Add(this.btnMoveDown, 3, 2);
			this._layoutPanel.Controls.Add(this.lbSelected, 2, 1);
			this._layoutPanel.Controls.Add(this.lbAvailable, 0, 1);
			this._layoutPanel.Controls.Add(this._availableLabel, 0, 0);
			this._layoutPanel.Controls.Add(this._groupColumnsLabel, 2, 0);
			this._layoutPanel.Controls.Add(this._buttonPanel, 1, 1);
			this._layoutPanel.Controls.Add(this.btnMoveUp, 2, 2);
			this._layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._layoutPanel.Location = new System.Drawing.Point(0, 0);
			this._layoutPanel.Name = "tableLayoutPanel1";
			this._layoutPanel.RowCount = 3;
			this._layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
			this._layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
			this._layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28f));
			this._layoutPanel.Size = new System.Drawing.Size(680, 363);
			this._layoutPanel.TabIndex = 0;
			this.btnMoveDown.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnMoveDown.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnMoveDown.Location = new System.Drawing.Point(535, 338);
			this.btnMoveDown.Name = "btnMoveDown";
			this.btnMoveDown.Size = new System.Drawing.Size(142, 22);
			this.btnMoveDown.TabIndex = 5;
			this.btnMoveDown.Text = "Move Down";
			this.btnMoveDown.UseVisualStyleBackColor = true;
			this.btnMoveDown.Click += new System.EventHandler(btnMoveDown_Click);
			this._layoutPanel.SetColumnSpan(this.lbSelected, 2);
			this.lbSelected.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbSelected.FormattingEnabled = true;
			this.lbSelected.Location = new System.Drawing.Point(388, 23);
			this.lbSelected.Name = "lbSelected";
			this.lbSelected.Size = new System.Drawing.Size(289, 303);
			this.lbSelected.TabIndex = 3;
			this.lbSelected.DoubleClick += new System.EventHandler(lbSelected_DoubleClick);
			this.lbSelected.SelectedIndexChanged += new System.EventHandler(lbSelected_SelectedIndexChanged);
			this.lbAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbAvailable.FormattingEnabled = true;
			this.lbAvailable.Location = new System.Drawing.Point(3, 23);
			this.lbAvailable.Name = "lbAvailable";
			this.lbAvailable.Size = new System.Drawing.Size(289, 303);
			this.lbAvailable.TabIndex = 0;
			this.lbAvailable.DoubleClick += new System.EventHandler(lbAvailable_DoubleClick);
			this._availableLabel.AutoSize = true;
			this._availableLabel.Location = new System.Drawing.Point(4, 4);
			this._availableLabel.Margin = new System.Windows.Forms.Padding(4);
			this._availableLabel.Name = "label1";
			this._availableLabel.Size = new System.Drawing.Size(96, 12);
			this._availableLabel.TabIndex = 1;
			this._availableLabel.Text = "Available Columns:";
			this._groupColumnsLabel.AutoSize = true;
			this._layoutPanel.SetColumnSpan(this._groupColumnsLabel, 2);
			this._groupColumnsLabel.Location = new System.Drawing.Point(389, 4);
			this._groupColumnsLabel.Margin = new System.Windows.Forms.Padding(4);
			this._groupColumnsLabel.Name = "label2";
			this._groupColumnsLabel.Size = new System.Drawing.Size(98, 12);
			this._groupColumnsLabel.TabIndex = 2;
			this._groupColumnsLabel.Text = "Columns To Group:";
			this._buttonPanel.Controls.Add(this.btnRemove);
			this._buttonPanel.Controls.Add(this.btnAdd);
			this._buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._buttonPanel.Location = new System.Drawing.Point(298, 23);
			this._buttonPanel.Name = "panel1";
			this._buttonPanel.Size = new System.Drawing.Size(84, 309);
			this._buttonPanel.TabIndex = 4;
			this.btnRemove.Location = new System.Drawing.Point(0, 38);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(84, 24);
			this.btnRemove.TabIndex = 1;
			this.btnRemove.Text = "<- Remove";
			this.btnRemove.UseVisualStyleBackColor = true;
			this.btnRemove.Click += new System.EventHandler(btnRemove_Click);
			this.btnAdd.Location = new System.Drawing.Point(0, 8);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(84, 24);
			this.btnAdd.TabIndex = 0;
			this.btnAdd.Text = "Add ->";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(btnAdd_Click);
			this.btnMoveUp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnMoveUp.Location = new System.Drawing.Point(388, 338);
			this.btnMoveUp.Name = "btnMoveUp";
			this.btnMoveUp.Size = new System.Drawing.Size(141, 22);
			this.btnMoveUp.TabIndex = 4;
			this.btnMoveUp.Text = "Move Up";
			this.btnMoveUp.UseVisualStyleBackColor = true;
			this.btnMoveUp.Click += new System.EventHandler(btnMoveUp_Click);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this._layoutPanel);
			base.Name = "CompositeColumnsChooser";
			base.Size = new System.Drawing.Size(680, 363);
			this._layoutPanel.ResumeLayout(false);
			this._layoutPanel.PerformLayout();
			this._buttonPanel.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}
