using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal partial class GridColorTable
	{
		// Fills every slot from the default theme. The slots are the ones the Office 2003 look reads, so a themed grid
		// takes the same flat paint path with the colors of the theme.
		private void LoadThemedColors()
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			bool flat = theme.Shape.IsFlat;
			ThemeStyle style = theme.Style;
			bool styled = style.FaceGradient != ThemeGradient.None;
			_themeName = "THEMED";
			Color header = theme.GetColor(ThemeRole.Header);
			Color headerHot = theme.GetColor(ThemeRole.HeaderHot);
			Color headerPressed = theme.GetColor(ThemeRole.HeaderPressed);
			Color border = theme.GetColor(ThemeRole.Border);
			Color alt = theme.GetColor(ThemeRole.SurfaceAlt);
			Color text = theme.GetColor(ThemeRole.Text);
			Color selection = theme.GetColor(ThemeRole.Selection);
			Color raised = theme.GetColor(ThemeRole.SurfaceRaised);
			_colors[(int)GridColorKey.GroupByBox] = alt;
			_colors[(int)GridColorKey.GroupByBoxGradient] = alt;
			_colors[(int)GridColorKey.GroupByBoxInfo] = alt;
			_colors[(int)GridColorKey.GroupByBoxInfoGradient] = alt;
			_colors[(int)GridColorKey.GroupByBoxInfoText] = theme.GetColor(ThemeRole.TextMuted);
			_colors[(int)GridColorKey.Header] = styled ? style.LightEnd(header) : header;
			_colors[(int)GridColorKey.HeaderGradient] = styled ? style.DarkEnd(header) : flat ? header : GridHelper.BlendColors(header, border, 0.25f);
			_colors[(int)GridColorKey.HeaderText] = theme.GetColor(ThemeRole.HeaderText);
			_colors[(int)GridColorKey.HeaderBorder] = border;
			_colors[(int)GridColorKey.Gridline] = theme.GetColor(ThemeRole.GridLine);
			_colors[(int)GridColorKey.GroupRow] = alt;
			_colors[(int)GridColorKey.GroupRowGradient] = alt;
			_colors[(int)GridColorKey.GroupRowText] = text;
			_colors[(int)GridColorKey.HotElement] = styled ? style.LightEnd(headerHot) : headerHot;
			_colors[(int)GridColorKey.HotElementGradient] = styled ? style.DarkEnd(headerHot) : flat ? headerHot : GridHelper.BlendColors(headerHot, border, 0.25f);
			_colors[(int)GridColorKey.HotElementBorder] = theme.GetColor(ThemeRole.BorderHot);
			_colors[(int)GridColorKey.PressedElement] = styled ? style.LightEnd(headerPressed) : headerPressed;
			_colors[(int)GridColorKey.PressedElementGradient] = styled ? style.DarkEnd(headerPressed) : flat ? headerPressed : GridHelper.BlendColors(headerPressed, border, 0.25f);
			_colors[(int)GridColorKey.RowBackColor] = theme.GetColor(ThemeRole.InputBack);
			_colors[(int)GridColorKey.RowForeColor] = text;
			_colors[(int)GridColorKey.AlternatingRow] = theme.GetColor(ThemeRole.RowAlt);
			_colors[(int)GridColorKey.AlternatingRowText] = text;
			_colors[(int)GridColorKey.CardCaption] = header;
			_colors[(int)GridColorKey.CardCaptionGradient] = header;
			_colors[(int)GridColorKey.CardCaptionText] = theme.GetColor(ThemeRole.HeaderText);
			_colors[(int)GridColorKey.SelectedCardCaption] = selection;
			_colors[(int)GridColorKey.SelectedCardCaptionGradient] = selection;
			_colors[(int)GridColorKey.SelectedCardCaptionText] = theme.GetColor(ThemeRole.SelectionText);
			_colors[(int)GridColorKey.Button] = raised;
			_colors[(int)GridColorKey.ButtonGradient] = raised;
			_colors[(int)GridColorKey.ButtonText] = text;
			_colors[(int)GridColorKey.ControlBorder] = border;
			_colors[(int)GridColorKey.Control] = theme.GetColor(ThemeRole.Surface);
			_colors[(int)GridColorKey.SelectedInactive] = theme.GetColor(ThemeRole.SelectionInactive);
			_colors[(int)GridColorKey.DisabledBorder] = theme.GetColor(ThemeRole.BorderDisabled);
			_colors[(int)GridColorKey.WindowColor] = theme.GetColor(ThemeRole.InputBack);
			_colors[(int)GridColorKey.WindowTextColor] = text;
			_colors[(int)GridColorKey.HighlightColor] = selection;
			_selectionBlend = GridSelectionGradient.For(theme);
			_colors[(int)GridColorKey.HighlightTextColor] = theme.GetColor(ThemeRole.SelectionText);
			_colors[(int)GridColorKey.ControlDark] = theme.GetColor(ThemeRole.BorderStrong);
			_colors[(int)GridColorKey.DisabledColor] = theme.GetColor(ThemeRole.TextDisabled);
			_colors[(int)GridColorKey.SortArrow] = theme.GetColor(ThemeRole.TextMuted);
			_colors[(int)GridColorKey.HeaderGlyph] = text;
			_colors[(int)GridColorKey.HotElementText] = theme.GetColor(ThemeRole.HeaderText);
			_colors[(int)GridColorKey.SelectedInactiveTextColor] = theme.GetColor(ThemeRole.SelectionInactiveText);
		}
	}
}
