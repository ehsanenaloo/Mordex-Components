using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxContextMenu
	{
		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		ISite IAccessibleCommandBarsElement.OwnerSite
		{
			get
			{
				if (Site != null)
				{
					return Site;
				}
				if (CommandManager != null)
				{
					return CommandManager.Site;
				}
				return Site;
			}
		}

		bool IAccessibleCommandBarsElement.IsInitializing => IsInitializing();

		AccessibleObject IAccessibleCommandBarsElement.AccessibilityObject => null;

		Control IFormatStyleOwner.Owner => null;

		Form ICommandMenuOwner.FindForm
		{
			get
			{
				if (SourceControl != null)
				{
					return RenderingHelper.FindForm(_sourceControl);
				}
				if (_commandManager != null)
				{
					return _commandManager.GetForm();
				}
				return null;
			}
		}

		Font ICommandMenuOwner.Font
		{
			get
			{
				if (_sourceControl != null)
				{
					Form form = RenderingHelper.FindForm(_sourceControl);
					if (form != null)
					{
						return form.Font;
					}
					return _sourceControl.Font;
				}
				return Control.DefaultFont;
			}
		}

		Point ICommandMenuOwner.MousePoint
		{
			get
			{
				return Point.Empty;
			}
			set
			{
			}
		}

		bool ICommandMenuOwner.CanUseThemes => VisualStyleSupport.IsThemingUsable(false);

		MxCommandManager ICommandMenuOwner.CommandManagerOwner => CommandManager;

		bool ICommandMenuOwner.UseSmallImages
		{
			get
			{
				if (GetEffectiveView() == View.LargeIcons && GetImageListForView(View.LargeIcons, null) != null)
				{
					return false;
				}
				return true;
			}
		}

		MxCommandCollection ICommandMenuOwner.Commands => Commands;

		Size ICommandMenuOwner.LargeImageSize
		{
			get
			{
				if (_largeImageSize.IsEmpty && CommandManager != null)
				{
					return CommandManager.LargeImageSize;
				}
				return _largeImageSize;
			}
		}

		Size ICommandMenuOwner.SmallImageSize
		{
			get
			{
				if (_smallImageSize.IsEmpty && CommandManager != null)
				{
					return CommandManager.SmallImageSize;
				}
				return _smallImageSize;
			}
		}


		DropDownAnimation ICommandMenuOwner.EffectiveAnimation => GetEffectiveDropDownAnimation();

		MxCommand ICommandMenuOwner.SelectedCommand
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		CommandControl ICommandMenuOwner.ChildMenu
		{
			get
			{
				return GetPopupControl();
			}
			set
			{
				SetPopupControl(value);
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

		bool ICommandMenuOwner.ShowToolTips => ShouldShowToolTips();

		[Localizable(true)]
		[Description("Determines the size that images will have when drawing a command in small icons view.")]
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
					InvalidateMenu();
				}
			}
		}

		[Description("Determines the size that images will have when drawing a command in large icons view.")]
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
					InvalidateMenu();
				}
			}
		}

		[Localizable(false)]
		[DefaultValue(InheritableBoolean.Default)]
		[Description("Indicates whether a ToolTip is shown when the mouse passes over a command.")]
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

		[Browsable(false)]
		[DefaultValue(null)]
		public Control SourceControl => _sourceControl;

		[Browsable(false)]
		public int Index => _index;

		[Browsable(false)]
		public bool IsVisible => _popupControl != null;

		[Description("Represents the key used to identify the UIContextMenu in a collection")]
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
						throw new ArgumentException("The UIContextMenu Key cannot be blank.");
					}
					if (CommandManager != null && !CommandManager.IsContextMenuKeyUnique(this, value))
					{
						throw new ArgumentException(string.Format("A UIContextMenu with key '{0}' already exist.", value));
					}
					_key = value;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public MxCommandCollection Commands => _commands;

		[DefaultValue(null)]
		[Description("Represents the ImageList to use when displaying commands as small icons in the control.")]
		[Localizable(true)]
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
					InvalidateMenu();
				}
			}
		}

		[DefaultValue(null)]
		[Localizable(true)]
		[Description("Represents the ImageList to use when displaying items as large icons in the control.")]
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
					InvalidateMenu();
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
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
					_commandManager.ContextMenus.Add(this);
				}
			}
		}

		[DefaultValue(RightToLeft.Inherit)]
		public RightToLeft RightToLeft
		{
			get
			{
				return _rightToLeft;
			}
			set
			{
				_rightToLeft = value;
			}
		}

		[Description("Represents the format style properties used to draw the popup windows associated with this component.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public PopupSettings PopupSettings
		{
			get
			{
				if (_popupSettings == null)
				{
					_popupSettings = new PopupSettings(this, empty: false);
				}
				return _popupSettings;
			}
		}

		[Description("Specifies whether a shadow is painted under the drop down menus")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean ShowShadowUnderMenus
		{
			get
			{
				return _showShadowUnderMenus;
			}
			set
			{
				if (_showShadowUnderMenus != value)
				{
					_showShadowUnderMenus = value;
				}
			}
		}

		[Localizable(false)]
		[Description("Specifies which ImageList will be used to get the images for the UICommand objects.")]
		[DefaultValue(View.Default)]
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
					InvalidateMenu();
				}
			}
		}

		[DefaultValue(true)]
		[Description("If True the first character preceded by an ampersand (&) will be used as the command's mnemonic key")]
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
					InvalidateMenu();
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
				return _dropDownAnimation;
			}
			set
			{
				if (!Enum.IsDefined(typeof(DropDownAnimation), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_dropDownAnimation = value;
			}
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
		public bool ShouldSerializeCommands()
		{
			return _commands.Count > 0;
		}
	}
}
