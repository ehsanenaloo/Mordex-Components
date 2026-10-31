using System;
using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal static partial class ThemedRibbon
	{
		/// <summary>Builds the color table of a themed ribbon from the roles of a theme. Gradients get equal ends, so they paint flat.</summary>
		internal static RibbonColorScheme CreateScheme(ResolvedTheme theme)
		{
			Color Role(ThemeRole role) => theme.Color(role);
			Color surface = Role(ThemeRole.Surface);
			Color panel = Role(ThemeRole.ToolbarBack);
			Color raised = Role(ThemeRole.SurfaceRaised);
			Color hot = Role(ThemeRole.SurfaceHot);
			Color pressed = Role(ThemeRole.SurfacePressed);
			// Office 365 Dark Gray puts the ribbon on a light card in a dark window: the card has its own text color.
			Color cardText = Role(ThemeRole.ToolbarText);
			bool lightCard = cardText.ToArgb() != Role(ThemeRole.Text).ToArgb();
			if (lightCard)
			{
				hot = MxTheme.Darken(panel, 0.12f);
				pressed = MxTheme.Darken(panel, 0.22f);
			}
			Color border = Role(ThemeRole.Border);
			Color borderHot = Role(ThemeRole.BorderHot);
			Color borderPressed = Role(ThemeRole.BorderPressed);
			Color accent = Role(ThemeRole.Accent);
			Color accentHot = Role(ThemeRole.AccentHot);
			Color accentSubtle = Role(ThemeRole.AccentSubtle);
			Color text = Role(ThemeRole.Text);
			Color muted = Role(ThemeRole.TextMuted);
			Color disabled = Role(ThemeRole.TextDisabled);
			Color cardDisabled = lightCard ? MxTheme.Blend(cardText, panel, 0.55f) : disabled;
			Color onAccent = Role(ThemeRole.TextOnAccent);
			Color statusBack = Role(ThemeRole.StatusBarBack);
			Color tabInactive = Role(ThemeRole.TabInactive);
			Color tabHot = Role(ThemeRole.TabHot);

			RibbonColorScheme s = new RibbonColorScheme { Theme = theme };

			// In Chrome and Firefox the tabs sit on the strip color and the selected tab opens into the lighter toolbar.
			bool browser = theme.Theme.Family == "Chrome" || theme.Theme.Family == "Firefox";
			s._headerBackColor = browser ? Role(ThemeRole.SurfaceAlt) : surface;
			s._headerBackgroundBottom = s._headerBackColor;
			s._tabContentBackColor = panel;
			s._contentGradientBottomStart = panel;
			s._tabContentBorderStart = border;
			s._dividerLine = border;
			s._innerBorderHighlight = panel;
			s._tabStripTopLine = border;
			s._captionBottomLine = border;
			s._darkLine = border;

			s._selectedTabFillEnd = ThemedRibbonPainter.SelectedTabFill(theme);

			s._groupBorderTop = border;
			s._groupOutlineStart = border;
			s._groupOutlineEnd = border;
			s._groupFillTopStart = panel;
			s._groupFillTopEnd = panel;
			s._groupFillBottomStart = panel;
			s._groupFillBottomEnd = panel;
			s._groupFillGlow = panel;

			SetButton(s, hot, borderHot, pressed, borderPressed, accentSubtle, accent, accentHot);

			s._menuBackground = lightCard ? panel : raised;
			s._menuSeparator = border;
			s._menuSectionHeaderBackground = surface;
			s._dialogBackground = lightCard ? panel : raised;
			s._dialogBorder = border;
			s._separatorGradientEnd = border;
			s._separatorOuterStart = border;
			s._separatorOuterEnd = border;
			s._controlBoxPanelBackground = surface;

			s._quickAccessBorder = border;
			s._quickAccessOutlineStart = border;
			s._quickAccessOutlineEnd = border;
			s._headerQuickAccessActiveFillStart = surface;
			s._headerQuickAccessActiveFillEnd = surface;
			s._headerQuickAccessInactiveFillStart = surface;
			s._headerQuickAccessInactiveFillEnd = surface;
			s._headerQuickAccessBorder = border;

			s._galleryBackground = lightCard ? panel : raised;
			s._galleryHotBackground = hot;
			s._scrollButtonOutlineStart = border;
			s._scrollButtonOutlineEnd = border;
			s._scrollButtonArrowStart = cardText;
			s._scrollButtonArrowEnd = cardText;
			s._scrollButtonTopStart = raised;
			s._scrollButtonTopEnd = raised;
			s._scrollButtonBottomStart = raised;
			s._scrollButtonBottomEnd = raised;

			s._statusBarTopLine = border;
			s._statusBarTopHighlight = statusBack;
			s._statusBarLeftTopStart = statusBack;
			s._statusBarLeftBottomEnd = statusBack;
			s._statusBarRightBottomEnd = statusBack;
			s._statusBarGripDot = Role(ThemeRole.StatusBarText);
			s._statusBarGripShadow = statusBack;

			s._activeCaptionGradientFirst = surface;
			s._activeCaptionGradientSecond = surface;
			s._inactiveCaptionGradientFirst = surface;
			s._inactiveCaptionGradientSecond = surface;
			s._activeWindowBorder = border;
			s._activeWindowInnerBorder = surface;
			s._activeWindowFrame = border;
			s._activeWindowInnerFrame = surface;
			s._inactiveWindowBorder = border;
			s._inactiveWindowInnerBorder = surface;
			s._inactiveWindowFrame = border;
			s._inactiveWindowInnerFrame = surface;
			s._activeCaptionDocumentText = muted;
			s._inactiveCaptionDocumentText = disabled;
			s._activeCaptionApplicationText = text;
			s._inactiveCaptionApplicationText = disabled;

			SetCaptionButtons(s, hot, borderHot, pressed, borderPressed);

			// The orb and the file tab use the accent.
			Color accentPressed = Role(ThemeRole.AccentPressed);

			s._superTipFooterLine = border;
			s._superTipBorder = border;

			s._glyphColor = cardText;
			s._glyphDisabledColor = cardDisabled;
			s._disabledTextColor = cardDisabled;
			s._foreColorDark = cardText;
			s._foreColorLight = text;
			s._popupShadowColor = Role(ThemeRole.Shadow);
			s.FileTabText = onAccent;
			ApplyOfficePalette(s, theme);
			return s;
		}

		private static void SetButton(RibbonColorScheme s, Color hot, Color borderHot, Color pressed, Color borderPressed, Color subtle, Color accent, Color accentHot)
		{
			s._hotButtonTopStart = hot;
			s._hotButtonTopEnd = hot;
			s._hotButtonBottomStart = hot;
			s._hotButtonBottomEnd = hot;
		}

		private static void SetCaptionButtons(RibbonColorScheme s, Color hot, Color borderHot, Color pressed, Color borderPressed)
		{
		}
	}

	/// <summary>Keeps a control or component on the theme manager's listener list only while it is themed.</summary>
	internal sealed class ThemeSubscription : IThemeListener
	{
		private readonly Action _changed;

		private bool _active;

		internal ThemeSubscription(Action changed)
		{
			_changed = changed;
		}

		internal void Set(bool wanted)
		{
			if (wanted && !_active)
			{
				MxThemeManager.AddListener(this);
				_active = true;
			}
			else if (!wanted && _active)
			{
				MxThemeManager.RemoveListener(this);
				_active = false;
			}
		}

		void IThemeListener.OnThemeChanged()
		{
			_changed();
		}
	}
}
