using System;
using System.Collections.Generic;
using System.Drawing;
using System.Resources;

namespace Mordex.Windows.FilterBuilder
{
	internal static class ResourceFile
	{
		private static readonly object Sync = new object();

		private static readonly Dictionary<string, Bitmap> Images = new Dictionary<string, Bitmap>();

		private static ResourceManager _resourceManager;

		internal static Bitmap AddCriterium => TransparentImage("AddCriterium");

		internal static Bitmap RemoveCriterium => TransparentImage("RemoveCriterium");

		internal static Bitmap FieldsButton => TransparentImage("Fields1");

		private static Bitmap TransparentImage(string name)
		{
			lock (Sync)
			{
				if (!Images.TryGetValue(name, out Bitmap image))
				{
					if (_resourceManager == null)
					{
						_resourceManager = new ResourceManager("Mordex.Windows.FilterBuilder.ResourceFile", typeof(ResourceFile).Assembly);
					}
					image = (Bitmap)_resourceManager.GetObject(name);
					try
					{
						image.MakeTransparent(Color.Magenta);
					}
					catch (InvalidOperationException)
					{
						// Indexed images cannot be made transparent; they are used as they are.
					}
					Images.Add(name, image);
				}
				return image;
			}
		}
	}
}
