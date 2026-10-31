using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	internal partial class SchedulerAppointmentView
	{
		public void HandleMouseUp(MouseEventArgs eventArgs)
		{
			if (!_isMouseDown)
			{
				return;
			}
			_mouseDownPosition = Point.Empty;
			if (!_wasDoubleClicked)
			{
				if (_wasFocusedOnMouseDown)
				{
					StartEditTimerIfAllowed();
				}
			}
			else
			{
				_wasDoubleClicked = false;
			}
			_isMouseDown = false;
		}

		public void HandleDoubleClick(EventArgs eventArgs)
		{
			_editDelayTimer.Stop();
			_scheduler.RaiseAppointmentDoubleClick(_appointment);
			_wasDoubleClicked = true;
		}

		public void HandleMouseLeave(EventArgs eventArgs)
		{
			_scheduler.SetCursor(null);
			HideToolTip();
		}

		public void HandleMouseMove(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.None && _hasHiddenText && !_scheduler._isOwnerListVisible)
			{
				ShowToolTip();
			}
			if (_allowAppointmentDrag == AllowAppointmentDrag.None)
			{
				return;
			}
			if (eventArgs.Button == MouseButtons.Left && _mouseDownPosition != Point.Empty)
			{
				Point position = Cursor.Position;
				if (IsOutsideDragRectangle(position.X, position.Y))
				{
					HideToolTip();
					CloseEditorIfOpen();
					_scheduler._schedulerSchedulerAppointment = _appointment;
					_scheduler._isAppointmentDragActive = true;
					_scheduler.StartAppointmentDrag(_appointment, _date.Add(_timeSpan), _mouseDownPosition);
					_scheduler._isAppointmentDragActive = false;
					_mouseDownPosition = Point.Empty;
				}
				return;
			}
			if (_view == SchedulerView.DayView)
			{
				Rectangle rectangle = _bounds;
				rectangle.Width = 7;
				_isPointerOverText = true;
				int resizeZoneHeight = 5;
				if (_appointmentState == AppointmentState.Focus)
				{
					resizeZoneHeight += 5;
				}
				if (rectangle.Contains(eventArgs.X, eventArgs.Y))
				{
					_isPointerOverText = false;
					if ((_allowAppointmentDrag & AllowAppointmentDrag.Move) != AllowAppointmentDrag.None)
					{
						_scheduler.SetCursor(Cursors.SizeAll);
						_dragType = DragType.Move;
					}
				}
				else if (eventArgs.Y >= _bounds.Bottom - resizeZoneHeight && (_allowAppointmentDrag & AllowAppointmentDrag.Resize) != AllowAppointmentDrag.None)
				{
					if (_scheduler.IsDragTimeAllowed(_appointment, AppointmentDragType.EndTimeChange))
					{
						_scheduler.SetCursor(Cursors.SizeNS);
						_dragType = DragType.ResizeRight;
					}
				}
				else if (eventArgs.Y <= _bounds.Top + resizeZoneHeight && (_allowAppointmentDrag & AllowAppointmentDrag.Resize) != AllowAppointmentDrag.None)
				{
					if (_scheduler.IsDragTimeAllowed(_appointment, AppointmentDragType.StartTimeChange))
					{
						_scheduler.SetCursor(Cursors.SizeNS);
						_dragType = DragType.ResizeLeft;
					}
				}
				else
				{
					_scheduler.SetCursor(null);
					_dragType = DragType.Move;
				}
			}
			else
			{
				_isPointerOverText = true;
				if (eventArgs.X <= _bounds.Left + 5 && _isFirstSegment && (_allowAppointmentDrag & AllowAppointmentDrag.Resize) != AllowAppointmentDrag.None)
				{
					_dragType = DragType.ResizeLeft;
					_scheduler.SetCursor(Cursors.SizeWE);
				}
				else if (eventArgs.X >= _bounds.Right - 5 && (_isLastSegment || IsContinuedToNext()) && (_allowAppointmentDrag & AllowAppointmentDrag.Resize) != AllowAppointmentDrag.None)
				{
					_dragType = DragType.ResizeRight;
					_scheduler.SetCursor(Cursors.SizeWE);
				}
				else if (_timeAreaBounds.Contains(eventArgs.X, eventArgs.Y))
				{
					if ((_allowAppointmentDrag & AllowAppointmentDrag.Move) != AllowAppointmentDrag.None)
					{
						_scheduler.SetCursor(Cursors.SizeAll);
						_dragType = DragType.Move;
					}
				}
				else
				{
					_dragType = DragType.Move;
					_scheduler.SetCursor(null);
				}
			}
			_scheduler._dragType = _dragType;
		}

		public void HandleMouseDown(MouseEventArgs eventArgs)
		{
			HideToolTip();
			_isMouseDown = true;
			_mouseDownPosition = Cursor.Position;
			if (_appointmentState == AppointmentState.Focus)
			{
				_wasFocusedOnMouseDown = true;
			}
			else
			{
				_wasFocusedOnMouseDown = false;
			}
			_scheduler.UpdateSelectionForClick(_appointment);
			_scheduler.CommitEditOfOtherView(this);
			_scheduler._schedulerOwner = this;
		}
	}
}
