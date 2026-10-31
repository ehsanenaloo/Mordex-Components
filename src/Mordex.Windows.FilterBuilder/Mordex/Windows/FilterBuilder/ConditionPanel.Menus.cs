using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Controls;
using ConditionOperator = Mordex.Data.ConditionOperator;
using LogicalOperator = Mordex.Data.LogicalOperator;

namespace Mordex.Windows.FilterBuilder
{
	internal partial class ConditionPanel
	{
		private const string AddConditionKey = "AddCondition";

		private const string DeleteKey = "Delete";

		private const string NegatedOperatorKeySuffix = "NotOperator";

		private static readonly (AvailableLogicalOperators Flag, string Key, LogicalOperator Operator, bool Negated)[] LogicalMenuItems =
		{
			(AvailableLogicalOperators.And, "AndOperator", LogicalOperator.And, false),
			(AvailableLogicalOperators.AndNot, "AndNotOperator", LogicalOperator.And, true),
			(AvailableLogicalOperators.Or, "OrOperator", LogicalOperator.Or, false),
			(AvailableLogicalOperators.OrNot, "OrNotOperator", LogicalOperator.Or, true),
			(AvailableLogicalOperators.Xor, "XorOperator", LogicalOperator.Xor, false),
			(AvailableLogicalOperators.XorNot, "XorNotOperator", LogicalOperator.Xor, true)
		};

		private static readonly (AvailableConditionOperators Flag, ConditionOperator Operator)[] OperatorFlags =
		{
			(AvailableConditionOperators.Equal, ConditionOperator.Equal),
			(AvailableConditionOperators.NotEqual, ConditionOperator.NotEqual),
			(AvailableConditionOperators.GreaterThan, ConditionOperator.GreaterThan),
			(AvailableConditionOperators.LessThan, ConditionOperator.LessThan),
			(AvailableConditionOperators.GreaterThanOrEqualTo, ConditionOperator.GreaterThanOrEqualTo),
			(AvailableConditionOperators.LessThanOrEqualTo, ConditionOperator.LessThanOrEqualTo),
			(AvailableConditionOperators.Between, ConditionOperator.Between),
			(AvailableConditionOperators.NotBetween, ConditionOperator.NotBetween),
			(AvailableConditionOperators.Contains, ConditionOperator.Contains),
			(AvailableConditionOperators.NotContains, ConditionOperator.NotContains),
			(AvailableConditionOperators.BeginsWith, ConditionOperator.BeginsWith),
			(AvailableConditionOperators.EndsWith, ConditionOperator.EndsWith),
			(AvailableConditionOperators.IsNull, ConditionOperator.IsNull),
			(AvailableConditionOperators.NotIsNull, ConditionOperator.NotIsNull),
			(AvailableConditionOperators.IsEmpty, ConditionOperator.IsEmpty),
			(AvailableConditionOperators.NotIsEmpty, ConditionOperator.NotIsEmpty),
			(AvailableConditionOperators.AnyOf, ConditionOperator.In),
			(AvailableConditionOperators.NoneOf, ConditionOperator.NotIn)
		};

		// The menu of the logical operator buttons of the child rows.
		private MxContextMenu LogicalOperatorMenu
		{
			get
			{
				if (_logicalOperatorMenu == null)
				{
					_logicalOperatorMenu = CreateMenu();
					AvailableLogicalOperators available = _owner.FilterBuilder.AvailableLogicalOperators;
					foreach (var item in LogicalMenuItems)
					{
						if ((available & item.Flag) != 0)
						{
							MxCommand command = new MxCommand(item.Key, FilterBuilderTexts.LogicalOperatorText(item.Operator, item.Negated, _texts));
							command.Tag = item.Operator;
							_logicalOperatorMenu.Commands.Add(command);
						}
					}
					_logicalOperatorMenu.CommandClick += OnLogicalOperatorMenuClick;
				}
				return _logicalOperatorMenu;
			}
		}

		// The right-click menu of the panel.
		private MxContextMenu RowMenu
		{
			get
			{
				if (_rowMenu == null)
				{
					_rowMenu = CreateMenu();
					MxCommand addCommand = new MxCommand(AddConditionKey);
					addCommand.Text = _texts[BuiltInText.AddCondition];
					_rowMenu.Commands.Add(addCommand);
					if (Condition != null)
					{
						MxCommand deleteCommand = new MxCommand(DeleteKey);
						deleteCommand.Text = _texts[BuiltInText.Delete];
						_rowMenu.Commands.Add(deleteCommand);
					}
					_rowMenu.CommandClick += OnRowMenuClick;
				}
				return _rowMenu;
			}
		}

		private MxContextMenu CreateMenu()
		{
			MxContextMenu menu = new MxContextMenu();
			menu.CommandManager = _owner.FilterBuilder.CommandManager;
			return menu;
		}

		private void RebuildOperatorMenu()
		{
			if (_operatorMenu != null)
			{
				_operatorMenu.CommandClick -= OnOperatorMenuClick;
				_operatorMenu.Dispose();
			}
			_operatorMenu = CreateMenu();
			foreach (ConditionOperator conditionOperator in GetAvailableOperators())
			{
				MxCommand command = new MxCommand(conditionOperator.ToString(), FilterBuilderTexts.ConditionOperatorText(conditionOperator, _texts));
				command.Tag = conditionOperator;
				_operatorMenu.Commands.Add(command);
			}
			_operatorMenu.CommandClick += OnOperatorMenuClick;
			if (_operatorButton != null)
			{
				_operatorButton.DropDownContextMenu = _operatorMenu;
			}
		}

		private void RebuildLogicalOperatorMenu()
		{
			if (_logicalOperatorMenu != null)
			{
				_logicalOperatorMenu.CommandClick -= OnLogicalOperatorMenuClick;
				_logicalOperatorMenu.Dispose();
				_logicalOperatorMenu = null;
			}
			if (_childRows.Count > 0)
			{
				MxContextMenu menu = LogicalOperatorMenu;
				foreach (ChildRow row in _childRows)
				{
					row.LogicalButton.DropDownContextMenu = menu;
				}
			}
		}

		private void RebuildRowMenu()
		{
			if (_rowMenu != null)
			{
				_rowMenu.CommandClick -= OnRowMenuClick;
				_rowMenu.Dispose();
				_rowMenu = null;
			}
		}

		private List<ConditionOperator> GetAvailableOperators()
		{
			TypeCode typeCode = _field != null ? FilterFields.EditorTypeCode(_field) : TypeCode.Object;
			AvailableConditionOperators available = _owner.FilterBuilder.AvailableConditionOperators;
			List<ConditionOperator> operators = new List<ConditionOperator>();
			if (available == AvailableConditionOperators.Default && typeCode != TypeCode.Object && typeCode != TypeCode.String && typeCode != TypeCode.Char)
			{
				if (typeCode != TypeCode.Boolean)
				{
					operators.Add(ConditionOperator.Between);
					operators.Add(ConditionOperator.GreaterThan);
					operators.Add(ConditionOperator.GreaterThanOrEqualTo);
					operators.Add(ConditionOperator.LessThan);
					operators.Add(ConditionOperator.LessThanOrEqualTo);
				}
				operators.Add(ConditionOperator.Equal);
				operators.Add(ConditionOperator.NotEqual);
				operators.Add(ConditionOperator.IsEmpty);
				operators.Add(ConditionOperator.NotIsEmpty);
				operators.Add(ConditionOperator.IsNull);
				operators.Add(ConditionOperator.NotIsNull);
				operators.Add(ConditionOperator.In);
				operators.Add(ConditionOperator.NotIn);
			}
			else if (available == AvailableConditionOperators.Default || available == AvailableConditionOperators.All)
			{
				foreach (ConditionOperator conditionOperator in Enum.GetValues(typeof(ConditionOperator)))
				{
					operators.Add(conditionOperator);
				}
			}
			else
			{
				foreach (var item in OperatorFlags)
				{
					if ((available & item.Flag) != AvailableConditionOperators.Default)
					{
						operators.Add(item.Operator);
					}
				}
				if (Condition != null && !operators.Contains(Condition.ConditionOperator))
				{
					operators.Add(Condition.ConditionOperator);
				}
			}
			return operators;
		}

		// The current operator, or Equal when the field type does not allow it.
		private ConditionOperator ValidOperator()
		{
			return GetAvailableOperators().Contains(_conditionOperator) ? _conditionOperator : ConditionOperator.Equal;
		}

		private void CorrectOperator()
		{
			ConditionOperator valid = ValidOperator();
			if (valid != _conditionOperator)
			{
				_conditionOperator = valid;
				Condition.ConditionOperator = valid;
			}
		}

		internal void RefreshOperatorMenus()
		{
			if (Condition != null)
			{
				RebuildOperatorMenu();
				ConditionOperator valid = ValidOperator();
				if (valid != _conditionOperator)
				{
					SetConditionOperator(valid);
				}
			}
			foreach (ChildRow row in _childRows)
			{
				row.Panel.RefreshOperatorMenus();
			}
		}

		internal void RefreshLogicalOperatorMenus()
		{
			RebuildLogicalOperatorMenu();
			foreach (ChildRow row in _childRows)
			{
				row.Panel.RefreshLogicalOperatorMenus();
			}
		}

		// Rebuilds everything that shows built-in texts.
		internal void RefreshTexts()
		{
			RefreshTextsRecursive();
			RefreshLayoutRecursive();
		}

		private void RefreshTextsRecursive()
		{
			RebuildRowMenu();
			if (Condition != null)
			{
				RebuildLogicalOperatorMenu();
				RebuildOperatorMenu();
			}
			else if (_addButton != null)
			{
				_addButton.Text = _texts[BuiltInText.EmptyConditionText];
				_addButton.Size = new Size(TextRenderer.MeasureText(_addButton.Text, Font).Width + ButtonTextPadding, ButtonHeight);
			}
			foreach (ChildRow row in _childRows)
			{
				row.Panel.RefreshTextsRecursive();
			}
		}

		private void DisposeMenus()
		{
			if (_logicalOperatorMenu != null)
			{
				_logicalOperatorMenu.CommandClick -= OnLogicalOperatorMenuClick;
				_logicalOperatorMenu.Dispose();
				_logicalOperatorMenu = null;
			}
			if (_operatorMenu != null)
			{
				_operatorMenu.CommandClick -= OnOperatorMenuClick;
				_operatorMenu.Dispose();
				_operatorMenu = null;
			}
			RebuildRowMenu();
		}

		private void OnFieldMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			MxContextMenu menu = (MxContextMenu)sender;
			if (menu.SourceControl == _fieldButton && eventArgs.Command.Tag is IMordexField field)
			{
				SetField(field);
			}
		}

		private void OnOperatorMenuClick(object sender, CommandEventArgs eventArgs)
		{
			SetConditionOperator((ConditionOperator)eventArgs.Command.Tag);
		}

		private void OnRowMenuClick(object sender, CommandEventArgs eventArgs)
		{
			switch (eventArgs.Command.Key)
			{
			case AddConditionKey:
				AddChildCondition();
				break;
			case DeleteKey:
				RemoveThisCondition();
				break;
			}
		}

		private void OnLogicalOperatorMenuClick(object sender, CommandEventArgs eventArgs)
		{
			if (!(sender is MxContextMenu menu) || !(menu.SourceControl is MxButton button) || !(button.Tag is IMordexCondition child))
			{
				return;
			}
			LogicalOperator logicalOperator = (LogicalOperator)eventArgs.Command.Tag;
			bool negated = eventArgs.Command.Key.EndsWith(NegatedOperatorKeySuffix, StringComparison.Ordinal);
			if (logicalOperator == child.LogicalOperator && negated == child.Negation)
			{
				return;
			}
			child.LogicalOperator = logicalOperator;
			child.Negation = negated;
			RefreshLayoutRecursive();
			if (child.Field != null)
			{
				NotifyConditionChanged();
			}
		}
	}
}