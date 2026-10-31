using System.Windows.Forms;

namespace Mordex.Windows.UI.Internal
{
	public interface IFormatStyleOwner
	{
		Control Owner { get; }

		void PropertyChange();
	}
}
