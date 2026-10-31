using System;

namespace Mordex.Windows.FilterBuilder
{
	[Flags]
	public enum AvailableLogicalOperators
	{
		And = 1,
		AndNot = 2,
		Or = 4,
		OrNot = 8,
		Xor = 0x10,
		XorNot = 0x20,
		All = 0x3F
	}
}
