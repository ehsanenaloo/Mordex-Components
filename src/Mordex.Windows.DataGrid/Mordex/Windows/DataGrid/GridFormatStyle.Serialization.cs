using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridFormatStyle
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_predefinedStyle = (PredefinedStyle)LayoutReader.GetPropertyValue(info, "PredefinedStyle", typeof(PredefinedStyle), PredefinedStyle.None);
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), -1);
			_appearance = (Appearance)LayoutReader.GetPropertyValue(info, "Appearance", typeof(Appearance), ComputeAppearance());
			_backgroundColor = (Color)LayoutReader.GetPropertyValue(info, "BackColor", typeof(Color), GetDefaultBackColor());
			_textColor = (Color)LayoutReader.GetPropertyValue(info, "ForeColor", typeof(Color), GetDefaultForeColor());
			_backColorGradient = (Color)LayoutReader.GetPropertyValue(info, "BackColorGradient", typeof(Color), GetDefaultBackColorGradient());
			_backColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "BackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_backgroundImage = (Image)LayoutReader.GetPropertyValue(info, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (BackgroundImageDrawMode)LayoutReader.GetPropertyValue(info, "BackgroundImageDrawMode", typeof(BackgroundImageDrawMode), BackgroundImageDrawMode.Empty);
			// The loaded font is new and belongs to the style. The font it replaces and the font resolved from it are released.
			Font loadedFont = (Font)LayoutReader.GetPropertyValue(info, "Font", typeof(Font), null);
			ReleaseBaseFont();
			ReleaseResolvedFont();
			_baseFont = loadedFont;
			_fontBold = (TriState)LayoutReader.GetPropertyValue(info, "FontBold", typeof(TriState), TriState.Empty);
			_fontItalic = (TriState)LayoutReader.GetPropertyValue(info, "FontItalic", typeof(TriState), TriState.Empty);
			_fontName = (string)LayoutReader.GetPropertyValue(info, "FontName", typeof(string), string.Empty);
			_fontSize = (float)LayoutReader.GetPropertyValue(info, "FontSize", typeof(float), 0f);
			_fontStrikeout = (TriState)LayoutReader.GetPropertyValue(info, "FontStrikeout", typeof(TriState), TriState.Empty);
			_fontUnderline = (TriState)LayoutReader.GetPropertyValue(info, "FontUnderline", typeof(TriState), ComputeTriState());
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "ForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_imageHorizontalAlignment = (ImageHorizontalAlignment)LayoutReader.GetPropertyValue(info, "ImageHorizontalAlignment", typeof(ImageHorizontalAlignment), ImageHorizontalAlignment.Empty);
			_imageVerticalAlignment = (ImageVerticalAlignment)LayoutReader.GetPropertyValue(info, "ImageVerticalAlignment", typeof(ImageVerticalAlignment), ImageVerticalAlignment.Empty);
			_identifier = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_lineAlignment = (TextAlignment)LayoutReader.GetPropertyValue(info, "LineAlignment", typeof(TextAlignment), ComputeTextAlignment());
			_textAlignment = (TextAlignment)LayoutReader.GetPropertyValue(info, "TextAlignment", typeof(TextAlignment), GetDefaultTextAlignment());
			_blend = (float)LayoutReader.GetPropertyValue(info, "Blend", typeof(float), -1f);
			_blendGradient = (float)LayoutReader.GetPropertyValue(info, "BlendGradient", typeof(float), -1f);
			_backgroundGradientMode = (BackgroundGradientMode)LayoutReader.GetPropertyValue(info, "BackgroundGradientMode", typeof(BackgroundGradientMode), BackgroundGradientMode.Empty);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_alpha != -1)
			{
				info.AddValue("Alpha", _alpha);
			}
			if (_appearance != ComputeAppearance())
			{
				info.AddValue("Appearance", _appearance);
			}
			if (ShouldSerializeBackColor())
			{
				info.AddValue("BackColor", BackColor);
			}
			if (ShouldSerializeForeColor())
			{
				info.AddValue("ForeColor", _textColor);
			}
			if (ShouldSerializeBackColorGradient())
			{
				info.AddValue("BackColorGradient", _backColorGradient);
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("BackColorAlphaMode", _backColorAlphaMode);
			}
			if (_backgroundImage != null)
			{
				info.AddValue("BackgroundImage", _backgroundImage);
			}
			if (_backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
			{
				info.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (_baseFont != null)
			{
				info.AddValue("Font", _baseFont);
			}
			if (_fontBold != TriState.Empty)
			{
				info.AddValue("FontBold", _fontBold);
			}
			if (_fontItalic != TriState.Empty)
			{
				info.AddValue("FontItalic", _fontItalic);
			}
			if (_fontName != string.Empty)
			{
				info.AddValue("FontName", _fontName);
			}
			if (_fontSize != 0f)
			{
				info.AddValue("FontSize", _fontSize);
			}
			if (_fontStrikeout != TriState.Empty)
			{
				info.AddValue("FontStrikeout", _fontStrikeout);
			}
			if (_fontUnderline != ComputeTriState())
			{
				info.AddValue("FontUnderline", _fontUnderline);
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("ForeColorAlphaMode", _foreColorAlphaMode);
			}
			if (_imageHorizontalAlignment != ImageHorizontalAlignment.Empty)
			{
				info.AddValue("ImageHorizontalAlignment", _imageHorizontalAlignment);
			}
			if (_imageVerticalAlignment != ImageVerticalAlignment.Empty)
			{
				info.AddValue("ImageVerticalAlignment", _imageVerticalAlignment);
			}
			if (_identifier != string.Empty)
			{
				info.AddValue("Key", _identifier);
			}
			if (_lineAlignment != ComputeTextAlignment())
			{
				info.AddValue("LineAlignment", _lineAlignment);
			}
			if (_predefinedStyle != PredefinedStyle.None)
			{
				info.AddValue("PredefinedStyle", _predefinedStyle);
			}
			if (_textAlignment != GetDefaultTextAlignment())
			{
				info.AddValue("TextAlignment", _textAlignment);
			}
			if (_blend != -1f)
			{
				info.AddValue("Blend", _blend);
			}
			if (_blendGradient != -1f)
			{
				info.AddValue("BlendGradient", _blendGradient);
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				info.AddValue("BackgroundGradientMode", _backgroundGradientMode);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorGradient()
		{
			return !_backColorGradient.Equals(GetDefaultBackColorGradient());
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !_backgroundColor.Equals(GetDefaultBackColor());
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColor()
		{
			return !_textColor.Equals(GetDefaultForeColor());
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAppearance()
		{
			return _appearance != ComputeAppearance();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFontUnderline()
		{
			return _fontUnderline != ComputeTriState();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTextAlignment()
		{
			return _textAlignment != GetDefaultTextAlignment();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLineAlignment()
		{
			return _lineAlignment != ComputeTextAlignment();
		}
	}
}
