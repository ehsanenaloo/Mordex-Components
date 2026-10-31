using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal partial class CommandControl
	{
		private void StartSubmenuTimer()
		{
			int delay = (_overflowItems != null && _overflowItems.ContainsItem(_selectedItem)) ? OverflowSubmenuDelay : SubmenuDelay;
			if (_submenuTimer == null)
			{
				_submenuTimer = new Timer();
				_submenuTimer.Tick += OnSubmenuTimerTick;
			}
			// Setting the interval restarts a running timer, so only set it when it changes.
			if (_submenuTimer.Interval != delay)
			{
				_submenuTimer.Interval = delay;
			}
			_submenuTimer.Start();
		}

		private void StartChildCloseTimer(int interval)
		{
			if (_childCloseTimer == null)
			{
				_childCloseTimer = new Timer();
				_childCloseTimer.Tick += OnChildCloseTimerTick;
			}
			if (_childCloseTimer.Interval != interval)
			{
				_childCloseTimer.Interval = interval;
			}
			_childCloseTimer.Start();
		}

		private void StartHoverTimer(int interval)
		{
			if (_hoverTimer == null)
			{
				_hoverTimer = new Timer();
				_hoverTimer.Tick += OnHoverTimerTick;
			}
			else
			{
				_hoverTimer.Stop();
			}
			_hoverTimer.Interval = interval;
			_hoverTimer.Start();
		}

		private static void DisposeTimer(Timer timer, EventHandler tickHandler)
		{
			if (timer == null)
			{
				return;
			}
			timer.Stop();
			timer.Tick -= tickHandler;
			timer.Dispose();
		}

		private void StopChildCloseTimer()
		{
			Timer timer = _childCloseTimer;
			_childCloseTimer = null;
			DisposeTimer(timer, OnChildCloseTimerTick);
		}

		private void StopSubmenuTimer()
		{
			Timer timer = _submenuTimer;
			_submenuTimer = null;
			DisposeTimer(timer, OnSubmenuTimerTick);
		}

		private void StopExpandAnimation()
		{
			Timer timer = _expandTimer;
			_expandTimer = null;
			DisposeTimer(timer, OnExpandTimerTick);
		}

		private void StopHoverTimer()
		{
			Timer timer = _hoverTimer;
			_hoverTimer = null;
			DisposeTimer(timer, OnHoverTimerTick);
		}

		public ImageList GetImageList(MxCommand cmd)
		{
			return _owner.GetImageList(cmd);
		}

		public CommandLayoutState GetOpenItem()
		{
			return _openItem;
		}

		public void SetOpenItem(CommandLayoutState uiCommandLayoutState)
		{
			if (_openItem != uiCommandLayoutState)
			{
				if (_openItem != null)
				{
					_openItem.SetPressed(false);
				}
				_openItem = uiCommandLayoutState;
				if (_openItem != null)
				{
					StopSubmenuTimer();
					_openItem.SetPressed(true);
				}
			}
		}

		internal MxCommand GetActiveCommand()
		{
			if (_childMenu != null && _childMenu.GetActiveCommand() != null)
			{
				return _childMenu.GetActiveCommand();
			}
			if (_selectedItem != null)
			{
				return _selectedItem.GetCommand();
			}
			return null;
		}

		internal CommandLayoutState GetSelectedItem()
		{
			return _selectedItem;
		}

		internal void SelectItem(CommandLayoutState item)
		{
			if (_selectedItem == item)
			{
				return;
			}
			if (_expandAfterDelay)
			{
				StopHoverTimer();
				_expandAfterDelay = false;
			}
			if (_expandOnHover)
			{
				StopHoverTimer();
				StartHoverTimer(_fullMenuDelay);
			}
			if (_selectedItem != null)
			{
				HideToolTip();
				if (_selectedItem.GetCommand().Control != null && _selectedItem.GetCommand().Control.ContainsFocus)
				{
					return;
				}
				_selectedItem.SetHot(false);
				if (item != null)
				{
					StopSubmenuTimer();
				}
				_selectedItem.GetCommand().NotifyMouseLeave();
			}
			_selectedItem = item;
			if (_selectedItem == null)
			{
				return;
			}
			if (_selectedItem.GetCommand().ToolTipText != null && _selectedItem.GetCommand().ToolTipText.Length > 0)
			{
				ShowToolTip(_selectedItem.GetCommand().ToolTipText);
			}
			if (ChildMenu != null && item != null)
			{
				if (ChildMenu._parentItem != item)
				{
					StartChildCloseTimer(SubmenuDelay);
				}
				else
				{
					StopChildCloseTimer();
				}
			}
			if (_selectedItem != GetOpenItem())
			{
				if (ChildMenu != null && _childMenu._parentItem == _selectedItem)
				{
					SetOpenItem(_selectedItem);
				}
				else
				{
					SetOpenItem(null);
				}
			}
			if (_selectedItem.GetCommand() != null)
			{
				_selectedItem.GetCommand().NotifyMouseHover();
			}
			_selectedItem.SetHot(true);
			_selectedItem._accessibleRole = AccessibleRole.MenuItem;
			NotifyAccessibilityClients(AccessibleEvents.Focus, _selectedItem._index);
		}

		internal void HideToolTip()
		{
			if (_toolTipWindow != null)
			{
				_toolTipWindow.HideAndDisposeTimers();
				_toolTipWindow = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
		}

		private bool ShouldShowToolTips()
		{
			if (_owner != null)
			{
				return _owner.ShowToolTips;
			}
			return false;
		}

		internal void ShowExpandToolTip()
		{
			if (ShouldShowToolTips())
			{
				string text = GetCommandManagerOwner().BuiltInTexts[CommandsBuiltInText.ExpandToolTipText];
				ShowToolTip(text);
			}
		}

		internal void ShowToolTip(string caption)
		{
			if (!ShouldShowToolTips())
			{
				return;
			}
			if (VisualStyleSupport.IsToolTipHookSupported())
			{
				if (_toolTipWindow == null)
				{
					_toolTipWindow = new WindowMessageHandler(Font, ShouldUseCompatibleTextRendering());
					_toolTipWindow.RequestShow(caption, this);
				}
				else
				{
					_toolTipWindow.UpdateTipText(caption);
				}
				return;
			}
			if (_toolTip == null)
			{
				_toolTip = new Mordex.Windows.Common.ThemedToolTip();
				_toolTip.ShowAlways = true;
			}
			_toolTip.SetToolTip(this, caption);
		}

		private FormatStyleLayoutState GetHotButton()
		{
			return _hotButton;
		}

		private void SetHotButton(FormatStyleLayoutState button)
		{
			if (_hotButton == button)
			{
				return;
			}
			if (_hotButton != null)
			{
				_hotButton.SetHot(false);
				StopHoverTimer();
				HideToolTip();
			}
			_hotButton = button;
			if (_hotButton == null)
			{
				return;
			}
			_expandOnHover = false;
			if (_hotButton._caption != "FloatingBar")
			{
				int hoverDelay = ScrollHoverDelay;
				if (_hotButton == _expandBar)
				{
					ShowExpandToolTip();
					hoverDelay = ExpandBarHoverDelay;
					_hotButton.SetHot(true);
					if (_fullMenuDelay <= 0)
					{
						hoverDelay = 0;
					}
				}
				if (hoverDelay > 0)
				{
					StartHoverTimer(hoverDelay);
				}
				_hotButton.SetHot(true);
			}
			else
			{
				_hotButton.SetHot(true);
			}
		}

		protected override void WndProc(ref Message message)
		{
			if (!_holdMessages)
			{
				base.WndProc(ref message);
				return;
			}
			switch (message.Msg)
			{
			case WmChar:
				SelectByMnemonic((char)(int)message.WParam);
				break;
			case WmActivateApp:
				if (!_activateOnShow)
				{
					TryCloseMenu(CommandCloseReason.AppFocusChange);
				}
				break;
			case WmMouseActivate:
				message.Result = (IntPtr)MaNoActivate;
				break;
			default:
				base.WndProc(ref message);
				break;
			}
		}

		private void OnExpandTimerTick(object sender, EventArgs eventArgs)
		{
			if (_expandTimer.Interval == ExpandTimerStartInterval)
			{
				_expandStep = ExpandStartStep;
				_expandTimer.Interval = ExpandTimerInterval;
			}
			else
			{
				_expandStep *= ExpandStepFactor;
			}
			_expandHeight += _expandStep;
			InvalidateLayout();
		}

		private void OnHoverTimerTick(object sender, EventArgs eventArgs)
		{
			if (_hotButton != null)
			{
				if (_hotButton._caption == "InfrequentItemsBar")
				{
					ExpandInfrequentItems();
				}
				else if (_hotButton._caption == "BottomRebar")
				{
					ScrollDown();
				}
				else if (_hotButton._caption == "TopRebar")
				{
					ScrollUp();
				}
			}
			else if (_expandAfterDelay)
			{
				ExpandInfrequentItems();
			}
			else if (_expandOnHover)
			{
				if (_isMouseOnly)
				{
					ExpandInfrequentItems();
				}
				else if (_expandBar != null)
				{
					_expandBar.SetHot(true);
				}
				_expandOnHover = false;
			}
		}

		private void OnChildCloseTimerTick(object sender, EventArgs eventArgs)
		{
			StopChildCloseTimer();
			if (_childMenu != null)
			{
				Point pt = _childMenu.PointToClient(Cursor.Position);
				if (!_childMenu.ClientRectangle.Contains(pt))
				{
					TryCloseChildMenu(CommandCloseReason.CommandFocused);
				}
			}
		}

		private void OnSubmenuTimerTick(object sender, EventArgs eventArgs)
		{
			StopSubmenuTimer();
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			if (_selectedItem != null && _selectedItem.GetCommand().IsParent)
			{
				try
				{
					OpenChildMenu(_selectedItem, false);
				}
				catch (Exception ex) when (ex is ExternalException || ex is InvalidOperationException)
				{
					// The menu can close while the timer fires. An exception here would reach the message loop.
				}
				return;
			}
			TryCloseChildMenu(CommandCloseReason.CommandFocused);
		}

		internal void OpenChildMenu(CommandLayoutState parentItem, bool selectFirstItem)
		{
			if (!base.IsDisposed && !base.Disposing && base.IsHandleCreated && (ChildMenu == null || ChildMenu._parentItem.GetCommand() != parentItem.GetCommand()) && TryCloseChildMenu(CommandCloseReason.CommandFocused))
			{
				PopupSettings baseSettings = _baseSettings.MergeWith(parentItem.GetCommand().GetEffectivePopupSettings(), null);
				CommandControl uiCommandControl = new CommandControl(parentItem.GetCommand().Commands, parentItem, this, baseSettings, _holdMessages);
				uiCommandControl._dropsDown = false;
				uiCommandControl._isFlipped = false;
				ChildMenu = uiCommandControl;
				uiCommandControl._selectFirstItem = selectFirstItem;
				uiCommandControl.ShowAt(PointToScreen(parentItem._bounds.Location));
			}
		}

		internal void ShowCommandPath(MxCommand uiCommand, Point point)
		{
			_holdMessages = false;
			ShowAt(point);
			CommandLayoutState uiCommandLayoutState = _layoutItems.FindCommandItem(uiCommand);
			if (uiCommandLayoutState != null)
			{
				_holdMessages = true;
				OpenChildMenu(uiCommandLayoutState, true);
				return;
			}
			foreach (CommandLayoutState item in _layoutItems)
			{
				if (!item.GetCommand().IsSeparator() && ContainsCommandKey(uiCommand.Key, item.GetCommand()))
				{
					uiCommandLayoutState = item;
					break;
				}
			}
			if (uiCommandLayoutState != null && TryCloseChildMenu(CommandCloseReason.CommandFocused))
			{
				PopupSettings baseSettings = _baseSettings.MergeWith(uiCommandLayoutState.GetCommand().GetEffectivePopupSettings(), null);
				CommandControl uiCommandControl = new CommandControl(uiCommandLayoutState.GetCommand().Commands, uiCommandLayoutState, this, baseSettings, _holdMessages);
				uiCommandControl._dropsDown = false;
				ChildMenu = uiCommandControl;
				uiCommandControl._selectFirstItem = true;
				uiCommandControl.ShowCommandPath(uiCommand, PointToScreen(uiCommandLayoutState._bounds.Location));
			}
		}

		internal bool ContainsCommandKey(string key, MxCommand uiCommand)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxCommand command in uiCommand.Commands)
			{
				if (string.Compare(key, command.Key, ignoreCase: true, invariantCulture) == 0)
				{
					return true;
				}
				if (ContainsCommandKey(key, command))
				{
					return true;
				}
			}
			return false;
		}

		private void ScrollDown()
		{
			if (_canScrollDown)
			{
				_alignItemsToTop = false;
				_firstVisibleIndex++;
				InvalidateLayout();
			}
		}

		private void ScrollUp()
		{
			if (_firstVisibleIndex > 0)
			{
				_alignItemsToTop = true;
				_firstVisibleIndex--;
				InvalidateLayout();
			}
		}

		private void ExpandInfrequentItems()
		{
			if (_owner == null || _owner.ShowInfrequentItems || !TryCloseChildMenu(CommandCloseReason.CommandFocused))
			{
				return;
			}
			if (_canAnimate)
			{
				_isExpanding = true;
				_expandTimer = new Timer();
				_expandTimer.Interval = ExpandTimerStartInterval;
				_expandTimer.Tick += OnExpandTimerTick;
				if (_owner != null)
				{
					_owner.ShowInfrequentItems = true;
				}
				_expandHeight = 1;
				_expandTimer.Enabled = true;
				InvalidateLayout();
			}
			else
			{
				if (_selectedItem != null && _selectedItem.GetCommand().Control != null && _selectedItem.GetCommand().Control.ContainsFocus)
				{
					_pendingSelectedCommand = _selectedItem.GetCommand();
					_selectedItem = null;
				}
				if (_owner != null)
				{
					_owner.ShowInfrequentItems = true;
				}
				_expandHeight = -1;
				InvalidateLayout();
			}
		}

		internal void InvalidateLayout()
		{
			_isLayoutDirty = true;
			Invalidate(invalidateChildren: true);
			if (ChildMenu != null)
			{
				ChildMenu.InvalidateLayout();
			}
		}

		void ILayoutItemOwner.DoDefaultAction(FormatStyleLayoutState item)
		{
		}

		void ILayoutItemOwner.Invalidate(Rectangle rect)
		{
			Invalidate(rect);
		}

		void ILayoutItemOwner.AccessibleStateChanged(FormatStyleLayoutState item)
		{
			if (_layoutItems != null)
			{
				int childID = _layoutItems.IndexOfItem(item);
				AccessibilityNotifyClients(AccessibleEvents.StateChange, childID);
			}
		}
	}
}
