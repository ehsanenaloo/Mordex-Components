using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane
{
	[ToolboxItem(false)]
	internal class TaskPaneControl : Control
	{
		private Container _container = new Container();

		private MxTaskPane _taskPane;

		internal int _borderInset;

		internal TaskPaneControl(MxTaskPane taskPane)
		{
			SetContainer();
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.DoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			base.TabStop = false;
			_taskPane = taskPane;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		private void SetContainer()
		{
			_container = new Container();
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			UpdateBorderRegion();
		}

		// WM_PRINT ignores the window region, so keep painting inside the border frame.
		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
			GraphicsState state = eventArgs.Graphics.Save();
			try
			{
				ClipToRegion(eventArgs.Graphics);
				base.OnPaintBackground(eventArgs);
			}
			finally
			{
				eventArgs.Graphics.Restore(state);
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			if (_taskPane != null)
			{
				GraphicsState state = eventArgs.Graphics.Save();
				try
				{
					ClipToRegion(eventArgs.Graphics);
					_taskPane.DrawBorder(eventArgs.Graphics);
				}
				finally
				{
					eventArgs.Graphics.Restore(state);
				}
			}
		}

		private void ClipToRegion(Graphics graphics)
		{
			if (base.Region != null)
			{
				graphics.SetClip(base.Region, CombineMode.Intersect);
			}
		}

		private void UpdateBorderRegion()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Region borderRegion = new Region(clientRectangle);
			clientRectangle.Inflate(_borderInset, _borderInset);
			borderRegion.Exclude(clientRectangle);
			base.Region = borderRegion;
		}
	}
}
