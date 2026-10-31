using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Tabs
{
	public partial class MxTabControl
	{
		internal void ResetTabSizes()
		{
			_tabThickness = 0;
			foreach (MxTabPage item in _tabPages)
			{
				item._isSizeDirty = true;
			}
			InvalidateLayout();
		}

		internal void InvalidateLayout()
		{
			_isLayoutDirty = true;
			Invalidate(invalidateChildren: true);
			_captionTabs?.Refresh();
		}

		private void LayoutTabsNow()
		{
			using (Graphics graphics = CreateGraphics())
			{
				LayoutTabs(graphics);
			}
		}

		public void EnsureVisible(MxTabPage tabPage)
		{
			if (tabPage == null)
			{
				return;
			}
			TabPageRenderer tabRenderer = _pageRenderers.FindTabPageItem(tabPage);
			if (tabRenderer == null)
			{
				return;
			}
			bool hasScrolled = false;
			if (_firstVisibleTabIndex == tabPage.Index)
			{
				return;
			}
			if (_isHorizontalStrip)
			{
				int visibleRight = _tabStripBounds.Right;
				if (_scrollBackButton != null && _scrollBackButton.IsVisible())
				{
					visibleRight = _scrollBackButton._bounds.Left;
				}
				if (tabRenderer._bounds.Right >= visibleRight)
				{
					if (_firstVisibleTabIndex < _pageRenderers.Count - 1)
					{
						_firstVisibleTabIndex++;
						hasScrolled = true;
					}
				}
				else if (tabRenderer._bounds.X < _tabStripBounds.Left && _firstVisibleTabIndex > 0)
				{
					_firstVisibleTabIndex--;
					hasScrolled = true;
				}
			}
			else
			{
				int visibleBottom = _tabStripBounds.Bottom;
				if (_scrollBackButton != null && _scrollBackButton.IsVisible())
				{
					visibleBottom = _scrollBackButton._bounds.Top;
				}
				if (tabRenderer._bounds.Bottom >= visibleBottom)
				{
					if (_firstVisibleTabIndex < _pageRenderers.Count - 1)
					{
						_firstVisibleTabIndex++;
						hasScrolled = true;
					}
				}
				else if (tabRenderer._bounds.Top < _tabStripBounds.Top && _firstVisibleTabIndex > 0)
				{
					_firstVisibleTabIndex--;
					hasScrolled = true;
				}
			}
			if (hasScrolled)
			{
				Form form = RenderingHelper.FindForm(this);
				if (form != null && !form.Disposing && form.Visible)
				{
					LayoutTabsNow();
				}
				EnsureVisible(tabPage);
				Invalidate();
			}
		}

		internal int GetTabThickness()
		{
			return _tabThickness;
		}

		internal void GrowTabThickness(int thickness)
		{
			if (_tabThickness < thickness)
			{
				_tabThickness = thickness;
			}
		}

		private int MeasureTabThickness(Graphics graphics)
		{
			if (!HasVisibleTabs())
			{
				return 0;
			}
			if (GetTabThickness() == 0)
			{
				foreach (MxTabPage tabPage in TabPages)
				{
					tabPage.MeasureTab(graphics);
				}
			}
			return GetTabThickness();
		}

		private void LayoutVerticalTabRows(Graphics graphics, int offsetX, int offsetY)
		{
			_tabRows = new ArrayList();
			int tabTop = base.ClientRectangle.Top + offsetY;
			int tabLeft = base.ClientRectangle.Left + offsetX;
			bool isLeftStrip = _tabStripAlignment == TabStripAlignment.Left;
			if (!isLeftStrip)
			{
				tabLeft = base.ClientRectangle.Right - _rowThickness - offsetX;
			}
			_tabStripBounds = new Rectangle(tabLeft, tabTop, _rowThickness, base.ClientRectangle.Height);
			ArrayList currentRow = new ArrayList();
			int rowLength = 0;
			int freeSpace = 0;
			int freeRemainder = 0;
			int selectedRow = 0;
			int flatInset = 0;
			int tabCount = 0;
			bool isRowStart = true;
			foreach (MxTabPage tabPage in TabPages)
			{
				if (tabPage._isSizeDirty)
				{
					tabPage.MeasureTab(graphics);
				}
				TabPageRenderer tabRenderer = new TabPageRenderer(tabPage.Text, tabPage, _tabStripAlignment);
				if (!tabPage.TabVisible)
				{
					((FormatStyleLayoutState)tabRenderer).SetVisible(false);
					_pageRenderers.AddItem(tabRenderer);
					tabPage.Visible = false;
					continue;
				}
				((FormatStyleLayoutState)tabRenderer).SetEnabled(tabPage.Enabled && base.Enabled);
				tabRenderer._itemImage = GetTabImage(tabPage);
				_pageRenderers.AddItem(tabRenderer);
				if (tabCount > 0 && rowLength + tabPage.GetTabSize().Height > base.Height - offsetY)
				{
					isRowStart = true;
					_tabStripBounds.Width += _rowThickness;
					if (isLeftStrip)
					{
						tabLeft += _rowThickness;
					}
					else
					{
						tabLeft -= _rowThickness;
						_tabStripBounds.X -= _rowThickness;
					}
					tabTop = base.ClientRectangle.Top + offsetY;
					if (_sizeMode == TabSizeMode.FillToRight)
					{
						int y = tabTop;
						freeSpace = base.Height - rowLength - offsetY;
						if (freeSpace > 0)
						{
							if (currentRow.Count > 0)
							{
								freeRemainder = freeSpace % currentRow.Count;
								freeSpace /= currentRow.Count;
							}
							else
							{
								freeRemainder = 0;
							}
							int positionInRow = 0;
							foreach (TabPageRenderer item in currentRow)
							{
								Rectangle bounds = item._bounds;
								bounds.Height += freeSpace;
								bounds.Y = y;
								if (positionInRow == currentRow.Count - 1)
								{
									if (item.IsPressed())
									{
										bounds.Height -= 2;
									}
									bounds.Height += freeRemainder;
								}
								item._textBounds = bounds;
								item._bounds = bounds;
								y = bounds.Bottom;
								positionInRow++;
							}
						}
					}
					_tabRows.Add(currentRow);
					currentRow = new ArrayList();
					rowLength = 0;
				}
				tabCount++;
				Rectangle tabBounds = ((!isLeftStrip) ? new Rectangle(tabLeft, tabTop, _rowThickness - flatInset, tabPage.GetTabSize().Height) : new Rectangle(tabLeft + flatInset, tabTop, _rowThickness - flatInset, tabPage.GetTabSize().Height));
				if (tabBounds.Height > base.Height - offsetY)
				{
					tabBounds.Height = base.Height - offsetY;
				}
				tabRenderer._bounds = tabBounds;
				tabRenderer._textBounds = tabBounds;
				tabRenderer._rowIndex = _tabRows.Count;
				currentRow.Add(tabRenderer);
				tabTop += tabPage.GetTabSize().Height;
				rowLength += tabPage.GetTabSize().Height;
				tabRenderer._isFirstInRow = isRowStart;
				if (_textOrientation == TextOrientation.Horizontal)
				{
					tabRenderer.SetSlanted(false);
				}
				tabRenderer._isLaidOut = true;
				isRowStart = false;
				tabRenderer._isLaidOut = true;
				if (tabPage == _selectedTab)
				{
					tabRenderer.SetPressed(true);
					selectedRow = _tabRows.Count;
				}
				else
				{
					tabPage.Visible = false;
				}
			}
			_tabRows.Add(currentRow);
			freeSpace = base.Height - rowLength - offsetY;
			if (_sizeMode == TabSizeMode.FillToRight && freeSpace > 0 && currentRow.Count > 0)
			{
				freeRemainder = freeSpace % currentRow.Count;
				freeSpace /= currentRow.Count;
				int y = base.ClientRectangle.Top + offsetY;
				int lastRowPosition = 0;
				foreach (TabPageRenderer lastRowTab in currentRow)
				{
					Rectangle lastRowBounds = lastRowTab._bounds;
					lastRowBounds.Height += freeSpace;
					lastRowBounds.Y = y;
					if (lastRowPosition == currentRow.Count - 1)
					{
						if (lastRowTab.IsPressed())
						{
							lastRowBounds.Height -= 2;
						}
						lastRowBounds.Height += freeRemainder;
					}
					lastRowTab._textBounds = lastRowBounds;
					lastRowTab._bounds = lastRowBounds;
					y = lastRowBounds.Bottom;
					lastRowPosition++;
				}
			}
			if (_tabRows.Count > 1)
			{
				MoveSelectedRowToStrip(_tabRows, selectedRow, false);
			}
			LayoutTabContents();
		}

		private void LayoutHorizontalTabRows(Graphics graphics, int offsetX, int offsetY)
		{
			int tabTop = base.ClientRectangle.Top + offsetY;
			int tabLeft = base.ClientRectangle.Left + offsetX;
			bool isTopStrip = _tabStripAlignment == TabStripAlignment.Top;
			if (!isTopStrip)
			{
				tabTop = base.ClientRectangle.Bottom - _rowThickness - offsetY;
			}
			_tabStripBounds = new Rectangle(tabLeft, tabTop, base.ClientRectangle.Width, _rowThickness);
			_tabRows = new ArrayList();
			ArrayList currentRow = new ArrayList();
			int rowLength = 0;
			int freeSpace = 0;
			int freeRemainder = 0;
			int selectedRow = 0;
			int flatInset = 0;
			int tabCount = 0;
			bool isRowStart = true;
			foreach (MxTabPage tabPage in TabPages)
			{
				if (tabPage._isSizeDirty)
				{
					tabPage.MeasureTab(graphics);
				}
				TabPageRenderer tabRenderer = new TabPageRenderer(tabPage.Text, tabPage, _tabStripAlignment);
				if (!tabPage.TabVisible)
				{
					((FormatStyleLayoutState)tabRenderer).SetVisible(false);
					_pageRenderers.AddItem(tabRenderer);
					tabPage.Visible = false;
					continue;
				}
				((FormatStyleLayoutState)tabRenderer).SetEnabled(base.Enabled && tabPage.Enabled);
				tabRenderer._itemImage = GetTabImage(tabPage);
				_pageRenderers.AddItem(tabRenderer);
				if (rowLength + tabPage.GetTabSize().Width > base.Width - offsetX && tabCount > 0)
				{
					isRowStart = true;
					_tabStripBounds.Height += _rowThickness;
					if (isTopStrip)
					{
						tabTop += _rowThickness;
					}
					else
					{
						tabTop -= _rowThickness;
						_tabStripBounds.Y -= _rowThickness;
					}
					tabLeft = base.ClientRectangle.Left + offsetX;
					if (_sizeMode == TabSizeMode.FillToRight)
					{
						int x = tabLeft;
						freeSpace = base.Width - rowLength - offsetX;
						if (freeSpace > 0)
						{
							if (currentRow.Count > 0)
							{
								freeRemainder = freeSpace % currentRow.Count;
								freeSpace /= currentRow.Count;
							}
							else
							{
								freeRemainder = 0;
							}
							int positionInRow = 0;
							foreach (TabPageRenderer item in currentRow)
							{
								Rectangle bounds = item._bounds;
								bounds.Width += freeSpace;
								bounds.X = x;
								if (positionInRow == currentRow.Count - 1)
								{
									if (item.IsPressed())
									{
										bounds.Width -= 2;
									}
									bounds.Width += freeRemainder;
								}
								item._textBounds = bounds;
								item._bounds = bounds;
								x = bounds.Right;
								positionInRow++;
							}
						}
					}
					_tabRows.Add(currentRow);
					currentRow = new ArrayList();
					rowLength = 0;
				}
				tabCount++;
				Rectangle tabBounds = ((!isTopStrip) ? new Rectangle(tabLeft, tabTop, tabPage.GetTabSize().Width, _rowThickness - flatInset) : new Rectangle(tabLeft, tabTop + flatInset, tabPage.GetTabSize().Width, _rowThickness - flatInset));
				if (tabBounds.Width > base.Width - offsetX)
				{
					tabBounds.Width = base.Width - offsetX;
				}
				tabRenderer._bounds = tabBounds;
				currentRow.Add(tabRenderer);
				tabRenderer._rowIndex = _tabRows.Count;
				tabLeft += tabPage.GetTabSize().Width;
				rowLength += tabPage.GetTabSize().Width;
				tabRenderer._isFirstInRow = isRowStart;
				if (_textOrientation == TextOrientation.Vertical)
				{
					tabRenderer.SetSlanted(false);
				}
				tabRenderer._isLaidOut = true;
				isRowStart = false;
				if (tabPage == _selectedTab)
				{
					tabRenderer.SetPressed(true);
					selectedRow = _tabRows.Count;
				}
				else
				{
					tabPage.Visible = false;
				}
			}
			_tabRows.Add(currentRow);
			freeSpace = base.Width - rowLength - offsetX;
			if (_sizeMode == TabSizeMode.FillToRight && freeSpace > 0 && currentRow.Count > 0)
			{
				freeRemainder = freeSpace % currentRow.Count;
				freeSpace /= currentRow.Count;
				int x = base.ClientRectangle.Left + offsetX;
				int lastRowPosition = 0;
				foreach (TabPageRenderer lastRowTab in currentRow)
				{
					Rectangle lastRowBounds = lastRowTab._bounds;
					lastRowBounds.Width += freeSpace;
					lastRowBounds.X = x;
					if (lastRowPosition == currentRow.Count - 1)
					{
						if (lastRowTab.IsPressed())
						{
							lastRowBounds.Width -= 2;
						}
						lastRowBounds.Width += freeRemainder;
					}
					lastRowTab._textBounds = lastRowBounds;
					lastRowTab._bounds = lastRowBounds;
					x = lastRowBounds.Right;
					lastRowPosition++;
				}
			}
			if (_tabRows.Count > 1)
			{
				MoveSelectedRowToStrip(_tabRows, selectedRow, true);
			}
			LayoutTabContents();
		}

		private void LayoutTabContents()
		{
			bool isNearStrip = TabStripAlignment == TabStripAlignment.Top || TabStripAlignment == TabStripAlignment.Left;
			foreach (TabPageRenderer item in _pageRenderers)
			{
				Size tabSize = item._bounds.Size;
				Rectangle bounds = item._bounds;
				if (item.IsPressed())
				{
					bounds.Inflate(2, 2);
					item._bounds = bounds;
				}
				if ((_tabDisplay != TabDisplay.Text && item._itemImage != null) || item.GetTabPage().Icon != null)
				{
					if (TabDisplay == TabDisplay.Image || item._caption.Length == 0)
					{
						item._imageBounds = bounds;
						item._textBounds = Rectangle.Empty;
					}
					else
					{
						Size iconSize = ((!_imageSize.IsEmpty) ? _imageSize : ((item.GetTabPage().Icon == null) ? item._itemImage.Size : item.GetTabPage().Icon.Size));
						MxTabPage page = item.GetTabPage();
						if (_isTextHorizontal)
						{
							if (iconSize.Width >= tabSize.Width)
							{
								item._imageBounds = Rectangle.Empty;
								item._textBounds = bounds;
							}
							else
							{
								int horizontalMargin = (bounds.Width - page.GetTabSize().Width) / 2;
								if (horizontalMargin <= MinImageMargin)
								{
									horizontalMargin = MinImageMargin;
								}
								item._imageBounds = new Rectangle(bounds.X + horizontalMargin, bounds.Y, iconSize.Width, bounds.Height);
								item._textBounds = new Rectangle(bounds.X + iconSize.Width + horizontalMargin, bounds.Y, bounds.Width - (iconSize.Width + horizontalMargin + MinImageMargin), bounds.Height);
							}
						}
						else if (iconSize.Height >= tabSize.Height)
						{
							item._imageBounds = Rectangle.Empty;
							item._textBounds = bounds;
						}
						else
						{
							int verticalMargin = (bounds.Height - page.GetTabSize().Height) / 2;
							if (verticalMargin <= MinImageMargin)
							{
								verticalMargin = MinImageMargin;
							}
							item._imageBounds = new Rectangle(bounds.X, bounds.Y + verticalMargin, bounds.Width, iconSize.Height);
							item._textBounds = new Rectangle(bounds.X, bounds.Y + iconSize.Height + verticalMargin, bounds.Width, bounds.Height - (iconSize.Height + verticalMargin + MinImageMargin));
						}
					}
				}
				else
				{
					item._textBounds = bounds;
					item._imageBounds = Rectangle.Empty;
				}
				ReserveTabCloseSpace(item);
				if (!item.IsPressed())
				{
					continue;
				}
				if (_isTextHorizontal)
				{
					if (!item._imageBounds.IsEmpty)
					{
						item._imageBounds.Offset(0, -1);
					}
					if (!item._textBounds.IsEmpty)
					{
						item._textBounds.Offset(0, -1);
					}
				}
				else
				{
					if (!item._imageBounds.IsEmpty)
					{
						item._imageBounds.Offset(-1, 0);
					}
					if (!item._textBounds.IsEmpty)
					{
						item._textBounds.Offset(-1, 0);
					}
				}
			}
		}

		private void MoveSelectedRowToStrip(ArrayList rows, int selectedRow, bool isHorizontal)
		{
			if (selectedRow == rows.Count)
			{
				return;
			}
			ArrayList selectedRowTabs = (ArrayList)rows[selectedRow];
			ArrayList lastRowTabs = (ArrayList)rows[rows.Count - 1];
			int selectedRowPosition;
			int lastRowPosition;
			if (isHorizontal)
			{
				selectedRowPosition = ((TabPageRenderer)selectedRowTabs[0])._bounds.Top;
				lastRowPosition = ((TabPageRenderer)lastRowTabs[0])._bounds.Top;
			}
			else
			{
				selectedRowPosition = ((TabPageRenderer)selectedRowTabs[0])._bounds.Left;
				lastRowPosition = ((TabPageRenderer)lastRowTabs[0])._bounds.Left;
			}
			foreach (TabPageRenderer selectedRowTab in selectedRowTabs)
			{
				selectedRowTab._rowIndex = _tabRows.Count - 1;
				if (isHorizontal)
				{
					selectedRowTab._bounds.Y = lastRowPosition;
					selectedRowTab._imageBounds.Y = lastRowPosition;
					selectedRowTab._textBounds.Y = lastRowPosition;
				}
				else
				{
					selectedRowTab._bounds.X = lastRowPosition;
					selectedRowTab._imageBounds.X = lastRowPosition;
					selectedRowTab._textBounds.X = lastRowPosition;
				}
			}
			foreach (TabPageRenderer lastRowTab in lastRowTabs)
			{
				lastRowTab._rowIndex = selectedRow;
				if (isHorizontal)
				{
					lastRowTab._bounds.Y = selectedRowPosition;
					lastRowTab._imageBounds.Y = selectedRowPosition;
					lastRowTab._textBounds.Y = selectedRowPosition;
				}
				else
				{
					lastRowTab._bounds.X = selectedRowPosition;
					lastRowTab._textBounds.X = selectedRowPosition;
					lastRowTab._imageBounds.X = selectedRowPosition;
				}
			}
			rows[selectedRow] = lastRowTabs;
			rows[rows.Count - 1] = selectedRowTabs;
		}

		private void UpdateDisplayRectangle()
		{
			int tabStripOffset = TabStripOffset;
			if (StripShown)
			{
				if (_tabStripBounds.IsEmpty)
				{
					LayoutTabs(null);
				}
				switch (_tabStripAlignment)
				{
				case TabStripAlignment.Top:
					_pageDisplayBounds = new Rectangle(base.ClientRectangle.Left, _tabStripBounds.Bottom, base.Width, base.Height - _tabStripBounds.Height - tabStripOffset);
					break;
				case TabStripAlignment.Bottom:
					_pageDisplayBounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, base.Width, base.Height - _tabStripBounds.Height - tabStripOffset);
					break;
				case TabStripAlignment.Left:
					_pageDisplayBounds = new Rectangle(_tabStripBounds.Right, base.ClientRectangle.Top, base.Width - _tabStripBounds.Width - tabStripOffset, base.Height);
					break;
				case TabStripAlignment.Right:
					_pageDisplayBounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, base.Width - _tabStripBounds.Width - tabStripOffset, base.Height);
					break;
				}
			}
			else
			{
				_pageDisplayBounds = base.ClientRectangle;
			}
			if (!VisualStyleSupport.IsThemingUsable(false))
			{
				int borderWidth = 0;
				int leftBorder = 0;
				int rightBorder = 0;
				int bottomBorder = 0;
				int topBorder = 0;
				borderWidth = (1);
				if (_pageBorder == PageBorder.All)
				{
					leftBorder = borderWidth;
					topBorder = borderWidth;
					bottomBorder = borderWidth;
					rightBorder = borderWidth;
				}
				else if (_pageBorder == PageBorder.StripLine)
				{
					switch (_tabStripAlignment)
					{
					case TabStripAlignment.Top:
						topBorder = borderWidth;
						break;
					case TabStripAlignment.Left:
						leftBorder = borderWidth;
						break;
					case TabStripAlignment.Right:
						rightBorder = borderWidth;
						break;
					case TabStripAlignment.Bottom:
						bottomBorder = borderWidth;
						break;
					}
				}
				_pageDisplayBounds = new Rectangle(_pageDisplayBounds.Left + leftBorder, _pageDisplayBounds.Top + topBorder, _pageDisplayBounds.Width - leftBorder - rightBorder, _pageDisplayBounds.Height - topBorder - bottomBorder);
				if (_selectedTab != null && _pageDisplayBounds != _selectedTab.Bounds)
				{
					_selectedTab.Bounds = _pageDisplayBounds;
					if (_selectedTab.Bounds == _pageDisplayBounds)
					{
						UpdateDisplayRectangle();
					}
					else
					{
						Invalidate();
					}
				}
				else
				{
					_pageDisplayBounds = new Rectangle(_pageDisplayBounds.Left - leftBorder, _pageDisplayBounds.Top - topBorder, _pageDisplayBounds.Width + leftBorder + rightBorder, _pageDisplayBounds.Height + topBorder + bottomBorder);
				}
			}
			else
			{
				Rectangle bounds = _pageDisplayBounds;
				if (_tabStripAlignment == TabStripAlignment.Left || _tabStripAlignment == TabStripAlignment.Top)
				{
					bounds.Offset(1, 1);
				}
				else if (_tabStripAlignment == TabStripAlignment.Right)
				{
					bounds.Offset(ThemedPageFarOffset, 1);
				}
				else
				{
					bounds.Offset(1, ThemedPageFarOffset);
				}
				bounds.Width -= ThemedPageShrink;
				bounds.Height -= ThemedPageShrink;
				if (_selectedTab != null)
				{
					_selectedTab.Bounds = bounds;
				}
			}
		}

		internal void SetTextHorizontal(bool isTextHorizontal, Graphics graphics)
		{
			if (_isTextHorizontal == isTextHorizontal)
			{
				return;
			}
			_isTextHorizontal = isTextHorizontal;
			foreach (MxTabPage tabPage in TabPages)
			{
				tabPage.MeasureTab(graphics);
			}
		}

		internal void SelectNearestVisibleTab(int pageIndex)
		{
			for (int i = pageIndex + 1; i < _tabPages.Count; i++)
			{
				MxTabPage nextPage = _tabPages[i];
				if (nextPage.TabVisible)
				{
					SelectedTab = nextPage;
					return;
				}
			}
			for (int previousIndex = pageIndex - 1; previousIndex >= 0; previousIndex--)
			{
				MxTabPage previousPage = _tabPages[previousIndex];
				if (previousPage.TabVisible)
				{
					SelectedTab = previousPage;
					break;
				}
			}
		}

		internal bool HasVisibleTabs()
		{
			foreach (MxTabPage tabPage in TabPages)
			{
				if (tabPage.TabVisible)
				{
					return true;
				}
			}
			return false;
		}

		internal void LayoutTabs(Graphics graphics)
		{
			SyncTabCloseLayout();
			if (_tabStripAlignment == TabStripAlignment.Top || _tabStripAlignment == TabStripAlignment.Bottom)
			{
				_isHorizontalStrip = true;
				if (_textOrientation == TextOrientation.Vertical)
				{
					SetTextHorizontal(false, graphics);
				}
				else
				{
					SetTextHorizontal(true, graphics);
				}
			}
			else
			{
				_isHorizontalStrip = false;
				if (_textOrientation == TextOrientation.Horizontal)
				{
					SetTextHorizontal(true, graphics);
				}
				else
				{
					SetTextHorizontal(false, graphics);
				}
			}
			_rowThickness = MeasureTabThickness(graphics);
			_pageRenderers.ClearItems();
			_tabRows = null;
			if (!HasVisibleTabs())
			{
				_tabStripBounds = Rectangle.Empty;
				_isLayoutDirty = false;
				return;
			}
			if (_selectedTab == null)
			{
				SelectedTab = TabPages[0];
			}
			int offsetY;
			int offsetX;
			if (_isHorizontalStrip)
			{
				offsetY = _tabStripOffset;
				offsetX = _firstTabOffset;
			}
			else
			{
				offsetX = _tabStripOffset;
				offsetY = _firstTabOffset;
			}
			if (!StripShown)
			{
				_tabStripBounds = Rectangle.Empty;
				UpdateDisplayRectangle();
				foreach (MxTabPage item in _tabPages)
				{
					if (!item.Selected || !item.TabVisible)
					{
						item.Visible = false;
					}
				}
				_isLayoutDirty = false;
				return;
			}
			if (_multiLine)
			{
				if (_scrollForwardButton != null)
				{
					_scrollForwardButton = null;
				}
				if (_scrollBackButton != null)
				{
					_scrollBackButton = null;
				}
				if (_closeButton != null)
				{
					_closeButton = null;
				}
				_firstVisibleTabIndex = -1;
				if (_isHorizontalStrip)
				{
					LayoutHorizontalTabRows(graphics, offsetX, offsetY);
				}
				else
				{
					LayoutVerticalTabRows(graphics, offsetX, offsetY);
				}
				_isLayoutDirty = false;
				UpdateDisplayRectangle();
				return;
			}
			bool shouldEnsureVisible = false;
			if (_firstVisibleTabIndex == -1)
			{
				_firstVisibleTabIndex = 0;
				shouldEnsureVisible = true;
			}
			else if (_firstVisibleTabIndex > TabPages.Count)
			{
				_firstVisibleTabIndex = TabPages.Count - 1;
			}
			int tabTop = base.ClientRectangle.Top + offsetY;
			int tabLeft = base.ClientRectangle.Left + offsetX;
			bool isOverflowing = false;
			bool hasFirstTab = false;
			if (_isHorizontalStrip)
			{
				if (_tabStripAlignment == TabStripAlignment.Bottom)
				{
					tabTop = base.ClientRectangle.Bottom - _rowThickness - offsetY;
				}
				_tabStripBounds = new Rectangle(tabLeft, tabTop, base.ClientRectangle.Width - tabLeft, _rowThickness);
				for (int i = _firstVisibleTabIndex; i < TabPages.Count; i++)
				{
					MxTabPage nextPage = _tabPages[i];
					if (nextPage._isSizeDirty)
					{
						nextPage.MeasureTab(graphics);
					}
					TabPageRenderer nextTab = new TabPageRenderer(nextPage.Text, nextPage, _tabStripAlignment);
					if (!nextPage.TabVisible)
					{
						((FormatStyleLayoutState)nextTab).SetVisible(false);
						nextPage.Visible = false;
						nextTab._index = i;
						_pageRenderers.AddItem(nextTab);
						continue;
					}
					if (nextPage == _selectedTab)
					{
						nextTab.SetPressed(true);
						nextTab.SetSelectedExtended(true);
					}
					else
					{
						nextPage.Visible = false;
					}
					if (!hasFirstTab)
					{
						hasFirstTab = true;
						nextTab.SetSelectedExtended(true);
					}
					((FormatStyleLayoutState)nextTab).SetEnabled(nextPage.Enabled && base.Enabled);
					nextTab._itemImage = GetTabImage(nextPage);
					Rectangle nextBounds = Rectangle.Empty;
					nextBounds = (nextTab._bounds = new Rectangle(tabLeft, tabTop, nextPage.GetTabSize().Width, _rowThickness));
					nextTab._textBounds = nextBounds;
					tabLeft += nextPage.GetTabSize().Width;
					_pageRenderers.AddItem(nextTab);
					nextTab._index = i;
				}
				if (tabLeft >= base.ClientRectangle.Right)
				{
					isOverflowing = true;
				}
				tabLeft = base.ClientRectangle.Left + offsetX;
				for (int previousIndex = _firstVisibleTabIndex - 1; previousIndex >= 0; previousIndex--)
				{
					MxTabPage previousPage = _tabPages[previousIndex];
					if (previousPage._isSizeDirty)
					{
						previousPage.MeasureTab(graphics);
					}
					TabPageRenderer previousTab = new TabPageRenderer(previousPage.Text, previousPage, _tabStripAlignment);
					if (!previousPage.TabVisible)
					{
						((FormatStyleLayoutState)previousTab).SetVisible(false);
						previousTab._index = previousIndex;
						previousPage.Visible = false;
						_pageRenderers.AddItem(previousTab);
					}
					else
					{
						((FormatStyleLayoutState)previousTab).SetEnabled(base.Enabled && previousPage.Enabled);
						if (previousPage == _selectedTab)
						{
							previousTab.SetPressed(true);
							previousTab.SetSelectedExtended(true);
						}
						else
						{
							previousPage.Visible = false;
						}
						previousTab._itemImage = GetTabImage(previousPage);
						tabLeft -= previousPage.GetTabSize().Width;
						Rectangle previousBounds = (previousTab._bounds = new Rectangle(tabLeft, tabTop, previousPage.GetTabSize().Width, _rowThickness));
						previousTab._textBounds = previousBounds;
						((FormatStyleLayoutState)previousTab).SetVisible(false);
						_pageRenderers.AddItem(previousTab);
						previousTab._index = previousIndex;
					}
				}
				if (tabLeft < base.ClientRectangle.Left + FirstTabOffset)
				{
					isOverflowing = true;
				}
			}
			else
			{
				if (_tabStripAlignment == TabStripAlignment.Right)
				{
					tabLeft = base.ClientRectangle.Right - _rowThickness - offsetX;
				}
				_tabStripBounds = new Rectangle(tabLeft, tabTop, _rowThickness, base.ClientRectangle.Height - tabTop);
				for (int j = _firstVisibleTabIndex; j < _tabPages.Count; j++)
				{
					MxTabPage nextVerticalPage = _tabPages[j];
					if (nextVerticalPage._isSizeDirty)
					{
						nextVerticalPage.MeasureTab(graphics);
					}
					TabPageRenderer nextVerticalTab = new TabPageRenderer(nextVerticalPage.Text, nextVerticalPage, _tabStripAlignment);
					if (!nextVerticalPage.TabVisible)
					{
						((FormatStyleLayoutState)nextVerticalTab).SetVisible(false);
						nextVerticalTab._index = j;
						_pageRenderers.AddItem(nextVerticalTab);
						nextVerticalPage.Visible = false;
						continue;
					}
					((FormatStyleLayoutState)nextVerticalTab).SetEnabled(base.Enabled && nextVerticalPage.Enabled);
					if (nextVerticalPage == _selectedTab)
					{
						nextVerticalTab.SetPressed(true);
						nextVerticalTab.SetSelectedExtended(true);
					}
					else
					{
						nextVerticalPage.Visible = false;
					}
					if (!hasFirstTab)
					{
						hasFirstTab = true;
						nextVerticalTab.SetSelectedExtended(true);
					}
					nextVerticalTab._itemImage = GetTabImage(nextVerticalPage);
					Rectangle nextVerticalBounds = (nextVerticalTab._bounds = new Rectangle(tabLeft, tabTop, _rowThickness, nextVerticalPage.GetTabSize().Height));
					nextVerticalTab._textBounds = nextVerticalBounds;
					tabTop += nextVerticalPage.GetTabSize().Height;
					_pageRenderers.AddItem(nextVerticalTab);
					nextVerticalTab._index = j;
				}
				if (tabTop >= base.ClientRectangle.Bottom)
				{
					isOverflowing = true;
				}
				tabTop = base.ClientRectangle.Top + offsetY;
				for (int previousVerticalIndex = _firstVisibleTabIndex - 1; previousVerticalIndex >= 0; previousVerticalIndex--)
				{
					MxTabPage previousVerticalPage = _tabPages[previousVerticalIndex];
					if (previousVerticalPage._isSizeDirty)
					{
						previousVerticalPage.MeasureTab(graphics);
					}
					TabPageRenderer previousVerticalTab = new TabPageRenderer(previousVerticalPage.Text, previousVerticalPage, _tabStripAlignment);
					if (!previousVerticalPage.TabVisible)
					{
						((FormatStyleLayoutState)previousVerticalTab).SetVisible(false);
						previousVerticalTab._index = previousVerticalIndex;
						_pageRenderers.AddItem(previousVerticalTab);
						previousVerticalPage.Visible = false;
					}
					else
					{
						((FormatStyleLayoutState)previousVerticalTab).SetEnabled(base.Enabled && previousVerticalPage.Enabled);
						if (previousVerticalPage == _selectedTab)
						{
							previousVerticalTab.SetPressed(true);
							previousVerticalTab.SetSelectedExtended(true);
						}
						else
						{
							previousVerticalPage.Visible = false;
						}
						previousVerticalTab._itemImage = GetTabImage(previousVerticalPage);
						tabTop -= previousVerticalPage.GetTabSize().Height;
						Rectangle previousVerticalBounds = (previousVerticalTab._bounds = new Rectangle(tabLeft, tabTop, _rowThickness, previousVerticalPage.GetTabSize().Height));
						((FormatStyleLayoutState)previousVerticalTab).SetVisible(false);
						previousVerticalTab._textBounds = previousVerticalBounds;
						_pageRenderers.AddItem(previousVerticalTab);
						previousVerticalTab._index = previousVerticalIndex;
					}
				}
				if (tabTop < base.ClientRectangle.Top + FirstTabOffset)
				{
					isOverflowing = true;
				}
			}
			LayoutTabContents();
			UpdateDisplayRectangle();
			LayoutCloseButton();
			if (isOverflowing)
			{
				LayoutScrollButtons();
			}
			else
			{
				if (_scrollForwardButton != null)
				{
					_scrollForwardButton = null;
				}
				if (_scrollBackButton != null)
				{
					_scrollBackButton = null;
				}
				SetScrollBackHot(false);
				SetScrollForwardHot(false);
				SetScrollBackPressed(false);
				SetScrollForwardPressed(false);
			}
			if (shouldEnsureVisible)
			{
				EnsureVisible(_selectedTab);
			}
			_isLayoutDirty = false;
		}

		private void LayoutCloseButton()
		{
			if (_showCloseButton && _selectedTab != null && !_tabCloseLayout)
			{
				if (_closeButton == null)
				{
					_closeButton = new FormatStyleLayoutState(this);
				}
				int inset = 0;
				int width = _scrollButtonSize.Width;
				int height = _scrollButtonSize.Height;
				switch (_tabStripAlignment)
				{
				case TabStripAlignment.Top:
				case TabStripAlignment.Bottom:
				{
					int x = base.ClientRectangle.Right - width;
					_closeButton._bounds = new Rectangle(x, _tabStripBounds.Top + inset, width, height);
					break;
				}
				case TabStripAlignment.Left:
				case TabStripAlignment.Right:
				{
					int y = base.ClientRectangle.Bottom - height;
					_closeButton._bounds = new Rectangle(_tabStripBounds.X + inset, y, width, height);
					break;
				}
				}
				_closeButton.SetEnabled(_selectedTab.AllowClose);
			}
			else
			{
				_closeButton = null;
				SetCloseButtonHot(false);
				SetCloseButtonPressed(false);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeScrollButtonSize()
		{
			return _scrollButtonSize != DefaultScrollButtonSize;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetScrollButtonSize()
		{
			ScrollButtonSize = DefaultScrollButtonSize;
		}

		private void LayoutScrollButtons()
		{
			if (_scrollBackButton == null)
			{
				_scrollBackButton = new FormatStyleLayoutState(this);
			}
			if (_scrollForwardButton == null)
			{
				_scrollForwardButton = new FormatStyleLayoutState(this);
			}
			int inset = 0;
			int width = _scrollButtonSize.Width;
			int height = _scrollButtonSize.Height;
			switch (_tabStripAlignment)
			{
			case TabStripAlignment.Top:
			case TabStripAlignment.Bottom:
			{
				int forwardButtonLeft = ((_closeButton == null) ? (base.ClientRectangle.Right - width) : (_closeButton._bounds.Left - width));
				_scrollBackButton._bounds = new Rectangle(forwardButtonLeft - width, _tabStripBounds.Top + inset, width, height);
				_scrollForwardButton._bounds = new Rectangle(forwardButtonLeft, _tabStripBounds.Top + inset, width, height);
				break;
			}
			case TabStripAlignment.Left:
			case TabStripAlignment.Right:
			{
				int forwardButtonTop = ((_closeButton == null) ? (base.ClientRectangle.Bottom - height) : (_closeButton._bounds.Top - height));
				_scrollBackButton._bounds = new Rectangle(_tabStripBounds.X + inset, forwardButtonTop - height, width, height);
				_scrollForwardButton._bounds = new Rectangle(_tabStripBounds.X + inset, forwardButtonTop, width, height);
				break;
			}
			}
			if (_firstVisibleTabIndex == 0)
			{
				_scrollBackButton.SetEnabled(false);
			}
			else
			{
				_scrollBackButton.SetEnabled(true);
			}
			if (_firstVisibleTabIndex == _pageRenderers.Count - 1)
			{
				_scrollForwardButton.SetEnabled(false);
				return;
			}
			for (int index = _pageRenderers.Count - 1; index >= 0; index--)
			{
				TabPageRenderer tabRenderer = (TabPageRenderer)_pageRenderers.FindItemByIndex(index);
				if (tabRenderer.GetTabPage().TabVisible && tabRenderer._bounds.X >= 0)
				{
					if (_isHorizontalStrip)
					{
						if (tabRenderer._bounds.Right <= _scrollBackButton._bounds.Left)
						{
							_scrollForwardButton.SetEnabled(false);
						}
						else
						{
							_scrollForwardButton.SetEnabled(true);
						}
					}
					else if (tabRenderer._bounds.Bottom <= _scrollBackButton._bounds.Top)
					{
						_scrollForwardButton.SetEnabled(false);
					}
					else
					{
						_scrollForwardButton.SetEnabled(true);
					}
					return;
				}
			}
			_scrollForwardButton.SetEnabled(true);
		}
	}
}
