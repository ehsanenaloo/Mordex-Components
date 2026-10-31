using System.Drawing;
using Mordex.Data;

namespace Mordex.Windows.DataGrid
{
	internal class GridValueItem : IMordexValueItem
	{
		private Image _itemImage;

		private int _imageIndex;

		private string _itemText;

		private object _dataRow;

		private object _itemValue;

		Image IMordexValueItem.Image
		{
			get
			{
				return _itemImage;
			}
			set
			{
				_itemImage = value;
			}
		}

		int IMordexValueItem.ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				_imageIndex = value;
			}
		}

		object IMordexValueItem.DataRow => _dataRow;

		object IMordexValueItem.Value
		{
			get
			{
				return _itemValue;
			}
			set
			{
				_itemValue = value;
			}
		}

		string IMordexValueItem.Text
		{
			get
			{
				return _itemText;
			}
			set
			{
				_itemText = value;
			}
		}

		internal GridValueItem(object dataRow)
		{
			_dataRow = dataRow;
		}
	}
}
