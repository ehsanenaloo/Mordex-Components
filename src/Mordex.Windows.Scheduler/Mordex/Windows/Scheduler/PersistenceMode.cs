using System;

namespace Mordex.Windows.Scheduler
{
	[Flags]
	internal enum PersistenceMode
	{
		Fields = 1,
		FormatConditions = 2,
		StoredFilters = 4,
		FilterCondition = 8,
		InternalImageList = 0x10,
		AppearanceProperties = 0x20,
		OtherProperties = 0x40,
		Dates = 0x80,
		Owners = 0x100,
		All = 0x1FF
	}
}
