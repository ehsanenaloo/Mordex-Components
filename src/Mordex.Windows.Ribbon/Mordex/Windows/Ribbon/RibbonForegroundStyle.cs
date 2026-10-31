using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class RibbonForegroundStyle : IMordexXmlLayoutsSupport
	{
		private bool? _fontBold;

		private bool? _fontItalic;

		private bool? _fontUnderline;

		private bool? _fontStrikeout;

		private Color _foreColor;

		private IInvalidatable _owner;

		[Description("Indicate whether the Font object used is bold.")]
		[DefaultValue(null)]
		public bool? FontBold
		{
			get
			{
				return _fontBold;
			}
			set
			{
				if (_fontBold != value)
				{
					_fontBold = value;
					InvalidateOwner();
				}
			}
		}

		[DefaultValue(null)]
		[Description("Indicates whether the Font object used is italic.")]
		public bool? FontItalic
		{
			get
			{
				return _fontItalic;
			}
			set
			{
				if (_fontItalic != value)
				{
					_fontItalic = value;
					InvalidateOwner();
				}
			}
		}

		[DefaultValue(null)]
		[Description("Indicates whether the Font object used is underlined.")]
		public bool? FontUnderline
		{
			get
			{
				return _fontUnderline;
			}
			set
			{
				if (_fontUnderline != value)
				{
					_fontUnderline = value;
					InvalidateOwner();
				}
			}
		}

		[DefaultValue(null)]
		[Description("Indicates whether the Font specifies a horizontal line through the font.")]
		public bool? FontStrikeout
		{
			get
			{
				return _fontStrikeout;
			}
			set
			{
				if (_fontStrikeout != value)
				{
					_fontStrikeout = value;
					InvalidateOwner();
				}
			}
		}

		[Description("The foreground color for this RibbonForegroundStyle.")]
		public Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				if (_foreColor != value)
				{
					_foreColor = value;
					InvalidateOwner();
				}
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal RibbonForegroundStyle(IInvalidatable owner)
		{
			_foreColor = Color.Empty;
			_owner = owner;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColor()
		{
			return !_foreColor.IsEmpty;
		}

		public void ResetForeColor()
		{
			ForeColor = Color.Empty;
		}

		private void InvalidateOwner()
		{
			if (_owner != null)
			{
				_owner.Invalidate();
			}
		}

		internal RibbonForegroundStyle CloneForOwner(IInvalidatable owner)
		{
			RibbonForegroundStyle ribbonForegroundStyle = new RibbonForegroundStyle(owner);
			ribbonForegroundStyle._fontBold = _fontBold;
			ribbonForegroundStyle._fontItalic = _fontItalic;
			ribbonForegroundStyle._fontStrikeout = _fontStrikeout;
			ribbonForegroundStyle._fontUnderline = _fontUnderline;
			ribbonForegroundStyle._foreColor = _foreColor;
			return ribbonForegroundStyle;
		}

		internal Color ResolveForeColor(Color color, Color disabledColor, bool isEnabled)
		{
			if (!isEnabled)
			{
				return disabledColor;
			}
			if (!_foreColor.IsEmpty)
			{
				return _foreColor;
			}
			return color;
		}

		// The font to measure or draw with: the given font, or a styled font from the font cache when a font flag is set. The
		// caller disposes the lease when it no longer uses the font. The style keeps no font, so a lease never outlives its use.
		internal RibbonFontLease LeaseFont(Font font)
		{
			if (_fontBold.HasValue || _fontItalic.HasValue || _fontStrikeout.HasValue || _fontUnderline.HasValue)
			{
				return ThemedRibbonFonts.Styled(font.Name, font.Size, GetStyledFontStyle(font), font.Unit);
			}
			return new RibbonFontLease(font);
		}

		private FontStyle GetStyledFontStyle(Font font)
		{
			FontStyle fontStyle = FontStyle.Regular;
			bool isBold = ((!_fontBold.HasValue) ? font.Bold : (_fontBold.Value && _fontBold.Value));
			bool isItalic = ((!_fontItalic.HasValue) ? font.Italic : (_fontItalic.Value && _fontItalic.Value));
			bool isStrikeout = ((!_fontStrikeout.HasValue) ? font.Strikeout : (_fontStrikeout.Value && _fontStrikeout.Value));
			bool isUnderline = ((!_fontUnderline.HasValue) ? font.Underline : (_fontUnderline.Value && _fontUnderline.Value));
			if (isBold)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (isItalic)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (isStrikeout)
			{
				fontStyle |= FontStyle.Strikeout;
			}
			if (isUnderline)
			{
				fontStyle |= FontStyle.Underline;
			}
			return fontStyle;
		}

		internal bool HasCustomStyle()
		{
			if (_fontBold.HasValue)
			{
				return true;
			}
			if (_fontItalic.HasValue)
			{
				return true;
			}
			if (_fontStrikeout.HasValue)
			{
				return true;
			}
			if (_fontUnderline.HasValue)
			{
				return true;
			}
			if (!_foreColor.IsEmpty)
			{
				return true;
			}
			return false;
		}

		internal void ResetAll()
		{
			_foreColor = Color.Empty;
			_fontBold = null;
			_fontItalic = null;
			_fontStrikeout = null;
			_fontUnderline = null;
			InvalidateOwner();
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			object propertyValue = LayoutReader.GetPropertyValue(reader, "FontBold", typeof(bool), null);
			if (propertyValue != null)
			{
				_fontBold = (bool)propertyValue;
			}
			else
			{
				_fontBold = null;
			}
			object italicValue = LayoutReader.GetPropertyValue(reader, "FontItalic", typeof(bool), null);
			if (italicValue != null)
			{
				_fontItalic = (bool)italicValue;
			}
			else
			{
				_fontItalic = null;
			}
			object strikeoutValue = LayoutReader.GetPropertyValue(reader, "FontStrikeout", typeof(bool), null);
			if (strikeoutValue != null)
			{
				_fontStrikeout = (bool)strikeoutValue;
			}
			else
			{
				_fontStrikeout = null;
			}
			object underlineValue = LayoutReader.GetPropertyValue(reader, "FontUnderline", typeof(bool), null);
			if (underlineValue != null)
			{
				_fontUnderline = (bool)underlineValue;
			}
			else
			{
				_fontUnderline = null;
			}
			_foreColor = (Color)LayoutReader.GetPropertyValue(reader, "ForeColor", typeof(Color), Color.Empty);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter writer)
		{
			if (!_foreColor.IsEmpty)
			{
				writer.AddValue("ForeColor", _foreColor);
			}
			if (_fontBold.HasValue)
			{
				writer.AddValue("FontBold", _fontBold.Value);
			}
			if (_fontItalic.HasValue)
			{
				writer.AddValue("FontItalic", _fontItalic.Value);
			}
			if (_fontStrikeout.HasValue)
			{
				writer.AddValue("FontStrikeout", _fontStrikeout.Value);
			}
			if (_fontUnderline.HasValue)
			{
				writer.AddValue("FontUnderline", _fontUnderline.Value);
			}
		}
	}
}
