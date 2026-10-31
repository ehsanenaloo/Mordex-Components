using System;

namespace Mordex.Windows.Timeline
{
	[Flags]
	internal enum PersistenceMode
	{
		Fields = 1,
		StoredFilters = 2,
		FilterCondition = 4,
		GroupHeaderTotals = 8,
		Items = 0x10,
		InternalImageList = 0x20,
		AppearanceProperties = 0x40,
		DropDownCalendar = 0x80,
		OtherProperties = 0x100,
		FormatConditions = 0x200,
		Groups = 0x400,
		WorkingHourSchema = 0x800,
		All = 0xFFF
	}
}
