using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal abstract class CellEditor
	{
		private GridColumnContainerControl _editContainer;

		private Control _editControl;

		private GridColumn _column;

		private GridCell _cell;

		private ControlStyle _editorStyle;

		private bool _isModified;

		private EventHandler _changeHandler;

		private EventHandler _gridColumnState2EventHandler;

		private GridEventCallback _dropDownHideHandler;

		public virtual Control EditControl
		{
			get
			{
				if (_editControl == null)
				{
					_editControl = CreateEditControl();
				}
				return _editControl;
			}
		}

		public virtual Control DropControl => null;

		public virtual object Value
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual EditType EditType
		{
			get
			{
				return EditType.NoEdit;
			}
			set
			{
			}
		}

		public virtual bool NotInList => false;

		public virtual TextBox EditTextBox => null;

		public virtual bool DroppedDown
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public virtual bool HandlesMouseWheel => false;

		public virtual GridValueListItemCollection EditValueList
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual GridDropDown EditDropDown
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		internal CellEditor(GridColumnContainerControl editContainer)
		{
			_editContainer = editContainer;
		}

		internal GridColumnContainerControl GetEditContainer()
		{
			return _editContainer;
		}

		public GridColumn GetColumn()
		{
			return _column;
		}

		internal GridCell GetCell()
		{
			return _cell;
		}

		public virtual void OnDropDownButtonMouseDown()
		{
		}

		public virtual void OnUpDownButtonClick(int button)
		{
		}

		public virtual void OnButtonMouseUp()
		{
		}

		public virtual void ResumeUpDownRepeater()
		{
		}

		public virtual void SuspendUpDownRepeater()
		{
		}

		public virtual void ShowDropList()
		{
		}

		public virtual void InitEdit(char editChar)
		{
		}

		public ControlStyle GetControlStyle()
		{
			return _editorStyle;
		}

		public void SetControlStyle(ControlStyle controlStyle)
		{
			if (_editorStyle != controlStyle)
			{
				_editorStyle = controlStyle;
				OnControlStyleChanged();
			}
		}

		protected virtual void OnControlStyleChanged()
		{
		}

		public virtual void StartEdit(MxDataGrid grid, GridColumn column, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control control)
		{
			_cell = cell;
			_column = column;
		}

		public virtual void SetComboVirtualWidth(int virtualWidth)
		{
		}

		public virtual void ToggleCheckBoxValue()
		{
		}

		public virtual void SetTextValue(string value)
		{
		}

		public bool GetModifiedFlag()
		{
			return _isModified;
		}

		public void SetModifiedFlag(bool condition)
		{
			if (_isModified != condition)
			{
				_isModified = condition;
			}
		}

		public void AddChangeHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _changeHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _changeHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		public void RemoveChangeHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _changeHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _changeHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected virtual void OnChange(EventArgs eventArgs)
		{
			_isModified = true;
			if (_changeHandler != null)
			{
				_changeHandler(this, eventArgs);
			}
		}

		public void AddDroppingListHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _gridColumnState2EventHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _gridColumnState2EventHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		public void RemoveDroppingListHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _gridColumnState2EventHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _gridColumnState2EventHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected virtual void OnDroppingList(EventArgs eventArgs)
		{
			if (_gridColumnState2EventHandler != null)
			{
				_gridColumnState2EventHandler(this, eventArgs);
			}
		}

		public void AddDropDownHideHandler(GridEventCallback gridCallback)
		{
			GridEventCallback dropDownHideHandler = _dropDownHideHandler;
			GridEventCallback comparand;
			do
			{
				comparand = dropDownHideHandler;
				GridEventCallback value = (GridEventCallback)Delegate.Combine(comparand, gridCallback);
				dropDownHideHandler = Interlocked.CompareExchange(ref _dropDownHideHandler, value, comparand);
			}
			while ((object)dropDownHideHandler != comparand);
		}

		public void RemoveDropDownHideHandler(GridEventCallback gridCallback)
		{
			GridEventCallback dropDownHideHandler = _dropDownHideHandler;
			GridEventCallback source;
			do
			{
				source = dropDownHideHandler;
				GridEventCallback value = (GridEventCallback)Delegate.Remove(source, gridCallback);
				dropDownHideHandler = Interlocked.CompareExchange(ref _dropDownHideHandler, value, source);
			}
			while ((object)dropDownHideHandler != source);
		}

		protected virtual void OnDropDownHide(GridEventArgs gridEventArgs)
		{
			if (_dropDownHideHandler != null)
			{
				_dropDownHideHandler(this, gridEventArgs);
			}
		}

		protected abstract Control CreateEditControl();

		public virtual MaskEdit GetMaskEdit()
		{
			return null;
		}

		public virtual void ReevaluateValue()
		{
		}

		public virtual bool DoMouseWheel(int delta)
		{
			return false;
		}

		public virtual void EndCustomEdit()
		{
		}

		public virtual void Destroy()
		{
			if (_editControl != null)
			{
				_editControl.Dispose();
				_editControl = null;
			}
		}
	}
}
