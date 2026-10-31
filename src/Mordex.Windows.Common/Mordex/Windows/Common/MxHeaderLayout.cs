using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	// Keeps a caption and a combo box inside a header bar at any DPI and font size.
	public static class MxHeaderLayout
	{
		private const int Margin = 4;

		private const int Gap = 6;

		public static void Attach(Control header, Control caption, Control combo)
		{
			header.Controls.Add(caption);
			header.Controls.Add(combo);
			caption.BackColor = Color.Transparent;
			caption.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			combo.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			void Arrange(object sender, EventArgs e)
			{
				int needed = combo.Height + 2 * Margin;
				if (header.Height < needed)
				{
					header.Height = needed;
					return;
				}
				combo.Left = Math.Max(0, header.ClientSize.Width - combo.Width - Margin);
				combo.Top = (header.ClientSize.Height - combo.Height) / 2;
				caption.Left = Math.Max(0, combo.Left - caption.Width - Gap);
				caption.Top = (header.ClientSize.Height - caption.Height) / 2;
			}

			void Detach(object sender, EventArgs e)
			{
				header.Resize -= Arrange;
				header.FontChanged -= Arrange;
				combo.SizeChanged -= Arrange;
				caption.SizeChanged -= Arrange;
				header.HandleCreated -= Arrange;
				header.Disposed -= Detach;
			}

			header.Resize += Arrange;
			header.FontChanged += Arrange;
			combo.SizeChanged += Arrange;
			caption.SizeChanged += Arrange;
			header.HandleCreated += Arrange;
			header.Disposed += Detach;
			Arrange(header, EventArgs.Empty);
		}
	}
}
