using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog
	{
		private void RenderContent(Graphics graphics, TextLayoutRenderer textRenderer, int maxHeight, int fullHeight, ref int popupWidth)
		{
			int heightToDistribute = maxHeight - fullHeight;
			IList<GalleryMenuRenderer> list = new List<GalleryMenuRenderer>();
			foreach (CommandBaseRenderer item in _layoutItems)
			{
				if (item is GalleryMenuRenderer)
				{
					list.Add(item as GalleryMenuRenderer);
				}
			}
			int heightPerGallery = heightToDistribute / list.Count;
			int heightRemainder = heightToDistribute % list.Count;
			for (int i = 0; i < list.Count; i++)
			{
				GalleryMenuRenderer thisValue = list[i];
				thisValue.GetPreferredSize();
				thisValue._hasScrollBar = true;
				if (i == list.Count - 1)
				{
					thisValue.SetRequestedSize(new Size(thisValue.GetPreferredSize().Width + SystemInformation.VerticalScrollBarWidth, heightPerGallery + heightRemainder));
				}
				else
				{
					thisValue.SetRequestedSize(new Size(thisValue.GetPreferredSize().Width + SystemInformation.VerticalScrollBarWidth, heightPerGallery));
				}
				thisValue.CalculateFullItemWidth(graphics, textRenderer, _owner.Font);
				if (thisValue._fullItemWidth + _contentPadding.Width > popupWidth)
				{
					popupWidth = thisValue._fullItemWidth + _contentPadding.Width;
				}
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			Brush brush = GdiCache.Brush(_popupColorScheme._dialogBackground);
			eventArgs.Graphics.FillRectangle(brush, base.ClientRectangle);
			if (_layoutItems != null && _layoutItems.Count > 0)
			{
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
				textLayoutRenderer.SetShowHotkeyPrefix(true);
				if (_isMenuBoundsDirty)
				{
					RebuildMenuAndBounds(eventArgs.Graphics);
				}
				if (_isLayoutDirty)
				{
					LayoutMenuItems(eventArgs.Graphics, textLayoutRenderer);
				}
				Color foreColor = ((GetRibbon() == null) ? _owner.ColorScheme._foreColorDark : GetRibbon().ForeColorDark);
				foreach (PopupMenuSection item in _sections)
				{
					if (item.GetCommandType() == CommandType.SmallButton)
					{
						Rectangle rectangle = new Rectangle(_contentBounds.X, item._items[0]._bounds.Top, _owner.SmallImageSize.Width + 8, item.GetTotalHeight());
						DrawBackground(eventArgs.Graphics, rectangle, _owner.ColorScheme);
					}
					foreach (CommandBaseRenderer sectionItem in item._items)
					{
						Rectangle itemClip = Rectangle.Intersect(eventArgs.ClipRectangle, sectionItem._bounds);
						if (itemClip.Width > 0 && itemClip.Height > 0)
						{
							sectionItem.Paint(eventArgs.Graphics, textLayoutRenderer, _owner.ColorScheme, Font, foreColor, eventArgs.ClipRectangle);
						}
					}
				}
			}
			if (_isDesignMode)
			{
				DrawDesignSelectionBorders(eventArgs.Graphics);
			}
			if (_dropDownResizeMode != DropDownResizeMode.None)
			{
				DrawResizeBar(eventArgs.Graphics);
			}
			DrawBorder(eventArgs.Graphics);
		}

		private void DrawSelectionBorder(Graphics graphics, Rectangle rectangle, DashStyle dashStyle)
		{
			using Pen pen = new Pen(Color.Black);
			pen.DashStyle = dashStyle;
			if (rectangle.Height <= 4)
			{
				rectangle.Y -= 2;
				rectangle.Height = 4;
			}
			RibbonDrawingUtils.DrawRectangleOutline(graphics, pen, rectangle);
		}

		protected virtual void DrawBorder(Graphics graphics)
		{
			Pen pen = GdiCache.Pen(_popupColorScheme._dialogBorder);
			RibbonDrawingUtils.DrawRectangleOutline(graphics, pen, base.ClientRectangle);
		}

		private void DrawResizeBar(Graphics graphics)
		{
			ThemedRibbonPainter.ResizeBar(graphics, _popupColorScheme.Theme, _resizeBarBounds, _dropDownResizeMode == DropDownResizeMode.Height, _resizeGripBounds);
		}

		private void DrawBackground(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme)
		{
			Brush brush = GdiCache.Brush(colorScheme._menuBackground);
			graphics.FillRectangle(brush, rect);
			Pen pen = GdiCache.Pen(colorScheme._menuSeparator);
			graphics.DrawLine(pen, rect.Right, rect.Y, rect.Right, rect.Bottom - 1);
		}
	}
}
