using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation.Design
{
	public class ImageEditor : UITypeEditor
	{
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
						value = Image.FromFile(openFileDialog.FileName);
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
			if (e.Value is Image previewImage)
			{
				try
				{
					e.Graphics.DrawImage(previewImage, e.Bounds, 0, 0, previewImage.Width, previewImage.Height, GraphicsUnit.Pixel);
					return;
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
				}
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
		}
	}
}
