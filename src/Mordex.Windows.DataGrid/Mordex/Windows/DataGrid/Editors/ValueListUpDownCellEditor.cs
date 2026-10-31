using System;
using Mordex.Windows.DataGrid.Internal;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class ValueListUpDownCellEditor : CellEditor
	{
		private EditType _gridState6EditType;

		public override bool NotInList => ComputeValueListUpDown().NotInList;

		public override EditType EditType
		{
			get
			{
				return _gridState6EditType;
			}
			set
			{
				_gridState6EditType = value;
				switch (_gridState6EditType)
				{
				case EditType.ValueListUpDown:
					ComputeValueListUpDown().UpDownStyle = UpDownStyle.UpDownList;
					break;
				case EditType.ValueListUpDownCombo:
					ComputeValueListUpDown().UpDownStyle = UpDownStyle.UpDownCombo;
					break;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public override object Value
		{
			get
			{
				return ComputeValueListUpDown().Value;
			}
			set
			{
				ComputeValueListUpDown().Value = value;
			}
		}

		public override TextBox EditTextBox => ComputeValueListUpDown().TextBox;

		internal ValueListUpDownCellEditor(GridColumnContainerControl editContainer)
			: base(editContainer)
		{
		}

		private MxValueListUpDown ComputeValueListUpDown()
		{
			return (MxValueListUpDown)EditControl;
		}

		protected override Control CreateEditControl()
		{
			MxValueListUpDown valueListUpDown = new MxValueListUpDown();
			valueListUpDown.ValueChanged += OnControlValueChanged;
			return valueListUpDown;
		}

		public override void StartEdit(MxDataGrid grid, GridColumn column, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control control)
		{
			base.StartEdit(grid, column, cell, style, value, image, capture, control);
			MxValueListUpDown valueListUpDown = ComputeValueListUpDown();
			valueListUpDown.AutoSize = false;
			valueListUpDown.UpdateUpDownBaseBoolean(false);
			((EditBase)valueListUpDown).SetUseHighlightColor(false);
			valueListUpDown.AutoSize = false;
			valueListUpDown.BorderStyle = BorderStyle.None;
			if (column.TextAlignment != TextAlignment.Empty)
			{
				valueListUpDown.TextAlignment = column.TextAlignment;
			}
			else
			{
				valueListUpDown.TextAlignment = style.TextAlignment;
			}
			valueListUpDown.ImageHorizontalAlignment = style.ImageHorizontalAlignment;
			valueListUpDown.ImageVerticalAlignment = style.ImageVerticalAlignment;
			valueListUpDown.Image = image;
			valueListUpDown.ImageList = grid.ComputeImageList();
			if (column.EditValueList != null)
			{
				valueListUpDown.SetValueListItems(column.EditValueList);
			}
			else if (column.ValueList != null)
			{
				valueListUpDown.SetValueListItems(column.ValueList);
			}
			if (cell.CellDisplayType == ColumnType.Image || cell.CellDisplayType == ColumnType.ImageAndText)
			{
				valueListUpDown.HasImage = true;
			}
			else
			{
				valueListUpDown.HasImage = false;
			}
			valueListUpDown.HasText = cell.CellDisplayType == ColumnType.Text || cell.CellDisplayType == ColumnType.ImageAndText || cell.CellDisplayType == ColumnType.Link;
			try
			{
				valueListUpDown.Value = value;
				valueListUpDown.SetSuppressTextChange(false);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			valueListUpDown.BackColor = GridColumnContainerControl.ResolveEditBackColor(grid, style);
			valueListUpDown.ForeColor = GridColumnContainerControl.ResolveEditForeColor(grid, style);
			valueListUpDown.Font = GridColumnContainerControl.ResolveEditFont(grid, style);
			valueListUpDown.MaxLength = column.MaxLength;
			valueListUpDown.CharacterCasing = column.CharacterCasing;
		}

		public override void InitEdit(char editChar)
		{
			ComputeValueListUpDown().SetInitialCharacter(editChar);
		}

		public override void Destroy()
		{
			MxValueListUpDown valueListUpDown = ComputeValueListUpDown();
			if (valueListUpDown != null)
			{
				valueListUpDown.ValueChanged -= OnControlValueChanged;
			}
			base.Destroy();
		}

		private void OnControlValueChanged(object sender, EventArgs eventArgs)
		{
			OnChange(eventArgs);
		}

		public override void OnUpDownButtonClick(int button)
		{
			if (button == 1)
			{
				ComputeValueListUpDown().StartRepeatUp();
			}
			else
			{
				ComputeValueListUpDown().StartRepeatDown();
			}
		}

		public override void OnButtonMouseUp()
		{
			ComputeValueListUpDown().StopRepeat();
		}

		public override void ResumeUpDownRepeater()
		{
			ComputeValueListUpDown().ResumeRepeat();
		}

		public override void SuspendUpDownRepeater()
		{
			ComputeValueListUpDown().SuspendRepeat();
		}

		public override MaskEdit GetMaskEdit()
		{
			if (ComputeValueListUpDown() != null && ComputeValueListUpDown().TextBox is MaskEdit)
			{
				return (MaskEdit)ComputeValueListUpDown().TextBox;
			}
			return null;
		}
	}
}
