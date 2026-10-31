using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	// Themed is the look of the controls on screen. The classic look (system colors) is only for printing and the standalone option button.
	internal class MxColorSchemeAppearanceState
	{
		private bool _useTextRenderer;

		private bool _isThemedLook;

		internal bool _isThemed;

		private GridColorTable _resolvedAppearance;

		private MxTheme _resolvedTheme;

		internal MxColorSchemeAppearanceState(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
		}

		internal bool IsThemedLook => _isThemedLook;

		internal void SetThemedLook(bool themedLook)
		{
			_isThemedLook = themedLook;
		}

		internal GridColorTable GetResolvedAppearance()
		{
			// The table is read from the default theme, so a theme change makes a new one.
			if (_resolvedAppearance == null || (_isThemedLook && !ReferenceEquals(_resolvedTheme, MxThemeManager.DefaultTheme)))
			{
				_resolvedTheme = MxThemeManager.DefaultTheme;
				_resolvedAppearance = new GridColorTable(_isThemedLook, _isThemed);
			}
			return _resolvedAppearance;
		}

		internal bool GetUseTextRenderer()
		{
			return _useTextRenderer;
		}
	}
}
