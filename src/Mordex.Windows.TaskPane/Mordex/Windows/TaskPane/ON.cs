namespace Mordex.Windows.TaskPane
{
	public class ON
	{
		private object _object;

		private string _name = "";

		public object Object
		{
			get
			{
				return _object;
			}
			set
			{
				_object = value;
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
			_object = value;
			_name = name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
