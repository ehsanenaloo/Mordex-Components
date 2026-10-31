using System;
using System.Reflection;

namespace Mordex.Windows.Ribbon.Design
{
	internal static class AssemblyLoaderHelper
	{
		private static readonly object s_sync = new object();

		private static volatile bool s_isLoaded;

		private static readonly string[] s_mordexReferences = { "Mordex.Windows.Common", "Mordex.Windows.Ribbon", "Mordex.Windows.UI" };

		internal static void AttachCurrentDomainEvents()
		{
			if (s_isLoaded)
			{
				return;
			}
			lock (s_sync)
			{
				if (s_isLoaded)
				{
					return;
				}
				AppDomain currentDomain = AppDomain.CurrentDomain;
				currentDomain.AssemblyResolve += OnAssemblyResolve;
				currentDomain.ProcessExit += DetachCurrentDomainEvents;
				s_isLoaded = true;
			}
		}

		private static void DetachCurrentDomainEvents(object sender, EventArgs e)
		{
			lock (s_sync)
			{
				if (!s_isLoaded)
				{
					return;
				}
				AppDomain currentDomain = AppDomain.CurrentDomain;
				currentDomain.AssemblyResolve -= OnAssemblyResolve;
				currentDomain.ProcessExit -= DetachCurrentDomainEvents;
				s_isLoaded = false;
			}
		}

		internal static Assembly FindLoadedAssembly(string requestedName, Assembly[] candidates)
		{
			AssemblyName requested;
			try
			{
				requested = new AssemblyName(requestedName);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is System.IO.FileLoadException)
			{
				// Not a parsable assembly name, so it is not one of ours.
				return null;
			}
			byte[] token = requested.GetPublicKeyToken();
			if (requested.Name == null || (token != null && token.Length != 0) || Array.IndexOf(s_mordexReferences, requested.Name) < 0)
			{
				return null;
			}
			foreach (Assembly assembly in candidates)
			{
				AssemblyName loaded = assembly.GetName();
				byte[] loadedToken = loaded.GetPublicKeyToken();
				if (string.Equals(loaded.Name, requested.Name, StringComparison.Ordinal) && (loadedToken == null || loadedToken.Length == 0))
				{
					return assembly;
				}
			}
			return null;
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			return FindLoadedAssembly(args.Name, AppDomain.CurrentDomain.GetAssemblies());
		}
	}
}
