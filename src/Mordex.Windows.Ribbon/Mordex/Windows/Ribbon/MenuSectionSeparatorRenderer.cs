using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class MenuSectionSeparatorRenderer : CommandBaseRenderer
	{
		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				return AccessibleRole.Separator;
			}
		}

		public override CommandType CommandType => CommandType.Separator;

		public MenuSectionSeparatorRenderer(Control parentControl)
			: base(parentControl)
		{
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			Pen pen = GdiCache.Pen(colorScheme._menuSeparator);
			graphics.DrawLine(pen, _bounds.X, _bounds.Top, _bounds.Right - 1, _bounds.Top);
		}
	}
}
