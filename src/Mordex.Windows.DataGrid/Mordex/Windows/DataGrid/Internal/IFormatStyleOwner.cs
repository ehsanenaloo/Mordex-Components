namespace Mordex.Windows.DataGrid.Internal
{
	public interface IFormatStyleOwner
	{
		MxDataGrid DataGrid { get; }

		void InvalidateFormatStyleLayout();
	}
}
