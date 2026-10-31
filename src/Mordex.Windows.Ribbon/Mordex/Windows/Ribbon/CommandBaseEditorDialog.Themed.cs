using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog
	{
		private bool ThemeAllowsShadow => _popupColorScheme?.Theme == null || _popupColorScheme.Theme.Shape.HasShadow;

	}
}
