using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class ControlStyle
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Determines the 3D appearance of buttons and scrollbars.")]
		[DefaultValue(ButtonAppearance.Light3D)]
		public ButtonAppearance ButtonAppearance
		{
			get
			{
				return _buttonAppearance;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ButtonAppearance), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _buttonAppearance)
				{
					_buttonAppearance = value;
					NotifyChanged();
				}
			}
		}

		[Description("Specifies the background color of buttons and scrollbars.")]
		public Color ControlColor
		{
			get
			{
				if (!_classicLook && !ShouldSerializeControlColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.Surface);
				}
				return _controlColor;
			}
			set
			{
				ValidateOpaqueColor(value);
				if (value != _controlColor)
				{
					_controlColor = value;
					RefreshPaletteColors();
				}
			}
		}

		[Description("Represents the foreground color of buttons and scrollbars.")]
		public Color ControlTextColor
		{
			get
			{
				if (!_classicLook && !ShouldSerializeControlTextColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.Text);
				}
				return _controlTextColor;
			}
			set
			{
				ValidateOpaqueColor(value);
				if (_controlTextColor != value)
				{
					_controlTextColor = value;
					DisposeTextResources();
				}
			}
		}

		[Description("Represents the background color of the ScrollBar.")]
		public Color ScrollBarColor
		{
			get
			{
				if (!_classicLook && !ShouldSerializeScrollBarColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.ScrollTrack);
				}
				return _scrollBarColor;
			}
			set
			{
				ValidateOpaqueColor(value);
				if (_scrollBarColor != value)
				{
					_scrollBarColor = value;
					ResetScrollBarBrush();
				}
			}
		}

		[Description("Represents the background color of dropdown windows.")]
		public Color WindowColor
		{
			get
			{
				if (!_classicLook && !ShouldSerializeWindowColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.InputBack);
				}
				return _windowColor;
			}
			set
			{
				ValidateOpaqueColor(value);
				if (value != _windowColor)
				{
					_windowColor = value;
					DisposeWindowBrush();
				}
			}
		}

		[Description("Represents the foreground color of dropdown windows.")]
		public Color WindowTextColor
		{
			get
			{
				if (!_classicLook && !ShouldSerializeWindowTextColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.Text);
				}
				return _windowTextColor;
			}
			set
			{
				ValidateOpaqueColor(value);
				if (_windowTextColor != value)
				{
					_windowTextColor = value;
					DisposeWindowTextResources();
				}
			}
		}

		[Description("Indicates the color that will be blended with the HoverBlendColor when a flat apperance is used.")]
		public Color HoverBaseColor
		{
			get
			{
				return _hoverBaseColor;
			}
			set
			{
				ValidateOpaqueColor(value);
				if (_hoverBaseColor != value)
				{
					_hoverBaseColor = value;
					DisposeHoverResources();
				}
			}
		}

		[Description("Represents the color that will be blended with the HoverBaseColor when a flat appearance is used.")]
		public Color HoverBlendColor
		{
			get
			{
				return _hoverBlendColor;
			}
			set
			{
				ValidateOpaqueColor(value);
				if (!_hoverBlendColor.Equals(value))
				{
					_hoverBlendColor = value;
					DisposeHoverResources();
				}
			}
		}

		[DefaultValue(0.667f)]
		[Description("Indicates the percentage of the HoverBlendColor to blend wiht the HoverBaseColor.")]
		public float HoverBlend
		{
			get
			{
				return _hoverBlend;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException("value", "HoverBlend property value must be between 0 and 1");
				}
				if (_hoverBlend != value)
				{
					_hoverBlend = value;
					DisposeHoverResources();
				}
			}
		}

		[DefaultValue(0.5f)]
		[Description("Indicates the percentage of the HoverBlendColor to blend with the HoverBaseColor when the mouse is captured by the button.")]
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
					DisposeHoverResources();
				}
			}
		}

		Font IGridEnvironment.DefaultFont
		{
			get
			{
				if (_printDocument != null)
				{
					return _printDocument.ControlStyleFont;
				}
				return (Font)Mordex.Windows.Common.ThemeFonts.Current.Clone();
			}
		}

		bool IGridEnvironment.RightToLeft
		{
			get
			{
				if (_printDocument != null)
				{
					return _printDocument.IsRightToLeft;
				}
				return false;
			}
		}

		MxDataGridPrintDocument IGridEnvironment.PrintDocument
		{
			get
			{
				if (_printDocument is MxDataGridPrintDocument)
				{
					return (MxDataGridPrintDocument)_printDocument;
				}
				return null;
			}
		}
	}
}
