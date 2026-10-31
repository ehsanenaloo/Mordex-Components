using System.Drawing;
using System.Globalization;
using System.Resources;

namespace Mordex.Windows.Navigation
{
	internal class NavigationBarState
	{
		private static ResourceManager _resourceManager;

		internal NavigationBarState()
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

		internal static Icon GetCabIcon()
		{
			object resource = GetResourceManager().GetObject("cab", CultureInfo.CurrentUICulture);
			return (Icon)resource;
		}

		internal static Icon GetCameraIcon()
		{
			object resource = GetResourceManager().GetObject("camera", CultureInfo.CurrentUICulture);
			return (Icon)resource;
		}

		internal static Bitmap GetDownBitmap()
		{
			object resource = GetResourceManager().GetObject("Down", CultureInfo.CurrentUICulture);
			return (Bitmap)resource;
		}

		internal static Icon GetFaxIcon()
		{
			object resource = GetResourceManager().GetObject("fax", CultureInfo.CurrentUICulture);
			return (Icon)resource;
		}

		internal static Bitmap GetLeftBitmap()
		{
			object resource = GetResourceManager().GetObject("Left", CultureInfo.CurrentUICulture);
			return (Bitmap)resource;
		}

		internal static Bitmap GetRightBitmap()
		{
			object resource = GetResourceManager().GetObject("Right", CultureInfo.CurrentUICulture);
			return (Bitmap)resource;
		}

		internal static Bitmap GetUpBitmap()
		{
			object resource = GetResourceManager().GetObject("Up", CultureInfo.CurrentUICulture);
			return (Bitmap)resource;
		}
	}
}
