using System;
using System.Reflection;

namespace Mordex.Windows.Common.Design
{
	internal static class CommonState
	{
		private static bool? _designAssemblyAvailable = null;

		internal static void EnsureDesignAssemblyLoaded()
		{
			if (!_designAssemblyAvailable.HasValue)
			{
				try
				{
					Assembly.Load("Mordex.Windows.Common.Design, Version=" + typeof(CommonState).Assembly.GetName().Version + ", Culture=neutral, PublicKeyToken=null");
					_designAssemblyAvailable = true;
				}
				catch (Exception ex) when (ex is System.IO.IOException || ex is BadImageFormatException)
				{
					_designAssemblyAvailable = false;
				}
			}
			if (!_designAssemblyAvailable.Value)
			{
				throw new Exception("This Form uses a Mordex WinForms Control that was not properly installed in this machine.  The Form Designer was prevented from being shown in order to avoid a possible corruption of the Form Settings. Please install the Mordex WinForms Controls using the Controls Setup in order to edit this Form.");
			}
		}
	}
}
