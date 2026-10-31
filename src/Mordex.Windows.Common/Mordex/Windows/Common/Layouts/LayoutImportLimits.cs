using System;
using System.IO;
using System.Threading;
using System.Xml;

namespace Mordex.Windows.Common.Layouts
{
	/// <summary>
	/// Limits for layout XML that is read from a file, stream, reader or string. Every control uses the same
	/// limits. A layout over a limit is rejected with an <see cref="XmlException"/> before the control changes.
	/// </summary>
	public static class LayoutImportLimits
	{
		private static long s_maxCharacters = 64L * 1024 * 1024;

		private static int s_maxDepth = 256;

		private static int s_maxElements = 1_000_000;

		private static int s_maxImageBytes = 32 * 1024 * 1024;

		private static long s_maxImagePixels = 64L * 1024 * 1024;

		/// <summary>Largest number of characters in names, attribute values and text of one layout document.</summary>
		public static long MaxCharacters
		{
			get { return Interlocked.Read(ref s_maxCharacters); }
			set { Interlocked.Exchange(ref s_maxCharacters, Positive(value)); }
		}

		/// <summary>Deepest element nesting allowed in one layout document.</summary>
		public static int MaxDepth
		{
			get { return Volatile.Read(ref s_maxDepth); }
			set { Volatile.Write(ref s_maxDepth, (int)Positive(value)); }
		}

		/// <summary>Largest number of elements in one layout document.</summary>
		public static int MaxElements
		{
			get { return Volatile.Read(ref s_maxElements); }
			set { Volatile.Write(ref s_maxElements, (int)Positive(value)); }
		}

		/// <summary>Largest encoded size, in bytes, of one image stored in a layout.</summary>
		public static int MaxImageBytes
		{
			get { return Volatile.Read(ref s_maxImageBytes); }
			set { Volatile.Write(ref s_maxImageBytes, (int)Positive(value)); }
		}

		/// <summary>Largest width times height of one image stored in a layout.</summary>
		public static long MaxImagePixels
		{
			get { return Interlocked.Read(ref s_maxImagePixels); }
			set { Interlocked.Exchange(ref s_maxImagePixels, Positive(value)); }
		}

		private static long Positive(long value)
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), "The limit must be greater than zero.");
			}
			return value;
		}

		/// <summary>
		/// Reads a layout document from the reader with DTD processing off, no external resolver and the limits
		/// above. The caller's reader is not closed.
		/// </summary>
		public static XmlDocument LoadDocument(XmlReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}
			// The wrapper rejects a DTD even when the caller's reader allows one. It is not disposed because that
			// would close the caller's reader.
			XmlReader safeReader = XmlReader.Create(reader, CreateSettings());
			return Build(safeReader);
		}

		internal static XmlDocument LoadString(string xml)
		{
			if (xml == null)
			{
				throw new ArgumentNullException(nameof(xml));
			}
			if (xml.Length > MaxCharacters)
			{
				throw TooLarge("characters", MaxCharacters);
			}
			using StringReader text = new StringReader(xml);
			using XmlReader reader = XmlReader.Create(text, CreateSettings());
			return Build(reader);
		}

		internal static XmlDocument LoadStream(Stream stream)
		{
			using XmlReader reader = XmlReader.Create(stream, CreateSettings());
			return Build(reader);
		}

		/// <summary>True when an encoded image of this length, in base64 characters, is within the limit.</summary>
		internal static bool IsImageTextAllowed(int base64Length)
		{
			return (long)base64Length / 4 * 3 <= MaxImageBytes;
		}

		internal static bool IsImageSizeAllowed(int width, int height)
		{
			return width > 0 && height > 0 && (long)width * height <= MaxImagePixels;
		}

		private static XmlReaderSettings CreateSettings()
		{
			return new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Prohibit,
				XmlResolver = null,
				MaxCharactersInDocument = MaxCharacters,
				MaxCharactersFromEntities = 1024
			};
		}

		private static XmlException TooLarge(string what, long limit)
		{
			return new XmlException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "The layout has more {0} than the limit of {1}.", what, limit));
		}

		// Builds the document node by node so the limits are checked before each allocation. A reader that wraps
		// another reader does not apply MaxCharactersInDocument, so characters are counted here as well.
		private static XmlDocument Build(XmlReader reader)
		{
			long maxCharacters = MaxCharacters;
			int maxDepth = MaxDepth;
			int maxElements = MaxElements;
			long characters = 0;
			int elements = 0;
			XmlDocument document = new XmlDocument { XmlResolver = null };
			XmlNode current = document;
			void Count(string text)
			{
				characters += text.Length;
				if (characters > maxCharacters)
				{
					throw TooLarge("characters", maxCharacters);
				}
			}
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
				case XmlNodeType.Element:
				{
					if (++elements > maxElements)
					{
						throw TooLarge("elements", maxElements);
					}
					if (reader.Depth >= maxDepth)
					{
						throw TooLarge("levels of nesting", maxDepth);
					}
					Count(reader.Name);
					XmlElement element = document.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
					bool isEmpty = reader.IsEmptyElement;
					while (reader.MoveToNextAttribute())
					{
						Count(reader.Name);
						Count(reader.Value);
						XmlAttribute attribute = document.CreateAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
						attribute.Value = reader.Value;
						element.Attributes.Append(attribute);
					}
					reader.MoveToElement();
					current.AppendChild(element);
					if (!isEmpty)
					{
						current = element;
					}
					break;
				}
				case XmlNodeType.EndElement:
					current = current.ParentNode;
					break;
				case XmlNodeType.Text:
					Count(reader.Value);
					current.AppendChild(document.CreateTextNode(reader.Value));
					break;
				case XmlNodeType.CDATA:
					Count(reader.Value);
					current.AppendChild(document.CreateCDataSection(reader.Value));
					break;
				case XmlNodeType.SignificantWhitespace:
					Count(reader.Value);
					current.AppendChild(document.CreateSignificantWhitespace(reader.Value));
					break;
				}
			}
			if (document.DocumentElement == null)
			{
				throw new XmlException("The layout has no root element.");
			}
			return document;
		}
	}
}
