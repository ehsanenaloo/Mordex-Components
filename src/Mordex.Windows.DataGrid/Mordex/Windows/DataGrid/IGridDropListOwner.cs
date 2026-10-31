using System.Windows.Forms;

namespace Mordex.Windows.DataGrid
{
	internal interface IGridDropListOwner
	{
		AccessibleObject AccessibleObject { get; }

		void OnDropListHide(bool cancel);

		void OnComputeRows();
	}
}
