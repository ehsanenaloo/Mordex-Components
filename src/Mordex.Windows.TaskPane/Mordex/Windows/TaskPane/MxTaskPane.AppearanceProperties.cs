using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane
	{
		[Category("Appearance")]
		[Description("The TaskPaneFormatStyle object used to draw background of the TaskPane control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public TaskPaneFormatStyle BackgroundFormatStyle => _backgroundFormatStyle;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("'GroupFormatStyle' is obsolete: This property has been deprecated. Use the GroupsStateStyle.FormatStyle property instead.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TaskPaneFormatStyle object used to draw the groups' header in the TaskPane control.")]
		[Browsable(false)]
		public TaskPaneFormatStyle GroupsFormatStyle => _groupsFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("'SpecialGroupsFormatStyle' is obsolete: This property has been deprecated. Use the SpecialGroupsStateStyles.FormatStyle property instead.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("Returns the TaskPaneFormatStyle object used to draw the header of the TaskPaneGroup marked as special in the TaskPane control.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TaskPaneFormatStyle SpecialGroupsFormatStyle => _specialGroupsFormatStyle;

		[Description("Returns the TaskPaneFormatStyle object used to draw the items in the TaskPane control.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("'ItemsFormatStyle' is obsolete: This property has been deprecated. Use the ItemsStateStyles.FormatStyle property instead.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TaskPaneFormatStyle ItemsFormatStyle => _itemsFormatStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Browsable(false)]
		[Obsolete("'SelectedItemFormatStyle' is obsolete: This property has been deprecated. Use the ItemsStateStyles.SelectedFormatStyle property instead.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Returns the TaskPaneFormatStyle object used to draw the selected item in the TaskPane control.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TaskPaneFormatStyle SelectedItemFormatStyle => _selectedItemFormatStyle;

		[Description("Returns the TaskPaneFormatStyle object used to draw the items in the TaskPane control when the mouse is over them.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Obsolete("'HoverItemFormatStyle' is obsolete: This property has been deprecated. Use the ItemsStateStyles.HotFormatStyle property instead.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TaskPaneFormatStyle HoverItemFormatStyle => _hoverItemFormatStyle;

		[Description("Returns the TaskPaneFormatStyle object used to draw the groups' header in the TaskPane control when the mouse is over them.")]
		[Obsolete("'HoverGroupFormatStyle' is obsolete: This property has been deprecated. Use the GroupsStateStyles.HotFormatStyle property instead.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TaskPaneFormatStyle HoverGroupFormatStyle => _hoverGroupFormatStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("Returns the TaskPaneFormatStyle object used to draw the header of TaskPaneGroup marked as special when the mouse is over them.")]
		[Obsolete("'HoverSpecialGroupFormatStyle' is obsolete: This property has been deprecated. Use the SpecialGroupsStateStyles.HotFormatStyle property instead.")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TaskPaneFormatStyle HoverSpecialGroupFormatStyle => _hoverSpecialGroupFormatStyle;

		[Category("Appearance")]
		[Description("Represents the object that holds the format styles used to draw the items in their different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TaskPaneStateStyles ItemsStateStyles => _itemsStateStyles;

		[Description("Represents the object that holds the format styles used to draw the groups' header in their different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		public TaskPaneStateStyles GroupsStateStyles => _groupsStateStyles;

		[Description("Represents the object that holds the format styles used to draw the special groups' header in their different states.")]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TaskPaneStateStyles SpecialGroupsStateStyles => _specialGroupsStateStyles;

		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the style used to draw the vertical scroll bar.")]
		public ControlStyle ScrollBarStyle => _verticalScrollbar.GetScrollBarStyle();

		[DefaultValue(BorderStyle.SunkenLight3D)]
		[Category("Appearance")]
		[Description("Represents the TaskPane's border style.")]
		public BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (_borderStyle != value)
				{
					_borderStyle = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Represents the color that blends with the BackColor and BackColorGradient properties of the TaskPaneFormatStyle when its Blend or BlendGradient properties are not zero.")]
		[Category("Appearance")]
		public Color BlendColor
		{
			get
			{
				if (_blendColor.IsEmpty)
				{
					return SystemColors.Window;
				}
				return _blendColor;
			}
			set
			{
				if (!_blendColor.Equals(value))
				{
					_blendColor = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Category("Appearance")]
		[Description("The color of the control's border when its style is flat.")]
		public Color FlatBorderColor
		{
			get
			{
				if (_flatBorderColor.IsEmpty)
				{
					return GetTheme().Color(ThemeRole.Border);
				}
				return _flatBorderColor;
			}
			set
			{
				if (!_flatBorderColor.Equals(value))
				{
					_flatBorderColor = value;
					ScrollBarStyle.UpdateFrameColor(FlatBorderColor);
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Category("Appearance")]
		[Description("Indicates the default percentage of the BlendColor to use with the TaskPaneFormatStyle objects associated with this TaskPane.")]
		[DefaultValue(0f)]
		public float DefaultBlend
		{
			get
			{
				return _defaultBlend;
			}
			set
			{
				if ((value < 0f || value > 1f) && value != -1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value == -1f)
				{
					value = 0f;
				}
				if (value != _defaultBlend)
				{
					_defaultBlend = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(ImageDrawMode.Stretched)]
		[Description("Specifies how the background image of the TaskPane is drawn.")]
		public ImageDrawMode BackgroundImageDrawMode
		{
			get
			{
				return _backgroundImageDrawMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ImageDrawMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backgroundImageDrawMode)
				{
					_backgroundImageDrawMode = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Description("Indicates how to draw a watermark image in the background.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		public TaskPaneWatermarkImage WatermarkImage
		{
			get
			{
				if (_watermarkImage == null)
				{
					_watermarkImage = new TaskPaneWatermarkImage(this);
				}
				return _watermarkImage;
			}
		}

		[Description("Determines the size that images will have when drawing a group or an item.")]
		[Category("Appearance")]
		[Localizable(true)]
		public Size ImageSize
		{
			get
			{
				return _imageSize;
			}
			set
			{
				if (_imageSize != value)
				{
					_imageSize = value;
					InvalidateLayout();
				}
			}
		}

		[Description("The ImageList that contains the Image objects used by the TaskPaneGroup and TaskPaneItem objects.")]
		[DefaultValue(null)]
		[Category("Appearance")]
		public ImageList ImageList
		{
			get
			{
				return _imageList;
			}
			set
			{
				if (_imageList != value)
				{
					_imageList = value;
				}
			}
		}

		[DefaultValue(10)]
		[Description("The space, in pixels, between columns of groups.")]
		[Category("Behavior")]
		public int ColumnSeparation
		{
			get
			{
				return _columnSeparation;
			}
			set
			{
				if (_columnSeparation != value)
				{
					_columnSeparation = value;
					if (_columns > 1)
					{
						InvalidateLayout();
					}
				}
			}
		}

		[DefaultValue(-1)]
		[Description("The vertical space, in pixels, between groups.")]
		[Category("Behavior")]
		public int GroupSeparation
		{
			get
			{
				return _groupSeparation;
			}
			set
			{
				if (_groupSeparation != value)
				{
					_groupSeparation = value;
					InvalidateLayout();
				}
			}
		}

		[Category("Layout")]
		[Description("The left margin for the TaskPane control.")]
		[DefaultValue(-1)]
		public int LeftMargin
		{
			get
			{
				return _leftMargin;
			}
			set
			{
				if (_leftMargin != value)
				{
					_leftMargin = value;
					InvalidateLayout();
				}
			}
		}

		[Category("Layout")]
		[Description("The left right for the TaskPane control.")]
		[DefaultValue(-1)]
		public int RightMargin
		{
			get
			{
				return _rightMargin;
			}
			set
			{
				if (_rightMargin != value)
				{
					_rightMargin = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[Category("Layout")]
		[Description("The top margin for the TaskPane control.")]
		public int TopMargin
		{
			get
			{
				return _topMargin;
			}
			set
			{
				if (_topMargin != value)
				{
					_topMargin = value;
					InvalidateLayout();
				}
			}
		}

		[Description("The background color for the control.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[DefaultValue(true)]
		[Category("Appearance")]
		[Description("Indicates whether a border is drawn around a group.")]
		public bool ShowGroupBorder
		{
			get
			{
				return _showGroupBorder;
			}
			set
			{
				if (_showGroupBorder != value)
				{
					_showGroupBorder = value;
					if (_hasContainerGroups)
					{
						InvalidateLayout();
					}
					else
					{
						Invalidate(invalidateChildren: true);
					}
				}
			}
		}

		[DefaultValue(Effect.Fade)]
		[Description("Indicates which effect to use when groups are expanded/collapsed.")]
		[Category("Behavior")]
		public Effect Effect
		{
			get
			{
				return _effect;
			}
			set
			{
				_effect = value;
			}
		}

		[Category("Appearance")]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Specifies the default image list index value of the image displayed in the items.")]
		public int DefaultItemImageIndex
		{
			get
			{
				return _defaultItemImageIndex;
			}
			set
			{
				if (_defaultItemImageIndex != value)
				{
					_defaultItemImageIndex = value;
					_defaultGroupImageKey = "";
					InvalidateLayout();
				}
			}
		}

		[Description("The key accessor for the default image in the ImageList displayed in the items.")]
		[Category("Appearance")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		public string DefaultItemImageKey
		{
			get
			{
				return _defaultItemImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_defaultItemImageKey != value)
				{
					_defaultItemImageKey = value;
					_defaultItemImageIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[DefaultValue(-1)]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Specifies the default image list index value of the image displayed in the groups.")]
		[Category("Appearance")]
		public int DefaultGroupImageIndex
		{
			get
			{
				return _defaultGroupImageIndex;
			}
			set
			{
				if (_defaultGroupImageIndex != value)
				{
					_defaultGroupImageIndex = value;
					_defaultGroupImageKey = "";
					InvalidateLayout();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Description("The key accessor for the default image in the ImageList displayed in the groups.")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Category("Appearance")]
		public string DefaultGroupImageKey
		{
			get
			{
				return _defaultGroupImageKey;
			}
			set
			{
				if (value == null)
				{
					_defaultGroupImageKey = "";
				}
				if (_defaultGroupImageKey != value)
				{
					_defaultGroupImageKey = value;
					_defaultGroupImageIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[Description("Gets or sets a value specifying the level of alpha (transparency) used to draw the objects in the TaskPane control.")]
		[Category("Appearance")]
		public int Alpha
		{
			get
			{
				return _alpha;
			}
			set
			{
				if (value < -1 || value > 255)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _alpha)
				{
					_alpha = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(AlphaMode.Opaque)]
		[Description("Specifies the level of alpha to use in the TaskPane control.")]
		public AlphaMode AlphaMode
		{
			get
			{
				return _alphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == AlphaMode.Empty)
				{
					throw new ArgumentException();
				}
				if (_alphaMode != value)
				{
					_alphaMode = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Description("Specifies the level of alpha (transparency) to use in the background.")]
		[Category("Appearance")]
		[DefaultValue(AlphaMode.Empty)]
		public AlphaMode BackColorAlphaMode
		{
			get
			{
				return _backColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != BackColorAlphaMode)
				{
					_backColorAlphaMode = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Description("Specifyies the level of alpha (transparency) to use in the foreground.")]
		[DefaultValue(AlphaMode.Empty)]
		[Category("Appearance")]
		public AlphaMode ForeColorAlphaMode
		{
			get
			{
				return _foreColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_foreColorAlphaMode != value)
				{
					_foreColorAlphaMode = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackgroundFormatStyle()
		{
			return _backgroundFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackgroundFormatStyle()
		{
			if (_backgroundFormatStyle.ShouldSerialize())
			{
				_backgroundFormatStyle.Reset();
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGroupsFormatStyle()
		{
			return _groupsFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGroupsFormatStyle()
		{
			if (_groupsFormatStyle.ShouldSerialize())
			{
				_groupsFormatStyle.Reset();
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSpecialGroupsFormatStyle()
		{
			return _specialGroupsFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSpecialGroupsFormatStyle()
		{
			if (_specialGroupsFormatStyle.ShouldSerialize())
			{
				_specialGroupsFormatStyle.Reset();
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsFormatStyle()
		{
			return _itemsFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsFormatStyle()
		{
			if (_itemsFormatStyle.ShouldSerialize())
			{
				_itemsFormatStyle.Reset();
				_itemsFormatStyle.Blend = 0.55f;
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedtemFormatStyle()
		{
			return _selectedItemFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedItemFormatStyle()
		{
			_selectedItemFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverItemFormatStyle()
		{
			return _hoverItemFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverItemFormatStyle()
		{
			_hoverItemFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverGroupFormatStyle()
		{
			_hoverGroupFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverGroupFormatStyle()
		{
			return _hoverGroupFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverSpecialGroupFormatStyle()
		{
			_hoverSpecialGroupFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverSpecialGroupFormatStyle()
		{
			return _hoverSpecialGroupFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBlendColor()
		{
			return !_blendColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBlendColor()
		{
			BlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !_flatBorderColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWatermarkImage()
		{
			if (_watermarkImage == null)
			{
				return false;
			}
			return _watermarkImage.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWatermarkImage()
		{
			WatermarkImage.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			return !_imageSize.Equals(Size.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.Equals(SystemColors.Window);
		}

		public override void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

	}
}
