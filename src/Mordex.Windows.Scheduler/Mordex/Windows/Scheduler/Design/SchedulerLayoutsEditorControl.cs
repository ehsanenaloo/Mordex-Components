using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.Design
{
	internal class SchedulerLayoutsEditorControl : UserControl, ISchedulerEditorPage
	{
		private ImageList _imageList;

		private SaveFileDialog _saveFileDialog;

		private MxToolBar _tbLayouts;

		private MxToolBarButton _loadLayout;

		private MxToolBarButton _saveLayout;

		private OpenFileDialog _openFileDialog;

		private TreeView _layoutTree;

		private Splitter _layoutSplitter;

		private Panel _panel;

		private Button _btnAddChild;

		private PropertyGrid _layoutProperties;

		private Button _deleteLayout;

		private Button _saveCurrentLayout;

		private Button _copyLayout;

		private Button _newLayout;

		private Button _btnModify;

		private IContainer _iContainer;

		private MxScheduler _scheduler;

		private static object _selectTabEventKey = new object();

		MxScheduler ISchedulerEditorPage.Scheduler
		{
			get
			{
				return _scheduler;
			}
			set
			{
				if (value != _scheduler)
				{
					_scheduler = value;
					RefreshLayoutTree();
				}
			}
		}

		event EventHandler ISchedulerEditorPage.SelectTab
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

		internal SchedulerLayoutsEditorControl()
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(SchedulerLayoutsEditorControl));
			_imageList = new ImageList(_iContainer);
			_saveFileDialog = new SaveFileDialog();
			_tbLayouts = new MxToolBar();
			_loadLayout = new MxToolBarButton();
			_saveLayout = new MxToolBarButton();
			_openFileDialog = new OpenFileDialog();
			_layoutTree = new TreeView();
			_layoutSplitter = new Splitter();
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
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			_imageList.Images.SetKeyName(2, "");
			_imageList.Images.SetKeyName(3, "");
			_saveFileDialog.CreatePrompt = true;
			_saveFileDialog.DefaultExt = "xml";
			_saveFileDialog.Filter = "Schedule Layout Files (*.xml)|*.xml|All Files|*.*";
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
			_openFileDialog.Filter = "Schedule Layout Files (*.xml)|*.xml|All Files|*.*";
			_layoutTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
			_layoutSplitter.Location = new Point(274, 28);
			_layoutSplitter.Name = "splitter1";
			_layoutSplitter.Size = new Size(4, 287);
			_layoutSplitter.TabIndex = 23;
			_layoutSplitter.TabStop = false;
			_panel.Controls.Add(_btnAddChild);
			_panel.Controls.Add(_layoutProperties);
			_panel.Controls.Add(_deleteLayout);
			_panel.Controls.Add(_saveCurrentLayout);
			_panel.Controls.Add(_copyLayout);
			_panel.Controls.Add(_newLayout);
			_panel.Controls.Add(_btnModify);
			_panel.Dock = DockStyle.Fill;
			_panel.Location = new Point(278, 28);
			_panel.Name = "panel1";
			_panel.Size = new Size(369, 287);
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
			_layoutProperties.HelpVisible = false;
			_layoutProperties.LineColor = SystemColors.ControlLight;
			_layoutProperties.Location = new Point(2, 0);
			_layoutProperties.Name = "layoutProperties";
			_layoutProperties.PropertySort = PropertySort.Alphabetical;
			_layoutProperties.Size = new Size(233, 293);
			_layoutProperties.TabIndex = 28;
			_layoutProperties.ToolbarVisible = false;
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
			base.Controls.Add(_panel);
			base.Controls.Add(_layoutSplitter);
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

		bool ISchedulerEditorPage.CommitChanges()
		{
			return true;
		}

		void ISchedulerEditorPage.Unload()
		{
		}

		private void RefreshLayoutTree()
		{
			_layoutTree.Nodes.Clear();
			SchedulerLayout currentLayout = _scheduler.CurrentLayout;
			if (currentLayout == null)
			{
				currentLayout = _scheduler.GetLayout();
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
			foreach (SchedulerLayout layout in _scheduler.Layouts)
			{
				_layoutTree.Nodes.Add(CreateLayoutNode(layout));
			}
			if (_scheduler.CurrentLayout != null)
			{
				SelectLayoutNode(_scheduler.CurrentLayout);
			}
			_layoutTree.Focus();
			UpdateButtonStates();
		}

		private TreeNode CreateLayoutNode(SchedulerLayout layout)
		{
			string text = "";
			text = ((layout.GetSchedulerLayoutCollection() != null) ? ("[" + layout.GetSchedulerLayoutCollection().IndexOf(layout) + "] - '" + layout.Key + "'") : "Draft");
			TreeNode treeNode = new TreeNode(text, 2, 2);
			if (layout._isCurrentLayout)
			{
				treeNode.SelectedImageIndex = 3;
				treeNode.ImageIndex = 3;
			}
			treeNode.Tag = layout;
			foreach (SchedulerLayout childLayout in layout.ChildLayouts)
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
				SchedulerLayout schedulerLayout = (SchedulerLayout)selectedNode.Tag;
				if (schedulerLayout.GetSchedulerLayoutCollection() != null)
				{
					schedulerLayout.GetSchedulerLayoutCollection().Remove(schedulerLayout);
				}
				RefreshLayoutTree();
				UpdateButtonStates();
			}
		}

		private void OnSaveCurrentLayoutClick(object sender, EventArgs eventArgs)
		{
			SchedulerControlDialog schedulerControlDialog = new SchedulerControlDialog();
			schedulerControlDialog.SetLayoutName("");
			if (schedulerControlDialog.ShowDialog() == DialogResult.OK)
			{
				SchedulerLayout layout = _scheduler.GetLayout();
				_scheduler.Layouts.Add(layout);
				try
				{
					layout.Key = schedulerControlDialog.GetLayoutName();
				}
				catch
				{
					int i;
					for (i = 1; _scheduler.Layouts.Contains("Layout" + i); i++)
					{
					}
					layout.Key = "Layout" + i;
				}
				_scheduler.CurrentLayout = layout;
				RefreshLayoutTree();
			}
			schedulerControlDialog.Dispose();
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
				SchedulerLayout schedulerLayout = (SchedulerLayout)selectedNode.Tag;
				if (schedulerLayout == _scheduler.CurrentLayout)
				{
					schedulerLayout.Update();
				}
				SchedulerLayout layoutCopy = schedulerLayout.CloneLayout(includeChildren: true);
				layoutCopy.Key = "CopyOf" + schedulerLayout.Key;
				if (schedulerLayout.GetSchedulerLayoutCollection() != null)
				{
					if (schedulerLayout.GetSchedulerLayoutCollection().Contains(layoutCopy.Key))
					{
						int i;
						for (i = 1; schedulerLayout.GetSchedulerLayoutCollection().Contains(layoutCopy.Key + i); i++)
						{
						}
						layoutCopy.Key += i;
					}
					schedulerLayout.GetSchedulerLayoutCollection().Add(layoutCopy);
				}
				_scheduler.CurrentLayout = layoutCopy;
				RefreshLayoutTree();
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
				SchedulerLayout schedulerLayout = (SchedulerLayout)selectedNode.Tag;
				if (schedulerLayout.GetSchedulerLayoutCollection() == null)
				{
					RaiseSelectTab(EventArgs.Empty);
					return;
				}
				if (_scheduler.CurrentLayout == null)
				{
					if (MessageBox.Show("You are about to select a layout that will overwrite all the settings that you may have set in the control.\nTo save the current layout in the Layouts Collection of the control, cancel this operation and click in the 'Save Current Layout' button.\nDo you want to continue?", "Schedule Control", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
					{
						return;
					}
				}
				else
				{
					_scheduler.CurrentLayout.Update();
				}
				_scheduler.CurrentLayout = schedulerLayout;
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
			if (eventArgs.Button == _loadLayout)
			{
				if (_openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					Stream stream = _openFileDialog.OpenFile();
					_scheduler.LoadLayoutFile(stream);
					RefreshLayoutTree();
				}
			}
			else if (eventArgs.Button == _saveLayout && _saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				Stream saveStream = _saveFileDialog.OpenFile();
				_scheduler.SaveLayoutFile(saveStream);
				saveStream.Close();
			}
		}

		private void OnNewLayoutClick(object sender, EventArgs eventArgs)
		{
			SchedulerLayout schedulerLayout = new SchedulerLayout();
			int i;
			for (i = 1; _scheduler.Layouts.Contains("Layout" + i); i++)
			{
			}
			schedulerLayout.Key = "Layout" + i;
			_scheduler.Layouts.Add(schedulerLayout);
			RefreshLayoutTree();
			SelectLayoutNode(schedulerLayout);
		}

		private void RaiseSelectTab(EventArgs eventArgs)
		{
			if (base.Events[_selectTabEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnLayoutTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode == null || ((SchedulerLayout)_layoutTree.SelectedNode.Tag).GetSchedulerLayoutCollection() == null)
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
			_btnModify.Enabled = enabled;
			if (((SchedulerLayout)_layoutTree.SelectedNode.Tag).GetSchedulerLayoutCollection() != null)
			{
				_copyLayout.Enabled = enabled;
				_deleteLayout.Enabled = enabled;
				_btnAddChild.Enabled = enabled;
			}
			else
			{
				_copyLayout.Enabled = false;
				_deleteLayout.Enabled = false;
				_btnAddChild.Enabled = false;
			}
		}

		private void OnBtnAddChildClick(object sender, EventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode != null)
			{
				SchedulerLayout schedulerLayout = (SchedulerLayout)_layoutTree.SelectedNode.Tag;
				SchedulerLayout childLayout = new SchedulerLayout();
				childLayout.DataSource = schedulerLayout.DataSource;
				childLayout.DataMember = schedulerLayout.DataMember;
				int i;
				for (i = 1; schedulerLayout.ChildLayouts.Contains("Layout" + i); i++)
				{
				}
				childLayout.Key = "Layout" + i;
				schedulerLayout.ChildLayouts.Add(childLayout);
				RefreshLayoutTree();
				SelectLayoutNode(childLayout);
			}
		}

		private void SelectLayoutNode(SchedulerLayout schedulerLayout)
		{
			_layoutTree.SelectedNode = FindNodeByLayout(_layoutTree.Nodes, schedulerLayout);
		}

		private TreeNode FindNodeByLayout(TreeNodeCollection treeNodeCollection, SchedulerLayout schedulerLayout)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if ((SchedulerLayout)item.Tag == schedulerLayout)
				{
					return item;
				}
				TreeNode foundNode = FindNodeByLayout(item.Nodes, schedulerLayout);
				if (foundNode != null)
				{
					return foundNode;
				}
			}
			return null;
		}
	}
}
