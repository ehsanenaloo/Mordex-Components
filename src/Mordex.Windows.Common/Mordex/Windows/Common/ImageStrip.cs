using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	public static class ImageStrip
	{
		public static Bitmap ToStrip(ImageList imageList)
		{
			Size size = imageList.ImageSize;
			Bitmap strip = new Bitmap(size.Width * imageList.Images.Count, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics graphics = Graphics.FromImage(strip))
			{
				for (int i = 0; i < imageList.Images.Count; i++)
				{
					graphics.DrawImage(imageList.Images[i], i * size.Width, 0, size.Width, size.Height);
				}
			}
			return strip;
		}

		public static void FromStrip(ImageList imageList, Image strip)
		{
			if (strip != null && strip.Width >= imageList.ImageSize.Width && imageList.ImageSize.Width > 0)
			{
				imageList.ColorDepth = ColorDepth.Depth32Bit;
				imageList.Images.AddStrip(strip);
			}
		}

		public static void Load(ImageList imageList, ResourceManager resources, string name)
		{
			Bitmap strip = (Bitmap)resources.GetObject(name);
			if (strip == null)
			{
				return;
			}
			imageList.ColorDepth = ColorDepth.Depth32Bit;
			imageList.Images.AddStrip(strip);
		}
	}
}
