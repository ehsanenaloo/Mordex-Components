using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
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
			if (type == typeof(SchedulerDayOfWeek))
			{
				for (int i = 0; i < values.Length; i++)
				{
					if ((SchedulerDayOfWeek)value == (SchedulerDayOfWeek)values.GetValue(i))
					{
						text = values.GetValue(i).ToString();
						break;
					}
				}
				if (text == "")
				{
					for (int j = 0; j < values.Length; j++)
					{
						if (((SchedulerDayOfWeek)value & (SchedulerDayOfWeek)values.GetValue(j)) == (SchedulerDayOfWeek)values.GetValue(j) && (int)values.GetValue(j) != 0)
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
			else if (type == typeof(AllowAppointmentDrag))
			{
				for (int m = 0; m < values.Length; m++)
				{
					if ((AllowAppointmentDrag)value == (AllowAppointmentDrag)values.GetValue(m))
					{
						text = values.GetValue(m).ToString();
						break;
					}
				}
				if (text == "")
				{
					for (int n = 0; n < values.Length; n++)
					{
						if (((AllowAppointmentDrag)value & (AllowAppointmentDrag)values.GetValue(n)) == (AllowAppointmentDrag)values.GetValue(n) && (int)values.GetValue(n) != 0)
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
			else if (type == typeof(OwnerDrawnArea))
			{
				try
				{
					for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
					{
						if ((OwnerDrawnArea)value == (OwnerDrawnArea)values.GetValue(valueIndex))
						{
							text = values.GetValue(valueIndex).ToString();
							break;
						}
					}
					if (text == "")
					{
						for (int flagIndex = 0; flagIndex < values.Length; flagIndex++)
						{
							if (((OwnerDrawnArea)value & (OwnerDrawnArea)values.GetValue(flagIndex)) == (OwnerDrawnArea)values.GetValue(flagIndex) && (int)values.GetValue(flagIndex) != 0)
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
				catch (Exception ex)
				{
					Mordex.Windows.Common.MxMessageBox.Show(ex.ToString());
				}
			}
			return text;
		}
	}
}
