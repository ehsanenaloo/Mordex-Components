using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog
	{
		public CommandBaseRenderer FindRendererForCommand(CommandBase commandBase)
		{
			if (_layoutItems != null)
			{
				foreach (CommandBaseRenderer item in _layoutItems)
				{
					if (item._commandBaseRendererCommand == commandBase)
					{
						return item;
					}
				}
				if (GetChildMenu() != null)
				{
					return GetChildMenu().FindRendererForCommand(commandBase);
				}
			}
			return null;
		}

		internal CommandBaseRenderer FindRendererAt(int x, int y)
		{
			if (_layoutItems != null)
			{
				foreach (CommandBaseRenderer item in _layoutItems)
				{
					if (item._bounds.Contains(x, y))
					{
						if (item is ControlBoxPanelRenderer)
						{
							return ((ControlBoxPanelRenderer)item).FindItemAt(x, y);
						}
						return item;
					}
				}
			}
			return null;
		}

		protected CommandBaseRenderer CreateRendererForCommand(CommandBase command)
		{
			if (command == _inPlaceEditCommand)
			{
				CommandSizeStyle commandSizeStyle = CommandSizeStyle.Small;
				if (command is ButtonCommand)
				{
					commandSizeStyle = ((ButtonCommand)command).GetSizeStyle();
					if (commandSizeStyle == CommandSizeStyle.Default)
					{
						commandSizeStyle = _commandSizeStyle;
					}
				}
				_ownerItemRenderer = new PopupOwnerItemRenderer(command, this, commandSizeStyle);
				return _ownerItemRenderer;
			}
			switch (command.CommandType)
			{
			case CommandType.Gallery:
				return new GalleryMenuRenderer(command as Gallery, this, GetGalleryColumnCount(), this);
			case CommandType.ContainerControl:
				return new MenuContainerControlRenderer(command as ContainerControlCommand, this, drawImageMenuBackground: true);
			case CommandType.Separator:
				return new RibbonForegroundStyleRenderer(command, this);
			default:
			{
				CommandSizeStyle sizeStyle = ((ButtonCommand)command).GetSizeStyle();
				if (sizeStyle == CommandSizeStyle.Default)
				{
					sizeStyle = _commandSizeStyle;
				}
				if (sizeStyle == CommandSizeStyle.Large)
				{
					return new LargeMenuButtonRenderer(command as ButtonCommand, this);
				}
				return new SmallMenuButtonRenderer(command as ButtonCommand, this);
			}
			}
		}

		protected int GetItemHeight(CommandBaseRenderer commandBaseRenderer, int defaultHeight)
		{
			switch (commandBaseRenderer.CommandType)
			{
			case CommandType.LargeButton:
				return ((LargeMenuButtonRenderer)commandBaseRenderer)._contentHeight;
			case CommandType.SmallButton:
				return defaultHeight;
			case CommandType.ContainerControl:
				if (commandBaseRenderer is ContainerControlCommandRenderer && ((ContainerControlCommandRenderer)commandBaseRenderer).GetHostedControl() != null)
				{
					int height = ((ContainerControlCommandRenderer)commandBaseRenderer).GetHostedControl().Height;
					if (height < defaultHeight)
					{
						return defaultHeight;
					}
					return height;
				}
				return defaultHeight;
			case CommandType.Gallery:
				return ((GalleryMenuRenderer)commandBaseRenderer).GetPreferredSize().Height;
			case CommandType.Separator:
				if (commandBaseRenderer is MenuSectionSeparatorRenderer)
				{
					return 2;
				}
				return ((RibbonForegroundStyleRenderer)commandBaseRenderer).GetPreferredHeight(Font);
			default:
				return defaultHeight;
			}
		}

		internal void RebuildMdiList()
		{
			if (_owner == null || _sourceItem == null || !(_sourceItem.GetCommand() is MdiListCommand))
			{
				return;
			}
			Form parentForm = _owner.ParentForm;
			if (parentForm == null || parentForm.MdiChildren.Length <= 0)
			{
				return;
			}
			DropDownCommand dropDownCommand = _sourceItem.GetCommand() as DropDownCommand;
			DisposeList();
			_mdiListCommands = new List<CommandBase>();
			if (dropDownCommand.Commands.Count > 0)
			{
				SeparatorCommand item = new SeparatorCommand();
				_mdiListCommands.Add(item);
			}
			int itemNumber = 1;
			bool showFormIcon = ((MdiListCommand)_sourceItem.GetCommand()).ShowFormIcon;
			Form[] mdiChildren = parentForm.MdiChildren;
			foreach (Form form in mdiChildren)
			{
				ButtonCommand buttonCommand = new ButtonCommand();
				buttonCommand.SetParentContainer(dropDownCommand);
				buttonCommand.Text = form.Text;
				buttonCommand.KeyTip = itemNumber.ToString();
				buttonCommand.CheckOnClick = true;
				if (showFormIcon)
				{
					buttonCommand.Icon = form.Icon;
				}
				if (parentForm.ActiveMdiChild == form)
				{
					buttonCommand.Checked = true;
				}
				buttonCommand.Tag = form;
				_mdiListCommands.Add(buttonCommand);
				buttonCommand.Click += OnControlClick;
				itemNumber++;
			}
		}

		private void DisposeList()
		{
			if (_mdiListCommands == null)
			{
				return;
			}
			foreach (CommandBase listCommand in _mdiListCommands)
			{
				if (listCommand is ButtonCommand listButton)
				{
					listButton.Click -= OnControlClick;
				}
				listCommand.Dispose();
			}
			_mdiListCommands = null;
		}

		public virtual void LayoutMenuItems(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_contentBounds = base.ClientRectangle;
			_contentBounds.Inflate(-_contentPadding.Width / 2, 0);
			_contentBounds.Height -= _contentPadding.Height;
			_contentBounds.Y += _contentOffsetY;
			int contentWidth = _contentBounds.Width;
			if (_hasVerticalScrollBar)
			{
				contentWidth -= SystemInformation.VerticalScrollBarWidth;
			}
			int smallItemHeight = _owner.SmallItemHeight;
			if (_dropDownResizeMode != DropDownResizeMode.None)
			{
				if (_dropDownResizeMode == DropDownResizeMode.Height)
				{
					_resizeBarBounds = new Rectangle(_contentBounds.X, base.ClientRectangle.Bottom - 9, _contentBounds.Width, 8);
					_resizeGripBounds = _resizeBarBounds;
				}
				else
				{
					_resizeBarBounds = new Rectangle(_contentBounds.Left, base.ClientRectangle.Bottom - 12, _contentBounds.Width, 11);
					_resizeGripBounds = new Rectangle(_contentBounds.Right - 11, base.ClientRectangle.Bottom - 12, 11, 11);
				}
				_resizeBarBounds.Y--;
				_contentBounds.Height -= _resizeBarBounds.Height;
			}
			else
			{
				_resizeBarBounds = Rectangle.Empty;
				_resizeGripBounds = Rectangle.Empty;
			}
			if (_scrollOffset < 0)
			{
				_scrollOffset = 0;
			}
			int top = _contentBounds.Top;
			int itemTop = top - _scrollOffset;
			for (int i = 0; i < _layoutItems.Count; i++)
			{
				CommandBaseRenderer commandBaseRenderer = (CommandBaseRenderer)_layoutItems[i];
				int itemHeight = GetItemHeight(commandBaseRenderer, smallItemHeight);
				commandBaseRenderer.ArrangeItem(graphics, textRenderer, _contentBounds.Left, itemTop, contentWidth, itemHeight);
				if (commandBaseRenderer is MenuContainerControlRenderer && (commandBaseRenderer._bounds.Top < _contentBounds.Top || commandBaseRenderer._bounds.Bottom > _contentBounds.Bottom))
				{
					((MenuContainerControlRenderer)commandBaseRenderer).RemoveControlFromParent();
				}
				itemTop += itemHeight;
			}
			if (_focusedItem == null && _selectFirstItem)
			{
				foreach (CommandBaseRenderer item in _layoutItems)
				{
					if (item._isEnabled && item.CommandType != CommandType.Separator && (item.CommandType != CommandType.Gallery || ((GalleryMenuRenderer)item).TrySelectFirstItem()))
					{
						FocusItem = item;
						break;
					}
				}
				_selectFirstItem = false;
			}
			if (itemTop < _contentBounds.Bottom)
			{
				_scrollOffset -= _contentBounds.Bottom - itemTop;
			}
			int scrollLineHeight = _owner.SmallItemHeight;
			if (_sourceItem != null && _sourceItem.GetCommand() is DropDownCommand && ((DropDownCommand)_sourceItem.GetCommand()).CommandsSizeStyle == CommandSizeStyle.Large)
			{
				scrollLineHeight = _owner.LargeImageSize.Height + 12;
			}
			if (_hasVerticalScrollBar)
			{
				UpdateScrollBar(_contentBounds, 0, _totalItemHeight - 1, _contentBounds.Height, _scrollOffset, scrollLineHeight);
			}
			ResumeLayout();
			_isLayoutDirty = false;
		}

		protected void AddRendererToSection(CommandBaseRenderer commandBaseRenderer, int index, ArrayList arrayList, ref PopupMenuSection item)
		{
			if (item == null || !item.CanContain(commandBaseRenderer.CommandType))
			{
				if (_sections.Count > 0 && item != null && commandBaseRenderer.CommandType != CommandType.Gallery && item.GetCommandType() != CommandType.Gallery)
				{
					MenuSectionSeparatorRenderer sectionSeparator = new MenuSectionSeparatorRenderer(this);
					((RibbonLayoutItem)sectionSeparator).SetItemIndex(arrayList.Count);
					arrayList.Add(sectionSeparator);
					item.AddItem(sectionSeparator);
				}
				if (commandBaseRenderer.CommandType == CommandType.Separator)
				{
					item = new PopupMenuSection(CommandType.SmallButton);
				}
				else
				{
					item = new PopupMenuSection(commandBaseRenderer.CommandType);
				}
				_sections.Add(item);
			}
			item.AddItem(commandBaseRenderer);
			((RibbonLayoutItem)commandBaseRenderer).SetItemIndex(arrayList.Count);
			arrayList.Insert(index, commandBaseRenderer);
		}

		protected void AddGalleryCommandRenderers(GalleryMenuRenderer galleryMenu, int insertIndex)
		{
			PopupMenuSection currentSection = null;
			foreach (CommandBase command in galleryMenu.GetGallery().Commands)
			{
				if (command.Visible)
				{
					CommandBaseRenderer commandBaseRenderer = CreateRendererForCommand(command);
					AddRendererToSection(commandBaseRenderer, insertIndex, _layoutItems, ref currentSection);
					insertIndex++;
				}
			}
			if (_isDesignMode)
			{
				AddRendererToSection(galleryMenu.GetContextMenuRenderer(), insertIndex, _layoutItems, ref currentSection);
			}
		}

		public virtual void RebuildMenuAndBounds(Graphics graphics)
		{
			_layoutItems = new ArrayList();
			using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
			textLayoutRenderer.SetShowHotkeyPrefix(true);
			int maxItemWidth = 0;
			int popupHeight = _contentPadding.Height;
			_totalItemHeight = 0;
			int smallItemHeight = _owner.SmallItemHeight;
			int itemCount = 0;
			int itemHeight = 0;
			_sections = new List<PopupMenuSection>();
			PopupMenuSection currentSection = null;
			foreach (Control control in base.Controls)
			{
				control.MouseEnter -= OnControlMouseEnter;
				control.MouseLeave -= OnControlMouseLeave;
			}
			base.Controls.Clear();
			if (_parentCommand is Gallery)
			{
				GalleryMenuRenderer galleryMenu = new GalleryMenuRenderer(_parentCommand as Gallery, this, GetGalleryColumnCount(), this, displayDescription: false);
				AddRendererToSection(galleryMenu, 0, _layoutItems, ref currentSection);
			}
			else
			{
				foreach (CommandBase item in _commands)
				{
					if (item.Visible)
					{
						CommandBaseRenderer commandBaseRenderer = CreateRendererForCommand(item);
						AddRendererToSection(commandBaseRenderer, _layoutItems.Count, _layoutItems, ref currentSection);
					}
				}
			}
			if (_isDesignMode && !(_parentCommand is Gallery))
			{
				AddRendererToSection(GetDesignContextMenuRenderer(), _layoutItems.Count, _layoutItems, ref currentSection);
			}
			if (_mdiListCommands != null)
			{
				foreach (CommandBase listCommand in _mdiListCommands)
				{
					CommandBaseRenderer listItemRenderer = CreateRendererForCommand(listCommand);
					AddRendererToSection(listItemRenderer, _layoutItems.Count, _layoutItems, ref currentSection);
				}
			}
			GalleryLastLayoutItemComparer comparer = new GalleryLastLayoutItemComparer();
			_layoutItems.Sort(comparer);
			for (int i = 0; i < _layoutItems.Count; i++)
			{
				CommandBaseRenderer sortedItem = (CommandBaseRenderer)_layoutItems[i];
				if (sortedItem.CommandType == CommandType.Gallery)
				{
					AddGalleryCommandRenderers((GalleryMenuRenderer)sortedItem, i + 1);
				}
			}
			bool hasGallery = false;
			_minPopupWidth = 0;
			int fullHeight = popupHeight;
			foreach (CommandBaseRenderer itemRenderer in _layoutItems)
			{
				itemRenderer.CalculateFullItemWidth(graphics, textLayoutRenderer, _owner.Font);
				itemHeight = GetItemHeight(itemRenderer, smallItemHeight);
				if (itemRenderer.CommandType != CommandType.Gallery)
				{
					itemCount++;
					_totalItemHeight += itemHeight;
					if (itemRenderer._fullItemWidth > _minPopupWidth)
					{
						_minPopupWidth = itemRenderer._fullItemWidth;
					}
				}
				else
				{
					if (_dropDownResizeMode == DropDownResizeMode.None && ((Gallery)itemRenderer.GetCommand()).AllowDropDownResize)
					{
						_dropDownResizeMode = DropDownResizeMode.Height;
					}
					if (_dropDownResizeMode == DropDownResizeMode.Height && ((Gallery)itemRenderer.GetCommand()).AllowResizeColumns)
					{
						_dropDownResizeMode = DropDownResizeMode.Both;
					}
					hasGallery = true;
				}
				if (_maxDropDownItems <= 0 || itemCount <= _maxDropDownItems)
				{
					popupHeight += itemHeight;
				}
				else
				{
					_hasVerticalScrollBar = true;
				}
				if (itemRenderer._fullItemWidth > maxItemWidth)
				{
					maxItemWidth = itemRenderer._fullItemWidth;
				}
			}
			if (!_hasVerticalScrollBar && _dropDownResizeMode != DropDownResizeMode.None && !hasGallery)
			{
				_hasVerticalScrollBar = true;
			}
			fullHeight += _totalItemHeight;
			maxItemWidth += _contentPadding.Width;
			if (_dropDownResizeMode != DropDownResizeMode.None)
			{
				if (_dropDownResizeMode == DropDownResizeMode.Height)
				{
					popupHeight += 8;
					fullHeight += 8;
				}
				else
				{
					popupHeight += 11;
					fullHeight += 11;
				}
			}
			if (_hasVerticalScrollBar)
			{
				maxItemWidth += SystemInformation.VerticalScrollBarWidth;
			}
			if (maxItemWidth < _minPopupSize.Width)
			{
				maxItemWidth = _minPopupSize.Width;
			}
			if (popupHeight < _minPopupSize.Height)
			{
				popupHeight = _minPopupSize.Height;
			}
			Size size = new Size(maxItemWidth, popupHeight);
			Point position = GetPosition(size);
			if (size.Height > _maxPopupHeight)
			{
				size.Height = _maxPopupHeight;
				if (hasGallery)
				{
					int width = size.Width;
					RenderContent(graphics, textLayoutRenderer, _maxPopupHeight, fullHeight, ref width);
					size = new Size(width, size.Height);
				}
				else if (!_hasVerticalScrollBar)
				{
					size.Width += SystemInformation.VerticalScrollBarWidth;
					_hasVerticalScrollBar = true;
				}
				position = GetPosition(size);
			}
			base.Bounds = new Rectangle(position, size);
			LayoutMenuItems(graphics, textLayoutRenderer);
			_isMenuBoundsDirty = false;
		}

		protected virtual Point GetPosition(Size winSize)
		{
			_adjustedAnchorBounds = _anchorScreenBounds;
			Rectangle workingArea = Screen.GetWorkingArea(_adjustedAnchorBounds);
			Point location = _adjustedAnchorBounds.Location;
			if (!_locationOverride.IsEmpty)
			{
				location = _locationOverride;
			}
			if (_isFlipped && !_opensVertically)
			{
				location.X -= _adjustedAnchorBounds.Width;
			}
			if (_isRightToLeft && _opensVertically)
			{
				location.X = _adjustedAnchorBounds.Right - winSize.Width;
				if (location.X < workingArea.X)
				{
					location.X = workingArea.X;
				}
			}
			if (_opensVertically)
			{
				_isFlipped = false;
				if (location.Y + winSize.Height + _adjustedAnchorBounds.Height > workingArea.Bottom && workingArea.Bottom - _adjustedAnchorBounds.Bottom < _adjustedAnchorBounds.Top - workingArea.Top)
				{
					_isFlipped = true;
				}
				if (!_isFlipped)
				{
					if (_sourceItem != null)
					{
						location.Y = _adjustedAnchorBounds.Bottom;
					}
					_maxPopupHeight = workingArea.Bottom - _adjustedAnchorBounds.Bottom;
				}
				else
				{
					if (_upwardOffset > 0)
					{
						_adjustedAnchorBounds.Y -= _upwardOffset;
					}
					if (_sourceItem != null)
					{
						location.Y = _adjustedAnchorBounds.Top - winSize.Height;
					}
					else
					{
						location.Y = _adjustedAnchorBounds.Top - winSize.Height - _anchorSize.Height;
					}
					if (location.Y < 0)
					{
						winSize.Height = location.Y;
						location.Y = 0;
					}
					if (location.Y < workingArea.Top)
					{
						location.Y = workingArea.Top;
					}
					_maxPopupHeight = _adjustedAnchorBounds.Top - workingArea.Top;
				}
				if (_adjustedAnchorBounds.Left + winSize.Width > workingArea.Right)
				{
					location.X = workingArea.Right - winSize.Width;
				}
			}
			else
			{
				if (location.X + winSize.Width + _adjustedAnchorBounds.Width > workingArea.Right && workingArea.Right - _adjustedAnchorBounds.Right < _adjustedAnchorBounds.X - workingArea.X)
				{
					_isFlipped = true;
				}
				if (!_isFlipped)
				{
					location.X = _adjustedAnchorBounds.Right;
				}
				else
				{
					location.X = _adjustedAnchorBounds.Left - winSize.Width;
					if (location.X < workingArea.Left)
					{
						location.X = workingArea.Left;
					}
				}
				if (_adjustedAnchorBounds.Top + winSize.Height > workingArea.Bottom)
				{
					location.Y = workingArea.Bottom - winSize.Height;
				}
				_maxPopupHeight = workingArea.Height;
				if (location.Y < workingArea.Top)
				{
					location.Y = workingArea.Top;
				}
			}
			return location;
		}

		internal void MarkLayoutDirty()
		{
			_isLayoutDirty = true;
			Invalidate(invalidateChildren: true);
		}

		internal void MarkMenuBoundsDirty()
		{
			_isMenuBoundsDirty = true;
			Invalidate(invalidateChildren: true);
		}

		private void ResizeByDrag(Point point)
		{
			_isResizing = true;
			RibbonDrawingUtils.SetCapture(this, true);
			if (_isResizeClamped)
			{
				if (point.Y < _resizeGripBounds.Top)
				{
					return;
				}
				_isResizeClamped = false;
			}
			int widthDelta = 0;
			int heightDelta = point.Y - _resizeStartPoint.Y;
			int minHeight = SystemInformation.VerticalScrollBarArrowHeight * 2 + 4;
			minHeight += _resizeBarBounds.Height;
			if (base.Size.Height + heightDelta < minHeight)
			{
				_isResizeClamped = true;
				point.Y = _resizeGripBounds.Top;
				heightDelta = minHeight - base.Size.Height;
			}
			foreach (CommandBaseRenderer item in _layoutItems)
			{
				if (item.CommandType != CommandType.Gallery)
				{
					continue;
				}
				GalleryMenuRenderer galleryMenu = item as GalleryMenuRenderer;
				if (_dropDownResizeMode == DropDownResizeMode.Both)
				{
					widthDelta = point.X - _resizeStartPoint.X;
					if (base.Size.Width + widthDelta < _minPopupWidth)
					{
						_isResizeClamped = true;
						point.X = _resizeGripBounds.X;
						widthDelta = _minPopupWidth - base.Size.Width;
					}
				}
				Size size = galleryMenu.GetPreferredSize();
				galleryMenu.SetRequestedSize(new Size(galleryMenu.GetPreferredSize().Width + widthDelta, galleryMenu.GetPreferredSize().Height + heightDelta));
				heightDelta = galleryMenu.GetPreferredSize().Height - size.Height;
				widthDelta = galleryMenu.GetPreferredSize().Width - size.Width;
				if (size.Height == galleryMenu.GetPreferredSize().Height && !_resizeGripBounds.Contains(_resizeGripBounds.X, point.Y))
				{
					point.Y = _resizeGripBounds.Top;
				}
				if (size.Width == galleryMenu.GetPreferredSize().Width && point.X < _resizeGripBounds.X && !_resizeGripBounds.Contains(point.X, _resizeGripBounds.Y))
				{
					point.X = _resizeGripBounds.X;
				}
			}
			_resizeStartPoint = point;
			base.Size = new Size(base.Size.Width + widthDelta, base.Size.Height + heightDelta);
			MarkLayoutDirty();
			Update();
			if (_owner != null && _owner.ParentForm != null)
			{
				if (_owner is CommandBaseEditorDialog)
				{
					((CommandBaseEditorDialog)_owner).Update();
				}
				_owner.ParentForm.Invalidate();
				_owner.ParentForm.Update();
			}
		}
	}
}
