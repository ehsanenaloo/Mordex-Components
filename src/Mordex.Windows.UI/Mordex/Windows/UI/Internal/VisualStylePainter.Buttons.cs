using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Internal
{
	internal partial class VisualStylePainter
	{

		public Pen GetBorderPen()
		{
			if (_borderPen == null)
			{
				_borderPen = new Pen(_isStandardStyle ? _comboBorderColor : _borderColor);
			}
			return _borderPen;
		}

		public Pen CreateStatePen(bool isHot, bool isChecked)
		{
			if (isChecked && isHot)
			{
				return new Pen(_pressedBorderColor);
			}
			if (isChecked)
			{
				return new Pen(_checkedBorderColor);
			}
			return new Pen(_hotBorderColor);
		}

		public void DrawButtonBackground(Graphics graphics, Rectangle bounds, Color baseColor, bool isHot, bool isChecked, bool isEnabled, bool hasGlossBlend, bool shouldDrawBorder, bool isVertical)
		{
			LinearGradientBrush linearGradientBrush = null;
			if (!isEnabled)
			{
				return;
			}
			if (bounds.Width > 0 && bounds.Height > 0)
			{
				LinearGradientMode linearGradientMode = LinearGradientMode.Vertical;
				if (!isVertical)
				{
					linearGradientMode = LinearGradientMode.Horizontal;
				}
				if (isHot && isChecked)
				{
					linearGradientBrush = new LinearGradientBrush(bounds, _pressedColor, _pressedGradientColor, linearGradientMode);
				}
				else if (isHot)
				{
					linearGradientBrush = new LinearGradientBrush(bounds, _hotColor, _hotGradientColor, linearGradientMode);
				}
				else if (isChecked)
				{
					linearGradientBrush = new LinearGradientBrush(bounds, _checkedColor, _checkedGradientColor, linearGradientMode);
				}
				else
				{
					linearGradientBrush = new LinearGradientBrush(bounds, _commandBarColor, _commandBarGradientColor, linearGradientMode);
					if (hasGlossBlend)
					{
						Blend blend = new Blend(4);
						blend.Positions = new float[4] { 0f, 0.53f, 0.53f, 1f };
						if (!IsHomeSteadTheme())
						{
							blend.Factors = new float[4] { 0f, 0.17f, 0.19f, 1f };
						}
						else
						{
							blend.Factors = new float[4] { 0f, 0.6f, 0.6f, 1f };
						}
						linearGradientBrush.Blend = blend;
					}
				}
				if (linearGradientBrush != null)
				{
					graphics.FillRectangle(linearGradientBrush, bounds);
					linearGradientBrush.Dispose();
					linearGradientBrush = null;
				}
			}
			if (!shouldDrawBorder)
			{
				return;
			}
			if (isEnabled && (isHot || isChecked))
			{
				using (Pen pen = CreateStatePen(isHot, isChecked))
				{
					RenderingHelper.DrawRectangleOutline(graphics, pen, bounds);
					return;
				}
			}
			RenderingHelper.DrawRectangleOutline(graphics, GetBorderPen(), bounds);
		}

		public void DrawGradientButton(Graphics graphics, Rectangle bounds, bool isHot, bool isChecked, bool isEnabled, bool shouldDrawBorder, bool isVertical)
		{
			LinearGradientBrush linearGradientBrush = null;
			if (!isEnabled || bounds.Width <= 0 || bounds.Height <= 0)
			{
				return;
			}
			LinearGradientMode linearGradientMode = LinearGradientMode.Vertical;
			if (!isVertical)
			{
				linearGradientMode = LinearGradientMode.Horizontal;
			}
			Color color;
			if (isHot && isChecked)
			{
				linearGradientBrush = new LinearGradientBrush(bounds, _pressedColor, _pressedGradientColor, linearGradientMode);
				color = _pressedBorderColor;
			}
			else if (isHot)
			{
				linearGradientBrush = new LinearGradientBrush(bounds, _hotColor, _hotGradientColor, linearGradientMode);
				color = _hotBorderColor;
			}
			else if (isChecked)
			{
				linearGradientBrush = new LinearGradientBrush(bounds, _checkedColor, _checkedGradientColor, linearGradientMode);
				color = _checkedBorderColor;
			}
			else
			{
				linearGradientBrush = new LinearGradientBrush(bounds, _commandBarColor, _commandBarGradientColor, linearGradientMode);
				color = _borderColor;
			}
			if (linearGradientBrush != null)
			{
				graphics.FillRectangle(linearGradientBrush, bounds);
				linearGradientBrush.Dispose();
				linearGradientBrush = null;
			}
			if (!shouldDrawBorder)
			{
				return;
			}
			Pen pen = GdiCache.Pen(color);
			RenderingHelper.DrawRectangleOutline(graphics, pen, bounds);
		}

		public void DrawToolBarButton(Graphics dc, Rectangle bounds, bool isEnabled, bool isHot, bool isPressed, bool isChecked)
		{
			bool isPressedWhileHot = isHot && isPressed;
			VisualStyleElement visualStyleElement = ((!isEnabled) ? VisualStyleElement.ToolBar.Button.Disabled : (isPressedWhileHot ? VisualStyleElement.ToolBar.Button.Pressed : (isHot ? ((!isChecked) ? VisualStyleElement.ToolBar.Button.Hot : VisualStyleElement.ToolBar.Button.HotChecked) : ((!isChecked) ? VisualStyleElement.ToolBar.Button.Normal : VisualStyleElement.ToolBar.Button.Checked))));
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawToolBarItem(Graphics dc, Rectangle bounds, bool isEnabled, bool isHot, bool isPressed, bool isChecked)
		{
			DrawToolBarButton(dc, bounds, isEnabled, isHot, isPressed, isChecked);
		}
	}
}
