using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	public class ImageEditor : UITypeEditor
	{
		public ImageEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "All image Files(*.bmp;*gif;*.jpg;*jpeg;*.png;*.ico;*emf;*wmf)|*.bmp;*gif;*.jpg;*jpeg;*.png;*.ico;*emf;*wmf|Bitmap files (*.bmp;*gif;*.jpg;*jpeg;*.png;*.ico)|*.bmp;*gif;*.jpg;*jpeg;*.png;*.ico|Metafiles(*emf;*wmf)|*emf;*wmf";
				DialogResult dialogResult = openFileDialog.ShowDialog();
				if (dialogResult == DialogResult.OK)
				{
					FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
					if (fileInfo.Exists)
					{
						try
						{
							value = Image.FromFile(openFileDialog.FileName);
						}
						catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
						{
							GridExceptions.Report(caught);
							throw new ArgumentException();
						}
					}
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
			Image image = (Image)e.Value;
			if (image != null)
			{
				try
				{
					e.Graphics.DrawImage(image, e.Bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
					return;
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
		}
	}
}
