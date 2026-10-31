using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public abstract partial class DockPanelBase
	{
		PanelDropInfo IDropInfoProvider.GetDropInfo(DockPanelBase panel, Point screenPoint)
		{
			return GetDropInfoAt(panel, screenPoint);
		}

		internal PanelDropInfo GetDropInfoAt(DockPanelBase panel, Point screenPoint)
		{
			if (panel == this)
			{
				return null;
			}
			if (panel.Contains(this))
			{
				return null;
			}
			if (panel != this && CanDropPanel() && PanelManager != null)
			{
				Point point = PointToClient(screenPoint);
				if (_captionRenderer != null && PanelManager.DefaultTabGroupStyle != TabGroupStyle.None && _captionRenderer.GetCaptionBounds().Contains(point))
				{
					if (PanelManager.DockingStyleMode == DockingStyleMode.VS2005)
					{
						PanelManager.SetDragHoverPanel(this);
					}
					return GetTabDropInfo();
				}
				if (PanelManager.DockingStyleMode == DockingStyleMode.VS2005)
				{
					PanelDropInfo result = new PanelDropInfo();
					if (DisplayRectangle.Contains(point))
					{
						PanelManager.SetDragHoverPanel(this);
						return result;
					}
				}
				else
				{
					PanelDropInfo result = GetEdgeDropInfo(point);
					if (result != null)
					{
						return result;
					}
					result = new PanelDropInfo();
					if (_splitterBounds.Contains(point))
					{
						result._createsGroup = false;
						result._panelDockStyle = DockStyle;
						result._panelDockState = DockState;
						Size size = PanelManager.GetDockedSize(panel, DockStyle, Size);
						Point p = Point.Empty;
						switch (DockStyle)
						{
						case PanelDockStyle.Left:
							p = new Point(base.Right, base.Top);
							break;
						case PanelDockStyle.Top:
							p = new Point(base.Left, base.Bottom);
							break;
						case PanelDockStyle.Right:
							p = new Point(base.Left - size.Width, base.Top);
							break;
						case PanelDockStyle.Bottom:
							p = new Point(base.Left, base.Top - size.Height);
							break;
						}
						p = RenderingHelper.GetParentControl(this)?.PointToScreen(p) ?? PointToScreen(p);
						result._dropBounds = new Rectangle(p, size);
						int draggedIndex = -1;
						if (panel.IsRootPanel() && panel.DockState == PanelDockState.Docked)
						{
							draggedIndex = panel._parentGroup.Panels.IndexOf(panel);
						}
						result._insertIndex = _parentGroup.Panels.IndexOf(this) + 1;
						if (draggedIndex != -1 && draggedIndex < result._insertIndex)
						{
							result._insertIndex--;
						}
						return result;
					}
				}
			}
			return null;
		}

		internal PanelDropInfo GetTabDropInfo()
		{
			PanelDropInfo panelDropInfo = new PanelDropInfo();
			panelDropInfo._createsGroup = true;
			panelDropInfo._dropBounds = DisplayRectangle;
			panelDropInfo._groupStyle = (PanelGroupStyle)PanelManager.DefaultTabGroupStyle;
			if (panelDropInfo._groupStyle == PanelGroupStyle.Tab && (ParentGroup == null || ParentGroup.GroupStyle != PanelGroupStyle.Tab))
			{
				panelDropInfo._dropBounds.Height -= NewTabHeight;
			}
			panelDropInfo._dropBounds.Location = PointToScreen(panelDropInfo._dropBounds.Location);
			panelDropInfo._insertIndex = 1;
			panelDropInfo._targetPanel = this;
			if (panelDropInfo._groupStyle == PanelGroupStyle.Tab)
			{
				if (ParentGroup == null || ParentGroup.GroupStyle != PanelGroupStyle.Tab)
				{
					panelDropInfo._tabHeaderBounds = new Rectangle(panelDropInfo._dropBounds.X + NewTabIndent, panelDropInfo._dropBounds.Bottom, NewTabWidth, NewTabHeight);
				}
				else
				{
					panelDropInfo._tabHeaderBounds = ParentGroup.GetNewTabHeaderBounds();
					panelDropInfo._tabHeaderBounds.Y = panelDropInfo._dropBounds.Bottom;
				}
			}
			else
			{
				panelDropInfo._tabHeaderBounds = new Rectangle(panelDropInfo._dropBounds.X, panelDropInfo._dropBounds.Top + GetPanelCaptionHeight(), panelDropInfo._dropBounds.Width, 0);
			}
			panelDropInfo._panelDockState = DockState;
			return panelDropInfo;
		}

		internal void FillTabDropInfo(PanelDropInfo panelDropInfo)
		{
			panelDropInfo._dropBounds = DisplayRectangle;
			if (panelDropInfo._groupStyle == PanelGroupStyle.Tab)
			{
				panelDropInfo._dropBounds.Height -= NewTabHeight;
			}
			panelDropInfo._dropBounds.Location = PointToScreen(panelDropInfo._dropBounds.Location);
			if (panelDropInfo._groupStyle == PanelGroupStyle.Tab)
			{
				panelDropInfo._tabHeaderBounds = new Rectangle(panelDropInfo._dropBounds.X + NewTabIndent, panelDropInfo._dropBounds.Bottom, NewTabWidth, NewTabHeight);
			}
			else
			{
				panelDropInfo._tabHeaderBounds = new Rectangle(panelDropInfo._dropBounds.X, panelDropInfo._dropBounds.Top + GetPanelCaptionHeight(), panelDropInfo._dropBounds.Width, 0);
			}
		}

		internal PanelDropInfo GetTileDropInfo(PanelDockStyle edge)
		{
			PanelDropInfo panelDropInfo = new PanelDropInfo();
			Rectangle displayRectangle = DisplayRectangle;
			if (HasBorderPanel)
			{
				displayRectangle.Inflate(1, 1);
			}
			if (ParentGroup != null && ParentGroup.IsTabGroup())
			{
				displayRectangle.Height = ParentGroup.DisplayRectangle.Height;
			}
			switch (edge)
			{
			case PanelDockStyle.Left:
				panelDropInfo._groupStyle = PanelGroupStyle.VerticalTiles;
				panelDropInfo._insertIndex = 0;
				displayRectangle.Width = (displayRectangle.Width - GetSplitterSize()) / 2;
				break;
			case PanelDockStyle.Top:
				panelDropInfo._groupStyle = PanelGroupStyle.HorizontalTiles;
				panelDropInfo._insertIndex = 0;
				displayRectangle.Height = (displayRectangle.Height - GetSplitterSize()) / 2;
				break;
			case PanelDockStyle.Right:
				panelDropInfo._groupStyle = PanelGroupStyle.VerticalTiles;
				panelDropInfo._insertIndex = 1;
				displayRectangle.Width = (displayRectangle.Width - GetSplitterSize()) / 2;
				displayRectangle.X = displayRectangle.Right + GetSplitterSize();
				break;
			case PanelDockStyle.Bottom:
				panelDropInfo._groupStyle = PanelGroupStyle.HorizontalTiles;
				panelDropInfo._insertIndex = 1;
				displayRectangle.Height = (displayRectangle.Height - GetSplitterSize()) / 2;
				displayRectangle.Y = displayRectangle.Bottom + GetSplitterSize();
				break;
			}
			panelDropInfo._createsGroup = true;
			panelDropInfo._targetPanel = this;
			panelDropInfo._panelDockStyle = DockStyle;
			panelDropInfo._panelDockState = _dockState;
			displayRectangle.Location = PointToScreen(displayRectangle.Location);
			panelDropInfo._dropBounds = displayRectangle;
			return panelDropInfo;
		}

		private PanelDropInfo GetEdgeDropInfo(Point pt)
		{
			Rectangle rectangle = _innerAreaBounds;
			if (PanelManager.DefaultTabGroupStyle == TabGroupStyle.None)
			{
				rectangle = _displayBounds;
			}
			if (!rectangle.Contains(pt))
			{
				return null;
			}
			if (pt.X < _innerAreaBounds.X + DropEdgeMargin)
			{
				return GetTileDropInfo(PanelDockStyle.Left);
			}
			if (pt.Y < _innerAreaBounds.Y + DropEdgeMargin)
			{
				return GetTileDropInfo(PanelDockStyle.Top);
			}
			if (pt.X > _innerAreaBounds.Right - DropEdgeMargin)
			{
				return GetTileDropInfo(PanelDockStyle.Right);
			}
			if (pt.Y > _innerAreaBounds.Bottom - DropEdgeMargin)
			{
				return GetTileDropInfo(PanelDockStyle.Bottom);
			}
			return null;
		}
	}
}
