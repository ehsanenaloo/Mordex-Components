using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class IntegerUpDownCellEditor : CellEditor
	{
		private EditType _gridState7EditType;

		public override bool HandlesMouseWheel => GetUpDownControl().IsMouseWheelEnabled();

		public override EditType EditType
		{
			get
			{
				return _gridState7EditType;
			}
			set
			{
				_gridState7EditType = value;
				switch (_gridState7EditType)
				{
				case EditType.IntegerUpDown:
					GetUpDownControl().UpDownStyle = UpDownStyle.UpDownList;
					break;
				case EditType.IntegerUpDownCombo:
					GetUpDownControl().UpDownStyle = UpDownStyle.UpDownCombo;
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
				return GetUpDownControl().BindableValue;
			}
			set
			{
				GetUpDownControl().BindableValue = value;
			}
		}

		public override TextBox EditTextBox => GetUpDownControl().TextBox;

		internal IntegerUpDownCellEditor(GridColumnContainerControl editContainer)
			: base(editContainer)
		{
		}

		private MxIntegerUpDown GetUpDownControl()
		{
			return (MxIntegerUpDown)EditControl;
		}

		protected override Control CreateEditControl()
		{
			MxIntegerUpDown integerUpDown = new MxIntegerUpDown();
			integerUpDown.ValueChanged += OnEditorValueChanged;
			return integerUpDown;
		}

		public override void StartEdit(MxDataGrid grid, GridColumn column, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control control)
		{
			base.StartEdit(grid, column, cell, style, value, image, capture, control);
			MxIntegerUpDown integerUpDown = GetUpDownControl();
			integerUpDown.AutoSize = false;
			integerUpDown.UpdateUpDownBaseBoolean(false);
			((EditBase)integerUpDown).SetUseHighlightColor(false);
			integerUpDown.AutoSize = false;
			integerUpDown.BorderStyle = BorderStyle.None;
			if (column.TextAlignment != TextAlignment.Empty)
			{
				integerUpDown.TextAlignment = column.TextAlignment;
			}
			else
			{
				integerUpDown.TextAlignment = style.TextAlignment;
			}
			if (integerUpDown.Maximum < column.UpDownMinimum)
			{
				integerUpDown.Maximum = column.UpDownMaximum;
				integerUpDown.Minimum = column.UpDownMinimum;
			}
			else
			{
				integerUpDown.Minimum = column.UpDownMinimum;
				integerUpDown.Maximum = column.UpDownMaximum;
			}
			integerUpDown.Increment = column.UpDownIncrement;
			integerUpDown.NullBehavior = NumericEditNullBehavior.AllowNull;
			integerUpDown.ImageHorizontalAlignment = style.ImageHorizontalAlignment;
			integerUpDown.ImageVerticalAlignment = style.ImageVerticalAlignment;
			integerUpDown.Image = image;
			integerUpDown.ImageList = grid.ComputeImageList();
			if (cell.CellDisplayType == ColumnType.Image || cell.CellDisplayType == ColumnType.ImageAndText)
			{
				integerUpDown.HasImage = true;
			}
			else
			{
				integerUpDown.HasImage = false;
			}
			integerUpDown.HasText = cell.CellDisplayType == ColumnType.Text || cell.CellDisplayType == ColumnType.ImageAndText || cell.CellDisplayType == ColumnType.Link;
			try
			{
				integerUpDown.LoadEditValue(value);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			integerUpDown.BackColor = GridColumnContainerControl.ResolveEditBackColor(grid, style);
			integerUpDown.ForeColor = GridColumnContainerControl.ResolveEditForeColor(grid, style);
			integerUpDown.Font = GridColumnContainerControl.ResolveEditFont(grid, style);
			integerUpDown.MaxLength = column.MaxLength;
		}

		public override void InitEdit(char editChar)
		{
			GetUpDownControl().ProcessCharacter(editChar);
		}

		public override void Destroy()
		{
			MxIntegerUpDown integerUpDown = GetUpDownControl();
			if (integerUpDown != null)
			{
				integerUpDown.ValueChanged -= OnEditorValueChanged;
			}
			base.Destroy();
		}

		private void OnEditorValueChanged(object sender, EventArgs eventArgs)
		{
			OnChange(eventArgs);
		}

		public override void OnUpDownButtonClick(int button)
		{
			if (button == 1)
			{
				GetUpDownControl().StartRepeatUp();
			}
			else
			{
				GetUpDownControl().StartRepeatDown();
			}
		}

		public override void OnButtonMouseUp()
		{
			GetUpDownControl().StopRepeat();
		}

		public override void ResumeUpDownRepeater()
		{
			GetUpDownControl().ResumeRepeat();
		}

		public override void SuspendUpDownRepeater()
		{
			GetUpDownControl().SuspendRepeat();
		}
	}
}
