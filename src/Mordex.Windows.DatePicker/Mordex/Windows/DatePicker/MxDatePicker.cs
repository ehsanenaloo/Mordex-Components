using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	[DefaultBindingProperty("BindableValue")]
	[Designer("Mordex.Windows.DatePicker.Design.DatePickerDesigner, Mordex.Windows.DatePicker.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[DefaultEvent("ValueChanged")]
	public partial class MxDatePicker : ContainerControl, IDatePickerHost, IDropListOwner, ITextBoxOwner, IRibbonEditControl
	{
		// Delay before a pressed spin button starts to repeat and the repeat interval, in milliseconds.
		private const int SpinFirstRepeatDelay = 300;

		private const int SpinRepeatInterval = 150;

		private const int OwnerDrawnButtonWidth = 11;

		private const int CheckBoxColumnWidth = 14;

		private const int DefaultWidth = 200;

		private const int DefaultHeight = 20;

		private DatePickerCalendar _calendar;

		private PickerButton _dropDownButton;

		private PickerButton _upButton;

		private PickerButton _downButton;

		private PickerButton _checkBoxButton;

		private ControlStyle _controlStyle;

		private System.Windows.Forms.Timer _spinRepeatTimer;

		internal int _baseLineOffset;

		private bool _useDefaultMenu = true;

		private bool _actAsRibbonEditControl;

		private ContextMenuStrip _defaultContextMenu;

		private ContextMenuStrip _contextMenuStrip;

		private Color _disabledBackColor = Color.Empty;

		private Color _disabledForeColor = Color.Empty;

		private object _nullValue = DBNull.Value;

		private object _bindableValue;

		private bool _isNullDate;

		private bool _reformatOnEndEdit = true;

		private DateTime _value;

		private bool? _useCompatibleTextRendering;

		private string _customFormat = "";

		private DateFormat _dateFormat = DateFormat.Short;

		private DateFieldPortion[] _portions;

		private CultureInfo _cultureStamp;

		private bool _dropDownEnabled = true;

		private ThemeRenderer _themeRenderer;

		private readonly ControlStyle _borderHelperStyle = new ControlStyle(null);

		private bool _layoutPending = true;

		private bool _isMouseOver;

		private DateFieldPortion _activePortion;

		private PickerButton _pressedButton;

		private PickerButton _hotButton;

		private HoverMode _hoverMode = HoverMode.None;

		private BorderStyle _borderStyle = BorderStyle.SunkenLight3D;

		internal int _borderWidth;

		private bool _autoSize = true;

		private bool _showDropDown = true;

		private bool _showUpDown;

		private Color _flatBorderColor = Color.Empty;

		private bool _checkedValue = true;

		private bool _autoCheckOnValueSet = true;

		private bool _showCheckBox;

		private bool _nullable;

		private int _dropDownColumns = 1;

		private int _dropDownRows = 1;

		private bool _droppedDown;

		private FieldIncrements _fieldIncrements = new FieldIncrements();

		private EditStyle _editStyle;

		private MaskedValidation _maskedValidation;

		private TextBox _textBox;

		private bool _readOnly;

		private bool _forwardingTextBoxEvent;

		private bool _focusingTextBox;

		TextBox ITextBoxOwner.TextBoxControl => _textBox;

		bool ITextBoxOwner.UseDefaultMenu
		{
			get
			{
				return _useDefaultMenu;
			}
			set
			{
				if (value == _useDefaultMenu)
				{
					return;
				}
				_useDefaultMenu = value;
				if (_textBox != null)
				{
					if (!_useDefaultMenu && _textBox.ContextMenuStrip == GetDefaultContextMenu())
					{
						_textBox.ContextMenuStrip = null;
					}
					else
					{
						_textBox.ContextMenuStrip = GetDefaultContextMenu();
					}
				}
			}
		}

		bool IRibbonEditControl.ActAsRibbonEditControl
		{
			get
			{
				return _actAsRibbonEditControl;
			}
			set
			{
				if (_actAsRibbonEditControl != value)
				{
					_actAsRibbonEditControl = value;
					if (_textBox != null)
					{
						_textBox.BackColor = GetEditBackColor();
					}
				}
			}
		}

		Pen IDatePickerHost.FlatBorderPen => GdiCache.Pen(FlatBorderColor);

		MxDatePicker IDatePickerHost.DatePicker => this;

		public MxDatePicker()
		{
			CreateParts();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.UserMouse, value: true);
		}

		internal ContextMenuStrip GetDefaultContextMenu()
		{
			if (_defaultContextMenu == null)
			{
				_defaultContextMenu = new ContextMenuStrip();
			}
			return _defaultContextMenu;
		}

		private void CreateParts()
		{
			_calendar = new DatePickerCalendar(this);
			_calendar.EnabledChanged += OnCalendarEnabledChanged;
			_spinRepeatTimer = new System.Windows.Forms.Timer();
			_spinRepeatTimer.Enabled = false;
			_spinRepeatTimer.Interval = SpinRepeatInterval;
			_spinRepeatTimer.Tick += OnTimerTick;
			_downButton = new PickerButton(this, 1);
			_upButton = new PickerButton(this, -1);
			_checkBoxButton = new PickerButton(this, 0);
			_dropDownButton = new PickerButton(this, 1);
			_value = DateTime.Today;
			_maskedValidation = MaskedValidation.OnChange;
			BuildPortions();
			_controlStyle = new ControlStyle(this);
			_calendar.SetControlStyle(_controlStyle);
			BackColor = SystemColors.Window;
			base.Size = new Size(DefaultWidth, DefaultHeight);
		}

		public static string GetDatePickerVersion()
		{
			string localPath = typeof(MxDatePicker).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		protected override void Dispose(bool disposing)
		{
			_contextMenuStrip = null;
			_calendar.EnabledChanged -= OnCalendarEnabledChanged;
			StopThemeListening();
			if (disposing)
			{
				DisposeTimer();
				_calendar.Dispose();
				_controlStyle.DisposeResources();
				_borderHelperStyle.DisposeResources();
				ReleaseThemeRenderer();
				if (_defaultContextMenu != null)
				{
					_defaultContextMenu.Dispose();
					_defaultContextMenu = null;
				}
			}
			base.Dispose(disposing);
		}

		private void DisposeTimer()
		{
			_spinRepeatTimer.Tick -= OnTimerTick;
			_spinRepeatTimer.Dispose();
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_pressedButton != null && _pressedButton.GetIsPressed() && _activePortion != null)
			{
				if (_pressedButton == _upButton)
				{
					Value = _activePortion.GetIncremented(_value, _fieldIncrements, MaxDate);
				}
				else
				{
					Value = _activePortion.GetDecremented(_value, _fieldIncrements, MinDate);
				}
				_spinRepeatTimer.Interval = SpinRepeatInterval;
			}
		}

		void IDatePickerHost.OnControlStyleChanged()
		{
			RequestLayout();
		}
	}
}
