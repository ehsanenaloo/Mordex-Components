using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class HeaderDragFeedback
	{
		internal int _dropIndex = -1;

		internal MxRowLayoutState _mordexRowLayoutState;

		internal GridGroupLayoutState _gridGroupLayoutState;

		private Rectangle _feedbackBounds;

		private Bitmap _feedbackImage;

		private Bitmap _gridRenderer3Bitmap;

		private Point _lastLocation;

		private Bitmap _savedBackground;

		internal Point _gridRenderer3Point;

		private bool _isDropAllowed;

		public HeaderDragFeedback()
		{
		}

		internal HeaderDragFeedback(Graphics graphics, GridGroupLayoutState item, int width, int height)
		{
			if (width > Timing.MaxDragImageSize)
			{
				width = Timing.MaxDragImageSize;
			}
			if (height > Timing.MaxDragImageSize)
			{
				height = Timing.MaxDragImageSize;
			}
			RenderContent(graphics, item, width, height);
		}

		private void RenderContent(Graphics graphics, GridGroupLayoutState gridGroupLayoutState, int width, int height)
		{
			_gridGroupLayoutState = gridGroupLayoutState;
			_feedbackBounds = new Rectangle(0, 0, width, height);
			_gridRenderer3Bitmap = new Bitmap(width, height, PixelFormat.Format48bppRgb);
			_feedbackImage = new Bitmap(width, height, PixelFormat.Format48bppRgb);
		}

		internal virtual void Move()
		{
			int x = Cursor.Position.X;
			int y = Cursor.Position.Y;
			Point point = new Point(x - _gridRenderer3Point.X, y - _gridRenderer3Point.Y);
			_lastLocation = point;
		}

		public virtual Graphics CreateFeedbackGraphics()
		{
			Graphics graphics = Graphics.FromImage(_feedbackImage);
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			return graphics;
		}

		public virtual void EraseHeader()
		{
		}

		public virtual void EraseArrows()
		{
		}

		public virtual void DrawArrows(Rectangle bounds)
		{
		}

		public virtual void Dispose()
		{
			if (_feedbackImage != null)
			{
				_feedbackImage.Dispose();
				_feedbackImage = null;
			}
			if (_gridRenderer3Bitmap != null)
			{
				_gridRenderer3Bitmap.Dispose();
				_gridRenderer3Bitmap = null;
			}
			if (_savedBackground != null)
			{
				_savedBackground.Dispose();
				_savedBackground = null;
			}
		}

		public bool IsDropTargetValid()
		{
			return _isDropAllowed;
		}

		public void SetDropAllowed(bool condition)
		{
			if (condition != _isDropAllowed)
			{
				_isDropAllowed = condition;
				if (_isDropAllowed)
				{
					Cursor.Current = Cursors.Arrow;
					return;
				}
				EraseArrows();
				_dropIndex = -1;
				Cursor.Current = Cursors.No;
			}
		}
	}
}
