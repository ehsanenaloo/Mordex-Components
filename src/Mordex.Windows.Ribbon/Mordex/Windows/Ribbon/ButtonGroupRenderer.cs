using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Ribbon.Design;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class ButtonGroupRenderer : CommandContainerRenderer, IKeyboardNavigable
	{
		private bool _hasRightSeparator = true;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				return AccessibleRole.Grouping;
			}
		}

		public override CommandType CommandType => CommandType.ButtonGroup;

		public override bool Focusable => false;

		public ButtonGroupRenderer(CommandBase command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
		}

		public bool HasRightSeparator()
		{
			return _hasRightSeparator;
		}

		public void SetHasRightSeparator(bool ribbonRenderer17Boolean)
		{
			_hasRightSeparator = ribbonRenderer17Boolean;
		}

		public ButtonGroup GetButtonGroup()
		{
			return _commandBaseRendererCommand as ButtonGroup;
		}

		private int GetItemGap()
		{
			return 0;
		}

		public int GetPreferredItemHeight(int availableHeight)
		{
			int maxItemHeight = GetRenderHost().SmallItemHeight;
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				int itemHeight = GetItemHeight(item, availableHeight);
				if (itemHeight > maxItemHeight)
				{
					maxItemHeight = itemHeight;
				}
			}
			return maxItemHeight;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_childRenderers = new List<CommandBaseRenderer>();
			CommandBaseRenderer commandBaseRenderer = null;
			int totalWidth = 4;
			totalWidth = 3;
			if (_hasRightSeparator)
			{
				totalWidth += 6;
			}
			foreach (CommandBase command in GetButtonGroup().Commands)
			{
				if (command.Visible)
				{
					commandBaseRenderer = CreateLayoutItem(command);
					commandBaseRenderer.CalculateFullItemWidth(graphics, textRenderer, font);
					totalWidth += commandBaseRenderer._fullItemWidth;
					_childRenderers.Add(commandBaseRenderer);
				}
			}
			SetLayoutWidth(_fullItemWidth = ((_childRenderers.Count <= 0) ? (totalWidth + GetRenderHost().SmallImageSize.Width) : (totalWidth + (_childRenderers.Count - 1) * GetItemGap())));
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int height)
		{
			_bounds = new Rectangle(left, top, width, height);
			int itemLeft = _bounds.Left;
			itemLeft += 3;
			int itemTop = _bounds.Top;
			int maxItemWidth = 0;
			_ = GetRenderHost().SmallItemHeight;
			_ = GetRenderHost().MaxItemRows;
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				int itemHeight = GetItemHeight(item, GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows);
				item._ownerBottom = _ownerBottom;
				item.ArrangeItem(graphics, textRenderer, itemLeft, itemTop, item.GetLayoutWidth(), itemHeight);
				itemLeft += item._fullItemWidth + GetItemGap();
				if (item._bounds.Width > maxItemWidth)
				{
					maxItemWidth = item._bounds.Width;
				}
			}
		}

		bool IKeyboardNavigable.MoveNext(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectFirstItem();
			}
			int startIndex = _childRenderers.IndexOf(selectedItem) + 1;
			for (int i = startIndex; i < _childRenderers.Count; i++)
			{
				CommandBaseRenderer thisValue = _childRenderers[i];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					GetRenderHost().ActiveInputItem = thisValue;
					return true;
				}
			}
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveNext(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MovePrevious(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectLastItem();
			}
			int startIndex = _childRenderers.IndexOf(selectedItem) - 1;
			for (int index = startIndex; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _childRenderers[index];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					GetRenderHost().ActiveInputItem = thisValue;
					return true;
				}
			}
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MovePrevious(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveRight(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectFirstItem();
			}
			int startIndex = _childRenderers.IndexOf(selectedItem) + 1;
			for (int i = startIndex; i < _childRenderers.Count; i++)
			{
				CommandBaseRenderer thisValue = _childRenderers[i];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					GetRenderHost().ActiveInputItem = thisValue;
					return true;
				}
			}
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveRight(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveLeft(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectLastItem();
			}
			int startIndex = _childRenderers.IndexOf(selectedItem) - 1;
			for (int index = startIndex; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _childRenderers[index];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					GetRenderHost().ActiveInputItem = thisValue;
					return true;
				}
			}
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveLeft(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveDown(CommandBaseRenderer selectedItem)
		{
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveDown(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveUp(CommandBaseRenderer selectedItem)
		{
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveUp(this);
			}
			return false;
		}

		bool IKeyboardNavigable.SelectLastItem()
		{
			for (int index = _childRenderers.Count - 1; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _childRenderers[index];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					if (thisValue is IKeyboardNavigable && ((IKeyboardNavigable)thisValue).SelectLastItem())
					{
						return true;
					}
					GetRenderHost().ActiveInputItem = thisValue;
					return true;
				}
			}
			return false;
		}

		bool IKeyboardNavigable.SelectFirstItem()
		{
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (!(item.GetCommand() is SeparatorCommand))
				{
					if (item is IKeyboardNavigable && ((IKeyboardNavigable)item).SelectFirstItem())
					{
						return true;
					}
					GetRenderHost().ActiveInputItem = item;
					return true;
				}
			}
			return false;
		}

		void IKeyboardNavigable.AddKeyTips(Graphics graphics, TextLayoutRenderer textRenderer, Font font, IList<RibbonKeyTipEntry> keyTips, bool showToolTip)
		{
			bool useCompatibleTextRendering = GetRenderHost().UseCompatibleTextRendering;
			foreach (CommandBaseRenderer itemRenderer in _childRenderers)
			{
				if (itemRenderer is IKeyboardNavigable)
				{
					((IKeyboardNavigable)itemRenderer).AddKeyTips(graphics, textRenderer, font, keyTips, showToolTip);
				}
				else
				{
					if (!(itemRenderer.GetCommand() is IKeyTipProvider))
					{
						continue;
					}
					IKeyTipProvider keyTipProvider = (IKeyTipProvider)itemRenderer.GetCommand();
					if (keyTipProvider.HasKeyTip)
					{
						SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(font, useCompatibleTextRendering, keyTipProvider.KeyTip, GetCommand().Ribbon._colorScheme);
						superTipSettingsWindowMessageHandler.SetCompactMode(true);
						superTipSettingsWindowMessageHandler.SetSticky(true);
						superTipSettingsWindowMessageHandler._isKeyTipEnabled = itemRenderer.GetCommand().IsEnabled;
						if (showToolTip)
						{
							superTipSettingsWindowMessageHandler.ShowAt(GetRenderHost().ParentControl, GetRenderHost().ParentControl.PointToScreen(itemRenderer.GetKeyTipLocation(graphics, textRenderer, font, keyTipProvider.KeyTip)));
						}
						RibbonKeyTipEntry item = new RibbonKeyTipEntry(keyTipProvider.KeyTip, itemRenderer.GetCommand(), itemRenderer, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
						keyTips.Add(item);
					}
				}
			}
		}

		CommandBase IKeyboardNavigable.GetCommandAt(int x, int y)
		{
			if (_bounds.Contains(x, y))
			{
				foreach (CommandBaseRenderer item in _childRenderers)
				{
					if (item._bounds.Contains(x, y))
					{
						if (item is IKeyboardNavigable)
						{
							return ((IKeyboardNavigable)item).GetCommandAt(x, y);
						}
						return item.GetCommand();
					}
				}
				return GetCommand();
			}
			return null;
		}

		CommandBaseRenderer IKeyboardNavigable.GetCommandLayoutItem(CommandBase command)
		{
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (item.GetCommand() == command)
				{
					return item;
				}
				if (item is IKeyboardNavigable)
				{
					CommandBaseRenderer commandLayoutItem = ((IKeyboardNavigable)item).GetCommandLayoutItem(command);
					if (commandLayoutItem != null)
					{
						return commandLayoutItem;
					}
				}
			}
			return null;
		}

		void IKeyboardNavigable.RemoveControls()
		{
			if (GetRenderHost() == null || GetRenderHost().ParentControl == null)
			{
				return;
			}
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (item is ContainerControlCommandRenderer)
				{
					Control control = ((ContainerControlCommandRenderer)item).GetHostedControl();
					if (GetRenderHost().ParentControl.Controls.Contains(control))
					{
						GetRenderHost().ParentControl.Controls.Remove(control);
					}
				}
				else if (item is IKeyboardNavigable)
				{
					((IKeyboardNavigable)item).RemoveControls();
				}
			}
		}

		private void PaintOffice2010Content(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			for (int i = 0; i < _childRenderers.Count; i++)
			{
				CommandBaseRenderer thisValue = _childRenderers[i];
				Rectangle rectangle = Rectangle.Intersect(clipRectangle, thisValue._bounds);
				if (rectangle.Width > 0 && rectangle.Height > 0)
				{
					thisValue.Paint(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
				}
			}
			if (HasRightSeparator())
			{
				ThemedRibbonPainter.GroupSeparator(graphics, colorScheme.Theme, _bounds);
			}
			else if (HasRightSeparator())
			{
				Rectangle groupBounds = _bounds;
				Rectangle rect = groupBounds;
				rect.Y--;
				rect.Height++;
				Blend blend = new Blend(3);
				blend.Positions = new float[3] { 0f, 0.5f, 1f };
				blend.Factors = new float[3] { 0f, 1f, 0f };
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, colorScheme._darkLine, colorScheme._separatorGradientEnd, LinearGradientMode.Vertical);
				linearGradientBrush.Blend = blend;
				Pen pen = new Pen(linearGradientBrush);
				graphics.DrawLine(pen, groupBounds.Right - 2, groupBounds.Y, groupBounds.Right - 2, groupBounds.Bottom - 1);
				LinearGradientBrush edgeGradientBrush = new LinearGradientBrush(rect, colorScheme._separatorOuterStart, colorScheme._separatorOuterEnd, LinearGradientMode.Vertical);
				blend = new Blend(3);
				blend.Positions = new float[3] { 0f, 0.5f, 1f };
				blend.Factors = new float[3] { 0f, 0.1f, 1f };
				edgeGradientBrush.Blend = blend;
				Pen edgePen = new Pen(edgeGradientBrush);
				graphics.DrawLine(edgePen, groupBounds.Right - 1, groupBounds.Y, groupBounds.Right - 1, groupBounds.Bottom - 1);
				graphics.DrawLine(edgePen, groupBounds.Right - 3, groupBounds.Y, groupBounds.Right - 3, groupBounds.Bottom - 1);
				edgePen.Dispose();
				pen.Dispose();
				linearGradientBrush.Dispose();
				edgeGradientBrush.Dispose();
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			PaintOffice2010Content(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
			return;
		}

		private void DrawSeparator(Graphics graphics, int x1, Color baseColor)
		{
			int y = _bounds.Top + 1;
			Pen pen = GdiCache.Pen(Color.FromArgb(30, baseColor));
			graphics.DrawLine(pen, x1, y, x1, _bounds.Bottom - 2);
			x1++;
			pen = GdiCache.Pen(Color.FromArgb(50, Color.White));
			graphics.DrawLine(pen, x1, y, x1, _bounds.Bottom - 2);
		}

		public override void MoveY(int deltaY)
		{
			base.MoveY(deltaY);
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				item.MoveY(deltaY);
			}
		}

		internal override CommandDropInfo GetDropInfo(Point pt, CommandBase command)
		{
			if (_childRenderers.Count == 0)
			{
				CommandDropInfo ribbonLayoutState = new CommandDropInfo();
				ribbonLayoutState._dropIndex = 0;
				ribbonLayoutState._dropBounds = _bounds;
				return ribbonLayoutState;
			}
			return null;
		}
	}
}
