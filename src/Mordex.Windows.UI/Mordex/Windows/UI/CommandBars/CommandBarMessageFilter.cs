using System;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.CommandBars
{
	internal class CommandBarMessageFilter : IMessageFilter
	{
		private const int WmActivate = 6;

		private const int WmActivateApp = 28;

		private const int WmKeyDown = 256;

		private const int VkEscape = 27;

		private const int WmNcLButtonDown = 161;

		private const int WmNcLButtonUp = 162;

		private const int WmNcRButtonDown = 164;

		private const int WmNcRButtonUp = 165;

		private const int WmNcMButtonDown = 167;

		private const int WmNcMButtonUp = 168;

		private const int WmNcXButtonDown = 171;

		private const int WmNcXButtonUp = 172;

		private const int WmRButtonDown = 516;

		private const int WmRButtonUp = 517;

		private const int WmMButtonDown = 519;

		private const int WmMButtonUp = 520;

		private const int WmXButtonDown = 523;

		private const int WmXButtonUp = 524;

		private const uint CursorSizeAll = 32646u;

		private const uint CursorSizeWE = 32644u;

		private const int HorizontalDragThreshold = 7;

		private const int VerticalDragThreshold = 5;

		private const int RowEdgeMargin = 5;

		private const int DropZoneSize = 10;

		private const int DropZonePadding = 4;

		private const int WidthRetryStep = 4;
		private MxCommandBar _dragBar;

		internal MxCommandManager _commandManager;

		private IntPtr _previousCursor = IntPtr.Zero;

		private bool _hasPassedDragThreshold;

		private bool _isAtOriginalRow = true;

		private Point _grabOffset = Point.Empty;

		internal CommandControl _tearOffControl;

		internal Rectangle _dropZoneBounds = Rectangle.Empty;

		internal Point _floatingGrabOffset = Point.Empty;

		private bool _isResizing;

		private ResizeMode _resizeMode;

		private Point _dragAnchor = Point.Empty;

		private readonly DesignEditScope _designEdit;

		// At design time a bar drag is one undoable edit of the command manager and the dragged bar. It opens before the drag
		// changes any bar state and closes in EndDrag.
		private static DesignEditScope BeginDesignEdit(MxCommandBar dragBar, string description)
		{
			return DesignEditScope.BeginComponents(description, dragBar.CommandManager, dragBar);
		}

		internal static CommandBarMessageFilter StartMove(MxCommandBar dragBar, Point ptOffset, Action beforeMove)
		{
			DesignEditScope designEdit = BeginDesignEdit(dragBar, "Move Command Bar");
			try
			{
				beforeMove?.Invoke();
				return new CommandBarMessageFilter(designEdit, dragBar.CommandManager, dragBar, ptOffset);
			}
			catch
			{
				designEdit.Dispose();
				throw;
			}
		}

		internal static CommandBarMessageFilter StartWindowDrag(MxCommandBar dragBar, ResizeMode resizeMode, Point originalPoint, Point offsetPoint)
		{
			DesignEditScope designEdit = BeginDesignEdit(dragBar, resizeMode == ResizeMode.Floating ? "Move Command Bar" : "Resize Command Bar");
			try
			{
				return new CommandBarMessageFilter(designEdit, dragBar, resizeMode, originalPoint, offsetPoint);
			}
			catch
			{
				designEdit.Dispose();
				throw;
			}
		}

		private CommandBarMessageFilter(DesignEditScope designEdit, MxCommandManager commandManager, MxCommandBar dragBar, Point ptOffset)
		{
			_designEdit = designEdit;
			_commandManager = commandManager;
			_dragBar = dragBar;
			_dragAnchor = _dragBar.PointToScreen(new Point(0, 0));
			_grabOffset = ptOffset;
			if (_dragBar.IsHorizontalLayout())
			{
				_floatingGrabOffset.Y = (int)((float)_grabOffset.Y / (float)_dragBar.Height * (float)_dragBar.FloatingSize.Height);
				_floatingGrabOffset.X = 0;
			}
			else
			{
				_floatingGrabOffset.X = (int)((float)_grabOffset.X / (float)_dragBar.Width * (float)_dragBar.FloatingSize.Width);
			}
			if (_dragBar.GetHostRow() != null && !_dragBar.GetHost().GetRows().Contains(_dragBar.GetHostRow()))
			{
				_dragBar.RowIndex += 1;
			}
		}

		private CommandBarMessageFilter(DesignEditScope designEdit, MxCommandBar dragBar, ResizeMode resizeMode, Point originalPoint, Point offsetPoint)
		{
			_designEdit = designEdit;
			_commandManager = dragBar.CommandManager;
			_dragBar = dragBar;
			_resizeMode = resizeMode;
			_floatingGrabOffset = offsetPoint;
			switch (_resizeMode)
			{
			case ResizeMode.Right:
			case ResizeMode.Bottom:
				_dragAnchor = new Point(_dragBar.GetFloatingWindow().Right, _dragBar.GetFloatingWindow().Bottom);
				break;
			case ResizeMode.Top:
			case ResizeMode.Left:
				_dragAnchor = _dragBar.GetFloatingWindow().Location;
				break;
			default:
				_dragAnchor = originalPoint;
				break;
			}
			_isResizing = true;
			if (_dragBar.GetHostRow() != null && !_dragBar.GetHost().GetRows().Contains(_dragBar.GetHostRow()))
			{
				_dragBar.RowIndex += 1;
			}
		}

		public void EndDrag()
		{
			try
			{
				try
				{
					Application.RemoveMessageFilter(this);
				}
				catch
				{
				}
				_commandManager.OnDesignChange(EventArgs.Empty);
				if (_tearOffControl != null)
				{
					_tearOffControl.SetTearingOff(false);
				}
				if (_previousCursor != IntPtr.Zero)
				{
					NativeMethods.SetCursor(_previousCursor);
					_previousCursor = IntPtr.Zero;
				}
				_dragBar.EndGripDrag();
				_designEdit.End();
			}
			finally
			{
				_designEdit.Dispose();
			}
		}

		private void AddFloatingRow()
		{
			int barWidth = _dragBar.GetFullLength() / (_dragBar.GetFloatingRowCount() + 1);
			if (barWidth < _dragBar.GetMinimumLength())
			{
				barWidth = _dragBar.GetMinimumLength();
			}
			int rowCount = _dragBar.GetFullLength() / barWidth;
			if (_dragBar.GetFullLength() % barWidth != 0)
			{
				rowCount++;
			}
			while (rowCount > _dragBar.GetFloatingRowCount() + 1)
			{
				barWidth++;
				rowCount = _dragBar.GetFullLength() / barWidth;
				if (_dragBar.GetFullLength() % barWidth != 0)
				{
					rowCount++;
				}
			}
			_dragBar.FloatingSize = new Size(barWidth, _dragBar.FloatingSize.Height);
			_dragBar.RecalculateLayout();
			if (_dragBar.GetFullLength() / _dragBar.FloatingSize.Width < rowCount)
			{
				_dragBar.RecalculateLayout();
			}
			_dragBar.GetFloatingWindow().LayoutCaption(null);
		}

		private void RemoveFloatingRow()
		{
			WidenFloatingBar(_dragBar.GetFloatingRowCount(), 0);
		}

		private void WidenFloatingBar(int rowCount, int extraWidth)
		{
			if (rowCount > 1)
			{
				_dragBar.FloatingSize = new Size(_dragBar.GetFullLength() / (_dragBar.GetFloatingRowCount() - 1) + extraWidth, _dragBar.FloatingSize.Height);
				_dragBar.RecalculateLayout();
				_dragBar.GetFloatingWindow().LayoutCaption(null);
				if (_dragBar.GetFloatingRowCount() == rowCount)
				{
					WidenFloatingBar(rowCount, extraWidth + WidthRetryStep);
				}
			}
		}

		private void ResizeFloatingBar(Point screenPoint)
		{
			int rowCountBefore = _dragBar.GetFloatingRowCount();
			switch (_resizeMode)
			{
			case ResizeMode.Left:
				if (screenPoint.X < _dragAnchor.X)
				{
					if (screenPoint.X < _dragBar.GetFloatingWindow().Left)
					{
						int widthBeforeGrow = _dragBar.Width;
						RemoveFloatingRow();
						if (rowCountBefore != _dragBar.GetFloatingRowCount())
						{
							_dragBar.GetFloatingWindow().Left -= _dragBar.Width - widthBeforeGrow;
						}
						if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
						{
							_dragBar.GetFloatingWindow().GetOwnerForm().Update();
						}
						_dragAnchor.X = _dragBar.GetFloatingWindow().Left;
					}
				}
				else if (screenPoint.X > _dragAnchor.X && screenPoint.X > _dragBar.GetFloatingWindow().Left)
				{
					_dragAnchor.X = _dragBar.GetFloatingWindow().Left;
					int widthBeforeShrink = _dragBar.Width;
					AddFloatingRow();
					if (rowCountBefore != _dragBar.GetFloatingRowCount())
					{
						_dragBar.GetFloatingWindow().Left -= _dragBar.Width - widthBeforeShrink;
					}
					if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
					{
						_dragBar.GetFloatingWindow().GetOwnerForm().Update();
					}
				}
				break;
			case ResizeMode.Right:
			{
				Point point = _dragBar.PointToScreen(new Point(_dragBar.Right, _dragBar.Top));
				if (screenPoint.X > _dragAnchor.X)
				{
					if (screenPoint.X > point.X)
					{
						RemoveFloatingRow();
						if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
						{
							_dragBar.GetFloatingWindow().GetOwnerForm().Update();
						}
						_dragAnchor.X = point.X;
					}
				}
				else if (screenPoint.X < point.X)
				{
					_dragAnchor.X = point.X;
					AddFloatingRow();
					if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
					{
						_dragBar.GetFloatingWindow().GetOwnerForm().Update();
					}
				}
				break;
			}
			case ResizeMode.Top:
				if (screenPoint.Y > _dragAnchor.Y)
				{
					if (screenPoint.Y > _dragBar.GetFloatingWindow().Top)
					{
						int heightBeforeGrow = _dragBar.Height;
						RemoveFloatingRow();
						if (rowCountBefore != _dragBar.GetFloatingRowCount())
						{
							_dragBar.GetFloatingWindow().Top -= _dragBar.Height - heightBeforeGrow;
						}
						if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
						{
							_dragBar.GetFloatingWindow().GetOwnerForm().Update();
						}
						_dragAnchor.Y = _dragBar.GetFloatingWindow().Top;
					}
				}
				else if (screenPoint.Y < _dragAnchor.Y && screenPoint.Y < _dragBar.GetFloatingWindow().Top)
				{
					_dragAnchor.Y = _dragBar.GetFloatingWindow().Top;
					int heightBeforeShrink = _dragBar.Height;
					AddFloatingRow();
					if (rowCountBefore != _dragBar.GetFloatingRowCount())
					{
						_dragBar.GetFloatingWindow().Top -= _dragBar.Height - heightBeforeShrink;
					}
					if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
					{
						_dragBar.GetFloatingWindow().GetOwnerForm().Update();
					}
				}
				break;
			case ResizeMode.Bottom:
			{
				Point point = _dragBar.PointToScreen(new Point(_dragBar.Left, _dragBar.Height));
				if (screenPoint.Y > _dragAnchor.Y)
				{
					if (screenPoint.Y > point.Y)
					{
						AddFloatingRow();
						if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
						{
							_dragBar.GetFloatingWindow().GetOwnerForm().Update();
						}
						_dragAnchor.Y = point.Y;
					}
				}
				else if (screenPoint.Y < point.Y)
				{
					_dragAnchor.Y = point.Y;
					RemoveFloatingRow();
					if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
					{
						_dragBar.GetFloatingWindow().GetOwnerForm().Update();
					}
				}
				break;
			}
			}
		}

		public void UpdateDrag(Point pt)
		{
			_dragBar._savedRowIndex = -1;
			if (_dragBar.GetSourceCommand() != null && _tearOffControl != null && _dropZoneBounds.Contains(pt))
			{
				RenderingHelper.SetMouseCapture(_tearOffControl, true);
				_commandManager.CommandBars.Remove(_dragBar);
				_tearOffControl.Visible = true;
				EndDrag();
				return;
			}
			if (_isResizing && _resizeMode != ResizeMode.Floating)
			{
				ResizeFloatingBar(pt);
				return;
			}
			if (_previousCursor == IntPtr.Zero)
			{
				_previousCursor = NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, CursorSizeAll));
			}
			if (!_hasPassedDragThreshold && !TryDock(pt))
			{
				pt.Y -= _floatingGrabOffset.Y;
				pt.X -= _floatingGrabOffset.X;
				try
				{
					_dragBar.FloatingLocation = pt;
					if (_dragBar.GetFloatingWindow() == null)
					{
						_dragBar.DockStyle = BarsDockStyle.Floating;
					}
					if (_dragBar.GetFloatingWindow() != null)
					{
						_dragBar.GetFloatingWindow().InvalidateLayout();
						_dragBar.InvalidateLayout();
						if (_dragBar.GetFloatingWindow().GetOwnerForm() != null)
						{
							_dragBar.GetFloatingWindow().GetOwnerForm().Update();
						}
						_dragBar.GetFloatingWindow().Update();
					}
					return;
				}
				catch
				{
					return;
				}
			}
			if (_dragBar.IsFullRowLayout())
			{
				return;
			}
			MxCommandBarHost uiRebar = _dragBar.GetHost();
			if (uiRebar == null)
			{
				return;
			}
			Point point = uiRebar.PointToClient(pt);
			_dragBar.SetDragging(true);
			if (_dragBar.IsHorizontalLayout())
			{
				if (_dragBar.GetHostRow() != null && _dragBar.GetHostRow().IsContentFull() && _isAtOriginalRow)
				{
					if (_hasPassedDragThreshold)
					{
						if (_previousCursor != IntPtr.Zero)
						{
							NativeMethods.SetCursor(_previousCursor);
							_previousCursor = IntPtr.Zero;
						}
						_previousCursor = NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, CursorSizeWE));
					}
					else if (pt.X < _dragAnchor.X - HorizontalDragThreshold || pt.X > _dragAnchor.X + HorizontalDragThreshold)
					{
						_hasPassedDragThreshold = true;
					}
					else if (pt.X < _dragAnchor.X || pt.X > _dragAnchor.X)
					{
						return;
					}
				}
				if (point.X < 0)
				{
					point.X = 0;
				}
				else if (_dragBar.GetHostRow() != null && point.X > _dragBar.GetHostRow()._bounds.Right)
				{
					point.X = _dragBar.GetHostRow()._bounds.Right;
				}
				if (!_hasPassedDragThreshold)
				{
					_dragBar.SetPosition(new Point(point.X, _dragBar.GetPosition().Y));
				}
				_dragBar.Location = new Point(point.X, _dragBar.GetPosition().Y);
			}
			else
			{
				if (_dragBar.GetHostRow().IsContentFull() && _isAtOriginalRow)
				{
					if (_hasPassedDragThreshold)
					{
						if (_previousCursor != IntPtr.Zero)
						{
							NativeMethods.SetCursor(_previousCursor);
							_previousCursor = IntPtr.Zero;
						}
						_previousCursor = NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, CursorSizeWE));
					}
					else if (pt.Y <= _dragAnchor.Y - VerticalDragThreshold || pt.Y >= _dragAnchor.Y + VerticalDragThreshold)
					{
						_hasPassedDragThreshold = true;
					}
				}
				if (point.Y < 0)
				{
					point.Y = 0;
				}
				else if (point.Y > _dragBar.GetHostRow()._bounds.Bottom)
				{
					point.Y = _dragBar.GetHostRow()._bounds.Bottom;
				}
				if (!_hasPassedDragThreshold)
				{
					_dragBar.SetPosition(new Point(_dragBar.GetPosition().X, point.Y));
				}
				_dragBar.Location = new Point(_dragBar.GetPosition().X, point.Y);
			}
			if (_dragBar.Location != _dragBar.GetPosition())
			{
				_dragBar.SetPosition(_dragBar.Location);
			}
			else if (_dragBar.IsHorizontalLayout())
			{
				_dragBar.MoveToLeft(_dragBar.GetPosition().X);
			}
			else
			{
				_dragBar.MoveToTop(_dragBar.GetPosition().Y);
			}
			_dragBar.SetDragging(false);
			uiRebar.Update();
		}

		private void MoveBarToRow(MxCommandBarHost uiRebar, Point point)
		{
			if (point.X < 0)
			{
				point.X = 0;
			}
			if (point.Y < 0)
			{
				point.Y = 0;
			}
			if (_dragBar._messageFilter == null)
			{
				RenderingHelper.SetMouseCapture(_dragBar, true);
				_dragBar._messageFilter = this;
			}
			bool startsNewRow = false;
			int targetRow = uiRebar.HitTestRow(point.X, point.Y, ref startsNewRow);
			if (targetRow < 0)
			{
				return;
			}
			if (_dragBar.GetHostRow() != null)
			{
				if (targetRow < _dragBar.GetHost().GetRows().Count)
				{
					CommandBarHostLayoutState uiRebarLayoutState = (CommandBarHostLayoutState)_dragBar.GetHost().GetRows()[targetRow];
					if (uiRebarLayoutState.GetBars().Count > 1)
					{
						if (_dragBar.IsHorizontalLayout())
						{
							if (_dragBar.GetHost().Dock == DockStyle.Bottom)
							{
								if (point.Y >= uiRebarLayoutState._bounds.Bottom - RowEdgeMargin)
								{
									startsNewRow = true;
									targetRow++;
								}
							}
							else if (point.Y <= uiRebarLayoutState._bounds.Top + RowEdgeMargin)
							{
								startsNewRow = true;
							}
						}
						else if (_dragBar.GetHost().Dock == DockStyle.Right)
						{
							if (point.X >= uiRebarLayoutState._bounds.Right - RowEdgeMargin)
							{
								startsNewRow = true;
								targetRow++;
							}
						}
						else if (point.X <= uiRebarLayoutState._bounds.X + RowEdgeMargin)
						{
							startsNewRow = true;
						}
					}
					else
					{
						if (!uiRebarLayoutState.AllowsSharedRow(_dragBar) && targetRow > _dragBar.RowIndex)
						{
							switch (uiRebar.Dock)
							{
							case DockStyle.Top:
								if (point.Y < uiRebarLayoutState._bounds.Bottom - RowEdgeMargin)
								{
									targetRow = _dragBar.RowIndex;
								}
								break;
							case DockStyle.Bottom:
								if (point.Y > uiRebarLayoutState._bounds.Top + RowEdgeMargin)
								{
									targetRow = _dragBar.RowIndex;
								}
								break;
							case DockStyle.Left:
								if (point.X < uiRebarLayoutState._bounds.Right - RowEdgeMargin)
								{
									targetRow = _dragBar.RowIndex;
								}
								break;
							case DockStyle.Right:
								if (point.X < uiRebarLayoutState._bounds.X + RowEdgeMargin)
								{
									targetRow = _dragBar.RowIndex;
								}
								break;
							}
						}
						startsNewRow = false;
					}
				}
				else if (_dragBar.GetHostRow().GetBars().Count == 1)
				{
					targetRow--;
				}
			}
			_dragBar.SetDragging(true);
			if (startsNewRow || targetRow != _dragBar.RowIndex)
			{
				_isAtOriginalRow = false;
			}
			_dragBar._forceRowUpdate = startsNewRow;
			int rowIndex = _dragBar.RowIndex;
			_dragBar.RowIndex = targetRow;
			_dragBar.DockStyle = uiRebar.GetBarsDockStyle();
			if (rowIndex != _dragBar.RowIndex && _dragBar.GetHostRow() != null)
			{
				_dragBar.GetHostRow().PackBars();
			}
			_dragBar.SetDragging(false);
		}

		private bool TryDock(Point screenPoint)
		{
			if (_dragBar._isHostingPopupControl)
			{
				return false;
			}
			Point point;
			Rectangle clientRectangle;
			if (_dragBar.GetHost() != null)
			{
				point = _dragBar.GetHost().PointToClient(screenPoint);
				clientRectangle = _dragBar.GetHost().ClientRectangle;
				clientRectangle.Inflate(DropZonePadding, DropZonePadding);
				if (clientRectangle.Contains(point))
				{
					if (!_dragBar.GetHost()._isHorizontal)
					{
						if (point.X > _dragBar.GetHost().Width)
						{
							point.X = _dragBar.GetHost().Width;
						}
					}
					else if (point.Y > _dragBar.GetHost().Height)
					{
						point.Y = _dragBar.GetHost().Height;
					}
					MoveBarToRow(_dragBar.GetHost(), point);
					return true;
				}
			}
			if (_commandManager.TopRebar.HasAddedBar())
			{
				point = _commandManager.TopRebar.PointToClient(screenPoint);
				clientRectangle = _commandManager.TopRebar.ClientRectangle;
				clientRectangle.Height += DropZoneSize;
			}
			else
			{
				clientRectangle = _commandManager.ContainerControl.DisplayRectangle;
				clientRectangle.Height = DropZoneSize;
				point = _commandManager.ContainerControl.PointToClient(screenPoint);
			}
			if (clientRectangle.Contains(point))
			{
				MoveBarToRow(_commandManager.TopRebar, point);
				return true;
			}
			if (_commandManager.LeftRebar.HasAddedBar())
			{
				point = _commandManager.LeftRebar.PointToClient(screenPoint);
				clientRectangle = _commandManager.LeftRebar.ClientRectangle;
				clientRectangle.Width += DropZoneSize;
			}
			else
			{
				clientRectangle = _commandManager.ContainerControl.DisplayRectangle;
				clientRectangle.Width = DropZoneSize;
				point = _commandManager.ContainerControl.PointToClient(screenPoint);
			}
			if (clientRectangle.Contains(point))
			{
				MoveBarToRow(_commandManager.LeftRebar, point);
				return true;
			}
			if (_commandManager.BottomRebar.HasAddedBar())
			{
				point = _commandManager.BottomRebar.PointToClient(screenPoint);
				clientRectangle = _commandManager.BottomRebar.ClientRectangle;
				clientRectangle.Y -= DropZoneSize;
				clientRectangle.Height += DropZoneSize;
			}
			else
			{
				clientRectangle = _commandManager.ContainerControl.DisplayRectangle;
				clientRectangle.Y = clientRectangle.Bottom - DropZoneSize;
				clientRectangle.Height = DropZoneSize;
				foreach (Control control in _commandManager.ContainerControl.Controls)
				{
					if (control.Dock == DockStyle.Bottom && (control is MxStatusBar))
					{
						clientRectangle.Y -= control.Height;
					}
				}
				point = _commandManager.ContainerControl.PointToClient(screenPoint);
			}
			if (clientRectangle.Contains(point))
			{
				MoveBarToRow(_commandManager.BottomRebar, point);
				return true;
			}
			if (_commandManager.RightRebar.HasAddedBar())
			{
				point = _commandManager.RightRebar.PointToClient(screenPoint);
				clientRectangle = _commandManager.RightRebar.ClientRectangle;
				clientRectangle.X -= DropZoneSize;
				clientRectangle.Width += DropZoneSize;
			}
			else
			{
				clientRectangle = _commandManager.ContainerControl.DisplayRectangle;
				clientRectangle.X = clientRectangle.Right - DropZoneSize;
				clientRectangle.Width = DropZoneSize;
				point = _commandManager.ContainerControl.PointToClient(screenPoint);
			}
			if (clientRectangle.Contains(point))
			{
				MoveBarToRow(_commandManager.RightRebar, point);
				return true;
			}
			return false;
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case WmActivate:
			case WmActivateApp:
				EndDrag();
				break;
			case WmKeyDown:
				if ((int)message.WParam == VkEscape)
				{
					EndDrag();
				}
				break;
			case WmNcLButtonDown:
			case WmNcRButtonDown:
			case WmNcMButtonDown:
			case WmNcXButtonDown:
			case WmRButtonDown:
			case WmMButtonDown:
			case WmXButtonDown:
				EndDrag();
				break;
			case WmNcLButtonUp:
			case WmNcRButtonUp:
			case WmNcMButtonUp:
			case WmNcXButtonUp:
			case WmRButtonUp:
			case WmMButtonUp:
			case WmXButtonUp:
				return true;
			}
			return false;
		}
	}
}
