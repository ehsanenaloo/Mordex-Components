using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class GridValueListItemCollectionState : ControlStyleContainerControl
	{
		internal class ValueListAccessibleObject : ControlAccessibleObject
		{
			public override string Name
			{
				get
				{
					return "CellEditor";
				}
				set
				{
					base.Name = value;
				}
			}

			public override AccessibleRole Role
			{
				get
				{
					if (GetOwnerControl().GetEditStyle() == EditStyle.Combo)
					{
						return AccessibleRole.ComboBox;
					}
					if (GetOwnerControl().GetEditStyle() == EditStyle.DropDownList)
					{
						return AccessibleRole.DropList;
					}
					return AccessibleRole.Text;
				}
			}

			public override AccessibleObject Parent
			{
				get
				{
					if (GetOwnerControl().Parent != null)
					{
						return GetOwnerControl().Parent.AccessibilityObject;
					}
					return null;
				}
			}

			internal ValueListAccessibleObject(GridValueListItemCollectionState owner)
				: base(owner)
			{
			}

			private GridValueListItemCollectionState GetOwnerControl()
			{
				return (GridValueListItemCollectionState)base.Owner;
			}

			public override int GetChildCount()
			{
				int childCount = 0;
				if (GetOwnerControl()._textBox != null)
				{
					childCount++;
				}
				if ((GetOwnerControl().GetEditStyle() == EditStyle.Combo || GetOwnerControl().GetEditStyle() == EditStyle.DropDownList) && GetOwnerControl()._gridValueListItemCollectionStateDropControl != null)
				{
					childCount++;
				}
				return childCount;
			}

			public override AccessibleObject GetChild(int index)
			{
				if (GetOwnerControl()._textBox != null)
				{
					if (index == 0)
					{
						return GetOwnerControl()._textBox.AccessibilityObject;
					}
					index--;
				}
				if (GetOwnerControl().GetEditStyle() == EditStyle.DropDownList || GetOwnerControl().GetEditStyle() == EditStyle.Combo)
				{
					if (index == 0 && GetOwnerControl()._gridValueListItemCollectionStateDropControl != null)
					{
						return GetOwnerControl()._gridValueListItemCollectionStateDropControl.AccessibilityObject;
					}
					index--;
				}
				throw new ArgumentOutOfRangeException();
			}
		}

		private Container _container = new Container();

		private GridValueListItemCollectionScrollableBase _gridValueListItemCollectionStateDropControl;

		private bool _useCompatibleTextRendering;

		private bool _isSyncingText;

		private GridValueListItemCollection _gridValueListItemCollection;

		private GridValueListItem _gridValueListItem;

		private EventHandler _selectedItemChangedHandler;

		private bool _isAutoCompleteEnabled = true;

		private object _gridValueListItemCollectionStateValue;

		private bool _isLoadingValue;

		public GridValueListItemCollectionState()
		{
			UpdateContainer();
			_gridValueListItemCollectionStateDropControl = new GridValueListItemCollectionScrollableBase();
			_gridValueListItemCollectionStateDropControl.AssignControlStyle(GetControlStyle());
		}

		internal void SetUseCompatibleTextRendering(bool gridValueListItemCollectionStateBoolean)
		{
			if (_useCompatibleTextRendering != gridValueListItemCollectionStateBoolean)
			{
				_useCompatibleTextRendering = gridValueListItemCollectionStateBoolean;
				if (_gridValueListItemCollectionStateDropControl != null)
				{
					_gridValueListItemCollectionStateDropControl.UseCompatibleTextRendering = _useCompatibleTextRendering;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_container != null)
				{
					_container.Dispose();
				}
				if (_gridValueListItemCollectionStateDropControl != null)
				{
					_gridValueListItemCollectionStateDropControl.Dispose();
					_gridValueListItemCollectionStateDropControl = null;
				}
			}
			base.Dispose(disposing);
		}

		private void UpdateContainer()
		{
			_container = new Container();
			BackColor = SystemColors.Control;
		}

		public void SetDropListFont(Font font)
		{
			if (_gridValueListItemCollectionStateDropControl != null)
			{
				_gridValueListItemCollectionStateDropControl.Font = font;
			}
		}

		protected override void DropListControl(bool setCapture)
		{
			base.DropListControl(setCapture);
			Rectangle rectangle = GetVirtualClientRectangle();
			rectangle.Location = PointToScreen(rectangle.Location);
			_gridValueListItemCollectionStateDropControl.SetSelectedFormatStyle(_gridFormatStyle);
			_gridValueListItemCollectionStateDropControl.SetItemImageList(GetImageList());
			_gridValueListItemCollectionStateDropControl.SetSelectedItem(GetSelectedItem());
			_gridValueListItemCollectionStateDropControl.RightToLeft = RightToLeft;
			_gridValueListItemCollectionStateDropControl.ForeColor = GetControlStyle().WindowTextColor;
			_gridValueListItemCollectionStateDropControl.BackColor = GetControlStyle().WindowColor;
			_gridValueListItemCollectionStateDropControl.SetShowImages(GetIsImageSet());
			_gridValueListItemCollectionStateDropControl.ShowDropDownAt(rectangle, this);
			Cursor = Cursors.Arrow;
			GridHelper.SetCapture(_gridValueListItemCollectionStateDropControl, setCapture);
		}

		protected internal override Control GetDropControl()
		{
			if (GetEditStyle() == EditStyle.Combo || GetEditStyle() == EditStyle.DropDownList)
			{
				return _gridValueListItemCollectionStateDropControl;
			}
			return null;
		}

		protected override void OnKeyPress(KeyPressEventArgs eventArgs)
		{
			base.OnKeyPress(eventArgs);
			if (!eventArgs.Handled)
			{
				InternalProcessChar(eventArgs.KeyChar);
			}
		}

		protected override void OnKeyDown(KeyEventArgs eventArgs)
		{
			base.OnKeyDown(eventArgs);
			if (!eventArgs.Handled && eventArgs.KeyCode == Keys.Delete && GetEditStyle() == EditStyle.DropDownList)
			{
				SetCurrentValue(null);
			}
		}

		protected override void OnDropHide(bool cancel)
		{
			_gridValueListItemCollectionStateDropControl.HideDropDown(cancel);
			if (_textBox != null)
			{
				SelectRange(Text.Length, 0);
				GridHelper.SelectAndFocus(_textBox);
			}
			else
			{
				GridHelper.SelectAndFocus(this);
			}
			if (!cancel)
			{
				SelectItem(_gridValueListItemCollectionStateDropControl.GetSelectedItem());
			}
			Cursor = Cursors.Default;
			base.OnDropHide(cancel);
		}

		protected override void OnMouseUp(MouseEventArgs me)
		{
			base.OnMouseUp(me);
			if (GetIsDroppedDown())
			{
				GridHelper.SetCapture(_gridValueListItemCollectionStateDropControl, true);
			}
		}

		protected override void OnMouseMove(MouseEventArgs me)
		{
			base.OnMouseMove(me);
			if (GetIsDroppedDown())
			{
				Point mousePosition = Control.MousePosition;
				if (_gridValueListItemCollectionStateDropControl.Bounds.Contains(mousePosition.X, mousePosition.Y))
				{
					GridHelper.SetCapture(_gridValueListItemCollectionStateDropControl, true);
				}
			}
		}

		internal void SyncSelectedItemFromText()
		{
			if (_gridValueListItemCollection != null)
			{
				GridValueListItem gridValueListItem = ((Text.Length != 0) ? _gridValueListItemCollection[Text] : null);
				if (gridValueListItem == null)
				{
					_gridValueListItem = null;
					_gridValueListItemCollectionStateValue = Text;
					Invalidate();
				}
				else if (gridValueListItem != _gridValueListItem)
				{
					_gridValueListItem = gridValueListItem;
					_gridValueListItemCollectionStateValue = gridValueListItem.Value;
					Invalidate();
				}
			}
			else
			{
				_gridValueListItemCollectionStateValue = Text;
				Invalidate();
			}
			if (!_isLoadingValue)
			{
				OnChange(EventArgs.Empty);
			}
		}

		protected override void OnTextChanged(EventArgs eventArgs)
		{
			base.OnTextChanged(eventArgs);
			if (!_isSyncingText)
			{
				SyncSelectedItemFromText();
			}
		}

		protected internal override void OnChange(EventArgs eventArgs)
		{
			if (!_isLoadingValue)
			{
				base.OnChange(eventArgs);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (GetIsDroppedDown() && _gridValueListItemCollectionStateDropControl != null && _gridValueListItemCollectionStateDropControl.DropedProcessDialogKey(keyData))
			{
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		public GridValueListItemCollection GetValueListItems()
		{
			return _gridValueListItemCollection;
		}

		public void AssignValueListItems(GridValueListItemCollection gridValueListItemCollection)
		{
			if (gridValueListItemCollection != _gridValueListItemCollection)
			{
				_gridValueListItemCollection = gridValueListItemCollection;
				_gridValueListItem = null;
				_gridValueListItemCollectionStateDropControl.AssignItems(gridValueListItemCollection);
			}
		}

		public GridValueListItem GetSelectedItem()
		{
			return _gridValueListItem;
		}

		public void SelectItem(GridValueListItem gridValueListItem)
		{
			_gridValueListItem = gridValueListItem;
			if (gridValueListItem == null)
			{
				_gridValueListItemCollectionStateValue = null;
				Text = "";
				SetImage(null);
			}
			else
			{
				if (_gridValueListItemCollection.ReplaceValues)
				{
					_gridValueListItemCollectionStateValue = gridValueListItem.Value;
				}
				else
				{
					_gridValueListItemCollectionStateValue = gridValueListItem.Text;
				}
				bool isLoadingValue = _isLoadingValue;
				_isLoadingValue = true;
				_isSyncingText = true;
				Text = gridValueListItem.Text;
				_isSyncingText = false;
				_isLoadingValue = isLoadingValue;
				if (GetIsImageSet())
				{
					SetImage(gridValueListItem.ResolveImage(GetImageList()));
				}
			}
			_gridValueListItem = gridValueListItem;
			if (_gridValueListItemCollectionStateDropControl != null)
			{
				_gridValueListItemCollectionStateDropControl.SetSelectedItem(_gridValueListItem);
			}
			if (!_isLoadingValue)
			{
			}
			OnSelectedItemChanged(EventArgs.Empty);
			Invalidate();
		}

		public void AddSelectedItemChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _selectedItemChangedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _selectedItemChangedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		public void RemoveSelectedItemChangedHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _selectedItemChangedHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _selectedItemChangedHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected virtual void OnSelectedItemChanged(EventArgs eventArgs)
		{
			if (_selectedItemChangedHandler != null)
			{
				_selectedItemChangedHandler(this, eventArgs);
			}
		}

		public void SetAutoCompleteEnabled(bool isAutoCompleteEnabled)
		{
			_isAutoCompleteEnabled = isAutoCompleteEnabled;
		}

		protected override void OnKeyPressed(EventArgs eventArgs)
		{
			base.OnKeyPressed(eventArgs);
			AutoCompleteText();
		}

		private void AutoCompleteText()
		{
			if (_gridValueListItemCollection != null && _isAutoCompleteEnabled && Text.Length > 0)
			{
				string text = Text;
				GridValueListItem gridValueListItem = _gridValueListItemCollection.FindItemStartingWith(text);
				if (gridValueListItem != null)
				{
					string otherText = gridValueListItem.Text;
					otherText = text + gridValueListItem.Text.Substring(text.Length);
					SelectItem(gridValueListItem);
					Text = otherText;
					SelectRange(gridValueListItem.Text.Length, text.Length);
				}
			}
		}

		protected override void InternalProcessChar(char keyChar)
		{
			if (_gridValueListItemCollection != null && GetEditStyle() == EditStyle.DropDownList && char.IsLetterOrDigit(keyChar))
			{
				string text = keyChar.ToString(Application.CurrentCulture);
				int value = -1;
				if (_gridValueListItem != null)
				{
					value = _gridValueListItemCollection.IndexOf(_gridValueListItem);
				}
				GridValueListItem gridValueListItem = _gridValueListItemCollection.FindItemByPrefix(value, text);
				if (gridValueListItem != null)
				{
					SelectItem(gridValueListItem);
				}
			}
		}

		public object GetCurrentValue()
		{
			if (IsNumericFormat() && _textBox != null)
			{
				object value = ((NumericEdit)_textBox).Value;
				if (value == null)
				{
					return "";
				}
				return value;
			}
			return _gridValueListItemCollectionStateValue;
		}

		public void SetCurrentValue(object gridValueListItemCollectionStateValue)
		{
			if (gridValueListItemCollectionStateValue == GetCurrentValue())
			{
				return;
			}
			_gridValueListItemCollectionStateValue = gridValueListItemCollectionStateValue;
			if (IsNumericFormat() && _textBox != null)
			{
				if (gridValueListItemCollectionStateValue == DBNull.Value || gridValueListItemCollectionStateValue == null)
				{
					gridValueListItemCollectionStateValue = "";
				}
				string text = "";
				text = GridHelper.ToSafeString(gridValueListItemCollectionStateValue);
				((NumericEdit)_textBox).Text = text;
				Text = _textBox.Text;
			}
			else if (_gridValueListItemCollectionStateValue == null)
			{
				Text = "";
				SelectItem(null);
			}
			else if (_gridValueListItemCollection != null)
			{
				if (_gridValueListItemCollection.ReplaceValues)
				{
					SelectItem(_gridValueListItemCollection[_gridValueListItemCollectionStateValue]);
				}
				else
				{
					try
					{
						SelectItem(_gridValueListItemCollection[(string)gridValueListItemCollectionStateValue]);
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
				if (GetSelectedItem() == null)
				{
					Text = GridHelper.ToSafeString(gridValueListItemCollectionStateValue);
				}
			}
			else
			{
				Text = GridHelper.ToSafeString(gridValueListItemCollectionStateValue);
			}
			if (!_isLoadingValue)
			{
			}
		}

		internal void LoadEditValue(object gridValueListItemCollectionStateValue)
		{
			_isLoadingValue = true;
			_gridValueListItemCollectionStateValue = gridValueListItemCollectionStateValue;
			if (_gridValueListItemCollection != null && _gridValueListItemCollection.ReplaceValues)
			{
				if (_gridValueListItemCollectionStateValue == null)
				{
					SelectItem(null);
				}
				else
				{
					SelectItem(_gridValueListItemCollection[_gridValueListItemCollectionStateValue]);
				}
				if (_gridValueListItem == null && gridValueListItemCollectionStateValue != null)
				{
					Text = GridHelper.ToSafeString(gridValueListItemCollectionStateValue);
				}
			}
			else if (_textBox is NumericEdit)
			{
				((NumericEdit)_textBox).Value = gridValueListItemCollectionStateValue;
			}
			else if (gridValueListItemCollectionStateValue == null)
			{
				base.Text = "";
			}
			else
			{
				base.Text = GridHelper.ToSafeString(_gridValueListItemCollectionStateValue);
			}
			_isLoadingValue = false;
		}

		protected override void OnControlStyleChanged()
		{
			if (_gridValueListItemCollectionStateDropControl != null)
			{
				_gridValueListItemCollectionStateDropControl.AssignControlStyle(GetControlStyle());
			}
		}

		public bool IsTextNotInList()
		{
			if (_gridValueListItemCollection != null && _gridValueListItem == null && Text.Length > 0)
			{
				return true;
			}
			return false;
		}

		protected override bool OnDoMouseWheel(int delta)
		{
			if (_gridValueListItemCollectionStateDropControl != null && GetIsDroppedDown())
			{
				_gridValueListItemCollectionStateDropControl.ScrollByWheelDelta(delta);
				return true;
			}
			return false;
		}

		internal void LoadEditText(string text)
		{
			SetCurrentValue(text);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new ValueListAccessibleObject(this);
		}
	}
}
