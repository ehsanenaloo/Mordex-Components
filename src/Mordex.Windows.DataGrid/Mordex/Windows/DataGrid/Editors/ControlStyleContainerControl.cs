using System;
using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	internal class ControlStyleContainerControl : ContainerControl, IGridDropListOwner
	{
		internal int _virtualWidth;

		internal TextBox _textBox;

		private ImageHorizontalAlignment _imageHorizontalAlignment;

		private ImageVerticalAlignment _imageVerticalAlignment;

		private TextLayoutRenderer _textLayoutRenderer;

		private Point _imageLocation;

		private Rectangle _textBounds;

		private bool _isForwardingFocus;

		private bool _isDroppedDown;

		private EditStyle _editStyle = EditStyle.Combo;

		private bool _isMultiline;

		private TextAlignment _textAlignment;

		private CharacterCasing _characterCasing;

		protected Image _displayImage;

		private EventHandler _changed;

		private bool _isImageSet;

		private bool _useInnerEditor = true;

		private int _contentMargin;

		private int _marginRight;

		private int _marginTop;

		private int _marginBottom;

		private TextAlignment _controlStyleContainerControlTextAlignment;

		private ImageList _imageList;

		private bool _isNumericFormat;

		private string _editFormat = "";

		private char _character = '_';

		private bool _includeLiterals = true;

		private ControlStyle _appearanceStyle;

		private EventHandler _dropped;

		private GridEventCallback _dropDownHideCallback;

		internal GridFormatStyle _gridFormatStyle;

		private char _controlStyleContainerControlCharacter;

		AccessibleObject IGridDropListOwner.AccessibleObject => base.AccessibilityObject;

		public override bool Focused => base.ContainsFocus;

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				if (value != Text)
				{
					if (_textBox != null && _textBox.Text != value)
					{
						_textBox.Text = value;
						SelectAllText();
					}
					else
					{
						base.Text = value;
						Invalidate();
					}
				}
			}
		}

		internal ControlStyleContainerControl()
		{
			DisposeResources();
			InitializeDefaults();
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			LayoutContent();
			base.CausesValidation = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SetIsDroppedDown(false);
				if (_textBox != null)
				{
					_editStyle = EditStyle.DropDownList;
					DisposeResources();
				}
			}
			base.Dispose(disposing);
		}

		private void InitializeDefaults()
		{
			base.Size = new Size(176, 15);
			ForeColor = SystemColors.WindowText;
			BackColor = SystemColors.Window;
		}

		protected override void OnResize(EventArgs re)
		{
			base.OnResize(re);
			LayoutContent();
		}

		public void UpdateImageHorizontalAlignment(ImageHorizontalAlignment imageHorizontalAlignment)
		{
			if (_imageHorizontalAlignment != imageHorizontalAlignment)
			{
				_imageHorizontalAlignment = imageHorizontalAlignment;
				LayoutContent();
			}
		}

		public void UpdateImageVerticalAlignment(ImageVerticalAlignment imageVerticalAlignment)
		{
			if (_imageVerticalAlignment != imageVerticalAlignment)
			{
				_imageVerticalAlignment = imageVerticalAlignment;
				LayoutContent();
			}
		}

		internal TextLayoutRenderer GetTextLayoutRenderer()
		{
			return _textLayoutRenderer;
		}

		internal void UpdateTextLayoutRenderer(TextLayoutRenderer textLayoutRenderer)
		{
			if (_textLayoutRenderer != textLayoutRenderer)
			{
				_textLayoutRenderer = textLayoutRenderer;
				LayoutContent();
			}
		}

		private void LayoutContent()
		{
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			Rectangle clientRectangle = base.ClientRectangle;
			if (_displayImage != null)
			{
				Graphics graphics = CreateGraphics();
				_imageLocation.X = GridHelper.CalculateImageLeft(graphics, _displayImage.Size, ref clientRectangle, _imageHorizontalAlignment, Text, _textLayoutRenderer, Font, RightToLeft == RightToLeft.Yes);
				_imageLocation.Y = GridHelper.RenderContent(graphics, _displayImage.Size, ref clientRectangle, _imageVerticalAlignment, Text, _textLayoutRenderer, Font, RightToLeft == RightToLeft.Yes);
				graphics.Dispose();
			}
			else
			{
				_imageLocation = new Point(0, 0);
			}
			_textBounds = clientRectangle;
			if (_textBox != null)
			{
				clientRectangle.X += 2;
				clientRectangle.Width -= 2;
				int width = clientRectangle.Width;
				int height = clientRectangle.Height;
				if (height > 0 && width > 0)
				{
					_textBox.WordWrap = _isMultiline;
					_textBox.Multiline = _isMultiline;
					_textBox.AutoSize = false;
					if (!_isMultiline)
					{
						height = _textBox.Font.Height + 1;
					}
					_textBox.Size = new Size(width, height + 2);
					if (_controlStyleContainerControlTextAlignment == TextAlignment.Near)
					{
						_textBox.Location = new Point(clientRectangle.Left, clientRectangle.Top);
					}
					else if (_controlStyleContainerControlTextAlignment == TextAlignment.Far)
					{
						_textBox.Location = new Point(clientRectangle.Left, clientRectangle.Bottom - _textBox.Height);
					}
					else
					{
						_textBox.Location = new Point(clientRectangle.Left, clientRectangle.Top + (clientRectangle.Height - height) / 2);
					}
					_textBox.Visible = true;
				}
				else
				{
					_textBox.Width = 0;
				}
			}
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			Brush brush = GdiCache.Brush(BackColor);
			pe.Graphics.FillRectangle(brush, base.ClientRectangle);
			brush = null;
			OnPaintText(pe.Graphics);
		}

		protected override void OnMouseDown(MouseEventArgs me)
		{
			base.OnMouseDown(me);
			if (!base.ContainsFocus)
			{
				GridHelper.SelectAndFocus(this);
			}
		}

		protected override void OnGotFocus(EventArgs eventArgs)
		{
			base.OnGotFocus(eventArgs);
			if (_textBox != null)
			{
				_isForwardingFocus = true;
				GridHelper.SelectAndFocus(_textBox);
				_isForwardingFocus = false;
			}
		}

		internal void PressDropDownButton()
		{
			GridHelper.SetCapture(this, true);
			OnScrollMouseDown();
		}

		protected virtual void OnScrollMouseDown()
		{
			DropListControl(setCapture: false);
		}

		protected virtual void DropListControl(bool setCapture)
		{
			OnDrop(EventArgs.Empty);
			_isDroppedDown = true;
			if (_textBox is MaskEdit)
			{
				((MaskEdit)_textBox)._isDropDownShown = true;
			}
		}

		public bool GetIsDroppedDown()
		{
			return _isDroppedDown;
		}

		public void SetIsDroppedDown(bool isDroppedDown)
		{
			if ((!isDroppedDown || (_editStyle != EditStyle.Text && _editStyle != EditStyle.None)) && isDroppedDown != _isDroppedDown)
			{
				_isDroppedDown = isDroppedDown;
				if (isDroppedDown)
				{
					DropListControl(setCapture: true);
				}
				else
				{
					OnDropHide(cancel: true);
				}
			}
		}

		protected internal virtual Control GetDropControl()
		{
			return null;
		}

		void IGridDropListOwner.OnDropListHide(bool cancel)
		{
			OnDropHide(cancel);
		}

		protected override void OnLostFocus(EventArgs eventArgs)
		{
			base.OnLostFocus(eventArgs);
		}

		void IGridDropListOwner.OnComputeRows()
		{
		}

		protected virtual void OnDropHide(bool cancel)
		{
			_isDroppedDown = false;
			OnDropDownHide(new GridEventArgs(!cancel));
			if (_textBox is MaskEdit)
			{
				((MaskEdit)_textBox)._isDropDownShown = false;
			}
		}

		public EditStyle GetEditStyle()
		{
			return _editStyle;
		}

		public void SetEditStyle(EditStyle editStyle)
		{
			if (!Enum.IsDefined(typeof(EditStyle), editStyle))
			{
				throw new InvalidEnumArgumentException();
			}
			if (editStyle != _editStyle)
			{
				_editStyle = editStyle;
				DisposeResources();
			}
		}

		public void SetMultiline(bool isMultiline)
		{
			if (isMultiline != _isMultiline)
			{
				_isMultiline = isMultiline;
				if (_textBox != null)
				{
					_textBox.WordWrap = _isMultiline;
					_textBox.Multiline = _isMultiline;
					LayoutContent();
				}
				else
				{
					Invalidate();
				}
			}
		}

		public void UpdateTextAlignment(TextAlignment textAlignment)
		{
			if (!Enum.IsDefined(typeof(TextAlignment), textAlignment))
			{
				throw new ArgumentException();
			}
			if (textAlignment != _textAlignment)
			{
				_textAlignment = textAlignment;
				ApplyTextAlignment();
			}
		}

		private void ApplyTextAlignment()
		{
			if (_textBox != null)
			{
				switch (_textAlignment)
				{
				case TextAlignment.Center:
					_textBox.TextAlign = HorizontalAlignment.Center;
					break;
				case TextAlignment.Far:
					_textBox.TextAlign = HorizontalAlignment.Right;
					break;
				default:
					_textBox.TextAlign = HorizontalAlignment.Left;
					break;
				}
			}
		}

		private void CreateTextBox()
		{
			if (_controlStyleContainerControlCharacter != 0)
			{
				_textBox = new MaskEdit(checkLicense: false);
			}
			else
			{
				_textBox = new GridMaskEdit();
			}
		}

		public void UpdateCharacterCasing(CharacterCasing characterCasing)
		{
			if (_characterCasing != characterCasing)
			{
				_characterCasing = characterCasing;
				if (_textBox != null)
				{
					_textBox.CharacterCasing = characterCasing;
				}
			}
		}

		private void DisposeResources()
		{
			int value = 0;
			int otherValue = 0;
			if (_textBox is MaskEdit)
			{
				value = 1;
			}
			else if (_textBox is NumericEdit)
			{
				value = 2;
			}
			if (_editStyle != EditStyle.DropDownList && _editStyle != EditStyle.None && _useInnerEditor)
			{
				otherValue = ((!_isNumericFormat || (_editStyle != EditStyle.Text && _editStyle != EditStyle.Combo)) ? 1 : 2);
			}
			if (otherValue != value && _textBox != null)
			{
				_textBox.AccessibleName = "InnerEdit";
				switch (value)
				{
				case 1:
					((MaskEdit)_textBox).RemoveKeyPressedHandler(HandleKeyPressed);
					((MaskEdit)_textBox).RemoveEscapeKeyHandler(HandleEscapeKey);
					break;
				case 2:
					((NumericEdit)_textBox).ValueChanged -= OnTextBoxValueChanged;
					break;
				}
				_textBox.TextChanged -= OnTextBoxTextChanged;
				_textBox.KeyDown -= OnTextBoxKeyDown;
				_textBox.KeyPress -= OnTextBoxKeyPress;
				_textBox.KeyUp -= OnTextBoxKeyUp;
				_textBox.GotFocus -= OnTextBoxGotFocus;
				_textBox.LostFocus -= OnTextBoxLostFocus;
				_textBox.MouseWheel -= OnTextBoxMouseWheel;
				_textBox.MouseDown -= OnTextBoxMouseDown;
				_textBox.MouseMove -= OnTextBoxMouseMove;
				_textBox.MouseUp -= OnTextBoxMouseUp;
				_textBox.MouseClick -= OnTextBoxMouseClick;
				_textBox.MouseDoubleClick -= OnTextBoxMouseDoubleClick;
				_textBox.Click -= OnTextBoxClick;
				_textBox.DoubleClick -= OnTextBoxDoubleClick;
				base.Controls.Remove(_textBox);
				_textBox.Dispose();
				_textBox = null;
			}
			if (otherValue != value)
			{
				switch (otherValue)
				{
				case 1:
					CreateTextBox();
					((MaskEdit)_textBox).Mask = _editFormat;
					((MaskEdit)_textBox).PromptChar = _character;
					((MaskEdit)_textBox).IncludeLiterals = _includeLiterals;
					((MaskEdit)_textBox).AddKeyPressedHandler(HandleKeyPressed);
					((MaskEdit)_textBox).AddEscapeKeyHandler(HandleEscapeKey);
					break;
				case 2:
					_textBox = new NumericEdit(checkLicense: false);
					ApplyNumericFormat(_editFormat);
					((NumericEdit)_textBox).ValueChanged += OnTextBoxValueChanged;
					break;
				default:
					return;
				}
				_textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
				ApplyTextAlignment();
				_textBox.AccessibleName = "Editor_Text";
				_textBox.Font = new Font(Font, Font.Style);
				_textBox.ForeColor = ForeColor;
				_textBox.BackColor = BackColor;
				_textBox.PasswordChar = _controlStyleContainerControlCharacter;
				_textBox.CharacterCasing = _characterCasing;
				base.Controls.Add(_textBox);
				_textBox.TextChanged += OnTextBoxTextChanged;
				_textBox.KeyDown += OnTextBoxKeyDown;
				_textBox.KeyPress += OnTextBoxKeyPress;
				_textBox.KeyUp += OnTextBoxKeyUp;
				_textBox.GotFocus += OnTextBoxGotFocus;
				_textBox.LostFocus += OnTextBoxLostFocus;
				_textBox.MouseWheel += OnTextBoxMouseWheel;
				_textBox.MouseDown += OnTextBoxMouseDown;
				_textBox.MouseMove += OnTextBoxMouseMove;
				_textBox.MouseUp += OnTextBoxMouseUp;
				_textBox.MouseClick += OnTextBoxMouseClick;
				_textBox.MouseDoubleClick += OnTextBoxMouseDoubleClick;
				_textBox.Click += OnTextBoxClick;
				_textBox.DoubleClick += OnTextBoxDoubleClick;
			}
			LayoutContent();
		}

		private void OnTextBoxValueChanged(object sender, EventArgs eventArgs)
		{
			OnChange(eventArgs);
		}

		private void OnTextBoxGotFocus(object sender, EventArgs eventArgs)
		{
			if (!_isForwardingFocus)
			{
				base.OnGotFocus(eventArgs);
			}
		}

		private void OnTextBoxLostFocus(object sender, EventArgs eventArgs)
		{
			base.OnLostFocus(eventArgs);
		}

		protected override void OnBackColorChanged(EventArgs eventArgs)
		{
			base.OnBackColorChanged(eventArgs);
			if (_textBox != null)
			{
				_textBox.BackColor = BackColor;
			}
		}

		protected override void OnFontChanged(EventArgs eventArgs)
		{
			base.OnFontChanged(eventArgs);
			if (_textBox != null)
			{
				_textBox.Font = Font;
			}
			LayoutContent();
		}

		protected override void OnForeColorChanged(EventArgs eventArgs)
		{
			base.OnForeColorChanged(eventArgs);
			if (_textBox != null)
			{
				_textBox.ForeColor = ForeColor;
			}
		}

		protected virtual void OnEscapeKey(EventArgs eventArgs)
		{
			if (_isDroppedDown)
			{
				OnDropHide(cancel: true);
			}
		}

		private void HandleEscapeKey(object sender, EventArgs eventArgs)
		{
			OnEscapeKey(eventArgs);
		}

		private void HandleKeyPressed(object sender, EventArgs eventArgs)
		{
			OnKeyPressed(eventArgs);
		}

		private void OnTextBoxKeyPress(object sender, KeyPressEventArgs eventArgs)
		{
			base.OnKeyPress(eventArgs);
		}

		private void OnTextBoxKeyDown(object sender, KeyEventArgs eventArgs)
		{
			OnKeyDown(eventArgs);
		}

		private void OnTextBoxKeyUp(object sender, KeyEventArgs eventArgs)
		{
			OnKeyUp(eventArgs);
		}

		private void OnTextBoxMouseDown(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseDown(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnTextBoxMouseUp(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseUp(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnTextBoxMouseClick(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseClick(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnTextBoxMouseDoubleClick(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseDoubleClick(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnTextBoxMouseMove(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseMove(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnTextBoxClick(object sender, EventArgs eventArgs)
		{
			base.OnClick(eventArgs);
		}

		private void OnTextBoxDoubleClick(object sender, EventArgs eventArgs)
		{
			base.OnDoubleClick(eventArgs);
		}

		protected virtual void OnKeyPressed(EventArgs eventArgs)
		{
		}

		internal void ProcessCharacter(char keyChar)
		{
			if (_textBox != null)
			{
				if (_isNumericFormat)
				{
					((NumericEdit)_textBox).SetTextFromCharacter(keyChar);
				}
				else if (((MaskEdit)_textBox).Mask.Length == 0)
				{
					Text = keyChar.ToString(GridHelper.GetCurrentCulture());
					_textBox.SelectionStart = Text.Length;
					_textBox.ScrollToCaret();
					OnKeyPressed(EventArgs.Empty);
				}
				else
				{
					_textBox.Text = "";
					((MaskEdit)_textBox).HandleMaskedChar(keyChar);
					OnKeyPressed(EventArgs.Empty);
				}
			}
			else
			{
				InternalProcessChar(keyChar);
			}
		}

		protected virtual void InternalProcessChar(char keyChar)
		{
		}

		public void SetImage(Image image)
		{
			if (_displayImage != image)
			{
				_displayImage = image;
				if (_displayImage != null)
				{
					_isImageSet = true;
				}
				LayoutContent();
				Invalidate();
			}
		}

		private void OnTextBoxTextChanged(object sender, EventArgs eventArgs)
		{
			if (_textBox is MaskEdit)
			{
				MaskEdit maskEdit = (MaskEdit)_textBox;
				if (maskEdit.IncludeLiterals)
				{
					base.Text = maskEdit.GetValueText();
				}
				else
				{
					base.Text = maskEdit.UnmaskedText;
				}
			}
			else
			{
				base.Text = _textBox.Text;
			}
		}

		internal void AddChangeHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _changed;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _changed, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		internal void RemoveChangeHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _changed;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _changed, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected internal virtual void OnChange(EventArgs eventArgs)
		{
			if (_changed != null)
			{
				_changed(this, eventArgs);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.F4:
			case Keys.Down | Keys.Alt:
				if (GetEditStyle() != EditStyle.Text && _editStyle != EditStyle.None)
				{
					if (_isDroppedDown)
					{
						OnDropHide(cancel: false);
					}
					else
					{
						DropListControl(setCapture: true);
					}
					return true;
				}
				break;
			case Keys.Escape:
				if (_editStyle != EditStyle.Text && _isDroppedDown)
				{
					OnDropHide(cancel: false);
					return true;
				}
				break;
			}
			IMordexContainer mordexContainer = GridHelper.ComputeIMordexContainer(this);
			if (mordexContainer != null && mordexContainer.PreProcessDialogKey(keyData))
			{
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs eventArgs)
		{
			base.OnKeyDown(eventArgs);
			if (eventArgs.Handled)
			{
				return;
			}
			switch (eventArgs.KeyCode)
			{
			case Keys.F4:
				SetIsDroppedDown(!_isDroppedDown);
				break;
			case Keys.Escape:
				SetIsDroppedDown(false);
				break;
			case Keys.Return:
				if (GetIsDroppedDown())
				{
					OnDropHide(cancel: false);
				}
				break;
			}
		}

		public bool GetIsImageSet()
		{
			return _isImageSet;
		}

		public void SetIsImageSet(bool isImageSet)
		{
			if (isImageSet != _isImageSet)
			{
				_isImageSet = isImageSet;
				LayoutContent();
			}
		}

		public void SetUseInnerEditor(bool useInnerEditor)
		{
			if (useInnerEditor != _useInnerEditor)
			{
				_useInnerEditor = useInnerEditor;
				DisposeResources();
				LayoutContent();
			}
		}

		internal void SetMargins(int value, int marginTop, int controlStyleContainerControlInteger, int marginBottom)
		{
			_contentMargin = value;
			_marginTop = marginTop;
			_marginRight = controlStyleContainerControlInteger;
			_marginBottom = marginBottom;
			LayoutContent();
		}

		public void SetTextAlignment(TextAlignment controlStyleContainerControlTextAlignment)
		{
			if (controlStyleContainerControlTextAlignment != _controlStyleContainerControlTextAlignment)
			{
				_controlStyleContainerControlTextAlignment = controlStyleContainerControlTextAlignment;
				LayoutContent();
			}
		}

		public ImageList GetImageList()
		{
			return _imageList;
		}

		public void SetImageList(ImageList imageList)
		{
			_imageList = imageList;
		}

		protected virtual void OnPaintText(Graphics graphics)
		{
			if (_displayImage != null)
			{
				graphics.DrawImage(_displayImage, new Rectangle(_imageLocation.X, _imageLocation.Y, _displayImage.Width, _displayImage.Height));
			}
			if (_useInnerEditor && (GetEditStyle() == EditStyle.DropDownList || GetEditStyle() == EditStyle.None))
			{
				Rectangle rectangle = _textBounds;
				GetTextLayoutRenderer().DrawTextWithColor(graphics, Text, rectangle, Font, ForeColor);
			}
		}

		internal bool IsNumericFormat()
		{
			return _isNumericFormat;
		}

		internal void SetNumericFormat(bool isNumericFormat)
		{
			if (isNumericFormat != _isNumericFormat)
			{
				_isNumericFormat = isNumericFormat;
				DisposeResources();
			}
		}

		internal void ProcessType(Type otherType)
		{
			if (!(_textBox is NumericEdit))
			{
				return;
			}
			try
			{
				Type type = otherType;
				if (otherType.IsGenericType && otherType.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					Type[] genericArguments = otherType.GetGenericArguments();
					if (genericArguments.Length == 1)
					{
						type = genericArguments[0];
					}
				}
				TypeCode typeCode = Type.GetTypeCode(type);
				((NumericEdit)_textBox).ValueType = (NumericEditValueType)typeCode;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				((NumericEdit)_textBox).ValueType = NumericEditValueType.Double;
			}
		}

		private void ApplyNumericFormat(string format)
		{
			if (!(_textBox is NumericEdit))
			{
				return;
			}
			NumericEdit numericEdit = (NumericEdit)_textBox;
			numericEdit.EditMode = NumericEditMode.FormattedText;
			string upper = format.ToUpperInvariant();
			NumericEditFormatMask formatMask;
			string digits;
			if (upper.StartsWith("CURRENCY", StringComparison.Ordinal))
			{
				formatMask = NumericEditFormatMask.Currency;
				digits = upper.Substring("CURRENCY".Length);
			}
			else if (upper.StartsWith("PERCENT", StringComparison.Ordinal))
			{
				formatMask = NumericEditFormatMask.Percent;
				digits = upper.Substring("PERCENT".Length);
			}
			else if (upper.StartsWith("GENERAL", StringComparison.Ordinal))
			{
				formatMask = NumericEditFormatMask.General;
				digits = upper.Substring("GENERAL".Length);
			}
			else if (upper.StartsWith("NUMBER", StringComparison.Ordinal))
			{
				formatMask = NumericEditFormatMask.Number;
				digits = upper.Substring("NUMBER".Length);
			}
			else
			{
				return;
			}
			int decimalDigits = -1;
			if (digits.Length > 0 && !int.TryParse(digits, out decimalDigits))
			{
				return;
			}
			numericEdit.DecimalDigits = decimalDigits;
			numericEdit.FormatMask = formatMask;
			numericEdit.SetEmptyTextAsString(true);
			numericEdit._validateOnLeave = false;
		}

		public void SetEditFormat(string controlStyleContainerControlText)
		{
			_editFormat = controlStyleContainerControlText;
			SetNumericFormat(GridHelper.IsStandardNumberFormat(_editFormat));
			if (_textBox is NumericEdit)
			{
				ApplyNumericFormat(_editFormat);
			}
			else if (_textBox is MaskEdit)
			{
				((MaskEdit)_textBox).Mask = _editFormat;
			}
		}

		public void UpdateCharacter(char character)
		{
			if (character != _character)
			{
				_character = character;
				if (_textBox is MaskEdit)
				{
					((MaskEdit)_textBox).PromptChar = _character;
				}
			}
		}

		internal void SelectAllText()
		{
			if (_textBox is MaskEdit)
			{
				((MaskEdit)_textBox).ShowFullMaskAndSelect();
			}
			else if (_textBox != null)
			{
				_textBox.SelectAll();
			}
		}

		internal void SelectRange(int otherStart, int nextStart)
		{
			if (_textBox is MaskEdit)
			{
				((MaskEdit)_textBox).SelectRange(otherStart, nextStart);
			}
			else if (_textBox != null)
			{
				int start;
				int length;
				if (otherStart < nextStart)
				{
					start = otherStart;
					length = nextStart - otherStart;
				}
				else
				{
					start = nextStart;
					length = otherStart - nextStart;
				}
				_textBox.Select(start, length);
			}
		}

		public bool GetIncludeLiterals()
		{
			return _includeLiterals;
		}

		public void SetIncludeLiterals(bool includeLiterals)
		{
			if (_includeLiterals != includeLiterals)
			{
				_includeLiterals = includeLiterals;
				if (_textBox is MaskEdit)
				{
					((MaskEdit)_textBox).IncludeLiterals = _includeLiterals;
				}
			}
		}

		public int GetMaxLength()
		{
			if (_textBox == null)
			{
				return 0;
			}
			return _textBox.MaxLength;
		}

		public void SetMaxLength(int maxLength)
		{
			if (_textBox != null)
			{
				_textBox.MaxLength = maxLength;
			}
		}

		public ControlStyle GetControlStyle()
		{
			return _appearanceStyle;
		}

		public void SetControlStyle(ControlStyle controlStyle)
		{
			if (_appearanceStyle != controlStyle)
			{
				_appearanceStyle = controlStyle;
				OnControlStyleChanged();
			}
		}

		protected virtual void OnControlStyleChanged()
		{
		}

		internal void AddDropHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _dropped;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _dropped, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		internal void RemoveDropHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _dropped;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _dropped, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected virtual void OnDrop(EventArgs eventArgs)
		{
			if (_dropped != null)
			{
				_dropped(this, eventArgs);
			}
		}

		protected Rectangle GetVirtualClientRectangle()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (RightToLeft == RightToLeft.Yes)
			{
				clientRectangle.X -= _virtualWidth - base.ClientRectangle.Width;
			}
			else
			{
				clientRectangle.X--;
			}
			clientRectangle.Width = _virtualWidth;
			return clientRectangle;
		}

		internal void AddDropDownHideCallback(GridEventCallback gridCallback)
		{
			GridEventCallback dropDownHideCallback = _dropDownHideCallback;
			GridEventCallback comparand;
			do
			{
				comparand = dropDownHideCallback;
				GridEventCallback value = (GridEventCallback)Delegate.Combine(comparand, gridCallback);
				dropDownHideCallback = Interlocked.CompareExchange(ref _dropDownHideCallback, value, comparand);
			}
			while ((object)dropDownHideCallback != comparand);
		}

		internal void RemoveDropDownHideCallback(GridEventCallback gridCallback)
		{
			GridEventCallback dropDownHideCallback = _dropDownHideCallback;
			GridEventCallback source;
			do
			{
				source = dropDownHideCallback;
				GridEventCallback value = (GridEventCallback)Delegate.Remove(source, gridCallback);
				dropDownHideCallback = Interlocked.CompareExchange(ref _dropDownHideCallback, value, source);
			}
			while ((object)dropDownHideCallback != source);
		}

		protected virtual void OnDropDownHide(GridEventArgs gridEventArgs)
		{
			if (_dropDownHideCallback != null)
			{
				_dropDownHideCallback(this, gridEventArgs);
			}
		}

		internal bool IsDropListOpen()
		{
			return GetIsDroppedDown();
		}

		internal bool InvokeMouseWheel(int delta)
		{
			return OnDoMouseWheel(delta);
		}

		protected virtual bool OnDoMouseWheel(int delta)
		{
			return false;
		}

		private void OnTextBoxMouseWheel(object sender, MouseEventArgs eventArgs)
		{
			if (_isDroppedDown)
			{
				OnDoMouseWheel(eventArgs.Delta);
			}
			else
			{
				base.OnMouseWheel(eventArgs);
			}
		}

		protected override void OnMouseWheel(MouseEventArgs eventArgs)
		{
			if (_isDroppedDown)
			{
				OnDoMouseWheel(eventArgs.Delta);
			}
			else
			{
				base.OnMouseWheel(eventArgs);
			}
		}

		internal char GetPasswordChar()
		{
			return _controlStyleContainerControlCharacter;
		}

		internal void SetPasswordChar(char controlStyleContainerControlCharacter)
		{
			if (_controlStyleContainerControlCharacter != controlStyleContainerControlCharacter)
			{
				_controlStyleContainerControlCharacter = controlStyleContainerControlCharacter;
				EditStyle editStyle = _editStyle;
				SetEditStyle(EditStyle.None);
				SetEditStyle(editStyle);
			}
		}

		protected override bool IsInputChar(char charCode)
		{
			if (char.IsLetterOrDigit(charCode))
			{
				return true;
			}
			return base.IsInputChar(charCode);
		}

		internal MaskEdit AsMaskEdit()
		{
			if (_textBox is MaskEdit)
			{
				return (MaskEdit)_textBox;
			}
			return null;
		}
	}
}
