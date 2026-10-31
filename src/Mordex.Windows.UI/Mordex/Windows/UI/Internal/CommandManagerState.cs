using System.Collections;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandManagerState : IMordexXmlLayoutsSupport
	{
		private MxCommandManager _commandManager;

		private ArrayList _commandBars;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public CommandManagerState(ArrayList bars, MxCommandManager manager)
		{
			_commandBars = bars;
			_commandManager = manager;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			for (int i = 0; i < reader.Count; i++)
			{
				PropertyValue propertyValue = reader[i];
				MxCommandBar uiCommandBar = _commandManager.CommandBars.FindBarByKey(propertyValue.Name);
				if (uiCommandBar != null)
				{
					CommandBarRowIndexState targetInstance = new CommandBarRowIndexState(uiCommandBar);
					propertyValue.DeserializeObject(targetInstance);
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			foreach (MxCommandBar item in _commandBars)
			{
				CommandBarRowIndexState value = new CommandBarRowIndexState(item);
				info.AddValue(item.Key, value);
			}
		}
	}
}
