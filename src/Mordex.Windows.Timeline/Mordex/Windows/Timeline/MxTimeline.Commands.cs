using System;
using System.Collections;
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
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimeline
	{
		private void UpdateIContainer()
		{
			_iContainer = new Container();
			new ResourceManager(typeof(MxTimeline));
			base.Size = new Size(200, 200);
		}

		private void UpdateFormatStyleColors()
		{
			_upperScaleTiersFormatStyle.ApplyThemedStyle();
			_lowerScaleTierFormatStyle.ApplyThemedStyle();
			_groupRowsFormatStyle.ApplyThemedStyle();
			_rowHeadersFormatStyle.ApplyThemedStyle();
			_selectedFormatStyle.ApplyThemedStyle();
			_selectedInactiveFormatStyle.ApplyThemedStyle();
			_itemsFormatStyle.ApplyThemedStyle();
			_itemsBarFormatStyle.ApplyThemedStyle();
			_itemsEstimatedBarFormatStyle.ApplyThemedStyle();
			_workingTimeFormatStyle.ApplyThemedStyle();
			if (_dropDownCalendar != null)
			{
				_dropDownCalendar.ApplyThemeToFormatStyles();
			}
			Invalidate();
		}

		internal void UpdateCurrencyManager(CurrencyManager currencyManager)
		{
			if (_currencyManager != currencyManager)
			{
				if (_currencyManager != null)
				{
					_currencyManager.ItemChanged -= OnCurrencyManagerItemChanged;
				}
				_currencyManager = currencyManager;
				if (_currencyManager != null)
				{
					_currencyManager.ItemChanged += OnCurrencyManagerItemChanged;
					_timelineDataSourceManager.BindableRootList = new DataBindableList(_currencyManager);
				}
				UpdateFormatConditions();
				InvalidateRows();
				InvalidateLayout();
			}
		}

		internal void ClearSelection()
		{
			SelectedItems.Clear();
			CurrentItem = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_groupRowsFormatStyle.Reset();
			_itemsBarFormatStyle.Reset();
			_itemsFormatStyle.Reset();
			_itemsEstimatedBarFormatStyle.Reset();
			_lowerScaleTierFormatStyle.Reset();
			_upperScaleTiersFormatStyle.Reset();
			_selectedFormatStyle.Reset();
			_selectedInactiveFormatStyle.Reset();
			BackgroundImage = null;
			_backgroundImageDrawMode = BackgroundImageDrawMode.Stretch;
			base.BorderStyle = BorderStyle.SunkenLight3D;
			_defaultAlpha = 255;
			_defaultAlphaMode = AlphaMode.Opaque;
			_defaultBackColorAlphaMode = AlphaMode.Empty;
			_defaultBlend = 0f;
			_defaultForeColorAlphaMode = AlphaMode.Empty;
			_defaultItemsImageIndex = -1;
			_defaultItemsImageKey = "";
			_groupIndent = 20;
			_headerAppearance = Appearance.Light3D;
			_itemsLineStyle = ItemsLineStyle.None;
			_maximumLabelLength = -1;
			BackColor = SystemColors.Window;
			_blendColor = Color.Empty;
			_showItemsDurationBar = true;
			_showItemsEstimatedDurationBar = true;
			base.FlatBorderColor = SystemColors.ControlDark;
			_showLabel = true;
			_scrollBarStyle.Reset();
			_acceptsTab = true;
			_allowEdit = true;
			_firstDayOfWeek = DayOfWeek.Sunday;
			_hideSelection = HideSelection.HighlightInactive;
			_imageSource = ImageSource.ExternalImageList;
			_itemsPrefix = string.Empty;
			_itemsSuffix = string.Empty;
			_minDate = DateTime.MinValue.AddMonths(1);
			_maxDate = DateTime.MaxValue.AddYears(-1);
			base.MouseWheelBehavior = MouseWheelBehavior.Automatic;
			base.VerticalScrollBar = VerticalScrollBar.Automatic;
			_weekRule = CalendarWeekRule.FirstDay;
			_yearText = "Year";
			_internalImageList = null;
			_externalImageList = null;
			_items = new TimelineItemCollection(this);
			_timelineDataSourceManager.RootTable = new TimelineMordexTableBase(this);
			SyncDataTableColumn(null, EditType.Clear);
			InvalidateRows();
			OnRootTableChanged(EventArgs.Empty);
			Invalidate();
		}

		internal void SetUnbound(bool isUnbound)
		{
			if (_isUnbound != isUnbound)
			{
				if (_isUnbound)
				{
					DisposeUnboundTable();
				}
				_isUnbound = isUnbound;
				if (_isUnbound)
				{
					UpdateDataTable();
				}
			}
		}

		public void RemoveFilter()
		{
			FilterCondition = null;
		}

		public void MoveFirst()
		{
			DateTime dateTime = DateTime.MaxValue;
			TimelineItem timelineItem = null;
			foreach (TimelineItem item in _items)
			{
				if (item.StartTime < dateTime)
				{
					dateTime = item.StartTime;
					timelineItem = item;
				}
			}
			SetFocusZone(FocusZone.Items);
			if (timelineItem != null)
			{
				CurrentItem = timelineItem;
			}
		}

		public void MoveLast()
		{
			DateTime dateTime = DateTime.MinValue;
			TimelineItem timelineItem = null;
			foreach (TimelineItem item in _items)
			{
				if (item.StartTime > dateTime)
				{
					dateTime = item.StartTime;
					timelineItem = item;
				}
			}
			SetFocusZone(FocusZone.Items);
			if (timelineItem != null)
			{
				CurrentItem = timelineItem;
			}
		}

		public void MovePrevious()
		{
			if (CurrentItem != null)
			{
				MovePrevious(CurrentItem);
			}
		}

		public void MovePrevious(TimelineItem item)
		{
			if (item == null)
			{
				throw new ArgumentNullException();
			}
			DateTime startTime = item.StartTime;
			DateTime dateTime = DateTime.MinValue;
			int lineIndex = item.GetSubRowIndex();
			TimelineItem timelineItem = null;
			foreach (TimelineItem candidate in _items)
			{
				DateTime candidateStart = candidate.StartTime;
				if (candidateStart <= startTime && candidateStart >= dateTime && (!(candidateStart == startTime) || (!item.Equals(candidate) && candidate.GetSubRowIndex() < lineIndex)))
				{
					dateTime = candidateStart;
					timelineItem = candidate;
				}
			}
			SetFocusZone(FocusZone.Items);
			if (timelineItem != null)
			{
				CurrentItem = timelineItem;
			}
		}

		public void MovePrevious(TimelineItem item, bool sameGroup)
		{
			if (item == null)
			{
				throw new ArgumentNullException();
			}
			if (!sameGroup && Groups.Count > 0)
			{
				MovePrevious(item);
				return;
			}
			if (Groups.Count == 0)
			{
				SelectPreviousItem((MxRowLayoutState)_rowLayouts[item.GetSubRowIndex()], item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex());
			}
			else
			{
				foreach (MxRowLayoutState rowLayout in _rowLayouts)
				{
					if (rowLayout.GetRowType() == RowType.Group)
					{
						if (rowLayout.GetGroupIndex() == item.GetGroupIndex() && rowLayout.GetRowIndex() == item.GetRowIndex() - 1)
						{
							SelectPreviousItem(rowLayout, item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex());
						}
					}
					else if (rowLayout.GetTimelineGroupRow() != null && rowLayout.GetGroupIndex() == item.GetGroupIndex() && rowLayout.GetRowIndex() == item.GetRowIndex())
					{
						SelectPreviousItemInGroup(rowLayout.GetParentRow(), item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex(), rowLayout.GetTimelineGroupRow().GetGroupRow().GroupValue);
					}
				}
			}
			SetFocusZone(FocusZone.Items);
			if (_currentItem != null)
			{
				EnsureVisible(_currentItem);
				SelectItemOnClick(_currentItem);
			}
		}

		public void MoveNext()
		{
			if (CurrentItem != null)
			{
				MoveNext(CurrentItem);
			}
		}

		public void MoveNext(TimelineItem item)
		{
			if (item == null)
			{
				throw new ArgumentNullException();
			}
			DateTime startTime = item.StartTime;
			DateTime dateTime = DateTime.MaxValue;
			TimelineItem timelineItem = null;
			foreach (TimelineItem candidate in _items)
			{
				DateTime candidateStart = candidate.StartTime;
				if (candidateStart >= startTime && candidateStart < dateTime && (!(candidateStart == startTime) || (!item.Equals(candidate) && candidate.GetSubRowIndex() > item.GetSubRowIndex())))
				{
					dateTime = candidateStart;
					timelineItem = candidate;
				}
			}
			SetFocusZone(FocusZone.Items);
			if (timelineItem != null)
			{
				CurrentItem = timelineItem;
			}
		}

		public void MoveNext(TimelineItem item, bool sameGroup)
		{
			if (item == null)
			{
				throw new ArgumentNullException();
			}
			if (!sameGroup && Groups.Count > 0)
			{
				MoveNext(item);
				return;
			}
			if (Groups.Count == 0)
			{
				SelectNextItem((MxRowLayoutState)_rowLayouts[item.GetSubRowIndex()], item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex());
			}
			else
			{
				foreach (MxRowLayoutState rowLayout in _rowLayouts)
				{
					if (rowLayout.GetRowType() == RowType.Group)
					{
						if (rowLayout.GetGroupIndex() == item.GetGroupIndex() && rowLayout.GetRowIndex() == item.GetRowIndex() - 1)
						{
							SelectNextItem(rowLayout, item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex());
						}
					}
					else if (rowLayout.GetTimelineGroupRow() != null && rowLayout.GetGroupIndex() == item.GetGroupIndex() && rowLayout.GetRowIndex() == item.GetRowIndex())
					{
						SelectNextItemInGroup(rowLayout.GetParentRow(), item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex(), rowLayout.GetTimelineGroupRow().GetGroupRow().GroupValue);
					}
				}
			}
			SetFocusZone(FocusZone.Items);
			if (_currentItem != null)
			{
				EnsureVisible(_currentItem);
				SelectItemOnClick(_currentItem);
			}
		}

		private void ShowToolTip(TimelineItem timelineItem, string caption)
		{
			Form form = null;
			try
			{
				form = FindForm();
			}
			finally
			{
			}
			if (form != null && !form.ContainsFocus)
			{
				return;
			}
			if (TimelineVisualStyleRenderer.IsAlwaysSupported())
			{
				if (_timelineWindowMessageHandler == null)
				{
					if (_timelineWindowMessageHandler == null)
					{
						_timelineWindowMessageHandler = new TimelineWindowMessageHandler(UseCompatibleTextRendering);
					}
					_timelineWindowMessageHandler.SetDisplayDuration(_toolTipDelay);
					_timelineWindowMessageHandler.SetFont(TooltipFont);
					_timelineWindowMessageHandler.ShowAtCursor(caption, this);
				}
			}
			else if (_toolTip == null)
			{
				_toolTip = new Mordex.Windows.Common.ThemedToolTip();
				_toolTip.AutomaticDelay = _toolTipDelay;
				_toolTip.ShowAlways = true;
				_toolTip.SetToolTip(this, caption);
			}
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_isInitializing = false;
				_currentLayout = _layouts.FindCurrentLayout();
				if (_currentLayout != null && _currentLayout != _timelineLayout)
				{
					_currentLayout._isCurrentLayout = true;
					_currentLayout.SetTimeline(this);
					UpdateTimelineLayout();
				}
				UpdateFormatConditions();
				InvalidateRows();
				OnLayoutLoad(EventArgs.Empty);
				AreTimescaleTiersValid(_timelineTimescaleTiers);
			}
		}

		private void UpdateFormatConditions()
		{
			foreach (TimelineFormatCondition item in _formatConditions)
			{
				item.UpdateTimelineFormatCondition();
			}
		}

		IMordexCondition ISupportFilterBuilder.CreateCondition()
		{
			return new TimelineFilterCondition();
		}
	}
}
