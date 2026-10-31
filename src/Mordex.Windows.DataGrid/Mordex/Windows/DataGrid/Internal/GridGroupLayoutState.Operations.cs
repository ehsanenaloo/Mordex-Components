using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid.Internal
{
	internal partial class GridGroupLayoutState
	{
		internal ButtonStyle GetButtonStyle()
		{
			return _buttonStyle;
		}

		internal void SetButtonStyle(ButtonStyle buttonStyle)
		{
			_buttonStyle = buttonStyle;
		}

		internal static string GetFilterOperatorText(MxDataGrid dataGrid, ConditionOperator conditionOperator)
		{
			switch (conditionOperator)
			{
			case ConditionOperator.BeginsWith:
				return "T*";
			case ConditionOperator.Contains:
				return "*T*";
			case ConditionOperator.EndsWith:
				return "*T";
			case ConditionOperator.Equal:
				return "=";
			case ConditionOperator.GreaterThan:
				return ">";
			case ConditionOperator.GreaterThanOrEqualTo:
				return ">=";
			case ConditionOperator.IsEmpty:
				return dataGrid.BuiltInTexts[GridBuiltInText.FilterRowConditionIsEmpty];
			case ConditionOperator.LessThan:
				return "<";
			case ConditionOperator.LessThanOrEqualTo:
				return "<=";
			case ConditionOperator.NotContains:
				return "{*T*}";
			case ConditionOperator.NotEqual:
				return "<>";
			case ConditionOperator.NotIsEmpty:
				return dataGrid.BuiltInTexts[GridBuiltInText.FilterRowConditionNotIsEmpty];
			default:
				return " ";
			}
		}

		internal static Image GetFilterOperatorImage(ConditionOperator conditionOperator)
		{
			Image result = null;
			switch (conditionOperator)
			{
			case ConditionOperator.BeginsWith:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterBeginsWithImage(), Color.Magenta);
				break;
			case ConditionOperator.Contains:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterContainsImage(), Color.Magenta);
				break;
			case ConditionOperator.EndsWith:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterEndsWithImage(), Color.Magenta);
				break;
			case ConditionOperator.Equal:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterEqualImage(), Color.Magenta);
				break;
			case ConditionOperator.GreaterThan:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterGreaterThanImage(), Color.Magenta);
				break;
			case ConditionOperator.GreaterThanOrEqualTo:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterGreaterThanorEqualsToImage(), Color.Magenta);
				break;
			case ConditionOperator.LessThan:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterLessThanImage(), Color.Magenta);
				break;
			case ConditionOperator.LessThanOrEqualTo:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterLessThanorEqualsToImage(), Color.Magenta);
				break;
			case ConditionOperator.NotEqual:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterNotEqualsImage(), Color.Magenta);
				break;
			case ConditionOperator.NotContains:
			case ConditionOperator.IsEmpty:
			case ConditionOperator.NotIsEmpty:
				result = GridHelper.MakeBitmapTransparent(ResourceImages.GetEmptyImage(), Color.Magenta);
				break;
			}
			return result;
		}

		private GridGroupLayoutState CreateRowSelectorItem(ref Rectangle rectangle)
		{
			Rectangle otherRectangle = rectangle;
			otherRectangle.X++;
			otherRectangle.Width = 15;
			GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowSelector, _gridGroupLayoutStateOwner, null, otherRectangle);
			gridGroupLayoutState._isHorizontallyScrolled = _isHorizontallyScrolled;
			return gridGroupLayoutState;
		}

		private GridGroupLayoutState CreateSelfReferencingExpandBoxItem(ref Rectangle rectangle)
		{
			Rectangle otherRectangle = rectangle;
			otherRectangle.X -= _column.Table.GetIndentWidth();
			otherRectangle.Width = _column.Table.GetIndentWidth();
			GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.SelfReferencingExpandBox, _gridGroupLayoutStateOwner, null, otherRectangle);
			gridGroupLayoutState._isHorizontallyScrolled = _isHorizontallyScrolled;
			return gridGroupLayoutState;
		}

		private GridGroupLayoutState CreateEditButtonItem(ref Rectangle rectangle, MxColorSchemeAppearanceState mordexColorSchemeAppearanceState, ButtonStyle buttonStyle, Image image, string text, Font font, Graphics graphics, bool useTextRenderer)
		{
			int otherWidth = SystemInformation.VerticalScrollBarWidth;
			if (buttonStyle == ButtonStyle.ButtonCell)
			{
				otherWidth = rectangle.Width - 2;
			}
			else if (buttonStyle == ButtonStyle.Image && image != null)
			{
				otherWidth = image.Width + 4;
			}
			else if (buttonStyle == ButtonStyle.TextButton && text.Length > 0 && graphics != null && font != null)
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer);
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
				otherWidth = (int)textLayoutRenderer.MeasureText(graphics, text, font, 0).Width;
				otherWidth += 6;
			}
			else if (mordexColorSchemeAppearanceState.IsThemedLook)
			{
				otherWidth = graphics != null ? (int)Math.Round(13 * graphics.DpiX / 96f) : 13;
			}
			int width = rectangle.Width;
			if (otherWidth > width)
			{
				otherWidth = width;
			}
			Rectangle otherRectangle = new Rectangle(rectangle.Right - otherWidth - 1, rectangle.Y + 1, otherWidth, rectangle.Height - 2);
			if (HasBottomBorder())
			{
				otherRectangle.Height--;
			}
			rectangle.Width -= otherWidth;
			GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CellColumnButton, _gridGroupLayoutStateOwner, _column, otherRectangle);
			gridGroupLayoutState._isHorizontallyScrolled = _isHorizontallyScrolled;
			if (buttonStyle == ButtonStyle.ButtonCell)
			{
				gridGroupLayoutState._owner = this;
			}
			return gridGroupLayoutState;
		}

		internal bool HasSize()
		{
			return _hasSize;
		}

		internal Size GetCachedSize()
		{
			return _cachedSize;
		}

		internal void SetCachedSize(Size size)
		{
			_cachedSize = size;
			_hasSize = true;
		}

		internal static GridGroupLayoutState CreateEmptyItem()
		{
			return new GridGroupLayoutState(LayoutItemType.EmptyItem, null, null);
		}

		internal bool IsHeaderItem()
		{
			switch (_type)
			{
			case LayoutItemType.GroupByBoxHeader:
			case LayoutItemType.ColumnHeader:
			case LayoutItemType.ColumnSetHeader:
				return true;
			default:
				return false;
			}
		}

		internal bool UsesVisualStyle(ControlStyleLayoutState controlStyleLayoutState)
		{
			if (IsButtonItem() && !IsDisabled())
			{
				return true;
			}
			switch (_type)
			{
			case LayoutItemType.GroupByBoxHeader:
			case LayoutItemType.ColumnHeader:
			case LayoutItemType.ColumnSetHeader:
				if (!controlStyleLayoutState.GetHeadersStyle()._isThemed)
				{
					return controlStyleLayoutState.GetHeadersStyle().IsThemedLook;
				}
				return true;
			case LayoutItemType.GroupExpandBox:
				if (!controlStyleLayoutState.GetHeadersStyle()._isThemed)
				{
					return controlStyleLayoutState.GetHeadersStyle().IsThemedLook;
				}
				return true;
			case LayoutItemType.ExpandBox:
			{
				MxDataGrid dataGrid = null;
				if (_gridGroupLayoutStateOwner != null && _gridGroupLayoutStateOwner.LayoutTable != null)
				{
					dataGrid = _gridGroupLayoutStateOwner.LayoutTable.DataGrid;
				}
				if (dataGrid != null)
				{
					switch (dataGrid.HierarchicalExpandBoxStyle)
					{
					case ExpandBoxStyle.Arrows:
						return true;
					}
					return false;
				}
				return false;
			}
			case LayoutItemType.CardSelector:
			case LayoutItemType.RowSelector:
				return controlStyleLayoutState.GetCheckBoxesStyle()._isThemed;
			case LayoutItemType.Cell:
			case LayoutItemType.CardCell:
				if (controlStyleLayoutState.GetHeadersStyle()._isThemed || controlStyleLayoutState.GetHeadersStyle().IsThemedLook)
				{
					GridCell gridCell = GetCell();
					if (gridCell != null && (gridCell.CellDisplayType == ColumnType.CheckBox || gridCell.CellDisplayType == ColumnType.Link))
					{
						return true;
					}
					return false;
				}
				return false;
			case LayoutItemType.CardExpandBox:
				return true;
			default:
				return false;
			}
		}

		internal bool IsDisabled()
		{
			if (_type == LayoutItemType.CellColumnButton)
			{
				GridCell gridCell = GetCell();
				if (gridCell != null)
				{
					return !gridCell.ButtonEnabled;
				}
			}
			return _isDisabled;
		}

		internal void SetDisabled(bool isDisabled)
		{
			_isDisabled = isDisabled;
		}

		internal bool IsCellItem()
		{
			if (_type != LayoutItemType.CardCell)
			{
				return _type == LayoutItemType.Cell;
			}
			return true;
		}

		public MordexRow GetMordexRow()
		{
			return _mordexRow;
		}

		public void SetMordexRow(MordexRow mordexRow)
		{
			if (_mordexRow != mordexRow)
			{
				_mordexRow = mordexRow;
			}
		}

		internal void CopyColumnMargins(GridColumn gridColumn)
		{
			_leftMargin = gridColumn.LeftMargin;
			_topMargin = gridColumn.TopMargin;
			_rightMargin = gridColumn.RightMargin;
			_bottomMargin = gridColumn.BottomMargin;
		}

		internal bool IsSideButton()
		{
			switch (_type)
			{
			case LayoutItemType.CellDropDownButton:
			case LayoutItemType.CellUpButton:
			case LayoutItemType.CellDownButton:
			case LayoutItemType.FilterClearButton:
			case LayoutItemType.FilterRowDropDown:
				return true;
			default:
				return false;
			}
		}

		internal bool IsButtonItem()
		{
			switch (_type)
			{
			case LayoutItemType.CellColumnButton:
			case LayoutItemType.CellDropDownButton:
			case LayoutItemType.CellUpButton:
			case LayoutItemType.CellDownButton:
			case LayoutItemType.CellUpDownBackground:
			case LayoutItemType.FilterClearButton:
			case LayoutItemType.FilterRowDropDown:
				return true;
			default:
				return false;
			}
		}

		internal Rectangle GetAbsoluteItemRect()
		{
			if (!_rect.IsEmpty && _gridGroupLayoutStateOwner != null)
			{
				return _gridGroupLayoutStateOwner.GetItemRect(_rect);
			}
			return _rect;
		}

		internal Rectangle GetContentRect(Rectangle rectangle, bool condition)
		{
			return CalculateContentRect(rectangle, condition);
		}

		internal Rectangle DeflateForGridlines(Rectangle rectangle, bool condition)
		{
			if (HasRightBorder())
			{
				if (condition)
				{
					rectangle.X++;
				}
				rectangle.Width--;
			}
			if (HasBottomBorder())
			{
				rectangle.Height--;
			}
			return rectangle;
		}

		internal Rectangle CalculateContentRect(Rectangle bounds, bool condition)
		{
			Rectangle result;
			if (_type == LayoutItemType.GroupHeaderRow)
			{
				result = Rectangle.Inflate(bounds, -1, -1);
			}
			else
			{
				int leftMargin = _leftMargin;
				int topMargin = _topMargin;
				int rightMargin = _rightMargin;
				int bottomMargin = _bottomMargin;
				if (_type == LayoutItemType.Cell || _type == LayoutItemType.CardCell || _type == LayoutItemType.FilterRowCell)
				{
					if (HasRightBorder())
					{
						if (condition)
						{
							bounds.X++;
						}
						bounds.Width--;
					}
					if (HasBottomBorder())
					{
						bounds.Height--;
					}
					bounds.Inflate(-1, -1);
					if (leftMargin != 0)
					{
						leftMargin--;
					}
					if (topMargin != 0)
					{
						topMargin--;
					}
					if (rightMargin != 0)
					{
						rightMargin--;
					}
					if (bottomMargin != 0)
					{
						bottomMargin--;
					}
					if (_children != null)
					{
						int value = 0;
						GridGroupLayoutState[] array = _children;
						foreach (GridGroupLayoutState gridGroupLayoutState in array)
						{
							if (gridGroupLayoutState._isVisible && gridGroupLayoutState._type != LayoutItemType.SelfReferencingExpandBox && gridGroupLayoutState._type != LayoutItemType.RowSelector && gridGroupLayoutState._type != LayoutItemType.CellUpButton && gridGroupLayoutState._type != LayoutItemType.CellDownButton)
							{
								value += gridGroupLayoutState._rect.Width;
							}
						}
						rightMargin += value;
					}
				}
				result = ((!condition) ? new Rectangle(bounds.X + leftMargin, bounds.Y + topMargin, bounds.Width - (leftMargin + rightMargin), bounds.Height - (topMargin + bottomMargin)) : new Rectangle(bounds.X + rightMargin, bounds.Y + topMargin, bounds.Width - (leftMargin + rightMargin), bounds.Height - (topMargin + bottomMargin)));
			}
			if (result.Width < 0 || result.Height < 0)
			{
				result.Width = 0;
				result.Height = 0;
			}
			return result;
		}

		internal object GetCellValue()
		{
			switch (_type)
			{
			case LayoutItemType.RowSelector:
				if (_gridGroupLayoutStateOwner is MxRowLayoutState mordexRowLayoutState && mordexRowLayoutState._rowType == LayoutRowType.ColumnHeader)
				{
					if (_gridGroupLayoutStateOwner != null && _gridGroupLayoutStateOwner.LayoutTable != null && _gridGroupLayoutStateOwner.LayoutTable.DataGrid != null)
					{
						MxDataGrid otherDataGrid = _gridGroupLayoutStateOwner.LayoutTable.DataGrid;
						if (GetMordexRow() != null)
						{
							MordexTableRow parentTableRow = RowStateHelper.FindParentTableRow(_mordexRow);
							return DataState.ComputeRowCheckState(parentTableRow.CheckedState);
						}
						if (otherDataGrid.GetRowSourceManager().RootRow != null)
						{
							return DataState.ComputeRowCheckState(otherDataGrid.GetRowSourceManager().RootRow.CheckedState);
						}
					}
					return false;
				}
				if (_gridGroupLayoutStateOwner.GridRow != null && _gridGroupLayoutStateOwner.GridRow.GetLogicalRow() != null)
				{
					return _gridGroupLayoutStateOwner.GridRow.CheckState;
				}
				return RowCheckState.Unchecked;
			case LayoutItemType.Cell:
				if (_gridGroupLayoutStateOwner.GridRow != null && _column != null)
				{
					if (!_column.ActAsSelector)
					{
						return _gridGroupLayoutStateOwner.GridRow.Cells[_column.Index].Value;
					}
					if (_column.DataGrid != null)
					{
						if (_column.DataGrid.RowCheckStateBehavior == RowCheckStateBehavior.CheckStateDependsOnChild)
						{
							return _gridGroupLayoutStateOwner.GridRow.CheckState;
						}
						return _gridGroupLayoutStateOwner.GridRow.IsChecked;
					}
				}
				return null;
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.CardCell:
			case LayoutItemType.FilterRowCell:
			case LayoutItemType.CardSelector:
				if (_gridGroupLayoutStateOwner.GridRow != null && _column != null)
				{
					return _gridGroupLayoutStateOwner.GridRow.Cells[_column.Index].Value;
				}
				return null;
			case LayoutItemType.PreviewRow:
				if (_gridGroupLayoutStateOwner.GridRow != null)
				{
					return _gridGroupLayoutStateOwner.GridRow.PreviewRowText;
				}
				return null;
			case LayoutItemType.ColumnHeader:
				if (_column != null && _column.ActAsSelector && _column.UseHeaderSelector)
				{
					if (_gridGroupLayoutStateOwner != null && _gridGroupLayoutStateOwner.LayoutTable != null && _gridGroupLayoutStateOwner.LayoutTable.DataGrid != null)
					{
						MxDataGrid dataGrid = _gridGroupLayoutStateOwner.LayoutTable.DataGrid;
						if (GetMordexRow() != null)
						{
							MordexTableRow mordexTableRow = RowStateHelper.FindParentTableRow(_mordexRow);
							return DataState.ComputeRowCheckState(mordexTableRow.CheckedState);
						}
						if (dataGrid.GetRowSourceManager().RootRow != null)
						{
							return DataState.ComputeRowCheckState(dataGrid.GetRowSourceManager().RootRow.CheckedState);
						}
					}
					return false;
				}
				return null;
			default:
				return null;
			}
		}

		internal string GetImageKey()
		{
			if (_owner != null)
			{
				return _owner.GetImageKey();
			}
			if (_type == LayoutItemType.ColumnHeader || _type == LayoutItemType.CardHeader || _type == LayoutItemType.GroupByBoxHeader)
			{
				if (_column != null)
				{
					return _column.HeaderImageKey;
				}
				return "";
			}
			if (_type == LayoutItemType.ColumnSetHeader)
			{
				return _columnSet.HeaderImageKey;
			}
			if (_type == LayoutItemType.TableHeader || _type == LayoutItemType.GroupByBoxTableHeader)
			{
				return GetOwnerTable().ImageKey;
			}
			if (_gridGroupLayoutStateOwner.GridRow != null)
			{
				if (_type == LayoutItemType.RowHeader)
				{
					return _gridGroupLayoutStateOwner.GridRow.RowHeaderImageKey;
				}
				if (_gridGroupLayoutStateOwner.GridRow.RowType == RowType.Record && _gridGroupLayoutStateOwner.GridRow.DataRow == null)
				{
					return "";
				}
				if (_column != null && _column.Visible)
				{
					return _gridGroupLayoutStateOwner.GridRow.Cells[_column.Index].ImageKey;
				}
			}
			return "";
		}

		public int GetImageIndex()
		{
			if (_owner != null)
			{
				return _owner.GetImageIndex();
			}
			if (_type == LayoutItemType.ColumnHeader || _type == LayoutItemType.CardHeader || _type == LayoutItemType.GroupByBoxHeader)
			{
				if (_column != null)
				{
					return _column.HeaderImageIndex;
				}
				return -1;
			}
			if (_type == LayoutItemType.ColumnSetHeader)
			{
				return _columnSet.HeaderImageIndex;
			}
			if (_type == LayoutItemType.TableHeader || _type == LayoutItemType.GroupByBoxTableHeader)
			{
				return GetOwnerTable().ImageIndex;
			}
			if (_gridGroupLayoutStateOwner.GridRow != null)
			{
				if (_type == LayoutItemType.RowHeader)
				{
					return _gridGroupLayoutStateOwner.GridRow.RowHeaderImageIndex;
				}
				if (_gridGroupLayoutStateOwner.GridRow.RowType == RowType.Record && _gridGroupLayoutStateOwner.GridRow.DataRow == null)
				{
					return -1;
				}
				if (_column != null && _column.Visible)
				{
					return _gridGroupLayoutStateOwner.GridRow.Cells[_column.Index].ImageIndex;
				}
			}
			return -1;
		}

		internal GridCell GetCell()
		{
			if (_type != LayoutItemType.GroupHeaderRow && _gridGroupLayoutStateOwner != null && _gridGroupLayoutStateOwner.GridRow != null && _column != null)
			{
				return _gridGroupLayoutStateOwner.GridRow.Cells[_column.Index];
			}
			return null;
		}

		internal string GetDisplayText()
		{
			if (_type == LayoutItemType.FilterRowDropDown)
			{
				return GetFilterOperatorText(_column.DataGrid, _column.FilterRowComparison);
			}
			if (_type == LayoutItemType.CellColumnButton && _column != null && _column.ButtonStyle == ButtonStyle.ButtonCell && _column.ButtonText.Length > 0)
			{
				return _column.ButtonText;
			}
			if (_owner != null)
			{
				return _owner.GetDisplayText();
			}
			switch (_type)
			{
			case LayoutItemType.GroupHeaderCell:
				if (_gridGroupLayoutStateOwner.GridRow != null && _column != null)
				{
					return _gridGroupLayoutStateOwner.GridRow.Cells[_column.Index].Text;
				}
				return "";
			case LayoutItemType.RowHeader:
				if (_gridGroupLayoutStateOwner != null && _gridGroupLayoutStateOwner.GridRow != null)
				{
					return _gridGroupLayoutStateOwner.GridRow.RowHeaderText;
				}
				return "";
			case LayoutItemType.CellColumnButton:
				if (_column != null)
				{
					return _column.ButtonText;
				}
				return "";
			case LayoutItemType.GroupHeaderRow:
				return _gridGroupLayoutStateOwner.GridRow.GroupCaption;
			case LayoutItemType.TableHeader:
			case LayoutItemType.GroupByBoxTableHeader:
				return GetOwnerTable().Caption;
			case LayoutItemType.CardCaption:
				if (_gridGroupLayoutStateOwner.GridRow != null)
				{
					return _gridGroupLayoutStateOwner.GridRow.CardCaptionText;
				}
				break;
			case LayoutItemType.Cell:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.CardCell:
			case LayoutItemType.FilterRowCell:
			{
				if (_gridGroupLayoutStateOwner is MxRowLayoutState)
				{
					MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)_gridGroupLayoutStateOwner;
					if (mordexRowLayoutState._rowType == LayoutRowType.EmptyRow)
					{
						return "";
					}
				}
				GridCell gridCell = GetCell();
				if (gridCell != null && gridCell.CellDisplayType != ColumnType.Image)
				{
					return gridCell.Text;
				}
				break;
			}
			case LayoutItemType.PreviewRow:
				if (_gridGroupLayoutStateOwner.GridRow != null)
				{
					return _gridGroupLayoutStateOwner.GridRow.PreviewRowText;
				}
				break;
			case LayoutItemType.GroupByBoxHeader:
				if (_gridGroup != null)
				{
					return _gridGroup.HeaderCaption;
				}
				break;
			case LayoutItemType.ColumnHeader:
			case LayoutItemType.CardHeader:
				if (_column != null)
				{
					if (_column.ActAsSelector && _column.UseHeaderSelector)
					{
						return "";
					}
					return _column.Caption;
				}
				break;
			case LayoutItemType.ColumnSetHeader:
				if (_columnSet != null)
				{
					return _columnSet.Caption;
				}
				break;
			case LayoutItemType.GroupByBoxInfo:
				if (_gridGroupLayoutStateOwner != null)
				{
					return _gridGroupLayoutStateOwner.LayoutTable.DataGrid.BuiltInTexts[GridBuiltInText.GroupByBoxInfo];
				}
				break;
			}
			return "";
		}

		private Image GetOwnImage()
		{
			switch (_type)
			{
			case LayoutItemType.Cell:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.CardCell:
			case LayoutItemType.FilterRowCell:
				if (_gridGroupLayoutStateOwner.GridRow != null && _column != null)
				{
					return _gridGroupLayoutStateOwner.GridRow.Cells[_column.Index].Image;
				}
				break;
			case LayoutItemType.CardCaption:
				return _ownImage;
			case LayoutItemType.GroupByBoxHeader:
			case LayoutItemType.ColumnHeader:
			case LayoutItemType.CardHeader:
				if (_column != null)
				{
					return _column.HeaderImage;
				}
				break;
			}
			return null;
		}

		internal bool HasLeftBorder()
		{
			if (_type == LayoutItemType.CardCell || _type == LayoutItemType.CardHeader)
			{
				return (_borderFlags & BorderFlags.Left) != 0;
			}
			return false;
		}

		internal bool HasTopBorder()
		{
			if (_gridGroupLayoutStateOwner is MxRowLayoutState)
			{
				MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)_gridGroupLayoutStateOwner;
				if (mordexRowLayoutState._isFirstRow && _rect.Y == 0)
				{
					switch (_type)
					{
					case LayoutItemType.GroupHeaderRow:
					case LayoutItemType.GroupTotalCell:
					case LayoutItemType.GroupExpandBox:
						if (GetOwnerTable().GetEffectiveGridLines() != GridLines.Both)
						{
							return GetOwnerTable().GetEffectiveGridLines() == GridLines.Horizontal;
						}
						return true;
					case LayoutItemType.Cell:
					case LayoutItemType.EmptyCell:
						if (GetOwnerTable().GetEffectiveGridLines() == GridLines.RowOutline)
						{
							return true;
						}
						return HasBottomBorder();
					case LayoutItemType.ColumnHeader:
					case LayoutItemType.RowHeader:
					case LayoutItemType.TotalRowSeparator:
					case LayoutItemType.ColumnSetHeader:
					case LayoutItemType.EmptyHeader:
					case LayoutItemType.TableHeader:
						return true;
					}
				}
			}
			else if (_type == LayoutItemType.CardCell || _type == LayoutItemType.CardHeader)
			{
				return (_borderFlags & BorderFlags.Top) != 0;
			}
			return false;
		}

		internal bool HasBottomBorder()
		{
			switch (_type)
			{
			case LayoutItemType.CardHeader:
			case LayoutItemType.CardCell:
				return (_borderFlags & BorderFlags.Bottom) != 0;
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.ExpandBox:
			case LayoutItemType.GroupIndent:
			case LayoutItemType.EmptyCell:
			case LayoutItemType.SpaceRow:
			{
				if ((_borderFlags & BorderFlags.Bottom) == 0)
				{
					return false;
				}
				GridLines gridLines = GetOwnerTable().GetEffectiveGridLines();
				switch (gridLines)
				{
				case GridLines.RowOutline:
					if (_gridGroupLayoutStateOwner is MxRowLayoutState mordexRowLayoutState && GetAbsoluteItemRect().Bottom == mordexRowLayoutState.GetContentBounds().Bottom)
					{
						return true;
					}
					return false;
				default:
					return gridLines == GridLines.Horizontal;
				case GridLines.Both:
					return true;
				}
			}
			case LayoutItemType.Cell:
			case LayoutItemType.PreviewRow:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupHeaderRow:
			case LayoutItemType.FilterRowCell:
			{
				if (!_drawBottomGridline)
				{
					return false;
				}
				GridLines gridLines = GetOwnerTable().GetEffectiveGridLines();
				switch (gridLines)
				{
				case GridLines.RowOutline:
					if (_gridGroupLayoutStateOwner is MxRowLayoutState rowLayoutState && GetAbsoluteItemRect().Bottom == rowLayoutState.GetContentBounds().Bottom)
					{
						return true;
					}
					return false;
				default:
					return gridLines == GridLines.Horizontal;
				case GridLines.Both:
					return true;
				}
			}
			case LayoutItemType.GroupExpandBox:
				if (!_gridGroupLayoutStateOwner.GridRow.Expanded)
				{
					GridLines gridLines = GetOwnerTable().GetEffectiveGridLines();
					if (gridLines != GridLines.Both && gridLines != GridLines.Horizontal)
					{
						return gridLines == GridLines.RowOutline;
					}
					return true;
				}
				return false;
			default:
				return false;
			}
		}

		internal bool HasRightBorder()
		{
			switch (_type)
			{
			case LayoutItemType.CardHeader:
			case LayoutItemType.CardCell:
				return (_borderFlags & BorderFlags.Right) != 0;
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.GroupIndent:
			case LayoutItemType.EmptyCell:
			{
				if ((_borderFlags & BorderFlags.Right) == 0)
				{
					return false;
				}
				GridLines gridLines = GetOwnerTable().GetEffectiveGridLines();
				switch (gridLines)
				{
				case GridLines.RowOutline:
					if (_gridGroupLayoutStateOwner is MxRowLayoutState mordexRowLayoutState && GetAbsoluteItemRect().Right == mordexRowLayoutState.GetContentBounds().Right)
					{
						return true;
					}
					return false;
				default:
					return gridLines == GridLines.Vertical;
				case GridLines.Both:
					return true;
				}
			}
			case LayoutItemType.Cell:
			case LayoutItemType.PreviewRow:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupHeaderRow:
			case LayoutItemType.ExpandBox:
			case LayoutItemType.FilterRowCell:
			{
				GridLines gridLines = GetOwnerTable().GetEffectiveGridLines();
				switch (gridLines)
				{
				case GridLines.RowOutline:
					if (_type == LayoutItemType.PreviewRow || _type == LayoutItemType.ExpandBox)
					{
						return true;
					}
					if (_gridGroupLayoutStateOwner is MxRowLayoutState rowLayoutState && GetAbsoluteItemRect().Right == rowLayoutState.GetContentBounds().Right)
					{
						return true;
					}
					return false;
				default:
					return gridLines == GridLines.Vertical;
				case GridLines.Both:
					return true;
				}
			}
			case LayoutItemType.Connector:
			{
				if (_rightMargin == -1)
				{
					return false;
				}
				GridLines gridLines = GetOwnerTable().GetEffectiveGridLines();
				if (gridLines != GridLines.Both && gridLines != GridLines.Vertical)
				{
					return gridLines == GridLines.RowOutline;
				}
				return true;
			}
			default:
				return false;
			}
		}

		internal Image ResolveImage(ImageList imageList)
		{
			if (_owner != null)
			{
				return _owner.GetDisplayImage(imageList);
			}
			Image image = GetOwnImage();
			if (image == null && imageList != null)
			{
				string text = GetImageKey();
				int indexOfKey = -1;
				if (text.Length > 0)
				{
					indexOfKey = imageList.Images.IndexOfKey(text);
				}
				indexOfKey = ((indexOfKey != -1) ? GetImageIndex() : GetImageIndex());
				if (indexOfKey >= 0 && indexOfKey < imageList.Images.Count)
				{
					try
					{
						image = imageList.Images[indexOfKey];
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
			}
			return image;
		}

		internal Image GetDisplayImage(ImageList imageList)
		{
			if (_owner != null)
			{
				return _owner.GetDisplayImage(imageList);
			}
			return GetOwnImage();
		}

		internal GridTable GetOwnerTable()
		{
			return _gridGroupLayoutStateOwner.LayoutTable;
		}

		internal bool IsRightToLeft(bool isRightToLeft)
		{
			switch (_type)
			{
			case LayoutItemType.Cell:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.CardCell:
			case LayoutItemType.FilterRowCell:
			case LayoutItemType.GroupHeaderCell:
				if (_column != null && _column.RightToLeft != RightToLeft.Inherit)
				{
					return _column.IsRightToLeft();
				}
				break;
			case LayoutItemType.GroupHeaderRow:
				if (_gridGroup != null && _gridGroup.Column != null && _gridGroup.Column.RightToLeft != RightToLeft.Inherit)
				{
					return _gridGroup.Column.IsRightToLeft();
				}
				break;
			}
			return isRightToLeft;
		}

		internal bool IsHitTestable(MxDataGrid dataGrid)
		{
			switch (_type)
			{
			case LayoutItemType.RowHeader:
				if (_gridGroupLayoutStateOwner is MxRowLayoutState && (((MxRowLayoutState)_gridGroupLayoutStateOwner)._rowType == LayoutRowType.EmptyRow || ((MxRowLayoutState)_gridGroupLayoutStateOwner)._rowType == LayoutRowType.ColumnHeader))
				{
					return false;
				}
				return true;
			case LayoutItemType.Cell:
			case LayoutItemType.EmptyCell:
				if (_gridGroupLayoutStateOwner is MxRowLayoutState && ((MxRowLayoutState)_gridGroupLayoutStateOwner)._rowType == LayoutRowType.EmptyRow)
				{
					return false;
				}
				return true;
			case LayoutItemType.ExpandBox:
			case LayoutItemType.GroupExpandBox:
			case LayoutItemType.SelfReferencingExpandBox:
				return dataGrid.SelectOnExpand;
			case LayoutItemType.PreviewRow:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupHeaderRow:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.CardCaption:
			case LayoutItemType.CardHeader:
			case LayoutItemType.CardCell:
			case LayoutItemType.CardExpandBox:
			case LayoutItemType.FilterRowCell:
			case LayoutItemType.CardSelector:
			case LayoutItemType.CellColumnButton:
			case LayoutItemType.CellDropDownButton:
			case LayoutItemType.CellUpButton:
			case LayoutItemType.CellDownButton:
			case LayoutItemType.FilterClearButton:
			case LayoutItemType.FilterRowDropDown:
			case LayoutItemType.RowSelector:
				return true;
			case LayoutItemType.GroupIndent:
				if (_gridGroup != null && _gridGroup.Table == GetOwnerTable())
				{
					return true;
				}
				return false;
			default:
				return false;
			}
		}

		public void ClearReferences()
		{
			_gridGroup = null;
			_column = null;
			_columnSet = null;
			SetMordexRow(null);
			_gridGroupLayoutStateOwner = null;
		}
	}
}
