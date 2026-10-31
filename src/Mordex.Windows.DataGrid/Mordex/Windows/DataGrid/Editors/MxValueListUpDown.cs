using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(true)]
	[DefaultBindingProperty("Value")]
	public class MxValueListUpDown : UpDownBase
	{
		private object _valueListUpDownValue;

		private GridValueListItemCollection _gridValueListItemCollection;

		private bool _valueListUpDownAutoComplete = true;

		private bool _isUpdatingText;

		private int _selectedIndex = -1;

		private GridMaskEdit _valueListUpDownDefaultEditControl;

		private static object _valueChangedKey = new object();

		[Browsable(false)]
		[DefaultValue(null)]
		[Bindable(BindableSupport.Yes)]
		public object Value
		{
			get
			{
				return _valueListUpDownValue;
			}
			set
			{
				if (_valueListUpDownValue != value)
				{
					_valueListUpDownValue = value;
					RefreshDisplayFromValue();
					SetSuppressTextChange(true);
					OnValueChanged(EventArgs.Empty);
				}
			}
		}

		public bool NotInList
		{
			get
			{
				if (_gridValueListItemCollection != null && Text.Length > 0)
				{
					GridValueListItem gridValueListItem = _gridValueListItemCollection[Text];
					if (gridValueListItem != null)
					{
						return false;
					}
					return true;
				}
				return false;
			}
		}

		[Description("Represents the GridValueListItemCollection associated with the control.")]
		[Editor("Mordex.Windows.DataGrid.Design.ValueListEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridValueListItemCollection ValueList => _gridValueListItemCollection;

		[Description("Indicates whether the control should auto complete the typed text when there is a partial match in the list.")]
		[DefaultValue(true)]
		public bool AutoComplete
		{
			get
			{
				return _valueListUpDownAutoComplete;
			}
			set
			{
				_valueListUpDownAutoComplete = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public int SelectedIndex
		{
			get
			{
				return _selectedIndex;
			}
			set
			{
				if (_selectedIndex != value)
				{
					_selectedIndex = value;
					if (_selectedIndex == -1)
					{
						Value = null;
					}
					else
					{
						Value = _gridValueListItemCollection[_selectedIndex].Value;
					}
				}
			}
		}

		protected override TextBox DefaultEditControl
		{
			get
			{
				UpdateValueListUpDownDefaultEditControl(new GridMaskEdit());
				return _valueListUpDownDefaultEditControl;
			}
		}

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

		public MxValueListUpDown()
		{
			_gridValueListItemCollection = new GridValueListItemCollection();
		}

		internal void SetValueListItems(GridValueListItemCollection gridValueListItemCollection)
		{
			if (gridValueListItemCollection != null)
			{
				_gridValueListItemCollection = gridValueListItemCollection;
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			if (_isUpdatingText)
			{
				return;
			}
			if (Text.Length == 0)
			{
				_valueListUpDownValue = null;
			}
			else
			{
				GridValueListItem gridValueListItem = _gridValueListItemCollection[Text];
				if (gridValueListItem != null)
				{
					_valueListUpDownValue = gridValueListItem.Value;
				}
				else
				{
					_valueListUpDownValue = Text;
				}
			}
			OnValueChanged(e);
		}

		private void UpdateValueListUpDown()
		{
			UpdateValueListUpDownValue();
		}

		private void UpdateValueListUpDownValue()
		{
			if (_valueListUpDownAutoComplete && Text.Length > 0)
			{
				string text = Text;
				GridValueListItem gridValueListItem = ValueList.FindItemStartingWith(Text);
				if (gridValueListItem != null)
				{
					Value = gridValueListItem.Value;
					_valueListUpDownDefaultEditControl.SelectRange(Text.Length, text.Length);
				}
				else
				{
					_valueListUpDownValue = Text;
				}
			}
		}

		private void RefreshDisplayFromValue()
		{
			_isUpdatingText = true;
			if (_gridValueListItemCollection.ReplaceValues)
			{
				GridValueListItem gridValueListItem = _gridValueListItemCollection[_valueListUpDownValue];
				if (gridValueListItem != null)
				{
					SelectedIndex = _gridValueListItemCollection.IndexOf(gridValueListItem);
					Text = gridValueListItem.Text;
					if (_valueListUpDownDefaultEditControl != null)
					{
						_valueListUpDownDefaultEditControl.SelectAll();
					}
					if (HasImage)
					{
						try
						{
							if (gridValueListItem.Image != null)
							{
								base.Image = gridValueListItem.Image;
							}
							else if (gridValueListItem.ImageIndex > -1 && base.ImageList != null)
							{
								base.Image = base.ImageList.Images[gridValueListItem.ImageIndex];
							}
							else
							{
								base.Image = null;
							}
						}
						catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
						{
							GridExceptions.Report(caught);
						}
					}
					_isUpdatingText = false;
					return;
				}
			}
			if (GridHelper.IsNullOrDBNull(_valueListUpDownValue))
			{
				Text = "";
			}
			else
			{
				Text = GridHelper.ToSafeString(_valueListUpDownValue);
			}
			_isUpdatingText = false;
		}

		protected override void UpButton()
		{
			if (_selectedIndex > 0)
			{
				SelectedIndex--;
			}
			else if (_selectedIndex == -1)
			{
				SelectedIndex = _gridValueListItemCollection.Count - 1;
			}
		}

		protected override void DownButton()
		{
			if (_selectedIndex < _gridValueListItemCollection.Count - 1)
			{
				SelectedIndex++;
			}
		}

		internal void SetInitialCharacter(char character)
		{
			if (_valueListUpDownDefaultEditControl != null)
			{
				if (_valueListUpDownDefaultEditControl.Mask.Length == 0)
				{
					Text = character.ToString(Application.CurrentCulture);
					_valueListUpDownDefaultEditControl.SelectionStart = Text.Length;
					_valueListUpDownDefaultEditControl.ScrollToCaret();
					UpdateValueListUpDown();
				}
				else
				{
					_valueListUpDownDefaultEditControl.Text = "";
					_valueListUpDownDefaultEditControl.HandleMaskedChar(character);
					UpdateValueListUpDown();
				}
			}
		}

		protected override void OnUpDownStyleChanged()
		{
			if (base.UpDownStyle == UpDownStyle.UpDownCombo)
			{
				if (GetValueListUpDownDefaultEditControl() == null)
				{
					UpdateValueListUpDownDefaultEditControl(new GridMaskEdit());
				}
			}
			else if (GetValueListUpDownDefaultEditControl() != null)
			{
				UpdateValueListUpDownDefaultEditControl(null);
			}
			base.EditControl = GetValueListUpDownDefaultEditControl();
		}

		private GridMaskEdit GetValueListUpDownDefaultEditControl()
		{
			return _valueListUpDownDefaultEditControl;
		}

		private void UpdateValueListUpDownDefaultEditControl(GridMaskEdit valueListUpDownDefaultEditControl)
		{
			if (_valueListUpDownDefaultEditControl != valueListUpDownDefaultEditControl)
			{
				if (_valueListUpDownDefaultEditControl != null)
				{
					_valueListUpDownDefaultEditControl.RemoveKeyPressedHandler(OnEditControlChanged);
				}
				_valueListUpDownDefaultEditControl = valueListUpDownDefaultEditControl;
				if (_valueListUpDownDefaultEditControl != null)
				{
					_valueListUpDownDefaultEditControl.AddKeyPressedHandler(OnEditControlChanged);
					_valueListUpDownDefaultEditControl.MaxLength = base.MaxLength;
					_valueListUpDownDefaultEditControl.CharacterCasing = CharacterCasing;
				}
			}
		}

		private void OnEditControlChanged(object sender, EventArgs eventArgs)
		{
			UpdateValueListUpDown();
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			if (base.Events[_valueChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
