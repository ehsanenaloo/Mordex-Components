using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class ValueListCellEditor : CellEditor
	{
		private EditType _gridState8EditType;

		public override bool HandlesMouseWheel => GetValueListControl().IsDropListOpen();

		public override GridValueListItemCollection EditValueList
		{
			get
			{
				if (GetValueListControl() != null)
				{
					return GetValueListControl().GetValueListItems();
				}
				return null;
			}
			set
			{
				if (GetValueListControl() != null)
				{
					GetValueListControl().AssignValueListItems(value);
				}
			}
		}

		public override EditType EditType
		{
			get
			{
				return _gridState8EditType;
			}
			set
			{
				_gridState8EditType = value;
				switch (_gridState8EditType)
				{
				case EditType.NoEdit:
					GetValueListControl().SetEditStyle(EditStyle.None);
					break;
				case EditType.TextBox:
					GetValueListControl().SetEditStyle(EditStyle.Text);
					break;
				case EditType.Combo:
					GetValueListControl().SetEditStyle(EditStyle.Combo);
					break;
				case EditType.DropDownList:
					GetValueListControl().SetEditStyle(EditStyle.DropDownList);
					break;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public override Control DropControl => GetValueListControl().GetDropControl();

		public override object Value
		{
			get
			{
				return GetValueListControl().GetCurrentValue();
			}
			set
			{
				GetValueListControl().SetCurrentValue(value);
			}
		}

		public override bool NotInList => GetValueListControl().IsTextNotInList();

		public override TextBox EditTextBox => GetValueListControl()._textBox;

		public override bool DroppedDown
		{
			get
			{
				return GetValueListControl().GetIsDroppedDown();
			}
			set
			{
				((ControlStyleContainerControl)GetValueListControl()).SetIsDroppedDown(value);
			}
		}

		internal ValueListCellEditor(GridColumnContainerControl editContainer)
			: base(editContainer)
		{
		}

		private GridValueListItemCollectionState GetValueListControl()
		{
			return (GridValueListItemCollectionState)EditControl;
		}

		protected override Control CreateEditControl()
		{
			GridValueListItemCollectionState gridValueListItemCollectionState = new GridValueListItemCollectionState();
			gridValueListItemCollectionState.AccessibleName = "CellEdit";
			if (GetEditContainer() != null)
			{
				gridValueListItemCollectionState.SetUseCompatibleTextRendering(GetEditContainer().IsCustomListLayout());
			}
			gridValueListItemCollectionState.SetControlStyle(GetControlStyle());
			gridValueListItemCollectionState.AddChangeHandler(OnEditorChanged);
			gridValueListItemCollectionState.AddDropHandler(OnEditorDroppingList);
			gridValueListItemCollectionState.AddDropDownHideCallback(OnEditorDropDownHide);
			gridValueListItemCollectionState.AddSelectedItemChangedHandler(OnSelectedItemEdited);
			return gridValueListItemCollectionState;
		}

		private void OnSelectedItemEdited(object sender, EventArgs eventArgs)
		{
			OnChange(EventArgs.Empty);
		}

		public override void StartEdit(MxDataGrid grid, GridColumn column, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control control)
		{
			base.StartEdit(grid, column, cell, style, value, image, capture, control);
			GridValueListItemCollectionState gridValueListItemCollectionState = GetValueListControl();
			gridValueListItemCollectionState.Enabled = cell.Enabled;
			gridValueListItemCollectionState.SetPasswordChar(column.PasswordChar);
			if (column.TextAlignment != TextAlignment.Empty)
			{
				gridValueListItemCollectionState.UpdateTextAlignment(column.TextAlignment);
			}
			else
			{
				gridValueListItemCollectionState.UpdateTextAlignment(style.TextAlignment);
			}
			gridValueListItemCollectionState.UpdateTextLayoutRenderer(column.ComputeTextLayoutRenderer(style, !grid.UseCompatibleTextRendering));
			gridValueListItemCollectionState.SetAutoCompleteEnabled(column.AutoComplete);
			gridValueListItemCollectionState.UpdateImageHorizontalAlignment(style.ImageHorizontalAlignment);
			gridValueListItemCollectionState.UpdateImageVerticalAlignment(style.ImageVerticalAlignment);
			gridValueListItemCollectionState._gridFormatStyle = grid.RowFormatStyle.ApplyOverrides(grid.SelectedFormatStyle);
			gridValueListItemCollectionState.SetEditFormat(column.InputMask);
			gridValueListItemCollectionState.UpdateCharacter(column.MaskPrompt);
			gridValueListItemCollectionState.SetIncludeLiterals(column.IncludeLiterals);
			gridValueListItemCollectionState.SetImage(image);
			gridValueListItemCollectionState.SetImageList(grid.ComputeImageList());
			if (column.EditValueList != null)
			{
				gridValueListItemCollectionState.AssignValueListItems(column.EditValueList);
			}
			else
			{
				gridValueListItemCollectionState.AssignValueListItems(cell.ValueList);
			}
			if (cell.CellDisplayType == ColumnType.Image || cell.CellDisplayType == ColumnType.ImageAndText)
			{
				gridValueListItemCollectionState.SetIsImageSet(true);
			}
			else
			{
				gridValueListItemCollectionState.SetIsImageSet(false);
			}
			gridValueListItemCollectionState.SetUseInnerEditor(cell.CellDisplayType == ColumnType.Text || cell.CellDisplayType == ColumnType.ImageAndText || cell.CellDisplayType == ColumnType.Link);
			gridValueListItemCollectionState.SetMultiline(column.WordWrap);
			gridValueListItemCollectionState.SetTextAlignment(column.LineAlignment);
			gridValueListItemCollectionState.SetMargins(column.LeftMargin, column.TopMargin, column.RightMargin, column.BottomMargin);
			gridValueListItemCollectionState.BackColor = GridColumnContainerControl.ResolveEditBackColor(grid, style);
			gridValueListItemCollectionState.ForeColor = GridColumnContainerControl.ResolveEditForeColor(grid, style);
			gridValueListItemCollectionState.Font = GridColumnContainerControl.ResolveEditFont(grid, style);
			gridValueListItemCollectionState.SetDropListFont(grid.Font);
			gridValueListItemCollectionState.SetMaxLength(column.MaxLength);
			gridValueListItemCollectionState.UpdateCharacterCasing(column.CharacterCasing);
			gridValueListItemCollectionState.ProcessType(GetColumn().Type);
			gridValueListItemCollectionState.LoadEditValue(value);
		}

		public override void OnDropDownButtonMouseDown()
		{
			GetValueListControl().PressDropDownButton();
		}

		public override void ShowDropList()
		{
			((ControlStyleContainerControl)GetValueListControl()).SetIsDroppedDown(true);
		}

		public override void InitEdit(char editChar)
		{
			GetValueListControl().ProcessCharacter(editChar);
		}

		protected override void OnControlStyleChanged()
		{
			if (GetValueListControl() != null)
			{
				GetValueListControl().SetControlStyle(GetControlStyle());
			}
		}

		public override void SetComboVirtualWidth(int virtualWidth)
		{
			GetValueListControl()._virtualWidth = virtualWidth;
		}

		public override void SetTextValue(string value)
		{
			GetValueListControl().LoadEditText(value);
		}

		public override MaskEdit GetMaskEdit()
		{
			return GetValueListControl().AsMaskEdit();
		}

		public override void ReevaluateValue()
		{
			GetValueListControl().SyncSelectedItemFromText();
		}

		public override bool DoMouseWheel(int delta)
		{
			return GetValueListControl().InvokeMouseWheel(delta);
		}

		public override void Destroy()
		{
			GridValueListItemCollectionState gridValueListItemCollectionState = GetValueListControl();
			if (gridValueListItemCollectionState != null)
			{
				gridValueListItemCollectionState.RemoveChangeHandler(OnEditorChanged);
				gridValueListItemCollectionState.RemoveDropHandler(OnEditorDroppingList);
				gridValueListItemCollectionState.RemoveDropDownHideCallback(OnEditorDropDownHide);
				gridValueListItemCollectionState.RemoveSelectedItemChangedHandler(OnSelectedItemEdited);
			}
			base.Destroy();
		}

		private void OnEditorChanged(object sender, EventArgs eventArgs)
		{
			OnChange(eventArgs);
		}

		private void OnEditorDroppingList(object sender, EventArgs eventArgs)
		{
			OnDroppingList(eventArgs);
		}

		private void OnEditorDropDownHide(object value, GridEventArgs gridEventArgs)
		{
			OnDropDownHide(gridEventArgs);
		}
	}
}
