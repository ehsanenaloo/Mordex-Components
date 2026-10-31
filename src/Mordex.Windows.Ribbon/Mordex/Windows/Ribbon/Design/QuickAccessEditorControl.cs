using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	internal class QuickAccessEditorControl : UserControl, IRibbonItemEditor
	{
		private ArrayList _allCommands;

		private Label _availableCommandsLabel;

		private Button _btnAdd;

		private Button _btnRemove;

		private ListBox _lbAllCommands;

		private ListBox _lbQACommands;

		private Button _btnMoveUp;

		private Button _btnMoveDown;

		private Label _quickAccessLabel;

		private Button _btnClear;

		private bool _isAllCommandsLoaded = true;

		internal MxRibbon _ribbon;

		private CommandCollection _quickAccessCommands;

		private ICollection _selectedCommands;

		private EventHandler _disableCancelHandler;

		private Image _itemImage;

		private static object s_disableCancelKey = new object();

		private static object s_refreshTreeKey = new object();

		private ToolbarOptions _toolbarOptions = ToolbarOptions.All;

		private EventHandler _dataChangedHandler;

		object IRibbonItemEditor.SelectedObject
		{
			get
			{
				return _selectedCommands;
			}
			set
			{
				if (!(value is ICollection))
				{
					throw new ArgumentException();
				}
				if (_selectedCommands != value)
				{
					_selectedCommands = (ICollection)value;
					LoadQuickAccessCommands();
				}
			}
		}

		Image IRibbonItemEditor.ItemImage
		{
			get
			{
				return _itemImage;
			}
			set
			{
				_itemImage = value;
				int itemHeight = (int)_lbQACommands.Font.GetHeight() + 2;
				if (_itemImage != null && _itemImage.Height + 2 > itemHeight)
				{
					itemHeight = _itemImage.Height + 2;
				}
				_lbQACommands.ItemHeight = itemHeight;
				_lbQACommands.Refresh();
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler remainingHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, remainingHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		event EventHandler IRibbonItemEditor.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(s_disableCancelKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_disableCancelKey, value);
			}
		}

		event EventHandler IRibbonItemEditor.RefreshTree
		{
			add
			{
				base.Events.AddHandler(s_refreshTreeKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_refreshTreeKey, value);
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler remainingHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, remainingHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		public QuickAccessEditorControl(MxRibbon ribbon)
		{
			InitializeControls();
			_ribbon = ribbon;
		}

		private void InitializeControls()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(QuickAccessEditorControl));
			_lbAllCommands = new ListBox();
			_availableCommandsLabel = new Label();
			_btnAdd = new Button();
			_btnRemove = new Button();
			_lbQACommands = new ListBox();
			_btnMoveUp = new Button();
			_btnMoveDown = new Button();
			_quickAccessLabel = new Label();
			_btnClear = new Button();
			SuspendLayout();
			_lbAllCommands.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			_lbAllCommands.Location = new Point(4, 26);
			_lbAllCommands.Name = "lbAllCommands";
			_lbAllCommands.Size = new Size(226, 329);
			_lbAllCommands.TabIndex = 0;
			_lbAllCommands.DrawItem += OnLbQACommandsDrawItem;
			_lbAllCommands.SelectedIndexChanged += OnLbAllCommandsSelectedIndexChanged;
			_lbAllCommands.MeasureItem += OnLbAllCommandsMeasureItem;
			_availableCommandsLabel.AutoSize = true;
			_availableCommandsLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_availableCommandsLabel.Location = new Point(4, 6);
			_availableCommandsLabel.Name = "label1";
			_availableCommandsLabel.Size = new Size(128, 13);
			_availableCommandsLabel.TabIndex = 7;
			_availableCommandsLabel.Text = "Available Commands:";
			_btnAdd.Image = (Image)componentResourceManager.GetObject("btnAdd.Image");
			_btnAdd.ImageAlign = ContentAlignment.MiddleLeft;
			_btnAdd.Location = new Point(240, 26);
			_btnAdd.Name = "btnAdd";
			_btnAdd.Size = new Size(112, 22);
			_btnAdd.TabIndex = 1;
			_btnAdd.Text = "         Add";
			_btnAdd.TextAlign = ContentAlignment.MiddleLeft;
			_btnAdd.Click += OnBtnAddClick;
			_btnRemove.Image = (Image)componentResourceManager.GetObject("btnRemove.Image");
			_btnRemove.ImageAlign = ContentAlignment.MiddleLeft;
			_btnRemove.Location = new Point(240, 54);
			_btnRemove.Name = "btnRemove";
			_btnRemove.Size = new Size(112, 22);
			_btnRemove.TabIndex = 3;
			_btnRemove.Text = "         Remove";
			_btnRemove.TextAlign = ContentAlignment.MiddleLeft;
			_btnRemove.Click += OnBtnRemoveClick;
			_lbQACommands.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_lbQACommands.Location = new Point(358, 26);
			_lbQACommands.Name = "lbQACommands";
			_lbQACommands.Size = new Size(230, 329);
			_lbQACommands.TabIndex = 7;
			_lbQACommands.DrawItem += OnLbQACommandsDrawItem;
			_lbQACommands.SelectedIndexChanged += OnLbQACommandsSelectedIndexChanged;
			_lbQACommands.MeasureItem += OnLbQACommandsMeasureItem;
			_btnMoveUp.Image = (Image)componentResourceManager.GetObject("btnMoveUp.Image");
			_btnMoveUp.ImageAlign = ContentAlignment.BottomLeft;
			_btnMoveUp.Location = new Point(240, 110);
			_btnMoveUp.Name = "btnMoveUp";
			_btnMoveUp.Size = new Size(112, 22);
			_btnMoveUp.TabIndex = 5;
			_btnMoveUp.Text = "     Move Up";
			_btnMoveUp.TextAlign = ContentAlignment.MiddleLeft;
			_btnMoveUp.Click += OnBtnMoveUpClick;
			_btnMoveDown.Image = (Image)componentResourceManager.GetObject("btnMoveDown.Image");
			_btnMoveDown.ImageAlign = ContentAlignment.BottomLeft;
			_btnMoveDown.Location = new Point(240, 138);
			_btnMoveDown.Name = "btnMoveDown";
			_btnMoveDown.Size = new Size(112, 22);
			_btnMoveDown.TabIndex = 6;
			_btnMoveDown.Text = "     Move Down";
			_btnMoveDown.TextAlign = ContentAlignment.MiddleLeft;
			_btnMoveDown.Click += OnBtnMoveDownClick;
			_quickAccessLabel.AutoSize = true;
			_quickAccessLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_quickAccessLabel.Location = new Point(364, 6);
			_quickAccessLabel.Name = "label2";
			_quickAccessLabel.Size = new Size(149, 13);
			_quickAccessLabel.TabIndex = 7;
			_quickAccessLabel.Text = "Quick Access Commands:";
			_btnClear.Image = (Image)componentResourceManager.GetObject("btnClear.Image");
			_btnClear.ImageAlign = ContentAlignment.MiddleLeft;
			_btnClear.Location = new Point(240, 82);
			_btnClear.Name = "btnClear";
			_btnClear.Size = new Size(112, 22);
			_btnClear.TabIndex = 4;
			_btnClear.Text = "      Clear";
			_btnClear.TextAlign = ContentAlignment.MiddleLeft;
			_btnClear.Click += OnBtnClearClick;
			base.Controls.Add(_btnClear);
			base.Controls.Add(_lbQACommands);
			base.Controls.Add(_btnAdd);
			base.Controls.Add(_availableCommandsLabel);
			base.Controls.Add(_lbAllCommands);
			base.Controls.Add(_btnRemove);
			base.Controls.Add(_btnMoveUp);
			base.Controls.Add(_btnMoveDown);
			base.Controls.Add(_quickAccessLabel);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "NewCommandPage";
			base.Size = new Size(596, 372);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		private void OnLbQACommandsMeasureItem(object sender, MeasureItemEventArgs eventArgs)
		{
			if (_ribbon != null)
			{
				eventArgs.ItemHeight = _ribbon.GetSmallItemHeight();
			}
		}

		private void OnLbAllCommandsMeasureItem(object sender, MeasureItemEventArgs eventArgs)
		{
			if (_ribbon != null)
			{
				eventArgs.ItemHeight = _ribbon.GetSmallItemHeight();
			}
		}

		private void OnLbQACommandsDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			ListBox listBox = (ListBox)sender;
			if (_ribbon == null || eventArgs.Index < 0 || eventArgs.Index >= listBox.Items.Count)
			{
				return;
			}
			CommandBase commandBase = (CommandBase)listBox.Items[eventArgs.Index];
			bool isSelected = (eventArgs.State & DrawItemState.Selected) != 0;
			Size smallImageSize = _ribbon.SmallImageSize;
			int iconColumnWidth = smallImageSize.Width + 8;
			Rectangle rectangle = new Rectangle(eventArgs.Bounds.X + 4, eventArgs.Bounds.Top + 4, smallImageSize.Width, smallImageSize.Height);
			Rectangle textBounds = new Rectangle(eventArgs.Bounds.X + iconColumnWidth + 7, eventArgs.Bounds.Top, eventArgs.Bounds.Width - iconColumnWidth - 7, eventArgs.Bounds.Height);
			using (Brush brush = new SolidBrush(_ribbon._colorScheme._menuBackground))
			{
				eventArgs.Graphics.FillRectangle(brush, new Rectangle(eventArgs.Bounds.X, eventArgs.Bounds.Y, iconColumnWidth, eventArgs.Bounds.Height));
			}
			if (isSelected)
			{
				RibbonChromePainter.DrawHotButtonBackground(eventArgs.Graphics, eventArgs.Bounds, _ribbon._colorScheme, true);
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
			try
			{
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				textLayoutRenderer.RenderContent(eventArgs.Graphics, commandBase.Name, textBounds, new RibbonForegroundStyle(null), _ribbon.Font, _ribbon.ForeColor, SystemColors.GrayText, commandBase.IsEnabled);
			}
			finally
			{
				textLayoutRenderer.Dispose();
			}
			if (commandBase is ButtonCommand)
			{
				ButtonCommand buttonCommand = (ButtonCommand)commandBase;
				RibbonDrawingUtils.DrawButtonImage(eventArgs.Graphics, buttonCommand, _ribbon.LargeImageList, _ribbon.ImageList, rectangle, CommandSizeStyle.Small, false, _ribbon._colorScheme);
			}
			else if (commandBase is RibbonGroup)
			{
				RibbonGroup ribbonGroup = (RibbonGroup)commandBase;
				RibbonDrawingUtils.DrawImageFromSources(eventArgs.Graphics, ribbonGroup.Icon, ribbonGroup.Image, ribbonGroup.ImageIndex, ribbonGroup.ImageKey, _ribbon.ImageList, rectangle, commandBase.IsEnabled, false, _ribbon._colorScheme);
			}
		}

		protected override void OnLoad(EventArgs eventArgs)
		{
			base.OnLoad(eventArgs);
			_lbQACommands.BackColor = _ribbon._colorScheme._dialogBackground;
			_lbAllCommands.BackColor = _ribbon._colorScheme._dialogBackground;
			if (_lbAllCommands.Items.Count == 0)
			{
				_btnAdd.Enabled = false;
			}
			else
			{
				_lbAllCommands.SelectedIndex = 0;
				_btnAdd.Enabled = true;
			}
			_lbAllCommands.DrawMode = DrawMode.OwnerDrawVariable;
			_lbQACommands.DrawMode = DrawMode.Normal;
			_lbQACommands.DrawMode = DrawMode.OwnerDrawVariable;
			RefreshButtonStates();
		}

		private void RefreshQuickAccessList()
		{
			_lbQACommands.DrawMode = DrawMode.Normal;
			_lbQACommands.Items.Clear();
			for (int i = 0; i < _quickAccessCommands.Count; i++)
			{
				_lbQACommands.Items.Add(_quickAccessCommands[i]);
			}
			_lbQACommands.DrawMode = DrawMode.OwnerDrawVariable;
			_lbQACommands.Refresh();
			RefreshButtonStates();
		}

		private void AddCommandsRecursively(CommandCollection commandCollection)
		{
			foreach (CommandBase item in commandCollection)
			{
				if (item.CommandType != CommandType.BackstageTab)
				{
					_allCommands.Add(item);
					if (item is DropDownCommand)
					{
						AddCommandsRecursively(((DropDownCommand)item).Commands);
					}
					if (item is ButtonGroup)
					{
						AddCommandsRecursively(((ButtonGroup)item).Commands);
					}
					if (item is CommandBox)
					{
						AddCommandsRecursively(((CommandBox)item).Commands);
					}
				}
			}
		}

		private void RebuildAllCommandsList()
		{
			_allCommands = new ArrayList();
			foreach (RibbonTab tab in _ribbon.Tabs)
			{
				foreach (RibbonGroup group in tab.Groups)
				{
					_allCommands.Add(group);
				}
				foreach (RibbonGroup commandGroup in tab.Groups)
				{
					AddCommandsRecursively(commandGroup.Commands);
				}
			}
			AddCommandsRecursively(_ribbon.ControlBoxMenu.LeftCommands);
			AddCommandsRecursively(_ribbon.ControlBoxMenu.RightCommands);
			AddCommandsRecursively(_ribbon.ControlBoxMenu.BottomCommands);
			AddCommandsRecursively(_ribbon.BackstageCommands);
			CommandKeyComparer comparer = new CommandKeyComparer();
			_allCommands.Sort(comparer);
			_lbAllCommands.DataSource = _allCommands;
			_isAllCommandsLoaded = true;
			_lbAllCommands.Invalidate();
		}

		private void RefreshButtonStates()
		{
			_btnClear.Enabled = _isAllCommandsLoaded && _quickAccessCommands != null && _quickAccessCommands.Count > 0;
			if (_lbQACommands.SelectedIndex < 0)
			{
				_btnMoveDown.Enabled = false;
				_btnMoveUp.Enabled = false;
				_btnRemove.Enabled = false;
				return;
			}
			_btnMoveDown.Enabled = _lbQACommands.SelectedIndex < _lbQACommands.Items.Count - 1;
			_btnMoveUp.Enabled = _lbQACommands.SelectedIndex > 0;
			CommandBase commandBase = (CommandBase)_lbQACommands.SelectedItem;
			if (commandBase.IsNotInherited())
			{
				_btnRemove.Enabled = _lbQACommands.SelectedIndex != -1;
			}
			else
			{
				_btnRemove.Enabled = false;
			}
		}

		private void LoadQuickAccessCommands()
		{
			_quickAccessCommands = (CommandCollection)_selectedCommands;
			RebuildAllCommandsList();
			RefreshQuickAccessList();
		}

		protected virtual void OnDisableCancel(EventArgs eventArgs)
		{
			if (_disableCancelHandler != null)
			{
				_disableCancelHandler(this, eventArgs);
			}
		}

		bool IRibbonItemEditor.CommitChanges()
		{
			if (_ribbon != null)
			{
				IComponentChangeService componentChangeService = _ribbon.Site?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
				if (componentChangeService != null)
				{
					componentChangeService.OnComponentChanging(_ribbon, TypeDescriptor.GetProperties(_ribbon)["QuickAccessCommands"]);
					componentChangeService.OnComponentChanged(_ribbon, TypeDescriptor.GetProperties(_ribbon)["QuickAccessCommands"], null, null);
				}
			}
			return true;
		}

		void IRibbonItemEditor.SetSite(ISite site)
		{
		}

		private void RaiseRefreshTree(EventArgs eventArgs)
		{
			if (base.Events[s_refreshTreeKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		bool IRibbonItemEditor.IsOptionSupported(ToolbarOptions option)
		{
			bool isSupported = (option & _toolbarOptions) != 0;
			if (isSupported)
			{
				switch (option)
				{
				case ToolbarOptions.Clear:
					if (_quickAccessCommands.Count == 0)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.MoveDown:
					if (_lbQACommands.SelectedIndex == _lbQACommands.Items.Count - 1)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.MoveUp:
					if (_lbQACommands.SelectedIndex < 1)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.Remove:
					if (_lbQACommands.SelectedIndex == -1)
					{
						isSupported = false;
					}
					break;
				}
			}
			return isSupported;
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		private void OnLbAllCommandsSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_lbAllCommands.SelectedIndex < 0)
			{
				_btnAdd.Enabled = false;
			}
			else
			{
				_btnAdd.Enabled = true;
			}
			_lbAllCommands.Invalidate();
		}

		private void OnBtnAddClick(object sender, EventArgs eventArgs)
		{
			if (_lbAllCommands.SelectedItem != null)
			{
				CommandBase commandBase = (CommandBase)_lbAllCommands.SelectedItem;
				_quickAccessCommands.Add(commandBase);
				RefreshQuickAccessList();
				_lbQACommands.SelectedItem = commandBase;
				RaiseRefreshTree(EventArgs.Empty);
			}
		}

		private void OnBtnRemoveClick(object sender, EventArgs eventArgs)
		{
			if (_lbQACommands.SelectedItem != null)
			{
				CommandBase command = (CommandBase)_lbQACommands.SelectedItem;
				_quickAccessCommands.Remove(command);
				RefreshQuickAccessList();
				RaiseRefreshTree(EventArgs.Empty);
			}
		}

		private void OnBtnClearClick(object sender, EventArgs eventArgs)
		{
			_quickAccessCommands.Clear();
			RefreshQuickAccessList();
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void OnLbQACommandsSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			RefreshButtonStates();
			_lbQACommands.Invalidate();
		}

		private void OnBtnMoveUpClick(object sender, EventArgs eventArgs)
		{
			if (_lbQACommands.SelectedIndex > 0)
			{
				int selectedIndex = _lbQACommands.SelectedIndex;
				_quickAccessCommands.MoveCommandTo(selectedIndex - 1, (CommandBase)_lbQACommands.SelectedItem);
				RefreshQuickAccessList();
				_lbQACommands.SelectedIndex = selectedIndex - 1;
				RaiseRefreshTree(EventArgs.Empty);
			}
		}

		private void OnBtnMoveDownClick(object sender, EventArgs eventArgs)
		{
			int selectedIndex = _lbQACommands.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < _quickAccessCommands.Count - 1)
			{
				_quickAccessCommands.MoveCommandTo(selectedIndex + 1, (CommandBase)_lbQACommands.SelectedItem);
				RefreshQuickAccessList();
				_lbQACommands.SelectedIndex = selectedIndex + 1;
				RaiseRefreshTree(EventArgs.Empty);
			}
		}
	}
}
