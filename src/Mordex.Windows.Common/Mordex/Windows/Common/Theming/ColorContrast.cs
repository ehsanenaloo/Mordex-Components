using System;
using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>Contrast of two colors as WCAG defines it: relative luminance in linear light and the (L1 + 0.05) / (L2 + 0.05) ratio.</summary>
	internal static class ColorContrast
	{
		private static readonly double[] LinearChannel = BuildLinearChannel();

		private static double[] BuildLinearChannel()
		{
			double[] table = new double[256];
			for (int i = 0; i < table.Length; i++)
			{
				double c = i / 255.0;
				table[i] = c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
			}
			return table;
		}

		/// <summary>The relative luminance of an opaque color, from 0 for black to 1 for white.</summary>
		public static double RelativeLuminance(Color color)
		{
			return 0.2126 * LinearChannel[color.R] + 0.7152 * LinearChannel[color.G] + 0.0722 * LinearChannel[color.B];
		}

		/// <summary>The contrast ratio of two colors, from 1 to 21. The order of the colors does not matter.</summary>
		public static double Ratio(Color a, Color b)
		{
			double la = RelativeLuminance(a);
			double lb = RelativeLuminance(b);
			return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
		}

		/// <summary>The lowest contrast ratio of a text color over any of the backgrounds.</summary>
		public static double WorstRatio(Color text, params Color[] backgrounds)
		{
			double worst = double.MaxValue;
			foreach (Color back in backgrounds)
			{
				worst = Math.Min(worst, Ratio(text, back));
			}
			return worst;
		}

		/// <summary>The text color of the two that reads better on all of the backgrounds. A tie keeps the first.</summary>
		public static Color BetterText(Color first, Color second, params Color[] backgrounds)
		{
			return WorstRatio(second, backgrounds) > WorstRatio(first, backgrounds) ? second : first;
		}
	}
}
