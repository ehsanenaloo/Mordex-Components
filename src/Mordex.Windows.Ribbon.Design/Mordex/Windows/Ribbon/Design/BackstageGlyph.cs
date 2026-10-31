using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Ribbon.Design
{
	internal class BackstageGlyph : Glyph
	{
		private GlyphOwnerWrapper _owner;

		private BehaviorService _behaviorService;

		private BackstageMenu _backstageMenu;

		private RibbonDesigner _ribbonDesigner;

		private Rectangle _bounds;

		private MxRibbon _ribbon;

		internal BackstageMenu BackstageMenu => _backstageMenu;

		public override Rectangle Bounds
		{
			get
			{
				Point point = _behaviorService.ControlToAdornerWindow(_owner.OwnerControl);
				return new Rectangle(point.X + _bounds.X, point.Y + _bounds.Y, _bounds.Width, _bounds.Height);
			}
		}

		internal BackstageGlyph(BehaviorService behaviorSvc, RibbonDesigner designer, MxRibbon owner, BackstageMenu bsMenu)
			: base(new BackstageBehavior(bsMenu, owner, designer, behaviorSvc))
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			_ribbon = owner;
			_behaviorService = behaviorSvc;
			_owner = new GlyphOwnerWrapper(owner);
			this._backstageMenu = bsMenu;
			_ribbonDesigner = designer;
			_bounds = _owner.GetBackstageBarBounds();
		}

		public override void Paint(PaintEventArgs pe)
		{
			if (_bounds.Width > 0 && _bounds.Height > 0)
			{
				pe.Graphics.FillRectangle(Brushes.Transparent, Bounds);
			}
		}

		public override Cursor GetHitTest(Point screenPt)
		{
			screenPt = _behaviorService.AdornerWindowToScreen(screenPt);
			if (DesignHelper.BackstageHitTest(_ribbon, screenPt) > 1 && _ribbonDesigner != null && !_ribbonDesigner.IsDragging)
			{
				return Cursors.Default;
			}
			return null;
		}
	}
}
