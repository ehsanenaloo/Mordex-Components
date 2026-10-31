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
			int controlStyleControl2Integer = _pressedArea;
			if (controlStyleControl2Integer == 2)
			{
				_isDraggingThumb = true;
				OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, _scrollValue));
				_valueAtDragStart = _scrollValue;
				if (_type == ScrollBarType.Vertical)
				{
					_dragOffset = eventArgs.Y - _controlStyleControl2DataArray[2]._bounds.Top;
				}
				else if (RightToLeft == RightToLeft.No)
				{
					_dragOffset = eventArgs.X - _controlStyleControl2DataArray[2]._bounds.Left;
				}
				else
				{
					_dragOffset = MirrorForRightToLeft(_controlStyleControl2DataArray[2]._bounds).Right - eventArgs.X;
				}
			}
			else
			{
				ScrollByArea(_pressedArea);
				_timer.Interval = Timing.ScrollBarInitialDelay;
				_timer.Enabled = true;
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
					UpdateHotArea(true, 0, 0);
				}
				_controlStyleControl2Timer.Enabled = false;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (_isDraggingThumb)
			{
				DragThumbTo(eventArgs.X, eventArgs.Y);
				return;
			}
			SetMouseOver(true);
			if (eventArgs.Button == MouseButtons.Left && _pressedArea != -1)
			{
				int hitTestArea = HitTestArea(eventArgs.X, eventArgs.Y);
				if (hitTestArea != _pressedArea)
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

		private void DragThumbTo(int value, int otherValue)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (_pressedArea != 2)
			{
				return;
			}
			if (_type == ScrollBarType.Vertical)
			{
				if (Rectangle.Inflate(clientRectangle, clientRectangle.Width * 8, SystemInformation.VerticalScrollBarArrowHeight * 2).Contains(value, otherValue))
				{
					int nextValue = _controlStyleControl2DataArray[4]._bounds.Top - _controlStyleControl2DataArray[0]._bounds.Bottom;
					int previousValue = otherValue - _dragOffset - _controlStyleControl2DataArray[0]._bounds.Bottom;
					if (previousValue < 0)
					{
						previousValue = 0;
					}
					if (previousValue > nextValue)
					{
						previousValue = nextValue;
					}
					float amount = (float)previousValue / (float)nextValue;
					int controlStyleControl2Integer = _minimum + (int)Math.Ceiling((double)(_maximum - _minimum) * (double)amount);
					if (controlStyleControl2Integer < _minimum)
					{
						controlStyleControl2Integer = _minimum;
					}
					if (controlStyleControl2Integer > GetMaxValue())
					{
						controlStyleControl2Integer = GetMaxValue();
					}
					if (controlStyleControl2Integer != _scrollValue)
					{
						_scrollValue = controlStyleControl2Integer;
						RelayoutAndInvalidate();
						ScrollByArea(2);
					}
				}
				else
				{
					_scrollValue = _valueAtDragStart;
					RelayoutAndInvalidate();
					ScrollByArea(2);
				}
			}
			else if (Rectangle.Inflate(base.ClientRectangle, SystemInformation.HorizontalScrollBarArrowWidth * 2, base.ClientRectangle.Height * 8).Contains(value, otherValue))
			{
				int firstValue = _controlStyleControl2DataArray[4]._bounds.Left - _controlStyleControl2DataArray[0]._bounds.Right;
				if (RightToLeft == RightToLeft.Yes)
				{
					value = base.ClientRectangle.Right - value;
				}
				int secondValue = value - _dragOffset - _controlStyleControl2DataArray[0]._bounds.Right;
				if (secondValue < 0)
				{
					secondValue = 0;
				}
				if (secondValue > firstValue)
				{
					secondValue = firstValue;
				}
				float otherAmount = (float)secondValue / (float)firstValue;
				int otherControlStyleControl2Integer = _minimum + (int)Math.Ceiling((double)(_maximum - _minimum) * (double)otherAmount);
				if (otherControlStyleControl2Integer < _minimum)
				{
					otherControlStyleControl2Integer = _minimum;
				}
				if (otherControlStyleControl2Integer > GetMaxValue())
				{
					otherControlStyleControl2Integer = GetMaxValue();
				}
				if (otherControlStyleControl2Integer != _scrollValue)
				{
					_scrollValue = otherControlStyleControl2Integer;
					RelayoutAndInvalidate();
					ScrollByArea(2);
				}
			}
			else
			{
				_scrollValue = _valueAtDragStart;
				RelayoutAndInvalidate();
				ScrollByArea(2);
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			ResetMouseState();
		}

		private int HitTestArea(int value, int otherValue)
		{
			int nextValue = 0;
			int previousValue = 4;
			for (int i = nextValue; i <= previousValue; i++)
			{
				ControlStyleControl2Data a = _controlStyleControl2DataArray[i];
				Rectangle rectangle = MirrorForRightToLeft(a._bounds);
				if (a._isVisible && rectangle.Contains(value, otherValue))
				{
					return i;
				}
			}
			return -1;
		}

		internal bool IsMouseOver()
		{
			return _isMouseOver;
		}

		internal void SetMouseOver(bool controlStyleControl2Boolean3)
		{
			if (_isMouseOver != controlStyleControl2Boolean3)
			{
				_isMouseOver = controlStyleControl2Boolean3;
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			SetMouseOver(false);
			ResetMouseState();
		}

		private void ResetMouseState()
		{
			if (_isDraggingThumb)
			{
				_isDraggingThumb = false;
				if (_pressedArea == 2 && _scrollValue != _valueAtDragStart)
				{
					ScrollByArea(5);
					OnValueChanged(new EventArgs());
				}
			}
			UpdateHotArea(true, 0, 0);
			SetAreaPressed(_pressedArea, false);
			_pressedArea = -1;
			_timer.Enabled = false;
		}

		internal void ProcessMouseInput(MouseEventArgs eventArgs, int value, int otherValue)
		{
			eventArgs = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, value, otherValue, eventArgs.Delta);
			OnMouseDown(eventArgs);
			GridHelper.SetCapture(this, true);
		}
	}
}
