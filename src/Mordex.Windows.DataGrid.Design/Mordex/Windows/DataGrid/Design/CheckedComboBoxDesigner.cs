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
	public class CheckedComboBoxDesigner : EditBaseDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[2]
				{
					new DesignerVerb("DropDown Designer", OnDesigner),
					new DesignerVerb("Configure CheckedComboBox", OnConfigureMultiColumnCombo)
				};
				return new DesignerVerbCollection(value);
			}
		}

		public CheckedComboBoxDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnConfigureMultiColumnCombo(object sender, EventArgs eventArgs)
		{
			if (Control is MxCheckedComboBox)
			{
				MxCheckedComboBox checkedComboBox = (MxCheckedComboBox)Control;
				DesignerChange.Run(checkedComboBox, null, "Configure Drop Down", () => DesignHelper.ShowDropDownWizard(checkedComboBox.DropDownList, Control.Container) == DialogResult.OK);
			}
		}

		private void OnDesigner(object sender, EventArgs eventArgs)
		{
			MxCheckedComboBox cCombo = (MxCheckedComboBox)Control;
			DesignerChange.Run(cCombo, "DesignTimeLayout", "DropDown Designer", () => DesignHelper.ShowDropDownDesigner(cCombo) == DialogResult.OK);
		}
	}
}
