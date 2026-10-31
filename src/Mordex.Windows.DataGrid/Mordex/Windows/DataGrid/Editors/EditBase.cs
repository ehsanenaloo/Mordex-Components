using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	[Designer("Mordex.Windows.DataGrid.Design.EditBaseDesigner, Mordex.Windows.DataGrid.Design")]
	public partial class EditBase : ContainerControl, IGridStyleSource, ITextBoxOwner, IRibbonEditControl
	{
		private const int ClipboardRetryCount = 5;

		private const int ClipboardRetryDelay = 100;

		private EditButtonType _buttonType;

		private GridLayoutState[] _buttonStates;

		private Rectangle _contentBounds;

		private ControlStyle _officeStyle;

		private TextBox _textBox;

		private System.Windows.Forms.Timer _timer;

		private bool _treatControlAreaAsButton;

		internal bool _isDropDownOpen;



		private bool _isMouseOver;

		private bool _hoverRaised;

		private bool _allowDrop;

		private int _pressedButtonIndex;

		private int _hotButtonIndex;

		private HoverMode _hoverMode;

		private BorderStyle _borderStyle;

		private Rectangle _textArea;

		private Rectangle _imageArea;

		private bool _autoSize;

		private bool _isDragOver;

		private Image _displayImage;

		private Image _editBaseImage;

		private int _imageIndex;

		private string _imageKey;

		private ImageList _imageList;

		private bool _hasImage;

		private bool _isTextPresent;

		private ImageHorizontalAlignment _imageHorizontalAlignment;

		private ImageVerticalAlignment _imageVerticalAlignment;

		private TextAlignment _textAlignment;

		private bool _isTextAlignmentDirty;

		private bool _readOnly;

		private EditButtonStyle _buttonStyle;

		private Image _buttonImage;

		private string _buttonCaption;

		private Font _buttonFont;

		private Size _buttonImageSize;

		private int _buttonImageIndex;

		private string _buttonImageKey;

		private static readonly object _readOnlyChangedEventKey = new object();

		private static readonly object _editBaseValue = new object();

		private static readonly object _imageListChangedKey = new object();

		private bool _isHighlightColorUsed;

		private bool _useDefaultMenu;

		private int _repeatDirection;

		private bool _repeatHadCapture;

		private CharacterCasing _characterCasing;

		private bool _hideSelection;

		private int _maxLength;

		private bool _modified;

		private bool _buttonEnabled;

		private Color _disabledBackground;

		private Color _disabledForeground;

		private ContextMenuStrip _contextMenuStrip;

		private bool _shortcutsEnabled;

		private bool? _useCompatibleTextRendering;



		private bool _actAsRibbonEditControl;

		public EditBase()
		{
			ApplyDefaultSettings();
			_buttonStates = new GridLayoutState[2];
			InitializeDefaults();
			SetStyle(ControlStyles.StandardDoubleClick, value: false);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.CacheText, value: true);
		}
	}
}
