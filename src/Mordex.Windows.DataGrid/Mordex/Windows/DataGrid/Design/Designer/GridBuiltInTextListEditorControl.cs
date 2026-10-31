using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	internal class GridBuiltInTextListEditorControl : UserControl, IGridElement
	{
		private Container _container = new Container();

		private GridBuiltInTextList _textList;

		private DataTable _dataTable;

		private MxDataGrid _grid;

		private object _editedObject;

		private static object _dataChangedEventKey = new object();

		private static object _gridBuiltInTextListEditorControlValue = new object();

		private EventHandler _dataChangedHandler;

		object IGridElement.SelectedObject
		{
			get
			{
				return _editedObject;
			}
			set
			{
				_editedObject = value;
			}
		}

		Image IGridElement.ItemImage
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		event EventHandler IGridElement.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(_dataChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dataChangedEventKey, value);
			}
		}

		event EventHandler IGridElement.RefreshTree
		{
			add
			{
				base.Events.AddHandler(_gridBuiltInTextListEditorControlValue, value);
			}
			remove
			{
				base.Events.RemoveHandler(_gridBuiltInTextListEditorControlValue, value);
			}
		}

		public GridBuiltInTextListEditorControl(GridBuiltInTextList textList)
		{
			InitializeControls();
			_textList = textList;
			UpdateDataTable();
			_grid.ColumnButtonClick += OnGridColumnButtonClick;
			_grid.RootTable.Columns["Reset"].AllowSize = false;
			_grid.RootTable.Columns["Reset"].Width = 45;
			_grid.RootTable.Columns["Reset"].TextAlignment = TextAlignment.Center;
			_grid.RootTable.Columns[0].Width = 100;
			_grid.CellToolTip = CellToolTip.TruncatedText;
		}

		private void OnGridColumnButtonClick(object sender, ColumnActionEventArgs eventArgs)
		{
			_grid.SetValue("Value", DBNull.Value);
		}

		private void UpdateDataTable()
		{
			_dataTable = new DataTable("BuiltInTexts");
			DataColumn column = new DataColumn("IDValue", typeof(int));
			_dataTable.Columns.Add(column);
			DataColumn dataColumn = new DataColumn("ID", typeof(string));
			dataColumn.ReadOnly = true;
			_dataTable.Columns.Add(dataColumn);
			DataColumn otherDataColumn = new DataColumn("DefaultValue", typeof(string));
			otherDataColumn.ReadOnly = true;
			_dataTable.Columns.Add(otherDataColumn);
			DataColumn nextDataColumn = new DataColumn("Value", typeof(string));
			_dataTable.Columns.Add(nextDataColumn);
			int value = Enum.GetNames(typeof(GridBuiltInText)).Length;
			for (int i = 0; i < value; i++)
			{
				GridBuiltInText gridBuiltInText = (GridBuiltInText)i;
				DataRow dataRow = _dataTable.NewRow();
				dataRow[column] = i;
				dataRow[dataColumn] = gridBuiltInText.ToString();
				dataRow[otherDataColumn] = _textList.GetDefaultText(gridBuiltInText);
				dataRow[nextDataColumn] = _textList.GetCustomText(gridBuiltInText);
				_dataTable.Rows.Add(dataRow);
			}
			_grid.DataSource = _dataTable;
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
			GridLayout gridLayout = new GridLayout();
			_grid = new MxDataGrid();
			((ISupportInitialize)_grid).BeginInit();
			SuspendLayout();
			_grid.ColumnAutoResize = true;
			gridLayout.LayoutString = "<GridLayoutData><RootTable><Columns Collection=\"true\"><Column0 ID=\"ID\"><DataMember>ID</DataMember><EditType>NoEdit</EditType><Key>ID</Key><Position>0</Position><Selectable>False</Selectable><Width>146</Width></Column0><Column1 ID=\"DefaultValue\"><Caption>Default Value</Caption><DataMember>DefaultValue</DataMember><EditType>NoEdit</EditType><Key>DefaultValue</Key><Position>1</Position><Selectable>False</Selectable><Width>146</Width></Column1><Column2 ID=\"Value\"><Caption>Value</Caption><DataMember>Value</DataMember><Key>Value</Key><Position>2</Position><Width>68</Width></Column2><Column3 ID=\"Reset\"><Bound>False</Bound><ButtonText>Reset</ButtonText><ButtonStyle>ButtonCell</ButtonStyle><EditType>NoEdit</EditType><Key>Reset</Key><Position>3</Position><Width>68</Width></Column3></Columns><GroupCondition ID=\"\" /></RootTable></GridLayoutData>";
			_grid.DesignTimeLayout = gridLayout;
			_grid.Dock = DockStyle.Fill;
			_grid.EditorsControlStyle.ButtonAppearance = ButtonAppearance.Regular;
			_grid.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_grid.GroupByBoxVisible = false;
			_grid.Name = "grid";
			_grid.Size = new Size(432, 360);
			_grid.TabIndex = 0;
			base.Controls.AddRange(new Control[1] { _grid });
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "BuiltInTextsPage";
			base.Size = new Size(432, 360);
			((ISupportInitialize)_grid).EndInit();
			ResumeLayout(performLayout: false);
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		bool IGridElement.CommitChanges()
		{
			_grid.UpdateData();
			foreach (DataRow row in _dataTable.Rows)
			{
				GridBuiltInText textID = (GridBuiltInText)row["IDValue"];
				object rowItem = row["Value"];
				if (rowItem == null || rowItem == DBNull.Value)
				{
					_textList[textID] = null;
				}
				else
				{
					_textList[textID] = (string)rowItem;
				}
			}
			return true;
		}

		void IGridElement.SetSite(ISite site)
		{
		}

		protected virtual void OnDisableCancel(EventArgs eventArgs)
		{
		}

		bool IGridElement.Support(ToolbarOptions option)
		{
			return false;
		}
	}
}
