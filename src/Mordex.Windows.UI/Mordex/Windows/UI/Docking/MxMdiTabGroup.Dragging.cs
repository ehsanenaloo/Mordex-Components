using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
		internal bool HasSplitter()
		{
			return _hasSplitter;
		}

		internal void SetHasSplitter(bool hasSplitter)
		{
			if (_hasSplitter == hasSplitter)
			{
				return;
			}
			if (_hasSplitter)
			{
				_splitter.Dispose();
				_splitter = null;
			}
			_hasSplitter = hasSplitter;
			if (_hasSplitter)
			{
				_splitter = new SplitterMdi(this);
				_splitter.Parent = base.Parent;
				_splitter.Visible = true;
			}
		}

		internal SplitterMdi GetSplitter()
		{
			return _splitter;
		}

		void IDockElement.CancelSizing()
		{
			EndTabDrag(true);
		}

		void IDockElement.EraseSizingBar()
		{
			EraseDropIndicator();
		}

		private void EraseDropIndicator()
		{
			if (PanelManager.TabbedMdiSettings.DockingStyleMode != DockingStyleMode.VS2005 && !_dropIndicatorBounds.IsEmpty)
			{
				RenderingHelper.InvertDragFrames(new Rectangle[1] { _dropIndicatorBounds });
				_dropIndicatorBounds = Rectangle.Empty;
			}
		}

		private void EndTabDrag(bool isCancelled)
		{
			if (!_isDraggingTab)
			{
				return;
			}
			DisposeDropPreview();
			Cursor.Current = Cursors.Default;
			_isDraggingTab = false;
			EraseDropIndicator();
			if (_dockMessageFilter != null)
			{
				_dockMessageFilter.DetachFromForm();
				Application.RemoveMessageFilter(_dockMessageFilter);
				_dockMessageFilter = null;
			}
			if (!isCancelled)
			{
				if (_dropTargetGroup != null)
				{
					_tabGroupHost.MoveFormToGroup(_dropTargetGroup, GetPressedTabLayout().GetChildTab().Form);
					GetPressedTabLayout().GetChildTab().Form.Activate();
				}
				else if (_dropGroupIndex != -1)
				{
					if (PanelManager.MdiTabGroups.Length <= 1)
					{
						PanelManager.TabbedMdiSettings.GroupOrientation = _mdiGroupOrientation;
					}
					_tabGroupHost.InsertGroup(_dropGroupIndex, GetPressedTabLayout().GetChildTab().Form);
					GetPressedTabLayout().GetChildTab().Form.Activate();
				}
			}
			_dropTargetGroup = null;
			_dropGroupIndex = -1;
		}

		private void BeginTabDrag()
		{
			if (_isDraggingTab || !PanelManager.TabbedMdiSettings.AllowDrag || _tabGroupHost.GetForm().MdiChildren.Length <= 1)
			{
				return;
			}
			_dropGroupIndex = -1;
			_isDraggingTab = true;
			_dockMessageFilter = new DockMessageFilter(this);
			Application.AddMessageFilter(_dockMessageFilter);
		}

		private bool CanUseOrientation(MdiGroupOrientation mdiGroupOrientation)
		{
			if (mdiGroupOrientation == PanelManager.TabbedMdiSettings.GroupOrientation)
			{
				return true;
			}
			return _tabGroupHost.GetGroups().Count < 2;
		}

		private Cursor GetDragCursor(int cursorKind)
		{
			Stream stream = null;
			if (cursorKind == 0)
			{
				if (_dragCursor == null)
				{
					stream = new MemoryStream(WindowsResources.DragPage());
					_dragCursor = new Cursor(stream);
					stream.Close();
				}
				return _dragCursor;
			}
			if (_noDragCursor == null)
			{
				stream = new MemoryStream(WindowsResources.NoDragPage());
				_noDragCursor = new Cursor(stream);
				stream.Close();
			}
			return _noDragCursor;
		}

		private void UpdateDropTarget()
		{
			_dropTargetGroup = null;
			_dropGroupIndex = -1;
			Point position = Cursor.Position;
			Point pt = PointToClient(position);
			MdiTabItem uiMdiChildTabLayoutState = HitTestTab(pt.X, pt.Y);
			if (uiMdiChildTabLayoutState != null && uiMdiChildTabLayoutState.GetIsVisible() && uiMdiChildTabLayoutState.GetButtonType() == TabButtonType.ChildForm && uiMdiChildTabLayoutState != _pressedTabLayout)
			{
				ReorderTabs(_pressedTabLayout, uiMdiChildTabLayoutState);
			}
			if (!base.ClientRectangle.Contains(pt))
			{
				Point mdiClientPoint = _tabGroupHost.GetMdiClient().PointToClient(position);
				if (_tabGroupHost.GetMdiClient().ClientRectangle.Contains(mdiClientPoint))
				{
					Cursor.Current = GetDragCursor(0);
					foreach (MxMdiTabGroup item in _tabGroupHost.GetGroups())
					{
						Point itemPoint = item.PointToClient(position);
						if (item.ClientRectangle.Contains(itemPoint))
						{
							_dropTargetGroup = item;
							Rectangle formRectangle = item.FormRectangle;
							formRectangle.X -= PanelManager.TabbedMdiSettings.WindowPadding.Left;
							formRectangle.Y -= PanelManager.TabbedMdiSettings.WindowPadding.Top;
							formRectangle.Width += PanelManager.TabbedMdiSettings.WindowPadding.Left + PanelManager.TabbedMdiSettings.WindowPadding.Right;
							formRectangle.Height += PanelManager.TabbedMdiSettings.WindowPadding.Top + PanelManager.TabbedMdiSettings.WindowPadding.Bottom;
							formRectangle.Location = _tabGroupHost.GetMdiClient().PointToScreen(formRectangle.Location);
							SetDropIndicatorBounds(formRectangle);
							break;
						}
						if (!item.HasSplitter())
						{
							continue;
						}
						itemPoint = item.GetSplitter().PointToClient(position);
						Rectangle clientRectangle = item._splitter.ClientRectangle;
						if (PanelManager.TabbedMdiSettings.GroupOrientation == MdiGroupOrientation.Horizontal)
						{
							clientRectangle.Y -= DropEdgeSize;
							clientRectangle.Height += DropEdgeSize;
						}
						else
						{
							clientRectangle.X -= DropEdgeSize;
							clientRectangle.Width += DropEdgeSize;
						}
						if (!clientRectangle.Contains(itemPoint))
						{
							continue;
						}
						_dropGroupIndex = item.GroupIndex + 1;
						if (TabCount == 1 && (_dropGroupIndex == GroupIndex || _dropGroupIndex == GroupIndex + 1))
						{
							_dropGroupIndex = -1;
						}
						if (_dropGroupIndex != -1)
						{
							Rectangle rectangle = item.GetGroupBounds();
							if (PanelManager.TabbedMdiSettings.GroupOrientation == MdiGroupOrientation.Horizontal)
							{
								rectangle.Y += rectangle.Height / 2;
							}
							else
							{
								rectangle.X += rectangle.Width / 2;
							}
							rectangle.Location = _tabGroupHost.GetMdiClient().PointToScreen(rectangle.Location);
							SetDropIndicatorBounds(rectangle);
							break;
						}
					}
					if (_dropTargetGroup != null || _dropGroupIndex != -1)
					{
						return;
					}
					Rectangle newGroupBounds = Rectangle.Empty;
					Rectangle mdiClientBounds = _tabGroupHost.GetMdiClient().ClientRectangle;
					if (mdiClientBounds.X + DropEdgeSize >= mdiClientPoint.X)
					{
						if (CanUseOrientation(MdiGroupOrientation.Vertical))
						{
							_mdiGroupOrientation = MdiGroupOrientation.Vertical;
							_dropGroupIndex = 0;
							newGroupBounds = new Rectangle(mdiClientBounds.X, mdiClientBounds.Y, mdiClientBounds.Width / (_tabGroupHost.GetGroups().Count + 1), mdiClientBounds.Height);
						}
					}
					else if (mdiClientBounds.Right - DropEdgeSize <= mdiClientPoint.X)
					{
						if (CanUseOrientation(MdiGroupOrientation.Vertical))
						{
							_mdiGroupOrientation = MdiGroupOrientation.Vertical;
							_dropGroupIndex = _tabGroupHost.GetGroups().Count;
							newGroupBounds = new Rectangle(mdiClientBounds.Right - mdiClientBounds.Width / (_tabGroupHost.GetGroups().Count + 1), mdiClientBounds.Y, mdiClientBounds.Width / (_tabGroupHost.GetGroups().Count + 1), mdiClientBounds.Height);
						}
					}
					else if (mdiClientBounds.Bottom - DropEdgeSize <= mdiClientPoint.Y && CanUseOrientation(MdiGroupOrientation.Horizontal))
					{
						_mdiGroupOrientation = MdiGroupOrientation.Horizontal;
						_dropGroupIndex = _tabGroupHost.GetGroups().Count;
						newGroupBounds = new Rectangle(mdiClientBounds.X, mdiClientBounds.Bottom - mdiClientBounds.Height / (_tabGroupHost.GetGroups().Count + 1), mdiClientBounds.Width, mdiClientBounds.Height / (_tabGroupHost.GetGroups().Count + 1));
					}
					if (!newGroupBounds.IsEmpty)
					{
						newGroupBounds.Location = _tabGroupHost.GetMdiClient().PointToScreen(newGroupBounds.Location);
					}
					SetDropIndicatorBounds(newGroupBounds);
				}
				else
				{
					Cursor.Current = GetDragCursor(1);
					SetDropIndicatorBounds(Rectangle.Empty);
				}
			}
			else
			{
				Cursor.Current = Cursors.Default;
				SetDropIndicatorBounds(Rectangle.Empty);
			}
		}

		private void ShowDropPreview(Rectangle previewBounds)
		{
			Rectangle bounds = previewBounds;
			GraphicsPath graphicsPath = null;
			if (_dropTargetGroup != null)
			{
				bounds.Y -= _dropTargetGroup._stripHeight;
				bounds.Height += _dropTargetGroup._stripHeight;
				graphicsPath = new GraphicsPath();
				Rectangle rectangle = _dropTargetGroup._groupLayout.GetTabBounds(_dropTargetGroup._groupLayout.GetFirstVisibleIndex(), false);
				graphicsPath.AddRectangle(new Rectangle(rectangle.X, 0, rectangle.Width, _dropTargetGroup._stripHeight));
				graphicsPath.AddRectangle(new Rectangle(new Point(0, _dropTargetGroup._stripHeight), previewBounds.Size));
				graphicsPath.CloseAllFigures();
			}
			else
			{
				graphicsPath = new GraphicsPath();
				graphicsPath.AddRectangle(new Rectangle(Point.Empty, previewBounds.Size));
				graphicsPath.CloseAllFigures();
			}
			if (_dropTargetWindowDialog == null)
			{
				_dropTargetWindowDialog = new DropTargetWindowDialog(PanelManager, DropTargetType.Preview);
			}
			_dropTargetWindowDialog.SetPreviewPath(graphicsPath);
			_dropTargetWindowDialog.Bounds = bounds;
			_allowCancelDrag = false;
			_dropTargetWindowDialog.ShowWithoutActivating();
			_allowCancelDrag = true;
		}

		private void DisposeDropPreview()
		{
			if (_dropTargetWindowDialog != null)
			{
				_dropTargetWindowDialog.Dispose();
				_dropTargetWindowDialog = null;
			}
		}

		private void SetDropIndicatorBounds(Rectangle bounds)
		{
			if (!(_dropIndicatorBounds != bounds))
			{
				return;
			}
			if (!_dropIndicatorBounds.IsEmpty)
			{
				if (PanelManager.TabbedMdiSettings.DockingStyleMode == DockingStyleMode.VS2005)
				{
					DisposeDropPreview();
				}
				else
				{
					RenderingHelper.InvertDragFrames(new Rectangle[1] { _dropIndicatorBounds });
				}
			}
			_dropIndicatorBounds = bounds;
			if (!_dropIndicatorBounds.IsEmpty)
			{
				if (PanelManager.TabbedMdiSettings.DockingStyleMode == DockingStyleMode.VS2005)
				{
					ShowDropPreview(_dropIndicatorBounds);
					return;
				}
				RenderingHelper.InvertDragFrames(new Rectangle[1] { _dropIndicatorBounds });
			}
		}

		internal void ReorderTabs(MdiTabItem uiMdiChildTabLayoutState, MdiTabItem targetTab)
		{
			_groupLayout.MoveTab(uiMdiChildTabLayoutState, targetTab);
			_childTabs.Clear();
			foreach (MdiTabItem item in (IEnumerable)_groupLayout)
			{
				if (item.GetButtonType() == TabButtonType.ChildForm)
				{
					_childTabs.Add(item.GetChildTab());
				}
			}
			if (_groupLayout != null)
			{
				_groupLayout.PositionCloseButton();
			}
			Invalidate();
			Update();
		}

		internal void MoveTabToStart(MdiChildTab item)
		{
			if (_childTabs.Contains(item))
			{
				_childTabs.Remove(item);
			}
			_childTabs.Insert(0, item);
			RebuildLayout(false);
		}
	}
}
