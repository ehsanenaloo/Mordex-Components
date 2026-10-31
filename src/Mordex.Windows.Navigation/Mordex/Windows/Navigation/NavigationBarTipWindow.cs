using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Navigation
{
	internal class NavigationBarTipWindow : NativeWindow, IMessageFilter
	{
		private bool _useCompatibleTextRendering;

		private Color _textColor;

		private Color _backColor;

		private Bitmap _rightShadowBitmap;

		private Bitmap _bottomShadowBitmap;

		private Bitmap _cornerShadowBitmap;

		public bool _isWrapEnabled = true;

		private Size _tipSize = Size.Empty;

		private Font _tipFont;

		public string _tipText = "";

		private bool _useLayeredWindow;

		private bool _canFadeIn = true;

		private bool _isWindowCreated;

		private Point _tipLocation = Point.Empty;

		internal Control _control;

		private const int WsPopup = unchecked((int)0x80000000);

		private const int WsExToolWindow = 0x80;

		private const int WsExTopmost = 0x8;

		private const int WsExLayered = 0x80000;

		private const int SwHide = 0;

		private const int SwShowNoActivate = 4;

		private const int IdcArrow = 32512;

		private const int MaNoActivate = 3;

		private const int WmActivate = 0x6;

		private const int WmPaint = 0xF;

		private const int WmActivateApp = 0x1C;

		private const int WmSetCursor = 0x20;

		private const int WmMouseActivate = 0x21;

		private const int WmNcHitTest = 0x84;

		private const int WmNcLButtonDown = 0xA1;

		private const int WmNcRButtonDown = 0xA4;

		private const int WmNcMButtonDown = 0xA7;

		private const int WmLButtonDown = 0x201;

		private const int WmRButtonDown = 0x204;

		private const int WmMButtonDown = 0x207;

		private const int WmHideTip = 0x401;
		private int _showDelay = 500;

		private int _hideDelay = 5000;

		private bool _isShown;

		public bool _isActive;

		private int _alpha = 20;

		private Timer _showHideTimer;

		private Timer _fadeTimer;

		private bool _isWaitingToHide;

		internal NavigationBarTipWindow(Font font, bool useCompatibleTextRendering)
		{
			_tipFont = font;
			_useCompatibleTextRendering = useCompatibleTextRendering;
			try
			{
				_useCompatibleTextRendering = SystemInformation.FontSmoothingType == 2;
			}
			catch (System.ComponentModel.Win32Exception)
			{
				// Keep the caller's setting when the system setting cannot be read.
			}
			CreateTimers();
			Application.AddMessageFilter(this);
		}

		public Color GetTextColor()
		{
			if (_textColor.IsEmpty)
			{
				return SystemColors.InfoText;
			}
			return _textColor;
		}

		public void SetTextColor(Color color)
		{
			_textColor = color;
		}

		private Color _borderColor;

		private bool _hasShadow = true;

		internal void SetBorderColor(Color color, bool hasShadow)
		{
			_borderColor = color;
			_hasShadow = hasShadow;
		}

		public Color GetBackColor()
		{
			if (_backColor.IsEmpty)
			{
				return SystemColors.Info;
			}
			return _backColor;
		}

		public void SetBackColor(Color backColor)
		{
			_backColor = backColor;
		}

		private Image GetRightShadowImage()
		{
			if (_rightShadowBitmap == null)
			{
				CreateRightShadowBitmap();
			}
			return _rightShadowBitmap;
		}

		private Image GetBottomShadowImage()
		{
			if (_bottomShadowBitmap == null)
			{
				CreateBottomShadowBitmap();
			}
			return _bottomShadowBitmap;
		}

		private Image GetCornerShadowImage()
		{
			if (_cornerShadowBitmap == null)
			{
				CreateCornerShadowBitmap();
			}
			return _cornerShadowBitmap;
		}

		private void CreateBottomShadowBitmap()
		{
			Bitmap shadowBitmap = new Bitmap(4, 4);
			int alphaBase = 8;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int alpha = alphaBase - alphaBase / 4 * j;
					Color pixelColor = Color.FromArgb(alpha, Color.Black);
					shadowBitmap.SetPixel(i, j, pixelColor);
				}
				alphaBase += 24;
			}
			_bottomShadowBitmap = shadowBitmap;
		}

		private void CreateRightShadowBitmap()
		{
			Bitmap shadowBitmap = new Bitmap(4, 4);
			int alphaBase = 8;
			for (int i = 0; i < 4; i++)
			{
				for (int x = 3; x >= 0; x--)
				{
					int alpha = alphaBase - alphaBase / 4 * x;
					Color pixelColor = Color.FromArgb(alpha, Color.Black);
					shadowBitmap.SetPixel(x, i, pixelColor);
				}
				alphaBase += 24;
			}
			_rightShadowBitmap = shadowBitmap;
		}

		private void CreateCornerShadowBitmap()
		{
			Bitmap shadowBitmap = new Bitmap(4, 4);
			int alphaBase = 96;
			for (int i = 0; i < 4; i++)
			{
				for (int x = 3; x >= 0; x--)
				{
					int alpha = alphaBase - alphaBase / 4 * x;
					Color pixelColor = Color.FromArgb(alpha, Color.Black);
					shadowBitmap.SetPixel(x, i, pixelColor);
				}
				alphaBase -= 24;
			}
			_cornerShadowBitmap = shadowBitmap;
		}

		private void DrawTip(Graphics graphics, Rectangle tipBounds)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			if (_useLayeredWindow)
			{
				tipBounds.Width -= 4;
				tipBounds.Height -= 4;
			}
			Brush backBrush = GdiCache.Brush(GetBackColor());
			tipBounds = NavigationGraphicsHelper.DrawBorderRectangle(graphics, _borderColor.IsEmpty ? SystemPens.ControlDarkDark : GdiCache.Pen(_borderColor), tipBounds);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateWrapEnabled(_isWrapEnabled);
			graphics.FillRectangle(backBrush, tipBounds);
			textLayoutRenderer.DrawTextWithColor(graphics, _tipText, tipBounds, GetTipFont(), GetTextColor());
			tipBounds.Inflate(1, 1);
			textLayoutRenderer.InvalidateFormat();
			if (_useLayeredWindow)
			{
				DrawShadow(graphics, tipBounds);
			}
		}

		private void DrawShadow(Graphics graphics, Rectangle tipBounds)
		{
			Color shadowStartColor = Color.FromArgb(96, Color.Black);
			Color.FromArgb(16, Color.Black);
			Color shadowEndColor = Color.FromArgb(0, Color.Black);
			if (tipBounds.Height >= 4)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(tipBounds.Right, tipBounds.Top + 4, 4, 4), image: GetRightShadowImage());
				if (tipBounds.Height >= 8)
				{
					Rectangle rightShadowBounds = new Rectangle(tipBounds.Right, tipBounds.Top + 8, 4, tipBounds.Height - 8);
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rightShadowBounds, shadowStartColor, shadowEndColor, 0f);
					graphics.FillRectangle(linearGradientBrush, rightShadowBounds);
					linearGradientBrush.Dispose();
				}
			}
			if (tipBounds.Width >= 4)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(tipBounds.Left + 4, tipBounds.Bottom, 4, 4), image: GetBottomShadowImage());
				Rectangle bottomShadowBounds = new Rectangle(tipBounds.Left + 8, tipBounds.Bottom, tipBounds.Width - 8, 4);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(bottomShadowBounds, shadowStartColor, shadowEndColor, 90f);
				graphics.FillRectangle(linearGradientBrush, bottomShadowBounds);
				linearGradientBrush.Dispose();
			}
			graphics.DrawImageUnscaled(rect: new Rectangle(tipBounds.Right, tipBounds.Bottom, 4, 4), image: GetCornerShadowImage());
		}

		private Size MeasureTip()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.UpdateWrapEnabled(true);
			IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
			Graphics screenGraphics = NavigationGraphicsHelper.GraphicsFromHdc(screenDc);
			_tipSize = textLayoutRenderer.MeasureText(screenGraphics, _tipText, GetTipFont(), 10000).ToSize();
			_tipSize.Height++;
			_tipSize.Width++;
			screenGraphics.Dispose();
			NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			_tipSize.Width += 6;
			_tipSize.Height += 8;
			textLayoutRenderer.InvalidateFormat();
			return _tipSize;
		}

		public Font GetTipFont()
		{
			if (_tipFont == null)
			{
				return Control.DefaultFont;
			}
			return _tipFont;
		}

		public void SetTipFont(Font font)
		{
			_tipFont = font;
		}

		private void FitTipToWorkingArea()
		{
			int x = _tipLocation.X;
			int y = _tipLocation.Y;
			Rectangle workingArea = Screen.GetWorkingArea(new Point(x, y));
			if (x + _tipSize.Width > workingArea.Right)
			{
				x = workingArea.Right - _tipSize.Width;
			}
			if (x < workingArea.X)
			{
				x = workingArea.X;
			}
			if (y + _tipSize.Height > workingArea.Bottom)
			{
				y -= _tipSize.Height;
			}
			_tipLocation.X = x;
			_tipLocation.Y = y;
		}

		private void CreateTipWindow()
		{
			_isWindowCreated = true;
			_useLayeredWindow = _hasShadow && OSFeature.Feature.IsPresent(OSFeature.LayeredWindows);
			Size tipSize = MeasureTip();
			FitTipToWorkingArea();
			Point location = _tipLocation;
			CreateParams createParams = new CreateParams();
			createParams.X = location.X;
			createParams.Y = location.Y;
			createParams.Height = tipSize.Height;
			createParams.Width = tipSize.Width;
			createParams.Parent = IntPtr.Zero;
			createParams.Style = WsPopup;
			createParams.ExStyle = WsExToolWindow | WsExTopmost;
			if (_useLayeredWindow)
			{
				createParams.ExStyle |= WsExLayered;
			}
			CreateHandle(createParams);
			NativeMethods.ShowWindow(base.Handle, SwShowNoActivate);
			if (_useLayeredWindow && _canFadeIn && _fadeTimer != null)
			{
				_fadeTimer.Start();
			}
		}

		public void ShowTipAtCursor(string text, Control control)
		{
			Point position = Cursor.Position;
			position.Y += 16;
			ShowTip(text, position, control);
		}

		public int GetHideDelay()
		{
			return _hideDelay;
		}

		public void SetHideDelay(int hideDelay)
		{
			_hideDelay = hideDelay;
		}

		public void ShowTip(string text, Point location, Control control)
		{
			_isActive = true;
			_showHideTimer.Stop();
			_control = control;
			_tipText = text;
			_tipLocation = location;
			if (_showDelay > 0)
			{
				_showHideTimer.Interval = _showDelay;
				_showHideTimer.Start();
			}
			else
			{
				ShowOrHideTip();
			}
		}

		private void ShowTipWindow()
		{
			if (!_isWindowCreated)
			{
				CreateTipWindow();
			}
			NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, new IntPtr(IdcArrow)));
			_isShown = true;
		}

		internal void HideTip()
		{
			_isActive = false;
			if (_showHideTimer != null)
			{
				_showHideTimer.Stop();
			}
			_alpha = 20;
			_isWaitingToHide = false;
			_fadeTimer?.Stop();
			if (_isShown)
			{
				NativeMethods.ShowWindow(base.Handle, SwHide);
				DestroyHandle();
			}
		}

		internal void DisposeTip()
		{
			_isActive = false;
			_showHideTimer?.Dispose();
			_showHideTimer = null;
			_fadeTimer?.Dispose();
			_fadeTimer = null;
			_alpha = 20;
			_isWaitingToHide = false;
			Application.RemoveMessageFilter(this);
			_rightShadowBitmap?.Dispose();
			_rightShadowBitmap = null;
			_bottomShadowBitmap?.Dispose();
			_bottomShadowBitmap = null;
			_cornerShadowBitmap?.Dispose();
			_cornerShadowBitmap = null;
			if (_isShown)
			{
				NativeMethods.ShowWindow(base.Handle, SwHide);
				DestroyHandle();
			}
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case WmActivate:
			{
				if (_control == null)
				{
					break;
				}
				if (_control.TopLevelControl != null)
				{
					if (message.HWnd == _control.TopLevelControl.Handle)
					{
						HideTip();
					}
				}
				else if (_control.Handle == message.HWnd)
				{
					HideTip();
				}
				break;
			}
			case WmNcLButtonDown:
			case WmNcRButtonDown:
			case WmNcMButtonDown:
			case WmLButtonDown:
			case WmRButtonDown:
			case WmMButtonDown:
				HideTip();
				break;
			}
			return false;
		}

		protected override void WndProc(ref Message message)
		{
			switch (message.Msg)
			{
			case WmActivateApp:
			case WmNcLButtonDown:
			case WmNcRButtonDown:
			case WmNcMButtonDown:
			case WmLButtonDown:
			case WmRButtonDown:
			case WmMButtonDown:
			case WmHideTip:
				HideTip();
				break;
			case WmMouseActivate:
				message.Result = (IntPtr)MaNoActivate;
				break;
			case WmPaint:
				PaintTipWindow(message);
				break;
			case WmSetCursor:
				NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, new IntPtr(IdcArrow)));
				break;
			case WmNcHitTest:
				message.Result = new IntPtr(-1);
				break;
			default:
				base.WndProc(ref message);
				break;
			}
		}

		private void UpdateLayeredTip(Point location)
		{
			Bitmap tipBitmap = new Bitmap(_tipSize.Width, _tipSize.Height, PixelFormat.Format32bppArgb);
			Graphics tipGraphics = Graphics.FromImage(tipBitmap);
			if (!_useCompatibleTextRendering)
			{
				tipGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			Rectangle tipBounds = new Rectangle(0, 0, _tipSize.Width, _tipSize.Height);
			DrawTip(tipGraphics, tipBounds);
			IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
			IntPtr memoryDc = NativeMethods.CreateCompatibleDC(screenDc);
			IntPtr hbitmap = tipBitmap.GetHbitmap(Color.FromArgb(0));
			IntPtr previousObject = NativeMethods.SelectObject(memoryDc, hbitmap);
			NativeMethods.NativePoint destination = default(NativeMethods.NativePoint);
			destination._x = location.X;
			destination._y = location.Y;
			NativeMethods.NativePoint source = default(NativeMethods.NativePoint);
			source._x = 0;
			source._y = 0;
			NativeMethods.NativeSize layerSize = default(NativeMethods.NativeSize);
			layerSize._width = _tipSize.Width;
			layerSize._height = _tipSize.Height;
			NativeMethods.NativeBlendFunction e = new NativeMethods.NativeBlendFunction
			{
				_blendOp = 0,
				_sourceConstantAlpha = (byte)_alpha,
				_blendFlags = 0,
				_alphaFormat = 1
			};
			NativeMethods.UpdateLayeredWindow(base.Handle, screenDc, ref destination, ref layerSize, memoryDc, ref source, 0, ref e, 2);
			NativeMethods.SelectObject(memoryDc, previousObject);
			NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			NativeMethods.DeleteObject(hbitmap);
			NativeMethods.DeleteDC(memoryDc);
			tipGraphics.Dispose();
			tipBitmap.Dispose();
		}

		protected void PaintTipWindow(Message message)
		{
			NativeMethods.NativePaintStruct paintStruct = default(NativeMethods.NativePaintStruct);
			IntPtr paintDc = NativeMethods.BeginPaint(message.HWnd, ref paintStruct);
			NativeMethods.NativeWindowRect windowRect = default(NativeMethods.NativeWindowRect);
			NativeMethods.GetWindowRect(message.HWnd, ref windowRect);
			Rectangle clientBounds = new Rectangle(0, 0, windowRect._right - windowRect._left, windowRect._bottom - windowRect._top);
			Graphics paintGraphics = NavigationGraphicsHelper.GraphicsFromHdc(paintDc);
			Bitmap bufferBitmap = new Bitmap(clientBounds.Width, clientBounds.Height);
			Graphics bufferGraphics = Graphics.FromImage(bufferBitmap);
			if (!_useCompatibleTextRendering)
			{
				bufferGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			DrawTip(bufferGraphics, clientBounds);
			paintGraphics.DrawImageUnscaled(bufferBitmap, 0, 0);
			bufferGraphics.Dispose();
			bufferBitmap.Dispose();
			paintGraphics.Dispose();
			NativeMethods.EndPaint(message.HWnd, ref paintStruct);
		}

		private void CreateTimers()
		{
			_showHideTimer = new Timer();
			_showHideTimer.Tick += OnShowDelayTimerTick;
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
			UpdateLayeredTip(_tipLocation);
			if (_alpha >= 255)
			{
				_fadeTimer.Stop();
			}
		}

		private void OnShowDelayTimerTick(object sender, EventArgs eventArgs)
		{
			_showHideTimer.Stop();
			ShowOrHideTip();
		}

		private void ShowOrHideTip()
		{
			if (!_isWaitingToHide)
			{
				ShowTipWindow();
				if (GetHideDelay() > 0)
				{
					_showHideTimer.Interval = GetHideDelay();
					_isWaitingToHide = true;
					_showHideTimer.Start();
				}
			}
			else
			{
				HideTip();
			}
		}
	}
}
