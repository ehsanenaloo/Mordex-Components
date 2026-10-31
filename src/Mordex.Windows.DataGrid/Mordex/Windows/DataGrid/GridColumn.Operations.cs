using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridColumn
	{
		internal void ResolveCustomGroupFromIndex()
		{
			if (_customGroupIndex >= 0 && _customGroupIndex < Table.CustomGroups.Count)
			{
				_customGroup = Table.CustomGroups[_customGroupIndex];
				_customGroupIndex = -1;
			}
		}

		protected override MordexValueItemCollectionBase BaseGetFilterValueList()
		{
			if (_columnType == ColumnType.CheckBox)
			{
				GridValueListItemCollection gridValueListItemCollection = new GridValueListItemCollection();
				gridValueListItemCollection.Add(CheckBoxTrueValue, bool.TrueString);
				gridValueListItemCollection.Add(CheckBoxFalseValue, bool.FalseString);
				return gridValueListItemCollection;
			}
			if (_dropDown != null && _dropDown.ReplaceValues)
			{
				return _dropDown.BuildValueList(CompareTarget == ColumnCompareTarget.Text);
			}
			return base.BaseGetFilterValueList();
		}

		protected override void Initialize(PropertyDescriptor propertyDescriptor)
		{
			base.Initialize(propertyDescriptor);
			if (propertyDescriptor == null)
			{
				return;
			}
			switch (GetDataTypeCode(propertyDescriptor.PropertyType))
			{
			case TypeCode.DateTime:
				_editType = EditType.DatePicker;
				FormatString = "d";
				break;
			case TypeCode.Boolean:
				_columnType = ColumnType.CheckBox;
				_editType = EditType.CheckBox;
				break;
			default:
				if (typeof(Image).IsAssignableFrom(propertyDescriptor.PropertyType) || propertyDescriptor.PropertyType == typeof(byte[]))
				{
					_columnType = ColumnType.BoundImage;
					_editType = EditType.NoEdit;
				}
				break;
			}
			if (propertyDescriptor.IsReadOnly)
			{
				_editType = EditType.NoEdit;
			}
		}

		internal PropertyDescriptor GetBasePropertyDescriptor()
		{
			return base.BasePropertyDescriptor;
		}

		protected override void OnDisplayNameChanged()
		{
			NotifyCollectionLayoutChanged();
		}

		protected override Image BaseGetDisplayImage()
		{
			if (_headerImage != null)
			{
				return _headerImage;
			}
			if (DataGrid != null)
			{
				ImageList imageList = DataGrid.ComputeImageList();
				if (imageList != null)
				{
					if (!string.IsNullOrEmpty(_headerImageKey) && imageList.Images.ContainsKey(_headerImageKey))
					{
						return imageList.Images[_headerImageKey];
					}
					if (_headerImageIndex >= 0 && _headerImageIndex < imageList.Images.Count)
					{
						return imageList.Images[_headerImageIndex];
					}
				}
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDefaultGroupFormatString()
		{
			BaseResetGroupFormatString();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDefaultGroupFormatMode()
		{
			BaseResetGroupFormatMode();
		}

		internal bool IsVisibleInCurrentLayout()
		{
			if (_owner != null && !_owner.Visible && Table.CellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return false;
			}
			return _visible;
		}

		internal static bool IsEffectivelyVisible(GridColumn gridColumn)
		{
			if (!gridColumn._visible)
			{
				return false;
			}
			if (gridColumn.Table != null && gridColumn.Table.CellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				if (gridColumn._owner == null)
				{
					return false;
				}
				return gridColumn.ColumnSet.Visible;
			}
			return gridColumn.Visible;
		}

		internal void NotifyCollectionLayoutChanged()
		{
			if (GetOwnerColumnCollection() != null)
			{
				GetOwnerColumnCollection().NotifyTableLayoutChanged();
			}
		}

		private void RequestAutoResize()
		{
			if (DataGrid != null && DataGrid.ColumnAutoResize)
			{
				Table._isAutoResizePending = true;
			}
		}

		internal void SetBaseParentCollection(GridColumnCollection parent)
		{
			BaseSetParentCollection(parent);
		}

		internal object GetRowValue(MordexRow row)
		{
			if (ColumnType == ColumnType.BoundImage)
			{
				object baseGetImageValue = BaseGetImageValue(row);
				return baseGetImageValue as Image;
			}
			return BaseGetValue(row);
		}

		internal object GetValueForCell(GridCell gridCell)
		{
			if (gridCell.CellDisplayType == ColumnType.BoundImage)
			{
				object baseGetImageValue = BaseGetImageValue(gridCell.Row.GetLogicalRow());
				return baseGetImageValue as Image;
			}
			return BaseGetValue(gridCell.Row.GetLogicalRow());
		}

		internal void SetRowValue(MordexRow row, GridCell gridCell, object value)
		{
			if (gridCell.CellDisplayType == ColumnType.BoundImage && value is Image)
			{
				Image source = (Image)value;
				Image filled = GridHelper.FillImageBackground(source, BoundImageMaskColor);
				if (filled == null)
				{
					BaseSetImageValue(row, source);
					return;
				}
				// A byte[] field stores a copy of the pixels, so the filled bitmap is not kept and is released here.
				bool storesBytes = BasePropertyDescriptor != null && BasePropertyDescriptor.PropertyType == typeof(byte[]);
				try
				{
					BaseSetImageValue(row, filled);
				}
				finally
				{
					if (storesBytes)
					{
						filled.Dispose();
					}
				}
			}
			else
			{
				BaseSetValue(row, value);
			}
		}

		protected override string BaseGetText(MordexRow row)
		{
			if (row.ExternalData == null && DataGrid != null)
			{
				GridRow gridRow = (GridRow)(row.ExternalData = DataGrid.GetOrCreateGridRow(row, false));
				try
				{
					DataGrid.PrepareRowForDisplay(gridRow, false);
					return gridRow.Cells[Index].Text;
				}
				finally
				{
					row.ExternalData = null;
				}
			}
			if (row.ExternalData is GridRow)
			{
				GridRow externalData = (GridRow)row.ExternalData;
				if (!externalData._isFormatting && DataGrid != null)
				{
					DataGrid.RaiseFormattingRow(externalData);
				}
				return externalData.Cells[Index].Text;
			}
			return base.BaseGetText(row);
		}

		internal string GetDisplayText(object value, bool condition)
		{
			if (!GridHelper.IsNullOrDBNull(value) && _passwordChar != 0)
			{
				return new string(_passwordChar, GridHelper.ToSafeString(value).Length);
			}
			string text = FormatValue(value);
			if (condition)
			{
				if (!_wordWrap)
				{
					text = text.Replace("\r", "");
					text = text.Replace("\n", " ");
				}
				switch (_characterCasing)
				{
				case CharacterCasing.Lower:
					return text.ToLower(GridHelper.GetCurrentCulture());
				case CharacterCasing.Upper:
					return text.ToUpper(GridHelper.GetCurrentCulture());
				default:
					return text;
				}
			}
			return text;
		}

		internal string GetTotalText(object args)
		{
			if (!GridHelper.IsNullOrDBNull(args))
			{
				if (_totalFormatMode == FormatMode.UseIFormattable)
				{
					if (args != null && typeof(IFormattable).IsAssignableFrom(args.GetType()))
					{
						return ((IFormattable)args).ToString(_summaryFormat, GridHelper.GetCurrentCulture());
					}
				}
				else
				{
					if (_totalFormatMode == FormatMode.UseInputMask)
					{
						string text = MaskEdit.FormatWithMask(args, InputMask);
						switch (_characterCasing)
						{
						case CharacterCasing.Lower:
							return text.ToLower(GridHelper.GetCurrentCulture());
						case CharacterCasing.Upper:
							return text.ToUpper(GridHelper.GetCurrentCulture());
						default:
							return text;
						}
					}
					if (_summaryFormat.Length > 0)
					{
						return string.Format(GridHelper.GetCurrentCulture(), _summaryFormat, args);
					}
				}
				return GridHelper.ToSafeString(args);
			}
			return "";
		}

		internal TextLayoutRenderer CreateTextLayoutForAlignment(GridFormatStyle gridFormatStyle, TextAlignment otherTextAlignment, bool useTextRenderer)
		{
			TextAlignment textAlignment = _textAlignment;
			_textAlignment = otherTextAlignment;
			TextLayoutRenderer result = ComputeTextLayoutRenderer(gridFormatStyle, useTextRenderer);
			_textAlignment = textAlignment;
			return result;
		}

		internal TextLayoutRenderer ComputeTextLayoutRenderer(GridFormatStyle gridFormatStyle, bool useTextRenderer)
		{
			TextLayoutRenderer textLayoutRenderer = null;
			textLayoutRenderer = ((_trimming != Trimming.Ellipsis) ? new TextLayoutRenderer(useTextRenderer) : new TextLayoutRenderer(useTextRenderer, StringTrimming.EllipsisCharacter));
			if (_textAlignment == TextAlignment.Empty)
			{
				textLayoutRenderer.SetStringAlignment(GridHelper.ToStringAlignmentDefaultNear(gridFormatStyle.TextAlignment));
			}
			else
			{
				textLayoutRenderer.SetStringAlignment(GridHelper.ToStringAlignmentDefaultNear(_textAlignment));
			}
			if (_lineAlignment == TextAlignment.Empty)
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(GridHelper.ToStringAlignmentDefaultCenter(gridFormatStyle.LineAlignment));
			}
			else
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(GridHelper.ToStringAlignmentDefaultCenter(_lineAlignment));
			}
			textLayoutRenderer.SetWordWrap(_wordWrap);
			if (IsRightToLeft())
			{
				textLayoutRenderer.UpdateRightToLeft(RightToLeft.Yes);
			}
			return textLayoutRenderer;
		}

		internal TextLayoutRenderer CreateHeaderLayoutRenderer(GridFormatStyle gridFormatStyle, bool useTextRenderer)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer);
			if (_headerAlignment == TextAlignment.Empty)
			{
				textLayoutRenderer.SetStringAlignment(GridHelper.ToStringAlignmentDefaultNear(gridFormatStyle.TextAlignment));
			}
			else
			{
				textLayoutRenderer.SetStringAlignment(GridHelper.ToStringAlignmentDefaultNear(_headerAlignment));
			}
			if (_trimming == Trimming.Ellipsis)
			{
				textLayoutRenderer.UpdateTrimming(StringTrimming.EllipsisCharacter);
			}
			if (_headerLineAlignment == TextAlignment.Empty)
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(GridHelper.ToStringAlignmentDefaultCenter(gridFormatStyle.LineAlignment));
			}
			else
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(GridHelper.ToStringAlignmentDefaultCenter(_headerLineAlignment));
			}
			if (Table != null && Table.HeaderLines > 1)
			{
				textLayoutRenderer.SetWordWrap(true);
			}
			return textLayoutRenderer;
		}

		internal void SetCardCaption()
		{
			_cardCaption = false;
		}

		internal void SetCardIcon()
		{
			_cardIcon = false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			BoundImageSize = Size.Empty;
		}

		internal int GetLastColumnSetRow()
		{
			return _columnSetRow + _rowSpan - 1;
		}

		internal bool ShouldHideWhenGrouped()
		{
			switch (_hideWhenGrouped)
			{
			case InheritableBoolean.False:
				return false;
			case InheritableBoolean.True:
				return true;
			default:
				if (Table == null)
				{
					return false;
				}
				return Table.ShouldHideColumnsWhenGrouped();
			}
		}

		internal bool IsRemoveAllowed()
		{
			switch (_allowRemove)
			{
			case InheritableBoolean.True:
				return true;
			case InheritableBoolean.False:
				return false;
			default:
				if (Table != null)
				{
					return Table.CanRemoveColumns();
				}
				return false;
			}
		}

		public void AutoSize()
		{
			if (DataGrid != null)
			{
				DataGrid.AutoSizeColumnToContent(this, false);
				return;
			}
			throw new InvalidOperationException();
		}

		internal ButtonStyle GetButtonStyle()
		{
			return _buttonStyle;
		}

		internal void ClearDisposedStyles()
		{
			if (!GridHelper.IsStyleAttached(_cellStyle))
			{
				_cellStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_headerStyle))
			{
				_headerStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_groupRowStyle))
			{
				_groupRowStyle = null;
			}
		}

		internal string GetEffectiveCellToolTip()
		{
			if (_cellToolTipContent == null)
			{
				if (Table != null)
				{
					return Table.GetEffectiveCellToolTipText();
				}
				return "";
			}
			return _cellToolTipContent;
		}

		internal CellToolTip ComputeCellToolTip()
		{
			if (_cellToolTip == CellToolTip.Default)
			{
				if (Table != null)
				{
					return Table.ComputeCellToolTip();
				}
				return CellToolTip.UseCellToolTipText;
			}
			return _cellToolTip;
		}

		void IFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			NotifyCollectionLayoutChanged();
		}

		internal bool IsRightToLeft()
		{
			switch (_rightToLeft)
			{
			case RightToLeft.Yes:
				return true;
			case RightToLeft.No:
				return false;
			default:
				return DataGrid?.GetScrollableBaseBoolean() ?? false;
			}
		}

		internal ColumnAutoSizeMode ComputeColumnAutoSizeMode(MxDataGrid dataGrid)
		{
			if (_autoSizeMode == ColumnAutoSizeMode.Default)
			{
				return dataGrid.ColumnAutoSizeMode;
			}
			return _autoSizeMode;
		}

		private ConditionOperator ComputeConditionOperator()
		{
			FilterConditionOperator defaultFilterRowComparison = _defaultFilterRowComparison;
			if (defaultFilterRowComparison == FilterConditionOperator.Default && DataGrid != null)
			{
				defaultFilterRowComparison = DataGrid.DefaultFilterRowComparison;
			}
			switch (defaultFilterRowComparison)
			{
			case FilterConditionOperator.BeginsWith:
				return ConditionOperator.BeginsWith;
			case FilterConditionOperator.Contains:
				return ConditionOperator.Contains;
			case FilterConditionOperator.Default:
				if (DataTypeCode == TypeCode.String || CompareTarget == ColumnCompareTarget.Text)
				{
					return ConditionOperator.BeginsWith;
				}
				return ConditionOperator.Equal;
			case FilterConditionOperator.NotContains:
				return ConditionOperator.NotContains;
			case FilterConditionOperator.EndsWith:
				return ConditionOperator.EndsWith;
			case FilterConditionOperator.Equal:
				return ConditionOperator.Equal;
			case FilterConditionOperator.GreaterThan:
				return ConditionOperator.GreaterThan;
			case FilterConditionOperator.GreaterThanOrEqual:
				return ConditionOperator.GreaterThanOrEqualTo;
			case FilterConditionOperator.LessThan:
				return ConditionOperator.LessThan;
			case FilterConditionOperator.LessThanOrEqual:
				return ConditionOperator.LessThanOrEqualTo;
			case FilterConditionOperator.NotEqual:
				return ConditionOperator.NotEqual;
			case FilterConditionOperator.IsEmpty:
				return ConditionOperator.IsEmpty;
			case FilterConditionOperator.NotIsEmpty:
				return ConditionOperator.NotIsEmpty;
			default:
				return ConditionOperator.Equal;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFilterRowComparison()
		{
			_filterRowComparison = null;
			if (DataGrid != null)
			{
				DataGrid.RefreshRowCountAndInvalidate();
			}
		}

		internal FilterRowButtonStyle ComputeFilterRowButtonStyle()
		{
			if (_filterRowButtonStyle == FilterRowButtonStyle.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.FilterRowButtonStyle;
				}
				return FilterRowButtonStyle.ClearButton;
			}
			return _filterRowButtonStyle;
		}
	}
}
