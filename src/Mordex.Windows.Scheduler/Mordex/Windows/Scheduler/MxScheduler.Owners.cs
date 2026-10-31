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
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		private void UpdateVisibleOwners()
		{
			_visibleOwners = new ArrayList();
			foreach (SchedulerAppointmentOwner item in _owners)
			{
				if (item.Visible)
				{
					_visibleOwners.Add(item);
				}
			}
		}

		private SchedulerAppointmentOwner FindOwnerAtPoint(int x, int integerArgument)
		{
			if (!IsMultiOwnerActive())
			{
				return null;
			}
			Rectangle empty = Rectangle.Empty;
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (_view == SchedulerView.DayView)
				{
					empty = item._bounds;
					empty.Width += empty.Left;
					empty.X = 0;
					if (empty.Contains(x, item._bounds.Top))
					{
						return item.GetOwner();
					}
				}
				else if (item._bounds.Contains(x, integerArgument))
				{
					return item.GetOwner();
				}
			}
			return null;
		}

		internal SchedulerHourRangeCollectionLayoutState FindDayCellByDateAndOwner(DateTime dateTime, SchedulerAppointmentOwner owner)
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (item.GetDate() == dateTime.Date && owner != null && item.GetOwner() != null && item.GetOwner().Equals(owner))
				{
					return item;
				}
			}
			return null;
		}

		private void ShowOwnersDropDown(Rectangle rectangle, bool capture)
		{
			_isOwnerListActive = true;
			_schedulerFormatStyleScrollableBase.UpdateSchedulerAppointmentOwner(CurrentOwner);
			Rectangle virtualScreen = SystemInformation.VirtualScreen;
			if (rectangle.X + base.Width > virtualScreen.Right)
			{
				rectangle.X = virtualScreen.Right - base.Width;
			}
			if (rectangle.X < virtualScreen.Left)
			{
				rectangle.X = SystemInformation.VirtualScreen.Left;
			}
			if (rectangle.Height > base.Height - _ownerHeaderBounds.Height)
			{
				rectangle.Height = base.Height - _ownerHeaderBounds.Height;
				rectangle.Height -= rectangle.Height % _actualRowHeight;
				rectangle.Height += _borderWidth * 2;
				((ScrollableBase)_schedulerFormatStyleScrollableBase).SetVerticalScrollRequired(true);
				int visibleItemCount = rectangle.Height / _actualRowHeight;
				_schedulerFormatStyleScrollableBase.UpdateVerticalScrollBar(0, _visibleOwners.Count - 1, visibleItemCount, 0);
			}
			else
			{
				((ScrollableBase)_schedulerFormatStyleScrollableBase).SetVerticalScrollRequired(false);
			}
			_schedulerFormatStyleScrollableBase.ShowDropList(rectangle, this, _visibleOwners, _actualRowHeight);
			SchedulerRenderer.SetCapture(_schedulerFormatStyleScrollableBase, capture);
		}

		internal string GetOwnerMemberName()
		{
			if (_usesInternalTable)
			{
				return "___OwnerMember";
			}
			return _ownerField;
		}

		internal void RebuildDayOwnerStates()
		{
			_appointmentsByOwnerDate.Clear();
			if (IsMultiOwnerActive())
			{
				foreach (SchedulerAppointmentOwner item in _visibleOwners)
				{
					if (_dates.Count > 0)
					{
						foreach (DateTime ownerDate in _dates)
						{
							SchedulerAppointmentOwnerState schedulerAppointmentOwnerState = new SchedulerAppointmentOwnerState(_publicAppointments, ownerDate, AllDay: false, item);
							_appointmentsByOwnerDate.Add(schedulerAppointmentOwnerState);
						}
					}
					else
					{
						SchedulerAppointmentOwnerState schedulerAppointmentOwnerStateArgument = new SchedulerAppointmentOwnerState(_publicAppointments, _date, AllDay: false, item);
						_appointmentsByOwnerDate.Add(schedulerAppointmentOwnerStateArgument);
					}
				}
				return;
			}
			if (_dates.Count > 0)
			{
				foreach (DateTime visibleDate in _dates)
				{
					SchedulerAppointmentOwnerState currentOwnerDay = new SchedulerAppointmentOwnerState(_publicAppointments, visibleDate, AllDay: false, _currentOwner);
					_appointmentsByOwnerDate.Add(currentOwnerDay);
				}
				return;
			}
			SchedulerAppointmentOwnerState singleDay = new SchedulerAppointmentOwnerState(_publicAppointments, _date, AllDay: false, _currentOwner);
			_appointmentsByOwnerDate.Add(singleDay);
		}

		internal bool IsMultiOwnerActive()
		{
			if (_multiOwner && _visibleOwners.Count > 0)
			{
				return true;
			}
			return false;
		}

		internal bool TrySelectAdjacentOwner()
		{
			int index = CurrentOwner.Index;
			for (int i = index + 1; i < Owners.Count; i++)
			{
				SchedulerAppointmentOwner schedulerAppointmentOwner = _owners[i];
				if (schedulerAppointmentOwner.Visible)
				{
					CurrentOwner = schedulerAppointmentOwner;
					return true;
				}
			}
			for (int previousIndex = index - 1; previousIndex >= 0; previousIndex--)
			{
				SchedulerAppointmentOwner previousOwner = _owners[previousIndex];
				if (previousOwner.Visible)
				{
					CurrentOwner = previousOwner;
					return true;
				}
			}
			return false;
		}

		public SchedulerAppointmentOwner GetOwnerAt(Point pt)
		{
			return FindOwnerAtPoint(pt.X, pt.Y);
		}

		public SchedulerAppointmentOwner GetOwnerAt()
		{
			Point pt = PointToClient(Cursor.Position);
			return GetOwnerAt(pt);
		}

		public SchedulerAppointmentOwner GetOwnerAt(int x, int y)
		{
			return FindOwnerAtPoint(x, y);
		}

		private void ShowOwnerListPopup()
		{
			if (!_showOwnerList)
			{
				return;
			}
			if (_schedulerFormatStyleScrollableBase == null)
			{
				_schedulerFormatStyleScrollableBase = new SchedulerFormatStyleScrollableBase();
				_schedulerFormatStyleScrollableBase._owner = this;
				_schedulerFormatStyleScrollableBase.Visible = false;
				base.Controls.Add(_schedulerFormatStyleScrollableBase);
			}
			_shouldIgnoreMouseSelection = true;
			_isOwnerListVisible = true;
			Graphics graphics = CreateGraphics();
			int height = _visibleOwners.Count * _actualRowHeight + _borderWidth * 2;
			int popupWidth = _ownerHeaderBounds.Width - _actualRowHeight;
			int itemWidth = 0;
			Font font = ((_view == SchedulerView.DayView) ? DayFont : ((_view != SchedulerView.WeekView) ? MonthFont : WeekFont));
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateWrapsText(false);
			bool hasOwnerImage = false;
			foreach (SchedulerAppointmentOwner item in _visibleOwners)
			{
				itemWidth = (int)textLayoutRenderer.RenderContent(graphics, item.Text, font, 0).Width + _actualRowHeight;
				if (itemWidth > popupWidth)
				{
					popupWidth = itemWidth;
					if (item.ImageIndex != -1)
					{
						hasOwnerImage = true;
					}
				}
			}
			popupWidth += 5;
			if (hasOwnerImage && GetImageList() != null)
			{
				popupWidth += GetImageList().ImageSize.Width + 5;
			}
			graphics.Dispose();
			graphics = null;
			textLayoutRenderer.ResetFormat();
			Size size = new Size(popupWidth, height);
			Point p = new Point(_ownerHeaderBounds.Left - 1, _ownerHeaderBounds.Bottom - 1);
			p = PointToScreen(p);
			_schedulerFormatStyleScrollableBase.BackColor = SystemColors.Menu;
			_schedulerFormatStyleScrollableBase.ForeColor = SystemColors.MenuText;
			_schedulerFormatStyleScrollableBase.UpdateSchedulerAppointmentOwner(CurrentOwner);
			_schedulerFormatStyleScrollableBase._schedulerFormatStyle = _selectedFormatStyle;
			Rectangle rectangle = new Rectangle(p, size);
			ShowOwnersDropDown(rectangle, true);
		}

		internal void RefreshOwners(bool raiseOwnersChanged)
		{
			_isHourRangeCacheStale = true;
			if (_schedulerOwner != null)
			{
				_schedulerOwner.CloseEditorIfOpen();
				_schedulerOwner = null;
			}
			if (_owners.Count > 0)
			{
				_hasOwners = true;
				if (CurrentOwner != null && _currentOwner._owner != _owners)
				{
					_currentOwner = null;
				}
			}
			else
			{
				_hasOwners = false;
				_currentOwner = null;
			}
			if (MultiOwner)
			{
				UpdateVisibleOwners();
				if (CurrentOwner == null && _visibleOwners.Count > 0)
				{
					CurrentOwner = (SchedulerAppointmentOwner)_visibleOwners[0];
				}
			}
			_isAppointmentCacheStale = true;
			RenderCurrentView(null);
			if (raiseOwnersChanged && Owners.Count > 0)
			{
				OnOwnersChanged(EventArgs.Empty);
			}
			Invalidate();
		}

		private void CloseOwnerList(bool isCancelled)
		{
			_isOwnerListActive = false;
			_isOwnerListVisible = false;
			_schedulerFormatStyleScrollableBase.DisposeResources();
			if (!isCancelled && _schedulerFormatStyleScrollableBase.GetSchedulerAppointmentOwner() != null)
			{
				CurrentOwner = _schedulerFormatStyleScrollableBase.GetSchedulerAppointmentOwner();
			}
			if (_schedulerFormatStyleScrollableBase != null)
			{
				_schedulerFormatStyleScrollableBase.Dispose();
				_schedulerFormatStyleScrollableBase = null;
			}
		}

		protected virtual void OnCurrentOwnerChanged(EventArgs e)
		{
			if (base.Events[_currentOwnerChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnOwnersChanged(EventArgs e)
		{
			if (base.Events[_ownersChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
