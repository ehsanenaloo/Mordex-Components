using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	[ToolboxItem(false)]
	internal class DockManagerEditorControl : UserControl, IManagerEditorControl
	{
		private const int IconCommandManager = 0;

		private const int IconCommandBars = 1;

		private const int IconCommandBar = 2;

		private const int IconCommands = 3;

		private const int IconCommand = 4;

		private const int IconContextMenus = 5;

		private const int IconContextMenu = 6;

		private const int IconPredefinedStylesClosed = 7;

		private const int IconPredefinedStylesOpen = 8;

		private const int IconFormatStyle = 9;

		private const int IconPopupSettings = 10;

		private const int IconBuiltInTexts = 11;

		private const int IconCategories = 12;

		private const int IconCategory = 13;

		private const int IconStateStyles = 14;

		private const int IconPanelManager = 15;

		private const int IconPanelGroup = 18;

		private const int IconPanels = 19;

		private const int IconPanel = 21;

		private const int ImageSizePx = 16;

		private IContainer _components;

		private Label _layoutLabel;

		private ComboBox _layoutComboBox;

		private EventHandler _dataChangedHandler;

		private EventHandler _disableCancelHandler;

		private ImageList _imageList;

		private Label _lblName;

		private Panel _bodyPanel;

		private Panel _rightPanel;

		private Splitter _splitter;

		private TreeView _objectTree;

		private MxDockManager _panelManager;

		private MxCommandManager _commandManager;

		private bool _isSelectingNode;

		private object _selectedValue;

		private IUIElement _activeEditor;

		private static object _selectTabKey = new object();

		MxDockManager IManagerEditorControl.PanelManager
		{
			get
			{
				return _panelManager;
			}
			set
			{
				if (_panelManager != value)
				{
					_panelManager = value;
					RefreshAll();
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
				if (_commandManager != value)
				{
					_commandManager = value;
					RefreshAll();
				}
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler newHandler = (EventHandler)Delegate.Combine(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, newHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler newHandler = (EventHandler)Delegate.Remove(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, newHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler newHandler = (EventHandler)Delegate.Combine(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, newHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
			remove
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler newHandler = (EventHandler)Delegate.Remove(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, newHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
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

		internal DockManagerEditorControl()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
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

		bool IManagerEditorControl.CommitChanges()
		{
			if (_activeEditor != null)
			{
				return _activeEditor.CommitChanges();
			}
			return true;
		}

		private void FillLayoutCombo()
		{
			_layoutComboBox.SelectedValueChanged -= OnComboBoxSelectedValueChanged;
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new ON(null, "(none)"));
			if (_commandManager != null)
			{
				AddCommandBarLayouts(arrayList, _commandManager.Layouts, 0);
			}
			else
			{
				AddPanelLayouts(arrayList, _panelManager.Layouts, 0);
			}
			_layoutComboBox.ValueMember = "Object";
			_layoutComboBox.DisplayMember = "Name";
			_layoutComboBox.DataSource = arrayList;
			_layoutComboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			if (_commandManager != null)
			{
				if (_commandManager.CurrentLayout != null)
				{
					_layoutComboBox.SelectedValue = _commandManager.CurrentLayout;
				}
				else
				{
					_layoutComboBox.SelectedIndex = -1;
				}
			}
			else if (_panelManager.CurrentLayout != null)
			{
				_layoutComboBox.SelectedValue = _panelManager.CurrentLayout;
			}
			else
			{
				_layoutComboBox.SelectedIndex = -1;
			}
		}

		private void AddPanelLayouts(ArrayList arrayList, DockPanelLayoutCollection uiPanelLayoutCollection, int depth)
		{
			foreach (DockPanelLayout item in uiPanelLayoutCollection)
			{
				arrayList.Add(new ON(item, new string(' ', depth * 3) + FormatLayoutText(uiPanelLayoutCollection.IndexOf(item), item.Key)));
				AddPanelLayouts(arrayList, item.ChildLayouts, depth + 1);
			}
		}

		private void AddCommandBarLayouts(ArrayList arrayList, CommandBarsLayoutCollection uiCommandBarsLayoutCollection, int depth)
		{
			foreach (CommandBarsLayout item in uiCommandBarsLayoutCollection)
			{
				arrayList.Add(new ON(item, new string(' ', depth * 3) + FormatLayoutText(uiCommandBarsLayoutCollection.IndexOf(item), item.Key)));
				AddCommandBarLayouts(arrayList, item.ChildLayouts, depth + 1);
			}
		}

		private string FormatLayoutText(int index, string text)
		{
			return "[" + index + "] - '" + text + "'";
		}

		private void RefreshAll()
		{
			_layoutLabel.Visible = true;
			_layoutComboBox.Visible = true;
			FillLayoutCombo();
			if (_commandManager != null)
			{
				_lblName.Text = "CommandManager Designer";
			}
			else
			{
				_lblName.Text = "PanelManager Designer";
			}
			RebuildTreeAndSelectRoot();
		}

		private void RebuildTreeAndSelectRoot()
		{
			SetSelectedValue(null);
			BuildTreeNodes();
			if (_commandManager != null)
			{
				SelectNodeByTag(_commandManager);
			}
			else
			{
				SelectNodeByTag(_panelManager);
			}
		}

		private void SelectNodeByTag(object value)
		{
			TrySelectNodeByTag(_objectTree.Nodes, value);
		}

		private bool TrySelectNodeByText(TreeNodeCollection treeNodeCollection, string text)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (item.Text == text)
				{
					_isSelectingNode = true;
					_objectTree.SelectedNode = item;
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
					_objectTree.SelectedNode = item;
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

		private void BuildTreeNodes()
		{
			_objectTree.Nodes.Clear();
			if (_commandManager != null)
			{
				_objectTree.Nodes.Add(CreateCommandManagerNode(_commandManager));
			}
			else
			{
				_objectTree.Nodes.Add(CreatePanelManagerNode(_panelManager));
			}
			int width = _objectTree.Width;
			_objectTree.Width = 0;
			_objectTree.Width = width;
		}

		private TreeNode CreateChildNode(object value)
		{
			return CreateNode(value, "");
		}

		private TreeNode CreateNode(object value, string text)
		{
			TreeNode treeNode = null;
			if (value is MxCommandManager)
			{
				treeNode = CreateCommandManagerNode((MxCommandManager)value);
			}
			else if (value is CommandBarCollection)
			{
				treeNode = CreateCommandBarsNode((CommandBarCollection)value);
			}
			else if (value is MxCommandBar)
			{
				treeNode = CreateCommandBarNode((MxCommandBar)value);
			}
			else if (value is MxCommand)
			{
				treeNode = CreateCommandNode((MxCommand)value);
			}
			else if (value is MxCommandCollection)
			{
				treeNode = CreateCommandsNode((MxCommandCollection)value);
			}
			else if (value is ContextMenuCollection)
			{
				treeNode = CreateContextMenusNode((ContextMenuCollection)value);
			}
			else if (value is MxContextMenu)
			{
				treeNode = CreateContextMenuNode((MxContextMenu)value);
			}
			else if (value is StateStyles)
			{
				treeNode = CreateStateStylesNode((StateStyles)value, text);
			}
			else if (value is FormatStyle)
			{
				treeNode = CreateFormatStyleNode((FormatStyle)value, text);
			}
			else if (value is PopupSettings)
			{
				treeNode = CreatePopupSettingsNode((PopupSettings)value, text);
			}
			else if (value is CommandCategoryCollection)
			{
				treeNode = CreateCategoriesNode((CommandCategoryCollection)value);
			}
			else if (value is CommandCategory)
			{
				treeNode = CreateCategoryNode((CommandCategory)value);
			}
			treeNode.Tag = value;
			return treeNode;
		}

		private TreeNode CreateStateStylesNode(StateStyles uiStateStyles, string text)
		{
			TreeNode treeNode = new TreeNode(text, IconStateStyles, IconStateStyles);
			treeNode.Nodes.Add(CreateNode(uiStateStyles.DisabledFormatStyle, "DisabledFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiStateStyles.FormatStyle, "FormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiStateStyles.HotFormatStyle, "HotFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiStateStyles.PressedFormatStyle, "PressedFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiStateStyles.SelectedFormatStyle, "SelectedFormatStyle"));
			return treeNode;
		}

		private TreeNode CreateCommandBuiltInTextsNode(Mordex.Windows.UI.CommandBars.BuiltInTextList builtInTextList)
		{
			TreeNode treeNode = new TreeNode("BuiltInTexts", IconBuiltInTexts, IconBuiltInTexts);
			treeNode.Tag = builtInTextList;
			return treeNode;
		}

		private TreeNode CreatePanelBuiltInTextsNode(Mordex.Windows.UI.Docking.BuiltInTextList builtInTextList)
		{
			TreeNode treeNode = new TreeNode("BuiltInTexts", IconBuiltInTexts, IconBuiltInTexts);
			treeNode.Tag = builtInTextList;
			return treeNode;
		}

		private TreeNode CreateCategoriesNode(CommandCategoryCollection uiCommandCategoryCollection)
		{
			TreeNode treeNode = new TreeNode("Categories", IconCategories, IconCategories);
			treeNode.Tag = uiCommandCategoryCollection;
			foreach (CommandCategory item in uiCommandCategoryCollection)
			{
				treeNode.Nodes.Add(CreateChildNode(item));
			}
			return treeNode;
		}

		private TreeNode CreateCommandsNode(MxCommandCollection uiCommandCollection)
		{
			TreeNode treeNode = new TreeNode(RenderingHelper.GetCommandNodeText(uiCommandCollection), IconCommands, IconCommands);
			foreach (MxCommand item in uiCommandCollection)
			{
				item.SetHostElement(uiCommandCollection._owner);
				treeNode.Nodes.Add(CreateChildNode(item));
			}
			return treeNode;
		}

		private TreeNode CreatePanelsNode(DockPanelCollection uiPanelCollection)
		{
			TreeNode treeNode = new TreeNode("Panels", IconPanels, IconPanels);
			foreach (DockPanelBase item in (IEnumerable)uiPanelCollection)
			{
				if (item is MxDockPanelGroup)
				{
					treeNode.Nodes.Add(CreatePanelGroupNode(item as MxDockPanelGroup));
				}
				else
				{
					treeNode.Nodes.Add(CreatePanelNode(item));
				}
			}
			treeNode.Tag = uiPanelCollection;
			return treeNode;
		}

		private TreeNode CreatePanelGroupNode(MxDockPanelGroup uiPanelGroup)
		{
			TreeNode treeNode = new TreeNode(uiPanelGroup.Site.Name, IconPanelGroup, IconPanelGroup);
			treeNode.Nodes.Add(CreatePanelsNode(uiPanelGroup.Panels));
			treeNode.Tag = uiPanelGroup;
			return treeNode;
		}

		private TreeNode CreateCommandBarsNode(CommandBarCollection uiCommandBarCollection)
		{
			TreeNode treeNode = new TreeNode(RenderingHelper.GetCommandNodeText(uiCommandBarCollection), IconCommandBars, IconCommandBars);
			foreach (MxCommandBar item in uiCommandBarCollection)
			{
				treeNode.Nodes.Add(CreateChildNode(item));
			}
			return treeNode;
		}

		private TreeNode CreateContextMenusNode(ContextMenuCollection uiContextMenuCollection)
		{
			TreeNode treeNode = new TreeNode(RenderingHelper.GetCommandNodeText(uiContextMenuCollection), IconContextMenus, IconContextMenus);
			foreach (MxContextMenu item in uiContextMenuCollection)
			{
				treeNode.Nodes.Add(CreateChildNode(item));
			}
			return treeNode;
		}

		private TreeNode CreateCategoryNode(CommandCategory uiCommandCategory)
		{
			return new TreeNode(RenderingHelper.GetCommandNodeText(uiCommandCategory), IconCategory, IconCategory);
		}

		private TreeNode CreateCommandBarNode(MxCommandBar uiCommandBar)
		{
			TreeNode treeNode = new TreeNode(RenderingHelper.GetCommandNodeText(uiCommandBar), IconCommandBar, IconCommandBar);
			treeNode.Nodes.Add(CreateChildNode(uiCommandBar.Commands));
			return treeNode;
		}

		private TreeNode CreatePanelNode(DockPanelBase uiPanelBase)
		{
			TreeNode treeNode = ((uiPanelBase.Site == null) ? new TreeNode(uiPanelBase.Name, IconPanel, IconPanel) : new TreeNode(uiPanelBase.Site.Name, IconPanel, IconPanel));
			treeNode.Tag = uiPanelBase;
			return treeNode;
		}

		private TreeNode CreateContextMenuNode(MxContextMenu uiContextMenu)
		{
			TreeNode treeNode = new TreeNode(RenderingHelper.GetCommandNodeText(uiContextMenu), IconContextMenu, IconContextMenu);
			treeNode.Nodes.Add(CreateChildNode(uiContextMenu.Commands));
			return treeNode;
		}

		private TreeNode CreateCommandNode(MxCommand uiCommand)
		{
			TreeNode treeNode = new TreeNode(RenderingHelper.GetCommandNodeText(uiCommand), IconCommand, IconCommand);
			treeNode.Nodes.Add(CreateChildNode(uiCommand.Commands));
			return treeNode;
		}

		private TreeNode CreateFormatStyleNode(FormatStyle uiFormatStyle, string text)
		{
			return new TreeNode(text, IconFormatStyle, IconFormatStyle);
		}

		private TreeNode CreatePopupSettingsNode(PopupSettings uiPopupSettings, string text)
		{
			TreeNode treeNode = new TreeNode(text, IconPopupSettings, IconPopupSettings);
			treeNode.Nodes.Add(CreateNode(uiPopupSettings.ImageAreaFormatStyle, "ImageAreaFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPopupSettings.InfrequentItemsFormatStyle, "InfrequentItemsFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPopupSettings.SidebarFormatStyle, "SideBarFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPopupSettings.StateStyles, "StateStyles"));
			return treeNode;
		}

		private TreeNode CreateCommandManagerNode(MxCommandManager uiCommandManager)
		{
			try
			{
				TreeNode treeNode = new TreeNode(RenderingHelper.GetCommandNodeText(_commandManager), IconCommandManager, IconCommandManager);
				treeNode.Nodes.Add(CreateChildNode(uiCommandManager.CommandBars));
				treeNode.Nodes.Add(CreateChildNode(uiCommandManager.Commands));
				treeNode.Nodes.Add(CreateChildNode(uiCommandManager.ContextMenus));
				treeNode.Nodes.Add(CreateCommandBuiltInTextsNode(uiCommandManager.BuiltInTexts));
				treeNode.Nodes.Add(CreateChildNode(uiCommandManager.Categories));
				treeNode.Nodes.Add(CreateCommandPredefinedStylesNode(uiCommandManager));
				treeNode.Tag = uiCommandManager;
				treeNode.Expand();
				treeNode.Expand();
				return treeNode;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return null;
			}
		}

		private TreeNode CreatePanelManagerNode(MxDockManager uiPanelManager)
		{
			try
			{
				TreeNode treeNode = new TreeNode("Panel Manager", IconPanelManager, IconPanelManager);
				treeNode.Nodes.Add(CreatePanelsNode(_panelManager.Panels));
				TreeNode defaultSettingsNode = new TreeNode("DefaultPanelSettings", IconPanelManager, IconPanelManager);
				defaultSettingsNode.Tag = _panelManager.DefaultPanelSettings;
				treeNode.Nodes.Add(defaultSettingsNode);
				treeNode.Nodes.Add(CreatePanelBuiltInTextsNode(uiPanelManager.BuiltInTexts));
				treeNode.Nodes.Add(CreatePanelPredefinedStylesNode(uiPanelManager));
				treeNode.Tag = uiPanelManager;
				treeNode.Expand();
				treeNode.Expand();
				return treeNode;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return null;
			}
		}

		private TreeNode CreateCommandPredefinedStylesNode(MxCommandManager uiCommandManager)
		{
			TreeNode treeNode = new TreeNode("Predefined Styles", IconPredefinedStylesClosed, IconPredefinedStylesClosed);
			treeNode.Nodes.Add(CreateNode(uiCommandManager.CommandBarsFormatStyle, "CommandBars FormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiCommandManager.CommandsStateStyles, "Commands StateStyles"));
			treeNode.Nodes.Add(CreateNode(uiCommandManager.PopupSettings, "Popup Settings"));
			return treeNode;
		}

		private TreeNode CreatePanelPredefinedStylesNode(MxDockManager uiPanelManager)
		{
			TreeNode treeNode = new TreeNode("Predefined Styles", IconPredefinedStylesClosed, IconPredefinedStylesClosed);
			treeNode.Nodes.Add(CreateNode(uiPanelManager.DefaultPanelSettings.ActiveCaptionFormatStyle, "ActiveCaptionFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPanelManager.DefaultPanelSettings.CaptionFormatStyle, "CaptionFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPanelManager.DefaultPanelSettings.DarkCaptionFormatStyle, "DarkCaptionFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPanelManager.DefaultPanelSettings.LightCaptionFormatStyle, "LightCaptionFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPanelManager.DefaultPanelSettings.InnerContainerFormatStyle, "InnerContainerFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPanelManager.DefaultPanelSettings.InfoTextFormatStyle, "InfoTextFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPanelManager.TabStripFormatStyle, "TabStripFormatStyle"));
			treeNode.Nodes.Add(CreateNode(uiPanelManager.DefaultPanelSettings.TabStateStyles, "Tabs StateStyles"));
			treeNode.Nodes.Add(CreateNode(uiPanelManager.TabbedMdiSettings.TabStateStyles, "TabbedMdi StateStyles"));
			return treeNode;
		}

		private void InitializeControls()
		{
			_components = new Container();
			ResourceManager resourceManager = new ResourceManager(typeof(DockManagerEditorControl));
			_imageList = new ImageList(_components);
			_lblName = new Label();
			_bodyPanel = new Panel();
			_rightPanel = new Panel();
			_splitter = new Splitter();
			_objectTree = new TreeView();
			_layoutComboBox = new ComboBox();
			_layoutLabel = new Label();
			_bodyPanel.SuspendLayout();
			SuspendLayout();
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(ImageSizePx, ImageSizePx);
			ImageStrip.Load(_imageList, resourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_lblName.BackColor = SystemColors.ControlDark;
			_lblName.Dock = DockStyle.Top;
			_lblName.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_lblName.ForeColor = SystemColors.ControlLightLight;
			_lblName.Name = "lblName";
			_lblName.Size = new Size(672, 24);
			_lblName.TabIndex = 3;
			_lblName.Text = " CommandManager Designer";
			_lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_bodyPanel.Controls.AddRange(new Control[3] { _rightPanel, _splitter, _objectTree });
			_bodyPanel.Dock = DockStyle.Fill;
			_bodyPanel.DockPadding.All = 4;
			_bodyPanel.Location = new Point(0, 24);
			_bodyPanel.Name = "bodyPanel";
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
			_objectTree.Dock = DockStyle.Left;
			_objectTree.HideSelection = false;
			_objectTree.ImageList = _imageList;
			_objectTree.Location = new Point(4, 4);
			_objectTree.Name = "tree";
			_objectTree.Size = new Size(204, 384);
			_objectTree.TabIndex = 0;
			_objectTree.AfterExpand += OnTreeAfterExpand;
			_objectTree.AfterCollapse += OnTreeAfterCollapse;
			_objectTree.AfterSelect += OnTreeAfterSelect;
			_layoutComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_layoutComboBox.BackColor = SystemColors.Window;
			_layoutComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_layoutComboBox.ForeColor = SystemColors.WindowText;
			_layoutComboBox.Location = new Point(520, 4);
			_layoutComboBox.Name = "ComboBox1";
			_layoutComboBox.Size = new Size(148, 21);
			_layoutComboBox.TabIndex = 0;
			_layoutComboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			_layoutLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_layoutLabel.AutoSize = true;
			_layoutLabel.BackColor = SystemColors.ControlDark;
			_layoutLabel.ForeColor = SystemColors.ControlLightLight;
			_layoutLabel.Location = new Point(476, 6);
			_layoutLabel.Name = "layoutLabel";
			_layoutLabel.Size = new Size(42, 14);
			_layoutLabel.TabIndex = 0;
			_layoutLabel.Text = "Layout:";
			base.Controls.AddRange(new Control[4] { _layoutLabel, _layoutComboBox, _bodyPanel, _lblName });
			Mordex.Windows.Common.MxHeaderLayout.Attach(_lblName, _layoutLabel, _layoutComboBox);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "GeneralPage";
			base.Size = new Size(672, 416);
			_bodyPanel.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		private void OnTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (!_isSelectingNode)
			{
				if (_objectTree.SelectedNode != null)
				{
					SetSelectedValue(_objectTree.SelectedNode.Tag);
				}
				else
				{
					SetSelectedValue(null);
				}
			}
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
				SetActiveEditor(null);
			}
			else if (!typeof(ICollection).IsAssignableFrom(_selectedValue.GetType()))
			{
				if (_selectedValue is Mordex.Windows.UI.CommandBars.BuiltInTextList || _selectedValue is Mordex.Windows.UI.Docking.BuiltInTextList)
				{
					if (_commandManager != null)
					{
						SetActiveEditor(new BuiltInTextListEditorControl(_commandManager.BuiltInTexts));
					}
					else
					{
						SetActiveEditor(new BuiltInTextListEditorControl(_panelManager.BuiltInTexts));
					}
				}
				else
				{
					SetActiveEditor(new EditorControl());
				}
			}
			else if (_selectedValue is MxCommandCollection)
			{
				if (_objectTree.SelectedNode.Parent.Tag is MxCommandManager)
				{
					SetActiveEditor(new CommandManagerEditorControl(_commandManager));
				}
				else
				{
					CommandCollectionEditorControl uiCommandCollectionEditorControl = new CommandCollectionEditorControl(_commandManager);
					if (_objectTree.SelectedNode.Parent.Tag is MxCommand)
					{
						uiCommandCollectionEditorControl.Enabled = ((MxCommand)_objectTree.SelectedNode.Parent.Tag).CanModify();
					}
					else if (_objectTree.SelectedNode.Parent.Tag is MxCommandBar)
					{
						uiCommandCollectionEditorControl.Enabled = ((MxCommandBar)_objectTree.SelectedNode.Parent.Tag).IsEditableInDesigner();
					}
					else if (_objectTree.SelectedNode.Parent.Tag is MxContextMenu)
					{
						uiCommandCollectionEditorControl.Enabled = ((MxContextMenu)_objectTree.SelectedNode.Parent.Tag).IsNotInheritedReadOnly();
					}
					SetActiveEditor(uiCommandCollectionEditorControl);
				}
			}
			else if (_commandManager != null)
			{
				SetActiveEditor(new CommandManagerEditorControl(_commandManager));
			}
			else
			{
				SetActiveEditor(new CommandManagerEditorControl(_panelManager));
			}
			ShowActiveEditor();
		}

		private void SetActiveEditor(IUIElement editor)
		{
			if (_activeEditor == editor)
			{
				return;
			}
			if (_activeEditor != null)
			{
				if (!_activeEditor.CommitChanges())
				{
					return;
				}
				_activeEditor.SetSite(null);
				_activeEditor.DisableCancel -= OnActiveEditorDisableCancel;
				_activeEditor.SelectedObjectChanged -= OnActiveEditorSelectedObjectChanged;
				_activeEditor.RefreshTree -= OnActiveEditorRefreshTree;
				_activeEditor.DataChanged -= OnActiveEditorDataChanged;
				((Control)_activeEditor).Dispose();
			}
			_activeEditor = editor;
			if (_activeEditor != null)
			{
				((Control)_activeEditor).Dock = DockStyle.Fill;
				try
				{
					_activeEditor.ItemImage = _imageList.Images[_objectTree.SelectedNode.ImageIndex + 1];
				}
				catch
				{
				}
				_activeEditor.SelectedObject = _selectedValue;
				_activeEditor.DisableCancel += OnActiveEditorDisableCancel;
				_activeEditor.SelectedObjectChanged += OnActiveEditorSelectedObjectChanged;
				_activeEditor.RefreshTree += OnActiveEditorRefreshTree;
				_activeEditor.DataChanged += OnActiveEditorDataChanged;
				if (_commandManager != null)
				{
					_activeEditor.SetSite(_commandManager.Site);
				}
				else if (_panelManager != null)
				{
					_activeEditor.SetSite(_panelManager.Site);
				}
			}
			ShowActiveEditor();
		}

		private void ShowActiveEditor()
		{
			if (_activeEditor != null)
			{
				((Control)_activeEditor).Dock = DockStyle.Fill;
				_activeEditor.SelectedObject = _selectedValue;
				_rightPanel.Controls.Add((Control)_activeEditor);
			}
		}

		private void OnActiveEditorDataChanged(object sender, EventArgs eventArgs)
		{
			OnDataChanged(eventArgs);
		}

		private void OnActiveEditorSelectedObjectChanged(object sender, EventArgs eventArgs)
		{
			ShowActiveEditor();
		}

		private void OnActiveEditorDisableCancel(object sender, EventArgs eventArgs)
		{
			OnDisableCancel(eventArgs);
		}

		private void OnActiveEditorRefreshTree(object sender, EventArgs eventArgs)
		{
			OnDataChanged(EventArgs.Empty);
			_ = _objectTree.SelectedNode;
			object selectedValue = _selectedValue;
			if (selectedValue is DockPanelBase[] || selectedValue == null)
			{
				string text = _objectTree.SelectedNode.Text;
				object panelsParent = null;
				if (text == "Panels")
				{
					panelsParent = _objectTree.SelectedNode.Parent;
				}
				BuildTreeNodes();
				if (panelsParent == null)
				{
					TrySelectNodeByText(_objectTree.Nodes, text);
					return;
				}
				SelectNodeByTag(panelsParent);
				TrySelectNodeByText(_objectTree.SelectedNode.Nodes, text);
				return;
			}
			BuildTreeNodes();
			SelectNodeByTag(selectedValue);
			if (_objectTree.SelectedNode == null)
			{
				if (_commandManager != null)
				{
					SelectNodeByTag(_commandManager);
				}
				else
				{
					SelectNodeByTag(_panelManager);
				}
			}
		}

		private void OnComboBoxSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_commandManager != null)
			{
				if (_commandManager.CurrentLayout != (CommandBarsLayout)_layoutComboBox.SelectedValue && (_activeEditor == null || _activeEditor.CommitChanges()))
				{
					SetActiveEditor(null);
					if (_commandManager.CurrentLayout != null)
					{
						_commandManager.CurrentLayout.Update();
					}
					_commandManager.CurrentLayout = (CommandBarsLayout)_layoutComboBox.SelectedValue;
					RebuildTreeAndSelectRoot();
					OnDataChanged(EventArgs.Empty);
				}
			}
			else if (_panelManager.CurrentLayout != (DockPanelLayout)_layoutComboBox.SelectedValue && (_activeEditor == null || _activeEditor.CommitChanges()))
			{
				SetActiveEditor(null);
				if (_panelManager.CurrentLayout != null)
				{
					_panelManager.CurrentLayout.Update();
				}
				_panelManager.CurrentLayout = (DockPanelLayout)_layoutComboBox.SelectedValue;
				RebuildTreeAndSelectRoot();
				OnDataChanged(EventArgs.Empty);
			}
		}

		private void OnTreeAfterCollapse(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == IconPredefinedStylesOpen)
			{
				eventArgs.Node.ImageIndex = IconPredefinedStylesClosed;
				eventArgs.Node.SelectedImageIndex = IconPredefinedStylesClosed;
			}
		}

		private void OnTreeAfterExpand(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == IconPredefinedStylesClosed)
			{
				eventArgs.Node.ImageIndex = IconPredefinedStylesOpen;
				eventArgs.Node.SelectedImageIndex = IconPredefinedStylesOpen;
			}
		}
	}
}
