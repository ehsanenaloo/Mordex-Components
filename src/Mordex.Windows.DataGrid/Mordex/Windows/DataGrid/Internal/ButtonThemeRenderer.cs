using System;
using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace Mordex.Windows.DataGrid.Internal
{
	internal static class ButtonThemeRenderer
	{
		private static class GridAppearanceState
		{
			internal static Color _borderColor = Color.FromArgb(171, 173, 179);

			internal static Color _disabledFillColor = Color.FromArgb(227, 233, 239);

			internal static Color _disabledBorderColor = Color.FromArgb(175, 175, 175);

			internal static Color _outerBorderColor = Color.FromArgb(60, 127, 177);

			internal static Color _pressedBorderColor = Color.FromArgb(44, 98, 139);

			internal static Color _innerHighlightColor = Color.FromArgb(250, 255, 255, 255);

			internal static Color _innerHighlightFadeColor = Color.FromArgb(133, 255, 255, 255);

			internal static Color _innerBorderColor = Color.FromArgb(255, 250, 253, 254);

			internal static Color _hotTopLightColor = Color.FromArgb(235, 247, 254);

			internal static Color _hotTopDarkColor = Color.FromArgb(196, 229, 246);

			internal static Color _hotBottomLightColor = Color.FromArgb(152, 209, 239);

			internal static Color _hotBottomDarkColor = Color.FromArgb(100, 172, 212);

			internal static Color _shadowColor = Color.FromArgb(150, 51, 51, 51);

			internal static Color _shadowFadeColor = Color.FromArgb(90, 51, 51, 51);

			internal static Color _pressedTopLightColor = Color.FromArgb(255, 234, 246, 253);

			internal static Color _pressedTopDarkColor = Color.FromArgb(255, 217, 240, 252);

			internal static Color _pressedBottomLightColor = Color.FromArgb(255, 190, 230, 253);

			internal static Color _pressedBottomDarkColor = Color.FromArgb(255, 167, 217, 245);
		}

		public static void DrawDropArrow(Graphics graphics, Rectangle rectangle, bool condition)
		{
			Pen pen = (condition ? GdiCache.Pen(SystemColors.ControlText) : GdiCache.Pen(GridAppearanceState._disabledBorderColor));
			int value = rectangle.Width - 2;
			ControlStyle.DrawVerticalTriangle(graphics, value, rectangle, 1, pen);
		}

		public static bool DrawBackground(Graphics dc, Color color, Rectangle bounds, ComboStyle comboStyle, bool condition, bool otherCondition, bool nextCondition)
		{
			if (comboStyle == ComboStyle.DropDownList)
			{
				bounds.Inflate(1, 1);
				VisualStyleElement element = VisualStyleElement.Button.PushButton.Normal;
				if (!condition)
				{
					element = VisualStyleElement.Button.PushButton.Disabled;
				}
				else if (nextCondition)
				{
					element = VisualStyleElement.Button.PushButton.Pressed;
				}
				else if (otherCondition)
				{
					element = VisualStyleElement.Button.PushButton.Hot;
				}
				if (!VisualStyleRenderer.IsElementDefined(element))
				{
					return false;
				}
				VisualStyleRenderer visualStyleRenderer;
				try
				{
					visualStyleRenderer = new VisualStyleRenderer(element);
				}
				catch (InvalidOperationException)
				{
					// Windows can refuse a visual style handle while the style changes.
					return false;
				}
				visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			SolidBrush solidBrush = GdiCache.Brush(color);
			dc.FillRectangle(solidBrush, bounds);
			return true;
		}

		public static bool DrawThemedBackground(Graphics graphics, Rectangle otherRectangle, bool condition, bool otherCondition, bool nextCondition, bool previousCondition, bool firstCondition)
		{
			Color color = GridAppearanceState._borderColor;
			if (nextCondition || previousCondition)
			{
				if (nextCondition)
				{
					color = GridAppearanceState._pressedBorderColor;
					Rectangle bounds = otherRectangle;
					bounds.Inflate(-1, -1);
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(bounds, GridAppearanceState._hotTopLightColor, GridAppearanceState._hotBottomLightColor, LinearGradientMode.Vertical);
					ColorBlend colorBlend = new ColorBlend();
					colorBlend.Positions = new float[4] { 0f, 0.5f, 0.5f, 1f };
					colorBlend.Colors = new Color[4]
					{
						GridAppearanceState._hotTopLightColor,
						GridAppearanceState._hotTopDarkColor,
						GridAppearanceState._hotBottomLightColor,
						GridAppearanceState._hotBottomDarkColor
					};
					linearGradientBrush.InterpolationColors = colorBlend;
					graphics.FillRectangle(linearGradientBrush, bounds);
					linearGradientBrush.Dispose();
					if (otherRectangle.Width > 4 && otherRectangle.Height > 4)
					{
						Rectangle nextRectangle = new Rectangle(otherRectangle.X, otherRectangle.Y + 1, 3, otherRectangle.Height - 2);
						Rectangle previousRectangle = new Rectangle(otherRectangle.X, otherRectangle.Y, otherRectangle.Width - 1, 3);
						Rectangle firstRectangle = new Rectangle(otherRectangle.Right - 3, otherRectangle.Y + 1, 2, otherRectangle.Height - 2);
						LinearGradientBrush otherLinearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(nextRectangle, 1, 1), GridAppearanceState._shadowFadeColor, Color.Transparent, LinearGradientMode.Horizontal);
						LinearGradientBrush brush = new LinearGradientBrush(Rectangle.Inflate(firstRectangle, 1, 1), GridAppearanceState._shadowFadeColor, Color.Transparent, LinearGradientMode.Horizontal);
						LinearGradientBrush nextLinearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(previousRectangle, 1, 1), GridAppearanceState._shadowColor, Color.Transparent, LinearGradientMode.Vertical);
						nextRectangle.Width--;
						previousRectangle.Height--;
						firstRectangle.X++;
						graphics.FillRectangle(nextLinearGradientBrush, previousRectangle);
						graphics.FillRectangle(otherLinearGradientBrush, nextRectangle);
						graphics.FillRectangle(brush, firstRectangle);
						nextLinearGradientBrush.Dispose();
						otherLinearGradientBrush.Dispose();
					}
				}
				else if (previousCondition)
				{
					color = GridAppearanceState._outerBorderColor;
					LinearGradientBrush previousLinearGradientBrush = new LinearGradientBrush(otherRectangle, GridAppearanceState._pressedTopLightColor, GridAppearanceState._pressedBottomDarkColor, LinearGradientMode.Vertical);
					ColorBlend otherColorBlend = new ColorBlend();
					otherColorBlend.Positions = new float[4] { 0f, 0.5f, 0.5f, 1f };
					otherColorBlend.Colors = new Color[4]
					{
						GridAppearanceState._pressedTopLightColor,
						GridAppearanceState._pressedTopDarkColor,
						GridAppearanceState._pressedBottomLightColor,
						GridAppearanceState._pressedBottomDarkColor
					};
					previousLinearGradientBrush.InterpolationColors = otherColorBlend;
					graphics.FillRectangle(previousLinearGradientBrush, Rectangle.Inflate(otherRectangle, -1, 0));
					previousLinearGradientBrush.Dispose();
				}
				Pen pen = GdiCache.Pen(color);
				DrawCutCornerBorder(graphics, otherRectangle, pen, condition && firstCondition, !condition && firstCondition);
				if (previousCondition)
				{
					Rectangle rectangle = Rectangle.Inflate(otherRectangle, -1, -1);
					using (new LinearGradientBrush(rectangle, GridAppearanceState._innerHighlightColor, GridAppearanceState._innerHighlightFadeColor, LinearGradientMode.Vertical))
					{
						Pen otherPen = GdiCache.Pen(GridAppearanceState._innerBorderColor);
						DrawCutCornerBorder(graphics, rectangle, otherPen, condition && firstCondition, !condition && firstCondition);
					}
				}
			}
			return true;
		}

		public static bool RenderContent(Graphics graphics, Rectangle rectangle, ComboStyle comboStyle, bool condition, bool otherCondition)
		{
			if (comboStyle == ComboStyle.DropDownList)
			{
				return true;
			}
			Brush brush;
			if (!condition)
			{
				brush = new SolidBrush(GridAppearanceState._disabledBorderColor);
			}
			else if (otherCondition)
			{
				Color color = Color.FromArgb(255, 61, 123, 173);
				Color otherColor = Color.FromArgb(255, 183, 217, 237);
				brush = new LinearGradientBrush(rectangle, color, otherColor, LinearGradientMode.Vertical);
				Blend blend = new Blend(3);
				blend.Positions = new float[3] { 0f, 0.07f, 1f };
				blend.Factors = new float[3] { 0f, 0.95f, 1f };
				((LinearGradientBrush)brush).Blend = blend;
			}
			else
			{
				brush = new LinearGradientBrush(rectangle, GridAppearanceState._borderColor, GridAppearanceState._disabledFillColor, LinearGradientMode.Vertical);
				Blend otherBlend = new Blend(4);
				otherBlend.Positions = new float[4] { 0f, 0.05f, 0.95f, 1f };
				otherBlend.Factors = new float[4] { 0f, 0.9f, 0.9f, 1f };
				((LinearGradientBrush)brush).Blend = otherBlend;
			}
			using (Pen pen = new Pen(brush))
			{
				DrawCutCornerBorder(graphics, rectangle, pen, true, true);
			}
			rectangle.Inflate(-1, -1);
			Pen otherPen = GdiCache.Pen(Color.FromArgb(190, Color.White));
			DrawCutCornerBorder(graphics, rectangle, otherPen, true, true);
			brush.Dispose();
			return true;
		}

		private static void DrawCutCornerBorder(Graphics graphics, Rectangle rectangle, Pen pen, bool condition, bool otherCondition)
		{
			int value = (condition ? 1 : 0);
			int otherValue = (otherCondition ? 1 : 0);
			int nextValue = value;
			int previousValue = otherValue;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(rectangle.X + value, rectangle.Top, rectangle.Right - 1 - otherValue, rectangle.Top);
			graphicsPath.AddLine(rectangle.Right - 1 - otherValue, rectangle.Top, rectangle.Right - 1 - otherValue, rectangle.Top + otherValue);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Top + otherValue, rectangle.Right - 1, rectangle.Bottom - 1 - previousValue);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Bottom - 1 - previousValue, rectangle.Right - 1 - previousValue, rectangle.Bottom - 1 - previousValue);
			graphicsPath.AddLine(rectangle.Right - 1 - previousValue, rectangle.Bottom - 1, rectangle.X + nextValue, rectangle.Bottom - 1);
			graphicsPath.AddLine(rectangle.X + nextValue, rectangle.Bottom - 1, rectangle.X + nextValue, rectangle.Bottom - 1 - nextValue);
			graphicsPath.AddLine(rectangle.X, rectangle.Bottom - 1 - nextValue, rectangle.X, rectangle.Y + value);
			graphicsPath.AddLine(rectangle.X, rectangle.Top + value, rectangle.X + value, rectangle.Top + value);
			graphics.DrawPath(pen, graphicsPath);
			graphicsPath.Dispose();
		}
	}
}
