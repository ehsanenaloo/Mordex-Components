using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class QuickAccessToolbarRenderer : IKeyboardNavigable
	{
		public Rectangle _rectangle;

		protected MxRibbon _ribbon;

		public IList<CommandBaseRenderer> _itemRenderers;

		protected bool _itemsDirty;

		private CommandBaseRenderer _hoveredItem;

		protected internal QuickAccessCustomizeButtonRenderer _customizeButton;

		protected internal QuickAccessOverflowButtonRenderer _overflowButton;

		internal int _contentWidth;

		private bool _isMousePressed;

		private AccessibleObject _cachedAccessibleObject;

		public QuickAccessToolbarRenderer(MxRibbon ribbon)
		{
			_ribbon = ribbon;
			_itemsDirty = true;
		}

		public MxRibbon GetRibbon()
		{
			return _ribbon;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_cachedAccessibleObject == null)
			{
				_cachedAccessibleObject = new QuickAccessToolbarAccessibleObject(this);
			}
			return _cachedAccessibleObject;
		}

		internal CommandBaseRenderer GetHoveredItem()
		{
			return _hoveredItem;
		}

		internal void SetHoveredItem(CommandBaseRenderer commandBaseRenderer)
		{
			if (_hoveredItem != commandBaseRenderer)
			{
				if (_hoveredItem != null)
				{
					_hoveredItem.StartPressAnimation();
					_hoveredItem.Hot = false;
					if (_hoveredItem.GetCommand() is IMouseEventInvoker)
					{
						((IMouseEventInvoker)_hoveredItem.GetCommand()).InvokeMouseLeave();
					}
				}
				if (commandBaseRenderer != null && commandBaseRenderer.GetCommand() is IMouseEventInvoker)
				{
					((IMouseEventInvoker)commandBaseRenderer.GetCommand()).InvokeMouseEnter();
				}
				_hoveredItem = commandBaseRenderer;
				if (_hoveredItem != null)
				{
					_hoveredItem.Hot = true;
				}
				if (_ribbon != null)
				{
					_ribbon.ShowSuperTip(_hoveredItem);
				}
			}
			else if (_hoveredItem != null && !_hoveredItem.Hot)
			{
				_hoveredItem.Hot = true;
			}
		}

		internal bool ContainsItem(CommandBaseRenderer item)
		{
			if (item == _customizeButton)
			{
				return true;
			}
			if (item == _overflowButton)
			{
				return true;
			}
			if (_itemRenderers != null)
			{
				return _itemRenderers.Contains(item);
			}
			return false;
		}

		private CommandBaseRenderer CreateItemRenderer(CommandBase command)
		{
			if (command is DropDownCommand)
			{
				return new QuickAccessDropDownRenderer(command as DropDownCommand, _ribbon, this);
			}
			if (command is RibbonGroup)
			{
				return new QuickAccessGroupRenderer(command as RibbonGroup, _ribbon, this);
			}
			if (command is SeparatorCommand)
			{
				return new RibbonVerticalSeparatorRenderer(command, _ribbon, null);
			}
			return new QuickAccessButtonRenderer(command as ButtonCommand, _ribbon, this);
		}

		internal void RebuildItems()
		{
			_itemRenderers = new List<CommandBaseRenderer>();
			foreach (CommandBase quickAccessCommand in _ribbon.QuickAccessCommands)
			{
				if (quickAccessCommand.Visible)
				{
					CommandBaseRenderer item = CreateItemRenderer(quickAccessCommand);
					_itemRenderers.Add(item);
				}
			}
			if (_ribbon.ShowCustomizeButton)
			{
				_customizeButton = new QuickAccessCustomizeButtonRenderer(_ribbon, this);
			}
			else
			{
				_customizeButton = null;
			}
			_itemsDirty = false;
		}

		public bool HasVisibleItems()
		{
			if (_customizeButton != null && _customizeButton._isVisible)
			{
				return true;
			}
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item._isVisible)
				{
					return true;
				}
			}
			return false;
		}

		public virtual void Shrink(Graphics graphics)
		{
			if (_customizeButton != null && _customizeButton._isVisible)
			{
				_customizeButton._isVisible = false;
			}
			if (_overflowButton == null)
			{
				_overflowButton = new QuickAccessOverflowButtonRenderer(_ribbon, this);
			}
			for (int index = _itemRenderers.Count - 1; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _itemRenderers[index];
				if (thisValue._isVisible)
				{
					thisValue._isVisible = false;
					break;
				}
			}
			_contentWidth = 0;
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item._isVisible)
				{
					_contentWidth += item._fullItemWidth;
				}
			}
			if (_contentWidth > 0)
			{
				_contentWidth += 2;
			}
			if (_overflowButton != null)
			{
				_overflowButton.CalculateFullItemWidth(graphics, null, null);
				_overflowButton.ArrangeItem(graphics, null, _rectangle.X + _contentWidth, _rectangle.Y, _overflowButton._fullItemWidth, _rectangle.Height);
				_contentWidth += _overflowButton._fullItemWidth;
			}
		}

		public virtual void ArrangeToolbar(Graphics graphics, int left, int top)
		{
			if (_itemsDirty)
			{
				RebuildItems();
			}
			_overflowButton = null;
			int toolbarHeight = _ribbon.ClientRectangle.Bottom - top;
			if (_itemRenderers.Count > 0 || _ribbon.ShowCustomizeButton)
			{
				int totalWidth = 0;
				_contentWidth = 0;
				int itemHeight = toolbarHeight - 4;
				using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					item.CalculateFullItemWidth(graphics, textRenderer, _ribbon.Font);
					item._isEnabled = !_ribbon.IsBackstageOpen() && item._isEnabled;
					totalWidth += item._fullItemWidth;
				}
				if (totalWidth > 0)
				{
					totalWidth += 2;
				}
				if (_customizeButton != null)
				{
					_customizeButton.CalculateFullItemWidth(graphics, textRenderer, _ribbon.Font);
					_customizeButton._isEnabled = !_ribbon.IsBackstageOpen();
					totalWidth += _customizeButton._fullItemWidth;
				}
				_contentWidth = totalWidth;
				_rectangle = new Rectangle(left, top, _ribbon.ClientRectangle.Width - left, toolbarHeight);
				left += 2;
				top += 2;
				foreach (CommandBaseRenderer placedItem in _itemRenderers)
				{
					placedItem.ArrangeItem(graphics, null, left, top, placedItem._fullItemWidth, itemHeight);
					placedItem._isVisible = true;
					left += placedItem._bounds.Width;
				}
				left += 2;
				if (_customizeButton != null)
				{
					_customizeButton.ArrangeItem(graphics, null, left, top, _customizeButton._fullItemWidth, itemHeight);
					_customizeButton._isVisible = true;
				}
			}
			else
			{
				_rectangle = new Rectangle(left, top, _ribbon.ClientRectangle.Width - left, toolbarHeight);
			}
		}

		internal ArrayList GetOverflowItems()
		{
			ArrayList arrayList = new ArrayList();
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (!item._isVisible)
				{
					arrayList.Add(item);
				}
			}
			if (_customizeButton != null)
			{
				arrayList.Add(_customizeButton);
			}
			return arrayList;
		}

		internal virtual CommandBaseRenderer GetItemAt(Point pt)
		{
			if (_customizeButton != null && _customizeButton._isVisible && _customizeButton._bounds.Contains(pt))
			{
				return _customizeButton;
			}
			if (_overflowButton != null && _overflowButton._bounds.Contains(pt))
			{
				return _overflowButton;
			}
			if (_itemRenderers != null)
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item._isVisible && item._bounds.Contains(pt))
					{
						return item;
					}
				}
			}
			return null;
		}

		public virtual void OnPaint(Graphics graphics, RibbonColorScheme colorScheme, Rectangle clipRectangle)
		{
			if (_rectangle.Width <= 0 || _rectangle.Height <= 0)
			{
				return;
			}
			ThemedRibbonPainter.QuickAccessBar(graphics, colorScheme.Theme, _rectangle);
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item._isVisible)
				{
					Rectangle rectangle = Rectangle.Intersect(clipRectangle, item._bounds);
					if (rectangle.Width > 0 && rectangle.Height > 0)
					{
						item.Paint(graphics, textRenderer, _ribbon._colorScheme, null, _ribbon.ForeColorDark, clipRectangle);
					}
				}
			}
			if (_customizeButton != null && _customizeButton._isVisible)
			{
				_customizeButton.Paint(graphics, textRenderer, _ribbon._colorScheme, null, _ribbon.ForeColorDark, clipRectangle);
			}
			if (_overflowButton != null)
			{
				_overflowButton.Paint(graphics, textRenderer, _ribbon._colorScheme, null, _ribbon.ForeColorDark, clipRectangle);
			}
		}

		public bool HandleMouseMove(Point pt)
		{
			CommandBaseRenderer itemAt = GetItemAt(pt);
			SetHoveredItem(itemAt);
			if (itemAt != null)
			{
				itemAt.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, pt.X, pt.Y, 0));
				return true;
			}
			return false;
		}

		public bool HandleMouseDown(Point pt, MouseButtons button)
		{
			_ribbon.ShowSuperTip(null);
			CommandBaseRenderer itemAt = GetItemAt(pt);
			_ribbon.SetPushedRenderer(itemAt);
			if (itemAt != null && button == MouseButtons.Left)
			{
				itemAt.Pushed = true;
			}
			if (itemAt != null)
			{
				itemAt.OnMouseDown(new MouseEventArgs(button, 1, pt.X, pt.Y, 0));
				if (Control.MouseButtons != MouseButtons.None)
				{
					_isMousePressed = true;
				}
				return true;
			}
			if (_rectangle.Contains(pt))
			{
				_isMousePressed = true;
				return true;
			}
			return false;
		}

		public bool HandleMouseUp(Point pt, MouseButtons button)
		{
			if (_isMousePressed && _ribbon != null)
			{
				_isMousePressed = false;
				CommandBaseRenderer itemAt = GetItemAt(pt);
				if (button == MouseButtons.Right && itemAt == null)
				{
					_ribbon.BuildRibbonContextMenu(null, false);
				}
				if (itemAt != null)
				{
					_ribbon._isQuickAccessItemMouseUp = true;
					itemAt.OnMouseUp(new MouseEventArgs(button, 1, pt.X, pt.Y, 0));
					_ribbon._isQuickAccessItemMouseUp = false;
					_ribbon.SetPushedRenderer(null);
					return true;
				}
				_ribbon.SetPushedRenderer(null);
			}
			return false;
		}

		internal void MarkItemsDirty()
		{
			_itemsDirty = true;
			Invalidate(update: false);
		}

		internal virtual void Invalidate(bool update)
		{
			if (_ribbon != null)
			{
				_ribbon.Invalidate(_rectangle);
			}
		}

		internal virtual Point PointToScreen(CommandBaseRenderer item, Point clientPt)
		{
			if (item.GetRenderHost() is CommandBaseEditorDialog)
			{
				return item.GetRenderHost().PointToScreen(clientPt);
			}
			return _ribbon.PointToScreen(clientPt);
		}

		private void RenderContent(Graphics graphics, RibbonColorScheme colorScheme)
		{
			Color borderColor = colorScheme._quickAccessBorder;
			Rectangle rect = _rectangle;
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, Color.FromArgb(150, Color.White), Color.FromArgb(130, Color.White), LinearGradientMode.Vertical);
			Pen pen = GdiCache.Pen(borderColor);
			Pen highlightPen = new Pen(linearGradientBrush);
			Rectangle rectangle = _rectangle;
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			rectangle.Offset(1, 1);
			rectangle.Height--;
			rectangle.Width--;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(rectangle.X, rectangle.Y, 6, 6, -180f, 90f);
			graphicsPath.AddLine(rectangle.X + 2, rectangle.Y, rectangle.Right - 3, rectangle.Y);
			graphicsPath.AddArc(rectangle.Right - 7, rectangle.Y, 6, 6, -90f, 90f);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 1, rectangle.Bottom - 3);
			graphicsPath.AddArc(rectangle.Right - 7, rectangle.Bottom - 7, 6, 6, 0f, 90f);
			graphicsPath.AddLine(rectangle.Right - 3, rectangle.Bottom - 1, rectangle.X + 2, rectangle.Bottom - 1);
			graphicsPath.AddArc(rectangle.X, rectangle.Bottom - 7, 6, 6, 90f, 90f);
			graphicsPath.AddLine(rectangle.X, rectangle.Bottom - 3, rectangle.X, rectangle.Y + 2);
			graphics.DrawPath(highlightPen, graphicsPath);
			graphicsPath.Dispose();
			rectangle.Offset(-1, -1);
			graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(rectangle.X, rectangle.Y, 6, 6, -180f, 90f);
			graphicsPath.AddLine(rectangle.X + 2, rectangle.Y, rectangle.Right - 3, rectangle.Y);
			graphicsPath.AddArc(rectangle.Right - 7, rectangle.Y, 6, 6, -90f, 90f);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 1, rectangle.Bottom - 3);
			graphicsPath.AddArc(rectangle.Right - 7, rectangle.Bottom - 7, 6, 6, 0f, 90f);
			graphicsPath.AddLine(rectangle.Right - 3, rectangle.Bottom - 1, rectangle.X + 2, rectangle.Bottom - 1);
			graphicsPath.AddArc(rectangle.X, rectangle.Bottom - 7, 6, 6, 90f, 90f);
			graphicsPath.AddLine(rectangle.X, rectangle.Bottom - 3, rectangle.X, rectangle.Y + 2);
			graphics.DrawPath(pen, graphicsPath);
			graphicsPath.Dispose();
			graphics.SmoothingMode = smoothingMode;
			linearGradientBrush.Dispose();
			highlightPen.Dispose();
		}

		public bool TrySelectItemAtX(int pointX)
		{
			CommandBaseRenderer itemAt = GetItemAt(new Point(pointX, _rectangle.Y + 2));
			if (itemAt != null)
			{
				_ribbon.SetKeyboardFocusRenderer(itemAt);
				return true;
			}
			if (pointX < _rectangle.X)
			{
				return ((IKeyboardNavigable)this).SelectFirstItem();
			}
			return ((IKeyboardNavigable)this).SelectLastItem();
		}

		private int GetLastVisibleItemIndex()
		{
			for (int index = _itemRenderers.Count - 1; index >= 0; index--)
			{
				if (_itemRenderers[index]._isVisible)
				{
					return index;
				}
			}
			return 0;
		}

		bool IKeyboardNavigable.MoveNext(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectFirstItem();
			}
			int nextIndex = -1;
			if (selectedItem == _customizeButton || selectedItem == _overflowButton)
			{
				nextIndex = _itemRenderers.Count + 1;
			}
			else
			{
				nextIndex = _itemRenderers.IndexOf(selectedItem) + 1;
				int lastVisibleIndex = GetLastVisibleItemIndex();
				if (nextIndex > lastVisibleIndex)
				{
					nextIndex = _itemRenderers.Count;
				}
			}
			CommandBaseRenderer commandBaseRenderer = null;
			if (nextIndex < _itemRenderers.Count)
			{
				commandBaseRenderer = _itemRenderers[nextIndex];
			}
			else if (_itemRenderers.Count == nextIndex && (_customizeButton != null || _overflowButton != null))
			{
				commandBaseRenderer = ((_customizeButton == null || !_customizeButton._isVisible) ? ((CommandBaseRenderer)_overflowButton) : ((CommandBaseRenderer)_customizeButton));
			}
			if (commandBaseRenderer != null)
			{
				_ribbon.SetKeyboardFocusRenderer(commandBaseRenderer);
				return true;
			}
			if (_ribbon != null)
			{
				RibbonTabRenderer ribbonTabRenderer = _ribbon._tabStripRenderer.FindTabRenderer(_ribbon.SelectedTab);
				if (ribbonTabRenderer != null)
				{
					_ribbon.SetKeyboardFocusRenderer(ribbonTabRenderer);
					return true;
				}
			}
			return false;
		}

		bool IKeyboardNavigable.MovePrevious(CommandBaseRenderer selectedItem)
		{
			return ((IKeyboardNavigable)this).MoveLeft(selectedItem);
		}

		bool IKeyboardNavigable.MoveRight(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectFirstItem();
			}
			int nextIndex = -1;
			if (selectedItem == _customizeButton || selectedItem == _overflowButton)
			{
				nextIndex = _itemRenderers.Count + 1;
			}
			else
			{
				nextIndex = _itemRenderers.IndexOf(selectedItem) + 1;
				int lastVisibleIndex = GetLastVisibleItemIndex();
				if (nextIndex > lastVisibleIndex)
				{
					nextIndex = _itemRenderers.Count;
				}
			}
			CommandBaseRenderer commandBaseRenderer = null;
			if (nextIndex < _itemRenderers.Count)
			{
				commandBaseRenderer = _itemRenderers[nextIndex];
			}
			else if (_itemRenderers.Count == nextIndex && (_customizeButton != null || _overflowButton != null))
			{
				commandBaseRenderer = ((_customizeButton == null || !_customizeButton._isVisible) ? ((CommandBaseRenderer)_overflowButton) : ((CommandBaseRenderer)_customizeButton));
			}
			else if (_ribbon.QuickAccessPosition == QuickAccessPosition.AboveRibbon && _ribbon._controlBoxButtonRenderer != null)
			{
				commandBaseRenderer = _ribbon._controlBoxButtonRenderer;
			}
			else if (_itemRenderers.Count > 0)
			{
				commandBaseRenderer = _itemRenderers[0];
			}
			if (commandBaseRenderer != null)
			{
				_ribbon.SetKeyboardFocusRenderer(commandBaseRenderer);
				return true;
			}
			return false;
		}

		bool IKeyboardNavigable.MoveLeft(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectLastItem();
			}
			int previousIndex = -1;
			previousIndex = ((selectedItem != _customizeButton && selectedItem != _overflowButton) ? (_itemRenderers.IndexOf(selectedItem) - 1) : (_itemRenderers.Count - 1));
			CommandBaseRenderer commandBaseRenderer = null;
			if (previousIndex >= 0)
			{
				commandBaseRenderer = _itemRenderers[previousIndex];
			}
			else if (_ribbon.QuickAccessPosition == QuickAccessPosition.AboveRibbon && _ribbon._controlBoxButtonRenderer != null)
			{
				commandBaseRenderer = _ribbon._controlBoxButtonRenderer;
			}
			else if (_customizeButton != null && _customizeButton._isVisible)
			{
				commandBaseRenderer = _customizeButton;
			}
			else if (_overflowButton != null)
			{
				commandBaseRenderer = _overflowButton;
			}
			else if (_itemRenderers.Count > 0)
			{
				commandBaseRenderer = _itemRenderers[_itemRenderers.Count - 1];
			}
			if (commandBaseRenderer != null)
			{
				_ribbon.SetKeyboardFocusRenderer(commandBaseRenderer);
				return true;
			}
			return false;
		}

		bool IKeyboardNavigable.MoveDown(CommandBaseRenderer selectedItem)
		{
			if (_ribbon.QuickAccessPosition == QuickAccessPosition.AboveRibbon)
			{
				int itemCenterX = selectedItem._bounds.X + selectedItem._bounds.Width / 2;
				RibbonTabRenderer ribbonTabRenderer = _ribbon._tabStripRenderer.GetTabAt(itemCenterX, _ribbon._tabStripRenderer._stripBounds.Top + 6);
				if (ribbonTabRenderer != null)
				{
					_ribbon.SelectedTab = ribbonTabRenderer.GetTab();
					_ribbon.SetKeyboardFocusRenderer(ribbonTabRenderer);
					return true;
				}
				_ribbon._tabStripRenderer.SelectFirstTab();
			}
			return false;
		}

		bool IKeyboardNavigable.MoveUp(CommandBaseRenderer selectedItem)
		{
			if (_ribbon.QuickAccessPosition == QuickAccessPosition.BelowRibbon)
			{
				if (_ribbon.Minimized)
				{
					int itemCenterX = selectedItem._bounds.X + selectedItem._bounds.Width / 2;
					RibbonTabRenderer ribbonTabRenderer = _ribbon._tabStripRenderer.GetTabAt(itemCenterX, _ribbon._tabStripRenderer._stripBounds.Top + 6);
					if (ribbonTabRenderer != null)
					{
						_ribbon.SelectedTab = ribbonTabRenderer.GetTab();
						_ribbon.SetKeyboardFocusRenderer(ribbonTabRenderer);
						return true;
					}
					_ribbon._tabStripRenderer.SelectFirstTab();
				}
				else if (_ribbon._tabContentRenderer != null)
				{
					_ribbon._tabContentRenderer.TryMoveUpIntoGroups(null);
				}
			}
			return false;
		}

		bool IKeyboardNavigable.SelectLastItem()
		{
			if (_customizeButton != null && _customizeButton._isVisible)
			{
				_ribbon.SetKeyboardFocusRenderer(_customizeButton);
				return true;
			}
			if (_overflowButton != null)
			{
				_ribbon.SetKeyboardFocusRenderer(_overflowButton);
				return true;
			}
			if (_itemRenderers.Count > 0)
			{
				_ribbon.SetKeyboardFocusRenderer(_itemRenderers[_itemRenderers.Count - 1]);
				return true;
			}
			return false;
		}

		bool IKeyboardNavigable.SelectFirstItem()
		{
			if (_itemRenderers.Count > 0)
			{
				_ribbon.SetKeyboardFocusRenderer(_itemRenderers[0]);
				return true;
			}
			return false;
		}

		void IKeyboardNavigable.AddKeyTips(Graphics graphics, TextLayoutRenderer textRenderer, Font font, IList<RibbonKeyTipEntry> keyTips, bool showToolTip)
		{
			int keyTipNumber = 1;
			string text = "";
			bool useLetters = false;
			string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			foreach (CommandBaseRenderer quickAccessItem in _itemRenderers)
			{
				if (!quickAccessItem._isVisible)
				{
					continue;
				}
				string keyTipText = (useLetters ? (text + letters.Substring(keyTipNumber, 1)) : (text + keyTipNumber));
				SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(font, _ribbon.UseCompatibleTextRendering, keyTipText, _ribbon._colorScheme);
				superTipSettingsWindowMessageHandler.SetCompactMode(true);
				superTipSettingsWindowMessageHandler.SetSticky(true);
				superTipSettingsWindowMessageHandler._isKeyTipEnabled = quickAccessItem.GetCommand().IsEnabled;
				if (showToolTip)
				{
					superTipSettingsWindowMessageHandler.ShowAt(_ribbon, quickAccessItem.GetKeyTipLocation(graphics, textRenderer, font, keyTipText));
				}
				RibbonKeyTipEntry item = new RibbonKeyTipEntry(keyTipText, quickAccessItem.GetCommand(), quickAccessItem, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
				keyTips.Add(item);
				if (!useLetters && text.Length > 0)
				{
					keyTipNumber--;
					if (keyTipNumber <= 0)
					{
						useLetters = true;
					}
				}
				else
				{
					keyTipNumber++;
				}
				if (!useLetters && keyTipNumber > 9)
				{
					text = "0";
					keyTipNumber = 9;
				}
			}
			if (_overflowButton != null)
			{
				SuperTipSettingsWindowMessageHandler overflowKeyTipWindow = new SuperTipSettingsWindowMessageHandler(font, _ribbon.UseCompatibleTextRendering, "YY", _ribbon._colorScheme);
				overflowKeyTipWindow.SetCompactMode(true);
				overflowKeyTipWindow.SetSticky(true);
				overflowKeyTipWindow.ShowAt(_ribbon, _overflowButton.GetKeyTipLocation(graphics, textRenderer, font, "YY"));
				RibbonKeyTipEntry overflowEntry = new RibbonKeyTipEntry("YY", null, _overflowButton, overflowKeyTipWindow, overflowKeyTipWindow._isKeyTipEnabled);
				keyTips.Add(overflowEntry);
			}
		}

		CommandBase IKeyboardNavigable.GetCommandAt(int pointX, int pointY)
		{
			if (_rectangle.Contains(pointX, pointY))
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item._bounds.Contains(pointX, pointY))
					{
						return item.GetCommand();
					}
				}
			}
			return null;
		}

		CommandBaseRenderer IKeyboardNavigable.GetCommandLayoutItem(CommandBase command)
		{
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item.GetCommand() == command)
				{
					return item;
				}
			}
			return null;
		}

		void IKeyboardNavigable.RemoveControls()
		{
		}
	}
}
