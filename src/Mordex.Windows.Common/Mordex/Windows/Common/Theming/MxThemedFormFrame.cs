using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>Whether a form takes the window frame of the default theme.</summary>
	public enum FormFrameMode
	{
		/// <summary>The caption and the frame follow the default theme.</summary>
		FollowTheme,
		/// <summary>The frame of Windows, with no change.</summary>
		System
	}

	/// <summary>
	/// Gives a form the window frame of the default theme: a blue caption with rounded corners for Windows XP, a 3D frame and a
	/// gradient caption for Windows Classic, a glass or gradient caption for Office 2007 and 2010, and the caption color and
	/// dark or light title bar for the flat themes. The frame follows the default theme while it changes. Setting
	/// <see cref="Mode"/> to <see cref="FormFrameMode.System"/>, or disposing the component, gives the form its own frame back.
	/// </summary>
	[ToolboxItem(true)]
	[DefaultProperty("Mode")]
	public class MxThemedFormFrame : Component, IThemeListener
	{
		private Form _form;
		private FormFrameMode _mode = FormFrameMode.FollowTheme;
		private FrameWindow _window;
		private FormFrameStyle _style = FormFrameStyle.System;
		private MxTheme _theme;
		private bool _active = true;
		private FrameButton _hot;
		private FrameButton _pressed;
		private bool _trackingLeave;
		private bool _listening;
		private Bitmap _iconBitmap;
		private int _iconDpi;
		private Icon _iconSource;
		private Font _font;
		private int _fontDpi;
		private FormFrameKind _fontKind;
		// The window that has the round region of the frame, and its geometry. Zero when no window has it.
		private IntPtr _regionHandle;
		private int _regionRadius;
		private Size _regionSize;
		private IFrameCaptionExtension _extension;
		private FrameShadow _shadow;
		// The extension that got the press of the gesture in progress, or null.
		private IFrameCaptionExtension _gesture;
		private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Form, MxThemedFormFrame> Frames = new System.Runtime.CompilerServices.ConditionalWeakTable<Form, MxThemedFormFrame>();

		// Raised on the thread of the form after a frame took a style for the form or let the form go. The sender is the form.
		// A control that draws into the caption may be ready before the frame of its form, so it binds when it hears this.
		internal static event EventHandler FrameApplied;

		/// <summary>Raised with the form after the caption slot became free, so a control that waited for it can take it.</summary>
		internal static event EventHandler CaptionSlotVacated;

		public MxThemedFormFrame()
		{
		}

		public MxThemedFormFrame(IContainer container)
		{
			container?.Add(this);
		}

		[DefaultValue(null)]
		[Description("The form that gets the themed caption and frame.")]
		public Form Form
		{
			get => _form;
			set
			{
				if (_form == value)
				{
					return;
				}
				Detach();
				_form = value;
				if (_form != null && !DesignMode)
				{
					_form.HandleCreated += OnFormHandleCreated;
					_form.HandleDestroyed += OnFormHandleDestroyed;
					_form.Disposed += OnFormDisposed;
					Listen(true);
					if (_form.IsHandleCreated)
					{
						Apply();
					}
				}
			}
		}

		[DefaultValue(FormFrameMode.FollowTheme)]
		[Description("Follow theme paints the frame to match the default theme. System leaves the frame of Windows alone.")]
		public FormFrameMode Mode
		{
			get => _mode;
			set
			{
				if (_mode == value)
				{
					return;
				}
				_mode = value;
				if (_form != null && _form.IsHandleCreated)
				{
					Apply();
				}
			}
		}

		/// <summary>The frame style that is in use now. It is <see cref="FormFrameStyle.System"/> when the mode is System or the form cannot take the frame.</summary>
		/// <summary>The frame that paints the caption of a form, or null when the form has none.</summary>
		public static MxThemedFormFrame FromForm(Form form)
		{
			return form != null && Frames.TryGetValue(form, out MxThemedFormFrame frame) ? frame : null;
		}

		/// <summary>
		/// Content that another control draws into the caption. Set it to null to remove it. The caption has one such slot: the
		/// extension that had it ends its gesture and hears <see cref="IFrameCaptionExtension.CaptionReleased"/>.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IFrameCaptionExtension CaptionExtension
		{
			get => _extension;
			set
			{
				if (ReferenceEquals(_extension, value))
				{
					return;
				}
				IFrameCaptionExtension previous = _extension;
				if (previous != null && ReferenceEquals(_gesture, previous))
				{
					CancelCaptionGesture();
				}
				_extension = value;
				if (_window != null && _form != null && _form.IsHandleCreated)
				{
					FrameNative.FrameChanged(_form.Handle);
				}
				previous?.CaptionReleased();
				if (value == null && previous != null && _form != null)
				{
					CaptionSlotVacated?.Invoke(_form, EventArgs.Empty);
				}
			}
		}

		/// <summary>True while a press on the caption extension is held.</summary>
		[Browsable(false)]
		public bool IsCaptionGestureActive => _gesture != null;

		/// <summary>Ends the press on the caption extension without a release, for example because its owner was disabled.</summary>
		public void CancelCaptionGesture()
		{
			IFrameCaptionExtension owner = _gesture;
			if (owner == null)
			{
				return;
			}
			_gesture = null;
			if (_pressed == FrameButton.None && _form != null && _form.IsHandleCreated && FrameNative.GetCapture() == _form.Handle)
			{
				FrameNative.ReleaseCapture();
			}
			owner.GestureCanceled();
		}

		/// <summary>The screen point as a point in window coordinates.</summary>
		public Point ScreenToWindow(Point screen)
		{
			if (_form == null || !_form.IsHandleCreated)
			{
				return screen;
			}
			Rectangle window = FrameNative.GetWindowRectangle(_form.Handle);
			return new Point(screen.X - window.X, screen.Y - window.Y);
		}

		/// <summary>The rectangle in window coordinates as a rectangle on the screen.</summary>
		public Rectangle WindowToScreen(Rectangle bounds)
		{
			if (_form == null || !_form.IsHandleCreated)
			{
				return bounds;
			}
			Rectangle window = FrameNative.GetWindowRectangle(_form.Handle);
			return new Rectangle(bounds.X + window.X, bounds.Y + window.Y, bounds.Width, bounds.Height);
		}

		/// <summary>Paints the caption again, for example after the extension changed its look.</summary>
		public void InvalidateCaption()
		{
			RepaintCaption();
		}

		[Browsable(false)]
		public FormFrameStyle CurrentStyle => _style;

		/// <summary>True while the component paints the caption and the frame itself.</summary>
		[Browsable(false)]
		public bool IsPainting => _window != null;

		/// <summary>The parts of the frame in window coordinates, or null when the component does not paint the frame.</summary>
		/// <summary>The parts of the frame as they are without the width that a caption extension takes. An extension uses this to place itself.</summary>
		public FormFrameMetrics GetMetricsWithoutExtension()
		{
			if (_window == null || !_form.IsHandleCreated)
			{
				return null;
			}
			return ComputeMetrics(false);
		}

		public FormFrameMetrics GetMetrics()
		{
			if (_window == null || !_form.IsHandleCreated)
			{
				return null;
			}
			return ComputeMetrics();
		}

		void IThemeListener.OnThemeChanged()
		{
			if (_form != null && _form.IsHandleCreated && !_form.IsDisposed)
			{
				Apply();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Detach();
				_font?.Dispose();
				_font = null;
				_iconBitmap?.Dispose();
				_iconBitmap = null;
			}
			base.Dispose(disposing);
		}

		private void Listen(bool on)
		{
			if (on && !_listening)
			{
				MxThemeManager.AddListener(this);
				_listening = true;
			}
			else if (!on && _listening)
			{
				MxThemeManager.RemoveListener(this);
				_listening = false;
			}
		}

		private void Detach()
		{
			if (_form == null)
			{
				return;
			}
			_form.HandleCreated -= OnFormHandleCreated;
			_form.HandleDestroyed -= OnFormHandleDestroyed;
			_form.Disposed -= OnFormDisposed;
			Listen(false);
			CancelCaptionGesture();
			DisposeShadow();
			if (_form.IsHandleCreated && !_form.IsDisposed)
			{
				Release(true);
			}
			// A newer frame of the same form keeps its own registration.
			if (Frames.TryGetValue(_form, out MxThemedFormFrame registered) && ReferenceEquals(registered, this))
			{
				Frames.Remove(_form);
			}
			IFrameCaptionExtension extension = _extension;
			_extension = null;
			extension?.CaptionReleased();
			Form form = _form;
			_form = null;
			FrameApplied?.Invoke(form, EventArgs.Empty);
		}

		private void OnFormHandleCreated(object sender, EventArgs e)
		{
			Apply();
		}

		private void OnFormHandleDestroyed(object sender, EventArgs e)
		{
			CancelCaptionGesture();
			DisposeShadow();
			_window = null;
			ForgetRegion();
		}

		private void OnFormDisposed(object sender, EventArgs e)
		{
			_window = null;
			Listen(false);
		}

		private bool CanPaintFrame()
		{
			if (_form == null || _form.IsMdiChild || _form.MdiParent != null || DesignMode)
			{
				return false;
			}
			switch (_form.FormBorderStyle)
			{
			case FormBorderStyle.None:
				return false;
			}
			// A ribbon that paints the caption itself marks the window.
			return FrameNative.GetProp(_form.Handle, "Mordex_Ribbon") == IntPtr.Zero;
		}

		private void Apply()
		{
			if (_form == null || !_form.IsHandleCreated)
			{
				return;
			}
			MxTheme theme = MxThemeManager.DefaultTheme;
			_theme = theme;
			FormFrameStyle wanted = _mode == FormFrameMode.System || !CanPaintFrame() ? FormFrameStyle.System : FormFrameStyle.For(theme);
			bool wasPainting = _window != null;
			bool shouldPaint = wanted.IsCustom;
			_style = wanted;
			if (shouldPaint)
			{
				Frames.AddOrUpdate(_form, this);
				if (!wasPainting)
				{
					_window = new FrameWindow(this);
					_window.AssignHandle(_form.Handle);
					FrameNative.SetNonClientRendering(_form.Handle, true);
					FrameNative.SetCaptionStyle(_form.Handle, false);
				}
				ApplyNativeAttributes(theme, true);
				ThemeWindow.Apply(_form, theme);
				RefreshFrame(true);
				UpdateShadow();
			}
			else
			{
				if (wasPainting)
				{
					Release(false);
				}
				if (wanted.Kind == FormFrameKind.Native)
				{
					ApplyNativeAttributes(theme, false);
					ThemeWindow.Apply(_form, theme);
				}
				else
				{
					ResetNativeAttributes();
					ThemeWindow.Apply(_form, theme);
				}
			}
			_extension?.FrameChanged();
			FrameApplied?.Invoke(_form, EventArgs.Empty);
		}

		private void Release(bool final)
		{
			CancelCaptionGesture();
			if (_window != null)
			{
				ForgetRegion();
				FrameNative.SetWindowRgn(_form.Handle, IntPtr.Zero, true);
				FrameNative.SetNonClientRendering(_form.Handle, false);
				FrameNative.SetCaptionStyle(_form.Handle, _form.FormBorderStyle != FormBorderStyle.None);
				_window.ReleaseHandle();
				_window = null;
				FrameNative.FrameChanged(_form.Handle);
			}
			// After the window procedure is gone, so no later position message makes the shadow again.
			DisposeShadow();
			if (final)
			{
				ResetNativeAttributes();
			}
		}

		private void ApplyNativeAttributes(MxTheme theme, bool custom)
		{
			IntPtr handle = _form.Handle;
			if (custom)
			{
				FormFrameColors colors = FormFrameColors.Create(theme, _style.Kind, true);
				FrameNative.SetCornerPreference(handle, 1);
				FrameNative.SetColorAttribute(handle, FrameNative.BorderColor, colors.FrameOuter);
				return;
			}
			FrameNative.SetCornerPreference(handle, 0);
			Color caption = theme.GetColor(ThemeRole.Caption);
			FrameNative.SetColorAttribute(handle, FrameNative.CaptionColor, caption);
			FrameNative.SetColorAttribute(handle, FrameNative.TextColor, MxTheme.Luminance(caption) > 0.55 ? Color.Black : Color.White);
			FrameNative.SetColorAttribute(handle, FrameNative.BorderColor, theme.GetColor(ThemeRole.Border));
		}

		private void ResetNativeAttributes()
		{
			IntPtr handle = _form.Handle;
			FrameNative.SetCornerPreference(handle, 0);
			FrameNative.SetDefaultColor(handle, FrameNative.CaptionColor);
			FrameNative.SetDefaultColor(handle, FrameNative.TextColor);
			FrameNative.SetDefaultColor(handle, FrameNative.BorderColor);
		}

		private void RefreshFrame(bool resize)
		{
			if (_window == null || !_form.IsHandleCreated)
			{
				return;
			}
			if (resize)
			{
				FrameNative.FrameChanged(_form.Handle);
			}
			UpdateRegion();
			FrameNative.RedrawFrame(_form.Handle);
		}

		private void UpdateRegion()
		{
			IntPtr handle = _form.Handle;
			int radius = _style.TopRadius;
			Size size = FrameNative.GetWindowSize(handle);
			int scaled = radius <= 0 || FrameNative.IsZoomed(handle) ? 0 : (int)Math.Round(radius * FrameNative.GetDpi(handle) / 96f);
			// SetWindowRgn sends a position message, so a region that is already right must not be set again. A window that
			// does not have the region needs one only when the frame is round.
			bool current = _regionHandle == handle;
			if (current ? _regionRadius == scaled && (scaled == 0 || _regionSize == size) : scaled == 0)
			{
				return;
			}
			IntPtr region = scaled == 0 ? IntPtr.Zero : FrameNative.CreateTopRoundRegion(size.Width, size.Height, scaled);
			// The position message comes back here while the region is set, so the cache already names the new region. It
			// is dropped when the window does not take it.
			_regionHandle = scaled == 0 ? IntPtr.Zero : handle;
			_regionRadius = scaled;
			_regionSize = size;
			if (FrameNative.SetWindowRgn(handle, region, true) == 0)
			{
				// The window did not take the region, so it is still ours to delete.
				if (region != IntPtr.Zero)
				{
					FrameNative.DeleteObject(region);
				}
				ForgetRegion();
			}
		}

		/// <summary>The shadow of the custom frame, or null while the frame has none.</summary>
		internal FrameShadow Shadow => _shadow;

		private void UpdateShadow()
		{
			if (_window == null || _form == null || !_form.IsHandleCreated)
			{
				return;
			}
			if (_shadow == null)
			{
				_shadow = new FrameShadow();
			}
			_shadow.Update(_form.Handle);
		}

		private void DisposeShadow()
		{
			_shadow?.Dispose();
			_shadow = null;
		}

		private void ForgetRegion()
		{
			_regionHandle = IntPtr.Zero;
			_regionRadius = 0;
			_regionSize = Size.Empty;
		}

		private FormFrameMetrics ComputeMetrics(bool withExtension = true)
		{
			IntPtr handle = _form.Handle;
			int dpi = FrameNative.GetDpi(handle);
			bool maximized = FrameNative.IsZoomed(handle);
			bool mirrored = (FrameNative.GetExStyle(handle) & FrameNative.WsExLayoutRtl) != 0;
			return FormFrameMetrics.Compute(
				FrameNative.GetWindowSize(handle), dpi, FrameNative.GetOverhang(dpi), _style, maximized,
				_form.ControlBox, _form.ControlBox && _form.MinimizeBox, _form.ControlBox && _form.MaximizeBox,
				_form.ShowIcon && _form.Icon != null, mirrored, withExtension && _extension != null ? _extension.LeftInset : 0,
				withExtension && _extension is ICaptionIconKeeper keeper && keeper.KeepsIcon);
		}

		private ThemedFramePainter.State BuildState(FormFrameMetrics metrics)
		{
			IntPtr handle = _form.Handle;
			int dpi = FrameNative.GetDpi(handle);
			return new ThemedFramePainter.State
			{
				Theme = _theme ?? MxThemeManager.DefaultTheme,
				Style = _style,
				Metrics = metrics,
				Active = _active,
				Hot = _hot,
				Pressed = _pressed,
				Title = _form.Text,
				Icon = metrics.Icon.IsEmpty ? null : GetIcon(dpi),
				Font = GetFont(dpi),
				Mirrored = metrics.Mirrored,
				Maximized = metrics.Maximized,
				MinimizeEnabled = _form.MinimizeBox,
				MaximizeEnabled = _form.MaximizeBox,
				Dpi = dpi,
				Extension = _extension
			};
		}

		private Bitmap GetIcon(int dpi)
		{
			Icon icon = _form.Icon;
			if (icon == null)
			{
				return null;
			}
			if (_iconBitmap != null && ReferenceEquals(icon, _iconSource) && _iconDpi == dpi)
			{
				return _iconBitmap;
			}
			_iconBitmap?.Dispose();
			_iconBitmap = null;
			int size = (int)Math.Round(16 * dpi / 96f);
			try
			{
				using (Icon sized = new Icon(icon, new Size(size, size)))
				{
					_iconBitmap = sized.ToBitmap();
				}
			}
			catch (ArgumentException)
			{
				_iconBitmap = null;
			}
			_iconSource = icon;
			_iconDpi = dpi;
			return _iconBitmap;
		}

		private Font GetFont(int dpi)
		{
			if (_font != null && _fontDpi == dpi && _fontKind == _style.Kind)
			{
				return _font;
			}
			_font?.Dispose();
			string family = "Segoe UI";
			FontStyle style = FontStyle.Regular;
			float size = 9f;
			if (_style.Kind == FormFrameKind.Luna)
			{
				family = FontFamily.GenericSansSerif.Name;
				foreach (FontFamily f in FontFamily.Families)
				{
					if (f.Name == "Trebuchet MS")
					{
						family = f.Name;
						break;
					}
				}
				style = FontStyle.Bold;
				size = 10f;
			}
			else if (_style.Kind == FormFrameKind.Classic)
			{
				style = FontStyle.Bold;
				size = 8.25f;
			}
			// The font is made in points at the DPI of the window, so the painter's bitmap (set to that DPI) draws it at the right size.
			_font = new Font(family, size, style, GraphicsUnit.Point);
			_fontDpi = dpi;
			_fontKind = _style.Kind;
			return _font;
		}

		private void PaintWindow(IntPtr dc, bool captionOnly)
		{
			if (_form == null || !_form.IsHandleCreated)
			{
				return;
			}
			FrameNative.SetLayoutLtr(dc);
			FormFrameMetrics metrics = ComputeMetrics();
			ThemedFramePainter.State state = BuildState(metrics);
			Size window = FrameNative.GetWindowSize(_form.Handle);
			FrameNative.ExcludeClientFromClip(dc, metrics.Client);
			using (Graphics g = Graphics.FromHdc(dc))
			{
				if (captionOnly)
				{
					ThemedFramePainter.PaintCaptionOnly(g, state);
				}
				else
				{
					ThemedFramePainter.Paint(g, window, state);
				}
			}
		}

		private void RepaintCaption()
		{
			if (_form == null || !_form.IsHandleCreated || _window == null)
			{
				return;
			}
			IntPtr dc = FrameNative.GetWindowDC(_form.Handle);
			if (dc == IntPtr.Zero)
			{
				return;
			}
			try
			{
				PaintWindow(dc, true);
			}
			finally
			{
				FrameNative.ReleaseDC(_form.Handle, dc);
			}
		}

		private FrameButton ButtonAt(int hit)
		{
			switch (hit)
			{
			case FrameNative.HtMinButton:
				return FrameButton.Minimize;
			case FrameNative.HtMaxButton:
				return FrameButton.Maximize;
			case FrameNative.HtClose:
				return FrameButton.Close;
			default:
				return FrameButton.None;
			}
		}

		internal int HitTest(Point screen)
		{
			IntPtr handle = _form.Handle;
			Rectangle window = FrameNative.GetWindowRectangle(handle);
			Point p = new Point(screen.X - window.X, screen.Y - window.Y);
			if (!new Rectangle(Point.Empty, window.Size).Contains(p))
			{
				return FrameNative.HtNowhere;
			}
			FormFrameMetrics m = ComputeMetrics();
			if (m.Client.Contains(p))
			{
				return FrameNative.HtClient;
			}
			bool sizable = !m.Maximized && _form.WindowState == FormWindowState.Normal
				&& (_form.FormBorderStyle == FormBorderStyle.Sizable || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow);
			int dpi = FrameNative.GetDpi(handle);
			int edge = Math.Max(m.Border, (int)Math.Round(5 * dpi / 96f));
			int corner = (int)Math.Round(16 * dpi / 96f);
			if (sizable)
			{
				bool l = p.X < edge;
				bool r = p.X >= window.Width - edge;
				bool t = p.Y < (int)Math.Round(4 * dpi / 96f);
				bool b = p.Y >= window.Height - edge;
				bool nearLeftRight = p.Y < corner || p.Y >= window.Height - corner;
				bool nearTopBottom = p.X < corner || p.X >= window.Width - corner;
				if (t && (nearTopBottom || l || r))
				{
					return p.X < window.Width / 2 ? FrameNative.HtTopLeft : FrameNative.HtTopRight;
				}
				if (b && (nearTopBottom || l || r))
				{
					return p.X < window.Width / 2 ? FrameNative.HtBottomLeft : FrameNative.HtBottomRight;
				}
				if ((l || r) && nearLeftRight)
				{
					bool top = p.Y < corner;
					return l ? (top ? FrameNative.HtTopLeft : FrameNative.HtBottomLeft) : (top ? FrameNative.HtTopRight : FrameNative.HtBottomRight);
				}
				if (t)
				{
					return FrameNative.HtTop;
				}
				if (b)
				{
					return FrameNative.HtBottom;
				}
				if (l)
				{
					return FrameNative.HtLeft;
				}
				if (r)
				{
					return FrameNative.HtRight;
				}
			}
			if (m.CloseButton.Contains(p))
			{
				return FrameNative.HtClose;
			}
			if (m.MaximizeButton.Contains(p))
			{
				return FrameNative.HtMaxButton;
			}
			if (m.MinimizeButton.Contains(p))
			{
				return FrameNative.HtMinButton;
			}
			if (!m.Icon.IsEmpty && m.Icon.Contains(p))
			{
				return FrameNative.HtSysMenu;
			}
			if (_extension != null && _extension.HitTest(p))
			{
				return FrameNative.HtMenu;
			}
			if (m.Caption.Contains(p))
			{
				return FrameNative.HtCaption;
			}
			return FrameNative.HtBorder;
		}

		private Point ScreenToWindow(IntPtr lParam)
		{
			int x = (short)((long)lParam & 0xFFFF);
			int y = (short)(((long)lParam >> 16) & 0xFFFF);
			Rectangle window = FrameNative.GetWindowRectangle(_form.Handle);
			return new Point(x - window.X, y - window.Y);
		}

		// A captured message carries client coordinates. ClientToScreen also handles a mirrored client area.
		private Point ClientToWindow(IntPtr lParam)
		{
			Point point = new Point((short)((long)lParam & 0xFFFF), (short)(((long)lParam >> 16) & 0xFFFF));
			FrameNative.ClientToScreen(_form.Handle, ref point);
			Rectangle window = FrameNative.GetWindowRectangle(_form.Handle);
			return new Point(point.X - window.X, point.Y - window.Y);
		}

		private void BeginGesture(Point point)
		{
			CancelCaptionGesture();
			IFrameCaptionExtension owner = _extension;
			FrameNative.SetCapture(_form.Handle);
			_gesture = owner;
			owner.MouseDown(point);
		}

		private void EndGesture(Point point)
		{
			IFrameCaptionExtension owner = _gesture;
			_gesture = null;
			if (_pressed == FrameButton.None && FrameNative.GetCapture() == _form.Handle)
			{
				FrameNative.ReleaseCapture();
			}
			owner.MouseUp(point);
		}

		private void SetHot(FrameButton hot)
		{
			if (_hot != hot)
			{
				_hot = hot;
				RepaintCaption();
			}
		}

		private void TrackLeave()
		{
			if (!_trackingLeave)
			{
				_trackingLeave = true;
				FrameNative.TrackNonClientLeave(_form.Handle);
			}
		}

		private void RunCommand(FrameButton button)
		{
			IntPtr command;
			switch (button)
			{
			case FrameButton.Minimize:
				command = new IntPtr(FrameNative.ScMinimize);
				break;
			case FrameButton.Maximize:
				command = new IntPtr(FrameNative.IsZoomed(_form.Handle) ? FrameNative.ScRestore : FrameNative.ScMaximize);
				break;
			default:
				command = new IntPtr(FrameNative.ScClose);
				break;
			}
			FrameNative.PostMessage(_form.Handle, FrameNative.WmSysCommand, command, IntPtr.Zero);
		}

		private void ShowSystemMenu()
		{
			FormFrameMetrics m = ComputeMetrics();
			Rectangle window = FrameNative.GetWindowRectangle(_form.Handle);
			IntPtr menu = FrameNative.GetSystemMenu(_form.Handle, false);
			if (menu == IntPtr.Zero)
			{
				return;
			}
			int x = window.X + (m.Mirrored ? m.Icon.Right : m.Icon.X);
			int y = window.Y + m.Icon.Bottom;
			int command = FrameNative.TrackPopup(menu, m.Mirrored, x, y, _form.Handle);
			if (command != 0)
			{
				FrameNative.PostMessage(_form.Handle, FrameNative.WmSysCommand, new IntPtr(command), IntPtr.Zero);
			}
		}

		private void HandleMessage(ref Message m, FrameWindow window)
		{
			switch (m.Msg)
			{
			case FrameNative.WmNcCalcSize:
				if (FrameNative.IsIconic(m.HWnd))
				{
					window.Default(ref m);
					return;
				}
				CalcSize(ref m);
				return;
			case FrameNative.WmNcHitTest:
			{
				int x = (short)((long)m.LParam & 0xFFFF);
				int y = (short)(((long)m.LParam >> 16) & 0xFFFF);
				int hit = HitTest(new Point(x, y));
				if (hit == FrameNative.HtNowhere)
				{
					window.Default(ref m);
					return;
				}
				m.Result = new IntPtr(hit);
				return;
			}
			case FrameNative.WmNcPaint:
			{
				IntPtr dc = FrameNative.GetWindowDC(m.HWnd);
				if (dc != IntPtr.Zero)
				{
					try
					{
						PaintWindow(dc, false);
					}
					finally
					{
						FrameNative.ReleaseDC(m.HWnd, dc);
					}
				}
				m.Result = IntPtr.Zero;
				return;
			}
			case FrameNative.WmNcActivate:
				_active = m.WParam != IntPtr.Zero;
				if (!_active)
				{
					CancelCaptionGesture();
				}
				m.LParam = new IntPtr(-1);
				window.Default(ref m);
				RepaintFull();
				m.Result = new IntPtr(1);
				return;
			case FrameNative.WmGetMinMaxInfo:
				window.Default(ref m);
				FrameNative.FitMaximizedBounds(m.HWnd, m.LParam, _form.MaximumSize, FrameNative.GetOverhang(FrameNative.GetDpi(m.HWnd)));
				m.Result = IntPtr.Zero;
				return;
			case FrameNative.WmNcUahDrawCaption:
			case FrameNative.WmNcUahDrawFrame:
				m.Result = IntPtr.Zero;
				return;
			case FrameNative.WmPrint:
				if (((long)m.LParam & FrameNative.PrfNonClient) != 0)
				{
					PaintWindow(m.WParam, false);
					Rectangle client = ComputeMetrics().Client;
					m.LParam = new IntPtr((long)m.LParam & ~FrameNative.PrfNonClient);
					// The client is printed at the origin of the device context; move it below the caption.
					FrameNative.SetViewportOrgEx(m.WParam, client.X, client.Y, out Point previous);
					window.Default(ref m);
					FrameNative.SetViewportOrgEx(m.WParam, previous.X, previous.Y, out _);
					return;
				}
				window.Default(ref m);
				return;
			case FrameNative.WmSetText:
			case FrameNative.WmSetIcon:
				if (m.Msg == FrameNative.WmSetIcon)
				{
					_iconSource = null;
				}
			{
				bool visible = FrameNative.HasVisibleStyle(m.HWnd);
				if (visible)
				{
					FrameNative.SetVisibleStyle(m.HWnd, false);
				}
				window.Default(ref m);
				if (visible)
				{
					FrameNative.SetVisibleStyle(m.HWnd, true);
				}
				RepaintCaption();
				return;
			}
			case FrameNative.WmSize:
			case FrameNative.WmWindowPosChanged:
				window.Default(ref m);
				UpdateRegion();
				FrameNative.RedrawFrame(m.HWnd);
				UpdateShadow();
				return;
			case FrameNative.WmDwmNcRenderingChanged:
				// Windows applies the new frame rendering some time after it was asked for and can put a region of its own
				// on the window then, so the region of the frame is set again, a square frame included.
				window.Default(ref m);
				_regionHandle = m.HWnd;
				_regionRadius = -1;
				UpdateRegion();
				return;
			case FrameNative.WmStyleChanged:
				window.Default(ref m);
				FrameNative.RedrawFrame(m.HWnd);
				return;
			case FrameNative.WmDpiChanged:
				window.Default(ref m);
				_iconSource = null;
				FrameNative.FrameChanged(m.HWnd);
				return;
			case FrameNative.WmNcMouseMove:
			{
				if (_gesture != null)
				{
					_gesture.MouseMove(ScreenToWindow(m.LParam));
				}
				else if (_extension != null)
				{
					if ((int)m.WParam == FrameNative.HtMenu)
					{
						_extension.MouseMove(ScreenToWindow(m.LParam));
						TrackLeave();
					}
					else
					{
						_extension.MouseLeave();
					}
				}
				FrameButton button = ButtonAt((int)m.WParam);
				SetHot(button);
				if (button != FrameButton.None)
				{
					TrackLeave();
				}
				window.Default(ref m);
				return;
			}
			case FrameNative.WmNcMouseLeave:
				_trackingLeave = false;
				_extension?.MouseLeave();
				if (_pressed == FrameButton.None)
				{
					SetHot(FrameButton.None);
				}
				window.Default(ref m);
				return;
			case FrameNative.WmNcLButtonDown:
			{
				if (_extension != null && (int)m.WParam == FrameNative.HtMenu)
				{
					BeginGesture(ScreenToWindow(m.LParam));
					m.Result = IntPtr.Zero;
					return;
				}
				FrameButton button = ButtonAt((int)m.WParam);
				if (button != FrameButton.None)
				{
					_pressed = button;
					_hot = button;
					FrameNative.SetCapture(m.HWnd);
					RepaintCaption();
					m.Result = IntPtr.Zero;
					return;
				}
				if ((int)m.WParam == FrameNative.HtSysMenu)
				{
					ShowSystemMenu();
					m.Result = IntPtr.Zero;
					return;
				}
				window.Default(ref m);
				return;
			}
			case FrameNative.WmNcLButtonUp:
				// The capture sends the release of a gesture as a client message, so this comes only when the capture was
				// not taken or the message was sent directly. The release ends the gesture over any part of the frame.
				if (_gesture != null)
				{
					EndGesture(ScreenToWindow(m.LParam));
					m.Result = IntPtr.Zero;
					return;
				}
				if (_extension != null && (int)m.WParam == FrameNative.HtMenu)
				{
					m.Result = IntPtr.Zero;
					return;
				}
				window.Default(ref m);
				return;
			case FrameNative.WmNcLButtonDblClk:
				if (_extension != null && (int)m.WParam == FrameNative.HtMenu)
				{
					BeginGesture(ScreenToWindow(m.LParam));
					m.Result = IntPtr.Zero;
					return;
				}
				if ((int)m.WParam == FrameNative.HtSysMenu)
				{
					FrameNative.PostMessage(m.HWnd, FrameNative.WmSysCommand, new IntPtr(FrameNative.ScClose), IntPtr.Zero);
					m.Result = IntPtr.Zero;
					return;
				}
				window.Default(ref m);
				return;
			case FrameNative.WmMouseMove:
				if (_gesture != null)
				{
					_gesture.MouseMove(ClientToWindow(m.LParam));
					m.Result = IntPtr.Zero;
					return;
				}
				if (_pressed != FrameButton.None)
				{
					SetHot(ButtonAt(HitTest(Cursor.Position)) == _pressed ? _pressed : FrameButton.None);
				}
				window.Default(ref m);
				return;
			case FrameNative.WmLButtonUp:
				if (_gesture != null)
				{
					EndGesture(ClientToWindow(m.LParam));
					m.Result = IntPtr.Zero;
					return;
				}
				if (_pressed != FrameButton.None)
				{
					FrameButton pressed = _pressed;
					bool inside = ButtonAt(HitTest(Cursor.Position)) == pressed;
					_pressed = FrameButton.None;
					FrameNative.ReleaseCapture();
					_hot = inside ? pressed : FrameButton.None;
					RepaintCaption();
					if (inside)
					{
						RunCommand(pressed);
					}
					m.Result = IntPtr.Zero;
					return;
				}
				window.Default(ref m);
				return;
			case FrameNative.WmActivate:
				if (((long)m.WParam & 0xFFFF) == 0)
				{
					CancelCaptionGesture();
				}
				window.Default(ref m);
				return;
			case FrameNative.WmCancelMode:
				CancelCaptionGesture();
				window.Default(ref m);
				return;
			case FrameNative.WmCaptureChanged:
				if (_gesture != null && m.LParam != m.HWnd)
				{
					CancelCaptionGesture();
				}
				if (_pressed != FrameButton.None)
				{
					_pressed = FrameButton.None;
					_hot = FrameButton.None;
					RepaintCaption();
				}
				window.Default(ref m);
				return;
			default:
				window.Default(ref m);
				return;
			}
		}

		private void RepaintFull()
		{
			if (_window == null || !_form.IsHandleCreated)
			{
				return;
			}
			IntPtr dc = FrameNative.GetWindowDC(_form.Handle);
			if (dc == IntPtr.Zero)
			{
				return;
			}
			try
			{
				PaintWindow(dc, false);
			}
			finally
			{
				FrameNative.ReleaseDC(_form.Handle, dc);
			}
		}

		private void CalcSize(ref Message m)
		{
			IntPtr handle = m.HWnd;
			int dpi = FrameNative.GetDpi(handle);
			bool maximized = FrameNative.IsZoomed(handle);
			bool mirrored = (FrameNative.GetExStyle(handle) & FrameNative.WsExLayoutRtl) != 0;
			if (m.WParam != IntPtr.Zero)
			{
				FrameNative.Rect[] rects = FrameNative.ReadCalcSize(m.LParam);
				Rectangle proposed = rects[0].ToRectangle();
				FormFrameMetrics frame = FormFrameMetrics.Compute(proposed.Size, dpi, FrameNative.GetOverhang(dpi), _style, maximized, false, false, false, false, mirrored);
				rects[0] = FrameNative.Rect.From(new Rectangle(proposed.X + frame.Client.X, proposed.Y + frame.Client.Y, frame.Client.Width, frame.Client.Height));
				FrameNative.WriteCalcSize(m.LParam, rects);
			}
			else
			{
				FrameNative.Rect rect = Marshal.PtrToStructure<FrameNative.Rect>(m.LParam);
				Rectangle proposed = rect.ToRectangle();
				FormFrameMetrics frame = FormFrameMetrics.Compute(proposed.Size, dpi, FrameNative.GetOverhang(dpi), _style, maximized, false, false, false, false, mirrored);
				Marshal.StructureToPtr(FrameNative.Rect.From(new Rectangle(proposed.X + frame.Client.X, proposed.Y + frame.Client.Y, frame.Client.Width, frame.Client.Height)), m.LParam, false);
			}
			m.Result = IntPtr.Zero;
		}

		private sealed class FrameWindow : NativeWindow
		{
			private readonly MxThemedFormFrame _owner;

			internal FrameWindow(MxThemedFormFrame owner)
			{
				_owner = owner;
			}

			internal void Default(ref Message m)
			{
				base.WndProc(ref m);
			}

			protected override void WndProc(ref Message m)
			{
				_owner.HandleMessage(ref m, this);
			}
		}
	}
}
