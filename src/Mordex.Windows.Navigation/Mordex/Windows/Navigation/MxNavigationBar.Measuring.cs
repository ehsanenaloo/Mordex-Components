using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Navigation.Internal;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public partial class MxNavigationBar
	{
		private Size GetGroupImageSize(NavigationGroup navigationBarGroup)
		{
			if (_smallImageSize != Size.Empty)
			{
				return _smallImageSize;
			}
			if (navigationBarGroup.Icon != null)
			{
				return navigationBarGroup.Icon.Size;
			}
			if (navigationBarGroup.Image != null)
			{
				return navigationBarGroup.Image.Size;
			}
			if (_smallImageList != null)
			{
				return _smallImageList.ImageSize;
			}
			return new Size(16, 16);
		}

		private Size GetItemImageSize(NavigationItem navigationBarItem)
		{
			if (navigationBarItem.Group.View == NavigationBarView.LargeIcons)
			{
				if (_largeImageSize != Size.Empty)
				{
					return _largeImageSize;
				}
				if (navigationBarItem.Icon != null)
				{
					return navigationBarItem.Icon.Size;
				}
				if (navigationBarItem.Image != null)
				{
					return navigationBarItem.Image.Size;
				}
				if (_largeImageList != null)
				{
					return _largeImageList.ImageSize;
				}
				return new Size(32, 32);
			}
			if (_smallImageSize != Size.Empty)
			{
				return _smallImageSize;
			}
			if (navigationBarItem.Icon != null)
			{
				return navigationBarItem.Icon.Size;
			}
			if (navigationBarItem.Image != null)
			{
				return navigationBarItem.Image.Size;
			}
			if (_smallImageList != null)
			{
				return _smallImageList.ImageSize;
			}
			return new Size(16, 16);
		}

		private int MeasureGroupHeaderHeight(Graphics graphics, NavigationGroup navigationBarGroup)
		{
			NavigationBarFormatStyle navigationBarFormatStyle = GetGroupFormatStyle(navigationBarGroup);
			if ((navigationBarGroup._itemState & ItemState.Focus) != ItemState.Normal)
			{
				navigationBarFormatStyle = navigationBarFormatStyle.MergeWith(_hoverGroupFormatStyle);
			}
			Font headerFont = navigationBarFormatStyle.GetEffectiveFont(Font);
			int contentHeight = (int)headerFont.GetHeight(graphics);
			int height = GetGroupImageSize(navigationBarGroup).Height;
			if (height > contentHeight)
			{
				contentHeight = height;
			}
			return contentHeight + 6;
		}

		private int MeasureItemHeight(Graphics graphics, NavigationItem navigationBarItem, NavigationBarFormatStyle navigationBarFormatStyle)
		{
			Font itemFont = navigationBarFormatStyle.GetEffectiveFont(Font);
			int contentHeight = (int)itemFont.GetHeight(graphics);
			int height = GetItemImageSize(navigationBarItem).Height;
			if (height > contentHeight)
			{
				contentHeight = height;
			}
			return contentHeight + 6;
		}

		private void LayoutHorizontalItems(Graphics graphics, NavigationGroup navigationBarGroup)
		{
			NavigationBarStateStyles navigationBarStateStyles = ItemsStateStyles.MergeWith(navigationBarGroup.ItemsStateStyles);
			NavigationBarFormatStyle formatStyle = navigationBarStateStyles.FormatStyle;
			NavigationBarFormatStyle navigationBarFormatStyle = null;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateMnemonicEnabled(_useMnemonic);
			int currentTop = _itemsAreaBounds.Top;
			Font itemFont = null;
			int currentLeft;
			if (_firstVisibleItem == null || !_firstVisibleItem.Visible)
			{
				_firstVisibleItem = GetFirstVisibleItem(_selectedGroup);
				currentLeft = _itemsAreaBounds.Left;
			}
			else
			{
				currentLeft = _firstVisibleItem.GetBounds().Left;
			}
			if (_firstVisibleItem == null)
			{
				return;
			}
			if (navigationBarGroup.View == NavigationBarView.LargeIcons)
			{
				textLayoutRenderer.UpdateWrapEnabled(true);
				int iconWidth = 32;
				int iconHeight = 32;
				int textLineHeight = (int)formatStyle.GetEffectiveFont(Font).GetHeight(graphics);
				currentTop = (_itemsAreaBounds.Height - (iconHeight + 3 + textLineHeight * 2)) / 2;
				currentLeft = _itemsAreaBounds.Left + 12;
				for (int i = _firstVisibleItem._index; i < navigationBarGroup.Items.Count; i++)
				{
					NavigationItem navigationBarItem = navigationBarGroup.Items[i];
					if (navigationBarItem.Visible)
					{
						navigationBarFormatStyle = navigationBarStateStyles.MergeWith(navigationBarItem.StateStyles).FormatStyle;
						itemFont = navigationBarFormatStyle.GetEffectiveFont(Font);
						Size imageSize = GetItemImageSize(navigationBarItem);
						iconWidth = imageSize.Width;
						iconHeight = imageSize.Height;
						Rectangle iconBounds = new Rectangle(currentLeft, currentTop, iconWidth, iconHeight);
						iconBounds.Inflate(2, 2);
						navigationBarItem.UpdateBounds(iconBounds);
						Rectangle textBounds = new Rectangle(iconBounds.Left - 10, navigationBarItem.GetBounds().Bottom, navigationBarItem.GetBounds().Width + 20, _itemsAreaBounds.Bottom - navigationBarItem.GetBounds().Bottom);
						textBounds.Height = (int)textLayoutRenderer.MeasureText(graphics, navigationBarItem.Text, itemFont, textBounds.Width).Height + 6;
						navigationBarItem.SetTextBounds(textBounds);
						currentLeft += navigationBarItem.GetBounds().Width + 25;
						navigationBarFormatStyle.DisposeBrushesAndFont();
					}
				}
				if (_firstVisibleItem._index > 0)
				{
					currentLeft = _firstVisibleItem.GetTextBounds().Left - 5;
					for (int previousIndex = _firstVisibleItem._index - 1; previousIndex >= 0; previousIndex--)
					{
						NavigationItem previousItem = navigationBarGroup.Items[previousIndex];
						if (previousItem.Visible)
						{
							Size previousImageSize = GetItemImageSize(previousItem);
							iconWidth = previousImageSize.Width + 4;
							iconHeight = previousImageSize.Height;
							currentLeft -= iconWidth + 20;
							Rectangle previousIconBounds = new Rectangle(currentLeft + 10, currentTop, iconWidth, iconHeight);
							previousIconBounds.Inflate(0, 2);
							previousItem.UpdateBounds(previousIconBounds);
							Rectangle previousTextBounds = new Rectangle(currentLeft, previousItem.GetBounds().Bottom, previousItem.GetBounds().Width + 20, _itemsAreaBounds.Bottom - previousItem.GetBounds().Bottom);
							previousItem.SetTextBounds(previousTextBounds);
							currentLeft = previousIconBounds.Left - 15;
						}
					}
				}
			}
			else
			{
				int columnWidth = 0;
				ArrayList arrayList = new ArrayList();
				for (int j = _firstVisibleItem._index; j < navigationBarGroup.Items.Count; j++)
				{
					NavigationItem entryItem = navigationBarGroup.Items[j];
					if (!entryItem.Visible)
					{
						continue;
					}
					navigationBarFormatStyle = navigationBarStateStyles.MergeWith(entryItem.StateStyles).FormatStyle;
					int entryWidth = GetItemImageSize(entryItem).Width + 9;
					if (entryItem.Text.Length > 0)
					{
						Font entryFont = navigationBarFormatStyle.GetEffectiveFont(Font);
						entryWidth += (int)textLayoutRenderer.MeasureText(graphics, entryItem.Text, entryFont, 0).Width + 3;
					}
					entryItem._fullWidth = 0;
					int entryHeight = MeasureItemHeight(graphics, entryItem, navigationBarFormatStyle);
					if (entryHeight + currentTop > _itemsAreaBounds.Bottom)
					{
						currentLeft += columnWidth;
						currentTop = _itemsAreaBounds.Top;
						foreach (NavigationItem item in arrayList)
						{
							item.UpdateBoundsWidth(columnWidth);
						}
						columnWidth = entryWidth;
						arrayList.Clear();
					}
					if (entryWidth > columnWidth)
					{
						columnWidth = entryWidth;
					}
					entryItem.UpdateBounds(new Rectangle(currentLeft, currentTop, entryWidth, entryHeight));
					currentTop += entryHeight;
					arrayList.Add(entryItem);
					navigationBarFormatStyle.DisposeBrushesAndFont();
				}
				foreach (NavigationItem columnItem in arrayList)
				{
					columnItem.UpdateBoundsWidth(columnWidth);
				}
				arrayList.Clear();
				if (_firstVisibleItem._index > 0)
				{
					columnWidth = 0;
					currentTop = _itemsAreaBounds.Bottom;
					currentLeft = _itemsAreaBounds.Left;
					for (int reverseIndex = _firstVisibleItem._index - 1; reverseIndex >= 0; reverseIndex--)
					{
						NavigationItem earlierItem = navigationBarGroup.Items[reverseIndex];
						if (earlierItem.Visible)
						{
							navigationBarFormatStyle = navigationBarStateStyles.MergeWith(earlierItem.StateStyles).FormatStyle;
							int earlierWidth = GetItemImageSize(earlierItem).Width + 9;
							if (earlierItem.Text.Length > 0)
							{
								Font earlierFont = navigationBarFormatStyle.GetEffectiveFont(Font);
								earlierWidth += (int)textLayoutRenderer.MeasureText(graphics, earlierItem.Text, earlierFont, 0).Width + 3;
							}
							if (earlierWidth > columnWidth)
							{
								columnWidth = earlierWidth;
							}
							earlierItem._fullWidth = 0;
							int earlierHeight = MeasureItemHeight(graphics, earlierItem, navigationBarFormatStyle);
							currentTop -= earlierHeight;
							earlierItem.UpdateBounds(new Rectangle(currentLeft, currentTop, earlierWidth, earlierHeight));
							arrayList.Add(earlierItem);
							if (currentTop - earlierHeight < _itemsAreaBounds.Top)
							{
								currentLeft -= columnWidth;
								int columnOverflow = currentTop - _itemsAreaBounds.Top;
								foreach (NavigationItem wrappedItem in arrayList)
								{
									wrappedItem.UpdateBoundsWidth(columnWidth);
									wrappedItem.UpdateBoundsX(wrappedItem.GetBoundsX() - columnWidth);
									wrappedItem.UpdateBoundsY(wrappedItem.GetBoundsY() - Math.Abs(columnOverflow));
								}
								currentTop = _itemsAreaBounds.Bottom;
								columnWidth = 0;
								arrayList.Clear();
							}
							navigationBarFormatStyle.DisposeBrushesAndFont();
						}
					}
				}
				int lastColumnOverflow = currentTop - _itemsAreaBounds.Top;
				foreach (NavigationItem leftoverItem in arrayList)
				{
					leftoverItem.UpdateBoundsWidth(columnWidth);
					leftoverItem.UpdateBoundsX(leftoverItem.GetBoundsX() - columnWidth);
					leftoverItem.UpdateBoundsY(leftoverItem.GetBoundsY() - Math.Abs(lastColumnOverflow));
				}
				arrayList.Clear();
			}
			textLayoutRenderer.InvalidateFormat();
			formatStyle.DisposeBrushesAndFont();
		}

		private void LayoutLargeIconItems(Graphics graphics, NavigationGroup navigationBarGroup)
		{
			NavigationBarStateStyles navigationBarStateStyles = ItemsStateStyles.MergeWith(navigationBarGroup.ItemsStateStyles);
			int itemTop = _itemsAreaBounds.Top;
			int itemLeft = _itemsAreaBounds.Left;
			if (_firstVisibleItem == null || !_firstVisibleItem.Visible)
			{
				_firstVisibleItem = GetFirstVisibleItem(_selectedGroup);
			}
			int imageHeight;
			if (_largeImageList != null)
			{
				int width = _largeImageList.ImageSize.Width;
				imageHeight = _largeImageList.ImageSize.Height;
			}
			else
			{
				imageHeight = 32;
			}
			if (_firstVisibleItem == null)
			{
				return;
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateWrapEnabled(true);
			textLayoutRenderer.UpdateMnemonicEnabled(_useMnemonic);
			int height;
			if (_orientation == NavigationBarOrientation.Vertical)
			{
				int barWidth = _itemsAreaBounds.Width;
				for (int i = _firstVisibleItem._index; i < navigationBarGroup.Items.Count; i++)
				{
					NavigationItem navigationBarItem = navigationBarGroup.Items[i];
					if (navigationBarItem.Visible)
					{
						NavigationBarFormatStyle formatStyle = navigationBarStateStyles.MergeWith(navigationBarItem.StateStyles).FormatStyle;
						Size imageSize = GetItemImageSize(navigationBarItem);
						int width = imageSize.Width;
						imageHeight = imageSize.Height;
						int x = (barWidth - width) / 2;
						Font textFont = formatStyle.GetEffectiveFont(Font);
						Rectangle textBounds = new Rectangle(itemLeft, itemTop + imageHeight + 7, barWidth, (int)textFont.GetHeight(graphics));
						textBounds.Height = (int)textLayoutRenderer.MeasureText(graphics, navigationBarItem.Text, textFont, textBounds.Width).Height;
						textBounds.Width++;
						textBounds.X = itemLeft + (barWidth - textBounds.Width) / 2;
						navigationBarItem.SetTextBounds(textBounds);
						height = navigationBarItem.GetTextBounds().Bottom + 5 - itemTop;
						navigationBarItem.UpdateBounds(new Rectangle(itemLeft, itemTop, barWidth, height));
						navigationBarItem.UpdateImageBounds(new Rectangle(x, itemTop + 5, width, imageHeight));
						itemTop = navigationBarItem.GetBounds().Bottom;
						formatStyle.DisposeBrushesAndFont();
					}
				}
				if (_firstVisibleItem._index <= 0)
				{
					return;
				}
				itemTop = _firstVisibleItem.GetBounds().Top;
				for (int reverseIndex = _firstVisibleItem._index - 1; reverseIndex >= 0; reverseIndex--)
				{
					NavigationItem earlierItem = navigationBarGroup.Items[reverseIndex];
					if (earlierItem.Visible)
					{
						NavigationBarFormatStyle formatStyle = navigationBarStateStyles.MergeWith(earlierItem.StateStyles).FormatStyle;
						Size earlierImageSize = GetItemImageSize(earlierItem);
						int width = earlierImageSize.Width;
						imageHeight = earlierImageSize.Height;
						int x = (barWidth - width) / 2;
						Font earlierFont = formatStyle.GetEffectiveFont(Font);
						Rectangle earlierTextBounds = new Rectangle(itemLeft, 0, barWidth, (int)earlierFont.GetHeight(graphics));
						earlierTextBounds.Size = textLayoutRenderer.MeasureText(graphics, earlierItem.Text, earlierFont, earlierTextBounds.Width).ToSize();
						earlierTextBounds.Width++;
						earlierTextBounds.X = itemLeft + (barWidth - earlierTextBounds.Width) / 2;
						earlierItem.SetTextBounds(earlierTextBounds);
						height = earlierItem.GetTextBounds().Height + imageHeight + 12;
						itemTop -= height;
						earlierItem.UpdateBounds(new Rectangle(itemLeft, itemTop, barWidth, height));
						earlierItem.UpdateImageBounds(new Rectangle(x, itemTop + 5, width, imageHeight));
						earlierItem.UpdateTextY(earlierItem.GetImageBounds().Bottom + 2);
						formatStyle.DisposeBrushesAndFont();
					}
				}
				return;
			}
			height = _itemsAreaBounds.Bottom - _itemsAreaBounds.Top;
			NavigationBarFormatStyle defaultFormatStyle = navigationBarStateStyles.FormatStyle;
			int twoLineTextHeight = (int)defaultFormatStyle.GetEffectiveFont(Font).GetHeight(graphics) * 2;
			defaultFormatStyle.DisposeBrushesAndFont();
			int imageTop = itemTop;
			imageTop += (height - imageHeight - twoLineTextHeight) / 2;
			for (int j = _firstVisibleItem._index; j < navigationBarGroup.Items.Count; j++)
			{
				NavigationItem rowItem = navigationBarGroup.Items[j];
				if (rowItem.Visible)
				{
					NavigationBarFormatStyle formatStyle = navigationBarStateStyles.MergeWith(rowItem.StateStyles).FormatStyle;
					Size rowImageSize = GetItemImageSize(rowItem);
					int width = rowImageSize.Width;
					imageHeight = rowImageSize.Height;
					int slotWidth = width + 24;
					int x = (slotWidth - width) / 2 + itemLeft;
					Font rowFont = formatStyle.GetEffectiveFont(Font);
					Rectangle rowTextBounds = new Rectangle(itemLeft, imageTop + imageHeight + 4, slotWidth, (int)rowFont.GetHeight(graphics));
					rowTextBounds.Height = (int)textLayoutRenderer.MeasureText(graphics, rowItem.Text, rowFont, rowTextBounds.Width).Height;
					rowItem.SetTextBounds(rowTextBounds);
					rowItem.UpdateBounds(new Rectangle(itemLeft, itemTop, slotWidth, height));
					rowItem.UpdateImageBounds(new Rectangle(x, imageTop, width, imageHeight));
					itemLeft = rowItem.GetBounds().Right;
					formatStyle.DisposeBrushesAndFont();
				}
			}
			if (_firstVisibleItem._index <= 0)
			{
				return;
			}
			itemLeft = _firstVisibleItem.GetBounds().Left;
			for (int rowReverseIndex = _firstVisibleItem._index - 1; rowReverseIndex >= 0; rowReverseIndex--)
			{
				NavigationItem earlierRowItem = navigationBarGroup.Items[rowReverseIndex];
				if (earlierRowItem.Visible)
				{
					NavigationBarFormatStyle formatStyle = navigationBarStateStyles.MergeWith(earlierRowItem.StateStyles).FormatStyle;
					Size earlierRowImageSize = GetItemImageSize(earlierRowItem);
					int width = earlierRowImageSize.Width;
					imageHeight = earlierRowImageSize.Height;
					int earlierSlotWidth = width + 24;
					itemLeft -= earlierSlotWidth;
					int x = (earlierSlotWidth - width) / 2 + itemLeft;
					Font earlierRowFont = formatStyle.GetEffectiveFont(Font);
					Rectangle earlierRowTextBounds = new Rectangle(itemLeft, imageTop + imageHeight + 4, earlierSlotWidth, (int)earlierRowFont.GetHeight(graphics));
					earlierRowTextBounds.Height = (int)textLayoutRenderer.MeasureText(graphics, earlierRowItem.Text, earlierRowFont, earlierRowTextBounds.Width).Height;
					earlierRowItem.SetTextBounds(earlierRowTextBounds);
					earlierRowItem.UpdateBounds(new Rectangle(itemLeft, itemTop, earlierSlotWidth, height));
					earlierRowItem.UpdateImageBounds(new Rectangle(x, imageTop, width, imageHeight));
					formatStyle.DisposeBrushesAndFont();
				}
			}
		}

		private void LayoutVerticalItems(Graphics graphics, NavigationGroup navigationBarGroup)
		{
			if (navigationBarGroup == null || navigationBarGroup.Container || navigationBarGroup.Items.Count <= 0 || !navigationBarGroup.Visible)
			{
				return;
			}
			if (navigationBarGroup.View == NavigationBarView.LargeIcons && _selectionArea == SelectionArea.FullItem)
			{
				LayoutLargeIconItems(graphics, navigationBarGroup);
				return;
			}
			if (Orientation == NavigationBarOrientation.Horizontal)
			{
				LayoutHorizontalItems(graphics, navigationBarGroup);
				return;
			}
			int itemTop;
			if (_firstVisibleItem == null || !_firstVisibleItem.Visible)
			{
				_firstVisibleItem = GetFirstVisibleItem(_selectedGroup);
				itemTop = _itemsAreaBounds.Top;
			}
			else
			{
				itemTop = _firstVisibleItem.GetBounds().Top;
			}
			if (_firstVisibleItem == null)
			{
				return;
			}
			NavigationBarStateStyles navigationBarStateStyles = ItemsStateStyles.MergeWith(navigationBarGroup.ItemsStateStyles);
			if (navigationBarGroup.View == NavigationBarView.SmallIcons)
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				textLayoutRenderer.UpdateMnemonicEnabled(_useMnemonic);
				for (int i = _firstVisibleItem._index; i < navigationBarGroup.Items.Count; i++)
				{
					NavigationItem navigationBarItem = navigationBarGroup.Items[i];
					if (navigationBarItem.Visible)
					{
						NavigationBarFormatStyle formatStyle = navigationBarStateStyles.MergeWith(navigationBarItem.StateStyles).FormatStyle;
						int entryWidth = GetItemImageSize(navigationBarItem).Width + 9;
						if (navigationBarItem.Text.Length > 0)
						{
							Font entryFont = formatStyle.GetEffectiveFont(Font);
							entryWidth += (int)textLayoutRenderer.MeasureText(graphics, navigationBarItem.Text, entryFont, 0).Width + 3;
						}
						navigationBarItem._fullWidth = entryWidth;
						int entryHeight = MeasureItemHeight(graphics, navigationBarItem, formatStyle);
						navigationBarItem.UpdateBounds(new Rectangle(_itemsAreaBounds.Left, itemTop, _itemsAreaBounds.Width, entryHeight));
						itemTop += entryHeight;
						formatStyle.DisposeBrushesAndFont();
					}
				}
				if (_firstVisibleItem._index <= 0)
				{
					return;
				}
				itemTop = _firstVisibleItem.GetBounds().Top;
				for (int reverseIndex = _firstVisibleItem._index - 1; reverseIndex >= 0; reverseIndex--)
				{
					NavigationItem earlierItem = navigationBarGroup.Items[reverseIndex];
					if (earlierItem.Visible)
					{
						NavigationBarFormatStyle formatStyle = navigationBarStateStyles.MergeWith(earlierItem.StateStyles).FormatStyle;
						int earlierEntryWidth = GetItemImageSize(earlierItem).Width + 9;
						if (earlierItem.Text.Length > 0)
						{
							Font earlierFont = formatStyle.GetEffectiveFont(Font);
							earlierEntryWidth += (int)textLayoutRenderer.MeasureText(graphics, earlierItem.Text, earlierFont, 0).Width + 3;
						}
						earlierItem._fullWidth = earlierEntryWidth;
						int earlierEntryHeight = MeasureItemHeight(graphics, earlierItem, formatStyle);
						itemTop -= earlierEntryHeight;
						earlierItem.UpdateBounds(new Rectangle(_itemsAreaBounds.Left, itemTop, _itemsAreaBounds.Width, earlierEntryHeight));
						formatStyle.DisposeBrushesAndFont();
						formatStyle = null;
					}
				}
				textLayoutRenderer.InvalidateFormat();
			}
			else
			{
				if (navigationBarGroup.View != NavigationBarView.LargeIcons)
				{
					return;
				}
				int iconWidth;
				if (_largeImageList != null)
				{
					iconWidth = _largeImageList.ImageSize.Width;
					int height = _largeImageList.ImageSize.Height;
				}
				else
				{
					iconWidth = 32;
				}
				int iconLeft = (_innerBounds.Width - iconWidth) / 2;
				itemTop = ((itemTop != _itemsAreaBounds.Top) ? (itemTop + 2) : (itemTop + 10));
				TextLayoutRenderer largeItemTextRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				largeItemTextRenderer.UpdateStringAlignment(StringAlignment.Center);
				largeItemTextRenderer.UpdateWrapEnabled(true);
				largeItemTextRenderer.UpdateMnemonicEnabled(_useMnemonic);
				Size empty = Size.Empty;
				for (int j = _firstVisibleItem._index; j < navigationBarGroup.Items.Count; j++)
				{
					NavigationItem largeItem = navigationBarGroup.Items[j];
					if (largeItem.Visible)
					{
						NavigationBarFormatStyle formatStyle = navigationBarStateStyles.MergeWith(largeItem.StateStyles).FormatStyle;
						empty = GetItemImageSize(largeItem);
						iconWidth = empty.Width;
						int height = empty.Height;
						iconLeft = (_innerBounds.Width - iconWidth) / 2;
						Rectangle iconBounds = new Rectangle(iconLeft, itemTop, iconWidth, height);
						iconBounds.Inflate(2, 2);
						largeItem.UpdateBounds(iconBounds);
						Font largeFont = formatStyle.GetEffectiveFont(Font);
						Rectangle textBounds = new Rectangle(0, largeItem.GetBounds().Bottom + 5, _itemsAreaBounds.Width, (int)largeFont.GetHeight(graphics));
						textBounds.Size = largeItemTextRenderer.MeasureText(graphics, largeItem.Text, largeFont, textBounds.Width).ToSize();
						textBounds.X = _itemsAreaBounds.Left + (_itemsAreaBounds.Width - textBounds.Width) / 2;
						textBounds.Inflate(3, 3);
						largeItem.SetTextBounds(textBounds);
						itemTop = largeItem.GetTextBounds().Bottom + 10;
						formatStyle.DisposeBrushesAndFont();
					}
				}
				if (_firstVisibleItem._index <= 0)
				{
					return;
				}
				itemTop = _firstVisibleItem.GetBounds().Top;
				for (int largeReverseIndex = _firstVisibleItem._index - 1; largeReverseIndex >= 0; largeReverseIndex--)
				{
					NavigationItem earlierLargeItem = navigationBarGroup.Items[largeReverseIndex];
					if (earlierLargeItem.Visible)
					{
						NavigationBarFormatStyle formatStyle = navigationBarStateStyles.MergeWith(earlierLargeItem.StateStyles).FormatStyle;
						empty = GetItemImageSize(earlierLargeItem);
						iconWidth = empty.Width;
						int height = empty.Height;
						iconLeft = (_innerBounds.Width - iconWidth) / 2;
						Rectangle earlierIconBounds = new Rectangle(iconLeft, itemTop, iconWidth, height);
						earlierIconBounds.Inflate(2, 2);
						earlierLargeItem.UpdateBounds(earlierIconBounds);
						Font earlierLargeFont = formatStyle.GetEffectiveFont(Font);
						Rectangle earlierTextBounds = new Rectangle(0, earlierLargeItem.GetBounds().Bottom + 5, _itemsAreaBounds.Width, (int)earlierLargeFont.GetHeight(graphics));
						earlierTextBounds.Size = largeItemTextRenderer.MeasureText(graphics, earlierLargeItem.Text, earlierLargeFont, earlierTextBounds.Width).ToSize();
						earlierTextBounds.X = _itemsAreaBounds.Left + (_itemsAreaBounds.Width - earlierTextBounds.Width) / 2;
						earlierTextBounds.Inflate(3, 3);
						earlierLargeItem.SetTextBounds(earlierTextBounds);
						itemTop -= earlierLargeItem.GetTextBounds().Height + earlierLargeItem.GetBounds().Height + 10;
						earlierLargeItem.UpdateBoundsY(itemTop);
						earlierLargeItem.UpdateTextY(earlierLargeItem.GetBounds().Bottom);
						formatStyle.DisposeBrushesAndFont();
					}
				}
			}
		}

		private void LayoutTrailingHeadersHorizontal(Graphics graphics)
		{
			int top = _innerBounds.Top;
			int leftEdge = _innerBounds.Right;
			int height = _innerBounds.Height;
			for (int groupIndex = _groups.Count - 1; groupIndex >= 0; groupIndex--)
			{
				NavigationGroup navigationBarGroup = _groups[groupIndex];
				if (navigationBarGroup.Selected)
				{
					break;
				}
				if (navigationBarGroup.Visible)
				{
					int headerWidth = MeasureGroupHeaderHeight(graphics, navigationBarGroup);
					leftEdge -= headerWidth;
					Rectangle headerBounds = new Rectangle(leftEdge, top, headerWidth, height);
					navigationBarGroup.SetBounds(headerBounds);
					if (navigationBarGroup.Container)
					{
						_hasContainerGroup = true;
						if (navigationBarGroup.ContainerControl == null)
						{
							UpdateGroupContainerControl(navigationBarGroup);
						}
						NavigationBarContainerControl containerControl = navigationBarGroup.ContainerControl;
						if (containerControl._owner == null)
						{
							containerControl._owner = this;
							containerControl._group = navigationBarGroup;
						}
						containerControl.Visible = false;
					}
				}
				else
				{
					if (navigationBarGroup.ContainerControl != null)
					{
						navigationBarGroup.ContainerControl.Visible = false;
					}
					navigationBarGroup.SetBounds(Rectangle.Empty);
				}
			}
			_itemsAreaBounds.Width = leftEdge - _itemsAreaBounds.Left;
		}

		private void LayoutTrailingHeadersVertical(Graphics graphics)
		{
			int topEdge = _innerBounds.Bottom;
			int headerHeight = 0;
			for (int groupIndex = _groups.Count - 1; groupIndex >= 0; groupIndex--)
			{
				NavigationGroup navigationBarGroup = _groups[groupIndex];
				if (navigationBarGroup.Selected)
				{
					break;
				}
				if (navigationBarGroup.Visible)
				{
					headerHeight = MeasureGroupHeaderHeight(graphics, _groups[groupIndex]);
					topEdge -= headerHeight;
					Rectangle headerBounds = new Rectangle(_innerBounds.Left, topEdge, _innerBounds.Width, headerHeight);
					_groups[groupIndex].SetBounds(headerBounds);
					if (navigationBarGroup.Container)
					{
						_hasContainerGroup = true;
						if (navigationBarGroup.ContainerControl == null)
						{
							UpdateGroupContainerControl(navigationBarGroup);
						}
						NavigationBarContainerControl containerControl = navigationBarGroup.ContainerControl;
						if (containerControl._owner == null)
						{
							containerControl._owner = this;
							containerControl._group = navigationBarGroup;
						}
						containerControl.Visible = false;
					}
				}
				else
				{
					if (navigationBarGroup.ContainerControl != null)
					{
						navigationBarGroup.ContainerControl.Visible = false;
					}
					_groups[groupIndex].SetBounds(Rectangle.Empty);
				}
			}
			_itemsAreaBounds.Height = topEdge - _itemsAreaBounds.Top;
		}

		private void LayoutGroupsHorizontal(Graphics graphics)
		{
			int headerLeft = _innerBounds.Left;
			int top = _innerBounds.Top;
			int height = _innerBounds.Height;
			int horizontalScrollBarArrowWidth = SystemInformation.HorizontalScrollBarArrowWidth;
			int horizontalScrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
			if (_headerGroupVisible && _selectedGroup != null)
			{
				foreach (NavigationGroup item in _groups)
				{
					if (item.Visible)
					{
						int headerWidth = MeasureGroupHeaderHeight(graphics, item);
						Rectangle headerBounds = new Rectangle(headerLeft, top, headerWidth, height);
						item.SetBounds(headerBounds);
						if (item.Container)
						{
							_hasContainerGroup = true;
							if (item.ContainerControl == null)
							{
								UpdateGroupContainerControl(item);
							}
							NavigationBarContainerControl containerControl = item.ContainerControl;
							if (containerControl._owner == null)
							{
								containerControl._owner = this;
								containerControl._group = item;
							}
							containerControl.Visible = false;
						}
						if (item.Selected && item._index != _groups.Count - 1)
						{
							_itemsAreaBounds.X = item.Bounds.Right;
							_itemsAreaBounds.Height = _innerBounds.Height;
							_itemsAreaBounds.Y = top;
							LayoutTrailingHeadersHorizontal(graphics);
							break;
						}
						headerLeft += headerWidth;
					}
					else
					{
						if (item.ContainerControl != null)
						{
							item.ContainerControl.Visible = false;
						}
						item.SetBounds(Rectangle.Empty);
					}
				}
				if (_scrollLocation == ScrollLocation.GroupsLevel)
				{
					Rectangle bounds = _selectedGroup.Bounds;
					int width = _selectedGroup.Bounds.Width;
					bounds.Height -= width;
					bounds.Y += width;
					_selectedGroup.SetBounds(bounds);
					_scrollPreviousButton.Location = new Point(_selectedGroup.Bounds.Left, _innerBounds.Top);
					_scrollPreviousButton.Size = new Size(width, width);
					NavigationGroup nextVisibleGroup = GetNextVisibleGroup();
					if (_selectedGroup == nextVisibleGroup)
					{
						_isSelectedGroupLast = true;
						_itemsAreaBounds.X = _selectedGroup.Bounds.Right;
						_itemsAreaBounds.Width = _innerBounds.Right - _itemsAreaBounds.X - width;
						_itemsAreaBounds.Y = top;
						_itemsAreaBounds.Height = _innerBounds.Height;
						_scrollNextButton.Location = new Point(_itemsAreaBounds.Right, _scrollPreviousButton.Top);
						_scrollNextButton.Size = new Size(width, width);
					}
					else
					{
						bounds = nextVisibleGroup.Bounds;
						width = nextVisibleGroup.Bounds.Width;
						bounds.Y += width;
						bounds.Height -= width;
						nextVisibleGroup.SetBounds(bounds);
						_scrollNextButton.Location = new Point(nextVisibleGroup.Bounds.Left, _innerBounds.Top);
						_scrollNextButton.Size = new Size(width, width);
					}
				}
				else
				{
					if (_selectedGroup._index == _groups.Count - 1)
					{
						_itemsAreaBounds.X = _selectedGroup.Bounds.Right;
						_itemsAreaBounds.Width = _innerBounds.Width - _itemsAreaBounds.X;
						_itemsAreaBounds.Y = _innerBounds.Top;
						_itemsAreaBounds.Height = _innerBounds.Height;
					}
					_scrollPreviousButton.Location = new Point(_itemsAreaBounds.Left + 5, _itemsAreaBounds.Top + 6);
					_scrollPreviousButton.Size = new Size(horizontalScrollBarArrowWidth, horizontalScrollBarHeight);
					_scrollNextButton.Location = new Point(_itemsAreaBounds.Right - (horizontalScrollBarArrowWidth + 5), _itemsAreaBounds.Top + 6);
					_scrollNextButton.Size = new Size(horizontalScrollBarArrowWidth, horizontalScrollBarHeight);
				}
				if (_selectedGroup != null && _selectedGroup._container && _selectedGroup.ContainerControl != null)
				{
					_selectedGroup.ContainerControl.Bounds = _itemsAreaBounds;
					_selectedGroup.ContainerControl.Visible = true;
				}
				UpdateScrollButtonBorders();
				return;
			}
			_itemsAreaBounds = _innerBounds;
			foreach (NavigationGroup hiddenGroup in _groups)
			{
				hiddenGroup.SetBounds(Rectangle.Empty);
				if (!hiddenGroup.Container)
				{
					continue;
				}
				_hasContainerGroup = true;
				if (hiddenGroup.Visible)
				{
					if (hiddenGroup.ContainerControl == null)
					{
						UpdateGroupContainerControl(hiddenGroup);
					}
					NavigationBarContainerControl groupContainer = hiddenGroup.ContainerControl;
					if (groupContainer._owner == null)
					{
						groupContainer._owner = this;
						groupContainer._group = hiddenGroup;
					}
					groupContainer.Visible = false;
				}
				else
				{
					if (hiddenGroup.ContainerControl != null)
					{
						hiddenGroup.ContainerControl.Visible = false;
					}
					hiddenGroup.SetBounds(Rectangle.Empty);
				}
			}
			if (_scrollLocation == ScrollLocation.GroupsLevel)
			{
				_scrollPreviousButton.Location = new Point(_itemsAreaBounds.Left, top);
				_scrollPreviousButton.Size = new Size(horizontalScrollBarArrowWidth, horizontalScrollBarHeight);
				_scrollNextButton.Location = new Point(_itemsAreaBounds.Right - horizontalScrollBarArrowWidth, top);
				_scrollNextButton.Size = new Size(horizontalScrollBarArrowWidth, horizontalScrollBarHeight);
			}
			else
			{
				_scrollPreviousButton.Location = new Point(_itemsAreaBounds.Left + 5, _itemsAreaBounds.Top + 6);
				_scrollPreviousButton.Size = new Size(horizontalScrollBarArrowWidth, horizontalScrollBarHeight);
				_scrollNextButton.Location = new Point(_itemsAreaBounds.Right - (horizontalScrollBarArrowWidth + 5), _itemsAreaBounds.Top + 6);
				_scrollNextButton.Size = new Size(horizontalScrollBarArrowWidth, horizontalScrollBarHeight);
			}
			if (_selectedGroup != null && _selectedGroup._container && _selectedGroup.ContainerControl != null)
			{
				_selectedGroup.ContainerControl.Bounds = _itemsAreaBounds;
				_selectedGroup.ContainerControl.Visible = true;
			}
			UpdateScrollButtonBorders();
		}

		private NavigationGroup GetPreviousVisibleGroup()
		{
			for (int index = _selectedGroup._index - 1; index >= 0; index--)
			{
				if (_groups[index].Visible)
				{
					return _groups[index];
				}
			}
			return _selectedGroup;
		}

		private void UpdateScrollButtonBorders()
		{
			if (_orientation == NavigationBarOrientation.Vertical)
			{
				if (_scrollLocation == ScrollLocation.GroupsLevel && (_groupAppearance == GroupAppearance.Flat || _groupAppearance == GroupAppearance.PopUp))
				{
					BorderFlags borderFlags = BorderFlags.Bottom;
					if (GetEffectiveBorderStyle() == BorderStyle.None && _scrollPreviousButton.Bounds.Top == _innerBounds.Top)
					{
						borderFlags |= BorderFlags.Top;
					}
					if (GetEffectiveBorderStyle() != BorderStyle.Flat)
					{
						borderFlags |= BorderFlags.Right;
					}
					if (!_headerGroupVisible || _groups.Count == 0)
					{
						borderFlags |= BorderFlags.Left;
					}
					_scrollPreviousButton._borderFlags = borderFlags;
					borderFlags = BorderFlags.Top;
					if (GetEffectiveBorderStyle() != BorderStyle.Flat)
					{
						borderFlags |= BorderFlags.Right;
					}
					if (_scrollNextButton.Bounds.Bottom != _innerBounds.Bottom)
					{
						borderFlags |= BorderFlags.Bottom;
					}
					else if (GetEffectiveBorderStyle() != BorderStyle.Flat)
					{
						borderFlags |= BorderFlags.Bottom;
					}
					if (!_headerGroupVisible || _groups.Count == 0)
					{
						borderFlags |= BorderFlags.Left;
					}
					else
					{
						NavigationGroup navigationBarGroup = GetNextVisibleGroup();
						if (_selectedGroup == navigationBarGroup)
						{
							borderFlags |= BorderFlags.Left;
						}
					}
					_scrollNextButton._borderFlags = borderFlags;
				}
				else
				{
					_scrollNextButton._borderFlags = BorderFlags.All;
					_scrollPreviousButton._borderFlags = BorderFlags.All;
				}
			}
			else if (_scrollLocation == ScrollLocation.GroupsLevel && (_groupAppearance == GroupAppearance.Flat || _groupAppearance == GroupAppearance.PopUp))
			{
				BorderFlags horizontalBorders = BorderFlags.Right;
				if (GetEffectiveBorderStyle() == BorderStyle.None && _scrollPreviousButton.Bounds.Left == _innerBounds.Left)
				{
					horizontalBorders |= BorderFlags.Left;
				}
				if (GetEffectiveBorderStyle() != BorderStyle.Flat)
				{
					horizontalBorders |= BorderFlags.Top;
				}
				horizontalBorders |= BorderFlags.Bottom;
				_scrollPreviousButton._borderFlags = horizontalBorders;
				horizontalBorders = BorderFlags.Left;
				if (GetEffectiveBorderStyle() != BorderStyle.Flat)
				{
					horizontalBorders |= BorderFlags.Top;
				}
				if (_scrollNextButton.Bounds.Right != _innerBounds.Right)
				{
					horizontalBorders |= BorderFlags.Right;
				}
				else if (GetEffectiveBorderStyle() != BorderStyle.Flat)
				{
					horizontalBorders |= BorderFlags.Right;
				}
				horizontalBorders |= BorderFlags.Bottom;
				_scrollNextButton._borderFlags = horizontalBorders;
			}
			else
			{
				_scrollNextButton._borderFlags = BorderFlags.All;
				_scrollPreviousButton._borderFlags = BorderFlags.All;
			}
		}

		internal void LayoutGroups(Graphics graphics)
		{
			try
			{
				_isSelectedGroupLast = false;
				int headerTop = _innerBounds.Top;
				_hasContainerGroup = false;
				if (Orientation == NavigationBarOrientation.Horizontal)
				{
					LayoutGroupsHorizontal(graphics);
					return;
				}
				int verticalScrollBarArrowHeight = SystemInformation.VerticalScrollBarArrowHeight;
				int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
				if (_headerGroupVisible && _selectedGroup != null)
				{
					foreach (NavigationGroup item in _groups)
					{
						if (item.Visible)
						{
							int headerHeight = MeasureGroupHeaderHeight(graphics, item);
							Rectangle headerBounds = new Rectangle(_innerBounds.Left, headerTop, _innerBounds.Width, headerHeight);
							item.SetBounds(headerBounds);
							if (item.Container)
							{
								_hasContainerGroup = true;
								if (item.ContainerControl == null)
								{
									UpdateGroupContainerControl(item);
								}
								NavigationBarContainerControl containerControl = item.ContainerControl;
								if (containerControl._owner == null)
								{
									containerControl._owner = this;
									containerControl._group = item;
								}
								containerControl.Visible = false;
							}
							if (item.Selected && item.Index != _groups.Count - 1)
							{
								_itemsAreaBounds.X = _innerBounds.Left;
								_itemsAreaBounds.Width = _innerBounds.Width;
								_itemsAreaBounds.Y = item.Bounds.Bottom;
								LayoutTrailingHeadersVertical(graphics);
								headerTop += headerHeight;
								break;
							}
							headerTop += headerHeight;
						}
						else
						{
							if (item._container && item.ContainerControl != null)
							{
								item.ContainerControl.Visible = false;
							}
							item.SetBounds(Rectangle.Empty);
						}
					}
					if (_scrollLocation == ScrollLocation.GroupsLevel)
					{
						Rectangle bounds = _selectedGroup.Bounds;
						int height = bounds.Height;
						bounds.Width -= height;
						_selectedGroup.SetBounds(bounds);
						_scrollPreviousButton.Location = new Point(_selectedGroup.Bounds.Right, _selectedGroup.Bounds.Top);
						_scrollPreviousButton.Size = new Size(height, height);
						NavigationGroup nextVisibleGroup = GetNextVisibleGroup();
						if (_selectedGroup == nextVisibleGroup)
						{
							_isSelectedGroupLast = true;
							_itemsAreaBounds.X = _innerBounds.Left;
							_itemsAreaBounds.Width = _innerBounds.Width;
							_itemsAreaBounds.Y = _selectedGroup.Bounds.Bottom;
							_itemsAreaBounds.Height = _innerBounds.Bottom - _itemsAreaBounds.Top - height;
							_scrollNextButton.Location = new Point(_scrollPreviousButton.Left, _itemsAreaBounds.Bottom);
							_scrollNextButton.Size = new Size(height, height);
						}
						else
						{
							bounds = nextVisibleGroup.Bounds;
							height = bounds.Height;
							bounds.Width -= height;
							nextVisibleGroup.SetBounds(bounds);
							_scrollNextButton.Location = new Point(nextVisibleGroup.Bounds.Right, _itemsAreaBounds.Bottom);
							_scrollNextButton.Size = new Size(height, height);
						}
					}
					else
					{
						NavigationGroup followingGroup = GetNextVisibleGroup();
						if (_selectedGroup == followingGroup)
						{
							_itemsAreaBounds.X = _innerBounds.Left;
							_itemsAreaBounds.Width = _innerBounds.Width;
							_itemsAreaBounds.Y = headerTop;
							_itemsAreaBounds.Height = _innerBounds.Bottom - _itemsAreaBounds.Top;
						}
						_scrollPreviousButton.Location = new Point(_itemsAreaBounds.Right - (verticalScrollBarWidth + 5), _itemsAreaBounds.Top + 6);
						_scrollPreviousButton.Size = new Size(verticalScrollBarWidth, verticalScrollBarArrowHeight);
						_scrollNextButton.Location = new Point(_itemsAreaBounds.Right - (verticalScrollBarWidth + 5), _itemsAreaBounds.Bottom - (verticalScrollBarArrowHeight + 5));
						_scrollNextButton.Size = new Size(verticalScrollBarWidth, verticalScrollBarArrowHeight);
					}
					if (_selectedGroup != null && _selectedGroup._container && _selectedGroup.ContainerControl != null)
					{
						_selectedGroup.ContainerControl.Bounds = _itemsAreaBounds;
						_selectedGroup.ContainerControl.Visible = true;
					}
					UpdateScrollButtonBorders();
					return;
				}
				_itemsAreaBounds = _innerBounds;
				foreach (NavigationGroup hiddenGroup in _groups)
				{
					hiddenGroup.SetBounds(Rectangle.Empty);
					if (!hiddenGroup.Container)
					{
						continue;
					}
					_hasContainerGroup = true;
					if (hiddenGroup.Visible)
					{
						if (hiddenGroup.ContainerControl == null)
						{
							UpdateGroupContainerControl(hiddenGroup);
						}
						NavigationBarContainerControl groupContainer = hiddenGroup.ContainerControl;
						if (groupContainer._owner == null)
						{
							groupContainer._owner = this;
							groupContainer._group = hiddenGroup;
						}
						groupContainer.Visible = false;
					}
					else
					{
						if (hiddenGroup.ContainerControl != null)
						{
							hiddenGroup.ContainerControl.Visible = false;
						}
						hiddenGroup.SetBounds(Rectangle.Empty);
					}
				}
				if (_scrollLocation == ScrollLocation.GroupsLevel)
				{
					_scrollPreviousButton.Location = new Point(_itemsAreaBounds.Right - verticalScrollBarWidth, _itemsAreaBounds.Top);
					_scrollPreviousButton.Size = new Size(verticalScrollBarWidth, verticalScrollBarArrowHeight);
					_scrollNextButton.Location = new Point(_itemsAreaBounds.Right - verticalScrollBarWidth, _itemsAreaBounds.Bottom - verticalScrollBarArrowHeight);
					_scrollNextButton.Size = new Size(verticalScrollBarWidth, verticalScrollBarArrowHeight);
				}
				else
				{
					_scrollPreviousButton.Location = new Point(_itemsAreaBounds.Right - (verticalScrollBarWidth + 5), _itemsAreaBounds.Top + 6);
					_scrollPreviousButton.Size = new Size(verticalScrollBarWidth, verticalScrollBarArrowHeight);
					_scrollNextButton.Location = new Point(_itemsAreaBounds.Right - (verticalScrollBarWidth + 5), _itemsAreaBounds.Bottom - (verticalScrollBarArrowHeight + 5));
					_scrollNextButton.Size = new Size(verticalScrollBarWidth, verticalScrollBarArrowHeight);
				}
				UpdateScrollButtonBorders();
				if (_selectedGroup != null && _selectedGroup._container && _selectedGroup.ContainerControl != null)
				{
					_selectedGroup.ContainerControl.Bounds = _itemsAreaBounds;
					_selectedGroup.ContainerControl.Visible = true;
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			InvalidateLayout(false);
		}

		internal void InvalidateItemLayout()
		{
			_isItemLayoutValid = false;
			Invalidate();
		}

		internal void UpdateGroupContainerControl(NavigationGroup group)
		{
			if (!group.Container)
			{
				if (group.ContainerControl != null)
				{
					group.ContainerControl.Dispose();
					group.ContainerControl = null;
				}
			}
			else if (group.ContainerControl == null)
			{
				NavigationBarContainerControl navigationBarContainerControl = (group.ContainerControl = new NavigationBarContainerControl(this, group));
				navigationBarContainerControl.Bounds = Rectangle.Empty;
				if (base.Container != null)
				{
					base.Container.Add(navigationBarContainerControl);
				}
				base.Controls.Add(navigationBarContainerControl);
			}
			else if (group.ContainerControl._group == null)
			{
				group.ContainerControl._group = group;
				group.ContainerControl._owner = this;
			}
			InvalidateLayout(true);
		}

		internal void InvalidateLayout(bool resetScroll)
		{
			SetHoverItem(null);
			if (resetScroll)
			{
				_firstVisibleItem = null;
			}
			_isGroupLayoutValid = false;
			_isItemLayoutValid = false;
			Invalidate(base.ClientRectangle, invalidateChildren: true);
		}

		internal void RequestRepaint()
		{
			Invalidate();
		}

		protected override void OnResize(EventArgs e)
		{
			CompleteSlideAnimation();
			base.OnResize(e);
			InvalidateLayout(false);
			Update();
		}

		private void InvalidateItem(NavigationItem navigationBarItem)
		{
			if (_selectedGroup != null)
			{
				Invalidate(navigationBarItem.GetBounds());
				if (_selectedGroup.View == NavigationBarView.LargeIcons)
				{
					Invalidate(navigationBarItem.GetTextBounds());
				}
				Update();
			}
		}

		private NavigationItem GetFirstVisibleItem(NavigationGroup navigationBarGroup)
		{
			if (navigationBarGroup == null)
			{
				return null;
			}
			foreach (NavigationItem item in navigationBarGroup.Items)
			{
				if (item.Visible)
				{
					return item;
				}
			}
			return null;
		}

		private NavigationItem GetLastVisibleItem(NavigationGroup navigationBarGroup)
		{
			if (navigationBarGroup == null)
			{
				return null;
			}
			for (int index = navigationBarGroup.Items.Count - 1; index >= 0; index--)
			{
				NavigationItem navigationBarItem = navigationBarGroup.Items[index];
				if (navigationBarItem.Visible)
				{
					return navigationBarItem;
				}
			}
			return null;
		}

		private NavigationItem GetNextVisibleItem(int itemIndex)
		{
			NavigationGroup navigationBarGroup = _selectedGroup;
			if (navigationBarGroup == null)
			{
				return null;
			}
			for (int i = itemIndex + 1; i < navigationBarGroup.Items.Count; i++)
			{
				NavigationItem navigationBarItem = navigationBarGroup.Items[i];
				if (navigationBarItem.Visible)
				{
					return navigationBarItem;
				}
			}
			return null;
		}

		private NavigationItem GetPreviousVisibleItem(int itemIndex)
		{
			NavigationGroup navigationBarGroup = _selectedGroup;
			if (navigationBarGroup == null)
			{
				return null;
			}
			for (int index = itemIndex - 1; index >= 0; index--)
			{
				NavigationItem navigationBarItem = navigationBarGroup.Items[index];
				if (navigationBarItem.Visible)
				{
					return navigationBarItem;
				}
			}
			return null;
		}

		private void UpdateScrollButtonStates()
		{
			NavigationItem navigationBarItem = GetFirstVisibleItem(_selectedGroup);
			NavigationItem lastVisibleItem = GetLastVisibleItem(_selectedGroup);
			if (_orientation == NavigationBarOrientation.Vertical)
			{
				if (_itemsAreaBounds.Height <= 0)
				{
					_scrollPreviousButton.SetButtonEnabled(false);
					_scrollNextButton.SetButtonEnabled(false);
					return;
				}
				if (_firstVisibleItem == null || navigationBarItem == null || (_firstVisibleItem._index == navigationBarItem._index && _firstVisibleItem.GetBounds().Top >= _itemsAreaBounds.Top))
				{
					_scrollPreviousButton.SetButtonEnabled(false);
				}
				else
				{
					_scrollPreviousButton.SetButtonEnabled(true);
				}
				if (_selectedGroup != null && lastVisibleItem != null)
				{
					if (_selectedGroup.View == NavigationBarView.LargeIcons)
					{
						if (_firstVisibleItem == null)
						{
							_scrollNextButton.SetButtonEnabled(false);
						}
						else if (lastVisibleItem.GetTextBounds().Bottom <= _itemsAreaBounds.Bottom || _firstVisibleItem._index == lastVisibleItem._index)
						{
							_scrollNextButton.SetButtonEnabled(false);
						}
						else
						{
							_scrollNextButton.SetButtonEnabled(true);
						}
					}
					else if (lastVisibleItem.GetBounds().Bottom <= _itemsAreaBounds.Bottom)
					{
						_scrollNextButton.SetButtonEnabled(false);
					}
					else
					{
						_scrollNextButton.SetButtonEnabled(true);
					}
				}
				else
				{
					_scrollNextButton.SetButtonEnabled(false);
				}
				return;
			}
			if (_itemsAreaBounds.Width <= 0)
			{
				_scrollPreviousButton.SetButtonEnabled(false);
				_scrollNextButton.SetButtonEnabled(false);
				return;
			}
			if (_firstVisibleItem == null || navigationBarItem == null || (_firstVisibleItem._index == navigationBarItem._index && _firstVisibleItem.GetBounds().Left >= _itemsAreaBounds.Left))
			{
				_scrollPreviousButton.SetButtonEnabled(false);
			}
			else
			{
				_scrollPreviousButton.SetButtonEnabled(true);
			}
			if (_selectedGroup != null && _selectedGroup.Items.Count > 0)
			{
				NavigationItem lastItem = GetLastVisibleItem(_selectedGroup);
				if (lastItem != null)
				{
					if (lastItem.GetBounds().Right + 10 <= _itemsAreaBounds.Right || _firstVisibleItem == lastItem)
					{
						_scrollNextButton.SetButtonEnabled(false);
					}
					else if (_firstVisibleItem == null)
					{
						_scrollNextButton.SetButtonEnabled(false);
					}
					else if (lastItem.GetBounds().Left != _firstVisibleItem.GetBounds().Left)
					{
						_scrollNextButton.SetButtonEnabled(true);
					}
					else
					{
						_scrollNextButton.SetButtonEnabled(false);
					}
				}
				else
				{
					_scrollNextButton.SetButtonEnabled(false);
				}
			}
			else
			{
				_scrollNextButton.SetButtonEnabled(false);
			}
		}
	}
}
