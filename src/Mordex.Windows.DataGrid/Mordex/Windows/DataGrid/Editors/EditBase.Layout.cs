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
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			RecalculateLayout();
			Invalidate();
		}

		private int GetBorderWidth()
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _officeStyle.GetLayoutState().GetControlBorderStyle();
			if (mordexColorSchemeAppearanceState.IsThemedLook)
			{
				if (BorderStyle != BorderStyle.None || _hoverMode != HoverMode.None)
				{
					return 1;
				}
			}
			else
			{
				if (!mordexColorSchemeAppearanceState._isThemed)
				{
					switch (BorderStyle)
					{
					case BorderStyle.Raised:
					case BorderStyle.Sunken:
						return 2;
					case BorderStyle.None:
						if (_hoverMode == HoverMode.None)
						{
							return 0;
						}
						return 1;
					default:
						return 1;
					}
				}
				if (BorderStyle != BorderStyle.None || _hoverMode != HoverMode.None)
				{
					if (GridHelper.IsWindows7RendererEnabled())
					{
						return 1;
					}
					return 2;
				}
			}
			return 0;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (base.Visible)
			{
				// An editor that had no handle when the theme changed never heard about it.
				ApplyThemedTextBoxColors();
				ApplyStaleTheme();
				RecalculateLayout();
			}
		}

		private int GetTextHeight()
		{
			return Font.Height + 3;
		}

		private int GetButtonWidth()
		{
			if (_buttonType == EditButtonType.SingleButton && _buttonStyle == EditButtonStyle.NoButton)
			{
				return 0;
			}
			if (ButtonType == EditButtonType.SingleButton)
			{
				if (_buttonStyle == EditButtonStyle.Image)
				{
					if (_buttonImageSize.Width > 0)
					{
						return _buttonImageSize.Width + 4;
					}
					Image image = GetButtonImage();
					if (image != null)
					{
						return image.Width + 4;
					}
				}
				else if (_buttonStyle == EditButtonStyle.TextButton && _buttonCaption.Length > 0)
				{
					Graphics graphics = CreateGraphics();
					TextLayoutRenderer textLayoutRenderer = CreateButtonTextRenderer();
					int width = (int)textLayoutRenderer.MeasureText(graphics, _buttonCaption, ButtonFont, 0).Width;
					width += 6;
					graphics.Dispose();
					textLayoutRenderer.ReleaseStringFormat();
					return width;
				}
			}
			return LogicalToDeviceUnits(13);
		}

		protected void RefreshLayout()
		{
			RecalculateLayout();
		}

		private void RecalculateLayout()
		{
			if (base.IsDisposed || base.Disposing || !base.IsHandleCreated || _officeStyle == null)
			{
				return;
			}
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _officeStyle.ResolveAppearanceState(GridStyleArea.EditControls);
			int textHeight = GetTextHeight();
			Image image = GetCurrentImage();
			if (_autoSize && (_textBox == null || !_textBox.Multiline))
			{
				int height = textHeight;
				int otherHeight = 0;
				if (image != null)
				{
					otherHeight = image.Height + 2;
					if (_imageVerticalAlignment == ImageVerticalAlignment.BottomOfText || _imageVerticalAlignment == ImageVerticalAlignment.TopOfText)
					{
						otherHeight += textHeight + 1;
					}
					if (otherHeight > height)
					{
						height = otherHeight;
					}
				}
				if (_buttonStyle == EditButtonStyle.Image && _buttonType == EditButtonType.SingleButton)
				{
					otherHeight = _buttonImageSize.Height;
					if (otherHeight == 0)
					{
						Image buttonImage = GetButtonImage();
						if (buttonImage != null)
						{
							otherHeight = buttonImage.Height;
						}
					}
					if (mordexColorSchemeAppearanceState.IsThemedLook)
					{
						otherHeight += 2;
					}
					else if (mordexColorSchemeAppearanceState._isThemed)
					{
						otherHeight += 4;
					}
					else
					{
						ButtonAppearance buttonAppearance = _officeStyle.ButtonAppearance;
						otherHeight = ((buttonAppearance != ButtonAppearance.Regular) ? (otherHeight + 2) : (otherHeight + 4));
					}
					if (otherHeight > height)
					{
						height = otherHeight;
					}
				}
				height = ((BorderStyle == BorderStyle.None && _hoverMode != HoverMode.Highlight) ? (height + 2) : (height + 4));
				if (base.Height != height)
				{
					base.Height = height;
					return;
				}
			}
			else if (_textBox != null && _textBox.Multiline)
			{
				textHeight = -1;
			}
			Size size = new Size(0, 0);
			if (image != null)
			{
				size = image.Size;
			}
			Rectangle displayRectangle = DisplayRectangle;
			if (_buttonType == EditButtonType.SingleButton && _buttonStyle == EditButtonStyle.NoButton)
			{
				_contentBounds = Rectangle.Empty;
				_textArea = displayRectangle;
			}
			else
			{
				int buttonWidth = GetButtonWidth();
				if (RightToLeft == RightToLeft.Yes)
				{
					_contentBounds = new Rectangle(displayRectangle.Left, displayRectangle.Top, buttonWidth, displayRectangle.Height);
					_textArea = new Rectangle(displayRectangle.Left + buttonWidth, displayRectangle.Top, displayRectangle.Width - buttonWidth, displayRectangle.Height);
				}
				else
				{
					_contentBounds = new Rectangle(displayRectangle.Right - buttonWidth, displayRectangle.Top, buttonWidth, displayRectangle.Height);
					_textArea = new Rectangle(displayRectangle.Left, displayRectangle.Top, displayRectangle.Width - buttonWidth, displayRectangle.Height);
				}
				if (!mordexColorSchemeAppearanceState.IsThemedLook && mordexColorSchemeAppearanceState._isThemed && GetBorderWidth() != 0 && (_buttonType == EditButtonType.UpDownButtons || _buttonStyle == EditButtonStyle.DownArrow))
				{
					if (RightToLeft == RightToLeft.No)
					{
						_contentBounds = new Rectangle(_contentBounds.X + 1, _contentBounds.Y - 1, _contentBounds.Width, _contentBounds.Height + 2);
					}
					else
					{
						_contentBounds = new Rectangle(_contentBounds.X - 1, _contentBounds.Y - 1, _contentBounds.Width, _contentBounds.Height + 2);
					}
				}
				if (_buttonType == EditButtonType.SingleButton)
				{
					_buttonStates[0]._bounds = _contentBounds;
				}
				else
				{
					int nextHeight = _contentBounds.Height / 2;
					bool condition = false;
					if (mordexColorSchemeAppearanceState.IsThemedLook)
					{
						condition = true;
					}
					else if (!mordexColorSchemeAppearanceState._isThemed)
					{
						switch (_officeStyle.ButtonAppearance)
						{
						case ButtonAppearance.Flat:
						case ButtonAppearance.FlatBorderless:
						case ButtonAppearance.PopUp:
							condition = false;
							break;
						}
					}
					if (condition)
					{
						nextHeight = (_contentBounds.Height + 1) / 2;
						_buttonStates[0]._bounds = new Rectangle(_contentBounds.X, _contentBounds.Y, _contentBounds.Width, nextHeight);
						_buttonStates[1]._bounds = new Rectangle(_contentBounds.X, _contentBounds.Y + nextHeight - 1, _contentBounds.Width, nextHeight);
						if ((_contentBounds.Height + 1) % 2 != 0)
						{
							_buttonStates[1]._bounds.Height++;
						}
					}
					else
					{
						_buttonStates[0]._bounds = new Rectangle(_contentBounds.X, _contentBounds.Y, _contentBounds.Width, nextHeight);
						_buttonStates[1]._bounds = new Rectangle(_contentBounds.X, _contentBounds.Y + nextHeight, _contentBounds.Width, nextHeight);
						if (_contentBounds.Height % 2 != 0)
						{
							_buttonStates[1]._bounds.Height++;
						}
					}
				}
			}
			_textArea.Inflate(-1, 0);
			if (_textBox != null)
			{
				textHeight = ((textHeight != -1) ? (Font.Height + 2) : _textArea.Height);
				_textBox.AutoSize = false;
				switch (TextAlignment)
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
				Rectangle b = _textArea;
				if (image != null)
				{
					Rectangle rectangle = _textArea;
					try
					{
						_imageArea = rectangle;
						_imageArea.Width = size.Width + 4;
						switch (_imageHorizontalAlignment)
						{
						case ImageHorizontalAlignment.Near:
						case ImageHorizontalAlignment.BeforeText:
							if (RightToLeft == RightToLeft.No)
							{
								_imageArea.X = rectangle.X;
								rectangle.X += size.Width + 4;
							}
							else
							{
								_imageArea.X = rectangle.Right - size.Width - 4;
							}
							rectangle.Width -= size.Width + 4;
							break;
						case ImageHorizontalAlignment.Far:
						case ImageHorizontalAlignment.AfterText:
							if (RightToLeft == RightToLeft.Yes)
							{
								rectangle.X += size.Width + 4;
							}
							rectangle.Width -= size.Width + 4;
							_imageArea.X = rectangle.Right;
							break;
						}
						switch (_imageVerticalAlignment)
						{
						case ImageVerticalAlignment.BottomOfText:
							rectangle.Height -= size.Height + 2;
							b = rectangle;
							break;
						case ImageVerticalAlignment.TopOfText:
							rectangle.Y += size.Height + 2;
							rectangle.Height -= size.Height + 2;
							b = rectangle;
							break;
						default:
							b = new Rectangle(rectangle.Left, rectangle.Top + (rectangle.Height - textHeight) / 2, rectangle.Width, textHeight);
							if ((_textArea.Height - b.Height) % 2 != 0)
							{
								b.Y++;
							}
							break;
						}
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
				else
				{
					b = new Rectangle(_textArea.Left, _textArea.Top + (_textArea.Height - textHeight) / 2, _textArea.Width, textHeight);
					if ((_textArea.Height - b.Height) % 2 != 0)
					{
						b.Y++;
					}
				}
				b = Rectangle.Intersect(DisplayRectangle, b);
				_textBox.Bounds = b;
				_textBox.Update();
				_textBox.Visible = HasText && base.Enabled;
			}
			Invalidate();
		}

		private void ApplyTextAlignment()
		{
			if (TextBox != null)
			{
				switch (TextAlignment)
				{
				case TextAlignment.Center:
					TextBox.TextAlign = HorizontalAlignment.Center;
					break;
				case TextAlignment.Far:
					TextBox.TextAlign = HorizontalAlignment.Right;
					break;
				default:
					TextBox.TextAlign = HorizontalAlignment.Left;
					break;
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTextAlignment()
		{
			TextAlignment = TextAlignment.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetButtonImageSize()
		{
			ButtonImageSize = new Size(0, 0);
		}
	}
}
