namespace Mordex.Data
{
	public interface ICollectionOwner
	{
		CollectionChangingAction OnCollectionChanging(MordexCollectionBase collection, CollectionChangeType change, object item);

		void OnCollectionChanged(object collection, CollectionChangeType change, object item);
	}
}
