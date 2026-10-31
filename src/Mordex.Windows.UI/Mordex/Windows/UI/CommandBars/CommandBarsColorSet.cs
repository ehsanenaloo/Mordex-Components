using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.CommandBars
{
	internal class CommandBarsColorSet
	{
		public Color _commandBarBackColor = Color.Empty;

		public Color _commandBarGradientColor = Color.Empty;

		public Color _imageAreaBackColor = Color.Empty;

		public Color _imageAreaGradientColor = Color.Empty;

		public Color _infrequentAreaBackColor = Color.Empty;

		public Color _infrequentAreaGradientColor = Color.Empty;

		public Color _menuBackColor = Color.Empty;

		public Color _rebarBackColor = Color.Empty;

		public Color _rebarGradientColor = Color.Empty;

		public Color _hotBackColor = Color.Empty;

		public Color _pressedBackColor = Color.Empty;

		public Color _selectedBackColor = Color.Empty;

		public Color _disabledForeColor = Color.Empty;

		public Color _foreColor = Color.Empty;

		internal CommandBarsColorSet()
		{
		}

		// The themed look reads the default theme. Gradients stay empty, so every area is a flat fill.
		private void ApplyThemedColors()
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			Color bar = theme.GetColor(ThemeRole.ToolbarBack);
			Color barText = theme.GetColor(ThemeRole.ToolbarText);
			// A light toolbar card in a dark window (Office 365 Dark Gray) puts the menus on the card too, in its text color.
			bool lightCard = barText.ToArgb() != theme.GetColor(ThemeRole.Text).ToArgb();
			_commandBarBackColor = bar;
			_rebarBackColor = bar;
			_menuBackColor = lightCard ? bar : theme.GetColor(ThemeRole.SurfaceRaised);
			_imageAreaBackColor = lightCard ? bar : theme.GetColor(ThemeRole.SurfaceRaised);
			_infrequentAreaBackColor = theme.GetColor(ThemeRole.SurfaceAlt);
			_hotBackColor = theme.GetColor(ThemeRole.SurfaceHot);
			_pressedBackColor = theme.GetColor(ThemeRole.SurfacePressed);
			_selectedBackColor = theme.GetColor(ThemeRole.AccentSubtle);
			_disabledForeColor = lightCard ? MxTheme.Blend(barText, bar, 0.55f) : theme.GetColor(ThemeRole.TextDisabled);
			_foreColor = barText;
		}

		public void ApplyVisualStyle()
		{
			Color controlColor = RenderingHelper.GetSchemeColor(ColorName.Control);
			Color windowColor = RenderingHelper.GetSchemeColor(ColorName.Window);
			Color highlightColor = RenderingHelper.GetSchemeColor(ColorName.Highlight);
			Color controlDarkColor = RenderingHelper.GetSchemeColor(ColorName.ControlDark);
			Color darkestControlColor = ControlPaint.DarkDark(controlColor);
			_commandBarBackColor = Color.Empty;
			_commandBarGradientColor = Color.Empty;
			_imageAreaBackColor = Color.Empty;
			_imageAreaGradientColor = Color.Empty;
			_infrequentAreaBackColor = Color.Empty;
			_infrequentAreaGradientColor = Color.Empty;
			_menuBackColor = Color.Empty;
			_rebarBackColor = Color.Empty;
			_rebarGradientColor = Color.Empty;
			_hotBackColor = Color.Empty;
			_pressedBackColor = Color.Empty;
			_selectedBackColor = Color.Empty;
			_disabledForeColor = RenderingHelper.GetSchemeColor(ColorName.GrayText);
			_foreColor = Color.Empty;
			ApplyThemedColors();
		}
	}
}
