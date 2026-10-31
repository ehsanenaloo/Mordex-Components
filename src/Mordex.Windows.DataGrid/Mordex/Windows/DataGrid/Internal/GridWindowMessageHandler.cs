using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GridWindowMessageHandler : NativeWindow
	{
		internal Rectangle _anchorBounds;

		private Control _ownerControl;

		private bool _useCompatibleTextRendering;

		private bool _wordWrap;

		private string _tooltipText = "";

		internal MxDataGrid _owner;

		private Size _requestedSize = Size.Empty;

		private Size _gridWindowMessageHandlerSize = Size.Empty;

		private bool _isLayeredWindowSupported;

		private bool _canFadeIn = true;

		private bool _isWindowShown;

		private GridMessageFilter _gridMessageFilter;

		private Point _screenPosition = Point.Empty;

		private int _showDelayMs = 500;

		private int _displayDuration = 5000;

		private Timer _timer;

		internal Point _fixedPosition = Point.Empty;

		private int _fadeAlpha = 20;

		private Timer _gridWindowMessageHandlerTimer;

		private Timer _fadeTimer;

		private bool _isDisplayTimerActive;

		private Bitmap _shadowTopLeft;

		private Bitmap _shadowTopRight;

		private Bitmap _shadowBottomRight;

		internal GridWindowMessageHandler(Control control, bool useCompatibleTextRendering)
		{
			_useCompatibleTextRendering = useCompatibleTextRendering;
			_ownerControl = control;
			InitializeComponent();
		}

		internal void SetMultiline(bool condition)
		{
			_wordWrap = condition;
		}

		internal void SetTooltipText(string text)
		{
			if (_tooltipText != text)
			{
				DestroyWindow();
				_tooltipText = text;
				if (_tooltipText != null && _tooltipText.Length > 0)
				{
					ShowCurrentText();
				}
			}
		}

		public void SetTooltipSize(Size size)
		{
			if (_requestedSize != size)
			{
				_requestedSize = size;
			}
		}

		private void DrawBackground(Graphics graphics, Rectangle rectangle)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
			textLayoutRenderer._rightToLeft = _ownerControl.RightToLeft;
			if (_isLayeredWindowSupported)
			{
				rectangle.Width -= 4;
				rectangle.Height -= 4;
			}
			ResolvedTheme theme = MxThemeManager.ResolveDefault(_ownerControl == null || _ownerControl.IsDisposed ? 96 : _ownerControl.DeviceDpi);
			rectangle = ThemedToolTip.PaintBox(graphics, rectangle, theme);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.SetWordWrap(_wordWrap);
			Brush solidBrush = theme.Brush(ThemeRole.TooltipText);
			if (!GridHelper.IsTextLengthValid(_tooltipText))
			{
				textLayoutRenderer.DrawTextWithBrush(graphics, GridHelper.TruncateText(_tooltipText), rectangle, GetOwnerFont(), solidBrush);
			}
			else
			{
				textLayoutRenderer.DrawTextWithBrush(graphics, _tooltipText, rectangle, GetOwnerFont(), solidBrush);
			}
			textLayoutRenderer.ReleaseStringFormat();
			rectangle.Inflate(1, 1);
			if (_isLayeredWindowSupported)
			{
				DrawShadow(graphics, rectangle);
			}
		}

		private Size MeasureSize()
		{
			try
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
				textLayoutRenderer.SetWordWrap(_wordWrap);
				_gridWindowMessageHandlerSize = _requestedSize;
				if (_gridWindowMessageHandlerSize.Width == 0)
				{
					IntPtr intPtr = NativeMethods.GetDC(IntPtr.Zero);
					Graphics graphics = Graphics.FromHdc(intPtr);
					if (!GridHelper.IsTextLengthValid(_tooltipText))
					{
						_gridWindowMessageHandlerSize = textLayoutRenderer.MeasureText(graphics, GridHelper.TruncateText(_tooltipText), GetOwnerFont(), 0).ToSize();
					}
					else
					{
						_gridWindowMessageHandlerSize = textLayoutRenderer.MeasureText(graphics, _tooltipText, GetOwnerFont(), 0).ToSize();
					}
					_gridWindowMessageHandlerSize.Height++;
					_gridWindowMessageHandlerSize.Width++;
					graphics.Dispose();
					NativeMethods.ReleaseDC(IntPtr.Zero, intPtr);
				}
				else if (_gridWindowMessageHandlerSize.Height == 0)
				{
					IntPtr getDC = NativeMethods.GetDC(IntPtr.Zero);
					Graphics otherGraphics = Graphics.FromHdc(getDC);
					_gridWindowMessageHandlerSize.Height = (int)textLayoutRenderer.MeasureText(otherGraphics, _tooltipText, GetOwnerFont(), _gridWindowMessageHandlerSize.Width).Height + 1;
					otherGraphics.Dispose();
					NativeMethods.ReleaseDC(IntPtr.Zero, getDC);
				}
				_gridWindowMessageHandlerSize.Width += 6;
				_gridWindowMessageHandlerSize.Height += 8;
				Rectangle virtualScreen = SystemInformation.VirtualScreen;
				if (_gridWindowMessageHandlerSize.Width > virtualScreen.Width)
				{
					_gridWindowMessageHandlerSize.Width = virtualScreen.Width;
				}
			}
			finally
			{
			}
			return _gridWindowMessageHandlerSize;
		}

		public Font GetOwnerFont()
		{
			return _ownerControl.Font;
		}

		private void KeepOnScreen()
		{
			int value = _screenPosition.X;
			int otherValue = _screenPosition.Y;
			Rectangle virtualScreen = SystemInformation.VirtualScreen;
			if (value + _gridWindowMessageHandlerSize.Width > virtualScreen.Right)
			{
				value = virtualScreen.Right - _gridWindowMessageHandlerSize.Width;
			}
			if (value < virtualScreen.X)
			{
				value = virtualScreen.X;
			}
			if (otherValue + _gridWindowMessageHandlerSize.Height > virtualScreen.Bottom)
			{
				otherValue -= _gridWindowMessageHandlerSize.Height;
			}
			_screenPosition.X = value;
			_screenPosition.Y = otherValue;
		}

		private void CreateAndShowWindow()
		{
			try
			{
				if (!_fixedPosition.IsEmpty)
				{
					_screenPosition = _fixedPosition;
				}
				else if (_screenPosition.IsEmpty)
				{
					_screenPosition = PointerPosition.Current;
					_screenPosition.Y += 20;
				}
				_isLayeredWindowSupported = OSFeature.Feature.IsPresent(OSFeature.LayeredWindows);
				Size size = MeasureSize();
				KeepOnScreen();
				Point point = _screenPosition;
				CreateParams createParams = new CreateParams();
				createParams.X = point.X;
				createParams.Y = point.Y;
				createParams.Height = size.Height;
				createParams.Width = size.Width;
				createParams.Parent = IntPtr.Zero;
				createParams.Style = int.MinValue;
				createParams.ExStyle = 136;
				if (_isLayeredWindowSupported)
				{
					createParams.ExStyle |= 524288;
				}
				CreateHandle(createParams);
				NativeMethods.ShowWindow(base.Handle, 4);
				if (_isLayeredWindowSupported && _canFadeIn)
				{
					EnsureFadeTimer();
					_fadeTimer.Start();
				}
			}
			finally
			{
			}
		}

		public void ShowCurrentText()
		{
			ShowText(_tooltipText);
		}

		public void ShowText(string text)
		{
			ShowTextAt(text, Point.Empty);
		}

		public int GetDisplayDuration()
		{
			return _displayDuration;
		}

		public void ShowTextAt(string text, Point point)
		{
			if (_gridWindowMessageHandlerTimer != null)
			{
				DisposeDelayTimer();
			}
			_tooltipText = text;
			_screenPosition = point;
			if (_showDelayMs > 0)
			{
				EnsureDelayTimer();
				_gridWindowMessageHandlerTimer.Interval = _showDelayMs;
				_gridWindowMessageHandlerTimer.Start();
			}
			else
			{
				ShowOrHide();
			}
		}

		private void TryShowWindow()
		{
			if (_tooltipText == null || _tooltipText.Length == 0)
			{
				return;
			}
			if (_ownerControl != null)
			{
				Control control = null;
				try
				{
					control = _ownerControl.TopLevelControl;
				}
				finally
				{
				}
				if (control != null && !control.ContainsFocus)
				{
					return;
				}
			}
			_gridMessageFilter = new GridMessageFilter(this);
			try
			{
				Application.AddMessageFilter(_gridMessageFilter);
			}
			finally
			{
			}
			CreateAndShowWindow();
			NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, 32512u));
			_isWindowShown = true;
		}

		internal void DisposeResources()
		{
			DisposeDelayTimer();
			_fadeAlpha = 20;
			_isDisplayTimerActive = false;
			if (_fadeTimer != null)
			{
				_fadeTimer.Stop();
				_fadeTimer.Dispose();
				_fadeTimer.Tick -= OnFadeTimerTick;
				_fadeTimer = null;
			}
			if (_isWindowShown)
			{
				NativeMethods.ShowWindow(base.Handle, 0);
				DestroyHandle();
				_timer = new Timer();
				_timer.Tick += OnTimerTick;
				_timer.Interval = Timing.PointerPollInterval;
				_timer.Start();
				_isWindowShown = false;
			}
		}

		internal void DestroyWindow()
		{
			if (_fadeTimer != null)
			{
				_fadeTimer.Stop();
				_fadeTimer.Dispose();
				_fadeTimer.Tick -= OnFadeTimerTick;
				_fadeTimer = null;
			}
			_fadeAlpha = 20;
			_isDisplayTimerActive = false;
			if (_isWindowShown)
			{
				NativeMethods.ShowWindow(base.Handle, 0);
				DestroyHandle();
				Application.RemoveMessageFilter(_gridMessageFilter);
				_gridMessageFilter = null;
				if (_timer != null)
				{
					_timer.Stop();
					_timer.Tick -= OnTimerTick;
					_timer.Dispose();
					_timer = null;
				}
				_isWindowShown = false;
			}
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			Application.RemoveMessageFilter(_gridMessageFilter);
			_gridMessageFilter = null;
			_timer.Stop();
			_timer.Dispose();
			_timer.Tick -= OnTimerTick;
			_timer = null;
		}

		protected override void WndProc(ref Message message)
		{
			switch (message.Msg)
			{
			case WindowMessages.NonClientHitTest:
				message.Result = new IntPtr(-1);
				break;
			case WindowMessages.NonClientLeftButtonDown:
			case WindowMessages.NonClientRightButtonDown:
			case WindowMessages.NonClientMiddleButtonDown:
			case WindowMessages.LeftButtonDown:
			case WindowMessages.RightButtonDown:
			case WindowMessages.MiddleButtonDown:
				DisposeResources();
				break;
			case 28:
				DestroyWindow();
				break;
			case 33:
				message.Result = (IntPtr)3;
				break;
			case 15:
				ProcessWindowMessage(message);
				break;
			case 32:
				NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, 32512u));
				break;
			default:
				base.WndProc(ref message);
				break;
			}
		}

		private void UpdateLayeredWindow(Point point)
		{
			Bitmap bitmap = new Bitmap(_gridWindowMessageHandlerSize.Width, _gridWindowMessageHandlerSize.Height, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			_ = _useCompatibleTextRendering;
			Rectangle rectangle = new Rectangle(0, 0, _gridWindowMessageHandlerSize.Width, _gridWindowMessageHandlerSize.Height);
			DrawBackground(graphics, rectangle);
			IntPtr intPtr = NativeMethods.GetDC(IntPtr.Zero);
			IntPtr compatibleDC = NativeMethods.CreateCompatibleDC(intPtr);
			IntPtr hbitmap = bitmap.GetHbitmap(Color.FromArgb(0));
			IntPtr selectObject = NativeMethods.SelectObject(compatibleDC, hbitmap);
			NativeMethods.NativePoint c = default(NativeMethods.NativePoint);
			c._x = _gridWindowMessageHandlerSize.Width;
			c._nativeMethodsData2Integer = _gridWindowMessageHandlerSize.Height;
			NativeMethods.NativeSize otherNativeMethodsData = default(NativeMethods.NativeSize);
			otherNativeMethodsData._width = point.X;
			otherNativeMethodsData._nativeMethodsData3Integer = point.Y;
			NativeMethods.NativeSize nativeMethodsData3Argument = default(NativeMethods.NativeSize);
			nativeMethodsData3Argument._width = 0;
			nativeMethodsData3Argument._nativeMethodsData3Integer = 0;
			NativeMethods.NativeMethodsData b = new NativeMethods.NativeMethodsData
			{
				_byteValue = 0,
				_nativeMethodsDataByte = 0,
				_sourceConstantAlpha = (byte)_fadeAlpha,
				_alphaFormat = 1
			};
			NativeMethods.UpdateLayeredWindow(base.Handle, intPtr, ref otherNativeMethodsData, ref c, compatibleDC, ref nativeMethodsData3Argument, 0, ref b, 2);
			NativeMethods.SelectObject(compatibleDC, selectObject);
			NativeMethods.ReleaseDC(IntPtr.Zero, intPtr);
			NativeMethods.DeleteObject(hbitmap);
			NativeMethods.DeleteDC(compatibleDC);
			graphics.Dispose();
			bitmap.Dispose();
		}

		private void ProcessWindowMessage(Message message)
		{
			try
			{
				NativeMethods.NativePaintStruct nativeMethodsData = default(NativeMethods.NativePaintStruct);
				IntPtr hdc = NativeMethods.BeginPaint(message.HWnd, ref nativeMethodsData);
				NativeMethods.NativeRectStruct e = default(NativeMethods.NativeRectStruct);
				NativeMethods.GetWindowRectStruct(message.HWnd, ref e);
				Rectangle rectangle = new Rectangle(0, 0, e._right - e._left, e._bottom - e._nativeMethodsData4Integer);
				Graphics graphics = Graphics.FromHdc(hdc);
				Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
				Graphics otherGraphics = Graphics.FromImage(bitmap);
				otherGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				DrawBackground(otherGraphics, rectangle);
				graphics.DrawImageUnscaled(bitmap, 0, 0);
				otherGraphics.Dispose();
				bitmap.Dispose();
				graphics.Dispose();
				NativeMethods.EndPaint(message.HWnd, ref nativeMethodsData);
			}
			finally
			{
			}
		}

		private void InitializeComponent()
		{
		}

		private void EnsureDelayTimer()
		{
			if (_gridWindowMessageHandlerTimer == null)
			{
				_gridWindowMessageHandlerTimer = new Timer();
				_gridWindowMessageHandlerTimer.Tick += OnDelayTimerTick;
			}
		}

		private void DisposeDelayTimer()
		{
			if (_gridWindowMessageHandlerTimer != null)
			{
				_gridWindowMessageHandlerTimer.Stop();
				_gridWindowMessageHandlerTimer.Tick -= OnDelayTimerTick;
				_gridWindowMessageHandlerTimer.Dispose();
				_gridWindowMessageHandlerTimer = null;
			}
		}

		private void EnsureFadeTimer()
		{
			if (_fadeTimer == null)
			{
				_fadeTimer = new Timer();
				_fadeTimer.Interval = Timing.FadeStepInterval;
				_fadeTimer.Tick += OnFadeTimerTick;
			}
		}

		private void OnFadeTimerTick(object sender, EventArgs eventArgs)
		{
			if (_fadeAlpha + 80 < 255)
			{
				_fadeAlpha += 80;
			}
			else
			{
				_fadeAlpha = 255;
			}
			UpdateLayeredWindow(_screenPosition);
			if (_fadeAlpha >= 255)
			{
				_fadeTimer.Stop();
			}
		}

		private void OnDelayTimerTick(object sender, EventArgs eventArgs)
		{
			DisposeDelayTimer();
			if (!_anchorBounds.IsEmpty && !_anchorBounds.Contains(PointerPosition.Current))
			{
				return;
			}
			if (_owner != null)
			{
				string text = _owner.GetCellToolTipText();
				_tooltipText = text;
				if (string.IsNullOrEmpty(_tooltipText))
				{
					return;
				}
			}
			ShowOrHide();
		}

		private void ShowOrHide()
		{
			try
			{
				if (!_isDisplayTimerActive)
				{
					TryShowWindow();
					EnsureDelayTimer();
					_gridWindowMessageHandlerTimer.Interval = GetDisplayDuration();
					_isDisplayTimerActive = true;
					_gridWindowMessageHandlerTimer.Start();
				}
				else
				{
					DisposeResources();
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private Image GetShadowTopLeftImage()
		{
			if (_shadowTopLeft == null)
			{
				CreateShadowTopLeftBitmap();
			}
			return _shadowTopLeft;
		}

		private Image GetShadowTopRightImage()
		{
			if (_shadowTopRight == null)
			{
				CreateShadowTopRightBitmap();
			}
			return _shadowTopRight;
		}

		private Image GetShadowBottomRightImage()
		{
			if (_shadowBottomRight == null)
			{
				CreateShadowBottomRightBitmap();
			}
			return _shadowBottomRight;
		}

		private void CreateShadowTopLeftBitmap()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			int value = 16;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int alpha = value - value / 4 * j;
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(i, j, color);
				}
				value += 32;
			}
			_shadowTopLeft = bitmap;
		}

		private void CreateShadowTopRightBitmap()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			int value = 16;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 3; j >= 0; j--)
				{
					int alpha = value - value / 4 * j;
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(j, i, color);
				}
				value += 32;
			}
			_shadowTopRight = bitmap;
		}

		private void CreateShadowBottomRightBitmap()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			int value = 112;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 3; j >= 0; j--)
				{
					int alpha = value - value / 4 * j;
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(j, i, color);
				}
				value -= 32;
			}
			_shadowBottomRight = bitmap;
		}

		private void DrawShadow(Graphics graphics, Rectangle rectangle)
		{
			Color color = Color.FromArgb(128, Color.Black);
			Color.FromArgb(24, Color.Black);
			Color otherColor = Color.FromArgb(0, Color.Black);
			if (rectangle.Height >= 4)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Right, rectangle.Top + 4, 4, 4), image: GetShadowTopRightImage());
				if (rectangle.Height >= 8)
				{
					Rectangle otherRectangle = new Rectangle(rectangle.Right, rectangle.Top + 8, 4, rectangle.Height - 8);
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(otherRectangle, color, otherColor, 0f);
					graphics.FillRectangle(linearGradientBrush, otherRectangle);
					linearGradientBrush.Dispose();
				}
			}
			if (rectangle.Width >= 4)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Left + 4, rectangle.Bottom, 4, 4), image: GetShadowTopLeftImage());
				Rectangle nextRectangle = new Rectangle(rectangle.Left + 8, rectangle.Bottom, rectangle.Width - 8, 4);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(nextRectangle, color, otherColor, 90f);
				graphics.FillRectangle(linearGradientBrush, nextRectangle);
				linearGradientBrush.Dispose();
			}
			graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Right, rectangle.Bottom, 4, 4), image: GetShadowBottomRightImage());
		}
	}
}
