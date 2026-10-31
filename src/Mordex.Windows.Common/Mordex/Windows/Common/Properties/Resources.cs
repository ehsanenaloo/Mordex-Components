using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Mordex.Windows.Common.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager _resourceManager;

		private static CultureInfo _cultureInfo;

		internal Resources()
		{
		}

		internal static ResourceManager GetResourceManager()
		{
			if (object.ReferenceEquals(_resourceManager, null))
			{
				ResourceManager resourceManager = new ResourceManager("Mordex.Windows.Common.Properties.Resources", typeof(Resources).Assembly);
				_resourceManager = resourceManager;
			}
			return _resourceManager;
		}

		internal static CultureInfo GetCultureInfo()
		{
			return _cultureInfo;
		}

		internal static void SetCultureInfo(CultureInfo cultureInfo)
		{
			_cultureInfo = cultureInfo;
		}
	}
}
