using System.ComponentModel;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	internal interface IAccessibleCommandBarsElement
	{
		bool IsInitializing { get; }

		ISite OwnerSite { get; }

		AccessibleObject AccessibilityObject { get; }

		MxCommandManager GetCommandManager();

		ImageList GetImageList(bool smallIcons, MxCommand cmd);

		void OnInvalidateCommand();

		void OnCommandMouseLeave(CommandEventArgs eventArgs);

		void OnCommandCollapsed(CommandEventArgs eventArgs);

		void OnCommandExpanded(CommandEventArgs eventArgs);

		void OnCommandClicked(CommandEventArgs eventArgs);

		void OnCommandPopUp(CommandEventArgs eventArgs);

		void OnCommandAdded(CommandActionEventArgs uiCommandEventArgs);

		void OnCommandRemoved(CommandActionEventArgs uiCommandEventArgs);

		void OnCommandCloseUp(CommandEventArgs eventArgs);

		void OnCommandHover(CommandEventArgs eventArgs);

		void OnCommandControlValueChanged(CommandEventArgs eventArgs);

		void OnCommandClosing(CommandClosingEventArgs eventArgs);
	}
}
