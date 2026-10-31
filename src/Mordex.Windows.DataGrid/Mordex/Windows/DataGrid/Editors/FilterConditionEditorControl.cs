using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	internal class FilterConditionEditorControl : Control
	{
		private GridFilterCondition _gridFilterCondition;

		private GridFilterCondition _filterConditionEditorControlGridFilterCondition;

		public FilterConditionEditorControl()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		}

		public void SetFilterCondition(GridFilterCondition gridFilterCondition)
		{
			_gridFilterCondition = gridFilterCondition;
			Invalidate();
		}

		public void UpdateFilterConditionEditorControlGridFilterCondition(GridFilterCondition filterConditionEditorControlGridFilterCondition)
		{
			_filterConditionEditorControlGridFilterCondition = filterConditionEditorControlGridFilterCondition;
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			base.OnPaint(eventArgs);
			Brush brush = GdiCache.Brush(BackColor);
			eventArgs.Graphics.FillRectangle(brush, base.ClientRectangle);
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-4, 0);
			GridHelper.DrawFlatBorder(eventArgs.Graphics, SystemPens.ControlDark, clientRectangle);
			clientRectangle.Inflate(-4, -4);
			brush = null;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: true);
			textLayoutRenderer.SetWordWrap(true);
			if (_gridFilterCondition != null)
			{
				string text = _gridFilterCondition.ToString();
				textLayoutRenderer.DrawTextWithColor(eventArgs.Graphics, text, clientRectangle, Font, ForeColor);
			}
			else
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				string text = "Click in the Add Condition button to create a new filter condition";
				textLayoutRenderer.DrawTextWithColor(eventArgs.Graphics, text, clientRectangle, Font, ForeColor);
			}
			textLayoutRenderer.ReleaseStringFormat();
		}
	}
}
