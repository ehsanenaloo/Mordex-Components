using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public abstract partial class DockPanelBase
	{
		protected override void OnPaint(PaintEventArgs e)
		{
			if (PanelManager == null)
			{
				base.OnPaint(e);
				return;
			}
			if (!_splitterBounds.IsEmpty)
			{
				PanelManager.GetPanelRenderer().DrawBackground(e.Graphics, this, _splitterBounds);
			}
			if (HasPanelBorder())
			{
				Rectangle borderRect = _displayBounds;
				borderRect.Inflate(1, 1);
				if (ParentGroup != null)
				{
					BorderEdges borderEdges = ParentGroup.GetChildBorderEdges(this);
					if ((borderEdges & BorderEdges.Bottom) == 0)
					{
						borderRect.Height++;
					}
					if ((borderEdges & BorderEdges.Top) == 0)
					{
						borderRect.Y--;
						borderRect.Height++;
					}
					if ((borderEdges & BorderEdges.Left) == 0)
					{
						borderRect.X--;
						borderRect.Width++;
					}
					if ((borderEdges & BorderEdges.Right) == 0)
					{
						borderRect.Width++;
					}
				}
				PanelManager.GetPanelRenderer().DrawPanelBorder(e.Graphics, this, borderRect);
			}
			if (!ContainerCaption)
			{
				DrawCaptionBar(e.Graphics);
			}
			base.OnPaint(e);
		}

		private FormatStyle GetTabStateFormatStyle(bool isHot, bool isSelected, bool isPressed, bool isDisabled)
		{
			if (isDisabled)
			{
				return _tabStateStyles.DisabledFormatStyle;
			}
			if (isPressed)
			{
				return _tabStateStyles.PressedFormatStyle;
			}
			if (isSelected)
			{
				return _tabStateStyles.SelectedFormatStyle;
			}
			if (isHot)
			{
				return _tabStateStyles.HotFormatStyle;
			}
			return _tabStateStyles.FormatStyle;
		}

		internal bool IsCaptionActive()
		{
			ActiveCaptionMode activeCaptionMode = ActiveCaptionMode;
			if (activeCaptionMode == ActiveCaptionMode.UseDefault)
			{
				activeCaptionMode = ((PanelManager == null) ? ActiveCaptionMode.PanelFocus : PanelManager.DefaultPanelSettings.ActiveCaptionMode);
			}
			switch (activeCaptionMode)
			{
			case ActiveCaptionMode.PanelFocus:
				if (_floatingWindow != null && PanelManager != null)
				{
					// A window that is not the active window shows an inactive title, whatever holds the focus inside.
					return _floatingWindow.IsWindowActive;
				}
				return base.ContainsFocus;
			case ActiveCaptionMode.ContainerCaptionFocus:
				if (_containerCaptionControl != null)
				{
					return _containerCaptionControl.ContainsFocus;
				}
				break;
			}
			return false;
		}

		internal bool HasCaptionBorder()
		{
			if (_borderCaption == InheritableBoolean.Default)
			{
				if (PanelManager != null)
				{
					return PanelManager.DefaultPanelSettings.BorderCaption;
				}
			}
			else if (_borderCaption == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal Color GetCaptionBorderColor()
		{
			if (_borderCaptionColor.IsEmpty)
			{
				if (PanelManager != null)
				{
					return PanelManager.DefaultPanelSettings.BorderCaptionColor;
				}
				return RenderingHelper.GetSchemeColor(ColorName.ControlDark);
			}
			return _borderCaptionColor;
		}

		internal int GetAutoHideButtonImageIndex()
		{
			if (PanelManager != null)
			{
			}
			return AutoHideImageIndex;
		}

		internal int GetDockedButtonImageIndex()
		{
			if (PanelManager != null)
			{
			}
			return DockedImageIndex;
		}

		internal Font GetCaptionFont(FormatStyle formatStyle)
		{
			Font font = ((PanelManager == null || PanelManager.ContainerControl == null) ? Font : PanelManager.ContainerControl.Font);
			return formatStyle.ResolveFont(font);
		}

		internal int GetCaptionHeight()
		{
			if (PanelManager != null && PanelManager.DefaultPanelSettings.CaptionHeight != -1)
			{
				return PanelManager.DefaultPanelSettings.CaptionHeight;
			}
			if (_captionHeight != -1)
			{
				return _captionHeight;
			}
			FormatStyle uiFormatStyle = GetCaptionFormatStyle(IsFloatingRoot(), base.Enabled);
			Font font = GetCaptionFont(uiFormatStyle);
			int captionSize = -1;
			if (PanelManager != null)
			{
				captionSize = PanelManager.GetPanelRenderer().GetCaptionSize(this, font, uiFormatStyle);
			}
			if (captionSize == -1)
			{
				captionSize = 0;
			}
			return captionSize;
		}

		internal int GetPanelCaptionHeight()
		{
			if (_captionHeight != -1)
			{
				return _captionHeight;
			}
			return GetCaptionHeight();
		}

		internal FormatStyle GetCaptionFormatStyle(bool isFloating, bool isEnabled)
		{
			FormatStyle uiFormatStyle = ((PanelManager == null) ? new FormatStyle() : PanelManager.GetCaptionStyle(GetEffectiveCaptionStyle(), false));
			uiFormatStyle = uiFormatStyle.MergeStyle(_formatStyleCaption);
			if (!isEnabled)
			{
				if (PanelManager != null)
				{
					uiFormatStyle = uiFormatStyle.MergeStyle(PanelManager.GetDisabledTextStyle());
				}
			}
			else if (IsCaptionActive())
			{
				if (PanelManager != null)
				{
					uiFormatStyle = uiFormatStyle.MergeStyle(PanelManager.GetActiveCaptionStyle());
				}
				uiFormatStyle = uiFormatStyle.MergeStyle(_formatStyleActiveCaption);
			}
			return uiFormatStyle;
		}

		internal FormatStyle GetInfoTextFormatStyle(bool isFloating)
		{
			FormatStyle uiFormatStyle = GetCaptionFormatStyle(isFloating, base.Enabled);
			if (PanelManager != null)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(PanelManager.DefaultPanelSettings.InfoTextFormatStyle);
			}
			return uiFormatStyle.MergeStyle(_formatStyleInfoText);
		}

		internal int GetSplitterSize()
		{
			if (PanelManager != null)
			{
				return PanelManager.SplitterSize;
			}
			return DefaultSplitterSize;
		}

		internal void HandleCaptionButtonClick(PanelButtonType panelButtonType)
		{
			switch (panelButtonType)
			{
			case PanelButtonType.CloseButton:
				if (ParentGroup == null || !ParentGroup.HandleCloseButtonClick(this))
				{
					Closed = true;
				}
				break;
			case PanelButtonType.AutoHideButton:
				if (ParentGroup == null || !ParentGroup.HandleAutoHideButtonClick(this))
				{
					AutoHide = !AutoHide;
					if (!AutoHide)
					{
						Activate();
					}
				}
				break;
			}
		}

		internal void HandleCaptionButtonDown(Rectangle buttonBounds, PanelButtonType panelButtonType)
		{
			if (panelButtonType == PanelButtonType.CaptionButton && ParentGroup != null)
			{
				ParentGroup.HandleChildCaptionButtonDown(this, buttonBounds);
			}
		}

		private void DrawCaptionBar(Graphics graphics)
		{
			if (_captionRenderer != null && PanelManager != null && (ParentGroup == null || !ParentGroup.PaintChildCaptionBar(graphics, _captionRenderer)))
			{
				_captionRenderer.DrawCaption(graphics, false, 0, 0);
			}
		}

		internal Color GetPanelBorderColor()
		{
			Color result = ((PanelManager == null) ? RenderingHelper.GetSchemeColor(ColorName.ControlDark) : PanelManager.DefaultPanelSettings.BorderPanelColor);
			if (!_borderPanelColor.IsEmpty)
			{
				result = _borderPanelColor;
			}
			return result;
		}

		internal bool HasPanelBorder()
		{
			return HasBorderPanel;
		}

		internal CommandImageSource GetLargeImageSource()
		{
			CommandImageSource imageInfo = new CommandImageSource();
			if (_largeIcon != null)
			{
				imageInfo._icon = _largeIcon;
			}
			else if (_largeImage != null)
			{
				imageInfo._image = _largeImage;
			}
			else if (PanelManager != null)
			{
				imageInfo._imageIndex = _largeImageIndex;
				imageInfo._imageKey = _largeImageKey;
				imageInfo._imageList = PanelManager.LargeImageList;
			}
			if (imageInfo.IsEmpty())
			{
				return GetImageSource();
			}
			return imageInfo;
		}

		internal CommandImageSource GetImageSource()
		{
			CommandImageSource imageInfo = new CommandImageSource();
			if (_icon != null)
			{
				imageInfo._icon = _icon;
			}
			else if (_captionImage != null)
			{
				imageInfo._image = _captionImage;
			}
			else if (PanelManager != null)
			{
				imageInfo._imageList = PanelManager.ImageList;
				imageInfo._imageIndex = _imageIndex;
				imageInfo._imageKey = _imageKey;
			}
			return imageInfo;
		}

		internal FormatStyle GetTabFormatStyle(ButtonState buttonState)
		{
			if (PanelManager == null)
			{
				return _tabStateStyles.FormatStyle;
			}
			FormatStyle uiFormatStyle = PanelManager.GetPanelTabStyle(false, false, false, false);
			uiFormatStyle = uiFormatStyle.MergeStyle(_tabStateStyles.FormatStyle);
			bool isPressed = buttonState == ButtonState.Pressed;
			bool isHot = buttonState == ButtonState.Hot;
			bool isSelected = buttonState == ButtonState.Selected;
			FormatStyle stateFormatStyle = PanelManager.GetPanelTabStyle(isHot, isSelected, isPressed, !base.Enabled);
			uiFormatStyle = uiFormatStyle.MergeStyle(stateFormatStyle);
			return uiFormatStyle.MergeStyle(GetTabStateFormatStyle(isHot, isSelected, isPressed, !base.Enabled));
		}

		internal FormatStyle GetAutoHideTabFormatStyle(ButtonState buttonState)
		{
			FormatStyle uiFormatStyle = PanelManager.GetAutoHideTabStyle(false, false, false, false);
			uiFormatStyle = uiFormatStyle.MergeStyle(_tabStateStyles.FormatStyle);
			bool isPressed = buttonState == ButtonState.Pressed;
			bool isHot = buttonState == ButtonState.Hot;
			bool isSelected = buttonState == ButtonState.Selected;
			FormatStyle stateFormatStyle = PanelManager.GetAutoHideTabStyle(isHot, isSelected, isPressed, !base.Enabled);
			uiFormatStyle = uiFormatStyle.MergeStyle(stateFormatStyle);
			return uiFormatStyle.MergeStyle(GetTabStateFormatStyle(isHot, isSelected, isPressed, !base.Enabled));
		}

		internal FormatStyle GetNavigatorButtonFormatStyle(ButtonState buttonState)
		{
			if (PanelManager == null)
			{
				return new FormatStyle();
			}
			FormatStyle uiFormatStyle = PanelManager.GetOutlookButtonStateStyle(false, false, false, false);
			uiFormatStyle = uiFormatStyle.MergeStyle(_tabStateStyles.FormatStyle);
			bool isPressed = buttonState == ButtonState.Pressed;
			bool isHot = buttonState == ButtonState.Hot;
			bool isSelected = buttonState == ButtonState.Selected;
			FormatStyle stateFormatStyle = PanelManager.GetOutlookButtonStateStyle(isHot, isSelected, isPressed, !base.Enabled);
			uiFormatStyle = uiFormatStyle.MergeStyle(stateFormatStyle);
			uiFormatStyle.BackColorGradient = stateFormatStyle.BackColorGradient;
			return uiFormatStyle.MergeStyle(GetTabStateFormatStyle(isHot, isSelected, isPressed, !base.Enabled));
		}

		internal FormatStyle GetCaptionButtonFormatStyle(FormatStyle captionFormatStyle, ButtonState buttonState)
		{
			bool isPressed = buttonState == ButtonState.Pressed;
			bool isHot = buttonState == ButtonState.Hot;
			bool isSelected = buttonState == ButtonState.Selected;
			FormatStyle uiFormatStyle = PanelManager.GetPlainOutlookButtonStyle(isHot, isSelected, isPressed, !base.Enabled);
			return captionFormatStyle.MergeStyle(uiFormatStyle);
		}

		internal PanelTextAlignment GetEffectiveTextAlignment()
		{
			if (_textAlignment == PanelTextAlignment.UseDefault)
			{
				if (PanelManager == null)
				{
					return PanelTextAlignment.Near;
				}
				return PanelManager.DefaultPanelSettings.TextAlignment;
			}
			return _textAlignment;
		}

		internal PanelImageAlignment GetEffectiveImageAlignment()
		{
			if (_imageAlignment == PanelImageAlignment.UseDefault)
			{
				if (PanelManager == null)
				{
					return PanelImageAlignment.Near;
				}
				return PanelManager.DefaultPanelSettings.ImageAlignment;
			}
			return _imageAlignment;
		}

		internal PanelTextAlignment GetEffectiveInfoTextAlignment()
		{
			if (_infoTextAlignment == PanelTextAlignment.UseDefault)
			{
				if (PanelManager == null)
				{
					return PanelTextAlignment.Far;
				}
				return PanelManager.DefaultPanelSettings.InfoTextAlignment;
			}
			return _infoTextAlignment;
		}
	}
}
