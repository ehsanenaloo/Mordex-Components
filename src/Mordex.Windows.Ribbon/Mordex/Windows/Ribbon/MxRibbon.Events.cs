using System;
using System.ComponentModel;
using System.Threading;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		[Description("Occurs when the drop-down window of one of the commands is about to be closed.")]
		public event DropDownClosingEventHandler CommandDropDownClosing
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownClosingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownClosingKey, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is closed.")]
		public event CommandEventHandler CommandDropDownClosed
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownClosedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownClosedKey, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is opening.")]
		public event CommandCancelEventHandler CommandDropDownOpening
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownOpeningKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownOpeningKey, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is opened.")]
		public event CommandEventHandler CommandDropDownOpened
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownOpenedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownOpenedKey, value);
			}
		}

		[Description("Occurs when the user clicks on a Command.")]
		public event CommandEventHandler CommandClick
		{
			add
			{
				base.Events.AddHandler(s_commandClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandClickKey, value);
			}
		}

		[Description("Occurs when a Command is added to the QuickAccesssCommands collection. .")]
		public event CommandEventHandler QuickAccessCommandAdded
		{
			add
			{
				base.Events.AddHandler(s_quickAccessCommandAddedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_quickAccessCommandAddedKey, value);
			}
		}

		[Description("Occurs when a Command is removed from the QuickAccesssCommands collection. .")]
		public event CommandEventHandler QuickAccessCommandRemoved
		{
			add
			{
				base.Events.AddHandler(s_quickAccessCommandRemovedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_quickAccessCommandRemovedKey, value);
			}
		}

		[Description("Occurs when a command in the CustomizeMenu is clicked.")]
		public event CustomizeCommandClickEventHandler CustomizeCommandClick
		{
			add
			{
				base.Events.AddHandler(s_customizeCommandClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_customizeCommandClickKey, value);
			}
		}

		[Description("Occurs after a layout is loaded.")]
		public event EventHandler LayoutLoad
		{
			add
			{
				base.Events.AddHandler(s_layoutLoadKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_layoutLoadKey, value);
			}
		}

		[Description("Occurs after the CurrentLayout property has changed.")]
		public event EventHandler CurrentLayoutChanged
		{
			add
			{
				base.Events.AddHandler(s_currentLayoutChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_currentLayoutChangedKey, value);
			}
		}

		[Description("Occurs before the CurrentLayout property has changed")]
		public event CancelEventHandler CurrentLayoutChanging
		{
			add
			{
				base.Events.AddHandler(s_currentLayoutChangingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_currentLayoutChangingKey, value);
			}
		}

		[Description("Event fired when the SelectedTab property has changed.")]
		public event TabEventHandler SelectedTabChanged
		{
			add
			{
				base.Events.AddHandler(s_selectedTabChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_selectedTabChangedKey, value);
			}
		}

		[Description("Event fired when a new RibbonTab is about to be selected")]
		public event TabCancelEventHandler ChangingSelectedTab
		{
			add
			{
				base.Events.AddHandler(s_changingSelectedTabKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_changingSelectedTabKey, value);
			}
		}

		[Description("Event fired when the Minimized property has changed.")]
		public event EventHandler MinimizedChanged
		{
			add
			{
				base.Events.AddHandler(s_minimizedChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_minimizedChangedKey, value);
			}
		}

		[Description("Event fired when the ControlBox menu is about to be opened.")]
		public event CancelEventHandler ControlBoxOpening
		{
			add
			{
				base.Events.AddHandler(s_controlBoxOpeningKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_controlBoxOpeningKey, value);
			}
		}

		[Description("Occurs when the ControlBoxMenu is opened.")]
		public event EventHandler ControlBoxOpened
		{
			add
			{
				base.Events.AddHandler(s_controlBoxOpenedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_controlBoxOpenedKey, value);
			}
		}

		[Description("Occurs when the ControlBox menu is about to be closed.")]
		public event CancelEventHandler ControlBoxClosing
		{
			add
			{
				base.Events.AddHandler(s_controlBoxClosingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_controlBoxClosingKey, value);
			}
		}

		[Description("Occurs when the ControlBox menu is closed.")]
		public event EventHandler ControlBoxClosed
		{
			add
			{
				base.Events.AddHandler(s_controlBoxClosedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_controlBoxClosedKey, value);
			}
		}

		[Description("Occurs when the Backstage menu is about to be closed.")]
		public event CancelEventHandler BackstageClosing
		{
			add
			{
				base.Events.AddHandler(s_backstageClosingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_backstageClosingKey, value);
			}
		}

		[Description("Occurs when the Backstage menu is closed.")]
		public event EventHandler BackstageClosed
		{
			add
			{
				base.Events.AddHandler(s_backstageClosedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_backstageClosedKey, value);
			}
		}

		[Description("Event fired when the Backstage menu is about to be opened.")]
		public event CancelEventHandler BackstageOpening
		{
			add
			{
				base.Events.AddHandler(s_backstageOpeningKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_backstageOpeningKey, value);
			}
		}

		[Description("Occurs when the Backstage is opened.")]
		public event EventHandler BackstageOpened
		{
			add
			{
				base.Events.AddHandler(s_backstageOpenedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_backstageOpenedKey, value);
			}
		}

		public event EventHandler RibbonMerged
		{
			add
			{
				EventHandler eventHandler = _ribbonMergedHandler;
				EventHandler originalHandler;
				do
				{
					originalHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(originalHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _ribbonMergedHandler, combinedHandler, originalHandler);
				}
				while ((object)eventHandler != originalHandler);
			}
			remove
			{
				EventHandler eventHandler = _ribbonMergedHandler;
				EventHandler originalHandler;
				do
				{
					originalHandler = eventHandler;
					EventHandler reducedHandler = (EventHandler)Delegate.Remove(originalHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _ribbonMergedHandler, reducedHandler, originalHandler);
				}
				while ((object)eventHandler != originalHandler);
			}
		}

		public event EventHandler RibbonUnmerged
		{
			add
			{
				EventHandler eventHandler = _ribbonUnmergedHandler;
				EventHandler originalHandler;
				do
				{
					originalHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(originalHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _ribbonUnmergedHandler, combinedHandler, originalHandler);
				}
				while ((object)eventHandler != originalHandler);
			}
			remove
			{
				EventHandler eventHandler = _ribbonUnmergedHandler;
				EventHandler originalHandler;
				do
				{
					originalHandler = eventHandler;
					EventHandler reducedHandler = (EventHandler)Delegate.Remove(originalHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _ribbonUnmergedHandler, reducedHandler, originalHandler);
				}
				while ((object)eventHandler != originalHandler);
			}
		}
	}
}
