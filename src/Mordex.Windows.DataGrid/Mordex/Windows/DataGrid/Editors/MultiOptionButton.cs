using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	public class MultiOptionButton : Control, IGridStyleSource
	{
		private ControlStyle _style;

		private object[] _optionValues;

		private int _selectedIndex = -1;

		private EventHandler _selectedIndexChangedHandler;

		private bool _threeState;

		private bool _isHot;

		private bool _isPressed;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlStyle ControlStyle => _style;

		Font IGridStyleSource.ControlStyleFont => Font;

		bool IGridStyleSource.IsRightToLeft => RightToLeft == RightToLeft.Yes;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public object[] Values
		{
			get
			{
				return _optionValues;
			}
			set
			{
				_optionValues = value;
				Invalidate();
				if (_selectedIndex == -1)
				{
					_selectedIndex = 0;
				}
				if (_optionValues != null)
				{
					if (_selectedIndex > _optionValues.Length - 1)
					{
						_selectedIndex = _optionValues.Length - 1;
					}
				}
				else
				{
					_selectedIndex = -1;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int SelectedIndex
		{
			get
			{
				return _selectedIndex;
			}
			set
			{
				if (_selectedIndex < -1 || (_optionValues != null && _selectedIndex > _optionValues.Length - 1))
				{
					throw new ArgumentOutOfRangeException();
				}
				if (_selectedIndex != value)
				{
					_selectedIndex = value;
					OnSelectedIndexChanged(EventArgs.Empty);
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public object SelectedValue
		{
			get
			{
				if (_selectedIndex == -1)
				{
					return null;
				}
				return _optionValues[_selectedIndex];
			}
			set
			{
				if (value == null)
				{
					_selectedIndex = -1;
				}
				else
				{
					if (_optionValues == null)
					{
						return;
					}
					for (int i = 0; i < _optionValues.Length; i++)
					{
						if (GridHelper.CompareValues(_optionValues[i], value, null, true) == 0)
						{
							_selectedIndex = i;
							break;
						}
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool ThreeState
		{
			get
			{
				return _threeState;
			}
			set
			{
				_threeState = value;
				Invalidate();
			}
		}

		public event EventHandler SelectedIndexChanged
		{
			add
			{
				EventHandler eventHandler = _selectedIndexChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedIndexChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _selectedIndexChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedIndexChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		public MultiOptionButton()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			_style = new ControlStyle(this);
			_style.UseClassicLook();
			_style.SetFlatBorderColor(SystemColors.ControlDark);
		}

		void IGridStyleSource.OnControlStyleChanged()
		{
			Invalidate();
		}

		MxColorSchemeAppearanceState IGridStyleSource.GetDrawingStyle(GridStyleArea styleArea)
		{
			return new MxColorSchemeAppearanceState(useTextRenderer: true);
		}

		ImageList IGridStyleSource.GetImageList()
		{
			return null;
		}

		protected virtual void OnSelectedIndexChanged(EventArgs e)
		{
			if (_selectedIndexChangedHandler != null)
			{
				_selectedIndexChangedHandler(this, e);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			_style.DrawBackground(e.Graphics, base.ClientRectangle, _isHot, _isPressed);
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = new Rectangle(clientRectangle.Right - 10, clientRectangle.Top, 10, clientRectangle.Height);
			_style.DrawVerticalArrow(e.Graphics, rectangle, 1, _isPressed, base.Enabled);
			clientRectangle.Inflate(-2, -2);
			clientRectangle.Width -= 6;
			if (SelectedValue != null)
			{
				Color color = ((!base.Enabled) ? SystemColors.GrayText : ForeColor);
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: true);
				textLayoutRenderer.DrawTextWithColor(e.Graphics, GridHelper.ToSafeString(SelectedValue), clientRectangle, Font, color);
				textLayoutRenderer.ReleaseStringFormat();
			}
		}

		private void SetHot(bool condition)
		{
			if (_isHot != condition)
			{
				_isHot = condition;
				Invalidate();
			}
		}

		private void UpdateMultiOptionButtonBoolean(bool multiOptionButtonBoolean)
		{
			if (_isPressed != multiOptionButtonBoolean)
			{
				_isPressed = multiOptionButtonBoolean;
				Invalidate();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			Control nextControl = GetNextControl(this, forward: true);
			if (nextControl != null)
			{
				GridHelper.SelectAndFocus(nextControl);
			}
			if (_optionValues == null)
			{
				return;
			}
			ContextMenuStrip contextMenu = new ContextMenuStrip();
			for (int i = 0; i < _optionValues.Length; i++)
			{
				ToolStripMenuItem menuItem = new ToolStripMenuItem(GridHelper.ToSafeString(_optionValues[i]), null, OnMenuItemClicked);
				if (i == _selectedIndex)
				{
					menuItem.Checked = true;
				}
				contextMenu.Items.Add(menuItem);
			}
			contextMenu.Show(this, new Point(0, base.Height));
		}

		private void OnMenuItemClicked(object sender, EventArgs eventArgs)
		{
			SelectedIndex = ((ToolStripItem)sender).Owner.Items.IndexOf((ToolStripItem)sender);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.Button == MouseButtons.Left)
			{
				UpdateMultiOptionButtonBoolean(base.ClientRectangle.Contains(e.X, e.Y));
			}
			else if (e.Button == MouseButtons.None)
			{
				SetHot(base.ClientRectangle.Contains(e.X, e.Y));
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (_isPressed)
			{
				UpdateMultiOptionButtonBoolean(false);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHot(false);
		}
	}
}
