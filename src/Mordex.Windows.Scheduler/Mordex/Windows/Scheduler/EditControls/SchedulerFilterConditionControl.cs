using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.EditControls
{
	[ToolboxItem(false)]
	internal class SchedulerFilterConditionControl : Control
	{
		private SchedulerFilterCondition _schedulerFilterCondition;

		private SchedulerFilterCondition _selectedCondition;

		public SchedulerFilterConditionControl()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		}

		public void UpdateSchedulerFilterCondition(SchedulerFilterCondition schedulerFilterCondition)
		{
			_schedulerFilterCondition = schedulerFilterCondition;
			Invalidate();
		}

		public void UpdateSelectedCondition(SchedulerFilterCondition schedulerFilterConditionControlSchedulerFilterCondition)
		{
			_selectedCondition = schedulerFilterConditionControlSchedulerFilterCondition;
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			base.OnPaint(eventArgs);
			Brush brush = new SolidBrush(BackColor);
			eventArgs.Graphics.FillRectangle(brush, base.ClientRectangle);
			brush.Dispose();
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-4, 0);
			SchedulerRenderer.DrawBorder(eventArgs.Graphics, SystemPens.ControlDark, clientRectangle);
			clientRectangle.Inflate(-4, -4);
			brush = null;
			if (_schedulerFilterCondition != null)
			{
				string text = _schedulerFilterCondition.ToString();
				if (_selectedCondition == null)
				{
					StringFormat stringFormat = new StringFormat();
					stringFormat.LineAlignment = StringAlignment.Center;
					stringFormat.Alignment = StringAlignment.Center;
					eventArgs.Graphics.DrawString(text, Font, brush, clientRectangle, stringFormat);
					return;
				}
				RectangleF rectangleF = new RectangleF(clientRectangle.Left, clientRectangle.Top, clientRectangle.Width, clientRectangle.Height);
				StringFormat textFormat = new StringFormat();
				brush = new SolidBrush(ForeColor);
				eventArgs.Graphics.DrawString(_schedulerFilterCondition.ToString(), Font, brush, rectangleF, textFormat);
				brush.Dispose();
				if (_selectedCondition != _schedulerFilterCondition)
				{
					bool hasReachedCondition = false;
					string textBeforeSelection = _schedulerFilterCondition.BuildTextUpTo(_selectedCondition, ref hasReachedCondition);
					string selectedText = _selectedCondition.ToString();
					textFormat.SetMeasurableCharacterRanges(new CharacterRange[1]
					{
						new CharacterRange(textBeforeSelection.Length, selectedText.Length)
					});
					Region[] selectedRegions = eventArgs.Graphics.MeasureCharacterRanges(text, Font, rectangleF, textFormat);
					textFormat.Dispose();
					eventArgs.Graphics.FillRegion(SystemBrushes.Highlight, selectedRegions[0]);
					eventArgs.Graphics.Clip = selectedRegions[0];
					eventArgs.Graphics.DrawString(text, Font, SystemBrushes.HighlightText, rectangleF, textFormat);
					eventArgs.Graphics.Clip = new Region();
					brush.Dispose();
				}
			}
			else
			{
				StringFormat centerFormat = new StringFormat();
				centerFormat.LineAlignment = StringAlignment.Center;
				centerFormat.Alignment = StringAlignment.Center;
				string text = "Click in the Add Condition button to create a new filter condition";
				brush = new SolidBrush(ForeColor);
				eventArgs.Graphics.DrawString(text, Font, brush, clientRectangle, centerFormat);
				brush.Dispose();
			}
		}
	}
}
