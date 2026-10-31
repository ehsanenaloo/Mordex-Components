using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		Control IFormatStyleOwner.Owner => this;

		Form ICommandMenuOwner.FindForm => RenderingHelper.FindForm(this);

		Point ICommandMenuOwner.MousePoint
		{
			get
			{
				return _mousePoint;
			}
			set
			{
				_mousePoint = value;
			}
		}

		MxCommand ICommandMenuOwner.SelectedCommand
		{
			get
			{
				if (GetSelectedItem() != null)
				{
					return GetSelectedItem().GetCommand();
				}
				return null;
			}
			set
			{
				CommandLayoutState uiCommandLayoutState = _layoutItems.FindCommandItem(value);
				if (uiCommandLayoutState != null)
				{
					SetSelectedItem(uiCommandLayoutState);
				}
				else if (value != null && _customizeItem != null && value == _customizeItem.GetCommand())
				{
					SetSelectedItem(_customizeItem);
				}
				else
				{
					SetSelectedItem(null);
				}
			}
		}

		bool ICommandMenuOwner.CanUseThemes => IsThemed();

		MxCommandCollection ICommandMenuOwner.Commands => Commands;

		CommandControl ICommandMenuOwner.ChildMenu
		{
			get
			{
				return GetChildMenu();
			}
			set
			{
				SetChildMenu(value);
			}
		}

		bool ICommandMenuOwner.ShowToolTips
		{
			get
			{
				if (CommandManager != null)
				{
					return CommandManager.ShowToolTipOnMenus;
				}
				return false;
			}
		}

		bool ICommandMenuOwner.ShowInfrequentItems
		{
			get
			{
				return ShouldShowInfrequentItems();
			}
			set
			{
				SetShowInfrequentItems(value);
			}
		}

		MxCommandManager ICommandMenuOwner.CommandManagerOwner => CommandManager;

		DropDownAnimation ICommandMenuOwner.EffectiveAnimation => GetEffectiveAnimation();

		Font ICommandMenuOwner.Font => Font;

		Size ICommandMenuOwner.SmallImageSize
		{
			get
			{
				if (CommandManager != null)
				{
					return CommandManager.SmallImageSize;
				}
				return Size.Empty;
			}
		}

		Size ICommandMenuOwner.LargeImageSize
		{
			get
			{
				if (CommandManager != null && !CommandManager.LargeImageSize.IsEmpty)
				{
					return CommandManager.LargeImageSize;
				}
				return DefaultLargeImageSize;
			}
		}

		bool ICommandMenuOwner.UseSmallImages
		{
			get
			{
				if (GetEffectivePopupView() == View.LargeIcons && ResolveImageList(View.LargeIcons, null) != null)
				{
					return false;
				}
				return true;
			}
		}


		Control ICommandBarsElement.Control => this;

		MxCollection ICommandBarsElement.LayoutItems => _layoutItems;

		MxCommand ICommandBarsElement.SelectedCommand
		{
			get
			{
				return GetCustomizeSelectedCommand();
			}
			set
			{
				SetCustomizeSelectedCommand(value);
			}
		}

		MxCommandCollection ICommandBarsElement.Commands => Commands;

		bool IAccessibleCommandBarsElement.IsInitializing => IsInitializing();

		ISite IAccessibleCommandBarsElement.OwnerSite
		{
			get
			{
				if (Site != null)
				{
					return Site;
				}
				if (_commandManager != null)
				{
					return _commandManager.Site;
				}
				return Site;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public MxCommandManager CommandManager
		{
			get
			{
				return _commandManager;
			}
			set
			{
				if (_commandManager != value)
				{
					if (_commandManager != null)
					{
						throw new InvalidOperationException();
					}
					_commandManager = value;
					_commandManager.CommandBars.Add(this);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new AnchorStyles Anchor
		{
			get
			{
				return base.Anchor;
			}
			set
			{
				base.Anchor = value;
			}
		}

		[Description("Specifies the default appearance and behavior of this control")]
		[Localizable(false)]
		[DefaultValue(CommandBarType.ToolBar)]
		public CommandBarType CommandBarType
		{
			get
			{
				return _commandBarType;
			}
			set
			{
				if (_commandBarType != value)
				{
					_commandBarType = value;
					ReassignSharedRow();
					AttachActiveMdiChild();
					InvalidateLayout();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = value;
			}
		}

		[DefaultValue(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop
		{
			get
			{
				return base.TabStop;
			}
			set
			{
				base.TabStop = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int TabIndex
		{
			get
			{
				return base.TabIndex;
			}
			set
			{
				base.TabIndex = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
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

		[Localizable(false)]
		[Description("Specifies whether the user can customize the CommandBar.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowCustomize
		{
			get
			{
				return _allowCustomize;
			}
			set
			{
				if (_allowCustomize != value)
				{
					_allowCustomize = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Specifies whether the user can close the CommandBar.")]
		[Localizable(false)]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowClose
		{
			get
			{
				return _allowClose;
			}
			set
			{
				if (_allowClose != value)
				{
					_allowClose = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(false)]
		[Description("Inidcates whether the customize button will be shown.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean ShowCustomizeButton
		{
			get
			{
				return _showCustomizeButton;
			}
			set
			{
				if (_showCustomizeButton != value)
				{
					_showCustomizeButton = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Localizable(false)]
		[Description("Inidcates whether the 'Add or Remove Buttons' command will be displayed.")]
		public InheritableBoolean ShowAddRemoveButton
		{
			get
			{
				return _showAddRemoveButton;
			}
			set
			{
				if (_showAddRemoveButton != value)
				{
					_showAddRemoveButton = value;
				}
			}
		}

		[Description("Represents the key used to identify the control in a collection")]
		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (_key != value)
				{
					if (value.Length <= 0)
					{
						throw new ArgumentException("The CommandBar Key cannot be blank.");
					}
					if (CommandManager != null && !CommandManager.IsCommandBarKeyUnique(this, value))
					{
						throw new ArgumentException(string.Format("A CommandBar with key '{0}' already exist.", value));
					}
					_key = value;
					if (Text.Length == 0)
					{
						Text = _key;
					}
				}
			}
		}

		[Description("Specifies whether the CommandBar should extent its size to occupy the size of its rebar control")]
		[DefaultValue(false)]
		public bool FullRow
		{
			get
			{
				return _fullRow;
			}
			set
			{
				if (_fullRow != value)
				{
					_fullRow = value;
					ReassignSharedRow();
					InvalidateLayout();
				}
			}
		}

		[Description("Indicates whether the toolbar commands wrap to the next line if the toolbar becomes too small to display all the commands on the same line.")]
		[Localizable(false)]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean Wrappable
		{
			get
			{
				return _commandBarWrappable;
			}
			set
			{
				if (_commandBarWrappable != value)
				{
					_commandBarWrappable = value;
					_wrappable = value;
					if (_commandBarWrappable == InheritableBoolean.True)
					{
						_fullRow = true;
					}
					ReassignSharedRow();
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(TextAlign.Default)]
		[Localizable(false)]
		[Description("Indicates how the text of the commands are aligned, relative to their area, in the control")]
		public TextAlign TextAlign
		{
			get
			{
				return _textAlign;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlign), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_textAlign != value)
				{
					_textAlign = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Specifies which ImageList will be used to get the images for the UICommand objects in a popup window.")]
		[DefaultValue(View.Default)]
		[Localizable(false)]
		public View PopupView
		{
			get
			{
				return _popupView;
			}
			set
			{
				if (!Enum.IsDefined(typeof(View), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_popupView != value)
				{
					_popupView = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(View.Default)]
		[Description("Specifies which ImageList will be used to get the images for the UICommand objects.")]
		[Localizable(false)]
		public View View
		{
			get
			{
				return _view;
			}
			set
			{
				if (!Enum.IsDefined(typeof(View), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_view != value)
				{
					_view = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(false)]
		[DefaultValue(DropDownAnimation.Default)]
		[Description("Specifies the animation style used to show popup menus")]
		public DropDownAnimation Animation
		{
			get
			{
				return _animation;
			}
			set
			{
				if (!Enum.IsDefined(typeof(DropDownAnimation), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_animation = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Size Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				base.Size = value;
			}
		}

		[Description("The position of the top-left corner of the UICommandBar when the control is floated.")]
		[Localizable(true)]
		public Point FloatingLocation
		{
			get
			{
				return _floatingLocation;
			}
			set
			{
				_floatingLocation = value;
				if (GetFloatingWindow() != null)
				{
					_floatingWindow.Location = _floatingLocation;
				}
			}
		}

		[Localizable(true)]
		[Description("The size of the UICommandBar when the control is floated.")]
		public Size FloatingSize
		{
			get
			{
				if (_floatingSize.IsEmpty)
				{
					if (_isHorizontal)
					{
						return new Size(_fullLength, base.Height + FloatingFrameExtra);
					}
					return new Size(_fullLength, base.Width + FloatingFrameExtra);
				}
				return _floatingSize;
			}
			set
			{
				if (_floatingSize != value)
				{
					_floatingSize = value;
				}
			}
		}

		[Description("Specifies the position and manner in which the toolbar is docked.")]
		[DefaultValue(BarsDockStyle.Top)]
		public BarsDockStyle DockStyle
		{
			get
			{
				return _dockStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BarsDockStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_dockStyle == value && !_forceDockStyleUpdate)
				{
					return;
				}
				_forceDockStyleUpdate = false;
				bool isChanged = _dockStyle != value;
				BarsDockStyle prevDockStyle = _dockStyle;
				if (isChanged)
				{
					DockStyleCancelEventArgs e = new DockStyleCancelEventArgs(_dockStyle, value);
					OnChangingDockStyle(e);
					if (e.Cancel)
					{
						return;
					}
				}
				if (GetFloatingWindow() != null)
				{
					SetFloatingWindow(null);
				}
				else if (GetHostRow() != null && _messageFilter == null && value == BarsDockStyle.Floating)
				{
					SaveDockedPosition();
				}
				_dockStyle = value;
				if (_isInitializing && value != BarsDockStyle.Floating)
				{
					_isHorizontal = _dockStyle != BarsDockStyle.Right && _dockStyle != BarsDockStyle.Left;
					return;
				}
				if (CommandManager != null && CommandManager.ContainerControl != null)
				{
					switch (_dockStyle)
					{
					case BarsDockStyle.Top:
						SetHorizontal(true);
						if (!CommandManager.TopRebar.CommandBars.Contains(this))
						{
							CommandManager.TopRebar.CommandBars.Add(this);
						}
						break;
					case BarsDockStyle.Left:
						SetHorizontal(false);
						if (!CommandManager.LeftRebar.CommandBars.Contains(this))
						{
							CommandManager.LeftRebar.CommandBars.Add(this);
						}
						break;
					case BarsDockStyle.Bottom:
						SetHorizontal(true);
						if (!CommandManager.BottomRebar.CommandBars.Contains(this))
						{
							CommandManager.BottomRebar.CommandBars.Add(this);
						}
						break;
					case BarsDockStyle.Right:
						SetHorizontal(false);
						if (!CommandManager.RightRebar.CommandBars.Contains(this))
						{
							CommandManager.RightRebar.CommandBars.Add(this);
						}
						break;
					case BarsDockStyle.Floating:
						SetHorizontal(true);
						SetHost(null);
						if (CommandManager != null && !IsClosed())
						{
							CreateFloatingWindow();
						}
						break;
					}
				}
				InvalidateLayout();
				DockStyleEventArgs changedArgs = new DockStyleEventArgs(prevDockStyle, _dockStyle);
				OnDockStyleChanged(changedArgs);
			}
		}

		[Description("Determines whether the control is locked to its container.")]
		[Localizable(false)]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean LockCommandBar
		{
			get
			{
				return _lockCommandBar;
			}
			set
			{
				if (_lockCommandBar != value)
				{
					_lockCommandBar = value;
					InvalidateLayout();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public MxCommandCollection Commands => _commands;

		[Description("Represents the object that holds the format styles used to draw the commands in their different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public StateStyles CommandsStateStyles
		{
			get
			{
				if (_commandsStateStyles == null)
				{
					_commandsStateStyles = new StateStyles(this);
				}
				return _commandsStateStyles;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the control.")]
		public FormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new FormatStyle(this);
				}
				return _formatStyle;
			}
		}

		[Description("Specifies how the control will draw its commands.")]
		[DefaultValue(CommandStyle.Default)]
		public CommandStyle CommandsStyle
		{
			get
			{
				return _commandsStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CommandStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_commandsStyle != value)
				{
					_commandsStyle = value;
					InvalidateLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the popup windows associated with this control.")]
		public PopupSettings PopupSettings
		{
			get
			{
				if (_popupSettings == null)
				{
					if (_commandManager != null)
					{
						_popupSettings = new PopupSettings(this);
					}
					else
					{
						_popupSettings = new PopupSettings(this, empty: false);
					}
				}
				return _popupSettings;
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Indicates whether a ToolTip is shown when the mouse passes over a command.")]
		[Localizable(false)]
		public InheritableBoolean ShowToolTips
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public MxCommand FocusedCommand
		{
			get
			{
				if (_childMenu != null && _childMenu.GetActiveCommand() != null)
				{
					return _childMenu.GetActiveCommand();
				}
				if (_hotItem != null)
				{
					return _hotItem.GetCommand();
				}
				return null;
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The ImageList to use when displaying commands as small icons in the control.")]
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
					InvalidateLayout();
				}
			}
		}

		[Description("The ImageList to use when displaying commands as large icons in the control.")]
		[DefaultValue(null)]
		[Localizable(true)]
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
					InvalidateLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Point Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				if (!(base.Location != value))
				{
					return;
				}
				if (GetHostRow() != null && !IsInitializing())
				{
					if (IsHorizontalLayout())
					{
						MoveToLeft(value.X);
					}
					else
					{
						MoveToTop(value.Y);
					}
					SetPosition(Location);
				}
				else
				{
					base.Location = value;
					if (_dockStyle != BarsDockStyle.Floating)
					{
						SetPosition(Location);
					}
				}
			}
		}

		[Description("Indicates the relative row position of the toolbar when it is merged.")]
		[DefaultValue(-1)]
		public int MergeRowOrder
		{
			get
			{
				return _mergeRowOrder;
			}
			set
			{
				_mergeRowOrder = value;
			}
		}

		[Description("Specifies the index value of the row that contains the control in UIRebar.")]
		[Localizable(true)]
		[DefaultValue(-1)]
		public int RowIndex
		{
			get
			{
				if (GetHostRow() != null)
				{
					return GetHostRow().GetRowIndex();
				}
				return _rowIndex;
			}
			set
			{
				if (RowIndex != value || _forceRowUpdate)
				{
					_forceRowUpdate = false;
					if (value < 0)
					{
						value = 0;
					}
					if (_hostRow != null && _hostRow.HasRowOwner())
					{
						_hostRow.GetBars().Remove(this);
					}
					_rowIndex = value;
					if (_isInitializing)
					{
						return;
					}
					if (GetHost() != null)
					{
						if (_rowIndex > GetHost().GetRows().Count)
						{
							_rowIndex = GetHost().GetRows().Count;
						}
						bool isAloneInRow = GetHostRow() != null && GetHostRow().GetBars().Count <= 1;
						if (isAloneInRow)
						{
							if (GetHost().GetRows().Count > _rowIndex)
							{
								CommandBarHostLayoutState uiRebarLayoutState = (CommandBarHostLayoutState)GetHost().GetRows()[_rowIndex];
								if (!uiRebarLayoutState.AllowsSharedRow(this))
								{
									if (_rowIndex > RowIndex)
									{
										GetHost().SwapRows(GetHostRow(), _rowIndex);
										return;
									}
									isAloneInRow = false;
								}
							}
							if (_rowIndex > 0 && GetHostRow() != null && GetHostRow().GetRowIndex() <= _rowIndex)
							{
								_rowIndex--;
							}
						}
						if (GetHostRow() != null)
						{
							GetHostRow().GetBars().Remove(this);
						}
						if (!isAloneInRow || GetHost().GetRows().Count == 0)
						{
							GetHost().AddRowForBar(this);
						}
						else
						{
							if (_rowIndex >= GetHost().GetRows().Count)
							{
								GetHost().AddRowForBar(this);
								return;
							}
							CommandBarHostLayoutState targetRow = (CommandBarHostLayoutState)_host.GetRows()[_rowIndex];
							try
							{
								targetRow.GetBars().Add(this);
							}
							catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
							{
								// The row can reject the bar while the layout is rebuilt; the bar keeps its current row.
							}
						}
					}
					if (GetHostRow() == null)
					{
						return;
					}
					if (GetPosition().IsEmpty)
					{
						MoveToPosition(Location);
					}
					if (!IsDragging())
					{
						if (_isHorizontal)
						{
							MoveToLeft(GetPosition().X);
						}
						else
						{
							MoveToTop(GetPosition().Y);
						}
					}
				}
				else if (_isInitializing)
				{
					_rowIndex = value;
				}
			}
		}

		[Description("Indicates whether the user can rename this CommandBar using the Customize Form")]
		[DefaultValue(false)]
		[Localizable(true)]
		public bool AllowRename
		{
			get
			{
				return _allowRename;
			}
			set
			{
				_allowRename = value;
			}
		}

		[Description("Indicates whether the user can delete this CommandBar using the Customize Form")]
		[Localizable(true)]
		[DefaultValue(false)]
		public bool AllowDelete
		{
			get
			{
				return _allowDelete;
			}
			set
			{
				_allowDelete = value;
			}
		}

		[Localizable(false)]
		[DefaultValue(InheritableBoolean.Default)]
		[Description("Specifies whether the control will merge its commands with the commands of a UICommandBar from a MDI Parent or MDI Child form.")]
		public InheritableBoolean AllowMerge
		{
			get
			{
				return _allowMerge;
			}
			set
			{
				_allowMerge = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsFullShown => _isFullShown;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ControlCollection Controls => base.Controls;

		[Browsable(false)]
		[Description("The cursor that is displayed when the mouse pointer is over the command.")]
		[Localizable(true)]
		public override Cursor Cursor
		{
			get
			{
				Point pt = PointToClient(Cursor.Position);
				if (_gripItem != null)
				{
					if (_gripItem._bounds.Contains(pt))
					{
						return Cursors.SizeAll;
					}
				}
				else
				{
					CommandLayoutState hitItem = HitTestItem(pt.X, pt.Y);
					if (hitItem != null && hitItem.GetCommand() != null && hitItem.GetCommand().Cursor != null)
					{
						return hitItem.GetCommand().Cursor;
					}
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		Control ILayoutItemOwner.ParentControl => this;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCursor()
		{
			return false;
		}
	}
}
