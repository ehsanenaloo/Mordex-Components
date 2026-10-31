using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{

		private TextLayoutRenderer CreateTextRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!ShouldUseCompatibleTextRendering());
			textLayoutRenderer.SetWordWrap(false);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			if (!_isHorizontal && GetEffectiveTextAlign() != TextAlign.Underneath)
			{
				textLayoutRenderer.SetVertical(true);
			}
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			if (CommandManager == null || CommandManager.UseMnemonic)
			{
				textLayoutRenderer.SetShowHotkeyPrefix(true);
			}
			return textLayoutRenderer;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_commandManager == null)
			{
				base.OnPaint(e);
				return;
			}
			_dropIndex = -1;
			_isDropAfterItem = false;
			TextLayoutRenderer textLayoutRenderer = CreateTextRenderer();
			StateStyles uiStateStyles = BuildCommandStateStyles();
			UpdateThemeListening();
			SetThemed(false);
			if (_isLayoutDirty)
			{
				LayoutBar(e.Graphics, textLayoutRenderer, uiStateStyles, true);
			}
			if (_isHostingPopupControl)
			{
				base.OnPaint(e);
				return;
			}
			PaintThemed(e, textLayoutRenderer, uiStateStyles);
			uiStateStyles.DisposeFormatStyleResources();
			textLayoutRenderer.ResetCache();
			base.OnPaint(e);
		}

		private void DrawCustomizeSelection(Graphics graphics)
		{
			if (GetCustomizeSelectedItem() != null)
			{
				Rectangle bounds = GetCustomizeSelectedItem()._bounds;
				Pen pen = GdiCache.Pen(Color.Black, 2f);
				RenderingHelper.DrawRectangleOutline(graphics, pen, bounds);
				pen = null;
			}
		}
	}
}
