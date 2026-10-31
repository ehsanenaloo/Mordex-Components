using System;
using System.ComponentModel;
using System.Drawing;

namespace Mordex.Windows.UI.Design
{
	internal interface IUIElement
	{
		object SelectedObject { get; set; }

		Image ItemImage { get; set; }

		event EventHandler DisableCancel;

		event EventHandler SelectedObjectChanged;

		event EventHandler RefreshTree;

		event EventHandler DataChanged;

		bool CommitChanges();

		void SetSite(ISite site);
	}
}
