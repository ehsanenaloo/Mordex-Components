using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandImageSource
	{
		internal ImageList _imageList;

		internal int _imageIndex;

		internal string _imageKey;

		internal Image _image;

		internal Icon _icon;

		internal CommandImageSource()
		{
		}

		internal CommandImageSource(Image img)
		{
			_image = img;
		}

		internal bool IsEmpty()
		{
			if (_icon != null)
			{
				return false;
			}
			if (_image != null)
			{
				return false;
			}
			if (_imageIndex >= 0)
			{
				return false;
			}
			if (_imageKey.Length > 0)
			{
				return false;
			}
			return true;
		}

		internal Size GetImageSize()
		{
			if (_icon != null)
			{
				return _icon.Size;
			}
			if (_image != null)
			{
				return _image.Size;
			}
			if (_imageList != null)
			{
				if (_imageKey.Length > 0 && _imageList.Images.ContainsKey(_imageKey))
				{
					return _imageList.ImageSize;
				}
				if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
				{
					return _imageList.ImageSize;
				}
			}
			return Size.Empty;
		}

		internal void DrawImage(Graphics graphics, Rectangle rect, bool isEnabled)
		{
			if (!isEnabled)
			{
				Image sourceImage = null;
				if (_image != null)
				{
					graphics.DrawImage(_image, rect);
				}
				else if (_imageList != null)
				{
					if (_imageKey.Length > 0 && _imageList.Images.ContainsKey(_imageKey))
					{
						sourceImage = _imageList.Images[_imageKey];
					}
					else if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
					{
						sourceImage = _imageList.Images[_imageIndex];
					}
				}
				if (sourceImage != null)
				{
					Bitmap bitmap = new Bitmap(sourceImage, rect.Width, rect.Height);
					ControlPaint.DrawImageDisabled(graphics, bitmap, rect.X, rect.Y, SystemColors.Control);
					bitmap.Dispose();
					bitmap = null;
				}
			}
			else if (_image != null)
			{
				graphics.DrawImage(_image, rect);
			}
			else
			{
				if (_imageList == null)
				{
					return;
				}
				if (_imageKey.Length > 0 && _imageList.Images.ContainsKey(_imageKey))
				{
					if (rect.Size == _imageList.ImageSize)
					{
						_imageList.Draw(graphics, rect.X, rect.Y, rect.Width, rect.Height, _imageList.Images.IndexOfKey(_imageKey));
						return;
					}
					Image imageByKey = _imageList.Images[_imageKey];
					graphics.DrawImage(imageByKey, rect);
				}
				else if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
				{
					if (_imageList.ImageSize == rect.Size)
					{
						_imageList.Draw(graphics, rect.X, rect.Y, rect.Width, rect.Height, _imageIndex);
						return;
					}
					Image imageByIndex = _imageList.Images[_imageIndex];
					graphics.DrawImage(imageByIndex, rect);
				}
			}
		}
	}
}
