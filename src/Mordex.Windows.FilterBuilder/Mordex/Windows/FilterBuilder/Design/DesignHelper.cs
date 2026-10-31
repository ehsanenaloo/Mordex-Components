using System.Windows.Forms;

namespace Mordex.Windows.FilterBuilder.Design
{
	public static class DesignHelper
	{
		public static DialogResult CustomizeBuiltInTexts(MxFilterBuilder filterBuilder)
		{
			using (BuiltInTextEditor editor = new BuiltInTextEditor(filterBuilder.BuiltInTextList))
			{
				return editor.ShowDialog();
			}
		}
	}
}
