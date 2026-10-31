using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
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
		internal void ShowAt(Point screenLocation)
		{
			NotifyAccessibilityClients(AccessibleEvents.SystemMenuPopupStart, 0);
			if (_parentItem != null && _parentItem.GetCommand() != null && GetCommandManagerOwner() != null && !GetCommandManagerOwner()._isCustomizing)
			{
				_parentItem.GetCommand().NotifyPopup();
				if (_parentItem != null)
				{
					_windowCommands = _parentItem.GetCommand().CreateMdiListCommands();
				}
			}
			if (_owner is MxContextMenu)
			{
				((MxContextMenu)_owner).RaisePopup();
			}
			if (base.IsDisposed)
			{
				return;
			}
			Graphics graphics = CreateGraphics();
			if (_parentItem != null)
			{
				_requestedAnchorBounds = new Rectangle(screenLocation, _parentItem._bounds.Size);
			}
			else if (_anchorSize.IsEmpty)
			{
				_requestedAnchorBounds = new Rectangle(screenLocation, new Size(0, _anchorHeight));
			}
			else
			{
				_requestedAnchorBounds = new Rectangle(screenLocation, _anchorSize);
			}
			if (!(_owner is MxCommandBar))
			{
				if ((_canUseThemes))
				{
					_requestedAnchorBounds.Y -= MenuOverlap;
				}
				else if (_owner is CommandControl)
				{
					if (_parentItem == null || !_parentItem._isOverflowItem)
					{
						_requestedAnchorBounds.Y += 2;
					}
				}
				else
				{
					_requestedAnchorBounds.Y++;
				}
			}
			_anchorBounds = _requestedAnchorBounds;
			if (_dropDownAnimation != DropDownAnimation.None && _canAnimate)
			{
				if (_dropDownAnimation == DropDownAnimation.System)
				{
					if (SystemInformation.IsMenuAnimationEnabled)
					{
						if (SystemInformation.IsMenuFadeEnabled)
						{
							_dropDownAnimation = DropDownAnimation.Fade;
						}
						else
						{
							_dropDownAnimation = DropDownAnimation.Slide;
						}
					}
					else
					{
						_dropDownAnimation = DropDownAnimation.None;
					}
				}
				else if (_dropDownAnimation == DropDownAnimation.Random)
				{
					Random random = new Random();
					switch (random.Next(0, 3))
					{
					case 0:
						_dropDownAnimation = DropDownAnimation.Slide;
						break;
					case 1:
						_dropDownAnimation = DropDownAnimation.Fade;
						break;
					default:
						_dropDownAnimation = DropDownAnimation.Unfold;
						break;
					}
				}
			}
			LayoutMenu(graphics);
			if (_owner is MxCommandBar)
			{
				FindOwnerCommandBar().Update();
				if (FindOwnerCommandBar().GetFloatingWindow() != null)
				{
					FindOwnerCommandBar().GetFloatingWindow().Invalidate();
					FindOwnerCommandBar().GetFloatingWindow().Update();
				}
			}
			_owner.MousePoint = Cursor.Position;
			if (_layoutItems.Count == 0 && base.Controls.Count == 0 && _expandBar == null)
			{
				CloseMenuChain();
			}
			else
			{
				if (base.IsDisposed)
				{
					return;
				}
				if (_dropDownAnimation == DropDownAnimation.None || !_canAnimate)
				{
					NativeMethods.ShowWindow(base.Handle, SwShowNoActivate);
					if (!base.Visible)
					{
						NativeMethods.ShowWindow(base.Handle, SwShowNormal);
					}
					if (!base.Visible)
					{
						Show();
					}
				}
				else if (_dropDownAnimation == DropDownAnimation.Fade)
				{
					NativeMethods.AnimateWindow(base.Handle, AnimationDuration, AwBlend);
				}
				else
				{
					uint animationFlags;
					if (!_isFlipped)
					{
						animationFlags = AnimateDownFlags;
						if (_dropDownAnimation == DropDownAnimation.Slide)
						{
							animationFlags += AnimateDownSlideFlags;
						}
					}
					else
					{
						animationFlags = AnimateUpFlags;
						if (_dropDownAnimation == DropDownAnimation.Slide)
						{
							animationFlags += AnimateUpSlideFlags;
						}
					}
					NativeMethods.AnimateWindow(base.Handle, AnimationDuration, animationFlags);
				}
				if (_activateOnShow)
				{
					NativeMethods.SetForegroundWindow(base.Handle);
					_activateOnShow = false;
				}
				if (_holdMessages && VisualStyleSupport.SupportsLayeredWindows() && _showShadow && ColorSchemeWindowMessageHandler.IsShadowAllowed())
				{
					// Showing the window may have made the shadow window already; it then only gets the cut-out and a new picture.
					bool isCreated = _shadowWindow != null;
					ColorSchemeWindowMessageHandler shadow = _shadowWindow ?? new ColorSchemeWindowMessageHandler();
					if (_isOverflowMenu && FindOwnerCommandBar().IsHorizontalLayout() && base.Right == _shadowAnchorBounds.Right && (!_canUseThemes))
					{
						shadow._drawRightShadowTopCorner = false;
					}
					Point location = PointToClient(_shadowAnchorBounds.Location);
					shadow._cutoutBounds = new Rectangle(location, _shadowAnchorBounds.Size);
					if (isCreated)
					{
						shadow.Redraw();
					}
					else
					{
						shadow.CreateShadowWindow(new Rectangle(base.Location, new Size(base.Bounds.Width + ShadowSize, base.Bounds.Height + ShadowSize)), _canUseThemes);
						_shadowWindow = shadow;
					}
				}
				graphics.Dispose();
				graphics = null;
				Invalidate();
				if (!_holdMessages)
				{
					return;
				}
				if (Control.MouseButtons != MouseButtons.None)
				{
					_ignoreNextMouseUp = true;
				}
				NativeMethods.ReleaseCapture();
				if (_expandAfterDelay)
				{
					if (_expandBar != null)
					{
						StartHoverTimer(_fullMenuDelay);
					}
					else
					{
						_expandAfterDelay = false;
					}
				}
				RunMessageLoop();
				if (!base.IsDisposed)
				{
					CloseMenuChain();
				}
			}
		}

		public void CloseMenuFor(MxCommand uiCommand)
		{
			if (_parentItem != null)
			{
				if (_parentItem.GetCommand() == uiCommand)
				{
					Dispose();
				}
				else if (ChildMenu != null)
				{
					ChildMenu.CloseMenuFor(uiCommand);
				}
			}
			else
			{
				CloseMenuChain();
			}
		}

		internal bool TryCloseChildMenu(CommandCloseReason commandCloseReason)
		{
			if (_childMenu != null)
			{
				if (_childMenu._parentItem != null && _childMenu._parentItem.GetCommand() != null)
				{
					MxCommand uiCommand = _childMenu._parentItem.GetCommand();
					if (uiCommand.ConfirmClose(commandCloseReason))
					{
						ChildMenu = null;
						if (commandCloseReason == CommandCloseReason.AppClicked)
						{
							CloseMenuChain();
						}
						return true;
					}
					return false;
				}
				if (_owner is MxContextMenu)
				{
					if (((MxContextMenu)_owner).TryClose(commandCloseReason))
					{
						ChildMenu = null;
						return true;
					}
					return false;
				}
			}
			return true;
		}

		public void CloseMenuChain()
		{
			if (_owner is MxCommandBar)
			{
				MxCommandBar uiCommandBar = (MxCommandBar)_owner;
				uiCommandBar.SetChildMenu(null);
				uiCommandBar.SetShowInfrequentItems(false);
			}
			else if (_owner is CommandControl)
			{
				((CommandControl)_owner).CloseMenuChain();
			}
			else if (_owner is MxContextMenu)
			{
				MxContextMenu uiContextMenu = (MxContextMenu)_owner;
				uiContextMenu.SetPopupControl(null);
				uiContextMenu.SetShowInfrequentItems(false);
			}
		}

		public bool TryCloseMenu(CommandCloseReason commandCloseReason)
		{
			if (_owner is MxCommandBar)
			{
				MxCommandBar uiCommandBar = (MxCommandBar)_owner;
				if (uiCommandBar.TryCloseChildMenu(commandCloseReason))
				{
					uiCommandBar.SetShowInfrequentItems(false);
					return true;
				}
				return false;
			}
			if (_owner is CommandControl)
			{
				return ((CommandControl)_owner).TryCloseChildMenu(commandCloseReason);
			}
			if (_owner is MxContextMenu)
			{
				MxContextMenu uiContextMenu = (MxContextMenu)_owner;
				if (uiContextMenu.TryClose(commandCloseReason))
				{
					uiContextMenu.SetShowInfrequentItems(false);
					return true;
				}
				return false;
			}
			return true;
		}
	}
}
