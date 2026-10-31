using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonGlassCaptionMessageHandler : RibbonWindowMessageHandler
	{
		private const uint GetDcExFlags = 0x13u;

		private const int SrcCopy = 0xCC0020;

		private int _captionHeight;

		private bool _wasMaximizedBeforeMinimize;

		private bool _isMinimized;

		private bool _isLayoutRestored = true;

		public override RibbonColorScheme ColorScheme
		{
			get
			{
				if (_ribbon != null)
				{
					return _ribbon._colorScheme;
				}
				return base.ColorScheme;
			}
		}

		public RibbonGlassCaptionMessageHandler(Form form, MxRibbon ribbon)
			: base(form, ribbon)
		{
			_captionHeight = SystemInformation.CaptionHeight;
			if (_form != null && (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow))
			{
				_captionHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			_captionHeight += RibbonDrawingUtils.GetFrameBorderSize().Height - 1;
			_isMinimized = false;
			_titleBarHeight = ((ribbon._captionHeight > 0) ? _ribbon._captionHeight : _captionHeight);
			if (!_ribbon.ShowCaptionBar)
			{
				_titleBarHeight = 0;
			}
			SetBorderWidth(8);
			if (_ribbon.IsInDesignMode())
			{
				_form.Height -= _titleBarHeight + GetTopBorderHeight() + _titleBarHeight;
			}
			else if (_form.WindowState != FormWindowState.Minimized)
			{
				AddOffsetHeight();
				_form.Width++;
				_form.Width--;
				if (_form.WindowState == FormWindowState.Maximized)
				{
					NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, NativeMethods.SwpRefreshFrame);
					_form.ResumeLayout();
				}
			}
		}

		protected RibbonTabStripRenderer GetTabStripRenderer()
		{
			if (_ribbon != null)
			{
				return _ribbon._tabStripRenderer;
			}
			return null;
		}

		private int GetTabStripRowHeight()
		{
			if (_ribbon.ShowCaptionBar)
			{
				return _ribbon._tabStripRowHeight;
			}
			return 0;
		}

		internal override void InvalidateNonClientArea()
		{
		}

		internal override void Refresh(bool update)
		{
			if (!update)
			{
				if (_ribbon != null && !_ribbon.IsTabStripHidden() && _ribbon.Visible)
				{
					_ribbon.Invalidate(invalidateChildren: true);
					_isFrameRefreshed = false;
				}
				else
				{
					base.Refresh(update);
				}
				return;
			}
			PaintCaptionBuffered();
		}

		protected override void WndProc(ref Message message)
		{
			if (!_ribbon.IsParentedToForm() || _ribbon.GetHostForm() == null || _ribbon.GetHostForm().Disposing)
			{
				base.WndProc(ref message);
				return;
			}
			switch (message.Msg)
			{
			case NativeMethods.WmSetText:
				base.ForwardToBaseWndProc(ref message);
				break;
			case NativeMethods.WmNcHitTest:
				HandleNcHitTest(ref message);
				break;
			case NativeMethods.WmWindowPosChanged:
				base.WndProc(ref message);
				if (_isMinimized && _form.WindowState == FormWindowState.Minimized)
				{
					_isLayoutRestored = false;
				}
				else if (_isMinimized && !_isLayoutRestored && _form.WindowState == FormWindowState.Normal)
				{
					_isLayoutRestored = true;
					_ribbon.RecalculateLayout(false);
					ExtendGlassIntoCaption();
					AddOffsetHeight();
					_isMinimized = false;
				}
				break;
			case NativeMethods.WmNcCalcSize:
				HandleNcCalcSize(ref message);
				break;
			case NativeMethods.WmPaint:
				HandlePaint(ref message);
				DrawCaptionBottomLine();
				break;
			case NativeMethods.WmNcLButtonDoubleClick:
				HandleNcLeftButtonDoubleClick(ref message);
				break;
			case NativeMethods.WmNcLButtonUp:
				HandleNcLeftButtonUp(ref message);
				break;
			case NativeMethods.WmNcLButtonDown:
				HandleNcLeftButtonDown(ref message);
				break;
			case NativeMethods.WmEraseBackground:
				if (_ribbon != null && _form != null && (_form.Modal || _ribbon.Minimized))
				{
					_ribbon.Region = null;
					base.WndProc(ref message);
					_ribbon.ApplyClientRegion();
				}
				else
				{
					base.WndProc(ref message);
				}
				DrawCaptionBottomLine();
				break;
			case NativeMethods.WmNcPaint:
				_isFrameRefreshed = true;
				base.WndProc(ref message);
				DrawCaptionBottomLine();
				break;
			case NativeMethods.WmNcRButtonUp:
				HandleNcRightButtonUp(ref message);
				break;
			case NativeMethods.WmNcRButtonDown:
				HandleNcRightButtonDown(ref message);
				break;
			case NativeMethods.WmSysCommand:
				switch (NativeMethods.ToInt32Unchecked(message.WParam) & 0xFFF0)
				{
				case 61472:
					_isMinimized = true;
					if (_wasMaximized)
					{
						_wasMaximizedBeforeMinimize = true;
					}
					_wasMaximized = false;
					break;
				case 61728:
					_isMinimized = false;
					_isLayoutRestored = true;
					_ribbon.RecalculateLayout(false);
					ExtendGlassIntoCaption();
					_wasMaximized = false;
					base.WndProc(ref message);
					if (!_wasMaximizedBeforeMinimize && _form.WindowState != FormWindowState.Maximized)
					{
						AddOffsetHeight();
					}
					_wasMaximizedBeforeMinimize = false;
					_ribbon.Refresh();
					return;
				case 61488:
					_isMinimized = false;
					_isLayoutRestored = true;
					_ribbon.RecalculateLayout(true);
					ExtendGlassIntoCaption();
					_wasMaximized = true;
					_wasMaximizedBeforeMinimize = false;
					_ribbon.Refresh();
					break;
				}
				base.WndProc(ref message);
				break;
			case NativeMethods.WmNcActivate:
				if (IsTopLevelMinimized())
				{
					base.WndProc(ref message);
				}
				else if (_form.IsMdiContainer)
				{
					base.WndProc(ref message);
					RedrawMdiChildren(ref message);
				}
				else
				{
					base.WndProc(ref message);
				}
				break;
			case NativeMethods.WmNcMouseLeave:
				StopTrackingMouseLeave();
				if (GetHeaderRenderer() != null)
				{
					GetHeaderRenderer().ClearHotAndPushedState();
				}
				if (GetTabStripRenderer() != null)
				{
					GetTabStripRenderer().ClearHoverState();
				}
				SetHotCaptionButton(null);
				break;
			default:
				base.WndProc(ref message);
				break;
			case 174:
				break;
			}
		}

		internal void RedrawMdiChildren(ref Message message)
		{
			if (_form == null || _form.WindowState == FormWindowState.Minimized)
			{
				return;
			}
			Form[] mdiChildren = _form.MdiChildren;
			foreach (Form form in mdiChildren)
			{
				if (form.IsHandleCreated)
				{
					NativeMethods.RedrawWindow(form.Handle, IntPtr.Zero, IntPtr.Zero, NativeMethods.RedrawFrameInvalidate);
				}
			}
			_ribbon.Invalidate();
			Refresh(update: true);
		}

		private void HandleNcHitTest(ref Message message)
		{
			IntPtr hitResult = IntPtr.Zero;
			NativeMethods.DwmDefWindowProc(message.HWnd, message.Msg, message.WParam, message.LParam, ref hitResult);
			if (hitResult == (IntPtr)0L)
			{
				Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
				Point point = PointToClient(screenPt);
				if ((_form.FormBorderStyle == FormBorderStyle.Sizable || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow) && !_wasMaximized && _form.WindowState != FormWindowState.Maximized)
				{
					int rightEdge = _form.ClientRectangle.Width + base.GetBorderWidth();
					if (point.Y <= GetTopBorderHeight())
					{
						if (point.X <= 16)
						{
							message.Result = (IntPtr)13L;
						}
						else if (point.X >= rightEdge)
						{
							message.Result = (IntPtr)14L;
						}
						else
						{
							message.Result = (IntPtr)12L;
						}
						return;
					}
				}
				if (GetHeaderRenderer() != null && GetHeaderRenderer().GetArea().Contains(point))
				{
					hitResult = (GetHeaderRenderer()._iconBounds.Contains(point) ? ((IntPtr)3L) : ((!GetHeaderRenderer().HandleMouseMove(point)) ? ((IntPtr)2L) : ((IntPtr)19L)));
				}
				else
				{
					if (GetTabStripRenderer() == null || !GetTabStripRenderer()._stripBounds.Contains(point))
					{
						message.Result = hitResult;
						base.WndProc(ref message);
						return;
					}
					if (GetTabStripRenderer().HandleMouseMove(point))
					{
						hitResult = (IntPtr)19L;
						StartMouseLeaveTracking();
					}
					else
					{
						base.WndProc(ref message);
						hitResult = (IntPtr)2L;
					}
				}
			}
			message.Result = hitResult;
		}

		private void HandleNcLeftButtonDown(ref Message message)
		{
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point point = PointToClient(screenPt);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseDown(point))
			{
				message.Result = IntPtr.Zero;
			}
			else if (GetTabStripRenderer() != null && GetTabStripRenderer().HandleMouseDown(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0)))
			{
				message.Result = IntPtr.Zero;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		private void HandleNcLeftButtonUp(ref Message message)
		{
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point point = PointToClient(screenPt);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseUp(point, MouseButtons.Left))
			{
				message.Result = IntPtr.Zero;
			}
			else if (GetTabStripRenderer() != null && GetTabStripRenderer().HandleMouseUp(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0)))
			{
				message.Result = IntPtr.Zero;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		private void HandleNcRightButtonUp(ref Message message)
		{
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point point = PointToClient(screenPt);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseUp(point, MouseButtons.Right))
			{
				message.Result = IntPtr.Zero;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		private void HandleNcRightButtonDown(ref Message message)
		{
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point point = PointToClient(screenPt);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleRightMouseDown(point))
			{
				message.Result = IntPtr.Zero;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		internal override void DrawHeader(Graphics graphics)
		{
			if (_ribbon != null)
			{
				DrawCaptionBackground(graphics, IsActive());
				if (GetTabStripRenderer() != null)
				{
					GetTabStripRenderer().DrawTabStrip(graphics, Rectangle.Empty);
				}
			}
		}

		private void DrawCaptionBottomLine()
		{
			if (_ribbon == null)
			{
				return;
			}
			NativeMethods.NativeRect i = default(NativeMethods.NativeRect);
			NativeMethods.GetWindowRect(base.Handle, ref i);
			Rectangle rectangle = Rectangle.FromLTRB(i._left, i._top, i._right, i._bottom);
			rectangle.X = 0;
			rectangle.Y = 0;
			if (rectangle.Width == 0 || rectangle.Height == 0)
			{
				return;
			}
			IntPtr windowDc = NativeMethods.GetWindowDC(base.Handle);
			if (windowDc == IntPtr.Zero)
			{
				return;
			}
			try
			{
				Rectangle targetRectangle = new Rectangle(rectangle.X, _titleBarHeight + GetTabStripRowHeight() - 1, rectangle.Width, 1);
				if (targetRectangle.Width <= 0 || targetRectangle.Height <= 0)
				{
					return;
				}
				using BufferedGraphics bufferedGraphics = BufferedGraphicsManager.Current.Allocate(windowDc, targetRectangle);
				RibbonTabRenderer ribbonTabRenderer = null;
				if (!_ribbon.Minimized)
				{
					ribbonTabRenderer = ((!_ribbon.IsBackstageOpen()) ? _ribbon._tabStripRenderer.FindTabRenderer(_ribbon.SelectedTab) : _ribbon._tabStripRenderer.GetTabRenderers()[0]);
				}
				Pen pen = GdiCache.Pen(_ribbon.GetTabContentColor());
				if (ribbonTabRenderer != null)
				{
					Rectangle tabRect = _ribbon._tabStripRenderer.GetVisibleTabBounds(ribbonTabRenderer);
					if (tabRect.Width > 0)
					{
						bufferedGraphics.Graphics.DrawLine(pen, rectangle.X, targetRectangle.Y, tabRect.X, targetRectangle.Y);
						bufferedGraphics.Graphics.DrawLine(pen, tabRect.Right, targetRectangle.Y, rectangle.Right - 1, targetRectangle.Y);
						Color tabLineColor = ColorScheme._selectedTabFillEnd;
						if (_ribbon.IsBackstageOpen())
						{
							tabLineColor = RibbonDrawingUtils.BlendColors(_ribbon.BackstageMenu.BackColor, _ribbon.BackstageMenu.GetBackstageMenuColor(), 0.3f);
						}
						Pen tabLinePen = GdiCache.Pen(tabLineColor);
						bufferedGraphics.Graphics.DrawLine(tabLinePen, tabRect.X + 1, targetRectangle.Y, tabRect.Right - 1, targetRectangle.Y);
					}
					else
					{
						bufferedGraphics.Graphics.DrawLine(pen, rectangle.X, targetRectangle.Y, rectangle.Right - 1, targetRectangle.Y);
					}
				}
				else
				{
					bufferedGraphics.Graphics.DrawLine(pen, rectangle.X, targetRectangle.Y, rectangle.Right - 1, targetRectangle.Y);
				}
				bufferedGraphics.Render(windowDc);
			}
			finally
			{
				NativeMethods.ReleaseDC(base.Handle, windowDc);
			}
		}

		private void HandlePaint(ref Message message)
		{
			base.WndProc(ref message);
			PaintCaptionBuffered();
		}

		private void PaintCaptionBuffered()
		{
			Rectangle rect = new Rectangle(0, 0, _form.Width, _form.Height);
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			rect.Height = _titleBarHeight;
			rect.Height += GetTabStripRowHeight();
			if (rect.Height <= 0)
			{
				return;
			}
			IntPtr windowDc = NativeMethods.GetDCEx(_form.Handle, IntPtr.Zero, GetDcExFlags);
			if (windowDc == IntPtr.Zero)
			{
				return;
			}
			IntPtr memoryDc = NativeMethods.CreateCompatibleDC(windowDc);
			IntPtr bitmap = NativeMethods.CreateCompatibleBitmap(windowDc, rect.Width, rect.Height);
			IntPtr oldBitmap = IntPtr.Zero;
			try
			{
				if (memoryDc == IntPtr.Zero || bitmap == IntPtr.Zero)
				{
					return;
				}
				oldBitmap = NativeMethods.SelectObject(memoryDc, bitmap);
				NativeMethods.BitBlt(memoryDc, 0, 0, rect.Width, rect.Height, windowDc, 0, 0, SrcCopy);
				using (Graphics graphics = Graphics.FromHdc(memoryDc))
				{
					if (_ribbon != null)
					{
						rect.Height--;
					}
					graphics.FillRectangle(Brushes.Black, rect);
					DrawCaptionBackground(graphics, IsActive());
					if (GetTabStripRenderer() != null)
					{
						GetTabStripRenderer().DrawTabStrip(graphics, Rectangle.Empty);
					}
				}
				DrawCaptionBottomLine();
				NativeMethods.BitBlt(windowDc, 0, 0, rect.Width, rect.Height, memoryDc, 0, 0, SrcCopy);
			}
			finally
			{
				if (oldBitmap != IntPtr.Zero)
				{
					NativeMethods.SelectObject(memoryDc, oldBitmap);
				}
				if (bitmap != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(bitmap);
				}
				if (memoryDc != IntPtr.Zero)
				{
					NativeMethods.DeleteDC(memoryDc);
				}
				NativeMethods.ReleaseDC(_form.Handle, windowDc);
			}
		}

		private void DrawCaptionBackground(Graphics graphics, bool isActive)
		{
			if (!_ribbon.IsCaptionBarShown() || !_ribbon.Visible || _ribbon.IsTabStripHidden())
			{
				return;
			}
			if (GetHeaderRenderer() != null)
			{
				GetHeaderRenderer().DrawHeaderBackground(graphics, IsActive(), false);
			}
			if (_ribbon.IsGlassCaptionActive())
			{
				int glowSize = GetTabStripRowHeight() + _titleBarHeight;
				if (glowSize <= 0)
				{
					return;
				}
				if (GetTabStripRowHeight() <= 0)
				{
					glowSize += 26;
				}
				Rectangle rect = new Rectangle(glowSize, 0, _form.Width - glowSize * 2, glowSize);
				Color color = Color.FromArgb(0, Color.White);
				Color gradientEndColor = Color.FromArgb(240, Color.White);
				Color cornerColor = Color.FromArgb(15, Color.White);
				if (_ribbon.IsDarkColorScheme())
				{
					color = Color.FromArgb(5, Color.Black);
					gradientEndColor = Color.FromArgb(190, Color.Black);
					cornerColor = Color.FromArgb(7, Color.Black);
				}
				Blend blend = new Blend(6);
				blend.Positions = new float[6] { 0f, 0.33f, 0.5f, 0.7f, 0.8f, 1f };
				blend.Factors = new float[6] { 0f, 0.2f, 0.5f, 0.75f, 0.9f, 1f };
				using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(rect, 1, 1), color, gradientEndColor, LinearGradientMode.Vertical))
				{
					linearGradientBrush.Blend = blend;
					graphics.FillRectangle(linearGradientBrush, rect);
				}
				Brush brush = GdiCache.Brush(cornerColor);
				graphics.FillRectangle(brush, new Rectangle(rect.Right, 0, glowSize, glowSize));
				graphics.FillRectangle(brush, new Rectangle(0, 0, glowSize, glowSize));
				using (GraphicsPath graphicsPath = new GraphicsPath())
				{
					graphicsPath.AddEllipse(new Rectangle(0, 0, glowSize * 2, glowSize * 2));
					graphicsPath.CloseAllFigures();
					using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
					pathGradientBrush.Blend = blend;
					pathGradientBrush.CenterColor = gradientEndColor;
					pathGradientBrush.SurroundColors = new Color[1] { Color.Transparent };
					graphics.FillRectangle(pathGradientBrush, new Rectangle(0, 0, glowSize, glowSize));
				}
				using (GraphicsPath rightCornerPath = new GraphicsPath())
				{
					rightCornerPath.AddEllipse(new Rectangle(rect.Right - glowSize, 0, glowSize * 2, glowSize * 2));
					using PathGradientBrush rightCornerBrush = new PathGradientBrush(rightCornerPath);
					rightCornerBrush.Blend = blend;
					rightCornerBrush.CenterColor = gradientEndColor;
					rightCornerBrush.SurroundColors = new Color[1] { Color.Transparent };
					graphics.FillRectangle(rightCornerBrush, new Rectangle(rect.Right, 0, glowSize, glowSize));
				}
			}
			if (GetHeaderRenderer() != null)
			{
				GetHeaderRenderer().PaintContextualTabs(graphics);
			}
		}

		private void HandleNcLeftButtonDoubleClick(ref Message message)
		{
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point point = PointToClient(screenPt);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleDoubleClick(point))
			{
				message.Result = IntPtr.Zero;
			}
			else if (GetTabStripRenderer() != null && GetTabStripRenderer().HandleDoubleClick(point))
			{
				message.Result = IntPtr.Zero;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		internal override void AddOffsetHeight()
		{
			int captionHeight = SystemInformation.CaptionHeight;
			if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				captionHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			Size size = RibbonDrawingUtils.GetFrameBorderSize();
			if (_form.FormBorderStyle == FormBorderStyle.FixedDialog || _form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.FixedSingle || _form.FormBorderStyle == FormBorderStyle.Fixed3D)
			{
				size = SystemInformation.FixedFrameBorderSize;
			}
			int systemCaptionHeight = captionHeight;
			if (captionHeight < 26)
			{
				captionHeight = 26;
			}
			int expectedClientHeight = _form.Height - systemCaptionHeight - size.Height * 2;
			int currentClientHeight = _form.Height - GetBottomBorderHeight();
			int expectedClientWidth = _form.Width - size.Width * 2;
			int currentClientWidth = _form.Width - base.GetBorderWidth() * 2;
			int newHeight = _form.Height;
			int newWidth = _form.Width;
			if (expectedClientHeight != currentClientHeight)
			{
				int heightDelta = expectedClientHeight - currentClientHeight;
				newHeight += heightDelta;
			}
			if (expectedClientWidth != currentClientWidth)
			{
				int widthDelta = expectedClientWidth - currentClientWidth;
				newWidth += widthDelta;
			}
			_form.Size = new Size(newWidth, newHeight);
		}

		private void HandleNcCalcSize(ref Message message)
		{
			if (_form == null || _form.FormBorderStyle != FormBorderStyle.None)
			{
				Rectangle captionBounds;
				if (message.WParam == IntPtr.Zero)
				{
					NativeMethods.NativeRect i = (NativeMethods.NativeRect)message.GetLParam(typeof(NativeMethods.NativeRect));
					NativeMethods.NativeRect i2 = CalculateClientRect(i);
					captionBounds = new Rectangle(0, 0, i2._right - i2._left, i2._bottom - i2._top - _titleBarHeight);
					Marshal.StructureToPtr(i2, message.LParam, fDeleteOld: false);
					message.Result = IntPtr.Zero;
				}
				else
				{
					NativeMethods.NcCalcSizeParams l = (NativeMethods.NcCalcSizeParams)message.GetLParam(typeof(NativeMethods.NcCalcSizeParams));
					NativeMethods.NativeRect field_ = l._newWindowRect;
					NativeMethods.NativeRect i2 = CalculateClientRect(field_);
					captionBounds = new Rectangle(0, 0, i2._right - i2._left, i2._bottom - i2._top);
					l._newWindowRect = i2;
					Marshal.StructureToPtr(l, message.LParam, fDeleteOld: false);
					message.Result = IntPtr.Zero;
				}
				ExtendGlassIntoCaption();
				SetCaptionBounds(captionBounds);
			}
		}

		private NativeMethods.NativeRect CalculateClientRect(NativeMethods.NativeRect windowRect)
		{
			SetBorderWidth(RibbonDrawingUtils.GetFrameBorderSize().Width);
			if (_form.FormBorderStyle == FormBorderStyle.FixedDialog || _form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.FixedSingle || _form.FormBorderStyle == FormBorderStyle.Fixed3D)
			{
				SetBorderWidth(SystemInformation.FixedFrameBorderSize.Width);
			}
			_ribbon.SetFrameBorderSize(new Size(base.GetBorderWidth(), 8));
			NativeMethods.NativeRect result = new NativeMethods.NativeRect
			{
				_left = windowRect._left + base.GetBorderWidth(),
				_top = windowRect._top,
				_right = windowRect._right - base.GetBorderWidth(),
				_bottom = windowRect._bottom - base.GetBorderWidth()
			};
			if (_wasMaximized || _form.WindowState == FormWindowState.Maximized)
			{
				if (result._bottom >= SystemInformation.VirtualScreen.Bottom)
				{
					result._bottom = SystemInformation.VirtualScreen.Bottom - 1;
				}
				if (result._left < SystemInformation.VirtualScreen.Left)
				{
					result._left = SystemInformation.VirtualScreen.Left;
				}
				if (result._right > SystemInformation.VirtualScreen.Right)
				{
					result._right = SystemInformation.VirtualScreen.Right;
				}
			}
			if (!_ribbon.IsShownOnVisibleForm())
			{
				result._top += _titleBarHeight;
			}
			return result;
		}

		internal void ResetGlassMargins()
		{
			NativeMethods.Margins d = default(NativeMethods.Margins);
			d._topHeight = 0;
			d._leftWidth = 0;
			d._rightWidth = 0;
			d._bottomHeight = 0;
			NativeMethods.DwmExtendFrameIntoClientArea(_form.Handle, ref d);
		}

		private void ExtendGlassIntoCaption()
		{
			int glassHeight = 0;
			if (!_ribbon.IsTabStripHidden() && _ribbon.Visible)
			{
				glassHeight = _ribbon._captionHeight;
				glassHeight += GetTabStripRowHeight();
			}
			else
			{
				glassHeight = 0;
			}
			NativeMethods.Margins d = default(NativeMethods.Margins);
			d._topHeight = glassHeight;
			d._leftWidth = 0;
			d._rightWidth = 0;
			d._bottomHeight = 0;
			NativeMethods.DwmExtendFrameIntoClientArea(_form.Handle, ref d);
		}

		internal override void SetCaptionBounds(Rectangle clientRect)
		{
			if (_form != null)
			{
				Size size = new Size(SystemInformation.CaptionButtonSize.Width, SystemInformation.CaptionButtonSize.Height);
				if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
				{
					size = new Size(SystemInformation.ToolWindowCaptionButtonSize.Width, SystemInformation.ToolWindowCaptionButtonSize.Height);
				}
				int buttonsLeft = clientRect.Right;
				if (GetControlBox())
				{
					buttonsLeft -= 43;
				}
				if (ShouldShowMinMaxButtons())
				{
					buttonsLeft -= 26;
					buttonsLeft -= 26;
				}
				if (ShouldShowHelpButton())
				{
					buttonsLeft -= 26;
				}
				_ribbon.SetCaptionAreaWidth(buttonsLeft);
				if (GetHeaderRenderer() != null)
				{
					GetHeaderRenderer().SetBorderSize(new Size(base.GetBorderWidth(), base.GetBorderWidth()));
					GetHeaderRenderer().SetArea(new Rectangle(0, 0, buttonsLeft - base.GetBorderWidth(), _titleBarHeight));
					GetHeaderRenderer().RequestLayoutAndRepaint();
				}
			}
		}
	}
}
