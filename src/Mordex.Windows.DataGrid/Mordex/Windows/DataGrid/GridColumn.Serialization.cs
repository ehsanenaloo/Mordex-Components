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
		private static object ReadTypedLayoutValue(LayoutReader info, string key, string typeName, object fallback)
		{
			Type valueType = LayoutTypeResolver.Resolve(typeName);
			return valueType == null ? fallback : LayoutReader.GetPropertyValue(info, key, valueType, fallback);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_calendarTodayButtonVisible = (TriState)LayoutReader.GetPropertyValue(info, "CalendarTodayButtonVisible", typeof(TriState), TriState.Empty);
			_calendarNoneButtonVisible = (TriState)LayoutReader.GetPropertyValue(info, "CalendarNoneButtonVisible", typeof(TriState), TriState.Empty);
			_autoComplete = (bool)LayoutReader.GetPropertyValue(info, "AutoComplete", typeof(bool), true);
			if (!(bool)LayoutReader.GetPropertyValue(info, "Bound", typeof(bool), true))
			{
				BoundMode = ColumnBoundMode.UnboundFetch;
			}
			else
			{
				BoundMode = (ColumnBoundMode)LayoutReader.GetPropertyValue(info, "BoundMode", typeof(ColumnBoundMode), ColumnBoundMode.Bound);
			}
			base.BaseDataTypeCode = (TypeCode)LayoutReader.GetPropertyValue(info, "DataTypeCode", typeof(TypeCode), TypeCode.Object);
			_buttonToolTipContent = (string)LayoutReader.GetPropertyValue(info, "ButtonToolTipText", typeof(string), null);
			Caption = (string)LayoutReader.GetPropertyValue(info, "Caption", typeof(string), string.Empty);
			FilterBuilderDisplayName = (string)LayoutReader.GetPropertyValue(info, "FilterBuilderDisplayName", typeof(string), string.Empty);
			CompareIgnoreCase = (bool)LayoutReader.GetPropertyValue(info, "CompareIgnoreCase", typeof(bool), true);
			CompareTarget = (ColumnCompareTarget)LayoutReader.GetPropertyValue(info, "CompareTarget", typeof(ColumnCompareTarget), ColumnCompareTarget.Value);
			MultipleValuesSort = (ColumnCompareTarget)LayoutReader.GetPropertyValue(info, "MultipleValuesSort", typeof(ColumnCompareTarget), ColumnCompareTarget.Value);
			DataMember = (string)LayoutReader.GetPropertyValue(info, "DataMember", typeof(string), string.Empty);
			_rightToLeft = (RightToLeft)LayoutReader.GetPropertyValue(info, "RightToLeft", typeof(RightToLeft), RightToLeft.Inherit);
			ImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), string.Empty);
			_sortIndicator = (SortIndicator)LayoutReader.GetPropertyValue(info, "SortIndicator", typeof(SortIndicator), SortIndicator.Automatic);
			_checkBoxAlignment = (CheckBoxAlignment)LayoutReader.GetPropertyValue(info, "CheckBoxAlignment", typeof(CheckBoxAlignment), CheckBoxAlignment.Default);
			ExportValueKind exportValueKind = (ExportValueKind)LayoutReader.GetPropertyValue(info, "ExportValueKind", typeof(ExportValueKind), ExportValueKind.Automatic);
			_exportValueKind = Enum.IsDefined(typeof(ExportValueKind), exportValueKind) ? exportValueKind : ExportValueKind.Automatic;
			_buttonCaption = (string)LayoutReader.GetPropertyValue(info, "ButtonText", typeof(string), "");
			_buttonDisplayMode = (CellButtonDisplayMode)LayoutReader.GetPropertyValue(info, "ButtonDisplayMode", typeof(CellButtonDisplayMode), CellButtonDisplayMode.CurrentCell);
			_editButtonDisplayMode = (CellButtonDisplayMode)LayoutReader.GetPropertyValue(info, "EditButtonDisplayMode", typeof(CellButtonDisplayMode), CellButtonDisplayMode.EditingCell);
			_invalidValueAction = (InvalidValueAction)LayoutReader.GetPropertyValue(info, "InvalidValueAction", typeof(InvalidValueAction), InvalidValueAction.Default);
			_characterCasing = (CharacterCasing)LayoutReader.GetPropertyValue(info, "CharacterCasing", typeof(CharacterCasing), CharacterCasing.Normal);
			_aggregateFunction = (AggregateFunction)LayoutReader.GetPropertyValue(info, "AggregateFunction", typeof(AggregateFunction), AggregateFunction.None);
			_allowDrag = (bool)LayoutReader.GetPropertyValue(info, "AllowDrag", typeof(bool), true);
			_allowGroup = (bool)LayoutReader.GetPropertyValue(info, "AllowGroup", typeof(bool), true);
			_allowRemove = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "AllowRemove", typeof(InheritableBoolean), InheritableBoolean.Default);
			_allowSize = (bool)LayoutReader.GetPropertyValue(info, "AllowSize", typeof(bool), true);
			_allowSort = (bool)LayoutReader.GetPropertyValue(info, "AllowSort", typeof(bool), true);
			_bottomMargin = (int)LayoutReader.GetPropertyValue(info, "BottomMargin", typeof(int), 1);
			_imageMaskColor = (Color)LayoutReader.GetPropertyValue(info, "BoundImageMaskColor", typeof(Color), Color.Empty);
			_buttonImage = (Image)LayoutReader.GetPropertyValue(info, "ButtonImage", typeof(Image), null);
			_buttonStyle = (ButtonStyle)LayoutReader.GetPropertyValue(info, "ButtonStyle", typeof(ButtonStyle), ButtonStyle.NoButton);
			CardCaption = (bool)LayoutReader.GetPropertyValue(info, "CardCaption", typeof(bool), false);
			CardIcon = (bool)LayoutReader.GetPropertyValue(info, "CardIcon", typeof(bool), false);
			_cardViewCaptionVisible = (bool)LayoutReader.GetPropertyValue(info, "CardViewCaptionVisible", typeof(bool), true);
			_cellToolTip = (CellToolTip)LayoutReader.GetPropertyValue(info, "CellToolTip", typeof(CellToolTip), CellToolTip.Default);
			_cellToolTipContent = (string)LayoutReader.GetPropertyValue(info, "CellToolTipText", typeof(string), null);
			base.BaseDataTypeCode = (TypeCode)LayoutReader.GetPropertyValue(info, "DataTypeCode", typeof(TypeCode), TypeCode.Object);
			string text = "";
			_checkBoxFalseValue = false;
			PropertyValue propertyValue = info["CheckBoxFalseValue"];
			if (propertyValue != null)
			{
				text = (string)LayoutReader.GetPropertyValue(info, "TypeNameFalseValue", typeof(string), "NULL");
				if (text == "NULL")
				{
					_checkBoxFalseValue = null;
				}
				else
				{
					_checkBoxFalseValue = ReadTypedLayoutValue(info, "CheckBoxFalseValue", text, false);
				}
			}
			_checkBoxNullValue = null;
			propertyValue = info["CheckBoxNullValue"];
			if (propertyValue != null)
			{
				text = (string)LayoutReader.GetPropertyValue(info, "TypeNameNullValue", typeof(string), "NULL");
				if (text == "NULL")
				{
					_checkBoxNullValue = null;
				}
				else if (text == typeof(DBNull).FullName)
				{
					_checkBoxNullValue = DBNull.Value;
				}
				else
				{
					_checkBoxNullValue = ReadTypedLayoutValue(info, "CheckBoxNullValue", text, null);
				}
			}
			_checkBoxTrueValue = true;
			propertyValue = info["CheckBoxTrueValue"];
			if (propertyValue != null)
			{
				text = (string)LayoutReader.GetPropertyValue(info, "TypeNameTrueValue", typeof(string), "NULL");
				if (text == "NULL")
				{
					_checkBoxTrueValue = null;
				}
				else
				{
					_checkBoxTrueValue = ReadTypedLayoutValue(info, "CheckBoxTrueValue", text, true);
				}
			}
			_emptyTextValue = DBNull.Value;
			propertyValue = info["EmptyStringValue"];
			if (propertyValue != null)
			{
				text = (string)LayoutReader.GetPropertyValue(info, "TypeNameEmptyStringValue", typeof(string), "NULL");
				if (text == "NULL")
				{
					_emptyTextValue = null;
				}
				else
				{
					_emptyTextValue = ReadTypedLayoutValue(info, "EmptyStringValue", text, "");
				}
			}
			_checkBoxTriState = (bool)LayoutReader.GetPropertyValue(info, "CheckBoxTriState", typeof(bool), false);
			ColumnType = (ColumnType)LayoutReader.GetPropertyValue(info, "ColumnType", typeof(ColumnType), ColumnType.Text);
			DefaultGroupFormatMode = (FormatMode)LayoutReader.GetPropertyValue(info, "DefaultGroupFormatMode", typeof(FormatMode), FormatMode.UseDefault);
			DefaultGroupFormatString = (string)LayoutReader.GetPropertyValue(info, "DefaultGroupFormatString", typeof(string), null);
			propertyValue = info["FilterRowComparison"];
			if (propertyValue != null)
			{
				_filterRowComparison = (ConditionOperator)LayoutReader.GetPropertyValue(info, "FilterRowComparison", typeof(ConditionOperator), ConditionOperator.Equal);
			}
			_filterRowButtonStyle = (FilterRowButtonStyle)LayoutReader.GetPropertyValue(info, "FilterRowButtonStyle", typeof(FilterRowButtonStyle), FilterRowButtonStyle.Default);
			_defaultFilterRowComparison = (FilterConditionOperator)LayoutReader.GetPropertyValue(info, "DefaultFilterRowComparison", typeof(FilterConditionOperator), FilterConditionOperator.Default);
			DefaultGroupInterval = (GroupInterval)LayoutReader.GetPropertyValue(info, "DefaultGroupInterval", typeof(GroupInterval), GroupInterval.Value);
			_defaultGroupPrefix = (string)LayoutReader.GetPropertyValue(info, "DefaultGroupPrefix", typeof(string), null);
			_editTarget = (EditTarget)LayoutReader.GetPropertyValue(info, "EditTarget", typeof(EditTarget), EditTarget.Value);
			_editType = (EditType)LayoutReader.GetPropertyValue(info, "EditType", typeof(EditType), EditType.TextBox);
			FormatString = (string)LayoutReader.GetPropertyValue(info, "FormatString", typeof(string), string.Empty);
			GroupEmptyString = (string)LayoutReader.GetPropertyValue(info, "GroupEmptyString", typeof(string), "(none)");
			_headerAlignment = (TextAlignment)LayoutReader.GetPropertyValue(info, "HeaderAlignment", typeof(TextAlignment), TextAlignment.Empty);
			_headerImage = (Image)LayoutReader.GetPropertyValue(info, "HeaderImage", typeof(Image), null);
			_headerImageIndex = (int)LayoutReader.GetPropertyValue(info, "HeaderImageIndex", typeof(int), -1);
			_headerImageKey = (string)LayoutReader.GetPropertyValue(info, "HeaderImageKey", typeof(string), string.Empty);
			_headerLineAlignment = (TextAlignment)LayoutReader.GetPropertyValue(info, "HeaderLineAlignment", typeof(TextAlignment), TextAlignment.Empty);
			_headerToolTip = (string)LayoutReader.GetPropertyValue(info, "HeaderToolTip", typeof(string), string.Empty);
			_hideWhenGrouped = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "HideWhenGrouped", typeof(InheritableBoolean), InheritableBoolean.Default);
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_columnImage = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_boundImageSize = (Size)LayoutReader.GetPropertyValue(info, "ImageSize", typeof(Size), Size.Empty);
			_includeLiterals = (bool)LayoutReader.GetPropertyValue(info, "IncludeLiterals", typeof(bool), true);
			InputMask = (string)LayoutReader.GetPropertyValue(info, "InputMask", typeof(string), string.Empty);
			Key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_leftMargin = (int)LayoutReader.GetPropertyValue(info, "LeftMargin", typeof(int), 1);
			_lineAlignment = (TextAlignment)LayoutReader.GetPropertyValue(info, "LineAlignment", typeof(TextAlignment), TextAlignment.Empty);
			_maskPrompt = (char)LayoutReader.GetPropertyValue(info, "MaskPrompt", typeof(char), '_');
			_passwordChar = (char)LayoutReader.GetPropertyValue(info, "PasswordChar", typeof(char), '\0');
			_maxLength = (int)LayoutReader.GetPropertyValue(info, "MaxLength", typeof(int), 0);
			_maxLines = (int)LayoutReader.GetPropertyValue(info, "MaxLines", typeof(int), 1);
			_minLines = (int)LayoutReader.GetPropertyValue(info, "MinLines", typeof(int), 1);
			NullText = (string)LayoutReader.GetPropertyValue(info, "NullText", typeof(string), string.Empty);
			_position = (int)LayoutReader.GetPropertyValue(info, "Position", typeof(int), -1);
			_printMode = (ColumnPrintMode)LayoutReader.GetPropertyValue(info, "PrintMode", typeof(ColumnPrintMode), ColumnPrintMode.PrintWhenVisible);
			_rightMargin = (int)LayoutReader.GetPropertyValue(info, "RightMargin", typeof(int), 1);
			_selectable = (bool)LayoutReader.GetPropertyValue(info, "Selectable", typeof(bool), true);
			_gridColumnSelectableCells = (SelectableCells)LayoutReader.GetPropertyValue(info, "SelectableCells", typeof(SelectableCells), SelectableCells.RowCells | SelectableCells.NewRowCells | SelectableCells.FilterRowCells);
			_showInFieldChooser = (bool)LayoutReader.GetPropertyValue(info, "ShowInFieldChooser", typeof(bool), true);
			_textAlignment = (TextAlignment)LayoutReader.GetPropertyValue(info, "TextAlignment", typeof(TextAlignment), TextAlignment.Empty);
			_topMargin = (int)LayoutReader.GetPropertyValue(info, "TopMargin", typeof(int), 1);
			_visible = (bool)LayoutReader.GetPropertyValue(info, "Visible", typeof(bool), true);
			_wordWrap = (bool)LayoutReader.GetPropertyValue(info, "WordWrap", typeof(bool), false);
			FormatMode = (FormatMode)LayoutReader.GetPropertyValue(info, "FormatMode", typeof(FormatMode), FormatMode.UseIFormattable);
			_totalFormatMode = (FormatMode)LayoutReader.GetPropertyValue(info, "TotalFormatMode", typeof(FormatMode), FormatMode.UseIFormattable);
			_summaryFormat = (string)LayoutReader.GetPropertyValue(info, "TotalFormatString", typeof(string), string.Empty);
			_trimming = (Trimming)LayoutReader.GetPropertyValue(info, "Trimming", typeof(Trimming), Trimming.Ellipsis);
			_ownerDrawnMode = (ColumnOwnerDrawnMode)LayoutReader.GetPropertyValue(info, "OwnerDrawnMode", typeof(ColumnOwnerDrawnMode), ColumnOwnerDrawnMode.None);
			_upDownMinimum = (int)LayoutReader.GetPropertyValue(info, "UpDownMinimum", typeof(int), 0);
			_upDownMaximum = (int)LayoutReader.GetPropertyValue(info, "UpDownMaximum", typeof(int), 100);
			_upDownIncrement = (int)LayoutReader.GetPropertyValue(info, "UpDownIncrement", typeof(int), 1);
			_limitToList = (bool)LayoutReader.GetPropertyValue(info, "LimitToList", typeof(bool), true);
			ActAsSelector = (bool)LayoutReader.GetPropertyValue(info, "ActAsSelector", typeof(bool), false);
			_useHeaderSelector = (bool)LayoutReader.GetPropertyValue(info, "UseHeaderSelector", typeof(bool), true);
			_tag = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			_autoSizeMode = (ColumnAutoSizeMode)LayoutReader.GetPropertyValue(info, "AutoSizeMode", typeof(ColumnAutoSizeMode), ColumnAutoSizeMode.Default);
			_defaultValue = (string)LayoutReader.GetPropertyValue(info, "DefaultValue", typeof(string), null);
			if (_defaultValue != null)
			{
				if (_defaultValue is string && (string)_defaultValue == "{DBNULL}")
				{
					_defaultValue = DBNull.Value;
				}
				else
				{
					text = (string)LayoutReader.GetPropertyValue(info, "DefaultValueType", typeof(string), null);
					if (text != null)
					{
						_defaultValue = ReadTypedLayoutValue(info, "DefaultValue", text, _defaultValue);
					}
				}
			}
			if (_owner == null)
			{
				_width = (int)LayoutReader.GetPropertyValue(info, "Width", typeof(int), 100);
			}
			HasValueList = (bool)LayoutReader.GetPropertyValue(info, "HasValueList", typeof(bool), false);
			if (HasValueList)
			{
				info.DeserializeObject("ValueList", ValueList);
			}
			if (_owner == null)
			{
				_columnSetColumn = -1;
				_columnSetRow = -1;
				_colSpan = -1;
				_rowSpan = -1;
			}
			PropertyValue infoItem = info["CellStyle"];
			if (infoItem == null)
			{
				CellStyle = DeserializeGridColumnState(info, "CellStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(CellStyle);
			}
			infoItem = info["GroupRowStyle"];
			if (infoItem == null)
			{
				GroupRowStyle = DeserializeGridColumnState(info, "GroupRowStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(GroupRowStyle);
			}
			infoItem = info["HeaderStyle"];
			if (infoItem == null)
			{
				HeaderStyle = DeserializeGridColumnState(info, "HeaderStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(HeaderStyle);
			}
			MultipleValues = (bool)LayoutReader.GetPropertyValue(info, "MultipleValues", typeof(bool), false);
			MultipleValueDataMember = (string)LayoutReader.GetPropertyValue(info, "MultipleValueDataMember", typeof(string), "");
			_showRowSelector = (bool)LayoutReader.GetPropertyValue(info, "ShowRowSelector", typeof(bool), false);
			if (DataGrid != null)
			{
				int otherPropertyValue = (int)LayoutReader.GetPropertyValue(info, "DropDownIndex", typeof(int), -1);
				if (otherPropertyValue >= 0 && otherPropertyValue < DataGrid.DropDowns.Count)
				{
					_dropDown = DataGrid.DropDowns[otherPropertyValue];
				}
			}
			if (_position == -1)
			{
				_position = Index;
			}
			_customGroupIndex = (int)LayoutReader.GetPropertyValue(info, "CustomGroupIndex", typeof(int), -1);
			FilterEditType = (FilterEditType)LayoutReader.GetPropertyValue(info, "FilterEditType", typeof(FilterEditType), FilterEditType.SameAsEditType);
		}

		private GridFormatStyle DeserializeGridColumnState(LayoutReader reader, string propertyName)
		{
			MxDataGrid dataGrid = DataGrid;
			if (dataGrid != null)
			{
				int propertyValue = (int)LayoutReader.GetPropertyValue(reader, propertyName, typeof(int), -1);
				if (propertyValue >= 0 && propertyValue < dataGrid.FormatStyles.Count)
				{
					return dataGrid.FormatStyles[propertyValue];
				}
			}
			return null;
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (!CompareIgnoreCase)
			{
				info.AddValue("CompareIgnoreCase", CompareIgnoreCase);
			}
			if (CompareTarget != ColumnCompareTarget.Value)
			{
				info.AddValue("CompareTarget", CompareTarget);
			}
			if (MultipleValuesSort != ColumnCompareTarget.Value)
			{
				info.AddValue("MultipleValuesSort", MultipleValuesSort);
			}
			if (_invalidValueAction != InvalidValueAction.Default)
			{
				info.AddValue("InvalidValueAction", _invalidValueAction);
			}
			if (_buttonDisplayMode != CellButtonDisplayMode.CurrentCell)
			{
				info.AddValue("ButtonDisplayMode", _buttonDisplayMode);
			}
			if (_editButtonDisplayMode != CellButtonDisplayMode.EditingCell)
			{
				info.AddValue("EditButtonDisplayMode", _editButtonDisplayMode);
			}
			if (_characterCasing != CharacterCasing.Normal)
			{
				info.AddValue("CharacterCasing", _characterCasing);
			}
			if (_aggregateFunction != AggregateFunction.None)
			{
				info.AddValue("AggregateFunction", _aggregateFunction);
			}
			if (!_allowDrag)
			{
				info.AddValue("AllowDrag", _allowDrag);
			}
			if (!_allowGroup)
			{
				info.AddValue("AllowGroup", _allowGroup);
			}
			if (_allowRemove != InheritableBoolean.Default)
			{
				info.AddValue("AllowRemove", _allowRemove);
			}
			if (!_allowSize)
			{
				info.AddValue("AllowSize", _allowSize);
			}
			if (!_allowSort)
			{
				info.AddValue("AllowSort", _allowSort);
			}
			if (_bottomMargin != 1)
			{
				info.AddValue("BottomMargin", _bottomMargin);
			}
			if (!_imageMaskColor.IsEmpty)
			{
				info.AddValue("BoundImageMaskColor", _imageMaskColor);
			}
			if (_buttonImage != null)
			{
				info.AddValue("ButtonImage", _buttonImage);
			}
			if (_buttonCaption.Length > 0)
			{
				info.AddValue("ButtonText", _buttonCaption);
			}
			if (_buttonToolTipContent != null)
			{
				info.AddValue("ButtonToolTipText", _buttonToolTipContent);
			}
			if (_buttonStyle != ButtonStyle.NoButton)
			{
				info.AddValue("ButtonStyle", _buttonStyle);
			}
			if (Caption != string.Empty)
			{
				info.AddValue("Caption", Caption);
			}
			if (FilterBuilderDisplayName != string.Empty)
			{
				info.AddValue("FilterBuilderDisplayName", FilterBuilderDisplayName);
			}
			if (_cardCaption)
			{
				info.AddValue("CardCaption", _cardCaption);
			}
			if (_cardIcon)
			{
				info.AddValue("CardIcon", _cardIcon);
			}
			if (!_cardViewCaptionVisible)
			{
				info.AddValue("CardViewCaptionVisible", _cardViewCaptionVisible);
			}
			if (_cellToolTip != CellToolTip.Default)
			{
				info.AddValue("CellToolTip", _cellToolTip);
			}
			if (_cellToolTipContent != null)
			{
				info.AddValue("CellToolTipText", _cellToolTipContent);
			}
			if (_checkBoxAlignment != CheckBoxAlignment.Default)
			{
				info.AddValue("CheckBoxAlignment", _checkBoxAlignment);
			}
			if (_exportValueKind != ExportValueKind.Automatic)
			{
				info.AddValue("ExportValueKind", _exportValueKind);
			}
			if (base.BaseDataTypeCode != TypeCode.Object)
			{
				info.AddValue("DataTypeCode", base.BaseDataTypeCode);
			}
			if (GridHelper.CompareValues(_checkBoxFalseValue, false, null, true) != 0)
			{
				if (_checkBoxFalseValue == null)
				{
					info.AddValue("TypeNameFalseValue", "NULL");
				}
				else
				{
					info.AddValue("TypeNameFalseValue", _checkBoxFalseValue.GetType().FullName);
				}
				info.AddValue("CheckBoxFalseValue", _checkBoxFalseValue);
			}
			if (_checkBoxNullValue != null)
			{
				info.AddValue("TypeNameNullValue", _checkBoxNullValue.GetType().FullName);
				info.AddValue("CheckBoxNullValue", _checkBoxNullValue);
			}
			if (_checkBoxTriState)
			{
				info.AddValue("CheckBoxTriState", _checkBoxTriState);
			}
			if (GridHelper.CompareValues(_checkBoxTrueValue, true, null, true) != 0)
			{
				if (_checkBoxTrueValue == null)
				{
					info.AddValue("TypeNameTrueValue", "NULL");
				}
				else
				{
					info.AddValue("TypeNameTrueValue", _checkBoxTrueValue.GetType().FullName);
				}
				info.AddValue("CheckBoxTrueValue", _checkBoxTrueValue);
			}
			if (_emptyTextValue != DBNull.Value)
			{
				if (_emptyTextValue == null)
				{
					info.AddValue("TypeNameEmptyStringValue", "NULL");
				}
				else
				{
					info.AddValue("TypeNameEmptyStringValue", _emptyTextValue.GetType().FullName);
				}
				info.AddValue("EmptyStringValue", _emptyTextValue);
			}
			if (BoundMode != ColumnBoundMode.Bound)
			{
				info.AddValue("BoundMode", BoundMode);
			}
			if (SortIndicator != SortIndicator.Automatic)
			{
				info.AddValue("SortIndicator", _sortIndicator);
			}
			if (ColumnType != ColumnType.Text)
			{
				info.AddValue("ColumnType", ColumnType);
			}
			if (DataMember != string.Empty)
			{
				info.AddValue("DataMember", DataMember);
			}
			if (ShouldSerializeDefaultGroupFormatMode())
			{
				info.AddValue("DefaultGroupFormatMode", DefaultGroupFormatMode);
			}
			if (ShouldSerializeDefaultGroupFormatString())
			{
				info.AddValue("DefaultGroupFormatString", base.BaseGroupFormatString);
			}
			if (DefaultGroupInterval != GroupInterval.Value)
			{
				info.AddValue("DefaultGroupInterval", DefaultGroupInterval);
			}
			if (_defaultGroupPrefix != null)
			{
				info.AddValue("DefaultGroupPrefix", _defaultGroupPrefix);
			}
			if (_editTarget != EditTarget.Value)
			{
				info.AddValue("EditTarget", _editTarget);
			}
			if (_editType != EditType.TextBox)
			{
				info.AddValue("EditType", _editType);
			}
			if (_filterEditType != FilterEditType.SameAsEditType)
			{
				info.AddValue("FilterEditType", _filterEditType);
			}
			if (FormatString != string.Empty)
			{
				info.AddValue("FormatString", FormatString);
			}
			if (GroupEmptyString != "(none)")
			{
				info.AddValue("GroupEmptyString", GroupEmptyString);
			}
			if (_headerAlignment != TextAlignment.Empty)
			{
				info.AddValue("HeaderAlignment", _headerAlignment);
			}
			if (_headerImage != null)
			{
				info.AddValue("HeaderImage", _headerImage);
			}
			if (_headerImageIndex != -1)
			{
				info.AddValue("HeaderImageIndex", _headerImageIndex);
			}
			if (_headerImageKey.Length > 0)
			{
				info.AddValue("HeaderImageKey", _headerImageKey);
			}
			if (_headerLineAlignment != TextAlignment.Empty)
			{
				info.AddValue("HeaderLineAlignment", _headerLineAlignment);
			}
			if (_headerToolTip != string.Empty)
			{
				info.AddValue("HeaderToolTip", _headerToolTip);
			}
			if (_hideWhenGrouped != InheritableBoolean.Default)
			{
				info.AddValue("HideWhenGrouped", _hideWhenGrouped);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_columnImage != null)
			{
				info.AddValue("Image", _columnImage);
			}
			if (!_boundImageSize.IsEmpty)
			{
				info.AddValue("ImageSize", _boundImageSize);
			}
			if (!_includeLiterals)
			{
				info.AddValue("IncludeLiterals", _includeLiterals);
			}
			if (InputMask != string.Empty)
			{
				info.AddValue("InputMask", InputMask);
			}
			if (Key != string.Empty)
			{
				info.AddValue("Key", Key);
			}
			if (_leftMargin != 1)
			{
				info.AddValue("LeftMargin", _leftMargin);
			}
			if (_lineAlignment != TextAlignment.Empty)
			{
				info.AddValue("LineAlignment", _lineAlignment);
			}
			if (_maskPrompt != '_')
			{
				info.AddValue("MaskPrompt", _maskPrompt);
			}
			if (_passwordChar != 0)
			{
				info.AddValue("PasswordChar", _passwordChar);
			}
			if (_maxLength != 0)
			{
				info.AddValue("MaxLength", _maxLength);
			}
			if (_maxLines != 1)
			{
				info.AddValue("MaxLines", _maxLines);
			}
			if (_minLines != 1)
			{
				info.AddValue("MinLines", _minLines);
			}
			if (NullText != string.Empty)
			{
				info.AddValue("NullText", NullText);
			}
			if (_printMode != ColumnPrintMode.PrintWhenVisible)
			{
				info.AddValue("PrintMode", _printMode);
			}
			if (_position != -1)
			{
				info.AddValue("Position", _position);
			}
			if (_rightMargin != 1)
			{
				info.AddValue("RightMargin", _rightMargin);
			}
			if (_rightToLeft != RightToLeft.Inherit)
			{
				info.AddValue("RightToLeft", _rightToLeft);
			}
			if (_calendarNoneButtonVisible != TriState.Empty)
			{
				info.AddValue("CalendarNoneButtonVisible", _calendarNoneButtonVisible);
			}
			if (_calendarTodayButtonVisible != TriState.Empty)
			{
				info.AddValue("CalendarTodayButtonVisible", _calendarTodayButtonVisible);
			}
			if (!_selectable)
			{
				info.AddValue("Selectable", _selectable);
			}
			if (_gridColumnSelectableCells != (SelectableCells.RowCells | SelectableCells.NewRowCells | SelectableCells.FilterRowCells))
			{
				info.AddValue("SelectableCells", _gridColumnSelectableCells);
			}
			if (!_showInFieldChooser)
			{
				info.AddValue("ShowInFieldChooser", _showInFieldChooser);
			}
			if (_textAlignment != TextAlignment.Empty)
			{
				info.AddValue("TextAlignment", _textAlignment);
			}
			if (_topMargin != 1)
			{
				info.AddValue("TopMargin", _topMargin);
			}
			if (!_visible)
			{
				info.AddValue("Visible", _visible);
			}
			if (_width != 100)
			{
				info.AddValue("Width", _width);
			}
			if (_wordWrap)
			{
				info.AddValue("WordWrap", _wordWrap);
			}
			if (FormatMode != FormatMode.UseIFormattable)
			{
				info.AddValue("FormatMode", FormatMode);
			}
			if (_totalFormatMode != FormatMode.UseIFormattable)
			{
				info.AddValue("TotalFormatMode", _totalFormatMode);
			}
			if (_summaryFormat != string.Empty)
			{
				info.AddValue("TotalFormatString", _summaryFormat);
			}
			if (_trimming != Trimming.Ellipsis)
			{
				info.AddValue("Trimming", _trimming);
			}
			if (_ownerDrawnMode != ColumnOwnerDrawnMode.None)
			{
				info.AddValue("OwnerDrawnMode", _ownerDrawnMode);
			}
			if (_upDownIncrement != 1)
			{
				info.AddValue("UpDownIncrement", _upDownIncrement);
			}
			if (_upDownMaximum != 100)
			{
				info.AddValue("UpDownMaximum", _upDownMaximum);
			}
			if (_upDownMinimum != 0)
			{
				info.AddValue("UpDownMinimum", _upDownMinimum);
			}
			if (!_limitToList)
			{
				info.AddValue("LimitToList", _limitToList);
			}
			if (_actAsSelector)
			{
				info.AddValue("ActAsSelector", _actAsSelector);
			}
			if (!_useHeaderSelector)
			{
				info.AddValue("UseHeaderSelector", _useHeaderSelector);
			}
			if (_tag is string)
			{
				info.AddValue("Tag", _tag);
			}
			if (ShouldSerializeFilterRowComparison())
			{
				info.AddValue("FilterRowComparison", _filterRowComparison);
			}
			if (_filterRowButtonStyle != FilterRowButtonStyle.Default)
			{
				info.AddValue("FilterRowButtonStyle", _filterRowButtonStyle);
			}
			if (_defaultFilterRowComparison != FilterConditionOperator.Default)
			{
				info.AddValue("DefaultFilterRowComparison", _defaultFilterRowComparison);
			}
			if (_defaultValue == DBNull.Value)
			{
				info.AddValue("DefaultValue", "{DBNULL}");
			}
			else if (_defaultValue != null)
			{
				info.AddValue("DefaultValue", _defaultValue);
				if (!(_defaultValue is string))
				{
					info.AddValue("DefaultValueType", _defaultValue.GetType().FullName);
				}
			}
			if (HasValueList)
			{
				info.AddValue("HasValueList", HasValueList);
			}
			if (ValueList != null && ValueList.Count > 0)
			{
				info.AddValue("ValueList", ValueList);
			}
			if (_customGroup != null)
			{
				info.AddValue("CustomGroupIndex", _customGroup.Index);
			}
			if (_cellStyle != null)
			{
				if (_cellStyle._owner == this)
				{
					if (!_cellStyle.IsDefaultStyle())
					{
						info.AddValue("CellStyle", _cellStyle);
					}
				}
				else if (DataGrid != null)
				{
					int value = DataGrid.FormatStyles.IndexOf(_cellStyle);
					info.AddValue("CellStyleIndex", value);
				}
			}
			if (_groupRowStyle != null)
			{
				if (_groupRowStyle._owner == this)
				{
					if (!_groupRowStyle.IsDefaultStyle())
					{
						info.AddValue("GroupRowStyle", _groupRowStyle);
					}
				}
				else if (DataGrid != null)
				{
					int otherIndexOf = DataGrid.FormatStyles.IndexOf(_groupRowStyle);
					info.AddValue("GroupRowStyleIndex", otherIndexOf);
				}
			}
			if (_headerStyle != null)
			{
				if (_headerStyle._owner == this)
				{
					if (!_headerStyle.IsDefaultStyle())
					{
						info.AddValue("HeaderStyle", _headerStyle);
					}
				}
				else if (DataGrid != null)
				{
					int nextIndexOf = DataGrid.FormatStyles.IndexOf(_headerStyle);
					info.AddValue("HeaderStyleIndex", nextIndexOf);
				}
			}
			if (_dropDown != null && DataGrid != null)
			{
				int previousIndexOf = DataGrid.DropDowns.IndexOf(_dropDown);
				info.AddValue("DropDownIndex", previousIndexOf);
			}
			if (_autoSizeMode != ColumnAutoSizeMode.Default)
			{
				info.AddValue("AutoSizeMode", _autoSizeMode);
			}
			if (!_autoComplete)
			{
				info.AddValue("AutoComplete", _autoComplete);
			}
			if (BoundMode != ColumnBoundMode.Bound && DataTypeCode != TypeCode.Object)
			{
				info.AddValue("DataTypeCode", DataTypeCode);
			}
			if (MultipleValues)
			{
				info.AddValue("MultipleValues", MultipleValues);
			}
			if (!string.IsNullOrEmpty(MultipleValueDataMember))
			{
				info.AddValue("MultipleValueDataMember", MultipleValueDataMember);
			}
			if (_showRowSelector)
			{
				info.AddValue("ShowRowSelector", _showRowSelector);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDefaultGroupFormatString()
		{
			return BaseShouldSerializeGroupFormatString();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDefaultGroupFormatMode()
		{
			return BaseShouldSerializeGroupFormatMode();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			return !_boundImageSize.IsEmpty;
		}

		public bool ShouldSerializeFilterRowComparison()
		{
			return _filterRowComparison.HasValue;
		}
	}
}
