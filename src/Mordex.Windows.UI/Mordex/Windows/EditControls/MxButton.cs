using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	[Designer("Mordex.Windows.UI.Controls.Design.BaseEditControlDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public partial class MxButton : Control, IFormatStyleOwner, IButtonControl, IBaseLineControl
	{
		private Container _container = new Container();

		internal WindowsAppearanceState _windowsAppearanceState;

		private Rectangle _faceBounds = Rectangle.Empty;

		private Rectangle _dropDownBounds = Rectangle.Empty;

		private Rectangle _textBounds = Rectangle.Empty;

		internal Rectangle _imageBounds = Rectangle.Empty;

		private Size _preferredSize = Size.Empty;

		// The font that the current layout measured with.
		private (string Name, float Size, FontStyle Style, GraphicsUnit Unit) _layoutFontKey;

		private DialogResult _dialogResult;

		private bool _isDefaultButton;

		private Color _backColor;

		private bool _autoSize;

		private bool _wordWrap = true;

		private Image _buttonImage;

		private Icon _icon;

		private Size _imageSize = DefaultImageSize;

		private ButtonStyle _buttonStyle;

		private ImageList _imageList;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private int _hotImageIndex = -1;

		private string _hotImageKey = "";

		private int _disabledImageIndex = -1;

		private string _disabledImageKey = "";

		private int _pressedImageIndex = -1;

		private string _pressedImageKey = "";

		private TextAlignment _textHorizontalAlignment = TextAlignment.Center;

		private TextAlignment _textVerticalAlignment = TextAlignment.Center;

		private ImageHorizontalAlignment _imageHorizontalAlignment = ImageHorizontalAlignment.BeforeText;

		private ImageVerticalAlignment _imageVerticalAlignment = ImageVerticalAlignment.Center;

		private Mordex.Windows.UI.Appearance _appearance = Mordex.Windows.UI.Appearance.Light3D;

		private string _toolTipText = "";

		private bool? _useCompatibleTextRendering;

		private bool _useMnemonic = true;

		private FormatStyle _activeFormatStyle;

		private StateStyles _stateStyles;

		private Color _flatBorderColor = Color.Empty;

		private bool _highlightActiveButton = true;

		private bool _showFocusRectangle = true;

		private string _associatedCommandKey = "";

		private AccessibleObject _accessibleObject;

		private MxContextMenu _dropDownContextMenu;

		private bool _isHot;

		private bool _isPressed;

		private bool _isDropDownOpen;

		private bool _isDropDownPressed;

		private bool _isDropDownHot;

		private Rectangle _contentBounds = Rectangle.Empty;

		private bool _isLayoutDirty = true;

		private WindowMessageHandler _customToolTip;

		private ToolTip _toolTip;

		private bool _isRightClickPending;

private const int DefaultWidth = 75;

private const int DefaultHeight = 23;

private static readonly Size DefaultImageSize = new Size(16, 16);

private const int BaselineOffsetFromBottom = 4;

private const int MouseLeaveMessage = 0x2A3;

private const int PreferredHeightPadding = 10;

private const int PreferredWidthPadding = 8;

private const int ImageTextSpacing = 4;

private const int EllipsisDotSize = 2;

private const int EllipsisTextWidth = 8;

private const int EllipsisNearOffset = 2;

		public MxButton()
		{
			ApplyDefaultSize();
			_activeFormatStyle = new FormatStyle(this);
			_stateStyles = new StateStyles(this);
			_windowsAppearanceState = new WindowsAppearanceState();
			_windowsAppearanceState.ApplyVisualStyle();
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.StandardClick, value: true);
			SetStyle(ControlStyles.StandardDoubleClick, value: false);
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

		public void NotifyDefault(bool value)
		{
			_isDefaultButton = value;
			Invalidate();
		}

		public void PerformClick()
		{
			if (!base.Enabled)
			{
				return;
			}
			for (Control control = RenderingHelper.GetParentControl(this); control != null; control = RenderingHelper.GetParentControl(control))
			{
				if (control is ContainerControl)
				{
					ContainerControl containerControl = (ContainerControl)control;
					if (containerControl.Validate())
					{
						break;
					}
					return;
				}
			}
			if (ResolveButtonStyle() == ButtonStyle.DropDown && _dropDownContextMenu != null)
			{
				DropDown();
			}
			SetPressed(true);
			try
			{
				OnClick(EventArgs.Empty);
			}
			finally
			{
				SetPressed(false);
			}
		}

		private void ApplyDefaultSize()
		{
			base.Size = new Size(DefaultWidth, DefaultHeight);
		}

		void IFormatStyleOwner.PropertyChange()
		{
			InvalidateLayoutAndPaint();
		}

		internal AccessibleStates GetAccessibleStates()
		{
			AccessibleStates accessibleStates = AccessibleStates.Focusable;
			if (Focused)
			{
				accessibleStates |= AccessibleStates.Focused;
			}
			if (IsPressed())
			{
				accessibleStates |= AccessibleStates.Pressed;
			}
			return accessibleStates;
		}

		internal MxCommand GetAssociatedCommand()
		{
			if (_associatedCommandKey.Length > 0 && DropDownContextMenu != null)
			{
				return DropDownContextMenu.Commands.FindByKey(_associatedCommandKey);
			}
			return null;
		}

		internal AccessibleObject GetOrCreateAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new ButtonAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal Rectangle GetDropDownBounds()
		{
			return _dropDownBounds;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			InvalidateLayoutAndPaint();
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

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			_windowsAppearanceState.ApplyVisualStyle();
			Invalidate();
		}
	}
}
