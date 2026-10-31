using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal static partial class ThemedRibbon
	{
		private const int SlotTabStrip = 12;
		private const int SlotRibbonBorder = 16;
		private const int SlotRibbonLight = 20;
		private const int SlotRibbonLightGradient = 21;
		private const int SlotRibbonDark = 22;
		private const int SlotRibbonDarkGradient = 23;
		private const int SlotGroupBorder = 32;
		private const int SlotGroupBorderGradient = 33;
		private const int SlotForeColorDark = 162;
		private const int SlotForeColorLight = 203;
		private const int SlotApplicationName = 246;

		// The Office 2007 and 2010 looks carry the slot palette of the old renderers: the tab strip is blue, the content area
		// and the groups have gradients and the group frames have their own colors.
		private static void ApplyOfficePalette(RibbonColorScheme s, ResolvedTheme theme)
		{
			if (!theme.Theme.HasOfficePalette)
			{
				return;
			}
			Color strip = theme.OfficeColorOr(SlotTabStrip, SlotRibbonDark);
			s._headerBackColor = theme.Theme.Family == "Office 2007" ? strip : Color.FromArgb(255, System.Math.Min(255, strip.R + (255 - strip.R) / 4), System.Math.Min(255, strip.G + (255 - strip.G) / 4), System.Math.Min(255, strip.B + (255 - strip.B) / 4));
			s._headerBackgroundBottom = strip;
			s._tabContentBorderStart = theme.OfficeColorOr(SlotRibbonBorder, 17);
			s._groupOutlineStart = theme.OfficeColorOr(SlotGroupBorder, 17);
			s._groupOutlineEnd = theme.OfficeColorOr(SlotGroupBorderGradient, 17);
			s._groupFillTopStart = theme.OfficeColor(SlotRibbonLight);
			s._groupFillTopEnd = theme.OfficeColorOr(SlotRibbonLightGradient, SlotRibbonLight);
			s._groupFillBottomStart = theme.OfficeColor(SlotRibbonDark);
			s._groupFillBottomEnd = theme.OfficeColorOr(SlotRibbonDarkGradient, SlotRibbonDark);
			if (theme.Theme.Family == "Office 2007")
			{
				// Office 2007 writes the ribbon in its own text colors: dark blue in Blue, slate in Silver. In Black the tabs
				// that sit on the dark strip are white. The group captions of Blue are a lighter blue.
				s._foreColorDark = theme.OfficeColorOr(SlotForeColorDark, 17);
				s._foreColorLight = theme.OfficeColorOr(SlotForeColorLight, SlotForeColorDark);
				s._groupCaptionText = theme.Theme.Name.EndsWith("Blue") ? theme.OfficeColor(SlotApplicationName) : s._foreColorDark;
			}
		}
	}
}
