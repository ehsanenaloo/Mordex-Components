using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Internal
{
	internal class ComboBoxThemeRenderer
	{
		private Color _borderColor;

		private Color _borderEndColor;

		private Color _disabledColor;

		private Color _hotBorderColor;

		private Color _pressedBorderColor;

		private Color _innerHighlightColor;

		private Color _pressedTopColor;

		private Color _pressedUpperMiddleColor;

		private Color _pressedLowerMiddleColor;

		private Color _pressedBottomColor;

		private Color _topShadowColor;

		private Color _sideShadowColor;

		private Color _hotTopColor;

		private Color _hotUpperMiddleColor;

		private Color _hotLowerMiddleColor;

		private Color _hotBottomColor;

		public ComboBoxThemeRenderer()
		{
			_borderColor = Color.FromArgb(171, 173, 179);
			_borderEndColor = Color.FromArgb(227, 233, 239);
			_disabledColor = Color.FromArgb(175, 175, 175);
			_hotBorderColor = Color.FromArgb(60, 127, 177);
			_pressedBorderColor = Color.FromArgb(44, 98, 139);
			_innerHighlightColor = Color.FromArgb(255, 250, 253, 254);
			_hotTopColor = Color.FromArgb(255, 234, 246, 253);
			_hotUpperMiddleColor = Color.FromArgb(255, 217, 240, 252);
			_hotLowerMiddleColor = Color.FromArgb(255, 190, 230, 253);
			_hotBottomColor = Color.FromArgb(255, 167, 217, 245);
			_pressedTopColor = Color.FromArgb(235, 247, 254);
			_pressedUpperMiddleColor = Color.FromArgb(196, 229, 246);
			_pressedLowerMiddleColor = Color.FromArgb(152, 209, 239);
			_pressedBottomColor = Color.FromArgb(100, 172, 212);
			_topShadowColor = Color.FromArgb(150, 51, 51, 51);
			_sideShadowColor = Color.FromArgb(90, 51, 51, 51);
		}

		public void DrawDropDownArrow(Graphics graphics, Rectangle rectangle, bool isEnabled)
		{
			Pen pen = (isEnabled ? GdiCache.Pen(SystemColors.ControlText) : GdiCache.Pen(_disabledColor));
			int arrowWidth = rectangle.Width - 2;
			RenderingHelper.DrawUpDownTriangle(graphics, arrowWidth, rectangle, 1, pen);
		}

		public bool DrawDropDownListButton(Graphics dc, Rectangle bounds, ComboStyle comboStyle, bool isEnabled, bool isHot, bool isPressed)
		{
			if (comboStyle == ComboStyle.DropDownList)
			{
				bounds.Inflate(1, 1);
				VisualStyleElement element = VisualStyleElement.Button.PushButton.Normal;
				if (!isEnabled)
				{
					element = VisualStyleElement.Button.PushButton.Disabled;
				}
				else if (isPressed)
				{
					element = VisualStyleElement.Button.PushButton.Pressed;
				}
				else if (isHot)
				{
					element = VisualStyleElement.Button.PushButton.Hot;
				}
				if (!VisualStyleRenderer.IsElementDefined(element))
				{
					return false;
				}
				VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(element);
				visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			dc.FillRectangle(SystemBrushes.Window, bounds);
			return true;
		}

		public bool DrawDropDownButton(Graphics graphics, Rectangle bounds, bool roundsLeftCorners, bool isEnabled, bool isPressed, bool isHot)
		{
			Color color = _borderColor;
			if (isPressed || isHot)
			{
				if (isPressed)
				{
					color = _pressedBorderColor;
					Rectangle fillBounds = bounds;
					fillBounds.Inflate(-1, -1);
					LinearGradientBrush pressedBrush = new LinearGradientBrush(fillBounds, _pressedTopColor, _pressedLowerMiddleColor, LinearGradientMode.Vertical);
					ColorBlend pressedBlend = new ColorBlend();
					pressedBlend.Positions = new float[4] { 0f, 0.5f, 0.5f, 1f };
					pressedBlend.Colors = new Color[4] { _pressedTopColor, _pressedUpperMiddleColor, _pressedLowerMiddleColor, _pressedBottomColor };
					pressedBrush.InterpolationColors = pressedBlend;
					graphics.FillRectangle(pressedBrush, fillBounds);
					pressedBrush.Dispose();
					if (bounds.Width > 4 && bounds.Height > 4)
					{
						Rectangle leftShadowBounds = new Rectangle(bounds.X, bounds.Y + 1, 3, bounds.Height - 2);
						Rectangle topShadowBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, 3);
						Rectangle rightShadowBounds = new Rectangle(bounds.Right - 3, bounds.Y + 1, 2, bounds.Height - 2);
						LinearGradientBrush leftShadowBrush = new LinearGradientBrush(Rectangle.Inflate(leftShadowBounds, 1, 1), _sideShadowColor, Color.Transparent, LinearGradientMode.Horizontal);
						LinearGradientBrush rightShadowBrush = new LinearGradientBrush(Rectangle.Inflate(rightShadowBounds, 1, 1), _sideShadowColor, Color.Transparent, LinearGradientMode.Horizontal);
						LinearGradientBrush topShadowBrush = new LinearGradientBrush(Rectangle.Inflate(topShadowBounds, 1, 1), _topShadowColor, Color.Transparent, LinearGradientMode.Vertical);
						leftShadowBounds.Width--;
						topShadowBounds.Height--;
						rightShadowBounds.X++;
						graphics.FillRectangle(topShadowBrush, topShadowBounds);
						graphics.FillRectangle(leftShadowBrush, leftShadowBounds);
						graphics.FillRectangle(rightShadowBrush, rightShadowBounds);
						topShadowBrush.Dispose();
						leftShadowBrush.Dispose();
						rightShadowBrush.Dispose();
					}
				}
				else if (isHot)
				{
					color = _hotBorderColor;
					LinearGradientBrush hotBrush = new LinearGradientBrush(bounds, _hotTopColor, _hotBottomColor, LinearGradientMode.Vertical);
					ColorBlend hotBlend = new ColorBlend();
					hotBlend.Positions = new float[4] { 0f, 0.5f, 0.5f, 1f };
					hotBlend.Colors = new Color[4] { _hotTopColor, _hotUpperMiddleColor, _hotLowerMiddleColor, _hotBottomColor };
					hotBrush.InterpolationColors = hotBlend;
					graphics.FillRectangle(hotBrush, Rectangle.Inflate(bounds, -1, 0));
					hotBrush.Dispose();
				}
				Pen pen = GdiCache.Pen(color);
				DrawBorder(graphics, bounds, pen, roundsLeftCorners, !roundsLeftCorners);
				if (isHot)
				{
					Rectangle innerBounds = Rectangle.Inflate(bounds, -1, -1);
					Pen innerPen = GdiCache.Pen(_innerHighlightColor);
					DrawBorder(graphics, innerBounds, innerPen, roundsLeftCorners, !roundsLeftCorners);
				}
			}
			DrawDropDownArrow(graphics, bounds, isEnabled);
			return true;
		}

		public bool DrawDropDownBorder(Graphics graphics, Rectangle rect, ComboStyle comboStyle, bool isEnabled, bool isPressed)
		{
			if (comboStyle == ComboStyle.DropDownList)
			{
				return true;
			}
			Brush brush;
			if (!isEnabled)
			{
				brush = new SolidBrush(_disabledColor);
			}
			else if (isPressed)
			{
				Color pressedStartColor = Color.FromArgb(255, 61, 123, 173);
				Color pressedEndColor = Color.FromArgb(255, 183, 217, 237);
				brush = new LinearGradientBrush(rect, pressedStartColor, pressedEndColor, LinearGradientMode.Vertical);
				Blend pressedFade = new Blend(3);
				pressedFade.Positions = new float[3] { 0f, 0.07f, 1f };
				pressedFade.Factors = new float[3] { 0f, 0.95f, 1f };
				((LinearGradientBrush)brush).Blend = pressedFade;
			}
			else
			{
				brush = new LinearGradientBrush(rect, _borderColor, _borderEndColor, LinearGradientMode.Vertical);
				Blend normalFade = new Blend(4);
				normalFade.Positions = new float[4] { 0f, 0.05f, 0.95f, 1f };
				normalFade.Factors = new float[4] { 0f, 0.9f, 0.9f, 1f };
				((LinearGradientBrush)brush).Blend = normalFade;
			}
			using (Pen pen = new Pen(brush))
			{
				DrawBorder(graphics, rect, pen, true, true);
			}
			rect.Inflate(-1, -1);
			Pen highlightPen = GdiCache.Pen(Color.FromArgb(190, Color.White));
			DrawBorder(graphics, rect, highlightPen, true, true);
			brush.Dispose();
			return true;
		}

		private void DrawBorder(Graphics graphics, Rectangle rectangle, Pen pen, bool cutLeftCorners, bool cutRightCorners)
		{
			int leftCut = (cutLeftCorners ? 1 : 0);
			int rightCut = (cutRightCorners ? 1 : 0);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(rectangle.X + leftCut, rectangle.Top, rectangle.Right - 1 - rightCut, rectangle.Top);
			graphicsPath.AddLine(rectangle.Right - 1 - rightCut, rectangle.Top, rectangle.Right - 1 - rightCut, rectangle.Top + rightCut);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Top + rightCut, rectangle.Right - 1, rectangle.Bottom - 1 - rightCut);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Bottom - 1 - rightCut, rectangle.Right - 1 - rightCut, rectangle.Bottom - 1 - rightCut);
			graphicsPath.AddLine(rectangle.Right - 1 - rightCut, rectangle.Bottom - 1, rectangle.X + leftCut, rectangle.Bottom - 1);
			graphicsPath.AddLine(rectangle.X + leftCut, rectangle.Bottom - 1, rectangle.X + leftCut, rectangle.Bottom - 1 - leftCut);
			graphicsPath.AddLine(rectangle.X, rectangle.Bottom - 1 - leftCut, rectangle.X, rectangle.Y + leftCut);
			graphicsPath.AddLine(rectangle.X, rectangle.Top + leftCut, rectangle.X + leftCut, rectangle.Top + leftCut);
			graphics.DrawPath(pen, graphicsPath);
			graphicsPath.Dispose();
		}
	}
}
