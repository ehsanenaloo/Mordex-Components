using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design.Designer
{
	internal class TimelineEditorControl : UserControl, ITimelineDesignerPage
	{
		private ImageList _imageList;

		private SaveFileDialog _saveFileDialog;

		private MxToolBar _tbLayouts;

		private MxToolBarButton _loadLayoutButton;

		private MxToolBarButton _saveLayoutButton;

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

		private MxTimeline _timeline;

		private static object _value = new object();

		MxTimeline ITimelineDesignerPage.Timeline
		{
			get
			{
				return _timeline;
			}
			set
			{
				if (value != _timeline)
				{
					_timeline = value;
					UpdateTimelineEditorControl();
				}
			}
		}

		event EventHandler ITimelineDesignerPage.SelectTab
		{
			add
			{
				base.Events.AddHandler(_value, value);
			}
			remove
			{
				base.Events.RemoveHandler(_value, value);
			}
		}

		internal TimelineEditorControl()
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
			ResourceManager resourceManager = new ResourceManager(typeof(TimelineEditorControl));
			_imageList = new ImageList(_iContainer);
			_saveFileDialog = new SaveFileDialog();
			_tbLayouts = new MxToolBar();
			_loadLayoutButton = new MxToolBarButton();
			_saveLayoutButton = new MxToolBarButton();
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
			_saveFileDialog.DefaultExt = "xml";
			_saveFileDialog.Filter = "Timeline Layout Files (*.xml)|*.xml|All Files|*.*";
			_saveFileDialog.Title = "Save As";
			_tbLayouts.Appearance = MxToolBarAppearance.Flat;
			_tbLayouts.Buttons.AddRange(new MxToolBarButton[2] { _loadLayoutButton, _saveLayoutButton });
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
			_loadLayoutButton.ImageIndex = 0;
			_loadLayoutButton.Text = "Load Layout File...";
			_saveLayoutButton.ImageIndex = 1;
			_saveLayoutButton.Text = "Save Layout File As...";
			_openFileDialog.AddExtension = false;
			_openFileDialog.Filter = "Timeline Layout Files (*.xml)|*.xml|All Files|*.*";
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
			_layoutProperties.Location = new Point(2, 0);
			_layoutProperties.Name = "layoutProperties";
			_layoutProperties.PropertySort = PropertySort.Alphabetical;
			_layoutProperties.Size = new Size(233, 296);
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

		bool ITimelineDesignerPage.CommitChanges()
		{
			return true;
		}

		void ITimelineDesignerPage.Unload()
		{
		}

		private void UpdateTimelineEditorControl()
		{
			_layoutTree.Nodes.Clear();
			TimelineLayout currentLayout = _timeline.CurrentLayout;
			if (currentLayout == null)
			{
				currentLayout = _timeline.GetLayout();
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
			foreach (TimelineLayout layout in _timeline.Layouts)
			{
				_layoutTree.Nodes.Add(CreateLayoutNode(layout));
			}
			if (_timeline.CurrentLayout != null)
			{
				SelectLayoutNode(_timeline.CurrentLayout);
			}
			_layoutTree.Focus();
			UpdateButtonStates();
		}

		private TreeNode CreateLayoutNode(TimelineLayout layout)
		{
			string text = "";
			text = ((layout.GetTimelineLayoutCollection() != null) ? ("[" + layout.GetTimelineLayoutCollection().IndexOf(layout) + "] - '" + layout.Key + "'") : "Draft");
			TreeNode treeNode = new TreeNode(text, 2, 2);
			if (layout._isCurrentLayout)
			{
				treeNode.SelectedImageIndex = 3;
				treeNode.ImageIndex = 3;
			}
			treeNode.Tag = layout;
			foreach (TimelineLayout childLayout in layout.ChildLayouts)
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
				TimelineLayout timelineLayout = (TimelineLayout)selectedNode.Tag;
				if (timelineLayout.GetTimelineLayoutCollection() != null)
				{
					timelineLayout.GetTimelineLayoutCollection().Remove(timelineLayout);
				}
				UpdateTimelineEditorControl();
				UpdateButtonStates();
			}
		}

		private void OnSaveCurrentLayoutClick(object sender, EventArgs eventArgs)
		{
			TimelineControlDialog timelineControlDialog = new TimelineControlDialog();
			timelineControlDialog.SetLayoutName("");
			if (timelineControlDialog.ShowDialog() == DialogResult.OK)
			{
				TimelineLayout layout = _timeline.GetLayout();
				_timeline.Layouts.Add(layout);
				try
				{
					layout.Key = timelineControlDialog.GetLayoutName();
				}
				catch
				{
					int i;
					for (i = 1; _timeline.Layouts.Contains("Layout" + i); i++)
					{
					}
					layout.Key = "Layout" + i;
				}
				_timeline.CurrentLayout = layout;
				UpdateTimelineEditorControl();
			}
			timelineControlDialog.Dispose();
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
				TimelineLayout timelineLayout = (TimelineLayout)selectedNode.Tag;
				if (timelineLayout == _timeline.CurrentLayout)
				{
					timelineLayout.Update();
				}
				TimelineLayout copiedLayout = timelineLayout.CloneLayout(includeChildren: true);
				copiedLayout.Key = "CopyOf" + timelineLayout.Key;
				if (timelineLayout.GetTimelineLayoutCollection() != null)
				{
					if (timelineLayout.GetTimelineLayoutCollection().Contains(copiedLayout.Key))
					{
						int i;
						for (i = 1; timelineLayout.GetTimelineLayoutCollection().Contains(copiedLayout.Key + i); i++)
						{
						}
						copiedLayout.Key += i;
					}
					timelineLayout.GetTimelineLayoutCollection().Add(copiedLayout);
				}
				_timeline.CurrentLayout = copiedLayout;
				UpdateTimelineEditorControl();
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
				TimelineLayout timelineLayout = (TimelineLayout)selectedNode.Tag;
				if (timelineLayout.GetTimelineLayoutCollection() == null)
				{
					RaiseSelectTab(EventArgs.Empty);
					return;
				}
				if (_timeline.CurrentLayout == null)
				{
					if (MessageBox.Show("You are about to select a layout that will overwrite all the settings that you may have set in the control.\n\rTo save the current layout in the Layouts Collection of the control, cancel this operation and click in the 'Save Current Layout' button.\n\rDo you want to continue?", "Timeline Control", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
					{
						return;
					}
				}
				else
				{
					_timeline.CurrentLayout.Update();
				}
				_timeline.CurrentLayout = timelineLayout;
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
			if (eventArgs.Button == _loadLayoutButton)
			{
				if (_openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					Stream stream = _openFileDialog.OpenFile();
					_timeline.LoadLayoutFile(stream);
					UpdateTimelineEditorControl();
				}
			}
			else if (eventArgs.Button == _saveLayoutButton && _saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				Stream saveStream = _saveFileDialog.OpenFile();
				_timeline.SaveLayoutFile(saveStream);
				saveStream.Close();
			}
		}

		private void OnNewLayoutClick(object sender, EventArgs eventArgs)
		{
			TimelineLayout timelineLayout = new TimelineLayout();
			int i;
			for (i = 1; _timeline.Layouts.Contains("Layout" + i); i++)
			{
			}
			timelineLayout.Key = "Layout" + i;
			_timeline.Layouts.Add(timelineLayout);
			UpdateTimelineEditorControl();
			SelectLayoutNode(timelineLayout);
		}

		private void RaiseSelectTab(EventArgs eventArgs)
		{
			if (base.Events[_value] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnLayoutTreeAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (_layoutTree.SelectedNode == null || ((TimelineLayout)_layoutTree.SelectedNode.Tag).GetTimelineLayoutCollection() == null)
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
			if (((TimelineLayout)_layoutTree.SelectedNode.Tag).GetTimelineLayoutCollection() != null)
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
				TimelineLayout timelineLayout = (TimelineLayout)_layoutTree.SelectedNode.Tag;
				TimelineLayout childLayout = new TimelineLayout();
				childLayout.DataSource = timelineLayout.DataSource;
				childLayout.DataMember = timelineLayout.DataMember;
				int i;
				for (i = 1; timelineLayout.ChildLayouts.Contains("Layout" + i); i++)
				{
				}
				childLayout.Key = "Layout" + i;
				timelineLayout.ChildLayouts.Add(childLayout);
				UpdateTimelineEditorControl();
				SelectLayoutNode(childLayout);
			}
		}

		private void SelectLayoutNode(TimelineLayout timelineLayout)
		{
			_layoutTree.SelectedNode = FindTreeNode(_layoutTree.Nodes, timelineLayout);
		}

		private TreeNode FindTreeNode(TreeNodeCollection treeNodeCollection, TimelineLayout timelineLayout)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if ((TimelineLayout)item.Tag == timelineLayout)
				{
					return item;
				}
				TreeNode foundNode = FindTreeNode(item.Nodes, timelineLayout);
				if (foundNode != null)
				{
					return foundNode;
				}
			}
			return null;
		}
	}
}
