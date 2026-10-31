using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandManager
	{
		private void OnMergedManagerMergeChanging(object sender, EventArgs eventArgs)
		{
			_pendingMergeManager = _mergedManager;
			SetMergedManager(null);
			_pendingMergeManager.AddMergeChangedHandler(OnMergedManagerMergeChanged);
		}

		private void OnMergedManagerMergeChanged(object sender, EventArgs eventArgs)
		{
			SetMergedManager(_pendingMergeManager);
			_pendingMergeManager.RemoveMergeChangedHandler(OnMergedManagerMergeChanged);
			_pendingMergeManager = null;
		}

		internal MxCommandManager GetMergedManager()
		{
			return _mergedManager;
		}

		internal void SetMergedManager(MxCommandManager mergedManager)
		{
			if (_mergedManager != mergedManager)
			{
				OnMergeChanging(EventArgs.Empty);
				_isToolbarsMenuDirty = true;
				if (_mergedManager != null)
				{
					_mergedManager.RemoveCommandAddedHandler(OnMergedManagerCommandAdded);
					_mergedManager.RemoveMergeChangingHandler(OnMergedManagerMergeChanging);
					_mergedManager._mergeHost = null;
					UnmergeCommands();
					_mergedManager.TopRebar.LayoutRows();
					_mergedManager.LeftRebar.LayoutRows();
					_mergedManager.BottomRebar.LayoutRows();
					_mergedManager.RightRebar.LayoutRows();
				}
				_mergedManager = mergedManager;
				if (_mergedManager != null)
				{
					MergeCommandsFrom(mergedManager);
					_mergedManager.TopRebar.LayoutRows();
					_mergedManager.LeftRebar.LayoutRows();
					_mergedManager.BottomRebar.LayoutRows();
					_mergedManager.RightRebar.LayoutRows();
					_mergedManager.AddCommandAddedHandler(OnMergedManagerCommandAdded);
					_mergedManager.AddMergeChangingHandler(OnMergedManagerMergeChanging);
					_mergedManager._mergeHost = this;
				}
				HideEmptyRebars();
				OnMergeChanged(EventArgs.Empty);
			}
		}

		private void AddMergeChangingHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_mergeChangingEvent, eventHandler);
		}

		private void RemoveMergeChangingHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(_mergeChangingEvent, eventHandler);
		}

		private void OnMergeChanging(EventArgs eventArgs)
		{
			if (base.Events[_mergeChangingEvent] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void AddMergeChangedHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_mergeChangedEvent, eventHandler);
		}

		private void RemoveMergeChangedHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(_mergeChangedEvent, eventHandler);
		}

		private void OnMergeChanged(EventArgs eventArgs)
		{
			if (base.Events[_mergeChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnMergedManagerCommandAdded(object value, CommandActionEventArgs uiCommandEventArgs)
		{
			MxCommand uiCommand = null;
			uiCommand = ((uiCommandEventArgs.GetParentCommand() == null) ? uiCommandEventArgs.GetCommand() : uiCommandEventArgs.GetParentCommand());
			if (uiCommand.BaseCommand != null)
			{
				uiCommand = uiCommand.BaseCommand;
			}
			Commands.UpdateMergedCommands(uiCommand);
		}

		public void MergeCommandManager(MxCommandManager mergedManager)
		{
			_isExplicitMerge = true;
			try
			{
				SetMergedManager(mergedManager);
			}
			finally
			{
				_isExplicitMerge = false;
			}
		}

		internal void MergeCommandsFrom(MxCommandManager mergedManager)
		{
			_isBatchUpdating = true;
			if (_mergedManager == null)
			{
				_mergedManager = mergedManager;
			}
			_mergedManager._isBatchUpdating = true;
			try
			{
				_mergedManager.Commands.ReleaseCommandsFromContainer();
				foreach (MxCommand command in mergedManager.Commands)
				{
					command.UpdateMergeState(_id, true);
				}
				_mergedSmallImageList = mergedManager.ImageList;
				_mergedLargeImageList = mergedManager.LargeImageList;
				foreach (MxCommand ownCommand in Commands)
				{
					if (ownCommand.Commands._mergedCommands == null)
					{
						continue;
					}
					MxCommand mergedCommand = mergedManager.Commands.FindByKey(ownCommand.Key);
					if (mergedCommand != null)
					{
						if (mergedCommand.MergeType == CommandMergeType.MergeItems)
						{
							ownCommand.Commands.MergeCommands(mergedCommand.Commands, mergedManager);
						}
					}
					else
					{
						ownCommand.Commands.MergeCommands(null, mergedManager);
						ownCommand.Commands._isMerging = false;
					}
				}
				CommandBars.MergeBars(mergedManager);
			}
			finally
			{
				_mergedManager._isBatchUpdating = false;
				_isBatchUpdating = false;
			}
			TopRebar.ReapplyBarOffsets();
			LeftRebar.ReapplyBarOffsets();
			RightRebar.ReapplyBarOffsets();
			BottomRebar.ReapplyBarOffsets();
		}

		public void UnmergeCommandManager()
		{
			_isExplicitMerge = true;
			SetMergedManager(null);
			_isExplicitMerge = false;
		}

		internal void UnmergeCommands()
		{
			_isBatchUpdating = true;
			foreach (MxCommand command in Commands)
			{
				if (command.Commands._mergedCommands == null)
				{
					continue;
				}
				try
				{
					MxCommandCollection uiCommandCollection = command.Commands.GetUnmergedCommands();
					if (uiCommandCollection != null)
					{
						command.UpdateCommands(uiCommandCollection);
					}
				}
				catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
				{
					// One command that cannot get its children back must not stop the others.
				}
			}
			_mergedSmallImageList = null;
			_mergedLargeImageList = null;
			CommandBars.UnmergeBars(_mergedManager, _isExplicitMerge);
			_mergedManager.Commands.AttachDisposeHandlers();
			_mergedManager.Commands.UpdateMergeStates(Id);
			foreach (MxCommandBar commandBar in _mergedManager.CommandBars)
			{
				commandBar.Commands.UpdateMergeStates(Id);
			}
			RefreshRebars();
			_isBatchUpdating = false;
		}
	}
}
