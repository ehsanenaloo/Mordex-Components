using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		protected override void OnLeave(EventArgs e)
		{
			if (_contextualTabList.Count > 0)
			{
				Form hostForm = GetHostForm();
				foreach (RibbonContextualTab ribbonContextualTab in _contextualTabList.ToArray())
				{
					if (ribbonContextualTab.Controls.Count == 0)
					{
						continue;
					}
					ribbonContextualTab.Active = IsContextualTabControlFocused(ribbonContextualTab, hostForm);
				}
			}
			base.OnLeave(e);
		}

		private static bool IsContextualTabControlFocused(RibbonContextualTab ribbonContextualTab, Form hostForm)
		{
			if (hostForm == null)
			{
				return false;
			}
			if (ribbonContextualTab.Controls.Contains(hostForm))
			{
				return true;
			}
			for (Control control = hostForm.ActiveControl; control != null; control = RibbonDrawingUtils.GetParentControl(control))
			{
				if (ribbonContextualTab.Controls.Contains(control))
				{
					return true;
				}
			}
			return false;
		}

		internal bool ForwardBackstageMouseUp(MouseButtons button, int screenX, int screenY)
		{
			if (GetBackstageMenuScrollableBase() != null)
			{
				Point point = GetBackstageMenuScrollableBase().PointToClient(new Point(screenX, screenY));
				MouseEventArgs e = new MouseEventArgs(button, 1, point.X, point.Y, 0);
				return GetBackstageMenuScrollableBase().HandleMouseUp(e);
			}
			return false;
		}

		internal bool ForwardBackstageMouseMove(MouseButtons button, int screenX, int screenY)
		{
			if (GetBackstageMenuScrollableBase() != null)
			{
				Point point = GetBackstageMenuScrollableBase().PointToClient(new Point(screenX, screenY));
				MouseEventArgs e = new MouseEventArgs(button, 1, point.X, point.Y, 0);
				return GetBackstageMenuScrollableBase().HandleMouseMove(e);
			}
			return false;
		}

		internal bool ForwardBackstageMouseDown(MouseButtons button, int screenX, int screenY)
		{
			if (GetBackstageMenuScrollableBase() != null)
			{
				NativeMethods.NativeMessage n = default(NativeMethods.NativeMessage);
				NativeMethods.GetMessage(ref n, IntPtr.Zero, 512u, 525u);
				Point point = GetBackstageMenuScrollableBase().PointToClient(new Point(screenX, screenY));
				MouseEventArgs e = new MouseEventArgs(button, 1, point.X, point.Y, 0);
				return GetBackstageMenuScrollableBase().HandleMouseDown(e);
			}
			return false;
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			if (_controlBoxButtonRenderer != null && _form != null && _handlesTitleBar && ControlBoxDoubleClickAction == ControlBoxDoubleClickAction.CloseForm)
			{
				Point pt = PointToClient(Cursor.Position);
				if (_controlBoxButtonBounds.Contains(pt))
				{
					_form.Close();
					return;
				}
			}
			if (_tabStripRenderer == null)
			{
				return;
			}
			Point point = PointToClient(Cursor.Position);
			RibbonTabRenderer ribbonTabRenderer = _tabStripRenderer.GetTabAt(point.X, point.Y);
			if (_minimizeOnDoubleClick && ribbonTabRenderer != null && ribbonTabRenderer.GetTab() != null)
			{
				if (!Minimized && _wasSelectedTabClicked)
				{
					Minimized = true;
				}
				else if (Minimized)
				{
					Minimized = false;
					SetPopupMenu(null);
				}
			}
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WmNcHitTest && _ribbonWindowMessageHandler != null && _headerRenderer != null && !IsGlassCaptionActive() && ShowCaptionBar)
			{
				Point p = new Point(NativeMethods.ToInt32Unchecked(m.LParam));
				Point pt = PointToClient(p);
				if (_form != null && _form.FormBorderStyle != FormBorderStyle.None && _headerRenderer.GetArea().Contains(pt))
				{
					m.Result = (IntPtr)HtTransparent;
					return;
				}
			}
			base.WndProc(ref m);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			ShowSuperTip(null);
			if (e.Button == MouseButtons.Left)
			{
				if (base.ContainsFocus && GetHostForm() != null)
				{
					GetHostForm().ActiveControl = null;
				}
				if (IsCaptionBarShown() && _headerRenderer.HandleMouseDown(new Point(e.X, e.Y)))
				{
					return;
				}
				if (GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxButton && _controlBoxButtonRenderer != null && _controlBoxButtonBounds.Contains(e.X, e.Y))
				{
					ShowControlBoxMenu();
					return;
				}
			}
			else if (IsCaptionBarShown() && _headerRenderer.HandleMouseDown(new Point(e.X, e.Y)))
			{
				return;
			}
			base.OnMouseDown(e);
			if (!_tabStripRenderer.HandleMouseDown(e) && (_quickAccessPosition != QuickAccessPosition.BelowRibbon || _quickAccessToolbarRenderer == null || !_quickAccessToolbarRenderer.HandleMouseDown(new Point(e.X, e.Y), e.Button)) && (!_minimized || IsMinimizedPopupOpen()))
			{
				_tabContentRenderer.HandleMouseDown(e);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (IsCaptionBarShown() && _headerRenderer.HandleMouseUp(new Point(e.X, e.Y), e.Button))
			{
				SetPushedRenderer(null);
			}
			else if (_quickAccessPosition != QuickAccessPosition.BelowRibbon || _quickAccessToolbarRenderer == null || !_quickAccessToolbarRenderer.HandleMouseUp(new Point(e.X, e.Y), e.Button))
			{
				if (_tabStripRenderer != null && _tabStripRenderer.HandleMouseUp(e))
				{
					SetPushedRenderer(null);
				}
				if (_tabContentRenderer != null && _tabContentRenderer.HandleMouseUp(e))
				{
					SetPushedRenderer(null);
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (GetHostForm() == null || (!GetHostForm().ContainsFocus && base.Parent == GetHostForm() && !GetHostForm().IsMdiChild))
			{
				base.OnMouseMove(e);
				return;
			}
			if (IsCaptionBarShown())
			{
				if (_headerRenderer.HandleMouseMove(new Point(e.X, e.Y)))
				{
					_tabContentRenderer.SetHotGroup(null);
					_tabContentRenderer.SetHotScrollButton(null);
					return;
				}
			}
			else
			{
				if (_controlBoxButtonBounds.Contains(e.X, e.Y))
				{
					_headerRenderer.SetHotItem(_controlBoxButtonRenderer);
					return;
				}
				_headerRenderer.SetHotItem(null);
			}
			base.OnMouseMove(e);
			if (_tabStripRenderer.HandleMouseMove(new Point(e.X, e.Y)))
			{
				_tabContentRenderer.SetHotGroup(null);
				_tabContentRenderer.SetHotScrollButton(null);
			}
			else if ((_quickAccessPosition != QuickAccessPosition.BelowRibbon || _quickAccessToolbarRenderer == null || !_quickAccessToolbarRenderer.HandleMouseMove(new Point(e.X, e.Y))) && (!_minimized || IsMinimizedPopupOpen()))
			{
				_tabContentRenderer.HandleMouseMove(e);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			if (IsCaptionBarShown())
			{
				_headerRenderer.ClearHotAndPushedState();
			}
			_tabStripRenderer.ClearHoverState();
			_tabContentRenderer.ClearHotItems();
			if (_quickAccessToolbarRenderer != null)
			{
				_quickAccessToolbarRenderer.SetHoveredItem(null);
			}
		}

		private void ScrollTabsByWheel(int wheelNotches)
		{
			if (base.Created && base.ClientRectangle.Width > 0 && base.ClientRectangle.Height > 0)
			{
				if (wheelNotches < 0)
				{
					_tabStripRenderer.SelectNextTab(false);
				}
				else if (wheelNotches > 0 && (SelectedTab == null || SelectedTab.Index != 0))
				{
					_tabStripRenderer.SelectPreviousTab(false);
				}
			}
		}

		private void SelectFirstVisibleTab()
		{
			if (Tabs.Count > 0)
			{
				int startIndex = 0;
				if (_selectedTab != null)
				{
					startIndex = SelectedTab.Index;
				}
				for (int i = startIndex; i < _tabs.Count; i++)
				{
					RibbonTab ribbonTab = _tabs[i];
					if (ribbonTab.Visible)
					{
						SelectedTab = ribbonTab;
						return;
					}
				}
				for (int j = 0; j < _tabs.Count; j++)
				{
					RibbonTab firstVisibleTab = _tabs[j];
					if (firstVisibleTab.Visible)
					{
						SelectedTab = firstVisibleTab;
						break;
					}
				}
			}
			else if (_contextualTabList != null && _contextualTabList.Count > 0)
			{
				foreach (RibbonContextualTab item in _contextualTabList)
				{
					foreach (RibbonTab tab in item.Tabs)
					{
						if (tab.Visible)
						{
							SelectedTab = tab;
							return;
						}
					}
				}
				SelectedTab = null;
			}
			else
			{
				SelectedTab = null;
			}
		}

		private int GetKeyCodeWithModifiers(IntPtr keyParam)
		{
			int keyCode = NativeMethods.ToInt32Unchecked(keyParam);
			Keys modifierKeys = Control.ModifierKeys;
			if ((modifierKeys & Keys.Alt) != Keys.Alt)
			{
				if ((modifierKeys & Keys.Shift) == Keys.Shift)
				{
					keyCode += (int)Keys.Shift;
				}
				if ((modifierKeys & Keys.Control) == Keys.Control)
				{
					keyCode += (int)Keys.Control;
				}
			}
			return keyCode;
		}

		private void NavigateDown()
		{
			ClearKeyTips();
			_keyTipOwner = null;
			_keyTipInput = "";
			if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer.ContainsItem(GetKeyboardFocusRenderer()))
			{
				((IKeyboardNavigable)_quickAccessToolbarRenderer).MoveDown(GetKeyboardFocusRenderer());
			}
			else if (_backstageMenuScrollableBase != null)
			{
				_backstageMenuScrollableBase.SelectNextItem();
			}
			else
			{
				_tabContentRenderer.MoveDownFromItem(GetKeyboardFocusRenderer());
			}
		}

		private void NavigateUp()
		{
			ClearKeyTips();
			_keyTipOwner = null;
			_keyTipInput = "";
			if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer.ContainsItem(GetKeyboardFocusRenderer()))
			{
				((IKeyboardNavigable)_quickAccessToolbarRenderer).MoveUp(GetKeyboardFocusRenderer());
			}
			else if (_backstageMenuScrollableBase != null)
			{
				_backstageMenuScrollableBase.SelectPreviousItem();
			}
			else if (GetKeyboardFocusRenderer() is RibbonTabRenderer)
			{
				if (_quickAccessPosition == QuickAccessPosition.AboveRibbon && _quickAccessToolbarRenderer != null)
				{
					_quickAccessToolbarRenderer.TrySelectItemAtX(GetKeyboardFocusRenderer()._bounds.X + GetKeyboardFocusRenderer()._bounds.Width / 2);
				}
			}
			else if (!_tabContentRenderer.TryMoveUpIntoGroups(GetKeyboardFocusRenderer()))
			{
				SetKeyboardFocusRenderer(_tabStripRenderer.FindTabRenderer(SelectedTab));
			}
		}

		private void NavigateLeft()
		{
			ClearKeyTips();
			_keyTipOwner = null;
			_keyTipInput = "";
			if (_controlBoxButtonRenderer != null && GetKeyboardFocusRenderer() == _controlBoxButtonRenderer)
			{
				_tabStripRenderer.SelectLastTab();
			}
			else if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer.ContainsItem(GetKeyboardFocusRenderer()))
			{
				((IKeyboardNavigable)_quickAccessToolbarRenderer).MoveLeft(GetKeyboardFocusRenderer());
			}
			else if (GetKeyboardFocusRenderer() == null || GetKeyboardFocusRenderer() is RibbonTabRenderer)
			{
				if (_tabStripRenderer.GetTabRenderers().IndexOf((RibbonTabRenderer)GetKeyboardFocusRenderer()) == 0 && ((RibbonTabRenderer)GetKeyboardFocusRenderer()).GetTab() == _selectedTab && _controlBoxButtonRenderer != null)
				{
					SetKeyboardFocusRenderer(_controlBoxButtonRenderer);
					return;
				}
				_tabStripRenderer.SelectPreviousTab(true);
				SetKeyboardFocusRenderer(_tabStripRenderer.FindTabRenderer(_selectedTab));
			}
			else
			{
				_tabContentRenderer.MoveFocusLeft(GetKeyboardFocusRenderer());
			}
		}

		private void NavigateRight()
		{
			ClearKeyTips();
			_keyTipOwner = null;
			_keyTipInput = "";
			if (_controlBoxButtonRenderer != null && GetKeyboardFocusRenderer() == _controlBoxButtonRenderer)
			{
				_tabStripRenderer.SelectFirstTab();
			}
			else if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer.ContainsItem(GetKeyboardFocusRenderer()))
			{
				((IKeyboardNavigable)_quickAccessToolbarRenderer).MoveRight(GetKeyboardFocusRenderer());
			}
			else if (GetKeyboardFocusRenderer() == null || GetKeyboardFocusRenderer() is RibbonTabRenderer)
			{
				if (GetKeyboardFocusRenderer() != null && _tabStripRenderer.GetTabRenderers().IndexOf((RibbonTabRenderer)GetKeyboardFocusRenderer()) == _tabStripRenderer.GetTabRenderers().Count - 1 && ((RibbonTabRenderer)GetKeyboardFocusRenderer()).GetTab() == _selectedTab && _controlBoxButtonRenderer != null)
				{
					SetKeyboardFocusRenderer(_controlBoxButtonRenderer);
					return;
				}
				_tabStripRenderer.SelectNextTab(true);
				SetKeyboardFocusRenderer(_tabStripRenderer.FindTabRenderer(_selectedTab));
			}
			else
			{
				_tabContentRenderer.MoveRightFromItem(GetKeyboardFocusRenderer());
			}
		}

		private void NavigateNext()
		{
			ClearKeyTips();
			_keyTipOwner = null;
			_keyTipInput = "";
			if (_controlBoxButtonRenderer != null && GetKeyboardFocusRenderer() == _controlBoxButtonRenderer)
			{
				if (_quickAccessToolbarRenderer != null)
				{
					((IKeyboardNavigable)_quickAccessToolbarRenderer).MoveNext((CommandBaseRenderer)null);
					return;
				}
				RibbonTabRenderer ribbonTabRenderer = _tabStripRenderer.FindTabRenderer(SelectedTab);
				if (ribbonTabRenderer != null)
				{
					SetKeyboardFocusRenderer(ribbonTabRenderer);
				}
			}
			else if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer.ContainsItem(GetKeyboardFocusRenderer()))
			{
				((IKeyboardNavigable)_quickAccessToolbarRenderer).MoveNext(GetKeyboardFocusRenderer());
			}
			else if (GetKeyboardFocusRenderer() is RibbonTabRenderer)
			{
				if (!_tabContentRenderer.TryMoveDownIntoGroups())
				{
					_tabStripRenderer.SelectNextTab(false);
				}
			}
			else if (_tabContentRenderer != null && !_tabContentRenderer.TryMoveNext(GetKeyboardFocusRenderer()))
			{
				if (_controlBoxButtonRenderer != null)
				{
					SetKeyboardFocusRenderer(_controlBoxButtonRenderer);
				}
				else if (_quickAccessToolbarRenderer != null)
				{
					((IKeyboardNavigable)_quickAccessToolbarRenderer).MoveNext((CommandBaseRenderer)null);
				}
			}
		}

		private void NavigatePrevious()
		{
			ClearKeyTips();
			_keyTipOwner = null;
			_keyTipInput = "";
			if (_controlBoxButtonRenderer != null && GetKeyboardFocusRenderer() == _controlBoxButtonRenderer)
			{
				_tabContentRenderer.TrySelectLastItem();
			}
			else if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer.ContainsItem(GetKeyboardFocusRenderer()))
			{
				((IKeyboardNavigable)_quickAccessToolbarRenderer).MovePrevious(GetKeyboardFocusRenderer());
			}
			else if (GetKeyboardFocusRenderer() is RibbonTabRenderer)
			{
				if (_quickAccessToolbarRenderer != null)
				{
					((IKeyboardNavigable)_quickAccessToolbarRenderer).MovePrevious((CommandBaseRenderer)null);
				}
				else if (_controlBoxButtonRenderer != null)
				{
					SetKeyboardFocusRenderer(_controlBoxButtonRenderer);
				}
			}
			else if (_tabContentRenderer != null && !_tabContentRenderer.TryMovePrevious(GetKeyboardFocusRenderer()))
			{
				RibbonTabRenderer ribbonTabRenderer = _tabStripRenderer.FindTabRenderer(SelectedTab);
				if (ribbonTabRenderer != null)
				{
					SetKeyboardFocusRenderer(ribbonTabRenderer);
				}
			}
		}

		private static bool IsRibbonFilterMessage(int message)
		{
			switch (message)
			{
			case WmActivate:
			case WmActivateApp:
			case WmNcLButtonDown:
			case WmNcRButtonDown:
			case WmNcMButtonDown:
			case WmLButtonDown:
			case WmRButtonDown:
			case WmMButtonDown:
			case WmXButtonDown:
			case WmMouseWheel:
			case WmKeyDown:
			case WmChar:
			case WmSysKeyDown:
			case WmSysKeyUp:
			case WmSysChar:
				return true;
			default:
				return false;
			}
		}

		private bool PreFilterRibbonMessage(ref Message msg)
		{
			if (!IsRibbonFilterMessage(msg.Msg))
			{
				return false;
			}
			bool result = false;
			Control control = RibbonDrawingUtils.GetParentControl(this);
			if (control == null || !control.Enabled)
			{
				return false;
			}
			Form form = FindForm();
			Form activeForm = Form.ActiveForm;
			if (form != activeForm && (activeForm == null || activeForm.ActiveMdiChild == null || activeForm.ActiveMdiChild != form))
			{
				StopKeyTipTimer();
				SetKeyTipMode(false);
				return false;
			}
			switch (msg.Msg)
			{
			case WmActivate:
			case WmActivateApp:
				SetKeyTipMode(false);
				break;
			case WmNcLButtonDown:
			case WmNcRButtonDown:
			case WmNcMButtonDown:
			case WmLButtonDown:
			case WmRButtonDown:
			case WmMButtonDown:
			case WmXButtonDown:
				SetKeyTipMode(false);
				break;
			case WmMouseWheel:
				if (!Minimized && !IsBackstageOpen() && base.ClientRectangle.Contains(PointToClient(Cursor.Position)) && (!base.ContainsFocus || Focused))
				{
					int wheelNotches = (NativeMethods.ToInt32Unchecked(msg.WParam) >> 16) / WheelDelta;
					ScrollTabsByWheel(wheelNotches);
					result = true;
				}
				break;
			case WmKeyDown:
			{
				StopKeyTipTimer();
				int keyDownWithModifiers = GetKeyCodeWithModifiers(msg.WParam);
				switch (keyDownWithModifiers)
				{
				case (int)Keys.Escape:
					CloseBackstage();
					if (IsKeyTipMode())
					{
						StepBackKeyTipLevel();
						result = true;
					}
					break;
				case (int)Keys.Left:
					if (IsKeyTipMode())
					{
						NavigateLeft();
						result = true;
					}
					break;
				case (int)(Keys.Shift | Keys.Tab):
					if (IsKeyTipMode())
					{
						NavigatePrevious();
						result = true;
					}
					break;
				case (int)Keys.Tab:
					if (IsKeyTipMode())
					{
						if (Control.ModifierKeys == Keys.Shift)
						{
							NavigatePrevious();
						}
						else
						{
							NavigateNext();
						}
						result = true;
					}
					break;
				case (int)Keys.Right:
					if (IsKeyTipMode())
					{
						NavigateRight();
						result = true;
					}
					break;
				case (int)Keys.Down:
					if (IsKeyTipMode())
					{
						NavigateDown();
						result = true;
					}
					break;
				case (int)Keys.Up:
					if (IsKeyTipMode())
					{
						NavigateUp();
						result = true;
					}
					break;
				case (int)Keys.F4:
					if (IsKeyTipMode() && GetKeyboardFocusRenderer() is IDropDownItem)
					{
						((IDropDownItem)GetKeyboardFocusRenderer()).DropDown(selectFirstItem: true);
						result = true;
						break;
					}
					goto default;
				case (int)Keys.Return:
				case (int)Keys.Space:
					if (!IsKeyTipMode())
					{
						break;
					}
					if (GetKeyboardFocusRenderer() != null && (GetKeyboardFocusRenderer().GetCommand() == null || GetKeyboardFocusRenderer().GetCommand().Enabled))
					{
						if (GetKeyboardFocusRenderer() is IDropDownItem)
						{
							((IDropDownItem)GetKeyboardFocusRenderer()).DropDown(selectFirstItem: true);
						}
						else if (GetKeyboardFocusRenderer().GetCommand() is IMouseEventInvoker)
						{
							GetKeyboardFocusRenderer().GetCommand().InvokeOnClick();
						}
					}
					result = true;
					break;
				default:
				{
					Shortcut keyDownShortcut = (Shortcut)keyDownWithModifiers;
					if (!Enum.IsDefined(typeof(Shortcut), keyDownShortcut))
					{
						break;
					}
					Control keyDownControl = Control.FromHandle(msg.HWnd);
					if (TryProcessShortcut(keyDownShortcut, keyDownControl, msg))
					{
						result = true;
					}
					break;
				}
				}
				break;
			}
			case WmSysChar:
			{
				if (IsKeyTipMode() || !s_isKeyTipSessionActive)
				{
					break;
				}
				int keyCode = NativeMethods.ToInt32Unchecked(msg.WParam);
				bool isAltCombination = false;
				Shortcut sysCharShortcut;
				if (keyCode == VirtualKeyF10)
				{
					int sysCharWithModifiers = GetKeyCodeWithModifiers(msg.WParam);
					sysCharShortcut = (Shortcut)sysCharWithModifiers;
				}
				else
				{
					isAltCombination = true;
					sysCharShortcut = (Shortcut)(KeysAltModifier + keyCode);
				}
				if (Enum.IsDefined(typeof(Shortcut), sysCharShortcut) && (!isAltCombination || !char.IsLetterOrDigit((char)keyCode)))
				{
					break;
				}
				int lParamFlags = NativeMethods.ToInt32Unchecked(msg.LParam);
				if ((lParamFlags & SysCharContextCodeBit) != SysCharContextCodeBit)
				{
					char c = (char)keyCode;
					if (char.IsLetterOrDigit(c))
					{
						SetKeyTipMode(true);
						HandleKeyTipChar(c);
					}
				}
				break;
			}
			case WmChar:
				if (IsKeyTipMode())
				{
					if (!_checkFirstKeyTipChar || !PreProcessMessage(ref msg))
					{
						_checkFirstKeyTipChar = false;
						HandleKeyTipChar((char)NativeMethods.ToInt32Unchecked(msg.WParam));
						result = true;
					}
					else
					{
						SetKeyTipMode(false);
					}
				}
				break;
			case WmSysKeyUp:
			{
				StopKeyTipTimer();
				int keyCode = NativeMethods.ToInt32Unchecked(msg.WParam);
				if (Control.ModifierKeys != Keys.Control)
				{
					if (keyCode == VirtualKeyF10)
					{
						if (!_keyTipsShownByF10)
						{
							SetKeyTipMode(false);
						}
						result = true;
					}
					else if (keyCode == VkMenu)
					{
						if (!_keyTipsShownByAltHold)
						{
							SetKeyTipMode(!IsKeyTipMode());
						}
						result = true;
					}
				}
				_keyTipsShownByF10 = false;
				_keyTipsShownByAltHold = false;
				break;
			}
			case WmSysKeyDown:
			{
				int keyCode = NativeMethods.ToInt32Unchecked(msg.WParam);
				if (IsKeyTipMode())
				{
					if (keyCode != VkMenu && keyCode != VirtualKeyF10)
					{
						Shortcut shortcut = (Shortcut)(KeysAltModifier + keyCode);
						if (!Enum.IsDefined(typeof(Shortcut), shortcut))
						{
							result = HandleKeyTipChar((char)keyCode);
						}
					}
				}
				else
				{
					if (s_isKeyTipSessionActive)
					{
						break;
					}
					Shortcut sysKeyDownShortcut;
					if (keyCode == VirtualKeyF10)
					{
						int sysKeyDownWithModifiers = GetKeyCodeWithModifiers(msg.WParam);
						sysKeyDownShortcut = (Shortcut)sysKeyDownWithModifiers;
					}
					else
					{
						sysKeyDownShortcut = (Shortcut)(KeysAltModifier + keyCode);
					}
					if (Enum.IsDefined(typeof(Shortcut), sysKeyDownShortcut))
					{
						StopKeyTipTimer();
						Control sysKeyDownControl = Control.FromHandle(msg.HWnd);
						if (TryProcessShortcut(sysKeyDownShortcut, sysKeyDownControl, msg))
						{
							result = true;
						}
						else if (keyCode == VirtualKeyF10 && (sysKeyDownControl == null || sysKeyDownControl.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageNotNeeded))
						{
							SetKeyTipMode(true);
							_keyTipsShownByF10 = true;
						}
					}
					else if (keyCode == VkMenu)
					{
						StartKeyTipTimer();
					}
					else
					{
						StopKeyTipTimer();
					}
				}
				break;
			}
			}
			return result;
		}

		internal bool TryCloseParentControl(CommandCloseReason commandCloseReason)
		{
			if (GetPopupMenu() != null)
			{
				if (GetPopupMenu().GetParentMenuCommand() != null && GetPopupMenu().GetParentMenuCommand() is DropDownCommand)
				{
					DropDownCommand dropDownCommand = GetPopupMenu().GetParentMenuCommand() as DropDownCommand;
					if (dropDownCommand.CanCloseDropDown(commandCloseReason))
					{
						if (commandCloseReason != CommandCloseReason.Keyboard)
						{
							SetKeyTipMode(false);
						}
						SetPopupMenu(null);
						return true;
					}
					return false;
				}
				CancelEventArgs e = new CancelEventArgs();
				OnControlBoxClosing(e);
				if (e.Cancel)
				{
					return false;
				}
				if (commandCloseReason != CommandCloseReason.Keyboard)
				{
					SetKeyTipMode(false);
				}
				SetPopupMenu(null);
				OnControlBoxClosed(EventArgs.Empty);
				return true;
			}
			return true;
		}

		private bool TryProcessShortcut(Shortcut shortcut, Control inputControl, Message msg)
		{
			if (_tabStripRenderer != null)
			{
				foreach (RibbonTab tab in Tabs)
				{
					if (tab.TryProcessShortcut(shortcut, inputControl, msg))
					{
						return true;
					}
				}
				foreach (RibbonContextualTab item in GetContextualTabList())
				{
					if (item.TryProcessShortcut(shortcut, inputControl, msg))
					{
						return true;
					}
				}
				foreach (CommandBase leftCommand in ControlBoxMenu.LeftCommands)
				{
					if (leftCommand.SearchShortcut(shortcut, inputControl, msg))
					{
						return true;
					}
				}
				foreach (CommandBase rightCommand in ControlBoxMenu.RightCommands)
				{
					if (rightCommand.SearchShortcut(shortcut, inputControl, msg))
					{
						return true;
					}
				}
				foreach (CommandBase bottomCommand in ControlBoxMenu.BottomCommands)
				{
					if (bottomCommand.SearchShortcut(shortcut, inputControl, msg))
					{
						return true;
					}
				}
			}
			foreach (CommandBase quickAccessCommand in QuickAccessCommands)
			{
				if (quickAccessCommand.SearchShortcut(shortcut, inputControl, msg))
				{
					return true;
				}
			}
			foreach (CommandBase quickAccessCustomize in QuickAccessCustomizeList)
			{
				if (quickAccessCustomize.SearchShortcut(shortcut, inputControl, msg))
				{
					return true;
				}
			}
			if (shortcut == Shortcut.CtrlF1)
			{
				Minimized = !Minimized;
				return true;
			}
			return false;
		}
	}
}
