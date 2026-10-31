using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonContextMenu
	{
		private bool TryShowForControl(Control item)
		{
			if (_menuTargets != null && _menuTargets.Count > 0 && _menuTargets.Contains(item))
			{
				ContextMenuStrip contextMenu = null;
				if (item != null)
				{
					contextMenu = item.ContextMenuStrip;
					item.ContextMenuStrip = null;
				}
				try
				{
					Show(item);
				}
				finally
				{
					if (item != null)
					{
						item.ContextMenuStrip = contextMenu;
					}
				}
				return true;
			}
			return false;
		}

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (sender is Control)
			{
				if ((!(sender is TreeView) && !(sender is ListView)) || eventArgs.Button != MouseButtons.Right)
				{
					return;
				}
				Control control = (Control)sender;
				ContextMenuStrip contextMenu = control.ContextMenuStrip;
				control.ContextMenuStrip = null;
				try
				{
					Show(control);
				}
				finally
				{
					control.ContextMenuStrip = contextMenu;
				}
				return;
			}
			if (sender is Component)
			{
				Show();
			}
		}

		private void OnControlKeyUp(object sender, KeyEventArgs eventArgs)
		{
			if (eventArgs.KeyData != Keys.Apps)
			{
				return;
			}
			Point empty = Point.Empty;
			if (sender is TextBoxBase)
			{
				NativeMethods.NativePoint m = default(NativeMethods.NativePoint);
				if (NativeMethods.GetCaretPos(ref m))
				{
					empty.X = m._x;
					empty.Y = m._y;
				}
			}
			Control control = sender as Control;
			if (empty.IsEmpty)
			{
				empty.X = control.Width / 2;
				empty.Y = control.Height / 2;
			}
			Show(control, empty);
		}

		private void OnControlComponentRename(object sender, ComponentRenameEventArgs eventArgs)
		{
			if (Site != null && Site.Name != null)
			{
				_name = Site.Name;
			}
			if (eventArgs.Component is CommandBase)
			{
				CommandBase commandBase = (CommandBase)eventArgs.Component;
				commandBase.SyncNameFromSite();
			}
		}

		internal bool TryClose(CommandCloseReason closeReason)
		{
			if (GetChildMenu() != null)
			{
				if (GetChildMenu().GetParentMenuCommand() != null && GetChildMenu().GetParentMenuCommand() is DropDownCommand)
				{
					DropDownCommand dropDownCommand = GetChildMenu().GetParentMenuCommand() as DropDownCommand;
					if (dropDownCommand.CanCloseDropDown(closeReason))
					{
						SetChildMenu(null);
						return true;
					}
					return false;
				}
				DropDownClosingEventArgs e = new DropDownClosingEventArgs(null, closeReason);
				OnClosing(e);
				if (!e.Cancel)
				{
					SetChildMenu(null);
				}
				return !e.Cancel;
			}
			return true;
		}

		public void Show()
		{
			ShowAt(null, Cursor.Position);
		}

		public void Show(Control control)
		{
			ShowAt(control, Cursor.Position);
		}

		public void Show(Control control, int x, int y)
		{
			Point p = new Point(x, y);
			p = control.PointToScreen(p);
			ShowAt(control, p);
		}

		public void Show(Control control, Point pos)
		{
			pos = control.PointToScreen(pos);
			ShowAt(control, pos);
		}

		public void Show(Point pos)
		{
			ShowAt(null, pos);
		}

		public void Close()
		{
			TryClose(CommandCloseReason.CloseCalled);
		}

		internal void CloseOrForwardDropDown(DropDownCommand dropDownCommand)
		{
			if (GetChildMenu() != null)
			{
				if (GetChildMenu().GetParentMenuCommand() == dropDownCommand)
				{
					TryClose(CommandCloseReason.CloseCalled);
				}
				else
				{
					GetChildMenu().CloseChildMenuFor(dropDownCommand);
				}
			}
		}

		internal void ShowAt(Control control, Point point)
		{
			_control = control;
			CancelEventArgs e = new CancelEventArgs();
			OnOpening(e);
			if (e.Cancel || (_commandCollection.Count == 0 && !base.DesignMode))
			{
				return;
			}
			CommandBaseEditorDialog commandBaseEditorDialog = new CommandBaseEditorDialog(this, _commandCollection, null, null);
			commandBaseEditorDialog._anchorSize = _anchorSize;
			commandBaseEditorDialog._upwardOffset = _upwardOffset;
			SetChildMenu(commandBaseEditorDialog);
			if (Form.ActiveForm == null)
			{
				GetChildMenu()._activateOnShow = true;
			}
			OnOpened(EventArgs.Empty);
			commandBaseEditorDialog.ShowAt(point);
		}

		protected virtual void OnClosed(EventArgs e)
		{
			if (base.Events[s_closedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnClosing(DropDownClosingEventArgs e)
		{
			if (base.Events[s_closingEvent] is DropDownClosingEventHandler dropDownClosingEventHandler)
			{
				dropDownClosingEventHandler(this, e);
			}
		}

		protected virtual void OnOpened(EventArgs e)
		{
			if (base.Events[s_openedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnOpening(CancelEventArgs e)
		{
			if (base.Events[s_openingEvent] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}
	}
}
