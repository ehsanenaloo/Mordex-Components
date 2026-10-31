using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Mordex.Windows.Common.Layouts
{
	public class LayoutWriter
	{
		private const int IndentSize = 4;

		private readonly List<PropertyValue> _values = new List<PropertyValue>();

		private readonly bool _isRoot;

		public LayoutWriter(PropertyValue root, bool isRoot)
		{
			_isRoot = isRoot;
			AddValue(root);
		}

		public LayoutWriter()
		{
			_isRoot = false;
		}

		public void AddValue(string propertyName, object value)
		{
			_values.Add(new PropertyValue(propertyName, value));
		}

		public void AddValue(PropertyValue propertyValue)
		{
			_values.Add(propertyValue);
		}

		public void FillStream(Stream stream)
		{
			XmlWriterSettings settings = CreateSettings(Encoding.UTF8);
			settings.Indent = true;
			settings.IndentChars = new string(' ', IndentSize);
			using (XmlWriter writer = XmlWriter.Create(stream, settings))
			{
				Write(writer, null);
			}
			stream.Flush();
		}

		public static void StoreInLayout(string rootName, LayoutWriter layoutWriter, IMordexLayout layout)
		{
			layout.LayoutReferences.Clear();
			string content;
			using (StringWriter text = new StringWriter(CultureInfo.InvariantCulture))
			{
				using XmlWriter xml = XmlWriter.Create(text, CreateSettings(null));
				layoutWriter.Write(xml, layout);
				content = text.ToString();
			}
			LayoutWriter rootWriter = new LayoutWriter();
			rootWriter.AddValue(rootName, new RawXML(content));
			using (StringWriter text = new StringWriter(CultureInfo.InvariantCulture))
			{
				using XmlWriter xml = XmlWriter.Create(text, CreateSettings(null));
				rootWriter.Write(xml, null);
				layout.LayoutString = text.ToString();
			}
			ApplyReferenceIds(layout);
		}

		private static XmlWriterSettings CreateSettings(Encoding encoding)
		{
			XmlWriterSettings settings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Auto,
				NewLineHandling = NewLineHandling.None,
				CloseOutput = false
			};
			if (encoding != null)
			{
				settings.Encoding = encoding;
			}
			return settings;
		}

		private static void ApplyReferenceIds(IMordexLayout layout)
		{
			if (layout.LayoutReferences.Count > 0)
			{
				XmlDocument document = LayoutXml.LoadString(layout.LayoutString);
				ApplyReferenceIds(layout.LayoutReferences, document.ChildNodes);
				layout.LayoutString = document.InnerXml;
			}
		}

		private static void ApplyReferenceIds(LayoutReferenceCollection references, XmlNodeList nodes)
		{
			foreach (XmlNode node in nodes)
			{
				XmlNode referenceAttribute = node.Attributes?.GetNamedItem("Reference");
				if (referenceAttribute != null)
				{
					LayoutReference reference = references[referenceAttribute.Value];
					if (reference != null)
					{
						referenceAttribute.Value = GetNodePath(node);
						reference.ReferenceId = referenceAttribute.Value;
					}
				}
				else
				{
					ApplyReferenceIds(references, node.ChildNodes);
				}
			}
		}

		private static string GetNodePath(XmlNode node)
		{
			if (node.ParentNode == null || node.ParentNode.NodeType == XmlNodeType.Document)
			{
				return node.Name;
			}
			return GetNodePath(node.ParentNode) + "." + node.Name;
		}

		public override string ToString()
		{
			using MemoryStream stream = new MemoryStream();
			using (XmlWriter writer = XmlWriter.Create(stream, CreateSettings(Encoding.UTF8)))
			{
				Write(writer, null);
			}
			stream.Position = 0L;
			using StreamReader reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		internal void Write(XmlWriter writer, IMordexLayout layout)
		{
			if (_isRoot)
			{
				writer.WriteStartDocument();
			}
			foreach (PropertyValue item in _values)
			{
				item.Write(writer, layout);
			}
			if (_isRoot)
			{
				writer.WriteEndDocument();
			}
			writer.Flush();
		}
	}
}
