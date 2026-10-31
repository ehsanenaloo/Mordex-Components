using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design.Designer
{
	[ToolboxItem(false)]
	public class GeneralPage : UserControl, ITimelineDesignerPage
	{

		private IContainer _iContainer;

		private Label _layoutLabel;

		private ComboBox _comboBox;

		private ImageList _imageList;

		private Label _lblName;

		private Panel _bodyPanel;

		private Panel _rightPanel;

		private Splitter _treeSplitter;

		private TreeView _tree;

		private MxTimeline _timeline;

		private object _value;

		private ITimelineElement _iTimelineElement;

		private static object _selectTabEventKey = new object();

		MxTimeline ITimelineDesignerPage.Timeline
		{
			get
			{
				return _timeline;
			}
			set
			{
				if (_timeline != value)
				{
					_timeline = value;
					InitializeForTimeline();
				}
			}
		}

		event EventHandler ITimelineDesignerPage.SelectTab
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

		internal GeneralPage()
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

		bool ITimelineDesignerPage.CommitChanges()
		{
			if (_iTimelineElement != null)
			{
				return _iTimelineElement.CommitChanges();
			}
			return true;
		}

		void ITimelineDesignerPage.Unload()
		{
			if (_iTimelineElement != null)
			{
				_iTimelineElement.Unload();
			}
		}

		private void UpdateGeneralPage()
		{
			_comboBox.SelectedValueChanged -= OnComboBoxSelectedValueChanged;
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new TimelineValueListItem(null, "(none)", -1));
			AddLayoutItems(arrayList, _timeline.Layouts, 0);
			_comboBox.ValueMember = "Value";
			_comboBox.DisplayMember = "Text";
			_comboBox.DataSource = arrayList;
			_comboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			if (_timeline.CurrentLayout != null)
			{
				_comboBox.SelectedValue = _timeline.CurrentLayout;
			}
			else
			{
				_comboBox.SelectedIndex = -1;
			}
		}

		private void AddLayoutItems(ArrayList arrayList, TimelineLayoutCollection timelineLayoutCollection, int depth)
		{
			foreach (TimelineLayout item in timelineLayoutCollection)
			{
				arrayList.Add(new TimelineValueListItem(item, new string(' ', depth * 3) + FormatLayoutLabel(timelineLayoutCollection.IndexOf(item), item), -1));
				AddLayoutItems(arrayList, item.ChildLayouts, depth + 1);
			}
		}

		private string FormatLayoutLabel(int layoutIndex, TimelineLayout timelineLayout)
		{
			return "[" + layoutIndex + "] - '" + timelineLayout.Key + "'";
		}

		private void InitializeForTimeline()
		{
			_layoutLabel.Visible = true;
			_comboBox.Visible = true;
			UpdateGeneralPage();
			_lblName.Text = "Timeline Designer";
			ReloadTree();
		}

		private void ReloadTree()
		{
			SetSelectedValue(null);
			FillTree();
			SelectNodeByValue(_timeline);
		}

		private void SelectNodeByValue(object value)
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

		private void FillTree()
		{
			_tree.Nodes.Clear();
			_tree.Nodes.Add(CreateNodeForValue(_timeline));
			int width = _tree.Width;
			_tree.Width = 0;
			_tree.Width = width;
		}

		private TreeNode CreateNodeForValue(object value)
		{
			TreeNode treeNode = null;
			if (value is MxTimeline)
			{
				treeNode = CreateTimelineNode((MxTimeline)value);
			}
			else if (value is TimelineFieldCollection)
			{
				treeNode = CreateFieldsNode((TimelineFieldCollection)value);
			}
			else if (value is TimelineFormatConditionCollection)
			{
				treeNode = CreateFormatConditionsNode((TimelineFormatConditionCollection)value);
			}
			else if (value is TimelineGroupCollection)
			{
				treeNode = CreateGroupsNode((TimelineGroupCollection)value);
			}
			else if (value is TimelineFormatStyle)
			{
				treeNode = CreateTreeNode((TimelineFormatStyle)value);
			}
			else if (value is TimelineGroupHeaderTotalCollection)
			{
				treeNode = CreateGroupHeaderTotalsNode((TimelineGroupHeaderTotalCollection)value);
			}
			else if (value is TimelineStoredFilterCollection)
			{
				treeNode = CreateStoredFiltersNode((TimelineStoredFilterCollection)value);
			}
			else if (value is WorkingHoursSchema)
			{
				treeNode = CreateWorkingHoursSchemaNode((WorkingHoursSchema)value);
			}
			else if (value is TimelineHourRangeCollection)
			{
				treeNode = CreateWorkingHoursRangeNode((TimelineHourRangeCollection)value);
			}
			else if (value is WorkingHourExceptionCollection)
			{
				treeNode = CreateExceptionsNode((WorkingHourExceptionCollection)value);
			}
			treeNode.Tag = value;
			return treeNode;
		}

		private TreeNode CreateTreeNode(TimelineFormatStyle timelineFormatStyle)
		{
			return new TreeNode(TimelineUtilities.GetObjectDisplayText(_timeline, timelineFormatStyle), 7, 7);
		}

		private TreeNode CreateStoredFiltersNode(TimelineStoredFilterCollection timelineStoredFilterCollection)
		{
			return new TreeNode(TimelineUtilities.GetObjectDisplayText(_timeline, timelineStoredFilterCollection), 4, 4);
		}

		private TreeNode CreateGroupHeaderTotalsNode(TimelineGroupHeaderTotalCollection timelineGroupHeaderTotalCollection)
		{
			return new TreeNode(TimelineUtilities.GetObjectDisplayText(_timeline, timelineGroupHeaderTotalCollection), 3, 3);
		}

		private TreeNode CreateTimelineNode(MxTimeline timeline)
		{
			try
			{
				TreeNode treeNode = new TreeNode(TimelineUtilities.GetObjectDisplayText(timeline, _timeline), 0, 0);
				treeNode.Nodes.Add(CreateNodeForValue(timeline.GroupHeaderTotals));
				treeNode.Nodes.Add(CreateNodeForValue(timeline.Groups));
				treeNode.Nodes.Add(CreateNodeForValue(timeline.Fields));
				treeNode.Nodes.Add(CreateNodeForValue(timeline.FormatConditions));
				treeNode.Nodes.Add(CreateNodeForValue(timeline.StoredFilters));
				treeNode.Nodes.Add(CreateNodeForValue(timeline.WorkingHourSchema));
				treeNode.Nodes.Add(CreatePredefinedStylesNode(timeline));
				treeNode.Expand();
				return treeNode;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return null;
			}
		}

		private TreeNode CreateWorkingHoursRangeNode(TimelineHourRangeCollection timelineHourRangeCollection)
		{
			return new TreeNode("WorkingHoursRange", 8, 8);
		}

		private TreeNode CreateExceptionsNode(WorkingHourExceptionCollection workingHourExceptionCollection)
		{
			return new TreeNode("Exceptions", 9, 9);
		}

		private TreeNode CreateWorkingHoursSchemaNode(WorkingHoursSchema workingHoursSchema)
		{
			TreeNode treeNode = new TreeNode("WorkingHoursSchema", 10, 10);
			treeNode.Nodes.Add(CreateNodeForValue(workingHoursSchema.WorkingHoursRange));
			treeNode.Nodes.Add(CreateNodeForValue(workingHoursSchema.Exceptions));
			return treeNode;
		}

		private TreeNode CreatePredefinedStylesNode(MxTimeline timeline)
		{
			TreeNode treeNode = new TreeNode("Predefined Styles", 5, 5);
			treeNode.Nodes.Add(CreateNodeForValue(timeline.BackgroundFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.GroupRowsFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.ItemsFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.ItemsBarFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.ItemsEstimatedBarFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.LowerScaleTierFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.RowHeadersFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.SelectedFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.SelectedInactiveFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.UpperScaleTiersFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(timeline.WorkingTimeFormatStyle));
			return treeNode;
		}

		private TreeNode CreateFieldsNode(TimelineFieldCollection timelineFieldCollection)
		{
			return new TreeNode(TimelineUtilities.GetObjectDisplayText(_timeline, timelineFieldCollection), 1, 1);
		}

		private TreeNode CreateFormatConditionsNode(TimelineFormatConditionCollection timelineFormatConditionCollection)
		{
			return new TreeNode(TimelineUtilities.GetObjectDisplayText(_timeline, timelineFormatConditionCollection), 2, 2);
		}

		private TreeNode CreateGroupsNode(TimelineGroupCollection timelineGroupCollection)
		{
			return new TreeNode(TimelineUtilities.GetObjectDisplayText(_timeline, timelineGroupCollection), 3, 3);
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(GeneralPage));
			_lblName = new Label();
			_bodyPanel = new Panel();
			_rightPanel = new Panel();
			_treeSplitter = new Splitter();
			_tree = new TreeView();
			_imageList = new ImageList(_iContainer);
			_comboBox = new ComboBox();
			_layoutLabel = new Label();
			_bodyPanel.SuspendLayout();
			SuspendLayout();
			_lblName.BackColor = SystemColors.ControlDark;
			_lblName.Dock = DockStyle.Top;
			_lblName.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_lblName.ForeColor = SystemColors.ControlLightLight;
			_lblName.Location = new Point(0, 0);
			_lblName.Name = "lblName";
			_lblName.Size = new Size(672, 27);
			_lblName.TabIndex = 3;
			_lblName.Text = " Timeline Designer";
			_lblName.TextAlign = ContentAlignment.MiddleLeft;
			_bodyPanel.Controls.Add(_rightPanel);
			_bodyPanel.Controls.Add(_treeSplitter);
			_bodyPanel.Controls.Add(_tree);
			_bodyPanel.Dock = DockStyle.Fill;
			_bodyPanel.Location = new Point(0, 27);
			_bodyPanel.Name = "bodyPanel";
			_bodyPanel.Padding = new Padding(4);
			_bodyPanel.Size = new Size(672, 389);
			_bodyPanel.TabIndex = 4;
			_rightPanel.Dock = DockStyle.Fill;
			_rightPanel.Location = new Point(216, 4);
			_rightPanel.Name = "rightPanel";
			_rightPanel.Size = new Size(452, 381);
			_rightPanel.TabIndex = 2;
			_treeSplitter.Location = new Point(208, 4);
			_treeSplitter.Name = "splitter1";
			_treeSplitter.Size = new Size(8, 381);
			_treeSplitter.TabIndex = 1;
			_treeSplitter.TabStop = false;
			_tree.Dock = DockStyle.Left;
			_tree.HideSelection = false;
			_tree.ImageIndex = 0;
			_tree.ImageList = _imageList;
			_tree.Location = new Point(4, 4);
			_tree.Name = "tree";
			_tree.SelectedImageIndex = 0;
			_tree.Size = new Size(204, 381);
			_tree.TabIndex = 0;
			_tree.AfterCollapse += OnTreeAfterCollapse;
			_tree.AfterSelect += OnTreeAfterSelect;
			_tree.AfterExpand += OnTreeAfterExpand;
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
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
			_comboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_comboBox.BackColor = SystemColors.Window;
			_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_comboBox.ForeColor = SystemColors.WindowText;
			_comboBox.Location = new Point(520, 4);
			_comboBox.Name = "ComboBox1";
			_comboBox.Size = new Size(148, 21);
			_comboBox.TabIndex = 0;
			_comboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			_layoutLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_layoutLabel.AutoSize = true;
			_layoutLabel.BackColor = SystemColors.ControlDark;
			_layoutLabel.ForeColor = SystemColors.ControlLightLight;
			_layoutLabel.Location = new Point(476, 7);
			_layoutLabel.Name = "layoutLabel";
			_layoutLabel.Size = new Size(42, 13);
			_layoutLabel.TabIndex = 0;
			_layoutLabel.Text = "Layout:";
			base.Controls.Add(_layoutLabel);
			base.Controls.Add(_comboBox);
			base.Controls.Add(_bodyPanel);
			base.Controls.Add(_lblName);
			Mordex.Windows.Common.MxHeaderLayout.Attach(_lblName, _layoutLabel, _comboBox);
			base.Name = "GeneralPage";
			base.Size = new Size(672, 416);
			_bodyPanel.ResumeLayout(performLayout: false);
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
			return _value;
		}

		private void SetSelectedValue(object value)
		{
			if (_value != value)
			{
				_value = value;
				if (_value == null || _value is WorkingHoursSchema)
				{
					UpdateITimelineElement(null);
				}
				else if (!typeof(ICollection).IsAssignableFrom(_value.GetType()))
				{
					UpdateITimelineElement(new ObjectDesigner());
				}
				else
				{
					UpdateITimelineElement(new CollectionPage());
				}
				ShowElementEditor();
			}
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

		private ITimelineElement GetITimelineElement()
		{
			return _iTimelineElement;
		}

		private void UpdateITimelineElement(ITimelineElement iTimelineElement)
		{
			if (_iTimelineElement == iTimelineElement)
			{
				return;
			}
			if (_iTimelineElement != null)
			{
				if (!_iTimelineElement.CommitChanges())
				{
					return;
				}
				_iTimelineElement.SelectedObjectChanged -= OnStateSelectedObjectChanged;
				_iTimelineElement.RefreshTree -= OnStateRefreshTree;
				_iTimelineElement.SetSite(null);
				((Control)_iTimelineElement).Dispose();
			}
			_iTimelineElement = iTimelineElement;
			if (_iTimelineElement != null)
			{
				((Control)_iTimelineElement).Dock = DockStyle.Fill;
				try
				{
					_iTimelineElement.ItemImage = _imageList.Images[_tree.SelectedNode.ImageIndex];
				}
				catch
				{
				}
				_iTimelineElement.SelectedObject = GetSelectedValue();
				_iTimelineElement.SelectedObjectChanged += OnStateSelectedObjectChanged;
				_iTimelineElement.RefreshTree += OnStateRefreshTree;
				if (_timeline != null)
				{
					_iTimelineElement.SetSite(_timeline.Site);
				}
			}
			ShowElementEditor();
		}

		private void ShowElementEditor()
		{
			if (_iTimelineElement != null)
			{
				((Control)_iTimelineElement).Dock = DockStyle.Fill;
				_iTimelineElement.SelectedObject = GetSelectedValue();
				_rightPanel.Controls.Add((Control)_iTimelineElement);
			}
		}

		private void OnStateSelectedObjectChanged(object sender, EventArgs eventArgs)
		{
			ShowElementEditor();
		}

		private void OnStateRefreshTree(object sender, EventArgs eventArgs)
		{
			object previousValue = _value;
			SetSelectedValue(null);
			FillTree();
			SelectNodeByValue(previousValue);
			if (_value == null)
			{
				SelectNodeByValue(_timeline);
			}
		}

		private void OnComboBoxSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_timeline.CurrentLayout != (TimelineLayout)_comboBox.SelectedValue && (_iTimelineElement == null || GetITimelineElement().CommitChanges()))
			{
				UpdateITimelineElement(null);
				if (_timeline.CurrentLayout != null)
				{
					_timeline.CurrentLayout.Update();
				}
				_timeline.CurrentLayout = (TimelineLayout)_comboBox.SelectedValue;
				ReloadTree();
			}
		}

		private void OnTreeAfterCollapse(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == 6)
			{
				eventArgs.Node.ImageIndex = 5;
				eventArgs.Node.SelectedImageIndex = 5;
			}
		}

		private void OnTreeAfterExpand(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == 5)
			{
				eventArgs.Node.ImageIndex = 6;
				eventArgs.Node.SelectedImageIndex = 6;
			}
		}
	}
}
