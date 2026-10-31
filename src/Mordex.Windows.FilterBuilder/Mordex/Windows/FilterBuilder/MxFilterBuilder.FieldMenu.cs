using System;
using System.Collections.Generic;
using System.Linq;
using Mordex.Data;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.FilterBuilder
{
	public partial class MxFilterBuilder
	{
		private const string AddConditionKey = "AddNewCondition";

		private MxContextMenu _addConditionMenu;

		private MxContextMenu _fieldMenu;

		// The menu that lists the fields (and child tables) of the table; it opens from the field buttons.
		internal MxContextMenu FieldMenu
		{
			get
			{
				if (_fieldMenu == null)
				{
					_fieldMenu = new MxContextMenu();
					_fieldMenu.CommandManager = _commandManager;
					_fieldMenu.UseMnemonic = false;
					if (_table != null)
					{
						AddTableCommands(_table, _fieldMenu.Commands);
					}
				}
				return _fieldMenu;
			}
		}

		// The right-click menu that adds the first condition.
		private MxContextMenu AddConditionMenu
		{
			get
			{
				if (_addConditionMenu == null)
				{
					_addConditionMenu = new MxContextMenu();
					_addConditionMenu.CommandManager = _commandManager;
					MxCommand addCommand = new MxCommand(AddConditionKey);
					addCommand.Text = _builtInTextList[BuiltInText.AddCondition];
					_addConditionMenu.Commands.Add(addCommand);
					_addConditionMenu.CommandClick += OnAddConditionMenuCommandClick;
				}
				return _addConditionMenu;
			}
		}

		private void OnAddConditionMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			if (eventArgs.Command.Key == AddConditionKey)
			{
				CreateRootCondition();
			}
		}

		private void ResetFieldMenu(bool reloadConditions)
		{
			if (_fieldMenu != null)
			{
				_fieldMenu.Dispose();
				_fieldMenu = null;
				_commandManager.Commands.Clear();
			}
			if (reloadConditions && HasFilterCondition)
			{
				Reload();
			}
		}

		private void DisposeMenus()
		{
			if (_addConditionMenu != null)
			{
				_addConditionMenu.CommandClick -= OnAddConditionMenuCommandClick;
				_addConditionMenu.Dispose();
				_addConditionMenu = null;
			}
			if (_fieldMenu != null)
			{
				_fieldMenu.Dispose();
				_fieldMenu = null;
			}
		}

		private void AddTableCommands(IMordexTable table, MxCommandCollection target)
		{
			HashSet<string> usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<MxCommand> fieldCommands = new List<MxCommand>();
			foreach (IMordexField field in table.Fields)
			{
				if (FilterFields.EffectiveEditType(field) == FilterEditType.NoEdit)
				{
					continue;
				}
				MxCommand command = new MxCommand();
				command.Text = FilterFields.DisplayName(field);
				command.Key = UniqueKey(FirstNotEmpty(field.DataMember, field.Key, "Field"), usedKeys);
				command.Tag = field;
				command.Image = field.GetDisplayImage();
				fieldCommands.Add(command);
			}
			foreach (MxCommand command in Sorted(fieldCommands))
			{
				target.Add(command);
			}
			if (!_showChildTables || table.ChildTables.Count <= 0)
			{
				return;
			}
			List<MxCommand> childCommands = new List<MxCommand>();
			foreach (IMordexTable childTable in table.ChildTables)
			{
				MxCommand command = new MxCommand();
				command.Text = childTable.TableCaption;
				command.Key = UniqueKey(FirstNotEmpty(childTable.DataMember, childTable.Key, "Table"), usedKeys);
				command.Tag = childTable;
				AddTableCommands(childTable, command.Commands);
				if (command.Commands.Count > 0)
				{
					childCommands.Add(command);
				}
				else
				{
					command.Dispose();
				}
			}
			if (childCommands.Count > 0)
			{
				target.AddSeparator();
				foreach (MxCommand command in Sorted(childCommands))
				{
					target.Add(command);
				}
			}
		}

		private IEnumerable<MxCommand> Sorted(List<MxCommand> commands)
		{
			return _sortFieldList ? commands.OrderBy(command => command.Text, StringComparer.CurrentCulture) : commands;
		}

		private static string FirstNotEmpty(string first, string second, string fallback)
		{
			if (!string.IsNullOrEmpty(first))
			{
				return first;
			}
			return string.IsNullOrEmpty(second) ? fallback : second;
		}

		// Command keys must differ inside one list and from the keys already in the field menu.
		private string UniqueKey(string baseKey, HashSet<string> usedKeys)
		{
			string key = baseKey;
			for (int suffix = 1; usedKeys.Contains(key) || _fieldMenu.Commands.Contains(key); suffix++)
			{
				key = baseKey + suffix;
			}
			usedKeys.Add(key);
			return key;
		}
	}
}