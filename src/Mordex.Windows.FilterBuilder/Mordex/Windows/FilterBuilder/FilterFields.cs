using System;
using Mordex.Data;

namespace Mordex.Windows.FilterBuilder
{
	internal static class FilterFields
	{
		internal static string DisplayName(IMordexField field)
		{
			if (field.FilterBuilderDisplayName.Length > 0)
			{
				return field.FilterBuilderDisplayName;
			}
			return field.DisplayName.Length > 0 ? field.DisplayName : field.Key;
		}

		internal static FilterEditType EffectiveEditType(IMordexField field)
		{
			return field.FilterEditType == FilterEditType.Default ? field.DefaultFilterEditType : field.FilterEditType;
		}

		// Fields that compare as text are edited as text whatever their data type is.
		internal static TypeCode EditorTypeCode(IMordexField field)
		{
			return field.CompareTarget == CompareTarget.Text ? TypeCode.String : field.DataTypeCode;
		}
	}
}
