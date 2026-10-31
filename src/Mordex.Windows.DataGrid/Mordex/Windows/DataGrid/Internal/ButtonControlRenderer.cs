using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class ButtonControlRenderer : CellRendererBase
	{
		internal bool _controlStyleRenderer2Boolean;

		internal ControlStyle _buttonControlStyle;

		internal bool _isRightToLeft;

		internal bool _isLeftToRight;

		internal ButtonControlRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
			_controlStyleRenderer2Boolean = false;
		}

		internal override void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect)
		{
			if (_drawingStyle.IsThemedLook)
			{
				if (item.IsDisabled())
				{
					Pen pen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.DisabledBorder));
					GridHelper.DrawFlatBorder(graphics, pen, rect);
				}
				else if (item._isHot || item._isPressed)
				{
					Pen otherPen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementBorder));
					GridHelper.DrawFlatBorder(graphics, otherPen, rect);
				}
				else
				{
					Pen nextPen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.ControlBorder));
					GridHelper.DrawFlatBorder(graphics, nextPen, rect);
				}
			}
			else if (!_drawingStyle._isThemed && _buttonControlStyle != null)
			{
				if (_controlStyleRenderer2Boolean && _buttonControlStyle.ButtonAppearance == ButtonAppearance.FlatBorderless && !item._isHot && !item._isPressed)
				{
					GridHelper.DrawBorderForStyle(graphics, _buttonControlStyle, rect, BorderStyle.Flat);
				}
				else
				{
					_buttonControlStyle.DrawButtonBorder(graphics, rect, item._isHot, item._isPressed);
				}
			}
		}

		internal override void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			if (_drawingStyle.IsThemedLook)
			{
				Color color;
				Color otherColor;
				if (item.IsDisabled())
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.DisabledColor);
					otherColor = color;
				}
				else if (item._isPressed)
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.PressedElement);
					otherColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.PressedElementGradient);
				}
				else if (item._isHot)
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElement);
					otherColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementGradient);
				}
				else
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.Button);
					otherColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.ButtonGradient);
				}
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(rect, 1, 1), color, otherColor, 90f);
				Blend blend = null;
				if (_drawingStyle._isThemed)
				{
					blend = new Blend(4);
					blend.Positions = new float[4] { 0f, 0.53f, 0.53f, 1f };
					blend.Factors = new float[4] { 0f, 0.17f, 0.19f, 1f };
					linearGradientBrush.Blend = blend;
				}
				else
				{
					blend = new Blend(4);
					blend.Positions = new float[4] { 0f, 0.53f, 0.53f, 1f };
					blend.Factors = new float[4] { 0f, 0.3f, 0.35f, 1f };
					linearGradientBrush.Blend = blend;
				}
				graphics.FillRectangle(linearGradientBrush, rect);
				linearGradientBrush.Dispose();
			}
			else if (_drawingStyle._isThemed)
			{
				switch (item._type)
				{
				case LayoutItemType.Cell:
				case LayoutItemType.CardCell:
				case LayoutItemType.CellColumnButton:
					if (item.GetButtonStyle() == ButtonStyle.DownArrow)
					{
						VisualStyleDrawer.DrawComboDropDownButton(graphics, rect, item._isHot, item._isPressed, !item.IsDisabled());
					}
					else
					{
						VisualStyleDrawer.DrawPushButton(graphics, rect, item._isHot, item._isPressed, item.IsDisabled());
					}
					break;
				case LayoutItemType.CellDropDownButton:
					if (GridHelper.IsWindows7RendererEnabled())
					{
						ButtonThemeRenderer.DrawThemedBackground(graphics, rect, _owner.RightToLeft, !item.IsDisabled(), item._isPressed, item._isHot, false);
						ButtonThemeRenderer.DrawDropArrow(graphics, rect, !item.IsDisabled());
					}
					else
					{
						VisualStyleDrawer.DrawComboDropDownButton(graphics, rect, item._isHot, item._isPressed, !item.IsDisabled());
					}
					break;
				case LayoutItemType.FilterClearButton:
				case LayoutItemType.FilterRowDropDown:
					VisualStyleDrawer.DrawPushButton(graphics, rect, item._isHot, item._isPressed, item.IsDisabled());
					break;
				case LayoutItemType.CellDownButton:
					VisualStyleDrawer.DrawSpinDownButton(graphics, rect, item._isHot, item._isPressed, true);
					break;
				case LayoutItemType.CellUpButton:
					VisualStyleDrawer.DrawSpinDownButton(graphics, rect, item._isHot, item._isPressed, true);
					break;
				}
			}
			else if (_buttonControlStyle != null)
			{
				_buttonControlStyle.FillButtonBackground(graphics, rect, item._isHot, item._isPressed);
			}
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (_buttonControlStyle == null)
			{
				return;
			}
			ImageList imageList = _owner.GetImageList();
			int direction = 1;
			Image image = null;
			int indexOfKey = -1;
			if (item._column != null)
			{
				if (item.GetButtonStyle() == ButtonStyle.ButtonCell)
				{
					image = item.GetDisplayImage(_owner.GetImageList());
					indexOfKey = item.GetImageIndex();
					string text = item.GetImageKey();
					if (text.Length > 0 && _owner.GetImageList() != null)
					{
						indexOfKey = _owner.GetImageList().Images.IndexOfKey(text);
					}
				}
				if (image == null)
				{
					image = item._column.ButtonImage;
				}
			}
			else if (item._cellValue is ImageListLookup)
			{
				ImageListLookup state = (ImageListLookup)item._cellValue;
				image = state._currentImage;
				indexOfKey = state.GetImageIndex();
				imageList = state._imageList;
			}
			else
			{
				image = item._ownImage;
			}
			int value = 0;
			ButtonStyle buttonStyle;
			switch (item._type)
			{
			case LayoutItemType.CellUpDownBackground:
				return;
			case LayoutItemType.CellUpButton:
				buttonStyle = ButtonStyle.DownArrow;
				direction = -1;
				value = rect.Width - 2;
				break;
			case LayoutItemType.CellDropDownButton:
				value = rect.Width - 2;
				buttonStyle = ButtonStyle.DownArrow;
				break;
			case LayoutItemType.CellDownButton:
				value = rect.Width - 2;
				buttonStyle = ButtonStyle.DownArrow;
				break;
			case LayoutItemType.FilterClearButton:
				buttonStyle = ButtonStyle.Image;
				image = GridHelper.MakeBitmapTransparent(ResourceImages.GetRemoveFilterImage(), Color.Magenta);
				break;
			case LayoutItemType.FilterRowDropDown:
				if (item._column.FilterRowComparison == ConditionOperator.IsEmpty || item._column.FilterRowComparison == ConditionOperator.NotIsEmpty)
				{
					buttonStyle = ButtonStyle.TextButton;
					break;
				}
				buttonStyle = ButtonStyle.Image;
				image = GridGroupLayoutState.GetFilterOperatorImage(item._column.FilterRowComparison);
				break;
			default:
				value = rect.Width - 2;
				buttonStyle = item.GetButtonStyle();
				break;
			}
			switch (buttonStyle)
			{
			case ButtonStyle.ButtonCell:
			{
				Font font = GetStyleFont(style);
				TextLayoutRenderer textRenderer = GetTextRenderer(item._owner, style);
				GridHelper.DrawImageAndText(graphics, _buttonControlStyle.GetControlTextBrush(), item.GetDisplayText(), image, rect, style.ImageHorizontalAlignment, style.ImageVerticalAlignment, textRenderer, font, item.IsRightToLeft(_owner.RightToLeft), indexOfKey, imageList, _owner.IsPrinting());
				break;
			}
			case ButtonStyle.Ellipsis:
				if (_drawingStyle.IsThemedLook)
				{
					Brush brush = (!item.IsDisabled()) ? GdiCache.Brush(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.ButtonText)) : GdiCache.Brush(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.DisabledBorder));
					rect.Offset(1, 0);
					ControlStyle.FillGripDots(graphics, rect, brush);
				}
				else
				{
					if (_drawingStyle._isThemed)
					{
						rect.Inflate(-1, -1);
					}
					_buttonControlStyle.DrawGrip(graphics, rect, item._isPressed, !item.IsDisabled());
				}
				break;
			case ButtonStyle.Image:
				if (image != null)
				{
					Rectangle.Inflate(rect, -1, -1);
					graphics.DrawImageUnscaled(image, rect.X + (rect.Width - image.Width) / 2, rect.Y + (rect.Height - image.Height) / 2);
				}
				else if (indexOfKey >= 0 && imageList != null)
				{
					Rectangle rectangle = Rectangle.Inflate(rect, -1, -1);
					imageList.Draw(graphics, rectangle.X + (rectangle.Width - imageList.ImageSize.Width) / 2, rectangle.Y + (rectangle.Height - imageList.ImageSize.Height) / 2, indexOfKey);
				}
				break;
			case ButtonStyle.TextButton:
				if (item.GetDisplayText().Length > 0)
				{
					TextLayoutRenderer textRenderer = new TextLayoutRenderer(_drawingStyle.GetUseTextRenderer());
					textRenderer.SetStringAlignment(StringAlignment.Center);
					textRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
					textRenderer.UpdateTextLayoutRendererBoolean(true);
					if (item.IsRightToLeft(_owner.RightToLeft))
					{
						textRenderer.UpdateRightToLeft(RightToLeft.Yes);
					}
					Color color = _buttonControlStyle.ControlTextColor;
					if (item.IsDisabled())
					{
						color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.DisabledBorder);
					}
					textRenderer.DrawTextWithColor(graphics, item.GetDisplayText(), rect, _owner.DefaultFont, color);
				}
				break;
			case ButtonStyle.DownArrow:
				if (_drawingStyle.IsThemedLook)
				{
					Pen pen = (item.IsDisabled() ? GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.DisabledBorder)) : GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.ButtonText)));
					ControlStyle.DrawVerticalTriangle(graphics, value, rect, direction, pen);
				}
				else if (!_drawingStyle._isThemed)
				{
					Pen controlTextPen = ((!item.IsDisabled()) ? _buttonControlStyle.GetControlTextPen() : _buttonControlStyle.GetControlDarkPen());
					if (item._type == LayoutItemType.CellDownButton || item._type == LayoutItemType.CellUpButton)
					{
						value = rect.Height + 2;
					}
					ControlStyle.DrawVerticalTriangle(graphics, value, rect, direction, controlTextPen);
				}
				break;
			}
		}
	}
}
