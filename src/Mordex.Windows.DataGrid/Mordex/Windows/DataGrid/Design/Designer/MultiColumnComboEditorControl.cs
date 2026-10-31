using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	[ToolboxItem(false)]
	internal class MultiColumnComboEditorControl : UserControl, IDesignerPage
	{
		private IContainer _iContainer;

		private Label _layoutLabel;

		private MxMultiColumnComboBox _layoutCombo;

		private EventHandler _dataChangedHandler;

		private ImageList _imageList;

		private Label _lblName;

		private Panel _bodyPanel;

		private Panel _rightPanel;

		private Splitter _treeSplitter;

		private TreeView _tree;

		private MxDataGrid _dataGrid;

		private object _selectedValue;

		private IGridElement _iGridElement;

		private static object _multiColumnComboEditorControlValue = new object();

		MxDataGrid IDesignerPage.DataGrid
		{
			get
			{
				return _dataGrid;
			}
			set
			{
				if (_dataGrid != value)
				{
					_dataGrid = value;
					UpdateHeaderForGrid();
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
				base.Events.AddHandler(_multiColumnComboEditorControlValue, value);
			}
			remove
			{
				base.Events.RemoveHandler(_multiColumnComboEditorControlValue, value);
			}
		}

		internal MultiColumnComboEditorControl()
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

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		void IDesignerPage.Unload()
		{
		}

		bool IDesignerPage.CommitChanges()
		{
			if (_iGridElement != null)
			{
				return _iGridElement.CommitChanges();
			}
			return true;
		}

		private void UpdateMultiColumnComboEditorControl()
		{
			FillLayoutCombo();
		}

		private void FillLayoutCombo()
		{
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new GridValueListItem(null, "(none)", -1));
			ProcessArrayList(arrayList, _dataGrid.Layouts, 0);
			_layoutCombo.ValueMember = "Value";
			_layoutCombo.DisplayMember = "Text";
			_layoutCombo.DropDownList.Columns.Add("Text");
			_layoutCombo.DropDownList.GridLines = GridLines.None;
			_layoutCombo.DropDownList.SetDataBinding(arrayList, "");
			_layoutCombo.Value = _dataGrid.CurrentLayout;
			_layoutCombo.DropDownList.ColumnAutoResize = true;
			_layoutCombo.DropDownList.ColumnHeaders = InheritableBoolean.False;
		}

		private void ProcessArrayList(ArrayList arrayList, GridLayoutCollection gridLayoutCollection, int value)
		{
			foreach (GridLayout item in gridLayoutCollection)
			{
				arrayList.Add(new GridValueListItem(item, new string(' ', value * 3) + GetLayoutText(gridLayoutCollection.IndexOf(item), item), -1));
				ProcessArrayList(arrayList, item.ChildLayouts, value + 1);
			}
		}

		private string GetLayoutText(int value, GridLayout gridLayout)
		{
			return "[" + value + "] - '" + gridLayout.Key + "'";
		}

		private void UpdateHeaderForGrid()
		{
			if (_dataGrid is GridDropDown)
			{
				_layoutLabel.Visible = false;
				_layoutCombo.Visible = false;
				_lblName.Text = "Drop Down Designer";
			}
			else
			{
				_layoutLabel.Visible = true;
				_layoutCombo.Visible = true;
				UpdateMultiColumnComboEditorControl();
				_lblName.Text = "DataGrid Designer";
			}
			ResetTree();
		}

		private void ResetTree()
		{
			SetSelectedValue(null);
			RebuildTree();
			SelectNodeByTag(_dataGrid);
		}

		private void SelectNodeByTag(object value)
		{
			TrySelectNode(_tree.Nodes, value);
		}

		private bool TrySelectNode(TreeNodeCollection treeNodeCollection, object value)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (item.Tag == value)
				{
					item.EnsureVisible();
					_tree.SelectedNode = item;
					return true;
				}
				if (TrySelectNode(item.Nodes, value))
				{
					return true;
				}
			}
			return false;
		}

		private void RebuildTree()
		{
			_tree.Nodes.Clear();
			_tree.Nodes.Add(CreateNodeFor(_dataGrid));
			int width = _tree.Width;
			_tree.Width = 0;
			_tree.Width = width;
		}

		private TreeNode CreateNodeFor(object value)
		{
			TreeNode treeNode = null;
			if (value is GridDropDown)
			{
				treeNode = ComputeTreeNode((GridDropDown)value);
			}
			else if (value is MxDataGrid)
			{
				treeNode = CreateDataGridNode((MxDataGrid)value);
			}
			else if (value is GridTable)
			{
				treeNode = CreateTableNode((GridTable)value);
			}
			else if (value is GridColumnCollection)
			{
				treeNode = CreateColumnsNode((GridColumnCollection)value);
			}
			else if (value is GridColumnSetCollection)
			{
				treeNode = CreateColumnSetsNode((GridColumnSetCollection)value);
			}
			else if (value is GridFormatConditionCollection)
			{
				treeNode = CreateTreeNode((GridFormatConditionCollection)value);
			}
			else if (value is GridGroupCollection)
			{
				treeNode = CreateGroupsNode((GridGroupCollection)value);
			}
			else if (value is GridSortKeyCollection)
			{
				treeNode = CreateSortKeysNode((GridSortKeyCollection)value);
			}
			else if (value is GridChildTableCollection)
			{
				treeNode = CreateChildTablesNode((GridChildTableCollection)value);
			}
			else if (value is GridFormatStyleCollection)
			{
				treeNode = CreateFormatStylesNode((GridFormatStyleCollection)value);
			}
			else if (value is GridDropDownCollection)
			{
				treeNode = CreateDropDownsNode((GridDropDownCollection)value);
			}
			else if (value is GridFormatStyle)
			{
				treeNode = CreateFormatStyleNode((GridFormatStyle)value);
			}
			else if (value is GridGroupHeaderTotalCollection)
			{
				treeNode = CreateGroupHeaderTotalsNode((GridGroupHeaderTotalCollection)value);
			}
			else if (value is GridStoredFilterCollection)
			{
				treeNode = CreateStoredFiltersNode((GridStoredFilterCollection)value);
			}
			else if (value is GridBuiltInTextList)
			{
				treeNode = CreateBuiltInTextsNode((GridBuiltInTextList)value);
			}
			else if (value is GridSelfReferencingSettings)
			{
				treeNode = CreateSelfReferencingNode((GridSelfReferencingSettings)value);
			}
			else if (value is GridCustomGroupCollection)
			{
				treeNode = CreateCustomGroupsNode((GridCustomGroupCollection)value);
			}
			treeNode.Tag = value;
			return treeNode;
		}

		private TreeNode CreateFormatStyleNode(GridFormatStyle gridFormatStyle)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridFormatStyle), 7, 7);
		}

		private TreeNode CreateStoredFiltersNode(GridStoredFilterCollection gridStoredFilterCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridStoredFilterCollection), 13, 13);
		}

		private TreeNode CreateBuiltInTextsNode(GridBuiltInTextList gridBuiltInTextList)
		{
			return new TreeNode("BuiltInTexts", 15, 15);
		}

		private TreeNode CreateSelfReferencingNode(GridSelfReferencingSettings gridSelfReferencingSettings)
		{
			return new TreeNode("SelfReferencingSettings", 16, 16);
		}

		private TreeNode CreateGroupHeaderTotalsNode(GridGroupHeaderTotalCollection gridGroupHeaderTotalCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridGroupHeaderTotalCollection), 9, 9);
		}

		private TreeNode ComputeTreeNode(GridDropDown gridDropDown)
		{
			TreeNode treeNode = new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridDropDown), 14, 14);
			treeNode.Nodes.Add(CreateNodeFor(gridDropDown.Columns));
			treeNode.Nodes.Add(CreateNodeFor(gridDropDown.ColumnSets));
			treeNode.Nodes.Add(CreateNodeFor(gridDropDown.FormatConditions));
			treeNode.Nodes.Add(CreateNodeFor(gridDropDown.SortKeys));
			treeNode.Nodes.Add(CreateNodeFor(gridDropDown.Groups));
			return treeNode;
		}

		private TreeNode CreateDataGridNode(MxDataGrid dataGrid)
		{
			try
			{
				TreeNode treeNode = new TreeNode(GridHelper.GetNodeDisplayText(dataGrid, _dataGrid), 3, 3);
				if (dataGrid.RootTable != null)
				{
					treeNode.Nodes.Add(CreateNodeFor(dataGrid.RootTable));
				}
				else
				{
					treeNode.Nodes.Add(new TreeNode("RootTable - {null}", 4, 4));
				}
				treeNode.Nodes.Add(CreateNodeFor(dataGrid.FormatStyles));
				treeNode.Nodes.Add(CreatePredefinedStylesNode(dataGrid));
				treeNode.Nodes.Add(CreateNodeFor(dataGrid.DropDowns));
				treeNode.Nodes.Add(CreateNodeFor(dataGrid.BuiltInTexts));
				treeNode.Expand();
				return treeNode;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return null;
			}
		}

		private TreeNode CreatePredefinedStylesNode(MxDataGrid dataGrid)
		{
			TreeNode treeNode = new TreeNode("Predefined Styles", 11, 11);
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.AlternatingRowFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.CardCaptionFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.CardColumnHeaderFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.FilterRowFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.FocusCellFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.GroupByBoxFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.GroupByBoxInfoFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.GroupRowFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.GroupTotalRowFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.HeaderFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.NewRowFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.PreviewRowFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.RowWithErrorsFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.RowFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.SelectedFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.SelectedInactiveFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.TableHeaderFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.RowHeaderFormatStyle));
			treeNode.Nodes.Add(CreateNodeFor(dataGrid.TotalRowFormatStyle));
			return treeNode;
		}

		private TreeNode CreateTableNode(GridTable gridTable)
		{
			TreeNode treeNode = new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridTable), 4, 4);
			treeNode.Nodes.Add(CreateNodeFor(gridTable.Columns));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.ColumnSets));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.FormatConditions));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.SortKeys));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.Groups));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.GroupHeaderTotals));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.StoredFilters));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.CustomGroups));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.SelfReferencingSettings));
			treeNode.Nodes.Add(CreateNodeFor(gridTable.ChildTables));
			treeNode.Expand();
			return treeNode;
		}

		private TreeNode CreateFormatStylesNode(GridFormatStyleCollection gridFormatStyleCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridFormatStyleCollection), 7, 7);
		}

		private TreeNode CreateDropDownsNode(GridDropDownCollection gridDropDownCollection)
		{
			TreeNode treeNode = new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridDropDownCollection), 14, 14);
			foreach (object item in gridDropDownCollection)
			{
				treeNode.Nodes.Add(CreateNodeFor(item));
			}
			return treeNode;
		}

		private TreeNode CreateColumnsNode(GridColumnCollection gridColumnCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridColumnCollection), 5, 5);
		}

		private TreeNode CreateColumnSetsNode(GridColumnSetCollection gridColumnSetCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridColumnSetCollection), 6, 6);
		}

		private TreeNode CreateTreeNode(GridFormatConditionCollection gridFormatConditionCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridFormatConditionCollection), 13, 13);
		}

		private TreeNode CreateSortKeysNode(GridSortKeyCollection gridSortKeyCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridSortKeyCollection), 8, 8);
		}

		private TreeNode CreateCustomGroupsNode(GridCustomGroupCollection gridCustomGroupCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridCustomGroupCollection), 9, 9);
		}

		private TreeNode CreateGroupsNode(GridGroupCollection gridGroupCollection)
		{
			return new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridGroupCollection), 9, 9);
		}

		private TreeNode CreateChildTablesNode(GridChildTableCollection gridChildTableCollection)
		{
			TreeNode treeNode = new TreeNode(GridHelper.GetNodeDisplayText(_dataGrid, gridChildTableCollection), 10, 10);
			foreach (object item in gridChildTableCollection)
			{
				treeNode.Nodes.Add(CreateNodeFor(item));
			}
			return treeNode;
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MultiColumnComboEditorControl));
			GridLayout gridLayout = new GridLayout();
			_imageList = new ImageList(_iContainer);
			_lblName = new Label();
			_bodyPanel = new Panel();
			_rightPanel = new Panel();
			_treeSplitter = new Splitter();
			_tree = new TreeView();
			_layoutLabel = new Label();
			_layoutCombo = new MxMultiColumnComboBox();
			_bodyPanel.SuspendLayout();
			((ISupportInitialize)_layoutCombo).BeginInit();
			SuspendLayout();
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Magenta;
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
			_imageList.Images.SetKeyName(16, "selfreferencing.bmp");
			_lblName.BackColor = SystemColors.ControlDark;
			_lblName.Dock = DockStyle.Top;
			_lblName.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_lblName.ForeColor = SystemColors.ControlLightLight;
			_lblName.Location = new Point(0, 0);
			_lblName.Name = "lblName";
			_lblName.Size = new Size(672, 25);
			_lblName.TabIndex = 3;
			_lblName.Text = " DataGrid Designer";
			_lblName.TextAlign = ContentAlignment.MiddleLeft;
			_bodyPanel.Controls.Add(_rightPanel);
			_bodyPanel.Controls.Add(_treeSplitter);
			_bodyPanel.Controls.Add(_tree);
			_bodyPanel.Dock = DockStyle.Fill;
			_bodyPanel.Location = new Point(0, 25);
			_bodyPanel.Name = "bodyPanel";
			_bodyPanel.Padding = new Padding(4);
			_bodyPanel.Size = new Size(672, 391);
			_bodyPanel.TabIndex = 4;
			_rightPanel.Dock = DockStyle.Fill;
			_rightPanel.Location = new Point(216, 4);
			_rightPanel.Name = "rightPanel";
			_rightPanel.Size = new Size(452, 383);
			_rightPanel.TabIndex = 2;
			_treeSplitter.BackColor = SystemColors.Control;
			_treeSplitter.Location = new Point(208, 4);
			_treeSplitter.Name = "splitter1";
			_treeSplitter.Size = new Size(8, 383);
			_treeSplitter.TabIndex = 1;
			_treeSplitter.TabStop = false;
			_tree.Dock = DockStyle.Left;
			_tree.HideSelection = false;
			_tree.ImageIndex = 0;
			_tree.ImageList = _imageList;
			_tree.Location = new Point(4, 4);
			_tree.Name = "tree";
			_tree.SelectedImageIndex = 0;
			_tree.Size = new Size(204, 383);
			_tree.TabIndex = 0;
			_tree.AfterCollapse += OnTreeAfterCollapse;
			_tree.AfterSelect += OnTreeAfterSelect;
			_tree.BeforeSelect += OnTreeBeforeSelect;
			_tree.AfterExpand += OnTreeAfterExpand;
			_layoutLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_layoutLabel.AutoSize = true;
			_layoutLabel.BackColor = SystemColors.ControlDark;
			_layoutLabel.ForeColor = SystemColors.ControlLightLight;
			_layoutLabel.Location = new Point(397, 6);
			_layoutLabel.Name = "layoutLabel";
			_layoutLabel.Size = new Size(42, 13);
			_layoutLabel.TabIndex = 0;
			_layoutLabel.Text = "Layout:";
			_layoutCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_layoutCombo.BorderStyle = Mordex.Windows.DataGrid.BorderStyle.Flat;
			_layoutCombo.ComboStyle = ComboStyle.DropDownList;
			_layoutCombo.ControlStyle.ButtonAppearance = ButtonAppearance.Flat;
			gridLayout.LayoutString = componentResourceManager.GetString("gridLayout1.LayoutString");
			_layoutCombo.DesignTimeLayout = gridLayout;
			_layoutCombo.FlatBorderColor = SystemColors.ControlDark;
			_layoutCombo.Location = new Point(445, 2);
			_layoutCombo.Name = "multiColumnCombo1";
			_layoutCombo.SelectedIndex = -1;
			_layoutCombo.SelectedItem = null;
			_layoutCombo.Size = new Size(223, 20);
			_layoutCombo.TabIndex = 0;
			_layoutCombo.TextAlignment = TextAlignment.Near;
			_layoutCombo.UseCompatibleTextRendering = false;
			_layoutCombo.ValueChanged += OnMultiColumnCombo1ValueChanged;
			base.Controls.Add(_layoutLabel);
			base.Controls.Add(_layoutCombo);
			base.Controls.Add(_bodyPanel);
			base.Controls.Add(_lblName);
			Mordex.Windows.Common.MxHeaderLayout.Attach(_lblName, _layoutLabel, _layoutCombo);
			base.Name = "GeneralPage";
			base.Size = new Size(672, 416);
			_bodyPanel.ResumeLayout(performLayout: false);
			((ISupportInitialize)_layoutCombo).EndInit();
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		private void OnTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
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

		private object GetSelectedValue()
		{
			return _selectedValue;
		}

		private void SetSelectedValue(object value)
		{
			if (_selectedValue != value)
			{
				_selectedValue = value;
				if (_selectedValue == null)
				{
					UpdateIGridElement(null);
				}
				else if (_selectedValue is GridBuiltInTextList)
				{
					UpdateIGridElement(new GridBuiltInTextListEditorControl((GridBuiltInTextList)_selectedValue));
				}
				else if (!typeof(ICollection).IsAssignableFrom(_selectedValue.GetType()))
				{
					UpdateIGridElement(new ObjectPropertyEditorControl());
				}
				else
				{
					UpdateIGridElement(new GridEditorControl());
				}
				ShowEditorForValue();
			}
		}

		private TreeNode FindNodeByTag(TreeNodeCollection treeNodeCollection, object value)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (item.Tag == value)
				{
					return item;
				}
				TreeNode nodeByTag = FindNodeByTag(item.Nodes, value);
				if (nodeByTag != null)
				{
					return nodeByTag;
				}
			}
			return null;
		}

		private void RefreshDropDownNodes()
		{
			object value = _selectedValue;
			TreeNode treeNode = FindNodeByTag(_tree.Nodes, _dataGrid.DropDowns);
			if (treeNode == null)
			{
				return;
			}
			_tree.SelectedNode = null;
			treeNode.Nodes.Clear();
			foreach (GridDropDown dropDown in _dataGrid.DropDowns)
			{
				treeNode.Nodes.Add(CreateNodeFor(dropDown));
			}
			_tree.SelectedNode = FindNodeByTag(_tree.Nodes, value);
		}

		private IGridElement GetIGridElement()
		{
			return _iGridElement;
		}

		private void UpdateIGridElement(IGridElement iGridElement)
		{
			try
			{
				if (_iGridElement == iGridElement)
				{
					return;
				}
				if (_iGridElement != null)
				{
					if (!_iGridElement.CommitChanges())
					{
						return;
					}
					_iGridElement.SetSite(null);
					_iGridElement.SelectedObjectChanged -= OnStateSelectedObjectChanged;
					_iGridElement.RefreshTree -= OnStateRefreshTree;
					_iGridElement.DataChanged -= OnStateDataChanged;
					if (_iGridElement.SelectedObject is GridDropDownCollection)
					{
						RefreshDropDownNodes();
					}
					((Control)_iGridElement).Dispose();
				}
				_iGridElement = iGridElement;
				if (_iGridElement != null)
				{
					_iGridElement.SetSite(_dataGrid.Site);
					((Control)_iGridElement).Dock = DockStyle.Fill;
					try
					{
						_iGridElement.ItemImage = _imageList.Images[_tree.SelectedNode.ImageIndex];
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
					_iGridElement.SelectedObject = GetSelectedValue();
					_iGridElement.SelectedObjectChanged += OnStateSelectedObjectChanged;
					_iGridElement.RefreshTree += OnStateRefreshTree;
					_iGridElement.DataChanged += OnStateDataChanged;
				}
				ShowEditorForValue();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void ShowEditorForValue()
		{
			if (_iGridElement != null)
			{
				((Control)_iGridElement).Dock = DockStyle.Fill;
				_iGridElement.SelectedObject = GetSelectedValue();
				_rightPanel.Controls.Add((Control)_iGridElement);
			}
		}

		private void OnStateSelectedObjectChanged(object sender, EventArgs eventArgs)
		{
			ShowEditorForValue();
		}

		private void OnStateDataChanged(object sender, EventArgs eventArgs)
		{
			OnDataChanged(eventArgs);
		}

		private void OnStateRefreshTree(object sender, EventArgs eventArgs)
		{
			OnDataChanged(EventArgs.Empty);
			object value = _selectedValue;
			SetSelectedValue(null);
			RebuildTree();
			SelectNodeByTag(value);
			if (_selectedValue == null)
			{
				SelectNodeByTag(_dataGrid);
			}
		}

		private void OnMultiColumnCombo1ValueChanged(object sender, EventArgs eventArgs)
		{
			if (_dataGrid.CurrentLayout != (GridLayout)_layoutCombo.Value && (_iGridElement == null || GetIGridElement().CommitChanges()))
			{
				UpdateIGridElement(null);
				if (_dataGrid.CurrentLayout != null)
				{
					_dataGrid.CurrentLayout.Update();
				}
				_dataGrid.CurrentLayout = (GridLayout)_layoutCombo.Value;
				ResetTree();
				OnDataChanged(EventArgs.Empty);
			}
		}

		private void OnTreeAfterCollapse(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == 12)
			{
				eventArgs.Node.ImageIndex = 11;
				eventArgs.Node.SelectedImageIndex = 11;
			}
		}

		private void OnTreeAfterExpand(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == 11)
			{
				eventArgs.Node.ImageIndex = 12;
				eventArgs.Node.SelectedImageIndex = 12;
			}
		}

		private void OnTreeBeforeSelect(object sender, TreeViewCancelEventArgs eventArgs)
		{
			if (_iGridElement != null && !_iGridElement.CommitChanges())
			{
				eventArgs.Cancel = true;
			}
		}
	}
}
