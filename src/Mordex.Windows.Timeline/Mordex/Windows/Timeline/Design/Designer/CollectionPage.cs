using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design.Designer
{
	[ToolboxItem(false)]
	public class CollectionPage : UserControl, ITimelineElement
	{
		private enum CollectionType
		{
			None,
			Fields,
			FormatConditions,
			Groups,
			GroupHeaderTotals,
			StoredFilters,
			HourRangeCollection,
			WorkingDaysException
		}

		private IContainer _iContainer;

		private ICollection _selectedObject;

		private CollectionType _collectionType;

		private MxToolBar _tbOperations;

		private MxToolBarButton _toolBarButton;

		private MxToolBarButton _removeButton;

		private MxToolBarButton _clearButton;

		private MxToolBarButton _firstSeparator;

		private MxToolBarButton _moveUpButton;

		private MxToolBarButton _moveDownButton;

		private MxToolBarButton _secondSeparator;

		private ImageList _imageList;

		private ListBox _itemList;

		private Splitter _listSplitter;

		private PropertyGrid _properties;

		private ArrayList _items;

		private Image _itemImage;

		private ToolbarOptions _toolbarOptions = ToolbarOptions.All;

		private static object _selectedObjectChangedEventKey = new object();

		private static object _refreshTreeEventKey = new object();

		object ITimelineElement.SelectedObject
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
					UpdateCollectionPage();
				}
			}
		}

		Image ITimelineElement.ItemImage
		{
			get
			{
				return _itemImage;
			}
			set
			{
				_itemImage = value;
				int itemHeight = (int)_itemList.Font.GetHeight() + 2;
				if (_itemImage != null && _itemImage.Height + 2 > itemHeight)
				{
					itemHeight = _itemImage.Height + 2;
				}
				_itemList.ItemHeight = itemHeight;
				_itemList.Refresh();
			}
		}

		event EventHandler ITimelineElement.SelectedObjectChanged
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

		event EventHandler ITimelineElement.RefreshTree
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

		internal CollectionPage()
		{
			InitializeComponent();
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

		private void UpdateCollectionPage()
		{
			if (_selectedObject == null)
			{
				_collectionType = CollectionType.None;
			}
			else if (_selectedObject is TimelineFieldCollection)
			{
				_collectionType = CollectionType.Fields;
			}
			else if (_selectedObject is TimelineFormatConditionCollection)
			{
				_collectionType = CollectionType.FormatConditions;
			}
			else if (_selectedObject is TimelineGroupCollection)
			{
				_collectionType = CollectionType.Groups;
			}
			else if (_selectedObject is TimelineGroupHeaderTotalCollection)
			{
				_collectionType = CollectionType.GroupHeaderTotals;
			}
			else if (_selectedObject is TimelineStoredFilterCollection)
			{
				_collectionType = CollectionType.StoredFilters;
			}
			else if (_selectedObject is TimelineHourRangeCollection)
			{
				_collectionType = CollectionType.HourRangeCollection;
			}
			else if (_selectedObject is WorkingHourExceptionCollection)
			{
				_collectionType = CollectionType.WorkingDaysException;
			}
			_items = new ArrayList(_selectedObject.Count);
			foreach (object item in _selectedObject)
			{
				_items.Add(item);
			}
			RefreshList();
			if (_itemList.Items.Count > 0)
			{
				_itemList.SelectedIndex = 0;
			}
			UpdateToolbarState();
		}

		private void RefreshList()
		{
			_itemList.Items.Clear();
			for (int i = 0; i < _items.Count; i++)
			{
				_itemList.Items.Add(new ObjectName(_items[i], GetItemDisplayText(i, _items[i])));
			}
		}

		private string GetItemDisplayText(int index, object component)
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

		bool ITimelineElement.CommitChanges()
		{
			switch (_collectionType)
			{
			case CollectionType.Fields:
				return CommitFields();
			case CollectionType.FormatConditions:
				return CommitFormatConditions();
			case CollectionType.Groups:
				return CommitGroups();
			case CollectionType.GroupHeaderTotals:
				return CommitGroupHeaderTotals();
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

		private bool ValidateFormatConditions()
		{
			for (int i = 0; i < _items.Count; i++)
			{
				TimelineFormatCondition timelineFormatCondition = (TimelineFormatCondition)_items[i];
				if (!timelineFormatCondition.IsValid(((TimelineFormatConditionCollection)_selectedObject)._timeline))
				{
					_itemList.SelectedIndex = i;
					MessageBox.Show("One more conditions are not valid.\n\rField property must be set in the condition to be valid.", "Mordex Timeline Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitFormatConditions()
		{
			if (!ValidateFormatConditions())
			{
				return false;
			}
			TimelineFormatConditionCollection timelineFormatConditionCollection = (TimelineFormatConditionCollection)_selectedObject;
			((ISupportInitialize)timelineFormatConditionCollection._timeline).BeginInit();
			timelineFormatConditionCollection.Clear();
			TimelineFormatCondition[] formatConditions = new TimelineFormatCondition[_items.Count];
			_items.CopyTo(formatConditions);
			TimelineFormatCondition[] formatConditionsToDetach = formatConditions;
			foreach (TimelineFormatCondition timelineFormatCondition in formatConditionsToDetach)
			{
				timelineFormatCondition.SetFormatConditionCollection(null);
			}
			timelineFormatConditionCollection.AddRange(formatConditions);
			((ISupportInitialize)timelineFormatConditionCollection._timeline).EndInit();
			return true;
		}

		private bool ValidateGroups()
		{
			MxTimeline timeline = ((TimelineGroupCollection)_selectedObject).GetTimeline();
			for (int i = 0; i < _items.Count; i++)
			{
				TimelineGroup timelineGroup = (TimelineGroup)_items[i];
				if (!timelineGroup.BelongsToTimeline(timeline))
				{
					_itemList.SelectedIndex = i;
					MessageBox.Show("One more Groups are not valid.\n\rField property must be set in the object to be valid.", "Mordex Timeline Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool ValidateGroupHeaderTotals()
		{
			MxTimeline timeline = ((TimelineGroupHeaderTotalCollection)_selectedObject).Timeline;
			for (int i = 0; i < _items.Count; i++)
			{
				TimelineGroupHeaderTotal timelineGroupHeaderTotal = (TimelineGroupHeaderTotal)_items[i];
				if (!timelineGroupHeaderTotal.IsValid(timeline))
				{
					_itemList.SelectedIndex = i;
					MessageBox.Show("One more group totals are not valid.", "Mordex Timeline Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
				if (timelineGroupHeaderTotal.Field != null && timeline != timelineGroupHeaderTotal.Field.Timeline)
				{
					_itemList.SelectedIndex = i;
					MessageBox.Show("One more group totals are not valid./n/rThe field asigned to a group header total must belong to the same Timeline.", "Mordex Timeline Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitGroups()
		{
			if (!ValidateGroups())
			{
				return false;
			}
			TimelineGroupCollection timelineGroupCollection = (TimelineGroupCollection)_selectedObject;
			((ISupportInitialize)timelineGroupCollection.GetTimeline()).BeginInit();
			timelineGroupCollection.Clear();
			TimelineGroup[] groups = new TimelineGroup[_items.Count];
			_items.CopyTo(groups);
			TimelineGroup[] groupsToDetach = groups;
			foreach (TimelineGroup timelineGroup in groupsToDetach)
			{
				timelineGroup.SetParentCollection(null);
			}
			timelineGroupCollection.AddRange(groups);
			((ISupportInitialize)timelineGroupCollection.GetTimeline()).EndInit();
			return true;
		}

		private bool CommitGroupHeaderTotals()
		{
			if (!ValidateGroupHeaderTotals())
			{
				return false;
			}
			TimelineGroupHeaderTotalCollection timelineGroupHeaderTotalCollection = (TimelineGroupHeaderTotalCollection)_selectedObject;
			((ISupportInitialize)timelineGroupHeaderTotalCollection.Timeline).BeginInit();
			timelineGroupHeaderTotalCollection.Clear();
			TimelineGroupHeaderTotal[] totals = new TimelineGroupHeaderTotal[_items.Count];
			_items.CopyTo(totals);
			TimelineGroupHeaderTotal[] totalsToDetach = totals;
			foreach (TimelineGroupHeaderTotal timelineGroupHeaderTotal in totalsToDetach)
			{
				timelineGroupHeaderTotal.SetParentCollection(null);
			}
			timelineGroupHeaderTotalCollection.AddRange(totals);
			((ISupportInitialize)timelineGroupHeaderTotalCollection.Timeline).EndInit();
			return true;
		}

		private bool ValidateStoredFilters()
		{
			MxTimeline timeline = ((TimelineStoredFilterCollection)_selectedObject).GetTimeline();
			for (int i = 0; i < _items.Count; i++)
			{
				TimelineStoredFilter timelineStoredFilter = (TimelineStoredFilter)_items[i];
				if (timelineStoredFilter.FilterCondition != null && !timelineStoredFilter.FilterCondition.IsValid(timeline))
				{
					_itemList.SelectedIndex = i;
					MessageBox.Show("One more FilterCondition assigned to the stored filters are not valid.", "Mordex Timeline Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitStoredFilters()
		{
			if (!ValidateStoredFilters())
			{
				return false;
			}
			TimelineStoredFilterCollection timelineStoredFilterCollection = (TimelineStoredFilterCollection)_selectedObject;
			TimelineStoredFilter timelineStoredFilter = null;
			if (timelineStoredFilterCollection.GetTimeline().FilterCondition is TimelineStoredFilter)
			{
				timelineStoredFilter = (TimelineStoredFilter)timelineStoredFilterCollection.GetTimeline().FilterCondition;
			}
			((ISupportInitialize)timelineStoredFilterCollection.GetTimeline()).BeginInit();
			timelineStoredFilterCollection.Clear();
			TimelineStoredFilter[] storedFilters = new TimelineStoredFilter[_items.Count];
			_items.CopyTo(storedFilters);
			TimelineStoredFilter[] storedFiltersToDetach = storedFilters;
			foreach (TimelineStoredFilter storedFilterToDetach in storedFiltersToDetach)
			{
				storedFilterToDetach.SetParentCollection(null);
			}
			timelineStoredFilterCollection.AddRange(storedFilters);
			if (timelineStoredFilter != null && timelineStoredFilterCollection.GetTimeline().FilterCondition == null && timelineStoredFilter.GetParentCollection() == timelineStoredFilterCollection)
			{
				timelineStoredFilterCollection.GetTimeline().FilterCondition = timelineStoredFilter;
			}
			((ISupportInitialize)timelineStoredFilterCollection.GetTimeline()).EndInit();
			return true;
		}

		private bool CommitFields()
		{
			TimelineFieldCollection timelineFieldCollection = (TimelineFieldCollection)_selectedObject;
			TimelineField[] fields = new TimelineField[_items.Count];
			_items.CopyTo(fields);
			TimelineField[] fieldsToCheck = fields;
			foreach (TimelineField timelineField in fieldsToCheck)
			{
				if (!timelineFieldCollection.Contains(timelineField))
				{
					timelineField.AssignParentCollection(null);
				}
			}
			timelineFieldCollection.ReplaceFields(fields);
			return true;
		}

		private bool CommitWorkingHourExceptions()
		{
			WorkingHourExceptionCollection workingHourExceptionCollection = (WorkingHourExceptionCollection)_selectedObject;
			workingHourExceptionCollection.Clear();
			WorkingHourException[] exceptions = new WorkingHourException[_items.Count];
			_items.CopyTo(exceptions);
			WorkingHourException[] exceptionsToDetach = exceptions;
			foreach (WorkingHourException ex in exceptionsToDetach)
			{
				ex._owner = null;
			}
			workingHourExceptionCollection.AddRange(exceptions);
			return true;
		}

		private bool CommitHourRanges()
		{
			TimelineHourRangeCollection timelineHourRangeCollection = (TimelineHourRangeCollection)_selectedObject;
			timelineHourRangeCollection.Clear();
			TimelineHourRange[] hourRanges = new TimelineHourRange[_items.Count];
			_items.CopyTo(hourRanges);
			TimelineHourRange[] hourRangesToDetach = hourRanges;
			foreach (TimelineHourRange timelineHourRange in hourRangesToDetach)
			{
				timelineHourRange._owner = null;
			}
			timelineHourRangeCollection.AddRange(hourRanges);
			return true;
		}

		void ITimelineElement.SetSite(ISite site)
		{
		}

		void ITimelineElement.Unload()
		{
		}

		bool ITimelineElement.Support(ToolbarOptions option)
		{
			bool isSupported = (option & _toolbarOptions) != 0;
			if (isSupported)
			{
				switch (option)
				{
				case ToolbarOptions.Clear:
					if (_items.Count == 0)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.MoveDown:
					if (_itemList.SelectedIndex == _itemList.Items.Count - 1)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.MoveUp:
					if (_itemList.SelectedIndex < 1)
					{
						isSupported = false;
					}
					break;
				case ToolbarOptions.Remove:
					if (_itemList.SelectedIndex == -1)
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
				_clearButton.Enabled = false;
				_moveDownButton.Enabled = false;
				_moveUpButton.Enabled = false;
				_removeButton.Enabled = false;
			}
			else
			{
				_clearButton.Enabled = _items.Count > 0;
				_moveDownButton.Enabled = _itemList.SelectedIndex < _itemList.Items.Count - 1;
				_moveUpButton.Enabled = _itemList.SelectedIndex > 0;
				_removeButton.Enabled = _itemList.SelectedIndex != -1;
			}
		}

		private void AddItem()
		{
			switch (_collectionType)
			{
			case CollectionType.GroupHeaderTotals:
				AddGroupHeaderTotal();
				break;
			case CollectionType.Fields:
				AddField();
				break;
			case CollectionType.FormatConditions:
				AddFormatCondition();
				break;
			case CollectionType.Groups:
				AddGroup();
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
			if (_itemList.SelectedItem == null)
			{
				return;
			}
			int selectedIndex = _itemList.SelectedIndex;
			_items.Remove(((ObjectName)_itemList.SelectedItem).Object);
			RefreshList();
			if (selectedIndex < _itemList.Items.Count)
			{
				_itemList.SelectedIndex = selectedIndex;
				return;
			}
			_itemList.SelectedIndex = _itemList.Items.Count - 1;
			if (_itemList.SelectedIndex == -1)
			{
				_properties.SelectedObject = null;
				RaiseSelectedObjectChanged(EventArgs.Empty);
			}
		}

		private void ClearItems()
		{
			_items.Clear();
			RefreshList();
			_properties.SelectedObject = null;
			RaiseSelectedObjectChanged(EventArgs.Empty);
		}

		private void MoveSelectedItemUp()
		{
			MoveItemUp(_itemList.SelectedIndex);
		}

		private void MoveSelectedItemDown()
		{
			MoveItemDown(_itemList.SelectedIndex);
		}

		private void RaiseSelectedObjectChanged(EventArgs eventArgs)
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
				object value = _items[index];
				int previousIndex = index - 1;
				_items[index] = _items[previousIndex];
				_items[previousIndex] = value;
				RefreshList();
				_itemList.SelectedIndex = previousIndex;
			}
		}

		private void MoveItemDown(int index)
		{
			if (index < _items.Count - 1)
			{
				object value = _items[index];
				int nextIndex = index + 1;
				_items[index] = _items[nextIndex];
				_items[nextIndex] = value;
				RefreshList();
				_itemList.SelectedIndex = nextIndex;
			}
		}

		private void SelectItem(object value)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (_items[i] == value)
				{
					_itemList.SelectedIndex = i;
					break;
				}
			}
		}

		private void AddItemAndSelect(object value)
		{
			_items.Add(value);
			RefreshList();
			SelectItem(value);
		}

		private void AddWorkingHourException()
		{
			WorkingHourException ex = new WorkingHourException();
			ex._owner = (WorkingHourExceptionCollection)_selectedObject;
			ex.Key = GenerateUniqueKey(typeof(WorkingHourException), "WorkingException", 1);
			AddItemAndSelect(ex);
		}

		private void AddHourRange()
		{
			TimelineHourRange timelineHourRange = new TimelineHourRange();
			timelineHourRange.Key = GenerateUniqueKey(typeof(TimelineHourRange), "HourRange", 1);
			AddItemAndSelect(timelineHourRange);
		}

		private void AddField()
		{
			TimelineField timelineField = new TimelineField();
			timelineField.AssignParentCollection((TimelineFieldCollection)_selectedObject);
			timelineField.Key = GenerateUniqueKey(typeof(TimelineField), "Field", 1);
			AddItemAndSelect(timelineField);
		}

		private void AddGroupHeaderTotal()
		{
			TimelineGroupHeaderTotal timelineGroupHeaderTotal = new TimelineGroupHeaderTotal();
			timelineGroupHeaderTotal.SetParentCollection((TimelineGroupHeaderTotalCollection)_selectedObject);
			timelineGroupHeaderTotal.Key = GenerateUniqueKey(typeof(TimelineGroupHeaderTotal), "GroupHeaderTotal", 1);
			AddItemAndSelect(timelineGroupHeaderTotal);
		}

		private void AddStoredFilter()
		{
			TimelineStoredFilter timelineStoredFilter = new TimelineStoredFilter();
			timelineStoredFilter.Key = GenerateUniqueKey(typeof(TimelineStoredFilter), "StoredFilter", 1);
			timelineStoredFilter.SetParentCollection((TimelineStoredFilterCollection)_selectedObject);
			AddItemAndSelect(timelineStoredFilter);
		}

		private string GenerateUniqueKey(Type componentType, string textArgument, int number)
		{
			string text = "";
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(componentType).Find("Key", ignoreCase: true);
			if (propertyDescriptor != null)
			{
				text = textArgument + number;
				foreach (object item in _items)
				{
					if (string.Compare((string)propertyDescriptor.GetValue(item), text, ignoreCase: true) == 0)
					{
						return GenerateUniqueKey(componentType, textArgument, number + 1);
					}
				}
			}
			return text;
		}

		private void AddFormatCondition()
		{
			TimelineFormatCondition timelineFormatCondition = new TimelineFormatCondition();
			timelineFormatCondition.SetFormatConditionCollection((TimelineFormatConditionCollection)_selectedObject);
			timelineFormatCondition.Key = GenerateUniqueKey(typeof(TimelineFormatCondition), "FormatCondition", 1);
			AddItemAndSelect(timelineFormatCondition);
		}

		private void AddGroup()
		{
			TimelineGroup timelineGroup = new TimelineGroup();
			timelineGroup.SetParentCollection((TimelineGroupCollection)_selectedObject);
			AddItemAndSelect(timelineGroup);
		}

		private void InitializeComponent()
		{
			_iContainer = new Container();
			ResourceManager resourceManager = new ResourceManager(typeof(CollectionPage));
			_tbOperations = new MxToolBar();
			_toolBarButton = new MxToolBarButton();
			_removeButton = new MxToolBarButton();
			_clearButton = new MxToolBarButton();
			_firstSeparator = new MxToolBarButton();
			_moveUpButton = new MxToolBarButton();
			_moveDownButton = new MxToolBarButton();
			_secondSeparator = new MxToolBarButton();
			_imageList = new ImageList(_iContainer);
			_itemList = new ListBox();
			_listSplitter = new Splitter();
			_properties = new PropertyGrid();
			SuspendLayout();
			_tbOperations.Appearance = MxToolBarAppearance.Flat;
			_tbOperations.Buttons.AddRange(new MxToolBarButton[7] { _toolBarButton, _removeButton, _clearButton, _firstSeparator, _moveUpButton, _moveDownButton, _secondSeparator });
			_tbOperations.Divider = false;
			_tbOperations.DropDownArrows = true;
			_tbOperations.ImageList = _imageList;
			_tbOperations.Name = "tbOperations";
			_tbOperations.ShowToolTips = true;
			_tbOperations.Size = new Size(496, 23);
			_tbOperations.TabIndex = 3;
			_tbOperations.TextAlign = MxToolBarTextAlign.Right;
			_tbOperations.ButtonClick += OnTbOperationsButtonClick;
			_toolBarButton.ImageIndex = 0;
			_toolBarButton.Tag = "Add";
			_toolBarButton.Text = "Add";
			_removeButton.ImageIndex = 1;
			_removeButton.Tag = "Remove";
			_removeButton.Text = "Remove";
			_clearButton.ImageIndex = 2;
			_clearButton.Tag = "Clear";
			_clearButton.Text = "Clear";
			_firstSeparator.Style = MxToolBarButtonStyle.Separator;
			_moveUpButton.ImageIndex = 3;
			_moveUpButton.Tag = "Up";
			_moveDownButton.ImageIndex = 4;
			_moveDownButton.Tag = "Down";
			_secondSeparator.Style = MxToolBarButtonStyle.Separator;
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(16, 16);
			ImageStrip.Load(_imageList, resourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_itemList.Dock = DockStyle.Left;
			_itemList.DrawMode = DrawMode.OwnerDrawFixed;
			_itemList.IntegralHeight = false;
			_itemList.Location = new Point(0, 23);
			_itemList.Name = "list";
			_itemList.Size = new Size(168, 385);
			_itemList.TabIndex = 4;
			_itemList.Resize += OnListResize;
			_itemList.DrawItem += OnListDrawItem;
			_itemList.SelectedIndexChanged += OnListSelectedIndexChanged;
			_listSplitter.Location = new Point(168, 23);
			_listSplitter.Name = "splitter1";
			_listSplitter.Size = new Size(8, 385);
			_listSplitter.TabIndex = 6;
			_listSplitter.TabStop = false;
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
			base.Controls.AddRange(new Control[4] { _properties, _listSplitter, _itemList, _tbOperations });
			base.Name = "CollectionPage";
			base.Size = new Size(496, 408);
			ResumeLayout(performLayout: false);
		}

		private void OnListSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (_itemList.SelectedIndex == -1)
			{
				_properties.SelectedObject = null;
			}
			else
			{
				_properties.SelectedObject = ((ObjectName)_itemList.SelectedItem).Object;
			}
			RaiseSelectedObjectChanged(EventArgs.Empty);
		}

		private void OnPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			if (eventArgs.ChangedItem.Label == "Key")
			{
				int selectedIndex = _itemList.SelectedIndex;
				RefreshList();
				_itemList.SelectedIndex = selectedIndex;
			}
			else if (eventArgs.ChangedItem.Label == "HasValueList")
			{
				RaiseSelectedObjectChanged(EventArgs.Empty);
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
				eventArgs.Graphics.DrawString(((ObjectName)_itemList.Items[eventArgs.Index]).Name, _itemList.Font, brush, rectangle, stringFormat);
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
			_itemList.Refresh();
		}
	}
}
