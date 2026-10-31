using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	[ToolboxItem(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class ScrollableBase : ContainerControl
	{

		private RibbonControl _horizontalScrollBar;

		internal Panel _cornerPanel;

		private RibbonControl _verticalScrollBar;

		private Point _scrollPosition;

		private Rectangle _clientBounds;

		private bool _isHorizontalScrollBarVisible;

		private bool _isVerticalScrollBarVisible;

		internal int _borderWidth = default;

		private Size _minSize;

		private bool _needsVerticalScrollBar;

		private bool _needsHorizontalScrollBar;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Point ScrollPosition
		{
			get
			{
				return _scrollPosition;
			}
			set
			{
				if (_scrollPosition != value)
				{
					_scrollPosition = value;
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
				Size clientSize = new Size(value.Width + _borderWidth * 2, value.Height + _borderWidth * 2);
				if (_isHorizontalScrollBarVisible)
				{
					clientSize.Height += _cornerPanel.Height;
				}
				if (_isVerticalScrollBarVisible)
				{
					clientSize.Width += _verticalScrollBar.Width;
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
				ComputeLayout();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Size MinSize
		{
			get
			{
				return _minSize;
			}
			set
			{
				if (_minSize != value)
				{
					_minSize = value;
					ApplyScrollBarNeeds();
				}
			}
		}

		public ScrollableBase()
		{
			InitializeBackColor();
			CreateScrollBars();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
			ComputeLayout();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void InitializeBackColor()
		{
			BackColor = SystemColors.Window;
		}

		private void CreateScrollBars()
		{
			_verticalScrollBar = new RibbonControl(ScrollBarType.Vertical);
			_horizontalScrollBar = new RibbonControl(ScrollBarType.Horizontal);
			_cornerPanel = new Panel();
			_verticalScrollBar.Visible = false;
			_verticalScrollBar.TabStop = false;
			_verticalScrollBar.AddScrollHandler(verticalScroll_Scroll);
			_verticalScrollBar.AddValueChangedHandler(verticalScroll_ValueChanged);
			_horizontalScrollBar.TabStop = false;
			_horizontalScrollBar.AddScrollHandler(horizontalScroll_Scroll);
			_horizontalScrollBar.AddValueChangedHandler(horizontalScroll_ValueChanged);
			_cornerPanel.TabStop = false;
			_cornerPanel.Resize += OnPanelResize;
			_horizontalScrollBar.VisibleChanged += OnStateVisibleChanged;
			_verticalScrollBar.VisibleChanged += OnStateVisibleChanged;
			_cornerPanel.BackColor = SystemColors.Control;
			_cornerPanel.Visible = false;
			BackColor = SystemColors.Window;
			base.Controls.Add(_cornerPanel);
			_cornerPanel.Controls.Add(_horizontalScrollBar);
			base.Controls.Add(_verticalScrollBar);
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
				_horizontalScrollBar.Location = new Point(_cornerPanel.ClientRectangle.Left + GetVerticalScrollBarWidth(), _cornerPanel.ClientRectangle.Top);
			}
			else
			{
				_horizontalScrollBar.Location = new Point(_cornerPanel.ClientRectangle.X, _cornerPanel.ClientRectangle.Top);
			}
			if (_isVerticalScrollBarVisible)
			{
				_horizontalScrollBar.Size = new Size(_cornerPanel.ClientRectangle.Width - GetVerticalScrollBarWidth(), _cornerPanel.ClientRectangle.Height);
			}
			else
			{
				_horizontalScrollBar.Size = _cornerPanel.ClientRectangle.Size;
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

		internal int GetHorizontalScrollBarHeight()
		{
			int height = SystemInformation.GetHorizontalScrollBarHeightForDpi(DeviceDpi);
			height -= height % 2;
			return height;
		}

		internal int GetVerticalScrollBarWidth()
		{
			int width = SystemInformation.GetVerticalScrollBarWidthForDpi(DeviceDpi);
			width -= width % 2;
			return width;
		}

		protected virtual void ComputeLayout()
		{
			if (_cornerPanel == null)
			{
				return;
			}
			SuspendLayout();
			int verticalBarWidth = 0;
			int verticalBarHeight = 0;
			int horizontalBarHeight = 0;
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-_borderWidth, -_borderWidth);
			_isVerticalScrollBarVisible = _needsVerticalScrollBar;
			_isHorizontalScrollBarVisible = _needsHorizontalScrollBar;
			_verticalScrollBar.Enabled = !_isVerticalScrollBarVisible || _needsVerticalScrollBar;
			_horizontalScrollBar.Enabled = !_isHorizontalScrollBarVisible || _needsHorizontalScrollBar;
			if (_isVerticalScrollBarVisible)
			{
				verticalBarWidth = GetVerticalScrollBarWidth();
			}
			if (_isHorizontalScrollBarVisible)
			{
				horizontalBarHeight = GetHorizontalScrollBarHeight();
			}
			verticalBarHeight = clientRectangle.Height - horizontalBarHeight - 2;
			if (IsRightToLeft())
			{
				_verticalScrollBar.Location = new Point(clientRectangle.Location.X, clientRectangle.Location.Y + 2);
			}
			else
			{
				_verticalScrollBar.Location = new Point(clientRectangle.Right - verticalBarWidth, clientRectangle.Top + 2);
			}
			_verticalScrollBar.Size = new Size(verticalBarWidth, verticalBarHeight);
			if (_isHorizontalScrollBarVisible)
			{
				_cornerPanel.Location = new Point(clientRectangle.Left, clientRectangle.Bottom - horizontalBarHeight);
				_cornerPanel.Size = new Size(clientRectangle.Width, horizontalBarHeight);
			}
			_cornerPanel.Visible = _isHorizontalScrollBarVisible;
			if (_isHorizontalScrollBarVisible)
			{
				clientRectangle.Height -= GetHorizontalScrollBarHeight();
			}
			if (_needsHorizontalScrollBar)
			{
				_isHorizontalScrollBarVisible = true;
			}
			else
			{
				_isHorizontalScrollBarVisible = false;
			}
			_horizontalScrollBar.Visible = _isHorizontalScrollBarVisible;
			OnPanelResize(_cornerPanel, new EventArgs());
			_verticalScrollBar.Visible = _isVerticalScrollBarVisible;
			if (_isVerticalScrollBarVisible)
			{
				if (IsRightToLeft())
				{
					clientRectangle.X = _verticalScrollBar.Bounds.Right;
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
			int contentHeight = Math.Max(MinSize.Height, base.Height);
			int contentWidth = Math.Max(MinSize.Width, base.Width);
			_verticalScrollBar.SetSmallChange(5);
			_horizontalScrollBar.SetSmallChange(5);
			ConfigureVerticalScrollBar(0, contentHeight, base.Height, ScrollPosition.Y);
			ConfigureHorizontalScrollBar(0, contentWidth, base.Width, ScrollPosition.X);
		}

		protected virtual void OnClientResize()
		{
		}

		internal bool NeedsVerticalScrollBar()
		{
			return _needsVerticalScrollBar;
		}

		internal void SetNeedsVerticalScrollBar(bool needed)
		{
			if (_needsVerticalScrollBar != needed)
			{
				_needsVerticalScrollBar = needed;
			}
		}

		internal void SetNeedsHorizontalScrollBar(bool needed)
		{
			if (_needsHorizontalScrollBar != needed)
			{
				_needsHorizontalScrollBar = needed;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			ApplyScrollBarNeeds();
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			ScrollByWheel(e.Delta);
		}

		internal void ScrollByWheel(int delta)
		{
			if (delta < 0)
			{
				if (NeedsVerticalScrollBar())
				{
					RibbonControl scrollableBaseRibbonControl = _verticalScrollBar;
					scrollableBaseRibbonControl.SetScrollValue(scrollableBaseRibbonControl.GetScrollValue() + _verticalScrollBar.GetSmallChange() * SystemInformation.MouseWheelScrollLines);
				}
			}
			else if (delta > 0 && NeedsVerticalScrollBar())
			{
				RibbonControl scrollUpBar = _verticalScrollBar;
				scrollUpBar.SetScrollValue(scrollUpBar.GetScrollValue() - _verticalScrollBar.GetSmallChange() * SystemInformation.MouseWheelScrollLines);
			}
		}

		private void ApplyScrollBarNeeds()
		{
			SetNeedsHorizontalScrollBar(_minSize.Width > base.Width);
			SetNeedsVerticalScrollBar(_minSize.Height > base.Height);
			ComputeLayout();
		}

		internal void ConfigureHorizontalScrollBar(int minimum, int maximum, int pageSize, int value)
		{
			_horizontalScrollBar.SetMinimum(minimum);
			_horizontalScrollBar.SetMaximum(maximum);
			if (pageSize > 0)
			{
				_horizontalScrollBar.SetLargeChange(pageSize);
			}
			_horizontalScrollBar.SetScrollValue(value);
		}

		internal RibbonControl GetHorizontalScrollBar()
		{
			return _horizontalScrollBar;
		}

		internal void ConfigureVerticalScrollBar(int minimum, int maximum, int pageSize, int value)
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
			_verticalScrollBar.SetScrollValue(value);
		}

		internal RibbonControl GetVerticalScrollBar()
		{
			return _verticalScrollBar;
		}

		protected virtual void OnScrollBarScroll(int ScrollCode)
		{
			ScrollPosition = new Point(_horizontalScrollBar.GetScrollValue(), _verticalScrollBar.GetScrollValue());
			switch (ScrollCode)
			{
			default:
				break;
			}
			ComputeLayout();
		}
	}
}
