using System;
using Mordex.Windows.DataGrid.Internal;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	public class ValueListDesigner : Form
	{
		private Container _container = new Container();

		private OpenFileDialog _openFileDialog;

		private Button _button;

		private CheckBox _checkBox;

		private Button _btnOK;

		private MxDataGrid _grid;

		private GridValueListItemCollection _gridValueListItemCollection;

		private Type _type;

		internal ValueListDesigner()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		internal void LoadValueListItems(GridValueListItemCollection gridValueListItemCollection, Type type, ImageList imageList)
		{
			if (_gridValueListItemCollection == gridValueListItemCollection)
			{
				return;
			}
			try
			{
				_gridValueListItemCollection = gridValueListItemCollection;
				_type = type;
				if (_gridValueListItemCollection != null)
				{
					_grid.SetDataBinding(gridValueListItemCollection, "");
					_grid.RetrieveStructure();
					_grid.RootTable.Columns["Value"].Position = 0;
					_grid.RootTable.Columns["Text"].Position = 1;
					_grid.RootTable.Columns["ImageIndex"].Position = 2;
					_grid.RootTable.Columns["Image"].Position = 3;
					GridColumn gridColumn = _grid.RootTable.Columns["ImageIndex"];
					gridColumn.HasValueList = true;
					if (imageList == null)
					{
						imageList = new ImageList();
					}
					_grid.ImageList = imageList;
					gridColumn.ValueList.Add(new GridValueListItem(-1, "(none)", -1));
					for (int i = 0; i < imageList.Images.Count; i++)
					{
						gridColumn.ValueList.Add(new GridValueListItem(i, "Images[" + i + "]", i));
					}
					gridColumn.ColumnType = ColumnType.ImageAndText;
					gridColumn.EditType = EditType.DropDownList;
					_grid.RootTable.Columns["Image"].ButtonStyle = ButtonStyle.Ellipsis;
					_grid.RootTable.Columns["Image"].BoundImageSize = new Size(16, 16);
					_grid.RootTable.Columns["Value"].EditType = EditType.TextBox;
					_grid.RootTable.Columns["Value"].FormatMode = FormatMode.UseStringFormat;
					_grid.RootTable.Columns["Value"].FormatString = "{0}";
					_checkBox.Checked = _gridValueListItemCollection.ReplaceValues;
				}
				else
				{
					_grid.SetDataBinding(null, "");
					_grid.RetrieveStructure();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void InitializeControls()
		{
			ResourceManager resourceManager = new ResourceManager(typeof(ValueListDesigner));
			_grid = new MxDataGrid();
			_openFileDialog = new OpenFileDialog();
			_button = new Button();
			_checkBox = new CheckBox();
			_btnOK = new Button();
			((ISupportInitialize)_grid).BeginInit();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_grid.AllowAddNew = InheritableBoolean.True;
			_grid.AllowDelete = InheritableBoolean.True;
			_grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_grid.ColumnAutoResize = true;
			_grid.DataMember = null;
			_grid.EmptyRows = true;
			_grid.FlatBorderColor = SystemColors.ControlDark;
			_grid.GroupByBoxVisible = false;
			_grid.Location = new Point(8, 32);
			_grid.Name = "grid";
			_grid.Size = new Size(344, 184);
			_grid.TabIndex = 0;
			_grid.UpdatingCell += OnGridUpdatingCell;
			_grid.ColumnButtonClick += OnGridColumnButtonClick;
			_grid.GetNewRow += OnGridGetNewRow;
			_grid.KeyDown += OnGridKeyDown;
			_openFileDialog.AddExtension = false;
			_openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.gif;*.ico|All Files|*.*";
			_openFileDialog.Title = "Open Picture File";
			_button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_button.Location = new Point(232, 4);
			_button.Name = "button1";
			_button.Size = new Size(120, 24);
			_button.TabIndex = 1;
			_button.Text = "Clear";
			_button.Click += OnButtonClick;
			_checkBox.Location = new Point(12, 8);
			_checkBox.Name = "checkBox1";
			_checkBox.Size = new Size(148, 20);
			_checkBox.TabIndex = 2;
			_checkBox.Text = "Replace Values";
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(260, 224);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(92, 24);
			_btnOK.TabIndex = 3;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			base.AcceptButton = _btnOK;
			base.ClientSize = new Size(360, 254);
			base.Controls.AddRange(new Control[4] { _btnOK, _checkBox, _button, _grid });
			base.Icon = (Icon)resourceManager.GetObject("$this.Icon");
			base.Name = "ValueListDesigner";
			base.StartPosition = FormStartPosition.CenterScreen;
			Text = "ValueList";
			base.Load += OnControlLoad;
			((ISupportInitialize)_grid).EndInit();
			ResumeLayout(performLayout: false);
		}

		private void OnGridUpdatingCell(object sender, UpdatingCellEventArgs eventArgs)
		{
			if (eventArgs.Column.Key == "Value" && _type != null)
			{
				try
				{
					eventArgs.Value = Convert.ChangeType(eventArgs.Value, _type);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
		}

		private void OnGridColumnButtonClick(object sender, ColumnActionEventArgs eventArgs)
		{
			if (eventArgs.Column.Key == "Image" && _openFileDialog.ShowDialog() == DialogResult.OK)
			{
				Image image = null;
				try
				{
					image = Image.FromFile(_openFileDialog.FileName);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					MessageBox.Show("Unable to open image file", "Mordex DataGrid Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				if (image != null)
				{
					_grid.SetValue(eventArgs.Column, image);
				}
			}
		}

		private void OnGridGetNewRow(object sender, GetNewRowEventArgs eventArgs)
		{
			eventArgs.NewRow = new GridValueListItem();
		}

		private void OnButtonClick(object sender, EventArgs eventArgs)
		{
			_gridValueListItemCollection.Clear();
			_grid.Refetch();
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
		}

		private void OnGridKeyDown(object sender, KeyEventArgs eventArgs)
		{
			if (eventArgs.KeyCode == Keys.Delete && _grid.CurrentColumn != null && string.Compare(_grid.CurrentColumn.Key, "Image", ignoreCase: true) == 0)
			{
				_grid.SetValue(_grid.CurrentColumn, null);
				eventArgs.Handled = true;
			}
		}
	}
}
