using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonControl : Control
	{
		private enum ScrollAreaType
		{
			NotSet,
			Arrow,
			Thumb,
			Page
		}

		private struct RibbonControlData
		{
			internal bool _isPressed;

			internal bool _isHot;

			internal Rectangle _bounds;

			internal bool _isVisible;

			internal ScrollAreaType _areaType;

			internal RibbonControlData(ScrollAreaType type)
			{
				_isPressed = false;
				_isHot = false;
				_bounds = Rectangle.Empty;
				_areaType = type;
				_isVisible = true;
			}
		}

		private const int HoverCheckInterval = 50;

		private const int RepeatDelay = 500;

		private const int RepeatInterval = 30;

		private const int MinThumbSize = 8;

		private System.Windows.Forms.Timer _repeatTimer;

		private System.Windows.Forms.Timer _hotTrackTimer;

		private bool _isDraggingThumb;

		private int _thumbGrabOffset;

		private int _dragStartValue;

		private bool _isMouseInside;

		private RibbonControlColorSet _colorSet;

		private RibbonControlData[] _areas = new RibbonControlData[10];

		private ScrollBarType _orientation;

		private int _largeChange = 1;

		private int _minimum;

		private int _maximum = int.MaxValue;

		private int _scrollValue;

		private int _smallChange = 1;

		private int _pressedArea = -1;

		private int _hotArea = -1;

		private ScrollEventHandler _scrollHandler;

		private EventHandler _valueChangedHandler;

		private void InitializeTimers()
		{
			_repeatTimer = new System.Windows.Forms.Timer();
			_repeatTimer.Tick += OnTimerTick;
			_hotTrackTimer = new System.Windows.Forms.Timer();
			_hotTrackTimer.Enabled = false;
			_hotTrackTimer.Interval = HoverCheckInterval;
			_hotTrackTimer.Tick += OnHotAreaTimerTick;
		}

		internal RibbonControl(ScrollBarType type)
		{
			_orientation = type;
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			BackColor = Color.Transparent;
			_colorSet = new RibbonControlColorSet();
			InitializeTimers();
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			RenderScrollBar(pe.Graphics);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_repeatTimer != null)
				{
					_repeatTimer.Tick -= OnTimerTick;
					_repeatTimer.Dispose();
				}
				if (_hotTrackTimer != null)
				{
					_hotTrackTimer.Tick -= OnHotAreaTimerTick;
					_hotTrackTimer.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			HandleMouseDown(eventArgs);
		}

		internal void ForwardMouseLeave(MouseEventArgs eventArgs)
		{
			OnMouseLeave(new EventArgs());
		}

		internal void HandleMouseMove(MouseEventArgs eventArgs)
		{
			if (_isDraggingThumb)
			{
				DragThumb(eventArgs.X, eventArgs.Y);
				return;
			}
			if (eventArgs.Button == MouseButtons.Left && _pressedArea != -1)
			{
				int hitArea = HitTestArea(eventArgs.X, eventArgs.Y);
				if (hitArea != _pressedArea)
				{
					SetAreaPressed(_pressedArea, false);
				}
				else
				{
					SetAreaPressed(_pressedArea, true);
				}
			}
			if (eventArgs.Button == MouseButtons.None)
			{
				UpdateHotArea(false, eventArgs.X, eventArgs.Y);
			}
		}

		internal void HandleMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}
			_pressedArea = HitTestArea(eventArgs.X, eventArgs.Y);
			SetAreaPressed(_pressedArea, true);
			if (_pressedArea == -1)
			{
				return;
			}
			int pressedArea = _pressedArea;
			if (pressedArea == 2)
			{
				_isDraggingThumb = true;
				OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, _scrollValue));
				_dragStartValue = _scrollValue;
				if (_orientation == ScrollBarType.Vertical)
				{
					_thumbGrabOffset = eventArgs.Y - _areas[2]._bounds.Top;
				}
				else
				{
					_thumbGrabOffset = eventArgs.X - _areas[2]._bounds.Left;
				}
			}
			else
			{
				RaiseScrollForArea(_pressedArea);
				_repeatTimer.Interval = RepeatDelay;
				_repeatTimer.Enabled = true;
			}
		}

		protected void OnTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				if (_pressedArea != -1)
				{
					Point position = Cursor.Position;
					position = PointToClient(position);
					OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
					if (_areas[_pressedArea]._isPressed)
					{
						RaiseScrollForArea(_pressedArea);
					}
				}
				if (_repeatTimer.Interval != RepeatInterval)
				{
					_repeatTimer.Interval = RepeatInterval;
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
			{
				// The control can be disposed while a timer tick is queued.
			}
		}

		protected void OnHotAreaTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				Point position = Cursor.Position;
				position = PointToClient(position);
				if (!base.ClientRectangle.Contains(position))
				{
					UpdateHotArea(true, 0, 0);
				}
				_hotTrackTimer.Enabled = false;
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
			{
				// The control can be disposed while a timer tick is queued.
			}
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			if (_orientation == ScrollBarType.Vertical)
			{
				int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
				scrollBarWidth -= SystemInformation.VerticalScrollBarWidth % 2;
				if (base.Width != scrollBarWidth)
				{
					base.Width = scrollBarWidth;
					return;
				}
			}
			else
			{
				int scrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
				scrollBarHeight -= SystemInformation.HorizontalScrollBarHeight % 2;
				if (base.Height != scrollBarHeight)
				{
					base.Height = scrollBarHeight;
					return;
				}
			}
			RecalculateLayout();
		}

		public void SetLargeChange(int largeChange)
		{
			if (largeChange < 0)
			{
				largeChange = 0;
			}
			if (largeChange != _largeChange)
			{
				_largeChange = largeChange;
				RecalculateLayout();
			}
		}

		public void SetMinimum(int minimum)
		{
			if (minimum != _minimum)
			{
				_minimum = minimum;
				if (_scrollValue < _minimum)
				{
					SetScrollValue(_minimum);
				}
				RecalculateLayout();
			}
		}

		public void SetMaximum(int maximum)
		{
			if (maximum != _maximum)
			{
				_maximum = maximum;
				if (_scrollValue > GetMaxScrollValue())
				{
					SetScrollValue(GetMaxScrollValue());
				}
				RecalculateLayout();
			}
		}

		public int GetScrollValue()
		{
			return _scrollValue;
		}

		public void SetScrollValue(int newValue)
		{
			if (newValue < _minimum)
			{
				newValue = _minimum;
			}
			if (newValue > GetMaxScrollValue())
			{
				newValue = GetMaxScrollValue();
			}
			if (newValue != _scrollValue)
			{
				_scrollValue = newValue;
				OnValueChanged(new EventArgs());
				RefreshThumb();
			}
		}

		public int GetSmallChange()
		{
			return _smallChange;
		}

		public void SetSmallChange(int smallChange)
		{
			if (smallChange < 0)
			{
				smallChange = 0;
			}
			if (smallChange != _smallChange)
			{
				_smallChange = smallChange;
			}
		}

		private void RecalculateLayout()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			int width = 0;
			int height = 0;
			if (_orientation == ScrollBarType.Vertical)
			{
				width = SystemInformation.VerticalScrollBarWidth;
				height = clientRectangle.Height;
				int verticalScrollBarArrowHeight = SystemInformation.VerticalScrollBarArrowHeight;
				RibbonControlData a = new RibbonControlData(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, width, verticalScrollBarArrowHeight);
				_areas[0] = a;
				a = new RibbonControlData(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Bottom - verticalScrollBarArrowHeight, width, verticalScrollBarArrowHeight);
				_areas[4] = a;
				CalculateVerticalLayout();
			}
			else
			{
				width = clientRectangle.Width;
				height = SystemInformation.HorizontalScrollBarHeight;
				int horizontalScrollBarArrowWidth = SystemInformation.HorizontalScrollBarArrowWidth;
				RibbonControlData a = new RibbonControlData(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, horizontalScrollBarArrowWidth, height);
				_areas[0] = a;
				a = new RibbonControlData(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Right - horizontalScrollBarArrowWidth, clientRectangle.Top, horizontalScrollBarArrowWidth, height);
				_areas[4] = a;
				CalculateHorizontalLayout();
			}
			Invalidate();
		}

		private void CalculateVerticalLayout()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = new Rectangle(clientRectangle.Left, _areas[0]._bounds.Bottom, clientRectangle.Width, _areas[4]._bounds.Top - _areas[0]._bounds.Bottom);
			RibbonControlData a = new RibbonControlData(ScrollAreaType.Thumb);
			Rectangle thumbRectangle;
			if (!base.Enabled || rectangle.Height < MinThumbSize)
			{
				thumbRectangle = Rectangle.Empty;
				a._isVisible = false;
			}
			else
			{
				int thumbHeight = 0;
				if (_largeChange > 0)
				{
					float thumbRatio = (float)_largeChange / (float)GetRange();
					thumbHeight = (int)((float)rectangle.Height * thumbRatio);
					if (thumbHeight < MinThumbSize)
					{
						thumbHeight = MinThumbSize;
					}
				}
				else
				{
					thumbHeight = SystemInformation.VerticalScrollBarThumbHeight;
				}
				if (GetRange() <= 0 || _maximum <= _minimum)
				{
					thumbHeight = rectangle.Height;
				}
				float thumbOffset = 0f;
				if (GetRange() - _largeChange > 0)
				{
					thumbOffset = (float)(_scrollValue - _minimum) / (float)(GetRange() - _largeChange);
					thumbOffset *= (float)(rectangle.Height - thumbHeight);
				}
				else
				{
					thumbHeight = rectangle.Height;
					thumbOffset = 0f;
				}
				int thumbTop = rectangle.Top;
				if (_scrollValue > _minimum)
				{
					thumbTop += (int)thumbOffset;
				}
				if (thumbTop < rectangle.Top)
				{
					thumbTop = rectangle.Top;
				}
				if (_scrollValue == GetMaxScrollValue() || thumbTop + thumbHeight > rectangle.Bottom)
				{
					thumbTop = rectangle.Bottom - thumbHeight;
				}
				thumbRectangle = new Rectangle(clientRectangle.Left, thumbTop, clientRectangle.Width, thumbHeight);
			}
			a._bounds = thumbRectangle;
			if (_pressedArea == 2)
			{
				a._isPressed = true;
			}
			_areas[2] = a;
			if (a._isVisible)
			{
				a = new RibbonControlData(ScrollAreaType.Page);
				if (thumbRectangle.Top > rectangle.Top)
				{
					a._bounds = new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, thumbRectangle.Top - rectangle.Top);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isVisible = false;
				}
				if (1 == _pressedArea)
				{
					a._isPressed = true;
				}
				_areas[1] = a;
				a = new RibbonControlData(ScrollAreaType.Page);
				if (thumbRectangle.Bottom < rectangle.Bottom)
				{
					a._bounds = new Rectangle(rectangle.Left, thumbRectangle.Bottom, rectangle.Width, rectangle.Bottom - thumbRectangle.Bottom);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isVisible = false;
				}
				if (3 == _pressedArea)
				{
					a._isPressed = true;
				}
				_areas[3] = a;
			}
			else
			{
				a = new RibbonControlData(ScrollAreaType.Page);
				a._bounds = rectangle;
				_areas[1] = a;
				a = new RibbonControlData(ScrollAreaType.Page);
				a._isVisible = false;
				_areas[3] = a;
			}
		}

		private void CalculateHorizontalLayout()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = new Rectangle(_areas[0]._bounds.Right, clientRectangle.Top, _areas[4]._bounds.Left - _areas[0]._bounds.Right, clientRectangle.Height);
			RibbonControlData a = new RibbonControlData(ScrollAreaType.Thumb);
			Rectangle thumbRectangle;
			if (rectangle.Width < MinThumbSize || !base.Enabled)
			{
				thumbRectangle = Rectangle.Empty;
				a._isVisible = false;
			}
			else
			{
				int thumbWidth = 0;
				if (_largeChange != 0)
				{
					float thumbRatio = (float)_largeChange / (float)GetRange();
					thumbWidth = (int)((float)rectangle.Width * thumbRatio);
					if (thumbWidth < MinThumbSize)
					{
						thumbWidth = MinThumbSize;
					}
				}
				else
				{
					thumbWidth = SystemInformation.HorizontalScrollBarThumbWidth;
				}
				if (GetRange() <= 0 || _maximum <= _minimum)
				{
					thumbWidth = rectangle.Width;
				}
				float thumbOffset = 0f;
				if (GetRange() - _largeChange > 0)
				{
					thumbOffset = (float)(_scrollValue - _minimum) / (float)(GetRange() - _largeChange);
					thumbOffset *= (float)(rectangle.Width - thumbWidth);
				}
				else
				{
					thumbWidth = rectangle.Width;
					thumbOffset = 0f;
				}
				int thumbLeft = rectangle.Left;
				if (_scrollValue > _minimum)
				{
					thumbLeft += (int)thumbOffset;
				}
				if (thumbLeft < rectangle.Left)
				{
					thumbLeft = rectangle.Left;
				}
				if (_scrollValue == GetMaxScrollValue() || thumbLeft + thumbWidth > rectangle.Right)
				{
					thumbLeft = rectangle.Right - thumbWidth;
				}
				thumbRectangle = new Rectangle(thumbLeft, base.ClientRectangle.Top, thumbWidth, base.ClientRectangle.Height);
			}
			a._bounds = thumbRectangle;

			_areas[2] = a;
			if (a._isVisible)
			{
				a = new RibbonControlData(ScrollAreaType.Page);
				if (thumbRectangle.Left > rectangle.Left)
				{
					a._bounds = new Rectangle(rectangle.Left, rectangle.Top, thumbRectangle.Left - rectangle.Left, rectangle.Height);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isVisible = false;
				}
				if (1 == _pressedArea)
				{
					a._isPressed = true;
				}
				_areas[1] = a;
				a = new RibbonControlData(ScrollAreaType.Page);
				if (thumbRectangle.Right < rectangle.Right)
				{
					a._bounds = new Rectangle(thumbRectangle.Right, rectangle.Top, rectangle.Right - thumbRectangle.Right, rectangle.Height);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isVisible = false;
				}
				if (3 == _pressedArea)
				{
					a._isPressed = true;
				}
				_areas[3] = a;
			}
			else
			{
				a = new RibbonControlData(ScrollAreaType.Page);
				a._bounds = rectangle;
				_areas[1] = a;
				a = new RibbonControlData(ScrollAreaType.Page);
				a._isVisible = false;
				_areas[3] = a;
			}
		}

		private int GetRange()
		{
			return _maximum - _minimum + 1;
		}

		private int GetMaxScrollValue()
		{
			return _maximum - _largeChange + 1;
		}

		internal bool IsMouseInside()
		{
			return _isMouseInside;
		}

		internal void SetMouseInside(bool ribbonControlBoolean)
		{
			if (_isMouseInside != ribbonControlBoolean)
			{
				_isMouseInside = ribbonControlBoolean;
				Invalidate();
			}
		}

		internal void RefreshThumb()
		{
			try
			{
				if (_orientation == ScrollBarType.Horizontal)
				{
					CalculateHorizontalLayout();
				}
				else
				{
					CalculateVerticalLayout();
				}
				Invalidate();
				Update();
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
			{
				// The control can be disposed while the layout updates.
			}
		}

		private void RenderScrollBar(Graphics graphics)
		{
			PaintDefaultTheme(graphics);
		}

		private int ClampValue(int value)
		{
			if (value < _minimum)
			{
				value = _minimum;
			}
			if (value > GetMaxScrollValue())
			{
				value = GetMaxScrollValue();
			}
			return value;
		}

		private void RaiseScrollForArea(int area)
		{
			int newValue = _scrollValue;
			ScrollEventArgs e;
			switch (area)
			{
			default:
				return;
			case 0:
				newValue = ClampValue(_scrollValue - _smallChange);
				e = new ScrollEventArgs(ScrollEventType.SmallDecrement, newValue);
				break;
			case 4:
				newValue = ClampValue(_scrollValue + _smallChange);
				e = new ScrollEventArgs(ScrollEventType.SmallIncrement, newValue);
				break;
			case 1:
				newValue = ClampValue(_scrollValue - _largeChange);
				e = new ScrollEventArgs(ScrollEventType.LargeDecrement, newValue);
				break;
			case 3:
				newValue = ClampValue(_scrollValue + _largeChange);
				e = new ScrollEventArgs(ScrollEventType.LargeIncrement, newValue);
				break;
			case 2:
				e = new ScrollEventArgs(ScrollEventType.ThumbTrack, _scrollValue);
				break;
			case 5:
				e = new ScrollEventArgs(ScrollEventType.EndScroll, _scrollValue);
				break;
			}
			OnScroll(e);
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			HandleMouseMove(eventArgs);
		}

		private void DragThumb(int mouseX, int mouseY)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (_pressedArea != 2)
			{
				return;
			}
			if (_orientation == ScrollBarType.Vertical)
			{
				if (Rectangle.Inflate(clientRectangle, clientRectangle.Width * 8, SystemInformation.VerticalScrollBarArrowHeight * 2).Contains(mouseX, mouseY))
				{
					int trackHeight = _areas[4]._bounds.Top - _areas[0]._bounds.Bottom;
					int thumbTopOffset = mouseY - _thumbGrabOffset - _areas[0]._bounds.Bottom;
					if (thumbTopOffset < 0)
					{
						thumbTopOffset = 0;
					}
					if (thumbTopOffset > trackHeight)
					{
						thumbTopOffset = trackHeight;
					}
					float verticalFraction = (float)thumbTopOffset / (float)trackHeight;
					int verticalTargetValue = _minimum + (int)((float)(_maximum - _minimum) * verticalFraction);
					if (verticalTargetValue < _minimum)
					{
						verticalTargetValue = _minimum;
					}
					if (verticalTargetValue > GetMaxScrollValue())
					{
						verticalTargetValue = GetMaxScrollValue();
					}
					if (verticalTargetValue != _scrollValue)
					{
						_scrollValue = verticalTargetValue;
						RefreshThumb();
						RaiseScrollForArea(2);
					}
				}
				else
				{
					_scrollValue = _dragStartValue;
					RefreshThumb();
					RaiseScrollForArea(2);
				}
			}
			else if (Rectangle.Inflate(base.ClientRectangle, SystemInformation.HorizontalScrollBarArrowWidth * 2, base.ClientRectangle.Height * 8).Contains(mouseX, mouseY))
			{
				int trackWidth = _areas[4]._bounds.Left - _areas[0]._bounds.Right;
				int thumbLeftOffset = mouseX - _thumbGrabOffset - _areas[0]._bounds.Right;
				if (thumbLeftOffset < 0)
				{
					thumbLeftOffset = 0;
				}
				if (thumbLeftOffset > trackWidth)
				{
					thumbLeftOffset = trackWidth;
				}
				float horizontalFraction = (float)thumbLeftOffset / (float)trackWidth;
				int horizontalTargetValue = _minimum + (int)((float)(_maximum - _minimum + 1) * horizontalFraction);
				if (horizontalTargetValue < _minimum)
				{
					horizontalTargetValue = _minimum;
				}
				if (horizontalTargetValue > GetMaxScrollValue())
				{
					horizontalTargetValue = GetMaxScrollValue();
				}
				if (horizontalTargetValue != _scrollValue)
				{
					_scrollValue = horizontalTargetValue;
					RefreshThumb();
					RaiseScrollForArea(2);
				}
			}
			else
			{
				_scrollValue = _dragStartValue;
				RefreshThumb();
				RaiseScrollForArea(2);
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			OnMouseLeave(new EventArgs());
		}

		private int HitTestArea(int pointX, int pointY)
		{
			int firstArea = 0;
			int lastArea = 4;
			for (int i = firstArea; i <= lastArea; i++)
			{
				RibbonControlData a = _areas[i];
				if (a._isVisible && a._bounds.Contains(pointX, pointY))
				{
					return i;
				}
			}
			return -1;
		}

		private void UpdateHotArea(bool clearHot, int mouseX, int mouseY)
		{
			if (clearHot)
			{
				SetAreaHot(_hotArea, false);
				_hotArea = -1;
				return;
			}
			int areaUnderMouse = HitTestArea(mouseX, mouseY);
			if (areaUnderMouse != _hotArea)
			{
				SetAreaHot(_hotArea, false);
				_hotArea = areaUnderMouse;
				SetAreaHot(_hotArea, true);
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			ResetMouseState();
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			base.OnMouseEnter(eventArgs);
			SetMouseInside(true);
		}

		internal void ResetMouseState()
		{
			base.OnMouseLeave(EventArgs.Empty);
			SetMouseInside(false);
			if (_isDraggingThumb)
			{
				_isDraggingThumb = false;
				if (_pressedArea == 2 && _scrollValue != _dragStartValue)
				{
					RaiseScrollForArea(5);
					OnValueChanged(new EventArgs());
				}
			}
			UpdateHotArea(true, 0, 0);
			SetAreaPressed(_pressedArea, false);
			_pressedArea = -1;
			_repeatTimer.Enabled = false;
		}

		private void SetAreaHot(int areaIndex, bool ribbonControlDataBoolean)
		{
			if (areaIndex != -1 && base.Enabled)
			{
				RibbonControlData a = _areas[areaIndex];
				if (a._isHot != ribbonControlDataBoolean && a._isVisible)
				{
					_areas[areaIndex]._isHot = ribbonControlDataBoolean;
					Invalidate(a._bounds);
					Update();
					_hotTrackTimer.Enabled = ribbonControlDataBoolean;
				}
			}
		}

		private void SetAreaPressed(int areaIndex, bool isPressed)
		{
			if (areaIndex == -1)
			{
				return;
			}
			RibbonControlData a = _areas[areaIndex];
			if (a._isPressed != isPressed)
			{
				_areas[areaIndex]._isPressed = isPressed;
				if (a._isVisible)
				{
					Invalidate(_areas[areaIndex]._bounds);
					Update();
				}
			}
		}

		public void AddScrollHandler(ScrollEventHandler handler)
		{
			ScrollEventHandler scrollEventHandler = _scrollHandler;
			ScrollEventHandler comparandHandler;
			do
			{
				comparandHandler = scrollEventHandler;
				ScrollEventHandler value = (ScrollEventHandler)Delegate.Combine(comparandHandler, handler);
				scrollEventHandler = Interlocked.CompareExchange(ref _scrollHandler, value, comparandHandler);
			}
			while ((object)scrollEventHandler != comparandHandler);
		}

		public void RemoveScrollHandler(ScrollEventHandler handler)
		{
			ScrollEventHandler scrollEventHandler = _scrollHandler;
			ScrollEventHandler comparandHandler;
			do
			{
				comparandHandler = scrollEventHandler;
				ScrollEventHandler value = (ScrollEventHandler)Delegate.Remove(comparandHandler, handler);
				scrollEventHandler = Interlocked.CompareExchange(ref _scrollHandler, value, comparandHandler);
			}
			while ((object)scrollEventHandler != comparandHandler);
		}

		protected virtual void OnScroll(ScrollEventArgs eventArgs)
		{
			if (_scrollHandler != null)
			{
				_scrollHandler(this, eventArgs);
			}
			SetScrollValue(eventArgs.NewValue);
		}

		public void AddValueChangedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _valueChangedHandler;
			EventHandler comparandHandler;
			do
			{
				comparandHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(comparandHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _valueChangedHandler, value, comparandHandler);
			}
			while ((object)eventHandler != comparandHandler);
		}

		protected virtual void OnValueChanged(EventArgs eventArgs)
		{
			if (_valueChangedHandler != null)
			{
				_valueChangedHandler(this, eventArgs);
			}
		}

		protected override void OnEnabledChanged(EventArgs eventArgs)
		{
			base.OnEnabledChanged(eventArgs);
			SetAreaHot(_hotArea, false);
			SetAreaPressed(_pressedArea, false);
			RecalculateLayout();
		}

		protected override void OnVisibleChanged(EventArgs eventArgs)
		{
			base.OnVisibleChanged(eventArgs);
		}
	}
}
