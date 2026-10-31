using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.Design
{
	[ToolboxItem(false)]
	internal class SchedulerEditorControl : UserControl, ISchedulerElement
	{
		private enum CollectionType
		{
			None,
			Fields,
			FormatConditions,
			AppointmentOwners,
			StoredFilters,
			HourRangeCollection,
			WorkingDaysException
		}

		private IContainer _iContainer;

		private ICollection _selectedObject;

		private CollectionType _collectionType;

		private MxToolBar _tbOperations;

		private MxToolBarButton _addToolBarButton;

		private MxToolBarButton _removeToolBarButton;

		private MxToolBarButton _clearToolBarButton;

		private MxToolBarButton _separatorToolBarButton;

		private MxToolBarButton _moveUpToolBarButton;

		private MxToolBarButton _moveDownToolBarButton;

		private ImageList _imageList;

		private ListBox _itemListBox;

		private Splitter _splitter;

		private PropertyGrid _properties;

		private ArrayList _editedItems;

		private Image _itemImage;

		private ToolbarOptions _toolbarOptions = ToolbarOptions.All;

		private static object _selectedObjectChangedEventKey = new object();

		private static object _refreshTreeEventKey = new object();

		object ISchedulerElement.SelectedObject
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
					UpdateSchedulerEditorControl();
				}
			}
		}

		Image ISchedulerElement.ItemImage
		{
			get
			{
				return _itemImage;
			}
			set
			{
				_itemImage = value;
				int itemHeight = (int)_itemListBox.Font.GetHeight() + 2;
				if (_itemImage != null && _itemImage.Height + 2 > itemHeight)
				{
					itemHeight = _itemImage.Height + 2;
				}
				_itemListBox.ItemHeight = itemHeight;
				_itemListBox.Refresh();
			}
		}

		event EventHandler ISchedulerElement.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(_selectedObjectChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedObjectChangedEventKey, value);
			}
		}

		event EventHandler ISchedulerElement.RefreshTree
		{
			add
			{
				base.Events.AddHandler(_refreshTreeEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_refreshTreeEventKey, value);
			}
		}

		internal SchedulerEditorControl()
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void UpdateSchedulerEditorControl()
		{
			if (_selectedObject == null)
			{
				_collectionType = CollectionType.None;
			}
			else if (_selectedObject is SchedulerFieldCollection)
			{
				_collectionType = CollectionType.Fields;
			}
			else if (_selectedObject is SchedulerAppointmentOwnerCollection)
			{
				_collectionType = CollectionType.AppointmentOwners;
			}
			else if (_selectedObject is SchedulerFormatConditionCollection)
			{
				_collectionType = CollectionType.FormatConditions;
			}
			else if (_selectedObject is SchedulerStoredFilterCollection)
			{
				_collectionType = CollectionType.StoredFilters;
			}
			else if (_selectedObject is SchedulerHourRangeCollection)
			{
				_collectionType = CollectionType.HourRangeCollection;
			}
			else if (_selectedObject is WorkingHourExceptionCollection)
			{
				_collectionType = CollectionType.WorkingDaysException;
			}
			_editedItems = new ArrayList(_selectedObject.Count);
			foreach (object item in _selectedObject)
			{
				_editedItems.Add(item);
			}
			RefreshList();
			if (_itemListBox.Items.Count > 0)
			{
				_itemListBox.SelectedIndex = 0;
			}
			UpdateToolbarState();
		}

		private void RefreshList()
		{
			_itemListBox.Items.Clear();
			for (int i = 0; i < _editedItems.Count; i++)
			{
				_itemListBox.Items.Add(new ON(_editedItems[i], GetItemText(i, _editedItems[i])));
			}
		}

		private string GetItemText(int index, object component)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component).Find("Key", ignoreCase: true);
			string text = "";
			if (propertyDescriptor != null)
			{
				text = (string)propertyDescriptor.GetValue(component);
			}
			if (text.Length > 0)
			{
				return "[" + index + "] - '" + text + "'";
			}
			return "[" + index + "] - {" + component.GetType().Name + "}";
		}

		bool ISchedulerElement.CommitChanges()
		{
			switch (_collectionType)
			{
			case CollectionType.Fields:
				return CommitFields();
			case CollectionType.FormatConditions:
				return CommitFormatConditions();
			case CollectionType.AppointmentOwners:
				return CommitAppointmentOwners();
			case CollectionType.StoredFilters:
				return CommitStoredFilters();
			case CollectionType.HourRangeCollection:
				return CommitHourRanges();
			case CollectionType.WorkingDaysException:
				return CommitWorkingHourExceptions();
			default:
				return true;
			}
		}

		void ISchedulerElement.SetSite(ISite site)
		{
		}

		void ISchedulerElement.Unload()
		{
		}

		private bool AreFormatConditionsValid()
		{
			for (int i = 0; i < _editedItems.Count; i++)
			{
				SchedulerFormatCondition schedulerFormatCondition = (SchedulerFormatCondition)_editedItems[i];
				if (!schedulerFormatCondition.IsValid(((SchedulerFormatConditionCollection)_selectedObject)._scheduler))
				{
					_itemListBox.SelectedIndex = i;
					MessageBox.Show("One more conditions are not valid.\nField property must be set in the condition to be valid.", "Mordex Schedule Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitFormatConditions()
		{
			if (!AreFormatConditionsValid())
			{
				return false;
			}
			SchedulerFormatConditionCollection schedulerFormatConditionCollection = (SchedulerFormatConditionCollection)_selectedObject;
			((ISupportInitialize)schedulerFormatConditionCollection._scheduler).BeginInit();
			schedulerFormatConditionCollection.Clear();
			SchedulerFormatCondition[] newConditions = new SchedulerFormatCondition[_editedItems.Count];
			_editedItems.CopyTo(newConditions);
			SchedulerFormatCondition[] conditionsToAssign = newConditions;
			foreach (SchedulerFormatCondition schedulerFormatCondition in conditionsToAssign)
			{
				schedulerFormatCondition._owner = null;
			}
			schedulerFormatConditionCollection.AddRange(newConditions);
			((ISupportInitialize)schedulerFormatConditionCollection._scheduler).EndInit();
			return true;
		}

		private bool AreStoredFiltersValid()
		{
			MxScheduler scheduler = ((SchedulerStoredFilterCollection)_selectedObject).Scheduler;
			for (int i = 0; i < _editedItems.Count; i++)
			{
				SchedulerStoredFilter schedulerStoredFilter = (SchedulerStoredFilter)_editedItems[i];
				if (schedulerStoredFilter.FilterCondition != null && !schedulerStoredFilter.FilterCondition.IsValidForScheduler(scheduler))
				{
					_itemListBox.SelectedIndex = i;
					MessageBox.Show("One more FilterCondition assigned to the stored filters are not valid.", "Mordex Schedule Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitStoredFilters()
		{
			if (!AreStoredFiltersValid())
			{
				return false;
			}
			SchedulerStoredFilterCollection schedulerStoredFilterCollection = (SchedulerStoredFilterCollection)_selectedObject;
			SchedulerStoredFilter schedulerStoredFilter = null;
			if (schedulerStoredFilterCollection.Scheduler.FilterCondition is SchedulerStoredFilter)
			{
				schedulerStoredFilter = (SchedulerStoredFilter)schedulerStoredFilterCollection.Scheduler.FilterCondition;
			}
			((ISupportInitialize)schedulerStoredFilterCollection.Scheduler).BeginInit();
			schedulerStoredFilterCollection.Clear();
			SchedulerStoredFilter[] newFilters = new SchedulerStoredFilter[_editedItems.Count];
			_editedItems.CopyTo(newFilters);
			SchedulerStoredFilter[] filtersToAssign = newFilters;
			foreach (SchedulerStoredFilter newFilter in filtersToAssign)
			{
				newFilter.SetSchedulerStoredFilterCollection(null);
			}
			schedulerStoredFilterCollection.AddRange(newFilters);
			if (schedulerStoredFilter != null && schedulerStoredFilterCollection.Scheduler.FilterCondition == null && schedulerStoredFilter.GetSchedulerStoredFilterCollection() == schedulerStoredFilterCollection)
			{
				schedulerStoredFilterCollection.Scheduler.FilterCondition = schedulerStoredFilter;
			}
			((ISupportInitialize)schedulerStoredFilterCollection.Scheduler).EndInit();
			return true;
		}

		private bool CommitFields()
		{
			SchedulerFieldCollection schedulerFieldCollection = (SchedulerFieldCollection)_selectedObject;
			((ISupportInitialize)schedulerFieldCollection.Scheduler).BeginInit();
			schedulerFieldCollection.Clear();
			SchedulerField[] newFields = new SchedulerField[_editedItems.Count];
			_editedItems.CopyTo(newFields);
			SchedulerField[] fieldsToAssign = newFields;
			foreach (SchedulerField schedulerField in fieldsToAssign)
			{
				schedulerField._owner = null;
			}
			schedulerFieldCollection.AddRange(newFields);
			((ISupportInitialize)schedulerFieldCollection.Scheduler).EndInit();
			return true;
		}

		private bool CommitWorkingHourExceptions()
		{
			WorkingHourExceptionCollection workingHourExceptionCollection = (WorkingHourExceptionCollection)_selectedObject;
			workingHourExceptionCollection.Clear();
			WorkingHourException[] newExceptions = new WorkingHourException[_editedItems.Count];
			_editedItems.CopyTo(newExceptions);
			WorkingHourException[] exceptionsToAssign = newExceptions;
			foreach (WorkingHourException ex in exceptionsToAssign)
			{
				ex._owner = null;
			}
			workingHourExceptionCollection.AddRange(newExceptions);
			return true;
		}

		private bool CommitHourRanges()
		{
			SchedulerHourRangeCollection schedulerHourRangeCollection = (SchedulerHourRangeCollection)_selectedObject;
			schedulerHourRangeCollection.Clear();
			SchedulerHourRange[] newHourRanges = new SchedulerHourRange[_editedItems.Count];
			_editedItems.CopyTo(newHourRanges);
			SchedulerHourRange[] hourRangesToAssign = newHourRanges;
			foreach (SchedulerHourRange schedulerHourRange in hourRangesToAssign)
			{
				schedulerHourRange.SetOwner(null);
			}
			schedulerHourRangeCollection.AddRange(newHourRanges);
			return true;
		}

		private bool CommitAppointmentOwners()
		{
			SchedulerAppointmentOwnerCollection schedulerAppointmentOwnerCollection = (SchedulerAppointmentOwnerCollection)_selectedObject;
			schedulerAppointmentOwnerCollection.Clear();
			SchedulerAppointmentOwner[] newOwners = new SchedulerAppointmentOwner[_editedItems.Count];
			_editedItems.CopyTo(newOwners);
			SchedulerAppointmentOwner[] ownersToAssign = newOwners;
			foreach (SchedulerAppointmentOwner schedulerAppointmentOwner in ownersToAssign)
			{
				schedulerAppointmentOwner._owner = null;
			}
			schedulerAppointmentOwnerCollection.AddRange(newOwners);
			return true;
		}

		bool ISchedulerElement.Support(ToolbarOptions option)
		{
			bool isSupported = (option & _toolbarOptions) != 0;
			if (isSupported)
			{
				switch (option)
				{
				case ToolbarOptions.Clear:
					if (_editedItems.Count == 0)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.MoveDown:
					if (_itemListBox.SelectedIndex == _itemListBox.Items.Count - 1)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.MoveUp:
					if (_itemListBox.SelectedIndex < 1)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.Remove:
					if (_itemListBox.SelectedIndex == -1)
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
				_clearToolBarButton.Enabled = false;
				_moveDownToolBarButton.Enabled = false;
				_moveUpToolBarButton.Enabled = false;
				_removeToolBarButton.Enabled = false;
			}
			else
			{
				_clearToolBarButton.Enabled = _editedItems.Count > 0;
				_moveDownToolBarButton.Enabled = _itemListBox.SelectedIndex < _itemListBox.Items.Count - 1;
				_moveUpToolBarButton.Enabled = _itemListBox.SelectedIndex > 0;
				_removeToolBarButton.Enabled = _itemListBox.SelectedIndex != -1;
			}
		}

		private void AddItem()
		{
			switch (_collectionType)
			{
			case CollectionType.AppointmentOwners:
				AddAppointmentOwner();
				break;
			case CollectionType.Fields:
				AddField();
				break;
			case CollectionType.FormatConditions:
				AddFormatCondition();
				break;
			case CollectionType.StoredFilters:
				AddStoredFilter();
				break;
			case CollectionType.HourRangeCollection:
				AddHourRange();
				break;
			case CollectionType.WorkingDaysException:
				AddWorkingHourException();
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
			_editedItems.Remove(((ON)_itemListBox.SelectedItem).Object);
			RefreshList();
			if (selectedIndex < _itemListBox.Items.Count)
			{
				_itemListBox.SelectedIndex = selectedIndex;
				return;
			}
			_itemListBox.SelectedIndex = _itemListBox.Items.Count - 1;
			if (_itemListBox.SelectedIndex == -1)
			{
				_properties.SelectedObject = null;
				NotifySelectedObjectChanged(EventArgs.Empty);
			}
		}

		private void ClearItems()
		{
			_editedItems.Clear();
			RefreshList();
			_properties.SelectedObject = null;
			NotifySelectedObjectChanged(EventArgs.Empty);
		}

		private void MoveSelectedItemUp()
		{
			MoveItemUp(_itemListBox.SelectedIndex);
		}

		private void MoveSelectedItemDown()
		{
			MoveItemDown(_itemListBox.SelectedIndex);
		}

		private void NotifySelectedObjectChanged(EventArgs eventArgs)
		{
			UpdateToolbarState();
			if (base.Events[_selectedObjectChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void MoveItemUp(int index)
		{
			if (index > 0)
			{
				object value = _editedItems[index];
				int previousIndex = index - 1;
				_editedItems[index] = _editedItems[previousIndex];
				_editedItems[previousIndex] = value;
				RefreshList();
				_itemListBox.SelectedIndex = previousIndex;
			}
		}

		private void MoveItemDown(int index)
		{
			if (index < _editedItems.Count - 1)
			{
				object value = _editedItems[index];
				int nextIndex = index + 1;
				_editedItems[index] = _editedItems[nextIndex];
				_editedItems[nextIndex] = value;
				RefreshList();
				_itemListBox.SelectedIndex = nextIndex;
			}
		}

		private void SelectItem(object value)
		{
			for (int i = 0; i < _editedItems.Count; i++)
			{
				if (_editedItems[i] == value)
				{
					_itemListBox.SelectedIndex = i;
					break;
				}
			}
		}

		private void AddAndSelectItem(object value)
		{
			_editedItems.Add(value);
			RefreshList();
			SelectItem(value);
		}

		private void AddField()
		{
			SchedulerField schedulerField = new SchedulerField();
			schedulerField._owner = (SchedulerFieldCollection)_selectedObject;
			schedulerField.Key = CreateUniqueKey(typeof(SchedulerField), "Field", 1);
			AddAndSelectItem(schedulerField);
		}

		private void AddAppointmentOwner()
		{
			SchedulerAppointmentOwner schedulerAppointmentOwner = new SchedulerAppointmentOwner();
			schedulerAppointmentOwner._owner = (SchedulerAppointmentOwnerCollection)_selectedObject;
			schedulerAppointmentOwner.Key = CreateUniqueKey(typeof(SchedulerAppointmentOwner), "Owner", 1);
			AddAndSelectItem(schedulerAppointmentOwner);
		}

		private void AddWorkingHourException()
		{
			WorkingHourException ex = new WorkingHourException();
			ex._owner = (WorkingHourExceptionCollection)_selectedObject;
			ex.Key = CreateUniqueKey(typeof(WorkingHourException), "WorkingException", 1);
			AddAndSelectItem(ex);
		}

		private void AddHourRange()
		{
			SchedulerHourRange schedulerHourRange = new SchedulerHourRange();
			schedulerHourRange.Key = CreateUniqueKey(typeof(SchedulerHourRange), "HourRange", 1);
			AddAndSelectItem(schedulerHourRange);
		}

		private void AddStoredFilter()
		{
			SchedulerStoredFilter schedulerStoredFilter = new SchedulerStoredFilter();
			schedulerStoredFilter.Key = CreateUniqueKey(typeof(SchedulerStoredFilter), "StoredFilter", 1);
			schedulerStoredFilter.SetSchedulerStoredFilterCollection((SchedulerStoredFilterCollection)_selectedObject);
			AddAndSelectItem(schedulerStoredFilter);
		}

		private string CreateUniqueKey(Type componentType, string textArgument, int number)
		{
			string text = "";
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(componentType).Find("Key", ignoreCase: true);
			if (propertyDescriptor != null)
			{
				text = textArgument + number;
				foreach (object item in _editedItems)
				{
					if (string.Compare((string)propertyDescriptor.GetValue(item), text, ignoreCase: true) == 0)
					{
						return CreateUniqueKey(componentType, textArgument, number + 1);
					}
				}
			}
			return text;
		}

		private void AddFormatCondition()
		{
			SchedulerFormatCondition schedulerFormatCondition = new SchedulerFormatCondition();
			schedulerFormatCondition._owner = (SchedulerFormatConditionCollection)_selectedObject;
			schedulerFormatCondition.Key = CreateUniqueKey(typeof(SchedulerFormatCondition), "FormatCondition", 1);
			AddAndSelectItem(schedulerFormatCondition);
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			ResourceManager resourceManager = new ResourceManager(typeof(SchedulerEditorControl));
			_tbOperations = new MxToolBar();
			_addToolBarButton = new MxToolBarButton();
			_removeToolBarButton = new MxToolBarButton();
			_clearToolBarButton = new MxToolBarButton();
			_separatorToolBarButton = new MxToolBarButton();
			_moveUpToolBarButton = new MxToolBarButton();
			_moveDownToolBarButton = new MxToolBarButton();
			_imageList = new ImageList(_iContainer);
			_itemListBox = new ListBox();
			_splitter = new Splitter();
			_properties = new PropertyGrid();
			SuspendLayout();
			_tbOperations.Appearance = MxToolBarAppearance.Flat;
			_tbOperations.Buttons.AddRange(new MxToolBarButton[6] { _addToolBarButton, _removeToolBarButton, _clearToolBarButton, _separatorToolBarButton, _moveUpToolBarButton, _moveDownToolBarButton });
			_tbOperations.Divider = false;
			_tbOperations.DropDownArrows = true;
			_tbOperations.ImageList = _imageList;
			_tbOperations.Name = "tbOperations";
			_tbOperations.ShowToolTips = true;
			_tbOperations.Size = new Size(496, 23);
			_tbOperations.TabIndex = 3;
			_tbOperations.TextAlign = MxToolBarTextAlign.Right;
			_tbOperations.ButtonClick += OnTbOperationsButtonClick;
			_addToolBarButton.ImageIndex = 0;
			_addToolBarButton.Tag = "Add";
			_addToolBarButton.Text = "Add";
			_removeToolBarButton.ImageIndex = 1;
			_removeToolBarButton.Tag = "Remove";
			_removeToolBarButton.Text = "Remove";
			_clearToolBarButton.ImageIndex = 2;
			_clearToolBarButton.Tag = "Clear";
			_clearToolBarButton.Text = "Clear";
			_separatorToolBarButton.Style = MxToolBarButtonStyle.Separator;
			_moveUpToolBarButton.ImageIndex = 3;
			_moveUpToolBarButton.Tag = "Up";
			_moveDownToolBarButton.ImageIndex = 4;
			_moveDownToolBarButton.Tag = "Down";
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
			}
			NotifySelectedObjectChanged(EventArgs.Empty);
		}

		private void OnPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			if (eventArgs.ChangedItem.Label == "Key")
			{
				int selectedIndex = _itemListBox.SelectedIndex;
				RefreshList();
				_itemListBox.SelectedIndex = selectedIndex;
			}
			else if (eventArgs.ChangedItem.Label == "HasValueList")
			{
				NotifySelectedObjectChanged(EventArgs.Empty);
			}
		}

		private void OnTbOperationsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			string text;
			if (eventArgs.Button.Tag == null || (text = (string)eventArgs.Button.Tag) == null)
			{
				return;
			}
			if (!(text == "Add"))
			{
				if (!(text == "Remove"))
				{
					if (!(text == "Clear"))
					{
						if (!(text == "Up"))
						{
							if (text == "Down")
							{
								MoveSelectedItemDown();
							}
						}
						else
						{
							MoveSelectedItemUp();
						}
					}
					else
					{
						ClearItems();
					}
				}
				else
				{
					RemoveSelectedItem();
				}
			}
			else
			{
				AddItem();
			}
		}

		private void OnListDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			eventArgs.DrawBackground();
			if (eventArgs.Index > -1)
			{
				int textLeft = eventArgs.Bounds.X + 2;
				if (_itemImage != null)
				{
					eventArgs.Graphics.DrawImageUnscaled(_itemImage, textLeft, eventArgs.Bounds.Top + (eventArgs.Bounds.Height - _itemImage.Height) / 2);
					textLeft += _itemImage.Width + 2;
				}
				StringFormat stringFormat = new StringFormat();
				stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				stringFormat.LineAlignment = StringAlignment.Center;
				Rectangle rectangle = new Rectangle(textLeft, eventArgs.Bounds.Y, eventArgs.Bounds.Right - textLeft, eventArgs.Bounds.Height);
				Brush brush = new SolidBrush(eventArgs.ForeColor);
				eventArgs.Graphics.DrawString(((ON)_itemListBox.Items[eventArgs.Index]).Name, _itemListBox.Font, brush, rectangle, stringFormat);
				brush.Dispose();
				stringFormat.Dispose();
			}
			if ((eventArgs.State & DrawItemState.Focus) != DrawItemState.None)
			{
				eventArgs.DrawFocusRectangle();
			}
		}

		private void OnListResize(object sender, EventArgs eventArgs)
		{
			_itemListBox.Refresh();
		}
	}
}
