using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonStatusBar
	{
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[DefaultValue(DockStyle.Bottom)]
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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
				if (CanShowSizeGrip() && _rightPanelRenderer != null)
				{
					Point pt = PointToClient(Cursor.Position);
					if (_rightPanelRenderer._sizeGripBounds.Contains(pt))
					{
						return Cursors.SizeNWSE;
					}
				}
				return base.Cursor;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CommandCollection LeftPanelCommands => _leftPanelCommands;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CommandCollection RightPanelCommands => _rightPanelCommands;

		[DefaultValue(true)]
		[Description("Indicates whether the resize grib should be displayed")]
		public bool ShowSizeGrip
		{
			get
			{
				return _showSizeGrip;
			}
			set
			{
				if (_showSizeGrip != value)
				{
					_showSizeGrip = value;
					RebuildPanelLayouts();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether a ToolTip is shown when the mouse passes over a command.")]
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

		[DefaultValue(9000)]
		[Description("Represents the period of time the ToolTip remains visible if the pointer is stationary on a control with specified ToolTip text.")]
		public int SuperTipDelay
		{
			get
			{
				return _superTipDelay;
			}
			set
			{
				_superTipDelay = value;
			}
		}

		[Description("Represents the width of the supertip for the commands")]
		[DefaultValue(210)]
		public int SuperTipWidth
		{
			get
			{
				return _superTipWidth;
			}
			set
			{
				_superTipWidth = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the MordexSuperTip component to show the supertip settings of the commands")]
		public MxSuperTip SuperTipComponent => _mordexSuperTip;

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
					InvalidateLayout();
				}
			}
		}

		[Description("The ImageList to use when displaying commands as small icons.")]
		[Localizable(true)]
		[DefaultValue(null)]
		public ImageList ImageList
		{
			get
			{
				return _commandImageList;
			}
			set
			{
				if (_commandImageList != value)
				{
					_commandImageList = value;
					if (_mordexSuperTip != null && _mordexSuperTip.ImageList == null)
					{
						_mordexSuperTip.ImageList = _commandImageList;
					}
					InvalidateLayout();
				}
			}
		}

		[Description("Determines the size that images will have when drawing a command.")]
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
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
				if (value != null)
				{
					IComponentChangeService changeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
					if (changeService != null)
					{
						changeService.ComponentRename += OnControlComponentRename;
					}
				}
			}
		}
	}
}
