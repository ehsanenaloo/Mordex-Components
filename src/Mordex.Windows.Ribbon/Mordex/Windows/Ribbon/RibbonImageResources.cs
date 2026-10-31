using System.Drawing;
using System.Resources;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonImageResources
	{
		private const string ResourceBaseName = "Mordex.Windows.Ribbon.ResourceFile";

		private static ResourceManager s_resourceManager;

		internal RibbonImageResources()
		{
		}

		internal static ResourceManager GetResourceManager()
		{
			if (object.ReferenceEquals(s_resourceManager, null))
			{
				ResourceManager resourceManager = new ResourceManager(ResourceBaseName, typeof(RibbonImageResources).Assembly);
				s_resourceManager = resourceManager;
			}
			return s_resourceManager;
		}

		internal static Bitmap GetHelp2010Bitmap()
		{
			object resource = GetResourceManager().GetObject("help2010");
			return (Bitmap)resource;
		}

	}
}
