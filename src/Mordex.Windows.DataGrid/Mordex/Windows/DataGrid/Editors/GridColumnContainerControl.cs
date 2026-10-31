using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	internal class GridColumnContainerControl : ContainerControl, IMordexContainer
	{
		internal class ColumnContainerAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleObject Parent
			{
				get
				{
					GridColumnContainerControl gridColumnContainerControl = (GridColumnContainerControl)base.Owner;
					if (gridColumnContainerControl.Parent is MxDataGrid dataGrid)
					{
						return dataGrid.GetEditCellAccessibleObject();
					}
					return null;
				}
			}

			public override AccessibleRole Role
			{
				get
				{
					GridColumnContainerControl owner = (GridColumnContainerControl)base.Owner;
					switch (owner.GetCurrentEditType())
					{
					case EditType.CheckBox:
						return AccessibleRole.CheckButton;
					case EditType.NoEdit:
						return AccessibleRole.StaticText;
					default:
						return AccessibleRole.Text;
					}
				}
			}

			public override string Value
			{
				get
				{
					return string.Concat(((GridColumnContainerControl)base.Owner).ReadEditorValue());
				}
				set
				{
					((GridColumnContainerControl)base.Owner).SetEditorText(value);
				}
			}

			public override string Name
			{
				get
				{
					if (((GridColumnContainerControl)base.Owner).Parent is MxDataGrid dataGrid && !string.IsNullOrEmpty(dataGrid.Name))
					{
						return dataGrid.Name + "_CellEditor";
					}
					return "CellEditor";
				}
			}

			internal ColumnContainerAccessibleObject(GridColumnContainerControl owner)
				: base(owner)
			{
			}

			public override int GetChildCount()
			{
				GridColumnContainerControl gridColumnContainerControl = (GridColumnContainerControl)base.Owner;
				if (gridColumnContainerControl != null && gridColumnContainerControl._editorState != null && gridColumnContainerControl._editorState.EditControl != null)
				{
					return 1;
				}
				return 0;
			}

			public override AccessibleObject GetChild(int index)
			{
				if (index == 0)
				{
					GridColumnContainerControl gridColumnContainerControl = (GridColumnContainerControl)base.Owner;
					if (gridColumnContainerControl != null && gridColumnContainerControl._editorState != null && gridColumnContainerControl._editorState.EditControl != null)
					{
						return gridColumnContainerControl._editorState.EditControl.AccessibilityObject;
					}
				}
				throw new ArgumentOutOfRangeException();
			}
		}

		private GridColumn _gridColumn;

		private CellEditor _editorState;

		private ControlStyle _editorStyle;

		private object _originalValue;

		private bool _isFilterMode;

		private EventHandler _editorEventHandler;

		private EventHandler _editorValueListHandler;

		private GridEventCallback _callbackHandler;

		private Rectangle _editBounds;

		private Rectangle _requestedEditBounds;

		private ColumnContainerAccessibleObject _containerAccessibleObject;

		public GridColumnContainerControl()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			base.CausesValidation = false;
			base.Visible = false;
		}

		internal bool IsCustomListLayout()
		{
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				base.ActiveControl = null;
			}
			base.Dispose(disposing);
		}

		internal void NotifyDropDownButtonMouseDown()
		{
			if (_editorState != null)
			{
				_editorState.OnDropDownButtonMouseDown();
			}
		}

		internal void NotifyUpDownButtonClick(int button)
		{
			if (_editorState != null)
			{
				_editorState.OnUpDownButtonClick(button);
			}
		}

		internal GridColumn GetColumn()
		{
			if (_editorState != null)
			{
				return _editorState.GetColumn();
			}
			return null;
		}

		internal void NotifyButtonMouseUp()
		{
			if (_editorState != null)
			{
				_editorState.OnButtonMouseUp();
			}
		}

		internal Control GetDropControl()
		{
			if (_editorState != null)
			{
				return _editorState.DropControl;
			}
			return null;
		}

		internal void SuspendEditorUpDownRepeater()
		{
			if (_editorState != null)
			{
				_editorState.SuspendUpDownRepeater();
			}
		}

		internal void ResumeEditorUpDownRepeater()
		{
			if (_editorState != null)
			{
				_editorState.ResumeUpDownRepeater();
			}
		}

		internal void DropEditorList()
		{
			if (_editorState != null)
			{
				_editorState.ShowDropList();
			}
		}

		internal void StartEditorWithChar(char editChar)
		{
			if (_editorState != null)
			{
				_editorState.InitEdit(editChar);
			}
		}

		internal ControlStyle GetEditorStyle()
		{
			return _editorStyle;
		}

		internal void SetEditorStyle(ControlStyle controlStyle)
		{
			if (_editorStyle != controlStyle)
			{
				_editorStyle = controlStyle;
				if (_editorState != null)
				{
					_editorState.SetControlStyle(_editorStyle);
				}
			}
		}

		internal void ConfigureForCell(MxDataGrid grid, GridColumn gridColumn, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control customControl)
		{
			_isFilterMode = false;
			switch (cell.EditType)
			{
			case EditType.TextBox:
				_editorState = new ValueListCellEditor(this);
				break;
			case EditType.Combo:
			case EditType.DropDownList:
				if (gridColumn.MultipleValues)
				{
					_editorState = new MultiValueCellEditor(this);
				}
				else
				{
					_editorState = new ValueListCellEditor(this);
				}
				break;
			case EditType.CheckBox:
				_editorState = new CheckBoxCellEditor(this);
				break;
			case EditType.CalendarDropDown:
			case EditType.DatePicker:
				_editorState = new CalendarCellEditor(this);
				break;
			case EditType.IntegerUpDown:
			case EditType.IntegerUpDownCombo:
				_editorState = new IntegerUpDownCellEditor(this);
				break;
			case EditType.ValueListUpDown:
			case EditType.ValueListUpDownCombo:
				_editorState = new ValueListUpDownCellEditor(this);
				break;
			case EditType.MultiColumnCombo:
			case EditType.MultiColumnDropDown:
				if (gridColumn.MultipleValues)
				{
					_editorState = new MultiValueCellEditor(this);
				}
				else
				{
					_editorState = new MultiColumnComboCellEditor(this);
				}
				break;
			case EditType.Custom:
				_editorState = new CustomCellEditor(this, customControl);
				break;
			}
			if (_editorState != null)
			{
				SetOriginalValue(value);
				_gridColumn = gridColumn;
				_editorState.EditType = cell.EditType;
				_editorState.SetControlStyle(GetEditorStyle());
				_editorState.StartEdit(grid, gridColumn, cell, style, value, image, capture, customControl);
				if (GetActiveEditControl() != null)
				{
					GetActiveEditControl().Parent = this;
				}
				AttachEditControlEvents();
				SetModified(false);
				ApplyEditBounds();
			}
		}

		protected override void OnVisibleChanged(EventArgs eventArgs)
		{
			base.OnVisibleChanged(eventArgs);
		}

		internal void ApplyComboVirtualWidth(int virtualWidth)
		{
			if (_editorState != null)
			{
				_editorState.SetComboVirtualWidth(virtualWidth);
			}
		}

		internal void ToggleEditorCheckBox()
		{
			if (_editorState != null)
			{
				_editorState.ToggleCheckBoxValue();
			}
		}

		internal void SetEditorText(string text)
		{
			if (_editorState != null)
			{
				_editorState.SetTextValue(text);
			}
		}

		internal object ReadEditorValue()
		{
			if (_editorState != null)
			{
				return _editorState.Value;
			}
			return null;
		}

		internal void WriteEditorValue(object value)
		{
			if (_editorState != null)
			{
				_editorState.Value = value;
			}
		}

		internal static EditType GetFilterEditType(GridColumn gridColumn)
		{
			switch (gridColumn.FilterEditType)
			{
			case FilterEditType.DatePicker:
				return EditType.DatePicker;
			case FilterEditType.CalendarDropDown:
				return EditType.CalendarDropDown;
			case FilterEditType.CheckBox:
				return EditType.CheckBox;
			case FilterEditType.Combo:
				return EditType.Combo;
			case FilterEditType.DropDownList:
				return EditType.DropDownList;
			case FilterEditType.TextBox:
				return EditType.TextBox;
			case FilterEditType.NoEdit:
				return EditType.NoEdit;
			case FilterEditType.Custom:
				return EditType.Custom;
			default:
				if (gridColumn.EditType == EditType.NoEdit)
				{
					return EditType.NoEdit;
				}
				return gridColumn.EditType;
			}
		}

		internal void ProcessDataGrid(MxDataGrid grid, GridColumn gridColumn, GridFormatStyle style, object value, FilterMode filterMode, bool capture, Control customControl)
		{
			_isFilterMode = true;
			EditType editType = GetFilterEditType(gridColumn);
			switch (editType)
			{
			case EditType.NoEdit:
				return;
			case EditType.TextBox:
				_editorState = new ValueListCellEditor(this);
				break;
			case EditType.Combo:
			case EditType.DropDownList:
				if (gridColumn.MultipleValues)
				{
					_editorState = new MultiValueCellEditor(this);
					break;
				}
				_editorState = new ValueListCellEditor(this);
				if (gridColumn.CompareTarget == ColumnCompareTarget.Value && gridColumn.DataTypeCode != TypeCode.String)
				{
					if (!gridColumn.HasValueList && gridColumn.DropDown == null)
					{
						value = gridColumn.GetDisplayText(value, true);
					}
				}
				else
				{
					if (gridColumn.CompareTarget != ColumnCompareTarget.Text || editType != EditType.DropDownList || !gridColumn.HasValueList || !(value is string))
					{
						break;
					}
					try
					{
						GridValueListItem gridValueListItem = gridColumn.ValueList[(string)value];
						if (gridValueListItem != null)
						{
							value = gridValueListItem.Value;
						}
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
				break;
			case EditType.CheckBox:
				_editorState = new CheckBoxCellEditor(this);
				break;
			case EditType.CalendarDropDown:
			case EditType.DatePicker:
				_editorState = new CalendarCellEditor(this);
				break;
			case EditType.IntegerUpDown:
			case EditType.IntegerUpDownCombo:
				_editorState = new IntegerUpDownCellEditor(this);
				break;
			case EditType.ValueListUpDown:
			case EditType.ValueListUpDownCombo:
				_editorState = new ValueListUpDownCellEditor(this);
				break;
			case EditType.MultiColumnCombo:
			case EditType.MultiColumnDropDown:
				if (gridColumn.MultipleValues)
				{
					_editorState = new MultiValueCellEditor(this);
				}
				else
				{
					_editorState = new MultiColumnComboCellEditor(this);
				}
				break;
			case EditType.Custom:
				_editorState = new CustomCellEditor(this, customControl);
				break;
			}
			if (_editorState != null)
			{
				SetOriginalValue(value);
				_gridColumn = gridColumn;
				_editorState.EditType = editType;
				_editorState.SetControlStyle(GetEditorStyle());
				_editorState.StartEdit(grid, gridColumn, grid.FilterRow.Cells[gridColumn], style, value, null, capture, customControl);
				if (GetActiveEditControl() != null)
				{
					GetActiveEditControl().Parent = this;
				}
				AttachEditControlEvents();
				SetModified(false);
				ApplyEditBounds();
			}
			if (GetActiveEditControl() != null)
			{
				GetActiveEditControl().Select();
			}
		}

		internal bool IsModified()
		{
			if (_editorState != null)
			{
				return _editorState.GetModifiedFlag();
			}
			return false;
		}

		internal void SetModified(bool condition)
		{
			if (_editorState != null)
			{
				_editorState.SetModifiedFlag(condition);
			}
		}

		internal EditType GetCurrentEditType()
		{
			if (_editorState != null)
			{
				return _editorState.EditType;
			}
			return EditType.NoEdit;
		}

		internal object GetOriginalValue()
		{
			return _originalValue;
		}

		internal void SetOriginalValue(object value)
		{
			_originalValue = value;
		}

		internal MaskEdit GetEditorMaskEdit()
		{
			if (_editorState != null)
			{
				return _editorState.GetMaskEdit();
			}
			return null;
		}

		internal Control GetActiveEditControl()
		{
			if (_editorState != null)
			{
				return _editorState.EditControl;
			}
			return null;
		}

		internal bool IsNotInList()
		{
			if (_editorState != null)
			{
				return _editorState.NotInList;
			}
			return false;
		}

		public void AddEditorEventHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _editorEventHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _editorEventHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		public void RemoveEditorEventHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _editorEventHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _editorEventHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		public void AddEditValueListHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _editorValueListHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _editorValueListHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		public void RemoveEditValueListHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _editorValueListHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _editorValueListHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		public void AddEditorCallback(GridEventCallback gridCallback)
		{
			GridEventCallback editorCallback = _callbackHandler;
			GridEventCallback comparand;
			do
			{
				comparand = editorCallback;
				GridEventCallback value = (GridEventCallback)Delegate.Combine(comparand, gridCallback);
				editorCallback = Interlocked.CompareExchange(ref _callbackHandler, value, comparand);
			}
			while ((object)editorCallback != comparand);
		}

		public void RemoveEditorCallback(GridEventCallback gridCallback)
		{
			GridEventCallback editorCallback = _callbackHandler;
			GridEventCallback source;
			do
			{
				source = editorCallback;
				GridEventCallback value = (GridEventCallback)Delegate.Remove(source, gridCallback);
				editorCallback = Interlocked.CompareExchange(ref _callbackHandler, value, source);
			}
			while ((object)editorCallback != source);
		}

		private void ApplyEditBounds()
		{
			_editBounds = _requestedEditBounds;
			if (GetActiveEditControl() != null)
			{
				if (_editBounds.Width > 0)
				{
					GetActiveEditControl().Visible = true;
					GetActiveEditControl().Bounds = _editBounds;
				}
				else
				{
					GetActiveEditControl().Visible = false;
				}
			}
			Invalidate();
		}

		internal void ClipEditBounds(Rectangle gridColumnContainerControlRectangle, Rectangle rectangle)
		{
			if (rectangle.Left > gridColumnContainerControlRectangle.Left || rectangle.Right < gridColumnContainerControlRectangle.Right || rectangle.Bottom < gridColumnContainerControlRectangle.Bottom || rectangle.Top > gridColumnContainerControlRectangle.Top)
			{
				_requestedEditBounds = gridColumnContainerControlRectangle;
				if (rectangle.Left > gridColumnContainerControlRectangle.Left)
				{
					_requestedEditBounds.X -= rectangle.Left;
				}
				else
				{
					_requestedEditBounds.X = 0;
				}
				if (rectangle.Top > gridColumnContainerControlRectangle.Top)
				{
					_requestedEditBounds.Y -= rectangle.Top;
				}
				else
				{
					_requestedEditBounds.Y = 0;
				}
				gridColumnContainerControlRectangle = Rectangle.Intersect(rectangle, gridColumnContainerControlRectangle);
				base.Bounds = gridColumnContainerControlRectangle;
				ApplyEditBounds();
			}
			else
			{
				_requestedEditBounds = gridColumnContainerControlRectangle;
				_requestedEditBounds.Location = new Point(0, 0);
				base.Bounds = gridColumnContainerControlRectangle;
				ApplyEditBounds();
			}
		}

		internal TextBox ComputeTextBox()
		{
			if (_editorState != null)
			{
				return _editorState.EditTextBox;
			}
			return null;
		}

		internal bool IsDroppedDown()
		{
			if (_editorState != null)
			{
				return _editorState.DroppedDown;
			}
			return false;
		}

		internal void SetDroppedDown(bool droppedDown)
		{
			if (_editorState != null)
			{
				_editorState.DroppedDown = droppedDown;
			}
		}

		internal void ReevaluateEditorValue()
		{
			if (_editorState != null)
			{
				_editorState.ReevaluateValue();
			}
		}

		internal bool EditorHandlesMouseWheel()
		{
			if (_editorState != null)
			{
				return _editorState.HandlesMouseWheel;
			}
			return false;
		}

		internal bool ForwardMouseWheel(int delta)
		{
			if (_editorState != null)
			{
				return _editorState.DoMouseWheel(delta);
			}
			return false;
		}

		internal void EndEditorCustomEdit()
		{
			if (_editorState != null)
			{
				_editorState.EndCustomEdit();
			}
		}

		private void AttachEditControlEvents()
		{
			Control control = GetActiveEditControl();
			if (control != null)
			{
				control.CausesValidation = false;
				control.GotFocus += OnControlGotFocus;
				control.LostFocus += OnControlLostFocus;
				control.MouseWheel += OnControlMouseWheel;
				control.KeyPress += OnControlKeyPress;
				control.KeyDown += OnControlKeyDown;
				control.KeyUp += OnControlKeyUp;
				control.MouseClick += OnControlMouseClick;
				control.MouseDoubleClick += OnControlMouseDoubleClick;
				control.MouseDown += OnControlMouseDown;
				control.MouseMove += OnControlMouseMove;
				control.MouseUp += OnControlMouseUp;
				control.Click += OnControlClick;
				control.DoubleClick += OnControlDoubleClick;
				_editorState.AddDropDownHideHandler(HandleEditControlEvent);
				_editorState.AddChangeHandler(RaiseEditorEvent);
				_editorState.AddDroppingListHandler(RefreshEditValueList);
			}
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);
			NativeMethods.SetProp(base.Handle, "PROP_ISGRIDEXCELLEDIT", new IntPtr(1));
		}

		protected override void OnHandleDestroyed(EventArgs eventArgs)
		{
			NativeMethods.RemoveProp(base.Handle, "PROP_ISGRIDEXCELLEDIT");
			base.OnHandleDestroyed(eventArgs);
		}

		private void DetachEditControlEvents()
		{
			Control control = GetActiveEditControl();
			if (control != null)
			{
				control.GotFocus -= OnControlGotFocus;
				control.LostFocus -= OnControlLostFocus;
				control.MouseWheel -= OnControlMouseWheel;
				control.KeyPress -= OnControlKeyPress;
				control.KeyDown -= OnControlKeyDown;
				control.KeyUp -= OnControlKeyUp;
				control.MouseClick -= OnControlMouseClick;
				control.MouseDoubleClick -= OnControlMouseDoubleClick;
				control.MouseDown -= OnControlMouseDown;
				control.MouseMove -= OnControlMouseMove;
				control.MouseUp -= OnControlMouseUp;
				control.Click -= OnControlClick;
				control.DoubleClick -= OnControlDoubleClick;
				_editorState.RemoveDropDownHideHandler(HandleEditControlEvent);
				_editorState.RemoveChangeHandler(RaiseEditorEvent);
				_editorState.RemoveDroppingListHandler(RefreshEditValueList);
			}
		}

		private void RaiseEditorEvent(object sender, EventArgs eventArgs)
		{
			if (_editorEventHandler != null)
			{
				_editorEventHandler(this, eventArgs);
			}
		}

		protected override void OnLeave(EventArgs eventArgs)
		{
			if (GetActiveEditControl() == null)
			{
				base.OnLostFocus(eventArgs);
			}
			base.OnLeave(eventArgs);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			IMordexContainer mordexContainer = GridHelper.ComputeIMordexContainer(this);
			if (mordexContainer != null && mordexContainer.PreProcessDialogKey(keyData))
			{
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		private void OnControlKeyDown(object sender, KeyEventArgs eventArgs)
		{
			OnKeyDown(eventArgs);
		}

		private void OnControlKeyUp(object sender, KeyEventArgs eventArgs)
		{
			OnKeyUp(eventArgs);
		}

		private void OnControlKeyPress(object sender, KeyPressEventArgs eventArgs)
		{
			base.OnKeyPress(eventArgs);
		}

		private void OnControlMouseDown(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseDown(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnControlMouseClick(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseClick(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnControlMouseDoubleClick(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseDoubleClick(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseUp(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnControlMouseMove(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseMove(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnControlClick(object sender, EventArgs eventArgs)
		{
			base.OnClick(eventArgs);
		}

		private void OnControlDoubleClick(object sender, EventArgs eventArgs)
		{
			base.OnDoubleClick(eventArgs);
		}

		private void OnControlMouseWheel(object sender, MouseEventArgs eventArgs)
		{
			OnMouseWheel(eventArgs);
		}

		private void OnControlGotFocus(object sender, EventArgs eventArgs)
		{
			base.OnGotFocus(eventArgs);
		}

		private void OnControlLostFocus(object sender, EventArgs eventArgs)
		{
			base.OnLostFocus(eventArgs);
		}

		private void HandleEditControlEvent(object value, GridEventArgs gridEventArgs)
		{
			if (_callbackHandler != null)
			{
				_callbackHandler(this, gridEventArgs);
			}
		}

		private void RefreshEditValueList(object sender, EventArgs eventArgs)
		{
			if (_editorValueListHandler != null)
			{
				_editorValueListHandler(this, eventArgs);
			}
			if (_isFilterMode)
			{
				if (_gridColumn.FilterEditType == FilterEditType.Combo || _gridColumn.FilterEditType == FilterEditType.DropDownList)
				{
					if (_gridColumn.EditValueList != null)
					{
						_editorState.EditValueList = _gridColumn.EditValueList;
					}
					else
					{
						_editorState.EditValueList = _gridColumn.GetDistinctValueList();
					}
				}
			}
			else if (_editorState is ValueListCellEditor)
			{
				if (_gridColumn.EditValueList != null)
				{
					_editorState.EditValueList = _gridColumn.EditValueList;
				}
				else
				{
					_editorState.EditValueList = _editorState.GetCell().ValueList;
				}
			}
			else if (_editorState is MultiColumnComboCellEditor)
			{
				_editorState.EditDropDown = _gridColumn.DropDown;
			}
		}

		internal static Color ResolveEditBackColor(MxDataGrid dataGrid, GridFormatStyle gridFormatStyle)
		{
			if (gridFormatStyle.GetBlendedBackColor() == Color.Empty)
			{
				return dataGrid.BackColor;
			}
			return gridFormatStyle.GetBlendedBackColor();
		}

		internal static Color ResolveEditForeColor(MxDataGrid dataGrid, GridFormatStyle gridFormatStyle)
		{
			if (gridFormatStyle.ForeColor == Color.Empty)
			{
				return dataGrid.ForeColor;
			}
			return gridFormatStyle.ForeColor;
		}

		internal static Font ResolveEditFont(MxDataGrid dataGrid, GridFormatStyle gridFormatStyle)
		{
			return (Font)gridFormatStyle.ResolveFont(dataGrid.Font).Clone();
		}

		internal void DestroyEditorState()
		{
			if (_editorState != null)
			{
				DetachEditControlEvents();
				_editorState.Destroy();
				_editorState = null;
			}
		}

		private bool PreProcessContainerKey(Keys keyData)
		{
			return GridHelper.ComputeIMordexContainer(this)?.PreProcessDialogKey(keyData) ?? false;
		}

		bool IMordexContainer.PreProcessDialogKey(Keys keyData)
		{
			return PreProcessContainerKey(keyData);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_containerAccessibleObject == null)
			{
				_containerAccessibleObject = new ColumnContainerAccessibleObject(this);
			}
			return _containerAccessibleObject;
		}
	}
}
