using System;
using System.ComponentModel;
using System.Drawing;

namespace Mordex.Windows.Ribbon.Design
{
	internal interface IRibbonItemEditor
	{
		object SelectedObject { get; set; }

		Image ItemImage { get; set; }

		event EventHandler DisableCancel;

		event EventHandler SelectedObjectChanged;

		event EventHandler RefreshTree;

		event EventHandler DataChanged;

		bool CommitChanges();

		bool IsOptionSupported(ToolbarOptions option);

		void SetSite(ISite site);
	}
}
