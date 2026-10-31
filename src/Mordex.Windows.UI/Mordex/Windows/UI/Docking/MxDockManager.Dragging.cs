using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxDockManager
	{
		internal void StartPanelDrag(DockPanelBase panel, int offsetX, int offsetY)
		{
			if (!RaisePanelStartDrag(panel) || Control.MouseButtons == MouseButtons.None)
			{
				return;
			}
			foreach (Control control in _containerControl.Controls)
			{
				if (control is DockPanelBasePanel)
				{
					((DockPanelBasePanel)control).CloseNow();
					break;
				}
			}
			if (offsetX == -1 && offsetY == -1)
			{
				offsetX = panel.DisplayRectangle.X + panel.DisplayRectangle.Width / 2;
				offsetY = panel.DisplayRectangle.Y + DragStartOffsetY;
			}
			Point offset = new Point(offsetX, offsetY);
			if (DockingStyleMode == DockingStyleMode.Standard)
			{
				_dragFilter = new DockManagerMessageFilter(this, panel, offset);
			}
			else
			{
				_dragFilter = new DockPanelBaseMessageFilter(this, panel, offset);
			}
			_dragFilter.StartDrag();
		}

		internal DockManagerMessageFilter GetDragFilter()
		{
			return _dragFilter;
		}

		private bool RaisePanelStartDrag(DockPanelBase panel)
		{
			if (panel.AutoHide)
			{
				return false;
			}
			PanelActionCancelEventArgs e = new PanelActionCancelEventArgs(panel, cancel: false);
			OnPanelStartDrag(e);
			return !e.Cancel;
		}

		internal void PlaceInAutoHideStrip(DockPanelBase panel, bool isAutoHide)
		{
			if (isAutoHide)
			{
				switch (panel.DockStyle)
				{
				case PanelDockStyle.Left:
					_topStrip.RemovePanel(panel);
					_rightStrip.RemovePanel(panel);
					_bottomStrip.RemovePanel(panel);
					_leftStrip.AddPanel(panel);
					break;
				case PanelDockStyle.Top:
					_leftStrip.RemovePanel(panel);
					_rightStrip.RemovePanel(panel);
					_bottomStrip.RemovePanel(panel);
					_topStrip.AddPanel(panel);
					break;
				case PanelDockStyle.Right:
					_leftStrip.RemovePanel(panel);
					_topStrip.RemovePanel(panel);
					_bottomStrip.RemovePanel(panel);
					_rightStrip.AddPanel(panel);
					break;
				case PanelDockStyle.Bottom:
					_leftStrip.RemovePanel(panel);
					_topStrip.RemovePanel(panel);
					_rightStrip.RemovePanel(panel);
					_bottomStrip.AddPanel(panel);
					break;
				}
			}
			else
			{
				_leftStrip.RemovePanel(panel);
				_topStrip.RemovePanel(panel);
				_rightStrip.RemovePanel(panel);
				_bottomStrip.RemovePanel(panel);
			}
		}

		internal PanelDropInfo QueryStandardDropInfo(DockPanelBase uiPanelBase, Point point)
		{
			PanelDropInfo panelDropInfo = GetStandardDropInfo(uiPanelBase, point);
			PanelDragDropEventArgs e = CreateDragDropEventArgs(uiPanelBase, panelDropInfo);
			Size size = panelDropInfo._dropBounds.Size;
			OnDraggingPanel(e);
			panelDropInfo._isAllowed = e.CanDrop;
			panelDropInfo = GetStandardDropInfo(uiPanelBase, point);
			panelDropInfo._isAllowed = e.CanDrop;
			if (panelDropInfo._isAllowed)
			{
				if (size != e.Size && !panelDropInfo._createsGroup)
				{
					if (panelDropInfo._panelDockState == PanelDockState.Floating)
					{
						panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, e.Size);
					}
					else
					{
						switch (panelDropInfo._panelDockStyle)
						{
						case PanelDockStyle.Left:
							panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, new Size(e.Size.Width, size.Height));
							break;
						case PanelDockStyle.Right:
						{
							int x = panelDropInfo._dropBounds.Left + (size.Width - e.Size.Width);
							panelDropInfo._dropBounds = new Rectangle(x, panelDropInfo._dropBounds.Location.Y, e.Size.Width, size.Height);
							break;
						}
						case PanelDockStyle.Top:
							panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, new Size(size.Width, e.Size.Height));
							break;
						case PanelDockStyle.Bottom:
						{
							int y = panelDropInfo._dropBounds.Top + (size.Height - e.Size.Height);
							panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location.X, y, size.Width, e.Size.Height);
							break;
						}
						default:
							panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, e.Size);
							break;
						}
					}
				}
				Cursor.Current = Cursors.Default;
				if (e.DropAction == PanelDropAction.CreateGroup)
				{
					panelDropInfo._groupStyle = e.GroupStyle;
					switch (e.GroupStyle)
					{
					case PanelGroupStyle.Tab:
					case PanelGroupStyle.OfficeTaskPane:
					case PanelGroupStyle.OutlookNavigator:
						e.ParentPanel.FillTabDropInfo(panelDropInfo);
						break;
					}
				}
			}
			else
			{
				Cursor.Current = Cursors.No;
			}
			return panelDropInfo;
		}

		private PanelDragDropEventArgs CreateDragDropEventArgs(DockPanelBase panel, PanelDropInfo panelDropInfo)
		{
			return new PanelDragDropEventArgs(dropAction: (!panelDropInfo._createsGroup) ? ((panelDropInfo._targetPanel is MxDockPanelGroup) ? PanelDropAction.InsertInGroup : ((panelDropInfo._panelDockState != PanelDockState.Floating) ? PanelDropAction.DockedReposition : PanelDropAction.FloatingReposition)) : PanelDropAction.CreateGroup, panel: panel, dockState: panelDropInfo._panelDockState, dockStyle: panelDropInfo._panelDockStyle, parent: panelDropInfo._targetPanel, groupStyle: panelDropInfo._groupStyle, size: panelDropInfo._dropBounds.Size);
		}

		internal DockPanelBase GetDragHoverPanel()
		{
			if (_dragFilter != null && _dragFilter is DockPanelBaseMessageFilter)
			{
				return ((DockPanelBaseMessageFilter)_dragFilter).GetTargetPanel();
			}
			return null;
		}

		internal void SetDragHoverPanel(DockPanelBase panel)
		{
			if (_dragFilter != null && _dragFilter is DockPanelBaseMessageFilter)
			{
				((DockPanelBaseMessageFilter)_dragFilter).SetTargetPanel(panel);
			}
		}

		internal PanelDropInfo GetGuideDropInfo(DockPanelBase panel, Point pt)
		{
			if ((Control.ModifierKeys & Keys.Control) != Keys.None)
			{
				return GetFloatingDropInfo(panel, pt);
			}
			foreach (DockPanelBase floatingPanel in FloatingPanels)
			{
				if (floatingPanel == panel || floatingPanel.Closed)
				{
					continue;
				}
				Rectangle floatingBounds = floatingPanel.ClientRectangle;
				floatingBounds.Location = floatingPanel.PointToScreen(floatingBounds.Location);
				if (floatingBounds.Contains(pt))
				{
					PanelDropInfo floatingDropInfo = ((IDropInfoProvider)floatingPanel).GetDropInfo(panel, pt);
					if (floatingDropInfo != null)
					{
						return floatingDropInfo;
					}
				}
			}
			foreach (DockPanelBase dockedPanel in DockedPanels)
			{
				if (dockedPanel == panel || dockedPanel.Closed)
				{
					continue;
				}
				Rectangle dockedBounds = dockedPanel.ClientRectangle;
				dockedBounds.Location = dockedPanel.PointToScreen(dockedBounds.Location);
				if (dockedBounds.Contains(pt))
				{
					PanelDropInfo dockedDropInfo = ((IDropInfoProvider)dockedPanel).GetDropInfo(panel, pt);
					if (dockedDropInfo != null)
					{
						return dockedDropInfo;
					}
				}
			}
			if (_dockingStyleMode == DockingStyleMode.VS2005)
			{
				SetDragHoverPanel(null);
			}
			return GetFloatingDropInfo(panel, pt);
		}

		internal PanelDropInfo GetStandardDropInfo(DockPanelBase panel, Point pt)
		{
			if ((Control.ModifierKeys & Keys.Control) != Keys.None)
			{
				return GetFloatingDropInfo(panel, pt);
			}
			bool isOverFloatingPanel = false;
			foreach (DockPanelBase floatingPanel in FloatingPanels)
			{
				if (floatingPanel == panel || floatingPanel.Closed)
				{
					continue;
				}
				Rectangle floatingBounds = floatingPanel.ClientRectangle;
				floatingBounds.Location = floatingPanel.PointToScreen(floatingBounds.Location);
				if (floatingBounds.Contains(pt))
				{
					PanelDropInfo floatingDropInfo = ((IDropInfoProvider)floatingPanel).GetDropInfo(panel, pt);
					if (floatingDropInfo != null)
					{
						return floatingDropInfo;
					}
					isOverFloatingPanel = true;
				}
			}
			if (isOverFloatingPanel)
			{
				return GetFloatingDropInfo(panel, pt);
			}
			Rectangle dockArea = _fillArea;
			dockArea.Location = _containerControl.PointToScreen(dockArea.Location);
			PanelDockStyle panelDockStyle = HitTestEdge(dockArea, pt, DockEdgeHitWidth);
			if (panelDockStyle != PanelDockStyle.Fill)
			{
				return GetFillAreaEdgeDropInfo(panel, panelDockStyle);
			}
			foreach (DockPanelBase dockedPanel in DockedPanels)
			{
				if (dockedPanel == panel || dockedPanel.Closed)
				{
					continue;
				}
				Rectangle dockedBounds = dockedPanel.ClientRectangle;
				dockedBounds.Location = dockedPanel.PointToScreen(dockedBounds.Location);
				if (dockedBounds.Contains(pt))
				{
					PanelDropInfo dockedDropInfo = ((IDropInfoProvider)dockedPanel).GetDropInfo(panel, pt);
					if (dockedDropInfo != null)
					{
						return dockedDropInfo;
					}
				}
			}
			if (_dockingStyleMode == DockingStyleMode.VS2005)
			{
				SetDragHoverPanel(null);
				return GetFloatingDropInfo(panel, pt);
			}
			Rectangle containerArea = _dockArea;
			containerArea.Location = _containerControl.PointToScreen(containerArea.Location);
			containerArea.Inflate(DockEdgeHitWidth, DockEdgeHitWidth);
			panelDockStyle = HitTestEdge(containerArea, pt, DockEdgeHitWidth);
			if (panelDockStyle != PanelDockStyle.Fill)
			{
				return GetContainerEdgeDropInfo(panel, panelDockStyle);
			}
			return GetFloatingDropInfo(panel, pt);
		}

		internal void ClearDragFilter()
		{
			_dragFilter = null;
		}

		internal void HideAutoHidePanels()
		{
			_bottomStrip.ClosePopup();
			_leftStrip.ClosePopup();
			_topStrip.ClosePopup();
			_rightStrip.ClosePopup();
		}

		internal void EndPanelDrag(DockPanelBase panel, PanelDropInfo panelDropInfo)
		{
			DockPanelBase groupCreated = null;
			if (panelDropInfo._isAllowed)
			{
				PanelDragDropEventArgs e = CreateDragDropEventArgs(panel, panelDropInfo);
				OnPanelDrop(e);
				if (e.CanDrop)
				{
					panelDropInfo._groupStyle = e.GroupStyle;
					panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, e.Size);
					DropPanel(panel, panelDropInfo);
					panel.Activate();
				}
			}
			_dragFilter = null;
			if (_designer != null)
			{
				DesignHelper.PanelManagerDesigner_EndPanelDragTransaction(_designer, panel, groupCreated);
			}
			RefreshFormIfNoComposition();
			Cursor.Current = Cursors.Default;
		}

		private void AttachLayoutState(DockPanelBase uiPanelBase, PanelDockState dockState)
		{
			DockLayoutState dockLayoutState = ((dockState != PanelDockState.Docked) ? uiPanelBase._floatingLayoutState : uiPanelBase._dockedLayoutState);
			dockLayoutState.Reparent(((IPanelContainer)this).GetDockLayoutState(dockState));
		}

		internal void DropPanel(DockPanelBase panel, PanelDropInfo panelDropInfo)
		{
			SuspendContainerLayout();
			if (panelDropInfo._createsGroup)
			{
				panelDropInfo._targetPanel.WrapInNewGroup(panelDropInfo._groupStyle, panel, panelDropInfo._insertIndex);
			}
			else if (panelDropInfo._targetPanel == null)
			{
				AttachLayoutState(panel, panelDropInfo._panelDockState);
				panel.AttachToGroupOrDetach(null, panelDropInfo._panelDockState, panelDropInfo._panelDockStyle);
				if (panelDropInfo._panelDockState == PanelDockState.Floating)
				{
					panel.FloatingLocation = panelDropInfo._dropBounds.Location;
				}
				else
				{
					int panelIndex = panelDropInfo._insertIndex;
					if (panelIndex == -1)
					{
						panelIndex = _panels.Count - 1;
					}
					_panels.SetPanelIndex(panel, panelIndex);
				}
				panel.Size = panelDropInfo._dropBounds.Size;
			}
			else
			{
				MxDockPanelGroup uiPanelGroup = (MxDockPanelGroup)panelDropInfo._targetPanel;
				uiPanelGroup.Panels.Insert(panelDropInfo._insertIndex, panel);
			}
			RemoveRedundantGroups();
			ResumeContainerLayout(true);
		}

		internal PanelDropInfo GetFloatingDropInfo(DockPanelBase uiPanelBase, Point point)
		{
			Rectangle floatingBounds = new Rectangle(0, 0, -1, -1);
			floatingBounds.Size = uiPanelBase.FloatingSize;
			if (floatingBounds.Size == new Size(-1, -1))
			{
				floatingBounds.Size = new Size(DefaultFloatingSize, DefaultFloatingSize);
			}
			int grabOffsetX = _dragFilter.GetOffset().X * floatingBounds.Width / uiPanelBase.Width;
			point.Offset(-grabOffsetX, -_dragFilter.GetOffset().Y);
			floatingBounds.Location = point;
			PanelDropInfo panelDropInfo = new PanelDropInfo();
			panelDropInfo._createsGroup = false;
			panelDropInfo._panelDockState = PanelDockState.Floating;
			panelDropInfo._dropBounds = floatingBounds;
			panelDropInfo._insertIndex = -1;
			panelDropInfo._targetPanel = null;
			return panelDropInfo;
		}

		internal Size GetDockedSize(DockPanelBase uiPanelBase, PanelDockStyle panelDockStyle, Size size)
		{
			switch (panelDockStyle)
			{
			case PanelDockStyle.Left:
			case PanelDockStyle.Right:
				if (uiPanelBase.IsRootPanel())
				{
					if (!uiPanelBase.IsFloating() && (uiPanelBase.DockStyle == PanelDockStyle.Left || uiPanelBase.DockStyle == PanelDockStyle.Right))
					{
						size.Width = uiPanelBase.Width;
						break;
					}
					size.Width = _fillArea.Width / 2;
					if (uiPanelBase._dockedLayoutState != null && (uiPanelBase._dockedLayoutState.GetDockStyle() == PanelDockStyle.Left || uiPanelBase._dockedLayoutState.GetDockStyle() == PanelDockStyle.Right) && uiPanelBase._dockedLayoutState.GetPanelSize().Width > 0)
					{
						size.Width = uiPanelBase._dockedLayoutState.GetPanelSize().Width;
					}
				}
				else
				{
					size.Width = _fillArea.Width / 2;
				}
				break;
			case PanelDockStyle.Top:
			case PanelDockStyle.Bottom:
				if (uiPanelBase.IsRootPanel())
				{
					if (!uiPanelBase.IsFloating() && (uiPanelBase.DockStyle == PanelDockStyle.Top || uiPanelBase.DockStyle == PanelDockStyle.Bottom))
					{
						size.Height = uiPanelBase.Height;
						break;
					}
					size.Height = _fillArea.Height / 2;
					if (uiPanelBase._dockedLayoutState != null && (uiPanelBase._dockedLayoutState.GetDockStyle() == PanelDockStyle.Top || uiPanelBase._dockedLayoutState.GetDockStyle() == PanelDockStyle.Bottom) && uiPanelBase._dockedLayoutState.GetPanelSize().Height > 0)
					{
						size.Height = uiPanelBase._dockedLayoutState.GetPanelSize().Height;
					}
				}
				else
				{
					size.Height = _fillArea.Height / 2;
				}
				break;
			}
			return size;
		}

		internal PanelDropInfo GetContainerEdgeDropInfo(DockPanelBase uiPanelBase, PanelDockStyle panelDockStyle)
		{
			Rectangle dropBounds = _dockArea;
			dropBounds.Size = GetDockedSize(uiPanelBase, panelDockStyle, dropBounds.Size);
			switch (panelDockStyle)
			{
			case PanelDockStyle.Right:
				dropBounds.X = _dockArea.Right - dropBounds.Width;
				break;
			case PanelDockStyle.Bottom:
				dropBounds.Y = _dockArea.Bottom - dropBounds.Height;
				break;
			}
			dropBounds.Location = _containerControl.PointToScreen(dropBounds.Location);
			PanelDropInfo panelDropInfo = new PanelDropInfo();
			panelDropInfo._createsGroup = false;
			panelDropInfo._panelDockStyle = panelDockStyle;
			panelDropInfo._dropBounds = dropBounds;
			panelDropInfo._insertIndex = 0;
			panelDropInfo._targetPanel = null;
			return panelDropInfo;
		}

		internal PanelDropInfo GetFillAreaEdgeDropInfo(DockPanelBase uiPanelBase, PanelDockStyle panelDockStyle)
		{
			if (GetDragHoverPanel() != null)
			{
				PanelDropInfo panelDropInfo = GetDragHoverPanel().GetTileDropInfo(panelDockStyle);
				if (GetDragHoverPanel().ParentGroup != null)
				{
					MxDockPanelGroup parentGroup = GetDragHoverPanel().ParentGroup;
					if (parentGroup.Panels.Contains(panelDropInfo._targetPanel))
					{
						parentGroup._groupState.ProcessChildDropInfo(panelDropInfo);
					}
				}
				return panelDropInfo;
			}
			Rectangle dropBounds = _fillArea;
			dropBounds.Size = GetDockedSize(uiPanelBase, panelDockStyle, dropBounds.Size);
			switch (panelDockStyle)
			{
			case PanelDockStyle.Right:
				dropBounds.X = _fillArea.Right - dropBounds.Width;
				break;
			case PanelDockStyle.Bottom:
				dropBounds.Y = _fillArea.Bottom - dropBounds.Height;
				break;
			}
			dropBounds.Location = _containerControl.PointToScreen(dropBounds.Location);
			PanelDropInfo edgeDropInfo = new PanelDropInfo();
			edgeDropInfo._createsGroup = false;
			edgeDropInfo._panelDockStyle = panelDockStyle;
			edgeDropInfo._dropBounds = dropBounds;
			edgeDropInfo._insertIndex = -1;
			edgeDropInfo._targetPanel = null;
			return edgeDropInfo;
		}

		private PanelDockStyle HitTestEdge(Rectangle bounds, Point pt, int edgeWidth)
		{
			if (bounds.Contains(pt))
			{
				if (pt.X < bounds.X + edgeWidth)
				{
					return PanelDockStyle.Left;
				}
				if (pt.Y < bounds.Y + edgeWidth)
				{
					return PanelDockStyle.Top;
				}
				if (pt.X > bounds.Right - edgeWidth)
				{
					return PanelDockStyle.Right;
				}
				if (pt.Y > bounds.Bottom - edgeWidth)
				{
					return PanelDockStyle.Bottom;
				}
			}
			return PanelDockStyle.Fill;
		}

		internal PanelDropInfo ApplyDraggingPanelResult(DockPanelBase uiPanelBase, PanelDropInfo panelDropInfo)
		{
			PanelDragDropEventArgs e = CreateDragDropEventArgs(uiPanelBase, panelDropInfo);
			Size size = panelDropInfo._dropBounds.Size;
			OnDraggingPanel(e);
			panelDropInfo._isAllowed = e.CanDrop;
			if (size != e.Size && !panelDropInfo._createsGroup)
			{
				if (panelDropInfo._panelDockState == PanelDockState.Floating)
				{
					panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, e.Size);
				}
				else
				{
					switch (panelDropInfo._panelDockStyle)
					{
					case PanelDockStyle.Left:
						panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, new Size(e.Size.Width, size.Height));
						break;
					case PanelDockStyle.Right:
					{
						int x = panelDropInfo._dropBounds.Left + (size.Width - e.Size.Width);
						panelDropInfo._dropBounds = new Rectangle(x, panelDropInfo._dropBounds.Location.Y, e.Size.Width, size.Height);
						break;
					}
					case PanelDockStyle.Top:
						panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, new Size(size.Width, e.Size.Height));
						break;
					case PanelDockStyle.Bottom:
					{
						int y = panelDropInfo._dropBounds.Top + (size.Height - e.Size.Height);
						panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location.X, y, size.Width, e.Size.Height);
						break;
					}
					default:
						panelDropInfo._dropBounds = new Rectangle(panelDropInfo._dropBounds.Location, e.Size);
						break;
					}
				}
			}
			return panelDropInfo;
		}
	}
}
