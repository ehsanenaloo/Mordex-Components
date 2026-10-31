using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	[ToolboxItem(false)]
	internal class CommandManagerEditorControl : UserControl, IUIElement
	{
		private enum CollectionType
		{
			None,
			CommandBars,
			ContextMenus,
			Categories,
			Commands,
			Panels,
			TabPage
		}

		private const int PanelIconIndex = 9;

		private const int CommandBarIconIndex = 7;

		private const int CommandIconIndex = 6;

		private const int CategoryIconIndex = 5;

		private const int ContextMenuIconIndex = 8;

		private const int TabPageIconIndex = 9;

		private const int ItemPadding = 2;

		private IContainer _components;

		private MxCommandManager _commandManager;

		private MxDockManager _dockManager;

		private MxTabControl _tabControl;

		private ICollection _selectedObject;

		private CollectionType _collectionType;

		private MxToolBar _tbOperations;

		private MxToolBarButton _addButton;

		private MxToolBarButton _removeButton;

		private MxToolBarButton _clearButton;

		private MxToolBarButton _separatorButton;

		private MxToolBarButton _moveUpButton;

		private MxToolBarButton _moveDownButton;

		private ImageList _imageList;

		private ListBox _itemListBox;

		private Splitter _splitter;

		private PropertyGrid _properties;

		private ArrayList _items;

		private EventHandler _dataChangedHandler;

		private EventHandler _disableCancelHandler;

		private Image _itemImage;


		private static object _selectedObjectChangedKey = new object();

		private static object _refreshTreeKey = new object();

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
					LoadCollection();
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
				int itemHeight = (int)_itemListBox.Font.GetHeight() + ItemPadding;
				if (_itemImage != null && _itemImage.Height + ItemPadding > itemHeight)
				{
					itemHeight = _itemImage.Height + ItemPadding;
				}
				_itemListBox.ItemHeight = itemHeight;
				_itemListBox.Refresh();
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

		internal CommandManagerEditorControl(MxCommandManager manager)
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
			_commandManager = manager;
		}

		internal CommandManagerEditorControl(MxDockManager manager)
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
			_dockManager = manager;
		}

		public CommandManagerEditorControl()
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
		}

		public void SetTabControl(MxTabControl tabControl)
		{
			_tabControl = tabControl;
			if (_tabControl != null)
			{
				_selectedObject = _tabControl.TabPages;
				LoadCollection();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void LoadCollection()
		{
			if (_selectedObject == null)
			{
				_collectionType = CollectionType.None;
			}
			else if (_selectedObject is DockPanelCollection)
			{
				_collectionType = CollectionType.Panels;
				_tbOperations.Buttons[0].ImageIndex = PanelIconIndex;
			}
			else if (_selectedObject is CommandBarCollection)
			{
				_collectionType = CollectionType.CommandBars;
				_tbOperations.Buttons[0].ImageIndex = CommandBarIconIndex;
			}
			else if (_selectedObject is MxCommandCollection)
			{
				_collectionType = CollectionType.Commands;
				_tbOperations.Buttons[0].ImageIndex = CommandIconIndex;
			}
			else if (_selectedObject is CommandCategoryCollection)
			{
				_collectionType = CollectionType.Categories;
				_tbOperations.Buttons[0].ImageIndex = CategoryIconIndex;
			}
			else if (_selectedObject is ContextMenuCollection)
			{
				_collectionType = CollectionType.ContextMenus;
				_tbOperations.Buttons[0].ImageIndex = ContextMenuIconIndex;
			}
			else if (_selectedObject is TabPageCollection)
			{
				_collectionType = CollectionType.TabPage;
				_tbOperations.Buttons[0].ImageIndex = TabPageIconIndex;
			}
			_items = new ArrayList(_selectedObject.Count);
			foreach (object item in _selectedObject)
			{
				_items.Add(item);
			}
			RebuildListItems();
			if (_itemListBox.Items.Count > 0)
			{
				_itemListBox.SelectedIndex = 0;
			}
			UpdateButtonStates();
		}

		private void RebuildListItems()
		{
			_itemListBox.DrawMode = DrawMode.Normal;
			_itemListBox.Items.Clear();
			for (int i = 0; i < _items.Count; i++)
			{
				_itemListBox.Items.Add(new ON(_items[i], FormatItemText(i, _items[i])));
			}
			_itemListBox.DrawMode = DrawMode.OwnerDrawFixed;
			_itemListBox.Invalidate();
		}

		private string FormatItemText(int itemIndex, object component)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component).Find("Key", ignoreCase: true);
			string text = "";
			if (propertyDescriptor == null)
			{
				propertyDescriptor = TypeDescriptor.GetProperties(component).Find("Name", ignoreCase: true);
			}
			if (propertyDescriptor == null)
			{
				propertyDescriptor = TypeDescriptor.GetProperties(component).Find("CategoryName", ignoreCase: true);
			}
			if (propertyDescriptor != null)
			{
				text = (string)propertyDescriptor.GetValue(component);
			}
			if (text.Length > 0)
			{
				return "[" + itemIndex + "] - '" + text + "'";
			}
			return "[" + itemIndex + "] - {" + component.GetType().Name + "}";
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		protected virtual void OnDisableCancel(EventArgs eventArgs)
		{
			if (_disableCancelHandler != null)
			{
				_disableCancelHandler(this, eventArgs);
			}
		}

		bool IUIElement.CommitChanges()
		{
			return CommitCurrentCollection();
		}

		void IUIElement.SetSite(ISite site)
		{
		}

		private bool CommitCurrentCollection()
		{
			switch (_collectionType)
			{
			case CollectionType.Panels:
				return CommitPanels();
			case CollectionType.Categories:
				return CommitCategories();
			case CollectionType.CommandBars:
				return CommitCommandBars();
			case CollectionType.Commands:
				return CommitCommands();
			case CollectionType.ContextMenus:
				return CommitContextMenus();
			default:
				return true;
			}
		}

		private bool CommitCategories()
		{
			try
			{
				CommandCategoryCollection uiCommandCategoryCollection = (CommandCategoryCollection)_selectedObject;
				uiCommandCategoryCollection.Clear();
				CommandCategory[] newCategories = new CommandCategory[_items.Count];
				_items.CopyTo(newCategories);
				uiCommandCategoryCollection.AddRange(newCategories);
				RaiseRefreshTree(EventArgs.Empty);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Mordex Windows UICommandManager");
				return false;
			}
		}

		internal bool CommitTabPages()
		{
			ArrayList arrayList = new ArrayList();
			TabPageCollection uiTabPageCollection = (TabPageCollection)_selectedObject;
			foreach (MxTabPage item in uiTabPageCollection)
			{
				arrayList.Add(item);
				item.SetOwnerCollection(null);
				item._index = -1;
			}
			uiTabPageCollection.ClearWithoutEvents();
			MxTabPage[] newPages = new MxTabPage[_items.Count];
			_items.CopyTo(newPages);
			_tabControl.SelectedTab = null;
			uiTabPageCollection.AddRange(newPages);
			foreach (MxTabPage previousPage in arrayList)
			{
				if (previousPage.GetOwnerCollection() == null)
				{
					if (previousPage.Container != null)
					{
						previousPage.Container.Remove(previousPage);
					}
					previousPage.Dispose();
				}
			}
			_tabControl.InvalidateLayout();
			return true;
		}

		private bool CommitContextMenus()
		{
			ContextMenuCollection uiContextMenuCollection = (ContextMenuCollection)_selectedObject;
			ArrayList arrayList = new ArrayList();
			foreach (MxContextMenu item in uiContextMenuCollection)
			{
				item._index = -1;
				item._commandManager = null;
				arrayList.Add(item);
			}
			uiContextMenuCollection.ClearItems();
			MxContextMenu[] newMenus = new MxContextMenu[_items.Count];
			_items.CopyTo(newMenus);
			uiContextMenuCollection.AddRange(newMenus);
			foreach (MxContextMenu previousMenu in arrayList)
			{
				if (previousMenu._commandManager == null)
				{
					uiContextMenuCollection._commandManager.DetachContextMenu(previousMenu);
				}
			}
			RaiseRefreshTree(EventArgs.Empty);
			return true;
		}

		private bool CommitPanels()
		{
			try
			{
				DockPanelCollection uiPanelCollection = (DockPanelCollection)_selectedObject;
				for (int index = uiPanelCollection.Count - 1; index >= 0; index--)
				{
					DockPanelBase uiPanelBase = uiPanelCollection[index];
					if (!_items.Contains(uiPanelBase))
					{
						uiPanelCollection.RemoveAt(index);
						IContainer container = uiPanelBase.Container;
						if (container != null)
						{
							container.Remove(uiPanelBase);
							if (uiPanelBase is MxDockPanel)
							{
								container.Remove(((MxDockPanel)uiPanelBase).InnerContainer);
							}
						}
					}
				}
				for (int i = 0; i < _items.Count; i++)
				{
					DockPanelBase panelToInsert = (DockPanelBase)_items[i];
					if (_dockManager.Container != null)
					{
						if (panelToInsert.Container == null)
						{
							_dockManager.Container.Add(panelToInsert, panelToInsert.Name);
							if (panelToInsert is MxDockPanel)
							{
								_dockManager.Container.Add(((MxDockPanel)panelToInsert).InnerContainer, panelToInsert.Name + "Container");
							}
						}
						if (panelToInsert.ContainerCaptionControl != null && panelToInsert.ContainerCaptionControl.Container == null)
						{
							_dockManager.Container.Add(panelToInsert.ContainerCaptionControl, panelToInsert.Name + "CaptionContainer");
						}
					}
					uiPanelCollection.Insert(i, panelToInsert);
				}
				RaiseRefreshTree(EventArgs.Empty);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return false;
			}
		}

		private bool CommitCommandBars()
		{
			CommandBarCollection uiCommandBarCollection = (CommandBarCollection)_selectedObject;
			ArrayList arrayList = new ArrayList();
			foreach (MxCommandBar item in uiCommandBarCollection)
			{
				item._detachedIndex = -1;
				item._commandManager = null;
				arrayList.Add(item);
			}
			uiCommandBarCollection.ClearWithoutEvents();
			MxCommandBar[] newBars = new MxCommandBar[_items.Count];
			_items.CopyTo(newBars);
			uiCommandBarCollection.AddRange(newBars);
			foreach (MxCommandBar previousBar in arrayList)
			{
				if (previousBar._commandManager == null)
				{
					((ICommandBarOwner)uiCommandBarCollection.GetOwnerCommandManager()).OnBarRemoved(previousBar);
					previousBar.Dispose();
				}
			}
			RaiseRefreshTree(EventArgs.Empty);
			return true;
		}

		private bool CommitCommands()
		{
			MxCommandCollection uiCommandCollection = (MxCommandCollection)_selectedObject;
			ArrayList arrayList = new ArrayList();
			foreach (MxCommand item in uiCommandCollection)
			{
				item.SetLayoutIndex(-1);
				item._commandManager = null;
				arrayList.Add(item);
			}
			if (uiCommandCollection._owner is MxCommandManager)
			{
				uiCommandCollection.DetachAllAndClear();
			}
			else
			{
				uiCommandCollection.Clear();
			}
			MxCommand[] newCommands = new MxCommand[_items.Count];
			_items.CopyTo(newCommands);
			if (uiCommandCollection._owner is MxCommandManager)
			{
				MxCommand[] commandsToAdd = newCommands;
				foreach (MxCommand newCommand in commandsToAdd)
				{
					uiCommandCollection.AddKeepingPositions(newCommand);
				}
				foreach (MxCommand previousCommand in arrayList)
				{
					if (previousCommand._commandManager == null)
					{
						previousCommand.Dispose();
					}
				}
			}
			else
			{
				uiCommandCollection.AddRange(newCommands);
			}
			RaiseRefreshTree(EventArgs.Empty);
			return true;
		}

		private void UpdateButtonStates()
		{
			if (_properties.SelectedObject == null)
			{
				_clearButton.Enabled = false;
				_moveDownButton.Enabled = false;
				_moveUpButton.Enabled = false;
				_removeButton.Enabled = false;
				return;
			}
			_removeButton.Enabled = _itemListBox.SelectedIndex != -1;
			if (_removeButton.Enabled)
			{
				_removeButton.Enabled = CanRemoveSelected();
			}
			_clearButton.Enabled = CanClearAll();
			_moveDownButton.Enabled = _itemListBox.SelectedIndex < _itemListBox.Items.Count - 1;
			_moveUpButton.Enabled = _itemListBox.SelectedIndex > 0;
		}

		private bool CanClearAll()
		{
			if (_items.Count == 0)
			{
				return false;
			}
			if (_selectedObject is MxCommandCollection)
			{
				MxCommandCollection uiCommandCollection = (MxCommandCollection)_selectedObject;
				foreach (MxCommand item in uiCommandCollection)
				{
					if (!item.IsNotInherited())
					{
						return false;
					}
				}
			}
			else if (_selectedObject is CommandBarCollection)
			{
				CommandBarCollection uiCommandBarCollection = (CommandBarCollection)_selectedObject;
				foreach (MxCommandBar commandBar in uiCommandBarCollection)
				{
					if (!commandBar.IsNotInheritedInDesigner())
					{
						return false;
					}
				}
			}
			else if (_selectedObject is ContextMenuCollection)
			{
				ContextMenuCollection uiContextMenuCollection = (ContextMenuCollection)_selectedObject;
				foreach (MxContextMenu contextMenu in uiContextMenuCollection)
				{
					if (!contextMenu.IsNotInherited())
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool CanRemoveSelected()
		{
			if (_selectedObject is MxCommandCollection)
			{
				MxCommand uiCommand = (MxCommand)((ON)_itemListBox.SelectedItem).Object;
				return uiCommand.IsNotInherited();
			}
			if (_selectedObject is CommandBarCollection)
			{
				MxCommandBar uiCommandBar = (MxCommandBar)((ON)_itemListBox.SelectedItem).Object;
				return uiCommandBar.IsNotInheritedInDesigner();
			}
			if (_selectedObject is ContextMenuCollection)
			{
				MxContextMenu uiContextMenu = (MxContextMenu)((ON)_itemListBox.SelectedItem).Object;
				return uiContextMenu.IsNotInherited();
			}
			return true;
		}

		private bool CanEditSelected()
		{
			if (_selectedObject is MxCommandCollection)
			{
				MxCommand uiCommand = (MxCommand)((ON)_itemListBox.SelectedItem).Object;
				return uiCommand.CanModifySelf();
			}
			if (_selectedObject is CommandBarCollection)
			{
				MxCommandBar uiCommandBar = (MxCommandBar)((ON)_itemListBox.SelectedItem).Object;
				return uiCommandBar.IsEditableInDesigner();
			}
			if (_selectedObject is ContextMenuCollection)
			{
				MxContextMenu uiContextMenu = (MxContextMenu)((ON)_itemListBox.SelectedItem).Object;
				return uiContextMenu.IsNotInheritedReadOnly();
			}
			return true;
		}

		private void AddNewItem()
		{
			switch (_collectionType)
			{
			case CollectionType.Panels:
				AddPanel();
				break;
			case CollectionType.Categories:
				AddCategory();
				break;
			case CollectionType.CommandBars:
				AddCommandBar();
				break;
			case CollectionType.Commands:
				AddCommand();
				break;
			case CollectionType.ContextMenus:
				AddContextMenu();
				break;
			case CollectionType.TabPage:
				AddTabPage();
				break;
			}
		}

		private void RemoveSelectedItem()
		{
			if (_itemListBox.SelectedItem == null)
			{
				return;
			}
			int selectedIndex = _itemListBox.SelectedIndex;
			object removedItem = ((ON)_itemListBox.SelectedItem).Object;
			_items.Remove(removedItem);
			RebuildListItems();
			if (selectedIndex < _itemListBox.Items.Count)
			{
				_itemListBox.SelectedIndex = selectedIndex;
			}
			else
			{
				_itemListBox.SelectedIndex = _itemListBox.Items.Count - 1;
				if (_itemListBox.SelectedIndex == -1)
				{
					_properties.SelectedObject = null;
					RaiseSelectedObjectChanged(EventArgs.Empty);
				}
			}
			if (_collectionType == CollectionType.CommandBars || _collectionType == CollectionType.Panels || _collectionType == CollectionType.ContextMenus)
			{
				OnDisableCancel(EventArgs.Empty);
			}
			CommitCurrentCollection();
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void ClearItems()
		{
			_items.Clear();
			RebuildListItems();
			_properties.SelectedObject = null;
			RaiseSelectedObjectChanged(EventArgs.Empty);
			CommitCurrentCollection();
			if (_collectionType == CollectionType.CommandBars || _collectionType == CollectionType.Panels || _collectionType == CollectionType.ContextMenus)
			{
				OnDisableCancel(EventArgs.Empty);
			}
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void MoveSelectedUp()
		{
			MoveItemUp(_itemListBox.SelectedIndex);
		}

		private void MoveSelectedDown()
		{
			MoveItemDown(_itemListBox.SelectedIndex);
		}

		private void RaiseRefreshTree(EventArgs eventArgs)
		{
			if (base.Events[_refreshTreeKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void RaiseSelectedObjectChanged(EventArgs eventArgs)
		{
			UpdateButtonStates();
			if (base.Events[_selectedObjectChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void MoveItemUp(int itemIndex)
		{
			if (itemIndex > 0)
			{
				object value = _items[itemIndex];
				int targetIndex = itemIndex - 1;
				_items[itemIndex] = _items[targetIndex];
				_items[targetIndex] = value;
				RebuildListItems();
				_itemListBox.SelectedIndex = targetIndex;
				_itemListBox.Refresh();
			}
		}

		private void MoveItemDown(int itemIndex)
		{
			if (itemIndex < _items.Count - 1)
			{
				object value = _items[itemIndex];
				int targetIndex = itemIndex + 1;
				_items[itemIndex] = _items[targetIndex];
				_items[targetIndex] = value;
				RebuildListItems();
				_itemListBox.SelectedIndex = targetIndex;
				_itemListBox.Refresh();
			}
		}

		private void SelectItem(object value)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (_items[i] == value)
				{
					_itemListBox.SelectedIndex = i;
					break;
				}
			}
		}

		private void AddAndSelectItem(object value)
		{
			_items.Add(value);
			RebuildListItems();
			SelectItem(value);
		}

		private void AddCommand()
		{
			MxCommandCollection uiCommandCollection = (MxCommandCollection)_selectedObject;
			MxCommand uiCommand = null;
			if (uiCommandCollection._owner is MxCommandManager)
			{
				CommitCommands();
				NewCommandDialog newCommandDialog = new NewCommandDialog();
				MxCommandManager uiCommandManager = (MxCommandManager)uiCommandCollection._owner;
				uiCommand = newCommandDialog.ShowAndCreateCommand(uiCommandManager, uiCommandManager.ImageList, uiCommandManager.LargeImageList);
				if (uiCommand != null)
				{
					uiCommand._commandManager = uiCommandManager;
				}
			}
			if (uiCommand != null)
			{
				uiCommand.SetHostElement(uiCommandCollection._owner);
				AddAndSelectItem(uiCommand);
				CommitCommands();
			}
		}

		private void AddCategory()
		{
			CommandCategory uiCommandCategory = new CommandCategory();
			uiCommandCategory.CategoryName = GenerateUniqueName(typeof(CommandCategory), "Category", 1, "CategoryName");
			AddAndSelectItem(uiCommandCategory);
			CommitCategories();
		}

		private void AddContextMenu()
		{
			MxContextMenu uiContextMenu = new MxContextMenu();
			uiContextMenu._commandManager = ((ContextMenuCollection)_selectedObject)._commandManager;
			uiContextMenu.Key = GenerateUniqueName(typeof(MxContextMenu), "ContextMenu", 1, "Key");
			AddAndSelectItem(uiContextMenu);
			CommitContextMenus();
		}

		private void AddPanel()
		{
			AddPanelWizard addPanelWizard = new AddPanelWizard();
			DockPanelBase uiPanelBase = addPanelWizard.RunWizard(_dockManager, false);
			if (uiPanelBase != null)
			{
				AddAndSelectItem(uiPanelBase);
				if (!CommitPanels())
				{
					LoadCollection();
				}
			}
		}

		private void AddTabPage()
		{
			MxTabPage uiTabPage = new MxTabPage();
			AddAndSelectItem(uiTabPage);
		}

		private void AddCommandBar()
		{
			MxCommandBar uiCommandBar = new MxCommandBar();
			uiCommandBar._commandManager = ((CommandBarCollection)_selectedObject).GetOwnerCommandManager();
			uiCommandBar.Key = GenerateUniqueName(typeof(MxCommandBar), "CommandBar", 1, "Key");
			AddAndSelectItem(uiCommandBar);
			CommitCommandBars();
		}

		private string GenerateUniqueName(Type componentType, string prefix, int number, string name)
		{
			string text = "";
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(componentType).Find(name, ignoreCase: true);
			if (propertyDescriptor != null)
			{
				text = prefix + number;
				foreach (object item in _items)
				{
					if (string.Compare((string)propertyDescriptor.GetValue(item), text, ignoreCase: true) == 0)
					{
						return GenerateUniqueName(componentType, prefix, number + 1, name);
					}
				}
			}
			return text;
		}

		private void InitializeControls()
		{
			_components = new Container();
			ResourceManager resourceManager = new ResourceManager(typeof(CommandManagerEditorControl));
			_tbOperations = new MxToolBar();
			_addButton = new MxToolBarButton();
			_removeButton = new MxToolBarButton();
			_clearButton = new MxToolBarButton();
			_separatorButton = new MxToolBarButton();
			_moveUpButton = new MxToolBarButton();
			_moveDownButton = new MxToolBarButton();
			_imageList = new ImageList(_components);
			_itemListBox = new ListBox();
			_splitter = new Splitter();
			_properties = new PropertyGrid();
			SuspendLayout();
			_tbOperations.Appearance = MxToolBarAppearance.Flat;
			_tbOperations.Buttons.AddRange(new MxToolBarButton[6] { _addButton, _removeButton, _clearButton, _separatorButton, _moveUpButton, _moveDownButton });
			_tbOperations.Divider = false;
			_tbOperations.DropDownArrows = true;
			_tbOperations.ImageList = _imageList;
			_tbOperations.Name = "tbOperations";
			_tbOperations.ShowToolTips = true;
			_tbOperations.Size = new Size(496, 23);
			_tbOperations.TabIndex = 3;
			_tbOperations.TextAlign = MxToolBarTextAlign.Right;
			_tbOperations.ButtonClick += OnTbOperationsButtonClick;
			_addButton.ImageIndex = 0;
			_addButton.Tag = "Add";
			_addButton.Text = "Add";
			_removeButton.ImageIndex = 1;
			_removeButton.Tag = "Remove";
			_removeButton.Text = "Remove";
			_clearButton.ImageIndex = 2;
			_clearButton.Tag = "Clear";
			_clearButton.Text = "Clear";
			_separatorButton.Style = MxToolBarButtonStyle.Separator;
			_moveUpButton.ImageIndex = 3;
			_moveUpButton.Tag = "Up";
			_moveDownButton.ImageIndex = 4;
			_moveDownButton.Tag = "Down";
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(16, 16);
			ImageStrip.Load(_imageList, resourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_itemListBox.Dock = DockStyle.Left;
			_itemListBox.DrawMode = DrawMode.OwnerDrawFixed;
			_itemListBox.IntegralHeight = false;
			_itemListBox.Location = new Point(0, 23);
			_itemListBox.Name = "list";
			_itemListBox.Size = new Size(168, 385);
			_itemListBox.TabIndex = 4;
			_itemListBox.Resize += OnListResize;
			_itemListBox.MouseDown += OnListMouseDown;
			_itemListBox.DrawItem += OnListDrawItem;
			_itemListBox.SelectedIndexChanged += OnListSelectedIndexChanged;
			_splitter.Location = new Point(168, 23);
			_splitter.Name = "splitter1";
			_splitter.Size = new Size(8, 385);
			_splitter.TabIndex = 6;
			_splitter.TabStop = false;
			_properties.CommandsVisibleIfAvailable = true;
			_properties.Dock = DockStyle.Fill;
			_properties.LargeButtons = false;
			_properties.LineColor = SystemColors.Control;
			_properties.Location = new Point(176, 23);
			_properties.Name = "properties";
			_properties.PropertySort = PropertySort.Alphabetical;
			_properties.Size = new Size(320, 385);
			_properties.TabIndex = 7;
			_properties.Text = "propertyGrid1";
			_properties.ToolbarVisible = false;
			_properties.ViewBackColor = SystemColors.Window;
			_properties.ViewForeColor = SystemColors.WindowText;
			_properties.PropertyValueChanged += OnPropertiesPropertyValueChanged;
			base.Controls.AddRange(new Control[4] { _properties, _splitter, _itemListBox, _tbOperations });
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "CollectionPage";
			base.Size = new Size(496, 408);
			ResumeLayout(performLayout: false);
		}

		private void OnListSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_itemListBox.SelectedIndex == -1)
			{
				_properties.SelectedObject = null;
			}
			else
			{
				_properties.SelectedObject = ((ON)_itemListBox.SelectedItem).Object;
				_properties.Enabled = CanEditSelected();
				_properties.Invalidate();
				_properties.Refresh();
			}
			RaiseSelectedObjectChanged(EventArgs.Empty);
		}

		private void OnPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			if (eventArgs.ChangedItem.Label == "Key" || eventArgs.ChangedItem.Label == "CategoryName")
			{
				int selectedIndex = _itemListBox.SelectedIndex;
				RebuildListItems();
				_itemListBox.SelectedIndex = selectedIndex;
			}
			OnDataChanged(EventArgs.Empty);
		}

		private void OnTbOperationsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			switch (eventArgs.Button.Tag as string)
			{
			case "Add":
				AddNewItem();
				break;
			case "Remove":
				RemoveSelectedItem();
				break;
			case "Clear":
				ClearItems();
				break;
			case "Up":
				MoveSelectedUp();
				RaiseRefreshTree(EventArgs.Empty);
				break;
			case "Down":
				MoveSelectedDown();
				RaiseRefreshTree(EventArgs.Empty);
				break;
			}
			OnDataChanged(EventArgs.Empty);
		}

		private void OnListDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			eventArgs.DrawBackground();
			if (eventArgs.Index > -1)
			{
				int textX = eventArgs.Bounds.X + ItemPadding;
				if (_itemImage != null)
				{
					eventArgs.Graphics.DrawImageUnscaled(_itemImage, textX, eventArgs.Bounds.Top + (eventArgs.Bounds.Height - _itemImage.Height) / 2);
					textX += _itemImage.Width + ItemPadding;
				}
				using StringFormat stringFormat = new StringFormat();
				stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				stringFormat.LineAlignment = StringAlignment.Center;
				Rectangle rectangle = new Rectangle(textX, eventArgs.Bounds.Y, eventArgs.Bounds.Right - textX, eventArgs.Bounds.Height);
				using Brush brush = new SolidBrush(eventArgs.ForeColor);
				eventArgs.Graphics.DrawString(((ON)_itemListBox.Items[eventArgs.Index]).Name, _itemListBox.Font, brush, rectangle, stringFormat);
			}
			if ((eventArgs.State & DrawItemState.Focus) != DrawItemState.None)
			{
				eventArgs.DrawFocusRectangle();
			}
		}

		private void OnListResize(object sender, EventArgs eventArgs)
		{
			_itemListBox.Invalidate();
			_itemListBox.Refresh();
		}

		private void OnListMouseDown(object sender, MouseEventArgs eventArgs)
		{
			if (_collectionType != CollectionType.Commands)
			{
				return;
			}
			int hitIndex = _itemListBox.IndexFromPoint(eventArgs.X, eventArgs.Y);
			if (hitIndex != -1)
			{
				MxCommand uiCommand = (MxCommand)((ON)_itemListBox.Items[hitIndex]).Object;
				if (uiCommand != null && _commandManager != null)
				{
					CommandDragState uiCommandCollectionLayoutState = new CommandDragState(_commandManager, uiCommand);
					uiCommandCollectionLayoutState.RunDragLoop();
				}
			}
		}
	}
}
