using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.StatusBar
{
	[DefaultProperty("Panels")]
	[DefaultEvent("PanelClick")]
	public partial class MxStatusBar : Control, IFormatStyleOwner, IMessageFilter
	{
		private const int WmActivate = 0x0006;

		private const int WmKeyUp = 0x0101;

		private const int WmNcHitTest = 0x0084;

		private const int WmActivateApp = 0x001C;

		private const int HtBottomRight = 17;

		private const int KeyStateCount = 256;

		private const int SizeGripWidth = 19;

		private const int PanelGap = 2;

		private const int ContentPadding = 4;

		private static readonly Size DefaultImageSize = new Size(16, 16);

		private Container _container = new Container();

		private bool _isLayoutDirty = true;

		private FormatStyleLayoutState _sizeGripState;

		private static readonly object _panelHoverKey = new object();

		private static readonly object _panelClickKey = new object();

		private static readonly object _panelDoubleClickKey = new object();

		private StatusBarPanelCollection _panels;

		private bool _needsKeyStateRefresh = true;

		private Form _form;

		private bool? _useCompatibleTextRendering;

		private FormatStyle _panelsFormatStyle;

		private FormatStyle _hotPanelsFormatStyle;

		private ImageList _imageList;

		private Size _imageSize = DefaultImageSize;

		private bool _isThemedRendering;

		private WindowMessageHandler _toolTipHandler;

		private ToolTip _toolTip;

		private bool _showToolTips = true;

		private MxStatusBarPanel _hotPanel;

		private Point _gripDragStart = Point.Empty;

		private Size _gripDragStartSize = Size.Empty;

		private Color _panelsBorderColor = Color.Empty;

		private VisualStylePainter _currentStylePainter;

		private VisualStylePainter _cachedStatusBarRenderer;

		private bool _messageFilterAdded;

		public MxStatusBar()
		{
			SetContainer();
			_panels = new StatusBarPanelCollection(this);
			Dock = DockStyle.Bottom;
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
		}

		protected override Size DefaultSize => new Size(200, Control.DefaultFont.Height + 8);

		// The filter keeps this control reachable from the application, so it is only
		// registered while there is a window that can repaint.
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (!_messageFilterAdded)
			{
				Application.AddMessageFilter(this);
				_messageFilterAdded = true;
			}
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			RemoveMessageFilter();
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}

		private void RemoveMessageFilter()
		{
			if (_messageFilterAdded)
			{
				Application.RemoveMessageFilter(this);
				_messageFilterAdded = false;
			}
		}

		private VisualStylePainter GetStatusBarRenderer()
		{
			if (_cachedStatusBarRenderer == null)
			{
				ReleaseStatusBarRenderer();
				_cachedStatusBarRenderer = new VisualStylePainter("Rebar");
			}
			return _cachedStatusBarRenderer;
		}

		private void ReleaseStatusBarRenderer()
		{
			if (_cachedStatusBarRenderer != null)
			{
				if (_currentStylePainter == _cachedStatusBarRenderer)
				{
					_currentStylePainter = null;
				}
				_cachedStatusBarRenderer.DisposeResources();
				_cachedStatusBarRenderer = null;
			}
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			ReleaseStatusBarRenderer();
			base.OnSystemColorsChanged(e);
		}

		bool IMessageFilter.PreFilterMessage(ref Message msg)
		{
			if (msg.Msg == WmActivate)
			{
				_needsKeyStateRefresh = true;
			}
			else if (msg.Msg == WmKeyUp)
			{
				switch ((int)msg.WParam)
				{
				case (int)Keys.CapsLock:
				case (int)Keys.Insert:
				case (int)Keys.NumLock:
				case (int)Keys.Scroll:
					_needsKeyStateRefresh = true;
					Invalidate();
					break;
				}
			}
			return false;
		}

		void IFormatStyleOwner.PropertyChange()
		{
			InvalidateLayout();
		}

		private void SetContainer()
		{
			_container = new Container();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_container != null)
				{
					_container.Dispose();
				}
				RemoveMessageFilter();
				DisposeResources();
				ReleaseStatusBarRenderer();
			}
			base.Dispose(disposing);
		}

		internal void RefreshToggleKeyStates()
		{
			foreach (MxStatusBarPanel item in _panels)
			{
				if (item.PanelType == StatusBarPanelType.ToggleKeyState)
				{
					byte[] keyStates = new byte[KeyStateCount];
					NativeMethods.GetKeyboardState(keyStates);
					switch (item.ToggleKeyValue)
					{
					case ToggleKeyValue.CapsLock:
						item.SetIsOn(keyStates[(int)Keys.CapsLock] != 0);
						break;
					case ToggleKeyValue.NumLock:
						item.SetIsOn(keyStates[(int)Keys.NumLock] != 0);
						break;
					case ToggleKeyValue.Insert:
						item.SetIsOn(keyStates[(int)Keys.Insert] != 0);
						break;
					case ToggleKeyValue.ScrollLock:
						item.SetIsOn(keyStates[(int)Keys.Scroll] != 0);
						break;
					}
				}
			}
			_needsKeyStateRefresh = false;
		}

		internal void SetParentForm(Form form)
		{
			if (_form != form)
			{
				if (_form != null)
				{
					_form.Activated -= OnFormActivated;
				}
				_form = form;
				if (_form != null)
				{
					_form.Activated += OnFormActivated;
				}
			}
		}

		private void OnFormActivated(object sender, EventArgs eventArgs)
		{
			_needsKeyStateRefresh = true;
			Invalidate();
		}

		private bool CanShowSizeGrip()
		{
			Control control = RenderingHelper.GetParentControl(this);
			if (control != null && control is Form)
			{
				Form form = control as Form;
				if (form.WindowState == FormWindowState.Normal && (form.FormBorderStyle == FormBorderStyle.Sizable || form.FormBorderStyle == FormBorderStyle.SizableToolWindow))
				{
					return true;
				}
			}
			return false;
		}

		internal bool GetIsThemedRendering()
		{
			return _isThemedRendering;
		}

		internal void SetIsThemedRendering(bool isThemed)
		{
			if (_isThemedRendering != isThemed)
			{
				_isThemedRendering = isThemed;
				InvalidateLayout();
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			InvalidateLayout();
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			Form form = RenderingHelper.FindForm(this);
			SetParentForm(form);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			InvalidateLayout();
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			MxStatusBarPanel panelAt = GetPanelAt();
			if (panelAt != null)
			{
				StatusBarEventArgs panelArgs = new StatusBarEventArgs(panelAt);
				OnPanelDoubleClick(panelArgs);
			}
		}

		internal void DisposeResources()
		{
			if (_toolTipHandler != null)
			{
				_toolTipHandler.HideAndDisposeTimers();
				_toolTipHandler = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
		}
	}
}
