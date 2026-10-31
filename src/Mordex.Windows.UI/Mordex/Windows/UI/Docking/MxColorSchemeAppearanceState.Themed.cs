using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Docking
{
	internal partial class MxColorSchemeAppearanceState
	{
		// The color table of the themed look: every slot comes from a role of the default theme. Gradients stay empty.
		private void ApplyThemedColors()
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			Color Role(ThemeRole role) => theme.GetColor(role);
			ColorRef(PanelColors.Border) = Role(ThemeRole.Border);
			ColorRef(PanelColors.FloatingWindowBorder) = Role(ThemeRole.BorderStrong);
			ColorRef(PanelColors.InnerWorkspace) = Role(ThemeRole.SurfaceAlt);
			ColorRef(PanelColors.InnerContainer) = Role(ThemeRole.Surface);
			ColorRef(PanelColors.InnerWindow) = Role(ThemeRole.InputBack);
			ColorRef(PanelColors.Caption) = Role(ThemeRole.CaptionInactive);
			ColorRef(PanelColors.CaptionText) = theme.Style.FaceGradient != ThemeGradient.None && Role(ThemeRole.CaptionInactive).GetBrightness() < 0.4f ? Role(ThemeRole.TextOnAccent) : theme.Style.Detail.ChoiceLook != ThemeChoiceLook.Default ? Role(ThemeRole.Text) : Role(ThemeRole.TextMuted);
			ColorRef(PanelColors.ActiveCaption) = Role(ThemeRole.Caption);
			ColorRef(PanelColors.ActiveCaptionText) = Role(ThemeRole.Text);
			ColorRef(PanelColors.DarkCaption) = Role(ThemeRole.Header);
			ColorRef(PanelColors.DarkCaptionText) = Role(ThemeRole.HeaderText);
			ColorRef(PanelColors.LightCaption) = Role(ThemeRole.SurfaceAlt);
			ColorRef(PanelColors.LightCaptionText) = Role(ThemeRole.Text);
			ColorRef(PanelColors.HotElement) = Role(ThemeRole.SurfaceHot);
			ColorRef(PanelColors.HotElementText) = Role(ThemeRole.Text);
			ColorRef(PanelColors.HotElementBorder) = Role(ThemeRole.BorderHot);
			ColorRef(PanelColors.PressedElement) = Role(ThemeRole.SurfacePressed);
			ColorRef(PanelColors.PressedElementText) = Role(ThemeRole.Text);
			ColorRef(PanelColors.PressedElementBorder) = Role(ThemeRole.BorderPressed);
			ColorRef(PanelColors.SelectedElement) = Role(ThemeRole.AccentSubtle);
			ColorRef(PanelColors.SelectedElementText) = Role(ThemeRole.Text);
			ColorRef(PanelColors.SelectedElementBorder) = Role(ThemeRole.Accent);
			ColorRef(PanelColors.AutoHideStrip) = Role(ThemeRole.ToolbarBack);
			ColorRef(PanelColors.AutoHideTab) = Role(ThemeRole.TabInactive);
			ColorRef(PanelColors.AutoHideTabText) = Role(ThemeRole.TextMuted);
			ColorRef(PanelColors.OutlookButton) = Role(ThemeRole.SurfaceAlt);
			ColorRef(PanelColors.OutlookButtonText) = Role(ThemeRole.Text);
			ColorRef(PanelColors.OutlookResizeBar) = Role(ThemeRole.SurfaceAlt);
			ColorRef(PanelColors.Splitter) = Role(ThemeRole.Surface);
			ColorRef(PanelColors.TabStrip) = Role(ThemeRole.TabInactive);
			ColorRef(PanelColors.TabText) = Role(ThemeRole.TextMuted);
			ColorRef(PanelColors.SelectedTab) = Role(ThemeRole.TabActive);
			ColorRef(PanelColors.SelectedTabText) = Role(ThemeRole.Text);
			ColorRef(PanelColors.TabSeparator) = Role(ThemeRole.Border);
			ColorRef(PanelColors.DisabledText) = Role(ThemeRole.TextDisabled);
			ColorRef(PanelColors.MdiTab) = Role(ThemeRole.TabInactive);
			ColorRef(PanelColors.MdiTabText) = Role(ThemeRole.TextMuted);
			ColorRef(PanelColors.MdiTabStrip) = Role(ThemeRole.TabInactive);
			foreach (PanelColors gradient in new[]
			{
				PanelColors.CaptionGradient, PanelColors.ActiveCaptionGradient, PanelColors.DarkCaptionGradient, PanelColors.LightCaptionGradient,
				PanelColors.HotElementGradient, PanelColors.PressedElementGradient, PanelColors.SelectedElementGradient,
				PanelColors.AutoHideStripGradient, PanelColors.AutoHideTabGradient, PanelColors.OutlookButtonGradient,
				PanelColors.OutlookButtonLightGradient, PanelColors.OutlookButtonDarkGradient, PanelColors.OutlookResizeBarGradient,
				PanelColors.SplitterGradient, PanelColors.TabStripGradient, PanelColors.SelectedTabGradient,
				PanelColors.MdiTabGradient, PanelColors.MdiTabStripGradient
			})
			{
				ColorRef(gradient) = Color.Empty;
			}
		}
	}
}
