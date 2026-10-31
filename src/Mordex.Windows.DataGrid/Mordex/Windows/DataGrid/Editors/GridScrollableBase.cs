using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class GridScrollableBase : ScrollableBase
	{
		private FilterConditionEditorPanel _conditionEditor;

		private Panel _panel;

		private EventHandler _selectionChangedHandler;

		private EventHandler _conditionChangedHandler;

		private int _scrollOffsetX;

		private int _scrollOffsetY;

		internal GridScrollableBase()
		{
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			InitializeEditorPanel();
		}

		private void InitializeEditorPanel()
		{
			long ticks = DateTime.Now.Ticks;
			_panel = new Panel();
			_conditionEditor = new FilterConditionEditorPanel();
			_conditionEditor.Location = new Point(2, 4);
			_conditionEditor.Resize += OnStateResize;
			_conditionEditor.AddSelectionChangedHandler(OnEditorSelectionChanged);
			_conditionEditor.AddFilterChangedHandler(OnEditorConditionChanged);
			_conditionEditor.AddActiveControlChangedHandler(OnEditorControlActivated);
			_panel.Controls.Add(_conditionEditor);
			base.Controls.Add(_panel);
			_gridControlStyle.ButtonAppearance = ButtonAppearance.Light3D;
			ticks = DateTime.Now.Ticks - ticks;
		}

		private void OnEditorControlActivated(object sender, EventArgs eventArgs)
		{
			Control control = _conditionEditor.GetFocusedChildControl();
			EnsureControlVisible(control);
		}

		private void EnsureControlVisible(Control control)
		{
			ScrollEditorControlIntoView(control);
		}

		private void OnEditorSelectionChanged(object sender, EventArgs eventArgs)
		{
			RaiseSelectionChanged(eventArgs);
		}

		private void ScrollEditorControlIntoView(Control control)
		{
			if (control != null)
			{
				Rectangle bounds = control.Bounds;
				if (control is TextBox)
				{
					bounds.Inflate(3, 2);
				}
				Point location = bounds.Location;
				location = GridHelper.GetParentSafe(control).PointToScreen(location);
				location = _panel.PointToClient(location);
				bounds.Location = location;
				Point otherLocation = bounds.Location;
				if (bounds.Right > _panel.ClientRectangle.Right)
				{
					otherLocation.X = _panel.ClientRectangle.Right - bounds.Width;
				}
				if (otherLocation.X < 0)
				{
					otherLocation.X = 0;
				}
				if (bounds.Bottom > _panel.ClientRectangle.Bottom)
				{
					otherLocation.Y = _panel.ClientRectangle.Bottom - bounds.Height;
				}
				if (otherLocation.Y < 0)
				{
					otherLocation.Y = 0;
				}
				SetScrollOffsetX(_scrollOffsetX + (bounds.X - otherLocation.X));
				SetScrollOffsetY(_scrollOffsetY + (bounds.Y - otherLocation.Y));
			}
		}

		public void AddSelectionChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _selectionChangedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _selectionChangedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		private void RaiseSelectionChanged(EventArgs eventArgs)
		{
			if (_selectionChangedHandler != null)
			{
				_selectionChangedHandler(this, eventArgs);
			}
		}

		private void OnEditorConditionChanged(object sender, EventArgs eventArgs)
		{
			RaiseConditionChanged(eventArgs);
		}

		public void AddConditionChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _conditionChangedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _conditionChangedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		private void RaiseConditionChanged(EventArgs eventArgs)
		{
			if (_conditionChangedHandler != null)
			{
				_conditionChangedHandler(this, eventArgs);
			}
		}

		private void OnStateResize(object sender, EventArgs eventArgs)
		{
			UpdateScrollBars();
		}

		protected override void OnClientResize()
		{
			base.OnClientResize();
			if (_panel != null)
			{
				_panel.Bounds = base.ClientRectangle;
			}
			UpdateScrollBars();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			DrawControlBorder(eventArgs.Graphics);
		}

		private Rectangle GetContentBounds()
		{
			if (_conditionEditor == null)
			{
				return Rectangle.Empty;
			}
			Rectangle bounds = _conditionEditor.Bounds;
			bounds.Inflate(2, 4);
			bounds.Offset(-bounds.Left, -bounds.Top);
			return bounds;
		}

		private void UpdateScrollBars()
		{
			if (_conditionEditor != null)
			{
				Rectangle rectangle = GetContentBounds();
				Rectangle clientRectangle = base.ClientRectangle;
				if (rectangle.Width > clientRectangle.Width)
				{
					SetHorizontalScrollRequired(true);
					ConfigureHorizontalScrollBar(0, GetContentBounds().Width, base.ClientRectangle.Width, _scrollOffsetX);
				}
				else
				{
					SetScrollOffsetX(0);
					SetHorizontalScrollRequired(false);
				}
				if (rectangle.Height > clientRectangle.Height)
				{
					SetVerticalScrollRequired(true);
					ConfigureVerticalScrollBar(0, GetContentBounds().Height, base.ClientRectangle.Height, _scrollOffsetY);
				}
				else
				{
					SetScrollOffsetY(0);
					SetVerticalScrollRequired(false);
				}
			}
		}

		private void SetScrollOffsetX(int controlStyleControl2Integer)
		{
			if (controlStyleControl2Integer < 0)
			{
				controlStyleControl2Integer = 0;
			}
			GetHorizontalScrollBar().SetScrollValue(controlStyleControl2Integer);
			_scrollOffsetX = GetHorizontalScrollBar().GetScrollValue();
			_conditionEditor.Left = -(_scrollOffsetX - 2);
		}

		private void SetScrollOffsetY(int controlStyleControl2Integer)
		{
			if (controlStyleControl2Integer < 0)
			{
				controlStyleControl2Integer = 0;
			}
			GetVScroll().SetScrollValue(controlStyleControl2Integer);
			_scrollOffsetY = GetVScroll().GetScrollValue();
			_conditionEditor.Top = -(_scrollOffsetY - 4);
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			switch (scrollCode)
			{
			case 2:
			case 3:
				SetScrollOffsetX(GetHorizontalScrollBar().GetScrollValue());
				break;
			case 0:
			case 1:
				SetScrollOffsetY(GetVScroll().GetScrollValue());
				break;
			}
		}

		public GridFilterCondition GetFilterCondition()
		{
			return _conditionEditor.GetCondition();
		}

		public void SetFilterCondition(GridFilterCondition gridFilterCondition)
		{
			_conditionEditor.SetCondition(gridFilterCondition);
		}

		public void AssignTable(GridTable gridTable)
		{
			_conditionEditor.SetTable(gridTable);
		}

		public GridFilterCondition GetActiveFilterCondition()
		{
			return _conditionEditor.GetActiveCondition();
		}

		public void ActivateAndShowCondition(GridFilterCondition gridFilterCondition)
		{
			_conditionEditor.ActivateEditorForCondition(gridFilterCondition);
			FilterConditionEditorPanel editorForCondition = _conditionEditor.FindEditorForCondition(gridFilterCondition);
			if (editorForCondition != null)
			{
				EnsureControlVisible(editorForCondition);
			}
		}
	}
}
