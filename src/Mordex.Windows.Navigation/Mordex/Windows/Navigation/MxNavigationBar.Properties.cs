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
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(false)]
		[Description("Indicates whether the control should automatically persist its application settings properties.")]
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

		[Category("Behavior")]
		[DefaultValue(UnfoldEffect.Slide)]
		[Description("Specifies which effect is used when groups are expanded/collapsed.")]
		public UnfoldEffect UnfoldEffect
		{
			get
			{
				return _unfoldEffect;
			}
			set
			{
				_unfoldEffect = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarLayoutCollection Layouts => _layouts;

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public NavigationBarLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.ResolveNavigationBar() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the control");
				}
				if (value != null && !value.IsResolvable())
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
						_currentLayout.SetAttachedNavigationBar(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.SetAttachedNavigationBar(this);
					}
					if (!_isInitializing)
					{
						ApplyCurrentLayout();
					}
				}
			}
		}

		[DefaultValue(true)]
		[Category("Behavior")]
		[Description("Indicates if the NavigationItem appears sunken while its selected.")]
		public bool KeepSelection
		{
			get
			{
				return _keepSelection;
			}
			set
			{
				if (_keepSelection == value)
				{
					return;
				}
				_keepSelection = value;
				if (_selectedItem != null)
				{
					if (!value)
					{
						_selectedItem._itemState = ItemState.Normal;
					}
					else
					{
						_selectedItem._itemState = ItemState.Selected;
					}
				}
				Invalidate();
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

		[DefaultValue(ScrollLocation.ItemsLevel)]
		[Category("Behavior")]
		[Description("Indicates the location of the scroll buttons in the NavigationBar.")]
		public ScrollLocation ScrollLocation
		{
			get
			{
				return _scrollLocation;
			}
			set
			{
				if (_scrollLocation != value)
				{
					_scrollLocation = value;
					_scrollNextButton.SetButtonEnabled(true);
					_scrollNextButton.Bounds = Rectangle.Empty;
					_scrollNextButton._scrollLocation = value;
					_scrollPreviousButton.SetButtonEnabled(true);
					_scrollPreviousButton.Bounds = Rectangle.Empty;
					_scrollPreviousButton._scrollLocation = value;
					InvalidateLayout(true);
				}
			}
		}

		[Description("Gets or sets a value indicating whether the header of the groups are visible.")]
		[DefaultValue(true)]
		[Category("Appearance")]
		public bool HeaderGroupVisible
		{
			get
			{
				return _headerGroupVisible;
			}
			set
			{
				if (_headerGroupVisible != value)
				{
					_headerGroupVisible = value;
					InvalidateLayout(true);
				}
			}
		}

		[Description("Indicates whether a ToolTip is shown when the mouse passes over a group or item.")]
		[Category("Behavior")]
		[DefaultValue(true)]
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
					InvalidateLayout(false);
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Indicates whether an ampersand (&) is included in the text of the control.")]
		public bool UseMnemonic
		{
			get
			{
				return _useMnemonic;
			}
			set
			{
				if (_useMnemonic != value)
				{
					_useMnemonic = value;
					InvalidateLayout(false);
				}
			}
		}

		[Description("Indicates whether the user can drag groups to change their position in the NavigationBar.")]
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool AllowDragGroups
		{
			get
			{
				return _allowDragGroups;
			}
			set
			{
				_allowDragGroups = value;
			}
		}

		[Category("Behavior")]
		[Description("Gets or sets a value specifying which area of a NavigationBar control needs to be drawn manually.")]
		[Editor(typeof(NavigationBarUITypeEditor), typeof(UITypeEditor))]
		[DefaultValue(OwnerDrawnArea.None)]
		[TypeConverter(typeof(FlagsEnumConverter))]
		public OwnerDrawnArea OwnerDrawnArea
		{
			get
			{
				return _ownerDrawnArea;
			}
			set
			{
				_ownerDrawnArea = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public NavigationGroupCollection Groups => _groups;

		[Description("The NavigationItem object currently selected.")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public NavigationItem SelectedItem
		{
			get
			{
				return _selectedItem;
			}
			set
			{
				if (_selectedItem == value)
				{
					return;
				}
				ItemCancelEventArgs e = new ItemCancelEventArgs(value);
				OnSelectingItem(e);
				if (e.Cancel)
				{
					if (value != null)
					{
						value._itemState = ItemState.Normal;
						value.UpdateSelected(false);
					}
					Invalidate();
					return;
				}
				if (_selectedItem != null)
				{
					_selectedItem._itemState = ItemState.Normal;
					_selectedItem.UpdateSelected(false);
					Invalidate(Rectangle.Inflate(_selectedItem.GetBounds(), 4, 4));
					Update();
				}
				_selectedItem = value;
				if (_selectedItem != null)
				{
					_selectedItem.UpdateSelected(true);
				}
				EventArgs itemSelectedArgs = new EventArgs();
				OnItemSelected(itemSelectedArgs);
				if (_selectedItem != null)
				{
					if (_selectedItem._owner.GetGroup() != _selectedGroup)
					{
						SelectedGroup = _selectedItem._owner.GetGroup();
						Update();
					}
					if (_keepSelection)
					{
						_selectedItem._itemState = ItemState.Selected;
					}
					EnsureVisible(_selectedItem);
				}
				Invalidate();
			}
		}

		// A removed item cannot stay selected, so the selection is cleared even when SelectingItem cancels it.
		internal void ClearSelectionOfRemovedItem(NavigationItem item)
		{
			SelectedItem = null;
			if (_selectedItem == item)
			{
				item._itemState = ItemState.Normal;
				item.UpdateSelected(false);
				_selectedItem = null;
				OnItemSelected(EventArgs.Empty);
			}
		}

		[Category("Behavior")]
		[DefaultValue(DragMode.Automatic)]
		public DragMode DragMode
		{
			get
			{
				return _dragMode;
			}
			set
			{
				_dragMode = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The NavigationGroup object currently selected.")]
		public NavigationGroup SelectedGroup
		{
			get
			{
				return _selectedGroup;
			}
			set
			{
				if (value == _selectedGroup)
				{
					return;
				}
				if (value == null && _groups.Count > 0)
				{
					throw new ArgumentNullException();
				}
				GroupCancelEventArgs e = new GroupCancelEventArgs(value);
				OnSelectingGroup(e);
				if (e.Cancel)
				{
					_groupSelectionAccepted = false;
					return;
				}
				_groupSelectionAccepted = true;
				if (_selectedGroup != null)
				{
					_selectedGroup.SetSelected(false);
				}
				_selectedGroup = value;
				EventArgs groupSelectedArgs = new EventArgs();
				_firstVisibleItem = null;
				if (_selectedGroup != null)
				{
					_selectedGroup.SetSelected(true);
				}
				SetHoverItem(null);
				OnGroupSelected(groupSelectedArgs);
				if (_selectedGroup != null && !_selectedGroup.Visible)
				{
					OnGroupVisibilityChanged(_selectedGroup, false);
				}
				InvalidateLayout(true);
			}
		}

		[DefaultValue(NavigationBarOrientation.Vertical)]
		[Description("Specifies the orientation of the NavigationBar control.")]
		[Category("Behavior")]
		public NavigationBarOrientation Orientation
		{
			get
			{
				return _orientation;
			}
			set
			{
				if (_orientation != value)
				{
					_orientation = value;
					_scrollNextButton.UpdateOrientation(_orientation);
					_scrollPreviousButton.UpdateOrientation(_orientation);
					InvalidateLayout(true);
				}
			}
		}

		[DefaultValue(SelectionArea.Image)]
		[Description("Specifies which part of the NavigationItem objects will be selected when the mouse pointer moves over them.")]
		[Category("Appearance")]
		public SelectionArea SelectionArea
		{
			get
			{
				return _selectionArea;
			}
			set
			{
				if (_selectionArea != value)
				{
					_selectionArea = value;
					InvalidateItemLayout();
				}
			}
		}

		MxNavigationBar INavigationBarOwner.NavigationBar => this;

		private void ResetDragState()
		{
			_isDropAfterItem = false;
			_dragGroupIndex = -1;
			_dropLinePosition = -1;
			_cancelDrag = true;
			_pressedGroup = null;
			_dragSourceItem = null;
			if (_selectedItem != null && !_keepSelection)
			{
				_selectedItem._itemState = ItemState.Normal;
			}
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateLayout(false);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}
	}
}
