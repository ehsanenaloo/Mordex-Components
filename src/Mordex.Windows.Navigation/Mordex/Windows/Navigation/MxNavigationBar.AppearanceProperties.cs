using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Navigation.Internal;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public partial class MxNavigationBar
	{
		[Description("Specifies how the drop target will be represented in a drag operation.")]
		[DefaultValue(DragStyle.DropLine)]
		[Category("Behavior")]
		public DragStyle DragStyle
		{
			get
			{
				return _dragStyle;
			}
			set
			{
				if (_dragStyle != value)
				{
					_dragStyle = value;
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(null)]
		[Description("The ImageList that contains the Image objects used by the NavigationGroup objects and NavigationItem objects with small icons.")]
		public ImageList SmallImageList
		{
			get
			{
				return _smallImageList;
			}
			set
			{
				if (_smallImageList != value)
				{
					_smallImageList = value;
					InvalidateLayout(false);
				}
			}
		}

		[Category("Appearance")]
		[Description("The ImageList that contains the Image objects to use when displaying NavigationItems as large icons in the NavigationBar.")]
		[DefaultValue(null)]
		public ImageList LargeImageList
		{
			get
			{
				return _largeImageList;
			}
			set
			{
				if (_largeImageList != value)
				{
					_largeImageList = value;
					InvalidateLayout(false);
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[Editor(typeof(SmallImageIndexEditor), typeof(UITypeEditor))]
		[Description("Specifies the default index value of the image displayed in the groups.")]
		[Category("Appearance")]
		[DefaultValue(-1)]
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
					InvalidateLayout(false);
				}
			}
		}

		[DefaultValue("")]
		[Editor(typeof(SmallImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[Description("Specifies the default key accessor for the image in the ImageList  displayed in the groups.")]
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
					value = "";
				}
				if (_defaultGroupImageKey != value)
				{
					_defaultGroupImageKey = value;
					_defaultGroupImageIndex = -1;
					InvalidateLayout(false);
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[Description("Specifies the default image list index value of the image displayed by the items when thy are in small icons view.")]
		[Category("Appearance")]
		[Editor(typeof(SmallImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		public int DefaultItemSmallImageIndex
		{
			get
			{
				return _defaultItemSmallImageIndex;
			}
			set
			{
				if (_defaultItemSmallImageIndex != value)
				{
					_defaultItemSmallImageIndex = value;
					_defaultItemSmallImageKey = "";
					InvalidateLayout(false);
				}
			}
		}

		[Editor(typeof(SmallImageKeyEditor), typeof(UITypeEditor))]
		[Description("Specifies the default key accessor for the image in the ImageList displayed by the items when thy are in small icons view.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Category("Appearance")]
		public string DefaultItemSmallImageKey
		{
			get
			{
				return _defaultItemSmallImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_defaultItemSmallImageKey != value)
				{
					_defaultItemSmallImageKey = value;
					_defaultItemSmallImageIndex = -1;
					InvalidateLayout(false);
				}
			}
		}

		[Category("Appearance")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[DefaultValue(-1)]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		[Description("Specifies the default image list index value of the image displayed by the items when thy are in large icons view.")]
		public int DefaultItemLargeImageIndex
		{
			get
			{
				return _defaultItemLargeImageIndex;
			}
			set
			{
				if (_defaultItemLargeImageIndex != value)
				{
					_defaultItemLargeImageIndex = value;
					_defaultItemLargeImageKey = "";
					InvalidateLayout(false);
				}
			}
		}

		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Category("Appearance")]
		[Description("Specifies the default key accessor for the image in the ImageList displayed by the items when thy are in large icons view.")]
		public string DefaultItemLargeImageKey
		{
			get
			{
				return _defaultItemLargeImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_defaultItemLargeImageKey != value)
				{
					_defaultItemLargeImageKey = value;
					_defaultItemLargeImageIndex = -1;
					InvalidateLayout(false);
				}
			}
		}

		[Category("Appearance")]
		[Description("Specifies the color of the line indicating the drop target.")]
		public Color DropLineColor
		{
			get
			{
				return _dropLineColor;
			}
			set
			{
				if (_dropLineColor != value)
				{
					_dropLineColor = value;
				}
			}
		}

		[Description("The color of the border of the control and headers when its border style is flat.")]
		[Category("Appearance")]
		public Color FlatBorderColor
		{
			get
			{
				if (_flatBorderColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.Border);
				}
				return _flatBorderColor;
			}
			set
			{
				if (_flatBorderColor != value)
				{
					_flatBorderColor = value;
					_mordexColorSchemeRenderer.SetFlatBorderColor(value);
					if (_scrollNextButton != null)
					{
						_scrollNextButton._mordexColorSchemeRenderer.SetFlatBorderColor(FlatBorderColor);
						_scrollNextButton.Invalidate();
					}
					if (_scrollPreviousButton != null)
					{
						_scrollPreviousButton._mordexColorSchemeRenderer.SetFlatBorderColor(FlatBorderColor);
						_scrollPreviousButton.Invalidate();
					}
					Invalidate(base.ClientRectangle);
				}
			}
		}

		[Description("Determines the appearance of the groups' headers in the NavigationBar.")]
		[Category("Appearance")]
		[DefaultValue(GroupAppearance.Light3D)]
		public GroupAppearance GroupAppearance
		{
			get
			{
				return _groupAppearance;
			}
			set
			{
				if (_groupAppearance != value)
				{
					_groupAppearance = value;
					if (_scrollNextButton != null)
					{
						_scrollNextButton.UpdateHeaderAppearance(value);
					}
					if (_scrollPreviousButton != null)
					{
						_scrollPreviousButton.UpdateHeaderAppearance(value);
					}
					InvalidateLayout(false);
				}
			}
		}

		[DefaultValue(BorderStyle.Light3D)]
		[Category("Appearance")]
		[Description("The NavigationBar's border style.")]
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
					if (_scrollNextButton != null)
					{
						_scrollNextButton.UpdateBorder(value);
					}
					if (_scrollPreviousButton != null)
					{
						_scrollPreviousButton.UpdateBorder(value);
					}
					InvalidateLayout(false);
				}
			}
		}

		[Description("Indicates the default percentage of the BlendColor to use with the NavigationBarFormatStyle objects associated with this NavigationBar.")]
		[Category("Appearance")]
		[DefaultValue(0f)]
		public float DefaultBlend
		{
			get
			{
				return _defaultBlend;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _defaultBlend)
				{
					_defaultBlend = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[Localizable(true)]
		[Description("Determines the size that images will have when drawing a group or an item in small icons view.")]
		public Size SmallImageSize
		{
			get
			{
				return _smallImageSize;
			}
			set
			{
				if (_smallImageSize != value)
				{
					_smallImageSize = value;
					InvalidateLayout(false);
				}
			}
		}

		[Category("Appearance")]
		[Description("Determines the size that images will have when drawing an item in large icons view.")]
		[Localizable(true)]
		public Size LargeImageSize
		{
			get
			{
				return _largeImageSize;
			}
			set
			{
				if (_largeImageSize != value)
				{
					_largeImageSize = value;
					InvalidateLayout(false);
				}
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether the image of the items should be painted with a shadow when the mouse hovers over them.")]
		[DefaultValue(false)]
		public bool ShadowOnHover
		{
			get
			{
				return _shadowOnHover;
			}
			set
			{
				if (value != _shadowOnHover)
				{
					_shadowOnHover = value;
				}
			}
		}

		[Category("Appearance")]
		[Description("Specifies how the background image of the NavigationBar is drawn.")]
		[DefaultValue(ImageDrawMode.Stretched)]
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
					Invalidate(_itemsAreaBounds);
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(ItemAppearance.PopUp)]
		[Description("Indicates the appearance of the NavigationItem objects in the NavigationBar control.")]
		public ItemAppearance ItemAppearance
		{
			get
			{
				return _itemAppearance;
			}
			set
			{
				if (_itemAppearance != value)
				{
					_itemAppearance = value;
					Invalidate();
				}
			}
		}

		[Description("The color that blends with the BackColor and BackColorGradient properties of the NavigationBarFormatStyle when its Blend or BlendGradient properties are not zero.")]
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
					Invalidate();
				}
			}
		}

		[Description("Represents the object that holds the format styles used to draw the groups' header in their different states.")]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarStateStyles GroupsStateStyles => _groupsStateStyles;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Browsable(false)]
		[Obsolete("'GroupFormatStyle' is obsolete: This property has been deprecated. Use the GroupsStateStyle.FormatStyle property instead.")]
		[Description("The NavigationBarFormatStyle object used to draw the groups' header in the NavigationBar control.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationBarFormatStyle GroupsFormatStyle => _groupsFormatStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The NavigationBarFormatStyle object used to draw the groups' background in the NavigationBar control.")]
		[Category("Appearance")]
		public NavigationBarFormatStyle GroupsBackgroundFormatStyle => _groupsBackgroundFormatStyle;

		[Category("Appearance")]
		[Description("Represents the object that holds the format styles used to draw the items in their different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarStateStyles ItemsStateStyles => _itemsStateStyles;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("'ItemsFormatStyle' is obsolete: This property has been deprecated. Use the ItemsStateStyles.FormatStyle property instead.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The NavigationBarFormatStyle object used to draw the items in the NavigationBar control.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationBarFormatStyle ItemsFormatStyle => _itemsFormatStyle;

		[Obsolete("'SelectedItemFormatStyle' is obsolete: This property has been deprecated. Use the ItemsStateStyles.SelectedFormatStyle property instead.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The NavigationBarFormatStyle object used to draw the selected item in the NavigationBar.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationBarFormatStyle SelectedItemFormatStyle => _selectedItemFormatStyle;

		[Obsolete("'HoverItemFormatStyle' is obsolete: This property has been deprecated. Use the ItemsStateStyles.HotFormatStyle property instead.")]
		[Description("The NavigationBarFormatStyle object used to draw the items in the NavigationBar control when the mouse is over them.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public NavigationBarFormatStyle HoverItemFormatStyle => _hoverItemFormatStyle;

		[Browsable(false)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Obsolete("'HoverGroupFormatStyle' is obsolete: This property has been deprecated. Use the GroupsStateStyles.HotFormatStyle property instead.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("The NavigationBarFormatStyle object used to draw the groups' header in the NavigationBar control when the mouse is over them.")]
		public NavigationBarFormatStyle HoverGroupFormatStyle => _hoverGroupFormatStyle;

		[Category("Appearance")]
		[DefaultValue(-1)]
		[Description("Specifies the level of alpha (transparency) used to draw the objects in the NavigationBar control.")]
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
					Invalidate();
				}
			}
		}

		[DefaultValue(AlphaMode.Opaque)]
		[Category("Appearance")]
		[Description("Specifies the level of alpha to use in the NavigationBar control.")]
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
					Invalidate();
				}
			}
		}

		[Description("Specifies the level of alpha (transparency) to use in the background.")]
		[DefaultValue(AlphaMode.Empty)]
		[Category("Appearance")]
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
				if (value != _backColorAlphaMode)
				{
					_backColorAlphaMode = value;
					Invalidate();
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
					Invalidate();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDropLineColor()
		{
			DropLineColor = Color.Red;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDropLineColor()
		{
			return !_dropLineColor.Equals(Color.Red);
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
		public bool ShouldSerializeSmallImageSize()
		{
			return !_smallImageSize.Equals(Size.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSmallImageSize()
		{
			SmallImageSize = Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeImageSize()
		{
			return !_largeImageSize.Equals(Size.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeImageSize()
		{
			LargeImageSize = Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBlendColor()
		{
			BlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBlendColor()
		{
			return !_blendColor.IsEmpty;
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
				InvalidateLayout(false);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGroupsBackgroundFormatStyle()
		{
			return _groupsBackgroundFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGroupsBackgroundFormatStyle()
		{
			if (_groupsBackgroundFormatStyle.ShouldSerialize())
			{
				_groupsBackgroundFormatStyle.Reset();
				Invalidate();
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
				InvalidateItemLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedItemFormatStyle()
		{
			return _selectedItemFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedItemFormatStyle()
		{
			if (_selectedItemFormatStyle.ShouldSerialize())
			{
				_selectedItemFormatStyle.Reset();
				Invalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverItemFormatStyle()
		{
			return _hoverItemFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverItemFormatStyle()
		{
			if (_hoverItemFormatStyle.ShouldSerialize())
			{
				_hoverItemFormatStyle.Reset();
				InvalidateItemLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverGroupFormatStyle()
		{
			return _hoverGroupFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverGroupFormatStyle()
		{
			if (_hoverGroupFormatStyle.ShouldSerialize())
			{
				_hoverGroupFormatStyle.Reset();
				Invalidate(invalidateChildren: true);
			}
		}
	}
}
