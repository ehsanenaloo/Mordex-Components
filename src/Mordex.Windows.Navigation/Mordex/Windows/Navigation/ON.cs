namespace Mordex.Windows.Navigation
{
	public class ON
	{
		private object _value;

		private string _name = "";

		public object Object
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
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
			_value = value;
			_name = name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
