using System;
using Microsoft.DotNet.DesignTools.Designers;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[CLSCompliant(false)]
	public class MultiColumnComboDesigner : EditBaseDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[2]
				{
					new DesignerVerb("DropDown Designer", OnDesigner),
					new DesignerVerb("Configure MultiColumnCombo", OnConfigureMultiColumnCombo)
				};
				return new DesignerVerbCollection(value);
			}
		}

		public MultiColumnComboDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnConfigureMultiColumnCombo(object sender, EventArgs eventArgs)
		{
			if (Control is MxMultiColumnComboBox)
			{
				MxMultiColumnComboBox multiColumnCombo = (MxMultiColumnComboBox)Control;
				DesignerChange.Run(multiColumnCombo, null, "Configure Drop Down", () => DesignHelper.ShowDropDownWizard(multiColumnCombo.DropDownList, Control.Container) == DialogResult.OK);
			}
		}

		private void OnDesigner(object sender, EventArgs eventArgs)
		{
			MxMultiColumnComboBox mcCcombo = (MxMultiColumnComboBox)Control;
			DesignerChange.Run(mcCcombo, "DesignTimeLayout", "DropDown Designer", () => DesignHelper.ShowDropDownDesigner(mcCcombo) == DialogResult.OK);
		}
	}
}
