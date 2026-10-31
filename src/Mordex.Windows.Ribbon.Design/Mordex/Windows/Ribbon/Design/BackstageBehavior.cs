using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Ribbon.Design
{
	internal class BackstageBehavior : Behavior
	{
		private MxRibbon _ribbon;

		private BehaviorService _behaviorService;

		internal BackstageBehavior(BackstageMenu bsMenu, MxRibbon owner, RibbonDesigner designer, BehaviorService behaviorService)
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			_ribbon = owner;
			this._behaviorService = behaviorService;
		}

		public override bool OnMouseDown(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			Point point = _behaviorService.AdornerWindowToScreen(mouseLoc);
			return DesignHelper.ProcessBackstageMouseDown(_ribbon, button, point.X, point.Y);
		}

		public override bool OnMouseUp(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			Point position = Cursor.Position;
			return DesignHelper.ProcessBackstageMouseUp(_ribbon, button, position.X, position.Y);
		}

		public override bool OnMouseMove(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			Point point = _behaviorService.AdornerWindowToScreen(mouseLoc);
			return DesignHelper.ProcessBackstageMouseMove(_ribbon, button, point.X, point.Y);
		}
	}
}
