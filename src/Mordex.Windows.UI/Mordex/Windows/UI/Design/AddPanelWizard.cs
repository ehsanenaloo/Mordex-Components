using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	public class AddPanelWizard : Form
	{
		private const int FullStepCount = 3;

		private const int ExistingPanelStepCount = 2;

		private const string GeneratedPanelNamePrefix = "uiPanel";

		private Button _btnOk;

		private Button _cmdNext;

		private IContainer _components;

		private int _currentStep;

		private Panel _propertiesStep;

		private Button _cmdPrevious;

		private Label _dockStateLabel;

		private Label _lblDockStyle;

		private RadioButton _optDockInContainer;

		private Panel _settingsStep;

		private RadioButton _optAddToGroup;

		private Label _panelGroupsLabel;

		private MxComboBox _cboPanelGroups;

		private Label _panelsLabel;

		private MxComboBox _cboPanels;

		private ImageList _iconImages;

		private RadioButton _optGroupWithPanel;

		private MxComboBox _cboGroupStyle;

		private GroupBox _grpGroupProperties;

		private Label _lblGroupStyle;

		private CheckBox _chkStaticGroup;

		private Panel _panelContainer;

		private CommandEditorControl _panelPropertiesControl;

		private TextBox _txtText;

		private Label _lblText;

		private Panel _typeStep;

		private RadioButton _optCreatePanel;

		private Label _staticGroupStyleLabel;

		private TextBox _txtName;

		private Label _lblName;

		private Label _typeHeaderLabel;

		private Label _createPanelHintLabel;

		private Label _createStaticGroupHintLabel;

		private MxComboBox _cboStaticGroupStyle;

		private RadioButton _optCreateStaticGroup;

		private Control[] _stepPanels;

		private MxComboBox _cboDockState;

		private MxComboBox _cboDockStyle;

		private Label _panelDockingHintLabel;

		private Label _staticGroupHintLabel;

		private PictureBox _createPanelPicture;

		private PictureBox _createStaticGroupPicture;

		private Label _propertiesHeaderLabel;

		private Label _settingsHeaderLabel;

		private GroupBox _addToGroupGroupBox;

		private GroupBox _dockInContainerGroupBox;

		private GroupBox _previewGroupBox;

		private GroupBox _propertiesGroupBox;

		private Label _dockInContainerHintLabel;

		private Label _groupWithPanelHintLabel;

		private Label _addToGroupHintLabel;

		private PictureBox _addToGroupPicture;

		private PictureBox _dockInContainerPicture;

		private PictureBox _groupWithPanelPicture;

		private Button _btnCancel;

		private int _stepCount;

		private MxComboBox _cboExistingPanels;

		private RadioButton _optExistingPanel;

		private PictureBox _existingPanelPicture;

		private bool _isStaticGroup;

		private DockPanelBase _samplePanel;

		private MxDockManager _previewManager;

		private MxDockManager _targetManager;

		private bool _shouldRegisterPanel = true;

		public AddPanelWizard()
		{
			InitializeControls();
			_currentStep = 0;
			_stepPanels = new Control[3];
			_stepPanels[0] = _typeStep;
			_stepPanels[1] = _propertiesStep;
			_stepPanels[2] = _settingsStep;
			_stepCount = FullStepCount;
		}

		private void SetStaticGroupMode(bool isStaticGroup)
		{
			if (_isStaticGroup != isStaticGroup)
			{
				_isStaticGroup = isStaticGroup;
				RebuildSamplePanel();
			}
		}

		private void RebuildSamplePanel()
		{
			_previewManager.Panels.Clear();
			if (!_isStaticGroup)
			{
				_samplePanel = new MxDockPanel();
			}
			else
			{
				_samplePanel = new MxDockPanelGroup((PanelGroupStyle)_cboStaticGroupStyle.SelectedItem.Value);
				((MxDockPanelGroup)_samplePanel).StaticGroup = true;
				((MxDockPanelGroup)_samplePanel).Panels.Add(new MxDockPanel("Sample Child Panel"));
				((MxDockPanelGroup)_samplePanel).Panels.Add(new MxDockPanel("Sample Child Panel"));
			}
			_samplePanel.Text = _txtText.Text;
			_previewManager.Panels.Add(_samplePanel);
			_panelPropertiesControl.SetTargetPanel(_samplePanel);
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(AddPanelWizard));
			_btnOk = new Button();
			_cmdNext = new Button();
			_iconImages = new ImageList(_components);
			_propertiesStep = new Panel();
			_propertiesGroupBox = new GroupBox();
			_txtName = new TextBox();
			_lblText = new Label();
			_lblName = new Label();
			_txtText = new TextBox();
			_panelPropertiesControl = new CommandEditorControl();
			_previewGroupBox = new GroupBox();
			_panelContainer = new Panel();
			_propertiesHeaderLabel = new Label();
			_dockStateLabel = new Label();
			_lblDockStyle = new Label();
			_cmdPrevious = new Button();
			_optDockInContainer = new RadioButton();
			_settingsStep = new Panel();
			_dockInContainerGroupBox = new GroupBox();
			_cboDockState = new MxComboBox();
			_cboDockStyle = new MxComboBox();
			_dockInContainerHintLabel = new Label();
			_dockInContainerPicture = new PictureBox();
			_optAddToGroup = new RadioButton();
			_addToGroupGroupBox = new GroupBox();
			_panelGroupsLabel = new Label();
			_cboPanelGroups = new MxComboBox();
			_addToGroupHintLabel = new Label();
			_addToGroupPicture = new PictureBox();
			_optGroupWithPanel = new RadioButton();
			_settingsHeaderLabel = new Label();
			_grpGroupProperties = new GroupBox();
			_groupWithPanelPicture = new PictureBox();
			_groupWithPanelHintLabel = new Label();
			_chkStaticGroup = new CheckBox();
			_lblGroupStyle = new Label();
			_cboGroupStyle = new MxComboBox();
			_cboPanels = new MxComboBox();
			_panelsLabel = new Label();
			_typeStep = new Panel();
			_cboExistingPanels = new MxComboBox();
			_existingPanelPicture = new PictureBox();
			_optExistingPanel = new RadioButton();
			_createStaticGroupPicture = new PictureBox();
			_createPanelPicture = new PictureBox();
			_staticGroupHintLabel = new Label();
			_panelDockingHintLabel = new Label();
			_createStaticGroupHintLabel = new Label();
			_createPanelHintLabel = new Label();
			_typeHeaderLabel = new Label();
			_staticGroupStyleLabel = new Label();
			_cboStaticGroupStyle = new MxComboBox();
			_optCreateStaticGroup = new RadioButton();
			_optCreatePanel = new RadioButton();
			_btnCancel = new Button();
			_propertiesStep.SuspendLayout();
			_propertiesGroupBox.SuspendLayout();
			_previewGroupBox.SuspendLayout();
			_settingsStep.SuspendLayout();
			_dockInContainerGroupBox.SuspendLayout();
			((ISupportInitialize)_dockInContainerPicture).BeginInit();
			_addToGroupGroupBox.SuspendLayout();
			((ISupportInitialize)_addToGroupPicture).BeginInit();
			_grpGroupProperties.SuspendLayout();
			((ISupportInitialize)_groupWithPanelPicture).BeginInit();
			_typeStep.SuspendLayout();
			((ISupportInitialize)_existingPanelPicture).BeginInit();
			((ISupportInitialize)_createStaticGroupPicture).BeginInit();
			((ISupportInitialize)_createPanelPicture).BeginInit();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOk.DialogResult = DialogResult.OK;
			_btnOk.Location = new Point(419, 348);
			_btnOk.Name = "btnOk";
			_btnOk.Size = new Size(80, 24);
			_btnOk.TabIndex = 2;
			_btnOk.Text = "OK";
			_btnOk.Click += OnBtnOkClick;
			_cmdNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_cmdNext.ImageAlign = ContentAlignment.MiddleRight;
			_cmdNext.ImageIndex = 2;
			_cmdNext.ImageList = _iconImages;
			_cmdNext.Location = new Point(327, 348);
			_cmdNext.Name = "cmdNext";
			_cmdNext.Size = new Size(80, 24);
			_cmdNext.TabIndex = 6;
			_cmdNext.Text = "   Next";
			_cmdNext.TextAlign = ContentAlignment.MiddleLeft;
			_cmdNext.Click += OnCmdNextClick;
			ImageStrip.Load(_iconImages, componentResourceManager, "imageList1.ImageStrip");
			_iconImages.TransparentColor = Color.Transparent;
			_iconImages.Images.SetKeyName(0, "");
			_iconImages.Images.SetKeyName(1, "");
			_iconImages.Images.SetKeyName(2, "");
			_iconImages.Images.SetKeyName(3, "");
			_propertiesStep.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_propertiesStep.Controls.Add(_propertiesGroupBox);
			_propertiesStep.Controls.Add(_previewGroupBox);
			_propertiesStep.Controls.Add(_propertiesHeaderLabel);
			_propertiesStep.Location = new Point(0, 0);
			_propertiesStep.Name = "step2";
			_propertiesStep.Size = new Size(586, 340);
			_propertiesStep.TabIndex = 7;
			_propertiesStep.Visible = false;
			_propertiesGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_propertiesGroupBox.Controls.Add(_txtName);
			_propertiesGroupBox.Controls.Add(_lblText);
			_propertiesGroupBox.Controls.Add(_lblName);
			_propertiesGroupBox.Controls.Add(_txtText);
			_propertiesGroupBox.Controls.Add(_panelPropertiesControl);
			_propertiesGroupBox.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_propertiesGroupBox.Location = new Point(10, 40);
			_propertiesGroupBox.Name = "groupBox4";
			_propertiesGroupBox.Size = new Size(340, 290);
			_propertiesGroupBox.TabIndex = 26;
			_propertiesGroupBox.TabStop = false;
			_propertiesGroupBox.Text = "Properties:";
			_txtName.Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_txtName.Location = new Point(61, 22);
			_txtName.Name = "txtName";
			_txtName.Size = new Size(110, 21);
			_txtName.TabIndex = 0;
			_lblText.Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_lblText.Location = new Point(181, 25);
			_lblText.Name = "lblText";
			_lblText.Size = new Size(35, 13);
			_lblText.TabIndex = 4;
			_lblText.Text = "Text:";
			_lblName.Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_lblName.Location = new Point(13, 24);
			_lblName.Name = "lblName";
			_lblName.Size = new Size(43, 15);
			_lblName.TabIndex = 1;
			_lblName.Text = "Name:";
			_txtText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_txtText.Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_txtText.Location = new Point(220, 22);
			_txtText.Name = "txtText";
			_txtText.Size = new Size(110, 21);
			_txtText.TabIndex = 3;
			_txtText.TextChanged += OnTxtTextTextChanged;
			_panelPropertiesControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_panelPropertiesControl.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_panelPropertiesControl.Location = new Point(9, 52);
			_panelPropertiesControl.Name = "panelPropertiesControl1";
			_panelPropertiesControl.Size = new Size(324, 230);
			_panelPropertiesControl.TabIndex = 15;
			_previewGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			_previewGroupBox.Controls.Add(_panelContainer);
			_previewGroupBox.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_previewGroupBox.Location = new Point(358, 40);
			_previewGroupBox.Name = "groupBox3";
			_previewGroupBox.Size = new Size(228, 290);
			_previewGroupBox.TabIndex = 25;
			_previewGroupBox.TabStop = false;
			_previewGroupBox.Text = "Preview:";
			_panelContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			_panelContainer.BackColor = SystemColors.AppWorkspace;
			_panelContainer.Location = new Point(8, 17);
			_panelContainer.Name = "panelContainer";
			_panelContainer.Size = new Size(212, 262);
			_panelContainer.TabIndex = 13;
			_propertiesHeaderLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_propertiesHeaderLabel.BackColor = SystemColors.ControlDark;
			_propertiesHeaderLabel.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_propertiesHeaderLabel.ForeColor = SystemColors.ControlLightLight;
			_propertiesHeaderLabel.ImageAlign = ContentAlignment.MiddleLeft;
			_propertiesHeaderLabel.Location = new Point(8, 8);
			_propertiesHeaderLabel.Name = "label12";
			_propertiesHeaderLabel.Size = new Size(572, 25);
			_propertiesHeaderLabel.TabIndex = 24;
			_propertiesHeaderLabel.Text = "  Panel Properties:";
			_propertiesHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
			_dockStateLabel.Location = new Point(23, 45);
			_dockStateLabel.Name = "label4";
			_dockStateLabel.Size = new Size(63, 13);
			_dockStateLabel.TabIndex = 10;
			_dockStateLabel.Text = "DockState:";
			_lblDockStyle.Location = new Point(281, 45);
			_lblDockStyle.Name = "lblDockStyle";
			_lblDockStyle.Size = new Size(72, 15);
			_lblDockStyle.TabIndex = 8;
			_lblDockStyle.Text = "DockStyle:";
			_cmdPrevious.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_cmdPrevious.ImageAlign = ContentAlignment.MiddleLeft;
			_cmdPrevious.ImageIndex = 3;
			_cmdPrevious.ImageList = _iconImages;
			_cmdPrevious.Location = new Point(244, 348);
			_cmdPrevious.Name = "cmdPrevious";
			_cmdPrevious.Size = new Size(80, 24);
			_cmdPrevious.TabIndex = 8;
			_cmdPrevious.Text = "   Previous";
			_cmdPrevious.TextAlign = ContentAlignment.MiddleRight;
			_cmdPrevious.Click += OnCmdPreviousClick;
			_optDockInContainer.Location = new Point(43, 43);
			_optDockInContainer.Name = "optParent1";
			_optDockInContainer.Size = new Size(77, 17);
			_optDockInContainer.TabIndex = 13;
			_optDockInContainer.Text = "Root Panel";
			_optDockInContainer.CheckedChanged += OnOptDockInContainerCheckedChanged;
			_settingsStep.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_settingsStep.Controls.Add(_optDockInContainer);
			_settingsStep.Controls.Add(_dockInContainerGroupBox);
			_settingsStep.Controls.Add(_optAddToGroup);
			_settingsStep.Controls.Add(_addToGroupGroupBox);
			_settingsStep.Controls.Add(_optGroupWithPanel);
			_settingsStep.Controls.Add(_settingsHeaderLabel);
			_settingsStep.Controls.Add(_grpGroupProperties);
			_settingsStep.Location = new Point(0, 0);
			_settingsStep.Name = "step3";
			_settingsStep.Size = new Size(586, 340);
			_settingsStep.TabIndex = 9;
			_settingsStep.Visible = false;
			_dockInContainerGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_dockInContainerGroupBox.Controls.Add(_dockStateLabel);
			_dockInContainerGroupBox.Controls.Add(_cboDockState);
			_dockInContainerGroupBox.Controls.Add(_cboDockStyle);
			_dockInContainerGroupBox.Controls.Add(_lblDockStyle);
			_dockInContainerGroupBox.Controls.Add(_dockInContainerHintLabel);
			_dockInContainerGroupBox.Controls.Add(_dockInContainerPicture);
			_dockInContainerGroupBox.Location = new Point(29, 45);
			_dockInContainerGroupBox.Name = "groupBox2";
			_dockInContainerGroupBox.Size = new Size(530, 76);
			_dockInContainerGroupBox.TabIndex = 28;
			_dockInContainerGroupBox.TabStop = false;
			_cboDockState.ComboStyle = ComboStyle.DropDownList;
			_cboDockState.DropListFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_cboDockState.Enabled = false;
			_cboDockState.ImageList = _iconImages;
			_cboDockState.Location = new Point(115, 43);
			_cboDockState.Name = "cboDockState";
			_cboDockState.Size = new Size(144, 22);
			_cboDockState.TabIndex = 24;
			_cboDockState.SelectedItemChanged += OnCboDockStateSelectedItemChanged;
			_cboDockStyle.ComboStyle = ComboStyle.DropDownList;
			_cboDockStyle.DropListFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_cboDockStyle.Enabled = false;
			_cboDockStyle.ImageList = _iconImages;
			_cboDockStyle.Location = new Point(351, 43);
			_cboDockStyle.Name = "cboDockStyle";
			_cboDockStyle.Size = new Size(144, 22);
			_cboDockStyle.TabIndex = 25;
			_cboDockStyle.SelectedItemChanged += OnCboDockStyleSelectedItemChanged;
			_dockInContainerHintLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Italic, GraphicsUnit.Point, 0);
			_dockInContainerHintLabel.Location = new Point(115, 18);
			_dockInContainerHintLabel.Name = "label2";
			_dockInContainerHintLabel.Size = new Size(391, 12);
			_dockInContainerHintLabel.TabIndex = 29;
			_dockInContainerHintLabel.Text = "An independent Panel that will be located directly in the Container.";
			_dockInContainerPicture.Image = (Image)componentResourceManager.GetObject("pictureBox4.Image");
			_dockInContainerPicture.Location = new Point(43, 19);
			_dockInContainerPicture.Name = "pictureBox4";
			_dockInContainerPicture.Size = new Size(16, 16);
			_dockInContainerPicture.TabIndex = 30;
			_dockInContainerPicture.TabStop = false;
			_optAddToGroup.Location = new Point(43, 135);
			_optAddToGroup.Name = "optParent2";
			_optAddToGroup.Size = new Size(159, 17);
			_optAddToGroup.TabIndex = 14;
			_optAddToGroup.Text = "Add to Existing PanelGroup";
			_optAddToGroup.CheckedChanged += OnOptAddToGroupCheckedChanged;
			_addToGroupGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_addToGroupGroupBox.Controls.Add(_panelGroupsLabel);
			_addToGroupGroupBox.Controls.Add(_cboPanelGroups);
			_addToGroupGroupBox.Controls.Add(_addToGroupHintLabel);
			_addToGroupGroupBox.Controls.Add(_addToGroupPicture);
			_addToGroupGroupBox.Location = new Point(29, 134);
			_addToGroupGroupBox.Name = "groupBox1";
			_addToGroupGroupBox.Size = new Size(530, 76);
			_addToGroupGroupBox.TabIndex = 27;
			_addToGroupGroupBox.TabStop = false;
			_panelGroupsLabel.Location = new Point(23, 50);
			_panelGroupsLabel.Name = "label1";
			_panelGroupsLabel.Size = new Size(73, 16);
			_panelGroupsLabel.TabIndex = 17;
			_panelGroupsLabel.Text = "PanelGroups:";
			_cboPanelGroups.ComboStyle = ComboStyle.DropDownList;
			_cboPanelGroups.DropListFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_cboPanelGroups.Enabled = false;
			_cboPanelGroups.ImageList = _iconImages;
			_cboPanelGroups.Location = new Point(115, 46);
			_cboPanelGroups.Name = "cboPanelGroups";
			_cboPanelGroups.Size = new Size(256, 22);
			_cboPanelGroups.TabIndex = 15;
			_addToGroupHintLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Italic, GraphicsUnit.Point, 0);
			_addToGroupHintLabel.Location = new Point(115, 21);
			_addToGroupHintLabel.Name = "label14";
			_addToGroupHintLabel.Size = new Size(313, 12);
			_addToGroupHintLabel.TabIndex = 31;
			_addToGroupHintLabel.Text = "This Panel will be added to an existing PanelGroup.";
			_addToGroupPicture.Image = (Image)componentResourceManager.GetObject("pictureBox3.Image");
			_addToGroupPicture.Location = new Point(21, 20);
			_addToGroupPicture.Name = "pictureBox3";
			_addToGroupPicture.Size = new Size(88, 24);
			_addToGroupPicture.SizeMode = PictureBoxSizeMode.AutoSize;
			_addToGroupPicture.TabIndex = 31;
			_addToGroupPicture.TabStop = false;
			_optGroupWithPanel.Location = new Point(43, 225);
			_optGroupWithPanel.Name = "optParent3";
			_optGroupWithPanel.Size = new Size(148, 16);
			_optGroupWithPanel.TabIndex = 16;
			_optGroupWithPanel.Text = " Create PanelGroup with:";
			_optGroupWithPanel.CheckedChanged += OnOptGroupWithPanelCheckedChanged;
			_settingsHeaderLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_settingsHeaderLabel.BackColor = SystemColors.ControlDark;
			_settingsHeaderLabel.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_settingsHeaderLabel.ForeColor = SystemColors.ControlLightLight;
			_settingsHeaderLabel.ImageAlign = ContentAlignment.MiddleLeft;
			_settingsHeaderLabel.Location = new Point(8, 8);
			_settingsHeaderLabel.Name = "label13";
			_settingsHeaderLabel.Size = new Size(572, 25);
			_settingsHeaderLabel.TabIndex = 26;
			_settingsHeaderLabel.Text = "  Panel Settings:";
			_settingsHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
			_grpGroupProperties.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_grpGroupProperties.Controls.Add(_groupWithPanelPicture);
			_grpGroupProperties.Controls.Add(_groupWithPanelHintLabel);
			_grpGroupProperties.Controls.Add(_chkStaticGroup);
			_grpGroupProperties.Controls.Add(_lblGroupStyle);
			_grpGroupProperties.Controls.Add(_cboGroupStyle);
			_grpGroupProperties.Controls.Add(_cboPanels);
			_grpGroupProperties.Controls.Add(_panelsLabel);
			_grpGroupProperties.Enabled = false;
			_grpGroupProperties.Location = new Point(29, 223);
			_grpGroupProperties.Name = "grpGroupProperties";
			_grpGroupProperties.Size = new Size(530, 108);
			_grpGroupProperties.TabIndex = 23;
			_grpGroupProperties.TabStop = false;
			_groupWithPanelPicture.Image = (Image)componentResourceManager.GetObject("pictureBox5.Image");
			_groupWithPanelPicture.Location = new Point(21, 20);
			_groupWithPanelPicture.Name = "pictureBox5";
			_groupWithPanelPicture.Size = new Size(88, 24);
			_groupWithPanelPicture.TabIndex = 32;
			_groupWithPanelPicture.TabStop = false;
			_groupWithPanelHintLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Italic, GraphicsUnit.Point, 0);
			_groupWithPanelHintLabel.Location = new Point(113, 26);
			_groupWithPanelHintLabel.Name = "label5";
			_groupWithPanelHintLabel.Size = new Size(371, 12);
			_groupWithPanelHintLabel.TabIndex = 30;
			_groupWithPanelHintLabel.Text = "This Panel will create a New PanelGroup with the Panel selected below.";
			_chkStaticGroup.CheckAlign = ContentAlignment.MiddleRight;
			_chkStaticGroup.Location = new Point(416, 74);
			_chkStaticGroup.Name = "checkBox1";
			_chkStaticGroup.Size = new Size(92, 24);
			_chkStaticGroup.TabIndex = 24;
			_chkStaticGroup.Text = "Static Group";
			_lblGroupStyle.Location = new Point(22, 81);
			_lblGroupStyle.Name = "lblGroupStyle";
			_lblGroupStyle.Size = new Size(72, 15);
			_lblGroupStyle.TabIndex = 23;
			_lblGroupStyle.Text = "GroupStyle:";
			_cboGroupStyle.ComboStyle = ComboStyle.DropDownList;
			_cboGroupStyle.DropListFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_cboGroupStyle.Enabled = false;
			_cboGroupStyle.Location = new Point(113, 77);
			_cboGroupStyle.Name = "cboGroupStyle";
			_cboGroupStyle.Size = new Size(256, 21);
			_cboGroupStyle.TabIndex = 20;
			_cboPanels.ComboStyle = ComboStyle.DropDownList;
			_cboPanels.DropListFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_cboPanels.Enabled = false;
			_cboPanels.Location = new Point(113, 51);
			_cboPanels.Name = "cboPanels";
			_cboPanels.Size = new Size(256, 21);
			_cboPanels.TabIndex = 18;
			_panelsLabel.Location = new Point(22, 55);
			_panelsLabel.Name = "label3";
			_panelsLabel.Size = new Size(88, 16);
			_panelsLabel.TabIndex = 19;
			_panelsLabel.Text = "Panel:";
			_typeStep.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_typeStep.Controls.Add(_cboExistingPanels);
			_typeStep.Controls.Add(_existingPanelPicture);
			_typeStep.Controls.Add(_optExistingPanel);
			_typeStep.Controls.Add(_createStaticGroupPicture);
			_typeStep.Controls.Add(_createPanelPicture);
			_typeStep.Controls.Add(_staticGroupHintLabel);
			_typeStep.Controls.Add(_panelDockingHintLabel);
			_typeStep.Controls.Add(_createStaticGroupHintLabel);
			_typeStep.Controls.Add(_createPanelHintLabel);
			_typeStep.Controls.Add(_typeHeaderLabel);
			_typeStep.Controls.Add(_staticGroupStyleLabel);
			_typeStep.Controls.Add(_cboStaticGroupStyle);
			_typeStep.Controls.Add(_optCreateStaticGroup);
			_typeStep.Controls.Add(_optCreatePanel);
			_typeStep.Location = new Point(0, 0);
			_typeStep.Name = "step1";
			_typeStep.Size = new Size(586, 340);
			_typeStep.TabIndex = 10;
			_cboExistingPanels.ComboStyle = ComboStyle.DropDownList;
			_cboExistingPanels.DropListFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_cboExistingPanels.Enabled = false;
			_cboExistingPanels.Location = new Point(193, 291);
			_cboExistingPanels.Name = "cboExistingPanels";
			_cboExistingPanels.Size = new Size(286, 21);
			_cboExistingPanels.TabIndex = 32;
			_cboExistingPanels.Visible = false;
			_existingPanelPicture.Image = (Image)componentResourceManager.GetObject("imgExistingPanel.Image");
			_existingPanelPicture.Location = new Point(43, 291);
			_existingPanelPicture.Name = "imgExistingPanel";
			_existingPanelPicture.Size = new Size(16, 16);
			_existingPanelPicture.TabIndex = 31;
			_existingPanelPicture.TabStop = false;
			_existingPanelPicture.Visible = false;
			_optExistingPanel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_optExistingPanel.Location = new Point(23, 288);
			_optExistingPanel.Name = "optExistingPanel";
			_optExistingPanel.Size = new Size(237, 24);
			_optExistingPanel.TabIndex = 30;
			_optExistingPanel.Text = "        Use Existing Panel:";
			_optExistingPanel.Visible = false;
			_optExistingPanel.CheckedChanged += OnOptExistingPanelCheckedChanged;
			_createStaticGroupPicture.Image = (Image)componentResourceManager.GetObject("pictureBox2.Image");
			_createStaticGroupPicture.Location = new Point(40, 163);
			_createStaticGroupPicture.Name = "pictureBox2";
			_createStaticGroupPicture.Size = new Size(16, 16);
			_createStaticGroupPicture.TabIndex = 29;
			_createStaticGroupPicture.TabStop = false;
			_createPanelPicture.Image = (Image)componentResourceManager.GetObject("pictureBox1.Image");
			_createPanelPicture.Location = new Point(40, 63);
			_createPanelPicture.Name = "pictureBox1";
			_createPanelPicture.Size = new Size(16, 16);
			_createPanelPicture.TabIndex = 28;
			_createPanelPicture.TabStop = false;
			_staticGroupHintLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Italic, GraphicsUnit.Point, 0);
			_staticGroupHintLabel.Location = new Point(67, 244);
			_staticGroupHintLabel.Name = "label11";
			_staticGroupHintLabel.Size = new Size(507, 48);
			_staticGroupHintLabel.TabIndex = 27;
			_staticGroupHintLabel.Text = "Static Groups work as a Single Panel but instead of hosting controls they will contain other panels.              After creating the Static Group you can add single panels to it.";
			_panelDockingHintLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Italic, GraphicsUnit.Point, 0);
			_panelDockingHintLabel.Location = new Point(67, 110);
			_panelDockingHintLabel.Name = "label10";
			_panelDockingHintLabel.Size = new Size(476, 30);
			_panelDockingHintLabel.TabIndex = 26;
			_panelDockingHintLabel.Text = "Panels can be docked to the ContainerControl, be part of a PanelGroup or float in a separate window.";
			_createStaticGroupHintLabel.Location = new Point(67, 222);
			_createStaticGroupHintLabel.Name = "label9";
			_createStaticGroupHintLabel.Size = new Size(507, 22);
			_createStaticGroupHintLabel.TabIndex = 25;
			_createStaticGroupHintLabel.Text = "Select this option when you want to create a group of panels that can not be modified by the user. ";
			_createPanelHintLabel.Location = new Point(67, 91);
			_createPanelHintLabel.Name = "label8";
			_createPanelHintLabel.Size = new Size(483, 17);
			_createPanelHintLabel.TabIndex = 24;
			_createPanelHintLabel.Text = "Select this option when you want to create a panel that will contain other controls in it. ";
			_typeHeaderLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_typeHeaderLabel.BackColor = SystemColors.ControlDark;
			_typeHeaderLabel.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_typeHeaderLabel.ForeColor = SystemColors.ControlLightLight;
			_typeHeaderLabel.ImageAlign = ContentAlignment.MiddleLeft;
			_typeHeaderLabel.Location = new Point(8, 8);
			_typeHeaderLabel.Name = "label7";
			_typeHeaderLabel.Size = new Size(572, 25);
			_typeHeaderLabel.TabIndex = 23;
			_typeHeaderLabel.Text = "  Select the type of panel you want to create:";
			_typeHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
			_staticGroupStyleLabel.Location = new Point(69, 185);
			_staticGroupStyleLabel.Name = "label6";
			_staticGroupStyleLabel.Size = new Size(78, 24);
			_staticGroupStyleLabel.TabIndex = 22;
			_staticGroupStyleLabel.Text = "GroupStyle:";
			_staticGroupStyleLabel.TextAlign = ContentAlignment.MiddleLeft;
			_cboStaticGroupStyle.ComboStyle = ComboStyle.DropDownList;
			_cboStaticGroupStyle.DropListFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_cboStaticGroupStyle.Enabled = false;
			_cboStaticGroupStyle.Location = new Point(154, 188);
			_cboStaticGroupStyle.Name = "cboStaticGroupStyle";
			_cboStaticGroupStyle.Size = new Size(184, 21);
			_cboStaticGroupStyle.TabIndex = 21;
			_optCreateStaticGroup.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_optCreateStaticGroup.Location = new Point(21, 158);
			_optCreateStaticGroup.Name = "optCreateStaticGroup";
			_optCreateStaticGroup.Size = new Size(283, 24);
			_optCreateStaticGroup.TabIndex = 1;
			_optCreateStaticGroup.Text = "        Create Static Group...";
			_optCreateStaticGroup.CheckedChanged += OnOptCreateStaticGroupCheckedChanged;
			_optCreatePanel.Checked = true;
			_optCreatePanel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_optCreatePanel.Location = new Point(21, 61);
			_optCreatePanel.Name = "optCreatePanel";
			_optCreatePanel.Size = new Size(160, 24);
			_optCreatePanel.TabIndex = 0;
			_optCreatePanel.TabStop = true;
			_optCreatePanel.Text = "        Create Panel...";
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(504, 348);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 11;
			_btnCancel.Text = "Cancel";
			base.AcceptButton = _btnOk;
			AutoScaleBaseSize = new Size(5, 14);
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(592, 380);
			base.Controls.Add(_btnCancel);
			base.Controls.Add(_cmdPrevious);
			base.Controls.Add(_cmdNext);
			base.Controls.Add(_btnOk);
			base.Controls.Add(_typeStep);
			base.Controls.Add(_propertiesStep);
			base.Controls.Add(_settingsStep);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "AddPanelWizard";
			base.ShowInTaskbar = false;
			base.SizeGripStyle = SizeGripStyle.Hide;
			Text = "Add Panel Wizard";
			base.Load += OnControlLoad;
			_propertiesStep.ResumeLayout(performLayout: false);
			_propertiesGroupBox.ResumeLayout(performLayout: false);
			_propertiesGroupBox.PerformLayout();
			_previewGroupBox.ResumeLayout(performLayout: false);
			_settingsStep.ResumeLayout(performLayout: false);
			_dockInContainerGroupBox.ResumeLayout(performLayout: false);
			((ISupportInitialize)_dockInContainerPicture).EndInit();
			_addToGroupGroupBox.ResumeLayout(performLayout: false);
			_addToGroupGroupBox.PerformLayout();
			((ISupportInitialize)_addToGroupPicture).EndInit();
			_grpGroupProperties.ResumeLayout(performLayout: false);
			((ISupportInitialize)_groupWithPanelPicture).EndInit();
			_typeStep.ResumeLayout(performLayout: false);
			((ISupportInitialize)_existingPanelPicture).EndInit();
			((ISupportInitialize)_createStaticGroupPicture).EndInit();
			((ISupportInitialize)_createPanelPicture).EndInit();
			ResumeLayout(performLayout: false);
		}

		private void OnBtnOkClick(object sender, EventArgs eventArgs)
		{
			DialogResult dialogResult = DialogResult.OK;
			if (_optExistingPanel.Checked)
			{
				if (_cboExistingPanels.SelectedItem == null)
				{
					MessageBox.Show("Select the panel you want to add to the panels collection.");
					RenderingHelper.TryFocusControl(_cboExistingPanels);
					dialogResult = DialogResult.None;
				}
			}
			else
			{
				if (_currentStep == 0)
				{
					GoToStep(1);
				}
				if (!IsPanelNameAvailable(_targetManager, _txtName.Text))
				{
					MessageBox.Show("Name: '" + _txtName.Text + "' is already assigned.");
					RenderingHelper.TryFocusControl(_txtName);
					dialogResult = DialogResult.None;
				}
				if (_optAddToGroup.Checked)
				{
					if (_cboPanelGroups.SelectedItem == null)
					{
						MessageBox.Show("Select the PanelGroup to which the new panel will be added.");
						RenderingHelper.TryFocusControl(_cboPanelGroups);
						dialogResult = DialogResult.None;
					}
				}
				else if (_optGroupWithPanel.Checked && _cboPanels.SelectedItem == null)
				{
					MessageBox.Show("Select the Panel to group the new panel with.");
					_cboPanels.Focus();
					dialogResult = DialogResult.None;
				}
			}
			base.DialogResult = dialogResult;
		}

		public DockPanelBase CreatePanel(MxDockManager panelManager)
		{
			return RunWizard(panelManager, registerPanel: true);
		}

		internal DockPanelBase RunWizard(MxDockManager panelManager, bool registerPanel)
		{
			_targetManager = panelManager;
			_previewManager = new MxDockManager();
			_previewManager.ImageList = _targetManager.ImageList;
			_previewManager.LoadLayout(_targetManager.GetLayout());
			_previewManager.AllowPanelDrag = false;
			_previewManager.AllowPanelDrop = false;
			_previewManager.Panels.Clear();
			_shouldRegisterPanel = registerPanel;
			if (!registerPanel)
			{
				_stepCount = ExistingPanelStepCount;
				_optExistingPanel.Visible = true;
				_cboExistingPanels.Visible = true;
				_existingPanelPicture.Visible = true;
				FillExistingPanelList();
			}
			_previewManager.ContainerControl = _panelContainer;
			RebuildSamplePanel();
			if (ShowDialog() == DialogResult.OK)
			{
				return BuildResultPanel();
			}
			return null;
		}

		private bool IsPanelNameAvailable(MxDockManager uiPanelManager, string name)
		{
			INameCreationService nameCreationService = (INameCreationService)uiPanelManager.Site.GetService(typeof(INameCreationService));
			if (nameCreationService != null)
			{
				try
				{
					if (!nameCreationService.IsValidName(name))
					{
						return false;
					}
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// The name service belongs to the design host. If it fails, the duplicate check below still runs.
				}
			}
			foreach (DockPanelBase item in uiPanelManager.GetAllPanels())
			{
				if (string.Compare(item.Name, name, ignoreCase: true) == 0)
				{
					return false;
				}
			}
			return true;
		}

		private DockPanelBase BuildResultPanel()
		{
			try
			{
				if (_optExistingPanel.Checked)
				{
					return (DockPanelBase)_cboExistingPanels.SelectedItem.Value;
				}
				if (_samplePanel is MxDockPanelGroup)
				{
					((MxDockPanelGroup)_samplePanel).Panels.Clear();
				}
				DockPanelBase uiPanelBase = _samplePanel;
				uiPanelBase.Name = _txtName.Text;
				if (_shouldRegisterPanel)
				{
					if (_targetManager.Container != null && uiPanelBase.Container == null)
					{
						_targetManager.Container.Add(uiPanelBase, uiPanelBase.Name);
						if (uiPanelBase is MxDockPanel)
						{
							_targetManager.Container.Add(((MxDockPanel)uiPanelBase).InnerContainer, uiPanelBase.Name + "Container");
						}
					}
					if (uiPanelBase.ContainerCaptionControl != null && uiPanelBase.ContainerCaptionControl.Container == null)
					{
						_targetManager.Container.Add(uiPanelBase.ContainerCaptionControl, uiPanelBase.Name + "CaptionContainer");
					}
				}
				if (_optDockInContainer.Checked)
				{
					uiPanelBase.DockState = (PanelDockState)_cboDockState.SelectedItem.Value;
					uiPanelBase.DockStyle = (PanelDockStyle)_cboDockStyle.SelectedItem.Value;
					if (_shouldRegisterPanel)
					{
						if (uiPanelBase.PanelManager != null && uiPanelBase.PanelManager.Equals(_previewManager))
						{
							uiPanelBase.PanelManager.Panels.Remove(uiPanelBase);
						}
						_targetManager.Panels.Add(uiPanelBase);
					}
				}
				else if (_optAddToGroup.Checked)
				{
					MxDockPanelGroup uiPanelGroup = (MxDockPanelGroup)_cboPanelGroups.SelectedItem.Value;
					uiPanelGroup.Panels.Add(uiPanelBase);
				}
				else
				{
					DockPanelBase partnerPanel = (DockPanelBase)_cboPanels.SelectedItem.Value;
					PanelGroupStyle groupStyle = (PanelGroupStyle)_cboGroupStyle.SelectedValue;
					MxDockPanelGroup newGroup = partnerPanel.CreateGroup(groupStyle, uiPanelBase);
					if (_chkStaticGroup.Checked)
					{
						newGroup.StaticGroup = true;
					}
				}
				uiPanelBase.AutoHide = false;
				return uiPanelBase;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return null;
			}
		}

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
			FillEnumCombo(typeof(PanelDockState), _cboDockState, PanelDockState.Docked);
			FillEnumCombo(typeof(PanelDockStyle), _cboDockStyle, PanelDockStyle.Left);
			FillEnumCombo(typeof(PanelGroupStyle), _cboGroupStyle, PanelGroupStyle.HorizontalTiles);
			FillEnumCombo(typeof(PanelGroupStyle), _cboStaticGroupStyle, PanelGroupStyle.HorizontalTiles);
			FillPanelGroupList();
			FillGroupWithPanelList();
			_optDockInContainer.Checked = true;
			SuggestPanelName();
			_cmdPrevious.Enabled = false;
		}

		private void FillGroupWithPanelList()
		{
			FillPanelCombo(_cboPanels, _targetManager.Panels, 0);
			if (_cboPanels.Items.Count == 0)
			{
				_optGroupWithPanel.Enabled = false;
			}
		}

		private void FillExistingPanelList()
		{
			FillPanelCombo(_cboExistingPanels, _targetManager.Panels, 0);
			if (_cboExistingPanels.Items.Count == 0)
			{
				_optExistingPanel.Enabled = false;
			}
		}

		private void FillPanelCombo(MxComboBox uiComboBox, DockPanelCollection uiPanelCollection, int depth)
		{
			foreach (DockPanelBase item in (IEnumerable)uiPanelCollection)
			{
				if (item is MxDockPanelGroup)
				{
					MxDockPanelGroup uiPanelGroup = (MxDockPanelGroup)item;
					string text = item.Name + ": {" + uiPanelGroup.GroupStyle;
					if (depth == 0)
					{
						text = text + ", " + uiPanelGroup.DockState;
						if (uiPanelGroup.DockState == PanelDockState.Docked)
						{
							text = text + ", " + uiPanelGroup.DockStyle;
						}
					}
					text += "}";
					ComboBoxItem uiComboBoxItem = new ComboBoxItem(uiPanelGroup, 0);
					uiComboBoxItem.Text = text;
					uiComboBoxItem.IndentLevel = depth;
					uiComboBox.Items.Add(uiComboBoxItem);
					FillPanelCombo(uiComboBox, uiPanelGroup.Panels, depth + 1);
					continue;
				}
				string panelText = item.Name + ": {'" + item.Text + "'";
				if (depth == 0)
				{
					panelText = panelText + ", " + item.DockState;
					if (item.DockState == PanelDockState.Docked)
					{
						panelText = panelText + ", " + item.DockStyle;
					}
				}
				panelText += "}";
				ComboBoxItem panelItem = new ComboBoxItem(item, 0);
				panelItem.Text = panelText;
				panelItem.IndentLevel = depth;
				uiComboBox.Items.Add(panelItem);
			}
		}

		private void FillPanelGroupList()
		{
			AddPanelGroupItems(_targetManager.Panels, 0);
			if (_cboPanelGroups.Items.Count == 0)
			{
				_optAddToGroup.Enabled = false;
			}
		}

		private void AddPanelGroupItems(DockPanelCollection uiPanelCollection, int depth)
		{
			foreach (DockPanelBase item in (IEnumerable)uiPanelCollection)
			{
				if (!(item is MxDockPanelGroup))
				{
					continue;
				}
				MxDockPanelGroup uiPanelGroup = (MxDockPanelGroup)item;
				string text = item.Name + ": {" + uiPanelGroup.GroupStyle;
				if (depth == 0)
				{
					text = text + ", " + uiPanelGroup.DockState;
					if (uiPanelGroup.DockState == PanelDockState.Docked)
					{
						text = text + ", " + uiPanelGroup.DockStyle;
					}
				}
				text += "}";
				ComboBoxItem uiComboBoxItem = new ComboBoxItem(uiPanelGroup, 0);
				uiComboBoxItem.IndentLevel = depth;
				uiComboBoxItem.Text = text;
				_cboPanelGroups.Items.Add(uiComboBoxItem);
				AddPanelGroupItems(uiPanelGroup.Panels, depth + 1);
			}
		}

		private void FillEnumCombo(Type enumType, MxComboBox uiComboBox, object value)
		{
			Array values = Enum.GetValues(enumType);
			foreach (object item in values)
			{
				uiComboBox.Items.Add(item);
			}
			uiComboBox.SelectedValue = value;
		}

		private void OnCmdNextClick(object sender, EventArgs eventArgs)
		{
			GoToStep(GetCurrentStep() + 1);
		}

		private int GetCurrentStep()
		{
			return _currentStep;
		}

		private void GoToStep(int step)
		{
			if (_currentStep != step)
			{
				_stepPanels[_currentStep].Visible = false;
				_currentStep = step;
				_stepPanels[_currentStep].Visible = true;
				if (_currentStep == _stepCount - 1)
				{
					_cmdNext.Enabled = false;
				}
				else
				{
					_cmdNext.Enabled = true;
				}
				UpdateStepButtons();
			}
		}

		private void UpdateStepButtons()
		{
			switch (_currentStep)
			{
			case 0:
				SetStaticGroupMode(false);
				break;
			case 1:
				SetStaticGroupMode(_optCreateStaticGroup.Checked);
				break;
			}
			_cmdPrevious.Enabled = _currentStep != 0;
		}

		private void SuggestPanelName()
		{
			int suffix = 0;
			string text = GeneratedPanelNamePrefix;
			string candidateName = text + suffix;
			while (!IsPanelNameAvailable(_targetManager, candidateName))
			{
				suffix++;
				candidateName = text + suffix;
			}
			_txtName.Text = candidateName;
			_txtText.Text = "Panel " + suffix;
		}

		private void OnCmdPreviousClick(object sender, EventArgs eventArgs)
		{
			GoToStep(GetCurrentStep() - 1);
		}

		private void OnOptDockInContainerCheckedChanged(object sender, EventArgs eventArgs)
		{
			_cboDockState.Enabled = _optDockInContainer.Checked;
			_cboDockStyle.Enabled = _optDockInContainer.Checked;
		}

		private void OnOptAddToGroupCheckedChanged(object sender, EventArgs eventArgs)
		{
			_cboPanelGroups.Enabled = _optAddToGroup.Checked;
		}

		private void OnOptGroupWithPanelCheckedChanged(object sender, EventArgs eventArgs)
		{
			_cboPanels.Enabled = _optGroupWithPanel.Checked;
			_cboGroupStyle.Enabled = _optGroupWithPanel.Checked;
			_grpGroupProperties.Enabled = _optGroupWithPanel.Checked;
		}

		private void OnTxtTextTextChanged(object sender, EventArgs eventArgs)
		{
			if (_samplePanel != null)
			{
				_samplePanel.Text = _txtText.Text;
			}
		}

		private void OnOptCreateStaticGroupCheckedChanged(object sender, EventArgs eventArgs)
		{
			_cboStaticGroupStyle.Enabled = _optCreateStaticGroup.Checked;
		}

		private void OnCboDockStateSelectedItemChanged(object sender, EventArgs eventArgs)
		{
			object value = _cboDockState.SelectedItem.Value;
			if (value != null)
			{
				if ((PanelDockState)value == PanelDockState.Floating)
				{
					_cboDockStyle.Visible = false;
					_lblDockStyle.Visible = false;
				}
				else
				{
					_cboDockStyle.Visible = true;
					_lblDockStyle.Visible = true;
				}
			}
		}

		private void OnCboDockStyleSelectedItemChanged(object sender, EventArgs eventArgs)
		{
			object value = _cboDockStyle.SelectedItem.Value;
			if (value == null)
			{
				return;
			}
			PanelDockStyle panelDockStyle = (PanelDockStyle)value;
			if (panelDockStyle != PanelDockStyle.Fill)
			{
				return;
			}
			foreach (DockPanelBase item in (IEnumerable)_targetManager.Panels)
			{
				if (item.DockState == PanelDockState.Docked && item.DockStyle == PanelDockStyle.Fill)
				{
					MessageBox.Show("There is already a panel with DockStyle 'Fill'.\n\n\rOnly one panel in the PanelManager can use this DockStyle.");
					_cboDockStyle.SelectedValue = PanelDockStyle.Left;
				}
			}
		}

		private void OnOptExistingPanelCheckedChanged(object sender, EventArgs eventArgs)
		{
			if (_optExistingPanel.Checked)
			{
				_cboExistingPanels.Enabled = true;
				_cmdNext.Enabled = false;
			}
			else
			{
				_cboExistingPanels.Enabled = false;
				_cmdNext.Enabled = true;
			}
		}
	}
}
