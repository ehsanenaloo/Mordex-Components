using System.Drawing;
using System.Globalization;
using System.Resources;

namespace Mordex.Windows.Timeline
{
	internal class TimelineImageResources
	{
		private static ResourceManager _resourceManager;

		internal TimelineImageResources()
		{
		}

		internal static ResourceManager GetResourceManager()
		{
			if (object.ReferenceEquals(_resourceManager, null))
			{
				ResourceManager resourceManager = new ResourceManager(typeof(ResourceFile));
				_resourceManager = resourceManager;
			}
			return _resourceManager;
		}

		internal static Bitmap GetCollapsedBitmap()
		{
			object resource = GetResourceManager().GetObject("collapsed", null);
			return (Bitmap)resource;
		}

		internal static Bitmap GetExpandedBitmap()
		{
			object resource = GetResourceManager().GetObject("expanded", null);
			return (Bitmap)resource;
		}

		internal static Bitmap GetLeftArrowBitmap()
		{
			object resource = GetResourceManager().GetObject("LeftXP", null);
			return (Bitmap)resource;
		}

		internal static Bitmap GetRightArrowBitmap()
		{
			object resource = GetResourceManager().GetObject("RightXP", null);
			return (Bitmap)resource;
		}
	}
}
