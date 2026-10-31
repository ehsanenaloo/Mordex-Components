using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class CornerShadowRenderer
	{
		private Bitmap _bottomLeftCorner;

		private Bitmap _gridRenderer2Bitmap;

		private Bitmap _bottomRightCorner;

		internal CornerShadowRenderer()
		{
		}

		internal void DrawShadow(Graphics graphics, Rectangle rectangle)
		{
			Color.FromArgb(128, Color.Black);
			Color.FromArgb(24, Color.Black);
			Color.FromArgb(0, Color.Black);
			graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Right, rectangle.Top, 5, 5), image: GetTopRightCornerImage());
			int alpha = 80;
			for (int i = 0; i < 5; i++)
			{
				Brush brush = GdiCache.Brush(Color.FromArgb(alpha, Color.Black));
				if (rectangle.Height > 5)
				{
					graphics.FillRectangle(brush, rectangle.Right + i, rectangle.Y + 5, 1, rectangle.Height - 5);
				}
				if (rectangle.Width > 5)
				{
					graphics.FillRectangle(brush, rectangle.X + 5, rectangle.Bottom + i, rectangle.Width - 5, 1);
				}
				alpha -= 12;
			}
			graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Left, rectangle.Bottom, 5, 5), image: GetBottomLeftCornerImage());
			graphics.DrawImageUnscaled(rect: new Rectangle(rectangle.Right, rectangle.Bottom, 5, 5), image: GetBottomRightCornerImage());
		}

		internal void DisposeResources()
		{
			if (_bottomLeftCorner != null)
			{
				_bottomLeftCorner.Dispose();
			}
			if (_bottomRightCorner != null)
			{
				_bottomRightCorner.Dispose();
			}
			if (_gridRenderer2Bitmap != null)
			{
				_gridRenderer2Bitmap.Dispose();
			}
		}

		private Image GetBottomLeftCornerImage()
		{
			if (_bottomLeftCorner == null)
			{
				CreateBottomLeftCorner();
			}
			return _bottomLeftCorner;
		}

		private Image GetTopRightCornerImage()
		{
			if (_gridRenderer2Bitmap == null)
			{
				CreateTopRightCorner();
			}
			return _gridRenderer2Bitmap;
		}

		private Image GetBottomRightCornerImage()
		{
			if (_bottomRightCorner == null)
			{
				CreateBottomRightCorner();
			}
			return _bottomRightCorner;
		}

		private void CreateBottomLeftCorner()
		{
			Bitmap bitmap = new Bitmap(5, 5);
			int value = 77;
			int otherValue = 12;
			for (int j = 4; j >= 0; j--)
			{
				int alpha = value;
				for (int i = 0; i < 5; i++)
				{
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(j, i, color);
					alpha -= otherValue;
				}
				value -= 12;
				otherValue -= 2;
			}
			_bottomLeftCorner = bitmap;
		}

		private void CreateTopRightCorner()
		{
			Bitmap bitmap = new Bitmap(5, 5);
			int value = 77;
			int otherValue = 12;
			for (int j = 4; j >= 0; j--)
			{
				int alpha = value;
				for (int i = 0; i < 5; i++)
				{
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(i, j, color);
					alpha -= otherValue;
				}
				value -= 12;
				otherValue -= 2;
			}
			_gridRenderer2Bitmap = bitmap;
		}

		private void CreateBottomRightCorner()
		{
			Bitmap bitmap = new Bitmap(5, 5);
			int value = 77;
			int otherValue = 12;
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					int alpha = value - otherValue * j;
					Color color = Color.FromArgb(alpha, Color.Black);
					bitmap.SetPixel(j, i, color);
				}
				value -= 12;
				otherValue -= 2;
			}
			_bottomRightCorner = bitmap;
		}
	}
}
