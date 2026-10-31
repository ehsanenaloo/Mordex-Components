using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Design
{
	internal class CommandCollectionEditorControl : UserControl, IUIElement
	{
		private const int SeparatorItemHeight = 8;

		private const int ItemHeightPadding = 2;

		private IContainer _components = default;

		private Button _btnOK;

		private Button _btnCancel;

		private Label _baseCommandsLabel;

		private Button _btnAdd;

		private Button _btnRemove;

		private Button _btnAddNew;

		private ListBox _lbBaseCommands;

		private ListBox _lbCommands;

		private Button _btnMoveUp;

		private Button _btnMoveDown;

		private Label _commandsLabel;

		private Button _btnClear;

		private MxCollection _baseCommandItems;

		private ComboBox _cboCategories;

		private Button _btnAddSeparator;

		private MxCollection _commandItems;

		private MxCommandCollection _commandCollection;

		private PopupSettingsRenderer _itemRenderer;

		private bool _canClearAll = true;

		internal MxCommandManager _commandManager;

		private ICollection _selectedObject;

		private EventHandler _disableCancelHandler;

		private Image _itemImage;

		private static object _selectedObjectChangedKey = new object();

		private static object _refreshTreeKey = new object();


		private EventHandler _dataChangedHandler;

		object IUIElement.SelectedObject
		{
			get
			{
				return _selectedObject;
			}
			set
			{
				if (!(value is ICollection))
				{
					throw new ArgumentException();
				}
				if (_selectedObject != value)
				{
					_selectedObject = (ICollection)value;
					LoadCommandCollection();
				}
			}
		}

		Image IUIElement.ItemImage
		{
			get
			{
				return _itemImage;
			}
			set
			{
				_itemImage = value;
				int itemHeight = (int)_lbCommands.Font.GetHeight() + ItemHeightPadding;
				if (_itemImage != null && _itemImage.Height + ItemHeightPadding > itemHeight)
				{
					itemHeight = _itemImage.Height + ItemHeightPadding;
				}
				_lbCommands.ItemHeight = itemHeight;
				_lbCommands.Refresh();
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler currentHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _disableCancelHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
			remove
			{
				EventHandler currentHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _disableCancelHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
		}

		event EventHandler IUIElement.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(_selectedObjectChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedObjectChangedKey, value);
			}
		}

		event EventHandler IUIElement.RefreshTree
		{
			add
			{
				base.Events.AddHandler(_refreshTreeKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_refreshTreeKey, value);
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler currentHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _dataChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
			remove
			{
				EventHandler currentHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = currentHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					currentHandler = Interlocked.CompareExchange(ref _dataChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)currentHandler != previousHandler);
			}
		}

		public CommandCollectionEditorControl(MxCommandManager manager)
		{
			InitializeControls();
			_commandManager = manager;
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
			ResourceManager resourceManager = new ResourceManager(typeof(CommandCollectionEditorControl));
			_btnOK = new Button();
			_lbBaseCommands = new ListBox();
			_btnCancel = new Button();
			_baseCommandsLabel = new Label();
			_btnAdd = new Button();
			_btnRemove = new Button();
			_btnAddNew = new Button();
			_lbCommands = new ListBox();
			_btnMoveUp = new Button();
			_btnMoveDown = new Button();
			_commandsLabel = new Label();
			_btnClear = new Button();
			_cboCategories = new ComboBox();
			_btnAddSeparator = new Button();
			SuspendLayout();
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(414, 340);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 24);
			_btnOK.TabIndex = 10;
			_btnOK.Text = "OK";
			_btnOK.Visible = false;
			_lbBaseCommands.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			_lbBaseCommands.Location = new Point(4, 26);
			_lbBaseCommands.Name = "lbBaseCommands";
			_lbBaseCommands.Size = new Size(226, 303);
			_lbBaseCommands.TabIndex = 0;
			_lbBaseCommands.MeasureItem += OnLbBaseCommandsMeasureItem;
			_lbBaseCommands.DrawItem += OnLbBaseCommandsDrawItem;
			_lbBaseCommands.SelectedIndexChanged += OnLbBaseCommandsSelectedIndexChanged;
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(505, 340);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 11;
			_btnCancel.Text = "Cancel";
			_btnCancel.Visible = false;
			_baseCommandsLabel.AutoSize = true;
			_baseCommandsLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_baseCommandsLabel.Location = new Point(4, 6);
			_baseCommandsLabel.Name = "label1";
			_baseCommandsLabel.Size = new Size(102, 14);
			_baseCommandsLabel.TabIndex = 7;
			_baseCommandsLabel.Text = "Base Commands:";
			_btnAdd.Image = (Bitmap)resourceManager.GetObject("btnAdd.Image");
			_btnAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnAdd.Location = new Point(240, 26);
			_btnAdd.Name = "btnAdd";
			_btnAdd.Size = new Size(112, 22);
			_btnAdd.TabIndex = 1;
			_btnAdd.Text = "         Add";
			_btnAdd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnAdd.Click += OnBtnAddClick;
			_btnRemove.Image = (Bitmap)resourceManager.GetObject("btnRemove.Image");
			_btnRemove.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnRemove.Location = new Point(240, 86);
			_btnRemove.Name = "btnRemove";
			_btnRemove.Size = new Size(112, 22);
			_btnRemove.TabIndex = 3;
			_btnRemove.Text = "         Remove";
			_btnRemove.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnRemove.Click += OnBtnRemoveClick;
			_btnAddNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_btnAddNew.Image = (Bitmap)resourceManager.GetObject("btnAddNew.Image");
			_btnAddNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnAddNew.Location = new Point(8, 340);
			_btnAddNew.Name = "btnAddNew";
			_btnAddNew.Size = new Size(134, 22);
			_btnAddNew.TabIndex = 9;
			_btnAddNew.Text = "New Command...";
			_btnAddNew.Click += OnBtnAddNewClick;
			_lbCommands.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_lbCommands.Location = new Point(358, 26);
			_lbCommands.Name = "lbCommands";
			_lbCommands.Size = new Size(230, 303);
			_lbCommands.TabIndex = 7;
			_lbCommands.MeasureItem += OnLbCommandsMeasureItem;
			_lbCommands.DrawItem += OnLbCommandsDrawItem;
			_lbCommands.SelectedIndexChanged += OnLbCommandsSelectedIndexChanged;
			_btnMoveUp.Image = (Bitmap)resourceManager.GetObject("btnMoveUp.Image");
			_btnMoveUp.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			_btnMoveUp.Location = new Point(240, 146);
			_btnMoveUp.Name = "btnMoveUp";
			_btnMoveUp.Size = new Size(112, 22);
			_btnMoveUp.TabIndex = 5;
			_btnMoveUp.Text = "     Move Up";
			_btnMoveUp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnMoveUp.Click += OnBtnMoveUpClick;
			_btnMoveDown.Image = (Bitmap)resourceManager.GetObject("btnMoveDown.Image");
			_btnMoveDown.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			_btnMoveDown.Location = new Point(240, 176);
			_btnMoveDown.Name = "btnMoveDown";
			_btnMoveDown.Size = new Size(112, 22);
			_btnMoveDown.TabIndex = 6;
			_btnMoveDown.Text = "     Move Down";
			_btnMoveDown.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnMoveDown.Click += OnBtnMoveDownClick;
			_commandsLabel.AutoSize = true;
			_commandsLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_commandsLabel.Location = new Point(362, 6);
			_commandsLabel.Name = "label2";
			_commandsLabel.Size = new Size(71, 14);
			_commandsLabel.TabIndex = 7;
			_commandsLabel.Text = "Commands:";
			_btnClear.Image = (Bitmap)resourceManager.GetObject("btnClear.Image");
			_btnClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnClear.Location = new Point(240, 116);
			_btnClear.Name = "btnClear";
			_btnClear.Size = new Size(112, 22);
			_btnClear.TabIndex = 4;
			_btnClear.Text = "      Clear";
			_btnClear.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnClear.Click += OnBtnClearClick;
			_cboCategories.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboCategories.Location = new Point(102, 2);
			_cboCategories.Name = "cboCategories";
			_cboCategories.Size = new Size(128, 21);
			_cboCategories.TabIndex = 8;
			_cboCategories.SelectedIndexChanged += OnCboCategoriesSelectedIndexChanged;
			_btnAddSeparator.Image = (Bitmap)resourceManager.GetObject("btnAddSeparator.Image");
			_btnAddSeparator.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnAddSeparator.Location = new Point(240, 56);
			_btnAddSeparator.Name = "btnAddSeparator";
			_btnAddSeparator.Size = new Size(112, 22);
			_btnAddSeparator.TabIndex = 2;
			_btnAddSeparator.Text = "      Add Separator";
			_btnAddSeparator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			_btnAddSeparator.Click += OnBtnAddSeparatorClick;
			base.Controls.AddRange(new Control[14]
			{
				_cboCategories, _btnClear, _lbCommands, _btnAddNew, _btnAdd, _baseCommandsLabel, _btnOK, _lbBaseCommands, _btnCancel, _btnRemove,
				_btnMoveUp, _btnMoveDown, _commandsLabel, _btnAddSeparator
			});
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "NewCommandPage";
			base.Size = new Size(596, 372);
			ResumeLayout(performLayout: false);
		}

		private void OnLbCommandsMeasureItem(object sender, MeasureItemEventArgs eventArgs)
		{
			try
			{
				if (_itemRenderer != null)
				{
					MxCommand uiCommand = (MxCommand)_lbCommands.Items[eventArgs.Index];
					CommandLayoutState uiCommandLayoutState = new CommandLayoutState(uiCommand, uiCommand.GetEffectiveText());
					if (uiCommand.CommandType == CommandType.Separator)
					{
						eventArgs.ItemHeight = SeparatorItemHeight;
					}
					else
					{
						eventArgs.ItemHeight = _itemRenderer.MeasureItemHeight(uiCommand, _lbCommands.Font);
					}
					_commandItems.AddItem(uiCommandLayoutState);
				}
			}
			catch
			{
			}
		}

		private void OnLbBaseCommandsMeasureItem(object sender, MeasureItemEventArgs eventArgs)
		{
			try
			{
				if (_itemRenderer != null)
				{
					MxCommand uiCommand = (MxCommand)_lbBaseCommands.Items[eventArgs.Index];
					CommandLayoutState uiCommandLayoutState = new CommandLayoutState(uiCommand, uiCommand.GetEffectiveText());
					eventArgs.ItemHeight = _itemRenderer.MeasureItemHeight(uiCommand, _lbBaseCommands.Font);
					_baseCommandItems.AddItem(uiCommandLayoutState);
				}
			}
			catch
			{
			}
		}

		private void OnLbCommandsDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			try
			{
				CommandLayoutState uiCommandLayoutState = (CommandLayoutState)_commandItems.FindItemByIndex(eventArgs.Index);
				uiCommandLayoutState.SetPressed((eventArgs.State & DrawItemState.Selected) != 0);
				_itemRenderer.DrawItem(eventArgs.Graphics, uiCommandLayoutState, eventArgs.Bounds);
			}
			catch
			{
			}
		}

		private void OnLbBaseCommandsDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			try
			{
				CommandLayoutState uiCommandLayoutState = (CommandLayoutState)_baseCommandItems.FindItemByIndex(eventArgs.Index);
				uiCommandLayoutState.SetPressed((eventArgs.State & DrawItemState.Selected) != 0);
				_itemRenderer.DrawItem(eventArgs.Graphics, uiCommandLayoutState, eventArgs.Bounds);
			}
			catch
			{
			}
		}

		protected override void OnLoad(EventArgs eventArgs)
		{
			base.OnLoad(eventArgs);
			_commandItems = new MxCollection(_lbCommands);
			_baseCommandItems = new MxCollection(_lbBaseCommands);
			_lbBaseCommands.BackColor = SystemColors.Control;
			_lbCommands.BackColor = SystemColors.Control;
			if (GetCommandManager() != null)
			{
				_itemRenderer = new PopupSettingsRenderer(GetCommandManager());
				ON item = new ON(null, "All Commands");
				_cboCategories.Items.Add(item);
				foreach (CommandCategory category in GetCommandManager().Categories)
				{
					item = new ON(category, category.CategoryName);
					_cboCategories.Items.Add(item);
				}
				_cboCategories.SelectedIndex = 0;
				_cboCategories.DisplayMember = "Name";
				_cboCategories.ValueMember = "Object";
			}
			if (_lbBaseCommands.Items.Count == 0)
			{
				_btnAdd.Enabled = false;
			}
			else
			{
				_lbBaseCommands.SelectedIndex = 0;
				_btnAdd.Enabled = true;
			}
			_lbBaseCommands.DrawMode = DrawMode.OwnerDrawVariable;
			_lbCommands.DrawMode = DrawMode.Normal;
			_lbCommands.DrawMode = DrawMode.OwnerDrawVariable;
			UpdateButtonStates();
		}

		private void RefreshCommandList()
		{
			_lbCommands.DrawMode = DrawMode.Normal;
			if (_commandItems != null)
			{
				_commandItems.ClearItems();
			}
			_lbCommands.Items.Clear();
			for (int i = 0; i < _commandCollection.Count; i++)
			{
				_lbCommands.Items.Add(_commandCollection[i]);
			}
			_lbCommands.DrawMode = DrawMode.OwnerDrawVariable;
			_lbCommands.Refresh();
			UpdateButtonStates();
		}

		private void FillBaseCommands(CommandCategory category)
		{
			_lbBaseCommands.Items.Clear();
			if (_baseCommandItems != null)
			{
				_baseCommandItems.ClearItems();
			}
			MxCommand uiCommand = null;
			MxCommandCollection uiCommandCollection = (MxCommandCollection)_selectedObject;
			_canClearAll = true;
			foreach (MxCommand item in uiCommandCollection)
			{
				if (!item.IsNotInherited())
				{
					_canClearAll = false;
					break;
				}
			}
			if (uiCommandCollection._owner is MxCommand)
			{
				uiCommand = (MxCommand)uiCommandCollection._owner;
			}
			if (category == null)
			{
				foreach (MxCommand command in GetCommandManager().Commands)
				{
					if (!command.IsSeparator() && command != uiCommand)
					{
						_lbBaseCommands.Items.Add(command);
					}
				}
			}
			else
			{
				MxCommand[] commandsByCategory = GetCommandManager().Commands.GetCommandsByCategory(category);
				foreach (MxCommand categoryCommand in commandsByCategory)
				{
					if (!categoryCommand.IsSeparator() && categoryCommand != uiCommand)
					{
						_lbBaseCommands.Items.Add(categoryCommand);
					}
				}
			}
			_lbBaseCommands.Invalidate();
		}

		public MxCommandManager GetCommandManager()
		{
			return _commandManager;
		}

		private void UpdateButtonStates()
		{
			_btnClear.Enabled = _canClearAll && _commandCollection.Count > 0;
			if (_lbCommands.SelectedIndex < 0)
			{
				_btnMoveDown.Enabled = false;
				_btnMoveUp.Enabled = false;
				_btnRemove.Enabled = false;
				return;
			}
			_btnMoveDown.Enabled = _lbCommands.SelectedIndex < _lbCommands.Items.Count - 1;
			_btnMoveUp.Enabled = _lbCommands.SelectedIndex > 0;
			MxCommand uiCommand = (MxCommand)_lbCommands.SelectedItem;
			if (uiCommand.IsNotInherited())
			{
				_btnRemove.Enabled = _lbCommands.SelectedIndex != -1;
			}
			else
			{
				_btnRemove.Enabled = false;
			}
		}

		private void LoadCommandCollection()
		{
			_commandCollection = (MxCommandCollection)_selectedObject;
			FillBaseCommands(null);
			RefreshCommandList();
		}

		bool IUIElement.CommitChanges()
		{
			if (_commandCollection != null && _commandCollection._owner is IComponent component && component.Site != null)
			{
				IComponentChangeService componentChangeService = (IComponentChangeService)component.Site.GetService(typeof(IComponentChangeService));
				if (componentChangeService != null)
				{
					componentChangeService.OnComponentChanging(component, TypeDescriptor.GetProperties(component)["Commands"]);
					componentChangeService.OnComponentChanged(component, TypeDescriptor.GetProperties(component)["Commands"], null, null);
				}
			}
			return true;
		}

		void IUIElement.SetSite(ISite site)
		{
		}

		private void RaiseRefreshTree(EventArgs eventArgs)
		{
			if (base.Events[_refreshTreeKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnLbBaseCommandsSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_lbBaseCommands.SelectedIndex < 0)
			{
				_btnAdd.Enabled = false;
			}
			else
			{
				_btnAdd.Enabled = true;
			}
		}

		private void OnBtnAddClick(object sender, EventArgs eventArgs)
		{
			if (_lbBaseCommands.SelectedItem == null)
			{
				return;
			}
			MxCommand baseCommand = (MxCommand)_lbBaseCommands.SelectedItem;
			MxCommand newCommand = new MxCommand(baseCommand);
			if (_commandCollection._owner is MxCommand ownerCommand)
			{
				foreach (MxCommand command in baseCommand.Commands)
				{
					if (command.BaseCommand == ownerCommand)
					{
						MessageBox.Show("The command cannot be added because command '" + ownerCommand.Key + "' is already a child of command '" + baseCommand.Key + "'.");
						return;
					}
				}
			}
			_commandCollection.Add(newCommand);
			RefreshCommandList();
			_lbCommands.SelectedItem = newCommand;
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void OnBtnAddSeparatorClick(object sender, EventArgs eventArgs)
		{
			MxCommand uiCommand = _commandCollection.AddSeparator();
			uiCommand.Key = "Separator";
			RefreshCommandList();
			_lbCommands.SelectedItem = uiCommand;
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void OnBtnRemoveClick(object sender, EventArgs eventArgs)
		{
			if (_lbCommands.SelectedItem != null)
			{
				MxCommand command = (MxCommand)_lbCommands.SelectedItem;
				_commandCollection.Remove(command);
				RefreshCommandList();
				RaiseRefreshTree(EventArgs.Empty);
			}
		}

		private void OnBtnClearClick(object sender, EventArgs eventArgs)
		{
			_commandCollection.Clear();
			RefreshCommandList();
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void OnBtnAddNewClick(object sender, EventArgs eventArgs)
		{
			NewCommandDialog newCommandDialog = new NewCommandDialog();
			MxCommand uiCommand = newCommandDialog.ShowAndCreateCommand(GetCommandManager(), GetCommandManager().ImageList, GetCommandManager().LargeImageList);
			if (uiCommand != null)
			{
				_commandManager.Commands.Add(uiCommand);
				uiCommand._commandManager = _commandManager;
				FillBaseCommands(null);
				_lbBaseCommands.SelectedItem = uiCommand;
				RaiseRefreshTree(EventArgs.Empty);
			}
		}

		private void OnLbCommandsSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			UpdateButtonStates();
		}

		private void OnBtnMoveUpClick(object sender, EventArgs eventArgs)
		{
			if (_lbCommands.SelectedIndex > 0)
			{
				int selectedIndex = _lbCommands.SelectedIndex;
				_commandCollection.MoveCommandUp(selectedIndex);
				RefreshCommandList();
				_lbCommands.SelectedIndex = selectedIndex - 1;
				RaiseRefreshTree(EventArgs.Empty);
			}
		}

		private void OnBtnMoveDownClick(object sender, EventArgs eventArgs)
		{
			int selectedIndex = _lbCommands.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < _commandCollection.Count - 1)
			{
				_commandCollection.MoveCommandDown(selectedIndex);
				RefreshCommandList();
				_lbCommands.SelectedIndex = selectedIndex + 1;
				RaiseRefreshTree(EventArgs.Empty);
			}
		}

		private void OnCboCategoriesSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_cboCategories.SelectedItem != null)
			{
				ON oN = (ON)_cboCategories.SelectedItem;
				CommandCategory uiCommandCategory = null;
				if (oN.Object != null)
				{
					uiCommandCategory = (CommandCategory)oN.Object;
				}
				FillBaseCommands(uiCommandCategory);
			}
		}
	}
}
