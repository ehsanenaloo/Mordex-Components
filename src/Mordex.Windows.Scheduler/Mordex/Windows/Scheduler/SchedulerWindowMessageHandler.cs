using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerWindowMessageHandler : NativeWindow, IMessageFilter
	{
		private Color _textColor;

		private Color _backColor;

		private Size _requestedSize = Size.Empty;

		private Bitmap _topRightShadow;

		private Bitmap _bottomLeftShadow;

		private Bitmap _bottomRightShadow;

		public bool _wrapsText = true;

		private Size _windowSize = Size.Empty;

		private Font _font;

		public string _text = "";

		private bool _isLayeredWindow;

		private bool _shouldFadeIn = true;

		private Point _screenPosition = Point.Empty;

		internal Control _control;

		private int _initialDelay = 500;

		private int _autoHideDelay = 5000;

		private Timer _filterRemovalTimer;

		private bool _isWindowCreated;

		public bool _isActive;

		private int _alpha = 20;

		private Timer _delayTimer;

		private Timer _fadeTimer;

		private bool _isHideTimerStarted;

		internal SchedulerWindowMessageHandler()
		{
			CreateTimers();
			try
			{
				Application.AddMessageFilter(this);
			}
			finally
			{
			}
		}

		public Color GetTextColor()
		{
			if (_textColor.IsEmpty)
			{
				return Theme().Color(ThemeRole.TooltipText);
			}
			return _textColor;
		}

		private ResolvedTheme Theme()
		{
			return MxThemeManager.ResolveDefault(_control == null || _control.IsDisposed ? 96 : _control.DeviceDpi);
		}

		public void SetTextColor(Color color)
		{
			_textColor = color;
		}

		public Color GetBackColor()
		{
			if (_backColor.IsEmpty)
			{
				return Theme().Color(ThemeRole.TooltipBack);
			}
			return _backColor;
		}

		public void SetBackColor(Color schedulerWindowMessageHandlerColor)
		{
			_backColor = schedulerWindowMessageHandlerColor;
		}

		public void SetRequestedSize(Size size)
		{
			if (_requestedSize != size)
			{
				_requestedSize = size;
			}
		}

		private Image GetTopRightShadowImage()
		{
			if (_topRightShadow == null)
			{
				CreateTopRightShadow();
			}
			return _topRightShadow;
		}

		private Image GetBottomLeftShadowImage()
		{
			if (_bottomLeftShadow == null)
			{
				CreateBottomLeftShadow();
			}
			return _bottomLeftShadow;
		}

		private Image GetBottomRightShadowImage()
		{
			if (_bottomRightShadow == null)
			{
				CreateBottomRightShadow();
			}
			return _bottomRightShadow;
		}

		private void CreateBottomLeftShadow()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			int baseAlpha = 8;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int alpha = baseAlpha - baseAlpha / 4 * j;
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(i, j, color);
				}
				baseAlpha += 24;
			}
			_bottomLeftShadow = bitmap;
		}

		private void CreateTopRightShadow()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			int baseAlpha = 8;
			for (int i = 0; i < 4; i++)
			{
				for (int column = 3; column >= 0; column--)
				{
					int alpha = baseAlpha - baseAlpha / 4 * column;
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(column, i, color);
				}
				baseAlpha += 24;
			}
			_topRightShadow = bitmap;
		}

		private void CreateBottomRightShadow()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			int baseAlpha = 96;
			for (int i = 0; i < 4; i++)
			{
				for (int column = 3; column >= 0; column--)
				{
					int alpha = baseAlpha - baseAlpha / 4 * column;
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(column, i, color);
				}
				baseAlpha -= 24;
			}
			_bottomRightShadow = bitmap;
		}

		private void DrawBackground(Graphics graphics, Rectangle rect)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
			if (_isLayeredWindow)
			{
				rect.Width -= 4;
				rect.Height -= 4;
			}
			rect = ThemedToolTip.PaintBox(graphics, rect, Theme());
			if (!_backColor.IsEmpty)
			{
				using (Brush brush = new SolidBrush(_backColor))
				{
					graphics.FillRectangle(brush, rect);
				}
			}
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateWrapsText(_wrapsText);
			textLayoutRenderer.DrawText(graphics, _text, rect, GetFont(), GetTextColor());
			rect.Inflate(1, 1);
			if (_isLayeredWindow)
			{
				DrawImage(graphics, rect);
			}
		}

		private void DrawImage(Graphics graphics, Rectangle rectangle)
		{
			Color color = Color.FromArgb(96, Color.Black);
			Color.FromArgb(16, Color.Black);
			Color transparentColor = Color.FromArgb(0, Color.Black);
			if (rectangle.Height >= 4)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Right, rectangle.Top + 4, 4, 4), image: GetTopRightShadowImage());
				if (rectangle.Height >= 8)
				{
					Rectangle rect = new Rectangle(rectangle.Right, rectangle.Top + 8, 4, rectangle.Height - 8);
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, transparentColor, 0f);
					graphics.FillRectangle(linearGradientBrush, rect);
					linearGradientBrush.Dispose();
				}
			}
			if (rectangle.Width >= 4)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Left + 4, rectangle.Bottom, 4, 4), image: GetBottomLeftShadowImage());
				Rectangle rect = new Rectangle(rectangle.Left + 8, rectangle.Bottom, rectangle.Width - 8, 4);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, transparentColor, 90f);
				graphics.FillRectangle(linearGradientBrush, rect);
				linearGradientBrush.Dispose();
			}
			graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Right, rectangle.Bottom, 4, 4), image: GetBottomRightShadowImage());
		}

		private Size MeasureWindowSize()
		{
			_windowSize = _requestedSize;
			if (_windowSize.Width == 0)
			{
				IntPtr intPtr = NativeMethods.GetDC(IntPtr.Zero);
				Graphics graphics = SchedulerRenderer.CreateGraphicsFromHdc(intPtr);
				graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
				textLayoutRenderer.UpdateWrapsText(_wrapsText);
				_windowSize = textLayoutRenderer.RenderContent(graphics, _text, GetFont(), 0).ToSize();
				_windowSize.Height++;
				_windowSize.Width++;
				graphics.Dispose();
				NativeMethods.ReleaseDC(IntPtr.Zero, intPtr);
			}
			else if (_windowSize.Height == 0)
			{
				IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
				Graphics measureGraphics = SchedulerRenderer.CreateGraphicsFromHdc(screenDc);
				TextFormatFlags flags = TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak;
				_windowSize.Height = TextRenderer.MeasureText(measureGraphics, _text, GetFont(), new Size(_windowSize.Width, 0), flags).Height + 1;
				measureGraphics.Dispose();
				NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			}
			_windowSize.Width += 6;
			_windowSize.Height += 8;
			return _windowSize;
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

		private void ClampPositionToWorkingArea()
		{
			int left = _screenPosition.X;
			int top = _screenPosition.Y;
			Rectangle workingArea = Screen.GetWorkingArea(new Point(left, top));
			if (left + _windowSize.Width > workingArea.Right)
			{
				left = workingArea.Right - _windowSize.Width;
			}
			if (left < workingArea.X)
			{
				left = workingArea.X;
			}
			if (top + _windowSize.Height > workingArea.Bottom)
			{
				top -= _windowSize.Height;
			}
			_screenPosition.X = left;
			_screenPosition.Y = top;
		}

		private static bool CanUseLayeredWindows()
		{
			try
			{
				if (!OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
				{
					return false;
				}
				IntPtr intPtr = NativeMethods.GetDC(IntPtr.Zero);
				int bitsPerPixel = NativeMethods.GetDeviceCaps(intPtr, 12).ToInt32();
				NativeMethods.ReleaseDC(IntPtr.Zero, intPtr);
				return bitsPerPixel >= 16;
			}
			catch
			{
				return false;
			}
		}

		private void CreateWindowHandle()
		{
			_isLayeredWindow = CanUseLayeredWindows();
			Size size = MeasureWindowSize();
			ClampPositionToWorkingArea();
			Point point = _screenPosition;
			CreateParams createParams = new CreateParams();
			createParams.X = point.X;
			createParams.Y = point.Y;
			createParams.Style = int.MinValue;
			createParams.Height = size.Height;
			createParams.Width = size.Width;
			createParams.Parent = IntPtr.Zero;
			createParams.ExStyle = 136;
			if (_isLayeredWindow)
			{
				createParams.ExStyle += 524288;
			}
			CreateHandle(createParams);
			NativeMethods.ShowWindow(base.Handle, 4);
			if (_isLayeredWindow && _shouldFadeIn && _fadeTimer != null)
			{
				_fadeTimer.Start();
			}
		}

		public int GetAutoHideDelay()
		{
			return _autoHideDelay;
		}

		public void SetAutoHideDelay(int schedulerWindowMessageHandlerInteger)
		{
			_autoHideDelay = schedulerWindowMessageHandlerInteger;
		}

		public void ShowToolTip(string text, Point point, Control control)
		{
			_isActive = true;
			_delayTimer.Stop();
			_control = control;
			_text = text;
			_screenPosition = point;
			if (_initialDelay > 0)
			{
				_delayTimer.Interval = _initialDelay;
				_delayTimer.Start();
			}
			else
			{
				ShowAndStartHideTimer();
			}
		}

		private void ShowToolTipWindow()
		{
			CreateWindowHandle();
			NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, 32512u));
			_isWindowCreated = true;
		}

		internal void DisposeResources()
		{
			_isActive = false;
			if (_delayTimer != null)
			{
				_delayTimer.Stop();
			}
			_alpha = 20;
			_isHideTimerStarted = false;
			if (_fadeTimer != null)
			{
				try
				{
					_fadeTimer.Stop();
					_fadeTimer.Dispose();
					_fadeTimer = null;
				}
				catch
				{
				}
			}
			if (_isWindowCreated)
			{
				NativeMethods.ShowWindow(base.Handle, 0);
				DestroyHandle();
			}
			if (_filterRemovalTimer == null)
			{
				_filterRemovalTimer = new Timer();
				_filterRemovalTimer.Tick += OnTimerTick;
				_filterRemovalTimer.Interval = 10;
				_filterRemovalTimer.Start();
			}
		}

		internal void DisposeAll()
		{
			_isActive = false;
			if (_delayTimer != null)
			{
				_delayTimer.Stop();
				if (_delayTimer != null)
				{
					_delayTimer.Dispose();
					_delayTimer = null;
				}
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
			_alpha = 20;
			_isHideTimerStarted = false;
			Application.RemoveMessageFilter(this);
			if (!_isWindowCreated)
			{
				return;
			}
			NativeMethods.ShowWindow(base.Handle, 0);
			DestroyHandle();
			if (_filterRemovalTimer != null)
			{
				try
				{
					_filterRemovalTimer.Stop();
					_filterRemovalTimer.Dispose();
					_filterRemovalTimer = null;
				}
				catch
				{
				}
			}
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			Application.RemoveMessageFilter(this);
			if (_filterRemovalTimer != null)
			{
				try
				{
					Timer timer = _filterRemovalTimer;
					_filterRemovalTimer = null;
					timer.Stop();
					timer.Dispose();
				}
				catch
				{
				}
			}
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case 6:
			{
				if (_control == null)
				{
					break;
				}
				try
				{
					if (_control.TopLevelControl != null)
					{
						if (message.HWnd == _control.TopLevelControl.Handle)
						{
							DisposeResources();
						}
					}
					else if (_control.Handle == message.HWnd)
					{
						DisposeResources();
					}
				}
				finally
				{
				}
				break;
			}
			case 161:
			case 164:
			case 167:
			case 513:
			case 516:
			case 519:
				DisposeResources();
				break;
			}
			return false;
		}

		protected override void WndProc(ref Message message)
		{
			switch (message.Msg)
			{
			case 28:
			case 161:
			case 164:
			case 167:
			case 513:
			case 516:
			case 519:
			case 1025:
				DisposeResources();
				break;
			case 33:
				message.Result = (IntPtr)3;
				break;
			case 15:
				PaintToolTipWindow(message);
				break;
			case 32:
				NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, 32512u));
				break;
			case 132:
				message.Result = new IntPtr(-1);
				break;
			default:
				base.WndProc(ref message);
				break;
			}
		}

		private void RenderLayeredWindow(Point point)
		{
			using Bitmap bitmap = new Bitmap(_windowSize.Width, _windowSize.Height, PixelFormat.Format32bppArgb);
			using Graphics graphics = Graphics.FromImage(bitmap);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Rectangle rectangle = new Rectangle(0, 0, _windowSize.Width, _windowSize.Height);
			DrawBackground(graphics, rectangle);
			IntPtr intPtr = NativeMethods.GetDC(IntPtr.Zero);
			IntPtr memoryDc = NativeMethods.CreateCompatibleDC(intPtr);
			IntPtr hbitmap = bitmap.GetHbitmap(Color.FromArgb(0));
			IntPtr previousBitmap = NativeMethods.SelectObject(memoryDc, hbitmap);
			try
			{
				NativeMethods.NativePoint windowPosition = default(NativeMethods.NativePoint);
				windowPosition.X = point.X;
				windowPosition.Y = point.Y;
				NativeMethods.NativePoint sourcePosition = default(NativeMethods.NativePoint);
				sourcePosition.X = 0;
				sourcePosition.Y = 0;
				NativeMethods.NativeSize c = default(NativeMethods.NativeSize);
				c.Width = _windowSize.Width;
				c.Height = _windowSize.Height;
				NativeMethods.NativeBlendFunction a = new NativeMethods.NativeBlendFunction
				{
					_byteValue = 0,
					SourceConstantAlpha = (byte)_alpha,
					_nativeMethodsDataByte = 0,
					AlphaFormat = 1
				};
				NativeMethods.UpdateLayeredWindow(base.Handle, intPtr, ref windowPosition, ref c, memoryDc, ref sourcePosition, 0, ref a, 2);
			}
			finally
			{
				NativeMethods.SelectObject(memoryDc, previousBitmap);
				NativeMethods.ReleaseDC(IntPtr.Zero, intPtr);
				NativeMethods.DeleteObject(hbitmap);
				NativeMethods.DeleteDC(memoryDc);
			}
		}

		protected void PaintToolTipWindow(Message message)
		{
			NativeMethods.NativePaintStruct paintStruct = default(NativeMethods.NativePaintStruct);
			IntPtr intPtr = NativeMethods.BeginPaint(message.HWnd, ref paintStruct);
			try
			{
				NativeMethods.NativeWindowRect e = default(NativeMethods.NativeWindowRect);
				NativeMethods.GetWindowRect(message.HWnd, ref e);
				Rectangle rectangle = new Rectangle(0, 0, e.Right - e.Left, e.Bottom - e.Top);
				using Graphics graphics = SchedulerRenderer.CreateGraphicsFromHdc(intPtr);
				using Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
				using Graphics bufferGraphics = Graphics.FromImage(bitmap);
				bufferGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				DrawBackground(bufferGraphics, rectangle);
				graphics.DrawImageUnscaled(bitmap, 0, 0);
			}
			finally
			{
				NativeMethods.EndPaint(message.HWnd, ref paintStruct);
			}
		}

		private void CreateTimers()
		{
			_delayTimer = new Timer();
			_delayTimer.Tick += OnDelayTimerTick;
			_fadeTimer = new Timer();
			_fadeTimer.Interval = 70;
			_fadeTimer.Tick += OnFadeTimerTick;
		}

		private void OnFadeTimerTick(object sender, EventArgs eventArgs)
		{
			if (_alpha + 80 < 255)
			{
				_alpha += 80;
			}
			else
			{
				_alpha = 255;
			}
			RenderLayeredWindow(_screenPosition);
			if (_alpha >= 255)
			{
				_fadeTimer.Stop();
			}
		}

		private void OnDelayTimerTick(object sender, EventArgs eventArgs)
		{
			if (_delayTimer != null)
			{
				_delayTimer.Stop();
			}
			ShowAndStartHideTimer();
		}

		private void ShowAndStartHideTimer()
		{
			if (!_isHideTimerStarted)
			{
				ShowToolTipWindow();
				if (GetAutoHideDelay() > 0)
				{
					_delayTimer.Interval = GetAutoHideDelay();
					_isHideTimerStarted = true;
					_delayTimer.Start();
				}
			}
			else
			{
				DisposeResources();
			}
		}
	}
}
