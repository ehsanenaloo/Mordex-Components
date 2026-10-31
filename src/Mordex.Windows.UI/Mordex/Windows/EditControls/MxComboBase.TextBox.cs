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
	public partial class MxComboBase
	{
		internal EditStyle GetEditStyle()
		{
			return _editStyle;
		}

		internal void UpdateEditStyle(EditStyle editStyle)
		{
			if (!Enum.IsDefined(typeof(EditStyle), editStyle))
			{
				throw new InvalidEnumArgumentException();
			}
			if (editStyle != _editStyle)
			{
				_editStyle = editStyle;
				UpdateTextBoxForEditStyle();
			}
		}

		private void ApplyTextAlignmentToTextBox()
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

		private void UpdateTextBoxForEditStyle()
		{
			if (GetEditStyle() == EditStyle.Combo)
			{
				if (_textBox == null)
				{
					_textBox = new TextBox();
					if (_useDefaultMenu)
					{
						if (_textBox.ContextMenuStrip == GetContextMenu())
						{
							_textBox.ContextMenuStrip = null;
						}
					}
					else
					{
						_textBox.ContextMenuStrip = GetContextMenu();
					}
					_textBox.WordWrap = false;
					_textBox.Multiline = false;
					_textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
					_textBox.Text = Text;
					_textBox.TabStop = false;
					_textBox.ReadOnly = _readOnly;
					base.AllowDrop = false;
					_textBox.AllowDrop = _allowDrop;
					_textBox.RightToLeft = RightToLeft.Inherit;
					_textBox.BackColor = GetEffectiveBackColor();
					_textBox.ForeColor = GetEffectiveForeColor();
					if (IsThemed)
					{
						Mordex.Windows.Common.ThemeWindow.ApplyNativeLook(_textBox, Mordex.Windows.Common.MxThemeManager.DefaultTheme);
					}
					_textBox.Click += OnTextBoxClick;
					_textBox.DoubleClick += OnTextBoxDoubleClick;
					_textBox.DragEnter += OnTextBoxDragEnter;
					_textBox.DragLeave += OnTextBoxDragLeave;
					_textBox.DragOver += OnTextBoxDragOver;
					_textBox.GotFocus += OnTextBoxGotFocus;
					_textBox.KeyDown += OnTextBoxKeyDown;
					_textBox.KeyPress += OnTextBoxKeyPress;
					_textBox.KeyUp += OnTextBoxKeyUp;
					_textBox.LostFocus += OnTextBoxLostFocus;
					_textBox.MouseDown += OnTextBoxMouseDown;
					_textBox.MouseEnter += OnTextBoxMouseEnter;
					_textBox.MouseLeave += OnTextBoxMouseLeave;
					_textBox.MouseMove += OnTextBoxMouseMove;
					_textBox.MouseUp += OnTextBoxMouseUp;
					_textBox.TextChanged += OnTextBoxTextChanged;
					base.Controls.Add(_textBox);
				}
			}
			else if (_textBox != null)
			{
				_textBox.Click -= OnTextBoxClick;
				_textBox.DoubleClick -= OnTextBoxDoubleClick;
				_textBox.DragEnter -= OnTextBoxDragEnter;
				_textBox.DragLeave -= OnTextBoxDragLeave;
				_textBox.DragOver -= OnTextBoxDragOver;
				_textBox.GotFocus -= OnTextBoxGotFocus;
				_textBox.KeyDown -= OnTextBoxKeyDown;
				_textBox.KeyPress -= OnTextBoxKeyPress;
				_textBox.KeyUp -= OnTextBoxKeyUp;
				_textBox.LostFocus -= OnTextBoxLostFocus;
				_textBox.MouseDown -= OnTextBoxMouseDown;
				_textBox.MouseEnter -= OnTextBoxMouseEnter;
				_textBox.MouseLeave -= OnTextBoxMouseLeave;
				_textBox.MouseMove -= OnTextBoxMouseMove;
				_textBox.MouseUp -= OnTextBoxMouseUp;
				_textBox.TextChanged -= OnTextBoxTextChanged;
				base.Controls.Remove(_textBox);
				_textBox.Dispose();
				_textBox = null;
				base.AllowDrop = _allowDrop;
			}
			InvalidateLayout();
		}

		private void OnTextBoxGotFocus(object sender, EventArgs eventArgs)
		{
			if (!_isFocusingTextBox)
			{
				OnGotFocus(eventArgs);
			}
		}

		public new bool Focus()
		{
			try
			{
				if (_textBox != null && HasText)
				{
					_textBox.Visible = true;
					return RenderingHelper.TryFocusControl(_textBox);
				}
				return base.Focus();
			}
			catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
			{
				return false;
			}
		}

		public void CommitSelection()
		{
			OnAcceptChanges(EventArgs.Empty);
		}

		private void OnTextBoxDragEnter(object sender, DragEventArgs eventArgs)
		{
			base.OnDragEnter(eventArgs);
		}

		private void OnTextBoxDragLeave(object sender, EventArgs eventArgs)
		{
			base.OnDragLeave(eventArgs);
		}

		private void OnTextBoxDragOver(object sender, DragEventArgs eventArgs)
		{
			base.OnDragOver(eventArgs);
		}

		private void OnTextBoxLostFocus(object sender, EventArgs eventArgs)
		{
			if (!base.ContainsFocus && !DroppedDown)
			{
				OnLostFocus(eventArgs);
			}
		}

		private void OnTextBoxMouseEnter(object sender, EventArgs eventArgs)
		{
			if (!_isMouseOver)
			{
				OnMouseEnter(eventArgs);
			}
		}

		private void OnTextBoxMouseLeave(object sender, EventArgs eventArgs)
		{
			Point pt = PointToClient(Cursor.Position);
			if (!base.ClientRectangle.Contains(pt))
			{
				OnMouseLeave(eventArgs);
			}
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			if (_textBox != null)
			{
				_textBox.BackColor = GetEffectiveBackColor();
			}
			base.OnBackColorChanged(e);
		}

		private void OnTextBoxKeyPress(object sender, KeyPressEventArgs eventArgs)
		{
			OnKeyPress(eventArgs);
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
			base.OnMouseDown(RenderingHelper.TranslateMouseEvent((Control)sender, this, eventArgs));
		}

		private void OnTextBoxMouseUp(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseUp(RenderingHelper.TranslateMouseEvent((Control)sender, this, eventArgs));
		}

		private void OnTextBoxMouseMove(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseMove(RenderingHelper.TranslateMouseEvent((Control)sender, this, eventArgs));
		}

		private void OnTextBoxClick(object sender, EventArgs eventArgs)
		{
			base.OnClick(eventArgs);
		}

		private void OnTextBoxDoubleClick(object sender, EventArgs eventArgs)
		{
			base.OnDoubleClick(eventArgs);
		}

		protected internal virtual void OnKeyPressed(EventArgs e)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual void InternalProcessChar(char keyChar)
		{
		}

		internal Image GetImage()
		{
			return _itemImage;
		}

		internal void SetImage(Image image)
		{
			if (_itemImage != image)
			{
				_itemImage = image;
				InvalidateLayout();
			}
		}

		private void OnTextBoxTextChanged(object sender, EventArgs eventArgs)
		{
			base.Text = _textBox.Text;
			if (GetImage() != null)
			{
				int selectionStart = TextBox.SelectionStart;
				LayoutContent(null);
				_textBox.SelectionStart = selectionStart;
			}
			if (_pendingKeyPressCount > 0)
			{
				_pendingKeyPressCount--;
				if (_pendingKeyPressCount == 0)
				{
					OnKeyPressed(eventArgs);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnChange(EventArgs e)
		{
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (_readOnly)
			{
				return base.ProcessDialogKey(keyData);
			}
			switch (keyData)
			{
			case Keys.F4:
			case Keys.Down | Keys.Alt:
				if (GetEditStyle() != EditStyle.Text)
				{
					if (_droppedDown)
					{
						OnDropHide(cancel: false);
					}
					else
					{
						DropListControl(setCapture: true);
					}
					return base.ProcessDialogKey(keyData);
				}
				break;
			case Keys.Escape:
				if (_editStyle != EditStyle.Text)
				{
					if (_droppedDown)
					{
						OnDropHide(cancel: true);
						return true;
					}
					OnCancelChanges(EventArgs.Empty);
					return base.ProcessDialogKey(keyData);
				}
				break;
			case Keys.Tab:
			{
				bool previousShouldFocus = _shouldFocusTextBox;
				_shouldFocusTextBox = false;
				bool result = base.ProcessDialogKey(keyData);
				_shouldFocusTextBox = previousShouldFocus;
				return result;
			}
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (e.KeyChar != '\r' && e.KeyChar != '\b')
			{
				_pendingKeyPressCount = 1;
			}
			else
			{
				_pendingKeyPressCount = -1;
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled || ReadOnly)
			{
				return;
			}
			switch (e.KeyCode)
			{
			case Keys.F4:
				if ((Control.ModifierKeys & Keys.Alt) != Keys.Alt)
				{
					DroppedDown = !_droppedDown;
				}
				break;
			case Keys.Escape:
				DroppedDown = false;
				break;
			case Keys.Return:
				if (DroppedDown)
				{
					OnDropHide(cancel: false);
				}
				else
				{
					OnAcceptChanges(EventArgs.Empty);
				}
				break;
			}
		}

		internal int GetImageIndex()
		{
			return _imageIndex;
		}

		internal void SetImageIndex(int imageIndex)
		{
			if (_imageIndex != imageIndex)
			{
				_imageIndex = imageIndex;
				_imageKey = "";
				InvalidateLayout();
			}
		}

		internal string GetImageKey()
		{
			return _imageKey;
		}

		internal void SetImageKey(string text)
		{
			if (text == null)
			{
				text = "";
			}
			if (_imageKey != text)
			{
				_imageKey = text;
				_imageIndex = -1;
				InvalidateLayout();
			}
		}

		internal void SelectRange(int firstPosition, int secondPosition)
		{
			if (_textBox != null)
			{
				int start;
				int length;
				if (firstPosition < secondPosition)
				{
					start = firstPosition;
					length = secondPosition - firstPosition;
				}
				else
				{
					start = secondPosition;
					length = firstPosition - secondPosition;
				}
				_textBox.Select(start, length);
				_textBox.ScrollToCaret();
			}
		}
	}
}
