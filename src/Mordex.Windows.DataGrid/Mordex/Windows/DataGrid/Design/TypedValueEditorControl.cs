using System;
using Mordex.Windows.DataGrid.Internal;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid.Design
{
	[ToolboxItem(false)]
	internal class TypedValueEditorControl : UserControl
	{
		private object _currentValue;

		private IContainer _iContainer = default;

		private TextBox _txtValue;

		private Label _lblType;

		private Label _lblValue;

		private ComboBox _cboType;

		public TypedValueEditorControl()
		{
			InitializeControls();
			Array values = Enum.GetValues(typeof(TypeCode));
			foreach (object item in values)
			{
				if ((TypeCode)item != TypeCode.Object)
				{
					_cboType.Items.Add(item);
				}
			}
			_cboType.SelectedIndexChanged += OnComboBoxSelectedIndexChanged;
		}

		internal object GetTypedValue()
		{
			return ParseValue(_txtValue.Text);
		}

		internal void SetTypedValue(object value)
		{
			_currentValue = value;
			ShowValue();
		}

		private void ShowValue()
		{
			if (_currentValue == null)
			{
				_cboType.SelectedItem = TypeCode.Empty;
				return;
			}
			if (_currentValue == DBNull.Value)
			{
				_cboType.SelectedItem = TypeCode.DBNull;
				return;
			}
			try
			{
				_cboType.SelectedItem = Type.GetTypeCode(_currentValue.GetType());
				string text = PropertyValue.ConvertObjectToString(_currentValue);
				_txtValue.Text = text;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				_cboType.SelectedItem = TypeCode.String;
				_txtValue.Text = "";
			}
		}

		private void OnComboBoxSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			TypeCode typeCode = (TypeCode)_cboType.SelectedItem;
			if (typeCode == TypeCode.Empty || typeCode == TypeCode.DBNull)
			{
				_txtValue.Text = "";
				_txtValue.Enabled = false;
			}
			else
			{
				_txtValue.Enabled = true;
			}
		}

		private object ParseValue(string strA)
		{
			if (strA == null)
			{
				return null;
			}
			TypeCode typeCode = (TypeCode)_cboType.SelectedItem;
			switch (typeCode)
			{
			case TypeCode.Empty:
				return null;
			case TypeCode.DBNull:
				return DBNull.Value;
			default:
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				if (typeCode != TypeCode.Object)
				{
					try
					{
						CultureInfo provider = invariantCulture;
						switch (typeCode)
						{
						case TypeCode.Boolean:
							if (string.Compare(strA, bool.TrueString, ignoreCase: true) == 0)
							{
								return true;
							}
							return false;
						case TypeCode.Byte:
							return ((IConvertible)strA).ToByte((IFormatProvider)provider);
						case TypeCode.DateTime:
							return ((IConvertible)strA).ToDateTime((IFormatProvider)provider);
						case TypeCode.Decimal:
							return ((IConvertible)strA).ToDecimal((IFormatProvider)provider);
						case TypeCode.Double:
							return ((IConvertible)strA).ToDouble((IFormatProvider)provider);
						case TypeCode.Int16:
							return ((IConvertible)strA).ToInt16((IFormatProvider)provider);
						case TypeCode.Int32:
							return ((IConvertible)strA).ToInt32((IFormatProvider)provider);
						case TypeCode.Int64:
							return ((IConvertible)strA).ToInt64((IFormatProvider)provider);
						case TypeCode.Single:
							return ((IConvertible)strA).ToSingle((IFormatProvider)provider);
						case TypeCode.Char:
						case TypeCode.SByte:
						case TypeCode.UInt16:
						case TypeCode.UInt32:
						case TypeCode.UInt64:
							break;
						}
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
					return Convert.ChangeType(strA, typeCode, invariantCulture);
				}
				return null;
			}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_txtValue = new TextBox();
			_lblType = new Label();
			_lblValue = new Label();
			_cboType = new ComboBox();
			SuspendLayout();
			_txtValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_txtValue.Location = new Point(50, 30);
			_txtValue.Name = "txtValue";
			_txtValue.Size = new Size(191, 20);
			_txtValue.TabIndex = 6;
			_lblType.AutoSize = true;
			_lblType.Location = new Point(2, 5);
			_lblType.Name = "lblType";
			_lblType.Size = new Size(34, 13);
			_lblType.TabIndex = 5;
			_lblType.Text = "Type:";
			_lblValue.AutoSize = true;
			_lblValue.Location = new Point(2, 33);
			_lblValue.Name = "lblValue";
			_lblValue.Size = new Size(37, 13);
			_lblValue.TabIndex = 4;
			_lblValue.Text = "Value:";
			_cboType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboType.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboType.FormattingEnabled = true;
			_cboType.Location = new Point(50, 2);
			_cboType.Name = "cboType";
			_cboType.Size = new Size(191, 21);
			_cboType.TabIndex = 3;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(_txtValue);
			base.Controls.Add(_lblType);
			base.Controls.Add(_lblValue);
			base.Controls.Add(_cboType);
			base.Name = "ObectTypeControl";
			base.Size = new Size(244, 56);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}
	}
}
