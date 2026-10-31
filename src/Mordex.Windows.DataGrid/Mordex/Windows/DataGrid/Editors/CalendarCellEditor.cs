using System;
using Mordex.Windows.DataGrid.Internal;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Editors.Calendar;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class CalendarCellEditor : CellEditor
	{
		private EditType _editType;

		public override EditType EditType
		{
			get
			{
				return _editType;
			}
			set
			{
				_editType = value;
				switch (_editType)
				{
				case EditType.DatePicker:
					GetCalendarState().SetEditStyle(EditStyle.Combo);
					break;
				case EditType.CalendarDropDown:
					GetCalendarState().SetEditStyle(EditStyle.DropDownList);
					break;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public override Control DropControl => GetCalendarState().GetDropControl();

		public override object Value
		{
			get
			{
				return GetCalendarState().GetValueOrText();
			}
			set
			{
				GetCalendarState().AssignDateValue(value);
			}
		}

		public override TextBox EditTextBox => GetCalendarState()._textBox;

		public override bool DroppedDown
		{
			get
			{
				return GetCalendarState().GetIsDroppedDown();
			}
			set
			{
				GetCalendarState().SetIsDroppedDown(value);
			}
		}

		internal CalendarCellEditor(GridColumnContainerControl editContainer)
			: base(editContainer)
		{
		}

		private CalendarEditControl GetCalendarState()
		{
			return (CalendarEditControl)EditControl;
		}

		protected override Control CreateEditControl()
		{
			CalendarEditControl calendarState = new CalendarEditControl();
			calendarState.SetControlStyle(GetControlStyle());
			calendarState.AddChangeHandler(OnEditorChanged);
			calendarState.AddDropHandler(HandleDroppingList);
			calendarState.AddDropDownHideCallback(HandleDropDownHide);
			return calendarState;
		}

		public override void StartEdit(MxDataGrid grid, GridColumn column, GridCell cell, GridFormatStyle style, object value, Image image, bool capture, Control control)
		{
			base.StartEdit(grid, column, cell, style, value, image, capture, control);
			CalendarEditControl calendarState = GetCalendarState();
			if (column.TextAlignment != TextAlignment.Empty)
			{
				calendarState.UpdateTextAlignment(column.TextAlignment);
			}
			else
			{
				calendarState.UpdateTextAlignment(style.TextAlignment);
			}
			calendarState.UpdateTextLayoutRenderer(column.ComputeTextLayoutRenderer(style, !grid.UseCompatibleTextRendering));
			calendarState.UpdateImageHorizontalAlignment(style.ImageHorizontalAlignment);
			calendarState.UpdateImageVerticalAlignment(style.ImageVerticalAlignment);
			calendarState._gridFormatStyle = grid.SelectedFormatStyle;
			string text = grid.BuiltInTexts[GridBuiltInText.CalendarTodayButton];
			string builtInText = grid.BuiltInTexts[GridBuiltInText.CalendarNoneButton];
			if (column.CalendarTodayButtonVisible == TriState.False)
			{
				text = "";
			}
			if (column.CalendarNoneButtonVisible == TriState.False)
			{
				builtInText = "";
			}
			if (column.CalendarTodayButtonVisible == TriState.True && string.IsNullOrEmpty(text))
			{
				text = " ";
			}
			if (column.CalendarNoneButtonVisible == TriState.True && string.IsNullOrEmpty(builtInText))
			{
				builtInText = " ";
			}
			calendarState.SetTodayButtonText(text);
			calendarState.SetNoneButtonText(builtInText);
			((ControlStyleContainerControl)calendarState).SetEditFormat(column.InputMask);
			calendarState.UpdateCharacter(column.MaskPrompt);
			calendarState.SetIncludeLiterals(column.IncludeLiterals);
			calendarState.SetImage(image);
			calendarState.SetImageList(grid.ComputeImageList());
			if (cell.CellDisplayType == ColumnType.Image || cell.CellDisplayType == ColumnType.ImageAndText)
			{
				calendarState.SetIsImageSet(true);
			}
			else
			{
				calendarState.SetIsImageSet(false);
			}
			calendarState.SetFormatString(column.FormatString);
			calendarState.SetUseInnerEditor(cell.CellDisplayType == ColumnType.Text || cell.CellDisplayType == ColumnType.ImageAndText || cell.CellDisplayType == ColumnType.Link);
			calendarState.SetControlStyle(GetControlStyle());
			calendarState.SetMultiline(column.WordWrap);
			calendarState.SetTextAlignment(column.LineAlignment);
			calendarState.SetMargins(column.LeftMargin, column.TopMargin - 1, column.RightMargin, column.BottomMargin - 1);
			calendarState.SetEditValue(value);
			calendarState.BackColor = GridColumnContainerControl.ResolveEditBackColor(grid, style);
			calendarState.ForeColor = GridColumnContainerControl.ResolveEditForeColor(grid, style);
			calendarState.Font = GridColumnContainerControl.ResolveEditFont(grid, style);
			calendarState.SetMaxLength(column.MaxLength);
			calendarState.UpdateCharacterCasing(column.CharacterCasing);
		}

		public override void OnDropDownButtonMouseDown()
		{
			GetCalendarState().PressDropDownButton();
		}

		public override void SetTextValue(string value)
		{
			DateTime? dateTime = null;
			if (!string.IsNullOrEmpty(value))
			{
				try
				{
					dateTime = DateTime.ParseExact(value, "MM/dd/yyyy", CultureInfo.InvariantCulture);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
				if (dateTime.HasValue)
				{
					GetCalendarState().AssignDateValue(dateTime.Value);
				}
				else
				{
					GetCalendarState().AssignDateValue(null);
				}
			}
			else
			{
				GetCalendarState().AssignDateValue(null);
			}
		}

		public override void ShowDropList()
		{
			GetCalendarState().SetIsDroppedDown(true);
		}

		public override void InitEdit(char editChar)
		{
			GetCalendarState().ProcessCharacter(editChar);
		}

		protected override void OnControlStyleChanged()
		{
			if (GetCalendarState() != null)
			{
				GetCalendarState().SetControlStyle(GetControlStyle());
			}
		}

		public override void SetComboVirtualWidth(int virtualWidth)
		{
			GetCalendarState()._virtualWidth = virtualWidth;
		}

		public override MaskEdit GetMaskEdit()
		{
			return GetCalendarState().AsMaskEdit();
		}

		public override bool DoMouseWheel(int delta)
		{
			return GetCalendarState().InvokeMouseWheel(delta);
		}

		public override void Destroy()
		{
			CalendarEditControl calendarState = GetCalendarState();
			if (calendarState != null)
			{
				calendarState.RemoveChangeHandler(OnEditorChanged);
				calendarState.RemoveDropHandler(HandleDroppingList);
				calendarState.RemoveDropDownHideCallback(HandleDropDownHide);
			}
			base.Destroy();
		}

		private void OnEditorChanged(object sender, EventArgs eventArgs)
		{
			OnChange(eventArgs);
		}

		private void HandleDroppingList(object sender, EventArgs eventArgs)
		{
			OnDroppingList(eventArgs);
		}

		private void HandleDropDownHide(object value, GridEventArgs gridEventArgs)
		{
			OnDropDownHide(gridEventArgs);
		}
	}
}
