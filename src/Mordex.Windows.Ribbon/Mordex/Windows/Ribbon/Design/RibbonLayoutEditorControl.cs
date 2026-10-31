using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	internal class RibbonLayoutEditorControl : UserControl, IRibbonDesignerPage
	{
		private ImageList _toolbarImageList;

		private SaveFileDialog _saveFileDialog;

		private MxToolBar _tbLayouts;

		private MxToolBarButton _loadLayoutFileButton;

		private MxToolBarButton _saveLayoutFileButton;

		private OpenFileDialog _openFileDialog;

		private TreeView _layoutTree;

		private Splitter _treeSplitter;

		private Panel _detailsPanel;

		private Button _btnAddChild;

		private PropertyGrid _layoutProperties;

		private Button _deleteLayout;

		private Button _saveCurrentLayout;

		private Button _copyLayout;

		private Button _newLayout;

		private Button _btnModify;

		private IContainer _components;

		private MxRibbon _ribbon;

		private MxRibbonStatusBar _statusBar;

		private static object s_selectTabKey = new object();

		private EventHandler _dataChangedHandler;

		private EventHandler _disableCancelHandler;

		MxRibbon IRibbonDesignerPage.Ribbon
		{
			get
			{
				return _ribbon;
			}
			set
			{
				if (value != _ribbon)
				{
					_ribbon = value;
					if (value != null)
					{
						_saveFileDialog.Filter = "Ribbon Layout File (*.xml)|*.xml|All Files|*.*";
					}
					RefreshLayoutTree();
				}
			}
		}

		MxRibbonStatusBar IRibbonDesignerPage.StatusBar
		{
			get
			{
				return _statusBar;
			}
			set
			{
				if (value != _statusBar)
				{
					_statusBar = value;
					if (value != null)
					{
						_saveFileDialog.Filter = "StatusBar Layout File (*.xml)|*.xml|All Files|*.*";
					}
					RefreshLayoutTree();
				}
			}
		}

		event EventHandler IRibbonDesignerPage.SelectTab
		{
			add
			{
				base.Events.AddHandler(s_selectTabKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_selectTabKey, value);
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler remainingHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, remainingHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler remainingHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, remainingHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		internal RibbonLayoutEditorControl()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_components?.Dispose();
				_openFileDialog?.Dispose();
				_saveFileDialog?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_components = new Container();
			ResourceManager resourceManager = new ResourceManager(typeof(RibbonLayoutEditorControl));
			_toolbarImageList = new ImageList(_components);
			_saveFileDialog = new SaveFileDialog();
			_tbLayouts = new MxToolBar();
			_loadLayoutFileButton = new MxToolBarButton();
			_saveLayoutFileButton = new MxToolBarButton();
			_openFileDialog = new OpenFileDialog();
			_layoutTree = new TreeView();
			_treeSplitter = new Splitter();
			_detailsPanel = new Panel();
			_btnAddChild = new Button();
			_layoutProperties = new PropertyGrid();
			_deleteLayout = new Button();
			_saveCurrentLayout = new Button();
			_copyLayout = new Button();
			_newLayout = new Button();
			_btnModify = new Button();
			_detailsPanel.SuspendLayout();
			SuspendLayout();
			_toolbarImageList.ColorDepth = ColorDepth.Depth8Bit;
			_toolbarImageList.ImageSize = new Size(16, 16);
			ImageStrip.Load(_toolbarImageList, resourceManager, "icons.ImageStrip");
			_toolbarImageList.TransparentColor = Color.Transparent;
			_saveFileDialog.CreatePrompt = true;
			_saveFileDialog.DefaultExt = "xml";
			_saveFileDialog.Filter = "Ribbon Layout Files (*.xml)|*.xml|All Files|*.*";
			_saveFileDialog.Title = "Save As";
			_tbLayouts.Appearance = MxToolBarAppearance.Flat;
			_tbLayouts.Buttons.AddRange(new MxToolBarButton[2] { _loadLayoutFileButton, _saveLayoutFileButton });
			_tbLayouts.Divider = false;
			_tbLayouts.DropDownArrows = true;
			_tbLayouts.ImageList = _toolbarImageList;
			_tbLayouts.Location = new Point(2, 2);
			_tbLayouts.Name = "tbLayouts";
			_tbLayouts.ShowToolTips = true;
			_tbLayouts.Size = new Size(645, 23);
			_tbLayouts.TabIndex = 13;
			_tbLayouts.TextAlign = MxToolBarTextAlign.Right;
			_tbLayouts.Wrappable = false;
			_tbLayouts.ButtonClick += OnTbLayoutsButtonClick;
			_loadLayoutFileButton.ImageIndex = 0;
			_loadLayoutFileButton.Text = "Load Layout File...";
			_saveLayoutFileButton.ImageIndex = 1;
			_saveLayoutFileButton.Text = "Save Layout File As...";
			_openFileDialog.AddExtension = false;
			_openFileDialog.Filter = "Ribbon Layout Files (*.xml)|*.xml|All Files|*.*";
			_layoutTree.BorderStyle = BorderStyle.None;
			_layoutTree.Dock = DockStyle.Left;
			_layoutTree.FullRowSelect = true;
			_layoutTree.HideSelection = false;
			_layoutTree.ImageList = _toolbarImageList;
			_layoutTree.Location = new Point(2, 25);
			_layoutTree.Name = "layoutTree";
			_layoutTree.Size = new Size(272, 290);
			_layoutTree.TabIndex = 14;
			_layoutTree.DoubleClick += OnLayoutTreeDoubleClick;
			_layoutTree.AfterSelect += OnLayoutTreeAfterSelect;
			_treeSplitter.Location = new Point(274, 25);
			_treeSplitter.Name = "splitter1";
			_treeSplitter.Size = new Size(4, 290);
			_treeSplitter.TabIndex = 23;
			_treeSplitter.TabStop = false;
			_detailsPanel.Controls.AddRange(new Control[7] { _btnAddChild, _layoutProperties, _deleteLayout, _saveCurrentLayout, _copyLayout, _newLayout, _btnModify });
			_detailsPanel.Dock = DockStyle.Fill;
			_detailsPanel.Location = new Point(278, 25);
			_detailsPanel.Name = "panel1";
			_detailsPanel.Size = new Size(369, 290);
			_detailsPanel.TabIndex = 24;
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
			_layoutProperties.Location = new Point(2, 0);
			_layoutProperties.Name = "layoutProperties";
			_layoutProperties.PropertySort = PropertySort.Alphabetical;
			_layoutProperties.Size = new Size(233, 296);
			_layoutProperties.TabIndex = 28;
			_layoutProperties.Text = "propertyGrid1";
			_layoutProperties.ToolbarVisible = false;
			_layoutProperties.ViewBackColor = SystemColors.Window;
			_layoutProperties.ViewForeColor = SystemColors.WindowText;
			_layoutProperties.PropertyValueChanged += OnLayoutPropertiesPropertyValueChanged;
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
			base.Controls.AddRange(new Control[4] { _detailsPanel, _treeSplitter, _layoutTree, _tbLayouts });
			base.DockPadding.All = 2;
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "LayoutTreePage";
			base.Size = new Size(649, 317);
			_detailsPanel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		void IRibbonDesignerPage.Unload()
		{
		}

		bool IRibbonDesignerPage.CommitChanges()
		{
			return true;
		}

		private void RefreshLayoutTree()
		{
			_layoutTree.Nodes.Clear();
			if (_ribbon != null)
			{
				RibbonLayout currentLayout = _ribbon.CurrentLayout;
				if (currentLayout == null)
				{
					currentLayout = _ribbon.GetLayout();
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
				foreach (RibbonLayout layout in _ribbon.Layouts)
				{
					_layoutTree.Nodes.Add(CreateLayoutNode(layout));
				}
				if (_ribbon.CurrentLayout != null)
				{
					SelectLayoutNode(_ribbon.CurrentLayout);
				}
			}
			_layoutTree.Focus();
			UpdateButtonStates();
		}

		private TreeNode CreateLayoutNode(RibbonLayout layout)
		{
			string text = "";
			text = ((layout.GetOwningCollection() != null) ? ("[" + layout.GetOwningCollection().IndexOf(layout) + "] - '" + layout.Key + "'") : "Draft");
			TreeNode treeNode = new TreeNode(text, 2, 2);
			if (layout._isCurrentLayout)
			{
				treeNode.SelectedImageIndex = 3;
				treeNode.ImageIndex = 3;
			}
			treeNode.Tag = layout;
			foreach (RibbonLayout childLayout in layout.ChildLayouts)
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
				RibbonLayout ribbonLayout = (RibbonLayout)selectedNode.Tag;
				if (ribbonLayout.GetOwningCollection() != null)
				{
					ribbonLayout.GetOwningCollection().Remove(ribbonLayout);
				}
				RefreshLayoutTree();
				UpdateButtonStates();
				OnDataChanged(EventArgs.Empty);
			}
		}

		private void OnSaveCurrentLayoutClick(object sender, EventArgs eventArgs)
		{
			using RibbonDialog ribbonDialog = new RibbonDialog();
			ribbonDialog.SetLayoutName("");
			if (ribbonDialog.ShowDialog() == DialogResult.OK)
			{
				if (_ribbon != null)
				{
					RibbonLayout layout = _ribbon.GetLayout();
					_ribbon.Layouts.Add(layout);
					try
					{
						layout.Key = ribbonDialog.GetLayoutName();
					}
					catch (InvalidOperationException)
					{
						// The typed key is already used, so pick the next free name.
						int i;
						for (i = 1; _ribbon.Layouts.Contains("Layout" + i); i++)
						{
						}
						layout.Key = "Layout" + i;
					}
					_ribbon.CurrentLayout = layout;
				}
				RefreshLayoutTree();
				OnDataChanged(EventArgs.Empty);
			}
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
				if (_ribbon != null)
				{
					RibbonLayout ribbonLayout = (RibbonLayout)selectedNode.Tag;
					if (ribbonLayout == _ribbon.CurrentLayout)
					{
						ribbonLayout.Update();
					}
					RibbonLayout layoutCopy = ribbonLayout.CloneLayout(includeChildren: true);
					layoutCopy.Key = "CopyOf" + ribbonLayout.Key;
					if (ribbonLayout.GetOwningCollection() != null)
					{
						if (ribbonLayout.GetOwningCollection().Contains(layoutCopy.Key))
						{
							int i;
							for (i = 1; ribbonLayout.GetOwningCollection().Contains(layoutCopy.Key + i); i++)
							{
							}
							layoutCopy.Key += i;
						}
						ribbonLayout.GetOwningCollection().Add(layoutCopy);
					}
					_ribbon.CurrentLayout = layoutCopy;
				}
				RefreshLayoutTree();
				OnDataChanged(EventArgs.Empty);
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
				if (selectedNode == null || _ribbon == null)
				{
					return;
				}
				RibbonLayout ribbonLayout = (RibbonLayout)selectedNode.Tag;
				if (ribbonLayout.GetOwningCollection() == null)
				{
					RaiseSelectTab(EventArgs.Empty);
					return;
				}
				if (_ribbon.CurrentLayout == null)
				{
					if (MessageBox.Show("You are about to select a layout that will overwrite all the settings that you may have set in the control.\n\rTo save the current layout in the Layouts Collection of the control, cancel this operation and click in the 'Save Current Layout' button.\n\rDo you want to continue?", "Ribbon", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
					{
						return;
					}
					OnDataChanged(EventArgs.Empty);
				}
				else
				{
					_ribbon.CurrentLayout.Update();
				}
				if (_ribbon.CurrentLayout != ribbonLayout)
				{
					OnDataChanged(EventArgs.Empty);
				}
				_ribbon.CurrentLayout = ribbonLayout;
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
			if (_ribbon == null)
			{
				return;
			}
			if (eventArgs.Button == _loadLayoutFileButton)
			{
				if (_openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					using (Stream stream = _openFileDialog.OpenFile())
					{
						_ribbon.LoadLayoutFile(stream);
					}
					RefreshLayoutTree();
					OnDataChanged(EventArgs.Empty);
				}
			}
			else if (eventArgs.Button == _saveLayoutFileButton && _saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				using (Stream stream = _saveFileDialog.OpenFile())
				{
					_ribbon.SaveLayoutFile(stream);
					stream.Flush();
				}
				OnDataChanged(EventArgs.Empty);
			}
		}

		private void OnNewLayoutClick(object sender, EventArgs eventArgs)
		{
			int i = 1;
			if (_ribbon != null)
			{
				RibbonLayout ribbonLayout = new RibbonLayout();
				for (; _ribbon.Layouts.Contains("Layout" + i); i++)
				{
				}
				ribbonLayout.Key = "Layout" + i;
				_ribbon.Layouts.Add(ribbonLayout);
				RefreshLayoutTree();
				SelectLayoutNode(ribbonLayout);
			}
			OnDataChanged(EventArgs.Empty);
		}

		private void RaiseSelectTab(EventArgs eventArgs)
		{
			if (base.Events[s_selectTabKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnLayoutTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			_layoutProperties.SelectedObject = _layoutTree.SelectedNode?.Tag;
			UpdateButtonStates();
		}

		private void UpdateButtonStates()
		{
			bool hasSelection = _layoutTree.SelectedNode != null;
			_btnModify.Enabled = hasSelection;
			if (_ribbon != null)
			{
				_copyLayout.Enabled = hasSelection && ((RibbonLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
				_deleteLayout.Enabled = hasSelection && ((RibbonLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
				_btnAddChild.Enabled = hasSelection && ((RibbonLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
			}
		}

		private void OnBtnAddChildClick(object sender, EventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode == null)
			{
				return;
			}
			if (_ribbon != null)
			{
				RibbonLayout ribbonLayout = (RibbonLayout)_layoutTree.SelectedNode.Tag;
				RibbonLayout childLayout = new RibbonLayout();
				int i;
				for (i = 1; ribbonLayout.ChildLayouts.Contains("Layout" + i); i++)
				{
				}
				childLayout.Key = "Layout" + i;
				ribbonLayout.ChildLayouts.Add(childLayout);
				RefreshLayoutTree();
				SelectLayoutNode(childLayout);
			}
			OnDataChanged(EventArgs.Empty);
		}

		private void SelectLayoutNode(object value)
		{
			_layoutTree.SelectedNode = FindNodeByTag(_layoutTree.Nodes, value);
		}

		private TreeNode FindNodeByTag(TreeNodeCollection treeNodeCollection, object value)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (item.Tag == value)
				{
					return item;
				}
				TreeNode foundNode = FindNodeByTag(item.Nodes, value);
				if (foundNode != null)
				{
					return foundNode;
				}
			}
			return null;
		}

		private void OnLayoutPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			OnDataChanged(EventArgs.Empty);
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		protected virtual void OnDisableCancel(EventArgs eventArgs)
		{
			if (_disableCancelHandler != null)
			{
				_disableCancelHandler(this, eventArgs);
			}
		}
	}
}
