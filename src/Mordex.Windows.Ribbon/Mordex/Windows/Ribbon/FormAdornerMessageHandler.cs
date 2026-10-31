using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class FormAdornerMessageHandler : RibbonWindowMessageHandler
	{
		private Rectangle _iconBounds;

		private Rectangle _applicationTextBounds;

		private Rectangle _documentTextBounds;

		private Rectangle _titleAreaBounds;

		private bool _isTitleLayoutPending;

		private bool _isUpdatingNonClientArea;

		private bool _isStatusBarHeightPending;
#pragma warning disable CS0414 // Layout state kept for the persisted format.

		private bool _hasResizedForm;
#pragma warning restore CS0414

		private bool _runTimeForm;

		private FormBorderStyle _originalBorderStyle;

		private int _captionHeight;

		private Size _frameBorderSize;
#pragma warning disable CS0414 // Layout state kept for the persisted format.

		private bool _isResizingForm;
#pragma warning restore CS0414

		private bool _isDesignerLoading;

		private FormWindowState _formWindowState;

		private bool _isSysCommandActive;

		internal override bool HasStatusBar
		{
			get
			{
				return base.HasStatusBar;
			}
			set
			{
				if (HasStatusBar == value)
				{
					return;
				}
				base.HasStatusBar = value;
				if (HasStatusBar && _isStatusBarHeightPending)
				{
					_isStatusBarHeightPending = false;
					if (!_formAdorner.IsInDesignMode())
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
				if (_formAdorner != null)
				{
					return _formAdorner._colorScheme;
				}
				return null;
			}
		}

		public override string DocumentText
		{
			get
			{
				string text = "";
				if (_formAdorner != null && _formAdorner.Form != null)
				{
					text = _formAdorner.DocumentName;
					if (text.Length > 0 && _formAdorner.ApplicationName.Length > 0)
					{
						text += " - ";
					}
				}
				return text;
			}
		}

		public override string ApplicationText
		{
			get
			{
				string result = "";
				if (_formAdorner != null && _formAdorner.Form != null)
				{
					result = _formAdorner.ApplicationName;
				}
				return result;
			}
		}

		public FormAdornerMessageHandler(Form form, MxRibbonFormAdorner formAdorner, bool addOffset, bool runTimeForm)
			: base(form, null)
		{
			_closeButton = new RibbonLayoutItem(form);
			_minimizeButton = new RibbonLayoutItem(form);
			_maximizeButton = new RibbonLayoutItem(form);
			_helpButton = new RibbonLayoutItem(form);
			_closeButton.AccessibleName = "Close";
			_minimizeButton.AccessibleName = "Minimize";
			_maximizeButton.AccessibleName = ((form.WindowState == FormWindowState.Maximized) ? "Restore" : "Maximize");
			_helpButton.AccessibleName = "Help";
			if (!form.ControlBox && form.Text.Length == 0)
			{
				_titleBarHeight = 0;
			}
			_runTimeForm = runTimeForm;
			_formAdorner = formAdorner;
			_isDesignerLoading = _formAdorner.IsInDesignMode() && !runTimeForm;
			try
			{
				if (_isDesignerLoading && formAdorner.Site != null)
				{
					IDesignerHost designerHost = (IDesignerHost)formAdorner.Site.GetService(typeof(IDesignerHost));
					if (designerHost != null && !designerHost.Loading)
					{
						_isDesignerLoading = false;
					}
				}
			}
			catch (Exception ex) when (ex is InvalidCastException || ex is ObjectDisposedException)
			{
				// No usable designer host while the site is being torn down.
			}
			_isStatusBarHeightPending = true;
			RebindStatusBar();
			SetCaptionBounds(form.ClientRectangle);
			form.Resize += base.OnFormResize;
			if (addOffset)
			{
				base.OnFormResize((object)null, EventArgs.Empty);
			}
			_originalBorderStyle = form.FormBorderStyle;
			if (addOffset)
			{
				if (!_formAdorner.IsInDesignMode())
				{
					AdjustFormSizeForFrame(true, 1);
					return;
				}
				_captionHeight = SystemInformation.CaptionHeight;
				_frameBorderSize = RibbonDrawingUtils.GetFrameBorderSize();
			}
		}

		internal void AdjustFormSizeForFrame(bool forceResize, int multiplier)
		{
			if (forceResize)
			{
				_captionHeight = SystemInformation.CaptionHeight;
				_frameBorderSize = RibbonDrawingUtils.GetFrameBorderSize();
			}
			int captionHeight = SystemInformation.CaptionHeight;
			if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				captionHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			Size size = RibbonDrawingUtils.GetFrameBorderSize();
			if (_form.FormBorderStyle == FormBorderStyle.FixedDialog || _form.FormBorderStyle == FormBorderStyle.FixedSingle || _form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.Fixed3D)
			{
				size = SystemInformation.FixedFrameBorderSize;
			}
			if (!_form.ControlBox && _form.Text.Length == 0)
			{
				captionHeight = 0;
			}
			int rawCaptionHeight = captionHeight;
			_frameBorderSize = size;
			_captionHeight = captionHeight;
			GetTopBorderHeight();
			if (captionHeight < 26)
			{
				captionHeight = 26;
			}
			int width = _form.Width;
			int height = _form.Height;
			int clientWidth = _form.Width - size.Width - size.Width;
			int clientHeight = _form.Height - rawCaptionHeight - size.Height - size.Height;
			int widthDelta = clientWidth - (_form.Width - GetBorderWidth() - GetBorderWidth());
			int heightDelta = clientHeight - (_form.Height - captionHeight - GetTopBorderHeight() - GetBorderWidth());
			if (_runTimeForm && rawCaptionHeight < captionHeight)
			{
				_formAdorner.IsInDesignMode();
			}
			if (_formAdorner.IsInDesignMode() || forceResize)
			{
				int scaledWidthDelta = 0;
				scaledWidthDelta = widthDelta * multiplier;
				heightDelta *= multiplier;
				widthDelta = scaledWidthDelta;
				if (HasStatusBar)
				{
					_isStatusBarHeightPending = false;
					heightDelta -= (GetBorderWidth() - GetBottomBorderHeight()) * multiplier;
				}
				width += widthDelta;
				height += heightDelta;
				if (width != _form.Size.Width || height != _form.Size.Height)
				{
					_isResizingForm = true;
					_form.Size = new Size(width, height);
					_isResizingForm = false;
					_hasResizedForm = true;
				}
			}
		}

		internal void RecalculateFormSize()
		{
			int captionHeight = SystemInformation.CaptionHeight;
			if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				captionHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			Size size = RibbonDrawingUtils.GetFrameBorderSize();
			if (_form.FormBorderStyle == FormBorderStyle.FixedDialog || _form.FormBorderStyle == FormBorderStyle.FixedSingle || _form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.Fixed3D)
			{
				size = SystemInformation.FixedFrameBorderSize;
			}
			if (!_form.ControlBox && _form.Text.Length == 0)
			{
				captionHeight = 0;
			}
			int rawCaptionHeight = captionHeight;
			_frameBorderSize = size;
			_captionHeight = captionHeight;
			GetTopBorderHeight();
			if (captionHeight < 26)
			{
				captionHeight = 26;
			}
			int width = _form.Width;
			int height = _form.Height;
			int clientWidth = _form.Width - size.Width - size.Width;
			int clientHeight = _form.Height - rawCaptionHeight - size.Height - size.Height;
			int widthDelta = clientWidth - (_form.Width - GetBorderWidth() - GetBorderWidth());
			int heightDelta = clientHeight - (_form.Height - captionHeight - GetTopBorderHeight() - GetBottomBorderHeight());
			width += widthDelta;
			height += heightDelta;
			if (width != _form.Size.Width || height != _form.Size.Height)
			{
				_isResizingForm = true;
				_form.Size = new Size(width, height);
				_isResizingForm = false;
				_hasResizedForm = true;
			}
		}

		internal void InitializeControls(object sender, EventArgs eventArgs)
		{
			if (GetBorderWidth() < 4)
			{
				SetBorderWidth(4);
			}
			_isDesignerLoading = false;
			if (!_runTimeForm)
			{
				_form.SuspendLayout();
				Size size = _form.Size;
				RecalculateFormSize();
				if (size == _form.Size)
				{
					base.OnFormResize((object)_form, EventArgs.Empty);
				}
				_form.ResumeLayout();
			}
			else
			{
				AdjustFormSizeForFrame(true, 2);
			}
		}

		internal override void Dispose()
		{
			base.Dispose();
			if (_form != null)
			{
				_form.VisibleChanged -= InitializeControls;
				_form.Resize -= base.OnFormResize;
			}
			_font?.Dispose();
			_font = null;
		}

		public Font GetTitleFont()
		{
			if (_font != null)
			{
				return _font;
			}
			if (_ribbon != null && _ribbon.TitleBarFont != null)
			{
				_font = new Font(_ribbon.TitleBarFont.FontFamily, SystemFonts.CaptionFont.Size, _ribbon.TitleBarFont.Style);
				return _font;
			}
			if (_formAdorner != null && _formAdorner.TitleBarFont != null)
			{
				_font = new Font(_formAdorner.TitleBarFont.FontFamily, SystemFonts.CaptionFont.Size, _formAdorner.TitleBarFont.Style);
				return _font;
			}
			return SystemFonts.CaptionFont;
		}

		internal void HandleNcActivate(ref Message message)
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

		private void RefreshNonClientLayout()
		{
			if (_form != null && !IsTopLevelMinimized())
			{
				PaintNonClientArea();
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
			else
			{
				base.WndProc(ref message);
			}
		}

		private void HandleNcLeftButtonDown(ref Message message)
		{
			if (_formAdorner != null && _formAdorner.IsInDesignMode())
			{
				SetPushedCaptionButton(null);
				base.WndProc(ref message);
				return;
			}
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point pt = PointToClient(screenPt);
			if (_minimizeButton._isVisible && _minimizeButton._isEnabled)
			{
				Rectangle bounds = _minimizeButton._bounds;
				if (bounds.Contains(pt))
				{
					SetPushedCaptionButton(_minimizeButton);
					if (_form.WindowState == FormWindowState.Minimized)
					{
						TrackCaptionButton(_minimizeButton, message.HWnd, (IntPtr)61728L);
					}
					else
					{
						TrackCaptionButton(_minimizeButton, message.HWnd, (IntPtr)61472L);
					}
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
						TrackCaptionButton(_maximizeButton, message.HWnd, (IntPtr)61728L);
					}
					else
					{
						TrackCaptionButton(_maximizeButton, message.HWnd, (IntPtr)61488L);
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
					TrackCaptionButton(_closeButton, message.HWnd, (IntPtr)61536L);
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
					TrackCaptionButton(_helpButton, message.HWnd, (IntPtr)61824L);
					message.Result = IntPtr.Zero;
					return;
				}
			}
			SetPushedCaptionButton(null);
			base.WndProc(ref message);
		}

		internal override void InvalidateNonClientArea()
		{
			if (_form != null && !_form.IsDisposed && !_form.Disposing)
			{
				try
				{
					DisposeResources();
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, NativeMethods.RedrawFrameInvalidate);
				}
				catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
				{
					// The form handle is gone during shutdown; there is nothing left to redraw.
				}
			}
		}

		internal override void Refresh(bool update)
		{
			if (_form != null && !_form.IsDisposed && !_form.Disposing)
			{
				try
				{
					DisposeResources();
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, NativeMethods.RedrawFrameInvalidate);
				}
				catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
				{
					// The form handle is gone during shutdown; there is nothing left to redraw.
				}
			}
		}

		private NativeMethods.NativeRect CalculateClientRect(NativeMethods.NativeRect windowRect)
		{
			SetBorderWidth(RibbonDrawingUtils.GetFrameBorderSize().Width);
			if (_form.FormBorderStyle == FormBorderStyle.FixedDialog || _form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.FixedSingle || _form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.Fixed3D)
			{
				SetBorderWidth(SystemInformation.FixedFrameBorderSize.Width);
			}
			if (GetBorderWidth() < 4 && !_isDesignerLoading)
			{
				SetBorderWidth(4);
			}
			if (_ribbon != null)
			{
				_ribbon.SetFrameBorderSize(new Size(GetBorderWidth(), GetBorderWidth()));
			}
			int borderWidth = GetBorderWidth();
			int titleHeight = SystemInformation.CaptionHeight;
			if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				titleHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			if (titleHeight < 26 && !_isDesignerLoading)
			{
				titleHeight = 26;
			}
			if (!_form.ControlBox && _form.Text.Length == 0 && !_isDesignerLoading)
			{
				titleHeight = 0;
			}
			int topOffset = (_isDesignerLoading ? GetBorderWidth() : GetTopBorderHeight());
			int bottomOffset = (_isDesignerLoading ? GetBorderWidth() : GetBottomBorderHeight());
			if ((_wasMaximized || _form.WindowState == FormWindowState.Maximized) && _form.IsMdiChild && windowRect._top < 0)
			{
				Size size = RibbonDrawingUtils.GetFrameBorderSize();
				windowRect._top = -(titleHeight + GetBorderWidth());
				windowRect._left = -GetBorderWidth();
				windowRect._right -= size.Width - GetBorderWidth();
				windowRect._bottom -= size.Height - GetBorderWidth();
			}
			_titleBarHeight = titleHeight;
			NativeMethods.NativeRect result = new NativeMethods.NativeRect
			{
				_left = windowRect._left + borderWidth,
				_top = windowRect._top + _titleBarHeight + topOffset,
				_right = windowRect._right - borderWidth,
				_bottom = windowRect._bottom - bottomOffset
			};
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
			if ((_wasMaximized || _form.WindowState == FormWindowState.Maximized) && _form.IsMdiChild)
			{
				return result;
			}
			return result;
		}

		private void HandleNcMouseMove(ref Message message)
		{
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point point = PointToClient(screenPt);
			if (_closeButton._isVisible && _closeButton._bounds.Contains(point))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
			}
			else if (_maximizeButton._isVisible && _maximizeButton._isEnabled && _maximizeButton._bounds.Contains(point))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
			}
			else if (_minimizeButton._isVisible && _minimizeButton._isEnabled && _minimizeButton._bounds.Contains(point))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
			}
			else if (_helpButton._isVisible && _helpButton._bounds.Contains(point))
			{
				StartMouseLeaveTracking();
				message.Result = IntPtr.Zero;
			}
			else if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseMove(point))
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
			if (_formWindowState != _form.WindowState && _form.WindowState == FormWindowState.Normal)
			{
				_formWindowState = _form.WindowState;
				AddOffsetHeight();
				RedrawFrame();
				return;
			}
			_formWindowState = _form.WindowState;
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
				NativeMethods.NativeRect adjustedRect = CalculateClientRect(field_);
				captionBounds = new Rectangle(0, 0, adjustedRect._right - adjustedRect._left, adjustedRect._bottom - adjustedRect._top);
				l._newWindowRect = adjustedRect;
				Marshal.StructureToPtr(l, message.LParam, fDeleteOld: false);
				message.Result = IntPtr.Zero;
			}
			SetCaptionBounds(captionBounds);
		}

		private void HandleNcHitTest(ref Message message)
		{
			if (_formAdorner != null && _formAdorner.IsInDesignMode())
			{
				message.Result = (IntPtr)2L;
				return;
			}
			Point screenPt = new Point(NativeMethods.ToInt32Unchecked(message.LParam));
			Point point = PointToClient(screenPt);
			if (_closeButton._isVisible && _closeButton._bounds.Contains(point))
			{
				SetHotCaptionButton(_closeButton);
				message.Result = (IntPtr)20L;
				return;
			}
			if (_maximizeButton._isVisible && _maximizeButton._isEnabled && _maximizeButton._bounds.Contains(point))
			{
				SetHotCaptionButton(_maximizeButton);
				message.Result = (IntPtr)9L;
				return;
			}
			if (_minimizeButton._isVisible && _minimizeButton._isEnabled && _minimizeButton._bounds.Contains(point))
			{
				SetHotCaptionButton(_minimizeButton);
				message.Result = (IntPtr)8L;
				return;
			}
			if (_helpButton._isVisible && _helpButton._bounds.Contains(point))
			{
				SetHotCaptionButton(_helpButton);
				message.Result = (IntPtr)21L;
				return;
			}
			SetHotCaptionButton(null);
			if (GetHeaderRenderer() != null && GetHeaderRenderer().HandleMouseMove(point))
			{
				message.Result = (IntPtr)19L;
				return;
			}
			if (_iconBounds.Contains(point))
			{
				message.Result = (IntPtr)3L;
				return;
			}
			int windowHeight = _form.ClientRectangle.Height + _titleBarHeight + GetTopBorderHeight();
			int windowWidth = _form.ClientRectangle.Width + GetBorderWidth();
			if (_form.WindowState != FormWindowState.Maximized && !_wasMaximized && (_form.FormBorderStyle == FormBorderStyle.Sizable || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow))
			{
				if (point.Y <= GetTopBorderHeight())
				{
					if (point.X <= 16)
					{
						message.Result = (IntPtr)13L;
					}
					else if (point.X >= windowWidth - 16)
					{
						message.Result = (IntPtr)14L;
					}
					else
					{
						message.Result = (IntPtr)12L;
					}
					return;
				}
				if (point.X <= GetBorderWidth())
				{
					if (point.Y <= 16)
					{
						message.Result = (IntPtr)13L;
					}
					else if (point.Y >= windowHeight - 16)
					{
						message.Result = (IntPtr)16L;
					}
					else
					{
						message.Result = (IntPtr)10L;
					}
					return;
				}
				if (point.X >= windowWidth)
				{
					if (point.Y <= 16)
					{
						message.Result = (IntPtr)14L;
					}
					else if (point.Y >= windowHeight - 16)
					{
						message.Result = (IntPtr)17L;
					}
					else
					{
						message.Result = (IntPtr)11L;
					}
					return;
				}
				if (point.Y >= windowHeight)
				{
					if (point.X <= 16)
					{
						message.Result = (IntPtr)16L;
					}
					else if (point.X >= windowWidth - 16)
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
			if (point.Y <= _titleBarHeight)
			{
				message.Result = (IntPtr)2L;
			}
			else if (point.X <= GetBorderWidth() || point.X >= windowWidth || point.Y >= windowHeight)
			{
				message.Result = (IntPtr)18L;
			}
			else
			{
				message.Result = (IntPtr)1L;
			}
		}

		private void LayoutTitleText(Graphics graphics, TextLayoutRenderer textLayoutRenderer)
		{
			if (!_titleAreaBounds.IsEmpty)
			{
				int documentWidth = (int)textLayoutRenderer.MeasureText(graphics, DocumentText, GetTitleFont(), 0, 0).Width;
				int applicationWidth = 0;
				if (ApplicationText.Length > 0)
				{
					applicationWidth = (int)textLayoutRenderer.MeasureText(graphics, ApplicationText, GetTitleFont(), 0, 0).Width;
				}
				if (documentWidth + applicationWidth > _titleAreaBounds.Width)
				{
					int ellipsisWidth = (int)textLayoutRenderer.MeasureText(graphics, "W", GetTitleFont(), 0, 0).Width;
					applicationWidth = ellipsisWidth;
					if (_titleAreaBounds.Width - documentWidth <= ellipsisWidth)
					{
						applicationWidth = ellipsisWidth;
						documentWidth = _titleAreaBounds.Width - applicationWidth;
					}
					else
					{
						applicationWidth = _titleAreaBounds.Width - documentWidth;
					}
					_documentTextBounds = new Rectangle(_titleAreaBounds.X, _titleAreaBounds.Y, documentWidth, _titleAreaBounds.Height);
					_applicationTextBounds = new Rectangle(_documentTextBounds.Right, _titleAreaBounds.Y, applicationWidth, _titleAreaBounds.Height);
				}
				else
				{
					_documentTextBounds = new Rectangle(_titleAreaBounds.X, _titleAreaBounds.Y, documentWidth, _titleAreaBounds.Height);
					_applicationTextBounds = new Rectangle(_documentTextBounds.Right, _titleAreaBounds.Y, applicationWidth, _titleAreaBounds.Height);
				}
			}
			_isTitleLayoutPending = false;
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
			int buttonX = clientRect.Right - size.Width + 4;
			int buttonY = _titleBarHeight - size.Height + 3;
			if (_form.IsMdiChild && _form.WindowState == FormWindowState.Minimized)
			{
				size = new Size(size.Width, 21);
				buttonX = clientRect.Right - size.Width;
				buttonY = 4;
			}
			else {
				buttonY--;
			}
			if (GetControlBox())
			{
				_closeButton._isVisible = true;
				_closeButton._bounds = new Rectangle(buttonX, buttonY, size.Width, size.Height);
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
				_maximizeButton._bounds = new Rectangle(buttonX, buttonY, size.Width, size.Height);
				buttonX -= size.Width;
				_minimizeButton._bounds = new Rectangle(buttonX, buttonY, size.Width, size.Height);
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
			_minimizeButton._isEnabled = _form.MinimizeBox;
			_maximizeButton._isEnabled = _form.MaximizeBox;
			if (ShouldShowHelpButton())
			{
				_helpButton._isVisible = true;
				_helpButton._bounds = new Rectangle(buttonX, buttonY, size.Width, size.Height);
				_helpButton._bounds.Inflate(0, -1);
			}
			else
			{
				_helpButton._isVisible = false;
				_helpButton._bounds = Rectangle.Empty;
			}
			if (_ribbon != null)
			{
				_ribbon.SetCaptionAreaWidth(buttonX);
			}
			buttonX += size.Width;
			if (GetHeaderRenderer() != null)
			{
				GetHeaderRenderer().SetBorderSize(new Size(GetBorderWidth(), GetBorderWidth()));
				GetHeaderRenderer().SetArea(new Rectangle(0, 0, buttonX - GetBorderWidth(), _titleBarHeight + GetTopBorderHeight()));
				_iconBounds = Rectangle.Empty;
				return;
			}
			_isTitleLayoutPending = true;
			int titleOffsetY = (_wasMaximized ? (-1) : 0);
			if (_form.IsMdiChild && _form.WindowState == FormWindowState.Minimized)
			{
				titleOffsetY = -3;
			}
			if (_form.ControlBox && _form.ShowIcon)
			{
				Size iconSize = new Size(_titleBarHeight - 8, _titleBarHeight - 8);
				switch (_form.FormBorderStyle)
				{
				case FormBorderStyle.FixedToolWindow:
				case FormBorderStyle.SizableToolWindow:
					_iconBounds = Rectangle.Empty;
					_titleAreaBounds = new Rectangle(clientRect.X + GetBorderWidth(), clientRect.Y + GetTopBorderHeight() + titleOffsetY, buttonX - GetBorderWidth() - clientRect.X, _titleBarHeight);
					break;
				case FormBorderStyle.FixedSingle:
				case FormBorderStyle.Fixed3D:
				case FormBorderStyle.FixedDialog:
					if (_form.Icon == null)
					{
						_iconBounds = Rectangle.Empty;
						_titleAreaBounds = new Rectangle(clientRect.X + GetBorderWidth(), clientRect.Y + GetTopBorderHeight() + titleOffsetY, buttonX - GetBorderWidth() - clientRect.X, _titleBarHeight);
					}
					else
					{
						_iconBounds = new Rectangle(clientRect.X + GetBorderWidth(), clientRect.Y + GetTopBorderHeight() + (_titleBarHeight - iconSize.Height) / 2 + titleOffsetY, iconSize.Width, iconSize.Height);
						_titleAreaBounds = new Rectangle(_iconBounds.Right + 4, clientRect.Y + GetTopBorderHeight() + titleOffsetY, buttonX - GetBorderWidth() - _iconBounds.Right - 4, _titleBarHeight);
					}
					break;
				default:
					_iconBounds = new Rectangle(clientRect.X + GetBorderWidth(), clientRect.Y + GetTopBorderHeight() + (_titleBarHeight - iconSize.Height) / 2 + titleOffsetY, iconSize.Width, iconSize.Height);
					_titleAreaBounds = new Rectangle(_iconBounds.Right + 4, clientRect.Y + GetTopBorderHeight() + titleOffsetY, buttonX - GetBorderWidth() - _iconBounds.Right - 4, _titleBarHeight);
					break;
				}
			}
			else
			{
				_iconBounds = Rectangle.Empty;
				_titleAreaBounds = new Rectangle(clientRect.X + GetBorderWidth(), clientRect.Y + GetTopBorderHeight() + titleOffsetY, buttonX - GetBorderWidth() - clientRect.X, _titleBarHeight);
			}
		}

		protected override void WndProc(ref Message message)
		{
			if (_ribbon == null)
			{
				IntPtr ribbonProp = NativeMethods.GetPropW(_form.Handle, "Mordex_Ribbon");
				if (ribbonProp == new IntPtr(1))
				{
					base.WndProc(ref message);
					return;
				}
			}
			if (_form != null && _form.IsMdiChild)
			{
				if (_form.Parent != null)
				{
					IntPtr panelManagerProp = NativeMethods.GetPropW(_form.Parent.Handle, "Mordex_PanelManager");
					if (panelManagerProp == new IntPtr(1))
					{
						base.WndProc(ref message);
						return;
					}
				}
				if (IsTopLevelMinimized())
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
					base.WndProc(ref message);
					break;
				}
				if (!_isSysCommandActive)
				{
					_wasMaximized = _form.WindowState == FormWindowState.Maximized;
				}
				if (!_isUpdatingNonClientArea)
				{
					_isUpdatingNonClientArea = true;
					RefreshNonClientLayout();
					_isUpdatingNonClientArea = false;
				}
				break;
			case NativeMethods.WmActivate:
				base.WndProc(ref message);
				InvalidateNonClientArea();
				break;
			case 546:
				base.WndProc(ref message);
				InvalidateNonClientArea();
				break;
			case NativeMethods.WmNcActivate:
				if (IsTopLevelMinimized())
				{
					base.WndProc(ref message);
					break;
				}
				if (!_form.IsMdiChild)
				{
					if (message.WParam == IntPtr.Zero)
					{
						message.Result = (IntPtr)1;
					}
					HandleNcActivate(ref message);
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
				HandleNcActivate(ref message);
				InvalidateNonClientArea();
				_suppressMdiChildActivation = false;
				break;
			case NativeMethods.WmNcLButtonUp:
				HandleNcLeftButtonUp(ref message);
				break;
			case NativeMethods.WmNcLButtonDown:
				HandleNcLeftButtonDown(ref message);
				break;
			case NativeMethods.WmNcHitTest:
				HandleNcHitTest(ref message);
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
				SetHotCaptionButton(null);
				break;
			case NativeMethods.WmSize:
				base.WndProc(ref message);
				InvalidateNonClientArea();
				break;
			case NativeMethods.WmSetCursor:
				base.WndProc(ref message);
				if (!VisualStyleInformation.IsEnabledByUser || !VisualStyleInformation.IsSupportedByOS)
				{
					InvalidateNonClientArea();
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
					break;
				case 61488:
					_wasMaximized = true;
					break;
				case 61456:
					_wasMaximized = false;
					base.WndProc(ref message);
					return;
				}
				base.OnFormResize((object)_form, EventArgs.Empty);
				_isSysCommandActive = true;
				base.WndProc(ref message);
				_isSysCommandActive = false;
				_form.Invalidate();
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
				break;
			default:
				base.WndProc(ref message);
				break;
			case 174:
				break;
			}
		}

		internal override void DrawHeader(Graphics graphics)
		{
			if (!_iconBounds.IsEmpty)
			{
				Rectangle rectangle = _iconBounds;
				rectangle.Inflate(-1, -1);
				RibbonDrawingUtils.DrawIcon(graphics, _form.Icon, rectangle, true, SystemColors.Window, false);
			}
			using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			textLayoutRenderer.SetWrapEnabled(false);
			if (_isTitleLayoutPending)
			{
				LayoutTitleText(graphics, textLayoutRenderer);
			}
			if (DocumentText.Length > 0 && _documentTextBounds.Width > 0)
			{
				Brush brush = GdiCache.Brush(ColorScheme._activeCaptionDocumentText);
				textLayoutRenderer.DrawTextWithBrush(graphics, DocumentText, _documentTextBounds, GetTitleFont(), brush);
			}
			if (ApplicationText.Length > 0 && _applicationTextBounds.Width > 0)
			{
				Brush applicationBrush = GdiCache.Brush(ColorScheme._activeCaptionApplicationText);
				textLayoutRenderer.DrawTextWithBrush(graphics, ApplicationText, _applicationTextBounds, GetTitleFont(), applicationBrush);
			}
		}

		private void TrackCaptionButton(RibbonLayoutItem button, IntPtr windowHandle, IntPtr sysCommand)
		{
			RibbonDrawingUtils.SetCapture(_form, true);
			button.Pushed = true;
			NativeMethods.NativeMessage n = default(NativeMethods.NativeMessage);
			bool isDone = false;
			while (!isDone)
			{
				NativeMethods.GetMessage(ref n, windowHandle, 512u, 525u);
				switch (n._message)
				{
				case 514:
				{
					RibbonDrawingUtils.SetCapture(_form, false);
					SetPushedCaptionButton(null);
					isDone = true;
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
