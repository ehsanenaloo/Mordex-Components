using System.Drawing;
using Mordex.Windows.Common;
using Mordex.Windows.UI;

namespace Mordex.Windows.UI.Controls
{
	internal class WindowsAppearanceState
	{

		private const float HighlightBlend = 0.1f;

		private const float WindowBlend = 0.33f;

		private const float BrightColorThreshold = 0.85f;

		private const float BrightColorReduction = 0.45f;

		private const float HotBackBlend = 0.8f;

		private const float HotBackWindowBlend = 0.53f;

		private const float PressedBlend = 0.5f;

		public Color _backColor = Color.Empty;

		public Color _backGradientColor = Color.Empty;

		public Color _hotBackColor = Color.Empty;

		public Color _pressedBackColor = Color.Empty;

		public Color _selectedBackColor = Color.Empty;

		public Color _disabledForeColor = Color.Empty;

		public Color _disabledBackColor = Color.Empty;

		public Color _foreColor = Color.Empty;

		public void ApplyVisualStyle()
		{
			_backColor = Color.Empty;
			_backGradientColor = Color.Empty;
			_hotBackColor = Color.Empty;
			_pressedBackColor = Color.Empty;
			_selectedBackColor = Color.Empty;
			_disabledBackColor = Color.Empty;
			Color controlColor = RenderingHelper.GetSchemeColor(ColorName.Control);
			Color windowColor = RenderingHelper.GetSchemeColor(ColorName.Window);
			Color highlightColor = RenderingHelper.GetSchemeColor(ColorName.Highlight);
			_foreColor = RenderingHelper.GetSchemeColor(ColorName.ControlText);
			_disabledForeColor = RenderingHelper.GetSchemeColor(ColorName.GrayText);
			{
				Color tintedColor = RenderingHelper.BlendColors(controlColor, highlightColor, HighlightBlend);
				Color blendedColor = RenderingHelper.BlendColors(tintedColor, windowColor, WindowBlend);
				float brightness = (float)(blendedColor.R + blendedColor.G + blendedColor.B) / 3f / 255f;
				if (brightness >= BrightColorThreshold)
				{
					float reducedBrightness = brightness - BrightColorReduction;
					_hotBackColor = RenderingHelper.BlendColors(RenderingHelper.BlendColors(tintedColor, Color.Black, 1f - reducedBrightness), windowColor, WindowBlend);
				}
				else
				{
					_hotBackColor = blendedColor;
				}
				_selectedBackColor = RenderingHelper.BlendColors(RenderingHelper.BlendColors(controlColor, highlightColor, HotBackBlend), windowColor, HotBackWindowBlend);
				_pressedBackColor = RenderingHelper.BlendColors(highlightColor, windowColor, PressedBlend);
			}
		}
	}
}
