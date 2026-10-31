using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Timeline.Internal
{
	internal static class TimelineRenderer
	{
		private static Bitmap _hotCollapsedBitmap;

		private static Bitmap _hotExpandedBitmap;

		internal static void DrawImage(Graphics graphics, Rectangle rectangle)
		{
			if (_hotCollapsedBitmap == null)
			{
				_hotCollapsedBitmap = CreateHotCollapsedBitmap();
			}
			graphics.DrawImageUnscaled(_hotCollapsedBitmap, new Point(rectangle.X + rectangle.Width / 2 - 8, rectangle.Y + rectangle.Height / 2 - 8));
		}

		internal static void DrawHotExpandedImage(Graphics graphics, Rectangle rectangle)
		{
			if (_hotExpandedBitmap == null)
			{
				_hotExpandedBitmap = CreateHotExpandedBitmap();
			}
			graphics.DrawImageUnscaled(_hotExpandedBitmap, new Point(rectangle.X + rectangle.Width / 2 - 8, rectangle.Y + rectangle.Height / 2 - 8));
		}

		private static void DrawBackground(Graphics graphics, Rectangle rectangleArgument)
		{
			Rectangle rectangle = rectangleArgument;
			Rectangle arrowBounds = new Rectangle(0, 0, 8, 8);
			arrowBounds.X = rectangleArgument.X + (rectangleArgument.Width - arrowBounds.Width) / 2;
			arrowBounds.Y = rectangleArgument.Y + (rectangleArgument.Height - arrowBounds.Height) / 2 - 1;
			rectangleArgument = arrowBounds;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.StartFigure();
			graphicsPath.AddLine(new Point(rectangleArgument.X, rectangleArgument.Y), new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Y + rectangleArgument.Height / 2));
			graphicsPath.AddLine(new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Y + rectangleArgument.Height / 2), new Point(rectangleArgument.X, rectangleArgument.Bottom));
			graphicsPath.AddLine(new Point(rectangleArgument.X, rectangleArgument.Bottom), new Point(rectangleArgument.X, rectangleArgument.Y));
			graphicsPath.CloseFigure();
			Color color = Color.FromArgb(255, 134, 26);
			Color fillColor = Color.FromArgb(255, 177, 26);
			Color centerColor = Color.FromArgb(255, 226, 185);
			GraphicsPath path = CreateRightArrowPath(new Point(rectangleArgument.X - 1, rectangleArgument.Y + rectangleArgument.Height / 2), 16);
			GraphicsPath glowPath = new GraphicsPath();
			glowPath.AddEllipse(new Rectangle(rectangle.X, rectangle.Y, 10, 16));
			PathGradientBrush pathGradientBrush = new PathGradientBrush(glowPath);
			pathGradientBrush.CenterPoint = new PointF(rectangleArgument.X, rectangleArgument.Y + rectangleArgument.Height / 2 - 1);
			pathGradientBrush.CenterColor = centerColor;
			pathGradientBrush.SurroundColors = new Color[1] { Color.Transparent };
			Blend blend = new Blend();
			blend.Positions = new float[3] { 1f, 1f, 0.25f };
			blend.Factors = new float[3] { 0f, 0.75f, 1f };
			pathGradientBrush.FocusScales = new PointF(0.33f, 0.33f);
			Region clip = graphics.Clip;
			Region glowClip = new Region(new Rectangle(rectangleArgument.X - 1, rectangleArgument.Y - 1, 7, 11));
			graphics.Clip = glowClip;
			graphics.FillPath(pathGradientBrush, path);
			pathGradientBrush.Dispose();
			graphics.Clip = clip;
			Pen pen = new Pen(color);
			Brush brush = new SolidBrush(fillColor);
			graphics.FillPath(brush, graphicsPath);
			graphics.DrawPath(pen, graphicsPath);
			pen.Dispose();
			brush.Dispose();
		}

		private static GraphicsPath CreateRightArrowPath(Point point, int size)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.StartFigure();
			graphicsPath.AddLine(new Point(point.X, point.Y - size / 2), new Point(point.X + size / 2, point.Y));
			graphicsPath.AddLine(new Point(point.X + size / 2, point.Y), new Point(point.X, point.Y + size / 2));
			graphicsPath.AddLine(new Point(point.X, point.Y + size / 2), new Point(point.X, point.Y - size / 2));
			graphicsPath.CloseFigure();
			return graphicsPath;
		}

		private static Bitmap CreateHotCollapsedBitmap()
		{
			Bitmap bitmap = new Bitmap(16, 16);
			Rectangle rectangle = new Rectangle(0, 0, 16, 16);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.FillRectangle(Brushes.Transparent, rectangle);
			DrawBackground(graphics, rectangle);
			graphics.Dispose();
			return bitmap;
		}

		private static Bitmap CreateHotExpandedBitmap()
		{
			Bitmap bitmap = new Bitmap(16, 16);
			Rectangle rectangle = new Rectangle(0, 0, 16, 16);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.FillRectangle(Brushes.Transparent, rectangle);
			DrawHotExpandedArrow(graphics, rectangle);
			graphics.Dispose();
			return bitmap;
		}

		private static void DrawHotExpandedArrow(Graphics graphics, Rectangle rectangleArgument)
		{
			Rectangle rectangle = new Rectangle(0, 0, 8, 8);
			rectangle.X = rectangleArgument.X + (rectangleArgument.Width - rectangle.Width) / 2;
			rectangle.Y = rectangleArgument.Y + (rectangleArgument.Height - rectangle.Height) / 2 - 1;
			rectangleArgument = rectangle;
			Color color = Color.FromArgb(255, 134, 26);
			Color fillColor = Color.FromArgb(255, 177, 26);
			Color highlightColor = Color.FromArgb(255, 255, 226, 185);
			Rectangle highlightBounds = new Rectangle(rectangleArgument.X - 4, rectangleArgument.Y, 9, 9);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddPolygon(new Point[3]
			{
				new Point(highlightBounds.X, highlightBounds.Bottom),
				new Point(highlightBounds.Right, highlightBounds.Bottom),
				new Point(highlightBounds.Right, highlightBounds.Y)
			});
			PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.CenterColor = Color.FromArgb(192, fillColor);
			pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(80, highlightColor) };
			graphics.FillPath(pathGradientBrush, graphicsPath);
			pathGradientBrush.Dispose();
			pathGradientBrush = null;
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(highlightBounds.X + 1, highlightBounds.Y, highlightBounds.Width, highlightBounds.Height), Color.Transparent, highlightColor, 0f);
			linearGradientBrush.SetSigmaBellShape(0.5f, 0.95f);
			Pen pen = new Pen(linearGradientBrush);
			graphics.DrawLine(pen, highlightBounds.X + 3, highlightBounds.Bottom, highlightBounds.Right - 1, highlightBounds.Bottom);
			pen.Dispose();
			linearGradientBrush.Dispose();
			linearGradientBrush = new LinearGradientBrush(new Rectangle(highlightBounds.X, highlightBounds.Y + 1, highlightBounds.Width, highlightBounds.Height), Color.Transparent, highlightColor, 90f);
			linearGradientBrush.SetSigmaBellShape(0.5f, 0.95f);
			pen = new Pen(linearGradientBrush);
			graphics.DrawLine(pen, highlightBounds.Right, highlightBounds.Y + 2, highlightBounds.Right, highlightBounds.Bottom);
			pen.Dispose();
			linearGradientBrush.Dispose();
			Brush brush = new SolidBrush(fillColor);
			Pen linePen = new Pen(fillColor);
			graphics.DrawLines(linePen, new Point[3]
			{
				new Point(rectangleArgument.X - 1, rectangleArgument.Bottom),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Bottom),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Y + rectangleArgument.Height / 2 - 1)
			});
			linePen.Dispose();
			Point[] points = new Point[6]
			{
				new Point(rectangleArgument.X, rectangleArgument.Bottom),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2 - 1, rectangleArgument.Bottom),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Bottom - 1),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2, rectangleArgument.Y + rectangleArgument.Height / 2),
				new Point(rectangleArgument.X + rectangleArgument.Width / 2 - 1, rectangleArgument.Y + rectangleArgument.Height / 2),
				new Point(rectangleArgument.X, rectangleArgument.Bottom - 1)
			};
			linePen = new Pen(color);
			graphics.FillPolygon(brush, points);
			graphics.DrawPolygon(linePen, points);
			linePen.Dispose();
			brush.Dispose();
		}
	}
}
