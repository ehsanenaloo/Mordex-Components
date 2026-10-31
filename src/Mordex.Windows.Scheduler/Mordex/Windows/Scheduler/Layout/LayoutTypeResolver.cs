using System;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Layouts
{
	/// <summary>Resolves type names read from layout files through the shared <see cref="LayoutTypePolicy"/>.</summary>
	internal static class LayoutTypeResolver
	{
		internal static Type Resolve(string typeName)
		{
			return LayoutTypePolicy.Resolve(typeName);
		}
	}
}
