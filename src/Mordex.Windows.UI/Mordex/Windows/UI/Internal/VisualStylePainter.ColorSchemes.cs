using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Internal
{
	internal partial class VisualStylePainter
	{
		private void ApplyVisualStyle()
		{
			_isStandardStyle = false;
			if (string.Compare(_className, "Rebar", ignoreCase: true) == 0)
			{
				string rebarThemeScheme;
				if ((rebarThemeScheme = VisualStyleInformation.ColorScheme) == null)
				{
					return;
				}
				if (!(rebarThemeScheme == ThemeNormalColor))
				{
					if (!(rebarThemeScheme == ThemeHomeStead))
					{
						if (rebarThemeScheme == ThemeMetallic)
						{
							if (_isStandardStyle)
							{
								_rebarColor = Color.FromArgb(244, 244, 238);
								_rebarGradientColor = Color.FromArgb(224, 225, 235);
							}
							else
							{
								_rebarColor = Color.FromArgb(215, 215, 229);
								_rebarGradientColor = Color.FromArgb(242, 242, 247);
							}
						}
					}
					else if (_isStandardStyle)
					{
						_rebarColor = Color.FromArgb(243, 244, 247);
						_rebarGradientColor = Color.FromArgb(233, 230, 207);
					}
					else {
						_rebarColor = Color.FromArgb(217, 217, 167);
						_rebarGradientColor = Color.FromArgb(241, 240, 227);
					}
				}
				else
				{
					_rebarColor = Color.FromArgb(243, 244, 247);
					_rebarGradientColor = Color.FromArgb(233, 230, 207);
				}
			}
			else if (!_isStandardStyle)
			{
				_checkedColor = Color.FromArgb(255, 213, 140);
				_checkedGradientColor = Color.FromArgb(255, 173, 86);
				_hotColor = Color.FromArgb(255, 244, 204);
				_hotGradientColor = Color.FromArgb(255, 208, 145);
				_pressedColor = Color.FromArgb(254, 138, 72);
				_pressedGradientColor = Color.FromArgb(255, 203, 135);
				_itemHotColor = Color.FromArgb(255, 238, 194);
				_itemCheckedColor = Color.FromArgb(255, 192, 111);
				_itemPressedColor = Color.FromArgb(254, 128, 62);
				string toolbarThemeScheme;
				if ((toolbarThemeScheme = VisualStyleInformation.ColorScheme) == null)
				{
					return;
				}
				if (!(toolbarThemeScheme == ThemeNormalColor))
				{
					if (!(toolbarThemeScheme == ThemeHomeStead))
					{
						if (toolbarThemeScheme == ThemeMetallic)
						{
							_borderColor = Color.FromArgb(124, 124, 148);
							_popupBorderColor = _borderColor;
							_hotBorderColor = _borderColor;
							_pressedBorderColor = _hotBorderColor;
							_checkedBorderColor = _hotBorderColor;
							_itemHotBorderColor = _hotBorderColor;
							_itemPressedBorderColor = _hotBorderColor;
							_itemCheckedBorderColor = _hotBorderColor;
							_commandBarColor = Color.FromArgb(245, 246, 252);
							_commandBarGradientColor = Color.FromArgb(147, 145, 176);
							_commandBarBorderColor = Color.FromArgb(124, 124, 148);
							_customizeButtonColor = Color.FromArgb(179, 178, 200);
							_customizeButtonGradientColor = Color.FromArgb(118, 116, 146);
							_openItemBackColor = Color.FromArgb(214, 211, 231);
							_menuColor = Color.FromArgb(253, 250, 255);
							_imageAreaColor = Color.FromArgb(249, 249, 255);
							_imageAreaGradientColor = Color.FromArgb(159, 157, 185);
							_infrequentImageAreaColor = Color.FromArgb(215, 215, 226);
							_infrequentImageAreaGradientColor = Color.FromArgb(128, 126, 158);
							_floatingBarColor = Color.FromArgb(192, 192, 211);
							_gripColor = Color.FromArgb(84, 84, 117);
							_gripHighlightColor = SystemColors.Window;
							_separatorColor = Color.FromArgb(106, 109, 143);
							_menuSeparatorColor = _separatorColor;
							_tabBorderColor = Color.FromArgb(165, 172, 178);
							_menuItemBorderColor = Color.FromArgb(75, 75, 111);
							_panelBackColor = Color.FromArgb(229, 229, 238);
							_tabPageBackColor = Color.FromArgb(238, 238, 238);
							_tabColor = Color.FromArgb(218, 218, 231);
						}
					}
					else {
						_borderColor = Color.FromArgb(138, 134, 122);
						_popupBorderColor = _borderColor;
						_hotBorderColor = Color.FromArgb(147, 160, 112);
						_pressedBorderColor = _hotBorderColor;
						_checkedBorderColor = _hotBorderColor;
						_itemHotBorderColor = _hotBorderColor;
						_itemPressedBorderColor = _hotBorderColor;
						_itemCheckedBorderColor = _hotBorderColor;
						_commandBarColor = Color.FromArgb(250, 249, 245);
						_commandBarGradientColor = Color.FromArgb(192, 192, 168);
						_commandBarBorderColor = Color.FromArgb(163, 163, 124);
						_customizeButtonColor = Color.FromArgb(239, 238, 235);
						_customizeButtonGradientColor = Color.FromArgb(192, 192, 168);
						_openItemBackColor = Color.FromArgb(239, 237, 222);
						_menuColor = Color.FromArgb(252, 252, 249);
						_imageAreaColor = Color.FromArgb(254, 254, 251);
						_imageAreaGradientColor = Color.FromArgb(196, 195, 172);
						_itemBackStartColor = Color.FromArgb(251, 251, 249);
						_itemBackEndColor = Color.FromArgb(247, 245, 239);
						_infrequentImageAreaColor = Color.FromArgb(242, 240, 228);
						_infrequentImageAreaGradientColor = Color.FromArgb(187, 184, 174);
						_floatingBarColor = Color.FromArgb(172, 168, 153);
						_gripColor = Color.FromArgb(193, 190, 179);
						_gripHighlightColor = SystemColors.Window;
						_separatorColor = Color.FromArgb(197, 194, 184);
						_menuSeparatorColor = _separatorColor;
						_tabColor = Color.FromArgb(254, 253, 253);
						_tabGradientColor = Color.FromArgb(241, 239, 226);
						_tabPageBackColor = Color.FromArgb(228, 226, 213);
						_tabBorderColor = Color.FromArgb(172, 168, 153);
						_menuItemBorderColor = _hotBorderColor;
						_panelBackColor = Color.FromArgb(236, 233, 216);
					}
				}
				else {
					_borderColor = Color.FromArgb(138, 134, 122);
					_popupBorderColor = _borderColor;
					_hotBorderColor = Color.FromArgb(49, 106, 197);
					_pressedBorderColor = _hotBorderColor;
					_checkedBorderColor = _hotBorderColor;
					_itemHotBorderColor = _hotBorderColor;
					_itemPressedBorderColor = _hotBorderColor;
					_itemCheckedBorderColor = _hotBorderColor;
					_commandBarColor = Color.FromArgb(250, 249, 245);
					_commandBarGradientColor = Color.FromArgb(192, 192, 168);
					_commandBarBorderColor = Color.FromArgb(163, 163, 124);
					_customizeButtonColor = Color.FromArgb(239, 238, 235);
					_customizeButtonGradientColor = Color.FromArgb(192, 192, 168);
					_openItemBackColor = Color.FromArgb(239, 237, 222);
					_menuColor = Color.FromArgb(252, 252, 249);
					_imageAreaColor = Color.FromArgb(254, 254, 251);
					_imageAreaGradientColor = Color.FromArgb(196, 195, 172);
					_itemBackStartColor = Color.FromArgb(251, 251, 249);
					_itemBackEndColor = Color.FromArgb(247, 245, 239);
					_infrequentImageAreaColor = Color.FromArgb(242, 240, 228);
					_infrequentImageAreaGradientColor = Color.FromArgb(187, 184, 174);
					_floatingBarColor = Color.FromArgb(172, 168, 153);
					_gripHighlightColor = SystemColors.Window;
					_gripColor = Color.FromArgb(193, 190, 179);
					_separatorColor = Color.FromArgb(197, 194, 184);
					_menuSeparatorColor = _separatorColor;
					_tabColor = Color.FromArgb(254, 253, 253);
					_tabGradientColor = Color.FromArgb(241, 239, 226);
					_tabPageBackColor = Color.FromArgb(228, 226, 213);
					_tabBorderColor = Color.FromArgb(172, 168, 153);
					_menuItemBorderColor = _hotBorderColor;
					_panelBackColor = Color.FromArgb(236, 233, 216);
				}
			}
			else
			{
				VisualStyleElement normal = VisualStyleElement.ComboBox.DropDownButton.Normal;
				if (TrySelectElement(normal))
				{
					_comboBorderColor = _visualStyleRenderer.GetColor(ColorProperty.BorderColor);
				}
			}
		}

		private static bool IsHomeSteadTheme()
		{
			return VisualStyleInformation.ColorScheme == ThemeHomeStead;
		}

		public static Color GetThemeBorderColor()
		{
			string colorScheme;
			if ((colorScheme = VisualStyleInformation.ColorScheme) != null)
			{
				if (colorScheme == ThemeNormalColor)
				{
					return Color.FromArgb(106, 140, 203);
				}
				if (colorScheme == ThemeHomeStead)
				{
					return Color.FromArgb(96, 128, 88);
				}
				if (colorScheme == ThemeMetallic)
				{
					return Color.FromArgb(106, 109, 143);
				}
			}
			return Color.Empty;
		}

		public static Color GetFloatingBarCaptionColor()
		{
			string colorScheme;
			if ((colorScheme = VisualStyleInformation.ColorScheme) != null)
			{
				if (colorScheme == ThemeNormalColor)
				{
					return Color.FromArgb(42, 102, 201);
				}
				if (colorScheme == ThemeHomeStead)
				{
					return Color.FromArgb(116, 134, 94);
				}
				if (colorScheme == ThemeMetallic)
				{
					return Color.FromArgb(122, 121, 153);
				}
			}
			return Color.Empty;
		}
	}
}
