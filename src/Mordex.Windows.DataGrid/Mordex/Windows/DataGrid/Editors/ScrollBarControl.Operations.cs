using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal partial class ScrollBarControl
	{
		private void InitializeTimers()
		{
			_timer = new Timer();
			_timer.Tick += OnTimerTick;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			_controlStyleControl2Timer = new Timer();
			_controlStyleControl2Timer.Enabled = false;
			_controlStyleControl2Timer.Interval = Timing.ScrollStepInterval;
			_controlStyleControl2Timer.Tick += OnHoverTimerTick;
		}

		private Rectangle MirrorForRightToLeft(Rectangle rectangle)
		{
			if (_type == ScrollBarType.Horizontal && RightToLeft == RightToLeft.Yes)
			{
				Rectangle result = new Rectangle(base.ClientRectangle.Right - rectangle.Right, rectangle.Y, rectangle.Width, rectangle.Height);
				return result;
			}
			return rectangle;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_controlStyleControl2Timer != null)
				{
					_controlStyleControl2Timer.Tick -= OnHoverTimerTick;
					_controlStyleControl2Timer.Dispose();
					_controlStyleControl2Timer = null;
				}
				if (_timer != null)
				{
					_timer.Tick -= OnTimerTick;
					_timer.Dispose();
					_timer = null;
				}
				_controlStyleControl2DataArray = null;
				_style = null;
				base.Events.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);
			SetScrollBarTypeProperty();
			SetScrollBarTypeProperty();
		}

		protected override void OnHandleDestroyed(EventArgs eventArgs)
		{
			NativeMethods.RemoveProp(base.Handle, "PROP_MORDEXSCROLBARTYPE");
			base.OnHandleDestroyed(eventArgs);
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
					if (_controlStyleControl2DataArray[_pressedArea]._isPressed)
					{
						ScrollByArea(_pressedArea);
					}
				}
				if (_timer.Interval != 50)
				{
					_timer.Interval = Timing.ScrollStepInterval;
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private void ApplyThickness()
		{
			if (_type == ScrollBarType.Vertical)
			{
				if (base.Width != GetScrollBarThickness())
				{
					base.Width = GetScrollBarThickness();
					return;
				}
			}
			else if (base.Height != GetScrollBarThickness())
			{
				base.Height = GetScrollBarThickness();
				return;
			}
			LayoutAreas();
		}

		public void SetMinimum(int controlStyleControl2Integer8)
		{
			if (controlStyleControl2Integer8 != _minimum)
			{
				_minimum = controlStyleControl2Integer8;
				if (_scrollValue < _minimum)
				{
					SetScrollValue(_minimum);
				}
				LayoutAreas();
			}
		}

		public void SetMaximum(int controlStyleControl2Integer9)
		{
			if (controlStyleControl2Integer9 != _maximum)
			{
				_maximum = controlStyleControl2Integer9;
				if (_scrollValue > GetMaxValue())
				{
					SetScrollValue(GetMaxValue());
				}
				LayoutAreas();
			}
		}

		private int GetRange()
		{
			return _maximum - _minimum + 1;
		}

		internal int GetMaxValue()
		{
			return _maximum - _pageSize + 1;
		}

		internal bool IsThemeEnabled()
		{
			return _controlStyleControl2Boolean;
		}

		private int ClampValue(int controlStyleControl2Integer)
		{
			if (controlStyleControl2Integer < _minimum)
			{
				controlStyleControl2Integer = _minimum;
			}
			if (controlStyleControl2Integer > GetMaxValue())
			{
				controlStyleControl2Integer = GetMaxValue();
			}
			return controlStyleControl2Integer;
		}

		private void UpdateHotArea(bool condition, int value, int otherValue)
		{
			if (!_style.UsesHoverEffect(IsThemeEnabled() && VisualStyleDrawer.IsThemingAvailable()))
			{
				return;
			}
			if (condition)
			{
				SetAreaHot(_hotArea, false);
				Invalidate();
				_hotArea = -1;
				return;
			}
			int hitTestArea = HitTestArea(value, otherValue);
			if (hitTestArea != _hotArea)
			{
				SetAreaHot(_hotArea, false);
				_hotArea = hitTestArea;
				SetAreaHot(_hotArea, true);
			}
		}

		private void SetAreaHot(int value, bool controlStyleControl2DataBoolean)
		{
			if (value == -1 || !base.Enabled)
			{
				return;
			}
			ControlStyleControl2Data a = _controlStyleControl2DataArray[value];
			if (a._controlStyleControl2DataBoolean != controlStyleControl2DataBoolean && a._isVisible)
			{
				_controlStyleControl2DataArray[value]._controlStyleControl2DataBoolean = controlStyleControl2DataBoolean;
				if (_style != null && _style.UsesHoverEffect(_controlStyleControl2Boolean && VisualStyleDrawer.IsThemingAvailable()))
				{
					Invalidate(MirrorForRightToLeft(a._bounds));
					Update();
				}
				_controlStyleControl2Timer.Enabled = controlStyleControl2DataBoolean;
			}
		}

		private void SetAreaPressed(int value, bool condition)
		{
			if (value == -1)
			{
				return;
			}
			ControlStyleControl2Data a = _controlStyleControl2DataArray[value];
			if (a._isPressed != condition)
			{
				if (a._type != ScrollAreaType.Thumb || !condition)
				{
					_controlStyleControl2DataArray[value]._isPressed = condition;
				}
				if (a._isVisible)
				{
					Invalidate(MirrorForRightToLeft(_controlStyleControl2DataArray[value]._bounds));
					Update();
				}
			}
		}

		public void AddScrollHandler(ScrollEventHandler scrollEventHandler)
		{
			base.Events.AddHandler(_scrollEventKey, scrollEventHandler);
		}

		public void RemoveScrollHandler(ScrollEventHandler scrollEventHandler)
		{
			base.Events.RemoveHandler(_scrollEventKey, scrollEventHandler);
		}

		public void AddValueChangedHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_controlStyleControl2Value, eventHandler);
		}

		public void RemoveValueChangedHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(_controlStyleControl2Value, eventHandler);
		}

		protected virtual void OnValueChanged(EventArgs eventArgs)
		{
			if (base.Events[_controlStyleControl2Value] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		protected override void OnEnabledChanged(EventArgs eventArgs)
		{
			base.OnEnabledChanged(eventArgs);
			SetAreaHot(_hotArea, false);
			SetAreaPressed(_pressedArea, false);
			LayoutAreas();
			Update();
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new ScrollBarAccessibleObject(this);
		}

		internal AccessibleStates ComputeAccessibleStates(int value)
		{
			if (!_controlStyleControl2DataArray[value]._isVisible)
			{
				return AccessibleStates.Invisible;
			}
			if (_controlStyleControl2DataArray[value]._isPressed)
			{
				return AccessibleStates.Pressed;
			}
			return AccessibleStates.None;
		}

		internal Rectangle GetAreaScreenBounds(int value)
		{
			Rectangle rectangle = _controlStyleControl2DataArray[value]._bounds;
			rectangle.Location = PointToScreen(rectangle.Location);
			return rectangle;
		}
	}
}
