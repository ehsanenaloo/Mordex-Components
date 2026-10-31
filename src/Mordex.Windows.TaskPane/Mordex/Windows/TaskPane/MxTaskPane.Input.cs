using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane
	{
		private TaskPaneItem GetPressedItem()
		{
			return _pressedItem;
		}

		private void UpdatePressedItem(TaskPaneItem taskPaneItem)
		{
			_pressedItem = taskPaneItem;
			if (taskPaneItem != null)
			{
				Invalidate(taskPaneItem.Bounds);
			}
		}

		private TaskPaneGroup GetPressedGroup()
		{
			return _pressedGroup;
		}

		private void UpdatePressedGroup(TaskPaneGroup taskPaneGroup)
		{
			_pressedGroup = taskPaneGroup;
			if (taskPaneGroup != null)
			{
				Rectangle invalidateRect = taskPaneGroup.GetHeaderBounds();
				invalidateRect.Inflate(1, 1);
				Invalidate(invalidateRect);
			}
		}

		public void EnsureVisible(TaskPaneGroup group)
		{
			if (!_isLayoutValid)
			{
				Update();
			}
			Rectangle targetBounds = group.GetItemsBounds();
			if (targetBounds.IsEmpty)
			{
				targetBounds = group.GetHeaderBounds();
			}
			if (group.GetHeaderBounds().Y < _contentBounds.Top && _verticalScrollPosition > 0)
			{
				int groupTop = group.GetHeaderBounds().Y;
				int scrollUpPosition = _verticalScrollPosition;
				while (groupTop < _contentBounds.Top && scrollUpPosition > 0)
				{
					groupTop += ScrollLineHeight;
					scrollUpPosition--;
				}
				_verticalScrollPosition = scrollUpPosition;
				InvalidateLayout();
			}
			else if (targetBounds.Bottom > _contentBounds.Bottom)
			{
				int groupBottom = targetBounds.Bottom;
				int scrollDownPosition = _verticalScrollPosition;
				while (groupBottom > _contentBounds.Bottom && scrollDownPosition < _verticalScrollbar.GetMaximum())
				{
					groupBottom -= ScrollLineHeight;
					scrollDownPosition++;
				}
				_verticalScrollPosition = scrollDownPosition;
				InvalidateLayout();
			}
		}

		public void EnsureVisible(TaskPaneItem item)
		{
			if (!_isLayoutValid)
			{
				Update();
			}
			if (item.Group != null && !item.Group.Expanded)
			{
				item.Group.Expanded = true;
				Update();
			}
			if (item.Bounds.Y < _contentBounds.Top && _verticalScrollPosition > 0)
			{
				int itemTop = item.Bounds.Y;
				int scrollUpPosition = _verticalScrollPosition;
				while (itemTop < _contentBounds.Top && scrollUpPosition > 0)
				{
					itemTop += ScrollLineHeight;
					scrollUpPosition--;
				}
				_verticalScrollPosition = scrollUpPosition;
				InvalidateLayout();
			}
			else if (item.Bounds.Bottom > _contentBounds.Bottom)
			{
				int itemBottom = item.Bounds.Bottom;
				int scrollDownPosition = _verticalScrollPosition;
				while (itemBottom > _contentBounds.Bottom && scrollDownPosition < _verticalScrollbar.GetMaximum())
				{
					itemBottom -= ScrollLineHeight;
					scrollDownPosition++;
				}
				_verticalScrollPosition = scrollDownPosition;
				InvalidateLayout();
			}
		}

		private void ScrollByWheelDelta(int wheelDelta)
		{
			if (!base.Created || base.ClientRectangle.Width <= 0 || base.ClientRectangle.Height <= 0)
			{
				return;
			}
			if (wheelDelta < 0)
			{
				if (_verticalScrollbar.Visible)
				{
					ControlStyleControl verticalScrollbar = _verticalScrollbar;
					verticalScrollbar.UpdateValue(verticalScrollbar.GetValue() + 1);
				}
			}
			else if (wheelDelta > 0 && _verticalScrollbar.Visible)
			{
				ControlStyleControl upScrollbar = _verticalScrollbar;
				upScrollbar.UpdateValue(upScrollbar.GetValue() - 1);
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			ScrollByWheelDelta(e.Delta);
		}

		private const int WmKeyDown = 0x100;

		private const int WmSysKeyDown = 0x104;

		private const int VirtualKeyF10 = 121;

		private const int AltShortcutFlag = 262144;

		private const int ShiftShortcutFlag = 65536;

		private const int ControlShortcutFlag = 131072;

		bool IMessageFilter.PreFilterMessage(ref Message msg)
		{
			if (msg.Msg != WmKeyDown && msg.Msg != WmSysKeyDown)
			{
				return false;
			}
			Form form = ComputeForm();
			if (form == null || !form.Enabled || !base.Enabled)
			{
				return false;
			}
			Form activeForm = Form.ActiveForm;
			if (form != activeForm && (activeForm == null || activeForm.ActiveMdiChild != form))
			{
				return false;
			}
			int virtualKey = unchecked((int)msg.WParam.ToInt64());
			Shortcut shortcut;
			if (msg.Msg == WmKeyDown || virtualKey == VirtualKeyF10)
			{
				shortcut = (Shortcut)GetShortcutKeyValue(virtualKey);
			}
			else
			{
				shortcut = (Shortcut)(AltShortcutFlag + virtualKey);
			}
			return Enum.IsDefined(shortcut) && InvokeItemByShortcut(shortcut);
		}

		private int GetShortcutKeyValue(int virtualKey)
		{
			int keyValue = virtualKey;
			Keys modifiers = Control.ModifierKeys;
			if ((modifiers & Keys.Alt) != Keys.Alt)
			{
				if ((modifiers & Keys.Shift) == Keys.Shift)
				{
					keyValue += ShiftShortcutFlag;
				}
				if ((modifiers & Keys.Control) == Keys.Control)
				{
					keyValue += ControlShortcutFlag;
				}
			}
			return keyValue;
		}

		private bool InvokeItemByShortcut(Shortcut shortcut)
		{
			foreach (TaskPaneGroup item in _groups)
			{
				if (!item.Enabled)
				{
					continue;
				}
				foreach (TaskPaneItem childItem in item.Items)
				{
					if (childItem.Enabled && childItem.ItemType == ItemType.LinkButton && childItem.Shortcut == shortcut)
					{
						childItem.InvokeOnClick();
						return true;
					}
				}
			}
			return false;
		}

		private void OnControlEnter(object sender, EventArgs eventArgs)
		{
			TaskPaneContainerControl taskPaneContainerControl = sender as TaskPaneContainerControl;
			if (taskPaneContainerControl.Item != null)
			{
				ActiveItem = taskPaneContainerControl.Item;
			}
			else if (taskPaneContainerControl.Group != null)
			{
				ActiveGroup = taskPaneContainerControl.Group;
			}
			_isKeyboardNavigating = true;
		}

		private void OnControlLeave(object sender, EventArgs eventArgs)
		{
			TaskPaneContainerControl taskPaneContainerControl = sender as TaskPaneContainerControl;
			if (taskPaneContainerControl.Item != null)
			{
				if (taskPaneContainerControl.Item == ActiveItem)
				{
					ActiveItem = null;
				}
			}
			else if (taskPaneContainerControl.Group != null && _activeGroup == taskPaneContainerControl._group)
			{
				ActiveGroup = null;
			}
		}

		private bool TryActivatePrevious(bool wrapAround)
		{
			_isKeyboardNavigating = true;
			int startIndex = -1;
			TaskPaneGroup taskPaneGroup = null;
			if (_activeItem != null)
			{
				if (ActiveItem.ContainerControl != null && ComputeForm() != null)
				{
					Control control = ComputeControl(ComputeForm().ActiveControl);
					if (ActiveItem.ContainerControl.Controls.Contains(control) && ActiveItem.ContainerControl.SelectNextControl(control, forward: false, tabStopOnly: true, nested: true, wrap: false))
					{
						return true;
					}
				}
				taskPaneGroup = _activeItem.Group;
				startIndex = _activeItem.Index - 1;
			}
			else
			{
				if (_activeGroup == null)
				{
					return false;
				}
				taskPaneGroup = _activeGroup;
				if (ActiveGroup.ContainerControl != null && ComputeForm() != null)
				{
					Control focusedControl = ComputeControl(ComputeForm().ActiveControl);
					if (ActiveGroup.ContainerControl.Controls.Contains(focusedControl) && ActiveGroup.ContainerControl.SelectNextControl(focusedControl, forward: false, tabStopOnly: true, nested: true, wrap: false))
					{
						return true;
					}
				}
			}
			if (taskPaneGroup != null)
			{
				if (taskPaneGroup.Expanded)
				{
					for (int itemIndex = startIndex; itemIndex >= 0; itemIndex--)
					{
						TaskPaneItem taskPaneItem = taskPaneGroup.Items[itemIndex];
						if ((taskPaneItem.ItemType == ItemType.LinkButton || taskPaneItem.Container) && taskPaneItem.Enabled && taskPaneItem.Visible)
						{
							ActiveItem = taskPaneItem;
							return true;
						}
					}
					if (_activeGroup == null)
					{
						ActiveGroup = taskPaneGroup;
						Focus();
						return true;
					}
				}
				for (int prevGroupIndex = taskPaneGroup._index - 1; prevGroupIndex >= 0; prevGroupIndex--)
				{
					TaskPaneGroup prevGroup = Groups[prevGroupIndex];
					if (prevGroup.Visible && prevGroup.Enabled)
					{
						if (prevGroup.Expanded)
						{
							for (int prevItemIndex = prevGroup.Items.Count - 1; prevItemIndex >= 0; prevItemIndex--)
							{
								TaskPaneItem prevItem = prevGroup.Items[prevItemIndex];
								if ((prevItem.Container || prevItem.ItemType == ItemType.LinkButton) && prevItem.Enabled && prevItem.Visible)
								{
									ActiveItem = prevItem;
									return true;
								}
							}
						}
						ActiveGroup = prevGroup;
						Focus();
						return true;
					}
				}
				if (wrapAround)
				{
					for (int wrapGroupIndex = Groups.Count - 1; wrapGroupIndex >= taskPaneGroup.Index; wrapGroupIndex--)
					{
						TaskPaneGroup wrapGroup = Groups[wrapGroupIndex];
						if (wrapGroup.Visible && wrapGroup.Enabled)
						{
							if (wrapGroup.Expanded)
							{
								for (int wrapItemIndex = wrapGroup.Items.Count - 1; wrapItemIndex >= 0; wrapItemIndex--)
								{
									TaskPaneItem wrapItem = wrapGroup.Items[wrapItemIndex];
									if ((wrapItem.Container || wrapItem.ItemType == ItemType.LinkButton) && wrapItem.Enabled && wrapItem.Visible)
									{
										ActiveItem = wrapItem;
										return true;
									}
								}
							}
							ActiveGroup = wrapGroup;
							Focus();
							return true;
						}
					}
				}
			}
			return false;
		}

		private bool TryActivateNext(bool wrapAround)
		{
			int startIndex = 0;
			TaskPaneGroup taskPaneGroup = null;
			_isKeyboardNavigating = true;
			if (_activeItem != null)
			{
				if (ActiveItem.ContainerControl != null && ComputeForm() != null)
				{
					Control ctl = ComputeControl(ComputeForm().ActiveControl);
					if (ActiveItem.ContainerControl.SelectNextControl(ctl, forward: true, tabStopOnly: true, nested: true, wrap: false))
					{
						return true;
					}
				}
				taskPaneGroup = _activeItem.Group;
				startIndex = _activeItem.Index + 1;
			}
			else
			{
				if (_activeGroup == null)
				{
					return false;
				}
				taskPaneGroup = _activeGroup;
				if (ActiveGroup.ContainerControl != null && ComputeForm() != null)
				{
					Control focusedControl = ComputeControl(ComputeForm().ActiveControl);
					if (ActiveGroup.ContainerControl.SelectNextControl(focusedControl, forward: true, tabStopOnly: true, nested: true, wrap: false))
					{
						return true;
					}
				}
			}
			if (taskPaneGroup != null)
			{
				if (taskPaneGroup.Expanded)
				{
					for (int i = startIndex; i < taskPaneGroup.Items.Count; i++)
					{
						TaskPaneItem taskPaneItem = taskPaneGroup.Items[i];
						if ((taskPaneItem.Container || taskPaneItem.ItemType == ItemType.LinkButton) && taskPaneItem.Enabled && taskPaneItem.Visible)
						{
							ActiveItem = taskPaneItem;
							return true;
						}
					}
				}
				for (int j = taskPaneGroup._index + 1; j < Groups.Count; j++)
				{
					TaskPaneGroup nextGroup = Groups[j];
					if (nextGroup.Visible && nextGroup.Enabled)
					{
						ActiveGroup = nextGroup;
						Focus();
						return true;
					}
				}
				if (wrapAround)
				{
					for (int k = 0; k < taskPaneGroup.Index; k++)
					{
						TaskPaneGroup wrapGroup = Groups[k];
						if (wrapGroup.Visible && wrapGroup.Enabled)
						{
							ActiveGroup = wrapGroup;
							Focus();
							return true;
						}
					}
				}
			}
			return false;
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (ActiveGroup != null)
			{
				switch (keyData)
				{
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
					return false;
				}
			}
			return base.IsInputKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled)
			{
				return;
			}
			switch (e.KeyData)
			{
			case Keys.Prior:
				if (_verticalScrollbar != null && _verticalScrollbar.Visible)
				{
					ControlStyleControl verticalScrollbar = _verticalScrollbar;
					verticalScrollbar.UpdateValue(verticalScrollbar.GetValue() - _verticalScrollbar.GetLargeChange());
				}
				break;
			case Keys.Next:
				if (_verticalScrollbar != null && _verticalScrollbar.Visible)
				{
					ControlStyleControl pageDownScrollbar = _verticalScrollbar;
					pageDownScrollbar.UpdateValue(pageDownScrollbar.GetValue() + _verticalScrollbar.GetLargeChange());
				}
				break;
			case Keys.Return:
			case Keys.Space:
				if (_activeItem != null)
				{
					_activeItem.InvokeOnClick();
				}
				if (_activeGroup != null && _activeGroup.Expandable && _activeGroup.Enabled && _activeGroup.ShowGroupCaption)
				{
					_activeGroup.Expanded = !_activeGroup.Expanded;
				}
				break;
			}
		}

		private Control ComputeControl(Control startControl)
		{
			Control control = startControl;
			while (control is IContainerControl && ((IContainerControl)control).ActiveControl != null)
			{
				control = ((IContainerControl)control).ActiveControl;
			}
			return control;
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new TaskPaneAccessibleObject(this);
			}
			return _accessibleObject;
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (base.ContainsFocus)
			{
				switch (keyData)
				{
				case Keys.Return:
				case Keys.Space:
					if (_activeGroup != null && _activeItem == null && Focused && _activeGroup.Enabled && _activeGroup.Expandable)
					{
						GroupEventArgs e = new GroupEventArgs(_activeGroup);
						OnGroupClick(e);
						_activeGroup.Expanded = !_activeGroup.Expanded;
						return true;
					}
					break;
				case Keys.Tab:
					if (TryActivateNext(false))
					{
						return true;
					}
					break;
				case Keys.Tab | Keys.Shift:
					if (TryActivatePrevious(false))
					{
						return true;
					}
					break;
				case Keys.Down:
					if (TryActivateNext(true))
					{
						return true;
					}
					break;
				case Keys.Up:
					if (TryActivatePrevious(true))
					{
						return true;
					}
					break;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);
			if (e.Control.GetType() == typeof(TaskPaneContainerControl))
			{
				TaskPaneContainerControl taskPaneContainerControl = (TaskPaneContainerControl)e.Control;
				taskPaneContainerControl.Enter -= OnControlEnter;
				taskPaneContainerControl.Leave -= OnControlLeave;
				if (taskPaneContainerControl._group != null && taskPaneContainerControl._group.Container)
				{
					taskPaneContainerControl._group.Container = false;
					InvalidateLayout();
				}
				else if (taskPaneContainerControl._item != null && taskPaneContainerControl._item._container)
				{
					taskPaneContainerControl._item.Container = false;
					InvalidateLayout();
				}
			}
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			_isKeyboardNavigating = false;
			if (ActiveGroup != null || ActiveItem != null)
			{
				Invalidate();
			}
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			foreach (TaskPaneGroup item in _groups)
			{
				item.SetHeaderBounds(Rectangle.Empty);
			}
			InvalidateLayout();
			base.OnRightToLeftChanged(e);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (ActiveGroup != null || ActiveItem != null)
			{
				Invalidate();
			}
			else if (_shouldActivateFirstGroup && ShowFocusRectangle && Groups.Count > 0)
			{
				ActiveGroup = _groups[0];
				Invalidate();
			}
			_shouldActivateFirstGroup = true;
		}

		protected override void Select(bool directed, bool forward)
		{
			base.Select(directed, forward);
			if (ShowFocusRectangle && Groups.Count > 0)
			{
				ActiveGroup = Groups[0];
				_isKeyboardNavigating = true;
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			UpdateForm(FindForm());
		}

		protected override void OnParentVisibleChanged(EventArgs e)
		{
			base.OnParentVisibleChanged(e);
			if (ComputeForm() != null)
			{
				UpdateForm(FindForm());
			}
		}

		private Form ComputeForm()
		{
			if (_form == null)
			{
				return FindForm();
			}
			return _form;
		}

		private void UpdateForm(Form form)
		{
			if (_form == form)
			{
				return;
			}
			if (_form != null)
			{
				_form.Activated -= OnFormDeactivate;
				_form.Deactivate -= OnFormDeactivate;
				try
				{
					Application.RemoveMessageFilter(this);
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
				}
			}
			_form = form;
			if (_form != null)
			{
				_form.Activated += OnFormDeactivate;
				_form.Deactivate += OnFormDeactivate;
				Application.AddMessageFilter(this);
			}
		}

		private void OnFormDeactivate(object sender, EventArgs eventArgs)
		{
			if (ActiveGroup != null)
			{
				Invalidate();
			}
		}

		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			if (e.Control.GetType() == typeof(TaskPaneContainerControl))
			{
				TaskPaneContainerControl taskPaneContainerControl = (TaskPaneContainerControl)e.Control;
				taskPaneContainerControl._owner = this;
				taskPaneContainerControl.Enter += OnControlEnter;
				taskPaneContainerControl.Leave += OnControlLeave;
			}
		}

		private bool HasMnemonic(string text)
		{
			return GetMnemonicChar(text) != '\0';
		}

		/// <summary>Returns the character after the first single ampersand, or '\0' when the text has no mnemonic.</summary>
		private static char GetMnemonicChar(string text)
		{
			for (int i = 0; i < text.Length - 1; i++)
			{
				if (text[i] != '&')
				{
					continue;
				}
				if (text[i + 1] == '&')
				{
					i++;
					continue;
				}
				return text[i + 1];
			}
			return '\0';
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			if (base.Enabled && base.Visible)
			{
				foreach (TaskPaneGroup group in Groups)
				{
					if (!group.Visible || !group.Enabled || !group.ShowGroupCaption)
					{
						continue;
					}
					if (HasMnemonic(group.Text))
					{
						char groupMnemonic = char.ToLower(GetMnemonicChar(group.Text));
						if (charCode == groupMnemonic)
						{
							GroupEventArgs e = new GroupEventArgs(group);
							if (group.Expandable)
							{
								group.Expanded = !group.Expanded;
							}
							OnGroupClick(e);
							return true;
						}
					}
					foreach (TaskPaneItem item in group.Items)
					{
						if (item.Enabled && item.Group.Enabled && item.Visible && HasMnemonic(item.Text))
						{
							char itemMnemonic = char.ToLower(GetMnemonicChar(item.Text));
							if (charCode == itemMnemonic)
							{
								item.InvokeOnClick();
								return true;
							}
						}
					}
				}
			}
			return base.ProcessMnemonic(charCode);
		}

		internal void ToggleGroup(TaskPaneGroup group)
		{
			GroupCancelEventArgs e = new GroupCancelEventArgs(group);
			bool willExpand = !group._expanded;
			if (group.Expanded)
			{
				OnGroupCollapsing(e);
			}
			else
			{
				OnGroupExpanding(e);
			}
			if (e.Cancel)
			{
				return;
			}
			GroupEventArgs groupArgs = new GroupEventArgs(group);
			group.SetExpanded(willExpand);
			if (_effect != Effect.None && !base.DesignMode && !_hasContainerGroups && !_hasColumnSpan)
			{
				if (group.Items.Count > 0)
				{
					AnimateGroupToggle(group);
				}
			}
			else
			{
				_minTextWidth = 30;
				_isLayoutValid = false;
				UpdateHoverItem(null);
				Invalidate();
			}
			if (group.Expanded)
			{
				OnGroupExpanded(groupArgs);
			}
			else
			{
				OnGroupCollapsed(groupArgs);
			}
		}

		private TaskPaneItem FindItemAt(int x, int y)
		{
			if (_groups.Count > 0)
			{
				foreach (TaskPaneGroup item in _groups)
				{
					if (!item.Visible)
					{
						continue;
					}
					foreach (TaskPaneItem groupItem in item.Items)
					{
						if (groupItem.Visible && groupItem.Bounds.Contains(x, y))
						{
							return groupItem;
						}
					}
				}
			}
			return null;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			UpdateHoverItem(null);
			UpdateHoverGroup(null);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (GetHoverGroup() != null && (GetHoverGroup().IsCloseButtonPressed() || GetHoverGroup().GetItemState() == ItemState.Selected || GetHoverGroup().GetItemState() == ItemState.SelectedFocus) && GetHoverGroup().Enabled)
			{
				GetHoverGroup().SetCloseButtonPressed(false);
				if (_isCloseButtonPressed)
				{
					if (_hoverGroup.GetCloseButtonBounds().Contains(e.X, e.Y))
					{
						TaskPaneGroup taskPaneGroup = _hoverGroup;
						GroupCancelEventArgs closingArgs = new GroupCancelEventArgs(taskPaneGroup);
						OnGroupClosing(closingArgs);
						if (!closingArgs.Cancel)
						{
							taskPaneGroup.Visible = false;
							GroupEventArgs closedArgs = new GroupEventArgs(taskPaneGroup);
							OnGroupClosed(closedArgs);
						}
					}
				}
				else if (GetPressedGroup() == GetHoverGroup())
				{
					TaskPaneGroup pressedGroup = GetHoverGroup();
					if (!pressedGroup.GetExpandButtonBounds().Contains(e.X, e.Y))
					{
						GroupEventArgs clickArgs = new GroupEventArgs(GetHoverGroup());
						OnGroupClick(clickArgs);
					}
					ActiveGroup = pressedGroup;
					if (pressedGroup.Expandable)
					{
						pressedGroup.Expanded = !pressedGroup.Expanded;
						Invalidate(pressedGroup.GetHeaderBounds());
					}
					if (_hoverGroup != null)
					{
						GetHoverGroup().SetItemState(ItemState.Focus);
					}
				}
			}
			else if (GetPressedItem() != null && GetPressedItem() == GetItemAt(e.X, e.Y))
			{
				ItemEventArgs itemClickArgs = new ItemEventArgs(GetPressedItem());
				SelectedItem = GetPressedItem();
				ActiveItem = GetPressedItem();
				itemClickArgs.Item.InvokeOnClick();
			}
			_isCloseButtonPressed = false;
			UpdatePressedGroup(null);
			UpdatePressedItem(null);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			_shouldActivateFirstGroup = false;
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			if (_selectable)
			{
				TaskPanePaintHelper.FocusControl(this);
			}
			TaskPaneGroup taskPaneGroup = HitTestGroupHeader(e.X, e.Y);
			if (taskPaneGroup != null && taskPaneGroup.Enabled && base.Enabled)
			{
				_isCloseButtonPressed = taskPaneGroup.GetCloseButtonBounds().Contains(e.X, e.Y);
				if (_isCloseButtonPressed)
				{
					taskPaneGroup.SetCloseButtonPressed(true);
				}
				else
				{
					taskPaneGroup.SetItemState(ItemState.SelectedFocus);
					UpdateHoverGroup(taskPaneGroup);
				}
				UpdatePressedGroup(taskPaneGroup);
			}
			else
			{
				TaskPaneItem taskPaneItem = FindItemAt(e.X, e.Y);
				if (taskPaneItem != null && taskPaneItem.Visible && taskPaneItem.ItemType == ItemType.LinkButton && base.Enabled && taskPaneItem.Enabled && taskPaneItem.Group.Enabled && !taskPaneItem.Container)
				{
					UpdatePressedItem(taskPaneItem);
				}
			}
		}

		private TaskPaneGroup GetHoverGroup()
		{
			return _hoverGroup;
		}

		private void UpdateHoverGroup(TaskPaneGroup taskPaneTaskPaneGroup)
		{
			if (_hoverGroup != taskPaneTaskPaneGroup)
			{
				Rectangle empty = Rectangle.Empty;
				if (_hoverGroup != null)
				{
					GetHoverGroup().SetCloseButtonPressed(false);
					_hoverGroup.SetCloseButtonHot(false);
					_hoverGroup.SetItemState(ItemState.Normal);
					empty = _hoverGroup.GetHeaderBounds();
					empty.Inflate(1, 1);
					Invalidate(empty);
				}
				_hoverGroup = taskPaneTaskPaneGroup;
				if (_hoverGroup != null)
				{
					if (_hoverGroup.GetItemState() != ItemState.Selected && _hoverGroup.GetItemState() != ItemState.SelectedFocus && !_hoverGroup.IsCloseButtonHot())
					{
						_hoverGroup.SetItemState(ItemState.Focus);
					}
					empty = _hoverGroup.GetHeaderBounds();
					empty.Inflate(1, 1);
					Invalidate(empty);
				}
			}
			else if (_hoverGroup != null && _hoverGroup.GetItemState() != ItemState.SelectedFocus)
			{
				if (taskPaneTaskPaneGroup.IsCloseButtonHot() || _isCloseButtonPressed)
				{
					_hoverGroup.SetItemState(ItemState.Normal);
					return;
				}
				_hoverGroup.SetItemState(ItemState.Focus);
				Rectangle invalidateRect = _hoverGroup.GetHeaderBounds();
				invalidateRect.Inflate(1, 1);
				Invalidate(invalidateRect);
			}
		}

		private void UpdateHoverItem(TaskPaneItem taskPaneTaskPaneItem)
		{
			if (taskPaneTaskPaneItem == _hoverItem)
			{
				return;
			}
			if (_hoverItem != null)
			{
				_hoverItem._itemState = ItemState.Normal;
				Invalidate(_hoverItem.Bounds);
			}
			_hoverItem = taskPaneTaskPaneItem;
			if (_hoverItem != null)
			{
				if (_hoverItem._itemState != ItemState.SelectedFocus)
				{
					_hoverItem._itemState = ItemState.Focus;
				}
				Invalidate(_hoverItem.Bounds);
			}
		}

		private void ApplyCursor(Cursor cursor)
		{
			if (cursor == null)
			{
				if (_cursor != null)
				{
					base.Cursor = _cursor;
					_cursor = null;
				}
			}
			else if (cursor != Cursor)
			{
				if (_cursor == null)
				{
					_cursor = Cursor;
				}
				base.Cursor = cursor;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			TaskPaneGroup taskPaneGroup = HitTestGroupHeader(e.X, e.Y);
			if (taskPaneGroup != null)
			{
				ApplyCursor(taskPaneGroup.Cursor);
				if (taskPaneGroup.ToolTipText != string.Empty)
				{
					if (_toolTipGroupIndex != taskPaneGroup._index)
					{
						_toolTipGroupIndex = taskPaneGroup._index;
						_toolTipItemIndex = -1;
						ShowToolTip(taskPaneGroup.ToolTipText);
					}
				}
				else if (_toolTip != null)
				{
					_toolTip.Dispose();
					_toolTip = null;
					_toolTipItemIndex = -1;
					_toolTipGroupIndex = -1;
				}
				if (e.Button == MouseButtons.Left)
				{
					if (_isCloseButtonPressed)
					{
						taskPaneGroup.SetCloseButtonPressed(taskPaneGroup.GetCloseButtonBounds().Contains(e.X, e.Y));
					}
					else
					{
						taskPaneGroup.SetItemState(ItemState.SelectedFocus);
					}
				}
				UpdateHoverItem(null);
				taskPaneGroup.SetCloseButtonHot(taskPaneGroup.GetCloseButtonBounds().Contains(e.X, e.Y));
				UpdateHoverGroup(taskPaneGroup);
				return;
			}
			UpdateHoverGroup(null);
			TaskPaneItem taskPaneItem = FindItemAt(e.X, e.Y);
			if (taskPaneItem != null)
			{
				ApplyCursor(taskPaneItem.Cursor);
				if (taskPaneItem.ToolTipText != string.Empty)
				{
					if (_toolTipItemIndex != taskPaneItem._index)
					{
						_toolTipItemIndex = taskPaneItem._index;
						_toolTipGroupIndex = -1;
						ShowToolTip(taskPaneItem.ToolTipText);
					}
				}
				else if (_toolTip != null)
				{
					_toolTip.Dispose();
					_toolTip = null;
					_toolTipItemIndex = -1;
					_toolTipGroupIndex = -1;
				}
				if (taskPaneItem.ItemType == ItemType.LinkButton)
				{
					UpdateHoverItem(taskPaneItem);
				}
				else
				{
					UpdateHoverItem(null);
				}
			}
			else
			{
				ApplyCursor(null);
				if (_toolTip != null)
				{
					_toolTip.Dispose();
					_toolTip = null;
					_toolTipItemIndex = -1;
					_toolTipGroupIndex = -1;
				}
				UpdateHoverItem(null);
			}
		}

		private void ShowToolTip(string caption)
		{
			if ((ComputeForm() == null || ComputeForm().ContainsFocus) && _showToolTips)
			{
				if (_toolTip != null)
				{
					_toolTip.Dispose();
					_toolTip = null;
				}
				_toolTip = new Mordex.Windows.Common.ThemedToolTip();
				_toolTip.SetToolTip(this, caption);
			}
		}

		public HitTest HitTest()
		{
			Point position = PointerPosition.Current;
			position = PointToClient(position);
			return HitTest(position);
		}

		public HitTest HitTest(int x, int y)
		{
			Point location = new Point(x, y);
			return HitTest(location);
		}

		public HitTest HitTest(Point pt)
		{
			if (_verticalScrollbar.Bounds.Contains(pt) && _verticalScrollbar.Visible)
			{
				return Mordex.Windows.TaskPane.HitTest.ScrollBar;
			}
			foreach (TaskPaneGroup item in _groups)
			{
				if (item.GetHeaderBounds().Contains(pt))
				{
					if (item.GetExpandButtonBounds().Contains(pt))
					{
						return Mordex.Windows.TaskPane.HitTest.CollapseHeaderButton;
					}
					if (item.GetCloseButtonBounds().Contains(pt))
					{
						return Mordex.Windows.TaskPane.HitTest.CloseHeaderButton;
					}
					return Mordex.Windows.TaskPane.HitTest.TaskPaneGroup;
				}
				if (item.GetItemsBounds().Contains(pt))
				{
					return Mordex.Windows.TaskPane.HitTest.TaskPaneItem;
				}
			}
			return Mordex.Windows.TaskPane.HitTest.None;
		}

		public TaskPaneItem GetItemAt()
		{
			Point position = PointerPosition.Current;
			position = PointToClient(position);
			return FindItemAt(position.X, position.Y);
		}

		public TaskPaneItem GetItemAt(int x, int y)
		{
			return FindItemAt(x, y);
		}

		public TaskPaneItem GetItemAt(Point point)
		{
			return FindItemAt(point.X, point.Y);
		}

		public TaskPaneGroup GetGroupAt()
		{
			Point position = PointerPosition.Current;
			position = PointToClient(position);
			return HitTestGroupHeader(position.X, position.Y);
		}

		public TaskPaneGroup GetGroupAt(int x, int y)
		{
			return HitTestGroupHeader(x, y);
		}

		public TaskPaneGroup GetGroupAt(Point point)
		{
			return HitTestGroupHeader(point.X, point.Y);
		}

		private TaskPaneGroup HitTestGroupHeader(int x, int y)
		{
			foreach (TaskPaneGroup item in _groups)
			{
				if (item.Visible && item.GetHeaderBounds().Contains(x, y))
				{
					return item;
				}
			}
			return null;
		}
	}
}
