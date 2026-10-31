using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class GridCell
	{
		private ColumnFormatStyleState _cellOverrides;

		private GridRow _row;

		private GridColumn _column;

		private object _cellValue;

		private bool _hasValue;

		private string _displayText = "";

		internal bool _isTextCached;

		private bool _hasOriginalValue;

		private object _gridCellValue;

		private bool _dataChanged;

		private string _errorMessage = "";

		private bool _hasErrorIcon;

		private Image _cellImage;

		private Image _gridCellImage;

		private bool _isImageCached;

		private Size _imageSize = Size.Empty;

		private GridFormatStyle _gridFormatStyle;

		internal object _groupHeaderTotal;

		private bool _enabled = true;

		private bool _buttonEnabled = true;

		public GridRow Row => _row;

		public string ToolTipText
		{
			get
			{
				if (_cellOverrides == null)
				{
					return null;
				}
				return _cellOverrides._toolTipText;
			}
			set
			{
				if (ToolTipText != value)
				{
					EnsureOverrides();
					_cellOverrides._toolTipText = value;
				}
			}
		}

		public GridColumn Column => _column;

		public object Value
		{
			get
			{
				if (_column.ActAsSelector)
				{
					if (_row.RowType == RowType.Record || _row.RowType == RowType.NewRecord)
					{
						return _row.IsChecked;
					}
					return null;
				}
				if (_hasValue || _column.BoundMode == ColumnBoundMode.UnboundFetch)
				{
					return _cellValue;
				}
				return _row.GetColumnValue(_column);
			}
			set
			{
				if (_column.BoundMode == ColumnBoundMode.Bound && !_row._isEditing && _row.DataGrid != null && _row.DataGrid.CurrentRow == _row)
				{
					_row.DataGrid.SetValue(_column, value);
					return;
				}
				if (_row.RowType == RowType.GroupHeader || _row.RowType == RowType.TotalRow || _row.RowType == RowType.GroupFooter)
				{
					if (!_row._isLayoutSuspended)
					{
						return;
					}
					AssignValue(value, false);
					if (Row.RowType == RowType.GroupHeader)
					{
						if (_groupHeaderTotal is GridGroupHeaderTotal gridGroupHeaderTotal)
						{
							Text = gridGroupHeaderTotal.FormatTotal(value);
						}
						else
						{
							Text = string.Concat(value);
						}
					}
					return;
				}
				if (!_row._isEditing && _row.DataGrid != null && _row.DataGrid.CurrentRow == _row)
				{
					_row.DataGrid.SetValue(_column, value);
					return;
				}
				if (!_row._isLoading && !_row._isEditing)
				{
					throw new InvalidOperationException("Value can not be set. Values in cells can be set only if they belong to the current row in the DataGrid control or a when an edit operation has been started with the BeginEdit method in the GridRow that owns the cell.");
				}
				if (_row.RowType == RowType.Record || _row.RowType == RowType.NewRecord)
				{
					if (_column.BoundMode != ColumnBoundMode.UnboundFetch && !_row._isEditing)
					{
						throw new InvalidOperationException();
					}
				}
				else if (_row.RowType == RowType.GroupHeader)
				{
					throw new InvalidOperationException();
				}
				if (_row._isEditing && _column.BoundMode != ColumnBoundMode.UnboundFetch && _row.DataRow != null)
				{
					_column.SetRowValue(_row.GetLogicalRow(), this, value);
					DataChanged = true;
					value = _row.GetColumnValue(_column);
				}
				_cellValue = value;
				_hasValue = true;
				_isTextCached = false;
				_isImageCached = false;
				if (_gridCellImage != null)
				{
					_cellImage = null;
				}
			}
		}

		public bool DataChanged
		{
			get
			{
				return _dataChanged;
			}
			set
			{
				if (_dataChanged == value)
				{
					return;
				}
				_dataChanged = value;
				if (_row != null)
				{
					if (_dataChanged)
					{
						_row.SetDataChanged(true);
					}
					else if (_column.BoundMode != ColumnBoundMode.UnboundFetch)
					{
						_hasValue = false;
						_isTextCached = false;
						_isImageCached = false;
					}
					else if (_hasOriginalValue && _hasValue)
					{
						_cellValue = _gridCellValue;
						_isTextCached = false;
						_isImageCached = false;
					}
				}
			}
		}

		public string Text
		{
			get
			{
				if (_cellOverrides != null && _cellOverrides._displayText != null)
				{
					return _cellOverrides._displayText;
				}
				if (!_isTextCached)
				{
					if ((_row != null && _row.RowType == RowType.TotalRow) || _row.RowType == RowType.GroupFooter)
					{
						_displayText = _column.GetTotalText(Value);
					}
					else
					{
						if (_row != null && _row.RowType == RowType.GroupHeader)
						{
							return "";
						}
						if (_row != null && _row.RowType == RowType.FilterRow)
						{
							if (GridHelper.IsNullOrDBNull(Value))
							{
								return "";
							}
							if (_column.CompareTarget == ColumnCompareTarget.Text)
							{
								return string.Concat(Value);
							}
							_displayText = _column.GetDisplayText(Value, true);
						}
						else if (_cellOverrides != null && _cellOverrides._valueList != null)
						{
							if (_cellOverrides._valueList.ReplaceValues)
							{
								if (_column.MultipleValues)
								{
									if (Value is Array array)
									{
										string text = "";
										for (int i = 0; i < array.Length; i++)
										{
											text += GetItemDisplayText(array.GetValue(i));
										}
										_displayText = text;
									}
									else
									{
										_displayText = GetItemDisplayText(Value);
									}
								}
								else
								{
									_displayText = GetItemDisplayText(Value);
								}
							}
							else
							{
								_column.GetDisplayText(Value, true);
							}
						}
						else
						{
							_displayText = _column.GetDisplayText(Value, true);
						}
					}
					_isTextCached = true;
					return _displayText;
				}
				return _displayText;
			}
			set
			{
				if (value != Text || !_isTextCached)
				{
					EnsureOverrides();
					_cellOverrides._displayText = value;
					_displayText = value;
					_isTextCached = true;
					NotifyRowChanged(false);
				}
			}
		}

		public string ImageKey
		{
			get
			{
				if (_cellOverrides != null && !string.IsNullOrEmpty(_cellOverrides._imageKey))
				{
					return _cellOverrides._imageKey;
				}
				if (_column.ValueList != null && _column.ValueList.ReplaceValues)
				{
					object value = Value;
					GridValueListItem gridValueListItem = _column.ValueList[value];
					if (gridValueListItem == null)
					{
						return _column.ImageKey;
					}
					return gridValueListItem.ImageKey;
				}
				if (_row.RowType != RowType.GroupFooter && _row.RowType != RowType.TotalRow)
				{
					return _column.ImageKey;
				}
				return "";
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (ImageKey != value)
				{
					EnsureOverrides();
					_cellOverrides._imageKey = value;
					if (value.Length > 0)
					{
						_cellOverrides._imageIndex = -1;
					}
					_row.RequestLayout(false);
				}
			}
		}

		public int ImageIndex
		{
			get
			{
				if (_cellOverrides != null && (_cellOverrides._imageIndex ?? (-1)) != -1)
				{
					return _cellOverrides._imageIndex ?? (-1);
				}
				if (_column.ValueList != null && _column.ValueList.ReplaceValues)
				{
					object value = Value;
					return _column.ValueList[value]?.ImageIndex ?? _column.ImageIndex;
				}
				if (_row.RowType != RowType.GroupFooter && _row.RowType != RowType.TotalRow)
				{
					return _column.ImageIndex;
				}
				return -1;
			}
			set
			{
				if (value != ImageIndex)
				{
					EnsureOverrides();
					_cellOverrides._imageIndex = value;
					if (value != -1)
					{
						_cellOverrides._imageKey = "";
					}
					_row.RequestLayout(false);
				}
			}
		}

		public Image Image
		{
			get
			{
				if (_cellOverrides != null && _cellOverrides.HasImage())
				{
					return _cellOverrides.GetColumnImage();
				}
				Image image = _cellImage;
				if (image == null && !_isImageCached)
				{
					if (_row.RowType == RowType.Record && _row.DataRow == null)
					{
						return null;
					}
					_gridCellImage = null;
					if (_column.ValueList != null && _column.ValueList.ReplaceValues)
					{
						object value = Value;
						GridValueListItem gridValueListItem = _column.ValueList[value];
						if (gridValueListItem == null)
						{
							_gridCellImage = _column.Image;
						}
						else
						{
							_gridCellImage = gridValueListItem.Image;
						}
					}
					else if (_column.DropDown != null && _column.DropDown.ReplaceValues && _column.DropDown.ImageColumn != null)
					{
						object otherValue = Value;
						_gridCellImage = _column.DropDown.GetImageForValue(otherValue);
					}
					_isImageCached = true;
					_cellImage = _gridCellImage;
					image = _gridCellImage;
				}
				if (image == null)
				{
					if (_row.RowType == RowType.Record)
					{
						image = _column.Image;
					}
					else if (_row.RowType == RowType.NewRecord && _row.DataGrid != null && _row == _row.DataGrid.CurrentRow)
					{
						image = _column.Image;
					}
				}
				return image;
			}
			set
			{
				if (value != Image)
				{
					EnsureOverrides();
					_cellOverrides.SetColumnImage(value);
					_row.RequestLayout(true);
				}
			}
		}

		public Size ImageSize
		{
			get
			{
				return _imageSize;
			}
			set
			{
				if (value != _imageSize)
				{
					_imageSize = value;
				}
			}
		}

		public GridFormatStyle FormatStyle
		{
			get
			{
				if (_cellOverrides == null)
				{
					return null;
				}
				return _cellOverrides._columnStyle;
			}
			set
			{
				if (FormatStyle != value)
				{
					EnsureOverrides();
					_cellOverrides._columnStyle = value;
				}
			}
		}

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		public ColumnType CellDisplayType
		{
			get
			{
				if (_cellOverrides == null || !_cellOverrides._cellDisplayType.HasValue)
				{
					if (Column != null)
					{
						return Column.ColumnType;
					}
					return ColumnType.Text;
				}
				return _cellOverrides._cellDisplayType.Value;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ColumnType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				EnsureOverrides();
				if (!_cellOverrides._cellDisplayType.HasValue || _cellOverrides._cellDisplayType.Value != value)
				{
					_cellOverrides._cellDisplayType = value;
					NotifyRowChanged(false);
				}
			}
		}

		public EditType EditType
		{
			get
			{
				if (_cellOverrides == null || !_cellOverrides._editType.HasValue)
				{
					if (Column != null)
					{
						return Column.EditType;
					}
					return EditType.NoEdit;
				}
				return _cellOverrides._editType.Value;
			}
			set
			{
				if (!Enum.IsDefined(typeof(EditType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				EnsureOverrides();
				if (!_cellOverrides._editType.HasValue || _cellOverrides._editType.Value != value)
				{
					_cellOverrides._editType = value;
					NotifyRowChanged(false);
				}
			}
		}

		public ButtonStyle ButtonStyle
		{
			get
			{
				if (_cellOverrides == null || !_cellOverrides._buttonStyle.HasValue)
				{
					if (Column != null)
					{
						return Column.ButtonStyle;
					}
					return ButtonStyle.NoButton;
				}
				return _cellOverrides._buttonStyle.Value;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ButtonStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				EnsureOverrides();
				if (!_cellOverrides._buttonStyle.HasValue || _cellOverrides._buttonStyle.Value != value)
				{
					_cellOverrides._buttonStyle = value;
					NotifyRowChanged(false);
				}
			}
		}

		public GridValueListItemCollection ValueList
		{
			get
			{
				if (_cellOverrides == null || _cellOverrides._valueList == null)
				{
					if (Column != null)
					{
						return Column.ValueList;
					}
					return null;
				}
				return _cellOverrides._valueList;
			}
			set
			{
				EnsureOverrides();
				if (_cellOverrides._valueList != value)
				{
					_cellOverrides._valueList = value;
					NotifyRowChanged(false);
				}
			}
		}

		public bool ButtonEnabled
		{
			get
			{
				return _buttonEnabled;
			}
			set
			{
				if (_buttonEnabled != value)
				{
					_buttonEnabled = value;
					NotifyRowChanged(false);
				}
			}
		}

		public bool Selected
		{
			get
			{
				return RowStateHelper.IsCellSelected(this);
			}
			set
			{
				if (value == Selected || _row == null)
				{
					return;
				}
				MxDataGrid dataGrid = _row.DataGrid;
				if (dataGrid != null)
				{
					try
					{
						dataGrid.SetCellSelected(this, value);
					}
					catch (InvalidOperationException)
					{
						throw new InvalidOperationException("Selected property can only be set when SelectionMode in DataGrid is MultipleCellSelection");
					}
				}
			}
		}

		internal GridCell(GridRow row, GridColumn column)
		{
			_row = row;
			_column = column;
			_cellOverrides = row.GetColumnFormatState(column, false);
		}

		internal void RefreshErrors()
		{
			if (_column != null && _column.DataGrid != null && _column.DataGrid.ShowErrors)
			{
				_column.DataGrid.UpdateCellError(this);
			}
		}

		internal string GetEffectiveToolTipText()
		{
			if (ToolTipText != null)
			{
				return ToolTipText;
			}
			if (Column != null)
			{
				return Column.GetEffectiveCellToolTip();
			}
			return "";
		}

		internal void Detach()
		{
			_row = null;
			_column = null;
			_cellValue = null;
			_displayText = "";
			_cellOverrides = null;
			_gridFormatStyle = null;
			_cellImage = null;
		}

		internal void AssignValue(object value, bool condition)
		{
			if (_column.ActAsSelector && _row.RowType == RowType.Record)
			{
				if (value is bool)
				{
					_row.IsChecked = (bool)value;
					DataChanged = true;
				}
			}
			else if (_cellValue != value || !_hasValue)
			{
				if (!_hasOriginalValue)
				{
					_gridCellValue = _cellValue;
					_hasOriginalValue = true;
				}
				_cellValue = value;
				_hasValue = true;
				_isTextCached = false;
				_isImageCached = false;
				if (_gridCellImage == _cellImage)
				{
					_cellImage = null;
				}
				if (_cellOverrides != null)
				{
					_cellOverrides._displayText = null;
				}
				if (condition)
				{
					DataChanged = true;
				}
			}
		}

		internal void ResetValueState()
		{
			_hasValue = false;
			_isTextCached = false;
			_isImageCached = false;
			_gridCellValue = null;
			_hasOriginalValue = false;
			_dataChanged = false;
		}

		internal void ClearValueAndDisplayCache()
		{
			_hasValue = false;
			_isTextCached = false;
			_isImageCached = false;
			if (_cellOverrides != null)
			{
				_cellOverrides._displayText = null;
			}
		}

		internal void InvalidateDisplayCache()
		{
			if (_cellOverrides != null && _cellOverrides._displayText != null)
			{
				_isTextCached = false;
				_isImageCached = false;
			}
		}

		internal string GetErrorText()
		{
			return _errorMessage;
		}

		internal void SetErrorText(string gridCellText)
		{
			_errorMessage = gridCellText;
		}

		internal void SetDataChanged(bool dataChanged)
		{
			_dataChanged = dataChanged;
		}

		private string GetItemDisplayText(object item)
		{
			GridValueListItem gridValueListItem = _cellOverrides._valueList[item];
			if (gridValueListItem != null)
			{
				return gridValueListItem.Text;
			}
			return string.Concat(item);
		}

		internal bool HasErrorIcon()
		{
			return _hasErrorIcon;
		}

		internal void SetHasErrorIcon(bool hasErrorIcon)
		{
			_hasErrorIcon = hasErrorIcon;
		}

		internal Image ResolveImage(ImageList imageList)
		{
			Image image = null;
			image = Image;
			if (image == null && imageList != null)
			{
				string imageKey = ImageKey;
				if (imageKey.Length > 0 && imageList.Images.ContainsKey(imageKey))
				{
					return imageList.Images[imageKey];
				}
				int imageIndex = ImageIndex;
				if (imageIndex >= 0 && imageIndex < imageList.Images.Count)
				{
					image = imageList.Images[imageIndex];
				}
			}
			return image;
		}

		internal bool HasImageReference()
		{
			if (ImageKey.Length > 0)
			{
				return true;
			}
			if (ImageIndex != -1)
			{
				return true;
			}
			return false;
		}

		internal GridFormatStyle GetEffectiveFormatStyle()
		{
			return _gridFormatStyle;
		}

		internal void DisposeEffectiveFormatStyle()
		{
			if (_gridFormatStyle != null)
			{
				_gridFormatStyle.DisposeResources();
				_gridFormatStyle = null;
			}
		}

		internal void MergeFormatStyle(GridFormatStyle formatStyle, bool condition)
		{
			if (_gridFormatStyle == null || !condition)
			{
				_gridFormatStyle = new GridFormatStyle(formatStyle);
				return;
			}
			GridFormatStyle gridFormatStyle = _gridFormatStyle;
			_gridFormatStyle = _gridFormatStyle.ApplyOverrides(formatStyle);
			if (gridFormatStyle != _gridFormatStyle)
			{
				gridFormatStyle.DisposeResources();
			}
		}

		private void EnsureOverrides()
		{
			if (_cellOverrides == null && _row != null)
			{
				_cellOverrides = _row.GetColumnFormatState(_column, true);
			}
		}

		private void NotifyRowChanged(bool condition)
		{
			if (_row != null)
			{
				_row.RequestLayout(condition);
			}
		}

		public void ResetCellDisplayType()
		{
			if (_cellOverrides != null)
			{
				_cellOverrides._cellDisplayType = null;
			}
		}

		public void ResetEditType()
		{
			if (_cellOverrides != null)
			{
				_cellOverrides._editType = null;
			}
		}

		public void ResetButtonStyle()
		{
			if (_cellOverrides != null)
			{
				_cellOverrides._buttonStyle = null;
			}
		}
	}
}
