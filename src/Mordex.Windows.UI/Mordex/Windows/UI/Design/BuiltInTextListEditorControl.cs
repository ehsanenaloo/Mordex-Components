using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	internal class BuiltInTextListEditorControl : UserControl, IUIElement
	{
		private const int EditorWidth = 432;

		private const int EditorHeight = 360;

		private const float EditorFontSize = 8.25f;

		private const double IdColumnWidthRatio = 0.2;

		private const double DefaultColumnWidthRatio = 0.4;

		private const int MinProposedColumnWidth = 20;

		private Container _components = new Container();

		private Mordex.Windows.UI.CommandBars.BuiltInTextList _commandTextList;

		private Mordex.Windows.UI.Docking.BuiltInTextList _panelTextList;

		private DataTable _dataTable;

		private DataGridView _grid;

		private int _lastGridWidth;

		private object _selectedObject;

		private static object _selectedObjectChangedKey = new object();

		private static object _refreshTreeKey = new object();

		private EventHandler _dataChangedHandler;

		private EventHandler _disableCancelHandler;

		object IUIElement.SelectedObject
		{
			get
			{
				return _selectedObject;
			}
			set
			{
				_selectedObject = value;
			}
		}

		Image IUIElement.ItemImage
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
				EventHandler currentHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _dataChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
			remove
			{
				EventHandler currentHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _dataChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
		}

		event EventHandler IUIElement.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(_selectedObjectChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedObjectChangedKey, value);
			}
		}

		event EventHandler IUIElement.RefreshTree
		{
			add
			{
				base.Events.AddHandler(_refreshTreeKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_refreshTreeKey, value);
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler currentHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _disableCancelHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
			remove
			{
				EventHandler currentHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _disableCancelHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
		}

		public BuiltInTextListEditorControl(Mordex.Windows.UI.CommandBars.BuiltInTextList textList)
		{
			InitializeControls();
			_commandTextList = textList;
			UpdateDataTable();
		}

		public BuiltInTextListEditorControl(Mordex.Windows.UI.Docking.BuiltInTextList textList)
		{
			InitializeControls();
			_panelTextList = textList;
			UpdateDataTable();
		}

		private void UpdateDataTable()
		{
			_dataTable = new DataTable("BuiltInTexts");
			DataColumn idValueColumn = new DataColumn("IDValue", typeof(int));
			_dataTable.Columns.Add(idValueColumn);
			DataColumn idColumn = new DataColumn("ID", typeof(string));
			idColumn.ReadOnly = true;
			_dataTable.Columns.Add(idColumn);
			DataColumn defaultValueColumn = new DataColumn("DefaultValue", typeof(string));
			defaultValueColumn.ReadOnly = true;
			_dataTable.Columns.Add(defaultValueColumn);
			DataColumn proposedValueColumn = new DataColumn("ProposedValue", typeof(string));
			_dataTable.Columns.Add(proposedValueColumn);
			if (_commandTextList != null)
			{
				int commandTextCount = Enum.GetNames(typeof(CommandsBuiltInText)).Length;
				for (int i = 0; i < commandTextCount; i++)
				{
					CommandsBuiltInText commandsBuiltInText = (CommandsBuiltInText)i;
					DataRow commandRow = _dataTable.NewRow();
					commandRow[idValueColumn] = i;
					commandRow[idColumn] = commandsBuiltInText.ToString();
					commandRow[defaultValueColumn] = _commandTextList.GetDefaultText(commandsBuiltInText);
					commandRow[proposedValueColumn] = _commandTextList.GetCustomText(commandsBuiltInText);
					_dataTable.Rows.Add(commandRow);
				}
			}
			else
			{
				int panelTextCount = Enum.GetNames(typeof(PanelsBuiltInText)).Length;
				for (int j = 0; j < panelTextCount; j++)
				{
					PanelsBuiltInText panelsBuiltInText = (PanelsBuiltInText)j;
					DataRow panelRow = _dataTable.NewRow();
					panelRow[idValueColumn] = j;
					panelRow[idColumn] = panelsBuiltInText.ToString();
					panelRow[defaultValueColumn] = _panelTextList.GetDefaultText(panelsBuiltInText);
					panelRow[proposedValueColumn] = _panelTextList.GetCustomText(panelsBuiltInText);
					_dataTable.Rows.Add(panelRow);
				}
			}
			_dataTable.DefaultView.AllowNew = false;
			_dataTable.DefaultView.Sort = "ID";
			_grid.DataSource = _dataTable;
			_grid.Columns["IDValue"].Visible = false;
			_grid.Columns["ID"].ReadOnly = true;
			_grid.Columns["DefaultValue"].HeaderText = "Default Value";
			_grid.Columns["DefaultValue"].ReadOnly = true;
			_grid.Columns["ProposedValue"].HeaderText = "Proposed Value";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_grid = new DataGridView();
			((ISupportInitialize)_grid).BeginInit();
			SuspendLayout();
			_grid.AllowUserToAddRows = false;
			_grid.AllowUserToDeleteRows = false;
			_grid.AutoGenerateColumns = true;
			_grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_grid.Dock = DockStyle.Fill;
			_grid.Name = "dataGrid1";
			_grid.RowHeadersVisible = false;
			_grid.Size = new Size(EditorWidth, EditorHeight);
			_grid.TabIndex = 0;
			_grid.Resize += OnGridResize;
			base.Controls.AddRange(new Control[1] { _grid });
			Font = new Font("Tahoma", EditorFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "BuiltInTextsPage";
			base.Size = new Size(EditorWidth, EditorHeight);
			((ISupportInitialize)_grid).EndInit();
			ResumeLayout(performLayout: false);
		}

		private void OnGridResize(object sender, EventArgs eventArgs)
		{
			DataGridViewColumn idColumn = _grid.Columns["ID"];
			DataGridViewColumn defaultColumn = _grid.Columns["DefaultValue"];
			DataGridViewColumn proposedColumn = _grid.Columns["ProposedValue"];
			if (idColumn == null || defaultColumn == null || proposedColumn == null)
			{
				return;
			}
			int idColumnWidth = (_lastGridWidth == 0) ? (int)((double)_grid.Width * IdColumnWidthRatio) : (int)((float)_grid.Width * ((float)idColumn.Width / (float)_lastGridWidth));
			int defaultColumnWidth = (int)((double)_grid.Width * DefaultColumnWidthRatio);
			idColumn.Width = idColumnWidth;
			defaultColumn.Width = defaultColumnWidth;
			proposedColumn.Width = Math.Max(MinProposedColumnWidth, _grid.Width - idColumnWidth - defaultColumnWidth - SystemInformation.VerticalScrollBarWidth);
			_lastGridWidth = _grid.Width;
		}

		bool IUIElement.CommitChanges()
		{
			foreach (DataRow row in _dataTable.Rows)
			{
				if (_commandTextList != null)
				{
					CommandsBuiltInText commandTextId = (CommandsBuiltInText)row["IDValue"];
					object commandProposedValue = row["ProposedValue"];
					if (commandProposedValue == null || commandProposedValue == DBNull.Value)
					{
						_commandTextList[commandTextId] = null;
					}
					else
					{
						_commandTextList[commandTextId] = (string)commandProposedValue;
					}
				}
				else
				{
					PanelsBuiltInText panelTextId = (PanelsBuiltInText)row["IDValue"];
					object panelProposedValue = row["ProposedValue"];
					if (panelProposedValue == null || panelProposedValue == DBNull.Value)
					{
						_panelTextList[panelTextId] = null;
					}
					else
					{
						_panelTextList[panelTextId] = (string)panelProposedValue;
					}
				}
			}
			return true;
		}

		void IUIElement.SetSite(ISite site)
		{
		}
	}
}
