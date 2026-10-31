using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class ControlStyle : IDatePickerElement
	{
		internal IDatePickerHost _owner;

		private Color _controlColor = Color.Empty;

		private Color _controlTextColor = Color.Empty;

		private Color _flatBorderColor = Color.Empty;

		private Brush _textBrush;

		private Color _hoverBaseColor = Color.Empty;

		private Color _hoverBlendColor = Color.Empty;

		private float _hoverSelectedBlend = 0.5f;

		private CalendarFormatStyle _formatStyle;

		private CalendarFormatStyle _hotFormatStyle;

		private CalendarFormatStyle _pressedFormatStyle;

		[Description("Specifies the background color of the buttons in the DatePicker and drop-down Calendar controls.")]
		public Color ControlColor
		{
			get
			{
				if (_controlColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault().Color(ThemeRole.SurfaceRaised);
				}
				return _controlColor;
			}
			set
			{
				if (value != _controlColor)
				{
					_controlColor = value;
					ResetHoverPen();
				}
			}
		}

		[Description("Specifies the foreground color of the buttons in the DatePicker and drop-down Calendar controls.")]
		public Color ControlTextColor
		{
			get
			{
				if (_controlTextColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault().Color(ThemeRole.Text);
				}
				return _controlTextColor;
			}
			set
			{
				if (_controlTextColor != value)
				{
					_controlTextColor = value;
					ResetTextResources();
				}
			}
		}

		[Description("The color that will be blended with the HoverBlendColor when a flat apperance is used.")]
		public Color HoverBaseColor
		{
			get
			{
				if (_hoverBaseColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault().Color(ThemeRole.Accent);
				}
				return _hoverBaseColor;
			}
			set
			{
				if (_hoverBaseColor != value)
				{
					_hoverBaseColor = value;
					ResetHoverPen();
				}
			}
		}

		[Description("The color that will be blended with the HoverBaseColor when a flat appearance is used.")]
		public Color HoverBlendColor
		{
			get
			{
				if (_hoverBlendColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault().Color(ThemeRole.InputBack);
				}
				return _hoverBlendColor;
			}
			set
			{
				if (!_hoverBlendColor.Equals(value))
				{
					_hoverBlendColor = value;
					ResetHoverPen();
				}
			}
		}

		[Description("The percentage of the HoverBlendColor to blend with the HoverBaseColor when the mouse is captured by the button.")]
		[DefaultValue(0.5f)]
		public float HoverSelectedBlend
		{
			get
			{
				return _hoverSelectedBlend;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException("value", "HoverSelectedBlend property value must be between 0 and 1");
				}
				if (_hoverSelectedBlend != value)
				{
					_hoverSelectedBlend = value;
					ResetHoverPen();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represent the format style used to draw the buttons in the control.")]
		public CalendarFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new CalendarFormatStyle(this, PredefinedStyle.Button);
				}
				return _formatStyle;
			}
		}

		[Description("Represent the format style used to draw the buttons in the control when the mouse is over them.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CalendarFormatStyle HotFormatStyle
		{
			get
			{
				if (_hotFormatStyle == null)
				{
					_hotFormatStyle = new CalendarFormatStyle(this, PredefinedStyle.Hot);
				}
				return _hotFormatStyle;
			}
		}

		[Description("Represent the format style used to draw the buttons in the control when they are pressed.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CalendarFormatStyle PressedFormatStyle
		{
			get
			{
				if (_pressedFormatStyle == null)
				{
					_pressedFormatStyle = new CalendarFormatStyle(this, PredefinedStyle.Pressed);
				}
				return _pressedFormatStyle;
			}
		}

		DatePickerCalendar IDatePickerElement.Calendar => null;

		internal ControlStyle(IDatePickerHost owner)
		{
			_owner = owner;
		}

		internal void DisposeResources()
		{
			if (_textBrush != null)
			{
				_textBrush.Dispose();
				_textBrush = null;
			}
			FormatStyle.DisposeResources();
			HotFormatStyle.DisposeResources();
			PressedFormatStyle.DisposeResources();
		}

		private void NotifyOwnerChanged()
		{
			if (_owner != null)
			{
				_owner.OnControlStyleChanged();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_hoverSelectedBlend = 0.5f;
			ResetControlColor();
			ResetControlTextColor();
			ResetHoverBaseColor();
			ResetHoverBlendColor();
		}

		internal void ApplyThemeToFormats()
		{
			FormatStyle.ApplyTheme();
			HotFormatStyle.ApplyTheme();
			PressedFormatStyle.ApplyTheme();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlColor()
		{
			ControlColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlColor()
		{
			return !_controlColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlTextColor()
		{
			ControlTextColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlTextColor()
		{
			return !_controlTextColor.IsEmpty;
		}

		internal Color GetFlatBorderColor()
		{
			return _flatBorderColor;
		}

		private void ResetTextResources()
		{
			if (_textBrush != null)
			{
				_textBrush.Dispose();
				_textBrush = null;
			}
			NotifyOwnerChanged();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverBaseColor()
		{
			HoverBaseColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverBaseColor()
		{
			return !_hoverBaseColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverBlendColor()
		{
			HoverBlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverBlendColor()
		{
			return !_hoverBlendColor.IsEmpty;
		}

		internal Pen GetHoverPen()
		{
			return GdiCache.Pen(HoverBaseColor);
		}

		private void ResetHoverPen()
		{
			NotifyOwnerChanged();
		}

		void IDatePickerElement.Invalidate()
		{
			NotifyOwnerChanged();
		}
	}
}
