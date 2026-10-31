using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(true)]
	[DefaultBindingProperty("Text")]
	public class MxMaskedTextBox : EditBase
	{
		private bool _isUpdatingText;

		protected override TextBox DefaultEditControl => new MaskEdit(checkLicense: false);

		[Editor("Mordex.Windows.DataGrid.Design.InputMaskEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Represents the input mask used when editing the control.")]
		[DefaultValue("")]
		public string Mask
		{
			get
			{
				return ComputeMaskEdit().Mask;
			}
			set
			{
				ComputeMaskEdit().Mask = value;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether literals should be included when updating a masked value.")]
		public bool IncludeLiterals
		{
			get
			{
				return ComputeMaskEdit().IncludeLiterals;
			}
			set
			{
				ComputeMaskEdit().IncludeLiterals = value;
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the control should accept only numbers.")]
		public bool Numeric
		{
			get
			{
				return ComputeMaskEdit().Numeric;
			}
			set
			{
				ComputeMaskEdit().Numeric = value;
			}
		}

		[DefaultValue('_')]
		[Description("Represents the character used to prompt a user for input when a mask is used.")]
		public char PromptChar
		{
			get
			{
				return ComputeMaskEdit().PromptChar;
			}
			set
			{
				ComputeMaskEdit().PromptChar = value;
			}
		}

		[Browsable(false)]
		public string UnmaskedText => ComputeMaskEdit().UnmaskedText;

		public override string Text
		{
			get
			{
				if (_isUpdatingText)
				{
					return base.Text;
				}
				if (IncludeLiterals)
				{
					if (UnmaskedText == "")
					{
						return "";
					}
					return ComputeMaskEdit().GetValueText();
				}
				return UnmaskedText;
			}
			set
			{
				base.Text = value;
			}
		}

		public MxMaskedTextBox()
		{
		}

		private MaskEdit ComputeMaskEdit()
		{
			return (MaskEdit)base.TextBox;
		}

		public bool IsTextValid()
		{
			return ComputeMaskEdit().IsTextValid();
		}

		protected override void OnTextBoxTextChanged(EventArgs e)
		{
			_isUpdatingText = true;
			base.Text = base.TextBox.Text;
			_isUpdatingText = false;
		}
	}
}
