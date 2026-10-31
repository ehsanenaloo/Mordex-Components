using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class SuperTipSettingsWindowMessageHandler : NativeWindow, IMessageFilter
	{
		private const int ClearTypeSmoothing = 2;

		private const int UlwAlpha = 2;

		private const int FadeInterval = 70;

		private const int FadeStep = 80;

		private const int MaxAlpha = 255;

		private const uint IdcArrow = 32512u;

		private bool _isShadowActive;

		private Bitmap _shadowBottomRightBitmap;

		private Bitmap _shadowBottomLeftBitmap;

		private Bitmap _shadowRightTopBitmap;

		private Font _font;

		private SuperTipSettings _superTipSettings;

		private Rectangle _windowBounds = Rectangle.Empty;

		private bool _useCompatibleTextRendering;

		private Font _ownedFont;

		private Rectangle _headerTextRect;

		private Rectangle _bodyTextRect;

		private Rectangle _footerTextRect;

		private Rectangle _headerImageRect;

		private Rectangle _bodyImageRect;

		private Rectangle _footerImageRect;

		private Rectangle _footerSeparatorRect;

		private string _headerSuffix;

		private bool _isShadowRequested;

		public bool _isKeyTipEnabled;

		public int _headerPadding;

		public RibbonColorScheme _office2007Colors;

		private bool _isCompactMode;

		private int _alpha = 20;

		private bool _isFadeEnabled = true;

		public Point _anchorPoint = Point.Empty;

		internal Control _anchorControl;

		private int _showDelay = 900;

		private int _displayDuration = 20000;

		private bool _isSticky;

		private System.Windows.Forms.Timer _filterRemovalTimer;

		private bool _isShown;

		public bool _isShowRequested;

		private System.Windows.Forms.Timer _delayTimer;

		private System.Windows.Forms.Timer _fadeTimer;

		private bool _isAutoHideTimerRunning;

		internal SuperTipSettingsWindowMessageHandler(Font font, bool useCompatibleTextRendering, string text, RibbonColorScheme office2007Colors)
		{
			InitializeTimers();
			SetFont(font);
			_useCompatibleTextRendering = useCompatibleTextRendering;
			try
			{
				_useCompatibleTextRendering = SystemInformation.FontSmoothingType == ClearTypeSmoothing;
			}
			catch (System.ComponentModel.Win32Exception)
			{
				// Keep the caller's setting when the system setting cannot be read.
			}
			_office2007Colors = office2007Colors ?? ThemedRibbon.CreateScheme(MxThemeManager.ResolveDefault(96));
			_superTipSettings = new SuperTipSettings();
			_superTipSettings.HeaderText = text;
			_headerSuffix = "";
			_showDelay = 0;
			_headerPadding = 1;
			_isShadowRequested = false;
			_alpha = 255;
		}

		private string GetFullHeaderText()
		{
			string text = _superTipSettings.HeaderText;
			if (text.Length > 0 && _headerSuffix.Length > 0)
			{
				text = text + " " + _headerSuffix;
			}
			return text;
		}

		private int GetCurrentAlpha()
		{
			if (!_isKeyTipEnabled)
			{
				return 150;
			}
			return _alpha;
		}

		private void SetAlpha(int superTipSettingsWindowMessageHandlerInteger)
		{
			_alpha = superTipSettingsWindowMessageHandlerInteger;
		}

		internal bool IsCompactMode()
		{
			return _isCompactMode;
		}

		internal void SetCompactMode(bool isCompact)
		{
			_isCompactMode = isCompact;
		}

		private void DrawBorder(Graphics graphics, Rectangle rectangle)
		{
			Pen pen = GdiCache.Pen(_office2007Colors._superTipBorder);
			graphics.DrawLine(pen, rectangle.X + 2, rectangle.Y, rectangle.Right - 3, rectangle.Y);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 1, rectangle.Bottom - 3);
			graphics.DrawLine(pen, rectangle.Right - 3, rectangle.Bottom - 1, rectangle.X + 2, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.X, rectangle.Bottom - 3, rectangle.X, rectangle.Y + 2);
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.DrawArc(pen, rectangle.X, rectangle.Y, 4, 4, -180, 90);
			graphics.DrawArc(pen, rectangle.Right - 5, rectangle.Y, 4, 4, -90, 90);
			graphics.DrawArc(pen, rectangle.X, rectangle.Bottom - 5, 4, 4, 180, -90);
			graphics.DrawArc(pen, rectangle.Right - 5, rectangle.Bottom - 5, 4, 4, 90, -90);
			graphics.SmoothingMode = smoothingMode;
		}

		private void DrawBackground(Graphics graphics, Rectangle bounds)
		{
			if (_isShadowActive)
			{
				bounds.Width -= 4;
				bounds.Height -= 4;
			}
			if (bounds.Width > 0 && bounds.Height > 0)
			{
				if (_isShadowActive && _office2007Colors.Theme.Shape.HasShadow)
				{
					DrawShadow(graphics, bounds);
				}
				ThemedRibbonPainter.KeyTip(graphics, _office2007Colors.Theme, bounds);
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				if (IsCompactMode())
				{
					textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				}
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				DrawImageAndText(graphics, textLayoutRenderer, _superTipSettings.HeaderImage, GetFullHeaderText(), GetFont(), _headerImageRect, _headerTextRect);
				textLayoutRenderer.SetWrapEnabled(true);
				DrawImageAndText(graphics, textLayoutRenderer, _superTipSettings.FooterImage, _superTipSettings.FooterText, GetFont(), _footerImageRect, _footerTextRect);
				textLayoutRenderer.SetLineAlignment(StringAlignment.Near);
				DrawImageAndText(graphics, textLayoutRenderer, _superTipSettings.Image, _superTipSettings.Text, GetFont(), _bodyImageRect, _bodyTextRect);
				if (!_footerSeparatorRect.IsEmpty)
				{
					DrawFooterSeparator(graphics);
				}
			}
		}

		private void DrawImageAndText(Graphics graphics, TextLayoutRenderer textLayoutRenderer, Image image, string text, Font font, Rectangle rect, Rectangle rectangle)
		{
			if (image != null)
			{
				graphics.DrawImageUnscaled(image, rect);
			}
			if (text.Length > 0)
			{
				textLayoutRenderer.DrawTextWithColor(graphics, text, rectangle, font, _office2007Colors.Theme.Color(ThemeRole.TooltipText));
			}
		}

		private void DrawFooterSeparator(Graphics graphics)
		{
			Pen pen = GdiCache.Pen(_office2007Colors._superTipFooterLine);
			graphics.DrawLine(pen, _footerSeparatorRect.Left + 3, _footerSeparatorRect.Top, _footerSeparatorRect.Width - 6, _footerSeparatorRect.Top);
			graphics.DrawLine(Pens.White, _footerSeparatorRect.Left + 3, _footerSeparatorRect.Top + 1, _footerSeparatorRect.Width - 6, _footerSeparatorRect.Top + 1);
		}

		private void DrawShadow(Graphics graphics, Rectangle rectangle)
		{
			Color color = Color.FromArgb(70, Color.Black);
			Color shadowEndColor = Color.FromArgb(0, Color.Black);
			Blend blend = new Blend(3);
			blend.Positions = new float[3] { 0f, 0.6f, 1f };
			blend.Factors = new float[3] { 0f, 0.4f, 1f };
			if (rectangle.Height >= 5)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Right - 1, rectangle.Top + 4, 5, 5), image: GetShadowRightTopBitmap());
				if (rectangle.Height >= 8)
				{
					Rectangle rect = new Rectangle(rectangle.Right - 1, rectangle.Top + 9, 6, rectangle.Height - 10);
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, shadowEndColor, 0f);
					linearGradientBrush.Blend = blend;
					graphics.FillRectangle(linearGradientBrush, rect);
					linearGradientBrush.Dispose();
				}
			}
			if (rectangle.Width >= 5)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Left + 4, rectangle.Bottom - 1, 5, 5), image: GetShadowBottomLeftBitmap());
				Rectangle rect = new Rectangle(rectangle.Left + 9, rectangle.Bottom - 1, rectangle.Width - 9, 6);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, shadowEndColor, 90f);
				linearGradientBrush.Blend = blend;
				graphics.FillRectangle(linearGradientBrush, rect);
				linearGradientBrush.Dispose();
			}
			graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Right, rectangle.Bottom - 1, 5, 5), image: GetShadowBottomRightBitmap());
		}

		private Size MeasureSize(bool includeShadow)
		{
			IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
			try
			{
				using (Graphics graphics = RibbonDrawingUtils.CreateGraphicsFromHdc(screenDc))
				{
					return MeasureSize(graphics, includeShadow);
				}
			}
			finally
			{
				NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			}
		}

		private Size MeasureSize(Graphics graphics, bool includeShadow)
		{
			using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			int footerHeight = 0;
			int headerHeight = 0;
			int bodyHeight = 0;
			int contentWidth = 0;
			int currentY = 1;
			int horizontalMargin = 3;
			if (_isCompactMode)
			{
				horizontalMargin = 1;
			}
			int currentX = horizontalMargin;
			if (_isShadowActive)
			{
				currentY = 2;
			}
			int topInset = currentY;
			int headerPadding = _headerPadding;
			if (_isCompactMode)
			{
				headerPadding = 0;
			}
			else if (_superTipSettings.Image != null || _superTipSettings.Text.Length > 0)
			{
				headerPadding += 2;
			}
			if (GetFullHeaderText().Length > 0 || _superTipSettings.HeaderImage != null)
			{
				string text = GetFullHeaderText();
				if (IsCompactMode() && text.Length == 1)
				{
					text = "W";
				}
				SizeF sizeF = textLayoutRenderer.MeasureText(graphics, text, GetFont(), 0, 0);
				headerHeight = (int)sizeF.Height + headerPadding;
				if (_superTipSettings.HeaderImage != null)
				{
					Size size = _superTipSettings.HeaderImage.Size;
					sizeF.Width += size.Width + 9;
					if (headerHeight < size.Height + 6)
					{
						headerHeight = size.Height + 6;
					}
					_headerImageRect = new Rectangle(horizontalMargin + 4, currentY + (headerHeight - size.Height) / 2, size.Width, size.Height);
					currentX += size.Width + 9;
				}
				contentWidth = ((!IsCompactMode()) ? ((int)sizeF.Width + 2) : ((int)sizeF.Width + horizontalMargin));
				_headerTextRect = new Rectangle(currentX, currentY, contentWidth, headerHeight);
				currentY += headerHeight;
				currentX = horizontalMargin;
			}
			int bodyMinWidth = 0;
			if (_superTipSettings.Image != null)
			{
				int width = _superTipSettings.Image.Width;
				if (_superTipSettings.Text.Length > 0)
				{
					bodyMinWidth += width + 12;
				}
			}
			if (contentWidth < bodyMinWidth && (_superTipSettings.Text.Length > 0 || _superTipSettings.Image != null))
			{
				contentWidth = bodyMinWidth;
			}
			if (_superTipSettings.Text.Length > 0 || _superTipSettings.Image != null)
			{
				int bodyTextWidth = contentWidth - 18;
				currentX = horizontalMargin + 9;
				Size bodyImageSize = Size.Empty;
				if (_superTipSettings.Image != null)
				{
					bodyImageSize = _superTipSettings.Image.Size;
					bodyHeight = bodyImageSize.Height + 12;
					bodyTextWidth -= bodyImageSize.Width + 11;
				}
				textLayoutRenderer.SetWrapEnabled(true);
				SizeF bodyTextSize = textLayoutRenderer.MeasureText(graphics, _superTipSettings.Text, GetFont(), bodyTextWidth, 0);
				if (bodyHeight < (int)bodyTextSize.Height + 10)
				{
					bodyHeight = (int)bodyTextSize.Height + 10;
				}
				if (_superTipSettings.Image != null)
				{
					_bodyImageRect = new Rectangle(horizontalMargin + 11, currentY + 4, bodyImageSize.Width, bodyImageSize.Height);
					currentX += bodyImageSize.Width + 12;
				}
				_bodyTextRect = new Rectangle(currentX, currentY + 4, bodyTextWidth, bodyHeight);
				currentY += bodyHeight;
			}
			currentX = horizontalMargin;
			if (_superTipSettings.FooterText.Length > 0 || _superTipSettings.FooterImage != null)
			{
				_footerSeparatorRect = new Rectangle(horizontalMargin, currentY - 2, contentWidth, 2);
				footerHeight = (int)textLayoutRenderer.MeasureText(graphics, _superTipSettings.FooterText, GetFont(), 0, 0).Height + 15;
				if (_superTipSettings.FooterImage != null)
				{
					Size footerImageSize = _superTipSettings.FooterImage.Size;
					if (footerHeight < footerImageSize.Height + 13)
					{
						footerHeight = footerImageSize.Height + 13;
					}
					_footerImageRect = new Rectangle(horizontalMargin + 4, currentY + (footerHeight - footerImageSize.Height) / 2, footerImageSize.Width, footerImageSize.Height);
					currentX += footerImageSize.Width + 9;
				}
				_footerTextRect = new Rectangle(currentX, currentY, contentWidth - currentX, footerHeight);
			}
			Size result = new Size(contentWidth + horizontalMargin * 2, headerHeight + bodyHeight + footerHeight + topInset);
			if (includeShadow)
			{
				result.Width += 4;
				result.Height += 6;
			}
			return result;
		}

		public Font GetFont()
		{
			if (_font == null)
			{
				return Control.DefaultFont;
			}
			return _font;
		}

		public void SetFont(Font font)
		{
			_font = font;
		}

		private Image GetShadowRightTopBitmap()
		{
			if (_shadowRightTopBitmap == null)
			{
				CreateShadowRightTopBitmap();
			}
			return _shadowRightTopBitmap;
		}

		private Image GetShadowBottomLeftBitmap()
		{
			if (_shadowBottomLeftBitmap == null)
			{
				CreateShadowBottomLeftBitmap();
			}
			return _shadowBottomLeftBitmap;
		}

		private Image GetShadowBottomRightBitmap()
		{
			if (_shadowBottomRightBitmap == null)
			{
				CreateShadowBottomRightBitmap();
			}
			return _shadowBottomRightBitmap;
		}

		private void CreateShadowBottomLeftBitmap()
		{
			Bitmap bitmap = new Bitmap(5, 5);
			int maxAlpha = 70;
			int unusedStep = 8;
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					int xAlpha = i * maxAlpha / 4;
					int yAlpha = (4 - j) * maxAlpha / 4;
					int alpha = (xAlpha + yAlpha) / 2;
					Color color = Color.FromArgb(alpha, Color.Empty);
					bitmap.SetPixel(i, j, color);
				}
				unusedStep += 24;
			}
			_shadowBottomLeftBitmap = bitmap;
		}

		private void CreateShadowRightTopBitmap()
		{
			Bitmap bitmap = new Bitmap(5, 5);
			int unusedStep = 8;
			int maxAlpha = 70;
			for (int i = 0; i < 5; i++)
			{
				for (int x = 4; x >= 0; x--)
				{
					int xAlpha = (4 - x) * maxAlpha / 4;
					int yAlpha = i * maxAlpha / 4;
					int alpha = (xAlpha + yAlpha) / 2;
					Color color = Color.FromArgb(alpha, Color.Empty);
					bitmap.SetPixel(x, i, color);
				}
				unusedStep += 12;
			}
			_shadowRightTopBitmap = bitmap;
		}

		private void CreateShadowBottomRightBitmap()
		{
			Bitmap bitmap = new Bitmap(5, 5);
			int unusedStep = 96;
			int maxAlpha = 65;
			for (int i = 0; i < 5; i++)
			{
				for (int x = 4; x >= 0; x--)
				{
					int xAlpha = (4 - x) * maxAlpha / 4;
					int yAlpha = (4 - i) * maxAlpha / 4;
					int alpha = (xAlpha + yAlpha) / 2;
					Color color = Color.FromArgb(alpha, Color.Empty);
					bitmap.SetPixel(x, i, color);
				}
				unusedStep -= 24;
			}
			_shadowBottomRightBitmap = bitmap;
		}

		private void PlaceWithinWorkingArea(Point pt)
		{
			int tipX = pt.X;
			int tipY = pt.Y;
			if (_anchorPoint.IsEmpty)
			{
				tipY += 16;
			}
			Rectangle workingArea = Screen.GetWorkingArea(pt);
			if (tipX + _windowBounds.Width > workingArea.Right)
			{
				tipX = workingArea.Right - _windowBounds.Width;
			}
			if (tipX < workingArea.X)
			{
				tipX = workingArea.X;
			}
			if (tipY + _windowBounds.Height > workingArea.Bottom)
			{
				tipY -= _windowBounds.Height;
			}
			_windowBounds.X = tipX;
			_windowBounds.Y = tipY;
		}

		private void CreateAndShowWindow()
		{
			bool isLayered = RibbonDrawingUtils.IsHighColorDisplay();
			_isShadowActive = isLayered && _isShadowRequested;
			_windowBounds.Size = MeasureSize(_isShadowActive);
			if (_anchorPoint.IsEmpty)
			{
				PlaceWithinWorkingArea(Cursor.Position);
			}
			else
			{
				PlaceWithinWorkingArea(_anchorPoint);
			}
			CreateParams createParams = new CreateParams();
			createParams.X = _windowBounds.X;
			createParams.Y = _windowBounds.Y;
			createParams.Style = int.MinValue;
			createParams.Height = _windowBounds.Height;
			createParams.Width = _windowBounds.Width;
			createParams.Parent = IntPtr.Zero;
			createParams.ExStyle = 136;
			if (isLayered)
			{
				createParams.ExStyle += 524288;
			}
			CreateHandle(createParams);
			NativeMethods.ShowWindow(base.Handle, 4);
			OnShown(EventArgs.Empty);
			if (_isFadeEnabled && isLayered)
			{
				if (_isSticky)
				{
					PaintLayeredWindow();
				}
				else
				{
					if (_fadeTimer == null)
					{
						_fadeTimer = CreateFadeTimer();
					}
					_fadeTimer.Start();
				}
			}
		}

		public int GetDisplayDuration()
		{
			return _displayDuration;
		}

		public bool IsSticky()
		{
			return _isSticky;
		}

		public void SetSticky(bool isSticky)
		{
			_isSticky = isSticky;
		}

		public void ShowAt(Control control, Point point)
		{
			_anchorPoint = point;
			if (_isShowRequested)
			{
				ShowTipWithAutoHide();
				return;
			}
			_isShowRequested = true;
			_delayTimer.Stop();
			_anchorControl = control;
			if (_showDelay > 0)
			{
				_delayTimer.Interval = _showDelay;
				_delayTimer.Start();
			}
			else
			{
				ShowTipWithAutoHide();
			}
		}

		private void ShowTip()
		{
			if (_filterRemovalTimer != null)
			{
				_filterRemovalTimer.Stop();
				_filterRemovalTimer.Tick -= OnFilterRemovalTimerTick;
				_filterRemovalTimer.Dispose();
				_filterRemovalTimer = null;
			}
			// The filter is registered at most once, even when the tip is shown again before it was removed.
			Application.RemoveMessageFilter(this);
			Application.AddMessageFilter(this);
			CreateAndShowWindow();
			NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, IdcArrow));
			_isShown = true;
		}

		internal void HideTip()
		{
			_isShowRequested = false;
			if (_ownedFont != null)
			{
				_ownedFont.Dispose();
				_ownedFont = null;
			}
			if (_delayTimer != null)
			{
				_delayTimer.Stop();
			}
			SetAlpha(20);
			_isAutoHideTimerRunning = false;
			if (_fadeTimer != null)
			{
				_fadeTimer.Stop();
				if (_fadeTimer != null)
				{
					_fadeTimer.Dispose();
					_fadeTimer = null;
				}
			}
			if (_isShown)
			{
				NativeMethods.ShowWindow(base.Handle, 0);
				DestroyHandle();
				if (_filterRemovalTimer != null)
				{
					_filterRemovalTimer.Stop();
					_filterRemovalTimer.Tick -= OnFilterRemovalTimerTick;
					_filterRemovalTimer.Dispose();
				}
				_filterRemovalTimer = new System.Windows.Forms.Timer();
				_filterRemovalTimer.Tick += OnFilterRemovalTimerTick;
				_filterRemovalTimer.Interval = 10;
				_filterRemovalTimer.Start();
				_isShown = false;
			}
		}

		private void DisposeBitmaps()
		{
			_shadowBottomRightBitmap?.Dispose();
			_shadowBottomRightBitmap = null;
			_shadowBottomLeftBitmap?.Dispose();
			_shadowBottomLeftBitmap = null;
			_shadowRightTopBitmap?.Dispose();
			_shadowRightTopBitmap = null;
		}

		internal void DisposeTimersAndBitmaps()
		{
			_isShowRequested = false;
			if (_delayTimer != null)
			{
				_delayTimer.Stop();
				if (_delayTimer != null)
				{
					_delayTimer.Dispose();
					_delayTimer = null;
				}
			}
			if (_ownedFont != null)
			{
				_ownedFont.Dispose();
				_ownedFont = null;
			}
			if (_fadeTimer != null)
			{
				_fadeTimer.Stop();
				if (_fadeTimer != null)
				{
					_fadeTimer.Dispose();
					_fadeTimer = null;
				}
			}
			SetAlpha(20);
			_isAutoHideTimerRunning = false;
			DisposeBitmaps();
			if (!_isShown)
			{
				return;
			}
			NativeMethods.ShowWindow(base.Handle, 0);
			DestroyHandle();
			Application.RemoveMessageFilter(this);
			if (_filterRemovalTimer != null)
			{
				_filterRemovalTimer.Stop();
				if (_filterRemovalTimer != null)
				{
					_filterRemovalTimer.Dispose();
					_filterRemovalTimer = null;
				}
			}
			_isShown = false;
		}

		private void OnFilterRemovalTimerTick(object sender, EventArgs eventArgs)
		{
			Application.RemoveMessageFilter(this);
			if (_filterRemovalTimer != null)
			{
			System.Windows.Forms.Timer timer = _filterRemovalTimer;
				_filterRemovalTimer = null;
				timer.Stop();
				timer.Dispose();
			}
		}

		protected override void WndProc(ref Message message)
		{
			if (message.Msg == 1025)
			{
				HideTip();
				return;
			}
			switch (message.Msg)
			{
			case NativeMethods.WmNcHitTest:
				message.Result = new IntPtr(-1);
				break;
			case NativeMethods.WmNcLButtonDown:
			case NativeMethods.WmNcRButtonDown:
			case NativeMethods.WmNcMButtonDown:
			case NativeMethods.WmLButtonDown:
			case NativeMethods.WmRButtonDown:
			case NativeMethods.WmMButtonDown:
				HideTip();
				break;
			case NativeMethods.WmActivateApp:
				HideTip();
				break;
			case NativeMethods.WmMouseActivate:
				message.Result = (IntPtr)3;
				break;
			case NativeMethods.WmPaint:
				PaintWindow(message);
				break;
			case NativeMethods.WmSetCursor:
				NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, IdcArrow));
				break;
			default:
				base.WndProc(ref message);
				break;
			}
		}

		private void PaintLayeredWindow()
		{
			using (Bitmap bitmap = new Bitmap(_windowBounds.Width, _windowBounds.Height, PixelFormat.Format32bppArgb))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					Rectangle rectangle = new Rectangle(0, 0, _windowBounds.Width, _windowBounds.Height);
					if (!_useCompatibleTextRendering)
					{
						graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
					}
					DrawBackground(graphics, rectangle);
				}
				IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
				IntPtr memoryDc = NativeMethods.CreateCompatibleDC(screenDc);
				IntPtr hbitmap = bitmap.GetHbitmap(Color.FromArgb(0));
				IntPtr previousBitmap = NativeMethods.SelectObject(memoryDc, hbitmap);
				try
				{
					NativeMethods.NativePoint m = default(NativeMethods.NativePoint);
					m._x = _windowBounds.X;
					m._y = _windowBounds.Y;
					NativeMethods.NativePoint sourcePoint = default(NativeMethods.NativePoint);
					sourcePoint._x = 0;
					sourcePoint._y = 0;
					NativeMethods.NativeSize h = default(NativeMethods.NativeSize);
					h._width = _windowBounds.Width;
					h._height = _windowBounds.Height;
					NativeMethods.BlendFunction g = new NativeMethods.BlendFunction
					{
						_blendOp = 0,
						_sourceConstantAlpha = (byte)GetCurrentAlpha(),
						_blendFlags = 0,
						_alphaFormat = 1
					};
					NativeMethods.UpdateLayeredWindow(base.Handle, screenDc, ref m, ref h, memoryDc, ref sourcePoint, 0, ref g, UlwAlpha);
				}
				finally
				{
					NativeMethods.SelectObject(memoryDc, previousBitmap);
					NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
					NativeMethods.DeleteObject(hbitmap);
					NativeMethods.DeleteDC(memoryDc);
				}
			}
		}

		protected void PaintWindow(Message message)
		{
			NativeMethods.PaintStruct c = default(NativeMethods.PaintStruct);
			IntPtr paintDc = NativeMethods.BeginPaint(message.HWnd, ref c);
			try
			{
				NativeMethods.NativeRect i = default(NativeMethods.NativeRect);
				NativeMethods.GetWindowRect(message.HWnd, ref i);
				Rectangle rectangle = new Rectangle(0, 0, i._right - i._left, i._bottom - i._top);
				using (Graphics graphics = RibbonDrawingUtils.CreateGraphicsFromHdc(paintDc))
				using (Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height))
				{
					using (Graphics bufferGraphics = Graphics.FromImage(bitmap))
					{
						if (!_useCompatibleTextRendering)
						{
							bufferGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
						}
						DrawBackground(bufferGraphics, rectangle);
					}
					graphics.DrawImageUnscaled(bitmap, 0, 0);
				}
			}
			finally
			{
				NativeMethods.EndPaint(message.HWnd, ref c);
			}
		}

		private void InitializeTimers()
		{
			_isKeyTipEnabled = true;
			_delayTimer = new System.Windows.Forms.Timer();
			_delayTimer.Tick += OnShowDelayTimerTick;
			_fadeTimer = CreateFadeTimer();
		}

		private System.Windows.Forms.Timer CreateFadeTimer()
		{
			System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer();
			fadeTimer.Interval = FadeInterval;
			fadeTimer.Tick += OnFadeTimerTick;
			return fadeTimer;
		}

		private void OnFadeTimerTick(object sender, EventArgs eventArgs)
		{
			if (GetCurrentAlpha() + FadeStep < MaxAlpha)
			{
				SetAlpha(GetCurrentAlpha() + FadeStep);
			}
			else
			{
				SetAlpha(MaxAlpha);
			}
			PaintLayeredWindow();
			if (GetCurrentAlpha() >= MaxAlpha)
			{
				_fadeTimer?.Stop();
			}
		}

		private void OnShowDelayTimerTick(object sender, EventArgs eventArgs)
		{
			if (_delayTimer != null)
			{
				_delayTimer.Stop();
			}
			ShowTipWithAutoHide();
		}

		private void ShowTipWithAutoHide()
		{
			if (!_isAutoHideTimerRunning)
			{
				ShowTip();
				if (!IsSticky())
				{
					_delayTimer.Interval = GetDisplayDuration();
					_isAutoHideTimerRunning = true;
					_delayTimer.Start();
				}
			}
			else
			{
				HideTip();
			}
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case NativeMethods.WmActivate:
			{
				if (_anchorControl == null)
				{
					break;
				}
				if (_anchorControl.TopLevelControl != null)
				{
					if (message.HWnd == _anchorControl.TopLevelControl.Handle)
					{
						HideTip();
					}
				}
				else if (_anchorControl.Handle == message.HWnd)
				{
					HideTip();
				}
				break;
			}
			case NativeMethods.WmNcLButtonDown:
			case NativeMethods.WmNcLButtonDoubleClick:
			case NativeMethods.WmNcRButtonDown:
			case NativeMethods.WmNcRButtonDoubleClick:
			case NativeMethods.WmNcMButtonDown:
			case 169:
			case NativeMethods.WmLButtonDown:
			case NativeMethods.WmLButtonDoubleClick:
			case NativeMethods.WmRButtonDown:
			case NativeMethods.WmRButtonDoubleClick:
			case NativeMethods.WmMButtonDown:
			case 521:
				HideTip();
				break;
			}
			return false;
		}

		protected virtual void OnShown(EventArgs eventArgs)
		{
		}
	}
}
