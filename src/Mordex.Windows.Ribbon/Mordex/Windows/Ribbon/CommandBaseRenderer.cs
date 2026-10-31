using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class CommandBaseRenderer : RibbonLayoutItem
	{
		public int _fullItemWidth;

		public bool _hasImageContent;

		public bool _hasTextContent;

		public int _layoutWidth;

		public bool _fadeFromHot;

		public bool _fadeFromPushed;

		public CommandBase _commandBaseRendererCommand;

		public int _pressAnimationAlpha;

		private const int PressAnimationStartAlpha = 200;

		private const int PressAnimationStepAlpha = 100;

		private const int PressAnimationEndAlpha = 55;

		private const int PressAnimationOpaqueAlpha = 255;

		private const int PressAnimationIntervalMilliseconds = 70;

		private Timer _commandBaseRendererTimer;

		internal int _rowInColumn;

		internal int _columnIndex;

		public int _ownerBottom;

		public CommandBaseRenderer _layoutItemOwner;

		private bool _active;

		protected MxRibbon _commandBaseRendererRibbon;

		internal ICommandRenderHost _commandBaseRendererOwner;

		public virtual CommandType CommandType => CommandType.SmallButton;

		public virtual bool HasChildren => false;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (_commandBaseRendererCommand != null)
				{
					return _commandBaseRendererCommand.Role;
				}
				return base.AccessibleRole;
			}
		}

		public override string AccessibleName
		{
			get
			{
				if (GetCommand() != null)
				{
					if (GetCommand().AccessibleName != null && GetCommand().AccessibleName.Length > 0)
					{
						return GetCommand().AccessibleName;
					}
					return GetCommand().Name;
				}
				return base.AccessibleName;
			}
		}

		public override string AccessibleDescription
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleDescription != null && GetCommand().AccessibleDescription.Length > 0)
				{
					return GetCommand().AccessibleDescription;
				}
				return base.AccessibleDescription;
			}
		}

		public override AccessibleStates AccessibleState
		{
			get
			{
				AccessibleStates accessibleStates = base.AccessibleState;
				if (HasChildren)
				{
					accessibleStates |= AccessibleStates.HasPopup;
				}
				return accessibleStates;
			}
		}

		public override AccessibleObject AccessibilityObject
		{
			get
			{
				if (_commandBaseRendererCommand != null)
				{
					return _commandBaseRendererCommand.AccessibilityObject;
				}
				return base.AccessibilityObject;
			}
		}

		public virtual bool Active
		{
			get
			{
				return _active;
			}
			set
			{
				if (_active != value)
				{
					_active = value;
					Invalidate();
				}
			}
		}

		public override bool Hot
		{
			get
			{
				return base.Hot;
			}
			set
			{
				if (Hot != value)
				{
					base.Hot = value;
					if (_isEnabled)
					{
						Invalidate();
					}
				}
			}
		}

		public override bool Pushed
		{
			get
			{
				return base.Pushed;
			}
			set
			{
				if (Pushed != value)
				{
					base.Pushed = value;
					Invalidate();
				}
			}
		}

		public override bool Selected
		{
			get
			{
				return base.Selected;
			}
			set
			{
				if (base.Selected != value)
				{
					base.Selected = value;
					Invalidate();
				}
			}
		}

		public int GetLayoutWidth()
		{
			return _layoutWidth;
		}

		public void SetLayoutWidth(int itemWidth)
		{
			_layoutWidth = itemWidth;
		}

		public CommandBaseRenderer(CommandBase command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
		{
			_commandBaseRendererCommand = command;
			_isEnabled = true;
			_isVisible = true;
			_layoutItemOwner = layoutItemOwner;
			_commandBaseRendererOwner = owner;
			if (_commandBaseRendererOwner != null)
			{
				_parentControl = owner.ParentControl;
			}
			if (_commandBaseRendererCommand != null)
			{
				_commandBaseRendererCommand._renderer = this;
			}
		}

		public CommandBaseRenderer(Control parentControl)
			: base(parentControl)
		{
		}

		public CommandBase GetCommand()
		{
			return _commandBaseRendererCommand;
		}

		public ICommandRenderHost GetRenderHost()
		{
			return _commandBaseRendererOwner;
		}

		public void SetRenderHost(ICommandRenderHost commandBaseRendererOwner)
		{
			_commandBaseRendererOwner = commandBaseRendererOwner;
		}

		public virtual void MoveY(int newY)
		{
			int deltaY = _bounds.Y - newY;
			_bounds.Y = newY;
			if (!_textBounds.IsEmpty)
			{
				_textBounds.Y -= deltaY;
			}
			if (!_imageBounds.IsEmpty)
			{
				_imageBounds.Y -= deltaY;
			}
		}

		public void MoveToX(int newX)
		{
			int deltaX = _bounds.X - newX;
			_bounds.X = newX;
			if (!_textBounds.IsEmpty)
			{
				_textBounds.X -= deltaX;
			}
			if (!_imageBounds.IsEmpty)
			{
				_imageBounds.X -= deltaX;
			}
		}

		public virtual void Invalidate()
		{
			if (_commandBaseRendererOwner != null && _commandBaseRendererOwner.ParentControl != null)
			{
				_commandBaseRendererOwner.ParentControl.Invalidate();
			}
		}

		public void ResetPressAnimation()
		{
			_pressAnimationAlpha = PressAnimationOpaqueAlpha;
			_fadeFromHot = false;
			_fadeFromPushed = false;
			DisposePressTimer();
		}

		public void StartPressAnimation()
		{
			if (_commandBaseRendererTimer == null)
			{
				_pressAnimationAlpha = PressAnimationStartAlpha;
				_commandBaseRendererTimer = new Timer();
				_commandBaseRendererTimer.Interval = PressAnimationIntervalMilliseconds;
				_commandBaseRendererTimer.Tick += OnPressAnimationTick;
				_commandBaseRendererTimer.Start();
			}
			_fadeFromHot = Hot;
			_fadeFromPushed = Pushed;
		}

		// Stops every running hover or press animation timer without releasing the renderer.
		public virtual void StopAnimation()
		{
			DisposePressTimer();
		}

		public virtual void Dispose()
		{
			StopAnimation();
			_commandBaseRendererOwner = null;
			_layoutItemOwner = null;
			_parentControl = null;
			if (_commandBaseRendererCommand != null)
			{
				_commandBaseRendererCommand._renderer = null;
			}
		}

		public void DisposePressTimer()
		{
			if (_commandBaseRendererTimer != null)
			{
				_commandBaseRendererTimer.Stop();
				_commandBaseRendererTimer.Tick -= OnPressAnimationTick;
				_commandBaseRendererTimer.Dispose();
				_commandBaseRendererTimer = null;
			}
		}

		private void OnPressAnimationTick(object sender, EventArgs eventArgs)
		{
			_pressAnimationAlpha -= PressAnimationStepAlpha;
			if (_pressAnimationAlpha < PressAnimationEndAlpha)
			{
				ResetPressAnimation();
			}
			Invalidate();
		}

		public virtual Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			return new Point(_bounds.Left + _bounds.Width / 2, _bounds.Top + _bounds.Height / 2);
		}

		public virtual void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
		}

		public virtual void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
		}

		public virtual void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
		}

		public virtual void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (GetCommand() is IMouseEventInvoker && GetCommand().Enabled)
			{
				((IMouseEventInvoker)GetCommand()).InvokeMouseDown(eventArgs);
			}
		}

		public virtual void OnMouseMove(MouseEventArgs eventArgs)
		{
			if (GetCommand() is IMouseEventInvoker)
			{
				((IMouseEventInvoker)GetCommand()).InvokeMouseMove(eventArgs);
			}
		}

		public virtual void OnMouseUp(MouseEventArgs eventArgs)
		{
			if (GetCommand() is IMouseEventInvoker && GetCommand().IsEnabled)
			{
				if (eventArgs.Button == MouseButtons.Left)
				{
					if (Pushed)
					{
						if (_commandBaseRendererOwner.Ribbon != null && _commandBaseRendererOwner.Ribbon.GetPopupMenu() != null)
						{
							_commandBaseRendererOwner.Ribbon.GetPopupMenu().TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
						}
						GetCommand().InvokeOnClick();
					}
				}
				else if (GetRenderHost() != null && GetRenderHost().MouseDownItem == this && eventArgs.Button == MouseButtons.Right)
				{
					GetRenderHost().ShowQuickCustomizeMenu(GetCommand());
				}
				((IMouseEventInvoker)GetCommand()).InvokeMouseUp(eventArgs);
			}
			else if (eventArgs.Button == MouseButtons.Right && GetRenderHost() != null && GetRenderHost().MouseDownItem == this && GetCommand() != null && GetCommand().CommandType != CommandType.Separator)
			{
				GetRenderHost().ShowQuickCustomizeMenu(GetCommand());
			}
		}

		internal virtual void DrawButtonBackground(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme, CommandSizeStyle sizeStyle, bool hot, bool selected, bool pushed)
		{
			if (pushed && hot)
			{
				RibbonChromePainter.DrawBackground(graphics, rect, colorScheme, sizeStyle != CommandSizeStyle.Large);
			}
			else if (selected)
			{
				if (hot)
				{
					RibbonChromePainter.DrawSelectedHotButtonBackground(graphics, rect, colorScheme, sizeStyle != CommandSizeStyle.Large);
				}
				else
				{
					RibbonChromePainter.DrawSelectedButtonBackground(graphics, rect, colorScheme, sizeStyle != CommandSizeStyle.Large);
				}
			}
			else if (hot)
			{
				RibbonChromePainter.DrawHotButtonBackground(graphics, rect, colorScheme, sizeStyle != CommandSizeStyle.Large);
			}
		}

		internal virtual void DrawButtonGroupBackground(Graphics graphics, RibbonColorScheme colorScheme, CommandSizeStyle sizeStyle, bool hot, bool selected, bool pushed)
		{
			if (pushed && hot)
			{
				Rectangle rectangle = Rectangle.Inflate(_bounds, 1, 0);
				rectangle.Height--;
				rectangle.Width--;
				RibbonChromePainter.DrawButtonGroupItemBackground(graphics, _bounds, colorScheme, false, true);
			}
			else if (selected)
			{
				Rectangle selectedBounds = Rectangle.Inflate(_bounds, 1, 0);
				selectedBounds.Height--;
				selectedBounds.Width--;
				if (hot)
				{
					RibbonChromePainter.DrawButtonGroupItemBackground(graphics, _bounds, colorScheme, true, false);
				}
				else
				{
					RibbonChromePainter.DrawButtonGroupItemBackground(graphics, _bounds, colorScheme, false, false);
				}
			}
			else if (hot)
			{
				Rectangle hotBounds = new Rectangle(_bounds.X - 1, _bounds.Y + 1, _bounds.Width + 1, _bounds.Height - 2);
				if (hotBounds.Right >= _layoutItemOwner._bounds.Right - 2)
				{
					hotBounds.Width++;
				}
				int topHeight = (int)((float)hotBounds.Height * 0.4f);
				Rectangle rect = new Rectangle(hotBounds.X, hotBounds.Y, hotBounds.Width, topHeight);
				Rectangle bottomHalf = new Rectangle(hotBounds.X, rect.Bottom, hotBounds.Width, hotBounds.Height - topHeight);
				GdiCache.FillGradient(graphics, rect, colorScheme._hotButtonTopStart, colorScheme._hotButtonTopEnd, LinearGradientMode.Vertical);
				bottomHalf.Y--;
				using (Brush bottomBrush = new LinearGradientBrush(bottomHalf, colorScheme._hotButtonBottomStart, colorScheme._hotButtonBottomEnd, LinearGradientMode.Vertical))
				{
					bottomHalf.Y++;
					graphics.FillRectangle(bottomBrush, bottomHalf);
				}
				Rectangle borderRect = new Rectangle(hotBounds.X, hotBounds.Y, hotBounds.Width, hotBounds.Height);
				Brush borderBrush = new LinearGradientBrush(borderRect, Color.FromArgb(80, Color.White), Color.FromArgb(125, Color.White), LinearGradientMode.Vertical);
				Pen pen = new Pen(borderBrush);
				RibbonDrawingUtils.DrawRectangleOutline(graphics, pen, borderRect);
				borderBrush.Dispose();
				pen.Dispose();
			}
			if (_bounds.Right < _layoutItemOwner._bounds.Right - 2)
			{
				Pen separatorPen = GdiCache.Pen(RibbonDrawingUtils.BlendColors(colorScheme._dividerLine, Color.White, 0.7f));
				graphics.DrawLine(separatorPen, _bounds.Right, _bounds.Y + 1, _bounds.Right, _bounds.Bottom - 2);
			}
		}
	}
}
