using System;
using System.Reflection;
using System.Threading;

namespace Mordex.Windows.DatePicker.Design
{
	// Makes the designer host reuse the Mordex assemblies that are already loaded instead of loading a second copy.
	internal static class AssemblyLoaderHelper
	{
		private static readonly string[] MordexAssemblyNames = { "Mordex.Windows.Common", "Mordex.Windows.DatePicker" };

		private static int _isAttached;

		internal static void AttachCurrentDomainEvents()
		{
			if (Interlocked.Exchange(ref _isAttached, 1) == 0)
			{
				AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			}
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			return FindLoadedAssembly(args.Name);
		}

		// Returns the loaded assembly for an unsigned Mordex assembly name, or null for any other request.
		internal static Assembly FindLoadedAssembly(string requestedName)
		{
			AssemblyName requested = new AssemblyName(requestedName);
			if (requested.GetPublicKeyToken().Length != 0 || Array.IndexOf(MordexAssemblyNames, requested.Name) < 0)
			{
				return null;
			}
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				AssemblyName loaded = assembly.GetName();
				if (string.Equals(loaded.Name, requested.Name, StringComparison.Ordinal) && loaded.GetPublicKeyToken().Length == 0)
				{
					return assembly;
				}
			}
			return null;
		}
	}
}
