using System;
using System.Drawing;

namespace Mordex.Windows.Navigation.Design
{
	internal interface INavigationBarElementEditor
	{
		object SelectedObject { get; set; }

		Image ItemImage { get; set; }

		event EventHandler SelectedObjectChanged;

		bool CommitChanges();

		bool Support(ToolbarOptions option);
	}
}
