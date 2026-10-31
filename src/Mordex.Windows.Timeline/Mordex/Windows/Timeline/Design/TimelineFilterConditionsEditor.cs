using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design
{
	internal class TimelineFilterConditionsEditor : UserControl
	{
		private Splitter _splitter;

		private Panel _panel;

		private TimelineScrollableBase _filterBuilder;

		private Button _btnClear;

		private Button _btnDelete;

		private Button _btnAddInner;

		private Button _btnAdd;

		private TimelineFilterConditionControl _filterConditionControl;

		private Container _container = new Container();

		internal TimelineFilterConditionsEditor()
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

		public TimelineFilterCondition GetTimelineFilterCondition()
		{
			return _filterBuilder.GetFilterCondition();
		}

		public void SetTimelineFilterCondition(TimelineFilterCondition timelineFilterCondition)
		{
			_filterBuilder.SetFilterCondition(timelineFilterCondition);
			UpdateFilterState(EventArgs.Empty);
		}

		public void SetTimeline(MxTimeline timeline)
		{
			_filterBuilder.SetTimeline(timeline);
		}

		private void InitializeControls()
		{
			_filterConditionControl = new TimelineFilterConditionControl();
			_splitter = new Splitter();
			_panel = new Panel();
			_filterBuilder = new TimelineScrollableBase();
			_btnClear = new Button();
			_btnDelete = new Button();
			_btnAddInner = new Button();
			_btnAdd = new Button();
			_panel.SuspendLayout();
			SuspendLayout();
			_filterConditionControl.Dock = DockStyle.Top;
			_filterConditionControl.UpdateTimelineFilterCondition(null);
			_filterConditionControl.ForeColor = SystemColors.WindowText;
			_filterConditionControl.Location = new Point(1, 1);
			_filterConditionControl.Name = "filterText";
			_filterConditionControl.UpdateSelectedCondition(null);
			_filterConditionControl.Size = new Size(573, 39);
			_filterConditionControl.TabIndex = 0;
			_filterConditionControl.Text = "filterTextIndicator1";
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
			_filterBuilder.BorderStyle = Mordex.Windows.Timeline.BorderStyle.Flat;
			_filterBuilder.SetFilterCondition(null);
			_filterBuilder.FlatBorderColor = SystemColors.ControlDark;
			_filterBuilder.Location = new Point(4, 27);
			_filterBuilder.Name = "filterBuilder";
			_filterBuilder.SelectFilterCondition(null);
			_filterBuilder.Size = new Size(564, 328);
			_filterBuilder.TabIndex = 11;
			_filterBuilder.SetTimeline(null);
			_filterBuilder.Text = "timelineCompositeFilter1";
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
			base.Controls.AddRange(new Control[3] { _panel, _splitter, _filterConditionControl });
			base.DockPadding.All = 1;
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "TimelineFilterConditionsEditor";
			base.Size = new Size(575, 405);
			_panel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		private void OnFilterBuilderChanged(object sender, EventArgs eventArgs)
		{
			UpdateFilterState(eventArgs);
		}

		private void UpdateFilterState(EventArgs eventArgs)
		{
			_filterConditionControl.UpdateTimelineFilterCondition(GetTimelineFilterCondition());
			if (GetTimelineFilterCondition() != null)
			{
				_btnClear.Enabled = !GetTimelineFilterCondition().IsEmpty() || GetTimelineFilterCondition().IsComposite;
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
			TimelineFilterCondition timelineFilterCondition = GetTimelineFilterCondition();
			timelineFilterCondition.Clear();
			_filterBuilder.SetFilterCondition(timelineFilterCondition);
		}

		private void OnSelectedConditionChanged(object sender, EventArgs eventArgs)
		{
			TimelineFilterCondition timelineFilterCondition = _filterBuilder.GetSelectedFilterCondition();
			_filterConditionControl.UpdateSelectedCondition(timelineFilterCondition);
			if (timelineFilterCondition == null)
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
				_filterBuilder.SetFilterCondition(new TimelineFilterCondition());
				return;
			}
			TimelineFilterCondition timelineFilterCondition = _filterBuilder.GetFilterCondition();
			TimelineFilterCondition newCondition = new TimelineFilterCondition();
			timelineFilterCondition.AddCondition(newCondition);
			_filterBuilder.SetFilterCondition(timelineFilterCondition);
			_filterBuilder.SelectFilterCondition(newCondition);
		}

		private void OnBtnAddInnerClick(object sender, EventArgs eventArgs)
		{
			if (_filterBuilder.GetFilterCondition() != null)
			{
				TimelineFilterCondition timelineFilterCondition = _filterBuilder.GetSelectedFilterCondition();
				TimelineFilterCondition newCondition = new TimelineFilterCondition();
				timelineFilterCondition.AddCondition(newCondition);
				_filterBuilder.SetFilterCondition(_filterBuilder.GetFilterCondition());
				_filterBuilder.SelectFilterCondition(newCondition);
			}
		}

		private void OnBtnDeleteClick(object sender, EventArgs eventArgs)
		{
			if (_filterBuilder.GetFilterCondition() != null)
			{
				TimelineFilterCondition timelineFilterCondition = _filterBuilder.GetSelectedFilterCondition();
				if (timelineFilterCondition == _filterBuilder.GetFilterCondition())
				{
					_filterBuilder.SetFilterCondition(null);
					return;
				}
				timelineFilterCondition.GetOwnerCondition().RemoveCondition(timelineFilterCondition);
				_filterBuilder.SetFilterCondition(_filterBuilder.GetFilterCondition());
			}
		}
	}
}
