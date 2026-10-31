namespace Mordex.Data
{
	internal interface IChangeableCollectionOwner : ICollectionOwner
	{
		bool CanChange();
	}
}
