using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class LayoutStateCellRenderer : GroupRowBackgroundRenderer
	{
		internal LayoutStateCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (IsEmptyBounds(rect))
			{
				return;
			}
			int value = -1;
			int otherValue = 0;
			foreach (GridGroupLayoutState otherItem in item._gridGroupLayoutStateOwner.Items)
			{
				if (otherItem._isVisible && otherItem._type == LayoutItemType.GroupHeaderCell && otherItem._rowHeight == item._rowHeight)
				{
					if (otherItem._isHorizontallyScrolled)
					{
						otherValue = ((otherValue != 0) ? Math.Min(otherItem._rect.X, otherValue) : otherItem._rect.X);
					}
					if (otherItem.GetDisplayText().Length > 0 && (otherItem._rect.X < value || value == -1))
					{
						value = otherItem._rect.X;
					}
				}
			}
			if (value != -1)
			{
				int width = value - item._rect.X;
				if (item._gridGroupLayoutStateOwner != null && item._gridGroupLayoutStateOwner.GridRow != null)
				{
					MxDataGrid dataGrid = item._gridGroupLayoutStateOwner.GridRow.DataGrid;
					if (dataGrid != null && dataGrid.FrozenColumns != 0)
					{
						width = ((otherValue - item._rect.X <= dataGrid.HorizontalScrollPosition) ? (otherValue - item._rect.X) : (width - dataGrid.HorizontalScrollPosition));
					}
				}
				if (_owner.RightToLeft)
				{
					rect.X = rect.Right - width;
				}
				rect.Width = width;
			}
			GridRow gridRow = item._gridGroupLayoutStateOwner.GridRow;
			if (!item.IsRightToLeft(_owner.RightToLeft))
			{
				rect.X += item._leftMargin;
			}
			rect.Width -= item._leftMargin;
			_ = gridRow.Group.Column;
			TextLayoutRenderer textLayoutRenderer = item.ComputeTextLayoutRenderer(style, _drawingStyle.GetUseTextRenderer());
			if (item.IsRightToLeft(_owner.RightToLeft))
			{
				textLayoutRenderer.UpdateRightToLeft(RightToLeft.Yes);
			}
			textLayoutRenderer.SetWordWrap(false);
			textLayoutRenderer.UpdateTrimming(StringTrimming.EllipsisCharacter);
			Font font = GetStyleFont(style);
			Brush foreBrush = GetForeBrush(style, item, selected, focused);
			if (gridRow.Group.GroupPrefix.Length > 0)
			{
				int roundCore = 0;
				roundCore = GridHelper.RoundToInt(textLayoutRenderer.MeasureText(graphics, gridRow.Group.GroupPrefix, font, 0).Width);
				if (foreBrush != null)
				{
					textLayoutRenderer.DrawTextWithBrush(graphics, gridRow.Group.GroupPrefix, rect, font, foreBrush);
				}
				if (!item.IsRightToLeft(_owner.RightToLeft))
				{
					rect.X += roundCore;
				}
				rect.Width -= roundCore;
			}
			if (rect.Width < 0)
			{
				return;
			}
			Image image = null;
			image = ((gridRow.GroupImage != null) ? gridRow.GroupImage : ((gridRow.GroupImageKey.Length <= 0) ? GetImageByIndex(gridRow.GroupImageIndex) : GetImageByKey(gridRow.GroupImageKey)));
			if (image != null)
			{
				if (!item.IsRightToLeft(_owner.RightToLeft))
				{
					graphics.DrawImage(image, rect.X, rect.Y + (rect.Height - image.Height) / 2, image.Width, image.Height);
					rect.X += image.Width + 2;
				}
				else
				{
					graphics.DrawImage(image, rect.Right - image.Width, rect.Y + (rect.Height - image.Height) / 2, image.Width, image.Height);
				}
				rect.Width -= image.Width + 2;
				if (rect.Width < 0)
				{
					return;
				}
			}
			string text = gridRow.GroupCaption;
			if (!GridHelper.IsTextLengthValid(text))
			{
				text = GridHelper.TruncateText(text);
			}
			if (gridRow.Table.UseGroupCondition && gridRow.Table.ShowGroupConditionCount && gridRow._groupConditionCount > 0)
			{
				if (gridRow.Table.GroupConditionCountTitle.Length == 0)
				{
					object nextItem = text;
					text = string.Concat(nextItem, " (", gridRow._groupConditionCount, ")");
				}
				else
				{
					object previousItem = text;
					text = string.Concat(previousItem, " (", gridRow._groupConditionCount, " ", gridRow.Table.GroupConditionCountTitle, ")");
				}
			}
			if (text.Length > 0 && foreBrush != null)
			{
				// The caption is the last thing painted in the item, so it can be held back.
				textLayoutRenderer.Batch = _owner.PaintTextBatch;
				try
				{
					textLayoutRenderer.DrawTextWithBrush(graphics, text, rect, GetStyleFont(style), foreBrush);
				}
				finally
				{
					textLayoutRenderer.Batch = null;
				}
			}
			foreBrush?.Dispose();
			textLayoutRenderer.ReleaseStringFormat();
		}
	}
}
