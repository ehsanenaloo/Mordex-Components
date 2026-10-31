using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		private int GetBorderPadding()
		{
			if (GetEffectiveBorderStyle() == BorderStyle.None)
			{
				return 0;
			}
			return 2;
		}

		private void LayoutElements(Graphics graphics)
		{
			UpdateBorderWidth();
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-_borderWidth, -_borderWidth);
			int right = clientRectangle.Right;
			int buttonWidth = 0;
			if (_showDropDown)
			{
				buttonWidth = OwnerDrawnButtonWidth;
			}
			Rectangle rectangle;
			if (RightToLeft == RightToLeft.Yes)
			{
				rectangle = new Rectangle(clientRectangle.Left, clientRectangle.Top, buttonWidth, clientRectangle.Height);
				if (buttonWidth > 0)
				{
					if (_borderWidth > 0)
					{
						rectangle.Inflate(0, 1);
						rectangle.X--;
					}
					rectangle.Width++;
				}
				right = rectangle.Right;
			}
			else
			{
				rectangle = new Rectangle(clientRectangle.Right - buttonWidth, clientRectangle.Top, buttonWidth, clientRectangle.Height);
				if (buttonWidth > 0)
				{
					if (_borderWidth > 0)
					{
						rectangle.Inflate(0, 1);
					}
					else
					{
						rectangle.X--;
					}
					rectangle.Width++;
				}
				right = rectangle.Left;
			}
			_dropDownButton._bounds = rectangle;
			int spinOverlap = 0;
			if (!_showUpDown)
			{
				buttonWidth = 0;
			}
			else
			{
				buttonWidth = OwnerDrawnButtonWidth;
				if (ShowDropDown)
				{
					if (_borderWidth < 2 && _borderWidth > 0)
					{
						spinOverlap = 2;
						if (RightToLeft == RightToLeft.Yes)
						{
							rectangle.X++;
						}
						else
						{
							rectangle.X--;
						}
					}
					else
					{
						spinOverlap = _borderWidth;
						if (RightToLeft == RightToLeft.Yes)
						{
							rectangle.X += _borderWidth;
						}
						else
						{
							rectangle.X -= _borderWidth;
						}
					}
				}
			}
			if (RightToLeft == RightToLeft.Yes)
			{
				rectangle = new Rectangle(rectangle.Right, clientRectangle.Top, buttonWidth, clientRectangle.Height);
				if (buttonWidth > 0)
				{
					if (_borderWidth > 0)
					{
						rectangle.Inflate(0, 1);
						rectangle.X--;
					}
					rectangle.Width++;
				}
				right = rectangle.Right;
			}
			else
			{
				rectangle = new Rectangle(rectangle.Left - buttonWidth, clientRectangle.Top, buttonWidth, clientRectangle.Height);
				if (buttonWidth > 0)
				{
					if (_borderWidth > 0)
					{
						rectangle.Inflate(0, 1);
					}
					else
					{
						rectangle.X--;
					}
					rectangle.Width++;
				}
				right = rectangle.Left;
			}
			_upButton._bounds = new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height / 2);
			_downButton._bounds = new Rectangle(rectangle.Left, rectangle.Top + (rectangle.Height + spinOverlap) / 2, rectangle.Width, rectangle.Height - spinOverlap - (rectangle.Height - spinOverlap) / 2);
			_downButton._arrowHeight = (rectangle.Height - spinOverlap) / 2;
			int checkBoxWidth = CheckBoxColumnWidth;
			if (!_showCheckBox)
			{
				checkBoxWidth = 0;
			}
			if (RightToLeft == RightToLeft.Yes)
			{
				if (_showCheckBox)
				{
					_checkBoxButton._bounds = new Rectangle(clientRectangle.Right - checkBoxWidth - 1, clientRectangle.Top, checkBoxWidth, clientRectangle.Height);
				}
				else
				{
					_checkBoxButton._bounds = new Rectangle(clientRectangle.Right - checkBoxWidth, clientRectangle.Top, checkBoxWidth, clientRectangle.Height);
				}
			}
			else if (_showCheckBox)
			{
				_checkBoxButton._bounds = new Rectangle(clientRectangle.Left + 1, clientRectangle.Top, checkBoxWidth, clientRectangle.Height);
			}
			else
			{
				_checkBoxButton._bounds = new Rectangle(clientRectangle.Left, clientRectangle.Top, checkBoxWidth, clientRectangle.Height);
			}
			if (_editStyle == EditStyle.Masked)
			{
				if (RightToLeft == RightToLeft.No)
				{
					int nextLeft = _checkBoxButton._bounds.Right + 1;
					DateFieldPortion[] portions = _portions;
					foreach (DateFieldPortion portion in portions)
					{
						portion.LayoutStartingAt(graphics, Font, nextLeft, clientRectangle.Top + 1, clientRectangle.Height - 2, _value);
						if (portion._bounds.Width > 0)
						{
							nextLeft += portion._bounds.Width + 1;
						}
					}
				}
				else
				{
					int nextRight = _checkBoxButton._bounds.Left - 1;
					for (int index = _portions.Length - 1; index >= 0; index--)
					{
						DateFieldPortion lastPortion = _portions[index];
						lastPortion.LayoutEndingAt(graphics, Font, nextRight, clientRectangle.Top + 1, clientRectangle.Height - 2, _value);
						if (lastPortion._bounds.Width > 0)
						{
							nextRight = lastPortion._bounds.Left - 1;
						}
					}
				}
			}
			else if (_textBox != null)
			{
				if (RightToLeft == RightToLeft.No)
				{
					int textBoxLeft = _checkBoxButton._bounds.Right + 1;
					_textBox.Bounds = new Rectangle(textBoxLeft, clientRectangle.Top, right - textBoxLeft - 1, clientRectangle.Height);
				}
				else
				{
					int textBoxRight = _checkBoxButton._bounds.Left - 1;
					_textBox.Bounds = new Rectangle(right, clientRectangle.Top, textBoxRight - right, clientRectangle.Height);
				}
				_textBox.Top = clientRectangle.Top + (clientRectangle.Height - _textBox.Height) / 2;
			}
			if (base.DesignMode)
			{
				using (TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering))
				{
					textLayoutRenderer.RightToLeft = RightToLeft;
					textLayoutRenderer.Alignment = StringAlignment.Near;
					textLayoutRenderer.NoPadding = true;
					int textHeight = (int)textLayoutRenderer.MeasureText(graphics, Text, Font, 0).Height;
					_baseLineOffset = (DisplayRectangle.Height - textHeight) / 2 + textHeight;
				}
			}
			Invalidate();
		}

		protected override void OnForeColorChanged(EventArgs e)
		{
			base.OnForeColorChanged(e);
			if (_textBox != null)
			{
				_textBox.ForeColor = GetEditForeColor();
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			RequestLayout();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			RequestLayout();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			RequestLayout();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (!base.Enabled)
			{
				SetPressedButton(null);
			}
			if (_textBox != null)
			{
				_textBox.Visible = base.Enabled;
			}
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			if (_textBox != null)
			{
				_textBox.BackColor = GetEditBackColor();
			}
			base.OnBackColorChanged(e);
		}

		private void UpdateAutoHeight()
		{
			UpdateBorderWidth();
			if (_autoSize && Dock == DockStyle.None)
			{
				int height = (int)Font.GetHeight() + 2;
				height += GetBorderPadding() * 2 + 2;
				if (GetBorderPadding() == 0)
				{
					height += 2;
				}
				if (base.Height != height)
				{
					base.Height = height;
				}
			}
		}

		internal void RequestLayout()
		{
			_layoutPending = true;
			UpdateAutoHeight();
			Invalidate();
		}

		private BorderStyle GetEffectiveBorderStyle()
		{
			if (_borderStyle == BorderStyle.None)
			{
				if (_hoverMode == HoverMode.Highlight)
				{
					return BorderStyle.Flat;
				}
				return _borderStyle;
			}
			return BorderStyle.Flat;
		}

		private void UpdateBorderWidth()
		{
			if (GetEffectiveBorderStyle() == BorderStyle.None)
			{
				_borderWidth = 0;
			}
			else
			{
				_borderWidth = 2;
			}
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
			RequestLayout();
		}
	}
}
