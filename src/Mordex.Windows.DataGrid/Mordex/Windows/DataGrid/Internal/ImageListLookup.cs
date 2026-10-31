using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class ImageListLookup
	{
		internal ImageList _imageList;

		private int _imageIndex;

		private string _imageKey;

		internal Image _currentImage;

		internal ImageListLookup()
		{
			_imageKey = "";
		}

		internal int GetImageIndex()
		{
			if (_imageKey.Length > 0 && _imageList != null)
			{
				return _imageList.Images.IndexOfKey(_imageKey);
			}
			return _imageIndex;
		}

		internal void SetIndex(int value)
		{
			_imageIndex = value;
		}

		internal void SetKey(string text)
		{
			_imageKey = text;
		}
	}
}
