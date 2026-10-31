using System;
using Mordex.Windows.Common;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mordex.Demo
{
	/// <summary>Switches the pages of the demo between the classic look and a theme. The work is done by <see cref="ControlThemer"/>.</summary>
	public static class ThemeApplier
	{
		private static readonly ConditionalWeakTable<Control, Font> FirstFonts = new ConditionalWeakTable<Control, Font>();

		public static void Apply(Control root, MxTheme theme, bool themed)
		{
			ControlThemer.Apply(root, theme, themed);
			FollowFont(root, theme, themed);
		}

		// The demo gives some labels and editors a font of their own in the family of the system font (page headings). They
		// take the font family of the theme and size and style stay as they were; the first font comes back with the classic look.
		private static void FollowFont(Control control, MxTheme theme, bool themed)
		{
			ThemeFont? wanted = themed && theme != null ? theme.Style.Detail.Font : null;
			string system = SystemFonts.MessageBoxFont.FontFamily.Name;
			if (FirstFonts.TryGetValue(control, out Font first))
			{
				if (wanted == null)
				{
					// A page can dispose a font it made itself; the control then goes back to the font it inherits.
					try
					{
						control.Font = first;
					}
					catch (ArgumentException)
					{
						control.ResetFont();
					}
					FirstFonts.Remove(control);
				}
				else if (control.Font.Name != wanted.Value.Family)
				{
					control.Font = new Font(wanted.Value.Family, first.SizeInPoints, first.Style, GraphicsUnit.Point);
				}
			}
			else if (wanted != null && (control is Label || control is RichTextBox) && control.Font.Name == system && (control.Font.SizeInPoints != SystemFonts.MessageBoxFont.SizeInPoints || control.Font.Style != FontStyle.Regular))
			{
				FirstFonts.Add(control, control.Font);
				control.Font = new Font(wanted.Value.Family, control.Font.SizeInPoints, control.Font.Style, GraphicsUnit.Point);
			}
			foreach (Control child in control.Controls)
			{
				FollowFont(child, theme, themed);
			}
		}
	}
}
