using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		internal Point GetMergedLocation()
		{
			return _mergedLocation;
		}

		internal void SetMergedLocation(Point mergedLocation)
		{
			_mergedLocation = mergedLocation;
		}

		internal BarsDockStyle GetDockedDockStyle()
		{
			return _dockedDockStyle;
		}

		internal Point GetDockedLocation()
		{
			return _dockedLocation;
		}

		internal void RemoveDockedPlaceholder()
		{
			switch (GetDockedDockStyle())
			{
			case BarsDockStyle.Top:
				CommandManager.TopRebar.RemoveBarState(this);
				break;
			case BarsDockStyle.Left:
				CommandManager.LeftRebar.RemoveBarState(this);
				break;
			case BarsDockStyle.Right:
				CommandManager.RightRebar.RemoveBarState(this);
				break;
			case BarsDockStyle.Bottom:
				CommandManager.BottomRebar.RemoveBarState(this);
				break;
			}
		}

		internal void SaveDockedPosition()
		{
			if (!_isLoadingLayout)
			{
				if (_dockedRowIndex != -1)
				{
					RemoveDockedPlaceholder();
				}
				_dockedDockStyle = DockStyle;
				_dockedLocation = Location;
				_dockedRowIndex = GetHost().GetOrAssignBarRowIndex(this, GetHostRow().GetRowIndex());
			}
		}

		internal void RestoreDockedPosition()
		{
			_isRestoringDockedPosition = true;
			DockStyle = _dockedDockStyle;
			if (_host != null)
			{
				GetHost().InsertRowByDockedIndex(GetHostRow(), _dockedRowIndex, this);
				Location = GetDockedLocation();
				_isRestoringDockedPosition = false;
				RemoveDockedPlaceholder();
				_dockedRowIndex = -1;
				_dockedDockStyle = BarsDockStyle.Top;
				_dockedLocation = Point.Empty;
			}
		}

		internal void UpdateRowIndex(int rowIndex)
		{
			if (GetHost() != null)
			{
				if (rowIndex > GetHost().GetRows().Count)
				{
					rowIndex = GetHost().GetRows().Count;
				}
				if (rowIndex < 0)
				{
					rowIndex = 0;
				}
				_rowIndex = rowIndex;
				if (GetHost().GetRows().Count > rowIndex)
				{
					CommandBarHostLayoutState uiRebarLayoutState = (CommandBarHostLayoutState)GetHost().GetRows()[rowIndex];
					if (!uiRebarLayoutState.AllowsSharedRow(this))
					{
						GetHost().SwapRows(GetHostRow(), rowIndex);
						return;
					}
				}
				if (GetHostRow() != null)
				{
					GetHostRow().GetBars().Remove(this);
				}
				if (rowIndex >= GetHost().GetRows().Count)
				{
					GetHost().AddRowForBar(this);
				}
				else
				{
					CommandBarHostLayoutState targetRow = (CommandBarHostLayoutState)_host.GetRows()[rowIndex];
					targetRow.GetBars().Add(this);
				}
			}
			if (GetHostRow() != null && GetHostRow().GetBars().Count > 1)
			{
				if (IsHorizontalLayout())
				{
					MoveToLeft(GetPosition().X);
				}
				else
				{
					MoveToLeft(GetPosition().Y);
				}
			}
		}

		internal void MoveToPosition(Point position)
		{
			if (position == _position || (CommandManager != null && CommandManager._isCustomizing) || (_host != null && _host.IsResizing()))
			{
				return;
			}
			if (GetHostRow() != null)
			{
				if (GetHostRow().HasRowOwner())
				{
					return;
				}
				if (IsHorizontalLayout())
				{
					if (position.X < _position.X)
					{
						foreach (MxCommandBar item in GetHostRow().GetBars())
						{
							if (item != this && item._rowIndex == _rowIndex && item.GetPosition().X == position.X)
							{
								item._position = new Point(item.GetPosition().X + 1, item.GetPosition().Y);
							}
						}
					}
					else
					{
						foreach (MxCommandBar bar in GetHostRow().GetBars())
						{
							if (bar != this && bar._rowIndex == _rowIndex && bar.GetPosition().X == position.X)
							{
								if (bar.GetPosition().X == 0)
								{
									Location = new Point(bar.Right, position.Y);
									return;
								}
								bar._position = new Point(bar.GetPosition().X - 1, bar.GetPosition().Y);
							}
						}
					}
				}
				else if (position.Y < _position.Y)
				{
					foreach (MxCommandBar bar in GetHostRow().GetBars())
					{
						if (bar != this && bar._rowIndex == _rowIndex && bar.GetPosition().Y == position.Y)
						{
							bar._position = new Point(bar.GetPosition().X, bar.GetPosition().Y + 1);
						}
					}
				}
				else
				{
					foreach (MxCommandBar bar in GetHostRow().GetBars())
					{
						if (bar != this && bar._rowIndex == _rowIndex && bar.GetPosition().Y == position.Y)
						{
							if (bar.GetPosition().Y == 0)
							{
								Location = new Point(position.X, position.Y + 1);
								return;
							}
							bar._position = new Point(bar.GetPosition().X, bar.GetPosition().Y - 1);
						}
					}
				}
			}
			_position = position;
		}

		internal Point GetPosition()
		{
			return _position;
		}

		internal void SetPosition(Point point)
		{
			if (_position != point)
			{
				MoveToPosition(point);
			}
		}

		internal int GetMinimumLength()
		{
			if (GetFloatingWindow() != null)
			{
				if (MinFloatingLength > _maxItemLength)
				{
					return MinFloatingLength;
				}
				return _maxItemLength;
			}
			int length = 0;
			if (IsFullRowLayout() && GetHostRow() != null)
			{
				length = ((!_isHorizontal) ? GetHostRow()._bounds.Height : GetHostRow()._bounds.Width);
				if (length == 0)
				{
					return GetDefaultThickness();
				}
				return length;
			}
			if (_gripItem != null)
			{
				length = ((!_isHorizontal) ? (length + _gripItem._bounds.Height) : (length + _gripItem._bounds.Width));
			}
			if (_layoutItems != null && _layoutItems.Count > 0)
			{
				FormatStyleLayoutState uiFormatStyleLayoutState = _layoutItems.FindItemByIndex(0);
				length = ((!_isHorizontal) ? (length + uiFormatStyleLayoutState._bounds.Height) : (length + uiFormatStyleLayoutState._bounds.Width));
			}
			else
			{
				length += GetDefaultThickness();
			}
			if (_customizeItem != null || _layoutItems.Count > 1)
			{
				length = ((!IsThemed()) ? (length + CustomizeButtonLength) : (length + ThemedCustomizeButtonLength));
			}
			length += 2;
			if (GetHostRow() != null)
			{
				if (_isHorizontal)
				{
					if (GetHostRow()._bounds.Width < length)
					{
						return GetHostRow()._bounds.Width;
					}
				}
				else if (GetHostRow()._bounds.Height < length)
				{
					return GetHostRow()._bounds.Height;
				}
			}
			return length;
		}

		internal void MoveToTop(int requestedTop)
		{
			if (GetHostRow() == null || IsInitializing() || GetHostRow()._bounds.Height <= 0)
			{
				return;
			}
			if (CommandManager != null && CommandManager._isLoadingLayout)
			{
				base.Location = new Point(Location.X, requestedTop);
				return;
			}
			if (requestedTop < 0)
			{
				int newHeight = base.Height + (Location.Y + requestedTop);
				if (newHeight > GetFullLength())
				{
					newHeight = GetFullLength();
				}
				if (newHeight < GetMinimumLength())
				{
					newHeight = GetMinimumLength();
				}
				base.Height = newHeight;
				if (Location.Y == 0)
				{
					return;
				}
			}
			int minTop = GetHostRow().ComputeMinOffsetY(this);
			if (requestedTop < minTop)
			{
				requestedTop = minTop;
			}
			else
			{
				int maxTop = GetHostRow().ComputeMaxOffsetY(this);
				if (requestedTop > maxTop)
				{
					requestedTop = maxTop;
				}
			}
			int delta = requestedTop - Location.Y;
			if (_isRowLayoutInProgress)
			{
				delta = 0;
			}
			if (delta < 0)
			{
				int grownHeight = 0;
				if (!_isFullShown && GetHostRow().IsLayoutActive())
				{
					grownHeight = base.Height - delta;
					if (grownHeight > GetFullLength())
					{
						grownHeight = _fullLength;
					}
					if (grownHeight < GetMinimumLength())
					{
						grownHeight = GetMinimumLength();
					}
					if (requestedTop + grownHeight <= GetHostRow()._bounds.Bottom && base.Height != grownHeight)
					{
						base.Height = grownHeight;
						if (GetHostRow() != null)
						{
							GetHostRow().RecalculateContentSize();
						}
					}
				}
			}
			int rowBottom = GetHostRow().GetMaxAvailableExtent(this);
			if (requestedTop + base.Height > rowBottom)
			{
				int fitHeight = 0;
				fitHeight = ((GetHostRow().GetDesiredBarExtent(this) <= rowBottom - requestedTop) ? (rowBottom - requestedTop) : GetHostRow().GetDesiredBarExtent(this));
				if (fitHeight < GetMinimumLength())
				{
					fitHeight = GetMinimumLength();
				}
				if (base.Height != fitHeight)
				{
					base.Height = fitHeight;
					if (GetHostRow() != null)
					{
						GetHostRow().RecalculateContentSize();
					}
				}
			}
			base.Location = new Point(Location.X, requestedTop);
			GetHostRow().ResolveOverlapVertical(this, delta);
		}

		internal void MoveToLeft(int requestedLeft)
		{
			if (GetHostRow() == null || IsInitializing() || GetHostRow()._bounds.Width <= 0)
			{
				return;
			}
			if (CommandManager != null && CommandManager._isLoadingLayout)
			{
				base.Location = new Point(requestedLeft, Location.Y);
				return;
			}
			if (requestedLeft < 0)
			{
				int newWidth = base.Width + (Location.X + requestedLeft);
				if (newWidth > GetFullLength())
				{
					newWidth = GetFullLength();
				}
				if (newWidth < GetMinimumLength())
				{
					newWidth = GetMinimumLength();
				}
				base.Width = newWidth;
				if (Location.X == 0)
				{
					return;
				}
			}
			int minLeft = GetHostRow().ComputeMinOffsetX(this);
			if (requestedLeft < minLeft)
			{
				requestedLeft = minLeft;
			}
			else
			{
				int maxLeft = GetHostRow().ComputeMaxOffsetX(this);
				if (requestedLeft > maxLeft && maxLeft >= minLeft)
				{
					requestedLeft = maxLeft;
				}
			}
			int delta = requestedLeft - Location.X;
			if (_isRowLayoutInProgress)
			{
				delta = 0;
			}
			if (delta < 0)
			{
				int grownWidth = 0;
				if (!_isFullShown && GetHostRow().IsLayoutActive())
				{
					grownWidth = base.Width - delta;
					if (grownWidth > GetFullLength())
					{
						grownWidth = _fullLength;
					}
					if (grownWidth < GetMinimumLength())
					{
						grownWidth = GetMinimumLength();
					}
					if (requestedLeft + grownWidth <= GetHostRow()._bounds.Right && base.Width != grownWidth)
					{
						base.Width = grownWidth;
						if (GetHostRow() != null)
						{
							GetHostRow().RecalculateContentSize();
						}
					}
				}
			}
			int rowRight = GetHostRow().GetMaxAvailableExtent(this);
			if (requestedLeft + base.Width > rowRight)
			{
				int fitWidth = 0;
				fitWidth = ((GetHostRow().GetDesiredBarExtent(this) <= rowRight - requestedLeft) ? (rowRight - requestedLeft) : GetHostRow().GetDesiredBarExtent(this));
				if (fitWidth < GetMinimumLength())
				{
					fitWidth = GetMinimumLength();
				}
				if (base.Width != fitWidth)
				{
					base.Width = fitWidth;
					if (GetHostRow() != null)
					{
						GetHostRow().RecalculateContentSize();
					}
				}
			}
			base.Location = new Point(requestedLeft, Location.Y);
			GetHostRow().ResolveOverlapHorizontal(this, delta);
		}

		internal int GetFullLength()
		{
			return _fullLength;
		}

		internal void SetFullLength(int fullLength)
		{
			if (_fullLength != fullLength)
			{
				_fullLength = fullLength;
				if (_hostRow != null)
				{
					GetHostRow().RecalculateContentSize();
				}
			}
		}

		private int GetDefaultThickness()
		{
			return DefaultBarThickness;
		}

		private int GetCustomizeButtonLength()
		{
			if (IsThemed())
			{
				return ThemedCustomizeButtonLength;
			}
			ImageList imageList = ResolveImageList(GetEffectiveView(), null);
			if (imageList != null)
			{
				return imageList.ImageSize.Width / 2 + CustomizeButtonImageMargin;
			}
			return CustomizeButtonLength;
		}

		internal int GetThickness()
		{
			if (_hostRow != null)
			{
				return _hostRow.ComputeMaxBarThickness();
			}
			return _thickness;
		}

		internal void SetThickness(int thickness)
		{
			if (_thickness != thickness)
			{
				_thickness = thickness;
				if (GetHost() != null)
				{
					GetHost().LayoutRows();
				}
			}
		}
	}
}
