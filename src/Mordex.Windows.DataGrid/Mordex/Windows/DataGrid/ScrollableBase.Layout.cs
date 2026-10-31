using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class ScrollableBase
	{
		private void UpdateScrollableBase()
		{
			BackColor = SystemColors.Window;
			ForeColor = SystemColors.WindowText;
			SetScrollableBaseBoolean();
		}

		public void ResetScrollBarWidth()
		{
			ScrollBarWidth = -1;
		}

		private void SetScrollableBaseBoolean()
		{
			_isRightToLeft = RightToLeft == RightToLeft.Yes;
		}

		internal bool GetScrollableBaseBoolean()
		{
			return _isRightToLeft;
		}

		private void InitializeScrollControls()
		{
			_gridControlStyle = new ControlStyle(this);
			_vScroll = new ScrollBarControl(ScrollBarType.Vertical);
			_vScroll.Name = "VScroll";
			_vScroll._style = _gridControlStyle;
			_horizontalScrollBar = new ScrollBarControl(ScrollBarType.Horizontal);
			_horizontalScrollBar._style = _gridControlStyle;
			_scrollableBasePanel = new Panel();
			_scrollableBasePanel.TabStop = false;
			_numericEditControl = new NumericEditControl(_gridControlStyle);
			_numericEditControl.SetUseCompatibleTextRendering(UseCompatibleTextRendering);
			_numericEditControl.TabStop = false;
			_numericEditControl.UpdateCStyle(_gridControlStyle);
			_numericEditControl.Enabled = false;
			_numericEditControl.Resize += OnNavigatorResize;
			_numericEditControl.AddButtonClickedHandler(OnNavigatorButtonClicked);
			_numericEditControl.AddChangedHandler(OnNavigatorRowEntered);
			_numericEditControl.Visible = _recordNavigator;
			_vScroll.Visible = false;
			_vScroll.TabStop = false;
			_vScroll.AddScrollHandler(OnVScrollBarScroll);
			_vScroll.AddValueChangedHandler(HandleScrollBarScroll);
			_horizontalScrollBar.TabStop = false;
			_horizontalScrollBar.AddScrollHandler(OnHScrollBarScroll);
			_horizontalScrollBar.AddValueChangedHandler(OnHScrollBarValueChanged);
			_scrollableBasePanel.TabStop = false;
			_scrollableBasePanel.Resize += OnPanelResize;
			_scrollableBasePanel.Paint += OnPanelPaint;
			_scrollableBasePanel.BackColor = SystemColors.Control;
			_scrollableBasePanel.Visible = false;
			BackColor = SystemColors.Window;
			base.Controls.Add(_scrollableBasePanel);
			_scrollableBasePanel.Controls.Add(_numericEditControl);
			_scrollableBasePanel.Controls.Add(_horizontalScrollBar);
			base.Controls.Add(_vScroll);
			_numericEditControl.SetButtonHeight(_scrollBarWidth);
			_horizontalScrollBar.SetScrollBarThickness(_scrollBarWidth);
			_vScroll.SetScrollBarThickness(_scrollBarWidth);
		}

		private void OnPanelResize(object sender, EventArgs eventArgs)
		{
			int width = 0;
			if (_recordNavigator)
			{
				_numericEditControl.Top = 0;
				_numericEditControl.Visible = true;
				_numericEditControl.RecalculateLayout();
				width = _numericEditControl.Width;
			}
			else
			{
				_numericEditControl.Visible = false;
				_numericEditControl.Top = _scrollableBasePanel.Height;
			}
			int top = _scrollableBasePanel.ClientRectangle.Top;
			if (RightToLeft == RightToLeft.Yes)
			{
				if (_isVerticalBarVisible)
				{
					_horizontalScrollBar.Location = new Point(_scrollableBasePanel.ClientRectangle.Left + ScrollBarWidth, _scrollableBasePanel.ClientRectangle.Top);
				}
				else
				{
					_horizontalScrollBar.Location = new Point(_scrollableBasePanel.ClientRectangle.Left, _scrollableBasePanel.ClientRectangle.Top);
				}
				if (_recordNavigator)
				{
					_numericEditControl.Location = new Point(_scrollableBasePanel.ClientRectangle.Right - _numericEditControl.Width, top);
				}
			}
			else
			{
				if (_recordNavigator)
				{
					_numericEditControl.Location = new Point(_scrollableBasePanel.ClientRectangle.X, top);
				}
				_horizontalScrollBar.Location = new Point(_scrollableBasePanel.ClientRectangle.X + width, _scrollableBasePanel.ClientRectangle.Top);
			}
			if (_isVerticalBarVisible)
			{
				if (_recordNavigator)
				{
					_horizontalScrollBar.Size = new Size(_scrollableBasePanel.ClientRectangle.Width - ScrollBarWidth - width, _scrollableBasePanel.ClientRectangle.Height);
				}
				else
				{
					_horizontalScrollBar.Size = new Size(_scrollableBasePanel.ClientRectangle.Width - ScrollBarWidth, _scrollableBasePanel.ClientRectangle.Height);
				}
			}
			else if (_recordNavigator)
			{
				_horizontalScrollBar.Size = new Size(_scrollableBasePanel.ClientRectangle.Width - width, _scrollableBasePanel.ClientRectangle.Height);
			}
			else
			{
				_horizontalScrollBar.Size = _scrollableBasePanel.ClientRectangle.Size;
			}
		}

		private void HandleScrollBarScroll(object sender, EventArgs eventArgs)
		{
			OnScrollBarScroll(0);
		}

		private void OnVScrollBarScroll(object sender, ScrollEventArgs eventArgs)
		{
			OnScrollBarScrollEvent(eventArgs);
			if (eventArgs.Type == ScrollEventType.ThumbTrack)
			{
				OnScrollBarScroll(1);
			}
		}

		private void OnHScrollBarValueChanged(object sender, EventArgs eventArgs)
		{
			OnScrollBarScroll(2);
		}

		private void OnHScrollBarScroll(object sender, ScrollEventArgs eventArgs)
		{
			OnScrollBarScrollEvent(eventArgs);
			if (eventArgs.Type == ScrollEventType.ThumbTrack)
			{
				OnScrollBarScroll(3);
			}
		}

		internal Size GetSizeWithScrollBars(Size size)
		{
			UpdateBorderWidth();
			Size result = new Size(size.Width + _borderWidth * 2, size.Height + _borderWidth * 2);
			if (_isHorizontalBarVisible)
			{
				result.Height += _scrollableBasePanel.Height;
			}
			if (_isVerticalBarVisible)
			{
				result.Width += _vScroll.Width;
			}
			return result;
		}

		protected override void OnResize(EventArgs e)
		{
			RelayoutScrollControls();
			base.OnResize(e);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (base.Visible)
			{
				RelayoutScrollControls();
			}
		}

		private void UpdateBorderWidth()
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetAreaAppearanceState(GridStyleArea.ControlBorder);
			if (mordexColorSchemeAppearanceState.IsThemedLook)
			{
				_borderWidth = 1;
			}
			else if (mordexColorSchemeAppearanceState._isThemed)
			{
				_borderWidth = 2;
			}
			else
			{
				switch (_borderStyle)
				{
				case BorderStyle.Flat:
				case BorderStyle.RaisedLight3D:
				case BorderStyle.SunkenLight3D:
					_borderWidth = 1;
					break;
				case BorderStyle.Raised:
				case BorderStyle.Sunken:
					_borderWidth = 2;
					break;
				}
			}
			if (_borderStyle == BorderStyle.None)
			{
				_borderWidth = 0;
			}
		}

		private void RelayoutScrollControls()
		{
			InitializeControls(base.ClientRectangle);
		}

		protected virtual void OnClientResize()
		{
			OnClientSizeChanged(EventArgs.Empty);
		}

		internal bool IsVerticalScrollRequired()
		{
			return _needsVerticalScroll;
		}

		internal void SetVerticalScrollRequired(bool needsVerticalScroll)
		{
			if (_needsVerticalScroll != needsVerticalScroll)
			{
				_needsVerticalScroll = needsVerticalScroll;
				RelayoutScrollControls();
			}
		}

		internal bool IsHorizontalScrollRequired()
		{
			return _needsHorizontalScroll;
		}

		internal void SetHorizontalScrollRequired(bool needsHorizontalScroll)
		{
			if (_needsHorizontalScroll != needsHorizontalScroll)
			{
				_needsHorizontalScroll = needsHorizontalScroll;
				RelayoutScrollControls();
			}
		}

		internal void ConfigureHorizontalScrollBar(int controlStyleControl2Integer, int otherControlStyleControl2Integer, int nextControlStyleControl2Integer, int previousControlStyleControl2Integer)
		{
			try
			{
				_horizontalScrollBar.SetMinimum(controlStyleControl2Integer);
				_horizontalScrollBar.SetMaximum(otherControlStyleControl2Integer);
				if (nextControlStyleControl2Integer > 0)
				{
					_horizontalScrollBar.SetPageSize(nextControlStyleControl2Integer);
				}
				_horizontalScrollBar.SetScrollValue(previousControlStyleControl2Integer);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		internal ScrollBarControl GetHorizontalScrollBar()
		{
			return _horizontalScrollBar;
		}

		internal void ConfigureVerticalScrollBar(int controlStyleControl2Integer, int otherControlStyleControl2Integer, int nextControlStyleControl2Integer, int previousControlStyleControl2Integer)
		{
			try
			{
				_vScroll.SetMinimum(controlStyleControl2Integer);
				_vScroll.SetMaximum(otherControlStyleControl2Integer);
				if (nextControlStyleControl2Integer > 0)
				{
					_vScroll.SetPageSize(nextControlStyleControl2Integer);
				}
				else
				{
					_vScroll.SetPageSize(1);
				}
				_vScroll.SetScrollValue(previousControlStyleControl2Integer);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		internal ScrollBarControl GetVScroll()
		{
			return _vScroll;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual void OnScrollBarScroll(int scrollCode)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnScrollBarScrollEvent(ScrollEventArgs e)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ScrollControlIntoView(Control ctl)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void SetAutoScrollMargin(int x, int y)
		{
		}
	}
}
