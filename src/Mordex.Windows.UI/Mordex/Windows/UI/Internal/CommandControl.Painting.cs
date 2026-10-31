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

		private void DrawOverflowItem(Graphics graphics, CommandLayoutState uiCommandLayoutState, TextLayoutRenderer textLayoutRenderer, FormatStyle itemStyle, FormatStyle classicStyle)
		{
			FormatStyle uiFormatStyle = null;
			Rectangle bounds = uiCommandLayoutState._bounds;
			bounds.Inflate(-2, -2);
			Brush brush = null;
			Color color = Color.Empty;
			uiFormatStyle = classicStyle;
			bounds.Inflate(2, 2);
			if (((FormatStyleLayoutState)uiCommandLayoutState).IsHot() || _childMenu != null)
			{
				Color backColor = classicStyle.BackColor;
				RenderingHelper.DrawColoredBorder(graphics, backColor, bounds, BorderStyle.SunkenLight3D);
			}
			if (brush != null)
			{
				graphics.FillRectangle(brush, bounds);
			}
			if ((uiCommandLayoutState == _openItem || ((FormatStyleLayoutState)uiCommandLayoutState).IsHot()))
			{
				Pen pen = GdiCache.Pen(color);
				RenderingHelper.DrawRectangleOutline(graphics, pen, bounds);
				pen = null;
			}
			DrawItemContent(graphics, uiCommandLayoutState, textLayoutRenderer, itemStyle, itemStyle);
			uiFormatStyle?.DisposeResources();
		}

		private void ApplyTextAlignment(ref TextLayoutRenderer textLayoutRenderer, MxCommand uiCommand)
		{
			Mordex.Windows.UI.CommandBars.ContentAlignment textAlignment = uiCommand._textAlignment;
			if (textAlignment == Mordex.Windows.UI.CommandBars.ContentAlignment.Default && uiCommand.BaseCommand != null)
			{
				textAlignment = uiCommand.BaseCommand._textAlignment;
			}
			switch (textAlignment)
			{
			case Mordex.Windows.UI.CommandBars.ContentAlignment.Default:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				return;
			case Mordex.Windows.UI.CommandBars.ContentAlignment.BottomCenter:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.BottomLeft:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.BottomRight:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Far);
				break;
			case Mordex.Windows.UI.CommandBars.ContentAlignment.MiddleCenter:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.MiddleLeft:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.MiddleRight:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				break;
			case Mordex.Windows.UI.CommandBars.ContentAlignment.TopCenter:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.TopLeft:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.TopRight:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Near);
				break;
			}
			switch (textAlignment)
			{
			case Mordex.Windows.UI.CommandBars.ContentAlignment.BottomCenter:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.MiddleCenter:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.TopCenter:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				break;
			case Mordex.Windows.UI.CommandBars.ContentAlignment.BottomLeft:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.MiddleLeft:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.TopLeft:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				break;
			case Mordex.Windows.UI.CommandBars.ContentAlignment.BottomRight:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.MiddleRight:
			case Mordex.Windows.UI.CommandBars.ContentAlignment.TopRight:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
				break;
			}
		}

		private void DrawCustomizeFrame(Graphics graphics, Rectangle rectangle)
		{
			rectangle.X = _menuClientBounds.Left + _borderSize.Width;
			rectangle.Width = _menuClientBounds.Width - _borderSize.Width * 2;
			Pen pen = GdiCache.Pen(Color.Black, 2f);
			RenderingHelper.DrawRectangleOutline(graphics, pen, rectangle);
			pen = null;
		}

		private void DrawInsertMark(Graphics graphics)
		{
			int markY = 0;
			CommandLayoutState uiCommandLayoutState = null;
			if (_layoutItems.Count != 0)
			{
				uiCommandLayoutState = ((_insertMarkIndex < _layoutItems.Count) ? ((CommandLayoutState)_layoutItems.FindItemByIndex(_insertMarkIndex)) : ((CommandLayoutState)_layoutItems.FindItemByIndex(_layoutItems.Count - 1)));
			}
			else if (_tearOffBar != null)
			{
				markY += _tearOffBar._bounds.Height;
			}
			if (_insertMarkAfter)
			{
				if (_insertMarkIndex < _layoutItems.Count - 1)
				{
					CommandLayoutState nextItem = (CommandLayoutState)_layoutItems.FindItemByIndex(_insertMarkIndex + 1);
					markY = (nextItem.GetCommand().IsSeparator() ? (uiCommandLayoutState._bounds.Bottom - InsertMarkOffset) : nextItem._bounds.Top);
				}
				else
				{
					markY = uiCommandLayoutState._bounds.Bottom - InsertMarkOffset;
				}
			}
			else
			{
				markY = uiCommandLayoutState._bounds.Top;
			}
			int x = base.ClientRectangle.Left + 1;
			int markWidth = base.ClientRectangle.Width - 3;
			Point point = PointToScreen(new Point(x, markY));
			x = point.X;
			markY = point.Y;
			Color backColor = RenderingHelper.GetSchemeColor(ColorName.Window);
			ControlPaint.DrawReversibleLine(new Point(x, markY), new Point(x, markY + InsertMarkHeight), backColor);
			ControlPaint.DrawReversibleLine(new Point(x + 1, markY + 1), new Point(x + 1, markY + InsertMarkHeight - 1), backColor);
			ControlPaint.DrawReversibleLine(new Point(x + 2, markY + 2), new Point(x + markWidth, markY + 2), backColor);
			ControlPaint.DrawReversibleLine(new Point(x + 2, markY + 3), new Point(x + markWidth, markY + 3), backColor);
			ControlPaint.DrawReversibleLine(new Point(x + markWidth, markY), new Point(x + markWidth, markY + InsertMarkHeight), backColor);
			ControlPaint.DrawReversibleLine(new Point(x + markWidth - 1, markY + 1), new Point(x + markWidth - 1, markY + InsertMarkHeight - 1), backColor);
		}

		private TextLayoutRenderer CreateTextRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!ShouldUseCompatibleTextRendering());
			textLayoutRenderer.SetWordWrap(false);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetShowHotkeyPrefix(_useMnemonic);
			return textLayoutRenderer;
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			_insertMarkIndex = -1;
			TextLayoutRenderer textLayoutRenderer = CreateTextRenderer();
			if (_isLayoutDirty)
			{
				MxCommand uiCommand = null;
				if (GetSelectedItem() != null && GetSelectedItem().GetCommand() != null)
				{
					uiCommand = _selectedItem.GetCommand();
				}
				LayoutItems(eventArgs.Graphics, textLayoutRenderer);
				if (uiCommand != null)
				{
					SelectItem(_layoutItems.FindCommandItem(uiCommand));
					StopChildCloseTimer();
				}
			}
			PaintThemed(eventArgs, textLayoutRenderer);
		}

		private void DrawItemContent(Graphics graphics, CommandLayoutState uiCommandLayoutState, TextLayoutRenderer textLayoutRenderer, FormatStyle uiFormatStyle, FormatStyle baseStyle)
		{
			if (uiCommandLayoutState.GetCommand().IsDefaultItem() && uiCommandLayoutState.GetCommand().IsEnabled)
			{
				uiFormatStyle.FontBold = TriState.True;
			}
			if (RightToLeft == RightToLeft.Yes)
			{
				textLayoutRenderer.SetRightToLeft(RightToLeft.Yes);
			}
			DrawText(graphics, uiCommandLayoutState, textLayoutRenderer, uiFormatStyle.ResolveFont(Font), uiFormatStyle.GetForeBrush(), baseStyle, uiFormatStyle.BackColor, uiFormatStyle.ForeColor);
		}

		private void DrawCheckFrame(Graphics graphics, Rectangle rect, FormatStyle uiFormatStyle, bool useThemes, bool isHot, bool isEnabled)
		{
			rect.Inflate(2, 2);
			if (useThemes)
			{
				VisualStylePainter.DrawCheckedToolBarButtonTheme(graphics, rect, isHot);
			}
			else
			{
				if (uiFormatStyle == null)
				{
					return;
				}
				Pen pen = null;
				if (!isEnabled)
				{
					pen = ((!uiFormatStyle.ForeColor.IsEmpty) ? GdiCache.Pen(uiFormatStyle.ForeColor) : GdiCache.Pen(SystemColors.GrayText));
				}
				else
				{
					Brush brush = uiFormatStyle.GetBackgroundBrush(rect);
					if (brush != null)
					{
						graphics.FillRectangle(brush, rect);
					}
					pen = GdiCache.Pen(uiFormatStyle.GetStateBorderColor(isHot, true));
				}
				RenderingHelper.DrawRectangleOutline(graphics, pen, rect);
				pen = null;
			}
		}

		private void DrawCheckBackground(Graphics graphics, CommandLayoutState uiCommandLayoutState, Rectangle rect, FormatStyle baseFormatStyle, Color backColor)
		{
			FormatStyle uiFormatStyle = null;
			if (_renderer != null)
			{
				_renderer.DrawMenuItemFrame(graphics, rect, uiCommandLayoutState);
			}
			else
			{
				if (!_canUseThemes)
				{
					uiFormatStyle = GetItemFormatStyle(uiCommandLayoutState, baseFormatStyle, true, ((FormatStyleLayoutState)uiCommandLayoutState).IsHot(), uiCommandLayoutState.GetCommand().IsInfrequent(), true, false);
				}
				DrawCheckFrame(graphics, rect, uiFormatStyle, _canUseThemes, ((FormatStyleLayoutState)uiCommandLayoutState).IsHot(), ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled());
			}
			uiFormatStyle?.DisposeResources();
		}

		private void DrawText(Graphics dc, CommandLayoutState uiCommandLayoutState, TextLayoutRenderer textLayoutRenderer, Font font, Brush brush, FormatStyle uiFormatStyle, Color backColor, Color foreColor)
		{
			try
			{
				Rectangle imageBounds = uiCommandLayoutState._imageBounds;
				if (uiCommandLayoutState._hasGroupLine)
				{
					imageBounds.Y++;
					imageBounds.Height--;
				}
				Size imageSize = Size.Empty;
				bool drawCheck = uiCommandLayoutState.GetCommand().IsChecked;
				Mordex.Windows.UI.CommandBars.View view = ResolvePopupView();
				if (view == Mordex.Windows.UI.CommandBars.View.LargeIcons && _useSmallImages)
				{
					view = Mordex.Windows.UI.CommandBars.View.SmallIcons;
				}
				if (backColor.IsEmpty)
				{
					backColor = RenderingHelper.GetSchemeColor(ColorName.Control);
				}
				if (uiCommandLayoutState.GetCommand().GetEffectiveIcon() != null)
				{
					imageSize = GetImageSize(ResolvePopupView(), uiCommandLayoutState.GetCommand());
					imageBounds.X += (imageBounds.Width - imageSize.Width) / 2;
					imageBounds.Y += (imageBounds.Height - imageSize.Height) / 2;
					imageBounds.Size = imageSize;
					if (drawCheck && !_hasCheckMargin)
					{
						DrawCheckBackground(dc, uiCommandLayoutState, imageBounds, uiFormatStyle, backColor);
						drawCheck = false;
					}
					if (!uiCommandLayoutState.GetCommand().GetEffectiveImageReplaceableColor().IsEmpty && uiCommandLayoutState.GetCommand().CommandType == CommandType.ColorPickerCommand)
					{
						ImageAttributes iconRemapAttributes = new ImageAttributes();
						ColorMap iconColorMap = new ColorMap();
						iconColorMap.NewColor = (Color)uiCommandLayoutState.GetCommand().ControlValue;
						iconColorMap.OldColor = uiCommandLayoutState.GetCommand().GetEffectiveImageReplaceableColor();
						iconRemapAttributes.SetRemapTable(new ColorMap[1] { iconColorMap }, ColorAdjustType.Bitmap);
						RenderingHelper.DrawStateImage(dc, uiCommandLayoutState.GetCommand().GetEffectiveIcon(), null, -1, null, imageBounds, ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot() || ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), false, backColor, _canUseThemes, iconRemapAttributes);
						iconRemapAttributes.Dispose();
					}
					else
					{
						RenderingHelper.DrawStateImage(dc, uiCommandLayoutState.GetCommand().GetEffectiveIcon(), null, -1, null, imageBounds, ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot() || ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), false, backColor, _canUseThemes, null);
					}
				}
				else
				{
					ImageList imageList = GetImageList(uiCommandLayoutState.GetCommand());
					Image image = RenderingHelper.GetCommandImage(imageList, uiCommandLayoutState, view);
					int imageIndex = RenderingHelper.FindCommandImageIndex(imageList, uiCommandLayoutState, view);
					if (image != null)
					{
						imageSize = GetImageSize(ResolvePopupView(), uiCommandLayoutState.GetCommand());
						imageBounds.X += (imageBounds.Width - imageSize.Width) / 2;
						imageBounds.Y += (imageBounds.Height - imageSize.Height) / 2;
						imageBounds.Size = imageSize;
						if (drawCheck && !_hasCheckMargin)
						{
							Rectangle checkBounds = imageBounds;
							DrawCheckBackground(dc, uiCommandLayoutState, checkBounds, uiFormatStyle, backColor);
							drawCheck = false;
						}
						if (!uiCommandLayoutState.GetCommand().GetEffectiveImageReplaceableColor().IsEmpty && uiCommandLayoutState.GetCommand().CommandType == CommandType.ColorPickerCommand)
						{
							ImageAttributes imageRemapAttributes = new ImageAttributes();
							ColorMap imageColorMap = new ColorMap();
							imageColorMap.NewColor = (Color)uiCommandLayoutState.GetCommand().ControlValue;
							imageColorMap.OldColor = uiCommandLayoutState.GetCommand().GetEffectiveImageReplaceableColor();
							imageRemapAttributes.SetRemapTable(new ColorMap[1] { imageColorMap }, ColorAdjustType.Bitmap);
							RenderingHelper.DrawStateImage(dc, null, image, imageIndex, imageList, imageBounds, ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot() || ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), false, backColor, _canUseThemes, imageRemapAttributes);
							imageRemapAttributes.Dispose();
						}
						else
						{
							RenderingHelper.DrawStateImage(dc, null, image, imageIndex, imageList, imageBounds, ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot() || ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), false, backColor, _canUseThemes, null);
						}
					}
				}
				bool hasDescription = false;
				Rectangle textBounds = uiCommandLayoutState._textBounds;
				Rectangle descriptionBounds = Rectangle.Empty;
				if (view == Mordex.Windows.UI.CommandBars.View.LargeIcons && uiCommandLayoutState.GetCommand() != null && uiCommandLayoutState.GetCommand().Description.Length > 0)
				{
					hasDescription = true;
					textBounds = new Rectangle(textBounds.X, textBounds.Y + 3, textBounds.Width, textBounds.Height / 3);
					descriptionBounds = new Rectangle(textBounds.X, textBounds.Bottom, textBounds.Width, uiCommandLayoutState._bounds.Height - textBounds.Height - 3);
				}
				if (brush != null)
				{
					if (uiCommandLayoutState._hasGroupLine)
					{
						textBounds.Y++;
						textBounds.Height--;
					}
					textLayoutRenderer.DrawTextWithBrush(dc, uiCommandLayoutState._caption, textBounds, font, brush);
					if (uiCommandLayoutState.GetShortcutText() != "")
					{
						textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
						textLayoutRenderer.DrawTextWithBrush(dc, uiCommandLayoutState.GetShortcutText(), textBounds, font, brush);
						textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					}
					if (hasDescription)
					{
						textLayoutRenderer.SetWordWrap(true);
						textLayoutRenderer.SetLineAlignment(StringAlignment.Near);
						textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
						textLayoutRenderer.DrawTextWithBrush(dc, uiCommandLayoutState.GetCommand().Description, descriptionBounds, font, brush);
					}
				}
				if (uiCommandLayoutState.GetCommand().IsParent)
				{
					if (_isOverflowMenu)
					{
						if (foreColor == Color.Empty)
						{
							foreColor = (((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled() ? RenderingHelper.GetSchemeColor(ColorName.ControlText) : RenderingHelper.GetSchemeColor(ColorName.GrayText));
						}
						Rectangle overflowArrowBounds = ((_canUseThemes) ? new Rectangle(uiCommandLayoutState._bounds.Right - SubmenuArrowWidth, uiCommandLayoutState._bounds.Y, SubmenuArrowWidth, uiCommandLayoutState._bounds.Height) : new Rectangle(uiCommandLayoutState._bounds.Right - SubmenuArrowRightMargin, uiCommandLayoutState._bounds.Y, SubmenuArrowWidth, uiCommandLayoutState._bounds.Height));
						{
							Pen overflowArrowPen = GdiCache.Pen(foreColor);
							RenderingHelper.DrawUpDownTriangle(dc, OverflowArrowGlyphSize, overflowArrowBounds, 1, overflowArrowPen);
							overflowArrowPen = null;
						}
					}
					else
					{
						Rectangle arrowBounds = ((_canUseThemes) ? new Rectangle(uiCommandLayoutState._bounds.Right - SubmenuArrowWidth, uiCommandLayoutState._bounds.Y, SubmenuArrowWidth, uiCommandLayoutState._bounds.Height) : new Rectangle(uiCommandLayoutState._bounds.Right - SubmenuArrowRightMargin, uiCommandLayoutState._bounds.Y, SubmenuArrowWidth, uiCommandLayoutState._bounds.Height));
						{
							Pen arrowPen = GdiCache.Pen(foreColor);
							RenderingHelper.DrawSideTriangle(dc, SubmenuArrowGlyphSize, arrowBounds, 1, arrowPen);
							arrowPen = null;
						}
					}
				}
				if (!drawCheck)
				{
					return;
				}
				imageSize = new Size(CheckGlyphSize, CheckGlyphSize);
				if (ResolvePopupView() == Mordex.Windows.UI.CommandBars.View.LargeIcons)
				{
					imageSize = new Size(imageBounds.Size.Height, imageBounds.Size.Height);
					imageSize.Height -= LargeCheckInset;
					imageSize.Width -= LargeCheckInset;
				}
				if (_hasCheckMargin)
				{
					imageBounds = uiCommandLayoutState._checkBounds;
				}
				imageBounds.X += (imageBounds.Width - imageSize.Width) / 2;
				imageBounds.Y += (imageBounds.Height - imageSize.Height) / 2;
				imageBounds.Size = imageSize;
				if (!_canUseThemes)
				{
					DrawCheckBackground(dc, uiCommandLayoutState, imageBounds, uiFormatStyle, RenderingHelper.GetSchemeColor(ColorName.Control));
				}
				Image checkImage = RenderingHelper.MakeMagentaTransparent(WindowsResources.Check());
				if (checkImage != null)
				{
					RenderingHelper.DrawRecoloredImage(dc, checkImage, imageBounds, foreColor);
				}
			}
			catch (Exception ex) when (ex is ArgumentException || ex is ExternalException)
			{
				// GDI+ throws for empty or off-screen bounds. A skipped item is better than a failed paint.
			}
		}
	}
}
