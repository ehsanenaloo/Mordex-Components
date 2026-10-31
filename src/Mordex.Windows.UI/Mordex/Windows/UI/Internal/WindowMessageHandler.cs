using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Internal
{
	internal class WindowMessageHandler : NativeWindow, IMessageFilter
	{
		private const int WmUserHide = 0x401;
		private const int WmPaint = 0x000F;
		private const int WmActivateApp = 0x001C;
		private const int WmActivate = 0x0006;
		private const int WmSetCursor = 0x0020;
		private const int WmMouseActivate = 0x0021;
		private const int WmNcHitTest = 0x0084;
		private const int WmNcLButtonDown = 0x00A1;
		private const int WmNcLButtonDoubleClick = 0x00A3;
		private const int WmNcRButtonDown = 0x00A4;
		private const int WmNcRButtonDoubleClick = 0x00A6;
		private const int WmNcMButtonDown = 0x00A7;
		private const int WmNcMButtonDoubleClick = 0x00A9;
		private const int WmLButtonDown = 0x0201;
		private const int WmLButtonDoubleClick = 0x0203;
		private const int WmRButtonDown = 0x0204;
		private const int WmRButtonDoubleClick = 0x0206;
		private const int WmMButtonDown = 0x0207;
		private const int WmMButtonDoubleClick = 0x0209;

		private const int HitTestTransparent = -1;
		private const int MouseActivateNoActivate = 3;
		private const uint IdcArrow = 32512u;

		private const int WsPopup = int.MinValue;
		private const int WsExTopMost = 0x8;
		private const int WsExToolWindow = 0x80;
		private const int WsExLayered = 0x80000;
		private const int ShowNoActivate = 4;
		private const int ShowHide = 0;
		private const uint SetWindowPosMoveOnly = 0x211u;
		private const int UpdateLayeredWindowAlpha = 2;
		private const byte BlendFormatPerPixelAlpha = 1;

		private const int DefaultShowDelayMs = 500;
		private const int AutoPopDelayMs = 5000;
		private const int FadeTimerIntervalMs = 70;
		private const int FilterRemovalDelayMs = 10;
		private const int InitialAlpha = 20;
		private const int FullAlpha = 255;
		private const int FadeStep = 80;

		private const int CursorOffsetY = 16;
		private const int ShadowSize = 5;
		private const int ShadowStartOffset = 8;
		private const int ShadowCornerSize = 24;
		private const int ShadowCornerRightOffset = 92;
		private const int ShadowInset = 4;
		private const int ShadowAlpha = 113;
		private const int TextPaddingWidth = 6;
		private const int TextPaddingHeight = 8;
		private const int MaxTextWidth = 100000;

		private bool _useCompatibleTextRendering;

		public bool _wrapsText = true;

		private Color _textColor;

		private Rectangle _windowBounds = Rectangle.Empty;

		private Font _textFont;

		public string _text = "";

		private bool _hasShadow;

		private bool _canFadeIn = true;

		public Point _anchorPoint = Point.Empty;

		internal Control _targetControl;

		private int _showDelayMs = DefaultShowDelayMs;

		private bool _isPersistent = true;

		private System.Windows.Forms.Timer _filterRemovalTimer;

		private bool _isVisible;

		public bool _isTipRequested;

		private int _alpha = InitialAlpha;

		private System.Windows.Forms.Timer _delayTimer;

		private System.Windows.Forms.Timer _fadeTimer;

		private bool _isAutoPopRunning;

		private EventHandler _shownHandler;

		internal WindowMessageHandler(Font font, bool useCompatibleTextRendering)
		{
			SetTextFont(font);
			// Font smoothing type 2 is ClearType; the caller's choice only mattered on Windows 2000 and older.
			_useCompatibleTextRendering = SystemInformation.FontSmoothingType == 2;
			InitializeTimers();
		}

		public Color GetTextColor()
		{
			if (_textColor.IsEmpty)
			{
				return MxThemeManager.ResolveDefault(TipDpi).Color(ThemeRole.TooltipText);
			}
			return _textColor;
		}

		private int TipDpi => _targetControl == null || _targetControl.IsDisposed ? 96 : _targetControl.DeviceDpi;

		public void SetTextColor(Color color)
		{
			_textColor = color;
		}

		private void DrawBackground(Graphics graphics, Rectangle rect)
		{
			if (_hasShadow)
			{
				rect.Width -= 4;
				rect.Height -= 4;
			}
			rect = ThemedToolTip.PaintBox(graphics, rect, MxThemeManager.ResolveDefault(TipDpi));
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetWordWrap(_wrapsText);
			textLayoutRenderer.DrawText(graphics, _text, rect, GetTextFont(), GetTextColor());
			rect.Inflate(1, 1);
			if (_hasShadow)
			{
				DrawShadows(graphics, rect);
			}
		}

		private void DrawBottomShadow(Graphics graphics, Rectangle rectangle)
		{
			Color transparentColor = Color.FromArgb(0, 0, 0, 0);
			Color shadowColor = Color.FromArgb(ShadowAlpha, 0, 0, 0);
			Rectangle rect = new Rectangle(rectangle.X + ShadowStartOffset, rectangle.Bottom - 1, rectangle.Width - ShadowStartOffset, ShadowSize);
			if (rect.Width > 0 && rect.Height > 0)
			{
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, shadowColor, transparentColor, 90f);
				Region region = new Region(rect);
				graphics.FillRegion(linearGradientBrush, region);
				linearGradientBrush.Dispose();
				linearGradientBrush = null;
				region.Dispose();
				region = null;
			}
			if (rectangle.Width > ShadowInset)
			{
				RenderingHelper.DrawVerticalFadeLines(graphics, new Point(rectangle.X + ShadowInset, rectangle.Bottom), ShadowCornerSize, ShadowCornerSize, Color.Empty);
				RenderingHelper.DrawHorizontalFadeLines(graphics, new Point(rectangle.Right, rectangle.Bottom), ShadowCornerRightOffset, -ShadowCornerSize, Color.Empty);
			}
		}

		private void DrawRightShadow(Graphics graphics, Rectangle rectangle)
		{
			Color shadowColor = Color.FromArgb(ShadowAlpha, 0, 0, 0);
			Color transparentColor = Color.FromArgb(0, 0, 0, 0);
			int topOffset = ShadowStartOffset;
			Rectangle rect = new Rectangle(rectangle.Right - 1, rectangle.Y + topOffset, ShadowSize, rectangle.Height - topOffset);
			if (rect.Width > 0 && rect.Height > 0)
			{
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, shadowColor, transparentColor, 0f);
				Region region = new Region(rect);
				graphics.FillRegion(linearGradientBrush, region);
				linearGradientBrush.Dispose();
				linearGradientBrush = null;
				region.Dispose();
				region = null;
			}
			if (rectangle.Height > ShadowInset)
			{
				RenderingHelper.DrawHorizontalFadeLines(graphics, new Point(rectangle.Right, rectangle.Y + ShadowInset), ShadowCornerSize, ShadowCornerSize, Color.Empty);
			}
		}

		private void DrawShadows(Graphics graphics, Rectangle rectangle)
		{
			DrawBottomShadow(graphics, rectangle);
			DrawRightShadow(graphics, rectangle);
		}

		private Size MeasureTipSize()
		{
			IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
			Graphics graphics = RenderingHelper.CreateGraphicsFromHdc(screenDc);
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetWordWrap(_wrapsText);
			SizeF sizeF = textLayoutRenderer.MeasureText(graphics, _text, GetTextFont(), MaxTextWidth);
			if ((double)sizeF.Width > (double)(float)(int)sizeF.Width + 0.5)
			{
				sizeF.Width = (int)sizeF.Width + 1;
			}
			Size result = sizeF.ToSize();
			result.Height++;
			result.Width++;
			graphics.Dispose();
			NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			result.Width += TextPaddingWidth;
			result.Height += TextPaddingHeight;
			Rectangle virtualScreen = SystemInformation.VirtualScreen;
			if (result.Width > virtualScreen.Width)
			{
				result.Width = virtualScreen.Width;
			}
			return result;
		}

		public Font GetTextFont()
		{
			if (_textFont == null)
			{
				return Control.DefaultFont;
			}
			return _textFont;
		}

		public void SetTextFont(Font font)
		{
			_textFont = font;
		}

		private void PositionNearPoint(Point pt)
		{
			int left = pt.X;
			int top = pt.Y + CursorOffsetY;
			Rectangle workingArea = Screen.GetWorkingArea(pt);
			if (left + _windowBounds.Width > workingArea.Right)
			{
				left = workingArea.Right - _windowBounds.Width;
			}
			if (left < workingArea.X)
			{
				left = workingArea.X;
			}
			if (top + _windowBounds.Height > workingArea.Bottom)
			{
				top -= _windowBounds.Height;
			}
			_windowBounds.X = left;
			_windowBounds.Y = top;
		}

		private void CreateTipWindow()
		{
			_hasShadow = VisualStyleSupport.SupportsLayeredWindows();
			_windowBounds.Size = MeasureTipSize();
			if (_anchorPoint.IsEmpty)
			{
				PositionNearPoint(Cursor.Position);
			}
			else
			{
				PositionNearPoint(_anchorPoint);
			}
			CreateParams createParams = new CreateParams();
			createParams.X = _windowBounds.X;
			createParams.Y = _windowBounds.Y;
			createParams.Style = WsPopup;
			createParams.Height = _windowBounds.Height;
			createParams.Width = _windowBounds.Width;
			createParams.Parent = IntPtr.Zero;
			createParams.ExStyle = WsExTopMost | WsExToolWindow;
			if (_hasShadow)
			{
				createParams.ExStyle |= WsExLayered;
			}
			CreateHandle(createParams);
			NativeMethods.ShowWindow(base.Handle, ShowNoActivate);
			OnShown(EventArgs.Empty);
			if (_hasShadow && _canFadeIn && _fadeTimer != null)
			{
				_fadeTimer.Start();
			}
		}

		public void SetShowDelayMs(int delayMs)
		{
			_showDelayMs = delayMs;
		}

		public bool GetIsPersistent()
		{
			return _isPersistent;
		}

		public void SetIsPersistent(bool isPersistent)
		{
			_isPersistent = isPersistent;
		}

		public void RequestShow(string text, Control control)
		{
			if (_isTipRequested)
			{
				_text = text;
				ShowOrHide();
				return;
			}
			_isTipRequested = true;
			_delayTimer.Stop();
			_targetControl = control;
			_text = text;
			if (_showDelayMs > 0)
			{
				_delayTimer.Interval = _showDelayMs;
				_delayTimer.Start();
			}
			else
			{
				ShowOrHide();
			}
		}

		private void ShowTip()
		{
			if (_filterRemovalTimer != null)
			{
				// A delayed removal is still pending, so the filter is still registered.
				StopFilterRemovalTimer();
			}
			else
			{
				Application.AddMessageFilter(this);
			}
			CreateTipWindow();
			NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, IdcArrow));
			_isVisible = true;
		}

		public void UpdateTipText(string text)
		{
			_text = text;
			_windowBounds.Size = MeasureTipSize();
			if (_anchorPoint.IsEmpty)
			{
				PositionNearPoint(Cursor.Position);
			}
			else
			{
				PositionNearPoint(_anchorPoint);
			}
			MoveTo(_windowBounds.Location);
			OnShown(EventArgs.Empty);
		}

		public void MoveTo(Point point)
		{
			_alpha = FullAlpha;
			_canFadeIn = false;
			_windowBounds.Location = point;
			NativeMethods.SetWindowPos(base.Handle, IntPtr.Zero, point.X, point.Y, 0, 0, SetWindowPosMoveOnly);
			if (_hasShadow)
			{
				RenderLayeredWindow();
			}
			_canFadeIn = true;
		}

		internal void Hide()
		{
			_isTipRequested = false;
			if (_delayTimer != null)
			{
				_delayTimer.Stop();
			}
			_alpha = InitialAlpha;
			_isAutoPopRunning = false;
			if (_fadeTimer != null)
			{
				_fadeTimer.Stop();
				_fadeTimer.Dispose();
				_fadeTimer = null;
			}
			if (_isVisible)
			{
				NativeMethods.ShowWindow(base.Handle, ShowHide);
				DestroyHandle();
				StopFilterRemovalTimer();
				_filterRemovalTimer = new System.Windows.Forms.Timer();
				_filterRemovalTimer.Tick += OnFilterRemovalTimerTick;
				_filterRemovalTimer.Interval = FilterRemovalDelayMs;
				_filterRemovalTimer.Start();
				_isVisible = false;
			}
		}

		internal void HideAndDisposeTimers()
		{
			_isTipRequested = false;
			if (_delayTimer != null)
			{
				_delayTimer.Stop();
				_delayTimer.Dispose();
				_delayTimer = null;
			}
			if (_fadeTimer != null)
			{
				_fadeTimer.Stop();
				_fadeTimer.Dispose();
				_fadeTimer = null;
			}
			_alpha = InitialAlpha;
			_isAutoPopRunning = false;
			if (!_isVisible)
			{
				return;
			}
			NativeMethods.ShowWindow(base.Handle, ShowHide);
			DestroyHandle();
			Application.RemoveMessageFilter(this);
			StopFilterRemovalTimer();
			_isVisible = false;
		}

		private void StopFilterRemovalTimer()
		{
			if (_filterRemovalTimer != null)
			{
				System.Windows.Forms.Timer timer = _filterRemovalTimer;
				_filterRemovalTimer = null;
				timer.Tick -= OnFilterRemovalTimerTick;
				timer.Stop();
				timer.Dispose();
			}
		}

		private void OnFilterRemovalTimerTick(object sender, EventArgs eventArgs)
		{
			Application.RemoveMessageFilter(this);
			StopFilterRemovalTimer();
		}

		protected override void WndProc(ref Message message)
		{
			if (message.Msg == WmUserHide)
			{
				Hide();
				return;
			}
			switch (message.Msg)
			{
			case WmNcHitTest:
				message.Result = new IntPtr(HitTestTransparent);
				break;
			case WmNcLButtonDown:
			case WmNcRButtonDown:
			case WmNcMButtonDown:
			case WmLButtonDown:
			case WmRButtonDown:
			case WmMButtonDown:
			case WmActivateApp:
				Hide();
				break;
			case WmMouseActivate:
				message.Result = (IntPtr)MouseActivateNoActivate;
				break;
			case WmPaint:
				ProcessWindowMessage(message);
				break;
			case WmSetCursor:
				NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, IdcArrow));
				break;
			default:
				base.WndProc(ref message);
				break;
			}
		}

		private void RenderLayeredWindow()
		{
			Bitmap bitmap = new Bitmap(_windowBounds.Width, _windowBounds.Height, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			Rectangle rectangle = new Rectangle(0, 0, _windowBounds.Width, _windowBounds.Height);
			if (!_useCompatibleTextRendering)
			{
				graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			DrawBackground(graphics, rectangle);
			IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
			IntPtr memoryDc = NativeMethods.CreateCompatibleDC(screenDc);
			IntPtr bitmapHandle = bitmap.GetHbitmap(Color.FromArgb(0));
			IntPtr previousObject = NativeMethods.SelectObject(memoryDc, bitmapHandle);
			NativeMethods.NativePoint windowPosition = default(NativeMethods.NativePoint);
			windowPosition._x = _windowBounds.X;
			windowPosition._y = _windowBounds.Y;
			NativeMethods.NativePoint sourcePosition = default(NativeMethods.NativePoint);
			sourcePosition._x = 0;
			sourcePosition._y = 0;
			NativeMethods.NativeSize windowSize = default(NativeMethods.NativeSize);
			windowSize._width = _windowBounds.Width;
			windowSize._height = _windowBounds.Height;
			NativeMethods.NativeBlendFunction blend = new NativeMethods.NativeBlendFunction
			{
				_blendOp = 0,
				_sourceConstantAlpha = (byte)_alpha,
				_blendFlags = 0,
				_alphaFormat = BlendFormatPerPixelAlpha
			};
			NativeMethods.UpdateLayeredWindow(base.Handle, screenDc, ref windowPosition, ref windowSize, memoryDc, ref sourcePosition, 0, ref blend, UpdateLayeredWindowAlpha);
			NativeMethods.SelectObject(memoryDc, previousObject);
			NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			NativeMethods.DeleteObject(bitmapHandle);
			NativeMethods.DeleteDC(memoryDc);
			graphics.Dispose();
			bitmap.Dispose();
		}

		protected void ProcessWindowMessage(Message message)
		{
			NativeMethods.NativePaintStruct h = default(NativeMethods.NativePaintStruct);
			IntPtr intPtr = NativeMethods.BeginPaint(message.HWnd, ref h);
			NativeMethods.NativeRect i = default(NativeMethods.NativeRect);
			NativeMethods.GetWindowRect(message.HWnd, ref i);
			Rectangle rectangle = new Rectangle(0, 0, i._right - i._left, i._bottom - i._top);
			Graphics graphics = RenderingHelper.CreateGraphicsFromHdc(intPtr);
			Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
			Graphics bitmapGraphics = Graphics.FromImage(bitmap);
			if (!_useCompatibleTextRendering)
			{
				bitmapGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			DrawBackground(bitmapGraphics, rectangle);
			graphics.DrawImageUnscaled(bitmap, 0, 0);
			bitmapGraphics.Dispose();
			bitmap.Dispose();
			graphics.Dispose();
			NativeMethods.EndPaint(message.HWnd, ref h);
		}

		private void InitializeTimers()
		{
			_delayTimer = new System.Windows.Forms.Timer();
			_delayTimer.Tick += OnDelayTimerTick;
			_fadeTimer = new System.Windows.Forms.Timer();
			_fadeTimer.Interval = FadeTimerIntervalMs;
			_fadeTimer.Tick += OnFadeTimerTick;
		}

		private void OnFadeTimerTick(object sender, EventArgs eventArgs)
		{
			if (_alpha + FadeStep < FullAlpha)
			{
				_alpha += FadeStep;
			}
			else
			{
				_alpha = FullAlpha;
			}
			RenderLayeredWindow();
			if (_alpha >= FullAlpha)
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
			ShowOrHide();
		}

		private void ShowOrHide()
		{
			if (!_isAutoPopRunning)
			{
				ShowTip();
				if (!GetIsPersistent())
				{
					_delayTimer.Interval = AutoPopDelayMs;
					_isAutoPopRunning = true;
					_delayTimer.Start();
				}
			}
			else
			{
				Hide();
			}
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case WmActivate:
			{
				if (_targetControl == null)
				{
					break;
				}
				if (_targetControl.TopLevelControl != null)
				{
					if (message.HWnd == _targetControl.TopLevelControl.Handle)
					{
						Hide();
					}
				}
				else if (_targetControl.Handle == message.HWnd)
				{
					Hide();
				}
				break;
			}
			case WmNcLButtonDown:
			case WmNcLButtonDoubleClick:
			case WmNcRButtonDown:
			case WmNcRButtonDoubleClick:
			case WmNcMButtonDown:
			case WmNcMButtonDoubleClick:
			case WmLButtonDown:
			case WmLButtonDoubleClick:
			case WmRButtonDown:
			case WmRButtonDoubleClick:
			case WmMButtonDown:
			case WmMButtonDoubleClick:
				Hide();
				break;
			}
			return false;
		}

		public void AddShownHandler(EventHandler handler)
		{
			EventHandler eventHandler = _shownHandler;
			EventHandler comparand;
			do
			{
				comparand = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(comparand, handler);
				eventHandler = Interlocked.CompareExchange(ref _shownHandler, value, comparand);
			}
			while ((object)eventHandler != comparand);
		}

		public void RemoveShownHandler(EventHandler handler)
		{
			EventHandler eventHandler = _shownHandler;
			EventHandler comparand;
			do
			{
				comparand = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(comparand, handler);
				eventHandler = Interlocked.CompareExchange(ref _shownHandler, value, comparand);
			}
			while ((object)eventHandler != comparand);
		}

		protected virtual void OnShown(EventArgs eventArgs)
		{
			if (_shownHandler != null)
			{
				_shownHandler(this, eventArgs);
			}
		}
	}
}
