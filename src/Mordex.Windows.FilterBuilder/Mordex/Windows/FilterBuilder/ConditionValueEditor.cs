using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DatePicker;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Controls;
using ConditionOperator = Mordex.Data.ConditionOperator;
using FilterEditType = Mordex.Data.FilterEditType;

namespace Mordex.Windows.FilterBuilder
{
	// Edits one value (or one compared field) of a condition.
	[ToolboxItem(false)]
	internal class ConditionValueEditor : UserControl
	{
		private const int CompareToggleWidth = 18;

		private const int CheckedComboSelectorWidth = 22;

		private readonly MxFilterBuilder _filterBuilder;

		private readonly IMordexCondition _condition;

		private readonly EditFilterValue _valueSlot;

		private MxButton _fieldButton;

		private MxCheckBox _compareToggle;

		private MxContextMenu _fieldMenu;

		private IMordexValueItemCollection _valueItems;

		private readonly TypeCode _typeCode;

		private Control _editor;

		private object _value;

		private IMordexField _valueField;

		private bool _comparingToField;

		internal ConditionValueEditor(MxFilterBuilder filterBuilder, IMordexCondition condition, TypeCode typeCode, EditFilterValue valueSlot)
		{
			_filterBuilder = filterBuilder;
			_condition = condition;
			_valueSlot = valueSlot;
			if (valueSlot == EditFilterValue.Value1)
			{
				_value = condition.Value1;
				_valueField = condition.ValueField1;
			}
			else
			{
				_value = condition.Value2;
				_valueField = condition.ValueField2;
			}
			NormalizeInitialValue();
			LoadValueItems();
			_typeCode = typeCode;
			BuildEditor();
			ShowCompareToggle(filterBuilder.AllowFilterByFieldValue || _valueField != null);
			if (_valueField != null)
			{
				_compareToggle.Checked = true;
			}
			base.Margin = new Padding(3, 5, 3, 4);
		}

		internal event EventHandler ValueChanged;

		internal object Value => _value;

		internal IMordexField ValueField => _valueField;

		internal void SetValue(object value)
		{
			if (FilterValueComparer.MatchCase.Compare(_value, value) == 0)
			{
				return;
			}
			_value = value;
			if (_valueSlot == EditFilterValue.Value1)
			{
				_condition.Value1 = value;
			}
			else
			{
				_condition.Value2 = value;
			}
			ValueChanged?.Invoke(this, EventArgs.Empty);
		}

		private void SetValueField(IMordexField field)
		{
			if (_valueField == field)
			{
				return;
			}
			_valueField = field;
			if (_valueSlot == EditFilterValue.Value1)
			{
				_condition.ValueField1 = field;
			}
			else
			{
				_condition.ValueField2 = field;
			}
			ValueChanged?.Invoke(this, EventArgs.Empty);
		}

		private bool IsListOperator => _condition.ConditionOperator == ConditionOperator.In || _condition.ConditionOperator == ConditionOperator.NotIn;

		private void NormalizeInitialValue()
		{
			if (_value == null)
			{
				return;
			}
			if (IsListOperator)
			{
				SetValue(AsObjectArray(_value));
			}
			else if (_value is Array storedArray)
			{
				object[] items = AsObjectArray(storedArray);
				SetValue(items.Length > 0 ? items[0] : null);
			}
		}

		private static object[] AsObjectArray(object value)
		{
			if (value is object[] objects)
			{
				return objects;
			}
			if (value is Array sourceArray)
			{
				return sourceArray.Cast<object>().ToArray();
			}
			return new object[] { value };
		}

		private void LoadValueItems()
		{
			FilterEditType editType = FilterFields.EffectiveEditType(_condition.Field);
			if (editType != FilterEditType.ComboBox && editType != FilterEditType.DropDownList && !IsListOperator)
			{
				return;
			}
			FetchFieldValuesEventArgs args = _filterBuilder.RaiseFetchFieldValues(_condition);
			if (args.HasValueList != TriState.False)
			{
				_valueItems = args.ValueList ?? _condition.Field.GetFilterValueList();
			}
		}

		private void ShowCompareToggle(bool show)
		{
			if (show)
			{
				CreateCompareToggle();
			}
			else
			{
				DisposeCompareToggle();
			}
		}

		private void CreateCompareToggle()
		{
			if (_compareToggle != null)
			{
				return;
			}
			_compareToggle = new MxCheckBox();
			_compareToggle.Appearance = System.Windows.Forms.Appearance.Button;
			_compareToggle.ShowFocusRectangle = false;
			_compareToggle.CheckedChanged += OnCompareToggleCheckedChanged;
			_compareToggle.Dock = DockStyle.Right;
			_compareToggle.Width = CompareToggleWidth;
			_compareToggle.Image = ResourceFile.FieldsButton;
			_compareToggle.ToolTipText = _filterBuilder.BuiltInTextList[BuiltInText.ShowFieldsButtonTooltip];
			_compareToggle.Enabled = _valueField != null || HasComparableField();
			base.Controls.Add(_compareToggle);
		}

		private bool HasComparableField()
		{
			IMordexField conditionField = _condition.Field;
			foreach (IMordexField field in conditionField.Table.Fields)
			{
				if (field != conditionField && field.DataTypeCode == conditionField.DataTypeCode)
				{
					return true;
				}
			}
			return false;
		}

		private void DisposeCompareToggle()
		{
			if (_compareToggle != null)
			{
				_compareToggle.CheckedChanged -= OnCompareToggleCheckedChanged;
				_compareToggle.Dispose();
				_compareToggle = null;
			}
		}

		private void OnCompareToggleCheckedChanged(object sender, EventArgs eventArgs)
		{
			SetComparingToField(_compareToggle.Checked);
		}

		private void SetComparingToField(bool comparing)
		{
			if (_comparingToField == comparing)
			{
				return;
			}
			_comparingToField = comparing;
			if (comparing)
			{
				SetValue(null);
				ShowFieldButton();
			}
			else
			{
				SetValueField(null);
				ShowEditor();
			}
		}

		private void CreateFieldMenu()
		{
			_fieldMenu = new MxContextMenu();
			_fieldMenu.CommandManager = _filterBuilder.CommandManager;
			IMordexField conditionField = _condition.Field;
			List<MxCommand> commands = new List<MxCommand>();
			foreach (IMordexField field in conditionField.Table.Fields)
			{
				if (field != conditionField && field.DataTypeCode == conditionField.DataTypeCode && field.FilterEditType != FilterEditType.NoEdit)
				{
					commands.Add(CreateFieldCommand(field));
				}
			}
			if (_valueField != null)
			{
				bool listed = commands.Any(command => string.Compare(command.Key, _valueField.DataMember, ignoreCase: true, CultureInfo.InvariantCulture) == 0);
				if (!listed)
				{
					commands.Add(CreateFieldCommand(_valueField));
				}
				_fieldButton.Text = FilterFields.DisplayName(_valueField);
			}
			else
			{
				_fieldButton.Text = _filterBuilder.BuiltInTextList[BuiltInText.ChooseFieldText];
			}
			IEnumerable<MxCommand> ordered = _filterBuilder.SortFieldList ? commands.OrderBy(command => command.Text, StringComparer.CurrentCulture) : commands;
			foreach (MxCommand command in ordered)
			{
				_fieldMenu.Commands.Add(command);
			}
			_fieldMenu.CommandClick += OnFieldMenuCommandClick;
		}

		private static MxCommand CreateFieldCommand(IMordexField field)
		{
			MxCommand command = new MxCommand(field.DataMember, FilterFields.DisplayName(field));
			command.Tag = field;
			return command;
		}

		private void ShowFieldButton()
		{
			if (_fieldButton == null)
			{
				_fieldButton = new MxButton();
				_fieldButton.ButtonStyle = Mordex.Windows.UI.Controls.ButtonStyle.DropDown;
				_fieldButton.AutoSize = false;
				_fieldButton.Dock = DockStyle.Fill;
				_fieldButton.ShowFocusRectangle = false;
				_fieldButton.WordWrap = false;
				_fieldButton.TextHorizontalAlignment = Mordex.Windows.UI.Controls.TextAlignment.Near;
				_fieldButton.Margin = new Padding(0);
				_fieldButton.LostFocus += OnEditorLostFocus;
			}
			if (_fieldMenu == null)
			{
				CreateFieldMenu();
			}
			if (_editor != null && base.Controls.Contains(_editor))
			{
				base.Controls.Remove(_editor);
			}
			_fieldButton.DropDownContextMenu = _fieldMenu;
			_fieldButton.Tag = _valueField;
			if (!base.Controls.Contains(_fieldButton))
			{
				base.Controls.Add(_fieldButton);
			}
			base.Controls.SetChildIndex(_fieldButton, 0);
		}

		private void ShowEditor()
		{
			if (_editor == null)
			{
				return;
			}
			if (_fieldButton != null && base.Controls.Contains(_fieldButton))
			{
				base.Controls.Remove(_fieldButton);
			}
			ShowValueInEditor();
			_editor.Dock = DockStyle.Fill;
			_editor.Margin = new Padding(0);
			SetEditorHandlers(attach: false);
			SetEditorHandlers(attach: true);
			if (!base.Controls.Contains(_editor))
			{
				base.Controls.Add(_editor);
			}
			base.Controls.SetChildIndex(_editor, 0);
		}

		private void SetEditorHandlers(bool attach)
		{
			if (attach)
			{
				_editor.KeyDown += OnEditorKeyUp;
				_editor.KeyUp += OnEditorKeyUp;
				_editor.LostFocus += OnEditorLostFocus;
			}
			else
			{
				_editor.KeyDown -= OnEditorKeyUp;
				_editor.KeyUp -= OnEditorKeyUp;
				_editor.LostFocus -= OnEditorLostFocus;
			}
			switch (_editor)
			{
			case Mordex.Windows.DatePicker.MxDatePicker datePicker:
				if (attach)
				{
					datePicker.BindableValueChanged += OnEditorValuesChanged;
				}
				else
				{
					datePicker.BindableValueChanged -= OnEditorValuesChanged;
				}
				break;
			case MxComboBox comboBox:
				if (attach)
				{
					comboBox.SelectedValueChanged += OnEditorValuesChanged;
				}
				else
				{
					comboBox.SelectedValueChanged -= OnEditorValuesChanged;
				}
				break;
			case MxCheckedComboBox checkedComboBox:
				if (attach)
				{
					checkedComboBox.CheckedValuesChanged += OnEditorValuesChanged;
				}
				else
				{
					checkedComboBox.CheckedValuesChanged -= OnEditorValuesChanged;
				}
				break;
			}
		}

		private void DisposeEditor()
		{
			if (_editor != null)
			{
				SetEditorHandlers(attach: false);
				_editor.Dispose();
				_editor = null;
			}
		}

		private void DisposeFieldButton()
		{
			if (_fieldButton != null)
			{
				_fieldButton.LostFocus -= OnEditorLostFocus;
				_fieldButton.Dispose();
				_fieldButton = null;
			}
			if (_fieldMenu != null)
			{
				_fieldMenu.CommandClick -= OnFieldMenuCommandClick;
				_fieldMenu.Dispose();
				_fieldMenu = null;
			}
		}

		private MxCheckedComboBox CreateCheckedComboBox()
		{
			MxCheckedComboBox comboBox = new MxCheckedComboBox();
			comboBox.AutoSize = false;
			comboBox.DropDownDataSource = _valueItems;
			comboBox.DropDownButtonsVisible = false;
			comboBox.DropDownList.ColumnAutoResize = true;
			GridColumn selectorColumn = new GridColumn();
			selectorColumn.ActAsSelector = true;
			selectorColumn.Width = CheckedComboSelectorWidth;
			selectorColumn.AllowSize = false;
			comboBox.DropDownList.Columns.Add(selectorColumn);
			GridColumn textColumn = comboBox.DropDownList.Columns.Add("Text", ColumnType.Text);
			textColumn.Caption = "";
			comboBox.DropDownValueMember = "Value";
			comboBox.DropDownDisplayMember = "Text";
			return comboBox;
		}

		private Control CreateDatePicker()
		{
			Mordex.Windows.DatePicker.MxDatePicker datePicker = new Mordex.Windows.DatePicker.MxDatePicker();
			datePicker.AutoSize = false;
			datePicker.TodayButtonText = _filterBuilder.BuiltInTextList[BuiltInText.CalendarTodayText];
			string format = _condition.Field.FormatString;
			if (!string.IsNullOrEmpty(format))
			{
				datePicker.DateFormat = DateFormat.Custom;
				datePicker.CustomFormat = format;
			}
			return datePicker;
		}

		private Control CreateComboBox(FilterEditType editType)
		{
			MxComboBox comboBox = new MxComboBox();
			comboBox.AutoSize = false;
			comboBox.BindingContext = new BindingContext();
			if (editType == FilterEditType.DropDownList)
			{
				comboBox.ComboStyle = Mordex.Windows.UI.Controls.ComboStyle.DropDownList;
			}
			if (_typeCode == TypeCode.Boolean)
			{
				comboBox.Items.Add(new ComboBoxItem(_filterBuilder.BuiltInTextList[BuiltInText.True], true));
				comboBox.Items.Add(new ComboBoxItem(_filterBuilder.BuiltInTextList[BuiltInText.False], false));
			}
			else
			{
				comboBox.DisplayMember = "Text";
				comboBox.ValueMember = "Value";
				comboBox.DataSource = _valueItems;
			}
			return comboBox;
		}

		private Control CreateScalarEditor()
		{
			NumericEditValueType? numericType = NumericValueType(_typeCode);
			if (numericType.HasValue)
			{
				MxNumericBox numericBox = new MxNumericBox();
				numericBox.ValueType = numericType.Value;
				numericBox.NullBehavior = NumericEditNullBehavior.AllowNull;
				numericBox.AutoSize = false;
				numericBox.FormatString = _condition.Field.FormatString;
				return numericBox;
			}
			MxTextBox textBox = new MxTextBox();
			textBox.AutoSize = false;
			return textBox;
		}

		private static NumericEditValueType? NumericValueType(TypeCode typeCode)
		{
			switch (typeCode)
			{
			case TypeCode.Decimal: return NumericEditValueType.Decimal;
			case TypeCode.Double: return NumericEditValueType.Double;
			case TypeCode.Byte: return NumericEditValueType.Byte;
			case TypeCode.Int16: return NumericEditValueType.Int16;
			case TypeCode.Int32: return NumericEditValueType.Int32;
			case TypeCode.Int64: return NumericEditValueType.Int64;
			case TypeCode.SByte: return NumericEditValueType.SByte;
			case TypeCode.Single: return NumericEditValueType.Single;
			case TypeCode.UInt16: return NumericEditValueType.UInt16;
			case TypeCode.UInt32: return NumericEditValueType.UInt32;
			case TypeCode.UInt64: return NumericEditValueType.UInt64;
			default: return null;
			}
		}

		private void BuildEditor()
		{
			DisposeEditor();
			if (IsListOperator)
			{
				_editor = CreateCheckedComboBox();
			}
			else
			{
				FilterEditType editType = FilterFields.EffectiveEditType(_condition.Field);
				switch (editType)
				{
				case FilterEditType.DatePicker:
					_editor = CreateDatePicker();
					break;
				case FilterEditType.ComboBox:
				case FilterEditType.DropDownList:
					_editor = CreateComboBox(editType);
					break;
				default:
					_editor = CreateScalarEditor();
					break;
				}
			}
			ShowEditor();
		}

		// Puts the stored value into the editor, or the stored field into the field button.
		private void ShowValueInEditor()
		{
			if (_comparingToField)
			{
				_fieldButton.Tag = _valueField;
				return;
			}
			switch (_editor)
			{
			case MxNumericBox numericBox:
				try
				{
					numericBox.Value = _value;
				}
				catch (Exception)
				{
					// The stored value does not fit this editor, so it is shown empty.
					numericBox.Value = null;
				}
				break;
			case Mordex.Windows.DatePicker.MxDatePicker datePicker:
				try
				{
					datePicker.BindableValue = _value;
				}
				catch (Exception)
				{
					// The stored value is not a valid date, so it is shown empty.
					datePicker.BindableValue = null;
				}
				break;
			case MxComboBox comboBox:
				comboBox.SelectedValue = _value;
				break;
			case MxCheckedComboBox checkedComboBox:
				checkedComboBox.CheckedValues = _value == null ? null : AsObjectArray(_value);
				break;
			default:
				_editor.Text = _value == null ? "" : _value.ToString();
				break;
			}
		}

		// Reads the editor into the condition, or puts the stored value back when the edit is cancelled.
		private void CommitOrRevert(bool revert)
		{
			if (revert)
			{
				ShowValueInEditor();
				return;
			}
			if (_comparingToField)
			{
				SetValueField(_fieldButton.Tag as IMordexField);
				return;
			}
			switch (_editor)
			{
			case MxNumericBox numericBox:
				SetValue(numericBox.Value);
				break;
			case Mordex.Windows.DatePicker.MxDatePicker datePicker:
				SetValue(datePicker.BindableValue);
				break;
			case MxComboBox comboBox:
				SetValue(comboBox.SelectedValue);
				break;
			case MxCheckedComboBox checkedComboBox:
				SetValue(checkedComboBox.CheckedValues);
				break;
			default:
				SetValue(_editor.Text);
				break;
			}
		}

		private void OnFieldMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			_fieldButton.Tag = eventArgs.Command.Tag;
			_fieldButton.Text = eventArgs.Command.Text;
			CommitOrRevert(revert: false);
		}

		private void OnEditorKeyUp(object sender, KeyEventArgs eventArgs)
		{
			if (eventArgs.KeyData == Keys.Return)
			{
				CommitOrRevert(revert: false);
				Form form = _editor?.FindForm();
				if (form != null)
				{
					form.ActiveControl = null;
				}
			}
			else if (eventArgs.KeyData == Keys.Escape)
			{
				CommitOrRevert(revert: true);
			}
		}

		private void OnEditorValuesChanged(object sender, EventArgs eventArgs)
		{
			CommitOrRevert(revert: false);
		}

		private void OnEditorLostFocus(object sender, EventArgs eventArgs)
		{
			CommitOrRevert(revert: false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeEditor();
				DisposeFieldButton();
				DisposeCompareToggle();
			}
			base.Dispose(disposing);
		}
	}
}
