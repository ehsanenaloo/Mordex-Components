using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
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

		public void DrawClassicRebarBackground(Graphics graphics, Rectangle bounds)
		{
			if (!_isStandardStyle)
			{
				if (bounds.Width > 0 && bounds.Height > 0)
				{
					GdiCache.FillGradient(graphics, bounds, _rebarGradientColor, _rebarColor, LinearGradientMode.Vertical);
				}
				return;
			}
			graphics.FillRectangle(SystemBrushes.Control, bounds);
			bool isMetallic = VisualStyleInformation.ColorScheme == ThemeMetallic;
			Color lineColor = (isMetallic ? Color.FromArgb(138, 139, 143) : Color.FromArgb(149, 147, 133));
			Pen linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Y, bounds.Right, bounds.Y);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(181, 182, 186) : Color.FromArgb(192, 193, 188));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Y + 1, bounds.Right, bounds.Y + 1);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(203, 204, 208) : Color.FromArgb(216, 215, 205));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Y + 2, bounds.Right, bounds.Y + 2);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(219, 220, 225) : Color.FromArgb(232, 229, 212));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Y + 3, bounds.Right, bounds.Y + 3);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(224, 225, 230) : Color.FromArgb(238, 235, 217));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Y + 4, bounds.Right, bounds.Y + 4);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(216, 217, 223) : Color.FromArgb(234, 230, 211));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Bottom - 6, bounds.Right, bounds.Bottom - 6);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(212, 213, 219) : Color.FromArgb(231, 228, 209));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Bottom - 5, bounds.Right, bounds.Bottom - 5);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(206, 207, 213) : Color.FromArgb(227, 225, 205));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Bottom - 4, bounds.Right, bounds.Bottom - 4);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(211, 211, 216) : Color.FromArgb(221, 219, 199));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Bottom - 3, bounds.Right, bounds.Bottom - 3);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(200, 201, 208) : Color.FromArgb(217, 214, 192));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Bottom - 2, bounds.Right, bounds.Bottom - 2);
			linePen = null;
			lineColor = (isMetallic ? Color.FromArgb(209, 210, 215) : Color.FromArgb(223, 220, 202));
			linePen = GdiCache.Pen(lineColor);
			graphics.DrawLine(linePen, bounds.X, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
			linePen = null;
		}

		public void DrawRebarBackground(Graphics graphics, Rectangle bounds, Rectangle gradientBounds, bool isHorizontal, DockStyle dockStyle)
		{
			if (gradientBounds.Width <= 0 || gradientBounds.Height <= 0)
			{
				return;
			}
			LinearGradientBrush linearGradientBrush = null;
			if (!_isStandardStyle)
			{
				SolidBrush solidBrush = null;
				switch (dockStyle)
				{
				case DockStyle.Top:
				case DockStyle.Bottom:
					solidBrush = new SolidBrush(_rebarGradientColor);
					graphics.FillRectangle(solidBrush, bounds);
					gradientBounds.Width = RebarGradientWidth;
					linearGradientBrush = new LinearGradientBrush(gradientBounds, _rebarColor, _rebarGradientColor, LinearGradientMode.Horizontal);
					graphics.FillRectangle(linearGradientBrush, gradientBounds);
					break;
				case DockStyle.Left:
					solidBrush = new SolidBrush(_rebarColor);
					graphics.FillRectangle(solidBrush, bounds);
					break;
				case DockStyle.Right:
					solidBrush = new SolidBrush(_rebarGradientColor);
					graphics.FillRectangle(solidBrush, bounds);
					break;
				}
				if (solidBrush != null)
				{
					solidBrush.Dispose();
					solidBrush = null;
				}
			}
			else {
				gradientBounds.Height = bounds.Height;
				gradientBounds.Y = bounds.Y;
				if (VisualStyleInformation.ColorScheme == ThemeMetallic)
				{
					linearGradientBrush = new LinearGradientBrush(gradientBounds, _rebarColor, _rebarGradientColor, LinearGradientMode.Horizontal);
					graphics.FillRectangle(linearGradientBrush, bounds);
				}
				else
				{
					LinearGradientBrush diagonalBrush = new LinearGradientBrush(gradientBounds, _rebarColor, _rebarGradientColor, LinearGradientMode.ForwardDiagonal);
					Blend blend = new Blend(4);
					blend.Positions = new float[4] { 0f, 0.33f, 0.66f, 1f };
					blend.Factors = new float[4] { 0f, 0.5f, 0.75f, 1f };
					diagonalBrush.Blend = blend;
					graphics.FillRectangle(diagonalBrush, bounds);
					diagonalBrush.Dispose();
					diagonalBrush = null;
					if (isHorizontal)
					{
						if (dockStyle == DockStyle.Top)
						{
							bounds.Y = bounds.Bottom - 3;
						}
						Pen horizontalEdgePen = GdiCache.Pen(Color.FromArgb(233, 232, 221));
						graphics.DrawLine(horizontalEdgePen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
						horizontalEdgePen = null;
						bounds.Y++;
						horizontalEdgePen = GdiCache.Pen(Color.FromArgb(224, 223, 208));
						graphics.DrawLine(horizontalEdgePen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
						horizontalEdgePen = null;
						bounds.Y++;
						horizontalEdgePen = GdiCache.Pen(Color.FromArgb(205, 202, 187));
						graphics.DrawLine(horizontalEdgePen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
						horizontalEdgePen = null;
					}
					else
					{
						if (dockStyle == DockStyle.Left)
						{
							bounds.X = bounds.Right - 3;
						}
						Pen verticalEdgePen = GdiCache.Pen(Color.FromArgb(233, 232, 221));
						graphics.DrawLine(verticalEdgePen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
						verticalEdgePen = null;
						bounds.X++;
						verticalEdgePen = GdiCache.Pen(Color.FromArgb(224, 223, 208));
						graphics.DrawLine(verticalEdgePen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
						verticalEdgePen = null;
						bounds.X++;
						verticalEdgePen = GdiCache.Pen(Color.FromArgb(205, 202, 187));
						graphics.DrawLine(verticalEdgePen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
						verticalEdgePen = null;
					}
				}
			}
			if (linearGradientBrush != null)
			{
				linearGradientBrush.Dispose();
				linearGradientBrush = null;
			}
		}
	}
}
