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
		internal int GetScrollBarThickness()
		{
			if (_thickness == -1)
			{
				return SystemInformation.GetVerticalScrollBarWidthForDpi(DeviceDpi);
			}
			return _thickness;
		}

		internal void SetScrollBarThickness(int controlStyleControl2Integer6)
		{
			if (_thickness != controlStyleControl2Integer6)
			{
				_thickness = controlStyleControl2Integer6;
				if (GetScrollBarType() == ScrollBarType.Horizontal)
				{
					base.Height = GetScrollBarThickness();
				}
				else
				{
					base.Width = GetScrollBarThickness();
				}
			}
		}

		public ScrollBarType GetScrollBarType()
		{
			return _type;
		}

		private void SetScrollBarTypeProperty()
		{
			if (base.IsHandleCreated)
			{
				if (_type == ScrollBarType.Horizontal)
				{
					NativeMethods.SetProp(base.Handle, "PROP_MORDEXSCROLBARTYPE", new IntPtr(1));
				}
				else
				{
					NativeMethods.SetProp(base.Handle, "PROP_MORDEXSCROLBARTYPE", new IntPtr(2));
				}
			}
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			ApplyThickness();
		}

		public int GetPageSize()
		{
			return _pageSize;
		}

		public void SetPageSize(int controlStyleControl2Integer7)
		{
			if (controlStyleControl2Integer7 < 0)
			{
				controlStyleControl2Integer7 = 0;
			}
			if (controlStyleControl2Integer7 != _pageSize)
			{
				_pageSize = controlStyleControl2Integer7;
				LayoutAreas();
			}
		}

		public int GetScrollValue()
		{
			return _scrollValue;
		}

		public void SetScrollValue(int controlStyleControl2Integer10)
		{
			if (controlStyleControl2Integer10 < _minimum)
			{
				controlStyleControl2Integer10 = _minimum;
			}
			if (controlStyleControl2Integer10 > GetMaxValue())
			{
				controlStyleControl2Integer10 = GetMaxValue();
			}
			if (controlStyleControl2Integer10 != _scrollValue)
			{
				_scrollValue = controlStyleControl2Integer10;
				OnValueChanged(new EventArgs());
				RelayoutAndInvalidate();
			}
		}

		public int GetScrollStep()
		{
			return _scrollStep;
		}

		public void SetScrollStep(int controlStyleControl2Integer11)
		{
			if (controlStyleControl2Integer11 < 0)
			{
				controlStyleControl2Integer11 = 0;
			}
			if (controlStyleControl2Integer11 != _scrollStep)
			{
				_scrollStep = controlStyleControl2Integer11;
			}
		}

		private void LayoutAreas()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			int scrollBarThickness = 0;
			int height = 0;
			if (_type == ScrollBarType.Vertical)
			{
				scrollBarThickness = GetScrollBarThickness();
				height = clientRectangle.Height;
				int otherScrollBarThickness = GetScrollBarThickness();
				ControlStyleControl2Data a = new ControlStyleControl2Data(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, scrollBarThickness, otherScrollBarThickness);
				_controlStyleControl2DataArray[0] = a;
				a = new ControlStyleControl2Data(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Bottom - otherScrollBarThickness, scrollBarThickness, otherScrollBarThickness);
				_controlStyleControl2DataArray[4] = a;
				LayoutVerticalThumb();
			}
			else
			{
				scrollBarThickness = clientRectangle.Width;
				height = GetScrollBarThickness();
				int nextScrollBarThickness = GetScrollBarThickness();
				ControlStyleControl2Data a = new ControlStyleControl2Data(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, nextScrollBarThickness, height);
				_controlStyleControl2DataArray[0] = a;
				a = new ControlStyleControl2Data(ScrollAreaType.Arrow);
				a._bounds = new Rectangle(clientRectangle.Right - nextScrollBarThickness, clientRectangle.Top, nextScrollBarThickness, height);
				_controlStyleControl2DataArray[4] = a;
				LayoutHorizontalThumb();
			}
			Invalidate();
		}

		private void LayoutVerticalThumb()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = new Rectangle(clientRectangle.Left, _controlStyleControl2DataArray[0]._bounds.Bottom, clientRectangle.Width, _controlStyleControl2DataArray[4]._bounds.Top - _controlStyleControl2DataArray[0]._bounds.Bottom);
			ControlStyleControl2Data a = new ControlStyleControl2Data(ScrollAreaType.Thumb);
			Rectangle otherRectangle;
			if (!base.Enabled || rectangle.Height < 8)
			{
				otherRectangle = Rectangle.Empty;
				a._isVisible = false;
			}
			else
			{
				int height = 0;
				if (_pageSize > 0)
				{
					float amount = (float)_pageSize / (float)GetRange();
					height = (int)((float)rectangle.Height * amount);
					if (height < 16)
					{
						height = 16;
					}
				}
				else
				{
					height = 16;
				}
				height = Math.Min(height, rectangle.Height);
				if (GetRange() <= 0 || _maximum <= _minimum)
				{
					height = rectangle.Height;
				}
				float otherAmount = 0f;
				if (GetRange() - _pageSize > 0)
				{
					otherAmount = (float)(_scrollValue - _minimum) / (float)(GetRange() - _pageSize);
					otherAmount *= (float)(rectangle.Height - height);
				}
				else
				{
					height = rectangle.Height;
					otherAmount = 0f;
				}
				int top = rectangle.Top;
				if (_scrollValue > _minimum)
				{
					top += (int)otherAmount;
				}
				if (top < rectangle.Top)
				{
					top = rectangle.Top;
				}
				if (_scrollValue == GetMaxValue() || top + height > rectangle.Bottom)
				{
					top = rectangle.Bottom - height;
				}
				otherRectangle = new Rectangle(clientRectangle.Left, top, clientRectangle.Width, height);
			}
			a._bounds = otherRectangle;
			if (_pressedArea == 2)
			{
				if (_style.ButtonAppearance == ButtonAppearance.PopUp)
				{
					a._controlStyleControl2DataBoolean = true;
				}
				else if (_style.UsesHoverEffect(_controlStyleControl2Boolean && VisualStyleDrawer.IsThemingAvailable()))
				{
					a._isPressed = true;
				}
			}
			_controlStyleControl2DataArray[2] = a;
			if (a._isVisible)
			{
				a = new ControlStyleControl2Data(ScrollAreaType.Page);
				if (otherRectangle.Top > rectangle.Top)
				{
					a._bounds = new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, otherRectangle.Top - rectangle.Top);
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
				_controlStyleControl2DataArray[1] = a;
				a = new ControlStyleControl2Data(ScrollAreaType.Page);
				if (otherRectangle.Bottom < rectangle.Bottom)
				{
					a._bounds = new Rectangle(rectangle.Left, otherRectangle.Bottom, rectangle.Width, rectangle.Bottom - otherRectangle.Bottom);
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
				_controlStyleControl2DataArray[3] = a;
			}
			else
			{
				a = new ControlStyleControl2Data(ScrollAreaType.Page);
				a._bounds = rectangle;
				_controlStyleControl2DataArray[1] = a;
				a = new ControlStyleControl2Data(ScrollAreaType.Page);
				a._isVisible = false;
				_controlStyleControl2DataArray[3] = a;
			}
		}

		private void LayoutHorizontalThumb()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = new Rectangle(_controlStyleControl2DataArray[0]._bounds.Right, clientRectangle.Top, _controlStyleControl2DataArray[4]._bounds.Left - _controlStyleControl2DataArray[0]._bounds.Right, clientRectangle.Height);
			ControlStyleControl2Data a = new ControlStyleControl2Data(ScrollAreaType.Thumb);
			Rectangle otherRectangle;
			if (rectangle.Width < 8 || !base.Enabled)
			{
				otherRectangle = Rectangle.Empty;
				a._isVisible = false;
			}
			else
			{
				int width = 0;
				if (_pageSize != 0)
				{
					float amount = (float)_pageSize / (float)GetRange();
					width = (int)((float)rectangle.Width * amount);
					if (width < 16)
					{
						width = 16;
					}
				}
				else
				{
					width = 16;
				}
				width = Math.Min(width, rectangle.Width);
				if (GetRange() <= 0 || _maximum <= _minimum)
				{
					width = rectangle.Width;
				}
				float otherAmount = 0f;
				if (GetRange() - _pageSize > 0)
				{
					otherAmount = (float)(_scrollValue - _minimum) / (float)(GetRange() - _pageSize);
					otherAmount *= (float)(rectangle.Width - width);
				}
				else
				{
					width = rectangle.Width;
					otherAmount = 0f;
				}
				int left = rectangle.Left;
				if (_scrollValue > _minimum)
				{
					left += (int)otherAmount;
				}
				if (left < rectangle.Left)
				{
					left = rectangle.Left;
				}
				if (_scrollValue == GetMaxValue() || left + width > rectangle.Right)
				{
					left = rectangle.Right - width;
				}
				otherRectangle = new Rectangle(left, base.ClientRectangle.Top, width, base.ClientRectangle.Height);
			}
			a._bounds = otherRectangle;
			if (_pressedArea == 2)
			{
				if (_style.ButtonAppearance == ButtonAppearance.PopUp)
				{
					a._controlStyleControl2DataBoolean = true;
				}
				else if (_style.UsesHoverEffect(_controlStyleControl2Boolean && VisualStyleDrawer.IsThemingAvailable()))
				{
					a._isPressed = true;
				}
			}
			_controlStyleControl2DataArray[2] = a;
			if (a._isVisible)
			{
				a = new ControlStyleControl2Data(ScrollAreaType.Page);
				if (otherRectangle.Left > rectangle.Left)
				{
					a._bounds = new Rectangle(rectangle.Left, rectangle.Top, otherRectangle.Left - rectangle.Left, rectangle.Height);
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
				_controlStyleControl2DataArray[1] = a;
				a = new ControlStyleControl2Data(ScrollAreaType.Page);
				if (otherRectangle.Right < rectangle.Right)
				{
					a._bounds = new Rectangle(otherRectangle.Right, rectangle.Top, rectangle.Right - otherRectangle.Right, rectangle.Height);
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
				_controlStyleControl2DataArray[3] = a;
			}
			else
			{
				a = new ControlStyleControl2Data(ScrollAreaType.Page);
				a._bounds = rectangle;
				_controlStyleControl2DataArray[1] = a;
				a = new ControlStyleControl2Data(ScrollAreaType.Page);
				a._isVisible = false;
				_controlStyleControl2DataArray[3] = a;
			}
		}

		private void ScrollByArea(int value)
		{
			int controlStyleControl2Integer = _scrollValue;
			ScrollEventArgs e;
			switch (value)
			{
			default:
				return;
			case 0:
				controlStyleControl2Integer = ClampValue(_scrollValue - _scrollStep);
				e = new ScrollEventArgs(ScrollEventType.SmallDecrement, controlStyleControl2Integer);
				break;
			case 4:
				controlStyleControl2Integer = ClampValue(_scrollValue + _scrollStep);
				e = new ScrollEventArgs(ScrollEventType.SmallIncrement, controlStyleControl2Integer);
				break;
			case 1:
				controlStyleControl2Integer = ClampValue(_scrollValue - _pageSize);
				e = new ScrollEventArgs(ScrollEventType.LargeDecrement, controlStyleControl2Integer);
				break;
			case 3:
				controlStyleControl2Integer = ClampValue(_scrollValue + _pageSize);
				e = new ScrollEventArgs(ScrollEventType.LargeIncrement, controlStyleControl2Integer);
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

		protected virtual void OnScroll(ScrollEventArgs eventArgs)
		{
			if (base.Events[_scrollEventKey] is ScrollEventHandler scrollEventHandler)
			{
				scrollEventHandler(this, eventArgs);
			}
			SetScrollValue(eventArgs.NewValue);
		}

		internal void PerformScroll(int value)
		{
			ScrollByArea(value);
		}

		protected override void OnVisibleChanged(EventArgs eventArgs)
		{
			base.OnVisibleChanged(eventArgs);
		}
	}
}
