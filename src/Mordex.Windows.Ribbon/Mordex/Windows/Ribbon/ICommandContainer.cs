using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal interface ICommandContainer
	{
		CommandCollection Commands { get; }

		MxRibbon Ribbon { get; }

		MxRibbonContextMenu RibbonContextMenu { get; }

		CommandType CommandType { get; }

		CommandSizeStyle DefaultSizeStyle { get; }

		bool IsEnabled { get; }

		BoxStyle LayoutDirection { get; }

		AccessibleObject AccessibilityObject { get; }

		bool CanBeParentTo(CommandType commandType);

		void CommandClicked(CommandEventArgs eventArgs);

		void CommandAdded(CommandBase command);

		void CommandRemoved(CommandBase command);

		bool CommandDropDownClosing(DropDownClosingEventArgs eventArgs);

		void CommandDropDownClosed(CommandEventArgs eventArgs);

		bool CommandDropDownOpening(CommandCancelEventArgs eventArgs);

		void CommandDropDownOpened(CommandEventArgs eventArgs);

		void CommandDisposed(CommandEventArgs eventArgs);

		void CommandCheckedChanged(CommandEventArgs eventArgs);

		void DetachCommand(CommandBase command);

		void InsertCommand(int index, CommandBase command);
	}
}
