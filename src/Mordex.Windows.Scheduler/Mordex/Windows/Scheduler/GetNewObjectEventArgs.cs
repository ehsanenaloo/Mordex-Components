using System;

namespace Mordex.Windows.Scheduler
{
	public class GetNewObjectEventArgs : EventArgs
	{
		private object _newObject;

		public object NewObject
		{
			get
			{
				return _newObject;
			}
			set
			{
				_newObject = value;
			}
		}

		internal GetNewObjectEventArgs()
		{
		}
	}
}
