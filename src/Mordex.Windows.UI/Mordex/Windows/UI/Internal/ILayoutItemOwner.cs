using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Internal
{
	internal interface ILayoutItemOwner
	{
		Control ParentControl { get; }

		void DoDefaultAction(FormatStyleLayoutState item);

		void Invalidate(Rectangle rect);

		void AccessibleStateChanged(FormatStyleLayoutState item);
	}
}
