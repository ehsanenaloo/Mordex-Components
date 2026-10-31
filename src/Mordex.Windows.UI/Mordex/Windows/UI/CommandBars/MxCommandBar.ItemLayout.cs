using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		internal ImageList ResolveImageList(View view, MxCommand uiCommand)
		{
			if (view == View.SmallIcons)
			{
				if (_imageList != null)
				{
					return _imageList;
				}
				if (_commandManager != null)
				{
					if (uiCommand != null && uiCommand.IsMergedCommand && _commandManager._mergedSmallImageList != null)
					{
						return _commandManager._mergedSmallImageList;
					}
					return _commandManager.ImageList;
				}
			}
			else
			{
				if (_largeImageList != null)
				{
					return _largeImageList;
				}
				if (_commandManager != null)
				{
					if (uiCommand != null && uiCommand.IsMergedCommand && _commandManager._mergedLargeImageList != null)
					{
						return _commandManager._mergedLargeImageList;
					}
					return _commandManager.LargeImageList;
				}
			}
			return null;
		}

		internal void InvalidateLayout()
		{
			if (GetFloatingWindow() != null)
			{
				GetFloatingWindow().Invalidate();
			}
			if (GetChildMenu() != null)
			{
				GetChildMenu().InvalidateLayout();
			}
			_isLayoutDirty = true;
			Invalidate(invalidateChildren: true);
		}

		private Image GetCommandImage(MxCommand uiCommand)
		{
			if (uiCommand.GetEffectiveImage() != null)
			{
				return uiCommand.GetEffectiveImage();
			}
			ImageList imageList = ResolveImageList(View.SmallIcons, uiCommand);
			ImageList largeImageList = ResolveImageList(View.LargeIcons, uiCommand);
			if (GetEffectiveView() == View.SmallIcons || largeImageList == null)
			{
				if (imageList != null)
				{
					try
					{
						if (uiCommand.GetEffectiveImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveImageKey()))
						{
							return imageList.Images[uiCommand.GetEffectiveImageKey()];
						}
						if (uiCommand.GetEffectiveImageIndex() >= 0 && uiCommand.GetEffectiveImageIndex() < imageList.Images.Count)
						{
							return imageList.Images[uiCommand.GetEffectiveImageIndex()];
						}
					}
					catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
					{
						// The image list can change while a command paints; draw it without an image.
						return null;
					}
				}
			}
			else if (largeImageList != null)
			{
				try
				{
					if (uiCommand.GetEffectiveLargeImageKey().Length > 0 && largeImageList.Images.ContainsKey(uiCommand.GetEffectiveLargeImageKey()))
					{
						return largeImageList.Images[uiCommand.GetEffectiveLargeImageKey()];
					}
					if (uiCommand.GetEffectiveLargeImageIndex() >= 0 && uiCommand.GetEffectiveLargeImageIndex() < largeImageList.Images.Count)
					{
						return largeImageList.Images[uiCommand.GetEffectiveLargeImageIndex()];
					}
				}
				catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
				{
					// The image list can change while a command paints; draw it without an image.
					return null;
				}
			}
			return null;
		}

		private void ApplyHostedControlStyle(Graphics graphics, Control control)
		{
			if (control is MxComboBase)
			{
				MxComboBase uiComboBase = (MxComboBase)control;
			}
			else if (control is MxColorPicker)
			{
				MxColorPicker uiColorPicker = (MxColorPicker)control;
				uiColorPicker.BackColor = Color.Transparent;
				uiColorPicker.RecalculateLayout();
			}
		}

		internal Size GetImageSize(View view, MxCommand uiCommand)
		{
			if (view == View.SmallIcons)
			{
				if (!CommandManager.SmallImageSize.IsEmpty)
				{
					return CommandManager.SmallImageSize;
				}
				if (uiCommand.GetEffectiveIcon() != null)
				{
					return uiCommand.GetEffectiveIcon().Size;
				}
				if (uiCommand.GetEffectiveImage() != null)
				{
					return uiCommand.GetEffectiveImage().Size;
				}
				return ResolveImageList(view, uiCommand)?.ImageSize ?? DefaultSmallImageSize;
			}
			if (!CommandManager.LargeImageSize.IsEmpty)
			{
				return CommandManager.LargeImageSize;
			}
			if (uiCommand.GetEffectiveIcon() != null)
			{
				return uiCommand.GetEffectiveIcon().Size;
			}
			if (uiCommand.GetEffectiveImage() != null)
			{
				return uiCommand.GetEffectiveImage().Size;
			}
			return ResolveImageList(view, uiCommand)?.ImageSize ?? DefaultLargeImageSize;
		}

		private void AlignImage(CommandLayoutState uiCommandLayoutState, ContentAlignment contentAlignment, Size size)
		{
			int x = uiCommandLayoutState._imageBounds.X;
			int y = uiCommandLayoutState._imageBounds.Y;
			int width = uiCommandLayoutState._imageBounds.Width;
			int height = uiCommandLayoutState._imageBounds.Height;
			switch (contentAlignment)
			{
			case ContentAlignment.BottomCenter:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.TopCenter:
				x += (width - size.Width) / 2;
				break;
			case ContentAlignment.BottomLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.TopLeft:
				x += 2;
				break;
			case ContentAlignment.BottomRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.TopRight:
				x = x + width - size.Width - 2;
				break;
			}
			switch (contentAlignment)
			{
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.MiddleRight:
				y += (height - size.Height) / 2;
				break;
			case ContentAlignment.TopCenter:
			case ContentAlignment.TopLeft:
			case ContentAlignment.TopRight:
				y += 2;
				break;
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomRight:
				y = y + height - size.Height - 2;
				break;
			}
			uiCommandLayoutState._imageBounds = new Rectangle(x, y, size.Width, size.Height);
		}

		private void ArrangeVerticalItem(CommandLayoutState uiCommandLayoutState, int left, int top, Size size, Size textSize)
		{
			MxCommand uiCommand = uiCommandLayoutState.GetCommand();
			TextImageRelation textImageRelation = uiCommand.GetEffectiveTextImageRelation();
			if (textImageRelation == TextImageRelation.Default)
			{
				textImageRelation = ((GetEffectiveTextAlign() == TextAlign.Underneath) ? TextImageRelation.ImageAboveText : TextImageRelation.ImageBeforeText);
			}
			if (!size.IsEmpty)
			{
				size = new Size(size.Width + ImagePadding, size.Height + ImagePadding);
			}
			int width = size.Width;
			int height = size.Height;
			if (textSize.Height > height)
			{
				height = textSize.Height;
			}
			if (textSize.Width > width)
			{
				width = textSize.Width;
			}
			width += 2;
			height += 2;
			switch (textImageRelation)
			{
			case TextImageRelation.Overlay:
				uiCommandLayoutState._imageBounds = new Rectangle(left, top, width, height);
				uiCommandLayoutState._textBounds = new Rectangle(left, top, width, height);
				uiCommandLayoutState._bounds = new Rectangle(left, top, width, height);
				break;
			case TextImageRelation.ImageAboveText:
				uiCommandLayoutState._imageBounds = new Rectangle(left, top, width, size.Height);
				uiCommandLayoutState._textBounds = new Rectangle(left, top + size.Height, width, textSize.Height);
				uiCommandLayoutState._bounds = new Rectangle(left, top, width, size.Height + textSize.Height);
				break;
			case TextImageRelation.ImageBeforeText:
				uiCommandLayoutState._imageBounds = new Rectangle(left, top, width, size.Height);
				if (!size.IsEmpty)
				{
					uiCommandLayoutState._textBounds = new Rectangle(left, top + size.Height - 2, width, textSize.Height);
				}
				else
				{
					uiCommandLayoutState._textBounds = new Rectangle(left, top, width, textSize.Height);
				}
				uiCommandLayoutState._bounds = new Rectangle(left, top, width, size.Height + textSize.Height);
				break;
			case TextImageRelation.TextAboveImage:
				uiCommandLayoutState._textBounds = new Rectangle(left, top, textSize.Width, height);
				uiCommandLayoutState._imageBounds = new Rectangle(left + textSize.Width, top, size.Width, height);
				uiCommandLayoutState._bounds = new Rectangle(left, top, size.Width + textSize.Width, height);
				break;
			case TextImageRelation.TextBeforeImage:
				uiCommandLayoutState._textBounds = new Rectangle(left, top, width, textSize.Height);
				uiCommandLayoutState._imageBounds = new Rectangle(left, top + textSize.Height, width, size.Height);
				uiCommandLayoutState._bounds = new Rectangle(left, top, width, size.Height + textSize.Height);
				break;
			}
			if (!size.IsEmpty)
			{
				AlignImage(uiCommandLayoutState, uiCommand.GetEffectiveImageAlignment(), new Size(size.Width - ImagePadding, size.Height - ImagePadding));
			}
		}

		private void ArrangeHorizontalItem(CommandLayoutState uiCommandLayoutState, int left, int top, Size size, Size textSize)
		{
			MxCommand uiCommand = uiCommandLayoutState.GetCommand();
			TextImageRelation textImageRelation = uiCommand.GetEffectiveTextImageRelation();
			if (textImageRelation == TextImageRelation.Default)
			{
				textImageRelation = ((GetEffectiveTextAlign() == TextAlign.Underneath) ? TextImageRelation.ImageAboveText : TextImageRelation.ImageBeforeText);
			}
			if (!size.IsEmpty)
			{
				size = new Size(size.Width + HorizontalImagePadding, size.Height + ImagePadding);
			}
			int width = size.Width;
			int height = size.Height;
			if (textSize.Height > height)
			{
				height = textSize.Height;
			}
			if (textSize.Width > width)
			{
				width = textSize.Width;
			}
			width += 3;
			height += 2;
			switch (textImageRelation)
			{
			case TextImageRelation.Overlay:
				uiCommandLayoutState._imageBounds = new Rectangle(left, top, width, height);
				uiCommandLayoutState._textBounds = new Rectangle(left, top, width, height);
				uiCommandLayoutState._bounds = new Rectangle(left, top, width, height);
				break;
			case TextImageRelation.ImageAboveText:
				uiCommandLayoutState._imageBounds = new Rectangle(left, top, width, size.Height);
				uiCommandLayoutState._textBounds = new Rectangle(left, top + size.Height, width, textSize.Height);
				uiCommandLayoutState._bounds = new Rectangle(left, top, width, size.Height + textSize.Height);
				break;
			case TextImageRelation.ImageBeforeText:
				uiCommandLayoutState._imageBounds = new Rectangle(left, top, size.Width, height);
				if (!size.IsEmpty)
				{
					uiCommandLayoutState._textBounds = new Rectangle(left + size.Width - 2, top, textSize.Width, height);
				}
				else
				{
					uiCommandLayoutState._textBounds = new Rectangle(left, top, textSize.Width, height);
				}
				uiCommandLayoutState._bounds = new Rectangle(left, top, size.Width + textSize.Width, height);
				break;
			case TextImageRelation.TextAboveImage:
				uiCommandLayoutState._textBounds = new Rectangle(left, top, width, textSize.Height);
				uiCommandLayoutState._imageBounds = new Rectangle(left, top + textSize.Height, width, size.Height);
				uiCommandLayoutState._bounds = new Rectangle(left, top, width, size.Height + textSize.Height);
				break;
			case TextImageRelation.TextBeforeImage:
				uiCommandLayoutState._textBounds = new Rectangle(left, top, textSize.Width, height);
				uiCommandLayoutState._imageBounds = new Rectangle(left + textSize.Width, top, size.Width, height);
				uiCommandLayoutState._bounds = new Rectangle(left, top, size.Width + textSize.Width, height);
				break;
			}
			if (!size.IsEmpty)
			{
				AlignImage(uiCommandLayoutState, uiCommand.GetEffectiveImageAlignment(), new Size(size.Width - HorizontalImagePadding, size.Height - ImagePadding));
			}
		}

		internal Size MeasureVerticalItem(Graphics graphics, CommandLayoutState uiCommandLayoutState, int startPosition, int offset, TextLayoutRenderer textLayoutRenderer, FormatStyle uiFormatStyle)
		{
			int position = startPosition;
			return LayoutVerticalItem(graphics, uiCommandLayoutState, ref position, ref position, offset, CommandAlignment.Near, textLayoutRenderer, uiFormatStyle);
		}

		internal Size LayoutVerticalItem(Graphics graphics, CommandLayoutState uiCommandLayoutState, ref int nearPosition, ref int farPosition, int offset, CommandAlignment commandAlignment, TextLayoutRenderer textLayoutRenderer, FormatStyle uiFormatStyle)
		{
			MxCommand uiCommand = uiCommandLayoutState.GetCommand();
			CommandStyle commandStyle = GetEffectiveCommandStyle(uiCommand);
			bool isNear = commandAlignment == CommandAlignment.Near;
			int position = nearPosition;
			if (!isNear)
			{
				position = farPosition;
			}
			if (uiCommand.IsSeparator())
			{
				((FormatStyleLayoutState)uiCommandLayoutState).SetEnabled(false);
				uiCommandLayoutState._bounds = new Rectangle(offset, position, 0, SeparatorThickness);
			}
			else
			{
				Size size = Size.Empty;
				Size textSize = Size.Empty;
				if (commandStyle != CommandStyle.Text)
				{
					if (uiCommand.GetEffectiveIcon() != null)
					{
						size = GetImageSize(GetEffectiveView(), uiCommand);
					}
					else
					{
						Image image = GetCommandImage(uiCommand);
						if (image != null)
						{
							uiCommandLayoutState._itemImage = image;
							size = GetImageSize(GetEffectiveView(), uiCommand);
						}
					}
				}
				if (commandStyle != CommandStyle.Image && uiCommand.GetEffectiveText().Length > 0)
				{
					Font font = uiFormatStyle.ResolveFont(Font);
					textSize = textLayoutRenderer.MeasureText(graphics, uiCommand.GetEffectiveText(), font, 0).ToSize();
					if (_commandBarType == CommandBarType.Menu && (commandStyle == CommandStyle.Text || (size.IsEmpty && commandStyle == CommandStyle.TextImage)))
					{
						textSize.Height += MenuTextExtraHeight;
					}
					textSize.Height++;
					textSize.Width += TextPadding;
				}
				ArrangeVerticalItem(uiCommandLayoutState, offset, position, size, textSize);
			}
			int itemWidth = uiCommandLayoutState._bounds.Width;
			int itemHeight = uiCommandLayoutState._bounds.Height;
			if (!uiCommand.IsSeparator())
			{
				int minWidth = MinItemLength;
				int minHeight = MinItemLength;
				if (IsThemed())
				{
					minHeight = MinItemThickness;
				}
				if (itemWidth < minWidth)
				{
					itemWidth = minWidth;
				}
				if (itemHeight < minHeight)
				{
					itemHeight = minHeight;
				}
			}
			if (uiCommand.IsParent && _commandBarType == CommandBarType.ToolBar && uiCommand.GetEffectiveDropDownButtonStyle() == DropDownButtonStyle.SplitButton)
			{
				int arrowWidth = (_isThemed) ? ThemedSplitArrowWidth : SplitArrowWidth;
				uiCommandLayoutState._dropDownBounds = new Rectangle(offset + itemWidth, position, arrowWidth, itemHeight);
				itemWidth += arrowWidth;
			}
			if (!uiCommand.GetEffectiveSize().IsEmpty)
			{
				itemWidth = uiCommand.Size.Width;
				itemHeight = uiCommand.Size.Height;
			}
			uiCommandLayoutState._bounds = new Rectangle(uiCommandLayoutState._bounds.X, uiCommandLayoutState._bounds.Y, itemWidth, itemHeight);
			if (isNear)
			{
				nearPosition += itemHeight;
			}
			else
			{
				farPosition -= itemHeight;
				uiCommandLayoutState._bounds.Y -= itemHeight;
				if (!uiCommandLayoutState._textBounds.IsEmpty)
				{
					uiCommandLayoutState._textBounds.Y -= itemHeight;
				}
				if (!uiCommandLayoutState._imageBounds.IsEmpty)
				{
					uiCommandLayoutState._imageBounds.Y -= itemHeight;
				}
				if (!uiCommandLayoutState._dropDownBounds.IsEmpty)
				{
					uiCommandLayoutState._dropDownBounds.Y -= itemHeight;
				}
			}
			return new Size(itemWidth, itemHeight);
		}

		internal Size MeasureHorizontalItem(Graphics graphics, CommandLayoutState uiCommandLayoutState, int startPosition, int offset, TextLayoutRenderer textLayoutRenderer, FormatStyle uiFormatStyle)
		{
			int position = startPosition;
			return LayoutHorizontalItem(graphics, uiCommandLayoutState, ref position, ref position, offset, CommandAlignment.Near, textLayoutRenderer, uiFormatStyle);
		}

		internal Size LayoutHorizontalItem(Graphics graphics, CommandLayoutState uiCommandLayoutState, ref int nearPosition, ref int farPosition, int offset, CommandAlignment commandAlignment, TextLayoutRenderer textLayoutRenderer, FormatStyle uiFormatStyle)
		{
			MxCommand uiCommand = uiCommandLayoutState.GetCommand();
			CommandStyle commandStyle = GetEffectiveCommandStyle(uiCommand);
			bool isNear = commandAlignment == CommandAlignment.Near;
			int position = nearPosition;
			if (!isNear)
			{
				position = farPosition;
			}
			if (uiCommand.IsSeparator())
			{
				((FormatStyleLayoutState)uiCommandLayoutState).SetEnabled(false);
				uiCommandLayoutState._bounds = new Rectangle(position, offset, SeparatorThickness, 0);
			}
			else
			{
				Size size = Size.Empty;
				Size textSize = Size.Empty;
				if (!uiCommand.HasEmbeddedControl() || uiCommand.ShouldShowTextInContainers())
				{
					if (commandStyle != CommandStyle.Text)
					{
						if (uiCommand.GetEffectiveIcon() != null)
						{
							size = GetImageSize(GetEffectiveView(), uiCommand);
						}
						else
						{
							Image image = GetCommandImage(uiCommand);
							if (image != null)
							{
								uiCommandLayoutState._itemImage = image;
								size = GetImageSize(GetEffectiveView(), uiCommand);
							}
						}
					}
					if (commandStyle != CommandStyle.Image && uiCommand.GetEffectiveText().Length > 0)
					{
						Font font = uiFormatStyle.ResolveFont(Font);
						textSize = textLayoutRenderer.MeasureText(graphics, uiCommand.GetEffectiveText(), font, 0).ToSize();
						if (_commandBarType == CommandBarType.Menu && (commandStyle == CommandStyle.Text || (size.IsEmpty && commandStyle == CommandStyle.TextImage)))
						{
							textSize.Width += MenuTextExtraWidth;
						}
						textSize.Width += TextPadding;
						textSize.Height += TextPadding;
					}
					ArrangeHorizontalItem(uiCommandLayoutState, position, offset, size, textSize);
				}
			}
			int itemWidth = uiCommandLayoutState._bounds.Width;
			int itemHeight = uiCommandLayoutState._bounds.Height;
			if (uiCommand.HasEmbeddedControl())
			{
				uiCommandLayoutState._hostsControl = true;
				ApplyHostedControlStyle(graphics, uiCommand.Control);
				itemWidth = ((itemWidth != 0) ? (itemWidth + uiCommand.Control.Width) : (itemWidth + (uiCommand.Control.Width + 2)));
				int controlHeight = uiCommand.Control.Height + 2;
				if (controlHeight > itemHeight)
				{
					itemHeight = controlHeight;
				}
				uiCommandLayoutState._bounds = new Rectangle(position, offset, itemWidth, itemHeight);
			}
			if (!uiCommand.IsSeparator())
			{
				if (itemWidth < MinItemThickness)
				{
					itemWidth = MinItemThickness;
				}
				if (itemHeight < MinItemThickness)
				{
					itemHeight = MinItemThickness;
				}
			}
			if (uiCommand.IsParent && _commandBarType == CommandBarType.ToolBar && uiCommand.GetEffectiveDropDownButtonStyle() == DropDownButtonStyle.SplitButton)
			{
				int arrowWidth = (_isThemed) ? ThemedSplitArrowWidth : SplitArrowWidth;
				uiCommandLayoutState._dropDownBounds = new Rectangle(position + itemWidth, offset, arrowWidth, itemHeight);
				itemWidth += arrowWidth;
			}
			if (!uiCommand.GetEffectiveSize().IsEmpty)
			{
				itemWidth = uiCommand.Size.Width;
				itemHeight = uiCommand.Size.Height;
			}
			uiCommandLayoutState._bounds = new Rectangle(uiCommandLayoutState._bounds.X, uiCommandLayoutState._bounds.Y, itemWidth, itemHeight);
			if (isNear)
			{
				nearPosition += itemWidth;
			}
			else
			{
				farPosition -= itemWidth;
				uiCommandLayoutState._bounds.X -= itemWidth;
				if (!uiCommandLayoutState._textBounds.IsEmpty)
				{
					uiCommandLayoutState._textBounds.X -= itemWidth;
				}
				if (!uiCommandLayoutState._imageBounds.IsEmpty)
				{
					uiCommandLayoutState._imageBounds.X -= itemWidth;
				}
				if (!uiCommandLayoutState._dropDownBounds.IsEmpty)
				{
					uiCommandLayoutState._dropDownBounds.X -= itemWidth;
				}
			}
			return new Size(itemWidth, itemHeight);
		}

		internal static void CenterItemsInRow(int rowHeight, ICollection iCollection, TextAlign textAlign)
		{
			List<CommandLayoutState> list = new List<CommandLayoutState>();
			foreach (CommandLayoutState item in iCollection)
			{
				list.Add(item);
			}
			foreach (CommandLayoutState layoutItem in list)
			{
				if (layoutItem.GetCommand() != null && layoutItem.GetCommand().IsSeparator())
				{
					layoutItem._bounds.Height = rowHeight - 2;
				}
				int offset = (rowHeight - layoutItem._bounds.Height) / 2;
				if ((rowHeight - layoutItem._bounds.Height) % 2 != 0)
				{
					offset++;
				}
				layoutItem._bounds.Y += offset;
				if (layoutItem._hostsControl)
				{
					Control control = layoutItem.GetCommand().Control;
					int x = layoutItem._bounds.X + (layoutItem._bounds.Width - control.Width - 2);
					int controlTop = layoutItem._bounds.Y + (layoutItem._bounds.Height - control.Height) / 2;
					Rectangle bounds = new Rectangle(x, controlTop, control.Width, control.Height);
					control.Bounds = bounds;
					offset = controlTop;
				}
				if (!layoutItem._dropDownBounds.Size.IsEmpty)
				{
					layoutItem._dropDownBounds.Y += offset;
				}
				if (!layoutItem._imageBounds.Size.IsEmpty)
				{
					layoutItem._imageBounds.Y += offset;
				}
				if (!layoutItem._textBounds.Size.IsEmpty)
				{
					layoutItem._textBounds.Y += offset;
				}
			}
		}

		private void CenterItemsInColumn(int columnWidth, ICollection iCollection, TextAlign textAlign)
		{
			foreach (CommandLayoutState item in iCollection)
			{
				if (item.GetCommand() != null && item.GetCommand().IsSeparator())
				{
					item._bounds.Width = columnWidth - 2;
				}
				int offset = (columnWidth - item._bounds.Width) / 2;
				if ((columnWidth - item._bounds.Width) % 2 != 0)
				{
					offset++;
				}
				item._bounds.X += offset;
				if (!item._dropDownBounds.Size.IsEmpty)
				{
					item._dropDownBounds.X += offset;
				}
				if (!item._imageBounds.Size.IsEmpty)
				{
					item._imageBounds.X += offset;
				}
				if (!item._textBounds.Size.IsEmpty)
				{
					item._textBounds.X += offset;
				}
			}
		}

		internal PopupSettings BuildPopupSettings(MxCommand uiCommand)
		{
			PopupSettings uiPopupSettings = (CommandManager == null) ? new PopupSettings() : CommandManager.CreatePopupSettings();
			uiPopupSettings = uiPopupSettings.MergeWith(PopupSettings, CommandsStateStyles);
			uiPopupSettings = uiPopupSettings.MergeWith(uiCommand.GetEffectivePopupSettings(), uiCommand.StateStyles);
			if (uiPopupSettings.StateStyles.FormatStyle.ForeColor.IsEmpty)
			{
				uiPopupSettings.StateStyles.FormatStyle.ForeColor = GetTextColor();
			}
			return uiPopupSettings;
		}

		internal FormatStyle BuildItemFormatStyle(StateStyles uiStateStyles, bool isHot, bool isSelected, bool isEnabled, bool isPressed)
		{
			if (!base.Enabled)
			{
				isEnabled = false;
			}
			FormatStyle uiFormatStyle = new FormatStyle(uiStateStyles.FormatStyle);
			if (!isEnabled)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.DisabledFormatStyle);
			}
			else if (isSelected && isHot && isPressed)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.PressedFormatStyle);
			}
			else if (isSelected)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.SelectedFormatStyle);
			}
			else if (isHot)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.HotFormatStyle);
			}
			if (uiFormatStyle.ForeColor.IsEmpty)
			{
				uiFormatStyle.ForeColor = GetTextColor();
			}
			return uiFormatStyle;
		}

		private CommandStyle GetEffectiveCommandStyle(MxCommand uiCommand)
		{
			if (uiCommand.GetEffectiveCommandStyle() != CommandStyle.Default)
			{
				return uiCommand.GetEffectiveCommandStyle();
			}
			if (_commandsStyle != CommandStyle.Default)
			{
				return _commandsStyle;
			}
			if (_commandManager.CommandsStyle != CommandStyle.Default)
			{
				return _commandManager.CommandsStyle;
			}
			if (_commandBarType == CommandBarType.Menu)
			{
				return CommandStyle.Text;
			}
			return CommandStyle.TextImage;
		}

		internal StateStyles BuildCommandStateStyles()
		{
			if (CommandManager == null)
			{
				return new StateStyles(CommandsStateStyles);
			}
			StateStyles uiStateStyles = CommandManager.CreateStateStyles();
			uiStateStyles = uiStateStyles.CreateMergedStateStyles(CommandsStateStyles);
			if (_commandBarType == CommandBarType.Menu)
			{
			}
			return uiStateStyles;
		}

		internal void RecalculateLayout()
		{
			RecalculateLayout(false);
		}

		internal void RecalculateLayout(bool useMaxExtent)
		{
			if (base.IsHandleCreated && _commandManager != null)
			{
				using (Graphics graphics = CreateGraphics())
				{
					TextLayoutRenderer textLayoutRenderer = CreateTextRenderer();
					StateStyles uiStateStyles = BuildCommandStateStyles();
					LayoutBar(graphics, textLayoutRenderer, uiStateStyles, useMaxExtent);
					textLayoutRenderer.ResetCache();
				}
			}
		}
	}
}
