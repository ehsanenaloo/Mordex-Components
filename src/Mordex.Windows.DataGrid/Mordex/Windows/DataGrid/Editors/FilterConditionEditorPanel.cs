using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	internal class FilterConditionEditorPanel : UserControl
	{
		private Label _lblColumn;

		private MxMultiColumnComboBox _jcboColumn;

		private MxMultiColumnComboBox _jcboOperator;

		private TextBox _txtFirstValue;

		private Label _lblAnd;

		private TextBox _txtSecondValue;

		private MxMultiColumnComboBox _cboFirstValueColumn;

		private MxMultiColumnComboBox _cboSecondValueColumn;

		private MultiOptionButton _optFirstValueSource;

		private MultiOptionButton _optSecondValueSource;

		private MultiOptionButton _moptNot;

		private Container _container = new Container();

		private GridTable _filterTable;

		private GridFilterCondition _gridFilterCondition;

		private Size _lastLayoutSize;

		private bool _canRaiseFilterChanged = true;

		private EventHandler _filterChangedHandler;

		private bool _filterConditionEditorControl2Boolean = true;

		private bool _isActive;

		private EventHandler _filterConditionEditorControl2EventHandler;

		private EventHandler _activeControlChangedHandler;

		public FilterConditionEditorPanel()
		{
			InitializeComponent();
			_optFirstValueSource.Values = new object[2]
			{
				"Value",
				"Column"
			};
			_moptNot.Values = new object[2]
			{
				"",
				"Not"
			};
			_optSecondValueSource.Values = new object[2]
			{
				"Value",
				"Column"
			};
			FillOperatorList();
			LoadConditionIntoControls();
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			Font font = new Font(Font.Name, Font.Size - 1f);
			_optFirstValueSource.Font = font;
			_optSecondValueSource.Font = font;
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
				GridHelper.DrawFlatBorder(eventArgs.Graphics, SystemPens.ControlDark, bounds);
			}
			if (_txtSecondValue.Visible)
			{
				Rectangle bounds = _txtSecondValue.Bounds;
				bounds.Inflate(3, 2);
				if (!_txtSecondValue.Enabled)
				{
					eventArgs.Graphics.FillRectangle(SystemBrushes.Control, bounds);
				}
				GridHelper.DrawFlatBorder(eventArgs.Graphics, SystemPens.ControlDark, bounds);
			}
			RenderContent(eventArgs.Graphics, _moptNot.Bounds.Right + 8, base.ClientRectangle.Top + 1, _lastLayoutSize.Height - 4, SystemPens.ControlDark);
			if (_isActive)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Size = _lastLayoutSize;
				Pen highlight = SystemPens.Highlight;
				GridHelper.DrawFlatBorder(eventArgs.Graphics, highlight, clientRectangle);
			}
		}

		public void SetTable(GridTable gridTable)
		{
			if (_filterTable != gridTable)
			{
				_filterTable = gridTable;
				ReloadColumnLists();
			}
		}

		private void ReloadColumnLists()
		{
			_jcboColumn.DataSource = null;
			_cboFirstValueColumn.DataSource = null;
			_cboSecondValueColumn.DataSource = null;
			if (_filterTable != null)
			{
				ProcessMultiColumnCombo(_jcboColumn, _filterTable.Columns);
				ProcessMultiColumnCombo(_cboFirstValueColumn, _filterTable.Columns);
				ProcessMultiColumnCombo(_cboSecondValueColumn, _filterTable.Columns);
			}
			LoadConditionIntoControls();
		}

		private void RefreshControlsEnabled()
		{
			bool condition = _filterTable != null && _gridFilterCondition != null;
			SetChildControlsEnabled(base.Controls, condition);
			Invalidate();
		}

		private void SetChildControlsEnabled(ControlCollection controlCollection, bool enabled)
		{
			foreach (Control item in controlCollection)
			{
				item.Enabled = enabled;
				SetChildControlsEnabled(item.Controls, enabled);
			}
		}

		private void ProcessMultiColumnCombo(MxMultiColumnComboBox multiColumnCombo, GridColumnCollection gridColumnCollection)
		{
			if (multiColumnCombo == null || gridColumnCollection == null)
			{
				return;
			}
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new GridValueListItem(DBNull.Value, "(none)"));
			foreach (GridColumn item in gridColumnCollection)
			{
				arrayList.Add(new GridValueListItem(item, GridHelper.GetNodeDisplayText(_filterTable.DataGrid, item)));
			}
			multiColumnCombo.DataSource = arrayList;
			multiColumnCombo.ValueMember = "Value";
			multiColumnCombo.DisplayMember = "Text";
		}

		private void FillOperatorList()
		{
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new GridValueListItem(ConditionOperator.Equal, "="));
			arrayList.Add(new GridValueListItem(ConditionOperator.NotEqual, "<>"));
			arrayList.Add(new GridValueListItem(ConditionOperator.GreaterThan, ">"));
			arrayList.Add(new GridValueListItem(ConditionOperator.LessThan, "<"));
			arrayList.Add(new GridValueListItem(ConditionOperator.GreaterThanOrEqualTo, ">="));
			arrayList.Add(new GridValueListItem(ConditionOperator.LessThanOrEqualTo, "<="));
			arrayList.Add(new GridValueListItem(ConditionOperator.Between, "Between"));
			arrayList.Add(new GridValueListItem(ConditionOperator.NotBetween, "Not Between"));
			arrayList.Add(new GridValueListItem(ConditionOperator.Contains, "Contains"));
			arrayList.Add(new GridValueListItem(ConditionOperator.NotContains, "Doesn't contain"));
			arrayList.Add(new GridValueListItem(ConditionOperator.BeginsWith, "Begins with"));
			arrayList.Add(new GridValueListItem(ConditionOperator.EndsWith, "Ends With"));
			arrayList.Add(new GridValueListItem(ConditionOperator.IsNull, "Is Null"));
			arrayList.Add(new GridValueListItem(ConditionOperator.NotIsNull, "Isn't Null"));
			arrayList.Add(new GridValueListItem(ConditionOperator.IsEmpty, "Is empty"));
			arrayList.Add(new GridValueListItem(ConditionOperator.NotIsEmpty, "Isn't empty"));
			_jcboOperator.DataSource = arrayList;
			_jcboOperator.ValueMember = "Value";
			_jcboOperator.DisplayMember = "Text";
		}

		public GridFilterCondition GetCondition()
		{
			return _gridFilterCondition;
		}

		public void SetCondition(GridFilterCondition gridFilterCondition)
		{
			_gridFilterCondition = gridFilterCondition;
			LoadConditionIntoControls();
			if (gridFilterCondition != null)
			{
				if (gridFilterCondition.GetParentCollection() == null)
				{
					SetActive(true);
				}
			}
			else
			{
				_isActive = false;
			}
			OnFilterChanged(EventArgs.Empty);
		}

		private void LoadConditionIntoControls()
		{
			_canRaiseFilterChanged = false;
			if (_gridFilterCondition != null)
			{
				_jcboColumn.Value = _gridFilterCondition.Column;
				_jcboOperator.Value = _gridFilterCondition.ConditionOperator;
				_cboSecondValueColumn.Value = _gridFilterCondition.ValueColumn2;
				_txtSecondValue.Text = string.Concat(_gridFilterCondition.Value2);
				if (_gridFilterCondition.Negation)
				{
					_moptNot.SelectedIndex = 1;
				}
				else
				{
					_moptNot.SelectedIndex = 0;
				}
				if (_gridFilterCondition.ValueColumn1 != null)
				{
					_optFirstValueSource.SelectedIndex = 1;
					_cboFirstValueColumn.Value = _gridFilterCondition.ValueColumn1;
				}
				else
				{
					_optFirstValueSource.SelectedIndex = 0;
					_txtFirstValue.Text = string.Concat(_gridFilterCondition.Value1);
				}
			}
			else
			{
				_optFirstValueSource.SelectedIndex = 0;
				_jcboColumn.Value = null;
				_cboFirstValueColumn.Value = null;
				_cboSecondValueColumn.Value = null;
				_txtFirstValue.Text = "";
				_txtSecondValue.Text = "";
				_jcboOperator.Value = null;
				_moptNot.SelectedIndex = 0;
			}
			RebuildChildEditors();
			LayoutControls();
			RefreshControlsEnabled();
			_canRaiseFilterChanged = true;
			RaiseFilterChangedIfEnabled();
		}

		internal FilterConditionEditorPanel FindEditorForCondition(GridFilterCondition gridFilterCondition)
		{
			if (gridFilterCondition == _gridFilterCondition)
			{
				return this;
			}
			foreach (Control control in base.Controls)
			{
				if (control is FilterConditionEditorPanel)
				{
					FilterConditionEditorPanel editorForCondition = ((FilterConditionEditorPanel)control).FindEditorForCondition(gridFilterCondition);
					if (editorForCondition != null)
					{
						return editorForCondition;
					}
				}
			}
			return null;
		}

		private void RebuildChildEditors()
		{
			long ticks = DateTime.Now.Ticks;
			RemoveChildEditors();
			if (_gridFilterCondition != null && _gridFilterCondition.IsComposite)
			{
				UpdateFilterConditionEditorControl2Boolean(!_gridFilterCondition.IsEmptyCondition());
				int value = 2;
				if (_filterConditionEditorControl2Boolean)
				{
					value += 24;
				}
				foreach (GridFilterCondition condition in _gridFilterCondition.Conditions)
				{
					if (!_gridFilterCondition.IsEmptyCondition() || _gridFilterCondition.Conditions.IndexOf(condition) > 0)
					{
						value += 2;
						MultiOptionButton multiOptionButton = CreateLogicalOperatorButton(condition);
						multiOptionButton.Location = new Point(_jcboColumn.Left + 8, value);
						base.Controls.Add(multiOptionButton);
						value += 22;
					}
					FilterConditionEditorPanel filterConditionEditorPanel = new FilterConditionEditorPanel();
					filterConditionEditorPanel.BackColor = BackColor;
					filterConditionEditorPanel.SetTable(_filterTable);
					filterConditionEditorPanel.SetCondition(condition);
					filterConditionEditorPanel.AddFilterChangedHandler(OnChildFilterChanged);
					filterConditionEditorPanel.AddSelectionChangedHandler(HandleSelectedConditionChanged);
					filterConditionEditorPanel.AddActiveControlChangedHandler(HandleActiveControlChanged);
					filterConditionEditorPanel.Location = new Point(_lblColumn.Left, value);
					base.Controls.Add(filterConditionEditorPanel);
					value += filterConditionEditorPanel.Height;
				}
			}
			else
			{
				UpdateFilterConditionEditorControl2Boolean(true);
			}
			ticks = DateTime.Now.Ticks - ticks;
		}

		private void HandleSelectedConditionChanged(object sender, EventArgs eventArgs)
		{
			OnSelectedFilterConditionChanged(eventArgs);
		}

		private void OnControlGotFocus(object sender, EventArgs eventArgs)
		{
			OnActiveControlChanged(eventArgs);
		}

		public Control GetFocusedChildControl()
		{
			if (base.ActiveControl == null)
			{
				FilterConditionEditorPanel focusedControl = (FilterConditionEditorPanel)FindFocusedControl();
				if (focusedControl == null)
				{
					return this;
				}
				return focusedControl.ActiveControl;
			}
			return base.ActiveControl;
		}

		private void RemoveChildEditors()
		{
			ArrayList arrayList = new ArrayList();
			foreach (Control control in base.Controls)
			{
				if (control is FilterConditionEditorPanel)
				{
					arrayList.Add(control);
				}
				if (control is MultiOptionButton && ((MultiOptionButton)control).Tag != null)
				{
					arrayList.Add(control);
				}
			}
			foreach (Control item in arrayList)
			{
				if (item is FilterConditionEditorPanel)
				{
					FilterConditionEditorPanel value = (FilterConditionEditorPanel)item;
					value.RemoveFilterChangedHandler(OnChildFilterChanged);
					base.Controls.Remove(value);
				}
				else if (item is MultiOptionButton)
				{
					MultiOptionButton multiOptionButton = (MultiOptionButton)item;
					multiOptionButton.SelectedIndexChanged -= OnControlSelectedIndexChanged;
					base.Controls.Remove(multiOptionButton);
				}
			}
		}

		private void InitializeComponent()
		{
			GridLayout gridLayout = new GridLayout();
			GridLayout layout = new GridLayout();
			GridLayout otherLayout = new GridLayout();
			GridLayout nextLayout = new GridLayout();
			_lblColumn = new Label();
			_jcboColumn = new MxMultiColumnComboBox();
			_jcboOperator = new MxMultiColumnComboBox();
			_cboFirstValueColumn = new MxMultiColumnComboBox();
			_cboSecondValueColumn = new MxMultiColumnComboBox();
			_txtFirstValue = new TextBox();
			_lblAnd = new Label();
			_txtSecondValue = new TextBox();
			_optFirstValueSource = new MultiOptionButton();
			_optSecondValueSource = new MultiOptionButton();
			_moptNot = new MultiOptionButton();
			SuspendLayout();
			_lblColumn.AutoSize = true;
			_lblColumn.Enabled = false;
			_lblColumn.Location = new Point(48, 5);
			_lblColumn.Name = "lblColumn";
			_lblColumn.Size = new Size(46, 14);
			_lblColumn.TabIndex = 0;
			_lblColumn.Text = "Column:";
			_lblColumn.MouseDown += OnLblAndMouseDown;
			_jcboColumn.AutoSize = false;
			_jcboColumn.BackColor = SystemColors.Window;
			_jcboColumn.BorderStyle = Mordex.Windows.DataGrid.BorderStyle.Flat;
			_jcboColumn.ComboStyle = ComboStyle.DropDownList;
			_jcboColumn.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			gridLayout.LayoutString = "<GridLayoutData><RootTable><Columns Collection=\"true\"><Column0 ID=\"Name\"><DataMember>Text</DataMember><Key>Name</Key><Position>0</Position></Column0></Columns><GroupCondition ID=\"\" /></RootTable><WatermarkImage /><BorderStyle>Flat</BorderStyle><ThemedAreas>None</ThemedAreas><Font>Tahoma, 8.25pt</Font><ControlStyle><ButtonAppearance>Flat</ButtonAppearance></ControlStyle><AllowEdit>False</AllowEdit><ColumnHeaders>False</ColumnHeaders><ExpandableGroups>False</ExpandableGroups><GridLines>None</GridLines><GroupByBoxVisible>False</GroupByBoxVisible><HideSelection>Highlight</HideSelection><DisplayMember>Text</DisplayMember><ValueMember>Value</ValueMember></GridLayoutData>";
			_jcboColumn.DesignTimeLayout = gridLayout;
			_jcboColumn.DisplayMember = "Text";
			_jcboColumn.Enabled = false;
			_jcboColumn.FlatBorderColor = SystemColors.ControlDark;
			_jcboColumn.ForeColor = SystemColors.WindowText;
			_jcboColumn.ImageVerticalAlignment = ImageVerticalAlignment.Center;
			_jcboColumn.Location = new Point(96, 2);
			_jcboColumn.Name = "jcboColumn";
			_jcboColumn.Size = new Size(120, 20);
			_jcboColumn.TabIndex = 1;
			_jcboColumn.ValueMember = "Value";
			_jcboColumn.DropDown += OnJcboColumnValue2DropDown;
			_jcboColumn.ValueChanged += OnJcboColumnValue2ValueChanged;
			_jcboOperator.AutoSize = false;
			_jcboOperator.BackColor = SystemColors.Window;
			_jcboOperator.BorderStyle = Mordex.Windows.DataGrid.BorderStyle.Flat;
			_jcboOperator.ComboStyle = ComboStyle.DropDownList;
			_jcboOperator.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			layout.LayoutString = "<GridLayoutData><RootTable><Columns Collection=\"true\"><Column0 ID=\"Name\"><DataMember>Text</DataMember><Key>Name</Key><Position>0</Position></Column0></Columns><GroupCondition ID=\"\" /></RootTable><WatermarkImage /><BorderStyle>Flat</BorderStyle><ThemedAreas>None</ThemedAreas><Font>Tahoma, 8.25pt</Font><ControlStyle><ButtonAppearance>Flat</ButtonAppearance></ControlStyle><AllowEdit>False</AllowEdit><ColumnHeaders>False</ColumnHeaders><ExpandableGroups>False</ExpandableGroups><GridLines>None</GridLines><GroupByBoxVisible>False</GroupByBoxVisible><HideSelection>Highlight</HideSelection><DisplayMember>Text</DisplayMember><ValueMember>Value</ValueMember></GridLayoutData>";
			_jcboOperator.DesignTimeLayout = layout;
			_jcboOperator.DisplayMember = "Text";
			_jcboOperator.Enabled = false;
			_jcboOperator.FlatBorderColor = SystemColors.ControlDark;
			_jcboOperator.ForeColor = SystemColors.WindowText;
			_jcboOperator.ImageVerticalAlignment = ImageVerticalAlignment.Center;
			_jcboOperator.Location = new Point(218, 2);
			_jcboOperator.Name = "jcboOperator";
			_jcboOperator.Size = new Size(104, 20);
			_jcboOperator.TabIndex = 2;
			_jcboOperator.ValueMember = "Value";
			_jcboOperator.DropDown += OnJcboColumnValue2DropDown;
			_jcboOperator.ValueChanged += OnJcboOperatorValueChanged;
			_cboFirstValueColumn.AutoSize = false;
			_cboFirstValueColumn.BackColor = SystemColors.Window;
			_cboFirstValueColumn.BorderStyle = Mordex.Windows.DataGrid.BorderStyle.Flat;
			_cboFirstValueColumn.ComboStyle = ComboStyle.DropDownList;
			_cboFirstValueColumn.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			otherLayout.LayoutString = "<GridLayoutData><RootTable><Columns Collection=\"true\"><Column0 ID=\"Name\"><DataMember>Text</DataMember><Key>Name</Key><Position>0</Position></Column0></Columns><GroupCondition ID=\"\" /></RootTable><WatermarkImage /><BorderStyle>Flat</BorderStyle><ThemedAreas>None</ThemedAreas><Font>Tahoma, 8.25pt</Font><ControlStyle><ButtonAppearance>Flat</ButtonAppearance></ControlStyle><AllowEdit>False</AllowEdit><ColumnHeaders>False</ColumnHeaders><ExpandableGroups>False</ExpandableGroups><GridLines>None</GridLines><GroupByBoxVisible>False</GroupByBoxVisible><HideSelection>Highlight</HideSelection><DisplayMember>Text</DisplayMember><ValueMember>Value</ValueMember></GridLayoutData>";
			_cboFirstValueColumn.DesignTimeLayout = otherLayout;
			_cboFirstValueColumn.DisplayMember = "Text";
			_cboFirstValueColumn.Enabled = false;
			_cboFirstValueColumn.FlatBorderColor = SystemColors.ControlDark;
			_cboFirstValueColumn.ForeColor = SystemColors.WindowText;
			_cboFirstValueColumn.ImageVerticalAlignment = ImageVerticalAlignment.Center;
			_cboFirstValueColumn.Location = new Point(376, 2);
			_cboFirstValueColumn.Name = "jcboColumnValue1";
			_cboFirstValueColumn.Size = new Size(120, 20);
			_cboFirstValueColumn.TabIndex = 16;
			_cboFirstValueColumn.ValueMember = "Value";
			_cboFirstValueColumn.Visible = false;
			_cboFirstValueColumn.DropDown += OnJcboColumnValue2DropDown;
			_cboFirstValueColumn.ValueChanged += OnJcboColumnValue2ValueChanged;
			_cboSecondValueColumn.AutoSize = false;
			_cboSecondValueColumn.BackColor = SystemColors.Window;
			_cboSecondValueColumn.BorderStyle = Mordex.Windows.DataGrid.BorderStyle.Flat;
			_cboSecondValueColumn.ComboStyle = ComboStyle.DropDownList;
			_cboSecondValueColumn.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			nextLayout.LayoutString = "<GridLayoutData><RootTable><Columns Collection=\"true\"><Column0 ID=\"Name\"><DataMember>Text</DataMember><Key>Name</Key><Position>0</Position></Column0></Columns><GroupCondition ID=\"\" /></RootTable><WatermarkImage /><BorderStyle>Flat</BorderStyle><ThemedAreas>None</ThemedAreas><Font>Tahoma, 8.25pt</Font><ControlStyle><ButtonAppearance>Flat</ButtonAppearance></ControlStyle><AllowEdit>False</AllowEdit><ColumnHeaders>False</ColumnHeaders><ExpandableGroups>False</ExpandableGroups><GridLines>None</GridLines><GroupByBoxVisible>False</GroupByBoxVisible><HideSelection>Highlight</HideSelection><DisplayMember>Text</DisplayMember><ValueMember>Value</ValueMember></GridLayoutData>";
			_cboSecondValueColumn.DesignTimeLayout = nextLayout;
			_cboSecondValueColumn.DisplayMember = "Text";
			_cboSecondValueColumn.Enabled = false;
			_cboSecondValueColumn.FlatBorderColor = SystemColors.ControlDark;
			_cboSecondValueColumn.ForeColor = SystemColors.WindowText;
			_cboSecondValueColumn.ImageVerticalAlignment = ImageVerticalAlignment.Center;
			_cboSecondValueColumn.Location = new Point(570, 2);
			_cboSecondValueColumn.Name = "jcboColumnValue2";
			_cboSecondValueColumn.Size = new Size(120, 20);
			_cboSecondValueColumn.TabIndex = 17;
			_cboSecondValueColumn.ValueMember = "Value";
			_cboSecondValueColumn.Visible = false;
			_cboSecondValueColumn.DropDown += OnJcboColumnValue2DropDown;
			_cboSecondValueColumn.ValueChanged += OnJcboColumnValue2ValueChanged;
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
			_optFirstValueSource.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			_optFirstValueSource.Enabled = false;
			_optFirstValueSource.Font = new Font("Tahoma", 6.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_optFirstValueSource.Location = new Point(324, 2);
			_optFirstValueSource.Name = "moptValue1";
			_optFirstValueSource.SelectedIndex = -1;
			_optFirstValueSource.SelectedValue = null;
			_optFirstValueSource.Size = new Size(50, 20);
			_optFirstValueSource.TabIndex = 25;
			_optFirstValueSource.Text = "multiOptionButton1";
			_optFirstValueSource.ThreeState = false;
			_optFirstValueSource.Values = null;
			_optFirstValueSource.SelectedIndexChanged += OnMoptValue1SelectedIndexChanged;
			_optSecondValueSource.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			_optSecondValueSource.Enabled = false;
			_optSecondValueSource.Font = new Font("Tahoma", 6.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_optSecondValueSource.Location = new Point(520, 2);
			_optSecondValueSource.Name = "moptValue2";
			_optSecondValueSource.SelectedIndex = -1;
			_optSecondValueSource.SelectedValue = null;
			_optSecondValueSource.Size = new Size(50, 20);
			_optSecondValueSource.TabIndex = 26;
			_optSecondValueSource.Text = "multiOptionButton1";
			_optSecondValueSource.ThreeState = false;
			_optSecondValueSource.Values = null;
			_optSecondValueSource.Visible = false;
			_optSecondValueSource.SelectedIndexChanged += OnMoptValue2SelectedIndexChanged;
			_moptNot.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			_moptNot.Location = new Point(4, 2);
			_moptNot.Name = "moptNot";
			_moptNot.SelectedIndex = -1;
			_moptNot.SelectedValue = null;
			_moptNot.Size = new Size(34, 20);
			_moptNot.TabIndex = 27;
			_moptNot.Text = "multiOptionButton1";
			_moptNot.ThreeState = false;
			_moptNot.Values = null;
			_moptNot.SelectedIndexChanged += OnMoptNotSelectedIndexChanged;
			BackColor = SystemColors.Window;
			base.Controls.AddRange(new Control[11]
			{
				_moptNot, _optSecondValueSource, _optFirstValueSource, _lblAnd, _jcboOperator, _jcboColumn, _lblColumn, _txtFirstValue, _cboFirstValueColumn, _txtSecondValue,
				_cboSecondValueColumn
			});
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "SimpleFilter";
			base.Size = new Size(760, 24);
			ResumeLayout(performLayout: false);
		}

		private void OnJcboOperatorValueChanged(object sender, EventArgs eventArgs)
		{
			if (_gridFilterCondition != null)
			{
				_gridFilterCondition.ConditionOperator = (ConditionOperator)_jcboOperator.Value;
			}
			bool visible = false;
			if (_jcboOperator.Value != null && ((ConditionOperator)_jcboOperator.Value == ConditionOperator.Between || (ConditionOperator)_jcboOperator.Value == ConditionOperator.NotBetween))
			{
				visible = true;
			}
			_lblAnd.Visible = visible;
			_optSecondValueSource.Visible = visible;
			_txtSecondValue.Visible = visible && _optSecondValueSource.SelectedIndex == 0;
			_cboSecondValueColumn.Visible = visible && _optSecondValueSource.SelectedIndex == 1;
			LayoutControls();
			RaiseFilterChangedIfEnabled();
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			LayoutControls();
		}

		protected override void OnParentChanged(EventArgs eventArgs)
		{
			base.OnParentChanged(eventArgs);
			LayoutControls();
		}

		private void LayoutControls()
		{
			_ = DateTime.Now.Ticks;
			if (_gridFilterCondition != null)
			{
				_gridFilterCondition.GetParentCollection();
			}
			bool condition = _gridFilterCondition != null && !_gridFilterCondition.IsComposite;
			int top = 0;
			if (_filterConditionEditorControl2Boolean)
			{
				top = 2;
				_lblColumn.Top = top + (20 - _lblColumn.Height) / 2;
				_lblAnd.Top = _lblColumn.Top;
				_jcboColumn.Top = top;
				_jcboOperator.Top = top;
				_optFirstValueSource.Top = top;
				_cboFirstValueColumn.Top = top;
				_cboSecondValueColumn.Top = top;
				_optSecondValueSource.Top = top;
			}
			Rectangle bounds = _cboFirstValueColumn.Bounds;
			bounds.Inflate(-3, -2);
			_txtFirstValue.Bounds = bounds;
			bounds = _cboSecondValueColumn.Bounds;
			bounds.Inflate(-3, -2);
			_txtSecondValue.Bounds = bounds;
			int width = 0;
			ConditionOperator conditionOperator = ConditionOperator.Equal;
			if (_jcboOperator.Value != null)
			{
				conditionOperator = (ConditionOperator)_jcboOperator.Value;
			}
			width = ((_jcboOperator.Value == null || (conditionOperator != ConditionOperator.Between && conditionOperator != ConditionOperator.NotBetween)) ? (_cboFirstValueColumn.Bounds.Right + 2) : (_cboSecondValueColumn.Bounds.Right + 2));
			int height = 24;
			_lastLayoutSize = new Size(width, height);
			int value = 2;
			if (!condition)
			{
				int right = 0;
				int bottom = 0;
				foreach (Control control in base.Controls)
				{
					if (control is FilterConditionEditorPanel)
					{
						if (control.Bounds.Right > right)
						{
							right = control.Bounds.Right;
						}
						if (control.Bounds.Bottom > bottom)
						{
							bottom = control.Bounds.Bottom;
						}
					}
				}
				if (_lastLayoutSize.Width < right + 4)
				{
					_lastLayoutSize.Width = right + 4;
				}
				if (_lastLayoutSize.Height < bottom + 4)
				{
					_lastLayoutSize.Height = bottom + 2;
				}
				if (!(base.Parent is FilterConditionEditorPanel))
				{
					value = 4;
					_lastLayoutSize.Height += 2;
				}
			}
			base.Size = _lastLayoutSize;
			top = 2;
			if (_gridFilterCondition != null && _gridFilterCondition.IsComposite)
			{
				_moptNot.Top = top + (_lastLayoutSize.Height - _moptNot.Height - top - value) / 2;
			}
			else
			{
				_moptNot.Top = top;
			}
			Invalidate();
		}

		private void RaiseFilterChangedIfEnabled()
		{
			if (_canRaiseFilterChanged)
			{
				OnFilterChanged(EventArgs.Empty);
			}
		}

		public void AddFilterChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _filterChangedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _filterChangedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		public void RemoveFilterChangedHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _filterChangedHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _filterChangedHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected virtual void OnFilterChanged(EventArgs eventArgs)
		{
			if (_filterChangedHandler != null)
			{
				_filterChangedHandler(this, eventArgs);
			}
		}

		private void OnJcboColumnValue2ValueChanged(object sender, EventArgs eventArgs)
		{
			if (_gridFilterCondition != null)
			{
				if (sender == _jcboColumn)
				{
					_gridFilterCondition.Column = (GridColumn)_jcboColumn.Value;
				}
				else if (sender == _cboFirstValueColumn)
				{
					_gridFilterCondition.ValueColumn1 = (GridColumn)_cboFirstValueColumn.Value;
				}
				else if (sender == _cboSecondValueColumn)
				{
					_gridFilterCondition.ValueColumn2 = (GridColumn)_cboSecondValueColumn.Value;
				}
			}
			RaiseFilterChangedIfEnabled();
		}

		private void OnTxtValue2TextChanged(object sender, EventArgs eventArgs)
		{
			if (_gridFilterCondition != null)
			{
				string text = ((TextBox)sender).Text;
				object item = null;
				if (text != "")
				{
					item = ((!(text == string.Concat('"', '"'))) ? text : "");
				}
				if (sender == _txtFirstValue)
				{
					_gridFilterCondition.Value1 = item;
				}
				else
				{
					_gridFilterCondition.Value2 = item;
				}
			}
			RaiseFilterChangedIfEnabled();
		}

		private void RenderContent(Graphics graphics, int x2, int y1, int value, Pen pen)
		{
			graphics.DrawLine(pen, x2 + 3, y1, x2, y1 + 3);
			graphics.DrawLine(pen, x2, y1 + 3, x2, y1 + 3 + (value - 12) / 2);
			graphics.DrawLine(pen, x2, y1 + (value - 12) / 2 + 3, x2 - 3, y1 + (value - 12) / 2 + 6);
			graphics.DrawLine(pen, x2 - 3, y1 + (value - 12) / 2 + 6, x2, y1 + (value - 12) / 2 + 9);
			graphics.DrawLine(pen, x2, y1 + (value - 12) / 2 + 9, x2, y1 + value - 3);
			graphics.DrawLine(pen, x2, y1 + value - 3, x2 + 3, y1 + value);
		}

		private void OnJcboColumnValue2DropDown(object sender, EventArgs eventArgs)
		{
			((MxMultiColumnComboBox)sender).DropDownList.AutoSizeColumns();
		}

		internal void UpdateFilterConditionEditorControl2Boolean(bool filterConditionEditorControl2Boolean)
		{
			if (_filterConditionEditorControl2Boolean != filterConditionEditorControl2Boolean)
			{
				_filterConditionEditorControl2Boolean = filterConditionEditorControl2Boolean;
				_lblColumn.Visible = _filterConditionEditorControl2Boolean;
				_jcboColumn.Visible = _filterConditionEditorControl2Boolean;
				_jcboOperator.Visible = _filterConditionEditorControl2Boolean;
				_txtFirstValue.Visible = _filterConditionEditorControl2Boolean && _optFirstValueSource.SelectedIndex == 0;
				_cboFirstValueColumn.Visible = _filterConditionEditorControl2Boolean && _optFirstValueSource.SelectedIndex == 1;
				_optFirstValueSource.Visible = _filterConditionEditorControl2Boolean;
				ConditionOperator conditionOperator = ConditionOperator.Equal;
				if (_jcboOperator.Value != null)
				{
					conditionOperator = (ConditionOperator)_jcboOperator.Value;
				}
				bool visible = conditionOperator == ConditionOperator.Between || conditionOperator == ConditionOperator.NotBetween;
				visible &= _filterConditionEditorControl2Boolean;
				_optSecondValueSource.Visible = visible;
				_txtSecondValue.Visible = visible;
				_cboSecondValueColumn.Visible = visible;
			}
		}

		private void OnChildFilterChanged(object sender, EventArgs eventArgs)
		{
			LayoutControls();
			OnFilterChanged(eventArgs);
		}

		private void ClearActiveState()
		{
			_isActive = false;
			foreach (Control control in base.Controls)
			{
				if (control is FilterConditionEditorPanel)
				{
					((FilterConditionEditorPanel)control).ClearActiveState();
				}
			}
			Invalidate();
		}

		private FilterConditionEditorPanel GetRootEditor()
		{
			if (base.Parent is FilterConditionEditorPanel)
			{
				return ((FilterConditionEditorPanel)base.Parent).GetRootEditor();
			}
			return this;
		}

		internal bool IsActive()
		{
			return _isActive;
		}

		internal void SetActive(bool filterConditionEditorControl2Boolean2)
		{
			if (_isActive != filterConditionEditorControl2Boolean2)
			{
				GetRootEditor().ClearActiveState();
				_isActive = filterConditionEditorControl2Boolean2;
				if (_isActive)
				{
					OnSelectedFilterConditionChanged(EventArgs.Empty);
				}
				Invalidate(invalidateChildren: true);
			}
		}

		protected override void OnEnter(EventArgs eventArgs)
		{
			base.OnEnter(eventArgs);
			if (FindFocusedControl() == this)
			{
				SetActive(true);
				Control parent = base.Parent;
				if (parent is FilterConditionEditorPanel)
				{
					((FilterConditionEditorPanel)parent).ActivateEditor(this);
				}
				else
				{
					ActivateEditor(this);
				}
				Invalidate();
			}
		}

		protected override void OnLeave(EventArgs eventArgs)
		{
			base.OnLeave(eventArgs);
			if (IsActive())
			{
				Control parent = base.Parent;
				while (parent is FilterConditionEditorPanel)
				{
					if (parent.ContainsFocus)
					{
						((FilterConditionEditorPanel)parent).OnEnter(EventArgs.Empty);
						break;
					}
					parent = parent.Parent;
				}
			}
			Invalidate();
		}

		private Control FindFocusedControl()
		{
			foreach (Control control in base.Controls)
			{
				if (control.ContainsFocus && control is FilterConditionEditorPanel)
				{
					return ((FilterConditionEditorPanel)control).FindFocusedControl();
				}
			}
			if (base.ContainsFocus)
			{
				return this;
			}
			return null;
		}

		internal FilterConditionEditorPanel FindActiveEditor()
		{
			if (IsActive())
			{
				return this;
			}
			foreach (Control control in base.Controls)
			{
				if (control is FilterConditionEditorPanel)
				{
					FilterConditionEditorPanel activeEditor = ((FilterConditionEditorPanel)control).FindActiveEditor();
					if (activeEditor != null)
					{
						return activeEditor;
					}
				}
			}
			return null;
		}

		internal void ActivateEditor(FilterConditionEditorPanel filterConditionEditorControl)
		{
			filterConditionEditorControl?.SetActive(true);
		}

		public void AddSelectionChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _filterConditionEditorControl2EventHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _filterConditionEditorControl2EventHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		protected virtual void OnSelectedFilterConditionChanged(EventArgs eventArgs)
		{
			if (_filterConditionEditorControl2EventHandler != null)
			{
				_filterConditionEditorControl2EventHandler(this, eventArgs);
			}
		}

		private void OnControlSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_gridFilterCondition != null)
			{
				MultiOptionButton multiOptionButton = (MultiOptionButton)sender;
				GridFilterCondition gridFilterCondition = (GridFilterCondition)multiOptionButton.Tag;
				gridFilterCondition.LogicalOperator = (LogicalOperator)multiOptionButton.SelectedValue;
				RaiseFilterChangedIfEnabled();
			}
		}

		private MultiOptionButton CreateLogicalOperatorButton(GridFilterCondition gridFilterCondition)
		{
			MultiOptionButton multiOptionButton = new MultiOptionButton();
			multiOptionButton.ThreeState = true;
			multiOptionButton.Values = new object[3]
			{
				LogicalOperator.And,
				LogicalOperator.Or,
				LogicalOperator.Xor
			};
			multiOptionButton.SelectedValue = gridFilterCondition.LogicalOperator;
			multiOptionButton.Tag = gridFilterCondition;
			multiOptionButton.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			multiOptionButton.Font = _optFirstValueSource.Font;
			multiOptionButton.Size = new Size(36, 20);
			multiOptionButton.GotFocus += OnControlGotFocus;
			multiOptionButton.SelectedIndexChanged += OnControlSelectedIndexChanged;
			return multiOptionButton;
		}

		private void OnMoptValue1SelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_txtFirstValue.Visible = _optFirstValueSource.SelectedIndex == 0;
			_cboFirstValueColumn.Visible = _optFirstValueSource.SelectedIndex == 1;
			if (!_cboFirstValueColumn.Visible)
			{
				_cboFirstValueColumn.Value = null;
			}
			else
			{
				_txtFirstValue.Text = "";
			}
		}

		private void OnMoptNotSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_gridFilterCondition != null)
			{
				_gridFilterCondition.Negation = _moptNot.SelectedIndex == 1;
				RaiseFilterChangedIfEnabled();
			}
		}

		private void OnMoptValue2SelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_txtSecondValue.Visible = _optSecondValueSource.SelectedIndex == 0;
			_cboSecondValueColumn.Visible = _optSecondValueSource.SelectedIndex == 1;
			if (!_cboSecondValueColumn.Visible)
			{
				_cboSecondValueColumn.Value = null;
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

		private void HandleActiveControlChanged(object sender, EventArgs eventArgs)
		{
			OnActiveControlChanged(eventArgs);
		}

		public void AddActiveControlChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _activeControlChangedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _activeControlChangedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		protected virtual void OnActiveControlChanged(EventArgs eventArgs)
		{
			if (_activeControlChangedHandler != null)
			{
				_activeControlChangedHandler(this, eventArgs);
			}
		}

		internal GridFilterCondition GetActiveCondition()
		{
			if (FindActiveEditor() == null)
			{
				return _gridFilterCondition;
			}
			return FindActiveEditor().GetCondition();
		}

		internal void ActivateEditorForCondition(GridFilterCondition gridFilterCondition)
		{
			if (_gridFilterCondition == gridFilterCondition)
			{
				SetActive(true);
				return;
			}
			foreach (Control control in base.Controls)
			{
				if (control is FilterConditionEditorPanel)
				{
					FilterConditionEditorPanel otherControl = (FilterConditionEditorPanel)control;
					otherControl.ActivateEditorForCondition(gridFilterCondition);
					if (otherControl.IsActive())
					{
						break;
					}
				}
			}
		}
	}
}
