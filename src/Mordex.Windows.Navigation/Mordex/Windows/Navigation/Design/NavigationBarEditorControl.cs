using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation.Design
{
	internal class NavigationBarEditorControl : UserControl, INavigationBarElement
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

		private MxNavigationBar _navigationBar;

		private static object _selectTabEventKey = new object();

		MxNavigationBar INavigationBarElement.NavigationBar
		{
			get
			{
				return _navigationBar;
			}
			set
			{
				if (value != _navigationBar)
				{
					_navigationBar = value;
					RefreshLayoutTree();
				}
			}
		}

		event EventHandler INavigationBarElement.SelectTab
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

		internal NavigationBarEditorControl()
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
			ResourceManager resourceManager = new ResourceManager(typeof(NavigationBarEditorControl));
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
			_saveFileDialog.DefaultExt = "bbl";
			_saveFileDialog.Filter = "NavigationBar Layout Files (*.bbl)|*.bbl|All Files|*.*";
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
			_openFileDialog.Filter = "NavigationBar Layout Files (*.bbl)|*.bbl|All Files|*.*";
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
			_layoutProperties.Location = new Point(2, 2);
			_layoutProperties.Name = "layoutProperties";
			_layoutProperties.PropertySort = PropertySort.Alphabetical;
			_layoutProperties.Size = new Size(233, 287);
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

		bool INavigationBarElement.CommitChanges()
		{
			return true;
		}

		private void RefreshLayoutTree()
		{
			_layoutTree.Nodes.Clear();
			NavigationBarLayout currentLayout = _navigationBar.CurrentLayout;
			if (currentLayout == null)
			{
				currentLayout = _navigationBar.GetLayout();
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
			foreach (NavigationBarLayout layout in _navigationBar.Layouts)
			{
				_layoutTree.Nodes.Add(CreateLayoutNode(layout));
			}
			if (_navigationBar.CurrentLayout != null)
			{
				SelectLayoutNode(_navigationBar.CurrentLayout);
			}
			_layoutTree.Focus();
			UpdateButtonStates();
		}

		private TreeNode CreateLayoutNode(NavigationBarLayout layout)
		{
			string nodeText = "";
			nodeText = ((layout.GetParentCollection() != null) ? ("[" + layout.GetParentCollection().IndexOf(layout) + "] - '" + layout.Key + "'") : "Draft");
			TreeNode treeNode = new TreeNode(nodeText, 2, 2);
			if (layout._isCurrentLayout)
			{
				treeNode.SelectedImageIndex = 3;
				treeNode.ImageIndex = 3;
			}
			treeNode.Tag = layout;
			foreach (NavigationBarLayout childLayout in layout.ChildLayouts)
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
				NavigationBarLayout navigationBarLayout = (NavigationBarLayout)selectedNode.Tag;
				if (navigationBarLayout.GetParentCollection() != null)
				{
					navigationBarLayout.GetParentCollection().Remove(navigationBarLayout);
				}
				RefreshLayoutTree();
				UpdateButtonStates();
			}
		}

		private void OnSaveCurrentLayoutClick(object sender, EventArgs eventArgs)
		{
			NavigationBarControlDialog navigationBarControlDialog = new NavigationBarControlDialog();
			navigationBarControlDialog.SetLayoutName("");
			if (navigationBarControlDialog.ShowDialog() == DialogResult.OK)
			{
				NavigationBarLayout layout = _navigationBar.GetLayout();
				_navigationBar.Layouts.Add(layout);
				try
				{
					layout.Key = navigationBarControlDialog.GetLayoutName();
				}
				catch (ArgumentException)
				{
					int i;
					for (i = 1; _navigationBar.Layouts.Contains("Layout" + i); i++)
					{
					}
					layout.Key = "Layout" + i;
				}
				_navigationBar.CurrentLayout = layout;
				RefreshLayoutTree();
			}
			navigationBarControlDialog.Dispose();
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
				NavigationBarLayout navigationBarLayout = (NavigationBarLayout)selectedNode.Tag;
				if (navigationBarLayout == _navigationBar.CurrentLayout)
				{
					navigationBarLayout.Update();
				}
				NavigationBarLayout copiedLayout = navigationBarLayout.CloneLayout(includeChildren: true);
				copiedLayout.Key = "CopyOf" + navigationBarLayout.Key;
				if (navigationBarLayout.GetParentCollection().Contains(copiedLayout.Key))
				{
					int i;
					for (i = 1; navigationBarLayout.GetParentCollection().Contains(copiedLayout.Key + i); i++)
					{
					}
					copiedLayout.Key += i;
				}
				navigationBarLayout.GetParentCollection().Add(copiedLayout);
				_navigationBar.CurrentLayout = copiedLayout;
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
				NavigationBarLayout navigationBarLayout = (NavigationBarLayout)selectedNode.Tag;
				if (navigationBarLayout.GetParentCollection() == null)
				{
					RaiseSelectTab(EventArgs.Empty);
					return;
				}
				if (_navigationBar.CurrentLayout == null)
				{
					if (MessageBox.Show("You are about to select a layout that will overwrite all the settings that you may have set in the control.\n\rTo save the current layout in the Layouts Collection of the control, cancel this operation and click in the 'Save Current Layout' button.\n\rDo you want to continue?", "NavigationBar Control", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
					{
						return;
					}
				}
				else
				{
					_navigationBar.CurrentLayout.Update();
				}
				_navigationBar.CurrentLayout = navigationBarLayout;
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
						_navigationBar.LoadLayoutFile(stream);
					}
					RefreshLayoutTree();
				}
			}
			else if (eventArgs.Button == _tbtnSaveLayout && _saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				using (Stream saveStream = _saveFileDialog.OpenFile())
				{
					_navigationBar.SaveLayoutFile(saveStream);
				}
			}
		}

		private void OnNewLayoutClick(object sender, EventArgs eventArgs)
		{
			NavigationBarLayout navigationBarLayout = new NavigationBarLayout();
			int i;
			for (i = 1; _navigationBar.Layouts.Contains("Layout" + i); i++)
			{
			}
			navigationBarLayout.Key = "Layout" + i;
			_navigationBar.Layouts.Add(navigationBarLayout);
			RefreshLayoutTree();
			SelectLayoutNode(navigationBarLayout);
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
			if (_layoutTree.SelectedNode == null || ((NavigationBarLayout)_layoutTree.SelectedNode.Tag).GetParentCollection() == null)
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
			_deleteLayout.Enabled = hasSelection && ((NavigationBarLayout)_layoutTree.SelectedNode.Tag).GetParentCollection() != null;
			_btnAddChild.Enabled = hasSelection;
		}

		private void OnBtnAddChildClick(object sender, EventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode != null)
			{
				NavigationBarLayout navigationBarLayout = (NavigationBarLayout)_layoutTree.SelectedNode.Tag;
				NavigationBarLayout childLayout = new NavigationBarLayout();
				int i;
				for (i = 1; navigationBarLayout.ChildLayouts.Contains("Layout" + i); i++)
				{
				}
				childLayout.Key = "Layout" + i;
				navigationBarLayout.ChildLayouts.Add(childLayout);
				RefreshLayoutTree();
				SelectLayoutNode(childLayout);
			}
		}

		private void SelectLayoutNode(NavigationBarLayout navigationBarLayout)
		{
			_layoutTree.SelectedNode = FindTreeNode(_layoutTree.Nodes, navigationBarLayout);
		}

		private TreeNode FindTreeNode(TreeNodeCollection treeNodeCollection, NavigationBarLayout navigationBarLayout)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if ((NavigationBarLayout)item.Tag == navigationBarLayout)
				{
					return item;
				}
				TreeNode foundNode = FindTreeNode(item.Nodes, navigationBarLayout);
				if (foundNode != null)
				{
					return foundNode;
				}
			}
			return null;
		}
	}
}
