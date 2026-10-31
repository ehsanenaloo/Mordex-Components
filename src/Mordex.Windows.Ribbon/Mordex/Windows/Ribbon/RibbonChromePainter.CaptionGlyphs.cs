using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonChromePainter
	{
		internal static void DrawCaptionButtonBackgroundFlat(Graphics graphics, Rectangle rectangle, RibbonColorScheme colorScheme, bool isHot, bool isPressed)
		{
			ThemedRibbonPainter.CaptionButton(graphics, colorScheme.Theme, Rectangle.Inflate(rectangle, -1, -1), isHot, isPressed, false);
			return;
		}

		internal static void DrawRestoreGlyphFlat(Graphics graphics, Rectangle rectangle, RibbonColorScheme colorScheme, bool isFormActive, bool isHot, bool isPressed)
		{
			ThemedRibbonPainter.CaptionGlyph(graphics, colorScheme.Theme, rectangle, ThemedCaptionGlyph.Restore, ThemeRole.Text);
			return;
		}

		internal static void DrawHelpGlyphFlat(Graphics graphics, Rectangle rectangle, RibbonColorScheme colorScheme, bool isFormActive, bool isHot, bool isPressed)
		{
			ThemedRibbonPainter.CaptionGlyph(graphics, colorScheme.Theme, rectangle, ThemedCaptionGlyph.Help, ThemeRole.Text);
			return;
		}

		internal static void DrawCloseButtonFlat(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme, bool isFormActive, bool isHot, bool isPressed)
		{
			ThemedRibbonPainter.CaptionButton(graphics, colorScheme.Theme, Rectangle.Inflate(bounds, -1, -1), isHot, isPressed, true);
			ThemedRibbonPainter.CaptionGlyph(graphics, colorScheme.Theme, bounds, ThemedCaptionGlyph.Close, isHot || isPressed ? ThemeRole.TextOnAccent : ThemeRole.Text);
			return;
		}

		internal static void DrawMinimizeGlyphFlat(Graphics graphics, Rectangle rectangle, RibbonColorScheme colorScheme, bool isFormActive, bool isHot, bool isPressed, bool isEnabled)
		{
			ThemedRibbonPainter.CaptionGlyph(graphics, colorScheme.Theme, rectangle, ThemedCaptionGlyph.Minimize, isEnabled ? ThemeRole.Text : ThemeRole.TextDisabled);
			return;
		}

		internal static void DrawMaximizeGlyphFlat(Graphics graphics, Rectangle rectangle, RibbonColorScheme colorScheme, bool isFormActive, bool isHot, bool isPressed, bool isEnabled)
		{
			ThemedRibbonPainter.CaptionGlyph(graphics, colorScheme.Theme, rectangle, ThemedCaptionGlyph.Maximize, isEnabled ? ThemeRole.Text : ThemeRole.TextDisabled);
			return;
		}

	}
}
