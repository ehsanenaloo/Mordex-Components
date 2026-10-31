using System;
using System.IO;
using System.Reflection;

namespace Mordex.Windows.Timeline.Design
{
	internal static class AssemblyLoaderHelper
	{
		private static readonly string[] MordexAssemblyNames = new string[] { "Mordex.Data", "Mordex.Windows.Common", "Mordex.Windows.Timeline" };

		private static readonly object AttachLock = new object();

		private static bool _isAttached;

		internal static void AttachCurrentDomainEvents()
		{
			lock (AttachLock)
			{
				if (!_isAttached)
				{
					AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
					_isAttached = true;
				}
			}
		}

		// The designer host can load a second copy of a Mordex assembly. Return the copy that is already loaded.
		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string requestedName;
			try
			{
				requestedName = new AssemblyName(args.Name).Name;
			}
			catch (Exception ex) when (ex is FileLoadException || ex is ArgumentException)
			{
				return null;
			}
			if (Array.FindIndex(MordexAssemblyNames, name => string.Equals(name, requestedName, StringComparison.OrdinalIgnoreCase)) < 0)
			{
				return null;
			}
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (string.Equals(assembly.GetName().Name, requestedName, StringComparison.OrdinalIgnoreCase))
				{
					return assembly;
				}
			}
			return null;
		}
	}
}
