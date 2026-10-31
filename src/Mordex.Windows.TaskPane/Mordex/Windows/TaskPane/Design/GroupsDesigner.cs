using Mordex.Windows.Common;
using System;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Mordex.Windows.TaskPane.Design
{
	public class GroupsDesigner : Form
	{

		private TabControl _tabControl;

		private IContainer _iContainer;

		private Label _label;

		private Label _groupsCaptionLabel;

		private GroupBox _groupBox;

		private Label _hintLabel;

		private PropertyGrid _propertyGrid;

		private TreeView _treeView;

		private Button _btnOk;

		private MxToolBar _tbOperations;

		private MxToolBarButton _tbtnAdd;

		private MxToolBarButton _tbtnRemove;

		private MxToolBarButton _tbtnClear;

		private MxToolBarButton _tbtnSeparator;

		private MxToolBarButton _tbtnMoveUp;

		private MxToolBarButton _tbtnMoveDown;

		private ImageList _imageList;

		private Label _titleLabel;

		private ComboBox _comboBox;

		private Label _layoutLabel;

		private TabPage _tabGroups;

		private TabPage _tabLayouts;

		private Button _btnCancel;

		private MxTaskPane _taskPane;

		private TreeNode _rootNode;

		private ITaskPaneElement _elementEditor;

		private TabPage _tabPage;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public MxTaskPane TaskPane
		{
			get
			{
				return _taskPane;
			}
			set
			{
				if (_taskPane != value)
				{
					_taskPane = value;
					PopulateTree();
					PopulateLayoutCombo();
					_tabPage = _tabGroups;
					_ = _taskPane;
				}
			}
		}

		public GroupsDesigner()
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(GroupsDesigner));
			_tabControl = new TabControl();
			_tabGroups = new TabPage();
			_layoutLabel = new Label();
			_comboBox = new ComboBox();
			_tbOperations = new MxToolBar();
			_tbtnAdd = new MxToolBarButton();
			_tbtnRemove = new MxToolBarButton();
			_tbtnClear = new MxToolBarButton();
			_tbtnSeparator = new MxToolBarButton();
			_tbtnMoveUp = new MxToolBarButton();
			_tbtnMoveDown = new MxToolBarButton();
			_imageList = new ImageList(_iContainer);
			_titleLabel = new Label();
			_label = new Label();
			_groupsCaptionLabel = new Label();
			_groupBox = new GroupBox();
			_hintLabel = new Label();
			_treeView = new TreeView();
			_propertyGrid = new PropertyGrid();
			_tabLayouts = new TabPage();
			_btnOk = new Button();
			_btnCancel = new Button();
			_tabControl.SuspendLayout();
			_tabGroups.SuspendLayout();
			_groupBox.SuspendLayout();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_tabControl.Controls.Add(_tabGroups);
			_tabControl.Controls.Add(_tabLayouts);
			_tabControl.ImageList = _imageList;
			_tabControl.Location = new Point(0, 0);
			_tabControl.Name = "tabControl1";
			_tabControl.SelectedIndex = 0;
			_tabControl.Size = new Size(568, 360);
			_tabControl.TabIndex = 0;
			_tabControl.SelectedIndexChanged += OnTabControlSelectedIndexChanged;
			_tabGroups.Controls.Add(_layoutLabel);
			_tabGroups.Controls.Add(_comboBox);
			_tabGroups.Controls.Add(_tbOperations);
			_tabGroups.Controls.Add(_titleLabel);
			_tabGroups.Controls.Add(_label);
			_tabGroups.Controls.Add(_groupsCaptionLabel);
			_tabGroups.Controls.Add(_groupBox);
			_tabGroups.Controls.Add(_treeView);
			_tabGroups.Controls.Add(_propertyGrid);
			_tabGroups.ImageIndex = 9;
			_tabGroups.Location = new Point(4, 23);
			_tabGroups.Name = "tabGroups";
			_tabGroups.Size = new Size(560, 333);
			_tabGroups.TabIndex = 0;
			_tabGroups.Text = "General Properties";
			_layoutLabel.AutoSize = true;
			_layoutLabel.BackColor = SystemColors.ControlDark;
			_layoutLabel.ForeColor = Color.White;
			_layoutLabel.Location = new Point(372, 3);
			_layoutLabel.Name = "label5";
			_layoutLabel.Size = new Size(40, 13);
			_layoutLabel.TabIndex = 26;
			_layoutLabel.Text = "Layout:";
			_comboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_comboBox.FormattingEnabled = true;
			_comboBox.Location = new Point(416, 0);
			_comboBox.Name = "comboBox1";
			_comboBox.Size = new Size(136, 21);
			_comboBox.TabIndex = 25;
			_comboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			_tbOperations.Appearance = MxToolBarAppearance.Flat;
			_tbOperations.Buttons.AddRange(new MxToolBarButton[6] { _tbtnAdd, _tbtnRemove, _tbtnClear, _tbtnSeparator, _tbtnMoveUp, _tbtnMoveDown });
			_tbOperations.Divider = false;
			_tbOperations.DropDownArrows = true;
			_tbOperations.ImageList = _imageList;
			_tbOperations.Location = new Point(0, 24);
			_tbOperations.Name = "tbOperations";
			_tbOperations.ShowToolTips = true;
			_tbOperations.Size = new Size(560, 26);
			_tbOperations.TabIndex = 23;
			_tbOperations.TextAlign = MxToolBarTextAlign.Right;
			_tbOperations.ButtonClick += OnTbOperationsButtonClick;
			_tbtnAdd.ImageIndex = 0;
			_tbtnAdd.Name = "tbtnAdd";
			_tbtnAdd.Tag = "Add";
			_tbtnAdd.Text = "Add";
			_tbtnRemove.ImageIndex = 1;
			_tbtnRemove.Name = "tbtnRemove";
			_tbtnRemove.Tag = "Remove";
			_tbtnRemove.Text = "Remove";
			_tbtnClear.ImageIndex = 2;
			_tbtnClear.Name = "tbtnClear";
			_tbtnClear.Tag = "Clear";
			_tbtnClear.Text = "Clear";
			_tbtnSeparator.Name = "toolSep1";
			_tbtnSeparator.Style = MxToolBarButtonStyle.Separator;
			_tbtnMoveUp.ImageIndex = 3;
			_tbtnMoveUp.Name = "tbtnMoveUp";
			_tbtnMoveUp.Tag = "Up";
			_tbtnMoveDown.ImageIndex = 4;
			_tbtnMoveDown.Name = "tbtnMoveDown";
			_tbtnMoveDown.Tag = "Down";
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			_imageList.Images.SetKeyName(2, "");
			_imageList.Images.SetKeyName(3, "");
			_imageList.Images.SetKeyName(4, "");
			_imageList.Images.SetKeyName(5, "");
			_imageList.Images.SetKeyName(6, "");
			_imageList.Images.SetKeyName(7, "");
			_imageList.Images.SetKeyName(8, "");
			_imageList.Images.SetKeyName(9, "");
			_imageList.Images.SetKeyName(10, "");
			_imageList.Images.SetKeyName(11, "");
			_imageList.Images.SetKeyName(12, "");
			_imageList.Images.SetKeyName(13, "");
			_imageList.Images.SetKeyName(14, "");
			_imageList.Images.SetKeyName(15, "");
			_imageList.Images.SetKeyName(16, "FolderFormatStyles.ico");
			_imageList.Images.SetKeyName(17, "FolderCerrado.ico");
			_imageList.Images.SetKeyName(18, "FolderAbierto.ico");
			_titleLabel.BackColor = SystemColors.ControlDark;
			_titleLabel.Dock = DockStyle.Top;
			_titleLabel.Font = new Font("Tahoma", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_titleLabel.ForeColor = Color.White;
			_titleLabel.Location = new Point(0, 0);
			_titleLabel.Name = "label4";
			_titleLabel.Size = new Size(560, 24);
			_titleLabel.TabIndex = 24;
			_titleLabel.Text = "TaskPane Designer";
			_titleLabel.TextAlign = ContentAlignment.MiddleLeft;
			_label.AutoSize = true;
			_label.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_label.Location = new Point(272, 56);
			_label.Name = "label1";
			_label.Size = new Size(65, 13);
			_label.TabIndex = 15;
			_label.Text = "Properties:";
			_groupsCaptionLabel.AutoSize = true;
			_groupsCaptionLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_groupsCaptionLabel.Location = new Point(8, 56);
			_groupsCaptionLabel.Name = "label2";
			_groupsCaptionLabel.Size = new Size(46, 13);
			_groupsCaptionLabel.TabIndex = 16;
			_groupsCaptionLabel.Text = "Groups:";
			_groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_groupBox.Controls.Add(_hintLabel);
			_groupBox.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_groupBox.Location = new Point(272, 64);
			_groupBox.Name = "groupBox1";
			_groupBox.Size = new Size(280, 263);
			_groupBox.TabIndex = 22;
			_groupBox.TabStop = false;
			_groupBox.Visible = false;
			_hintLabel.Dock = DockStyle.Fill;
			_hintLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_hintLabel.Location = new Point(3, 17);
			_hintLabel.Name = "label3";
			_hintLabel.Size = new Size(274, 243);
			_hintLabel.TabIndex = 0;
			_treeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			_treeView.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_treeView.HideSelection = false;
			_treeView.ImageIndex = 0;
			_treeView.ImageList = _imageList;
			_treeView.Location = new Point(8, 72);
			_treeView.Name = "treeView1";
			_treeView.SelectedImageIndex = 0;
			_treeView.Size = new Size(256, 255);
			_treeView.TabIndex = 13;
			_treeView.AfterExpand += OnTreeViewAfterExpand;
			_treeView.AfterSelect += OnTreeViewAfterSelect;
			_treeView.AfterCollapse += OnTreeViewAfterCollapse;
			_propertyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_propertyGrid.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_propertyGrid.LineColor = SystemColors.Control;
			_propertyGrid.Location = new Point(272, 72);
			_propertyGrid.Name = "propertyGrid1";
			_propertyGrid.Size = new Size(272, 255);
			_propertyGrid.TabIndex = 14;
			_propertyGrid.ToolbarVisible = false;
			_propertyGrid.PropertyValueChanged += OnPropertyGridPropertyValueChanged;
			_tabLayouts.ImageIndex = 10;
			_tabLayouts.Location = new Point(4, 23);
			_tabLayouts.Name = "tabLayouts";
			_tabLayouts.Size = new Size(560, 333);
			_tabLayouts.TabIndex = 1;
			_tabLayouts.Text = "Layout Manager";
			_btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOk.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_btnOk.Location = new Point(400, 376);
			_btnOk.Name = "btnOk";
			_btnOk.Size = new Size(72, 24);
			_btnOk.TabIndex = 11;
			_btnOk.Text = "OK";
			_btnOk.Click += OnBtnOkClick;
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_btnCancel.Location = new Point(488, 376);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(72, 24);
			_btnCancel.TabIndex = 11;
			_btnCancel.Text = "Cancel";
			_btnCancel.Click += OnBtnCancelClick;
			base.ClientSize = new Size(568, 414);
			base.Controls.Add(_btnOk);
			base.Controls.Add(_tabControl);
			base.Controls.Add(_btnCancel);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "GroupsDesigner";
			Text = "Groups Designer";
			base.Load += OnControlLoad;
			base.FormClosing += OnControlClosing;
			_tabControl.ResumeLayout(performLayout: false);
			_tabGroups.ResumeLayout(performLayout: false);
			Mordex.Windows.Common.MxHeaderLayout.Attach(_titleLabel, _layoutLabel, _comboBox);
			_tabGroups.PerformLayout();
			_groupBox.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		private TaskPaneGroupCollection CloneGroups()
		{
			TaskPaneGroupCollection taskPaneGroupCollection = new TaskPaneGroupCollection(_taskPane);
			foreach (TaskPaneGroup group in _taskPane.Groups)
			{
				TaskPaneGroup clonedGroup = group.CloneDetached();
				foreach (TaskPaneItem sourceItem in group.Items)
				{
					TaskPaneItem item = sourceItem.CloneDetached();
					clonedGroup.Items.Add(item);
				}
				taskPaneGroupCollection.Add(clonedGroup);
			}
			return taskPaneGroupCollection;
		}

		private void PopulateTree()
		{
			try
			{
				TaskPaneGroupCollection taskPaneGroupCollection = CloneGroups();
				_treeView.Nodes.Clear();
				TreeNode treeNode = new TreeNode("TaskPane", 15, 15);
				treeNode.Tag = _taskPane;
				_treeView.Nodes.Add(treeNode);
				_rootNode = new TreeNode("Groups", 11, 11);
				_rootNode.Tag = taskPaneGroupCollection;
				treeNode.Nodes.Add(_rootNode);
				foreach (TaskPaneGroup item in taskPaneGroupCollection)
				{
					TreeNode groupNode = new TreeNode("[" + item._index + "] " + item.Key, 12, 12);
					groupNode.Tag = item;
					_rootNode.Nodes.Add(groupNode);
					TreeNode itemsNode = new TreeNode("Items", 13, 13);
					itemsNode.Tag = item.Items;
					groupNode.Nodes.Add(itemsNode);
					foreach (TaskPaneItem groupItem in item.Items)
					{
						TreeNode itemNode = new TreeNode("[" + groupItem._index + "] " + groupItem.Key, 14, 14);
						itemNode.Tag = groupItem;
						itemsNode.Nodes.Add(itemNode);
					}
				}
				TreeNode stylesNode = new TreeNode("Predefined Styles", 17, 17);
				stylesNode.Nodes.Add(CreateFormatStyleNode(_taskPane.BackgroundFormatStyle, "Background FormatStyle"));
				stylesNode.Nodes.Add(CreateStateStylesNode(_taskPane.GroupsStateStyles, "Groups StateStyles"));
				stylesNode.Nodes.Add(CreateStateStylesNode(_taskPane.SpecialGroupsStateStyles, "Special Groups StateStyles"));
				stylesNode.Nodes.Add(CreateStateStylesNode(_taskPane.ItemsStateStyles, "Items StateStyles"));
				treeNode.Nodes.Add(stylesNode);
				treeNode.Expand();
				_rootNode.Expand();
				stylesNode.Expand();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private TreeNode CreateFormatStyleNode(TaskPaneFormatStyle taskPaneFormatStyle, string nodeText)
		{
			TreeNode treeNode = new TreeNode(nodeText, 8, 8);
			treeNode.Tag = taskPaneFormatStyle;
			return treeNode;
		}

		private TreeNode CreateStateStylesNode(TaskPaneStateStyles taskPaneStateStyles, string nodeText)
		{
			TreeNode treeNode = new TreeNode(nodeText, 16, 16);
			treeNode.Nodes.Add(CreateFormatStyleNode(taskPaneStateStyles.DisabledFormatStyle, "DisabledFormatStyle"));
			treeNode.Nodes.Add(CreateFormatStyleNode(taskPaneStateStyles.FormatStyle, "FormatStyle"));
			treeNode.Nodes.Add(CreateFormatStyleNode(taskPaneStateStyles.HotFormatStyle, "HotFormatStyle"));
			treeNode.Nodes.Add(CreateFormatStyleNode(taskPaneStateStyles.PressedFormatStyle, "PressedFormatStyle"));
			treeNode.Nodes.Add(CreateFormatStyleNode(taskPaneStateStyles.SelectedFormatStyle, "SelectedFormatStyle"));
			return treeNode;
		}

		private void OnTreeViewAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.Tag == null)
			{
				_tbtnAdd.Enabled = false;
				_tbtnClear.Enabled = false;
				_tbtnRemove.Enabled = false;
				_propertyGrid.Visible = false;
				_groupBox.Visible = false;
				return;
			}
			Type type = eventArgs.Node.Tag.GetType();
			if (type == typeof(MxTaskPane) || type == typeof(TaskPaneFormatStyle))
			{
				_tbtnAdd.Enabled = false;
				_tbtnClear.Enabled = false;
				_tbtnRemove.Enabled = false;
				_propertyGrid.Visible = true;
				_groupBox.Visible = false;
				_propertyGrid.SelectedObject = eventArgs.Node.Tag;
			}
			else
			{
				_tbtnAdd.Enabled = true;
				if (type == typeof(TaskPaneItemCollection) || type == typeof(TaskPaneGroupCollection))
				{
					_tbtnClear.Enabled = true;
					if (type == typeof(TaskPaneGroupCollection))
					{
						_tbtnAdd.Text = "Add Group";
						_hintLabel.Text = "Click the 'AddGroup' button to add a new group to this control";
					}
					else
					{
						_hintLabel.Text = "Click the 'AddItem' button to add a new item to this group";
						_tbtnAdd.Text = "Add Item";
					}
					_tbtnRemove.Enabled = false;
					_propertyGrid.Visible = false;
					_groupBox.Visible = true;
				}
				else
				{
					_tbtnClear.Enabled = false;
					_propertyGrid.Visible = true;
					_groupBox.Visible = false;
					_hintLabel.Text = "";
					_tbtnRemove.Enabled = true;
					if (type == typeof(TaskPaneGroup))
					{
						_tbtnAdd.Text = "Add Group";
					}
					else
					{
						_tbtnAdd.Text = "Add Item";
					}
					_propertyGrid.SelectedObject = eventArgs.Node.Tag;
				}
			}
			if (_treeView.SelectedNode.NextNode == null)
			{
				_tbtnMoveDown.Enabled = false;
			}
			else
			{
				_tbtnMoveDown.Enabled = true;
			}
			if (_treeView.SelectedNode.PrevNode == null)
			{
				_tbtnMoveUp.Enabled = false;
			}
			else
			{
				_tbtnMoveUp.Enabled = true;
			}
		}

		private void AddItemsNode(TreeNode parentNode, TaskPaneGroup taskPaneGroup)
		{
			TreeNode treeNode = new TreeNode("Items", 13, 13);
			treeNode.Tag = taskPaneGroup.Items;
			parentNode.Nodes.Add(treeNode);
		}

		private string GenerateUniqueKey(Type componentType, string prefix, int number, ArrayList arrayList)
		{
			string key = "";
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(componentType).Find("Key", ignoreCase: true);
			if (propertyDescriptor != null)
			{
				key = prefix + number;
				foreach (object item in arrayList)
				{
					if (string.Compare((string)propertyDescriptor.GetValue(item), key, ignoreCase: true) == 0)
					{
						return GenerateUniqueKey(componentType, prefix, number + 1, arrayList);
					}
				}
			}
			return key;
		}

		private void AddGroupOrItem()
		{
			if (TaskPane == null || _treeView.SelectedNode == null)
			{
				return;
			}
			if (_treeView.SelectedNode.Tag is TaskPaneGroup || _treeView.SelectedNode.Tag is TaskPaneGroupCollection)
			{
				TreeNode treeNode = null;
				_treeView.SelectedNode.Tag.GetType();
				TaskPaneGroup taskPaneGroup = new TaskPaneGroup("New Group");
				ArrayList arrayList = new ArrayList(_rootNode.Nodes.Count);
				foreach (TreeNode node in _rootNode.Nodes)
				{
					arrayList.Add(node.Tag);
				}
				taskPaneGroup.Key = GenerateUniqueKey(typeof(TaskPaneGroup), "Group", 1, arrayList);
				treeNode = new TreeNode("[" + _rootNode.Nodes.Count + "] " + taskPaneGroup.Key, 12, 12);
				treeNode.Tag = taskPaneGroup;
				taskPaneGroup._owner = (TaskPaneGroupCollection)_rootNode.Tag;
				_rootNode.Nodes.Add(treeNode);
				_treeView.SelectedNode = treeNode;
				AddItemsNode(treeNode, taskPaneGroup);
			}
			else
			{
				AddItem();
			}
		}

		private void RemoveSelectedNode()
		{
			if (_taskPane == null || _treeView.SelectedNode == null)
			{
				return;
			}
			if (_treeView.SelectedNode.Tag is TaskPaneGroup)
			{
				TreeNode selectedNode = _treeView.SelectedNode;
				if (_treeView.SelectedNode.NextNode == null)
				{
					_treeView.SelectedNode = _treeView.SelectedNode.PrevNode;
				}
				else
				{
					_treeView.SelectedNode = _treeView.SelectedNode.NextNode;
				}
				selectedNode.Remove();
			}
			else if (_treeView.SelectedNode.Tag is TaskPaneItem)
			{
				TreeNode selectedItemNode = _treeView.SelectedNode;
				if (_treeView.SelectedNode.NextNode == null)
				{
					_treeView.SelectedNode = _treeView.SelectedNode.PrevNode;
				}
				else
				{
					_treeView.SelectedNode = _treeView.SelectedNode.NextNode;
				}
				selectedItemNode.Remove();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Point lineStart = new Point(base.ClientRectangle.Left + 10, base.ClientRectangle.Bottom - 45);
			Point lineEnd = new Point(base.ClientRectangle.Right - 10, base.ClientRectangle.Bottom - 45);
			e.Graphics.DrawLine(SystemPens.ControlDark, lineStart, lineEnd);
			lineStart.Offset(0, 1);
			lineEnd.Offset(0, 1);
			e.Graphics.DrawLine(SystemPens.ControlLightLight, lineStart, lineEnd);
		}

		private void OnBtnOkClick(object sender, EventArgs eventArgs)
		{
			CommitTreeToTaskPane();
			base.DialogResult = DialogResult.OK;
			Close();
		}

		private void CommitTreeToTaskPane()
		{
			TaskPaneGroup[] newGroups = new TaskPaneGroup[_rootNode.Nodes.Count];
			_treeView.SelectedNode = _rootNode.FirstNode;
			for (int i = 0; i < _rootNode.Nodes.Count; i++)
			{
				TaskPaneGroup taskPaneGroup = (TaskPaneGroup)_rootNode.Nodes[i].Tag;
				taskPaneGroup._owner = null;
				TreeNode treeNode = _rootNode.Nodes[i].Nodes[0];
				ReadTreeNodeIntoGroup(treeNode, taskPaneGroup);
				newGroups[i] = taskPaneGroup;
			}
			ArrayList arrayList = new ArrayList();
			foreach (TaskPaneGroup group in _taskPane.Groups)
			{
				if (group.ContainerControl != null)
				{
					arrayList.Add(group.ContainerControl);
				}
			}
			_taskPane.Groups.ClearKeepingContainers();
			_taskPane.Groups.AddRange(newGroups);
			bool isContainerOrphaned = true;
			foreach (TaskPaneContainerControl item in arrayList)
			{
				isContainerOrphaned = true;
				TaskPaneGroup[] candidateGroups = newGroups;
				foreach (TaskPaneGroup candidateGroup in candidateGroups)
				{
					if (candidateGroup.ContainerControl != null && candidateGroup.ContainerControl.Equals(item))
					{
						candidateGroup.ContainerControl._group = candidateGroup;
						isContainerOrphaned = false;
						break;
					}
				}
				if (isContainerOrphaned)
				{
					item.Dispose();
				}
			}
		}

		private void ReadTreeNodeIntoGroup(TreeNode treeNode, TaskPaneGroup taskPaneGroup)
		{
			if (treeNode.Tag.GetType() == typeof(TaskPaneItemCollection))
			{
				TaskPaneItem[] newItems = new TaskPaneItem[treeNode.Nodes.Count];
				for (int i = 0; i < treeNode.Nodes.Count; i++)
				{
					TaskPaneItem taskPaneItem = (TaskPaneItem)treeNode.Nodes[i].Tag;
					taskPaneItem._owner = null;
					newItems[i] = taskPaneItem;
				}
				taskPaneGroup.Items.Clear();
				taskPaneGroup.Items.AddRange(newItems);
			}
		}

		private void RemoveNewItems(TaskPaneGroup taskPaneGroup)
		{
			foreach (TaskPaneItem item in taskPaneGroup.Items.Cast<TaskPaneItem>().ToArray())
			{
				if (item._owner == null)
				{
					taskPaneGroup.Items.Remove(item);
				}
			}
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			foreach (TreeNode node in _rootNode.Nodes)
			{
				TaskPaneGroup taskPaneGroup = (TaskPaneGroup)node.Tag;
				if (taskPaneGroup._owner == null)
				{
					_taskPane.Groups.Remove(taskPaneGroup);
				}
				RemoveNewItems(taskPaneGroup);
			}
			Close();
		}

		private void MoveSelectedNodeUp()
		{
			if (_treeView.SelectedNode.PrevNode != null)
			{
				TreeNode parent = _treeView.SelectedNode.Parent;
				TreeNode selectedNode = _treeView.SelectedNode;
				int index = _treeView.SelectedNode.Index - 1;
				parent.Nodes.Remove(selectedNode);
				parent.Nodes.Insert(index, selectedNode);
				_treeView.SelectedNode = selectedNode;
			}
		}

		private void MoveSelectedNodeDown()
		{
			if (_treeView.SelectedNode.NextNode != null)
			{
				TreeNode parent = _treeView.SelectedNode.Parent;
				TreeNode selectedNode = _treeView.SelectedNode;
				int index = _treeView.SelectedNode.Index + 1;
				parent.Nodes.Remove(selectedNode);
				parent.Nodes.Insert(index, selectedNode);
				_treeView.SelectedNode = selectedNode;
			}
		}

		private void AddItem()
		{
			if (TaskPane == null || _treeView.SelectedNode == null)
			{
				return;
			}
			TreeNode treeNode = null;
			Type type = _treeView.SelectedNode.Tag.GetType();
			TaskPaneItem taskPaneItem = new TaskPaneItem("New Item");
			TreeNode itemsNode = null;
			if (type == typeof(TaskPaneGroup))
			{
				itemsNode = _treeView.SelectedNode.Nodes[0];
			}
			if (type == typeof(TaskPaneItemCollection))
			{
				itemsNode = _treeView.SelectedNode;
			}
			else if (type == typeof(TaskPaneItem))
			{
				itemsNode = _treeView.SelectedNode.Parent;
			}
			ArrayList arrayList = new ArrayList(itemsNode.Nodes.Count);
			foreach (TreeNode node in itemsNode.Nodes)
			{
				arrayList.Add(node.Tag);
			}
			taskPaneItem._owner = (TaskPaneItemCollection)itemsNode.Tag;
			taskPaneItem.Key = GenerateUniqueKey(typeof(TaskPaneItem), "Item", 1, arrayList);
			treeNode = new TreeNode("[" + itemsNode.Nodes.Count + "] " + taskPaneItem.Key, 14, 14);
			treeNode.Tag = taskPaneItem;
			itemsNode.Nodes.Add(treeNode);
			_treeView.SelectedNode = treeNode;
		}

		private void OnPropertyGridPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			if (eventArgs.ChangedItem.PropertyDescriptor.Name == "Key")
			{
				_treeView.SelectedNode.Text = "[" + _treeView.SelectedNode.Index + "] " + eventArgs.ChangedItem.Value.ToString();
			}
		}

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\TaskPane control");
				Rectangle empty = Rectangle.Empty;
				empty.Width = LogicalToDeviceUnits((int)registryKey.GetValue("Width", 538));
				empty.Height = LogicalToDeviceUnits((int)registryKey.GetValue("Height", 352));
				empty.Y = (int)registryKey.GetValue("Top", 0);
				empty.X = (int)registryKey.GetValue("Left", 0);
				Rectangle virtualScreen = SystemInformation.VirtualScreen;
				if (empty.Right > virtualScreen.Right)
				{
					empty.X = empty.Right - empty.Width;
				}
				if (empty.X < virtualScreen.Left)
				{
					empty.X = SystemInformation.VirtualScreen.Left;
				}
				if (empty.Y + empty.Height > virtualScreen.Bottom)
				{
					if (empty.Y > virtualScreen.Bottom - empty.Bottom)
					{
						empty.Y -= empty.Height;
						if (empty.Y < SystemInformation.VirtualScreen.Top)
						{
							empty.Y = SystemInformation.VirtualScreen.Top;
						}
					}
					else
					{
						empty.Height = virtualScreen.Bottom - empty.Bottom;
					}
				}
				base.Bounds = empty;
				string windowStateText = registryKey.GetValue("Window State", "Normal") as string;
				if (windowStateText == "Normal")
				{
					base.WindowState = FormWindowState.Normal;
				}
				else if (windowStateText == "Maximized")
				{
					base.WindowState = FormWindowState.Maximized;
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
			_propertyGrid.CommandsVisibleIfAvailable = false;
			_propertyGrid.PropertySort = PropertySort.Alphabetical;
			_propertyGrid.ToolbarVisible = true;
			_treeView.SelectedNode = _treeView.Nodes[0];
			_treeView.Nodes[0].Expand();
		}

		private void OnControlClosing(object sender, CancelEventArgs eventArgs)
		{
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\TaskPane control");
				_ = Rectangle.Empty;
				registryKey.SetValue("Width", (int)(base.Width * 96.0 / DeviceDpi + 0.5));
				registryKey.SetValue("Height", (int)(base.Height * 96.0 / DeviceDpi + 0.5));
				registryKey.SetValue("Top", base.Top);
				registryKey.SetValue("Left", base.Left);
				registryKey.SetValue("Window State", base.WindowState);
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
		}

		private void OnTbOperationsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			switch (_tbOperations.Buttons.IndexOf(eventArgs.Button))
			{
			case 0:
				AddGroupOrItem();
				break;
			case 1:
				RemoveSelectedNode();
				break;
			case 2:
				_treeView.SelectedNode.Nodes.Clear();
				break;
			case 4:
				MoveSelectedNodeUp();
				break;
			case 5:
				MoveSelectedNodeDown();
				break;
			case 3:
				break;
			}
		}

		private void OnTabControlSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_tabPage != _tabLayouts)
			{
				CommitTreeToTaskPane();
			}
			ShowTabPage(_tabControl.SelectedTab);
		}

		private ITaskPaneElement GetElementEditor()
		{
			return _elementEditor;
		}

		private void SetElementEditor(ITaskPaneElement iTaskPaneElement)
		{
			if (_elementEditor != iTaskPaneElement)
			{
				if (_elementEditor != null)
				{
					_elementEditor.SelectTab -= OnStateSelectTab;
					((Control)_elementEditor).Dispose();
				}
				_elementEditor = iTaskPaneElement;
				if (_elementEditor != null)
				{
					((Control)_elementEditor).Dock = DockStyle.Fill;
					_elementEditor.TaskPane = _taskPane;
					_elementEditor.SelectTab += OnStateSelectTab;
					_tabControl.SelectedTab.Controls.Add((Control)_elementEditor);
				}
			}
		}

		private void OnComboBoxSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_tabPage == _tabLayouts)
			{
				return;
			}
			CommitTreeToTaskPane();
			if (_taskPane.CurrentLayout != (TaskPaneLayout)_comboBox.SelectedValue && (_elementEditor == null || GetElementEditor().CommitChanges()))
			{
				SetElementEditor(null);
				if (_taskPane.CurrentLayout != null)
				{
					_taskPane.CurrentLayout.Update();
				}
				_taskPane.CurrentLayout = (TaskPaneLayout)_comboBox.SelectedValue;
				ReloadTree();
			}
		}

		private void PopulateLayoutCombo()
		{
			_comboBox.SelectedValueChanged -= OnComboBoxSelectedValueChanged;
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new ON(null, "(none)"));
			AddLayoutEntries(arrayList, _taskPane.Layouts, 0);
			_comboBox.ValueMember = "Object";
			_comboBox.DisplayMember = "Name";
			_comboBox.DataSource = arrayList;
			_comboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			if (_taskPane.CurrentLayout != null)
			{
				_comboBox.SelectedValue = _taskPane.CurrentLayout;
			}
			else
			{
				_comboBox.SelectedIndex = 0;
			}
		}

		private void AddLayoutEntries(ArrayList arrayList, TaskPaneLayoutCollection taskPaneLayoutCollection, int depth)
		{
			foreach (TaskPaneLayout item in taskPaneLayoutCollection)
			{
				arrayList.Add(new ON(item, new string(' ', depth * 3) + FormatLayoutText(taskPaneLayoutCollection.IndexOf(item), item)));
				AddLayoutEntries(arrayList, item.ChildLayouts, depth + 1);
			}
		}

		private string FormatLayoutText(int layoutIndex, TaskPaneLayout taskPaneLayout)
		{
			return "[" + layoutIndex + "] - '" + taskPaneLayout.Key + "'";
		}

		private void ReloadTree()
		{
			PopulateTree();
			_treeView.SelectedNode = _treeView.Nodes[0];
		}

		private void OnStateSelectTab(object sender, EventArgs eventArgs)
		{
			_tabControl.SelectedTab = _tabGroups;
		}

		private void ShowTabPage(TabPage tabPage)
		{
			if (_tabPage == tabPage)
			{
				return;
			}
			if (_elementEditor != null && !_elementEditor.CommitChanges())
			{
				_tabControl.SelectedTab = _tabPage;
				return;
			}
			_tabPage = tabPage;
			string name;
			if ((name = _tabPage.Name) != null)
			{
				if (name == "tabLayouts")
				{
					SetElementEditor(new TaskPaneEditorControl());
					return;
				}
				if (name == "tabGroups")
				{
					PopulateTree();
					PopulateLayoutCombo();
					return;
				}
			}
			SetElementEditor(null);
		}

		private void OnTreeViewAfterExpand(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == 17)
			{
				eventArgs.Node.ImageIndex = 18;
				eventArgs.Node.SelectedImageIndex = 18;
			}
		}

		private void OnTreeViewAfterCollapse(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == 18)
			{
				eventArgs.Node.ImageIndex = 17;
				eventArgs.Node.SelectedImageIndex = 17;
			}
		}
	}
}
