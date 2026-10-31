using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class OutlookNavigatorGroupState : DockPanelGroupState, IPanelButtonOwner
	{
		private const string ResizeKind = "Resize";

		private const string MoreItemsKind = "MoreItems";

		private const string GrabKind = "Grab";

		private const string ChevronKind = "Chevron";

		private const string SemiKind = "Semi";

		private const int ResizeBarHeight = 6;

		private const int MinButtonHeight = 24;

		private const int ButtonVerticalPadding = 8;

		private const int SmallButtonWidth = 22;

		private const int GrabHandleWidth = 12;

		private const int ChevronButtonWidth = 18;

		private const int ChevronButtonRightMargin = 19;

		private const int PanelDragStartOffset = 10;

		private int _resizeBarHeight;

		private bool _isLayoutUpdating;

		private OutlookPanelButton _pressedButton;

		private OutlookPanelButton _hotButton;

		internal IList<OutlookPanelButton> _buttons;

		internal OutlookPanelButton _resizeButton;

		private Rectangle _contentBounds;

		private int _buttonHeight;

		private int _lastSmallButtonIndex;

		internal override bool HasBorderPanel => true;

		internal override Size MinimumSize
		{
			get
			{
				int minWidth = -1;
				int minHeight = -1;
				foreach (DockPanelBase item in GetVisiblePanels())
				{
					Size minimumSize = item.GetMinimumSize();
					if (minWidth < minimumSize.Width)
					{
						minWidth = minimumSize.Width;
					}
				}
				if (GetSelectedPanel() != null)
				{
					minHeight = GetSelectedPanel().GetMinimumSize().Height;
				}
				minHeight = (_panelGroup.AllowOutlookButtonsResize ? (minHeight + (_buttonHeight + _resizeBarHeight + 1)) : (minHeight + (_buttonHeight * GetVisiblePanels().Count + _resizeBarHeight + 1)));
				return new Size(minWidth, minHeight);
			}
		}

		internal override Size MaximumSize
		{
			get
			{
				int maxWidth = 0;
				int maxHeight = 0;
				foreach (DockPanelBase item in GetVisiblePanels())
				{
					Size maximumSize = item.GetMaximumSize();
					if (maxHeight > maximumSize.Height || maxHeight == 0)
					{
						maxHeight = maximumSize.Height;
					}
					if (maxWidth > maximumSize.Width || maxWidth == 0)
					{
						maxWidth = maximumSize.Width;
					}
				}
				return new Size(maxWidth, maxHeight);
			}
		}

		internal OutlookNavigatorGroupState(MxDockPanelGroup panelGroup)
			: base(panelGroup)
		{
			_buttons = new List<OutlookPanelButton>();
		}

		internal override void RefreshGroupLayout()
		{
			if (GetVisiblePanels().Count > 0)
			{
				if (GetSelectedPanel() == null || GetSelectedPanel().Closed || GetSelectedPanel().ParentGroup != _panelGroup)
				{
					SetSelectedPanel((DockPanelBase)GetVisiblePanels()[0]);
				}
			}
			else if (GetSelectedPanel() != null && _panelGroup.ContainsPanel(GetSelectedPanel()))
			{
				GetSelectedPanel().Visible = true;
			}
			else if (GetSelectedPanel() == null && _panelGroup.Panels.Count > 0)
			{
				SetSelectedPanel(_panelGroup.Panels[0]);
			}
			if (GetSelectedPanel() != null)
			{
				GetSelectedPanel().BringToFront();
				if (_panelGroup.GetAutoHidePanel() == null)
				{
					LayoutButtons();
					{
						foreach (DockPanelBase item in (IEnumerable)_panelGroup.Panels)
						{
							item.Bounds = _contentBounds;
							if (item.Closed && item != GetSelectedPanel())
							{
								item.Visible = false;
							}
						}
						return;
					}
				}
				{
					foreach (DockPanelBase groupedPanel in (IEnumerable)_panelGroup.Panels)
					{
						groupedPanel.Bounds = ContentBounds;
					}
					return;
				}
			}
			if (_buttons != null)
			{
				_buttons = new List<OutlookPanelButton>();
			}
		}

		void IPanelButtonOwner.Invalidate(object panelButton)
		{
			if (panelButton is OutlookPanelButton uiPanelBaseLayoutState && !_isLayoutUpdating && _panelGroup != null)
			{
				_panelGroup.Invalidate(uiPanelBaseLayoutState.GetBounds());
			}
		}

		private void SetPressedButton(OutlookPanelButton uiPanelBaseLayoutState)
		{
			if (_pressedButton != uiPanelBaseLayoutState)
			{
				if (_pressedButton != null)
				{
					_pressedButton.SetPressed(false);
				}
				_pressedButton = uiPanelBaseLayoutState;
				if (_pressedButton != null)
				{
					_pressedButton.SetPressed(true);
				}
			}
		}

		private void SetHotButton(OutlookPanelButton dockRenderer5UIPanelBaseLayoutState)
		{
			if (_hotButton == dockRenderer5UIPanelBaseLayoutState)
			{
				return;
			}
			if (_hotButton != null)
			{
				_hotButton.SetHot(false);
			}
			_hotButton = dockRenderer5UIPanelBaseLayoutState;
			string text = "";
			if (_hotButton != null)
			{
				_hotButton.SetHot(true);
				if (_hotButton.GetKind() == SemiKind)
				{
					text = _hotButton.GetPanel().Text;
				}
			}
			_panelGroup.ApplyToolTipText(text);
		}

		private OutlookPanelButton FindPanelButton(DockPanelBase uiPanelBase)
		{
			foreach (OutlookPanelButton item in _buttons)
			{
				if (item.GetPanel() == uiPanelBase && item.GetKind().Length == 0)
				{
					return item;
				}
			}
			return null;
		}

		private int GetVisibleButtonCount()
		{
			if (_panelGroup.VisibleOutlookButtons == -1 || _panelGroup.VisibleOutlookButtons > GetVisiblePanels().Count)
			{
				return GetVisiblePanels().Count;
			}
			return _panelGroup.VisibleOutlookButtons;
		}

		private void SetVisibleButtonCount(int visibleCount)
		{
			if (visibleCount >= GetVisiblePanels().Count)
			{
				visibleCount = -1;
			}
			if (_panelGroup.VisibleOutlookButtons != visibleCount)
			{
				_panelGroup.VisibleOutlookButtons = visibleCount;
			}
		}

		private void LayoutButtons()
		{
			_buttons.Clear();
			Rectangle displayRectangle = ContentBounds;
			if (_panelGroup.HasPanelBorder())
			{
				displayRectangle.Inflate(1, 1);
			}
			_isLayoutUpdating = true;
			int panelHeight = 0;
			panelHeight = GetSelectedPanel().GetMinimumSize().Height;
			int availableHeight = displayRectangle.Height - panelHeight;
			int usedHeight = 0;
			OutlookPanelButton uiPanelBaseLayoutState = null;
			_buttonHeight = GetButtonHeight();
			_resizeBarHeight = ResizeBarHeight;
			if (_panelGroup.AllowOutlookButtonsResize)
			{
				if (_resizeButton == null)
				{
					_resizeButton = new OutlookPanelButton(this, null, AccessibleRole.PushButton, "Button Splitter");
				}
				_resizeButton.SetKind(ResizeKind);
				_resizeButton.SetBounds(new Rectangle(displayRectangle.X, 0, displayRectangle.Width, _resizeBarHeight));
				_buttons.Add(_resizeButton);
				usedHeight += _resizeBarHeight;
				uiPanelBaseLayoutState = new OutlookPanelButton(this, null, AccessibleRole.None);
				uiPanelBaseLayoutState.SetBounds(new Rectangle(displayRectangle.X, 0, displayRectangle.Width, _buttonHeight - 1));
				uiPanelBaseLayoutState.SetKind(MoreItemsKind);
				uiPanelBaseLayoutState._canHover = false;
				usedHeight += _buttonHeight;
			}
			else
			{
				_resizeButton = null;
			}
			int fullButtonCount = GetVisibleButtonCount();
			for (int i = 0; i < fullButtonCount; i++)
			{
				usedHeight += _buttonHeight;
				if (usedHeight > availableHeight)
				{
					usedHeight -= _buttonHeight;
					fullButtonCount = i;
					break;
				}
				DockPanelBase uiPanelBase = (DockPanelBase)GetVisiblePanels()[i];
				OutlookPanelButton panelButton = new OutlookPanelButton(this, uiPanelBase, AccessibleRole.PushButton);
				panelButton.SetBounds(new Rectangle(displayRectangle.X, 0, displayRectangle.Width, _buttonHeight));
				panelButton._canHover = true;
				panelButton.SetImageSource(uiPanelBase.GetLargeImageSource());
				panelButton.SetText(uiPanelBase.Text);
				panelButton.SetSelected(GetSelectedPanel() == uiPanelBase);
				_buttons.Add(panelButton);
			}
			if (uiPanelBaseLayoutState != null)
			{
				_buttons.Add(uiPanelBaseLayoutState);
			}
			panelHeight = displayRectangle.Height - usedHeight;
			if (panelHeight < GetSelectedPanel().GetMinimumSize().Height)
			{
				panelHeight = GetSelectedPanel().GetMinimumSize().Height;
			}
			int buttonTop = displayRectangle.Y + panelHeight;
			foreach (OutlookPanelButton item in _buttons)
			{
				Rectangle rectangle = item.GetBounds();
				rectangle.Y = buttonTop;
				buttonTop += rectangle.Height;
				rectangle.Height++;
				item.SetBounds(rectangle);
			}
			int smallButtonRight = displayRectangle.Right - 1;
			if (_panelGroup.ShowOutlookNavigatorConfigureMenu && _buttons.Count > 0)
			{
				OutlookPanelButton thisValue = _buttons[_buttons.Count - 1];
				OutlookPanelButton chevronButton = new OutlookPanelButton(this, null, AccessibleRole.PushButton, "Configure buttons");
				chevronButton.SetBounds(new Rectangle(thisValue.GetBounds().Right - ChevronButtonRightMargin, thisValue.GetBounds().Top + 1, ChevronButtonWidth, thisValue.GetBounds().Height - 2));
				chevronButton.SetKind(ChevronKind);
				_buttons.Add(chevronButton);
				smallButtonRight = chevronButton.GetBounds().X;
			}
			if (uiPanelBaseLayoutState != null)
			{
				int smallButtonWidth = SmallButtonWidth;
				int smallButtonCapacity = (smallButtonRight - displayRectangle.X) / smallButtonWidth;
				_lastSmallButtonIndex = fullButtonCount + smallButtonCapacity;
				if (_lastSmallButtonIndex > GetVisiblePanels().Count)
				{
					_lastSmallButtonIndex = GetVisiblePanels().Count;
				}
				_lastSmallButtonIndex--;
				for (int smallButtonIndex = _lastSmallButtonIndex; smallButtonIndex >= fullButtonCount; smallButtonIndex--)
				{
					smallButtonRight -= smallButtonWidth - 1;
					DockPanelBase smallButtonPanel = (DockPanelBase)GetVisiblePanels()[smallButtonIndex];
					OutlookPanelButton smallButton = new OutlookPanelButton(this, smallButtonPanel, AccessibleRole.PushButton);
					smallButton.SetBounds(new Rectangle(smallButtonRight, uiPanelBaseLayoutState.GetBounds().Top + 1, smallButtonWidth, _buttonHeight - 2));
					smallButton.SetKind(SemiKind);
					smallButton.SetImageSource(smallButtonPanel.GetImageSource());
					smallButton.SetSelected(GetSelectedPanel() == smallButtonPanel);
					_buttons.Add(smallButton);
				}
			}
			if (!_panelGroup.StaticGroup)
			{
				for (int buttonIndex = _buttons.Count - 1; buttonIndex >= 0; buttonIndex--)
				{
					OutlookPanelButton sourceButton = _buttons[buttonIndex];
					if (sourceButton.GetPanel() != null && sourceButton.GetKind().Length == 0 && sourceButton.GetPanel().CanDragPanel())
					{
						OutlookPanelButton grabButton = new OutlookPanelButton(this, sourceButton.GetPanel(), AccessibleRole.None, "");
						grabButton._canHover = false;
						grabButton.SetKind(GrabKind);
						grabButton.SetBounds(new Rectangle(sourceButton.GetBounds().X, sourceButton.GetBounds().Y, GrabHandleWidth, sourceButton.GetBounds().Height));
						_buttons.Add(grabButton);
					}
				}
			}
			_contentBounds = new Rectangle(displayRectangle.X, displayRectangle.Top, displayRectangle.Width, panelHeight);
			_isLayoutUpdating = false;
		}

		private int GetButtonHeight()
		{
			int buttonHeight = (int)_panelGroup.Font.GetHeight();
			if (buttonHeight < MinButtonHeight)
			{
				buttonHeight = MinButtonHeight;
			}
			return buttonHeight + ButtonVerticalPadding;
		}

		internal override void ProcessPaint(PaintEventArgs eventArgs)
		{
			if (_buttons == null)
			{
				return;
			}
			DockManagerRendererBase dockRenderer = _panelGroup.PanelManager.GetPanelRenderer();
			foreach (OutlookPanelButton item in _buttons)
			{
				RenderContent(eventArgs.Graphics, dockRenderer, item);
			}
		}

		private void RenderContent(Graphics graphics, DockManagerRendererBase dockRenderer, OutlookPanelButton uiPanelBaseLayoutState)
		{
			FormatStyle uiFormatStyle = null;
			string text;
			if ((text = uiPanelBaseLayoutState.GetKind()) != null)
			{
				if (text == ResizeKind)
				{
					dockRenderer.DrawResizeBar(graphics, _panelGroup, uiPanelBaseLayoutState.GetBounds());
					return;
				}
				if (text == MoreItemsKind)
				{
					uiFormatStyle = GetButtonFormatStyle(uiPanelBaseLayoutState);
					dockRenderer.DrawBarGroupButtonBackground(graphics, uiPanelBaseLayoutState.GetPanel(), uiPanelBaseLayoutState.GetBounds(), ButtonState.Normal, uiFormatStyle);
					dockRenderer.DrawBarGroupButtonBorder(graphics, _panelGroup, uiPanelBaseLayoutState.GetBounds(), ButtonState.Normal);
					return;
				}
				if (text == GrabKind)
				{
					dockRenderer.DrawGrabItem(graphics, uiPanelBaseLayoutState.GetBounds());
					return;
				}
				if (text == ChevronKind)
				{
					uiFormatStyle = GetButtonFormatStyle(uiPanelBaseLayoutState);
					dockRenderer.DrawBarGroupSmallButtonBackground(graphics, null, uiPanelBaseLayoutState.GetBounds(), uiPanelBaseLayoutState.GetButtonState(), uiFormatStyle);
					dockRenderer.DrawBarGroupChevron(graphics, uiPanelBaseLayoutState.GetBounds(), uiPanelBaseLayoutState.GetButtonState(), uiFormatStyle);
					return;
				}
				if (text == SemiKind)
				{
					uiFormatStyle = GetButtonFormatStyle(uiPanelBaseLayoutState);
					dockRenderer.DrawBarGroupSmallButton(graphics, uiPanelBaseLayoutState.GetPanel(), uiPanelBaseLayoutState.GetBounds(), uiPanelBaseLayoutState.GetButtonState(), uiPanelBaseLayoutState.GetImageSource(), uiFormatStyle);
					return;
				}
			}
			uiFormatStyle = GetButtonFormatStyle(uiPanelBaseLayoutState);
			dockRenderer.DrawBarGroupButton(graphics, uiPanelBaseLayoutState.GetPanel(), uiPanelBaseLayoutState.GetBounds(), uiPanelBaseLayoutState.GetButtonState(), uiPanelBaseLayoutState.GetImageSource(), uiPanelBaseLayoutState.GetText(), uiFormatStyle, uiPanelBaseLayoutState.GetPanel().GetCaptionFont(uiFormatStyle));
		}

		private FormatStyle GetButtonFormatStyle(OutlookPanelButton uiPanelBaseLayoutState)
		{
			MxDockManager uiPanelManager = ((_panelGroup != null) ? _panelGroup.PanelManager : null);
			ButtonState buttonState = uiPanelBaseLayoutState.GetButtonState();
			if (uiPanelBaseLayoutState.GetPanel() == null)
			{
				return _panelGroup.PanelManager.GetOutlookButtonStyle(buttonState);
			}
			return uiPanelBaseLayoutState.GetPanel().GetNavigatorButtonFormatStyle(buttonState);
		}

		internal override void OnSelectedPanelChanged(bool activate)
		{
			if (GetSelectedPanel() != null)
			{
				GetSelectedPanel().RecalculateLayout();
				GetSelectedPanel().BringToFront();
			}
			else if (GetVisiblePanels().Count > 0 && (GetSelectedPanel() == null || GetSelectedPanel().Closed))
			{
				SetSelectedPanel((DockPanelBase)GetVisiblePanels()[0]);
			}
			DockPanelBase uiPanelBase = null;
			if (_hotButton != null)
			{
				uiPanelBase = _hotButton.GetPanel();
			}
			_panelGroup.RecalculateLayout();
			if (uiPanelBase != null)
			{
				SetHotButton(FindPanelButton(uiPanelBase));
			}
			_panelGroup.Update();
			base.OnSelectedPanelChanged(activate);
		}

		internal override bool ShouldDragGroup()
		{
			return true;
		}

		internal override bool ProcessCloseButtonClick(DockPanelBase panel)
		{
			if (_panelGroup.IsFloating())
			{
				_panelGroup.Closed = true;
				return true;
			}
			return false;
		}

		internal override bool ProcessAutoHideButtonClick(DockPanelBase panel)
		{
			if (_panelGroup.AutoHide == panel.AutoHide)
			{
				_panelGroup.AutoHide = !_panelGroup.AutoHide;
			}
			else
			{
				panel.AutoHide = !panel.AutoHide;
			}
			if (_panelGroup.PanelManager != null)
			{
				_panelGroup.PanelManager.RestoreDockLayout();
			}
			_panelGroup.SelectedPanel = panel;
			_panelGroup.RecalculateLayout();
			if (!panel.AutoHide)
			{
				panel.Activate();
			}
			return true;
		}

		internal override void ProcessChildDropInfo(PanelDropInfo dropInfo)
		{
			if (dropInfo._createsGroup)
			{
				if (dropInfo._groupStyle == PanelGroupStyle.VerticalTiles)
				{
					dropInfo._dropBounds.Height = ContentBounds.Height;
				}
				else if (dropInfo._groupStyle == PanelGroupStyle.HorizontalTiles)
				{
					dropInfo._dropBounds.Width = dropInfo._targetPanel.Bounds.Width;
				}
				if (dropInfo._groupStyle != PanelGroupStyle.HorizontalTiles && dropInfo._groupStyle != PanelGroupStyle.VerticalTiles)
				{
					dropInfo._createsGroup = false;
					dropInfo._insertIndex = GetVisiblePanels().Count;
					dropInfo._dropBounds.Height = _panelGroup.DisplayRectangle.Height;
					dropInfo._tabHeaderBounds = new Rectangle(dropInfo._dropBounds.X, dropInfo._dropBounds.Top + dropInfo._targetPanel.GetPanelCaptionHeight(), dropInfo._dropBounds.Width, 0);
				}
				dropInfo._targetPanel = _panelGroup;
			}
		}

		internal override BorderEdges GetChildPanelEdges(DockPanelBase childPanel)
		{
			if (childPanel.GetAutoHidePanel() == null)
			{
				return BorderEdges.Left | BorderEdges.Top | BorderEdges.Right;
			}
			return base.GetChildPanelEdges(childPanel);
		}

		private OutlookPanelButton HitTestInteractiveButton(int x, int y)
		{
			return HitTestButton(x, y, true);
		}

		internal OutlookPanelButton HitTestButton(int x, int y, bool hoverableOnly)
		{
			if (_buttons != null)
			{
				for (int index = _buttons.Count - 1; index >= 0; index--)
				{
					OutlookPanelButton thisValue = _buttons[index];
					if ((!hoverableOnly || thisValue._canHover) && thisValue.GetBounds().Contains(x, y))
					{
						return thisValue;
					}
				}
			}
			return null;
		}

		internal override bool ProcessMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				SetHotButton(null);
				OutlookPanelButton uiPanelBaseLayoutState = HitTestButton(eventArgs.X, eventArgs.Y, false);
				if (uiPanelBaseLayoutState != null && uiPanelBaseLayoutState.GetKind() == ChevronKind)
				{
					uiPanelBaseLayoutState.SetSelected(true);
					ShowButtonMenu(uiPanelBaseLayoutState);
					uiPanelBaseLayoutState.SetSelected(false);
					return false;
				}
				if (uiPanelBaseLayoutState == null || uiPanelBaseLayoutState.GetKind() == MoreItemsKind)
				{
					SetPressedButton(null);
				}
				else
				{
					SetPressedButton(uiPanelBaseLayoutState);
				}
			}
			return true;
		}

		private void AddHiddenPanelCommands(MxContextMenu uiContextMenu)
		{
			if (_lastSmallButtonIndex >= GetVisiblePanels().Count - 1)
			{
				return;
			}
			uiContextMenu.Commands.AddSeparator();
			for (int index = GetVisiblePanels().Count - 1; index > _lastSmallButtonIndex; index--)
			{
				DockPanelBase uiPanelBase = (DockPanelBase)GetVisiblePanels()[index];
				MxCommand uiCommand = new MxCommand("Panel" + index, uiPanelBase.Text);
				uiCommand.TagString = "InvisiblePanel";
				CommandImageSource imageSource = uiPanelBase.GetImageSource();
				if (imageSource._icon != null)
				{
					uiCommand.Icon = imageSource._icon;
				}
				else if (imageSource._image != null)
				{
					uiCommand.Image = imageSource._image;
				}
				else
				{
					uiCommand.ImageIndex = imageSource._imageIndex;
					uiCommand.ImageKey = imageSource._imageKey;
				}
				uiCommand.Tag = uiPanelBase;
				uiContextMenu.Commands.Add(uiCommand);
			}
		}

		private void ShowButtonMenu(OutlookPanelButton uiPanelBaseLayoutState)
		{
			MxCommandManager uiCommandManager = new MxCommandManager();
			MxContextMenu uiContextMenu = new MxContextMenu();
			uiCommandManager.ContextMenus.Add(uiContextMenu);
			uiCommandManager.SmallImageSize = _panelGroup.PanelManager.GetSmallImageSize();
			string text;
			if (_panelGroup.AllowOutlookButtonsResize)
			{
				text = _panelGroup.PanelManager.BuiltInTexts[PanelsBuiltInText.ShowMoreButtons];
				if (text.Length > 0)
				{
					MxCommand uiCommand = new MxCommand("More", text);
					uiCommand.TagString = "More";
					uiCommand.Image = RenderingHelper.MakeMagentaTransparent(WindowsResources.OutPanelUpArrow());
					if (GetVisibleButtonCount() == -1 || GetVisibleButtonCount() == GetVisiblePanels().Count)
					{
						uiCommand.Enabled = InheritableBoolean.False;
					}
					uiContextMenu.Commands.Add(uiCommand);
				}
				text = _panelGroup.PanelManager.BuiltInTexts[PanelsBuiltInText.ShowFewerButtons];
				if (text.Length > 0)
				{
					MxCommand uiCommand = new MxCommand("Fewer", text);
					uiCommand.TagString = "Fewer";
					uiCommand.Image = RenderingHelper.MakeMagentaTransparent(WindowsResources.OutPanelDownArrow());
					if (GetVisibleButtonCount() == 0)
					{
						uiCommand.Enabled = InheritableBoolean.False;
					}
					uiContextMenu.Commands.Add(uiCommand);
				}
			}
			text = _panelGroup.PanelManager.BuiltInTexts[PanelsBuiltInText.AddOrRemoveButtons];
			if (text.Length > 0)
			{
				MxCommand uiCommand = new MxCommand("AddOrRemove", text);
				uiCommand.TagString = "AddOrRemove";
				for (int i = 0; i < _panelGroup.Panels.Count; i++)
				{
					MxCommand panelCommand = new MxCommand("Panel" + i, _panelGroup.Panels[i].Text);
					CommandImageSource imageSource = _panelGroup.Panels[i].GetImageSource();
					if (imageSource._icon != null)
					{
						panelCommand.Icon = imageSource._icon;
					}
					else if (imageSource._image != null)
					{
						panelCommand.Image = imageSource._image;
					}
					else
					{
						panelCommand.ImageIndex = imageSource._imageIndex;
						panelCommand.ImageKey = imageSource._imageKey;
						uiCommandManager.ImageList = imageSource._imageList;
					}
					if (!_panelGroup.Panels[i].Closed)
					{
						panelCommand.Checked = InheritableBoolean.True;
					}
					panelCommand.Tag = _panelGroup.Panels[i];
					uiCommand.Commands.Add(panelCommand);
				}
				uiContextMenu.Commands.Add(uiCommand);
			}
			AddHiddenPanelCommands(uiContextMenu);
			if (uiContextMenu.Commands.Count > 0)
			{
				uiContextMenu.CommandClick += OnMenuCommandClick;
				uiContextMenu.Show(_panelGroup, new Point(uiPanelBaseLayoutState.GetBounds().Right, uiPanelBaseLayoutState.GetBounds().Y + uiPanelBaseLayoutState.GetBounds().Height / 2));
			}
		}

		private void OnMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			string tagString;
			if ((tagString = eventArgs.Command.TagString) != null)
			{
				if (tagString == "More")
				{
					if (GetVisibleButtonCount() != -1)
					{
						SetVisibleButtonCount(GetVisibleButtonCount() + 1);
					}
					return;
				}
				if (tagString == "Fewer")
				{
					if (GetVisibleButtonCount() == -1)
					{
						SetVisibleButtonCount(GetVisiblePanels().Count - 1);
					}
					else if (GetVisibleButtonCount() > 0)
					{
						SetVisibleButtonCount(GetVisibleButtonCount() - 1);
					}
					return;
				}
				if (tagString == "InvisiblePanel")
				{
					if (eventArgs.Command.Tag is DockPanelBase)
					{
						SetSelectedPanel((DockPanelBase)eventArgs.Command.Tag);
					}
					return;
				}
			}
			if (eventArgs.Command.Tag is DockPanelBase)
			{
				((DockPanelBase)eventArgs.Command.Tag).Closed = !((DockPanelBase)eventArgs.Command.Tag).Closed;
			}
		}

		internal override bool ProcessDoubleClick()
		{
			Point point = _panelGroup.PointToClient(Cursor.Position);
			OutlookPanelButton uiPanelBaseLayoutState = HitTestButton(point.X, point.Y, false);
			if (uiPanelBaseLayoutState != null && uiPanelBaseLayoutState.GetPanel() != null && uiPanelBaseLayoutState.GetKind() == GrabKind && uiPanelBaseLayoutState.GetPanel().GetEffectiveCaptionDoubleClickAction() == CaptionDoubleClickAction.ToggleDockState)
			{
				if (uiPanelBaseLayoutState.GetPanel().IsFloating())
				{
					uiPanelBaseLayoutState.GetPanel().DockPanel();
				}
				else
				{
					uiPanelBaseLayoutState.GetPanel().UndockPanel();
				}
			}
			return false;
		}

		internal override bool ProcessMouseMove(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.None)
			{
				SetHotButton(HitTestInteractiveButton(eventArgs.X, eventArgs.Y));
			}
			else if (_panelGroup.Capture && _pressedButton != null)
			{
				if (_pressedButton == _resizeButton)
				{
					int dragY = eventArgs.Y;
					if (eventArgs.Y > _resizeButton.GetBounds().Top - _buttonHeight && eventArgs.Y < _resizeButton.GetBounds().Bottom + _buttonHeight)
					{
						return true;
					}
					if (dragY < GetSelectedPanel().GetMinimumSize().Height)
					{
						dragY = GetSelectedPanel().GetMinimumSize().Height;
					}
					int buttonsHeight = ContentBounds.Height - dragY;
					int visibleCount = buttonsHeight / _buttonHeight;
					if (eventArgs.Y < _resizeButton.GetBounds().Bottom)
					{
						visibleCount--;
					}
					if (visibleCount >= 0)
					{
						SetVisibleButtonCount(visibleCount);
						SetPressedButton(_resizeButton);
					}
				}
				else if (_pressedButton.GetKind() == GrabKind && !_pressedButton.GetBounds().Contains(eventArgs.X, eventArgs.Y))
				{
					_pressedButton.GetPanel().PanelManager.StartPanelDrag(_pressedButton.GetPanel(), PanelDragStartOffset, PanelDragStartOffset);
				}
				_pressedButton.SetPressed(_pressedButton.GetBounds().Contains(eventArgs.X, eventArgs.Y));
			}
			return true;
		}

		internal override bool ProcessMouseUp(MouseEventArgs eventArgs)
		{
			OutlookPanelButton uiPanelBaseLayoutState = _pressedButton;
			if (_pressedButton != null)
			{
				if (_pressedButton.GetBounds().Contains(eventArgs.X, eventArgs.Y) && _pressedButton.GetPanel() != null && _pressedButton.GetPanel().Enabled)
				{
					SetHotButton(HitTestInteractiveButton(eventArgs.X, eventArgs.Y));
					SetSelectedPanel(_pressedButton.GetPanel());
				}
				SetPressedButton(null);
			}
			if (uiPanelBaseLayoutState != null && uiPanelBaseLayoutState.IsSelected())
			{
				SetHotButton(HitTestInteractiveButton(eventArgs.X, eventArgs.Y));
			}
			return true;
		}

		internal override Cursor GetCursor(Point pt)
		{
			OutlookPanelButton uiPanelBaseLayoutState = HitTestInteractiveButton(pt.X, pt.Y);
			if (_buttons != null)
			{
				for (int index = _buttons.Count - 1; index >= 0; index--)
				{
					OutlookPanelButton thisValue = _buttons[index];
					if (thisValue.GetBounds().Contains(pt))
					{
						uiPanelBaseLayoutState = thisValue;
						break;
					}
				}
			}
			if (uiPanelBaseLayoutState != null)
			{
				if (uiPanelBaseLayoutState == _resizeButton)
				{
					return Cursors.SizeNS;
				}
				if (uiPanelBaseLayoutState.GetKind() == GrabKind)
				{
					return Cursors.SizeAll;
				}
				if (uiPanelBaseLayoutState.GetKind() != MoreItemsKind)
				{
					return Cursors.Hand;
				}
			}
			return null;
		}

		internal override int GetAccessibleChildCount()
		{
			int count = 0;
			if (GetSelectedPanel() != null)
			{
				count++;
			}
			if (_buttons != null)
			{
				foreach (OutlookPanelButton item in _buttons)
				{
					if (item.GetAccessibleRole() != AccessibleRole.None)
					{
						count++;
					}
				}
			}
			return count;
		}

		internal override AccessibleObject GetAccessibleChild(int index)
		{
			List<AccessibleObject> list = new List<AccessibleObject>();
			if (GetSelectedPanel() != null)
			{
				list.Add(GetSelectedPanel().AccessibilityObject);
			}
			if (_buttons != null)
			{
				foreach (OutlookPanelButton item in _buttons)
				{
					if (item.GetAccessibleRole() != AccessibleRole.None)
					{
						list.Add(item.GetAccessibleObject());
					}
				}
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}
	}
}
