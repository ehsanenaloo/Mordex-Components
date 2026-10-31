namespace Mordex.Data
{
	public interface IValueConverter
	{
		string FormatValue(object value);

		object ParseText(string text);
	}
}
