using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public partial class TimelineSurface
	{
		private void UpdateIContainer()
		{
			_iContainer = new Container();
			BackColor = SystemColors.Window;
			ForeColor = SystemColors.ControlText;
			Text = "Calendar";
		}

		internal void ApplyTheme()
		{
			ApplyThemeToFormatStyles();
			Size size = _borderSize;
			UpdateBorderSize();
			base.ClientRectangle.Inflate(new Size(_borderSize.Width - size.Width, _borderSize.Height - size.Height));
			ComputeDroppedControlSize();
			UpdateCellSizes();
			UpdateTimelineLayoutStateArray(true);
			UpdateLayout(true);
			Invalidate();
		}

		internal void UpdateCellSizes()
		{
			_contentBounds = base.ClientRectangle;
			_contentBounds.Inflate(-_borderSize.Width / 2, -_borderSize.Height / 2);
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!ShouldUseCompatibleTextRendering());
			textLayoutRenderer.UpdateIsPaddingDisabled(true);
			if (HeaderFont == null || _calendarFont == null)
			{
				return;
			}
			_monthLayoutTemplate = new TimelineLayoutState();
			Graphics graphics = CreateGraphics();
			SizeF sizeF = textLayoutRenderer.RenderContent(graphics, "88", HeaderFont, 0);
			_headerHeight = new Size((int)sizeF.Width, (int)sizeF.Height).Height + 6;
			sizeF = textLayoutRenderer.RenderContent(graphics, "88", _calendarFont, 0);
			Size size = new Size((int)sizeF.Width, (int)sizeF.Height);
			_dayCellWidth = size.Width + 2;
			_dayCellHeight = size.Height + 3;
			for (int i = 0; i < 7; i++)
			{
				sizeF = textLayoutRenderer.RenderContent(graphics, _textArray[i], base.Font, 0);
				size = new Size((int)sizeF.Width, (int)sizeF.Height);
				if (size.Width + 2 > _dayCellWidth)
				{
					_dayCellWidth = size.Width + 2;
				}
			}
			_weekNumberColumnWidth = _dayCellWidth;
			Rectangle rectangle = new Rectangle(0, 0, _weekNumberColumnWidth * 2 + _dayCellWidth * 7, _headerHeight);
			_monthLayoutTemplate.SetHeaderBounds(rectangle);
			_monthLayoutTemplate.LayoutGrid(_dayCellWidth, _dayCellHeight, _weekNumberColumnWidth, new Point(0, 0), _borderSize);
			graphics.Dispose();
			graphics = null;
			ComputeDroppedControlSize();
			textLayoutRenderer.ResetFormat();
			Invalidate();
		}

		private void UpdateBorderSize()
		{
			_borderSize = new Size(8, 8);
		}

		internal void ApplyThemeToFormatStyles()
		{
			if (_headerFormatStyle != null)
			{
				HeaderFormatStyle.ApplyThemedStyle();
			}
			if (_selectedFormatStyle != null)
			{
				SelectedFormatStyle.ApplyThemedStyle();
			}
			if (_trailingFormatStyle != null)
			{
				TrailingFormatStyle.ApplyThemedStyle();
			}
			Invalidate();
		}
	}
}
