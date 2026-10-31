using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	/// <summary>
	/// Draws the quick access bar of a ribbon into the caption of the themed window frame. In the Office 2007 frame it also
	/// draws the application button, whose lower part the ribbon draws in its own client area. In the other custom frames the
	/// bar follows the icon of the window, as in Office 2010.
	/// </summary>
	internal sealed class RibbonCaptionExtension : IFrameCaptionExtension, ICaptionIconKeeper
	{
		private const int QuickAccessCell = 24;
		private const int BarCell = 22;

		private readonly MxRibbon _ribbon;
		private readonly MxThemedFormFrame _frame;
		private ButtonCommand _hotCommand;
		// The command that the press started on, and the commands that the bar showed then. The press runs only that command,
		// and only while the bar still shows the same commands.
		private ButtonCommand _pressedCommand;
		private List<ButtonCommand> _pressedCommands;

		internal RibbonCaptionExtension(MxRibbon ribbon, MxThemedFormFrame frame)
		{
			_ribbon = ribbon;
			_frame = frame;
		}

		internal MxThemedFormFrame Frame => _frame;

		/// <summary>True when the frame paints its own caption, holds this extension, and the orb or the bar is in it.</summary>
		internal bool IsActive => TryGetLift(out _, out _) || IsBarOnly;

		/// <summary>
		/// True when the caption shows only the quick access bar: a custom frame that is not the Office 2007 frame, a ribbon
		/// without its own caption bar whose bar lies above the ribbon, a window that is not mirrored, and at least one command.
		/// </summary>
		internal bool IsBarOnly
		{
			get
			{
				if (!ReferenceEquals(_frame.CaptionExtension, this) || !_frame.IsPainting || !_frame.CurrentStyle.IsCustom || _frame.CurrentStyle.Kind == FormFrameKind.Glass)
				{
					return false;
				}
				if (_ribbon.ShowCaptionBar || _ribbon.IsDisposed || !_ribbon.IsHandleCreated || _ribbon.QuickAccessPosition != QuickAccessPosition.AboveRibbon)
				{
					return false;
				}
				FormFrameMetrics metrics = _frame.GetMetricsWithoutExtension();
				Form form = _frame.Form;
				if (metrics == null || metrics.Mirrored || form == null || !ReferenceEquals(_ribbon.FindForm(), form) || !(_ribbon.Visible || !form.Visible))
				{
					return false;
				}
				return Commands().Count > 0;
			}
		}

		public bool KeepsIcon => !TryGetLift(out _, out _);

		private float Scale => _ribbon.DeviceDpi / 96f;

		private int S(int value) => (int)Math.Round(value * Scale);

		/// <summary>
		/// The lifted orb and the client origin of the ribbon, both in window coordinates. The orb sits in the corner of the
		/// window and reaches down into the ribbon, so it can be lifted only while the ribbon starts at the top of the client
		/// area, at its left edge, and spans the orb. Otherwise the ribbon draws its ordinary orb.
		/// </summary>
		internal bool TryGetLift(out Rectangle orb, out Point origin)
		{
			orb = Rectangle.Empty;
			origin = Point.Empty;
			if (!ReferenceEquals(_frame.CaptionExtension, this) || _frame.CurrentStyle.Kind != FormFrameKind.Glass || _ribbon.ShowCaptionBar || _ribbon.IsDisposed || !_ribbon.IsHandleCreated)
			{
				return false;
			}
			FormFrameMetrics metrics = _frame.GetMetricsWithoutExtension();
			Form form = _frame.Form;
			if (metrics == null || form == null || !ReferenceEquals(_ribbon.FindForm(), form) || !(_ribbon.Visible || !form.Visible))
			{
				return false;
			}
			origin = _frame.ScreenToWindow(_ribbon.PointToScreen(Point.Empty));
			int size = S(42);
			orb = new Rectangle(metrics.Visible.X + S(5), metrics.Visible.Y + S(5), size, size);
			return origin == metrics.Client.Location && origin.X + _ribbon.ClientSize.Width >= orb.Right;
		}

		private List<ButtonCommand> Commands()
		{
			List<ButtonCommand> list = new List<ButtonCommand>();
			foreach (CommandBase command in _ribbon.QuickAccessCommands)
			{
				if (command is ButtonCommand button && button.Visible)
				{
					list.Add(button);
				}
			}
			return list;
		}

		// The application button in window coordinates, or empty when it is not lifted.
		private Rectangle OrbBounds => TryGetLift(out Rectangle orb, out _) ? orb : Rectangle.Empty;

		internal bool OrbHot { get; set; }

		private Rectangle QuickAccessBounds(Rectangle caption, int count)
		{
			if (count == 0)
			{
				return Rectangle.Empty;
			}
			if (!TryGetLift(out _, out _))
			{
				// After the icon, centered in the caption: flat cells with no capsule, as in Office 2010.
				FormFrameMetrics metrics = _frame.GetMetricsWithoutExtension();
				if (metrics == null)
				{
					return Rectangle.Empty;
				}
				int barTop = metrics.Title.Y;
				int barHeight = metrics.Title.Height;
				int start = metrics.Icon.IsEmpty ? metrics.Visible.X + metrics.Border + S(6) : metrics.Icon.Right + S(6);
				int height = Math.Min(S(22), barHeight);
				return new Rectangle(start, barTop + (barHeight - height) / 2, count * S(BarCell), height);
			}
			Rectangle orb = OrbBounds;
			int x = orb.Right - S(10);
			int y = caption.Y + S(7);
			return new Rectangle(x, y, count * S(QuickAccessCell) + S(20), S(24));
		}

		// The cell of the command at the given place in the bar.
		private Rectangle CellBounds(Rectangle quick, int index)
		{
			if (!TryGetLift(out _, out _))
			{
				return new Rectangle(quick.X + index * S(BarCell), quick.Y, S(BarCell), quick.Height);
			}
			int cell = S(QuickAccessCell);
			return new Rectangle(quick.X + S(14) + index * cell, quick.Y + 1, cell, quick.Height - 2);
		}

		private Rectangle Caption => _frame.GetMetricsWithoutExtension()?.Caption ?? Rectangle.Empty;

		public int LeftInset
		{
			get
			{
				if (!TryGetLift(out Rectangle orb, out _))
				{
					if (!IsBarOnly)
					{
						return 0;
					}
					Rectangle bar = QuickAccessBounds(Caption, Commands().Count);
					return bar.IsEmpty ? 0 : bar.Right + S(8);
				}
				Rectangle quick = QuickAccessBounds(Caption, Commands().Count);
				int right = Math.Max(orb.Right, quick.IsEmpty ? 0 : quick.Right);
				return right + S(8);
			}
		}

		public void Paint(Graphics graphics, Rectangle caption, bool active)
		{
			if (!TryGetLift(out Rectangle orb, out _))
			{
				if (IsBarOnly)
				{
					List<ButtonCommand> items = Commands();
					PaintFlatQuickAccess(graphics, QuickAccessBounds(caption, items.Count), items);
				}
				return;
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			List<ButtonCommand> commands = Commands();
			Rectangle quick = QuickAccessBounds(caption, commands.Count);
			if (!quick.IsEmpty)
			{
				PaintQuickAccess(graphics, quick, commands);
			}
			BackstageMenuTabRenderer renderer = _ribbon.GetBackstageTabRenderer();
			bool open = (renderer != null && renderer.Selected) || _ribbon.IsBackstageOpen();
			bool hot = renderer != null && renderer.Hot && _ribbon.Enabled;
			BackstageMenuTabRenderer.PaintOrb(graphics, _ribbon, _ribbon._colorScheme.Theme, orb, open, hot);
			graphics.SmoothingMode = SmoothingMode.None;
		}

		private void PaintQuickAccess(Graphics graphics, Rectangle quick, List<ButtonCommand> commands)
		{
			ResolvedTheme theme = _ribbon._colorScheme.Theme;
			Color edge = Color.FromArgb(190, MxTheme.Darken(theme.Color(ThemeRole.Border), 0.2f));
			Color top = Color.FromArgb(210, 255, 255, 255);
			Color bottom = Color.FromArgb(90, MxTheme.Lighten(theme.Color(ThemeRole.Border), 0.3f));
			int radius = quick.Height / 2;
			using (GraphicsPath path = Capsule(quick, radius))
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(quick.X, quick.Y - 1, quick.Width, quick.Height + 2), top, bottom, LinearGradientMode.Vertical))
			{
				graphics.FillPath(brush, path);
				graphics.DrawPath(GdiCache.Pen(edge), path);
			}
			int icon = S(16);
			for (int index = 0; index < commands.Count; index++)
			{
				ButtonCommand button = commands[index];
				Rectangle slot = CellBounds(quick, index);
				bool enabled = button.IsEnabled;
				bool hot = enabled && ReferenceEquals(button, _hotCommand);
				bool pressed = enabled && ReferenceEquals(button, _pressedCommand);
				if (hot || pressed)
				{
					bool down = pressed && hot;
					// The golden glass of an Office 2007 button, darker while it is pressed.
					Color lit = down ? Color.FromArgb(255, 232, 166) : Color.FromArgb(255, 252, 235);
					Color deep = down ? Color.FromArgb(255, 171, 63) : Color.FromArgb(255, 222, 130);
					using (GraphicsPath face = RoundRect(new Rectangle(slot.X, slot.Y, slot.Width - 1, slot.Height - 1), Math.Max(2, S(3))))
					using (LinearGradientBrush glass = new LinearGradientBrush(new Rectangle(slot.X, slot.Y - 1, slot.Width, slot.Height + 2), lit, deep, LinearGradientMode.Vertical))
					{
						graphics.FillPath(glass, face);
						graphics.DrawPath(GdiCache.Pen(down ? Color.FromArgb(194, 138, 48) : Color.FromArgb(255, 189, 105)), face);
					}
				}
				Rectangle image = new Rectangle(slot.X + (slot.Width - icon) / 2, slot.Y + (slot.Height - icon) / 2, icon, icon);
				RibbonDrawingUtils.DrawImageFromSources(graphics, null, button.Image, button.ImageIndex, button.ImageKey, ((ICommandRenderHost)_ribbon).GetImageList(button), image, enabled, false, _ribbon._colorScheme);
			}
		}

		// The cells of the bar in a caption without the orb: the image of each command, a theme face while it is hot or pressed.
		private void PaintFlatQuickAccess(Graphics graphics, Rectangle quick, List<ButtonCommand> commands)
		{
			if (quick.IsEmpty)
			{
				return;
			}
			ResolvedTheme theme = _ribbon._colorScheme.Theme;
			SmoothingMode smoothing = graphics.SmoothingMode;
			int icon = S(16);
			for (int index = 0; index < commands.Count; index++)
			{
				ButtonCommand button = commands[index];
				Rectangle slot = CellBounds(quick, index);
				bool enabled = button.IsEnabled && _ribbon.Enabled;
				bool hot = enabled && ReferenceEquals(button, _hotCommand);
				bool pressed = enabled && ReferenceEquals(button, _pressedCommand);
				if (hot || pressed)
				{
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					theme.FillRounded(graphics, new Rectangle(slot.X, slot.Y, slot.Width - 1, slot.Height - 1), pressed && hot ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, Math.Min(theme.Shape.CornerRadius, S(3)));
				}
				graphics.SmoothingMode = SmoothingMode.None;
				Rectangle image = new Rectangle(slot.X + (slot.Width - icon) / 2, slot.Y + (slot.Height - icon) / 2, icon, icon);
				RibbonDrawingUtils.DrawImageFromSources(graphics, null, button.Image, button.ImageIndex, button.ImageKey, ((ICommandRenderHost)_ribbon).GetImageList(button), image, enabled, false, _ribbon._colorScheme);
			}
			graphics.SmoothingMode = smoothing;
		}

		private static GraphicsPath RoundRect(Rectangle r, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			int d = radius * 2;
			path.AddArc(r.X, r.Y, d, d, 180, 90);
			path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
			path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
			path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}

		private static GraphicsPath Capsule(Rectangle r, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			int d = Math.Max(2, radius * 2);
			path.AddArc(r.X, r.Y, d, d, 90, 180);
			path.AddArc(r.Right - d - 1, r.Y, d, d, 270, 180);
			path.CloseFigure();
			return path;
		}

		// The command whose cell holds the point, or null.
		private ButtonCommand CommandAt(Point point, List<ButtonCommand> commands)
		{
			Rectangle quick = QuickAccessBounds(Caption, commands.Count);
			if (quick.IsEmpty || !quick.Contains(point))
			{
				return null;
			}
			for (int index = 0; index < commands.Count; index++)
			{
				if (CellBounds(quick, index).Contains(point))
				{
					return commands[index];
				}
			}
			return null;
		}

		/// <summary>The cell of the command in window coordinates, or empty when the bar does not show it.</summary>
		internal Rectangle GetCommandBounds(ButtonCommand command)
		{
			List<ButtonCommand> commands = Commands();
			int index = commands.IndexOf(command);
			Rectangle quick = QuickAccessBounds(Caption, commands.Count);
			return index < 0 || quick.IsEmpty ? Rectangle.Empty : CellBounds(quick, index);
		}

		/// <summary>The lifted orb in window coordinates, or empty.</summary>
		internal Rectangle GetOrbBounds() => OrbBounds;

		private bool OnOrb(Point point)
		{
			Rectangle full = OrbBounds;
			if (full.IsEmpty)
			{
				return false;
			}
			if (Rectangle.Intersect(full, Caption).IsEmpty)
			{
				return false;
			}
			float cx = full.X + full.Width / 2f;
			float cy = full.Y + full.Height / 2f;
			float radius = full.Width / 2f;
			float dx = point.X - cx;
			float dy = point.Y - cy;
			return dx * dx + dy * dy <= radius * radius;
		}

		public bool HitTest(Point point)
		{
			return IsActive && (OnOrb(point) || CommandAt(point, Commands()) != null);
		}

		public void FrameChanged()
		{
			_ribbon.RelayoutForCaptionExtension();
		}

		public void MouseMove(Point point)
		{
			if (!IsActive)
			{
				return;
			}
			if (_pressedCommand != null && !PressStillValid())
			{
				CancelPress();
			}
			bool enabled = _ribbon.Enabled;
			ButtonCommand command = enabled ? CommandAt(point, Commands()) : null;
			bool changed = !ReferenceEquals(command, _hotCommand);
			_hotCommand = command;
			_ribbon.SetCaptionOrbHot(enabled && OnOrb(point));
			if (changed)
			{
				_frame.InvalidateCaption();
			}
		}

		public void MouseLeave()
		{
			bool changed = _hotCommand != null;
			_hotCommand = null;
			_ribbon.SetCaptionOrbHot(false);
			if (changed)
			{
				_frame.InvalidateCaption();
			}
		}

		public void MouseDown(Point point)
		{
			_pressedCommand = null;
			_pressedCommands = null;
			if (!IsActive || !_ribbon.Enabled)
			{
				return;
			}
			if (OnOrb(point))
			{
				_ribbon.ShowBackstageMenu();
				return;
			}
			List<ButtonCommand> commands = Commands();
			ButtonCommand command = CommandAt(point, commands);
			if (command != null && command.IsEnabled)
			{
				_pressedCommand = command;
				_pressedCommands = commands;
			}
			_frame.InvalidateCaption();
		}

		public void MouseUp(Point point)
		{
			ButtonCommand pressed = _pressedCommand;
			bool valid = pressed != null && PressStillValid();
			_pressedCommand = null;
			_pressedCommands = null;
			if (valid && IsActive && ReferenceEquals(CommandAt(point, Commands()), pressed))
			{
				pressed.InvokeOnClick();
			}
			_frame.InvalidateCaption();
		}

		// The press stays good while the ribbon is enabled and the bar shows the same commands, the pressed one enabled.
		private bool PressStillValid()
		{
			if (_pressedCommand == null || _pressedCommands == null || !_ribbon.Enabled || !_pressedCommand.Visible || !_pressedCommand.IsEnabled)
			{
				return false;
			}
			List<ButtonCommand> now = Commands();
			if (now.Count != _pressedCommands.Count)
			{
				return false;
			}
			for (int i = 0; i < now.Count; i++)
			{
				if (!ReferenceEquals(now[i], _pressedCommands[i]))
				{
					return false;
				}
			}
			return true;
		}

		public void GestureCanceled()
		{
			_pressedCommand = null;
			_pressedCommands = null;
			_frame.InvalidateCaption();
		}

		public void CaptionReleased()
		{
			_ribbon.OnCaptionExtensionReleased(this);
		}

		// Ends a press of this extension that the frame still holds.
		internal void CancelPress()
		{
			if (ReferenceEquals(_frame.CaptionExtension, this) && _frame.IsCaptionGestureActive)
			{
				_frame.CancelCaptionGesture();
			}
			else
			{
				GestureCanceled();
			}
		}

		internal void ClearState()
		{
			_pressedCommand = null;
			_pressedCommands = null;
			_hotCommand = null;
			OrbHot = false;
		}

		internal ButtonCommand PressedCommand => _pressedCommand;
	}
}
