using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandManager
	{
		internal PopupSettings CreatePopupSettings()
		{
			PopupSettings uiPopupSettings = new PopupSettings();
			bool isStyleSwapped = false;
			_commandBarsAppearanceState.ApplyVisualStyle();
			isStyleSwapped = true;
			uiPopupSettings.StateStyles.FormatStyle.BackColor = _commandBarsAppearanceState._menuBackColor;
			if (!_commandBarsAppearanceState._foreColor.IsEmpty)
			{
				uiPopupSettings.StateStyles.FormatStyle.ForeColor = _commandBarsAppearanceState._foreColor;
			}
			uiPopupSettings.ImageAreaFormatStyle.BackColor = _commandBarsAppearanceState._imageAreaBackColor;
			uiPopupSettings.ImageAreaFormatStyle.BackColorGradient = _commandBarsAppearanceState._imageAreaGradientColor;
			uiPopupSettings.InfrequentItemsFormatStyle.BackColor = _commandBarsAppearanceState._infrequentAreaBackColor;
			uiPopupSettings.InfrequentItemsFormatStyle.BackColorGradient = _commandBarsAppearanceState._infrequentAreaGradientColor;
			uiPopupSettings = uiPopupSettings.MergeWith(PopupSettings, CreateStateStyles());
			if (isStyleSwapped)
			{
				_commandBarsAppearanceState.ApplyVisualStyle();
			}
			return uiPopupSettings;
		}

		internal StateStyles CreateStateStyles()
		{
			StateStyles uiStateStyles = new StateStyles();
			bool isStyleSwapped = false;
			_commandBarsAppearanceState.ApplyVisualStyle();
			isStyleSwapped = true;
			if (!_commandBarsAppearanceState._foreColor.IsEmpty)
			{
				uiStateStyles.FormatStyle.ForeColor = _commandBarsAppearanceState._foreColor;
			}
			uiStateStyles.DisabledFormatStyle.ForeColor = _commandBarsAppearanceState._disabledForeColor;
			uiStateStyles.HotFormatStyle.BackColor = _commandBarsAppearanceState._hotBackColor;
			uiStateStyles.PressedFormatStyle.BackColor = _commandBarsAppearanceState._pressedBackColor;
			uiStateStyles.SelectedFormatStyle.BackColor = _commandBarsAppearanceState._selectedBackColor;
			uiStateStyles = uiStateStyles.CreateMergedStateStyles(CommandsStateStyles);
			if (isStyleSwapped)
			{
				_commandBarsAppearanceState.ApplyVisualStyle();
			}
			return uiStateStyles;
		}

		public Color GetDefaultColor(CommandColors commandColor)
		{
			return GetDefaultColor(commandColor, true);
		}

		public static Color GetColor(CommandColors commandColor, bool useThemes)
		{
			CommandBarsColorSet commandBarsAppearanceState = null;
			VisualStylePainter renderer = null;
			if ((useThemes && VisualStyleSupport.IsThemingUsable(false)))
			{
				renderer = ((commandColor != CommandColors.Rebar && commandColor != CommandColors.RebarGradient) ? new VisualStylePainter("Toolbar") : new VisualStylePainter("Rebar"));
			}
			else
			{
				useThemes = false;
				commandBarsAppearanceState = new CommandBarsColorSet();
				commandBarsAppearanceState.ApplyVisualStyle();
			}
			switch (commandColor)
			{
			case CommandColors.Border:
				if (useThemes)
				{
					return renderer._borderColor;
				}
				return SystemColors.ControlDark;
			case CommandColors.CustomizeButton:
				break;
			case CommandColors.CustomizeButtonGradient:
				break;
			case CommandColors.FloatingBar:
				break;
			case CommandColors.FloatingBarCaption:
				if (useThemes)
				{
					return SystemColors.ActiveCaption;
				}
				return SystemColors.Highlight;
			case CommandColors.HotElement:
				if (useThemes)
				{
					return SystemColors.Highlight;
				}
				return commandBarsAppearanceState._hotBackColor;
			case CommandColors.HotElementGradient:
				break;
			case CommandColors.ImageArea:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._imageAreaBackColor;
			case CommandColors.ImageAreaGradient:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._imageAreaGradientColor;
			case CommandColors.InfrequentImageArea:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._infrequentAreaBackColor;
			case CommandColors.InfrequentImageAreaGradient:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._infrequentAreaGradientColor;
			case CommandColors.Menu:
				if (useThemes)
				{
					return SystemColors.Menu;
				}
				return commandBarsAppearanceState._menuBackColor;
			case CommandColors.PressedElement:
				if (useThemes)
				{
					return SystemColors.Highlight;
				}
				return commandBarsAppearanceState._pressedBackColor;
			case CommandColors.PressedElementGradient:
				break;
			case CommandColors.Rebar:
				if (useThemes)
				{
					return renderer._rebarColor;
				}
				return commandBarsAppearanceState._rebarBackColor;
			case CommandColors.RebarGradient:
				if (useThemes)
				{
					return renderer._rebarGradientColor;
				}
				return commandBarsAppearanceState._rebarGradientColor;
			case CommandColors.SelectedElement:
				if (useThemes)
				{
					return SystemColors.Highlight;
				}
				return commandBarsAppearanceState._selectedBackColor;
			case CommandColors.SelectedElementGradient:
				break;
			case CommandColors.CommandBar:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._commandBarBackColor;
			case CommandColors.CommandBarGradient:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._commandBarGradientColor;
			default:
				return Color.Empty;
			}
			return Color.Empty;
		}

		public Color GetDefaultColor(CommandColors commandColor, bool useThemes)
		{
			CommandBarsColorSet commandBarsAppearanceState = null;
			VisualStylePainter renderer = null;
			if ((useThemes && VisualStyleSupport.IsThemingUsable(false)))
			{
				renderer = ((commandColor != CommandColors.Rebar && commandColor != CommandColors.RebarGradient) ? new VisualStylePainter("Toolbar") : new VisualStylePainter("Rebar"));
			}
			else
			{
				useThemes = false;
				commandBarsAppearanceState = new CommandBarsColorSet();
				commandBarsAppearanceState.ApplyVisualStyle();
			}
			switch (commandColor)
			{
			case CommandColors.Border:
				if (useThemes)
				{
					return renderer._borderColor;
				}
				return RenderingHelper.GetSchemeColor(ColorName.ControlDark);
			case CommandColors.CustomizeButton:
				break;
			case CommandColors.CustomizeButtonGradient:
				break;
			case CommandColors.FloatingBar:
				break;
			case CommandColors.FloatingBarCaption:
				if (useThemes)
				{
					return RenderingHelper.GetSchemeColor(ColorName.ActiveCaption);
				}
				return RenderingHelper.GetSchemeColor(ColorName.Highlight);
			case CommandColors.HotElement:
				if (useThemes)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Highlight);
				}
				return commandBarsAppearanceState._hotBackColor;
			case CommandColors.HotElementGradient:
				break;
			case CommandColors.ImageArea:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._imageAreaBackColor;
			case CommandColors.ImageAreaGradient:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._imageAreaGradientColor;
			case CommandColors.InfrequentImageArea:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._infrequentAreaBackColor;
			case CommandColors.InfrequentImageAreaGradient:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._infrequentAreaGradientColor;
			case CommandColors.Menu:
				if (useThemes)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Menu);
				}
				return commandBarsAppearanceState._menuBackColor;
			case CommandColors.PressedElement:
				if (useThemes)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Highlight);
				}
				return commandBarsAppearanceState._pressedBackColor;
			case CommandColors.PressedElementGradient:
				break;
			case CommandColors.Rebar:
				if (useThemes)
				{
					return renderer._rebarColor;
				}
				return commandBarsAppearanceState._rebarBackColor;
			case CommandColors.RebarGradient:
				if (useThemes)
				{
					return renderer._rebarGradientColor;
				}
				return commandBarsAppearanceState._rebarGradientColor;
			case CommandColors.SelectedElement:
				if (useThemes)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Highlight);
				}
				return commandBarsAppearanceState._selectedBackColor;
			case CommandColors.SelectedElementGradient:
				break;
			case CommandColors.CommandBar:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._commandBarBackColor;
			case CommandColors.CommandBarGradient:
				if (useThemes)
				{
					break;
				}
				return commandBarsAppearanceState._commandBarGradientColor;
			default:
				return Color.Empty;
			}
			return Color.Empty;
		}
	}
}
