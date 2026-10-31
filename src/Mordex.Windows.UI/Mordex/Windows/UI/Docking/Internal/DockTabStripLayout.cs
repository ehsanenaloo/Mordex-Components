using System.Collections;
using System.Drawing;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class DockTabStripLayout : IEnumerable
	{
		private const int DefaultEdgePadding = 4;

		private const int MinItemExtent = 4;

		private ArrayList _items;

		private ItemOrientation _itemOrientation;

		private int _edgePadding;

		private Rectangle _bounds;

		internal DockTabStripLayout()
		{
			_edgePadding = DefaultEdgePadding;
			_items = new ArrayList();
		}

		internal void AddItem(DockTabItem item)
		{
			_items.Add(item);
		}

		internal int GetCount()
		{
			return _items.Count;
		}

		internal void SetItemOrientation(ItemOrientation itemOrientation)
		{
			_itemOrientation = itemOrientation;
		}

		internal void ShrinkToFit(Size size, Size availableSize)
		{
			if (_items.Count == 0)
			{
				return;
			}
			if (_itemOrientation == ItemOrientation.Horizontal)
			{
				if (availableSize.Width >= size.Width)
				{
					return;
				}
				int totalWidth = 0;
				int targetWidth = availableSize.Width - _edgePadding * 2;
				if (targetWidth < _items.Count * MinItemExtent)
				{
					targetWidth = _items.Count * MinItemExtent;
				}
				int averageWidth = targetWidth / _items.Count;
				int[] widths = new int[_items.Count];
				for (int i = 0; i < _items.Count; i++)
				{
					DockTabItem item = (DockTabItem)_items[i];
					widths[i] = item.GetItemSize().Width;
					totalWidth += item.GetItemSize().Width;
				}
				int shrinkIndex = 0;
				while (totalWidth > targetWidth)
				{
					if (shrinkIndex >= _items.Count)
					{
						shrinkIndex = 0;
					}
					if (widths[shrinkIndex] >= averageWidth)
					{
						widths[shrinkIndex]--;
						totalWidth--;
					}
					shrinkIndex++;
				}
				int nextX = _edgePadding;
				for (int j = 0; j < widths.Length; j++)
				{
					DockTabItem tabItem = (DockTabItem)_items[j];
					tabItem.SetLocation(new Point(nextX, tabItem.GetLocation().Y));
					tabItem.SetItemSize(new Size(widths[j], tabItem.GetItemSize().Height));
					nextX += widths[j];
				}
			}
			else
			{
				if (availableSize.Height >= size.Height)
				{
					return;
				}
				int totalHeight = 0;
				int targetHeight = availableSize.Height;
				if (targetHeight < _items.Count * MinItemExtent)
				{
					targetHeight = _items.Count * MinItemExtent;
				}
				int averageHeight = targetHeight / _items.Count;
				int[] heights = new int[_items.Count];
				for (int k = 0; k < _items.Count; k++)
				{
					DockTabItem item = (DockTabItem)_items[k];
					heights[k] = item.GetItemSize().Height;
					totalHeight += heights[k];
				}
				int shrinkIndex = 0;
				while (totalHeight > targetHeight)
				{
					if (shrinkIndex >= _items.Count)
					{
						shrinkIndex = 0;
					}
					if (heights[shrinkIndex] >= averageHeight)
					{
						heights[shrinkIndex]--;
						totalHeight--;
					}
					shrinkIndex++;
				}
				int nextY = 0;
				for (int index = 0; index < heights.Length; index++)
				{
					DockTabItem item = (DockTabItem)_items[index];
					item.SetLocation(new Point(item.GetLocation().X, nextY));
					item.SetItemSize(new Size(item.GetItemSize().Width, heights[index]));
					nextY += heights[index];
				}
			}
		}

		private static ButtonState GetItemButtonState(DockTabItem item)
		{
			if (item.IsSelected)
			{
				return ButtonState.Selected;
			}
			if (item._isHot)
			{
				return ButtonState.Hot;
			}
			return ButtonState.Normal;
		}

		internal Size LayoutItems(Graphics graphics, Font font, int integer, int tabSpacing)
		{
			if (_itemOrientation == ItemOrientation.Horizontal)
			{
				int nextX = _edgePadding;
				int maxHeight = 0;
				foreach (DockTabItem item in _items)
				{
					item.SetLocation(new Point(nextX, 0));
					item.SetItemSize(item.MeasureContent(graphics, item.GetPanel().GetTabFormatStyle(GetItemButtonState(item)), font, integer, _itemOrientation));
					if (item.GetItemSize().Height > maxHeight)
					{
						maxHeight = item.GetItemSize().Height;
					}
					nextX += item.GetItemSize().Width + tabSpacing;
				}
				foreach (DockTabItem stretchedItem in _items)
				{
					if (stretchedItem.GetItemSize().Height != maxHeight)
					{
						stretchedItem.SetItemSize(new Size(stretchedItem.GetItemSize().Width, maxHeight));
					}
				}
				return new Size(nextX, maxHeight);
			}
			int nextY = _edgePadding;
			int maxWidth = 0;
			foreach (DockTabItem verticalItem in _items)
			{
				verticalItem.SetLocation(new Point(0, nextY));
				verticalItem.SetItemSize(verticalItem.MeasureContent(graphics, verticalItem.GetPanel().GetTabFormatStyle(GetItemButtonState(verticalItem)), font, integer, _itemOrientation));
				if (verticalItem.GetItemSize().Width > maxWidth)
				{
					maxWidth = verticalItem.GetItemSize().Width;
				}
				nextY += verticalItem.GetItemSize().Height + tabSpacing;
			}
			foreach (DockTabItem stretchedVerticalItem in _items)
			{
				if (stretchedVerticalItem.GetItemSize().Width != maxWidth)
				{
					stretchedVerticalItem.SetItemSize(new Size(maxWidth, stretchedVerticalItem.GetItemSize().Height));
				}
			}
			return new Size(maxWidth, nextY);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		internal DockTabItem GetItem(int index)
		{
			return (DockTabItem)_items[index];
		}

		internal int GetEdgePadding()
		{
			return _edgePadding;
		}

		internal void SetEdgePadding(int edgePadding)
		{
			_edgePadding = edgePadding;
		}

		public Rectangle GetBounds()
		{
			return _bounds;
		}

		public void SetBounds(Rectangle rectangle)
		{
			_bounds = rectangle;
		}
	}
}
