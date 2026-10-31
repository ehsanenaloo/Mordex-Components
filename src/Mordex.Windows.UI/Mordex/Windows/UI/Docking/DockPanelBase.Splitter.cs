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
		private ResizeMode GetResizeModeAt(int x, int y)
		{
			if (!_splitterBounds.IsEmpty && CanResize() && _splitterBounds.Contains(x, y))
			{
				if (IsSideDocked())
				{
					return ResizeMode.Vertical;
				}
				return ResizeMode.Horizontal;
			}
			return ResizeMode.NoResize;
		}

		private void BeginSplitterDrag(int x, int y)
		{
			if (PanelManager == null)
			{
				return;
			}
			if (IsSideDocked())
			{
				int minimumContainerWidth = MinimumContainerSpace;
				int minimumWidth = GetMinimumSize().Width;
				int maximumWidth = GetMaximumSize().Width;
				int availableWidth = ((!AutoHide) ? (Width + PanelManager.GetFillArea().Width) : PanelManager.GetDockArea().Width);
				int dragWidth = availableWidth - minimumContainerWidth - minimumWidth;
				if (maximumWidth > 0 && maximumWidth < dragWidth + minimumWidth)
				{
					dragWidth = maximumWidth - minimumWidth;
				}
				if (DockStyle == PanelDockStyle.Left)
				{
					_splitterDragArea = new Rectangle(minimumWidth, 0, dragWidth, Height);
				}
				else
				{
					int maximumLeft = Width - minimumWidth;
					_splitterDragArea = new Rectangle(maximumLeft - dragWidth, 0, dragWidth, Height);
				}
			}
			else
			{
				int minimumContainerHeight = MinimumContainerSpace;
				int minimumHeight = GetMinimumSize().Height;
				int maximumHeight = GetMaximumSize().Height;
				int availableHeight = ((!AutoHide) ? (Height + PanelManager.GetFillArea().Height) : PanelManager.GetDockArea().Height);
				int dragHeight = availableHeight - minimumContainerHeight - minimumHeight;
				if (maximumHeight > 0 && maximumHeight < dragHeight + minimumHeight)
				{
					dragHeight = maximumHeight - minimumHeight;
				}
				if (DockStyle == PanelDockStyle.Top)
				{
					_splitterDragArea = new Rectangle(0, minimumHeight, Width, dragHeight);
				}
				else
				{
					int maximumTop = Height - minimumHeight;
					_splitterDragArea = new Rectangle(0, maximumTop - dragHeight, Width, dragHeight);
				}
			}
			_splitterDragArea.Location = PointToScreen(_splitterDragArea.Location);
			_splitterTrackBounds = _splitterBounds;
			if (!_splitterTrackBounds.IsEmpty)
			{
				_splitterTrackBounds.Location = PointToScreen(_splitterTrackBounds.Location);
				DockRenderer.ToggleDragFrame(_splitterTrackBounds, GetSplitterSize());
				_splitterStartLocation = _splitterTrackBounds.Location;
			}
			_dockMessageFilter = new DockMessageFilter(this);
			Application.AddMessageFilter(_dockMessageFilter);
		}

		private Rectangle GetSplitterTrackBounds(int x, int y)
		{
			Point point = PointToScreen(new Point(x, y));
			Rectangle result = Rectangle.Empty;
			if (_resizeMode == ResizeMode.Horizontal)
			{
				int trackTop = point.Y - GetSplitterSize() / 2;
				if (trackTop < _splitterDragArea.Y)
				{
					trackTop = _splitterDragArea.Y;
				}
				else if (trackTop + GetSplitterSize() > _splitterDragArea.Bottom)
				{
					trackTop = _splitterDragArea.Bottom - GetSplitterSize();
				}
				result = new Rectangle(_splitterDragArea.X, trackTop, _splitterDragArea.Width, GetSplitterSize());
			}
			else if (_resizeMode == ResizeMode.Vertical)
			{
				int trackLeft = point.X - GetSplitterSize() / 2;
				if (trackLeft < _splitterDragArea.Left)
				{
					trackLeft = _splitterDragArea.Left;
				}
				else if (trackLeft + GetSplitterSize() > _splitterDragArea.Right)
				{
					trackLeft = _splitterDragArea.Right - GetSplitterSize();
				}
				result = new Rectangle(trackLeft, _splitterDragArea.Y, GetSplitterSize(), _splitterDragArea.Height);
			}
			return result;
		}

		private void MoveSplitterTrack(int x, int y)
		{
			Rectangle rectangle = GetSplitterTrackBounds(x, y);
			if (rectangle != _splitterTrackBounds)
			{
				if (!_isSplitterTrackErased)
				{
					DockRenderer.ToggleDragFrame(_splitterTrackBounds, GetSplitterSize());
				}
				_splitterTrackBounds = rectangle;
				DockRenderer.ToggleDragFrame(_splitterTrackBounds, GetSplitterSize());
				_isSplitterTrackErased = false;
			}
			else if (_isSplitterTrackErased)
			{
				DockRenderer.ToggleDragFrame(_splitterTrackBounds, GetSplitterSize());
				_isSplitterTrackErased = false;
			}
		}

		internal void EraseSplitterTrack()
		{
			if (!_isSplitterTrackErased)
			{
				DockRenderer.ToggleDragFrame(_splitterTrackBounds, GetSplitterSize());
				_isSplitterTrackErased = true;
			}
		}

		internal void EndSplitterDrag(int x, int y, bool isCanceled)
		{
			if (!_dragStartBounds.IsEmpty)
			{
				_dragStartBounds = Rectangle.Empty;
			}
			if (_resizeMode == ResizeMode.NoResize)
			{
				return;
			}
			PanelManager.RaiseComponentChanging("Size Panel", this);
			DockRenderer.ToggleDragFrame(_splitterTrackBounds, GetSplitterSize());
			if (!isCanceled)
			{
				if (_resizeMode == ResizeMode.Horizontal)
				{
					int height = Height;
					height = ((DockStyle != PanelDockStyle.Top) ? (height + (_splitterStartLocation.Y - _splitterTrackBounds.Y)) : (height + (_splitterTrackBounds.Y - _splitterStartLocation.Y)));
					if (height < GetMinimumSizeWithSplitter().Height)
					{
						height = GetMinimumSizeWithSplitter().Height;
					}
					Height = height;
				}
				else if (_resizeMode == ResizeMode.Vertical)
				{
					int width = Width;
					width = ((DockStyle != PanelDockStyle.Left) ? (width + (_splitterStartLocation.X - _splitterTrackBounds.X)) : (width + (_splitterTrackBounds.X - _splitterStartLocation.X)));
					Width = width;
				}
				_splitterDragArea = Rectangle.Empty;
			}
			else
			{
				RenderingHelper.SetMouseCapture(this, false);
			}
			if (_dockMessageFilter != null)
			{
				_dockMessageFilter.DetachFromForm();
				try
				{
					Application.RemoveMessageFilter(_dockMessageFilter);
				}
				catch
				{
				}
				_dockMessageFilter = null;
			}
			_resizeMode = ResizeMode.NoResize;
			_splitterTrackBounds = Rectangle.Empty;
			try
			{
				PanelManager.RaiseComponentChanged("Size Panel", this);
			}
			catch
			{
			}
		}
	}
}
