using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class ListCellRenderer : CellRendererBase
	{
		internal ListCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		// The images between the texts are painted in order with the texts (the batch is flushed before each image), so a list cell
		// can hold its texts like a plain cell.
		internal override bool TextMayBeDeferred(GridFormatStyle style)
		{
			return GetType() == typeof(ListCellRenderer) && (style.Appearance == Appearance.Flat || style.Appearance == Appearance.Empty);
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (item.GetCellValue() is IList && item._column != null)
			{
				TextLayoutRenderer textRenderer = GetTextRenderer(item, style);
				Brush foreBrush = GetForeBrush(style, item, selected, focused);
				Font font = GetStyleFont(style);
				ILookUpTable lookUpTable = null;
				if (item._column.ValueList != null && item._column.ValueList.ReplaceValues)
				{
					lookUpTable = item._column.ValueList;
				}
				else if (item._column.DropDown != null && item._column.DropDown.ReplaceValues)
				{
					lookUpTable = item._column.DropDown;
				}
				textRenderer.Batch = _owner.PaintTextBatch;
				try
				{
					DrawListItems(graphics, rect, item.GetCellValue(), lookUpTable, _owner.GetImageList(), font, foreBrush, textRenderer, item.IsRightToLeft(_owner.RightToLeft));
				}
				finally
				{
					textRenderer.Batch = null;
				}
				if (foreBrush != null)
				{
					foreBrush.Dispose();
					foreBrush = null;
				}
			}
			else
			{
				base.PaintForeground(graphics, item, style, rect, selected, focused);
			}
		}

		internal static void DrawListItems(Graphics graphics, Rectangle bounds, object value, ILookUpTable iLookUpTable, ImageList imageList, Font font, Brush brush, TextLayoutRenderer textLayoutRenderer, bool condition)
		{
			if (!(value is IList))
			{
				return;
			}
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetWordWrap(false);
			if (condition)
			{
				textLayoutRenderer.UpdateRightToLeft(RightToLeft.Yes);
			}
			IList list = (IList)value;
			int left;
			int right;
			if (condition)
			{
				right = bounds.Right;
				left = bounds.Left;
			}
			else
			{
				right = bounds.X;
				left = bounds.Right;
			}
			for (int i = 0; i < list.Count; i++)
			{
				object listItem = list[i];
				IMordexValueItem mordexValueItem = null;
				if (iLookUpTable != null)
				{
					mordexValueItem = iLookUpTable.GetItemFromValue(listItem);
				}
				Image image = null;
				string text = "";
				string otherText = "";
				int imageIndex = -1;
				if (mordexValueItem == null)
				{
					image = null;
					text = string.Concat(listItem);
				}
				else
				{
					image = mordexValueItem.Image;
					text = mordexValueItem.Text;
					imageIndex = mordexValueItem.ImageIndex;
				}
				if (image != null)
				{
					if (condition)
					{
						right -= 2 + image.Width;
						if (right < left)
						{
							break;
						}
					}
					else
					{
						right += 2;
						if (right > left)
						{
							break;
						}
					}
					textLayoutRenderer.Batch?.Flush();
					int y = bounds.Y + (bounds.Height - image.Height) / 2;
					graphics.DrawImage(image, right, y);
					right = (condition ? (right - 2) : (right + (image.Width + 2)));
				}
				else if (imageList != null)
				{
					if (otherText.Length > 0)
					{
						imageIndex = imageList.Images.IndexOfKey(otherText);
					}
					if (imageIndex >= 0 && imageIndex < imageList.Images.Count)
					{
						if (condition)
						{
							right -= imageList.ImageSize.Width + 2;
							if (right < left)
							{
								break;
							}
						}
						else
						{
							right += 2;
							if (right > left)
							{
								break;
							}
						}
						textLayoutRenderer.Batch?.Flush();
						int y2 = bounds.Y + (bounds.Height - imageList.ImageSize.Height) / 2;
						imageList.Draw(graphics, new Point(right, y2), imageIndex);
						right = ((!condition) ? (right + (imageList.ImageSize.Width + 2)) : (right - 2));
					}
				}
				if (i < list.Count - 1)
				{
					text += ", ";
				}
				if (text.Length <= 0)
				{
					continue;
				}
				Rectangle rectangle;
				if (condition)
				{
					if (right <= left)
					{
						break;
					}
					rectangle = Rectangle.FromLTRB(left, bounds.Y, right, bounds.Bottom);
				}
				else
				{
					if (right >= bounds.Right)
					{
						break;
					}
					rectangle = Rectangle.FromLTRB(right, bounds.Y, left, bounds.Bottom);
				}
				textLayoutRenderer.DrawTextWithBrush(graphics, text, rectangle, font, brush);
				RectangleF layoutRect = new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
				float width = 0f;
				if (textLayoutRenderer._useTextRenderer)
				{
					width = textLayoutRenderer.MeasureText(graphics, text, font, 0).Width;
					right = ((!condition) ? (right + (int)width) : (right - (int)width));
					continue;
				}
				StringFormat stringFormat = new StringFormat(textLayoutRenderer.GetStringFormat());
				stringFormat.SetMeasurableCharacterRanges(new CharacterRange[1]
				{
					new CharacterRange(0, text.Length)
				});
				Region[] array = graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat);
				if (array != null && array.Length > 0)
				{
					if (array[0].IsEmpty(graphics))
					{
						break;
					}
					right = ((!condition) ? ((int)array[0].GetBounds(graphics).Right + 4) : ((int)array[0].GetBounds(graphics).Left - 4));
				}
				else
				{
					width = textLayoutRenderer.MeasureText(graphics, text, font, 0).Width;
					right = ((!condition) ? (right + (int)width) : (right - (int)width));
				}
				stringFormat.Dispose();
			}
		}

		internal static int MeasureListItems(Graphics graphics, object value, ILookUpTable iLookUpTable, ImageList imageList, Font font, TextLayoutRenderer textLayoutRenderer)
		{
			if (value is IList)
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				textLayoutRenderer.SetWordWrap(false);
				IList list = (IList)value;
				int measureListItems = 0;
				for (int i = 0; i < list.Count; i++)
				{
					object listItem = list[i];
					IMordexValueItem mordexValueItem = null;
					if (iLookUpTable != null)
					{
						mordexValueItem = iLookUpTable.GetItemFromValue(listItem);
					}
					Image image = null;
					string text = "";
					string otherText = "";
					int imageIndex = -1;
					if (mordexValueItem == null)
					{
						image = null;
						text = string.Concat(listItem);
					}
					else
					{
						image = mordexValueItem.Image;
						text = mordexValueItem.Text;
						imageIndex = mordexValueItem.ImageIndex;
					}
					if (image != null)
					{
						measureListItems += 4 + image.Width;
					}
					else if (imageList != null)
					{
						if (otherText.Length > 0)
						{
							imageIndex = imageList.Images.IndexOfKey(otherText);
						}
						if (imageIndex >= 0 && imageIndex < imageList.Images.Count)
						{
							measureListItems += imageList.ImageSize.Width + 4;
						}
					}
					if (i < list.Count - 1)
					{
						text += ", ";
					}
					if (text.Length <= 0)
					{
						continue;
					}
					if (textLayoutRenderer._useTextRenderer)
					{
						measureListItems += (int)textLayoutRenderer.MeasureText(graphics, text, font, 0).Width;
						continue;
					}
					RectangleF layoutRect = new RectangleF(0f, 0f, 32000f, font.Height * 2);
					StringFormat stringFormat = new StringFormat(textLayoutRenderer.GetStringFormat());
					stringFormat.SetMeasurableCharacterRanges(new CharacterRange[1]
					{
						new CharacterRange(0, text.Length)
					});
					Region[] array = graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat);
					if (array != null && array.Length > 0)
					{
						if (array[0].IsEmpty(graphics))
						{
							break;
						}
						measureListItems += (int)Math.Abs(array[0].GetBounds(graphics).Width) + 4;
					}
					else
					{
						measureListItems += (int)textLayoutRenderer.MeasureText(graphics, text, font, 0).Width;
					}
					stringFormat.Dispose();
				}
				return measureListItems;
			}
			return -1;
		}
	}
}
