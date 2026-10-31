using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	[ToolboxItem(false)]
	internal class CommandEditorControl : UserControl
	{
		private IContainer _components;

		private MxCommandBar _commandBar;

		private MxDockManager _dockManager;

		private MxCommandBarHost _topRebar;

		private ImageList _imageList;

		private MxDockPanel _propertiesPanel;

		private DockPanelInnerContainer _propertiesPanelContainer;

		private MxDockPanelGroup _panelGroup;

		private MxDockPanel _helpPanel;

		private DockPanelInnerContainer _helpPanelContainer;

		private PropertyGrid _propertyGrid;

		private Label _lblName;

		private Label _lblHelp;

		private MxCommand _appearanceCommand;

		private MxCommand _behaviorCommand;

		private MxCommand _imagesCommand;

		private MxCommand _bordersCommand;

		private MxCommand _allCommand;

		private MxCommand _barAppearanceCommand;

		private MxCommand _barBehaviorCommand;

		private MxCommand _barSeparatorCommand;

		private MxCommand _barAllCommand;

		private MxCommandBarHost _bottomRebar;

		private MxCommandBarHost _leftRebar;

		private MxCommandBarHost _rightRebar;

		private MxCommandManager _commandManager;

		private DockPanelBase _targetPanel;

		private string _viewMode = "";

		public CommandEditorControl()
		{
			InitializeControls();
			MxDockManager uiPanelManager = new MxDockManager();
			_propertyGrid.LineColor = uiPanelManager.GetDefaultColor(PanelColors.DarkCaption);
			_propertyGrid.HelpBackColor = uiPanelManager.GetDefaultColor(PanelColors.Caption);
			_propertyGrid.HelpForeColor = SystemColors.ControlText;
			_propertiesPanelContainer.DockPadding.All = -1;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(CommandEditorControl));
			_appearanceCommand = new MxCommand("Appearance");
			_behaviorCommand = new MxCommand("Behavior");
			_imagesCommand = new MxCommand("Images");
			_bordersCommand = new MxCommand("Borders");
			_allCommand = new MxCommand("All");
			_barAppearanceCommand = new MxCommand("Appearance");
			_barBehaviorCommand = new MxCommand("Behavior");
			_barSeparatorCommand = new MxCommand("Separator");
			_barAllCommand = new MxCommand("All");
			_commandManager = new MxCommandManager(_components);
			_bottomRebar = new MxCommandBarHost();
			_commandBar = new MxCommandBar();
			_imageList = new ImageList(_components);
			_leftRebar = new MxCommandBarHost();
			_rightRebar = new MxCommandBarHost();
			_topRebar = new MxCommandBarHost();
			_dockManager = new MxDockManager(_components);
			_panelGroup = new MxDockPanelGroup();
			_propertiesPanel = new MxDockPanel();
			_propertiesPanelContainer = new DockPanelInnerContainer();
			_propertyGrid = new PropertyGrid();
			_helpPanel = new MxDockPanel();
			_helpPanelContainer = new DockPanelInnerContainer();
			_lblHelp = new Label();
			_lblName = new Label();
			((ISupportInitialize)_commandManager).BeginInit();
			((ISupportInitialize)_bottomRebar).BeginInit();
			((ISupportInitialize)_commandBar).BeginInit();
			((ISupportInitialize)_leftRebar).BeginInit();
			((ISupportInitialize)_rightRebar).BeginInit();
			((ISupportInitialize)_topRebar).BeginInit();
			_topRebar.SuspendLayout();
			((ISupportInitialize)_dockManager).BeginInit();
			((ISupportInitialize)_panelGroup).BeginInit();
			_panelGroup.SuspendLayout();
			((ISupportInitialize)_propertiesPanel).BeginInit();
			_propertiesPanel.SuspendLayout();
			_propertiesPanelContainer.SuspendLayout();
			((ISupportInitialize)_helpPanel).BeginInit();
			_helpPanel.SuspendLayout();
			_helpPanelContainer.SuspendLayout();
			SuspendLayout();
			_appearanceCommand.ImageIndex = 0;
			_appearanceCommand.IsEditableControl = InheritableBoolean.False;
			_appearanceCommand.Key = "Appearance";
			_appearanceCommand.Name = "uiCommand1";
			_appearanceCommand.Text = "Appearance";
			_behaviorCommand.ImageIndex = 0;
			_behaviorCommand.Key = "Behavior";
			_behaviorCommand.Name = "uiCommand2";
			_behaviorCommand.Text = "Behavior";
			_imagesCommand.ImageIndex = 0;
			_imagesCommand.Key = "Images";
			_imagesCommand.Name = "uiCommand3";
			_imagesCommand.Text = "Panel Image";
			_bordersCommand.ImageIndex = 0;
			_bordersCommand.Key = "Borders";
			_bordersCommand.Name = "uiCommand4";
			_bordersCommand.Text = "Borders";
			_allCommand.ImageIndex = 0;
			_allCommand.Key = "All";
			_allCommand.Name = "uiCommand5";
			_allCommand.Text = "All";
			_barAppearanceCommand.Key = "Appearance";
			_barAppearanceCommand.Name = "uiCommand6";
			_barBehaviorCommand.Key = "Behavior";
			_barBehaviorCommand.Name = "uiCommand7";
			_barSeparatorCommand.CommandType = CommandType.Separator;
			_barSeparatorCommand.Key = "Separator";
			_barSeparatorCommand.Name = "uiCommand8";
			_barAllCommand.Key = "All";
			_barAllCommand.Name = "uiCommand9";
			_commandManager.AllowCustomize = InheritableBoolean.False;
			_commandManager.BottomRebar = _bottomRebar;
			_commandManager.CommandBars.AddRange(new MxCommandBar[1] { _commandBar });
			_commandManager.Commands.AddRange(new MxCommand[5] { _appearanceCommand, _behaviorCommand, _imagesCommand, _bordersCommand, _allCommand });
			_commandManager.ContainerControl = this;
			_commandManager.Id = new Guid("72e3979f-7d07-4d80-88c8-198113f6b048");
			_commandManager.ImageList = _imageList;
			_commandManager.LeftRebar = _leftRebar;
			_commandManager.RightRebar = _rightRebar;
			_commandManager.TopRebar = _topRebar;
			_bottomRebar.CommandManager = _commandManager;
			_bottomRebar.Dock = DockStyle.Bottom;
			_bottomRebar.Location = new Point(0, 0);
			_bottomRebar.Name = "BottomRebar1";
			_bottomRebar.Size = new Size(0, 0);
			_commandBar.AllowCustomize = InheritableBoolean.False;
			_commandBar.CommandManager = _commandManager;
			_commandBar.Commands.AddRange(new MxCommand[4] { _barAppearanceCommand, _barBehaviorCommand, _barSeparatorCommand, _barAllCommand });
			_commandBar.FullRow = true;
			_commandBar.Key = "CommandBar1";
			_commandBar.Location = new Point(0, 0);
			_commandBar.LockCommandBar = InheritableBoolean.True;
			_commandBar.Name = "uiCommandBar1";
			_commandBar.RowIndex = 0;
			_commandBar.Size = new Size(488, 26);
			_commandBar.Text = "CommandBar1";
			_commandBar.Wrappable = InheritableBoolean.True;
			_commandBar.CommandClick += OnCommandBarCommandClick;
			ImageStrip.Load(_imageList, componentResourceManager, "imageList1.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_imageList.Images.SetKeyName(0, "");
			_leftRebar.CommandManager = _commandManager;
			_leftRebar.Dock = DockStyle.Left;
			_leftRebar.Location = new Point(0, 0);
			_leftRebar.Name = "LeftRebar1";
			_leftRebar.Size = new Size(0, 0);
			_rightRebar.CommandManager = _commandManager;
			_rightRebar.Dock = DockStyle.Right;
			_rightRebar.Location = new Point(0, 0);
			_rightRebar.Name = "RightRebar1";
			_rightRebar.Size = new Size(0, 0);
			_topRebar.CommandBars.AddRange(new MxCommandBar[1] { _commandBar });
			_topRebar.CommandManager = _commandManager;
			_topRebar.Controls.Add(_commandBar);
			_topRebar.Dock = DockStyle.Top;
			_topRebar.Location = new Point(0, 0);
			_topRebar.Name = "uiRebar1";
			_topRebar.Size = new Size(488, 26);
			_dockManager.ContainerControl = this;
			_panelGroup.Id = new Guid("33c7812b-6bac-48a3-ae24-211cce2775a2");
			_propertiesPanel.Id = new Guid("6881cd39-d20d-455c-aee4-80e092ed5555");
			_panelGroup.Panels.Add(_propertiesPanel);
			_helpPanel.Id = new Guid("46bb7849-7016-46b9-9d16-bbd423809d1c");
			_panelGroup.Panels.Add(_helpPanel);
			_dockManager.Panels.Add(_panelGroup);
			_dockManager.BeginPanelInfo();
			_dockManager.AddDockPanelInfo(new Guid("33c7812b-6bac-48a3-ae24-211cce2775a2"), PanelGroupStyle.HorizontalTiles, PanelDockStyle.Fill, isStaticGroup: false, new Size(482, 264), isCurrent: true);
			_dockManager.AddDockPanelInfo(new Guid("6881cd39-d20d-455c-aee4-80e092ed5555"), new Guid("33c7812b-6bac-48a3-ae24-211cce2775a2"), 140, isCurrent: true);
			_dockManager.AddDockPanelInfo(new Guid("46bb7849-7016-46b9-9d16-bbd423809d1c"), new Guid("33c7812b-6bac-48a3-ae24-211cce2775a2"), 120, isCurrent: true);
			_dockManager.AddFloatingPanelInfo(new Guid("6881cd39-d20d-455c-aee4-80e092ed5555"), new Point(-1, -1), new Size(-1, -1), isCurrent: false);
			_dockManager.AddFloatingPanelInfo(new Guid("46bb7849-7016-46b9-9d16-bbd423809d1c"), new Point(-1, -1), new Size(-1, -1), isCurrent: false);
			_dockManager.EndPanelInfo();
			_panelGroup.CaptionVisible = InheritableBoolean.False;
			_panelGroup.Location = new Point(3, 29);
			_panelGroup.Name = "uiPanelGroup1";
			_panelGroup.Size = new Size(482, 264);
			_panelGroup.TabIndex = 6;
			_propertiesPanel.CaptionVisible = InheritableBoolean.False;
			_propertiesPanel.InnerAreaStyle = PanelInnerAreaStyle.Window;
			_propertiesPanel.InnerContainer = _propertiesPanelContainer;
			_propertiesPanel.Location = new Point(0, 0);
			_propertiesPanel.Name = "uiPanel0";
			_propertiesPanel.Size = new Size(482, 140);
			_propertiesPanel.TabIndex = 4;
			_propertiesPanel.Text = "Panel 0";
			_propertiesPanelContainer.Controls.Add(_propertyGrid);
			_propertiesPanelContainer.Location = new Point(1, 1);
			_propertiesPanelContainer.Name = "uiPanel0Container";
			_propertiesPanelContainer.Size = new Size(480, 138);
			_propertiesPanelContainer.TabIndex = 0;
			_propertyGrid.Dock = DockStyle.Fill;
			_propertyGrid.HelpVisible = false;
			_propertyGrid.LineColor = SystemColors.ScrollBar;
			_propertyGrid.Location = new Point(0, 0);
			_propertyGrid.Name = "pp";
			_propertyGrid.PropertySort = PropertySort.NoSort;
			_propertyGrid.Size = new Size(480, 138);
			_propertyGrid.TabIndex = 0;
			_propertyGrid.ToolbarVisible = false;
			_propertyGrid.SelectedGridItemChanged += OnGridSelectedItemChanged;
			_helpPanel.CaptionVisible = InheritableBoolean.False;
			_helpPanel.InnerAreaStyle = PanelInnerAreaStyle.ContainerPanel;
			_helpPanel.InnerContainer = _helpPanelContainer;
			_helpPanel.Location = new Point(0, 144);
			_helpPanel.Name = "uiPanel1";
			_helpPanel.Size = new Size(482, 120);
			_helpPanel.TabIndex = 4;
			_helpPanel.Text = "Panel 1";
			_helpPanelContainer.Controls.Add(_lblHelp);
			_helpPanelContainer.Controls.Add(_lblName);
			_helpPanelContainer.Location = new Point(1, 1);
			_helpPanelContainer.Name = "uiPanel1Container";
			_helpPanelContainer.Size = new Size(480, 118);
			_helpPanelContainer.TabIndex = 0;
			_helpPanelContainer.TabStop = false;
			_lblHelp.BackColor = Color.Transparent;
			_lblHelp.Dock = DockStyle.Fill;
			_lblHelp.Location = new Point(0, 16);
			_lblHelp.Name = "lblHelp";
			_lblHelp.Size = new Size(480, 102);
			_lblHelp.TabIndex = 1;
			_lblName.BackColor = Color.Transparent;
			_lblName.Dock = DockStyle.Top;
			_lblName.Font = new Font("Verdana", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_lblName.Location = new Point(0, 0);
			_lblName.Name = "lblName";
			_lblName.Size = new Size(480, 16);
			_lblName.TabIndex = 0;
			_lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			base.Controls.Add(_panelGroup);
			base.Controls.Add(_topRebar);
			Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "PanelPropertiesControl";
			base.Size = new Size(488, 296);
			((ISupportInitialize)_commandManager).EndInit();
			((ISupportInitialize)_bottomRebar).EndInit();
			((ISupportInitialize)_commandBar).EndInit();
			((ISupportInitialize)_leftRebar).EndInit();
			((ISupportInitialize)_rightRebar).EndInit();
			((ISupportInitialize)_topRebar).EndInit();
			_topRebar.ResumeLayout(performLayout: false);
			((ISupportInitialize)_dockManager).EndInit();
			((ISupportInitialize)_panelGroup).EndInit();
			_panelGroup.ResumeLayout(performLayout: false);
			((ISupportInitialize)_propertiesPanel).EndInit();
			_propertiesPanel.ResumeLayout(performLayout: false);
			_propertiesPanelContainer.ResumeLayout(performLayout: false);
			((ISupportInitialize)_helpPanel).EndInit();
			_helpPanel.ResumeLayout(performLayout: false);
			_helpPanelContainer.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		private void OnCommandBarCommandClick(object sender, CommandEventArgs eventArgs)
		{
			SelectView(eventArgs.Command.Key);
		}

		private void SelectView(string text)
		{
			foreach (MxCommand command in _commandBar.Commands)
			{
				if (command.Key == text)
				{
					command.Checked = InheritableBoolean.True;
				}
				else
				{
					command.Checked = InheritableBoolean.False;
				}
			}
			SetViewMode(text);
		}

		private void OnGridSelectedItemChanged(object sender, SelectedGridItemChangedEventArgs eventArgs)
		{
			if (eventArgs.NewSelection.GridItemType == GridItemType.Property)
			{
				_lblName.Text = eventArgs.NewSelection.PropertyDescriptor.Name;
				_lblHelp.Text = eventArgs.NewSelection.PropertyDescriptor.Description;
			}
			else
			{
				_lblName.Text = "";
				_lblHelp.Text = "";
			}
		}

		private void SetViewMode(string text)
		{
			_viewMode = text;
			_propertyGrid.SelectedObject = null;
			if (_viewMode == "Appearance")
			{
				_propertyGrid.PropertySort = PropertySort.Categorized;
			}
			else if (_viewMode == "All")
			{
				_propertyGrid.PropertySort = PropertySort.Alphabetical;
			}
			else
			{
				try
				{
					_propertyGrid.PropertySort = PropertySort.NoSort;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			}
			RefreshPropertyGrid();
		}

		private void RefreshPropertyGrid()
		{
			if (_targetPanel != null)
			{
				_propertyGrid.SelectedObject = new DockPanelPropertyFilter(_targetPanel, _viewMode);
				if (_propertyGrid.PropertySort == PropertySort.Categorized)
				{
					_propertyGrid.CollapseAllGridItems();
				}
			}
			else
			{
				_propertyGrid.SelectedObject = null;
			}
		}

		internal void SetTargetPanel(DockPanelBase uiPanelBase)
		{
			if (_targetPanel != uiPanelBase)
			{
				_targetPanel = uiPanelBase;
				SelectView("Appearance");
			}
		}
	}
}
