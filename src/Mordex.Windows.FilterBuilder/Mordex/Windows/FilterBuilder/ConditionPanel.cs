using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Controls;
using ConditionOperator = Mordex.Data.ConditionOperator;

namespace Mordex.Windows.FilterBuilder
{
	// One condition: field, operator, value editors and buttons, plus one nested panel per child condition.
	[ToolboxItem(false)]
	internal partial class ConditionPanel : TableLayoutPanel, IFilterBuilderElement
	{
		private const int FieldColumn = 1;

		private const int OperatorColumn = 2;

		private const int FirstValueColumn = 3;

		private const int SecondValueColumn = 4;

		private const int AddColumn = 5;

		private const int RemoveColumn = 6;

		private const int ConditionColumnCount = 8;

		private const int SpacerColumnWidth = 3;

		private const int DefaultColumnWidth = 100;

		private const int FieldButtonInitialWidth = 91;

		private const int OperatorButtonInitialWidth = 94;

		private const int IconButtonWidth = 32;

		private const int ButtonHeightInset = 9;

		private const int ButtonTextPadding = 32;

		private const int LogicalButtonTextPadding = 45;

		private const int ChildRowGap = 3;

		private const int MinimumSizePadding = 6;

		private sealed class ChildRow
		{
			internal ChildRow(ConditionPanel panel, TransparentTableLayoutPanel host, MxButton logicalButton)
			{
				Panel = panel;
				Host = host;
				LogicalButton = logicalButton;
			}

			internal ConditionPanel Panel { get; }

			internal TransparentTableLayoutPanel Host { get; }

			internal MxButton LogicalButton { get; }
		}

		private readonly IFilterBuilderElement _owner;

		private readonly BuiltInTextList _texts;

		private readonly List<ChildRow> _childRows = new List<ChildRow>();

		private MxButton _fieldButton;

		private MxButton _operatorButton;

		private MxButton _addButton;

		private MxButton _removeButton;

		private MxContextMenu _logicalOperatorMenu;

		private MxContextMenu _operatorMenu;

		private MxContextMenu _rowMenu;

		private MxContextMenu _subscribedFieldMenu;

		private ConditionValueEditor _firstEditor;

		private ConditionValueEditor _secondEditor;

		private IMordexField _field;

		private ConditionOperator _conditionOperator;

		private Size _minimumSize = Size.Empty;

		int IFilterBuilderElement.RowHeight => _owner.RowHeight;

		BuiltInTextList IFilterBuilderElement.BuiltInTextList => _owner.BuiltInTextList;

		MxFilterBuilder IFilterBuilderElement.FilterBuilder => _owner.FilterBuilder;

		IMordexCondition IFilterBuilderElement.FilterCondition => Condition;

		public override Size MinimumSize
		{
			get
			{
				return _minimumSize;
			}
			set
			{
				if (_minimumSize != value)
				{
					_minimumSize = value;
					MinimumSizeUpdated?.Invoke(this, EventArgs.Empty);
					base.MinimumSize = value;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal IMordexCondition Condition { get; private set; }

		// Set by the filter builder when built-in texts changed; the texts are refreshed on the next paint.
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal bool RefreshTextsOnPaint { get; set; }

		internal event EventHandler MinimumSizeUpdated;

		internal ConditionPanel(IFilterBuilderElement owner, IMordexCondition condition)
		{
			SuspendLayout();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
			_owner = owner;
			_texts = owner.BuiltInTextList;
			base.Height = CurrentRowHeight;
			if (owner is Control ownerControl)
			{
				Font = ownerControl.Font;
			}
			if (condition != null)
			{
				CreateConditionControls();
				LoadCondition(condition);
				MxContextMenu fieldMenu = _owner.FilterBuilder.FieldMenu;
				_fieldButton.DropDownContextMenu = fieldMenu;
				_subscribedFieldMenu = fieldMenu;
				fieldMenu.CommandClick += OnFieldMenuCommandClick;
			}
			else
			{
				CreateEmptyConditionButton();
			}
			ResumeLayout();
		}

		private int CurrentRowHeight => _owner.RowHeight;

		private int ButtonHeight => CurrentRowHeight - ButtonHeightInset;

		private MxButton CreateButton()
		{
			MxButton button = new MxButton();
			button.ShowFocusRectangle = false;
			return button;
		}

		private void CreateEmptyConditionButton()
		{
			_addButton = CreateButton();
			_addButton.Margin = new Padding(7, 5, 3, 3);
			_addButton.TextHorizontalAlignment = TextAlignment.Near;
			_addButton.Image = ResourceFile.AddCriterium;
			_addButton.Text = _texts[BuiltInText.EmptyConditionText];
			_addButton.Click += OnAddButtonClick;
			int width = TextRenderer.MeasureText(_addButton.Text, Font).Width + ButtonTextPadding;
			_addButton.Size = new Size(width, ButtonHeight);
			base.Controls.Add(_addButton);
		}

		private void CreateConditionControls()
		{
			_fieldButton = CreateButton();
			_fieldButton.Width = FieldButtonInitialWidth;
			_fieldButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			_fieldButton.Margin = new Padding(3, 5, 3, 3);
			_fieldButton.ButtonStyle = ButtonStyle.DropDown;
			_fieldButton.TextHorizontalAlignment = TextAlignment.Near;
			_fieldButton.UseMnemonic = false;
			_operatorButton = CreateButton();
			_operatorButton.Width = OperatorButtonInitialWidth;
			_operatorButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			_operatorButton.Margin = new Padding(3, 5, 3, 3);
			_operatorButton.ButtonStyle = ButtonStyle.DropDown;
			_operatorButton.ImageHorizontalAlignment = ImageHorizontalAlignment.Center;
			_addButton = CreateButton();
			_addButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			_addButton.Margin = new Padding(3, 5, 3, 3);
			_addButton.Width = IconButtonWidth;
			_addButton.Image = ResourceFile.AddCriterium;
			_addButton.ToolTipText = _texts[BuiltInText.AddConditionTooltip];
			_addButton.Click += OnAddButtonClick;
			_removeButton = CreateButton();
			_removeButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			_removeButton.Margin = new Padding(3, 5, 3, 3);
			_removeButton.Width = IconButtonWidth;
			_removeButton.Image = ResourceFile.RemoveCriterium;
			_removeButton.ToolTipText = _texts[BuiltInText.RemoveConditionTooltip];
			_removeButton.Click += OnRemoveButtonClick;
			base.ColumnCount = ConditionColumnCount;
			base.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SpacerColumnWidth));
			for (int i = FieldColumn; i <= RemoveColumn; i++)
			{
				base.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DefaultColumnWidth));
			}
			base.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			base.RowCount = 1;
			base.RowStyles.Add(new RowStyle(SizeType.Absolute, CurrentRowHeight));
			base.Controls.Add(_fieldButton, FieldColumn, 0);
			base.Controls.Add(_operatorButton, OperatorColumn, 0);
			base.Controls.Add(_addButton, AddColumn, 0);
			base.Controls.Add(_removeButton, RemoveColumn, 0);
		}

		private void LoadCondition(IMordexCondition condition)
		{
			Condition = condition;
			_conditionOperator = condition.ConditionOperator;
			_field = condition.Field;
			CorrectOperator();
			RebuildOperatorMenu();
			ConfigureEditors(reuseEditors: false);
			foreach (IMordexCondition child in condition.Conditions)
			{
				AddChildRow(child);
			}
			RefreshControls();
		}

		void IFilterBuilderElement.OnChildConditionAdded(IMordexCondition condition)
		{
			_owner.InvalidateConditions();
		}

		void IFilterBuilderElement.OnChildConditionRemoved(ConditionPanel panel)
		{
			int index = _childRows.FindIndex(row => row.Panel == panel);
			if (index >= 0)
			{
				ChildRow removed = _childRows[index];
				SuspendLayout();
				_childRows.RemoveAt(index);
				removed.Host.Dispose();
				base.RowCount--;
				base.RowStyles.RemoveAt(index + 1);
				for (int i = index; i < _childRows.Count; i++)
				{
					SetRow(_childRows[i].Host, i + 1);
				}
				ResumeLayout(performLayout: false);
			}
			_owner.InvalidateConditions();
		}

		void IFilterBuilderElement.OnConditionChanged(IMordexCondition condition)
		{
			_owner.OnConditionChanged(condition);
		}

		void IFilterBuilderElement.InvalidateConditions()
		{
			_owner.InvalidateConditions();
		}

		private void SetField(IMordexField field)
		{
			if (_field == field)
			{
				return;
			}
			if (_field != null)
			{
				Condition.Value1 = null;
				Condition.Value2 = null;
				Condition.ValueField1 = null;
				Condition.ValueField2 = null;
			}
			_field = field;
			Condition.Field = field;
			CorrectOperator();
			RebuildOperatorMenu();
			ConfigureEditors(reuseEditors: false);
			RefreshControls();
			NotifyConditionChanged();
		}

		private void SetConditionOperator(ConditionOperator conditionOperator)
		{
			if (_conditionOperator == conditionOperator)
			{
				return;
			}
			if (IsListOperator(_conditionOperator) || IsListOperator(conditionOperator))
			{
				ReplaceEditor(ref _firstEditor, null, FirstValueColumn);
			}
			_conditionOperator = conditionOperator;
			Condition.ConditionOperator = conditionOperator;
			ConfigureEditors(reuseEditors: true);
			RefreshControls();
			NotifyConditionChanged();
		}

		private static bool IsListOperator(ConditionOperator conditionOperator)
		{
			return conditionOperator == ConditionOperator.In || conditionOperator == ConditionOperator.NotIn;
		}

		private static bool IsRangeOperator(ConditionOperator conditionOperator)
		{
			return conditionOperator == ConditionOperator.Between || conditionOperator == ConditionOperator.NotBetween;
		}

		private static bool IsValuelessOperator(ConditionOperator conditionOperator)
		{
			switch (conditionOperator)
			{
			case ConditionOperator.IsNull:
			case ConditionOperator.NotIsNull:
			case ConditionOperator.IsEmpty:
			case ConditionOperator.NotIsEmpty:
				return true;
			default:
				return false;
			}
		}

		// A condition is active when it has a field and everything its operator needs.
		private bool IsConditionComplete()
		{
			if (_field == null)
			{
				return false;
			}
			if (_firstEditor == null)
			{
				return IsValuelessOperator(_conditionOperator);
			}
			object value = _firstEditor.Value;
			if (value != null)
			{
				return !(value is string text && text.Length == 0);
			}
			return _firstEditor.ValueField != null;
		}

		private void ReplaceEditor(ref ConditionValueEditor slot, ConditionValueEditor editor, int column)
		{
			if (slot == editor)
			{
				return;
			}
			if (slot != null)
			{
				slot.ValueChanged -= OnEditorValueChanged;
				slot.Dispose();
			}
			slot = editor;
			if (slot != null)
			{
				slot.Dock = DockStyle.Fill;
				slot.ValueChanged += OnEditorValueChanged;
				base.ColumnStyles[column].Width = _owner.FilterBuilder.EditControlWidth;
				base.Controls.Add(slot, column, 0);
			}
			else
			{
				base.ColumnStyles[column].Width = 0f;
			}
		}

		// Creates the value editors that the field and operator need. With reuseEditors the existing ones are kept.
		private void ConfigureEditors(bool reuseEditors)
		{
			if (_field == null)
			{
				ReplaceEditor(ref _firstEditor, null, FirstValueColumn);
				ReplaceEditor(ref _secondEditor, null, SecondValueColumn);
				return;
			}
			TypeCode typeCode = FilterFields.EditorTypeCode(_field);
			MxFilterBuilder filterBuilder = _owner.FilterBuilder;
			if (IsValuelessOperator(_conditionOperator))
			{
				ReplaceEditor(ref _firstEditor, null, FirstValueColumn);
			}
			else if (!reuseEditors || _firstEditor == null)
			{
				ReplaceEditor(ref _firstEditor, new ConditionValueEditor(filterBuilder, Condition, typeCode, EditFilterValue.Value1), FirstValueColumn);
			}
			if (IsRangeOperator(_conditionOperator))
			{
				if (!reuseEditors || _secondEditor == null)
				{
					ReplaceEditor(ref _secondEditor, new ConditionValueEditor(filterBuilder, Condition, typeCode, EditFilterValue.Value2), SecondValueColumn);
				}
			}
			else
			{
				ReplaceEditor(ref _secondEditor, null, SecondValueColumn);
			}
		}

		private void OnEditorValueChanged(object sender, EventArgs eventArgs)
		{
			NotifyConditionChanged();
		}

		private void NotifyConditionChanged()
		{
			Condition.Enabled = IsConditionComplete();
			_owner.OnConditionChanged(Condition);
			Invalidate();
		}

		private void OnAddButtonClick(object sender, EventArgs eventArgs)
		{
			AddChildCondition();
		}

		private void OnRemoveButtonClick(object sender, EventArgs eventArgs)
		{
			RemoveThisCondition();
		}

		private void AddChildCondition()
		{
			if (Condition == null)
			{
				_owner.FilterBuilder.CreateRootCondition();
				return;
			}
			IMordexCondition child = Condition.AddCondition();
			child.ConditionOperator = _owner.FilterBuilder.DefaultConditionOperator;
			AddChildRow(child);
			_owner.InvalidateConditions();
		}

		private void RemoveThisCondition()
		{
			if (_owner.FilterCondition != Condition)
			{
				_owner.FilterCondition.RemoveCondition(Condition);
			}
			_owner.OnChildConditionRemoved(this);
		}

		private void AddChildRow(IMordexCondition condition)
		{
			base.RowCount++;
			base.RowStyles.Add(new RowStyle(SizeType.Absolute, CurrentRowHeight));
			string logicalText = FilterBuilderTexts.LogicalOperatorText(condition.LogicalOperator, condition.Negation, _texts);
			TransparentTableLayoutPanel host = new TransparentTableLayoutPanel();
			host.Margin = new Padding(0);
			host.Dock = DockStyle.Fill;
			host.MouseDown += OnHostMouseDown;
			host.MouseUp += OnHostMouseUp;
			host.ColumnCount = 2;
			host.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, TextRenderer.MeasureText(logicalText, Font).Width + LogicalButtonTextPadding));
			host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			SetColumnSpan(host, base.ColumnCount);
			MxButton logicalButton = new MxButton();
			logicalButton.Text = logicalText;
			logicalButton.TextHorizontalAlignment = TextAlignment.Near;
			logicalButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			logicalButton.ButtonStyle = ButtonStyle.DropDown;
			logicalButton.Height = ButtonHeight;
			logicalButton.Margin = new Padding(20, 5, 0, 0);
			logicalButton.DropDownContextMenu = LogicalOperatorMenu;
			logicalButton.Tag = condition;
			logicalButton.ShowFocusRectangle = false;
			ConditionPanel panel = new ConditionPanel(this, condition);
			panel.Margin = new Padding(3, 0, 3, 0);
			panel.Dock = DockStyle.Top;
			panel.MinimumSizeUpdated += OnChildMinimumSizeChanged;
			_childRows.Add(new ChildRow(panel, host, logicalButton));
			host.Controls.Add(logicalButton, 0, 0);
			host.Controls.Add(panel, 1, 0);
			RefreshLayoutRecursive();
			base.Controls.Add(host, 0, base.RowCount - 1);
			base.RowStyles[base.RowCount - 1].Height = panel.Height + ChildRowGap;
		}

		private void OnHostMouseDown(object sender, MouseEventArgs eventArgs)
		{
			OnMouseDown(eventArgs);
		}

		private void OnHostMouseUp(object sender, MouseEventArgs eventArgs)
		{
			OnMouseUp(eventArgs);
		}

		internal IMordexCondition FindFocusedCondition()
		{
			if (!ContainsFocus)
			{
				return null;
			}
			foreach (ChildRow row in _childRows)
			{
				if (row.Panel.ContainsFocus)
				{
					return row.Panel.FindFocusedCondition() ?? row.Panel.Condition;
				}
			}
			return Condition;
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (eventArgs.Button == MouseButtons.Right)
			{
				RowMenu.Show(this);
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			Focus();
		}

		protected override void OnParentChanged(EventArgs eventArgs)
		{
			base.OnParentChanged(eventArgs);
			if (base.Parent != null && !(base.Parent is ConditionPanel))
			{
				UpdateMinimumSize();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_addButton != null)
				{
					_addButton.Click -= OnAddButtonClick;
				}
				if (_removeButton != null)
				{
					_removeButton.Click -= OnRemoveButtonClick;
				}
				if (_subscribedFieldMenu != null)
				{
					_subscribedFieldMenu.CommandClick -= OnFieldMenuCommandClick;
					_subscribedFieldMenu = null;
				}
				if (_firstEditor != null)
				{
					_firstEditor.ValueChanged -= OnEditorValueChanged;
				}
				if (_secondEditor != null)
				{
					_secondEditor.ValueChanged -= OnEditorValueChanged;
				}
				DisposeMenus();
			}
			base.Dispose(disposing);
		}
	}
}