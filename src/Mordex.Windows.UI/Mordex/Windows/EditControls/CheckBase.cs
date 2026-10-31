using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	[Designer("Mordex.Windows.UI.Controls.Design.BaseEditControlDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(false)]
	public partial class CheckBase : Control, IFormatStyleOwner, IBaseLineControl
	{
		private const double ReferenceDpi = 96.0;

		private const double CheckGlyphSizeAtReferenceDpi = 13.0;

		private const int DefaultImageSide = 16;

		private const int GlyphTextGap = 2;

		private const int TextExtraWidth = 8;

		private const int ImageGap = 4;

		private const int FocusRectInflateInButton = 3;

		private static readonly Size DefaultControlSize = new Size(104, 23);

		private Container _container = new Container();

		internal bool _isLayoutDirty = true;

		internal Rectangle _glyphBounds = Rectangle.Empty;

		internal Rectangle _imageBounds = Rectangle.Empty;

		internal Rectangle _textBounds = Rectangle.Empty;

		internal VisualStylePainter _themeRenderer;

		internal WindowsAppearanceState _windowsAppearanceState;

		private Size _preferredSize = Size.Empty;

		private System.Windows.Forms.Appearance _appearance;

		private bool _autoCheck = true;

		private bool _autoSize;

		private Image _image;

		private ContentAlignment _checkAlign = ContentAlignment.MiddleLeft;

		private ImageAlignment _imageVerticalAlignment;

		private TextAlignment _textVerticalAlignment = TextAlignment.Center;

		private Icon _icon;

		private ImageList _imageList;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private Size _imageSize = new Size(DefaultImageSide, DefaultImageSide);

		private string _toolTipText = "";

		private bool _showFocusRectangle = true;

		private ImageHorizontalAlignment _imageAlignment = ImageHorizontalAlignment.BeforeText;

		private bool? _useCompatibleTextRendering;

		private TextAlignment _textAlignment = TextAlignment.Near;

		private FormatStyle _formatStyle;

		private FormatStyle _disabledFormatStyle;

		private bool _isHot;

		private bool _isPressed;

		private WindowMessageHandler _customToolTip;

		private ToolTip _toolTip;

		private int _textBaseline;

		private bool _wordWrap = true;

		public CheckBase()
		{
			InitializeContainer();
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			_formatStyle = new FormatStyle(this);
			_disabledFormatStyle = new FormatStyle(this);
			_windowsAppearanceState = new WindowsAppearanceState();
			_windowsAppearanceState.ApplyVisualStyle();
		}

		void IFormatStyleOwner.PropertyChange()
		{
			InvalidateLayout();
		}

		private void InitializeContainer()
		{
			_container = new Container();
			base.Size = DefaultControlSize;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			DisposeResources();
			base.Dispose(disposing);
		}

		// The style of one paint: the built defaults, then FormatStyle, then DisabledFormatStyle when the control is disabled. The
		// caller disposes the result.
		private protected FormatStyle MergePaintStyle(FormatStyle built)
		{
			FormatStyle style = MergeOwned(built, _formatStyle);
			if (!base.Enabled)
			{
				style = MergeOwned(style, _disabledFormatStyle);
			}
			return style;
		}

		// Merges and releases the resources of the replaced intermediate style.
		private static FormatStyle MergeOwned(FormatStyle current, FormatStyle overrideStyle)
		{
			FormatStyle merged = current.MergeStyle(overrideStyle);
			if (!ReferenceEquals(merged, current))
			{
				current.DisposeResources();
			}
			return merged;
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			// The disabled style can change the font, so the text is measured again.
			InvalidateLayout();
		}

		internal Color GetEffectiveForeColor()
		{
			return base.ForeColor;
		}

		internal Color GetEffectiveBackColor()
		{
			if (BackColor == SystemColors.Control && (!VisualStyleSupport.IsThemingUsable(false)))
			{
				return RenderingHelper.GetSchemeColor(ColorName.Control);
			}
			return BackColor;
		}

		internal bool GetIsHot()
		{
			return _isHot;
		}

		internal void SetIsHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				if (_isHot)
				{
					UpdateToolTip(_toolTipText);
				}
				else
				{
					UpdateToolTip("");
				}
				Invalidate();
			}
		}

		internal bool GetIsPressed()
		{
			return _isPressed;
		}

		internal void SetIsPressed(bool isPressed)
		{
			if (_isPressed != isPressed)
			{
				_isPressed = isPressed;
				Invalidate();
			}
		}

		internal void DisposeResources()
		{
			if (_customToolTip != null)
			{
				_customToolTip.HideAndDisposeTimers();
				_customToolTip = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
		}

		private void UpdateToolTip(string caption)
		{
			if (caption == null || caption.Length == 0)
			{
				DisposeResources();
				return;
			}
			if (VisualStyleSupport.IsToolTipHookSupported())
			{
				if (_customToolTip == null)
				{
					_customToolTip = new WindowMessageHandler(Font, UseCompatibleTextRendering);
					_customToolTip.RequestShow(caption, this);
				}
				else
				{
					_customToolTip.UpdateTipText(caption);
				}
				return;
			}
			if (_toolTip == null)
			{
				_toolTip = new Mordex.Windows.Common.ThemedToolTip();
				_toolTip.ShowAlways = true;
			}
			_toolTip.SetToolTip(this, caption);
		}
	}
}
