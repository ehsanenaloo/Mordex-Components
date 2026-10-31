using System;
using System.Drawing;
using System.Globalization;
using System.Xml;

namespace Mordex.Windows.DataGrid.Export
{
	internal class ExportAppearanceState
	{
		private bool _hasFormatting;

		private string _fontName;

		private InheritableBoolean _bold;

		private InheritableBoolean _italic;

		private InheritableBoolean _underline;

		private InheritableBoolean _strikeout;

		private decimal _fontSize;

		private string _numberFormat;

		internal string _horizontalAlignment;

		private string _name;

		private Color _backColor;

		private Color _fontColor;

		internal ExportAppearanceState()
			: this(null)
		{
		}

		internal ExportAppearanceState(string name)
		{
			_name = name;
			_backColor = Color.Empty;
			_fontColor = Color.Empty;
			ApplyFont(null);
		}

		internal bool HasFormatting()
		{
			return _hasFormatting;
		}

		internal void SetHasFormatting(bool condition)
		{
			_hasFormatting = condition;
		}

		internal void ApplyFont(Font font)
		{
			if (font == null)
			{
				_bold = InheritableBoolean.Default;
				_italic = InheritableBoolean.Default;
				_strikeout = InheritableBoolean.Default;
				_underline = InheritableBoolean.Default;
				_fontSize = 0m;
				_fontName = null;
				return;
			}
			_fontName = font.Name;
			_fontSize = (decimal)font.SizeInPoints;
			if (font.Bold)
			{
				_bold = InheritableBoolean.True;
			}
			else
			{
				_bold = InheritableBoolean.Default;
			}
			if (font.Italic)
			{
				_italic = InheritableBoolean.True;
			}
			else
			{
				_italic = InheritableBoolean.Default;
			}
			if (font.Underline)
			{
				_underline = InheritableBoolean.True;
			}
			else
			{
				_underline = InheritableBoolean.Default;
			}
			if (font.Strikeout)
			{
				_strikeout = InheritableBoolean.True;
			}
			else
			{
				_strikeout = InheritableBoolean.Default;
			}
		}

		public void SetNumberFormat(string numberFormat)
		{
			_numberFormat = numberFormat;
		}

		public string GetName()
		{
			return _name;
		}

		public void SetName(string name)
		{
			_name = name;
		}

		public Color GetBackColor()
		{
			return _backColor;
		}

		public Color GetFontColor()
		{
			return _fontColor;
		}

		/// <summary>Holds every field that <see cref="HasSameFormatting"/> compares when either side has formatting.</summary>
		internal readonly record struct FormattingKey(string NumberFormat, Color BackColor, Color FontColor, InheritableBoolean Bold, InheritableBoolean Italic, string FontName, decimal FontSize, InheritableBoolean Strikeout, InheritableBoolean Underline);

		internal FormattingKey GetFormattingKey()
		{
			return new FormattingKey(_numberFormat, _backColor, _fontColor, _bold, _italic, _fontName, _fontSize, _strikeout, _underline);
		}

		public bool HasSameFormatting(ExportAppearanceState exportAppearanceState)
		{
			if (_numberFormat != exportAppearanceState._numberFormat)
			{
				return false;
			}
			if (_hasFormatting || exportAppearanceState._hasFormatting)
			{
				if (_backColor != exportAppearanceState._backColor)
				{
					return false;
				}
				if (_fontColor != exportAppearanceState._fontColor)
				{
					return false;
				}
				if (_bold != exportAppearanceState._bold)
				{
					return false;
				}
				if (_italic != exportAppearanceState._italic)
				{
					return false;
				}
				if (_fontName != exportAppearanceState._fontName)
				{
					return false;
				}
				if (_fontSize != exportAppearanceState._fontSize)
				{
					return false;
				}
				if (_strikeout != exportAppearanceState._strikeout)
				{
					return false;
				}
				if (_underline != exportAppearanceState._underline)
				{
					return false;
				}
			}
			return true;
		}

		internal bool IsEmpty()
		{
			if (_numberFormat != null && _numberFormat.Length > 0)
			{
				return false;
			}
			if (_hasFormatting)
			{
				if (!_backColor.IsEmpty)
				{
					return false;
				}
				if (!_fontColor.IsEmpty)
				{
					return false;
				}
				if (_bold != InheritableBoolean.Default)
				{
					return false;
				}
				if (_italic != InheritableBoolean.Default)
				{
					return false;
				}
				if (_fontName != null && _fontName.Length > 0)
				{
					return false;
				}
				if (_fontSize > 0m)
				{
					return false;
				}
				if (_strikeout != InheritableBoolean.Default)
				{
					return false;
				}
				if (_underline != InheritableBoolean.Default)
				{
					return false;
				}
			}
			return true;
		}

		internal void ProcessXmlWriter(XmlWriter xmlWriter, string prefix, string ns)
		{
			if (IsEmpty())
			{
				return;
			}
			xmlWriter.WriteStartElement("Style");
			xmlWriter.WriteStartAttribute(prefix, "ID", ns);
			xmlWriter.WriteString(_name);
			xmlWriter.WriteEndAttribute();
			if (_numberFormat != null && _numberFormat.Length > 0)
			{
				xmlWriter.WriteStartElement(prefix, "NumberFormat", ns);
				xmlWriter.WriteAttributeString(prefix, "Format", ns, _numberFormat);
				xmlWriter.WriteEndElement();
			}
			if (!string.IsNullOrEmpty(_horizontalAlignment))
			{
				xmlWriter.WriteStartElement(prefix, "Alignment", ns);
				xmlWriter.WriteAttributeString(prefix, "Horizontal", ns, _horizontalAlignment);
				xmlWriter.WriteEndElement();
			}
			if (_hasFormatting)
			{
				if (!_backColor.IsEmpty && ToHexColor(_backColor) != "#ffffff")
				{
					xmlWriter.WriteStartElement(prefix, "Interior", ns);
					xmlWriter.WriteAttributeString(prefix, "Color", ns, ToHexColor(_backColor));
					xmlWriter.WriteAttributeString(prefix, "Pattern", ns, "Solid");
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteStartElement(prefix, "Font", ns);
				if (_fontName != null)
				{
					xmlWriter.WriteAttributeString(prefix, "FontName", ns, _fontName);
				}
				if (_fontSize > 0m)
				{
					xmlWriter.WriteAttributeString(prefix, "Size", ns, _fontSize.ToString(CultureInfo.InvariantCulture));
				}
				if (_bold != InheritableBoolean.Default)
				{
					xmlWriter.WriteAttributeString(prefix, "Bold", ns, "1");
				}
				if (_italic != InheritableBoolean.Default)
				{
					xmlWriter.WriteAttributeString(prefix, "Italic", ns, "1");
				}
				if (_underline != InheritableBoolean.Default)
				{
					xmlWriter.WriteAttributeString(prefix, "Underline", ns, "Single");
				}
				if (_strikeout != InheritableBoolean.Default)
				{
					xmlWriter.WriteAttributeString(prefix, "StrikeThrough", ns, "1");
				}
				if (!_fontColor.IsEmpty)
				{
					xmlWriter.WriteAttributeString(prefix, "Color", ns, ToHexColor(_fontColor));
				}
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
		}

		internal string ToHexColor(Color color)
		{
			if (color.IsEmpty)
			{
				return "";
			}
			color = Color.FromArgb(color.ToArgb());
			return "#" + ToHexByte(color.R) + ToHexByte(color.G) + ToHexByte(color.B);
		}

		private string ToHexByte(int value)
		{
			string text = value.ToString("x");
			if (text.Length == 0)
			{
				return "00";
			}
			if (text.Length == 1)
			{
				return "0" + text;
			}
			if (text.Length == 2)
			{
				return text;
			}
			throw new ArgumentException();
		}

		internal void ProcessTextAlignment(TextAlignment textAlignment)
		{
			switch (textAlignment)
			{
			case TextAlignment.Near:
				_horizontalAlignment = "Left";
				break;
			case TextAlignment.Center:
				_horizontalAlignment = "Center";
				break;
			case TextAlignment.Far:
				_horizontalAlignment = "Right";
				break;
			}
		}

		internal void ApplyFormatStyle(GridFormatStyle gridFormatStyle, MxDataGrid dataGrid)
		{
			ApplyFont(gridFormatStyle.ResolveFont(dataGrid.Font));
			_backColor = gridFormatStyle.BackColor;
			_fontColor = gridFormatStyle.ForeColor;
		}
	}
}
