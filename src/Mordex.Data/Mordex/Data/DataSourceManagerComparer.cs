using System.Collections;

namespace Mordex.Data
{
	internal class DataSourceManagerComparer : IComparer
	{
		private DataSourceManager _manager;

		internal DataSourceManagerComparer(DataSourceManager manager)
		{
			_manager = manager;
		}

		int IComparer.Compare(object value, object otherValue)
		{
			return DataValueHelper.CompareValues(_manager.CurrentCulture, value, otherValue, null, true, false);
		}
	}
}
