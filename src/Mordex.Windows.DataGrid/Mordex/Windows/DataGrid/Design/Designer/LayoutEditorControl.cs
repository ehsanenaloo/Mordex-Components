using Mordex.Windows.Common;
using System;
using Mordex.Windows.DataGrid.Internal;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	internal class LayoutEditorControl : UserControl, IDesignerPage
	{
		private ImageList _imageList;

		private SaveFileDialog _saveFileDialog;

		private MxToolBar _tbLayouts;

		private MxToolBarButton _loadLayout;

		private MxToolBarButton _saveLayout;

		private OpenFileDialog _openFileDialog;

		private TreeView _layoutTree;

		private Splitter _treeSplitter;

		private Panel _panel;

		private PropertyGrid _layoutProperties;

		private MxToolBar _tbCommands;

		private MxToolBarButton _tbbSaveCurrentLayout;

		private MxToolBarButton _tbbNewLayout;

		private MxToolBarButton _tbbAddChildLayout;

		private MxToolBarButton _tbbCopyLayout;

		private MxToolBarButton _tbbDelete;

		private MxToolBarButton _tbbModify;

		private IContainer _iContainer;

		private MxDataGrid _dataGrid;

		private EventHandler _dataChangedHandler;

		private static object _dataChangedEventKey = new object();

		MxDataGrid IDesignerPage.DataGrid
		{
			get
			{
				return _dataGrid;
			}
			set
			{
				if (value != _dataGrid)
				{
					_dataGrid = value;
					RefreshLayoutTree();
				}
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

		event EventHandler IDesignerPage.SelectTab
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

		internal LayoutEditorControl()
		{
			InitializeControls();
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(LayoutEditorControl));
			_imageList = new ImageList(_iContainer);
			_saveFileDialog = new SaveFileDialog();
			_tbLayouts = new MxToolBar();
			_loadLayout = new MxToolBarButton();
			_saveLayout = new MxToolBarButton();
			_openFileDialog = new OpenFileDialog();
			_layoutTree = new TreeView();
			_treeSplitter = new Splitter();
			_panel = new Panel();
			_layoutProperties = new PropertyGrid();
			_tbCommands = new MxToolBar();
			_tbbSaveCurrentLayout = new MxToolBarButton();
			_tbbNewLayout = new MxToolBarButton();
			_tbbAddChildLayout = new MxToolBarButton();
			_tbbCopyLayout = new MxToolBarButton();
			_tbbDelete = new MxToolBarButton();
			_tbbModify = new MxToolBarButton();
			_panel.SuspendLayout();
			SuspendLayout();
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Magenta;
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			_imageList.Images.SetKeyName(2, "");
			_imageList.Images.SetKeyName(3, "");
			_imageList.Images.SetKeyName(4, "Clear.bmp");
			_imageList.Images.SetKeyName(5, "NewItem.ico");
			_saveFileDialog.CreatePrompt = true;
			_saveFileDialog.DefaultExt = "gxl";
			_saveFileDialog.Filter = "DataGrid Layout Files (*.gxl)|*.gxl|All Files|*.*";
			_saveFileDialog.Title = "Save As";
			_tbLayouts.Appearance = MxToolBarAppearance.Flat;
			_tbLayouts.Buttons.AddRange(new MxToolBarButton[2] { _loadLayout, _saveLayout });
			_tbLayouts.Divider = false;
			_tbLayouts.DropDownArrows = true;
			_tbLayouts.ImageList = _imageList;
			_tbLayouts.Location = new Point(2, 2);
			_tbLayouts.Name = "tbLayouts";
			_tbLayouts.ShowToolTips = true;
			_tbLayouts.Size = new Size(645, 26);
			_tbLayouts.TabIndex = 13;
			_tbLayouts.TextAlign = MxToolBarTextAlign.Right;
			_tbLayouts.Wrappable = false;
			_tbLayouts.ButtonClick += OnTbLayoutsButtonClick;
			_loadLayout.ImageIndex = 0;
			_loadLayout.Name = "loadLayout";
			_loadLayout.Text = "Load Layout File...";
			_saveLayout.ImageIndex = 1;
			_saveLayout.Name = "saveLayout";
			_saveLayout.Text = "Save Layout File As...";
			_openFileDialog.AddExtension = false;
			_openFileDialog.Filter = "DataGrid Layout Files (*.gxl)|*.gxl|All Files|*.*";
			_layoutTree.Dock = DockStyle.Left;
			_layoutTree.FullRowSelect = true;
			_layoutTree.HideSelection = false;
			_layoutTree.ImageIndex = 0;
			_layoutTree.ImageList = _imageList;
			_layoutTree.Location = new Point(2, 28);
			_layoutTree.Name = "layoutTree";
			_layoutTree.SelectedImageIndex = 0;
			_layoutTree.Size = new Size(272, 287);
			_layoutTree.TabIndex = 14;
			_layoutTree.DoubleClick += OnLayoutTreeDoubleClick;
			_layoutTree.AfterSelect += OnLayoutTreeAfterSelect;
			_treeSplitter.BackColor = SystemColors.Menu;
			_treeSplitter.Location = new Point(274, 28);
			_treeSplitter.Name = "splitter1";
			_treeSplitter.Size = new Size(4, 287);
			_treeSplitter.TabIndex = 23;
			_treeSplitter.TabStop = false;
			_panel.Controls.Add(_layoutProperties);
			_panel.Controls.Add(_tbCommands);
			_panel.Dock = DockStyle.Fill;
			_panel.Location = new Point(278, 28);
			_panel.Name = "panel1";
			_panel.Size = new Size(369, 287);
			_panel.TabIndex = 24;
			_layoutProperties.Dock = DockStyle.Fill;
			_layoutProperties.HelpVisible = false;
			_layoutProperties.LineColor = SystemColors.ControlLight;
			_layoutProperties.Location = new Point(0, 0);
			_layoutProperties.Name = "layoutProperties";
			_layoutProperties.PropertySort = PropertySort.Alphabetical;
			_layoutProperties.Size = new Size(237, 287);
			_layoutProperties.TabIndex = 28;
			_layoutProperties.ToolbarVisible = false;
			_layoutProperties.PropertyValueChanged += OnLayoutPropertiesPropertyValueChanged;
			_tbCommands.Appearance = MxToolBarAppearance.Flat;
			_tbCommands.AutoSize = false;
			_tbCommands.Buttons.AddRange(new MxToolBarButton[6] { _tbbSaveCurrentLayout, _tbbNewLayout, _tbbAddChildLayout, _tbbCopyLayout, _tbbDelete, _tbbModify });
			_tbCommands.ButtonSize = new Size(124, 22);
			_tbCommands.Divider = false;
			_tbCommands.Dock = DockStyle.Right;
			_tbCommands.DropDownArrows = true;
			_tbCommands.ImageList = _imageList;
			_tbCommands.Location = new Point(237, 0);
			_tbCommands.Name = "tbCommands";
			_tbCommands.Padding = new Padding(2, 0, 0, 0);
			_tbCommands.ShowToolTips = true;
			_tbCommands.Size = new Size(132, 287);
			_tbCommands.TabIndex = 30;
			_tbCommands.TextAlign = MxToolBarTextAlign.Right;
			_tbCommands.ButtonClick += OnTbCommandsButtonClick;
			_tbbSaveCurrentLayout.Name = "tbbSaveCurrentLayout";
			_tbbSaveCurrentLayout.Text = "Save Current Layout";
			_tbbNewLayout.Name = "tbbNewLayout";
			_tbbNewLayout.Text = "New Layout";
			_tbbAddChildLayout.Name = "tbbAddChildLayout";
			_tbbAddChildLayout.Text = "Add Child Layout";
			_tbbCopyLayout.Name = "tbbCopyLayout";
			_tbbCopyLayout.Text = "Copy Layout...";
			_tbbDelete.ImageKey = "Clear.bmp";
			_tbbDelete.Name = "tbbDelete";
			_tbbDelete.Text = "Delete";
			_tbbModify.Name = "tbbModify";
			_tbbModify.Text = "Modify...";
			base.Controls.Add(_panel);
			base.Controls.Add(_treeSplitter);
			base.Controls.Add(_layoutTree);
			base.Controls.Add(_tbLayouts);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "LayoutTreePage";
			base.Padding = new Padding(2);
			base.Size = new Size(649, 317);
			_panel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		void IDesignerPage.Unload()
		{
		}

		bool IDesignerPage.CommitChanges()
		{
			return true;
		}

		private void RefreshLayoutTree()
		{
			_layoutTree.Nodes.Clear();
			GridLayout currentLayout = _dataGrid.CurrentLayout;
			if (currentLayout == null)
			{
				currentLayout = _dataGrid.GetLayout();
				TreeNode treeNode = BuildLayoutNode(currentLayout);
				treeNode.ImageIndex = 3;
				treeNode.SelectedImageIndex = 3;
				_layoutTree.Nodes.Add(treeNode);
				_tbbSaveCurrentLayout.Enabled = true;
				_layoutTree.SelectedNode = treeNode;
			}
			else
			{
				_tbbSaveCurrentLayout.Enabled = false;
			}
			foreach (GridLayout layout in _dataGrid.Layouts)
			{
				_layoutTree.Nodes.Add(BuildLayoutNode(layout));
			}
			if (_dataGrid.CurrentLayout != null)
			{
				SelectLayoutNode(_dataGrid.CurrentLayout);
			}
			_layoutTree.Focus();
			UpdateButtonStates();
		}

		private TreeNode BuildLayoutNode(GridLayout layout)
		{
			string text = "";
			text = ((layout.GetOwnerCollection() != null) ? ("[" + layout.GetOwnerCollection().IndexOf(layout) + "] - '" + layout.Key + "'") : "Draft");
			TreeNode treeNode = new TreeNode(text, 2, 2);
			if (layout._isCurrentLayout)
			{
				treeNode.SelectedImageIndex = 3;
				treeNode.ImageIndex = 3;
			}
			treeNode.Tag = layout;
			foreach (GridLayout childLayout in layout.ChildLayouts)
			{
				treeNode.Nodes.Add(BuildLayoutNode(childLayout));
			}
			treeNode.Expand();
			return treeNode;
		}

		private void DeleteSelectedLayout()
		{
			if (_layoutTree.SelectedNode != null)
			{
				TreeNode selectedNode = _layoutTree.SelectedNode;
				GridLayout gridLayout = (GridLayout)selectedNode.Tag;
				if (gridLayout.GetOwnerCollection() != null)
				{
					gridLayout.GetOwnerCollection().Remove(gridLayout);
				}
				RefreshLayoutTree();
				UpdateButtonStates();
				OnDataChanged(EventArgs.Empty);
			}
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		private void DisposeResources()
		{
			GridControlDialog gridControlDialog = new GridControlDialog();
			gridControlDialog.SetLayoutName("");
			if (gridControlDialog.ShowDialog() == DialogResult.OK)
			{
				GridLayout layout = _dataGrid.GetLayout();
				_dataGrid.Layouts.Add(layout);
				try
				{
					layout.Key = gridControlDialog.GetLayoutName();
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					int i;
					for (i = 1; _dataGrid.Layouts.Contains("Layout" + i); i++)
					{
					}
					layout.Key = "Layout" + i;
				}
				_dataGrid.CurrentLayout = layout;
				RefreshLayoutTree();
				OnDataChanged(EventArgs.Empty);
			}
			gridControlDialog.Dispose();
		}

		private void CopySelectedLayout()
		{
			try
			{
				TreeNode selectedNode = _layoutTree.SelectedNode;
				if (selectedNode == null)
				{
					return;
				}
				GridLayout gridLayout = (GridLayout)selectedNode.Tag;
				if (gridLayout == _dataGrid.CurrentLayout)
				{
					gridLayout.Update();
				}
				GridLayout cloneLayout = gridLayout.CloneLayout(includeChildren: true);
				cloneLayout.Key = "CopyOf" + gridLayout.Key;
				if (gridLayout.GetOwnerCollection().Contains(cloneLayout.Key))
				{
					int i;
					for (i = 1; gridLayout.GetOwnerCollection().Contains(cloneLayout.Key + i); i++)
					{
					}
					cloneLayout.Key += i;
				}
				gridLayout.GetOwnerCollection().Add(cloneLayout);
				_dataGrid.CurrentLayout = cloneLayout;
				RefreshLayoutTree();
				OnDataChanged(EventArgs.Empty);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void ApplySelectedLayout()
		{
			try
			{
				TreeNode selectedNode = _layoutTree.SelectedNode;
				if (selectedNode == null)
				{
					return;
				}
				GridLayout gridLayout = (GridLayout)selectedNode.Tag;
				if (gridLayout.GetOwnerCollection() == null)
				{
					RaiseDataChangedEvent(EventArgs.Empty);
					return;
				}
				if (_dataGrid.CurrentLayout == null)
				{
					if (MessageBox.Show("You are about to select a layout that will overwrite all the settings that you may have set in the control.\n\rTo save the current layout in the Layouts Collection of the control, cancel this operation and click in the 'Save Current Layout' button.\n\rDo you want to continue?", "DataGrid Control", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
					{
						return;
					}
					OnDataChanged(EventArgs.Empty);
				}
				else
				{
					_dataGrid.CurrentLayout.Update();
				}
				if (_dataGrid.CurrentLayout != gridLayout)
				{
					OnDataChanged(EventArgs.Empty);
				}
				_dataGrid.CurrentLayout = gridLayout;
				RaiseDataChangedEvent(EventArgs.Empty);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void OnLayoutTreeDoubleClick(object sender, EventArgs eventArgs)
		{
			if (_tbbModify.Enabled)
			{
				ApplySelectedLayout();
			}
		}

		private void OnTbLayoutsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			if (eventArgs.Button == _loadLayout)
			{
				if (_openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						using (Stream stream = _openFileDialog.OpenFile())
						{
							_dataGrid.LoadLayoutFile(stream);
						}
					}
					catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Xml.XmlException || ex is ArgumentException || ex is InvalidOperationException)
					{
						MessageBox.Show(this, ex.Message, "Load layout", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
					RefreshLayoutTree();
					OnDataChanged(EventArgs.Empty);
				}
			}
			else if (eventArgs.Button == _saveLayout && _saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					using (Stream openFile = _saveFileDialog.OpenFile())
					{
						_dataGrid.SaveLayoutFile(openFile);
					}
				}
				catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
				{
					MessageBox.Show(this, ex.Message, "Save layout", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				OnDataChanged(EventArgs.Empty);
			}
		}

		private void AddLayout()
		{
			GridLayout gridLayout = new GridLayout();
			int i;
			for (i = 1; _dataGrid.Layouts.Contains("Layout" + i); i++)
			{
			}
			gridLayout.Key = "Layout" + i;
			_dataGrid.Layouts.Add(gridLayout);
			RefreshLayoutTree();
			SelectLayoutNode(gridLayout);
			OnDataChanged(EventArgs.Empty);
		}

		private void RaiseDataChangedEvent(EventArgs eventArgs)
		{
			if (base.Events[_dataChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnLayoutTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode == null || ((GridLayout)_layoutTree.SelectedNode.Tag).GetOwnerCollection() == null)
			{
				_layoutProperties.SelectedObject = null;
			}
			else
			{
				_layoutProperties.SelectedObject = _layoutTree.SelectedNode.Tag;
			}
			UpdateButtonStates();
		}

		private void UpdateButtonStates()
		{
			bool enabled = _layoutTree.SelectedNode != null;
			_tbbCopyLayout.Enabled = enabled;
			_tbbModify.Enabled = enabled;
			_tbbDelete.Enabled = enabled && ((GridLayout)_layoutTree.SelectedNode.Tag).GetOwnerCollection() != null;
			_tbbAddChildLayout.Enabled = enabled;
		}

		private void AddChildLayout()
		{
			if (_layoutTree.SelectedNode != null)
			{
				GridLayout gridLayout = (GridLayout)_layoutTree.SelectedNode.Tag;
				GridLayout layout = new GridLayout();
				layout.DataSource = gridLayout.DataSource;
				layout.DataMember = gridLayout.DataMember;
				int i;
				for (i = 1; gridLayout.ChildLayouts.Contains("Layout" + i); i++)
				{
				}
				layout.Key = "Layout" + i;
				gridLayout.ChildLayouts.Add(layout);
				RefreshLayoutTree();
				SelectLayoutNode(layout);
				OnDataChanged(EventArgs.Empty);
			}
		}

		private void SelectLayoutNode(GridLayout gridLayout)
		{
			_layoutTree.SelectedNode = ComputeTreeNode(_layoutTree.Nodes, gridLayout);
		}

		private TreeNode ComputeTreeNode(TreeNodeCollection treeNodeCollection, GridLayout gridLayout)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if ((GridLayout)item.Tag == gridLayout)
				{
					return item;
				}
				TreeNode treeNode = ComputeTreeNode(item.Nodes, gridLayout);
				if (treeNode != null)
				{
					return treeNode;
				}
			}
			return null;
		}

		private void OnLayoutPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			OnDataChanged(EventArgs.Empty);
		}

		private void OnTbCommandsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			if (eventArgs.Button == _tbbAddChildLayout)
			{
				AddChildLayout();
			}
			else if (eventArgs.Button == _tbbCopyLayout)
			{
				CopySelectedLayout();
			}
			else if (eventArgs.Button == _tbbDelete)
			{
				DeleteSelectedLayout();
			}
			else if (eventArgs.Button == _tbbModify)
			{
				ApplySelectedLayout();
			}
			else if (eventArgs.Button == _tbbNewLayout)
			{
				AddLayout();
			}
			else if (eventArgs.Button == _tbbSaveCurrentLayout)
			{
				DisposeResources();
			}
		}
	}
}
