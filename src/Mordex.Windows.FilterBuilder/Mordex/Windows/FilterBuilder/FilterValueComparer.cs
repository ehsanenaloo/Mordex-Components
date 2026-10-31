using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace Mordex.Windows.FilterBuilder
{
	// Total order over filter values of any type, so it is safe for sorting.
	// Groups sort in this order: null, boolean, number, date, text, other, array.
	// Numbers of different types are compared by exact value.
	internal sealed class FilterValueComparer : IComparer<object>, IComparer
	{
		private enum ValueGroup
		{
			Null,
			Boolean,
			Number,
			DateTime,
			Text,
			Other,
			Array
		}

		internal static readonly FilterValueComparer IgnoreCase = new FilterValueComparer(ignoreCase: true);

		internal static readonly FilterValueComparer MatchCase = new FilterValueComparer(ignoreCase: false);

		private const int DoubleMantissaBits = 52;

		private const int DoubleExponentBias = 1075;

		private const int DoubleExponentMask = 0x7FF;

		private readonly bool _ignoreCase;

		private FilterValueComparer(bool ignoreCase)
		{
			_ignoreCase = ignoreCase;
		}

		public int Compare(object x, object y)
		{
			return Math.Sign(CompareNormalized(Normalize(x), Normalize(y)));
		}

		private static object Normalize(object value)
		{
			return value is DBNull ? null : value;
		}

		private int CompareNormalized(object x, object y)
		{
			ValueGroup xGroup = GroupOf(x);
			ValueGroup yGroup = GroupOf(y);
			if (xGroup != yGroup)
			{
				return xGroup.CompareTo(yGroup);
			}
			switch (xGroup)
			{
			case ValueGroup.Null:
				return 0;
			case ValueGroup.Boolean:
				return ((bool)x).CompareTo((bool)y);
			case ValueGroup.Number:
				return CompareNumbers(x, y);
			case ValueGroup.DateTime:
				return ((DateTime)x).CompareTo((DateTime)y);
			case ValueGroup.Text:
				return string.Compare(AsText(x), AsText(y), _ignoreCase, CultureInfo.CurrentCulture);
			case ValueGroup.Array:
				return CompareArrays((Array)x, (Array)y);
			default:
				return CompareOthers(x, y);
			}
		}

		private static ValueGroup GroupOf(object value)
		{
			if (value == null)
			{
				return ValueGroup.Null;
			}
			if (value is Array vector && vector.Rank == 1)
			{
				return ValueGroup.Array;
			}
			Type type = value.GetType();
			if (type.IsEnum)
			{
				return ValueGroup.Other;
			}
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Boolean:
				return ValueGroup.Boolean;
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
				return ValueGroup.Number;
			case TypeCode.DateTime:
				return ValueGroup.DateTime;
			case TypeCode.String:
			case TypeCode.Char:
				return ValueGroup.Text;
			default:
				return ValueGroup.Other;
			}
		}

		private static string AsText(object value)
		{
			return value as string ?? value.ToString();
		}

		private int CompareArrays(Array x, Array y)
		{
			int common = Math.Min(x.Length, y.Length);
			for (int i = 0; i < common; i++)
			{
				int result = Compare(x.GetValue(i), y.GetValue(i));
				if (result != 0)
				{
					return result;
				}
			}
			return x.Length.CompareTo(y.Length);
		}

		private static int CompareOthers(object x, object y)
		{
			int byType = string.CompareOrdinal(x.GetType().AssemblyQualifiedName, y.GetType().AssemblyQualifiedName);
			if (byType != 0)
			{
				return byType;
			}
			if (x is IComparable comparable)
			{
				try
				{
					return comparable.CompareTo(y);
				}
				catch (Exception ex) when (ex is ArgumentException || ex is InvalidCastException)
				{
					// The type refuses this pair; fall back to the text order below.
				}
			}
			if (x.Equals(y))
			{
				return 0;
			}
			return string.CompareOrdinal(x.ToString(), y.ToString());
		}

		private static int CompareNumbers(object x, object y)
		{
			if (x.GetType() == y.GetType())
			{
				return ((IComparable)x).CompareTo(y);
			}
			int xSpecial = SpecialRank(x);
			int ySpecial = SpecialRank(y);
			if (xSpecial != 0 || ySpecial != 0)
			{
				return xSpecial.CompareTo(ySpecial);
			}
			ToFraction(x, out BigInteger xNumerator, out BigInteger xDenominator);
			ToFraction(y, out BigInteger yNumerator, out BigInteger yDenominator);
			return (xNumerator * yDenominator).CompareTo(yNumerator * xDenominator);
		}

		// NaN sorts below minus infinity, infinity above all finite numbers; finite numbers are 0.
		private static int SpecialRank(object number)
		{
			double value;
			switch (number)
			{
			case double doubleValue:
				value = doubleValue;
				break;
			case float floatValue:
				value = floatValue;
				break;
			default:
				return 0;
			}
			if (double.IsNaN(value))
			{
				return -2;
			}
			if (double.IsNegativeInfinity(value))
			{
				return -1;
			}
			return double.IsPositiveInfinity(value) ? 1 : 0;
		}

		private static void ToFraction(object number, out BigInteger numerator, out BigInteger denominator)
		{
			denominator = BigInteger.One;
			switch (number)
			{
			case decimal decimalValue:
				int[] parts = decimal.GetBits(decimalValue);
				BigInteger mantissa = ((BigInteger)(uint)parts[2] << 64) | ((BigInteger)(uint)parts[1] << 32) | (uint)parts[0];
				numerator = parts[3] < 0 ? -mantissa : mantissa;
				denominator = BigInteger.Pow(10, (parts[3] >> 16) & 0xFF);
				break;
			case double doubleValue:
				FromDouble(doubleValue, out numerator, out denominator);
				break;
			case float floatValue:
				FromDouble(floatValue, out numerator, out denominator);
				break;
			case ulong unsignedValue:
				numerator = unsignedValue;
				break;
			default:
				numerator = Convert.ToInt64(number, CultureInfo.InvariantCulture);
				break;
			}
		}

		private static void FromDouble(double value, out BigInteger numerator, out BigInteger denominator)
		{
			long bits = BitConverter.DoubleToInt64Bits(value);
			int exponent = (int)((bits >> DoubleMantissaBits) & DoubleExponentMask);
			long mantissa = bits & ((1L << DoubleMantissaBits) - 1);
			if (exponent == 0)
			{
				exponent = 1;
			}
			else
			{
				mantissa |= 1L << DoubleMantissaBits;
			}
			exponent -= DoubleExponentBias;
			numerator = bits < 0 ? -mantissa : mantissa;
			denominator = BigInteger.One;
			if (exponent > 0)
			{
				numerator <<= exponent;
			}
			else
			{
				denominator <<= -exponent;
			}
		}
	}
}
