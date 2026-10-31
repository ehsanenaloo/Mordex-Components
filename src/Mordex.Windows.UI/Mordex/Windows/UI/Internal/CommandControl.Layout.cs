using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal partial class CommandControl
	{
		private Image GetCommandImage(MxCommand cmd)
		{
			if (cmd.GetEffectiveImage() != null)
			{
				return cmd.GetEffectiveImage();
			}
			ImageList imageList = GetImageList(cmd);
			if (imageList != null)
			{
				if (ResolvePopupView() == Mordex.Windows.UI.CommandBars.View.SmallIcons || _useSmallImages)
				{
					try
					{
						if (cmd.GetEffectiveImageKey().Length > 0 && imageList.Images.ContainsKey(cmd.GetEffectiveImageKey()))
						{
							return imageList.Images[cmd.GetEffectiveImageKey()];
						}
						if (cmd.GetEffectiveImageIndex() >= 0 && cmd.GetEffectiveImageIndex() < imageList.Images.Count)
						{
							return imageList.Images[cmd.GetEffectiveImageIndex()];
						}
					}
					catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is ExternalException)
					{
						// A broken image in the list shows as no image.
						return null;
					}
				}
				else
				{
					try
					{
						if (cmd.GetEffectiveLargeImageKey().Length > 0 && imageList.Images.ContainsKey(cmd.GetEffectiveLargeImageKey()))
						{
							return imageList.Images[cmd.GetEffectiveLargeImageKey()];
						}
						if (cmd.GetEffectiveLargeImageIndex() >= 0 && cmd.GetEffectiveLargeImageIndex() < imageList.Images.Count)
						{
							return imageList.Images[cmd.GetEffectiveLargeImageIndex()];
						}
					}
					catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is ExternalException)
					{
						// A broken image in the list shows as no image.
						return null;
					}
				}
			}
			return null;
		}

		internal FormatStyle GetItemFormatStyle(CommandLayoutState uiCommandLayoutState, FormatStyle formatStyle, bool isPressed, bool isHot, bool isInfrequent, bool isChecked, bool applyAll)
		{
			FormatStyle uiFormatStyle = new FormatStyle(formatStyle);
			if (isInfrequent)
			{
				bool applyInfrequentAll = false;
				applyInfrequentAll = true;
				uiFormatStyle = uiFormatStyle.MergeStyle(_baseSettings.InfrequentItemsFormatStyle, applyInfrequentAll);
			}
			if (!uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().FormatStyle.IsDefaultStyle())
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().FormatStyle, applyAll);
			}
			if (!((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled())
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(_baseSettings.StateStyles.DisabledFormatStyle, applyAll);
				if (!uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().DisabledFormatStyle.IsDefaultStyle())
				{
					uiFormatStyle = uiFormatStyle.MergeStyle(uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().DisabledFormatStyle, applyAll);
				}
			}
			else if (isChecked)
			{
				if (isHot || uiCommandLayoutState == _openItem)
				{
					uiFormatStyle = uiFormatStyle.MergeStyle(_baseSettings.StateStyles.PressedFormatStyle, applyAll);
					if (!uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().PressedFormatStyle.IsDefaultStyle())
					{
						uiFormatStyle = uiFormatStyle.MergeStyle(uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().PressedFormatStyle, applyAll);
					}
				}
				else
				{
					uiFormatStyle = uiFormatStyle.MergeStyle(_baseSettings.StateStyles.SelectedFormatStyle, applyAll);
					if (!uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().SelectedFormatStyle.IsDefaultStyle())
					{
						uiFormatStyle = uiFormatStyle.MergeStyle(uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().SelectedFormatStyle, applyAll);
					}
				}
			}
			else if (!_isExpanding && (isPressed || isHot))
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(_baseSettings.StateStyles.HotFormatStyle, applyAll);
				if (!uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().HotFormatStyle.IsDefaultStyle())
				{
					uiFormatStyle = uiFormatStyle.MergeStyle(uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().HotFormatStyle, applyAll);
				}
			}
			return uiFormatStyle;
		}

		internal Size GetImageSize(Mordex.Windows.UI.CommandBars.View view, MxCommand cmd)
		{
			if (view == Mordex.Windows.UI.CommandBars.View.SmallIcons)
			{
				if (!_owner.SmallImageSize.IsEmpty)
				{
					return _owner.SmallImageSize;
				}
				if (cmd.GetEffectiveIcon() != null)
				{
					return cmd.GetEffectiveIcon().Size;
				}
				if (cmd.GetEffectiveImage() != null)
				{
					return cmd.GetEffectiveImage().Size;
				}
				return GetImageList(cmd)?.ImageSize ?? DefaultImageSize;
			}
			return _owner.LargeImageSize;
		}

		private Size MeasureItem(Graphics graphics, CommandLayoutState uiCommandLayoutState, string text, MxCommand uiCommand, TextLayoutRenderer textLayoutRenderer, FormatStyle baseStyle, ref int maxImageAreaWidth)
		{
			if (uiCommand.IsSeparator())
			{
				if (_canUseThemes)
				{
					return new Size(0, SeparatorHeightThemed);
				}
				return new Size(0, SeparatorHeight);
			}
			if (_isOverflowMenu && text.Length > 0)
			{
				text += " ";
			}
			int width = 0;
			FormatStyle uiFormatStyle = GetItemFormatStyle(uiCommandLayoutState, baseStyle, ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot(), uiCommandLayoutState.GetCommand().IsInfrequent(), uiCommandLayoutState.GetCommand().IsChecked, true);
			Font font = uiFormatStyle.ResolveFont(Font);
			if (text.Length > 0)
			{
				if (uiCommandLayoutState.GetCommand().IsDefaultItem() && uiCommandLayoutState.GetCommand().IsEnabled)
				{
					uiFormatStyle.FontBold = TriState.True;
				}
				uiFormatStyle.DisposeResources();
				font = uiFormatStyle.ResolveFont(Font);
				width = (int)textLayoutRenderer.MeasureText(graphics, text, font, 0).Width + ItemPadding;
			}
			int height = (int)Font.GetHeight(graphics);
			if (!_canUseThemes)
			{
				height = ((height + ItemHeightPadding));
			}
			if (ResolvePopupView() == Mordex.Windows.UI.CommandBars.View.LargeIcons && uiCommand.Description.Length > 0)
			{
				textLayoutRenderer.SetWordWrap(true);
				int descriptionWidth = RenderingHelper.MeasureWrappedTextWidth(graphics, textLayoutRenderer, uiCommand.Description, uiFormatStyle, font, width, 0);
				height += font.Height * 2;
				textLayoutRenderer.SetWordWrap(false);
				if (descriptionWidth > width)
				{
					width = descriptionWidth;
				}
			}
			int imageGap = ImageGap;
			int imageAreaWidth = 0;
			if (uiCommand.GetEffectiveIcon() != null)
			{
				_hasImages = true;
				Size iconSize = GetImageSize(ResolvePopupView(), uiCommand);
				imageAreaWidth = iconSize.Width + imageGap;
				width += imageAreaWidth;
				if (iconSize.Height + 2 > height)
				{
					height = iconSize.Height + 2;
				}
			}
			else
			{
				Image image = GetCommandImage(uiCommand);
				if (image != null)
				{
					Size imageSize = GetImageSize(ResolvePopupView(), uiCommand);
					_hasImages = true;
					imageAreaWidth = imageSize.Width + imageGap;
					width += imageAreaWidth;
					if (imageSize.Height + 2 > height)
					{
						height = imageSize.Height + 2;
					}
				}
				else
				{
					width += maxImageAreaWidth;
				}
			}
			if (uiCommand.Control != null && uiCommand.CommandType != CommandType.ControlPopup && uiCommand.CommandType != CommandType.ColorPickerCommand)
			{
				if (uiCommand.Control is MxComboBase)
				{
					MxComboBase uiComboBase = (MxComboBase)uiCommand.Control;
					uiComboBase.LayoutContent(graphics);
				}
				if (uiCommand.Control.Height + 2 > height)
				{
					height = uiCommand.Control.Height + 2;
				}
				width += uiCommand.Control.Width + ItemPadding;
				if (uiCommand.IsParent)
				{
					width += SubmenuArrowWidth;
				}
			}
			if (maxImageAreaWidth < imageAreaWidth)
			{
				maxImageAreaWidth = imageAreaWidth;
			}
			height += ItemPadding;
			width += ItemPadding;
			if (height % 2 != 0)
			{
				height++;
			}
			if (_canUseThemes)
			{
				height -= 2;
				if (height < MinItemHeight)
				{
					height = MinItemHeight;
				}
			}
			else if (height < MinItemHeightModern)
			{
				if (height < MinItemHeight)
				{
					height = MinItemHeight;
				}
			}
			return new Size(width, height);
		}

		private void UpdateMaxHeight()
		{
			Rectangle workingArea = Screen.GetWorkingArea(_anchorBounds);
			if (_dropsDown)
			{
				if (workingArea.Bottom - _anchorBounds.Bottom < _anchorBounds.Top - workingArea.Top)
				{
					_maxHeight = _anchorBounds.Top - workingArea.Top;
					if (_upwardOffset > 0)
					{
						_maxHeight -= _upwardOffset;
					}
				}
				else
				{
					_maxHeight = workingArea.Bottom - _anchorBounds.Bottom;
				}
				_maxHeight -= _borderSize.Height;
			}
			else
			{
				_maxHeight = workingArea.Height;
				_maxHeight -= _borderSize.Height;
			}
		}

		private Point ComputeMenuLocation(Size size)
		{
			_anchorBounds = _requestedAnchorBounds;
			Rectangle workingArea = Screen.GetWorkingArea(_anchorBounds);
			Point location = _anchorBounds.Location;
			if (_isFlipped && !_dropsDown)
			{
				location.X -= _anchorBounds.Width;
			}
			if ((_isOverflowMenu || _alignRight) && _dropsDown)
			{
				location.X = _anchorBounds.Right - size.Width;
				if (location.X < workingArea.X)
				{
					location.X = workingArea.X;
				}
			}
			if (_dropsDown)
			{
				if (!_isExpanding)
				{
					_isFlipped = false;
					if (location.Y + size.Height + _anchorBounds.Height > workingArea.Bottom && workingArea.Bottom - _anchorBounds.Bottom < _anchorBounds.Top - workingArea.Top)
					{
						_isFlipped = true;
					}
				}
				if (!_isFlipped)
				{
					location.Y = _anchorBounds.Bottom;
					_maxHeight = workingArea.Bottom - _anchorBounds.Bottom;
					if (_parentItem == null)
					{
						location.Y -= MenuOverlap;
					}
				}
				else
				{
					if (_upwardOffset > 0)
					{
						_anchorBounds.Y -= _upwardOffset;
					}
					location.Y = _anchorBounds.Top - size.Height;
					if (location.Y < 0)
					{
						size.Height = location.Y;
						location.Y = 0;
					}
					if (_parentItem == null)
					{
						location.Y += MenuOverlap;
					}
					if (location.Y < workingArea.Top)
					{
						location.Y = workingArea.Top;
					}
					_maxHeight = _anchorBounds.Top - workingArea.Top;
				}
				if (_anchorBounds.Left + size.Width > workingArea.Right)
				{
					if (_owner is MxContextMenu)
					{
						if (!_anchorSize.IsEmpty)
						{
							location.X = _anchorBounds.Right - size.Width;
						}
						else
						{
							location.X = _anchorBounds.X - size.Width;
						}
					}
					else
					{
						location.X = workingArea.Right - size.Width;
					}
				}
				_maxHeight -= _borderSize.Height;
			}
			else
			{
				if (!_isExpanding && location.X + size.Width + _anchorBounds.Width > workingArea.Right && workingArea.Right - _anchorBounds.Right < _anchorBounds.X - workingArea.X)
				{
					_isFlipped = true;
				}
				if (!_isFlipped)
				{
					if (_canUseThemes)
					{
						_anchorBounds.X -= MenuOverlap;
					}
					else if (_owner is CommandControl)
					{
						CommandControl uiCommandControl = (CommandControl)_owner;
						if (uiCommandControl._isOverflowMenu)
						{
							_anchorBounds.X -= MenuOverlap;
						}
					}
					location.X = _anchorBounds.Right;
				}
				else
				{
					if (_canUseThemes)
					{
						_anchorBounds.X += MenuOverlap;
					}
					else if (_owner is CommandControl)
					{
						CommandControl ownerMenu = (CommandControl)_owner;
						if (ownerMenu._isOverflowMenu)
						{
							_anchorBounds.X += MenuOverlap;
						}
					}
					location.X = _anchorBounds.Left - size.Width;
					if (location.X < workingArea.Left)
					{
						location.X = workingArea.Left;
					}
				}
				if (_anchorBounds.Top + size.Height > workingArea.Bottom)
				{
					location.Y = workingArea.Bottom - size.Height;
				}
				_maxHeight = workingArea.Height;
				if (location.Y < workingArea.Top)
				{
					location.Y = workingArea.Top;
				}
				_maxHeight -= _borderSize.Height;
			}
			return location;
		}

		private void SetItemBounds(CommandLayoutState uiCommandLayoutState, int itemWidth, int imageAreaWidth, int checkMarginWidth)
		{
			Rectangle bounds = uiCommandLayoutState._bounds;
			int contentLeft = 0;
			if (checkMarginWidth > 0)
			{
				contentLeft += checkMarginWidth;
				imageAreaWidth -= checkMarginWidth;
				uiCommandLayoutState._checkBounds = new Rectangle(bounds.X, bounds.Y, checkMarginWidth, Math.Max(bounds.Height - 4, checkMarginWidth));
			}
			else
			{
				uiCommandLayoutState._checkBounds = Rectangle.Empty;
			}
			int textLeft = imageAreaWidth + contentLeft;
			bounds.Width = itemWidth;
			uiCommandLayoutState._bounds = bounds;
			uiCommandLayoutState._imageBounds = new Rectangle(bounds.X + contentLeft, bounds.Y, imageAreaWidth, bounds.Height);
			if (!_canUseThemes)
			{
				uiCommandLayoutState._textBounds = new Rectangle(bounds.X + textLeft + TextGap, bounds.Y, bounds.Width - textLeft - TextGap, bounds.Height);
			}
			else
			{
				uiCommandLayoutState._textBounds = new Rectangle(bounds.X + textLeft + TextGapThemed, bounds.Y, bounds.Width - textLeft - TextGapThemed, bounds.Height);
			}
			if (!_isOverflowMenu)
			{
				bounds.Width += SubmenuArrowWidth;
			}
			uiCommandLayoutState._bounds = bounds;
			if (uiCommandLayoutState.GetCommand().Control != null && uiCommandLayoutState.GetCommand().CommandType != CommandType.ControlPopup && uiCommandLayoutState.GetCommand().CommandType != CommandType.ColorPickerCommand)
			{
				Control control = uiCommandLayoutState.GetCommand().Control;
				Size size = control.Size;
				if (!_isOverflowMenu)
				{
					size.Width += SubmenuArrowWidth;
				}
				int y = uiCommandLayoutState._bounds.Y + (uiCommandLayoutState._bounds.Height - size.Height) / 2;
				int controlLeft = uiCommandLayoutState._bounds.Right - size.Width - 2;
				if (uiCommandLayoutState.GetCommand().IsParent)
				{
					controlLeft -= SubmenuArrowWidth;
				}
				control.Location = new Point(controlLeft, y);
				control.Font = Font;
				control.Enabled = ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled();
				if (!base.Controls.Contains(control))
				{
					control.Resize += OnControlResize;
					control.MouseMove += OnControlMouseMove;
					control.LostFocus += OnControlLostFocus;
					base.Controls.Add(control);
				}
				if (GetCommandManagerOwner() != null && control is MxFontPicker)
				{
					((MxFontPicker)control).UseFontInFontNames = GetCommandManagerOwner().UseFontInFontNames;
				}
				_canAnimate = false;
			}
		}

		private void OnControlResize(object sender, EventArgs eventArgs)
		{
			InvalidateLayout();
		}

		internal int LayoutOverflowItems(Graphics graphics, ref int width, int left, int top, TextLayoutRenderer textLayoutRenderer)
		{
			_overflowItems = new MxCollection(this);
			ArrayList arrayList = new ArrayList();
			if (FindOwnerCommandBar().GetHostRow() != null)
			{
				arrayList = FindOwnerCommandBar().GetHostRow().GetBars().GetSortedBars(FindOwnerCommandBar().GetHostRow().GetIsHorizontal(), false);
			}
			else
			{
				arrayList.Add(FindOwnerCommandBar());
			}
			int cursorX = left + 1;
			int cursorY = top + 2;
			int maxRight = width - 2;
			int rowHeight = 0;
			int totalHeight = 0;
			MxCollection rowItems = new MxCollection(null);
			FormatStyle barStyle = null;
			FormatStyle itemStyle = null;
			int insertIndex = 0;
			foreach (MxCommandBar item in arrayList)
			{
				if (item.IsFullShown || item._overflowItems == null)
				{
					continue;
				}
				barStyle = item.BuildCommandStateStyles().FormatStyle;
				itemStyle = null;
				for (int i = 0; i < item._overflowItems.Count; i++)
				{
					CommandLayoutState uiCommandLayoutState = item._overflowItems.GetCommandItemAt(i);
					uiCommandLayoutState.SetHot(false);
					uiCommandLayoutState._ownerCommandBar = item;
					itemStyle = barStyle;
					uiCommandLayoutState._bounds = Rectangle.Empty;
					if (!uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().FormatStyle.IsDefaultStyle())
					{
						itemStyle = itemStyle.MergeStyle(uiCommandLayoutState.GetCommand().GetEffectiveStateStyles().FormatStyle);
					}
					Size size = item.MeasureHorizontalItem(graphics, uiCommandLayoutState, cursorX, cursorY, textLayoutRenderer, itemStyle);
					if (size.Width > maxRight - (left + 1))
					{
						width = size.Width + ItemPadding;
						return LayoutOverflowItems(graphics, ref width, left, top, textLayoutRenderer);
					}
					cursorX += size.Width;
					if (cursorX > maxRight)
					{
						MxCommandBar.CenterItemsInRow(rowHeight, rowItems, TextAlign.Right);
						rowItems.ClearItems();
						cursorY += rowHeight;
						cursorX = left + 1;
						totalHeight += rowHeight;
						rowHeight = 0;
						i--;
						continue;
					}
					if (size.Height > rowHeight)
					{
						rowHeight = size.Height;
					}
					uiCommandLayoutState.SetHasDropDown(uiCommandLayoutState.GetCommand().IsParent);
					if (uiCommandLayoutState.GetCommand().Control != null && !uiCommandLayoutState.HasDropDown())
					{
						Control control = uiCommandLayoutState.GetCommand().Control;
						int x = uiCommandLayoutState._bounds.X + (uiCommandLayoutState._bounds.Width - control.Width) / 2;
						int y = uiCommandLayoutState._bounds.Y + (uiCommandLayoutState._bounds.Height - control.Height) / 2;
						Rectangle bounds = new Rectangle(x, y, control.Width, control.Height);
						control.Enabled = ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled();
						control.Font = Font;
						if (!base.Controls.Contains(control))
						{
							control.Resize += OnControlResize;
							control.MouseMove += OnControlMouseMove;
							control.LostFocus += OnControlLostFocus;
							base.Controls.Add(control);
						}
						if (control is MxFontPicker)
						{
							((MxFontPicker)control).UseFontInFontNames = GetCommandManagerOwner().UseFontInFontNames;
						}
						_canAnimate = false;
						control.Bounds = bounds;
					}
					rowItems.AddItem(uiCommandLayoutState);
					uiCommandLayoutState._isOverflowItem = true;
					_overflowItems.AddItem(uiCommandLayoutState);
					_layoutItems.InsertItem(insertIndex, uiCommandLayoutState);
					insertIndex++;
				}
			}
			totalHeight += rowHeight;
			MxCommandBar.CenterItemsInRow(rowHeight, rowItems, TextAlign.Right);
			if (totalHeight > 0)
			{
				totalHeight = ((totalHeight + OverflowBottomPadding));
			}
			foreach (CommandLayoutState layoutItem in _layoutItems)
			{
				if (!layoutItem._isOverflowItem)
				{
					layoutItem._bounds.Y += totalHeight;
					layoutItem._textBounds.Y += totalHeight;
					layoutItem._imageBounds.Y += totalHeight;
				}
			}
			return totalHeight;
		}

		private void OnWindowCommandClick(object sender, CommandEventArgs eventArgs)
		{
			if (eventArgs.Command.Tag != null && eventArgs.Command.Tag is Form)
			{
				Form form = eventArgs.Command.Tag as Form;
				form.Activate();
			}
			_isClosing = true;
		}

		internal void MirrorLayout(Graphics graphics, TextLayoutRenderer textLayoutRenderer)
		{
			try
			{
				int right = _menuClientBounds.Right;
				if (!_sidebarBounds.IsEmpty)
				{
					right -= _sidebarBounds.Width;
					_sidebarBounds = new Rectangle(right, _sidebarBounds.Top, _sidebarBounds.Width, _sidebarBounds.Height);
				}
				right -= _imageAreaBounds.Width;
				_imageAreaBounds = new Rectangle(right, _imageAreaBounds.Top, _imageAreaBounds.Width, _imageAreaBounds.Height);
				foreach (FormatStyleLayoutState item in _layoutItems)
				{
					if (!item._textBounds.IsEmpty)
					{
						int textX = _menuClientBounds.Left + (_menuClientBounds.Width - item._textBounds.Width - item._textBounds.X);
						item._textBounds = new Rectangle(textX, item._textBounds.Y, item._textBounds.Width, item._textBounds.Height);
					}
					if (!item._imageBounds.IsEmpty)
					{
						int imageX = _menuClientBounds.Left + (_menuClientBounds.Width - item._imageBounds.Width - item._imageBounds.X);
						item._imageBounds = new Rectangle(imageX, item._imageBounds.Y, item._imageBounds.Width, item._imageBounds.Height);
					}
					if (item is CommandLayoutState uiCommandLayoutState && uiCommandLayoutState.GetCommand() != null)
					{
						if (uiCommandLayoutState.GetCommand().Control != null)
						{
							Control control = uiCommandLayoutState.GetCommand().Control;
							int controlX = _menuClientBounds.Left + (_menuClientBounds.Width - control.Bounds.Width - control.Bounds.X);
							control.Bounds = new Rectangle(controlX, control.Bounds.Y, control.Bounds.Width, control.Bounds.Height);
						}
						if (!uiCommandLayoutState._checkBounds.IsEmpty)
						{
							int checkX = _menuClientBounds.Left + (_menuClientBounds.Width - uiCommandLayoutState._checkBounds.Width - uiCommandLayoutState._checkBounds.X);
							uiCommandLayoutState._checkBounds = new Rectangle(checkX, uiCommandLayoutState._checkBounds.Y, uiCommandLayoutState._checkBounds.Width, uiCommandLayoutState._checkBounds.Height);
						}
					}
				}
			}
			catch (InvalidOperationException)
			{
				// Moving a hosted control can change the item list during the loop. The next layout pass mirrors it again.
			}
		}

		internal void LayoutItems(Graphics graphics, TextLayoutRenderer textLayoutRenderer)
		{
			bool showInfrequent = true;
			if (_owner != null)
			{
				showInfrequent = _owner.ShowInfrequentItems;
			}
			else if (GetCommandManagerOwner() != null)
			{
				showInfrequent = GetCommandManagerOwner().AlwaysShowFullMenus;
			}
			ArrayList visibleCommands = new ArrayList();
			bool canAddSeparator = false;
			for (int i = 0; i < _commands.Count; i++)
			{
				MxCommand uiCommand = _commands[i];
				if (!uiCommand.IsEffectivelyVisible())
				{
					continue;
				}
				if (uiCommand.IsSeparator())
				{
					if (!canAddSeparator)
					{
						continue;
					}
					canAddSeparator = false;
				}
				else if (!uiCommand.IsInfrequent() || showInfrequent)
				{
					canAddSeparator = true;
				}
				visibleCommands.Add(uiCommand);
			}
			if (visibleCommands.Count > 0)
			{
				MxCommand lastCommand = (MxCommand)visibleCommands[visibleCommands.Count - 1];
				if (lastCommand.IsSeparator())
				{
					visibleCommands.Remove(lastCommand);
				}
			}
			if (_windowCommands != null)
			{
				MxCommand[] windowCommands = _windowCommands;
				foreach (MxCommand windowCommand in windowCommands)
				{
					windowCommand.Click += OnWindowCommandClick;
					visibleCommands.Add(windowCommand);
				}
			}
			_shadowAnchorBounds = _anchorBounds;
			if (_isOverflowMenu && _parentItem != null && _owner is MxCommandBar && FindOwnerCommandBar().GetFloatingWindow() == null)
			{
				Point location = FindOwnerCommandBar().PointToScreen(FindOwnerCommandBar()._customizeItem._bounds.Location);
				_shadowAnchorBounds = new Rectangle(location, FindOwnerCommandBar()._customizeItem._bounds.Size);
			}
			int menuWidth = 0;
			int y = _borderSize.Height;
			int contentHeight = 0;
			_layoutItems.ClearItems();
			bool isClipped = false;
			int fullHeight = _borderSize.Height * 2;
			int imageAreaWidth = 0;
			int scrollOffset = 0;
			int firstItemTop = 0;
			int left = _borderSize.Width;
			if (!_isOverflowMenu && _baseSettings.SidebarVisible == InheritableBoolean.True)
			{
				left += _baseSettings.SidebarWidth;
			}
			if (_parentItem != null && _parentItem.GetCommand().CanFloatChildren() && GetCommandManagerOwner() != null)
			{
				if (_tearOffBar == null)
				{
					_tearOffBar = new FormatStyleLayoutState("FloatingBar", this);
				}
				int tearOffHeight = TearOffBarHeight;
				Rectangle tearOffRect = new Rectangle(left, y, 0, tearOffHeight);
				y += tearOffHeight;
				contentHeight += tearOffHeight;
				_tearOffBar._bounds = tearOffRect;
			}
			else
			{
				_tearOffBar = null;
			}
			Control popupControl = null;
			if (_parentItem != null && _parentItem.GetCommand().Control != null && (_parentItem.GetCommand().CommandType == CommandType.ColorPickerCommand || _parentItem.GetCommand().CommandType == CommandType.ControlPopup))
			{
				popupControl = _parentItem.GetCommand().Control;
				if (popupControl is MxColorPicker)
				{
					MxColorPicker uiColorPicker = (MxColorPicker)popupControl;
					uiColorPicker.BackColor = Color.Transparent;
					uiColorPicker.RecalculateLayout();
				}
				if (!base.Controls.Contains(popupControl))
				{
					base.Controls.Add(popupControl);
					popupControl.Resize += OnControlResize;
				}
				popupControl.Top = y;
				popupControl.Left = left;
				int height = popupControl.Height;
				y += height;
				contentHeight += height;
				menuWidth = popupControl.Width;
				_canAnimate = false;
				_hostsPopupControl = true;
			}
			if (visibleCommands.Count > 0)
			{
				FormatStyle formatStyle = _baseSettings.StateStyles.FormatStyle;
				bool hasHiddenItems = false;
				MxCommand command = null;
				if (_firstVisibleIndex > 0)
				{
					if (_scrollUpButton == null)
					{
						_scrollUpButton = new FormatStyleLayoutState("TopRebar", this);
					}
					int scrollUpHeight = (int)Font.GetHeight(graphics);
					scrollUpHeight = ((!_canUseThemes) ? (scrollUpHeight + ScrollButtonPadding) : (scrollUpHeight + ScrollButtonPaddingThemed));
					Rectangle scrollUpRect = new Rectangle(left, y, 0, scrollUpHeight);
					y += scrollUpHeight;
					contentHeight += scrollUpHeight;
					_scrollUpButton._bounds = scrollUpRect;
				}
				else
				{
					_scrollUpButton = null;
				}
				string text = "";
				int checkMarginWidth = CheckMarginWidth;
				if (!_canUseThemes)
				{
					checkMarginWidth = CheckMarginWidthClassic;
				}
				imageAreaWidth = checkMarginWidth;
				if (_isOverflowMenu)
				{
					imageAreaWidth = 0;
				}
				bool wasInfrequent = false;
				firstItemTop = y;
				bool isCustomizing = GetCommandManagerOwner() != null && GetCommandManagerOwner()._isCustomizing;
				for (int index = _firstVisibleIndex - 1; index >= 0; index--)
				{
					command = (MxCommand)visibleCommands[index];
					CommandLayoutState uiCommandLayoutState = new CommandLayoutState(command, command.GetEffectiveText());
					((FormatStyleLayoutState)uiCommandLayoutState).SetEnabled(command.IsEnabled || isCustomizing);
					text = uiCommandLayoutState._caption;
					if (command.GetEffectiveShortcut() != Shortcut.None)
					{
						uiCommandLayoutState.SetShortcutText(RenderingHelper.GetShortcutText(command.GetEffectiveShortcut()));
						text = text + "   " + uiCommandLayoutState.GetShortcutText();
					}
					Size upperItemSize = MeasureItem(graphics, uiCommandLayoutState, text, command, textLayoutRenderer, formatStyle, ref imageAreaWidth);
					if (upperItemSize.Width > menuWidth)
					{
						menuWidth = upperItemSize.Width;
					}
					if (!command.IsInfrequent() || showInfrequent)
					{
						int upperItemHeight = upperItemSize.Height;
						wasInfrequent = command.IsInfrequent();
						if (command.IsInfrequent() && _isExpanding && upperItemSize.Height > _expandHeight)
						{
							upperItemHeight = _expandHeight;
							isClipped = true;
						}
						y -= upperItemHeight;
						Rectangle upperItemBounds = new Rectangle(left, y, menuWidth, upperItemSize.Height);
						uiCommandLayoutState._bounds = upperItemBounds;
						_layoutItems.AddItem(uiCommandLayoutState);
					}
					else
					{
						hasHiddenItems = true;
					}
				}
				y = firstItemTop;
				wasInfrequent = false;
				for (int itemIndex = _firstVisibleIndex; itemIndex < visibleCommands.Count; itemIndex++)
				{
					command = (MxCommand)visibleCommands[itemIndex];
					CommandLayoutState layoutItem = new CommandLayoutState(command, command.GetEffectiveText());
					((FormatStyleLayoutState)layoutItem).SetEnabled(command.IsEnabled || isCustomizing);
					if (command.IsSeparator())
					{
						((FormatStyleLayoutState)layoutItem).SetEnabled(false);
					}
					text = layoutItem._caption;
					if (text.Length > 0)
					{
						text = ((_canUseThemes) ? (text + "  ") : (text + "   "));
					}
					if (command.GetEffectiveShortcut() != Shortcut.None)
					{
						layoutItem.SetShortcutText(RenderingHelper.GetShortcutText(command.GetEffectiveShortcut()));
						text += layoutItem.GetShortcutText();
					}
					Size itemSize = MeasureItem(graphics, layoutItem, text, command, textLayoutRenderer, formatStyle, ref imageAreaWidth);
					if (itemSize.Width > menuWidth)
					{
						menuWidth = itemSize.Width;
					}
					if (!command.IsInfrequent() || showInfrequent)
					{
						wasInfrequent = command.IsInfrequent();
						int itemHeight = itemSize.Height;
						fullHeight += itemHeight;
						if (command.IsInfrequent() && _isExpanding && itemSize.Height > _expandHeight)
						{
							itemHeight = _expandHeight;
							isClipped = true;
						}
						Rectangle itemBounds = new Rectangle(left, y, menuWidth, itemSize.Height);
						y += itemHeight;
						contentHeight += itemHeight;
						layoutItem._bounds = itemBounds;
						_layoutItems.AddItem(layoutItem);
					}
					else
					{
						hasHiddenItems = true;
					}
				}
				if (_hasCheckMargin)
				{
					if (_hasImages)
					{
						menuWidth += checkMarginWidth;
						imageAreaWidth += checkMarginWidth;
					}
				}
				else
				{
					checkMarginWidth = 0;
				}
				bool addArrowWidth = true;
				if (popupControl != null)
				{
					if ((_canUseThemes))
					{
						addArrowWidth = false;
						if (menuWidth < popupControl.Width + _borderSize.Width)
						{
							menuWidth = popupControl.Width + _borderSize.Width;
							popupControl.Left = (menuWidth + _borderSize.Width * 2 - popupControl.Width) / 2;
						}
						else
						{
							popupControl.Left = (menuWidth + _borderSize.Width * 2 - popupControl.Width) / 2;
						}
					}
					else
					{
						popupControl.Left = imageAreaWidth + left;
						if (menuWidth + SubmenuArrowWidth < popupControl.Width + imageAreaWidth + _borderSize.Width)
						{
							menuWidth = popupControl.Width + imageAreaWidth + _borderSize.Width - SubmenuArrowWidth;
						}
					}
				}
				foreach (CommandLayoutState item in _layoutItems)
				{
					SetItemBounds(item, menuWidth, imageAreaWidth, checkMarginWidth);
				}
				if (!_isOverflowMenu)
				{
					if (addArrowWidth)
					{
						menuWidth += SubmenuArrowWidth;
					}
					if (_baseSettings.SidebarVisible == InheritableBoolean.True)
					{
						menuWidth += _baseSettings.SidebarWidth;
					}
				}
				if (_maxHeight == 0)
				{
					UpdateMaxHeight();
				}
				_visibleItemCount = _layoutItems.Count;
				if (_isExpanding && fullHeight + _borderSize.Height * 2 > _maxHeight)
				{
					_isExpanding = false;
					StopExpandAnimation();
					_expandHeight = -1;
					UpdateMaxHeight();
					LayoutItems(graphics, textLayoutRenderer);
					return;
				}
				int expandBarHeight = 0;
				if (hasHiddenItems)
				{
					_expandBar = new FormatStyleLayoutState("InfrequentItemsBar", this);
					expandBarHeight = (int)Font.GetHeight(graphics);
					int expandBarTop = contentHeight + _borderSize.Height;
					if (expandBarTop + expandBarHeight > _maxHeight)
					{
						expandBarTop = _maxHeight - expandBarHeight;
					}
					Rectangle expandBarBounds = new Rectangle(left, expandBarTop, menuWidth, expandBarHeight);
					_expandBar._bounds = expandBarBounds;
					contentHeight += expandBarHeight;
				}
				else
				{
					_expandBar = null;
					_expandOnHover = false;
				}
				if (contentHeight + _borderSize.Height * 2 > _maxHeight)
				{
					int visibleBottom = -1;
					if (!isClipped)
					{
						if (_scrollDownButton == null)
						{
							_scrollDownButton = new FormatStyleLayoutState("BottomRebar", this);
						}
						_canScrollDown = true;
						int scrollDownHeight = (int)Font.GetHeight(graphics);
						scrollDownHeight = ((!_canUseThemes) ? (scrollDownHeight + ScrollButtonPadding) : (scrollDownHeight + ScrollButtonPaddingThemed));
						int scrollDownTop = _maxHeight - scrollDownHeight - expandBarHeight - _borderSize.Height;
						FormatStyleLayoutState itemAtScrollDown = HitTestItem(left, scrollDownTop);
						if (itemAtScrollDown != null)
						{
							scrollOffset = itemAtScrollDown._bounds.Bottom - scrollDownTop;
						}
						visibleBottom = scrollDownTop;
						Rectangle scrollDownBounds = new Rectangle(left, scrollDownTop, menuWidth, scrollDownHeight);
						_scrollDownButton._bounds = scrollDownBounds;
						if (_alignItemsToTop)
						{
							contentHeight = _maxHeight;
						}
						else
						{
							visibleBottom += scrollOffset;
						}
					}
					else
					{
						_canScrollDown = false;
						contentHeight = _maxHeight;
						_scrollDownButton = null;
					}
					if (visibleBottom > 0)
					{
						foreach (FormatStyleLayoutState clippedItem in _layoutItems)
						{
							if (clippedItem._bounds.Bottom > visibleBottom)
							{
								clippedItem.SetVisible(false);
								_visibleItemCount--;
							}
						}
					}
				}
				else
				{
					_canScrollDown = false;
					_scrollDownButton = null;
				}
				if (_tearOffBar != null)
				{
					Rectangle tearOffBounds = _tearOffBar._bounds;
					tearOffBounds.Width = menuWidth;
					_tearOffBar._bounds = tearOffBounds;
				}
				if (_scrollUpButton != null)
				{
					Rectangle scrollUpBounds = _scrollUpButton._bounds;
					scrollUpBounds.Width = menuWidth;
					_scrollUpButton._bounds = scrollUpBounds;
					if (_scrollDownButton == null)
					{
						scrollOffset = contentHeight - _maxHeight;
					}
					if (!_alignItemsToTop)
					{
						contentHeight = _maxHeight;
					}
					_visibleItemCount = 0;
					if (!_alignItemsToTop)
					{
						foreach (FormatStyleLayoutState scrolledItem in _layoutItems)
						{
							scrolledItem._bounds.Y -= scrollOffset;
							scrolledItem._textBounds.Y -= scrollOffset;
							scrolledItem._imageBounds.Y -= scrollOffset;
							if (scrolledItem._bounds.Y < scrollUpBounds.Bottom)
							{
								scrolledItem.SetVisible(false);
								_visibleItemCount--;
							}
						}
					}
					else
					{
						foreach (FormatStyleLayoutState coveredItem in _layoutItems)
						{
							if (coveredItem._bounds.Y < scrollUpBounds.Bottom)
							{
								coveredItem.SetVisible(false);
								_visibleItemCount--;
							}
						}
					}
				}
				if (_selectedItem == null && _selectFirstItem && _layoutItems.Count > 0)
				{
					foreach (CommandLayoutState firstItem in _layoutItems)
					{
						if (((FormatStyleLayoutState)firstItem).IsEnabled() && ((FormatStyleLayoutState)firstItem).IsVisible() && !firstItem.GetCommand().IsSeparator())
						{
							SelectItem(firstItem);
							break;
						}
					}
					_selectFirstItem = false;
				}
			}
			if (_expandTimer != null && _expandTimer.Enabled)
			{
				if (!isClipped)
				{
					_isExpanding = false;
					StopExpandAnimation();
					_expandHeight = -1;
				}
				else if (_isFlipped && _menuSize.Height > contentHeight + _borderSize.Height * 2)
				{
					contentHeight = _menuSize.Height - _borderSize.Height * 2;
				}
			}
			if (!_isOverflowMenu)
			{
				int sidebarWidth = 0;
				if (_baseSettings.SidebarVisible == InheritableBoolean.True)
				{
					sidebarWidth = _baseSettings.SidebarWidth;
					if (sidebarWidth < 0)
					{
						sidebarWidth = 0;
					}
				}
				if (sidebarWidth <= 0)
				{
					_sidebarBounds = Rectangle.Empty;
				}
				int sideAreaTop = 1;
				if (!(_owner is MxCommandBar))
				{
					sideAreaTop = ((_tearOffBar == null) ? SideAreaTopInPopup : SideAreaTopWithTearOff);
				}
				else if (_tearOffBar != null)
				{
					sideAreaTop = SideAreaTopInBarWithTearOff;
				}
				if (sidebarWidth > 0)
				{
					_sidebarBounds = new Rectangle(1, sideAreaTop, sidebarWidth, contentHeight - sideAreaTop);
				}
				_imageAreaBounds = new Rectangle(1 + sidebarWidth, sideAreaTop, imageAreaWidth + left - _borderSize.Width - sidebarWidth, contentHeight - sideAreaTop);
			}
			if (_isOverflowMenu)
			{
				y = _borderSize.Height;
				if (_tearOffBar != null)
				{
					y = _tearOffBar._bounds.Bottom;
				}
				contentHeight += LayoutOverflowItems(graphics, ref menuWidth, left, y, textLayoutRenderer);
			}
			_menuSize = new Size(menuWidth + _borderSize.Width * 2, contentHeight + _borderSize.Height * 2);
			_menuLocation = ComputeMenuLocation(_menuSize);
			if (_owner is MxCommandBar && _isFlipped)
			{
				_imageAreaBounds.Y++;
			}
			if (_selectedItem == null)
			{
				if (_pendingSelectionIndex != -1)
				{
					SelectItem((CommandLayoutState)_layoutItems.FindItemByIndex(_pendingSelectionIndex));
					_pendingSelectionIndex = -1;
				}
				else if (_pendingSelectedCommand != null)
				{
					SelectItem(_layoutItems.FindCommandItem(_pendingSelectedCommand));
					_pendingSelectedCommand = null;
				}
			}
			if (_openItem != null)
			{
				SetOpenItem(_layoutItems.FindCommandItem(_openItem.GetCommand()));
			}
			_menuClientBounds = new Rectangle(new Point(0, 0), _menuSize);
			_isLayoutDirty = false;
			if (RightToLeft == RightToLeft.Yes)
			{
				MirrorLayout(graphics, textLayoutRenderer);
			}
			base.Bounds = new Rectangle(_menuLocation, _menuSize);
		}

		internal void LayoutMenu(Graphics graphics)
		{
			TextLayoutRenderer textLayoutRenderer = CreateTextRenderer();
			LayoutItems(graphics, textLayoutRenderer);
			textLayoutRenderer.ResetCache();
		}
	}
}
