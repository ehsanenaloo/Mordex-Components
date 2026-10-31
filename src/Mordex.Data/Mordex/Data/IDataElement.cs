namespace Mordex.Data
{
	internal interface IDataElement
	{
		void OnRowDelete();

		void OnRowChange();
	}
}
