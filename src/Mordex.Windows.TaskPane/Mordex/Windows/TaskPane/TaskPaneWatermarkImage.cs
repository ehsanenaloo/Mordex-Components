using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public class TaskPaneWatermarkImage : IMordexXmlLayoutsSupport
	{
		private TaskPaneGroup _group;

		private MxTaskPane _taskPane;

		private Image _image;

		private Size _size = Size.Empty;

		private ContentAlignment _imageAlign = ContentAlignment.BottomRight;

		private WashMode _washMode;

		private int _alpha = 76;

		private Color _washColor = Color.Empty;

		private Color _maskColor = Color.Empty;

		private Bitmap _washedBitmap;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(null)]
		[Description("Represents the System.Drawing.Image object to use as watermark.")]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		public Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				if (_image != value)
				{
					_image = value;
					RebuildWashedBitmap();
					InvalidateTaskPane();
				}
			}
		}

		[Description("Indicates the size of the image. If empty, the size of the image will be used.")]
		public Size Size
		{
			get
			{
				return _size;
			}
			set
			{
				if (_size != value)
				{
					_size = value;
					InvalidateTaskPane();
				}
			}
		}

		[Description("Indicates the alignment of the image on the control.")]
		[DefaultValue(ContentAlignment.BottomRight)]
		public ContentAlignment ImageAlign
		{
			get
			{
				return _imageAlign;
			}
			set
			{
				if (_imageAlign != value)
				{
					_imageAlign = value;
					InvalidateTaskPane();
				}
			}
		}

		[Description("Determines which color to use to wash the image")]
		[DefaultValue(WashMode.System)]
		public WashMode WashMode
		{
			get
			{
				return _washMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(WashMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_washMode != value)
				{
					_washMode = value;
					RebuildWashedBitmap();
					InvalidateTaskPane();
				}
			}
		}

		[DefaultValue(76)]
		[Description("Indicates the alpha component that is applied when drawing the image.")]
		public int Alpha
		{
			get
			{
				return _alpha;
			}
			set
			{
				if (value < 0 || value > 255)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (_alpha != value)
				{
					_alpha = value;
					InvalidateTaskPane();
				}
			}
		}

		[Description("The color to apply to the image when WashMode is UseWashColor")]
		public Color WashColor
		{
			get
			{
				return _washColor;
			}
			set
			{
				if (_washColor != value)
				{
					_washColor = value;
					RebuildWashedBitmap();
					InvalidateTaskPane();
				}
			}
		}

		[Description("The color to treat as transparent.")]
		public Color MaskColor
		{
			get
			{
				return _maskColor;
			}
			set
			{
				if (_maskColor != value)
				{
					_maskColor = value;
					RebuildWashedBitmap();
					InvalidateTaskPane();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), 76);
			_image = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_maskColor = (Color)LayoutReader.GetPropertyValue(info, "MaskColor", typeof(Color), Color.Empty);
			_size = (Size)LayoutReader.GetPropertyValue(info, "Size", typeof(Size), Size.Empty);
			_washColor = (Color)LayoutReader.GetPropertyValue(info, "WashColor", typeof(Color), Color.Empty);
			_washMode = (WashMode)LayoutReader.GetPropertyValue(info, "WashMode", typeof(WashMode), WashMode.System);
			_imageAlign = (ContentAlignment)LayoutReader.GetPropertyValue(info, "ImageAlign", typeof(ContentAlignment), ContentAlignment.BottomRight);
			RebuildWashedBitmap();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_alpha != 76)
			{
				info.AddValue("Alpha", _alpha);
			}
			if (_image != null)
			{
				info.AddValue("Image", _image);
			}
			if (_imageAlign != ContentAlignment.BottomRight)
			{
				info.AddValue("ImageAlign", _imageAlign);
			}
			if (ShouldSerializeMaskColor())
			{
				info.AddValue("MaskColor", _maskColor);
			}
			if (ShouldSerializeSize())
			{
				info.AddValue("Size", _size);
			}
			if (ShouldSerializeWashColor())
			{
				info.AddValue("WashColor", _washColor);
			}
			if (_washMode != WashMode.System)
			{
				info.AddValue("WashMode", _washMode);
			}
		}

		internal TaskPaneWatermarkImage(MxTaskPane explorer)
		{
			_taskPane = explorer;
		}

		internal TaskPaneWatermarkImage(TaskPaneGroup group)
		{
			_group = group;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_alpha != 76)
			{
				return true;
			}
			if (_image != null)
			{
				return true;
			}
			if (_imageAlign != ContentAlignment.BottomRight)
			{
				return true;
			}
			if (_maskColor != Color.Empty)
			{
				return true;
			}
			if (_size != Size.Empty)
			{
				return true;
			}
			if (_washColor != Color.Empty)
			{
				return true;
			}
			if (_washMode != WashMode.System)
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_alpha = 76;
			_image = null;
			_imageAlign = ContentAlignment.BottomRight;
			_maskColor = Color.Empty;
			_size = Size.Empty;
			_washColor = Color.Empty;
			_washMode = WashMode.System;
			InvalidateTaskPane();
		}

		internal MxTaskPane ResolveTaskPane()
		{
			if (_taskPane != null)
			{
				return _taskPane;
			}
			if (_group != null)
			{
				return _group.TaskPane;
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSize()
		{
			return !_size.Equals(Size.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSize()
		{
			Size = Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWashColor()
		{
			return !_washColor.Equals(Color.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWashColor()
		{
			WashColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaskColor()
		{
			return !_maskColor.Equals(Color.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaskColor()
		{
			MaskColor = Color.Empty;
		}

		private void InvalidateTaskPane()
		{
			if (ResolveTaskPane() != null)
			{
				ResolveTaskPane().Invalidate(invalidateChildren: true);
			}
		}

		internal void DrawImage(Graphics graphics, Rectangle bounds)
		{
			if (_image == null || _alpha == 0)
			{
				return;
			}
			Size imageSize = _size;
			if (imageSize == Size.Empty)
			{
				imageSize = _image.Size;
			}
			if (imageSize.Width != 0 && imageSize.Height != 0)
			{
				Image sourceImage = _image;
				if (_washedBitmap != null)
				{
					sourceImage = _washedBitmap;
				}
				ImageAttributes imageAttributes = new ImageAttributes();
				float alphaFactor = (float)_alpha / 255f;
				float[][] newColorMatrix = new float[5][]
				{
					new float[5] { 1f, 0f, 0f, 0f, 0f },
					new float[5] { 0f, 1f, 0f, 0f, 0f },
					new float[5] { 0f, 0f, 1f, 0f, 0f },
					new float[5] { 0f, 0f, 0f, alphaFactor, 0f },
					new float[5] { 0f, 0f, 0f, 0f, 1f }
				};
				ColorMatrix colorMatrix = new ColorMatrix(newColorMatrix);
				imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default);
				Rectangle destRect = GetAlignedBounds(bounds, imageSize.Width, imageSize.Height);
				graphics.DrawImage(sourceImage, destRect, 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, imageAttributes);
				imageAttributes.Dispose();
				imageAttributes = null;
			}
		}

		private Rectangle GetAlignedBounds(Rectangle bounds, int imageWidth, int imageHeight)
		{
			switch (_imageAlign)
			{
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomRight:
				bounds.Y = bounds.Bottom - imageHeight - 2;
				break;
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.MiddleRight:
				bounds.Y = bounds.Top + (bounds.Height - imageHeight) / 2;
				break;
			}
			switch (_imageAlign)
			{
			case ContentAlignment.TopCenter:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.BottomCenter:
				bounds.X += (bounds.Width - imageWidth) / 2;
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				bounds.X = bounds.Right - imageWidth - 2;
				break;
			}
			bounds.Width = imageWidth;
			bounds.Height = imageHeight;
			return bounds;
		}

		internal void DisposeWashedBitmap()
		{
			_washedBitmap?.Dispose();
			_washedBitmap = null;
		}

		internal void RebuildWashedBitmap()
		{
			if (_taskPane == null || _taskPane.IsInitializing())
			{
				return;
			}
			if (_washedBitmap != null)
			{
				_washedBitmap.Dispose();
				_washedBitmap = null;
			}
			if (_image == null || (!(_maskColor != Color.Empty) && _washMode != WashMode.System && !(_washColor != Color.Empty)))
			{
				return;
			}
			_washedBitmap = new Bitmap(_image);
			if (_maskColor != Color.Empty)
			{
				_washedBitmap.MakeTransparent(_maskColor);
			}
			Color washColor = _washColor;
			if (_washMode == WashMode.System)
			{
				washColor = SystemColors.ControlDark;
			}
			if (washColor != Color.Empty)
			{
				ApplyWash(_washedBitmap, washColor, 127);
			}
		}

		private unsafe Color GetPixelColor(BitmapData bitmapData, int x, int y)
		{
			if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
			{
				byte* ptr = (byte*)(void*)bitmapData.Scan0 + (long)y * (long)bitmapData.Stride + (long)x * 4L;
				return Color.FromArgb(ptr[3], ptr[2], ptr[1], *ptr);
			}
			if (bitmapData.PixelFormat == PixelFormat.Format24bppRgb)
			{
				byte* rgbPixelPtr = (byte*)(void*)bitmapData.Scan0 + (long)y * (long)bitmapData.Stride + (long)x * 3L;
				return Color.FromArgb(rgbPixelPtr[2], rgbPixelPtr[1], *rgbPixelPtr);
			}
			return Color.Empty;
		}

		private unsafe void WritePixel(BitmapData bitmapData, Color newColor, int x, int y)
		{
			if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
			{
				byte* ptr = (byte*)(void*)bitmapData.Scan0 + (long)y * (long)bitmapData.Stride + (long)x * 4L;
				*ptr = newColor.B;
				ptr[1] = newColor.G;
				ptr[2] = newColor.R;
				ptr[3] = newColor.A;
			}
			else if (bitmapData.PixelFormat == PixelFormat.Format24bppRgb)
			{
				byte* rgbPixelPtr = (byte*)(void*)bitmapData.Scan0 + (long)y * (long)bitmapData.Stride + (long)x * 3L;
				*rgbPixelPtr = newColor.B;
				rgbPixelPtr[1] = newColor.G;
				rgbPixelPtr[2] = newColor.R;
			}
		}

		private void ApplyWash(Bitmap bitmap, Color washColor, int grayThreshold)
		{
			if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				ApplyWashSlow(bitmap, washColor, grayThreshold);
				return;
			}
			BitmapData bitmapData = null;
			try
			{
				int height = bitmap.Height;
				int width = bitmap.Width;
				bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						Color pixelColor = GetPixelColor(bitmapData, j, i);
						if (pixelColor.A != 0)
						{
							int grayLevel = (int)Math.Pow(pixelColor.R * pixelColor.G * pixelColor.B, 1.0 / 3.0);
							Color washedColor = pixelColor;
							if (grayLevel > grayThreshold)
							{
								float lightBlend = (float)(grayLevel - grayThreshold) / (float)(255 - grayThreshold);
								washedColor = BlendWashColor(pixelColor.A, Color.White, washColor, lightBlend);
							}
							else
							{
								float darkBlend = (float)grayLevel / (float)grayThreshold;
								washedColor = BlendWashColor(pixelColor.A, washColor, Color.Black, darkBlend);
							}
							if (!pixelColor.Equals(washedColor))
							{
								WritePixel(bitmapData, washedColor, j, i);
							}
						}
					}
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
			finally
			{
				if (bitmapData != null)
				{
					try
					{
						bitmap.UnlockBits(bitmapData);
					}
					catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
					{
						ControlFailure.Log(failure);
					}
				}
			}
		}

		private void ApplyWashSlow(Bitmap bitmap, Color washColor, int grayThreshold)
		{
			int height = bitmap.Height;
			int width = bitmap.Width;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					Color pixel = bitmap.GetPixel(j, i);
					if (pixel.A != 0)
					{
						int grayLevel = (int)Math.Pow(pixel.R * pixel.G * pixel.B, 1.0 / 3.0);
						Color washedColor;
						if (grayLevel > grayThreshold)
						{
							float lightBlend = (float)(grayLevel - grayThreshold) / (float)(255 - grayThreshold);
							washedColor = BlendWashColor(pixel.A, Color.White, washColor, lightBlend);
						}
						else
						{
							float darkBlend = (float)grayLevel / (float)grayThreshold;
							washedColor = BlendWashColor(pixel.A, washColor, Color.Black, darkBlend);
						}
						bitmap.SetPixel(j, i, washedColor);
					}
				}
			}
		}

		private Color BlendWashColor(int alpha, Color primaryColor, Color secondaryColor, float floatValue)
		{
			int red = (int)((float)(int)primaryColor.R * floatValue + (float)(int)secondaryColor.R * (1f - floatValue));
			int green = (int)((float)(int)primaryColor.G * floatValue + (float)(int)secondaryColor.G * (1f - floatValue));
			int blue = (int)((float)(int)primaryColor.B * floatValue + (float)(int)secondaryColor.B * (1f - floatValue));
			return Color.FromArgb(alpha, red, green, blue);
		}
	}
}
