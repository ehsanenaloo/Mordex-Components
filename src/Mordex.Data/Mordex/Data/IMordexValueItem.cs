using System.Drawing;

namespace Mordex.Data
{
	public interface IMordexValueItem
	{
		object DataRow { get; }

		object Value { get; set; }

		string Text { get; set; }

		Image Image { get; set; }

		int ImageIndex { get; set; }
	}
}
