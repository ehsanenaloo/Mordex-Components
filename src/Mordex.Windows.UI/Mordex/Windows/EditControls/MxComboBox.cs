using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.Controls
{
	[DefaultBindingProperty("SelectedValue")]
	[ToolboxItem(true)]
	[DefaultEvent("SelectedIndexChanged")]
	[LookupBindingProperties("DataSource", "DisplayMember", "ValueMember", "SelectedValue")]
	public partial class MxComboBox : MxComboBase
	{
		private readonly Container _container = new Container();

		private bool _shouldClearText = true;

		private bool _raiseSelectionEvents = true;

		private static readonly object _dataSourceChangedKey = new object();

		private ComboBoxItemCollection _baseItems;

		private bool _sorted;

		private object _dataSource;

		private bool _selectInDataSource;

		private BindingMemberInfo _valueMemberInfo;

		private BindingMemberInfo _displayMemberInfo;

		private BindingMemberInfo _dataMemberInfo;

		private bool _hasBindingContext;

		private CurrencyManager _currencyManager;

		private IList _boundList;

		private CurrencyManager _activeCurrencyManager;

		private BindingContext _bindingContext;

		private IList _itemList;

		private bool _shouldResetSelectedValue = true;

		private ComboBoxItem _selectedItem;

		private EventHandler _selectedItemChangedHandler;

		private static readonly object _selectedIndexChangedKey = new object();

		private EventHandler _selectedValueChangedHandler;

		private bool _autoComplete = true;

		private object _selectedValue;

		private bool _isUpdatingText;

		[Description("Represents the items in the control")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ComboBoxItemCollection Items => _baseItems;

		[Description("Specifies whether the items in the combo box are sorted. ")]
		[DefaultValue(false)]
		public bool Sorted
		{
			get
			{
				return _sorted;
			}
			set
			{
				if (_sorted != value)
				{
					_sorted = value;
					if (_sorted)
					{
						_baseItems.Sort();
					}
					else
					{
						_baseItems.RestoreOriginalOrder();
					}
				}
			}
		}

		[Category("Data")]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
		[DefaultValue(null)]
		[Description("The data source for the dropdown list.")]
		[AttributeProvider(typeof(IListSource))]
		public object DataSource
		{
			get
			{
				return _dataSource;
			}
			set
			{
				SetDataSourceEmpty(value == null);
				BindDataSource(value, _dataMemberInfo, true);
				OnDataSourceChanged(EventArgs.Empty);
				if (value == null)
				{
					DataMember = "";
				}
			}
		}

		[Description("Specifies whether a change in the SelectedValue of the control changes the position in the CurrencyManager the control is bound to.")]
		[DefaultValue(false)]
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
				}
			}
		}

		public override Cursor Cursor
		{
			get
			{
				if (base.DroppedDown)
				{
					return Cursors.Arrow;
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		[Category("Data")]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[DefaultValue("")]
		[Description("Specifies the property of the data source from which to draw the value.")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		public string ValueMember
		{
			get
			{
				return _valueMemberInfo.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_valueMemberInfo))
				{
					return;
				}
				if (!IsDataSourceEmpty())
				{
					if (DataMember.Length == 0)
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_valueMemberInfo = bindingMemberInfo;
					if (_selectedItem != null)
					{
						_selectedValue = _selectedItem.Value;
					}
					ReloadItems();
				}
				else
				{
					_valueMemberInfo = bindingMemberInfo;
				}
			}
		}

		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[Description("Specifies the property of the data source whose contents you want to display.")]
		[DefaultValue("")]
		[Category("Data")]
		public string DisplayMember
		{
			get
			{
				return _displayMemberInfo.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_displayMemberInfo))
				{
					return;
				}
				if (!IsDataSourceEmpty())
				{
					if (DataMember.Length == 0)
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_displayMemberInfo = bindingMemberInfo;
					ReloadItems();
				}
				else
				{
					_displayMemberInfo = bindingMemberInfo;
				}
			}
		}

		[Description("The name of a list in a data source for which the drop down list displays its data.")]
		[Category("Data")]
		[Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(UITypeEditor))]
		[DefaultValue("")]
		public string DataMember
		{
			get
			{
				return _dataMemberInfo.BindingMember;
			}
			set
			{
				BindingMemberInfo bindingMemberInfo = _dataMemberInfo;
				try
				{
					if (IsDataSourceEmpty())
					{
						_dataMemberInfo = new BindingMemberInfo(value);
					}
					BindDataSource(_dataSource, new BindingMemberInfo(value), false);
				}
				catch
				{
					_dataMemberInfo = bindingMemberInfo;
					throw;
				}
			}
		}

		protected CurrencyManager CurrencyManager => _currencyManager;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		[Browsable(false)]
		public ComboBoxItem SelectedItem
		{
			get
			{
				return _selectedItem;
			}
			set
			{
				if (_selectedItem == value)
				{
					return;
				}
				_selectedItem = value;
				if (value == null)
				{
					if (_shouldResetSelectedValue)
					{
						_selectedValue = null;
						if (_shouldClearText)
						{
							base.Text = "";
						}
					}
					SetImage(null);
					SetImageIndex(-1);
					SetImageKey("");
				}
				else
				{
					_selectedValue = value.Value;
					_isUpdatingText = true;
					base.Text = value.Text;
					_isUpdatingText = false;
					if (value.Image != null)
					{
						SetImage(value.Image);
					}
					else
					{
						SetImage(null);
						SetImageIndex(value.ImageIndex);
						SetImageKey(value.ImageKey);
					}
				}
				SyncPositionWithSelection();
				if (_formatStyleRenderer != null)
				{
					_formatStyleRenderer.SetHighlightedItem(_selectedItem);
				}
				if (_raiseSelectionEvents)
				{
					OnSelectedItemChanged(EventArgs.Empty);
					OnSelectedIndexChanged(EventArgs.Empty);
					OnSelectedValueChanged(EventArgs.Empty);
				}
				Invalidate();
			}
		}

		[Browsable(false)]
		[DefaultValue(-1)]
		public int SelectedIndex
		{
			get
			{
				if (_selectedItem == null)
				{
					return -1;
				}
				return _selectedItem._listIndex;
			}
			set
			{
				if (SelectedIndex != value)
				{
					if (value >= Items.Count)
					{
						throw new IndexOutOfRangeException("Specified argument was out of the range of valid values");
					}
					if (value < 0)
					{
						SelectedItem = null;
						return;
					}
					ComboBoxItem selectedItem = FindItemBySelectedIndex(value);
					SelectedItem = selectedItem;
				}
			}
		}

		[DefaultValue(true)]
		[Description("Specifies whether the control should complete the typed text when there is a partial match in the list.")]
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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object SelectedValue
		{
			get
			{
				return _selectedValue;
			}
			set
			{
				if (RenderingHelper.CompareValuesCurrentCulture(value, _selectedValue, null, true) == 0)
				{
					return;
				}
				_selectedValue = value;
				if (_selectedValue == null)
				{
					ComboBoxItem uiComboBoxItem = _selectedItem;
					base.Text = "";
					_selectedItem = uiComboBoxItem;
					if (SelectedItem == null)
					{
						OnSelectedValueChanged(EventArgs.Empty);
					}
					SelectedItem = null;
					return;
				}
				try
				{
					ComboBoxItem matchedItem = Items[_selectedValue];
					if (matchedItem != null)
					{
						SelectedItem = matchedItem;
					}
					else if (matchedItem == SelectedItem)
					{
						OnSelectedValueChanged(EventArgs.Empty);
					}
					else
					{
						_shouldResetSelectedValue = false;
						SelectedItem = null;
						_shouldResetSelectedValue = true;
					}
					if (_selectedValue != null && _selectedItem == null)
					{
						_isUpdatingText = true;
						base.Text = _selectedValue.ToString();
						_isUpdatingText = false;
					}
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// A value that the bound list cannot match or convert leaves the combo with no selection.
					SelectedItem = null;
				}
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		[Bindable(BindableSupport.Yes)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				if (!(value != Text))
				{
					return;
				}
				_isUpdatingText = true;
				base.Text = value;
				_isUpdatingText = false;
				ComboBoxItem uiComboBoxItem = Items[value];
				if (uiComboBoxItem != null)
				{
					if (uiComboBoxItem != SelectedItem)
					{
						_shouldClearText = false;
						SelectedItem = uiComboBoxItem;
						_shouldClearText = true;
					}
					return;
				}
				if (base.Text.Length > 0)
				{
					_selectedValue = base.Text;
				}
				else
				{
					_selectedValue = null;
				}
				OnSelectedValueChanged(EventArgs.Empty);
				SyncPositionWithSelection();
				if (_selectedItem != null)
				{
					_selectedItem = null;
					SetImage(null);
					OnSelectedItemChanged(EventArgs.Empty);
					OnSelectedIndexChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public bool NotInList
		{
			get
			{
				if (_selectedItem == null)
				{
					return true;
				}
				return false;
			}
		}

		public MxComboBox()
		{
			_baseItems = new ComboBoxItemCollection(this);
			_formatStyleRenderer.SetDropDownItems(_baseItems);
			Text = "";
		}

		internal MxComboBox(ComboBoxItemCollection baseItems)
		{
			_baseItems = baseItems;
			_formatStyleRenderer.SetDropDownItems(baseItems);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_container != null)
				{
					_container.Dispose();
				}
				if (_formatStyleRenderer != null)
				{
					_formatStyleRenderer.Dispose();
					_formatStyleRenderer = null;
				}
			}
			UnsubscribeFromCurrencyManager();
			base.Dispose(disposing);
		}

		protected internal override void DropListControl(bool setCapture)
		{
			CancelEventArgs e = new CancelEventArgs();
			OnDroppingDown(e);
			if (e.Cancel)
			{
				return;
			}
			if (_formatStyleRenderer != null)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Location = PointToScreen(clientRectangle.Location);
				clientRectangle.Height--;
				_formatStyleRenderer.Font = base.DropListFont;
				_formatStyleRenderer.RightToLeft = RightToLeft;
				_formatStyleRenderer.SetImageList(base.ImageList);
				_formatStyleRenderer.SetFlatBorderColor(base.FlatBorderColor);
				_formatStyleRenderer.SetHighlightFormatStyle(base.SelectedItemFormatStyle);
				_formatStyleRenderer.SetNormalFormatStyle(base.ItemsFormatStyle);
				_formatStyleRenderer.SetHighlightedItem(SelectedItem);
				_formatStyleRenderer.ForeColor = base.ControlAppearance.WindowTextColor;
				_formatStyleRenderer.SetBackColor(base.ControlAppearance.WindowColor);
				_formatStyleRenderer.SetMaxVisibleItems(base.MaxDropDownItems);
				_formatStyleRenderer.SetMaxDropDownWidth(base.MaxDropDownWidth);
				_formatStyleRenderer.ShowDropDown(clientRectangle, this);
				base.DropListControl(setCapture);
				if (!base.Capture)
				{
					RenderingHelper.SetMouseCapture(_formatStyleRenderer, true);
				}
				else
				{
					RenderingHelper.SetMouseCapture(_formatStyleRenderer, setCapture);
				}
			}
			else
			{
				base.DropListControl(setCapture);
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (!e.Handled && !base.ReadOnly)
			{
				InternalProcessChar(e.KeyChar);
			}
		}

		private void SyncSelectionWithText()
		{
			ComboBoxItem itemByText = Items[Text];
			if (itemByText != null && itemByText.Value == SelectedValue && (itemByText.Value == null || itemByText.Value.Equals(SelectedValue)))
			{
				return;
			}
			ComboBoxItem itemByValue = null;
			if (_selectedValue != null)
			{
				itemByValue = Items[SelectedValue];
			}
			_selectedItem = itemByValue;
			if (itemByText == null)
			{
				_shouldClearText = false;
				if (base.ComboStyle == ComboStyle.Combo)
				{
					if (Text.Length == 0)
					{
						SelectedValue = null;
					}
					else
					{
						SelectedValue = Text;
					}
				}
				else
				{
					SelectedIndex = -1;
				}
				_shouldClearText = true;
			}
			else if ((itemByValue == null || string.Compare(itemByValue.Text, itemByText.Text, ignoreCase: true) != 0) && itemByText.Value != _selectedValue)
			{
				if (itemByText != SelectedItem)
				{
					SelectedItem = itemByText;
				}
				else
				{
					SelectedValue = itemByText.Value;
				}
				Invalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal override void OnCancelChanges(EventArgs e)
		{
			_raiseSelectionEvents = false;
			if (SelectedValue != null)
			{
				ComboBoxItem selectedItem = Items[SelectedValue];
				SelectedItem = selectedItem;
			}
			else if (SelectedItem != null && SelectedItem.Value != null)
			{
				SelectedItem = null;
				base.Text = "";
			}
			_raiseSelectionEvents = true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal override void OnAcceptChanges(EventArgs e)
		{
			if (base.DroppedDown)
			{
				if (_formatStyleRenderer != null)
				{
					_formatStyleRenderer.HideDropDown(false);
				}
				else
				{
					base.DroppedDown = false;
				}
				return;
			}
			SyncSelectionWithText();
		}

		protected internal override void OnDropHide(bool cancel)
		{
			_formatStyleRenderer.HideDropDown(cancel);
			if (!cancel)
			{
				if (_textBox != null && _shouldFocusTextBox)
				{
					SelectRange(0, Text.Length);
					RenderingHelper.TryFocusControl(_textBox);
				}
				if (_formatStyleRenderer.GetHighlightedItem() != null)
				{
					_formatStyleRenderer.GetHighlightedItem().SetOwnerCollection(Items);
				}
				SelectedItem = _formatStyleRenderer.GetHighlightedItem();
				if (_textBox != null)
				{
					SelectRange(0, Text.Length);
				}
			}
			else if (_selectedItem != null)
			{
				base.Text = _selectedItem.Text;
				if (_selectedItem.Image != null)
				{
					SetImage(_selectedItem.Image);
				}
				else
				{
					SetImageIndex(_selectedItem.ImageIndex);
					SetImageKey(_selectedItem.ImageKey);
				}
			}
			base.OnDropHide(cancel);
		}

		protected override void OnMouseUp(MouseEventArgs me)
		{
			base.OnMouseUp(me);
			if (base.DroppedDown)
			{
				try
				{
					RenderingHelper.SetMouseCapture(_formatStyleRenderer, true);
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// The drop list can already be closed or disposed; releasing its capture is best effort.
				}
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			if (_isUpdatingText)
			{
				return;
			}
			ComboBoxItem uiComboBoxItem = ((Text.Length != 0) ? Items[Text] : null);
			if (uiComboBoxItem == null)
			{
				if (!base.DroppedDown)
				{
					_selectedItem = null;
				}
				Invalidate();
			}
			else if (uiComboBoxItem != _selectedItem)
			{
				if (!base.DroppedDown)
				{
					_selectedItem = uiComboBoxItem;
				}
				Invalidate();
			}
			OnChange(EventArgs.Empty);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Prior:
			case Keys.Next:
			case Keys.Up:
			case Keys.Down:
				return true;
			case Keys.Left:
			case Keys.Right:
				if (base.ComboStyle == ComboStyle.DropDownList)
				{
					return true;
				}
				return base.IsInputKey(keyData);
			default:
				return base.IsInputKey(keyData);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled || base.ReadOnly)
			{
				return;
			}
			switch (e.KeyData)
			{
			case Keys.Home:
				if (base.ComboStyle == ComboStyle.DropDownList || base.DroppedDown)
				{
					SelectPreviousItem(Items.Count);
					e.Handled = true;
				}
				break;
			case Keys.End:
				if (base.ComboStyle == ComboStyle.DropDownList)
				{
					SelectNextItem(Items.Count);
					e.Handled = true;
				}
				break;
			case Keys.Up:
				SelectPreviousItem(1);
				e.Handled = true;
				break;
			case Keys.Down:
				SelectNextItem(1);
				e.Handled = true;
				break;
			case Keys.Left:
				if (GetEditStyle() == EditStyle.DropDownList)
				{
					SelectPreviousItem(1);
					e.Handled = true;
				}
				break;
			case Keys.Right:
				if (GetEditStyle() == EditStyle.DropDownList)
				{
					SelectNextItem(1);
					e.Handled = true;
				}
				break;
			case Keys.Next:
				SelectNextItem(_selectedItem != null ? PageStepWithSelection : PageStepWithoutSelection);
				e.Handled = true;
				break;
			case Keys.Prior:
				SelectPreviousItem(_selectedItem != null ? PageStepWithSelection : PageStepWithoutSelection);
				e.Handled = true;
				break;
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (base.DroppedDown && _formatStyleRenderer != null && _formatStyleRenderer.ProcessDropDownKey(keyData))
			{
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		protected internal override void OnKeyPressed(EventArgs e)
		{
			base.OnKeyPressed(e);
			AutoCompleteText();
		}

		private void AutoCompleteText()
		{
			if (Items.Count <= 0 || !_autoComplete || Text.Length <= 0)
			{
				return;
			}
			string text = Text;
			ComboBoxItem uiComboBoxItem = Items.FindItemByTextPrefix(text);
			if (uiComboBoxItem != null)
			{
				base.Text = uiComboBoxItem.Text;
				SelectRange(text.Length, uiComboBoxItem.Text.Length);
				if (base.DroppedDown)
				{
					_formatStyleRenderer.SetHighlightedItem(uiComboBoxItem);
					_formatStyleRenderer.ScrollSelectionIntoView();
				}
			}
			else if (base.DroppedDown)
			{
				_formatStyleRenderer.SetHighlightedItem(null);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void InternalProcessChar(char keyChar)
		{
			if (char.IsLetterOrDigit(keyChar) && GetEditStyle() == EditStyle.DropDownList)
			{
				string text = keyChar.ToString();
				int currentIndex = -1;
				if (_selectedItem != null)
				{
					currentIndex = Items.IndexOf(_selectedItem);
				}
				ComboBoxItem uiComboBoxItem = Items.FindNextItemByTextPrefix(currentIndex, text);
				if (uiComboBoxItem != null)
				{
					SelectedItem = uiComboBoxItem;
				}
			}
		}

		private void SelectNextItem(int step)
		{
			int targetIndex = ((!base.DroppedDown || _formatStyleRenderer.GetHighlightedItem() == null) ? (SelectedIndex + step) : (Items.IndexOf(_formatStyleRenderer.GetHighlightedItem()) + step));
			if (targetIndex >= Items.Count)
			{
				targetIndex = _baseItems.Count - 1;
			}
			ComboBoxItem uiComboBoxItem = FindItemBySelectedIndex(targetIndex);
			while (uiComboBoxItem != null && uiComboBoxItem.IsSeparator)
			{
				targetIndex++;
				uiComboBoxItem = FindItemBySelectedIndex(targetIndex);
			}
			SelectedItem = uiComboBoxItem;
			try
			{
				AccessibilityNotifyClients(AccessibleEvents.Focus, 0);
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// Accessibility notification is best effort and must not stop the selection change.
			}
		}

		private void SelectPreviousItem(int step)
		{
			int targetIndex = ((!base.DroppedDown || _formatStyleRenderer.GetHighlightedItem() == null) ? (SelectedIndex - step) : (Items.IndexOf(_formatStyleRenderer.GetHighlightedItem()) - step));
			if (targetIndex < 0)
			{
				targetIndex = 0;
			}
			ComboBoxItem uiComboBoxItem = FindItemBySelectedIndex(targetIndex);
			while (uiComboBoxItem != null && uiComboBoxItem.IsSeparator)
			{
				targetIndex--;
				uiComboBoxItem = FindItemBySelectedIndex(targetIndex);
			}
			SelectedItem = uiComboBoxItem;
			try
			{
				AccessibilityNotifyClients(AccessibleEvents.Focus, 0);
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// Accessibility notification is best effort and must not stop the selection change.
			}
		}

		protected internal override void OnDoMouseWheel(int delta)
		{
			if (base.DroppedDown && _formatStyleRenderer != null)
			{
				((ScrollableHostControl)_formatStyleRenderer).ScrollByWheelDelta(delta);
			}
			else if (!base.ReadOnly)
			{
				if (delta < 0)
				{
					SelectNextItem(1);
				}
				else
				{
					SelectPreviousItem(1);
				}
			}
		}
	}
}
