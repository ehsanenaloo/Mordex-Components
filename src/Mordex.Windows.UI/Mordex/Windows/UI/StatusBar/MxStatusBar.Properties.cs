using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.StatusBar
{
	public partial class MxStatusBar
	{
		Control IFormatStyleOwner.Owner => this;

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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the collection of panels contained whithin the control.")]
		public StatusBarPanelCollection Panels => _panels;

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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw panels associated with this control.")]
		public FormatStyle PanelsFormatStyle
		{
			get
			{
				if (_panelsFormatStyle == null)
				{
					_panelsFormatStyle = new FormatStyle(this);
				}
				return _panelsFormatStyle;
			}
		}

		[Description("Represents the format style properties used to draw panels associated with this control when the mouse is over them.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle HotPanelsFormatStyle
		{
			get
			{
				if (_hotPanelsFormatStyle == null)
				{
					_hotPanelsFormatStyle = new FormatStyle(this);
				}
				return _hotPanelsFormatStyle;
			}
		}

		[Description("The ImageList that contains the images used in the control.")]
		[Localizable(true)]
		[DefaultValue(null)]
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

		public override Cursor Cursor
		{
			get
			{
				if (_sizeGripState != null)
				{
					Point pt = PointToClient(Cursor.Position);
					if (_sizeGripState._bounds.Contains(pt))
					{
						return Cursors.SizeNWSE;
					}
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		[Description("Indicates whether a ToolTip is shown when the mouse passes over a panel.")]
		[DefaultValue(true)]
		[Localizable(true)]
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
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

		[Description("The color of the border of the panels")]
		public Color PanelsBorderColor
		{
			get
			{
				if (_panelsBorderColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.ControlDark);
				}
				return _panelsBorderColor;
			}
			set
			{
				if (_panelsBorderColor != value)
				{
					_panelsBorderColor = value;
					Invalidate();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			return !_imageSize.Equals(DefaultImageSize);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = DefaultImageSize;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializePanelsBorderColor()
		{
			return !_panelsBorderColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetPanelsBorderColor()
		{
			PanelsBorderColor = Color.Empty;
		}
	}
}
