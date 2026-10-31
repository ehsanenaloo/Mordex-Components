using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	internal class CommandDragState
	{
		private const int WmActivate = 6;

		private const int WmActivateApp = 28;

		private const int WmSetCursor = 32;

		private const int WmNcLButtonDown = 161;

		private const int WmNcLButtonUp = 162;

		private const int WmNcRButtonDown = 164;

		private const int WmNcRButtonUp = 165;

		private const int WmNcMButtonDown = 167;

		private const int WmNcMButtonUp = 168;

		private const int WmNcXButtonDown = 171;

		private const int WmNcXButtonUp = 172;

		private const int WmKeyDown = 256;

		private const int WmKeyUp = 257;

		private const int WmSysKeyDown = 260;

		private const int WmSysKeyUp = 261;

		private const int WmMouseMove = 512;

		private const int WmLButtonDown = 513;

		private const int WmLButtonUp = 514;

		private const int WmRButtonDown = 516;

		private const int WmRButtonUp = 517;

		private const int WmMButtonDown = 519;

		private const int WmMButtonUp = 520;

		private const int WmXButtonDown = 523;

		private const int WmXButtonUp = 524;

		private const int WmMouseLeave = 675;

		private const int VkControl = 17;

		private const int VkEscape = 27;

		private const int VkLeftWin = 91;

		private bool _isCopying;

		private Rectangle _itemBounds = Rectangle.Empty;

		private MxCommandCollection _collectionOwner;

		private MxCommand _dragCommand;

		private MxCommandManager _commandManager;

		private IntPtr _previousCursor = IntPtr.Zero;

		private bool _isDragStarted;

		private Point _dragStartPoint = Point.Empty;

		private Cursor _dragCursor;

		private Cursor _invalidDragCursor;

		private Cursor _moveCursor;

		private MxCommandBar _dropTargetBar;

		internal CommandDragState(MxCommandManager commandManager, MxCommand dragCommand)
			: this(commandManager, dragCommand, null, Rectangle.Empty)
		{
		}

		internal CommandDragState(MxCommandManager commandManager, MxCommand dragCommand, MxCommandCollection collectionOwner, Rectangle itemBounds)
		{
			_itemBounds = itemBounds;
			_collectionOwner = collectionOwner;
			if (_collectionOwner == null)
			{
				_isDragStarted = true;
			}
			else
			{
				_dragStartPoint = Cursor.Position;
			}
			_commandManager = commandManager;
			_dragCommand = dragCommand;
			_isCopying = Control.ModifierKeys == Keys.Control && dragCommand.CommandType != CommandType.ControlContainer;
			Cursor cursor = null;
			cursor = ((_collectionOwner != null && !_isCopying) ? GetCursor("ToolBarMove") : GetCursor("ToolBarDrag"));
			if (cursor != null)
			{
				_previousCursor = NativeMethods.SetCursor(cursor.Handle);
			}
		}

		private bool CheckDragStarted()
		{
			if (_isDragStarted)
			{
				return true;
			}
			Point position = Cursor.Position;
			Size dragSize = SystemInformation.DragSize;
			if (!new Rectangle(_dragStartPoint.X - dragSize.Width, _dragStartPoint.Y - dragSize.Height, dragSize.Width * 2, dragSize.Height * 2).Contains(position.X, position.Y))
			{
				_isDragStarted = true;
			}
			return _isDragStarted;
		}

		internal void RunDragLoop()
		{
			NativeMethods.NativeMsg message = default(NativeMethods.NativeMsg);
			bool isDone = false;
			while (!isDone)
			{
				if (!NativeMethods.WaitMessage())
				{
					EndDrag(true);
					break;
				}
				while (!isDone && NativeMethods.PeekMessage(ref message, IntPtr.Zero, 0u, 0u, 0u))
				{
					bool shouldSwallow = false;
					switch (message._message)
					{
					case WmActivate:
					case WmActivateApp:
						EndDrag(true);
						isDone = true;
						break;
					case WmMouseMove:
						if (CheckDragStarted())
						{
							UpdateDropTarget(Cursor.Position);
						}
						shouldSwallow = true;
						break;
					case WmLButtonUp:
						EndDrag(false);
						isDone = true;
						break;
					case WmRButtonUp:
					case WmMButtonUp:
					case WmXButtonUp:
						shouldSwallow = true;
						break;
					case WmNcLButtonUp:
					case WmNcRButtonUp:
					case WmNcMButtonUp:
					case WmNcXButtonUp:
						EndDrag(true);
						shouldSwallow = true;
						break;
					case WmNcLButtonDown:
					case WmNcRButtonDown:
					case WmNcMButtonDown:
					case WmNcXButtonDown:
					case WmLButtonDown:
					case WmRButtonDown:
					case WmMButtonDown:
					case WmXButtonDown:
						shouldSwallow = true;
						break;
					case WmKeyUp:
					case WmSysKeyDown:
						if ((int)message._wParam == VkControl)
						{
							_isCopying = false;
							UpdateDropTarget(Cursor.Position);
						}
						shouldSwallow = true;
						break;
					case WmSysKeyUp:
						shouldSwallow = true;
						break;
					case WmKeyDown:
						if ((int)message._wParam == VkLeftWin)
						{
							EndDrag(true);
							isDone = true;
						}
						else if ((int)message._wParam == VkEscape)
						{
							EndDrag(true);
							isDone = true;
						}
						else if ((int)message._wParam == VkControl)
						{
							_isCopying = true;
							UpdateDropTarget(Cursor.Position);
						}
						shouldSwallow = true;
						break;
					case WmSetCursor:
						shouldSwallow = true;
						break;
					case WmMouseLeave:
						shouldSwallow = true;
						break;
					}
					if (shouldSwallow)
					{
						NativeMethods.NativeMsg consumedMessage = default(NativeMethods.NativeMsg);
						if (!NativeMethods.TryGetMessage(ref consumedMessage) && !isDone)
						{
							EndDrag(true);
							isDone = true;
						}
					}
					else if (NativeMethods.TryGetMessage(ref message))
					{
						NativeMethods.TranslateMessage(ref message);
						NativeMethods.DispatchMessage(ref message);
					}
					else if (!isDone)
					{
						EndDrag(true);
						isDone = true;
					}
				}
			}
		}

		private Cursor GetCursor(string cursorName)
		{
			Stream stream = null;
			if (cursorName == "ToolBarDrag")
			{
				if (_dragCursor == null)
				{
					stream = new MemoryStream(WindowsResources.ToolBarDrag());
					_dragCursor = new Cursor(stream);
					stream.Close();
				}
				return _dragCursor;
			}
			if (cursorName == "ToolBarInvalidDrag")
			{
				if (_invalidDragCursor == null)
				{
					stream = new MemoryStream(WindowsResources.ToolBarInvalidDrag());
					_invalidDragCursor = new Cursor(stream);
					stream.Close();
				}
				return _invalidDragCursor;
			}
			if (cursorName == "ToolBarMove")
			{
				if (_moveCursor == null)
				{
					stream = new MemoryStream(WindowsResources.ToolBarMove());
					_moveCursor = new Cursor(stream);
					stream.Close();
				}
				return _moveCursor;
			}
			return null;
		}

		public void UpdateDropTarget(Point point)
		{
			MxCommandBar uiCommandBar = null;
			if (_dropTargetBar != null && _dropTargetBar.ContainsScreenPoint(point))
			{
				uiCommandBar = _dropTargetBar;
			}
			if (uiCommandBar == null)
			{
				foreach (MxCommandBar commandBar in _commandManager.CommandBars)
				{
					if (commandBar.ContainsScreenPoint(point))
					{
						uiCommandBar = commandBar;
						break;
					}
				}
			}
			if (uiCommandBar != null)
			{
				if (_dropTargetBar != uiCommandBar)
				{
					if (_dropTargetBar != null)
					{
						_dropTargetBar.SetDropIndex(-1);
						_dropTargetBar.SetChildMenu(null);
					}
					else
					{
						foreach (MxCommandBar otherBar in _commandManager.CommandBars)
						{
							if (otherBar != uiCommandBar)
							{
								otherBar.SetChildMenu(null);
							}
						}
					}
					_dropTargetBar = uiCommandBar;
				}
				Cursor cursor = ((_collectionOwner != null && !_isCopying) ? GetCursor("ToolBarMove") : GetCursor("ToolBarDrag"));
				if (cursor != null)
				{
					NativeMethods.SetCursor(cursor.Handle);
				}
				uiCommandBar.DragOverCommand(_dragCommand, point, _collectionOwner, _isCopying);
			}
			else
			{
				if (_dropTargetBar != null)
				{
					_dropTargetBar.ClearDropMarker();
					_dropTargetBar = null;
				}
				Cursor cursor = GetCursor("ToolBarInvalidDrag");
				if (cursor != null)
				{
					NativeMethods.SetCursor(cursor.Handle);
				}
			}
		}

		public void EndDrag(bool isCancelled)
		{
			if (_previousCursor != IntPtr.Zero)
			{
				NativeMethods.SetCursor(_previousCursor);
			}
			if (!CheckDragStarted())
			{
				return;
			}
			if (!isCancelled)
			{
				Point position = Cursor.Position;
				if (_itemBounds.Contains(position))
				{
					return;
				}
				if (_dropTargetBar != null)
				{
					if (_dropTargetBar.ContainsScreenPoint(position))
					{
						_dropTargetBar.DropCommand(_dragCommand, position, _collectionOwner, _isCopying);
						return;
					}
					if (_collectionOwner != null && !_isCopying)
					{
						_collectionOwner.Remove(_dragCommand);
						_commandManager.SetActiveElement(null);
					}
					_dropTargetBar.SetDropIndex(-1);
					_dropTargetBar.SetChildMenu(null);
				}
				else
				{
					if (_collectionOwner == null || _isCopying)
					{
						return;
					}
					if (_dragCommand.IsMergedCommand)
					{
						_dragCommand.Visible = InheritableBoolean.False;
						if (_collectionOwner._owner is MxCommandBar && ((MxCommandBar)_collectionOwner._owner).GetIsMergedCommandBar())
						{
							_commandManager.SetActiveElement(null);
							_commandManager.RefreshRebars();
							return;
						}
					}
					_collectionOwner.Remove(_dragCommand);
					_commandManager.SetActiveElement(null);
				}
				return;
			}
			foreach (MxCommandBar commandBar in _commandManager.CommandBars)
			{
				commandBar.SetDropIndex(-1);
				commandBar.SetChildMenu(null);
			}
		}
	}
}
