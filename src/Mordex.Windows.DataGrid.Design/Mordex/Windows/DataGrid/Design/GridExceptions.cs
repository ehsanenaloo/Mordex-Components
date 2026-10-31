using System;
using System.Diagnostics;
using System.Threading;

namespace Mordex.Windows.DataGrid.Design
{
	/// <summary>
	/// Shared handling for best-effort code paths (painting, layout files, designer helpers) where a failure must not
	/// bring the host application down. Fatal runtime errors are never swallowed, everything else goes to the trace output.
	/// </summary>
	internal static class GridExceptions
	{
		internal static bool IsRecoverable(Exception exception)
		{
			return !(exception is OutOfMemoryException || exception is StackOverflowException || exception is AccessViolationException || exception is ThreadAbortException || exception is AppDomainUnloadedException);
		}

		internal static void Report(Exception exception)
		{
			Trace.TraceWarning("Mordex DataGrid: {0}", exception);
		}
	}
}
