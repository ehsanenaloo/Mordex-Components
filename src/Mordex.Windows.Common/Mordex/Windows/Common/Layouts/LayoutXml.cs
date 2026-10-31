using System.IO;
using System.Xml;

namespace Mordex.Windows.Common.Layouts
{
	/// <summary>Loads layout text without DTD processing or external resolvers, within <see cref="LayoutImportLimits"/>.</summary>
	internal static class LayoutXml
	{
		internal static XmlDocument LoadString(string xml)
		{
			return LayoutImportLimits.LoadString(xml);
		}

		internal static XmlDocument LoadStream(Stream stream)
		{
			return LayoutImportLimits.LoadStream(stream);
		}
	}
}
