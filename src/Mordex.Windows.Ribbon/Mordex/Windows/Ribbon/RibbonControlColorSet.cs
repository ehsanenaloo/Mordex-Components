using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonControlColorSet
	{
		public Color _baseColor;

		public Color _trackColor;

		public Color _trackLightColor;

		public Color _trackBorderColor;

		public Color _trackDarkBorderColor;

		public Color _arrowColor;

		public Color _thumbBorderColor;

		public Color _thumbGradientTopColor;

		public Color _thumbGradientBottomColor;

		public Color _thumbHighlightColor;

		public Color _arrowHotColor;

		public Color _thumbHotBorderColor;

		public Color _thumbHotGradientTopColor;

		public Color _thumbHotGradientBottomColor;

		public Color _thumbHotHighlightColor;

		public Color _gripDarkColor;

		public Color _gripLightColor;

		public RibbonControlColorSet()
		{
			LoadDefaultColors();
		}

		private void LoadDefaultColors()
		{
			_baseColor = Color.FromArgb(87, 104, 144);
			_trackColor = Color.FromArgb(227, 229, 231);
			_trackLightColor = Color.FromArgb(231, 239, 245);
			_trackBorderColor = Color.FromArgb(200, 212, 225);
			_trackDarkBorderColor = Color.FromArgb(182, 196, 216);
			_arrowColor = Color.FromArgb(60, 110, 176);
			_thumbBorderColor = Color.FromArgb(190, 208, 232);
			_thumbGradientTopColor = Color.FromArgb(202, 223, 250);
			_thumbGradientBottomColor = Color.FromArgb(170, 203, 246);
			_thumbHighlightColor = Color.FromArgb(233, 241, 253);
			_arrowHotColor = Color.FromArgb(23, 73, 138);
			_thumbHotBorderColor = Color.FromArgb(156, 187, 229);
			_thumbHotGradientTopColor = Color.FromArgb(164, 199, 246);
			_thumbHotGradientBottomColor = Color.FromArgb(110, 166, 240);
			_thumbHotHighlightColor = Color.FromArgb(202, 222, 249);
			_gripDarkColor = Color.FromArgb(66, 75, 99);
			_gripLightColor = Color.FromArgb(110, 126, 166);
		}
	}
}
