using System.Drawing;
using System.Globalization;
using System.Resources;

namespace Mordex.Windows.TaskPane
{
	internal class TaskPaneState
	{
		private static ResourceManager _resourceManager;

		internal TaskPaneState()
		{
		}

		internal static ResourceManager GetResourceManager()
		{
			if (_resourceManager == null)
			{
				ResourceManager resourceManager = new ResourceManager(typeof(ResourceFile));
				_resourceManager = resourceManager;
			}
			return _resourceManager;
		}

		internal static Bitmap GetCloseBitmap()
		{
			return (Bitmap)GetResourceManager().GetObject("Close", CultureInfo.CurrentUICulture);
		}

	}
}
