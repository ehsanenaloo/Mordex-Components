using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Mordex.Windows.UI.Design
{
	internal static class AssemblyLoaderHelper
	{
		private static readonly string[] _mordexReferences = new string[2] { "Mordex.Windows.Common", "Mordex.Windows.UI" };

		private static readonly object _lock = new object();

		private static bool _isLoaded;

		internal static void AttachCurrentDomainEvents()
		{
			lock (_lock)
			{
				if (_isLoaded)
				{
					return;
				}
				AppDomain currentDomain = AppDomain.CurrentDomain;
				currentDomain.AssemblyResolve += OnAssemblyResolve;
				currentDomain.ProcessExit += OnDomainShutdown;
				currentDomain.DomainUnload += OnDomainShutdown;
				_isLoaded = true;
			}
		}

		internal static void DetachCurrentDomainEvents()
		{
			lock (_lock)
			{
				if (!_isLoaded)
				{
					return;
				}
				AppDomain currentDomain = AppDomain.CurrentDomain;
				currentDomain.AssemblyResolve -= OnAssemblyResolve;
				currentDomain.ProcessExit -= OnDomainShutdown;
				currentDomain.DomainUnload -= OnDomainShutdown;
				_isLoaded = false;
			}
		}

		private static void OnDomainShutdown(object sender, EventArgs e)
		{
			DetachCurrentDomainEvents();
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			return FindLoadedAssembly(args.Name, AppDomain.CurrentDomain.GetAssemblies());
		}

		// Returns the loaded assembly whose simple name is exactly the requested Mordex assembly, or null.
		// Only unsigned requests are matched, because the Mordex assemblies have no strong name.
		internal static Assembly FindLoadedAssembly(string requestedName, IEnumerable<Assembly> loadedAssemblies)
		{
			AssemblyName requested;
			try
			{
				requested = new AssemblyName(requestedName);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is FileLoadException)
			{
				return null;
			}
			byte[] token = requested.GetPublicKeyToken();
			if (token != null && token.Length > 0)
			{
				return null;
			}
			if (Array.FindIndex(_mordexReferences, name => string.Equals(name, requested.Name, StringComparison.OrdinalIgnoreCase)) < 0)
			{
				return null;
			}
			foreach (Assembly assembly in loadedAssemblies)
			{
				if (string.Equals(assembly.GetName().Name, requested.Name, StringComparison.OrdinalIgnoreCase))
				{
					return assembly;
				}
			}
			return null;
		}
	}
}
