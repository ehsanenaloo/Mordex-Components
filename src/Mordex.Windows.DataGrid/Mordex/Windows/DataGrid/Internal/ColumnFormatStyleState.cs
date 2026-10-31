using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class ColumnFormatStyleState
	{
		internal GridFormatStyle _columnStyle;

		internal string _displayText;

		internal string _toolTipText;

		private Image _columnImage;

		private bool _gridFormatStyleState2Boolean;

		internal int? _imageIndex;

		internal string _imageKey;

		internal EditType? _editType = null;

		internal ButtonStyle? _buttonStyle;

		internal ColumnType? _cellDisplayType;

		internal GridValueListItemCollection _valueList;

		internal Image GetColumnImage()
		{
			return _columnImage;
		}

		internal void SetColumnImage(Image image)
		{
			_columnImage = image;
			_gridFormatStyleState2Boolean = true;
		}

		internal bool HasImage()
		{
			return _gridFormatStyleState2Boolean;
		}
	}
}
