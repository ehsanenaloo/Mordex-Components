using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
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
		bool IDockElement.AllowCancelDrag => true;

		Control IFormatStyleOwner.Owner => this;

		[Description("Determines which action is executed when the user double clicks the caption bar.")]
		[DefaultValue(CaptionDoubleClickAction.Default)]
		public CaptionDoubleClickAction CaptionDoubleClickAction
		{
			get
			{
				return _captionDoubleClickAction;
			}
			set
			{
				_captionDoubleClickAction = value;
			}
		}

		[Localizable(false)]
		[Category("Caption")]
		[Description("Indicates whether the caption bar is shown.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean CaptionVisible
		{
			get
			{
				return _captionVisible;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_captionVisible != value)
				{
					_captionVisible = value;
					RefreshLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Caption Format Settings")]
		[Description("Settings used to draw the caption bar")]
		public FormatStyle CaptionFormatStyle => _formatStyleCaption;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Caption Format Settings")]
		[Description("Settings used to draw the info text in the caption bar")]
		public FormatStyle InfoTextFormatStyle => _formatStyleInfoText;

		[Category("Caption Format Settings")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Settings used to draw the caption bar when the panel is active.")]
		public FormatStyle ActiveCaptionFormatStyle => _formatStyleActiveCaption;

		[Description("Indicates the border color of a panel.")]
		[Category("Panel Borders")]
		public Color BorderPanelColor
		{
			get
			{
				return _borderPanelColor;
			}
			set
			{
				if (!_borderPanelColor.Equals(value))
				{
					_borderPanelColor = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[DefaultValue(TabDisplayMode.UseDefault)]
		[Category("Caption")]
		[Description("Determines how the tab, representing the panel in an AutoHideStrip, is displayed.")]
		[Localizable(false)]
		public TabDisplayMode AutoHideTabDisplay
		{
			get
			{
				return _autoHideTabDisplay;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_autoHideTabDisplay != value)
				{
					_autoHideTabDisplay = value;
					if (_autoHide && PanelManager != null)
					{
						PanelManager.LayoutAutoHideStrips();
					}
				}
			}
		}

		[Description("Determines whether the control is docked or floating.")]
		[Localizable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual PanelDockState DockState
		{
			get
			{
				if (ParentGroup == null || _isNotifyingDockState)
				{
					return _dockState;
				}
				return ParentGroup.DockState;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelDockState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == DockState)
				{
					return;
				}
				if (ParentGroup != null && !_isInitializing)
				{
					throw new InvalidOperationException("DockState property can not be changed in a panel contained in a Group.");
				}
				if (_dockState != value)
				{
					PanelDockState panelDockState = _dockState;
					ApplyDockState(value);
					if (PanelManager != null)
					{
						PanelManager.MoveLayoutState(Id, panelDockState, _dockState);
					}
					PropagateDockState();
				}
			}
		}

		[Browsable(false)]
		public int PreferredCaptionHeight => GetCaptionHeight();

		[Localizable(true)]
		[DefaultValue(-1)]
		[Category("Caption")]
		[Description("The height of the caption bar in a panel or -1 to use the default height.")]
		public int CaptionHeight
		{
			get
			{
				return _captionHeight;
			}
			set
			{
				if (value < -1)
				{
					throw new ArgumentOutOfRangeException("CaptionHeight must be greater than or equal to 0 or use -1 to use the default PanelCaptionSize.");
				}
				if (_captionHeight != value)
				{
					_captionHeight = value;
					RefreshLayout();
				}
			}
		}

		[Description("Determines when the panel should paint its caption bar using the active caption style.")]
		[Localizable(false)]
		[DefaultValue(ActiveCaptionMode.UseDefault)]
		public ActiveCaptionMode ActiveCaptionMode
		{
			get
			{
				return _activeCaptionMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ActiveCaptionMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_activeCaptionMode != value)
				{
					_activeCaptionMode = value;
					RefreshLayout();
				}
			}
		}

		[Description("Determines whether the panels should use the ActiveCaptionFormatStyle to draw the caption bar when it is active.")]
		[Localizable(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is obsolete. Use ActiveCaptionMode property instead.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean UseActiveCaptionStyle
		{
			get
			{
				return _useActiveCaptionStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_useActiveCaptionStyle != value)
				{
					_useActiveCaptionStyle = value;
					switch (value)
					{
					case InheritableBoolean.True:
						ActiveCaptionMode = ActiveCaptionMode.PanelFocus;
						break;
					case InheritableBoolean.False:
						ActiveCaptionMode = ActiveCaptionMode.Never;
						break;
					default:
						ActiveCaptionMode = ActiveCaptionMode.UseDefault;
						break;
					}
				}
			}
		}

		[Localizable(false)]
		[DefaultValue(InheritableBoolean.Default)]
		[Description("Determines whether the panel can be resized.")]
		public InheritableBoolean AllowResize
		{
			get
			{
				return _allowResize;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_allowResize = value;
			}
		}

		[Category("Caption Format Settings")]
		[Localizable(false)]
		[Description("Determines which predefined style will be used to draw the background of the caption bar.")]
		[DefaultValue(PanelCaptionStyle.UseDefault)]
		public PanelCaptionStyle CaptionStyle
		{
			get
			{
				return _captionStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelCaptionStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_captionStyle != value)
				{
					_captionStyle = value;
					RecalculateLayout();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Guid Id
		{
			get
			{
				return _panelId;
			}
			set
			{
				if (value == Guid.Empty)
				{
					throw new ArgumentOutOfRangeException("Id property can not be an empty Guid.");
				}
				if (_panelId != value)
				{
					_panelId = value;
					_dockedLayoutState.SetId(_panelId);
					_floatingLayoutState.SetId(_panelId);
				}
			}
		}

		[Browsable(false)]
		public bool IsActive
		{
			get
			{
				if (_isActive && this is MxDockPanelGroup && ((MxDockPanelGroup)this).Panels.Count > 0)
				{
					return false;
				}
				return _isActive;
			}
		}

		[Browsable(false)]
		public MxFloatingWindow FloatingWindow => _floatingWindow;

		[Localizable(false)]
		[Description("Determines whether users can drop other panels over the control to create new panel groups.")]
		public InheritableBoolean AllowPanelDrop
		{
			get
			{
				return _allowPanelDrop;
			}
			set
			{
				if (_allowPanelDrop != value)
				{
					_allowPanelDrop = value;
				}
			}
		}

		[Localizable(false)]
		[Description("Determines whether the panel can be dragged to change its DockStyle or to create groups with other panels.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowPanelDrag
		{
			get
			{
				return _allowPanelDrag;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_allowPanelDrag != value)
				{
					_allowPanelDrag = value;
					RecalculateLayout();
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
				}
			}
		}

		[Localizable(false)]
		[DefaultValue(InheritableBoolean.Default)]
		[Category("Caption")]
		[Description("Indicates whether the auto hide button is visible.")]
		public InheritableBoolean AutoHideButtonVisible
		{
			get
			{
				return _autoHideButtonVisible;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_autoHideButtonVisible != value)
				{
					_autoHideButtonVisible = value;
					RefreshLayout();
				}
			}
		}

		[Category("Caption")]
		[DefaultValue(InheritableBoolean.Default)]
		[Localizable(false)]
		[Description("Indicates whether the close button is visible.")]
		public InheritableBoolean CloseButtonVisible
		{
			get
			{
				return _closeButtonVisible;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_closeButtonVisible != value)
				{
					_closeButtonVisible = value;
					RecalculateLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public MxDockManager PanelManager
		{
			get
			{
				if (_dockManager != null)
				{
					return _dockManager;
				}
				if (_parentGroup == null)
				{
					return null;
				}
				return _parentGroup.PanelManager;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public MxDockPanelGroup ParentGroup
		{
			get
			{
				if (_parentGroup is MxDockPanelGroup)
				{
					return (MxDockPanelGroup)_parentGroup;
				}
				return null;
			}
			set
			{
				if (value != ParentGroup)
				{
					MxDockManager panelManager = PanelManager;
					if (ParentGroup != null)
					{
						ParentGroup.Panels.Remove(this);
					}
					if (value != null)
					{
						value.Panels.Add(this);
					}
					else
					{
						panelManager.Panels.Add(this);
					}
				}
			}
		}

		[Description("Indicates whether the panel is drawn with a border around it.")]
		[Category("Panel Borders")]
		[DefaultValue(InheritableBoolean.Default)]
		[Localizable(false)]
		public InheritableBoolean BorderPanel
		{
			get
			{
				return _borderPanel;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_borderPanel != value)
				{
					_borderPanel = value;
					RecalculateLayout();
				}
			}
		}

		[DefaultValue(null)]
		public DockPanelCaptionContainer ContainerCaptionControl
		{
			get
			{
				return _containerCaptionControl;
			}
			set
			{
				if (value == null)
				{
					ContainerCaption = false;
				}
				if (_containerCaptionControl == value)
				{
					return;
				}
				if (_containerCaptionControl != null)
				{
					DetachContainerCaption();
				}
				_containerCaptionControl = value;
				if (_containerCaptionControl != null)
				{
					if (_containerCaptionControl.Panel != this)
					{
						_containerCaptionControl.Panel = this;
					}
					_containerCaptionControl.MouseDown += OnContainerCaptionControlMouseDown;
					_containerCaptionControl.MouseUp += OnContainerCaptionControlMouseUp;
					_containerCaptionControl.MouseMove += OnContainerCaptionControlMouseMove;
					_containerCaptionControl.DoubleClick += OnContainerCaptionControlDoubleClick;
					_containerCaptionControl.MouseLeave += OnContainerCaptionControlMouseLeave;
					Controls.Add(_containerCaptionControl);
				}
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the caption of the panel should act as a container for other controls.")]
		public bool ContainerCaption
		{
			get
			{
				return _useContainerCaption;
			}
			set
			{
				if (_useContainerCaption != value)
				{
					_useContainerCaption = value;
					RefreshLayout();
				}
			}
		}

		[DefaultValue(PanelCaptionDisplayMode.UseDefault)]
		[Description("Determines what is displayed in the caption bar.")]
		[Category("Caption")]
		[Localizable(false)]
		public PanelCaptionDisplayMode CaptionDisplayMode
		{
			get
			{
				return _captionDisplayMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelCaptionDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _captionDisplayMode)
				{
					_captionDisplayMode = value;
					RecalculateLayout();
				}
			}
		}

		[Description("The UIStateStyles that holds the format styles used to draw the tab representing the panel in their different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public StateStyles TabStateStyles => _tabStateStyles;

		[Category("Image")]
		[DefaultValue(null)]
		[Description("The image that is displayed on the panel caption.")]
		[Localizable(true)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		public Image Image
		{
			get
			{
				return _captionImage;
			}
			set
			{
				if (_captionImage != value)
				{
					_captionImage = value;
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
					RecalculateLayout();
				}
			}
		}

		[Description("The index of the image in the image list displayed on the panel.")]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Localizable(true)]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Category("Image")]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					_imageKey = "";
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
					RecalculateLayout();
				}
			}
		}

		[Category("Image")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[Description("The key accessor for the image in the ImageList displayed on the panel.")]
		[DefaultValue("")]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageKey != value)
				{
					_imageKey = value;
					_imageIndex = -1;
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
					RecalculateLayout();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The Icon that is displayed on the panel.")]
		[Category("Image")]
		[Localizable(true)]
		public Icon Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				if (_icon != value)
				{
					_icon = value;
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
					RecalculateLayout();
				}
			}
		}

		[Category("Tab Image in Outlook Navigator")]
		[Localizable(true)]
		[DefaultValue(null)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Description("The image that is displayed on the button representing the panel in an OutlookNavigator group.")]
		public Image LargeImage
		{
			get
			{
				return _largeImage;
			}
			set
			{
				if (_largeImage != value)
				{
					_largeImage = value;
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
				}
			}
		}

		[DefaultValue(null)]
		[Localizable(true)]
		[Description("The icon that is displayed on the button representing the panel in an OutlookNavigator group.")]
		[Category("Tab Image in Outlook Navigator")]
		public Icon LargeIcon
		{
			get
			{
				return _largeIcon;
			}
			set
			{
				if (_largeIcon != value)
				{
					_largeIcon = value;
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
				}
			}
		}

		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[Category("Tab Image in Outlook Navigator")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Localizable(true)]
		[Description("The index of the image in the image list that is displayed on the button representing the panel in an OutlookNavigator group.")]
		public int LargeImageIndex
		{
			get
			{
				return _largeImageIndex;
			}
			set
			{
				if (_largeImageIndex != value)
				{
					_largeImageIndex = value;
					_largeImageKey = "";
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
				}
			}
		}

		[Localizable(true)]
		[Description("The key accessor for the image in the ImageList displayed on the button representing the panel in an OutlookNavigator group.")]
		[Category("Tab Image in Outlook Navigator")]
		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		public string LargeImageKey
		{
			get
			{
				return _largeImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_largeImageKey != value)
				{
					_largeImageKey = value;
					_largeImageIndex = -1;
					if (ParentGroup != null)
					{
						ParentGroup.RecalculateLayout();
					}
				}
			}
		}

		[Description("Specifies to which edge of the container control a panel is docked to.")]
		[Localizable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PanelDockStyle DockStyle
		{
			get
			{
				return _dockedLayoutState.GetTopLevelState().GetDockStyle();
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelDockStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (ParentGroup != null)
				{
					throw new InvalidOperationException("DockStyle property can not be set in panels that are part of a group.\n\rSet the property in the panel group instead.");
				}
				if (DockStyle != value)
				{
					if (PanelManager != null && !_shouldSkipLayoutSync)
					{
						DockLayoutState dockedRoot = ((IPanelContainer)PanelManager).GetDockLayoutState(PanelDockState.Docked);
						DockLayoutState oldParent = _dockedLayoutState.GetParentState();
						_dockedLayoutState.Reparent(dockedRoot);
						if (oldParent != null && oldParent != dockedRoot)
						{
							CollapseLayoutGroupsAbove(oldParent, dockedRoot);
						}
					}
					// The getter reads the docked entry, so a floating panel keeps the value for when it is docked again.
					_dockedLayoutState.GetTopLevelState().SetDockStyle(value);
					DockLayoutState currentState = GetCurrentLayoutState();
					if (currentState != _dockedLayoutState)
					{
						currentState.SetDockStyle(value);
					}
					OnDockStyleChanged();
					if (PanelManager != null)
					{
						PanelManager.RaisePanelDockStyleChanged(this);
					}
				}
			}
		}

		// Folds the groups that held a moved entry, as RemoveRedundantGroups would on its next run.
		// Only that branch of the layout changes: no live panel, other branch or floating entry is touched.
		// The live tree needs no change, because a panel without a parent group is not a live child of these groups.
		private static void CollapseLayoutGroupsAbove(DockLayoutState group, DockLayoutState root)
		{
			ArrayList chain = new ArrayList();
			for (DockLayoutState state = group; state != null && state != root; state = state.GetParentState())
			{
				chain.Add(state);
			}
			if (chain.Count == 0 || ((DockLayoutState)chain[chain.Count - 1]).GetParentState() != root)
			{
				return;
			}
			foreach (DockLayoutState state in chain)
			{
				if (state.GetParentState() != null)
				{
					state.CollapseGroup();
				}
			}
		}

		[DefaultValue(false)]
		[Description("Determines whether the panel should automatically hide itself when it is not activated.")]
		[Localizable(true)]
		public virtual bool AutoHide
		{
			get
			{
				return _autoHide;
			}
			set
			{
				if (value && ParentGroup != null && ParentGroup.StaticGroup)
				{
					throw new InvalidOperationException("Can not set AutoHide in a panel contained in an ActsAsSinglePanel Group.");
				}
				if (value != AutoHide && CanChangeAutoHide())
				{
					_autoHide = value;
					if (PanelManager != null)
					{
						PanelManager.RaisePanelAutoHideChanged(this);
					}
					OnAutoHideChanged();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AutoHideActive
		{
			get
			{
				if (_autoHidePanel != null)
				{
					return true;
				}
				return false;
			}
			set
			{
				if (!IsAutoHideEffective())
				{
					return;
				}
				if (value)
				{
					if (GetAutoHideStrip() != null)
					{
						GetAutoHideStrip().ShowPopup(this, false);
					}
				}
				else if (GetAutoHideStrip() != null)
				{
					GetAutoHideStrip().HidePopup();
				}
				else if (_autoHidePanel != null)
				{
					_autoHidePanel.SlideClose(true);
				}
			}
		}

		[Description("Determines whether the panel is visible.")]
		[Localizable(true)]
		[DefaultValue(false)]
		public virtual bool Closed
		{
			get
			{
				return _closed;
			}
			set
			{
				if (value != Closed && CanChangeClosed())
				{
					SetIsChangingClosed(true);
					_closed = value;
					OnClosedChanged();
					if (DockState == PanelDockState.Floating && !value)
					{
						PropagateDockState();
					}
					SetIsChangingClosed(false);
				}
			}
		}

		[Description("The maximum size the panel can be resized to.")]
		[Localizable(true)]
		public new Size MaximumSize
		{
			get
			{
				return _maximumSize;
			}
			set
			{
				if (value.Height < -1 || value.Width < -1)
				{
					throw new ArgumentException("Maximum Size must be greater than or equal to 0. or -1 to use the default maximum size.");
				}
				if (_maximumSize != value)
				{
					_maximumSize = value;
					ApplySizeLimits();
				}
			}
		}

		[Localizable(true)]
		[Description("The minimum size the panel can be resized to.")]
		public new Size MinimumSize
		{
			get
			{
				return _minimumSize;
			}
			set
			{
				if (value.Height < -1 || value.Width < -1)
				{
					throw new ArgumentException("Minimum Size must be greater than or equal to 0. or -1 to use the default minimum (the size of the panel caption).");
				}
				if (_minimumSize != value)
				{
					_minimumSize = value;
					ApplySizeLimits();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
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

		[DefaultValue(InheritableBoolean.Default)]
		[Category("Panel Borders")]
		[Description("Indicates whether the caption is drawn with a border around it.")]
		[Localizable(false)]
		public InheritableBoolean BorderCaption
		{
			get
			{
				return _borderCaption;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_borderCaption != value)
				{
					_borderCaption = value;
					RefreshLayout();
				}
			}
		}

		[Category("Panel Borders")]
		[Description("Indicates the border color of the caption bar.")]
		public Color BorderCaptionColor
		{
			get
			{
				return _borderCaptionColor;
			}
			set
			{
				if (!_borderCaptionColor.Equals(value))
				{
					_borderCaptionColor = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				throw new InvalidOperationException("Can not set Dock property directly in a panel.\n\rUse DockStyle property instead.");
			}
		}

		public override Cursor Cursor
		{
			get
			{
				if (_captionRenderer != null && _captionRenderer.GetDragHandle() != null && _captionRenderer.GetHotItem() == _captionRenderer.GetDragHandle())
				{
					return Cursors.SizeAll;
				}
				Point position = Cursor.Position;
				position = PointToClient(position);
				switch (GetResizeModeAt(position.X, position.Y))
				{
				case ResizeMode.Horizontal:
					return Cursors.HSplit;
				case ResizeMode.Vertical:
					return Cursors.VSplit;
				default:
					return base.Cursor;
				}
			}
			set
			{
				base.Cursor = value;
			}
		}

		public override Rectangle DisplayRectangle => _displayBounds;

		[Description("The size and location of the control including its non client elements, in pixels, relative to the parent control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Rectangle Bounds
		{
			get
			{
				return base.Bounds;
			}
			set
			{
				_isLayoutSizing = false;
				base.Bounds = value;
				_isLayoutSizing = true;
			}
		}

		[Localizable(true)]
		[Description("Represents the height and width of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Size Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				_isLayoutSizing = false;
				base.Size = value;
				_isLayoutSizing = true;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new int Width
		{
			get
			{
				return base.Width;
			}
			set
			{
				_isLayoutSizing = false;
				base.Width = value;
				_isLayoutSizing = true;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new int Height
		{
			get
			{
				return base.Height;
			}
			set
			{
				_isLayoutSizing = false;
				base.Height = value;
				_isLayoutSizing = true;
			}
		}

		public override BindingContext BindingContext
		{
			get
			{
				if (PanelManager != null && PanelManager.ContainerControl != null)
				{
					return PanelManager.ContainerControl.BindingContext;
				}
				return base.BindingContext;
			}
		}

		protected virtual bool HasCaption
		{
			get
			{
				switch (_captionVisible)
				{
				case InheritableBoolean.False:
					return false;
				case InheritableBoolean.Default:
					if (PanelManager != null)
					{
						return PanelManager.DefaultPanelSettings.CaptionVisible;
					}
					break;
				}
				return true;
			}
		}

		protected virtual bool HasBorderPanel
		{
			get
			{
				if (_borderPanel == InheritableBoolean.Default)
				{
					if (PanelManager != null)
					{
						return PanelManager.DefaultPanelSettings.BorderPanel;
					}
				}
				else if (_borderPanel == InheritableBoolean.False)
				{
					return false;
				}
				return true;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ControlCollection Controls => base.Controls;

		protected override Size DefaultSize => new Size(DefaultPanelSize, DefaultPanelSize);

		[Description("Location of the panel when it acts as a floating panel.")]
		public Point FloatingLocation
		{
			get
			{
				return _floatingLayoutState.GetTopLevelState().GetLocation();
			}
			set
			{
				if (FloatingLocation != value)
				{
					_floatingLayoutState.GetTopLevelState().SetLocation(value);
					if (_floatingWindow != null && FloatingLocation != new Point(-1, -1))
					{
						_floatingWindow.Location = FloatingLocation;
					}
				}
			}
		}

		[Description("The size of a panel when its DockState is Floating.")]
		public Size FloatingSize
		{
			get
			{
				Size size = _floatingLayoutState.GetTopLevelState().GetPanelSize();
				if (size == new Size(-1, -1))
				{
					return DefaultSize;
				}
				return size;
			}
			set
			{
				if (FloatingSize != value)
				{
					_floatingLayoutState.GetTopLevelState().SetPanelSize(value);
					if (_floatingWindow != null)
					{
						_floatingWindow.Size = FloatingSize;
					}
				}
			}
		}

		[DefaultValue(PanelTextAlignment.UseDefault)]
		[Category("Caption")]
		[Description("Determines the alignment of the text in the panel caption.")]
		public PanelTextAlignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelTextAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _textAlignment)
				{
					_textAlignment = value;
					RefreshLayout();
				}
			}
		}

		[Description("Determines the alignment of the image in the panel caption.")]
		[Category("Caption")]
		[DefaultValue(PanelImageAlignment.UseDefault)]
		public PanelImageAlignment ImageAlignment
		{
			get
			{
				return _imageAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelImageAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _imageAlignment)
				{
					_imageAlignment = value;
					RefreshLayout();
				}
			}
		}

		[Description("Determines the alignment of the InfoText in the panel caption.")]
		[Category("Caption")]
		[DefaultValue(PanelTextAlignment.UseDefault)]
		public PanelTextAlignment InfoTextAlignment
		{
			get
			{
				return _infoTextAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelTextAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _infoTextAlignment)
				{
					_infoTextAlignment = value;
					RefreshLayout();
				}
			}
		}

		[Category("Caption")]
		[DefaultValue("")]
		[Description("Text to be displayed in the caption of a panel separated from the Panel's Text.")]
		public string InfoText
		{
			get
			{
				return _infoText;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (_infoText != value)
				{
					_infoText = value;
					RefreshLayout();
				}
			}
		}
	}
}
