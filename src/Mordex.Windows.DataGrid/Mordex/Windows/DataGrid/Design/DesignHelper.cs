using System;
using Mordex.Windows.DataGrid.Internal;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.DataGrid.Design.Designer;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	public static class DesignHelper
	{
		private static bool? _nullable = null;

		public static Bitmap ErrorImage => ResourceImages.GetErrorImage();

		internal static void EnsureDesignAssemblyLoaded()
		{
			if (!_nullable.HasValue)
			{
				try
				{
					Assembly.Load("Mordex.Windows.DataGrid.Design");
					_nullable = true;
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					_nullable = false;
				}
			}
			if (!_nullable.Value)
			{
				throw new Exception("This Form uses a Mordex WinForms Control that was not properly installed in this machine.  The Form Designer was prevented from being shown in order to avoid a possible corruption of the Form Settings. Please install the Mordex WinForms Controls using the Controls Setup in order to edit this Form.");
			}
		}

		public static DialogResult ShowDropDownWizard(GridDropDown dd, IContainer controlContainer)
		{
			MultiColumnComboConfigurationWizardDialog multiColumnComboConfigurationWizardDialog = new MultiColumnComboConfigurationWizardDialog();
			DialogResult result = multiColumnComboConfigurationWizardDialog.ComputeDialogResult(dd, controlContainer);
			multiColumnComboConfigurationWizardDialog.Dispose();
			return result;
		}

		public static DialogResult ShowDropDownDesigner(MxCheckedComboBox cCombo)
		{
			DropDownDesignerDialog dropDownDesignerDialog = new DropDownDesignerDialog();
			DialogResult result = dropDownDesignerDialog.ComputeDialogResult(cCombo);
			dropDownDesignerDialog.Dispose();
			return result;
		}

		public static DialogResult ShowDropDownDesigner(MxMultiColumnComboBox mcCcombo)
		{
			DropDownDesignerDialog dropDownDesignerDialog = new DropDownDesignerDialog();
			DialogResult result = dropDownDesignerDialog.ShowForCombo(mcCcombo);
			dropDownDesignerDialog.Dispose();
			return result;
		}

		public static DialogResult ShowCustomGroupRowDialog(GridCustomGroup customGroup)
		{
			CustomGroupRowsDialog customGroupRowsDialog = new CustomGroupRowsDialog();
			customGroupRowsDialog.SetCustomGroupRows(customGroup.GroupRows);
			DialogResult result = customGroupRowsDialog.ShowDialog();
			customGroupRowsDialog.Dispose();
			return result;
		}

		public static void ShowAboutBox(MxDataGrid grid)
		{
			GridAboutDialog gridAboutDialog = new GridAboutDialog();
			gridAboutDialog.ShowAbout(grid);
			gridAboutDialog.Dispose();
		}

		public static DialogResult ShowGridDesigner(MxDataGrid grid)
		{
			GridEditorDialog gridEditorDialog = new GridEditorDialog();
			DialogResult result = gridEditorDialog.ComputeDialogResult(grid);
			gridEditorDialog.Dispose();
			return result;
		}

		public static Form CreateValueListDesigner(GridValueListItemCollection valueList, Type propertyType, ImageList imageList)
		{
			ValueListDesigner valueListDesigner = new ValueListDesigner();
			valueListDesigner.LoadValueListItems(valueList, propertyType, imageList);
			return valueListDesigner;
		}

		public static Form CreateFilterBuilderDesigner(GridTable table, GridFilterCondition condition)
		{
			MxFilterBuilderDialog filterBuilderDialogDialog = new MxFilterBuilderDialog();
			filterBuilderDialogDialog.SetFilterCondition(condition);
			filterBuilderDialogDialog.SetGridTable(table);
			return filterBuilderDialogDialog;
		}

		public static PropertyDescriptor[] GetTableAvailableFields(GridTable tbl, bool includeFields, bool includeLists)
		{
			return tbl.GetBaseProperties(includeFields, includeLists);
		}

		public static GridTable GetDesignModeTable(object o)
		{
			if (o is GridColumn gridColumn)
			{
				return gridColumn.Table;
			}
			if (o is GridGroup gridGroup)
			{
				return gridGroup.GetEffectiveTable();
			}
			if (o is GridSortKey gridSortKey)
			{
				return gridSortKey.GetParentSortKeys().GetOwnerTable();
			}
			if (o is GridGroupHeaderTotal gridGroupHeaderTotal)
			{
				return gridGroupHeaderTotal.GetEffectiveTable();
			}
			if (o is GridCustomGroup gridCustomGroup)
			{
				return gridCustomGroup.GetOwnerTable();
			}
			if (o is GridCustomGroupRow gridCustomGroupRow)
			{
				return gridCustomGroupRow.GetOwnerTable();
			}
			if (o is GridStoredFilter gridStoredFilter)
			{
				return gridStoredFilter.GetEffectiveTable();
			}
			if (o is IConditionOwner conditionOwner)
			{
				return conditionOwner.Table as GridTable;
			}
			return null;
		}

		public static ImageList GetImageList(MxDataGrid grid)
		{
			return grid?.ComputeImageList();
		}

		public static bool IsInFormatStylesCollection(GridFormatStyle formatStyle)
		{
			return formatStyle.IsInCollection();
		}

		public static int GetSnapLinesOffset(EditBase edit)
		{
			return edit.GetBaselineOffset();
		}

		public static Control CreateDataSourceControl(IContainer dataSourceContainer, object dataSource, string dataMember, bool dataMemberOnly)
		{
			DataSourceEditorControl dataSourceEditorControl = new DataSourceEditorControl();
			dataSourceEditorControl.SetTreeOnlyMode(dataMemberOnly);
			dataSourceEditorControl.UpdateIContainer(dataSourceContainer);
			dataSourceEditorControl.SetDataSource(dataSource);
			dataSourceEditorControl.SetDataMember(dataMember);
			return dataSourceEditorControl;
		}
	}
}
