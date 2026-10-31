using System.Collections;

namespace Mordex.Windows.DataGrid
{
	internal class SelectedItemPositionComparer : IComparer
	{
		internal SelectedItemPositionComparer()
		{
		}

		int IComparer.Compare(object value, object valueArgument)
		{
			GridSelectedItem gridSelectedItem = (GridSelectedItem)value;
			GridSelectedItem selectedItem = (GridSelectedItem)valueArgument;
			return gridSelectedItem.Position.CompareTo(selectedItem.Position);
		}
	}
}
