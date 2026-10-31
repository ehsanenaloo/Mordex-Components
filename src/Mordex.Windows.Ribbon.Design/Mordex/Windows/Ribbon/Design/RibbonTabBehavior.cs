using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Ribbon.Design
{
	internal class RibbonTabBehavior : Behavior
	{
		private RibbonTab _tab;

		private RibbonDesigner _designer;

		private Point _dragStartPoint;

		internal RibbonTabBehavior(RibbonTab tab, RibbonDesigner designer)
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			_tab = tab;
			this._designer = designer;
		}

		public override bool OnMouseDown(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			_dragStartPoint = button == MouseButtons.Left ? Cursor.Position : Point.Empty;
			if (button == MouseButtons.Left && _tab is BackstageMenu)
			{
				_designer.ShowBackstageMenu();
			}
			return base.OnMouseDown(glyph, button, mouseLoc);
		}

		public override bool OnMouseUp(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			_dragStartPoint = Point.Empty;
			return base.OnMouseUp(glyph, button, mouseLoc);
		}

		public override bool OnMouseMove(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			if (button == MouseButtons.Left && !_dragStartPoint.IsEmpty && DesignerDragHelper.DragDetect(_dragStartPoint))
			{
				_dragStartPoint = Point.Empty;
				_designer.StartDrag(_tab);
			}
			return base.OnMouseMove(glyph, button, mouseLoc);
		}
	}
}
