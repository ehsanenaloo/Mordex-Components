using System.Globalization;
using System.Xml;

namespace Mordex.Windows.DataGrid.Export
{
	internal class ExportColumn
	{
		private float _width;

		internal ExportColumn(float width)
		{
			if (width > 1000f)
			{
				width = 1000f;
			}
			_width = width;
		}

		internal void ProcessXmlTextWriter(XmlTextWriter xmlTextWriter, string prefix, string ns)
		{
			xmlTextWriter.WriteStartElement("Column");
			xmlTextWriter.WriteAttributeString(prefix, "AutoFit", ns, "0");
			xmlTextWriter.WriteAttributeString(prefix, "Width", ns, _width.ToString("", CultureInfo.InvariantCulture));
			xmlTextWriter.WriteEndElement();
		}
	}
}
