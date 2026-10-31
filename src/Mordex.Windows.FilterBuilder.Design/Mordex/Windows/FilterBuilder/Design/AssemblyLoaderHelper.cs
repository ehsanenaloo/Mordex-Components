using System;
using System.IO;
using System.Reflection;

namespace Mordex.Windows.FilterBuilder.Design
{
	internal static class AssemblyLoaderHelper
	{
		private static readonly string[] MordexAssemblyNames = { "Mordex.Windows.Common", "Mordex.Windows.Navigation" };

		private static readonly object Sync = new object();

		private static bool _attached;

		internal static void AttachCurrentDomainEvents()
		{
			lock (Sync)
			{
				if (_attached)
				{
					return;
				}
				AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
				_attached = true;
			}
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			return FindLoadedAssembly(args.Name);
		}

		// Returns the loaded assembly that has the same simple name as the requested unsigned Mordex assembly.
		internal static Assembly FindLoadedAssembly(string requestedName)
		{
			AssemblyName requested = ParseName(requestedName);
			if (requested == null || !IsUnsignedMordexAssembly(requested))
			{
				return null;
			}
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				AssemblyName loaded = assembly.GetName();
				if (string.Equals(loaded.Name, requested.Name, StringComparison.OrdinalIgnoreCase) && IsUnsigned(loaded))
				{
					return assembly;
				}
			}
			return null;
		}

		private static AssemblyName ParseName(string name)
		{
			try
			{
				return new AssemblyName(name);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is FileLoadException)
			{
				// Not a valid assembly name, so it is not one of ours.
				return null;
			}
		}
		private static bool IsUnsignedMordexAssembly(AssemblyName name)
		{
			if (!IsUnsigned(name))
			{
				return false;
			}
			foreach (string mordexName in MordexAssemblyNames)
			{
				if (string.Equals(name.Name, mordexName, StringComparison.OrdinalIgnoreCase))
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
