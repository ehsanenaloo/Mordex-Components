using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Docking
{
	[ToolboxItem(false)]
	public class SplitterMdi : Control
	{
		// Space between a tab group's bottom/right edge and the next group (strip border and padding).
		private const int GroupGap = 4;

		private MxMdiTabGroup _tabGroup;

		private bool _isDragging;

		private Point _minCursorPosition;

		private Point _maxCursorPosition;

		private Rectangle _dragBounds;

		public override Cursor Cursor
		{
			get
			{
				if (_tabGroup.PanelManager.TabbedMdiSettings.GroupOrientation == MdiGroupOrientation.Horizontal)
				{
					return Cursors.HSplit;
				}
				return Cursors.VSplit;
			}
			set
			{
				base.Cursor = value;
			}
		}

		internal SplitterMdi(MxMdiTabGroup tabGroup)
		{
			_tabGroup = tabGroup;
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		}

		private MdiGroupOrientation GetGroupOrientation()
		{
			return _tabGroup.PanelManager.TabbedMdiSettings.GroupOrientation;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			_tabGroup.PanelManager.GetPanelRenderer().DrawBackground(e.Graphics, this, base.ClientRectangle);
			Pen pen = GdiCache.Pen(_tabGroup.PanelManager.TabbedMdiSettings.BorderColor);
			if (GetGroupOrientation() == MdiGroupOrientation.Horizontal)
			{
				e.Graphics.DrawLine(pen, base.ClientRectangle.X, base.ClientRectangle.Top, base.ClientRectangle.Right, base.ClientRectangle.Top);
				e.Graphics.DrawLine(pen, base.ClientRectangle.X, base.ClientRectangle.Bottom - 1, base.ClientRectangle.Right, base.ClientRectangle.Bottom - 1);
			}
			else
			{
				e.Graphics.DrawLine(pen, base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.X, base.ClientRectangle.Bottom);
				e.Graphics.DrawLine(pen, base.ClientRectangle.Right - 1, base.ClientRectangle.Y, base.ClientRectangle.Right - 1, base.ClientRectangle.Bottom);
			}
			base.OnPaint(e);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				_isDragging = true;
				Rectangle clientRectangle = base.ClientRectangle;
				if (GetGroupOrientation() == MdiGroupOrientation.Horizontal)
				{
					_minCursorPosition = new Point(0, _tabGroup.Height + GroupGap);
					_minCursorPosition = _tabGroup.PointToScreen(_minCursorPosition);
					MxMdiTabGroup nextGroup = (MxMdiTabGroup)_tabGroup.GetTabGroupHost().GetGroups()[_tabGroup.GroupIndex + 1];
					_maxCursorPosition = new Point(0, nextGroup.GetGroupBounds().Bottom - (nextGroup.Height + GroupGap + base.Height));
					_maxCursorPosition = nextGroup.GetTabGroupHost().GetMdiClient().PointToScreen(_maxCursorPosition);
					clientRectangle.Inflate(0, -1);
				}
				else
				{
					_minCursorPosition = new Point(_tabGroup.Height + GroupGap, 0);
					_minCursorPosition = _tabGroup.PointToScreen(_minCursorPosition);
					MxMdiTabGroup nextGroup = (MxMdiTabGroup)_tabGroup.GetTabGroupHost().GetGroups()[_tabGroup.GroupIndex + 1];
					_maxCursorPosition = new Point(nextGroup.GetGroupBounds().Right - (nextGroup.Height + GroupGap + base.Width));
					_maxCursorPosition = nextGroup.GetTabGroupHost().GetMdiClient().PointToScreen(_maxCursorPosition);
					clientRectangle.Inflate(-1, 0);
				}
				clientRectangle.Location = PointToScreen(clientRectangle.Location);
				SetDragBounds(clientRectangle);
			}
			base.OnMouseDown(e);
		}

		private void SetDragBounds(Rectangle rectangle)
		{
			if (_dragBounds != rectangle)
			{
				if (!_dragBounds.IsEmpty)
				{
					RenderingHelper.InvertDragFrames(new Rectangle[1] { _dragBounds });
				}
				_dragBounds = rectangle;
				if (!_dragBounds.IsEmpty)
				{
					RenderingHelper.InvertDragFrames(new Rectangle[1] { _dragBounds });
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_isDragging)
			{
				if (GetGroupOrientation() == MdiGroupOrientation.Horizontal)
				{
					int y = Cursor.Position.Y;
					if (y < _minCursorPosition.Y)
					{
						y = _minCursorPosition.Y;
					}
					else if (y > _maxCursorPosition.Y)
					{
						y = _maxCursorPosition.Y;
					}
					y -= (base.ClientRectangle.Height - 2) / 2;
					Rectangle clientRectangle = base.ClientRectangle;
					clientRectangle.Inflate(0, -1);
					clientRectangle.Location = PointToScreen(clientRectangle.Location);
					clientRectangle.Y = y;
					SetDragBounds(clientRectangle);
				}
				else
				{
					int x = Cursor.Position.X;
					if (x < _minCursorPosition.X)
					{
						x = _minCursorPosition.X;
					}
					else if (x > _maxCursorPosition.X)
					{
						x = _maxCursorPosition.X;
					}
					x -= (base.ClientRectangle.Width - 2) / 2;
					Rectangle splitterBounds = base.ClientRectangle;
					splitterBounds.Inflate(-1, 0);
					splitterBounds.Location = PointToScreen(splitterBounds.Location);
					splitterBounds.X = x;
					SetDragBounds(splitterBounds);
				}
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (_isDragging)
			{
				MxMdiTabGroup nextGroup = (MxMdiTabGroup)_tabGroup.GetTabGroupHost().GetGroups()[_tabGroup.GroupIndex + 1];
				Point location = _tabGroup.Location;
				location = RenderingHelper.GetParentControl(this).PointToScreen(location);
				if (GetGroupOrientation() == MdiGroupOrientation.Horizontal)
				{
					int splitOffsetY = _dragBounds.Y - location.Y;
					float newSizeY = _tabGroup._relativeSize * ((float)splitOffsetY / (float)_tabGroup.GetGroupBounds().Height);
					float nextSizeY = nextGroup._relativeSize + (_tabGroup._relativeSize - newSizeY);
					_tabGroup._relativeSize = newSizeY;
					nextGroup._relativeSize = nextSizeY;
				}
				else
				{
					int splitOffsetX = _dragBounds.X - location.X;
					float newSizeX = _tabGroup._relativeSize * ((float)splitOffsetX / (float)_tabGroup.GetGroupBounds().Width);
					float nextSizeX = nextGroup._relativeSize + (_tabGroup._relativeSize - newSizeX);
					_tabGroup._relativeSize = newSizeX;
					nextGroup._relativeSize = nextSizeX;
				}
				SetDragBounds(Rectangle.Empty);
				_isDragging = false;
				_tabGroup.GetTabGroupHost().LayoutGroups();
			}
			base.OnMouseUp(e);
		}
	}
}
