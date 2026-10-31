using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design
{
	[ToolboxItem(false)]
	internal class FilterConditionTreeEditor : UserControl
	{
		private Label _lblColumn;

		private ComboBox _jcboColumn;

		private ComboBox _jcboOperator;

		private TextBox _txtFirstValue;

		private Label _lblAnd;

		private TextBox _txtSecondValue;

		private ComboBox _jcboFirstValueField;

		private ComboBox _jcboSecondValueField;

		private ScrollBarStyleControl _moptFirstValueKind;

		private ScrollBarStyleControl _moptSecondValueKind;

		private ScrollBarStyleControl _moptNot;

		private Container _container = new Container();

		private MxTimeline _timeline;

		private TimelineFilterCondition _timelineFilterCondition;

		private Size _contentSize;

		private bool _raisesFilterChanged = true;

		private EventHandler _eventHandler;

		private bool _isConditionRowVisible = true;

		private bool _isSelected;

		private EventHandler _timelineEditorControl2EventHandler;

		private EventHandler _activeControlChanged;

		public FilterConditionTreeEditor()
		{
			InitializeComponent();
			_moptFirstValueKind.SetValues(new object[2]
			{
				"Value",
				"Field"
			});
			_moptNot.SetValues(new object[2]
			{
				"",
				"Not"
			});
			_moptSecondValueKind.SetValues(new object[2]
			{
				"Value",
				"Field"
			});
			FillOperatorList();
			LoadConditionIntoControls();
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			Font font = new Font(Font.Name, Font.Size - 1f);
			_moptFirstValueKind.Font = font;
			_moptSecondValueKind.Font = font;
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
			if (_txtFirstValue.Visible)
			{
				Rectangle bounds = _txtFirstValue.Bounds;
				bounds.Inflate(3, 2);
				if (!_txtFirstValue.Enabled)
				{
					eventArgs.Graphics.FillRectangle(SystemBrushes.Control, bounds);
				}
				TimelineUtilities.DrawFlatBorder(eventArgs.Graphics, SystemPens.ControlDark, bounds);
			}
			if (_txtSecondValue.Visible)
			{
				Rectangle bounds = _txtSecondValue.Bounds;
				bounds.Inflate(3, 2);
				if (!_txtSecondValue.Enabled)
				{
					eventArgs.Graphics.FillRectangle(SystemBrushes.Control, bounds);
				}
				TimelineUtilities.DrawFlatBorder(eventArgs.Graphics, SystemPens.ControlDark, bounds);
			}
			RenderContent(eventArgs.Graphics, _moptNot.Bounds.Right + 8, base.ClientRectangle.Top + 1, _contentSize.Height - 4, SystemPens.ControlDark);
			if (_isSelected)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Size = _contentSize;
				Pen highlight = SystemPens.Highlight;
				TimelineUtilities.DrawFlatBorder(eventArgs.Graphics, highlight, clientRectangle);
			}
		}

		public void UpdateTimeline(MxTimeline timeline)
		{
			if (_timeline != timeline)
			{
				_timeline = timeline;
				RebindFieldLists();
			}
		}

		private void RebindFieldLists()
		{
			_jcboColumn.DataSource = null;
			_jcboFirstValueField.DataSource = null;
			_jcboSecondValueField.DataSource = null;
			if (_timeline != null)
			{
				FillFieldComboBox(_jcboColumn, _timeline.Fields);
				FillFieldComboBox(_jcboFirstValueField, _timeline.Fields);
				FillFieldComboBox(_jcboSecondValueField, _timeline.Fields);
			}
			LoadConditionIntoControls();
		}

		private void UpdateLayout()
		{
			bool isEnabled = _timeline != null && _timelineFilterCondition != null;
			SetControlsEnabled(base.Controls, isEnabled);
			Invalidate();
		}

		private void SetControlsEnabled(ControlCollection controlCollection, bool isEnabled)
		{
			foreach (Control item in controlCollection)
			{
				item.Enabled = isEnabled;
				SetControlsEnabled(item.Controls, isEnabled);
			}
		}

		private void FillFieldComboBox(ComboBox comboBox, TimelineFieldCollection timelineFieldCollection)
		{
			comboBox.SelectedValueChanged -= OnJcboColumnValue2SelectedValueChanged;
			if (comboBox != null && timelineFieldCollection != null)
			{
				ArrayList arrayList = new ArrayList();
				arrayList.Add(new ObjectName(null, "(none)"));
				foreach (TimelineField item in timelineFieldCollection)
				{
					arrayList.Add(new ObjectName(item, TimelineUtilities.GetObjectDisplayText(_timeline, item)));
				}
				comboBox.DataSource = arrayList;
				comboBox.ValueMember = "Object";
				comboBox.DisplayMember = "Name";
			}
			comboBox.SelectedValueChanged += OnJcboColumnValue2SelectedValueChanged;
		}

		private void FillOperatorList()
		{
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new ObjectName(ConditionOperator.Equal, "="));
			arrayList.Add(new ObjectName(ConditionOperator.NotEqual, "<>"));
			arrayList.Add(new ObjectName(ConditionOperator.GreaterThan, ">"));
			arrayList.Add(new ObjectName(ConditionOperator.LessThan, "<"));
			arrayList.Add(new ObjectName(ConditionOperator.GreaterThanOrEqualTo, ">="));
			arrayList.Add(new ObjectName(ConditionOperator.LessThanOrEqualTo, "<="));
			arrayList.Add(new ObjectName(ConditionOperator.Between, "Between"));
			arrayList.Add(new ObjectName(ConditionOperator.NotBetween, "Not Between"));
			arrayList.Add(new ObjectName(ConditionOperator.Contains, "Contains"));
			arrayList.Add(new ObjectName(ConditionOperator.NotContains, "Doesn't contain"));
			arrayList.Add(new ObjectName(ConditionOperator.BeginsWith, "Begins with"));
			arrayList.Add(new ObjectName(ConditionOperator.EndsWith, "Ends With"));
			arrayList.Add(new ObjectName(ConditionOperator.IsNull, "Is Null"));
			arrayList.Add(new ObjectName(ConditionOperator.NotIsNull, "Isn't Null"));
			arrayList.Add(new ObjectName(ConditionOperator.IsEmpty, "Is empty"));
			arrayList.Add(new ObjectName(ConditionOperator.NotIsEmpty, "Isn't empty"));
			_jcboOperator.DataSource = arrayList;
			_jcboOperator.ValueMember = "Object";
			_jcboOperator.DisplayMember = "Name";
		}

		public TimelineFilterCondition GetTimelineFilterCondition()
		{
			return _timelineFilterCondition;
		}

		public void SetTimelineFilterCondition(TimelineFilterCondition timelineFilterCondition)
		{
			_timelineFilterCondition = timelineFilterCondition;
			LoadConditionIntoControls();
			if (timelineFilterCondition != null)
			{
				if (timelineFilterCondition.GetOwnerCondition() == null)
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

		private void LoadConditionIntoControls()
		{
			_raisesFilterChanged = false;
			if (_timelineFilterCondition != null)
			{
				if (_timelineFilterCondition.Field == null)
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
					_jcboColumn.SelectedValue = _timelineFilterCondition.Field;
				}
				_jcboOperator.SelectedValue = _timelineFilterCondition.ConditionOperator;
				if (_timelineFilterCondition.ValueField1 == null)
				{
					if (_jcboFirstValueField.Items.Count > 0)
					{
						_jcboFirstValueField.SelectedIndex = 0;
					}
					else
					{
						_jcboFirstValueField.SelectedIndex = -1;
					}
				}
				else
				{
					_jcboFirstValueField.SelectedValue = _timelineFilterCondition.ValueField1;
				}
				if (_timelineFilterCondition.ValueField2 == null)
				{
					if (_jcboSecondValueField.Items.Count > 0)
					{
						_jcboSecondValueField.SelectedIndex = 0;
					}
					else
					{
						_jcboSecondValueField.SelectedIndex = -1;
					}
				}
				else
				{
					_jcboSecondValueField.SelectedValue = _timelineFilterCondition.ValueField2;
				}
				_txtFirstValue.Text = string.Concat(_timelineFilterCondition.Value1);
				_txtSecondValue.Text = string.Concat(_timelineFilterCondition.Value2);
				if (_timelineFilterCondition.Negation)
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
					_moptFirstValueKind.SetSelectedIndex(0);
					if (_jcboColumn.Items.Count > 0)
					{
						_jcboColumn.SelectedIndex = 0;
					}
					else
					{
						_jcboColumn.SelectedIndex = -1;
					}
					if (_jcboFirstValueField.Items.Count > 0)
					{
						_jcboFirstValueField.SelectedIndex = 0;
					}
					else
					{
						_jcboFirstValueField.SelectedIndex = -1;
					}
					if (_jcboSecondValueField.Items.Count > 0)
					{
						_jcboSecondValueField.SelectedIndex = 0;
					}
					else
					{
						_jcboSecondValueField.SelectedIndex = -1;
					}
					_txtFirstValue.Text = "";
					_txtSecondValue.Text = "";
					_jcboOperator.SelectedValue = -1;
					_moptNot.SetSelectedIndex(0);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			}
			RebuildChildControls();
			ResizeToContent();
			UpdateLayout();
			_raisesFilterChanged = true;
			NotifyFilterChanged();
		}

		internal FilterConditionTreeEditor FindEditorForCondition(TimelineFilterCondition timelineFilterCondition)
		{
			if (timelineFilterCondition == _timelineFilterCondition)
			{
				return this;
			}
			foreach (Control control in base.Controls)
			{
				if (control is FilterConditionTreeEditor)
				{
					FilterConditionTreeEditor foundEditor = ((FilterConditionTreeEditor)control).FindEditorForCondition(timelineFilterCondition);
					if (foundEditor != null)
					{
						return foundEditor;
					}
				}
			}
			return null;
		}

		private void RebuildChildControls()
		{
			RemoveChildControls();
			if (_timelineFilterCondition != null && _timelineFilterCondition.IsComposite)
			{
				SetConditionRowVisible(!_timelineFilterCondition.IsEmpty());
				int nextTop = 2;
				if (_isConditionRowVisible)
				{
					nextTop += 24;
				}
				{
					foreach (TimelineFilterCondition condition in _timelineFilterCondition.Conditions)
					{
						if (!_timelineFilterCondition.IsEmpty() || _timelineFilterCondition.Conditions.IndexOf(condition) > 0)
						{
							nextTop += 2;
							ScrollBarStyleControl scrollBarStyleControl = CreateLogicalOperatorButton(condition);
							scrollBarStyleControl.Location = new Point(_jcboColumn.Left + 8, nextTop);
							base.Controls.Add(scrollBarStyleControl);
							nextTop += 22;
						}
						FilterConditionTreeEditor childEditor = new FilterConditionTreeEditor();
						childEditor.BackColor = BackColor;
						childEditor.UpdateTimeline(_timeline);
						childEditor.SetTimelineFilterCondition(condition);
						childEditor.AddFilterChangedHandler(OnChildFilterChanged);
						childEditor.AddSelectedFilterConditionChangedHandler(OnChildSelectedFilterConditionChanged);
						childEditor.AddActiveControlChangedHandler(OnChildActiveControlChanged);
						childEditor.Location = new Point(_lblColumn.Left, nextTop);
						base.Controls.Add(childEditor);
						nextTop += childEditor.Height;
					}
					return;
				}
			}
			SetConditionRowVisible(true);
		}

		private void OnChildSelectedFilterConditionChanged(object sender, EventArgs eventArgs)
		{
			OnSelectedFilterConditionChanged(eventArgs);
		}

		private void OnControlGotFocus(object sender, EventArgs eventArgs)
		{
			OnActiveControlChanged(eventArgs);
		}

		public Control GetFocusedControl()
		{
			if (base.ActiveControl == null)
			{
				FilterConditionTreeEditor focusedEditor = (FilterConditionTreeEditor)GetFocusedEditor();
				if (focusedEditor == null)
				{
					return this;
				}
				return focusedEditor.ActiveControl;
			}
			return base.ActiveControl;
		}

		private void RemoveChildControls()
		{
			ArrayList arrayList = new ArrayList();
			foreach (Control childControl in base.Controls)
			{
				if (childControl is FilterConditionTreeEditor)
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
				if (item is FilterConditionTreeEditor)
				{
					FilterConditionTreeEditor childEditor = (FilterConditionTreeEditor)item;
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
			_jcboFirstValueField = new ComboBox();
			_jcboSecondValueField = new ComboBox();
			_txtFirstValue = new TextBox();
			_lblAnd = new Label();
			_txtSecondValue = new TextBox();
			_moptFirstValueKind = new ScrollBarStyleControl();
			_moptSecondValueKind = new ScrollBarStyleControl();
			_moptNot = new ScrollBarStyleControl();
			SuspendLayout();
			_lblColumn.AutoSize = true;
			_lblColumn.Enabled = false;
			_lblColumn.Location = new Point(48, 5);
			_lblColumn.Name = "lblColumn";
			_lblColumn.Size = new Size(46, 14);
			_lblColumn.TabIndex = 0;
			_lblColumn.Text = "Field:";
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
			_jcboFirstValueField.BackColor = SystemColors.Window;
			_jcboFirstValueField.DropDownStyle = ComboBoxStyle.DropDownList;
			_jcboFirstValueField.DisplayMember = "Name";
			_jcboFirstValueField.Enabled = false;
			_jcboFirstValueField.ForeColor = SystemColors.WindowText;
			_jcboFirstValueField.Location = new Point(376, 2);
			_jcboFirstValueField.Name = "jcboColumnValue1";
			_jcboFirstValueField.Size = new Size(120, 20);
			_jcboFirstValueField.TabIndex = 16;
			_jcboFirstValueField.SelectedValue = null;
			_jcboFirstValueField.ValueMember = "Object";
			_jcboFirstValueField.Visible = false;
			_jcboFirstValueField.SelectedValueChanged += OnJcboColumnValue2SelectedValueChanged;
			_jcboSecondValueField.BackColor = SystemColors.Window;
			_jcboSecondValueField.DropDownStyle = ComboBoxStyle.DropDownList;
			_jcboSecondValueField.DisplayMember = "Name";
			_jcboSecondValueField.Enabled = false;
			_jcboSecondValueField.ForeColor = SystemColors.WindowText;
			_jcboSecondValueField.Location = new Point(570, 2);
			_jcboSecondValueField.Name = "jcboColumnValue2";
			_jcboSecondValueField.Size = new Size(120, 20);
			_jcboSecondValueField.TabIndex = 17;
			_jcboSecondValueField.SelectedValue = null;
			_jcboSecondValueField.ValueMember = "Object";
			_jcboSecondValueField.Visible = false;
			_jcboSecondValueField.SelectedValueChanged += OnJcboColumnValue2SelectedValueChanged;
			_txtFirstValue.AutoSize = false;
			_txtFirstValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_txtFirstValue.Enabled = false;
			_txtFirstValue.Location = new Point(416, 2);
			_txtFirstValue.Name = "txtValue1";
			_txtFirstValue.Size = new Size(108, 20);
			_txtFirstValue.TabIndex = 5;
			_txtFirstValue.Text = "";
			_txtFirstValue.TextChanged += OnTxtValue2TextChanged;
			_lblAnd.AutoSize = true;
			_lblAnd.Enabled = false;
			_lblAnd.Location = new Point(496, 5);
			_lblAnd.Name = "lblAnd";
			_lblAnd.Size = new Size(24, 14);
			_lblAnd.TabIndex = 11;
			_lblAnd.Text = "And";
			_lblAnd.Visible = false;
			_lblAnd.MouseDown += OnLblAndMouseDown;
			_txtSecondValue.AutoSize = false;
			_txtSecondValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_txtSecondValue.Enabled = false;
			_txtSecondValue.Location = new Point(636, 2);
			_txtSecondValue.Name = "txtValue2";
			_txtSecondValue.Size = new Size(112, 20);
			_txtSecondValue.TabIndex = 14;
			_txtSecondValue.Text = "";
			_txtSecondValue.Visible = false;
			_txtSecondValue.TextChanged += OnTxtValue2TextChanged;
			_moptFirstValueKind.GetScrollBarStyle().ButtonAppearance = Appearance.Flat;
			_moptFirstValueKind.Enabled = false;
			_moptFirstValueKind.Font = new Font("Tahoma", 6.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_moptFirstValueKind.Location = new Point(324, 2);
			_moptFirstValueKind.Name = "moptValue1";
			_moptFirstValueKind.SetSelectedIndex(-1);
			_moptFirstValueKind.SelectValue(null);
			_moptFirstValueKind.Size = new Size(50, 20);
			_moptFirstValueKind.TabIndex = 25;
			_moptFirstValueKind.Text = "multiOptionButton1";
			_moptFirstValueKind.SetChecked(false);
			_moptFirstValueKind.SetValues(null);
			_moptFirstValueKind.AddSelectedIndexChangedHandler(OnFirstValueKindChanged);
			_moptSecondValueKind.GetScrollBarStyle().ButtonAppearance = Appearance.Flat;
			_moptSecondValueKind.Enabled = false;
			_moptSecondValueKind.Font = new Font("Tahoma", 6.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_moptSecondValueKind.Location = new Point(520, 2);
			_moptSecondValueKind.Name = "moptValue2";
			_moptSecondValueKind.SetSelectedIndex(-1);
			_moptSecondValueKind.SelectValue(null);
			_moptSecondValueKind.Size = new Size(50, 20);
			_moptSecondValueKind.TabIndex = 26;
			_moptSecondValueKind.Text = "multiOptionButton1";
			_moptSecondValueKind.SetChecked(false);
			_moptSecondValueKind.SetValues(null);
			_moptSecondValueKind.Visible = false;
			_moptSecondValueKind.AddSelectedIndexChangedHandler(OnSecondValueKindChanged);
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
				_moptNot, _moptSecondValueKind, _moptFirstValueKind, _lblAnd, _jcboOperator, _jcboColumn, _lblColumn, _txtFirstValue, _jcboFirstValueField, _txtSecondValue,
				_jcboSecondValueField
			});
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "SimpleFilter";
			base.Size = new Size(760, 24);
			ResumeLayout(performLayout: false);
		}

		private void OnJcboOperatorSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_timelineFilterCondition != null)
			{
				_timelineFilterCondition.ConditionOperator = (ConditionOperator)_jcboOperator.SelectedValue;
			}
			bool isBetween = false;
			if (_jcboOperator.SelectedValue != null && ((ConditionOperator)_jcboOperator.SelectedValue == ConditionOperator.Between || (ConditionOperator)_jcboOperator.SelectedValue == ConditionOperator.NotBetween))
			{
				isBetween = true;
			}
			_lblAnd.Visible = isBetween;
			_moptSecondValueKind.Visible = isBetween;
			_txtSecondValue.Visible = isBetween && _moptSecondValueKind.GetSelectedIndex() == 0;
			_jcboSecondValueField.Visible = isBetween && _moptSecondValueKind.GetSelectedIndex() == 1;
			ResizeToContent();
			NotifyFilterChanged();
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
			if (_timelineFilterCondition != null)
			{
				_timelineFilterCondition.GetOwnerCondition();
			}
			bool isLeafCondition = _timelineFilterCondition != null && !_timelineFilterCondition.IsComposite;
			int rowTop = 0;
			if (_isConditionRowVisible)
			{
				rowTop = 2;
				_lblColumn.Top = rowTop + (20 - _lblColumn.Height) / 2;
				_lblAnd.Top = _lblColumn.Top;
				_jcboColumn.Top = rowTop;
				_jcboOperator.Top = rowTop;
				_moptFirstValueKind.Top = rowTop;
				_jcboFirstValueField.Top = rowTop;
				_jcboSecondValueField.Top = rowTop;
				_moptSecondValueKind.Top = rowTop;
			}
			Rectangle bounds = _jcboFirstValueField.Bounds;
			bounds.Inflate(-3, -2);
			_txtFirstValue.Bounds = bounds;
			bounds = _jcboSecondValueField.Bounds;
			bounds.Inflate(-3, -2);
			_txtSecondValue.Bounds = bounds;
			int contentWidth = 0;
			ConditionOperator conditionOperator = ConditionOperator.Equal;
			if (_jcboOperator.SelectedValue != null)
			{
				conditionOperator = (ConditionOperator)_jcboOperator.SelectedValue;
			}
			contentWidth = ((_jcboOperator.SelectedValue == null || (conditionOperator != ConditionOperator.Between && conditionOperator != ConditionOperator.NotBetween)) ? (_jcboFirstValueField.Bounds.Right + 2) : (_jcboSecondValueField.Bounds.Right + 2));
			int height = 24;
			_contentSize = new Size(contentWidth, height);
			int bottomPadding = 2;
			if (!isLeafCondition)
			{
				int maxChildRight = 0;
				int maxChildBottom = 0;
				foreach (Control control in base.Controls)
				{
					if (control is FilterConditionTreeEditor)
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
				if (!(base.Parent is FilterConditionTreeEditor))
				{
					bottomPadding = 4;
					_contentSize.Height += 2;
				}
			}
			base.Size = _contentSize;
			rowTop = 2;
			if (_timelineFilterCondition != null && _timelineFilterCondition.IsComposite)
			{
				_moptNot.Top = rowTop + (_contentSize.Height - _moptNot.Height - rowTop - bottomPadding) / 2;
			}
			else
			{
				_moptNot.Top = rowTop;
			}
			Invalidate();
		}

		private void NotifyFilterChanged()
		{
			if (_raisesFilterChanged)
			{
				OnFilterChanged(EventArgs.Empty);
			}
		}

		public void AddFilterChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _eventHandler;
			EventHandler expectedHandler;
			do
			{
				expectedHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(expectedHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _eventHandler, value, expectedHandler);
			}
			while ((object)eventHandler != expectedHandler);
		}

		public void RemoveFilterChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _eventHandler;
			EventHandler expectedHandler;
			do
			{
				expectedHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(expectedHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _eventHandler, value, expectedHandler);
			}
			while ((object)eventHandler != expectedHandler);
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
			if (_timelineFilterCondition != null)
			{
				if (sender == _jcboColumn)
				{
					_timelineFilterCondition.Field = (TimelineField)_jcboColumn.SelectedValue;
				}
				else if (sender == _jcboFirstValueField)
				{
					_timelineFilterCondition.ValueField1 = (TimelineField)_jcboFirstValueField.SelectedValue;
				}
				else if (sender == _jcboSecondValueField)
				{
					_timelineFilterCondition.ValueField2 = (TimelineField)_jcboSecondValueField.SelectedValue;
				}
			}
			NotifyFilterChanged();
		}

		private void OnTxtValue2TextChanged(object sender, EventArgs eventArgs)
		{
			if (_timelineFilterCondition != null)
			{
				string text = ((TextBox)sender).Text;
				object conditionValue = null;
				if (text != "")
				{
					conditionValue = ((!(text == string.Concat('"', '"'))) ? text : "");
				}
				if (sender == _txtFirstValue)
				{
					_timelineFilterCondition.Value1 = conditionValue;
				}
				else
				{
					_timelineFilterCondition.Value2 = conditionValue;
				}
			}
			NotifyFilterChanged();
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

		internal void SetConditionRowVisible(bool timelineEditorControl2Boolean)
		{
			if (_isConditionRowVisible != timelineEditorControl2Boolean)
			{
				_isConditionRowVisible = timelineEditorControl2Boolean;
				_lblColumn.Visible = _isConditionRowVisible;
				_jcboColumn.Visible = _isConditionRowVisible;
				_jcboOperator.Visible = _isConditionRowVisible;
				_txtFirstValue.Visible = _isConditionRowVisible && _moptFirstValueKind.GetSelectedIndex() == 0;
				_jcboFirstValueField.Visible = _isConditionRowVisible && _moptFirstValueKind.GetSelectedIndex() == 1;
				_moptFirstValueKind.Visible = _isConditionRowVisible;
				ConditionOperator conditionOperator = ConditionOperator.Equal;
				if (_jcboOperator.SelectedValue != null)
				{
					conditionOperator = (ConditionOperator)_jcboOperator.SelectedValue;
				}
				bool isBetween = conditionOperator == ConditionOperator.Between || conditionOperator == ConditionOperator.NotBetween;
				isBetween &= _isConditionRowVisible;
				_moptSecondValueKind.Visible = isBetween;
				_txtSecondValue.Visible = isBetween;
				_jcboSecondValueField.Visible = isBetween;
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
				if (control is FilterConditionTreeEditor)
				{
					((FilterConditionTreeEditor)control).ClearSelection();
				}
			}
			Invalidate();
		}

		private FilterConditionTreeEditor GetRootEditor()
		{
			if (base.Parent is FilterConditionTreeEditor)
			{
				return ((FilterConditionTreeEditor)base.Parent).GetRootEditor();
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
			if (GetFocusedEditor() == this)
			{
				SetSelected(true);
				Control parent = base.Parent;
				if (parent is FilterConditionTreeEditor)
				{
					((FilterConditionTreeEditor)parent).SelectEditor(this);
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
				while (parent is FilterConditionTreeEditor)
				{
					if (parent.ContainsFocus)
					{
						((FilterConditionTreeEditor)parent).OnEnter(EventArgs.Empty);
						break;
					}
					parent = parent.Parent;
				}
			}
			Invalidate();
		}

		private Control GetFocusedEditor()
		{
			foreach (Control control in base.Controls)
			{
				if (control.ContainsFocus && control is FilterConditionTreeEditor)
				{
					return ((FilterConditionTreeEditor)control).GetFocusedEditor();
				}
			}
			if (base.ContainsFocus)
			{
				return this;
			}
			return null;
		}

		internal FilterConditionTreeEditor FindSelectedEditor()
		{
			if (IsSelected())
			{
				return this;
			}
			foreach (Control control in base.Controls)
			{
				if (control is FilterConditionTreeEditor)
				{
					FilterConditionTreeEditor selectedChild = ((FilterConditionTreeEditor)control).FindSelectedEditor();
					if (selectedChild != null)
					{
						return selectedChild;
					}
				}
			}
			return null;
		}

		internal void SelectEditor(FilterConditionTreeEditor editor)
		{
			editor?.SetSelected(true);
		}

		public void AddSelectedFilterConditionChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _timelineEditorControl2EventHandler;
			EventHandler expectedHandler;
			do
			{
				expectedHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(expectedHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _timelineEditorControl2EventHandler, value, expectedHandler);
			}
			while ((object)eventHandler != expectedHandler);
		}

		protected virtual void OnSelectedFilterConditionChanged(EventArgs eventArgs)
		{
			if (_timelineEditorControl2EventHandler != null)
			{
				_timelineEditorControl2EventHandler(this, eventArgs);
			}
		}

		private void OnLogicalOperatorChanged(object sender, EventArgs eventArgs)
		{
			if (_timelineFilterCondition != null)
			{
				ScrollBarStyleControl scrollBarStyleControl = (ScrollBarStyleControl)sender;
				TimelineFilterCondition timelineFilterCondition = (TimelineFilterCondition)scrollBarStyleControl.Tag;
				timelineFilterCondition.LogicalOperator = (LogicalOperator)scrollBarStyleControl.GetSelectedValue();
				NotifyFilterChanged();
			}
		}

		private ScrollBarStyleControl CreateLogicalOperatorButton(TimelineFilterCondition timelineFilterCondition)
		{
			ScrollBarStyleControl scrollBarStyleControl = new ScrollBarStyleControl();
			scrollBarStyleControl.SetChecked(true);
			scrollBarStyleControl.SetValues(new object[3]
			{
				LogicalOperator.And,
				LogicalOperator.Or,
				LogicalOperator.Xor
			});
			scrollBarStyleControl.SelectValue(timelineFilterCondition.LogicalOperator);
			scrollBarStyleControl.Tag = timelineFilterCondition;
			scrollBarStyleControl.GetScrollBarStyle().ButtonAppearance = Appearance.Flat;
			scrollBarStyleControl.Font = _moptFirstValueKind.Font;
			scrollBarStyleControl.Size = new Size(36, 20);
			scrollBarStyleControl.GotFocus += OnControlGotFocus;
			scrollBarStyleControl.AddSelectedIndexChangedHandler(OnLogicalOperatorChanged);
			return scrollBarStyleControl;
		}

		private void OnFirstValueKindChanged(object sender, EventArgs eventArgs)
		{
			_txtFirstValue.Visible = _moptFirstValueKind.GetSelectedIndex() == 0;
			_jcboFirstValueField.Visible = _moptFirstValueKind.GetSelectedIndex() == 1;
			if (!_jcboFirstValueField.Visible)
			{
				_jcboFirstValueField.SelectedValue = null;
			}
			else
			{
				_txtFirstValue.Text = "";
			}
		}

		private void OnNegationChanged(object sender, EventArgs eventArgs)
		{
			if (_timelineFilterCondition != null)
			{
				_timelineFilterCondition.Negation = _moptNot.GetSelectedIndex() == 1;
				NotifyFilterChanged();
			}
		}

		private void OnSecondValueKindChanged(object sender, EventArgs eventArgs)
		{
			_txtSecondValue.Visible = _moptSecondValueKind.GetSelectedIndex() == 0;
			_jcboSecondValueField.Visible = _moptSecondValueKind.GetSelectedIndex() == 1;
			if (!_jcboSecondValueField.Visible)
			{
				_jcboSecondValueField.SelectedValue = null;
			}
			else
			{
				_txtFirstValue.Text = "";
			}
		}

		private void OnLblAndMouseDown(object sender, MouseEventArgs eventArgs)
		{
			Focus();
		}

		private void OnChildActiveControlChanged(object sender, EventArgs eventArgs)
		{
			OnActiveControlChanged(eventArgs);
		}

		public void AddActiveControlChangedHandler(EventHandler eventHandlerArgument)
		{
			EventHandler eventHandler = _activeControlChanged;
			EventHandler expectedHandler;
			do
			{
				expectedHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(expectedHandler, eventHandlerArgument);
				eventHandler = Interlocked.CompareExchange(ref _activeControlChanged, value, expectedHandler);
			}
			while ((object)eventHandler != expectedHandler);
		}

		protected virtual void OnActiveControlChanged(EventArgs eventArgs)
		{
			if (_activeControlChanged != null)
			{
				_activeControlChanged(this, eventArgs);
			}
		}

		internal TimelineFilterCondition GetSelectedFilterCondition()
		{
			if (FindSelectedEditor() == null)
			{
				return _timelineFilterCondition;
			}
			return FindSelectedEditor().GetTimelineFilterCondition();
		}

		internal void SelectCondition(TimelineFilterCondition timelineFilterCondition)
		{
			if (_timelineFilterCondition == timelineFilterCondition)
			{
				SetSelected(true);
				return;
			}
			foreach (Control control in base.Controls)
			{
				if (control is FilterConditionTreeEditor)
				{
					FilterConditionTreeEditor childEditor = (FilterConditionTreeEditor)control;
					childEditor.SelectCondition(timelineFilterCondition);
					if (childEditor.IsSelected())
					{
						break;
					}
				}
			}
		}
	}
}
