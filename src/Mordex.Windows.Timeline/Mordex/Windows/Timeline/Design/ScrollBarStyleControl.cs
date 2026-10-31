using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design
{
	[ToolboxItem(false)]
	internal class ScrollBarStyleControl : Control, IStyleAwareControl
	{
		private ScrollBarStyle _scrollBarStyle;

		private object[] _valueArray;

		private int _selectedIndex = -1;

		private EventHandler _eventHandler;

		private bool _isChecked;

		private bool _isHovered;

		private bool _isPressed;

		Pen IStyleAwareControl.FlatBorderPen => SystemPens.ControlDark;

		public ScrollBarStyleControl()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			_scrollBarStyle = new ScrollBarStyle();
		}

		public ScrollBarStyle GetScrollBarStyle()
		{
			return _scrollBarStyle;
		}

		void IStyleAwareControl.OnControlStyleChanged()
		{
			Invalidate();
		}

		public void SetValues(object[] valueArray)
		{
			_valueArray = valueArray;
			Invalidate();
			if (_selectedIndex == -1)
			{
				_selectedIndex = 0;
			}
			if (_valueArray != null)
			{
				if (_selectedIndex > _valueArray.Length - 1)
				{
					_selectedIndex = _valueArray.Length - 1;
				}
			}
			else
			{
				_selectedIndex = -1;
			}
		}

		public int GetSelectedIndex()
		{
			return _selectedIndex;
		}

		public void SetSelectedIndex(int selectedIndex)
		{
			if (_selectedIndex < -1 || (_valueArray != null && _selectedIndex > _valueArray.Length - 1))
			{
				throw new ArgumentOutOfRangeException();
			}
			if (_selectedIndex != selectedIndex)
			{
				_selectedIndex = selectedIndex;
				OnSelectedIndexChanged(EventArgs.Empty);
				Invalidate();
			}
		}

		public void AddSelectedIndexChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _eventHandler;
			EventHandler comparandHandler;
			do
			{
				comparandHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(comparandHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _eventHandler, value, comparandHandler);
			}
			while ((object)eventHandler != comparandHandler);
		}

		public void RemoveSelectedIndexChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _eventHandler;
			EventHandler comparandHandler;
			do
			{
				comparandHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(comparandHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _eventHandler, value, comparandHandler);
			}
			while ((object)eventHandler != comparandHandler);
		}

		protected virtual void OnSelectedIndexChanged(EventArgs eventArgs)
		{
			if (_eventHandler != null)
			{
				_eventHandler(this, eventArgs);
			}
		}

		public object GetSelectedValue()
		{
			if (_selectedIndex == -1)
			{
				return null;
			}
			return _valueArray[_selectedIndex];
		}

		public void SelectValue(object value)
		{
			if (value == null)
			{
				_selectedIndex = -1;
			}
			else
			{
				if (_valueArray == null)
				{
					return;
				}
				for (int i = 0; i < _valueArray.Length; i++)
				{
					if (TimelineUtilities.CompareValuesIgnoreCase(_valueArray[i], value, null) == 0)
					{
						_selectedIndex = i;
						break;
					}
				}
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			_scrollBarStyle.DrawButtonBackground(eventArgs.Graphics, base.ClientRectangle, _isHovered, _isPressed);
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = new Rectangle(clientRectangle.Right - 10, clientRectangle.Top, 10, clientRectangle.Height);
			_scrollBarStyle.DrawVerticalArrow(eventArgs.Graphics, rectangle, 1, _isPressed, _isHovered, base.Enabled);
			clientRectangle.Inflate(-2, -2);
			clientRectangle.Width -= 6;
			if (GetSelectedValue() != null)
			{
				Brush brush = ((!base.Enabled) ? new SolidBrush(SystemColors.GrayText) : new SolidBrush(ForeColor));
				StringFormat stringFormat = new StringFormat();
				stringFormat.LineAlignment = StringAlignment.Center;
				stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				eventArgs.Graphics.DrawString(GetSelectedValue().ToString(), Font, brush, clientRectangle, stringFormat);
				brush.Dispose();
			}
		}

		public void SetChecked(bool isChecked)
		{
			_isChecked = isChecked;
			Invalidate();
		}

		private void UpdateIsHovered(bool scrollBarStyleControlBoolean)
		{
			if (_isHovered != scrollBarStyleControlBoolean)
			{
				_isHovered = scrollBarStyleControlBoolean;
				Invalidate();
			}
		}

		private void UpdateIsPressed(bool isPressed)
		{
			if (_isPressed != isPressed)
			{
				_isPressed = isPressed;
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
			GetNextControl(this, forward: true)?.Focus();
			if (_valueArray == null)
			{
				return;
			}
			ContextMenuStrip contextMenu = new ContextMenuStrip();
			for (int i = 0; i < _valueArray.Length; i++)
			{
				ToolStripMenuItem menuItem = new ToolStripMenuItem(_valueArray[i].ToString(), null, OnMenuItemClick);
				if (i == _selectedIndex)
				{
					menuItem.Checked = true;
				}
				contextMenu.Items.Add(menuItem);
			}
			contextMenu.Show(this, new Point(0, base.Height));
		}

		private void OnMenuItemClick(object sender, EventArgs eventArgs)
		{
			SetSelectedIndex(((ToolStripItem)sender).Owner.Items.IndexOf((ToolStripItem)sender));
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (eventArgs.Button == MouseButtons.Left)
			{
				UpdateIsPressed(base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y));
			}
			else if (eventArgs.Button == MouseButtons.None)
			{
				UpdateIsHovered(base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y));
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (_isPressed)
			{
				UpdateIsPressed(false);
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			UpdateIsHovered(false);
		}
	}
}
