using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxFloatingWindow
	{
		private bool _isWindowActive;

		private bool _hasThemedBack;

		private ContentAlignment _hotEdge = ContentAlignment.MiddleCenter;

		// Set while the window is the active window; the themed frame and caption read it.
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal bool IsWindowActive
		{
			get => _isWindowActive;
			set => SetWindowActive(value);
		}

		// The resize edge under the pointer, or MiddleCenter when there is none.
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal ContentAlignment HotEdge => _hotEdge;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal int CornerSize => SystemInformation.VerticalScrollBarWidth;

		private bool IsThemed => _manager != null;

		private void SetWindowActive(bool active)
		{
			if (_isWindowActive == active)
			{
				return;
			}
			_isWindowActive = active;
			if (IsThemed)
			{
				Invalidate();
			}
		}

		private void UpdateHotEdge(Point point)
		{
			ContentAlignment edge = ContentAlignment.MiddleCenter;
			if (_panel != null && IsThemed && _panel.CanResize())
			{
				edge = HitTestResizeEdge(point);
			}
			if (edge != _hotEdge)
			{
				_hotEdge = edge;
				Invalidate();
			}
		}

		// Called when the style of the dock manager or the default theme changes.
		internal void ApplyLook()
		{
			if (IsDisposed || _manager == null)
			{
				return;
			}
			if (IsThemed)
			{
				BackColor = _manager.GetDefaultColor(PanelColors.InnerContainer);
				_hasThemedBack = true;
			}
			else if (_hasThemedBack)
			{
				ResetBackColor();
				_hasThemedBack = false;
			}
			Invalidate();
		}
	}
}
