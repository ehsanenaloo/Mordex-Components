using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonStatusBar
	{
		internal HitTest GetHitTest(int x, int y)
		{
			if (!_sizeGripBounds.IsEmpty && _sizeGripBounds.Contains(x, y))
			{
				return HitTest.StatusBarResizePanel;
			}
			if (_leftPanelRenderer != null)
			{
				CommandBaseRenderer commandBaseRenderer = _leftPanelRenderer.FindItemRendererAt(x, y);
				if (commandBaseRenderer != null)
				{
					return HitTest.Command;
				}
			}
			if (_rightPanelRenderer != null)
			{
				CommandBaseRenderer rightRenderer = _rightPanelRenderer.FindItemRendererAt(x, y);
				if (rightRenderer != null)
				{
					return HitTest.Command;
				}
			}
			return HitTest.None;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHotRenderer(null);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (_resizeStartPoint != Point.Empty)
			{
				if (base.Capture)
				{
					Point position = Cursor.Position;
					Size size = new Size(_resizeStartPoint.X - position.X, _resizeStartPoint.Y - position.Y);
					Control control = RibbonDrawingUtils.GetParentControl(this);
					if (control == null)
					{
						return;
					}
					size = (control.Size = new Size(control.Size.Width - size.Width, control.Size.Height - size.Height));
					if (control.Size.Width != size.Width)
					{
						position.X += control.Size.Width - size.Width;
					}
					if (control.Size.Height != size.Height)
					{
						position.Y += control.Size.Height - size.Height;
					}
					_resizeStartPoint = position;
					control.Update();
				}
				else
				{
					_resizeStartPoint = Point.Empty;
				}
			}
			else if (!_leftPanelRenderer.HandleMouseMove(e) && !_rightPanelRenderer.HandleMouseMove(e))
			{
				SetHotRenderer(null);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (_leftPanelRenderer.HandleMouseUp(e))
			{
				SetPushedRenderer(null);
			}
			else if (_rightPanelRenderer.HandleMouseUp(e))
			{
				SetPushedRenderer(null);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left && _sizeGripBounds.Contains(e.X, e.Y))
			{
				_resizeStartPoint = Cursor.Position;
				_resizeStartSize = RibbonDrawingUtils.GetParentControl(this).Size;
			}
			else if (!_leftPanelRenderer.HandleMouseDown(e))
			{
				_rightPanelRenderer.HandleMouseDown(e);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Escape && !_resizeStartPoint.IsEmpty)
			{
				_resizeStartPoint = Point.Empty;
				if (!_resizeStartSize.IsEmpty)
				{
					RibbonDrawingUtils.GetParentControl(this).Size = _resizeStartSize;
				}
				_resizeStartSize = Size.Empty;
			}
			return base.ProcessDialogKey(keyData);
		}
	}
}
