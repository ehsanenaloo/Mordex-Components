using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class FilterConditionsEditor : UserControl
	{
		private Splitter _splitter;

		private Panel _panel;

		private GridScrollableBase _filterBuilder;

		private Button _btnClear;

		private Button _btnDelete;

		private Button _btnAddInner;

		private Button _btnAdd;

		private FilterConditionEditorControl _filterPreview;

		private Container _container = new Container();

		internal FilterConditionsEditor()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		public GridFilterCondition GetFilterCondition()
		{
			return _filterBuilder.GetFilterCondition();
		}

		public void SetFilterCondition(GridFilterCondition gridFilterCondition)
		{
			_filterBuilder.SetFilterCondition(gridFilterCondition);
			ProcessEventArgs(EventArgs.Empty);
		}

		public void SetTable(GridTable gridTable)
		{
			_filterBuilder.AssignTable(gridTable);
		}

		private void InitializeControls()
		{
			_filterPreview = new FilterConditionEditorControl();
			_splitter = new Splitter();
			_panel = new Panel();
			_filterBuilder = new GridScrollableBase();
			_btnClear = new Button();
			_btnDelete = new Button();
			_btnAddInner = new Button();
			_btnAdd = new Button();
			_panel.SuspendLayout();
			SuspendLayout();
			_filterPreview.Dock = DockStyle.Top;
			_filterPreview.SetFilterCondition(null);
			_filterPreview.ForeColor = SystemColors.WindowText;
			_filterPreview.Location = new Point(1, 1);
			_filterPreview.Name = "filterText";
			_filterPreview.UpdateFilterConditionEditorControlGridFilterCondition(null);
			_filterPreview.Size = new Size(573, 39);
			_filterPreview.TabIndex = 0;
			_filterPreview.Text = "filterTextIndicator1";
			_splitter.Dock = DockStyle.Top;
			_splitter.Location = new Point(1, 40);
			_splitter.Name = "splitter1";
			_splitter.Size = new Size(573, 4);
			_splitter.TabIndex = 1;
			_splitter.TabStop = false;
			_panel.Controls.AddRange(new Control[5] { _filterBuilder, _btnClear, _btnDelete, _btnAddInner, _btnAdd });
			_panel.Dock = DockStyle.Fill;
			_panel.Location = new Point(1, 44);
			_panel.Name = "panel1";
			_panel.Size = new Size(573, 360);
			_panel.TabIndex = 7;
			_filterBuilder.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_filterBuilder.BorderStyle = Mordex.Windows.DataGrid.BorderStyle.Flat;
			_filterBuilder.SetFilterCondition(null);
			_filterBuilder.FlatBorderColor = SystemColors.ControlDark;
			_filterBuilder.Location = new Point(4, 27);
			_filterBuilder.Name = "filterBuilder";
			_filterBuilder.ActivateAndShowCondition(null);
			_filterBuilder.Size = new Size(564, 328);
			_filterBuilder.TabIndex = 11;
			_filterBuilder.AssignTable(null);
			_filterBuilder.Text = "gridCompositeFilter1";
			_filterBuilder.AddSelectionChangedHandler(OnBuilderSelectionChanged);
			_filterBuilder.AddConditionChangedHandler(OnBuilderConditionChanged);
			_btnClear.FlatStyle = FlatStyle.Popup;
			_btnClear.Location = new Point(376, 0);
			_btnClear.Name = "btnClear";
			_btnClear.Size = new Size(120, 24);
			_btnClear.TabIndex = 10;
			_btnClear.Text = "Clear";
			_btnClear.Click += OnBtnClearClick;
			_btnDelete.FlatStyle = FlatStyle.Popup;
			_btnDelete.Location = new Point(252, 0);
			_btnDelete.Name = "btnDelete";
			_btnDelete.Size = new Size(120, 24);
			_btnDelete.TabIndex = 8;
			_btnDelete.Text = "Delete Condition";
			_btnDelete.Click += OnBtnDeleteClick;
			_btnAddInner.FlatStyle = FlatStyle.Popup;
			_btnAddInner.Location = new Point(128, 0);
			_btnAddInner.Name = "btnAddInner";
			_btnAddInner.Size = new Size(120, 24);
			_btnAddInner.TabIndex = 7;
			_btnAddInner.Text = "Add Inner Condition";
			_btnAddInner.Click += OnBtnAddInnerClick;
			_btnAdd.FlatStyle = FlatStyle.Popup;
			_btnAdd.Location = new Point(4, 0);
			_btnAdd.Name = "btnAdd";
			_btnAdd.Size = new Size(120, 24);
			_btnAdd.TabIndex = 6;
			_btnAdd.Text = "Add Condition";
			_btnAdd.Click += OnBtnAddClick;
			base.Controls.AddRange(new Control[3] { _panel, _splitter, _filterPreview });
			base.DockPadding.All = 1;
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "GridFilterConditionsEditor";
			base.Size = new Size(575, 405);
			_panel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		private void OnBuilderConditionChanged(object sender, EventArgs eventArgs)
		{
			ProcessEventArgs(eventArgs);
		}

		private void ProcessEventArgs(EventArgs eventArgs)
		{
			_filterPreview.SetFilterCondition(GetFilterCondition());
			if (GetFilterCondition() != null)
			{
				_btnClear.Enabled = !GetFilterCondition().IsEmptyCondition() || GetFilterCondition().IsComposite;
			}
			else
			{
				_btnClear.Enabled = false;
			}
			OnBuilderSelectionChanged(null, eventArgs);
			OnFilterChanged(eventArgs);
		}

		protected virtual void OnFilterChanged(EventArgs eventArgs)
		{
		}

		private void OnBtnClearClick(object sender, EventArgs eventArgs)
		{
			GridFilterCondition gridFilterCondition = GetFilterCondition();
			gridFilterCondition.Clear();
			_filterBuilder.SetFilterCondition(gridFilterCondition);
		}

		private void OnBuilderSelectionChanged(object sender, EventArgs eventArgs)
		{
			GridFilterCondition gridFilterCondition = _filterBuilder.GetActiveFilterCondition();
			_filterPreview.UpdateFilterConditionEditorControlGridFilterCondition(gridFilterCondition);
			if (gridFilterCondition == null)
			{
				_btnDelete.Enabled = false;
				_btnAdd.Enabled = true;
				_btnAddInner.Enabled = false;
			}
			else
			{
				_btnDelete.Enabled = true;
				_btnAddInner.Enabled = true;
				_btnAdd.Enabled = true;
			}
		}

		private void OnBtnAddClick(object sender, EventArgs eventArgs)
		{
			if (_filterBuilder.GetFilterCondition() == null)
			{
				_filterBuilder.SetFilterCondition(new GridFilterCondition());
				return;
			}
			GridFilterCondition gridFilterCondition = _filterBuilder.GetFilterCondition();
			GridFilterCondition filterCondition = new GridFilterCondition();
			gridFilterCondition.AddCondition(filterCondition);
			_filterBuilder.SetFilterCondition(gridFilterCondition);
			_filterBuilder.ActivateAndShowCondition(filterCondition);
		}

		private void OnBtnAddInnerClick(object sender, EventArgs eventArgs)
		{
			if (_filterBuilder.GetFilterCondition() != null)
			{
				GridFilterCondition gridFilterCondition = _filterBuilder.GetActiveFilterCondition();
				GridFilterCondition filterCondition = new GridFilterCondition();
				gridFilterCondition.AddCondition(filterCondition);
				_filterBuilder.SetFilterCondition(_filterBuilder.GetFilterCondition());
				_filterBuilder.ActivateAndShowCondition(filterCondition);
			}
		}

		private void OnBtnDeleteClick(object sender, EventArgs eventArgs)
		{
			if (_filterBuilder.GetFilterCondition() != null)
			{
				GridFilterCondition gridFilterCondition = _filterBuilder.GetActiveFilterCondition();
				if (gridFilterCondition == _filterBuilder.GetFilterCondition())
				{
					_filterBuilder.SetFilterCondition(null);
					return;
				}
				gridFilterCondition.GetParentCondition().RemoveCondition(gridFilterCondition);
				_filterBuilder.SetFilterCondition(_filterBuilder.GetFilterCondition());
			}
		}
	}
}
