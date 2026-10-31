using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI
{
	[ToolboxItem(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal class ScrollableHostControl : Control, IWindowsElement
	{
		private const int ThinBorderWidth = 1;

		private const int ThickBorderWidth = 2;

		private const int VerticalScrollCode = 0;

		private const int VerticalThumbTrackCode = 1;

		private const int HorizontalScrollCode = 2;

		private const int HorizontalThumbTrackCode = 3;

		private ThemedScrollBar _horizontalScrollBar;

		internal Panel _scrollBarPanel;

		private ThemedScrollBar _verticalScrollBar;

		private BorderStyle _borderStyle = BorderStyle.SunkenLight3D;

		private Rectangle _contentBounds;

		private bool _hasHorizontalScrollBar;

		private bool _hasVerticalScrollBar;

		internal int _borderWidth;

		private bool _isVerticalScrollNeeded;

		private bool _isDropDownOpen;

		private ControlAppearance _appearance;

		private Color _flatBorderColor = Color.Empty;

		Color IWindowsElement.FlatBorderColor => GetFlatBorderColor();

		internal ScrollableHostControl()
		{
			InitializeScrollBars();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
			LayoutScrollBars();
		}

		protected override void OnControlAdded(ControlEventArgs eventArgs)
		{
			base.OnControlAdded(eventArgs);
			eventArgs.Control.TabStop = false;
		}

		void IWindowsElement.OnControlStyleChanged()
		{
			_scrollBarPanel.BackColor = _appearance.ControlColor;
			Invalidate(invalidateChildren: true);
		}

		private void InitializeScrollBars()
		{
			_appearance = new ControlAppearance(this);
			_verticalScrollBar = new ThemedScrollBar(ScrollBarType.Vertical);
			_verticalScrollBar._appearance = _appearance;
			_horizontalScrollBar = new ThemedScrollBar(ScrollBarType.Horizontal);
			_horizontalScrollBar._appearance = _appearance;
			_scrollBarPanel = new Panel();
			_scrollBarPanel.TabStop = false;
			_verticalScrollBar.Visible = false;
			_verticalScrollBar.TabStop = false;
			_verticalScrollBar.AddScrollHandler(OnVerticalScrollBarScroll);
			_verticalScrollBar.AddValueChangedHandler(OnVerticalScrollBarValueChanged);
			_horizontalScrollBar.TabStop = false;
			_horizontalScrollBar.AddScrollHandler(OnHorizontalScrollBarScroll);
			_horizontalScrollBar.AddValueChangedHandler(OnHorizontalScrollBarValueChanged);
			_scrollBarPanel.Resize += OnPanelResize;
			_scrollBarPanel.BackColor = SystemColors.Control;
			_scrollBarPanel.Visible = false;
			SetBackColor(SystemColors.Window);
			base.Controls.Add(_scrollBarPanel);
			_scrollBarPanel.Controls.Add(_horizontalScrollBar);
			base.Controls.Add(_verticalScrollBar);
		}

		private void OnPanelResize(object sender, EventArgs eventArgs)
		{
			LayoutHorizontalScrollBar();
		}

		private void LayoutHorizontalScrollBar()
		{
			if (RightToLeft == RightToLeft.Yes && _hasVerticalScrollBar)
			{
				_horizontalScrollBar.Location = new Point(_scrollBarPanel.ClientRectangle.Left + GetVerticalScrollBarWidth(), _scrollBarPanel.ClientRectangle.Top);
			}
			else
			{
				_horizontalScrollBar.Location = new Point(_scrollBarPanel.ClientRectangle.X, _scrollBarPanel.ClientRectangle.Top);
			}
			if (_hasVerticalScrollBar)
			{
				_horizontalScrollBar.Size = new Size(_scrollBarPanel.ClientRectangle.Width - GetVerticalScrollBarWidth(), _scrollBarPanel.ClientRectangle.Height);
			}
			else
			{
				_horizontalScrollBar.Size = _scrollBarPanel.ClientRectangle.Size;
			}
		}

		private void OnVerticalScrollBarValueChanged(object sender, EventArgs eventArgs)
		{
			OnScrollBarScroll(VerticalScrollCode);
		}

		private void OnVerticalScrollBarScroll(object sender, ScrollEventArgs eventArgs)
		{
			if (eventArgs.Type == ScrollEventType.ThumbTrack)
			{
				OnScrollBarScroll(VerticalThumbTrackCode);
			}
		}

		private void OnHorizontalScrollBarValueChanged(object sender, EventArgs eventArgs)
		{
			OnScrollBarScroll(HorizontalScrollCode);
		}

		private void OnHorizontalScrollBarScroll(object sender, ScrollEventArgs eventArgs)
		{
			if (eventArgs.Type == ScrollEventType.ThumbTrack)
			{
				OnScrollBarScroll(HorizontalThumbTrackCode);
			}
		}

		public void UpdateBorderStyle(BorderStyle borderStyle)
		{
			if (!Enum.IsDefined(typeof(BorderStyle), borderStyle))
			{
				throw new InvalidEnumArgumentException();
			}
			if (_borderStyle != borderStyle)
			{
				_borderStyle = borderStyle;
				LayoutScrollBars();
				Invalidate();
			}
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			LayoutScrollBars();
			base.OnResize(eventArgs);
		}

		private void UpdateBorderWidth()
		{
			switch (_borderStyle)
			{
			case BorderStyle.Flat:
			case BorderStyle.RaisedLight3D:
			case BorderStyle.SunkenLight3D:
				_borderWidth = ThinBorderWidth;
				break;
			case BorderStyle.Raised:
			case BorderStyle.Sunken:
				_borderWidth = ThickBorderWidth;
				break;
			default:
				_borderWidth = 0;
				break;
			}
		}

		internal void PaintBorder(Graphics graphics, VisualStyleSupport themeState)
		{
			if (_borderStyle != BorderStyle.None)
			{
				Rectangle bounds = base.ClientRectangle;
				graphics.DrawRectangle(MxThemeManager.ResolveDefault(DeviceDpi).Pen(ThemeRole.Border), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
			}
		}

		private void LayoutScrollBars()
		{
			InitializeControls(base.ClientRectangle);
		}

		private void InitializeControls(Rectangle hostBounds)
		{
			if (_scrollBarPanel == null)
			{
				return;
			}
			SuspendLayout();
			UpdateBorderWidth();
			int verticalScrollBarWidth = 0;
			int verticalScrollBarHeight = 0;
			int horizontalScrollBarHeight = 0;
			Rectangle rectangle = hostBounds;
			rectangle.Inflate(-_borderWidth, -_borderWidth);
			// The vertical bar shows only when needed; this host never shows the horizontal bar.
			_hasVerticalScrollBar = _isVerticalScrollNeeded;
			_verticalScrollBar.Enabled = true;
			_hasHorizontalScrollBar = false;
			_horizontalScrollBar.Enabled = true;
			if (_hasVerticalScrollBar)
			{
				verticalScrollBarWidth = GetVerticalScrollBarWidth();
			}
			if (_hasHorizontalScrollBar)
			{
				horizontalScrollBarHeight = GetHorizontalScrollBarHeight();
			}
			verticalScrollBarHeight = rectangle.Height - horizontalScrollBarHeight;
			if (RightToLeft == RightToLeft.Yes)
			{
				_verticalScrollBar.Location = new Point(hostBounds.X, rectangle.Top);
			}
			else
			{
				_verticalScrollBar.Location = new Point(rectangle.Right - verticalScrollBarWidth, rectangle.Top);
			}
			_verticalScrollBar.Size = new Size(verticalScrollBarWidth, verticalScrollBarHeight);
			if (_hasHorizontalScrollBar)
			{
				_scrollBarPanel.Location = new Point(rectangle.Left, rectangle.Bottom - horizontalScrollBarHeight);
				_scrollBarPanel.Size = new Size(rectangle.Width, horizontalScrollBarHeight);
			}
			_scrollBarPanel.Visible = _hasHorizontalScrollBar;
			if (_hasHorizontalScrollBar)
			{
				rectangle.Height -= GetHorizontalScrollBarHeight();
			}
			_horizontalScrollBar.Visible = _hasHorizontalScrollBar;
			OnPanelResize(_scrollBarPanel, new EventArgs());
			_verticalScrollBar.Visible = _hasVerticalScrollBar;
			if (_hasVerticalScrollBar)
			{
				if (RightToLeft == RightToLeft.Yes)
				{
					rectangle.X = _verticalScrollBar.Bounds.Right;
				}
				rectangle.Width -= GetVerticalScrollBarWidth();
			}
			ResumeLayout(performLayout: true);
			if (!rectangle.Equals(_contentBounds))
			{
				_contentBounds = rectangle;
				OnClientResize();
			}
		}

		public Rectangle GetContentBounds()
		{
			return _contentBounds;
		}

		protected override void OnRightToLeftChanged(EventArgs eventArgs)
		{
			base.OnRightToLeftChanged(eventArgs);
			LayoutScrollBars();
		}

		protected virtual void OnClientResize()
		{
		}

		internal bool GetIsVerticalScrollNeeded()
		{
			return _isVerticalScrollNeeded;
		}

		internal void SetIsVerticalScrollNeeded(bool isVerticalScrollNeeded)
		{
			if (_isVerticalScrollNeeded != isVerticalScrollNeeded)
			{
				_isVerticalScrollNeeded = isVerticalScrollNeeded;
				LayoutScrollBars();
			}
		}

		internal int GetHorizontalScrollBarHeight()
		{
			int height = SystemInformation.HorizontalScrollBarHeight;
			return height;
		}

		internal int GetVerticalScrollBarWidth()
		{
			int width = SystemInformation.VerticalScrollBarWidth;
			return width;
		}

		internal void SetVerticalScrollRange(int minimum, int maximum, int pageSize, int value)
		{
			try
			{
				_verticalScrollBar.SetMinimum(minimum);
				_verticalScrollBar.SetMaximum(maximum);
				if (pageSize > 0)
				{
					_verticalScrollBar.SetLargeChange(pageSize);
				}
				else
				{
					_verticalScrollBar.SetLargeChange(1);
				}
				_verticalScrollBar.SetValue(value);
			}
			catch
			{
			}
		}

		internal ThemedScrollBar GetVerticalScrollBar()
		{
			return _verticalScrollBar;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnScrollBarScroll(int ScrollCode)
		{
		}

		internal bool GetIsDropDownOpen()
		{
			return _isDropDownOpen;
		}

		internal void SetIsDropDownOpen(bool isDropDownOpen)
		{
			if (_isDropDownOpen != isDropDownOpen)
			{
				_isDropDownOpen = isDropDownOpen;
				AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
			}
		}

		internal void SetAppearance(ControlAppearance appearance)
		{
			_appearance = appearance;
			try
			{
				_scrollBarPanel.BackColor = _appearance.ControlColor;
				_verticalScrollBar._appearance = appearance;
				_horizontalScrollBar._appearance = appearance;
			}
			catch
			{
			}
		}

		protected override void OnSystemColorsChanged(EventArgs eventArgs)
		{
			base.OnSystemColorsChanged(eventArgs);
			Invalidate();
		}

		public Color GetFlatBorderColor()
		{
			return _flatBorderColor;
		}

		public void SetFlatBorderColor(Color flatBorderColor)
		{
			if (!_flatBorderColor.Equals(flatBorderColor))
			{
				_flatBorderColor = flatBorderColor;
				Invalidate();
			}
		}

		protected override void OnMouseWheel(MouseEventArgs eventArgs)
		{
			base.OnMouseWheel(eventArgs);
			ScrollByWheelDelta(eventArgs.Delta);
		}

		internal void ScrollByWheelDelta(int wheelDelta)
		{
			ScrollByWheelDelta(wheelDelta, SystemInformation.MouseWheelScrollLines);
		}

		internal void ScrollByWheelDelta(int wheelDelta, int scrollLines)
		{
			if (wheelDelta == 0 || !GetIsVerticalScrollNeeded())
			{
				return;
			}
			ThemedScrollBar scrollBar = _verticalScrollBar;
			// A negative line count means the wheel is set to scroll one page at a time.
			int step = ((scrollLines < 0) ? scrollBar.GetLargeChange() : (scrollBar.GetSmallChange() * scrollLines));
			scrollBar.SetValue((wheelDelta < 0) ? (scrollBar.GetValue() + step) : (scrollBar.GetValue() - step));
		}

		protected virtual void OnVisualStyleChanged()
		{
			if (_verticalScrollBar != null)
			{
				_verticalScrollBar.SetUseVisualStyles(true);
			}
			if (_horizontalScrollBar != null)
			{
				_horizontalScrollBar.SetUseVisualStyles(true);
			}
		}

		public void SetBackColor(Color color)
		{
			base.BackColor = color;
		}

	}
}
