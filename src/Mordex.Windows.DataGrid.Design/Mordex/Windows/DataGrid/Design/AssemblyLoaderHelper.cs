using System;
using System.Reflection;

namespace Mordex.Windows.DataGrid.Design
{
	/// <summary>
	/// Redirects requests for the Mordex assemblies to the copies that are already loaded in the designer process,
	/// whatever their version.
	/// </summary>
	internal static class AssemblyLoaderHelper
	{
		private static readonly string[] _mordexAssemblyNames = { "Mordex.Data", "Mordex.Windows.Common", "Mordex.Windows.DataGrid" };

		private static bool _isAttached;

		internal static void AttachCurrentDomainEvents()
		{
			if (!_isAttached)
			{
				AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
				_isAttached = true;
			}
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			AssemblyName requested;
			try
			{
				requested = new AssemblyName(args.Name);
			}
			catch (Exception ex) when (ex is System.IO.FileLoadException || ex is ArgumentException)
			{
				return null;
			}
			if (!IsMordexAssembly(requested.Name) || !IsUnsigned(requested))
			{
				return null;
			}
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (string.Equals(assembly.GetName().Name, requested.Name, StringComparison.OrdinalIgnoreCase))
				{
					return assembly;
				}
			}
			return null;
		}

		internal static bool IsMordexAssembly(string simpleName)
		{
			foreach (string name in _mordexAssemblyNames)
			{
				if (string.Equals(name, simpleName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsUnsigned(AssemblyName name)
		{
			byte[] token = name.GetPublicKeyToken();
			return token == null || token.Length == 0;
		}
	}
}
