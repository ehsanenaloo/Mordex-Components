using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Timeline
{
	internal partial class StyledScrollBar : Control
	{
		private enum ScrollAreaType
		{
			NotSet,
			Arrow,
			Thumb,
			Page,
			MoveFirst,
			MoveLast
		}

		private struct ScrollBarStyleControl2Data
		{
			internal bool _isPressed;

			internal bool _isHovered;

			internal Rectangle _bounds;

			internal bool _isVisible;

			internal ScrollAreaType _type;

			internal ScrollBarStyleControl2Data(ScrollAreaType type)
			{
				_isPressed = false;
				_isHovered = false;
				_bounds = Rectangle.Empty;
				_type = type;
				_isVisible = true;
			}
		}

		private System.Windows.Forms.Timer _repeatTimer;

		private ImageList _imageList;

		private IContainer _iContainer;

		private System.Windows.Forms.Timer _scrollBarStyleControl2Timer;

		internal bool _shouldClampScrollValue = true;

		private ScrollBarStyleControl2Data[] _scrollBarStyleControl2DataArray = new ScrollBarStyleControl2Data[10];

		private ScrollBarType _type;

		private bool _isMouseOver;

		private int _largeChange = 1;

		private int _minimum;

		private int _valueScale = 1;

		private int _maximum = int.MaxValue;

		internal int _scrollValue;

		private int _smallChange = 1;

		private bool _isDraggingThumb;

		private int _thumbGrabOffset;

		private int _valueAtDragStart;

		private int _pressedAreaIndex = -1;

		private int _hoveredAreaIndex = -1;

		private static readonly object _value = new object();

		private static readonly object _scrollBarStyleControl2Value = new object();

		private static readonly object _findPreviousEventKey = new object();

		private ScrollEventHandler _scrollEventHandler;

		public bool _isClampAllowed;

		private EventHandler _eventHandler;

		internal ScrollBarStyle _scrollBarStyle;

		private void InitializeComponents()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			_iContainer = new Container();
			ResourceManager resourceManager = new ResourceManager(typeof(StyledScrollBar));
			_repeatTimer = new System.Windows.Forms.Timer(_iContainer);
			_scrollBarStyleControl2Timer = new System.Windows.Forms.Timer(_iContainer);
			_imageList = new ImageList(_iContainer);
			_repeatTimer.Tick += OnTimerTick;
			_scrollBarStyleControl2Timer.Interval = 50;
			_scrollBarStyleControl2Timer.Tick += OnHoverTimerTick;
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(15, 15);
			ImageStrip.Load(_imageList, resourceManager, "imgScroll.ImageStrip");
			_imageList.TransparentColor = Color.Magenta;
		}

		internal StyledScrollBar(ScrollBarType type)
		{
			_type = type;
			InitializeComponents();
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			if (_scrollBarStyle == null)
			{
				pe.Graphics.FillRectangle(SystemBrushes.Control, base.ClientRectangle);
			}
			else
			{
				RenderContent(pe.Graphics);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
		}

		protected override void Dispose(bool disposing)
		{
			StopThemeListening();
			base.Dispose(disposing);
		}

		public bool IsMouseOver()
		{
			return _isMouseOver;
		}

		public void UpdateMouseOver(bool scrollBarStyleControl2Boolean)
		{
			if (_isMouseOver != scrollBarStyleControl2Boolean)
			{
				_isMouseOver = scrollBarStyleControl2Boolean;
				Invalidate();
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}
			_pressedAreaIndex = HitTestArea(eventArgs.X, eventArgs.Y);
			SetAreaPressed(_pressedAreaIndex, true);
			if (_pressedAreaIndex == -1)
			{
				return;
			}
			int pressedAreaIndex = _pressedAreaIndex;
			if (pressedAreaIndex == 2)
			{
				_isDraggingThumb = true;
				OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, _scrollValue));
				_valueAtDragStart = _scrollValue;
				if (_type == ScrollBarType.Vertical)
				{
					_thumbGrabOffset = eventArgs.Y - _scrollBarStyleControl2DataArray[2]._bounds.Top;
				}
				else
				{
					_thumbGrabOffset = eventArgs.X - _scrollBarStyleControl2DataArray[2]._bounds.Left;
				}
			}
			else
			{
				RaiseScrollForArea(_pressedAreaIndex);
				_repeatTimer.Interval = 500;
				_repeatTimer.Enabled = true;
			}
		}

		protected void OnTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				if (_pressedAreaIndex != -1)
				{
					Point position = Cursor.Position;
					position = PointToClient(position);
					OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
					if (_scrollBarStyleControl2DataArray[_pressedAreaIndex]._isPressed)
					{
						RaiseScrollForArea(_pressedAreaIndex);
					}
				}
				if (_repeatTimer.Interval != 30)
				{
					_repeatTimer.Interval = 30;
				}
			}
			catch
			{
			}
		}

		protected void OnHoverTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				Point position = Cursor.Position;
				position = PointToClient(position);
				if (!base.ClientRectangle.Contains(position))
				{
					UpdateHoveredArea(true, 0, 0);
				}
				_scrollBarStyleControl2Timer.Enabled = false;
			}
			catch
			{
			}
		}

		// The system sizes of the arrows are for the system DPI; scale them with the thickness of the bar at the DPI of this window.
		private static int ScaleToDpi(int value, int systemThickness, int deviceThickness)
		{
			return systemThickness <= 0 ? value : (int)Math.Round(value * (double)deviceThickness / systemThickness);
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			if (_type == ScrollBarType.Vertical)
			{
				int scrollBarWidth = SystemInformation.GetVerticalScrollBarWidthForDpi(DeviceDpi);
				if (base.Width != scrollBarWidth)
				{
					base.Width = scrollBarWidth;
					return;
				}
			}
			else
			{
				int scrollBarHeight = SystemInformation.GetHorizontalScrollBarHeightForDpi(DeviceDpi);
				if (base.Height != scrollBarHeight)
				{
					base.Height = scrollBarHeight;
					return;
				}
			}
			RecalculateAreaBounds();
		}

		public int GetLargeChange()
		{
			return _largeChange;
		}

		public void UpdateLargeChange(int largeChange)
		{
			largeChange *= _valueScale;
			if (largeChange < 0)
			{
				largeChange = 0;
			}
			if (largeChange != _largeChange)
			{
				_largeChange = largeChange;
				RecalculateAreaBounds();
			}
		}

		public int GetMinimum()
		{
			return _minimum;
		}

		public void UpdateMinimum(int minimum)
		{
			minimum *= _valueScale;
			if (minimum != _minimum)
			{
				_minimum = minimum;
				if (_scrollValue < _minimum)
				{
					UpdateScrollValue(_minimum);
				}
				RecalculateAreaBounds();
			}
		}

		public int GetValueScale()
		{
			return _valueScale;
		}

		public void SetValueScale(int valueScale)
		{
			_valueScale = valueScale;
		}

		public void UpdateMaximum(int maximum)
		{
			maximum *= _valueScale;
			if (maximum != _maximum)
			{
				_maximum = maximum;
				if (_scrollValue > GetMaxScrollValue())
				{
					UpdateScrollValue(GetMaxScrollValue());
				}
				RecalculateAreaBounds();
			}
		}

		public int GetScrollValue()
		{
			return _scrollValue;
		}

		public void UpdateScrollValue(int scrollValue)
		{
			if (scrollValue < _minimum)
			{
				scrollValue = _minimum;
			}
			if (scrollValue > GetMaxScrollValue())
			{
				scrollValue = GetMaxScrollValue();
			}
			if (scrollValue != _scrollValue)
			{
				_scrollValue = scrollValue;
				OnValueChanged(new EventArgs());
				RefreshThumbLayout();
			}
		}

		internal void SetScrollValueWithoutNotify(int scrollValue)
		{
			if (scrollValue < _minimum)
			{
				scrollValue = _minimum;
			}
			if (scrollValue > GetMaxScrollValue())
			{
				scrollValue = GetMaxScrollValue();
			}
			if (_scrollValue != scrollValue)
			{
				_scrollValue = scrollValue;
				RefreshThumbLayout();
			}
		}

		public int GetSmallChange()
		{
			return _smallChange;
		}

		public void UpdateSmallChange(int smallChange)
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

		private void RecalculateAreaBounds()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			int barWidth = 0;
			int barHeight = 0;
			if (_type == ScrollBarType.Vertical)
			{
				barWidth = SystemInformation.GetVerticalScrollBarWidthForDpi(DeviceDpi);
				barHeight = clientRectangle.Height;
				int verticalScrollBarArrowHeight = ScaleToDpi(SystemInformation.VerticalScrollBarArrowHeight, SystemInformation.VerticalScrollBarWidth, barWidth);
				ScrollBarStyleControl2Data a = new ScrollBarStyleControl2Data(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, barWidth, verticalScrollBarArrowHeight);
				_scrollBarStyleControl2DataArray[0] = a;
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Bottom - verticalScrollBarArrowHeight, barWidth, verticalScrollBarArrowHeight);
				_scrollBarStyleControl2DataArray[4] = a;
				LayoutVerticalThumb();
			}
			else
			{
				barWidth = clientRectangle.Width;
				barHeight = SystemInformation.GetHorizontalScrollBarHeightForDpi(DeviceDpi);
				int horizontalScrollBarArrowWidth = ScaleToDpi(SystemInformation.HorizontalScrollBarArrowWidth, SystemInformation.HorizontalScrollBarHeight, barHeight);
				int left = clientRectangle.Left;
				int x = clientRectangle.Right - horizontalScrollBarArrowWidth;
				ScrollBarStyleControl2Data a = new ScrollBarStyleControl2Data(ScrollAreaType.MoveFirst);
				a._bounds = Rectangle.Empty;
				_scrollBarStyleControl2DataArray[6] = a;
				a = new ScrollBarStyleControl2Data(ScrollAreaType.MoveLast);
				a._bounds = Rectangle.Empty;
				_scrollBarStyleControl2DataArray[7] = a;
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(left, clientRectangle.Top, horizontalScrollBarArrowWidth, barHeight);
				_scrollBarStyleControl2DataArray[0] = a;
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(x, clientRectangle.Top, horizontalScrollBarArrowWidth, barHeight);
				_scrollBarStyleControl2DataArray[4] = a;
				LayoutHorizontalThumb();
			}
			Invalidate();
		}

		private void LayoutVerticalThumb()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle field_ = new Rectangle(clientRectangle.Left, _scrollBarStyleControl2DataArray[0]._bounds.Bottom, clientRectangle.Width, _scrollBarStyleControl2DataArray[4]._bounds.Top - _scrollBarStyleControl2DataArray[0]._bounds.Bottom);
			ScrollBarStyleControl2Data a = new ScrollBarStyleControl2Data(ScrollAreaType.Thumb);
			Rectangle rectangle;
			if (!base.Enabled || field_.Height < 8)
			{
				rectangle = Rectangle.Empty;
				a._isVisible = false;
			}
			else
			{
				int thumbLength = 0;
				if (_largeChange > 0)
				{
					float visibleRatio = (float)_largeChange / (float)GetValueRange();
					thumbLength = (int)((float)field_.Height * visibleRatio);
					if (thumbLength < 8)
					{
						thumbLength = 8;
					}
				}
				else
				{
					thumbLength = SystemInformation.VerticalScrollBarThumbHeight;
				}
				if (GetValueRange() <= 0 || _maximum <= _minimum)
				{
					thumbLength = field_.Height;
				}
				float thumbOffset = 0f;
				if (GetValueRange() - _largeChange > 0)
				{
					thumbOffset = (float)(_scrollValue - _minimum) / (float)(GetValueRange() - _largeChange);
					thumbOffset *= (float)(field_.Height - thumbLength);
				}
				else
				{
					thumbLength = field_.Height;
					thumbOffset = 0f;
				}
				int thumbTop = field_.Top;
				if (_scrollValue > _minimum)
				{
					thumbTop += (int)thumbOffset;
				}
				if (thumbTop < field_.Top)
				{
					thumbTop = field_.Top;
				}
				if (_scrollValue == GetMaxScrollValue() || thumbTop + thumbLength > field_.Bottom)
				{
					thumbTop = field_.Bottom - thumbLength;
				}
				rectangle = new Rectangle(clientRectangle.Left, thumbTop, clientRectangle.Width, thumbLength);
			}
			a._bounds = rectangle;
			if (_pressedAreaIndex == 2)
			{
				if (_scrollBarStyle.ButtonAppearance == Appearance.PopUp)
				{
					a._isHovered = true;
				}
				else if (_scrollBarStyle.IsFlatOrThemed())
				{
					a._isPressed = true;
				}
			}
			_scrollBarStyleControl2DataArray[2] = a;
			if (a._isVisible)
			{
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Page);
				if (rectangle.Top > field_.Top)
				{
					a._bounds = new Rectangle(field_.Left, field_.Top, field_.Width, rectangle.Top - field_.Top);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isVisible = false;
				}
				if (1 == _pressedAreaIndex)
				{
					a._isPressed = true;
				}
				_scrollBarStyleControl2DataArray[1] = a;
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Page);
				if (rectangle.Bottom < field_.Bottom)
				{
					a._bounds = new Rectangle(field_.Left, rectangle.Bottom, field_.Width, field_.Bottom - rectangle.Bottom);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isVisible = false;
				}
				if (3 == _pressedAreaIndex)
				{
					a._isPressed = true;
				}
				_scrollBarStyleControl2DataArray[3] = a;
			}
			else
			{
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Page);
				a._bounds = field_;
				_scrollBarStyleControl2DataArray[1] = a;
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Page);
				a._isVisible = false;
				_scrollBarStyleControl2DataArray[3] = a;
			}
		}

		private void LayoutHorizontalThumb()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle field_ = new Rectangle(_scrollBarStyleControl2DataArray[0]._bounds.Right, clientRectangle.Top, _scrollBarStyleControl2DataArray[4]._bounds.Left - _scrollBarStyleControl2DataArray[0]._bounds.Right, clientRectangle.Height);
			ScrollBarStyleControl2Data a = new ScrollBarStyleControl2Data(ScrollAreaType.Thumb);
			Rectangle rectangle;
			if (field_.Width < 8 || !base.Enabled)
			{
				rectangle = Rectangle.Empty;
				a._isVisible = false;
			}
			else
			{
				int thumbLength = 0;
				if (_largeChange != 0)
				{
					float visibleRatio = (float)_largeChange / (float)GetValueRange();
					thumbLength = (int)((float)field_.Width * visibleRatio);
					if (thumbLength < 8)
					{
						thumbLength = 8;
					}
				}
				else
				{
					thumbLength = SystemInformation.HorizontalScrollBarThumbWidth;
				}
				if (GetValueRange() <= 0 || _maximum <= _minimum)
				{
					thumbLength = field_.Width;
				}
				float thumbOffset = 0f;
				if (GetValueRange() - _largeChange > 0)
				{
					thumbOffset = (float)(_scrollValue - _minimum) / (float)(GetValueRange() - _largeChange);
					thumbOffset *= (float)(field_.Width - thumbLength);
				}
				else
				{
					thumbLength = field_.Width;
					thumbOffset = 0f;
				}
				int thumbLeft = field_.Left;
				if (_scrollValue > _minimum)
				{
					thumbLeft += (int)thumbOffset;
				}
				if (thumbLeft < field_.Left)
				{
					thumbLeft = field_.Left;
				}
				if (_scrollValue == GetMaxScrollValue() || thumbLeft + thumbLength > field_.Right)
				{
					thumbLeft = field_.Right - thumbLength;
				}
				rectangle = new Rectangle(thumbLeft, base.ClientRectangle.Top, thumbLength, base.ClientRectangle.Height);
			}
			a._bounds = rectangle;
			if (_pressedAreaIndex == 2)
			{
				if (_scrollBarStyle.ButtonAppearance == Appearance.PopUp)
				{
					a._isHovered = true;
				}
				else if (_scrollBarStyle.IsFlatOrThemed())
				{
					a._isPressed = true;
				}
			}
			_scrollBarStyleControl2DataArray[2] = a;
			if (a._isVisible)
			{
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Page);
				if (rectangle.Left > field_.Left)
				{
					a._bounds = new Rectangle(field_.Left, field_.Top, rectangle.Left - field_.Left, field_.Height);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isVisible = false;
				}
				if (1 == _pressedAreaIndex)
				{
					a._isPressed = true;
				}
				_scrollBarStyleControl2DataArray[1] = a;
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Page);
				if (rectangle.Right < field_.Right)
				{
					a._bounds = new Rectangle(rectangle.Right, field_.Top, field_.Right - rectangle.Right, field_.Height);
				}
				else
				{
					a._bounds = Rectangle.Empty;
					a._isVisible = false;
				}
				if (3 == _pressedAreaIndex)
				{
					a._isPressed = true;
				}
				_scrollBarStyleControl2DataArray[3] = a;
			}
			else
			{
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Page);
				a._bounds = field_;
				_scrollBarStyleControl2DataArray[1] = a;
				a = new ScrollBarStyleControl2Data(ScrollAreaType.Page);
				a._isVisible = false;
				_scrollBarStyleControl2DataArray[3] = a;
			}
		}

		private int GetValueRange()
		{
			return _maximum - _minimum + 1;
		}

		internal int GetMaxScrollValue()
		{
			int maxScrollValue = _maximum - _largeChange + 1;
			if (maxScrollValue < 0)
			{
				return 0;
			}
			return maxScrollValue;
		}

		internal void RefreshThumbLayout()
		{
			if (base.Visible)
			{
				if (_type == ScrollBarType.Horizontal)
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
		}

		private void RenderContent(Graphics graphics)
		{
			RenderThemedScrollBar(graphics);
		}

		private int ClampToScrollRange(int scrollValue)
		{
			if (scrollValue < _minimum)
			{
				scrollValue = _minimum;
			}
			if (scrollValue > GetMaxScrollValue())
			{
				scrollValue = GetMaxScrollValue();
			}
			return scrollValue;
		}

		private void RaiseScrollForArea(int areaIndex)
		{
			int newValue = _scrollValue;
			ScrollEventArgs e;
			switch (areaIndex)
			{
			default:
				return;
			case 0:
				newValue = ClampToScrollRange(_scrollValue - _smallChange);
				e = new ScrollEventArgs(ScrollEventType.SmallDecrement, newValue);
				break;
			case 4:
				newValue = ClampToScrollRange(_scrollValue + _smallChange);
				e = new ScrollEventArgs(ScrollEventType.SmallIncrement, newValue);
				break;
			case 1:
				newValue = ClampToScrollRange(_scrollValue - _largeChange);
				e = new ScrollEventArgs(ScrollEventType.LargeDecrement, newValue);
				break;
			case 3:
				newValue = ClampToScrollRange(_scrollValue + _largeChange);
				e = new ScrollEventArgs(ScrollEventType.LargeIncrement, newValue);
				break;
			case 2:
				e = new ScrollEventArgs(ScrollEventType.ThumbTrack, _scrollValue);
				break;
			case 5:
				e = new ScrollEventArgs(ScrollEventType.EndScroll, _scrollValue);
				break;
			case 7:
				OnScrollFindNext(EventArgs.Empty);
				return;
			case 6:
				OnScrollFindPrevious(EventArgs.Empty);
				return;
			}
			OnScroll(e);
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (_isDraggingThumb)
			{
				BeginPressAt(eventArgs.X, eventArgs.Y);
				return;
			}
			if (eventArgs.Button == MouseButtons.Left && _pressedAreaIndex != -1)
			{
				int hitAreaIndex = HitTestArea(eventArgs.X, eventArgs.Y);
				if (hitAreaIndex != _pressedAreaIndex)
				{
					SetAreaPressed(_pressedAreaIndex, false);
				}
				else
				{
					SetAreaPressed(_pressedAreaIndex, true);
				}
			}
			if (eventArgs.Button == MouseButtons.None)
			{
				UpdateHoveredArea(false, eventArgs.X, eventArgs.Y);
			}
		}

		private void BeginPressAt(int x, int integerArgument)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (_pressedAreaIndex != 2)
			{
				return;
			}
			if (_type == ScrollBarType.Vertical)
			{
				if (Rectangle.Inflate(clientRectangle, clientRectangle.Width * 8, SystemInformation.VerticalScrollBarArrowHeight * 2).Contains(x, integerArgument))
				{
					int trackLength = _scrollBarStyleControl2DataArray[4]._bounds.Top - _scrollBarStyleControl2DataArray[0]._bounds.Bottom;
					int thumbOffset = integerArgument - _thumbGrabOffset - _scrollBarStyleControl2DataArray[0]._bounds.Bottom;
					if (thumbOffset < 0)
					{
						thumbOffset = 0;
					}
					if (thumbOffset > trackLength)
					{
						thumbOffset = trackLength;
					}
					float thumbFraction = (float)thumbOffset / (float)trackLength;
					int newValue = _minimum + (int)Math.Ceiling((float)(_maximum - _minimum) * thumbFraction);
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
						RefreshThumbLayout();
						RaiseScrollForArea(2);
					}
				}
				else
				{
					_scrollValue = _valueAtDragStart;
					RefreshThumbLayout();
					RaiseScrollForArea(2);
				}
			}
			else if (Rectangle.Inflate(base.ClientRectangle, SystemInformation.HorizontalScrollBarArrowWidth * 2, base.ClientRectangle.Height * 8).Contains(x, integerArgument))
			{
				int scrollTrackLength = _scrollBarStyleControl2DataArray[4]._bounds.Left - _scrollBarStyleControl2DataArray[0]._bounds.Right;
				int scrollThumbOffset = x - _thumbGrabOffset - _scrollBarStyleControl2DataArray[0]._bounds.Right;
				if (scrollThumbOffset < 0)
				{
					scrollThumbOffset = 0;
				}
				if (scrollThumbOffset > scrollTrackLength)
				{
					scrollThumbOffset = scrollTrackLength;
				}
				float scrollThumbFraction = (float)scrollThumbOffset / (float)scrollTrackLength;
				int scrollNewValue = _minimum + (int)Math.Ceiling((float)(_maximum - _minimum) * scrollThumbFraction);
				if (scrollNewValue < _minimum)
				{
					scrollNewValue = _minimum;
				}
				if (scrollNewValue > GetMaxScrollValue())
				{
					scrollNewValue = GetMaxScrollValue();
				}
				if (scrollNewValue != _scrollValue)
				{
					_scrollValue = scrollNewValue;
					RefreshThumbLayout();
					RaiseScrollForArea(2);
				}
			}
			else
			{
				_scrollValue = _valueAtDragStart;
				RefreshThumbLayout();
				RaiseScrollForArea(2);
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			OnMouseLeave(new EventArgs());
			ClampScrollValue();
		}

		internal void ClampScrollValue()
		{
			if (_isClampAllowed && _shouldClampScrollValue)
			{
				if (_scrollValue < _minimum + _largeChange)
				{
					_scrollValue = _minimum + _largeChange;
					OnUpdatePosition(EventArgs.Empty);
					RefreshThumbLayout();
				}
				if (_scrollValue > GetMaxScrollValue() - _largeChange)
				{
					_scrollValue = GetMaxScrollValue() - _largeChange;
					OnUpdatePosition(EventArgs.Empty);
					RefreshThumbLayout();
				}
			}
		}

		private int HitTestArea(int x, int integerArgument)
		{
			int firstIndex = 0;
			int lastIndex = 4;
			for (int i = firstIndex; i <= lastIndex; i++)
			{
				ScrollBarStyleControl2Data a = _scrollBarStyleControl2DataArray[i];
				if (a._isVisible && a._bounds.Contains(x, integerArgument))
				{
					return i;
				}
			}
			return -1;
		}

		private void UpdateHoveredArea(bool shouldClearHover, int x, int integerArgument)
		{
			if (!_scrollBarStyle.IsFlatOrThemed())
			{
				return;
			}
			if (shouldClearHover)
			{
				UpdateLayout(_hoveredAreaIndex, false);
				_hoveredAreaIndex = -1;
				return;
			}
			int hitAreaIndex = HitTestArea(x, integerArgument);
			if (hitAreaIndex != _hoveredAreaIndex)
			{
				UpdateLayout(_hoveredAreaIndex, false);
				_hoveredAreaIndex = hitAreaIndex;
				UpdateLayout(_hoveredAreaIndex, true);
			}
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			base.OnMouseEnter(eventArgs);
			UpdateMouseOver(true);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			UpdateMouseOver(false);
			if (_isDraggingThumb)
			{
				_isDraggingThumb = false;
				if (_pressedAreaIndex == 2 && _scrollValue != _valueAtDragStart)
				{
					RaiseScrollForArea(5);
					OnValueChanged(new EventArgs());
				}
			}
			UpdateHoveredArea(true, 0, 0);
			SetAreaPressed(_pressedAreaIndex, false);
			_pressedAreaIndex = -1;
			_repeatTimer.Enabled = false;
		}

		private void UpdateLayout(int areaIndex, bool scrollBarStyleControl2DataBoolean)
		{
			if (areaIndex == -1 || !base.Enabled)
			{
				return;
			}
			ScrollBarStyleControl2Data a = _scrollBarStyleControl2DataArray[areaIndex];
			if (a._isHovered != scrollBarStyleControl2DataBoolean && a._isVisible)
			{
				_scrollBarStyleControl2DataArray[areaIndex]._isHovered = scrollBarStyleControl2DataBoolean;
				if (_scrollBarStyle != null && _scrollBarStyle.IsFlatOrThemed())
				{
					Invalidate(a._bounds);
					Update();
				}
				_scrollBarStyleControl2Timer.Enabled = scrollBarStyleControl2DataBoolean;
			}
		}

		private void SetAreaPressed(int areaIndex, bool isPressed)
		{
			if (areaIndex == -1)
			{
				return;
			}
			ScrollBarStyleControl2Data a = _scrollBarStyleControl2DataArray[areaIndex];
			if (a._isPressed != isPressed)
			{
				if (a._type != ScrollAreaType.Thumb || !isPressed)
				{
					_scrollBarStyleControl2DataArray[areaIndex]._isPressed = isPressed;
				}
				if (a._isVisible)
				{
					Invalidate(_scrollBarStyleControl2DataArray[areaIndex]._bounds);
					Update();
				}
			}
		}

		internal void AddUpdatePositionHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_value, eventHandler);
		}

		protected internal virtual void OnUpdatePosition(EventArgs eventArgs)
		{
			if (base.Events[_value] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		public void AddFindNextHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_scrollBarStyleControl2Value, eventHandler);
		}

		protected virtual void OnScrollFindNext(EventArgs eventArgs)
		{
			if (base.Events[_scrollBarStyleControl2Value] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		public void AddFindPreviousHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_findPreviousEventKey, eventHandler);
		}

		protected virtual void OnScrollFindPrevious(EventArgs eventArgs)
		{
			if (base.Events[_findPreviousEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		public void AddScrollHandler(ScrollEventHandler scrollEventHandlerArgument)
		{
			ScrollEventHandler scrollEventHandler = _scrollEventHandler;
			ScrollEventHandler previousHandler;
			do
			{
				previousHandler = scrollEventHandler;
				ScrollEventHandler value = (ScrollEventHandler)Delegate.Combine(previousHandler, scrollEventHandlerArgument);
				scrollEventHandler = Interlocked.CompareExchange(ref _scrollEventHandler, value, previousHandler);
			}
			while ((object)scrollEventHandler != previousHandler);
		}

		protected virtual void OnScroll(ScrollEventArgs eventArgs)
		{
			if (_scrollEventHandler != null)
			{
				_scrollEventHandler(this, eventArgs);
			}
			UpdateScrollValue(eventArgs.NewValue);
		}

		public void AddValueChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _eventHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(previousHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _eventHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		protected virtual void OnValueChanged(EventArgs eventArgs)
		{
			if (_eventHandler != null)
			{
				_eventHandler(this, eventArgs);
			}
		}

		protected override void OnEnabledChanged(EventArgs eventArgs)
		{
			base.OnEnabledChanged(eventArgs);
			UpdateLayout(_hoveredAreaIndex, false);
			SetAreaPressed(_pressedAreaIndex, false);
			RecalculateAreaBounds();
		}
	}
}
