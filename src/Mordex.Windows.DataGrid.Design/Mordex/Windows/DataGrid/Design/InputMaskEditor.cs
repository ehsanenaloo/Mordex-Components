using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	public class InputMaskEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public InputMaskEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null)
				{
					using InputMaskDesigner inputMaskDesigner = new InputMaskDesigner();
					inputMaskDesigner.CreateControl();
					if (context.Instance is GridColumn)
					{
						inputMaskDesigner.IncludeNumericFormats = true;
						GridColumn gridColumn = (GridColumn)context.Instance;
						inputMaskDesigner.IncludeLiterals = gridColumn.IncludeLiterals;
						inputMaskDesigner.MaskPrompt = gridColumn.MaskPrompt.ToString();
					}
					else if (context.Instance is MaskEdit)
					{
						inputMaskDesigner.IncludeNumericFormats = false;
						MaskEdit maskEdit = (MaskEdit)context.Instance;
						inputMaskDesigner.IncludeLiterals = maskEdit.IncludeLiterals;
						inputMaskDesigner.MaskPrompt = maskEdit.PromptChar.ToString();
					}
					else if (context.Instance is MxMaskedTextBox)
					{
						inputMaskDesigner.IncludeNumericFormats = false;
						MxMaskedTextBox maskedEditBox = (MxMaskedTextBox)context.Instance;
						inputMaskDesigner.IncludeLiterals = maskedEditBox.IncludeLiterals;
						inputMaskDesigner.MaskPrompt = maskedEditBox.PromptChar.ToString();
					}
					inputMaskDesigner.Initialize();
					inputMaskDesigner.InputMask = (string)value;
					edSvc.ShowDialog(inputMaskDesigner);
					if (inputMaskDesigner.DialogResult == DialogResult.OK)
					{
						bool hasPrompt = TryGetPromptChar(inputMaskDesigner.MaskPrompt, out char prompt);
						if (context.Instance is GridColumn column)
						{
							// A grid column is not a component, so the change is reported through the editing context.
							if (!inputMaskDesigner.IsNumericMask && context.OnComponentChanging())
							{
								column.IncludeLiterals = inputMaskDesigner.IncludeLiterals;
								if (hasPrompt)
								{
									column.MaskPrompt = prompt;
								}
								context.OnComponentChanged();
							}
						}
						else if (context.Instance is MaskEdit || context.Instance is MxMaskedTextBox)
						{
							Common.Design.DesignerChange.SetProperty(context.Instance, "IncludeLiterals", inputMaskDesigner.IncludeLiterals);
							if (hasPrompt)
							{
								Common.Design.DesignerChange.SetProperty(context.Instance, "PromptChar", prompt);
							}
						}
						return inputMaskDesigner.InputMask;
					}
					return value;
				}
			}
			return value;
		}

		// The prompt box allows any text; only a single character is a valid prompt.
		internal static bool TryGetPromptChar(string text, out char prompt)
		{
			prompt = '\0';
			if (text == null || text.Length != 1)
			{
				return false;
			}
			prompt = text[0];
			return true;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}
	}
}
