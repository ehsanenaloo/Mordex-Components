using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.EditControls
{
	internal class SchedulerFilterConditionEditor : UserControl
	{
		private Label _lblColumn;

		private ComboBox _jcboColumn;

		private ComboBox _jcboOperator;

		private TextBox _firstValueTextBox;

		private Label _lblAnd;

		private TextBox _secondValueTextBox;

		private ComboBox _firstValueFieldComboBox;

		private ComboBox _secondValueFieldComboBox;

		private ScrollBarStyleControl _firstValueModeButton;

		private ScrollBarStyleControl _secondValueModeButton;

		private ScrollBarStyleControl _moptNot;

		private Container _container = new Container();

		private MxScheduler _scheduler;

		private SchedulerFilterCondition _schedulerFilterCondition;

		private Size _contentSize;

		private bool _shouldRaiseFilterChanged = true;

		private EventHandler _eventHandler;

		private bool _isConditionRowVisible = true;

		private bool _isSelected;

		private EventHandler _schedulerEditorControl6EventHandler;

		private EventHandler _activeControlChangedHandler;

		internal SchedulerFilterConditionEditor()
		{
			InitializeComponent();
			_firstValueModeButton.SetValues(new object[2]
			{
				"Value",
				"Field"
			});
			_moptNot.SetValues(new object[2]
			{
				"",
				"Not"
			});
			_secondValueModeButton.SetValues(new object[2]
			{
				"Value",
				"Field"
			});
			FillOperatorList();
			LoadCondition();
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			Font font = new Font(Font.Name, Font.Size - 1f);
			_firstValueModeButton.Font = font;
			_secondValueModeButton.Font = font;
			_moptNot.Font = font;
			foreach (Control control in base.Controls)
			{
				control.GotFocus += OnControlGotFocus;
			}
			base.GotFocus += OnControlGotFocus;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			Focus();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			if (_firstValueTextBox.Visible)
			{
				Rectangle bounds = _firstValueTextBox.Bounds;
				bounds.Inflate(3, 2);
				if (!_firstValueTextBox.Enabled)
				{
					eventArgs.Graphics.FillRectangle(SystemBrushes.Control, bounds);
				}
				SchedulerRenderer.DrawBorder(eventArgs.Graphics, SystemPens.ControlDark, bounds);
			}
			if (_secondValueTextBox.Visible)
			{
				Rectangle bounds = _secondValueTextBox.Bounds;
				bounds.Inflate(3, 2);
				if (!_secondValueTextBox.Enabled)
				{
					eventArgs.Graphics.FillRectangle(SystemBrushes.Control, bounds);
				}
				SchedulerRenderer.DrawBorder(eventArgs.Graphics, SystemPens.ControlDark, bounds);
			}
			RenderContent(eventArgs.Graphics, _moptNot.Bounds.Right + 8, base.ClientRectangle.Top + 1, _contentSize.Height - 4, SystemPens.ControlDark);
			if (_isSelected)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Size = _contentSize;
				Pen highlight = SystemPens.Highlight;
				SchedulerRenderer.DrawBorder(eventArgs.Graphics, highlight, clientRectangle);
			}
		}

		public void UpdateSchedule(MxScheduler scheduler)
		{
			if (_scheduler != scheduler)
			{
				_scheduler = scheduler;
				RefreshFieldLists();
			}
		}

		private void RefreshFieldLists()
		{
			_jcboColumn.DataSource = null;
			_firstValueFieldComboBox.DataSource = null;
			_secondValueFieldComboBox.DataSource = null;
			if (_scheduler != null)
			{
				FillFieldComboBox(_jcboColumn, _scheduler.Fields);
				FillFieldComboBox(_firstValueFieldComboBox, _scheduler.Fields);
				FillFieldComboBox(_secondValueFieldComboBox, _scheduler.Fields);
			}
			LoadCondition();
		}

		private void UpdateLayout()
		{
			bool canEdit = _scheduler != null && _schedulerFilterCondition != null;
			SetControlsEnabled(base.Controls, canEdit);
			Invalidate();
		}

		private void SetControlsEnabled(ControlCollection controlCollection, bool enabled)
		{
			foreach (Control item in controlCollection)
			{
				item.Enabled = enabled;
				SetControlsEnabled(item.Controls, enabled);
			}
		}

		private void FillFieldComboBox(ComboBox comboBox, SchedulerFieldCollection schedulerFieldCollection)
		{
			comboBox.SelectedValueChanged -= OnJcboColumnValue2SelectedValueChanged;
			if (comboBox != null && schedulerFieldCollection != null)
			{
				ArrayList arrayList = new ArrayList();
				arrayList.Add(new ON(null, "(none)"));
				foreach (SchedulerField item in schedulerFieldCollection)
				{
					arrayList.Add(new ON(item, SchedulerRenderer.GetDisplayText(_scheduler, item)));
				}
				comboBox.DataSource = arrayList;
				comboBox.ValueMember = "Object";
				comboBox.DisplayMember = "Name";
			}
			comboBox.SelectedValueChanged += OnJcboColumnValue2SelectedValueChanged;
		}

		private void FillOperatorList()
		{
			_jcboOperator.SelectedValueChanged -= OnJcboOperatorSelectedValueChanged;
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new ON(ConditionOperator.Equal, "="));
			arrayList.Add(new ON(ConditionOperator.NotEqual, "<>"));
			arrayList.Add(new ON(ConditionOperator.GreaterThan, ">"));
			arrayList.Add(new ON(ConditionOperator.LessThan, "<"));
			arrayList.Add(new ON(ConditionOperator.GreaterThanOrEqualTo, ">="));
			arrayList.Add(new ON(ConditionOperator.LessThanOrEqualTo, "<="));
			arrayList.Add(new ON(ConditionOperator.Between, "Between"));
			arrayList.Add(new ON(ConditionOperator.NotBetween, "Not Between"));
			arrayList.Add(new ON(ConditionOperator.Contains, "Contains"));
			arrayList.Add(new ON(ConditionOperator.NotContains, "Doesn't contain"));
			arrayList.Add(new ON(ConditionOperator.BeginsWith, "Begins with"));
			arrayList.Add(new ON(ConditionOperator.EndsWith, "Ends With"));
			arrayList.Add(new ON(ConditionOperator.IsNull, "Is Null"));
			arrayList.Add(new ON(ConditionOperator.NotIsNull, "Isn't Null"));
			arrayList.Add(new ON(ConditionOperator.IsEmpty, "Is empty"));
			arrayList.Add(new ON(ConditionOperator.NotIsEmpty, "Isn't empty"));
			arrayList.Add(new ON(ConditionOperator.In, "Is In"));
			arrayList.Add(new ON(ConditionOperator.NotIn, "Is Not In"));
			_jcboOperator.DataSource = arrayList;
			_jcboOperator.ValueMember = "Object";
			_jcboOperator.DisplayMember = "Name";
			_jcboOperator.SelectedValueChanged += OnJcboOperatorSelectedValueChanged;
		}

		public SchedulerFilterCondition GetSchedulerFilterCondition()
		{
			return _schedulerFilterCondition;
		}

		public void SetSchedulerFilterCondition(SchedulerFilterCondition schedulerFilterCondition)
		{
			_schedulerFilterCondition = schedulerFilterCondition;
			LoadCondition();
			if (schedulerFilterCondition != null)
			{
				if (schedulerFilterCondition._owner == null)
				{
					SetSelected(true);
				}
			}
			else
			{
				_isSelected = false;
			}
			OnFilterChanged(EventArgs.Empty);
		}

		private void LoadCondition()
		{
			_shouldRaiseFilterChanged = false;
			if (_schedulerFilterCondition != null)
			{
				if (_schedulerFilterCondition.Field == null)
				{
					if (_jcboColumn.Items.Count > 0)
					{
						_jcboColumn.SelectedIndex = 0;
					}
					else
					{
						_jcboColumn.SelectedIndex = -1;
					}
				}
				else
				{
					_jcboColumn.SelectedValue = _schedulerFilterCondition.Field;
				}
				_jcboOperator.SelectedValue = _schedulerFilterCondition.ConditionOperator;
				if (_schedulerFilterCondition.ValueField1 == null)
				{
					if (_firstValueFieldComboBox.Items.Count > 0)
					{
						_firstValueFieldComboBox.SelectedIndex = 0;
					}
					else
					{
						_firstValueFieldComboBox.SelectedIndex = -1;
					}
				}
				else
				{
					_firstValueFieldComboBox.SelectedValue = _schedulerFilterCondition.ValueField1;
				}
				if (_schedulerFilterCondition.ValueField2 == null)
				{
					if (_secondValueFieldComboBox.Items.Count > 0)
					{
						_secondValueFieldComboBox.SelectedIndex = 0;
					}
					else
					{
						_secondValueFieldComboBox.SelectedIndex = -1;
					}
				}
				else
				{
					_secondValueFieldComboBox.SelectedValue = _schedulerFilterCondition.ValueField2;
				}
				_firstValueTextBox.Text = string.Concat(_schedulerFilterCondition.Value1);
				_secondValueTextBox.Text = string.Concat(_schedulerFilterCondition.Value2);
				if (_schedulerFilterCondition.Negation)
				{
					_moptNot.SetSelectedIndex(1);
				}
				else
				{
					_moptNot.SetSelectedIndex(0);
				}
			}
			else
			{
				try
				{
					_firstValueModeButton.SetSelectedIndex(0);
					if (_jcboColumn.Items.Count > 0)
					{
						_jcboColumn.SelectedIndex = 0;
					}
					else
					{
						_jcboColumn.SelectedIndex = -1;
					}
					if (_firstValueFieldComboBox.Items.Count > 0)
					{
						_firstValueFieldComboBox.SelectedIndex = 0;
					}
					else
					{
						_firstValueFieldComboBox.SelectedIndex = -1;
					}
					if (_secondValueFieldComboBox.Items.Count > 0)
					{
						_secondValueFieldComboBox.SelectedIndex = 0;
					}
					else
					{
						_secondValueFieldComboBox.SelectedIndex = -1;
					}
					_firstValueTextBox.Text = "";
					_secondValueTextBox.Text = "";
					_jcboOperator.SelectedIndex = -1;
					_moptNot.SetSelectedIndex(0);
				}
				catch (Exception ex)
				{
					Mordex.Windows.Common.MxMessageBox.Show(ex.ToString());
				}
			}
			RebuildChildEditors();
			ResizeToContent();
			UpdateLayout();
			_shouldRaiseFilterChanged = true;
			RaiseFilterChangedIfEnabled();
		}

		internal SchedulerFilterConditionEditor FindEditor(SchedulerFilterCondition schedulerFilterCondition)
		{
			if (schedulerFilterCondition == _schedulerFilterCondition)
			{
				return this;
			}
			foreach (Control control in base.Controls)
			{
				if (control is SchedulerFilterConditionEditor)
				{
					SchedulerFilterConditionEditor childEditor = ((SchedulerFilterConditionEditor)control).FindEditor(schedulerFilterCondition);
					if (childEditor != null)
					{
						return childEditor;
					}
				}
			}
			return null;
		}

		private void RebuildChildEditors()
		{
			RemoveChildEditors();
			if (_schedulerFilterCondition != null && _schedulerFilterCondition.IsComposite)
			{
				SetConditionRowVisible(!_schedulerFilterCondition.IsEmptyCondition());
				int top = 2;
				if (_isConditionRowVisible)
				{
					top += 24;
				}
				{
					foreach (SchedulerFilterCondition item in (IEnumerable)_schedulerFilterCondition.Conditions)
					{
						if (!_schedulerFilterCondition.IsEmptyCondition() || _schedulerFilterCondition.Conditions.IndexOf(item) > 0)
						{
							top += 2;
							ScrollBarStyleControl scrollBarStyleControl = CreateLogicalOperatorButton(item);
							scrollBarStyleControl.Location = new Point(_jcboColumn.Left + 8, top);
							base.Controls.Add(scrollBarStyleControl);
							top += 22;
						}
						SchedulerFilterConditionEditor childEditor = new SchedulerFilterConditionEditor();
						childEditor.BackColor = BackColor;
						childEditor.UpdateSchedule(_scheduler);
						childEditor.SetSchedulerFilterCondition(item);
						childEditor.AddFilterChangedHandler(OnChildFilterChanged);
						childEditor.AddSelectedFilterConditionChangedHandler(ForwardSelectedFilterConditionChanged);
						childEditor.AddActiveControlChangedHandler(ForwardActiveControlChanged);
						childEditor.Location = new Point(_lblColumn.Left, top);
						base.Controls.Add(childEditor);
						top += childEditor.Height;
					}
					return;
				}
			}
			SetConditionRowVisible(true);
		}

		private void ForwardSelectedFilterConditionChanged(object sender, EventArgs eventArgs)
		{
			OnSelectedFilterConditionChanged(eventArgs);
		}

		private void OnControlGotFocus(object sender, EventArgs eventArgs)
		{
			OnActiveControlChanged(eventArgs);
		}

		public Control GetEffectiveActiveControl()
		{
			if (base.ActiveControl == null)
			{
				SchedulerFilterConditionEditor focusedEditor = (SchedulerFilterConditionEditor)FindFocusedEditor();
				if (focusedEditor == null)
				{
					return this;
				}
				return focusedEditor.ActiveControl;
			}
			return base.ActiveControl;
		}

		private void RemoveChildEditors()
		{
			ArrayList arrayList = new ArrayList();
			foreach (Control childControl in base.Controls)
			{
				if (childControl is SchedulerFilterConditionEditor)
				{
					arrayList.Add(childControl);
				}
				if (childControl is ScrollBarStyleControl && ((ScrollBarStyleControl)childControl).Tag != null)
				{
					arrayList.Add(childControl);
				}
			}
			foreach (Control item in arrayList)
			{
				if (item is SchedulerFilterConditionEditor)
				{
					SchedulerFilterConditionEditor childEditor = (SchedulerFilterConditionEditor)item;
					childEditor.RemoveFilterChangedHandler(OnChildFilterChanged);
					base.Controls.Remove(childEditor);
				}
				else if (item is ScrollBarStyleControl)
				{
					ScrollBarStyleControl scrollBarStyleControl = (ScrollBarStyleControl)item;
					scrollBarStyleControl.RemoveSelectedIndexChangedHandler(OnLogicalOperatorChanged);
					base.Controls.Remove(scrollBarStyleControl);
				}
			}
		}

		private void InitializeComponent()
		{
			_lblColumn = new Label();
			_jcboColumn = new ComboBox();
			_jcboOperator = new ComboBox();
			_firstValueFieldComboBox = new ComboBox();
			_secondValueFieldComboBox = new ComboBox();
			_firstValueTextBox = new TextBox();
			_lblAnd = new Label();
			_secondValueTextBox = new TextBox();
			_firstValueModeButton = new ScrollBarStyleControl();
			_secondValueModeButton = new ScrollBarStyleControl();
			_moptNot = new ScrollBarStyleControl();
			SuspendLayout();
			_lblColumn.AutoSize = true;
			_lblColumn.Enabled = false;
			_lblColumn.Location = new Point(48, 5);
			_lblColumn.Name = "lblColumn";
			_lblColumn.Size = new Size(46, 14);
			_lblColumn.TabIndex = 0;
			_lblColumn.Text = "Column:";
			_lblColumn.MouseDown += OnLblAndMouseDown;
			_jcboColumn.BackColor = SystemColors.Window;
			_jcboColumn.DropDownStyle = ComboBoxStyle.DropDownList;
			_jcboColumn.DisplayMember = "Name";
			_jcboColumn.Enabled = false;
			_jcboColumn.ForeColor = SystemColors.WindowText;
			_jcboColumn.Location = new Point(96, 2);
			_jcboColumn.Name = "jcboColumn";
			_jcboColumn.Size = new Size(120, 20);
			_jcboColumn.TabIndex = 1;
			_jcboColumn.SelectedValue = null;
			_jcboColumn.ValueMember = "Object";
			_jcboColumn.SelectedValueChanged += OnJcboColumnValue2SelectedValueChanged;
			_jcboOperator.BackColor = SystemColors.Window;
			_jcboOperator.DropDownStyle = ComboBoxStyle.DropDownList;
			_jcboOperator.DisplayMember = "Name";
			_jcboOperator.Enabled = false;
			_jcboOperator.ForeColor = SystemColors.WindowText;
			_jcboOperator.Location = new Point(218, 2);
			_jcboOperator.Name = "jcboOperator";
			_jcboOperator.Size = new Size(104, 20);
			_jcboOperator.TabIndex = 2;
			_jcboOperator.SelectedValue = null;
			_jcboOperator.ValueMember = "Object";
			_jcboOperator.SelectedValueChanged += OnJcboOperatorSelectedValueChanged;
			_firstValueFieldComboBox.BackColor = SystemColors.Window;
			_firstValueFieldComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_firstValueFieldComboBox.DisplayMember = "Name";
			_firstValueFieldComboBox.Enabled = false;
			_firstValueFieldComboBox.ForeColor = SystemColors.WindowText;
			_firstValueFieldComboBox.Location = new Point(376, 2);
			_firstValueFieldComboBox.Name = "jcboColumnValue1";
			_firstValueFieldComboBox.Size = new Size(120, 20);
			_firstValueFieldComboBox.TabIndex = 16;
			_firstValueFieldComboBox.SelectedValue = null;
			_firstValueFieldComboBox.ValueMember = "Object";
			_firstValueFieldComboBox.Visible = false;
			_firstValueFieldComboBox.SelectedValueChanged += OnJcboColumnValue2SelectedValueChanged;
			_secondValueFieldComboBox.BackColor = SystemColors.Window;
			_secondValueFieldComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_secondValueFieldComboBox.DisplayMember = "Name";
			_secondValueFieldComboBox.Enabled = false;
			_secondValueFieldComboBox.ForeColor = SystemColors.WindowText;
			_secondValueFieldComboBox.Location = new Point(570, 2);
			_secondValueFieldComboBox.Name = "jcboColumnValue2";
			_secondValueFieldComboBox.Size = new Size(120, 20);
			_secondValueFieldComboBox.TabIndex = 17;
			_secondValueFieldComboBox.SelectedValue = null;
			_secondValueFieldComboBox.ValueMember = "Object";
			_secondValueFieldComboBox.Visible = false;
			_secondValueFieldComboBox.SelectedValueChanged += OnJcboColumnValue2SelectedValueChanged;
			_firstValueTextBox.AutoSize = false;
			_firstValueTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_firstValueTextBox.Enabled = false;
			_firstValueTextBox.Location = new Point(416, 2);
			_firstValueTextBox.Name = "txtValue1";
			_firstValueTextBox.Size = new Size(108, 20);
			_firstValueTextBox.TabIndex = 5;
			_firstValueTextBox.Text = "";
			_firstValueTextBox.TextChanged += OnTxtValue2TextChanged;
			_lblAnd.AutoSize = true;
			_lblAnd.Enabled = false;
			_lblAnd.Location = new Point(496, 5);
			_lblAnd.Name = "lblAnd";
			_lblAnd.Size = new Size(24, 14);
			_lblAnd.TabIndex = 11;
			_lblAnd.Text = "And";
			_lblAnd.Visible = false;
			_lblAnd.MouseDown += OnLblAndMouseDown;
			_secondValueTextBox.AutoSize = false;
			_secondValueTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_secondValueTextBox.Enabled = false;
			_secondValueTextBox.Location = new Point(636, 2);
			_secondValueTextBox.Name = "txtValue2";
			_secondValueTextBox.Size = new Size(112, 20);
			_secondValueTextBox.TabIndex = 14;
			_secondValueTextBox.Text = "";
			_secondValueTextBox.Visible = false;
			_secondValueTextBox.TextChanged += OnTxtValue2TextChanged;
			_firstValueModeButton.GetScrollBarStyle().ButtonAppearance = Appearance.Flat;
			_firstValueModeButton.Enabled = false;
			_firstValueModeButton.Font = new Font("Tahoma", 6.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_firstValueModeButton.Location = new Point(324, 2);
			_firstValueModeButton.Name = "moptValue1";
			_firstValueModeButton.SetSelectedIndex(-1);
			_firstValueModeButton.SelectValue(null);
			_firstValueModeButton.Size = new Size(50, 20);
			_firstValueModeButton.TabIndex = 25;
			_firstValueModeButton.Text = "multiOptionButton1";
			_firstValueModeButton.SetChecked(false);
			_firstValueModeButton.SetValues(null);
			_firstValueModeButton.AddSelectedIndexChangedHandler(OnFirstValueModeChanged);
			_secondValueModeButton.GetScrollBarStyle().ButtonAppearance = Appearance.Flat;
			_secondValueModeButton.Enabled = false;
			_secondValueModeButton.Font = new Font("Tahoma", 6.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_secondValueModeButton.Location = new Point(520, 2);
			_secondValueModeButton.Name = "moptValue2";
			_secondValueModeButton.SetSelectedIndex(-1);
			_secondValueModeButton.SelectValue(null);
			_secondValueModeButton.Size = new Size(50, 20);
			_secondValueModeButton.TabIndex = 26;
			_secondValueModeButton.Text = "multiOptionButton1";
			_secondValueModeButton.SetChecked(false);
			_secondValueModeButton.SetValues(null);
			_secondValueModeButton.Visible = false;
			_secondValueModeButton.AddSelectedIndexChangedHandler(OnSecondValueModeChanged);
			_moptNot.GetScrollBarStyle().ButtonAppearance = Appearance.Flat;
			_moptNot.Location = new Point(4, 2);
			_moptNot.Name = "moptNot";
			_moptNot.SetSelectedIndex(-1);
			_moptNot.SelectValue(null);
			_moptNot.Size = new Size(34, 20);
			_moptNot.TabIndex = 27;
			_moptNot.Text = "multiOptionButton1";
			_moptNot.SetChecked(false);
			_moptNot.SetValues(null);
			_moptNot.AddSelectedIndexChangedHandler(OnNegationChanged);
			BackColor = SystemColors.Window;
			base.Controls.AddRange(new Control[11]
			{
				_moptNot, _secondValueModeButton, _firstValueModeButton, _lblAnd, _jcboOperator, _jcboColumn, _lblColumn, _firstValueTextBox, _firstValueFieldComboBox, _secondValueTextBox,
				_secondValueFieldComboBox
			});
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "SimpleFilter";
			base.Size = new Size(760, 24);
			ResumeLayout(performLayout: false);
		}

		private void OnJcboOperatorSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_schedulerFilterCondition != null)
			{
				_schedulerFilterCondition.ConditionOperator = (ConditionOperator)_jcboOperator.SelectedValue;
			}
			bool isBetween = false;
			if (_jcboOperator.SelectedValue != null && ((ConditionOperator)_jcboOperator.SelectedValue == ConditionOperator.Between || (ConditionOperator)_jcboOperator.SelectedValue == ConditionOperator.NotBetween))
			{
				isBetween = true;
			}
			_lblAnd.Visible = isBetween;
			_secondValueModeButton.Visible = isBetween;
			_secondValueTextBox.Visible = isBetween && _secondValueModeButton.GetSelectedIndex() == 0;
			_secondValueFieldComboBox.Visible = isBetween && _secondValueModeButton.GetSelectedIndex() == 1;
			ResizeToContent();
			RaiseFilterChangedIfEnabled();
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			ResizeToContent();
		}

		protected override void OnParentChanged(EventArgs eventArgs)
		{
			base.OnParentChanged(eventArgs);
			ResizeToContent();
		}

		private void ResizeToContent()
		{
			if (_schedulerFilterCondition != null)
			{
				_ = _schedulerFilterCondition._owner;
			}
			bool isSimpleCondition = _schedulerFilterCondition != null && !_schedulerFilterCondition.IsComposite;
			int rowTop = 0;
			if (_isConditionRowVisible)
			{
				rowTop = 2;
				_lblColumn.Top = rowTop + (20 - _lblColumn.Height) / 2;
				_lblAnd.Top = _lblColumn.Top;
				_jcboColumn.Top = rowTop;
				_jcboOperator.Top = rowTop;
				_firstValueModeButton.Top = rowTop;
				_firstValueFieldComboBox.Top = rowTop;
				_secondValueFieldComboBox.Top = rowTop;
				_secondValueModeButton.Top = rowTop;
			}
			Rectangle bounds = _firstValueFieldComboBox.Bounds;
			bounds.Inflate(-3, -2);
			_firstValueTextBox.Bounds = bounds;
			bounds = _secondValueFieldComboBox.Bounds;
			bounds.Inflate(-3, -2);
			_secondValueTextBox.Bounds = bounds;
			int contentWidth = 0;
			ConditionOperator conditionOperator = ConditionOperator.Equal;
			if (_jcboOperator.SelectedValue != null)
			{
				conditionOperator = (ConditionOperator)_jcboOperator.SelectedValue;
			}
			contentWidth = ((_jcboOperator.SelectedValue == null || (conditionOperator != ConditionOperator.Between && conditionOperator != ConditionOperator.NotBetween)) ? (_firstValueFieldComboBox.Bounds.Right + 2) : (_secondValueFieldComboBox.Bounds.Right + 2));
			int height = 24;
			_contentSize = new Size(contentWidth, height);
			int bottomPadding = 2;
			if (!isSimpleCondition)
			{
				int maxChildRight = 0;
				int maxChildBottom = 0;
				foreach (Control control in base.Controls)
				{
					if (control is SchedulerFilterConditionEditor)
					{
						if (control.Bounds.Right > maxChildRight)
						{
							maxChildRight = control.Bounds.Right;
						}
						if (control.Bounds.Bottom > maxChildBottom)
						{
							maxChildBottom = control.Bounds.Bottom;
						}
					}
				}
				if (_contentSize.Width < maxChildRight + 4)
				{
					_contentSize.Width = maxChildRight + 4;
				}
				if (_contentSize.Height < maxChildBottom + 4)
				{
					_contentSize.Height = maxChildBottom + 2;
				}
				if (!(base.Parent is SchedulerFilterConditionEditor))
				{
					bottomPadding = 4;
					_contentSize.Height += 2;
				}
			}
			base.Size = _contentSize;
			rowTop = 2;
			if (_schedulerFilterCondition != null && _schedulerFilterCondition.IsComposite)
			{
				_moptNot.Top = rowTop + (_contentSize.Height - _moptNot.Height - rowTop - bottomPadding) / 2;
			}
			else
			{
				_moptNot.Top = rowTop;
			}
			Invalidate();
		}

		private void RaiseFilterChangedIfEnabled()
		{
			if (_shouldRaiseFilterChanged)
			{
				OnFilterChanged(EventArgs.Empty);
			}
		}

		public void AddFilterChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _eventHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(previousHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _eventHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		public void RemoveFilterChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _eventHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(previousHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _eventHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		protected virtual void OnFilterChanged(EventArgs eventArgs)
		{
			if (_eventHandler != null)
			{
				_eventHandler(this, eventArgs);
			}
		}

		private void OnJcboColumnValue2SelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_schedulerFilterCondition != null)
			{
				if (sender == _jcboColumn)
				{
					_schedulerFilterCondition.Field = (SchedulerField)_jcboColumn.SelectedValue;
				}
				else if (sender == _firstValueFieldComboBox)
				{
					_schedulerFilterCondition.ValueField1 = (SchedulerField)_firstValueFieldComboBox.SelectedValue;
				}
				else if (sender == _secondValueFieldComboBox)
				{
					_schedulerFilterCondition.ValueField2 = (SchedulerField)_secondValueFieldComboBox.SelectedValue;
				}
			}
			RaiseFilterChangedIfEnabled();
		}

		private void OnTxtValue2TextChanged(object sender, EventArgs eventArgs)
		{
			if (_schedulerFilterCondition != null)
			{
				string text = ((TextBox)sender).Text;
				object conditionValue = null;
				if (text != "")
				{
					conditionValue = ((!(text == string.Concat('"', '"'))) ? text : "");
				}
				if (sender == _firstValueTextBox)
				{
					_schedulerFilterCondition.Value1 = conditionValue;
				}
				else
				{
					_schedulerFilterCondition.Value2 = conditionValue;
				}
			}
			RaiseFilterChangedIfEnabled();
		}

		private void RenderContent(Graphics graphics, int x2, int y1, int height, Pen pen)
		{
			graphics.DrawLine(pen, x2 + 3, y1, x2, y1 + 3);
			graphics.DrawLine(pen, x2, y1 + 3, x2, y1 + 3 + (height - 12) / 2);
			graphics.DrawLine(pen, x2, y1 + (height - 12) / 2 + 3, x2 - 3, y1 + (height - 12) / 2 + 6);
			graphics.DrawLine(pen, x2 - 3, y1 + (height - 12) / 2 + 6, x2, y1 + (height - 12) / 2 + 9);
			graphics.DrawLine(pen, x2, y1 + (height - 12) / 2 + 9, x2, y1 + height - 3);
			graphics.DrawLine(pen, x2, y1 + height - 3, x2 + 3, y1 + height);
		}

		internal void SetConditionRowVisible(bool schedulerEditorControl6Boolean)
		{
			if (_isConditionRowVisible != schedulerEditorControl6Boolean)
			{
				_isConditionRowVisible = schedulerEditorControl6Boolean;
				_lblColumn.Visible = _isConditionRowVisible;
				_jcboColumn.Visible = _isConditionRowVisible;
				_jcboOperator.Visible = _isConditionRowVisible;
				_firstValueTextBox.Visible = _isConditionRowVisible && _firstValueModeButton.GetSelectedIndex() == 0;
				_firstValueFieldComboBox.Visible = _isConditionRowVisible && _firstValueModeButton.GetSelectedIndex() == 1;
				_firstValueModeButton.Visible = _isConditionRowVisible;
				ConditionOperator conditionOperator = ConditionOperator.Equal;
				if (_jcboOperator.SelectedValue != null)
				{
					conditionOperator = (ConditionOperator)_jcboOperator.SelectedValue;
				}
				bool isBetween = conditionOperator == ConditionOperator.Between || conditionOperator == ConditionOperator.NotBetween;
				isBetween &= _isConditionRowVisible;
				_secondValueModeButton.Visible = isBetween;
				_secondValueTextBox.Visible = isBetween;
				_secondValueFieldComboBox.Visible = isBetween;
			}
		}

		private void OnChildFilterChanged(object sender, EventArgs eventArgs)
		{
			ResizeToContent();
			OnFilterChanged(eventArgs);
		}

		private void ClearSelection()
		{
			_isSelected = false;
			foreach (Control control in base.Controls)
			{
				if (control is SchedulerFilterConditionEditor)
				{
					((SchedulerFilterConditionEditor)control).ClearSelection();
				}
			}
			Invalidate();
		}

		private SchedulerFilterConditionEditor GetRootEditor()
		{
			if (base.Parent is SchedulerFilterConditionEditor)
			{
				return ((SchedulerFilterConditionEditor)base.Parent).GetRootEditor();
			}
			return this;
		}

		internal bool IsSelected()
		{
			return _isSelected;
		}

		internal void SetSelected(bool isSelected)
		{
			if (_isSelected != isSelected)
			{
				GetRootEditor().ClearSelection();
				_isSelected = isSelected;
				if (_isSelected)
				{
					OnSelectedFilterConditionChanged(EventArgs.Empty);
				}
				Invalidate(invalidateChildren: true);
			}
		}

		protected override void OnEnter(EventArgs eventArgs)
		{
			base.OnEnter(eventArgs);
			if (FindFocusedEditor() == this)
			{
				SetSelected(true);
				Control parent = base.Parent;
				if (parent is SchedulerFilterConditionEditor)
				{
					((SchedulerFilterConditionEditor)parent).SelectEditor(this);
				}
				else
				{
					SelectEditor(this);
				}
				Invalidate();
			}
		}

		protected override void OnLeave(EventArgs eventArgs)
		{
			base.OnLeave(eventArgs);
			if (IsSelected())
			{
				Control parent = base.Parent;
				while (parent is SchedulerFilterConditionEditor)
				{
					if (parent.ContainsFocus)
					{
						((SchedulerFilterConditionEditor)parent).OnEnter(EventArgs.Empty);
						break;
					}
					parent = parent.Parent;
				}
			}
			Invalidate();
		}

		private Control FindFocusedEditor()
		{
			foreach (Control control in base.Controls)
			{
				if (control.ContainsFocus && control is SchedulerFilterConditionEditor)
				{
					return ((SchedulerFilterConditionEditor)control).FindFocusedEditor();
				}
			}
			if (base.ContainsFocus)
			{
				return this;
			}
			return null;
		}

		internal SchedulerFilterConditionEditor FindSelectedEditor()
		{
			if (IsSelected())
			{
				return this;
			}
			foreach (Control control in base.Controls)
			{
				if (control is SchedulerFilterConditionEditor)
				{
					SchedulerFilterConditionEditor selectedChild = ((SchedulerFilterConditionEditor)control).FindSelectedEditor();
					if (selectedChild != null)
					{
						return selectedChild;
					}
				}
			}
			return null;
		}

		internal void SelectEditor(SchedulerFilterConditionEditor editor)
		{
			editor?.SetSelected(true);
		}

		public void AddSelectedFilterConditionChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _schedulerEditorControl6EventHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(previousHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _schedulerEditorControl6EventHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		protected virtual void OnSelectedFilterConditionChanged(EventArgs eventArgs)
		{
			if (_schedulerEditorControl6EventHandler != null)
			{
				_schedulerEditorControl6EventHandler(this, eventArgs);
			}
		}

		private void OnLogicalOperatorChanged(object sender, EventArgs eventArgs)
		{
			if (_schedulerFilterCondition != null)
			{
				ScrollBarStyleControl scrollBarStyleControl = (ScrollBarStyleControl)sender;
				SchedulerFilterCondition schedulerFilterCondition = (SchedulerFilterCondition)scrollBarStyleControl.Tag;
				schedulerFilterCondition.LogicalOperator = (LogicalOperator)scrollBarStyleControl.GetSelectedValue();
				RaiseFilterChangedIfEnabled();
			}
		}

		private ScrollBarStyleControl CreateLogicalOperatorButton(SchedulerFilterCondition schedulerFilterCondition)
		{
			ScrollBarStyleControl scrollBarStyleControl = new ScrollBarStyleControl();
			scrollBarStyleControl.SetChecked(true);
			scrollBarStyleControl.SetValues(new object[3]
			{
				LogicalOperator.And,
				LogicalOperator.Or,
				LogicalOperator.Xor
			});
			scrollBarStyleControl.SelectValue(schedulerFilterCondition.LogicalOperator);
			scrollBarStyleControl.Tag = schedulerFilterCondition;
			scrollBarStyleControl.GetScrollBarStyle().ButtonAppearance = Appearance.Flat;
			scrollBarStyleControl.Font = _firstValueModeButton.Font;
			scrollBarStyleControl.Size = new Size(36, 20);
			scrollBarStyleControl.GotFocus += OnControlGotFocus;
			scrollBarStyleControl.AddSelectedIndexChangedHandler(OnLogicalOperatorChanged);
			return scrollBarStyleControl;
		}

		private void OnFirstValueModeChanged(object sender, EventArgs eventArgs)
		{
			_firstValueTextBox.Visible = _firstValueModeButton.GetSelectedIndex() == 0;
			_firstValueFieldComboBox.Visible = _firstValueModeButton.GetSelectedIndex() == 1;
			if (!_firstValueFieldComboBox.Visible)
			{
				_firstValueFieldComboBox.SelectedIndex = -1;
			}
			else
			{
				_firstValueTextBox.Text = "";
			}
		}

		private void OnNegationChanged(object sender, EventArgs eventArgs)
		{
			if (_schedulerFilterCondition != null)
			{
				_schedulerFilterCondition.Negation = _moptNot.GetSelectedIndex() == 1;
				RaiseFilterChangedIfEnabled();
			}
		}

		private void OnSecondValueModeChanged(object sender, EventArgs eventArgs)
		{
			_secondValueTextBox.Visible = _secondValueModeButton.GetSelectedIndex() == 0;
			_secondValueFieldComboBox.Visible = _secondValueModeButton.GetSelectedIndex() == 1;
			if (!_secondValueFieldComboBox.Visible)
			{
				_secondValueFieldComboBox.SelectedIndex = -1;
			}
			else
			{
				_firstValueTextBox.Text = "";
			}
		}

		private void OnLblAndMouseDown(object sender, MouseEventArgs eventArgs)
		{
			Focus();
		}

		private void ForwardActiveControlChanged(object sender, EventArgs eventArgs)
		{
			OnActiveControlChanged(eventArgs);
		}

		public void AddActiveControlChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _activeControlChangedHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(previousHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _activeControlChangedHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		protected virtual void OnActiveControlChanged(EventArgs eventArgs)
		{
			if (_activeControlChangedHandler != null)
			{
				_activeControlChangedHandler(this, eventArgs);
			}
		}

		internal SchedulerFilterCondition GetSelectedFilterCondition()
		{
			if (FindSelectedEditor() == null)
			{
				return _schedulerFilterCondition;
			}
			return FindSelectedEditor().GetSchedulerFilterCondition();
		}

		internal void SelectCondition(SchedulerFilterCondition schedulerFilterCondition)
		{
			if (_schedulerFilterCondition == schedulerFilterCondition)
			{
				SetSelected(true);
				return;
			}
			foreach (Control control in base.Controls)
			{
				if (control is SchedulerFilterConditionEditor)
				{
					SchedulerFilterConditionEditor childEditor = (SchedulerFilterConditionEditor)control;
					childEditor.SelectCondition(schedulerFilterCondition);
					if (childEditor.IsSelected())
					{
						break;
					}
				}
			}
		}
	}
}
