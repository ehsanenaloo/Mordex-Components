using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		private void OnControlMouseDoubleClick(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseDoubleClick(eventArgs);
		}

		private void OnControlMouseClick(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseClick(eventArgs);
		}

		private void OnControlDoubleClick(object sender, EventArgs eventArgs)
		{
			OnDoubleClick(eventArgs);
		}

		private void OnControlMouseDown(object sender, MouseEventArgs eventArgs)
		{
			if (!base.DesignMode)
			{
				Point position = Cursor.Position;
				position = PointToClient(position);
				MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, position.X, position.Y, eventArgs.Delta);
				OnMouseDown(e);
			}
		}

		private void OnChildControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, position.X, position.Y, eventArgs.Delta);
			OnMouseUp(e);
		}

		private void OnControlClick(object sender, EventArgs eventArgs)
		{
			OnClick(eventArgs);
		}

		private void OnControlMouseMove(object sender, MouseEventArgs eventArgs)
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, position.X, position.Y, eventArgs.Delta);
			OnMouseMove(e);
		}

		private void OnControlMouseLeave(object sender, EventArgs eventArgs)
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			if (!base.ClientRectangle.Contains(position))
			{
				OnMouseLeave(eventArgs);
			}
		}

		private void OnControlKeyDown(object sender, KeyEventArgs eventArgs)
		{
			OnKeyDown(eventArgs);
		}

		private void OnControlKeyUp(object sender, KeyEventArgs eventArgs)
		{
			OnKeyUp(eventArgs);
		}

		protected override void OnParentFontChanged(EventArgs e)
		{
			base.OnParentFontChanged(e);
			if (_dayFont == null && _view == SchedulerView.DayView)
			{
				UpdateRowHeight();
				InvalidateAppointmentLayout();
			}
			else if (_weekFont == null && _view == SchedulerView.WeekView)
			{
				UpdateRowHeight();
				InvalidateAppointmentLayout();
			}
			else if (_monthFont == null && _view == SchedulerView.MonthView)
			{
				UpdateRowHeight();
				InvalidateAppointmentLayout();
			}
		}

		protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
		{
			if (_isColumnSplitterActive)
			{
				if (base.Bounds.Contains(PointToClient(Cursor.Position)))
				{
					e.UseDefaultCursors = false;
					SetCursor(Cursors.VSplit);
				}
				else
				{
					e.UseDefaultCursors = false;
					SetCursor(null);
				}
			}
			else if (_isAppointmentDragActive)
			{
				if (_dragType != DragType.Move)
				{
					e.UseDefaultCursors = false;
					if (_view == SchedulerView.DayView)
					{
						SetCursor(Cursors.SizeNS);
					}
					else
					{
						SetCursor(Cursors.SizeWE);
					}
				}
				else
				{
					e.UseDefaultCursors = true;
				}
			}
			base.OnGiveFeedback(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			if (_view == SchedulerView.DayView)
			{
				UpdateRowHeight();
			}
			InvalidateAppointmentLayout();
		}

		private void OnControlListChanged(object sender, ListChangedEventArgs eventArgs)
		{
			if (base.InvokeRequired)
			{
				ListChangedCallback method = OnDataSourceListChanged;
				Invoke(method, eventArgs);
				Thread.Sleep(0);
			}
			else
			{
				OnDataSourceListChanged(eventArgs);
			}
		}

		internal override void OnAreaStyleChanged(EventArgs eventArgs)
		{
			_allDayRowGap = 4;
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			_currentTimeTopColor = theme.Color(ThemeRole.AccentSubtle);
			_currentTimeLineColor = theme.Color(ThemeRole.Accent);
			UpdateThemeListening();
			ApplyThemedScrollBarColors();
			UpdateCurrentTimeTimer();
			ApplyThemeToFormatStyles();
			InvalidateAppointmentLayout();
		}

		protected override void OnClientResize()
		{
			base.OnClientResize();
			if (base.Created)
			{
				if (_appointmentEditor != null)
				{
					SetEditor(null);
				}
				if (base.ClientRectangle.Width > 0 && base.ClientRectangle.Height > 0)
				{
					_dayAreaTop = 0;
					if (_view == SchedulerView.DayView)
					{
						InvalidateAppointmentLayout();
					}
					else
					{
						InvalidateAppointmentLayout();
					}
				}
			}
			Invalidate();
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			ApplyThemeToFormatStyles();
			Invalidate(invalidateChildren: true);
		}

		protected virtual void OnDrawSchedulerArea(DrawSchedulerAreaEventArgs e)
		{
			if (base.Events[_value] is DrawSchedulerEventHandler drawSchedulerEventHandler)
			{
				drawSchedulerEventHandler(this, e);
			}
		}

		// Does not build the event data when the area is not owner drawn.
		internal bool IsAreaDrawnByHandler(Graphics graphics, OwnerDrawnArea area, Rectangle bounds, object itemDrawn)
		{
			if ((_ownerDrawnArea & area) == OwnerDrawnArea.None)
			{
				return false;
			}
			DrawSchedulerAreaEventArgs eventArgs = new DrawSchedulerAreaEventArgs(graphics, this, area, bounds, itemDrawn);
			OnDrawSchedulerArea(eventArgs);
			return eventArgs.Handled;
		}

		internal void RaiseDrawSchedulerArea(DrawSchedulerAreaEventArgs eventArgs)
		{
			if ((_ownerDrawnArea & eventArgs.OwnerDrawnArea) != OwnerDrawnArea.None)
			{
				OnDrawSchedulerArea(eventArgs);
			}
		}

		protected virtual void OnGetNewObject(GetNewObjectEventArgs e)
		{
			if (base.Events[_getNewObjectEventKey] is GetNewObjectEventHandler getNewObjectEventHandler)
			{
				getNewObjectEventHandler(this, e);
			}
		}

		protected virtual void OnViewChanged(EventArgs e)
		{
			if (base.Events[_viewChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnFilterApplied(EventArgs e)
		{
			if (base.Events[_filterAppliedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnApplyingFilter(CancelEventArgs e)
		{
			if (base.Events[_applyingFilterEventKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}
	}
}
