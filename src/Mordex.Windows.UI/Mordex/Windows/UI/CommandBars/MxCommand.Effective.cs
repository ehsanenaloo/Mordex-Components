using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommand
	{
		internal Color GetEffectiveImageReplaceableColor()
		{
			if (_imageReplaceableColor.IsEmpty && _baseCommand != null)
			{
				return _baseCommand.GetEffectiveImageReplaceableColor();
			}
			return _imageReplaceableColor;
		}

		internal Shortcut GetEffectiveShortcut()
		{
			if (!_isShortcutSet && _baseCommand != null)
			{
				return _baseCommand.GetEffectiveShortcut();
			}
			return _shortcut;
		}

		internal string GetEffectiveText()
		{
			if (_text.Length > 0)
			{
				return _text;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveText();
			}
			return _text;
		}

		internal Icon GetEffectiveIcon()
		{
			if (_icon != null)
			{
				return _icon;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveIcon();
			}
			return _icon;
		}

		internal Image GetEffectiveImage()
		{
			if (_commandImage != null)
			{
				return _commandImage;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveImage();
			}
			return _commandImage;
		}

		internal int GetEffectiveDisabledImageIndex()
		{
			if (_disabledImageIndex != -1)
			{
				return _disabledImageIndex;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveDisabledImageIndex();
			}
			return _disabledImageIndex;
		}

		internal string GetEffectiveDisabledImageKey()
		{
			if (_disabledImageKey.Length > 0)
			{
				return _disabledImageKey;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveDisabledImageKey();
			}
			return _disabledImageKey;
		}

		internal int GetEffectiveSelectedImageIndex()
		{
			if (_selectedImageIndex != -1)
			{
				return _selectedImageIndex;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveSelectedImageIndex();
			}
			return _selectedImageIndex;
		}

		internal string GetEffectiveSelectedImageKey()
		{
			if (_selectedImageKey.Length > 0)
			{
				return _selectedImageKey;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveSelectedImageKey();
			}
			return _selectedImageKey;
		}

		internal int GetEffectiveImageIndex()
		{
			if (_imageIndex != -1)
			{
				return _imageIndex;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveImageIndex();
			}
			return _imageIndex;
		}

		internal string GetEffectiveImageKey()
		{
			if (_imageKey.Length > 0)
			{
				return _imageKey;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveImageKey();
			}
			return _imageKey;
		}

		internal int GetEffectiveHotImageIndex()
		{
			if (_hotImageIndex != -1)
			{
				return _hotImageIndex;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveHotImageIndex();
			}
			return _hotImageIndex;
		}

		internal string GetEffectiveHotImageKey()
		{
			if (_hotImageKey.Length > 0)
			{
				return _hotImageKey;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveHotImageKey();
			}
			return _hotImageKey;
		}

		internal int GetEffectiveLargeDisabledImageIndex()
		{
			if (_largeDisabledImageIndex != -1)
			{
				return _largeDisabledImageIndex;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveLargeDisabledImageIndex();
			}
			return _largeDisabledImageIndex;
		}

		internal string GetEffectiveLargeDisabledImageKey()
		{
			if (_largeDisabledImageKey.Length > 0)
			{
				return _largeDisabledImageKey;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveLargeDisabledImageKey();
			}
			return _largeDisabledImageKey;
		}

		internal int GetEffectiveLargeSelectedImageIndex()
		{
			if (_largeSelectedImageIndex != -1)
			{
				return _largeSelectedImageIndex;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveLargeSelectedImageIndex();
			}
			return _largeSelectedImageIndex;
		}

		internal string GetEffectiveLargeSelectedImageKey()
		{
			if (_largeSelectedImageKey.Length > 0)
			{
				return _largeSelectedImageKey;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveLargeSelectedImageKey();
			}
			return _largeSelectedImageKey;
		}

		internal int GetEffectiveLargeImageIndex()
		{
			if (_largeImageIndex != -1)
			{
				return _largeImageIndex;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveLargeImageIndex();
			}
			return _largeImageIndex;
		}

		internal string GetEffectiveLargeImageKey()
		{
			if (_largeImageKey.Length > 0)
			{
				return _largeImageKey;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveLargeImageKey();
			}
			return _largeImageKey;
		}

		internal int GetEffectiveLargeHotImageIndex()
		{
			if (_largeHotImageIndex != -1)
			{
				return _largeHotImageIndex;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveLargeHotImageIndex();
			}
			return _largeHotImageIndex;
		}

		internal string GetEffectiveLargeHotImageKey()
		{
			if (_largeHotImageKey.Length > 0)
			{
				return _largeHotImageKey;
			}
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveLargeHotImageKey();
			}
			return _largeHotImageKey;
		}

		internal bool CanFloatChildren()
		{
			if (_allowFloatingChildren == InheritableBoolean.Default)
			{
				if (_baseCommand != null)
				{
					return _baseCommand.CanFloatChildren();
				}
				return false;
			}
			if (_allowFloatingChildren == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal CommandStyle GetEffectiveCommandStyle()
		{
			if (_commandStyle == CommandStyle.Default && _baseCommand != null)
			{
				return _baseCommand.GetEffectiveCommandStyle();
			}
			return _commandStyle;
		}

		internal CommandAlignment GetEffectiveAlignment()
		{
			if (_alignment == CommandAlignment.Default)
			{
				if (_baseCommand != null)
				{
					return _baseCommand.GetEffectiveAlignment();
				}
				return CommandAlignment.Near;
			}
			return _alignment;
		}

		internal DropDownButtonStyle GetEffectiveDropDownButtonStyle()
		{
			if (_dropDownButtonStyle == DropDownButtonStyle.Default)
			{
				if (_baseCommand != null)
				{
					return _baseCommand.GetEffectiveDropDownButtonStyle();
				}
				return DropDownButtonStyle.DropDown;
			}
			return _dropDownButtonStyle;
		}

		internal TextImageRelation GetEffectiveTextImageRelation()
		{
			if (_textImageRelation == TextImageRelation.Default && _baseCommand != null)
			{
				return _baseCommand.GetEffectiveTextImageRelation();
			}
			return _textImageRelation;
		}

		internal ContentAlignment GetEffectiveTextAlignment()
		{
			if (_textAlignment == ContentAlignment.Default)
			{
				if (_baseCommand != null)
				{
					return _baseCommand.GetEffectiveTextAlignment();
				}
				return ContentAlignment.MiddleCenter;
			}
			return _textAlignment;
		}

		internal ContentAlignment GetEffectiveImageAlignment()
		{
			if (_imageAlignment == ContentAlignment.Default)
			{
				if (_baseCommand != null)
				{
					return _baseCommand.GetEffectiveImageAlignment();
				}
				return ContentAlignment.MiddleCenter;
			}
			return _imageAlignment;
		}

		internal Size GetEffectiveSize()
		{
			if (_commandSize.IsEmpty && _baseCommand != null)
			{
				return _baseCommand.GetEffectiveSize();
			}
			return _commandSize;
		}

		internal bool IsDefaultItem()
		{
			if (_defaultItem == InheritableBoolean.Default)
			{
				if (_baseCommand != null)
				{
					return _baseCommand.IsDefaultItem();
				}
				return false;
			}
			if (_defaultItem == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal int GetEffectiveDaysForInfrequency()
		{
			if (_baseCommand != null)
			{
				return _baseCommand.GetEffectiveDaysForInfrequency();
			}
			if (_daysForInfrequency == -1)
			{
				if (CommandManager != null)
				{
					return CommandManager.DaysForInfrequency;
				}
				return 0;
			}
			return _daysForInfrequency;
		}

		internal StateStyles GetEffectiveStateStyles()
		{
			if (_baseCommand != null)
			{
				StateStyles uiStateStyles = BaseCommand.StateStyles;
				if (_stateStyles != null)
				{
					uiStateStyles = uiStateStyles.CreateMergedStateStyles(StateStyles);
				}
				return uiStateStyles;
			}
			return StateStyles;
		}

		internal PopupSettings GetEffectivePopupSettings()
		{
			if (_baseCommand != null)
			{
				PopupSettings uiPopupSettings = BaseCommand.PopupSettings;
				if (_popupSettings != null)
				{
					uiPopupSettings = uiPopupSettings.MergeWith(PopupSettings, StateStyles);
				}
				return uiPopupSettings;
			}
			return PopupSettings;
		}

		internal bool ShouldShowTextInContainers()
		{
			if (_showTextInContainers == InheritableBoolean.Default)
			{
				if (_baseCommand != null)
				{
					return _baseCommand.ShouldShowTextInContainers();
				}
				return false;
			}
			if (_showTextInContainers == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal bool IsEffectivelyVisible()
		{
			if (IsHiddenByMerge())
			{
				return false;
			}
			if (_visible == InheritableBoolean.Default)
			{
				if (_baseCommand != null)
				{
					return _baseCommand.IsEffectivelyVisible();
				}
			}
			else if (_visible == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}
	}
}
