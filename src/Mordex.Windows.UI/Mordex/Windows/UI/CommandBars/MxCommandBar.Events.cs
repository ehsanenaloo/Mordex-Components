using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		[Description("Occurs when the mouse pointer hovers over one of the associated commands.")]
		public event CommandEventHandler CommandMouseHover
		{
			add
			{
				base.Events.AddHandler(_commandMouseHoverEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandMouseHoverEvent, value);
			}
		}

		[Description("Occurs when the mouse pointer leaves one of the associated commands.")]
		public event CommandEventHandler CommandMouseLeave
		{
			add
			{
				base.Events.AddHandler(_commandMouseLeaveEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandMouseLeaveEvent, value);
			}
		}

		[Description("Occurs when the DockStyle property is about to change.")]
		public event DockStyleCancelEventHandler ChangingDockStyle
		{
			add
			{
				base.Events.AddHandler(_changingDockStyleEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_changingDockStyleEvent, value);
			}
		}

		[Description("Occurs when the DockStyle property has changed.")]
		public event DockStyleEventHandler DockStyleChanged
		{
			add
			{
				base.Events.AddHandler(_dockStyleChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dockStyleChangedEvent, value);
			}
		}

		[Description("Occurs when the ControlValue property of one of the commands has changed.")]
		public event CommandEventHandler CommandControlValueChanged
		{
			add
			{
				base.Events.AddHandler(_commandControlValueChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandControlValueChangedEvent, value);
			}
		}

		[Description("Occurs when the user clicks on a UICommand.")]
		public event CommandEventHandler CommandClick
		{
			add
			{
				base.Events.AddHandler(_commandClickEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandClickEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is dismissed and disappears.")]
		public event CommandEventHandler CommandCloseUp
		{
			add
			{
				base.Events.AddHandler(_commandCloseUpEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandCloseUpEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is about to be closed.")]
		public event CommandClosingEventHandler CommandClosing
		{
			add
			{
				base.Events.AddHandler(_commandClosingEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandClosingEvent, value);
			}
		}

		[Description("Occurs before one of the commands in the CommandBar displays its list of subcommands.")]
		public event CommandEventHandler CommandPopup
		{
			add
			{
				base.Events.AddHandler(_commandPopupEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandPopupEvent, value);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandAdded(CommandActionEventArgs uiCommandEventArgs)
		{
			if (uiCommandEventArgs.GetCommand() != null && uiCommandEventArgs.GetCommand().CommandType == CommandType.ComboBoxCommand)
			{
				uiCommandEventArgs.GetCommand().GetUIComboBox().BindingContext = BindingContext;
			}
			if (!IsInitializing() && CommandManager != null && !CommandManager._isBatchUpdating && !CommandManager._isLoadingLayout)
			{
				RecalculateLayout(true);
			}
			else
			{
				InvalidateLayout();
			}
		}

		void IAccessibleCommandBarsElement.OnCommandRemoved(CommandActionEventArgs uiCommandEventArgs)
		{
			if (uiCommandEventArgs.GetCommand().GetExistingControl() != null)
			{
				RemoveHostedControl(uiCommandEventArgs.GetCommand());
			}
			if (!IsInitializing() && CommandManager != null && !CommandManager._isBatchUpdating && !CommandManager._isLoadingLayout)
			{
				if (!base.Disposing && !base.IsDisposed && GetParentForm() != null)
				{
					RecalculateLayout(true);
				}
				else
				{
					InvalidateLayout();
				}
			}
			else
			{
				InvalidateLayout();
			}
		}

		void IAccessibleCommandBarsElement.OnCommandHover(CommandEventArgs eventArgs)
		{
			if ((GetChildMenu() != null || _commands.Contains(eventArgs.Command)) && !eventArgs.Command.IsMergedCommand)
			{
				OnCommandMouseHover(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandMouseLeave(CommandEventArgs eventArgs)
		{
			if ((GetChildMenu() != null || _commands.Contains(eventArgs.Command)) && !eventArgs.Command.IsMergedCommand)
			{
				OnCommandMouseLeave(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandControlValueChanged(CommandEventArgs eventArgs)
		{
			if (GetChildMenu() != null || _commands.Contains(eventArgs.Command))
			{
				if (!eventArgs.Command.IsMergedCommand)
				{
					OnCommandControlValueChanged(eventArgs);
				}
				Invalidate();
			}
		}

		void IAccessibleCommandBarsElement.OnCommandClicked(CommandEventArgs eventArgs)
		{
			if (!_isInvokingShortcut && GetChildMenu() == null && !_commands.Contains(eventArgs.Command) && !eventArgs.Command._isClickInvokedInCode)
			{
				return;
			}
			_isInvokingShortcut = false;
			if (!eventArgs.Command.IsParent)
			{
				TryCloseChildMenu(CommandCloseReason.CommandClicked);
				_isMenuActive = false;
				if (eventArgs.Command.Control != null && eventArgs.Command.IsControlEditable())
				{
					if (!eventArgs.Command.IsMergedCommand)
					{
						OnCommandClick(eventArgs);
					}
					return;
				}
				_showInfrequentItems = false;
			}
			if (!eventArgs.Command.IsMergedCommand)
			{
				OnCommandClick(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandPopUp(CommandEventArgs eventArgs)
		{
			if ((Commands.Contains(eventArgs.Command) || GetChildMenu() != null) && !eventArgs.Command.IsMergedCommand)
			{
				OnCommandPopup(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandCloseUp(CommandEventArgs eventArgs)
		{
			if ((Commands.Contains(eventArgs.Command) || GetChildMenu() != null) && !eventArgs.Command.IsMergedCommand)
			{
				OnCommandCloseUp(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandClosing(CommandClosingEventArgs eventArgs)
		{
			if ((Commands.Contains(eventArgs.Command) || GetChildMenu() != null) && !eventArgs.Command.IsMergedCommand)
			{
				OnCommandClosing(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandExpanded(CommandEventArgs eventArgs)
		{
			if (base.Visible && _layoutItems != null)
			{
				ExpandCommand(eventArgs.Command);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandCollapsed(CommandEventArgs eventArgs)
		{
			if (GetChildMenu() != null)
			{
				GetChildMenu().CloseMenuFor(eventArgs.Command);
			}
		}

		protected virtual void OnCommandMouseHover(CommandEventArgs e)
		{
			if (base.Events[_commandMouseHoverEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandMouseLeave(CommandEventArgs e)
		{
			if (base.Events[_commandMouseLeaveEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnChangingDockStyle(DockStyleCancelEventArgs e)
		{
			if (base.Events[_changingDockStyleEvent] is DockStyleCancelEventHandler dockStyleCancelEventHandler)
			{
				dockStyleCancelEventHandler(this, e);
			}
		}

		protected virtual void OnDockStyleChanged(DockStyleEventArgs e)
		{
			if (base.Events[_dockStyleChangedEvent] is DockStyleEventHandler dockStyleEventHandler)
			{
				dockStyleEventHandler(this, e);
			}
		}

		protected virtual void OnCommandControlValueChanged(CommandEventArgs e)
		{
			if (base.Events[_commandControlValueChangedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandClick(CommandEventArgs e)
		{
			if (base.Events[_commandClickEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandCloseUp(CommandEventArgs e)
		{
			if (base.Events[_commandCloseUpEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandClosing(CommandClosingEventArgs e)
		{
			if (base.Events[_commandClosingEvent] is CommandClosingEventHandler commandClosingEventHandler)
			{
				commandClosingEventHandler(this, e);
			}
		}

		protected virtual void OnCommandPopup(CommandEventArgs e)
		{
			if (base.Events[_commandPopupEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}
	}
}
