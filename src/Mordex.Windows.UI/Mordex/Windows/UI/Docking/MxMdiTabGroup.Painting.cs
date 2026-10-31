using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxMdiTabGroup
	{
		private Rectangle InsetTabTop(Rectangle bounds)
		{
			int inset = TabTopInsetDefault;
			bounds.Y += inset;
			bounds.Height -= inset;
			return bounds;
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			DockManagerRendererBase renderer = PanelManager.GetMdiRenderer();
			FormatStyle formatStyle = PanelManager.GetMdiTabStripStyle();
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Height = _stripHeight;
			renderer.DrawTabStrip(pe.Graphics, this, clientRectangle, formatStyle, PanelManager.TabbedMdiSettings.BorderColor, PanelDockStyle.Top);
			Rectangle rectangle = _groupLayout.GetTabsBounds(base.ClientRectangle);
			Region region = new Region(rectangle);
			Region clip = pe.Graphics.Clip;
			pe.Graphics.Clip = region;
			int selectedTabIndex = -1;
			for (int tabIndex = _groupLayout.GetCount() - 1; tabIndex >= 0; tabIndex--)
			{
				MdiTabItem uiMdiChildTabLayoutState = _groupLayout.GetTab(tabIndex);
				Rectangle bounds = _groupLayout.GetTabBounds(tabIndex, false);
				if (!bounds.IsEmpty && uiMdiChildTabLayoutState.GetIsVisible())
				{
					if (uiMdiChildTabLayoutState.GetChildTab().Form == _form)
					{
						selectedTabIndex = tabIndex;
						continue;
					}
					bounds = InsetTabTop(bounds);
					FormatStyle tabStyle = ResolveTabStyle(uiMdiChildTabLayoutState.GetChildTab(), uiMdiChildTabLayoutState.GetButtonState(), false);
					renderer.DrawMdiTabBackground(pe.Graphics, this, tabStyle, uiMdiChildTabLayoutState.GetButtonState(), bounds, isSelectedTab: false);
					bounds = _groupLayout.GetTabBounds(tabIndex, true);
					if (!bounds.IsEmpty)
					{
						bounds.Y++;
						bounds.Height -= 2;
						renderer.DrawMdiTabForeground(pe.Graphics, this, uiMdiChildTabLayoutState.GetChildTab(), tabStyle, uiMdiChildTabLayoutState.GetButtonState(), bounds, rectangle);
					}
				}
				if (uiMdiChildTabLayoutState.GetButtonState() == ButtonState.Hot && uiMdiChildTabLayoutState._closeButton != null)
				{
					int hotCloseImageIndex = (CloseImageIndex);
					renderer.DrawPanelButton(pe.Graphics, PanelCaptionStyle.Standard, null, PanelButtonType.CloseButton, uiMdiChildTabLayoutState._closeButton.GetBounds(), uiMdiChildTabLayoutState._closeButton.GetButtonState(), null, ResolveTabStyle(null, uiMdiChildTabLayoutState._closeButton.GetButtonState(), true), MxDockManager.GetButtonImage(hotCloseImageIndex, PanelManager.DefaultPanelSettings));
				}
			}
			if (selectedTabIndex != -1)
			{
				MdiTabItem selectedTab = _groupLayout.GetTab(selectedTabIndex);
				Rectangle selectedBounds = _groupLayout.GetTabBounds(selectedTabIndex, false);
				selectedBounds = InsetTabTop(selectedBounds);
				ButtonState buttonState = (selectedTab.GetStoredButtonState());
				FormatStyle selectedStyle = ResolveTabStyle(selectedTab.GetChildTab(), buttonState, false);
				renderer.DrawMdiTabBackground(pe.Graphics, this, selectedStyle, buttonState, selectedBounds, isSelectedTab: true);
				selectedBounds = _groupLayout.GetTabBounds(selectedTabIndex, true);
				if (!selectedBounds.IsEmpty)
				{
					selectedBounds.Y++;
					selectedBounds.Height -= 2;
					selectedStyle = ResolveTabStyle(selectedTab.GetChildTab(), selectedTab.GetButtonState(), false);
					renderer.DrawMdiTabForeground(pe.Graphics, this, selectedTab.GetChildTab(), selectedStyle, selectedTab.GetButtonState(), selectedBounds, rectangle);
				}
			}
			pe.Graphics.Clip = clip;
			clip.Dispose();
			region.Dispose();
			if (_groupLayout.GetCloseButton() != null)
			{
				ButtonState closeButtonState = _groupLayout.GetCloseButton().GetButtonState();
				int closeImageIndex = CloseImageIndex;
				renderer.DrawPanelButton(pe.Graphics, PanelCaptionStyle.Standard, null, PanelButtonType.CloseButton, _groupLayout.GetCloseButton().GetBounds(), closeButtonState, null, ResolveTabStyle(null, _groupLayout.GetCloseButton().GetButtonState(), true), MxDockManager.GetButtonImage(closeImageIndex, PanelManager.DefaultPanelSettings));
			}
			if (_groupLayout.GetActiveFilesButton() != null)
			{
				ButtonState activeFilesButtonState = _groupLayout.GetActiveFilesButton().GetButtonState();
				if (_groupLayout.GetIsMenuOpen())
				{
					activeFilesButtonState = ButtonState.Hot;
				}
				int activeFilesImageIndex = _groupLayout._hasOverflow ? ActiveFilesAlternateImageIndex : ActiveFilesImageIndex;
				renderer.DrawPanelButton(pe.Graphics, PanelCaptionStyle.Standard, null, PanelButtonType.ActiveFiles, _groupLayout.GetActiveFilesButton().GetBounds(), activeFilesButtonState, null, ResolveTabStyle(null, activeFilesButtonState, true), MxDockManager.GetButtonImage(activeFilesImageIndex, PanelManager.DefaultPanelSettings));
			}
			if (_groupLayout.GetScrollLeftButton() != null)
			{
				renderer.DrawPanelButton(pe.Graphics, PanelCaptionStyle.Standard, null, PanelButtonType.ScrollLeft, _groupLayout.GetScrollLeftButton().GetBounds(), _groupLayout.GetScrollLeftButton().GetButtonState(), null, ResolveTabStyle(null, _groupLayout.GetScrollLeftButton().GetButtonState(), true), null);
			}
			if (_groupLayout.GetScrollLeftButton() != null)
			{
				renderer.DrawPanelButton(pe.Graphics, PanelCaptionStyle.Standard, null, PanelButtonType.ScrollRight, _groupLayout.GetScrollRightButton().GetBounds(), _groupLayout.GetScrollRightButton().GetButtonState(), null, ResolveTabStyle(null, _groupLayout.GetScrollRightButton().GetButtonState(), true), null);
			}
			rectangle.Width++;
			rectangle.Inflate(0, -1);
			renderer.DrawSeparator(pe.Graphics, PanelManager.TabbedMdiSettings.BorderColor, rectangle);
			base.OnPaint(pe);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new MdiTabGroupAccessibleObject(this);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			Invalidate();
		}

		internal void SetToolTipText(string caption, Rectangle rectangle)
		{
			if (caption.Length <= 0)
			{
				DisposeToolTipResources();
			}
			else if (VisualStyleSupport.IsToolTipHookSupported())
			{
				if (_toolTipPopup == null)
				{
					bool useCompatibleTextRendering = false;
					if (PanelManager != null)
					{
						useCompatibleTextRendering = PanelManager.UseCompatibleTextRendering;
					}
					_toolTipPopup = new WindowMessageHandler(Font, useCompatibleTextRendering);
					if (!rectangle.IsEmpty)
					{
						_toolTipPopup._anchorPoint = PointToScreen(new Point(rectangle.X + ToolTipOffsetX, rectangle.Bottom - ToolTipOffsetFromBottom));
					}
					_toolTipPopup.RequestShow(caption, this);
				}
				else
				{
					if (!rectangle.IsEmpty)
					{
						_toolTipPopup._anchorPoint = PointToScreen(new Point(rectangle.X + ToolTipOffsetX, rectangle.Bottom - ToolTipOffsetFromBottom));
					}
					_toolTipPopup.UpdateTipText(caption);
				}
			}
			else
			{
				if (_toolTip == null)
				{
					_toolTip = new Mordex.Windows.Common.ThemedToolTip();
					_toolTip.ShowAlways = true;
				}
				_toolTip.SetToolTip(this, caption);
			}
		}
	}
}
