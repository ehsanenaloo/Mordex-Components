using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxDockManager
	{
		private void InitializeDefaults()
		{
			_dockInfo = new DockLayoutSnapshot(this);
			_panels = new DockPanelCollection(this, setOwner: true);
			_tabbedMdiSettings = new TabbedMdiSettings(this);
			_layouts = new DockPanelLayoutCollection(this);
			_formatStyleTabStrip = new FormatStyle(this);
			_backColorAutoHideStrip = Color.Empty;
			_backColorGradientAutoHideStrip = Color.Empty;
			_backColorGradientSplitter = Color.Empty;
			_backColorResizeBar = Color.Empty;
			_backColorGradientResizeBar = Color.Empty;
			_backColorSplitter = Color.Empty;
			_defaultTabGroupStyle = TabGroupStyle.Tab;
			CreateRenderers();
			_panelPadding = new PanelPadding(this, DefaultPanelPadding, DefaultPanelPadding, DefaultPanelPadding, DefaultPanelPadding);
			_defaultPanelSettings = new DefaultPanelSettings(this);
			_bottomStrip = new MxAutoHideStrip(this, AutoHideStripDock.Bottom);
			_leftStrip = new MxAutoHideStrip(this, AutoHideStripDock.Left);
			_rightStrip = new MxAutoHideStrip(this, AutoHideStripDock.Right);
			_topStrip = new MxAutoHideStrip(this, AutoHideStripDock.Top);
			_splitterSize = DefaultSplitterSize;
			_allowPanelDrag = true;
			_allowPanelDrop = true;
			_allowPanelResize = true;
			_dockingStyleMode = DockingStyleMode.VS2005;
		}

		internal void CreateMdiRenderer()
		{
			_mdiRenderer = null;
			if (_tabbedMdiSettings.ShouldUseThemes())
			{
				return;
			}
			_mdiRenderer = new ThemedDockRenderer(this);
		}

		private void CreateRenderers()
		{
			UpdateThemeListening();
			_panelRenderer = new ThemedDockRenderer(this);
			CreateMdiRenderer();
		}

		internal DockManagerRendererBase GetPanelRenderer()
		{
			return _panelRenderer;
		}

		internal DockManagerRendererBase GetMdiRenderer()
		{
			if (_mdiRenderer == null)
			{
				return _panelRenderer;
			}
			return _mdiRenderer;
		}

		internal FormatStyle GetTabStripStyle()
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetPanelAppearance();
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.TabStrip);
			uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.TabStripGradient);
			return uiFormatStyle.MergeStyle(_formatStyleTabStrip);
		}

		internal FormatStyle GetMdiTabStripStyle()
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetMdiAppearance();
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.MdiTabStrip);
			uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.MdiTabStripGradient);
			return uiFormatStyle.MergeStyle(TabbedMdiSettings.TabStripFormatStyle);
		}

		internal FormatStyle GetOutlookButtonStyle(ButtonState buttonState)
		{
			FormatStyle uiFormatStyle = GetOutlookButtonStateStyle(false, false, false, false);
			bool isPressed = buttonState == ButtonState.Pressed;
			bool isHot = buttonState == ButtonState.Hot;
			bool isSelected = buttonState == ButtonState.Selected;
			return uiFormatStyle.MergeStyle(GetOutlookButtonStateStyle(isHot, isSelected, isPressed, false));
		}

		internal FormatStyle GetOutlookButtonStateStyle(bool isHot, bool isSelected, bool isPressed, bool isDisabled)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetPanelAppearance();
			FormatStyle uiFormatStyle = new FormatStyle();
			if (isDisabled)
			{
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DisabledText);
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.OutlookButton);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.OutlookButtonGradient);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.DisabledFormatStyle);
			}
			if (isPressed)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElementText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.PressedFormatStyle);
			}
			if (isSelected)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElementText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.SelectedFormatStyle);
			}
			if (isHot)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElementText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.HotFormatStyle);
			}
			uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.OutlookButton);
			uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.OutlookButtonGradient);
			uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.OutlookButtonText);
			return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.FormatStyle);
		}

		internal FormatStyle GetAutoHideTabStyle(bool isHot, bool isSelected, bool isPressed, bool isDisabled)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetPanelAppearance();
			FormatStyle uiFormatStyle = new FormatStyle();
			if (isDisabled)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.AutoHideTab);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.AutoHideTabGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DisabledText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.DisabledFormatStyle);
			}
			if (isPressed)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElementText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.PressedFormatStyle);
			}
			if (isSelected)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElementText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.SelectedFormatStyle);
			}
			if (isHot)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElementText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.HotFormatStyle);
			}
			uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.AutoHideTab);
			uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.AutoHideTabGradient);
			uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.AutoHideTabText);
			return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.FormatStyle);
		}

		internal FormatStyle GetPanelTabStyle(bool isHot, bool isSelected, bool isPressed, bool isDisabled)
		{
			FormatStyle uiFormatStyle = new FormatStyle();
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetPanelAppearance();
			if (isDisabled)
			{
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DisabledText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.DisabledFormatStyle);
			}
			if (isPressed)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTab);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTabGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTabText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.PressedFormatStyle);
			}
			if (isSelected)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTab);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTabGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTabText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.SelectedFormatStyle);
			}
			if (isHot)
			{
				return _defaultPanelSettings.TabStateStyles.HotFormatStyle;
			}
			if (mordexColorSchemeAppearanceState.IsDarkScheme())
			{
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DarkCaptionText);
			}
			else
			{
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.TabText);
			}
			return uiFormatStyle.MergeStyle(_defaultPanelSettings.TabStateStyles.FormatStyle);
		}

		private MxColorSchemeAppearanceState GetMdiAppearance()
		{
			return GetMdiRenderer().GetColorSchemeAppearance();
		}

		private MxColorSchemeAppearanceState GetPanelAppearance()
		{
			return _panelRenderer.GetColorSchemeAppearance();
		}

		internal FormatStyle GetMdiTabStyle(ButtonState buttonState, bool useElementColors)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetMdiAppearance();
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.TabText);
			switch (buttonState)
			{
			case ButtonState.Disabled:
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DisabledText);
				break;
			case ButtonState.Pressed:
				if (useElementColors)
				{
					uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElement);
					uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElementGradient);
					uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElementText);
					break;
				}
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTab);
				uiFormatStyle.BackColorGradient = Color.Empty;
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTabText);
				uiFormatStyle.FontBold = TriState.True;
				break;
			case ButtonState.Selected:
				if (useElementColors)
				{
					uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElement);
					uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElementGradient);
					uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElementText);
					break;
				}
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTab);
				uiFormatStyle.BackColorGradient = Color.Empty;
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedTabText);
				uiFormatStyle.FontBold = TriState.True;
				break;
			case ButtonState.Hot:
				if (useElementColors)
				{
					uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElement);
					uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElementGradient);
					uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElementText);
				}
				break;
			}
			return uiFormatStyle;
		}

		internal FormatStyle GetMdiTabStateStyle(ButtonState buttonState)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetMdiAppearance();
			switch (buttonState)
			{
			case ButtonState.Disabled:
				return _tabbedMdiSettings.TabStateStyles.DisabledFormatStyle;
			case ButtonState.Pressed:
			{
				FormatStyle uiFormatStyle = new FormatStyle();
				return uiFormatStyle.MergeStyle(_tabbedMdiSettings.TabStateStyles.PressedFormatStyle);
			}
			case ButtonState.Selected:
			{
				FormatStyle selectedStyle = new FormatStyle();
				return selectedStyle.MergeStyle(_tabbedMdiSettings.TabStateStyles.SelectedFormatStyle);
			}
			case ButtonState.Hot:
				return _tabbedMdiSettings.TabStateStyles.HotFormatStyle;
			default:
				return _tabbedMdiSettings.TabStateStyles.FormatStyle;
			}
		}

		internal FormatStyle GetPlainOutlookButtonStyle(bool isHot, bool isSelected, bool isPressed, bool isDisabled)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetPanelAppearance();
			FormatStyle uiFormatStyle = new FormatStyle();
			if (isDisabled)
			{
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DisabledText);
			}
			else if (isPressed)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.PressedElementText);
			}
			else if (isSelected)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.SelectedElementText);
			}
			else if (isHot)
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElement);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElementGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.HotElementText);
			}
			else
			{
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.OutlookButton);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.OutlookButtonGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.OutlookButtonText);
			}
			return uiFormatStyle;
		}

		internal FormatStyle GetActiveCaptionStyle()
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetPanelAppearance();
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.ActiveCaption);
			uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.ActiveCaptionGradient);
			uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.ActiveCaptionText);
			return uiFormatStyle.MergeStyle(_defaultPanelSettings.ActiveCaptionFormatStyle);
		}

		internal FormatStyle GetDisabledTextStyle()
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetPanelAppearance();
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DisabledText);
			return uiFormatStyle;
		}

		internal FormatStyle GetCaptionStyle(PanelCaptionStyle panelCaptionStyle, bool useMdiAppearance)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = ((!useMdiAppearance) ? GetPanelAppearance() : GetMdiAppearance());
			FormatStyle uiFormatStyle = new FormatStyle();
			switch (panelCaptionStyle)
			{
			case PanelCaptionStyle.Dark:
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DarkCaption);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.DarkCaptionGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.DarkCaptionText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.DarkCaptionFormatStyle);
			case PanelCaptionStyle.Light:
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.LightCaption);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.LightCaptionGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.LightCaptionText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.LightCaptionFormatStyle);
			default:
				uiFormatStyle.BackColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.Caption);
				uiFormatStyle.BackColorGradient = mordexColorSchemeAppearanceState.GetColor(PanelColors.CaptionGradient);
				uiFormatStyle.ForeColor = mordexColorSchemeAppearanceState.GetColor(PanelColors.CaptionText);
				return uiFormatStyle.MergeStyle(_defaultPanelSettings.CaptionFormatStyle);
			}
		}

		internal FormatStyle GetInnerContainerStyle()
		{
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.BackColor = GetPanelAppearance().GetColor(PanelColors.InnerContainer);
			return uiFormatStyle.MergeStyle(_defaultPanelSettings.InnerContainerFormatStyle);
		}

		public Color GetDefaultColor(PanelColors panelColor)
		{
			return GetPanelAppearance().GetColor(panelColor);
		}

		internal static Image GetButtonImage(int buttonId, DefaultPanelSettings defaultPanelSettings)
		{
			switch (buttonId)
			{
			case 0:
				if (defaultPanelSettings.CloseButtonImage != null)
				{
					return FitButtonImage(defaultPanelSettings.CloseButtonImage);
				}
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.CloseImage());
			case 9:
				if (defaultPanelSettings.CloseButtonImage != null)
				{
					return FitButtonImage(defaultPanelSettings.CloseButtonImage);
				}
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.CloseVs2010());
			case 1:
				if (defaultPanelSettings.PinButtonImage != null)
				{
					return FitButtonImage(defaultPanelSettings.PinButtonImage);
				}
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.PinImage());
			case 2:
				if (defaultPanelSettings.UnpinButtonImage != null)
				{
					return FitButtonImage(defaultPanelSettings.UnpinButtonImage);
				}
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.UnpinImage());
			case 3:
				if (defaultPanelSettings.PinButtonImage != null)
				{
					return FitButtonImage(defaultPanelSettings.PinButtonImage);
				}
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.OfficePinImage());
			case 4:
				if (defaultPanelSettings.UnpinButtonImage != null)
				{
					return FitButtonImage(defaultPanelSettings.UnpinButtonImage);
				}
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.OfficeUnpinImage());
			case 7:
				if (defaultPanelSettings.PinButtonImage != null)
				{
					return FitButtonImage(defaultPanelSettings.PinButtonImage);
				}
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.OfficePinVs2010());
			case 8:
				if (defaultPanelSettings.UnpinButtonImage != null)
				{
					return FitButtonImage(defaultPanelSettings.UnpinButtonImage);
				}
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.OfficeUnpinVs2010());
			case 5:
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.DownArrowImage());
			case 6:
				return RenderingHelper.MakeMagentaTransparent(WindowsResources.ActiveFiles());
			default:
				return null;
			}
		}

		private static Image FitButtonImage(Image image)
		{
			if (image.Width != ButtonImageSize || image.Height != ButtonImageSize)
			{
				return new Bitmap(image, new Size(ButtonImageSize, ButtonImageSize));
			}
			return image;
		}
	}
}
