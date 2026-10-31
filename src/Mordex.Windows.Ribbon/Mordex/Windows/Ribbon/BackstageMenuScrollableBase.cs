using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class BackstageMenuScrollableBase : ScrollableBase, ICommandRenderHost
	{
		private const int WsChildClipChildren = 0x42000000;

		private const int WsExNoActivate = 0x08000000;

		private const int WmMouseActivate = 33;

		private const int WmMouseWheel = 522;

		private const int MaNoActivate = 3;

		private BackstageMenu _backstageMenu;

		private MxRibbon _ribbon;

		internal BackstageMenuRenderer _backstageMenuRenderer;

		private ICommandRenderHost _renderHost;

		private BackstageTab _backstageTab;

		private CommandBaseRenderer _selectedRenderer;

		internal Rectangle _anchorBounds;

		internal Panel _tabPagePanel;

		private bool _canForwardMouseWheel = true;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Style = WsChildClipChildren;
				createParams.ExStyle = WsExNoActivate;
				return createParams;
			}
		}

		AccessibleObject ICommandRenderHost.AccessibilityObject => base.AccessibilityObject;

		Control ICommandRenderHost.ParentControl => this;

		int ICommandRenderHost.SmallItemHeight => _renderHost.SmallItemHeight;

		Size ICommandRenderHost.SmallImageSize => _renderHost.SmallImageSize;

		Size ICommandRenderHost.LargeImageSize => _renderHost.LargeImageSize;

		Font ICommandRenderHost.Font => _renderHost.Font;

		RibbonColorScheme ICommandRenderHost.ColorScheme => _renderHost.ColorScheme;


		CommandBaseRenderer ICommandRenderHost.FocusItem
		{
			get
			{
				return ((CommandContainerRenderer)_backstageMenuRenderer).GetHotRenderer();
			}
			set
			{
				((CommandContainerRenderer)_backstageMenuRenderer).SetHotRenderer(value);
			}
		}

		CommandBaseRenderer ICommandRenderHost.MouseDownItem
		{
			get
			{
				if (_renderHost != null)
				{
					return _renderHost.MouseDownItem;
				}
				return null;
			}
			set
			{
				_renderHost.MouseDownItem = value;
			}
		}

		int ICommandRenderHost.MaxItemRows => _renderHost.MaxItemRows;

		CommandBaseRenderer ICommandRenderHost.ActiveInputItem
		{
			get
			{
				return _renderHost.ActiveInputItem;
			}
			set
			{
				_renderHost.ActiveInputItem = value;
			}
		}

		ISite ICommandRenderHost.Site => _renderHost.Site;

		bool ICommandRenderHost.UseCompatibleTextRendering => _renderHost.UseCompatibleTextRendering;

		RightToLeft ICommandRenderHost.RightToLeft => _renderHost.RightToLeft;

		bool ICommandRenderHost.IsInDesignMode => _renderHost.IsInDesignMode;

		CommandBaseEditorDialog ICommandRenderHost.ChildMenu
		{
			get
			{
				return _ribbon.GetPopupMenu();
			}
			set
			{
				_ribbon.SetPopupMenu(value);
			}
		}

		CommandCollection ICommandRenderHost.Commands => _backstageMenu.Commands;

		Form ICommandRenderHost.ParentForm => _renderHost.ParentForm;

		CommandBaseRenderer ICommandRenderHost.SelectedItem
		{
			get
			{
				return GetSelectedRenderer();
			}
			set
			{
				SetSelectedRenderer(value);
			}
		}

		bool ICommandRenderHost.ShowToolTips => _renderHost.ShowToolTips;

		MxRibbon ICommandRenderHost.Ribbon => _ribbon;

		public BackstageMenuScrollableBase(MxRibbon ribbon, BackstageMenu backStageMenu)
		{
			_ribbon = ribbon;
			_renderHost = ribbon;
			_backstageMenu = backStageMenu;
			_backstageMenuRenderer = new BackstageMenuRenderer(backStageMenu, this);
			_tabPagePanel = new Panel();
			base.Controls.Add(_tabPagePanel);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			BackColor = _ribbon._colorScheme.Theme.Color(ThemeRole.Surface);
			_tabPagePanel.BackColor = BackColor;
			base.TabStop = true;
			base.MinSize = new Size(400, 300);
		}

		public BackstageMenu GetBackstageMenu()
		{
			return _backstageMenu;
		}

		public BackstageTab GetBackstageTab()
		{
			return _backstageTab;
		}

		public void SetBackstageTab(BackstageTab backstageTab)
		{
			if (_backstageTab == backstageTab)
			{
				return;
			}
			if (_backstageTab != null)
			{
				_backstageTab.Selected = false;
				if (_tabPagePanel.Controls.Contains(_backstageTab.TabPage))
				{
					_tabPagePanel.Controls.Remove(_backstageTab.TabPage);
				}
			}
			_backstageTab = backstageTab;
			if (_backstageTab != null)
			{
				_backstageTab.Selected = true;
				if (!_tabPagePanel.Controls.Contains(_backstageTab.TabPage))
				{
					_tabPagePanel.Controls.Add(_backstageTab.TabPage);
				}
			}
		}

		internal void EnsureItemSelected()
		{
			if (_backstageMenuRenderer != null && ((CommandContainerRenderer)_backstageMenuRenderer).GetHotRenderer() == null)
			{
				if (_backstageMenuRenderer._childRenderers.Count > 0)
				{
					((CommandContainerRenderer)_backstageMenuRenderer).SetHotRenderer(_backstageMenuRenderer._childRenderers[0]);
				}
				else
				{
					_backstageMenuRenderer._selectFirstOnLayout = true;
				}
			}
		}

		private CommandBaseRenderer GetSelectedRenderer()
		{
			return _selectedRenderer;
		}

		private void SetSelectedRenderer(CommandBaseRenderer commandBaseRenderer)
		{
			if (_selectedRenderer != commandBaseRenderer)
			{
				if (_selectedRenderer != null)
				{
					_selectedRenderer.Selected = false;
				}
				_selectedRenderer = commandBaseRenderer;
				if (_selectedRenderer != null)
				{
					_selectedRenderer.Selected = true;
				}
				if (commandBaseRenderer is BackstageTabRenderer)
				{
					SetBackstageTab(((BackstageTabRenderer)commandBaseRenderer).GetBackStageTab());
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// The tab page belongs to the tab, so it must outlive this control.
				if (_backstageTab != null && _backstageTab.TabPage != null && _tabPagePanel.Controls.Contains(_backstageTab.TabPage))
				{
					_tabPagePanel.Controls.Remove(_backstageTab.TabPage);
				}
				if (_backstageMenuRenderer != null)
				{
					_backstageMenuRenderer.Dispose();
				}
			}
			_ribbon = null;
			base.Dispose(disposing);
		}

		protected override void OnMouseCaptureChanged(EventArgs eventArgs)
		{
			base.OnMouseCaptureChanged(eventArgs);
			TryShowBelow(_anchorBounds);
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == WmMouseActivate)
			{
				msg.Result = (IntPtr)MaNoActivate;
				base.WndProc(ref msg);
			}
			else if (msg.Msg == WmMouseWheel && _canForwardMouseWheel)
			{
				if (GetBackstageTab() != null && GetBackstageTab().TabPage != null)
				{
					_canForwardMouseWheel = false;
					try
					{
						NativeMethods.SendMessage(GetBackstageTab().TabPage.Handle, WmMouseWheel, msg.WParam, msg.LParam);
					}
					finally
					{
						_canForwardMouseWheel = true;
					}
				}
			}
			else
			{
				base.WndProc(ref msg);
			}
		}

		protected override void ComputeLayout()
		{
			base.ComputeLayout();
			if (_ribbon != null)
			{
				using Graphics graphics = CreateGraphics();
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
				RenderContent(graphics, textLayoutRenderer);
			}
		}

		internal void ApplyLayout()
		{
			if (_ribbon != null)
			{
				Point location = new Point(0, _anchorBounds.Bottom + 1);
				using Graphics graphics = CreateGraphics();
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
				_backstageMenuRenderer.CalculateFullItemWidth(graphics, textLayoutRenderer, _ribbon.Font);
				if (_backstageMenuRenderer._fullItemWidth > 0)
				{
					base.Location = location;
					if (_ribbon.Parent != null)
					{
						Size size = _ribbon.Parent.ClientRectangle.Size;
						size.Height -= _anchorBounds.Bottom + 1;
						base.Size = size;
						BringToFront();
					}
					else
					{
						Size ribbonSize = _ribbon.Size;
						ribbonSize.Height -= _anchorBounds.Bottom + 1;
						base.Size = ribbonSize;
					}
					RenderContent(graphics, textLayoutRenderer);
				}
				else
				{
					base.Size = Size.Empty;
				}
			}
			Invalidate(invalidateChildren: true);
		}

		internal bool TryShowBelow(Rectangle rectangle)
		{
			_anchorBounds = rectangle;
			if (_ribbon.Parent != null)
			{
				base.Parent = _ribbon.Parent;
			}
			else
			{
				base.Parent = _ribbon;
			}
			ApplyLayout();
			if (!base.Size.IsEmpty)
			{
				Show();
				RibbonDrawingUtils.TryFocusControl(this);
				_tabPagePanel.Show();
				return true;
			}
			return false;
		}

		internal Rectangle GetScreenBounds()
		{
			if (_backstageMenuRenderer != null)
			{
				return new Rectangle(PointToScreen(Point.Empty), base.ClientRectangle.Size);
			}
			return Rectangle.Empty;
		}

		private void RenderContent(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			if (base.ClientRectangle.Width <= 0 || base.ClientRectangle.Height <= 0)
			{
				return;
			}
			int contentHeight = Math.Max(base.MinSize.Height, base.ClientRectangle.Height);
			_backstageMenuRenderer.ArrangeItem(graphics, textRenderer, -base.ScrollPosition.X, -base.ScrollPosition.Y, _backstageMenuRenderer._fullItemWidth, contentHeight);
			foreach (CommandBaseRenderer item in _backstageMenuRenderer._childRenderers)
			{
				if (item is BackstageTabRenderer backstageTabRenderer && backstageTabRenderer.GetBackStageTab() != null && backstageTabRenderer.GetBackStageTab().GetSelectedState() && GetSelectedRenderer() != item)
				{
					SetSelectedRenderer(item);
					break;
				}
			}
			contentHeight -= 2;
			int val = Math.Max(base.ClientRectangle.Width, base.MinSize.Width) - _backstageMenuRenderer._fullItemWidth;
			int width = Math.Max(base.MinSize.Width, val);
			int x = _backstageMenuRenderer._fullItemWidth - base.ScrollPosition.X;
			_tabPagePanel.Bounds = new Rectangle(x, 2, base.ClientRectangle.Width - _backstageMenuRenderer._fullItemWidth + base.ScrollPosition.X, base.ClientRectangle.Height - 2);
			if (_selectedRenderer != null && _selectedRenderer is BackstageTabRenderer)
			{
				BackstageTab backstageTab = ((BackstageTabRenderer)_selectedRenderer).GetBackStageTab();
				if (backstageTab != null)
				{
					backstageTab.TabPage.Bounds = new Rectangle(0, -base.ScrollPosition.Y, width, contentHeight);
				}
			}
			Invalidate(invalidateChildren: true);
		}

		private void PaintThemed(PaintEventArgs eventArgs, ResolvedTheme theme)
		{
			ThemedRibbonPainter.BackstageTopLine(eventArgs.Graphics, theme, base.Width);
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
			if (_backstageMenuRenderer != null)
			{
				Rectangle rectangle = new Rectangle(0, 2, _backstageMenuRenderer._fullItemWidth - base.ScrollPosition.X, base.ClientRectangle.Height);
				GraphicsState state = eventArgs.Graphics.Save();
				eventArgs.Graphics.SetClip(rectangle);
				_backstageMenuRenderer.Paint(eventArgs.Graphics, textRenderer, _ribbon._colorScheme, _ribbon.Font, _ribbon.ForeColorDark, rectangle);
				eventArgs.Graphics.Restore(state);
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			PaintThemed(eventArgs, _ribbon._colorScheme.Theme);
			return;
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			HandleMouseMove(eventArgs);
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			HandleMouseDown(eventArgs);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			HandleMouseUp(eventArgs);
		}

		internal void SelectPreviousItem()
		{
			if (_backstageMenuRenderer != null)
			{
				_backstageMenuRenderer.SelectPrevious();
			}
		}

		internal void SelectNextItem()
		{
			if (_backstageMenuRenderer != null)
			{
				_backstageMenuRenderer.SelectNext();
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Up:
				SelectPreviousItem();
				return true;
			case Keys.Down:
				SelectNextItem();
				return true;
			case Keys.Return:
			case Keys.Space:
				if (_backstageMenuRenderer != null && ((CommandContainerRenderer)_backstageMenuRenderer).GetHotRenderer() != null)
				{
					if (((CommandContainerRenderer)_backstageMenuRenderer).GetHotRenderer().CommandType == CommandType.SmallButton)
					{
						((CommandContainerRenderer)_backstageMenuRenderer).GetHotRenderer().GetCommand().InvokeOnClick();
					}
					return true;
				}
				break;
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			((CommandContainerRenderer)_backstageMenuRenderer).SetHotRenderer((CommandBaseRenderer)null);
			if (_ribbon != null)
			{
				_ribbon.ShowSuperTip(null);
			}
		}

		internal bool HandleMouseDown(MouseEventArgs eventArgs)
		{
			if (_ribbon != null && _ribbon.GetHostForm() != null)
			{
				_ribbon.GetHostForm().Activate();
			}
			RibbonDrawingUtils.TryFocusControl(this);
			if (_backstageMenuRenderer._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				_backstageMenuRenderer.OnMouseDown(eventArgs);
				return true;
			}
			if (GetVerticalScrollBar() != null && GetVerticalScrollBar().Bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				Point point = GetVerticalScrollBar().PointToClient(PointToScreen(new Point(eventArgs.X, eventArgs.Y)));
				MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, point.X, point.Y, eventArgs.Delta);
				GetVerticalScrollBar().HandleMouseDown(e);
				return true;
			}
			if (GetHorizontalScrollBar() != null)
			{
				Point ribbonClientPoint = GetHorizontalScrollBar().PointToClient(PointToScreen(new Point(eventArgs.X, eventArgs.Y)));
				if (GetHorizontalScrollBar().Bounds.Contains(ribbonClientPoint.X, ribbonClientPoint.Y))
				{
					MouseEventArgs e2 = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, ribbonClientPoint.X, ribbonClientPoint.Y, eventArgs.Delta);
					GetHorizontalScrollBar().HandleMouseDown(e2);
					return true;
				}
			}
			return false;
		}

		internal bool HandleMouseMove(MouseEventArgs eventArgs)
		{
			if (_backstageMenuRenderer._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				_backstageMenuRenderer.OnMouseMove(eventArgs);
				return true;
			}
			if (GetVerticalScrollBar() != null && GetVerticalScrollBar().Bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				Point point = GetVerticalScrollBar().PointToClient(PointToScreen(new Point(eventArgs.X, eventArgs.Y)));
				MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, point.X, point.Y, eventArgs.Delta);
				GetVerticalScrollBar().HandleMouseMove(e);
				return true;
			}
			if (GetHorizontalScrollBar() != null)
			{
				Point ribbonClientPoint = GetHorizontalScrollBar().PointToClient(PointToScreen(new Point(eventArgs.X, eventArgs.Y)));
				if (GetHorizontalScrollBar().Bounds.Contains(ribbonClientPoint.X, ribbonClientPoint.Y))
				{
					MouseEventArgs e2 = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, ribbonClientPoint.X, ribbonClientPoint.Y, eventArgs.Delta);
					GetHorizontalScrollBar().HandleMouseMove(e2);
					return true;
				}
			}
			return false;
		}

		internal bool HandleMouseUp(MouseEventArgs eventArgs)
		{
			if (_backstageMenuRenderer._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				_backstageMenuRenderer.OnMouseUp(eventArgs);
				return true;
			}
			if (GetVerticalScrollBar() != null && GetVerticalScrollBar().Bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				Point point = GetVerticalScrollBar().PointToClient(PointToScreen(new Point(eventArgs.X, eventArgs.Y)));
				MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, point.X, point.Y, eventArgs.Delta);
				GetVerticalScrollBar().ForwardMouseLeave(e);
				return true;
			}
			if (GetHorizontalScrollBar() != null)
			{
				Point ribbonClientPoint = GetHorizontalScrollBar().PointToClient(PointToScreen(new Point(eventArgs.X, eventArgs.Y)));
				if (GetHorizontalScrollBar().Bounds.Contains(ribbonClientPoint.X, ribbonClientPoint.Y))
				{
					MouseEventArgs e2 = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, ribbonClientPoint.X, ribbonClientPoint.Y, eventArgs.Delta);
					GetHorizontalScrollBar().ForwardMouseLeave(e2);
					return true;
				}
			}
			return false;
		}

		internal int GetHitRegion(Point pt)
		{
			if (GetVerticalScrollBar() != null && GetVerticalScrollBar().Bounds.Contains(pt))
			{
				return 3;
			}
			Point p = PointToScreen(pt);
			if (GetHorizontalScrollBar() != null && GetHorizontalScrollBar().Bounds.Contains(GetHorizontalScrollBar().PointToClient(p)))
			{
				return 4;
			}
			if (base.ClientRectangle.Contains(pt))
			{
				if (_backstageMenuRenderer._bounds.Contains(pt))
				{
					return 2;
				}
				if (_tabPagePanel.Bounds.Contains(pt))
				{
					return 1;
				}
			}
			return 0;
		}

		internal virtual IList<RibbonKeyTipEntry> GetKeyTips(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			IList<RibbonKeyTipEntry> list = new List<RibbonKeyTipEntry>();
			if (_ribbon != null)
			{
				_ribbon.SetKeyTipOwner(GetBackstageMenu());
				foreach (CommandBaseRenderer itemRenderer in _backstageMenuRenderer._childRenderers)
				{
					if (!(itemRenderer.GetCommand() is IKeyTipProvider))
					{
						continue;
					}
					IKeyTipProvider keyTipProvider = (IKeyTipProvider)itemRenderer.GetCommand();
					if (keyTipProvider.HasKeyTip)
					{
						Point keyTipLocation = itemRenderer.GetKeyTipLocation(graphics, textRenderer, Font, keyTipProvider.KeyTip);
						if (keyTipLocation.Y < base.ClientRectangle.Bottom && keyTipLocation.Y >= base.ClientRectangle.Top)
						{
							SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(Font, _renderHost.UseCompatibleTextRendering, keyTipProvider.KeyTip, _ribbon._colorScheme);
							superTipSettingsWindowMessageHandler.SetCompactMode(true);
							superTipSettingsWindowMessageHandler._isKeyTipEnabled = itemRenderer.GetCommand().IsEnabled;
							superTipSettingsWindowMessageHandler.SetSticky(true);
							superTipSettingsWindowMessageHandler.ShowAt(_renderHost.ParentControl, PointToScreen(keyTipLocation));
							RibbonKeyTipEntry item = new RibbonKeyTipEntry(keyTipProvider.KeyTip, itemRenderer.GetCommand(), itemRenderer, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
							list.Add(item);
						}
					}
				}
			}
			return list;
		}

		ImageList ICommandRenderHost.GetImageList(CommandBase command)
		{
			return _renderHost.GetImageList(command);
		}

		ImageList ICommandRenderHost.GetLargeImageList(CommandBase command)
		{
			return _renderHost.GetLargeImageList(command);
		}

		Point ICommandRenderHost.PointToScreen(Point clientPoint)
		{
			return PointToScreen(clientPoint);
		}

		void ICommandRenderHost.ShowQuickCustomizeMenu(CommandBase command)
		{
			_ribbon.BuildRibbonContextMenu(command, _ribbon._isQuickAccessItemMouseUp);
		}
	}
}
