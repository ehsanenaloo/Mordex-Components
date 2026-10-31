using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonChromePainter
	{
		internal static void DrawTranslucentHotButtonBackgroundFlat(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Face(graphics, colorScheme.Theme, rect, ThemeRole.SurfaceHot, ThemeRole.BorderHot);
			return;
		}

		internal static void DrawTranslucentHotButtonBackground(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme, bool isSmall)
		{
			DrawTranslucentHotButtonBackgroundFlat(graphics, bounds, colorScheme);
			return;
		}

		internal static void DrawSelectedHotButtonBackground(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme, bool isSmall)
		{
			DrawSelectedHotButtonBackgroundFlat(graphics, bounds, colorScheme);
			return;
		}

		internal static void DrawSelectedHotButtonBackgroundFlat(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Face(graphics, colorScheme.Theme, rect, ThemeRole.AccentSubtle, ThemeRole.AccentHot);
			return;
		}

		internal static void DrawSelectedButtonBackgroundFlat(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Face(graphics, colorScheme.Theme, rect, ThemeRole.AccentSubtle, ThemeRole.Accent);
			return;
		}

		internal static void DrawSelectedButtonBackground(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme, bool isSmall)
		{
			DrawSelectedButtonBackgroundFlat(graphics, bounds, colorScheme);
			return;
		}

		internal static void DrawButtonGroupItemBackground(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme, bool isHot, bool isPressed)
		{
			ThemedRibbonPainter.Face(graphics, colorScheme.Theme, bounds, isPressed ? ThemeRole.SurfacePressed : isHot ? ThemeRole.SurfaceHot : ThemeRole.AccentSubtle, isPressed ? ThemeRole.BorderPressed : isHot ? ThemeRole.BorderHot : ThemeRole.Accent);
			return;
		}

		internal static void DrawPressedButtonBackgroundFlat(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Face(graphics, colorScheme.Theme, bounds, ThemeRole.SurfacePressed, ThemeRole.BorderPressed);
			return;
		}

		internal static void DrawSplitButtonActiveBackground(Graphics graphics, Rectangle bounds, Rectangle rectangle, Rectangle arrowBounds, RibbonColorScheme colorScheme, bool isArrowHighlighted, bool isLarge)
		{
			ThemedRibbonPainter.SplitFace(graphics, colorScheme.Theme, bounds, rectangle, isArrowHighlighted, isLarge, true);
			return;
		}

		internal static void DrawSplitButtonHotBackground(Graphics graphics, Rectangle rect, Rectangle buttonBounds, Rectangle arrowBounds, RibbonColorScheme colorScheme, bool isArrowHighlighted, bool isLarge)
		{
			ThemedRibbonPainter.SplitFace(graphics, colorScheme.Theme, rect, buttonBounds, isArrowHighlighted, isLarge, false);
			return;
		}

		internal static void DrawHotButtonBackgroundFlat(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Face(graphics, colorScheme.Theme, rect, ThemeRole.SurfaceHot, ThemeRole.BorderHot);
			return;
		}

		internal static void DrawHotButtonBackground(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme, bool isSmall)
		{
			DrawHotButtonBackgroundFlat(graphics, bounds, colorScheme);
			return;
		}

		internal static void DrawHotMenuItemBackground(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme, bool isSmall)
		{
			ThemedRibbonPainter.Row(graphics, colorScheme.Theme, bounds, ThemeRole.SurfaceHot);
			return;
		}
	}
}
