using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class FilterConditionEditor : UITypeEditor
	{
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				try
				{
					GridFilterCondition gridFilterCondition = (GridFilterCondition)value;
					if (gridFilterCondition != null)
					{
						gridFilterCondition = gridFilterCondition.Clone();
					}
					GridTable designModeTable = DesignHelper.GetDesignModeTable(context.Instance);
					if (designModeTable != null)
					{
						Form form = DesignHelper.CreateFilterBuilderDesigner(designModeTable, gridFilterCondition);
						form.Text = context.PropertyDescriptor.Name;
						form.ShowDialog();
						if (form.DialogResult == DialogResult.OK && form is IDesignDialog designDialog)
						{
							value = designDialog.Value;
						}
						form.Dispose();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			}
			return value;
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
