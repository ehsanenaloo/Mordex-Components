using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	public interface ITextBoxOwner
	{
		TextBox TextBoxControl { get; }

		bool UseDefaultMenu { get; set; }
	}
}
