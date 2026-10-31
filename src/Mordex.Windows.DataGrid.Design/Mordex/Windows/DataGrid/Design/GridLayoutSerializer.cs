using System;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[CLSCompliant(false)]
	public class GridLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(GridLayout);

		public GridLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
