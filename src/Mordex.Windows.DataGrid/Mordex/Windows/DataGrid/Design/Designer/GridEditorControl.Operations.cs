using Mordex.Windows.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;
using Microsoft.Win32;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	internal partial class GridEditorControl
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private bool ShouldGroupProperties()
		{
			if (_collectionType == CollectionType.Columns)
			{
				try
				{
					RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\DataGrid Control");
					string text = (string)registryKey.GetValue("GroupPropertiesColumns", "TRUE");
					registryKey.Close();
					if (text == "TRUE")
					{
						return true;
					}
					return false;
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					return true;
				}
			}
			return false;
		}

		private void SaveGroupPropertiesSetting(bool condition)
		{
			if (_collectionType == CollectionType.Columns)
			{
				try
				{
					RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\DataGrid Control");
					registryKey.SetValue(value: (!condition) ? "FALSE" : "TRUE", name: "GroupPropertiesColumns");
					registryKey.Close();
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
		}

		void IGridElement.SetSite(ISite site)
		{
		}

		private bool ValidateSortKeys()
		{
			GridTable gridTable = ((GridSortKeyCollection)_selectedCollection).GetOwnerTable();
			for (int i = 0; i < _arrayList.Count; i++)
			{
				GridSortKey gridSortKey = (GridSortKey)_arrayList[i];
				if (!gridSortKey.BelongsToTable(gridTable))
				{
					_itemListBox.SelectedIndex = i;
					MessageBox.Show("One or more SortKeys are not valid.\n\rColumn property must be set in the object to be valid.", "Mordex DataGrid Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitSortKeys()
		{
			if (!ValidateSortKeys())
			{
				return false;
			}
			GridSortKeyCollection gridSortKeyCollection = (GridSortKeyCollection)_selectedCollection;
			((ISupportInitialize)gridSortKeyCollection.ComputeDataGrid()).BeginInit();
			gridSortKeyCollection.Clear();
			GridSortKey[] array = new GridSortKey[_arrayList.Count];
			_arrayList.CopyTo(array);
			GridSortKey[] otherArray = array;
			foreach (GridSortKey gridSortKey in otherArray)
			{
				gridSortKey.SetBaseParentCollection(null);
			}
			gridSortKeyCollection.AddRange(array);
			((ISupportInitialize)gridSortKeyCollection.ComputeDataGrid()).EndInit();
			return true;
		}

		private bool ValidateGroupHeaderTotals()
		{
			GridTable table = ((GridGroupHeaderTotalCollection)_selectedCollection).Table;
			for (int i = 0; i < _arrayList.Count; i++)
			{
				GridGroupHeaderTotal gridGroupHeaderTotal = (GridGroupHeaderTotal)_arrayList[i];
				try
				{
					gridGroupHeaderTotal.ValidateForTable(table);
				}
				catch (Exception ex)
				{
					_itemListBox.SelectedIndex = i;
					MessageBox.Show(ex.Message, GetItemDisplayText(i, gridGroupHeaderTotal), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitGroupHeaderTotals()
		{
			if (!ValidateGroupHeaderTotals())
			{
				return false;
			}
			GridGroupHeaderTotalCollection gridGroupHeaderTotalCollection = (GridGroupHeaderTotalCollection)_selectedCollection;
			((ISupportInitialize)gridGroupHeaderTotalCollection.Table.DataGrid).BeginInit();
			gridGroupHeaderTotalCollection.Clear();
			GridGroupHeaderTotal[] array = new GridGroupHeaderTotal[_arrayList.Count];
			_arrayList.CopyTo(array);
			GridGroupHeaderTotal[] otherArray = array;
			foreach (GridGroupHeaderTotal gridGroupHeaderTotal in otherArray)
			{
				gridGroupHeaderTotal._ownerTable = null;
			}
			gridGroupHeaderTotalCollection.AddRange(array);
			((ISupportInitialize)gridGroupHeaderTotalCollection.Table.DataGrid).EndInit();
			return true;
		}

		private bool ValidateStoredFilters()
		{
			GridTable gridTable = ((GridStoredFilterCollection)_selectedCollection).GetOwnerTable();
			for (int i = 0; i < _arrayList.Count; i++)
			{
				GridStoredFilter gridStoredFilter = (GridStoredFilter)_arrayList[i];
				if (gridStoredFilter.FilterCondition != null)
				{
					try
					{
						gridStoredFilter.FilterCondition.ValidateBaseCondition(gridTable);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message, "Mordex DataGrid Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						_itemListBox.SelectedIndex = i;
						return false;
					}
				}
			}
			return true;
		}

		private bool CommitStoredFilters()
		{
			if (!ValidateStoredFilters())
			{
				return false;
			}
			GridStoredFilterCollection gridStoredFilterCollection = (GridStoredFilterCollection)_selectedCollection;
			GridStoredFilter gridStoredFilter = null;
			if (gridStoredFilterCollection.GetOwnerTable().FilterCondition is GridStoredFilter)
			{
				gridStoredFilter = (GridStoredFilter)gridStoredFilterCollection.GetOwnerTable().FilterCondition;
			}
			((ISupportInitialize)gridStoredFilterCollection.GetOwnerTable().DataGrid).BeginInit();
			gridStoredFilterCollection.Clear();
			GridStoredFilter[] array = new GridStoredFilter[_arrayList.Count];
			_arrayList.CopyTo(array);
			GridStoredFilter[] otherArray = array;
			foreach (GridStoredFilter storedFilter in otherArray)
			{
				storedFilter._ownerTable = null;
			}
			gridStoredFilterCollection.AddRange(array);
			if (gridStoredFilter != null && gridStoredFilterCollection.GetOwnerTable().FilterCondition == null && gridStoredFilter.GetParentFilters() == gridStoredFilterCollection)
			{
				gridStoredFilterCollection.GetOwnerTable().FilterCondition = gridStoredFilter;
			}
			((ISupportInitialize)gridStoredFilterCollection.GetOwnerTable().DataGrid).EndInit();
			return true;
		}

		private bool CommitDropDowns()
		{
			GridDropDownCollection gridDropDownCollection = (GridDropDownCollection)_selectedCollection;
			((ISupportInitialize)gridDropDownCollection.DataGrid).BeginInit();
			for (int index = gridDropDownCollection.Count - 1; index >= 0; index--)
			{
				gridDropDownCollection.RemoveAt(index);
			}
			GridDropDown[] array = new GridDropDown[_arrayList.Count];
			_arrayList.CopyTo(array);
			gridDropDownCollection.AddRange(array);
			((ISupportInitialize)gridDropDownCollection.DataGrid).EndInit();
			return true;
		}

		bool IGridElement.Support(ToolbarOptions option)
		{
			bool support = (option & _toolbarOptions) != 0;
			if (support)
			{
				switch (option)
				{
				case ToolbarOptions.Clear:
					if (_arrayList.Count == 0)
					{
						support = false;
					}
					break;
				case ToolbarOptions.MoveDown:
					if (_itemListBox.SelectedIndex == _itemListBox.Items.Count - 1)
					{
						support = false;
					}
					break;
				case ToolbarOptions.MoveUp:
					if (_itemListBox.SelectedIndex < 1)
					{
						support = false;
					}
					break;
				case ToolbarOptions.Remove:
					if (_itemListBox.SelectedIndex == -1)
					{
						support = false;
					}
					break;
				}
			}
			return support;
		}

		private void UpdateToolbarState()
		{
			if (_properties.SelectedObject == null)
			{
				_tbtnClear.Enabled = false;
				_tbtnMoveDown.Enabled = false;
				_tbtnMoveUp.Enabled = false;
				_tbtnRemove.Enabled = false;
				_tbtnValueList.Enabled = false;
				return;
			}
			_tbtnClear.Enabled = _arrayList.Count > 0;
			_tbtnMoveDown.Enabled = _itemListBox.SelectedIndex < _itemListBox.Items.Count - 1;
			_tbtnMoveUp.Enabled = _itemListBox.SelectedIndex > 0;
			_tbtnRemove.Enabled = _itemListBox.SelectedIndex != -1;
			if (_properties.SelectedObject is GridColumn && ((GridColumn)_properties.SelectedObject).HasValueList)
			{
				_tbtnValueList.Enabled = true;
			}
			else
			{
				_tbtnValueList.Enabled = false;
			}
		}

		private void RaiseDataChangedEvent(EventArgs eventArgs)
		{
			if (base.Events[_gridEditorControlValue] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void ProcessEventArgs(EventArgs eventArgs)
		{
			UpdateToolbarState();
			if (base.Events[_dataChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void SelectItemByValue(object value)
		{
			for (int i = 0; i < _arrayList.Count; i++)
			{
				if (_arrayList[i] == value)
				{
					_itemListBox.SelectedIndex = i;
					break;
				}
			}
		}

		private void AddAndSelectItem(object value)
		{
			_arrayList.Add(value);
			RefreshList();
			SelectItemByValue(value);
		}

		private void AddGroupHeaderTotalItem()
		{
			GridGroupHeaderTotal gridGroupHeaderTotal = new GridGroupHeaderTotal();
			gridGroupHeaderTotal._ownerTable = ((GridGroupHeaderTotalCollection)_selectedCollection).Table;
			gridGroupHeaderTotal.Key = GenerateUniqueKey(typeof(GridGroupHeaderTotal), "GroupHeaderTotal", 1);
			AddAndSelectItem(gridGroupHeaderTotal);
		}

		private void AddStoredFilterItem()
		{
			GridStoredFilter gridStoredFilter = new GridStoredFilter();
			gridStoredFilter.Key = GenerateUniqueKey(typeof(GridStoredFilter), "StoredFilter", 1);
			gridStoredFilter._ownerTable = ((GridStoredFilterCollection)_selectedCollection).GetOwnerTable();
			AddAndSelectItem(gridStoredFilter);
		}

		private string GenerateUniqueKey(Type componentType, string otherText, int value)
		{
			string text = "";
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(componentType).Find("Key", ignoreCase: true);
			if (propertyDescriptor != null)
			{
				text = otherText + value;
				foreach (object item in _arrayList)
				{
					if (string.Compare((string)propertyDescriptor.GetValue(item), text, ignoreCase: true) == 0)
					{
						return GenerateUniqueKey(componentType, otherText, value + 1);
					}
				}
			}
			return text;
		}

		private void AddDropDownItem()
		{
			try
			{
				GridDropDown gridDropDown = new GridDropDown();
				gridDropDown.Key = GenerateUniqueKey(typeof(GridDropDown), "DropDown", 1);
				((IList)_selectedCollection).Add(gridDropDown);
				MultiColumnComboConfigurationWizardDialog multiColumnComboConfigurationWizardDialog = new MultiColumnComboConfigurationWizardDialog();
				if (multiColumnComboConfigurationWizardDialog.ComputeDialogResult(gridDropDown, (multiColumnComboConfigurationWizardDialog._dataGrid = ((GridDropDownCollection)_selectedCollection).DataGrid).Container) == DialogResult.OK)
				{
					AddAndSelectItem(gridDropDown);
					if (multiColumnComboConfigurationWizardDialog._gridColumn != null && multiColumnComboConfigurationWizardDialog._gridColumn.EditType != EditType.MultiColumnCombo && multiColumnComboConfigurationWizardDialog._gridColumn.EditType != EditType.MultiColumnDropDown)
					{
						multiColumnComboConfigurationWizardDialog._gridColumn.EditType = EditType.MultiColumnCombo;
						multiColumnComboConfigurationWizardDialog._gridColumn.CompareTarget = ColumnCompareTarget.Text;
						multiColumnComboConfigurationWizardDialog._gridColumn.DefaultGroupInterval = GroupInterval.Text;
						multiColumnComboConfigurationWizardDialog._gridColumn.DropDown = gridDropDown;
					}
					CommitDropDowns();
					RaiseDataChangedEvent(EventArgs.Empty);
					ProcessEventArgs(EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void AddSortKeyItem()
		{
			GridSortKey gridSortKey = new GridSortKey();
			gridSortKey.SetBaseParentCollection((GridSortKeyCollection)_selectedCollection);
			AddAndSelectItem(gridSortKey);
		}

		private void AddCustomGroupRowItem()
		{
			GridCustomGroupRow gridCustomGroupRow = new GridCustomGroupRow();
			gridCustomGroupRow._ownerTable = ((GridCustomGroupRowCollection)_selectedCollection).GetOwnerTable();
			AddAndSelectItem(gridCustomGroupRow);
		}

		private void InitializeComponent()
		{
			_iContainer = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(GridEditorControl));
			_tbOperations = new MxToolBar();
			_tbtnGroupProperties = new MxToolBarButton();
			_separatorThird = new MxToolBarButton();
			_tbtnAdd = new MxToolBarButton();
			_tbtnRemove = new MxToolBarButton();
			_tbtnClear = new MxToolBarButton();
			_separatorFirst = new MxToolBarButton();
			_tbtnMoveUp = new MxToolBarButton();
			_tbtnMoveDown = new MxToolBarButton();
			_separatorSecond = new MxToolBarButton();
			_tbtnValueList = new MxToolBarButton();
			_tbtnColumnSets = new MxToolBarButton();
			_imageList = new ImageList(_iContainer);
			_itemListBox = new ListBox();
			_listSplitter = new Splitter();
			_properties = new PropertyGrid();
			SuspendLayout();
			_tbOperations.Appearance = MxToolBarAppearance.Flat;
			_tbOperations.Buttons.AddRange(new MxToolBarButton[11]
			{
				_tbtnGroupProperties, _separatorThird, _tbtnAdd, _tbtnRemove, _tbtnClear, _separatorFirst, _tbtnMoveUp, _tbtnMoveDown, _separatorSecond, _tbtnValueList,
				_tbtnColumnSets
			});
			_tbOperations.Divider = false;
			_tbOperations.DropDownArrows = true;
			_tbOperations.ImageList = _imageList;
			_tbOperations.Location = new Point(0, 0);
			_tbOperations.Name = "tbOperations";
			_tbOperations.ShowToolTips = true;
			_tbOperations.Size = new Size(634, 26);
			_tbOperations.TabIndex = 3;
			_tbOperations.TextAlign = MxToolBarTextAlign.Right;
			_tbOperations.ButtonClick += OnTbOperationsButtonClick;
			_tbtnGroupProperties.ImageIndex = 7;
			_tbtnGroupProperties.Name = "tbtnGroupProperties";
			_tbtnGroupProperties.Pushed = true;
			_tbtnGroupProperties.Style = MxToolBarButtonStyle.ToggleButton;
			_tbtnGroupProperties.Tag = "GroupProps";
			_separatorThird.Name = "ToolSep3";
			_separatorThird.Style = MxToolBarButtonStyle.Separator;
			_tbtnAdd.ImageIndex = 0;
			_tbtnAdd.Name = "tbtnAdd";
			_tbtnAdd.Tag = "Add";
			_tbtnAdd.Text = "Add";
			_tbtnRemove.ImageIndex = 1;
			_tbtnRemove.Name = "tbtnRemove";
			_tbtnRemove.Tag = "Remove";
			_tbtnRemove.Text = "Remove";
			_tbtnClear.ImageIndex = 2;
			_tbtnClear.Name = "tbtnClear";
			_tbtnClear.Tag = "Clear";
			_tbtnClear.Text = "Clear";
			_separatorFirst.Name = "toolSep1";
			_separatorFirst.Style = MxToolBarButtonStyle.Separator;
			_tbtnMoveUp.ImageIndex = 3;
			_tbtnMoveUp.Name = "tbtnMoveUp";
			_tbtnMoveUp.Tag = "Up";
			_tbtnMoveDown.ImageIndex = 4;
			_tbtnMoveDown.Name = "tbtnMoveDown";
			_tbtnMoveDown.Tag = "Down";
			_separatorSecond.Name = "toolSep2";
			_separatorSecond.Style = MxToolBarButtonStyle.Separator;
			_tbtnValueList.Enabled = false;
			_tbtnValueList.ImageIndex = 5;
			_tbtnValueList.Name = "tbtnValueList";
			_tbtnValueList.Tag = "ValueList";
			_tbtnValueList.Text = "ValueList...";
			_tbtnColumnSets.ImageIndex = 6;
			_tbtnColumnSets.Name = "tbtnColumnSets";
			_tbtnColumnSets.Tag = "ColumnSets";
			_tbtnColumnSets.Text = "ColumnSet Designer...";
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			_imageList.Images.SetKeyName(2, "");
			_imageList.Images.SetKeyName(3, "");
			_imageList.Images.SetKeyName(4, "");
			_imageList.Images.SetKeyName(5, "");
			_imageList.Images.SetKeyName(6, "");
			_imageList.Images.SetKeyName(7, "");
			_itemListBox.Dock = DockStyle.Left;
			_itemListBox.DrawMode = DrawMode.OwnerDrawFixed;
			_itemListBox.FormattingEnabled = true;
			_itemListBox.IntegralHeight = false;
			_itemListBox.Location = new Point(0, 26);
			_itemListBox.Name = "list";
			_itemListBox.Size = new Size(175, 382);
			_itemListBox.TabIndex = 4;
			_itemListBox.DrawItem += OnListDrawItem;
			_itemListBox.Resize += OnListResize;
			_itemListBox.SelectedIndexChanged += OnListSelectedIndexChanged;
			_listSplitter.BackColor = SystemColors.Control;
			_listSplitter.Location = new Point(175, 26);
			_listSplitter.Name = "splitter1";
			_listSplitter.Size = new Size(8, 382);
			_listSplitter.TabIndex = 6;
			_listSplitter.TabStop = false;
			_properties.Dock = DockStyle.Fill;
			_properties.LineColor = SystemColors.Control;
			_properties.Location = new Point(183, 26);
			_properties.Name = "properties";
			_properties.Size = new Size(451, 382);
			_properties.TabIndex = 7;
			_properties.ToolbarVisible = false;
			_properties.PropertyValueChanged += OnPropertiesPropertyValueChanged;
			base.Controls.Add(_properties);
			base.Controls.Add(_listSplitter);
			base.Controls.Add(_itemListBox);
			base.Controls.Add(_tbOperations);
			base.Name = "CollectionPage";
			base.Size = new Size(634, 408);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		private void OnTbOperationsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			string key;
			if (eventArgs.Button.Tag != null && (key = (string)eventArgs.Button.Tag) != null)
			{
				if (StringSwitchCache._dictionary == null)
				{
					StringSwitchCache._dictionary = new Dictionary<string, int>(8)
					{
						{
							"GroupProps",
							0
						},
						{
							"Add",
							1
						},
						{
							"Remove",
							2
						},
						{
							"Clear",
							3
						},
						{
							"Up",
							4
						},
						{
							"Down",
							5
						},
						{
							"ValueList",
							6
						},
						{
							"ColumnSets",
							7
						}
					};
				}
				if (StringSwitchCache._dictionary.TryGetValue(key, out var value))
				{
					switch (value)
					{
					case 0:
						if (eventArgs.Button.Pushed)
						{
							_properties.PropertySort = PropertySort.CategorizedAlphabetical;
							SaveGroupPropertiesSetting(true);
						}
						else
						{
							_properties.PropertySort = PropertySort.Alphabetical;
							SaveGroupPropertiesSetting(false);
						}
						break;
					case 1:
						AddItem();
						break;
					case 2:
						RemoveSelectedItem();
						break;
					case 3:
						ClearItems();
						break;
					case 4:
						MoveSelectedItemUp();
						break;
					case 5:
						MoveSelectedItemDown();
						break;
					case 6:
						EditValueList();
						break;
					case 7:
						EditColumnSets();
						break;
					}
				}
			}
			OnDataChanged(EventArgs.Empty);
		}

		private void OnListDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			eventArgs.DrawBackground();
			if (eventArgs.Index > -1)
			{
				int value = eventArgs.Bounds.X + 2;
				if (_itemImage != null)
				{
					eventArgs.Graphics.DrawImageUnscaled(_itemImage, value, eventArgs.Bounds.Top + (eventArgs.Bounds.Height - _itemImage.Height) / 2);
					value += _itemImage.Width + 2;
				}
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: true);
				Rectangle rectangle = new Rectangle(value, eventArgs.Bounds.Y, eventArgs.Bounds.Right - value, eventArgs.Bounds.Height);
				string text = ((GridValueListItem)_itemListBox.Items[eventArgs.Index]).Text;
				if (!GridHelper.IsTextLengthValid(text))
				{
					text = GridHelper.TruncateText(text);
				}
				textLayoutRenderer.DrawTextWithColor(eventArgs.Graphics, text, rectangle, _itemListBox.Font, eventArgs.ForeColor);
				textLayoutRenderer.ReleaseStringFormat();
			}
			if ((eventArgs.State & DrawItemState.Focus) != DrawItemState.None)
			{
				eventArgs.DrawFocusRectangle();
			}
		}

		private void OnListResize(object sender, EventArgs eventArgs)
		{
			_itemListBox.Refresh();
		}

		private void OnListSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_itemListBox.SelectedIndex == -1)
			{
				_properties.SelectedObject = null;
			}
			else
			{
				_properties.SelectedObject = ((GridValueListItem)_itemListBox.SelectedItem).Value;
			}
			ProcessEventArgs(EventArgs.Empty);
		}
	}
}
