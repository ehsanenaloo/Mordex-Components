using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxGroupBox
	{
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);
			PaintThemedBackground(e.Graphics);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			PaintThemed(e.Graphics);
			base.OnPaint(e);
		}

		private TextLayoutRenderer CreateTextLayoutRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.SetWordWrap(false);
			textLayoutRenderer.SetRightToLeft(RightToLeft);
			textLayoutRenderer.SetStringTrimming(StringTrimming.Character);
			textLayoutRenderer.SetShowHotkeyPrefix(true);
			if (_textAlignment == TextAlignment.Center)
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			}
			else if (_textAlignment == TextAlignment.Far)
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
			}
			else
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			}
			textLayoutRenderer.SetLineAlignment(StringAlignment.Near);
			return textLayoutRenderer;
		}

		private FormatStyle CreateBaseFormatStyle()
		{
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.BackgroundImage = BackgroundImage;
			uiFormatStyle.BackColor = GetEffectiveBackColor();
			if (base.Enabled)
			{
				uiFormatStyle.ForeColor = GetEffectiveForeColor();
			}
			else
			{
				uiFormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.GrayText);
			}
			return uiFormatStyle;
		}
	}
}
