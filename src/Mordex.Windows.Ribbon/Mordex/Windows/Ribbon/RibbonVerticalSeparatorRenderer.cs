using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonVerticalSeparatorRenderer : CommandBaseRenderer
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

		public RibbonVerticalSeparatorRenderer(CommandBase command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_fullItemWidth = 9;
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			ThemedRibbonPainter.VerticalSeparator(graphics, colorScheme.Theme, _bounds);
			return;
		}
	}
}
