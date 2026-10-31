using System.Drawing;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class OfficeTaskPaneGroupState : DockPanelGroupState
	{
		private bool _isCaptionButtonPressed;

		internal override Size MinimumSize
		{
			get
			{
				int minWidth = -1;
				int minHeight = -1;
				foreach (DockPanelBase item in GetVisiblePanels())
				{
					Size minimumSize = item.GetMinimumSize();
					if (minHeight < minimumSize.Height)
					{
						minHeight = minimumSize.Height;
					}
					if (minWidth < minimumSize.Width)
					{
						minWidth = minimumSize.Width;
					}
				}
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

		internal OfficeTaskPaneGroupState(MxDockPanelGroup panelGroup)
			: base(panelGroup)
		{
		}

		internal override void RefreshGroupLayout()
		{
			if (GetVisiblePanels().Count > 0 && (GetSelectedPanel() == null || GetSelectedPanel().Closed))
			{
				SetSelectedPanel((DockPanelBase)GetVisiblePanels()[0]);
			}
			if (GetSelectedPanel() == null)
			{
				return;
			}
			GetSelectedPanel().BringToFront();
			Rectangle displayRectangle = ContentBounds;
			foreach (DockPanelBase item in GetVisiblePanels())
			{
				item.Bounds = displayRectangle;
			}
		}

		internal override void OnSelectedPanelChanged(bool activate)
		{
			if (GetSelectedPanel() != null)
			{
				GetSelectedPanel().RecalculateLayout();
				GetSelectedPanel().BringToFront();
			}
			_panelGroup.Invalidate();
			base.OnSelectedPanelChanged(activate);
		}

		internal override bool ShouldDragGroup()
		{
			return true;
		}

		internal override bool ProcessCloseButtonClick(DockPanelBase panel)
		{
			_panelGroup.Closed = true;
			return true;
		}

		internal override bool ProcessCaptionButtonDown(DockPanelBase panel, Rectangle buttonBounds)
		{
			_isCaptionButtonPressed = true;
			panel.Invalidate();
			panel.Update();
			ShowPanelMenu(buttonBounds);
			_isCaptionButtonPressed = false;
			panel.Invalidate();
			panel.Update();
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

		internal override bool ProcessCaptionDoubleClick(DockPanelBase panel)
		{
			if (_panelGroup.IsFloating())
			{
				_panelGroup.DockPanel();
			}
			else
			{
				_panelGroup.UndockPanel();
			}
			return true;
		}

		private void ShowPanelMenu(Rectangle rectangle)
		{
			MxCommandManager uiCommandManager = new MxCommandManager();
			MxContextMenu uiContextMenu = new MxContextMenu();
			uiCommandManager.ContextMenus.Add(uiContextMenu);
			uiCommandManager.SmallImageSize = _panelGroup.PanelManager.GetSmallImageSize();
			uiContextMenu.CommandClick += OnPanelCommandClick;
			int panelIndex = 0;
			foreach (DockPanelBase item in GetVisiblePanels())
			{
				MxCommand uiCommand = new MxCommand("Panel" + panelIndex);
				panelIndex++;
				uiCommand.Text = item.Text;
				uiCommand.Tag = item;
				CommandImageSource imageSource = item.GetImageSource();
				if (imageSource._icon != null)
				{
					uiCommand.Icon = item.Icon;
				}
				else if (imageSource._image != null)
				{
					uiCommand.Image = item.Image;
				}
				else
				{
					uiCommandManager.ImageList = imageSource._imageList;
					uiCommand.ImageIndex = imageSource._imageIndex;
					uiCommand.ImageKey = imageSource._imageKey;
				}
				if (item == GetSelectedPanel())
				{
					uiCommand.Checked = InheritableBoolean.True;
				}
				uiCommand.IsEnabled = item.Enabled;
				uiContextMenu.Commands.Add(uiCommand);
			}
			uiContextMenu._isRightAligned = true;
			uiContextMenu.Show(GetSelectedPanel(), new Point(rectangle.Right, rectangle.Bottom + 1));
		}

		internal override bool PaintChildCaptionBar(Graphics graphics, DockCaptionRenderer captionBar)
		{
			if (_isCaptionButtonPressed)
			{
				captionBar.DrawCaption(graphics, _isCaptionButtonPressed, 0, 0);
				return true;
			}
			return false;
		}

		private void OnPanelCommandClick(object sender, CommandEventArgs eventArgs)
		{
			DockPanelBase uiPanelBase = (DockPanelBase)eventArgs.Command.Tag;
			SetSelectedPanel(uiPanelBase);
			((MxContextMenu)sender).Dispose();
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

		internal override bool TreatChildPanelTextAsButton()
		{
			return true;
		}
	}
}
