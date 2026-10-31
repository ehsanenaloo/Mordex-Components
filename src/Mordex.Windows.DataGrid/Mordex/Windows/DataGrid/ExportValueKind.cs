namespace Mordex.Windows.DataGrid
{
	/// <summary>Determines how <see cref="Export.GridExporter"/> writes the numeric values of a column to Excel.</summary>
	public enum ExportValueKind
	{
		/// <summary>
		/// Writes a number cell when Excel can hold the value exactly (15 significant digits or fewer), and a text
		/// cell with the exact value otherwise.
		/// </summary>
		Automatic,

		/// <summary>Always writes a number cell. Excel keeps only 15 significant digits.</summary>
		Number,

		/// <summary>Always writes a text cell with the exact value, for identifiers such as long account numbers.</summary>
		Text
	}
}
