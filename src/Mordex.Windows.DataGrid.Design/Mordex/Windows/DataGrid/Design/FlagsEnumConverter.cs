using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class FlagsEnumConverter : TypeConverter
	{
		public FlagsEnumConverter()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

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
			if (type == typeof(ColumnOwnerDrawnMode))
			{
				for (int i = 0; i < values.Length; i++)
				{
					if ((ColumnOwnerDrawnMode)value == (ColumnOwnerDrawnMode)values.GetValue(i))
					{
						text = values.GetValue(i).ToString();
						break;
					}
				}
				if (text == "")
				{
					for (int j = 0; j < values.Length; j++)
					{
						if (((ColumnOwnerDrawnMode)value & (ColumnOwnerDrawnMode)values.GetValue(j)) == (ColumnOwnerDrawnMode)values.GetValue(j) && (int)values.GetValue(j) != 0)
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
			else if (type == typeof(GridOwnerDrawnArea))
			{
				for (int k = 0; k < values.Length; k++)
				{
					if ((GridOwnerDrawnArea)value == (GridOwnerDrawnArea)values.GetValue(k))
					{
						text = values.GetValue(k).ToString();
						break;
					}
				}
				if (text == "")
				{
					for (int index = 0; index < values.Length; index++)
					{
						if (((GridOwnerDrawnArea)value & (GridOwnerDrawnArea)values.GetValue(index)) == (GridOwnerDrawnArea)values.GetValue(index) && (int)values.GetValue(index) != 0)
						{
							if (text.Length > 0)
							{
								text += ", ";
							}
							text += values.GetValue(index).ToString();
						}
					}
				}
			}
			return text;
		}
	}
}
