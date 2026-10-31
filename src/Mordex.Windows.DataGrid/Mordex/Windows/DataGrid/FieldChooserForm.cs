using System;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class FieldChooserForm : Form
	{
		private MxMultiColumnComboBox _multiColumnCombo;

		private MxFieldChooser _fieldChooser;

		private MxDataGrid _dataGrid;

		private int _borderThickness;

		internal FieldChooserForm()
		{
			InitializeControls();
			Mordex.Windows.Common.ControlThemer.Attach(this);
		}

		private void AddTableItem(GridValueListItemCollection gridValueListItemCollection, GridTable gridTable)
		{
			string text = "";
			for (int i = 0; i < gridTable.HierarchyLevel; i++)
			{
				text += "   ";
			}
			text += gridTable.Caption;
			gridValueListItemCollection.Add(new GridValueListItem(gridTable, text, -1));
			AddChildTableItems(gridValueListItemCollection, gridTable.ChildTables);
		}

		private void AddChildTableItems(GridValueListItemCollection gridValueListItemCollection, GridChildTableCollection gridChildTableCollection)
		{
			foreach (GridTable item in gridChildTableCollection)
			{
				AddTableItem(gridValueListItemCollection, item);
			}
		}

		internal void ProcessDataGrid(MxDataGrid dataGrid, GridTable gridTable, bool condition)
		{
			try
			{
				_dataGrid = dataGrid;
				_fieldChooser.DataGrid = _dataGrid;
				_fieldChooser.Table = gridTable;
				int value = _fieldChooser.Height + 6;
				if (condition && _dataGrid.Hierarchical && _dataGrid.RootTable.HasChildTables())
				{
					if (_multiColumnCombo != null)
					{
						_multiColumnCombo.ValueChanged -= OnMultiColumnComboValueChanged;
						_multiColumnCombo.Dispose();
						_multiColumnCombo = null;
					}
					GridValueListItemCollection gridValueListItemCollection = new GridValueListItemCollection(null);
					AddTableItem(gridValueListItemCollection, _dataGrid.RootTable);
					switch (_fieldChooser.BorderStyle)
					{
					case BorderStyle.Flat:
					case BorderStyle.RaisedLight3D:
					case BorderStyle.SunkenLight3D:
						_borderThickness = 1;
						break;
					case BorderStyle.Raised:
					case BorderStyle.Sunken:
						_borderThickness = 2;
						break;
					default:
						_borderThickness = 0;
						break;
					}
					UpdateMultiColumnCombo(dataGrid, gridTable, gridValueListItemCollection);
					value += _multiColumnCombo.Height + 2;
					_fieldChooser.Location = new Point(2, _multiColumnCombo.Height + 6);
				}
				else
				{
					if (_multiColumnCombo != null)
					{
						_multiColumnCombo.ValueChanged -= OnMultiColumnComboValueChanged;
						_multiColumnCombo.Dispose();
						_multiColumnCombo = null;
					}
					_fieldChooser.Location = new Point(2, 4);
				}
				SetClientSizeCore(_fieldChooser.Width + 4, value);
				_ = base.Size;
				try
				{
					Control control = GridHelper.GetParentSafe(dataGrid);
					Point location = control.ClientRectangle.Location;
					int otherValue = control.PointToScreen(location).X + control.Bounds.Width + 4;
					if (otherValue + base.Width > SystemInformation.VirtualScreen.Right)
					{
						otherValue = SystemInformation.VirtualScreen.Right - base.Width;
					}
					location = dataGrid.ClientRectangle.Location;
					base.Location = new Point(otherValue, dataGrid.PointToScreen(location).Y);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
				base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
				base.MaximizeBox = false;
				base.MinimizeBox = false;
				_fieldChooser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			}
			finally
			{
			}
			Show();
		}

		private void UpdateMultiColumnCombo(MxDataGrid dataGrid, GridTable gridTable, GridValueListItemCollection gridValueListItemCollection)
		{
			_multiColumnCombo = new MxMultiColumnComboBox();
			_multiColumnCombo.DataSource = gridValueListItemCollection;
			_multiColumnCombo.ValueMember = "Value";
			_multiColumnCombo.DisplayMember = "Text";
			_multiColumnCombo.DropDownList.GridLines = GridLines.None;
			_multiColumnCombo.DropDownList.ColumnAutoResize = true;
			_multiColumnCombo.DropDownList.ColumnHeaders = InheritableBoolean.False;
			_multiColumnCombo.DropDownList.Columns.Add("Text", ColumnType.Text);
			_multiColumnCombo.Value = gridTable;
			_multiColumnCombo.ValueChanged += OnMultiColumnComboValueChanged;
			_multiColumnCombo.BackColor = dataGrid.ControlStyle.WindowColor;
			_multiColumnCombo.ForeColor = dataGrid.ControlStyle.WindowTextColor;
			_multiColumnCombo.ComboStyle = ComboStyle.DropDownList;
			_multiColumnCombo.Location = new Point(2, 4);
			_multiColumnCombo.Size = new Size(base.ClientRectangle.Width - 4, (int)Font.GetHeight() + 2 + _borderThickness * 2);
			_multiColumnCombo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			base.Controls.Add(_multiColumnCombo);
		}

		private void OnMultiColumnComboValueChanged(object sender, EventArgs eventArgs)
		{
			GridValueListItem gridValueListItem = (GridValueListItem)_multiColumnCombo.SelectedItem;
			if (gridValueListItem == null)
			{
				gridValueListItem = ((GridValueListItemCollection)_multiColumnCombo.DataSource)[0];
			}
			_fieldChooser.Table = (GridTable)gridValueListItem.Value;
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			base.Visible = false;
			if (_dataGrid != null)
			{
				_dataGrid.DisposeFieldChooser();
			}
			base.Owner = null;
			_dataGrid = null;
			_fieldChooser.DataGrid = null;
			_fieldChooser.Table = null;
		}

		private void InitializeControls()
		{
			_fieldChooser = new MxFieldChooser();
			SuspendLayout();
			_fieldChooser.Location = new Point(8, 40);
			_fieldChooser.Name = "columnList";
			_fieldChooser.Size = new Size(156, 248);
			_fieldChooser.TabIndex = 0;
			_fieldChooser.Text = "columnList1";
			base.ClientSize = new Size(170, 298);
			base.Controls.AddRange(new Control[1] { _fieldChooser });
			base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			base.Name = "FieldChooser";
			base.StartPosition = FormStartPosition.Manual;
			Text = "Field Chooser";
			ResumeLayout(performLayout: false);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (base.WindowState != FormWindowState.Normal)
			{
				base.WindowState = FormWindowState.Normal;
			}
		}

		internal bool IsCursorOverColumnList()
		{
			Point pt = PointToClient(Cursor.Position);
			Control control = null;
			try
			{
				control = GetChildAtPoint(pt);
			}
			finally
			{
			}
			if (control == _fieldChooser)
			{
				return true;
			}
			return false;
		}
	}
}
