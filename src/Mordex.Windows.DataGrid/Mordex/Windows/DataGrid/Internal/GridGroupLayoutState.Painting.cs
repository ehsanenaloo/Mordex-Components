using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid.Internal
{
	internal partial class GridGroupLayoutState
	{
		internal void RenderContent(MxColorSchemeAppearanceState mordexColorSchemeAppearanceState, bool condition, Graphics graphics, Font font, bool useTextRenderer)
		{
			if (_hasRendered)
			{
				return;
			}
			_hasRendered = true;
			ButtonStyle buttonStyle = ButtonStyle.Ellipsis;
			Image image = null;
			string text = "";
			bool otherCondition = false;
			bool nextCondition = false;
			bool previousCondition = false;
			bool hasSelfReferencingChildren = false;
			bool firstCondition = false;
			int leftMargin = 0;
			bool secondCondition = false;
			if (_type == LayoutItemType.ColumnHeader)
			{
				if (_column.ShowRowSelector && _column.UseHeaderSelector)
				{
					_children = new GridGroupLayoutState[1];
					Rectangle rectangle = _rect;
					rectangle.X += _leftMargin;
					_children[0] = CreateRowSelectorItem(ref rectangle);
					_leftMargin += 16;
				}
			}
			else if (_type == LayoutItemType.Cell || _type == LayoutItemType.CardCell)
			{
				GridCell gridCell = GetCell();
				secondCondition = !gridCell.Enabled;
				if (_column.Table.GetExpandColumn() == _column)
				{
					leftMargin = RowStateHelper.GetSelfReferencingDepth(_gridGroupLayoutStateOwner.GridRow.GetLogicalRow()) + 1;
					leftMargin *= _column.Table.GetIndentWidth();
					_leftMargin += leftMargin;
					hasSelfReferencingChildren = RowStateHelper.HasSelfReferencingChildren(_gridGroupLayoutStateOwner.GridRow.GetLogicalRow());
				}
				if (_column.ShowRowSelector)
				{
					bool thirdCondition = false;
					if (_gridGroupLayoutStateOwner != null)
					{
						GridRow gridRow = _gridGroupLayoutStateOwner.GridRow;
						if (gridRow != null)
						{
							thirdCondition = gridRow.GetLogicalRow() is DataMordexRow;
						}
					}
					if (!thirdCondition)
					{
						_leftMargin += 16;
						firstCondition = true;
					}
				}
				buttonStyle = gridCell.ButtonStyle;
				otherCondition = buttonStyle != ButtonStyle.NoButton;
				image = _column.ButtonImage;
				text = _column.ButtonText;
				switch (gridCell.EditType)
				{
				case EditType.CalendarDropDown:
				case EditType.DatePicker:
				case EditType.Combo:
				case EditType.DropDownList:
				case EditType.MultiColumnCombo:
				case EditType.MultiColumnDropDown:
					nextCondition = true;
					break;
				case EditType.IntegerUpDown:
				case EditType.IntegerUpDownCombo:
				case EditType.ValueListUpDown:
				case EditType.ValueListUpDownCombo:
					previousCondition = true;
					break;
				}
				if (buttonStyle == ButtonStyle.ButtonCell)
				{
					nextCondition = false;
					previousCondition = false;
				}
			}
			else if (_type == LayoutItemType.FilterRowCell)
			{
				FilterEditType filterEditType = _column.FilterEditType;
				EditType editType = EditType.NoEdit;
				switch (filterEditType)
				{
				case FilterEditType.SameAsEditType:
					editType = _column.EditType;
					break;
				case FilterEditType.TextBox:
					editType = EditType.TextBox;
					break;
				case FilterEditType.Combo:
					editType = EditType.Combo;
					break;
				case FilterEditType.DropDownList:
					editType = EditType.DropDownList;
					break;
				case FilterEditType.DatePicker:
					editType = EditType.DatePicker;
					break;
				case FilterEditType.CalendarDropDown:
					editType = EditType.CalendarDropDown;
					break;
				case FilterEditType.CheckBox:
					editType = EditType.CheckBox;
					break;
				}
				switch (_column.ComputeFilterRowButtonStyle())
				{
				case FilterRowButtonStyle.None:
					otherCondition = false;
					break;
				case FilterRowButtonStyle.ClearButton:
					otherCondition = true;
					buttonStyle = ButtonStyle.Image;
					image = GridHelper.MakeBitmapTransparent(ResourceImages.GetRemoveFilterImage(), Color.Magenta);
					break;
				case FilterRowButtonStyle.ConditionOperatorDropDown:
					otherCondition = true;
					buttonStyle = ButtonStyle.Image;
					image = GetFilterOperatorImage(_column.FilterRowComparison);
					break;
				}
				if (_column.ActAsSelector)
				{
					otherCondition = false;
					editType = EditType.NoEdit;
				}
				switch (editType)
				{
				case EditType.CalendarDropDown:
				case EditType.DatePicker:
				case EditType.Combo:
				case EditType.DropDownList:
				case EditType.MultiColumnCombo:
				case EditType.MultiColumnDropDown:
					nextCondition = true;
					break;
				case EditType.IntegerUpDown:
				case EditType.IntegerUpDownCombo:
				case EditType.ValueListUpDown:
				case EditType.ValueListUpDownCombo:
					previousCondition = true;
					break;
				}
			}
			int value = 0;
			if (otherCondition)
			{
				value++;
			}
			if (nextCondition)
			{
				value++;
			}
			if (previousCondition)
			{
				value += 3;
			}
			if (hasSelfReferencingChildren)
			{
				value++;
			}
			if (firstCondition)
			{
				value++;
			}
			if (value == 0)
			{
				return;
			}
			_children = new GridGroupLayoutState[value];
			value--;
			Rectangle otherRectangle = _rect;
			otherRectangle.X += leftMargin;
			otherRectangle.Width -= leftMargin;
			if (HasRightBorder())
			{
				otherRectangle.Width--;
			}
			if (otherCondition)
			{
				GridGroupLayoutState gridGroupLayoutState = CreateEditButtonItem(ref otherRectangle, mordexColorSchemeAppearanceState, buttonStyle, image, text, font, graphics, useTextRenderer);
				if (_type == LayoutItemType.FilterRowCell)
				{
					if (_column.ComputeFilterRowButtonStyle() == FilterRowButtonStyle.ClearButton)
					{
						gridGroupLayoutState._type = LayoutItemType.FilterClearButton;
					}
					else
					{
						gridGroupLayoutState._type = LayoutItemType.FilterRowDropDown;
						if (_column.FilterRowComparison == ConditionOperator.IsEmpty || _column.FilterRowComparison == ConditionOperator.NotIsEmpty)
						{
							gridGroupLayoutState._rect = _rect;
							otherRectangle.Width = 0;
						}
					}
				}
				else
				{
					gridGroupLayoutState._type = LayoutItemType.CellColumnButton;
					gridGroupLayoutState._buttonStyle = buttonStyle;
				}
				_children[value] = gridGroupLayoutState;
				value--;
			}
			if (nextCondition)
			{
				GridGroupLayoutState editButtonItem = CreateEditButtonItem(ref otherRectangle, mordexColorSchemeAppearanceState, ButtonStyle.DownArrow, null, "", font, graphics, useTextRenderer);
				editButtonItem.SetDisabled(secondCondition);
				editButtonItem._type = LayoutItemType.CellDropDownButton;
				_children[value] = editButtonItem;
				value--;
			}
			if (previousCondition)
			{
				GridGroupLayoutState otherEditButtonItem = CreateEditButtonItem(ref otherRectangle, mordexColorSchemeAppearanceState, ButtonStyle.Ellipsis, null, "", font, graphics, useTextRenderer);
				otherEditButtonItem._type = LayoutItemType.CellUpDownBackground;
				_children[value] = otherEditButtonItem;
				value--;
				Rectangle bounds = otherEditButtonItem._rect;
				bounds.Height /= 2;
				GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.CellUpButton, _gridGroupLayoutStateOwner, _column, bounds);
				groupLayoutState.SetDisabled(secondCondition);
				_children[value] = groupLayoutState;
				value--;
				bounds = Rectangle.FromLTRB(otherEditButtonItem._rect.X, bounds.Bottom, otherEditButtonItem._rect.Right, otherEditButtonItem._rect.Bottom);
				groupLayoutState = new GridGroupLayoutState(LayoutItemType.CellDownButton, _gridGroupLayoutStateOwner, _column, bounds);
				groupLayoutState.SetDisabled(secondCondition);
				_children[value] = groupLayoutState;
				value--;
			}
			if (hasSelfReferencingChildren)
			{
				GridGroupLayoutState selfReferencingExpandBoxItem = CreateSelfReferencingExpandBoxItem(ref otherRectangle);
				selfReferencingExpandBoxItem._column = _column;
				_children[value] = selfReferencingExpandBoxItem;
				value--;
			}
			if (firstCondition)
			{
				GridGroupLayoutState rowSelectorItem = CreateRowSelectorItem(ref otherRectangle);
				rowSelectorItem.SetDisabled(secondCondition);
				_children[value] = rowSelectorItem;
				value--;
			}
		}

		internal TextLayoutRenderer ComputeTextLayoutRenderer(GridFormatStyle gridFormatStyle, bool useTextRenderer)
		{
			switch (_type)
			{
			case LayoutItemType.Cell:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.CardCell:
			case LayoutItemType.FilterRowCell:
				if (_column != null)
				{
					return _column.ComputeTextLayoutRenderer(gridFormatStyle, useTextRenderer);
				}
				break;
			case LayoutItemType.GroupByBoxHeader:
				if (_column != null)
				{
					return _column.CreateHeaderLayoutRenderer(gridFormatStyle, useTextRenderer);
				}
				break;
			case LayoutItemType.ColumnHeader:
				if (_column != null)
				{
					return _column.CreateHeaderLayoutRenderer(gridFormatStyle, useTextRenderer);
				}
				break;
			case LayoutItemType.CardHeader:
			{
				TextLayoutRenderer textLayoutRenderer = _column.CreateTextLayoutForAlignment(gridFormatStyle, _column.HeaderAlignment, useTextRenderer);
				GridCell gridCell = GetCell();
				if ((_column.WordWrap || (gridCell != null && gridCell.CellDisplayType == ColumnType.BoundImage)) && gridFormatStyle.LineAlignment == TextAlignment.Empty)
				{
					textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Near);
				}
				return textLayoutRenderer;
			}
			case LayoutItemType.ColumnSetHeader:
				if (_columnSet != null)
				{
					return _columnSet.ComputeTextLayoutRenderer(gridFormatStyle, useTextRenderer);
				}
				break;
			case LayoutItemType.GroupByBoxTableHeader:
			{
				TextLayoutRenderer otherTextLayoutRenderer = GridHelper.ComputeTextLayoutRenderer(gridFormatStyle, useTextRenderer);
				otherTextLayoutRenderer.SetWordWrap(false);
				otherTextLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				otherTextLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
				return otherTextLayoutRenderer;
			}
			case LayoutItemType.PreviewRow:
			{
				TextLayoutRenderer nextTextLayoutRenderer = GridHelper.ComputeTextLayoutRenderer(gridFormatStyle, useTextRenderer);
				nextTextLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Near);
				nextTextLayoutRenderer.SetWordWrap(true);
				return nextTextLayoutRenderer;
			}
			case LayoutItemType.TableHeader:
				return GridHelper.ComputeTextLayoutRenderer(gridFormatStyle, useTextRenderer);
			}
			return GridHelper.ComputeTextLayoutRenderer(gridFormatStyle, useTextRenderer);
		}
	}
}
