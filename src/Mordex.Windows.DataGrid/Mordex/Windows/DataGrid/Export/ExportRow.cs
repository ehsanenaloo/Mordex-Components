using System.Collections.Generic;
using System.Xml;

namespace Mordex.Windows.DataGrid.Export
{
	internal class ExportRow
	{
		private List<ExportCell> _cells;

		internal int _rowKind;

		internal ExportRow()
		{
			_cells = new List<ExportCell>();
		}

		internal List<ExportCell> GetCells()
		{
			return _cells;
		}

		internal void ProcessXmlTextWriter(XmlTextWriter xmlTextWriter, string text, string otherText)
		{
			xmlTextWriter.WriteStartElement("Row");
			for (int i = 0; i < _cells.Count; i++)
			{
				int value = 0;
				if (_rowKind > 1 && i == 0)
				{
					value = i + _rowKind;
				}
				ExportCell thisValue = _cells[i];
				thisValue.ProcessXmlTextWriter(xmlTextWriter, text, otherText, value);
			}
			xmlTextWriter.WriteEndElement();
		}

		internal void AssignStyleIds(ExportStyleTable exportState, bool condition)
		{
			foreach (ExportCell item in _cells)
			{
				item.GetAppearance().SetHasFormatting(condition);
				item.ApplyNumberFormat();
				item.SetStyleId(exportState.GetOrAddStyleName(item.GetAppearance()));
			}
		}
	}
}
