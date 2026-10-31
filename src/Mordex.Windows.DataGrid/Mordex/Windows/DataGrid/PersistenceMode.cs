using System;

namespace Mordex.Windows.DataGrid
{
	[Flags]
	internal enum PersistenceMode
	{
		TableStructure = 1,
		DropDowns = 2,
		FormatStyles = 4,
		InternalImageList = 8,
		AppearanceProperties = 0x10,
		OtherProperties = 0x20,
		All = 0x3F
	}
}
