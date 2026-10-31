using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class DropDownListForm : Form
	{
		internal string _okCaption;

		internal string _cancelCaption;

		private ILookUpTable _lookUpTable;

		private MxDataGrid _dataGrid;

		private Rectangle _anchorBounds;

		private GridGroupLayoutState _gridGroupLayoutState;

		private GridGroupLayoutState _gridEditorDialog2GridGroupLayoutState;

		private bool _areDropDownButtonsVisible;

		private BindingContext _bindingContext;

		internal Form _form;

		private IGridDropListOwner _dropDownOwner;

		private int _buttonAreaWidth;

		private ControlStyle _fallbackStyle;

		private GridGroupLayoutState _hotButton;

		private GridGroupLayoutState _pressedButton;

		private bool _isHideDropDownPending;

		internal DropDownListForm(ILookUpTable lookUpTable, ImageList imageList)
		{
			_bindingContext = new BindingContext();
			_areDropDownButtonsVisible = true;
			_okCaption = "OK";
			_cancelCaption = "Cancel";
			base.Size = new Size(100, 100);
			_lookUpTable = lookUpTable;
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.Manual;
			CreateListGrid();
			if (_dataGrid != null)
			{
				_dataGrid.ImageList = imageList;
			}
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		}

		internal bool GetDropDownButtonsVisible()
		{
			return _areDropDownButtonsVisible;
		}

		internal void SetDropDownButtonsVisible(bool condition)
		{
			if (_areDropDownButtonsVisible != condition)
			{
				_areDropDownButtonsVisible = condition;
				if (base.Visible)
				{
					ApplyAnchorBounds(_anchorBounds);
				}
			}
		}

		internal void SetDropDownOwner(IGridDropListOwner dropDownOwner)
		{
			_dropDownOwner = dropDownOwner;
		}

		private void CreateListGrid()
		{
			if (_lookUpTable is GridValueListItemCollection)
			{
				_dataGrid = new MxDataGrid();
				_dataGrid.BorderStyle = BorderStyle.Flat;
				_dataGrid._isEditorDialogOpen = true;
				_dataGrid.Parent = this;
				_dataGrid.RootTable = new GridTable();
				_dataGrid.GroupByBoxVisible = false;
				_dataGrid.ColumnHeaders = InheritableBoolean.False;
				GridColumn gridColumn = _dataGrid.RootTable.Columns.Add("Selector");
				gridColumn.ActAsSelector = true;
				gridColumn.Selectable = false;
				gridColumn._width = 16;
				gridColumn.AllowSize = false;
				gridColumn = _dataGrid.RootTable.Columns.Add("Text", ColumnType.ImageAndText);
				gridColumn.Selectable = false;
				_dataGrid.GridLines = GridLines.None;
				_dataGrid.FormattingRow += OnGridFormattingRow;
				_dataGrid.DataSource = (GridValueListItemCollection)_lookUpTable;
				_dataGrid.FocusStyle = FocusStyle.None;
				_dataGrid.EnterKeyBehavior = EnterKeyBehavior.None;
				_dataGrid.AcceptsEscape = false;
				_dataGrid.FocusCellFormatStyle.BackColor = _dataGrid.SelectedFormatStyle.BackColor;
				_dataGrid.FocusCellFormatStyle.ForeColor = _dataGrid.SelectedFormatStyle.ForeColor;
			}
			else if (_lookUpTable is GridDropDown)
			{
				_dataGrid = (MxDataGrid)_lookUpTable;
			}
			if (_dataGrid != null)
			{
				_dataGrid.BindingContext = _bindingContext;
			}
		}

		private void OnGridFormattingRow(object sender, RowLoadEventArgs eventArgs)
		{
			GridValueListItem gridValueListItem = (GridValueListItem)eventArgs.Row.DataRow;
			if (gridValueListItem != null)
			{
				if (gridValueListItem.Image != null)
				{
					eventArgs.Row.Cells["Text"].Image = gridValueListItem.Image;
				}
				else
				{
					eventArgs.Row.Cells["Text"].ImageIndex = gridValueListItem.ImageIndex;
				}
			}
		}

		private int GetButtonHeight()
		{
			return Font.Height + 8;
		}

		private int CalculateMinimumWidth()
		{
			Graphics graphics = CreateGraphics();
			int value = (int)graphics.MeasureString(_okCaption, Font).Width + 8;
			int otherValue = (int)graphics.MeasureString(_cancelCaption, Font).Width + 8;
			if (value < otherValue)
			{
				value = otherValue;
			}
			if (value < 60)
			{
				value = 60;
			}
			_buttonAreaWidth = value;
			graphics.Dispose();
			return _buttonAreaWidth * 2 + 12 + 4;
		}

		private void LayoutButtons()
		{
			if (_areDropDownButtonsVisible)
			{
				int buttonHeight = GetButtonHeight();
				int value = _buttonAreaWidth * 2 + 8;
				Rectangle rectangle = new Rectangle((base.Width - value) / 2, base.ClientRectangle.Bottom - buttonHeight - 6, _buttonAreaWidth, buttonHeight);
				GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CellColumnButton, null, null, rectangle);
				gridGroupLayoutState.SetButtonStyle(ButtonStyle.TextButton);
				_gridGroupLayoutState = gridGroupLayoutState;
				rectangle = new Rectangle((base.Width - value) / 2 + _buttonAreaWidth + 8, base.ClientRectangle.Bottom - buttonHeight - 6, _buttonAreaWidth, buttonHeight);
				gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CellColumnButton, null, null, rectangle);
				gridGroupLayoutState.SetButtonStyle(ButtonStyle.TextButton);
				_gridEditorDialog2GridGroupLayoutState = gridGroupLayoutState;
			}
		}

		internal object[] ComputeValueArray()
		{
			GridRow[] checkedRows = _dataGrid.GetCheckedRows();
			if (checkedRows != null && checkedRows.Length > 0)
			{
				GridColumn gridColumn = null;
				GridDropDown gridDropDown = _lookUpTable as GridDropDown;
				if (gridDropDown != null)
				{
					gridColumn = gridDropDown.FindColumnByKey(gridDropDown.ValueMember);
				}
				object[] array = new object[checkedRows.Length];
				for (int i = 0; i < checkedRows.Length; i++)
				{
					GridRow gridRow = checkedRows[i];
					if (gridDropDown != null)
					{
						if (gridColumn != null)
						{
							array[i] = gridRow.Cells[gridColumn].Value;
						}
						else
						{
							array[i] = gridDropDown.GetValueFromDataRow(gridRow.DataRow);
						}
					}
					else if (_lookUpTable is GridValueListItemCollection)
					{
						GridValueListItem gridValueListItem = (GridValueListItem)gridRow.DataRow;
						array[i] = gridValueListItem.Value;
					}
				}
				return array;
			}
			return new object[0];
		}

		internal void CheckSelectedValues(object value)
		{
			if (_dataGrid == null)
			{
				return;
			}
			_dataGrid.UnCheckAllRecords();
			if (!(value is IList list))
			{
				return;
			}
			if (_lookUpTable is GridDropDown gridDropDown)
			{
				GridColumn gridColumn = gridDropDown.FindColumnByKey(gridDropDown.ValueMember);
				if (gridColumn != null && list.Count > 10)
				{
					Hashtable hashtable = new Hashtable(list.Count);
					foreach (object item in list)
					{
						hashtable.Add(item, true);
					}
					MordexRow rootRow = gridDropDown.GetRowSourceManager().RootRow;
					if (rootRow == null)
					{
						return;
					}
					MordexRow[] dataRows = rootRow.GetDataRows(rootRow.Table);
					if (dataRows == null)
					{
						return;
					}
					MordexRow[] array = dataRows;
					foreach (MordexRow mordexRow in array)
					{
						object key = gridColumn.GetRowValue(mordexRow);
						if (hashtable.ContainsKey(key))
						{
							mordexRow.IsChecked = true;
						}
					}
					return;
				}
				{
					foreach (object listItem in list)
					{
						object dataRow = gridDropDown.FindItem(listItem);
						if (dataRow != null)
						{
							GridRow row = _dataGrid.GetRow(dataRow);
							if (row != null)
							{
								row.IsChecked = true;
							}
						}
					}
					return;
				}
			}
			foreach (object otherListItem in list)
			{
				IMordexValueItem itemFromValue = _lookUpTable.GetItemFromValue(otherListItem);
				if (itemFromValue != null)
				{
					GridRow otherRow = _dataGrid.GetRow(itemFromValue.DataRow);
					if (otherRow != null)
					{
						otherRow.IsChecked = true;
					}
				}
			}
		}

		internal void ResizeAndPosition()
		{
			_dataGrid.Location = new Point(0, 0);
			int value = GetButtonHeight() + 10;
			if (!_areDropDownButtonsVisible)
			{
				value = 0;
			}
			base.Height = _dataGrid.Height + value;
			LayoutButtons();
			Point location = new Point(_anchorBounds.X, _anchorBounds.Bottom + 1);
			if (location.X + base.Width > SystemInformation.VirtualScreen.Right)
			{
				location.X = SystemInformation.VirtualScreen.Right - base.Width;
			}
			if (location.X < SystemInformation.VirtualScreen.Left)
			{
				location.X = SystemInformation.VirtualScreen.Left;
			}
			if (location.Y + base.Height > SystemInformation.VirtualScreen.Bottom)
			{
				if (_anchorBounds.Y > SystemInformation.VirtualScreen.Bottom - _anchorBounds.Bottom)
				{
					location.Y = _anchorBounds.Y - base.Height;
					if (location.Y < SystemInformation.VirtualScreen.Top)
					{
						location.Y = SystemInformation.VirtualScreen.Top;
						base.Height = _anchorBounds.Y;
					}
				}
				else
				{
					base.Height = SystemInformation.VirtualScreen.Bottom - _anchorBounds.Bottom;
				}
			}
			base.Location = location;
		}

		internal void ApplyAnchorBounds(Rectangle rectangle)
		{
			if (_dataGrid == null)
			{
				return;
			}
			_anchorBounds = rectangle;
			if (!TryAutoSizeGrid())
			{
				return;
			}
			_dataGrid.Location = new Point(0, 0);
			int minimumWidth = CalculateMinimumWidth();
			if (minimumWidth > _dataGrid.Width)
			{
				base.Width = minimumWidth;
				_dataGrid.ColumnAutoResize = true;
				_dataGrid.Width = base.Width;
			}
			else
			{
				base.Width = _dataGrid.Width;
			}
			if (_dataGrid is GridDropDown)
			{
				((GridDropDown)_dataGrid).SetDropDownWidth(_dataGrid.Width);
			}
			int value = GetButtonHeight() + 10;
			if (!_areDropDownButtonsVisible)
			{
				value = 0;
			}
			base.Height = _dataGrid.Height + value;
			LayoutButtons();
			if (_dataGrid != null)
			{
				_dataGrid.Click += OnGridClick;
				_dataGrid.SizingColumn += OnGridSizingColumn;
			}
			Point location = new Point(rectangle.X, rectangle.Bottom + 1);
			if (location.X + base.Width > SystemInformation.VirtualScreen.Right)
			{
				location.X = SystemInformation.VirtualScreen.Right - base.Width;
			}
			if (location.X < SystemInformation.VirtualScreen.Left)
			{
				location.X = SystemInformation.VirtualScreen.Left;
			}
			if (location.Y + base.Height > SystemInformation.WorkingArea.Bottom)
			{
				if (rectangle.Y > SystemInformation.VirtualScreen.Bottom - rectangle.Bottom)
				{
					location.Y = rectangle.Y - base.Height;
					if (location.Y < SystemInformation.VirtualScreen.Top)
					{
						location.Y = SystemInformation.VirtualScreen.Top;
						base.Height = rectangle.Y;
					}
				}
				else
				{
					base.Height = SystemInformation.VirtualScreen.Bottom - rectangle.Bottom;
				}
			}
			base.Location = location;
			if (_form != null)
			{
				base.TopMost = true;
				Show(_form);
			}
			else
			{
				Show();
			}
			if (_dataGrid != null)
			{
				GridHelper.SelectAndFocus(_dataGrid);
			}
		}

		private void OnGridSizingColumn(object sender, SizingColumnEventArgs eventArgs)
		{
			eventArgs.Column.Width = eventArgs.ProposedWidth;
			eventArgs.Cancel = true;
			Invalidate();
		}

		internal ControlStyle ResolveControlStyle()
		{
			if (_dataGrid != null)
			{
				return _dataGrid.ControlStyle;
			}
			return _fallbackStyle;
		}

		internal void SetFallbackControlStyle(ControlStyle controlStyle)
		{
			_fallbackStyle = controlStyle;
		}

		internal static void DrawBackground(Graphics graphics, Rectangle bounds, ControlStyle controlStyle)
		{
			try
			{
				MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = null;
				mordexColorSchemeAppearanceState = controlStyle.GetLayoutState().GetEditControlsStyle();
				Color color = mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.Control);
				Brush brush = GdiCache.Brush(color);
				graphics.FillRectangle(brush, bounds);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			base.OnPaint(eventArgs);
			ControlStyle controlStyle = ResolveControlStyle();
			if (controlStyle != null)
			{
				DrawBackground(eventArgs.Graphics, base.ClientRectangle, controlStyle);
				MxColorSchemeAppearanceState drawingStyle = controlStyle.GetLayoutState().GetEditControlsStyle();
				controlStyle.DrawBorder(eventArgs.Graphics, base.ClientRectangle, BorderStyle.Flat, true);
				ControlStyleRenderer controlStyleRenderer = new ControlStyleRenderer(drawingStyle, controlStyle);
				if (_gridGroupLayoutState != null)
				{
					controlStyleRenderer.RenderContent(eventArgs.Graphics, _gridGroupLayoutState._rect, Font, _okCaption, _gridGroupLayoutState._isPressed, _gridGroupLayoutState._isHot, RightToLeft == RightToLeft.Yes);
				}
				if (_gridEditorDialog2GridGroupLayoutState != null)
				{
					controlStyleRenderer.RenderContent(eventArgs.Graphics, _gridEditorDialog2GridGroupLayoutState._rect, Font, _cancelCaption, _gridEditorDialog2GridGroupLayoutState._isPressed, _gridEditorDialog2GridGroupLayoutState._isHot, RightToLeft == RightToLeft.Yes);
				}
			}
		}

		private GridGroupLayoutState HitTestButton(int value, int otherValue)
		{
			if (_gridGroupLayoutState._rect.Contains(value, otherValue))
			{
				return _gridGroupLayoutState;
			}
			if (_gridEditorDialog2GridGroupLayoutState._rect.Contains(value, otherValue))
			{
				return _gridEditorDialog2GridGroupLayoutState;
			}
			return null;
		}

		private void SetHotButton(GridGroupLayoutState gridEditorDialog2GridGroupLayoutState2)
		{
			if (_hotButton != gridEditorDialog2GridGroupLayoutState2)
			{
				if (_hotButton != null)
				{
					_hotButton._isHot = false;
				}
				_hotButton = gridEditorDialog2GridGroupLayoutState2;
				if (_hotButton != null)
				{
					_hotButton._isHot = true;
				}
				Invalidate();
			}
		}

		private GridGroupLayoutState GetPressedButton()
		{
			return _pressedButton;
		}

		private void SetPressedButton(GridGroupLayoutState gridEditorDialog2GridGroupLayoutState3)
		{
			if (_pressedButton != gridEditorDialog2GridGroupLayoutState3)
			{
				if (_pressedButton != null)
				{
					_pressedButton._isPressed = false;
				}
				_pressedButton = gridEditorDialog2GridGroupLayoutState3;
				if (_pressedButton != null)
				{
					SetHotButton(null);
					_pressedButton._isPressed = true;
				}
				Invalidate();
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (eventArgs.Button == MouseButtons.Left)
			{
				SetPressedButton(HitTestButton(eventArgs.X, eventArgs.Y));
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (eventArgs.Button == MouseButtons.None)
			{
				SetHotButton(HitTestButton(eventArgs.X, eventArgs.Y));
			}
			else
			{
				if (GetPressedButton() == null)
				{
					return;
				}
				if (GetPressedButton()._rect.Contains(eventArgs.X, eventArgs.Y))
				{
					if (!GetPressedButton()._isPressed)
					{
						GetPressedButton()._isPressed = true;
						Invalidate();
					}
				}
				else if (GetPressedButton()._isPressed)
				{
					GetPressedButton()._isPressed = false;
					Invalidate();
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (GetPressedButton() != null)
			{
				if (GetPressedButton()._isPressed)
				{
					ClickButton(GetPressedButton());
				}
				SetPressedButton(null);
			}
		}

		private void ClickButton(GridGroupLayoutState gridGroupLayoutState)
		{
			if (gridGroupLayoutState == _gridGroupLayoutState)
			{
				ProcessHideDropDownAction(HideDropDownAction.Ok);
			}
			else if (gridGroupLayoutState == _gridEditorDialog2GridGroupLayoutState)
			{
				ProcessHideDropDownAction(HideDropDownAction.Cancel);
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			SetHotButton(null);
		}

		private bool TryAutoSizeGrid()
		{
			if (base.IsDisposed || base.Disposing)
			{
				return true;
			}
			if (_lookUpTable is GridValueListItemCollection)
			{
				_dataGrid.ColumnAutoResize = false;
				_dataGrid.RootTable.Columns["Text"].AutoSize();
				int fullWidth = 0;
				int height = 0;
				fullWidth = _dataGrid.RootTable.CalculateFullWidth();
				if (_dataGrid.RowCount > 8)
				{
					fullWidth += SystemInformation.VerticalScrollBarWidth;
				}
				Graphics graphics = CreateGraphics();
				int measureAutoRowHeight = _dataGrid.MeasureAutoRowHeight(graphics);
				height = ((_dataGrid.RowCount == 0) ? measureAutoRowHeight : ((_dataGrid.RowCount > 8) ? (8 * measureAutoRowHeight) : (_dataGrid.RowCount * measureAutoRowHeight)));
				if (fullWidth > SystemInformation.PrimaryMonitorSize.Width)
				{
					fullWidth = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width;
					height += SystemInformation.HorizontalScrollBarHeight;
				}
				if (height > SystemInformation.PrimaryMonitorMaximizedWindowSize.Height)
				{
					height = SystemInformation.PrimaryMonitorMaximizedWindowSize.Height;
				}
				Size size = new Size(fullWidth, height);
				while (_dataGrid.ClientSize != size)
				{
					_dataGrid.ClientSize = size;
					_dataGrid.CalculateLayoutIfReady();
				}
				if (_dataGrid.Width < _anchorBounds.Width)
				{
					_dataGrid.Width = _anchorBounds.Width;
					_dataGrid.ColumnAutoResize = true;
				}
				if (base.Width > SystemInformation.PrimaryMonitorSize.Width)
				{
					base.Width = SystemInformation.PrimaryMonitorSize.Width;
				}
				graphics.Dispose();
				graphics = null;
				_dataGrid.ColumnAutoResize = true;
			}
			else if (_lookUpTable is GridDropDown)
			{
				bool condition = false;
				foreach (GridColumn column in _dataGrid.RootTable.Columns)
				{
					if ((column.ActAsSelector || column.ShowRowSelector) && column.Visible)
					{
						condition = true;
					}
					column.Selectable = false;
				}
				if (!condition)
				{
					Mordex.Windows.Common.MxMessageBox.Show("GridDropDown must have a column with its ActAsSelector or ShowRowSelector property equal to true when used in a MultipleValues column.");
					return false;
				}
				_dataGrid.Parent = this;
				_dataGrid.Location = new Point(0, 0);
				((GridDropDown)_dataGrid).ResizeToFitRows(_anchorBounds.Width);
				_dataGrid.EnterKeyBehavior = EnterKeyBehavior.None;
				_dataGrid.AcceptsEscape = false;
			}
			return true;
		}

		private void OnGridClick(object sender, EventArgs eventArgs)
		{
			MxDataGrid dataGrid = (MxDataGrid)sender;
			if (dataGrid.HitTest() != GridArea.Cell)
			{
				return;
			}
			GridColumn gridColumn = dataGrid.ColumnFromPoint();
			if (gridColumn == null || !gridColumn.ActAsSelector)
			{
				return;
			}
			int rowPositionFromPoint = dataGrid.RowPositionFromPoint();
			if (rowPositionFromPoint >= 0 && rowPositionFromPoint < dataGrid.RowCount)
			{
				GridRow row = dataGrid.GetRow(rowPositionFromPoint);
				if (row != null)
				{
					dataGrid.Row = rowPositionFromPoint;
					dataGrid.ApplyRowCheckFromEditor(!row.IsChecked);
				}
			}
		}

		protected override void OnDeactivate(EventArgs eventArgs)
		{
			base.OnDeactivate(eventArgs);
			if (!_isHideDropDownPending)
			{
				if (_areDropDownButtonsVisible)
				{
					ProcessHideDropDownAction(HideDropDownAction.Deactivate);
				}
				else
				{
					ProcessHideDropDownAction(HideDropDownAction.Ok);
				}
			}
		}

		internal void ProcessHideDropDownAction(HideDropDownAction hideDropDownAction)
		{
			_isHideDropDownPending = true;
			if (_dataGrid != null)
			{
				_dataGrid.Click -= OnGridClick;
				_dataGrid.SizingColumn -= OnGridSizingColumn;
			}
			if (_dataGrid is GridDropDown)
			{
				_dataGrid.Parent = null;
			}
			if (_dropDownOwner != null)
			{
				_dropDownOwner.OnDropListHide(hideDropDownAction != HideDropDownAction.Ok);
			}
			if (_dropDownOwner is Control control && control.IsHandleCreated && NativeMethods.IsWindowVisible(control.Handle))
			{
				GridHelper.SelectAndFocus(_dropDownOwner as Control);
			}
			Hide();
			_isHideDropDownPending = false;
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Escape:
				ProcessHideDropDownAction(HideDropDownAction.Cancel);
				return true;
			case Keys.Return:
				ProcessHideDropDownAction(HideDropDownAction.Ok);
				return true;
			default:
				return false;
			}
		}
	}
}
