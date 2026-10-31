using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Ribbon.Design
{
	internal readonly struct ArrowPair
	{
		internal Rectangle First { get; }

		internal ArrowDirection FirstDirection { get; }

		internal Rectangle Second { get; }

		internal ArrowDirection SecondDirection { get; }

		internal ArrowPair(Rectangle first, ArrowDirection firstDirection, Rectangle second, ArrowDirection secondDirection)
		{
			First = first;
			FirstDirection = firstDirection;
			Second = second;
			SecondDirection = secondDirection;
		}
	}

	// Code shared by the ribbon and status bar designers.
	internal static class DesignerDragHelper
	{
		private const int ArrowSize = 10;

		private const int ArrowAfterX = 4;

		private const int ArrowBeforeX = 6;

		private const int ArrowAboveY = 5;

		private const int ArrowBelowY = 2;

		private const int ContainerArrowX = 4;

		private const int ContainerArrowBottomY = 7;

		private const int SideArrowX = 5;

		private const int ContainerSideArrowTopY = 4;

		private const int ContainerSideArrowRightX = 7;

		// True once the cursor has left the small rectangle around the point where the mouse went down.
		internal static bool DragDetect(Point startPoint)
		{
			Point position = Cursor.Position;
			Size dragSize = SystemInformation.DragSize;
			Rectangle dragArea = new Rectangle(startPoint.X - dragSize.Width, startPoint.Y - dragSize.Height, dragSize.Width * 2, dragSize.Height * 2);
			return !dragArea.Contains(position);
		}

		internal static DragDropEffects GetDragEffect()
		{
			return Control.ModifierKeys == Keys.Control ? DragDropEffects.All : DragDropEffects.Move;
		}

		internal static bool SelectOnly(ISelectionService selectionService, object component)
		{
			if (selectionService == null)
			{
				return false;
			}
			selectionService.SetSelectedComponents(new ArrayList { component });
			return selectionService.SelectionCount == 1;
		}

		// Gives a newly added, sited component its site name as its text, through the property so the change is recorded.
		internal static void NameFromSite(IComponent component, bool nameFromSite)
		{
			if (nameFromSite && component?.Site != null)
			{
				DesignerChange.SetProperty(component, "Text", component.Site.Name);
			}
		}

		internal static bool CanDropCommand(CommandBase dragCommand, CommandBase dropTarget)
		{
			if (dropTarget == null)
			{
				return false;
			}
			if (DesignHelper.GetCommandType(dragCommand) == CommandType.RibbonGroup)
			{
				return true;
			}
			object parent;
			switch (DesignHelper.GetCommandType(dropTarget))
			{
			case CommandType.Gallery:
				return false;
			case CommandType.ButtonGroup:
			case CommandType.Box:
			case CommandType.RibbonGroup:
				parent = dropTarget;
				break;
			default:
				parent = DesignHelper.GetOwner(dropTarget);
				break;
			}
			return parent != null && DesignHelper.OwnerCanParentCommandType(parent, DesignHelper.GetCommandType(dragCommand));
		}

		// Arrows above and below the gap that follows or precedes the bounds, for items laid out side by side.
		internal static ArrowPair HorizontalInsertArrows(Rectangle bounds, bool insertAfter)
		{
			int x = insertAfter ? bounds.Right - ArrowAfterX : bounds.X - ArrowBeforeX;
			Rectangle top = new Rectangle(x, bounds.Top - ArrowAboveY, ArrowSize, ArrowSize);
			Rectangle bottom = new Rectangle(x, bounds.Bottom - ArrowBelowY, ArrowSize, ArrowSize);
			return new ArrowPair(bottom, ArrowDirection.Up, top, ArrowDirection.Down);
		}

		// Arrows left and right of the gap that follows or precedes the bounds, for items stacked vertically.
		internal static ArrowPair VerticalInsertArrows(Rectangle bounds, bool insertAfter)
		{
			int y = insertAfter ? bounds.Bottom - ArrowSize : bounds.Top - ArrowAboveY;
			Rectangle left = new Rectangle(bounds.X - SideArrowX, y, ArrowSize, ArrowSize);
			Rectangle right = new Rectangle(bounds.Right - ArrowAfterX, y, ArrowSize, ArrowSize);
			return new ArrowPair(left, ArrowDirection.Right, right, ArrowDirection.Left);
		}

		// Arrows at the start of an empty or horizontal container.
		internal static ArrowPair ContainerStartArrows(Rectangle area)
		{
			Rectangle top = new Rectangle(area.X + ContainerArrowX, area.Top - ArrowAboveY, ArrowSize, ArrowSize);
			Rectangle bottom = new Rectangle(area.X + ContainerArrowX, area.Bottom - ContainerArrowBottomY, ArrowSize, ArrowSize);
			return new ArrowPair(bottom, ArrowDirection.Up, top, ArrowDirection.Down);
		}

		// Arrows at the top of a vertical container.
		internal static ArrowPair ContainerSideArrows(Rectangle area)
		{
			Rectangle left = new Rectangle(area.X - SideArrowX, area.Top + ContainerSideArrowTopY, ArrowSize, ArrowSize);
			Rectangle right = new Rectangle(area.Right - ContainerSideArrowRightX, area.Top + ContainerSideArrowTopY, ArrowSize, ArrowSize);
			return new ArrowPair(left, ArrowDirection.Right, right, ArrowDirection.Left);
		}
	}
}
