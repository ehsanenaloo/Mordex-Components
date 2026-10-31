using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class PopupSettingsRenderer
	{
		private const int DefaultImageAreaWidth = 24;

		private const int TextLeftPadding = 6;

		private const int MinShortcutAreaWidth = 35;

		private const int DropButtonWidth = 11;

		private const int DropButtonRightOffset = 13;

		private const int SubmenuArrowSize = 17;

		private const int SubmenuArrowRightOffset = 19;

		private const float SeparatorBlend = 0.7f;

		private PopupSettings _popupSettings;

		private FormatStyle _baseFormatStyle;

		private TextLayoutRenderer _textLayoutRenderer;

		private MxCommandManager _commandManager;

		private Font _textFont;

		private int _imageAreaWidth = DefaultImageAreaWidth;

		private bool _isThemed;

		private readonly Color _comboColor = Color.Empty;

		private Pen _highlightPen;

		internal PopupSettingsRenderer(MxCommandManager commandManager)
		{
			SetCommandManager(commandManager);
		}

		private Image GetCommandImage(MxCommand command)
		{
			if (command.GetEffectiveImage() != null)
			{
				return command.GetEffectiveImage();
			}
			ImageList imageList = _commandManager.GetImageListForView(Mordex.Windows.UI.CommandBars.View.SmallIcons, command);
			if (imageList != null)
			{
				try
				{
					if (command.GetEffectiveLargeImageKey().Length > 0 && imageList.Images.ContainsKey(command.GetEffectiveLargeImageKey()))
					{
						return imageList.Images[command.GetEffectiveLargeImageKey()];
					}
					if (command.GetEffectiveImageIndex() >= 0 && command.GetEffectiveImageIndex() < imageList.Images.Count)
					{
						return imageList.Images[command.GetEffectiveImageIndex()];
					}
				}
				catch (System.Exception ex) when (ex is System.ArgumentException || ex is System.InvalidOperationException)
				{
					// The image list can change while the popup paints; draw the item without an image.
					return null;
				}
			}
			return null;
		}

		public int MeasureItemHeight(MxCommand command, Font font)
		{
			if (_textLayoutRenderer == null)
			{
				_textLayoutRenderer = new TextLayoutRenderer(!_commandManager.UseCompatibleTextRendering);
				_textLayoutRenderer.SetWordWrap(false);
				_textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				_textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				if (_commandManager.UseMnemonic)
				{
					_textLayoutRenderer.SetShowHotkeyPrefix(true);
				}
			}
			_textFont = font;
			int itemHeight = (int)font.GetHeight() + 6;
			int imagePadding = 8;
			int imageWidth = 0;
			if (_commandManager.SmallImageSize.IsEmpty)
			{
				if (command.GetEffectiveIcon() != null)
				{
					imageWidth = command.GetEffectiveIcon().Width + imagePadding;
					if (command.GetEffectiveIcon().Height + 2 > itemHeight)
					{
						itemHeight = command.GetEffectiveIcon().Height + 2;
					}
				}
				else
				{
					Image image = GetCommandImage(command);
					if (image != null)
					{
						imageWidth = image.Width + imagePadding;
						if (image.Height + 2 > itemHeight)
						{
							itemHeight = image.Height + 2;
						}
					}
				}
			}
			else
			{
				itemHeight = _commandManager.SmallImageSize.Height + 2;
			}
			if (_imageAreaWidth < imageWidth)
			{
				_imageAreaWidth = imageWidth;
			}
			return itemHeight + 4;
		}

		public MxCommandManager GetCommandManager()
		{
			return _commandManager;
		}

		public void SetCommandManager(MxCommandManager commandManager)
		{
			if (_commandManager == commandManager)
			{
				return;
			}
			_commandManager = commandManager;
			if (_commandManager == null)
			{
				return;
			}
			_isThemed = (VisualStyleSupport.IsThemingUsable(false));
			_popupSettings = _commandManager.CreatePopupSettings();
			_popupSettings.StateStyles.FormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.ControlText);
			if (_isThemed)
			{
				_baseFormatStyle = new FormatStyle();
				_baseFormatStyle.BackColor = SystemColors.Menu;
				_baseFormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.ControlText);
			}
			else
			{
				_baseFormatStyle = new FormatStyle(_popupSettings.StateStyles.FormatStyle);
				_highlightPen = new Pen(RenderingHelper.GetSchemeColor(ColorName.Highlight));
			}
		}

		private FormatStyle GetItemFormatStyle(CommandLayoutState itemState)
		{
			FormatStyle itemStyle = new FormatStyle(_baseFormatStyle);
			if (!itemState.GetCommand().GetEffectiveStateStyles().FormatStyle.IsDefaultStyle())
			{
				itemStyle = itemStyle.MergeStyle(itemState.GetCommand().GetEffectiveStateStyles().FormatStyle);
			}
			if (((FormatStyleLayoutState)itemState).IsPressed())
			{
				if (_isThemed)
				{
					itemStyle.BackColor = RenderingHelper.GetSchemeColor(ColorName.Highlight);
					itemStyle.Blend = 0f;
					itemStyle.BackgroundGradientMode = BackgroundGradientMode.Solid;
					itemStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.HighlightText);
				}
				else
				{
					itemStyle = itemStyle.MergeStyle(_popupSettings.StateStyles.HotFormatStyle);
				}
			}
			return itemStyle;
		}

		public void DrawItem(Graphics graphics, CommandLayoutState itemState, Rectangle bounds)
		{
			Brush brush = _baseFormatStyle.GetBackgroundBrush(bounds);
			if (brush != null)
			{
				graphics.FillRectangle(brush, bounds);
			}
			FormatStyle itemStyle = GetItemFormatStyle(itemState);
			bool hasImageArea = false;
			if ((!_isThemed))
			{
				Rectangle imageAreaBounds = new Rectangle(bounds.X, bounds.Y, _imageAreaWidth, bounds.Height);
				Brush imageAreaBrush = _popupSettings.ImageAreaFormatStyle.GetBackgroundBrush(imageAreaBounds);
				if (imageAreaBrush != null)
				{
					graphics.FillRectangle(imageAreaBrush, imageAreaBounds);
				}
				_popupSettings.ImageAreaFormatStyle.DisposeResources();
				hasImageArea = true;
			}
			LayoutItem(graphics, itemState, bounds);
			DrawItemBackground(graphics, itemState, hasImageArea, itemStyle);
			if (itemState.GetCommand() != null && itemState.GetCommand().IsSeparator())
			{
				DrawSeparator(graphics, itemState);
			}
			else
			{
				DrawItemContent(graphics, itemState, itemStyle.GetForeBrush(), itemStyle.BackColor, itemStyle.ForeColor);
			}
			itemStyle.DisposeResources();
		}

		private void DrawSeparator(Graphics graphics, CommandLayoutState itemState)
		{
			if (!_isThemed)
			{
				{
					Pen indentedPen = null;
					indentedPen = ((!_isThemed) ? GdiCache.Pen(RenderingHelper.BlendColors(RenderingHelper.GetSchemeColor(ColorName.ControlDark), RenderingHelper.GetSchemeColor(ColorName.Window), SeparatorBlend)) : GdiCache.Pen(VisualStylePainter.GetThemeBorderColor()));
					graphics.DrawLine(indentedPen, itemState._bounds.X + itemState._imageBounds.Width + 8, itemState._bounds.Y + 1, itemState._bounds.Right, itemState._bounds.Y + 1);
					indentedPen = null;
				}
			}
			else
			{
				Pen compactPen = GdiCache.Pen(RenderingHelper.BlendColors(RenderingHelper.GetSchemeColor(ColorName.ControlDark), RenderingHelper.GetSchemeColor(ColorName.Window), SeparatorBlend));
				graphics.DrawLine(compactPen, itemState._bounds.X + 1, itemState._bounds.Y + 3, itemState._bounds.Right - 2, itemState._bounds.Y + 3);
				compactPen = null;
			}
		}

		private void LayoutItem(Graphics graphics, CommandLayoutState itemState, Rectangle bounds)
		{
			int imageAreaOffset = 0;
			int textOffset = _imageAreaWidth + imageAreaOffset;
			itemState._bounds = bounds;
			itemState._imageBounds = new Rectangle(bounds.X + imageAreaOffset, bounds.Y, _imageAreaWidth, bounds.Height);
			if (itemState.GetCommand().HasEmbeddedControl())
			{
				int width = (int)_textLayoutRenderer.MeasureText(graphics, itemState._caption, _textFont, 0).Width + 4;
				itemState._textBounds = new Rectangle(bounds.X + textOffset + TextLeftPadding, bounds.Y, width, bounds.Height);
				int shortcutWidth = itemState._bounds.Width - itemState._textBounds.Right - 1;
				if (shortcutWidth < MinShortcutAreaWidth)
				{
					itemState._textBounds.Width -= MinShortcutAreaWidth - shortcutWidth;
					shortcutWidth = MinShortcutAreaWidth;
				}
				itemState._dropDownBounds = new Rectangle(itemState._textBounds.Right, bounds.Y + 1, shortcutWidth, bounds.Height - 2);
			}
			else
			{
				itemState._textBounds = new Rectangle(bounds.X + textOffset + TextLeftPadding, bounds.Y, bounds.Width - textOffset - 2, bounds.Height);
			}
		}

		private void DrawItemBackground(Graphics graphics, CommandLayoutState itemState, bool hasImageArea, FormatStyle itemStyle)
		{
			Rectangle bounds = itemState._bounds;
			if (!hasImageArea || ((FormatStyleLayoutState)itemState).IsPressed())
			{
				bounds.Inflate(-1, -1);
				Brush brush = itemStyle.GetBackgroundBrush(bounds);
				if (brush != null)
				{
					graphics.FillRectangle(brush, bounds);
				}
				if (((FormatStyleLayoutState)itemState).IsPressed() && _highlightPen != null)
				{
					RenderingHelper.DrawRectangleOutline(graphics, _highlightPen, bounds);
				}
			}
			else
			{
				bounds = itemState._textBounds;
				bounds.Width += itemState._bounds.Right - itemState._textBounds.Right;
				bounds.Inflate(-1, -1);
				Brush fullRowBrush = itemStyle.GetBackgroundBrush(bounds);
				if (fullRowBrush != null)
				{
					graphics.FillRectangle(fullRowBrush, bounds);
				}
			}
			if (!itemState._dropDownBounds.IsEmpty)
			{
				DrawComboArea(graphics, itemState, itemStyle);
			}
		}

		private void DrawComboArea(Graphics graphics, CommandLayoutState itemState, FormatStyle itemStyle)
		{
			graphics.FillRectangle(SystemBrushes.Window, itemState._dropDownBounds);
			if (_isThemed && VisualStyleSupport.AreVisualStylesEnabled())
			{
				VisualStyleSupport.DrawComboBoxBorder(graphics, itemState._dropDownBounds, true, true);
				if (itemState.GetCommand().CommandType == CommandType.ComboBoxCommand || itemState.GetCommand().CommandType == CommandType.FontPickerCommand)
				{
					int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
					Rectangle scrollBarBounds = new Rectangle(itemState._dropDownBounds.Right - verticalScrollBarWidth - 1, itemState._dropDownBounds.Y + 1, verticalScrollBarWidth, itemState._dropDownBounds.Height - 2);
					VisualStylePainter.DrawComboDropDownTheme(graphics, scrollBarBounds, ((FormatStyleLayoutState)itemState).IsPressed(), false, true);
				}
				return;
			}
			if (!((FormatStyleLayoutState)itemState).IsPressed())
			{
				Pen borderPen = GdiCache.Pen(_comboColor);
				RenderingHelper.DrawRectangleOutline(graphics, borderPen, itemState._dropDownBounds);
				borderPen = null;
			}
			else {
				Rectangle comboBounds = itemState._dropDownBounds;
				comboBounds.Inflate(0, 1);
				RenderingHelper.DrawColoredBorder(graphics, _comboColor, comboBounds, BorderStyle.SunkenLight3D);
			}
			if (itemState.GetCommand().CommandType != CommandType.ComboBoxCommand && itemState.GetCommand().CommandType != CommandType.FontPickerCommand)
			{
				return;
			}
			Rectangle dropButtonBounds = new Rectangle(itemState._dropDownBounds.Right - DropButtonRightOffset, itemState._dropDownBounds.Y + 2, DropButtonWidth, itemState._dropDownBounds.Height - 4);
			Brush dropButtonBrush = null;
			if (((FormatStyleLayoutState)itemState).IsPressed())
			{
				dropButtonBrush = GdiCache.Brush(itemStyle.GetBlendedBackColor());
				dropButtonBounds.Inflate(2, 2);
			}
			else
			{
				dropButtonBrush = GdiCache.Brush(_comboColor);
			}
			graphics.FillRectangle(dropButtonBrush, dropButtonBounds);
			if (((FormatStyleLayoutState)itemState).IsPressed() && _highlightPen != null)
			{
				RenderingHelper.DrawRectangleOutline(graphics, _highlightPen, dropButtonBounds);
			}
			dropButtonBrush = null;
			Pen arrowPen = GdiCache.Pen(RenderingHelper.GetSchemeColor(ColorName.ControlText));
			RenderingHelper.DrawUpDownTriangle(graphics, DropButtonWidth, dropButtonBounds, 1, arrowPen);
		}

		private void DrawItemContent(Graphics graphics, CommandLayoutState itemState, Brush textBrush, Color backColor, Color foreColor)
		{
			Rectangle imageBounds = itemState._imageBounds;
			Size imageSize = Size.Empty;
			if (backColor.IsEmpty)
			{
				backColor = RenderingHelper.GetSchemeColor(ColorName.Control);
			}
			if (foreColor.IsEmpty)
			{
				foreColor = RenderingHelper.GetSchemeColor(ColorName.ControlText);
			}
			if (itemState.GetCommand().GetEffectiveIcon() != null)
			{
				imageSize = ((!_commandManager.SmallImageSize.IsEmpty) ? _commandManager.SmallImageSize : itemState.GetCommand().GetEffectiveIcon().Size);
				imageBounds.X += (imageBounds.Width - imageSize.Width) / 2;
				imageBounds.Y += (imageBounds.Height - imageSize.Height) / 2;
				imageBounds.Size = imageSize;
				RenderingHelper.DrawStateImage(graphics, itemState.GetCommand().GetEffectiveIcon(), null, -1, null, imageBounds, ((FormatStyleLayoutState)itemState).IsEnabled(), ((FormatStyleLayoutState)itemState).IsPressed(), false, backColor, _isThemed, null);
			}
			else
			{
				ImageList imageList = _commandManager.GetImageListForView(Mordex.Windows.UI.CommandBars.View.SmallIcons, itemState.GetCommand());
				Image image = RenderingHelper.GetCommandImage(imageList, itemState, Mordex.Windows.UI.CommandBars.View.SmallIcons);
				int imageIndex = RenderingHelper.FindCommandImageIndex(imageList, itemState, Mordex.Windows.UI.CommandBars.View.SmallIcons);
				if (image != null)
				{
					imageSize = ((!_commandManager.SmallImageSize.IsEmpty) ? _commandManager.SmallImageSize : image.Size);
					imageBounds.X += (imageBounds.Width - imageSize.Width) / 2;
					imageBounds.Y += (imageBounds.Height - imageSize.Height) / 2;
					imageBounds.Size = imageSize;
					RenderingHelper.DrawStateImage(graphics, null, image, imageIndex, imageList, imageBounds, ((FormatStyleLayoutState)itemState).IsEnabled(), ((FormatStyleLayoutState)itemState).IsPressed(), false, backColor, _isThemed, null);
				}
			}
			if (textBrush != null)
			{
				_textLayoutRenderer.DrawTextWithBrush(graphics, itemState._caption, itemState._textBounds, _textFont, textBrush);
				if (itemState.GetShortcutText() != "")
				{
					_textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
					_textLayoutRenderer.DrawTextWithBrush(graphics, itemState.GetShortcutText(), itemState._textBounds, _textFont, textBrush);
					_textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				}
			}
			if (itemState.GetCommand().IsParent)
			{
				Rectangle arrowBounds = new Rectangle(itemState._bounds.Right - SubmenuArrowRightOffset, itemState._bounds.Y, SubmenuArrowSize, itemState._bounds.Height);
				Pen arrowPen = GdiCache.Pen(foreColor);
				RenderingHelper.DrawSideTriangle(graphics, SubmenuArrowSize, arrowBounds, 1, arrowPen);
				arrowPen = null;
			}
		}
	}
}
