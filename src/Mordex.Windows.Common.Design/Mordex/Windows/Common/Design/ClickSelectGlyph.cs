using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Common.Design
{
	// The Design Tools Server delivers mouse input to glyph behaviors, not to the control's WndProc.
	// This glyph covers a control and reacts to the mouse over the parts the designer cares about.
	// Mouse down and mouse up both run the callbacks; a callback returns true when it handled the click.
	[CLSCompliant(false)]
	public class ClickSelectGlyph : Glyph
	{
		private readonly BehaviorService behaviorService;

		private readonly Control owner;

		private readonly Func<Point, bool> canSelect;

		public ClickSelectGlyph(BehaviorService behaviorService, Control owner, Func<Point, bool> canSelect, Func<Point, MouseButtons, bool> select)
			: this(behaviorService, owner, canSelect, select, null)
		{
		}

		public ClickSelectGlyph(BehaviorService behaviorService, Control owner, Func<Point, bool> canSelect, Func<Point, MouseButtons, bool> select, Func<Point, MouseButtons, bool> mouseUp)
			: base(new ClickSelectBehavior(behaviorService, owner, select, mouseUp))
		{
			this.behaviorService = behaviorService;
			this.owner = owner;
			this.canSelect = canSelect;
		}

		public override Rectangle Bounds => new Rectangle(behaviorService.ControlToAdornerWindow(owner), owner.ClientSize);

		public override void Paint(PaintEventArgs pe)
		{
		}

		public override Cursor GetHitTest(Point pt)
		{
			Point origin = behaviorService.ControlToAdornerWindow(owner);
			if (Bounds.Contains(pt) && canSelect(new Point(pt.X - origin.X, pt.Y - origin.Y)))
			{
				return Cursors.Default;
			}
			return null;
		}
	}

	[CLSCompliant(false)]
	public class ClickSelectBehavior : Behavior
	{
		private readonly BehaviorService behaviorService;

		private readonly Control owner;

		private readonly Func<Point, MouseButtons, bool> select;

		private readonly Func<Point, MouseButtons, bool> mouseUp;

		public ClickSelectBehavior(BehaviorService behaviorService, Control owner, Func<Point, MouseButtons, bool> select, Func<Point, MouseButtons, bool> mouseUp)
		{
			this.behaviorService = behaviorService;
			this.owner = owner;
			this.select = select;
			this.mouseUp = mouseUp;
		}

		private Point ToClient(Point mouseLoc)
		{
			Point origin = behaviorService.ControlToAdornerWindow(owner);
			return new Point(mouseLoc.X - origin.X, mouseLoc.Y - origin.Y);
		}

		public override bool OnMouseDown(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			if (select == null || (button != MouseButtons.Left && button != MouseButtons.Right))
			{
				return false;
			}
			return select(ToClient(mouseLoc), button);
		}

		public override bool OnMouseUp(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			if (mouseUp == null || (button != MouseButtons.Left && button != MouseButtons.Right))
			{
				return false;
			}
			return mouseUp(ToClient(mouseLoc), button);
		}
	}

	// Forwards a mouse gesture (down, move, up) to the real control, for controls that handle drags themselves,
	// such as dragging a DataGrid column header.
	[CLSCompliant(false)]
	public class MouseForwardGlyph : Glyph
	{
		private readonly BehaviorService behaviorService;

		private readonly Control owner;

		private readonly Func<Point, bool> canForward;

		private readonly MouseForwardBehavior behavior;

		public MouseForwardGlyph(BehaviorService behaviorService, Control owner, Func<Point, bool> canForward, Action completed)
			: this(behaviorService, owner, owner, canForward, completed)
		{
		}

		// starting runs before the gesture is forwarded; when it returns false the gesture is not forwarded.
		public MouseForwardGlyph(BehaviorService behaviorService, Control owner, Func<Point, bool> canForward, Func<bool> starting, Action completed)
			: this(behaviorService, owner, canForward, new MouseForwardBehavior(behaviorService, owner, owner, starting, completed))
		{
		}

		// Mouse points are relative to owner; they reach target, which must be owner or a direct child of it.
		public MouseForwardGlyph(BehaviorService behaviorService, Control owner, Control target, Func<Point, bool> canForward, Action completed)
			: this(behaviorService, owner, canForward, new MouseForwardBehavior(behaviorService, owner, target, completed))
		{
		}

		private MouseForwardGlyph(BehaviorService behaviorService, Control owner, Func<Point, bool> canForward, MouseForwardBehavior behavior)
			: base(behavior)
		{
			this.behaviorService = behaviorService;
			this.owner = owner;
			this.canForward = canForward;
			this.behavior = behavior;
		}

		public override Rectangle Bounds => new Rectangle(behaviorService.ControlToAdornerWindow(owner), owner.ClientSize);

		public override void Paint(PaintEventArgs pe)
		{
		}

		public override Cursor GetHitTest(Point pt)
		{
			// During a gesture the glyph keeps the mouse, so the release reaches the behavior wherever it happens.
			if (behavior.Forwarding)
			{
				return Cursors.Default;
			}
			Point origin = behaviorService.ControlToAdornerWindow(owner);
			if (Bounds.Contains(pt) && canForward(new Point(pt.X - origin.X, pt.Y - origin.Y)))
			{
				return Cursors.Default;
			}
			return null;
		}
	}

	[CLSCompliant(false)]
	public class MouseForwardBehavior : Behavior
	{
		private const int WM_MOUSEMOVE = 0x200;

		private const int WM_LBUTTONDOWN = 0x201;

		private const int WM_LBUTTONUP = 0x202;

		private const int MK_LBUTTON = 1;

		private readonly BehaviorService behaviorService;

		private readonly Control owner;

		private readonly Control target;

		private readonly Action completed;

		private bool forwarding;

		public MouseForwardBehavior(BehaviorService behaviorService, Control owner, Action completed)
			: this(behaviorService, owner, owner, completed)
		{
		}

		public MouseForwardBehavior(BehaviorService behaviorService, Control owner, Control target, Action completed)
			: this(behaviorService, owner, target, null, completed)
		{
		}

		public MouseForwardBehavior(BehaviorService behaviorService, Control owner, Control target, Func<bool> starting, Action completed)
		{
			this.behaviorService = behaviorService;
			this.owner = owner;
			this.target = target;
			this.starting = starting;
			this.completed = completed;
		}

		private readonly Func<bool> starting;

		public bool Forwarding => forwarding;

		private void Send(int message, Point mouseLoc, int keys)
		{
			Point origin = behaviorService.ControlToAdornerWindow(owner);
			Point offset = target == owner ? Point.Empty : target.Location;
			MouseForwarder.Send(target, message, new Point(mouseLoc.X - origin.X - offset.X, mouseLoc.Y - origin.Y - offset.Y), keys);
		}

		private Point lastLocation;

		private Timer releaseWatcher;

		// The Design Tools Server does not always report the mouse release to the behavior, so the real button state is polled too.
		private void Release()
		{
			if (!forwarding)
			{
				return;
			}
			forwarding = false;
			StopReleaseWatcher();
			Send(WM_LBUTTONUP, lastLocation, 0);
			PointerPosition.ClearOverride();
			completed?.Invoke();
		}

		private void StopReleaseWatcher()
		{
			if (releaseWatcher != null)
			{
				releaseWatcher.Stop();
				releaseWatcher.Tick -= WatchRelease;
				releaseWatcher.Dispose();
				releaseWatcher = null;
			}
		}

		private void WatchRelease(object sender, EventArgs e)
		{
			if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.None)
			{
				Release();
			}
		}

		public override bool OnMouseDown(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			if (button != MouseButtons.Left)
			{
				return false;
			}
			if (!forwarding && starting != null && !starting())
			{
				return false;
			}
			forwarding = true;
			lastLocation = mouseLoc;
			if (releaseWatcher == null)
			{
				releaseWatcher = new Timer { Interval = 40 };
				releaseWatcher.Tick += WatchRelease;
			}
			releaseWatcher.Start();
			Send(WM_LBUTTONDOWN, mouseLoc, MK_LBUTTON);
			return true;
		}

		public override bool OnMouseMove(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			if (!forwarding)
			{
				return false;
			}
			lastLocation = mouseLoc;
			Send(WM_MOUSEMOVE, mouseLoc, MK_LBUTTON);
			return true;
		}

		public override bool OnMouseUp(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			if (!forwarding)
			{
				return false;
			}
			lastLocation = mouseLoc;
			Release();
			return true;
		}
	}

	public static class MouseForwarder
	{
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public static bool Send(Control owner, int message, Point client, int keys)
		{
			if (!owner.IsHandleCreated)
			{
				return false;
			}
			PointerPosition.SetOverride(owner.PointToScreen(client));
			SendMessage(owner.Handle, (uint)message, (IntPtr)keys, (IntPtr)((client.Y << 16) | (client.X & 0xFFFF)));
			return true;
		}
	}
}
