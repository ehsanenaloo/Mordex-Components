using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI;

namespace Mordex.Windows.UI.Controls
{
	[ToolboxItem(true)]
	public class MxFontPicker : MxComboBase
	{
		private const int DefaultMaxRecentItems = 10;

		private const float DropListFontSize = 10f;

		private const float InitialDropListFontSize = 12f;

		private readonly Container _container = new Container();

		private ComboBoxItemCollection _items;

		private bool _isDropListFontAutomatic = true;

		private Font _pickerDropListFont;

		private Font _initialRendererFont;

		private bool _isUpdatingText;

		private object _selectedValue;

		private bool _isCommittingSelection;

		private ComboBoxItem _selectedItem;

		private bool _useFontInFontNames = true;

		private EventHandler _selectedValueChangedHandler;

		private FontFamily[] _recentItems;

		private int _maxRecentItems = DefaultMaxRecentItems;

		private FontFamily[] _customFamilies;

		private int _separatorIndex = -1;

		[Localizable(true)]
		[Description("Represents the Font used by the drop down window")]
		public new Font DropListFont
		{
			get
			{
				if (_pickerDropListFont == null)
				{
					_pickerDropListFont = new Font(Font.FontFamily, DropListFontSize);
				}
				return _pickerDropListFont;
			}
			set
			{
				if (value == null)
				{
					ResetDropListFont();
					return;
				}
				if (value == _pickerDropListFont)
				{
					return;
				}
				ReleaseAutomaticDropListFont();
				_pickerDropListFont = value;
				_isDropListFontAutomatic = false;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
				ComboBoxItem uiComboBoxItem = GetItems()[value];
				if (value == null || value.Length <= 0)
				{
					uiComboBoxItem = null;
				}
				if (uiComboBoxItem != null)
				{
					if (uiComboBoxItem != GetSelectedItem())
					{
						SetSelectedItem(uiComboBoxItem);
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
				if (_selectedItem != null)
				{
					_selectedItem = null;
					SetImage(null);
				}
			}
		}

		[Bindable(BindableSupport.Yes)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object SelectedValue
		{
			get
			{
				return _selectedValue;
			}
			set
			{
				if ((value == null || !value.Equals(_selectedValue)) && value != _selectedValue)
				{
					_selectedValue = value;
					if (_selectedValue == null)
					{
						ComboBoxItem uiComboBoxItem = _selectedItem;
						base.Text = "";
						_selectedItem = uiComboBoxItem;
						SetSelectedItem(null);
					}
					else
					{
						SetSelectedItem(GetItems()[_selectedValue]);
					}
					OnSelectedValueChanged(EventArgs.Empty);
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(true)]
		[Description("Specifying whether the font names are displayed using their corresponding Font.")]
		public bool UseFontInFontNames
		{
			get
			{
				return _useFontInFontNames;
			}
			set
			{
				if (_useFontInFontNames != value)
				{
					_useFontInFontNames = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(-1)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public int SelectedIndex
		{
			get
			{
				if (_selectedItem == null)
				{
					return -1;
				}
				return GetItems().IndexOf(_selectedItem);
			}
			set
			{
				if (SelectedIndex != value)
				{
					if (value >= GetItems().Count)
					{
						throw new IndexOutOfRangeException("Specified argument was out of the range of valid values");
					}
					ComboBoxItem uiComboBoxItem = FindItemBySelectedIndex(value);
					SetSelectedItem(uiComboBoxItem);
				}
			}
		}

		[Browsable(false)]
		public bool NotInList
		{
			get
			{
				if (GetSelectedItem() == null)
				{
					return true;
				}
				return false;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public FontFamily[] RecentItems
		{
			get
			{
				if (_recentItems == null)
				{
					_recentItems = new FontFamily[0];
				}
				return _recentItems;
			}
			set
			{
				if (_recentItems != value)
				{
					_recentItems = value;
					RebuildFontItems();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(DefaultMaxRecentItems)]
		[Description("Represents the maximum number of recent items that will be displayed on top of the list.")]
		public int MaxRecentItems
		{
			get
			{
				return _maxRecentItems;
			}
			set
			{
				if (_maxRecentItems != value)
				{
					_maxRecentItems = value;
					RebuildFontItems();
				}
			}
		}

		[Description("Event fired when the SelectedValue property has changed.")]
		public event EventHandler SelectedValueChanged
		{
			add
			{
				EventHandler eventHandler = _selectedValueChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedValueChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _selectedValueChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler updatedHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedValueChangedHandler, updatedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		public MxFontPicker()
		{
			_initialRendererFont = new Font(Font.FontFamily, InitialDropListFontSize, Font.Style);
			_formatStyleRenderer.Font = _initialRendererFont;
			RebuildFontItems();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
			if (disposing)
			{
				ReleaseAutomaticDropListFont();
				_initialRendererFont?.Dispose();
				_initialRendererFont = null;
			}
		}

		// Only the automatic font is created here; a font the user assigned is never disposed.
		private void ReleaseAutomaticDropListFont()
		{
			if (_isDropListFontAutomatic && _pickerDropListFont != null)
			{
				Font font = _pickerDropListFont;
				_pickerDropListFont = null;
				if (_formatStyleRenderer != null && _formatStyleRenderer.Font == font)
				{
					_formatStyleRenderer.Font = null;
				}
				font.Dispose();
			}
		}

		internal ComboBoxItemCollection GetItems()
		{
			if (_items == null)
			{
				_items = new ComboBoxItemCollection(this);
				_formatStyleRenderer.SetDropDownItems(_items);
			}
			return _items;
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			if (_isDropListFontAutomatic)
			{
				ResetDropListFont();
			}
		}

		public new bool ShouldSerializeDropListFont()
		{
			return !_isDropListFontAutomatic;
		}

		public new void ResetDropListFont()
		{
			ReleaseAutomaticDropListFont();
			_pickerDropListFont = null;
			_isDropListFontAutomatic = true;
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
				_formatStyleRenderer.RightToLeft = RightToLeft;
				_formatStyleRenderer.SetImageList(base.ImageList);
				_formatStyleRenderer.SetFlatBorderColor(base.FlatBorderColor);
				_formatStyleRenderer.SetHighlightFormatStyle(base.SelectedItemFormatStyle);
				_formatStyleRenderer.SetNormalFormatStyle(base.ItemsFormatStyle);
				_formatStyleRenderer.SetHighlightedItem(GetSelectedItem());
				_formatStyleRenderer.ForeColor = _formatStyleRenderer.ForeColor;
				_formatStyleRenderer.SetBackColor(base.ControlAppearance.WindowColor);
				_formatStyleRenderer._dropDownAnimation = GetDropDownAnimation();
				_formatStyleRenderer.SetMaxVisibleItems(base.MaxDropDownItems);
				_formatStyleRenderer.SetMaxDropDownWidth(base.MaxDropDownWidth);
				_formatStyleRenderer.Font = DropListFont;
				_formatStyleRenderer.SetUseItemFonts(UseFontInFontNames);
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
			if (!e.Handled)
			{
				InternalProcessChar(e.KeyChar);
			}
		}

		protected internal override void OnDropHide(bool cancel)
		{
			_formatStyleRenderer.HideDropDown(cancel);
			if (!cancel)
			{
				_isCommittingSelection = true;
				SetSelectedItem(_formatStyleRenderer.GetHighlightedItem());
				_isCommittingSelection = false;
				if (_textBox != null && _shouldFocusTextBox)
				{
					SelectRange(0, Text.Length);
					RenderingHelper.TryFocusControl(_textBox);
				}
			}
			else if (_selectedItem == null)
			{
				Text = "";
				SetImage(null);
			}
			else
			{
				Text = _selectedItem.Text;
				SetImage(_selectedItem.GetDisplayImage(base.ImageList));
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

		protected internal override void OnKeyPressed(EventArgs e)
		{
			base.OnKeyPressed(e);
			AutoCompleteText();
		}

		private void AutoCompleteText()
		{
			if (GetItems().Count <= 0 || Text.Length <= 0)
			{
				return;
			}
			string text = Text;
			ComboBoxItem uiComboBoxItem = GetItems().FindItemByTextPrefix(text);
			if (uiComboBoxItem != null)
			{
				Text = uiComboBoxItem.Text;
				SelectRange(text.Length, uiComboBoxItem.Text.Length);
				if (base.DroppedDown)
				{
					_formatStyleRenderer.SetHighlightedItem(uiComboBoxItem);
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
			ComboBoxItem uiComboBoxItem = ((Text.Length != 0) ? GetItems()[Text] : null);
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
				SelectNextItem(_selectedItem == null ? PageStepWithoutSelection : PageStepWithSelection);
				e.Handled = true;
				break;
			case Keys.Prior:
				SelectPreviousItem(_selectedItem == null ? PageStepWithoutSelection : PageStepWithSelection);
				e.Handled = true;
				break;
			case Keys.End:
			case Keys.Home:
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

		internal ComboBoxItem GetSelectedItem()
		{
			return _selectedItem;
		}

		internal void SetSelectedItem(ComboBoxItem item)
		{
			if (_selectedItem == item && !_isCommittingSelection)
			{
				return;
			}
			_selectedItem = item;
			if (_selectedItem != null)
			{
				AddRecentFamily(_selectedItem._fontFamily);
				ComboBoxItem uiComboBoxItem = FindItemByFamily(_selectedItem._fontFamily);
				if (uiComboBoxItem != null)
				{
					_selectedItem = uiComboBoxItem;
				}
			}
			if (_selectedItem == null)
			{
				base.Text = "";
				SelectedValue = null;
				SetImage(null);
			}
			else
			{
				_isUpdatingText = true;
				SelectedValue = _selectedItem.Value;
				base.Text = _selectedItem.Text;
				SetImage(_selectedItem.GetDisplayImage(base.ImageList));
				_isUpdatingText = false;
			}
			if (_formatStyleRenderer != null)
			{
				_formatStyleRenderer.SetHighlightedItem(_selectedItem);
			}
			Invalidate();
		}

		protected virtual void OnSelectedValueChanged(EventArgs e)
		{
			if (_selectedValueChangedHandler != null)
			{
				_selectedValueChangedHandler(this, e);
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
					currentIndex = GetItems().IndexOf(_selectedItem);
				}
				ComboBoxItem uiComboBoxItem = GetItems().FindNextItemByTextPrefix(currentIndex, text);
				if (uiComboBoxItem != null)
				{
					SetSelectedItem(uiComboBoxItem);
				}
			}
		}

		private ComboBoxItem FindItemByFamily(FontFamily fontFamily)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			int firstSearchIndex = 0;
			if (_recentItems != null && _recentItems.Length > 0)
			{
				firstSearchIndex = _recentItems.Length - 1;
			}
			for (int i = firstSearchIndex; i < _items.Count; i++)
			{
				ComboBoxItem uiComboBoxItem = _items[i];
				if (!uiComboBoxItem._isSeparator && string.Compare(uiComboBoxItem._fontFamily.Name, fontFamily.Name, ignoreCase: true, invariantCulture) == 0)
				{
					return uiComboBoxItem;
				}
			}
			return null;
		}

		private ComboBoxItem FindItemBySelectedIndex(int selectedIndex)
		{
			foreach (ComboBoxItem item in GetItems())
			{
				if (item._listIndex == selectedIndex)
				{
					return item;
				}
			}
			return null;
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
			if (_textBox != null)
			{
				_textBox.SelectionStart = Text.Length;
			}
			CommitText();
		}

		protected internal override void OnCancelChanges(EventArgs e)
		{
			if (SelectedValue != null)
			{
				ComboBoxItem uiComboBoxItem = GetItems()[SelectedValue];
				SetSelectedItem(uiComboBoxItem);
			}
			else
			{
				SetSelectedItem(null);
			}
		}

		private void CommitText()
		{
			ComboBoxItem uiComboBoxItem = ((Text.Length != 0) ? GetItems()[Text] : null);
			if (uiComboBoxItem == null)
			{
				if (SelectedValue != null)
				{
					ComboBoxItem selectedItem = GetItems()[SelectedValue];
					SetSelectedItem(selectedItem);
				}
				else
				{
					SetSelectedItem(null);
				}
			}
			else
			{
				SelectedValue = uiComboBoxItem.Value;
				Invalidate();
			}
		}

		private void SelectNextItem(int step)
		{
			int targetIndex = ((!base.DroppedDown || _formatStyleRenderer.GetHighlightedItem() == null) ? (SelectedIndex + step) : (GetItems().IndexOf(_formatStyleRenderer.GetHighlightedItem()) + step));
			if (targetIndex == _separatorIndex)
			{
				targetIndex++;
			}
			if (targetIndex >= GetItems().Count)
			{
				targetIndex = 0;
			}
			ComboBoxItem uiComboBoxItem = FindItemBySelectedIndex(targetIndex);
			if (uiComboBoxItem == null)
			{
				Text = "";
				SetImage(null);
				if (_formatStyleRenderer != null)
				{
					_formatStyleRenderer.SetHighlightedItem(null);
				}
			}
			else
			{
				Text = uiComboBoxItem.Text;
				SetImage(uiComboBoxItem.GetDisplayImage(base.ImageList));
				if (_formatStyleRenderer != null)
				{
					_formatStyleRenderer.SetHighlightedItem(uiComboBoxItem);
				}
			}
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
			int targetIndex = ((!base.DroppedDown || _formatStyleRenderer.GetHighlightedItem() == null) ? (SelectedIndex - step) : (GetItems().IndexOf(_formatStyleRenderer.GetHighlightedItem()) - step));
			if (targetIndex == _separatorIndex)
			{
				targetIndex--;
			}
			if (targetIndex < 0)
			{
				targetIndex = GetItems().Count - 1;
			}
			ComboBoxItem uiComboBoxItem = FindItemBySelectedIndex(targetIndex);
			if (uiComboBoxItem == null)
			{
				Text = "";
				SetImage(null);
				if (_formatStyleRenderer != null)
				{
					_formatStyleRenderer.SetHighlightedItem(null);
				}
			}
			else
			{
				Text = uiComboBoxItem.Text;
				SetImage(uiComboBoxItem.GetDisplayImage(base.ImageList));
				if (_formatStyleRenderer != null)
				{
					_formatStyleRenderer.SetHighlightedItem(uiComboBoxItem);
				}
			}
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

		private void AddRecentFamily(FontFamily family)
		{
			int newCount = RecentItems.Length + 1;
			FontFamily[] recentItems = RecentItems;
			foreach (FontFamily fontFamily in recentItems)
			{
				if (fontFamily != null && fontFamily.Equals(family))
				{
					newCount--;
				}
			}
			if (newCount > _maxRecentItems)
			{
				newCount = _maxRecentItems;
			}
			FontFamily[] updatedItems = new FontFamily[newCount];
			if (newCount > 0)
			{
				updatedItems.SetValue(family, 0);
				int writeIndex = 1;
				for (int j = 0; j < RecentItems.Length; j++)
				{
					FontFamily existingFamily = (FontFamily)RecentItems.GetValue(j);
					if (existingFamily == null || !existingFamily.Equals(family))
					{
						updatedItems.SetValue(existingFamily, writeIndex);
						writeIndex++;
						if (writeIndex >= _maxRecentItems)
						{
							break;
						}
					}
				}
			}
			RecentItems = updatedItems;
		}

		public void SetFamilies(FontFamily[] families)
		{
			FontFamily[] copiedFamilies = new FontFamily[families.Length];
			for (int i = 0; i < families.Length; i++)
			{
				FontFamily fontFamily = families[i];
				copiedFamilies[i] = new FontFamily(fontFamily.Name);
			}
			_customFamilies = copiedFamilies;
			_recentItems = null;
			RebuildFontItems();
		}

		protected internal override void OnVisualStyleChanged()
		{
			base.OnVisualStyleChanged();
			RebuildFontItems();
		}

		private static ComboBoxItem CreateFontItem(FontFamily fontFamily, Image image, int localeId)
		{
			ComboBoxItem item = new ComboBoxItem(fontFamily, 0);
			item.Image = image;
			item.Text = fontFamily.GetName(localeId);
			item._fontFamily = fontFamily;
			return item;
		}

		private void RebuildFontItems()
		{
			int localeId = CultureInfo.CurrentCulture.LCID;
			GetItems().Clear();
			_separatorIndex = -1;
			Image image = (RenderingHelper.MakeMagentaTransparent(WindowsResources.TTFont()));
			if (GetImage() != null && GetSelectedItem() != null)
			{
				SetImage(image);
			}
			if (RecentItems.Length > 0)
			{
				foreach (FontFamily recentFamily in RecentItems)
				{
					GetItems().AddItemUnchecked(CreateFontItem(recentFamily, image, localeId));
				}
				ComboBoxItem separator = new ComboBoxItem(null);
				separator._isSeparator = true;
				GetItems().AddItemUnchecked(separator);
				_separatorIndex = separator._listIndex;
			}
			FontFamily[] families = _customFamilies ?? FontFamily.Families;
			foreach (FontFamily family in families)
			{
				GetItems().AddItemUnchecked(CreateFontItem(family, image, localeId));
			}
			if (GetSelectedItem() == null)
			{
				SetSelectedItem(FindItemByFamily(Font.FontFamily));
			}
		}
	}
}
