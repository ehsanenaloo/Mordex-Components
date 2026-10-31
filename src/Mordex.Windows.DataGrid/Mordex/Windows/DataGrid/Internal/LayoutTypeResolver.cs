using System;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid.Internal
{
	/// <summary>
	/// Resolves the type names stored in grid layout files through the shared <see cref="LayoutTypePolicy"/>,
	/// so a layout file cannot make the grid load assemblies or run converters of arbitrary types.
	/// </summary>
	internal static class LayoutTypeResolver
	{
		/// <summary>Returns the type for a saved type name, or null when the name is empty, unknown or not allowed.</summary>
		internal static Type Resolve(string typeName)
		{
			return LayoutTypePolicy.Resolve(typeName);
		}

		/// <summary>Converts text to the saved type; returns the text itself when the type cannot be used.</summary>
		internal static object ConvertOrKeepText(string text, string typeName)
		{
			return LayoutTypePolicy.ConvertOrKeepText(text, typeName);
		}
	}
}
