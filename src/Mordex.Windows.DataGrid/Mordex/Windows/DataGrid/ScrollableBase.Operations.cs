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
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			e.Control.TabStop = false;
		}

		private void UpdateNavigatorLayout()
		{
			if (_numericEditControl != null)
			{
				int scrollBarWidth = ScrollBarWidth;
				_numericEditControl.Left = 0;
				_numericEditControl.SetButtonHeight(scrollBarWidth);
			}
		}

		public bool ShouldSerializeScrollBarWidth()
		{
			return _scrollBarWidth != -1;
		}

		void IGridStyleSource.OnControlStyleChanged()
		{
			_scrollableBasePanel.BackColor = _gridControlStyle.GetLayoutState().GetEditControlsStyle().GetResolvedAppearance()
				.GetGridColor(GridColorKey.Control);
			_numericEditControl.UpdateCStyle(_gridControlStyle);
			Invalidate(invalidateChildren: true);
		}

		MxColorSchemeAppearanceState IGridStyleSource.GetDrawingStyle(GridStyleArea styleArea)
		{
			return GetAreaAppearanceState(styleArea);
		}

		ImageList IGridStyleSource.GetImageList()
		{
			return GetControlImageList();
		}

		protected virtual ImageList GetControlImageList()
		{
			return null;
		}

		private void OnPanelPaint(object sender, PaintEventArgs eventArgs)
		{
			NumericEditControl.DrawBackground(eventArgs.Graphics, _scrollableBasePanel.ClientRectangle, _gridControlStyle);
		}

		internal MxColorSchemeAppearanceState GetAreaAppearanceState(GridStyleArea gridStyleArea)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = new MxColorSchemeAppearanceState(!UseCompatibleTextRendering);
			mordexColorSchemeAppearanceState.SetThemedLook(true);
			return mordexColorSchemeAppearanceState;
		}

		internal void DrawControlBorder(Graphics graphics)
		{
			if (_gridControlStyle != null)
			{
				_gridControlStyle.DrawBorder(graphics, base.ClientRectangle, _borderStyle, base.Enabled);
			}
		}

		private void InitializeControls(Rectangle bounds)
		{
			if (_scrollableBasePanel == null)
			{
				return;
			}
			UpdateNavigatorLayout();
			SuspendLayout();
			UpdateBorderWidth();
			int scrollBarWidth = 0;
			int height = 0;
			int otherScrollBarWidth = 0;
			Rectangle rectangle = bounds;
			rectangle.Inflate(-_borderWidth, -_borderWidth);
			switch (_scrollBars)
			{
			case ScrollBars.Automatic:
				_isVerticalBarVisible = _needsVerticalScroll;
				break;
			case ScrollBars.Vertical:
			case ScrollBars.Both:
				_isVerticalBarVisible = true;
				break;
			default:
				_isVerticalBarVisible = false;
				break;
			}
			_vScroll.Enabled = !_isVerticalBarVisible || _needsVerticalScroll;
			if (_recordNavigator)
			{
				_isHorizontalBarVisible = true;
			}
			else
			{
				switch (_scrollBars)
				{
				case ScrollBars.Automatic:
					_isHorizontalBarVisible = _needsHorizontalScroll;
					break;
				case ScrollBars.Horizontal:
				case ScrollBars.Both:
					_isHorizontalBarVisible = true;
					break;
				default:
					_isHorizontalBarVisible = false;
					break;
				}
			}
			_horizontalScrollBar.Enabled = !_isHorizontalBarVisible || _needsHorizontalScroll;
			if (_isVerticalBarVisible)
			{
				scrollBarWidth = ScrollBarWidth;
			}
			else
			{
				_ = rectangle.Width;
			}
			if (_isHorizontalBarVisible)
			{
				otherScrollBarWidth = ScrollBarWidth;
			}
			_ = rectangle.Width;
			height = rectangle.Height - otherScrollBarWidth;
			if (RightToLeft == RightToLeft.Yes)
			{
				_vScroll.Location = rectangle.Location;
			}
			else
			{
				_vScroll.Location = new Point(rectangle.Right - scrollBarWidth, rectangle.Top);
			}
			_vScroll.Size = new Size(scrollBarWidth, height);
			if (_isHorizontalBarVisible)
			{
				_scrollableBasePanel.Location = new Point(rectangle.Left, rectangle.Bottom - otherScrollBarWidth);
				_scrollableBasePanel.Size = new Size(rectangle.Width, otherScrollBarWidth);
			}
			_scrollableBasePanel.Visible = _isHorizontalBarVisible;
			if (_isHorizontalBarVisible)
			{
				rectangle.Height -= _scrollableBasePanel.Height;
			}
			else
			{
				_scrollableBasePanel.Width = 0;
			}
			switch (_scrollBars)
			{
			case ScrollBars.Automatic:
				_isHorizontalBarVisible = _needsHorizontalScroll;
				break;
			case ScrollBars.Horizontal:
			case ScrollBars.Both:
				_isHorizontalBarVisible = true;
				break;
			default:
				_isHorizontalBarVisible = false;
				break;
			}
			_horizontalScrollBar.Visible = _isHorizontalBarVisible;
			OnPanelResize(_scrollableBasePanel, new EventArgs());
			_vScroll.Visible = _isVerticalBarVisible;
			if (_isVerticalBarVisible)
			{
				if (RightToLeft == RightToLeft.Yes)
				{
					rectangle.X += _vScroll.Width;
					rectangle.Width -= _vScroll.Width;
				}
				else
				{
					rectangle.Width -= _vScroll.Width;
				}
			}
			_numericEditControl.SetCompactLayout(true);
			if (rectangle.Width < 0)
			{
				rectangle.Width = 0;
			}
			if (rectangle.Height < 0)
			{
				rectangle.Height = 0;
			}
			if (!rectangle.Equals(_contentBounds))
			{
				_contentBounds = rectangle;
				ResumeLayout(performLayout: true);
				OnClientResize();
			}
			else
			{
				ResumeLayout(performLayout: true);
			}
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
			SetScrollableBaseBoolean();
			RelayoutScrollControls();
		}

		internal bool IsDropDownOpenState()
		{
			return _isDropDownOpen;
		}

		internal void SetDropDownOpenState(bool isDropDownOpen)
		{
			_isDropDownOpen = isDropDownOpen;
		}

		private void OnNavigatorResize(object sender, EventArgs eventArgs)
		{
			OnPanelResize(null, eventArgs);
		}

		private void OnNavigatorButtonClicked(object value, NavigatorButtonEventArgs eventArgs)
		{
			OnNavigatorButtonClick((int)eventArgs.GetNavigatorButton());
		}

		protected internal virtual void OnNavigatorButtonClick(int navigatorButton)
		{
		}

		private void OnNavigatorRowEntered(object sender, EventArgs eventArgs)
		{
			OnNavigatorMoveToRow(_numericEditControl.GetNumericEditControlInteger());
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnNavigatorMoveToRow(int row)
		{
		}

		internal void AssignControlStyle(ControlStyle controlStyle)
		{
			_gridControlStyle = controlStyle;
			try
			{
				if (_gridControlStyle != null)
				{
					_scrollableBasePanel.BackColor = _gridControlStyle.ControlColor;
				}
				_vScroll._style = controlStyle;
				_horizontalScrollBar._style = controlStyle;
				_numericEditControl.UpdateCStyle(controlStyle);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		internal void AssignOwnedControlStyle(ControlStyle controlStyle, bool condition)
		{
			_gridControlStyle = controlStyle;
			if (condition)
			{
				_gridControlStyle._printDocument = this;
			}
			try
			{
				_scrollableBasePanel.BackColor = _gridControlStyle.ControlColor;
				_vScroll._style = controlStyle;
				_horizontalScrollBar._style = controlStyle;
				_numericEditControl.UpdateCStyle(controlStyle);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = SystemColors.WindowFrame;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !FlatBorderColor.Equals(SystemColors.WindowFrame);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			ScrollByWheelDelta(e.Delta);
		}

		internal void ScrollByWheelDelta(int value)
		{
			if (value < 0)
			{
				if (IsVerticalScrollRequired())
				{
					if (SystemInformation.MouseWheelScrollLines == -1)
					{
						ScrollBarControl vScroll = _vScroll;
						vScroll.SetScrollValue(vScroll.GetScrollValue() + _vScroll.GetPageSize());
					}
					else
					{
						ScrollBarControl otherVScroll = _vScroll;
						otherVScroll.SetScrollValue(otherVScroll.GetScrollValue() + _vScroll.GetScrollStep() * SystemInformation.MouseWheelScrollLines);
					}
				}
			}
			else if (value > 0 && IsVerticalScrollRequired())
			{
				if (SystemInformation.MouseWheelScrollLines == -1)
				{
					ScrollBarControl nextVScroll = _vScroll;
					nextVScroll.SetScrollValue(nextVScroll.GetScrollValue() - _vScroll.GetPageSize());
				}
				else
				{
					ScrollBarControl previousVScroll = _vScroll;
					previousVScroll.SetScrollValue(previousVScroll.GetScrollValue() - _vScroll.GetScrollStep() * SystemInformation.MouseWheelScrollLines);
				}
			}
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			OnVisualStyleAreaChanged();
			if (_gridControlStyle != null)
			{
				_gridControlStyle.GetLayoutState().ResetCachedStyles();
				_gridControlStyle.ReleaseLayoutState();
			}
			RelayoutScrollControls();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColor()
		{
			return base.ForeColor != SystemColors.WindowText;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ResetForeColor()
		{
			base.ForeColor = SystemColors.WindowText;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !base.BackColor.Equals(SystemColors.Window);
		}

		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		internal void RaiseVisualStyleAreaChanged()
		{
			OnVisualStyleAreaChanged();
		}

		protected virtual void OnVisualStyleAreaChanged()
		{
			_gridControlStyle.GetLayoutState().ResetCachedStyles();
			Invalidate(invalidateChildren: true);
			RelayoutScrollControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_vScroll != null)
				{
					_vScroll.RemoveScrollHandler(OnVScrollBarScroll);
					_vScroll.RemoveValueChangedHandler(HandleScrollBarScroll);
					_vScroll.Dispose();
					_vScroll = null;
				}
				if (_horizontalScrollBar != null)
				{
					_horizontalScrollBar.RemoveScrollHandler(OnHScrollBarScroll);
					_horizontalScrollBar.RemoveValueChangedHandler(OnHScrollBarValueChanged);
					_horizontalScrollBar.Dispose();
					_horizontalScrollBar = null;
				}
				if (_numericEditControl != null)
				{
					_numericEditControl.Resize -= OnNavigatorResize;
					_numericEditControl.RemoveButtonClickedHandler(OnNavigatorButtonClicked);
					_numericEditControl.RemoveChangedHandler(OnNavigatorRowEntered);
					_numericEditControl.Dispose();
					_numericEditControl = null;
				}
				if (_scrollableBasePanel != null)
				{
					_scrollableBasePanel.Paint -= OnPanelPaint;
					_scrollableBasePanel.Resize -= OnPanelResize;
					_scrollableBasePanel.Dispose();
					_scrollableBasePanel = null;
				}
				if (_gridControlStyle != null)
				{
					_gridControlStyle.ReleaseLayoutState();
					if (_gridControlStyle._printDocument == this)
					{
						_gridControlStyle._printDocument = null;
					}
				}
			}
			base.Dispose(disposing);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void PerformAutoScale()
		{
		}

		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		public void ResetUseCompatibleTextRendering()
		{
			_useCompatibleTextRendering = null;
			OnUseCompatibleTextRenderingChanged();
		}

		protected virtual void OnUseCompatibleTextRenderingChanged()
		{
			if (_numericEditControl != null)
			{
				_numericEditControl.SetUseCompatibleTextRendering(UseCompatibleTextRendering);
			}
			RelayoutScrollControls();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_vScroll != null && _vScroll.Visible)
			{
				_vScroll.Update();
			}
			if (_horizontalScrollBar != null && _horizontalScrollBar.Visible)
			{
				_horizontalScrollBar.Update();
			}
			base.OnPaint(e);
		}
	}
}
