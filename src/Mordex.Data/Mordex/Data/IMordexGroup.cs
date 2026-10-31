namespace Mordex.Data
{
	public interface IMordexGroup
	{
		IMordexField Field { get; set; }

		IMordexTable Table { get; }

		IMordexCustomGroup CustomGroup { get; set; }

		SortOrder SortOrder { get; set; }

		GroupInterval GroupInterval { get; set; }

		FormatMode GroupFormatMode { get; set; }

		string GroupFormatString { get; set; }

		IMordexField[] Fields { get; }

		string MultipleFieldSeparator { get; set; }
	}
}
