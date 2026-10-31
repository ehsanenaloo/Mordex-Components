using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.CommandBars
{
	[SuppressUnmanagedCodeSecurity]
	public partial class CustomizeForm : Form
	{
		private const uint CursorSizeAll = 32646u;

		private const uint CursorArrow = 32512u;

		private const int DefaultShowFullMenuDelay = 5000;

		private const int WmActivateApp = 28;

		private const int WmSetCursor = 32;

		private const int WmNcLButtonDown = 161;

		private const int WmNcLButtonUp = 162;

		private const int WmNcRButtonDown = 164;

		private const int WmNcRButtonUp = 165;

		private const int WmNcMButtonDown = 167;

		private const int WmNcMButtonUp = 168;

		private const int WmNcXButtonDown = 171;

		private const int WmNcXButtonUp = 172;

		private const int WmKeyDown = 256;

		private const int WmKeyUp = 257;

		private const int WmChar = 258;

		private const int WmSysKeyDown = 260;

		private const int WmSysKeyUp = 261;

		private const int WmMouseMove = 512;

		private const int WmLButtonDown = 513;

		private const int WmLButtonUp = 514;

		private const int WmRButtonDown = 516;

		private const int WmRButtonUp = 517;

		private const int WmMButtonDown = 519;

		private const int WmMButtonUp = 520;

		private const int WmXButtonDown = 523;

		private const int WmXButtonUp = 524;

		public MxTabControl mordexTab;

		public MxButton btnClose;

		public MxButton btnKeyboard;

		public MxTabPage jtpToolbars;

		public MxTabPage jtpCommands;

		public MxTabPage jtpOptions;

		public MxButton btnNew;

		public MxButton btnRename;

		public MxButton btnDelete;

		public Label lblToolbars;

		public Label lblPersonalize;

		public MxButton btnResetUsageData;

		public Label lblOther;

		public MxComboBox jcboAnimation;

		public Label lblAnimation;

		public ListView lvwToolbars;

		public MxButton btnReset;

		public ColumnHeader columnHeader1;

		public Label lblInfo1;

		public Label lblCategories;

		public Label lblCommands;

		public ListBox lbCategories;

		public MxButton btnModifySelection;

		public ListBox lbCommands;

		public Label lblSelected;

		public MxButton btnDescription;

		public MxCheckBox chkAlwaysShow;

		public MxCheckBox chkShowAfterDelay;

		public MxCheckBox chkLargeIcons;

		public MxCheckBox chkFontName;

		public MxCheckBox chkScreenTips;

		public MxCheckBox chkShortcutTips;

		private Container _components = new Container();

		private MxCommandCollection _customizableCommands;

		private string _animationTextSystem = "";

		private string _animationTextFade = "";

		private string _animationTextNone = "";

		private string _animationTextSlide = "";

		private string _allCommandsText = "";

		private string _animationTextUnfold = "";

		private string _animationTextRandom = "";

		private CommandCategoryCollection _categories;

		private MxCommandManager _commandManager;

		private PopupSettingsRenderer _commandItemRenderer;

		private MxCollection _commandListItems;

		private MxCommandBar _selectedBar;

		private IntPtr _previousCursor = IntPtr.Zero;

		private bool _isLoopDone;

		private WindowMessageHandler _tipHandler;

		[Localizable(true)]
		[Description("Represents the text for the System option in the AnimationStyle combo.")]
		[DefaultValue("")]
		public string AnimationTextSystem
		{
			get
			{
				return _animationTextSystem;
			}
			set
			{
				_animationTextSystem = value;
			}
		}

		[Description("Represents the text for the Fade option in the AnimationStyle combo.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string AnimationTextFade
		{
			get
			{
				return _animationTextFade;
			}
			set
			{
				_animationTextFade = value;
			}
		}

		[Localizable(true)]
		[Description("Represents the text for the None option in the AnimationStyle combo.")]
		[DefaultValue("")]
		public string AnimationTextNone
		{
			get
			{
				return _animationTextNone;
			}
			set
			{
				_animationTextNone = value;
			}
		}

		[Description("Represents the text for the Slide option in the AnimationStyle combo.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string AnimationTextSlide
		{
			get
			{
				return _animationTextSlide;
			}
			set
			{
				_animationTextSlide = value;
			}
		}

		[Localizable(true)]
		[Description("Represents the text for the All Commands item in the categories list box.")]
		[DefaultValue("")]
		public string AllCommandsText
		{
			get
			{
				return _allCommandsText;
			}
			set
			{
				_allCommandsText = value;
			}
		}

		[Description("Represents the text for the Unfold option in the AnimationStyle combo.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string AnimationTextUnfold
		{
			get
			{
				return _animationTextUnfold;
			}
			set
			{
				_animationTextUnfold = value;
			}
		}

		[Description("Represents the text for the Random option in the AnimationStyle combo.")]
		[DefaultValue("")]
		[Localizable(true)]
		public string AnimationTextRandom
		{
			get
			{
				return _animationTextRandom;
			}
			set
			{
				_animationTextRandom = value;
			}
		}

		[Browsable(false)]
		public MxCommandManager CommandManager => _commandManager;

		public CustomizeForm()
		{
			InitializeControls();
			Mordex.Windows.Common.ControlThemer.AttachDialog(this);
		}

		protected override void Dispose(bool disposing)
		{
			_isLoopDone = true;
			if (disposing && _commandManager != null)
			{
				_commandManager.RemoveSelectedCommandChangedHandler(OnSelectedCommandChanged);
			}
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			mordexTab = new MxTabControl();
			jtpToolbars = new MxTabPage();
			lvwToolbars = new ListView();
			columnHeader1 = new ColumnHeader();
			btnReset = new MxButton();
			btnNew = new MxButton();
			btnRename = new MxButton();
			btnDelete = new MxButton();
			lblToolbars = new Label();
			jtpCommands = new MxTabPage();
			btnModifySelection = new MxButton();
			lbCategories = new ListBox();
			lblCategories = new Label();
			lblInfo1 = new Label();
			lblCommands = new Label();
			lbCommands = new ListBox();
			lblSelected = new Label();
			btnDescription = new MxButton();
			jtpOptions = new MxTabPage();
			chkAlwaysShow = new MxCheckBox();
			lblAnimation = new Label();
			jcboAnimation = new MxComboBox();
			lblOther = new Label();
			btnResetUsageData = new MxButton();
			lblPersonalize = new Label();
			chkShowAfterDelay = new MxCheckBox();
			chkLargeIcons = new MxCheckBox();
			chkFontName = new MxCheckBox();
			chkScreenTips = new MxCheckBox();
			chkShortcutTips = new MxCheckBox();
			btnClose = new MxButton();
			btnKeyboard = new MxButton();
			((ISupportInitialize)mordexTab).BeginInit();
			mordexTab.SuspendLayout();
			jtpToolbars.SuspendLayout();
			jtpCommands.SuspendLayout();
			jtpOptions.SuspendLayout();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			mordexTab.BackColor = Color.Transparent;
			mordexTab.Location = new Point(8, 12);
			mordexTab.Name = "mordexTab";
			mordexTab.Size = new Size(364, 284);
			mordexTab.TabIndex = 0;
			mordexTab.TabPages.AddRange(new MxTabPage[3] { jtpToolbars, jtpCommands, jtpOptions });
			jtpToolbars.Controls.Add(lvwToolbars);
			jtpToolbars.Controls.Add(btnReset);
			jtpToolbars.Controls.Add(btnNew);
			jtpToolbars.Controls.Add(btnRename);
			jtpToolbars.Controls.Add(btnDelete);
			jtpToolbars.Controls.Add(lblToolbars);
			jtpToolbars.Location = new Point(1, 21);
			jtpToolbars.Name = "jtpToolbars";
			jtpToolbars.Size = new Size(360, 260);
			jtpToolbars.TabStop = true;
			jtpToolbars.Text = "Toolbars";
			lvwToolbars.CheckBoxes = true;
			lvwToolbars.Columns.AddRange(new ColumnHeader[1] { columnHeader1 });
			lvwToolbars.FullRowSelect = true;
			lvwToolbars.HeaderStyle = ColumnHeaderStyle.None;
			lvwToolbars.HideSelection = false;
			lvwToolbars.LabelWrap = false;
			lvwToolbars.Location = new Point(8, 28);
			lvwToolbars.MultiSelect = false;
			lvwToolbars.Name = "lvwToolbars";
			lvwToolbars.Size = new Size(264, 228);
			lvwToolbars.TabIndex = 0;
			lvwToolbars.UseCompatibleStateImageBehavior = false;
			lvwToolbars.View = System.Windows.Forms.View.Details;
			lvwToolbars.SelectedIndexChanged += OnListViewSelectedIndexChanged;
			lvwToolbars.ItemCheck += OnListViewItemCheck;
			columnHeader1.Width = 260;
			btnReset.Location = new Point(280, 118);
			btnReset.Name = "btnReset";
			btnReset.Size = new Size(73, 23);
			btnReset.TabIndex = 4;
			btnReset.Text = "&Reset...";
			btnReset.Click += OnResetClick;
			btnNew.Location = new Point(280, 28);
			btnNew.Name = "btnNew";
			btnNew.Size = new Size(73, 23);
			btnNew.TabIndex = 1;
			btnNew.Text = "&New...";
			btnNew.Click += OnNewClick;
			btnRename.Enabled = false;
			btnRename.Location = new Point(280, 58);
			btnRename.Name = "btnRename";
			btnRename.Size = new Size(73, 23);
			btnRename.TabIndex = 2;
			btnRename.Text = "R&ename...";
			btnRename.Click += OnRenameClick;
			btnDelete.Enabled = false;
			btnDelete.Location = new Point(280, 88);
			btnDelete.Name = "btnDelete";
			btnDelete.Size = new Size(73, 23);
			btnDelete.TabIndex = 3;
			btnDelete.Text = "&Delete";
			btnDelete.Click += OnDeleteClick;
			lblToolbars.AutoSize = true;
			lblToolbars.BackColor = Color.Transparent;
			lblToolbars.Location = new Point(8, 8);
			lblToolbars.Name = "lblToolbars";
			lblToolbars.Size = new Size(52, 13);
			lblToolbars.TabIndex = 0;
			lblToolbars.Text = "Toolb&ars:";
			jtpCommands.Controls.Add(btnModifySelection);
			jtpCommands.Controls.Add(lbCategories);
			jtpCommands.Controls.Add(lblCategories);
			jtpCommands.Controls.Add(lblInfo1);
			jtpCommands.Controls.Add(lblCommands);
			jtpCommands.Controls.Add(lbCommands);
			jtpCommands.Controls.Add(lblSelected);
			jtpCommands.Controls.Add(btnDescription);
			jtpCommands.Location = new Point(1, 21);
			jtpCommands.Name = "jtpCommands";
			jtpCommands.Size = new Size(360, 260);
			jtpCommands.TabStop = true;
			jtpCommands.Text = "Commands";
			btnModifySelection.Location = new Point(114, 233);
			btnModifySelection.Name = "btnModifySelection";
			btnModifySelection.Size = new Size(100, 24);
			btnModifySelection.TabIndex = 6;
			btnModifySelection.Text = "&Modify Selection";
			btnModifySelection.Click += OnModifySelectionClick;
			lbCategories.Location = new Point(8, 52);
			lbCategories.Name = "lbCategories";
			lbCategories.Size = new Size(120, 160);
			lbCategories.TabIndex = 1;
			lbCategories.SelectedIndexChanged += OnListBoxSelectedIndexChanged;
			lblCategories.BackColor = Color.Transparent;
			lblCategories.Location = new Point(8, 36);
			lblCategories.Name = "lblCategories";
			lblCategories.Size = new Size(63, 13);
			lblCategories.TabIndex = 0;
			lblCategories.Text = "Cate&gories:";
			lblInfo1.BackColor = Color.Transparent;
			lblInfo1.Location = new Point(8, 4);
			lblInfo1.Name = "lblInfo1";
			lblInfo1.Size = new Size(344, 32);
			lblInfo1.TabIndex = 0;
			lblInfo1.Text = "To add a command to a toolbar: select a category and drag the command out of this dialog box to a toolbar";
			lblCommands.BackColor = Color.Transparent;
			lblCommands.Location = new Point(144, 36);
			lblCommands.Name = "lblCommands";
			lblCommands.Size = new Size(70, 13);
			lblCommands.TabIndex = 2;
			lblCommands.Text = "Comman&ds:";
			lbCommands.DrawMode = DrawMode.OwnerDrawVariable;
			lbCommands.Location = new Point(144, 52);
			lbCommands.Name = "lbCommands";
			lbCommands.Size = new Size(208, 160);
			lbCommands.TabIndex = 3;
			lbCommands.DrawItem += OnListBoxDrawItem;
			lbCommands.MeasureItem += OnListBoxMeasureItem;
			lbCommands.MouseDown += OnListBoxMouseDown;
			lblSelected.AutoSize = true;
			lblSelected.BackColor = Color.Transparent;
			lblSelected.Location = new Point(8, 215);
			lblSelected.Name = "lblSelected";
			lblSelected.Size = new Size(100, 13);
			lblSelected.TabIndex = 4;
			lblSelected.Text = "Selected command:";
			btnDescription.Location = new Point(8, 233);
			btnDescription.Name = "btnDescription";
			btnDescription.Size = new Size(100, 24);
			btnDescription.TabIndex = 5;
			btnDescription.Text = "Descri&ption";
			btnDescription.Click += OnDescriptionClick;
			jtpOptions.Controls.Add(chkAlwaysShow);
			jtpOptions.Controls.Add(lblAnimation);
			jtpOptions.Controls.Add(jcboAnimation);
			jtpOptions.Controls.Add(lblOther);
			jtpOptions.Controls.Add(btnResetUsageData);
			jtpOptions.Controls.Add(lblPersonalize);
			jtpOptions.Controls.Add(chkShowAfterDelay);
			jtpOptions.Controls.Add(chkLargeIcons);
			jtpOptions.Controls.Add(chkFontName);
			jtpOptions.Controls.Add(chkScreenTips);
			jtpOptions.Controls.Add(chkShortcutTips);
			jtpOptions.Location = new Point(1, 21);
			jtpOptions.Name = "jtpOptions";
			jtpOptions.Size = new Size(360, 260);
			jtpOptions.TabStop = true;
			jtpOptions.Text = "Options";
			chkAlwaysShow.BackColor = Color.Transparent;
			chkAlwaysShow.Location = new Point(20, 34);
			chkAlwaysShow.Name = "chkAlwaysShow";
			chkAlwaysShow.Size = new Size(140, 16);
			chkAlwaysShow.TabIndex = 0;
			chkAlwaysShow.Text = "Always show full me&nus";
			chkAlwaysShow.CheckedChanged += OnAlwaysShowChanged;
			lblAnimation.AutoSize = true;
			lblAnimation.BackColor = Color.Transparent;
			lblAnimation.Location = new Point(8, 219);
			lblAnimation.Name = "lblAnimation";
			lblAnimation.Size = new Size(91, 13);
			lblAnimation.TabIndex = 7;
			lblAnimation.Text = "&Menu animations:";
			jcboAnimation.ComboStyle = ComboStyle.DropDownList;
			jcboAnimation.DropListFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			jcboAnimation.Location = new Point(108, 216);
			jcboAnimation.Name = "jcboAnimation";
			jcboAnimation.Size = new Size(148, 21);
			jcboAnimation.TabIndex = 7;
			jcboAnimation.SelectedValueChanged += OnAnimationSelectedValueChanged;
			lblOther.AutoSize = true;
			lblOther.BackColor = Color.Transparent;
			lblOther.Location = new Point(4, 108);
			lblOther.Name = "lblOther";
			lblOther.Size = new Size(35, 13);
			lblOther.TabIndex = 3;
			lblOther.Text = "Other";
			btnResetUsageData.Location = new Point(20, 74);
			btnResetUsageData.Name = "btnResetUsageData";
			btnResetUsageData.Size = new Size(216, 24);
			btnResetUsageData.TabIndex = 2;
			btnResetUsageData.Text = "&Reset menu and toolbar usage data";
			btnResetUsageData.Click += OnResetUsageDataClick;
			lblPersonalize.AutoSize = true;
			lblPersonalize.BackColor = Color.Transparent;
			lblPersonalize.Location = new Point(4, 12);
			lblPersonalize.Name = "lblPersonalize";
			lblPersonalize.Size = new Size(166, 13);
			lblPersonalize.TabIndex = 0;
			lblPersonalize.Text = "Personalized Menus and Toolbars";
			chkShowAfterDelay.BackColor = Color.Transparent;
			chkShowAfterDelay.Location = new Point(36, 54);
			chkShowAfterDelay.Name = "chkShowAfterDelay";
			chkShowAfterDelay.Size = new Size(200, 16);
			chkShowAfterDelay.TabIndex = 1;
			chkShowAfterDelay.Text = "Show f&ull menus after a short delay";
			chkShowAfterDelay.CheckedChanged += OnShowAfterDelayChanged;
			chkLargeIcons.BackColor = Color.Transparent;
			chkLargeIcons.Location = new Point(20, 132);
			chkLargeIcons.Name = "chkLargeIcons";
			chkLargeIcons.Size = new Size(84, 16);
			chkLargeIcons.TabIndex = 3;
			chkLargeIcons.Text = "&Large icons";
			chkLargeIcons.CheckedChanged += OnLargeIconsChanged;
			chkFontName.BackColor = Color.Transparent;
			chkFontName.Location = new Point(20, 153);
			chkFontName.Name = "chkFontName";
			chkFontName.Size = new Size(160, 16);
			chkFontName.TabIndex = 4;
			chkFontName.Text = "List &font names in their font";
			chkFontName.CheckedChanged += OnFontNameChanged;
			chkScreenTips.BackColor = Color.Transparent;
			chkScreenTips.Location = new Point(20, 174);
			chkScreenTips.Name = "chkScreenTips";
			chkScreenTips.Size = new Size(168, 16);
			chkScreenTips.TabIndex = 5;
			chkScreenTips.Text = "Show Screen&Tips on toolbars";
			chkScreenTips.CheckedChanged += OnScreenTipsChanged;
			chkShortcutTips.BackColor = Color.Transparent;
			chkShortcutTips.Location = new Point(40, 195);
			chkShortcutTips.Name = "chkShortcutTips";
			chkShortcutTips.Size = new Size(192, 16);
			chkShortcutTips.TabIndex = 6;
			chkShortcutTips.Text = "Show s&hortcut keys  in ScreenTips";
			chkShortcutTips.CheckedChanged += OnShortcutTipsChanged;
			btnClose.Location = new Point(296, 304);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(75, 23);
			btnClose.TabIndex = 1;
			btnClose.Text = "Close";
			btnClose.Click += OnCloseClick;
			btnKeyboard.Location = new Point(212, 304);
			btnKeyboard.Name = "btnKeyboard";
			btnKeyboard.Size = new Size(76, 23);
			btnKeyboard.TabIndex = 2;
			btnKeyboard.Text = "&Keyboard...";
			btnKeyboard.Click += OnKeyboardClick;
			base.ClientSize = new Size(377, 332);
			base.Controls.Add(mordexTab);
			base.Controls.Add(btnClose);
			base.Controls.Add(btnKeyboard);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "CustomizeForm";
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterScreen;
			Text = "Customize ";
			((ISupportInitialize)mordexTab).EndInit();
			mordexTab.ResumeLayout(performLayout: false);
			jtpToolbars.ResumeLayout(performLayout: false);
			jtpToolbars.PerformLayout();
			jtpCommands.ResumeLayout(performLayout: false);
			jtpCommands.PerformLayout();
			jtpOptions.ResumeLayout(performLayout: false);
			jtpOptions.PerformLayout();
			ResumeLayout(performLayout: false);
		}

		internal MxCommandCollection GetCustomizableCommands()
		{
			if (_customizableCommands != null)
			{
				return _customizableCommands;
			}
			if (_commandManager.GetMergedManager() == null)
			{
				_customizableCommands = new MxCommandCollection(_commandManager);
				foreach (MxCommand command in CommandManager.Commands)
				{
					if (command.ShowInCustomizeDialog)
					{
						_customizableCommands.AddWithoutEvents(command);
					}
				}
			}
			else
			{
				_customizableCommands = new MxCommandCollection(_commandManager);
				foreach (MxCommand localCommand in CommandManager.Commands)
				{
					if (localCommand.ShowInCustomizeDialog)
					{
						_customizableCommands.AddWithoutEvents(localCommand);
					}
				}
				foreach (MxCommand parentCommand in CommandManager.GetMergedManager().Commands)
				{
					if (parentCommand.ShowInCustomizeDialog && CommandManager.Commands.FindByKey(parentCommand.Key) == null)
					{
						_customizableCommands.AddWithoutEvents(parentCommand);
					}
				}
			}
			return _customizableCommands;
		}

		internal CommandCategoryCollection GetEffectiveCategories()
		{
			if (_categories != null)
			{
				return _categories;
			}
			if (_commandManager.GetMergedManager() == null)
			{
				_categories = _commandManager.Categories;
			}
			else
			{
				_categories = new CommandCategoryCollection(_commandManager);
				foreach (CommandCategory category in CommandManager.Categories)
				{
					_categories.Add(category);
				}
				foreach (CommandCategory parentCategory in CommandManager.GetMergedManager().Categories)
				{
					if (!CommandManager.Categories.ContainsName(parentCategory.CategoryName))
					{
						_categories.Add(parentCategory);
					}
				}
			}
			return _categories;
		}

		internal void AttachCommandManager(MxCommandManager uiCommandManager)
		{
			_commandManager?.RemoveSelectedCommandChangedHandler(OnSelectedCommandChanged);
			_commandManager = uiCommandManager;
			_commandManager.AddSelectedCommandChangedHandler(OnSelectedCommandChanged);
			btnModifySelection.Enabled = false;
			btnDescription.Enabled = false;
			chkFontName.Checked = CommandManager.UseFontInFontNames;
			chkLargeIcons.Checked = CommandManager.View == View.LargeIcons;
			chkScreenTips.Checked = CommandManager.ShowToolTips;
			chkAlwaysShow.Checked = CommandManager.AlwaysShowFullMenus;
			chkShowAfterDelay.Checked = CommandManager.ShowFullMenuDelay >= 0;
			chkShortcutTips.Checked = CommandManager.ShowShortcutInToolTips;
		}

		private void OnSelectedCommandChanged(object sender, EventArgs eventArgs)
		{
			DisposeTipHandler();
			if (_commandManager.GetSelectedCommand() != null)
			{
				btnModifySelection.Enabled = true;
				btnDescription.Enabled = true;
			}
			else
			{
				btnModifySelection.Enabled = false;
				btnDescription.Enabled = false;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (base.DesignMode)
			{
				return;
			}
			_commandListItems = new MxCollection(lbCommands);
			if (CommandManager != null)
			{
				lbCommands.BackColor = RenderingHelper.GetSchemeColor(ColorName.Control);
				_commandItemRenderer = new PopupSettingsRenderer(CommandManager);
				CommandManager.CommandBarAdded += OnControlCommandBarAdded;
				CommandManager.CommandBarRemoved += OnControlCommandBarRemoved;
				foreach (MxCommandBar commandBar in CommandManager.CommandBars)
				{
					commandBar.VisibleChanged += OnControlVisibleChanged;
					commandBar.TextChanged += OnControlTextChanged;
					ListViewItem listViewItem = new ListViewItem(commandBar.Text);
					listViewItem.Checked = commandBar.Visible;
					listViewItem.Tag = commandBar;
					lvwToolbars.Items.Add(listViewItem);
				}
				lbCategories.DisplayMember = "Name";
				lbCategories.ValueMember = "Object";
				string name = "All Commands";
				if (_allCommandsText.Length > 0)
				{
					name = _allCommandsText;
				}
				ON item = new ON(null, name);
				lbCategories.Items.Add(item);
				foreach (CommandCategory category in GetEffectiveCategories())
				{
					item = new ON(category, category.CategoryName);
					lbCategories.Items.Add(item);
				}
				lbCategories.SelectedIndex = 0;
				base.Owner = RenderingHelper.FindForm(CommandManager.ContainerControl);
			}
			else
			{
				lbCommands.BackColor = SystemColors.Control;
			}
			if (lvwToolbars.Items.Count > 0)
			{
				lvwToolbars.Items[0].Selected = true;
			}
			FillAnimationItems();
		}

		private void FillAnimationItems()
		{
			ComboBoxItem uiComboBoxItem = null;
			uiComboBoxItem = new ComboBoxItem(DropDownAnimation.Fade);
			if (AnimationTextFade.Length > 0)
			{
				uiComboBoxItem.Text = AnimationTextFade;
			}
			else
			{
				uiComboBoxItem.Text = "Fade";
			}
			jcboAnimation.Items.Add(uiComboBoxItem);
			uiComboBoxItem = new ComboBoxItem(DropDownAnimation.None);
			if (AnimationTextNone.Length > 0)
			{
				uiComboBoxItem.Text = AnimationTextNone;
			}
			else
			{
				uiComboBoxItem.Text = "None";
			}
			jcboAnimation.Items.Add(uiComboBoxItem);
			uiComboBoxItem = new ComboBoxItem(DropDownAnimation.Random);
			if (AnimationTextRandom.Length > 0)
			{
				uiComboBoxItem.Text = AnimationTextRandom;
			}
			else
			{
				uiComboBoxItem.Text = "Random";
			}
			jcboAnimation.Items.Add(uiComboBoxItem);
			uiComboBoxItem = new ComboBoxItem(DropDownAnimation.Slide);
			if (AnimationTextSlide.Length > 0)
			{
				uiComboBoxItem.Text = AnimationTextSlide;
			}
			else
			{
				uiComboBoxItem.Text = "Slide";
			}
			jcboAnimation.Items.Add(uiComboBoxItem);
			uiComboBoxItem = new ComboBoxItem(DropDownAnimation.System);
			if (AnimationTextSystem.Length > 0)
			{
				uiComboBoxItem.Text = AnimationTextSystem;
			}
			else
			{
				uiComboBoxItem.Text = "System";
			}
			jcboAnimation.Items.Add(uiComboBoxItem);
			uiComboBoxItem = new ComboBoxItem(DropDownAnimation.Unfold);
			if (AnimationTextUnfold.Length > 0)
			{
				uiComboBoxItem.Text = AnimationTextUnfold;
			}
			else
			{
				uiComboBoxItem.Text = "Unfold";
			}
			jcboAnimation.Items.Add(uiComboBoxItem);
			jcboAnimation.SelectedValue = CommandManager.MenuAnimation;
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		internal MxCommandBar GetSelectedBar()
		{
			return _selectedBar;
		}

		internal void SetSelectedBar(MxCommandBar uiCommandBar)
		{
			if (_selectedBar != uiCommandBar)
			{
				if (_selectedBar != null)
				{
					_selectedBar.SetCustomizeSelectedCommand(null);
					_selectedBar.SetChildMenu(null);
				}
				_selectedBar = uiCommandBar;
			}
		}

		private void FillCommandList(CommandCategory category)
		{
			if (_commandListItems != null)
			{
				_commandListItems.ClearItems();
			}
			lbCommands.Items.Clear();
			if (category == null)
			{
				foreach (MxCommand item in GetCustomizableCommands())
				{
					if (!item.IsSeparator() && item.CommandType != CommandType.ControlContainer && item.CommandType != CommandType.ControlPopup)
					{
						lbCommands.Items.Add(item);
					}
				}
			}
			else
			{
				MxCommand[] commandsByCategory = GetCustomizableCommands().GetCommandsByCategory(category);
				foreach (MxCommand categoryCommand in commandsByCategory)
				{
					if (!categoryCommand.IsSeparator() && categoryCommand.CommandType != CommandType.ControlContainer && categoryCommand.CommandType != CommandType.ControlPopup)
					{
						lbCommands.Items.Add(categoryCommand);
					}
				}
			}
			lbCommands.Invalidate();
		}

		internal void DisposeTipHandler()
		{
			if (_tipHandler != null)
			{
				_tipHandler.HideAndDisposeTimers();
				_tipHandler = null;
			}
		}
	}
}
