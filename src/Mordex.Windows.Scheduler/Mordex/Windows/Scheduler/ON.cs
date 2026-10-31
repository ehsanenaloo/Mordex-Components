namespace Mordex.Windows.Scheduler
{
	public class ON
	{
		private object _objectValue;

		private string _name;

		public object Object
		{
			get
			{
				return _objectValue;
			}
			set
			{
				if (_objectValue != value)
				{
					_objectValue = value;
				}
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		internal ON(object value, string name)
		{
			_objectValue = value;
			_name = name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
