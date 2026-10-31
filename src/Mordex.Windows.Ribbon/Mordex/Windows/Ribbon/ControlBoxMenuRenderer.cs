using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class ControlBoxMenuRenderer : CommandBaseEditorDialog
	{
		private ControlBoxMenu _controlBoxMenu;

		private ControlBoxPanelRenderer _leftPanel;

		private ControlBoxPanelRenderer _rightPanel;

		private ControlBoxPanelRenderer _bottomPanel;

		private int _bottomPanelHeight;

		private BackstageMenuTabRenderer _backstageTabRenderer;

		private IList<CommandBaseRenderer> _navigableItems;

		public ControlBoxMenuRenderer(ControlBoxMenu controlBoxMenu)
			: base(controlBoxMenu.Ribbon, null, null, null)
		{
			_controlBoxMenu = controlBoxMenu;
			_ribbon = controlBoxMenu.Ribbon;
			_contentPadding = new Size(12, 24);
			_contentOffsetY = 18;
		}

		protected override void Dispose(bool disposing)
		{
			_ribbon = null;
			if (_leftPanel != null)
			{
				_leftPanel.Dispose();
			}
			if (_rightPanel != null)
			{
				_rightPanel.Dispose();
			}
			if (_bottomPanel != null)
			{
				_bottomPanel.Dispose();
			}
			_leftPanel = null;
			_rightPanel = null;
			_bottomPanel = null;
			if (_backstageTabRenderer != null)
			{
				_backstageTabRenderer.Dispose();
				_backstageTabRenderer = null;
			}
			_controlBoxMenu = null;
			_navigableItems = null;
			base.Dispose(disposing);
		}

		private void RebuildNavigableItems()
		{
			_navigableItems = new List<CommandBaseRenderer>();
			if (_leftPanel != null)
			{
				foreach (CommandBaseRenderer item in _leftPanel._itemRenderers)
				{
					_navigableItems.Add(item);
				}
			}
			if (_rightPanel != null)
			{
				foreach (CommandBaseRenderer rightItem in _rightPanel._itemRenderers)
				{
					_navigableItems.Add(rightItem);
				}
			}
			if (_bottomPanel != null)
			{
				for (int index = _bottomPanel._itemRenderers.Count - 1; index >= 0; index--)
				{
					_navigableItems.Add(_bottomPanel._itemRenderers[index]);
				}
			}
		}

		public override void OpenDropDown(DropDownCommand cmd)
		{
			if (cmd.GetParentContainer() is DropDownCommand && cmd.GetParentContainer() != GetParentMenuCommand())
			{
				OpenDropDown(cmd.GetParentContainer() as DropDownCommand);
				if (GetChildMenu() != null)
				{
					GetChildMenu().OpenDropDown(cmd);
				}
				return;
			}
			foreach (CommandBaseRenderer item in _layoutItems)
			{
				if (item is ControlBoxPanelRenderer)
				{
					ControlBoxPanelRenderer commandCollectionRenderer = (ControlBoxPanelRenderer)item;
					foreach (CommandBaseRenderer panelItem in commandCollectionRenderer._itemRenderers)
					{
						if (panelItem.GetCommand() == cmd && panelItem.HasChildren)
						{
							ShowChildMenu(panelItem, selectItem: true);
							return;
						}
					}
				}
				else if (item.GetCommand() == cmd && item.HasChildren)
				{
					ShowChildMenu(item, selectItem: true);
					break;
				}
			}
		}

		public override void RebuildMenuAndBounds(Graphics graphics)
		{
			_bottomPanelHeight = 0;
			if (_ribbon != null)
			{
				_contentOffsetY = _ribbon._tabStripRowHeight - 1;
			}
			else
			{
				_contentOffsetY = 18;
			}
			_bottomPanelHeight = 16;
			_contentPadding = new Size(2, _contentOffsetY);
			int contentWidth = 0;
			_layoutItems = new ArrayList();
			using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
			textLayoutRenderer.SetShowHotkeyPrefix(true);
			int height = _contentPadding.Height;
			_totalItemHeight = 0;
			int leftPanelHeight = 0;
			int rightPanelHeight = 0;
			if (_controlBoxMenu.LeftCommands.Count > 0 || _isDesignMode)
			{
				_leftPanel = new ControlBoxPanelRenderer(_controlBoxMenu.LeftCommands, this, this, _owner.SmallItemHeight, ControlBoxPanelType.Left);
				_leftPanel.CalculateFullItemWidth(graphics, textLayoutRenderer, Font);
				if (_controlBoxMenu.LeftPanelWidth > 0 && _leftPanel._itemRenderers.Count > 0)
				{
					_leftPanel._fullItemWidth = _controlBoxMenu.LeftPanelWidth;
				}
				if (_leftPanel._fullItemWidth > 0)
				{
					_layoutItems.Add(_leftPanel);
					contentWidth += _leftPanel._fullItemWidth;
					leftPanelHeight = _leftPanel.GetPanelHeight(_owner.SmallItemHeight);
				}
				else
				{
					_leftPanel = null;
				}
			}
			else
			{
				_leftPanel = null;
			}
			if (_controlBoxMenu.RightCommands.Count > 0 || _isDesignMode)
			{
				_rightPanel = new ControlBoxPanelRenderer(_controlBoxMenu.RightCommands, this, this, _owner.SmallItemHeight, ControlBoxPanelType.Right);
				_rightPanel.CalculateFullItemWidth(graphics, textLayoutRenderer, Font);
				if (_controlBoxMenu.RightPanelWidth > 0 && _rightPanel._itemRenderers.Count > 0)
				{
					_rightPanel._fullItemWidth = _controlBoxMenu.RightPanelWidth;
				}
				if (_rightPanel._fullItemWidth > 0)
				{
					_layoutItems.Add(_rightPanel);
					contentWidth += _rightPanel._fullItemWidth;
					rightPanelHeight = _rightPanel.GetPanelHeight(_owner.SmallItemHeight);
				}
				else
				{
					_rightPanel = null;
				}
			}
			else
			{
				_rightPanel = null;
			}
			if (_controlBoxMenu.BottomCommands.Count > 0 || _isDesignMode)
			{
				_bottomPanel = new ControlBoxPanelRenderer(_controlBoxMenu.BottomCommands, this, this, _owner.SmallItemHeight, ControlBoxPanelType.Bottom);
				_bottomPanel.CalculateFullItemWidth(graphics, textLayoutRenderer, Font);
				if (_bottomPanel._fullItemWidth > 0)
				{
					_layoutItems.Add(_bottomPanel);
					int bottomHeight = _bottomPanel.GetPanelHeight(_owner.SmallItemHeight) + 1;
					bottomHeight += 3;
					_bottomPanelHeight = Math.Max(_bottomPanelHeight, bottomHeight);
					if (_bottomPanel._fullItemWidth > contentWidth)
					{
						contentWidth = _bottomPanel._fullItemWidth;
					}
				}
				else
				{
					_bottomPanel = null;
				}
			}
			else
			{
				_bottomPanel = null;
			}
			height = ((rightPanelHeight <= leftPanelHeight) ? (height + leftPanelHeight) : (height + rightPanelHeight));
			RebuildNavigableItems();
			height += _bottomPanelHeight;
			contentWidth += _contentPadding.Width;
			if (_hasVerticalScrollBar)
			{
				contentWidth += SystemInformation.VerticalScrollBarWidth;
			}
			Size size = new Size(contentWidth, height);
			Point position = GetPosition(size);
			if (size.Height > _maxPopupHeight)
			{
				size.Height = _maxPopupHeight;
				size.Width += SystemInformation.VerticalScrollBarWidth;
				position = GetPosition(size);
				_hasVerticalScrollBar = true;
			}
			_totalItemHeight = Math.Max(leftPanelHeight, rightPanelHeight);
			base.Bounds = new Rectangle(position, size);
			LayoutMenuItems(graphics, textLayoutRenderer);
			_isMenuBoundsDirty = false;
		}

		public override void LayoutMenuItems(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			SuspendLayout();
			_contentBounds = base.ClientRectangle;
			_contentBounds.Inflate(-_contentPadding.Width / 2, 0);
			_contentBounds.Height -= _contentPadding.Height + _bottomPanelHeight;
			_contentBounds.Y += _contentOffsetY;
			int left = _contentBounds.Left;
			int top = _contentBounds.Top;
			int height = _contentBounds.Height;
			int nextPanelX = left;
			if (_scrollOffset < 0)
			{
				_scrollOffset = 0;
			}
			top -= _scrollOffset;
			if (_ribbon != null && (_ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.BackstageMenu || _ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxTab))
			{
				_backstageTabRenderer?.Dispose();
				_backstageTabRenderer = new BackstageMenuTabRenderer(_ribbon.BackstageMenu);
				_backstageTabRenderer.CalculateFullItemWidth(graphics, textRenderer, _ribbon.Font);
				_backstageTabRenderer.ArrangeItem(graphics, textRenderer, 1, 1, _backstageTabRenderer._fullItemWidth, _contentOffsetY - 1);
			}
			if (_leftPanel != null)
			{
				_leftPanel.ArrangeItem(graphics, textRenderer, nextPanelX, top, _leftPanel._fullItemWidth, height + _scrollOffset);
				nextPanelX += _leftPanel._fullItemWidth;
			}
			if (_rightPanel != null)
			{
				_rightPanel.ArrangeItem(graphics, textRenderer, nextPanelX, top, _rightPanel._fullItemWidth, height + _scrollOffset);
			}
			if (_bottomPanel != null)
			{
				_bottomPanel.ArrangeItem(graphics, textRenderer, left, _contentBounds.Bottom + 2, _contentBounds.Width - 4, _bottomPanelHeight - 1);
			}
			if (FocusItem == null && _selectFirstItem)
			{
				foreach (CommandBaseRenderer item in _navigableItems)
				{
					if (item._isEnabled && item.CommandType != CommandType.Separator && (item.CommandType != CommandType.Gallery || ((GalleryMenuRenderer)item).TrySelectFirstItem()))
					{
						FocusItem = item;
						break;
					}
				}
				_selectFirstItem = false;
			}
			int scrollItemHeight = _owner.SmallItemHeight;
			if (_sourceItem != null && _sourceItem.GetCommand() is DropDownCommand && ((DropDownCommand)_sourceItem.GetCommand()).CommandsSizeStyle == CommandSizeStyle.Large)
			{
				scrollItemHeight = _owner.LargeImageSize.Height + 12;
			}
			if (_hasVerticalScrollBar)
			{
				UpdateScrollBar(_contentBounds, 0, _totalItemHeight - 1, _contentBounds.Height, _scrollOffset, scrollItemHeight);
			}
			ResumeLayout();
			_isLayoutDirty = false;
		}

		internal override void ShowChildMenu(CommandBaseRenderer item, bool selectItem)
		{
			if ((GetChildMenu() != null && GetChildMenu()._sourceItem.GetCommand() == item.GetCommand()) || !TryCloseChildMenu(CommandCloseReason.CommandFocused) || !(item.GetCommand() is DropDownCommand dropDownCommand) || !dropDownCommand.CanOpenDropDown())
			{
				return;
			}
			CommandBaseEditorDialog commandBaseEditorDialog = new CommandBaseEditorDialog(this, dropDownCommand.Commands, item, item.GetCommand() as DropDownCommand);
			if (_leftPanel != null && _leftPanel._itemRenderers.Contains(item))
			{
				if (_rightPanel != null)
				{
					commandBaseEditorDialog._minPopupSize = new Size(_rightPanel._bounds.Width - 4, _contentBounds.Height);
					commandBaseEditorDialog._locationOverride = PointToScreen(new Point(_leftPanel._bounds.Right, _leftPanel._bounds.Top));
				}
				else if (_controlBoxMenu.RightPanelWidth > 0)
				{
					commandBaseEditorDialog._minPopupSize = new Size(_controlBoxMenu.RightPanelWidth - 4, _contentBounds.Height);
					commandBaseEditorDialog._locationOverride = PointToScreen(new Point(_leftPanel._bounds.Right, _leftPanel._bounds.Top));
				}
				commandBaseEditorDialog._commandSizeStyle = CommandSizeStyle.Large;
			}
			commandBaseEditorDialog._opensVertically = false;
			commandBaseEditorDialog._isFlipped = false;
			commandBaseEditorDialog._selectFirstItem = selectItem;
			commandBaseEditorDialog.ShowAt(PointToScreen(item._bounds.Location));
		}

		private void DrawThemedFrame(Graphics graphics, ResolvedTheme theme)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle header = new Rectangle(clientRectangle.X, clientRectangle.Top, clientRectangle.Width, _contentOffsetY);
			Rectangle footer = new Rectangle(clientRectangle.X, clientRectangle.Bottom - _bottomPanelHeight, clientRectangle.Width, _bottomPanelHeight);
			graphics.FillRectangle(theme.Brush(ThemeRole.SurfaceAlt), header);
			graphics.FillRectangle(theme.Brush(ThemeRole.SurfaceAlt), footer);
			if (_backstageTabRenderer != null)
			{
				Color foreColor = _backstageTabRenderer.Selected ? _ribbon.ForeColorDark : _ribbon.ForeColorLight;
				using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
				_backstageTabRenderer.Paint(graphics, textRenderer, _ribbon._colorScheme, _ribbon.Font, foreColor, clientRectangle);
			}
			Pen pen = theme.Pen(ThemeRole.Border);
			graphics.DrawRectangle(pen, clientRectangle.X, clientRectangle.Y, clientRectangle.Width - 1, clientRectangle.Height - 1);
			graphics.DrawLine(pen, clientRectangle.X + 1, header.Bottom - 1, clientRectangle.Right - 2, header.Bottom - 1);
			graphics.DrawLine(pen, footer.X, footer.Y, footer.Right - 1, footer.Y);
			DrawApplicationButton(graphics);
		}

		private void DrawOffice2010Frame(Graphics graphics)
		{
			DrawThemedFrame(graphics, _popupColorScheme.Theme);
			return;
		}

		protected override void DrawBorder(Graphics graphics)
		{
			DrawOffice2010Frame(graphics);
			return;
		}

		protected override void OnDragDrop(DragEventArgs eventArgs)
		{
			FocusItem = null;
			if (eventArgs.Data.GetDataPresent(typeof(CommandBaseState)))
			{
				CommandBaseState commandBaseState = (CommandBaseState)eventArgs.Data.GetData(typeof(CommandBaseState));
				Point point = PointToClient(new Point(eventArgs.X, eventArgs.Y));
				CommandBaseRenderer commandBaseRenderer = FindRendererAt(point.X, point.Y);
				if (commandBaseRenderer != null && commandBaseRenderer != commandBaseState.GetRenderer())
				{
					int dropIndex = 0;
					if (_leftPanel != null && _leftPanel._itemRenderers.Contains(commandBaseRenderer))
					{
						dropIndex = _leftPanel._itemRenderers.IndexOf(commandBaseRenderer);
						if (_controlBoxMenu.LeftCommands.Contains(commandBaseState.GetRenderer().GetCommand()))
						{
							_controlBoxMenu.LeftCommands.MoveCommandTo(dropIndex, commandBaseState.GetRenderer().GetCommand());
						}
						else
						{
							_controlBoxMenu.RemoveFromAllPanels(commandBaseState.GetRenderer().GetCommand());
							_controlBoxMenu.LeftCommands.Insert(dropIndex, commandBaseState.GetRenderer().GetCommand());
						}
					}
					else if (_rightPanel != null && _rightPanel._itemRenderers.Contains(commandBaseRenderer))
					{
						dropIndex = _rightPanel._itemRenderers.IndexOf(commandBaseRenderer);
						if (_controlBoxMenu.RightCommands.Contains(commandBaseState.GetRenderer().GetCommand()))
						{
							_controlBoxMenu.RightCommands.MoveCommandTo(dropIndex, commandBaseState.GetRenderer().GetCommand());
						}
						else
						{
							_controlBoxMenu.RemoveFromAllPanels(commandBaseState.GetRenderer().GetCommand());
							_controlBoxMenu.RightCommands.Insert(dropIndex, commandBaseState.GetRenderer().GetCommand());
						}
					}
					else if (_bottomPanel != null && _bottomPanel._itemRenderers.Contains(commandBaseRenderer))
					{
						dropIndex = _bottomPanel._itemRenderers.IndexOf(commandBaseRenderer);
						if (_controlBoxMenu.BottomCommands.Contains(commandBaseState.GetRenderer().GetCommand()))
						{
							_controlBoxMenu.BottomCommands.MoveCommandTo(dropIndex, commandBaseState.GetRenderer().GetCommand());
						}
						else
						{
							_controlBoxMenu.RemoveFromAllPanels(commandBaseState.GetRenderer().GetCommand());
							_controlBoxMenu.BottomCommands.Insert(dropIndex, commandBaseState.GetRenderer().GetCommand());
						}
					}
					MarkMenuBoundsDirty();
				}
			}
			base.OnDragDrop(eventArgs);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			if (base.ClientRectangle.Width <= 0 || base.ClientRectangle.Height <= 0)
			{
				return;
			}
			Brush brush = GdiCache.Brush(_popupColorScheme._dialogBackground);
			eventArgs.Graphics.FillRectangle(brush, base.ClientRectangle);
			if (_layoutItems != null && _layoutItems.Count > 0)
			{
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
				textLayoutRenderer.SetShowHotkeyPrefix(true);
				if (_isMenuBoundsDirty)
				{
					RebuildMenuAndBounds(eventArgs.Graphics);
				}
				if (_isLayoutDirty)
				{
					LayoutMenuItems(eventArgs.Graphics, textLayoutRenderer);
				}
				Color foreColor = ForeColor;
				if (GetRibbon() != null)
				{
					foreColor = GetRibbon().ForeColorDark;
				}
				foreach (CommandBaseRenderer item in _layoutItems)
				{
					if (item != _bottomPanel)
					{
						Rectangle rectangle = Rectangle.Intersect(eventArgs.ClipRectangle, item._bounds);
						if (rectangle.Width > 0 && rectangle.Height > 0)
						{
							item.Paint(eventArgs.Graphics, textLayoutRenderer, _owner.ColorScheme, Font, foreColor, eventArgs.ClipRectangle);
						}
					}
				}
				DrawBorder(eventArgs.Graphics);
				if (_bottomPanel != null)
				{
					_bottomPanel.Paint(eventArgs.Graphics, textLayoutRenderer, _owner.ColorScheme, Font, foreColor, eventArgs.ClipRectangle);
				}
			}
			else
			{
				DrawBorder(eventArgs.Graphics);
			}
			if (_leftPanel != null && _rightPanel != null)
			{
				eventArgs.Graphics.DrawLine(_popupColorScheme.Theme.Pen(ThemeRole.Border), _leftPanel._bounds.Right - 1, _leftPanel._bounds.Top, _leftPanel._bounds.Right - 1, _leftPanel._bounds.Bottom - 1);
			}
			if (_isDesignMode)
			{
				base.DrawDesignSelectionBorders(eventArgs.Graphics);
			}
		}

		private void DrawApplicationButton(Graphics graphics)
		{
			if (_ribbon._controlBoxButtonRenderer == null)
			{
				return;
			}
			Rectangle ribbonRectangle = _ribbon._controlBoxButtonBounds;
			if (_ribbon.IsCaptionBarShown())
			{
				ribbonRectangle.Y -= _ribbon._captionHeight;
			}
			if (_ribbon._controlBoxButtonRenderer.Pushed || _ribbon._controlBoxButtonRenderer.IsMenuOpen())
			{
				RibbonChromePainter.DrawOrbPressed(graphics, ribbonRectangle, _popupColorScheme);
			}
			else if (_ribbon._controlBoxButtonRenderer.Hot || _ribbon._controlBoxButtonRenderer.Active || _ribbon._controlBoxButtonRenderer._fadeFromHot)
			{
				if (_ribbon._controlBoxButtonRenderer._pressAnimationAlpha != 255)
				{
					RibbonChromePainter.DrawOrbNormal(graphics, ribbonRectangle, _popupColorScheme);
				}
				RibbonChromePainter.DrawOrbHot(graphics, ribbonRectangle, _ribbon._controlBoxButtonRenderer._pressAnimationAlpha, _popupColorScheme);
			}
			else
			{
				RibbonChromePainter.DrawOrbNormal(graphics, ribbonRectangle, _popupColorScheme);
			}
			if (_ribbon.GetHostForm() != null)
			{
				Rectangle rectangle = ribbonRectangle;
				rectangle.Inflate(-6, -6);
				rectangle.X++;
				rectangle.Y++;
				RibbonDrawingUtils.DrawIcon(graphics, _ribbon.ResolveControlBoxIcon(), rectangle, true, SystemColors.Window, _ribbon.IsGlassCaptionActive());
			}
		}

		public override void HandleKeyLeft()
		{
			if (FocusItem != null && GetChildMenu() == null)
			{
				if (_rightPanel != null && _rightPanel._itemRenderers.Contains(FocusItem))
				{
					if (_leftPanel != null)
					{
						int leftProbeX = _leftPanel._bounds.X + 4;
						int leftProbeY = FocusItem._bounds.Top + FocusItem._bounds.Height / 2;
						CommandBaseRenderer commandBaseRenderer = _leftPanel.FindItemAt(leftProbeX, leftProbeY);
						if (commandBaseRenderer == null)
						{
							FocusItem = _leftPanel.GetLastNonSeparatorItem();
						}
						if (commandBaseRenderer != null)
						{
							FocusItem = commandBaseRenderer;
						}
					}
					return;
				}
				if (_leftPanel != null && _leftPanel._itemRenderers.Contains(FocusItem))
				{
					if (_rightPanel != null)
					{
						int rightProbeX = _rightPanel._bounds.X + 4;
						int rightProbeY = FocusItem._bounds.Top + FocusItem._bounds.Height / 2;
						CommandBaseRenderer rightTarget = _rightPanel.FindItemAt(rightProbeX, rightProbeY);
						if (rightTarget == null)
						{
							FocusItem = _rightPanel.GetLastNonSeparatorItem();
						}
						if (rightTarget != null)
						{
							FocusItem = rightTarget;
						}
					}
					return;
				}
				if (_bottomPanel != null && _bottomPanel._itemRenderers.Contains(FocusItem))
				{
					int focusIndex = _bottomPanel._itemRenderers.IndexOf(FocusItem);
					for (int previousIndex = focusIndex - 1; previousIndex >= 0; previousIndex--)
					{
						if (_bottomPanel._itemRenderers[previousIndex]._isVisible)
						{
							FocusItem = _bottomPanel._itemRenderers[previousIndex];
							return;
						}
					}
					for (int lastIndex = _bottomPanel._itemRenderers.Count - 1; lastIndex >= 0; lastIndex--)
					{
						if (_bottomPanel._itemRenderers[lastIndex]._isVisible)
						{
							FocusItem = _bottomPanel._itemRenderers[lastIndex];
							return;
						}
					}
				}
			}
			if (_owner is CommandBaseEditorDialog)
			{
				TryCloseOwnerChildMenu(CommandCloseReason.Keyboard);
			}
		}

		public override bool HandleKeyRight(NativeMethods.NativeMessage msg)
		{
			if (FocusItem != null && GetChildMenu() == null)
			{
				if (FocusItem._isEnabled && FocusItem.HasChildren)
				{
					NativeMethods.NativeMessage n = default(NativeMethods.NativeMessage);
					NativeMethods.GetMessage(ref n, IntPtr.Zero, 0u, 0u);
					ShowChildMenu(FocusItem, selectItem: true);
					return true;
				}
				if (_leftPanel != null && _leftPanel._itemRenderers.Contains(FocusItem))
				{
					if (_rightPanel != null)
					{
						int rightProbeX = _rightPanel._bounds.X + 4;
						int rightProbeY = FocusItem._bounds.Top + FocusItem._bounds.Height / 2;
						CommandBaseRenderer commandBaseRenderer = _rightPanel.FindItemAt(rightProbeX, rightProbeY);
						if (commandBaseRenderer == null)
						{
							FocusItem = _rightPanel.GetLastNonSeparatorItem();
						}
						if (commandBaseRenderer != null)
						{
							FocusItem = commandBaseRenderer;
						}
					}
					return false;
				}
				if (_rightPanel != null && _rightPanel._itemRenderers.Contains(FocusItem))
				{
					if (_leftPanel != null)
					{
						int leftProbeX = _leftPanel._bounds.X + 4;
						int leftProbeY = FocusItem._bounds.Top + FocusItem._bounds.Height / 2;
						CommandBaseRenderer leftTarget = _leftPanel.FindItemAt(leftProbeX, leftProbeY);
						if (leftTarget == null)
						{
							FocusItem = _leftPanel.GetLastNonSeparatorItem();
						}
						if (leftTarget != null)
						{
							FocusItem = leftTarget;
						}
					}
					return false;
				}
				if (_bottomPanel != null && _bottomPanel._itemRenderers.Contains(FocusItem))
				{
					int focusIndex = _bottomPanel._itemRenderers.IndexOf(FocusItem);
					for (int i = focusIndex + 1; i < _bottomPanel._itemRenderers.Count; i++)
					{
						if (_bottomPanel._itemRenderers[i]._isVisible)
						{
							FocusItem = _bottomPanel._itemRenderers[i];
							return false;
						}
					}
					for (int j = 0; j < _bottomPanel._itemRenderers.Count; j++)
					{
						if (_bottomPanel._itemRenderers[j]._isVisible)
						{
							FocusItem = _bottomPanel._itemRenderers[j];
							return false;
						}
					}
				}
			}
			return false;
		}

		public override void SelectPrevious()
		{
			int startIndex = -1;
			if (FocusItem != null)
			{
				startIndex = _navigableItems.IndexOf(FocusItem) - 1;
			}
			int count = _navigableItems.Count;
			if (count <= 0)
			{
				return;
			}
			if (startIndex < 0)
			{
				startIndex = _navigableItems.Count - 1;
			}
			for (int i = startIndex; i >= 0; i--)
			{
				CommandBaseRenderer thisValue = _navigableItems[i];
				if (thisValue.CommandType == CommandType.Separator)
				{
					if (startIndex != 0)
					{
						continue;
					}
					thisValue = FindLastNonSeparatorItem();
					if (thisValue == null)
					{
						break;
					}
				}
				else if (thisValue.CommandType == CommandType.Gallery && !((GalleryMenuRenderer)thisValue).TrySelectLastItem())
				{
					continue;
				}
				FocusItem = thisValue;
				if (_controlBoxMenu != null && thisValue.GetCommand() != null && !_controlBoxMenu.BottomCommands.Contains(thisValue.GetCommand()))
				{
					if (thisValue._bounds.Bottom > _contentBounds.Bottom)
					{
						SetScrollOffset(GetScrollOffset() + (thisValue._bounds.Bottom - _contentBounds.Bottom));
					}
					else if (thisValue._bounds.Top < _contentBounds.Top)
					{
						SetScrollOffset(GetScrollOffset() - (_contentBounds.Top - thisValue._bounds.Top));
					}
				}
				break;
			}
		}

		public override void SelectNext()
		{
			int startIndex = 0;
			if (FocusItem != null)
			{
				startIndex = _navigableItems.IndexOf(FocusItem) + 1;
			}
			int endIndex = _navigableItems.Count;
			if (endIndex <= 0)
			{
				return;
			}
			CommandBaseRenderer thisValue = _navigableItems[endIndex - 1];
			if (thisValue.CommandType == CommandType.Separator)
			{
				endIndex--;
			}
			if (startIndex > endIndex - 1)
			{
				FocusItem = null;
				_selectFirstItem = true;
				SetScrollOffset(0);
				MarkLayoutDirty();
				return;
			}
			for (int i = startIndex; i < endIndex; i++)
			{
				CommandBaseRenderer candidate = _navigableItems[i];
				if (candidate.CommandType != CommandType.Separator)
				{
					if (candidate.CommandType != CommandType.Gallery || ((GalleryMenuRenderer)candidate).TrySelectFirstItem())
					{
						FocusItem = candidate;
						if (_controlBoxMenu != null && candidate.GetCommand() != null && !_controlBoxMenu.BottomCommands.Contains(candidate.GetCommand()) && candidate._bounds.Bottom > _contentBounds.Bottom)
						{
							SetScrollOffset(GetScrollOffset() + (candidate._bounds.Bottom - _contentBounds.Bottom));
						}
						break;
					}
				}
				else if (startIndex == endIndex - 1)
				{
					FocusItem = null;
					_selectFirstItem = true;
					SetScrollOffset(0);
					MarkLayoutDirty();
				}
			}
		}

		private CommandBaseRenderer FindLastNonSeparatorItem()
		{
			for (int i = _navigableItems.Count - 1; i >= 0; i--)
			{
				CommandBaseRenderer commandBaseRenderer = _navigableItems[i];
				if (commandBaseRenderer.CommandType != CommandType.Separator)
				{
					return commandBaseRenderer;
				}
			}
			return null;
		}

		internal override IList<RibbonKeyTipEntry> GetKeyTips(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			if (GetChildMenu() != null)
			{
				return GetChildMenu().GetKeyTips(graphics, textRenderer);
			}
			IList<RibbonKeyTipEntry> list = new List<RibbonKeyTipEntry>();
			if (GetRibbon() != null)
			{
				foreach (CommandBaseRenderer menuItem in _navigableItems)
				{
					if (!(menuItem.GetCommand() is IKeyTipProvider))
					{
						continue;
					}
					IKeyTipProvider keyTipProvider = (IKeyTipProvider)menuItem.GetCommand();
					if (keyTipProvider.HasKeyTip)
					{
						Point keyTipLocation = menuItem.GetKeyTipLocation(graphics, textRenderer, Font, keyTipProvider.KeyTip);
						if (keyTipLocation.Y < base.ClientRectangle.Bottom && keyTipLocation.Y >= base.ClientRectangle.Top)
						{
							SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(Font, _owner.UseCompatibleTextRendering, keyTipProvider.KeyTip, _popupColorScheme);
							superTipSettingsWindowMessageHandler.SetCompactMode(true);
							superTipSettingsWindowMessageHandler._isKeyTipEnabled = menuItem.GetCommand().IsEnabled;
							superTipSettingsWindowMessageHandler.SetSticky(true);
							superTipSettingsWindowMessageHandler.ShowAt(_owner.ParentControl, PointToScreen(keyTipLocation));
							RibbonKeyTipEntry item = new RibbonKeyTipEntry(keyTipProvider.KeyTip, menuItem.GetCommand(), menuItem, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
							list.Add(item);
						}
					}
				}
			}
			return list;
		}

		internal override bool OnComponentSelected(ISelectionService selectionService)
		{
			EndInPlaceEdit(false);
			if (selectionService != null && selectionService.SelectionCount > 0)
			{
				foreach (Component selectedComponent in selectionService.GetSelectedComponents())
				{
					if (!(selectedComponent is CommandBase))
					{
						continue;
					}
					CommandBase commandBase = selectedComponent as CommandBase;
					if (GetParentMenuCommand() == selectedComponent)
					{
						SetDesignSelectedItem(null);
						return true;
					}
					bool isGalleryItem = commandBase is GalleryItem;
					foreach (CommandBaseRenderer item in _navigableItems)
					{
						if (isGalleryItem)
						{
							if (item.GetCommand() == commandBase.GetParentContainer())
							{
								SetDesignSelectedItem(null);
								return true;
							}
						}
						else if (item.GetCommand() == commandBase)
						{
							SetDesignSelectedItem(item);
							return true;
						}
					}
				}
				SetDesignSelectedItem(null);
				if (GetChildMenu() != null)
				{
					if (!GetChildMenu().OnComponentSelected(selectionService))
					{
						ClearOwnerChildMenu();
					}
					return false;
				}
				ClearOwnerChildMenu();
			}
			else
			{
				SetDesignSelectedItem(null);
				if (GetChildMenu() != null)
				{
					GetChildMenu().OnComponentSelected(selectionService);
					return false;
				}
			}
			return false;
		}
	}
}
