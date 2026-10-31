using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.Timeline.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class FlagsEnumConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type type)
		{
			if (type == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, type);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value == null)
			{
				return "";
			}
			Type type = value.GetType();
			string text = "";
			Array values = Enum.GetValues(type);
			if (type == typeof(DayOfWeek))
			{
				for (int i = 0; i < values.Length; i++)
				{
					if ((DayOfWeek)value == (DayOfWeek)values.GetValue(i))
					{
						text = values.GetValue(i).ToString();
						break;
					}
				}
				if (text == "")
				{
					for (int j = 0; j < values.Length; j++)
					{
						if (((DayOfWeek)value & (DayOfWeek)values.GetValue(j)) == (DayOfWeek)values.GetValue(j) && (int)values.GetValue(j) != 0)
						{
							if (text.Length > 0)
							{
								text += ", ";
							}
							text += values.GetValue(j).ToString();
						}
					}
				}
			}
			else if (type == typeof(OwnerDrawnArea))
			{
				for (int m = 0; m < values.Length; m++)
				{
					if ((OwnerDrawnArea)value == (OwnerDrawnArea)values.GetValue(m))
					{
						text = values.GetValue(m).ToString();
						break;
					}
				}
				if (text == "")
				{
					for (int n = 0; n < values.Length; n++)
					{
						if (((OwnerDrawnArea)value & (OwnerDrawnArea)values.GetValue(n)) == (OwnerDrawnArea)values.GetValue(n) && (int)values.GetValue(n) != 0)
						{
							if (text.Length > 0)
							{
								text += ", ";
							}
							text += values.GetValue(n).ToString();
						}
					}
				}
			}
			else if (type == typeof(AllowItemDrag))
			{
				for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
				{
					if ((AllowItemDrag)value == (AllowItemDrag)values.GetValue(valueIndex))
					{
						text = values.GetValue(valueIndex).ToString();
						break;
					}
				}
				if (text == "")
				{
					for (int flagIndex = 0; flagIndex < values.Length; flagIndex++)
					{
						if (((AllowItemDrag)value & (AllowItemDrag)values.GetValue(flagIndex)) == (AllowItemDrag)values.GetValue(flagIndex) && (int)values.GetValue(flagIndex) != 0)
						{
							if (text.Length > 0)
							{
								text += ", ";
							}
							text += values.GetValue(flagIndex).ToString();
						}
					}
				}
			}
			return text;
		}
	}
}
