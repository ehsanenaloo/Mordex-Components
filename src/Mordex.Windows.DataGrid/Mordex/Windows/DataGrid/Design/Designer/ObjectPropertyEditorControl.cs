using Mordex.Windows.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	[ToolboxItem(false)]
	internal class ObjectPropertyEditorControl : UserControl, IGridElement
	{
		private IContainer _iContainer;

		private EventHandler _dataChangedHandler;

		private MxToolBar _tbCommands;

		private PropertyGrid _properties;

		private ImageList _imageList;

		private object _editedObject;

		private ToolbarOptions _toolbarOptions = default;

		private static object _dataChangedEventKey = new object();

		private static object _gridEditorControl3Value = new object();

		object IGridElement.SelectedObject
		{
			get
			{
				return _editedObject;
			}
			set
			{
				_editedObject = value;
				ShowObjectProperties();
			}
		}

		Image IGridElement.ItemImage
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		event EventHandler IGridElement.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(_dataChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dataChangedEventKey, value);
			}
		}

		event EventHandler IGridElement.RefreshTree
		{
			add
			{
				base.Events.AddHandler(_gridEditorControl3Value, value);
			}
			remove
			{
				base.Events.RemoveHandler(_gridEditorControl3Value, value);
			}
		}

		internal ObjectPropertyEditorControl()
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ObjectPropertyEditorControl));
			_tbCommands = new MxToolBar();
			_imageList = new ImageList(_iContainer);
			_properties = new PropertyGrid();
			SuspendLayout();
			_tbCommands.Appearance = MxToolBarAppearance.Flat;
			_tbCommands.Divider = false;
			_tbCommands.DropDownArrows = true;
			_tbCommands.ImageList = _imageList;
			_tbCommands.Location = new Point(0, 0);
			_tbCommands.Name = "tbCommands";
			_tbCommands.ShowToolTips = true;
			_tbCommands.Size = new Size(456, 26);
			_tbCommands.TabIndex = 0;
			_tbCommands.TextAlign = MxToolBarTextAlign.Right;
			_tbCommands.ButtonClick += OnTbCommandsButtonClick;
			ImageStrip.Load(_imageList, componentResourceManager, "imageList1.ImageStrip");
			_imageList.TransparentColor = Color.Magenta;
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			_imageList.Images.SetKeyName(2, "");
			_imageList.Images.SetKeyName(3, "");
			_imageList.Images.SetKeyName(4, "selfreferencingwizard");
			_properties.Dock = DockStyle.Fill;
			_properties.LineColor = SystemColors.Control;
			_properties.Location = new Point(0, 26);
			_properties.Name = "properties";
			_properties.PropertySort = PropertySort.Alphabetical;
			_properties.Size = new Size(456, 366);
			_properties.TabIndex = 1;
			_properties.ToolbarVisible = false;
			_properties.PropertyValueChanged += OnPropertiesPropertyValueChanged;
			base.Controls.Add(_properties);
			base.Controls.Add(_tbCommands);
			base.Name = "ObjectDesigner";
			base.Size = new Size(456, 392);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		void IGridElement.SetSite(ISite site)
		{
		}

		private void ShowObjectProperties()
		{
			_properties.SelectedObject = _editedObject;
			_tbCommands.Visible = false;
			_tbCommands.Buttons.Clear();
			MxToolBarButton[] array = null;
			if (_editedObject is GridDropDown)
			{
				array = new MxToolBarButton[1]
				{
					new MxToolBarButton("Retrieve Structure")
				};
				array[0].Tag = "RetrieveDropDownStructure";
				array[0].ImageIndex = 1;
				_tbCommands.Buttons.AddRange(array);
				_tbCommands.Visible = true;
			}
			else if (_editedObject is GridSelfReferencingSettings)
			{
				array = new MxToolBarButton[1]
				{
					new MxToolBarButton("SelfReferencing Wizard...")
				};
				array[0].Tag = "SelfReferencing";
				array[0].ImageKey = "SelfReferencingWizard";
				_tbCommands.Buttons.AddRange(array);
				_tbCommands.Visible = true;
			}
			else if (_editedObject is MxDataGrid)
			{
				array = new MxToolBarButton[4]
				{
					new MxToolBarButton("Create Root Table"),
					null,
					null,
					null
				};
				array[0].Tag = "NewRootTable";
				array[0].ImageIndex = 0;
				if (((MxDataGrid)_editedObject).RootTable != null)
				{
					array[0].Enabled = false;
				}
				array[1] = new MxToolBarButton("Reset Properties");
				array[1].Tag = "ResetGridProperties";
				array[1].ImageIndex = 2;
				array[2] = new MxToolBarButton("Retrieve Structure");
				array[2].Tag = "RetrieveFlatStructure";
				array[2].ImageIndex = 1;
				array[3] = new MxToolBarButton("Retrieve Hierarchical Structure");
				array[3].Tag = "RetrieveHierarchicalStructure";
				array[3].ImageIndex = 1;
				if (((MxDataGrid)_editedObject).DataSource == null)
				{
					array[2].Enabled = false;
					array[3].Enabled = false;
				}
				_tbCommands.Buttons.AddRange(array);
				_tbCommands.Visible = true;
			}
			else if (_editedObject is GridTable)
			{
				if (((GridTable)_editedObject).IsRootTable())
				{
					array = new MxToolBarButton[3]
					{
						null,
						null,
						new MxToolBarButton("Remove Root Table")
					};
					array[2].Tag = "RemoveRootTable";
					array[2].ImageIndex = 3;
				}
				else
				{
					array = new MxToolBarButton[2];
				}
				array[0] = new MxToolBarButton("Retrieve Columns");
				array[0].Tag = "RetrieveTableStructure";
				array[0].ImageIndex = 1;
				array[1] = new MxToolBarButton("Retrieve Child Tables");
				array[1].Tag = "RetrieveChildTables";
				array[1].ImageIndex = 1;
				if (((GridTable)_editedObject).DataGrid.DataSource == null)
				{
					array[0].Enabled = false;
					array[1].Enabled = false;
				}
				_tbCommands.Buttons.AddRange(array);
				_tbCommands.Visible = true;
			}
		}

		bool IGridElement.CommitChanges()
		{
			return true;
		}

		bool IGridElement.Support(ToolbarOptions option)
		{
			return (_toolbarOptions & option) != 0;
		}

		private void OnTbCommandsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			string key;
			if ((key = (string)eventArgs.Button.Tag) == null)
			{
				return;
			}
			if (StringSwitchCache._stringSwitchCacheDictionary == null)
			{
				StringSwitchCache._stringSwitchCacheDictionary = new Dictionary<string, int>(9)
				{
					{
						"NewRootTable",
						0
					},
					{
						"ResetGridProperties",
						1
					},
					{
						"RetrieveFlatStructure",
						2
					},
					{
						"RetrieveHierarchicalStructure",
						3
					},
					{
						"RetrieveTableStructure",
						4
					},
					{
						"RetrieveChildTables",
						5
					},
					{
						"RemoveRootTable",
						6
					},
					{
						"RetrieveDropDownStructure",
						7
					},
					{
						"SelfReferencing",
						8
					}
				};
			}
			if (!StringSwitchCache._stringSwitchCacheDictionary.TryGetValue(key, out var value))
			{
				return;
			}
			switch (value)
			{
			case 0:
				((MxDataGrid)_editedObject).RootTable = new GridTable();
				OnDataChanged(EventArgs.Empty);
				RaiseDataChanged();
				break;
			case 1:
				if (MessageBox.Show("Are you sure you want to loose all the changes in the properties of the control?", "DataGrid Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{
					((MxDataGrid)_editedObject).Reset();
					OnDataChanged(EventArgs.Empty);
					RaiseDataChanged();
				}
				break;
			case 2:
				ProcessDataGrid((MxDataGrid)_editedObject, false);
				OnDataChanged(EventArgs.Empty);
				break;
			case 3:
				ProcessDataGrid((MxDataGrid)_editedObject, true);
				OnDataChanged(EventArgs.Empty);
				break;
			case 4:
				RetrieveTableFields((GridTable)_editedObject);
				OnDataChanged(EventArgs.Empty);
				break;
			case 5:
				RetrieveChildTables((GridTable)_editedObject);
				OnDataChanged(EventArgs.Empty);
				break;
			case 6:
				((GridTable)_editedObject).DataGrid.RootTable = null;
				OnDataChanged(EventArgs.Empty);
				RaiseDataChanged();
				break;
			case 7:
				RetrieveDropDownStructure((GridDropDown)_editedObject);
				OnDataChanged(EventArgs.Empty);
				break;
			case 8:
			{
				SelfReferencingSettingsDialog selfReferencingSettingsDialog = new SelfReferencingSettingsDialog();
				if (selfReferencingSettingsDialog.ComputeDialogResult((GridSelfReferencingSettings)_editedObject) == DialogResult.OK)
				{
					_properties.SelectedObject = null;
					_properties.SelectedObject = _editedObject;
					OnDataChanged(EventArgs.Empty);
				}
				break;
			}
			}
		}

		private void RetrieveDropDownStructure(GridDropDown gridDropDown)
		{
			if (gridDropDown.Columns.Count > 0 && MessageBox.Show("This operation will overwrite the column layout you currently have. \n\r\n\rDo you want to continue?", "Mordex DataGrid Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.No)
			{
				return;
			}
			DataSourceDialog dataSourceDialog = new DataSourceDialog();
			dataSourceDialog.Owner = base.ParentForm;
			if (gridDropDown.DropDownCollection != null)
			{
				dataSourceDialog.ProcessDataGrid(gridDropDown.DropDownCollection.DataGrid, gridDropDown.DataSource, gridDropDown.DataMember);
			}
			if (gridDropDown.DropDownCollection == null || dataSourceDialog.DialogResult == DialogResult.OK)
			{
				if (gridDropDown.DropDownCollection != null)
				{
					gridDropDown.SetDataBinding(dataSourceDialog.GetSelectedDataSource(), dataSourceDialog.GetSelectedDataMember());
				}
				if (gridDropDown._gridDropDownOwner != null)
				{
					gridDropDown._gridDropDownOwner.RetrieveStructure();
				}
				else
				{
					gridDropDown.RetrieveStructure(retrieveChildTables: false);
				}
				RaiseDataChangedEvent(EventArgs.Empty);
			}
		}

		private void RetrieveTableFields(GridTable gridTable)
		{
			if (gridTable.Columns.Count <= 0 || MessageBox.Show("This operation will overwrite the column layout you currently have. \n\r\n\rDo you want to continue?", "Mordex DataGrid Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.No)
			{
				gridTable.RetrieveBaseFields();
				RaiseDataChanged();
			}
		}

		private void RetrieveChildTables(GridTable gridTable)
		{
			if (gridTable.ChildTables.Count <= 0 || MessageBox.Show("This operation will overwrite the child tables collection you currently have. \n\r\n\rDo you want to continue?", "Mordex DataGrid Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.No)
			{
				gridTable.RetrieveBaseChildTables();
				RaiseDataChanged();
			}
		}

		private void ProcessDataGrid(MxDataGrid dataGrid, bool retrieveChildTables)
		{
			if (dataGrid.RootTable == null || MessageBox.Show("This operation will overwrite the Table-Column layout you currently have. \n\r\n\rDo you want to continue?", "Mordex DataGrid Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.No)
			{
				dataGrid.RetrieveStructure(retrieveChildTables);
				dataGrid.Hierarchical = retrieveChildTables;
				RaiseDataChanged();
			}
		}

		private void RaiseDataChanged()
		{
			RaiseDataChangedEvent(EventArgs.Empty);
		}

		private void RaiseDataChangedEvent(EventArgs eventArgs)
		{
			if (base.Events[_gridEditorControl3Value] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			if (_editedObject is MxDataGrid && !(_editedObject is GridDropDown))
			{
				if (((MxDataGrid)_editedObject).DataSource == null)
				{
					_tbCommands.Buttons[2].Enabled = false;
					_tbCommands.Buttons[3].Enabled = false;
				}
				else
				{
					_tbCommands.Buttons[2].Enabled = true;
					_tbCommands.Buttons[3].Enabled = true;
				}
			}
			OnDataChanged(EventArgs.Empty);
		}
	}
}
