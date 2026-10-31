using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class ControlBoxPanelRenderer : CommandBaseRenderer, ICommandDesignTarget
	{
		internal IList<CommandBaseRenderer> _itemRenderers;

		private CommandCollection _commands;

		private CommandBaseEditorDialog _menuOwner;

		private int _smallItemHeight;

		private ControlBoxPanelType _panelType;

		private RibbonContextMenuRenderer _designContextMenuRenderer;

		Control ICommandDesignTarget.OwnerControl => _menuOwner;

		ImageList ICommandDesignTarget.DesignerImageList
		{
			get
			{
				if (_menuOwner != null && _menuOwner.GetRibbon() != null)
				{
					return _menuOwner.GetRibbon()._designerImageList;
				}
				return null;
			}
		}

		public override AccessibleRole AccessibleRole => AccessibleRole.Grouping;

		public ControlBoxPanelRenderer(CommandCollection commands, ICommandRenderHost owner, CommandBaseEditorDialog menuOwner, int smallItemHeight, ControlBoxPanelType panelType)
			: base(null, owner, null)
		{
			_commands = commands;
			_menuOwner = menuOwner;
			_smallItemHeight = smallItemHeight;
			_panelType = panelType;
		}

		public override void Dispose()
		{
			DisposePressTimer();
			_commandBaseRendererCommand = null;
			_commands = null;
			_layoutItemOwner = null;
			_menuOwner = null;
			SetRenderHost(null);
			_itemRenderers?.Clear();
			SetDesignContextMenuRenderer(null);
		}

		bool ICommandDesignTarget.CanAddCommand(CommandType commandType)
		{
			if (_panelType == ControlBoxPanelType.Bottom && commandType != CommandType.SmallButton)
			{
				return false;
			}
			return true;
		}

		void ICommandDesignTarget.AddNewCommand(CommandBase command, bool editItem)
		{
			if (_menuOwner != null)
			{
				_menuOwner.AddCommandToCollection(command, _commands, editItem);
			}
		}

		Point ICommandDesignTarget.PointToScreen(Point clientPoint)
		{
			if (_menuOwner != null)
			{
				return _menuOwner.PointToScreen(clientPoint);
			}
			return clientPoint;
		}

		public RibbonContextMenuRenderer GetDesignContextMenuRenderer()
		{
			if (_designContextMenuRenderer == null)
			{
				_designContextMenuRenderer = new RibbonContextMenuRenderer(GetRenderHost(), this, drawImageArea: false);
			}
			return _designContextMenuRenderer;
		}

		public void SetDesignContextMenuRenderer(RibbonContextMenuRenderer ribbonContextMenuRenderer)
		{
			if (_designContextMenuRenderer != ribbonContextMenuRenderer)
			{
				if (_designContextMenuRenderer != null)
				{
					_designContextMenuRenderer.Dispose();
				}
				_designContextMenuRenderer = ribbonContextMenuRenderer;
			}
		}

		protected CommandBaseRenderer CreateItemRenderer(CommandBase command)
		{
			if (command == _menuOwner._inPlaceEditCommand)
			{
				CommandSizeStyle sizeStyle = CommandSizeStyle.Small;
				if (command is ButtonCommand && _panelType == ControlBoxPanelType.Left)
				{
					sizeStyle = CommandSizeStyle.Large;
				}
				_menuOwner._ownerItemRenderer = new PopupOwnerItemRenderer(command, _menuOwner, sizeStyle);
				return _menuOwner._ownerItemRenderer;
			}
			switch (command.CommandType)
			{
			case CommandType.Gallery:
				return new GalleryMenuRenderer(command as Gallery, GetRenderHost(), -1, _menuOwner);
			case CommandType.ContainerControl:
				return new MenuContainerControlRenderer(command as ContainerControlCommand, GetRenderHost(), _panelType != ControlBoxPanelType.Bottom);
			case CommandType.Separator:
			{
				RibbonForegroundStyleRenderer ribbonForegroundStyleRenderer = new RibbonForegroundStyleRenderer(command, GetRenderHost());
				if (_panelType == ControlBoxPanelType.Right)
				{
					ribbonForegroundStyleRenderer._isInControlBoxPanel = true;
				}
				return ribbonForegroundStyleRenderer;
			}
			default:
				if (_panelType == ControlBoxPanelType.Left)
				{
					return new LargeMenuButtonRenderer(command as ButtonCommand, GetRenderHost());
				}
				if (_panelType == ControlBoxPanelType.Bottom)
				{
					return new ButtonCommandRenderer(command as ButtonCommand, GetRenderHost(), _menuOwner._owner.SmallImageSize);
				}
				return new ControlBoxPanelButtonRenderer(command as ButtonCommand, GetRenderHost());
			}
		}

		protected int GetItemHeight(CommandBaseRenderer commandBaseRenderer, int defaultHeight)
		{
			if (commandBaseRenderer is PopupOwnerItemRenderer)
			{
				return ((PopupOwnerItemRenderer)commandBaseRenderer)._preferredHeight;
			}
			switch (commandBaseRenderer.CommandType)
			{
			case CommandType.LargeButton:
				return ((LargeMenuButtonRenderer)commandBaseRenderer)._contentHeight;
			case CommandType.SmallButton:
				return defaultHeight;
			case CommandType.Gallery:
				return ((GalleryMenuRenderer)commandBaseRenderer).GetPreferredSize().Height;
			case CommandType.Separator:
				if (commandBaseRenderer is MenuSectionSeparatorRenderer)
				{
					return 2;
				}
				return ((RibbonForegroundStyleRenderer)commandBaseRenderer).GetPreferredHeight(_menuOwner.Font);
			default:
				return defaultHeight;
			}
		}

		public int GetPanelHeight(int defaultHeight)
		{
			int totalHeight = 0;
			if (_panelType == ControlBoxPanelType.Bottom)
			{
				return defaultHeight;
			}
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				totalHeight += GetItemHeight(item, defaultHeight);
			}
			return totalHeight;
		}

		internal CommandBaseRenderer FindItemAt(int x, int y)
		{
			if (_itemRenderers != null)
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item._bounds.Contains(x, y))
					{
						return item;
					}
				}
			}
			return null;
		}

		internal CommandBaseRenderer GetLastNonSeparatorItem()
		{
			for (int index = _itemRenderers.Count - 1; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _itemRenderers[index];
				if (thisValue.CommandType != CommandType.Separator)
				{
					return thisValue;
				}
			}
			return null;
		}

		public bool TryActivateMnemonic(char character)
		{
			for (int i = 0; i < _itemRenderers.Count; i++)
			{
				CommandBase commandBase = _itemRenderers[i].GetCommand();
				if (commandBase == null || !commandBase.HasMnemonic || !commandBase.IsEnabled || !commandBase.Visible || !(commandBase is ButtonCommand))
				{
					continue;
				}
				char c = char.ToLower(((ButtonCommand)commandBase).Mnemonic);
				if (character == c)
				{
					_menuOwner.FocusItem = _itemRenderers[i];
					if (_menuOwner.FocusItem.HasChildren)
					{
						_menuOwner.ShowChildMenu(_menuOwner.FocusItem, selectItem: true);
					}
					else
					{
						_menuOwner.StopHoverTimer();
						commandBase.InvokeOnClick();
					}
					return true;
				}
			}
			return false;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_itemRenderers = new List<CommandBaseRenderer>();
			foreach (CommandBase command in _commands)
			{
				if (command.Visible)
				{
					CommandBaseRenderer item = CreateItemRenderer(command);
					_itemRenderers.Add(item);
				}
			}
			if (_menuOwner._isDesignMode)
			{
				_itemRenderers.Add(GetDesignContextMenuRenderer());
			}
			int panelWidth = 0;
			foreach (CommandBaseRenderer itemRenderer in _itemRenderers)
			{
				itemRenderer.CalculateFullItemWidth(graphics, textRenderer, _menuOwner.Font);
				if (_panelType == ControlBoxPanelType.Bottom)
				{
					panelWidth += itemRenderer._fullItemWidth + 6;
				}
				else if (itemRenderer._fullItemWidth > panelWidth)
				{
					panelWidth = itemRenderer._fullItemWidth;
				}
			}
			_fullItemWidth = panelWidth;
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int panelHeight)
		{
			_bounds = new Rectangle(left, top, width, panelHeight);
			int currentTop = top;
			if (_panelType != ControlBoxPanelType.Bottom)
			{
				for (int i = 0; i < _itemRenderers.Count; i++)
				{
					CommandBaseRenderer thisValue = _itemRenderers[i];
					int rowHeight = GetItemHeight(thisValue, _smallItemHeight);
					thisValue.ArrangeItem(graphics, textRenderer, left, currentTop, width, rowHeight);
					if (thisValue is MenuContainerControlRenderer && (thisValue._bounds.Top < _bounds.Top || thisValue._bounds.Bottom > _bounds.Bottom))
					{
						((MenuContainerControlRenderer)thisValue).RemoveControlFromParent();
					}
					currentTop += rowHeight;
				}
			}
			else
			{
				int currentRight = _bounds.Right + 2;
				for (int j = 0; j < _itemRenderers.Count; j++)
				{
					CommandBaseRenderer itemRenderer = _itemRenderers[j];
					int itemHeight = GetItemHeight(itemRenderer, _smallItemHeight);
					currentRight -= itemRenderer._fullItemWidth;
					itemRenderer.ArrangeItem(graphics, textRenderer, currentRight, currentTop, itemRenderer._fullItemWidth, itemHeight);
					currentRight -= 6;
				}
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (_panelType == ControlBoxPanelType.Right)
			{
				Color backgroundColor = colorScheme._controlBoxPanelBackground;
				Brush brush = GdiCache.Brush(backgroundColor);
				graphics.FillRectangle(brush, _bounds);
			}
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				Rectangle rectangle = Rectangle.Intersect(clipRectangle, item._bounds);
				if (rectangle.Width > 0 && rectangle.Height > 0)
				{
					item.Paint(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
				}
			}
		}
	}
}
