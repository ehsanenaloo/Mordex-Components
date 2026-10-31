using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Accessibility;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ComplexBindingProperties("DataSource", "DataMember")]
	[DefaultBindingProperty("Value")]
	[ToolboxItem(true)]
	[Designer("Mordex.Windows.DataGrid.Design.MultiColumnComboDesigner, Mordex.Windows.DataGrid.Design")]
	[DefaultProperty("DataSource")]
	public class MxMultiColumnComboBox : ComboBase, IGridDropListOwner, ISupportInitialize, IPersistComponentSettings
	{
		internal class MultiColumnComboAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleRole Role => AccessibleRole.ComboBox;

			public override string Value
			{
				get
				{
					return string.Concat(ComputeMultiColumnCombo().Value);
				}
				set
				{
					ComputeMultiColumnCombo().SetValueFromText(value);
					ComputeMultiColumnCombo().DroppedDown = false;
				}
			}

			internal MultiColumnComboAccessibleObject(MxMultiColumnComboBox ctl)
				: base(ctl)
			{
			}

			private MxMultiColumnComboBox ComputeMultiColumnCombo()
			{
				return (MxMultiColumnComboBox)base.Owner;
			}

			public override int GetChildCount()
			{
				MxMultiColumnComboBox multiColumnCombo = ComputeMultiColumnCombo();
				int childCount = 2;
				if (multiColumnCombo.EditControl != null)
				{
					childCount++;
				}
				return childCount;
			}

			public override AccessibleObject GetChild(int index)
			{
				MxMultiColumnComboBox multiColumnCombo = ComputeMultiColumnCombo();
				if (multiColumnCombo.TextBox != null)
				{
					if (index == 0)
					{
						return multiColumnCombo.TextBox.AccessibilityObject;
					}
					index--;
				}
				switch (index)
				{
				case 0:
					return multiColumnCombo.GetOrCreateEditAccessibleObject();
				case 1:
					return multiColumnCombo.DropDownList.AccessibilityObject;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		private GridDropDown _multiColumnComboGridDropDown;

		private bool _autoComplete = true;

		private bool _isUpdatingText;

		private object _multiColumnComboValue;

		private static readonly object _valueChangedKey = new object();

		private bool _selectInDataSource;

		private EventHandler _dropDownHandler;

		private EventHandler _closeUpHandler;

		private ComboDropDownHideEventHandler _dropDownHideHandler;

		private ApplicationSettings _multiColumnComboMordexApplicationSettings;

		private bool _multiColumnComboSaveSettings;

		private string _multiColumnComboSettingsKey = "";

		private MultiColumnComboAccessibleObject _comboAccessibleObject;

		private AccessibleObject _editAccessibleObject;

		[Description("Represents the GridDropDown object associated to the control.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridDropDown DropDownList => _multiColumnComboGridDropDown;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("'LayoutData' property is obsolete. To preserve DataGrid settings use LoadLayoutFile method instead.")]
		[Browsable(false)]
		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string LayoutData
		{
			get
			{
				PropertyValue root = new PropertyValue("DropDownLayout", _multiColumnComboGridDropDown);
				LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: false);
				return mordexLayoutWriter.ToString();
			}
			set
			{
				if (value != null && value != "")
				{
					try
					{
						LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
						mordexLayoutReader["DropDownLayout"]?.DeserializeObject(_multiColumnComboGridDropDown);
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						throw new InvalidOperationException("LayoutData stream is invalid or corrupted.");
					}
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the control should auto complete the typed text when there is a partial match in the list.")]
		public bool AutoComplete
		{
			get
			{
				return _autoComplete;
			}
			set
			{
				_autoComplete = value;
			}
		}

		[Bindable(BindableSupport.Yes)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object Value
		{
			get
			{
				return _multiColumnComboValue;
			}
			set
			{
				SetCurrentValue(value, false);
			}
		}

		[Browsable(false)]
		public bool NotInList
		{
			get
			{
				if (base.Disposing || base.IsDisposed)
				{
					return false;
				}
				if (base.ComboStyle != ComboStyle.DropDownList && Text.Length > 0 && !_multiColumnComboGridDropDown.IsDisplayTextInList(Text))
				{
					return true;
				}
				return false;
			}
		}

		AccessibleObject IGridDropListOwner.AccessibleObject => base.AccessibilityObject;

		[DefaultValue("")]
		[Description("The name of a list in a data source for which the drop down list displays its data.")]
		[Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(UITypeEditor))]
		[Category("Data")]
		public string DataMember
		{
			get
			{
				return _multiColumnComboGridDropDown.DataMember;
			}
			set
			{
				_multiColumnComboGridDropDown.DataMember = value;
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether a change in the value of the control changes the position in the CurrencyManager the control is bound to.")]
		public bool SelectInDataSource
		{
			get
			{
				return _selectInDataSource;
			}
			set
			{
				if (_selectInDataSource != value)
				{
					_selectInDataSource = value;
					_multiColumnComboGridDropDown.SetDropDownActive(value);
				}
			}
		}

		[Category("Data")]
		[Description("The source of data for the MultiColumnCombo control.")]
		[AttributeProvider(typeof(IListSource))]
		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
		public object DataSource
		{
			get
			{
				return DropDownList.DataSource;
			}
			set
			{
				DropDownList.SetDataBinding(value, DataMember);
				RefreshTextFromValue();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.DataSourceFieldsEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.DataSourceFieldsConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue("")]
		[Description("Specifies the property of the data source whose contents you want to display.")]
		public string DisplayMember
		{
			get
			{
				return _multiColumnComboGridDropDown.DisplayMember;
			}
			set
			{
				_multiColumnComboGridDropDown.DisplayMember = value;
				RefreshTextFromValue();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.DataSourceFieldsEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue("")]
		[Description("Specifies the property of the data source from which to draw the value.")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.DataSourceFieldsConverter, Mordex.Windows.DataGrid.Design")]
		public string ValueMember
		{
			get
			{
				return _multiColumnComboGridDropDown.ValueMember;
			}
			set
			{
				_multiColumnComboGridDropDown.ValueMember = value;
				RefreshTextFromValue();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public GridLayout DesignTimeLayout
		{
			get
			{
				if (_multiColumnComboGridDropDown != null)
				{
					GridLayout gridLayout = _multiColumnComboGridDropDown.BuildDesignTimeLayout();
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
				if (_multiColumnComboGridDropDown != null)
				{
					_multiColumnComboGridDropDown.ApplyDesignTimeLayout(value);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public object SelectedItem
		{
			get
			{
				return _multiColumnComboGridDropDown.FindItem(_multiColumnComboValue);
			}
			set
			{
				GridRow row = _multiColumnComboGridDropDown.GetRow(value);
				if (row != null)
				{
					object dataRow = row.DataRow;
					object valueFromDataRow = _multiColumnComboGridDropDown.GetValueFromDataRow(dataRow);
					if (valueFromDataRow != null)
					{
						Value = valueFromDataRow;
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int SelectedIndex
		{
			get
			{
				object selectedItem = SelectedItem;
				if (selectedItem != null)
				{
					GridRow row = _multiColumnComboGridDropDown.GetRow(selectedItem);
					if (row != null)
					{
						return row.Position;
					}
				}
				return -1;
			}
			set
			{
				if (value == -1)
				{
					Value = null;
					return;
				}
				GridRow row = _multiColumnComboGridDropDown.GetRow(value);
				if (row != null)
				{
					object dataRow = row.DataRow;
					object valueFromDataRow = _multiColumnComboGridDropDown.GetValueFromDataRow(dataRow);
					if (valueFromDataRow != null)
					{
						Value = valueFromDataRow;
					}
				}
			}
		}

		[Description("Indicates whether the control should automatically persist its application settings properties.")]
		[DefaultValue(false)]
		public bool SaveSettings
		{
			get
			{
				return _multiColumnComboSaveSettings;
			}
			set
			{
				_multiColumnComboSaveSettings = value;
			}
		}

		[Description("Gets or sets the value of the application settings key for the current instance of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string SettingsKey
		{
			get
			{
				if (string.IsNullOrEmpty(_multiColumnComboSettingsKey) && base.Name.Length > 0)
				{
					return base.Name;
				}
				return _multiColumnComboSettingsKey;
			}
			set
			{
				_multiColumnComboSettingsKey = value;
			}
		}

		[Description("Event fired when the Value property has changed")]
		public event EventHandler ValueChanged
		{
			add
			{
				base.Events.AddHandler(_valueChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_valueChangedKey, value);
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

		public MxMultiColumnComboBox()
		{
			UpdateMultiColumnComboGridDropDown();
		}

		protected override void Dispose(bool disposing)
		{
			if (SaveSettings)
			{
				SaveComponentSettings();
			}
			if (disposing && _multiColumnComboGridDropDown != null)
			{
				_multiColumnComboGridDropDown.SelectionChanged -= OnGridDropDownSelectionChanged;
				_multiColumnComboGridDropDown.RemoveInternalChangedHandler(OnRefreshTextRequested);
				_multiColumnComboGridDropDown.Dispose();
				_multiColumnComboGridDropDown = null;
			}
			base.Dispose(disposing);
		}

		private void UpdateMultiColumnComboGridDropDown()
		{
			_multiColumnComboGridDropDown = new GridDropDown();
			_multiColumnComboGridDropDown._isCellDropDown = true;
			_multiColumnComboGridDropDown._isLookupCacheEnabled = false;
			_multiColumnComboGridDropDown.SelectionChanged += OnGridDropDownSelectionChanged;
			_multiColumnComboGridDropDown.AssignOwnedControlStyle(base.ControlStyle, false);
			_multiColumnComboGridDropDown.AddInternalChangedHandler(OnRefreshTextRequested);
			_multiColumnComboGridDropDown._dropListOwner = this;
			_multiColumnComboGridDropDown.Visible = false;
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			if (_multiColumnComboGridDropDown != null)
			{
				_multiColumnComboGridDropDown.BindingContext = BindingContext;
				RefreshTextFromValue();
			}
		}

		private void OnRefreshTextRequested(object sender, EventArgs eventArgs)
		{
			RefreshTextFromValue();
		}

		protected internal override void OnDropControl(bool mouseCapture)
		{
			if (_multiColumnComboGridDropDown != null)
			{
				_multiColumnComboGridDropDown.EnsureGraphicsReady();
			}
			_multiColumnComboGridDropDown.SetDropDownActive(false);
			base.OnDropControl(mouseCapture);
			if (_multiColumnComboGridDropDown != null)
			{
				OnDropDown(EventArgs.Empty);
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Location = PointToScreen(clientRectangle.Location);
				clientRectangle.Height--;
				_multiColumnComboGridDropDown.RightToLeft = RightToLeft;
				_multiColumnComboGridDropDown.SelectRowByValue(Value);
				_multiColumnComboGridDropDown.ScrollSelectedRowToTop();
				_multiColumnComboGridDropDown.ShowDropDown(clientRectangle, this);
				Cursor = Cursors.Arrow;
				GridHelper.SetCapture(_multiColumnComboGridDropDown, mouseCapture);
			}
		}

		private void OnGridDropDownSelectionChanged(object sender, EventArgs eventArgs)
		{
			if (!base.DroppedDown && SelectInDataSource)
			{
				SetCurrentValue(_multiColumnComboGridDropDown.GetSelectedValue(), true);
			}
		}

		protected override void OnHideDroppedControl(bool cancel)
		{
			_multiColumnComboGridDropDown.HideDropDown(cancel);
			_multiColumnComboGridDropDown.SetDropDownActive(SelectInDataSource);
			object selectedValue = _multiColumnComboGridDropDown.GetSelectedValue();
			OnDropDownHide(new ComboDropDownHideEventArgs(!cancel));
			if (!cancel)
			{
				if (base.EditControl != null)
				{
					GridHelper.SelectAndFocus(base.EditControl);
				}
				SetCurrentValue(selectedValue, true);
				if (base.EditControl != null)
				{
					((GridMaskEdit)base.EditControl).SelectRange(Text.Length, 0);
				}
			}
			Cursor = Cursors.Default;
			base.OnHideDroppedControl(cancel);
			OnCloseUp(EventArgs.Empty);
		}

		protected override void OnMouseUp(MouseEventArgs me)
		{
			base.OnMouseUp(me);
			if (base.DroppedDown)
			{
				GridHelper.SetCapture(_multiColumnComboGridDropDown, true);
			}
		}

		protected override void OnMouseMove(MouseEventArgs me)
		{
			base.OnMouseMove(me);
			if (base.DroppedDown)
			{
				Point mousePosition = Control.MousePosition;
				if (_multiColumnComboGridDropDown.Bounds.Contains(mousePosition.X, mousePosition.Y))
				{
					GridHelper.SetCapture(_multiColumnComboGridDropDown, true);
				}
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			if (base.EditControl == null || ((GridMaskEdit)base.EditControl)._isTypingForward)
			{
				return;
			}
			if (_multiColumnComboGridDropDown != null)
			{
				if (!_isUpdatingText)
				{
					if (Text.Length == 0)
					{
						SetCurrentValue(null, true);
					}
					else if (_multiColumnComboGridDropDown.IsDisplayTextInList(Text))
					{
						SetCurrentValue(_multiColumnComboGridDropDown.FindValueByDisplayText(Text), true);
					}
					else if (Text.Length > 0)
					{
						_multiColumnComboValue = Text;
						OnValueChanged(EventArgs.Empty);
					}
					else if (_multiColumnComboValue != null)
					{
						_multiColumnComboValue = null;
						OnValueChanged(EventArgs.Empty);
					}
				}
			}
			else
			{
				SetCurrentValue(Text, true);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Return && TryCloseDropDown())
			{
				return true;
			}
			if (base.DroppedDown && _multiColumnComboGridDropDown != null && _multiColumnComboGridDropDown.HandleNavigationKey(keyData))
			{
				return true;
			}
			switch (keyData)
			{
			case Keys.Up:
				UpdateMultiColumnCombo();
				return true;
			case Keys.Down:
				SelectNextItem();
				return true;
			case Keys.Left:
			case Keys.Right:
				return true;
			default:
				return base.ProcessDialogKey(keyData);
			}
		}

		private void UpdateMultiColumnCombo()
		{
			if ((DataSource == null && DropDownList.BoundMode == BoundMode.Bound) || base.ReadOnly)
			{
				return;
			}
			_multiColumnComboGridDropDown.SelectRowByValue(Value);
			_multiColumnComboGridDropDown.MovePrevious();
			try
			{
				SetCurrentValue(_multiColumnComboGridDropDown.GetSelectedValue(), true);
				if (base.EditControl != null)
				{
					base.EditControl.SelectAll();
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private void SelectNextItem()
		{
			if ((DataSource == null && DropDownList.BoundMode == BoundMode.Bound) || base.ReadOnly)
			{
				return;
			}
			_multiColumnComboGridDropDown.SelectRowByValue(Value);
			_multiColumnComboGridDropDown.MoveNext();
			try
			{
				SetCurrentValue(_multiColumnComboGridDropDown.GetSelectedValue(), true);
				if (base.EditControl != null)
				{
					base.EditControl.SelectAll();
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		protected internal override void OnKeyPressed()
		{
			base.OnKeyPressed();
			AutoCompleteText();
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (_multiColumnComboGridDropDown != null && base.ComboStyle == ComboStyle.DropDownList && !base.ReadOnly && char.IsLetterOrDigit(e.KeyChar))
			{
				string text = e.KeyChar.ToString(GridHelper.GetCurrentCulture());
				object nextByFirstChar = _multiColumnComboGridDropDown.FindNextByFirstChar(Value, text);
				if (nextByFirstChar != null)
				{
					SetCurrentValue(nextByFirstChar, true);
				}
			}
		}

		private void AutoCompleteText()
		{
			if (_multiColumnComboGridDropDown == null || Text.Length <= 0)
			{
				return;
			}
			if (_autoComplete)
			{
				string text = Text;
				_multiColumnComboGridDropDown.SelectionChanged -= OnGridDropDownSelectionChanged;
				object selectByPrefix = _multiColumnComboGridDropDown.SelectByPrefix(text);
				_multiColumnComboGridDropDown.SelectionChanged += OnGridDropDownSelectionChanged;
				if (selectByPrefix != null)
				{
					SetCurrentValue(selectByPrefix, true);
					_isUpdatingText = true;
					Text = text + Text.Substring(text.Length);
					_isUpdatingText = false;
					((GridMaskEdit)base.EditControl).SelectRange(Text.Length, text.Length);
				}
				else
				{
					_multiColumnComboValue = text;
					OnValueChanged(EventArgs.Empty);
				}
			}
			else
			{
				Value = _multiColumnComboGridDropDown.FindValueByDisplayText(Text);
			}
		}

		internal void SetValueFromText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				Value = null;
				return;
			}
			PropertyDescriptor propertyDescriptor = _multiColumnComboGridDropDown.ComputePropertyDescriptor(ValueMember);
			if (propertyDescriptor != null)
			{
				if (propertyDescriptor.PropertyType != typeof(string))
				{
					try
					{
						object value = GridHelper.ParseValue(text, propertyDescriptor.PropertyType, Application.CurrentCulture);
						Value = value;
						return;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						Text = text;
						return;
					}
				}
				Value = text;
			}
			else
			{
				Text = text;
			}
		}

		private void SetCurrentValue(object multiColumnComboValue, bool condition)
		{
			if (GridHelper.CompareValues(multiColumnComboValue, _multiColumnComboValue, null, false) != 0)
			{
				_multiColumnComboValue = multiColumnComboValue;
				RefreshTextFromValue();
				OnValueChanged(EventArgs.Empty);
				if (condition)
				{
					base.Modified = true;
				}
				else
				{
					base.Modified = false;
				}
			}
			else
			{
				_multiColumnComboValue = multiColumnComboValue;
				RefreshTextFromValue();
			}
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			if (base.Events[_valueChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		private void RefreshTextFromValue()
		{
			bool isSettingText = _isUpdatingText;
			_isUpdatingText = true;
			try
			{
				if (_multiColumnComboGridDropDown != null && _multiColumnComboGridDropDown.ReplaceValues)
				{
					Text = _multiColumnComboGridDropDown.GetDisplayTextForValue(_multiColumnComboValue, "", false);
					if (_multiColumnComboGridDropDown.ImageColumn != null)
					{
						base.Image = _multiColumnComboGridDropDown.GetImageForValue(_multiColumnComboValue);
					}
					_multiColumnComboGridDropDown.ScrollSelectedRowToTop();
				}
				else if (GridHelper.IsNullOrDBNull(_multiColumnComboValue))
				{
					Text = "";
				}
				else
				{
					Text = GridHelper.ToSafeString(_multiColumnComboValue);
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			finally
			{
				if (_selectInDataSource && _multiColumnComboValue != null && _multiColumnComboGridDropDown.GetCurrencyManager() != null && !_multiColumnComboGridDropDown.GetCurrencyManager().IsBindingSuspended)
				{
					object dataRow = _multiColumnComboGridDropDown.FindItem(_multiColumnComboValue);
					if (dataRow != null)
					{
						GridRow row = _multiColumnComboGridDropDown.GetRow(dataRow);
						if (row != null)
						{
							DropDownList.GetCurrencyManager().Position = row.RowIndex;
						}
					}
				}
				_isUpdatingText = isSettingText;
			}
		}

		private bool TryCloseDropDown()
		{
			if (base.DroppedDown)
			{
				if (_multiColumnComboGridDropDown != null)
				{
					_multiColumnComboGridDropDown.HideDropDown(false);
					return true;
				}
				base.DroppedDown = false;
			}
			else if (_autoComplete && base.EditControl != null)
			{
				if (base.EditControl.SelectionLength > 0 && base.EditControl.SelectionLength == base.EditControl.Text.Length)
				{
					base.EditControl.SelectionStart = Text.Length;
				}
				else if (base.EditControl.SelectionLength > 0)
				{
					base.EditControl.SelectAll();
				}
			}
			return false;
		}

		void IGridDropListOwner.OnDropListHide(bool cancel)
		{
			OnHideDroppedControl(cancel);
		}

		void IGridDropListOwner.OnComputeRows()
		{
			if (_multiColumnComboValue != null)
			{
				RefreshTextFromValue();
			}
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			_multiColumnComboGridDropDown.SetDataBinding(dataSource, dataMember);
			RefreshTextFromValue();
		}

		protected override void OnImageListChanged(EventArgs e)
		{
			base.OnImageListChanged(e);
			_multiColumnComboGridDropDown.ImageList = base.ImageList;
			RefreshTextFromValue();
		}

		public override void Refresh()
		{
			_multiColumnComboGridDropDown.Refresh();
			RefreshTextFromValue();
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

		protected override void OnLostFocus(EventArgs e)
		{
			if (!base.ContainsFocus)
			{
				base.OnLostFocus(e);
				base.DroppedDown = false;
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			RefreshTextFromValue();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDesignTimeLayout()
		{
			if (_multiColumnComboGridDropDown != null)
			{
				return _multiColumnComboGridDropDown.ShouldSerializeDesignTimeLayout();
			}
			return false;
		}

		protected internal override void OnDoMouseWheel(int delta)
		{
			base.OnDoMouseWheel(delta);
			if (base.DroppedDown)
			{
				if (_multiColumnComboGridDropDown != null)
				{
					((ScrollableBase)_multiColumnComboGridDropDown).ScrollByWheelDelta(delta);
				}
			}
			else if (delta < 0)
			{
				SelectNextItem();
			}
			else
			{
				UpdateMultiColumnCombo();
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (base.DroppedDown && _multiColumnComboGridDropDown != null)
			{
				((ScrollableBase)_multiColumnComboGridDropDown).ScrollByWheelDelta(e.Delta);
			}
			base.OnMouseWheel(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			if (_multiColumnComboGridDropDown != null)
			{
				_multiColumnComboGridDropDown.Font = Font;
			}
		}

		void ISupportInitialize.BeginInit()
		{
			((ISupportInitialize)_multiColumnComboGridDropDown).BeginInit();
		}

		void ISupportInitialize.EndInit()
		{
			((ISupportInitialize)_multiColumnComboGridDropDown).EndInit();
		}

		private ApplicationSettings GetApplicationSettings()
		{
			if (_multiColumnComboMordexApplicationSettings == null)
			{
				_multiColumnComboMordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			else if (_multiColumnComboMordexApplicationSettings.SettingsKey != SettingsKey)
			{
				_multiColumnComboMordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			return _multiColumnComboMordexApplicationSettings;
		}

		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				byte[] layoutSettings = GetApplicationSettings().LayoutSettings;
				if (layoutSettings != null && layoutSettings.Length > 0)
				{
					MemoryStream memoryStream = new MemoryStream(layoutSettings);
					_multiColumnComboGridDropDown.LoadLayoutFile(memoryStream);
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
			_multiColumnComboGridDropDown.SaveLayoutFile(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			GetApplicationSettings().LayoutSettings = memoryStream.ToArray();
			GetApplicationSettings().Save();
			memoryStream.Close();
			memoryStream.Dispose();
			memoryStream = null;
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_comboAccessibleObject == null)
			{
				_comboAccessibleObject = new MultiColumnComboAccessibleObject(this);
			}
			return _comboAccessibleObject;
		}

		internal AccessibleObject GetOrCreateEditAccessibleObject()
		{
			if (_editAccessibleObject == null)
			{
				_editAccessibleObject = new EditBaseAccessibleObject(this);
			}
			return _editAccessibleObject;
		}
	}
}
