using System;
using System.Buffers.Binary;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Mordex.Windows.Common.Layouts
{
	[ComVisible(false)]
	public class PropertyValue
	{
		private const string StreamTypeAttribute = "StreamType";

		private const string ImageStreamType = "Image";

		private const string IconStreamType = "Icon";

		private const string ConverterStreamType = "Converter";

		private const string LayoutVersion = "1.1";

		private const int BytesPerPixel = 4;

		private const int AlphaOffset = 3;

		private const int IconDirectorySize = 6;

		private const int IconEntrySize = 16;

		private const int PngHeaderSize = 24;

		private const int BitmapHeaderSize = 12;

		private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

		internal XmlNode _xmlNode;

		// When set, images, icons and converter values are stored inline instead of as layout references.
		internal bool _embedStreams;

		private IMordexLayout _layout;

		private string _layoutType;

		private string _name = "";

		private object _value;

		public string LayoutType
		{
			get
			{
				return _layoutType;
			}
			set
			{
				_layoutType = value;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public static PropertyValue FromXmlNode(XmlNode node, IMordexLayout layout)
		{
			return FromOwnedNode(node.CloneNode(deep: true), layout);
		}

		// Keeps the node without a copy. Only for nodes that nothing else changes, such as the children of a node
		// this class already copied; copying again at every level made deep layouts cost quadratic time.
		internal static PropertyValue FromOwnedNode(XmlNode node, IMordexLayout layout)
		{
			PropertyValue propertyValue = new PropertyValue(XmlConvert.DecodeName(node.Name), null);
			propertyValue._layout = layout;
			propertyValue._xmlNode = node;
			return propertyValue;
		}

		public PropertyValue(string name, object value)
		{
			_name = name;
			_value = value;
		}

		public void DeserializeObject(object targetInstance)
		{
			if (_xmlNode != null && targetInstance is IMordexXmlLayoutsSupport target)
			{
				IMordexLayout layout = _layout;
				if (layout == null && targetInstance is IMordexLayout targetLayout)
				{
					layout = targetLayout;
				}
				target.Deserialize(LayoutReader.FromNode(_xmlNode, layout));
			}
		}

		public LayoutReader GetLayoutReader()
		{
			if (_xmlNode == null)
			{
				return null;
			}
			return LayoutReader.FromNode(_xmlNode, _layout);
		}

		internal static bool IsConversionFailure(Exception ex)
		{
			return ex is FormatException
				|| ex is InvalidCastException
				|| ex is ArgumentException
				|| ex is ArithmeticException
				|| ex is NotSupportedException
				|| ex is InvalidOperationException
				|| ex is XmlException
				|| ex is IOException
				|| ex is ExternalException;
		}

		private object ResolveReference(XmlNode referenceAttribute)
		{
			if (_layout == null)
			{
				return null;
			}
			return _layout.LayoutReferences[referenceAttribute.Value]?.Instance;
		}

		// Reads a value that was stored inline (see _embedStreams).
		internal object ReadEmbeddedValue()
		{
			string streamType = _xmlNode?.Attributes.GetNamedItem(StreamTypeAttribute)?.Value;
			switch (streamType)
			{
			case ImageStreamType:
				return ReadImage(_xmlNode);
			case IconStreamType:
				return ReadIcon(_xmlNode.FirstChild?.Value);
			case ConverterStreamType:
			{
				Type valueType = LayoutTypeResolver.Resolve(_xmlNode.Attributes.GetNamedItem("TypeName")?.Value);
				if (valueType != null && _xmlNode.FirstChild != null)
				{
					TypeConverter converter = TypeDescriptor.GetConverter(valueType);
					if (converter.CanConvertFrom(typeof(string)))
					{
						return converter.ConvertFromInvariantString(_xmlNode.FirstChild.Value);
					}
				}
				return null;
			}
			default:
				return null;
			}
		}

		public object GetValue(Type destinationType)
		{
			if (_xmlNode == null)
			{
				return null;
			}
			XmlNode reference = _xmlNode.Attributes.GetNamedItem("Reference");
			if (reference != null)
			{
				return ResolveReference(reference);
			}
			if (typeof(IMordexXmlLayoutsSupport).IsAssignableFrom(destinationType))
			{
				try
				{
					IMordexXmlLayoutsSupport instance = (IMordexXmlLayoutsSupport)Activator.CreateInstance(destinationType, nonPublic: false);
					DeserializeObject(instance);
					return instance;
				}
				catch (Exception ex) when (IsConversionFailure(ex) || ex is MissingMethodException || ex is System.Reflection.TargetInvocationException)
				{
					// A type that cannot be created or filled is treated as a missing value.
					return null;
				}
			}
			if (destinationType == typeof(RawXML))
			{
				return new RawXML(_xmlNode.InnerXml);
			}
			if (typeof(Image).IsAssignableFrom(destinationType))
			{
				return ReadImage(_xmlNode);
			}
			if (destinationType == typeof(Icon))
			{
				return ReadIcon(_xmlNode.FirstChild?.Value);
			}
			if (destinationType == typeof(char))
			{
				if (_xmlNode.FirstChild == null)
				{
					return null;
				}
				string text = _xmlNode.FirstChild.Value;
				if (text.Length > 1 && text[0] == '\'' && text[text.Length - 1] == '\'')
				{
					return ConvertValue(text.Substring(1, 1), destinationType);
				}
				return ConvertValue(text, destinationType);
			}
			if (_xmlNode.FirstChild == null)
			{
				if (destinationType == typeof(string))
				{
					return "";
				}
				if (destinationType == typeof(Color))
				{
					return Color.Empty;
				}
				return null;
			}
			return ConvertValue(_xmlNode.FirstChild.Value, destinationType);
		}

		private static void WriteAttribute(XmlWriter writer, string name, string value)
		{
			writer.WriteStartAttribute(name, null);
			writer.WriteString(value);
			writer.WriteEndAttribute();
		}

		internal void Write(XmlWriter writer, IMordexLayout layout)
		{
			if (layout == null)
			{
				_embedStreams = true;
			}
			writer.WriteStartElement(XmlConvert.EncodeLocalName(_name));
			if (_layoutType != null)
			{
				WriteAttribute(writer, "LayoutType", _layoutType);
				WriteAttribute(writer, "LayoutVersion", LayoutVersion);
			}
			if (_value != null)
			{
				try
				{
					WriteValue(writer, layout);
				}
				catch (Exception ex) when (IsConversionFailure(ex))
				{
					// A value that cannot be stored is left out so the rest of the layout is still saved.
				}
			}
			writer.WriteEndElement();
		}

		private void WriteValue(XmlWriter writer, IMordexLayout layout)
		{
			// Every step that can fail runs before the first attribute is written, so a failed value leaves an empty element.
			if (_value is IMordexXmlLayoutsSupport layoutSupport)
			{
				LayoutWriter childWriter = new LayoutWriter();
				layoutSupport.Serialize(childWriter);
				if (!string.IsNullOrEmpty(layoutSupport.Identifier))
				{
					WriteAttribute(writer, "ID", layoutSupport.Identifier);
				}
				if (layoutSupport.IsCollection)
				{
					WriteAttribute(writer, "Collection", "true");
					if (layoutSupport.CollectionElementName != null)
					{
						WriteAttribute(writer, "ElementName", layoutSupport.CollectionElementName);
					}
				}
				childWriter.Write(writer, layout);
			}
			else if (_value is RawXML rawXml)
			{
				writer.WriteRaw(rawXml.XML);
			}
			else if (_value is DateTime dateTime)
			{
				writer.WriteString(dateTime.ToString("o", CultureInfo.InvariantCulture));
			}
			else if (Type.GetTypeCode(_value.GetType()) != TypeCode.Object)
			{
				writer.WriteString(ConvertPrimitiveToString(_value, quoteChars: true));
			}
			else if (_value is TimeSpan || _value is Guid)
			{
				writer.WriteString(_value.ToString());
			}
			else if (_embedStreams && _value is Image image)
			{
				using MemoryStream alpha = new MemoryStream();
				using MemoryStream data = EncodeImage(image, alpha);
				WriteAttribute(writer, StreamTypeAttribute, ImageStreamType);
				WriteImage(writer, data, alpha);
			}
			else if (_embedStreams && _value is Icon icon)
			{
				using MemoryStream stream = new MemoryStream();
				icon.Save(stream);
				WriteAttribute(writer, StreamTypeAttribute, IconStreamType);
				writer.WriteBase64(stream.GetBuffer(), 0, (int)stream.Length);
			}
			else
			{
				WriteConvertedValue(writer, layout);
			}
		}

		private void WriteConvertedValue(XmlWriter writer, IMordexLayout layout)
		{
			string text = TryConvertToString(_value);
			if (text != null)
			{
				writer.WriteString(text);
			}
			else if (_embedStreams)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(_value.GetType());
				if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
				{
					string converted = converter.ConvertToInvariantString(_value);
					WriteAttribute(writer, StreamTypeAttribute, ConverterStreamType);
					WriteAttribute(writer, "TypeName", _value.GetType().AssemblyQualifiedName);
					writer.WriteString(converted);
				}
			}
			else
			{
				LayoutReference reference = layout.LayoutReferences.Add(_value);
				WriteAttribute(writer, "Reference", reference.ReferenceId);
			}
		}

		private static string ConvertPrimitiveToString(object value, bool quoteChars)
		{
			switch (value)
			{
			case DateTime dateTime:
				return dateTime.ToString(CultureInfo.InvariantCulture);
			case bool isTrue:
				return isTrue ? bool.TrueString : bool.FalseString;
			case char character when quoteChars:
				return "'" + character.ToString(CultureInfo.InvariantCulture) + "'";
			case IConvertible convertible:
				return convertible.ToString(CultureInfo.InvariantCulture);
			default:
				return Convert.ToString(value, CultureInfo.InvariantCulture);
			}
		}

		private static void WriteImage(XmlWriter writer, MemoryStream data, MemoryStream alpha)
		{
			if (alpha.Length > 0)
			{
				writer.WriteStartElement("TransparencyData");
				writer.WriteBase64(alpha.GetBuffer(), 0, (int)alpha.Length);
				writer.WriteEndElement();
			}
			writer.WriteStartElement("ImageData");
			writer.WriteBase64(data.GetBuffer(), 0, (int)data.Length);
			writer.WriteEndElement();
		}

		// Metafiles and other formats can carry drawing records or external references, so only plain raster formats are read.
		private static bool IsAllowedImageFormat(ImageFormat format)
		{
			Guid id = format.Guid;
			return id == ImageFormat.Png.Guid || id == ImageFormat.Bmp.Guid || id == ImageFormat.MemoryBmp.Guid || id == ImageFormat.Jpeg.Guid || id == ImageFormat.Gif.Guid;
		}

		private static Image CreateImage(Stream stream, byte[] alpha)
		{
			// The header is read without decoding the pixels, so an oversized image is refused before its memory is taken.
			using Image header = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: false);
			if (!LayoutImportLimits.IsImageSizeAllowed(header.Width, header.Height))
			{
				throw new ArgumentException("The image in the layout is larger than the limit.");
			}
			if (!IsAllowedImageFormat(header.RawFormat))
			{
				throw new ArgumentException("The image in the layout has a format that is not allowed.");
			}
			using Bitmap loaded = new Bitmap(header);
			if (alpha != null && alpha.Length > 0)
			{
				return ApplyAlphaChannel(loaded, alpha);
			}
			return new Bitmap(loaded);
		}

		// Replaces the alpha of every pixel with the stored row-major alpha bytes.
		private static Bitmap ApplyAlphaChannel(Bitmap source, byte[] alpha)
		{
			int width = source.Width;
			int height = source.Height;
			Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			Rectangle area = new Rectangle(0, 0, width, height);
			using (Bitmap argbSource = source.Clone(area, PixelFormat.Format32bppArgb))
			{
				BitmapData sourceData = argbSource.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				BitmapData resultData = result.LockBits(area, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				try
				{
					byte[] row = new byte[width * BytesPerPixel];
					for (int y = 0; y < height; y++)
					{
						Marshal.Copy(IntPtr.Add(sourceData.Scan0, y * sourceData.Stride), row, 0, row.Length);
						for (int x = 0; x < width; x++)
						{
							int index = y * width + x;
							row[x * BytesPerPixel + AlphaOffset] = index < alpha.Length ? alpha[index] : byte.MaxValue;
						}
						Marshal.Copy(row, 0, IntPtr.Add(resultData.Scan0, y * resultData.Stride), row.Length);
					}
				}
				finally
				{
					argbSource.UnlockBits(sourceData);
					result.UnlockBits(resultData);
				}
			}
			return result;
		}

		// Encodes the image. Alpha that the chosen format would lose is written to alphaStream.
		private static MemoryStream EncodeImage(Image image, MemoryStream alphaStream)
		{
			MemoryStream encoded = new MemoryStream();
			try
			{
				if (!Image.IsAlphaPixelFormat(image.PixelFormat))
				{
					image.Save(encoded, IsAllowedImageFormat(image.RawFormat) ? image.RawFormat : ImageFormat.Bmp);
				}
				else if (image.RawFormat.Guid == ImageFormat.Png.Guid)
				{
					image.Save(encoded, ImageFormat.Png);
				}
				else
				{
					image.Save(encoded, ImageFormat.Bmp);
					WriteAlphaChannel(image, alphaStream);
				}
			}
			catch (ExternalException)
			{
				// The source format has no encoder, so fall back to a bitmap.
				encoded.SetLength(0L);
				alphaStream.SetLength(0L);
				image.Save(encoded, ImageFormat.Bmp);
				if (Image.IsAlphaPixelFormat(image.PixelFormat))
				{
					WriteAlphaChannel(image, alphaStream);
				}
			}
			encoded.Position = 0L;
			return encoded;
		}

		private static void WriteAlphaChannel(Image original, Stream stream)
		{
			using Bitmap bitmap = new Bitmap(original);
			int width = bitmap.Width;
			int height = bitmap.Height;
			Rectangle area = new Rectangle(0, 0, width, height);
			BitmapData data = bitmap.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			bool hasTransparency = false;
			try
			{
				byte[] row = new byte[width * BytesPerPixel];
				byte[] alphaRow = new byte[width];
				for (int y = 0; y < height; y++)
				{
					Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, row.Length);
					for (int x = 0; x < width; x++)
					{
						byte alpha = row[x * BytesPerPixel + AlphaOffset];
						alphaRow[x] = alpha;
						if (alpha < byte.MaxValue)
						{
							hasTransparency = true;
						}
					}
					stream.Write(alphaRow, 0, width);
				}
			}
			finally
			{
				bitmap.UnlockBits(data);
			}
			if (!hasTransparency)
			{
				stream.SetLength(0L);
			}
		}

		private static string TryConvertToString(object value)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
			if (!converter.CanConvertFrom(typeof(string)) || !converter.CanConvertTo(typeof(string)))
			{
				return null;
			}
			try
			{
				return (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(string));
			}
			catch (Exception ex) when (IsConversionFailure(ex))
			{
				return null;
			}
		}

		private static object ConvertWithTypeConverter(string text, Type destinationType)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
			if (!converter.CanConvertFrom(typeof(string)))
			{
				return null;
			}
			return converter.ConvertFrom(null, CultureInfo.InvariantCulture, text);
		}

		public static string ConvertObjectToString(object value)
		{
			if (value == null || Convert.IsDBNull(value))
			{
				return "{null}";
			}
			if (Type.GetTypeCode(value.GetType()) != TypeCode.Object)
			{
				return ConvertPrimitiveToString(value, quoteChars: false);
			}
			if (value is Guid || value is TimeSpan)
			{
				return value.ToString();
			}
			if (value is IList list)
			{
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < list.Count; i++)
				{
					if (i > 0)
					{
						builder.Append(", ");
					}
					builder.Append(ConvertObjectToString(list[i]));
				}
				return builder.ToString();
			}
			return TryConvertToString(value) ?? throw new InvalidOperationException(string.Format("Unable to store type {0} into Xml", value.GetType()));
		}

		public static object ConvertValue(string value, Type destinationType)
		{
			return ConvertValue(value, destinationType, CultureInfo.InvariantCulture);
		}

		public static object ConvertValue(string value, Type destinationType, CultureInfo cultureInfo)
		{
			if (value == null)
			{
				return null;
			}
			TypeCode typeCode = Type.GetTypeCode(destinationType);
			if (typeCode != TypeCode.Object)
			{
				if (destinationType.IsEnum)
				{
					return Enum.Parse(destinationType, value, ignoreCase: true);
				}
				if (typeCode == TypeCode.Boolean)
				{
					// Text that is not True or False is a format error, like bad text for a number, so a reader can
					// use its default instead of a silent false.
					if (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
					if (string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					throw new FormatException(string.Format(CultureInfo.InvariantCulture, "'{0}' is not a Boolean value.", value));
				}
				if (typeCode == TypeCode.DateTime)
				{
					// Reads the round-trip format with its kind, and the older invariant format without milliseconds.
					return DateTime.Parse(value, cultureInfo, DateTimeStyles.RoundtripKind);
				}
				return Convert.ChangeType(value, typeCode, cultureInfo);
			}
			if (destinationType == typeof(TimeSpan))
			{
				return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
			}
			if (destinationType == typeof(Guid))
			{
				return Guid.Parse(value);
			}
			if (destinationType == typeof(Color) && value.Length == 0)
			{
				return Color.Empty;
			}
			if (destinationType == typeof(object))
			{
				return value;
			}
			try
			{
				return ConvertWithTypeConverter(value, destinationType);
			}
			catch (Exception ex) when (IsConversionFailure(ex))
			{
				// The text does not describe a value of the requested type.
				return null;
			}
		}

		private static Icon ReadIcon(string encodedIcon)
		{
			if (string.IsNullOrEmpty(encodedIcon) || !LayoutImportLimits.IsImageTextAllowed(encodedIcon.Length))
			{
				return null;
			}
			try
			{
				byte[] data = Convert.FromBase64String(encodedIcon);
				if (!IsIconSizeAllowed(data))
				{
					return null;
				}
				using MemoryStream stream = new MemoryStream(data);
				return new Icon(stream);
			}
			catch (Exception ex) when (IsConversionFailure(ex))
			{
				return null;
			}
		}

		// Reads the icon directory and each frame header without decoding pixels. The directory can only state sizes
		// up to 256, so the size is taken from the frame itself (PNG or bitmap header). All frames together must stay
		// within the pixel limit.
		private static bool IsIconSizeAllowed(byte[] data)
		{
			if (data.Length < IconDirectorySize)
			{
				return false;
			}
			int count = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(4));
			if (count == 0 || data.Length < IconDirectorySize + (long)count * IconEntrySize)
			{
				return false;
			}
			long limit = LayoutImportLimits.MaxImagePixels;
			long pixels = 0;
			for (int i = 0; i < count; i++)
			{
				long offset = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(IconDirectorySize + i * IconEntrySize + 12));
				long width;
				long height;
				if (offset + PngHeaderSize <= data.Length && data.AsSpan((int)offset, PngSignature.Length).SequenceEqual(PngSignature))
				{
					width = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan((int)offset + 16));
					height = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan((int)offset + 20));
				}
				else if (offset + BitmapHeaderSize <= data.Length)
				{
					// The bitmap height covers the color rows and the mask rows.
					width = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan((int)offset + 4));
					height = Math.Abs((long)BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan((int)offset + 8))) / 2;
				}
				else
				{
					return false;
				}
				// A PNG header can state sizes up to 2^32 - 1, so the product is checked by division and never overflows.
				if (width <= 0 || height <= 0 || width > (limit - pixels) / height)
				{
					return false;
				}
				pixels += width * height;
			}
			return true;
		}

		private static Image ReadImage(XmlNode node)
		{
			if (node.FirstChild == null || !node.FirstChild.HasChildNodes)
			{
				return null;
			}
			XmlNode alphaNode = LayoutMerger.FindChild(node, "TransparencyData", null);
			XmlNode dataNode = LayoutMerger.FindChild(node, "ImageData", null);
			if (dataNode?.FirstChild == null)
			{
				return null;
			}
			if (!LayoutImportLimits.IsImageTextAllowed(dataNode.FirstChild.Value.Length)
				|| (alphaNode?.FirstChild != null && !LayoutImportLimits.IsImageTextAllowed(alphaNode.FirstChild.Value.Length)))
			{
				return null;
			}
			try
			{
				byte[] alpha = alphaNode?.FirstChild == null ? null : Convert.FromBase64String(alphaNode.FirstChild.Value);
				using MemoryStream stream = new MemoryStream(Convert.FromBase64String(dataNode.FirstChild.Value));
				return CreateImage(stream, alpha);
			}
			catch (Exception ex) when (IsConversionFailure(ex))
			{
				return null;
			}
		}
	}
}
