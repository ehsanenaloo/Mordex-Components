using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane
	{

		private void DrawControlBackground(Graphics graphics, Rectangle area)
		{
			Brush backgroundBrush = _backgroundFormatStyle.GetBackBrush(base.ClientRectangle);
			if (backgroundBrush != null)
			{
				graphics.FillRectangle(backgroundBrush, area);
				_backgroundFormatStyle.DisposeDrawingObjects();
			}
			if (BackgroundImage != null)
			{
				switch (_backgroundImageDrawMode)
				{
				case ImageDrawMode.Centered:
					DrawCenteredImage(graphics, BackgroundImage, _contentBounds, area);
					break;
				case ImageDrawMode.Stretched:
					DrawStretchedImage(graphics, BackgroundImage, _contentBounds, area);
					break;
				case ImageDrawMode.Tiled:
					DrawTiledImage(graphics, BackgroundImage, _contentBounds, area);
					break;
				}
			}
			WatermarkImage.DrawImage(graphics, area);
		}

		private void DrawClientBackground(Graphics graphics, Rectangle backgroundBounds, Rectangle clipRect)
		{
			Rectangle visibleRect = Rectangle.Intersect(clipRect, backgroundBounds);
			Brush backgroundBrush = null;
			if (visibleRect.Width == 0 || visibleRect.Height == 0)
			{
				return;
			}
			TaskPaneFormatStyle taskPaneFormatStyle = CreateDefaultFormatStyle();
			taskPaneFormatStyle = taskPaneFormatStyle.MergeWith(_backgroundFormatStyle);
			backgroundBrush = taskPaneFormatStyle.GetBackBrush(base.ClientRectangle);
			if (backgroundBrush != null)
			{
				graphics.FillRectangle(backgroundBrush, visibleRect);
				_backgroundFormatStyle.DisposeDrawingObjects();
			}
			if (BackgroundImage != null)
			{
				switch (_backgroundImageDrawMode)
				{
				case ImageDrawMode.Centered:
					DrawCenteredImage(graphics, BackgroundImage, backgroundBounds, visibleRect);
					break;
				case ImageDrawMode.Stretched:
					DrawStretchedImage(graphics, BackgroundImage, backgroundBounds, visibleRect);
					break;
				case ImageDrawMode.Tiled:
					DrawTiledImage(graphics, BackgroundImage, backgroundBounds, visibleRect);
					break;
				}
			}
			DrawBorder(graphics);
			WatermarkImage.DrawImage(graphics, _contentBounds);
			if (backgroundBrush != null)
			{
				backgroundBrush.Dispose();
				backgroundBrush = null;
			}
		}

		private void DrawTiledImage(Graphics graphics, Image image, Rectangle bounds, Rectangle visibleBounds)
		{
			int i = bounds.X;
			for (int j = bounds.Y; j < bounds.Bottom; j += image.Height)
			{
				for (; i < bounds.Right; i += image.Width)
				{
					Rectangle destRect = new Rectangle(i, j, image.Width, image.Height);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
				}
				i = bounds.X;
			}
		}

		private void DrawCenteredImage(Graphics graphics, Image image, Rectangle bounds, Rectangle visibleBounds)
		{
			int x = bounds.X + (bounds.Width - image.Width) / 2;
			int y = bounds.Y + (bounds.Height - image.Height) / 2;
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
		}

		private void DrawStretchedImage(Graphics graphics, Image image, Rectangle destRect, Rectangle unusedRect)
		{
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
		}

		private void PaintContent(Graphics graphics, Rectangle clipBounds)
		{
			if (!_isLayoutValid)
			{
				LayoutContent(graphics);
			}
			PaintGroups(graphics, clipBounds);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (e.ClipRectangle == Rectangle.Empty || _isPainting)
			{
				base.OnPaint(e);
				return;
			}
			CompleteGroupAnimation();
			_isPainting = true;
			try
			{
				using Region clip = e.Graphics.Clip;
				_contentBounds = base.ClientRectangle;
				bool isHandled = false;
				if (IsAreaOwnerDrawn(OwnerDrawnArea.Background))
				{
					DrawTaskPaneAreaEventArgs drawEventArgs = new DrawTaskPaneAreaEventArgs(e.Graphics, this, TaskPaneArea.Background, _contentBounds, null);
					OnDrawTaskPaneArea(drawEventArgs);
					isHandled = drawEventArgs.Handled;
				}
				if (!isHandled)
				{
					DrawClientBackground(e.Graphics, _contentBounds, e.ClipRectangle);
				}
				using (Region clientRegion = new Region(_contentBounds))
				{
					e.Graphics.Clip = clientRegion;
					PaintContent(e.Graphics, e.ClipRectangle);
				}
				e.Graphics.Clip = clip;
				DrawBorder(e.Graphics);
				base.OnPaint(e);
			}
			finally
			{
				_isPainting = false;
			}
		}

		internal void DrawBorder(Graphics graphics)
		{
			if (ComputeBorderStyle() == BorderStyle.None)
			{
				return;
			}
			TaskPaneFormatStyle taskPaneFormatStyle = CreateDefaultFormatStyle();
			taskPaneFormatStyle = taskPaneFormatStyle.MergeWith(_backgroundFormatStyle);
			Color backColor = taskPaneFormatStyle.BackColor;
			_contentBounds = TaskPanePaintHelper.DrawBorder(graphics, backColor, FlatBorderColor, base.ClientRectangle, ComputeBorderStyle());
		}
	}
}
