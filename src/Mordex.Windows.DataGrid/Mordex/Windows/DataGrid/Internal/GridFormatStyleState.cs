using System.Collections;
using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GridFormatStyleState
	{
		internal string _cardCaptionText;

		internal string _groupCaption;

		internal GridFormatStyle _rowFormatStyle;

		private Image _groupImage;

		private bool _hasGroupImage;

		private int _groupImageIndex;

		private bool _hasGroupImageIndex;

		internal string _groupImageKey;

		internal string _previewRowText;

		internal bool? _previewRowVisible;

		internal int _rowHeaderImageIndex = -1;

		internal string _rowHeaderImageKey = "";

		internal string _rowHeaderText;

		private Hashtable _hashtable;

		internal Image GetGroupImage()
		{
			return _groupImage;
		}

		internal void SetGroupImage(Image image)
		{
			_groupImage = image;
			_hasGroupImage = true;
		}

		internal bool HasGroupImage()
		{
			return _hasGroupImage;
		}

		internal int GetGroupImageIndex()
		{
			return _groupImageIndex;
		}

		internal void SetGroupImageIndex(int value)
		{
			_groupImageIndex = value;
			_hasGroupImageIndex = true;
		}

		internal bool HasGroupImageIndex()
		{
			return _hasGroupImageIndex;
		}

		internal ColumnFormatStyleState GetColumnStyleState(GridColumn key, bool condition)
		{
			ColumnFormatStyleState hashtableItem = null;
			if (_hashtable != null && _hashtable.ContainsKey(key))
			{
				hashtableItem = (ColumnFormatStyleState)_hashtable[key];
			}
			if (hashtableItem == null && condition)
			{
				hashtableItem = new ColumnFormatStyleState();
				if (_hashtable == null)
				{
					_hashtable = new Hashtable();
				}
				_hashtable.Add(key, hashtableItem);
			}
			return hashtableItem;
		}
	}
}
