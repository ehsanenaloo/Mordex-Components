using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerWatermarkImage : IMordexXmlLayoutsSupport
	{
		private MxCalendarView _calendar;

		private MxScheduler _scheduler;

		private Image _watermarkImage;

		private Size _imageSize = Size.Empty;

		private System.Drawing.ContentAlignment _imageAlign = System.Drawing.ContentAlignment.BottomRight;

		private WashMode _washMode;

		private int _alpha = 76;

		private Color _washColor = Color.Empty;

		private Color _maskColor = Color.Empty;

		private Bitmap _washedBitmap;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Represents the System.Drawing.Image object to use as watermark.")]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		public Image Image
		{
			get
			{
				return _watermarkImage;
			}
			set
			{
				if (_watermarkImage != value)
				{
					_watermarkImage = value;
					DisposeResources();
					UpdateLayout();
				}
			}
		}

		[Description("Indicates the size of the image. If empty, the size of the image will be used.")]
		public Size Size
		{
			get
			{
				return _imageSize;
			}
			set
			{
				if (_imageSize != value)
				{
					_imageSize = value;
					UpdateLayout();
				}
			}
		}

		[DefaultValue(System.Drawing.ContentAlignment.BottomRight)]
		[Description("Indicates the alignment of the image on the control.")]
		public System.Drawing.ContentAlignment ImageAlign
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
					UpdateLayout();
				}
			}
		}

		[DefaultValue(WashMode.System)]
		[Description("Determines which color to use to wash the image")]
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
					DisposeResources();
					UpdateLayout();
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
					UpdateLayout();
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
					DisposeResources();
					UpdateLayout();
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
					DisposeResources();
					UpdateLayout();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), 76);
			_watermarkImage = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_maskColor = (Color)LayoutReader.GetPropertyValue(info, "MaskColor", typeof(Color), Color.Empty);
			_imageSize = (Size)LayoutReader.GetPropertyValue(info, "Size", typeof(Size), Size.Empty);
			_washColor = (Color)LayoutReader.GetPropertyValue(info, "WashColor", typeof(Color), Color.Empty);
			_washMode = (WashMode)LayoutReader.GetPropertyValue(info, "WashMode", typeof(WashMode), WashMode.System);
			_imageAlign = (System.Drawing.ContentAlignment)LayoutReader.GetPropertyValue(info, "ImageAlign", typeof(System.Drawing.ContentAlignment), System.Drawing.ContentAlignment.BottomRight);
			DisposeResources();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_alpha != 76)
			{
				info.AddValue("Alpha", _alpha);
			}
			if (_watermarkImage != null)
			{
				info.AddValue("Image", _watermarkImage);
			}
			if (_imageAlign != System.Drawing.ContentAlignment.BottomRight)
			{
				info.AddValue("ImageAlign", _imageAlign);
			}
			if (ShouldSerializeMaskColor())
			{
				info.AddValue("MaskColor", _maskColor);
			}
			if (ShouldSerializeSize())
			{
				info.AddValue("Size", _imageSize);
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

		internal SchedulerWatermarkImage(MxScheduler scheduler)
		{
			_scheduler = scheduler;
			_scheduler.SystemColorsChanged += OnCalendarSystemColorsChanged;
		}

		internal SchedulerWatermarkImage(MxCalendarView calendar)
		{
			_calendar = calendar;
			_calendar.SystemColorsChanged += OnCalendarSystemColorsChanged;
		}

		internal MxScheduler GetSchedule()
		{
			return _scheduler;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_alpha != 76)
			{
				return true;
			}
			if (_watermarkImage != null)
			{
				return true;
			}
			if (_imageAlign != System.Drawing.ContentAlignment.BottomRight)
			{
				return true;
			}
			if (_maskColor != Color.Empty)
			{
				return true;
			}
			if (_imageSize != Size.Empty)
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
			_watermarkImage = null;
			_imageAlign = System.Drawing.ContentAlignment.BottomRight;
			_maskColor = Color.Empty;
			_imageSize = Size.Empty;
			_washColor = Color.Empty;
			_washMode = WashMode.System;
			UpdateLayout();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSize()
		{
			return !_imageSize.Equals(Size.Empty);
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

		private void UpdateLayout()
		{
			if (GetSchedule() != null)
			{
				GetSchedule().Invalidate(invalidateChildren: true);
			}
			else if (_calendar != null)
			{
				_calendar.Invalidate();
			}
		}

		internal void DrawImage(Graphics graphics, Rectangle rectangle)
		{
			if (_watermarkImage == null || _alpha == 0)
			{
				return;
			}
			Size size = _imageSize;
			if (size == Size.Empty)
			{
				size = _watermarkImage.Size;
			}
			if (size.Width != 0 && size.Height != 0)
			{
				Image image = _watermarkImage;
				if (_washedBitmap != null)
				{
					image = _washedBitmap;
				}
				ImageAttributes imageAttributes = new ImageAttributes();
				float opacity = (float)_alpha / 255f;
				float[][] newColorMatrix = new float[5][]
				{
					new float[5] { 1f, 0f, 0f, 0f, 0f },
					new float[5] { 0f, 1f, 0f, 0f, 0f },
					new float[5] { 0f, 0f, 1f, 0f, 0f },
					new float[5] { 0f, 0f, 0f, opacity, 0f },
					new float[5] { 0f, 0f, 0f, 0f, 1f }
				};
				ColorMatrix colorMatrix = new ColorMatrix(newColorMatrix);
				imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default);
				Rectangle destRect = GetImageBounds(rectangle, size.Width, size.Height);
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
				imageAttributes.Dispose();
				imageAttributes = null;
			}
		}

		private Rectangle GetImageBounds(Rectangle rectangle, int imageWidth, int integerArgument)
		{
			switch (_imageAlign)
			{
			case System.Drawing.ContentAlignment.BottomLeft:
			case System.Drawing.ContentAlignment.BottomCenter:
			case System.Drawing.ContentAlignment.BottomRight:
				rectangle.Y = rectangle.Bottom - integerArgument - 2;
				break;
			case System.Drawing.ContentAlignment.MiddleLeft:
			case System.Drawing.ContentAlignment.MiddleCenter:
			case System.Drawing.ContentAlignment.MiddleRight:
				rectangle.Y = rectangle.Top + (rectangle.Height - integerArgument) / 2;
				break;
			}
			switch (_imageAlign)
			{
			case System.Drawing.ContentAlignment.TopCenter:
			case System.Drawing.ContentAlignment.MiddleCenter:
			case System.Drawing.ContentAlignment.BottomCenter:
				rectangle.X += (rectangle.Width - imageWidth) / 2;
				break;
			case System.Drawing.ContentAlignment.TopRight:
			case System.Drawing.ContentAlignment.MiddleRight:
			case System.Drawing.ContentAlignment.BottomRight:
				rectangle.X = rectangle.Right - imageWidth - 2;
				break;
			}
			rectangle.Width = imageWidth;
			rectangle.Height = integerArgument;
			return rectangle;
		}

		private void DisposeResources()
		{
			if (_washedBitmap != null)
			{
				_washedBitmap.Dispose();
				_washedBitmap = null;
			}
			if (_watermarkImage == null || (!(_maskColor != Color.Empty) && _washMode != WashMode.System && !(_washColor != Color.Empty)))
			{
				return;
			}
			_washedBitmap = new Bitmap(_watermarkImage);
			if (_maskColor != Color.Empty)
			{
				_washedBitmap.MakeTransparent(_maskColor);
			}
			else
			{
				_washedBitmap.MakeTransparent(Color.Transparent);
			}
			Color color = _washColor;
			if (_washMode == WashMode.System)
			{
				color = MxThemeManager.ResolveDefault().Color(ThemeRole.BorderStrong);
			}
			if (color != Color.Empty)
			{
				WashBitmap(_washedBitmap, color, 127);
			}
		}

		private void OnCalendarSystemColorsChanged(object sender, EventArgs eventArgs)
		{
			DisposeResources();
		}

		private unsafe Color GetPixelColor(BitmapData bitmapData, int pixelX, int integerArgument)
		{
			if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
			{
				byte* ptr = (byte*)(void*)bitmapData.Scan0 + (long)integerArgument * (long)bitmapData.Stride + (long)pixelX * 4L;
				return Color.FromArgb(ptr[3], ptr[2], ptr[1], *ptr);
			}
			if (bitmapData.PixelFormat == PixelFormat.Format24bppRgb)
			{
				byte* rgbPixelPointer = (byte*)(void*)bitmapData.Scan0 + (long)integerArgument * (long)bitmapData.Stride + (long)pixelX * 3L;
				return Color.FromArgb(rgbPixelPointer[2], rgbPixelPointer[1], *rgbPixelPointer);
			}
			return Color.Empty;
		}

		private unsafe void SetPixelColor(BitmapData bitmapData, Color color, int pixelX, int integerArgument)
		{
			if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
			{
				byte* ptr = (byte*)(void*)bitmapData.Scan0 + (long)integerArgument * (long)bitmapData.Stride + (long)pixelX * 4L;
				*ptr = color.B;
				ptr[1] = color.G;
				ptr[2] = color.R;
				ptr[3] = color.A;
			}
			else if (bitmapData.PixelFormat == PixelFormat.Format24bppRgb)
			{
				byte* rgbPixelPointer = (byte*)(void*)bitmapData.Scan0 + (long)integerArgument * (long)bitmapData.Stride + (long)pixelX * 3L;
				*rgbPixelPointer = color.B;
				rgbPixelPointer[1] = color.G;
				rgbPixelPointer[2] = color.R;
			}
		}

		private void WashBitmap(Bitmap bitmap, Color colorArgument, int brightnessThreshold)
		{
			if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				WashBitmapPerPixel(bitmap, colorArgument, brightnessThreshold);
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
						Color color = GetPixelColor(bitmapData, j, i);
						if (color.A != 0)
						{
							int brightness = (int)Math.Pow(color.R * color.G * color.B, 1.0 / 3.0);
							Color washedColor = color;
							if (brightness > brightnessThreshold)
							{
								float lightBlend = (float)(brightness - brightnessThreshold) / (float)(255 - brightnessThreshold);
								washedColor = BlendColorsWithAlpha(color.A, Color.White, colorArgument, lightBlend);
							}
							else
							{
								float darkBlend = (float)brightness / (float)brightnessThreshold;
								washedColor = BlendColorsWithAlpha(color.A, colorArgument, Color.Black, darkBlend);
							}
							if (!color.Equals(washedColor))
							{
								SetPixelColor(bitmapData, washedColor, j, i);
							}
						}
					}
				}
			}
			catch
			{
			}
			finally
			{
				if (bitmapData != null)
				{
					try
					{
						bitmap.UnlockBits(bitmapData);
					}
					catch
					{
					}
				}
			}
		}

		private void WashBitmapPerPixel(Bitmap bitmap, Color colorArgument, int brightnessThreshold)
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
						int brightness = (int)Math.Pow(pixel.R * pixel.G * pixel.B, 1.0 / 3.0);
						Color color;
						if (brightness > brightnessThreshold)
						{
							float lightBlend = (float)(brightness - brightnessThreshold) / (float)(255 - brightnessThreshold);
							color = BlendColorsWithAlpha(pixel.A, Color.White, colorArgument, lightBlend);
						}
						else
						{
							float darkBlend = (float)brightness / (float)brightnessThreshold;
							color = BlendColorsWithAlpha(pixel.A, colorArgument, Color.Black, darkBlend);
						}
						bitmap.SetPixel(j, i, color);
					}
				}
			}
		}

		private Color BlendColorsWithAlpha(int alpha, Color color, Color colorArgument, float floatValue)
		{
			int red = (int)((float)(int)color.R * floatValue + (float)(int)colorArgument.R * (1f - floatValue));
			int green = (int)((float)(int)color.G * floatValue + (float)(int)colorArgument.G * (1f - floatValue));
			int blue = (int)((float)(int)color.B * floatValue + (float)(int)colorArgument.B * (1f - floatValue));
			return Color.FromArgb(alpha, red, green, blue);
		}
	}
}
