using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Tabs
{
	[ToolboxItem(true)]
	[Designer("Mordex.Windows.UI.Design.TabControlDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[DefaultEvent("SelectedTabChanged")]
	[DefaultProperty("TabPages")]
	public partial class MxTabControl : Control, IFormatStyleOwner, ISupportInitialize, ILayoutItemOwner, IMordexXmlLayoutsSupport, IPersistComponentSettings
	{
		private delegate void DeferredSizeChangedCallback(EventArgs eventArgs);

		private const int DefaultTabStripOffset = 2;

		private const int DefaultFirstTabOffset = 3;

		private const int MinTabOffset = 2;

		private static readonly Size DefaultImageSize = new Size(16, 16);

		private static readonly Size DefaultScrollButtonSize = new Size(16, 16);

		private static readonly Size DefaultControlSize = new Size(200, 100);

		// Extra length of a slanted Office 2003 or VS2005 tab when its text is not rotated.
		private const int OfficeTabOverlap = 5;

		private const int MinImageMargin = 3;

		// Repeat rate of a held scroll button, after the first delay.
		private const int ScrollRepeatInterval = 100;

		// State ids understood by the theme renderer when it draws a tab.
		private const int TabStateNormal = 1;

		private const int TabStateHot = 2;

		private const int TabStateSelected = 3;

		private const int TabStateDisabled = 4;

		// Part ids understood by the theme renderer: a tab at the start or end of the strip has its own look.
		private const int TabPartDefault = 1;

		private const int TabPartFirst = 2;

		private const int TabPartLast = 3;

		// A themed page is drawn inside the theme border.
		private const int ThemedPageShrink = 4;

		private const int ThemedPageFarOffset = 3;

		private const float HotHighlightBlend = 0.1f;

		private const float HotWindowBlend = 0.33f;

		// A hot color brighter than this is darkened so it stays visible.
		private const float HotBrightnessLimit = 0.85f;

		private const float HotDarkenOffset = 0.45f;

		private const float SelectedWindowBlend = 0.5f;

		private Timer _scrollTimer;

		private Container _container = new Container();

		internal bool _isHorizontalStrip = true;

		internal FormatStyleLayoutState _scrollBackButton;

		internal FormatStyleLayoutState _scrollForwardButton;

		internal FormatStyleLayoutState _closeButton;

		private bool _isInitializing;

		private ApplicationSettings _applicationSettings;

		private bool _saveSettings;

		private string _settingsKey;

		private TabLayout _currentLayout;

		private TabLayoutCollection _layouts;

		private Stream _layoutStream;

		private TabPageCollection _tabPages;

		internal bool _isLayoutDirty = true;

		private PageBorder _pageBorder;

		private TabStripAlignment _tabStripAlignment;

		private TabDisplay _tabDisplay;

		private TextOrientation _textOrientation;

		private bool _multiLine;

		private bool _showToolTips = true;

		private int _tabStripOffset = DefaultTabStripOffset;

		private int _firstTabOffset = DefaultFirstTabOffset;

		private bool _showTabs = true;

		private int _selectedIndex = -1;

		private MxTabPage _inputFocusTab;

		private bool _showCloseButton;

		internal MxTabPage _selectedTab;

		private ImageList _imageList;

		private Size _imageSize = DefaultImageSize;

		private FormatStyle _tabStripFormatStyle;

		private FormatStyle _panelFormatStyle;

		private StateStyles _tabStateStyles;

		private int _tabThickness;

		private Size _itemSize = Size.Empty;

		private bool? _useCompatibleTextRendering;

		private TabSizeMode _sizeMode;

		internal MxCollection _pageRenderers;

		private int _rowThickness;

		private Rectangle _tabStripBounds = Rectangle.Empty;

		private ArrayList _tabRows;

		private Rectangle _pageDisplayBounds = Rectangle.Empty;

		private bool _showFocusRectangle = true;

		private Color _flatBorderColor = Color.Empty;

		internal bool _isTextHorizontal = true;

		private Size _scrollButtonSize = DefaultScrollButtonSize;

		private int _firstVisibleTabIndex = -1;

		private bool _isScrollBackPressed;

		private bool _isScrollForwardPressed;

		private bool _isCloseButtonPressed;

		private bool _isScrollForwardHot;

		private bool _isScrollBackHot;

		private bool _isCloseButtonHot;

		private bool _focusOnClick = true;

		private WindowMessageHandler _themedToolTip;

		private ToolTip _toolTip;

		private TabPageRenderer _hotTabRenderer;

		private bool _isThemeRendered;

		private VisualStylePainter _themePainter;

		private VisualStylePainter _cachedThemePainter;

		private bool _sizeChangedPending;

		private bool _isFirstShow = true;

		private static readonly object _layoutLoadEvent = new object();

		private static readonly object _currentLayoutChangedEvent = new object();

		private static readonly object _currentLayoutChangingEvent = new object();

		private static readonly object _closingTabEvent = new object();

		private static readonly object _tabClosedEvent = new object();

		private static readonly object _selectedTabChangedEvent = new object();

		private static readonly object _changingSelectedTabEvent = new object();

		public MxTabControl()
		{
			SetDefaultSize();
			InitializeCollections();
			SetStyle(ControlStyles.ContainerControl, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			_isInitializing = false;
			if (_selectedTab == null && HasVisibleTabs())
			{
				SelectedTab = _tabPages[0];
			}
			else
			{
				UpdateDisplayRectangle();
			}
		}

		void IFormatStyleOwner.PropertyChange()
		{
			ResetTabSizes();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_saveSettings && !base.DesignMode)
				{
					SaveComponentSettings();
				}
				StopScrollTimer();
				DisposeToolTip();
				ReleaseThemePainter();
				if (_container != null)
				{
					_container.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		// The first repeat waits as long as a double click so a single click scrolls only once.
		private void StartScrollTimer()
		{
			if (_scrollTimer == null)
			{
				_scrollTimer = new Timer();
				_scrollTimer.Tick += OnTimerTick;
			}
			_scrollTimer.Interval = SystemInformation.DoubleClickTime;
			_scrollTimer.Start();
		}

		private void StopScrollTimer()
		{
			Timer timer = _scrollTimer;
			if (timer != null)
			{
				_scrollTimer = null;
				timer.Stop();
				timer.Tick -= OnTimerTick;
				timer.Dispose();
			}
		}

		// The renderer opens a theme handle and builds pens and colors, so it is kept
		// between paints and rebuilt only when one of the inputs below changes.
		private VisualStylePainter GetThemePainter()
		{
			if (_cachedThemePainter == null)
			{
				ReleaseThemePainter();
				_cachedThemePainter = new VisualStylePainter("Tab");
			}
			return _cachedThemePainter;
		}

		private void ReleaseThemePainter()
		{
			if (_cachedThemePainter != null)
			{
				if (_themePainter == _cachedThemePainter)
				{
					_themePainter = null;
				}
				_cachedThemePainter.DisposeResources();
				_cachedThemePainter = null;
			}
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			ReleaseThemePainter();
			base.OnSystemColorsChanged(e);
		}

		private void SetDefaultSize()
		{
			base.Size = DefaultControlSize;
		}

		internal void AddTabPageControl(MxTabPage page)
		{
			if (base.Container != null && page.Container == null)
			{
				base.Container.Add(page);
			}
			if (!Controls.Contains(page))
			{
				Controls.Add(page);
			}
			bool shouldReplaceHidden = _selectedTab != null && !_selectedTab.TabVisible && page.TabVisible;
			if (page._isSelected || _tabPages.Count == 1 || shouldReplaceHidden)
			{
				if (!_isInitializing)
				{
					SelectedTab = page;
					page.Visible = true;
					if (_isLayoutDirty && base.IsHandleCreated)
					{
						LayoutTabsNow();
					}
				}
			}
			else
			{
				page.Visible = false;
			}
			Invalidate(invalidateChildren: true);
		}

		internal void ChangeSelection(MxTabPage page)
		{
			if (page.Selected)
			{
				for (int i = page._index + 1; i < _tabPages.Count; i++)
				{
					MxTabPage nextPage = _tabPages[i];
					if (nextPage.TabVisible)
					{
						SelectedTab = nextPage;
						return;
					}
				}
				for (int index = page._index - 1; index >= 0; index--)
				{
					MxTabPage previousPage = _tabPages[index];
					if (previousPage.Visible)
					{
						SelectedTab = previousPage;
						break;
					}
				}
			}
			else
			{
				SelectedTab = page;
			}
		}

		internal void RemoveTabPageControl(MxTabPage page)
		{
			if (page.Selected)
			{
				if (TabPages.Count > 0)
				{
					SelectedTab = FindNextEnabledTab(page._index, true);
				}
				else
				{
					_selectedTab = null;
				}
			}
			if (base.Container != null && page.Container != null)
			{
				page.Container.Remove(page);
			}
			if (!Controls.Contains(page))
			{
				return;
			}
			try
			{
				if (page.ContainsFocus)
				{
					Focus();
				}
				Controls.Remove(page);
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// The page may already be disposed while the control tears down; removing it is best effort.
			}
		}

		private MxTabPage FindNextEnabledTab(int startIndex, bool canFallBack)
		{
			for (int i = startIndex; i < _tabPages.Count; i++)
			{
				MxTabPage page = _tabPages[i];
				if (page.TabVisible && page.Enabled)
				{
					return page;
				}
			}
			if (canFallBack)
			{
				if (TabPages.Count > startIndex)
				{
					return _tabPages[startIndex];
				}
				return _tabPages[_tabPages.Count - 1];
			}
			return null;
		}

		private MxTabPage FindPreviousEnabledTab(int startIndex)
		{
			for (int index = startIndex; index >= 0; index--)
			{
				MxTabPage page = _tabPages[index];
				if (page.TabVisible && page.Enabled)
				{
					return page;
				}
			}
			return null;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			OnVisibleChangedForCaption();
			if (!base.Visible || !_isFirstShow)
			{
				return;
			}
			foreach (MxTabPage item in _tabPages)
			{
				if (!item.Selected)
				{
					item.Visible = true;
					item.Visible = false;
				}
			}
			_isFirstShow = false;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (base.Created && base.ClientRectangle.Width > 0 && base.ClientRectangle.Height > 0)
			{
				try
				{
					LayoutTabs(null);
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// The layout runs again on the next paint, so a failure while resizing is not fatal.
				}
			}
			InvalidateLayout();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if (base.IsHandleCreated && base.Visible)
			{
				// During a drag resize many size changes arrive at once; raise only one deferred event.
				if (!_sizeChangedPending)
				{
					_sizeChangedPending = true;
					DeferredSizeChangedCallback method = RaiseDeferredSizeChanged;
					BeginInvoke(method, e);
				}
			}
			else
			{
				base.OnSizeChanged(e);
			}
		}

		private void RaiseDeferredSizeChanged(EventArgs eventArgs)
		{
			_sizeChangedPending = false;
			if (!base.IsDisposed)
			{
				base.OnSizeChanged(eventArgs);
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			// A deferred size change posted to the old handle is lost, so do not wait for it.
			_sizeChangedPending = false;
			_themeHook?.Stop();
			_formWatch?.Stop();
			ReleaseCaptionTabs();
			base.OnHandleDestroyed(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			ResetTabSizes();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			OnEnabledChangedForCaption();
			InvalidateLayout();
		}

		public static string GetUIVersionString()
		{
			string localPath = typeof(MxTabControl).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			_scrollTimer.Interval = ScrollRepeatInterval;
			if (_isScrollBackPressed)
			{
				ScrollBack();
			}
			else if (_isScrollForwardPressed)
			{
				ScrollForward();
			}
		}
	}
}
