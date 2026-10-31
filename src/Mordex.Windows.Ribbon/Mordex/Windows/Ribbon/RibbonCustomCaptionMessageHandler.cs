using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonCustomCaptionMessageHandler : RibbonWindowMessageHandler
	{
		private bool _isRepainting;

		private bool _addOffset;

		internal bool _isRunTimeForm;

		private FormBorderStyle _lastFormBorderStyle = FormBorderStyle.Sizable;

		private FormWindowState _lastWindowState;

		private bool _isSysCommandActive;

		internal override bool HasStatusBar
		{
			get
			{
				return base.HasStatusBar;
			}
			set
			{
				if (HasStatusBar != value)
				{
					base.HasStatusBar = value;
					if (HasStatusBar && !_ribbon.IsInDesignMode())
					{
						_form.Height -= GetBorderWidth() - GetBottomBorderHeight();
					}
				}
			}
		}

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

		public RibbonCustomCaptionMessageHandler(Form form, MxRibbon ribbon, bool addOffset, bool runTimeForm)
			: base(form, ribbon)
		{
			_isRunTimeForm = runTimeForm;
			_closeButton = new RibbonLayoutItem(ribbon);
			_closeButton.AccessibleName = "Close";
			_minimizeButton = new RibbonLayoutItem(ribbon);
			_minimizeButton.AccessibleName = "Minimize";
			_maximizeButton = new RibbonLayoutItem(ribbon);
			_maximizeButton.AccessibleName = ((form.WindowState == FormWindowState.Maximized) ? "Restore" : "Maximize");
			_helpButton = new RibbonLayoutItem(ribbon);
			_helpButton.AccessibleName = "Help";
			RebindStatusBar();
			SetCaptionBounds(form.ClientRectangle);
			_form.Resize += base.OnFormResize;
			if (addOffset)
			{
				AddOffsetHeight();
				_lastFormBorderStyle = form.FormBorderStyle;
				base.OnFormResize((object)null, EventArgs.Empty);
				AttachFormResizeHandler();
				_addOffset = addOffset;
			}
			if (form.WindowState == FormWindowState.Maximized)
			{
				if (!VisualStyleInformation.IsEnabledByUser || !VisualStyleInformation.IsSupportedByOS)
				{
					Rectangle bounds = form.Bounds;
					bounds.X = 0;
					bounds.Y = 0;
					using GraphicsPath path = CreateRoundedPath(bounds);
					// The control owns and disposes the region it is given.
					form.Region = new Region(path);
					form.Region = null;
				}
				else
				{
					NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, NativeMethods.SwpRefreshFrame);
					_form.ResumeLayout();
				}
			}
			else
			{
				_isAdjustingFormSize = true;
				form.Width++;
				form.Width--;
				_isAdjustingFormSize = false;
			}
			InvalidateNonClientArea();
		}

		internal void AttachFormResizeHandler()
		{
			if (_form != null)
			{
				_form.Resize += OnRunTimeFormResize;
			}
		}

		internal void OnFormVisibleChanged(object sender, EventArgs eventArgs)
		{
			DetachFormResizeHandler();
			if (_ribbon != null && _ribbon.IsInDesignMode() && _isRunTimeForm)
			{
				_form.VisibleChanged -= OnFormVisibleChanged;
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
				int systemClientHeight = _form.Height - systemCaptionHeight - size.Height * 2;
				int customClientHeight = _form.Height - GetBottomBorderHeight();
				int systemClientWidth = _form.Width - size.Width * 2;
				int customClientWidth = _form.Width - GetBorderWidth() * 2;
				int newWidth = _form.Width;
				int newHeight = _form.Height;
				if (systemClientHeight != customClientHeight)
				{
					int heightDelta = systemClientHeight - customClientHeight;
					newHeight += heightDelta;
				}
				if (systemClientWidth != customClientWidth)
				{
					int widthDelta = systemClientWidth - customClientWidth;
					newWidth += widthDelta;
				}
				_isAdjustingFormSize = true;
				_form.Size = new Size(newWidth, newHeight);
				_isAdjustingFormSize = false;
			}
			else if (_lastFormBorderStyle != _form.FormBorderStyle)
			{
				int oldCaptionHeight = SystemInformation.CaptionHeight;
				if (_lastFormBorderStyle == FormBorderStyle.FixedToolWindow || _lastFormBorderStyle == FormBorderStyle.SizableToolWindow)
				{
					oldCaptionHeight = SystemInformation.ToolWindowCaptionHeight;
				}
				int newCaptionHeight = SystemInformation.CaptionHeight;
				if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
				{
					newCaptionHeight = SystemInformation.ToolWindowCaptionHeight;
				}
				if (oldCaptionHeight != newCaptionHeight)
				{
					_isAdjustingFormSize = true;
					_form.Height += oldCaptionHeight - newCaptionHeight;
					_isAdjustingFormSize = false;
				}
			}
		}

		internal void DetachFormResizeHandler()
		{
			_form.Resize -= OnRunTimeFormResize;
		}

		private void OnRunTimeFormResize(object sender, EventArgs eventArgs)
		{
			if (_isAdjustingFormSize || !_isRunTimeForm)
			{
				return;
			}
			if (_isRunTimeForm && _lastFormBorderStyle == _form.FormBorderStyle)
			{
				_form.Resize -= OnRunTimeFormResize;
			}
			else if (!_isRunTimeForm)
			{
				_form.Resize -= OnRunTimeFormResize;
			}
			_isAdjustingFormSize = true;
			int captionHeight = SystemInformation.CaptionHeight;
			if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				captionHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			int systemCaptionHeight = captionHeight;
			Size size = RibbonDrawingUtils.GetFrameBorderSize();
			if (_form.FormBorderStyle == FormBorderStyle.FixedDialog || _form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.FixedSingle || _form.FormBorderStyle == FormBorderStyle.Fixed3D)
			{
				size = SystemInformation.FixedFrameBorderSize;
			}
			if (_ribbon.IsInDesignMode())
			{
				int topBorderHeight = GetTopBorderHeight();
				if (captionHeight < 26)
				{
					topBorderHeight -= 26 - captionHeight;
					captionHeight = 26;
					topBorderHeight = Math.Max(0, topBorderHeight);
				}
				int systemClientHeight = _form.Height - systemCaptionHeight - size.Height * 2;
				int customClientHeight = _form.Height - GetBottomBorderHeight() - topBorderHeight;
				int systemClientWidth = _form.Width - size.Width * 2;
				int customClientWidth = _form.Width - GetBorderWidth() * 2;
				int newHeight = _form.Height;
				int newWidth = _form.Width;
				if (systemClientHeight != customClientHeight)
				{
					int heightDelta = systemClientHeight - customClientHeight;
					newHeight += heightDelta;
				}
				if (systemClientWidth != customClientWidth)
				{
					int widthDelta = systemClientWidth - customClientWidth;
					newWidth += widthDelta;
				}
				_form.Size = new Size(newWidth, newHeight);
			}
			_lastFormBorderStyle = _form.FormBorderStyle;
			_isAdjustingFormSize = false;
		}

		internal override void Dispose()
		{
			base.Dispose();
			if (_form != null)
			{
				_form.VisibleChanged -= OnFormVisibleChanged;
				_form.Resize -= OnRunTimeFormResize;
				_form.Resize -= base.OnFormResize;
			}
		}

		internal void RedrawMdiChildrenAndFrame(ref Message message)
		{
			if (_form == null || IsTopLevelMinimized())
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
			DisposeResources();
			PaintNonClientArea();
		}

		internal override void DrawHeader(Graphics graphics)
		{
			if (GetHeaderRenderer() != null && _ribbon != null && (!_ribbon.IsCaptionBarShown() || !_ribbon.Visible || !_ribbon.IsTabStripHidden()))
			{
				GetHeaderRenderer().DrawHeaderBackground(graphics, IsActive(), true);
			}
		}

		internal override void PaintOffice2010FormBorder(Graphics graphics, bool drawCaption, Rectangle rect)
		{
			Rectangle rectangle = new Rectangle(rect.X, rect.Y, rect.Width, _titleBarHeight + GetTopBorderHeight() - 1);
			Color color;
			Color innerBorderColor;
			Color frameColor;
			Color innerFrameColor;
			Color gradientTopColor;
			if (IsActive())
			{
				color = ColorScheme._activeWindowBorder;
				innerBorderColor = ColorScheme._activeWindowInnerBorder;
				frameColor = ColorScheme._activeWindowFrame;
				innerFrameColor = ColorScheme._activeWindowInnerFrame;
				gradientTopColor = ColorScheme._activeCaptionGradientSecond;
				if (drawCaption)
				{
					DrawBackground(graphics, rectangle);
				}
			}
			else
			{
				color = ColorScheme._inactiveWindowBorder;
				innerBorderColor = ColorScheme._inactiveWindowInnerBorder;
				frameColor = ColorScheme._inactiveWindowFrame;
				innerFrameColor = ColorScheme._inactiveWindowInnerFrame;
				gradientTopColor = ColorScheme._inactiveCaptionGradientSecond;
				if (drawCaption)
				{
					DrawInactiveCaptionGradient(graphics, rectangle);
				}
			}
			Pen pen = GdiCache.Pen(color);
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			Pen cornerPen = GdiCache.Pen(color, 3f);
			graphics.DrawArc(cornerPen, rect.X, rect.Y, RibbonWindowMessageHandler.s_cornerRadius, RibbonWindowMessageHandler.s_cornerRadius, 180, 90);
			graphics.DrawArc(cornerPen, rect.Right - RibbonWindowMessageHandler.s_cornerRadius - 1, rect.Y, RibbonWindowMessageHandler.s_cornerRadius, RibbonWindowMessageHandler.s_cornerRadius, 270, 90);
			graphics.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Y);
			graphics.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Bottom);
			graphics.DrawLine(pen, rect.X + 1, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
			graphics.DrawLine(pen, rect.Right - 1, rect.Bottom, rect.Right - 1, rect.Y);
			pen = GdiCache.Pen(innerBorderColor);
			rect.Inflate(-1, -1);
			cornerPen = GdiCache.Pen(innerBorderColor, 1f);
			graphics.DrawArc(cornerPen, rect.X + 1, rect.Y + 1, 6, 6, 180, 90);
			graphics.DrawArc(cornerPen, rect.Right - 6 - 2, rect.Y + 1, 6, 6, 270, 90);
			graphics.DrawLine(pen, rect.X + 3, rect.Y, rect.Right - 4, rect.Y);
			graphics.DrawLine(pen, rect.X, rect.Y + 3, rect.X, rect.Bottom);
			graphics.DrawLine(pen, rect.X + 1, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
			graphics.DrawLine(pen, rect.Right - 1, rect.Bottom, rect.Right - 1, rect.Y + 3);
			graphics.SmoothingMode = smoothingMode;
			rect.Inflate(1, 1);
			pen = GdiCache.Pen(frameColor);
			int top = _ribbon.GetContentAreaBounds().Top;
			graphics.DrawLine(pen, rect.X + 2, top, rect.X + 2, rect.Bottom - 3);
			graphics.DrawLine(pen, rect.Right - 3, rect.Bottom - 3, rect.Right - 3, top);
			graphics.DrawLine(pen, rect.X + 1, rect.Bottom - 3, rect.Right - 2, rect.Bottom - 3);
			int gapHeight = top - rectangle.Bottom;
			if (gapHeight > 0)
			{
				Rectangle gapRect = new Rectangle(rect.X + 2, rectangle.Bottom, GetBorderWidth() - 2, gapHeight);
				using (Brush brush = new LinearGradientBrush(gapRect, gradientTopColor, frameColor, LinearGradientMode.Vertical))
				{
					graphics.FillRectangle(brush, gapRect);
					gapRect = new Rectangle(rect.Right - GetBorderWidth() - 1, rectangle.Bottom, GetBorderWidth() - 1, gapHeight);
					graphics.FillRectangle(brush, gapRect);
				}
			}
			if (GetBorderWidth() >= 4)
			{
				for (int i = 0; i <= GetBorderWidth() - 4; i++)
				{
					graphics.DrawLine(pen, rect.X + 3 + i, top, rect.X + 3 + i, rect.Bottom - 4 - i);
					graphics.DrawLine(pen, rect.X + 2 + i, rect.Bottom - 4 - i, rect.Right - 3 - i, rect.Bottom - 4 - i);
					graphics.DrawLine(pen, rect.Right - 4 - i, rect.Bottom - 4 - i, rect.Right - 4 - i, top);
				}
			}
			pen = GdiCache.Pen(innerFrameColor);
			if (GetBorderWidth() > 4)
			{
				int innerInset = GetBorderWidth() - 1;
				graphics.DrawLine(pen, rect.X + innerInset, top, rect.X + innerInset, rect.Bottom - innerInset - 1);
				graphics.DrawLine(pen, rect.X + innerInset, rect.Bottom - innerInset - 1, rect.Right - innerInset - 1, rect.Bottom - innerInset - 1);
				graphics.DrawLine(pen, rect.Right - innerInset - 1, rect.Bottom - innerInset - 1, rect.Right - innerInset - 1, top);
			}
			RenderContent(graphics);
		}

		internal override void CreateBuffers(IntPtr dc)
		{
			if (!(dc != IntPtr.Zero) || _hasBuffers || IsTopLevelMinimized())
			{
				return;
			}
			if (!_ribbon.Visible)
			{
				base.CreateBuffers(dc);
				return;
			}
			try
			{
				if (_ribbon != null)
				{
					SetActive(((ICommandRenderHost)_ribbon).IsInDesignMode || _form == Form.ActiveForm);
				}
				else
				{
					SetActive(_form == Form.ActiveForm);
				}
				if (!IsActive() && Form.ActiveForm != null && Form.ActiveForm.ActiveMdiChild == _form && _form.MdiParent == Form.ActiveForm)
				{
					SetActive(true);
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
			{
				// Active form lookup fails while the form is closing; treat it as active.
				SetActive(true);
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
			Rectangle targetRectangle = new Rectangle(rectangle.X, rectangle.Bottom - GetBottomBorderHeight(), rectangle.Width, GetBottomBorderHeight());
			if (targetRectangle.Width > 0 && targetRectangle.Height > 0)
			{
				_bottomBorderBuffer = BufferedGraphicsManager.Current.Allocate(dc, targetRectangle);
				if (HasStatusBar)
				{
					PaintStatusBarBorder(_bottomBorderBuffer.Graphics, rectangle);
				}
				else
				{
					PaintFormBorder(_bottomBorderBuffer.Graphics, drawCaption: false, rectangle);
				}
				PaintBottomCorners(_bottomBorderBuffer.Graphics, rectangle);
			}
			Rectangle leftBorderRect = new Rectangle(rectangle.X, rectangle.Y, GetBorderWidth(), rectangle.Height - targetRectangle.Height);
			if (leftBorderRect.Width > 0 && leftBorderRect.Height > 0)
			{
				_leftBorderBuffer = BufferedGraphicsManager.Current.Allocate(dc, leftBorderRect);
				if (IsActive())
				{
					DrawActiveCaptionBackground(_leftBorderBuffer.Graphics, new Rectangle(rectangle.X, rectangle.Y, GetBorderWidth(), _titleBarHeight + GetTopBorderHeight()));
				}
				else
				{
					DrawInactiveCaptionBackground(_leftBorderBuffer.Graphics, new Rectangle(rectangle.X, rectangle.Y, GetBorderWidth(), _titleBarHeight + GetTopBorderHeight()));
				}
				PaintFormBorder(_leftBorderBuffer.Graphics, drawCaption: false, rectangle);
			}
			Rectangle rightBorderRect = new Rectangle(rectangle.Right - GetBorderWidth(), rectangle.Y, GetBorderWidth(), leftBorderRect.Height);
			if (rightBorderRect.Width > 0 && rightBorderRect.Height > 0)
			{
				_rightBorderBuffer = BufferedGraphicsManager.Current.Allocate(dc, rightBorderRect);
				if (IsActive())
				{
					DrawActiveCaptionBackground(_rightBorderBuffer.Graphics, new Rectangle(rightBorderRect.X, rectangle.Y, GetBorderWidth(), _titleBarHeight + GetTopBorderHeight()));
				}
				else
				{
					DrawInactiveCaptionBackground(_rightBorderBuffer.Graphics, new Rectangle(rightBorderRect.X, rectangle.Y, GetBorderWidth(), _titleBarHeight + GetTopBorderHeight()));
				}
				PaintFormBorder(_rightBorderBuffer.Graphics, drawCaption: false, rectangle);
			}
			if (rectangle.Width > 0 && rectangle.Height > 0)
			{
				if (_leftBorderBuffer != null)
				{
					PaintBottomCorners(_leftBorderBuffer.Graphics, rectangle);
				}
				if (_rightBorderBuffer != null)
				{
					PaintBottomCorners(_rightBorderBuffer.Graphics, rectangle);
				}
			}
			_hasBuffers = true;
		}

		private void RepaintFrame()
		{
			if (_form != null && !IsTopLevelMinimized())
			{
				PaintNonClientArea();
			}
		}

		internal override void PaintCaptionButtons(RibbonLayoutItem button)
		{
			if (_ribbon != null && _ribbon._headerRenderer != null && !_ribbon.IsTabStripHidden())
			{
				_ribbon._headerRenderer.RepaintCaptionButton(button);
			}
			else
			{
				base.PaintCaptionButtons(button);
			}
		}

		private void HandleNcLeftButtonUp(ref Message message)
		{
			Point point = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point clientPoint = PointToClient(point);
			if (_ribbon != null)
			{
				clientPoint = _ribbon.PointToClient(point);
			}
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseUp(clientPoint, MouseButtons.Left))
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

		private void HandleNcLeftButtonDoubleClick(ref Message message)
		{
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point point = PointToClient(screenPt);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleDoubleClick(point))
			{
				message.Result = IntPtr.Zero;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		private void HandleNcLeftButtonDown(ref Message message)
		{
			if (IsTopLevelMinimized())
			{
				base.WndProc(ref message);
				return;
			}
			Point point = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point pt = PointToClient(point);
			if (_ribbon == null || !_ribbon.IsInDesignMode())
			{
				if (_minimizeButton._isVisible && _minimizeButton._isEnabled)
				{
					Rectangle bounds = _minimizeButton._bounds;
					if (bounds.Contains(pt))
					{
						SetPushedCaptionButton(_minimizeButton);
						TrackCaptionButtonClick(_minimizeButton, message.HWnd, (IntPtr)61472L);
						message.Result = IntPtr.Zero;
						return;
					}
				}
				if (_maximizeButton._isVisible && _maximizeButton._isEnabled)
				{
					Rectangle maximizeBounds = _maximizeButton._bounds;
					if (maximizeBounds.Contains(pt))
					{
						SetPushedCaptionButton(_maximizeButton);
						if (_form.WindowState == FormWindowState.Maximized)
						{
							TrackCaptionButtonClick(_maximizeButton, message.HWnd, (IntPtr)61728L);
						}
						else
						{
							TrackCaptionButtonClick(_maximizeButton, message.HWnd, (IntPtr)61488L);
						}
						message.Result = IntPtr.Zero;
						return;
					}
				}
				if (_closeButton._isVisible)
				{
					Rectangle closeBounds = _closeButton._bounds;
					if (closeBounds.Contains(pt))
					{
						SetPushedCaptionButton(_closeButton);
						TrackCaptionButtonClick(_closeButton, message.HWnd, (IntPtr)61536L);
						message.Result = IntPtr.Zero;
						return;
					}
				}
				if (_helpButton._isVisible)
				{
					Rectangle helpBounds = _helpButton._bounds;
					if (helpBounds.Contains(pt))
					{
						SetPushedCaptionButton(_helpButton);
						TrackCaptionButtonClick(_helpButton, message.HWnd, (IntPtr)61824L);
					}
				}
			}
			SetPushedCaptionButton(null);
			pt = _ribbon.PointToClient(point);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseDown(pt))
			{
				message.Result = IntPtr.Zero;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		private NativeMethods.NativeRect CalculateClientRect(NativeMethods.NativeRect windowRect)
		{
			SetBorderWidth(RibbonDrawingUtils.GetFrameBorderSize().Width);
			int captionHeight = SystemInformation.CaptionHeight;
			if (GetBorderWidth() < 4)
			{
				SetBorderWidth(4);
			}
			if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				captionHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			if (_form != null && _form.WindowState == FormWindowState.Maximized && GetBorderWidth() != RibbonDrawingUtils.GetFrameBorderSize().Width && !_form.IsMdiChild)
			{
				if (windowRect._top < -GetBorderWidth())
				{
					windowRect._bottom -= GetBorderWidth();
					windowRect._top += GetBorderWidth();
				}
				if (windowRect._left < -GetBorderWidth())
				{
					windowRect._right -= GetBorderWidth();
					windowRect._left += GetBorderWidth();
				}
			}
			if (captionHeight < 26)
			{
				captionHeight = 26;
			}
			_ribbon.SetFrameBorderSize(new Size(GetBorderWidth(), GetBorderWidth()));
			if ((_wasMaximized || _form.WindowState == FormWindowState.Maximized) && _form.IsMdiChild && windowRect._top < 0)
			{
				Size size = RibbonDrawingUtils.GetFrameBorderSize();
				windowRect._top = -(captionHeight + GetTopBorderHeight());
				windowRect._left = -GetBorderWidth();
				windowRect._right -= size.Width - GetBorderWidth();
				windowRect._bottom -= size.Height - GetBorderWidth();
			}
			int borderWidth = GetBorderWidth();
			_titleBarHeight = captionHeight;
			NativeMethods.NativeRect result = new NativeMethods.NativeRect
			{
				_left = windowRect._left + borderWidth
			};
			if (_ribbon != null && (_ribbon.Visible || !_form.Visible))
			{
				result._top = windowRect._top;
			}
			else
			{
				result._top = windowRect._top + _titleBarHeight + GetTopBorderHeight();
			}
			result._right = windowRect._right - borderWidth;
			result._bottom = windowRect._bottom - GetBottomBorderHeight();
			if ((_wasMaximized || _form.WindowState == FormWindowState.Maximized) && !_form.IsMdiChild)
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
			return result;
		}

		private void HandleNcMouseMove(ref Message message)
		{
			Point point = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point pt = PointToClient(point);
			if (_closeButton._isVisible && _closeButton._bounds.Contains(pt))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
				return;
			}
			if (_maximizeButton._isVisible && _maximizeButton._isEnabled && _maximizeButton._bounds.Contains(pt))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
				return;
			}
			if (_minimizeButton._isVisible && _minimizeButton._isEnabled && _minimizeButton._bounds.Contains(pt))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
				return;
			}
			if (_helpButton._isVisible && _helpButton._isEnabled && _helpButton._bounds.Contains(pt))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
				return;
			}
			pt = _ribbon.PointToClient(point);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseMove(pt))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
			}
		}

		private void HandleNcCalcSize(ref Message message)
		{
			if (_form != null && _form.FormBorderStyle == FormBorderStyle.None)
			{
				return;
			}
			if (_lastWindowState != _form.WindowState && _form.WindowState == FormWindowState.Normal)
			{
				_lastWindowState = _form.WindowState;
				AddOffsetHeight();
				RedrawFrame();
				return;
			}
			_lastWindowState = _form.WindowState;
			Rectangle captionBounds;
			if (message.WParam == IntPtr.Zero)
			{
				NativeMethods.NativeRect i = (NativeMethods.NativeRect)message.GetLParam(typeof(NativeMethods.NativeRect));
				NativeMethods.NativeRect i2 = CalculateClientRect(i);
				captionBounds = new Rectangle(0, 0, i2._right - i2._left, i2._bottom - i2._top);
				Marshal.StructureToPtr(i2, message.LParam, fDeleteOld: false);
				message.Result = IntPtr.Zero;
			}
			else
			{
				NativeMethods.NcCalcSizeParams l = (NativeMethods.NcCalcSizeParams)message.GetLParam(typeof(NativeMethods.NcCalcSizeParams));
				NativeMethods.NativeRect field_ = l._newWindowRect;
				NativeMethods.NativeRect calculatedRect = CalculateClientRect(field_);
				captionBounds = new Rectangle(0, 0, calculatedRect._right - calculatedRect._left, calculatedRect._bottom - calculatedRect._top);
				l._newWindowRect = calculatedRect;
				Marshal.StructureToPtr(l, message.LParam, fDeleteOld: false);
				message.Result = IntPtr.Zero;
			}
			SetCaptionBounds(captionBounds);
		}

		private void HandleNcHitTest(ref Message message)
		{
			Point point = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point clientPoint = PointToClient(point);
			if (_closeButton._isVisible && _closeButton._bounds.Contains(clientPoint))
			{
				SetHotCaptionButton(_closeButton);
				message.Result = (IntPtr)20L;
				return;
			}
			if (_maximizeButton._isVisible && _maximizeButton._isEnabled && _maximizeButton._bounds.Contains(clientPoint))
			{
				SetHotCaptionButton(_maximizeButton);
				message.Result = (IntPtr)9L;
				return;
			}
			if (_minimizeButton._isVisible && _minimizeButton._isEnabled && _minimizeButton._bounds.Contains(clientPoint))
			{
				SetHotCaptionButton(_minimizeButton);
				message.Result = (IntPtr)8L;
				return;
			}
			if (_helpButton._isVisible && _helpButton._isEnabled && _helpButton._bounds.Contains(clientPoint))
			{
				SetHotCaptionButton(_helpButton);
				message.Result = (IntPtr)21L;
				return;
			}
			SetHotCaptionButton(null);
			Point ribbonPoint = clientPoint;
			ribbonPoint = _ribbon.PointToClient(point);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseMove(ribbonPoint))
			{
				if (GetHeaderRenderer()._iconBounds.Contains(ribbonPoint))
				{
					message.Result = (IntPtr)3L;
				}
				else
				{
					message.Result = (IntPtr)19L;
				}
				return;
			}
			int clientBottom = _form.ClientRectangle.Height + GetTopBorderHeight();
			int clientRight = _form.ClientRectangle.Width + GetBorderWidth();
			if (_form.WindowState != FormWindowState.Maximized && !_wasMaximized && (_form.FormBorderStyle == FormBorderStyle.Sizable || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow))
			{
				if (clientPoint.Y <= GetTopBorderHeight())
				{
					if (clientPoint.X <= 16)
					{
						message.Result = (IntPtr)13L;
					}
					else if (clientPoint.X >= clientRight - 16)
					{
						message.Result = (IntPtr)14L;
					}
					else
					{
						message.Result = (IntPtr)12L;
					}
					return;
				}
				if (clientPoint.X <= GetBorderWidth())
				{
					if (clientPoint.Y <= 16)
					{
						message.Result = (IntPtr)13L;
					}
					else if (clientPoint.Y >= clientBottom - 16)
					{
						message.Result = (IntPtr)16L;
					}
					else
					{
						message.Result = (IntPtr)10L;
					}
					return;
				}
				if (clientPoint.X >= clientRight)
				{
					if (clientPoint.Y <= 16)
					{
						message.Result = (IntPtr)14L;
					}
					else if (clientPoint.Y >= clientBottom - 16)
					{
						message.Result = (IntPtr)17L;
					}
					else
					{
						message.Result = (IntPtr)11L;
					}
					return;
				}
				if (clientPoint.Y >= clientBottom)
				{
					if (clientPoint.X <= 16)
					{
						message.Result = (IntPtr)16L;
					}
					else if (clientPoint.X >= clientRight - 16)
					{
						message.Result = (IntPtr)17L;
					}
					else
					{
						message.Result = (IntPtr)15L;
					}
					return;
				}
			}
			if (clientPoint.Y <= _titleBarHeight)
			{
				message.Result = (IntPtr)2L;
			}
			else if (clientPoint.X <= GetBorderWidth() || clientPoint.X >= clientRight || clientPoint.Y >= clientBottom)
			{
				message.Result = (IntPtr)18L;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		internal override void SetCaptionBounds(Rectangle clientRect)
		{
			if (_form == null)
			{
				return;
			}
			Size size = new Size(SystemInformation.CaptionButtonSize.Width, SystemInformation.CaptionButtonSize.Height);
			if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				size = new Size(SystemInformation.ToolWindowCaptionButtonSize.Width, SystemInformation.ToolWindowCaptionButtonSize.Height);
			}
			if (size.Width < 25)
			{
				size = new Size(25, size.Height);
			}
			if (size.Height < 21)
			{
				size = new Size(size.Width, 21);
			}
			int buttonX = clientRect.Right - size.Width + 4;
			int y = _titleBarHeight - size.Height + 3;
			if (_form.IsMdiChild && _form.WindowState == FormWindowState.Minimized)
			{
				size = new Size(size.Width, 21);
				buttonX = clientRect.Right - size.Width + 4;
				y = 4;
			}
			if (GetControlBox())
			{
				_closeButton._isVisible = true;
				_closeButton._bounds = new Rectangle(buttonX, y, size.Width, size.Height);
				buttonX -= size.Width;
				_closeButton._bounds.Inflate(0, -1);
			}
			else
			{
				_closeButton._isVisible = false;
				_closeButton._bounds = Rectangle.Empty;
			}
			if (ShouldShowMinMaxButtons())
			{
				_maximizeButton._isVisible = true;
				_minimizeButton._isVisible = true;
				_maximizeButton._bounds = new Rectangle(buttonX, y, size.Width, size.Height);
				buttonX -= size.Width;
				_minimizeButton._bounds = new Rectangle(buttonX, y, size.Width, size.Height);
				buttonX -= size.Width;
				_minimizeButton._bounds.Inflate(0, -1);
				_maximizeButton._bounds.Inflate(0, -1);
			}
			else
			{
				_maximizeButton._isVisible = false;
				_maximizeButton._bounds = Rectangle.Empty;
				_minimizeButton._isVisible = false;
				_minimizeButton._bounds = Rectangle.Empty;
			}
			if (ShouldShowHelpButton())
			{
				_helpButton._isVisible = true;
				_helpButton._bounds = new Rectangle(buttonX, y, size.Width, size.Height);
				_helpButton._bounds.Inflate(0, -1);
			}
			else
			{
				_helpButton._isVisible = false;
				_helpButton._bounds = Rectangle.Empty;
			}
			_minimizeButton._isEnabled = _form.MinimizeBox;
			_maximizeButton._isEnabled = _form.MaximizeBox;
			_ribbon.SetCaptionAreaWidth(buttonX);
			if (GetHeaderRenderer() != null)
			{
				GetHeaderRenderer().SetBorderSize(new Size(GetBorderWidth(), GetBorderWidth()));
				if (!_ribbon.IsTabStripHidden())
				{
					GetHeaderRenderer().SetArea(new Rectangle(0, 0, clientRect.Right, _titleBarHeight + GetTopBorderHeight()));
				}
				else
				{
					GetHeaderRenderer().SetArea(new Rectangle(GetBorderWidth(), 0, clientRect.Right, _titleBarHeight + GetTopBorderHeight()));
				}
				GetHeaderRenderer().RequestLayoutAndRepaint();
			}
		}

		internal override void InvalidateNonClientArea()
		{
			if (_form != null && !_form.IsDisposed && !_form.Disposing)
			{
				try
				{
					DisposeResources();
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, NativeMethods.RedrawFrameNoChildren);
				}
				catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
				{
					// The form handle is gone during shutdown; there is nothing left to redraw.
				}
			}
		}

		internal override void Refresh(bool update)
		{
			try
			{
				DisposeResources();
				if (_form.IsHandleCreated && !_form.IsDisposed && !_form.Disposing)
				{
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, NativeMethods.RedrawFrameInvalidate);
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
			{
				// The form handle is gone during shutdown; there is nothing left to redraw.
			}
		}

		protected override void WndProc(ref Message message)
		{
			if (_form != null && _form.IsMdiChild && _form.Parent != null)
			{
				IntPtr panelManagerFlag = NativeMethods.GetPropW(_form.Parent.Handle, "Mordex_PanelManager");
				if (panelManagerFlag == new IntPtr(1))
				{
					base.WndProc(ref message);
					return;
				}
			}
			switch (message.Msg)
			{
			case 125:
				DisposeResources();
				base.WndProc(ref message);
				break;
			case NativeMethods.WmSetIcon:
				base.ForwardToBaseWndProc(ref message);
				break;
			case NativeMethods.WmNcPaint:
				if (IsTopLevelMinimized() || _form.FormBorderStyle == FormBorderStyle.None)
				{
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, NativeMethods.RedrawFrameInvalidate);
					base.WndProc(ref message);
					break;
				}
				if (!_isSysCommandActive)
				{
					_wasMaximized = _form.WindowState == FormWindowState.Maximized;
				}
				if (!_isRepainting)
				{
					_isRepainting = true;
					RepaintFrame();
					_isRepainting = false;
				}
				break;
			case 546:
				base.WndProc(ref message);
				InvalidateNonClientArea();
				break;
			case NativeMethods.WmActivate:
				base.WndProc(ref message);
				break;
			case NativeMethods.WmNcActivate:
				if (IsTopLevelMinimized())
				{
					base.WndProc(ref message);
					break;
				}
				if (!_form.IsMdiChild || _form.FormBorderStyle == FormBorderStyle.None)
				{
					if (message.WParam == IntPtr.Zero)
					{
						message.Result = (IntPtr)1;
					}
					if (_form.IsMdiChild)
					{
						NativeMethods.SendMessage(_form.Handle, NativeMethods.WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
						base.WndProc(ref message);
						NativeMethods.SendMessage(_form.Handle, NativeMethods.WmSetRedraw, (IntPtr)1, IntPtr.Zero);
					}
					else
					{
						RedrawMdiChildrenAndFrame(ref message);
					}
					break;
				}
				if (message.WParam == IntPtr.Zero)
				{
					message.Result = (IntPtr)1;
				}
				NativeMethods.SendMessage(_form.Handle, NativeMethods.WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
				base.WndProc(ref message);
				NativeMethods.SendMessage(_form.Handle, NativeMethods.WmSetRedraw, (IntPtr)1, IntPtr.Zero);
				_form.Invalidate(invalidateChildren: true);
				RedrawMdiChildrenAndFrame(ref message);
				InvalidateNonClientArea();
				break;
			case NativeMethods.WmNcLButtonDoubleClick:
				if (!IsTopLevelMinimized())
				{
					HandleNcLeftButtonDoubleClick(ref message);
				}
				else
				{
					base.WndProc(ref message);
				}
				break;
			case NativeMethods.WmNcRButtonUp:
				if (!IsTopLevelMinimized())
				{
					HandleNcRightButtonUp(ref message);
				}
				else
				{
					base.WndProc(ref message);
				}
				break;
			case NativeMethods.WmNcLButtonUp:
				if (!IsTopLevelMinimized())
				{
					HandleNcLeftButtonUp(ref message);
				}
				else
				{
					base.WndProc(ref message);
				}
				break;
			case NativeMethods.WmNcRButtonDown:
				if (!IsTopLevelMinimized())
				{
					HandleNcRightButtonDown(ref message);
				}
				else
				{
					base.WndProc(ref message);
				}
				break;
			case NativeMethods.WmNcLButtonDown:
				if (!IsTopLevelMinimized())
				{
					HandleNcLeftButtonDown(ref message);
				}
				else
				{
					base.WndProc(ref message);
				}
				break;
			case NativeMethods.WmNcHitTest:
				if (!IsTopLevelMinimized())
				{
					HandleNcHitTest(ref message);
				}
				else
				{
					base.WndProc(ref message);
				}
				break;
			case NativeMethods.WmNcCalcSize:
				HandleNcCalcSize(ref message);
				break;
			case NativeMethods.WmNcMouseMove:
				HandleNcMouseMove(ref message);
				break;
			case NativeMethods.WmSetText:
				base.ForwardToBaseWndProc(ref message);
				break;
			case NativeMethods.WmNcMouseLeave:
				StopTrackingMouseLeave();
				if (GetHeaderRenderer() != null)
				{
					GetHeaderRenderer().ClearHotAndPushedState();
				}
				SetHotCaptionButton(null);
				break;
			case NativeMethods.WmSize:
				base.WndProc(ref message);
				InvalidateNonClientArea();
				break;
			case NativeMethods.WmSetCursor:
				base.WndProc(ref message);
				if ((!VisualStyleInformation.IsEnabledByUser || !VisualStyleInformation.IsSupportedByOS) && _ribbon != null)
				{
					_ribbon.Invalidate();
				}
				break;
			case NativeMethods.WmSysCommand:
				switch (NativeMethods.ToInt32Unchecked(message.WParam) & 0xFFF0)
				{
				case 61472:
					_wasMaximized = false;
					break;
				case 61728:
					_wasMaximized = false;
					_form.Invalidate();
					break;
				case 61488:
					_wasMaximized = true;
					break;
				case 61456:
					_wasMaximized = false;
					base.WndProc(ref message);
					return;
				}
				_isSysCommandActive = true;
				base.WndProc(ref message);
				_isSysCommandActive = false;
				_wasMaximized = false;
				break;
			case NativeMethods.WmWindowPosChanging:
				if (_form.IsMdiChild)
				{
					bool isMaximized = _form.WindowState == FormWindowState.Maximized;
					if (_wasMaximized && _wasMaximized != isMaximized)
					{
						_wasMaximized = isMaximized;
					}
				}
				base.WndProc(ref message);
				break;
			default:
				base.WndProc(ref message);
				break;
			case 174:
				break;
			}
		}

		private void TrackCaptionButtonClick(RibbonLayoutItem button, IntPtr windowHandle, IntPtr sysCommand)
		{
			RibbonDrawingUtils.SetCapture(_form, true);
			button.Pushed = true;
			bool isReleased = false;
			NativeMethods.NativeMessage n = default(NativeMethods.NativeMessage);
			while (!isReleased)
			{
				NativeMethods.PeekMessage(ref n, windowHandle, NativeMethods.WmMouseFirst, NativeMethods.WmMouseLast, NativeMethods.PmRemove);
				switch (n._message)
				{
				case 514:
				{
					RibbonDrawingUtils.SetCapture(_form, false);
					SetPushedCaptionButton(null);
					isReleased = true;
					Point screenPt = _form.PointToScreen(new Point(NativeMethods.ToInt32Unchecked(n._lParam)));
					Point pt = PointToClient(screenPt);
					if (button._bounds.Contains(pt))
					{
						NativeMethods.SendMessage(windowHandle, NativeMethods.WmSysCommand, sysCommand, IntPtr.Zero);
					}
					break;
				}
				case 512:
				{
					Point screenPt = _form.PointToScreen(new Point(NativeMethods.ToInt32Unchecked(n._lParam)));
					Point pt = PointToClient(screenPt);
					if (button._bounds.Contains(pt))
					{
						SetPushedCaptionButton(button);
					}
					else if (button.Pushed)
					{
						SetPushedCaptionButton(null);
					}
					break;
				}
				}
			}
		}
	}
}
