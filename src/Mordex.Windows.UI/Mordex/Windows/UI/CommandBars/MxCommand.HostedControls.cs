using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommand
	{
		internal bool IsControlEditable()
		{
			if (IsEditableControl == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal Control GetExistingControl()
		{
			if (CommandManager == null)
			{
				return _control;
			}
			Control control = null;
			if (_baseCommand != null && _baseCommand._hostedControls != null)
			{
				if (_baseCommand._hostedControls.ContainsKey(this))
				{
					control = (Control)_baseCommand._hostedControls[this];
				}
			}
			else if (_hostedControls != null && _hostedControls.ContainsKey(this))
			{
				control = (Control)_hostedControls[this];
			}
			if (control != null)
			{
				return control;
			}
			if (_control == null && _baseCommand != null)
			{
				return _baseCommand._control;
			}
			return _control;
		}

		public TextBox GetTextBox()
		{
			if (CommandType == CommandType.TextBoxCommand)
			{
				return (TextBox)Control;
			}
			return null;
		}

		public MxComboBox GetUIComboBox()
		{
			if (CommandType == CommandType.ComboBoxCommand)
			{
				return (MxComboBox)Control;
			}
			return null;
		}

		public MxFontPicker GetUIFontPicker()
		{
			if (CommandType == CommandType.FontPickerCommand)
			{
				return (MxFontPicker)Control;
			}
			return null;
		}

		public MxColorPicker GetUIColorPicker()
		{
			if (CommandType == CommandType.ColorPickerCommand)
			{
				return (MxColorPicker)Control;
			}
			return null;
		}

		private Control CreateTextBox(MxCommand key)
		{
			TextBox textBox = new TextBox();
			textBox.Width = ControlWidth;
			textBox.AcceptsReturn = true;
			if (_hostedControls == null)
			{
				_hostedControls = new Hashtable();
			}
			_hostedControls.Add(key, textBox);
			if (key._baseCommand != null && ControlValue != null)
			{
				textBox.Text = (string)ControlValue;
			}
			textBox.TextChanged += OnControlTextChanged;
			textBox.TabStop = false;
			return textBox;
		}

		private Control CreateColorPicker(MxCommand key)
		{
			MxColorPicker uiColorPicker = new MxColorPicker();
			uiColorPicker.BorderStyle = BorderStyle.None;
			uiColorPicker.BackColor = Color.Transparent;
			if (_hostedControls == null)
			{
				_hostedControls = new Hashtable();
			}
			_hostedControls.Add(key, uiColorPicker);
			if (key._baseCommand != null && ControlValue != null)
			{
				uiColorPicker.SelectedColor = (Color)ControlValue;
			}
			uiColorPicker.SelectedColorChanged += OnControlSelectedColorChanged;
			uiColorPicker.TabStop = false;
			return uiColorPicker;
		}

		private MxFontPicker CreateFontPicker(MxCommand key)
		{
			MxFontPicker uiFontPicker = new MxFontPicker();
			uiFontPicker.Width = ControlWidth;
			uiFontPicker._shouldFocusTextBox = false;
			if (_hostedControls == null)
			{
				_hostedControls = new Hashtable();
			}
			_hostedControls.Add(key, uiFontPicker);
			if (key._baseCommand != null && ControlValue != null)
			{
				uiFontPicker.SelectedValue = ControlValue;
			}
			uiFontPicker.HoverMode = HoverMode.Highlight;
			uiFontPicker.BorderStyle = BorderStyle.None;
			uiFontPicker.TabStop = false;
			uiFontPicker.SelectedValueChanged += OnFontPickerSelectedValueChanged;
			return uiFontPicker;
		}

		private MxComboBox CreateComboBox(MxCommand key)
		{
			MxComboBox uiComboBox = null;
			if (key.BaseCommand == null)
			{
				uiComboBox = new MxComboBox();
			}
			else
			{
				MxComboBox baseComboBox = key.BaseCommand.GetUIComboBox();
				if (baseComboBox.GetActiveCurrencyManager() == null)
				{
					uiComboBox = new MxComboBox(baseComboBox.Items);
				}
				else
				{
					uiComboBox = new MxComboBox();
					uiComboBox.SetActiveCurrencyManager(baseComboBox.GetActiveCurrencyManager());
				}
			}
			uiComboBox.Width = ControlWidth;
			uiComboBox._shouldFocusTextBox = false;
			uiComboBox.HoverMode = HoverMode.Highlight;
			uiComboBox.BorderStyle = BorderStyle.None;
			if (_hostedControls == null)
			{
				_hostedControls = new Hashtable();
			}
			_hostedControls.Add(key, uiComboBox);
			if (key._baseCommand != null && ControlValue != null)
			{
				uiComboBox.SelectedValue = ControlValue;
			}
			uiComboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			uiComboBox.TabStop = false;
			return uiComboBox;
		}

		internal bool IsButtonType()
		{
			switch (CommandType)
			{
			case CommandType.Command:
			case CommandType.ToggleButton:
			case CommandType.MdiList:
			case CommandType.ColorPickerCommand:
			case CommandType.ControlPopup:
				return true;
			default:
				return false;
			}
		}

		internal bool HasEmbeddedControl()
		{
			switch (CommandType)
			{
			case CommandType.ComboBoxCommand:
			case CommandType.TextBoxCommand:
			case CommandType.FontPickerCommand:
			case CommandType.ControlContainer:
				if (Control != null)
				{
					return true;
				}
				break;
			}
			return false;
		}

		private Control GetOrCreateControl(MxCommand key)
		{
			if (_hostedControls != null && _hostedControls.ContainsKey(key))
			{
				return (Control)_hostedControls[key];
			}
			switch (CommandType)
			{
			case CommandType.ColorPickerCommand:
				return CreateColorPicker(key);
			case CommandType.TextBoxCommand:
				return CreateTextBox(key);
			case CommandType.ComboBoxCommand:
				return CreateComboBox(key);
			case CommandType.FontPickerCommand:
				return CreateFontPicker(key);
			default:
				return null;
			}
		}

		private void DisposeHostedControls()
		{
			if (_hostedControls == null)
			{
				return;
			}
			foreach (object key in _hostedControls.Keys)
			{
				Control control = (Control)_hostedControls[key];
				switch (CommandType)
				{
				case CommandType.FontPickerCommand:
				{
					MxFontPicker uiFontPicker = (MxFontPicker)control;
					uiFontPicker.SelectedValueChanged -= OnFontPickerSelectedValueChanged;
					uiFontPicker.Dispose();
					break;
				}
				case CommandType.TextBoxCommand:
				{
					TextBox textBox = (TextBox)control;
					textBox.TextChanged -= OnControlTextChanged;
					textBox.Dispose();
					break;
				}
				case CommandType.ComboBoxCommand:
				{
					MxComboBox uiComboBox = (MxComboBox)control;
					uiComboBox.SelectedValueChanged -= OnComboBoxSelectedValueChanged;
					uiComboBox.Dispose();
					break;
				}
				case CommandType.ColorPickerCommand:
				{
					MxColorPicker uiColorPicker = (MxColorPicker)control;
					uiColorPicker.SelectedColorChanged -= OnControlSelectedColorChanged;
					uiColorPicker.Dispose();
					break;
				}
				}
			}
			_hostedControls.Clear();
			_hostedControls = null;
		}

		private void ClearActiveControl(Control control)
		{
			Form form = RenderingHelper.FindForm(control);
			if (form != null)
			{
				form.ActiveControl = null;
			}
		}

		private void OnFontPickerSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_hostedControls != null)
			{
				MxFontPicker uiFontPicker = (MxFontPicker)sender;
				if (uiFontPicker.ContainsFocus)
				{
					ClearActiveControl(uiFontPicker);
				}
				object selectedValue = uiFontPicker.SelectedValue;
				foreach (object key in _hostedControls.Keys)
				{
					MxFontPicker otherFontPicker = (MxFontPicker)_hostedControls[key];
					otherFontPicker.SelectedValueChanged -= OnFontPickerSelectedValueChanged;
					otherFontPicker.SelectedValue = selectedValue;
					otherFontPicker.SelectedValueChanged += OnFontPickerSelectedValueChanged;
				}
			}
			CommandEventArgs e = new CommandEventArgs(this);
			OnControlValueChanged(e);
		}

		private void OnControlSelectedColorChanged(object sender, EventArgs eventArgs)
		{
			if (_hostedControls != null)
			{
				MxColorPicker uiColorPicker = (MxColorPicker)sender;
				if (uiColorPicker.ContainsFocus)
				{
					ClearActiveControl(uiColorPicker);
				}
				Color selectedColor = uiColorPicker.SelectedColor;
				foreach (object key in _hostedControls.Keys)
				{
					MxColorPicker otherColorPicker = (MxColorPicker)_hostedControls[key];
					otherColorPicker.SelectedColorChanged -= OnControlSelectedColorChanged;
					otherColorPicker.SelectedColor = selectedColor;
					otherColorPicker.SelectedColorChanged += OnControlSelectedColorChanged;
				}
			}
			CommandEventArgs e = new CommandEventArgs(this);
			OnControlValueChanged(e);
		}

		private void OnControlTextChanged(object sender, EventArgs eventArgs)
		{
			if (_hostedControls != null)
			{
				TextBox textBox = (TextBox)sender;
				string text = textBox.Text;
				foreach (object key in _hostedControls.Keys)
				{
					TextBox otherTextBox = (TextBox)_hostedControls[key];
					otherTextBox.TextChanged -= OnControlTextChanged;
					otherTextBox.Text = text;
					otherTextBox.TextChanged += OnControlTextChanged;
				}
			}
			CommandEventArgs e = new CommandEventArgs(this);
			OnControlValueChanged(e);
		}

		private void OnComboBoxSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_hostedControls != null)
			{
				MxComboBox uiComboBox = (MxComboBox)sender;
				object selectedValue = uiComboBox.SelectedValue;
				foreach (object key in _hostedControls.Keys)
				{
					MxComboBox otherComboBox = (MxComboBox)_hostedControls[key];
					otherComboBox.SelectedValueChanged -= OnComboBoxSelectedValueChanged;
					otherComboBox.SelectedValue = selectedValue;
					otherComboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
				}
			}
			CommandEventArgs e = new CommandEventArgs(this);
			OnControlValueChanged(e);
		}

		private void SetControlValue(Control control, object value)
		{
			if (control == null)
			{
				return;
			}
			if (control is MxFontPicker uiFontPicker)
			{
				uiFontPicker.SelectedValue = value;
			}
			else if (control is MxComboBox uiComboBox)
			{
				uiComboBox.SelectedValue = value;
			}
			else if (control is TextBox textBox)
			{
				if (!(value is string) && value != null)
				{
					throw new ArgumentException("The value must be of type 'string'");
				}
				textBox.Text = (string)value;
			}
			else if (control is MxColorPicker uiColorPicker)
			{
				Color selectedColor;
				if (value is Color)
				{
					selectedColor = (Color)value;
				}
				else
				{
					if (value != null)
					{
						throw new ArgumentException("The value must be of type 'System.Drawing.Color'");
					}
					selectedColor = Color.Empty;
				}
				uiColorPicker.SelectedColor = selectedColor;
			}
		}
	}
}
