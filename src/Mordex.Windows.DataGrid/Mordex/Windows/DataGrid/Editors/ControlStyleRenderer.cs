using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class ControlStyleRenderer
	{
		private MxColorSchemeAppearanceState _drawingStyle;

		private ControlStyle _style;

		internal ControlStyleRenderer(MxColorSchemeAppearanceState drawingStyle, ControlStyle controlStyle)
		{
			_drawingStyle = drawingStyle;
			_style = controlStyle;
		}

		internal void RenderContent(Graphics graphics, Rectangle rectangle, Font font, string text, bool pressed, bool hot, bool condition)
		{
			if (_drawingStyle.IsThemedLook)
			{
				DrawBackground(graphics, rectangle, font, text, pressed, hot, condition);
			}
			else if (_drawingStyle._isThemed)
			{
				VisualStyleDrawer.DrawPushButton(graphics, rectangle, hot, pressed, false);
				TextLayoutRenderer otherTextLayoutRenderer = new TextLayoutRenderer(_drawingStyle.GetUseTextRenderer());
				otherTextLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				GridHelper.DrawImageAndText(graphics, SystemBrushes.ControlText, text, null, rectangle, ImageHorizontalAlignment.BeforeText, ImageVerticalAlignment.Center, otherTextLayoutRenderer, font, condition, -1, null, false);
			}
			else
			{
				DrawButtonText(graphics, rectangle, font, text, pressed, hot, condition);
			}
		}

		private void DrawButtonText(Graphics graphics, Rectangle rectangle, Font font, string text, bool condition, bool otherCondition, bool nextCondition)
		{
			_style.DrawBackground(graphics, rectangle, otherCondition, condition);
			rectangle.Inflate(-2, -2);
			if (condition && _style.IsFlatAppearance())
			{
				rectangle.Offset(1, 1);
			}
			DrawCenteredText(graphics, text, rectangle, font, _style.ControlTextColor, nextCondition, _drawingStyle.GetUseTextRenderer());
		}

		private void DrawBackground(Graphics graphics, Rectangle rectangle, Font font, string text, bool condition, bool otherCondition, bool nextCondition)
		{
			Color color;
			Color gridColor;
			if (condition)
			{
				color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.PressedElement);
				gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.PressedElementGradient);
			}
			else if (otherCondition)
			{
				color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElement);
				gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementGradient);
			}
			else
			{
				color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.Button);
				gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.ButtonGradient);
			}
			Brush brush = new LinearGradientBrush(Rectangle.Inflate(rectangle, 1, 1), color, gridColor, 90f);
			graphics.FillRectangle(brush, rectangle);
			brush.Dispose();
			Pen pen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.ControlBorder));
			GridHelper.DrawFlatBorder(graphics, pen, rectangle);
			DrawCenteredText(graphics, text, rectangle, font, SystemColors.ControlText, nextCondition, _drawingStyle.GetUseTextRenderer());
		}

		private static void DrawCenteredText(Graphics graphics, string text, Rectangle rectangle, Font font, Color color, bool condition, bool useTextRenderer)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			if (condition)
			{
				textLayoutRenderer.UpdateRightToLeft(RightToLeft.Yes);
			}
			textLayoutRenderer.DrawTextWithColor(graphics, text, rectangle, font, color);
			textLayoutRenderer.ReleaseStringFormat();
		}
	}
}
