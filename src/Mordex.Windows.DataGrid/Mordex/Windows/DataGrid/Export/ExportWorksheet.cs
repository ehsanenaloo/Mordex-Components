using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace Mordex.Windows.DataGrid.Export
{
	/// <summary>
	/// Collects one worksheet. SpreadsheetML needs every style before the first row, so each row is given its
	/// style IDs and written to a buffer as soon as it is added, and only the styles stay in memory. The final
	/// document is written when all rows are known.
	/// </summary>
	internal sealed class ExportWorksheet : IDisposable
	{
		private const string Prefix = "ss";

		private const string SpreadsheetNamespace = "urn:schemas-microsoft-com:office:spreadsheet";

		private const int MaxPaletteColors = 40;

		private readonly List<ExportColumn> _columns = new List<ExportColumn>();

		private readonly ExportStyleTable _styleTable = new ExportStyleTable();

		private readonly bool _includeFormatStyle;

		private readonly CancellationToken _cancellationToken;

		private readonly ExportRowBuffer _rowBuffer;

		private readonly XmlTextWriter _rowWriter;

		private long _rowsStart = -1;

		private int _rowCount;

		internal ExportWorksheet(bool includeFormatStyle, CancellationToken cancellationToken, ExportRowBuffer rowBuffer)
		{
			_includeFormatStyle = includeFormatStyle;
			_cancellationToken = cancellationToken;
			_rowBuffer = rowBuffer;
			// The rows are written at the same depth and with the same namespaces as in the final document, so their
			// bytes can be copied into it unchanged.
			_rowWriter = new XmlTextWriter(_rowBuffer, Encoding.UTF8);
			Configure(_rowWriter);
			StartDocument(_rowWriter);
			_rowWriter.WriteStartElement(Prefix, "Worksheet", SpreadsheetNamespace);
			_rowWriter.WriteStartElement("Table");
		}

		internal List<ExportColumn> GetColumns()
		{
			return _columns;
		}

		internal void AddRow(ExportRow row)
		{
			_cancellationToken.ThrowIfCancellationRequested();
			row.AssignStyleIds(_styleTable, _includeFormatStyle);
			if (_rowsStart < 0)
			{
				_rowWriter.Flush();
				// The open start tag of the table is closed with one '>' byte when the first row is written.
				_rowsStart = _rowBuffer.Position + 1;
			}
			row.ProcessXmlTextWriter(_rowWriter, Prefix, SpreadsheetNamespace);
			_rowCount++;
		}

		/// <summary>
		/// Writes the workbook to the target. This is the commit point: the target is not touched before it, so a cancel
		/// or failure up to here leaves the target unchanged. From here on, the token is still checked before each block
		/// of buffered rows, but a cancel or failure leaves an incomplete workbook in the target.
		/// </summary>
		internal void WriteDocument(Stream target, GridExporter gridExporter)
		{
			_cancellationToken.ThrowIfCancellationRequested();
			_rowWriter.Flush();
			long rowsEnd = _rowBuffer.Position;
			XmlTextWriter writer = new XmlTextWriter(new Internal.LeaveOpenStream(target), Encoding.UTF8);
			Configure(writer);
			writer.WriteStartDocument();
			if (gridExporter.IncludeExcelProcessingInstruction)
			{
				writer.WriteProcessingInstruction("mso-application", "progid='Excel.Sheet'");
			}
			StartDocument(writer);
			WritePalette(writer);
			WriteStyles(writer);
			writer.WriteStartElement(Prefix, "Worksheet", SpreadsheetNamespace);
			writer.WriteStartAttribute(Prefix, "Name", SpreadsheetNamespace);
			writer.WriteString(ExportCell.XmlSafe(gridExporter.SheetName));
			writer.WriteEndAttribute();
			if (_rowCount > 0)
			{
				writer.WriteStartElement("Table");
				foreach (ExportColumn column in _columns)
				{
					column.ProcessXmlTextWriter(writer, Prefix, SpreadsheetNamespace);
				}
				if (_columns.Count == 0)
				{
					writer.WriteRaw("");
				}
				writer.Flush();
				_rowBuffer.CopyRange(_rowsStart, rowsEnd, target, _cancellationToken);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndDocument();
			// The writer is closed only on success. Closing it after a failure would add end tags after the bytes that
			// were already written, and could throw again and hide the first error. It holds no other resources.
			writer.Close();
		}

		private static void Configure(XmlTextWriter writer)
		{
			writer.Indentation = 1;
			writer.Formatting = Formatting.Indented;
			writer.Namespaces = true;
		}

		private static void StartDocument(XmlTextWriter writer)
		{
			writer.WriteStartElement("", "Workbook", SpreadsheetNamespace);
			writer.WriteAttributeString("xmlns", Prefix, null, SpreadsheetNamespace);
			writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
			writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
			writer.WriteAttributeString("xmlns", "html", null, "http://www.w3.org/TR/REC-html40");
		}

		private void WritePalette(XmlTextWriter writer)
		{
			List<string> colors = new List<string>();
			HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
			foreach (ExportAppearanceState style in _styleTable.GetArrayList())
			{
				if (style.HasFormatting())
				{
					foreach (string color in new[] { style.ToHexColor(style.GetBackColor()), style.ToHexColor(style.GetFontColor()) })
					{
						if (color.Length > 0 && seen.Add(color))
						{
							colors.Add(color);
						}
					}
				}
			}
			if (colors.Count == 0)
			{
				return;
			}
			writer.WriteStartElement("OfficeDocumentSettings", "urn:schemas-microsoft-com:office:office");
			writer.WriteStartElement("Colors");
			for (int i = 0; i < colors.Count && i < MaxPaletteColors; i++)
			{
				writer.WriteStartElement("Color");
				writer.WriteElementString("Index", (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
				writer.WriteElementString("RGB", colors[i]);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private void WriteStyles(XmlTextWriter writer)
		{
			if (_styleTable.GetArrayList().Count <= 0)
			{
				return;
			}
			writer.WriteStartElement("Styles");
			foreach (ExportAppearanceState style in _styleTable.GetArrayList())
			{
				style.ProcessXmlWriter(writer, Prefix, SpreadsheetNamespace);
			}
			writer.WriteEndElement();
		}

		public void Dispose()
		{
			_rowWriter.Close();
			_rowBuffer.Dispose();
		}
	}
}
