using System;
using Mordex.Windows.DataGrid.Internal;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid.Design
{
	public class ObjectTypeDialog : Form
	{
		private Label _lblType;

		private ComboBox _cboType;

		private Label _lblValue;

		private TextBox _txtValue;

		private Button _btnOK;

		private Button _btnCancel;

		private Container _container = new Container();

		private object _selectedValue;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public object Value
		{
			get
			{
				return _selectedValue;
			}
			set
			{
				_selectedValue = value;
			}
		}

		public ObjectTypeDialog()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_cboType = new ComboBox();
			_lblType = new Label();
			_lblValue = new Label();
			_txtValue = new TextBox();
			_btnOK = new Button();
			_btnCancel = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_cboType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboType.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboType.Location = new Point(56, 9);
			_cboType.Name = "cboType";
			_cboType.Size = new Size(172, 21);
			_cboType.TabIndex = 0;
			_cboType.SelectedIndexChanged += OnCboTypeSelectedIndexChanged;
			_lblType.AutoSize = true;
			_lblType.Location = new Point(8, 12);
			_lblType.Name = "lblType";
			_lblType.Size = new Size(33, 14);
			_lblType.TabIndex = 1;
			_lblType.Text = "Type:";
			_lblValue.AutoSize = true;
			_lblValue.Location = new Point(8, 40);
			_lblValue.Name = "lblValue";
			_lblValue.Size = new Size(36, 14);
			_lblValue.TabIndex = 1;
			_lblValue.Text = "Value:";
			_txtValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_txtValue.Location = new Point(56, 37);
			_txtValue.Name = "txtValue";
			_txtValue.Size = new Size(172, 21);
			_txtValue.TabIndex = 2;
			_txtValue.Text = "";
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(76, 72);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(72, 24);
			_btnOK.TabIndex = 3;
			_btnOK.Text = "OK";
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(156, 72);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(72, 24);
			_btnCancel.TabIndex = 4;
			_btnCancel.Text = "Cancel";
			base.AcceptButton = _btnOK;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(236, 106);
			base.Controls.AddRange(new Control[6] { _btnOK, _txtValue, _lblType, _lblValue, _cboType, _btnCancel });
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "ObjectTypeDialog";
			base.StartPosition = FormStartPosition.CenterParent;
			Text = "Object Editor:";
			base.FormClosing += OnControlClosing;
			base.Load += OnControlLoad;
			ResumeLayout(performLayout: false);
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

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
			Array values = Enum.GetValues(typeof(TypeCode));
			foreach (object item in values)
			{
				if ((TypeCode)item != TypeCode.Object)
				{
					_cboType.Items.Add(item);
				}
			}
			if (Value == null)
			{
				_cboType.SelectedItem = TypeCode.Empty;
				return;
			}
			if (Value == DBNull.Value)
			{
				_cboType.SelectedItem = TypeCode.DBNull;
				return;
			}
			try
			{
				_cboType.SelectedItem = Type.GetTypeCode(Value.GetType());
				string text = PropertyValue.ConvertObjectToString(Value);
				_txtValue.Text = text;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				_cboType.SelectedItem = TypeCode.String;
				_txtValue.Text = "";
			}
		}

		private void OnControlClosing(object sender, CancelEventArgs eventArgs)
		{
			if (base.DialogResult == DialogResult.OK)
			{
				try
				{
					Value = ParseValue(_txtValue.Text);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					MessageBox.Show(this, "Can't convert '" + _txtValue.Text + "' to: " + _cboType.Text, "Error");
					eventArgs.Cancel = true;
				}
			}
		}

		private void OnCboTypeSelectedIndexChanged(object sender, EventArgs eventArgs)
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
	}
}
