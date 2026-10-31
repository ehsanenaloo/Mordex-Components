using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	// The pop-up window that shows one super tip. It is a layered window with a soft shadow when the system supports it.
	internal sealed class SuperTipWindow : NativeWindow, IMessageFilter, IDisposable
	{
		private const int InitialAlpha = 20;

		private const int FadeAlphaStep = 80;

		private const int FadeIntervalMs = 70;

		private const int OpaqueAlpha = 255;

		private const int FilterRemovalDelayMs = 10;

		private const int ContentMargin = 3;

		private const int ShadowSize = 4;

		private const int CornerSize = 5;

		private const int HeaderPadding = 6;

		private const int CursorOffsetY = 16;

		private const int MinColorDepth = 16;

		private const int SchemeGradientEndColor = 186;

		private const int SchemeSeparatorColor = 187;

		private const int SchemeBorderColor = 188;

		private readonly MxSuperTip _superTip;

		private readonly ResolvedTheme _theme;

		private readonly SuperTipSettings _settings;

		private readonly Font _bodyFont;

		private readonly Font _headerFont;

		private readonly Font _footerFont;

		private readonly bool _rightToLeft;

		private readonly Color _gradientEndColor;

		private readonly Color _borderColor;

		private readonly Color _separatorColor;

		private Bitmap _shadowTopRight;

		private Bitmap _shadowBottomLeft;

		private Bitmap _shadowBottomRight;

		private Bitmap _backBuffer;

		private Rectangle _bounds = Rectangle.Empty;

		private Rectangle _headerTextBounds;

		private Rectangle _bodyTextBounds;

		private Rectangle _footerTextBounds;

		private Rectangle _headerImageBounds;

		private Rectangle _bodyImageBounds;

		private Rectangle _footerImageBounds;

		private Rectangle _separatorBounds;

		private bool _hasShadow;

		private int _alpha = InitialAlpha;

		private bool _fadeInAllowed = true;

		private Point _position = Point.Empty;

		private Control _anchorControl;

		private int _initialDelay;

		private int _autoPopDelay;

		private bool _showAlways;

		private bool _scheduled;

		private bool _autoPopArmed;

		private bool _windowVisible;

		private bool _disposed;

		private bool _filterAdded;

		private Timer _showTimer;

		private Timer _fadeTimer;

		private Timer _filterRemovalTimer;

		internal SuperTipWindow(MxSuperTip superTip, SuperTipSettings settings)
		{
			_superTip = superTip;
			_settings = settings;
			_autoPopDelay = superTip.AutoPopDelay;
			_initialDelay = superTip.InitialDelay;
			_showAlways = superTip.ShowAlways;
			_rightToLeft = superTip.RightToLeft;
			_bodyFont = superTip.CreateBodyFont();
			_headerFont = superTip.CreateHeaderFont();
			_footerFont = superTip.CreateFooterFont();
			_showTimer = new Timer();
			_showTimer.Tick += OnShowTimerTick;
			_fadeTimer = new Timer { Interval = FadeIntervalMs };
			_fadeTimer.Tick += OnFadeTimerTick;
			if (superTip.UseDefaultTheme)
			{
				_theme = MxThemeManager.ResolveDefault(96);
				_gradientEndColor = _theme.Color(ThemeRole.TooltipBack);
				_borderColor = _theme.Color(ThemeRole.Border);
				_separatorColor = _theme.Color(ThemeRole.Border);
				return;
			}
			if (settings.ToolTipStyle == ToolTipStyle.Standard)
			{
				_borderColor = SystemColors.WindowFrame;
				_separatorColor = SystemColors.ControlDark;
				return;
			}
			uint[] palette = OfficePalettes.Office2007Blue;
			_gradientEndColor = Color.FromArgb(unchecked((int)palette[SchemeGradientEndColor]));
			_borderColor = Color.FromArgb(unchecked((int)palette[SchemeBorderColor]));
			_separatorColor = Color.FromArgb(unchecked((int)palette[SchemeSeparatorColor]));
		}

		internal bool IsScheduled => _scheduled;

		internal int InitialDelay
		{
			set
			{
				_initialDelay = value;
			}
		}

		internal int AutoPopDelay
		{
			set
			{
				_autoPopDelay = value;
			}
		}

		internal bool ShowAlways
		{
			set
			{
				_showAlways = value;
			}
		}

		// Shows the tip after the initial delay, near the control or at the given point.
		internal void Schedule(Control control, Point position)
		{
			_position = position;
			if (_scheduled)
			{
				ToggleVisibility();
				return;
			}
			_scheduled = true;
			_showTimer.Stop();
			_anchorControl = control;
			StartShowTimer(_initialDelay > 0 && control != null);
		}

		internal void SchedulePoint(Point position)
		{
			_position = position;
			if (_scheduled)
			{
				ToggleVisibility();
				return;
			}
			_scheduled = true;
			_showTimer.Stop();
			StartShowTimer(_initialDelay > 0);
		}

		private void StartShowTimer(bool useDelay)
		{
			if (useDelay)
			{
				_showTimer.Interval = _initialDelay;
				_showTimer.Start();
			}
			else
			{
				ToggleVisibility();
			}
		}

		private void OnShowTimerTick(object sender, EventArgs e)
		{
			_showTimer.Stop();
			ToggleVisibility();
		}

		private void ToggleVisibility()
		{
			if (_disposed)
			{
				return;
			}
			if (_autoPopArmed)
			{
				Hide();
				return;
			}
			ShowWindow();
			if (!_showAlways)
			{
				_showTimer.Interval = _autoPopDelay;
				_autoPopArmed = true;
				_showTimer.Start();
			}
		}

		private void ShowWindow()
		{
			DisposeFilterRemovalTimer();
			if (!_filterAdded)
			{
				Application.AddMessageFilter(this);
				_filterAdded = true;
			}
			CreateAndShow();
			NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IdcArrow));
			_windowVisible = true;
		}

		private static bool SupportsLayeredWindows()
		{
			if (!OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
			{
				return false;
			}
			IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
			try
			{
				return NativeMethods.GetDeviceCaps(screenDc, NativeMethods.BitsPerPixelCaps) >= MinColorDepth;
			}
			finally
			{
				NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			}
		}

		private void CreateAndShow()
		{
			bool layered = SupportsLayeredWindows();
			_hasShadow = layered;
			_bounds.Size = MeasureTip(_hasShadow);
			PositionInWorkingArea(_position.IsEmpty ? Cursor.Position : _position);
			CreateParams createParams = new CreateParams
			{
				X = _bounds.X,
				Y = _bounds.Y,
				Width = _bounds.Width,
				Height = _bounds.Height,
				Style = NativeMethods.WsPopup,
				ExStyle = NativeMethods.WsExTopMost | NativeMethods.WsExToolWindow,
				Parent = IntPtr.Zero
			};
			if (layered)
			{
				createParams.ExStyle |= NativeMethods.WsExLayered;
			}
			CreateHandle(createParams);
			NativeMethods.ShowWindow(Handle, NativeMethods.SwShowNoActivate);
			if (_fadeInAllowed && layered)
			{
				if (_showAlways)
				{
					_alpha = OpaqueAlpha;
					PaintLayered();
				}
				else
				{
					_fadeTimer.Start();
				}
			}
		}

		private void PositionInWorkingArea(Point point)
		{
			int x = point.X;
			int y = point.Y;
			if (_position.IsEmpty)
			{
				y += CursorOffsetY;
			}
			Rectangle workingArea = Screen.GetWorkingArea(point);
			if (x + _bounds.Width > workingArea.Right)
			{
				x = workingArea.Right - _bounds.Width;
			}
			if (x < workingArea.X)
			{
				x = workingArea.X;
			}
			if (y + _bounds.Height > workingArea.Bottom)
			{
				y -= _bounds.Height;
			}
			_bounds.X = x;
			_bounds.Y = y;
		}

		private void OnFadeTimerTick(object sender, EventArgs e)
		{
			_alpha = Math.Min(_alpha + FadeAlphaStep, OpaqueAlpha);
			PaintLayered();
			if (_alpha >= OpaqueAlpha)
			{
				_fadeTimer.Stop();
			}
		}

		// Hides the window but keeps the object usable; the message filter is removed a moment later.
		internal void Hide()
		{
			if (_disposed)
			{
				return;
			}
			_scheduled = false;
			_showTimer.Stop();
			_alpha = InitialAlpha;
			_autoPopArmed = false;
			_fadeTimer.Stop();
			if (_windowVisible)
			{
				NativeMethods.ShowWindow(Handle, NativeMethods.SwHide);
				DestroyHandle();
				_filterRemovalTimer?.Dispose();
				_filterRemovalTimer = new Timer { Interval = FilterRemovalDelayMs };
				_filterRemovalTimer.Tick += OnFilterRemovalTimerTick;
				_filterRemovalTimer.Start();
				_windowVisible = false;
			}
		}

		private void OnFilterRemovalTimerTick(object sender, EventArgs e)
		{
			RemoveFilter();
			DisposeFilterRemovalTimer();
		}

		private void RemoveFilter()
		{
			if (_filterAdded)
			{
				Application.RemoveMessageFilter(this);
				_filterAdded = false;
			}
		}

		private void DisposeFilterRemovalTimer()
		{
			if (_filterRemovalTimer != null)
			{
				_filterRemovalTimer.Stop();
				_filterRemovalTimer.Dispose();
				_filterRemovalTimer = null;
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			if (_windowVisible)
			{
				NativeMethods.ShowWindow(Handle, NativeMethods.SwHide);
				DestroyHandle();
				_windowVisible = false;
			}
			RemoveFilter();
			_disposed = true;
			DisposeFilterRemovalTimer();
			_showTimer.Stop();
			_showTimer.Dispose();
			_fadeTimer.Stop();
			_fadeTimer.Dispose();
			_bodyFont.Dispose();
			_headerFont.Dispose();
			_footerFont.Dispose();
			_shadowTopRight?.Dispose();
			_shadowBottomLeft?.Dispose();
			_shadowBottomRight?.Dispose();
			_backBuffer?.Dispose();
		}

		private TextLayoutRenderer CreateRenderer()
		{
			TextLayoutRenderer renderer = new TextLayoutRenderer(useTextRenderer: false);
			if (_rightToLeft)
			{
				renderer.RightToLeft = RightToLeft.Yes;
			}
			renderer.HorizontalAlignment = StringAlignment.Near;
			renderer.VerticalAlignment = StringAlignment.Center;
			return renderer;
		}

		// Measures the tip with header, body and footer, and lays out their rectangles.
		private Size MeasureTip(bool withShadow)
		{
			if (_settings.HasNoHeaderOrFooter())
			{
				return MeasureBodyOnlyTip(withShadow);
			}
			using Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
			using TextLayoutRenderer renderer = CreateRenderer();
			int footerHeight = 0;
			int headerHeight = 0;
			int bodyHeight = 0;
			int width = 0;
			int y = withShadow ? 2 : 1;
			int margin = ContentMargin;
			int x = margin;
			int headerTextWidth = 0;
			int headerExtra = HeaderPadding;
			Image bodyImage = _settings.GetImage(_superTip);
			Image headerImage = _settings.GetHeaderImage(_superTip);
			Image footerImage = _settings.GetFooterImage(_superTip);
			if (bodyImage != null || _settings.Text.Length > 0)
			{
				headerExtra += 2;
			}
			if (_settings.HeaderText.Length > 0 || headerImage != null)
			{
				SizeF headerSize = renderer.Measure(graphics, _settings.HeaderText, _headerFont, 0, 0);
				headerHeight = (int)headerSize.Height + headerExtra;
				if (headerImage != null)
				{
					Size imageSize = headerImage.Size;
					headerSize.Width += imageSize.Width + 9;
					if (headerHeight < imageSize.Height + 6)
					{
						headerHeight = imageSize.Height + 6;
					}
					width = (int)headerSize.Width + 8;
					if (_rightToLeft)
					{
						_headerImageBounds = new Rectangle(width - 4 - imageSize.Width, y + (headerHeight - imageSize.Height) / 2, imageSize.Width, imageSize.Height);
						headerTextWidth = _headerImageBounds.X;
					}
					else
					{
						_headerImageBounds = new Rectangle(margin + 4, y + (headerHeight - imageSize.Height) / 2, imageSize.Width, imageSize.Height);
						x += imageSize.Width + 9;
						headerTextWidth = width - x;
					}
				}
				else
				{
					width = (int)headerSize.Width + 8;
					headerTextWidth = width - x;
				}
				_headerTextBounds = new Rectangle(x, y, headerTextWidth, headerHeight);
				y += headerHeight;
				x = margin;
			}
			if (_settings.FooterText.Length > 0 || footerImage != null)
			{
				int footerWidth = (int)renderer.Measure(graphics, _settings.FooterText, _footerFont, 0, 0).Width + 8;
				if (footerImage != null)
				{
					footerWidth += footerImage.Width + 9;
				}
				if (footerWidth > width)
				{
					width = footerWidth;
				}
			}
			int bodyWidth = width * 3;
			if (_superTip.BodyWidth > 0 && !_settings.HasNoHeaderOrFooter())
			{
				bodyWidth = _superTip.BodyWidth;
			}
			if (bodyImage != null && _settings.Text.Length > 0)
			{
				bodyWidth += bodyImage.Width + 12;
			}
			if (width > bodyWidth)
			{
				bodyWidth = width;
			}
			if (width < bodyWidth && (_settings.Text.Length > 0 || bodyImage != null))
			{
				width = bodyWidth;
			}
			if (_settings.Text.Length > 0 || bodyImage != null)
			{
				int textWidth = width - 18;
				x = margin + 9;
				Size imageSize = Size.Empty;
				if (bodyImage != null)
				{
					imageSize = bodyImage.Size;
					bodyHeight = imageSize.Height + 12;
					textWidth -= imageSize.Width + 11;
				}
				renderer.WordWrap = true;
				SizeF textSize;
				if (width <= 0)
				{
					textSize = renderer.Measure(graphics, _settings.Text, _bodyFont, 0, 0);
					textWidth = (int)textSize.Width;
					width = textWidth + 18;
				}
				else
				{
					textSize = renderer.Measure(graphics, _settings.Text, _bodyFont, textWidth, 0);
				}
				if (bodyHeight < (int)textSize.Height + 10)
				{
					bodyHeight = (int)textSize.Height + 10;
				}
				if (bodyImage != null)
				{
					_bodyImageBounds = new Rectangle(margin + 11, y + 4, imageSize.Width, imageSize.Height);
					x += imageSize.Width + 12;
				}
				_bodyTextBounds = new Rectangle(x, y + 4, textWidth, bodyHeight);
				y += bodyHeight;
			}
			x = margin;
			if (_settings.FooterText.Length > 0 || footerImage != null)
			{
				_separatorBounds = new Rectangle(margin, y - 2, width, 2);
				footerHeight = (int)renderer.Measure(graphics, _settings.FooterText, _footerFont, 0, 0).Height + 15;
				if (footerImage != null)
				{
					Size imageSize = footerImage.Size;
					if (footerHeight < imageSize.Height + 13)
					{
						footerHeight = imageSize.Height + 13;
					}
					_footerImageBounds = new Rectangle(margin + 4, y + (footerHeight - imageSize.Height) / 2, imageSize.Width, imageSize.Height);
					x += imageSize.Width + 9;
				}
				_footerTextBounds = new Rectangle(x, y, width - x, footerHeight);
			}
			Size result = new Size(width, headerHeight + bodyHeight + footerHeight);
			if (_rightToLeft)
			{
				MirrorLayout(width, margin, headerImage, footerImage, bodyImage);
			}
			if (withShadow)
			{
				result.Width += ShadowSize;
				result.Height += 6;
			}
			return result;
		}

		private void MirrorLayout(int width, int margin, Image headerImage, Image footerImage, Image bodyImage)
		{
			if (headerImage != null)
			{
				_headerImageBounds.X = width - 4 - headerImage.Size.Width;
				_headerTextBounds.X = margin;
				_headerTextBounds.Width = _headerImageBounds.X - margin - 4;
			}
			else
			{
				_headerTextBounds.X = margin;
				_headerTextBounds.Width = width - margin;
			}
			if (footerImage != null)
			{
				_footerImageBounds.X = width - 4 - footerImage.Size.Width;
				_footerTextBounds.X = margin;
				_footerTextBounds.Width = _footerImageBounds.X - margin - 4;
			}
			else
			{
				_footerTextBounds.X = margin;
				_footerTextBounds.Width = width - margin;
			}
			if (bodyImage != null)
			{
				_bodyImageBounds.X = width - 11 - bodyImage.Size.Width;
				_bodyTextBounds.X = margin + 6;
				_bodyTextBounds.Width = _bodyImageBounds.X - 4;
			}
			else
			{
				_bodyTextBounds.X = margin + 6;
				_bodyTextBounds.Width = width - 18;
			}
		}

		// Measures a tip that has only body text and an optional image.
		private Size MeasureBodyOnlyTip(bool withShadow)
		{
			using Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
			using TextLayoutRenderer renderer = CreateRenderer();
			renderer.WordWrap = _settings.WordWrap;
			int wrapWidth = renderer.WordWrap ? _superTip.BodyWidth : 0;
			int height = 0;
			int width = 0;
			int y = withShadow ? 2 : 1;
			int x = ContentMargin;
			Image image = _settings.GetImage(_superTip);
			if (_settings.Text.Length > 0 || image != null)
			{
				SizeF textSize = renderer.Measure(graphics, _settings.Text, _bodyFont, wrapWidth, 0);
				height = (int)textSize.Height + 4;
				if (image != null)
				{
					Size imageSize = image.Size;
					textSize.Width += imageSize.Width + 9;
					if (height < imageSize.Height + 6)
					{
						height = imageSize.Height + 6;
					}
					_bodyImageBounds = new Rectangle(ContentMargin + 4, y + (height - imageSize.Height) / 2, imageSize.Width, imageSize.Height);
					x += imageSize.Width + 9;
				}
				width = (int)textSize.Width + 5;
				_bodyTextBounds = new Rectangle(x, y, width - x, height);
			}
			Size result = new Size(width, height);
			if (withShadow)
			{
				result.Width += ShadowSize;
				result.Height += 6;
			}
			return result;
		}

		internal void AttachTo(Control control)
		{
			if (!control.IsHandleCreated)
			{
				control.CreateControl();
			}
			NativeMethods.SetParent(Handle, control.Handle);
		}

		private Bitmap CreateShadowCorner(int maxAlpha, Func<int, int, int> alphaFactor)
		{
			Bitmap bitmap = new Bitmap(CornerSize, CornerSize);
			int last = CornerSize - 1;
			for (int x = 0; x < CornerSize; x++)
			{
				for (int y = 0; y < CornerSize; y++)
				{
					int alpha = alphaFactor(x, y) * maxAlpha / (2 * last);
					bitmap.SetPixel(x, y, Color.FromArgb(alpha, 0, 0, 0));
				}
			}
			return bitmap;
		}

		private Bitmap GetShadowTopRight()
		{
			int last = CornerSize - 1;
			return _shadowTopRight ??= CreateShadowCorner(70, (x, y) => (last - x) + y);
		}

		private Bitmap GetShadowBottomLeft()
		{
			int last = CornerSize - 1;
			return _shadowBottomLeft ??= CreateShadowCorner(70, (x, y) => x + (last - y));
		}

		private Bitmap GetShadowBottomRight()
		{
			int last = CornerSize - 1;
			return _shadowBottomRight ??= CreateShadowCorner(65, (x, y) => (last - x) + (last - y));
		}

		private void DrawTip(Graphics graphics, Rectangle rect)
		{
			if (_hasShadow)
			{
				rect.Width -= ShadowSize;
				rect.Height -= ShadowSize;
			}
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			if (_hasShadow)
			{
				DrawShadow(graphics, rect);
			}
			if (_theme != null)
			{
				graphics.FillRectangle(_theme.Brush(ThemeRole.TooltipBack), Rectangle.Inflate(rect, -1, -1));
			}
			else
			{
				using (Brush brush = _settings.ToolTipStyle != ToolTipStyle.Standard
					? new LinearGradientBrush(rect, Color.White, _gradientEndColor, LinearGradientMode.Vertical)
					: (Brush)new SolidBrush(_superTip.BackColor))
				{
					graphics.FillRectangle(brush, Rectangle.Inflate(rect, -1, -1));
				}
			}
			DrawBorder(graphics, rect);
			using TextLayoutRenderer renderer = CreateRenderer();
			DrawImageAndText(graphics, renderer, _settings.GetHeaderImage(_superTip), _settings.HeaderText, _headerFont, _headerImageBounds, _headerTextBounds, TextColor(_superTip.HeaderForeColor, _superTip.ShouldSerializeHeaderForeColor() || _superTip.ShouldSerializeForeColor()));
			renderer.WordWrap = true;
			DrawImageAndText(graphics, renderer, _settings.GetFooterImage(_superTip), _settings.FooterText, _footerFont, _footerImageBounds, _footerTextBounds, TextColor(_superTip.FooterForeColor, _superTip.ShouldSerializeFooterForeColor() || _superTip.ShouldSerializeForeColor()));
			bool centerBody = !_settings.WordWrap && _settings.HasNoHeaderOrFooter();
			renderer.HorizontalAlignment = centerBody ? StringAlignment.Center : StringAlignment.Near;
			renderer.WordWrap = !centerBody;
			DrawImageAndText(graphics, renderer, _settings.GetImage(_superTip), _settings.Text, _bodyFont, _bodyImageBounds, _bodyTextBounds, TextColor(_superTip.ForeColor, _superTip.ShouldSerializeForeColor()));
			if (!_separatorBounds.IsEmpty)
			{
				DrawSeparator(graphics);
			}
		}

		private static void DrawImageAndText(Graphics graphics, TextLayoutRenderer renderer, Image image, string text, Font font, Rectangle imageBounds, Rectangle textBounds, Color color)
		{
			if (image != null)
			{
				graphics.DrawImage(image, imageBounds, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
			}
			if (text.Length > 0)
			{
				renderer.DrawText(graphics, text, textBounds, font, color);
			}
		}

		// A themed tip uses the tip text color of the theme unless the developer chose a color.
		private Color TextColor(Color configured, bool custom)
		{
			return _theme != null && !custom ? _theme.Color(ThemeRole.TooltipText) : configured;
		}

		private void DrawBorder(Graphics graphics, Rectangle rect)
		{
			Pen pen = GdiCache.Pen(_borderColor);
			graphics.DrawLine(pen, rect.X + 2, rect.Y, rect.Right - 3, rect.Y);
			graphics.DrawLine(pen, rect.Right - 1, rect.Y + 2, rect.Right - 1, rect.Bottom - 3);
			graphics.DrawLine(pen, rect.Right - 3, rect.Bottom - 1, rect.X + 2, rect.Bottom - 1);
			graphics.DrawLine(pen, rect.X, rect.Bottom - 3, rect.X, rect.Y + 2);
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.DrawArc(pen, rect.X, rect.Y, 4, 4, -180, 90);
			graphics.DrawArc(pen, rect.Right - 5, rect.Y, 4, 4, -90, 90);
			graphics.DrawArc(pen, rect.X, rect.Bottom - 5, 4, 4, 180, -90);
			graphics.DrawArc(pen, rect.Right - 5, rect.Bottom - 5, 4, 4, 90, -90);
			graphics.SmoothingMode = smoothingMode;
		}

		private void DrawSeparator(Graphics graphics)
		{
			Pen pen = GdiCache.Pen(_separatorColor);
			graphics.DrawLine(pen, _separatorBounds.Left + 3, _separatorBounds.Top, _separatorBounds.Right - 6, _separatorBounds.Top);
			if (_theme == null)
			{
				graphics.DrawLine(Pens.White, _separatorBounds.Left + 3, _separatorBounds.Top + 1, _separatorBounds.Right - 6, _separatorBounds.Top + 1);
			}
		}

		private void DrawShadow(Graphics graphics, Rectangle rect)
		{
			Color dark = Color.FromArgb(70, Color.Black);
			Color clear = Color.FromArgb(0, Color.Black);
			Blend blend = new Blend(3)
			{
				Positions = new float[3] { 0f, 0.6f, 1f },
				Factors = new float[3] { 0f, 0.4f, 1f }
			};
			if (rect.Height >= CornerSize)
			{
				graphics.DrawImageUnscaled(GetShadowTopRight(), new Rectangle(rect.Right - 1, rect.Top + 4, CornerSize, CornerSize));
				if (rect.Height >= 8)
				{
					Rectangle side = new Rectangle(rect.Right - 1, rect.Top + 9, 6, rect.Height - 10);
					using LinearGradientBrush sideBrush = new LinearGradientBrush(side, dark, clear, 0f);
					sideBrush.Blend = blend;
					graphics.FillRectangle(sideBrush, side);
				}
			}
			if (rect.Width >= CornerSize)
			{
				graphics.DrawImageUnscaled(GetShadowBottomLeft(), new Rectangle(rect.Left + 4, rect.Bottom - 1, CornerSize, CornerSize));
				Rectangle bottom = new Rectangle(rect.Left + 9, rect.Bottom - 1, rect.Width - 9, 6);
				using LinearGradientBrush bottomBrush = new LinearGradientBrush(bottom, dark, clear, 90f);
				bottomBrush.Blend = blend;
				graphics.FillRectangle(bottomBrush, bottom);
			}
			graphics.DrawImageUnscaled(GetShadowBottomRight(), new Rectangle(rect.Right, rect.Bottom - 1, CornerSize, CornerSize));
		}

		// Draws the tip into the shared back buffer, resized when the window size changes.
		private Bitmap RenderBackBuffer(int width, int height)
		{
			if (_backBuffer == null || _backBuffer.Width != width || _backBuffer.Height != height)
			{
				_backBuffer?.Dispose();
				_backBuffer = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			}
			using Graphics graphics = Graphics.FromImage(_backBuffer);
			graphics.Clear(Color.Transparent);
			DrawTip(graphics, new Rectangle(0, 0, width, height));
			return _backBuffer;
		}

		private void PaintLayered()
		{
			Bitmap buffer = RenderBackBuffer(_bounds.Width, _bounds.Height);
			IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
			IntPtr memoryDc = NativeMethods.CreateCompatibleDC(screenDc);
			IntPtr bitmapHandle = IntPtr.Zero;
			IntPtr previousObject = IntPtr.Zero;
			try
			{
				bitmapHandle = buffer.GetHbitmap(Color.FromArgb(0));
				previousObject = NativeMethods.SelectObject(memoryDc, bitmapHandle);
				NativeMethods.Point windowPosition = new NativeMethods.Point { X = _bounds.X, Y = _bounds.Y };
				NativeMethods.Point sourcePosition = default;
				NativeMethods.Size size = new NativeMethods.Size { Width = _bounds.Width, Height = _bounds.Height };
				NativeMethods.BlendFunction blend = new NativeMethods.BlendFunction
				{
					SourceConstantAlpha = (byte)_alpha,
					AlphaFormat = 1
				};
				NativeMethods.UpdateLayeredWindow(Handle, screenDc, ref windowPosition, ref size, memoryDc, ref sourcePosition, 0, ref blend, NativeMethods.UlwAlpha);
			}
			finally
			{
				if (previousObject != IntPtr.Zero)
				{
					NativeMethods.SelectObject(memoryDc, previousObject);
				}
				if (bitmapHandle != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(bitmapHandle);
				}
				NativeMethods.DeleteDC(memoryDc);
				NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			}
		}

		private void PaintWindow(Message message)
		{
			IntPtr dc = NativeMethods.BeginPaint(message.HWnd, out NativeMethods.PaintStruct paint);
			try
			{
				NativeMethods.GetWindowRect(message.HWnd, out NativeMethods.Rect rect);
				int width = rect.Right - rect.Left;
				int height = rect.Bottom - rect.Top;
				if (width > 0 && height > 0)
				{
					Bitmap buffer = RenderBackBuffer(width, height);
					using Graphics graphics = Graphics.FromHdc(dc);
					graphics.DrawImageUnscaled(buffer, 0, 0);
				}
			}
			finally
			{
				NativeMethods.EndPaint(message.HWnd, ref paint);
			}
		}

		protected override void WndProc(ref Message message)
		{
			switch (message.Msg)
			{
			case NativeMethods.WmHideSuperTip:
			case NativeMethods.WmActivateApp:
			case 161: // WM_NCLBUTTONDOWN
			case 164: // WM_NCRBUTTONDOWN
			case 167: // WM_NCMBUTTONDOWN
			case 513: // WM_LBUTTONDOWN
			case 516: // WM_RBUTTONDOWN
			case 519: // WM_MBUTTONDOWN
				Hide();
				break;
			case NativeMethods.WmNcHitTest:
				message.Result = new IntPtr(NativeMethods.HtTransparent);
				break;
			case NativeMethods.WmMouseActivate:
				message.Result = (IntPtr)NativeMethods.MaNoActivate;
				break;
			case NativeMethods.WmPaint:
				PaintWindow(message);
				break;
			case NativeMethods.WmSetCursor:
				NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IdcArrow));
				break;
			default:
				base.WndProc(ref message);
				break;
			}
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case NativeMethods.WmActivate:
				if (_anchorControl != null)
				{
					Control topLevel = _anchorControl.TopLevelControl;
					IntPtr target = topLevel != null ? topLevel.Handle : _anchorControl.Handle;
					if (message.HWnd == target)
					{
						Hide();
					}
				}
				break;
			case 161: // WM_NCLBUTTONDOWN
			case 163: // WM_NCLBUTTONDBLCLK
			case 164: // WM_NCRBUTTONDOWN
			case 166: // WM_NCRBUTTONDBLCLK
			case 167: // WM_NCMBUTTONDOWN
			case 169: // WM_NCMBUTTONDBLCLK
			case 513: // WM_LBUTTONDOWN
			case 515: // WM_LBUTTONDBLCLK
			case 516: // WM_RBUTTONDOWN
			case 518: // WM_RBUTTONDBLCLK
			case 519: // WM_MBUTTONDOWN
			case 521: // WM_MBUTTONDBLCLK
				Hide();
				break;
			}
			return false;
		}
	}
}
