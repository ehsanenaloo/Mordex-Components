using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Controls
{
	[ToolboxItem(true)]
	[Designer("Mordex.Windows.UI.Design.ContainerDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public partial class MxGroupBox : ScrollableControl, IFormatStyleOwner, ISupportInitialize
	{
		private const int DefaultWidth = 200;

		private const int DefaultHeight = 100;

		private const int DefaultTextOffset = 4;

		private const int DefaultTextInset = 8;

		private const int DefaultImageEdge = 16;

		private const int FrameTopGap = 6;

		private const int FrameSideMargin = 3;

		private const int FrameTopPadding = 2;

		private const int FrameBottomPadding = 3;

		private const int WmHScroll = 0x114;

		private const int WmVScroll = 0x115;

		private readonly Container _container = new Container();

		private bool _isLayoutDirty = true;

		private Rectangle _imageBounds = Rectangle.Empty;

		private Rectangle _textBounds = Rectangle.Empty;

		private Rectangle _captionBounds = Rectangle.Empty;

		private bool _isInitializing;

		private bool? _useCompatibleTextRendering;

		private BackgroundStyle _backgroundStyle;

		private FrameStyle _frameStyle = FrameStyle.Frame;

		private Icon _icon;

		private Image _displayImage;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private ImageList _imageList;

		private Size _imageSize = new Size(DefaultImageEdge, DefaultImageEdge);

		private ImageHorizontalAlignment _imageAlignment = ImageHorizontalAlignment.BeforeText;

		private TextAlignment _textAlignment = TextAlignment.Near;

		private FormatStyle _formatStyle;

		private Color _borderColor;

		private FormatStyle _disabledFormatStyle;

		private int _textOffset = DefaultTextOffset;

		public MxGroupBox()
		{
			SetDefaultSize();
			SetStyle(ControlStyles.ContainerControl, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			_formatStyle = new FormatStyle(this);
			_disabledFormatStyle = new FormatStyle(this);
			TabStop = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		private void SetDefaultSize()
		{
			base.Size = new Size(DefaultWidth, DefaultHeight);
		}

		void IFormatStyleOwner.PropertyChange()
		{
			InvalidateLayout();
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_isInitializing = false;
			}
		}

		internal Color GetEffectiveBackColor()
		{
			if (BackColor == SystemColors.Control && (!VisualStyleSupport.IsThemingUsable(false)))
			{
				return RenderingHelper.GetSchemeColor(ColorName.Control);
			}
			return BackColor;
		}

		internal Color GetEffectiveForeColor()
		{
			return base.ForeColor;
		}
	}
}
