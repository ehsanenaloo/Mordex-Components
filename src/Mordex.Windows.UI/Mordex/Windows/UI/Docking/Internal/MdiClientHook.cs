using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class MdiClientHook : WindowSubclass
	{
		private const int GWL_STYLE = -16;

		private const int GWL_EXSTYLE = -20;

		private const int WS_BORDER = 0x800000;

		private const int WS_EX_CLIENTEDGE = 0x200;

		private const int WM_PAINT = 0x000F;

		private const int WM_NCPAINT = 0x0085;

		private const int WM_NCACTIVATE = 0x0086;

		private const int WM_MDISETMENU = 0x0230;

		private const int WM_MDIREFRESHMENU = 0x0234;

		private const string PanelManagerPropertyName = "Mordex_PanelManager";

		private static ArrayList _activeHooks;

		// True while the MdiClient styles in _integer and _dockRenderer2Integer are replaced and must be put back.
		private bool _stylesChanged;

		private MdiClient _mdiClient;

		private MdiTabGroupRenderer _tabRenderer;

		private int _originalStyle;

		private int _originalExStyle;

		private bool _usedByMenu;

		private bool _usedByTabs;

		private bool _isHooked;

		private static MdiClientHook FindHook(MdiClient mdiClient)
		{
			foreach (MdiClientHook item in _activeHooks)
			{
				if (item._mdiClient == mdiClient)
				{
					return item;
				}
			}
			return null;
		}

		internal static void AddMenuUser(MdiClient mdiClient)
		{
			if (_activeHooks == null)
			{
				_activeHooks = new ArrayList();
			}
			MdiClientHook existing = FindHook(mdiClient);
			if (existing != null)
			{
				existing._usedByMenu = true;
				return;
			}
			_activeHooks.Add(new MdiClientHook(null, mdiClient, menu: true, tabs: false));
		}

		internal static void RemoveMenuUser(MdiClient mdiClient)
		{
			if (_activeHooks == null || mdiClient == null)
			{
				return;
			}
			MdiClientHook hook = FindHook(mdiClient);
			if (hook != null)
			{
				hook._usedByMenu = false;
				if (!hook._usedByTabs)
				{
					hook.DetachClient();
					_activeHooks.Remove(hook);
				}
			}
		}

		internal static void AddTabUser(MdiTabGroupRenderer tabRenderer, MdiClient mdiClient)
		{
			if (_activeHooks == null)
			{
				_activeHooks = new ArrayList();
			}
			NativeMethods.SetPropW(mdiClient.Handle, PanelManagerPropertyName, new IntPtr(1));
			MdiClientHook existing = FindHook(mdiClient);
			if (existing != null)
			{
				existing.SetTabRenderer(tabRenderer);
				existing._usedByTabs = true;
				return;
			}
			_activeHooks.Add(new MdiClientHook(tabRenderer, mdiClient, menu: false, tabs: true));
		}

		internal static void RemoveTabUser(MdiClient mdiClient)
		{
			if (_activeHooks == null || mdiClient == null)
			{
				return;
			}
			NativeMethods.RemovePropW(mdiClient.Handle, PanelManagerPropertyName);
			MdiClientHook hook = FindHook(mdiClient);
			if (hook != null)
			{
				hook._usedByTabs = false;
				if (!hook._usedByMenu)
				{
					hook.DetachClient();
					_activeHooks.Remove(hook);
				}
				else
				{
					hook.SetTabRenderer(null);
				}
			}
		}

		private MdiClientHook(MdiTabGroupRenderer tabRenderer, MdiClient mdiClient, bool menu, bool tabs)
		{
			_usedByTabs = tabs;
			_usedByMenu = menu;
			mdiClient.Resize += OnMdiClientResize;
			_mdiClient = mdiClient;
			SetTabRenderer(tabRenderer);
			if (mdiClient.Visible)
			{
				_isHooked = true;
				AttachToWindow(mdiClient.Handle);
			}
			mdiClient.VisibleChanged += OnMdiClientVisibleChanged;
			mdiClient.HandleDestroyed += OnMdiClientHandleDestroyed;
			mdiClient.HandleCreated += OnMdiClientHandleCreated;
		}

		private void OnMdiClientVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (_mdiClient == null)
			{
				return;
			}
			if (_mdiClient.Visible)
			{
				if (!_isHooked)
				{
					AttachToWindow(_mdiClient.Handle);
					_isHooked = true;
				}
			}
			else
			{
				_isHooked = false;
				base.Detach();
			}
		}

		private void OnMdiClientHandleDestroyed(object sender, EventArgs eventArgs)
		{
			_isHooked = false;
			base.Detach();
		}

		private void OnMdiClientHandleCreated(object sender, EventArgs eventArgs)
		{
			_isHooked = true;
			AttachToWindow(_mdiClient.Handle);
		}

		private void SetTabRenderer(MdiTabGroupRenderer tabRenderer)
		{
			if (tabRenderer == _tabRenderer)
			{
				return;
			}
			_tabRenderer = tabRenderer;
			if (_mdiClient == null || _mdiClient.IsDisposed || !_mdiClient.IsHandleCreated)
			{
				return;
			}
			IntPtr handle = _mdiClient.Handle;
			if (_tabRenderer == null)
			{
				if (_stylesChanged)
				{
					NativeMethods.SetWindowLongPtr(handle, GWL_EXSTYLE, new IntPtr(_originalExStyle));
					NativeMethods.SetWindowLongPtr(handle, GWL_STYLE, new IntPtr(_originalStyle));
					_stylesChanged = false;
				}
			}
			else
			{
				int exStyle = NativeMethods.GetWindowLongPtr(handle, GWL_EXSTYLE).ToInt32();
				if ((exStyle & WS_EX_CLIENTEDGE) != 0 && !_stylesChanged)
				{
					_originalExStyle = exStyle;
					_originalStyle = NativeMethods.GetWindowLongPtr(handle, GWL_STYLE).ToInt32();
					_stylesChanged = true;
					NativeMethods.SetWindowLongPtr(handle, GWL_EXSTYLE, new IntPtr(exStyle & ~WS_EX_CLIENTEDGE));
					NativeMethods.SetWindowLongPtr(handle, GWL_STYLE, new IntPtr(_originalStyle | WS_BORDER));
				}
			}
			_mdiClient.Height--;
			_mdiClient.Refresh();
		}

		internal void DetachClient()
		{
			base.Detach();
			SetTabRenderer(null);
			_mdiClient.Resize -= OnMdiClientResize;
			_mdiClient.VisibleChanged -= OnMdiClientVisibleChanged;
			_mdiClient.HandleCreated -= OnMdiClientHandleCreated;
			_mdiClient.HandleDestroyed -= OnMdiClientHandleDestroyed;
			_mdiClient = null;
		}

		private void OnMdiClientResize(object sender, EventArgs eventArgs)
		{
			if (GetWindowHandle() != IntPtr.Zero && _usedByTabs && _mdiClient != null)
			{
				_mdiClient.Invalidate();
				PaintClientBorder();
			}
		}

		internal void PaintClientBorder()
		{
			if (_mdiClient != null && _tabRenderer != null)
			{
				Form form = RenderingHelper.FindForm(_mdiClient);
				if (form == null || form.IsHandleCreated)
				{
					IntPtr handle = _mdiClient.Handle;
					IntPtr windowDc = NativeMethods.GetWindowDC(handle);
					if (windowDc == IntPtr.Zero)
					{
						return;
					}
					try
					{
						using (Graphics graphics = RenderingHelper.CreateGraphicsFromHdc(windowDc))
						{
							Rectangle bounds = _mdiClient.Bounds;
							bounds.Offset(-bounds.Left, -bounds.Top);
							_tabRenderer.GetManager().GetMdiRenderer().DrawPanelBorder(graphics, null, bounds);
						}
					}
					finally
					{
						NativeMethods.ReleaseDC(handle, windowDc);
					}
				}
			}
		}

		protected override IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			if (msg == WM_MDISETMENU || msg == WM_MDIREFRESHMENU)
			{
				if (_usedByMenu)
				{
					return IntPtr.Zero;
				}
				Control control = RenderingHelper.GetParentControl(_mdiClient);
				return base.WndProc(control.Handle, msg, wParam, lParam);
			}
			if (_usedByTabs)
			{
				switch (msg)
				{
				case WM_NCPAINT:
				case WM_NCACTIVATE:
					PaintClientBorder();
					return IntPtr.Zero;
				case WM_PAINT:
				{
					MdiTabGroupRenderer renderer = _tabRenderer;
					MdiClient mdiClient = _mdiClient;
					if (renderer == null || mdiClient == null)
					{
						break;
					}
					NativeMethods.NativePaintStruct paintStruct = default(NativeMethods.NativePaintStruct);
					IntPtr paintDc = NativeMethods.BeginPaint(hWnd, ref paintStruct);
					try
					{
						using (Graphics graphics = RenderingHelper.CreateGraphicsFromHdc(paintDc))
						{
							Rectangle clientRectangle = mdiClient.ClientRectangle;
							Image backgroundImage = renderer.GetMdiClient().BackgroundImage;
							if (backgroundImage != null)
							{
								Form form = renderer.GetForm();
								if (form != null)
								{
									switch (form.BackgroundImageLayout)
									{
									case ImageLayout.Center:
										RenderingHelper.DrawCenteredImage(graphics, backgroundImage, clientRectangle);
										break;
									case ImageLayout.Stretch:
									case ImageLayout.Zoom:
										RenderingHelper.DrawStretchedImage(graphics, backgroundImage, clientRectangle);
										break;
									case ImageLayout.Tile:
										RenderingHelper.DrawTiledBackgroundImage(graphics, backgroundImage, clientRectangle);
										break;
									case ImageLayout.None:
										RenderingHelper.DrawTiledImage(graphics, backgroundImage, clientRectangle);
										break;
									}
								}
								else
								{
									RenderingHelper.DrawTiledImage(graphics, mdiClient.BackgroundImage, clientRectangle);
								}
							}
							else
							{
								renderer.RenderContent(graphics, clientRectangle);
							}
						}
					}
					finally
					{
						NativeMethods.EndPaint(hWnd, ref paintStruct);
					}
					return IntPtr.Zero;
				}
				}
			}
			return base.WndProc(hWnd, msg, wParam, lParam);
		}
	}
}
