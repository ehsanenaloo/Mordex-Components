using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class ScrollableBase
	{
		[Description("The width of scrollbars in the control.")]
		public int ScrollBarWidth
		{
			get
			{
				if (_scrollBarWidth == -1)
				{
					return SystemInformation.GetVerticalScrollBarWidthForDpi(DeviceDpi);
				}
				return _scrollBarWidth;
			}
			set
			{
				if (value < -1)
				{
					throw new InvalidEnumArgumentException();
				}
				if (_scrollBarWidth != value)
				{
					_scrollBarWidth = value;
					if (_horizontalScrollBar != null)
					{
						_horizontalScrollBar.SetScrollBarThickness(_scrollBarWidth);
					}
					if (_vScroll != null)
					{
						_vScroll.SetScrollBarThickness(_scrollBarWidth);
					}
					if (_numericEditControl != null)
					{
						UpdateNavigatorLayout();
					}
					RelayoutScrollControls();
				}
			}
		}

		Font IGridStyleSource.ControlStyleFont
		{
			get
			{
				if (_controlStyleFont != null)
				{
					return _controlStyleFont;
				}
				return Font;
			}
		}

		bool IGridStyleSource.IsRightToLeft => GetScrollableBaseBoolean();

		[DefaultValue(BorderStyle.SunkenLight3D)]
		[Description("Represents the border style for the control.")]
		[Category("Appearance")]
		public BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_borderStyle != value)
				{
					_borderStyle = value;
					RelayoutScrollControls();
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Size ClientSize
		{
			get
			{
				return ClientRectangle.Size;
			}
			set
			{
				UpdateBorderWidth();
				Size clientSize = new Size(value.Width + _borderWidth * 2, value.Height + _borderWidth * 2);
				InitializeControls(new Rectangle(0, 0, clientSize.Width, clientSize.Height));
				if (_isHorizontalBarVisible)
				{
					clientSize.Height += _scrollableBasePanel.Height;
				}
				if (_isVerticalBarVisible)
				{
					clientSize.Width += _vScroll.Width;
				}
				base.ClientSize = clientSize;
			}
		}

		public new Rectangle ClientRectangle => _contentBounds;

		[DefaultValue(ScrollBars.Automatic)]
		[Description("Specifies which scroll bars should appear in the control.")]
		[Category("Behavior")]
		public ScrollBars ScrollBars
		{
			get
			{
				return _scrollBars;
			}
			set
			{
				if (_scrollBars != value)
				{
					_scrollBars = value;
					RelayoutScrollControls();
					OnPanelResize(_scrollableBasePanel, EventArgs.Empty);
				}
			}
		}

		[Description("Indicates whether the record navigator should be shown.")]
		[Category("Behavior")]
		[DefaultValue(false)]
		public bool RecordNavigator
		{
			get
			{
				return _recordNavigator;
			}
			set
			{
				if (value != _recordNavigator)
				{
					_recordNavigator = value;
					RelayoutScrollControls();
					OnPanelResize(_scrollableBasePanel, EventArgs.Empty);
				}
			}
		}

		[Category("Appearance")]
		[Description("The ControlStyle object used to draw internal controls in the grid like scrollbars and buttons.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlStyle ControlStyle => _gridControlStyle;

		[Description("Indicates the color of the flat border of the control and headers.")]
		public Color FlatBorderColor
		{
			get
			{
				if (_gridControlStyle != null)
				{
					return _gridControlStyle.GetFlatBorderColor();
				}
				return SystemColors.WindowFrame;
			}
			set
			{
				_gridControlStyle.SetFlatBorderColor(value);
			}
		}

		public new Color ForeColor
		{
			get
			{
				if (IsThemedLook && !ShouldSerializeForeColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.Text);
				}
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		public new Color BackColor
		{
			get
			{
				if (IsThemedLook && !ShouldSerializeBackColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.InputBack);
				}
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override bool AutoScroll
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new Size AutoScrollMargin
		{
			get
			{
				return new Size(0, 0);
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new Size AutoScrollMinSize
		{
			get
			{
				return new Size(0, 0);
			}
			set
			{
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Point AutoScrollPosition
		{
			get
			{
				return default(Point);
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new DockPaddingEdges DockPadding
		{
			get
			{
				return null;
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new AutoScaleMode AutoScaleMode
		{
			get
			{
				return AutoScaleMode.None;
			}
			set
			{
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new SizeF AutoScaleDimensions
		{
			get
			{
				return default(SizeF);
			}
			set
			{
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Padding Padding
		{
			get
			{
				return new Padding(0);
			}
			set
			{
			}
		}

		[Description("Specifies whether text rendering should be compatible with previous releases of Windows Forms.")]
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
					OnUseCompatibleTextRenderingChanged();
				}
			}
		}

		public event EventHandler ColorSchemeChanged
		{
			add
			{
				base.Events.AddHandler(_scrollableBaseValue, value);
			}
			remove
			{
				base.Events.RemoveHandler(_scrollableBaseValue, value);
			}
		}
	}
}
