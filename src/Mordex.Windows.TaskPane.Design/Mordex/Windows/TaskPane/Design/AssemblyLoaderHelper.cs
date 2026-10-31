using System;
using System.Reflection;

namespace Mordex.Windows.TaskPane.Design
{
	/// <summary>
	/// The designer host can load a second copy of the Mordex assemblies. This resolver hands out the copy that is
	/// already loaded when the requested simple name is one of ours.
	/// </summary>
	internal static class AssemblyLoaderHelper
	{
		private static readonly string[] MordexAssemblyNames = { "Mordex.Windows.Common", "Mordex.Windows.TaskPane" };

		private static readonly object SyncRoot = new object();

		private static bool _isAttached;

		internal static void AttachCurrentDomainEvents()
		{
			lock (SyncRoot)
			{
				if (!_isAttached)
				{
					AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
					_isAttached = true;
				}
			}
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			AssemblyName requested;
			try
			{
				requested = new AssemblyName(args.Name);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is System.IO.FileLoadException)
			{
				return null;
			}
			if (!IsMordexAssembly(requested.Name) || requested.GetPublicKeyToken()?.Length > 0)
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

		private static bool IsMordexAssembly(string simpleName)
		{
			foreach (string name in MordexAssemblyNames)
			{
				if (string.Equals(name, simpleName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}
}
