using System.Collections;
using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal interface ILayoutElement
	{
		GridRow GridRow { get; }

		GridTable LayoutTable { get; }

		IList Items { get; }

		Rectangle GetItemRect(Rectangle bounds);
	}
}
