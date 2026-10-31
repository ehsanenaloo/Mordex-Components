using Mordex.Windows.Common;
using System;
using Mordex.Windows.DataGrid.Internal;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	[ToolboxItem(false)]
	internal class GridEditorDialog : Form
	{
		private struct GridEditorDialogData
		{
			internal int[] _layoutPath;

			internal object _dataSource;

			internal string _dataMember;
		}

		private MemoryStream _memoryStream;

		private IContainer _iContainer;

		private Panel _panel;

		private TabControl _editorTabs;

		private TabPage _tabGeneral;

		private TabPage _tabLayouts;

		private Button _btnOK;

		private Button _btnCancel;

		private ImageList _imageList;

		private Button _btnApply;

		private MxDataGrid _dataGrid;

		private TabPage _tabPage;

		private IDesignerPage _activeEditor;

		private bool _isLayoutModified;

		private bool _isApplied;

		private ArrayList _arrayList;

		internal GridEditorDialog()
		{
			InitializeControls();
			base.AutoScaleMode = AutoScaleMode.None;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(GridEditorDialog));
			_panel = new Panel();
			_btnApply = new Button();
			_btnCancel = new Button();
			_btnOK = new Button();
			_editorTabs = new TabControl();
			_tabGeneral = new TabPage();
			_tabLayouts = new TabPage();
			_imageList = new ImageList(_iContainer);
			_panel.SuspendLayout();
			_editorTabs.SuspendLayout();
			SuspendLayout();
			_panel.Controls.Add(_btnApply);
			_panel.Controls.Add(_btnCancel);
			_panel.Controls.Add(_btnOK);
			_panel.Dock = DockStyle.Bottom;
			_panel.Location = new Point(4, 350);
			_panel.Name = "panel1";
			_panel.Size = new Size(640, 44);
			_panel.TabIndex = 2;
			_btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnApply.Enabled = false;
			_btnApply.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_btnApply.Location = new Point(560, 12);
			_btnApply.Name = "btnApply";
			_btnApply.Size = new Size(80, 24);
			_btnApply.TabIndex = 2;
			_btnApply.Text = "Apply";
			_btnApply.Click += OnBtnApplyClick;
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(476, 12);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 1;
			_btnCancel.Text = "Cancel";
			_btnCancel.Click += OnBtnCancelClick;
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(392, 12);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 24);
			_btnOK.TabIndex = 0;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			_editorTabs.Controls.Add(_tabGeneral);
			_editorTabs.Controls.Add(_tabLayouts);
			_editorTabs.Dock = DockStyle.Fill;
			_editorTabs.ImageList = _imageList;
			_editorTabs.Location = new Point(4, 4);
			_editorTabs.Multiline = true;
			_editorTabs.Name = "tabControl1";
			_editorTabs.SelectedIndex = 0;
			_editorTabs.Size = new Size(640, 346);
			_editorTabs.TabIndex = 3;
			_editorTabs.SelectedIndexChanged += OnTabControl1SelectedIndexChanged;
			_tabGeneral.ImageIndex = 0;
			_tabGeneral.Location = new Point(4, 23);
			_tabGeneral.Name = "tabGeneral";
			_tabGeneral.Size = new Size(632, 319);
			_tabGeneral.TabIndex = 0;
			_tabGeneral.Text = "General Properties";
			_tabLayouts.ImageIndex = 1;
			_tabLayouts.Location = new Point(4, 23);
			_tabLayouts.Name = "tabLayouts";
			_tabLayouts.Size = new Size(632, 319);
			_tabLayouts.TabIndex = 1;
			_tabLayouts.Text = "Layout Manager";
			_tabLayouts.Visible = false;
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			base.ClientSize = new Size(648, 398);
			base.Controls.Add(_editorTabs);
			base.Controls.Add(_panel);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "GridDesignerDialog";
			base.Padding = new Padding(4);
			_panel.ResumeLayout(performLayout: false);
			_editorTabs.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		internal DialogResult ComputeDialogResult(MxDataGrid dataGrid)
		{
			if (dataGrid == null)
			{
				throw new ArgumentNullException();
			}
			_dataGrid = dataGrid;
			Text = "Mordex DataGrid Control Designer - " + dataGrid.Name;
			UpdateTabPage(_editorTabs.SelectedTab);
			SaveLayoutSnapshot();
			return ShowDialog();
		}

		private void UpdateTabPage(TabPage tabPage)
		{
			if (_tabPage == tabPage)
			{
				return;
			}
			if (_activeEditor != null && !_activeEditor.CommitChanges())
			{
				_editorTabs.SelectedTab = _tabPage;
				return;
			}
			_tabPage = tabPage;
			string name;
			if ((name = _tabPage.Name) != null)
			{
				if (name == "tabLayouts")
				{
					SetActiveEditor(new LayoutEditorControl());
					return;
				}
				if (name == "tabGeneral")
				{
					SetActiveEditor(new MultiColumnComboEditorControl());
					return;
				}
			}
			SetActiveEditor(null);
		}

		private void OnTabControl1SelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			UpdateTabPage(_editorTabs.SelectedTab);
		}

		private IDesignerPage GetActiveEditor()
		{
			return _activeEditor;
		}

		private void SetActiveEditor(IDesignerPage activeEditor)
		{
			if (_activeEditor != activeEditor)
			{
				if (_activeEditor != null)
				{
					_activeEditor.SelectTab -= OnStateSelectTab;
					_activeEditor.DataChanged -= OnStateDataChanged;
					((Control)_activeEditor).Dispose();
				}
				_activeEditor = activeEditor;
				if (_activeEditor != null)
				{
					((Control)_activeEditor).Dock = DockStyle.Fill;
					_activeEditor.SelectTab += OnStateSelectTab;
					_activeEditor.DataChanged += OnStateDataChanged;
					_editorTabs.SelectedTab.Controls.Add((Control)_activeEditor);
					_activeEditor.DataGrid = _dataGrid;
				}
			}
		}

		private void OnStateSelectTab(object sender, EventArgs eventArgs)
		{
			_editorTabs.SelectedTab = _tabGeneral;
		}

		private void OnStateDataChanged(object sender, EventArgs eventArgs)
		{
			_isLayoutModified = true;
			_btnApply.Enabled = true;
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			_isLayoutModified = false;
			Close();
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			base.OnFormClosing(eventArgs);
			if (base.DialogResult != DialogResult.Cancel && _activeEditor != null && !_activeEditor.CommitChanges())
			{
				eventArgs.Cancel = true;
			}
			if (_isLayoutModified)
			{
				switch (MessageBox.Show("Save Changes?", "DataGrid Designer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
				{
				case DialogResult.Cancel:
					eventArgs.Cancel = true;
					break;
				case DialogResult.No:
					RestoreLayoutSnapshot();
					break;
				}
			}
			else if (_isApplied)
			{
				base.DialogResult = DialogResult.OK;
			}
			if (GetActiveEditor() != null)
			{
				GetActiveEditor().Unload();
			}
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\DataGrid Control");
				_ = Rectangle.Empty;
				if (base.WindowState != FormWindowState.Maximized)
				{
					registryKey.SetValue("Width", (int)(base.Width * 96.0 / DeviceDpi + 0.5));
					registryKey.SetValue("Height", (int)(base.Height * 96.0 / DeviceDpi + 0.5));
					registryKey.SetValue("Top", base.Top);
					registryKey.SetValue("Left", base.Left);
				}
				registryKey.SetValue("Window State", base.WindowState);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		protected override void OnLoad(EventArgs eventArgs)
		{
			base.OnLoad(eventArgs);
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\DataGrid Control");
				base.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), (string)registryKey.GetValue("Window State", "Normal"));
				if (base.WindowState == FormWindowState.Minimized)
				{
					base.WindowState = FormWindowState.Normal;
				}
				if (base.WindowState != FormWindowState.Minimized)
				{
					Rectangle empty = Rectangle.Empty;
					empty.Width = LogicalToDeviceUnits((int)registryKey.GetValue("Width", (int)(base.Width * 96.0 / DeviceDpi + 0.5)));
					empty.Height = LogicalToDeviceUnits((int)registryKey.GetValue("Height", (int)(base.Height * 96.0 / DeviceDpi + 0.5)));
					empty.Y = (int)registryKey.GetValue("Top", base.Top);
					empty.X = (int)registryKey.GetValue("Left", base.Left);
					if (empty.Width > SystemInformation.VirtualScreen.Width)
					{
						empty.Width = SystemInformation.VirtualScreen.Width;
					}
					if (empty.Height > SystemInformation.VirtualScreen.Height)
					{
						empty.Height = SystemInformation.VirtualScreen.Height;
					}
					if (empty.Right > SystemInformation.VirtualScreen.Right)
					{
						empty.X = SystemInformation.VirtualScreen.Right - empty.Width;
					}
					if (empty.X < SystemInformation.VirtualScreen.Left)
					{
						empty.X = SystemInformation.VirtualScreen.Left;
					}
					if (empty.Bottom > SystemInformation.VirtualScreen.Bottom)
					{
						empty.Y = SystemInformation.VirtualScreen.Bottom - empty.Height;
					}
					if (empty.Y < SystemInformation.VirtualScreen.Top)
					{
						empty.Y = SystemInformation.VirtualScreen.Top;
					}
					base.Bounds = empty;
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			RestoreLayoutSnapshot();
			Close();
		}

		private void OnBtnApplyClick(object sender, EventArgs eventArgs)
		{
			if (_activeEditor == null || _activeEditor.CommitChanges())
			{
				UpdateMemoryStream();
				_isApplied = true;
			}
		}

		private void RestoreLayoutSnapshot()
		{
			if (_memoryStream == null)
			{
				return;
			}
			_dataGrid.LoadLayoutFile(_memoryStream);
			foreach (GridEditorDialogData item in _arrayList)
			{
				ApplyDataBinding(item);
			}
			UpdateMemoryStream();
		}

		private void ApplyDataBinding(GridEditorDialogData gridEditorDialogData)
		{
			GridLayoutCollection gridLayoutCollection = _dataGrid.Layouts;
			int index = gridEditorDialogData._layoutPath[gridEditorDialogData._layoutPath.Length - 1];
			for (int i = 1; i < gridEditorDialogData._layoutPath.Length; i++)
			{
				gridLayoutCollection = gridLayoutCollection[gridEditorDialogData._layoutPath[i - 1]].ChildLayouts;
			}
			gridLayoutCollection[index]._dataMember = gridEditorDialogData._dataMember;
			gridLayoutCollection[index]._dataSource = gridEditorDialogData._dataSource;
			if (_dataGrid.CurrentLayout == gridLayoutCollection[index])
			{
				_dataGrid.SetDataBinding(gridEditorDialogData._dataSource, gridEditorDialogData._dataMember);
			}
		}

		private void UpdateMemoryStream()
		{
			if (_memoryStream != null)
			{
				_memoryStream.Close();
				_memoryStream = null;
				SaveLayoutSnapshot();
				_btnApply.Enabled = false;
			}
		}

		private void SaveLayoutSnapshot()
		{
			_memoryStream = new MemoryStream();
			_dataGrid.SaveLayoutFile(_memoryStream);
			_memoryStream.Position = 0L;
			_arrayList = new ArrayList();
			ProcessArrayList(_arrayList, _dataGrid.Layouts, null);
			_isLayoutModified = false;
		}

		private void ProcessArrayList(ArrayList arrayList, GridLayoutCollection gridLayoutCollection, int[] integerArray)
		{
			for (int i = 0; i < gridLayoutCollection.Count; i++)
			{
				arrayList.Add(CreateLayoutData(integerArray, i, gridLayoutCollection[i]));
				if (gridLayoutCollection[i].ChildLayouts.Count <= 0)
				{
					continue;
				}
				int[] array;
				if (integerArray == null)
				{
					array = new int[1] { i };
				}
				else
				{
					array = new int[integerArray.Length + 1];
					for (int j = 0; j < integerArray.Length; j++)
					{
						array[j] = integerArray[j];
					}
					array[array.Length - 1] = i;
				}
				ProcessArrayList(arrayList, gridLayoutCollection[i].ChildLayouts, array);
			}
		}

		private GridEditorDialogData CreateLayoutData(int[] integerArray, int layoutPathItem, GridLayout gridLayout)
		{
			GridEditorDialogData result = default(GridEditorDialogData);
			if (integerArray == null)
			{
				result._layoutPath = new int[1] { layoutPathItem };
			}
			else
			{
				result._layoutPath = new int[integerArray.Length + 1];
				for (int i = 0; i < integerArray.Length; i++)
				{
					result._layoutPath[i] = integerArray[i];
				}
				result._layoutPath[result._layoutPath.Length - 1] = layoutPathItem;
			}
			result._dataMember = gridLayout.DataMember;
			result._dataSource = gridLayout._dataSource;
			return result;
		}
	}
}
