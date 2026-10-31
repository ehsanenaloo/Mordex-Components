using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabContentRenderer
	{
		public void RebuildGroupRenderers()
		{
			_areGroupsStale = false;
			if (_groupRenderers != null)
			{
				StopItemAnimations();
				_renderHost.ParentControl.Controls.Clear();
			}
			_groupRenderers = new List<RibbonGroupBoxRenderer>();
			if (_ribbon.SelectedTab == null)
			{
				return;
			}
			foreach (RibbonGroup group in _ribbon.SelectedTab.Groups)
			{
				if (group.Visible)
				{
					RibbonGroupBoxRenderer item = new RibbonGroupBoxRenderer(group, _ribbon, _renderHost);
					_groupRenderers.Add(item);
				}
			}
			if (_ribbon.ShouldShowAddNewItems() && _ribbon.SelectedTab.IsEditable())
			{
				AddNewGroupRenderer addGroupRenderer = new AddNewGroupRenderer(_ribbon, _renderHost);
				_groupRenderers.Add(addGroupRenderer);
			}
		}

		public bool TryShrinkGalleryColumn(Graphics graphics, TextLayoutRenderer textLayoutRenderer)
		{
			for (int groupIndex = _groupRenderers.Count - 1; groupIndex >= 0; groupIndex--)
			{
				RibbonGroupBoxRenderer thisValue = _groupRenderers[groupIndex];
				if (thisValue._galleryColumns != null && thisValue._galleryColumns.Count > 0)
				{
					for (int columnIndex = thisValue._galleryColumns.Count - 1; columnIndex >= 0; columnIndex--)
					{
						RibbonCommandColumn column = thisValue._galleryColumns[columnIndex];
						if (column.CanShrink(false))
						{
							_totalGroupsWidth -= thisValue._fullItemWidth;
							column.Shrink();
							thisValue.CalculateCollapsedLayout(graphics, textLayoutRenderer, _ribbon.Font);
							_totalGroupsWidth += thisValue._fullItemWidth;
							return true;
						}
					}
				}
			}
			return false;
		}

		private void LayoutGroupsAtOffset(Graphics graphics, TextLayoutRenderer textRenderer, int scrollOffset)
		{
			int groupSpacing = 0;
			if (_totalGroupsWidth > _groupsBounds.Width)
			{
				if (TryShrinkGalleryColumn(graphics, textRenderer))
				{
					LayoutGroupsAtOffset(graphics, textRenderer, scrollOffset);
					return;
				}
				if (TryShrinkGroupCaptions(graphics, _totalGroupsWidth, _groupsBounds.Width))
				{
					_totalGroupsWidth = 0;
					foreach (RibbonGroupBoxRenderer item in _groupRenderers)
					{
						_totalGroupsWidth += item._fullItemWidth;
					}
					_totalGroupsWidth += (_groupRenderers.Count - 1) * groupSpacing;
				}
			}
			if (_totalGroupsWidth > _groupsBounds.Width)
			{
				if (!TryCollapseGroups(graphics, textRenderer, _groupsBounds.Width, false))
				{
					RefreshScrollButtons();
				}
				else
				{
					_totalGroupsWidth = 0;
					foreach (RibbonGroupBoxRenderer widthGroup in _groupRenderers)
					{
						_totalGroupsWidth += widthGroup._fullItemWidth;
					}
					_leftScrollButton = null;
					_rightScrollButton = null;
				}
			}
			else
			{
				_leftScrollButton = null;
				_rightScrollButton = null;
			}
			int groupLeft = _groupsBounds.Left + scrollOffset;
			int top = _groupsBounds.Top;
			foreach (RibbonGroupBoxRenderer layoutGroup in _groupRenderers)
			{
				layoutGroup.ArrangeItem(graphics, textRenderer, groupLeft, top, layoutGroup._fullItemWidth, _groupsBounds.Height);
				groupLeft += layoutGroup._bounds.Width + groupSpacing;
			}
			_totalGroupsWidth = groupLeft - scrollOffset;
			if (scrollOffset < 0 && scrollOffset + _totalGroupsWidth < _bounds.Right - groupSpacing)
			{
				int newOffset = _bounds.Right - groupSpacing - _totalGroupsWidth;
				if (newOffset > 0)
				{
					newOffset = 0;
				}
				LayoutGroupsAtOffset(graphics, textRenderer, newOffset);
			}
		}

		private void RefreshScrollButtons()
		{
			if (!_isScrollTimerRunning)
			{
				if (GetScrollOffset() < 0)
				{
					_leftScrollButton = new RibbonLayoutItem(_ribbon);
					_leftScrollButton._bounds = new Rectangle(_bounds.X - 1, _bounds.Y, 13, _bounds.Height - 2);
				}
				else
				{
					_leftScrollButton = null;
				}
				if (_totalGroupsWidth + GetScrollOffset() > _bounds.Right - 2)
				{
					_rightScrollButton = new RibbonLayoutItem(_ribbon);
					_rightScrollButton._bounds = new Rectangle(_bounds.Right - 12, _bounds.Y, 13, _bounds.Height - 2);
				}
				else
				{
					_rightScrollButton = null;
				}
			}
		}

		public bool TryShrinkGroupCaptions(Graphics graphics, int totalWidth, int availableWidth)
		{
			bool result = false;
			for (int groupIndex = _groupRenderers.Count - 1; groupIndex >= 0; groupIndex--)
			{
				RibbonGroupBoxRenderer thisValue = _groupRenderers[groupIndex];
				if (thisValue._captionWidth > thisValue._contentWidth && thisValue._captionWidth > thisValue._minCaptionWidth)
				{
					result = true;
					int reduction = thisValue._captionWidth - thisValue._contentWidth;
					if (reduction > totalWidth - availableWidth)
					{
						reduction = totalWidth - availableWidth;
					}
					thisValue._captionWidth -= reduction;
					if (thisValue._captionWidth < thisValue._minCaptionWidth)
					{
						int overshoot = thisValue._minCaptionWidth - thisValue._captionWidth;
						thisValue._captionWidth = thisValue._minCaptionWidth;
						reduction -= overshoot;
					}
					thisValue.SetLayoutWidth(thisValue._captionWidth);
					thisValue._fullItemWidth = thisValue._captionWidth;
					totalWidth -= reduction;
					if (totalWidth == availableWidth)
					{
						break;
					}
				}
			}
			return result;
		}

		private bool TryCollapseGroups(Graphics graphics, TextLayoutRenderer textLayoutRenderer, int availableWidth, bool forceCollapse)
		{
			bool collapsedAny = false;
			if (_ribbon.IsInDesignMode())
			{
				return false;
			}
			for (int groupIndex = _groupRenderers.Count - 1; groupIndex >= 0; groupIndex--)
			{
				RibbonGroupBoxRenderer thisValue = _groupRenderers[groupIndex];
				if (thisValue.CanReduce(true) || (forceCollapse && !thisValue._isCollapsed))
				{
					thisValue.ShrinkOrCollapse(graphics, textLayoutRenderer, forceCollapse, _ribbon.Font);
					collapsedAny = true;
					break;
				}
			}
			if (collapsedAny)
			{
				int totalWidth = 0;
				foreach (RibbonGroupBoxRenderer item in _groupRenderers)
				{
					totalWidth += item._fullItemWidth;
				}
				totalWidth += (_groupRenderers.Count - 1) * 2;
				if (totalWidth > availableWidth)
				{
					return TryCollapseGroups(graphics, textLayoutRenderer, availableWidth, forceCollapse);
				}
				return true;
			}
			if (!forceCollapse)
			{
				return TryCollapseGroups(graphics, textLayoutRenderer, availableWidth, true);
			}
			return false;
		}
	}
}
