using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	[ToolboxItem(false)]
	[Designer("Mordex.Windows.UI.Controls.Design.BaseEditControlDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public partial class MxComboBase : ContainerControl, IDropListOwner, IBaseLineControl, IWindowsElement, IFormatStyleOwner, ITextBoxOwner, IRibbonEditControl
	{
		private const int DefaultWidth = 176;

		private const int DefaultHeight = 20;

		private const int DefaultIndentWidth = 20;

		private const int DefaultMaxDropDownItems = 8;

		private const int ClassicButtonWidth = 13;

		private const int FontHeightPadding = 2;

		internal const int PageStepWithSelection = 7;

		internal const int PageStepWithoutSelection = 8;

		private const int DropDownButtonChildId = 1;

		private const int DisabledArrowSize = 11;

		internal TextBox _textBox;

		internal ComboDropDownList _formatStyleRenderer;

		internal Rectangle _textBounds = Rectangle.Empty;

		private Rectangle _imageBounds = Rectangle.Empty;

		internal WindowsAppearanceState _windowsAppearanceState;

		internal bool _shouldFocusTextBox = true;

		internal WindowsLayoutState _windowsLayoutState;

		private bool _isWindows7OrLater;

		private bool _useDefaultMenu = true;

		private bool _actAsRibbonEditControl;

		private ContextMenuStrip _contextMenu;

		private bool _isDisposing;

		private Font _dropListFont;

		private bool _readOnly;

		private Color _flatBorderColor = Color.Empty;

		private bool _isDataSourceEmpty = true;

		private int _indentWidth = DefaultIndentWidth;

		private bool? _useCompatibleTextRendering;

		private ComboBoxThemeRenderer _comboThemeRenderer;

		private TextLayoutRenderer _textLayoutRenderer;

		private StringFormat _stringFormat;

		private bool _autoSize = true;

		private int _baselineOffset;

		private Mordex.Windows.UI.BorderStyle _borderStyle = Mordex.Windows.UI.BorderStyle.SunkenLight3D;

		private HoverMode _hoverMode = HoverMode.None;

		private StateStyles _stateStyles;

		private VisualStylePainter _themeRenderer;

		private bool _isMouseOver;

		private ComboTextAccessibleObject _textAccessibleObject;

		private ComboDropDownButtonAccessibleObject _dropDownButtonAccessibleObject;

		private bool _isButtonMouseDown;

		private bool _isFocusingTextBox;

		private Form _form;

		private bool _droppedDown;

		private EditStyle _editStyle = EditStyle.Combo;

		private ComboStyle _comboStyle;

		private ImageHorizontalAlignment _imageHorizontalAlignment = ImageHorizontalAlignment.BeforeText;

		private ImageVerticalAlignment _imageVerticalAlignment = ImageVerticalAlignment.Center;

		private TextAlignment _textAlignment = TextAlignment.Near;

		private CharacterCasing _characterCasing;

		private bool _allowDrop;

		private bool _isLayoutDirty = true;

		internal Image _itemImage;

		private int _pendingKeyPressCount;

		private bool _hasImage = true;

		private bool _hasText = true;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private ImageList _imageList;

		private FormatStyle _itemsFormatStyle;

		private FormatStyle _selectedItemFormatStyle;

		private bool _includeLiterals = true;

		private int _maxDropDownItems = DefaultMaxDropDownItems;

		private int _maxDropDownWidth = -1;

		private ControlAppearance _controlAppearance;

		private DropDownAnimation _dropDownAnimation = DropDownAnimation.None;

		private static readonly object _readOnlyChangedKey = new object();

		private static readonly object _droppingDownKey = new object();

		private static readonly object _closedKey = new object();

		private EventHandler _dropDownHandler;

		public MxComboBase()
		{
			InitializeControlAppearance();
			_isWindows7OrLater = RenderingHelper.IsWindows7();
			UpdateTextBoxForEditStyle();
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			_formatStyleRenderer = new ComboDropDownList();
			_formatStyleRenderer.SetIndentWidth(_indentWidth);
			_formatStyleRenderer.CreateControl();
			_windowsAppearanceState = new WindowsAppearanceState();
			_windowsAppearanceState.ApplyVisualStyle();
			LayoutContent(null);
		}

		void IWindowsElement.OnControlStyleChanged()
		{
			OnControlStyleChanged();
			Invalidate();
		}

		void IFormatStyleOwner.PropertyChange()
		{
			InvalidateLayout();
		}

		internal ContextMenuStrip GetContextMenu()
		{
			if (_contextMenu == null)
			{
				_contextMenu = new ContextMenuStrip();
			}
			return _contextMenu;
		}

		internal Rectangle GetButtonBounds()
		{
			return _windowsLayoutState._buttonBounds;
		}

		protected override void Dispose(bool disposing)
		{
			_isDisposing = true;
			if (disposing)
			{
				if (_textBox != null)
				{
					_editStyle = EditStyle.DropDownList;
					UpdateTextBoxForEditStyle();
				}
				if (_formatStyleRenderer != null)
				{
					_formatStyleRenderer.Dispose();
					_formatStyleRenderer = null;
				}
			}
			base.Dispose(disposing);
		}

		internal bool IsDataSourceEmpty()
		{
			return _isDataSourceEmpty;
		}

		internal void SetDataSourceEmpty(bool isEmpty)
		{
			if (_isDataSourceEmpty != isEmpty)
			{
				_isDataSourceEmpty = isEmpty;
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new ComboBaseAccessibleObject(this);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (_textBox != null && !UsesNativeTheme())
			{
				_textBox.Visible = base.Enabled && HasText;
			}
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			_windowsAppearanceState.ApplyVisualStyle();
			Invalidate();
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
			if (_stringFormat != null)
			{
				_stringFormat.Dispose();
				_stringFormat = null;
			}
			if (_textLayoutRenderer != null)
			{
				_textLayoutRenderer.ResetCache();
				_textLayoutRenderer = null;
			}
			InvalidateLayout();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			if (_textBox != null && !base.Disposing && !base.IsDisposed && base.Parent != null)
			{
				try
				{
					_textBox.Font = Font;
				}
				catch (Exception ex) when (ex is ArgumentException || ex is ObjectDisposedException || ex is InvalidOperationException)
				{
					// The inner text box keeps its old font; the combo still works.
				}
			}
			if (_autoSize)
			{
				InvalidateLayout();
			}
		}

		protected override void OnForeColorChanged(EventArgs e)
		{
			if (_textBox != null)
			{
				_textBox.ForeColor = GetEffectiveForeColor();
			}
			base.OnForeColorChanged(e);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnCancelChanges(EventArgs e)
		{
			if (_droppedDown)
			{
				OnDropHide(cancel: true);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnAcceptChanges(EventArgs e)
		{
			if (_droppedDown)
			{
				OnDropHide(cancel: false);
			}
		}

		internal DropDownAnimation GetDropDownAnimation()
		{
			return _dropDownAnimation;
		}

		protected virtual void OnControlStyleChanged()
		{
			if (_formatStyleRenderer != null)
			{
				_formatStyleRenderer.SetAppearance(ControlAppearance);
			}
		}
	}
}
