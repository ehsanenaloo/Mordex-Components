using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
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
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDescription()
		{
			return _description.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDescription()
		{
			Description = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlignment()
		{
			return _alignment != CommandAlignment.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlignment()
		{
			Alignment = CommandAlignment.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTextAlignment()
		{
			return _textAlignment != ContentAlignment.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTextAlignment()
		{
			TextAlignment = ContentAlignment.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageAlignment()
		{
			return _imageAlignment != ContentAlignment.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageAlignment()
		{
			ImageAlignment = ContentAlignment.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTextImageRelation()
		{
			return _textImageRelation != TextImageRelation.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTextImageRelation()
		{
			TextImageRelation = TextImageRelation.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSize()
		{
			return !_commandSize.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSize()
		{
			Size = Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAllowFloatingChildren()
		{
			return _allowFloatingChildren != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAllowFloatingChildren()
		{
			AllowFloatingChildren = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAllowCustomizeCommands()
		{
			return _allowCustomizeCommands != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAllowCustomizeCommands()
		{
			AllowCustomizeCommands = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMergeType()
		{
			return _mergeType != CommandMergeType.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMergeType()
		{
			MergeType = CommandMergeType.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMergeOrder()
		{
			return _mergeOrder != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMergeOrder()
		{
			MergeOrder = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCategoryName()
		{
			return _categoryName.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCategoryName()
		{
			_categoryName = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeChecked()
		{
			return _checkedValue != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetChecked()
		{
			Checked = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeIsEditableControl()
		{
			return _isEditableControl != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetIsEditableControl()
		{
			IsEditableControl = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControl()
		{
			if ((CommandType == CommandType.ControlContainer || CommandType == CommandType.ControlPopup) && Control != null)
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControl()
		{
			if (ShouldSerializeControl())
			{
				Control = null;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDropDownButtonStyle()
		{
			return _dropDownButtonStyle != DropDownButtonStyle.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDropDownButtonStyle()
		{
			DropDownButtonStyle = DropDownButtonStyle.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCommandStyle()
		{
			return _commandStyle != CommandStyle.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCommandStyle()
		{
			CommandStyle = CommandStyle.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCommandType()
		{
			return _commandType != CommandType.Command;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCommandType()
		{
			CommandType = CommandType.Command;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeText()
		{
			return _text.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetText()
		{
			Text = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeIcon()
		{
			return _icon != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetIcon()
		{
			Icon = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageIndex()
		{
			return _imageIndex != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageIndex()
		{
			ImageIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageKey()
		{
			return _imageKey.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageKey()
		{
			ImageKey = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeImageIndex()
		{
			return _largeImageIndex != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeImageIndex()
		{
			LargeImageIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeImageKey()
		{
			return _largeImageKey.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeImageKey()
		{
			LargeImageKey = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHotImageIndex()
		{
			return _hotImageIndex != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHotImageIndex()
		{
			HotImageIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHotImageKey()
		{
			return _hotImageKey.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHotImageKey()
		{
			HotImageKey = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeHotImageIndex()
		{
			return _largeHotImageIndex != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeHotImageIndex()
		{
			LargeHotImageIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeHotImageKey()
		{
			return _largeHotImageKey.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeHotImageKey()
		{
			LargeHotImageKey = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDisabledImageIndex()
		{
			return _disabledImageIndex != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDisabledImageIndex()
		{
			DisabledImageIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDisabledImageKey()
		{
			return _disabledImageKey.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDisabledImageKey()
		{
			DisabledImageKey = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeDisabledImageIndex()
		{
			return _largeDisabledImageIndex != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeDisabledImageIndex()
		{
			LargeDisabledImageIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeDisabledImageKey()
		{
			return _largeDisabledImageKey.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeDisabledImageKey()
		{
			LargeDisabledImageKey = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedImageIndex()
		{
			return _selectedImageIndex != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedImageIndex()
		{
			SelectedImageIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedImageKey()
		{
			return _selectedImageKey.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedImageKey()
		{
			SelectedImageKey = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeSelectedImageIndex()
		{
			return _largeSelectedImageIndex != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeSelectedImageIndex()
		{
			LargeSelectedImageIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeSelectedImageKey()
		{
			return _largeSelectedImageKey.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeSelectedImageKey()
		{
			LargeSelectedImageKey = "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageReplaceableColor()
		{
			return !_imageReplaceableColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageReplaceableColor()
		{
			ImageReplaceableColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImage()
		{
			return _commandImage != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImage()
		{
			Image = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeStateStyles()
		{
			return _stateStyles != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetStateStyles()
		{
			if (ShouldSerializeStateStyles())
			{
				_stateStyles = null;
				NotifyInvalidated();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializePopupSettings()
		{
			return _popupSettings != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetPopupSettings()
		{
			if (ShouldSerializePopupSettings())
			{
				_popupSettings = null;
				NotifyInvalidated();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCommands()
		{
			return _baseCommand == null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeShortcut()
		{
			return _isShortcutSet;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetShortcut()
		{
			Shortcut = Shortcut.None;
			_isShortcutSet = false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeVisible()
		{
			return _visible != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetVisible()
		{
			Visible = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTag()
		{
			return _tag != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTag()
		{
			Tag = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTagString()
		{
			return _tagString != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTagString()
		{
			_tagString = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCursor()
		{
			return _cursor != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCursor()
		{
			Cursor = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMdiList()
		{
			return _mdiList != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMdiList()
		{
			MdiList = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlWidth()
		{
			return _controlWidth != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlWidth()
		{
			ControlWidth = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDefaultItem()
		{
			return _defaultItem != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDefaultItem()
		{
			DefaultItem = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeShowTextInContainers()
		{
			return _showTextInContainers != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetShowTextInContainers()
		{
			ShowTextInContainers = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeEnabled()
		{
			return _enabled != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetEnabled()
		{
			Enabled = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeToolTipText()
		{
			return _toolTipText.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetToolTipText()
		{
			ToolTipText = "";
		}
	}
}
