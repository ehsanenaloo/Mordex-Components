using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Ribbon.Design
{
	internal class RibbonTabGlyph : Glyph
	{
		private MxRibbon _ribbon;

		private BehaviorService _behaviorService;

		private RibbonTab _tab;

		private RibbonDesigner _designer;

		private Rectangle _bounds;

		private bool _handleMouseEvents;

		internal RibbonTab Tab => _tab;

		public override Rectangle Bounds
		{
			get
			{
				Point point = _behaviorService.ControlToAdornerWindow(_ribbon);
				return new Rectangle(point.X + _bounds.X, point.Y + _bounds.Y, _bounds.Width, _bounds.Height);
			}
		}

		public override Behavior Behavior => base.Behavior;

		internal RibbonTabGlyph(BehaviorService behaviorSvc, RibbonDesigner designer, MxRibbon ribbon, RibbonTab tab, bool handleMouseEvents)
			: base(new RibbonTabBehavior(tab, designer))
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			_behaviorService = behaviorSvc;
			this._ribbon = ribbon;
			_tab = tab;
			this._designer = designer;
			_bounds = DesignHelper.GetTabBounds(ribbon, tab);
			this._handleMouseEvents = handleMouseEvents;
		}

		internal void Invalidate()
		{
			_bounds = DesignHelper.GetTabBounds(_ribbon, _tab);
		}

		public override void Paint(PaintEventArgs pe)
		{
			using Pen pen = new Pen(Mordex.Windows.Common.MxThemeManager.DefaultTheme.GetColor(Mordex.Windows.Common.ThemeRole.Text));
			Rectangle rect = Bounds;
			rect.Inflate(-3, -2);
			rect.Width--;
			rect.Height++;
			pen.DashStyle = DashStyle.Dot;
			pe.Graphics.DrawRectangle(pen, rect);
		}

		public override Cursor GetHitTest(Point pt)
		{
			if (_handleMouseEvents && Bounds.Contains(pt))
			{
				return Cursors.Default;
			}
			return null;
		}
	}
}
