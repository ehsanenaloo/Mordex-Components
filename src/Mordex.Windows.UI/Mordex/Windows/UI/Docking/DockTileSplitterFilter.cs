using System;
using System.Collections;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class DockTileSplitterFilter : DockPanelGroupState, IMessageFilter
	{
		internal struct SplitterInfo
		{
			internal Rectangle _bounds;

			internal DockPanelBase _firstPanel;

			internal DockPanelBase _secondPanel;

			internal SplitterInfo(Rectangle bounds, DockPanelBase firstPanel, DockPanelBase secondPanel)
			{
				_bounds = bounds;
				_firstPanel = firstPanel;
				_secondPanel = secondPanel;
			}
		}

		internal enum TileOrientation
		{
			Vertical,
			Horizontal
		}

		private const int WM_PAINT = 0x000F;

		private const int WM_KEYDOWN = 0x0100;

		private const int WM_NCLBUTTONUP = 0x00A2;

		private const int WM_NCRBUTTONUP = 0x00A5;

		private const int WM_NCMBUTTONUP = 0x00A8;

		private const int WM_NCXBUTTONUP = 0x00AC;

		private const int WM_RBUTTONUP = 0x0205;

		private const int WM_MBUTTONUP = 0x0208;

		private const int WM_XBUTTONUP = 0x020C;

		private const int VK_ESCAPE = 0x1B;

		private TileOrientation _tileOrientation;

		protected SplitterInfo[] _splitters;

		private int _activeSplitterIndex;

		private Rectangle _dragLimitBounds;

		private Rectangle _markerBounds;

		private Point _markerStartLocation;

		private bool _isMarkerHidden;

		internal override bool IsTabLayout => false;

		internal override bool HasCaption
		{
			get
			{
				if (_panelGroup.IsFloatingRoot())
				{
					return GetVisiblePanels().Count > 1;
				}
				return false;
			}
		}

		internal override Size MinimumSize
		{
			get
			{
				int width = 0;
				int height = -1;
				if (!HasVerticalSplitters())
				{
					width = -1;
					height = 0;
					foreach (DockPanelBase item in GetVisiblePanels())
					{
						Size minimumSize = item.GetMinimumSize();
						height += minimumSize.Height;
						if (width < minimumSize.Width)
						{
							width = minimumSize.Width;
						}
					}
				}
				else
				{
					foreach (DockPanelBase childPanel in GetVisiblePanels())
					{
						Size childMinimumSize = childPanel.GetMinimumSize();
						width += childMinimumSize.Width;
						if (height < childMinimumSize.Height)
						{
							height = childMinimumSize.Height;
						}
					}
				}
				width += (GetVisiblePanels().Count - 1) * _panelGroup.GetEffectiveSplitterSize();
				if (height > 0)
				{
					height += _panelGroup.GetGroupCaptionHeight();
				}
				return new Size(width, height);
			}
		}

		internal override Size MaximumSize
		{
			get
			{
				int width = 0;
				int height = 0;
				bool isBounded = GetVisiblePanels().Count > 0;
				foreach (DockPanelBase item in GetVisiblePanels())
				{
					Size maximumSize = item.GetMaximumSize();
					if (!isBounded)
					{
						continue;
					}
					if (_tileOrientation == TileOrientation.Horizontal)
					{
						if (maximumSize.Height == 0)
						{
							isBounded = false;
						}
						else
						{
							height += maximumSize.Height;
						}
					}
					else if (maximumSize.Width == 0)
					{
						isBounded = false;
					}
					else
					{
						width += maximumSize.Width;
					}
				}
				if (isBounded)
				{
					width += (GetVisiblePanels().Count - 1) * _panelGroup.GetEffectiveSplitterSize();
					if (height > 0)
					{
						height += _panelGroup.GetGroupCaptionHeight();
					}
					return new Size(width, height);
				}
				return new Size(0, 0);
			}
		}

		internal DockTileSplitterFilter(MxDockPanelGroup panelGroup, TileOrientation tileOrientation)
			: base(panelGroup)
		{
			_tileOrientation = tileOrientation;
			_activeSplitterIndex = -1;
			_splitters = null;
		}

		internal override bool ShouldDragGroup()
		{
			if (GetVisiblePanels().Count == 1 && _panelGroup._captionRenderer == null && _panelGroup.DockState == PanelDockState.Floating)
			{
				return true;
			}
			return false;
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case WM_KEYDOWN:
				if ((int)message.WParam == VK_ESCAPE)
				{
					EndSplitterDrag(0, 0, true);
				}
				break;
			case WM_PAINT:
				HideMarker();
				break;
			case WM_NCLBUTTONUP:
			case WM_NCRBUTTONUP:
			case WM_NCMBUTTONUP:
			case WM_NCXBUTTONUP:
			case WM_RBUTTONUP:
			case WM_MBUTTONUP:
			case WM_XBUTTONUP:
				return true;
			}
			return false;
		}

		private bool HasVerticalSplitters()
		{
			return _tileOrientation == TileOrientation.Vertical;
		}

		internal override bool ProcessMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				_activeSplitterIndex = HitTestSplitter(eventArgs.X, eventArgs.Y);
				if (_activeSplitterIndex != -1)
				{
					BeginSplitterDrag(eventArgs.X, eventArgs.Y);
					return false;
				}
			}
			return true;
		}

		internal override void ProcessChildDropInfo(PanelDropInfo dropInfo)
		{
			if (dropInfo._createsGroup && dropInfo._groupStyle == _panelGroup.GroupStyle)
			{
				dropInfo._createsGroup = false;
				dropInfo._insertIndex = dropInfo._targetPanel.GetIndexInParent() + dropInfo._insertIndex;
				dropInfo._targetPanel = _panelGroup;
			}
		}

		internal override bool ProcessMouseMove(MouseEventArgs eventArgs)
		{
			if (_activeSplitterIndex != -1)
			{
				UpdateDragMarker(eventArgs.X, eventArgs.Y);
				return false;
			}
			return true;
		}

		internal override bool ProcessMouseUp(MouseEventArgs eventArgs)
		{
			if (_activeSplitterIndex != -1)
			{
				EndSplitterDrag(eventArgs.X, eventArgs.Y, false);
				return false;
			}
			return true;
		}

		internal override void ProcessPaint(PaintEventArgs eventArgs)
		{
			if (_splitters == null)
			{
				return;
			}
			MxDockManager uiPanelManager = null;
			if (_panelGroup != null)
			{
				uiPanelManager = _panelGroup.PanelManager;
			}
			if (uiPanelManager != null)
			{
				DockManagerRendererBase renderer = uiPanelManager.GetPanelRenderer();
				SplitterInfo[] splitters = _splitters;
				for (int i = 0; i < splitters.Length; i++)
				{
					SplitterInfo splitter = splitters[i];
					renderer.DrawBackground(eventArgs.Graphics, _panelGroup, splitter._bounds);
				}
			}
		}

		internal override bool ProcessCaptionDoubleClick(DockPanelBase panel)
		{
			return false;
		}

		internal override void OnPanelResize(DockPanelBase panel)
		{
			if (!GetVisiblePanels().Contains(panel))
			{
				return;
			}
			if (GetVisiblePanels().Count < 1)
			{
				RefreshGroupLayout();
			}
			else
			{
				if (ContentBounds.IsEmpty)
				{
					return;
				}
				int totalSize = 0;
				foreach (DockPanelBase item in GetVisiblePanels())
				{
					totalSize += item.GetTileSize();
				}
				int availableSize = 0;
				availableSize = ((!HasVerticalSplitters()) ? ContentBounds.Height : ContentBounds.Width);
				availableSize -= _panelGroup.GetEffectiveSplitterSize() * (GetVisiblePanels().Count - 1);
				float scale = (float)availableSize / (float)totalSize;
				foreach (DockPanelBase scaledPanel in GetVisiblePanels())
				{
					scaledPanel.SetTileSize((int)((float)scaledPanel.GetTileSize() * scale));
				}
				totalSize = 0;
				foreach (DockPanelBase measuredPanel in GetVisiblePanels())
				{
					totalSize += measuredPanel.GetTileSize();
				}
				if (totalSize != availableSize)
				{
					panel.SetTileSize(panel.GetTileSize() - (totalSize - availableSize));
				}
				int panelIndex = GetVisiblePanels().IndexOf(panel);
				DockPanelBase neighbor = FindNeighbor(panelIndex, true);
				if (neighbor == null)
				{
					neighbor = FindNeighbor(panelIndex, false);
				}
				if (neighbor == null)
				{
					RefreshGroupLayout();
					return;
				}
				int panelSize = 0;
				panelSize = ((!HasVerticalSplitters()) ? panel.Height : panel.Width);
				totalSize = panel.GetTileSize() + neighbor.GetTileSize();
				panel.SetTileSize(panelSize);
				neighbor.SetTileSize(totalSize - panelSize);
				RefreshGroupLayout();
			}
		}

		private DockPanelBase FindNeighbor(int index, bool anyPanel)
		{
			int foundIndex = -1;
			for (int i = index + 1; i < GetVisiblePanels().Count; i++)
			{
				DockPanelBase uiPanelBase = (DockPanelBase)GetVisiblePanels()[i];
				if (anyPanel || uiPanelBase.CanResize())
				{
					foundIndex = i;
					break;
				}
			}
			if (foundIndex == -1)
			{
				for (int previousIndex = index - 1; previousIndex >= 0; previousIndex--)
				{
					DockPanelBase previousPanel = (DockPanelBase)GetVisiblePanels()[previousIndex];
					if (anyPanel || previousPanel.CanResize())
					{
						foundIndex = previousIndex;
						break;
					}
				}
			}
			if (foundIndex == -1)
			{
				return null;
			}
			return (DockPanelBase)GetVisiblePanels()[foundIndex];
		}

		internal override void RefreshGroupLayout()
		{
			if (GetVisiblePanels().Count == 0 || ContentBounds.Size.IsEmpty)
			{
				return;
			}
			UnhookPanelEvents();
			int[] sizes = new int[GetVisiblePanels().Count];
			_splitters = new SplitterInfo[GetVisiblePanels().Count - 1];
			int gapTotal = _splitters.Length * _panelGroup.GetEffectiveSplitterSize();
			int explicitTotal = 0;
			int minimumTotal = 0;
			int maximumTotal = 0;
			bool allBounded = true;
			int unsizedCount = 0;
			foreach (DockPanelBase item in GetVisiblePanels())
			{
				if (item.GetTileSize() <= -1)
				{
					unsizedCount++;
				}
				else
				{
					explicitTotal += item.GetTileSize();
				}
			}
			if (explicitTotal == 0)
			{
				explicitTotal = GetLengthAlongAxis(ContentBounds.Size) - gapTotal;
			}
			if (unsizedCount > 0)
			{
				int sizedCount = _panelGroup.Panels.Count - unsizedCount;
				int defaultSize = GetLengthAlongAxis(ContentBounds.Size) / GetVisiblePanels().Count;
				if (sizedCount > 0)
				{
					defaultSize = explicitTotal / sizedCount;
				}
				foreach (DockPanelBase unsizedPanel in (IEnumerable)_panelGroup.Panels)
				{
					if (unsizedPanel.GetTileSize() <= -1)
					{
						unsizedPanel.SetTileSize(defaultSize);
					}
				}
			}
			explicitTotal = 0;
			foreach (DockPanelBase measuredPanel in GetVisiblePanels())
			{
				int panelMinimum = GetLengthAlongAxis(measuredPanel.GetMinimumSize());
				if (panelMinimum < 0)
				{
					panelMinimum = 0;
				}
				minimumTotal += panelMinimum;
				if (allBounded)
				{
					int panelMaximum = GetLengthAlongAxis(measuredPanel.GetMaximumSize());
					if (panelMaximum <= 0)
					{
						allBounded = false;
					}
					else
					{
						maximumTotal += panelMaximum;
					}
				}
				explicitTotal += measuredPanel.GetTileSize();
			}
			int freeSpace = GetLengthAlongAxis(ContentBounds.Size) - gapTotal - minimumTotal;
			int distributed = 0;
			if (freeSpace < 0)
			{
				freeSpace = 0;
				for (int i = 0; i < GetVisiblePanels().Count; i++)
				{
					DockPanelBase minimumPanel = GetVisiblePanels()[i] as DockPanelBase;
					sizes[i] = GetLengthAlongAxis(minimumPanel.GetMinimumSize());
				}
			}
			else
			{
				for (int j = 0; j < GetVisiblePanels().Count; j++)
				{
					DockPanelBase currentPanel = (DockPanelBase)GetVisiblePanels()[j];
					sizes[j] = currentPanel.GetTileSize();
					int currentMinimum = GetLengthAlongAxis(currentPanel.GetMinimumSize());
					int currentMaximum = GetLengthAlongAxis(currentPanel.GetMaximumSize());
					if (currentMinimum < 0)
					{
						currentMinimum = 0;
					}
					int extraShare = ((explicitTotal > 0) ? ((int)Math.Floor((float)currentPanel.GetTileSize() / (float)explicitTotal * (float)(freeSpace + minimumTotal))) : 0);
					extraShare -= currentMinimum;
					if (extraShare < 0)
					{
						extraShare = 0;
					}
					if (currentMaximum > 0 && currentMaximum < currentMinimum + extraShare)
					{
						distributed += currentMaximum - currentMinimum;
						sizes[j] = currentMaximum;
					}
					else if (extraShare > 0)
					{
						sizes[j] = currentMinimum + extraShare;
						distributed += extraShare;
					}
					else
					{
						sizes[j] = currentMinimum;
					}
				}
				int cursor = 0;
				int lastDistributed = distributed;
				int step = 1;
				if (distributed > freeSpace)
				{
					step = -1;
					while (distributed != freeSpace)
					{
						if (cursor >= GetVisiblePanels().Count)
						{
							if (distributed == lastDistributed)
							{
								break;
							}
							lastDistributed = distributed;
							cursor = 0;
						}
						DockPanelBase shrinkPanel = (DockPanelBase)GetVisiblePanels()[cursor];
						if (shrinkPanel.CanResize())
						{
							int shrinkMinimum = GetLengthAlongAxis(shrinkPanel.GetMinimumSize());
							int shrunkSize = sizes[cursor] + step;
							if (shrunkSize >= shrinkMinimum)
							{
								sizes[cursor] = shrunkSize;
								distributed += step;
							}
						}
						cursor++;
					}
						sizes[FindLastResizableIndex()] += freeSpace - distributed;
				}
				else
				{
					step = 1;
					while (distributed != freeSpace)
					{
						if (cursor >= GetVisiblePanels().Count)
						{
							if (distributed == lastDistributed)
							{
								break;
							}
							lastDistributed = distributed;
							cursor = 0;
						}
						DockPanelBase growPanel = (DockPanelBase)GetVisiblePanels()[cursor];
						if (growPanel.CanResize())
						{
							int growMaximum = GetLengthAlongAxis(growPanel.GetMaximumSize());
							int growMinimum = GetLengthAlongAxis(growPanel.GetMinimumSize());
							int grownSize = sizes[cursor] + step;
							if (growMaximum <= 0)
							{
								if (grownSize >= growMinimum)
								{
									sizes[cursor] = grownSize;
									distributed += step;
								}
							}
							else if (grownSize <= growMaximum && grownSize >= growMinimum)
							{
								sizes[cursor] = grownSize;
								distributed += step;
							}
						}
						cursor++;
					}
						sizes[FindLastResizableIndex()] += freeSpace - distributed;
				}
			}
			int offset = ((!HasVerticalSplitters()) ? ContentBounds.Y : ContentBounds.X);
			for (int k = 0; k < GetVisiblePanels().Count; k++)
			{
				DockPanelBase placedPanel = (DockPanelBase)GetVisiblePanels()[k];
				if (HasVerticalSplitters())
				{
					placedPanel.Bounds = new Rectangle(offset, ContentBounds.Y, sizes[k], ContentBounds.Height);
				}
				else
				{
					placedPanel.Bounds = new Rectangle(ContentBounds.X, offset, ContentBounds.Width, sizes[k]);
				}
				offset += GetLengthAlongAxis(placedPanel.Size);
				if (k < _splitters.Length)
				{
					Rectangle bounds = ((!HasVerticalSplitters()) ? new Rectangle(ContentBounds.X, offset, ContentBounds.Width, _panelGroup.GetEffectiveSplitterSize()) : new Rectangle(offset, ContentBounds.Y, _panelGroup.GetEffectiveSplitterSize(), ContentBounds.Height));
					ref SplitterInfo reference = ref _splitters[k];
					reference = new SplitterInfo(bounds, placedPanel, (DockPanelBase)GetVisiblePanels()[k + 1]);
					offset += _panelGroup.GetEffectiveSplitterSize();
				}
			}
			HookPanelEvents();
		}

		private int FindLastResizableIndex()
		{
			for (int i = GetVisiblePanels().Count - 1; i >= 0; i--)
			{
				if (((DockPanelBase)GetVisiblePanels()[i]).CanResize())
				{
					return i;
				}
			}
			for (int j = GetVisiblePanels().Count - 1; j >= 0; j--)
			{
				DockPanelBase panel = (DockPanelBase)GetVisiblePanels()[j];
				if (panel.MinimumSize.Width <= 0 && panel.MinimumSize.Height <= 0 && panel.MaximumSize.Width <= 0 && panel.MaximumSize.Height <= 0)
				{
					return j;
				}
			}
			return GetVisiblePanels().Count - 1;
		}

		private int GetLengthAlongAxis(Size size)
		{
			if (HasVerticalSplitters())
			{
				return size.Width;
			}
			return size.Height;
		}

		internal override Cursor GetCursor(Point pt)
		{
			if (_splitters != null)
			{
				int splitterIndex = HitTestSplitter(pt.X, pt.Y);
				if (splitterIndex != -1)
				{
					if (HasVerticalSplitters())
					{
						return Cursors.VSplit;
					}
					return Cursors.HSplit;
				}
			}
			return base.GetCursor(pt);
		}

		protected int HitTestSplitter(int x, int y)
		{
			if (_splitters != null)
			{
				for (int i = 0; i < _splitters.Length; i++)
				{
					SplitterInfo splitter = _splitters[i];
					if (splitter._bounds.Contains(x, y) && splitter._firstPanel.CanResize() && splitter._secondPanel.CanResize())
					{
						return i;
					}
				}
			}
			return -1;
		}

		private void BeginSplitterDrag(int x, int y)
		{
			_activeSplitterIndex = HitTestSplitter(x, y);
			if (_activeSplitterIndex == -1)
			{
				return;
			}
			SplitterInfo splitter = _splitters[_activeSplitterIndex];
			if (!HasVerticalSplitters())
			{
				Rectangle rectangle = new Rectangle(ContentBounds.Left, splitter._firstPanel.Top, ContentBounds.Width, splitter._firstPanel.Height + splitter._secondPanel.Height);
				Rectangle upperLimit = rectangle;
				int height = splitter._firstPanel.GetMaximumSize().Height;
				if (height > 0 && height < upperLimit.Height)
				{
					upperLimit.Height = height;
				}
				int minimumHeight = splitter._firstPanel.GetMinimumSize().Height;
				if (minimumHeight > 0)
				{
					upperLimit.Y += minimumHeight;
					upperLimit.Height -= minimumHeight;
				}
				upperLimit.Height += splitter._bounds.Height;
				Rectangle lowerLimit = rectangle;
				height = splitter._secondPanel.GetMaximumSize().Height;
				if (height > 0 && height < lowerLimit.Height)
				{
					lowerLimit.Y = lowerLimit.Bottom - height;
					lowerLimit.Height = height;
				}
				minimumHeight = splitter._secondPanel.GetMinimumSize().Height;
				if (minimumHeight > 0)
				{
					lowerLimit.Height -= minimumHeight;
				}
				lowerLimit.Height += splitter._bounds.Height;
				_dragLimitBounds = Rectangle.Intersect(upperLimit, lowerLimit);
				if (_dragLimitBounds.IsEmpty)
				{
					_dragLimitBounds = splitter._bounds;
				}
			}
			else
			{
				Rectangle sideBySideArea = new Rectangle(splitter._firstPanel.Left, ContentBounds.Top, splitter._firstPanel.Width + splitter._secondPanel.Width, ContentBounds.Height);
				Rectangle leftLimit = sideBySideArea;
				int width = splitter._firstPanel.GetMaximumSize().Width;
				if (width > 0 && width < leftLimit.Width)
				{
					leftLimit.Width = width;
				}
				int minimumWidth = splitter._firstPanel.GetMinimumSize().Width;
				if (minimumWidth > 0)
				{
					leftLimit.X += minimumWidth;
					leftLimit.Width -= minimumWidth;
				}
				leftLimit.Width += splitter._bounds.Width;
				Rectangle rightLimit = sideBySideArea;
				width = splitter._secondPanel.GetMaximumSize().Width;
				if (width > 0 && width < rightLimit.Width)
				{
					rightLimit.X = rightLimit.Right - width;
					rightLimit.Width = width;
				}
				minimumWidth = splitter._secondPanel.GetMinimumSize().Width;
				if (minimumWidth > 0)
				{
					rightLimit.Width -= minimumWidth;
				}
				rightLimit.Width += splitter._bounds.Width;
				_dragLimitBounds = Rectangle.Intersect(leftLimit, rightLimit);
				if (_dragLimitBounds.IsEmpty)
				{
					_dragLimitBounds = splitter._bounds;
				}
			}
			_dragLimitBounds.Location = _panelGroup.PointToScreen(_dragLimitBounds.Location);
			_markerBounds = splitter._bounds;
			_markerBounds.Location = _panelGroup.PointToScreen(_markerBounds.Location);
			DockRenderer.ToggleDragFrame(_markerBounds, _panelGroup.GetEffectiveSplitterSize());
			_markerStartLocation = _markerBounds.Location;
			Application.AddMessageFilter(this);
		}

		private Rectangle GetMarkerBounds(int x, int y)
		{
			Point point = _panelGroup.PointToScreen(new Point(x, y));
			Rectangle empty = Rectangle.Empty;
			if (!HasVerticalSplitters())
			{
				int top = point.Y - _panelGroup.GetEffectiveSplitterSize() / 2;
				if (top < _dragLimitBounds.Y)
				{
					top = _dragLimitBounds.Y;
				}
				else if (top + _panelGroup.GetEffectiveSplitterSize() > _dragLimitBounds.Bottom)
				{
					top = _dragLimitBounds.Bottom - _panelGroup.GetEffectiveSplitterSize();
				}
				empty = new Rectangle(_dragLimitBounds.X, top, _dragLimitBounds.Width, _panelGroup.GetEffectiveSplitterSize());
			}
			else
			{
				int left = point.X - _panelGroup.GetEffectiveSplitterSize() / 2;
				if (left < _dragLimitBounds.Left)
				{
					left = _dragLimitBounds.Left;
				}
				else if (left + _panelGroup.GetEffectiveSplitterSize() > _dragLimitBounds.Right)
				{
					left = _dragLimitBounds.Right - _panelGroup.GetEffectiveSplitterSize();
				}
				empty = new Rectangle(left, _dragLimitBounds.Y, _panelGroup.GetEffectiveSplitterSize(), _dragLimitBounds.Height);
			}
			return empty;
		}

		private void UpdateDragMarker(int x, int y)
		{
			Rectangle rectangle = GetMarkerBounds(x, y);
			if (rectangle != _markerBounds)
			{
				if (!_isMarkerHidden)
				{
					DockRenderer.ToggleDragFrame(_markerBounds, _panelGroup.GetEffectiveSplitterSize());
				}
				_markerBounds = rectangle;
				DockRenderer.ToggleDragFrame(_markerBounds, _panelGroup.GetEffectiveSplitterSize());
				_isMarkerHidden = false;
			}
			else if (_isMarkerHidden)
			{
				DockRenderer.ToggleDragFrame(_markerBounds, _panelGroup.GetEffectiveSplitterSize());
				_isMarkerHidden = false;
			}
		}

		internal void HideMarker()
		{
			if (!_isMarkerHidden)
			{
				DockRenderer.ToggleDragFrame(_markerBounds, _panelGroup.GetEffectiveSplitterSize());
				_isMarkerHidden = true;
			}
		}

		internal void EndSplitterDrag(int x, int y, bool isCancelled)
		{
			HideMarker();
			if (!isCancelled)
			{
				SplitterInfo a = _splitters[_activeSplitterIndex];
				if (_panelGroup.PanelManager != null)
				{
					_panelGroup.PanelManager.BeginSizeTransaction();
				}
				UnhookPanelEvents();
				if (!HasVerticalSplitters())
				{
					int height = a._firstPanel.Height;
					height += _markerBounds.Y - _markerStartLocation.Y;
					a._firstPanel.Height = height;
					height = a._secondPanel.Height;
					height += _markerStartLocation.Y - _markerBounds.Y;
					a._secondPanel.Height = height;
					foreach (DockPanelBase item in GetVisiblePanels())
					{
						item.SetTileSize(item.Height);
					}
				}
				else
				{
					int width = a._firstPanel.Width;
					width += _markerBounds.X - _markerStartLocation.X;
					a._firstPanel.Width = width;
					width = a._secondPanel.Width;
					width += _markerStartLocation.X - _markerBounds.X;
					a._secondPanel.Width = width;
					foreach (DockPanelBase resizedPanel in GetVisiblePanels())
					{
						resizedPanel.SetTileSize(resizedPanel.Width);
					}
				}
				HookPanelEvents();
				RefreshGroupLayout();
				if (_panelGroup.PanelManager != null)
				{
					_panelGroup.PanelManager.EndSizeTransaction();
				}
			}
			else
			{
				RenderingHelper.SetMouseCapture(_panelGroup, false);
			}
			_dragLimitBounds = Rectangle.Empty;
			_activeSplitterIndex = -1;
			try
			{
				Application.RemoveMessageFilter(this);
			}
			catch
			{
			}
			if (_panelGroup.PanelManager != null && _panelGroup.PanelManager.ContainerControl != null)
			{
				_panelGroup.PanelManager.ContainerControl.Invalidate(invalidateChildren: true);
			}
			_markerBounds = Rectangle.Empty;
			_isMarkerHidden = false;
		}
	}
}
