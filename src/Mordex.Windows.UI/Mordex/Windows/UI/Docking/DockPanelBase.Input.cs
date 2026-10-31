using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public abstract partial class DockPanelBase
	{
		internal void HandleCaptionDoubleClick()
		{
			if (_captionRenderer == null)
			{
				return;
			}
			Point pt = PointToClient(Cursor.Position);
			if (AutoHide || !_captionRenderer.GetCaptionBounds().Contains(pt))
			{
				return;
			}
			if ((ParentGroup == null || ((!ParentGroup.StaticGroup || ParentGroup.IsTabGroup()) && !ParentGroup.HandleCaptionDoubleClick(this))) && GetEffectiveCaptionDoubleClickAction() == CaptionDoubleClickAction.ToggleDockState)
			{
				if (IsFloating())
				{
					DockPanel();
				}
				else
				{
					UndockPanel();
				}
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			HandleCaptionDoubleClick();
			base.OnDoubleClick(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (_captionRenderer != null && _captionRenderer.HandleMouseDown(e))
			{
				return;
			}
			if (e.Button == MouseButtons.Left)
			{
				_resizeMode = GetResizeModeAt(e.X, e.Y);
				if (_resizeMode != ResizeMode.NoResize)
				{
					BeginSplitterDrag(e.X, e.Y);
				}
				if (_captionRenderer != null && _captionRenderer.GetCaptionBounds().Contains(e.X, e.Y))
				{
					Activate();
					if (CanDragPanel())
					{
						Size dragSize = SystemInformation.DragSize;
						_dragStartBounds = new Rectangle(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2, dragSize.Width, dragSize.Height);
					}
				}
			}
			else if (e.Button == MouseButtons.Right && _captionRenderer != null && _captionRenderer.GetCaptionBounds().Contains(e.X, e.Y))
			{
				Activate();
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_captionRenderer == null || !_captionRenderer.HandleMouseMove(e))
			{
				if (!_dragStartBounds.IsEmpty && !_dragStartBounds.Contains(e.X, e.Y))
				{
					StartPanelDrag();
				}
				if (_resizeMode != ResizeMode.NoResize)
				{
					MoveSplitterTrack(e.X, e.Y);
				}
				else
				{
					base.OnMouseMove(e);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (_captionRenderer == null || !_captionRenderer.HandleMouseUp(e))
			{
				if (_resizeMode != ResizeMode.NoResize)
				{
					EndSplitterDrag(e.X, e.Y, false);
				}
				_dragStartBounds = Rectangle.Empty;
				RenderingHelper.SetMouseCapture(this, false);
				base.OnMouseUp(e);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (_captionRenderer != null)
			{
				_captionRenderer.ClearHotItem();
			}
			base.OnMouseLeave(e);
		}

		protected override void OnEnter(EventArgs e)
		{
			RefreshLayout();
			base.OnEnter(e);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			Invalidate();
			if (ParentGroup != null)
			{
				ParentGroup.Invalidate();
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			RefreshLayout();
			base.OnLeave(e);
		}

		private void StartPanelDrag()
		{
			if (ParentGroup != null && ParentGroup.ShouldDragWholeGroup())
			{
				if (ParentGroup.ParentGroup != null)
				{
					ParentGroup.StartPanelDrag();
				}
				else if (ParentGroup.CanDragPanel())
				{
					PanelManager.StartPanelDrag(ParentGroup, _dragStartBounds.X + _dragStartBounds.Width / 2, _dragStartBounds.Y + _dragStartBounds.Height / 2);
				}
			}
			else
			{
				PanelManager.StartPanelDrag(this, _dragStartBounds.X + _dragStartBounds.Width / 2, _dragStartBounds.Y + _dragStartBounds.Height / 2);
			}
			_dragStartBounds = Rectangle.Empty;
		}

		private void OnContainerCaptionControlMouseLeave(object sender, EventArgs eventArgs)
		{
			OnMouseLeave(eventArgs);
		}

		private void OnContainerCaptionControlMouseMove(object sender, MouseEventArgs eventArgs)
		{
			MouseEventArgs e = RenderingHelper.TranslateMouseEvent(sender as Control, this, eventArgs);
			OnMouseMove(e);
		}

		private void OnContainerCaptionControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			MouseEventArgs e = RenderingHelper.TranslateMouseEvent(sender as Control, this, eventArgs);
			OnMouseUp(e);
		}

		private void OnContainerCaptionControlDoubleClick(object sender, EventArgs eventArgs)
		{
			OnDoubleClick(eventArgs);
		}

		private void OnContainerCaptionControlMouseDown(object sender, MouseEventArgs eventArgs)
		{
			MouseEventArgs e = RenderingHelper.TranslateMouseEvent(sender as Control, this, eventArgs);
			OnMouseDown(e);
		}
	}
}
