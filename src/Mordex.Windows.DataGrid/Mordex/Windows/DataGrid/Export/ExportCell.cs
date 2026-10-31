using System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Mordex.Windows.DataGrid.Export
{
	internal class ExportCell
	{
		private string _cellText;

		private object _cellValue;

		private ExportAppearanceState _exportAppearanceState;

		private string _styleId;

		private int _mergeAcross;

		private TypeCode _typeCode;

		private bool _isTextOnly;

		private ExportValueKind _exportValueKind;

		private string _numberFormat;

		internal ExportCell()
		{
			_exportAppearanceState = new ExportAppearanceState();
			_mergeAcross = 0;
			_numberFormat = "";
		}

		internal void SetIsTextOnly(bool condition)
		{
			_isTextOnly = condition;
		}

		internal void SetTypeCode(TypeCode typeCode)
		{
			_typeCode = typeCode;
		}

		internal void SetExportValueKind(ExportValueKind exportValueKind)
		{
			_exportValueKind = exportValueKind;
		}

		/// <summary>Excel keeps 15 significant digits. Integer and decimal values with more digits lose precision there.</summary>
		internal static bool IsExactInExcel(object value)
		{
			string digits;
			switch (value)
			{
			case long number:
				digits = number.ToString(CultureInfo.InvariantCulture).TrimStart('-');
				break;
			case ulong number:
				digits = number.ToString(CultureInfo.InvariantCulture);
				break;
			case decimal number:
				digits = number.ToString(CultureInfo.InvariantCulture).TrimStart('-').Replace(".", "");
				break;
			default:
				return true;
			}
			digits = digits.TrimStart('0').TrimEnd('0');
			return digits.Length <= 15;
		}

		/// <summary>Chooses a text cell for a finite number when the column asks for text or Excel would round it.</summary>
		internal static bool WritesNumberAsText(object value, ExportValueKind exportValueKind)
		{
			switch (exportValueKind)
			{
			case ExportValueKind.Text:
				return true;
			case ExportValueKind.Number:
				return false;
			default:
				return !IsExactInExcel(value);
			}
		}

		internal int GetMergeAcross()
		{
			return _mergeAcross;
		}

		internal void SetMergeAcross(int value)
		{
			_mergeAcross = value;
		}

		internal ExportAppearanceState GetAppearance()
		{
			return _exportAppearanceState;
		}

		internal void SetStyleId(string exportStateText)
		{
			_styleId = exportStateText;
		}

		// Characters that XML 1.0 does not allow make a file that Excel cannot open. They are removed.
		internal static string XmlSafe(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			StringBuilder builder = null;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (XmlConvert.IsXmlChar(c))
				{
					builder?.Append(c);
				}
				else if (i + 1 < text.Length && XmlConvert.IsXmlSurrogatePair(text[i + 1], c))
				{
					builder?.Append(c).Append(text[i + 1]);
					i++;
				}
				else
				{
					builder ??= new StringBuilder(text, 0, i, text.Length);
				}
			}
			return builder == null ? text : builder.ToString();
		}

		public string GetCellText()
		{
			return _cellText;
		}

		public void SetCellText(string text)
		{
			_cellText = text;
		}

		public object GetCellValue()
		{
			return _cellValue;
		}

		public void SetCellValue(object value)
		{
			_cellValue = value;
		}

		public void SetNumberFormat(string formatString)
		{
			_numberFormat = formatString;
		}

		internal void ApplyNumberFormat()
		{
			if (_isTextOnly)
			{
				_exportAppearanceState.SetNumberFormat(null);
			}
			else
			{
				if (_numberFormat == null)
				{
					return;
				}
				if (_typeCode == TypeCode.DateTime)
				{
					if (_numberFormat.Length == 0)
					{
						_exportAppearanceState.SetNumberFormat("m/d/yy\\ h:mm:ss;@");
					}
					else
					{
						_exportAppearanceState.SetNumberFormat("Short Date");
					}
				}
				else if (_numberFormat.StartsWith("C") || _numberFormat.StartsWith("c"))
				{
					_exportAppearanceState.SetNumberFormat("Currency");
				}
				else if (_numberFormat.StartsWith("P") || _numberFormat.StartsWith("p"))
				{
					_exportAppearanceState.SetNumberFormat("Percent");
				}
				else if (!string.IsNullOrEmpty(_numberFormat))
				{
					_exportAppearanceState.SetNumberFormat("Standard");
				}
				else
				{
					_exportAppearanceState.SetNumberFormat("");
				}
			}
		}

		internal void ProcessXmlTextWriter(XmlTextWriter xmlTextWriter, string prefix, string ns, int value)
		{
			xmlTextWriter.WriteStartElement("Cell");
			if (value != 0)
			{
				xmlTextWriter.WriteAttributeString(prefix, "Index", ns, string.Concat(value));
			}
			if (_styleId != null && _styleId.Length > 0)
			{
				xmlTextWriter.WriteAttributeString(prefix, "StyleID", ns, _styleId);
			}
			if (_mergeAcross > 0)
			{
				xmlTextWriter.WriteAttributeString(prefix, "MergeAcross", ns, string.Concat(_mergeAcross));
			}
			if (_isTextOnly)
			{
				xmlTextWriter.Formatting = Formatting.None;
				xmlTextWriter.WriteStartElement("Data");
				xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "String");
				xmlTextWriter.WriteString(XmlSafe(GetCellText()));
				xmlTextWriter.WriteEndElement();
			}
			else
			{
				xmlTextWriter.Formatting = Formatting.None;
				object item = GetCellValue();
				if (item != null && item != DBNull.Value)
				{
					xmlTextWriter.WriteStartElement("Data");
					if (_isTextOnly)
					{
						xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "String");
						xmlTextWriter.WriteString(XmlSafe(GetCellText()));
					}
					else
					{
						switch (_typeCode)
						{
						case TypeCode.Boolean:
							xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "Boolean");
							if (item is bool)
							{
								if ((bool)item)
								{
									xmlTextWriter.WriteString("1");
								}
								else
								{
									xmlTextWriter.WriteString("0");
								}
							}
							else
							{
								xmlTextWriter.WriteString("0");
							}
							break;
						case TypeCode.DateTime:
						{
							DateTime otherDateTime = new DateTime(1900, 1, 1);
							if (!(item is DateTime dateTime) || dateTime < otherDateTime)
							{
								xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "String");
								xmlTextWriter.WriteString(XmlSafe(GetCellText()));
							}
							else
							{
								xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "DateTime");
								xmlTextWriter.WriteString(dateTime.ToString("s", CultureInfo.InvariantCulture));
							}
							break;
						}
						case TypeCode.SByte:
						case TypeCode.Byte:
						case TypeCode.Int16:
						case TypeCode.UInt16:
						case TypeCode.Int32:
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							bool condition = false;
							if (item is double)
							{
								condition = !double.IsFinite((double)item);
							}
							if (item is float)
							{
								condition = !float.IsFinite((float)item);
							}
							if (item is IFormattable && !condition && WritesNumberAsText(item, _exportValueKind))
							{
								xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "String");
								xmlTextWriter.WriteString(((IFormattable)item).ToString("", CultureInfo.InvariantCulture));
							}
							else if (item is IFormattable && !condition)
							{
								xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "Number");
								xmlTextWriter.WriteString(((IFormattable)item).ToString("", CultureInfo.InvariantCulture));
							}
							else
							{
								xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "String");
								xmlTextWriter.WriteString(XmlSafe(GetCellText()));
							}
							break;
						}
						default:
							xmlTextWriter.WriteAttributeString(prefix, "Type", ns, "String");
							xmlTextWriter.WriteString(XmlSafe(GetCellText()));
							break;
						}
					}
					xmlTextWriter.WriteEndElement();
				}
			}
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.Formatting = Formatting.Indented;
		}
	}
}
