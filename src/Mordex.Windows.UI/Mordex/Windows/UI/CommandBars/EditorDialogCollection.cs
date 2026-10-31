using System.Collections;
using System.Reflection;

namespace Mordex.Windows.UI.CommandBars
{
	[DefaultMember("Item")]
	internal class EditorDialogCollection : CollectionBase
	{
		private MxCommandManager _commandManager;

		internal EditorDialogCollection(MxCommandManager commandManager)
		{
			_commandManager = commandManager;
		}

		public void AddDialog(CommandBarEditorDialog uiCommandBarEditorDialog)
		{
			base.List.Add(uiCommandBarEditorDialog);
		}

		public void RemoveDialog(CommandBarEditorDialog uiCommandBarEditorDialog)
		{
			if (base.List.Contains(uiCommandBarEditorDialog))
			{
				base.List.Remove(uiCommandBarEditorDialog);
			}
		}
	}
}
