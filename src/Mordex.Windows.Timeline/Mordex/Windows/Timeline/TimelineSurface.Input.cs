using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public partial class TimelineSurface
	{
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			UpdateTimelineLayoutState(null);
			SetHoveredButton(null);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (Visible && _isDroppedDown)
			{
				_calendarTimer = new Timer();
				_calendarTimer.Interval = 20;
				_calendarTimer.Tick += OnCalendarTimerTick;
				_calendarTimer.Enabled = true;
			}
			_scrollTimer.Enabled = false;
			_scrollTimer.Interval = 200;
			if (_pressedButton == _todayButton)
			{
				if (_todayButton._bounds.Contains(e.X, e.Y))
				{
					SelectToday();
				}
				SetPressedButton(null);
			}
			else if (_pressedButton == _noneButton)
			{
				if (_noneButton._bounds.Contains(e.X, e.Y))
				{
					SelectNone();
				}
				SetPressedButton(null);
			}
			_scrollTimer.Enabled = false;
			_scrollTimer.Interval = 200;
			if (base.ClientRectangle.Contains(e.X, e.Y))
			{
				TimelineCalendarDay dayCell = GetDayCellAt(e.X, e.Y, false);
				if (dayCell != null)
				{
					UnDrop(cancel: false);
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (_pressedButton != null)
			{
				if (GetButtonAt(e.X, e.Y) != _pressedButton)
				{
					_pressedButton.SetPressed(false);
				}
				else
				{
					_pressedButton.SetPressed(true);
				}
				return;
			}
			if (e.Button == MouseButtons.None)
			{
				SetHoveredButton(GetButtonAt(e.X, e.Y));
			}
			TimelineLayoutState timelineLayoutState;
			try
			{
				FindMonthByWeekNumberArea(e.X, e.Y);
				timelineLayoutState = FindMonthByHeaderAt(e.X, e.Y, false);
			}
			catch
			{
				return;
			}
			if (timelineLayoutState != null)
			{
				if (timelineLayoutState != _timelineLayoutState)
				{
					UpdateTimelineLayoutState(timelineLayoutState);
				}
			}
			else
			{
				UpdateTimelineLayoutState(null);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			try
			{
				base.OnMouseDown(e);
				if (_isDroppedDown && !base.ClientRectangle.Contains(e.X, e.Y))
				{
					TimelineUtilities.SetControlCapture(this, false);
					UnDrop(cancel: true);
					return;
				}
				for (int i = 0; i < 2; i++)
				{
					if (_rectangleArray[i].Contains(e.X, e.Y))
					{
						_ = _scrollChange;
						if (i == 0)
						{
							_monthScrollStep = -_scrollChange;
						}
						else
						{
							_monthScrollStep = _scrollChange;
						}
						SetFirstVisibleMonth(_firstVisibleMonth.AddMonths(_monthScrollStep));
						_scrollTimer.Interval = SystemInformation.DoubleClickTime;
						_scrollTimer.Enabled = true;
						return;
					}
				}
				TimelineLayoutState timelineLayoutState = FindMonthByHeaderAt(e.X, e.Y, true);
				if (timelineLayoutState != null)
				{
					UpdateCalendarTimelineLayoutState(timelineLayoutState);
					Update();
					_clickedMonthIndex = timelineLayoutState.GetMonthIndex();
					Rectangle rectangle = timelineLayoutState.GetHeaderBounds();
					rectangle.Width -= _borderSize.Width / 2;
					rectangle.Inflate(-12, -1);
					Point location = rectangle.Location;
					location = PointToScreen(location);
					_calendarControl.BackColor = GetEffectiveBackColor();
					_calendarControl.ForeColor = GetEffectiveForeColor();
					_calendarControl.Font = Font;
					_calendarControl.SetHighlightedMonth(new DateTime(timelineLayoutState.GetYear(), timelineLayoutState.GetMonth(), 1));
					_calendarControl._timelineFormatStyle = _selectedFormatStyle;
					_calendarControl._borderColor = new TimelineThemeRenderer()._controlBorderColor;
					_calendarControl.Drop(location, rectangle.Size, (DateTime)_calendarControl.GetHighlightedMonth());
				}
				else
				{
					TimelineCalendarDay dayCell = GetDayCellAt(e.X, e.Y, true);
					if (dayCell != null && dayCell.IsInRange())
					{
						CurrentDate = dayCell.GetDate();
					}
					SetPressedButton(GetButtonAt(e.X, e.Y));
				}
			}
			catch
			{
			}
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Down || keyData == Keys.Up || keyData == Keys.Left || keyData == Keys.Right)
			{
				return true;
			}
			return base.IsInputKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled)
			{
				switch (e.KeyData)
				{
				case Keys.Left:
					CurrentDate = _currentDate.AddDays(-1.0);
					break;
				case Keys.Right:
					CurrentDate = _currentDate.AddDays(1.0);
					break;
				case Keys.Up:
					CurrentDate = _currentDate.AddDays(-7.0);
					break;
				case Keys.Down:
					CurrentDate = _currentDate.AddDays(7.0);
					break;
				}
			}
		}

		internal bool HandleDialogKey(Keys keyData)
		{
			return ProcessDialogKey(keyData);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Prior:
				CurrentDate = _currentDate.AddMonths(-1);
				break;
			case Keys.Next:
				CurrentDate = _currentDate.AddMonths(1);
				break;
			case Keys.Down:
				CurrentDate = _currentDate.AddDays(7.0);
				break;
			case Keys.Up:
				CurrentDate = _currentDate.AddDays(-7.0);
				break;
			case Keys.Right:
				CurrentDate = _currentDate.AddDays(1.0);
				break;
			case Keys.Left:
				CurrentDate = _currentDate.AddDays(-1.0);
				break;
			case Keys.Return:
			case Keys.F4:
			case Keys.Up | Keys.Alt:
			case Keys.Down | Keys.Alt:
				if (_calendarControl != null)
				{
					_calendarControl.UnDrop(cancel: false);
				}
				UnDrop(cancel: false);
				break;
			case Keys.Escape:
				if (_calendarControl != null)
				{
					_calendarControl.UnDrop(cancel: false);
				}
				UnDrop(cancel: true);
				break;
			default:
				return base.ProcessDialogKey(keyData);
			}
			return true;
		}
	}
}
