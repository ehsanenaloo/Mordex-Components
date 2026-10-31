using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class MaskEdit
	{
		[DefaultValue(false)]
		public bool Numeric
		{
			get
			{
				return _numeric;
			}
			set
			{
				if (_numeric != value)
				{
					_numeric = value;
				}
			}
		}

		[DefaultValue("")]
		[Editor("Mordex.Windows.DataGrid.Design.InputMaskEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Localizable(true)]
		public string Mask
		{
			get
			{
				return _mask;
			}
			set
			{
				if (!(value != _mask))
				{
					return;
				}
				string text = ((!_hasMask) ? base.Text : Text);
				_mask = value;
				UpdateMaskEdit();
				if (_hasMask)
				{
					if (!IsTextValidForMask(text, false))
					{
						ClearSlots(0, _maskSlots.Length);
					}
					ApplyText(GetDisplayText(Focused));
				}
				else
				{
					Text = text;
				}
			}
		}

		[DefaultValue('_')]
		public char PromptChar
		{
			get
			{
				return _promptChar;
			}
			set
			{
				if (value != _promptChar)
				{
					_promptChar = value;
				}
			}
		}

		[DefaultValue(true)]
		public bool IncludeLiterals
		{
			get
			{
				return _includeLiterals;
			}
			set
			{
				if (value != _includeLiterals)
				{
					_includeLiterals = value;
				}
			}
		}

		[Browsable(false)]
		public string UnmaskedText
		{
			get
			{
				if (_hasMask)
				{
					StringBuilder stringBuilder = new StringBuilder(_maskSlots.Length);
					for (int i = 0; i < _maskSlots.Length; i++)
					{
						if (_maskSlots[i]._rawText != null && _maskSlots[i]._rawText.Length != 0 && !_maskSlots[i]._isLiteral)
						{
							stringBuilder.Append(_maskSlots[i]._rawText);
						}
					}
					return stringBuilder.ToString();
				}
				return base.Text;
			}
		}

		public override string Text
		{
			get
			{
				if (_isHandlingTextChange || base.DesignMode)
				{
					return base.Text;
				}
				return BuildValueText();
			}
			set
			{
				bool condition = value != BuildValueText();
				_isInternalTextChange = true;
				if (_maskSlots != null && _maskSlots.Length > 0)
				{
					ClearSlots(0, _maskSlots.Length);
					_hasMask = true;
				}
				if (_hasMask)
				{
					if (value == null || value.Length == 0)
					{
						ClearSlots(0, _maskSlots.Length);
						ApplyText(GetDisplayText(Focused));
					}
					else if (IsTextValidForMask(value, true))
					{
						ApplyText(GetDisplayText(Focused));
					}
					else
					{
						_hasMask = false;
						ApplyText(value);
					}
				}
				else if (value == null || value.Length == 0)
				{
					if (_mask != null && _mask.Length > 0)
					{
						ClearSlots(0, _maskSlots.Length);
						_hasMask = true;
						ApplyText(GetDisplayText(Focused));
					}
					else
					{
						ApplyText(value);
					}
				}
				else
				{
					ApplyText(value);
				}
				_isInternalTextChange = false;
				base.Modified = false;
				if (condition)
				{
					base.OnTextChanged(EventArgs.Empty);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string SelectedText
		{
			get
			{
				int selectionStart = base.SelectionStart;
				int selectionLength = SelectionLength;
				if (selectionLength < 0)
				{
					selectionStart += selectionLength;
					selectionLength *= -1;
				}
				if (selectionLength == 0)
				{
					return "";
				}
				if (selectionStart >= Text.Length)
				{
					return "";
				}
				return Text.Substring(selectionStart, selectionLength);
			}
			set
			{
				base.SelectedText = value;
			}
		}
	}
}
