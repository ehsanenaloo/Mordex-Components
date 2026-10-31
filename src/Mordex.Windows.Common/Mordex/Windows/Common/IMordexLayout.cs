using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Common
{
	public interface IMordexLayout : IMordexXmlLayoutsSupport
	{
		string LayoutString { get; set; }

		LayoutReferenceCollection LayoutReferences { get; }

		string DesignTimeName { get; }

		object Owner { get; }

		void Update();
	}
}
