using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace Mordex.Data
{
	internal class DataValueHelper
	{
		internal static bool IsListLike(object value)
		{
			return value == null || value is IList || value is IListSource;
		}

		public static IList GetList(object value)
		{
			if (value is IList result)
			{
				return result;
			}
			if (value is IListSource listSource)
			{
				return listSource.GetList();
			}
			return null;
		}

		public static object GetValueByPath(object component, string text)
		{
			int separator = text.IndexOf('.');
			string name = separator != -1 ? text.Substring(0, separator) : text;
			if (component == null)
			{
				return null;
			}
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component)?.Find(name, ignoreCase: true);
			if (propertyDescriptor == null)
			{
				return null;
			}
			object value = propertyDescriptor.GetValue(component);
			return separator >= 0 ? GetValueByPath(value, text.Substring(separator + 1)) : value;
		}

		public static void SetPropertyValue(CultureInfo cultureInfo, object component, PropertyDescriptor propertyDescriptor, string text, object value)
		{
			if (component == null)
			{
				return;
			}
			if (propertyDescriptor == null)
			{
				SetValueByPath(cultureInfo, component, text, value);
			}
			else if (!propertyDescriptor.IsReadOnly)
			{
				propertyDescriptor.SetValue(component, ConvertValue(cultureInfo, value, propertyDescriptor.PropertyType));
			}
		}

		public static object GetPropertyValue(object component, PropertyDescriptor propertyDescriptor, string text)
		{
			if (component == null || component is SampleRow)
			{
				return null;
			}
			if (propertyDescriptor != null)
			{
				try
				{
					object value = propertyDescriptor.GetValue(component);
					if (value != null && value.GetType() != propertyDescriptor.PropertyType)
					{
						value = ConvertValue(null, value, propertyDescriptor.PropertyType);
					}
					return value;
				}
				catch (Exception ex) when (ex is TargetInvocationException || ex is InvalidCastException || ex is FormatException || ex is ArgumentException || ex is NotSupportedException || ex is OverflowException)
				{
					// Like data binding, a property that cannot be read or converted is treated as having no value here; the lookup below is tried next.
				}
			}
			bool descriptorMatchesComponent = propertyDescriptor != null
				&& (component.GetType().IsAssignableFrom(propertyDescriptor.ComponentType) || propertyDescriptor.ComponentType.IsAssignableFrom(component.GetType()));
			if (!descriptorMatchesComponent)
			{
				return GetValueByPath(component, text);
			}
			try
			{
				return propertyDescriptor.GetValue(component);
			}
			catch (Exception ex) when (ex is TargetInvocationException || ex is ArgumentException || ex is NotSupportedException)
			{
				return null;
			}
		}

		public static object ConvertValue(CultureInfo cultureInfo, object value, Type type)
		{
			if (value == null || value == DBNull.Value || value.GetType() == type)
			{
				return value;
			}
			if (value is string text)
			{
				return text.Length != 0 ? ParseText(cultureInfo, text, type) : null;
			}
			if (type.IsAssignableFrom(value.GetType()))
			{
				return value;
			}
			return ConvertUsingEnumOrConverter(cultureInfo, value, type) ?? Convert.ChangeType(value, type);
		}

		public static void SetValueByPath(CultureInfo cultureInfo, object component, string text, object value)
		{
			int separator = text.IndexOf('.');
			string name = separator != -1 ? text.Substring(0, separator) : text;
			if (component == null)
			{
				return;
			}
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component)?.Find(name, ignoreCase: true);
			if (propertyDescriptor == null)
			{
				return;
			}
			if (separator >= 0)
			{
				SetValueByPath(cultureInfo, propertyDescriptor.GetValue(component), text.Substring(separator + 1), value);
			}
			else if (!propertyDescriptor.IsReadOnly)
			{
				propertyDescriptor.SetValue(component, ConvertValue(cultureInfo, value, propertyDescriptor.PropertyType));
			}
		}

		internal static object ConvertUsingEnumOrConverter(CultureInfo culture, object value, Type enumType)
		{
			if (value != null && value != DBNull.Value && enumType.IsEnum)
			{
				// An integer or a value of another enum keeps its numeric value, also for a combination of flags with
				// no named member. A value that does not fit the underlying type throws OverflowException. Another
				// enum must not reach the member scan below: it converts each member back to the type of the value,
				// which would scan that enum and convert back again without end.
				if (value.GetType() == enumType)
				{
					return value;
				}
				if (IsIntegralOrEnum(value))
				{
					Type underlying = Enum.GetUnderlyingType(enumType);
					return Enum.ToObject(enumType, Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture));
				}
				foreach (object item in Enum.GetValues(enumType))
				{
					object member;
					try
					{
						member = ConvertValue(null, item, value.GetType());
					}
					catch (OverflowException)
					{
						// A member that does not fit the type of the value cannot be equal to it.
						continue;
					}
					if (member.Equals(value))
					{
						return item;
					}
				}
			}
			if (value != null)
			{
				TypeConverter typeConverter = TypeDescriptor.GetConverter(enumType);
				if (typeConverter.CanConvertFrom(value.GetType()))
				{
					return typeConverter.ConvertFrom(null, culture, value);
				}
			}
			return null;
		}

		// An enum has the type code of its underlying integer type.
		private static bool IsIntegralOrEnum(object value)
		{
			TypeCode code = Type.GetTypeCode(value.GetType());
			return code >= TypeCode.SByte && code <= TypeCode.UInt64;
		}

		internal static object ParseText(CultureInfo cultureInfo, string strA, Type enumType)
		{
			if (enumType.IsEnum && Enum.TryParse(enumType, strA, ignoreCase: true, out object enumValue))
			{
				return enumValue;
			}
			if (enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				Type[] genericArguments = enumType.GetGenericArguments();
				if (genericArguments.Length == 1)
				{
					return ParseText(cultureInfo, strA, genericArguments[0]);
				}
				throw new InvalidCastException();
			}
			switch (Type.GetTypeCode(enumType))
			{
			case TypeCode.Boolean:
				if (string.Equals(strA, bool.TrueString, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (string.Equals(strA, bool.FalseString, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				throw new InvalidCastException();
			case TypeCode.DateTime:
				return DateTime.Parse(strA, cultureInfo.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
			case TypeCode.Decimal:
				return decimal.Parse(strA, NumberStyles.Currency, cultureInfo.NumberFormat);
			case TypeCode.Double:
				return double.Parse(strA, NumberStyles.Float, cultureInfo.NumberFormat);
			case TypeCode.Single:
				return float.Parse(strA, NumberStyles.Float, cultureInfo.NumberFormat);
			case TypeCode.Int16:
				return short.Parse(strA, NumberStyles.Integer, cultureInfo.NumberFormat);
			case TypeCode.Int32:
				return int.Parse(strA, NumberStyles.Integer, cultureInfo.NumberFormat);
			case TypeCode.Int64:
				return long.Parse(strA, NumberStyles.Integer, cultureInfo.NumberFormat);
			case TypeCode.UInt16:
				return ushort.Parse(strA, NumberStyles.Integer, cultureInfo.NumberFormat);
			case TypeCode.UInt32:
				return uint.Parse(strA, NumberStyles.Integer, cultureInfo.NumberFormat);
			case TypeCode.UInt64:
				return ulong.Parse(strA, NumberStyles.Integer, cultureInfo.NumberFormat);
			case TypeCode.Object:
				if (enumType == typeof(TimeSpan))
				{
					return TimeSpan.Parse(strA, cultureInfo);
				}
				if (enumType == typeof(Guid))
				{
					return new Guid(strA);
				}
				return ConvertTextWithConverter(cultureInfo, strA, enumType) ?? strA;
			default:
				return Convert.ChangeType(strA, enumType);
			}
		}

		internal static CultureInfo GetCulture(DataSourceManager dataSourceManager)
		{
			if (dataSourceManager != null)
			{
				return dataSourceManager.CurrentCulture;
			}
			return CultureInfo.CurrentCulture;
		}

		private static object ConvertTextWithConverter(CultureInfo culture, string text, Type targetType)
		{
			TypeConverter typeConverter = TypeDescriptor.GetConverter(targetType);
			return typeConverter.CanConvertFrom(typeof(string)) ? typeConverter.ConvertFrom(null, culture, text) : null;
		}

		public static bool CanConvertToDecimal(object value)
		{
			if (value == null || value == DBNull.Value || value is string)
			{
				return false;
			}
			try
			{
				Convert.ToDecimal(value);
				return true;
			}
			catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
			{
				return false;
			}
		}

		private DataValueHelper()
		{
		}

		public static bool IsNullOrDBNull(object value)
		{
			return value == null || Convert.IsDBNull(value);
		}

		// Orders values that have no natural order against each other, so that Compare(a, b) and Compare(b, a) always have opposite signs.
		private static int CompareUnrelated(object value, object other)
		{
			int byType = string.CompareOrdinal(value.GetType().FullName, other.GetType().FullName);
			if (byType != 0)
			{
				return byType;
			}
			int byText = string.CompareOrdinal(value.ToString(), other.ToString());
			return byText != 0 ? byText : value.GetHashCode().CompareTo(other.GetHashCode());
		}

		/// <summary>Compares two values for sorting, grouping or matching.</summary>
		/// <remarks>
		/// <para>A custom <paramref name="iComparer"/> replaces the rules below.</para>
		/// <para>Without conversion (<paramref name="boolean"/> is false) the comparison is strict. Null and DBNull are
		/// equal and come first. Text is compared with <paramref name="culture"/> and <paramref name="ignoreCase"/>.
		/// Values of one type use their own order. Values of different types are never equal: they are ordered by type
		/// name, so an enum keeps its type identity and the number 1 is not equal to the text "1".</para>
		/// <para>With conversion the rules of <see cref="CompareInDomain"/> apply with no declared domain: numbers,
		/// booleans, characters and enums of any type are compared by their exact numeric value, and text is never
		/// parsed. Callers that know the declared type of the values pass it to <see cref="CompareInDomain"/>.</para>
		/// </remarks>
		public static int CompareValues(CultureInfo culture, object value, object other, IComparer iComparer, bool ignoreCase, bool boolean)
		{
			if (iComparer != null)
			{
				return iComparer.Compare(value, other);
			}
			if (boolean)
			{
				return CompareInDomain(culture, value, other, null, ignoreCase, null);
			}
			value ??= DBNull.Value;
			other ??= DBNull.Value;
			if (value is string first && other is string second)
			{
				return string.Compare(first, second, ignoreCase, culture);
			}
			if (value == DBNull.Value)
			{
				return other == DBNull.Value ? 0 : -1;
			}
			if (other == DBNull.Value)
			{
				return 1;
			}
			if (!(value is IComparable comparable))
			{
				return value.Equals(other) ? 0 : CompareUnrelated(value, other);
			}
			if (other.GetType() == value.GetType())
			{
				return comparable.CompareTo(other);
			}
			return CompareUnrelated(value, other);
		}

		/// <summary>Compares two values after each one is normalized, on its own, to the declared type of the values.</summary>
		/// <param name="culture">Culture for parsing and comparing text; null means the current culture.</param>
		/// <param name="value">The first value.</param>
		/// <param name="other">The second value.</param>
		/// <param name="iComparer">A custom comparer that replaces the rules below, or null.</param>
		/// <param name="ignoreCase">True to compare text without case.</param>
		/// <param name="domain">The declared type of the field, column or key, or null when it is not known.</param>
		/// <remarks>
		/// <para>Each value is normalized without looking at the other value, so equality and order stay transitive for
		/// any mix of inputs.</para>
		/// <list type="bullet">
		/// <item><description>Null and DBNull are equal and come first.</description></item>
		/// <item><description>Integer domains: text is parsed with <paramref name="culture"/> and
		/// <see cref="NumberStyles.Integer"/>, within the range of the type.</description></item>
		/// <item><description>Single and Double: <see cref="NumberStyles.Float"/>. Decimal: <see cref="NumberStyles.Currency"/>,
		/// so the currency symbol and group separators of <paramref name="culture"/> are accepted.</description></item>
		/// <item><description>Boolean: "0" and False are false; "1", "-1" and True are true, without case. Char: one
		/// character. Enum: a member name or an integer.</description></item>
		/// <item><description>DateTime: any date or time text of <paramref name="culture"/>; a time alone gets the
		/// current date.</description></item>
		/// <item><description>String domain: every value is compared as its text in <paramref name="culture"/>.</description></item>
		/// <item><description>Other domains, and no domain (null or Object): text is not parsed.</description></item>
		/// </list>
		/// <para>After normalization numbers, booleans (0 and 1), characters and enums of any type share one numeric
		/// domain and are compared by exact value. Numbers come before every other kind of value. Text is compared with
		/// <paramref name="culture"/> and <paramref name="ignoreCase"/>, values of one type use their own order, and other
		/// values are ordered by type name. Text that cannot be parsed stays text, so it never equals a number.</para>
		/// </remarks>
		public static int CompareInDomain(CultureInfo culture, object value, object other, IComparer iComparer, bool ignoreCase, Type domain)
		{
			if (iComparer != null)
			{
				return iComparer.Compare(value, other);
			}
			culture ??= CultureInfo.CurrentCulture;
			domain = GetDomain(domain);
			return CompareNormalized(culture, Normalize(culture, value, domain), Normalize(culture, other, domain), ignoreCase);
		}

		internal static Type GetTypeOfCode(TypeCode code)
		{
			switch (code)
			{
			case TypeCode.Empty:
			case TypeCode.Object:
			case TypeCode.DBNull:
				return null;
			default:
				return Type.GetType("System." + code, throwOnError: false);
			}
		}

		private static Type GetDomain(Type type)
		{
			if (type == null)
			{
				return null;
			}
			type = Nullable.GetUnderlyingType(type) ?? type;
			return type == typeof(object) ? null : type;
		}

		private static object Normalize(CultureInfo culture, object value, Type domain)
		{
			if (value == null || value == DBNull.Value)
			{
				return DBNull.Value;
			}
			if (domain == null || domain.IsInstanceOfType(value))
			{
				return value;
			}
			if (domain == typeof(string))
			{
				return Convert.ToString(value, culture);
			}
			if (!(value is string text))
			{
				return value;
			}
			return ParseInDomain(culture, text, domain) ?? value;
		}

		// Accepts the same text for each type as the comparison did before it had a declared domain. A value that does
		// not fit the type, such as "300" for Byte, is not parsed.
		private static object ParseInDomain(CultureInfo culture, string text, Type domain)
		{
			if (domain.IsEnum)
			{
				return Enum.TryParse(domain, text.Trim(), ignoreCase: true, out object member) ? member : null;
			}
			NumberFormatInfo numbers = culture.NumberFormat;
			switch (Type.GetTypeCode(domain))
			{
			case TypeCode.Boolean:
				return ParseBoolean(text);
			case TypeCode.Char:
				return text.Length == 1 ? text[0] : null;
			case TypeCode.SByte:
				return sbyte.TryParse(text, NumberStyles.Integer, numbers, out sbyte signedByte) ? signedByte : null;
			case TypeCode.Byte:
				return byte.TryParse(text, NumberStyles.Integer, numbers, out byte unsignedByte) ? unsignedByte : null;
			case TypeCode.Int16:
				return short.TryParse(text, NumberStyles.Integer, numbers, out short shortValue) ? shortValue : null;
			case TypeCode.UInt16:
				return ushort.TryParse(text, NumberStyles.Integer, numbers, out ushort ushortValue) ? ushortValue : null;
			case TypeCode.Int32:
				return int.TryParse(text, NumberStyles.Integer, numbers, out int intValue) ? intValue : null;
			case TypeCode.UInt32:
				return uint.TryParse(text, NumberStyles.Integer, numbers, out uint uintValue) ? uintValue : null;
			case TypeCode.Int64:
				return long.TryParse(text, NumberStyles.Integer, numbers, out long longValue) ? longValue : null;
			case TypeCode.UInt64:
				return ulong.TryParse(text, NumberStyles.Integer, numbers, out ulong ulongValue) ? ulongValue : null;
			case TypeCode.Single:
				return float.TryParse(text, NumberStyles.Float, numbers, out float single) ? single : null;
			case TypeCode.Double:
				return double.TryParse(text, NumberStyles.Float, numbers, out double real) ? real : null;
			case TypeCode.Decimal:
				return decimal.TryParse(text, NumberStyles.Currency, numbers, out decimal number) ? number : null;
			case TypeCode.DateTime:
				return DateTime.TryParse(text, culture.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces, out DateTime date) ? date : null;
			default:
				return null;
			}
		}

		// "0" and "False" are false; "1", "-1" and "True" are true. Case is ignored.
		private static object ParseBoolean(string text)
		{
			if (text == "0" || string.Equals(text, bool.FalseString, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (text == "1" || text == "-1" || string.Equals(text, bool.TrueString, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return null;
		}

		private static int CompareNormalized(CultureInfo culture, object value, object other, bool ignoreCase)
		{
			if (value == DBNull.Value || other == DBNull.Value)
			{
				return value == other ? 0 : value == DBNull.Value ? -1 : 1;
			}
			if (value is string first && other is string second)
			{
				return string.Compare(first, second, ignoreCase, culture);
			}
			bool isNumber = IsNumeric(value);
			bool otherIsNumber = IsNumeric(other);
			if (isNumber && otherIsNumber)
			{
				return CompareNumbers(value, other);
			}
			if (isNumber != otherIsNumber)
			{
				return isNumber ? -1 : 1;
			}
			if (value.GetType() == other.GetType() && value is IComparable comparable)
			{
				return comparable.CompareTo(other);
			}
			return value.Equals(other) ? 0 : CompareUnrelated(value, other);
		}

		private static bool IsNumeric(object value)
		{
			TypeCode code = Type.GetTypeCode(value.GetType());
			return code >= TypeCode.Boolean && code <= TypeCode.Decimal;
		}

		private static bool IsReal(object value)
		{
			return value is double || value is float;
		}

		// Exact for every pair: no value is rounded to the other type, so equality stays transitive across types.
		private static int CompareNumbers(object value, object other)
		{
			if (IsReal(value) && IsReal(other))
			{
				return Convert.ToDouble(value, CultureInfo.InvariantCulture).CompareTo(Convert.ToDouble(other, CultureInfo.InvariantCulture));
			}
			if (IsReal(value))
			{
				return CompareRealWithExact(Convert.ToDouble(value, CultureInfo.InvariantCulture), other);
			}
			if (IsReal(other))
			{
				return -CompareRealWithExact(Convert.ToDouble(other, CultureInfo.InvariantCulture), value);
			}
			if (value is decimal || other is decimal)
			{
				return ToDecimal(value).CompareTo(ToDecimal(other));
			}
			return ToInteger(value).CompareTo(ToInteger(other));
		}

		// Booleans, characters and enums count as their integer value.
		private static Int128 ToInteger(object value)
		{
			if (Type.GetTypeCode(value.GetType()) == TypeCode.UInt64)
			{
				return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
			}
			if (value is char character)
			{
				return character;
			}
			return Convert.ToInt64(value, CultureInfo.InvariantCulture);
		}

		private static decimal ToDecimal(object value)
		{
			return value is decimal number ? number : (decimal)ToInteger(value);
		}

		// Orders NaN below every number, like double.CompareTo.
		private static int CompareRealWithExact(double real, object other)
		{
			if (double.IsNaN(real))
			{
				return -1;
			}
			if (double.IsInfinity(real))
			{
				return real > 0 ? 1 : -1;
			}
			BigInteger numerator = ToInteger(real, out int binaryExponent);
			BigInteger otherNumerator;
			int decimalScale = 0;
			if (other is decimal number)
			{
				otherNumerator = ToInteger(number, out decimalScale);
			}
			else
			{
				otherNumerator = (BigInteger)ToInteger(other);
			}
			// real = numerator * 2^binaryExponent and other = otherNumerator / 10^decimalScale.
			numerator *= BigInteger.Pow(10, decimalScale);
			if (binaryExponent >= 0)
			{
				numerator <<= binaryExponent;
			}
			else
			{
				otherNumerator <<= -binaryExponent;
			}
			return numerator.CompareTo(otherNumerator);
		}

		private static BigInteger ToInteger(double real, out int binaryExponent)
		{
			long bits = BitConverter.DoubleToInt64Bits(real);
			int exponent = (int)((bits >> 52) & 0x7FF);
			long mantissa = bits & 0xFFFFFFFFFFFFFL;
			if (exponent == 0)
			{
				exponent = 1;
			}
			else
			{
				mantissa |= 1L << 52;
			}
			binaryExponent = exponent - 1075;
			return bits < 0 ? -(BigInteger)mantissa : mantissa;
		}

		private static BigInteger ToInteger(decimal number, out int decimalScale)
		{
			int[] parts = decimal.GetBits(number);
			BigInteger magnitude = ((BigInteger)(uint)parts[2] << 64) | ((BigInteger)(uint)parts[1] << 32) | (uint)parts[0];
			decimalScale = (parts[3] >> 16) & 0xFF;
			return parts[3] < 0 ? -magnitude : magnitude;
		}
	}
}
