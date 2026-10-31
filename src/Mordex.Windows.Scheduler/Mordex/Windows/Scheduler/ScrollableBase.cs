using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[ToolboxItem(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class ScrollableBase : Control
	{

		private StyledScrollBar _horizontalScrollBarControl;

		internal Panel _horizontalScrollBarPanel;

		private StyledScrollBar _verticalScrollBarControl;

		private int _headerHeight;

		private BorderStyle _borderStyle = BorderStyle.SunkenLight3D;

		private Rectangle _clientBounds;

		private bool _isHorizontalScrollBarVisible;

		private bool _isVerticalScrollBarVisible;

		internal int _borderWidth;

		private bool _isVerticalScrollRequired;

		private bool _isHorizontalScrollRequired;

		private ScrollBars _scrollBars = ScrollBars.Automatic;

		private VerticalScrollBar _verticalScrollBar;

		internal ScrollBarStyle _scrollBarStyle;

		private Color _flatBorderColor = Color.Empty;

		[DefaultValue(BorderStyle.SunkenLight3D)]
		[Description("Gets or sets the Schedule's border style")]
		[Category("Appearance")]
		public BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_borderStyle != value)
				{
					_borderStyle = value;
					InitializeControls();
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Size ClientSize
		{
			get
			{
				return ClientRectangle.Size;
			}
			set
			{
				UpdateBorderWidth();
				Size clientSize = new Size(value.Width + _borderWidth * 2, value.Height + _borderWidth * 2);
				if (_isHorizontalScrollBarVisible)
				{
					clientSize.Height += _horizontalScrollBarPanel.Height;
				}
				if (_isVerticalScrollBarVisible)
				{
					clientSize.Width += _verticalScrollBarControl.Width;
				}
				base.ClientSize = clientSize;
			}
		}

		public new Rectangle ClientRectangle => _clientBounds;

		public override RightToLeft RightToLeft
		{
			get
			{
				return base.RightToLeft;
			}
			set
			{
				base.RightToLeft = value;
				InitializeControls();
			}
		}

		[Category("Behavior")]
		[DefaultValue(VerticalScrollBar.Automatic)]
		[Description("Indicates when the vertical scroll bar will be visible.")]
		public VerticalScrollBar VerticalScrollBar
		{
			get
			{
				return _verticalScrollBar;
			}
			set
			{
				if (!Enum.IsDefined(typeof(VerticalScrollBar), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_verticalScrollBar != value)
				{
					_verticalScrollBar = value;
					switch (_verticalScrollBar)
					{
					case VerticalScrollBar.Always:
						UpdateScrollBars(ScrollBars.Vertical);
						break;
					case VerticalScrollBar.Automatic:
						UpdateScrollBars(ScrollBars.Automatic);
						break;
					case VerticalScrollBar.Never:
						UpdateScrollBars(ScrollBars.None);
						break;
					}
				}
			}
		}

		[Description("Representes the style used to draw the vertical scroll bar.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		public ScrollBarStyle ScrollBarStyle => _scrollBarStyle;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("The color of the border and headers and the control when it\u00b4s border is Flat.")]
		public Color FlatBorderColor
		{
			get
			{
				if (_flatBorderColor.IsEmpty)
				{
					return SystemColors.ControlDark;
				}
				return _flatBorderColor;
			}
			set
			{
				if (!_flatBorderColor.Equals(value))
				{
					_flatBorderColor = value;
					Invalidate();
				}
			}
		}

		public ScrollableBase()
		{
			UpdateScrollableBase();
			CreateScrollBars();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void UpdateScrollableBase()
		{
		}

		// The panel behind the horizontal scroll bar has no paint code of its own, so it takes the theme track color.
		internal void ApplyThemedScrollBarColors()
		{
			_horizontalScrollBarPanel.BackColor = MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.ScrollTrack);
			Invalidate(invalidateChildren: true);
		}

		internal void RefreshScrollBarColors()
		{
			ApplyThemedScrollBarColors();
		}

		private void CreateScrollBars()
		{
			_scrollBarStyle = new ScrollBarStyle(this);
			_verticalScrollBarControl = new StyledScrollBar(ScrollBarType.Vertical);
			_verticalScrollBarControl._scrollBarStyle = _scrollBarStyle;
			_horizontalScrollBarControl = new StyledScrollBar(ScrollBarType.Horizontal);
			_horizontalScrollBarControl._scrollBarStyle = _scrollBarStyle;
			_horizontalScrollBarPanel = new Panel();
			_verticalScrollBarControl.Visible = false;
			_verticalScrollBarControl.TabStop = false;
			_verticalScrollBarControl.AddScrollHandler(verticalScroll_Scroll);
			_verticalScrollBarControl.AddScrollingHandler(verticalScroll_Scrolling);
			_verticalScrollBarControl.AddValueChangedHandler(verticalScroll_ValueChanged);
			_horizontalScrollBarControl.TabStop = false;
			_horizontalScrollBarControl.AddScrollHandler(horizontalScroll_Scroll);
			_horizontalScrollBarControl.AddValueChangedHandler(horizontalScroll_ValueChanged);
			_horizontalScrollBarPanel.TabStop = false;
			_horizontalScrollBarPanel.Resize += OnPanelResize;
			_horizontalScrollBarPanel.BackColor = SystemColors.Control;
			_horizontalScrollBarPanel.Visible = false;
			BackColor = SystemColors.Window;
			base.Controls.Add(_horizontalScrollBarPanel);
			_horizontalScrollBarPanel.Controls.Add(_horizontalScrollBarControl);
			base.Controls.Add(_verticalScrollBarControl);
			_headerHeight = 100;
		}

		private bool IsRightToLeft()
		{
			if (RightToLeft == RightToLeft.Yes)
			{
				return true;
			}
			return false;
		}

		private void OnPanelResize(object sender, EventArgs eventArgs)
		{
			LayoutHorizontalScrollBar();
		}

		private void LayoutHorizontalScrollBar()
		{
			if (IsRightToLeft() && _isVerticalScrollBarVisible)
			{
				_horizontalScrollBarControl.Location = new Point(_horizontalScrollBarPanel.ClientRectangle.Left + GetVerticalScrollBarWidth(), _horizontalScrollBarPanel.ClientRectangle.Top);
			}
			else
			{
				_horizontalScrollBarControl.Location = new Point(_horizontalScrollBarPanel.ClientRectangle.X, _horizontalScrollBarPanel.ClientRectangle.Top);
			}
			if (_isVerticalScrollBarVisible)
			{
				_horizontalScrollBarControl.Size = new Size(_horizontalScrollBarPanel.ClientRectangle.Width - GetVerticalScrollBarWidth(), _horizontalScrollBarPanel.ClientRectangle.Height);
			}
			else
			{
				_horizontalScrollBarControl.Size = _horizontalScrollBarPanel.ClientRectangle.Size;
			}
		}

		protected void verticalScroll_ValueChanged(object sender, EventArgs e)
		{
			OnScrollBarScroll(0);
		}

		protected void verticalScroll_Scrolling(object sender, CancelEventArgs e)
		{
			OnScrolling(e);
		}

		protected internal virtual void OnVerticalScroll(ScrollEventType eventType)
		{
		}

		protected void verticalScroll_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.Type == ScrollEventType.ThumbTrack)
			{
				OnScrollBarScroll(1);
			}
			OnVerticalScroll(e.Type);
		}

		protected void horizontalScroll_ValueChanged(object sender, EventArgs e)
		{
			OnScrollBarScroll(7);
		}

		protected void horizontalScroll_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.Type == ScrollEventType.ThumbTrack)
			{
				OnScrollBarScroll(8);
			}
		}

		internal BorderStyle GetEffectiveBorderStyle()
		{
			if (BorderStyle == BorderStyle.None)
			{
				return BorderStyle;
			}
			return BorderStyle.Flat;
		}

		internal virtual int GetHeaderControlWidth()
		{
			int width = base.ClientRectangle.Width;
			return width - _borderWidth * 2;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			InitializeControls();
		}

		private void UpdateBorderWidth()
		{
			if (_borderStyle != BorderStyle.None)
			{
				_borderWidth = 1;
				return;
			}
			switch (GetEffectiveBorderStyle())
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
			default:
				_borderWidth = 0;
				break;
			}
		}

		internal void DrawBorder(Graphics graphics, SchedulerThemeColors renderer)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (renderer != null && GetEffectiveBorderStyle() != BorderStyle.None)
			{
				clientRectangle = SchedulerRenderer.DrawBorder(graphics, GdiCache.Pen(renderer._borderColor), clientRectangle);
			}
			else
			{
				SchedulerRenderer.RenderContent(graphics, _scrollBarStyle, clientRectangle, GetEffectiveBorderStyle());
			}
		}

		internal void InitializeControls()
		{
			if (_horizontalScrollBarPanel == null)
			{
				return;
			}
			SuspendLayout();
			UpdateBorderWidth();
			int verticalScrollBarWidth = 0;
			int verticalScrollBarHeight = 0;
			int horizontalScrollBarHeight = 0;
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-_borderWidth, -_borderWidth);
			if (_scrollBars == ScrollBars.None)
			{
				_isVerticalScrollBarVisible = false;
			}
			else if (_isVerticalScrollRequired || _scrollBars == ScrollBars.Vertical || _scrollBars == ScrollBars.Both)
			{
				_isVerticalScrollBarVisible = true;
			}
			else
			{
				_isVerticalScrollBarVisible = false;
			}
			_verticalScrollBarControl.Enabled = !_isVerticalScrollBarVisible || _isVerticalScrollRequired;
			if (_scrollBars == ScrollBars.None)
			{
				_isHorizontalScrollBarVisible = false;
			}
			else if (_isHorizontalScrollRequired || _scrollBars == ScrollBars.Horizontal || _scrollBars == ScrollBars.Both)
			{
				_isHorizontalScrollBarVisible = true;
			}
			else
			{
				_isHorizontalScrollBarVisible = false;
			}
			_horizontalScrollBarControl.Enabled = !_isHorizontalScrollBarVisible || _isHorizontalScrollRequired;
			if (_isVerticalScrollBarVisible)
			{
				verticalScrollBarWidth = GetVerticalScrollBarWidth();
			}
			else
			{
				_ = clientRectangle.Width;
			}
			if (_isHorizontalScrollBarVisible)
			{
				horizontalScrollBarHeight = GetHorizontalScrollBarHeight();
			}
			_ = clientRectangle.Width;
			verticalScrollBarHeight = clientRectangle.Height - horizontalScrollBarHeight - GetHeaderHeight();
			if (IsRightToLeft())
			{
				_verticalScrollBarControl.Location = clientRectangle.Location;
			}
			else
			{
				_verticalScrollBarControl.Location = new Point(clientRectangle.Right - verticalScrollBarWidth, clientRectangle.Top + GetHeaderHeight());
			}
			_verticalScrollBarControl.Size = new Size(verticalScrollBarWidth, verticalScrollBarHeight);
			if (_isHorizontalScrollBarVisible)
			{
				_horizontalScrollBarPanel.Location = new Point(clientRectangle.Left, clientRectangle.Bottom - horizontalScrollBarHeight);
				_horizontalScrollBarPanel.Size = new Size(clientRectangle.Width, horizontalScrollBarHeight);
			}
			_horizontalScrollBarPanel.Visible = _isHorizontalScrollBarVisible;
			if (_isHorizontalScrollBarVisible)
			{
				clientRectangle.Height -= GetHorizontalScrollBarHeight();
			}
			if (_scrollBars == ScrollBars.None)
			{
				_isHorizontalScrollBarVisible = false;
			}
			else if (_isHorizontalScrollRequired || _scrollBars == ScrollBars.Horizontal || _scrollBars == ScrollBars.Both)
			{
				_isHorizontalScrollBarVisible = true;
			}
			else
			{
				_isHorizontalScrollBarVisible = false;
			}
			_horizontalScrollBarControl.Visible = _isHorizontalScrollBarVisible;
			OnPanelResize(_horizontalScrollBarPanel, new EventArgs());
			_verticalScrollBarControl.Visible = _isVerticalScrollBarVisible;
			if (_isVerticalScrollBarVisible)
			{
				if (IsRightToLeft())
				{
					clientRectangle.X = _verticalScrollBarControl.Bounds.Right;
				}
				else
				{
					clientRectangle.Width -= GetVerticalScrollBarWidth();
				}
			}
			ResumeLayout(performLayout: true);
			if (!clientRectangle.Equals(_clientBounds))
			{
				_clientBounds = clientRectangle;
				OnClientResize();
			}
		}

		protected virtual void OnClientResize()
		{
		}

		internal bool IsVerticalScrollRequired()
		{
			return _isVerticalScrollRequired;
		}

		internal void SetVerticalScrollRequired(bool isRequired)
		{
			if (_isVerticalScrollRequired != isRequired)
			{
				_isVerticalScrollRequired = isRequired;
				InitializeControls();
			}
		}

		internal bool IsHorizontalScrollRequired()
		{
			return _isHorizontalScrollRequired;
		}

		internal void SetHorizontalScrollRequired(bool isRequired)
		{
			if (_isHorizontalScrollRequired != isRequired)
			{
				_isHorizontalScrollRequired = isRequired;
				InitializeControls();
			}
		}

		internal void UpdateHorizontalScrollBar(int minimum, int integerArgument, int largeChange, int value)
		{
			try
			{
				_horizontalScrollBarControl.UpdateMinimum(minimum);
				_horizontalScrollBarControl.UpdateMaximum(integerArgument);
				if (largeChange > 0)
				{
					_horizontalScrollBarControl.UpdateLargeChange(largeChange);
				}
				_horizontalScrollBarControl.UpdateScrollValue(value);
			}
			catch
			{
			}
		}

		internal StyledScrollBar GetHorizontalScrollBarControl()
		{
			return _horizontalScrollBarControl;
		}

		internal int GetHorizontalScrollBarHeight()
		{
			int height = SystemInformation.GetHorizontalScrollBarHeightForDpi(DeviceDpi);
			return height;
		}

		internal int GetVerticalScrollBarWidth()
		{
			int width = SystemInformation.GetVerticalScrollBarWidthForDpi(DeviceDpi);
			return width;
		}

		internal void UpdateVerticalScrollBar(int minimum, int integerArgument, int largeChange, int value)
		{
			try
			{
				_verticalScrollBarControl.UpdateMinimum(minimum);
				_verticalScrollBarControl.UpdateMaximum(integerArgument);
				if (largeChange > 0)
				{
					_verticalScrollBarControl.UpdateLargeChange(largeChange);
				}
				else
				{
					_verticalScrollBarControl.UpdateLargeChange(1);
				}
				_verticalScrollBarControl.UpdateScrollValue(value);
			}
			catch
			{
			}
		}

		internal StyledScrollBar GetVerticalScrollBarControl()
		{
			return _verticalScrollBarControl;
		}

		private void UpdateScrollBars(ScrollBars scrollBars)
		{
			if (_scrollBars != scrollBars)
			{
				_scrollBars = scrollBars;
				InitializeControls();
			}
		}

		internal int GetHeaderHeight()
		{
			return _headerHeight;
		}

		internal void SetHeaderHeight(int headerHeight)
		{
			if (_headerHeight != headerHeight)
			{
				_headerHeight = headerHeight;
				InitializeControls();
			}
		}

		protected virtual void OnScrollBarScroll(int ScrollCode)
		{
		}

		internal virtual void OnAreaStyleChanged(EventArgs eventArgs)
		{
		}

		protected internal virtual void OnScrolling(CancelEventArgs e)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeScrollBarStyle()
		{
			return _scrollBarStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetScrollBarStyle()
		{
			if (_scrollBarStyle.ShouldSerialize())
			{
				UpdateScrollBarStyle(new ScrollBarStyle(this));
			}
		}

		internal void UpdateScrollBarStyle(ScrollBarStyle scrollBarStyle)
		{
			_scrollBarStyle = scrollBarStyle;
			try
			{
				_horizontalScrollBarPanel.BackColor = _scrollBarStyle.ControlColor;
				_verticalScrollBarControl._scrollBarStyle = scrollBarStyle;
				_horizontalScrollBarControl._scrollBarStyle = scrollBarStyle;
			}
			catch
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !_flatBorderColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = Color.Empty;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			ScrollByWheelDelta(e.Delta);
		}

		internal void ScrollByWheelDelta(int delta)
		{
			if (delta < 0)
			{
				if (IsVerticalScrollRequired())
				{
					StyledScrollBar verticalScrollBarControl = _verticalScrollBarControl;
					verticalScrollBarControl.UpdateScrollValue(verticalScrollBarControl.GetScrollValue() + _verticalScrollBarControl.GetSmallChange() * SystemInformation.MouseWheelScrollLines);
				}
			}
			else if (delta > 0 && IsVerticalScrollRequired())
			{
				StyledScrollBar scrollableBaseScrollBarStyleControl2Argument = _verticalScrollBarControl;
				scrollableBaseScrollBarStyleControl2Argument.UpdateScrollValue(scrollableBaseScrollBarStyleControl2Argument.GetScrollValue() - _verticalScrollBarControl.GetSmallChange() * SystemInformation.MouseWheelScrollLines);
			}
		}

	}
}
