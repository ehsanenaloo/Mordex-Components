using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal partial class ControlStyleControl
	{
		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			try
			{
				base.OnMouseUp(eventArgs);
				if (_isTodayPressed)
				{
					UpdateTodayButtonHover(false);
					if (_gridLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y))
					{
						SelectToday();
					}
					else
					{
						GridHelper.SetCapture(this, true);
					}
					_isTodayPressed = false;
					StartRecaptureTimer();
					return;
				}
				if (_isNonePressed)
				{
					SetNoneButtonPressed(false);
					if (_controlStyleControlGridLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y))
					{
						SelectNone();
					}
					_isNonePressed = false;
					StartRecaptureTimer();
					return;
				}
				_timer.Enabled = false;
				_isRangeSelecting = false;
				_rangeStartDate = null;
				if (base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
				{
					CalendarDayCell dayCellAt = FindDayCellAt(eventArgs.X, eventArgs.Y);
					if (dayCellAt != null)
					{
						CloseDropDown(false);
					}
					else
					{
						StartRecaptureTimer();
					}
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			try
			{
				base.OnMouseMove(eventArgs);
				if (_isTodayPressed)
				{
					UpdateTodayButtonHover(_gridLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y));
				}
				else if (_isNonePressed)
				{
					SetNoneButtonPressed(_controlStyleControlGridLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y));
				}
				else if (_isRangeSelecting)
				{
					CalendarDayCell dayCellAt = FindDayCellAt(eventArgs.X, eventArgs.Y);
					if (dayCellAt != null)
					{
						ApplyRangeSelection(new CalendarValueState(_rangeStartDate, dayCellAt.GetDate()));
					}
				}
				else if (eventArgs.Button == MouseButtons.None)
				{
					SetTodayButtonHot(_gridLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y));
					SetNoneButtonHot(_controlStyleControlGridLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y));
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			SetTodayButtonHot(false);
			UpdateTodayButtonHover(false);
			_isTodayPressed = false;
			SetNoneButtonPressed(false);
			SetNoneButtonPressed(false);
			_isNonePressed = false;
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			try
			{
				base.OnMouseDown(eventArgs);
				if (_isDropDownShown && !base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
				{
					GridHelper.SetCapture(this, false);
					CloseDropDown(true);
					return;
				}
				for (int i = 0; i < 2; i++)
				{
					if (_arrowRectangles[i].Contains(eventArgs.X, eventArgs.Y))
					{
						_ = _scrollMonthStep;
						if (i == 0)
						{
							_scrollMonthDelta = -_scrollMonthStep;
						}
						else
						{
							_scrollMonthDelta = _scrollMonthStep;
						}
						try
						{
							SetVisibleMonth(_dateTime.AddMonths(_scrollMonthDelta));
						}
						catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
						{
							GridExceptions.Report(caught);
						}
						_timer.Enabled = true;
						return;
					}
				}
				CalendarLayoutState calendarLayoutState = FindMonthTitleAt(eventArgs.X, eventArgs.Y);
				if (calendarLayoutState != null)
				{
					_clickedMonthIndex = calendarLayoutState.GetIndex();
					Rectangle rectangle = calendarLayoutState.GetTitleLayout()._bounds;
					rectangle.Inflate(-12, -1);
					Point location = rectangle.Location;
					location = PointToScreen(location);
					_innerCalendarControl.BackColor = _calendarStyle.WindowColor;
					_innerCalendarControl.ForeColor = _calendarStyle.WindowTextColor;
					_innerCalendarControl.Font = Font;
					_innerCalendarControl.SetSelectedMonth(new DateTime(calendarLayoutState.GetYear(), calendarLayoutState.GetMonth(), 1));
					_innerCalendarControl._highlightColor = SystemColors.Highlight;
					_innerCalendarControl._selectedTextColor = SystemColors.HighlightText;
					Color color = _gridFormatStyle.GetEffectiveBackColor();
					_innerCalendarControl._highlightColor = color;
					if (color.IsEmpty || color.A == 0)
					{
						_innerCalendarControl._highlightColor = SystemColors.Highlight;
					}
					color = _gridFormatStyle.ForeColor;
					_innerCalendarControl._selectedTextColor = color;
					if (color.IsEmpty)
					{
						if (_innerCalendarControl._highlightColor == SystemColors.Highlight)
						{
							_innerCalendarControl._selectedTextColor = SystemColors.HighlightText;
						}
						else
						{
							_innerCalendarControl._selectedTextColor = SystemColors.WindowText;
						}
					}
					_innerCalendarControl.ShowPopupAt(location, rectangle.Size, (DateTime)_innerCalendarControl.GetSelectedMonth(), _mordexColorSchemeAppearanceState.GetUseTextRenderer());
					return;
				}
				CalendarDayCell dayCellAt = FindDayCellAt(eventArgs.X, eventArgs.Y);
				if (dayCellAt != null)
				{
					switch (_selectionStyleEnum)
					{
					case SelectionStyleEnum.Single:
						SetSelectedValue(dayCellAt.GetDate());
						break;
					case SelectionStyleEnum.Range:
						_rangeStartDate = dayCellAt.GetDate();
						ApplyRangeSelection(new CalendarValueState(dayCellAt.GetDate(), dayCellAt.GetDate()));
						_isRangeSelecting = true;
						break;
					case SelectionStyleEnum.Free:
						break;
					}
				}
				else
				{
					SetTodayButtonHot(false);
					SetNoneButtonHot(false);
					if (_gridLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y))
					{
						UpdateTodayButtonHover(true);
						_isTodayPressed = true;
					}
					else if (_controlStyleControlGridLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y))
					{
						SetNoneButtonPressed(true);
						_isNonePressed = true;
					}
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private void StartRecaptureTimer()
		{
			_controlStyleControlTimer = new Timer();
			_controlStyleControlTimer.Interval = Timing.DeferredActionInterval;
			_controlStyleControlTimer.Tick += OnRecaptureTimerTick;
			_controlStyleControlTimer.Enabled = true;
		}

		private void OnRecaptureTimerTick(object sender, EventArgs eventArgs)
		{
			if (base.Visible)
			{
				GridHelper.SetCapture(this, true);
			}
			if (_controlStyleControlTimer != null)
			{
				_controlStyleControlTimer.Stop();
				_controlStyleControlTimer.Tick -= OnRecaptureTimerTick;
				_controlStyleControlTimer.Dispose();
				_controlStyleControlTimer = null;
			}
		}

		internal void ShowDropDown(Rectangle otherBounds, IGridDropListOwner dropOwner)
		{
			_isDropDownShown = true;
			_dropOwner = dropOwner;
			DisposeResources();
			LayoutFooterButtons();
			base.Visible = false;
			base.Parent = dropOwner as Control;
			int x = otherBounds.Right - base.Width - 3;
			int y = otherBounds.Bottom + 1;
			_originalParentHandle = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			_ = SystemInformation.WorkingArea;
			Point location = new Point(x, y);
			Screen screen = Screen.FromRectangle(otherBounds);
			if (screen == null)
			{
				screen = Screen.PrimaryScreen;
			}
			Rectangle bounds = screen.Bounds;
			if (location.X + base.Width > bounds.Right)
			{
				location.X = bounds.Right - base.Width;
			}
			if (location.X < bounds.Left)
			{
				location.X = bounds.Left;
			}
			if (location.Y + base.Height > bounds.Bottom)
			{
				if (otherBounds.Y > bounds.Bottom - otherBounds.Bottom)
				{
					location.Y = otherBounds.Y - base.Height;
					if (location.Y < bounds.Top)
					{
						location.Y = bounds.Top;
						base.Height = otherBounds.Y;
					}
				}
				else
				{
					base.Height = bounds.Bottom - otherBounds.Bottom;
				}
			}
			base.Location = location;
			NativeMethods.SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 3);
			base.Visible = true;
			Update();
		}

		internal void CloseDropDown(bool cancel)
		{
			if (_isDropDownShown)
			{
				HideDropDown();
				if (_dropOwner != null)
				{
					_dropOwner.OnDropListHide(cancel);
					_dropOwner = null;
				}
			}
		}

		internal void HideDropDown()
		{
			_isDropDownShown = false;
			Hide();
			NativeMethods.SetParent(base.Handle, _originalParentHandle);
			GridHelper.SetCapture(this, false);
			_originalParentHandle = IntPtr.Zero;
		}
	}
}
