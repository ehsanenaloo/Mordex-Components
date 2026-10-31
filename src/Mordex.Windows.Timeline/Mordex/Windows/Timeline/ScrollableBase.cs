using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[ToolboxItem(false)]
	public class ScrollableBase : Control
	{

		private StyledScrollBar _horizontalScrollBarControl;

		internal Panel _horizontalScrollBarPanel;

		private StyledScrollBar _verticalScrollBarControl;

		internal bool _shouldClampScrollValue = true;

		internal int _steps = -1;

		private BorderStyle _borderStyle = BorderStyle.SunkenLight3D;

		private Rectangle _clientBounds;

		private bool _isHorizontalScrollBarVisible;

		private bool _isVerticalScrollBarVisible;

		internal int _borderWidth;

		private int _headerHeight;

		private bool _isVerticalScrollRequired;

		private bool _isHorizontalScrollRequired;

		private ScrollBars _scrollBars = ScrollBars.Automatic;

		private VerticalScrollBar _verticalScrollBar;

		private HorizontalScrollBar _horizontalScrollBar;

		internal ScrollBarStyle _scrollBarStyle;

		private Color _flatBorderColor = Color.Empty;

		private MouseWheelBehavior _mouseWheelBehavior = MouseWheelBehavior.Automatic;

		internal bool _isHorizontalWheelScroll;

		[Category("Appearance")]
		[DefaultValue(BorderStyle.SunkenLight3D)]
		[Description("Gets or sets the Timeline's border style")]
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
					Invalidate(invalidateChildren: true);
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

		[Description("Specifies when the vertical scroll bar will be visible.")]
		[DefaultValue(VerticalScrollBar.Automatic)]
		[Category("Behavior")]
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
						UpdateScrollBars(ScrollBars.Automatic);
						break;
					}
					InitializeControls();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(HorizontalScrollBar.Automatic)]
		[Description("Specifies when the horizontal scroll bar will be visible.")]
		public HorizontalScrollBar HorizontalScrollBar
		{
			get
			{
				return _horizontalScrollBar;
			}
			set
			{
				if (!Enum.IsDefined(typeof(HorizontalScrollBar), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_horizontalScrollBar != value)
				{
					_horizontalScrollBar = value;
					InitializeControls();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		[Description("Representes the style used to draw the vertical scroll bar.")]
		public ScrollBarStyle ScrollBarStyle => _scrollBarStyle;

		[Description("The color of the border of the headers and the control when its border is Flat.")]
		[Category("Appearance")]
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
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Description("Indicates which scroll bar will recive the action when the mouse wheel is rotated.")]
		[Category("Behavior")]
		[DefaultValue(MouseWheelBehavior.Automatic)]
		public MouseWheelBehavior MouseWheelBehavior
		{
			get
			{
				return _mouseWheelBehavior;
			}
			set
			{
				_mouseWheelBehavior = value;
			}
		}

		public ScrollableBase()
		{
			UpdateScrollableBase();
			CreateScrollBars();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			InitializeControls();
		}

		internal Rectangle GetClientRectangle()
		{
			return base.ClientRectangle;
		}

		private void UpdateScrollableBase()
		{
			BackColor = SystemColors.Window;
		}

		// The panel behind the horizontal scroll bar has no paint code of its own, so it takes the theme track color.
		internal void RefreshHorizontalPanelColor()
		{
			_horizontalScrollBarPanel.BackColor = MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.ScrollTrack);
			Invalidate(invalidateChildren: true);
		}

		private void CreateScrollBars()
		{
			_scrollBarStyle = new ScrollBarStyle(this);
			_verticalScrollBarControl = new StyledScrollBar(ScrollBarType.Vertical);
			_verticalScrollBarControl._scrollBarStyle = _scrollBarStyle;
			_horizontalScrollBarControl = new StyledScrollBar(ScrollBarType.Horizontal);
			_horizontalScrollBarControl._scrollBarStyle = _scrollBarStyle;
			_horizontalScrollBarControl._isClampAllowed = true;
			_horizontalScrollBarPanel = new Panel();
			_verticalScrollBarControl.Visible = false;
			_verticalScrollBarControl.TabStop = false;
			_verticalScrollBarControl.AddScrollHandler(verticalScroll_Scroll);
			_verticalScrollBarControl.AddValueChangedHandler(verticalScroll_ValueChanged);
			_horizontalScrollBarControl.TabStop = false;
			_horizontalScrollBarControl.AddUpdatePositionHandler(SetSteps);
			_horizontalScrollBarControl.AddScrollHandler(horizontalScroll_Scroll);
			_horizontalScrollBarControl.AddValueChangedHandler(horizontalScroll_ValueChanged);
			_horizontalScrollBarControl.AddFindPreviousHandler(horizontalScroll_ScrollFindPrevious);
			_horizontalScrollBarControl.AddFindNextHandler(horizontalScroll_ScrollFindNext);
			_horizontalScrollBarPanel.TabStop = false;
			_horizontalScrollBarPanel.Resize += OnPanelResize;
			_horizontalScrollBarControl.VisibleChanged += OnStateVisibleChanged;
			_verticalScrollBarControl.VisibleChanged += OnStateVisibleChanged;
			_horizontalScrollBarPanel.BackColor = SystemColors.Control;
			_horizontalScrollBarPanel.Visible = false;
			BackColor = SystemColors.Window;
			base.Controls.Add(_horizontalScrollBarPanel);
			_horizontalScrollBarPanel.Controls.Add(_horizontalScrollBarControl);
			base.Controls.Add(_verticalScrollBarControl);
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

		protected internal virtual void OnVerticalScroll(ScrollEventType eventType)
		{
		}

		protected internal virtual void OnHorizontalScroll(ScrollEventType eventType)
		{
		}

		protected internal virtual void OnScrollFindNext(EventArgs e)
		{
		}

		protected internal virtual void OnScrollFindPrevious(EventArgs e)
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

		internal void SetSteps(object sender, EventArgs eventArgs)
		{
			_steps = _horizontalScrollBarControl.GetScrollValue();
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
			OnHorizontalScroll(e.Type);
		}

		private void OnStateVisibleChanged(object sender, EventArgs eventArgs)
		{
			Invalidate(invalidateChildren: true);
		}

		protected void horizontalScroll_ScrollFindNext(object sender, EventArgs e)
		{
			OnScrollFindNext(e);
		}

		protected void horizontalScroll_ScrollFindPrevious(object sender, EventArgs e)
		{
			OnScrollFindPrevious(e);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			InitializeControls();
		}

		private void UpdateBorderWidth()
		{
			_borderWidth = _borderStyle != BorderStyle.None ? 1 : 0;
		}

		internal void RenderContent(Graphics graphics, Rectangle rectangle, TimelineThemeRenderer renderer, BorderStyle borderStyle)
		{
			if (borderStyle == BorderStyle.None)
			{
				return;
			}
			if (renderer != null)
			{
				using (Pen pen = new Pen(renderer._controlBorderColor))
				{
					TimelineUtilities.DrawFlatBorder(graphics, pen, rectangle);
				}
			}
			else
			{
				TimelineUtilities.RenderContent(graphics, _scrollBarStyle, rectangle, borderStyle);
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

		internal int GetHorizontalScrollBarHeight()
		{
			return SystemInformation.GetHorizontalScrollBarHeightForDpi(DeviceDpi);
		}

		internal int GetVerticalScrollBarWidth()
		{
			return SystemInformation.GetVerticalScrollBarWidthForDpi(DeviceDpi);
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
			switch (VerticalScrollBar)
			{
			case VerticalScrollBar.Automatic:
				_isVerticalScrollBarVisible = _isVerticalScrollRequired;
				break;
			case VerticalScrollBar.Never:
				_isVerticalScrollBarVisible = false;
				break;
			default:
				_isVerticalScrollBarVisible = true;
				break;
			}
			switch (HorizontalScrollBar)
			{
			case HorizontalScrollBar.Automatic:
				_isHorizontalScrollBarVisible = _isHorizontalScrollRequired;
				break;
			case HorizontalScrollBar.Never:
				_isHorizontalScrollBarVisible = false;
				break;
			default:
				_isHorizontalScrollBarVisible = true;
				break;
			}
			_verticalScrollBarControl.Enabled = !_isVerticalScrollBarVisible || _isVerticalScrollRequired;
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
			verticalScrollBarHeight = clientRectangle.Height - horizontalScrollBarHeight;
			if (IsRightToLeft())
			{
				_verticalScrollBarControl.Location = clientRectangle.Location;
			}
			else
			{
				_verticalScrollBarControl.Location = new Point(clientRectangle.Right - verticalScrollBarWidth, clientRectangle.Top);
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
			else if (_isHorizontalScrollRequired || HorizontalScrollBar == HorizontalScrollBar.Always)
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

		internal virtual void OnAreaStyleChanged(EventArgs eventArgs)
		{
		}

		internal BorderStyle GetEffectiveBorderStyle()
		{
			return BorderStyle == BorderStyle.None ? BorderStyle.None : BorderStyle.Flat;
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
				OnClientResize();
			}
		}

		internal void UpdateHorizontalScrollBar(int minimum, int integerArgument, int largeChange, int value)
		{
			ApplyHorizontalScrollBar(minimum, integerArgument, largeChange, value, true);
		}

		internal void ApplyHorizontalScrollBar(int minimum, int integerArgument, int largeChange, int value, bool shouldClampScrollValue)
		{
			try
			{
				_shouldClampScrollValue = shouldClampScrollValue;
				_horizontalScrollBarControl._shouldClampScrollValue = shouldClampScrollValue;
				_horizontalScrollBarControl.UpdateMinimum(minimum);
				_horizontalScrollBarControl.UpdateMaximum(integerArgument);
				if (largeChange > 0)
				{
					_horizontalScrollBarControl.UpdateLargeChange(largeChange);
				}
				_horizontalScrollBarControl.UpdateScrollValue(value);
			}
			catch (ArgumentException)
			{
				// The range can be inconsistent while the control is resized; the next layout pass sets it again.
			}
		}

		internal StyledScrollBar GetHorizontalScrollBarControl()
		{
			return _horizontalScrollBarControl;
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

		protected virtual void OnScrollBarScroll(int ScrollCode)
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

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			_ = _scrollBarStyle;
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
				if (_mouseWheelBehavior != MouseWheelBehavior.Horizontal && IsVerticalScrollRequired())
				{
					StyledScrollBar verticalScrollBarControl = _verticalScrollBarControl;
					verticalScrollBarControl.UpdateScrollValue(verticalScrollBarControl.GetScrollValue() + _verticalScrollBarControl.GetSmallChange() * SystemInformation.MouseWheelScrollLines);
					_isHorizontalWheelScroll = false;
				}
				else if (IsHorizontalScrollRequired() && MouseWheelBehavior != MouseWheelBehavior.Vertical)
				{
					StyledScrollBar horizontalScrollBarControl = _horizontalScrollBarControl;
					horizontalScrollBarControl.UpdateScrollValue(horizontalScrollBarControl.GetScrollValue() + _horizontalScrollBarControl.GetSmallChange() * SystemInformation.MouseWheelScrollLines);
					_isHorizontalWheelScroll = true;
				}
			}
			else if (delta > 0)
			{
				if (_mouseWheelBehavior != MouseWheelBehavior.Horizontal && IsVerticalScrollRequired())
				{
					StyledScrollBar scrollableBaseScrollBarStyleControl2Argument = _verticalScrollBarControl;
					scrollableBaseScrollBarStyleControl2Argument.UpdateScrollValue(scrollableBaseScrollBarStyleControl2Argument.GetScrollValue() - _verticalScrollBarControl.GetSmallChange() * SystemInformation.MouseWheelScrollLines);
					_isHorizontalWheelScroll = false;
				}
				else if (IsHorizontalScrollRequired() && MouseWheelBehavior != MouseWheelBehavior.Vertical)
				{
					StyledScrollBar scrollBarStyleControl2Argument = _horizontalScrollBarControl;
					scrollBarStyleControl2Argument.UpdateScrollValue(scrollBarStyleControl2Argument.GetScrollValue() - _horizontalScrollBarControl.GetSmallChange() * SystemInformation.MouseWheelScrollLines);
					_isHorizontalWheelScroll = true;
				}
			}
		}
	}
}
