using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class DropTargetWindowDialog : Form
	{
		private const int WS_POPUP = int.MinValue;

		private const int WS_EX_NOACTIVATE = 0x08000000;

		private const int WM_MOUSEACTIVATE = 0x0021;

		private const int MA_NOACTIVATE = 3;

		private const int SW_SHOWNORMAL = 1;

		private const int SW_SHOWNOACTIVATE = 4;

		private const double ModernTargetOpacity = 0.85;

		private const double PreviewOpacity = 0.39;

		private static readonly Size DefaultClientSize = new Size(173, 128);

		private MxDockManager _panelManager;

		private DropTargetType _targetType;

		private Image _targetImage;

		private bool _allowTab;

		private GraphicsPath _previewPath;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Parent = IntPtr.Zero;
				createParams.Style = WS_POPUP;
				if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				{
					createParams.ExStyle += WS_EX_NOACTIVATE;
				}
				else if (Environment.OSVersion.Version.Major > 4)
				{
					createParams.ExStyle += WS_EX_NOACTIVATE;
				}
				return createParams;
			}
		}

		internal DropTargetWindowDialog(MxDockManager panelManager, DropTargetType windowType, bool allowTab)
		{
			base.TransparencyKey = Color.Magenta;
			BackColor = Color.Magenta;
			ConfigureWindow();
			_allowTab = allowTab;
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			_panelManager = panelManager;
			_targetType = windowType;
			switch (_targetType)
			{
			case DropTargetType.Bottom:
				SetTargetImage(true, WindowsResources.Vs2010DropTargetBottom, WindowsResources.DropTargetBottom);
				break;
			case DropTargetType.Left:
				SetTargetImage(true, WindowsResources.Vs2010DropTargetLeft, WindowsResources.DropTargetLeft);
				break;
			case DropTargetType.Right:
				SetTargetImage(true, WindowsResources.Vs2010DropTargetRight, WindowsResources.DropTargetRight);
				break;
			case DropTargetType.Top:
				SetTargetImage(true, WindowsResources.Vs2010DropTargetBottom, WindowsResources.DropTargetBottom);
				break;
			case DropTargetType.All:
				if (allowTab)
				{
					SetTargetImage(true, WindowsResources.Vs2010DropTargetAll, WindowsResources.DropTargetAll);
				}
				else
				{
					SetTargetImage(true, WindowsResources.Vs2010DropTargetStatic, WindowsResources.DropTargetStatic);
				}
				break;
			case DropTargetType.Preview:
				base.TopMost = false;
				base.Opacity = PreviewOpacity;
				break;
			}
		}

		internal DropTargetWindowDialog(MxDockManager panelManager, DropTargetType windowType)
			: this(panelManager, windowType, allowTab: true)
		{
		}

		private void SetTargetImage(bool isModernStyle, Func<Bitmap> modernBitmap, Func<Bitmap> classicBitmap)
		{
			if (isModernStyle)
			{
				_targetImage = RenderingHelper.MakeMagentaTransparent(modernBitmap());
				base.Opacity = ModernTargetOpacity;
			}
			else
			{
				_targetImage = RenderingHelper.MakeMagentaTransparent(classicBitmap());
			}
		}

		public void SetAllowTab(bool allowTab)
		{
			if (_allowTab != allowTab)
			{
				_allowTab = allowTab;
				if (_allowTab)
				{
					_targetImage = RenderingHelper.MakeMagentaTransparent(WindowsResources.DropTargetAll());
				}
				else
				{
					_targetImage = RenderingHelper.MakeMagentaTransparent(WindowsResources.DropTargetStatic());
				}
				Invalidate();
			}
		}

		public void SetPreviewPath(GraphicsPath graphicsPath)
		{
			// The dialog owns the path it is given and frees the one it replaces.
			if (_previewPath != null && _previewPath != graphicsPath)
			{
				_previewPath.Dispose();
			}
			_previewPath = graphicsPath;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _previewPath != null)
			{
				_previewPath.Dispose();
				_previewPath = null;
			}
			base.Dispose(disposing);
		}

		// The target image is a 3x3 grid. The edge cells dock to that side and the middle cell tabs into the panel.
		internal PanelDropInfo GetCellDropInfo(DockPanelBase panel, Point point)
		{
			Point pt = PointToClient(point);
			int cellWidth = base.Width / 3;
			int cellHeight = base.Height / 3;
			if (new Rectangle(cellWidth, 0, cellWidth, cellHeight).Contains(pt))
			{
				return _panelManager.GetFillAreaEdgeDropInfo(panel, PanelDockStyle.Top);
			}
			if (new Rectangle(0, cellHeight, cellWidth, cellHeight).Contains(pt))
			{
				return _panelManager.GetFillAreaEdgeDropInfo(panel, PanelDockStyle.Left);
			}
			Rectangle bottomCell = new Rectangle(cellWidth, base.ClientRectangle.Bottom - cellHeight, cellWidth, cellHeight);
			if (bottomCell.Contains(pt))
			{
				return _panelManager.GetFillAreaEdgeDropInfo(panel, PanelDockStyle.Bottom);
			}
			Rectangle rightCell = new Rectangle(base.ClientRectangle.Right - cellWidth, cellHeight, cellWidth, cellHeight);
			if (rightCell.Contains(pt))
			{
				return _panelManager.GetFillAreaEdgeDropInfo(panel, PanelDockStyle.Right);
			}
			Rectangle centerCell = new Rectangle(cellWidth, cellHeight, cellWidth, cellHeight);
			if (_allowTab && centerCell.Contains(pt))
			{
				if (_panelManager.GetDragHoverPanel() != null)
				{
					DockPanelBase targetPanel = _panelManager.GetDragHoverPanel();
					PanelDropInfo panelDropInfo = targetPanel.GetTabDropInfo();
					if (_panelManager.GetDragHoverPanel().ParentGroup != null)
					{
						MxDockPanelGroup parentGroup = _panelManager.GetDragHoverPanel().ParentGroup;
						if (parentGroup.Panels.Contains(panelDropInfo._targetPanel))
						{
							parentGroup._groupState.ProcessChildDropInfo(panelDropInfo);
						}
					}
					return panelDropInfo;
				}
				Rectangle screenBounds = _panelManager.GetFillArea();
				screenBounds.Location = _panelManager.ContainerControl.PointToScreen(screenBounds.Location);
				PanelDropInfo fillDropInfo = new PanelDropInfo();
				fillDropInfo._createsGroup = false;
				fillDropInfo._panelDockStyle = PanelDockStyle.Fill;
				fillDropInfo._dropBounds = screenBounds;
				return fillDropInfo;
			}
			return null;
		}

		internal PanelDropInfo GetDropInfo(DockPanelBase panel, Point pt)
		{
			if (base.Bounds.Contains(pt))
			{
				switch (_targetType)
				{
				case DropTargetType.All:
					return GetCellDropInfo(panel, pt);
				case DropTargetType.Left:
					return _panelManager.GetContainerEdgeDropInfo(panel, PanelDockStyle.Left);
				case DropTargetType.Top:
					return _panelManager.GetContainerEdgeDropInfo(panel, PanelDockStyle.Top);
				case DropTargetType.Right:
					return _panelManager.GetContainerEdgeDropInfo(panel, PanelDockStyle.Right);
				case DropTargetType.Bottom:
					return _panelManager.GetContainerEdgeDropInfo(panel, PanelDockStyle.Bottom);
				}
			}
			return null;
		}

		protected override void WndProc(ref Message message)
		{
			if (message.Msg == WM_MOUSEACTIVATE)
			{
				message.Result = (IntPtr)MA_NOACTIVATE;
			}
			base.WndProc(ref message);
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			eventArgs.Graphics.FillRectangle(Brushes.Magenta, base.ClientRectangle);
			switch (_targetType)
			{
			case DropTargetType.All:
			case DropTargetType.Left:
			case DropTargetType.Top:
			case DropTargetType.Right:
			case DropTargetType.Bottom:
				eventArgs.Graphics.DrawImageUnscaled(_targetImage, 0, 0);
				break;
			case DropTargetType.Preview:
			{
				bool themed = _panelManager != null;
				Brush brush = themed ? MxThemeManager.ResolveDefault(DeviceDpi).Brush(ThemeRole.Accent) : new SolidBrush(SystemColors.Highlight);
				if (_previewPath == null)
				{
					eventArgs.Graphics.FillRectangle(brush, base.ClientRectangle);
				}
				else
				{
					eventArgs.Graphics.FillPath(brush, _previewPath);
				}
				if (!themed)
				{
					brush.Dispose();
				}
				break;
			}
			}
		}

		internal void ShowWithoutActivating()
		{
			NativeMethods.ShowWindow(base.Handle, SW_SHOWNOACTIVATE);
			if (!base.Visible)
			{
				NativeMethods.ShowWindow(base.Handle, SW_SHOWNORMAL);
			}
			if (!base.Visible)
			{
				Show();
			}
			Update();
		}

		private void ConfigureWindow()
		{
			SuspendLayout();
			base.ClientSize = DefaultClientSize;
			base.ControlBox = false;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Name = "DropTargetWindow";
			base.StartPosition = FormStartPosition.Manual;
			base.ShowInTaskbar = false;
			Text = "DropTargetWindow";
			base.TopMost = true;
			ResumeLayout(performLayout: false);
		}
	}
}
