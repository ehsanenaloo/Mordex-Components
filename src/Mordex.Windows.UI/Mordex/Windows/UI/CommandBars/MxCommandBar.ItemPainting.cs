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
		// Draws the drop marker with reversible lines on the screen, so calling it twice
		// at the same index erases it. It does not need a Graphics.
		private void DrawDropMarker()
		{
			int position = 0;
			CommandLayoutState uiCommandLayoutState = null;
			int markerLength = 0;
			if (_layoutItems.Count != 0)
			{
				uiCommandLayoutState = ((_dropIndex < _layoutItems.Count) ? ((CommandLayoutState)_layoutItems.FindItemByIndex(_dropIndex)) : ((CommandLayoutState)_layoutItems.FindItemByIndex(_layoutItems.Count - 1)));
			}
			else if (_gripItem != null)
			{
				if (IsHorizontalLayout())
				{
					position += _gripItem._bounds.Width;
					markerLength = _gripItem._bounds.Height;
				}
				else
				{
					position += _gripItem._bounds.Height;
					markerLength = _gripItem._bounds.Width;
				}
			}
			if (_isDropAfterItem && uiCommandLayoutState != null)
			{
				if (_dropIndex < _layoutItems.Count - 1)
				{
					CommandLayoutState nextItem = (CommandLayoutState)_layoutItems.FindItemByIndex(_dropIndex + 1);
					if (!nextItem.GetCommand().IsSeparator())
					{
						if (IsHorizontalLayout())
						{
							position = nextItem._bounds.Left;
							markerLength = nextItem._bounds.Height;
						}
						else
						{
							position = nextItem._bounds.Top;
							markerLength = nextItem._bounds.Width;
						}
					}
					else if (IsHorizontalLayout())
					{
						position = uiCommandLayoutState._bounds.Right;
						markerLength = uiCommandLayoutState._bounds.Height;
					}
					else
					{
						position = uiCommandLayoutState._bounds.Bottom;
						markerLength = uiCommandLayoutState._bounds.Width;
					}
				}
				else if (IsHorizontalLayout())
				{
					position = uiCommandLayoutState._bounds.Right - DropMarkerWidth;
					markerLength = uiCommandLayoutState._bounds.Height;
				}
				else
				{
					position = uiCommandLayoutState._bounds.Bottom - DropMarkerWidth;
					markerLength = uiCommandLayoutState._bounds.Width;
				}
			}
			else if (uiCommandLayoutState != null)
			{
				if (IsHorizontalLayout())
				{
					position = uiCommandLayoutState._bounds.Left;
					markerLength = uiCommandLayoutState._bounds.Height;
				}
				else
				{
					position = uiCommandLayoutState._bounds.Top;
					markerLength = uiCommandLayoutState._bounds.Width;
				}
			}
			Color backColor = RenderingHelper.GetSchemeColor(ColorName.Control);
			if (IsHorizontalLayout())
			{
				int y = base.ClientRectangle.Top + 1;
				int lineLength = base.ClientRectangle.Height - 3;
				if (uiCommandLayoutState != null)
				{
					y = uiCommandLayoutState._bounds.Y;
					lineLength = uiCommandLayoutState._bounds.Height;
				}
				lineLength = markerLength;
				Point point = PointToScreen(new Point(position, y));
				position = point.X;
				y = point.Y;
				ControlPaint.DrawReversibleLine(new Point(position, y), new Point(position + DropMarkerWidth, y), backColor);
				ControlPaint.DrawReversibleLine(new Point(position + 1, y + 1), new Point(position + DropMarkerWidth - 1, y + 1), backColor);
				ControlPaint.DrawReversibleLine(new Point(position + 2, y + 2), new Point(position + 2, y + lineLength - 1), backColor);
				ControlPaint.DrawReversibleLine(new Point(position + 3, y + 2), new Point(position + 3, y + lineLength - 1), backColor);
				ControlPaint.DrawReversibleLine(new Point(position, y + lineLength), new Point(position + DropMarkerWidth, y + lineLength), backColor);
				ControlPaint.DrawReversibleLine(new Point(position + 1, y + lineLength - 1), new Point(position + DropMarkerWidth - 1, y + lineLength - 1), backColor);
			}
			else
			{
				int x = base.ClientRectangle.Left + 1;
				int lineLength = base.ClientRectangle.Width - 3;
				if (uiCommandLayoutState != null)
				{
					x = uiCommandLayoutState._bounds.X;
					lineLength = uiCommandLayoutState._bounds.Width;
				}
				lineLength = markerLength;
				Point screenPoint = PointToScreen(new Point(x, position));
				position = screenPoint.Y;
				x = screenPoint.X;
				ControlPaint.DrawReversibleLine(new Point(x, position), new Point(x, position + DropMarkerWidth), backColor);
				ControlPaint.DrawReversibleLine(new Point(x + 1, position + 1), new Point(x + 1, position + DropMarkerWidth - 1), backColor);
				ControlPaint.DrawReversibleLine(new Point(x + 2, position + 2), new Point(x + lineLength - 1, position + 2), backColor);
				ControlPaint.DrawReversibleLine(new Point(x + 2, position + 3), new Point(x + lineLength - 1, position + 3), backColor);
				ControlPaint.DrawReversibleLine(new Point(x + lineLength, position), new Point(x + lineLength, position + DropMarkerWidth), backColor);
				ControlPaint.DrawReversibleLine(new Point(x + lineLength - 1, position + 1), new Point(x + lineLength - 1, position + DropMarkerWidth - 1), backColor);
			}
		}

		private void ApplyTextAlignment(ref TextLayoutRenderer textLayoutRenderer, MxCommand uiCommand)
		{
			switch (uiCommand.GetEffectiveTextAlignment())
			{
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomRight:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Far);
				break;
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.MiddleRight:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				break;
			case ContentAlignment.TopCenter:
			case ContentAlignment.TopLeft:
			case ContentAlignment.TopRight:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Near);
				break;
			}
			switch (uiCommand.GetEffectiveTextAlignment())
			{
			case ContentAlignment.BottomCenter:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.TopCenter:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				break;
			case ContentAlignment.BottomLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.TopLeft:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				break;
			case ContentAlignment.BottomRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.TopRight:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
				break;
			}
		}

		internal void DrawItem(Graphics graphics, VisualStylePainter renderer, CommandLayoutState uiCommandLayoutState, StateStyles baseStateStyles, TextLayoutRenderer textLayoutRenderer, Rectangle rectangle)
		{
			DrawThemedItem(graphics, MxThemeManager.ResolveDefault(DeviceDpi), uiCommandLayoutState, baseStateStyles, textLayoutRenderer);
		}

		private void DrawText(Graphics dc, CommandLayoutState uiCommandLayoutState, FormatStyle uiFormatStyle, TextLayoutRenderer textLayoutRenderer)
		{
			CommandStyle commandStyle = GetEffectiveCommandStyle(uiCommandLayoutState.GetCommand());
			if (uiCommandLayoutState.GetCommand().HasEmbeddedControl() && _isHorizontal && !uiCommandLayoutState.GetCommand().ShouldShowTextInContainers())
			{
				return;
			}
			Color backColor = uiFormatStyle.BackColor;
			if (backColor.IsEmpty)
			{
				if (_barBackColor.IsEmpty)
				{
					_barBackColor = RenderingHelper.GetSchemeColor(ColorName.Control);
				}
				backColor = _barBackColor;
			}
			if (commandStyle != CommandStyle.Text)
			{
				DrawItemImage(dc, uiCommandLayoutState, backColor);
			}
			if (commandStyle == CommandStyle.Image || uiCommandLayoutState._caption.Length <= 0)
			{
				return;
			}
			textLayoutRenderer.DrawText(dc, uiCommandLayoutState._caption, uiCommandLayoutState._textBounds, uiFormatStyle.ResolveFont(Font), uiFormatStyle.GetEffectiveForeColor());
		}

		private void DrawItemImage(Graphics graphics, CommandLayoutState uiCommandLayoutState, Color color)
		{
			Rectangle rectangle = uiCommandLayoutState._imageBounds;
			if (uiCommandLayoutState.GetCommand().GetEffectiveIcon() != null)
			{
				if (!uiCommandLayoutState.GetCommand().GetEffectiveImageReplaceableColor().IsEmpty && uiCommandLayoutState.GetCommand().CommandType == CommandType.ColorPickerCommand)
				{
					ImageAttributes imageAttributes = new ImageAttributes();
					ColorMap colorMap = new ColorMap();
					colorMap.NewColor = (Color)uiCommandLayoutState.GetCommand().ControlValue;
					colorMap.OldColor = uiCommandLayoutState.GetCommand().GetEffectiveImageReplaceableColor();
					imageAttributes.SetRemapTable(new ColorMap[1] { colorMap }, ColorAdjustType.Bitmap);
					RenderingHelper.DrawStateImage(graphics, uiCommandLayoutState.GetCommand().GetEffectiveIcon(), null, -1, null, rectangle, ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot(), ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), color, IsThemed(), imageAttributes);
					imageAttributes.Dispose();
				}
				else
				{
					RenderingHelper.DrawStateImage(graphics, uiCommandLayoutState.GetCommand().GetEffectiveIcon(), null, -1, null, rectangle, ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot(), ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), color, IsThemed(), null);
				}
				return;
			}
			View view = GetEffectiveView();
			ImageList imageList = ResolveImageList(view, uiCommandLayoutState.GetCommand());
			if (view == View.LargeIcons && imageList == null)
			{
				view = View.SmallIcons;
				imageList = ResolveImageList(view, uiCommandLayoutState.GetCommand());
			}
			Image image = RenderingHelper.GetCommandImage(imageList, uiCommandLayoutState, view);
			int imageIndex = RenderingHelper.FindCommandImageIndex(imageList, uiCommandLayoutState, view);
			if (image != null)
			{
				if (!uiCommandLayoutState.GetCommand().GetEffectiveImageReplaceableColor().IsEmpty && uiCommandLayoutState.GetCommand().CommandType == CommandType.ColorPickerCommand)
				{
					ImageAttributes imageAttributes = new ImageAttributes();
					ColorMap colorMap = new ColorMap();
					colorMap.NewColor = (Color)uiCommandLayoutState.GetCommand().ControlValue;
					colorMap.OldColor = uiCommandLayoutState.GetCommand().GetEffectiveImageReplaceableColor();
					imageAttributes.SetRemapTable(new ColorMap[1] { colorMap }, ColorAdjustType.Bitmap);
					RenderingHelper.DrawStateImage(graphics, null, image, imageIndex, imageList, rectangle, ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot(), ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), color, IsThemed(), imageAttributes);
					imageAttributes.Dispose();
				}
				else
				{
					RenderingHelper.DrawStateImage(graphics, null, image, imageIndex, imageList, rectangle, ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled(), ((FormatStyleLayoutState)uiCommandLayoutState).IsHot(), ((FormatStyleLayoutState)uiCommandLayoutState).IsPressed(), color, IsThemed(), null);
				}
			}
		}
	}
}
