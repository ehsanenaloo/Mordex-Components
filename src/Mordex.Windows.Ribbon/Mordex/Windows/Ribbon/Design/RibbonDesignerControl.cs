using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	[ToolboxItem(false)]
	internal class RibbonDesignerControl : UserControl, IRibbonDesignerPage
	{
		private IContainer _components;

		private Label _layoutLabel;

		private ComboBox _comboBox;

		private EventHandler _dataChangedHandler;

		private EventHandler _disableCancelHandler;

		private Label _nameLabel;

		private Panel _bodyPanel;

		private Panel _rightPanel;

		private Splitter _splitter;

		private TreeView _tree;

		private ImageList _treeImages;

		private MxRibbon _ribbon;

		private MxRibbonStatusBar _statusBar;

		private bool _isSelectingNode;

		private object _selectedValue;

		private IRibbonItemEditor _itemEditor;

		private Image _editorItemImage;

		private static object s_selectTabEventKey = new object();

		MxRibbon IRibbonDesignerPage.Ribbon
		{
			get
			{
				return _ribbon;
			}
			set
			{
				if (_ribbon != value)
				{
					_ribbon = value;
					ShowRibbonDesigner();
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
				if (_statusBar != value)
				{
					_statusBar = value;
					ShowStatusBarDesigner();
				}
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
					EventHandler combinedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, combinedHandler, previousHandler);
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
					EventHandler combinedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		event EventHandler IRibbonDesignerPage.SelectTab
		{
			add
			{
				base.Events.AddHandler(s_selectTabEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_selectTabEventKey, value);
			}
		}

		internal RibbonDesignerControl()
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			_components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(RibbonDesignerControl));
			_nameLabel = new Label();
			_bodyPanel = new Panel();
			_rightPanel = new Panel();
			_splitter = new Splitter();
			_tree = new TreeView();
			_treeImages = new ImageList(_components);
			_comboBox = new ComboBox();
			_layoutLabel = new Label();
			_bodyPanel.SuspendLayout();
			SuspendLayout();
			_nameLabel.BackColor = SystemColors.ControlDark;
			_nameLabel.Dock = DockStyle.Top;
			_nameLabel.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_nameLabel.ForeColor = SystemColors.ControlLightLight;
			_nameLabel.Location = new Point(0, 0);
			_nameLabel.Name = "lblName";
			_nameLabel.Size = new Size(672, 24);
			_nameLabel.TabIndex = 3;
			_nameLabel.Text = " Ribbon Designer";
			_nameLabel.TextAlign = ContentAlignment.MiddleLeft;
			_bodyPanel.Controls.Add(_rightPanel);
			_bodyPanel.Controls.Add(_splitter);
			_bodyPanel.Controls.Add(_tree);
			_bodyPanel.Dock = DockStyle.Fill;
			_bodyPanel.Location = new Point(0, 24);
			_bodyPanel.Name = "bodyPanel";
			_bodyPanel.Padding = new Padding(4);
			_bodyPanel.Size = new Size(672, 392);
			_bodyPanel.TabIndex = 4;
			_rightPanel.Dock = DockStyle.Fill;
			_rightPanel.Location = new Point(216, 4);
			_rightPanel.Name = "rightPanel";
			_rightPanel.Size = new Size(452, 384);
			_rightPanel.TabIndex = 2;
			_splitter.Location = new Point(208, 4);
			_splitter.Name = "splitter1";
			_splitter.Size = new Size(8, 384);
			_splitter.TabIndex = 1;
			_splitter.TabStop = false;
			_tree.Dock = DockStyle.Left;
			_tree.HideSelection = false;
			_tree.ImageIndex = 3;
			_tree.ImageList = _treeImages;
			_tree.Location = new Point(4, 4);
			_tree.Name = "tree";
			_tree.SelectedImageIndex = 0;
			_tree.Size = new Size(204, 384);
			_tree.TabIndex = 0;
			_tree.AfterSelect += OnTreeAfterSelect;
			ImageStrip.Load(_treeImages, componentResourceManager, "imgIcons.ImageStrip");
			_treeImages.TransparentColor = Color.Transparent;
			_treeImages.Images.SetKeyName(0, "ContextualOK.ico");
			_treeImages.Images.SetKeyName(1, "Group.ico");
			_treeImages.Images.SetKeyName(2, "Groups.ico");
			_treeImages.Images.SetKeyName(3, "Ribbon.ico");
			_treeImages.Images.SetKeyName(4, "Tab.ico");
			_treeImages.Images.SetKeyName(5, "Tabs.ico");
			_treeImages.Images.SetKeyName(6, "Texts.ico");
			_treeImages.Images.SetKeyName(7, "ButtonGroup.ico");
			_treeImages.Images.SetKeyName(8, "CheckBox.ico");
			_treeImages.Images.SetKeyName(9, "ComboBox.ico");
			_treeImages.Images.SetKeyName(10, "Command.ico");
			_treeImages.Images.SetKeyName(11, "CommandBox.ico");
			_treeImages.Images.SetKeyName(12, "ContainerControl.ico");
			_treeImages.Images.SetKeyName(13, "ControlBoxMenu2.ico");
			_treeImages.Images.SetKeyName(14, "EditBox.ico");
			_treeImages.Images.SetKeyName(15, "Gallery.ico");
			_treeImages.Images.SetKeyName(16, "GalleryItems.ico");
			_treeImages.Images.SetKeyName(17, "Label.ico");
			_treeImages.Images.SetKeyName(18, "QuickAccess.ico");
			_treeImages.Images.SetKeyName(19, "Separator.ico");
			_treeImages.Images.SetKeyName(20, "CommandsCollection.ico");
			_treeImages.Images.SetKeyName(21, "CommandDropDown.ico");
			_comboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_comboBox.BackColor = SystemColors.Window;
			_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_comboBox.ForeColor = SystemColors.WindowText;
			_comboBox.Location = new Point(520, 1);
			_comboBox.Name = "ComboBox1";
			_comboBox.Size = new Size(148, 21);
			_comboBox.TabIndex = 0;
			_comboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			_layoutLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_layoutLabel.AutoSize = true;
			_layoutLabel.BackColor = SystemColors.ControlDark;
			_layoutLabel.ForeColor = SystemColors.ControlLightLight;
			_layoutLabel.Location = new Point(470, 6);
			_layoutLabel.Name = "layoutLabel";
			_layoutLabel.Size = new Size(44, 13);
			_layoutLabel.TabIndex = 0;
			_layoutLabel.Text = "Layout:";
			base.Controls.Add(_layoutLabel);
			base.Controls.Add(_comboBox);
			base.Controls.Add(_bodyPanel);
			base.Controls.Add(_nameLabel);
			Mordex.Windows.Common.MxHeaderLayout.Attach(_nameLabel, _layoutLabel, _comboBox);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "GeneralPage";
			base.Size = new Size(672, 416);
			_bodyPanel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_components?.Dispose();
				_editorItemImage?.Dispose();
				_editorItemImage = null;
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

		protected virtual void OnDisableCancel(EventArgs eventArgs)
		{
			if (_disableCancelHandler != null)
			{
				_disableCancelHandler(this, eventArgs);
			}
		}

		private void PopulateLayoutComboBox()
		{
			_comboBox.SelectedValueChanged -= OnComboBoxSelectedValueChanged;
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new DictionaryEntry("(none)", null));
			AddLayoutEntries(arrayList, _ribbon.Layouts, 0);
			_comboBox.ValueMember = "Value";
			_comboBox.DisplayMember = "Key";
			_comboBox.DataSource = arrayList;
			_comboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			if (_ribbon.CurrentLayout != null)
			{
				_comboBox.SelectedValue = _ribbon.CurrentLayout;
			}
			else
			{
				_comboBox.SelectedIndex = -1;
			}
		}

		private void AddLayoutEntries(ArrayList arrayList, RibbonLayoutCollection ribbonLayoutCollection, int depth)
		{
			foreach (RibbonLayout item in ribbonLayoutCollection)
			{
				arrayList.Add(new DictionaryEntry(new string(' ', depth * 3) + FormatLayoutLabel(ribbonLayoutCollection.IndexOf(item), item.Key), item));
				AddLayoutEntries(arrayList, item.ChildLayouts, depth + 1);
			}
		}

		private string FormatLayoutLabel(int layoutIndex, string text)
		{
			return "[" + layoutIndex + "] - '" + text + "'";
		}

		private void ShowRibbonDesigner()
		{
			_layoutLabel.Visible = true;
			_comboBox.Visible = true;
			PopulateLayoutComboBox();
			_nameLabel.Text = "Ribbon Designer";
			ReloadDesigner();
		}

		private void ShowStatusBarDesigner()
		{
			_layoutLabel.Visible = false;
			_comboBox.Visible = false;
			_nameLabel.Text = "StatusBar Designer";
			ReloadDesigner();
		}

		private void ReloadDesigner()
		{
			SetSelectedValue(null);
			RebuildTree();
			if (_ribbon != null)
			{
				SelectNodeByTag(_ribbon);
			}
			else
			{
				SelectNodeByTag(_statusBar);
			}
		}

		private void SelectNodeByTag(object value)
		{
			TrySelectNodeByTag(_tree.Nodes, value);
		}

		private bool TrySelectNodeByText(TreeNodeCollection treeNodeCollection, string text)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (item.Text == text)
				{
					_isSelectingNode = true;
					_tree.SelectedNode = item;
					if (item != null)
					{
						item.EnsureVisible();
						item.Expand();
					}
					_isSelectingNode = false;
					return true;
				}
				if (TrySelectNodeByText(item.Nodes, text))
				{
					return true;
				}
			}
			return false;
		}

		private bool TrySelectNodeByTag(TreeNodeCollection treeNodeCollection, object value)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (item.Tag == value)
				{
					_tree.SelectedNode = item;
					if (item != null)
					{
						item.EnsureVisible();
						item.Expand();
					}
					return true;
				}
				if (TrySelectNodeByTag(item.Nodes, value))
				{
					return true;
				}
			}
			return false;
		}

		private void RebuildTree()
		{
			_tree.Nodes.Clear();
			if (_ribbon != null)
			{
				_tree.Nodes.Add(CreateRibbonNode(_ribbon));
			}
			else
			{
				_tree.Nodes.Add(CreateStatusBarNode(_statusBar));
			}
			int width = _tree.Width;
			_tree.Width = 0;
			_tree.Width = width;
		}

		private TreeNode CreateBuiltInTextsNode(BuiltInTextList builtInTextList)
		{
			TreeNode treeNode = new TreeNode("BuiltInTexts", 6, 6);
			treeNode.Tag = builtInTextList;
			return treeNode;
		}

		private TreeNode CreateBackstageMenuNode(BackstageMenu backstageMenu)
		{
			TreeNode treeNode = new TreeNode("Backstage Menu", 13, 13);
			treeNode.Tag = backstageMenu;
			treeNode.Nodes.Add(CreateCommandCollectionNode(backstageMenu.Commands, "Commands", 20));
			return treeNode;
		}

		private TreeNode CreateControlBoxMenuNode(ControlBoxMenu controlBoxMenu)
		{
			TreeNode treeNode = new TreeNode("ControlBox Menu", 13, 13);
			treeNode.Tag = controlBoxMenu;
			treeNode.Nodes.Add(CreateCommandCollectionNode(controlBoxMenu.LeftCommands, "Left Commands", 20));
			treeNode.Nodes.Add(CreateCommandCollectionNode(controlBoxMenu.RightCommands, "Right Commands", 20));
			treeNode.Nodes.Add(CreateCommandCollectionNode(controlBoxMenu.BottomCommands, "Bottom Commands", 20));
			return treeNode;
		}

		private TreeNode CreateContextualTabsNode(RibbonContextualTabCollection ribbonContextualTabCollection)
		{
			TreeNode treeNode = new TreeNode("ContextualTabs", 0, 0);
			treeNode.Tag = ribbonContextualTabCollection;
			foreach (RibbonContextualTab item in ribbonContextualTabCollection)
			{
				treeNode.Nodes.Add(CreateContextualTabNode(item));
			}
			return treeNode;
		}

		private TreeNode CreateContextualTabNode(RibbonContextualTab ribbonContextualTab)
		{
			TreeNode treeNode = new TreeNode(RibbonDrawingUtils.GetIndexKeyLabel(ribbonContextualTab), 0, 0);
			treeNode.Tag = ribbonContextualTab;
			treeNode.Nodes.Add(CreateTabsNode(ribbonContextualTab.Tabs));
			return treeNode;
		}

		private TreeNode CreateTabNode(RibbonTab ribbonTab)
		{
			TreeNode treeNode = new TreeNode(RibbonDrawingUtils.GetIndexKeyLabel(ribbonTab), 4, 4);
			treeNode.Nodes.Add(CreateGroupsNode(ribbonTab.Groups));
			treeNode.Tag = ribbonTab;
			return treeNode;
		}

		private TreeNode CreateGalleryItemsNode(GalleryItemCollection galleryItemCollection)
		{
			TreeNode treeNode = new TreeNode("Gallery Items", 15, 15);
			treeNode.Tag = galleryItemCollection;
			foreach (GalleryItem item in galleryItemCollection)
			{
				treeNode.Nodes.Add(CreateCommandNode(item));
			}
			return treeNode;
		}

		private TreeNode CreateGroupsNode(RibbonGroupCollection ribbonGroupCollection)
		{
			TreeNode treeNode = new TreeNode("Groups", 2, 2);
			treeNode.Tag = ribbonGroupCollection;
			foreach (CommandBase item in ribbonGroupCollection)
			{
				treeNode.Nodes.Add(CreateCommandNode(item));
			}
			return treeNode;
		}

		private TreeNode CreateTabsNode(RibbonTabCollection ribbonTabCollection)
		{
			TreeNode treeNode = new TreeNode("Tabs", 5, 5);
			foreach (RibbonTab item in ribbonTabCollection)
			{
				treeNode.Nodes.Add(CreateTabNode(item));
			}
			treeNode.Tag = ribbonTabCollection;
			return treeNode;
		}

		private TreeNode CreateCommandCollectionNode(CommandCollection commandCollection, string text, int imageIndex)
		{
			TreeNode treeNode = new TreeNode(text, imageIndex, imageIndex);
			treeNode.Tag = commandCollection;
			foreach (CommandBase item in commandCollection)
			{
				treeNode.Nodes.Add(CreateCommandNode(item));
			}
			return treeNode;
		}

		private TreeNode CreateCommandNode(CommandBase commandBase)
		{
			int imageIndex = 10;
			switch (commandBase.CommandType)
			{
			case CommandType.RibbonGroup:
				imageIndex = 1;
				break;
			case CommandType.Box:
				imageIndex = 11;
				break;
			case CommandType.ButtonGroup:
				imageIndex = 7;
				break;
			case CommandType.ContainerControl:
				imageIndex = ((!(commandBase is ComboBoxCommand)) ? ((!(commandBase is TextBoxCommand)) ? ((!(commandBase is CheckBoxCommand)) ? 12 : 8) : 14) : 9);
				break;
			case CommandType.DropDownButton:
				imageIndex = 21;
				break;
			case CommandType.Gallery:
				imageIndex = 15;
				break;
			case CommandType.GalleryItem:
				imageIndex = 16;
				break;
			case CommandType.Label:
				imageIndex = 17;
				break;
			case CommandType.Separator:
				imageIndex = 19;
				break;
			case CommandType.BackstageTab:
				imageIndex = 4;
				break;
			}
			TreeNode treeNode = new TreeNode(RibbonDrawingUtils.GetIndexKeyLabel(commandBase), imageIndex, imageIndex);
			treeNode.Tag = commandBase;
			if (commandBase is DropDownCommand)
			{
				treeNode.Nodes.Add(CreateCommandCollectionNode(((DropDownCommand)commandBase).Commands, "Commands", 20));
			}
			else if (commandBase is RibbonGroup)
			{
				treeNode.Nodes.Add(CreateCommandCollectionNode(((RibbonGroup)commandBase).Commands, "Commands", 20));
			}
			else if (commandBase is CommandBox)
			{
				treeNode.Nodes.Add(CreateCommandCollectionNode(((CommandBox)commandBase).Commands, "Commands", 20));
			}
			else if (commandBase is ButtonGroup)
			{
				treeNode.Nodes.Add(CreateCommandCollectionNode(((ButtonGroup)commandBase).Commands, "Commands", 20));
			}
			if (commandBase is Gallery)
			{
				treeNode.Nodes.Add(CreateGalleryItemsNode(((Gallery)commandBase).GalleryItems));
			}
			return treeNode;
		}

		private TreeNode CreateRibbonNode(MxRibbon ribbon)
		{
			TreeNode treeNode = new TreeNode("Ribbon", 3, 3);
			treeNode.Nodes.Add(CreateTabsNode(ribbon.Tabs));
			treeNode.Nodes.Add(CreateContextualTabsNode(ribbon.ContextualTabs));
			treeNode.Nodes.Add(CreateControlBoxMenuNode(ribbon.ControlBoxMenu));
			treeNode.Nodes.Add(CreateBackstageMenuNode(ribbon.BackstageMenu));
			treeNode.Nodes.Add(CreateCommandCollectionNode(ribbon.QuickAccessCommands, "Quick Access Commands", 18));
			treeNode.Nodes.Add(CreateCommandCollectionNode(ribbon.QuickAccessCustomizeList, "Quick Access Customize List", 18));
			treeNode.Nodes.Add(CreateBuiltInTextsNode(ribbon.BuiltInTexts));
			treeNode.Tag = ribbon;
			treeNode.Expand();
			return treeNode;
		}

		private TreeNode CreateStatusBarNode(MxRibbonStatusBar ribbonStatusBar)
		{
			TreeNode treeNode = new TreeNode("RibbonStatusBar", 3, 3);
			treeNode.Nodes.Add(CreateCommandCollectionNode(ribbonStatusBar.LeftPanelCommands, "Left Panel commands", 20));
			treeNode.Nodes.Add(CreateCommandCollectionNode(ribbonStatusBar.RightPanelCommands, "Right Panel Commands", 20));
			treeNode.Tag = ribbonStatusBar;
			treeNode.Expand();
			return treeNode;
		}

		private void OnTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (!_isSelectingNode)
			{
				if (_tree.SelectedNode != null)
				{
					SetSelectedValue(_tree.SelectedNode.Tag);
				}
				else
				{
					SetSelectedValue(null);
				}
			}
		}

		private object GetSelectedValue()
		{
			return _selectedValue;
		}

		private void SetSelectedValue(object value)
		{
			if (_selectedValue == value)
			{
				return;
			}
			_selectedValue = value;
			if (_selectedValue == null)
			{
				SetItemEditor(null);
			}
			else if (!typeof(ICollection).IsAssignableFrom(_selectedValue.GetType()))
			{
				if (_selectedValue is BuiltInTextList)
				{
					SetItemEditor(new BuiltInTextListEditorControl(_ribbon.BuiltInTexts));
				}
				else
				{
					SetItemEditor(new RibbonPropertyEditorControl());
				}
			}
			else if (_selectedValue is CommandCollection)
			{
				if (_tree.SelectedNode?.Parent?.Tag is MxRibbon)
				{
					SetItemEditor(new QuickAccessEditorControl(_ribbon));
				}
				else if (_ribbon != null)
				{
					SetItemEditor(new RibbonItemListEditorControl(_ribbon));
				}
				else
				{
					SetItemEditor(new RibbonItemListEditorControl(_statusBar));
				}
			}
			else if (_ribbon != null)
			{
				SetItemEditor(new RibbonItemListEditorControl(_ribbon));
			}
			else
			{
				SetItemEditor(new RibbonItemListEditorControl(_statusBar));
			}
			HostItemEditor();
		}

		private IRibbonItemEditor GetItemEditor()
		{
			return _itemEditor;
		}

		private void SetItemEditor(IRibbonItemEditor itemEditor)
		{
			if (_itemEditor == itemEditor)
			{
				return;
			}
			if (_itemEditor != null)
			{
				if (!_itemEditor.CommitChanges())
				{
					return;
				}
				_itemEditor.SetSite(null);
				_itemEditor.DisableCancel -= OnStateDisableCancel;
				_itemEditor.SelectedObjectChanged -= OnStateSelectedObjectChanged;
				_itemEditor.RefreshTree -= OnStateRefreshTree;
				_itemEditor.DataChanged -= OnStateDataChanged;
				((Control)_itemEditor).Dispose();
			}
			_itemEditor = itemEditor;
			_editorItemImage?.Dispose();
			_editorItemImage = null;
			if (_itemEditor != null)
			{
				((Control)_itemEditor).Dock = DockStyle.Fill;
				int itemImageIndex = _tree.SelectedNode == null ? -1 : _tree.SelectedNode.ImageIndex + 1;
				if (itemImageIndex >= 0 && itemImageIndex < _treeImages.Images.Count)
				{
					// The list returns a copy that this control owns and disposes.
					_editorItemImage = _treeImages.Images[itemImageIndex];
					_itemEditor.ItemImage = _editorItemImage;
				}
				_itemEditor.SelectedObject = GetSelectedValue();
				_itemEditor.DisableCancel += OnStateDisableCancel;
				_itemEditor.SelectedObjectChanged += OnStateSelectedObjectChanged;
				_itemEditor.RefreshTree += OnStateRefreshTree;
				_itemEditor.DataChanged += OnStateDataChanged;
				if (_ribbon != null)
				{
					_itemEditor.SetSite(_ribbon.Site);
				}
				else
				{
					_itemEditor.SetSite(_statusBar.Site);
				}
			}
			HostItemEditor();
		}

		private void HostItemEditor()
		{
			if (_itemEditor != null)
			{
				((Control)_itemEditor).Dock = DockStyle.Fill;
				_itemEditor.SelectedObject = GetSelectedValue();
				_rightPanel.Controls.Add((Control)_itemEditor);
			}
		}

		private void OnStateDataChanged(object sender, EventArgs eventArgs)
		{
			OnDataChanged(eventArgs);
		}

		private void OnStateSelectedObjectChanged(object sender, EventArgs eventArgs)
		{
			HostItemEditor();
		}

		private void OnStateDisableCancel(object sender, EventArgs eventArgs)
		{
			OnDisableCancel(eventArgs);
		}

		private void OnStateRefreshTree(object sender, EventArgs eventArgs)
		{
			OnDataChanged(EventArgs.Empty);
			object selectedValue = _selectedValue;
			if (selectedValue == null)
			{
				string text = _tree.SelectedNode?.Text;
				RebuildTree();
				if (text != null)
				{
					TrySelectNodeByText(_tree.Nodes, text);
				}
				return;
			}
			RebuildTree();
			SelectNodeByTag(selectedValue);
			if (_tree.SelectedNode == null)
			{
				if (_ribbon != null)
				{
					SelectNodeByTag(_ribbon);
				}
				else
				{
					SelectNodeByTag(_statusBar);
				}
			}
		}

		private void OnComboBoxSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_ribbon != null && _ribbon.CurrentLayout != (RibbonLayout)_comboBox.SelectedValue && (_itemEditor == null || GetItemEditor().CommitChanges()))
			{
				SetItemEditor(null);
				if (_ribbon.CurrentLayout != null)
				{
					_ribbon.CurrentLayout.Update();
				}
				_ribbon.CurrentLayout = (RibbonLayout)_comboBox.SelectedValue;
				ReloadDesigner();
				OnDataChanged(EventArgs.Empty);
			}
		}

		void IRibbonDesignerPage.Unload()
		{
			SetItemEditor(null);
		}

		bool IRibbonDesignerPage.CommitChanges()
		{
			if (_itemEditor != null)
			{
				return _itemEditor.CommitChanges();
			}
			return true;
		}
	}
}
