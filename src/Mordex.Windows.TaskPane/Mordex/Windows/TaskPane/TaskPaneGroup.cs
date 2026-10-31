using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	[TypeConverter(typeof(TaskPaneGroupConverter))]
	[DefaultProperty("Text")]
	public partial class TaskPaneGroup : ITaskPaneOwner, IMordexXmlLayoutsSupport
	{
		private int _columnSpan = 1;

		private AccessibleObject _accessibleObject;

		private string _accessibleName;

		private string _accessibleDescription;

		private AccessibleRole _accessibleRole = AccessibleRole.Default;

		internal TaskPaneGroupCollection _owner;

		internal bool _container;

		private int _headerHeight = -1;

		private int _containerHeight = -1;

		internal TaskPaneContainerControl _containerControl;

		private object _tag;

		private Icon _icon;

		private Rectangle _closeButtonBounds = Rectangle.Empty;

		private Rectangle _expandButtonBounds = Rectangle.Empty;

		private string _tagString = "";

		internal int _index;

		private string _key = string.Empty;

		private Alignment _textAlignment;

		private Alignment _imageAlignment;

		private bool _enabled = true;

		private bool _visible = true;

		private ItemState _itemState;

		private Rectangle _headerBounds = Rectangle.Empty;

		private ButtonAlignment _buttonAlignment;

		private ButtonAlignment _closeButtonAlignment;

		private ButtonAlignment _expandButtonAlignment;

		private Cursor _cursor;

		private bool _expandable = true;

		private int _collapsedButtonImageIndex = -1;

		private string _collapsedButtonImageKey = "";

		private int _expandButtonImageIndex = -1;

		private string _expandButtonImageKey = "";

		private bool _showCloseButton;

		private bool _isCloseButtonPressed;

		private bool _isCloseButtonHot;

		private Rectangle _itemsBounds;

		private TaskPaneItemCollection _items;

		private Image _image;

		private int _focusImageIndex = -1;

		private string _focusImageKey = "";

		private int _disabledImageIndex = -1;

		private string _disabledImageKey = "";

		private int _imageIndex = -1;

		private string _imageKey = "";

		private TriState _showGroupBorder;

		private TaskPaneWatermarkImage _watermarkImage;

		private string _text = "";

		private string _toolTipText = "";

		private bool _specialGroup;

		private Image _backgroundImage;

		private ImageDrawMode _backgroundImageDrawMode;

		private TaskPaneFormatStyle _formatStyle;

		internal bool _expanded = true;

		private bool _showGroupCaption = true;

		internal int _columnIndex = 1;

		private TaskPaneStateStyles _itemsStateStyles;

		private TaskPaneStateStyles _stateStyles;

		private TaskPaneFormatStyle _itemsFormatStyle;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_showCloseButton = (bool)LayoutReader.GetPropertyValue(info, "ShowCloseButton", typeof(bool), false);
			_backgroundImage = (Image)LayoutReader.GetPropertyValue(info, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (ImageDrawMode)LayoutReader.GetPropertyValue(info, "BackgroundImageDrawMode", typeof(ImageDrawMode), ImageDrawMode.Stretched);
			_columnSpan = (int)LayoutReader.GetPropertyValue(info, "ColumnSpan", typeof(int), 1);
			Container = (bool)LayoutReader.GetPropertyValue(info, "Container", typeof(bool), false);
			_containerHeight = (int)LayoutReader.GetPropertyValue(info, "ControlHeight", typeof(int), -1);
			_collapsedButtonImageKey = (string)LayoutReader.GetPropertyValue(info, "CollapsedButtonImageKey", typeof(string), "");
			_collapsedButtonImageIndex = (int)LayoutReader.GetPropertyValue(info, "CollapsedButtonImageIndex", typeof(int), -1);
			_expandButtonImageKey = (string)LayoutReader.GetPropertyValue(info, "ExpandButtonImageKey", typeof(string), "");
			_expandButtonImageIndex = (int)LayoutReader.GetPropertyValue(info, "ExpandButtonImageIndex", typeof(int), -1);
			_cursor = (Cursor)LayoutReader.GetPropertyValue(info, "Cursor", typeof(Cursor), null);
			_disabledImageIndex = (int)LayoutReader.GetPropertyValue(info, "DisabledImageIndex", typeof(int), -1);
			_disabledImageKey = (string)LayoutReader.GetPropertyValue(info, "DisabledImageKey", typeof(string), "");
			_enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			_expandable = (bool)LayoutReader.GetPropertyValue(info, "Expandable", typeof(bool), true);
			_buttonAlignment = (ButtonAlignment)LayoutReader.GetPropertyValue(info, "ButtonAlignment", typeof(ButtonAlignment), ButtonAlignment.Default);
			_expandButtonAlignment = (ButtonAlignment)LayoutReader.GetPropertyValue(info, "ExpandButtonAlignment", typeof(ButtonAlignment), ButtonAlignment.Default);
			_closeButtonAlignment = (ButtonAlignment)LayoutReader.GetPropertyValue(info, "CloseButtonAlignment", typeof(ButtonAlignment), ButtonAlignment.Default);
			_expanded = (bool)LayoutReader.GetPropertyValue(info, "Expanded", typeof(bool), true);
			_itemsStateStyles.Reset();
			_stateStyles.Reset();
			info.DeserializeObject("ItemsStateStyles", _itemsStateStyles);
			info.DeserializeObject("StateStyles", _stateStyles);
			info["FormatStyle"]?.DeserializeObject(_stateStyles.FormatStyle);
			info["ItemsFormatStyle"]?.DeserializeObject(ItemsStateStyles.FormatStyle);
			_focusImageIndex = (int)LayoutReader.GetPropertyValue(info, "FocusImageIndex", typeof(int), -1);
			_focusImageKey = (string)LayoutReader.GetPropertyValue(info, "FocusImageKey", typeof(string), "");
			_headerHeight = (int)LayoutReader.GetPropertyValue(info, "HeaderHeight", typeof(int), -1);
			_icon = (Icon)LayoutReader.GetPropertyValue(info, "Icon", typeof(Icon), null);
			_image = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_imageAlignment = (Alignment)LayoutReader.GetPropertyValue(info, "ImageAlignment", typeof(Alignment), Alignment.Near);
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			PropertyValue propertyValue = info["Items"];
			_items = new TaskPaneItemCollection(this);
			propertyValue?.DeserializeObject(_items);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			_showGroupCaption = (bool)LayoutReader.GetPropertyValue(info, "ShowGroupCaption", typeof(bool), true);
			_specialGroup = (bool)LayoutReader.GetPropertyValue(info, "SpecialGroup", typeof(bool), false);
			string tagString = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			_tag = LayoutTypePolicy.ConvertOrKeepText(tagString, (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null));
			_tagString = (string)LayoutReader.GetPropertyValue(info, "TagString", typeof(string), "");
			_text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), "");
			_textAlignment = (Alignment)LayoutReader.GetPropertyValue(info, "TextAlignment", typeof(Alignment), Alignment.Near);
			_toolTipText = (string)LayoutReader.GetPropertyValue(info, "ToolTipText", typeof(string), "");
			_visible = (bool)LayoutReader.GetPropertyValue(info, "Visible", typeof(bool), true);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_showCloseButton)
			{
				info.AddValue("ShowCloseButton", _showCloseButton);
			}
			if (_backgroundImage != null)
			{
				info.AddValue("BackgroundImage", _backgroundImage);
			}
			if (_backgroundImageDrawMode != ImageDrawMode.Stretched)
			{
				info.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (_columnSpan != 1)
			{
				info.AddValue("ColumnSpan", _columnSpan);
			}
			if (_container)
			{
				info.AddValue("Container", _container);
			}
			if (_containerHeight != -1)
			{
				info.AddValue("ControlHeight", _containerHeight);
			}
			if (_cursor != null)
			{
				info.AddValue("Cursor", _cursor);
			}
			if (_disabledImageIndex != -1)
			{
				info.AddValue("DisabledImageIndex", _disabledImageIndex);
			}
			if (_disabledImageKey.Length > 0)
			{
				info.AddValue("DisabledImageKey", _disabledImageKey);
			}
			if (!_enabled)
			{
				info.AddValue("Enabled", _enabled);
			}
			if (!_expandable)
			{
				info.AddValue("Expandable", _expandable);
			}
			if (_buttonAlignment != ButtonAlignment.Default)
			{
				info.AddValue("ButtonAlignment", _buttonAlignment);
			}
			if (_expandButtonAlignment != ButtonAlignment.Default)
			{
				info.AddValue("ExpandButtonAlignment", _expandButtonAlignment);
			}
			if (_closeButtonAlignment != ButtonAlignment.Default)
			{
				info.AddValue("CloseButtonAlignment", _closeButtonAlignment);
			}
			if (_collapsedButtonImageIndex != -1)
			{
				info.AddValue("CollapsedButtonImageIndex", _collapsedButtonImageIndex);
			}
			if (_collapsedButtonImageKey.Length > 0)
			{
				info.AddValue("CollapsedButtonImageKey", _collapsedButtonImageKey);
			}
			if (_expandButtonImageIndex != -1)
			{
				info.AddValue("ExpandButtonImageIndex", _expandButtonImageIndex);
			}
			if (_expandButtonImageKey.Length > 0)
			{
				info.AddValue("ExpandButtonImageKey", _expandButtonImageKey);
			}
			if (!_expanded)
			{
				info.AddValue("Expanded", _expanded);
			}
			if (_focusImageIndex != -1)
			{
				info.AddValue("FocusImageIndex", _focusImageIndex);
			}
			if (_focusImageKey.Length > 0)
			{
				info.AddValue("FocusImageKey", _focusImageKey);
			}
			if (_headerHeight != -1)
			{
				info.AddValue("HeaderHeight", _headerHeight);
			}
			if (_icon != null)
			{
				info.AddValue("Icon", _icon);
			}
			if (_image != null)
			{
				info.AddValue("Image", _image);
			}
			if (_imageAlignment != Alignment.Near)
			{
				info.AddValue("ImageAlignment", _imageAlignment);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_items.Count > 0)
			{
				info.AddValue("Items", _items);
			}
			if (_itemsStateStyles.ShouldSerialize())
			{
				info.AddValue("ItemsStateStyles", _itemsStateStyles);
			}
			if (_stateStyles.ShouldSerialize())
			{
				info.AddValue("StateStyles", _stateStyles);
			}
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
			if (!_showGroupCaption)
			{
				info.AddValue("ShowGroupCaption", _showGroupCaption);
			}
			if (_specialGroup)
			{
				info.AddValue("SpecialGroup", _specialGroup);
			}
			if (_tag != null)
			{
				if (!(_tag is string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				info.AddValue("Tag", _tag);
			}
			if (_tagString != "")
			{
				info.AddValue("TagString", _tagString);
			}
			if (_text != "")
			{
				info.AddValue("Text", _text);
			}
			if (_textAlignment != Alignment.Near)
			{
				info.AddValue("TextAlignment", _textAlignment);
			}
			if (_toolTipText != "")
			{
				info.AddValue("ToolTipText", _toolTipText);
			}
			if (!_visible)
			{
				info.AddValue("Visible", _visible);
			}
		}

		public TaskPaneGroup(string text)
		{
			_text = text;
			_items = new TaskPaneItemCollection(this);
			_textAlignment = Alignment.Near;
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			_stateStyles = new TaskPaneStateStyles(this, FormatStyle, null, null);
#pragma warning restore CS0618
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			_itemsStateStyles = new TaskPaneStateStyles(this, ItemsFormatStyle, null, null);
#pragma warning restore CS0618
		}

		public TaskPaneGroup()
			: this("")
		{
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new TaskPaneGroupAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal AccessibleStates ComputeAccessibleStates()
		{
			AccessibleStates accessibleStates = AccessibleStates.Focusable;
			if (_expanded)
			{
				return accessibleStates | AccessibleStates.Expanded;
			}
			return accessibleStates | AccessibleStates.Collapsed;
		}

		internal AccessibleRole GetAccessibleRole()
		{
			if (AccessibleRole != AccessibleRole.Default)
			{
				return AccessibleRole;
			}
			return AccessibleRole.OutlineItem;
		}

		internal void InvalidateOwnerLayout()
		{
			if (_owner != null && _owner.GetOwnerTaskPane() != null)
			{
				_owner.InvalidateLayout();
			}
		}

		internal void InvalidateTaskPane()
		{
			if (_owner != null)
			{
				_owner.InvalidateTaskPane();
			}
		}

		internal Rectangle GetCloseButtonBounds()
		{
			return _closeButtonBounds;
		}

		internal Rectangle GetExpandButtonBounds()
		{
			return _expandButtonBounds;
		}

		internal ItemState GetItemState()
		{
			return _itemState;
		}

		internal void SetItemState(ItemState itemState)
		{
			_itemState = itemState;
		}

		internal Rectangle GetHeaderBounds()
		{
			return _headerBounds;
		}

		internal void SetHeaderBounds(Rectangle headerBounds)
		{
			_headerBounds = headerBounds;
			LayoutHeaderButtons();
		}

		internal ButtonAlignment ComputeButtonAlignment()
		{
			ButtonAlignment buttonAlignment = CloseButtonAlignment;
			if (buttonAlignment == ButtonAlignment.Default)
			{
				buttonAlignment = ButtonAlignment.Right;
			}
			if (TaskPane != null && TaskPane.RightToLeft == RightToLeft.Yes)
			{
				switch (buttonAlignment)
				{
				case ButtonAlignment.Right:
					return ButtonAlignment.Left;
				case ButtonAlignment.Left:
					return ButtonAlignment.Right;
				}
			}
			return buttonAlignment;
		}

		internal ButtonAlignment ResolveExpandButtonAlignment()
		{
			ButtonAlignment buttonAlignment = ExpandButtonAlignment;
			if (buttonAlignment == ButtonAlignment.Default)
			{
				buttonAlignment = (ButtonAlignment.Right);
			}
			if (TaskPane != null && TaskPane.RightToLeft == RightToLeft.Yes)
			{
				switch (buttonAlignment)
				{
				case ButtonAlignment.Right:
					return ButtonAlignment.Left;
				case ButtonAlignment.Left:
					return ButtonAlignment.Right;
				}
			}
			return buttonAlignment;
		}

		private void LayoutHeaderButtons()
		{
			if (_headerBounds.Height <= 0 || _headerBounds.Width <= GetHeaderButtonSize() + 8)
			{
				return;
			}
			int buttonTop = (_headerBounds.Height - GetHeaderButtonSize()) / 2;
			int closeButtonLeft = 0;
			int expandButtonLeft = 0;
			if (_showCloseButton)
			{
				closeButtonLeft = ((ComputeButtonAlignment() != ButtonAlignment.Right) ? (_headerBounds.Left + 4) : (_headerBounds.Right - GetHeaderButtonSize() - 6));
				_closeButtonBounds = new Rectangle(closeButtonLeft, buttonTop + _headerBounds.Top, GetHeaderButtonSize(), GetHeaderButtonSize());
				if (_closeButtonBounds.Right + 6 > _headerBounds.Right)
				{
					_closeButtonBounds = Rectangle.Empty;
				}
			}
			else
			{
				_closeButtonBounds = Rectangle.Empty;
			}
			if (_expandable)
			{
				expandButtonLeft = ((ResolveExpandButtonAlignment() == ButtonAlignment.Right) ? ((!_showCloseButton || ComputeButtonAlignment() != ButtonAlignment.Right) ? (_headerBounds.Right - GetHeaderButtonSize() - 6) : (closeButtonLeft - GetHeaderButtonSize() - 2)) : ((!_showCloseButton || ComputeButtonAlignment() != ButtonAlignment.Left) ? (_headerBounds.Left + 4) : (closeButtonLeft + GetHeaderButtonSize() + 2)));
				_expandButtonBounds = new Rectangle(expandButtonLeft, buttonTop + _headerBounds.Top, GetHeaderButtonSize(), GetHeaderButtonSize());
				if (_expandButtonBounds.Right + 6 > _headerBounds.Right)
				{
					_expandButtonBounds = Rectangle.Empty;
				}
			}
			else
			{
				_expandButtonBounds = Rectangle.Empty;
			}
		}

		internal bool IsCloseButtonPressed()
		{
			return _isCloseButtonPressed;
		}

		internal void SetCloseButtonPressed(bool isPressed)
		{
			if (_isCloseButtonPressed != isPressed)
			{
				_isCloseButtonPressed = isPressed;
				if (TaskPane != null)
				{
					TaskPane.Invalidate(_headerBounds);
				}
			}
		}

		internal bool IsCloseButtonHot()
		{
			return _isCloseButtonHot;
		}

		internal void SetCloseButtonHot(bool isHot)
		{
			if (_isCloseButtonHot != isHot)
			{
				_isCloseButtonHot = isHot;
				if (TaskPane != null)
				{
					TaskPane.Invalidate(_headerBounds);
				}
			}
		}

		internal TaskPaneGroup CloneDetached()
		{
			TaskPaneGroup taskPaneGroup = (TaskPaneGroup)MemberwiseClone();
			taskPaneGroup._items = new TaskPaneItemCollection(taskPaneGroup);
			taskPaneGroup._owner = null;
			taskPaneGroup._containerControl = _containerControl;
			return taskPaneGroup;
		}

		internal int GetHeaderButtonSize()
		{
			return 18;
		}

		internal Rectangle GetItemsBounds()
		{
			return _itemsBounds;
		}

		internal void SetItemsBounds(Rectangle itemsBounds)
		{
			_itemsBounds = itemsBounds;
			if (_containerControl != null)
			{
				Rectangle bounds = itemsBounds;
				if (TaskPane != null && TaskPane.ShouldShowGroupBorder(this))
				{
					bounds.X++;
					bounds.Height--;
					bounds.Width -= 2;
				}
				if (!itemsBounds.IsEmpty)
				{
					_containerControl.Bounds = bounds;
				}
			}
		}

		internal void SetExpanded(bool expanded)
		{
			_expanded = expanded;
		}
	}
}
