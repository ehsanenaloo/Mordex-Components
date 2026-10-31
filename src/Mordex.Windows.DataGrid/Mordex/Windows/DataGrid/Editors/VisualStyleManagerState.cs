using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class VisualStyleManagerState : ControlStyleContainerControl
	{
		private DropDownListForm _dropDownDialog;

		private string _okCaption;

		private string _cancelCaption;

		private bool _isApplyingDropDownResult;

		private ILookUpTable _iLookUpTable;

		private object _visualStyleManagerStateValue;

		private bool _isInitializingValue;

		public VisualStyleManagerState()
		{
			_dropDownDialog = new DropDownListForm(null, GetImageList());
		}

		public string GetVisualStyleManagerStateText()
		{
			return _okCaption;
		}

		public void UpdateVisualStyleManagerStateText(string visualStyleManagerStateText)
		{
			if (visualStyleManagerStateText != _okCaption)
			{
				_okCaption = visualStyleManagerStateText;
				if (_dropDownDialog != null)
				{
					_dropDownDialog._okCaption = _okCaption;
				}
			}
		}

		public void SetCancelButtonText(string cancelButtonText)
		{
			_cancelCaption = cancelButtonText;
			if (_dropDownDialog != null)
			{
				_dropDownDialog._cancelCaption = _cancelCaption;
			}
		}

		protected override void DropListControl(bool setCapture)
		{
			_dropDownDialog.SetFallbackControlStyle(GetControlStyle());
			_dropDownDialog.SetDropDownOwner(this);
			base.DropListControl(setCapture: false);
			GridHelper.SetCapture(this, false);
			Rectangle rectangle = GetVirtualClientRectangle();
			rectangle.Location = PointToScreen(rectangle.Location);
			_dropDownDialog.CheckSelectedValues(_visualStyleManagerStateValue);
			_dropDownDialog._form = FindForm();
			_dropDownDialog.ApplyAnchorBounds(rectangle);
		}

		private bool AreValuesDifferent(object value, object otherItem)
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

		protected override void OnDropHide(bool cancel)
		{
			if (!cancel)
			{
				_isApplyingDropDownResult = true;
				object valueArray = _dropDownDialog.ComputeValueArray();
				if (AreValuesDifferent(_visualStyleManagerStateValue, valueArray))
				{
					SyncValueAndDisplayText(valueArray);
					OnChange(EventArgs.Empty);
				}
				_isApplyingDropDownResult = false;
			}
			base.OnDropHide(cancel);
		}

		internal void UpdateVisualStyleManagerStateValue()
		{
			string[] array = null;
			ArrayList arrayList = new ArrayList();
			if (_iLookUpTable != null)
			{
				if (Text.Length > 0)
				{
					array = Text.Split(new char[1] { ',' }, StringSplitOptions.None);
					string[] otherArray = array;
					foreach (string text in otherArray)
					{
						string nextText = text.Trim();
						if (nextText.Length > 0)
						{
							IMordexValueItem itemFromText = _iLookUpTable.GetItemFromText(nextText);
							if (itemFromText != null)
							{
								arrayList.Add(itemFromText.Value);
							}
							else
							{
								arrayList.Add(nextText);
							}
						}
					}
				}
			}
			else if (Text.Length > 0)
			{
				array = Text.Split(new char[1] { ',' }, StringSplitOptions.None);
				string[] nextArray = array;
				foreach (string otherText in nextArray)
				{
					string otherTrim = otherText.Trim();
					if (otherTrim.Length > 0)
					{
						arrayList.Add(otherTrim);
					}
				}
			}
			object[] value = new object[arrayList.Count];
			for (int k = 0; k < arrayList.Count; k++)
			{
				value[k] = arrayList[k];
			}
			if (AreValuesDifferent(value, _visualStyleManagerStateValue))
			{
				_visualStyleManagerStateValue = value;
				OnChange(EventArgs.Empty);
			}
		}

		protected override void OnTextChanged(EventArgs eventArgs)
		{
			base.OnTextChanged(eventArgs);
			if (!_isInitializingValue && !_isApplyingDropDownResult)
			{
				OnChange(EventArgs.Empty);
				UpdateVisualStyleManagerStateValue();
			}
		}

		public void ProcessILookUpTable(ILookUpTable iLookUpTable)
		{
			if (iLookUpTable != _iLookUpTable)
			{
				_iLookUpTable = iLookUpTable;
				_dropDownDialog = new DropDownListForm(_iLookUpTable, GetImageList());
				_dropDownDialog._okCaption = GetVisualStyleManagerStateText();
				_dropDownDialog._cancelCaption = _cancelCaption;
			}
		}

		public object GetVisualStyleManagerStateValue()
		{
			return _visualStyleManagerStateValue;
		}

		public void SyncValueAndDisplayText(object visualStyleManagerStateValue)
		{
			if (visualStyleManagerStateValue == _visualStyleManagerStateValue)
			{
				return;
			}
			_visualStyleManagerStateValue = visualStyleManagerStateValue;
			if (visualStyleManagerStateValue is IList)
			{
				StringBuilder stringBuilder = new StringBuilder();
				IList list = (IList)visualStyleManagerStateValue;
				for (int i = 0; i < list.Count; i++)
				{
					IMordexValueItem mordexValueItem = null;
					if (_iLookUpTable != null)
					{
						mordexValueItem = _iLookUpTable.GetItemFromValue(list[i]);
					}
					if (mordexValueItem == null)
					{
						stringBuilder.Append(GridHelper.ToSafeString(list[i]));
					}
					else
					{
						stringBuilder.Append(mordexValueItem.Text);
					}
					if (i < list.Count - 1)
					{
						stringBuilder.Append(", ");
					}
				}
				Text = stringBuilder.ToString();
			}
			else
			{
				Text = "";
			}
		}

		protected override void OnPaintText(Graphics graphics)
		{
			_ = base.ClientRectangle.X;
			if (_visualStyleManagerStateValue is IList)
			{
				Brush brush = GdiCache.Brush(ForeColor);
				ListCellRenderer.DrawListItems(graphics, base.ClientRectangle, _visualStyleManagerStateValue, _iLookUpTable, GetImageList(), Font, brush, GetTextLayoutRenderer(), RightToLeft == RightToLeft.Yes);
				brush = null;
			}
		}

		internal void InitializeValue(object value)
		{
			_isInitializingValue = true;
			SyncValueAndDisplayText(value);
			_isInitializingValue = false;
		}

		public bool HasItemsNotInList()
		{
			if (_iLookUpTable != null && _visualStyleManagerStateValue is IList)
			{
				IList list = (IList)_visualStyleManagerStateValue;
				foreach (object item in list)
				{
					IMordexValueItem itemFromValue = _iLookUpTable.GetItemFromValue(item);
					if (itemFromValue == null)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
