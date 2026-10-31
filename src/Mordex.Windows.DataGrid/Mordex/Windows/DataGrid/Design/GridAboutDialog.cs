using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Design
{
	internal class GridAboutDialog : MxAboutDialog
	{
		internal GridAboutDialog()
			: base(typeof(GridAboutDialog).Assembly)
		{
		}

		internal void ShowAbout(MxDataGrid dataGrid)
		{
			ShowDialog();
		}
	}
}
