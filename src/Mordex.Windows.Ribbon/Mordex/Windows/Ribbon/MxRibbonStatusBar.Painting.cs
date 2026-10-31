using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonStatusBar
	{
		protected override void OnInvalidated(InvalidateEventArgs e)
		{
			base.OnInvalidated(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			RenderContent(e.Graphics, e.ClipRectangle);
			base.OnPaint(e);
		}

		private void RenderContent(Graphics graphics, Rectangle rectangle)
		{
			using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			if (_isLayoutDirty)
			{
				LayoutPanels(graphics, textLayoutRenderer, true);
			}
			Pen pen = GdiCache.Pen(_colorScheme._statusBarTopLine);
			graphics.DrawLine(pen, base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Right - 1, base.ClientRectangle.Y);
			Pen highlightPen = GdiCache.Pen(_colorScheme._statusBarTopHighlight);
			graphics.DrawLine(highlightPen, base.ClientRectangle.X, base.ClientRectangle.Y + 1, base.ClientRectangle.Right - 1, base.ClientRectangle.Y + 1);
			_leftPanelRenderer.PaintPanel(graphics, textLayoutRenderer, _colorScheme, Font, rectangle);
			_rightPanelRenderer.PaintPanel(graphics, textLayoutRenderer, _colorScheme, Font, rectangle);
		}

		internal void DrawPanelBackgrounds(Graphics graphics, Rectangle bounds, int offsetX)
		{
			if (_isLayoutDirty)
			{
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				LayoutPanels(graphics, textLayoutRenderer, false);
			}
			try
			{
				if (_leftPanelRenderer != null)
				{
					Rectangle rectangle = new Rectangle(bounds.X + offsetX, bounds.Y, _leftPanelRenderer._bounds.Width, bounds.Height);
					_leftPanelRenderer.FillPanelBackground(graphics, _colorScheme, rectangle);
				}
				if (_rightPanelRenderer != null)
				{
					Rectangle rightPanelRect = new Rectangle(bounds.X + _leftPanelRenderer._bounds.Width + offsetX, bounds.Y, _rightPanelRenderer._bounds.Width, bounds.Height);
					_rightPanelRenderer.FillPanelBackground(graphics, _colorScheme, rightPanelRect);
				}
			}
			catch (System.Runtime.InteropServices.ExternalException)
			{
				// GDI+ can fail while the window is resized or closed; the next paint redraws it.
			}
		}

	}
}
