using System;

namespace Mordex.Windows.FilterBuilder
{
	[Flags]
	public enum AvailableConditionOperators
	{
		Default = 0,
		Equal = 1,
		NotEqual = 2,
		GreaterThan = 4,
		LessThan = 8,
		GreaterThanOrEqualTo = 0x10,
		LessThanOrEqualTo = 0x20,
		Between = 0x40,
		NotBetween = 0x80,
		Contains = 0x100,
		NotContains = 0x200,
		BeginsWith = 0x400,
		EndsWith = 0x800,
		IsNull = 0x1000,
		NotIsNull = 0x2000,
		IsEmpty = 0x4000,
		NotIsEmpty = 0x8000,
		AnyOf = 0x10000,
		NoneOf = 0x20000,
		All = 0x3FFFF
	}
}
