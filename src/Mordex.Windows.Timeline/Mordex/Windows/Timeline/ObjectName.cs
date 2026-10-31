namespace Mordex.Windows.Timeline
{
	public class ObjectName
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
				_objectValue = value;
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

		internal ObjectName(object value, string name)
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
