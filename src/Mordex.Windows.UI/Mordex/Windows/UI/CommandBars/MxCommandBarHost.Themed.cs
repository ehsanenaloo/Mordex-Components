using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBarHost
	{
		private ThemeHook _themeHook;

		// Called from paint, so the host listens to the default theme exactly while it paints with it.
		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				_themeHook = new ThemeHook(this);
			}
			_themeHook.Update(true);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}

		private void PaintThemedBackground(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool themeBack = false;
			Color color = FormatStyle.BackColor;
			if (color.IsEmpty)
			{
				color = CommandManager.CommandBarsFormatStyle.BackColor;
			}
			if (color.IsEmpty)
			{
				color = theme.Color(ThemeRole.ToolbarBack);
				themeBack = true;
			}
			if (themeBack)
			{
				theme.FillBand(graphics, base.ClientRectangle, ThemeRole.ToolbarBack, _isHorizontal ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal);
			}
			else
			{
				graphics.FillRectangle(GdiCache.Brush(color), base.ClientRectangle);
			}
			// A line under each row but the last one separates the rows of bars.
			for (int i = 0; i < _rows.Count - 1; i++)
			{
				Rectangle row = ((CommandBarHostLayoutState)_rows[i])._bounds;
				if (_isHorizontal)
				{
					graphics.DrawLine(theme.Pen(ThemeRole.Border), row.Left, row.Bottom - 1, row.Right, row.Bottom - 1);
				}
				else
				{
					graphics.DrawLine(theme.Pen(ThemeRole.Border), row.Right - 1, row.Top, row.Right - 1, row.Bottom);
				}
			}
		}
	}
}
