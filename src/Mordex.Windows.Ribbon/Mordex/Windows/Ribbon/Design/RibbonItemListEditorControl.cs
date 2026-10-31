using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	[ToolboxItem(false)]
	internal class RibbonItemListEditorControl : UserControl, IRibbonItemEditor
	{
		private enum CollectionType
		{
			None,
			Commands,
			Groups,
			Tabs,
			ContextualTabs,
			GalleryItems
		}

		private const int ImageButtonGroup = 10;

		private const int ImageCheckBox = 11;

		private const int ImageComboBox = 12;

		private const int ImageCommand = 13;

		private const int ImageCommandBox = 14;

		private const int ImageDropDown = 15;

		private const int ImageContainerControl = 16;

		private const int ImageTextBox = 17;

		private const int ImageGallery = 18;

		private const int ImageLabel = 19;

		private const int ImageSeparator = 20;

		private const int ListItemImagePadding = 2;

		private IContainer _components;

		private MxRibbon _ribbon;

		private MxRibbonStatusBar _statusBar;

		private ToolStripDropDownButton _tbtnAdd;

		private ToolbarOptions _toolbarOptions = ToolbarOptions.All;

		private ICollection _editedCollection;

		private CollectionType _collectionType;

		private ToolStrip _tbOperations;

		private ToolStripButton _tbtnRemove;

		private ToolStripButton _tbtnClear;

		private ToolStripSeparator _toolbarSeparator;

		private ToolStripButton _tbtnMoveUp;

		private ToolStripButton _tbtnMoveDown;

		private ImageList _commandTypeImageList;

		private ListBox _list;

		private Splitter _listSplitter;

		private PropertyGrid _properties;

		private ArrayList _workingItems;

		private EventHandler _dataChangedHandler;

		private EventHandler _disableCancelHandler;

		private Image _listItemImage;

		private static object s_selectedObjectChangedKey = new object();

		private static object s_refreshTreeKey = new object();

		object IRibbonItemEditor.SelectedObject
		{
			get
			{
				return _editedCollection;
			}
			set
			{
				if (!(value is ICollection))
				{
					throw new ArgumentException();
				}
				if (_editedCollection != value)
				{
					_editedCollection = (ICollection)value;
					LoadSelectedObject();
				}
			}
		}

		Image IRibbonItemEditor.ItemImage
		{
			get
			{
				return _listItemImage;
			}
			set
			{
				_listItemImage = value;
				int itemHeight = (int)_list.Font.GetHeight() + 2;
				if (_listItemImage != null && _listItemImage.Height + 2 > itemHeight)
				{
					itemHeight = _listItemImage.Height + 2;
				}
				_list.ItemHeight = itemHeight;
				_list.Refresh();
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, combinedHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler reducedHandler = (EventHandler)Delegate.Remove(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, reducedHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, combinedHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
			remove
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler reducedHandler = (EventHandler)Delegate.Remove(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, reducedHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
		}

		event EventHandler IRibbonItemEditor.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(s_selectedObjectChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_selectedObjectChangedKey, value);
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

		internal RibbonItemListEditorControl(MxRibbonStatusBar statusBar)
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
			_statusBar = statusBar;
		}

		internal RibbonItemListEditorControl(MxRibbon ribbon)
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
			_ribbon = ribbon;
		}

		public RibbonItemListEditorControl()
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ClearAddMenu();
				_components?.Dispose();
			}
			base.Dispose(disposing);
		}

		internal void InvalidateTargetLayout()
		{
			if (_ribbon != null)
			{
				_ribbon.InvalidateFullLayout();
			}
			else
			{
				_statusBar?.RebuildPanelLayouts();
			}
		}

		private void AddMenuItem(string text, string name, int imageIndex, EventHandler handler)
		{
			ToolStripMenuItem item = new ToolStripMenuItem(text);
			item.Image = _commandTypeImageList.Images[imageIndex];
			item.Name = name;
			item.Click += handler;
			_tbtnAdd.DropDownItems.Add(item);
		}

		private void ClearAddMenu()
		{
			for (int i = _tbtnAdd.DropDownItems.Count - 1; i >= 0; i--)
			{
				ToolStripItem item = _tbtnAdd.DropDownItems[i];
				_tbtnAdd.DropDownItems.RemoveAt(i);
				// Images taken from the image list are copies owned by the item.
				item.Image?.Dispose();
				item.Dispose();
			}
		}

		private void RebuildAddMenu()
		{
			ClearAddMenu();
			if (_editedCollection is CommandCollection commandCollection)
			{
				ICommandContainer parent = commandCollection._parent;
				if (parent.CanBeParentTo(CommandType.StatusBarPanel) && _statusBar != null)
				{
					AddMenuItem("StatusBar Panel", "Panel", ImageCommand, OnAddStatusBarPanelClick);
				}
				if (parent.CanBeParentTo(CommandType.BackstageTab))
				{
					AddMenuItem("Backstage Tab", "Tab", ImageCommand, OnAddBackstageTabClick);
				}
				if (parent.CanBeParentTo(CommandType.SmallButton))
				{
					AddMenuItem("Button Command", "Button", ImageCommand, OnAddButtonCommandClick);
				}
				if (parent.CanBeParentTo(CommandType.DropDownButton))
				{
					AddMenuItem("DropDown Command", "DropDownButton", ImageDropDown, OnAddDropDownClick);
				}
				if (parent.CanBeParentTo(CommandType.ButtonGroup))
				{
					AddMenuItem("Button Group", "ButtonGroup", ImageButtonGroup, OnAddButtonGroupClick);
				}
				if (parent.CanBeParentTo(CommandType.Box))
				{
					AddMenuItem("Command Box", "Box", ImageCommandBox, OnAddCommandBoxClick);
				}
				if (parent.CanBeParentTo(CommandType.Separator))
				{
					AddMenuItem("Separator", "Separator", ImageSeparator, OnControlClick);
				}
				if (parent.CanBeParentTo(CommandType.Label))
				{
					AddMenuItem("Label", "Label", ImageLabel, OnAddLabelClick);
				}
				if (parent.CanBeParentTo(CommandType.ContainerControl))
				{
					AddMenuItem("ComboBox", "ComboBox", ImageComboBox, OnAddComboBoxClick);
					AddMenuItem("CheckBox", "CheckBox", ImageCheckBox, OnAddCheckBoxClick);
					AddMenuItem("TextBox", "TextBox", ImageTextBox, OnAddTextBoxClick);
					AddMenuItem("ContainerControl", "ContainerControl", ImageContainerControl, OnAddContainerControlClick);
				}
				if (parent.CanBeParentTo(CommandType.Gallery))
				{
					AddMenuItem("Gallery", "Gallery", ImageGallery, OnAddGalleryClick);
				}
				_tbtnAdd.ShowDropDownArrow = true;
			}
			else
			{
				_tbtnAdd.DropDown = null;
				_tbtnAdd.ShowDropDownArrow = false;
			}
		}

		private void LoadSelectedObject()
		{
			if (_editedCollection == null)
			{
				_collectionType = CollectionType.None;
			}
			else if (_editedCollection is CommandCollection)
			{
				_collectionType = CollectionType.Commands;
			}
			else if (_editedCollection is RibbonGroupCollection)
			{
				_collectionType = CollectionType.Groups;
			}
			else if (_editedCollection is RibbonTabCollection)
			{
				_collectionType = CollectionType.Tabs;
			}
			else if (_editedCollection is RibbonContextualTabCollection)
			{
				_collectionType = CollectionType.ContextualTabs;
			}
			else if (_editedCollection is GalleryItemCollection)
			{
				_collectionType = CollectionType.GalleryItems;
			}
			RebuildAddMenu();
			_workingItems = new ArrayList(_editedCollection.Count);
			foreach (object item in _editedCollection)
			{
				_workingItems.Add(item);
			}
			RefreshListItems();
			if (_list.Items.Count > 0)
			{
				_list.SelectedIndex = 0;
			}
			UpdateToolbarState();
		}

		private void RefreshListItems()
		{
			_list.DrawMode = DrawMode.Normal;
			_list.Items.Clear();
			for (int i = 0; i < _workingItems.Count; i++)
			{
				_list.Items.Add(new DictionaryEntry(BuildListItemText(i, _workingItems[i]), _workingItems[i]));
			}
			_list.DrawMode = DrawMode.OwnerDrawFixed;
			_list.Invalidate();
		}

		private string BuildListItemText(int index, object component)
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
				text = propertyDescriptor.GetValue(component) as string ?? "";
			}
			if (text.Length > 0)
			{
				return "[" + index + "] - '" + text + "'";
			}
			return "[" + index + "] - {" + component.GetType().Name + "}";
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

		bool IRibbonItemEditor.CommitChanges()
		{
			return CommitCollection();
		}

		void IRibbonItemEditor.SetSite(ISite site)
		{
		}

		private bool CommitCollection()
		{
			switch (_collectionType)
			{
			case CollectionType.Commands:
				return CommitCommands();
			case CollectionType.Groups:
				return CommitGroups();
			case CollectionType.ContextualTabs:
				return CommitContextualTabs();
			case CollectionType.GalleryItems:
				return CommitGalleryItems();
			case CollectionType.Tabs:
				return CommitTabs();
			default:
				return true;
			}
		}

		private bool CommitCommands()
		{
			CommandCollection commandCollection = (CommandCollection)_editedCollection;
			ArrayList arrayList = new ArrayList();
			foreach (CommandBase item in commandCollection)
			{
				arrayList.Add(item);
			}
			commandCollection.RemoveAllAndClearOwners();
			CommandBase[] newItems = new CommandBase[_workingItems.Count];
			_workingItems.CopyTo(newItems);
			commandCollection.AddRange(newItems);
			foreach (CommandBase originalCommand in arrayList)
			{
				if (!commandCollection.Contains(originalCommand))
				{
					originalCommand.Dispose();
				}
			}
			InvalidateTargetLayout();
			RaiseRefreshTree(EventArgs.Empty);
			return true;
		}

		private bool CommitGroups()
		{
			RibbonGroupCollection ribbonGroupCollection = (RibbonGroupCollection)_editedCollection;
			ArrayList arrayList = new ArrayList();
			foreach (RibbonGroup item in ribbonGroupCollection)
			{
				arrayList.Add(item);
			}
			ribbonGroupCollection.DetachAll();
			RibbonGroup[] newItems = new RibbonGroup[_workingItems.Count];
			_workingItems.CopyTo(newItems);
			ribbonGroupCollection.AddRange(newItems);
			foreach (RibbonGroup originalGroup in arrayList)
			{
				if (!ribbonGroupCollection.Contains(originalGroup))
				{
					originalGroup.Dispose();
				}
			}
			InvalidateTargetLayout();
			RaiseRefreshTree(EventArgs.Empty);
			return true;
		}

		private bool CommitContextualTabs()
		{
			RibbonContextualTabCollection ribbonContextualTabCollection = (RibbonContextualTabCollection)_editedCollection;
			ArrayList arrayList = new ArrayList();
			foreach (RibbonContextualTab item in ribbonContextualTabCollection)
			{
				arrayList.Add(item);
			}
			ribbonContextualTabCollection.RemoveAllWithoutEvents();
			RibbonContextualTab[] newItems = new RibbonContextualTab[_workingItems.Count];
			_workingItems.CopyTo(newItems);
			ribbonContextualTabCollection.AddRange(newItems);
			foreach (RibbonContextualTab originalContextualTab in arrayList)
			{
				if (!ribbonContextualTabCollection.Contains(originalContextualTab))
				{
					originalContextualTab.Dispose();
				}
			}
			InvalidateTargetLayout();
			RaiseRefreshTree(EventArgs.Empty);
			return true;
		}

		private bool CommitGalleryItems()
		{
			GalleryItemCollection galleryItemCollection = (GalleryItemCollection)_editedCollection;
			ArrayList arrayList = new ArrayList();
			foreach (GalleryItem item in galleryItemCollection)
			{
				arrayList.Add(item);
			}
			galleryItemCollection.RemoveAllWithoutEvents();
			GalleryItem[] newItems = new GalleryItem[_workingItems.Count];
			_workingItems.CopyTo(newItems);
			galleryItemCollection.AddRange(newItems);
			foreach (GalleryItem originalGalleryItem in arrayList)
			{
				if (!galleryItemCollection.Contains(originalGalleryItem))
				{
					originalGalleryItem.Dispose();
				}
			}
			InvalidateTargetLayout();
			RaiseRefreshTree(EventArgs.Empty);
			return true;
		}

		private bool CommitTabs()
		{
			RibbonTabCollection ribbonTabCollection = (RibbonTabCollection)_editedCollection;
			ArrayList arrayList = new ArrayList();
			foreach (RibbonTab item in ribbonTabCollection)
			{
				arrayList.Add(item);
			}
			ribbonTabCollection.DetachAll();
			RibbonTab[] newItems = new RibbonTab[_workingItems.Count];
			_workingItems.CopyTo(newItems);
			ribbonTabCollection.AddRange(newItems);
			foreach (RibbonTab originalTab in arrayList)
			{
				if (!ribbonTabCollection.Contains(originalTab))
				{
					originalTab.Dispose();
				}
			}
			InvalidateTargetLayout();
			RaiseRefreshTree(EventArgs.Empty);
			return true;
		}

		bool IRibbonItemEditor.IsOptionSupported(ToolbarOptions option)
		{
			bool isSupported = (option & _toolbarOptions) != 0;
			if (isSupported)
			{
				switch (option)
				{
				case ToolbarOptions.Clear:
					if (_workingItems.Count == 0)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.MoveDown:
					if (_list.SelectedIndex == _list.Items.Count - 1)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.MoveUp:
					if (_list.SelectedIndex < 1)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.Remove:
					if (_list.SelectedIndex == -1)
					{
						isSupported = false;
					}
					break;
				}
			}
			return isSupported;
		}

		private void UpdateToolbarState()
		{
			if (_properties.SelectedObject == null)
			{
				_tbtnClear.Enabled = false;
				_tbtnMoveDown.Enabled = false;
				_tbtnMoveUp.Enabled = false;
				_tbtnRemove.Enabled = false;
				return;
			}
			_tbtnRemove.Enabled = _list.SelectedIndex != -1;
			if (_tbtnRemove.Enabled)
			{
				_tbtnRemove.Enabled = CanRemoveSelected();
			}
			_tbtnClear.Enabled = CanRemoveAll();
			_tbtnMoveDown.Enabled = _list.SelectedIndex < _list.Items.Count - 1;
			_tbtnMoveUp.Enabled = _list.SelectedIndex > 0;
		}

		private bool CanRemoveAll()
		{
			if (_workingItems.Count == 0)
			{
				return false;
			}
			if (_editedCollection is CommandCollection)
			{
				CommandCollection commandCollection = (CommandCollection)_editedCollection;
				foreach (CommandBase item in commandCollection)
				{
					if (!item.IsNotInherited())
					{
						return false;
					}
				}
			}
			else if (_editedCollection is RibbonGroupCollection)
			{
				RibbonGroupCollection ribbonGroupCollection = (RibbonGroupCollection)_editedCollection;
				foreach (RibbonGroup group in ribbonGroupCollection)
				{
					if (!group.IsNotInherited())
					{
						return false;
					}
				}
			}
			else if (_editedCollection is RibbonTabCollection)
			{
				RibbonTabCollection ribbonTabCollection = (RibbonTabCollection)_editedCollection;
				foreach (RibbonTab tab in ribbonTabCollection)
				{
					if (!tab.IsNotInherited())
					{
						return false;
					}
				}
			}
			else if (_editedCollection is RibbonContextualTabCollection)
			{
				RibbonContextualTabCollection ribbonContextualTabCollection = (RibbonContextualTabCollection)_editedCollection;
				foreach (RibbonContextualTab contextualTab in ribbonContextualTabCollection)
				{
					if (!contextualTab.IsNotInherited())
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool CanRemoveSelected()
		{
			if (_editedCollection is CommandCollection)
			{
				CommandBase commandBase = (CommandBase)((DictionaryEntry)_list.SelectedItem).Value;
				return commandBase.IsNotInherited();
			}
			if (_editedCollection is RibbonGroupCollection)
			{
				RibbonGroup ribbonGroup = (RibbonGroup)((DictionaryEntry)_list.SelectedItem).Value;
				return ribbonGroup.IsNotInherited();
			}
			if (_editedCollection is RibbonTabCollection)
			{
				RibbonTab ribbonTab = (RibbonTab)((DictionaryEntry)_list.SelectedItem).Value;
				return ribbonTab.IsNotInherited();
			}
			if (_editedCollection is RibbonContextualTabCollection)
			{
				RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)((DictionaryEntry)_list.SelectedItem).Value;
				return ribbonContextualTab.IsNotInherited();
			}
			return true;
		}

		private bool CanEditSelected()
		{
			if (_editedCollection is CommandCollection)
			{
				CommandBase commandBase = (CommandBase)((DictionaryEntry)_list.SelectedItem).Value;
				return commandBase.IsEditable();
			}
			if (_editedCollection is RibbonGroupCollection)
			{
				RibbonGroup ribbonGroup = (RibbonGroup)((DictionaryEntry)_list.SelectedItem).Value;
				return ribbonGroup.IsEditable();
			}
			if (_editedCollection is RibbonTabCollection)
			{
				RibbonTab ribbonTab = (RibbonTab)((DictionaryEntry)_list.SelectedItem).Value;
				return ribbonTab.IsEditable();
			}
			if (_editedCollection is RibbonContextualTabCollection)
			{
				RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)((DictionaryEntry)_list.SelectedItem).Value;
				return ribbonContextualTab.IsEditable();
			}
			return true;
		}

		private void AddNewItem()
		{
			switch (_collectionType)
			{
			case CollectionType.Tabs:
				AddNewTab();
				break;
			case CollectionType.GalleryItems:
				AddNewGalleryItem();
				break;
			case CollectionType.ContextualTabs:
				AddNewContextualTab();
				break;
			case CollectionType.Commands:
				if (_ribbon != null)
				{
					AddNamedCommand(new ButtonCommand(), "ButtonCommand");
				}
				else
				{
					AddNamedCommand(new StatusBarPanel(), "StatusBarPanel");
				}
				break;
			case CollectionType.Groups:
				AddNewGroup();
				break;
			}
		}

		private void RemoveSelectedItem()
		{
			if (_list.SelectedItem == null)
			{
				return;
			}
			int selectedIndex = _list.SelectedIndex;
			object value = ((DictionaryEntry)_list.SelectedItem).Value;
			_workingItems.Remove(value);
			RefreshListItems();
			if (selectedIndex < _list.Items.Count)
			{
				_list.SelectedIndex = selectedIndex;
			}
			else
			{
				_list.SelectedIndex = _list.Items.Count - 1;
				if (_list.SelectedIndex == -1)
				{
					_properties.SelectedObject = null;
					RaiseSelectedObjectChanged(EventArgs.Empty);
				}
			}
			CommitCollection();
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void ClearItems()
		{
			_workingItems.Clear();
			RefreshListItems();
			_properties.SelectedObject = null;
			RaiseSelectedObjectChanged(EventArgs.Empty);
			CommitCollection();
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void MoveSelectedUp()
		{
			MoveItemUp(_list.SelectedIndex);
		}

		private void MoveSelectedDown()
		{
			MoveItemDown(_list.SelectedIndex);
		}

		private void RaiseRefreshTree(EventArgs eventArgs)
		{
			if (base.Events[s_refreshTreeKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void RaiseSelectedObjectChanged(EventArgs eventArgs)
		{
			UpdateToolbarState();
			if (base.Events[s_selectedObjectChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void MoveItemUp(int index)
		{
			if (index > 0)
			{
				object value = _workingItems[index];
				int previousIndex = index - 1;
				_workingItems[index] = _workingItems[previousIndex];
				_workingItems[previousIndex] = value;
				RefreshListItems();
				_list.SelectedIndex = previousIndex;
				_list.Refresh();
			}
		}

		private void MoveItemDown(int index)
		{
			if (index >= 0 && index < _workingItems.Count - 1)
			{
				object value = _workingItems[index];
				int nextIndex = index + 1;
				_workingItems[index] = _workingItems[nextIndex];
				_workingItems[nextIndex] = value;
				RefreshListItems();
				_list.SelectedIndex = nextIndex;
				_list.Refresh();
			}
		}

		private void SelectItemInList(object value)
		{
			for (int i = 0; i < _workingItems.Count; i++)
			{
				if (_workingItems[i] == value)
				{
					_list.SelectedIndex = i;
					break;
				}
			}
		}

		private void AddAndSelectItem(object value)
		{
			_workingItems.Add(value);
			RefreshListItems();
			SelectItemInList(value);
		}

		private void AddNamedCommand(CommandBase commandBase, string text)
		{
			commandBase.Key = BuildUniqueName(typeof(CommandBase), text, 1, "Key");
			commandBase._directRibbon = _ribbon;
			AddAndSelectItem(commandBase);
		}

		private void AddNewTab()
		{
			RibbonTab ribbonTab = new RibbonTab();
			ribbonTab.Key = BuildUniqueName(typeof(RibbonTab), "RibbonTab", 1, "Key");
			ribbonTab.Text = BuildUniqueName(typeof(RibbonTab), "Tab ", 1, "Text");
			AddAndSelectItem(ribbonTab);
		}

		private void AddNewGroup()
		{
			RibbonGroup ribbonGroup = new RibbonGroup();
			ribbonGroup._directRibbon = _ribbon;
			ribbonGroup.Key = BuildUniqueName(typeof(RibbonGroup), "RibbonGroup", 1, "Key");
			ribbonGroup.Text = BuildUniqueName(typeof(RibbonGroup), "Group ", 0, "Text");
			AddAndSelectItem(ribbonGroup);
		}

		private void AddNewContextualTab()
		{
			RibbonContextualTab ribbonContextualTab = new RibbonContextualTab();
			ribbonContextualTab.Key = BuildUniqueName(typeof(RibbonContextualTab), "ContextualTab", 1, "Key");
			ribbonContextualTab.Text = ribbonContextualTab.Key;
			AddAndSelectItem(ribbonContextualTab);
		}

		private void AddNewGalleryItem()
		{
			GalleryItem galleryItem = new GalleryItem();
			galleryItem.Key = BuildUniqueName(typeof(GalleryItem), "Item", 1, "Key");
			AddAndSelectItem(galleryItem);
		}

		private string BuildUniqueName(Type componentType, string prefix, int number, string name)
		{
			string text = "";
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(componentType).Find(name, ignoreCase: true);
			if (propertyDescriptor != null)
			{
				text = prefix + number;
				foreach (object item in _workingItems)
				{
					if (string.Equals(propertyDescriptor.GetValue(item) as string, text, StringComparison.OrdinalIgnoreCase))
					{
						return BuildUniqueName(componentType, prefix, number + 1, name);
					}
				}
			}
			return text;
		}

		private void InitializeControls()
		{
			_components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(RibbonItemListEditorControl));
			_tbOperations = new ToolStrip();
			_commandTypeImageList = new ImageList(_components);
			_tbtnAdd = new ToolStripDropDownButton();
			_tbtnRemove = new ToolStripButton();
			_tbtnClear = new ToolStripButton();
			_toolbarSeparator = new ToolStripSeparator();
			_tbtnMoveUp = new ToolStripButton();
			_tbtnMoveDown = new ToolStripButton();
			_list = new ListBox();
			_listSplitter = new Splitter();
			_properties = new PropertyGrid();
			_tbOperations.SuspendLayout();
			SuspendLayout();
			_tbOperations.GripStyle = ToolStripGripStyle.Hidden;
			_tbOperations.ImageList = _commandTypeImageList;
			_tbOperations.Items.AddRange(new ToolStripItem[6] { _tbtnAdd, _tbtnRemove, _tbtnClear, _toolbarSeparator, _tbtnMoveUp, _tbtnMoveDown });
			_tbOperations.Location = new Point(0, 0);
			_tbOperations.Name = "tbOperations";
			_tbOperations.RenderMode = ToolStripRenderMode.System;
			_tbOperations.Size = new Size(496, 25);
			_tbOperations.TabIndex = 3;
			_tbOperations.ItemClicked += OnTbOperationsItemClicked;
			ImageStrip.Load(_commandTypeImageList, componentResourceManager, "icons.ImageStrip");
			_commandTypeImageList.TransparentColor = Color.Transparent;
			_commandTypeImageList.Images.SetKeyName(0, "");
			_commandTypeImageList.Images.SetKeyName(1, "");
			_commandTypeImageList.Images.SetKeyName(2, "");
			_commandTypeImageList.Images.SetKeyName(3, "");
			_commandTypeImageList.Images.SetKeyName(4, "");
			_commandTypeImageList.Images.SetKeyName(5, "");
			_commandTypeImageList.Images.SetKeyName(6, "");
			_commandTypeImageList.Images.SetKeyName(7, "");
			_commandTypeImageList.Images.SetKeyName(8, "");
			_commandTypeImageList.Images.SetKeyName(9, "");
			_commandTypeImageList.Images.SetKeyName(10, "ButtonGroup.ico");
			_commandTypeImageList.Images.SetKeyName(11, "CheckBox.ico");
			_commandTypeImageList.Images.SetKeyName(12, "ComboBox.ico");
			_commandTypeImageList.Images.SetKeyName(13, "Command.ico");
			_commandTypeImageList.Images.SetKeyName(14, "CommandBox.ico");
			_commandTypeImageList.Images.SetKeyName(15, "CommandDropDown.ico");
			_commandTypeImageList.Images.SetKeyName(16, "ContainerControl.ico");
			_commandTypeImageList.Images.SetKeyName(17, "EditBox.ico");
			_commandTypeImageList.Images.SetKeyName(18, "Gallery.ico");
			_commandTypeImageList.Images.SetKeyName(19, "Label.ico");
			_commandTypeImageList.Images.SetKeyName(20, "Separator.ico");
			_commandTypeImageList.Images.SetKeyName(21, "Tab.ico");
			_tbtnAdd.ImageIndex = 0;
			_tbtnAdd.Name = "tbtnAdd";
			_tbtnAdd.Size = new Size(58, 22);
			_tbtnAdd.Tag = "Add";
			_tbtnAdd.Text = "Add";
			_tbtnRemove.ImageIndex = 1;
			_tbtnRemove.Name = "tbtnRemove";
			_tbtnRemove.Size = new Size(70, 22);
			_tbtnRemove.Tag = "Remove";
			_tbtnRemove.Text = "Remove";
			_tbtnClear.ImageIndex = 2;
			_tbtnClear.Name = "tbtnClear";
			_tbtnClear.Size = new Size(54, 22);
			_tbtnClear.Tag = "Clear";
			_tbtnClear.Text = "Clear";
			_toolbarSeparator.Name = "toolSep1";
			_toolbarSeparator.Size = new Size(6, 25);
			_tbtnMoveUp.ImageIndex = 3;
			_tbtnMoveUp.Name = "tbtnMoveUp";
			_tbtnMoveUp.Size = new Size(23, 22);
			_tbtnMoveUp.Tag = "Up";
			_tbtnMoveDown.ImageIndex = 4;
			_tbtnMoveDown.Name = "tbtnMoveDown";
			_tbtnMoveDown.Size = new Size(23, 22);
			_tbtnMoveDown.Tag = "Down";
			_list.Dock = DockStyle.Left;
			_list.DrawMode = DrawMode.OwnerDrawFixed;
			_list.IntegralHeight = false;
			_list.Location = new Point(0, 25);
			_list.Name = "list";
			_list.Size = new Size(168, 383);
			_list.TabIndex = 4;
			_list.DrawItem += OnListDrawItem;
			_list.SelectedIndexChanged += OnListSelectedIndexChanged;
			_list.Resize += OnListResize;
			_listSplitter.Location = new Point(168, 25);
			_listSplitter.Name = "splitter1";
			_listSplitter.Size = new Size(8, 383);
			_listSplitter.TabIndex = 6;
			_listSplitter.TabStop = false;
			_properties.Dock = DockStyle.Fill;
			_properties.LineColor = SystemColors.Control;
			_properties.Location = new Point(176, 25);
			_properties.Name = "properties";
			_properties.PropertySort = PropertySort.Alphabetical;
			_properties.Size = new Size(320, 383);
			_properties.TabIndex = 7;
			_properties.ToolbarVisible = false;
			_properties.PropertyValueChanged += OnPropertiesPropertyValueChanged;
			base.Controls.Add(_properties);
			base.Controls.Add(_listSplitter);
			base.Controls.Add(_list);
			base.Controls.Add(_tbOperations);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "CollectionPage";
			base.Size = new Size(496, 408);
			_tbOperations.ResumeLayout(performLayout: false);
			_tbOperations.PerformLayout();
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		private void OnListSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_list.SelectedIndex == -1)
			{
				_properties.SelectedObject = null;
			}
			else
			{
				_properties.SelectedObject = ((DictionaryEntry)_list.SelectedItem).Value;
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
				int selectedIndex = _list.SelectedIndex;
				RefreshListItems();
				_list.SelectedIndex = selectedIndex;
			}
			OnDataChanged(EventArgs.Empty);
		}

		private void OnTbOperationsItemClicked(object sender, ToolStripItemClickedEventArgs eventArgs)
		{
			switch (eventArgs.ClickedItem.Tag as string)
			{
			case "Add":
				if (_tbtnAdd.DropDown == null || !_tbtnAdd.HasDropDownItems)
				{
					AddNewItem();
				}
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

		private void OnControlClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new SeparatorCommand(), "SeparatorCommand");
		}

		private void OnAddLabelClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new LabelCommand(), "LabelCommand");
		}

		private void OnAddGalleryClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new Gallery(), "Gallery");
		}

		private void OnAddDropDownClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new DropDownCommand(), "DropDownCommand");
		}

		private void OnAddCheckBoxClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new CheckBoxCommand(), "CheckBoxCommand");
		}

		private void OnAddComboBoxClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new ComboBoxCommand(), "ComboBoxCommand");
		}

		private void OnAddTextBoxClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new TextBoxCommand(), "TextBoxCommand");
		}

		private void OnAddContainerControlClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new ContainerControlCommand(), "ContainerControlCommand");
		}

		private void OnAddButtonGroupClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new ButtonGroup(), "ButtonGroup");
		}

		private void OnAddCommandBoxClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new CommandBox(), "CommandBox");
		}

		private void OnAddStatusBarPanelClick(object sender, EventArgs eventArgs)
		{
			if (_statusBar != null)
			{
				AddNamedCommand(new StatusBarPanel(), "StatusBarPanel");
			}
		}

		private void OnAddBackstageTabClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new BackstageTab(), "BackstageTab");
		}

		private void OnAddButtonCommandClick(object sender, EventArgs eventArgs)
		{
			AddNamedCommand(new ButtonCommand(), "ButtonCommand");
		}

		private int GetCommandImageIndex(CommandBase commandBase)
		{
			if (commandBase == null)
			{
				return -1;
			}
			switch (commandBase.CommandType)
			{
			case CommandType.ButtonGroup:
				return ImageButtonGroup;
			case CommandType.ContainerControl:
				if (commandBase is ComboBoxCommand)
				{
					return ImageComboBox;
				}
				if (commandBase is TextBoxCommand)
				{
					return ImageTextBox;
				}
				if (commandBase is CheckBoxCommand)
				{
					return ImageCheckBox;
				}
				return ImageContainerControl;
			case CommandType.Box:
				return ImageCommandBox;
			case CommandType.DropDownButton:
				return ImageDropDown;
			case CommandType.Gallery:
				return ImageGallery;
			case CommandType.Label:
				return ImageLabel;
			case CommandType.Separator:
				return ImageSeparator;
			default:
				return ImageCommand;
			}
		}

		private void OnListDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			eventArgs.DrawBackground();
			if (eventArgs.Index > -1)
			{
				int textX = eventArgs.Bounds.X + ListItemImagePadding;
				DictionaryEntry entry = (DictionaryEntry)_list.Items[eventArgs.Index];
				if (_listItemImage != null)
				{
					int imageIndex = GetCommandImageIndex(entry.Value as CommandBase);
					int imageWidth;
					if (imageIndex >= 0)
					{
						Size imageSize = _commandTypeImageList.ImageSize;
						_commandTypeImageList.Draw(eventArgs.Graphics, textX, eventArgs.Bounds.Top + (eventArgs.Bounds.Height - imageSize.Height) / 2, imageIndex);
						imageWidth = imageSize.Width;
					}
					else
					{
						eventArgs.Graphics.DrawImageUnscaled(_listItemImage, textX, eventArgs.Bounds.Top + (eventArgs.Bounds.Height - _listItemImage.Height) / 2);
						imageWidth = _listItemImage.Width;
					}
					textX += imageWidth + ListItemImagePadding;
				}
				using (StringFormat stringFormat = new StringFormat())
				using (Brush brush = new SolidBrush(eventArgs.ForeColor))
				{
					stringFormat.FormatFlags = StringFormatFlags.NoWrap;
					stringFormat.LineAlignment = StringAlignment.Center;
					Rectangle rectangle = new Rectangle(textX, eventArgs.Bounds.Y, eventArgs.Bounds.Right - textX, eventArgs.Bounds.Height);
					eventArgs.Graphics.DrawString((string)entry.Key, _list.Font, brush, rectangle, stringFormat);
				}
			}
			if ((eventArgs.State & DrawItemState.Focus) != DrawItemState.None)
			{
				eventArgs.DrawFocusRectangle();
			}
		}

		private void OnListResize(object sender, EventArgs eventArgs)
		{
			_list.Invalidate();
			_list.Refresh();
		}
	}
}
