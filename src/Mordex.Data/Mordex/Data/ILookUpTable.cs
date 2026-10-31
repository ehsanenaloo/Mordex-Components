namespace Mordex.Data
{
	public interface ILookUpTable
	{
		bool ReplaceValues { get; }

		IMordexValueItem GetItemFromValue(object value);

		IMordexValueItem GetItemFromText(string text);
	}
}
