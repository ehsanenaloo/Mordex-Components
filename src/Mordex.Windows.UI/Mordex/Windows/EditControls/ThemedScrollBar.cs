using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	internal partial class ThemedScrollBar : Control
	{
		private enum ScrollAreaType
		{
			NotSet,
			Arrow,
			Thumb,
			Page
		}

		private struct ScrollAreaState
		{
			internal bool _isPressed;

			internal bool _isHot;

			internal Rectangle _bounds;

			internal bool _isPresent;

			internal ScrollAreaType _areaType;

			internal ScrollAreaState(ScrollAreaType type)
			{
				_isPressed = false;
				_isHot = false;
				_bounds = Rectangle.Empty;
				_areaType = type;
				_isPresent = true;
			}
		}

		private const int AreaLowArrow = 0;

		private const int AreaLowPage = 1;

		private const int AreaThumb = 2;

		private const int AreaHighPage = 3;

		private const int AreaHighArrow = 4;

		private const int AreaCount = 5;

		// Not an area. Passed to the scroll handler to raise EndScroll.
		private const int ActionEndScroll = 5;

		private const int NoArea = -1;

		private const int MinThumbSize = 8;

		private const int HoverCheckIntervalMs = 50;

		private const int RepeatDelayMs = 500;

		private const int RepeatIntervalMs = 30;

		// A thumb drag is cancelled when the pointer leaves the bar by more than this many bar widths (or arrow sizes).
		private const int DragCancelBarFactor = 8;

		private const int DragCancelArrowFactor = 2;

		private System.Windows.Forms.Timer _repeatTimer;

		private System.Windows.Forms.Timer _hoverCheckTimer;


		private bool _isMouseOver;

		private ScrollAreaState[] _areas = new ScrollAreaState[AreaCount];

		private ScrollBarType _scrollBarType;

		private int _largeChange = 1;

		private int _minimum;

		private int _maximum = int.MaxValue;

		private int _value;

		private int _smallChange = 1;

		private bool _useVisualStyles = true;

		private bool _isDraggingThumb;

		private int _thumbGrabOffset;

		private int _valueBeforeDrag;

		private int _pressedArea = NoArea;

		private int _hotArea = NoArea;

		private ScrollEventHandler _scrollHandler;

		private EventHandler _valueChangedHandler;

		internal ControlAppearance _appearance;

		private VisualStylePainter _themePainter;

		private void InitializeControl()
		{
			_repeatTimer = new System.Windows.Forms.Timer();
			_repeatTimer.Tick += OnRepeatTimerTick;
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			_hoverCheckTimer = new System.Windows.Forms.Timer();
			_hoverCheckTimer.Enabled = false;
			_hoverCheckTimer.Interval = HoverCheckIntervalMs;
			_hoverCheckTimer.Tick += OnHoverCheckTimerTick;
		}

		internal void SetIsMouseOver(bool isMouseOver)
		{
			if (_isMouseOver != isMouseOver)
			{
				_isMouseOver = isMouseOver;
				Invalidate();
			}
		}

		internal ThemedScrollBar(ScrollBarType type)
		{
			_scrollBarType = type;
			InitializeControl();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_repeatTimer.Dispose();
				_hoverCheckTimer.Dispose();
				if (_themePainter != null)
				{
					_themePainter.DisposeResources();
					_themePainter = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			if (_appearance == null)
			{
				pe.Graphics.FillRectangle(SystemBrushes.Control, base.ClientRectangle);
				return;
			}
			PaintScrollBar(pe.Graphics);
			_appearance.DisposeResources();
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}
			_pressedArea = HitTestArea(eventArgs.X, eventArgs.Y);
			SetAreaPressed(_pressedArea, true);
			if (_pressedArea == NoArea)
			{
				return;
			}
			int area = _pressedArea;
			if (area == AreaThumb)
			{
				_isDraggingThumb = true;
				OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, _value));
				_valueBeforeDrag = _value;
				if (_scrollBarType == ScrollBarType.Vertical)
				{
					_thumbGrabOffset = eventArgs.Y - _areas[AreaThumb]._bounds.Top;
				}
				else
				{
					_thumbGrabOffset = eventArgs.X - _areas[AreaThumb]._bounds.Left;
				}
			}
			else
			{
				RaiseScrollForArea(_pressedArea);
				_repeatTimer.Interval = RepeatDelayMs;
				_repeatTimer.Enabled = true;
			}
		}

		protected void OnRepeatTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				if (_pressedArea != NoArea)
				{
					Point position = Cursor.Position;
					position = PointToClient(position);
					OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
					if (_areas[_pressedArea]._isPressed)
					{
						RaiseScrollForArea(_pressedArea);
					}
				}
				if (_repeatTimer.Interval != RepeatIntervalMs)
				{
					_repeatTimer.Interval = RepeatIntervalMs;
				}
			}
			catch (ObjectDisposedException)
			{
				// A last tick can arrive while the control is being disposed.
			}
		}

		protected void OnHoverCheckTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				Point position = Cursor.Position;
				position = PointToClient(position);
				if (!base.ClientRectangle.Contains(position))
				{
					UpdateHotArea(true, 0, 0);
				}
				_hoverCheckTimer.Enabled = false;
			}
			catch (ObjectDisposedException)
			{
				// A last tick can arrive while the control is being disposed.
			}
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			if (_scrollBarType == ScrollBarType.Vertical)
			{
				int barWidth = SystemInformation.VerticalScrollBarWidth;
				if (base.Width != barWidth)
				{
					base.Width = barWidth;
					return;
				}
			}
			else
			{
				int barHeight = SystemInformation.HorizontalScrollBarHeight;
				if (base.Height != barHeight)
				{
					base.Height = barHeight;
					return;
				}
			}
			LayoutAreas();
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
				LayoutAreas();
			}
		}

		public void SetMinimum(int minimum)
		{
			if (minimum != _minimum)
			{
				_minimum = minimum;
				if (_value < _minimum)
				{
					SetValue(_minimum);
				}
				LayoutAreas();
			}
		}

		public void SetMaximum(int maximum)
		{
			if (maximum != _maximum)
			{
				_maximum = maximum;
				if (_value > GetMaxValue())
				{
					SetValue(GetMaxValue());
				}
				LayoutAreas();
			}
		}

		public int GetValue()
		{
			return _value;
		}

		public void SetValue(int value)
		{
			if (value < _minimum)
			{
				value = _minimum;
			}
			if (value > GetMaxValue())
			{
				value = GetMaxValue();
			}
			if (value != _value)
			{
				_value = value;
				OnValueChanged(new EventArgs());
				RefreshThumb();
			}
		}

		public int GetLargeChange()
		{
			return _largeChange;
		}

		public int GetSmallChange()
		{
			return _smallChange;
		}

		private void LayoutAreas()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			int barWidth = 0;
			int barHeight = 0;
			if (_scrollBarType == ScrollBarType.Vertical)
			{
				barWidth = SystemInformation.VerticalScrollBarWidth;
				barHeight = clientRectangle.Height;
				int verticalScrollBarArrowHeight = SystemInformation.VerticalScrollBarArrowHeight;
				ScrollAreaState a = new ScrollAreaState(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, barWidth, verticalScrollBarArrowHeight);
				_areas[AreaLowArrow] = a;
				a = new ScrollAreaState(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Bottom - verticalScrollBarArrowHeight, barWidth, verticalScrollBarArrowHeight);
				_areas[AreaHighArrow] = a;
				LayoutVerticalThumb();
			}
			else
			{
				barWidth = clientRectangle.Width;
				barHeight = SystemInformation.HorizontalScrollBarHeight;
				int horizontalScrollBarArrowWidth = SystemInformation.HorizontalScrollBarArrowWidth;
				ScrollAreaState a = new ScrollAreaState(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, horizontalScrollBarArrowWidth, barHeight);
				_areas[AreaLowArrow] = a;
				a = new ScrollAreaState(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Right - horizontalScrollBarArrowWidth, clientRectangle.Top, horizontalScrollBarArrowWidth, barHeight);
				_areas[AreaHighArrow] = a;
				LayoutHorizontalThumb();
			}
			Invalidate();
		}

		private void LayoutVerticalThumb()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = new Rectangle(clientRectangle.Left, _areas[AreaLowArrow]._bounds.Bottom, clientRectangle.Width, _areas[AreaHighArrow]._bounds.Top - _areas[AreaLowArrow]._bounds.Bottom);
			ScrollAreaState a = new ScrollAreaState(ScrollAreaType.Thumb);
			Rectangle thumbBounds;
			if (!base.Enabled || rectangle.Height < MinThumbSize)
			{
				thumbBounds = Rectangle.Empty;
				a._isPresent = false;
			}
			else
			{
				int thumbLength = 0;
				if (_largeChange > 0)
				{
					float pageRatio = (float)_largeChange / (float)GetRange();
					thumbLength = (int)((float)rectangle.Height * pageRatio);
					if (thumbLength < MinThumbSize)
					{
						thumbLength = MinThumbSize;
					}
				}
				else
				{
					thumbLength = SystemInformation.VerticalScrollBarThumbHeight;
				}
				if (GetRange() <= 0 || _maximum <= _minimum)
				{
					thumbLength = rectangle.Height;
				}
				float thumbOffset = 0f;
				if (GetRange() - _largeChange > 0)
				{
					thumbOffset = (float)(_value - _minimum) / (float)(GetRange() - _largeChange);
					thumbOffset *= (float)(rectangle.Height - thumbLength);
				}
				else
				{
					thumbLength = rectangle.Height;
					thumbOffset = 0f;
				}
				int thumbStart = rectangle.Top;
				if (_value > _minimum)
				{
					thumbStart += (int)thumbOffset;
				}
				if (thumbStart < rectangle.Top)
				{
					thumbStart = rectangle.Top;
				}
				if (_value == GetMaxValue() || thumbStart + thumbLength > rectangle.Bottom)
				{
					thumbStart = rectangle.Bottom - thumbLength;
				}
				thumbBounds = new Rectangle(clientRectangle.Left, thumbStart, clientRectangle.Width, thumbLength);
			}
			a._bounds = thumbBounds;
			ApplyPressedThumbState(ref a);
			_areas[AreaThumb] = a;
			if (a._isPresent)
			{
				a = new ScrollAreaState(ScrollAreaType.Page);
				if (thumbBounds.Top > rectangle.Top)
				{
					a._bounds = new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, thumbBounds.Top - rectangle.Top);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isPresent = false;
				}
				if (AreaLowPage == _pressedArea)
				{
					a._isPressed = true;
				}
				_areas[AreaLowPage] = a;
				a = new ScrollAreaState(ScrollAreaType.Page);
				if (thumbBounds.Bottom < rectangle.Bottom)
				{
					a._bounds = new Rectangle(rectangle.Left, thumbBounds.Bottom, rectangle.Width, rectangle.Bottom - thumbBounds.Bottom);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isPresent = false;
				}
				if (AreaHighPage == _pressedArea)
				{
					a._isPressed = true;
				}
				_areas[AreaHighPage] = a;
			}
			else
			{
				a = new ScrollAreaState(ScrollAreaType.Page);
				a._bounds = rectangle;
				_areas[AreaLowPage] = a;
				a = new ScrollAreaState(ScrollAreaType.Page);
				a._isPresent = false;
				_areas[AreaHighPage] = a;
			}
		}

		private void LayoutHorizontalThumb()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = new Rectangle(_areas[AreaLowArrow]._bounds.Right, clientRectangle.Top, _areas[AreaHighArrow]._bounds.Left - _areas[AreaLowArrow]._bounds.Right, clientRectangle.Height);
			ScrollAreaState a = new ScrollAreaState(ScrollAreaType.Thumb);
			Rectangle thumbBounds;
			if (rectangle.Width < MinThumbSize || !base.Enabled)
			{
				thumbBounds = Rectangle.Empty;
				a._isPresent = false;
			}
			else
			{
				int thumbLength = 0;
				if (_largeChange != 0)
				{
					float pageRatio = (float)_largeChange / (float)GetRange();
					thumbLength = (int)((float)rectangle.Width * pageRatio);
					if (thumbLength < MinThumbSize)
					{
						thumbLength = MinThumbSize;
					}
				}
				else
				{
					thumbLength = SystemInformation.HorizontalScrollBarThumbWidth;
				}
				if (GetRange() <= 0 || _maximum <= _minimum)
				{
					thumbLength = rectangle.Width;
				}
				float thumbOffset = 0f;
				if (GetRange() - _largeChange > 0)
				{
					thumbOffset = (float)(_value - _minimum) / (float)(GetRange() - _largeChange);
					thumbOffset *= (float)(rectangle.Width - thumbLength);
				}
				else
				{
					thumbLength = rectangle.Width;
					thumbOffset = 0f;
				}
				int thumbStart = rectangle.Left;
				if (_value > _minimum)
				{
					thumbStart += (int)thumbOffset;
				}
				if (thumbStart < rectangle.Left)
				{
					thumbStart = rectangle.Left;
				}
				if (_value == GetMaxValue() || thumbStart + thumbLength > rectangle.Right)
				{
					thumbStart = rectangle.Right - thumbLength;
				}
				thumbBounds = new Rectangle(thumbStart, base.ClientRectangle.Top, thumbLength, base.ClientRectangle.Height);
			}
			a._bounds = thumbBounds;
			ApplyPressedThumbState(ref a);
			_areas[AreaThumb] = a;
			if (a._isPresent)
			{
				a = new ScrollAreaState(ScrollAreaType.Page);
				if (thumbBounds.Left > rectangle.Left)
				{
					a._bounds = new Rectangle(rectangle.Left, rectangle.Top, thumbBounds.Left - rectangle.Left, rectangle.Height);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isPresent = false;
				}
				if (AreaLowPage == _pressedArea)
				{
					a._isPressed = true;
				}
				_areas[AreaLowPage] = a;
				a = new ScrollAreaState(ScrollAreaType.Page);
				if (thumbBounds.Right < rectangle.Right)
				{
					a._bounds = new Rectangle(thumbBounds.Right, rectangle.Top, rectangle.Right - thumbBounds.Right, rectangle.Height);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isPresent = false;
				}
				if (AreaHighPage == _pressedArea)
				{
					a._isPressed = true;
				}
				_areas[AreaHighPage] = a;
			}
			else
			{
				a = new ScrollAreaState(ScrollAreaType.Page);
				a._bounds = rectangle;
				_areas[AreaLowPage] = a;
				a = new ScrollAreaState(ScrollAreaType.Page);
				a._isPresent = false;
				_areas[AreaHighPage] = a;
			}
		}

		// Without an appearance the bar paints a plain background, so a pressed thumb needs no state.
		private void ApplyPressedThumbState(ref ScrollAreaState thumb)
		{
			if (_pressedArea != AreaThumb || _appearance == null)
			{
				return;
			}
			if (_appearance.ButtonAppearance == ButtonAppearance.PopUp)
			{
				thumb._isHot = true;
			}
			else if (IsHoverHighlightEnabled())
			{
				thumb._isPressed = true;
			}
		}

		private bool IsHoverHighlightEnabled()
		{
			return _appearance != null && _appearance.ShouldHighlightOnHover(GetUseVisualStyles() && VisualStyleSupport.IsThemingUsable(false));
		}

		private int GetRange()
		{
			return _maximum - _minimum + 1;
		}

		private int GetMaxValue()
		{
			return _maximum - _largeChange + 1;
		}

		internal void RefreshThumb()
		{
			try
			{
				if (_scrollBarType == ScrollBarType.Horizontal)
				{
					LayoutHorizontalThumb();
				}
				else
				{
					LayoutVerticalThumb();
				}
				Invalidate();
				Update();
			}
			catch (Exception ex) when (ex is ArithmeticException || ex is ObjectDisposedException)
			{
				// Best effort: an empty range or a closing control keeps the old thumb.
			}
		}

		internal bool GetUseVisualStyles()
		{
			return _useVisualStyles;
		}

		internal void SetUseVisualStyles(bool useVisualStyles)
		{
			if (useVisualStyles != _useVisualStyles)
			{
				_useVisualStyles = useVisualStyles;
				Invalidate();
			}
		}

		private void PaintScrollBar(Graphics graphics)
		{
			PaintDefaultTheme(graphics);
		}

		private int ClampValue(int value)
		{
			if (value < _minimum)
			{
				value = _minimum;
			}
			if (value > GetMaxValue())
			{
				value = GetMaxValue();
			}
			return value;
		}

		private void RaiseScrollForArea(int area)
		{
			int newValue = _value;
			ScrollEventArgs e;
			switch (area)
			{
			default:
				return;
			case AreaLowArrow:
				newValue = ClampValue(_value - _smallChange);
				e = new ScrollEventArgs(ScrollEventType.SmallDecrement, newValue);
				break;
			case AreaHighArrow:
				newValue = ClampValue(_value + _smallChange);
				e = new ScrollEventArgs(ScrollEventType.SmallIncrement, newValue);
				break;
			case AreaLowPage:
				newValue = ClampValue(_value - _largeChange);
				e = new ScrollEventArgs(ScrollEventType.LargeDecrement, newValue);
				break;
			case AreaHighPage:
				newValue = ClampValue(_value + _largeChange);
				e = new ScrollEventArgs(ScrollEventType.LargeIncrement, newValue);
				break;
			case AreaThumb:
				e = new ScrollEventArgs(ScrollEventType.ThumbTrack, _value);
				break;
			case ActionEndScroll:
				e = new ScrollEventArgs(ScrollEventType.EndScroll, _value);
				break;
			}
			OnScroll(e);
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (_isDraggingThumb)
			{
				TrackThumb(eventArgs.X, eventArgs.Y);
				return;
			}
			if (eventArgs.Button == MouseButtons.Left && _pressedArea != NoArea)
			{
				int areaUnderPointer = HitTestArea(eventArgs.X, eventArgs.Y);
				if (areaUnderPointer != _pressedArea)
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

		private void TrackThumb(int x, int y)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (_pressedArea != AreaThumb)
			{
				return;
			}
			if (_scrollBarType == ScrollBarType.Vertical)
			{
				if (Rectangle.Inflate(clientRectangle, clientRectangle.Width * DragCancelBarFactor, SystemInformation.VerticalScrollBarArrowHeight * DragCancelArrowFactor).Contains(x, y))
				{
					int trackLength = _areas[AreaHighArrow]._bounds.Top - _areas[AreaLowArrow]._bounds.Bottom - _areas[AreaThumb]._bounds.Height;
					int thumbPosition = y - _thumbGrabOffset - _areas[AreaLowArrow]._bounds.Bottom;
					int newValue = ValueFromThumbPosition(thumbPosition, trackLength);
					if (newValue < _minimum)
					{
						newValue = _minimum;
					}
					if (newValue > GetMaxValue())
					{
						newValue = GetMaxValue();
					}
					if (newValue != _value)
					{
						_value = newValue;
						RefreshThumb();
						RaiseScrollForArea(AreaThumb);
					}
				}
				else
				{
					_value = _valueBeforeDrag;
					RefreshThumb();
					RaiseScrollForArea(AreaThumb);
				}
			}
			else if (Rectangle.Inflate(base.ClientRectangle, SystemInformation.HorizontalScrollBarArrowWidth * DragCancelArrowFactor, base.ClientRectangle.Height * DragCancelBarFactor).Contains(x, y))
			{
				int trackLength = _areas[AreaHighArrow]._bounds.Left - _areas[AreaLowArrow]._bounds.Right - _areas[AreaThumb]._bounds.Width;
				int thumbPosition = x - _thumbGrabOffset - _areas[AreaLowArrow]._bounds.Right;
				int newValue = ValueFromThumbPosition(thumbPosition, trackLength);
				if (newValue < _minimum)
				{
					newValue = _minimum;
				}
				if (newValue > GetMaxValue())
				{
					newValue = GetMaxValue();
				}
				if (newValue != _value)
				{
					_value = newValue;
					RefreshThumb();
					RaiseScrollForArea(AreaThumb);
				}
			}
			else
			{
				_value = _valueBeforeDrag;
				RefreshThumb();
				RaiseScrollForArea(AreaThumb);
			}
		}

		// trackLength is the distance the thumb can travel; the inverse of the thumb layout.
		private int ValueFromThumbPosition(int thumbPosition, int trackLength)
		{
			if (trackLength <= 0)
			{
				return _minimum;
			}
			if (thumbPosition < 0)
			{
				thumbPosition = 0;
			}
			if (thumbPosition > trackLength)
			{
				thumbPosition = trackLength;
			}
			double trackRatio = (double)thumbPosition / (double)trackLength;
			return _minimum + (int)Math.Round(((double)GetMaxValue() - _minimum) * trackRatio);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			OnMouseLeave(new EventArgs());
		}

		private int HitTestArea(int x, int y)
		{
			for (int i = 0; i < AreaCount; i++)
			{
				ScrollAreaState a = _areas[i];
				if (a._isPresent && a._bounds.Contains(x, y))
				{
					return i;
				}
			}
			return NoArea;
		}

		private void UpdateHotArea(bool clearHot, int x, int y)
		{
			if (!IsHoverHighlightEnabled())
			{
				return;
			}
			if (clearHot)
			{
				SetAreaHot(_hotArea, false);
				_hotArea = NoArea;
				return;
			}
			int areaUnderPointer = HitTestArea(x, y);
			if (areaUnderPointer != _hotArea)
			{
				SetAreaHot(_hotArea, false);
				_hotArea = areaUnderPointer;
				SetAreaHot(_hotArea, true);
			}
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			base.OnMouseEnter(eventArgs);
			SetIsMouseOver(true);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			SetIsMouseOver(false);
			if (_isDraggingThumb)
			{
				_isDraggingThumb = false;
				if (_pressedArea == AreaThumb && _value != _valueBeforeDrag)
				{
					RaiseScrollForArea(ActionEndScroll);
					OnValueChanged(new EventArgs());
				}
			}
			UpdateHotArea(true, 0, 0);
			SetAreaPressed(_pressedArea, false);
			_pressedArea = NoArea;
			_repeatTimer.Enabled = false;
		}

		private void SetAreaHot(int area, bool isHot)
		{
			if (area == NoArea || !base.Enabled)
			{
				return;
			}
			ScrollAreaState a = _areas[area];
			if (a._isHot != isHot && a._isPresent)
			{
				_areas[area]._isHot = isHot;
				if (IsHoverHighlightEnabled())
				{
					Invalidate(a._bounds);
					Update();
				}
				_hoverCheckTimer.Enabled = isHot;
			}
		}

		private void SetAreaPressed(int area, bool isPressed)
		{
			if (area == NoArea)
			{
				return;
			}
			ScrollAreaState a = _areas[area];
			if (a._isPressed != isPressed)
			{
				if (a._areaType != ScrollAreaType.Thumb || !isPressed)
				{
					_areas[area]._isPressed = isPressed;
				}
				if (a._isPresent)
				{
					Invalidate(_areas[area]._bounds);
					Update();
				}
			}
		}

		public void AddScrollHandler(ScrollEventHandler handlerToAdd)
		{
			ScrollEventHandler scrollEventHandler = _scrollHandler;
			ScrollEventHandler previousHandler;
			do
			{
				previousHandler = scrollEventHandler;
				ScrollEventHandler value = (ScrollEventHandler)Delegate.Combine(previousHandler, handlerToAdd);
				scrollEventHandler = Interlocked.CompareExchange(ref _scrollHandler, value, previousHandler);
			}
			while ((object)scrollEventHandler != previousHandler);
		}

		protected virtual void OnScroll(ScrollEventArgs eventArgs)
		{
			if (_scrollHandler != null)
			{
				_scrollHandler(this, eventArgs);
			}
			SetValue(eventArgs.NewValue);
		}

		public void AddValueChangedHandler(EventHandler handlerToAdd)
		{
			EventHandler eventHandler = _valueChangedHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(previousHandler, handlerToAdd);
				eventHandler = Interlocked.CompareExchange(ref _valueChangedHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		protected virtual void OnValueChanged(EventArgs eventArgs)
		{
			if (_valueChangedHandler != null)
			{
				_valueChangedHandler(this, eventArgs);
			}
		}

		internal void ForwardMouseDown(MouseEventArgs eventArgs, int x, int y)
		{
			eventArgs = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, x, y, eventArgs.Delta);
			OnMouseDown(eventArgs);
			RenderingHelper.SetMouseCapture(this, true);
		}

		protected override void OnEnabledChanged(EventArgs eventArgs)
		{
			base.OnEnabledChanged(eventArgs);
			SetAreaHot(_hotArea, false);
			SetAreaPressed(_pressedArea, false);
			LayoutAreas();
		}
	}
}
