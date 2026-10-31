using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Ribbon.Design
{
	internal class CommandGlyph : Glyph
	{
		private GlyphOwnerWrapper _owner;

		private BehaviorService _behaviorService;

		private CommandBase _command;

		private RibbonDesigner _ribbonDesigner;

		private StatusBarDesigner _statusBarDesigner;

		private Rectangle _bounds;

		internal CommandBase Command => _command;

		public override Rectangle Bounds
		{
			get
			{
				Point point = _behaviorService.ControlToAdornerWindow(_owner.OwnerControl);
				return new Rectangle(point.X + _bounds.X, point.Y + _bounds.Y, _bounds.Width, _bounds.Height);
			}
		}

		internal CommandGlyph(BehaviorService behaviorSvc, RibbonDesigner designer, MxRibbon owner, CommandBase command)
			: base(new CommandBehavior(command, owner, designer, behaviorSvc))
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			_behaviorService = behaviorSvc;
			_owner = new GlyphOwnerWrapper(owner);
			this._command = command;
			_ribbonDesigner = designer;
			_bounds = _owner.GetCommandBounds(command);
		}

		internal CommandGlyph(BehaviorService behaviorSvc, StatusBarDesigner designer, MxRibbonStatusBar owner, CommandBase command)
			: base(new CommandBehavior(command, owner, designer, behaviorSvc))
		{
			_behaviorService = behaviorSvc;
			_owner = new GlyphOwnerWrapper(owner);
			this._command = command;
			_statusBarDesigner = designer;
			_bounds = _owner.GetCommandBounds(command);
		}

		internal void Invalidate()
		{
			_bounds = _owner.GetCommandBounds(_command);
		}

		public override void Paint(PaintEventArgs pe)
		{
			if (_bounds.Width > 0 && _bounds.Height > 0)
			{
				DesignHelper.DrawGlyphHot(_owner.OwnerControl, pe.Graphics, Bounds);
			}
		}

		public override Cursor GetHitTest(Point pt)
		{
			if (Bounds.Contains(pt))
			{
				if (_ribbonDesigner != null)
				{
					if (!_ribbonDesigner.IsDragging)
					{
						return Cursors.SizeAll;
					}
				}
				else if (_statusBarDesigner != null && !_statusBarDesigner.IsDragging)
				{
					return Cursors.SizeAll;
				}
			}
			return null;
		}
	}
}
