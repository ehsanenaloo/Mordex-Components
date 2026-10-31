using System;
using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class CellRendererBase
	{
		protected MxColorSchemeAppearanceState _drawingStyle;

		protected IGridEnvironment _owner;

		internal GridlinePenRenderer _gridlineRenderer;

		internal bool _alignsText;

		internal bool _gridRenderer5Boolean;

		internal bool _drawsText;

		internal CellRendererBase(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
		{
			if (drawingStyle == null)
			{
				_drawingStyle = new MxColorSchemeAppearanceState(useTextRenderer: false);
			}
			else
			{
				_drawingStyle = drawingStyle;
			}
			_owner = owner;
			_gridRenderer5Boolean = true;
			_drawsText = true;
		}

		internal void ReleaseResources()
		{
			OnDispose();
		}

		internal virtual void OnDispose()
		{
		}

		internal Image GetImageByKey(string key)
		{
			if (_owner != null)
			{
				ImageList imageList = _owner.GetImageList();
				if (imageList != null && imageList.Images.ContainsKey(key))
				{
					return imageList.Images[key];
				}
			}
			return null;
		}

		internal Image GetImageByIndex(int value)
		{
			if (_owner != null)
			{
				ImageList imageList = _owner.GetImageList();
				if (imageList != null && value >= 0 && value < imageList.Images.Count)
				{
					return imageList.Images[value];
				}
			}
			return null;
		}

		internal bool IsEmptyBounds(Rectangle rectangle)
		{
			if (rectangle.Width <= 0 || rectangle.Height <= 0)
			{
				return true;
			}
			return false;
		}

		internal virtual void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rectangle, bool selected, bool focused)
		{
			if (!IsEmptyBounds(rectangle))
			{
				Brush backBrush = GetBackBrush(rectangle, item, style, selected, focused);
				if (backBrush != null)
				{
					graphics.FillRectangle(backBrush, rectangle);
					backBrush.Dispose();
					backBrush = null;
				}
				PaintBackgroundImage(graphics, rectangle, style);
			}
		}

		internal virtual Brush GetBackBrush(Rectangle rectangle, GridGroupLayoutState item, GridFormatStyle style, bool selected, bool focused)
		{
			return style.CreateBackBrush(rectangle);
		}

		internal virtual void PaintBackgroundImage(Graphics graphics, Rectangle destRect, GridFormatStyle style)
		{
			if (style != null && style.BackgroundImage != null)
			{
				GridHelper.DrawBackgroundImage(graphics, style.BackgroundImage, style.BackgroundImageDrawMode, destRect, destRect, style.Alpha, style.BackColorAlphaMode);
			}
		}

		internal virtual void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rectangle, bool selected, bool focused)
		{
			if (IsEmptyBounds(rectangle))
			{
				return;
			}
			Image image = null;
			int indexOfKey = -1;
			string text = "";
			if (ShouldDrawImage())
			{
				image = item.GetDisplayImage(_owner.GetImageList());
				indexOfKey = item.GetImageIndex();
				text = item.GetImageKey();
				if (text.Length > 0 && _owner.GetImageList() != null)
				{
					indexOfKey = _owner.GetImageList().Images.IndexOfKey(text);
				}
			}
			string displayText = "";
			if (ShouldDrawText())
			{
				displayText = item.GetDisplayText();
			}
			TextLayoutRenderer textRenderer = GetTextRenderer(item, style);
			Brush foreBrush = GetForeBrush(style, item, selected, focused);
			Font font = GetStyleFont(style);
			textRenderer.Batch = _owner.PaintTextBatch;
			try
			{
				GridHelper.DrawImageAndText(graphics, foreBrush, displayText, image, rectangle, style.ImageHorizontalAlignment, style.ImageVerticalAlignment, textRenderer, font, item.IsRightToLeft(_owner.RightToLeft), indexOfKey, _owner.GetImageList(), _owner.IsPrinting());
			}
			finally
			{
				textRenderer.Batch = null;
			}
			foreBrush?.Dispose();
			font = null;
			foreBrush = null;
		}

		internal virtual Font GetStyleFont(GridFormatStyle style)
		{
			return style.ResolveFont(_owner.DefaultFont);
		}

		internal virtual Brush GetForeBrush(GridFormatStyle style, GridGroupLayoutState item, bool selected, bool focused)
		{
			return GridHelper.CreateAlphaBrush(style.ForeColor, style.Alpha, style.ForeColorAlphaMode);
		}

		internal virtual TextLayoutRenderer GetTextRenderer(GridGroupLayoutState item, GridFormatStyle style)
		{
			TextLayoutRenderer textLayoutRenderer = item.ComputeTextLayoutRenderer(style, _drawingStyle.GetUseTextRenderer());
			if (item.IsRightToLeft(_owner.RightToLeft))
			{
				textLayoutRenderer.UpdateRightToLeft(RightToLeft.Yes);
			}
			return textLayoutRenderer;
		}

		internal virtual bool ShouldDrawImage()
		{
			return _gridRenderer5Boolean;
		}

		internal virtual bool ShouldDrawText()
		{
			return _drawsText;
		}

		internal virtual Rectangle GetForegroundRect(Rectangle otherRectangle, GridGroupLayoutState item, GridFormatStyle style)
		{
			Rectangle rectangle;
			switch (style.Appearance)
			{
			case Appearance.Raised:
				rectangle = new Rectangle(otherRectangle.X + 2, otherRectangle.Y + 2, otherRectangle.Width - 3, otherRectangle.Height - 3);
				break;
			case Appearance.SunkenLight:
			case Appearance.RaisedLight:
				rectangle = Rectangle.Inflate(otherRectangle, -1, -1);
				break;
			case Appearance.Sunken:
				rectangle = new Rectangle(otherRectangle.X + 1, otherRectangle.Y + 1, otherRectangle.Width - 3, otherRectangle.Height - 3);
				break;
			default:
				rectangle = otherRectangle;
				break;
			}
			return item.CalculateContentRect(rectangle, item.IsRightToLeft(_owner.RightToLeft));
		}

		internal virtual void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rectangle)
		{
			if (_owner.IsPrinting())
			{
				if (style.Appearance != Appearance.Flat && style.Appearance != Appearance.Empty)
				{
					rectangle = new Rectangle(rectangle.X - 1, rectangle.Y - 1, rectangle.Width + 1, rectangle.Height + 1);
					Pen pen = GdiCache.Pen(GridHelper.BlendColors(style.GetEffectiveBackColor(), Color.Black, 0.5f));
					GridHelper.DrawFlatBorder(graphics, pen, rectangle);
				}
			}
			else
			{
				RenderContent(graphics, rectangle, style.Appearance, style.GetEffectiveBackColor());
			}
		}

		// True when nothing this renderer paints after the text of an item, other than the gridlines, can touch the text.
		internal virtual bool TextMayBeDeferred(GridFormatStyle style)
		{
			return GetType() == typeof(CellRendererBase) && (style.Appearance == Appearance.Flat || style.Appearance == Appearance.Empty);
		}

		// The part of the item that nothing painted after the text of the item touches: the area the gridlines leave free, unless the
		// renderer paints more after its text.
		internal virtual Rectangle GetTextFreeArea(GridGroupLayoutState item, Rectangle bounds, Rectangle gridlineFreeArea)
		{
			return gridlineFreeArea;
		}

		internal virtual void PaintGridlines(Graphics graphics, GridGroupLayoutState item, Rectangle rectangle)
		{
			if (_gridlineRenderer != null)
			{
				_gridlineRenderer.PaintGridlines(graphics, item, rectangle, _owner.RightToLeft);
				_gridlineRenderer.RenderContent(graphics, item, rectangle, _owner.RightToLeft);
			}
		}

		internal static void DrawRecoloredImage(Graphics graphics, Image image, Rectangle rectangle, Color color, bool condition, ImageHorizontalAlignment imageHorizontalAlignment, bool otherCondition)
		{
			if (image == null)
			{
				return;
			}
			ImageAttributes imageAttributes = new ImageAttributes();
			ColorMap colorMap = new ColorMap();
			colorMap.NewColor = color;
			colorMap.OldColor = Color.Black;
			ColorMap[] remapTable = new ColorMap[1] { colorMap };
			imageAttributes.SetRemapTable(remapTable);
			Rectangle destRect;
			switch (imageHorizontalAlignment)
			{
			case ImageHorizontalAlignment.Near:
			case ImageHorizontalAlignment.BeforeText:
				destRect = ((!otherCondition) ? new Rectangle(rectangle.Left + 4, rectangle.Top + (rectangle.Height - image.Height) / 2, image.Width, image.Height) : new Rectangle(rectangle.Right - 4 - image.Width, rectangle.Top + (rectangle.Height - image.Height) / 2, image.Width, image.Height));
				break;
			case ImageHorizontalAlignment.Far:
			case ImageHorizontalAlignment.AfterText:
				if (otherCondition)
				{
					int value = rectangle.Width - image.Width / 2;
					if (value > 2)
					{
						value = (value = 2);
					}
					destRect = new Rectangle(rectangle.Left + value, rectangle.Top + (rectangle.Height - image.Height) / 2, image.Width, image.Height);
				}
				else
				{
					destRect = new Rectangle(rectangle.Right - 4 - image.Width, rectangle.Top + (rectangle.Height - image.Height) / 2, image.Width, image.Height);
				}
				break;
			default:
				destRect = new Rectangle(rectangle.Left + (rectangle.Width - image.Width) / 2, rectangle.Top + (rectangle.Height - image.Height) / 2, image.Width, image.Height);
				break;
			}
			if ((rectangle.Width - image.Width) % 2 != 0)
			{
				destRect.X++;
			}
			if ((rectangle.Height - image.Height) % 2 != 0)
			{
				destRect.Y++;
			}
			if (condition)
			{
				destRect.X = destRect.Right;
				destRect.Width = -destRect.Width;
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
				return;
			}
			try
			{
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		internal static void RenderContent(Graphics graphics, Rectangle rectangle, Appearance appearance, Color color)
		{
			if (appearance == Appearance.Flat || appearance == Appearance.Empty)
			{
				return;
			}
			Pen pen = null;
			bool condition = false;
			if (color == SystemColors.Control || color == SystemColors.Window)
			{
				condition = true;
			}
			switch (appearance)
			{
			case Appearance.Raised:
			case Appearance.RaisedLight:
				pen = ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(Color.White, color, 0.75f)) : SystemPens.ControlLightLight);
				break;
			case Appearance.Sunken:
			case Appearance.SunkenLight:
				pen = ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(color, Color.Black, 0.5f)) : SystemPens.ControlDark);
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			switch (appearance)
			{
			case Appearance.Raised:
				pen = ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(Color.Black, color, 0.75f)) : SystemPens.ControlDarkDark);
				break;
			case Appearance.RaisedLight:
				pen = ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(color, Color.Black, 0.5f)) : SystemPens.ControlDark);
				break;
			case Appearance.Sunken:
			case Appearance.SunkenLight:
				pen = ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(Color.White, color, 0.75f)) : SystemPens.ControlLightLight);
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			if (appearance == Appearance.Raised || appearance == Appearance.Sunken)
			{
				rectangle.Inflate(-1, -1);
				pen = ((appearance == Appearance.Raised) ? ((!condition) ? GdiCache.Pen(color) : SystemPens.ControlLight) : ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(Color.Black, color, 0.75f)) : SystemPens.ControlDarkDark));
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
				pen = ((appearance == Appearance.Raised) ? ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(Color.Black, color, 0.5f)) : SystemPens.ControlDark) : ((!condition) ? GdiCache.Pen(color) : SystemPens.ControlLight));
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
				graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			}
		}
	}
}
