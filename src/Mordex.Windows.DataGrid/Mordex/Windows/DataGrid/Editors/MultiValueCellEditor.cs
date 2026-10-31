using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class MultiValueCellEditor : CellEditor
	{
		private EditType _gridState4EditType;

		public override EditType EditType
		{
			get
			{
				return _gridState4EditType;
			}
			set
			{
				_gridState4EditType = value;
				switch (_gridState4EditType)
				{
				case EditType.Combo:
				case EditType.MultiColumnCombo:
					ComputeVisualStyleManagerState().SetEditStyle(EditStyle.Combo);
					break;
				case EditType.DropDownList:
				case EditType.MultiColumnDropDown:
					ComputeVisualStyleManagerState().SetEditStyle(EditStyle.DropDownList);
					break;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public override Control DropControl => ComputeVisualStyleManagerState().GetDropControl();

		public override object Value
		{
			get
			{
				return ComputeVisualStyleManagerState().GetVisualStyleManagerStateValue();
			}
			set
			{
				ComputeVisualStyleManagerState().SyncValueAndDisplayText(value);
			}
		}

		public override TextBox EditTextBox => ComputeVisualStyleManagerState()._textBox;

		public override bool DroppedDown
		{
			get
			{
				return ((ControlStyleContainerControl)ComputeVisualStyleManagerState()).GetIsDroppedDown();
			}
			set
			{
				ComputeVisualStyleManagerState().SetIsDroppedDown(value);
			}
		}

		public override bool NotInList => ComputeVisualStyleManagerState().HasItemsNotInList();

		internal MultiValueCellEditor(GridColumnContainerControl editContainer)
			: base(editContainer)
		{
		}

		private VisualStyleManagerState ComputeVisualStyleManagerState()
		{
			return (VisualStyleManagerState)EditControl;
		}

		protected override Control CreateEditControl()
		{
			VisualStyleManagerState visualStyleManagerState = new VisualStyleManagerState();
			visualStyleManagerState.SetControlStyle(GetControlStyle());
			visualStyleManagerState.AddChangeHandler(OnEditorChanged);
			visualStyleManagerState.AddDropHandler(OnEditorDroppingList);
			visualStyleManagerState.AddDropDownHideCallback(OnEditorDropDownHide);
			return visualStyleManagerState;
		}

		public override void StartEdit(MxDataGrid grid, GridColumn column, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control control)
		{
			base.StartEdit(grid, column, cell, style, value, image, capture, control);
			VisualStyleManagerState visualStyleManagerState = ComputeVisualStyleManagerState();
			visualStyleManagerState.UpdateVisualStyleManagerStateText(grid.BuiltInTexts[GridBuiltInText.DropDownOkButton]);
			visualStyleManagerState.SetCancelButtonText(grid.BuiltInTexts[GridBuiltInText.DropDownCancelButton]);
			visualStyleManagerState.SetImageList(grid.ComputeImageList());
			if (cell.EditType == EditType.MultiColumnCombo || cell.EditType == EditType.MultiColumnDropDown)
			{
				visualStyleManagerState.ProcessILookUpTable(column.DropDown);
			}
			else if (column.EditValueList != null)
			{
				visualStyleManagerState.ProcessILookUpTable(column.EditValueList);
			}
			else
			{
				visualStyleManagerState.ProcessILookUpTable(column.ValueList);
			}
			if (column.TextAlignment != TextAlignment.Empty)
			{
				visualStyleManagerState.UpdateTextAlignment(column.TextAlignment);
			}
			else
			{
				visualStyleManagerState.UpdateTextAlignment(style.TextAlignment);
			}
			visualStyleManagerState.SetControlStyle(GetControlStyle());
			visualStyleManagerState.UpdateCharacter(column.MaskPrompt);
			visualStyleManagerState.SetIncludeLiterals(column.IncludeLiterals);
			visualStyleManagerState.SetImage(image);
			visualStyleManagerState.SetImageList(grid.ComputeImageList());
			visualStyleManagerState.UpdateTextLayoutRenderer(column.ComputeTextLayoutRenderer(style, !grid.UseCompatibleTextRendering));
			visualStyleManagerState.UpdateImageHorizontalAlignment(style.ImageHorizontalAlignment);
			visualStyleManagerState.UpdateImageVerticalAlignment(style.ImageVerticalAlignment);
			if (visualStyleManagerState.GetEditStyle() == EditStyle.Combo)
			{
				visualStyleManagerState.SetIsImageSet(false);
			}
			else
			{
				visualStyleManagerState.SetIsImageSet(true);
			}
			visualStyleManagerState.SetMultiline(column.WordWrap);
			visualStyleManagerState.SetTextAlignment(column.LineAlignment);
			visualStyleManagerState.SetMargins(column.LeftMargin, column.TopMargin - 1, column.RightMargin, column.BottomMargin - 1);
			visualStyleManagerState.InitializeValue(value);
			visualStyleManagerState.BackColor = GridColumnContainerControl.ResolveEditBackColor(grid, style);
			visualStyleManagerState.ForeColor = GridColumnContainerControl.ResolveEditForeColor(grid, style);
			visualStyleManagerState.Font = GridColumnContainerControl.ResolveEditFont(grid, style);
			visualStyleManagerState.SetMaxLength(column.MaxLength);
			visualStyleManagerState.UpdateCharacterCasing(column.CharacterCasing);
		}

		public override void OnDropDownButtonMouseDown()
		{
			ComputeVisualStyleManagerState().PressDropDownButton();
		}

		public override void ShowDropList()
		{
			ComputeVisualStyleManagerState().SetIsDroppedDown(true);
		}

		public override void InitEdit(char editChar)
		{
			ComputeVisualStyleManagerState().ProcessCharacter(editChar);
		}

		protected override void OnControlStyleChanged()
		{
			if (ComputeVisualStyleManagerState() != null)
			{
				ComputeVisualStyleManagerState().SetControlStyle(GetControlStyle());
			}
		}

		public override void SetComboVirtualWidth(int virtualWidth)
		{
			ComputeVisualStyleManagerState()._virtualWidth = virtualWidth;
		}

		public override MaskEdit GetMaskEdit()
		{
			return ComputeVisualStyleManagerState().AsMaskEdit();
		}

		public override void ReevaluateValue()
		{
			ComputeVisualStyleManagerState().UpdateVisualStyleManagerStateValue();
		}

		public override bool DoMouseWheel(int delta)
		{
			return ComputeVisualStyleManagerState().InvokeMouseWheel(delta);
		}

		public override void Destroy()
		{
			VisualStyleManagerState visualStyleManagerState = ComputeVisualStyleManagerState();
			if (visualStyleManagerState != null)
			{
				visualStyleManagerState.RemoveChangeHandler(OnEditorChanged);
				visualStyleManagerState.RemoveDropHandler(OnEditorDroppingList);
				visualStyleManagerState.RemoveDropDownHideCallback(OnEditorDropDownHide);
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
