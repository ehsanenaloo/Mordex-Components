using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerAllDayArea : Control
	{
		private IContainer _iContainer;

		internal bool _isLayoutStale = true;

		internal bool _hasHiddenAppointments;

		private MxScheduler _scheduler;

		private Timer _buttonRepeatTimer;

		internal int _rowHeight;

		private DateCollection _dates;

		internal bool _isMultiOwner;

		private int _leftPanelWidth;

		private SchedulerDayOfWeek _workWeek;

		private SchedulerDayOfWeek _startDay;

		internal int _columnWidthWithGap;

		internal int _columnCount;

		private Rectangle _previousButtonBounds = Rectangle.Empty;

		private Rectangle _schedulerControl2Rectangle = Rectangle.Empty;

		private ScrollBarStyle _scrollBarStyle;

		internal SchedulerHourRangeLayoutCollection _dayCells = new SchedulerHourRangeLayoutCollection();

		internal SchedulerOwnerLayoutCollection _dateHeaders;

		internal SchedulerOwnerLayoutCollection _ownerHeaders;

		private SchedulerHiddenAppointmentButton _hiddenAppointmentsIndicator;

		private bool _shouldUseLongDate = true;

		private Rectangle _dayAreaBounds;

		private StyledScrollBar _appointmentScrollBar;

		private Rectangle _contentBounds;

		private int _headerHeight;

		private bool _hasDayNavigationButtons;

		private bool _isPreviousButtonPressed;

		private bool _isNextButtonPressed;

		private bool _isNextButtonHovered;

		private SchedulerAppointmentOwnerLayoutState _schedulerAppointmentOwnerLayoutState;

		private bool _isPreviousButtonHovered;

		private Point _lastMouseUpPoint = Point.Empty;

		private Brush _allDayAreaBrush;

		internal SchedulerAllDayArea(MxScheduler scheduler, DateCollection dates, int rowHeight, int leftDayPanel, SchedulerDayOfWeek workWeek, SchedulerDayOfWeek startDay)
		{
			SetDateCollection(dates);
			_rowHeight = rowHeight;
			SetLeftPanelWidth(leftDayPanel);
			_workWeek = workWeek;
			_startDay = startDay;
			_scheduler = scheduler;
			_columnCount = GetDateCollection().Count;
			UpdateDayNavigationButtons(scheduler.DayNavigationButtons);
			_scrollBarStyle = scheduler.ScrollBarStyle;
			InitializeTimer();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			BackColor = _scheduler.GetThemedBackColor();
		}

		protected override void OnDoubleClick(EventArgs eventArgs)
		{
			if (!_previousButtonBounds.Contains(_lastMouseUpPoint) && !_schedulerControl2Rectangle.Contains(_lastMouseUpPoint))
			{
				base.OnDoubleClick(eventArgs);
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			Point point = new Point(eventArgs.X, eventArgs.Y);
			if (_scheduler.HeaderAppearance == Appearance.PopUp || _scheduler.HeaderAppearance == Appearance.FlatBorderless)
			{
				SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = FindHeaderAt(point);
				UpdateSchedulerAppointmentOwnerLayoutState(schedulerAppointmentOwnerLayoutState);
			}
			if (eventArgs.Button == MouseButtons.Left)
			{
				if (HasDayNavigationButtons() && UpdateNavigationButtonsAtPoint(point, true, true))
				{
					return;
				}
				if (_dayCells != null && _appointmentScrollBar != null)
				{
					foreach (SchedulerHourRangeCollectionLayoutState item in _dayCells)
					{
						if (item._hiddenBelowButton != null && item._hiddenBelowButton._bounds.Contains(eventArgs.X, eventArgs.Y))
						{
							if (_scheduler.HiddenAppointmentMode == HiddenAppointmentMode.Manual)
							{
								_scheduler.RaiseHiddenAppointmentClick(item.GetDate());
							}
							else
							{
								_appointmentScrollBar.UpdateScrollValue(_appointmentScrollBar.GetMaximum());
							}
							return;
						}
						if (item._hiddenAboveButton != null && item._hiddenAboveButton._bounds.Contains(eventArgs.X, eventArgs.Y))
						{
							if (_scheduler.HiddenAppointmentMode == HiddenAppointmentMode.Manual)
							{
								_scheduler.RaiseHiddenAppointmentClick(item.GetDate());
							}
							else
							{
								_appointmentScrollBar.UpdateScrollValue(_appointmentScrollBar.GetMinimum());
							}
							return;
						}
					}
				}
			}
			base.OnMouseDown(eventArgs);
		}

		private void InitializeTimer()
		{
			_iContainer = new Container();
			_buttonRepeatTimer = new Timer(_iContainer);
			_buttonRepeatTimer.Interval = 150;
			_buttonRepeatTimer.Tick += OnTimerTick;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		internal DateCollection GetDateCollection()
		{
			return _dates;
		}

		internal void SetDateCollection(DateCollection dateCollection)
		{
			if (_dates != dateCollection)
			{
				_dates = dateCollection;
				_isLayoutStale = true;
			}
		}

		internal int GetLeftPanelWidth()
		{
			return _leftPanelWidth;
		}

		internal void SetLeftPanelWidth(int leftPanelWidth)
		{
			_leftPanelWidth = leftPanelWidth;
		}

		private void DrawBackground(Graphics graphics, Rectangle rect)
		{
			SchedulerFormatStyle schedulerFormatStyle = null;
			if (!_scheduler.ContainsFocus && _scheduler.HideSelection != HideSelection.Highlight)
			{
				if (_scheduler.HideSelection == HideSelection.HighlightInactive)
				{
					schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.SelectedInactiveFormatStyle);
				}
			}
			else
			{
				schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.SelectedFormatStyle);
			}
			if (schedulerFormatStyle != null)
			{
				Brush brush = schedulerFormatStyle.GetBackBrush(rect);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rect);
					brush.Dispose();
				}
			}
		}

		private void DrawHeaders(Graphics graphics)
		{
			Region region = new Region(_dayAreaBounds);
			Region clip = graphics.Clip;
			graphics.Clip = region;
			if (_columnCount == 0)
			{
				_columnCount = 1;
			}
			if (_columnCount > _scheduler.DayColumns && _scheduler.DayColumns > 0)
			{
				_columnCount = _scheduler.DayColumns;
			}
			_ = (_contentBounds.Width - GetLeftPanelWidth() + 1) % _columnCount;
			GetLeftPanelWidth();
			int ownerRowTop = _contentBounds.Top;
			if (_isMultiOwner && _dates.Count > 1 && _scheduler.ShowOwnersHeader)
			{
				ownerRowTop += _rowHeight;
			}
			new Rectangle(GetLeftPanelWidth() + 1, ownerRowTop, _contentBounds.Width - GetLeftPanelWidth() - 2, _rowHeight);
			int separatorMinX = _scheduler._dayAreaLeft - 5 - _contentBounds.Left;
			int separatorMaxX = _contentBounds.Right;
			if (_hasDayNavigationButtons)
			{
				separatorMinX += _rowHeight;
				separatorMaxX -= _rowHeight;
			}
			Pen pen = GdiCache.Pen(SchedulerThemeColors.GetDefaultBorderColor());
			bool isMultiDaySelection = _scheduler.SelectedDays.End.Subtract(_scheduler.SelectedDays.Start).TotalDays >= 1.0;
			for (int i = 0; i < _dateHeaders.Count; i++)
			{
				SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = _dateHeaders.GetLayoutAt(i);
				DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.DateHeader, schedulerAppointmentOwnerLayoutState._bounds, schedulerAppointmentOwnerLayoutState);
				_scheduler.RaiseDrawSchedulerArea(e);
				if (e.Handled)
				{
					continue;
				}
				DrawHeaderCell(graphics, schedulerAppointmentOwnerLayoutState, schedulerAppointmentOwnerLayoutState.IsHovered(), true);
				if (_isMultiOwner && _scheduler.ShowOwnersHeader)
				{
					continue;
				}
				if (isMultiDaySelection && _scheduler.SelectedDays.Start.Date <= schedulerAppointmentOwnerLayoutState.GetDate() && _scheduler.SelectedDays.End > schedulerAppointmentOwnerLayoutState.GetDate())
				{
					Rectangle rectangle = new Rectangle(schedulerAppointmentOwnerLayoutState._bounds.X + 1, schedulerAppointmentOwnerLayoutState._bounds.Bottom, schedulerAppointmentOwnerLayoutState._bounds.Width - 2, base.Bounds.Bottom - schedulerAppointmentOwnerLayoutState._bounds.Bottom);
					DrawBackground(graphics, rectangle);
				}
				if (schedulerAppointmentOwnerLayoutState._bounds.Right < separatorMaxX && schedulerAppointmentOwnerLayoutState._bounds.Right > separatorMinX)
				{
					graphics.DrawLine(pen, schedulerAppointmentOwnerLayoutState._bounds.Right, schedulerAppointmentOwnerLayoutState._bounds.Bottom + 3, schedulerAppointmentOwnerLayoutState._bounds.Right, base.Bounds.Bottom - 7);
				}
			}
			if (_isMultiOwner && _scheduler.ShowOwnersHeader)
			{
				ownerRowTop = _contentBounds.Top + _rowHeight;
				int appointmentAreaTop = ownerRowTop + _rowHeight;
				separatorMinX = _scheduler._dayAreaLeft - _scheduler._columnGap - _contentBounds.Left;
				new Rectangle(GetLeftPanelWidth() + 1, ownerRowTop, _contentBounds.Width - GetLeftPanelWidth() - 2, _rowHeight);
				foreach (SchedulerAppointmentOwnerLayoutState item in _ownerHeaders)
				{
					DrawSchedulerAreaEventArgs ownerEventArgs = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.OwnerHeader, item._bounds, item);
					_scheduler.RaiseDrawSchedulerArea(ownerEventArgs);
					if (ownerEventArgs.Handled)
					{
						continue;
					}
					DrawHeaderCell(graphics, item, false, false);
					if (isMultiDaySelection && _scheduler.SelectedDays.Start.Date <= item.GetDate() && _scheduler.SelectedDays.End > item.GetDate() && _scheduler.CurrentOwner == item.ResolveOwner())
					{
						Rectangle ownerColumnBounds = new Rectangle(item._bounds.X + 1, item._bounds.Bottom, item._bounds.Width - 2, base.Bounds.Bottom - item._bounds.Bottom);
						DrawBackground(graphics, ownerColumnBounds);
					}
					if (item._bounds.Right < _contentBounds.Right && item._bounds.Right > separatorMinX)
					{
						graphics.DrawLine(pen, item._bounds.Right, appointmentAreaTop + 3, item._bounds.Right, base.Bounds.Bottom - 7);
					}
				}
			}
			if (_hasDayNavigationButtons)
			{
				RenderContent(graphics, -1, _previousButtonBounds, _isPreviousButtonPressed, _isPreviousButtonHovered);
				RenderContent(graphics, 1, _schedulerControl2Rectangle, _isNextButtonPressed, _isNextButtonHovered);
			}
			region.Dispose();
			region = null;
			graphics.Clip = clip;
		}

		private void DisposePaintResources()
		{
			if (_allDayAreaBrush != null)
			{
				_allDayAreaBrush.Dispose();
				_allDayAreaBrush = null;
			}
			_scrollBarStyle.DisposeAllResources();
			if (_scheduler != null)
			{
				_scheduler.DisposeDrawingResources();
			}
		}

		public bool HasDayNavigationButtons()
		{
			return _hasDayNavigationButtons;
		}

		public void UpdateDayNavigationButtons(bool hasDayNavigationButtons)
		{
			if (_hasDayNavigationButtons != hasDayNavigationButtons)
			{
				_hasDayNavigationButtons = hasDayNavigationButtons;
				_isLayoutStale = true;
				Invalidate();
			}
		}

		private void UpdateColumnCount()
		{
			if (_isMultiOwner)
			{
				_columnCount = _scheduler._visibleOwners.Count * _dates.Count;
			}
			else
			{
				_columnCount = _dates.Count;
			}
			if (_columnCount > _scheduler.DayColumns && _scheduler.DayColumns > 0)
			{
				_columnCount = _scheduler.DayColumns;
			}
		}

		internal void RemoveScrollBar()
		{
			if (_appointmentScrollBar != null)
			{
				_appointmentScrollBar.Dispose();
				_appointmentScrollBar = null;
			}
		}

		private void OnAppointmentScrollBarChanged(object sender, EventArgs eventArgs)
		{
			_scheduler._allDayScrollOffset = _appointmentScrollBar.GetScrollValue();
			_scheduler.InvalidateAppointmentCacheAndLayout();
		}

		internal void EnsureScrollBar(Rectangle rectangle)
		{
			if (_appointmentScrollBar == null)
			{
				_appointmentScrollBar = new StyledScrollBar(ScrollBarType.Vertical);
				_appointmentScrollBar.Bounds = rectangle;
				_appointmentScrollBar.AddValueChangedHandler(OnAppointmentScrollBarChanged);
				base.Controls.Add(_appointmentScrollBar);
			}
			else
			{
				_appointmentScrollBar.Bounds = rectangle;
			}
			_appointmentScrollBar._scrollBarStyle = _scheduler.ScrollBarStyle;
		}

		internal void LayoutHeaders(Graphics graphics)
		{
			int scrollBarWidth = 0;
			if (_scheduler.ShouldShowAllDayAreaScrollBar())
			{
				scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
			}
			int top = base.ClientRectangle.Top;
			_headerHeight = _rowHeight;
			int ownerRowTop = base.ClientRectangle.Top;
			int dayAreaLeft = _scheduler._dayAreaLeft - _scheduler.ClientRectangle.Left - _scheduler._columnGap;
			int contentWidth = base.ClientRectangle.Width - scrollBarWidth;
			_contentBounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, contentWidth, base.ClientRectangle.Height);
			_dayAreaBounds = new Rectangle(dayAreaLeft, _contentBounds.Top, _contentBounds.Width - dayAreaLeft, _contentBounds.Height);
			UpdateColumnCount();
			if (_isMultiOwner && _scheduler.ShowOwnersHeader)
			{
				ownerRowTop += _rowHeight;
				_headerHeight += _rowHeight;
			}
			int y = _headerHeight + _contentBounds.Top;
			_ownerHeaders = null;
			_dateHeaders = new SchedulerOwnerLayoutCollection();
			int cellLeft = GetLeftPanelWidth();
			int height = _contentBounds.Height - _headerHeight;
			int timePanelRight = _scheduler._dayAreaLeft - _scheduler._columnGap - _scheduler.ClientRectangle.Left;
			int spareWidth = contentWidth - (timePanelRight + _columnWidthWithGap * _columnCount);
			int columnWidth = _columnWidthWithGap;
			int cellNumber = 1;
			_dayCells.Clear();
			int count = _scheduler._visibleOwners.Count;
			int cellExtraWidth = 0;
			if (_isMultiOwner)
			{
				foreach (DateTime item in GetDateCollection())
				{
					cellExtraWidth = ((cellLeft + columnWidth * count + columnWidth > _contentBounds.Left + contentWidth - spareWidth) ? (cellExtraWidth + spareWidth) : 0);
					SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(item);
					schedulerHourRangeCollectionLayoutState._bounds = new Rectangle(cellLeft, y, columnWidth * count + cellExtraWidth, height);
					Rectangle rect = new Rectangle(cellLeft, top, columnWidth * count + cellExtraWidth, _rowHeight);
					_dayCells.AddLayout(schedulerHourRangeCollectionLayoutState);
					cellLeft += columnWidth * count + cellExtraWidth;
					string empty = string.Empty;
					SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = new SchedulerAppointmentOwnerLayoutState(rect, empty);
					schedulerAppointmentOwnerLayoutState.UpdateDate(item);
					_dateHeaders.Add(schedulerAppointmentOwnerLayoutState);
					schedulerHourRangeCollectionLayoutState._schedulerAppointmentOwnerLayoutState = schedulerAppointmentOwnerLayoutState;
					cellNumber++;
				}
			}
			else
			{
				foreach (DateTime columnDate in GetDateCollection())
				{
					spareWidth = ((_contentBounds.Right - cellLeft - columnWidth < _columnCount && _contentBounds.Left + contentWidth - cellLeft - columnWidth > 0) ? (_contentBounds.Left + contentWidth - cellLeft - columnWidth) : 0);
					SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = new SchedulerHourRangeCollectionLayoutState(columnDate);
					schedulerHourRangeCollectionLayoutStateArgument._bounds = new Rectangle(cellLeft, y, columnWidth + spareWidth, height);
					Rectangle headerCellRect = new Rectangle(cellLeft, top, columnWidth + spareWidth, _rowHeight);
					_dayCells.AddLayout(schedulerHourRangeCollectionLayoutStateArgument);
					cellLeft += columnWidth + spareWidth;
					string emptyText = string.Empty;
					SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutStateArgument = new SchedulerAppointmentOwnerLayoutState(headerCellRect, emptyText);
					schedulerAppointmentOwnerLayoutStateArgument.UpdateDate(columnDate);
					_dateHeaders.Add(schedulerAppointmentOwnerLayoutStateArgument);
					schedulerHourRangeCollectionLayoutStateArgument._schedulerAppointmentOwnerLayoutState = schedulerAppointmentOwnerLayoutStateArgument;
					cellNumber++;
				}
			}
			if (_hasDayNavigationButtons)
			{
				SchedulerAppointmentOwnerLayoutState firstDateHeader = FindHeaderAt(new Point(timePanelRight + 2, top + 2));
				SchedulerAppointmentOwnerLayoutState lastDateHeader = FindHeaderAt(new Point(_contentBounds.Left + contentWidth - 2, top + 2));
				if (firstDateHeader != null && lastDateHeader != null)
				{
					Rectangle bounds = firstDateHeader._bounds;
					Rectangle rectangle = new Rectangle(_contentBounds.Left + contentWidth - _rowHeight, top, _rowHeight, _rowHeight);
					bounds.X = timePanelRight;
					bounds.Height = _rowHeight;
					_previousButtonBounds = bounds;
					_schedulerControl2Rectangle = rectangle;
					_previousButtonBounds.Width = _previousButtonBounds.Height;
					bounds.Width -= bounds.Height;
					bounds.X += bounds.Height;
					firstDateHeader._bounds.X += bounds.Height;
					firstDateHeader._bounds.Width -= bounds.Height;
					lastDateHeader._bounds.Width -= rectangle.Height;
				}
			}
			else
			{
				_previousButtonBounds = Rectangle.Empty;
				_schedulerControl2Rectangle = Rectangle.Empty;
			}
			if (_isMultiOwner)
			{
				_ownerHeaders = new SchedulerOwnerLayoutCollection();
				ArrayList owners = _scheduler._visibleOwners;
				cellNumber = 1;
				cellLeft = GetLeftPanelWidth();
				columnWidth = _columnWidthWithGap;
				foreach (DateTime ownerColumnDate in GetDateCollection())
				{
					columnWidth = _columnWidthWithGap;
					foreach (SchedulerAppointmentOwner owner in owners)
					{
						spareWidth = ((_contentBounds.Right - cellLeft - columnWidth < _columnCount && _contentBounds.Left + contentWidth - cellLeft - columnWidth > 0) ? ((_columnCount <= columnWidth || cellNumber >= _columnCount) ? (_contentBounds.Left + contentWidth - cellLeft - columnWidth) : 0) : 0);
						Rectangle ownerCellRect = new Rectangle(cellLeft, ownerRowTop, columnWidth + spareWidth, _rowHeight);
						cellLeft += columnWidth + spareWidth;
						SchedulerAppointmentOwnerLayoutState ownerHeader = new SchedulerAppointmentOwnerLayoutState(ownerCellRect, owner.Text);
						ownerHeader.SetImageIndex(owner.ImageIndex);
						ownerHeader.UpdateDate(ownerColumnDate);
						ownerHeader.SetSchedulerAppointmentOwner(owner);
						_ownerHeaders.Add(ownerHeader);
						cellNumber++;
					}
				}
			}
			bool hasCustomDateFormat = true;
			if (_scheduler.CustomDateFormat == "")
			{
				ChooseDateFormat(graphics);
				hasCustomDateFormat = false;
			}
			if (_scheduler.ShouldShowAllDayAreaScrollBar())
			{
				Rectangle scrollBarBounds = new Rectangle(_contentBounds.Right, _contentBounds.Top, scrollBarWidth, _contentBounds.Height);
				EnsureScrollBar(scrollBarBounds);
				int visibleRowCount = (_contentBounds.Height - _headerHeight) / (_rowHeight + _scheduler._allDayRowGap);
				if (_scheduler.MaxAllDayRows > 0)
				{
					int maxRowCount = _scheduler.MaxAllDayRows;
					if (_scheduler.AddNewMode == AddNewMode.Automatic)
					{
						maxRowCount++;
					}
					if (visibleRowCount > maxRowCount)
					{
						visibleRowCount = maxRowCount;
					}
				}
				ConfigureScrollBar(0, _scheduler._allDayRowCount - 1, visibleRowCount, _scheduler._allDayScrollOffset);
			}
			else
			{
				RemoveScrollBar();
			}
			foreach (SchedulerAppointmentOwnerLayoutState dateHeader in _dateHeaders)
			{
				if (hasCustomDateFormat)
				{
					try
					{
						dateHeader.SetHeaderText(dateHeader.GetDate().Date.ToString(_scheduler.CustomDateFormat, Application.CurrentCulture));
					}
					catch
					{
						dateHeader.SetHeaderText("");
					}
				}
				else if (_shouldUseLongDate)
				{
					dateHeader.SetHeaderText(dateHeader.GetDate().Date.ToLongDateString());
				}
				else
				{
					dateHeader.SetHeaderText(dateHeader.GetDate().Date.ToShortDateString());
				}
			}
			foreach (SchedulerHourRangeCollectionLayoutState sourceCell in _scheduler._dayLayouts)
			{
				SchedulerHourRangeCollectionLayoutState targetCell = _dayCells.FindLayout(sourceCell.GetDate(), sourceCell.GetOwner());
				if (targetCell != null)
				{
					if (sourceCell.GetHasAllDayRowsAbove())
					{
						targetCell._hiddenAboveButton = new SchedulerAppointmentOwnerLayoutState(bounds: new Rectangle(targetCell._bounds.Right - 16, targetCell._bounds.Top + 2, 13, 13), date: sourceCell.GetDate());
					}
					if (sourceCell.GetHasAllDayRowsBelow())
					{
						targetCell._hiddenBelowButton = new SchedulerAppointmentOwnerLayoutState(bounds: new Rectangle(targetCell._bounds.Right - 16, targetCell._bounds.Bottom - 15, 13, 13), date: sourceCell.GetDate());
					}
				}
			}
			_isLayoutStale = false;
		}

		internal void ScrollAppointmentIntoView(SchedulerAppointment schedulerAppointment)
		{
			if (_scheduler.MaxAllDayRows <= 0 || _appointmentScrollBar == null || _scheduler == null || _scheduler._allDayAppointmentViews == null)
			{
				return;
			}
			int appointmentIndex = 0;
			foreach (SchedulerAppointmentView item in _scheduler._allDayAppointmentViews)
			{
				if (item.GetAppointment() != schedulerAppointment)
				{
					appointmentIndex++;
					continue;
				}
				break;
			}
			if (appointmentIndex >= _appointmentScrollBar.GetScrollValue() + _appointmentScrollBar.GetLargeChange() - 1 || appointmentIndex < _appointmentScrollBar.GetScrollValue())
			{
				_appointmentScrollBar.UpdateScrollValue(appointmentIndex);
			}
		}

		internal void ConfigureScrollBar(int minimum, int integerArgument, int largeChange, int value)
		{
			try
			{
				_appointmentScrollBar.UpdateMinimum(minimum);
				_appointmentScrollBar.UpdateMaximum(integerArgument);
				if (largeChange > 0)
				{
					_appointmentScrollBar.UpdateLargeChange(largeChange);
				}
				else
				{
					_appointmentScrollBar.UpdateLargeChange(1);
				}
				_appointmentScrollBar.UpdateScrollValue(value);
				if (largeChange > integerArgument)
				{
					_appointmentScrollBar.Enabled = false;
				}
				else
				{
					_appointmentScrollBar.Enabled = true;
				}
			}
			catch
			{
			}
		}

		private void ChooseDateFormat(Graphics graphics)
		{
			_shouldUseLongDate = true;
			int dayAreaLeft = _scheduler._dayAreaLeft - _scheduler._columnGap - _scheduler.ClientRectangle.Left;
			int availableWidth = 0;
			foreach (SchedulerAppointmentOwnerLayoutState item in _dateHeaders)
			{
				string text = item.GetDate().Date.ToLongDateString();
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_scheduler.UseCompatibleTextRendering);
				int textWidth = (int)textLayoutRenderer.RenderContent(graphics, text, Font, 0).Width;
				textLayoutRenderer.ResetFormat();
				availableWidth = item._bounds.Width;
				if (item._bounds.X < dayAreaLeft)
				{
					availableWidth = item._bounds.Right - dayAreaLeft;
					if (_hasDayNavigationButtons)
					{
						availableWidth -= _rowHeight;
					}
				}
				if (item._bounds.Right > _contentBounds.Right)
				{
					availableWidth = _contentBounds.Right - item._bounds.X;
					if (_hasDayNavigationButtons)
					{
						availableWidth -= _rowHeight;
					}
				}
				if (textWidth >= availableWidth && availableWidth > 0)
				{
					_shouldUseLongDate = false;
					break;
				}
			}
		}

		internal void DrawAreaBackground(Graphics graphics, Rectangle rectangleArgument)
		{
			Rectangle rect = Rectangle.Intersect(base.ClientRectangle, rectangleArgument);
			Brush brush = GdiCache.Brush(_scheduler.GetThemedBackColor());
			graphics.FillRectangle(brush, rect);
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.TimeNavigatorFormatStyle);
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Width = _dayAreaBounds.Left + 1;
			clientRectangle.Height = base.ClientRectangle.Height + _scheduler.ClientRectangle.Height;
			_scheduler.DrawThemedTimeNavigatorBackground(graphics, clientRectangle);
			Rectangle rectangle = _dayAreaBounds;
			rectangle.Y += _rowHeight;
			rectangle.Height -= _rowHeight;
			if (_isMultiOwner && _scheduler.ShowOwnersHeader)
			{
				rectangle.Y += _rowHeight;
				rectangle.Height -= _rowHeight;
			}
			_scheduler.DrawAllDayAreaBackground(graphics, rectangle);
			schedulerFormatStyle.DisposeResources();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			if (_scheduler == null)
			{
				return;
			}
			_scheduler.RefreshLayoutIfDirty(eventArgs.Graphics);
			if (_isLayoutStale)
			{
				LayoutHeaders(eventArgs.Graphics);
			}
			DrawSchedulerAreaEventArgs allDayArgs = new DrawSchedulerAreaEventArgs(eventArgs.Graphics, _scheduler, OwnerDrawnArea.AllDayArea, _contentBounds, null);
			_scheduler.RaiseDrawSchedulerArea(allDayArgs);
			if (!allDayArgs.Handled)
			{
				DrawAreaBackground(eventArgs.Graphics, eventArgs.ClipRectangle);
			}
			DrawHeaders(eventArgs.Graphics);
			DrawAppointments(eventArgs.Graphics, eventArgs.ClipRectangle);
			if (_hasHiddenAppointments)
			{
				if (_hiddenAppointmentsIndicator == null)
				{
					_hiddenAppointmentsIndicator = new SchedulerHiddenAppointmentButton(_scheduler, _dates[0], -1, HiddenAppointmentMode.Manual);
					base.Controls.Add(_hiddenAppointmentsIndicator);
				}
				_hiddenAppointmentsIndicator.Bounds = new Rectangle(_contentBounds.Right - 24, _contentBounds.Height - 12, 20, 8);
			}
			else if (_hiddenAppointmentsIndicator != null)
			{
				_hiddenAppointmentsIndicator.Dispose();
				_hiddenAppointmentsIndicator = null;
			}
			DisposePaintResources();
		}

		private void DrawAppointments(Graphics graphics, Rectangle rectangle)
		{
			foreach (SchedulerAppointmentView item in _scheduler._allDayAppointmentViews)
			{
				if (!item._isShown || item._bounds.Bottom > _contentBounds.Bottom || item._bounds.Top < _contentBounds.Top + _headerHeight)
				{
					if (item._isShown)
					{
						item._isShown = false;
					}
					_hasHiddenAppointments = true;
					item._bounds = Rectangle.Empty;
				}
				else if (!Rectangle.Intersect(rectangle, item._bounds).IsEmpty && item._bounds.Left >= _dayAreaBounds.Left)
				{
					DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.Appointments, item._bounds, item.GetAppointment());
					_scheduler.RaiseDrawSchedulerArea(e);
					if (!e.Handled)
					{
						item.DrawAppointment(graphics);
					}
				}
			}
		}

		private void ShowPreviousDays()
		{
			_scheduler.MovePrevious();
			_isLayoutStale = true;
		}

		private void ShowNextDays()
		{
			_scheduler.MoveNext();
			_isLayoutStale = true;
		}

		public void SetPreviousButtonPressed(bool isPressed)
		{
			if (_isPreviousButtonPressed != isPressed)
			{
				_isPreviousButtonPressed = isPressed;
				Invalidate(_previousButtonBounds);
				if (isPressed)
				{
					ShowPreviousDays();
					_buttonRepeatTimer.Interval = SystemInformation.DoubleClickTime;
					_buttonRepeatTimer.Start();
				}
				else
				{
					_buttonRepeatTimer.Stop();
				}
			}
		}

		public void SetNextButtonPressed(bool isPressed)
		{
			if (_isNextButtonPressed != isPressed)
			{
				_isNextButtonPressed = isPressed;
				Invalidate(_schedulerControl2Rectangle);
				if (!_buttonRepeatTimer.Enabled && isPressed)
				{
					ShowNextDays();
					_buttonRepeatTimer.Interval = SystemInformation.DoubleClickTime;
					_buttonRepeatTimer.Start();
				}
				else if (_buttonRepeatTimer.Enabled && !isPressed)
				{
					_buttonRepeatTimer.Stop();
				}
			}
		}

		public void SetNextButtonHovered(bool isHovered)
		{
			if (_isNextButtonHovered != isHovered)
			{
				_isNextButtonHovered = isHovered;
				Invalidate(_schedulerControl2Rectangle);
			}
		}

		public void UpdateSchedulerAppointmentOwnerLayoutState(SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState)
		{
			if (schedulerAppointmentOwnerLayoutState != _schedulerAppointmentOwnerLayoutState)
			{
				if (_schedulerAppointmentOwnerLayoutState != null)
				{
					_schedulerAppointmentOwnerLayoutState.SetHovered(false);
				}
				_schedulerAppointmentOwnerLayoutState = schedulerAppointmentOwnerLayoutState;
				if (schedulerAppointmentOwnerLayoutState != null)
				{
					_schedulerAppointmentOwnerLayoutState.SetHovered(true);
				}
				Invalidate();
			}
		}

		private void DrawHeaderCell(Graphics graphics, SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState, bool isHovered, bool booleanArgument)
		{
			Rectangle bounds = schedulerAppointmentOwnerLayoutState._bounds;
			if (!schedulerAppointmentOwnerLayoutState.Equals(_previousButtonBounds) && !schedulerAppointmentOwnerLayoutState.Equals(_schedulerControl2Rectangle))
			{
				int dayAreaLeft = _scheduler._dayAreaLeft - _scheduler._columnGap - _scheduler.ClientRectangle.Left;
				if (bounds.X < dayAreaLeft && bounds.Right > dayAreaLeft)
				{
					int leftOverflow = _scheduler._dayAreaLeft - _scheduler._columnGap - bounds.X - _scheduler._borderWidth;
					bounds.X += leftOverflow;
					bounds.Width -= leftOverflow;
				}
				if (bounds.Right > _contentBounds.Right)
				{
					int visibleWidth = _contentBounds.Right - bounds.X;
					if (_hasDayNavigationButtons)
					{
						visibleWidth -= _rowHeight;
					}
					bounds.Width = visibleWidth;
				}
			}
			_scheduler.DrawDateHeader(graphics, schedulerAppointmentOwnerLayoutState, isHovered, bounds, booleanArgument);
		}

		public void SetPreviousButtonHovered(bool isHovered)
		{
			if (_isPreviousButtonHovered != isHovered)
			{
				_isPreviousButtonHovered = isHovered;
				Invalidate(_previousButtonBounds);
			}
		}

		internal SchedulerAppointmentOwnerLayoutState FindHeaderAt(Point pt)
		{
			if (_dateHeaders != null)
			{
				foreach (SchedulerAppointmentOwnerLayoutState item in _dateHeaders)
				{
					if (item._bounds.Contains(pt))
					{
						return item;
					}
				}
			}
			if (_ownerHeaders != null)
			{
				foreach (SchedulerAppointmentOwnerLayoutState ownerHeader in _ownerHeaders)
				{
					if (ownerHeader._bounds.Contains(pt))
					{
						return ownerHeader;
					}
				}
			}
			return null;
		}

		private bool UpdateNavigationButtonsAtPoint(Point pt, bool isHovered, bool booleanArgument)
		{
			if (_previousButtonBounds.Contains(pt))
			{
				SetPreviousButtonHovered(isHovered);
				SetPreviousButtonPressed(booleanArgument);
				return true;
			}
			SetPreviousButtonHovered(false);
			SetPreviousButtonPressed(false);
			if (_schedulerControl2Rectangle.Contains(pt))
			{
				SetNextButtonHovered(isHovered);
				SetNextButtonPressed(booleanArgument);
				return true;
			}
			SetNextButtonHovered(false);
			SetNextButtonPressed(false);
			return false;
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			Point point = new Point(eventArgs.X, eventArgs.Y);
			if (eventArgs.Button == MouseButtons.Left)
			{
				if (!UpdateNavigationButtonsAtPoint(point, true, true))
				{
					base.OnMouseMove(eventArgs);
				}
				return;
			}
			if (_scheduler.HeaderAppearance == Appearance.PopUp)
			{
				UpdateNavigationButtonsAtPoint(point, true, false);
			}
			UpdateNavigationButtonsAtPoint(point, true, false);
			base.OnMouseMove(eventArgs);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			SetPreviousButtonHovered(false);
			SetNextButtonHovered(false);
			SetPreviousButtonPressed(false);
			SetNextButtonPressed(false);
			UpdateSchedulerAppointmentOwnerLayoutState(null);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			SetPreviousButtonPressed(false);
			SetNextButtonPressed(false);
			_lastMouseUpPoint = new Point(eventArgs.X, eventArgs.Y);
			base.OnMouseUp(eventArgs);
		}

		private void RenderContent(Graphics graphics, int direction, Rectangle rect, bool isPressed, bool booleanArgument)
		{
			DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.ScrollDayButton, rect, null);
			_scheduler.RaiseDrawSchedulerArea(e);
			if (e.Handled)
			{
				return;
			}
			bool isPrevious = false;
			if (direction == -1)
			{
				isPrevious = true;
			}
			_scheduler.DrawNavigationArrow(graphics, rect, isPrevious, isPressed, booleanArgument);
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			_isLayoutStale = true;
			Invalidate();
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			_buttonRepeatTimer.Interval = 150;
			if (_isPreviousButtonPressed)
			{
				ShowPreviousDays();
			}
			else if (_isNextButtonPressed)
			{
				ShowNextDays();
			}
		}
	}
}
