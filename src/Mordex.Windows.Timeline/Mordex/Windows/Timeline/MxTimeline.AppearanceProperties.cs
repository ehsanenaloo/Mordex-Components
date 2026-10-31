using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimeline
	{
		TextBox ITextBoxOwner.TextBoxControl
		{
			get
			{
				if (_timelineFormatStyleControl != null)
				{
					return _timelineFormatStyleControl._textBoxControl;
				}
				return null;
			}
		}

		[Browsable(false)]
		public int IntervalWidth => _intervalWidth;

		[Description("Specifies how the width of the items will be calculated")]
		[DefaultValue(ItemSizeMode.TextBased)]
		[Category("Timeline Items")]
		public ItemSizeMode ItemSizeMode
		{
			get
			{
				return _itemSizeMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ItemSizeMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_itemSizeMode != value)
				{
					_itemSizeMode = value;
					InvalidateRows();
					InvalidateLayout();
				}
			}
		}

		[Description("Specifies the width of the row headers.")]
		[DefaultValue(120)]
		[Category("Groups and Filter")]
		public int RowHeaderWidth
		{
			get
			{
				return _rowHeaderWidth;
			}
			set
			{
				if (_rowHeaderWidth != value)
				{
					_rowHeaderWidth = value;
					InvalidateRows();
				}
			}
		}

		[Description("Specifies whether the last group of the Timeline should be displayed as a row header or as a group row.")]
		[DefaultValue(false)]
		[Category("Groups and Filter")]
		public bool ShowLastGroupAsRowHeader
		{
			get
			{
				return _showLastGroupAsRowHeader;
			}
			set
			{
				if (_showLastGroupAsRowHeader != value)
				{
					_showLastGroupAsRowHeader = value;
					InvalidateRows();
				}
			}
		}

		[Description("Specifies whether the text rendering should be compatible with previous release of Windows Forms.")]
		public bool UseCompatibleTextRendering
		{
			get
			{
				if (!_useCompatibleTextRendering.HasValue)
				{
					return TextRenderingOptions.DefaultUseCompatibleTextRendering;
				}
				return _useCompatibleTextRendering.Value;
			}
			set
			{
				if (_useCompatibleTextRendering != value)
				{
					_useCompatibleTextRendering = value;
					InvalidateRows();
					InvalidateLayout();
					if (DropDownCalendar != null)
					{
						DropDownCalendar.UpdateCalendarControlTextRendering();
					}
				}
			}
		}

		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[Description("Specifies the property of the data source to map to the Text property of the TimelineItem objects.")]
		[Category("Data")]
		[DefaultValue("")]
		public string TextMember
		{
			get
			{
				return _textBinding.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_textBinding))
				{
					return;
				}
				if (!IsUnbound())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_textBinding = bindingMemberInfo;
					_textField = _textBinding.BindingField;
					InvalidateRowsAndLayout();
				}
				else
				{
					_textBinding = bindingMemberInfo;
					_textField = _textBinding.BindingField;
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.Timeline.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Category("Timeline Items")]
		[Description("The key accessor for the image in the ImageList that will be the default image displayed in the items..")]
		public string DefaultItemsImageKey
		{
			get
			{
				return _defaultItemsImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_defaultItemsImageKey != value)
				{
					_defaultItemsImageKey = value;
					_defaultItemsImageIndex = -1;
					Invalidate();
				}
			}
		}

		[Category("Timeline Items")]
		[Description("Specifies the default image list index value of the image displayed in the items.")]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.Timeline.Design.ImageIndexConverter))]
		public int DefaultItemsImageIndex
		{
			get
			{
				return _defaultItemsImageIndex;
			}
			set
			{
				if (_defaultItemsImageIndex != value)
				{
					_defaultItemsImageIndex = value;
					_defaultItemsImageKey = "";
					Invalidate();
				}
			}
		}

		[Description("Determines the default AlphaMode used in format styles with AlphaMode empty.")]
		[Category("Appearance")]
		[DefaultValue(AlphaMode.Opaque)]
		public AlphaMode DefaultAlphaMode
		{
			get
			{
				return _defaultAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == AlphaMode.Empty)
				{
					throw new InvalidOperationException("DefaultAlphaMode property can not be empty");
				}
				if (_defaultAlphaMode != value)
				{
					_defaultAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(AlphaMode.Empty)]
		[Description("Represents the default BackColorAlphaMode used in format styles with BackColorAlphaMode empty.")]
		public AlphaMode DefaultBackColorAlphaMode
		{
			get
			{
				return _defaultBackColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _defaultBackColorAlphaMode)
				{
					_defaultBackColorAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(AlphaMode.Empty)]
		[Description("Represents the default ForeColorAlphaMode used in format styles with ForeColorAlphaMode empty.")]
		public AlphaMode DefaultForeColorAlphaMode
		{
			get
			{
				return _defaultForeColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _defaultForeColorAlphaMode)
				{
					_defaultForeColorAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Description("Determines the default alpha level used in format styles with Alpha property empty.")]
		[Category("Appearance")]
		[DefaultValue(255)]
		public int DefaultAlpha
		{
			get
			{
				return _defaultAlpha;
			}
			set
			{
				if (value < 0 || value > 255)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (_defaultAlpha != value)
				{
					_defaultAlpha = value;
					Invalidate();
				}
			}
		}

		[Description("The default percentage used in format styles with Blend empty. Valid values are from 0 to 1")]
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
				if (_defaultBlend != value)
				{
					if (value < 0f || value > 1f)
					{
						throw new ArgumentOutOfRangeException("DefaultBlend", "The value must be between 0 and 1");
					}
					_defaultBlend = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Category("Appearance")]
		[Description("The color that blends with the BackColor of format styles with Blend greater than 0")]
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

		[Description("Represents the font used in the tooltip of the items.")]
		public Font TooltipFont
		{
			get
			{
				if (_tooltipFont == null)
				{
					return Font;
				}
				return _tooltipFont;
			}
			set
			{
				_tooltipFont = value;
			}
		}

		[Description("Represents the height of the item's duration bar")]
		[DefaultValue(7)]
		public int ItemsDurationBarHeight
		{
			get
			{
				return _itemsDurationBarHeight;
			}
			set
			{
				if (_itemsDurationBarHeight != value)
				{
					_itemsDurationBarHeight = value;
					UpdateRowHeights();
					InvalidateRows();
				}
			}
		}

		[Description("Specifies the height of the rows in the control ")]
		[Category("Groups and Filter")]
		public int RowHeight
		{
			get
			{
				if (_timelineRowHeight > 0)
				{
					return _timelineRowHeight;
				}
				return _rowHeight;
			}
			set
			{
				if (_timelineRowHeight != value)
				{
					_timelineRowHeight = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Specifies if a bar indicating the duration should be displayed on top of the items.")]
		[DefaultValue(true)]
		[Category("Timeline Items")]
		public bool ShowItemsDurationBar
		{
			get
			{
				return _showItemsDurationBar;
			}
			set
			{
				if (_showItemsDurationBar != value)
				{
					_showItemsDurationBar = value;
					UpdateRowHeights();
					InvalidateLayout();
					InvalidateRows();
				}
			}
		}

		[Category("Timeline Items")]
		[Description("Specifies if a bar indicating the estimated duration should be displayed on top of the items.")]
		[DefaultValue(false)]
		public bool ShowItemsEstimatedDurationBar
		{
			get
			{
				return _showItemsEstimatedDurationBar;
			}
			set
			{
				if (_showItemsEstimatedDurationBar != value)
				{
					_showItemsEstimatedDurationBar = value;
					UpdateRowHeights();
					InvalidateRows();
				}
			}
		}

		[Description("Specifies if the text of the items should be displayed.")]
		[Category("Timeline Items")]
		[DefaultValue(true)]
		public bool ShowLabel
		{
			get
			{
				return _showLabel;
			}
			set
			{
				if (_showLabel != value)
				{
					_showLabel = value;
					InvalidateRows();
				}
			}
		}

		[Description("Indicates whether week numbers should be displayed in the upper header.")]
		[DefaultValue(false)]
		[Category("Timescale")]
		public bool ShowWeekNumbers
		{
			get
			{
				return _showWeekNumbers;
			}
			set
			{
				if (_showWeekNumbers != value)
				{
					_showWeekNumbers = value;
					_bottomTier.SetLayoutDirty(true);
					InvalidateTimescale();
				}
			}
		}

		[Category("Timescale")]
		[Description("The text used before the week number in the upper header when the ShowWeekNumbers property is set to true.")]
		[Localizable(true)]
		[DefaultValue("Week")]
		public string WeekText
		{
			get
			{
				return _weekText;
			}
			set
			{
				if (_weekText != value)
				{
					_weekText = value;
					if (_showWeekNumbers)
					{
						_bottomTier.SetLayoutDirty(true);
						InvalidateTimescale();
					}
				}
			}
		}

		[Description("Indicates the minimum allowed width for the items")]
		[Category("Timeline Items")]
		[DefaultValue(20)]
		public int MinItemWidth
		{
			get
			{
				return _minItemWidth;
			}
			set
			{
				if (_minItemWidth != value)
				{
					if (value < 2)
					{
						value = 2;
					}
					_minItemWidth = value;
					InvalidateRows();
				}
			}
		}

		[Browsable(false)]
		public int DesiredHeight
		{
			get
			{
				if (_isLayoutDirty || _areRowsDirty)
				{
					Update();
				}
				int height = GetHeaderHeight();
				bool isGroupExpanded = true;
				int lastGroupIndex = 0;
				foreach (MxRowLayoutState item in _allRowLayouts)
				{
					if (item.GetRowType() == RowType.Group)
					{
						if (item.GetNode().Group != null && (isGroupExpanded || item.GetNode().Group.Index <= lastGroupIndex))
						{
							if ((item.GetNode().Group != null && item.GetNode().Group.Index != Groups.Count - 1) || !ShowLastGroupAsRowHeader)
							{
								isGroupExpanded = item.IsExpanded();
								height += GetGroupRowHeight();
							}
							else
							{
								height += RowHeight;
								isGroupExpanded = item.IsExpanded();
							}
							lastGroupIndex = item.GetNode().Group.Index;
						}
					}
					else if (isGroupExpanded && item.GetRowType() == RowType.Record)
					{
						height += RowHeight;
					}
				}
				return height;
			}
		}

		[Description("Indicates the hour time format for the Timeline")]
		[Localizable(true)]
		[DefaultValue(TimeFormat.TwelveHours)]
		[Category("Timescale")]
		public TimeFormat TimeFormat
		{
			get
			{
				return _timeFormat;
			}
			set
			{
				if (_timeFormat != value)
				{
					_timeFormat = value;
					Invalidate();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(OwnerDrawnArea.None)]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		[Description("Specifies which area of a Timeline control needs to be drawn manually.")]
		[TypeConverter(typeof(FlagsEnumConverter))]
		public OwnerDrawnArea OwnerDrawnArea
		{
			get
			{
				return _ownerDrawnArea;
			}
			set
			{
				if (_ownerDrawnArea != value)
				{
					_ownerDrawnArea = value;
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public int TimeScalesHeight => GetHeaderHeight();

		[Description("The background color for the Timeline control.")]
		public new Color BackColor
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

		public override Cursor Cursor
		{
			get
			{
				if (_isDraggingItems && _itemDragType != ItemDragType.Move)
				{
					return Cursors.SizeWE;
				}
				if (_allowItemDrag != AllowItemDrag.None)
				{
					Point pt = PointToClient(Cursor.Position);
					if (base.ClientRectangle.Contains(pt))
					{
						TimelineItem itemAt = GetItemAt(pt.X, pt.Y);
						if (itemAt != null)
						{
							Rectangle rectangle = itemAt._durationBarBounds;
							if (rectangle.Width < 10)
							{
								rectangle.Width = 10;
							}
							if (rectangle.Contains(pt.X, pt.Y) & ((_allowItemDrag & AllowItemDrag.Resize) != 0))
							{
								if (pt.X >= rectangle.Right - 5)
								{
									return Cursors.SizeWE;
								}
								if (pt.X <= rectangle.X + 5)
								{
									return Cursors.SizeWE;
								}
							}
							Rectangle field_ = itemAt._estimatedBarBounds;
							if (field_.Width < 10)
							{
								field_.Width = 10;
							}
							if (field_.Contains(pt.X, pt.Y) & ((_allowItemDrag & AllowItemDrag.EstimatedDurationChange) != 0))
							{
								if (pt.X >= field_.Right - 5)
								{
									return Cursors.PanEast;
								}
								if (pt.X <= field_.X + 5)
								{
									return Cursors.PanWest;
								}
							}
							if ((_allowItemDrag & AllowItemDrag.Move) != AllowItemDrag.None || (_allowItemDrag & AllowItemDrag.GroupChange) != AllowItemDrag.None)
							{
								return Cursors.SizeAll;
							}
						}
					}
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		[Description("Specifies how the background image of the Timeline is drawn")]
		[DefaultValue(BackgroundImageDrawMode.Stretch)]
		[Category("Appearance")]
		public BackgroundImageDrawMode BackgroundImageDrawMode
		{
			get
			{
				return _backgroundImageDrawMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BackgroundImageDrawMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == BackgroundImageDrawMode.Empty)
				{
					throw new InvalidOperationException("BackgroundImageDrawMode property can not be empty");
				}
				if (value != _backgroundImageDrawMode)
				{
					_backgroundImageDrawMode = value;
					Invalidate();
				}
			}
		}

		[Description("The TimelineFormatStyle object used to draw the selected items in the Timeline.")]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public TimelineFormatStyle SelectedFormatStyle => _selectedFormatStyle;

		[Description("Indicates the way the selected item in the control is highlighted when the control loses focus")]
		[Category("Appearance")]
		[DefaultValue(HideSelection.HighlightInactive)]
		public HideSelection HideSelection
		{
			get
			{
				return _hideSelection;
			}
			set
			{
				if (_hideSelection != value)
				{
					_hideSelection = value;
					Invalidate();
				}
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		[Description("The TimelineFormatStyle object used to draw the selected items in the Timeline when the control does not have the focus.")]
		public TimelineFormatStyle SelectedInactiveFormatStyle => _selectedInactiveFormatStyle;

		[Category("Timeline Items")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TimelineFormatStyle object used to draw the items objects in the Timeline control.")]
		public TimelineFormatStyle ItemsFormatStyle => _itemsFormatStyle;

		[Description("The TimelineFormatStyle object used to draw the background of the Timeline control.")]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public TimelineFormatStyle BackgroundFormatStyle => _backgroundFormatStyle;

		[Category("Working Time")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TimelineFormatStyle object used to draw the background of the working time in the Timeline control.")]
		public TimelineFormatStyle WorkingTimeFormatStyle => _workingTimeFormatStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The TimelineFormatStyle object used to draw the bottom tier scale of the Timeline control.")]
		public TimelineFormatStyle LowerScaleTierFormatStyle => _lowerScaleTierFormatStyle;

		[Description("The TimelineFormatStyle object used to draw the top and middle tiers tier scale of the Timeline control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		public TimelineFormatStyle UpperScaleTiersFormatStyle => _upperScaleTiersFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Timeline Items")]
		[Description("The TimelineFormatStyle object that will be applied to the duration bar of the items.")]
		public TimelineFormatStyle ItemsBarFormatStyle => _itemsBarFormatStyle;

		[Category("Timeline Items")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The TimelineFormatStyle object that will be applied to the estmiated duration bar of the items.")]
		public TimelineFormatStyle ItemsEstimatedBarFormatStyle => _itemsEstimatedBarFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Groups and Filter")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TimelineFormatStyle object used to draw the row headers.")]
		public TimelineFormatStyle RowHeadersFormatStyle => _rowHeadersFormatStyle;

		[Category("Groups and Filter")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Format setting used to draw group header rows.")]
		public TimelineFormatStyle GroupRowsFormatStyle => _groupRowsFormatStyle;

		[Category("Timeline Items")]
		[DefaultValue(ItemsLineStyle.None)]
		[Description("Specifies the border style for the TimelineItem objects.")]
		public ItemsLineStyle ItemsLineStyle
		{
			get
			{
				return _itemsLineStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ItemsLineStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_itemsLineStyle != value)
				{
					_itemsLineStyle = value;
					Invalidate();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TimelineFormatConditionCollection FormatConditions => _formatConditions;

		[Obsolete("ImageSource property will be removed in future versions of the control. Use ImageList property to assign the ImageList component with the images used in the control")]
		[Category("Appearance")]
		[DefaultValue(ImageSource.ExternalImageList)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("Indicates whether an external or the internal image list should be used.")]
		public ImageSource ImageSource
		{
			get
			{
				return _imageSource;
			}
			set
			{
				if (_imageSource != value)
				{
					_imageSource = value;
					UpdateRowHeights();
					InvalidateRows();
				}
			}
		}

		[Category("Appearance")]
		[Description("Represents the ImageList used when the ImageSource property is equal to InternalImageList.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("InternalImageList property will be removed in future versions of the control. Use ImageList property to assign the ImageList component with the images used in the control")]
		public ImageList InternalImageList
		{
			get
			{
				if (_internalImageList == null)
				{
					_internalImageList = new ImageList();
				}
				return _internalImageList;
			}
		}

		[Category("Appearance")]
		[DefaultValue(null)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("ExternalImageList property will be removed in future versions. Use ImageList property instead")]
		[Description("The ImageList that contains the images used in the control.")]
		public ImageList ExternalImageList
		{
			get
			{
				return _externalImageList;
			}
			set
			{
				if (_externalImageList != value)
				{
					_externalImageList = value;
					UpdateRowHeights();
					InvalidateRows();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(null)]
		[Description("The ImageList that contains the images used in the control.")]
		public ImageList ImageList
		{
			get
			{
				return _externalImageList;
			}
			set
			{
				if (_externalImageList != value)
				{
					_externalImageList = value;
					if (value != null)
					{
						_imageSource = ImageSource.ExternalImageList;
					}
					UpdateRowHeights();
					InvalidateRows();
				}
			}
		}

		[DefaultValue(GridLines.None)]
		[Category("Appearance")]
		[Description("Indicates whether lines lines between intervals and rows are drawn.")]
		public GridLines GridLines
		{
			get
			{
				return _gridLines;
			}
			set
			{
				if (_gridLines != value)
				{
					_gridLines = value;
					Invalidate();
				}
			}
		}

		[Description("Determines how grid lines are drawn.")]
		[Category("Appearance")]
		[DefaultValue(GridLineStyle.SmallDots)]
		public GridLineStyle GridLineStyle
		{
			get
			{
				return _gridLineStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GridLineStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_gridLineStyle != value)
				{
					_gridLineStyle = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(ExpandBoxStyle.UseDefault)]
		[Description("Specifies how the control draws expand box in group rows.")]
		public ExpandBoxStyle GroupExpandBoxStyle
		{
			get
			{
				return _groupExpandBoxStyle;
			}
			set
			{
				if (_groupExpandBoxStyle != value)
				{
					_groupExpandBoxStyle = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(null)]
		public Image ExpandGlyph
		{
			get
			{
				return _expandGlyph;
			}
			set
			{
				if (_expandGlyph != value)
				{
					_expandGlyph = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(null)]
		public Image CollapseGlyph
		{
			get
			{
				return _collapseGlyph;
			}
			set
			{
				if (_collapseGlyph != value)
				{
					_collapseGlyph = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[Description("The color used to draw grid lines.")]
		public Color GridLineColor
		{
			get
			{
				if (_gridLineColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.GridLine);
				}
				return _gridLineColor;
			}
			set
			{
				if (value.A != byte.MaxValue)
				{
					throw new ArgumentException();
				}
				if (!_gridLineColor.Equals(value))
				{
					_gridLineColor = value;
					Invalidate();
				}
			}
		}

		[DefaultValue("Year")]
		[Localizable(true)]
		[Category("Timescale")]
		[Description("The text used before the year in the upper header when the Interval property is set to Year.")]
		public string YearText
		{
			get
			{
				return _yearText;
			}
			set
			{
				if (_yearText != value)
				{
					_yearText = value;
					Invalidate();
				}
			}
		}

		[Description("Specifies the size (in percentage) of the time slot in the TimeScale. Valid values: 25-1000 (%)")]
		[Category("Timescale")]
		[DefaultValue(100)]
		public int IntervalSize
		{
			get
			{
				return _intervalSize;
			}
			set
			{
				if (_intervalSize != value)
				{
					if (value < 25 || value > 1000)
					{
						throw new ArgumentOutOfRangeException("The IntervalSize must be between 25 and 1000");
					}
					_intervalSize = value;
					DisposeResources();
					InvalidateRows();
					InvalidateTimescale();
					InvalidateLayout();
				}
			}
		}

		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("Indicates how to draw a watermark image in the background.")]
		public TimelineWatermarkImage WatermarkImage
		{
			get
			{
				if (_watermarkImage == null)
				{
					_watermarkImage = new TimelineWatermarkImage(this);
				}
				return _watermarkImage;
			}
		}
	}
}
