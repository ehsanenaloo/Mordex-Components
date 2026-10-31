using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Ribbon.Design
{
	internal class DropArrowGlyph : Glyph
	{
		private BehaviorService _behaviorService;

		private Rectangle _bounds;

		private GlyphOwnerWrapper _owner;

		private ArrowDirection _arrowDirection;

		public override Rectangle Bounds
		{
			get
			{
				Point point = _behaviorService.ControlToAdornerWindow(_owner.OwnerControl);
				return new Rectangle(point.X + _bounds.X, point.Y + _bounds.Y, _bounds.Width, _bounds.Height);
			}
		}

		internal DropArrowGlyph(BehaviorService behaviorSvc, MxRibbon owner, Rectangle bounds, ArrowDirection arrowDirection)
			: base(new DropArrowBehavior())
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			_behaviorService = behaviorSvc;
			this._bounds = bounds;
			this._owner = new GlyphOwnerWrapper(owner);
			this._arrowDirection = arrowDirection;
		}

		internal DropArrowGlyph(BehaviorService behaviorSvc, MxRibbonStatusBar owner, Rectangle bounds, ArrowDirection arrowDirection)
			: base(new DropArrowBehavior())
		{
			_behaviorService = behaviorSvc;
			this._bounds = bounds;
			this._owner = new GlyphOwnerWrapper(owner);
			this._arrowDirection = arrowDirection;
		}

		public override Cursor GetHitTest(Point point)
		{
			return null;
		}

		public override void Paint(PaintEventArgs pe)
		{
			if (_bounds.Width <= 0 || _bounds.Height <= 0)
			{
				return;
			}
			using Pen pen = new Pen(Mordex.Windows.Common.MxThemeManager.DefaultTheme.GetColor(Mordex.Windows.Common.ThemeRole.Error));
			Rectangle rectangle = Bounds;
			switch (_arrowDirection)
			{
			case ArrowDirection.Down:
			{
				for (int i = 0; i < 4; i++)
				{
					pe.Graphics.DrawLine(pen, rectangle.X + 3, rectangle.Top + i, rectangle.X + 6, rectangle.Top + i);
				}
				pe.Graphics.DrawLine(pen, rectangle.X, rectangle.Y + 4, rectangle.X + 9, rectangle.Top + 4);
				pe.Graphics.DrawLine(pen, rectangle.X + 1, rectangle.Y + 5, rectangle.X + 8, rectangle.Top + 5);
				pe.Graphics.DrawLine(pen, rectangle.X + 2, rectangle.Y + 6, rectangle.X + 7, rectangle.Top + 6);
				pe.Graphics.DrawLine(pen, rectangle.X + 3, rectangle.Y + 7, rectangle.X + 6, rectangle.Top + 7);
				pe.Graphics.DrawLine(pen, rectangle.X + 4, rectangle.Y + 8, rectangle.X + 5, rectangle.Top + 8);
				break;
			}
			case ArrowDirection.Up:
			{
				for (int i = 1; i < 5; i++)
				{
					pe.Graphics.DrawLine(pen, rectangle.X + 3, rectangle.Y + rectangle.Height - i, rectangle.X + 6, rectangle.Y + rectangle.Height - i);
				}
				pe.Graphics.DrawLine(pen, rectangle.X, rectangle.Y + rectangle.Height - 5, rectangle.X + 9, rectangle.Y + rectangle.Height - 5);
				pe.Graphics.DrawLine(pen, rectangle.X + 1, rectangle.Y + rectangle.Height - 6, rectangle.X + 8, rectangle.Y + rectangle.Height - 6);
				pe.Graphics.DrawLine(pen, rectangle.X + 2, rectangle.Y + rectangle.Height - 7, rectangle.X + 7, rectangle.Y + rectangle.Height - 7);
				pe.Graphics.DrawLine(pen, rectangle.X + 3, rectangle.Y + rectangle.Height - 8, rectangle.X + 6, rectangle.Y + rectangle.Height - 8);
				pe.Graphics.DrawLine(pen, rectangle.X + 4, rectangle.Y + rectangle.Height - 9, rectangle.X + 5, rectangle.Y + rectangle.Height - 9);
				break;
			}
			case ArrowDirection.Right:
			{
				for (int i = 1; i < 5; i++)
				{
					pe.Graphics.DrawLine(pen, rectangle.X + i, rectangle.Y + 3, rectangle.X + i, rectangle.Y + 6);
				}
				pe.Graphics.DrawLine(pen, rectangle.X + 4, rectangle.Y, rectangle.X + 4, rectangle.Y + 9);
				pe.Graphics.DrawLine(pen, rectangle.X + 5, rectangle.Y + 1, rectangle.X + 5, rectangle.Y + 8);
				pe.Graphics.DrawLine(pen, rectangle.X + 6, rectangle.Y + 2, rectangle.X + 6, rectangle.Y + 7);
				pe.Graphics.DrawLine(pen, rectangle.X + 7, rectangle.Y + 3, rectangle.X + 7, rectangle.Y + 6);
				pe.Graphics.DrawLine(pen, rectangle.X + 8, rectangle.Y + 4, rectangle.X + 8, rectangle.Y + 5);
				break;
			}
			case ArrowDirection.Left:
			{
				for (int i = 1; i < 5; i++)
				{
					pe.Graphics.DrawLine(pen, rectangle.X + rectangle.Width - i, rectangle.Y + 3, rectangle.X + rectangle.Width - i, rectangle.Y + 6);
				}
				pe.Graphics.DrawLine(pen, rectangle.X + rectangle.Width - 5, rectangle.Y, rectangle.X + rectangle.Width - 5, rectangle.Y + 9);
				pe.Graphics.DrawLine(pen, rectangle.X + rectangle.Width - 6, rectangle.Y + 1, rectangle.X + rectangle.Width - 6, rectangle.Y + 8);
				pe.Graphics.DrawLine(pen, rectangle.X + rectangle.Width - 7, rectangle.Y + 2, rectangle.X + rectangle.Width - 7, rectangle.Y + 7);
				pe.Graphics.DrawLine(pen, rectangle.X + rectangle.Width - 8, rectangle.Y + 3, rectangle.X + rectangle.Width - 8, rectangle.Y + 6);
				pe.Graphics.DrawLine(pen, rectangle.X + rectangle.Width - 9, rectangle.Y + 4, rectangle.X + rectangle.Width - 9, rectangle.Y + 5);
				break;
			}
			}
		}
	}
}
