using System;

namespace Mordex.Windows.Common.Layouts
{
	/// <summary>Resolves type names read from layout text through <see cref="LayoutTypePolicy"/>.</summary>
	internal static class LayoutTypeResolver
	{
		internal static Type Resolve(string typeName)
		{
			return LayoutTypePolicy.Resolve(typeName);
		}
	}
}
