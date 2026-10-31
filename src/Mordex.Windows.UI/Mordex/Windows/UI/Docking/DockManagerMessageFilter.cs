using System.Drawing;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class DockManagerMessageFilter : IMessageFilter
	{
		private const int WM_SETREDRAW = 0x000B;

		private const int WM_PAINT = 0x000F;

		private const int WM_SETCURSOR = 0x0020;

		private const int WM_NCLBUTTONUP = 0x00A2;

		private const int WM_NCRBUTTONUP = 0x00A5;

		private const int WM_NCMBUTTONUP = 0x00A8;

		private const int WM_NCXBUTTONUP = 0x00AC;

		private const int WM_KEYDOWN = 0x0100;

		private const int WM_MOUSEMOVE = 0x0200;

		private const int WM_LBUTTONUP = 0x0202;

		private const int WM_RBUTTONUP = 0x0205;

		private const int WM_MBUTTONUP = 0x0208;

		private const int WM_XBUTTONUP = 0x020C;

		private const int VK_ESCAPE = 0x1B;

		private const int DropMarkerInset = 4;

		private const int DropMarkerHeight = 8;

		private MxDockManager _dockManager;

		internal Rectangle _dropAreaBounds;

		internal Rectangle _dropTabBounds;

		internal DockPanelBase _draggedPanel;

		internal Point _offset;

		internal DockManagerMessageFilter(MxDockManager manager, DockPanelBase panel, Point offset)
		{
			_dockManager = manager;
			_draggedPanel = panel;
			_offset = offset;
			_dropAreaBounds = Rectangle.Empty;
		}

		internal MxDockManager GetDockManager()
		{
			return _dockManager;
		}

		internal Point GetOffset()
		{
			return _offset;
		}

		internal void SetOffset(Point offset)
		{
			_offset = offset;
		}

		internal virtual void StartDrag()
		{
			Application.AddMessageFilter(this);
			ContinueDrag();
		}

		protected virtual void ContinueDrag()
		{
			PanelDropInfo panelDropInfo = GetDockManager().QueryStandardDropInfo(_draggedPanel, Cursor.Position);
			if (panelDropInfo != null && panelDropInfo._isAllowed)
			{
				if (_dropAreaBounds != panelDropInfo._dropBounds || _dropTabBounds != panelDropInfo._tabHeaderBounds)
				{
					RenderingHelper.InvertDragFrames(GetDropFrames());
					_dropAreaBounds = panelDropInfo._dropBounds;
					_dropTabBounds = panelDropInfo._tabHeaderBounds;
					RenderingHelper.InvertDragFrames(GetDropFrames());
				}
			}
			else
			{
				ClearDropFrames();
			}
		}

		private Rectangle[] GetDropFrames()
		{
			if (!_dropTabBounds.IsEmpty)
			{
				Rectangle rectangle = new Rectangle(_dropTabBounds.X + DropMarkerInset, _dropTabBounds.Y - DropMarkerInset, _dropTabBounds.Width - 2 * DropMarkerInset, DropMarkerHeight);
				return new Rectangle[3] { _dropAreaBounds, _dropTabBounds, rectangle };
			}
			return new Rectangle[1] { _dropAreaBounds };
		}

		private void ClearDropFrames()
		{
			if (!_dropAreaBounds.IsEmpty)
			{
				RenderingHelper.InvertDragFrames(GetDropFrames());
				_dropAreaBounds = Rectangle.Empty;
				_dropTabBounds = Rectangle.Empty;
			}
		}

		internal virtual void EndDrag(bool cancel)
		{
			if (!_dropAreaBounds.IsEmpty)
			{
				RenderingHelper.InvertDragFrames(GetDropFrames());
				_dropAreaBounds = Rectangle.Empty;
				_dropTabBounds = Rectangle.Empty;
			}
			if (!cancel)
			{
				Point position = Cursor.Position;
				PanelDropInfo panelDropInfo = _dockManager.QueryStandardDropInfo(_draggedPanel, position);
				if (panelDropInfo != null)
				{
					_dockManager.EndPanelDrag(_draggedPanel, panelDropInfo);
				}
			}
			else
			{
				_dockManager.ClearDragFilter();
			}
			try
			{
				Application.RemoveMessageFilter(this);
			}
			catch
			{
			}
			for (Control control = _draggedPanel; control != null; control = RenderingHelper.GetParentControl(control))
			{
				if (control.Capture)
				{
					RenderingHelper.SetMouseCapture(control, false);
					break;
				}
			}
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case WM_LBUTTONUP:
				EndDrag(cancel: false);
				return false;
			case WM_KEYDOWN:
				if ((int)message.WParam == VK_ESCAPE)
				{
					EndDrag(cancel: true);
				}
				else
				{
					ContinueDrag();
				}
				return true;
			case WM_SETREDRAW:
			case WM_PAINT:
				ClearDropFrames();
				break;
			case WM_SETCURSOR:
			case WM_NCLBUTTONUP:
			case WM_NCRBUTTONUP:
			case WM_NCMBUTTONUP:
			case WM_NCXBUTTONUP:
			case WM_RBUTTONUP:
			case WM_MBUTTONUP:
			case WM_XBUTTONUP:
				return true;
			case WM_MOUSEMOVE:
				ContinueDrag();
				return true;
			}
			return false;
		}
	}
}
