using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;

namespace Mordex.Windows.FilterBuilder.Design
{
	[CLSCompliant(false)]
	public class FilterBuilderDesigner : ControlDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[1]
				{
					new DesignerVerb("Define built-in texts...", OnCustomizeBuiltInText)
				};
				return new DesignerVerbCollection(value);
			}
		}

		public FilterBuilderDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnCustomizeBuiltInText(object sender, EventArgs eventArgs)
		{
			DialogResult dialogResult = DesignHelper.CustomizeBuiltInTexts((MxFilterBuilder)Control);
			if (dialogResult == DialogResult.OK)
			{
				SetDirty();
			}
		}

		private void SetDirty()
		{
			MxFilterBuilder filterBuilder = Control as MxFilterBuilder;
			PropertyDescriptor member = TypeDescriptor.GetProperties(filterBuilder)["BuiltInTextsData"];
			RaiseComponentChanging(member);
			RaiseComponentChanged(member, "", filterBuilder.BuiltInTextsData);
		}
	}
}
