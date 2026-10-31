using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		internal bool IsKeyTipMode()
		{
			return _isKeyTipMode;
		}

		internal void SetKeyTipMode(bool isKeyTipMode)
		{
			if (_isKeyTipMode == isKeyTipMode)
			{
				return;
			}
			_keyTipOwner = null;
			_keyTipInput = "";
			_isKeyTipMode = isKeyTipMode;
			if (_isKeyTipMode)
			{
				s_isKeyTipSessionActive = true;
				_checkFirstKeyTipChar = true;
				if (GetBackstageMenuScrollableBase() != null)
				{
					SetKeyboardFocusRenderer(_tabStripRenderer.GetTabRenderers()[0]);
					LoadBackstageKeyTips();
				}
				else if (_tabStripRenderer != null && _selectedTab != null)
				{
					SetKeyboardFocusRenderer(_tabStripRenderer.FindTabRenderer(_selectedTab));
					DisposeResources();
				}
				else
				{
					SetKeyboardFocusRenderer(null);
					ClearKeyTips();
				}
			}
			else
			{
				s_isKeyTipSessionActive = false;
				_checkFirstKeyTipChar = false;
				SetKeyboardFocusRenderer(null);
				ClearKeyTips();
			}
		}

		internal bool HandleKeyTipChar(char keyTipChar)
		{
			_keyTipInput += keyTipChar;
			if (_keyTips != null)
			{
				IList<RibbonKeyTipEntry> list = new List<RibbonKeyTipEntry>();
				foreach (RibbonKeyTipEntry item in _keyTips)
				{
					if (string.Compare(_keyTipInput, item.GetKeyTip(), ignoreCase: true) == 0)
					{
						_keyTipInput = "";
						if (item.IsEnabled())
						{
							ActivateKeyTipTarget(item.GetCommand(), item.GetRenderer());
						}
						else
						{
							SetKeyTipMode(false);
						}
						return true;
					}
					if (item.GetKeyTip().Length > _keyTipInput.Length)
					{
						string strA = item.GetKeyTip().Substring(0, _keyTipInput.Length);
						if (string.Compare(strA, _keyTipInput, ignoreCase: true) == 0)
						{
							list.Add(item);
						}
					}
				}
				if (list.Count == 0)
				{
					_keyTipInput = _keyTipInput.Remove(_keyTipInput.Length - 1);
				}
				else
				{
					SetKeyTips(list);
				}
			}
			return false;
		}

		internal void StepBackKeyTipLevel()
		{
			_keyTipInput = "";
			ClearKeyTips();
			if (_keyTipOwner != null)
			{
				if (_keyTipOwner is MxRibbon)
				{
					SetKeyTipMode(false);
				}
				else if (_keyTipOwner is RibbonTab || _keyTipOwner is BackstageMenu)
				{
					DisposeResources();
				}
				else if (_keyTipOwner is RibbonGroup)
				{
					if (GetPopupMenu() != null)
					{
						_keyTipOwner = SelectedTab;
						LoadParentControlKeyTips();
					}
					else
					{
						BuildKeyTips(SelectedTab);
					}
				}
				else
				{
					if (!(_keyTipOwner is CommandBase))
					{
						return;
					}
					ICommandContainer iAccessibleRibbonElement = ((CommandBase)_keyTipOwner).GetParentContainer();
					if (iAccessibleRibbonElement == null)
					{
						return;
					}
					if (iAccessibleRibbonElement is DropDownCommand)
					{
						LoadParentControlKeyTips();
						return;
					}
					if (iAccessibleRibbonElement == this)
					{
						if (GetPopupMenu() != null && GetPopupMenu() is ControlBoxMenuRenderer && _controlBoxButtonRenderer != null)
						{
							LoadParentControlKeyTips();
							return;
						}
						if (_selectedTab != null)
						{
							iAccessibleRibbonElement = _selectedTab;
						}
					}
					_keyTipOwner = iAccessibleRibbonElement;
					StepBackKeyTipLevel();
				}
			}
			else
			{
				SetKeyTipMode(false);
			}
		}

		private void ActivateKeyTipTarget(object owner, CommandBaseRenderer commandBaseRenderer)
		{
			ClearKeyTips();
			if (owner is BackstageTab)
			{
				_backstageMenu.SelectedTab = owner as BackstageTab;
			}
			else if (owner is RibbonTab)
			{
				RibbonTab ribbonTab = (SelectedTab = (RibbonTab)owner);
				SetKeyboardFocusRenderer(_tabStripRenderer.FindTabRenderer(ribbonTab));
				if (Minimized)
				{
					_isOpeningKeyTipDropDown = true;
					_keyTipOwner = ribbonTab;
					try
					{
						_tabStripRenderer.ShowMinimizedRibbonPopup();
					}
					finally
					{
						_isOpeningKeyTipDropDown = false;
					}
					return;
				}
				if (_isTabStripLayoutDirty)
				{
					RenderRibbonImmediately();
				}
				if (_isTabContentDirty)
				{
					RenderTabContentImmediately();
				}
				if (_backstageMenuScrollableBase == null)
				{
					BuildKeyTips(ribbonTab);
				}
				else
				{
					LoadBackstageKeyTips();
				}
			}
			else if (owner is RibbonGroup)
			{
				if (commandBaseRenderer is RibbonGroupDialogLauncherRenderer)
				{
					((RibbonGroup)owner).RaiseDialogButtonClick();
					SetKeyTipMode(false);
					return;
				}
				_isOpeningKeyTipDropDown = true;
				_keyTipOwner = owner;
				try
				{
					if (_tabContentRenderer != null && commandBaseRenderer is IDropDownItem)
					{
						((IDropDownItem)commandBaseRenderer).DropDown(selectFirstItem: false);
					}
				}
				finally
				{
					_isOpeningKeyTipDropDown = false;
				}
			}
			else if (commandBaseRenderer is IDropDownItem)
			{
				_isOpeningKeyTipDropDown = true;
				try
				{
					if (commandBaseRenderer is QuickAccessOverflowButtonRenderer || commandBaseRenderer is BackstageMenuTabRenderer)
					{
						((IDropDownItem)commandBaseRenderer).DropDown(selectFirstItem: false);
					}
					else
					{
						if (owner == null)
						{
							_keyTipOwner = _selectedTab;
						}
						if (commandBaseRenderer is ControlBoxButtonRenderer)
						{
							((IDropDownItem)commandBaseRenderer).DropDown(selectFirstItem: false);
						}
						else if (owner is CommandBase commandBase && commandBase.Enabled)
						{
							if (owner is DropDownCommand)
							{
								DropDownCommand dropDownCommand = owner as DropDownCommand;
								if (dropDownCommand.Commands.Count > 0)
								{
									OpenDropDownCommand(dropDownCommand);
								}
								else
								{
									dropDownCommand.InvokeOnClick();
								}
							}
							else if (owner is IMouseEventInvoker)
							{
								((CommandBase)owner).InvokeOnClick();
							}
						}
					}
				}
				finally
				{
					_isOpeningKeyTipDropDown = false;
				}
			}
			else if (owner is CheckBoxCommand)
			{
				CheckBoxCommand checkBoxCommand = (CheckBoxCommand)owner;
				if (checkBoxCommand.Control != null)
				{
					checkBoxCommand.Control.Focus();
				}
				checkBoxCommand.Checked = !checkBoxCommand.Checked;
			}
			else if (owner is ContainerControlCommand)
			{
				ContainerControlCommand containerControlCommand = (ContainerControlCommand)owner;
				if (containerControlCommand.Control != null)
				{
					containerControlCommand.Control.Focus();
				}
			}
			else if (owner is IMouseEventInvoker && ((CommandBase)owner).Enabled)
			{
				((CommandBase)owner).InvokeOnClick();
			}
		}

		private void SetKeyTips(IList<RibbonKeyTipEntry> keyTips)
		{
			foreach (RibbonKeyTipEntry item in _keyTips)
			{
				if (!keyTips.Contains(item))
				{
					item.DisposeToolTip();
				}
			}
			_keyTips = keyTips;
		}

		private void LoadBackstageKeyTips()
		{
			using Graphics graphics = CreateGraphics();
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			if (GetBackstageMenuScrollableBase() != null)
			{
				GetBackstageMenuScrollableBase().EnsureItemSelected();
				_keyTips = GetBackstageMenuScrollableBase().GetKeyTips(graphics, textRenderer);
			}
		}

		private void LoadParentControlKeyTips()
		{
			using Graphics graphics = CreateGraphics();
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			if (GetPopupMenu() != null)
			{
				_keyTips = GetPopupMenu().GetKeyTips(graphics, textRenderer);
			}
		}

		private void ClearKeyTips()
		{
			if (_keyTips == null)
			{
				return;
			}
			foreach (RibbonKeyTipEntry item in _keyTips)
			{
				item.DisposeToolTip();
			}
			_keyTips = null;
		}

		private void StopKeyTipTimer()
		{
			if (_keyTipTimer != null)
			{
				_keyTipTimer.Stop();
				_keyTipTimer.Tick -= OnTimerTick;
				_keyTipTimer.Dispose();
				_keyTipTimer = null;
			}
		}
	}
}
