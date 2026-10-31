using System;
using Mordex.Windows.DataGrid.Internal;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class GridWatermarkImage : IMordexXmlLayoutsSupport
	{
		internal MxDataGrid _grid;

		private Image _sourceImage;

		private Size _imageSize = Size.Empty;

		private ContentAlignment _imageAlign = ContentAlignment.BottomRight;

		private WashMode _washMode;

		private int _alpha = 76;

		private Color _tintColor = Color.Empty;

		private Color _transparentColor = Color.Empty;

		private Bitmap _renderedBitmap;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("The System.Drawing.Image object to use as watermark.")]
		[DefaultValue(null)]
		public Image Image
		{
			get
			{
				return _sourceImage;
			}
			set
			{
				if (_sourceImage != value)
				{
					_sourceImage = value;
					DisposeResources();
					InvalidateGrid();
				}
			}
		}

		[Description("Indicates the size of the image.")]
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
					InvalidateGrid();
				}
			}
		}

		[DefaultValue(ContentAlignment.BottomRight)]
		[Description("Indicates the alignment of the image on the control.")]
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
					InvalidateGrid();
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
					InvalidateGrid();
				}
			}
		}

		[Description("Indicates the alpha component that is applied when drawing the image.")]
		[DefaultValue(76)]
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
					InvalidateGrid();
				}
			}
		}

		[Description("Indicates the color to apply to the image when WashMode is UseWashColor")]
		public Color WashColor
		{
			get
			{
				return _tintColor;
			}
			set
			{
				if (_tintColor != value)
				{
					_tintColor = value;
					DisposeResources();
					InvalidateGrid();
				}
			}
		}

		[Description("Indicates the color to treat as transparent.")]
		public Color MaskColor
		{
			get
			{
				return _transparentColor;
			}
			set
			{
				if (_transparentColor != value)
				{
					_transparentColor = value;
					DisposeResources();
					InvalidateGrid();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), 76);
			_sourceImage = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_transparentColor = (Color)LayoutReader.GetPropertyValue(info, "MaskColor", typeof(Color), Color.Empty);
			_imageSize = (Size)LayoutReader.GetPropertyValue(info, "Size", typeof(Size), Size.Empty);
			_tintColor = (Color)LayoutReader.GetPropertyValue(info, "WashColor", typeof(Color), Color.Empty);
			_washMode = (WashMode)LayoutReader.GetPropertyValue(info, "WashMode", typeof(WashMode), WashMode.System);
			_imageAlign = (ContentAlignment)LayoutReader.GetPropertyValue(info, "ImageAlign", typeof(ContentAlignment), ContentAlignment.BottomRight);
			DisposeResources();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_alpha != 76)
			{
				info.AddValue("Alpha", _alpha);
			}
			if (_sourceImage != null)
			{
				info.AddValue("Image", _sourceImage);
			}
			if (_imageAlign != ContentAlignment.BottomRight)
			{
				info.AddValue("ImageAlign", _imageAlign);
			}
			if (ShouldSerializeMaskColor())
			{
				info.AddValue("MaskColor", _transparentColor);
			}
			if (ShouldSerializeSize())
			{
				info.AddValue("Size", _imageSize);
			}
			if (ShouldSerializeWashColor())
			{
				info.AddValue("WashColor", _tintColor);
			}
			if (_washMode != WashMode.System)
			{
				info.AddValue("WashMode", _washMode);
			}
		}

		internal GridWatermarkImage(MxDataGrid grid)
		{
			_grid = grid;
			if (_grid != null)
			{
				_grid.SystemColorsChanged += OnGridSystemColorsChanged;
			}
		}

		internal void DetachFromGrid()
		{
			if (_grid != null)
			{
				_grid.SystemColorsChanged -= OnGridSystemColorsChanged;
				_grid = null;
			}
		}

		private void OnGridSystemColorsChanged(object sender, EventArgs eventArgs)
		{
			DisposeResources();
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
			return !_tintColor.Equals(Color.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWashColor()
		{
			WashColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaskColor()
		{
			return !_transparentColor.Equals(Color.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaskColor()
		{
			MaskColor = Color.Empty;
		}

		private void InvalidateGrid()
		{
			if (_grid != null)
			{
				_grid.Invalidate();
			}
		}

		internal void DrawWatermark(Graphics graphics, Rectangle rectangle)
		{
			if (_sourceImage == null || _alpha == 0)
			{
				return;
			}
			Size size = _imageSize;
			if (size == Size.Empty)
			{
				size = _sourceImage.Size;
			}
			if (size.Width != 0 && size.Height != 0)
			{
				Image image = _sourceImage;
				if (_renderedBitmap != null)
				{
					image = _renderedBitmap;
				}
				ImageAttributes imageAttributes = new ImageAttributes();
				float amount = (float)_alpha / 255f;
				float[][] newColorMatrix = new float[5][]
				{
					new float[5] { 1f, 0f, 0f, 0f, 0f },
					new float[5] { 0f, 1f, 0f, 0f, 0f },
					new float[5] { 0f, 0f, 1f, 0f, 0f },
					new float[5] { 0f, 0f, 0f, amount, 0f },
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

		private Rectangle GetImageBounds(Rectangle rectangle, int width, int height)
		{
			switch (_imageAlign)
			{
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomRight:
				rectangle.Y = rectangle.Bottom - height - 2;
				break;
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.MiddleRight:
				rectangle.Y = rectangle.Top + (rectangle.Height - height) / 2;
				break;
			}
			switch (_imageAlign)
			{
			case ContentAlignment.TopCenter:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.BottomCenter:
				rectangle.X += (rectangle.Width - width) / 2;
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				rectangle.X = rectangle.Right - width;
				break;
			}
			rectangle.Width = width;
			rectangle.Height = height;
			return rectangle;
		}

		private void DisposeResources()
		{
			if (_renderedBitmap != null)
			{
				_renderedBitmap.Dispose();
				_renderedBitmap = null;
			}
			if (_sourceImage != null && (_transparentColor != Color.Empty || _washMode == WashMode.System || _tintColor != Color.Empty))
			{
				_renderedBitmap = new Bitmap(_sourceImage);
				if (_transparentColor != Color.Empty)
				{
					_renderedBitmap.MakeTransparent(_transparentColor);
				}
				else
				{
					_renderedBitmap.MakeTransparent(Color.Transparent);
				}
				Color color = _tintColor;
				if (_washMode == WashMode.System)
				{
					color = ((!_grid.ControlStyle.GetLayoutState().GetGridlinesStyle()._isThemed) ? SystemColors.ControlDark : _grid._gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
						.GetGridColor(GridColorKey.SelectedCardCaption));
				}
				if (color != Color.Empty)
				{
					TintBitmap(_renderedBitmap, color, 127);
				}
			}
		}

		private unsafe Color GetPixelColor(BitmapData bitmapData, int value, int otherValue)
		{
			if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
			{
				byte* item = (byte*)(void*)bitmapData.Scan0 + (long)otherValue * (long)bitmapData.Stride + (long)value * 4L;
				return Color.FromArgb(item[3], item[2], item[1], *item);
			}
			if (bitmapData.PixelFormat == PixelFormat.Format24bppRgb)
			{
				byte* otherItem = (byte*)(void*)bitmapData.Scan0 + (long)otherValue * (long)bitmapData.Stride + (long)value * 3L;
				return Color.FromArgb(otherItem[2], otherItem[1], *otherItem);
			}
			return Color.Empty;
		}

		private unsafe void ProcessBitmapData(BitmapData bitmapData, Color color, int value, int otherValue)
		{
			if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
			{
				byte* item = (byte*)(void*)bitmapData.Scan0 + (long)otherValue * (long)bitmapData.Stride + (long)value * 4L;
				*item = color.B;
				item[1] = color.G;
				item[2] = color.R;
				item[3] = color.A;
			}
			else if (bitmapData.PixelFormat == PixelFormat.Format24bppRgb)
			{
				byte* otherItem = (byte*)(void*)bitmapData.Scan0 + (long)otherValue * (long)bitmapData.Stride + (long)value * 3L;
				*otherItem = color.B;
				otherItem[1] = color.G;
				otherItem[2] = color.R;
			}
		}

		private void TintBitmap(Bitmap bitmap, Color otherColor, int value)
		{
			if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				TintBitmapSlow(bitmap, otherColor, value);
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
							int otherValue = (int)Math.Pow(color.R * color.G * color.B, 1.0 / 3.0);
							Color nextColor = color;
							if (otherValue > value)
							{
								float floatValue = (float)(otherValue - value) / (float)(255 - value);
								nextColor = BlendColorsWithAlpha(color.A, Color.White, otherColor, floatValue);
							}
							else
							{
								float otherFloatValue = (float)otherValue / (float)value;
								nextColor = BlendColorsWithAlpha(color.A, otherColor, Color.Black, otherFloatValue);
							}
							if (!color.Equals(nextColor))
							{
								ProcessBitmapData(bitmapData, nextColor, j, i);
							}
						}
					}
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			finally
			{
				if (bitmapData != null)
				{
					try
					{
						bitmap.UnlockBits(bitmapData);
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
			}
		}

		private void TintBitmapSlow(Bitmap bitmap, Color otherColor, int value)
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
						int otherValue = (int)Math.Pow(pixel.R * pixel.G * pixel.B, 1.0 / 3.0);
						Color color;
						if (otherValue > value)
						{
							float floatValue = (float)(otherValue - value) / (float)(255 - value);
							color = BlendColorsWithAlpha(pixel.A, Color.White, otherColor, floatValue);
						}
						else
						{
							float otherFloatValue = (float)otherValue / (float)value;
							color = BlendColorsWithAlpha(pixel.A, otherColor, Color.Black, otherFloatValue);
						}
						bitmap.SetPixel(j, i, color);
					}
				}
			}
		}

		private Color BlendColorsWithAlpha(int alpha, Color color, Color otherColor, float floatValue)
		{
			int red = (int)((float)(int)color.R * floatValue + (float)(int)otherColor.R * (1f - floatValue));
			int green = (int)((float)(int)color.G * floatValue + (float)(int)otherColor.G * (1f - floatValue));
			int blue = (int)((float)(int)color.B * floatValue + (float)(int)otherColor.B * (1f - floatValue));
			return Color.FromArgb(alpha, red, green, blue);
		}
	}
}
