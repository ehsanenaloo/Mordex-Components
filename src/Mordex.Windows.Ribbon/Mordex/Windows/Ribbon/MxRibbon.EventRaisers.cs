using System;
using System.ComponentModel;
using System.Threading;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		internal void NotifyQuickAccessCommandAdded(CommandBase command, CommandCollection commandCollection)
		{
			if (_quickAccessCommands == commandCollection)
			{
				CommandEventArgs e = new CommandEventArgs(command);
				OnQuickAccessCommandAdded(e);
			}
		}

		internal void NotifyQuickAccessCommandRemoved(CommandBase command, CommandCollection commandCollection)
		{
			if (_quickAccessCommands == commandCollection)
			{
				CommandEventArgs e = new CommandEventArgs(command);
				OnQuickAccessCommandRemoved(e);
			}
		}

		protected virtual void OnCommandDropDownClosing(DropDownClosingEventArgs e)
		{
			if (base.Events[s_commandDropDownClosingKey] is DropDownClosingEventHandler dropDownClosingEventHandler)
			{
				dropDownClosingEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownClosed(CommandEventArgs e)
		{
			if (base.Events[s_commandDropDownClosedKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownOpening(CommandCancelEventArgs e)
		{
			if (base.Events[s_commandDropDownOpeningKey] is CommandCancelEventHandler commandCancelEventHandler)
			{
				commandCancelEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownOpened(CommandEventArgs e)
		{
			if (base.Events[s_commandDropDownOpenedKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandClick(CommandEventArgs e)
		{
			if (base.Events[s_commandClickKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnQuickAccessCommandAdded(CommandEventArgs e)
		{
			if (base.Events[s_quickAccessCommandAddedKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnQuickAccessCommandRemoved(CommandEventArgs e)
		{
			if (base.Events[s_quickAccessCommandRemovedKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCustomizeCommandClick(CustomizeCommandClickEventArgs e)
		{
			if (base.Events[s_customizeCommandClickKey] is CustomizeCommandClickEventHandler customizeCommandClickEventHandler)
			{
				customizeCommandClickEventHandler(this, e);
			}
		}

		protected virtual void OnLayoutLoad(EventArgs e)
		{
			if (base.Events[s_layoutLoadKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanged(EventArgs e)
		{
			if (base.Events[s_currentLayoutChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[s_currentLayoutChangingKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnSelectedTabChanged(TabEventArgs e)
		{
			if (base.Events[s_selectedTabChangedKey] is TabEventHandler tabEventHandler)
			{
				tabEventHandler(this, e);
			}
		}

		protected virtual void OnChangingSelectedTab(TabCancelEventArgs e)
		{
			if (base.Events[s_changingSelectedTabKey] is TabCancelEventHandler tabCancelEventHandler)
			{
				tabCancelEventHandler(this, e);
			}
		}

		protected virtual void OnMinimizedChanged(EventArgs e)
		{
			if (base.Events[s_minimizedChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnControlBoxOpening(CancelEventArgs e)
		{
			if (base.Events[s_controlBoxOpeningKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnControlBoxOpened(EventArgs e)
		{
			if (base.Events[s_controlBoxOpenedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnControlBoxClosing(CancelEventArgs e)
		{
			if (base.Events[s_controlBoxClosingKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnControlBoxClosed(EventArgs e)
		{
			if (base.Events[s_controlBoxClosedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnBackstageClosing(CancelEventArgs e)
		{
			if (base.Events[s_backstageClosingKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnBackstageClosed(EventArgs e)
		{
			if (base.Events[s_backstageClosedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnBackstageOpening(CancelEventArgs e)
		{
			if (base.Events[s_backstageOpeningKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnBackstageOpened(EventArgs e)
		{
			if (base.Events[s_backstageOpenedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal void AddContentRenderedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _contentRenderedHandler;
			EventHandler originalHandler;
			do
			{
				originalHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(originalHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _contentRenderedHandler, value, originalHandler);
			}
			while ((object)eventHandler != originalHandler);
		}

		internal void RemoveContentRenderedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _contentRenderedHandler;
			EventHandler originalHandler;
			do
			{
				originalHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(originalHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _contentRenderedHandler, value, originalHandler);
			}
			while ((object)eventHandler != originalHandler);
		}

		private void OnPopupDropDownCommand(object sender, CommandEventArgs eventArgs)
		{
			if (_dropDownCommand != null && _dropDownCommand != eventArgs.Command)
			{
				GetPopupMenu().OpenDropDown(_dropDownCommand);
				_dropDownCommand = null;
			}
			else if (_isOpeningKeyTipDropDown)
			{
				LoadParentControlKeyTips();
				_isOpeningKeyTipDropDown = false;
			}
		}

		private void OnContentSlideTimerTick(object sender, EventArgs eventArgs)
		{
			if (_contentAreaWidth > base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right)
			{
				_contentAreaWidth -= 10;
				if (_contentAreaWidth + _tabContentRenderer.GetScrollOffset() < base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right)
				{
					_contentAreaWidth = base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right;
					StopContentSlideTimer();
				}
			}
			else
			{
				_contentAreaWidth += 10;
				if (_contentAreaWidth + _tabContentRenderer.GetScrollOffset() > base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right)
				{
					_contentAreaWidth = base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right;
					StopContentSlideTimer();
				}
			}
			InvalidateTabContent();
			Update();
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			StopKeyTipTimer();
			if (!IsKeyTipMode() && !s_isKeyTipSessionActive)
			{
				SetKeyTipMode(true);
				_keyTipsShownByAltHold = true;
			}
		}

		private void OnMordexSuperTipToolTipPopUp(object sender, ToolTipPopUpEventArgs eventArgs)
		{
		}

		private void OnRibbonContextMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			if (eventArgs.Command.Tag != null && eventArgs.Command.Tag is CommandBase)
			{
				CommandBase commandBase = eventArgs.Command.Tag as CommandBase;
				if (eventArgs.Command.Key == "AddToQA")
				{
					CustomizeCommandClickEventArgs e = new CustomizeCommandClickEventArgs(CustomizeCommandButton.AddToQuickAccessBar);
					OnCustomizeCommandClick(e);
					if (e.DoDefaultAction)
					{
						_quickAccessCommands.Add(commandBase);
						if (commandBase.IsMerged() && _mergedRibbon != null)
						{
							_mergedRibbon.QuickAccessCommands.Add(commandBase);
						}
					}
					return;
				}
				if (eventArgs.Command.Key == "RemoveFromQA")
				{
					CustomizeCommandClickEventArgs e = new CustomizeCommandClickEventArgs(CustomizeCommandButton.RemoveFromQuickAccessBar);
					OnCustomizeCommandClick(e);
					if (e.DoDefaultAction)
					{
						_quickAccessCommands.Remove(commandBase);
						if (commandBase.IsMerged() && _mergedRibbon != null && _mergedRibbon.QuickAccessCommands.Contains(commandBase))
						{
							_mergedRibbon.QuickAccessCommands.Remove(commandBase);
						}
					}
					return;
				}
				if (eventArgs.Command is ButtonCommand)
				{
					if (((ButtonCommand)eventArgs.Command).Checked)
					{
						if (!_quickAccessCommands.Contains(commandBase))
						{
							_quickAccessCommands.Add(commandBase);
						}
					}
					else if (_quickAccessCommands.Contains(commandBase))
					{
						_quickAccessCommands.Remove(commandBase);
					}
				}
			}
			else if (eventArgs.Command.Key == "cmdQAPosition")
			{
				CustomizeCommandClickEventArgs e = new CustomizeCommandClickEventArgs(CustomizeCommandButton.QuickAccessBarPosition);
				OnCustomizeCommandClick(e);
				if (e.DoDefaultAction)
				{
					if (_quickAccessPosition == QuickAccessPosition.AboveRibbon)
					{
						QuickAccessPosition = QuickAccessPosition.BelowRibbon;
					}
					else
					{
						QuickAccessPosition = QuickAccessPosition.AboveRibbon;
					}
				}
			}
			else if (eventArgs.Command.Key == "cmdMinimize")
			{
				CustomizeCommandClickEventArgs e = new CustomizeCommandClickEventArgs(CustomizeCommandButton.MinimizeRibbon);
				OnCustomizeCommandClick(e);
				if (e.DoDefaultAction)
				{
					Minimized = !Minimized;
				}
			}
			else if (eventArgs.Command.Key == "cmdMoreCommands")
			{
				CustomizeCommandClickEventArgs e = new CustomizeCommandClickEventArgs(CustomizeCommandButton.MoreCommands);
				OnCustomizeCommandClick(e);
			}
			SetPopupMenu(null);
		}

		private void OnRibbonCommandClick(object sender, CommandEventArgs eventArgs)
		{
			if (GetPopupMenu() != null)
			{
				GetPopupMenu().TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
			}
		}
	}
}
