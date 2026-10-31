using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane.Design
{
	internal class TaskPaneEditorControl : UserControl, ITaskPaneElement
	{
		private ImageList _imageList;

		private SaveFileDialog _saveFileDialog;

		private MxToolBar _tbLayouts;

		private MxToolBarButton _tbtnLoadLayout;

		private MxToolBarButton _tbtnSaveLayout;

		private OpenFileDialog _openFileDialog;

		private TreeView _layoutTree;

		private Splitter _splitter;

		private Panel _panel;

		private Button _btnAddChild;

		private PropertyGrid _layoutProperties;

		private Button _deleteLayout;

		private Button _saveCurrentLayout;

		private Button _copyLayout;

		private Button _newLayout;

		private Button _btnModify;

		private IContainer _iContainer;

		private MxTaskPane _taskPane;

		private static object _selectTabEventKey = new object();

		MxTaskPane ITaskPaneElement.TaskPane
		{
			get
			{
				return _taskPane;
			}
			set
			{
				if (value != _taskPane)
				{
					_taskPane = value;
					RebuildLayoutTree();
				}
			}
		}

		event EventHandler ITaskPaneElement.SelectTab
		{
			add
			{
				base.Events.AddHandler(_selectTabEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectTabEventKey, value);
			}
		}

		internal TaskPaneEditorControl()
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
			ResourceManager resourceManager = new ResourceManager(typeof(TaskPaneEditorControl));
			_imageList = new ImageList(_iContainer);
			_saveFileDialog = new SaveFileDialog();
			_tbLayouts = new MxToolBar();
			_tbtnLoadLayout = new MxToolBarButton();
			_tbtnSaveLayout = new MxToolBarButton();
			_openFileDialog = new OpenFileDialog();
			_layoutTree = new TreeView();
			_splitter = new Splitter();
			_panel = new Panel();
			_btnAddChild = new Button();
			_layoutProperties = new PropertyGrid();
			_deleteLayout = new Button();
			_saveCurrentLayout = new Button();
			_copyLayout = new Button();
			_newLayout = new Button();
			_btnModify = new Button();
			_panel.SuspendLayout();
			SuspendLayout();
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(16, 16);
			ImageStrip.Load(_imageList, resourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_saveFileDialog.CreatePrompt = true;
			_saveFileDialog.DefaultExt = "ebl";
			_saveFileDialog.Filter = "TaskPane Layout Files (*.ebl)|*.ebl|All Files|*.*";
			_saveFileDialog.Title = "Save As";
			_tbLayouts.Appearance = MxToolBarAppearance.Flat;
			_tbLayouts.Buttons.AddRange(new MxToolBarButton[2] { _tbtnLoadLayout, _tbtnSaveLayout });
			_tbLayouts.Divider = false;
			_tbLayouts.DropDownArrows = true;
			_tbLayouts.ImageList = _imageList;
			_tbLayouts.Location = new Point(2, 2);
			_tbLayouts.Name = "tbLayouts";
			_tbLayouts.ShowToolTips = true;
			_tbLayouts.Size = new Size(645, 23);
			_tbLayouts.TabIndex = 13;
			_tbLayouts.TextAlign = MxToolBarTextAlign.Right;
			_tbLayouts.Wrappable = false;
			_tbLayouts.ButtonClick += OnTbLayoutsButtonClick;
			_tbtnLoadLayout.ImageIndex = 0;
			_tbtnLoadLayout.Text = "Load Layout File...";
			_tbtnSaveLayout.ImageIndex = 1;
			_tbtnSaveLayout.Text = "Save Layout File As...";
			_openFileDialog.AddExtension = false;
			_openFileDialog.Filter = "TaskPane Layout Files (*.ebl)|*.ebl|All Files|*.*";
			_layoutTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_layoutTree.Dock = DockStyle.Left;
			_layoutTree.FullRowSelect = true;
			_layoutTree.HideSelection = false;
			_layoutTree.ImageList = _imageList;
			_layoutTree.Location = new Point(2, 25);
			_layoutTree.Name = "layoutTree";
			_layoutTree.Size = new Size(272, 290);
			_layoutTree.TabIndex = 14;
			_layoutTree.DoubleClick += OnLayoutTreeDoubleClick;
			_layoutTree.AfterSelect += OnLayoutTreeAfterSelect;
			_splitter.Location = new Point(274, 25);
			_splitter.Name = "splitter1";
			_splitter.Size = new Size(4, 290);
			_splitter.TabIndex = 23;
			_splitter.TabStop = false;
			_panel.Controls.AddRange(new Control[7] { _btnAddChild, _layoutProperties, _deleteLayout, _saveCurrentLayout, _copyLayout, _newLayout, _btnModify });
			_panel.Dock = DockStyle.Fill;
			_panel.Location = new Point(278, 25);
			_panel.Name = "panel1";
			_panel.Size = new Size(369, 290);
			_panel.TabIndex = 24;
			_btnAddChild.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnAddChild.FlatStyle = FlatStyle.Flat;
			_btnAddChild.Location = new Point(240, 60);
			_btnAddChild.Name = "btnAddChild";
			_btnAddChild.Size = new Size(124, 24);
			_btnAddChild.TabIndex = 29;
			_btnAddChild.Text = "Add Child Layout";
			_btnAddChild.TextAlign = ContentAlignment.MiddleLeft;
			_btnAddChild.Click += OnBtnAddChildClick;
			_layoutProperties.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_layoutProperties.CommandsVisibleIfAvailable = true;
			_layoutProperties.HelpVisible = false;
			_layoutProperties.LargeButtons = false;
			_layoutProperties.LineColor = SystemColors.ControlLight;
			_layoutProperties.Name = "layoutProperties";
			_layoutProperties.PropertySort = PropertySort.Alphabetical;
			_layoutProperties.Size = new Size(233, 290);
			_layoutProperties.TabIndex = 28;
			_layoutProperties.Text = "propertyGrid1";
			_layoutProperties.ToolbarVisible = false;
			_layoutProperties.ViewBackColor = SystemColors.Window;
			_layoutProperties.ViewForeColor = SystemColors.WindowText;
			_deleteLayout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_deleteLayout.FlatStyle = FlatStyle.Flat;
			_deleteLayout.Location = new Point(240, 112);
			_deleteLayout.Name = "deleteLayout";
			_deleteLayout.Size = new Size(124, 24);
			_deleteLayout.TabIndex = 26;
			_deleteLayout.Text = "Delete";
			_deleteLayout.TextAlign = ContentAlignment.MiddleLeft;
			_deleteLayout.Click += OnDeleteLayoutClick;
			_saveCurrentLayout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_saveCurrentLayout.Enabled = false;
			_saveCurrentLayout.FlatStyle = FlatStyle.Flat;
			_saveCurrentLayout.Location = new Point(240, 8);
			_saveCurrentLayout.Name = "saveCurrentLayout";
			_saveCurrentLayout.Size = new Size(124, 24);
			_saveCurrentLayout.TabIndex = 25;
			_saveCurrentLayout.Text = "Save Current Layout";
			_saveCurrentLayout.TextAlign = ContentAlignment.MiddleLeft;
			_saveCurrentLayout.Click += OnSaveCurrentLayoutClick;
			_copyLayout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_copyLayout.FlatStyle = FlatStyle.Flat;
			_copyLayout.Location = new Point(240, 86);
			_copyLayout.Name = "copyLayout";
			_copyLayout.Size = new Size(124, 24);
			_copyLayout.TabIndex = 24;
			_copyLayout.Text = "Copy Layout...";
			_copyLayout.TextAlign = ContentAlignment.MiddleLeft;
			_copyLayout.Click += OnCopyLayoutClick;
			_newLayout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_newLayout.FlatStyle = FlatStyle.Flat;
			_newLayout.Location = new Point(240, 34);
			_newLayout.Name = "newLayout";
			_newLayout.Size = new Size(124, 24);
			_newLayout.TabIndex = 23;
			_newLayout.Text = "New Layout";
			_newLayout.TextAlign = ContentAlignment.MiddleLeft;
			_newLayout.Click += OnNewLayoutClick;
			_btnModify.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnModify.FlatStyle = FlatStyle.Flat;
			_btnModify.Location = new Point(240, 138);
			_btnModify.Name = "btnModify";
			_btnModify.Size = new Size(124, 24);
			_btnModify.TabIndex = 27;
			_btnModify.Text = "Modify...";
			_btnModify.TextAlign = ContentAlignment.MiddleLeft;
			_btnModify.Click += OnBtnModifyClick;
			base.Controls.AddRange(new Control[4] { _panel, _splitter, _layoutTree, _tbLayouts });
			base.DockPadding.All = 2;
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "LayoutTreePage";
			base.Size = new Size(649, 317);
			_panel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		bool ITaskPaneElement.CommitChanges()
		{
			return true;
		}

		private void RebuildLayoutTree()
		{
			_layoutTree.Nodes.Clear();
			TaskPaneLayout currentLayout = _taskPane.CurrentLayout;
			if (currentLayout == null)
			{
				currentLayout = _taskPane.GetLayout();
				TreeNode treeNode = CreateLayoutNode(currentLayout);
				treeNode.ImageIndex = 3;
				treeNode.SelectedImageIndex = 3;
				_layoutTree.Nodes.Add(treeNode);
				_saveCurrentLayout.Enabled = true;
				_layoutTree.SelectedNode = treeNode;
			}
			else
			{
				_saveCurrentLayout.Enabled = false;
			}
			foreach (TaskPaneLayout layout in _taskPane.Layouts)
			{
				_layoutTree.Nodes.Add(CreateLayoutNode(layout));
			}
			if (_taskPane.CurrentLayout != null)
			{
				SelectLayoutNode(_taskPane.CurrentLayout);
			}
			_layoutTree.Focus();
			UpdateButtonStates();
		}

		private TreeNode CreateLayoutNode(TaskPaneLayout layout)
		{
			string nodeText = "";
			nodeText = ((layout.GetOwnerCollection() != null) ? ("[" + layout.GetOwnerCollection().IndexOf(layout) + "] - '" + layout.Key + "'") : "Draft");
			TreeNode treeNode = new TreeNode(nodeText, 2, 2);
			if (layout._isCurrentLayout)
			{
				treeNode.SelectedImageIndex = 3;
				treeNode.ImageIndex = 3;
			}
			treeNode.Tag = layout;
			foreach (TaskPaneLayout childLayout in layout.ChildLayouts)
			{
				treeNode.Nodes.Add(CreateLayoutNode(childLayout));
			}
			treeNode.Expand();
			return treeNode;
		}

		private void OnDeleteLayoutClick(object sender, EventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode != null)
			{
				TreeNode selectedNode = _layoutTree.SelectedNode;
				TaskPaneLayout taskPaneLayout = (TaskPaneLayout)selectedNode.Tag;
				if (taskPaneLayout.GetOwnerCollection() != null)
				{
					taskPaneLayout.GetOwnerCollection().Remove(taskPaneLayout);
				}
				RebuildLayoutTree();
				UpdateButtonStates();
			}
		}

		private void OnSaveCurrentLayoutClick(object sender, EventArgs eventArgs)
		{
			TaskPaneControlDialog taskPaneControlDialog = new TaskPaneControlDialog();
			taskPaneControlDialog.SetLayoutName("");
			if (taskPaneControlDialog.ShowDialog() == DialogResult.OK)
			{
				TaskPaneLayout layout = _taskPane.GetLayout();
				_taskPane.Layouts.Add(layout);
				try
				{
					layout.Key = taskPaneControlDialog.GetLayoutName();
				}
				catch (ArgumentException)
				{
					int i;
					for (i = 1; _taskPane.Layouts.Contains("Layout" + i); i++)
					{
					}
					layout.Key = "Layout" + i;
				}
				_taskPane.CurrentLayout = layout;
				RebuildLayoutTree();
			}
			taskPaneControlDialog.Dispose();
		}

		private void OnCopyLayoutClick(object sender, EventArgs eventArgs)
		{
			try
			{
				TreeNode selectedNode = _layoutTree.SelectedNode;
				if (selectedNode == null)
				{
					return;
				}
				TaskPaneLayout taskPaneLayout = (TaskPaneLayout)selectedNode.Tag;
				if (taskPaneLayout == _taskPane.CurrentLayout)
				{
					taskPaneLayout.Update();
				}
				TaskPaneLayout copiedLayout = taskPaneLayout.CloneLayout(includeChildren: true);
				copiedLayout.Key = "CopyOf" + taskPaneLayout.Key;
				if (taskPaneLayout.GetOwnerCollection().Contains(copiedLayout.Key))
				{
					int i;
					for (i = 1; taskPaneLayout.GetOwnerCollection().Contains(copiedLayout.Key + i); i++)
					{
					}
					copiedLayout.Key += i;
				}
				taskPaneLayout.GetOwnerCollection().Add(copiedLayout);
				_taskPane.CurrentLayout = copiedLayout;
				RebuildLayoutTree();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void OnBtnModifyClick(object sender, EventArgs eventArgs)
		{
			try
			{
				TreeNode selectedNode = _layoutTree.SelectedNode;
				if (selectedNode == null)
				{
					return;
				}
				TaskPaneLayout taskPaneLayout = (TaskPaneLayout)selectedNode.Tag;
				if (taskPaneLayout.GetOwnerCollection() == null)
				{
					RaiseSelectTab(EventArgs.Empty);
					return;
				}
				if (_taskPane.CurrentLayout == null)
				{
					if (MessageBox.Show("You are about to select a layout that will overwrite all the settings that you may have set in the control.\n\rTo save the current layout in the Layouts Collection of the control, cancel this operation and click in the 'Save Current Layout' button.\n\rDo you want to continue?", "TaskPane Control", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
					{
						return;
					}
				}
				else
				{
					_taskPane.CurrentLayout.Update();
				}
				_taskPane.CurrentLayout = taskPaneLayout;
				RaiseSelectTab(EventArgs.Empty);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void OnLayoutTreeDoubleClick(object sender, EventArgs eventArgs)
		{
			if (_btnModify.Enabled)
			{
				OnBtnModifyClick(_btnModify, EventArgs.Empty);
			}
		}

		private void OnTbLayoutsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			if (eventArgs.Button == _tbtnLoadLayout)
			{
				if (_openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					using (Stream stream = _openFileDialog.OpenFile())
					{
						_taskPane.LoadLayoutFile(stream);
					}
					RebuildLayoutTree();
				}
			}
			else if (eventArgs.Button == _tbtnSaveLayout && _saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				using (Stream saveStream = _saveFileDialog.OpenFile())
				{
					_taskPane.SaveLayoutFile(saveStream);
				}
			}
		}

		private void OnNewLayoutClick(object sender, EventArgs eventArgs)
		{
			TaskPaneLayout taskPaneLayout = new TaskPaneLayout();
			int i;
			for (i = 1; _taskPane.Layouts.Contains("Layout" + i); i++)
			{
			}
			taskPaneLayout.Key = "Layout" + i;
			_taskPane.Layouts.Add(taskPaneLayout);
			RebuildLayoutTree();
			SelectLayoutNode(taskPaneLayout);
		}

		private void RaiseSelectTab(EventArgs eventArgs)
		{
			if (base.Events[_selectTabEventKey] is EventHandler selectTabHandler)
			{
				selectTabHandler(this, eventArgs);
			}
		}

		private void OnLayoutTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode == null || ((TaskPaneLayout)_layoutTree.SelectedNode.Tag).GetOwnerCollection() == null)
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
			bool hasSelection = _layoutTree.SelectedNode != null;
			_copyLayout.Enabled = hasSelection;
			_btnModify.Enabled = hasSelection;
			_deleteLayout.Enabled = hasSelection && ((TaskPaneLayout)_layoutTree.SelectedNode.Tag).GetOwnerCollection() != null;
			_btnAddChild.Enabled = hasSelection;
		}

		private void OnBtnAddChildClick(object sender, EventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode != null)
			{
				TaskPaneLayout taskPaneLayout = (TaskPaneLayout)_layoutTree.SelectedNode.Tag;
				TaskPaneLayout childLayout = new TaskPaneLayout();
				int i;
				for (i = 1; taskPaneLayout.ChildLayouts.Contains("Layout" + i); i++)
				{
				}
				childLayout.Key = "Layout" + i;
				taskPaneLayout.ChildLayouts.Add(childLayout);
				RebuildLayoutTree();
				SelectLayoutNode(childLayout);
			}
		}

		private void SelectLayoutNode(TaskPaneLayout taskPaneLayout)
		{
			_layoutTree.SelectedNode = ComputeTreeNode(_layoutTree.Nodes, taskPaneLayout);
		}

		private TreeNode ComputeTreeNode(TreeNodeCollection treeNodeCollection, TaskPaneLayout taskPaneLayout)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if ((TaskPaneLayout)item.Tag == taskPaneLayout)
				{
					return item;
				}
				TreeNode foundNode = ComputeTreeNode(item.Nodes, taskPaneLayout);
				if (foundNode != null)
				{
					return foundNode;
				}
			}
			return null;
		}
	}
}
