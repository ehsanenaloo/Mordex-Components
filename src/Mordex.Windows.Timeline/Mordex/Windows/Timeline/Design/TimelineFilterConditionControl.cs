using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design
{
	[ToolboxItem(false)]
	internal class TimelineFilterConditionControl : Control
	{
		private TimelineFilterCondition _timelineFilterCondition;

		private TimelineFilterCondition _selectedCondition;

		public TimelineFilterConditionControl()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		}

		public void UpdateTimelineFilterCondition(TimelineFilterCondition timelineFilterCondition)
		{
			_timelineFilterCondition = timelineFilterCondition;
			Invalidate();
		}

		public void UpdateSelectedCondition(TimelineFilterCondition timelineFilterConditionControlTimelineFilterCondition)
		{
			_selectedCondition = timelineFilterConditionControlTimelineFilterCondition;
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
			TimelineUtilities.DrawFlatBorder(eventArgs.Graphics, SystemPens.ControlDark, clientRectangle);
			clientRectangle.Inflate(-4, -4);
			brush = null;
			if (_timelineFilterCondition != null)
			{
				string text = _timelineFilterCondition.ToString();
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
				eventArgs.Graphics.DrawString(_timelineFilterCondition.ToString(), Font, brush, rectangleF, textFormat);
				brush.Dispose();
				if (_selectedCondition != _timelineFilterCondition)
				{
					bool hasReachedCondition = false;
					string textBeforeSelection = _timelineFilterCondition.GetTextBeforeCondition(_selectedCondition, ref hasReachedCondition);
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
