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
		protected void Deserialize(LayoutReader reader)
		{
			_panelId = (Guid)LayoutReader.GetPropertyValue(reader, "Id", typeof(Guid), Guid.NewGuid());
			PropertyValue propertyValue = reader["TabStateStyles"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_tabStateStyles);
			}
			else
			{
				_tabStateStyles._owner = null;
				_tabStateStyles = new StateStyles(this);
			}
			propertyValue = reader["FormatStyleActiveCaption"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleActiveCaption);
			}
			else
			{
				_formatStyleActiveCaption.Reset();
			}
			propertyValue = reader["FormatStyleCaption"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleCaption);
			}
			else
			{
				_formatStyleCaption.Reset();
			}
			propertyValue = reader["FormatStyleInfoText"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleInfoText);
			}
			else
			{
				_formatStyleInfoText.Reset();
			}
			_activeCaptionMode = (ActiveCaptionMode)LayoutReader.GetPropertyValue(reader, "ActiveCaptionMode", typeof(ActiveCaptionMode), ActiveCaptionMode.UseDefault);
			_captionImage = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			_largeImage = (Image)LayoutReader.GetPropertyValue(reader, "LargeImage", typeof(Image), null);
			_icon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_largeIcon = (Icon)LayoutReader.GetPropertyValue(reader, "LargeIcon", typeof(Icon), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), "");
			_largeImageIndex = (int)LayoutReader.GetPropertyValue(reader, "LargeImageIndex", typeof(int), -1);
			_largeImageKey = (string)LayoutReader.GetPropertyValue(reader, "LargeImageKey", typeof(string), "");
			_captionDisplayMode = (PanelCaptionDisplayMode)LayoutReader.GetPropertyValue(reader, "CaptionDisplayMode", typeof(PanelCaptionDisplayMode), PanelCaptionDisplayMode.UseDefault);
			_autoHideTabDisplay = (TabDisplayMode)LayoutReader.GetPropertyValue(reader, "AutoHideTabDisplay", typeof(TabDisplayMode), TabDisplayMode.UseDefault);
			_captionStyle = (PanelCaptionStyle)LayoutReader.GetPropertyValue(reader, "CaptionStyle", typeof(PanelCaptionStyle), PanelCaptionStyle.UseDefault);
			_captionVisible = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "CaptionVisible", typeof(InheritableBoolean), InheritableBoolean.Default);
			_borderCaption = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "BorderCaption", typeof(InheritableBoolean), InheritableBoolean.Default);
			_borderPanel = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "BorderPanel", typeof(InheritableBoolean), InheritableBoolean.Default);
			_allowResize = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowResize", typeof(InheritableBoolean), InheritableBoolean.Default);
			_allowPanelDrag = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowPanelDrag", typeof(InheritableBoolean), InheritableBoolean.Default);
			_allowPanelDrop = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowPanelDrop", typeof(InheritableBoolean), InheritableBoolean.Default);
			_closeButtonVisible = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "CloseButtonVisible", typeof(InheritableBoolean), InheritableBoolean.Default);
			_autoHideButtonVisible = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AutoHideButtonVisible", typeof(InheritableBoolean), InheritableBoolean.Default);
			// Old layouts store UseActiveCaptionStyle; it applies only when ActiveCaptionMode is absent.
			if (reader["UseActiveCaptionStyle"] != null && reader["ActiveCaptionMode"] == null)
			{
				_useActiveCaptionStyle = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "UseActiveCaptionStyle", typeof(InheritableBoolean), InheritableBoolean.Default);
				switch (_useActiveCaptionStyle)
				{
				case InheritableBoolean.True:
					_activeCaptionMode = ActiveCaptionMode.PanelFocus;
					break;
				case InheritableBoolean.False:
					_activeCaptionMode = ActiveCaptionMode.Never;
					break;
				default:
					_activeCaptionMode = ActiveCaptionMode.UseDefault;
					break;
				}
			}
			_borderCaptionColor = (Color)LayoutReader.GetPropertyValue(reader, "BorderCaptionColor", typeof(Color), Color.Empty);
			_borderPanelColor = (Color)LayoutReader.GetPropertyValue(reader, "BorderPanelColor", typeof(Color), Color.Empty);
			_captionHeight = (int)LayoutReader.GetPropertyValue(reader, "CaptionHeight", typeof(int), -1);
			_minimumSize = (Size)LayoutReader.GetPropertyValue(reader, "MinimumSize", typeof(Size), new Size(-1, -1));
			_maximumSize = (Size)LayoutReader.GetPropertyValue(reader, "MaximumSize", typeof(Size), new Size(-1, -1));
			AutoHide = (bool)LayoutReader.GetPropertyValue(reader, "AutoHide", typeof(bool), false);
			_closed = (bool)LayoutReader.GetPropertyValue(reader, "Closed", typeof(bool), false);
			GetCurrentLayoutState().SetDockStyle((PanelDockStyle)LayoutReader.GetPropertyValue(reader, "DockStyle", typeof(PanelDockStyle), PanelDockStyle.Left));
			_dockState = (PanelDockState)LayoutReader.GetPropertyValue(reader, "DockState", typeof(PanelDockState), PanelDockState.Docked);
			Text = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
			_infoText = (string)LayoutReader.GetPropertyValue(reader, "InfoText", typeof(string), "");
			_useContainerCaption = (bool)LayoutReader.GetPropertyValue(reader, "ContainerCaption", typeof(bool), false);
			_textAlignment = (PanelTextAlignment)LayoutReader.GetPropertyValue(reader, "TextAlignment", typeof(PanelTextAlignment), PanelTextAlignment.UseDefault);
			_imageAlignment = (PanelImageAlignment)LayoutReader.GetPropertyValue(reader, "ImageAlignment", typeof(PanelImageAlignment), PanelImageAlignment.UseDefault);
			_infoTextAlignment = (PanelTextAlignment)LayoutReader.GetPropertyValue(reader, "InfoTextAlignment", typeof(PanelTextAlignment), PanelTextAlignment.UseDefault);
			string tagTypeName = (string)LayoutReader.GetPropertyValue(reader, "TagType", typeof(string), "");
			if (tagTypeName == "")
			{
				base.Tag = LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null);
			}
			else
			{
				base.Tag = LayoutTypePolicy.ConvertOrKeepText((string)LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null), tagTypeName);
			}
			base.Name = (string)LayoutReader.GetPropertyValue(reader, "Name", typeof(string), "");
		}

		protected void Serialize(LayoutWriter info)
		{
			if (_activeCaptionMode != ActiveCaptionMode.UseDefault)
			{
				info.AddValue("ActiveCaptionMode", _activeCaptionMode);
			}
			if (_tabStateStyles.ShouldSerialize())
			{
				info.AddValue("TabStateStyles", _tabStateStyles);
			}
			if (_captionImage != null)
			{
				info.AddValue("Image", _captionImage);
			}
			if (_largeImage != null)
			{
				info.AddValue("LargeImage", _largeImage);
			}
			if (_icon != null)
			{
				info.AddValue("Icon", _icon);
			}
			if (_largeIcon != null)
			{
				info.AddValue("LargeIcon", _largeIcon);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_largeImageIndex != -1)
			{
				info.AddValue("LargeImageIndex", _largeImageIndex);
			}
			if (_largeImageKey.Length > 0)
			{
				info.AddValue("LargeImageKey", _largeImageKey);
			}
			if (_captionDisplayMode != PanelCaptionDisplayMode.UseDefault)
			{
				info.AddValue("CaptionDisplayMode", _captionDisplayMode);
			}
			if (_autoHideTabDisplay != TabDisplayMode.UseDefault)
			{
				info.AddValue("AutoHideTabDisplay", _autoHideTabDisplay);
			}
			if (_captionStyle != PanelCaptionStyle.UseDefault)
			{
				info.AddValue("CaptionStyle", _captionStyle);
			}
			if (_captionVisible != InheritableBoolean.Default)
			{
				info.AddValue("CaptionVisible", _captionVisible);
			}
			info.AddValue("Id", _panelId);
			if (!_formatStyleActiveCaption.IsDefaultStyle())
			{
				info.AddValue("FormatStyleActiveCaption", _formatStyleActiveCaption);
			}
			if (!_formatStyleCaption.IsDefaultStyle())
			{
				info.AddValue("FormatStyleCaption", _formatStyleCaption);
			}
			if (!_formatStyleInfoText.IsDefaultStyle())
			{
				info.AddValue("FormatStyleInfoText", _formatStyleInfoText);
			}
			if (_borderCaption != InheritableBoolean.Default)
			{
				info.AddValue("BorderCaption", _borderCaption);
			}
			if (_borderCaptionColor != Color.Empty)
			{
				info.AddValue("BorderCaptionColor", _borderCaptionColor);
			}
			if (_borderPanel != InheritableBoolean.Default)
			{
				info.AddValue("BorderPanel", _borderPanel);
			}
			if (_borderPanelColor != Color.Empty)
			{
				info.AddValue("BorderPanelColor", _borderPanelColor);
			}
			if (_captionHeight != -1)
			{
				info.AddValue("CaptionHeight", _captionHeight);
			}
			if (_minimumSize != new Size(-1, -1))
			{
				info.AddValue("MinimumSize", _minimumSize);
			}
			if (_maximumSize != new Size(-1, -1))
			{
				info.AddValue("MaximumSize", _maximumSize);
			}
			if (_allowResize != InheritableBoolean.Default)
			{
				info.AddValue("AllowResize", _allowResize);
			}
			if (_allowPanelDrag != InheritableBoolean.Default)
			{
				info.AddValue("AllowPanelDrag", _allowPanelDrag);
			}
			if (ShouldSerializeAllowPanelDrop())
			{
				info.AddValue("AllowPanelDrop", _allowPanelDrop);
			}
			if (_closeButtonVisible != InheritableBoolean.Default)
			{
				info.AddValue("CloseButtonVisible", _closeButtonVisible);
			}
			if (_useContainerCaption)
			{
				info.AddValue("ContainerCaption", _useContainerCaption);
			}
			if (_autoHideButtonVisible != InheritableBoolean.Default)
			{
				info.AddValue("AutoHideButtonVisible", _autoHideButtonVisible);
			}
			if (_autoHide)
			{
				info.AddValue("AutoHide", _autoHide);
			}
			if (_closed)
			{
				info.AddValue("Closed", _closed);
			}
			if (IsRootPanel() && DockStyle != PanelDockStyle.Left)
			{
				info.AddValue("DockStyle", DockStyle);
			}
			if (_dockState != PanelDockState.Docked)
			{
				info.AddValue("DockState", DockState);
			}
			if (Text != "")
			{
				info.AddValue("Text", Text);
			}
			else if (_text != "")
			{
				info.AddValue("Text", _text);
			}
			if (_infoText != "")
			{
				info.AddValue("InfoText", _infoText);
			}
			if (_textAlignment != PanelTextAlignment.UseDefault)
			{
				info.AddValue("TextAlignment", _textAlignment);
			}
			if (_imageAlignment != PanelImageAlignment.UseDefault)
			{
				info.AddValue("ImageAlignment", _imageAlignment);
			}
			if (_infoTextAlignment != PanelTextAlignment.UseDefault)
			{
				info.AddValue("InfoTextAlignment", _infoTextAlignment);
			}
			if (base.Tag != null)
			{
				if (!(base.Tag is string))
				{
					info.AddValue("TagType", base.Tag.GetType().FullName);
				}
				info.AddValue("Tag", base.Tag);
			}
			string name = base.Name;
			if (Site != null)
			{
				name = Site.Name;
			}
			if (name != null && name.Length > 0)
			{
				info.AddValue("Name", name);
			}
		}

		internal object GetLayoutLoadState()
		{
			return _layoutLoadState;
		}

		internal void SetLayoutLoadState(object value)
		{
			_layoutLoadState = value;
		}

		internal DockLayoutState GetCurrentLayoutState()
		{
			if (_dockState == PanelDockState.Docked)
			{
				return _dockedLayoutState;
			}
			return _floatingLayoutState;
		}

		private void SyncLayoutStateParent()
		{
			if (!_shouldSkipLayoutSync)
			{
				if (_parentGroup == null)
				{
					GetCurrentLayoutState().Reparent(null);
				}
				else
				{
					GetCurrentLayoutState().Reparent(_parentGroup.GetDockLayoutState(_dockState));
				}
			}
		}
	}
}
