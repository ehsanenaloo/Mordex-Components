using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class CheckBoxCellEditor : CellEditor
	{
		public override object Value
		{
			get
			{
				return GetCheckBoxControl().GetStoredValue();
			}
			set
			{
				GetCheckBoxControl().SetCheckState(value);
			}
		}

		public override EditType EditType
		{
			get
			{
				return EditType.CheckBox;
			}
			set
			{
				if (value != EditType.CheckBox)
				{
					throw new InvalidOperationException();
				}
			}
		}

		internal CheckBoxCellEditor(GridColumnContainerControl editContainer)
			: base(editContainer)
		{
		}

		private GridColumnControl GetCheckBoxControl()
		{
			return (GridColumnControl)EditControl;
		}

		public override void InitEdit(char editChar)
		{
			GetCheckBoxControl().ProcessCharacter(editChar);
		}

		public override void StartEdit(MxDataGrid grid, GridColumn column, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control control)
		{
			base.StartEdit(grid, column, cell, style, value, image, capture, control);
			GridColumnControl gridColumnControl = GetCheckBoxControl();
			gridColumnControl.Enabled = cell.Enabled;
			gridColumnControl.BackColor = GridColumnContainerControl.ResolveEditBackColor(grid, style);
			gridColumnControl.ForeColor = GridColumnContainerControl.ResolveEditForeColor(grid, style);
			gridColumnControl.SetCentered(grid.View == View.TableView);
			gridColumnControl.SetControlStyle(GetControlStyle());
			if (column.ActAsSelector && grid.RowCheckStateBehavior == RowCheckStateBehavior.CheckStateDependsOnChild)
			{
				gridColumnControl.SetIndeterminateValue(RowCheckState.Mixed);
				gridColumnControl.SetCheckedValue(RowCheckState.Checked);
				gridColumnControl.SetUncheckedValue(RowCheckState.Unchecked);
				gridColumnControl.SetThreeState(false);
				gridColumnControl._isRowSelector = true;
			}
			else
			{
				gridColumnControl.SetIndeterminateValue(column.CheckBoxNullValue);
				gridColumnControl.SetThreeState(column.CheckBoxTriState);
				gridColumnControl.SetCheckedValue(column.CheckBoxTrueValue);
				gridColumnControl.SetUncheckedValue(column.CheckBoxFalseValue);
				gridColumnControl._isRowSelector = false;
			}
			gridColumnControl.InitializeValue(value);
		}

		public override void ToggleCheckBoxValue()
		{
			GetCheckBoxControl().CycleValueIfEnabled();
		}

		public override void SetTextValue(string value)
		{
			GetCheckBoxControl().SetValueFromText(value);
		}

		protected override Control CreateEditControl()
		{
			if (GetColumn() != null)
			{
				GridColumnControl gridColumnControl = new GridColumnControl(GetColumn());
				gridColumnControl.AddValueChangedHandler(HandleEditorChanged);
				gridColumnControl.SetControlStyle(GetControlStyle());
				return gridColumnControl;
			}
			return null;
		}

		public override void Destroy()
		{
			if (GetCheckBoxControl() != null)
			{
				GetCheckBoxControl().RemoveValueChangedHandler(HandleEditorChanged);
			}
			base.Destroy();
		}

		private void HandleEditorChanged(object sender, EventArgs eventArgs)
		{
			OnChange(eventArgs);
		}

		protected override void OnControlStyleChanged()
		{
			if (GetCheckBoxControl() != null)
			{
				GetCheckBoxControl().SetControlStyle(GetControlStyle());
			}
		}
	}
}
