using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace Mordex.Demo
{
	/// <summary>Draws small icons from the system symbol font, so the demo needs no image files.</summary>
	internal static class DemoIcons
	{
		public const int Home = 0xE80F;
		public const int Mail = 0xE715;
		public const int Calendar = 0xE787;
		public const int Contact = 0xE77B;
		public const int Settings = 0xE713;
		public const int Folder = 0xE8B7;
		public const int Document = 0xE8A5;
		public const int Search = 0xE721;
		public const int Save = 0xE74E;
		public const int Cut = 0xE8C6;
		public const int Copy = 0xE8C8;
		public const int Paste = 0xE77F;
		public const int Undo = 0xE7A7;
		public const int Redo = 0xE7A6;
		public const int Print = 0xE749;
		public const int Delete = 0xE74D;
		public const int Add = 0xE710;
		public const int Refresh = 0xE72C;
		public const int Help = 0xE897;
		public const int Edit = 0xE70F;
		public const int Download = 0xE896;
		public const int Send = 0xE724;
		public const int Task = 0xE762;
		public const int Chart = 0xE9D2;
		public const int Cancel = 0xE711;
		public const int Play = 0xE768;
		public const int Tag = 0xE8EC;
		public const int Pin = 0xE718;
		public const int Archive = 0xE7B8;
		public const int Contactinfo = 0xE779;

		private static readonly string[] SymbolFonts = { "Segoe Fluent Icons", "Segoe MDL2 Assets" };

		public static Bitmap Glyph(int code, Color color, int size)
		{
			Bitmap bitmap = new Bitmap(size, size);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				string family = SymbolFonts.FirstOrDefault(name => FontFamily.Families.Any(f => f.Name == name));
				using (Brush brush = new SolidBrush(color))
				{
					if (family == null)
					{
						g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
						g.FillEllipse(brush, 2, 2, size - 4, size - 4);
						return bitmap;
					}
					using (Font font = new Font(family, size * 0.8f, FontStyle.Regular, GraphicsUnit.Pixel))
					using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
					{
						g.DrawString(char.ConvertFromUtf32(code), font, brush, new RectangleF(0, 0, size, size), format);
					}
				}
			}
			return bitmap;
		}

		public static ImageList List(int size, Color color, params int[] codes)
		{
			ImageList list = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(size, size) };
			Bitmap[] bitmaps = codes.Select(code => Glyph(code, color, size)).ToArray();
			foreach (Bitmap bitmap in bitmaps)
			{
				list.Images.Add(bitmap);
			}
			// The list reads the bitmaps when its handle is created, so create it before they are released.
			_ = list.Handle;
			foreach (Bitmap bitmap in bitmaps)
			{
				bitmap.Dispose();
			}
			return list;
		}
	}
}
