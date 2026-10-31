namespace Mordex.Windows.UI
{
	public class ON
	{
		private object _wrappedObject;

		private string _name;

		public object Object
		{
			get
			{
				return _wrappedObject;
			}
			set
			{
				if (_wrappedObject != value)
				{
					_wrappedObject = value;
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
			_wrappedObject = value;
			_name = name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
