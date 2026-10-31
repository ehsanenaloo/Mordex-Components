using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class TabbedMdiSettings : IFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		private bool _showCloseButtonOnSelectedTab;

		private Color _borderColor;


		private Size _imageSize;

		private bool _useFormIcons;

		private bool _allowDrag;

		private MdiGroupOrientation _groupOrientation;

		private MxDockManager _panelManager;

		private PanelPadding _windowPadding;

		private FormatStyle _tabStripFormatStyle;

		private StateStyles _tabStateStyles;

		private bool _showCloseButton;

		private TabScrollButtons _scrollButtons;

		private StringTrimming _tabCaptionTrimming;

		private int _maxTabSize;

		private DockingStyleMode _dockingStyleMode;

		[Description("Indicates whether the close button is shown in the selected mdi tab.")]
		[DefaultValue(false)]
		public bool ShowCloseButtonOnSelectedTab
		{
			get
			{
				return _showCloseButtonOnSelectedTab;
			}
			set
			{
				if (_showCloseButtonOnSelectedTab != value)
				{
					_showCloseButtonOnSelectedTab = value;
					NotifyManager();
				}
			}
		}

		[Description("Indicates the border color the mdi client area and Mdi Tab Groups.")]
		public Color BorderColor
		{
			get
			{
				if (_borderColor.IsEmpty)
				{
					return _panelManager.GetMdiRenderer().GetColorSchemeAppearance().GetColor(PanelColors.Border);
				}
				return _borderColor;
			}
			set
			{
				if (_borderColor != value)
				{
					_borderColor = value;
					NotifyManager();
				}
			}
		}

		[DefaultValue(DockingStyleMode.VS2005)]
		[Description("Indicates the style used to drag/drop the mdi tabs")]
		public DockingStyleMode DockingStyleMode
		{
			get
			{
				return _dockingStyleMode;
			}
			set
			{
				_dockingStyleMode = value;
			}
		}

		[Description("Determines whether tabs should draw the icon of the form in an MdiChildTab.")]
		[Localizable(true)]
		[DefaultValue(false)]
		public bool UseFormIcons
		{
			get
			{
				return _useFormIcons;
			}
			set
			{
				if (_useFormIcons != value)
				{
					_useFormIcons = value;
					NotifyManager();
				}
			}
		}

		[Localizable(true)]
		[Description("The size used to draw images and icons on an Mdi tabs")]
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
					NotifyManager();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Determines whether users can drag Mdi tabs to create new groups.")]
		[Localizable(true)]
		public bool AllowDrag
		{
			get
			{
				return _allowDrag;
			}
			set
			{
				_allowDrag = value;
			}
		}

		[Localizable(false)]
		[Description("Determines whether the tab groups are tiled horizontally or vertically within the client region of the Mdi parent form.")]
		[DefaultValue(MdiGroupOrientation.Horizontal)]
		public MdiGroupOrientation GroupOrientation
		{
			get
			{
				return _groupOrientation;
			}
			set
			{
				if (!Enum.IsDefined(typeof(MdiGroupOrientation), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_groupOrientation != value)
				{
					_groupOrientation = value;
					NotifyManager();
				}
			}
		}

		[Localizable(true)]
		[Description("Determines how text is trimmed in mdi tabs.")]
		[DefaultValue(StringTrimming.Character)]
		public StringTrimming TabCaptionTrimming
		{
			get
			{
				return _tabCaptionTrimming;
			}
			set
			{
				if (!Enum.IsDefined(typeof(StringTrimming), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_tabCaptionTrimming != value)
				{
					_tabCaptionTrimming = value;
					NotifyManager();
				}
			}
		}

		[DefaultValue(0)]
		[Description("Determines the maximum width for an mdi tab.")]
		[Localizable(true)]
		public int MaxTabSize
		{
			get
			{
				return _maxTabSize;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				if (_maxTabSize != value)
				{
					_maxTabSize = value;
					NotifyManager();
				}
			}
		}

		[Localizable(false)]
		[Description("Padding settings for all edges of an mdi child form.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public PanelPadding WindowPadding => _windowPadding;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Settings used to draw the tab strip background of mdi tab groups.")]
		public FormatStyle TabStripFormatStyle => _tabStripFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The UIStateStyles that holds the format styles used to draw the tabs of mdi tab groups in their different states.")]
		public StateStyles TabStateStyles => _tabStateStyles;

		[DefaultValue(true)]
		[Localizable(true)]
		[Description("Determines whether the close button is visible in Mdi tab groups.")]
		public bool ShowCloseButton
		{
			get
			{
				return _showCloseButton;
			}
			set
			{
				if (_showCloseButton != value)
				{
					_showCloseButton = value;
					NotifyManager();
				}
			}
		}

		[Description("Determines when scroll buttons are visible in an Mdi tab group.")]
		[DefaultValue(TabScrollButtons.Default)]
		[Localizable(false)]
		public TabScrollButtons ScrollButtons
		{
			get
			{
				return _scrollButtons;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabScrollButtons), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_scrollButtons != value)
				{
					_scrollButtons = value;
					NotifyManager();
				}
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		Control IFormatStyleOwner.Owner => null;

		internal TabbedMdiSettings(MxDockManager panelManager)
		{
			_panelManager = panelManager;
			ResetToDefaults();
		}

		private void ResetToDefaults()
		{
			_borderColor = Color.Empty;
			_useFormIcons = false;
			_tabStripFormatStyle = new FormatStyle(this);
			_tabStateStyles = new StateStyles(this);
			_showCloseButton = true;
			_scrollButtons = TabScrollButtons.Default;
			_windowPadding = new PanelPadding(_panelManager, 0, 0, 0, 0);
			_tabCaptionTrimming = StringTrimming.Character;
			_dockingStyleMode = DockingStyleMode.VS2005;
			_maxTabSize = 0;
			_groupOrientation = MdiGroupOrientation.Horizontal;
			_allowDrag = true;
			_imageSize = new Size(0, 0);
		}

		private void NotifyManager()
		{
			_panelManager.RefreshMdiTabGroups();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBorderColor()
		{
			return !_borderColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBorderColor()
		{
			BorderColor = Color.Empty;
		}

		internal TabScrollButtons ResolveTabScrollButtons()
		{
			if (_scrollButtons == TabScrollButtons.Default)
			{
				return TabScrollButtons.Automatic;
			}
			return _scrollButtons;
		}

		internal bool ShouldUseThemes()
		{
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_tabStateStyles.Reset();
			_tabStripFormatStyle.Reset();
			_showCloseButton = true;
			_scrollButtons = TabScrollButtons.Default;
			_windowPadding.Reset();
			_tabCaptionTrimming = StringTrimming.Character;
			_maxTabSize = 0;
			_groupOrientation = MdiGroupOrientation.Horizontal;
			_allowDrag = true;
			_useFormIcons = false;
			_imageSize = new Size(0, 0);
			_showCloseButtonOnSelectedTab = false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			return !_imageSize.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = new Size(0, 0);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_imageSize = (Size)LayoutReader.GetPropertyValue(reader, "ImageSize", typeof(Size), Size.Empty);
			_useFormIcons = (bool)LayoutReader.GetPropertyValue(reader, "UseFormIcons", typeof(bool), false);
			_allowDrag = (bool)LayoutReader.GetPropertyValue(reader, "AllowDrag", typeof(bool), true);
			_groupOrientation = (MdiGroupOrientation)LayoutReader.GetPropertyValue(reader, "GroupOrientation", typeof(MdiGroupOrientation), MdiGroupOrientation.Horizontal);
			_maxTabSize = (int)LayoutReader.GetPropertyValue(reader, "MaxTabSize", typeof(int), 0);
			_scrollButtons = (TabScrollButtons)LayoutReader.GetPropertyValue(reader, "ScrollButtons", typeof(TabScrollButtons), TabScrollButtons.Default);
			_showCloseButton = (bool)LayoutReader.GetPropertyValue(reader, "ShowCloseButton", typeof(bool), true);
			_tabCaptionTrimming = (StringTrimming)LayoutReader.GetPropertyValue(reader, "TabCaptionTrimming", typeof(StringTrimming), StringTrimming.Character);
			_dockingStyleMode = (DockingStyleMode)LayoutReader.GetPropertyValue(reader, "DockingStyleMode", typeof(DockingStyleMode), DockingStyleMode.VS2005);
			_showCloseButtonOnSelectedTab = (bool)LayoutReader.GetPropertyValue(reader, "ShowCloseButtonOnSelectedTab", typeof(bool), false);
			PropertyValue propertyValue = reader["TabStateStyles"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_tabStateStyles);
			}
			else
			{
				_tabStateStyles.Reset();
			}
			propertyValue = reader["TabStripFormatStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_tabStripFormatStyle);
			}
			else
			{
				_tabStripFormatStyle.Reset();
			}
			propertyValue = reader["WindowPadding"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_windowPadding);
			}
			else
			{
				_windowPadding.Reset();
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (!_allowDrag)
			{
				info.AddValue("AllowDrag", _allowDrag);
			}
			if (_groupOrientation != MdiGroupOrientation.Horizontal)
			{
				info.AddValue("GroupOrientation", _groupOrientation);
			}
			if (_dockingStyleMode != DockingStyleMode.VS2005)
			{
				info.AddValue("DockingStyleMode", _dockingStyleMode);
			}
			if (_maxTabSize != 0)
			{
				info.AddValue("MaxTabSize", _maxTabSize);
			}
			if (ShouldSerializeImageSize())
			{
				info.AddValue("ImageSize", _imageSize);
			}
			if (_scrollButtons != TabScrollButtons.Default)
			{
				info.AddValue("ScrollButtons", _scrollButtons);
			}
			if (!_showCloseButton)
			{
				info.AddValue("ShowCloseButton", _showCloseButton);
			}
			if (_showCloseButtonOnSelectedTab)
			{
				info.AddValue("ShowCloseButtonOnSelectedTab", _showCloseButtonOnSelectedTab);
			}
			if (_tabCaptionTrimming != StringTrimming.Character)
			{
				info.AddValue("TabCaptionTrimming", _tabCaptionTrimming);
			}
			if (_tabStateStyles.ShouldSerialize())
			{
				info.AddValue("TabStateStyles", _tabStateStyles);
			}
			if (!_tabStripFormatStyle.IsDefaultStyle())
			{
				info.AddValue("TabStripFormatStyle", _tabStripFormatStyle);
			}
			if (_useFormIcons)
			{
				info.AddValue("UseFormIcons", _useFormIcons);
			}
			if (_windowPadding.ShouldSerialize())
			{
				info.AddValue("WindowPadding", _windowPadding);
			}
		}

		void IFormatStyleOwner.PropertyChange()
		{
			NotifyManager();
		}
	}
}
