using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.DataGrid.Internal
{
	internal static class GridRenderer
	{
		private static Bitmap _hotCollapsedGlyph;

		private static Bitmap _expandedBitmap;

		internal static void DrawHotCollapsedGlyph(Graphics graphics, Rectangle rectangle)
		{
			if (_hotCollapsedGlyph == null)
			{
				_hotCollapsedGlyph = CreateCollapsedBitmap();
			}
			graphics.DrawImageUnscaled(_hotCollapsedGlyph, new Point(rectangle.X + rectangle.Width / 2 - 8, rectangle.Y + rectangle.Height / 2 - 8));
		}

		internal static void DrawHotExpandedGlyph(Graphics graphics, Rectangle rectangle)
		{
			if (_expandedBitmap == null)
			{
				_expandedBitmap = CreateExpandedBitmap();
			}
			graphics.DrawImageUnscaled(_expandedBitmap, new Point(rectangle.X + rectangle.Width / 2 - 8, rectangle.Y + rectangle.Height / 2 - 8));
		}

		private static void DrawBackground(Graphics graphics, Rectangle bounds)
		{
			Rectangle rectangle = bounds;
			Rectangle otherRectangle = new Rectangle(0, 0, 8, 8);
			otherRectangle.X = bounds.X + (bounds.Width - otherRectangle.Width) / 2;
			otherRectangle.Y = bounds.Y + (bounds.Height - otherRectangle.Height) / 2 - 1;
			bounds = otherRectangle;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.StartFigure();
			graphicsPath.AddLine(new Point(bounds.X, bounds.Y), new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2));
			graphicsPath.AddLine(new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2), new Point(bounds.X, bounds.Bottom));
			graphicsPath.AddLine(new Point(bounds.X, bounds.Bottom), new Point(bounds.X, bounds.Y));
			graphicsPath.CloseFigure();
			Color color = Color.FromArgb(255, 134, 26);
			Color otherColor = Color.FromArgb(255, 177, 26);
			Color centerColor = Color.FromArgb(255, 226, 185);
			GraphicsPath path = ComputeGraphicsPath(new Point(bounds.X - 1, bounds.Y + bounds.Height / 2), 16);
			GraphicsPath otherGraphicsPath = new GraphicsPath();
			otherGraphicsPath.AddEllipse(new Rectangle(rectangle.X, rectangle.Y, 10, 16));
			PathGradientBrush pathGradientBrush = new PathGradientBrush(otherGraphicsPath);
			pathGradientBrush.CenterPoint = new PointF(bounds.X, bounds.Y + bounds.Height / 2 - 1);
			pathGradientBrush.CenterColor = centerColor;
			pathGradientBrush.SurroundColors = new Color[1] { Color.Transparent };
			Blend blend = new Blend();
			blend.Positions = new float[3] { 1f, 1f, 0.25f };
			blend.Factors = new float[3] { 0f, 0.75f, 1f };
			pathGradientBrush.FocusScales = new PointF(0.33f, 0.33f);
			Region clip = graphics.Clip;
			Region region = new Region(new Rectangle(bounds.X - 1, bounds.Y - 1, 7, 11));
			graphics.Clip = region;
			graphics.FillPath(pathGradientBrush, path);
			pathGradientBrush.Dispose();
			graphics.Clip = clip;
			Pen pen = new Pen(color);
			Brush brush = new SolidBrush(otherColor);
			graphics.FillPath(brush, graphicsPath);
			graphics.DrawPath(pen, graphicsPath);
			pen.Dispose();
			brush.Dispose();
			clip.Dispose();
			region.Dispose();
			path.Dispose();
			graphicsPath.Dispose();
			otherGraphicsPath.Dispose();
		}

		private static GraphicsPath ComputeGraphicsPath(Point point, int value)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.StartFigure();
			graphicsPath.AddLine(new Point(point.X, point.Y - value / 2), new Point(point.X + value / 2, point.Y));
			graphicsPath.AddLine(new Point(point.X + value / 2, point.Y), new Point(point.X, point.Y + value / 2));
			graphicsPath.AddLine(new Point(point.X, point.Y + value / 2), new Point(point.X, point.Y - value / 2));
			graphicsPath.CloseFigure();
			return graphicsPath;
		}

		private static Bitmap CreateCollapsedBitmap()
		{
			Bitmap bitmap = new Bitmap(16, 16);
			Rectangle rectangle = new Rectangle(0, 0, 16, 16);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.FillRectangle(Brushes.Transparent, rectangle);
			DrawBackground(graphics, rectangle);
			graphics.Dispose();
			return bitmap;
		}

		private static Bitmap CreateExpandedBitmap()
		{
			Bitmap bitmap = new Bitmap(16, 16);
			Rectangle rectangle = new Rectangle(0, 0, 16, 16);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.FillRectangle(Brushes.Transparent, rectangle);
			DrawExpandedArrow(graphics, rectangle);
			graphics.Dispose();
			return bitmap;
		}

		private static void DrawExpandedArrow(Graphics graphics, Rectangle otherRectangle)
		{
			Rectangle rectangle = new Rectangle(0, 0, 8, 8);
			rectangle.X = otherRectangle.X + (otherRectangle.Width - rectangle.Width) / 2;
			rectangle.Y = otherRectangle.Y + (otherRectangle.Height - rectangle.Height) / 2 - 1;
			otherRectangle = rectangle;
			Color color = Color.FromArgb(255, 134, 26);
			Color baseColor = Color.FromArgb(255, 177, 26);
			Color otherBaseColor = Color.FromArgb(255, 255, 226, 185);
			Rectangle nextRectangle = new Rectangle(otherRectangle.X - 4, otherRectangle.Y, 9, 9);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddPolygon(new Point[3]
			{
				new Point(nextRectangle.X, nextRectangle.Bottom),
				new Point(nextRectangle.Right, nextRectangle.Bottom),
				new Point(nextRectangle.Right, nextRectangle.Y)
			});
			PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.CenterColor = Color.FromArgb(192, baseColor);
			pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(80, otherBaseColor) };
			graphics.FillPath(pathGradientBrush, graphicsPath);
			pathGradientBrush.Dispose();
			pathGradientBrush = null;
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(nextRectangle.X + 1, nextRectangle.Y, nextRectangle.Width, nextRectangle.Height), Color.Transparent, otherBaseColor, 0f);
			linearGradientBrush.SetSigmaBellShape(0.5f, 0.95f);
			Pen pen = new Pen(linearGradientBrush);
			graphics.DrawLine(pen, nextRectangle.X + 3, nextRectangle.Bottom, nextRectangle.Right - 1, nextRectangle.Bottom);
			pen.Dispose();
			linearGradientBrush.Dispose();
			linearGradientBrush = new LinearGradientBrush(new Rectangle(nextRectangle.X, nextRectangle.Y + 1, nextRectangle.Width, nextRectangle.Height), Color.Transparent, otherBaseColor, 90f);
			linearGradientBrush.SetSigmaBellShape(0.5f, 0.95f);
			pen = new Pen(linearGradientBrush);
			graphics.DrawLine(pen, nextRectangle.Right, nextRectangle.Y + 2, nextRectangle.Right, nextRectangle.Bottom);
			pen.Dispose();
			linearGradientBrush.Dispose();
			Brush brush = new SolidBrush(baseColor);
			Pen otherPen = new Pen(baseColor);
			graphics.DrawLines(otherPen, new Point[3]
			{
				new Point(otherRectangle.X - 1, otherRectangle.Bottom),
				new Point(otherRectangle.X + otherRectangle.Width / 2, otherRectangle.Bottom),
				new Point(otherRectangle.X + otherRectangle.Width / 2, otherRectangle.Y + otherRectangle.Height / 2 - 1)
			});
			otherPen.Dispose();
			Point[] points = new Point[6]
			{
				new Point(otherRectangle.X, otherRectangle.Bottom),
				new Point(otherRectangle.X + otherRectangle.Width / 2 - 1, otherRectangle.Bottom),
				new Point(otherRectangle.X + otherRectangle.Width / 2, otherRectangle.Bottom - 1),
				new Point(otherRectangle.X + otherRectangle.Width / 2, otherRectangle.Y + otherRectangle.Height / 2),
				new Point(otherRectangle.X + otherRectangle.Width / 2 - 1, otherRectangle.Y + otherRectangle.Height / 2),
				new Point(otherRectangle.X, otherRectangle.Bottom - 1)
			};
			otherPen = new Pen(color);
			graphics.FillPolygon(brush, points);
			graphics.DrawPolygon(otherPen, points);
			otherPen.Dispose();
			brush.Dispose();
			graphicsPath.Dispose();
		}
	}
}
