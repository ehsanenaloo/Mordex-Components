using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Mordex.Windows.Common.Design
{
	internal static class AssemblyLoaderHelper
	{
		private const string CommonAssemblyName = "Mordex.Windows.Common";

		private static int _attached;

		internal static void AttachCurrentDomainEvents()
		{
			if (Interlocked.Exchange(ref _attached, 1) == 0)
			{
				AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			}
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (!IsUnsignedCommonAssembly(args.Name))
			{
				return null;
			}
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (IsUnsignedCommonAssembly(assembly.FullName))
				{
					return assembly;
				}
			}
			return null;
		}

		private static bool IsUnsignedCommonAssembly(string fullName)
		{
			AssemblyName name;
			try
			{
				name = new AssemblyName(fullName);
			}
			catch (Exception ex) when (ex is ArgumentException || ex is FileLoadException)
			{
				return false;
			}
			byte[] token = name.GetPublicKeyToken();
			return name.Name == CommonAssemblyName && (token == null || token.Length == 0);
		}
	}
}
