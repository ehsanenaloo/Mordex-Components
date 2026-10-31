using System;
using System.Threading;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		internal ApplicationSettings GetApplicationSettings()
		{
			if (_mordexApplicationSettings == null)
			{
				_mordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			else if (string.Compare(_mordexApplicationSettings.SettingsKey, SettingsKey, ignoreCase: true) != 0)
			{
				_mordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			return _mordexApplicationSettings;
		}

		public void UnmergeRibbon()
		{
			SetMergedRibbon(null);
		}

		public void MergeRibbon(MxRibbon mergedRibbon)
		{
			SetMergedRibbon(mergedRibbon);
			_isAutoMerged = false;
		}

		internal void AddMergeChangingHandler(EventHandler handler)
		{
			EventHandler eventHandler = _mergeChangingHandler;
			EventHandler originalHandler;
			do
			{
				originalHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(originalHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _mergeChangingHandler, value, originalHandler);
			}
			while ((object)eventHandler != originalHandler);
		}

		internal void RemoveMergeChangingHandler(EventHandler handler)
		{
			EventHandler eventHandler = _mergeChangingHandler;
			EventHandler originalHandler;
			do
			{
				originalHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(originalHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _mergeChangingHandler, value, originalHandler);
			}
			while ((object)eventHandler != originalHandler);
		}

		internal void AddMergeChangedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _mergeChangedHandler;
			EventHandler originalHandler;
			do
			{
				originalHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(originalHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _mergeChangedHandler, value, originalHandler);
			}
			while ((object)eventHandler != originalHandler);
		}

		internal void RemoveMergeChangedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _mergeChangedHandler;
			EventHandler originalHandler;
			do
			{
				originalHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(originalHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _mergeChangedHandler, value, originalHandler);
			}
			while ((object)eventHandler != originalHandler);
		}

		internal void UndoMergedContent()
		{
			_mergedRibbon.CommandClick -= OnRibbonCommandClick;
			_ribbonImageList = null;
			_ribbonLargeImageList = null;
			_quickAccessCommands.UnmergeCommands();
			_quickAccessCustomizeList.UnmergeCommands();
			_contextualTabs.UndoMerge();
			Tabs.UndoMerge();
			_controlBoxMenu.LeftCommands.UnmergeCommands();
			_controlBoxMenu.RightCommands.UnmergeCommands();
			_controlBoxMenu.BottomCommands.UnmergeCommands();
			_backstageMenu.Commands.UnmergeCommands();
			if (!Tabs.Contains(_selectedTab))
			{
				RibbonTab ribbonTab = null;
				if (_selectedTab != null)
				{
					ribbonTab = Tabs.FindByKey(_selectedTab.Key);
				}
				if (ribbonTab != null)
				{
					SelectedTab = ribbonTab;
				}
				else if (Tabs.Count > 0)
				{
					SelectedTab = Tabs[0];
				}
				else
				{
					SelectedTab = null;
				}
			}
			if (_selectedTab == null || !_selectedTab.Visible)
			{
				SelectFirstVisibleTab();
			}
			if (_mergedRibbon._mergeHost == this)
			{
				_mergedRibbon.SetSourceMergeState(false);
				_mergedRibbon._mergeHost = null;
			}
			InvalidateFullLayout();
			if (_ribbonUnmergedHandler != null)
			{
				_ribbonUnmergedHandler(this, EventArgs.Empty);
			}
		}

		internal void ImportSourceRibbonContent(MxRibbon ribbon)
		{
			_ribbonImageList = ribbon.ImageList;
			_ribbonLargeImageList = ribbon.LargeImageList;
			_quickAccessCommands.MergeCommands(_mergedRibbon._quickAccessCommands);
			_quickAccessCustomizeList.MergeCommands(_mergedRibbon._quickAccessCustomizeList);
			_controlBoxMenu.LeftCommands.MergeCommands(_mergedRibbon._controlBoxMenu.LeftCommands);
			_controlBoxMenu.RightCommands.MergeCommands(_mergedRibbon._controlBoxMenu.RightCommands);
			_controlBoxMenu.BottomCommands.MergeCommands(_mergedRibbon._controlBoxMenu.BottomCommands);
			_contextualTabs.MergeFrom(_mergedRibbon._contextualTabs);
			_tabs.MergeTabs(_mergedRibbon.Tabs);
			_backstageMenu.Commands.MergeCommands(_mergedRibbon.BackstageMenu.Commands);
			if (!Tabs.Contains(_selectedTab) && _selectedTab != null)
			{
				RibbonTab ribbonTab = Tabs.FindByKey(_selectedTab.Key);
				if (ribbonTab != null)
				{
					_selectedTab = ribbonTab;
				}
				else if (Tabs.Count > 0)
				{
					_selectedTab = Tabs[0];
				}
				else
				{
					_selectedTab = null;
				}
			}
			if (_selectedTab == null || !_selectedTab.Visible)
			{
				SelectFirstVisibleTab();
			}
			_mergedRibbon.CommandClick += OnRibbonCommandClick;
			_mergedRibbon.SetSourceMergeState(true);
			InvalidateFullLayout();
			if (_ribbonMergedHandler != null)
			{
				_ribbonMergedHandler(this, EventArgs.Empty);
			}
		}

		private void SetSourceMergeState(bool isMerged)
		{
			if (isMerged)
			{
				_visibleBeforeMerge = base.Visible;
				ContextualTabs.RemoveMergedTabs();
				Tabs.StashMergedTabs();
				RequestTabStripLayout(true);
				if (!_tabs.Contains(_selectedTab))
				{
					SelectFirstVisibleTab();
				}
				else
				{
					_selectedTab.SetMarkedSelected(false);
				}
				if (_selectedTab == null)
				{
					SetTabStripHidden(true);
					if (GetHostForm() != null && GetHostForm().IsMdiChild)
					{
						base.Controls.Clear();
						base.Visible = false;
					}
					if (_ribbonWindowMessageHandler != null)
					{
						_ribbonWindowMessageHandler.RedrawFrame();
					}
				}
			}
			else
			{
				ContextualTabs.ReinsertRemovedTabs();
				Tabs.ReinsertRemovedTabs();
				RequestTabStripLayout(true);
				if (_selectedTab == null)
				{
					SelectFirstVisibleTab();
				}
				else
				{
					_selectedTab.SetMarkedSelected(true);
				}
				base.Visible = _visibleBeforeMerge;
				SetTabStripHidden(false);
			}
			RelayoutForFormState();
			InvalidateTabContent();
		}

		private void HandleSourceMergeChanged(object sender, EventArgs eventArgs)
		{
			SetMergedRibbon(_mergeSourceRibbon);
			_mergeSourceRibbon.RemoveMergeChangedHandler(HandleSourceMergeChanged);
			_mergeSourceRibbon = null;
		}
	}
}
