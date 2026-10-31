using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.EditControls
{
	internal class SchedulerFilterConditionsEditor : UserControl
	{
		private Splitter _splitter;

		private Panel _panel;

		private SchedulerScrollableBase _filterBuilder;

		private Button _btnClear;

		private Button _btnDelete;

		private Button _btnAddInner;

		private Button _btnAdd;

		private SchedulerFilterConditionControl _filterSummary;

		private Container _container = new Container();

		internal SchedulerFilterConditionsEditor()
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

		public SchedulerFilterCondition GetFilterCondition()
		{
			return _filterBuilder.GetFilterCondition();
		}

		public void SetFilterCondition(SchedulerFilterCondition schedulerFilterCondition)
		{
			_filterBuilder.SetFilterCondition(schedulerFilterCondition);
			RefreshAfterFilterChange(EventArgs.Empty);
		}

		public void SetScheduler(MxScheduler scheduler)
		{
			_filterBuilder.SetScheduler(scheduler);
		}

		private void InitializeControls()
		{
			_filterSummary = new SchedulerFilterConditionControl();
			_splitter = new Splitter();
			_panel = new Panel();
			_filterBuilder = new SchedulerScrollableBase();
			_btnClear = new Button();
			_btnDelete = new Button();
			_btnAddInner = new Button();
			_btnAdd = new Button();
			_panel.SuspendLayout();
			SuspendLayout();
			_filterSummary.Dock = DockStyle.Top;
			_filterSummary.UpdateSchedulerFilterCondition(null);
			_filterSummary.ForeColor = SystemColors.WindowText;
			_filterSummary.Location = new Point(1, 1);
			_filterSummary.Name = "filterText";
			_filterSummary.UpdateSelectedCondition(null);
			_filterSummary.Size = new Size(573, 39);
			_filterSummary.TabIndex = 0;
			_filterSummary.Text = "filterTextIndicator1";
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
			_filterBuilder.BorderStyle = Mordex.Windows.Scheduler.BorderStyle.Flat;
			_filterBuilder.SetFilterCondition(null);
			_filterBuilder.FlatBorderColor = SystemColors.ControlDark;
			_filterBuilder.Location = new Point(4, 27);
			_filterBuilder.Name = "filterBuilder";
			_filterBuilder.SelectFilterCondition(null);
			_filterBuilder.Size = new Size(564, 328);
			_filterBuilder.TabIndex = 11;
			_filterBuilder.SetScheduler(null);
			_filterBuilder.Text = "SchedulerCompositeFilter1";
			_filterBuilder.AddSelectedFilterChangedHandler(OnSelectedConditionChanged);
			_filterBuilder.AddChangedHandler(OnFilterBuilderChanged);
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
			base.Controls.AddRange(new Control[3] { _panel, _splitter, _filterSummary });
			base.DockPadding.All = 1;
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "SchedulerFilterConditionsEditor";
			base.Size = new Size(575, 405);
			_panel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		private void OnFilterBuilderChanged(object sender, EventArgs eventArgs)
		{
			RefreshAfterFilterChange(eventArgs);
		}

		private void RefreshAfterFilterChange(EventArgs eventArgs)
		{
			_filterSummary.UpdateSchedulerFilterCondition(GetFilterCondition());
			if (GetFilterCondition() != null)
			{
				_btnClear.Enabled = !GetFilterCondition().IsEmptyCondition() || GetFilterCondition().IsComposite;
			}
			else
			{
				_btnClear.Enabled = false;
			}
			OnSelectedConditionChanged(null, eventArgs);
			OnFilterChanged(eventArgs);
		}

		protected virtual void OnFilterChanged(EventArgs eventArgs)
		{
		}

		private void OnBtnClearClick(object sender, EventArgs eventArgs)
		{
			SchedulerFilterCondition schedulerFilterCondition = GetFilterCondition();
			schedulerFilterCondition.Clear();
			_filterBuilder.SetFilterCondition(schedulerFilterCondition);
		}

		private void OnSelectedConditionChanged(object sender, EventArgs eventArgs)
		{
			SchedulerFilterCondition schedulerFilterCondition = _filterBuilder.GetSelectedFilterCondition();
			_filterSummary.UpdateSelectedCondition(schedulerFilterCondition);
			if (schedulerFilterCondition == null)
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
				_filterBuilder.SetFilterCondition(new SchedulerFilterCondition());
				return;
			}
			SchedulerFilterCondition schedulerFilterCondition = _filterBuilder.GetFilterCondition();
			SchedulerFilterCondition newCondition = new SchedulerFilterCondition();
			schedulerFilterCondition.AddCondition(newCondition);
			_filterBuilder.SetFilterCondition(schedulerFilterCondition);
			_filterBuilder.SelectFilterCondition(newCondition);
		}

		private void OnBtnAddInnerClick(object sender, EventArgs eventArgs)
		{
			if (_filterBuilder.GetFilterCondition() != null)
			{
				SchedulerFilterCondition schedulerFilterCondition = _filterBuilder.GetSelectedFilterCondition();
				SchedulerFilterCondition newCondition = new SchedulerFilterCondition();
				schedulerFilterCondition.AddCondition(newCondition);
				_filterBuilder.SetFilterCondition(_filterBuilder.GetFilterCondition());
				_filterBuilder.SelectFilterCondition(newCondition);
			}
		}

		private void OnBtnDeleteClick(object sender, EventArgs eventArgs)
		{
			if (_filterBuilder.GetFilterCondition() != null)
			{
				SchedulerFilterCondition schedulerFilterCondition = _filterBuilder.GetSelectedFilterCondition();
				if (schedulerFilterCondition == _filterBuilder.GetFilterCondition())
				{
					_filterBuilder.SetFilterCondition(null);
					return;
				}
				schedulerFilterCondition._owner.GetSchedulerFilterCondition().RemoveCondition(schedulerFilterCondition);
				_filterBuilder.SetFilterCondition(_filterBuilder.GetFilterCondition());
			}
		}
	}
}
