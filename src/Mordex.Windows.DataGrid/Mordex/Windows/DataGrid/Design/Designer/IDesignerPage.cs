using System;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	internal interface IDesignerPage
	{
		MxDataGrid DataGrid { get; set; }

		event EventHandler SelectTab;

		event EventHandler DataChanged;

		bool CommitChanges();

		void Unload();
	}
}
