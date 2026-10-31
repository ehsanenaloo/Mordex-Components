using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class EditBase
	{
		private void ApplyDefaultSettings()
		{
			_characterCasing = CharacterCasing.Normal;
			_hideSelection = true;
			_maxLength = 32767;
			_autoSize = true;
			_borderStyle = BorderStyle.Sunken;
			_buttonEnabled = true;
			_buttonImageIndex = -1;
			_buttonImageKey = "";
			_buttonImageSize = new Size(0, 0);
			_buttonStyle = EditButtonStyle.NoButton;
			_buttonCaption = "";
			_hasImage = true;
			_isTextPresent = true;
			_hotButtonIndex = -1;
			_hoverMode = HoverMode.None;
			_imageHorizontalAlignment = ImageHorizontalAlignment.BeforeText;
			_imageIndex = -1;
			_imageKey = "";
			_imageVerticalAlignment = ImageVerticalAlignment.Center;
			_pressedButtonIndex = -1;
			_shortcutsEnabled = true;
			_textAlignment = TextAlignment.Empty;
			_useDefaultMenu = true;
			_isHighlightColorUsed = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_officeStyle != null)
				{
					_officeStyle.RemoveChangedHandler(HandleControlStyleChanged);
					_officeStyle.ReleaseLayoutState();
					_officeStyle = null;
				}
				StopListeningToThemes();
			}
			base.Dispose(disposing);
		}

		private void InitializeDefaults()
		{
			_officeStyle = new ControlStyle(this);
			_buttonType = EditButtonType.SingleButton;
			_officeStyle.AddChangedHandler(HandleControlStyleChanged);
			BackColor = SystemColors.Window;
			base.ForeColor = SystemColors.WindowText;
			_disabledBackground = Color.Empty;
			_disabledForeground = SystemColors.GrayText;
			EditControl = DefaultEditControl;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return base.BackColor != SystemColors.Window;
		}

		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColor()
		{
			return base.ForeColor != SystemColors.WindowText;
		}

		public new void ResetForeColor()
		{
			ForeColor = SystemColors.WindowText;
		}

		private BorderStyle ComputeBorderStyle()
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _officeStyle.GetLayoutState().GetControlBorderStyle();
			BorderStyle borderStyle = BorderStyle;
			if (base.Enabled)
			{
				if (_hoverMode == HoverMode.Highlight)
				{
					if (base.ContainsFocus || IsMouseOver())
					{
						borderStyle = BorderStyle.Flat;
						if (!mordexColorSchemeAppearanceState.IsThemedLook && !mordexColorSchemeAppearanceState._isThemed)
						{
							if (BorderStyle == BorderStyle.Flat || BorderStyle == BorderStyle.None)
							{
								if (_officeStyle.ButtonAppearance == ButtonAppearance.PopUp)
								{
									borderStyle = BorderStyle.SunkenLight3D;
								}
							}
							else if (BorderStyle != BorderStyle.None)
							{
								borderStyle = BorderStyle;
							}
						}
					}
					else if (mordexColorSchemeAppearanceState.IsThemedLook)
					{
						if (borderStyle != BorderStyle.None)
						{
							borderStyle = BorderStyle.Flat;
						}
					}
					else if (mordexColorSchemeAppearanceState._isThemed && borderStyle != BorderStyle.None)
					{
						borderStyle = BorderStyle.Flat;
					}
				}
				else if (mordexColorSchemeAppearanceState.IsThemedLook)
				{
					if (borderStyle != BorderStyle.None)
					{
						borderStyle = BorderStyle.Flat;
					}
				}
				else if (mordexColorSchemeAppearanceState._isThemed && borderStyle != BorderStyle.None)
				{
					borderStyle = BorderStyle.Flat;
				}
			}
			else if (HoverMode == HoverMode.Highlight)
			{
				if (borderStyle == BorderStyle.None)
				{
					borderStyle = BorderStyle.Flat;
				}
			}
			else if (mordexColorSchemeAppearanceState.IsThemedLook)
			{
				if (borderStyle != BorderStyle.None)
				{
					borderStyle = BorderStyle.Flat;
				}
			}
			else if (mordexColorSchemeAppearanceState._isThemed && borderStyle != BorderStyle.None)
			{
				borderStyle = BorderStyle.Flat;
			}
			return borderStyle;
		}

		private ButtonStyle ComputeButtonStyle(EditButtonStyle editButtonStyle)
		{
			switch (editButtonStyle)
			{
			case EditButtonStyle.DownArrow:
				return Mordex.Windows.DataGrid.ButtonStyle.DownArrow;
			case EditButtonStyle.Ellipsis:
				return Mordex.Windows.DataGrid.ButtonStyle.Ellipsis;
			case EditButtonStyle.Image:
				return Mordex.Windows.DataGrid.ButtonStyle.Image;
			case EditButtonStyle.TextButton:
				return Mordex.Windows.DataGrid.ButtonStyle.TextButton;
			default:
				return Mordex.Windows.DataGrid.ButtonStyle.NoButton;
			}
		}

		private void SetPressedButton(int editBaseInteger)
		{
			if (editBaseInteger != _pressedButtonIndex)
			{
				if (_pressedButtonIndex != -1)
				{
					_buttonStates[_pressedButtonIndex]._isPressed = false;
					_buttonStates[_pressedButtonIndex]._isHot = false;
				}
				_pressedButtonIndex = editBaseInteger;
				if (_pressedButtonIndex != -1)
				{
					_buttonStates[_pressedButtonIndex]._isPressed = true;
				}
				Invalidate();
				Update();
			}
		}

		private void SetHotButton(int hotButtonIndex)
		{
			try
			{
				if (hotButtonIndex != -1 && (base.TopLevelControl == null || !base.TopLevelControl.ContainsFocus))
				{
					hotButtonIndex = -1;
				}
				if (hotButtonIndex != _hotButtonIndex)
				{
					if (_hotButtonIndex != -1)
					{
						_buttonStates[_hotButtonIndex]._isHot = false;
					}
					_hotButtonIndex = hotButtonIndex;
					if (_hotButtonIndex != -1)
					{
						_buttonStates[_hotButtonIndex]._isHot = true;
					}
					Invalidate();
				}
			}
			finally
			{
			}
		}

		private void OnEditControlResize(object sender, EventArgs eventArgs)
		{
			NativeMethods.NativeRectStruct e = default(NativeMethods.NativeRectStruct);
			NativeMethods.GetWindowRectStruct(_textBox.Handle, ref e);
		}

		private Image GetCurrentImage()
		{
			if (!HasImage)
			{
				return null;
			}
			Image image = _displayImage;
			if (image == null && _imageList != null)
			{
				if (!string.IsNullOrEmpty(_imageKey) && _imageList.Images.ContainsKey(_imageKey))
				{
					return _imageList.Images[_imageKey];
				}
				if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
				{
					image = _imageList.Images[_imageIndex];
				}
			}
			return image;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = SystemColors.WindowFrame;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !FlatBorderColor.Equals(SystemColors.WindowFrame);
		}

		private void OnEditControlGotFocus(object sender, EventArgs eventArgs)
		{
			base.OnGotFocus(eventArgs);
		}

		private void OnEditControlLostFocus(object sender, EventArgs eventArgs)
		{
			if (!base.ContainsFocus)
			{
				base.OnLostFocus(eventArgs);
			}
		}

		private void OnEditControlKeyDown(object sender, KeyEventArgs eventArgs)
		{
			OnKeyDown(eventArgs);
		}

		private void OnEditControlKeyPress(object sender, KeyPressEventArgs eventArgs)
		{
			OnKeyPress(eventArgs);
		}

		private void OnEditControlKeyUp(object sender, KeyEventArgs eventArgs)
		{
			OnKeyUp(eventArgs);
		}

		private void OnEditControlTextChanged(object sender, EventArgs eventArgs)
		{
			OnTextBoxTextChanged(eventArgs);
		}

		ImageList IGridStyleSource.GetImageList()
		{
			return _imageList;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTextAlignment()
		{
			return _textAlignment != TextAlignment.Empty;
		}

		protected override bool IsInputChar(char charCode)
		{
			if (char.IsLetterOrDigit(charCode))
			{
				return true;
			}
			return base.IsInputChar(charCode);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeButtonFont()
		{
			return _buttonFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetButtonFont()
		{
			ButtonFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeButtonImageSize()
		{
			return !_buttonImageSize.IsEmpty;
		}

		private Image GetButtonImage()
		{
			if (_buttonImage != null)
			{
				return _buttonImage;
			}
			if (_imageList != null)
			{
				if (!string.IsNullOrEmpty(_buttonImageKey) && _imageList.Images.ContainsKey(_buttonImageKey))
				{
					return _imageList.Images[_buttonImageKey];
				}
				if (_buttonImageIndex >= 0 && _buttonImageIndex < _imageList.Images.Count)
				{
					return _imageList.Images[_buttonImageIndex];
				}
			}
			return null;
		}

		private EditButton GetButtonForIndex(int value)
		{
			if (_buttonType == EditButtonType.SingleButton)
			{
				return EditButton.Button;
			}
			if (value == 0)
			{
				return EditButton.UpButton;
			}
			return EditButton.DownButton;
		}

		protected virtual void OnImageListChanged(EventArgs e)
		{
			if (base.Events[_imageListChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal bool GetUseHighlightColor()
		{
			return UseHighlightColorCore;
		}

		internal void SetUseHighlightColor(bool useHighlightColorCore)
		{
			UseHighlightColorCore = useHighlightColorCore;
		}

		private void UpdateTimer()
		{
			if (_timer == null)
			{
				_timer = new System.Windows.Forms.Timer();
				_timer.Enabled = false;
				_timer.Interval = Timing.InitialAutoRepeatDelay;
				_timer.Tick += OnTimerTick;
			}
		}

		private void DisposeResources()
		{
			if (_timer != null)
			{
				_repeatDirection = 0;
				_timer.Stop();
				_timer.Enabled = false;
				_timer.Dispose();
				_timer = null;
			}
		}

		internal void StartRepeatUp()
		{
			if (!_readOnly)
			{
				_repeatHadCapture = base.Capture;
				_repeatDirection = 1;
				OnButtonClick(EditButton.UpButton);
				UpdateTimer();
				_timer.Enabled = true;
			}
		}

		internal void StartRepeatDown()
		{
			if (!_readOnly)
			{
				_repeatHadCapture = base.Capture;
				_repeatDirection = 2;
				OnButtonClick(EditButton.DownButton);
				UpdateTimer();
				_timer.Enabled = true;
			}
		}

		internal void StopRepeat()
		{
			DisposeResources();
		}

		internal void SuspendRepeat()
		{
			if (_repeatDirection > 0)
			{
				_repeatDirection = -_repeatDirection;
			}
		}

		internal void ResumeRepeat()
		{
			if (_repeatDirection < 0)
			{
				_repeatDirection = -_repeatDirection;
			}
		}

		public void AppendText(string text)
		{
			if (TextBox != null)
			{
				TextBox.AppendText(text);
			}
			else
			{
				Text += text;
			}
		}

		public void Clear()
		{
			if (TextBox != null)
			{
				TextBox.Clear();
			}
			else
			{
				Text = "";
			}
		}

		public void ClearUndo()
		{
			if (TextBox != null)
			{
				TextBox.ClearUndo();
			}
		}

		public void Copy()
		{
			if (TextBox != null)
			{
				TextBox.Copy();
			}
			else
			{
				TrySetClipboardText();
			}
		}

		private bool TrySetClipboardText()
		{
			try
			{
				Clipboard.SetDataObject(Text, true, ClipboardRetryCount, ClipboardRetryDelay);
				return true;
			}
			catch (ExternalException)
			{
				// The clipboard is locked by another program.
				return false;
			}
		}

		public void Cut()
		{
			if (TextBox != null)
			{
				TextBox.Cut();
				return;
			}
			if (TrySetClipboardText())
			{
				Text = "";
			}
		}

		public void Paste()
		{
			try
			{
				if (TextBox != null)
				{
					TextBox.Paste();
					return;
				}
				IDataObject dataObject = Clipboard.GetDataObject();
				if (dataObject != null && dataObject.GetDataPresent(typeof(string)))
				{
					Text = (string)dataObject.GetData(typeof(string));
				}
			}
			catch (ExternalException)
			{
				// The clipboard is locked by another program; the paste is skipped.
			}
		}

		public void Select(int start, int length)
		{
			if (TextBox != null)
			{
				TextBox.Select(start, length);
			}
		}

		public void SelectAll()
		{
			if (TextBox != null)
			{
				TextBox.SelectAll();
			}
		}

		public void Undo()
		{
			if (TextBox != null)
			{
				TextBox.Undo();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDisabledBackColor()
		{
			return !_disabledBackground.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDisabledBackColor()
		{
			_disabledBackground = Color.Empty;
			InvalidateOnColorChange();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDisabledForeColor()
		{
			return _disabledForeground != SystemColors.GrayText;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDisabledForeColor()
		{
			DisabledForeColor = SystemColors.GrayText;
		}

		private void InvalidateOnColorChange()
		{
			Invalidate();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			IMordexContainer mordexContainer = GridHelper.ComputeIMordexContainer(this);
			if (mordexContainer != null && mordexContainer.PreProcessDialogKey(keyData))
			{
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		internal int GetBaselineOffset()
		{
			Graphics graphics = CreateGraphics();
			string text = Text;
			if (text.Length == 0)
			{
				text = "0";
			}
			int height = (int)ComputeTextLayoutRenderer().MeasureText(graphics, text, Font, _textArea.Width).Height;
			graphics.Dispose();
			return (DisplayRectangle.Height - height) / 2 + height + 1;
		}

		internal Rectangle GetButtonScreenBounds(int value)
		{
			Rectangle rectangle = _buttonStates[value]._bounds;
			if (rectangle.IsEmpty)
			{
				return rectangle;
			}
			rectangle.Location = PointToScreen(rectangle.Location);
			return rectangle;
		}

		internal AccessibleRole GetButtonAccessibleRole()
		{
			if (ButtonType == EditButtonType.SingleButton)
			{
				if (ButtonStyle == EditButtonStyle.DownArrow)
				{
					return AccessibleRole.ButtonDropDown;
				}
				return AccessibleRole.PushButton;
			}
			return AccessibleRole.SpinButton;
		}

		internal AccessibleObject GetAccessibilityObjectOrBase()
		{
			if (AccessibleObjectCore == null)
			{
				return base.AccessibilityObject;
			}
			return AccessibleObjectCore;
		}
	}
}
