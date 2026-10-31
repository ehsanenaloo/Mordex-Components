using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DatePicker.Design;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override bool AutoScroll
		{
			get
			{
				return base.AutoScroll;
			}
			set
			{
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout
		{
			get
			{
				return base.BackgroundImageLayout;
			}
			set
			{
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Size AutoScrollMinSize
		{
			get
			{
				return base.AutoScrollMinSize;
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new Size AutoScrollMargin
		{
			get
			{
				return base.AutoScrollMargin;
			}
			set
			{
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Padding Padding
		{
			get
			{
				return base.Padding;
			}
			set
			{
				base.Padding = value;
			}
		}

		public new Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		public override ContextMenuStrip ContextMenuStrip
		{
			get
			{
				return _contextMenuStrip;
			}
			set
			{
				if (_contextMenuStrip != value)
				{
					_contextMenuStrip = value;
					if (TextBox != null)
					{
						TextBox.ContextMenuStrip = value;
						OnContextMenuStripChanged(EventArgs.Empty);
					}
					else
					{
						base.ContextMenuStrip = value;
					}
				}
			}
		}

		[Description("Specifies the background of the control when it is disabled")]
		[Category("Appearance")]
		public Color DisabledBackColor
		{
			get
			{
				if (_disabledBackColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.InputBackDisabled);
				}
				return _disabledBackColor;
			}
			set
			{
				if (_disabledBackColor != value)
				{
					_disabledBackColor = value;
					if (!base.Enabled)
					{
						Invalidate();
					}
				}
			}
		}

		[Category("Appearance")]
		[Description("Specifies the foreground of the control when it is disabled")]
		public Color DisabledForeColor
		{
			get
			{
				if (_disabledForeColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.TextDisabled);
				}
				return _disabledForeColor;
			}
			set
			{
				if (_disabledForeColor != value)
				{
					_disabledForeColor = value;
					if (!base.Enabled)
					{
						Invalidate();
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object NullValue
		{
			get
			{
				return _nullValue;
			}
			set
			{
				if (_nullValue != value)
				{
					_nullValue = value;
					RefreshNullBinding();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[Bindable(BindableSupport.Yes)]
		[Description("Represents a date, or System.DBNull.Value, associated with the control.")]
		public object BindableValue
		{
			get
			{
				if (_isNullDate)
				{
					return _nullValue;
				}
				return Value;
			}
			set
			{
				_bindableValue = value;
				if (value == null || value == DBNull.Value || DatePickerHelper.CompareValues(value, _nullValue, null, Application.CurrentCulture) == 0)
				{
					IsNullDate = true;
					return;
				}
				if (value is DateTime dateValue)
				{
					if (dateValue < MinDate)
					{
						dateValue = MinDate;
					}
					else if (dateValue > MaxDate)
					{
						dateValue = MaxDate;
					}
					if (dateValue != Value || IsNullDate)
					{
						_isNullDate = false;
						ApplyValue(dateValue);
					}
					return;
				}
				string text = value as string ?? value.ToString();
				if (text.Length == 0)
				{
					IsNullDate = true;
					return;
				}
				if (!DateTime.TryParse(text, DatePickerHelper.GetDisplayFormat(), DateTimeStyles.None, out DateTime parsedDate))
				{
					throw new ArgumentException("The value cannot be converted to a date.", nameof(value));
				}
				Value = parsedDate;
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether the Value property is null.")]
		[DefaultValue(false)]
		public bool IsNullDate
		{
			get
			{
				return _isNullDate;
			}
			set
			{
				if (_isNullDate != value)
				{
					_isNullDate = value;
					SyncTextBoxFromValue();
					Invalidate();
					_reformatOnEndEdit = false;
					OnBindableValueChanged(EventArgs.Empty);
					_reformatOnEndEdit = true;
					if (EditStyle == EditStyle.Masked)
					{
						OnTextChanged(EventArgs.Empty);
					}
				}
			}
		}

		[Bindable(BindableSupport.Yes)]
		[Description("The date-time value assigned to the control.")]
		[Category("Behavior")]
		public DateTime Value
		{
			get
			{
				EndEdit();
				if (IsNullDate && _nullValue is DateTime)
				{
					return (DateTime)_nullValue;
				}
				return _value;
			}
			set
			{
				if (DatePickerHelper.CompareValues(value, _nullValue, null, Application.CurrentCulture) == 0)
				{
					IsNullDate = true;
					if (_nullValue is DateTime)
					{
						OnValueChanged(EventArgs.Empty);
					}
					return;
				}
				if (value < MinDate)
				{
					value = MinDate;
				}
				if (value > MaxDate)
				{
					value = MaxDate;
				}
				if (value != _value || _isNullDate)
				{
					if (!Checked && !IsNullDate && _autoCheckOnValueSet)
					{
						_checkedValue = true;
					}
					if (_isNullDate)
					{
						_isNullDate = false;
					}
					ApplyValue(value);
				}
			}
		}

		[Description("Specifies whether the text rendering should be compatible with previous release of Windows Forms.")]
		public bool UseCompatibleTextRendering
		{
			get
			{
				if (!_useCompatibleTextRendering.HasValue)
				{
					return false;
				}
				return _useCompatibleTextRendering.Value;
			}
			set
			{
				if (_useCompatibleTextRendering != value)
				{
					_useCompatibleTextRendering = value;
					if (_calendar != null)
					{
						_calendar.OnTextRenderingChanged();
					}
					BuildPortions();
					RequestLayout();
				}
			}
		}

		[Description("The custom date-time format string.")]
		[Localizable(true)]
		[DefaultValue("")]
		[Category("Behavior")]
		public string CustomFormat
		{
			get
			{
				return _customFormat;
			}
			set
			{
				if (_customFormat != value)
				{
					if (string.Compare(value, "d", ignoreCase: false) == 0)
					{
						value = DatePickerHelper.GetDisplayFormat().ShortDatePattern;
					}
					if (string.Compare(value, "D", ignoreCase: false) == 0)
					{
						value = DatePickerHelper.GetDisplayFormat().LongDatePattern;
					}
					if (string.Compare(value, "G", ignoreCase: false) == 0)
					{
						value = DatePickerHelper.GetDisplayFormat().ShortDatePattern + ", " + DatePickerHelper.GetDisplayFormat().LongTimePattern;
					}
					if (string.Compare(value, "F", ignoreCase: false) == 0)
					{
						value = DatePickerHelper.GetDisplayFormat().FullDateTimePattern;
					}
					if (string.Compare(value, "f", ignoreCase: false) == 0 || string.Compare(value, "g", ignoreCase: false) == 0)
					{
						value = DatePickerHelper.GetDisplayFormat().ShortDatePattern + ", " + DatePickerHelper.GetDisplayFormat().ShortTimePattern;
					}
					if (string.Compare(value, "t", ignoreCase: false) == 0)
					{
						value = DatePickerHelper.GetDisplayFormat().ShortTimePattern;
					}
					if (string.Compare(value, "T", ignoreCase: false) == 0)
					{
						value = DatePickerHelper.GetDisplayFormat().LongTimePattern;
					}
					_customFormat = value;
					BuildPortions();
				}
			}
		}

		[Category("Behavior")]
		[Description("The format of the date and time displayed in the control.")]
		[Localizable(true)]
		[DefaultValue(DateFormat.Short)]
		public DateFormat DateFormat
		{
			get
			{
				return _dateFormat;
			}
			set
			{
				if (!Enum.IsDefined(typeof(DateFormat), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_dateFormat != value)
				{
					_dateFormat = value;
					RequestLayout();
					BuildPortions();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		[Category("Behavior")]
		[Description("The minimum allowable date")]
		[Editor(typeof(DateTimeEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(System.ComponentModel.DateTimeConverter))]
		public DateTime MinDate
		{
			get
			{
				return _calendar.GetMinDate();
			}
			set
			{
				_calendar.SetMinDate(value);
				ClampValueToRange();
			}
		}

		[Description("The maximum allowable date")]
		[TypeConverter(typeof(System.ComponentModel.DateTimeConverter))]
		[Category("Behavior")]
		[Editor(typeof(DateTimeEditor), typeof(UITypeEditor))]
		public DateTime MaxDate
		{
			get
			{
				return _calendar.GetMaxDate();
			}
			set
			{
				_calendar.SetMaxDate(value);
				ClampValueToRange();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DatePortion ActiveDatePortion
		{
			get
			{
				if (GetActivePortion() != null)
				{
					return DatePickerHelper.ToDatePortion(GetActivePortion().GetPortionType());
				}
				return DatePortion.None;
			}
			set
			{
				if (value != ActiveDatePortion)
				{
					EndEdit();
					SetActivePortion(FindPortion(value));
				}
			}
		}

		[Description("Specifies the effect of the control when the mouse hovers over it")]
		[Category("Appearance")]
		[DefaultValue(HoverMode.None)]
		public HoverMode HoverMode
		{
			get
			{
				return _hoverMode;
			}
			set
			{
				if (_hoverMode != value)
				{
					_hoverMode = value;
					RequestLayout();
				}
			}
		}

		[Description("The DatePicker\u00b4s border style.")]
		[Category("Appearance")]
		[DefaultValue(BorderStyle.SunkenLight3D)]
		public BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_borderStyle != value)
				{
					_borderStyle = value;
					RequestLayout();
				}
			}
		}

		[DefaultValue(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Indicates whether the height of the control automatically adjusts when the font assigned to the control is changed.")]
		[Browsable(true)]
		public override bool AutoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					RequestLayout();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("Indicates whether the drop-down button is showed.")]
		public bool ShowDropDown
		{
			get
			{
				return _showDropDown;
			}
			set
			{
				if (_showDropDown != value)
				{
					_showDropDown = value;
					RequestLayout();
				}
			}
		}

		[Category("Appearance")]
		[Description("Indicates whether an up-down control is used to adjust the date-time value.")]
		[DefaultValue(false)]
		public bool ShowUpDown
		{
			get
			{
				return _showUpDown;
			}
			set
			{
				if (_showUpDown != value)
				{
					_showUpDown = value;
					RequestLayout();
				}
			}
		}

		[Category("Appearance")]
		[Description("The color of the border of the headers and the control when it\u00b4s border is Flat.")]
		public Color FlatBorderColor
		{
			get
			{
				if (_flatBorderColor.IsEmpty)
				{
					return SystemColors.ControlDark;
				}
				return _flatBorderColor;
			}
			set
			{
				if (value != _flatBorderColor)
				{
					_flatBorderColor = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the style used to draw the buttons in the DatePicker and drop-down Calendar controls.")]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlStyle ButtonsStyle => _controlStyle;

		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Indicates whether the Value property has been set with a valid date-time value and the displayed value is able to be updated.")]
		[Bindable(BindableSupport.Yes)]
		public bool Checked
		{
			get
			{
				return _checkedValue;
			}
			set
			{
				if (_checkedValue != value)
				{
					_checkedValue = value;
					_autoCheckOnValueSet = false;
					if (value)
					{
						ActivateFirstPortion();
					}
					else
					{
						SetActivePortion(null);
					}
					_autoCheckOnValueSet = true;
					Invalidate();
					OnCheckedChanged(EventArgs.Empty);
				}
			}
		}

		[Description("Indicates whether a check box is displayed to the left of the selected date.")]
		[Category("Appearance")]
		[DefaultValue(false)]
		public bool ShowCheckBox
		{
			get
			{
				return _showCheckBox;
			}
			set
			{
				if (_showCheckBox != value)
				{
					_showCheckBox = value;
					RequestLayout();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(false)]
		[Description("Indicates whether the DatePicker control can accept null values.")]
		public bool Nullable
		{
			get
			{
				return _nullable;
			}
			set
			{
				if (value != _nullable)
				{
					_nullable = value;
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(1)]
		[Description("Spcefies the number of columns for the DropDownCalendar")]
		public int DropDownColumns
		{
			get
			{
				return _dropDownColumns;
			}
			set
			{
				if (value < 0)
				{
					throw new InvalidOperationException();
				}
				_dropDownColumns = value;
			}
		}

		[Category("Appearance")]
		[DefaultValue(1)]
		[Description("Spcefies the number of rows for the DropDownCalendar")]
		public int DropDownRows
		{
			get
			{
				return _dropDownRows;
			}
			set
			{
				if (value < 0)
				{
					throw new InvalidOperationException();
				}
				_dropDownRows = value;
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get
			{
				EnsureCulture();
				if (_textBox != null)
				{
					return _textBox.Text;
				}
				if (_isNullDate)
				{
					return "";
				}
				return DatePickerHelper.FormatDate(_value, GetFormatPattern());
			}
			set
			{
				EnsureCulture();
				if (value == "")
				{
					IsNullDate = true;
					return;
				}
				DateTimeFormatInfo format = DatePickerHelper.GetDisplayFormat();
				if (DateTime.TryParseExact(value, GetFormatPattern(), format, DateTimeStyles.None, out DateTime exactDate))
				{
					BindableValue = exactDate;
				}
				else
				{
					BindableValue = DateTime.Parse(value, format);
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(false)]
		public bool DroppedDown
		{
			get
			{
				return _droppedDown;
			}
			set
			{
				if (_droppedDown != value)
				{
					_droppedDown = value;
					if (_dropDownButton != null)
					{
						_dropDownButton.SetIsPressed(value);
					}
					if (value)
					{
						OpenDropDown(true);
					}
					else
					{
						CloseDropDown(true);
					}
				}
			}
		}

		[Description("The drop-down Calendar control associated with this DatePicker control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor(typeof(DropDownEditor), typeof(UITypeEditor))]
		public DatePickerCalendar DropDownCalendar => _calendar;

		[Localizable(true)]
		[Category("Appearance")]
		[DefaultValue("Today")]
		[Description("The text displayed in the Today button.")]
		public string TodayButtonText
		{
			get
			{
				return _calendar.GetTodayButtonText();
			}
			set
			{
				_calendar.SetTodayButtonText(value);
			}
		}

		[DefaultValue("None")]
		[Description("The text displayed in the Null button.")]
		[Localizable(true)]
		[Category("Appearance")]
		public string NullButtonText
		{
			get
			{
				return _calendar.GetNoneButtonText();
			}
			set
			{
				_calendar.SetNoneButtonText(value);
			}
		}

		[Description("Indicates the number of years to increase/decrease when the user clicks on the up-down buttons.")]
		[Category("Behavior")]
		[DefaultValue(1)]
		public int YearIncrement
		{
			get
			{
				return _fieldIncrements._yearIncrement;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				_fieldIncrements._yearIncrement = value;
			}
		}

		[Description("Indicates the number of months to increase/decrease when the user clicks on the up-down buttons.")]
		[Category("Behavior")]
		[DefaultValue(1)]
		public int MonthIncrement
		{
			get
			{
				return _fieldIncrements._monthIncrement;
			}
			set
			{
				if (value < 0 || value > 11)
				{
					throw new ArgumentOutOfRangeException();
				}
				_fieldIncrements._monthIncrement = value;
			}
		}

		[Category("Behavior")]
		[DefaultValue(1)]
		[Description("Indicates the number of days to increase/decrease when the user clicks on the up-down buttons.")]
		public int DayIncrement
		{
			get
			{
				return _fieldIncrements._dayIncrement;
			}
			set
			{
				if (value < 0 || value > 30)
				{
					throw new ArgumentOutOfRangeException();
				}
				_fieldIncrements._dayIncrement = value;
			}
		}

		[Category("Behavior")]
		[DefaultValue(1)]
		[Description("Indicates the number of hours to increase/decrease when the user clicks on the up-down buttons.")]
		public int HourIncrement
		{
			get
			{
				return _fieldIncrements._hourIncrement;
			}
			set
			{
				if (value < 0 || value > 12)
				{
					throw new ArgumentOutOfRangeException();
				}
				_fieldIncrements._hourIncrement = value;
			}
		}

		[Category("Behavior")]
		[DefaultValue(5)]
		[Description("Indicates the number of minutes to increase/decrease when the user clicks on the up-down buttons.")]
		public int MinuteIncrement
		{
			get
			{
				return _fieldIncrements._minuteIncrement;
			}
			set
			{
				if (value < 0 || value > 60)
				{
					throw new ArgumentOutOfRangeException();
				}
				_fieldIncrements._minuteIncrement = value;
			}
		}

		[DefaultValue(5)]
		[Category("Behavior")]
		[Description("Indicates the number of seconds to increase/decrease when the user clicks on the up-down buttons.")]
		public int SecondIncrement
		{
			get
			{
				return _fieldIncrements._secondIncrement;
			}
			set
			{
				if (value < 0 || value > 60)
				{
					throw new ArgumentOutOfRangeException();
				}
				_fieldIncrements._secondIncrement = value;
			}
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("Indicates whether the today button is showed.")]
		public bool ShowTodayButton
		{
			get
			{
				return _calendar.GetShowTodayButton();
			}
			set
			{
				_calendar.SetShowTodayButton(value);
			}
		}

		[Description("Indicates whether the null button is showed.")]
		[Category("Appearance")]
		[DefaultValue(false)]
		public bool ShowNullButton
		{
			get
			{
				return _calendar.GetShowNoneButton();
			}
			set
			{
				_calendar.SetShowNoneButton(value);
			}
		}

		[Category("Behavior")]
		[Description("Specifies the edition style for the control.")]
		[DefaultValue(EditStyle.Masked)]
		public EditStyle EditStyle
		{
			get
			{
				return _editStyle;
			}
			set
			{
				if (_editStyle != value)
				{
					_editStyle = value;
					ApplyEditStyle();
				}
			}
		}

		[DefaultValue(MaskedValidation.OnChange)]
		[Description("Indicates when the control should validate the date in the control")]
		[Category("Behavior")]
		public MaskedValidation MaskedValidation
		{
			get
			{
				return _maskedValidation;
			}
			set
			{
				if (_maskedValidation != value)
				{
					_maskedValidation = value;
					BuildPortions();
				}
			}
		}

		[Browsable(false)]
		public TextBox TextBox => _textBox;

		[DefaultValue(false)]
		[Description("Indicates whether the contents of the control can be changed.")]
		[Category("Behavior")]
		public bool ReadOnly
		{
			get
			{
				return _readOnly;
			}
			set
			{
				if (_readOnly != value)
				{
					_readOnly = value;
					if (_textBox != null)
					{
						_textBox.ReadOnly = value;
					}
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.Equals(SystemColors.Window);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDisabledBackColor()
		{
			return !_disabledBackColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDisabledBackColor()
		{
			DisabledBackColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDisabledForeColor()
		{
			return !_disabledForeColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDisabledForeColor()
		{
			DisabledForeColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				if (_calendar != null)
				{
					_calendar.OnTextRenderingChanged();
				}
				BuildPortions();
				RequestLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetValue()
		{
			Value = DateTime.Today;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeValue()
		{
			if (IsNullDate)
			{
				return false;
			}
			return !_value.Equals(DateTime.Today);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaxDate()
		{
			MaxDate = DateTime.MaxValue.Date.AddYears(-1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaxDate()
		{
			return !MaxDate.Equals(DateTime.MaxValue.Date.AddYears(-1));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMinDate()
		{
			return !MinDate.Equals(DateTime.MinValue.AddMonths(1));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMinDate()
		{
			MinDate = DateTime.MinValue.AddMonths(1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !_flatBorderColor.IsEmpty;
		}
	}
}
