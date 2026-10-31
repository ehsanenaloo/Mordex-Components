using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.DataGrid;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.FilterBuilder
{
	public class BuiltInTextEditor : Form
	{
		private const string IdValueColumn = "IDValue";

		private const string IdColumn = "ID";

		private const string DefaultValueColumn = "DefaultValue";

		private const string ValueColumn = "Value";

		private const string ResetColumn = "Reset";

		private const int ResetColumnWidth = 45;

		private const int IdColumnWidth = 100;

		private readonly BuiltInTextList _textList;

		private DataTable _dataTable;

		private MxButton _btnCancel;

		private MxButton _btnOK;

		private Mordex.Windows.DataGrid.MxDataGrid _grid;

		private MxGroupBox _grbBackground;

		public BuiltInTextEditor(BuiltInTextList textList)
		{
			InitializeControls();
			_textList = textList;
			FillGrid();
			_grid.ColumnButtonClick += OnGridColumnButtonClick;
			_grid.RootTable.Columns[ResetColumn].AllowSize = false;
			_grid.RootTable.Columns[ResetColumn].Width = ResetColumnWidth;
			_grid.RootTable.Columns[ResetColumn].TextAlignment = Mordex.Windows.DataGrid.TextAlignment.Center;
			_grid.RootTable.Columns[0].Width = IdColumnWidth;
			_grid.CellToolTip = CellToolTip.TruncatedText;
			Mordex.Windows.Common.ControlThemer.AttachDialog(this);
		}

		private void FillGrid()
		{
			_dataTable = new DataTable("BuiltInTexts");
			DataColumn idValueColumn = new DataColumn(IdValueColumn, typeof(int));
			_dataTable.Columns.Add(idValueColumn);
			DataColumn idColumn = new DataColumn(IdColumn, typeof(string));
			idColumn.ReadOnly = true;
			_dataTable.Columns.Add(idColumn);
			DataColumn defaultValueColumn = new DataColumn(DefaultValueColumn, typeof(string));
			defaultValueColumn.ReadOnly = true;
			_dataTable.Columns.Add(defaultValueColumn);
			DataColumn valueColumn = new DataColumn(ValueColumn, typeof(string));
			_dataTable.Columns.Add(valueColumn);
			foreach (BuiltInText textID in Enum.GetValues(typeof(BuiltInText)))
			{
				DataRow row = _dataTable.NewRow();
				row[idValueColumn] = (int)textID;
				row[idColumn] = textID.ToString();
				row[defaultValueColumn] = BuiltInTextList.DefaultText(textID);
				row[valueColumn] = _textList.CustomText(textID);
				_dataTable.Rows.Add(row);
			}
			_grid.DataSource = _dataTable;
		}

		private void OnGridColumnButtonClick(object sender, ColumnActionEventArgs eventArgs)
		{
			_grid.SetValue(ValueColumn, DBNull.Value);
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			foreach (DataRow row in _dataTable.Rows)
			{
				BuiltInText textID = (BuiltInText)row[IdValueColumn];
				object value = row[ValueColumn];
				_textList[textID] = value == null || value == DBNull.Value ? null : (string)value;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_dataTable?.Dispose();
			}
		}

		private void InitializeControls()
		{
			GridLayout gridLayout = new GridLayout();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(BuiltInTextEditor));
			_btnCancel = new MxButton();
			_btnOK = new MxButton();
			_grid = new Mordex.Windows.DataGrid.MxDataGrid();
			_grbBackground = new MxGroupBox();
			((ISupportInitialize)_grid).BeginInit();
			((ISupportInitialize)_grbBackground).BeginInit();
			_grbBackground.SuspendLayout();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(632, 376);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(86, 23);
			_btnCancel.TabIndex = 2;
			_btnCancel.Text = "Cancel";
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(540, 376);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(86, 23);
			_btnOK.TabIndex = 1;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			_grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_grid.ColumnAutoResize = true;
			gridLayout.LayoutString = componentResourceManager.GetString("gridLayout1.LayoutString");
			_grid.DesignTimeLayout = gridLayout;
			_grid.Font = new Font("Tahoma", 8.25f);
			_grid.GroupByBoxVisible = false;
			_grid.Location = new Point(0, 0);
			_grid.Name = "grid";
			_grid.RowHeaders = Mordex.Windows.DataGrid.InheritableBoolean.True;
			_grid.SaveSettings = false;
			_grid.Size = new Size(730, 367);
			_grid.TabIndex = 1;
			_grbBackground.BackgroundStyle = BackgroundStyle.Panel;
			_grbBackground.Controls.Add(_grid);
			_grbBackground.Controls.Add(_btnOK);
			_grbBackground.Controls.Add(_btnCancel);
			_grbBackground.Dock = DockStyle.Fill;
			_grbBackground.FrameStyle = Mordex.Windows.UI.Controls.FrameStyle.None;
			_grbBackground.Location = new Point(0, 0);
			_grbBackground.Name = "grbBackground";
			_grbBackground.Size = new Size(730, 411);
			_grbBackground.TabIndex = 4;
			base.ClientSize = new Size(730, 411);
			base.Controls.Add(_grbBackground);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "BuiltInTextEditor";
			Text = "Built-in texts editor";
			((ISupportInitialize)_grid).EndInit();
			((ISupportInitialize)_grbBackground).EndInit();
			_grbBackground.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}
	}
}
