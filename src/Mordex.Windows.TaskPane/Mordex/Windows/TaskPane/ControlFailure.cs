using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mordex.Windows.TaskPane
{
	/// <summary>Shared rules for failures that drawing and layout code may recover from.</summary>
	internal static class ControlFailure
	{
		internal static bool IsRecoverable(Exception exception)
		{
			return exception is ArgumentException
				|| exception is InvalidOperationException
				|| exception is ExternalException
				|| exception is OutOfMemoryException
				|| exception is IOException
				|| exception is NotSupportedException
				|| exception is UnauthorizedAccessException
				|| exception is System.Security.SecurityException;
		}

		internal static void Log(Exception exception, [CallerMemberName] string source = null)
		{
			Debug.WriteLine(source + ": " + exception.GetType().Name + ": " + exception.Message);
		}
	}
}