using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class MultiColumnComboCellEditor : CellEditor
	{
		private EditType _gridState5EditType;

		public override GridDropDown EditDropDown
		{
			get
			{
				if (GetDropDownControl() != null)
				{
					return GetDropDownControl().GetDropDownList();
				}
				return null;
			}
			set
			{
				GetDropDownControl().SetDropDownList(value);
			}
		}

		public override EditType EditType
		{
			get
			{
				return _gridState5EditType;
			}
			set
			{
				_gridState5EditType = value;
				switch (_gridState5EditType)
				{
				case EditType.TextBox:
					GetDropDownControl().SetEditStyle(EditStyle.Text);
					break;
				case EditType.MultiColumnCombo:
					GetDropDownControl().SetEditStyle(EditStyle.Combo);
					break;
				case EditType.MultiColumnDropDown:
					GetDropDownControl().SetEditStyle(EditStyle.DropDownList);
					break;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public override Control DropControl => GetDropDownControl().GetDropControl();

		public override object Value
		{
			get
			{
				return GetDropDownControl().GetCurrentValue();
			}
			set
			{
				GetDropDownControl().SetCurrentValue(value);
			}
		}

		public override bool NotInList => GetDropDownControl().IsTextNotInList();

		public override TextBox EditTextBox => GetDropDownControl()._textBox;

		public override bool DroppedDown
		{
			get
			{
				return GetDropDownControl().GetIsDroppedDown();
			}
			set
			{
				GetDropDownControl().SetIsDroppedDown(value);
			}
		}

		public override bool HandlesMouseWheel => GetDropDownControl().IsDropListOpen();

		internal MultiColumnComboCellEditor(GridColumnContainerControl editContainer)
			: base(editContainer)
		{
		}

		private GridDropDownState GetDropDownControl()
		{
			return (GridDropDownState)EditControl;
		}

		protected override Control CreateEditControl()
		{
			GridDropDownState gridDropDownState = new GridDropDownState(null);
			gridDropDownState.SetControlStyle(GetControlStyle());
			gridDropDownState.AddChangeHandler(OnEditorChanged);
			gridDropDownState.AddDropHandler(OnEditorDroppingList);
			gridDropDownState.AddDropDownHideCallback(OnEditorDropDownHide);
			return gridDropDownState;
		}

		public override void StartEdit(MxDataGrid grid, GridColumn column, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control control)
		{
			base.StartEdit(grid, column, cell, style, value, image, capture, control);
			GridDropDownState gridDropDownState = GetDropDownControl();
			gridDropDownState.SetDropDownList(column.DropDown);
			if (column.TextAlignment != TextAlignment.Empty)
			{
				gridDropDownState.UpdateTextAlignment(column.TextAlignment);
			}
			else
			{
				gridDropDownState.UpdateTextAlignment(style.TextAlignment);
			}
			gridDropDownState.SetAutoCompleteEnabled(column.AutoComplete);
			gridDropDownState.SetEditFormat(column.InputMask);
			gridDropDownState.SetControlStyle(GetControlStyle());
			gridDropDownState.UpdateCharacter(column.MaskPrompt);
			gridDropDownState.SetIncludeLiterals(column.IncludeLiterals);
			gridDropDownState.SetImage(image);
			gridDropDownState.SetImageList(grid.ComputeImageList());
			gridDropDownState.UpdateTextLayoutRenderer(column.ComputeTextLayoutRenderer(style, !grid.UseCompatibleTextRendering));
			gridDropDownState.UpdateImageHorizontalAlignment(style.ImageHorizontalAlignment);
			gridDropDownState.UpdateImageVerticalAlignment(style.ImageVerticalAlignment);
			if (cell.CellDisplayType == ColumnType.Image || cell.CellDisplayType == ColumnType.ImageAndText)
			{
				gridDropDownState.SetIsImageSet(true);
			}
			else
			{
				gridDropDownState.SetIsImageSet(false);
			}
			gridDropDownState.SetUseInnerEditor(cell.CellDisplayType == ColumnType.Text || cell.CellDisplayType == ColumnType.ImageAndText || cell.CellDisplayType == ColumnType.Link);
			gridDropDownState.SetMultiline(column.WordWrap);
			gridDropDownState.SetTextAlignment(column.LineAlignment);
			gridDropDownState.SetMargins(column.LeftMargin, column.TopMargin - 1, column.RightMargin, column.BottomMargin - 1);
			gridDropDownState.SetValueSilently(value);
			gridDropDownState.BackColor = GridColumnContainerControl.ResolveEditBackColor(grid, style);
			gridDropDownState.ForeColor = GridColumnContainerControl.ResolveEditForeColor(grid, style);
			gridDropDownState.Font = GridColumnContainerControl.ResolveEditFont(grid, style);
			gridDropDownState.SetMaxLength(column.MaxLength);
			gridDropDownState.UpdateCharacterCasing(column.CharacterCasing);
		}

		public override void OnDropDownButtonMouseDown()
		{
			GetDropDownControl().PressDropDownButton();
		}

		public override void ShowDropList()
		{
			GetDropDownControl().SetIsDroppedDown(true);
		}

		public override void InitEdit(char editChar)
		{
			GetDropDownControl().ProcessCharacter(editChar);
		}

		protected override void OnControlStyleChanged()
		{
			if (GetDropDownControl() != null)
			{
				GetDropDownControl().SetControlStyle(GetControlStyle());
			}
		}

		public override void SetComboVirtualWidth(int virtualWidth)
		{
			GetDropDownControl()._virtualWidth = virtualWidth;
		}

		public override void SetTextValue(string value)
		{
			GetDropDownControl().SetValueFromText(value);
		}

		public override MaskEdit GetMaskEdit()
		{
			return GetDropDownControl().AsMaskEdit();
		}

		public override void ReevaluateValue()
		{
			GetDropDownControl().SyncValueFromText();
		}

		public override bool DoMouseWheel(int delta)
		{
			return GetDropDownControl().InvokeMouseWheel(delta);
		}

		public override void Destroy()
		{
			GridDropDownState gridDropDownState = GetDropDownControl();
			if (gridDropDownState != null)
			{
				gridDropDownState.RemoveChangeHandler(OnEditorChanged);
				gridDropDownState.RemoveDropHandler(OnEditorDroppingList);
				gridDropDownState.RemoveDropDownHideCallback(OnEditorDropDownHide);
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
