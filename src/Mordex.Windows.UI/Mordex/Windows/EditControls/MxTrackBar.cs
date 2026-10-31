using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Controls
{
	[DefaultProperty("Value")]
	[DefaultEvent("ValueChanged")]
	[ToolboxItem(true)]
	[DefaultBindingProperty("Value")]
	public partial class MxTrackBar : Control
	{
		private const int TrackEndInset = 5;

		private const int TrackMargin = 8;

		private const int TrackThickness = 4;

		private const int TrackCrossOffset = 10;

		private const int TickBandSize = 10;

		private const int ThumbLength = 21;

		private const int ThumbWidth = 11;

		private const int ThumbCrossOffset = 8;

		private const int TickNearOffset = 6;

		private const int TickFarOffset = 10;

		private const float TickLength = 2f;

		private const float EndTickLength = 3f;

		private const int TrackHitInflate = 3;

		private const int RepeatInitialDelayMs = 500;

		private const int RepeatIntervalMs = 30;

		private Container _container = new Container();

		private System.Windows.Forms.Timer _repeatTimer;

		private bool _isDraggingThumb;

		private bool _isLayoutDirty = true;

		private bool _isPagingTowardMax = true;

		private int _valueBeforeDrag;

		private Rectangle _trackBounds;

		private Rectangle _thumbBounds;

		private int _largeChange = 5;

		private int _smallChange = 1;

		private bool _showFocusRectangle = true;

		private int _value;

		private bool _isUserChange;

		private int _maximum = 10;

		private int _minimum;

		private TickStyle _tickStyle = TickStyle.BottomRight;

		private Color _thumbColor = SystemColors.Control;

		private Orientation _orientation;

		private bool _isThumbHot;

		private bool _isThumbPressed;

		private static readonly object _valueChangedKey = new object();

		private EventHandler _scrollHandler;

		private VisualStylePainter _themeRenderer;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		[DefaultValue(5)]
		[Description("The number of positions added to or subtracted from the Value property when the slider is moved a large distance.")]
		public int LargeChange
		{
			get
			{
				return _largeChange;
			}
			set
			{
				_largeChange = value;
			}
		}

		[Description("The number of positions added to or subtracted from the Value property when the slider is moved a small distance.")]
		[DefaultValue(1)]
		public int SmallChange
		{
			get
			{
				return _smallChange;
			}
			set
			{
				_smallChange = value;
			}
		}

		[Description("Indicates whether the control should display focus rectangles.")]
		[DefaultValue(true)]
		public bool ShowFocusRectangle
		{
			get
			{
				return _showFocusRectangle;
			}
			set
			{
				if (_showFocusRectangle != value)
				{
					_showFocusRectangle = value;
					Invalidate();
				}
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
			}
		}

		[Description("Represents the current position for the progress bar, in the range specified by the Minimum and Maximum properties.")]
		[DefaultValue(0)]
		[Bindable(BindableSupport.Yes)]
		public int Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (value < _minimum)
				{
					value = _minimum;
				}
				if (value > _maximum)
				{
					value = _maximum;
				}
				if (_value != value)
				{
					_value = value;
					if (_isUserChange)
					{
						OnScroll(EventArgs.Empty);
					}
					OnValueChanged(EventArgs.Empty);
					MarkLayoutDirty();
					Update();
				}
			}
		}

		[Description("Represents the maximum value of the range of the control.")]
		[DefaultValue(10)]
		public int Maximum
		{
			get
			{
				return _maximum;
			}
			set
			{
				if (_maximum != value)
				{
					if (value < _minimum)
					{
						_minimum = value;
					}
					_maximum = value;
					if (Value > _maximum)
					{
						Value = _maximum;
					}
					MarkLayoutDirty();
				}
			}
		}

		[Description("Represents the minimum value of the range of the control.")]
		[DefaultValue(0)]
		public int Minimum
		{
			get
			{
				return _minimum;
			}
			set
			{
				if (_minimum != value)
				{
					if (value > _maximum)
					{
						_maximum = value;
					}
					_minimum = value;
					if (Value < _minimum)
					{
						Value = _minimum;
					}
					MarkLayoutDirty();
				}
			}
		}

		[Description("Indicates how to display the tick marks on the track bar. ")]
		[DefaultValue(TickStyle.BottomRight)]
		public TickStyle TickStyle
		{
			get
			{
				return _tickStyle;
			}
			set
			{
				if (_tickStyle != value)
				{
					_tickStyle = value;
					MarkLayoutDirty();
				}
			}
		}

		[Description("The color of the scroll box")]
		public Color ThumbColor
		{
			get
			{
				return _thumbColor;
			}
			set
			{
				if (_thumbColor != value)
				{
					_thumbColor = value;
					Invalidate();
				}
			}
		}

		[Description("Indicates the horizontal or vertical orientation of the track bar.")]
		[DefaultValue(Orientation.Horizontal)]
		public Orientation Orientation
		{
			get
			{
				return _orientation;
			}
			set
			{
				if (_orientation != value)
				{
					_ = base.Height;
					_ = base.Width;
					_orientation = value;
					base.Size = new Size(base.Height, base.Width);
					MarkLayoutDirty();
				}
			}
		}

		[Description("Event fired when the Value property has changed.")]
		public event EventHandler ValueChanged
		{
			add
			{
				base.Events.AddHandler(_valueChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_valueChangedKey, value);
			}
		}

		[Description("Event fired when either a mouse or keyboard action moves the slider.")]
		public event EventHandler Scroll
		{
			add
			{
				EventHandler eventHandler = _scrollHandler;
				EventHandler comparand;
				do
				{
					comparand = eventHandler;
					EventHandler combined = (EventHandler)Delegate.Combine(comparand, value);
					eventHandler = Interlocked.CompareExchange(ref _scrollHandler, combined, comparand);
				}
				while ((object)eventHandler != comparand);
			}
			remove
			{
				EventHandler eventHandler = _scrollHandler;
				EventHandler comparand;
				do
				{
					comparand = eventHandler;
					EventHandler combined = (EventHandler)Delegate.Remove(comparand, value);
					eventHandler = Interlocked.CompareExchange(ref _scrollHandler, combined, comparand);
				}
				while ((object)eventHandler != comparand);
			}
		}

		public MxTrackBar()
		{
			InitializeComponents();
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_container?.Dispose();
				_repeatTimer?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponents()
		{
			_container = new Container();
			_repeatTimer = new System.Windows.Forms.Timer();
			_repeatTimer.Tick += OnRepeatTimerTick;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeThumbColor()
		{
			return _thumbColor != SystemColors.Control;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetThumbColor()
		{
			ThumbColor = SystemColors.Control;
		}

		private bool IsThumbHot()
		{
			return _isThumbHot;
		}

		private void SetThumbHot(bool isHot)
		{
			if (_isThumbHot != isHot)
			{
				_isThumbHot = isHot;
				Invalidate();
			}
		}

		private bool IsThumbPressed()
		{
			return _isThumbPressed;
		}

		private void SetThumbPressed(bool isPressed)
		{
			if (_isThumbPressed != isPressed)
			{
				_isThumbPressed = isPressed;
				Invalidate();
			}
		}

		internal void MarkLayoutDirty()
		{
			_isLayoutDirty = true;
			Invalidate();
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			if (base.Events[_valueChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnScroll(EventArgs e)
		{
			if (_scrollHandler != null)
			{
				_scrollHandler(this, e);
			}
		}

		// The usable length of the track between its end insets; 0 when the control has no room for it.
		private int GetUsableTrackLength()
		{
			int length = ((_orientation == Orientation.Horizontal) ? _trackBounds.Width : _trackBounds.Height) - 2 * TrackEndInset;
			return Math.Max(0, length);
		}

		// With no usable track length, a pointer position cannot select a value, so the current value is kept.
		private int ValueFromPosition(int position)
		{
			int length = GetUsableTrackLength();
			if (length == 0)
			{
				return _value;
			}
			long offset = ((_orientation == Orientation.Horizontal) ? ((long)position - (_trackBounds.X + TrackEndInset)) : ((long)_trackBounds.Bottom - TrackEndInset - position));
			return GetValueFromFraction(_minimum, _maximum, offset / (double)length);
		}

		// Returns the leading edge of the thumb. With no usable track length every value is at the track start.
		private int PositionFromValue(int value)
		{
			int length = GetUsableTrackLength();
			int offset = (int)Math.Round(GetValueFraction(_minimum, _maximum, value) * length);
			if (_orientation == Orientation.Horizontal)
			{
				return _trackBounds.X + offset;
			}
			return _trackBounds.Y + length - offset;
		}

		// Maps a value to [0, 1] across the range; the span is widened so the full Int32 range does not wrap.
		internal static double GetValueFraction(int minimum, int maximum, long value)
		{
			long valueRange = (long)maximum - minimum;
			if (valueRange <= 0)
			{
				return 0.0;
			}
			double fraction = (value - minimum) / (double)valueRange;
			return Math.Clamp(fraction, 0.0, 1.0);
		}

		internal static int GetValueFromFraction(int minimum, int maximum, double fraction)
		{
			long valueRange = (long)maximum - minimum;
			if (valueRange <= 0 || double.IsNaN(fraction))
			{
				return minimum;
			}
			fraction = Math.Clamp(fraction, 0.0, 1.0);
			long value = minimum + (long)Math.Round(fraction * valueRange);
			return (int)Math.Clamp(value, minimum, maximum);
		}

		private void LayoutTrackAndThumb()
		{
			if (_orientation == Orientation.Horizontal)
			{
				int trackTop = TrackCrossOffset;
				if (_tickStyle == TickStyle.TopLeft || _tickStyle == TickStyle.Both)
				{
					trackTop += TickBandSize;
				}
				_trackBounds = new Rectangle(base.ClientRectangle.Left + TrackMargin, trackTop, base.ClientRectangle.Width - 2 * TrackMargin, TrackThickness);
				_thumbBounds = _trackBounds;
				_thumbBounds.Offset(0, -ThumbCrossOffset);
				_thumbBounds.Height = ThumbLength;
				_thumbBounds.Width = ThumbWidth;
				_thumbBounds.X = PositionFromValue(Value);
			}
			else
			{
				int trackLeft = TrackCrossOffset;
				if (_tickStyle == TickStyle.TopLeft || _tickStyle == TickStyle.Both)
				{
					trackLeft += TickBandSize;
				}
				_trackBounds = new Rectangle(trackLeft, base.ClientRectangle.Top + TrackMargin, TrackThickness, base.ClientRectangle.Height - 2 * TrackMargin);
				_thumbBounds = _trackBounds;
				_thumbBounds.Offset(-ThumbCrossOffset, 0);
				_thumbBounds.Height = ThumbWidth;
				_thumbBounds.Width = ThumbLength;
				_thumbBounds.Y = PositionFromValue(Value);
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			RenderingHelper.SetMouseCapture(this, false);
			_isDraggingThumb = false;
			_repeatTimer.Stop();
			SetThumbPressed(false);
			Invalidate();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			MarkLayoutDirty();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_isLayoutDirty)
			{
				LayoutTrackAndThumb();
				_isLayoutDirty = false;
			}
			if (IsThemed)
			{
				if (base.Width > 0 && base.Height > 0)
				{
					PaintThemed(e.Graphics);
				}
				base.OnPaint(e);
				return;
			}
			if (VisualStyleSupport.IsThemingUsable(false))
			{
				_themeRenderer = new VisualStylePainter("ProgessBar");
			}
			if (base.Width <= 0 || base.Height <= 0)
			{
				base.OnPaint(e);
				return;
			}
			Bitmap bitmap = new Bitmap(base.Width, base.Height);
			Graphics graphics = Graphics.FromImage(bitmap);
			if (_themeRenderer != null)
			{
				_themeRenderer.DrawTrackBar(graphics, base.ClientRectangle, _trackBounds, _thumbBounds, _orientation, _tickStyle, base.Enabled, IsThumbHot(), IsThumbPressed(), Focused);
			}
			else
			{
				DrawTrackBackground(graphics);
				if (_orientation == Orientation.Horizontal)
				{
					DrawHorizontalThumb(graphics, _thumbBounds);
				}
				else
				{
					DrawVerticalThumb(graphics, _thumbBounds);
				}
			}
			if (base.ContainsFocus && ShowFocusRectangle)
			{
				Pen pen = new Pen(SystemColors.WindowFrame);
				pen.DashStyle = DashStyle.Dot;
				RenderingHelper.DrawRectangleOutline(graphics, pen, base.ClientRectangle);
				pen.Dispose();
				pen = null;
			}
			if (_orientation == Orientation.Vertical)
			{
				DrawVerticalTicks(graphics);
			}
			else
			{
				DrawHorizontalTicks(graphics);
			}
			if (RightToLeft == RightToLeft.Yes)
			{
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
			}
			e.Graphics.DrawImage(bitmap, base.ClientRectangle);
			graphics.Dispose();
			graphics = null;
			bitmap.Dispose();
			bitmap = null;
			if (_themeRenderer != null)
			{
				_themeRenderer.DisposeResources();
				_themeRenderer = null;
			}
			base.OnPaint(e);
		}

		private void DrawHorizontalThumb(Graphics graphics, Rectangle rect)
		{
			Brush brush = GdiCache.Brush(ThumbColor);
			if (_tickStyle == TickStyle.Both)
			{
				rect = RenderingHelper.DrawColoredBorder(graphics, ThumbColor, rect, Mordex.Windows.UI.BorderStyle.Raised);
				graphics.FillRectangle(brush, rect);
				return;
			}
			Pen lightPen = GdiCache.Pen(ControlPaint.Light(ThumbColor, 0f));
			Pen darkPen = GdiCache.Pen(ControlPaint.Dark(ThumbColor, 0f));
			Pen darkestPen = GdiCache.Pen(ControlPaint.DarkDark(ThumbColor));
			Pen lightestPen = GdiCache.Pen(ControlPaint.LightLight(ThumbColor));
			if (_tickStyle == TickStyle.BottomRight || _tickStyle == TickStyle.None)
			{
				using GraphicsPath bottomRightPath = new GraphicsPath();
				bottomRightPath.AddLine(rect.Left, rect.Top, rect.Right - 1, rect.Top);
				bottomRightPath.AddLine(rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 6);
				bottomRightPath.AddLine(rect.Right - 1, rect.Bottom - 6, rect.Right - 6, rect.Bottom - 1);
				bottomRightPath.AddLine(rect.Right - 6, rect.Bottom - 1, rect.Left, rect.Bottom - 6);
				bottomRightPath.AddLine(rect.Left, rect.Bottom - 6, rect.Left, rect.Top);
				graphics.FillPath(brush, bottomRightPath);
				graphics.DrawLine(lightestPen, rect.Left, rect.Top, rect.Right - 1, rect.Top);
				graphics.DrawLine(lightestPen, rect.Left, rect.Top, rect.Left, rect.Bottom - 6);
				graphics.DrawLine(lightPen, rect.Left + 1, rect.Top + 1, rect.Right - 2, rect.Top + 1);
				graphics.DrawLine(lightPen, rect.Left + 1, rect.Top + 1, rect.Left + 1, rect.Bottom - 6);
				graphics.DrawLine(darkestPen, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 6);
				graphics.DrawLine(darkPen, rect.Right - 2, rect.Top + 1, rect.Right - 2, rect.Bottom - 6);
				graphics.DrawLine(lightestPen, rect.Left, rect.Bottom - 6, rect.Left + 5, rect.Bottom - 1);
				graphics.DrawLine(lightPen, rect.Left + 2, rect.Bottom - 5, rect.Left + 5, rect.Bottom - 2);
				graphics.DrawLine(darkestPen, rect.Right - 1, rect.Bottom - 6, rect.Right - 6, rect.Bottom - 1);
				graphics.DrawLine(darkPen, rect.Right - 3, rect.Bottom - 5, rect.Right - 6, rect.Bottom - 2);
			}
			else
			{
				using GraphicsPath topLeftPath = new GraphicsPath();
				topLeftPath.AddLine(rect.Left, rect.Bottom - 1, rect.Left, rect.Top + 5);
				topLeftPath.AddLine(rect.Left, rect.Top + 5, rect.Left + 5, rect.Top);
				topLeftPath.AddLine(rect.Left + 5, rect.Top, rect.Right - 1, rect.Top + 5);
				topLeftPath.AddLine(rect.Right - 1, rect.Top + 5, rect.Right - 1, rect.Bottom - 1);
				topLeftPath.AddLine(rect.Right - 1, rect.Bottom - 1, rect.Left, rect.Bottom - 1);
				graphics.FillPath(brush, topLeftPath);
				graphics.DrawLine(lightestPen, rect.Left, rect.Bottom - 1, rect.Left, rect.Top + 5);
				graphics.DrawLine(lightestPen, rect.Left, rect.Top + 5, rect.Left + 5, rect.Top);
				graphics.DrawLine(lightPen, rect.Left + 1, rect.Bottom - 1, rect.Left + 1, rect.Top + 5);
				graphics.DrawLine(lightPen, rect.Left + 1, rect.Top + 5, rect.Left + 5, rect.Top + 1);
				graphics.DrawLine(darkestPen, rect.Left + 5, rect.Top, rect.Right - 1, rect.Top + 5);
				graphics.DrawLine(darkestPen, rect.Right - 1, rect.Top + 5, rect.Right - 1, rect.Bottom - 1);
				graphics.DrawLine(darkestPen, rect.Right - 1, rect.Bottom - 1, rect.Left, rect.Bottom - 1);
				graphics.DrawLine(darkPen, rect.Left + 5, rect.Top + 1, rect.Right - 2, rect.Top + 5);
				graphics.DrawLine(darkPen, rect.Right - 2, rect.Top + 5, rect.Right - 2, rect.Bottom - 2);
				graphics.DrawLine(darkPen, rect.Right - 2, rect.Bottom - 2, rect.Left + 1, rect.Bottom - 2);
			}
			brush = null;
			lightPen = null;
			darkPen = null;
			darkestPen = null;
			lightestPen = null;
		}

		private void DrawVerticalThumb(Graphics graphics, Rectangle rect)
		{
			Brush brush = GdiCache.Brush(ThumbColor);
			if (_tickStyle == TickStyle.Both)
			{
				rect = RenderingHelper.DrawColoredBorder(graphics, ThumbColor, rect, Mordex.Windows.UI.BorderStyle.Raised);
				graphics.FillRectangle(brush, rect);
				return;
			}
			Pen lightPen = GdiCache.Pen(ControlPaint.Light(ThumbColor, 0f));
			Pen darkPen = GdiCache.Pen(ControlPaint.Dark(ThumbColor, 0f));
			Pen darkestPen = GdiCache.Pen(ControlPaint.DarkDark(ThumbColor));
			Pen lightestPen = GdiCache.Pen(ControlPaint.LightLight(ThumbColor));
			if (_tickStyle == TickStyle.BottomRight || _tickStyle == TickStyle.None)
			{
				using GraphicsPath bottomRightPath = new GraphicsPath();
				bottomRightPath.AddLine(rect.Left, rect.Bottom - 1, rect.Left, rect.Top);
				bottomRightPath.AddLine(rect.Left, rect.Top, rect.Right - 6, rect.Top);
				bottomRightPath.AddLine(rect.Right - 6, rect.Top, rect.Right - 1, rect.Top + 5);
				bottomRightPath.AddLine(rect.Right - 1, rect.Top + 5, rect.Right - 6, rect.Bottom - 1);
				bottomRightPath.AddLine(rect.Right - 6, rect.Bottom - 1, rect.Left, rect.Bottom - 1);
				graphics.FillPath(brush, bottomRightPath);
				graphics.DrawLine(lightestPen, rect.Left, rect.Bottom - 1, rect.Left, rect.Top);
				graphics.DrawLine(lightestPen, rect.Left, rect.Top, rect.Right - 6, rect.Top);
				graphics.DrawLine(lightestPen, rect.Right - 6, rect.Top, rect.Right - 1, rect.Top + 5);
				graphics.DrawLine(lightPen, rect.Left + 1, rect.Bottom - 1, rect.Left + 1, rect.Top + 1);
				graphics.DrawLine(lightPen, rect.Left + 1, rect.Top + 1, rect.Right - 6, rect.Top + 1);
				graphics.DrawLine(lightPen, rect.Right - 6, rect.Top + 1, rect.Right - 2, rect.Top + 5);
				graphics.DrawLine(darkestPen, rect.Right - 1, rect.Top + 5, rect.Right - 6, rect.Bottom - 1);
				graphics.DrawLine(darkestPen, rect.Right - 6, rect.Bottom - 1, rect.Left, rect.Bottom - 1);
				graphics.DrawLine(darkPen, rect.Right - 2, rect.Top + 5, rect.Right - 6, rect.Bottom - 2);
				graphics.DrawLine(darkPen, rect.Right - 6, rect.Bottom - 2, rect.Left + 1, rect.Bottom - 2);
			}
			else
			{
				using GraphicsPath topLeftPath = new GraphicsPath();
				topLeftPath.AddLine(rect.Left, rect.Top + 5, rect.Left + 5, rect.Top);
				topLeftPath.AddLine(rect.Left + 5, rect.Top, rect.Right - 1, rect.Top);
				topLeftPath.AddLine(rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
				topLeftPath.AddLine(rect.Right - 1, rect.Bottom - 1, rect.Left + 5, rect.Bottom - 1);
				topLeftPath.AddLine(rect.Left + 5, rect.Bottom - 1, rect.Left, rect.Top + 5);
				graphics.FillPath(brush, topLeftPath);
				graphics.DrawLine(lightestPen, rect.Left, rect.Top + 5, rect.Left + 5, rect.Top);
				graphics.DrawLine(lightestPen, rect.Left + 5, rect.Top, rect.Right - 1, rect.Top);
				graphics.DrawLine(lightPen, rect.Left + 1, rect.Top + 5, rect.Left + 5, rect.Top + 1);
				graphics.DrawLine(lightPen, rect.Left + 5, rect.Top + 1, rect.Right - 1, rect.Top + 1);
				graphics.DrawLine(darkestPen, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
				graphics.DrawLine(darkestPen, rect.Right - 1, rect.Bottom - 1, rect.Left + 5, rect.Bottom - 1);
				graphics.DrawLine(darkestPen, rect.Left + 5, rect.Bottom - 1, rect.Left, rect.Top + 5);
				graphics.DrawLine(darkPen, rect.Right - 2, rect.Top + 1, rect.Right - 2, rect.Bottom - 2);
				graphics.DrawLine(darkPen, rect.Right - 2, rect.Bottom - 2, rect.Left + 5, rect.Bottom - 2);
				graphics.DrawLine(darkPen, rect.Left + 5, rect.Bottom - 2, rect.Left + 1, rect.Top + 5);
			}
			brush = null;
			lightPen = null;
			darkPen = null;
			darkestPen = null;
			lightestPen = null;
		}

		private void DrawVerticalTicks(Graphics graphics)
		{
			Pen pen = ((_themeRenderer == null) ? GdiCache.Pen(ForeColor) : GdiCache.Pen(SystemColors.ControlDark));
			float trackStart = _trackBounds.Y + TrackEndInset;
			float trackLength = _trackBounds.Height - 2 * TrackEndInset;
			if (_tickStyle == TickStyle.Both || _tickStyle == TickStyle.TopLeft)
			{
				DrawTickRow(graphics, pen, base.ClientRectangle.X + TickNearOffset, trackStart, trackLength);
			}
			if (_tickStyle == TickStyle.Both || _tickStyle == TickStyle.BottomRight)
			{
				DrawTickRow(graphics, pen, _trackBounds.Right + TickFarOffset, trackStart, trackLength);
			}
			pen = null;
		}

		private void DrawTickRow(Graphics graphics, Pen pen, float tickStart, float trackStart, float trackLength)
		{
			long step = GetTickStep(_minimum, _maximum, trackLength);
			if (step == 0)
			{
				return;
			}
			for (long i = _minimum; i <= _maximum; i += step)
			{
				float tickLength = ((i != _minimum && i != _maximum) ? TickLength : EndTickLength);
				float tickPosition = GetTickPosition(_minimum, _maximum, i, trackStart, trackLength);
				if (_orientation == Orientation.Vertical)
				{
					graphics.DrawLine(pen, tickStart, tickPosition, tickStart + tickLength, tickPosition);
				}
				else
				{
					graphics.DrawLine(pen, tickPosition, tickStart, tickPosition, tickStart + tickLength);
				}
			}
		}

		// Returns 0 when the track has no room for ticks.
		internal static long GetTickStep(int minimum, int maximum, float trackLength)
		{
			if (maximum < minimum || !(trackLength > 0f) || float.IsPositiveInfinity(trackLength))
			{
				return 0;
			}
			long valueRange = (long)maximum - minimum;
			if (valueRange == 0)
			{
				return 1;
			}
			// At most about one tick per pixel, so a very large range cannot stall painting.
			double step = Math.Ceiling(valueRange / (double)trackLength);
			return (step <= 1.0) ? 1 : (long)Math.Min(step, valueRange);
		}

		internal static float GetTickPosition(int minimum, int maximum, long value, float trackStart, float trackLength)
		{
			return trackStart + (float)(GetValueFraction(minimum, maximum, value) * trackLength);
		}

		private void DrawTrackBackground(Graphics graphics)
		{
			Brush brush = GdiCache.Brush(BackColor);
			graphics.FillRectangle(brush, base.ClientRectangle);
			Color color = ThumbColor;
			if (color.IsEmpty)
			{
				color = BackColor;
			}
			brush = GdiCache.Brush(color);
			graphics.FillRectangle(brush, _trackBounds);
			brush = null;
			RenderingHelper.DrawColoredBorder(graphics, ThumbColor, _trackBounds, Mordex.Windows.UI.BorderStyle.Sunken);
		}

		private void DrawHorizontalTicks(Graphics graphics)
		{
			Pen pen = ((_themeRenderer == null) ? GdiCache.Pen(ForeColor) : GdiCache.Pen(SystemColors.ControlDark));
			float trackStart = _trackBounds.X + TrackEndInset;
			float trackLength = _trackBounds.Width - 2 * TrackEndInset;
			if (_tickStyle == TickStyle.Both || _tickStyle == TickStyle.TopLeft)
			{
				DrawTickRow(graphics, pen, base.ClientRectangle.Y + TickNearOffset, trackStart, trackLength);
			}
			if (_tickStyle == TickStyle.Both || _tickStyle == TickStyle.BottomRight)
			{
				DrawTickRow(graphics, pen, _trackBounds.Bottom + TickFarOffset, trackStart, trackLength);
			}
			pen = null;
		}

		private void SetValueFromMouse(int x, int y)
		{
			int value = Value;
			value = ((_orientation != Orientation.Vertical) ? ValueFromPosition(x) : ValueFromPosition(y));
			SetValueByUser(value);
		}

		private void SetValueByUser(int value)
		{
			_isUserChange = true;
			try
			{
				Value = value;
			}
			finally
			{
				_isUserChange = false;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (_isDraggingThumb)
			{
				SetValueFromMouse(e.X, e.Y);
			}
			else if (_thumbBounds.Contains(e.X, e.Y))
			{
				SetThumbHot(true);
			}
			else
			{
				SetThumbHot(false);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			RenderingHelper.TryFocusControl(this);
			if (_thumbBounds.Contains(e.X, e.Y))
			{
				SetThumbPressed(true);
				_valueBeforeDrag = Value;
				_isDraggingThumb = true;
				return;
			}
			SetThumbPressed(false);
			Rectangle rectangle = _trackBounds;
			rectangle.Inflate(TrackHitInflate, TrackHitInflate);
			if (!rectangle.Contains(e.X, e.Y) || GetUsableTrackLength() == 0)
			{
				return;
			}
			if (_orientation == Orientation.Horizontal)
			{
				if (e.X > PositionFromValue(Value))
				{
					_isPagingTowardMax = true;
				}
				else
				{
					_isPagingTowardMax = false;
				}
			}
			else if (e.Y < PositionFromValue(Value))
			{
				_isPagingTowardMax = true;
			}
			else
			{
				_isPagingTowardMax = false;
			}
			StepTowardCursor();
			_repeatTimer.Interval = RepeatInitialDelayMs;
			_repeatTimer.Enabled = true;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetThumbHot(false);
			if (_isDraggingThumb)
			{
				_isDraggingThumb = false;
			}
			_repeatTimer.Stop();
			_repeatTimer.Enabled = false;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (_isDraggingThumb)
			{
				_isDraggingThumb = false;
			}
			_repeatTimer.Stop();
			_repeatTimer.Enabled = false;
			SetThumbPressed(false);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Prior:
			case Keys.Next:
			case Keys.Left:
			case Keys.Up:
			case Keys.Right:
			case Keys.Down:
				return true;
			default:
				return base.IsInputKey(keyData);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled)
			{
				return;
			}
			int value = Value;
			switch (e.KeyData)
			{
			case Keys.Escape:
				if (_isDraggingThumb)
				{
					Value = _valueBeforeDrag;
					_isDraggingThumb = false;
				}
				break;
			case Keys.Left:
			case Keys.Up:
				if (_orientation == Orientation.Vertical)
				{
					value += SmallChange;
					if (value > Maximum)
					{
						value = Maximum;
					}
				}
				else
				{
					value -= SmallChange;
					if (value < Minimum)
					{
						value = Minimum;
					}
				}
				SetValueByUser(value);
				e.Handled = true;
				break;
			case Keys.Right:
			case Keys.Down:
				if (_orientation == Orientation.Vertical)
				{
					value -= SmallChange;
					if (value < Minimum)
					{
						value = Minimum;
					}
				}
				else
				{
					value += SmallChange;
					if (value > Maximum)
					{
						value = Maximum;
					}
				}
				SetValueByUser(value);
				e.Handled = true;
				break;
			case Keys.Next:
				if (_orientation == Orientation.Vertical)
				{
					value -= LargeChange;
					if (value < Minimum)
					{
						value = Minimum;
					}
				}
				else
				{
					value += LargeChange;
					if (value > Maximum)
					{
						value = Maximum;
					}
				}
				SetValueByUser(value);
				e.Handled = true;
				break;
			case Keys.Prior:
				if (_orientation == Orientation.Vertical)
				{
					value += LargeChange;
					if (value > Maximum)
					{
						value = Maximum;
					}
				}
				else
				{
					value -= LargeChange;
					if (value < Minimum)
					{
						value = Minimum;
					}
				}
				SetValueByUser(value);
				e.Handled = true;
				break;
			}
		}

		private void OnRepeatTimerTick(object sender, EventArgs eventArgs)
		{
			if (base.IsDisposed || !base.IsHandleCreated)
			{
				_repeatTimer.Stop();
				return;
			}
			StepTowardCursor();
			if (_repeatTimer.Interval != RepeatIntervalMs)
			{
				_repeatTimer.Interval = RepeatIntervalMs;
			}
		}

		private void StepTowardCursor()
		{
			if (GetUsableTrackLength() == 0)
			{
				return;
			}
			int value = Value;
			Point point = PointToClient(Cursor.Position);
			if (_isPagingTowardMax)
			{
				if (_orientation == Orientation.Horizontal)
				{
					if (PositionFromValue(Value) >= point.X)
					{
						return;
					}
				}
				else if (PositionFromValue(Value) <= point.Y)
				{
					return;
				}
				value += LargeChange;
				if (value > Maximum)
				{
					value = Maximum;
				}
			}
			else
			{
				if (_orientation == Orientation.Horizontal)
				{
					if (PositionFromValue(Value) <= point.X)
					{
						return;
					}
				}
				else if (PositionFromValue(Value) >= point.Y)
				{
					return;
				}
				value -= LargeChange;
				if (value < Minimum)
				{
					value = Minimum;
				}
			}
			SetValueByUser(value);
		}
	}
}
