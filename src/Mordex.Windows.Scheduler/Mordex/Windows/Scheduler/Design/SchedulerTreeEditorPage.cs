using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.Design
{
	[ToolboxItem(false)]
	internal class SchedulerTreeEditorPage : UserControl, ISchedulerEditorPage
	{

		private IContainer _iContainer;

		private Label _layoutLabel;

		private ComboBox _comboBox;

		private ImageList _imageList;

		private ImageList _schedulerEditorControl2ImageList;

		private Label _lblName;

		private Panel _bodyPanel;

		private Panel _rightPanel;

		private Splitter _treeSplitter;

		private TreeView _tree;

		private MxScheduler _scheduler;

		private object _value;

		private ISchedulerElement _iSchedulerElement;

		private static object _schedulerEditorControl2Value = new object();

		MxScheduler ISchedulerEditorPage.Scheduler
		{
			get
			{
				return _scheduler;
			}
			set
			{
				if (_scheduler != value)
				{
					_scheduler = value;
					InitializeForScheduler();
				}
			}
		}

		event EventHandler ISchedulerEditorPage.SelectTab
		{
			add
			{
				base.Events.AddHandler(_schedulerEditorControl2Value, value);
			}
			remove
			{
				base.Events.RemoveHandler(_schedulerEditorControl2Value, value);
			}
		}

		internal SchedulerTreeEditorPage()
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

		bool ISchedulerEditorPage.CommitChanges()
		{
			if (_iSchedulerElement != null)
			{
				return _iSchedulerElement.CommitChanges();
			}
			return true;
		}

		void ISchedulerEditorPage.Unload()
		{
			if (_iSchedulerElement != null)
			{
				_iSchedulerElement.Unload();
			}
		}

		private void FillLayoutComboBox()
		{
			_comboBox.SelectedValueChanged -= OnComboBoxSelectedValueChanged;
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new ON(null, "(none)"));
			AddLayoutItems(arrayList, _scheduler.Layouts, 0);
			_comboBox.ValueMember = "Object";
			_comboBox.DisplayMember = "Text";
			_comboBox.DataSource = arrayList;
			_comboBox.SelectedValueChanged += OnComboBoxSelectedValueChanged;
			if (_scheduler.CurrentLayout != null)
			{
				_comboBox.SelectedValue = _scheduler.CurrentLayout;
			}
			else
			{
				_comboBox.SelectedIndex = -1;
			}
		}

		private void AddLayoutItems(ArrayList arrayList, SchedulerLayoutCollection schedulerLayoutCollection, int depth)
		{
			foreach (SchedulerLayout item in schedulerLayoutCollection)
			{
				arrayList.Add(new ON(item, new string(' ', depth * 3) + GetLayoutDisplayText(schedulerLayoutCollection.IndexOf(item), item)));
				AddLayoutItems(arrayList, item.ChildLayouts, depth + 1);
			}
		}

		private string GetLayoutDisplayText(int layoutIndex, SchedulerLayout schedulerLayout)
		{
			return "[" + layoutIndex + "] - '" + schedulerLayout.Key + "'";
		}

		private void InitializeForScheduler()
		{
			_layoutLabel.Visible = true;
			_comboBox.Visible = true;
			FillLayoutComboBox();
			_lblName.Text = "Schedule Designer";
			ReloadTree();
		}

		private void ReloadTree()
		{
			SetSelectedValue(null);
			FillTree();
			SelectNodeByValue(_scheduler);
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
			_tree.Nodes.Add(CreateNodeForValue(_scheduler));
			int width = _tree.Width;
			_tree.Width = 0;
			_tree.Width = width;
		}

		private TreeNode CreateNodeForValue(object value)
		{
			TreeNode treeNode = null;
			if (value is MxScheduler)
			{
				treeNode = CreateSchedulerNode((MxScheduler)value);
			}
			else if (value is SchedulerFieldCollection)
			{
				treeNode = CreateTreeNode((SchedulerFieldCollection)value);
			}
			else if (value is SchedulerFormatConditionCollection)
			{
				treeNode = CreateFormatConditionsNode((SchedulerFormatConditionCollection)value);
			}
			else if (value is SchedulerAppointmentOwnerCollection)
			{
				treeNode = CreateOwnersNode((SchedulerAppointmentOwnerCollection)value);
			}
			else if (value is SchedulerFormatStyle)
			{
				treeNode = CreateFormatStyleNode((SchedulerFormatStyle)value);
			}
			else if (value is SchedulerStoredFilterCollection)
			{
				treeNode = CreateStoredFiltersNode((SchedulerStoredFilterCollection)value);
			}
			else if (value is WorkingHoursSchema)
			{
				treeNode = CreateWorkingHoursNode((WorkingHoursSchema)value);
			}
			else if (value is SchedulerHourRangeCollection)
			{
				treeNode = CreateWorkingHoursRangeNode((SchedulerHourRangeCollection)value);
			}
			else if (value is WorkingHourExceptionCollection)
			{
				treeNode = CreateExceptionsNode((WorkingHourExceptionCollection)value);
			}
			treeNode.Tag = value;
			return treeNode;
		}

		private TreeNode CreateFormatStyleNode(SchedulerFormatStyle schedulerFormatStyle)
		{
			return new TreeNode(SchedulerRenderer.GetDisplayText(_scheduler, schedulerFormatStyle), 5, 5);
		}

		private TreeNode CreateStoredFiltersNode(SchedulerStoredFilterCollection schedulerStoredFilterCollection)
		{
			return new TreeNode(SchedulerRenderer.GetDisplayText(_scheduler, schedulerStoredFilterCollection), 7, 7);
		}

		private TreeNode CreateOwnersNode(SchedulerAppointmentOwnerCollection schedulerAppointmentOwnerCollection)
		{
			return new TreeNode(SchedulerRenderer.GetDisplayText(_scheduler, schedulerAppointmentOwnerCollection), 6, 6);
		}

		private TreeNode CreateSchedulerNode(MxScheduler scheduler)
		{
			try
			{
				TreeNode treeNode = new TreeNode(SchedulerRenderer.GetDisplayText(scheduler, _scheduler), 0, 0);
				treeNode.Nodes.Add(CreateNodeForValue(scheduler.Fields));
				treeNode.Nodes.Add(CreateNodeForValue(scheduler.FormatConditions));
				treeNode.Nodes.Add(CreateNodeForValue(scheduler.Owners));
				treeNode.Nodes.Add(CreateNodeForValue(scheduler.StoredFilters));
				treeNode.Nodes.Add(CreateNodeForValue(scheduler.WorkingHourSchema));
				treeNode.Nodes.Add(CreatePredefinedStylesNode(scheduler));
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

		private TreeNode CreatePredefinedStylesNode(MxScheduler scheduler)
		{
			TreeNode treeNode = new TreeNode("Predefined Styles", 3, 3);
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.AllDayAreaBackgroundStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.AlternatingHourBackgroundStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.AlternatingMonthDaysFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.AlternatingWorkingHourBackgroundStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.AppointmentsFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.HeadersFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.HourBackgroundStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.MonthDaysFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.SelectedFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.SelectedInactiveFormatStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.WeekDaysBackgroundStyle));
			treeNode.Nodes.Add(CreateNodeForValue(scheduler.WorkingHourBackgroundStyle));
			return treeNode;
		}

		private TreeNode CreateWorkingHoursRangeNode(SchedulerHourRangeCollection schedulerHourRangeCollection)
		{
			return new TreeNode("WorkingHoursRange", 8, 8);
		}

		private TreeNode CreateExceptionsNode(WorkingHourExceptionCollection workingHourExceptionCollection)
		{
			return new TreeNode("Exceptions", 9, 9);
		}

		private TreeNode CreateWorkingHoursNode(WorkingHoursSchema workingHoursSchema)
		{
			TreeNode treeNode = new TreeNode("WorkingHoursSchema", 10, 10);
			treeNode.Nodes.Add(CreateNodeForValue(workingHoursSchema.WorkingHoursRange));
			treeNode.Nodes.Add(CreateNodeForValue(workingHoursSchema.Exceptions));
			return treeNode;
		}

		private TreeNode CreateTreeNode(SchedulerFieldCollection schedulerFieldCollection)
		{
			return new TreeNode(SchedulerRenderer.GetDisplayText(_scheduler, schedulerFieldCollection), 1, 1);
		}

		private TreeNode CreateFormatConditionsNode(SchedulerFormatConditionCollection schedulerFormatConditionCollection)
		{
			return new TreeNode(SchedulerRenderer.GetDisplayText(_scheduler, schedulerFormatConditionCollection), 2, 2);
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(SchedulerTreeEditorPage));
			_imageList = new ImageList(_iContainer);
			_lblName = new Label();
			_bodyPanel = new Panel();
			_rightPanel = new Panel();
			_treeSplitter = new Splitter();
			_tree = new TreeView();
			_schedulerEditorControl2ImageList = new ImageList(_iContainer);
			_comboBox = new ComboBox();
			_layoutLabel = new Label();
			_bodyPanel.SuspendLayout();
			SuspendLayout();
			ImageStrip.Load(_imageList, componentResourceManager, "oldicons.ImageStrip");
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
			_imageList.Images.SetKeyName(11, "");
			_imageList.Images.SetKeyName(12, "");
			_imageList.Images.SetKeyName(13, "");
			_imageList.Images.SetKeyName(14, "");
			_lblName.BackColor = SystemColors.ControlDark;
			_lblName.Dock = DockStyle.Top;
			_lblName.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_lblName.ForeColor = SystemColors.ControlLightLight;
			_lblName.Location = new Point(0, 0);
			_lblName.Name = "lblName";
			_lblName.Size = new Size(672, 27);
			_lblName.TabIndex = 3;
			_lblName.Text = " Schedule Designer";
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
			_tree.ImageList = _schedulerEditorControl2ImageList;
			_tree.Location = new Point(4, 4);
			_tree.Name = "tree";
			_tree.SelectedImageIndex = 0;
			_tree.Size = new Size(204, 381);
			_tree.TabIndex = 0;
			_tree.AfterCollapse += OnTreeAfterCollapse;
			_tree.AfterSelect += OnTreeAfterSelect;
			_tree.AfterExpand += OnTreeAfterExpand;
			ImageStrip.Load(_schedulerEditorControl2ImageList, componentResourceManager, "icons.ImageStrip");
			_schedulerEditorControl2ImageList.TransparentColor = Color.Transparent;
			_schedulerEditorControl2ImageList.Images.SetKeyName(0, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(1, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(2, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(3, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(4, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(5, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(6, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(7, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(8, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(9, "");
			_schedulerEditorControl2ImageList.Images.SetKeyName(10, "");
			_comboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_comboBox.BackColor = SystemColors.Window;
			_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_comboBox.ForeColor = SystemColors.WindowText;
			_comboBox.Location = new Point(517, 3);
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
					UpdateISchedulerElement(null);
				}
				else if (!typeof(ICollection).IsAssignableFrom(_value.GetType()))
				{
					UpdateISchedulerElement(new SchedulerObjectDesigner());
				}
				else
				{
					UpdateISchedulerElement(new SchedulerEditorControl());
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

		private ISchedulerElement GetISchedulerElement()
		{
			return _iSchedulerElement;
		}

		private void UpdateISchedulerElement(ISchedulerElement iSchedulerElement)
		{
			if (_iSchedulerElement == iSchedulerElement)
			{
				return;
			}
			if (_iSchedulerElement != null)
			{
				if (!_iSchedulerElement.CommitChanges())
				{
					return;
				}
				_iSchedulerElement.SelectedObjectChanged -= OnStateSelectedObjectChanged;
				_iSchedulerElement.RefreshTree -= OnStateRefreshTree;
				((Control)_iSchedulerElement).Dispose();
			}
			_iSchedulerElement = iSchedulerElement;
			if (_iSchedulerElement != null)
			{
				((Control)_iSchedulerElement).Dock = DockStyle.Fill;
				try
				{
					_iSchedulerElement.ItemImage = _schedulerEditorControl2ImageList.Images[_tree.SelectedNode.ImageIndex];
				}
				catch
				{
				}
				_iSchedulerElement.SelectedObject = GetSelectedValue();
				_iSchedulerElement.SelectedObjectChanged += OnStateSelectedObjectChanged;
				_iSchedulerElement.RefreshTree += OnStateRefreshTree;
				_ = _scheduler;
			}
			ShowElementEditor();
		}

		private void ShowElementEditor()
		{
			if (_iSchedulerElement != null)
			{
				((Control)_iSchedulerElement).Dock = DockStyle.Fill;
				_iSchedulerElement.SelectedObject = GetSelectedValue();
				_rightPanel.Controls.Add((Control)_iSchedulerElement);
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
				SelectNodeByValue(_scheduler);
			}
		}

		private void OnComboBoxSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			if (_scheduler.CurrentLayout != (SchedulerLayout)_comboBox.SelectedValue && (_iSchedulerElement == null || GetISchedulerElement().CommitChanges()))
			{
				UpdateISchedulerElement(null);
				if (_scheduler.CurrentLayout != null)
				{
					_scheduler.CurrentLayout.Update();
				}
				_scheduler.CurrentLayout = (SchedulerLayout)_comboBox.SelectedValue;
				ReloadTree();
			}
		}

		private void OnTreeAfterCollapse(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == 4)
			{
				eventArgs.Node.ImageIndex = 3;
				eventArgs.Node.SelectedImageIndex = 3;
			}
		}

		private void OnTreeAfterExpand(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node.ImageIndex == 3)
			{
				eventArgs.Node.ImageIndex = 4;
				eventArgs.Node.SelectedImageIndex = 4;
			}
		}
	}
}
