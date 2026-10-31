using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonContextMenu
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandCollection Commands => _commandCollection;

		[DefaultValue("")]
		[Description("Represents the key used to identify the ContextMenu in a collection")]
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
					_key = value;
				}
			}
		}

		[DefaultValue("")]
		[Browsable(false)]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
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
				if (base.Site != null)
				{
					IComponentChangeService componentChangeService = (IComponentChangeService)base.Site.GetService(typeof(IComponentChangeService));
					if (componentChangeService != null)
					{
						componentChangeService.ComponentRename -= OnControlComponentRename;
					}
				}
				base.Site = value;
				if (value != null)
				{
					IComponentChangeService newComponentChangeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
					if (newComponentChangeService != null)
					{
						newComponentChangeService.ComponentRename += OnControlComponentRename;
					}
					if (_key == null)
					{
						_key = Site.Name;
					}
					if (base.Site.Name != null)
					{
						_name = base.Site.Name;
					}
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represents the ImageList to use when displaying commands as small icons in the control.")]
		[Localizable(true)]
		public ImageList ImageList
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
					RefreshChildMenuLayout();
				}
			}
		}

		[Description("Represents the ImageList to use when displaying items as large icons in the control.")]
		[Localizable(true)]
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
					RefreshChildMenuLayout();
				}
			}
		}

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
					RefreshChildMenuLayout();
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
					RefreshChildMenuLayout();
				}
			}
		}

		[Description("Indicates whether a ToolTip is shown when the mouse passes over a command.")]
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

		[Localizable(true)]
		[Description("Indicates whether the shortcut of the command is displayed on its tooltip")]
		[DefaultValue(false)]
		public bool ShowShortcutInToolTips
		{
			get
			{
				return _showShortcutInToolTips;
			}
			set
			{
				_showShortcutInToolTips = value;
			}
		}

		[Localizable(true)]
		[Description("Indicates whether the shortcut of the command is displayed in the menu window")]
		[DefaultValue(false)]
		public bool ShowShortcutInMenus
		{
			get
			{
				return _showShortcutInMenus;
			}
			set
			{
				_showShortcutInMenus = value;
			}
		}

		[Description("Indicates the maximum number of child commands to display in the dropdown list.")]
		[DefaultValue(25)]
		public int MaxDropDownItems
		{
			get
			{
				return _maxDropDownItems;
			}
			set
			{
				_maxDropDownItems = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		[DefaultValue(null)]
		[Browsable(false)]
		public Control SourceControl => _control;

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
					RefreshChildMenuLayout();
				}
			}
		}

		[Browsable(false)]
		public bool IsVisible => _childMenu != null;
	}
}
