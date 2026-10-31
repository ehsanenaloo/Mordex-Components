using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	internal class LayoutEditorControl : UserControl, IManagerEditorControl
	{
		private const int LayoutImageIndex = 2;

		private const int CurrentLayoutImageIndex = 3;

		private ImageList _imageList;

		private SaveFileDialog _saveFileDialog;

		private MxToolBar _tbLayouts;

		private MxToolBarButton _loadButton;

		private MxToolBarButton _saveButton;

		private OpenFileDialog _openFileDialog;

		private TreeView _layoutTree;

		private Splitter _splitter;

		private Panel _buttonPanel;

		private Button _btnAddChild;

		private PropertyGrid _layoutProperties;

		private Button _btnDeleteLayout;

		private Button _btnSaveCurrentLayout;

		private Button _btnCopyLayout;

		private Button _btnNewLayout;

		private Button _btnModify;

		private IContainer _components;

		private MxCommandManager _commandManager;

		private MxDockManager _panelManager;

		private static object _selectTabKey = new object();

		private EventHandler _dataChangedHandler;

		private EventHandler _disableCancelHandler;

		MxDockManager IManagerEditorControl.PanelManager
		{
			get
			{
				return _panelManager;
			}
			set
			{
				if (value != _panelManager)
				{
					_panelManager = value;
					if (value != null)
					{
						_saveFileDialog.Filter = "Panels Layout File (*.xml)|*.xml|All Files|*.*";
					}
					RefreshLayoutTree();
				}
			}
		}

		MxCommandManager IManagerEditorControl.CommandManager
		{
			get
			{
				return _commandManager;
			}
			set
			{
				if (value != _commandManager)
				{
					_commandManager = value;
					if (value != null)
					{
						_saveFileDialog.Filter = "CommandBars Layout File(*.xml)|*.xml|All Files|*.*";
					}
					RefreshLayoutTree();
				}
			}
		}

		event EventHandler IManagerEditorControl.SelectTab
		{
			add
			{
				base.Events.AddHandler(_selectTabKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectTabKey, value);
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler currentHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _dataChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
			remove
			{
				EventHandler currentHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _dataChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler currentHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _disableCancelHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
			remove
			{
				EventHandler currentHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _disableCancelHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
		}

		internal LayoutEditorControl()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_components != null)
				{
					_components.Dispose();
				}
				_saveFileDialog.Dispose();
				_openFileDialog.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_components = new Container();
			ResourceManager resourceManager = new ResourceManager(typeof(LayoutEditorControl));
			_imageList = new ImageList(_components);
			_saveFileDialog = new SaveFileDialog();
			_tbLayouts = new MxToolBar();
			_loadButton = new MxToolBarButton();
			_saveButton = new MxToolBarButton();
			_openFileDialog = new OpenFileDialog();
			_layoutTree = new TreeView();
			_splitter = new Splitter();
			_buttonPanel = new Panel();
			_btnAddChild = new Button();
			_layoutProperties = new PropertyGrid();
			_btnDeleteLayout = new Button();
			_btnSaveCurrentLayout = new Button();
			_btnCopyLayout = new Button();
			_btnNewLayout = new Button();
			_btnModify = new Button();
			_buttonPanel.SuspendLayout();
			SuspendLayout();
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(16, 16);
			ImageStrip.Load(_imageList, resourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_saveFileDialog.CreatePrompt = true;
			_saveFileDialog.DefaultExt = "scl";
			_saveFileDialog.Filter = "CommandBars Layout Files (*.xml)|*.xml|All Files|*.*";
			_saveFileDialog.Title = "Save As";
			_tbLayouts.Appearance = MxToolBarAppearance.Flat;
			_tbLayouts.Buttons.AddRange(new MxToolBarButton[2] { _loadButton, _saveButton });
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
			_loadButton.ImageIndex = 0;
			_loadButton.Text = "Load Layout File...";
			_saveButton.ImageIndex = 1;
			_saveButton.Text = "Save Layout File As...";
			_openFileDialog.AddExtension = false;
			_openFileDialog.Filter = "CommandBars Layout Files (*.xml)|*.xml|All Files|*.*";
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
			_buttonPanel.Controls.AddRange(new Control[7] { _btnAddChild, _layoutProperties, _btnDeleteLayout, _btnSaveCurrentLayout, _btnCopyLayout, _btnNewLayout, _btnModify });
			_buttonPanel.Dock = DockStyle.Fill;
			_buttonPanel.Location = new Point(278, 25);
			_buttonPanel.Name = "panel1";
			_buttonPanel.Size = new Size(369, 290);
			_buttonPanel.TabIndex = 24;
			_btnAddChild.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnAddChild.FlatStyle = FlatStyle.Flat;
			_btnAddChild.Location = new Point(240, 60);
			_btnAddChild.Name = "btnAddChild";
			_btnAddChild.Size = new Size(124, 24);
			_btnAddChild.TabIndex = 29;
			_btnAddChild.Text = "Add Child Layout";
			_btnAddChild.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			_btnDeleteLayout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnDeleteLayout.FlatStyle = FlatStyle.Flat;
			_btnDeleteLayout.Location = new Point(240, 112);
			_btnDeleteLayout.Name = "deleteLayout";
			_btnDeleteLayout.Size = new Size(124, 24);
			_btnDeleteLayout.TabIndex = 26;
			_btnDeleteLayout.Text = "Delete";
			_btnDeleteLayout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnDeleteLayout.Click += OnDeleteLayoutClick;
			_btnSaveCurrentLayout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnSaveCurrentLayout.Enabled = false;
			_btnSaveCurrentLayout.FlatStyle = FlatStyle.Flat;
			_btnSaveCurrentLayout.Location = new Point(240, 8);
			_btnSaveCurrentLayout.Name = "saveCurrentLayout";
			_btnSaveCurrentLayout.Size = new Size(124, 24);
			_btnSaveCurrentLayout.TabIndex = 25;
			_btnSaveCurrentLayout.Text = "Save Current Layout";
			_btnSaveCurrentLayout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnSaveCurrentLayout.Click += OnSaveCurrentLayoutClick;
			_btnCopyLayout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnCopyLayout.FlatStyle = FlatStyle.Flat;
			_btnCopyLayout.Location = new Point(240, 86);
			_btnCopyLayout.Name = "copyLayout";
			_btnCopyLayout.Size = new Size(124, 24);
			_btnCopyLayout.TabIndex = 24;
			_btnCopyLayout.Text = "Copy Layout...";
			_btnCopyLayout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnCopyLayout.Click += OnCopyLayoutClick;
			_btnNewLayout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnNewLayout.FlatStyle = FlatStyle.Flat;
			_btnNewLayout.Location = new Point(240, 34);
			_btnNewLayout.Name = "newLayout";
			_btnNewLayout.Size = new Size(124, 24);
			_btnNewLayout.TabIndex = 23;
			_btnNewLayout.Text = "New Layout";
			_btnNewLayout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnNewLayout.Click += OnNewLayoutClick;
			_btnModify.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnModify.FlatStyle = FlatStyle.Flat;
			_btnModify.Location = new Point(240, 138);
			_btnModify.Name = "btnModify";
			_btnModify.Size = new Size(124, 24);
			_btnModify.TabIndex = 27;
			_btnModify.Text = "Modify...";
			_btnModify.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnModify.Click += OnBtnModifyClick;
			base.Controls.AddRange(new Control[4] { _buttonPanel, _splitter, _layoutTree, _tbLayouts });
			base.DockPadding.All = 2;
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "LayoutTreePage";
			base.Size = new Size(649, 317);
			_buttonPanel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		bool IManagerEditorControl.CommitChanges()
		{
			return true;
		}

		private void RefreshLayoutTree()
		{
			_layoutTree.Nodes.Clear();
			if (_commandManager != null)
			{
				CommandBarsLayout currentLayout = _commandManager.CurrentLayout;
				if (currentLayout == null)
				{
					currentLayout = _commandManager.GetLayout();
					TreeNode treeNode = CreateCommandBarsLayoutNode(currentLayout);
					treeNode.ImageIndex = CurrentLayoutImageIndex;
					treeNode.SelectedImageIndex = CurrentLayoutImageIndex;
					_layoutTree.Nodes.Add(treeNode);
					_btnSaveCurrentLayout.Enabled = true;
					_layoutTree.SelectedNode = treeNode;
				}
				else
				{
					_btnSaveCurrentLayout.Enabled = false;
				}
				foreach (CommandBarsLayout layout in _commandManager.Layouts)
				{
					_layoutTree.Nodes.Add(CreateCommandBarsLayoutNode(layout));
				}
				if (_commandManager.CurrentLayout != null)
				{
					SelectNodeForLayout(_commandManager.CurrentLayout);
				}
			}
			else
			{
				DockPanelLayout currentPanelLayout = _panelManager.CurrentLayout;
				if (currentPanelLayout == null)
				{
					currentPanelLayout = _panelManager.GetLayout();
					TreeNode panelNode = CreatePanelLayoutNode(currentPanelLayout);
					panelNode.ImageIndex = CurrentLayoutImageIndex;
					panelNode.SelectedImageIndex = CurrentLayoutImageIndex;
					_layoutTree.Nodes.Add(panelNode);
					_btnSaveCurrentLayout.Enabled = true;
					_layoutTree.SelectedNode = panelNode;
				}
				else
				{
					_btnSaveCurrentLayout.Enabled = false;
				}
				foreach (DockPanelLayout panelLayout in _panelManager.Layouts)
				{
					_layoutTree.Nodes.Add(CreatePanelLayoutNode(panelLayout));
				}
				if (_panelManager.CurrentLayout != null)
				{
					SelectNodeForLayout(_panelManager.CurrentLayout);
				}
			}
			_layoutTree.Focus();
			UpdateButtonStates();
		}

		private TreeNode CreatePanelLayoutNode(DockPanelLayout layout)
		{
			string text = "";
			text = ((layout.GetOwningCollection() != null) ? ("[" + layout.GetOwningCollection().IndexOf(layout) + "] - '" + layout.Key + "'") : "Draft");
			TreeNode treeNode = new TreeNode(text, LayoutImageIndex, LayoutImageIndex);
			if (layout._isCurrentLayout)
			{
				treeNode.SelectedImageIndex = CurrentLayoutImageIndex;
				treeNode.ImageIndex = CurrentLayoutImageIndex;
			}
			treeNode.Tag = layout;
			foreach (DockPanelLayout childLayout in layout.ChildLayouts)
			{
				treeNode.Nodes.Add(CreatePanelLayoutNode(childLayout));
			}
			treeNode.Expand();
			return treeNode;
		}

		private TreeNode CreateCommandBarsLayoutNode(CommandBarsLayout layout)
		{
			string text = "";
			text = ((layout.GetOwningCollection() != null) ? ("[" + layout.GetOwningCollection().IndexOf(layout) + "] - '" + layout.Key + "'") : "Draft");
			TreeNode treeNode = new TreeNode(text, LayoutImageIndex, LayoutImageIndex);
			if (layout._isCurrentLayout)
			{
				treeNode.SelectedImageIndex = CurrentLayoutImageIndex;
				treeNode.ImageIndex = CurrentLayoutImageIndex;
			}
			treeNode.Tag = layout;
			foreach (CommandBarsLayout childLayout in layout.ChildLayouts)
			{
				treeNode.Nodes.Add(CreateCommandBarsLayoutNode(childLayout));
			}
			treeNode.Expand();
			return treeNode;
		}

		private void OnDeleteLayoutClick(object sender, EventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode == null)
			{
				return;
			}
			TreeNode selectedNode = _layoutTree.SelectedNode;
			if (selectedNode.Tag is CommandBarsLayout)
			{
				CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)selectedNode.Tag;
				if (uiCommandBarsLayout.GetOwningCollection() != null)
				{
					uiCommandBarsLayout.GetOwningCollection().Remove(uiCommandBarsLayout);
				}
			}
			else if (selectedNode.Tag is TabLayout)
			{
				TabLayout uiTabLayout = (TabLayout)selectedNode.Tag;
				if (uiTabLayout.GetOwnerCollection() != null)
				{
					uiTabLayout.GetOwnerCollection().Remove(uiTabLayout);
				}
			}
			else if (selectedNode.Tag is DockPanelLayout)
			{
				DockPanelLayout uiPanelLayout = (DockPanelLayout)selectedNode.Tag;
				if (uiPanelLayout.GetOwningCollection() != null)
				{
					uiPanelLayout.GetOwningCollection().Remove(uiPanelLayout);
				}
			}
			RefreshLayoutTree();
			UpdateButtonStates();
			OnDataChanged(EventArgs.Empty);
		}

		private void OnSaveCurrentLayoutClick(object sender, EventArgs eventArgs)
		{
			CommandManagerDialog commandManagerDialog = new CommandManagerDialog();
			commandManagerDialog.SetLayoutName("");
			if (commandManagerDialog.ShowDialog() == DialogResult.OK)
			{
				if (_commandManager != null)
				{
					CommandBarsLayout layout = _commandManager.GetLayout();
					_commandManager.Layouts.Add(layout);
					try
					{
						layout.Key = commandManagerDialog.GetLayoutName();
					}
					catch (InvalidOperationException)
					{
						int i;
						for (i = 1; _commandManager.Layouts.Contains("Layout" + i); i++)
						{
						}
						layout.Key = "Layout" + i;
					}
					_commandManager.CurrentLayout = layout;
				}
				else
				{
					DockPanelLayout panelLayout = _panelManager.GetLayout();
					_panelManager.Layouts.Add(panelLayout);
					try
					{
						panelLayout.Key = commandManagerDialog.GetLayoutName();
					}
					catch (InvalidOperationException)
					{
						int j;
						for (j = 1; _panelManager.Layouts.Contains("Layout" + j); j++)
						{
						}
						panelLayout.Key = "Layout" + j;
					}
					_panelManager.CurrentLayout = panelLayout;
				}
				RefreshLayoutTree();
				OnDataChanged(EventArgs.Empty);
			}
			commandManagerDialog.Dispose();
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
				if (_commandManager != null)
				{
					CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)selectedNode.Tag;
					if (uiCommandBarsLayout == _commandManager.CurrentLayout)
					{
						uiCommandBarsLayout.Update();
					}
					CommandBarsLayout copiedLayout = uiCommandBarsLayout.CloneLayout(includeChildren: true);
					copiedLayout.Key = "CopyOf" + uiCommandBarsLayout.Key;
					if (uiCommandBarsLayout.GetOwningCollection() != null)
					{
						if (uiCommandBarsLayout.GetOwningCollection().Contains(copiedLayout.Key))
						{
							int i;
							for (i = 1; uiCommandBarsLayout.GetOwningCollection().Contains(copiedLayout.Key + i); i++)
							{
							}
							copiedLayout.Key += i;
						}
						uiCommandBarsLayout.GetOwningCollection().Add(copiedLayout);
					}
					_commandManager.CurrentLayout = copiedLayout;
				}
				else
				{
					DockPanelLayout uiPanelLayout = (DockPanelLayout)selectedNode.Tag;
					if (uiPanelLayout == _panelManager.CurrentLayout)
					{
						uiPanelLayout.Update();
					}
					DockPanelLayout copiedLayout = uiPanelLayout.CloneLayout(includeChildren: true);
					copiedLayout.Key = "CopyOf" + uiPanelLayout.Key;
					if (uiPanelLayout.GetOwningCollection() != null)
					{
						if (uiPanelLayout.GetOwningCollection().Contains(copiedLayout.Key))
						{
							int j;
							for (j = 1; uiPanelLayout.GetOwningCollection().Contains(copiedLayout.Key + j); j++)
							{
							}
							copiedLayout.Key += j;
						}
						uiPanelLayout.GetOwningCollection().Add(copiedLayout);
					}
					_panelManager.CurrentLayout = copiedLayout;
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
				if (selectedNode == null)
				{
					return;
				}
				if (_commandManager != null)
				{
					CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)selectedNode.Tag;
					if (uiCommandBarsLayout.GetOwningCollection() == null)
					{
						RaiseSelectTab(EventArgs.Empty);
						return;
					}
					if (_commandManager.CurrentLayout == null)
					{
						if (MessageBox.Show("You are about to select a layout that will overwrite all the settings that you may have set in the control.\n\rTo save the current layout in the Layouts Collection of the control, cancel this operation and click in the 'Save Current Layout' button.\n\rDo you want to continue?", "CommandManager", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
						{
							return;
						}
						OnDataChanged(EventArgs.Empty);
					}
					else
					{
						_commandManager.CurrentLayout.Update();
					}
					if (_commandManager.CurrentLayout != uiCommandBarsLayout)
					{
						OnDataChanged(EventArgs.Empty);
					}
					_commandManager.CurrentLayout = uiCommandBarsLayout;
					return;
				}
				DockPanelLayout uiPanelLayout = (DockPanelLayout)selectedNode.Tag;
				if (uiPanelLayout.GetOwningCollection() == null)
				{
					RaiseSelectTab(EventArgs.Empty);
				}
				else
				{
					if (_panelManager.CurrentLayout == null)
					{
						if (MessageBox.Show("You are about to select a layout that will overwrite all the settings that you may have set in the control.\n\rTo save the current layout in the Layouts Collection of the control, cancel this operation and click in the 'Save Current Layout' button.\n\rDo you want to continue?", "PanelManager", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
						{
							return;
						}
						OnDataChanged(EventArgs.Empty);
					}
					else
					{
						_panelManager.CurrentLayout.Update();
					}
					if (_panelManager.CurrentLayout != uiPanelLayout)
					{
						if (WouldDisposeControls(_panelManager, uiPanelLayout) && MessageBox.Show("You are about to select a layout that contains a different set of panels.\n\rIf the layout is applied controls contained by the panels not present in the new layout will be disposed.\n\nDo you want to load the layout and lose the child controls?", "PanelManager", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.No)
						{
							return;
						}
						OnDataChanged(EventArgs.Empty);
					}
					_panelManager.CurrentLayout = uiPanelLayout;
				}
				RaiseSelectTab(EventArgs.Empty);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private bool WouldDisposeControls(MxDockManager currentManager, DockPanelLayout layout)
		{
			if (layout == null)
			{
				ArrayList arrayList = currentManager.GetAllPanels();
				foreach (DockPanelBase item in arrayList)
				{
					if (HasChildControls(item))
					{
						return true;
					}
				}
				return false;
			}
			MxDockManager uiPanelManager = new MxDockManager();
			uiPanelManager.LoadLayout(layout);
			ArrayList layoutPanels = uiPanelManager.GetAllPanels();
			ArrayList currentPanels = currentManager.GetAllPanels();
			foreach (DockPanelBase currentPanel in currentPanels)
			{
				if (HasChildControls(currentPanel) && FindPanelById(layoutPanels, currentPanel.Id) == null)
				{
					return true;
				}
			}
			return false;
		}

		private bool HasChildControls(DockPanelBase uiPanelBase)
		{
			if (uiPanelBase is MxDockPanel)
			{
				MxDockPanel uiPanel = (MxDockPanel)uiPanelBase;
				if (uiPanel.InnerContainer.Controls.Count > 0)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		private DockPanelBase FindPanelById(ArrayList arrayList, Guid guid)
		{
			foreach (DockPanelBase item in arrayList)
			{
				if (item.Id == guid)
				{
					return item;
				}
			}
			return null;
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
			if (eventArgs.Button == _loadButton)
			{
				if (_openFileDialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				using (Stream stream = _openFileDialog.OpenFile())
				{
					if (_commandManager != null)
					{
						_commandManager.LoadLayoutFile(stream);
					}
					else
					{
						if (_panelManager.Panels.Count > 0 && MessageBox.Show("When the layout is loaded, controls contained by the panels will be disposed.\n\nDo you want to load the layout and lose the child controls?", "PanelManager", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.No)
						{
							return;
						}
						_panelManager.LoadLayoutFile(stream);
					}
				}
				RefreshLayoutTree();
				OnDataChanged(EventArgs.Empty);
			}
			else if (eventArgs.Button == _saveButton && _saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				Stream saveStream = _saveFileDialog.OpenFile();
				if (_commandManager != null)
				{
					_commandManager.SaveLayoutFile(saveStream);
				}
				else
				{
					_panelManager.SaveLayoutFile(saveStream);
				}
				saveStream.Flush();
				saveStream.Position = 0L;
				saveStream.Close();
				OnDataChanged(EventArgs.Empty);
			}
		}

		private void OnNewLayoutClick(object sender, EventArgs eventArgs)
		{
			int i = 1;
			if (_commandManager != null)
			{
				CommandBarsLayout uiCommandBarsLayout = new CommandBarsLayout();
				for (; _commandManager.Layouts.Contains("Layout" + i); i++)
				{
				}
				uiCommandBarsLayout.Key = "Layout" + i;
				_commandManager.Layouts.Add(uiCommandBarsLayout);
				RefreshLayoutTree();
				SelectNodeForLayout(uiCommandBarsLayout);
			}
			else
			{
				DockPanelLayout uiPanelLayout = new DockPanelLayout();
				for (; _panelManager.Layouts.Contains("Layout" + i); i++)
				{
				}
				uiPanelLayout.Key = "Layout" + i;
				_panelManager.Layouts.Add(uiPanelLayout);
				RefreshLayoutTree();
				SelectNodeForLayout(uiPanelLayout);
			}
			OnDataChanged(EventArgs.Empty);
		}

		private void RaiseSelectTab(EventArgs eventArgs)
		{
			if (base.Events[_selectTabKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnLayoutTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode == null)
			{
				_layoutProperties.SelectedObject = null;
			}
			else if (_commandManager != null && ((CommandBarsLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() == null)
			{
				_layoutProperties.SelectedObject = null;
			}
			else if (_panelManager != null && ((DockPanelLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() == null)
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
			_btnModify.Enabled = hasSelection;
			if (_commandManager != null)
			{
				_btnCopyLayout.Enabled = hasSelection && ((CommandBarsLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
				_btnDeleteLayout.Enabled = hasSelection && ((CommandBarsLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
				_btnAddChild.Enabled = hasSelection && ((CommandBarsLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
			}
			else
			{
				_btnCopyLayout.Enabled = hasSelection && ((DockPanelLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
				_btnDeleteLayout.Enabled = hasSelection && ((DockPanelLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
				_btnAddChild.Enabled = hasSelection && ((DockPanelLayout)_layoutTree.SelectedNode.Tag).GetOwningCollection() != null;
			}
		}

		private void OnBtnAddChildClick(object sender, EventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode == null)
			{
				return;
			}
			if (_commandManager != null)
			{
				CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)_layoutTree.SelectedNode.Tag;
				CommandBarsLayout childLayout = new CommandBarsLayout();
				int i;
				for (i = 1; uiCommandBarsLayout.ChildLayouts.Contains("Layout" + i); i++)
				{
				}
				childLayout.Key = "Layout" + i;
				uiCommandBarsLayout.ChildLayouts.Add(childLayout);
				RefreshLayoutTree();
				SelectNodeForLayout(childLayout);
			}
			else
			{
				DockPanelLayout uiPanelLayout = (DockPanelLayout)_layoutTree.SelectedNode.Tag;
				DockPanelLayout childLayout = new DockPanelLayout();
				int j;
				for (j = 1; uiPanelLayout.ChildLayouts.Contains("Layout" + j); j++)
				{
				}
				childLayout.Key = "Layout" + j;
				uiPanelLayout.ChildLayouts.Add(childLayout);
				RefreshLayoutTree();
				SelectNodeForLayout(childLayout);
			}
			OnDataChanged(EventArgs.Empty);
		}

		private void SelectNodeForLayout(object value)
		{
			_layoutTree.SelectedNode = FindNode(_layoutTree.Nodes, value);
		}

		private TreeNode FindNode(TreeNodeCollection treeNodeCollection, object value)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (item.Tag == value)
				{
					return item;
				}
				TreeNode foundNode = FindNode(item.Nodes, value);
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
	}
}
