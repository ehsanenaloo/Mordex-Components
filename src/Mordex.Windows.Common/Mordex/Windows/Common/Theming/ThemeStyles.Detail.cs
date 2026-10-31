using System.Drawing;

namespace Mordex.Windows.Common
{
	// The parts of the older Windows looks that go beyond faces and edges: the font, the check boxes, the group box and
	// the drop-down buttons. Each look starts from the style in ThemeStyles.
	internal static partial class ThemeStyles
	{
		private static readonly ThemeFont Tahoma8 = new ThemeFont("Tahoma", 8f);

		private static readonly Color LunaGreen = Color.FromArgb(33, 161, 33);

		private static readonly Color LunaOrange = Color.FromArgb(248, 179, 48);

		private static readonly Color LunaGroupFrame = Color.FromArgb(208, 208, 191);

		internal static ThemeStyle WindowsClassicLook()
		{
			return WindowsClassic() with
			{
				Detail = new ThemeDetailStyle
				{
					Font = new ThemeFont("Microsoft Sans Serif", 8.25f),
					ChoiceLook = ThemeChoiceLook.Classic,
					ChoiceMark = Color.Black,
					MenuLook = ThemeMenuLook.Classic,
					MenuSelection = Color.FromArgb(10, 36, 106),
					MenuSelectionText = Color.White,
					DropButton = ThemeDropButtonLook.Classic
				}
			};
		}

		internal static ThemeStyle WindowsXpLook()
		{
			return WindowsXp() with { Detail = Luna(), AppointmentRadius = 3 };
		}

		internal static ThemeStyle VisualStudio2005Look()
		{
			return VisualStudio2005() with { Detail = Luna(), AppointmentRadius = 3 };
		}

		private static ThemeDetailStyle Luna()
		{
			return new ThemeDetailStyle
			{
				Font = Tahoma8,
				ChoiceLook = ThemeChoiceLook.Luna,
				ChoiceMark = LunaGreen,
				ChoiceHot = LunaOrange,
				DropButton = ThemeDropButtonLook.Luna,
				GroupTitleRole = ThemeRole.AccentPressed,
				GroupFrame = LunaGroupFrame,
				TabPageBack = Color.FromArgb(252, 252, 254),
				NavHeaderTint = true,
				AppointmentGradient = true,
				MenuLook = ThemeMenuLook.Xp,
				MenuSelection = Color.FromArgb(49, 106, 197),
				MenuSelectionText = Color.White
			};
		}

		internal static ThemeStyle OfficeXpLook()
		{
			return OfficeXp() with
			{
				Detail = new ThemeDetailStyle
				{
					Font = Tahoma8,
					ChoiceLook = ThemeChoiceLook.Flat,
					ChoiceMark = Color.FromArgb(0, 0, 128),
					DropButton = ThemeDropButtonLook.Flat,
					MenuLook = ThemeMenuLook.OfficeXp
				}
			};
		}

		internal static ThemeStyle Office2003Look()
		{
			return Office2003() with
			{
				AppointmentRadius = 3,
				Detail = new ThemeDetailStyle
				{
					Font = Tahoma8,
					ChoiceLook = ThemeChoiceLook.Flat,
					ChoiceMark = Color.FromArgb(0, 0, 128),
					DropButton = ThemeDropButtonLook.Gradient,
					GroupTitleRole = ThemeRole.BorderStrong,
					MenuLook = ThemeMenuLook.Office2003,
					AppointmentGradient = true,
					NavHeaderTint = true
				}
			};
		}
	}
}
