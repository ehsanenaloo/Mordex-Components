using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.EditControls
{
	[ToolboxItem(false)]
	internal class SchedulerScrollableBase : ScrollableBase
	{
		private SchedulerFilterConditionEditor _filterEditor;

		private Panel _panel;

		private EventHandler _selectedFilterChangedHandler;

		private EventHandler _changedHandler;

		private int _horizontalOffset;

		private int _verticalOffset;

		public SchedulerScrollableBase()
		{
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			UpdateSchedulerScrollableBase();
		}

		private void UpdateSchedulerScrollableBase()
		{
			_panel = new Panel();
			_filterEditor = new SchedulerFilterConditionEditor();
			_filterEditor.Location = new Point(2, 4);
			_filterEditor.Resize += OnStateResize;
			_filterEditor.AddSelectedFilterConditionChangedHandler(OnFilterEditorSelectionChanged);
			_filterEditor.AddFilterChangedHandler(OnFilterEditorChanged);
			_filterEditor.AddActiveControlChangedHandler(OnActiveControlChanged);
			_panel.Controls.Add(_filterEditor);
			base.Controls.Add(_panel);
			_scrollBarStyle.ButtonAppearance = Appearance.Light3D;
		}

		private void OnActiveControlChanged(object sender, EventArgs eventArgs)
		{
			Control control = _filterEditor.GetEffectiveActiveControl();
			EnsureControlVisible(control);
		}

		private void EnsureControlVisible(Control control)
		{
			ScrollControlIntoView(control);
		}

		private void OnFilterEditorSelectionChanged(object sender, EventArgs eventArgs)
		{
			OnSelectedFilterChanged(eventArgs);
		}

		private void ScrollControlIntoView(Control control)
		{
			if (control != null)
			{
				Rectangle bounds = control.Bounds;
				if (control is TextBox)
				{
					bounds.Inflate(3, 2);
				}
				Point location = bounds.Location;
				location = control.Parent.PointToScreen(location);
				location = _panel.PointToClient(location);
				bounds.Location = location;
				Point clampedLocation = bounds.Location;
				if (bounds.Right > _panel.ClientRectangle.Right)
				{
					clampedLocation.X = _panel.ClientRectangle.Right - bounds.Width;
				}
				if (clampedLocation.X < 0)
				{
					clampedLocation.X = 0;
				}
				if (bounds.Bottom > _panel.ClientRectangle.Bottom)
				{
					clampedLocation.Y = _panel.ClientRectangle.Bottom - bounds.Height;
				}
				if (clampedLocation.Y < 0)
				{
					clampedLocation.Y = 0;
				}
				SetHorizontalOffset(_horizontalOffset + (bounds.X - clampedLocation.X));
				SetVerticalOffset(_verticalOffset + (bounds.Y - clampedLocation.Y));
			}
		}

		public void AddSelectedFilterChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _selectedFilterChangedHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(previousHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _selectedFilterChangedHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		protected virtual void OnSelectedFilterChanged(EventArgs eventArgs)
		{
			if (_selectedFilterChangedHandler != null)
			{
				_selectedFilterChangedHandler(this, eventArgs);
			}
		}

		private void OnFilterEditorChanged(object sender, EventArgs eventArgs)
		{
			OnChanged(eventArgs);
		}

		public void AddChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _changedHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(previousHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _changedHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		protected virtual void OnChanged(EventArgs eventArgs)
		{
			if (_changedHandler != null)
			{
				_changedHandler(this, eventArgs);
			}
		}

		private void OnStateResize(object sender, EventArgs eventArgs)
		{
			UpdateScrollRanges();
		}

		protected override void OnClientResize()
		{
			base.OnClientResize();
			if (_panel != null)
			{
				_panel.Bounds = base.ClientRectangle;
			}
			UpdateScrollRanges();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			DrawBorder(eventArgs.Graphics, null);
		}

		private Rectangle GetContentBounds()
		{
			if (_filterEditor == null)
			{
				return Rectangle.Empty;
			}
			Rectangle bounds = _filterEditor.Bounds;
			bounds.Inflate(2, 4);
			bounds.Offset(-bounds.Left, -bounds.Top);
			return bounds;
		}

		private void UpdateScrollRanges()
		{
			if (_filterEditor != null)
			{
				Rectangle rectangle = GetContentBounds();
				Rectangle clientRectangle = base.ClientRectangle;
				if (rectangle.Width > clientRectangle.Width)
				{
					SetHorizontalScrollRequired(true);
					UpdateHorizontalScrollBar(0, GetContentBounds().Width, base.ClientRectangle.Width, _horizontalOffset);
				}
				else
				{
					SetHorizontalOffset(0);
					SetHorizontalScrollRequired(false);
				}
				if (rectangle.Height > clientRectangle.Height)
				{
					SetVerticalScrollRequired(true);
					UpdateVerticalScrollBar(0, GetContentBounds().Height, base.ClientRectangle.Height, _verticalOffset);
				}
				else
				{
					SetVerticalOffset(0);
					SetVerticalScrollRequired(false);
				}
			}
		}

		private void SetHorizontalOffset(int horizontalOffset)
		{
			if (horizontalOffset < 0)
			{
				horizontalOffset = 0;
			}
			GetHorizontalScrollBarControl().UpdateScrollValue(horizontalOffset);
			_horizontalOffset = GetHorizontalScrollBarControl().GetScrollValue();
			_filterEditor.Left = -(_horizontalOffset - 2);
		}

		private void SetVerticalOffset(int verticalOffset)
		{
			if (verticalOffset < 0)
			{
				verticalOffset = 0;
			}
			GetVerticalScrollBarControl().UpdateScrollValue(verticalOffset);
			_verticalOffset = GetVerticalScrollBarControl().GetScrollValue();
			_filterEditor.Top = -(_verticalOffset - 4);
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			switch (scrollCode)
			{
			case 7:
			case 8:
				SetHorizontalOffset(GetHorizontalScrollBarControl().GetScrollValue());
				break;
			case 0:
			case 1:
				SetVerticalOffset(GetVerticalScrollBarControl().GetScrollValue());
				break;
			}
		}

		public SchedulerFilterCondition GetFilterCondition()
		{
			return _filterEditor.GetSchedulerFilterCondition();
		}

		public void SetFilterCondition(SchedulerFilterCondition schedulerFilterCondition)
		{
			_filterEditor.SetSchedulerFilterCondition(schedulerFilterCondition);
		}

		public void SetScheduler(MxScheduler scheduler)
		{
			_filterEditor.UpdateSchedule(scheduler);
		}

		public SchedulerFilterCondition GetSelectedFilterCondition()
		{
			return _filterEditor.GetSelectedFilterCondition();
		}

		public void SelectFilterCondition(SchedulerFilterCondition schedulerFilterCondition)
		{
			_filterEditor.SelectCondition(schedulerFilterCondition);
			SchedulerFilterConditionEditor selectedEditor = _filterEditor.FindEditor(schedulerFilterCondition);
			if (selectedEditor != null)
			{
				EnsureControlVisible(selectedEditor);
			}
		}
	}
}
