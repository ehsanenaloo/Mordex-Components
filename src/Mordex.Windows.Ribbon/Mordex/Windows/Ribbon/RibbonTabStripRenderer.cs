using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabStripRenderer : IDisposable
	{
		private MxRibbon _ribbon;

		private IList<RibbonTabRenderer> _tabRenderers;

		private int _scrollOffset;

		internal RibbonLayoutItem _leftScrollButton;

		internal RibbonLayoutItem _rightScrollButton;

		private bool _areTabRenderersDirty;

		private const int ScrollTickMilliseconds = 40;

		private Timer _timer;

		private bool _isRightScrollPressed;

		private bool _isLeftScrollPressed;

		private bool _isRightScrollHot;

		private bool _isLeftScrollHot;

		public Rectangle _stripBounds;

		private Rectangle _tabsBounds;

		private int _totalTabsWidth;

		private bool _isMousePressed;

		private Rectangle _tabsClipBounds;

		private int _defaultFirstTabOffset = 48;

		private RibbonTabRenderer _hotTabRenderer;

		private RibbonLayoutItem _pressedItem;

		internal RibbonMdiControlBox _mdiControlBox;

		private RibbonLayoutItem _minimizeButton;

		private RibbonLayoutItem _helpButton;

		private bool _areTabsCompressed;

		internal BackstageMenuTabRenderer _backstageTabRenderer;

		private AccessibleObject _cachedAccessibleObject;

		private CommandBaseEditorDialog _minimizedPopup;

		public RibbonTabStripRenderer(MxRibbon ribbon)
		{
			_ribbon = ribbon;
			_rightScrollButton = new CommandBaseRenderer(ribbon);
			_leftScrollButton = new CommandBaseRenderer(ribbon);
			_minimizeButton = new RibbonLayoutItem(ribbon);
			_helpButton = new CommandBaseRenderer(ribbon);
			_minimizeButton._isVisible = false;
			_helpButton._isVisible = false;
			_leftScrollButton._isVisible = false;
			_rightScrollButton._isVisible = false;
			_areTabRenderersDirty = true;
			_tabRenderers = new List<RibbonTabRenderer>();
		}

		public MxRibbon GetRibbon()
		{
			return _ribbon;
		}

		public int GetScrollOffset()
		{
			return _scrollOffset;
		}

		public void SetScrollOffset(int ribbonRenderer21Integer)
		{
			if (ribbonRenderer21Integer > 0)
			{
				ribbonRenderer21Integer = 0;
			}
			if (_scrollOffset != ribbonRenderer21Integer)
			{
				_scrollOffset = ribbonRenderer21Integer;
				LayoutTabsAtScrollOffset();
				_ribbon.Invalidate();
			}
		}

		public IList<RibbonTabRenderer> GetTabRenderers()
		{
			return _tabRenderers;
		}

		public void SetHotTabRenderer(RibbonTabRenderer ribbonTabRenderer)
		{
			if (_hotTabRenderer != ribbonTabRenderer)
			{
				if (_hotTabRenderer != null)
				{
					_hotTabRenderer.Hot = false;
				}
				_hotTabRenderer = ribbonTabRenderer;
				if (_hotTabRenderer != null)
				{
					_hotTabRenderer.Hot = true;
				}
			}
		}

		public bool IsLeftScrollPressed()
		{
			return _isLeftScrollPressed;
		}

		public void SetLeftScrollPressed(bool isPressed)
		{
			if (_isLeftScrollPressed != isPressed)
			{
				_isLeftScrollPressed = isPressed;
				_leftScrollButton.Pushed = isPressed;
				if (isPressed)
				{
					StartTimer(SystemInformation.DoubleClickTime);
					SetScrollOffset(GetScrollOffset() + 5);
				}
				else
				{
					StopTimer();
				}
			}
		}

		public void SetLeftScrollHot(bool isHot)
		{
			if (_isLeftScrollHot == isHot)
			{
				return;
			}
			_isLeftScrollHot = isHot;
			_leftScrollButton.Hot = isHot;
			_ribbon.Invalidate(_leftScrollButton._bounds);
			if (isHot)
			{
				if (IsLeftScrollPressed() && (_timer == null || !_timer.Enabled))
				{
					StartTimer(ScrollTickMilliseconds);
				}
			}
			else
			{
				StopTimer();
			}
		}

		public void SetRightScrollHot(bool isHot)
		{
			if (_isRightScrollHot == isHot)
			{
				return;
			}
			_isRightScrollHot = isHot;
			_rightScrollButton.Hot = isHot;
			_ribbon.Invalidate(_rightScrollButton._bounds);
			if (isHot)
			{
				if (IsRightScrollPressed() && (_timer == null || !_timer.Enabled))
				{
					StartTimer(ScrollTickMilliseconds);
				}
			}
			else
			{
				StopTimer();
			}
		}

		public bool IsRightScrollPressed()
		{
			return _isRightScrollPressed;
		}

		public void SetRightScrollPressed(bool ribbonRenderer21Boolean)
		{
			if (_isRightScrollPressed != ribbonRenderer21Boolean)
			{
				_isRightScrollPressed = ribbonRenderer21Boolean;
				_rightScrollButton.Pushed = ribbonRenderer21Boolean;
				if (ribbonRenderer21Boolean)
				{
					StartTimer(SystemInformation.DoubleClickTime);
					SetScrollOffset(GetScrollOffset() - 5);
				}
				else
				{
					StopTimer();
				}
			}
		}

		public bool IsHelpButtonHot()
		{
			return _helpButton.Hot;
		}

		public void SetHelpButtonHot(bool isHot)
		{
			if (_helpButton.Hot != isHot)
			{
				_helpButton.Hot = isHot;
				InvalidateRegion(_helpButton._bounds);
			}
		}

		public bool IsHelpButtonPressed()
		{
			return _helpButton.Pushed;
		}

		public void SetHelpButtonPressed(bool isPressed)
		{
			if (_helpButton.Pushed != isPressed)
			{
				_helpButton.Pushed = isPressed;
				InvalidateRegion(_helpButton._bounds);
			}
		}

		public bool IsMinimizeButtonHot()
		{
			return _minimizeButton.Hot;
		}

		public void SetMinimizeButtonHot(bool isHot)
		{
			if (_minimizeButton.Hot != isHot)
			{
				_minimizeButton.Hot = isHot;
				InvalidateRegion(_minimizeButton._bounds);
			}
		}

		public bool IsMinimizeButtonPressed()
		{
			return _minimizeButton.Pushed;
		}

		public void SetMinimizeButtonPressed(bool isPressed)
		{
			if (_minimizeButton.Pushed != isPressed)
			{
				_minimizeButton.Pushed = isPressed;
				InvalidateRegion(_minimizeButton._bounds);
			}
		}

		public void Dispose()
		{
			StopTimer();
			StopItemAnimations();
		}

		private void StopItemAnimations()
		{
			if (_tabRenderers == null)
			{
				return;
			}
			foreach (RibbonTabRenderer item in _tabRenderers)
			{
				item.StopAnimation();
			}
		}

		private void StopTimer()
		{
			Timer timer = _timer;
			if (timer == null)
			{
				return;
			}
			_timer = null;
			timer.Stop();
			timer.Tick -= OnTimerTick;
			timer.Dispose();
		}

		private void StartTimer(int intervalMs)
		{
			if (_timer == null)
			{
				_timer = new Timer();
				_timer.Tick += OnTimerTick;
			}
			_timer.Interval = intervalMs;
			_timer.Start();
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_timer == null)
			{
				return;
			}
			_timer.Interval = ScrollTickMilliseconds;
			if (_isLeftScrollPressed)
			{
				SetScrollOffset(GetScrollOffset() + 5);
			}
			else if (_isRightScrollPressed)
			{
				SetScrollOffset(GetScrollOffset() - 5);
			}
		}

		public RibbonTabRenderer FindTabRenderer(RibbonTab ribbonTab)
		{
			if (_tabRenderers != null)
			{
				foreach (RibbonTabRenderer item in _tabRenderers)
				{
					if (item.GetTab() == ribbonTab)
					{
						return item;
					}
				}
			}
			return null;
		}
	}
}
