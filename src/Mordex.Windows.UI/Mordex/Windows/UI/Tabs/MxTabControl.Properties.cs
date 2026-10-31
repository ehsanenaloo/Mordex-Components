using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Tabs
{
	public partial class MxTabControl
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		Control IFormatStyleOwner.Owner => this;

		[Description("Indicates whether the control should automatically persist its application settings properties.")]
		[DefaultValue(false)]
		public bool SaveSettings
		{
			get
			{
				return _saveSettings;
			}
			set
			{
				_saveSettings = value;
			}
		}

		[Description("Gets or sets the value of the application settings key for the current instance of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string SettingsKey
		{
			get
			{
				if ((_settingsKey == null || _settingsKey.Length == 0) && base.Name != null && base.Name.Length > 0)
				{
					return base.Name;
				}
				if (_settingsKey == null)
				{
					return "";
				}
				return _settingsKey;
			}
			set
			{
				_settingsKey = value;
			}
		}

		public override Rectangle DisplayRectangle
		{
			get
			{
				if (_isLayoutDirty)
				{
					UpdateDisplayRectangle();
				}
				if (_selectedTab != null)
				{
					return _selectedTab.Bounds;
				}
				return _pageDisplayBounds;
			}
		}

		[DefaultValue(null)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TabLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.GetTabControl() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the control");
				}
				if (value != null && !value.CanBeLoaded())
				{
					throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
				}
				if (_currentLayout == value)
				{
					return;
				}
				CancelEventArgs e = new CancelEventArgs(cancel: false);
				OnCurrentLayoutChanging(e);
				if (!e.Cancel)
				{
					if (_currentLayout != null)
					{
						_currentLayout.SetTabControl(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.SetTabControl(this);
					}
					if (!_isInitializing)
					{
						ApplyCurrentLayout();
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TabLayoutCollection Layouts => _layouts;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DefaultValue(null)]
		[Browsable(false)]
		[Localizable(true)]
		public Stream LayoutStream
		{
			get
			{
				if (_layouts != null && _layouts.Count == 0)
				{
					return null;
				}
				MemoryStream memoryStream = new MemoryStream();
				SaveLayoutFile(memoryStream);
				memoryStream.Flush();
				memoryStream.Position = 0L;
				return memoryStream;
			}
			set
			{
				if (value != null)
				{
					if (!_isInitializing)
					{
						LoadLayoutFile(value);
					}
					else
					{
						_layoutStream = value;
					}
				}
			}
		}

		[Editor(typeof(TabPagesEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The collection of tab pages in this tab control.")]
		public TabPageCollection TabPages => _tabPages;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ControlCollection Controls => base.Controls;

		[Description("Specifies the parts of the border that will be painted in the control. This property has no effect using Windows XP style.")]
		[DefaultValue(PageBorder.All)]
		public PageBorder PageBorder
		{
			get
			{
				return _pageBorder;
			}
			set
			{
				if (_pageBorder != value)
				{
					_pageBorder = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(TabStripAlignment.Top)]
		[Description("Indicates the area of the control where the tabs are aligned.")]
		[Localizable(false)]
		public TabStripAlignment TabStripAlignment
		{
			get
			{
				return _tabStripAlignment;
			}
			set
			{
				if (_tabStripAlignment != value)
				{
					_tabStripAlignment = value;
					if (_selectedTab != null)
					{
						_selectedTab.ClearBackgroundCache();
					}
					ResetTabSizes();
				}
			}
		}

		[Description("Specifies the how the tab pages will be drawn on the control.")]
		[Localizable(false)]
		[DefaultValue(TabDisplay.ImageAndText)]
		public TabDisplay TabDisplay
		{
			get
			{
				return _tabDisplay;
			}
			set
			{
				if (_tabDisplay != value)
				{
					_tabDisplay = value;
					ResetTabSizes();
				}
			}
		}

		[Localizable(false)]
		[Description("Specifies the orientation of the text of the pages in the control.")]
		[DefaultValue(TextOrientation.Default)]
		public TextOrientation TextOrientation
		{
			get
			{
				return _textOrientation;
			}
			set
			{
				if (_textOrientation != value)
				{
					_textOrientation = value;
					ResetTabSizes();
				}
			}
		}

		[DefaultValue(false)]
		[Localizable(true)]
		[Description("Indicates whether more than one row of tabs can be displayed.")]
		public bool MultiLine
		{
			get
			{
				return _multiLine;
			}
			set
			{
				if (_multiLine != value)
				{
					_multiLine = value;
					ResetTabSizes();
				}
			}
		}

		[Localizable(true)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[Localizable(true)]
		[DefaultValue(true)]
		[Description("Indicates whether a ToolTip is shown when the mouse passes over a tab page.")]
		public bool ShowToolTips
		{
			get
			{
				return _showToolTips;
			}
			set
			{
				_showToolTips = value;
			}
		}

		[Localizable(true)]
		[DefaultValue(DefaultTabStripOffset)]
		[Description("Represents the offset of the tab strip in the control.")]
		public int TabStripOffset
		{
			get
			{
				return _tabStripOffset;
			}
			set
			{
				if (value < MinTabOffset)
				{
					value = MinTabOffset;
				}
				if (_tabStripOffset != value)
				{
					_tabStripOffset = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Represents the offset of the first tab page in the control.")]
		[DefaultValue(DefaultFirstTabOffset)]
		[Localizable(true)]
		public int FirstTabOffset
		{
			get
			{
				return _firstTabOffset;
			}
			set
			{
				if (value < MinTabOffset)
				{
					value = MinTabOffset;
				}
				if (_firstTabOffset != value)
				{
					_firstTabOffset = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the tab strip will be displayed in the control.")]
		public bool ShowTabs
		{
			get
			{
				return _showTabs;
			}
			set
			{
				if (_showTabs != value)
				{
					_showTabs = value;
					InvalidateLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[DefaultValue(-1)]
		public int SelectedIndex
		{
			get
			{
				if (_selectedTab != null)
				{
					return _selectedTab._index;
				}
				return _selectedIndex;
			}
			set
			{
				if (SelectedIndex == value)
				{
					return;
				}
				if (_tabPages.Count == 0)
				{
					_selectedIndex = -1;
				}
				else if (value >= 0)
				{
					_selectedIndex = value;
					if (_tabPages.Count > value)
					{
						SelectedTab = _tabPages[value];
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public MxTabPage InputFocusTab
		{
			get
			{
				return _inputFocusTab;
			}
			set
			{
				if (_inputFocusTab != value)
				{
					_inputFocusTab = value;
					EnsureVisible(_inputFocusTab);
					Invalidate();
				}
			}
		}

		[Description("Specifies whether a button for closing the selected tab is displayed. This property has no effect if the Multiline property of the UITab is set as true. With TabsInCaption, each tab in the title bar whose page allows closing gets its own close button.")]
		[DefaultValue(false)]
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
					InvalidateLayout();
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MxTabPage SelectedTab
		{
			get
			{
				return _selectedTab;
			}
			set
			{
				if (_selectedTab == value)
				{
					return;
				}
				if (value == null && HasVisibleTabs())
				{
					throw new ArgumentNullException();
				}
				if (value != null && !_tabPages.Contains(value) && RenderingHelper.GetParentControl(value) != this)
				{
					throw new InvalidOperationException("The SelectedTab must be in the TabPages collection of the control");
				}
				if (value != null && !value.TabVisible)
				{
					SelectNearestVisibleTab(value.Index);
					return;
				}
				TabCancelEventArgs e = new TabCancelEventArgs(_selectedTab, value);
				OnChangingSelectedTab(e);
				if (e.Cancel && _selectedTab != null)
				{
					return;
				}
				TabEventArgs changedArgs = new TabEventArgs(_selectedTab, value);
				if (_selectedTab != null)
				{
					_selectedTab.Visible = false;
					_selectedTab._isSelected = false;
				}
				_selectedTab = value;
				if (_inputFocusTab != value)
				{
					_inputFocusTab = value;
				}
				if (_selectedTab != null)
				{
					_selectedTab.Visible = true;
					_selectedTab._isSelected = true;
				}
				else
				{
					_firstVisibleTabIndex = -1;
				}
				ResetTabSizes();
				if (_isLayoutDirty)
				{
					Form form = RenderingHelper.FindForm(this);
					if (form != null && !form.Disposing && form.Visible)
					{
						LayoutTabsNow();
					}
				}
				if (!MultiLine)
				{
					EnsureVisible(_selectedTab);
				}
				Update();
				OnSelectedTabChanged(changedArgs);
				if (_selectedTab == null)
				{
					return;
				}
				TabPageRenderer uiTabPageRenderer = _pageRenderers.FindTabPageItem(_selectedTab);
				if (uiTabPageRenderer == null)
				{
					return;
				}
				try
				{
					Form ownerForm = RenderingHelper.FindForm(this);
					if (ownerForm != null && !ownerForm.Disposing && ownerForm.Visible)
					{
						AccessibilityNotifyClients(AccessibleEvents.Focus, uiTabPageRenderer._index);
					}
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// Accessibility notification is best effort and must not break tab selection.
				}
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The ImageList object from which this tabs takes its images.")]
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
					ResetTabSizes();
				}
			}
		}

		[Localizable(true)]
		[Description("The size used to draw images and icons on panels")]
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		[Description("The format style properties used to draw the background of the tab strip.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle TabStripFormatStyle
		{
			get
			{
				if (_tabStripFormatStyle == null)
				{
					_tabStripFormatStyle = new FormatStyle(this);
				}
				return _tabStripFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the background of the UITabPage.")]
		public FormatStyle PanelFormatStyle
		{
			get
			{
				if (_panelFormatStyle == null)
				{
					_panelFormatStyle = new FormatStyle(this);
				}
				return _panelFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the format styles used to draw the tab pages in their different states.")]
		public StateStyles TabsStateStyles
		{
			get
			{
				if (_tabStateStyles == null)
				{
					_tabStateStyles = new StateStyles(this);
				}
				return _tabStateStyles;
			}
		}

		[Localizable(true)]
		[Description("Indicates the size of the control's tabs.")]
		public Size ItemSize
		{
			get
			{
				return _itemSize;
			}
			set
			{
				if (_itemSize != value)
				{
					_itemSize = value;
					ResetTabSizes();
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
					ResetTabSizes();
				}
			}
		}

		[Description("Indicates how tabs are sized.")]
		[Localizable(true)]
		[DefaultValue(TabSizeMode.Normal)]
		public TabSizeMode SizeMode
		{
			get
			{
				return _sizeMode;
			}
			set
			{
				if (_sizeMode != value)
				{
					_sizeMode = value;
					ResetTabSizes();
				}
			}
		}

		[Browsable(false)]
		public int RowCount
		{
			get
			{
				if (_tabRows != null && _multiLine)
				{
					return _tabRows.Count;
				}
				return 1;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the control should display focus rectangles on the tab pages.")]
		public bool ShowFocusRectangle
		{
			get
			{
				return _showFocusRectangle;
			}
			set
			{
				if (_showFocusRectangle != value)
				{
					_showFocusRectangle = value;
					Invalidate();
				}
			}
		}

		[Description("The color of the border of the control when it\u00b4s border is Flat.")]
		public Color FlatBorderColor
		{
			get
			{
				if (_flatBorderColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.ControlDark);
				}
				return _flatBorderColor;
			}
			set
			{
				if (!_flatBorderColor.Equals(value))
				{
					_flatBorderColor = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the size in pixels of the scroll buttons")]
		public Size ScrollButtonSize
		{
			get
			{
				return _scrollButtonSize;
			}
			set
			{
				if (_scrollButtonSize != value)
				{
					_scrollButtonSize = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[Description("Indicates whether the control can receive keyboard focus")]
		[DefaultValue(true)]
		public bool FocusOnClick
		{
			get
			{
				return _focusOnClick;
			}
			set
			{
				_focusOnClick = value;
			}
		}

		Control ILayoutItemOwner.ParentControl => this;
	}
}
