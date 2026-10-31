using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class ListDataMemberEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public override bool IsDropDownResizable => true;

		public ListDataMemberEditor()
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
					object dropDownDataSource = null;
					string text = "";
					if (!(context.Instance is MxCheckedComboBox))
					{
						return value;
					}
					MxCheckedComboBox checkedComboBox = (MxCheckedComboBox)context.Instance;
					if (string.Compare(context.PropertyDescriptor.Name, "DropDownDataMember", ignoreCase: true) == 0)
					{
						dropDownDataSource = checkedComboBox.DropDownDataSource;
						text = checkedComboBox.DropDownDataMember;
					}
					else
					{
						if (string.Compare(context.PropertyDescriptor.Name, "ValuesDataMember", ignoreCase: true) != 0)
						{
							return value;
						}
						dropDownDataSource = checkedComboBox.ValuesDataSource;
						text = checkedComboBox.ValuesDataMember;
					}
					Control control = DesignHelper.CreateDataSourceControl(context.Container, dropDownDataSource, text, dataMemberOnly: true);
					control.Width = 200;
					IDesignDialog designDialog = control as IDesignDialog;
					edSvc.DropDownControl(control);
					value = designDialog.Value;
				}
			}
			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.DropDown;
			}
			return base.GetEditStyle(context);
		}
	}
}
