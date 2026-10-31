using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandBarRowIndexState : IMordexXmlLayoutsSupport
	{
		private MxCommandBar _commandBar;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public CommandBarRowIndexState(MxCommandBar commandBar)
		{
			_commandBar = commandBar;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_ = _commandBar.Location;
			_commandBar.UpdateRowIndex((int)LayoutReader.GetPropertyValue(reader, "RowIndex", typeof(int), -1));
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			info.AddValue("RowIndex", _commandBar.RowIndex);
		}
	}
}
