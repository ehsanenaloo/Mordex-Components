using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI.CommandBars
{
	public class CustomizeKeyboardForm : Form
	{
		private Container _components = new Container();

		public MxComboBox cboShortcuts;

		public MxButton btnAssign;

		public MxButton btnRemove;

		public MxButton btnReset;

		public MxButton btnClose;

		public Label lblAssigned;

		public Label lblAssignedTo;

		public ListBox lbCommands;

		public ListBox lbCategories;

		public Label lblInfo1;

		public Label lblInfo2;

		public ListBox lbCurrentShortcut;

		public MxGroupBox grpDescription;

		public Label lblDescription;

		public Label lblCategories;

		public Label lblCommands;

		public Label lblCurrentShortcut;

		public Label lblNewShortcut;

		internal MxCommandManager _commandManager;

		private MxCommandCollection _customizableCommands;

		private string _allCommandsText = "";

		private CommandCategoryCollection _categories;

		[Browsable(false)]
		public MxCommandManager CommandManager => _commandManager;

		[DefaultValue("")]
		[Localizable(true)]
		[Description("Represents the text for the All Commands item in the categories list box.")]
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

		public CustomizeKeyboardForm()
		{
			InitializeControls();
			Mordex.Windows.Common.ControlThemer.AttachDialog(this);
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
			lblInfo1 = new Label();
			lblCategories = new Label();
			lblCommands = new Label();
			lblInfo2 = new Label();
			lblCurrentShortcut = new Label();
			lblNewShortcut = new Label();
			cboShortcuts = new MxComboBox();
			btnAssign = new MxButton();
			btnRemove = new MxButton();
			btnReset = new MxButton();
			btnClose = new MxButton();
			lblAssigned = new Label();
			lblAssignedTo = new Label();
			lbCommands = new ListBox();
			lbCategories = new ListBox();
			lbCurrentShortcut = new ListBox();
			grpDescription = new MxGroupBox();
			lblDescription = new Label();
			((ISupportInitialize)grpDescription).BeginInit();
			grpDescription.SuspendLayout();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			lblInfo1.AutoSize = true;
			lblInfo1.FlatStyle = FlatStyle.System;
			lblInfo1.Location = new Point(4, 8);
			lblInfo1.Name = "lblInfo1";
			lblInfo1.Size = new Size(100, 13);
			lblInfo1.TabIndex = 0;
			lblInfo1.Text = "Specify a command";
			lblCategories.AutoSize = true;
			lblCategories.FlatStyle = FlatStyle.System;
			lblCategories.Location = new Point(12, 28);
			lblCategories.Name = "lblCategories";
			lblCategories.Size = new Size(60, 13);
			lblCategories.TabIndex = 1;
			lblCategories.Text = "&Categories:";
			lblCommands.AutoSize = true;
			lblCommands.FlatStyle = FlatStyle.System;
			lblCommands.Location = new Point(156, 28);
			lblCommands.Name = "lblCommands";
			lblCommands.Size = new Size(62, 13);
			lblCommands.TabIndex = 3;
			lblCommands.Text = "C&ommands:";
			lblInfo2.AutoSize = true;
			lblInfo2.FlatStyle = FlatStyle.System;
			lblInfo2.Location = new Point(4, 156);
			lblInfo2.Name = "lblInfo2";
			lblInfo2.Size = new Size(95, 13);
			lblInfo2.TabIndex = 0;
			lblInfo2.Text = "Specify a shortcut:";
			lblCurrentShortcut.AutoSize = true;
			lblCurrentShortcut.FlatStyle = FlatStyle.System;
			lblCurrentShortcut.Location = new Point(12, 180);
			lblCurrentShortcut.Name = "lblCurrentShortcut";
			lblCurrentShortcut.Size = new Size(85, 13);
			lblCurrentShortcut.TabIndex = 4;
			lblCurrentShortcut.Text = "C&urrent shortcut:";
			lblNewShortcut.AutoSize = true;
			lblNewShortcut.FlatStyle = FlatStyle.System;
			lblNewShortcut.Location = new Point(156, 180);
			lblNewShortcut.Name = "lblNewShortcut";
			lblNewShortcut.Size = new Size(113, 13);
			lblNewShortcut.TabIndex = 4;
			lblNewShortcut.Text = "Select a &new shortcut:";
			cboShortcuts.ComboStyle = ComboStyle.DropDownList;
			cboShortcuts.Location = new Point(156, 204);
			cboShortcuts.Name = "cboShortcuts";
			cboShortcuts.Size = new Size(176, 20);
			cboShortcuts.TabIndex = 6;
			cboShortcuts.SelectedIndexChanged += OnShortcutSelectedIndexChanged;
			btnAssign.Location = new Point(8, 336);
			btnAssign.Name = "btnAssign";
			btnAssign.Size = new Size(74, 23);
			btnAssign.TabIndex = 7;
			btnAssign.Text = "&Assign";
			btnAssign.Click += OnAssignClick;
			btnRemove.Location = new Point(88, 336);
			btnRemove.Name = "btnRemove";
			btnRemove.Size = new Size(74, 23);
			btnRemove.TabIndex = 8;
			btnRemove.Text = "&Remove";
			btnRemove.Click += OnRemoveClick;
			btnReset.Location = new Point(168, 336);
			btnReset.Name = "btnReset";
			btnReset.Size = new Size(80, 23);
			btnReset.TabIndex = 9;
			btnReset.Text = "Re&set All...";
			btnReset.Click += OnResetClick;
			btnClose.DialogResult = DialogResult.Cancel;
			btnClose.Location = new Point(256, 336);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(72, 23);
			btnClose.TabIndex = 0;
			btnClose.Text = "Close";
			btnClose.Click += OnCloseClick;
			lblAssigned.AutoSize = true;
			lblAssigned.FlatStyle = FlatStyle.System;
			lblAssigned.Location = new Point(12, 236);
			lblAssigned.Name = "lblAssigned";
			lblAssigned.Size = new Size(103, 13);
			lblAssigned.TabIndex = 0;
			lblAssigned.Text = "Currently asigned to:";
			lblAssigned.Visible = false;
			lblAssignedTo.AutoSize = true;
			lblAssignedTo.Location = new Point(156, 236);
			lblAssignedTo.Name = "lblAssignedTo";
			lblAssignedTo.Size = new Size(0, 13);
			lblAssignedTo.TabIndex = 0;
			lbCommands.Location = new Point(156, 48);
			lbCommands.Name = "lbCommands";
			lbCommands.Size = new Size(176, 95);
			lbCommands.TabIndex = 4;
			lbCommands.SelectedIndexChanged += OnCommandSelected;
			lbCategories.Location = new Point(12, 48);
			lbCategories.Name = "lbCategories";
			lbCategories.Size = new Size(132, 95);
			lbCategories.TabIndex = 2;
			lbCategories.SelectedIndexChanged += OnCategorySelected;
			lbCurrentShortcut.IntegralHeight = false;
			lbCurrentShortcut.Location = new Point(12, 204);
			lbCurrentShortcut.Name = "lbCurrentShortcut";
			lbCurrentShortcut.Size = new Size(132, 21);
			lbCurrentShortcut.TabIndex = 5;
			lbCurrentShortcut.SelectedIndexChanged += OnCurrentShortcutSelected;
			grpDescription.Controls.Add(lblDescription);
			grpDescription.Location = new Point(12, 264);
			grpDescription.Name = "grpDescription";
			grpDescription.Size = new Size(320, 60);
			grpDescription.TabIndex = 0;
			grpDescription.Text = "Description";
			lblDescription.FlatStyle = FlatStyle.System;
			lblDescription.Location = new Point(8, 16);
			lblDescription.Name = "lblDescription";
			lblDescription.Size = new Size(308, 40);
			lblDescription.TabIndex = 1;
			base.ClientSize = new Size(337, 368);
			base.Controls.Add(grpDescription);
			base.Controls.Add(lbCurrentShortcut);
			base.Controls.Add(lbCategories);
			base.Controls.Add(lbCommands);
			base.Controls.Add(btnAssign);
			base.Controls.Add(cboShortcuts);
			base.Controls.Add(lblCurrentShortcut);
			base.Controls.Add(lblInfo1);
			base.Controls.Add(lblCategories);
			base.Controls.Add(lblCommands);
			base.Controls.Add(lblInfo2);
			base.Controls.Add(lblNewShortcut);
			base.Controls.Add(lblAssigned);
			base.Controls.Add(btnRemove);
			base.Controls.Add(btnReset);
			base.Controls.Add(btnClose);
			base.Controls.Add(lblAssignedTo);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "CustomizeKeyboardForm";
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			Text = "Customize Keyboard";
			((ISupportInitialize)grpDescription).EndInit();
			grpDescription.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 28 && m.WParam != IntPtr.Zero)
			{
				BringToFront();
			}
			base.WndProc(ref m);
		}

		protected override void OnLoad(EventArgs e)
		{
			if (base.DesignMode)
			{
				return;
			}
			base.OnLoad(e);
			Array values = Enum.GetValues(typeof(Shortcut));
			foreach (object shortcutValue in values)
			{
				cboShortcuts.Items.Add(shortcutValue);
			}
			ComboBoxItem uiComboBoxItem = cboShortcuts.Items[Shortcut.None];
			if (uiComboBoxItem != null)
			{
				cboShortcuts.Items.Remove(uiComboBoxItem);
			}
			lbCategories.DisplayMember = "Name";
			lbCategories.ValueMember = "Object";
			string name = "All Commands";
			if (AllCommandsText.Length > 0)
			{
				name = AllCommandsText;
			}
			ON item = new ON(null, name);
			lbCategories.Items.Add(item);
			foreach (CommandCategory category in GetEffectiveCategories())
			{
				item = new ON(category, category.CategoryName);
				lbCategories.Items.Add(item);
			}
			lbCategories.SelectedIndex = 0;
			if (lbCommands.Items.Count > 0)
			{
				lbCommands.SelectedIndex = 0;
			}
			lbCategories.SelectedIndex = 0;
			lbCommands.DisplayMember = "Name";
			lbCommands.ValueMember = "Object";
			btnAssign.Enabled = false;
			btnRemove.Enabled = false;
			UpdateResetButtonState();
		}

		private void UpdateResetButtonState()
		{
			foreach (MxCommand item in GetCustomizableCommands())
			{
				if (item.GetOriginalShortcut() != null)
				{
					btnReset.Enabled = true;
					return;
				}
			}
			btnReset.Enabled = false;
		}

		internal MxCommand GetSelectedCommand()
		{
			if (lbCommands.SelectedItem != null)
			{
				return (MxCommand)((ON)lbCommands.SelectedItem).Object;
			}
			return null;
		}

		private void OnAssignClick(object sender, EventArgs eventArgs)
		{
			if (cboShortcuts.SelectedItem != null)
			{
				lbCurrentShortcut.Items.Clear();
				Shortcut shortcut = (Shortcut)cboShortcuts.SelectedValue;
				lbCurrentShortcut.Items.Add(shortcut);
				lbCurrentShortcut.SelectedIndex = 0;
				if (GetSelectedCommand() != null)
				{
					GetSelectedCommand().UpdateOriginalShortcut(GetSelectedCommand().Shortcut);
					GetSelectedCommand().Shortcut = (Shortcut)lbCurrentShortcut.SelectedItem;
				}
				UpdateResetButtonState();
			}
			btnAssign.Enabled = false;
		}

		private void OnResetClick(object sender, EventArgs eventArgs)
		{
			foreach (MxCommand item in GetCustomizableCommands())
			{
				item.RestoreOriginalShortcut();
			}
			lbCurrentShortcut.Items.Clear();
			if (lbCommands.SelectedItem != null)
			{
				MxCommand selectedCommand = (MxCommand)((ON)lbCommands.SelectedItem).Object;
				if (selectedCommand.Shortcut != Shortcut.None)
				{
					lbCurrentShortcut.Items.Add(RenderingHelper.GetShortcutText(selectedCommand.Shortcut));
				}
			}
		}

		private void OnRemoveClick(object sender, EventArgs eventArgs)
		{
			if (lbCurrentShortcut.SelectedItem != null)
			{
				if (GetSelectedCommand() != null)
				{
					GetSelectedCommand().UpdateOriginalShortcut(GetSelectedCommand().Shortcut);
					GetSelectedCommand().Shortcut = Shortcut.None;
				}
				lbCommands.SelectedItem = null;
				UpdateResetButtonState();
			}
			btnRemove.Enabled = false;
		}

		private void OnCloseClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnShortcutSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			MxCommand uiCommand = null;
			if (cboShortcuts.SelectedItem != null)
			{
				btnAssign.Enabled = true;
				uiCommand = CommandManager.FindCommandByShortcut((Shortcut)cboShortcuts.SelectedValue);
			}
			else
			{
				btnAssign.Enabled = false;
			}
			if (uiCommand == null)
			{
				lblAssigned.Visible = false;
				lblAssignedTo.Visible = false;
			}
			else
			{
				lblAssigned.Visible = true;
				lblAssignedTo.Visible = true;
				lblAssignedTo.Text = uiCommand.GetEffectiveText();
			}
		}

		private void OnCommandSelected(object sender, EventArgs eventArgs)
		{
			lbCurrentShortcut.Items.Clear();
			if (lbCommands.SelectedItem != null)
			{
				MxCommand uiCommand = (MxCommand)((ON)lbCommands.SelectedItem).Object;
				lblDescription.Text = uiCommand.Description;
				cboShortcuts.SelectedItem = null;
				if (uiCommand.Shortcut != Shortcut.None)
				{
					lbCurrentShortcut.Items.Add(RenderingHelper.GetShortcutText(uiCommand.Shortcut));
				}
			}
			else
			{
				lblDescription.Text = "";
			}
		}

		private void OnCurrentShortcutSelected(object sender, EventArgs eventArgs)
		{
			if (lbCurrentShortcut.SelectedItem != null)
			{
				btnRemove.Enabled = true;
			}
			else
			{
				btnRemove.Enabled = false;
			}
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

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
		}

		private void FillCommandList(CommandCategory category)
		{
			lbCommands.Items.Clear();
			if (category == null)
			{
				foreach (MxCommand item in GetCustomizableCommands())
				{
					if (!item.IsSeparator())
					{
						string text = item.Text.Replace("&", "");
						if (text.Length > 0)
						{
							lbCommands.Items.Add(new ON(item, text));
						}
						else
						{
							lbCommands.Items.Add(new ON(item, item.Key));
						}
					}
				}
			}
			else
			{
				MxCommand[] commandsByCategory = GetCustomizableCommands().GetCommandsByCategory(category);
				foreach (MxCommand categoryCommand in commandsByCategory)
				{
					if (!categoryCommand.IsSeparator())
					{
						string categoryCommandText = categoryCommand.Text.Replace("&", "");
						if (categoryCommandText.Length > 0)
						{
							lbCommands.Items.Add(new ON(categoryCommand, categoryCommandText));
						}
						else
						{
							lbCommands.Items.Add(new ON(categoryCommand, categoryCommand.Key));
						}
					}
				}
			}
			lbCommands.Invalidate();
		}

		private void OnCategorySelected(object sender, EventArgs eventArgs)
		{
			if (lbCategories.SelectedItem != null)
			{
				ON oN = (ON)lbCategories.SelectedItem;
				CommandCategory uiCommandCategory = null;
				if (oN.Object != null)
				{
					uiCommandCategory = (CommandCategory)oN.Object;
				}
				FillCommandList(uiCommandCategory);
			}
		}
	}
}
