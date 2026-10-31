using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxColorPicker
	{

		private TextLayoutRenderer CreateTextLayoutRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			textLayoutRenderer.SetWordWrap(false);
			return textLayoutRenderer;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			TextLayoutRenderer textLayoutRenderer = CreateTextLayoutRenderer();
			if (_isLayoutDirty)
			{
				LayoutContent(e.Graphics, textLayoutRenderer);
			}
			PaintThemed(e.Graphics, textLayoutRenderer);
			textLayoutRenderer.ResetCache();
		}
	}
}
