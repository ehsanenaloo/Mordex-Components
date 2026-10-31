using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.TaskPane.Internal;

namespace Mordex.Windows.TaskPane
{
	internal class ControlStyleControl : Control
	{
		private enum ScrollAreaType
		{
			NotSet,
			Arrow,
			Thumb,
			Page
		}

		private struct ScrollAreaData
		{
			internal bool _isPressed;

			internal bool _isHot;

			internal Rectangle _bounds;

			internal bool _isVisible;

			internal ScrollAreaType _type;

			internal ScrollAreaData(ScrollAreaType type)
			{
				_isPressed = false;
				_isHot = false;
				_bounds = Rectangle.Empty;
				_type = type;
				_isVisible = true;
			}
		}

		private System.Windows.Forms.Timer _timer;

		private System.Windows.Forms.Timer _hoverTimer;

		private ScrollAreaData[] _areas = new ScrollAreaData[10];

		private AccessibleObject _accessibleObject;

		private ScrollBarType _type;

		private int _largeChange = 1;

		private int _minimum;

		private int _maximum = int.MaxValue;

		private int _value;

		private int _smallChange = 1;

		private bool _isMouseOver;

		private bool _isDraggingThumb;

		private int _thumbGrabOffset;

		private int _dragStartValue;

		internal int _pressedArea = -1;

		private int _hotArea = -1;

		private ScrollEventHandler _scrollHandler;

		private EventHandler _valueChangedHandler;

		private ControlStyle _controlStyle;

		private void InitializeControl()
		{
			_timer = new System.Windows.Forms.Timer();
			_timer.Tick += OnTimerTick;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			_hoverTimer = new System.Windows.Forms.Timer();
			_hoverTimer.Enabled = false;
			_hoverTimer.Interval = 50;
			_hoverTimer.Tick += OnHoverTimerTick;
			_controlStyle = new ControlStyle(this);
		}

		internal ControlStyleControl(ScrollBarType type)
		{
			_type = type;
			InitializeControl();
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			if (_controlStyle == null)
			{
				pe.Graphics.FillRectangle(SystemBrushes.Control, base.ClientRectangle);
			}
			else
			{
				RenderScrollBar(pe.Graphics);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timer?.Dispose();
				_hoverTimer?.Dispose();
				_controlStyle?.DisposeAllResources();
			}
			base.Dispose(disposing);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new ScrollBarAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal AccessibleStates ComputeAccessibleStates(int areaIndex)
		{
			if (!_areas[areaIndex]._isVisible)
			{
				return AccessibleStates.Invisible;
			}
			if (_areas[areaIndex]._isPressed)
			{
				return AccessibleStates.Pressed;
			}
			return AccessibleStates.None;
		}

		internal Rectangle GetAreaScreenBounds(int areaIndex)
		{
			Rectangle areaBounds = _areas[areaIndex]._bounds;
			areaBounds.Location = PointToScreen(areaBounds.Location);
			return areaBounds;
		}

		internal void InvokeAreaAction(int areaIndex)
		{
			RaiseAreaScroll(areaIndex);
		}

		public ScrollBarType GetScrollBarType()
		{
			return _type;
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
			if (_pressedArea == -1)
			{
				return;
			}
			int hitArea = _pressedArea;
			if (hitArea == 2)
			{
				_isDraggingThumb = true;
				OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, _value));
				_dragStartValue = _value;
				if (_type == ScrollBarType.Vertical)
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
				RaiseAreaScroll(_pressedArea);
				_timer.Interval = 500;
				_timer.Enabled = true;
			}
		}

		protected void OnTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				if (_pressedArea != -1)
				{
					Point position = PointerPosition.Current;
					position = PointToClient(position);
					OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
					if (_areas[_pressedArea]._isPressed)
					{
						RaiseAreaScroll(_pressedArea);
					}
				}
				if (_timer.Interval != 30)
				{
					_timer.Interval = 30;
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
		}

		protected void OnHoverTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				Point position = PointerPosition.Current;
				position = PointToClient(position);
				if (!base.ClientRectangle.Contains(position))
				{
					UpdateHotArea(true, 0, 0);
				}
				_hoverTimer.Enabled = false;
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			if (_type == ScrollBarType.Vertical)
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
			LayoutAllAreas();
		}

		public int GetLargeChange()
		{
			return _largeChange;
		}

		public void UpdateLargeChange(int largeChange)
		{
			if (largeChange < 0)
			{
				largeChange = 0;
			}
			if (largeChange != _largeChange)
			{
				_largeChange = largeChange;
				LayoutAllAreas();
			}
		}

		public void UpdateMinimum(int minimum)
		{
			if (minimum != _minimum)
			{
				_minimum = minimum;
				if (_value < _minimum)
				{
					UpdateValue(_minimum);
				}
				LayoutAllAreas();
			}
		}

		public int GetMaximum()
		{
			return _maximum;
		}

		public void UpdateMaximum(int maximum)
		{
			if (maximum != _maximum)
			{
				_maximum = maximum;
				if (_value > ComputeMaxValue())
				{
					UpdateValue(ComputeMaxValue());
				}
				LayoutAllAreas();
			}
		}

		public int GetValue()
		{
			return _value;
		}

		public void UpdateValue(int value)
		{
			if (value < _minimum)
			{
				value = _minimum;
			}
			if (value > ComputeMaxValue())
			{
				value = ComputeMaxValue();
			}
			if (value != _value)
			{
				_value = value;
				OnValueChanged(new EventArgs());
				RelayoutIfVisible();
			}
		}

		private void LayoutAllAreas()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			int barWidth = 0;
			int barHeight = 0;
			if (_type == ScrollBarType.Vertical)
			{
				barWidth = SystemInformation.VerticalScrollBarWidth;
				barHeight = clientRectangle.Height;
				int verticalScrollBarArrowHeight = SystemInformation.VerticalScrollBarArrowHeight;
				ScrollAreaData area = new ScrollAreaData(ScrollAreaType.Arrow);
				area._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, barWidth, verticalScrollBarArrowHeight);
				_areas[0] = area;
				area = new ScrollAreaData(ScrollAreaType.Arrow);
				area._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Bottom - verticalScrollBarArrowHeight, barWidth, verticalScrollBarArrowHeight);
				_areas[4] = area;
				LayoutVerticalAreas();
			}
			else
			{
				barWidth = clientRectangle.Width;
				barHeight = SystemInformation.HorizontalScrollBarHeight;
				int horizontalScrollBarArrowWidth = SystemInformation.HorizontalScrollBarArrowWidth;
				ScrollAreaData area = new ScrollAreaData(ScrollAreaType.Arrow);
				area._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, horizontalScrollBarArrowWidth, barHeight);
				_areas[0] = area;
				area = new ScrollAreaData(ScrollAreaType.Arrow);
				area._bounds = new Rectangle(clientRectangle.Right - horizontalScrollBarArrowWidth, clientRectangle.Top, horizontalScrollBarArrowWidth, barHeight);
				_areas[4] = area;
				LayoutHorizontalAreas();
			}
			Invalidate();
		}

		private void LayoutVerticalAreas()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle trackBounds = new Rectangle(clientRectangle.Left, _areas[0]._bounds.Bottom, clientRectangle.Width, _areas[4]._bounds.Top - _areas[0]._bounds.Bottom);
			ScrollAreaData area = new ScrollAreaData(ScrollAreaType.Thumb);
			Rectangle thumbBounds;
			if (!base.Enabled || trackBounds.Height < 8)
			{
				thumbBounds = Rectangle.Empty;
				area._isVisible = false;
			}
			else
			{
				int thumbLength = 0;
				if (_largeChange > 0)
				{
					float thumbRatio = (float)_largeChange / (float)ComputeValueRange();
					thumbLength = (int)((float)trackBounds.Height * thumbRatio);
					if (thumbLength < 8)
					{
						thumbLength = 8;
					}
				}
				else
				{
					thumbLength = SystemInformation.VerticalScrollBarThumbHeight;
				}
				if (ComputeValueRange() <= 0 || _maximum <= _minimum)
				{
					thumbLength = trackBounds.Height;
				}
				float thumbOffset = 0f;
				if (ComputeValueRange() - _largeChange > 0)
				{
					thumbOffset = (float)(_value - _minimum) / (float)(ComputeValueRange() - _largeChange);
					thumbOffset *= (float)(trackBounds.Height - thumbLength);
				}
				else
				{
					thumbLength = trackBounds.Height;
					thumbOffset = 0f;
				}
				int thumbStart = trackBounds.Top;
				if (_value > _minimum)
				{
					thumbStart += (int)thumbOffset;
				}
				if (thumbStart < trackBounds.Top)
				{
					thumbStart = trackBounds.Top;
				}
				if (_value == ComputeMaxValue() || thumbStart + thumbLength > trackBounds.Bottom)
				{
					thumbStart = trackBounds.Bottom - thumbLength;
				}
				thumbBounds = new Rectangle(clientRectangle.Left, thumbStart, clientRectangle.Width, thumbLength);
			}
			area._bounds = thumbBounds;
			if (_pressedArea == 2)
			{
				if (_controlStyle.ButtonAppearance == ScrollBarAppearance.PopUp)
				{
					area._isHot = true;
				}
				else if (_controlStyle.IsHotTrackingEnabled())
				{
					area._isPressed = true;
				}
			}
			_areas[2] = area;
			if (area._isVisible)
			{
				area = new ScrollAreaData(ScrollAreaType.Page);
				if (thumbBounds.Top > trackBounds.Top)
				{
					area._bounds = new Rectangle(trackBounds.Left, trackBounds.Top, trackBounds.Width, thumbBounds.Top - trackBounds.Top);
				}
				else
				{
					area._bounds = Rectangle.Empty;
					area._isVisible = false;
				}
				if (1 == _pressedArea)
				{
					area._isPressed = true;
				}
				_areas[1] = area;
				area = new ScrollAreaData(ScrollAreaType.Page);
				if (thumbBounds.Bottom < trackBounds.Bottom)
				{
					area._bounds = new Rectangle(trackBounds.Left, thumbBounds.Bottom, trackBounds.Width, trackBounds.Bottom - thumbBounds.Bottom);
				}
				else
				{
					area._bounds = Rectangle.Empty;
					area._isVisible = false;
				}
				if (3 == _pressedArea)
				{
					area._isPressed = true;
				}
				_areas[3] = area;
			}
			else
			{
				area = new ScrollAreaData(ScrollAreaType.Page);
				area._bounds = trackBounds;
				_areas[1] = area;
				area = new ScrollAreaData(ScrollAreaType.Page);
				area._isVisible = false;
				_areas[3] = area;
			}
		}

		private void LayoutHorizontalAreas()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle trackBounds = new Rectangle(_areas[0]._bounds.Right, clientRectangle.Top, _areas[4]._bounds.Left - _areas[0]._bounds.Right, clientRectangle.Height);
			ScrollAreaData area = new ScrollAreaData(ScrollAreaType.Thumb);
			Rectangle thumbBounds;
			if (trackBounds.Width < 8 || !base.Enabled)
			{
				thumbBounds = Rectangle.Empty;
				area._isVisible = false;
			}
			else
			{
				int thumbLength = 0;
				if (_largeChange != 0)
				{
					float thumbRatio = (float)_largeChange / (float)ComputeValueRange();
					thumbLength = (int)((float)trackBounds.Width * thumbRatio);
					if (thumbLength < 8)
					{
						thumbLength = 8;
					}
				}
				else
				{
					thumbLength = SystemInformation.HorizontalScrollBarThumbWidth;
				}
				if (ComputeValueRange() <= 0 || _maximum <= _minimum)
				{
					thumbLength = trackBounds.Width;
				}
				float thumbOffset = 0f;
				if (ComputeValueRange() - _largeChange > 0)
				{
					thumbOffset = (float)(_value - _minimum) / (float)(ComputeValueRange() - _largeChange);
					thumbOffset *= (float)(trackBounds.Width - thumbLength);
				}
				else
				{
					thumbLength = trackBounds.Width;
					thumbOffset = 0f;
				}
				int thumbStart = trackBounds.Left;
				if (_value > _minimum)
				{
					thumbStart += (int)thumbOffset;
				}
				if (thumbStart < trackBounds.Left)
				{
					thumbStart = trackBounds.Left;
				}
				if (_value == ComputeMaxValue() || thumbStart + thumbLength > trackBounds.Right)
				{
					thumbStart = trackBounds.Right - thumbLength;
				}
				thumbBounds = new Rectangle(thumbStart, base.ClientRectangle.Top, thumbLength, base.ClientRectangle.Height);
			}
			area._bounds = thumbBounds;
			if (_pressedArea == 2)
			{
				if (_controlStyle.ButtonAppearance == ScrollBarAppearance.PopUp)
				{
					area._isHot = true;
				}
				else if (_controlStyle.IsHotTrackingEnabled())
				{
					area._isPressed = true;
				}
			}
			_areas[2] = area;
			if (area._isVisible)
			{
				area = new ScrollAreaData(ScrollAreaType.Page);
				if (thumbBounds.Left > trackBounds.Left)
				{
					area._bounds = new Rectangle(trackBounds.Left, trackBounds.Top, thumbBounds.Left - trackBounds.Left, trackBounds.Height);
				}
				else
				{
					area._bounds = Rectangle.Empty;
					area._isVisible = false;
				}
				if (1 == _pressedArea)
				{
					area._isPressed = true;
				}
				_areas[1] = area;
				area = new ScrollAreaData(ScrollAreaType.Page);
				if (thumbBounds.Right < trackBounds.Right)
				{
					area._bounds = new Rectangle(thumbBounds.Right, trackBounds.Top, trackBounds.Right - thumbBounds.Right, trackBounds.Height);
				}
				else
				{
					area._bounds = Rectangle.Empty;
					area._isVisible = false;
				}
				if (3 == _pressedArea)
				{
					area._isPressed = true;
				}
				_areas[3] = area;
			}
			else
			{
				area = new ScrollAreaData(ScrollAreaType.Page);
				area._bounds = trackBounds;
				_areas[1] = area;
				area = new ScrollAreaData(ScrollAreaType.Page);
				area._isVisible = false;
				_areas[3] = area;
			}
		}

		private int ComputeValueRange()
		{
			return (int)Math.Min((long)_maximum - _minimum + 1, int.MaxValue);
		}

		private int ComputeMaxValue()
		{
			return (int)Math.Min((long)_maximum - _largeChange + 1, int.MaxValue);
		}

		public bool GetIsMouseOver()
		{
			return _isMouseOver;
		}

		public void UpdateIsMouseOver(bool isMouseOver)
		{
			if (_isMouseOver != isMouseOver)
			{
				_isMouseOver = isMouseOver;
				Invalidate();
			}
		}

		internal void RelayoutIfVisible()
		{
			if (base.Visible)
			{
				if (_type == ScrollBarType.Horizontal)
				{
					LayoutHorizontalAreas();
				}
				else
				{
					LayoutVerticalAreas();
				}
				Invalidate();
			}
		}

		private void DrawPageArea(Graphics graphics, int areaIndex)
		{
			ScrollAreaData area = _areas[areaIndex];
			if (area._isVisible)
			{
				if (area._isPressed)
				{
					Brush pressedBrush = GdiCache.Brush(Color.Black);
					graphics.FillRectangle(pressedBrush, area._bounds);
				}
				else
				{
					graphics.FillRectangle(_controlStyle.GetTrackBrush(), area._bounds);
				}
			}
		}

		private void RenderScrollBar(Graphics graphics)
		{
			if (_controlStyle.GetIsThemed() && TaskPaneRenderer.IsThemingActive())
			{
				RenderThemedScrollBar(graphics);
			}
			else
			{
				RenderClassicScrollBar(graphics);
			}
		}

		private void RenderClassicScrollBar(Graphics graphics)
		{
			RenderClassicArea(graphics, 0);
			RenderClassicArea(graphics, 4);
			RenderClassicArea(graphics, 2);
			DrawPageArea(graphics, 1);
			DrawPageArea(graphics, 3);
		}

		private void RenderThemedScrollBar(Graphics graphics)
		{
			bool isDisabled = !base.Enabled;
			TaskPaneRenderer taskPaneRenderer = new TaskPaneRenderer();
			if (_type == ScrollBarType.Vertical)
			{
				ScrollAreaData area = _areas[0];
				if (area._isVisible)
				{
					taskPaneRenderer.DrawUpArrow(graphics, area._bounds, area._isHot, area._isPressed, isDisabled);
				}
				area = _areas[4];
				if (area._isVisible)
				{
					taskPaneRenderer.DrawDownArrow(graphics, area._bounds, area._isHot, area._isPressed, isDisabled);
				}
				area = _areas[2];
				if (area._isVisible && !isDisabled)
				{
					taskPaneRenderer.DrawVerticalThumb(graphics, area._bounds, area._isHot, area._isPressed);
				}
				area = _areas[1];
				if (area._isVisible)
				{
					taskPaneRenderer.DrawUpperTrack(graphics, area._bounds, area._isHot, area._isPressed, isDisabled);
				}
				area = _areas[3];
				if (area._isVisible)
				{
					taskPaneRenderer.DrawLowerTrack(graphics, area._bounds, area._isHot, area._isPressed, isDisabled);
				}
			}
			else
			{
				ScrollAreaData area = _areas[0];
				if (area._isVisible)
				{
					taskPaneRenderer.DrawLeftArrow(graphics, area._bounds, area._isHot, area._isPressed, isDisabled);
				}
				area = _areas[4];
				if (area._isVisible)
				{
					taskPaneRenderer.DrawRightArrow(graphics, area._bounds, area._isHot, area._isPressed, isDisabled);
				}
				area = _areas[2];
				if (area._isVisible && !isDisabled)
				{
					taskPaneRenderer.DrawHorizontalThumb(graphics, area._bounds, area._isHot, area._isPressed);
				}
				area = _areas[1];
				if (area._isVisible)
				{
					taskPaneRenderer.DrawLeftTrack(graphics, area._bounds, area._isHot, area._isPressed, isDisabled);
				}
				area = _areas[3];
				if (area._isVisible)
				{
					taskPaneRenderer.DrawRightTrack(graphics, area._bounds, area._isHot, area._isPressed, isDisabled);
				}
			}
		}

		private void RenderClassicArea(Graphics graphics, int areaIndex)
		{
			ScrollAreaData area = _areas[areaIndex];
			_controlStyle.GetButtonBrush();
			Pen glyphPen = _controlStyle.GetButtonTextPen();
			bool ownsPen = false;
			if (!area._isVisible)
			{
				return;
			}
			bool isPressed = area._isPressed;
			if (isPressed && area._type == ScrollAreaType.Thumb && _controlStyle.ButtonAppearance == ScrollBarAppearance.PopUp)
			{
				isPressed = false;
			}
			_controlStyle.DrawButtonBackground(graphics, area._bounds, area._isHot, isPressed);
			switch (areaIndex)
			{
			case 0:
				if (_type == ScrollBarType.Vertical)
				{
					_controlStyle.DrawVerticalArrow(graphics, area._bounds, -1, area._isPressed, area._isHot, base.Enabled);
				}
				else
				{
					_controlStyle.DrawHorizontalArrow(graphics, area._bounds, -1, area._isPressed, area._isHot, base.Enabled);
				}
				break;
			case 4:
				if (_type == ScrollBarType.Vertical)
				{
					_controlStyle.DrawVerticalArrow(graphics, area._bounds, 1, area._isPressed, area._isHot, base.Enabled);
				}
				else
				{
					_controlStyle.DrawHorizontalArrow(graphics, area._bounds, 1, area._isPressed, area._isHot, base.Enabled);
				}
				break;
			}
			if (ownsPen)
			{
				glyphPen.Dispose();
				glyphPen = null;
			}
		}

		private int ClampValue(int value)
		{
			if (value < _minimum)
			{
				value = _minimum;
			}
			if (value > ComputeMaxValue())
			{
				value = ComputeMaxValue();
			}
			return value;
		}

		private void RaiseAreaScroll(int areaIndex)
		{
			int newValue = _value;
			ScrollEventArgs e;
			switch (areaIndex)
			{
			default:
				return;
			case 0:
				newValue = ClampValue(_value - _smallChange);
				e = new ScrollEventArgs(ScrollEventType.SmallDecrement, newValue);
				break;
			case 4:
				newValue = ClampValue(_value + _smallChange);
				e = new ScrollEventArgs(ScrollEventType.SmallIncrement, newValue);
				break;
			case 1:
				newValue = ClampValue(_value - _largeChange);
				e = new ScrollEventArgs(ScrollEventType.LargeDecrement, newValue);
				break;
			case 3:
				newValue = ClampValue(_value + _largeChange);
				e = new ScrollEventArgs(ScrollEventType.LargeIncrement, newValue);
				break;
			case 2:
				e = new ScrollEventArgs(ScrollEventType.ThumbTrack, _value);
				break;
			case 5:
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
				UpdateThumbDrag(eventArgs.X, eventArgs.Y);
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

		private void UpdateThumbDrag(int mouseX, int mouseY)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (_pressedArea != 2)
			{
				return;
			}
			if (_type == ScrollBarType.Vertical)
			{
				if (Rectangle.Inflate(clientRectangle, clientRectangle.Width * 8, SystemInformation.VerticalScrollBarArrowHeight * 2).Contains(mouseX, mouseY))
				{
					int trackLength = _areas[4]._bounds.Top - _areas[0]._bounds.Bottom;
					int thumbOffset = mouseY - _thumbGrabOffset - _areas[0]._bounds.Bottom;
					if (thumbOffset < 0)
					{
						thumbOffset = 0;
					}
					if (thumbOffset > trackLength)
					{
						thumbOffset = trackLength;
					}
					float thumbFraction = (float)thumbOffset / (float)trackLength;
					int newValue = _minimum + (int)((float)((long)_maximum - _minimum) * thumbFraction);
					if (newValue < _minimum)
					{
						newValue = _minimum;
					}
					if (newValue > ComputeMaxValue())
					{
						newValue = ComputeMaxValue();
					}
					if (newValue != _value)
					{
						_value = newValue;
						RelayoutIfVisible();
						RaiseAreaScroll(2);
					}
				}
				else
				{
					_value = _dragStartValue;
					RelayoutIfVisible();
					RaiseAreaScroll(2);
				}
			}
			else if (Rectangle.Inflate(base.ClientRectangle, SystemInformation.HorizontalScrollBarArrowWidth * 2, base.ClientRectangle.Height * 8).Contains(mouseX, mouseY))
			{
				int trackLength = _areas[4]._bounds.Left - _areas[0]._bounds.Right;
				int thumbOffset = mouseX - _thumbGrabOffset - _areas[0]._bounds.Right;
				if (thumbOffset < 0)
				{
					thumbOffset = 0;
				}
				if (thumbOffset > trackLength)
				{
					thumbOffset = trackLength;
				}
				float thumbFraction = (float)thumbOffset / (float)trackLength;
				int newValue = _minimum + (int)((float)((long)_maximum - _minimum + 1) * thumbFraction);
				if (newValue < _minimum)
				{
					newValue = _minimum;
				}
				if (newValue > ComputeMaxValue())
				{
					newValue = ComputeMaxValue();
				}
				if (newValue != _value)
				{
					_value = newValue;
					RelayoutIfVisible();
					RaiseAreaScroll(2);
				}
			}
			else
			{
				_value = _dragStartValue;
				RelayoutIfVisible();
				RaiseAreaScroll(2);
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			OnMouseLeave(new EventArgs());
		}

		private int HitTestArea(int x, int y)
		{
			int firstArea = 0;
			int lastArea = 4;
			for (int i = firstArea; i <= lastArea; i++)
			{
				ScrollAreaData area = _areas[i];
				if (area._isVisible && area._bounds.Contains(x, y))
				{
					return i;
				}
			}
			return -1;
		}

		private void UpdateHotArea(bool isPointerOutside, int mouseX, int mouseY)
		{
			if (!_controlStyle.IsHotTrackingEnabled())
			{
				return;
			}
			if (isPointerOutside)
			{
				SetAreaHot(_hotArea, false);
				_hotArea = -1;
				return;
			}
			int hitArea = HitTestArea(mouseX, mouseY);
			if (hitArea != _hotArea)
			{
				SetAreaHot(_hotArea, false);
				_hotArea = hitArea;
				SetAreaHot(_hotArea, true);
			}
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			base.OnMouseEnter(eventArgs);
			UpdateIsMouseOver(true);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			UpdateIsMouseOver(false);
			if (_isDraggingThumb)
			{
				_isDraggingThumb = false;
				if (_pressedArea == 2 && _value != _dragStartValue)
				{
					RaiseAreaScroll(5);
					OnValueChanged(new EventArgs());
				}
			}
			UpdateHotArea(true, 0, 0);
			SetAreaPressed(_pressedArea, false);
			_pressedArea = -1;
			_timer.Enabled = false;
		}

		private void SetAreaHot(int areaIndex, bool isHot)
		{
			if (areaIndex == -1 || !base.Enabled)
			{
				return;
			}
			ScrollAreaData area = _areas[areaIndex];
			if (area._isHot != isHot && area._isVisible)
			{
				_areas[areaIndex]._isHot = isHot;
				if (_controlStyle != null && _controlStyle.IsHotTrackingEnabled())
				{
					Invalidate(area._bounds);
					Update();
				}
				_hoverTimer.Enabled = isHot;
			}
		}

		internal void SetAreaPressed(int areaIndex, bool isPressed)
		{
			if (areaIndex == -1)
			{
				return;
			}
			ScrollAreaData area = _areas[areaIndex];
			if (area._isPressed != isPressed)
			{
				if (area._type != ScrollAreaType.Thumb || !isPressed)
				{
					_areas[areaIndex]._isPressed = isPressed;
				}
				if (area._isVisible)
				{
					Invalidate(_areas[areaIndex]._bounds);
					Update();
				}
			}
		}

		public void AddScrollHandler(ScrollEventHandler handler)
		{
			ScrollEventHandler current = _scrollHandler;
			ScrollEventHandler observed;
			do
			{
				observed = current;
				ScrollEventHandler updated = (ScrollEventHandler)Delegate.Combine(observed, handler);
				current = Interlocked.CompareExchange(ref _scrollHandler, updated, observed);
			}
			while ((object)current != observed);
		}

		protected virtual void OnScroll(ScrollEventArgs eventArgs)
		{
			if (_scrollHandler != null)
			{
				_scrollHandler(this, eventArgs);
			}
			UpdateValue(eventArgs.NewValue);
		}

		public void AddValueChangedHandler(EventHandler handler)
		{
			EventHandler current = _valueChangedHandler;
			EventHandler observed;
			do
			{
				observed = current;
				EventHandler updated = (EventHandler)Delegate.Combine(observed, handler);
				current = Interlocked.CompareExchange(ref _valueChangedHandler, updated, observed);
			}
			while ((object)current != observed);
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
			LayoutAllAreas();
		}

		protected override void OnVisibleChanged(EventArgs eventArgs)
		{
			base.OnVisibleChanged(eventArgs);
		}

		public ControlStyle GetScrollBarStyle()
		{
			return _controlStyle;
		}
	}
}
