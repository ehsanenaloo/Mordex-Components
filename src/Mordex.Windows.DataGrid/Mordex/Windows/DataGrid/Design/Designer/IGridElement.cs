using System;
using System.ComponentModel;
using System.Drawing;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	internal interface IGridElement
	{
		object SelectedObject { get; set; }

		Image ItemImage { get; set; }

		event EventHandler SelectedObjectChanged;

		event EventHandler RefreshTree;

		event EventHandler DataChanged;

		bool CommitChanges();

		bool Support(ToolbarOptions option);

		void SetSite(ISite site);
	}
}
