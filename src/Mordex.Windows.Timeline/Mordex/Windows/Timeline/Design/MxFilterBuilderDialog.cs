using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Timeline.Data;

namespace Mordex.Windows.Timeline.Design
{
	internal class MxFilterBuilderDialog : Form
	{
		private TimelineFilterConditionsEditor _filterConditionsEditor;

		private Button _btnOk;

		private Button _btnCancel;

		private Container _container = new Container();

		private IConditionOwner _iConditionOwner;

		internal MxFilterBuilderDialog()
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

		public TimelineFilterCondition GetFilterCondition()
		{
			return _filterConditionsEditor.GetTimelineFilterCondition();
		}

		public void SetFilterCondition(TimelineFilterCondition timelineFilterCondition)
		{
			_filterConditionsEditor.SetTimelineFilterCondition(timelineFilterCondition);
		}

		internal void UpdateIConditionOwner(IConditionOwner iConditionOwner)
		{
			_iConditionOwner = iConditionOwner;
			if (_iConditionOwner != null)
			{
				_filterConditionsEditor.SetTimeline(((TimelineMordexTableBase)_iConditionOwner.Table).GetTimeline());
			}
			else
			{
				_filterConditionsEditor.SetTimeline(null);
			}
		}

		private void InitializeControls()
		{
			_filterConditionsEditor = new TimelineFilterConditionsEditor();
			_btnOk = new Button();
			_btnCancel = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_filterConditionsEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_filterConditionsEditor.DockPadding.All = 1;
			_filterConditionsEditor.SetTimelineFilterCondition(null);
			_filterConditionsEditor.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_filterConditionsEditor.Location = new Point(0, 2);
			_filterConditionsEditor.Name = "timelineFilterConditionsEditor1";
			_filterConditionsEditor.Size = new Size(559, 293);
			_filterConditionsEditor.TabIndex = 0;
			_filterConditionsEditor.SetTimeline(null);
			_btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOk.DialogResult = DialogResult.OK;
			_btnOk.Location = new Point(388, 297);
			_btnOk.Name = "btnOk";
			_btnOk.Size = new Size(80, 24);
			_btnOk.TabIndex = 1;
			_btnOk.Text = "OK";
			_btnOk.Click += OnBtnOkClick;
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(473, 297);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 2;
			_btnCancel.Text = "Cancel";
			_btnCancel.Click += OnBtnCancelClick;
			base.AcceptButton = _btnOk;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(560, 326);
			base.Controls.AddRange(new Control[3] { _btnCancel, _btnOk, _filterConditionsEditor });
			base.DockPadding.Top = 4;
			base.Name = "FilterBuilderDialog";
			Text = "FilterBuilderDialog";
			ResumeLayout(performLayout: false);
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnBtnOkClick(object sender, EventArgs eventArgs)
		{
			if (GetFilterCondition() != null)
			{
				GetFilterCondition().UpdateTimelineFilterCondition();
			}
			Close();
		}
	}
}
