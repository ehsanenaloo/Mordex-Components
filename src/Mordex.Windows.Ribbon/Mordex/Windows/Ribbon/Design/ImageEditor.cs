using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	public class ImageEditor : UITypeEditor
	{
		private const string ImageFilter = "All image Files(*.bmp;*gif;*.jpg;*jpeg;*.png;*.ico;*emf;*wmf)|*.bmp;*gif;*.jpg;*jpeg;*.png;*.ico;*emf;*wmf|Bitmap files (*.bmp;*gif;*.jpg;*jpeg;*.png;*.ico)|*.bmp;*gif;*.jpg;*jpeg;*.png;*.ico|Metafiles(*emf;*wmf)|*emf;*wmf";

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = ImageFilter;
				if (openFileDialog.ShowDialog() == DialogResult.OK && File.Exists(openFileDialog.FileName))
				{
					// Read the file into memory so the picture does not keep the file locked.
					value = Image.FromStream(new MemoryStream(File.ReadAllBytes(openFileDialog.FileName)));
				}
			}
			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return true;
			}
			return false;
		}

		public override void PaintValue(PaintValueEventArgs e)
		{
			if (e.Context == null || e.Context.Instance == null)
			{
				return;
			}
			if (e.Value is Image image)
			{
				try
				{
					e.Graphics.DrawImage(image, e.Bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
					return;
				}
				catch (Exception ex) when (ex is ArgumentException || ex is ExternalException)
				{
					// A bad image falls back to the plain background below.
				}
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
		}
	}
}
