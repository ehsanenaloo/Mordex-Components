using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[LookupBindingProperties("DropDownDataSource", "DropDownDisplayMember", "DropDownValueMember", "Values")]
	[ToolboxItem(true)]
	[ComplexBindingProperties("DropDownDataSource", "DropDownDataMember")]
	[DefaultEvent("CheckedValuesChanged")]
	[DefaultProperty("DataSource")]
	[Designer("Mordex.Windows.DataGrid.Design.CheckedComboBoxDesigner, Mordex.Windows.DataGrid.Design")]
	public class MxCheckedComboBox : ComboBase, IGridDropListOwner, IPersistComponentSettings
	{
		private class CheckedValueEntry
		{
			internal object _itemValue;

			internal string _displayText;

			internal bool _isNotInList;

			internal CheckedValueEntry()
			{
			}
		}

		private List<CheckedValueEntry> _checkedEntries;

		private CheckedComboUpdateMode _checkedComboUpdateMode;

		private GridDropDown _gridDropDown;

		private DropDownListForm _dropDownDialog;

		private bool _isUpdatingText;

		private EventHandler _dropDownHandler;

		private EventHandler _closeUpHandler;

		private ComboDropDownHideEventHandler _dropDownHideHandler;

		private IList _boundValues;

		private string _valueItemDataMember;

		private CheckedBoxNotInListEventHandler _notInListHandler;

		private EventHandler _checkedValuesChangedHandler;

		private object _valuesDataSource;

		private string _valuesDataMember;

		private CurrencyManager _currencyManager;

		private ApplicationSettings _mordexApplicationSettings;

		private bool _saveSettings;

		private string _settingsKey = "";

		[Description("Represents the GridDropDown object associated to the control.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridDropDown DropDownList => _gridDropDown;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public GridLayout DesignTimeLayout
		{
			get
			{
				if (_gridDropDown != null)
				{
					GridLayout gridLayout = _gridDropDown.BuildDesignTimeLayout();
					if (gridLayout != null)
					{
						gridLayout._owner = this;
					}
					return gridLayout;
				}
				return null;
			}
			set
			{
				if (_gridDropDown != null)
				{
					_gridDropDown.ApplyDesignTimeLayout(value);
				}
			}
		}

		[DefaultValue(true)]
		public bool DropDownButtonsVisible
		{
			get
			{
				return _dropDownDialog.GetDropDownButtonsVisible();
			}
			set
			{
				_dropDownDialog.SetDropDownButtonsVisible(value);
			}
		}

		[Description("The source of data for the dropdown list.")]
		[DefaultValue(null)]
		[AttributeProvider(typeof(IListSource))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
		[Category("DropDown DataBinding")]
		public object DropDownDataSource
		{
			get
			{
				return _gridDropDown.DataSource;
			}
			set
			{
				_gridDropDown.DataSource = value;
			}
		}

		[Description("The name of a list in a data source for which the drop down list displays its data.")]
		[DefaultValue("")]
		[Category("DropDown DataBinding")]
		[Editor("Mordex.Windows.DataGrid.Design.ListDataMemberEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public string DropDownDataMember
		{
			get
			{
				return _gridDropDown.DataMember;
			}
			set
			{
				_gridDropDown.DataMember = value;
			}
		}

		protected override bool UseHighlightColorCore
		{
			get
			{
				if (base.DroppedDown)
				{
					return true;
				}
				return base.UseHighlightColorCore;
			}
			set
			{
				base.UseHighlightColorCore = value;
			}
		}

		AccessibleObject IGridDropListOwner.AccessibleObject => base.AccessibilityObject;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object[] CheckedValues
		{
			get
			{
				if (_checkedEntries.Count == 0)
				{
					return null;
				}
				object[] array = new object[_checkedEntries.Count];
				for (int i = 0; i < _checkedEntries.Count; i++)
				{
					array[i] = _checkedEntries[i]._itemValue;
				}
				return array;
			}
			set
			{
				if (!AreValuesDifferent(CheckedValues, value))
				{
					return;
				}
				_checkedEntries.Clear();
				if (value != null)
				{
					foreach (object otherValue in value)
					{
						_checkedEntries.Add(CreateCheckedEntry(otherValue));
					}
				}
				RefreshDisplayText(false);
				OnCheckedValuesChanged(EventArgs.Empty);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object[] NotInListItems
		{
			get
			{
				ArrayList arrayList = new ArrayList();
				for (int i = 0; i < _checkedEntries.Count; i++)
				{
					if (_checkedEntries[i]._isNotInList)
					{
						arrayList.Add(_checkedEntries[i]._itemValue);
					}
				}
				if (arrayList.Count == 0)
				{
					return null;
				}
				object[] array = new object[arrayList.Count];
				arrayList.CopyTo(array);
				return array;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object[] CheckedItems
		{
			get
			{
				if (_checkedEntries.Count == 0)
				{
					return null;
				}
				object[] array = new object[_checkedEntries.Count];
				for (int i = 0; i < _checkedEntries.Count; i++)
				{
					array[i] = _gridDropDown.FindItem(_checkedEntries[i]._itemValue);
				}
				return array;
			}
			set
			{
				if (value != null)
				{
					object[] array = new object[value.Length];
					for (int i = 0; i < value.Length; i++)
					{
						object valueFromDataRow = _gridDropDown.GetValueFromDataRow(value[i]);
						array[i] = valueFromDataRow;
					}
					CheckedValues = array;
				}
				else
				{
					CheckedValues = null;
				}
			}
		}

		[DefaultValue("")]
		[Category("DropDown DataBinding")]
		public string DropDownValueMember
		{
			get
			{
				return _gridDropDown.ValueMember;
			}
			set
			{
				_gridDropDown.ValueMember = value;
			}
		}

		[Category("DropDown DataBinding")]
		[DefaultValue("")]
		public string DropDownDisplayMember
		{
			get
			{
				return _gridDropDown.DisplayMember;
			}
			set
			{
				_gridDropDown.DisplayMember = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		[Browsable(false)]
		public IList Values
		{
			get
			{
				return _boundValues;
			}
			set
			{
				if (value == null)
				{
					_boundValues = null;
					CheckedValues = null;
				}
				else
				{
					if (value == null)
					{
						throw new ArgumentException("BindableValue must be a collection that implements IList");
					}
					_boundValues = value;
				}
				LoadCheckedValuesFromValues();
			}
		}

		[Bindable(BindableSupport.No)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[DefaultValue("")]
		public string ValueItemDataMember
		{
			get
			{
				return _valueItemDataMember;
			}
			set
			{
				if (_valueItemDataMember != value)
				{
					_valueItemDataMember = value;
					LoadCheckedValuesFromValues();
				}
			}
		}

		[DefaultValue("OK")]
		[Localizable(true)]
		public string ButtonOKText
		{
			get
			{
				return _dropDownDialog._okCaption;
			}
			set
			{
				_dropDownDialog._okCaption = value;
			}
		}

		[DefaultValue("Cancel")]
		[Localizable(true)]
		public string ButtonCancelText
		{
			get
			{
				return _dropDownDialog._cancelCaption;
			}
			set
			{
				_dropDownDialog._cancelCaption = value;
			}
		}

		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
		[DefaultValue(null)]
		[AttributeProvider(typeof(IListSource))]
		public object ValuesDataSource
		{
			get
			{
				return _valuesDataSource;
			}
			set
			{
				if (_valuesDataSource != value)
				{
					if (_valuesDataSource is IComponent)
					{
						((IComponent)_valuesDataSource).Disposed -= OnValuesDataSourceDisposed;
					}
					_valuesDataSource = value;
					RefreshValuesCurrencyManager();
					if (_valuesDataSource is IComponent)
					{
						((IComponent)_valuesDataSource).Disposed += OnValuesDataSourceDisposed;
					}
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ListDataMemberEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue("")]
		public string ValuesDataMember
		{
			get
			{
				return _valuesDataMember;
			}
			set
			{
				if (_valuesDataMember != value)
				{
					_valuesDataMember = value;
					RefreshValuesCurrencyManager();
				}
			}
		}

		[Description("Indicates whether the control should automatically persist its application settings properties.")]
		[DefaultValue(true)]
		public bool SaveSettings
		{
			get
			{
				return _saveSettings;
			}
			set
			{
				_saveSettings = value;
			}
		}

		[Description("Gets or sets the value of the application settings key for the current instance of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string SettingsKey
		{
			get
			{
				if (string.IsNullOrEmpty(_settingsKey) && base.Name.Length > 0)
				{
					return base.Name;
				}
				return _settingsKey;
			}
			set
			{
				_settingsKey = value;
			}
		}

		[Description("Event fired when the drop down list is shown")]
		public event EventHandler DropDown
		{
			add
			{
				EventHandler eventHandler = _dropDownHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dropDownHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _dropDownHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dropDownHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		[Description("Event fired when the drop down list is closed")]
		public event EventHandler CloseUp
		{
			add
			{
				EventHandler eventHandler = _closeUpHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _closeUpHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _closeUpHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _closeUpHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		[Description("Event fired when the drop down list is hidden")]
		public event ComboDropDownHideEventHandler DropDownHide
		{
			add
			{
				ComboDropDownHideEventHandler comboDropDownHideEventHandler = _dropDownHideHandler;
				ComboDropDownHideEventHandler otherComboDropDownHideEventHandler;
				do
				{
					otherComboDropDownHideEventHandler = comboDropDownHideEventHandler;
					ComboDropDownHideEventHandler otherValue = (ComboDropDownHideEventHandler)Delegate.Combine(otherComboDropDownHideEventHandler, value);
					comboDropDownHideEventHandler = Interlocked.CompareExchange(ref _dropDownHideHandler, otherValue, otherComboDropDownHideEventHandler);
				}
				while ((object)comboDropDownHideEventHandler != otherComboDropDownHideEventHandler);
			}
			remove
			{
				ComboDropDownHideEventHandler comboDropDownHideEventHandler = _dropDownHideHandler;
				ComboDropDownHideEventHandler otherComboDropDownHideEventHandler;
				do
				{
					otherComboDropDownHideEventHandler = comboDropDownHideEventHandler;
					ComboDropDownHideEventHandler otherValue = (ComboDropDownHideEventHandler)Delegate.Remove(otherComboDropDownHideEventHandler, value);
					comboDropDownHideEventHandler = Interlocked.CompareExchange(ref _dropDownHideHandler, otherValue, otherComboDropDownHideEventHandler);
				}
				while ((object)comboDropDownHideEventHandler != otherComboDropDownHideEventHandler);
			}
		}

		public event CheckedBoxNotInListEventHandler NotInList
		{
			add
			{
				CheckedBoxNotInListEventHandler checkedBoxNotInListEventHandler = _notInListHandler;
				CheckedBoxNotInListEventHandler otherCheckedBoxNotInListEventHandler;
				do
				{
					otherCheckedBoxNotInListEventHandler = checkedBoxNotInListEventHandler;
					CheckedBoxNotInListEventHandler otherValue = (CheckedBoxNotInListEventHandler)Delegate.Combine(otherCheckedBoxNotInListEventHandler, value);
					checkedBoxNotInListEventHandler = Interlocked.CompareExchange(ref _notInListHandler, otherValue, otherCheckedBoxNotInListEventHandler);
				}
				while ((object)checkedBoxNotInListEventHandler != otherCheckedBoxNotInListEventHandler);
			}
			remove
			{
				CheckedBoxNotInListEventHandler checkedBoxNotInListEventHandler = _notInListHandler;
				CheckedBoxNotInListEventHandler otherCheckedBoxNotInListEventHandler;
				do
				{
					otherCheckedBoxNotInListEventHandler = checkedBoxNotInListEventHandler;
					CheckedBoxNotInListEventHandler otherValue = (CheckedBoxNotInListEventHandler)Delegate.Remove(otherCheckedBoxNotInListEventHandler, value);
					checkedBoxNotInListEventHandler = Interlocked.CompareExchange(ref _notInListHandler, otherValue, otherCheckedBoxNotInListEventHandler);
				}
				while ((object)checkedBoxNotInListEventHandler != otherCheckedBoxNotInListEventHandler);
			}
		}

		public event EventHandler CheckedValuesChanged
		{
			add
			{
				EventHandler eventHandler = _checkedValuesChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _checkedValuesChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _checkedValuesChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _checkedValuesChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		public MxCheckedComboBox()
		{
			UpdateCheckedComboBox();
		}

		protected override void Dispose(bool disposing)
		{
			if (SaveSettings)
			{
				SaveComponentSettings();
			}
			if (disposing)
			{
				if (_gridDropDown != null)
				{
					_gridDropDown.RemoveInternalChangedHandler(HandleDropDownChanged);
					_gridDropDown.Dispose();
					_gridDropDown = null;
				}
				if (_dropDownDialog != null)
				{
					_dropDownDialog.SetDropDownOwner(null);
					_dropDownDialog.Dispose();
					_dropDownDialog = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			RefreshValuesCurrencyManager();
			if (_gridDropDown != null)
			{
				_gridDropDown.BindingContext = BindingContext;
				RefreshDisplayText(true);
			}
		}

		private void UpdateCheckedComboBox()
		{
			_checkedEntries = new List<CheckedValueEntry>();
			_gridDropDown = new GridDropDown();
			_gridDropDown._dropListOwner = this;
			_gridDropDown._gridDropDownOwner = this;
			_gridDropDown.AddInternalChangedHandler(HandleDropDownChanged);
			_gridDropDown.ImageList = base.ImageList;
			_dropDownDialog = new DropDownListForm(_gridDropDown, null);
			_dropDownDialog.SetDropDownOwner(this);
			_gridDropDown.AssignOwnedControlStyle(base.ControlStyle, false);
			_checkedComboUpdateMode = CheckedComboUpdateMode.Automatic;
			_valueItemDataMember = string.Empty;
		}

		protected internal override void OnDropControl(bool mouseCapture)
		{
			base.OnDropControl(mouseCapture);
			if (_gridDropDown != null)
			{
				OnDropDown(EventArgs.Empty);
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Location = PointToScreen(clientRectangle.Location);
				clientRectangle.Height--;
				_gridDropDown.RightToLeft = RightToLeft;
				_dropDownDialog._form = FindForm();
				_dropDownDialog.Font = Font;
				_dropDownDialog.ApplyAnchorBounds(clientRectangle);
				_dropDownDialog.CheckSelectedValues(CheckedValues);
				Cursor = Cursors.Arrow;
			}
		}

		protected override void OnImageListChanged(EventArgs e)
		{
			base.OnImageListChanged(e);
			_gridDropDown.ImageList = base.ImageList;
			RefreshDisplayText(false);
		}

		private void HandleDropDownChanged(object sender, EventArgs eventArgs)
		{
			RefreshDisplayText(true);
		}

		private void RefreshDisplayText(bool condition)
		{
			_isUpdatingText = true;
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < _checkedEntries.Count; i++)
			{
				if (condition)
				{
					_checkedEntries[i] = CreateCheckedEntry(_checkedEntries[i]._itemValue);
				}
				string text = "";
				text = _checkedEntries[i]._displayText;
				stringBuilder.Append(text);
				if (i < _checkedEntries.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			Text = stringBuilder.ToString();
			SelectAll();
			_isUpdatingText = false;
			Invalidate();
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			ParseTextToCheckedList();
		}

		private void ParseTextToCheckedList()
		{
			if (!_isUpdatingText)
			{
				object[] checkedValues = CheckedValues;
				string[] array = Text.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
				_checkedEntries.Clear();
				string[] otherArray = array;
				foreach (string text in otherArray)
				{
					string otherValueByDisplayText = text.Trim();
					object valueByDisplayText = _gridDropDown.FindValueByDisplayText(otherValueByDisplayText);
					_checkedEntries.Add(CreateCheckedEntry(valueByDisplayText));
				}
				if (AreValuesDifferent(checkedValues, CheckedValues))
				{
					OnCheckedValuesChanged(EventArgs.Empty);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDesignTimeLayout()
		{
			if (_gridDropDown != null)
			{
				return _gridDropDown.ShouldSerializeDesignTimeLayout();
			}
			return false;
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			_gridDropDown.SetDataBinding(dataSource, dataMember);
		}

		public void RetrieveStructure()
		{
			_gridDropDown.RetrieveStructure();
			GridColumn gridColumn = new GridColumn();
			gridColumn.Width = 24;
			gridColumn.ActAsSelector = true;
			gridColumn.UseHeaderSelector = false;
			gridColumn.Key = base.Name + "_Selector";
			_gridDropDown.Columns.Insert(0, gridColumn);
		}

		protected override void PaintText(Graphics g, Brush foreBrush, Rectangle textRect)
		{
			if (base.ComboStyle == ComboStyle.DropDownList)
			{
				ListCellRenderer.DrawListItems(g, textRect, CheckedValues, _gridDropDown, base.ImageList, Font, foreBrush, ComputeTextLayoutRenderer(), RightToLeft == RightToLeft.Yes);
			}
			else
			{
				base.PaintText(g, foreBrush, textRect);
			}
		}

		protected override void OnHideDroppedControl(bool cancel)
		{
			OnDropDownHide(new ComboDropDownHideEventArgs(!cancel));
			if (!cancel)
			{
				object[] checkedValues = CheckedValues;
				LoadCheckedListFromDropDown();
				RefreshDisplayText(false);
				if (AreValuesDifferent(checkedValues, CheckedValues))
				{
					OnCheckedValuesChanged(EventArgs.Empty);
				}
			}
			Cursor = Cursors.Default;
			base.OnHideDroppedControl(cancel);
			if (!base.DroppedDown)
			{
				try
				{
					FindForm()?.Activate();
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
				if (NativeMethods.IsWindowVisible(base.Handle))
				{
					if (base.EditControl != null)
					{
						GridHelper.SelectAndFocus(base.EditControl);
					}
					else
					{
						GridHelper.SelectAndFocus(this);
					}
				}
				_dropDownDialog._form = null;
				_dropDownDialog.Owner = null;
			}
			OnCloseUp(EventArgs.Empty);
		}

		protected virtual void OnDropDown(EventArgs e)
		{
			if (_dropDownHandler != null)
			{
				_dropDownHandler(this, e);
			}
		}

		protected virtual void OnCloseUp(EventArgs e)
		{
			if (_closeUpHandler != null)
			{
				_closeUpHandler(this, e);
			}
		}

		protected virtual void OnDropDownHide(ComboDropDownHideEventArgs e)
		{
			if (_dropDownHideHandler != null)
			{
				_dropDownHideHandler(this, e);
			}
		}

		void IGridDropListOwner.OnDropListHide(bool cancel)
		{
			OnHideDroppedControl(cancel);
		}

		void IGridDropListOwner.OnComputeRows()
		{
		}

		private static bool AreValuesDifferent(object value, object otherItem)
		{
			if (value == null && otherItem == null)
			{
				return false;
			}
			if (value is IList && otherItem is IList)
			{
				IList list = (IList)value;
				IList otherList = (IList)otherItem;
				if (list.Count != otherList.Count)
				{
					return true;
				}
				foreach (object item in list)
				{
					if (!otherList.Contains(item))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}

		private CheckedValueEntry CreateCheckedEntry(object value)
		{
			CheckedValueEntry a = new CheckedValueEntry();
			a._itemValue = value;
			object item = _gridDropDown.FindItem(value);
			if (item == null)
			{
				a._displayText = string.Concat(value);
				a._isNotInList = true;
			}
			else
			{
				a._isNotInList = false;
				if (_gridDropDown.ReplaceValues)
				{
					a._displayText = _gridDropDown.GetDisplayTextForValue(value, "", false);
				}
				else
				{
					a._displayText = string.Concat(value);
				}
			}
			return a;
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			if (_checkedComboUpdateMode == CheckedComboUpdateMode.Automatic)
			{
				e.Cancel = !UpdateValueDataSource();
			}
			if (!e.Cancel)
			{
				base.OnValidating(e);
			}
		}

		private void LoadCheckedValuesFromValues()
		{
			if (_boundValues == null)
			{
				CheckedValues = null;
				return;
			}
			ArrayList arrayList = new ArrayList();
			foreach (object item in _boundValues)
			{
				if (_valueItemDataMember != null && _valueItemDataMember.Length > 0)
				{
					PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(item);
					if (properties != null)
					{
						PropertyDescriptor propertyDescriptor = properties.Find(_valueItemDataMember, ignoreCase: true);
						if (propertyDescriptor != null)
						{
							arrayList.Add(propertyDescriptor.GetValue(item));
							continue;
						}
					}
				}
				arrayList.Add(item);
			}
			object[] array = new object[arrayList.Count];
			arrayList.CopyTo(array);
			CheckedValues = array;
		}

		private object ResolveItemValue(object value)
		{
			return null;
		}

		public bool UpdateValueDataSource()
		{
			if (_boundValues != null)
			{
				bool condition = false;
				for (int index = _checkedEntries.Count - 1; index >= 0; index--)
				{
					if (_checkedEntries[index]._isNotInList)
					{
						CheckedBoxNotInListEventArs checkedBoxNotInListEventArs = new CheckedBoxNotInListEventArs(_checkedEntries[index]._displayText);
						OnNotInList(checkedBoxNotInListEventArs);
						switch (checkedBoxNotInListEventArs.Action)
						{
						case NotInListAction.Cancel:
							return false;
						case NotInListAction.Discard:
							_checkedEntries.RemoveAt(index);
							break;
						case NotInListAction.Continue:
							condition = true;
							break;
						}
					}
				}
				if (condition)
				{
					RefreshDisplayText(true);
				}
				IList list = null;
				list = ((CheckedValues != null) ? ((IList)CheckedValues) : ((IList)new ArrayList()));
				bool[] array = new bool[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					array[i] = false;
				}
				IList values = _boundValues;
				PropertyDescriptor propertyDescriptor = null;
				try
				{
					for (int otherIndex = values.Count - 1; otherIndex >= 0; otherIndex--)
					{
						object component = values[otherIndex];
						object value = component;
						if (propertyDescriptor == null && _valueItemDataMember != null && _valueItemDataMember.Length > 0)
						{
							PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
							if (properties != null)
							{
								propertyDescriptor = properties.Find(_valueItemDataMember, ignoreCase: true);
							}
						}
						if (propertyDescriptor != null)
						{
							value = propertyDescriptor.GetValue(component);
						}
						int otherValue = list.IndexOf(value);
						if (otherValue == -1)
						{
							values.RemoveAt(otherIndex);
						}
						else
						{
							array[otherValue] = true;
						}
					}
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j])
						{
							continue;
						}
						object itemValue = ResolveItemValue(list[j]);
						if (itemValue != null)
						{
							if (!values.Contains(itemValue))
							{
								values.Add(itemValue);
							}
						}
						else if (values is IBindingList)
						{
							itemValue = ((IBindingList)values).AddNew();
							if (propertyDescriptor == null && _valueItemDataMember != null && _valueItemDataMember.Length > 0)
							{
								PropertyDescriptorCollection otherProperties = TypeDescriptor.GetProperties(itemValue);
								if (otherProperties != null)
								{
									propertyDescriptor = otherProperties.Find(_valueItemDataMember, ignoreCase: true);
								}
							}
							propertyDescriptor?.SetValue(itemValue, list[j]);
							if (itemValue is IEditableObject)
							{
								((IEditableObject)itemValue).EndEdit();
							}
						}
						else
						{
							values.Add(list[j]);
						}
					}
				}
				catch (Exception ex)
				{
					ShowErrorMessage(ex);
					return false;
				}
			}
			RefreshDisplayText(false);
			Invalidate();
			return true;
		}

		private void ShowErrorMessage(Exception exception)
		{
			Mordex.Windows.Common.MxMessageBox.Show(exception.Message);
		}

		protected virtual void OnNotInList(CheckedBoxNotInListEventArs e)
		{
			if (_notInListHandler != null)
			{
				_notInListHandler(this, e);
			}
		}

		protected virtual void OnCheckedValuesChanged(EventArgs e)
		{
			if (_checkedValuesChangedHandler != null)
			{
				_checkedValuesChangedHandler(this, e);
			}
		}

		public void CheckAll()
		{
			_gridDropDown.CheckAllRecords();
			object[] checkedValues = CheckedValues;
			LoadCheckedListFromDropDown();
			RefreshDisplayText(false);
			if (AreValuesDifferent(checkedValues, CheckedValues))
			{
				OnCheckedValuesChanged(EventArgs.Empty);
			}
		}

		internal void LoadCheckedListFromDropDown()
		{
			_checkedEntries.Clear();
			if (_gridDropDown == null)
			{
				return;
			}
			GridRow[] checkedRows = _gridDropDown.GetCheckedRows();
			if (checkedRows == null || checkedRows.Length <= 0)
			{
				return;
			}
			GridColumn gridColumn = null;
			gridColumn = _gridDropDown.FindColumnByKey(_gridDropDown.ValueMember);
			GridColumn columnByKey = _gridDropDown.FindColumnByKey(_gridDropDown.DisplayMember);
			foreach (GridRow gridRow in checkedRows)
			{
				CheckedValueEntry a = new CheckedValueEntry();
				if (gridColumn != null)
				{
					a._itemValue = gridRow.Cells[gridColumn].Value;
				}
				else
				{
					a._itemValue = _gridDropDown.GetValueFromDataRow(gridRow.DataRow);
				}
				if (_gridDropDown.ReplaceValues)
				{
					if (columnByKey != null)
					{
						a._displayText = gridRow.Cells[columnByKey].Text;
					}
					else
					{
						a._displayText = _gridDropDown.GetDisplayTextForValue(a._itemValue, "", true);
					}
				}
				else
				{
					a._displayText = string.Concat(a._itemValue);
				}
				_checkedEntries.Add(a);
			}
		}

		public void UncheckAll()
		{
			CheckedValues = null;
			if (_gridDropDown != null)
			{
				_gridDropDown.UnCheckAllRecords();
			}
		}

		private void UpdateCurrencyManager(CurrencyManager currencyManager)
		{
			if (currencyManager != _currencyManager)
			{
				if (_currencyManager != null)
				{
					_currencyManager.ItemChanged -= OnCurrencyManagerItemChanged;
				}
				_currencyManager = currencyManager;
				if (_currencyManager != null)
				{
					Values = _currencyManager.List;
					_currencyManager.ItemChanged += OnCurrencyManagerItemChanged;
				}
				else
				{
					Values = null;
				}
			}
		}

		private void OnCurrencyManagerItemChanged(object sender, ItemChangedEventArgs eventArgs)
		{
			Values = _currencyManager.List;
		}

		private void RefreshValuesCurrencyManager()
		{
			if (!base.IsHandleCreated)
			{
				return;
			}
			CurrencyManager currencyManager = null;
			if (_valuesDataSource != null)
			{
				try
				{
					currencyManager = BindingContext[_valuesDataSource, _valuesDataMember] as CurrencyManager;
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					ValuesDataMember = "";
					return;
				}
			}
			UpdateCurrencyManager(currencyManager);
		}

		private void OnValuesDataSourceDisposed(object sender, EventArgs eventArgs)
		{
			_valuesDataMember = "";
			ValuesDataSource = null;
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			if (_gridDropDown != null)
			{
				_gridDropDown.Font = Font;
			}
		}

		private ApplicationSettings GetApplicationSettings()
		{
			if (_mordexApplicationSettings == null)
			{
				_mordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			else if (_mordexApplicationSettings.SettingsKey != SettingsKey)
			{
				_mordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			return _mordexApplicationSettings;
		}

		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				byte[] layoutSettings = GetApplicationSettings().LayoutSettings;
				if (layoutSettings != null && layoutSettings.Length > 0)
				{
					MemoryStream memoryStream = new MemoryStream(layoutSettings);
					_gridDropDown.LoadLayoutFile(memoryStream);
					memoryStream.Close();
					memoryStream.Dispose();
					memoryStream = null;
				}
			}
		}

		public void ResetComponentSettings()
		{
			GetApplicationSettings().Reset();
		}

		public void SaveComponentSettings()
		{
			MemoryStream memoryStream = new MemoryStream();
			_gridDropDown.SaveLayoutFile(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			GetApplicationSettings().LayoutSettings = memoryStream.ToArray();
			GetApplicationSettings().Save();
			memoryStream.Close();
			memoryStream.Dispose();
			memoryStream = null;
		}
	}
}
