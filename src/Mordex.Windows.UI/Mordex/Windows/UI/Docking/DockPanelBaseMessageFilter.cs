using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class DockPanelBaseMessageFilter : DockManagerMessageFilter
	{
		private const int CenterTargetSize = 88;

		private const int EdgeTargetWidth = 30;

		private const int EdgeTargetHeight = 33;

		private const int EdgeTargetMargin = 15;

		// Margin plus target height, so the far edge targets keep the same gap as the near ones.
		private const int EdgeTargetFarOffset = 48;

		private DropTargetWindowDialog _topTarget;

		private DropTargetWindowDialog _leftTarget;

		private DropTargetWindowDialog _bottomTarget;

		private DropTargetWindowDialog _rightTarget;

		private DropTargetWindowDialog _centerTarget;

		private DropTargetWindowDialog _previewWindow;

		private DockPanelBase _targetPanel;

		internal DockPanelBaseMessageFilter(MxDockManager manager, DockPanelBase panel, Point offset)
			: base(manager, panel, offset)
		{
		}

		internal override void StartDrag()
		{
			RenderingHelper.SetMouseCapture(GetDockManager().ContainerControl, true);
			Application.AddMessageFilter(this);
			LayoutCenterTarget(true);
			CreateEdgeTargets();
			ContinueDrag();
		}

		private bool ShowDropPreview(PanelDropInfo panelDropInfo)
		{
			panelDropInfo = GetDockManager().ApplyDraggingPanelResult(_draggedPanel, panelDropInfo);
			if (!panelDropInfo._isAllowed)
			{
				return false;
			}
			Rectangle previewBounds = panelDropInfo._dropBounds;
			GraphicsPath graphicsPath = null;
			if (panelDropInfo._createsGroup)
			{
				if (panelDropInfo._groupStyle == PanelGroupStyle.Tab)
				{
					previewBounds.Height += panelDropInfo._tabHeaderBounds.Height;
					graphicsPath = new GraphicsPath();
					graphicsPath.AddRectangle(new Rectangle(Point.Empty, panelDropInfo._dropBounds.Size));
					graphicsPath.AddRectangle(new Rectangle(panelDropInfo._tabHeaderBounds.X - panelDropInfo._dropBounds.X, panelDropInfo._tabHeaderBounds.Y - panelDropInfo._dropBounds.Y, panelDropInfo._tabHeaderBounds.Width, panelDropInfo._tabHeaderBounds.Height));
					graphicsPath.CloseAllFigures();
				}
			}
			else if (panelDropInfo._targetPanel is MxDockPanelGroup)
			{
				MxDockPanelGroup uiPanelGroup = panelDropInfo._targetPanel as MxDockPanelGroup;
				if (uiPanelGroup.IsTabGroup())
				{
					bool canInsertTab = true;
					if (_draggedPanel is MxDockPanelGroup && !((MxDockPanelGroup)_draggedPanel).StaticGroup)
					{
						canInsertTab = false;
					}
					if (canInsertTab)
					{
						uiPanelGroup.Panels.Insert(panelDropInfo._insertIndex, _draggedPanel);
						GetDockManager().RemoveRedundantGroups();
						if (_previewWindow != null)
						{
							_previewWindow.Dispose();
							_previewWindow = null;
						}
						return true;
					}
				}
			}
			if (_previewWindow == null)
			{
				_previewWindow = new DropTargetWindowDialog(GetDockManager(), DropTargetType.Preview);
			}
			_previewWindow.SetPreviewPath(graphicsPath);
			_previewWindow.Bounds = previewBounds;
			_previewWindow.ShowWithoutActivating();
			return true;
		}

		private void FloatPanelAtCursor()
		{
			PanelDockState dockState = _draggedPanel.DockState;
			int width = _draggedPanel.Width;
			PanelDropInfo panelDropInfo = GetDockManager().GetFloatingDropInfo(_draggedPanel, Cursor.Position);
			panelDropInfo = GetDockManager().ApplyDraggingPanelResult(_draggedPanel, panelDropInfo);
			if (panelDropInfo._isAllowed)
			{
				_draggedPanel.AttachToGroupOrDetach(null, panelDropInfo._panelDockState, panelDropInfo._panelDockStyle);
				if (width != _draggedPanel.Width)
				{
					int x = (int)((float)GetOffset().X / (float)width * (float)_draggedPanel.Width);
					SetOffset(new Point(x, GetOffset().Y));
				}
				_draggedPanel.FloatingLocation = panelDropInfo._dropBounds.Location;
				_draggedPanel.FloatingSize = panelDropInfo._dropBounds.Size;
				if (_draggedPanel.FloatingWindow != null)
				{
					_draggedPanel.FloatingWindow.Update();
				}
				if (dockState != PanelDockState.Floating)
				{
					LayoutCenterTarget(true);
					GetDockManager().RemoveRedundantGroups();
				}
				GetDockManager().RefreshFormIfNoComposition();
				GetDockManager().RefreshFloatingWindows();
			}
		}

		private void HideTargets()
		{
			if (_topTarget != null)
			{
				_topTarget.Hide();
			}
			if (_leftTarget != null)
			{
				_leftTarget.Hide();
			}
			if (_rightTarget != null)
			{
				_rightTarget.Hide();
			}
			if (_bottomTarget != null)
			{
				_bottomTarget.Hide();
			}
			if (_centerTarget != null)
			{
				_centerTarget.Hide();
			}
		}

		private void ShowTargets()
		{
			if (_topTarget != null)
			{
				_topTarget.ShowWithoutActivating();
			}
			if (_leftTarget != null)
			{
				_leftTarget.ShowWithoutActivating();
			}
			if (_rightTarget != null)
			{
				_rightTarget.ShowWithoutActivating();
			}
			if (_bottomTarget != null)
			{
				_bottomTarget.ShowWithoutActivating();
			}
			if (_centerTarget != null && (!(_targetPanel is MxDockPanelGroup uiPanelGroup) || uiPanelGroup.IsTabGroup()))
			{
				_centerTarget.ShowWithoutActivating();
			}
		}

		private void ShowPreviewOnly(PanelDropInfo panelDropInfo)
		{
			HideTargets();
			if (_draggedPanel.FloatingWindow != null)
			{
				RenderingHelper.SetMouseCapture(_draggedPanel, true);
			}
			ShowDropPreview(panelDropInfo);
		}

		protected override void ContinueDrag()
		{
			Point position = Cursor.Position;
			PanelDropInfo panelDropInfo = GetDockManager().GetGuideDropInfo(_draggedPanel, position);
			if (panelDropInfo._createsGroup || (panelDropInfo._groupStyle != PanelGroupStyle.HorizontalTiles && panelDropInfo._groupStyle != PanelGroupStyle.VerticalTiles))
			{
				panelDropInfo = GetDockManager().ApplyDraggingPanelResult(_draggedPanel, panelDropInfo);
				if (panelDropInfo._isAllowed)
				{
					ShowPreviewOnly(panelDropInfo);
				}
				return;
			}
			ShowTargets();
			if (_topTarget.Bounds.Contains(position))
			{
				PanelDropInfo topDropInfo = _topTarget.GetDropInfo(_draggedPanel, position);
				if (ShowDropPreview(topDropInfo))
				{
					return;
				}
			}
			else if (_leftTarget.Bounds.Contains(position))
			{
				PanelDropInfo leftDropInfo = _leftTarget.GetDropInfo(_draggedPanel, position);
				if (ShowDropPreview(leftDropInfo))
				{
					return;
				}
			}
			else if (_bottomTarget.Bounds.Contains(position))
			{
				PanelDropInfo bottomDropInfo = _bottomTarget.GetDropInfo(_draggedPanel, position);
				if (ShowDropPreview(bottomDropInfo))
				{
					return;
				}
			}
			else if (_rightTarget.Bounds.Contains(position))
			{
				PanelDropInfo rightDropInfo = _rightTarget.GetDropInfo(_draggedPanel, position);
				if (ShowDropPreview(rightDropInfo))
				{
					return;
				}
			}
			if (_centerTarget != null)
			{
				PanelDropInfo centerDropInfo = _centerTarget.GetDropInfo(_draggedPanel, position);
				if (centerDropInfo != null && ShowDropPreview(centerDropInfo))
				{
					return;
				}
			}
			if (_previewWindow != null)
			{
				_previewWindow.Dispose();
				_previewWindow = null;
			}
			FloatPanelAtCursor();
		}

		internal override void EndDrag(bool cancel)
		{
			try
			{
				Application.RemoveMessageFilter(this);
			}
			catch
			{
			}
			if (cancel)
			{
				GetDockManager().ClearDragFilter();
			}
			else
			{
				Point position = Cursor.Position;
				PanelDropInfo panelDropInfo = GetDockManager().GetGuideDropInfo(_draggedPanel, position);
				if (!panelDropInfo._createsGroup && (panelDropInfo._groupStyle == PanelGroupStyle.HorizontalTiles || panelDropInfo._groupStyle == PanelGroupStyle.VerticalTiles))
				{
					panelDropInfo = (_topTarget.Bounds.Contains(position) ? _topTarget.GetDropInfo(_draggedPanel, position) : (_leftTarget.Bounds.Contains(position) ? _leftTarget.GetDropInfo(_draggedPanel, position) : (_bottomTarget.Bounds.Contains(position) ? _bottomTarget.GetDropInfo(_draggedPanel, position) : (_rightTarget.Bounds.Contains(position) ? _rightTarget.GetDropInfo(_draggedPanel, position) : ((_centerTarget == null || !_centerTarget.Bounds.Contains(position)) ? GetDockManager().GetFloatingDropInfo(_draggedPanel, position) : _centerTarget.GetDropInfo(_draggedPanel, position))))));
				}
				if (panelDropInfo != null)
				{
					panelDropInfo = GetDockManager().ApplyDraggingPanelResult(_draggedPanel, panelDropInfo);
					GetDockManager().EndPanelDrag(_draggedPanel, panelDropInfo);
				}
				_draggedPanel.Activate();
			}
			DisposeTargetWindows();
			for (Control control = _draggedPanel; control != null; control = RenderingHelper.GetParentControl(control))
			{
				if (control.Capture)
				{
					RenderingHelper.SetMouseCapture(control, false);
					break;
				}
			}
		}

		private void LayoutCenterTarget(bool allowTab)
		{
			Size size = new Size(CenterTargetSize, CenterTargetSize);
			if (GetTargetPanel() == null && !GetDockManager().HasNonMdiContainer())
			{
				allowTab = false;
			}
			if (_centerTarget == null)
			{
				_centerTarget = new DropTargetWindowDialog(GetDockManager(), DropTargetType.All, allowTab);
			}
			else
			{
				_centerTarget.SetAllowTab(allowTab);
			}
			Rectangle rectangle;
			if (_targetPanel != null)
			{
				if (_targetPanel is MxDockPanelGroup)
				{
					MxDockPanelGroup uiPanelGroup = _targetPanel as MxDockPanelGroup;
					if (uiPanelGroup.SelectedPanel != null)
					{
						rectangle = uiPanelGroup.SelectedPanel.DisplayRectangle;
						rectangle.Location = uiPanelGroup.SelectedPanel.PointToScreen(rectangle.Location);
					}
					else
					{
						rectangle = uiPanelGroup.DisplayRectangle;
						rectangle.Location = _targetPanel.PointToScreen(rectangle.Location);
					}
				}
				else
				{
					rectangle = _targetPanel.DisplayRectangle;
					rectangle.Location = _targetPanel.PointToScreen(rectangle.Location);
				}
			}
			else
			{
				rectangle = GetDockManager().GetFillArea();
				rectangle.Location = GetDockManager().ContainerControl.PointToScreen(rectangle.Location);
			}
			int x = rectangle.X + (rectangle.Width - size.Width) / 2;
			int y = rectangle.Y + (rectangle.Height - size.Height) / 2;
			_centerTarget.Bounds = new Rectangle(x, y, size.Width, size.Height);
		}

		private void CreateEdgeTargets()
		{
			Size size = new Size(EdgeTargetWidth, EdgeTargetHeight);
			_topTarget = new DropTargetWindowDialog(GetDockManager(), DropTargetType.Top);
			_leftTarget = new DropTargetWindowDialog(GetDockManager(), DropTargetType.Left);
			_bottomTarget = new DropTargetWindowDialog(GetDockManager(), DropTargetType.Bottom);
			_rightTarget = new DropTargetWindowDialog(GetDockManager(), DropTargetType.Right);
			Rectangle rectangle = GetDockManager().GetDockArea();
			rectangle.Location = GetDockManager().ContainerControl.PointToScreen(rectangle.Location);
			int x = rectangle.X + (rectangle.Width - size.Width) / 2;
			int y = rectangle.Y + (rectangle.Height - size.Width) / 2;
			Rectangle topBounds = new Rectangle(x, rectangle.Y + EdgeTargetMargin, size.Width, size.Height);
			Rectangle leftBounds = new Rectangle(rectangle.X + EdgeTargetMargin, y, size.Height, size.Width);
			Rectangle rightBounds = new Rectangle(rectangle.Right - EdgeTargetFarOffset, y, size.Height, size.Width);
			Rectangle bottomBounds = new Rectangle(x, rectangle.Bottom - EdgeTargetFarOffset, size.Width, size.Height);
			_topTarget.Bounds = topBounds;
			_leftTarget.Bounds = leftBounds;
			_rightTarget.Bounds = rightBounds;
			_bottomTarget.Bounds = bottomBounds;
		}

		private void DisposeCenterTarget()
		{
			if (_centerTarget != null)
			{
				_centerTarget.Dispose();
				_centerTarget = null;
			}
		}

		internal void DisposeTargetWindows()
		{
			_topTarget.Dispose();
			_leftTarget.Dispose();
			_bottomTarget.Dispose();
			_rightTarget.Dispose();
			if (_centerTarget != null)
			{
				_centerTarget.Dispose();
			}
			if (_previewWindow != null)
			{
				_previewWindow.Dispose();
			}
		}

		internal DockPanelBase GetTargetPanel()
		{
			return _targetPanel;
		}

		internal void SetTargetPanel(DockPanelBase panel)
		{
			if (_targetPanel == panel)
			{
				return;
			}
			_targetPanel = panel;
			if (GetTargetPanel() is MxDockPanelGroup)
			{
				MxDockPanelGroup uiPanelGroup = _targetPanel as MxDockPanelGroup;
				if (!uiPanelGroup.IsTabGroup())
				{
					DisposeCenterTarget();
				}
				else
				{
					LayoutCenterTarget(!uiPanelGroup.StaticGroup);
				}
			}
			else
			{
				LayoutCenterTarget(true);
			}
		}
	}
}
