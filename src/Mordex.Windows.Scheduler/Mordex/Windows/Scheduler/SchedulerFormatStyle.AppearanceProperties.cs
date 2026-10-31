using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerFormatStyle
	{
		[Description("The background color of this SchedulerFormatStyle.")]
		public Color BackColor
		{
			get
			{
				if (_backColor.IsEmpty)
				{
					return _themeBackColor;
				}
				return _backColor;
			}
			set
			{
				if (value != _backColor)
				{
					_backColor = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Specifies the background color used in a linear gradient pathern.")]
		public Color BackColorGradient
		{
			get
			{
				if (_backColorGradient.IsEmpty)
				{
					return _themeBackColorGradient;
				}
				return _backColorGradient;
			}
			set
			{
				if (value != _backColorGradient)
				{
					_backColorGradient = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Specifies if a linear gradient will be applied and its direction.")]
		public BackgroundGradientMode BackgroundGradientMode
		{
			get
			{
				if (_backgroundGradientMode == BackgroundGradientMode.Empty)
				{
					return _themeGradientMode;
				}
				return _backgroundGradientMode;
			}
			set
			{
				if (_backgroundGradientMode != value)
				{
					_backgroundGradientMode = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Specifies the foreground color of the SchedulerFormatStyle.")]
		public Color ForeColor
		{
			get
			{
				if (_foreColor.IsEmpty)
				{
					return _themeForeColor;
				}
				return _foreColor;
			}
			set
			{
				if (value != _foreColor)
				{
					_foreColor = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object used is bold.")]
		public TriState FontBold
		{
			get
			{
				return _fontBold;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontBold)
				{
					_fontBold = value;
					if (_customFont != null)
					{
						RebuildFont();
					}
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Indicates whether the Font object used is italic.")]
		[DefaultValue(TriState.Empty)]
		public TriState FontItalic
		{
			get
			{
				return _fontItalic;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontItalic)
				{
					_fontItalic = value;
					if (_customFont != null)
					{
						RebuildFont();
					}
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Specifies whether the Font object used is underlined.")]
		public TriState FontUnderline
		{
			get
			{
				return _fontUnderline;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontUnderline)
				{
					_fontUnderline = value;
					if (_customFont != null)
					{
						RebuildFont();
					}
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Specifies whether the Font object used has a horizontal line through the font.")]
		public TriState FontStrikeout
		{
			get
			{
				return _fontStrikeout;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontStrikeout)
				{
					_fontStrikeout = value;
					if (_customFont != null)
					{
						RebuildFont();
					}
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(-1f)]
		[Description("Indicates the percentage of the BlendColor to blend with the BackColor.")]
		public float Blend
		{
			get
			{
				return _blend;
			}
			set
			{
				if ((value < 0f || value > 1f) && value != -1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _blend)
				{
					_blend = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(-1f)]
		[Description("Indicates the percentage of the BlendColor to blend with the BackColorGradient.")]
		public float BlendGradient
		{
			get
			{
				return _blendGradient;
			}
			set
			{
				if ((value < 0f || value > 1f) && value != -1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _blendGradient)
				{
					_blendGradient = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Represents the alpha component that will be applied to colors in the FormatStyle.")]
		[DefaultValue(-1)]
		public int Alpha
		{
			get
			{
				return _alpha;
			}
			set
			{
				if (value < -1 || value > 255)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _alpha)
				{
					_alpha = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Determines how alpha should be combined with the background color.")]
		[DefaultValue(AlphaMode.Empty)]
		public AlphaMode BackColorAlphaMode
		{
			get
			{
				return _backColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backColorAlphaMode)
				{
					_backColorAlphaMode = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(AlphaMode.Empty)]
		[Description("Determines how alpha should be combined with the foreground color.")]
		public AlphaMode ForeColorAlphaMode
		{
			get
			{
				return _foreColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _foreColorAlphaMode)
				{
					_foreColorAlphaMode = value;
					InvalidateOwnerLayout();
				}
			}
		}
	}
}
