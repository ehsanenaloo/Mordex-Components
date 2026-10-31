using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class ImageCellRenderer : CellRendererBase
	{
		internal ImageCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (IsEmptyBounds(rect))
			{
				return;
			}
			object cellValue = item.GetCellValue();
			Bitmap bitmap = null;
			if (GridHelper.IsNullOrDBNull(cellValue))
			{
				return;
			}
			Image image = cellValue as Image;
			if (image != null)
			{
				bitmap = ((!item._column.BoundImageSize.IsEmpty) ? new Bitmap(image, item._column.BoundImageSize) : new Bitmap(image));
				if (!item._column.BoundImageMaskColor.IsEmpty)
				{
					bitmap.MakeTransparent(item._column.BoundImageMaskColor);
				}
				image = bitmap;
			}
			GridHelper.DrawImageAndText(graphics, null, "", image, rect, style.ImageHorizontalAlignment, style.ImageVerticalAlignment, null, null, item.IsRightToLeft(_owner.RightToLeft), -1, null, _owner.IsPrinting());
			bitmap?.Dispose();
		}
	}
}
