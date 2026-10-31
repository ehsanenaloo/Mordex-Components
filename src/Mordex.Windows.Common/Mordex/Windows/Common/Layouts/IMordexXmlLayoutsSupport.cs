namespace Mordex.Windows.Common.Layouts
{
	public interface IMordexXmlLayoutsSupport
	{
		string Identifier { get; }

		bool IsCollection { get; }

		string CollectionElementName { get; }

		void Serialize(LayoutWriter mordexWriter);

		void Deserialize(LayoutReader mordexReader);
	}
}
